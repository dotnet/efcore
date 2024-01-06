// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

namespace Microsoft.EntityFrameworkCore.Update;

public class BatchExecutorTest
{
    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExecuteAsync_calls_Commit_if_no_transaction(bool async)
    {
        using var context = new TestContext();
        var connection = SetupConnection(context);

        await context.AddAsync(new Foo { Id = "1" });
        await context.AddAsync(new Bar { Id = "1" });

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        Assert.Equal(1, connection.DbTransactions.Single().CommitCount);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExecuteAsync_does_not_call_Commit_if_existing_transaction(bool async)
    {
        using var context = new TestContext();
        var connection = SetupConnection(context);
        var transaction = new FakeDbTransaction(connection);
        context.Database.UseTransaction(transaction);

        await context.AddAsync(
            new Foo { Id = "1" });

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        Assert.Empty(connection.DbTransactions);
        Assert.Equal(0, transaction.CommitCount);
    }

    private static FakeDbConnection SetupConnection(TestContext context)
    {
        var dataReader = new FakeDbDataReader(
            ["RowsAffected"], new List<object[]> { new object[] { 1 } });

        var connection = new FakeDbConnection(
            "A=B", new FakeCommandExecutor(
                executeReader: (c, b) => dataReader,
                executeReaderAsync: (c, b, ct) => Task.FromResult<DbDataReader>(dataReader)));

        ((FakeRelationalConnection)context.GetService<IRelationalConnection>()).UseConnection(connection);
        return connection;
    }

    private class TestContext : DbContext
    {
        private static readonly IServiceProvider _serviceProvider
            = FakeRelationalOptionsExtension.AddEntityFrameworkRelationalDatabase(
                    new ServiceCollection())
                .BuildServiceProvider(validateScopes: true);

        public TestContext()
            : base(FakeRelationalTestHelpers.Instance.CreateOptions(_serviceProvider))
        {
        }

        public DbSet<Foo> Foos { get; set; }
        public DbSet<Bar> Bars { get; set; }
    }

    private class Foo
    {
        public string Id { get; set; }
    }

    private class Bar
    {
        public string Id { get; set; }
    }
}
