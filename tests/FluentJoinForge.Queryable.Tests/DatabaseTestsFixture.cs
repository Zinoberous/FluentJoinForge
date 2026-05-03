using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FluentJoinForge.Queryable.Tests;

public class DatabaseTestsFixture<TDbContext> : IAsyncLifetime
    where TDbContext : DbContext
{
    public SqliteConnection Connection { get; } = new("Data Source=:memory:");

    public async ValueTask InitializeAsync()
    {
        await Connection.OpenAsync();

        var options = new DbContextOptionsBuilder<TDbContext>()
            .UseSqlite(Connection)
            .Options;

        await using var context = (TDbContext)Activator.CreateInstance(typeof(TDbContext), options)!;

        await context.Database.EnsureCreatedAsync();

        await InitializeAsync(context);
    }

    protected virtual Task InitializeAsync(TDbContext context) => Task.CompletedTask;

    #region dispose

    private bool _disposed;

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }

    protected virtual async Task DisposeAsync(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            await Connection.DisposeAsync();
        }

        _disposed = true;
    }

    #endregion
}
