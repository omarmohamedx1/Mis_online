using Microsoft.EntityFrameworkCore;
using MIS.Application.Interfaces;

namespace MIS.Infrastructure.Persistence;

public sealed class EfHrTransactionRunner : IHrTransactionRunner
{
    private readonly ApplicationDbContext _dbContext;

    public EfHrTransactionRunner(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        await ExecuteAsync(async token =>
        {
            await operation(token);
            return true;
        }, cancellationToken);
    }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        if (_dbContext.Database.CurrentTransaction is not null)
            return await operation(cancellationToken);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var result = await operation(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }
}
