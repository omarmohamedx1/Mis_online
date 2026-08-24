using MIS.Domain.Constants;
using MIS.Domain.Entities;
using MIS.Domain.Services;
using Xunit;

namespace MIS.Domain.Tests;

public sealed class CollectionRulesTests
{
    [Theory]
    [InlineData("2026-08-13", "2026-08-13", 0, 0, "DUE_TODAY")]
    [InlineData("2026-08-14", "2026-08-13", 0, 0, "UPCOMING")]
    [InlineData("2026-08-10", "2026-08-13", 0, 0, "BROKEN")]
    [InlineData("2026-08-10", "2026-08-13", 25, 0, "PARTIALLY_FULFILLED")]
    [InlineData("2026-08-10", "2026-08-13", 99, 1, "FULFILLED")]
    public void PromiseEvaluation_IsDeterministic(string due, string today, decimal paid, decimal tolerance, string expected)
    {
        var result = CollectionRules.EvaluatePromise(100m, paid, DateOnly.Parse(due), DateOnly.Parse(today), 0, tolerance);
        Assert.Equal(expected, result.Status);
    }

    [Fact]
    public void PriorityScore_IsExplainableAndClamped()
    {
        var result = CollectionRules.CalculatePriority(500_000m, 220, true, true, 45);
        Assert.Equal(100, result.Score);
        Assert.Contains("BROKEN_PTP", result.Reasons);
        Assert.Contains("NO_RECENT_CONTACT", result.Reasons);
        Assert.Contains("HIGH_OUTSTANDING", result.Reasons);
    }

    [Fact]
    public void SensitiveValues_AreMaskedWithOnlyEdgesVisible()
    {
        Assert.Equal("298********123", CollectionRules.MaskNationalId("29812345678123"));
        Assert.Equal("01*******45", CollectionRules.MaskPhone("01012345645"));
    }

    [Theory]
    [InlineData("ANSWERED", true)]
    [InlineData("CALLBACK_REQUESTED", true)]
    [InlineData("REFUSED_TO_PAY", true)]
    [InlineData("NO_ANSWER", false)]
    [InlineData("BUSY", false)]
    [InlineData("SWITCHED_OFF", false)]
    [InlineData("WRONG_NUMBER", false)]
    public void DcrSuccessfulContactUsesMaintainedCallOutcomeDefinition(string outcome, bool expected)
    {
        Assert.Equal(expected, CollectionRules.IsSuccessfulDcrContact(CollectionsValues.ActivityTypes.Call, outcome));
        Assert.False(CollectionRules.IsSuccessfulDcrContact(CollectionsValues.ActivityTypes.Sms, outcome));
    }

    [Fact]
    public void MakerCannotApproveOwnPayment()
    {
        var maker = Guid.NewGuid();
        var payment = new CollectionPayment(Guid.NewGuid(), 500m, new DateOnly(2026, 8, 13), "CASH", "REF-1", maker, null, DateTimeOffset.UtcNow);
        Assert.Throws<InvalidOperationException>(() => payment.Review(maker, true, null, true, DateTimeOffset.UtcNow));
        Assert.Equal(CollectionsValues.PaymentStatuses.Submitted, payment.Status);
    }

    [Fact]
    public void IndependentReviewerCanApprovePayment()
    {
        var payment = new CollectionPayment(Guid.NewGuid(), 500m, new DateOnly(2026, 8, 13), "CASH", "REF-2", Guid.NewGuid(), null, DateTimeOffset.UtcNow);
        payment.Review(Guid.NewGuid(), true, null, true, DateTimeOffset.UtcNow);
        Assert.Equal(CollectionsValues.PaymentStatuses.Approved, payment.Status);
        Assert.NotNull(payment.VerifiedAt);
    }

