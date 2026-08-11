namespace MIS.Application.Interfaces;

public interface IHrTransactionRunner
{
    Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken);

    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken);
}
