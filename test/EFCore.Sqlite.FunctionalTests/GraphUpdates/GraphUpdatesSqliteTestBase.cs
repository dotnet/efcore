// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class GraphUpdatesSqliteTestBase<TFixture> : GraphUpdatesTestBase<TFixture>
        where TFixture : GraphUpdatesSqliteTestBase<TFixture>.GraphUpdatesSqliteFixtureBase, new()
    {
        protected GraphUpdatesSqliteTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory(Skip = "Default owned collection pattern does not work with SQLite due to composite key.")]
        public override Task Update_principal_with_shadow_key_owned_collection_throws(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "Default owned collection pattern does not work with SQLite due to composite key.")]
        public override Task Delete_principal_with_shadow_key_owned_collection_throws(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "Default owned collection pattern does not work with SQLite due to composite key.")]
        public override Task Clearing_shadow_key_owned_collection_throws(bool async, bool useUpdate, bool addNew)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "Default owned collection pattern does not work with SQLite due to composite key.")]
        public override Task Update_principal_with_CLR_key_owned_collection(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "Default owned collection pattern does not work with SQLite due to composite key.")]
        public override Task Delete_principal_with_CLR_key_owned_collection(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "Default owned collection pattern does not work with SQLite due to composite key.")]
        public override Task Clearing_CLR_key_owned_collection(bool async, bool useUpdate, bool addNew)
            => Task.CompletedTask;

        protected override IQueryable<Root> ModifyQueryRoot(IQueryable<Root> query)
            => query.AsSplitQuery();

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public abstract class GraphUpdatesSqliteFixtureBase : GraphUpdatesFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory
                => (TestSqlLoggerFactory)ListLoggerFactory;

            protected override ITestStoreFactory TestStoreFactory
                => SqliteTestStoreFactory.Instance;

            protected virtual bool AutoDetectChanges
                => false;

            public override PoolableDbContext CreateContext()
            {
                var context = base.CreateContext();
                context.ChangeTracker.AutoDetectChangesEnabled = AutoDetectChanges;

                return context;
            }
        }
    }
}
