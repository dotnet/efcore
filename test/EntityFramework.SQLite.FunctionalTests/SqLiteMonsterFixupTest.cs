// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.SQLite.FunctionalTests
{
    // TODO: SaveChanges fails with "Batch queries are not supported." for SQLite
    // TODO: Re-enable when #1053 is fixed
    internal class SqLiteMonsterFixupTest : MonsterFixupTestBase
    {
        private static readonly HashSet<string> _createdDatabases = new HashSet<string>();

        private static readonly ConcurrentDictionary<string, AsyncLock> _creationLocks
            = new ConcurrentDictionary<string, AsyncLock>();

        protected override IServiceProvider CreateServiceProvider(bool throwingStateManager = false)
        {
            var serviceCollection = new ServiceCollection()
                .AddEntityFramework()
                .AddSQLite()
                .ServiceCollection;

            if (throwingStateManager)
            {
                serviceCollection.AddScoped<StateManager, ThrowingMonsterStateManager>();
            }

            return serviceCollection.BuildServiceProvider();
        }

        protected override DbContextOptions CreateOptions(string databaseName)
        {
            return new DbContextOptions().UseSQLite("Filename=" + databaseName + ".db");
        }

        protected override async Task CreateAndSeedDatabase(string databaseName, Func<MonsterContext> createContext)
        {
            var creationLock = _creationLocks.GetOrAdd(databaseName, n => new AsyncLock());
            using (await creationLock.LockAsync())
            {
                if (!_createdDatabases.Contains(databaseName))
                {
                    using (var context = createContext())
                    {
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();
                        context.SeedUsingFKs();
                    }

                    _createdDatabases.Add(databaseName);
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            var keyProperties =
                from t in builder.Model.EntityTypes
                let ps = t.GetPrimaryKey().Properties
                where ps.Count == 1
                let p = ps[0]
                where p.PropertyType == typeof(int)
                select p;

            foreach (var property in keyProperties)
            {
                // Fix-up int properties to be INTEGER columns so rowid aliasing is enabled
                // TODO: Make SQLite specific type. Issue #875
                property.Relational().ColumnType = "INTEGER";
            }
        }
    }
}
