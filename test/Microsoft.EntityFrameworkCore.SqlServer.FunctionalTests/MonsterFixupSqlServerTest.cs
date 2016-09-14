// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class MonsterFixupSqlServerTest : MonsterFixupTestBase, IDisposable
    {
        protected override IServiceProvider CreateServiceProvider(bool throwingStateManager = false)
        {
            var serviceCollection = new ServiceCollection()
                .AddEntityFrameworkSqlServer();

            if (throwingStateManager)
            {
                serviceCollection.AddScoped<IStateManager, ThrowingMonsterStateManager>();
            }

            return serviceCollection.BuildServiceProvider();
        }

        protected override DbContextOptions CreateOptions(string databaseName)
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(CreateConnectionString(databaseName), b => b.ApplyConfiguration());

            return optionsBuilder.Options;
        }

        private static string CreateConnectionString(string name)
            => new SqlConnectionStringBuilder(TestEnvironment.DefaultConnection)
            {
                MultipleActiveResultSets = true,
                InitialCatalog = name
            }.ConnectionString;

        private SqlServerTestStore _testStore;

        protected override void CreateAndSeedDatabase(string databaseName, Func<MonsterContext> createContext, Action<MonsterContext> seed)
        {
            _testStore = SqlServerTestStore.GetOrCreateShared(databaseName, () =>
                {
                    using (var context = createContext())
                    {
                        context.Database.EnsureCreated();
                        seed(context);

                        TestSqlLoggerFactory.Reset();
                    }
                });
        }

        public virtual void Dispose() => _testStore?.Dispose();

        public override void OnModelCreating<TMessage, TProductPhoto, TProductReview>(ModelBuilder builder)
        {
            base.OnModelCreating<TMessage, TProductPhoto, TProductReview>(builder);

            builder.Entity<TMessage>().Property(e => e.MessageId).UseSqlServerIdentityColumn();
            builder.Entity<TProductPhoto>().Property(e => e.PhotoId).UseSqlServerIdentityColumn();
            builder.Entity<TProductReview>().Property(e => e.ReviewId).UseSqlServerIdentityColumn();
        }
    }
}
