// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

internal abstract class TestContext<TEntity> : DbContext
    where TEntity : class
{
    private static readonly InMemoryDatabaseRoot _dbRoot = new();

    private readonly IServiceProvider _internalServiceProvider;
    private readonly string _dbName;
    private readonly bool _useLazyLoadingProxies;
    private readonly bool _ignoreNonVirtualNavigations;
    private readonly bool _useChangeDetectionProxies;
    private readonly bool _checkEquality;
    private readonly ChangeTrackingStrategy? _changeTrackingStrategy;

    protected TestContext(
        string dbName = null,
        bool useLazyLoading = false,
        bool useChangeDetection = false,
        bool checkEquality = true,
        ChangeTrackingStrategy? changeTrackingStrategy = null,
        bool ignoreNonVirtualNavigations = false)
    {
        _internalServiceProvider
            = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddEntityFrameworkProxies()
                .BuildServiceProvider(validateScopes: true);

        _dbName = dbName;
        _useLazyLoadingProxies = useLazyLoading;
        _useChangeDetectionProxies = useChangeDetection;
        _checkEquality = checkEquality;
        _changeTrackingStrategy = changeTrackingStrategy;
        _ignoreNonVirtualNavigations = ignoreNonVirtualNavigations;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_useLazyLoadingProxies)
        {
            optionsBuilder.UseLazyLoadingProxies(
                b =>
                {
                    if (_ignoreNonVirtualNavigations)
                    {
                        b.IgnoreNonVirtualNavigations();
                    }
                });
        }

        if (_useChangeDetectionProxies)
        {
            optionsBuilder.UseChangeTrackingProxies(checkEquality: _checkEquality);
        }

        if (_internalServiceProvider != null)
        {
            optionsBuilder.UseInternalServiceProvider(_internalServiceProvider);
        }

        optionsBuilder.UseInMemoryDatabase(_dbName ?? "TestContext", _dbRoot);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (_changeTrackingStrategy.HasValue)
        {
            modelBuilder.HasChangeTrackingStrategy(_changeTrackingStrategy.Value);
        }

        modelBuilder.Entity<TEntity>();
    }
}
