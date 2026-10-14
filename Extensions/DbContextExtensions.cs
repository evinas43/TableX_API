using System.Data;
using Microsoft.EntityFrameworkCore;

namespace TableX_API.Extensions;

public static class DbContextExtensions
{
    public static Task<T> ExecuteInTransactionAsync<T>(this DbContext db, Func<CancellationToken, Task<T>> action,
        IsolationLevel isolationLevel, CancellationToken ct = default)
    {
        var strategy = db.Database.CreateExecutionStrategy();

        return strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(isolationLevel, ct);
            var result = await action(ct);
            await transaction.CommitAsync(ct);
            return result;
        });
    }
}
