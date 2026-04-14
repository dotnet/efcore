// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

internal abstract class TestContext<TEntity>(
    string dbName = null,
    bool useLazyLoading = false,
    bool useChangeDetection = false,
    bool checkEquality = true,
    ChangeTrackingStrategy? changeTrackingStrategy = null,
    bool ignoreNonVirtualNavigations = false)
    : DbContext
    where TEntity : class
{
    private static readonly InMemoryDatabaseRoot _dbRoot = new();

    private readonly IServiceProvider _internalServiceProvider = new ServiceCollection()
        .AddEntityFrameworkInMemoryDatabase()
        .AddEntityFrameworkProxies()
        .BuildServiceProvider(validateScopes: true);

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (useLazyLoading)
        {
            optionsBuilder.UseLazyLoadingProxies(
                b =>
                {
                    if (ignoreNonVirtualNavigations)
                    {
                        b.IgnoreNonVirtualNavigations();
                    }
                });
        }

        if (useChangeDetection)
        {
            optionsBuilder.UseChangeTrackingProxies(checkEquality: checkEquality);
        }

        if (_internalServiceProvider != null)
        {
            optionsBuilder.UseInternalServiceProvider(_internalServiceProvider);
        }

        optionsBuilder.UseInMemoryDatabase(dbName ?? "TestContext", _dbRoot);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (changeTrackingStrategy.HasValue)
        {
            modelBuilder.HasChangeTrackingStrategy(changeTrackingStrategy.Value);
        }

        modelBuilder.Entity<TEntity>();
    }
}
