using MIS.Domain.Entities;
using Xunit;

namespace MIS.Domain.Tests;

public sealed class CollectionDcrTests
{
    [Fact]
    public void Record_NormalizesStableValues_AndKeepsFeedbackSeparateFromComment()
    {
        var record = new CollectionDcr(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 8, 22),
            "call and visit", "paid partial", " Customer paid at branch. ", " Internal review. ", null, null,
            new DateOnly(2026, 8, 21), 1250.50m, null, null, null, null, DateTimeOffset.UtcNow);

        Assert.Equal("CALL_AND_VISIT", record.ActionCover);
        Assert.Equal("PAID_PARTIAL", record.Action);
        Assert.Equal("Customer paid at branch.", record.Feedback);
        Assert.Equal("Internal review.", record.Comment);
        Assert.Equal(1250.50m, record.PaidAmount);
    }

    [Fact]
    public void LinkPtp_SetsRelationalReference()
    {
        var record = new CollectionDcr(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 8, 22),
            "CALL", "PTP", "Confirmed.", null, new DateOnly(2026, 8, 28), 10000m, null, null, null, null, null, null, DateTimeOffset.UtcNow);
        var ptpId = Guid.NewGuid();
        record.LinkPtp(ptpId, DateTimeOffset.UtcNow);
        Assert.Equal(ptpId, record.LinkedPtpId);
    }
}
