// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable AccessToDisposedClosure
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsyncSimpleQueryOracleTest : AsyncSimpleQueryTestBase<NorthwindQueryOracleFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public AsyncSimpleQueryOracleTest(NorthwindQueryOracleFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
            //fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override Task Query_backed_by_database_view()
        {
            // TODO: #10680

            return Task.CompletedTask;
        }

        [Fact(Skip = "Issue #13029")]
        public Task Single_Predicate_Cancellation()
        {
            return Assert.ThrowsAsync<OperationCanceledException>(
                async () =>
                    await Single_Predicate_Cancellation_test(Fixture.TestSqlLoggerFactory.CancelQuery()));
        }

        [Fact]
        public async Task Concurrent_async_queries_are_serialized()
        {
            using (var context = CreateContext())
            {
                var task1 = context.Customers.Where(c => c.City == "México D.F.").ToListAsync();
                var task2 = context.Customers.Where(c => c.City == "London").ToListAsync();
                var task3 = context.Customers.Where(c => c.City == "Sao Paulo").ToListAsync();

                var tasks = await Task.WhenAll(task1, task2, task3);

                Assert.Equal(5, tasks[0].Count);
                Assert.Equal(6, tasks[1].Count);
                Assert.Equal(4, tasks[2].Count);
            }
        }

        [Fact]
        public async Task Concurrent_async_queries_are_serialized2()
        {
            using (var context = CreateContext())
            {
                await context.OrderDetails
                    .Where(od => od.OrderID > 0)
                    .Intersect(
                        context.OrderDetails
                            .Where(od => od.OrderID > 0))
                    .Intersect(
                        context.OrderDetails
                            .Where(od => od.OrderID > 0)).ToListAsync();
            }
        }

        [Fact]
        public async Task Concurrent_async_queries_are_serialized_find()
        {
            using (var context = CreateContext())
            {
                var task1 = context.Customers.FindAsync("ALFKI");
                var task2 = context.Customers.FindAsync("ANATR");
                var task3 = context.Customers.FindAsync("FISSA");

                var tasks = await Task.WhenAll(task1, task2, task3);

                Assert.NotNull(tasks[0]);
                Assert.NotNull(tasks[1]);
                Assert.NotNull(tasks[2]);
            }
        }

        [Fact]
        public async Task Concurrent_async_queries_are_serialized_mixed1()
        {
            using (var context = CreateContext())
            {
                await context.Customers.ForEachAsync(
                    c => context.Orders.Where(o => o.CustomerID == c.CustomerID).ToList());
            }
        }

        [Fact]
        public async Task Concurrent_async_queries_are_serialized_mixed2()
        {
            using (var context = CreateContext())
            {
                foreach (var c in context.Customers)
                {
                    await context.Orders.Where(o => o.CustomerID == c.CustomerID).ToListAsync();
                }
            }
        }

        [Fact]
        public async Task Concurrent_async_queries_when_raw_query()
        {
            using (var context = CreateContext())
            {
                using (var asyncEnumerator = context.Customers.AsAsyncEnumerable().GetEnumerator())
                {
                    while (await asyncEnumerator.MoveNext(default))
                    {
                        if (!context.GetService<IRelationalConnection>().IsMultipleActiveResultSetsEnabled)
                        {
                            // Not supported, we could make it work by triggering buffering
                            // from RelationalCommand.

                            await Assert.ThrowsAsync<InvalidOperationException>(
                                () => context.Database.ExecuteSqlCommandAsync(
                                    @"SELECT ""CustomerID"" FROM ""Customers"" Where ""CustomerID"" = {0}",
                                    asyncEnumerator.Current.CustomerID));
                        }
                        else
                        {
                            await context.Database.ExecuteSqlCommandAsync(
                                @"SELECT ""CustomerID"" FROM ""Customers"" Where ""CustomerID"" = {0}",
                                asyncEnumerator.Current.CustomerID);
                        }
                    }
                }
            }
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public Task Cancelation_token_properly_passed_to_GetResult_method_for_queries_with_result_operators_and_outer_parameter_injection(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Select(o => new { o.Customer.City, Count = o.OrderDetails.Count() }),
                elementSorter: e => e.City + " " + e.Count);
        }
    }
}
