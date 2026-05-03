using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FluentJoinForge.Queryable.Tests;

public abstract class DatabaseTests<TDbContext>(DatabaseTestsFixture<TDbContext> fixture) : DatabaseTests<TDbContext, DatabaseTestsFixture<TDbContext>>(fixture)
    where TDbContext : DbContext;

public abstract class DatabaseTests<TDbContext, TFixture>(TFixture fixture) : IClassFixture<TFixture>, IAsyncLifetime
    where TDbContext : DbContext
    where TFixture : DatabaseTestsFixture<TDbContext>
{
    public TDbContext Context { get; private set; } = null!;

    private IDbContextTransaction _transaction = null!;

    public async ValueTask InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<TDbContext>()
            .UseSqlite(fixture.Connection)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .LogTo(TestContext.Current.TestOutputHelper!.WriteLine)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;

        Context = (TDbContext)Activator.CreateInstance(typeof(TDbContext), options)!;

        _transaction = await Context.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);
    }

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
            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                await Context.DisposeAsync();
            }
        }

        _disposed = true;
    }

    #endregion
}
