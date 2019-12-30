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
        private readonly IServiceProvider _internalServiceProvider;
        private static readonly InMemoryDatabaseRoot _dbRoot = new InMemoryDatabaseRoot();
        private readonly bool _useLazyLoadingProxies;
        private readonly bool _useChangeDetectionProxies;
        private readonly string _dbName;

        protected TestContext(string dbName = null, bool useLazyLoading = false, bool useChangeDetection = false)
        {
            _internalServiceProvider
                = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .AddEntityFrameworkProxies()
                    .BuildServiceProvider();

            _dbName = dbName;
            _useLazyLoadingProxies = useLazyLoading;
            _useChangeDetectionProxies = useChangeDetection;
        }

        protected TestContext(IServiceProvider internalServiceProvider, string dbName = null, bool useLazyLoading = false, bool useChangeDetection = false)
            : this(dbName, useLazyLoading, useChangeDetection)
        {
            _internalServiceProvider = internalServiceProvider;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_useLazyLoadingProxies)
            {
                optionsBuilder.UseLazyLoadingProxies();
            }

            if (_useChangeDetectionProxies)
            {
                optionsBuilder.UseChangeDetectionProxies();
            }

            if (_internalServiceProvider != null)
            {
                optionsBuilder.UseInternalServiceProvider(_internalServiceProvider);
            }

            optionsBuilder.UseInMemoryDatabase(_dbName ?? "TestContext", _dbRoot);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TEntity>();
        }
    }
}
