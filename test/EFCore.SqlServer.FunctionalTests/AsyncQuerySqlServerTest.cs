// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable AccessToDisposedClosure

#pragma warning disable 1998
namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class AsyncQuerySqlServerTest : AsyncQueryTestBase<NorthwindQuerySqlServerFixture>
    {
        public AsyncQuerySqlServerTest(NorthwindQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        public override async Task ToList_on_nav_in_projection_is_async()
        {
            await base.ToList_on_nav_in_projection_is_async();

            Assert.Contains(
                @"_SelectAsync(
            source: IAsyncEnumerable<Customer> _ShapedQuery(
                queryContext: queryContext, 
                shaperCommandContext: SelectExpression: 
                    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
                    FROM [Customers] AS [c]
                    WHERE [c].[CustomerID] = N'ALFKI', 
                shaper: BufferedEntityShaper<Customer>), 
            selector: (Customer c | CancellationToken Param_0) => Task<<>f__AnonymousType3<Customer, List<Order>>> _ExecuteAsync(
                taskFactories: new Func<Task<object>>[]{ () => Task<object> _ToObjectTask(Task<List<Order>> ToList((IAsyncEnumerable<Order>) EnumerableAdapter<Order> _ToEnumerable(IAsyncEnumerable<Order> _InjectParameters(
                                    queryContext: queryContext, 
                                    source: IAsyncEnumerable<Order> _ShapedQuery(
                                        queryContext: queryContext, 
                                        shaperCommandContext: SelectExpression: 
                                            SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
                                            FROM [Orders] AS [o]
                                            WHERE @_outer_CustomerID = [o].[CustomerID], 
                                        shaper: BufferedEntityShaper<Order>), 
                                    parameterNames: new string[]{ ""_outer_CustomerID"" }, 
                                    parameterValues: new object[]{ string GetValueFromEntity(
                                            clrPropertyGetter: ClrPropertyGetter<Customer, string>, 
                                            entity: c) })))) }, 
                selector: (Object[] results) => new <>f__AnonymousType3<Customer, List<Order>>(
                    c, 
                    (List<Order>) results[0]
                )))",
                TestSqlLoggerFactory.Log);
        }

        [ConditionalFact]
        public async Task Query_compiler_concurrency()
        {
            const int threadCount = 50;

            var tasks = new Task[threadCount];

            for (var i = 0; i < threadCount; i++)
            {
                tasks[i] = Task.Run(() =>
                    {
                        using (var context = CreateContext())
                        {
                            var enumerator = (from c in context.Customers
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
                                .GetEnumerator();
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

        // TODO: Complex projection translation.

        public override async Task Projection_when_arithmetic_expressions()
        {
            //base.Projection_when_arithmetic_expressions();
        }

        public override async Task Projection_when_arithmetic_mixed()
        {
            //base.Projection_when_arithmetic_mixed();
        }

        public override async Task Projection_when_arithmetic_mixed_subqueries()
        {
            //base.Projection_when_arithmetic_mixed_subqueries();
        }

        public override async Task String_Contains_Literal() => 
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains("M")), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains("M") || c.ContactName.Contains("m")), // case-sensitive
                entryCount: 34);

        public override async Task String_Contains_MethodCall()
            => await AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains(LocalMethod1())), // case-insensitive
                cs => cs.Where(c =>c.ContactName.Contains(LocalMethod1().ToLower()) || c.ContactName.Contains(LocalMethod1().ToUpper())), // case-sensitive
                entryCount: 34);

        public async Task Skip_when_no_order_by()
            => await Assert.ThrowsAsync<Exception>(async () => await AssertQuery<Customer>(cs => cs.Skip(5).Take(10)));

        [Fact]
        public async Task Single_Predicate_Cancellation()
            => await Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await Single_Predicate_Cancellation(Fixture.CancelQuery()));

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
        public async Task Cancelation_token_properly_passed_to_GetResult_method_for_queries_with_result_operators_and_outer_parameter_injection()
            => await AssertQuery<Order>(
                os => os.Select(o => new { o.Customer.City, Count = o.OrderDetails.Count() }));
    }
}
