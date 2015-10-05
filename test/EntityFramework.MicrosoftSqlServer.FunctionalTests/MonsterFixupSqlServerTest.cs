// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class MonsterFixupSqlServerTest : MonsterFixupTestBase
    {
        private static readonly HashSet<string> _createdDatabases = new HashSet<string>();

        private static readonly ConcurrentDictionary<string, object> _creationLocks
            = new ConcurrentDictionary<string, object>();

        protected override IServiceProvider CreateServiceProvider(bool throwingStateManager = false)
        {
            var serviceCollection = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection();

            if (throwingStateManager)
            {
                serviceCollection.AddScoped<IStateManager, ThrowingMonsterStateManager>();
            }

            return serviceCollection.BuildServiceProvider();
        }

        protected override DbContextOptions CreateOptions(string databaseName)
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(CreateConnectionString(databaseName));

            return optionsBuilder.Options;
        }

        private static string CreateConnectionString(string name)
        {
            var connStrBuilder = new SqlConnectionStringBuilder
                {
                    MultipleActiveResultSets = true,
                    InitialCatalog = name
                };
            return connStrBuilder.ApplyConfiguration().ConnectionString;
        }

        protected override void CreateAndSeedDatabase(string databaseName, Func<MonsterContext> createContext)
        {
            var creationLock = _creationLocks.GetOrAdd(databaseName, n => new object());
            lock (creationLock)
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

        public override void OnModelCreating<TMessage, TProductPhoto, TProductReview>(ModelBuilder builder)
        {
            base.OnModelCreating<TMessage, TProductPhoto, TProductReview>(builder);

            builder.Entity<TMessage>().Property(e => e.MessageId).UseSqlServerIdentityColumn();
            builder.Entity<TProductPhoto>().Property(e => e.PhotoId).UseSqlServerIdentityColumn();
            builder.Entity<TProductReview>().Property(e => e.ReviewId).UseSqlServerIdentityColumn();
        }
    }
}
