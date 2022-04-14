// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

// ReSharper disable MethodHasAsyncOverload

namespace Microsoft.EntityFrameworkCore;

public class TransactionSqlServerTest : TransactionTestBase<TransactionSqlServerTest.TransactionSqlServerFixture>
{
    public TransactionSqlServerTest(TransactionSqlServerFixture fixture)
        : base(fixture)
    {
    }

    // Test relies on savepoints, which are disabled when MARS is enabled
    public override Task SaveChanges_implicitly_creates_savepoint(bool async)
        => new SqlConnectionStringBuilder(TestStore.ConnectionString).MultipleActiveResultSets
            ? Task.CompletedTask
            : base.SaveChanges_implicitly_creates_savepoint(async);

    // Savepoints cannot be released in SQL Server
    public override Task Savepoint_can_be_released(bool async)
        => Task.CompletedTask;

    // Test relies on savepoints, which are disabled when MARS is enabled
    public override Task SaveChanges_uses_explicit_transaction_with_failure_behavior(bool async, bool autoTransaction)
        => new SqlConnectionStringBuilder(TestStore.ConnectionString).MultipleActiveResultSets
            ? Task.CompletedTask
            : base.SaveChanges_uses_explicit_transaction_with_failure_behavior(async, autoTransaction);

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Savepoints_are_disabled_with_MARS(bool async)
    {
        await using var context = CreateContextWithConnectionString(
            SqlServerTestStore.CreateConnectionString(TestStore.Name, multipleActiveResultSets: true));

        await using var transaction = await context.Database.BeginTransactionAsync();

        var orderId = 300;
        foreach (var _ in context.Set<TransactionCustomer>())
        {
            context.Add(new TransactionOrder { Id = orderId++, Name = "Order " + orderId });
            if (async)
            {
                await context.SaveChangesAsync();
            }
            else
            {
                context.SaveChanges();
            }
        }

        await transaction.CommitAsync();

        Assert.Contains(Fixture.ListLoggerFactory.Log, t => t.Id == SqlServerEventId.SavepointsDisabledBecauseOfMARS);
    }

    protected override bool SnapshotSupported
        => true;

    protected override bool AmbientTransactionsSupported
        => true;

    protected override DbContext CreateContextWithConnectionString()
        => CreateContextWithConnectionString(null);

    protected DbContext CreateContextWithConnectionString(string connectionString)
    {
        var options = Fixture.AddOptions(
                new DbContextOptionsBuilder()
                    .UseSqlServer(
                        connectionString ?? TestStore.ConnectionString,
                        b => b.ApplyConfiguration().ExecutionStrategy(c => new SqlServerExecutionStrategy(c))))
            .ConfigureWarnings(b => b.Log(SqlServerEventId.SavepointsDisabledBecauseOfMARS))
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
            builder.ConfigureWarnings(b => b.Log(SqlServerEventId.SavepointsDisabledBecauseOfMARS));
            return builder;
        }
    }
}
