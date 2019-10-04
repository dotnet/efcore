// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Update
{
    public class BatchExecutorTest
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ExecuteAsync_calls_Commit_if_no_transaction(bool async)
        {
            using (var context = new TestContext())
            {
                var connection = SetupConnection(context);

                context.Add(
                    new Foo
                    {
                        Id = "1"
                    });

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
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ExecuteAsync_does_not_call_Commit_if_existing_transaction(bool async)
        {
            using (var context = new TestContext())
            {
                var connection = SetupConnection(context);
                var transaction = new FakeDbTransaction(connection);
                context.Database.UseTransaction(transaction);

                context.Add(
                    new Foo
                    {
                        Id = "1"
                    });

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
        }

        private static FakeDbConnection SetupConnection(TestContext context)
        {
            var dataReader = new FakeDbDataReader(
                new[] { "RowsAffected" }, new List<object[]>
                {
                    new object[] { 1 }
                });

            var connection = new FakeDbConnection(
                "A=B", new FakeCommandExecutor(
                    executeReader: (c, b) => dataReader,
                    executeReaderAsync: (c, b, ct) => Task.FromResult<DbDataReader>(dataReader)));

            ((FakeRelationalConnection)context.GetService<IRelationalConnection>()).UseConnection(connection);
            return connection;
        }

        private class TestContext : DbContext
        {
            public TestContext()
                : base(RelationalTestHelpers.Instance.CreateOptions())
            {
            }

            public DbSet<Foo> Foos { get; set; }
        }

        private class Foo
        {
            public string Id { get; set; }
        }
    }
}
