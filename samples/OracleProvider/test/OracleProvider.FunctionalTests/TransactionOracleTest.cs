// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class TransactionOracleTest : TransactionTestBase<TransactionOracleTest.TransactionOracleFixture>, IDisposable
    {
        public TransactionOracleTest(TransactionOracleFixture fixture)
            : base(fixture)
        {
            TestOracleRetryingExecutionStrategy.Suspended = true;
        }

        protected override bool SnapshotSupported => false;
        protected override bool DirtyReadsOccur => false;

        public virtual void Dispose()
        {
            TestOracleRetryingExecutionStrategy.Suspended = false;
        }

        protected override DbContext CreateContextWithConnectionString()
        {
            var options = Fixture.AddOptions(
                    new DbContextOptionsBuilder()
                        .UseOracle(TestStore.ConnectionString, b => b.ApplyConfiguration().CommandTimeout(OracleTestStore.CommandTimeout)))
                .UseInternalServiceProvider(Fixture.ServiceProvider);

            return new DbContext(options.Options);
        }

        public class TransactionOracleFixture : TransactionFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => OracleTestStoreFactory.Instance;

            public override void Reseed()
            {
                using (var context = CreateContext())
                {
                    context.Set<TransactionCustomer>().RemoveRange(context.Set<TransactionCustomer>());
                    context.SaveChanges();

                    Seed(context);
                }
            }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                new OracleDbContextOptionsBuilder(
                        base.AddOptions(builder)
                            .ConfigureWarnings(w => w.Log(RelationalEventId.QueryClientEvaluationWarning)
                                                     .Log(CoreEventId.FirstWithoutOrderByAndFilterWarning)))
                    .MaxBatchSize(1);
                return builder;
            }
        }
    }
}
