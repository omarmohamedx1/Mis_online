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
}
