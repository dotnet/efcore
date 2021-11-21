// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

public abstract class FindSqliteTest : FindTestBase<FindSqliteTest.FindSqliteFixture>
{
    protected FindSqliteTest(FindSqliteFixture fixture)
        : base(fixture)
    {
    }

    public class FindSqliteTestSet : FindSqliteTest
    {
        public FindSqliteTestSet(FindSqliteFixture fixture)
            : base(fixture)
        {
        }

        protected override TEntity Find<TEntity>(DbContext context, params object[] keyValues)
            => context.Set<TEntity>().Find(keyValues);

        protected override ValueTask<TEntity> FindAsync<TEntity>(DbContext context, params object[] keyValues)
            => context.Set<TEntity>().FindAsync(keyValues);
    }

    public class FindSqliteTestContext : FindSqliteTest
    {
        public FindSqliteTestContext(FindSqliteFixture fixture)
            : base(fixture)
        {
        }

        protected override TEntity Find<TEntity>(DbContext context, params object[] keyValues)
            => context.Find<TEntity>(keyValues);

        protected override ValueTask<TEntity> FindAsync<TEntity>(DbContext context, params object[] keyValues)
            => context.FindAsync<TEntity>(keyValues);
    }

    public class FindSqliteTestNonGeneric : FindSqliteTest
    {
        public FindSqliteTestNonGeneric(FindSqliteFixture fixture)
            : base(fixture)
        {
        }

        protected override TEntity Find<TEntity>(DbContext context, params object[] keyValues)
            => (TEntity)context.Find(typeof(TEntity), keyValues);

        protected override async ValueTask<TEntity> FindAsync<TEntity>(DbContext context, params object[] keyValues)
            => (TEntity)await context.FindAsync(typeof(TEntity), keyValues);
    }

    public class FindSqliteFixture : FindFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
