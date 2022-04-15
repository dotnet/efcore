// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public abstract class FindInMemoryTest : FindTestBase<FindInMemoryTest.FindInMemoryFixture>
{
    protected FindInMemoryTest(FindInMemoryFixture fixture)
        : base(fixture)
    {
    }

    public class FindInMemoryTestSet : FindInMemoryTest
    {
        public FindInMemoryTestSet(FindInMemoryFixture fixture)
            : base(fixture)
        {
        }

        protected override TEntity Find<TEntity>(DbContext context, params object[] keyValues)
            => context.Set<TEntity>().Find(keyValues);

        protected override ValueTask<TEntity> FindAsync<TEntity>(DbContext context, params object[] keyValues)
            => context.Set<TEntity>().FindAsync(keyValues);
    }

    public class FindInMemoryTestContext : FindInMemoryTest
    {
        public FindInMemoryTestContext(FindInMemoryFixture fixture)
            : base(fixture)
        {
        }

        protected override TEntity Find<TEntity>(DbContext context, params object[] keyValues)
            => context.Find<TEntity>(keyValues);

        protected override ValueTask<TEntity> FindAsync<TEntity>(DbContext context, params object[] keyValues)
            => context.FindAsync<TEntity>(keyValues);
    }

    public class FindInMemoryTestNonGeneric : FindInMemoryTest
    {
        public FindInMemoryTestNonGeneric(FindInMemoryFixture fixture)
            : base(fixture)
        {
        }

        protected override TEntity Find<TEntity>(DbContext context, params object[] keyValues)
            => (TEntity)context.Find(typeof(TEntity), keyValues);

        protected override async ValueTask<TEntity> FindAsync<TEntity>(DbContext context, params object[] keyValues)
            => (TEntity)await context.FindAsync(typeof(TEntity), keyValues);
    }

    public class FindInMemoryFixture : FindFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
