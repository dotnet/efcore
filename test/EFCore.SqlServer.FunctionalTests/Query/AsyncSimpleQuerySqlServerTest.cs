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
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable AccessToDisposedClosure
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsyncSimpleQuerySqlServerTest : AsyncSimpleQueryTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public AsyncSimpleQuerySqlServerTest(NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalFact]
        public async Task Query_compiler_concurrency()
        {
            const int threadCount = 50;

            var tasks = new Task[threadCount];

            for (var i = 0; i < threadCount; i++)
            {
                tasks[i] = Task.Run(
                    () =>
                        {
                            using (var context = CreateContext())
                            {
                                using ((from c in context.Customers
                                        where c.City == "London"
                                        orderby c.CustomerID
                                        select (from o1 in context.Orders
                                                where o1.CustomerID == c.CustomerID
                                                      && o1.OrderDate.Value.Year == 1997
                                                orderby o1.OrderID
                                                select (from o2 in context.Orders
                                                        where o1.CustomerID == c.CustomerID
                                                        orderby o2.OrderID
                                                        select o1.OrderID)))
                                    .GetEnumerator())
                                {
                                }
                            }
                        });
            }

            await Task.WhenAll(tasks);
        }

        [ConditionalFact]
        public async Task Race_when_context_disposed_before_query_termination()
        {
            DbSet<Customer> task;

            using (var context = CreateContext())
            {
                task = context.Customers;
            }

            await Assert.ThrowsAsync<ObjectDisposedException>(() => task.SingleAsync(c => c.CustomerID == "ALFKI"));
        }

        public override async Task String_Contains_Literal()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains("M")), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains("M") || c.ContactName.Contains("m")), // case-sensitive
                entryCount: 34);
        }

        public override async Task String_Contains_MethodCall()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains(LocalMethod1())), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains(LocalMethod1().ToLower()) || c.ContactName.Contains(LocalMethod1().ToUpper())), // case-sensitive
                entryCount: 34);
        }

        public async Task Skip_when_no_order_by()
        {
            await Assert.ThrowsAsync<Exception>(async () => await AssertQuery<Customer>(cs => cs.Skip(5).Take(10)));
        }

        [Fact]
        public async Task Single_Predicate_Cancellation()
        {
            await Assert.ThrowsAsync<TaskCanceledException>(
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
                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    c => { context.Orders.Where(o => o.CustomerID == c.CustomerID).ToList(); });
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
                                    "[dbo].[CustOrderHist] @CustomerID = {0}",
                                    asyncEnumerator.Current.CustomerID));
                        }
                        else
                        {
                            await context.Database.ExecuteSqlCommandAsync(
                                "[dbo].[CustOrderHist] @CustomerID = {0}",
                                asyncEnumerator.Current.CustomerID);
                        }
                    }
                }
            }
        }

        [Fact]
        public async Task Cancelation_token_properly_passed_to_GetResult_method_for_queries_with_result_operators_and_outer_parameter_injection()
        {
            await AssertQuery<Order>(
                os => os.Select(o => new { o.Customer.City, Count = o.OrderDetails.Count() }),
                elementSorter: e => e.City + " " + e.Count);
        }

        [Fact]
        public async Task Sum_with_no_data_nullable_legacy_behavior()
        {
            AppContext.SetSwitch("Microsoft.EntityFrameworkCore.Issue12314", true);

            try
            {
                await Assert.ThrowsAsync<InvalidOperationException>(
                    async () => await AssertSingleResult<Order>(os => os.Where(o => o.OrderID < 0).Select(o => (int?)o.OrderID).SumAsync()));
            }
            finally
            {
                AppContext.SetSwitch("Microsoft.EntityFrameworkCore.Issue12314", false);
            }
        }
    }
}
