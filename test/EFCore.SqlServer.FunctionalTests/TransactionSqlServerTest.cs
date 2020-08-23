// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class TransactionSqlServerTest : TransactionTestBase<TransactionSqlServerTest.TransactionSqlServerFixture>
    {
        public TransactionSqlServerTest(TransactionSqlServerFixture fixture)
            : base(fixture)
        {
        }

        protected override bool SnapshotSupported
            => true;

        protected override bool AmbientTransactionsSupported
            => true;

        protected override DbContext CreateContextWithConnectionString()
        {
            var options = Fixture.AddOptions(
                    new DbContextOptionsBuilder()
                        .UseSqlServer(
                            TestStore.ConnectionString,
                            b => b.ApplyConfiguration().ExecutionStrategy(c => new SqlServerExecutionStrategy(c))))
                .UseInternalServiceProvider(Fixture.ServiceProvider);

            return new DbContext(options.Options);
        }

        public class TransactionSqlServerFixture : TransactionFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqlServerTestStoreFactory.Instance;

            protected override void Seed(PoolableDbContext context)
            {
                base.Seed(context);

                context.Database.ExecuteSqlRaw("ALTER DATABASE [" + StoreName + "] SET ALLOW_SNAPSHOT_ISOLATION ON");
                context.Database.ExecuteSqlRaw("ALTER DATABASE [" + StoreName + "] SET READ_COMMITTED_SNAPSHOT ON");
            }

            public override void Reseed()
            {
                using var context = CreateContext();
                context.Set<TransactionCustomer>().RemoveRange(context.Set<TransactionCustomer>());
                context.Set<TransactionOrder>().RemoveRange(context.Set<TransactionOrder>());
                context.SaveChanges();

                base.Seed(context);
            }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                new SqlServerDbContextOptionsBuilder(
                        base.AddOptions(builder))
                    .ExecutionStrategy(c => new SqlServerExecutionStrategy(c));
                return builder;
            }
        }
    }
}
