namespace MIS.Domain.Constants;

public static class CollectionsValues
{
    public static class OrganizationTypes
    {
        public const string Bank = "BANK";
        public const string ConsumerFinance = "CONSUMER_FINANCE";
        public const string FinancialInstitution = "FINANCIAL_INSTITUTION";
        public const string Other = "OTHER";
    }

    public static class CaseStatuses
    {
        public const string Active = "ACTIVE";
        public const string OnHold = "ON_HOLD";
        public const string Settled = "SETTLED";
        public const string Closed = "CLOSED";
        public const string Legal = "LEGAL";
        public const string WriteOff = "WRITE_OFF";
    }

    public static class PromiseStatuses
    {
        public const string Active = "ACTIVE";
        public const string DueToday = "DUE_TODAY";
        public const string Upcoming = "UPCOMING";
        public const string UnderReview = "UNDER_REVIEW";
        public const string Fulfilled = "FULFILLED";
        public const string PartiallyFulfilled = "PARTIALLY_FULFILLED";
        public const string Broken = "BROKEN";
        public const string Cancelled = "CANCELLED";
        public const string Rescheduled = "RESCHEDULED";
    }

    public static class PaymentStatuses
    {
        public const string Submitted = "SUBMITTED";
        public const string UnderReview = "UNDER_REVIEW";
        public const string Approved = "APPROVED";
        public const string Rejected = "REJECTED";
        public const string Reversed = "REVERSED";
    }

    public static class AssignmentSources
    {
        public const string Manual = "MANUAL";
        public const string Automatic = "AUTOMATIC";
        public const string Import = "IMPORT";
    }

    public static class ActivityTypes
    {
        public const string Call = "CALL";
        public const string Sms = "SMS";
        public const string Email = "EMAIL";
        public const string Note = "NOTE";
        public const string Assignment = "ASSIGNMENT";
        public const string PtpCreated = "PTP_CREATED";
        public const string Payment = "PAYMENT";
        public const string Visit = "VISIT";
        public const string Complaint = "COMPLAINT";
        public const string StatusChange = "STATUS_CHANGE";
    }

    public static class VisitStatuses
    {
        public const string Planned = "PLANNED";
        public const string Assigned = "ASSIGNED";
        public const string InProgress = "IN_PROGRESS";
        public const string Completed = "COMPLETED";
        public const string Failed = "FAILED";
        public const string Rescheduled = "RESCHEDULED";
        public const string Cancelled = "CANCELLED";
    }

    public static class ComplaintStatuses
    {
        public const string New = "NEW";
        public const string Assigned = "ASSIGNED";
        public const string InProgress = "IN_PROGRESS";
        public const string AwaitingInformation = "AWAITING_INFORMATION";
        public const string Resolved = "RESOLVED";
        public const string Reopened = "REOPENED";
        public const string Closed = "CLOSED";
        public const string Escalated = "ESCALATED";
    }
}