    [Fact]
    public void BucketDefinitionRejectsInvertedRange()
    {
        Assert.Throws<ArgumentException>(() => new DelinquencyBucketDefinition(Guid.NewGuid(), null, "BAD", "غير صحيح", "Invalid", 90, 30, 1, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void CollectionCaseMaintainsAuthoritativeTotalDue()
    {
        var item = new CollectionCase(Guid.NewGuid(), Guid.NewGuid(), "CASE-1", "ACC-1", 5_000m, 4_000m, 1_200m, 30, Guid.NewGuid(), DateTimeOffset.UtcNow);
        Assert.Equal(1_200m, item.TotalDue);
        item.RecordApprovedPayment(200m, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        Assert.Equal(1_000m, item.TotalDue);
        Assert.Equal(3_800m, item.OutstandingBalance);
    }

    [Fact]
    public void CollectionCaseCanReturnToUnassignedQueue()
    {
        var item = new CollectionCase(Guid.NewGuid(), Guid.NewGuid(), "CASE-2", "ACC-2", 5_000m, 4_000m, 1_200m, 30, Guid.NewGuid(), DateTimeOffset.UtcNow);
        item.Assign(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
        item.Unassign(DateTimeOffset.UtcNow);
        Assert.Null(item.AssignedCollectorId);
        Assert.Null(item.AssignedTeamId);
    }

    [Fact]
    public void NewFollowUpReplacesTheCurrentCaseReminder()
    {
        var now = DateTimeOffset.UtcNow;
        var item = new CollectionCase(Guid.NewGuid(), Guid.NewGuid(), "CASE-3", "ACC-3", 5_000m, 4_000m, 1_200m, 30, Guid.NewGuid(), now);
        item.ScheduleNextFollowUp(now.AddDays(1), now);
        item.ScheduleNextFollowUp(now.AddDays(2), now.AddMinutes(1));
        Assert.Equal(now.AddDays(2), item.NextFollowUpAt);
    }

    [Fact]
    public void PendingPromiseCanBeMarkedKeptWithServerTimestamp()
    {
        var now = DateTimeOffset.UtcNow;
        var promise = new PromiseToPay(Guid.NewGuid(), 500m, DateOnly.FromDateTime(now.Date), Guid.NewGuid(), "BANK_WORKSPACE", null, now);

        promise.Transition(CollectionsValues.PromiseStatuses.Fulfilled, now.AddMinutes(1));

        Assert.Equal(CollectionsValues.PromiseStatuses.Fulfilled, promise.Status);
        Assert.Equal(now.AddMinutes(1), promise.FulfilledAt);
        Assert.Equal(now.AddMinutes(1), promise.EvaluatedAt);
    }

    [Fact]
    public void ResolvedPromiseCannotBeTransitionedAgain()
    {
        var now = DateTimeOffset.UtcNow;
        var promise = new PromiseToPay(Guid.NewGuid(), 500m, DateOnly.FromDateTime(now.Date), Guid.NewGuid(), "BANK_WORKSPACE", null, now);
        promise.Transition(CollectionsValues.PromiseStatuses.Broken, now);

        Assert.Throws<InvalidOperationException>(() => promise.Transition(CollectionsValues.PromiseStatuses.Cancelled, now.AddMinutes(1)));
    }

    [Fact]
    public void VisitLifecyclePreservesScheduleAndCompletionTimestamps()
    {
        var now = DateTimeOffset.UtcNow;
        var visit = new FieldVisit(Guid.NewGuid(), Guid.NewGuid(), now.AddDays(1), "Customer address", null, null, Guid.NewGuid(), now, "Collection follow-up");
        Assert.Equal(CollectionsValues.VisitStatuses.Scheduled, visit.Status);
        visit.Reschedule(now.AddDays(2), now.AddMinutes(1));
        visit.Start(now.AddDays(2));
        visit.Complete(CollectionsValues.VisitResults.CustomerMet, "Customer requested another call.", now.AddDays(2).AddMinutes(10));
        Assert.Equal(CollectionsValues.VisitStatuses.Completed, visit.Status);
        Assert.Equal(CollectionsValues.VisitResults.CustomerMet, visit.Result);
        Assert.NotNull(visit.CheckedOutAt);
        Assert.Throws<InvalidOperationException>(() => visit.Cancel(null, now.AddDays(3)));
    }

    [Fact]
    public void MissedVisitIsPersistedAsFinalHistory()
    {
        var now = DateTimeOffset.UtcNow;
        var visit = new FieldVisit(Guid.NewGuid(), Guid.NewGuid(), now, "Customer address", null, null, Guid.NewGuid(), now);
        visit.MarkMissed("No one answered.", now.AddHours(1));
        Assert.Equal(CollectionsValues.VisitStatuses.Missed, visit.Status);
        Assert.Throws<InvalidOperationException>(() => visit.Complete(CollectionsValues.VisitResults.CustomerMet, null, now.AddHours(2)));
    }

    [Fact]
    public void ClientConfigurationRejectsInvalidJson()
    {
        var client = new ClientOrganization("CLIENT", "عميل", "Client", CollectionsValues.OrganizationTypes.Other, DateTimeOffset.UtcNow);
        Assert.Throws<ArgumentException>(() => client.Update("عميل", "Client", CollectionsValues.OrganizationTypes.Other, null, null, "{invalid", true, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void AttachmentNormalizesOriginalFileName()
    {
        var attachment = new CollectionAttachment(Guid.NewGuid(), null, "CASE_DOCUMENT", "../unsafe.pdf", "application/pdf", 100, new string('a', 64), "collections/key.pdf", Guid.NewGuid(), DateTimeOffset.UtcNow);
        Assert.Equal("unsafe.pdf", attachment.OriginalFileName);
    }

    [Fact]
    public void ComplaintWorkflowRequiresControlledTransitionsAndServerActors()
    {
        var now = DateTimeOffset.UtcNow;
        var resolver = Guid.NewGuid();
        var complaint = new CollectionComplaint(Guid.NewGuid(), "CMP-2026-ABC12345", "BANK_WORKSPACE", "PAYMENT_ISSUE", CollectionsValues.ComplaintPriorities.High, "Payment was not reflected.", now, now.AddDays(2), null, Guid.NewGuid(), now);

        Assert.Equal(CollectionsValues.ComplaintStatuses.Open, complaint.Status);
        Assert.Throws<InvalidOperationException>(() => complaint.Resolve("Resolved", resolver, now.AddMinutes(1)));
        complaint.Start(now.AddMinutes(1));
        complaint.Resolve("Payment reconciled.", resolver, now.AddMinutes(2));
        complaint.Close(now.AddMinutes(3));

        Assert.Equal(CollectionsValues.ComplaintStatuses.Closed, complaint.Status);
        Assert.Equal(resolver, complaint.ResolvedById);
        Assert.Equal(now.AddMinutes(2), complaint.ResolvedAt);
        Assert.Equal(now.AddMinutes(3), complaint.ClosedAt);
    }

    [Fact]
    public void ComplaintReopenRequiresReasonAndPreservesResolution()
    {
        var now = DateTimeOffset.UtcNow;
        var complaint = new CollectionComplaint(Guid.NewGuid(), "CMP-2026-DEF12345", "BANK_WORKSPACE", "OTHER", CollectionsValues.ComplaintPriorities.Medium, "Customer complaint.", now, null, null, Guid.NewGuid(), now);
        complaint.Start(now.AddMinutes(1));
        complaint.Resolve("Original resolution", Guid.NewGuid(), now.AddMinutes(2));
        Assert.Throws<ArgumentException>(() => complaint.Reopen("", now.AddMinutes(3)));
        complaint.Reopen("Customer supplied new evidence.", now.AddMinutes(3));
        Assert.Equal(CollectionsValues.ComplaintStatuses.InProgress, complaint.Status);
        Assert.Equal("Original resolution", complaint.Resolution);
    }

    [Fact]
    public void RejectedComplaintCannotBeAssigned()
    {
        var now = DateTimeOffset.UtcNow;
        var complaint = new CollectionComplaint(Guid.NewGuid(), "CMP-2026-GHI12345", "BANK_WORKSPACE", "OTHER", CollectionsValues.ComplaintPriorities.Low, "Invalid complaint.", now, null, null, Guid.NewGuid(), now);
        complaint.Reject("Not applicable.", now.AddMinutes(1));
        Assert.Equal(CollectionsValues.ComplaintStatuses.Rejected, complaint.Status);
        Assert.Throws<InvalidOperationException>(() => complaint.Assign(Guid.NewGuid(), now.AddMinutes(2)));
    }
}
