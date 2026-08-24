using MIS.Domain.Entities;
using Xunit;

namespace MIS.Domain.Tests;

public sealed class CollectionArchiveLifecycleTests
{
    [Fact]
    public void Case_archive_and_restore_preserve_archive_metadata_and_return_unassigned()
    {
        var now = DateTimeOffset.UtcNow;
        var item = new CollectionCase(Guid.NewGuid(), Guid.NewGuid(), "C-ARCHIVE-1", "A-1", 1000, 800, 500, 30, Guid.NewGuid(), now);
        var collector = Guid.NewGuid(); var manager = Guid.NewGuid();
        item.Assign(collector, Guid.NewGuid(), now.AddMinutes(1));

        item.Archive("DEBT_SETTLED", "Paid at branch", manager, now.AddMinutes(2));

        Assert.True(item.IsArchived); Assert.Equal("DEBT_SETTLED", item.ArchiveReason); Assert.Equal(collector, item.AssignedCollectorId);

        item.Restore("Bank requested reopening", manager, now.AddMinutes(3));

        Assert.False(item.IsArchived); Assert.Null(item.AssignedCollectorId); Assert.Null(item.AssignedTeamId); Assert.Equal("DEBT_SETTLED", item.ArchiveReason); Assert.Equal("Bank requested reopening", item.RestoreReason);
    }

    [Fact]
    public void Portfolio_archive_does_not_change_file_retention_metadata()
    {
        var now = DateTimeOffset.UtcNow; var manager = Guid.NewGuid();
        var item = new BankPortfolioImport(Guid.NewGuid(), "August 2026", "portfolio.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 1024, new string('a', 64), "bank/import.xlsx", 10, manager, now);

        item.Archive("PORTFOLIO_ENDED", null, manager, now.AddMinutes(1));
        item.Restore("Bank reopened portfolio", manager, now.AddMinutes(2));

        Assert.False(item.IsArchived); Assert.Equal("bank/import.xlsx", item.StorageKey); Assert.Equal("portfolio.xlsx", item.OriginalFileName); Assert.Equal("PORTFOLIO_ENDED", item.ArchiveReason);
    }
}
