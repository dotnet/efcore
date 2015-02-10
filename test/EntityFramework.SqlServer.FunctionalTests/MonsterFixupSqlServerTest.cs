// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class MonsterFixupSqlServerTest : MonsterFixupTestBase
    {
        private static readonly HashSet<string> _createdDatabases = new HashSet<string>();

        private static readonly ConcurrentDictionary<string, AsyncLock> _creationLocks
            = new ConcurrentDictionary<string, AsyncLock>();

        protected override IServiceProvider CreateServiceProvider(bool throwingStateManager = false)
        {
            var serviceCollection = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection;

            if (throwingStateManager)
            {
                serviceCollection.AddScoped<StateManager, ThrowingMonsterStateManager>();
            }

            return serviceCollection.BuildServiceProvider();
        }

        protected override DbContextOptions CreateOptions(string databaseName)
        {
            var options = new DbContextOptions();
            options.UseSqlServer(CreateConnectionString(databaseName));

            return options;
        }

        private static string CreateConnectionString(string name)
        {
            return new SqlConnectionStringBuilder
                {
                    DataSource = @"(localdb)\MSSQLLocalDB",
                    MultipleActiveResultSets = true,
                    InitialCatalog = name,
                    IntegratedSecurity = true,
                    ConnectTimeout = 30
                }.ConnectionString;
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
    }
}
