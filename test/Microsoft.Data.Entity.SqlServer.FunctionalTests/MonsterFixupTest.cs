// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class MonsterFixupTest : MonsterFixupTestBase
    {
        private static readonly AsyncLock _creationLock = new AsyncLock();
        private static bool _databaseCreated;

        protected override IServiceProvider CreateServiceProvider()
        {
            return new ServiceCollection().AddEntityFramework().AddSqlServer().ServiceCollection.BuildServiceProvider();
        }

        protected override DbContextOptions CreateOptions(string databaseName)
        {
            return new DbContextOptions().UseSqlServer(CreateConnectionString(databaseName));
        }

        private static string CreateConnectionString(string name)
        {
            return new SqlConnectionStringBuilder
                {
                    DataSource = @"(localdb)\v11.0",
                    MultipleActiveResultSets = true,
                    InitialCatalog = name,
                    IntegratedSecurity = true,
                    ConnectTimeout = 30
                }.ConnectionString;
        }

        protected override async Task CreateAndSeedDatabase(IServiceProvider serviceProvider, string databaseName)
        {
            using (await _creationLock.LockAsync())
            {
                if (!_databaseCreated)
                {
                    using (var context = new MonsterContext(serviceProvider, CreateOptions(databaseName)))
                    {
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();
                        context.SeedUsingFKs();
                    }

                    _databaseCreated = true;
                }
            }
        }
    }
}
