using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace VehicleSalesFIAP.Tests.Integration;

internal sealed class TestRowVersionInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        SetRowVersionValues(eventData.Context);

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        SetRowVersionValues(eventData.Context);

        return ValueTask.FromResult(result);
    }

    private static void SetRowVersionValues(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        var entries = dbContext.ChangeTracker
            .Entries()
            .Where(entry =>
                entry.Metadata.FindProperty("RowVersion") is not null &&
                entry.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Property("RowVersion").CurrentValue = Guid.NewGuid().ToByteArray();
        }
    }
}
