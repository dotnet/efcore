// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToExpressionBodyWhenPossible
// ReSharper disable AccessToDisposedClosure
// ReSharper disable AccessToModifiedClosure
// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable StringEndsWithIsCultureSpecific
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindAsyncSimpleQueryTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected NorthwindAsyncSimpleQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected NorthwindContext CreateContext()
            => Fixture.CreateContext();

        [ConditionalFact]
        public virtual async Task Query_backed_by_database_view()
        {
            using var context = CreateContext();
            var results = await context.Set<ProductView>().ToArrayAsync();

            Assert.Equal(69, results.Length);
        }

        [ConditionalFact]
        public virtual async Task ToList_context_subquery_deadlock_issue()
        {
            using var context = CreateContext();
            var _ = await context.Customers
                .Select(
                    c => new
                    {
                        c.CustomerID,
                        Posts = context.Orders.Where(o => o.CustomerID == c.CustomerID)
                            .Select(
                                m => new { m.CustomerID })
                            .ToList()
                    })
                .ToListAsync();
        }

        [ConditionalFact]
        public virtual async Task ToArray_on_nav_subquery_in_projection()
        {
            using var context = CreateContext();
            var results
                = await context.Customers.Select(
                        c => new { Orders = c.Orders.ToArray() })
                    .ToListAsync();

            Assert.Equal(830, results.SelectMany(a => a.Orders).ToList().Count);
        }

        [ConditionalFact]
        public virtual async Task ToArray_on_nav_subquery_in_projection_nested()
        {
            using var context = CreateContext();
            var results
                = await context.Customers.Select(
                        c => new
                        {
                            Orders = c.Orders.Select(
                                    o => new { OrderDetails = o.OrderDetails.ToArray() })
                                .ToArray()
                        })
                    .ToListAsync();

            Assert.Equal(2155, results.SelectMany(a => a.Orders.SelectMany(o => o.OrderDetails)).ToList().Count);
        }

        [ConditionalFact]
        public virtual async Task ToList_on_nav_subquery_in_projection()
        {
            using var context = CreateContext();
            var results
                = await context.Customers.Select(
                        c => new { Orders = c.Orders.ToList() })
                    .ToListAsync();

            Assert.Equal(830, results.SelectMany(a => a.Orders).ToList().Count);
        }

        [ConditionalFact]
        public virtual async Task ToList_on_nav_subquery_with_predicate_in_projection()
        {
            using var context = CreateContext();
            var results
                = await context.Customers.Select(
                        c => new { Orders = c.Orders.Where(o => o.OrderID > 10).ToList() })
                    .ToListAsync();

            Assert.Equal(830, results.SelectMany(a => a.Orders).ToList().Count);
        }

        [ConditionalFact(Skip = "Issue #17775")]
        public virtual async Task Average_on_nav_subquery_in_projection()
        {
            using var context = CreateContext();
            var results
                = await context.Customers.Select(
                        c => new { Ave = c.Orders.Average(o => o.OrderID) })
                    .ToListAsync();

            Assert.Equal(91, results.ToList().Count);
        }

        [ConditionalFact]
        public virtual async Task ToListAsync_can_be_canceled()
        {
            for (var i = 0; i < 10; i++)
            {
                // without fix, this usually throws within 2 or three iterations

                using var context = CreateContext();
                var tokenSource = new CancellationTokenSource();
                var query = context.Employees.AsNoTracking().ToListAsync(tokenSource.Token);
                tokenSource.Cancel();
                List<Employee> result = null;
                Exception exception = null;
                try
                {
                    result = await query;
                }
                catch (Exception e)
                {
                    exception = e;
                }

                if (exception != null)
                {
                    Assert.Null(result);
                }
                else
                {
                    Assert.Equal(9, result.Count);
                }
            }
        }

        [ConditionalFact(Skip = "Issue #17019")]
        public virtual async Task Mixed_sync_async_query()
        {
            using var context = CreateContext();
            var results
                = (await context.Customers
                    .Select(
                        c => new { c.CustomerID, Orders = context.Orders.Where(o => o.Customer.CustomerID == c.CustomerID) })
                    .ToListAsync())
                .Select(
                    x => new
                    {
                        Orders = x.Orders
                            .GroupJoin(
                                new[] { "ALFKI" }, y => x.CustomerID, y => y, (h, id) => new { h.Customer })
                    })
                .ToList();

            Assert.Equal(830, results.SelectMany(r => r.Orders).ToList().Count);
        }

        [ConditionalFact]
        public virtual async Task LoadAsync_should_track_results()
        {
            using var context = CreateContext();
            await context.Customers.LoadAsync();

            Assert.Equal(91, context.ChangeTracker.Entries().Count());
        }

        protected virtual async Task Single_Predicate_Cancellation_test(CancellationToken cancellationToken)
        {
            using var ctx = CreateContext();
            var result = await ctx.Customers.SingleAsync(c => c.CustomerID == "ALFKI", cancellationToken);

            Assert.Equal("ALFKI", result.CustomerID);
        }

        [ConditionalFact]
        public virtual async Task Mixed_sync_async_in_query_cache()
        {
            using var context = CreateContext();
            Assert.Equal(91, context.Customers.AsNoTracking().ToList().Count);
            Assert.Equal(91, (await context.Customers.AsNoTracking().ToListAsync()).Count);
        }

        [ConditionalFact]
        public virtual async Task Throws_on_concurrent_query_list()
        {
            using var context = CreateContext();
            using var synchronizationEvent = new ManualResetEventSlim(false);
            using var blockingSemaphore = new SemaphoreSlim(0);
            var blockingTask = Task.Run(
                () =>
                {
                    try
                    {
                        context.Customers.Select(
                            c => Process(c, synchronizationEvent, blockingSemaphore)).ToList();
                    }
                    finally
                    {
                        synchronizationEvent.Set();
                    }
                });

            var throwingTask = Task.Run(
                async () =>
                {
                    synchronizationEvent.Wait(TimeSpan.FromMinutes(5));

                    Assert.Equal(
                        CoreStrings.ConcurrentMethodInvocation,
                        (await Assert.ThrowsAsync<InvalidOperationException>(
                            () => context.Customers.ToListAsync())).Message);
                });

            await throwingTask;

            blockingSemaphore.Release(1);

            await blockingTask;
        }

        [ConditionalFact]
        public virtual async Task Throws_on_concurrent_query_first()
        {
            using var context = CreateContext();
            using var synchronizationEvent = new ManualResetEventSlim(false);
            using var blockingSemaphore = new SemaphoreSlim(0);
            var blockingTask = Task.Run(
                () =>
                {
                    try
                    {
                        context.Customers.Select(
                            c => Process(c, synchronizationEvent, blockingSemaphore)).ToList();
                    }
                    finally
                    {
                        synchronizationEvent.Set();
                    }
                });

            var throwingTask = Task.Run(
                async () =>
                {
                    synchronizationEvent.Wait(TimeSpan.FromMinutes(5));

                    Assert.Equal(
                        CoreStrings.ConcurrentMethodInvocation,
                        (await Assert.ThrowsAsync<InvalidOperationException>(
                            () => context.Customers.FirstAsync())).Message);
                });

            await throwingTask;

            blockingSemaphore.Release(1);

            await blockingTask;
        }

        private static Customer Process(Customer c, ManualResetEventSlim e, SemaphoreSlim s)
        {
            e.Set();
            s.Wait(TimeSpan.FromMinutes(5));
            s.Release(1);
            return c;
        }

        // Set Operations

        [ConditionalFact]
        public virtual async Task Concat_dbset()
        {
            using var context = CreateContext();
            var query = await context.Set<Customer>()
                .Where(c => c.City == "México D.F.")
                .Concat(
                    context.Set<Customer>())
                .ToListAsync();

            Assert.Equal(96, query.Count);
        }

        [ConditionalFact]
        public virtual async Task Concat_simple()
        {
            using var context = CreateContext();
            var query = await context.Set<Customer>()
                .Where(c => c.City == "México D.F.")
                .Concat(
                    context.Set<Customer>()
                        .Where(s => s.ContactTitle == "Owner"))
                .ToListAsync();

            Assert.Equal(22, query.Count);
        }

        [ConditionalFact]
        public virtual async Task Concat_non_entity()
        {
            using var context = CreateContext();
            var query = await context.Set<Customer>()
                .Where(c => c.City == "México D.F.")
                .Select(c => c.CustomerID)
                .Concat(
                    context.Set<Customer>()
                        .Where(s => s.ContactTitle == "Owner")
                        .Select(c => c.CustomerID))
                .ToListAsync();

            Assert.Equal(22, query.Count);
        }

        [ConditionalFact]
        public virtual async Task Except_non_entity()
        {
            using var context = CreateContext();
            var query = await context.Set<Customer>()
                .Where(s => s.ContactTitle == "Owner")
                .Select(c => c.CustomerID)
                .Except(
                    context.Set<Customer>()
                        .Where(c => c.City == "México D.F.")
                        .Select(c => c.CustomerID))
                .ToListAsync();

            Assert.Equal(14, query.Count);
        }

        [ConditionalFact]
        public virtual async Task Intersect_non_entity()
        {
            using var context = CreateContext();
            var query = await context.Set<Customer>()
                .Where(c => c.City == "México D.F.")
                .Select(c => c.CustomerID)
                .Intersect(
                    context.Set<Customer>()
                        .Where(s => s.ContactTitle == "Owner")
                        .Select(c => c.CustomerID))
                .ToListAsync();

            Assert.Equal(3, query.Count);
        }

        [ConditionalFact]
        public virtual async Task Union_non_entity()
        {
            using var context = CreateContext();
            var query = await context.Set<Customer>()
                .Where(s => s.ContactTitle == "Owner")
                .Select(c => c.CustomerID)
                .Union(
                    context.Set<Customer>()
                        .Where(c => c.City == "México D.F.")
                        .Select(c => c.CustomerID))
                .ToListAsync();

            Assert.Equal(19, query.Count);
        }

        [ConditionalFact]
        public virtual async Task Select_bitwise_or()
        {
            using var context = CreateContext();
            var query = await context.Customers.OrderBy(c => c.CustomerID).Select(
                c => new { c.CustomerID, Value = c.CustomerID == "ALFKI" | c.CustomerID == "ANATR" }).ToListAsync();

            Assert.All(query.Take(2), t => Assert.True(t.Value));
            Assert.All(query.Skip(2), t => Assert.False(t.Value));
        }

        [ConditionalFact]
        public virtual async Task Select_bitwise_or_multiple()
        {
            using var context = CreateContext();
            var query = await context.Customers.OrderBy(c => c.CustomerID)
                .Select(
                    c => new { c.CustomerID, Value = c.CustomerID == "ALFKI" | c.CustomerID == "ANATR" | c.CustomerID == "ANTON" })
                .ToListAsync();

            Assert.All(query.Take(3), t => Assert.True(t.Value));
            Assert.All(query.Skip(3), t => Assert.False(t.Value));
        }

        [ConditionalFact]
        public virtual async Task Select_bitwise_and()
        {
            using var context = CreateContext();
            var query = await context.Customers.OrderBy(c => c.CustomerID).Select(
                c => new { c.CustomerID, Value = c.CustomerID == "ALFKI" & c.CustomerID == "ANATR" }).ToListAsync();

            Assert.All(query, t => Assert.False(t.Value));
        }

        [ConditionalFact]
        public virtual async Task Select_bitwise_and_or()
        {
            using var context = CreateContext();
            var query = await context.Customers.OrderBy(c => c.CustomerID)
                .Select(
                    c => new { c.CustomerID, Value = c.CustomerID == "ALFKI" & c.CustomerID == "ANATR" | c.CustomerID == "ANTON" })
                .ToListAsync();

            Assert.All(query.Where(c => c.CustomerID != "ANTON"), t => Assert.False(t.Value));
        }

        [ConditionalFact]
        public virtual async Task Select_bitwise_or_with_logical_or()
        {
            using var context = CreateContext();
            var query = await context.Customers.OrderBy(c => c.CustomerID).Select(
                    c => new { c.CustomerID, Value = c.CustomerID == "ALFKI" | c.CustomerID == "ANATR" || c.CustomerID == "ANTON" })
                .ToListAsync();

            Assert.All(query.Take(3), t => Assert.True(t.Value));
            Assert.All(query.Skip(3), t => Assert.False(t.Value));
        }

        [ConditionalFact]
        public virtual async Task Select_bitwise_and_with_logical_and()
        {
            using var context = CreateContext();
            var query = await context.Customers.OrderBy(c => c.CustomerID).Select(
                    c => new { c.CustomerID, Value = c.CustomerID == "ALFKI" & c.CustomerID == "ANATR" && c.CustomerID == "ANTON" })
                .ToListAsync();

            Assert.All(query, t => Assert.False(t.Value));
        }
    }
}
