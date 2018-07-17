// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class FindOracleTest : FindTestBase<FindOracleTest.FindOracleFixture>
    {
        protected FindOracleTest(FindOracleFixture fixture)
            : base(fixture)
        {
        }

        public class FindOracleTestSet : FindOracleTest
        {
            public FindOracleTestSet(FindOracleFixture fixture)
                : base(fixture)
            {
            }

            protected override TEntity Find<TEntity>(DbContext context, params object[] keyValues)
                => context.Set<TEntity>().Find(keyValues);

            protected override Task<TEntity> FindAsync<TEntity>(DbContext context, params object[] keyValues)
                => context.Set<TEntity>().FindAsync(keyValues);
        }

        public class FindOracleTestContext : FindOracleTest
        {
            public FindOracleTestContext(FindOracleFixture fixture)
                : base(fixture)
            {
            }

            protected override TEntity Find<TEntity>(DbContext context, params object[] keyValues)
                => context.Find<TEntity>(keyValues);

            protected override Task<TEntity> FindAsync<TEntity>(DbContext context, params object[] keyValues)
                => context.FindAsync<TEntity>(keyValues);
        }

        public class FindOracleTestNonGeneric : FindOracleTest
        {
            public FindOracleTestNonGeneric(FindOracleFixture fixture)
                : base(fixture)
            {
            }

            protected override TEntity Find<TEntity>(DbContext context, params object[] keyValues)
                => (TEntity)context.Find(typeof(TEntity), keyValues);

            protected override async Task<TEntity> FindAsync<TEntity>(DbContext context, params object[] keyValues)
                => (TEntity)await context.FindAsync(typeof(TEntity), keyValues);
        }

        public class FindOracleFixture : FindFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
            protected override ITestStoreFactory TestStoreFactory => OracleTestStoreFactory.Instance;
        }
    }
}
