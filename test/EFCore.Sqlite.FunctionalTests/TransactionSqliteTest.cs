// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class TransactionSqliteTest : TransactionTestBase<TransactionSqliteTest.TransactionSqliteFixture>
    {
        public TransactionSqliteTest(TransactionSqliteFixture fixture)
            : base(fixture)
        {
        }

        protected override bool SnapshotSupported => false;

        protected override DbContext CreateContextWithConnectionString()
        {
            var options = Fixture.AddOptions(
                    new DbContextOptionsBuilder().UseSqlite(TestStore.ConnectionString))
                .UseInternalServiceProvider(Fixture.ServiceProvider);

            return new DbContext(options.Options);
        }

        public class TransactionSqliteFixture : TransactionFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;

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
                return base.AddOptions(builder)
                    .ConfigureWarnings(
                        w
                            => w.Log(RelationalEventId.QueryClientEvaluationWarning)
                                .Log(CoreEventId.FirstWithoutOrderByAndFilterWarning));
            }
        }
    }
}
