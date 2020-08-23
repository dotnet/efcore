// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable AccessToDisposedClosure
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindAsyncSimpleQuerySqlServerTest : NorthwindAsyncSimpleQueryRelationalTestBase<
        NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public NorthwindAsyncSimpleQuerySqlServerTest(
            NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override bool CanExecuteQueryString
            => true;

        [ConditionalFact]
        public override Task Throws_on_concurrent_query_list()
        {
            return base.Throws_on_concurrent_query_list();
        }

        [ConditionalFact]
        public Task Query_compiler_concurrency()
        {
            const int threadCount = 50;

            var tasks = new Task[threadCount];

            for (var i = 0; i < threadCount; i++)
            {
                tasks[i] = Task.Run(
                    () =>
                    {
                        using var context = CreateContext();
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
                                                select o1.OrderID).ToList()).ToList())
                            .GetEnumerator())
                        {
                        }
                    });
            }

            return Task.WhenAll(tasks);
        }

        [ConditionalFact(Skip = "Issue#16218")]
        public Task Race_when_context_disposed_before_query_termination()
        {
            DbSet<Customer> task;

            using (var context = CreateContext())
            {
                task = context.Customers;
            }

            return Assert.ThrowsAsync<ObjectDisposedException>(() => task.SingleAsync(c => c.CustomerID == "ALFKI"));
        }

        [ConditionalFact]
        public Task Single_Predicate_Cancellation()
        {
            return Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => Single_Predicate_Cancellation_test(Fixture.TestSqlLoggerFactory.CancelQuery()));
        }

        [ConditionalFact]
        public async Task Concurrent_async_queries_are_serialized2()
        {
            using var context = CreateContext();
            await context.OrderDetails
                .Where(od => od.OrderID > 0)
                .Intersect(
                    context.OrderDetails
                        .Where(od => od.OrderID > 0))
                .Intersect(
                    context.OrderDetails
                        .Where(od => od.OrderID > 0)).ToListAsync();
        }

        [ConditionalFact]
        public async Task Concurrent_async_queries_when_raw_query()
        {
            using var context = CreateContext();
            await using var asyncEnumerator = context.Customers.AsAsyncEnumerable().GetAsyncEnumerator();
            while (await asyncEnumerator.MoveNextAsync())
            {
                // Outer query is buffered by default
                await context.Database.ExecuteSqlRawAsync(
                    "[dbo].[CustOrderHist] @CustomerID = {0}",
                    asyncEnumerator.Current.CustomerID);
            }
        }

        [ConditionalFact(Skip = "Issue#16218")]
        public override Task Select_bitwise_and_with_logical_and()
        {
            return base.Select_bitwise_and_with_logical_and();
        }

        [ConditionalFact(Skip = "Issue#16218")]
        public override Task Mixed_sync_async_in_query_cache()
        {
            return base.Mixed_sync_async_in_query_cache();
        }
    }
}
