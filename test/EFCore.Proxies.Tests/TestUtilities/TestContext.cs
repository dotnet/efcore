// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    internal abstract class TestContext<TEntity> : DbContext
        where TEntity : class
    {
        private static readonly InMemoryDatabaseRoot _dbRoot = new InMemoryDatabaseRoot();

        private readonly IServiceProvider _internalServiceProvider;
        private readonly string _dbName;
        private readonly bool _useLazyLoadingProxies;
        private readonly bool _useChangeDetectionProxies;
        private readonly bool _checkEquality;
        private readonly ChangeTrackingStrategy? _changeTrackingStrategy;

        protected TestContext(
            string dbName = null,
            bool useLazyLoading = false,
            bool useChangeDetection = false,
            bool checkEquality = true,
            ChangeTrackingStrategy? changeTrackingStrategy = null)
        {
            _internalServiceProvider
                = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .AddEntityFrameworkProxies()
                    .BuildServiceProvider();

            _dbName = dbName;
            _useLazyLoadingProxies = useLazyLoading;
            _useChangeDetectionProxies = useChangeDetection;
            _checkEquality = checkEquality;
            _changeTrackingStrategy = changeTrackingStrategy;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_useLazyLoadingProxies)
            {
                optionsBuilder.UseLazyLoadingProxies();
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
}
