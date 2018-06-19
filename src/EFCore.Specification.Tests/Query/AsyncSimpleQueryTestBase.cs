// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToExpressionBodyWhenPossible
// ReSharper disable AccessToDisposedClosure
// ReSharper disable AccessToModifiedClosure
// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable StringEndsWithIsCultureSpecific
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class AsyncSimpleQueryTestBase<TFixture> : AsyncQueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected AsyncSimpleQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();

        [ConditionalFact]
        public virtual async Task GroupBy_tracking_after_dispose()
        {
            List<IGrouping<string, Order>> groups;

            using (var context = CreateContext())
            {
                groups = await context.Orders.GroupBy(o => o.CustomerID).ToListAsync();
            }

            groups[0].First();
        }

        [ConditionalFact]
        public virtual async Task Query_simple()
        {
            await AssertQuery<CustomerView>(cvs => cvs);
        }

        [ConditionalFact]
        public virtual async Task Query_where_simple()
        {
            await AssertQuery<CustomerView>(
                cvs => cvs.Where(c => c.City == "London"));
        }

        [ConditionalFact]
        public virtual async Task Query_backed_by_database_view()
        {
            using (var context = CreateContext())
            {
                var results = await context.Query<ProductQuery>().ToArrayAsync();

                Assert.Equal(69, results.Length);
            }
        }

        [ConditionalFact]
        public virtual async Task ToList_context_subquery_deadlock_issue()
        {
            using (var context = CreateContext())
            {
                var results = await context.Customers
                    .Select(
                        c => new
                        {
                            c.CustomerID,
                            Posts = context.Orders.Where(o => o.CustomerID == c.CustomerID)
                                .Select(
                                    m => new
                                    {
                                        m.CustomerID
                                    })
                                    .ToList()
                        })
                    .ToListAsync();
            }
        }

        [ConditionalFact]
        public virtual async Task Query_with_nav()
        {
            await AssertQuery<OrderQuery>(ovs => ovs.Where(ov => ov.CustomerID == "ALFKI"));
        }

        [ConditionalFact]
        public virtual async Task Select_query_where_navigation()
        {
            await AssertQuery<OrderQuery>(
                ovs => from ov in ovs
                       where ov.Customer.City == "Seattle"
                       select ov);
        }

        [ConditionalFact]
        public virtual async Task Select_query_where_navigation_multi_level()
        {
            await AssertQuery<OrderQuery>(
                ovs => from ov in ovs
                       where ov.Customer.Orders.Any()
                       select ov);
        }

        [ConditionalFact]
        public virtual async Task Projection_when_client_evald_subquery()
        {
            await AssertQuery<Customer>(
                cs => cs.Select(c => string.Join(", ", c.Orders.Select(o => o.CustomerID))));
        }

        [ConditionalFact]
        public virtual async Task ToArray_on_nav_subquery_in_projection()
        {
            using (var context = CreateContext())
            {
                var results
                    = await context.Customers.Select(
                            c => new
                            {
                                Orders = c.Orders.ToArray()
                            })
                        .ToListAsync();

                Assert.Equal(830, results.SelectMany(a => a.Orders).ToList().Count);
            }
        }

        [ConditionalFact]
        public virtual async Task ToArray_on_nav_subquery_in_projection_nested()
        {
            using (var context = CreateContext())
            {
                var results
                    = await context.Customers.Select(
                            c => new
                            {
                                Orders = c.Orders.Select(
                                        o => new
                                        {
                                            OrderDetails = o.OrderDetails.ToArray()
                                        })
                                    .ToArray()
                            })
                        .ToListAsync();

                Assert.Equal(2155, results.SelectMany(a => a.Orders.SelectMany(o => o.OrderDetails)).ToList().Count);
            }
        }

        [ConditionalFact]
        public virtual async Task ToList_on_nav_subquery_in_projection()
        {
            using (var context = CreateContext())
            {
                var results
                    = await context.Customers.Select(
                            c => new
                            {
                                Orders = c.Orders.ToList()
                            })
                        .ToListAsync();

                Assert.Equal(830, results.SelectMany(a => a.Orders).ToList().Count);
            }
        }

        [ConditionalFact]
        public virtual async Task ToList_on_nav_subquery_with_predicate_in_projection()
        {
            using (var context = CreateContext())
            {
                var results
                    = await context.Customers.Select(
                            c => new
                            {
                                Orders = c.Orders.Where(o => o.OrderID > 10).ToList()
                            })
                        .ToListAsync();

                Assert.Equal(830, results.SelectMany(a => a.Orders).ToList().Count);
            }
        }

        [ConditionalFact]
        public virtual async Task Average_on_nav_subquery_in_projection()
        {
            using (var context = CreateContext())
            {
                var results
                    = await context.Customers.Select(
                            c => new
                            {
                                Ave = c.Orders.Average(o => o.Freight)
                            })
                        .ToListAsync();

                Assert.Equal(91, results.ToList().Count);
            }
        }

        [ConditionalFact]
        public virtual async Task Where_subquery_correlated_client_eval()
        {
            await AssertQuery<Customer>(
                cs => cs.Take(5).OrderBy(c1 => c1.CustomerID).Where(c1 => cs.Any(c2 => c1.CustomerID == c2.CustomerID && c2.IsLondon)),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task ToListAsync_can_be_canceled()
        {
            for (var i = 0; i < 10; i++)
            {
                // without fix, this usually throws within 2 or three iterations

                using (var context = CreateContext())
                {
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
        }

        [ConditionalFact]
        public virtual async Task Mixed_sync_async_query()
        {
            using (var context = CreateContext())
            {
                var results
                    = (await context.Customers
                        .Select(
                            c => new
                            {
                                c.CustomerID,
                                Orders = context.Orders.Where(o => o.Customer.CustomerID == c.CustomerID)
                            }).ToListAsync())
                    .Select(
                        x => new
                        {
                            Orders = x.Orders
                                .GroupJoin(
                                    new[] { "ALFKI" }, y => x.CustomerID, y => y, (h, id) => new
                                    {
                                        h.Customer
                                    })
                        })
                    .ToList();

                Assert.Equal(830, results.SelectMany(r => r.Orders).ToList().Count);
            }
        }

        [ConditionalFact]
        public virtual async Task LoadAsync_should_track_results()
        {
            using (var context = CreateContext())
            {
                await context.Customers.LoadAsync();

                Assert.Equal(91, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual async Task Where_all_any_client()
        {
            var expectedOrders = new[] { 1, 2, 3 };

            await AssertQuery<Customer>(
                cs => cs
                    .Where(
                        c => expectedOrders
                            .All(
                                expected => c.Orders.Select(o => o.OrderID)
                                    .Any(orderId => orderId == expected))));
        }

        protected virtual async Task Single_Predicate_Cancellation_test(CancellationToken cancellationToken)
        {
            await AssertSingleResult<Customer, Customer>(
                cs => cs.SingleAsync(c => c.CustomerID == "ALFKI", cancellationToken));
        }

        [ConditionalFact]
        public virtual async Task Mixed_sync_async_in_query_cache()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(91, context.Customers.AsNoTracking().ToList().Count);
                Assert.Equal(91, (await context.Customers.AsNoTracking().ToListAsync()).Count);
            }
        }

        [ConditionalFact]
        public virtual async Task Queryable_simple()
        {
            await AssertQuery<Customer>(
                cs => cs,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task Queryable_simple_anonymous()
        {
            await AssertQuery<Customer>(
                cs => cs.Select(c => new { c }),
                elementSorter: e => e.c.CustomerID,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task Queryable_nested_simple()
        {
            await AssertQuery<Customer>(
                cs =>
                    from c1 in (from c2 in (from c3 in cs select c3) select c2) select c1,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task Take_simple()
        {
            await AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Take(10),
                assertOrder: true,
                entryCount: 10);
        }

        [ConditionalFact]
        public virtual async Task Take_simple_projection()
        {
            await AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Select(c => c.City).Take(10),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual async Task Skip()
        {
            await AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Skip(5),
                assertOrder: true,
                entryCount: 86);
        }

        [ConditionalFact]
        public virtual async Task Take_Skip()
        {
            await AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Take(10).Skip(5),
                assertOrder: true,
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual async Task Distinct_Skip()
        {
            await AssertQuery<Customer>(
                cs => cs.Distinct().OrderBy(c => c.CustomerID).Skip(5),
                assertOrder: true,
                entryCount: 86);
        }

        [ConditionalFact]
        public virtual async Task Skip_Take()
        {
            await AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Skip(5).Take(10),
                assertOrder: true,
                entryCount: 10);
        }

        [ConditionalFact]
        public virtual async Task Distinct_Skip_Take()
        {
            await AssertQuery<Customer>(
                cs => cs.Distinct().OrderBy(c => c.ContactName).Skip(5).Take(10),
                assertOrder: true,
                entryCount: 10);
        }

        [ConditionalFact]
        public virtual async Task Skip_Distinct()
        {
            await AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Skip(5).Distinct(),
                entryCount: 86);
        }

        [ConditionalFact]
        public virtual async Task Skip_Take_Distinct()
        {
            await AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Skip(5).Take(10).Distinct(),
                entryCount: 10);
        }

        [ConditionalFact]
        public virtual async Task Take_Skip_Distinct()
        {
            await AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Take(10).Skip(5).Distinct(),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual async Task Take_Distinct()
        {
            await AssertQuery<Order>(
                os => os.OrderBy(o => o.OrderID).Take(5).Distinct(),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual async Task Distinct_Take()
        {
            await AssertQuery<Order>(
                os => os.Distinct().OrderBy(o => o.OrderID).Take(5),
                assertOrder: true,
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual async Task Distinct_Take_Count()
        {
            await AssertSingleResult<Order, int>(
                os => os.Distinct().Take(5).CountAsync());
        }

        [ConditionalFact]
        public virtual async Task Take_Distinct_Count()
        {
            await AssertSingleResult<Order>(
                os => os.Take(5).Distinct().CountAsync());
        }

        [ConditionalFact]
        public virtual async Task Any_simple()
        {
            await AssertSingleResult<Customer>(
                cs => cs.AnyAsync());
        }

        [ConditionalFact]
        public virtual async Task OrderBy_Take_Count()
        {
            await AssertSingleResult<Order>(
                os => os.OrderBy(o => o.OrderID).Take(5).CountAsync());
        }

        [ConditionalFact]
        public virtual async Task Take_OrderBy_Count()
        {
            await AssertSingleResult<Order>(
                os => os.Take(5).OrderBy(o => o.OrderID).CountAsync());
        }

        [ConditionalFact]
        public virtual async Task Any_predicate()
        {
            await AssertSingleResult<Customer>(
                cs => cs.AnyAsync(c => c.ContactName.StartsWith("A")));
        }

        [ConditionalFact]
        public virtual async Task All_top_level()
        {
            await AssertSingleResult<Customer>(
                cs => cs.AllAsync(c => c.ContactName.StartsWith("A")));
        }

        [ConditionalFact]
        public virtual async Task All_top_level_subquery()
        {
            // ReSharper disable once PossibleUnintendedReferenceComparison
            await AssertSingleResult<Customer>(
                cs => cs.AllAsync(c1 => cs.Any(c2 => cs.Any(c3 => c1 == c3))));
        }

        [ConditionalFact]
        public virtual async Task Select_into()
        {
            await AssertQuery<Customer>(
                cs =>
                    from c in cs
                    select c.CustomerID
                    into id
                    where id == "ALFKI"
                    select id);
        }

        [ConditionalFact]
        public virtual async Task Projection_when_arithmetic_expressions()
        {
            await AssertQuery<Order>(
                os => os.Select(
                    o => new
                    {
                        o.OrderID,
                        Double = o.OrderID * 2,
                        Add = o.OrderID + 23,
                        Sub = 100000 - o.OrderID,
                        Divide = o.OrderID / (o.OrderID / 2),
                        Literal = 42,
                        o
                    }),
                elementSorter: e => e.OrderID,
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual async Task Projection_when_arithmetic_mixed()
        {
            await AssertQuery<Order, Employee>(
                (os, es) =>
                    from o in os
                    from e in es
                    select new
                    {
                        Add = e.EmployeeID + o.OrderID,
                        o.OrderID,
                        o,
                        Literal = 42,
                        e.EmployeeID,
                        e
                    },
                elementSorter: e => e.OrderID + " " + e.EmployeeID,
                entryCount: 839);
        }

        [ConditionalFact]
        public virtual async Task Projection_when_arithmetic_mixed_subqueries()
        {
            await AssertQuery<Order, Employee>(
                (os, es) =>
                    from o in os.Select(o2 => new { o2, Mod = o2.OrderID % 2 })
                    from e in es.Select(e2 => new { e2, Square = e2.EmployeeID ^ 2 })
                    select new
                    {
                        Add = e.e2.EmployeeID + o.o2.OrderID,
                        e.Square,
                        e.e2,
                        Literal = 42,
                        o.o2,
                        o.Mod
                    },
                elementSorter: e => e.e2.EmployeeID + " " + e.o2.OrderID,
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual async Task Projection_when_null_value()
        {
            await AssertQuery<Customer>(
                cs => cs.Select(c => c.Region));
        }

        [ConditionalFact]
        public virtual async Task Take_with_single()
        {
            await AssertSingleResult<Customer, Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Take(1).SingleAsync(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Take_with_single_select_many()
        {
            await AssertSingleResult<Customer, Order>(
                (cs, os) =>
                    (from c in cs
                     from o in os
                     orderby c.CustomerID, o.OrderID
                     select new { c, o })
                    .Take(1)
                    .Cast<object>()
                    .SingleAsync(),
                entryCount: 2);
        }

        [ConditionalFact]
        public virtual async Task Cast_results_to_object()
        {
            await AssertQuery<Customer>(cs => from c in cs.Cast<object>() select c, entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task Where_simple()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == "London"),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual async Task Where_simple_closure()
        {
            // ReSharper disable once ConvertToConstant.Local
            var city = "London";

            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual async Task Where_simple_closure_constant()
        {
            // ReSharper disable once ConvertToConstant.Local
            var predicate = true;

            await AssertQuery<Customer>(
                cs => cs.Where(c => predicate),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task Where_simple_closure_via_query_cache()
        {
            var city = "London";

            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city),
                entryCount: 6);

            city = "Seattle";

            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city),
                entryCount: 1);
        }

        private class City
        {
            // ReSharper disable once StaticMemberInGenericType
            public static string StaticFieldValue;

            // ReSharper disable once StaticMemberInGenericType
            public static string StaticPropertyValue { get; set; }

            public string InstanceFieldValue;
            public string InstancePropertyValue { get; set; }

            public int Int { get; set; }
            public int? NullableInt { get; set; }

            public City Nested;

            public City Throw()
            {
                throw new NotImplementedException();
            }

            public string GetCity()
            {
                return InstanceFieldValue;
            }
        }

        [ConditionalFact]
        public virtual async Task Where_method_call_nullable_type_closure_via_query_cache()
        {
            var city = new City { Int = 2 };

            await AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == city.Int),
                entryCount: 5);

            city.Int = 5;

            await AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == city.Int),
                entryCount: 3);
        }

        [ConditionalFact]
        public virtual async Task Where_method_call_nullable_type_reverse_closure_via_query_cache()
        {
            var city = new City { NullableInt = 1 };

            await AssertQuery<Employee>(
                es => es.Where(e => e.EmployeeID > city.NullableInt),
                entryCount: 8);

            city.NullableInt = 5;

            await AssertQuery<Employee>(
                es => es.Where(e => e.EmployeeID > city.NullableInt),
                entryCount: 4);
        }

        [ConditionalFact]
        public virtual async Task Where_method_call_closure_via_query_cache()
        {
            var city = new City { InstanceFieldValue = "London" };

            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city.GetCity()),
                entryCount: 6);

            city.InstanceFieldValue = "Seattle";

            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city.GetCity()),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_field_access_closure_via_query_cache()
        {
            var city = new City { InstanceFieldValue = "London" };

            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city.InstanceFieldValue),
                entryCount: 6);

            city.InstanceFieldValue = "Seattle";

            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city.InstanceFieldValue),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_property_access_closure_via_query_cache()
        {
            var city = new City { InstancePropertyValue = "London" };

            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city.InstancePropertyValue),
                entryCount: 6);

            city.InstancePropertyValue = "Seattle";

            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city.InstancePropertyValue),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_static_field_access_closure_via_query_cache()
        {
            City.StaticFieldValue = "London";

            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == City.StaticFieldValue),
                entryCount: 6);

            City.StaticFieldValue = "Seattle";

            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == City.StaticFieldValue),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_static_property_access_closure_via_query_cache()
        {
            City.StaticPropertyValue = "London";

            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == City.StaticPropertyValue),
                entryCount: 6);

            City.StaticPropertyValue = "Seattle";

            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == City.StaticPropertyValue),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_nested_field_access_closure_via_query_cache()
        {
            var city = new City { Nested = new City { InstanceFieldValue = "London" } };

            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city.Nested.InstanceFieldValue),
                entryCount: 6);

            city.Nested.InstanceFieldValue = "Seattle";

            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city.Nested.InstanceFieldValue),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_nested_property_access_closure_via_query_cache()
        {
            var city = new City { Nested = new City { InstancePropertyValue = "London" } };

            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city.Nested.InstancePropertyValue),
                entryCount: 6);

            city.Nested.InstancePropertyValue = "Seattle";

            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city.Nested.InstancePropertyValue),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_nested_field_access_closure_via_query_cache_error_null()
        {
            var city = new City();

            using (var context = CreateContext())
            {
                await Assert.ThrowsAsync<InvalidOperationException>(
                    async () =>
                        await context.Set<Customer>()
                            .Where(c => c.City == city.Nested.InstanceFieldValue)
                            .ToListAsync());
            }
        }

        [ConditionalFact]
        public virtual async Task Where_nested_field_access_closure_via_query_cache_error_method_null()
        {
            var city = new City();

            using (var context = CreateContext())
            {
                await Assert.ThrowsAsync<InvalidOperationException>(
                    async () =>
                        await context.Set<Customer>()
                            .Where(c => c.City == city.Throw().InstanceFieldValue)
                            .ToListAsync());
            }
        }

        [ConditionalFact]
        public virtual async Task Where_new_instance_field_access_closure_via_query_cache()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == new City { InstanceFieldValue = "London" }.InstanceFieldValue),
                entryCount: 6);

            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == new City { InstanceFieldValue = "Seattle" }.InstanceFieldValue),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_simple_closure_via_query_cache_nullable_type()
        {
            int? reportsTo = 2;

            await AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == reportsTo),
                entryCount: 5);

            reportsTo = 5;

            await AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == reportsTo),
                entryCount: 3);

            reportsTo = null;

            await AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == reportsTo),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_simple_closure_via_query_cache_nullable_type_reverse()
        {
            int? reportsTo = null;

            await AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == reportsTo),
                entryCount: 1);

            reportsTo = 5;

            await AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == reportsTo),
                entryCount: 3);

            reportsTo = 2;

            await AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == reportsTo),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual async Task Where_simple_shadow()
        {
            await AssertQuery<Employee>(
                es => es.Where(e => EF.Property<string>(e, "Title") == "Sales Representative"),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual async Task Where_simple_shadow_projection()
        {
            await AssertQuery<Employee>(
                es => es.Where(e => EF.Property<string>(e, "Title") == "Sales Representative")
                    .Select(e => EF.Property<string>(e, "Title")));
        }

        [ConditionalFact]
        public virtual async Task Where_simple_shadow_projection_mixed()
        {
            await AssertQuery<Employee>(
                es => es.Where(e => EF.Property<string>(e, "Title") == "Sales Representative")
                    .Select(e => new { e, Title = EF.Property<string>(e, "Title") }),
                elementSorter: e => e.e.EmployeeID,
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual async Task Where_simple_shadow_subquery()
        {
            await AssertQuery<Employee>(
                es => from e in es.OrderBy(e => e.EmployeeID).Take(5)
                      where EF.Property<string>(e, "Title") == "Sales Representative"
                      select e,
                entryCount: 3);
        }

        [ConditionalFact]
        public virtual async Task Where_shadow_subquery_first()
        {
            await AssertQuery<Employee>(
                es =>
                    from e in es
                    where EF.Property<string>(e, "Title")
                          == EF.Property<string>(es.OrderBy(e2 => EF.Property<string>(e2, "Title")).First(), "Title")
                    select e,
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_client()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.IsLondon),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual async Task First_client_predicate()
        {
            await AssertSingleResult<Customer, Customer>(
                cs => cs.OrderBy(c => c.CustomerID).FirstAsync(c => c.IsLondon),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_equals_method_string()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City.Equals("London")),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual async Task Where_equals_method_int()
        {
            await AssertQuery<Employee>(
                es => es.Where(e => e.EmployeeID.Equals(1)),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_comparison_nullable_type_not_null()
        {
            await AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == 2),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual async Task Where_comparison_nullable_type_null()
        {
            await AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == null),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_string_length()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City.Length == 6),
                entryCount: 20);
        }

        [ConditionalFact]
        public virtual async Task Where_simple_reversed()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => "London" == c.City),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual async Task Where_is_null()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == null));
        }

        [ConditionalFact]
        public virtual async Task Where_null_is_null()
        {
            // ReSharper disable once EqualExpressionComparison
            await AssertQuery<Customer>(
                cs => cs.Where(c => null == null),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task Where_constant_is_null()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => "foo" == null));
        }

        [ConditionalFact]
        public virtual async Task Where_is_not_null()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City != null),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task Where_null_is_not_null()
        {
            // ReSharper disable once EqualExpressionComparison
            await AssertQuery<Customer>(
                cs => cs.Where(c => null != null));
        }

        [ConditionalFact]
        public virtual async Task Where_constant_is_not_null()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => "foo" != null),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task Where_identity_comparison()
        {
            // ReSharper disable once EqualExpressionComparison
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == c.City),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task Where_select_many_or()
        {
            await AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == "London"
                          || e.City == "London"
                    select new { c, e },
                elementSorter: e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 100);
        }

        [ConditionalFact]
        public virtual async Task Where_select_many_or2()
        {
            await AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == "London"
                          || c.City == "Berlin"
                    select new { c, e },
                elementSorter: e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 16);
        }

        [ConditionalFact]
        public virtual async Task Where_select_many_or3()
        {
            await AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == "London"
                          || c.City == "Berlin"
                          || c.City == "Seattle"
                    select new { c, e },
                elementSorter: e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 17);
        }

        [ConditionalFact]
        public virtual async Task Where_select_many_or4()
        {
            await AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == "London"
                          || c.City == "Berlin"
                          || c.City == "Seattle"
                          || c.City == "Lisboa"
                    select new { c, e },
                elementSorter: e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 19);
        }

        [ConditionalFact]
        public virtual async Task Where_select_many_or_with_parameter()
        {
            var london = "London";
            var lisboa = "Lisboa";

            await AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == london
                          || c.City == "Berlin"
                          || c.City == "Seattle"
                          || c.City == lisboa
                    select new { c, e },
                elementSorter: e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 19);
        }

        [ConditionalFact]
        public virtual async Task Where_in_optimization_multiple()
        {
            await AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == "London"
                          || c.City == "Berlin"
                          || c.CustomerID == "ALFKI"
                          || c.CustomerID == "ABCDE"
                    select new { c, e },
                elementSorter: e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 16);
        }

        [ConditionalFact]
        public virtual async Task Where_not_in_optimization1()
        {
            await AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City != "London"
                          && e.City != "London"
                    select new { c, e },
                elementSorter: e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 90);
        }

        [ConditionalFact]
        public virtual async Task Where_not_in_optimization2()
        {
            await AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City != "London"
                          && c.City != "Berlin"
                    select new { c, e },
                elementSorter: e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 93);
        }

        [ConditionalFact]
        public virtual async Task Where_not_in_optimization3()
        {
            await AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City != "London"
                          && c.City != "Berlin"
                          && c.City != "Seattle"
                    select new { c, e },
                elementSorter: e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 92);
        }

        [ConditionalFact]
        public virtual async Task Where_not_in_optimization4()
        {
            await AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City != "London"
                          && c.City != "Berlin"
                          && c.City != "Seattle"
                          && c.City != "Lisboa"
                    select new { c, e },
                elementSorter: e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 90);
        }

        [ConditionalFact]
        public virtual async Task Where_select_many_and()
        {
            await AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == "London" && c.Country == "UK"
                          && e.City == "London" && e.Country == "UK"
                    select new { c, e },
                elementSorter: e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 10);
        }

        [ConditionalFact]
        public virtual async Task Where_primitive()
        {
            await AssertQueryScalar<Employee>(
                es => es.Select(e => e.EmployeeID).Take(9).Where(i => i == 5));
        }

        [ConditionalFact]
        public virtual async Task Where_primitive_tracked()
        {
            await AssertQuery<Employee>(
                es => es.Take(9).Where(e => e.EmployeeID == 5),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_primitive_tracked2()
        {
            await AssertQuery<Employee>(
                es => es.Take(9).Select(e => new { e }).Where(e => e.e.EmployeeID == 5),
                entryCount: 1);
        }

        [ConditionalFact(Skip = "issue #8956")]
        public virtual async Task Where_subquery_anon()
        {
            await AssertQuery<Employee, Order>(
                (es, os) =>
                    from e in es.Take(3).Select(e => new { e })
                    from o in os.Take(5).Select(o => new { o })
                    where e.e.EmployeeID == o.o.EmployeeID
                    select new { e, o },
                elementSorter: e => e.e.e.EmployeeID + " " + e.o.o.OrderID,
                entryCount: 2);
        }

        [ConditionalFact]
        public virtual async Task Where_bool_member()
        {
            await AssertQuery<Product>(ps => ps.Where(p => p.Discontinued), entryCount: 8);
        }

        [ConditionalFact]
        public virtual async Task Where_bool_member_false()
        {
            await AssertQuery<Product>(ps => ps.Where(p => !p.Discontinued), entryCount: 69);
        }

        [ConditionalFact]
        public virtual async Task Where_bool_member_negated_twice()
        {
            // ReSharper disable once DoubleNegationOperator
            await AssertQuery<Product>(ps => ps.Where(p => !!p.Discontinued), entryCount: 8);
        }

        [ConditionalFact]
        public virtual async Task Where_bool_member_shadow()
        {
            await AssertQuery<Product>(ps => ps.Where(p => EF.Property<bool>(p, "Discontinued")), entryCount: 8);
        }

        [ConditionalFact]
        public virtual async Task Where_bool_member_false_shadow()
        {
            await AssertQuery<Product>(ps => ps.Where(p => !EF.Property<bool>(p, "Discontinued")), entryCount: 69);
        }

        [ConditionalFact]
        public virtual async Task Where_bool_member_equals_constant()
        {
            await AssertQuery<Product>(ps => ps.Where(p => p.Discontinued.Equals(true)), entryCount: 8);
        }

        [ConditionalFact]
        public virtual async Task Where_bool_member_in_complex_predicate()
        {
            // ReSharper disable once RedundantBoolCompare
            await AssertQuery<Product>(ps => ps.Where(p => p.ProductID > 100 && p.Discontinued || (p.Discontinued == true)), entryCount: 8);
        }

        [ConditionalFact]
        public virtual async Task Where_bool_member_compared_to_binary_expression()
        {
            await AssertQuery<Product>(ps => ps.Where(p => p.Discontinued == p.ProductID > 50), entryCount: 44);
        }

        [ConditionalFact]
        public virtual async Task Where_not_bool_member_compared_to_not_bool_member()
        {
            // ReSharper disable once EqualExpressionComparison
            await AssertQuery<Product>(ps => ps.Where(p => !p.Discontinued == !p.Discontinued), entryCount: 77);
        }

        [ConditionalFact]
        public virtual async Task Where_negated_boolean_expression_compared_to_another_negated_boolean_expression()
        {
            await AssertQuery<Product>(ps => ps.Where(p => !(p.ProductID > 50) == !(p.ProductID > 20)), entryCount: 47);
        }

        [ConditionalFact]
        public virtual async Task Where_not_bool_member_compared_to_binary_expression()
        {
            await AssertQuery<Product>(ps => ps.Where(p => !p.Discontinued == p.ProductID > 50), entryCount: 33);
        }

        [ConditionalFact]
        public virtual async Task Where_bool_parameter()
        {
            var prm = true;
            await AssertQuery<Product>(ps => ps.Where(p => prm), entryCount: 77);
        }

        [ConditionalFact]
        public virtual async Task Where_bool_parameter_compared_to_binary_expression()
        {
            var prm = true;
            await AssertQuery<Product>(ps => ps.Where(p => p.ProductID > 50 != prm), entryCount: 50);
        }

        [ConditionalFact]
        public virtual async Task Where_bool_member_and_parameter_compared_to_binary_expression_nested()
        {
            var prm = true;
            await AssertQuery<Product>(ps => ps.Where(p => p.Discontinued == (p.ProductID > 50 != prm)), entryCount: 33);
        }

        [ConditionalFact]
        public virtual async Task Where_de_morgan_or_optimizated()
        {
            await AssertQuery<Product>(ps => ps.Where(p => !(p.Discontinued || (p.ProductID < 20))), entryCount: 53);
        }

        [ConditionalFact]
        public virtual async Task Where_de_morgan_and_optimizated()
        {
            await AssertQuery<Product>(ps => ps.Where(p => !(p.Discontinued && (p.ProductID < 20))), entryCount: 74);
        }

        [ConditionalFact]
        public virtual async Task Where_complex_negated_expression_optimized()
        {
            await AssertQuery<Product>(ps => ps.Where(p => !(!(!p.Discontinued && (p.ProductID < 60)) || !(p.ProductID > 30))), entryCount: 27);
        }

        [ConditionalFact]
        public virtual async Task Where_short_member_comparison()
        {
            await AssertQuery<Product>(ps => ps.Where(p => p.UnitsInStock > 10), entryCount: 63);
        }

        [ConditionalFact]
        public virtual async Task Where_true()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => true),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task Where_false()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => false));
        }

        [ConditionalFact]
        public virtual async Task Where_bool_closure()
        {
            var boolean = false;

            await AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID == "ALFKI" && boolean));

            boolean = true;

            await AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID == "ALFKI" && boolean),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Select_bool_closure()
        {
            var boolean = false;

            await AssertQuery<Customer>(
                cs => cs.Select(c => new { f = boolean }),
                assertOrder: true);

            boolean = true;

            await AssertQuery<Customer>(
                cs => cs.Select(c => new { f = boolean }),
                assertOrder: true);
        }

        // TODO: Re-write entity ref equality to identity equality.
        //
        // [ConditionalFact]
        // public virtual async Task Where_compare_entity_equal()
        // {
        //     var alfki = NorthwindData.Customers.SingleAsync(c => c.CustomerID == "ALFKI");
        //
        //     Assert.Equal(1,
        //         // ReSharper disable once PossibleUnintendedReferenceComparison
        //         AssertQuery<Customer>(cs => cs.Where(c => c == alfki)));
        // }
        //
        // [ConditionalFact]
        // public virtual async Task Where_compare_entity_not_equal()
        // {
        //     var alfki = new Customer { CustomerID = "ALFKI" };
        //
        //     Assert.Equal(90,
        //         // ReSharper disable once PossibleUnintendedReferenceComparison
        //         AssertQuery<Customer>(cs => cs.Where(c => c != alfki)));
        //
        // [ConditionalFact]
        // public virtual async Task Project_compare_entity_equal()
        // {
        //     var alfki = NorthwindData.Customers.SingleAsync(c => c.CustomerID == "ALFKI");
        //
        //     Assert.Equal(1,
        //         // ReSharper disable once PossibleUnintendedReferenceComparison
        //         AssertQuery<Customer>(cs => cs.Select(c => c == alfki)));
        // }
        //
        // [ConditionalFact]
        // public virtual async Task Project_compare_entity_not_equal()
        // {
        //     var alfki = new Customer { CustomerID = "ALFKI" };
        //
        //     Assert.Equal(90,
        //         // ReSharper disable once PossibleUnintendedReferenceComparison
        //         AssertQuery<Customer>(cs => cs.Select(c => c != alfki)));
        // }

        [ConditionalFact]
        public virtual async Task Where_compare_constructed_equal()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => new { x = c.City } == new { x = "London" }));
        }

        [ConditionalFact]
        public virtual async Task Where_compare_constructed_multi_value_equal()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => new { x = c.City, y = c.Country } == new { x = "London", y = "UK" }));
        }

        [ConditionalFact]
        public virtual async Task Where_compare_constructed_multi_value_not_equal()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => new { x = c.City, y = c.Country } != new { x = "London", y = "UK" }),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task Where_compare_constructed()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => new { x = c.City } == new { x = "London" }));
        }

        [ConditionalFact]
        public virtual async Task Where_compare_null()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == null && c.Country == "UK"));
        }

        [ConditionalFact]
        public virtual async Task Where_projection()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == "London").Select(c => c.CompanyName));
        }

        [ConditionalFact]
        public virtual async Task Select_scalar()
        {
            await AssertQuery<Customer>(
                cs => cs.Select(c => c.City));
        }

        [ConditionalFact]
        public virtual async Task Select_anonymous_one()
        {
            await AssertQuery<Customer>(
                cs => cs.Select(c => new { c.City }),
                elementSorter: e => e.City);
        }

        [ConditionalFact]
        public virtual async Task Select_anonymous_two()
        {
            await AssertQuery<Customer>(
                cs => cs.Select(c => new { c.City, c.Phone }),
                elementSorter: e => e.City + " " + e.Phone);
        }

        [ConditionalFact]
        public virtual async Task Select_anonymous_three()
        {
            await AssertQuery<Customer>(
                cs => cs.Select(c => new { c.City, c.Phone, c.Country }),
                elementSorter: e => e.City + " " + e.Phone + " " + e.Country);
        }

        [ConditionalFact]
        public virtual async Task Select_anonymous_conditional_expression()
        {
            await AssertQuery<Product>(
                ps => ps.Select(p => new { p.ProductID, IsAvailable = p.UnitsInStock > 0 }),
                elementSorter: e => e.ProductID);
        }

        [ConditionalFact]
        public virtual async Task Select_customer_table()
        {
            await AssertQuery<Customer>(
                cs => cs,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task Select_customer_identity()
        {
            await AssertQuery<Customer>(
                cs => cs.Select(c => c),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task Select_anonymous_with_object()
        {
            await AssertQuery<Customer>(
                cs => cs.Select(c => new { c.City, c }),
                elementSorter: e => e.c.CustomerID,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task Select_anonymous_nested()
        {
            await AssertQuery<Customer>(
                cs => cs.Select(c => new { c.City, Country = new { c.Country } }),
                elementSorter: e => e.City);
        }

        [ConditionalFact]
        public virtual async Task Select_anonymous_empty()
        {
            await AssertQuery<Customer>(
                cs => cs.Select(c => new { }),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual async Task Select_anonymous_literal()
        {
            await AssertQuery<Customer>(
                cs => cs.Select(c => new { X = 10 }),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual async Task Select_constant_int()
        {
            await AssertQueryScalar<Customer>(cs => cs.Select(c => 0));
        }

        [ConditionalFact]
        public virtual async Task Select_constant_null_string()
        {
            await AssertQuery<Customer>(cs => cs.Select(c => (string)null));
        }

        [ConditionalFact]
        public virtual async Task Select_local()
        {
            // ReSharper disable once ConvertToConstant.Local
            var x = 10;

            await AssertQueryScalar<Customer>(cs => cs.Select(c => x));
        }

        [ConditionalFact]
        public virtual async Task Select_scalar_primitive()
        {
            await AssertQueryScalar<Employee>(
                es => es.Select(e => e.EmployeeID));
        }

        [ConditionalFact]
        public virtual async Task Select_scalar_primitive_after_take()
        {
            await AssertQueryScalar<Employee>(
                es => es.Take(9).Select(e => e.EmployeeID));
        }

        [ConditionalFact]
        public virtual async Task Select_project_filter()
        {
            await AssertQuery<Customer>(
                cs =>
                    from c in cs
                    where c.City == "London"
                    select c.CompanyName);
        }

        [ConditionalFact]
        public virtual async Task Select_project_filter2()
        {
            await AssertQuery<Customer>(
                cs =>
                    from c in cs
                    where c.City == "London"
                    select c.City);
        }

        [ConditionalFact]
        public virtual async Task Select_nested_collection()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    where c.City == "London"
                    orderby c.CustomerID
                    select os
                        .Where(
                            o => o.CustomerID == c.CustomerID
                                 && o.OrderDate.Value.Year == 1997)
                        .Select(o => o.OrderID)
                        .OrderBy(o => o),
                e => ((IEnumerable<int>)e).Count(),
                elementAsserter: (e, a) => CollectionAsserter<int>(i => i));
        }

        [ConditionalFact]
        public virtual async Task Select_correlated_subquery_projection()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs.Take(3)
                    orderby c.CustomerID
                    select os.Where(o => o.CustomerID == c.CustomerID),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Order>(o => o.OrderID));
        }

        [ConditionalFact]
        public virtual async Task Select_correlated_subquery_filtered()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    where c.CustomerID.StartsWith("A")
                    orderby c.CustomerID
                    select os.Where(o => o.CustomerID == c.CustomerID),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Order>(o => o.OrderID));
        }

        [ConditionalFact]
        public virtual async Task Select_correlated_subquery_ordered()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    select os.OrderBy(o => c.CustomerID),
                elementSorter: CollectionSorter<Order>(),
                elementAsserter: CollectionAsserter<Order>());
        }

        // TODO: Re-linq parser
        // [ConditionalFact]
        // public virtual async Task Select_nested_ordered_enumerable_collection()
        // {
        //     AssertQuery<Customer>(cs =>
        //         cs.Select(c => cs.AsEnumerable().OrderBy(c2 => c2.CustomerID)),
        //         assertOrder: true);
        // }

        [ConditionalFact]
        public virtual async Task Select_nested_collection_in_anonymous_type()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    where c.CustomerID == "ALFKI"
                    select new
                    {
                        CustomerId = c.CustomerID,
                        OrderIds
                        = os.Where(
                                o => o.CustomerID == c.CustomerID
                                     && o.OrderDate.Value.Year == 1997)
                            .Select(o => o.OrderID)
                            .OrderBy(o => o),
                        Customer = c
                    },
                elementAsserter: (e, a) =>
                    {
                        Assert.Equal(e.CustomerId, a.CustomerId);
                        Assert.Equal((IEnumerable<int>)e.OrderIds, (IEnumerable<int>)a.OrderIds);
                        Assert.Equal(e.Customer, a.Customer);
                    },
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Select_subquery_recursive_trivial()
        {
            await AssertQuery<Employee>(
                es =>
                    from e1 in es
                    select (from e2 in es
                            select (from e3 in es
                                    orderby e3.EmployeeID
                                    select e3)),
                e => ((IEnumerable<IEnumerable<Employee>>)e).Count(),
                elementAsserter: (e, a) =>
                    {
                        var expected = ((IEnumerable<IEnumerable<Employee>>)e).SelectMany(i => i).ToList();
                        var actual = ((IEnumerable<IEnumerable<Employee>>)e).SelectMany(i => i).ToList();

                        Assert.Equal(expected, actual);
                    });
        }

        [ConditionalFact]
        public virtual async Task Where_subquery_on_collection()
        {
            await AssertQuery<Product, OrderDetail>(
                (pr, od) =>
                    pr.Where(
                        p => od
                            .Where(o => o.ProductID == p.ProductID)
                            .Select(odd => odd.Quantity).Contains<short>(5)),
                entryCount: 43);
        }

        [ConditionalFact]
        public virtual async Task Where_query_composition()
        {
            await AssertQuery<Employee>(
                es =>
                    from e1 in es
                    where e1.FirstName == es.OrderBy(e => e.EmployeeID).First().FirstName
                    select e1,
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_subquery_recursive_trivial()
        {
            await AssertQuery<Employee>(
                es =>
                    from e1 in es
                    where (from e2 in es
                           where (from e3 in es
                                  orderby e3.EmployeeID
                                  select e3).Any()
                           select e2).Any()
                    orderby e1.EmployeeID
                    select e1,
                assertOrder: true,
                entryCount: 9);
        }

        [ConditionalFact]
        public virtual async Task Select_nested_collection_deep()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    where c.City == "London"
                    orderby c.CustomerID
                    select (from o1 in os
                            where o1.CustomerID == c.CustomerID
                                  && o1.OrderDate.Value.Year == 1997
                            orderby o1.OrderID
                            select (from o2 in os
                                    where o1.CustomerID == c.CustomerID
                                    orderby o2.OrderID
                                    select o1.OrderID)),
                assertOrder: true,
                elementAsserter: (e, a) =>
                    {
                        var expected = ((IEnumerable<IEnumerable<int>>)e).SelectMany(i => i).ToList();
                        var actual = ((IEnumerable<IEnumerable<int>>)e).SelectMany(i => i).ToList();

                        Assert.Equal(expected, actual);
                    });
        }

        [ConditionalFact]
        public virtual async Task OrderBy_scalar_primitive()
        {
            await AssertQueryScalar<Employee>(
                es => es.Select(e => e.EmployeeID).OrderBy(i => i),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual async Task SelectMany_mixed()
        {
            await AssertQuery<Employee, Customer>(
                (es, cs) => from e1 in es
                            from s in new[] { "a", "b" }
                            from c in cs
                            select new { e1, s, c },
                elementSorter: e => e.e1.EmployeeID + " " + e.s + " " + e.c.CustomerID,
                entryCount: 100);
        }

        [ConditionalFact]
        public virtual async Task SelectMany_simple1()
        {
            await AssertQuery<Employee, Customer>(
                (es, cs) => from e in es
                            from c in cs
                            select new { c, e },
                elementSorter: e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 100);
        }

        [ConditionalFact]
        public virtual async Task SelectMany_simple2()
        {
            await AssertQuery<Employee, Customer>(
                (es, cs) => from e1 in es
                            from c in cs
                            from e2 in es
                            select new { e1, c, e2.FirstName },
                elementSorter: e => e.e1.EmployeeID + " " + e.c.CustomerID + " " + e.FirstName,
                entryCount: 100);
        }

        [ConditionalFact]
        public virtual async Task SelectMany_entity_deep()
        {
            await AssertQuery<Employee>(
                es =>
                    from e1 in es
                    from e2 in es
                    from e3 in es
                    select new { e2, e3, e1 },
                elementSorter: e => e.e2.EmployeeID + " " + e.e3.EmployeeID + " " + e.e1.EmployeeID,
                entryCount: 9);
        }

        [ConditionalFact]
        public virtual async Task SelectMany_projection1()
        {
            await AssertQuery<Employee>(
                es => from e1 in es
                      from e2 in es
                      select new { e1.City, e2.Country },
                elementSorter: e => e.City + " " + e.Country);
        }

        [ConditionalFact]
        public virtual async Task SelectMany_projection2()
        {
            await AssertQuery<Employee>(
                es => from e1 in es
                      from e2 in es
                      from e3 in es
                      select new { e1.City, e2.Country, e3.FirstName },
                elementSorter: e => e.City + " " + e.Country + " " + e.FirstName);
        }

        [ConditionalFact]
        public virtual async Task SelectMany_nested_simple()
        {
            await AssertQuery<Customer>(
                cs =>
                    from c in cs
                    from c1 in
                        (from c2 in (from c3 in cs select c3) select c2)
                    orderby c1.CustomerID
                    select c1,
                assertOrder: true,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task SelectMany_correlated_simple()
        {
            await AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == e.City
                    orderby c.CustomerID, e.EmployeeID
                    select new { c, e },
                assertOrder: true,
                entryCount: 15);
        }

        [ConditionalFact]
        public virtual async Task SelectMany_correlated_subquery_simple()
        {
            await AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es.Where(e => e.City == c.City)
                    orderby c.CustomerID, e.EmployeeID
                    select new { c, e },
                assertOrder: true,
                entryCount: 15);
        }

        [ConditionalFact]
        public virtual async Task SelectMany_correlated_subquery_hard()
        {
            await AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c1 in
                        (from c2 in cs.Take(91) select c2.City).Distinct()
                    from e1 in
                        (from e2 in es where c1 == e2.City select new { e2.City, c1 }).Take(9)
                    from e2 in
                        (from e3 in es where e1.City == e3.City select c1).Take(9)
                    select new { c1, e1 },
                elementSorter: e => e.c1 + " " + e.e1.City + " " + e.e1.c1);
        }

        [ConditionalFact]
        public virtual async Task SelectMany_cartesian_product_with_ordering()
        {
            await AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == e.City
                    orderby e.City, c.CustomerID descending
                    select new { c, e.City },
                assertOrder: true,
                entryCount: 8);
        }

        [ConditionalFact]
        public virtual async Task SelectMany_primitive()
        {
            await AssertQueryScalar<Employee>(
                es =>
                    from e1 in es
                    from i in es.Select(e2 => e2.EmployeeID)
                    select i);
        }

        [ConditionalFact]
        public virtual async Task SelectMany_primitive_select_subquery()
        {
            await AssertQueryScalar<Employee>(
                es =>
                    from e1 in es
                    from i in es.Select(e2 => e2.EmployeeID)
                    select es.Any());
        }

        [ConditionalFact]
        public virtual async Task Join_customers_orders_projection()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o in os on c.CustomerID equals o.CustomerID
                    select new { c.ContactName, o.OrderID },
                elementSorter: e => e.ContactName + " " + e.OrderID);
        }

        [ConditionalFact]
        public virtual async Task Join_customers_orders_entities()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o in os on c.CustomerID equals o.CustomerID
                    select new { c, o },
                elementSorter: e => e.c.CustomerID + " " + e.o.OrderID,
                elementAsserter: (e, a) =>
                    {
                        Assert.Equal(e.c.CustomerID, a.c.CustomerID);
                        Assert.Equal(e.o.OrderID, a.o.OrderID);
                    },
                entryCount: 919);
        }

        [ConditionalFact]
        public virtual async Task Join_select_many()
        {
            await AssertQuery<Customer, Order, Employee>(
                (cs, os, es) =>
                    from c in cs
                    join o in os on c.CustomerID equals o.CustomerID
                    from e in es
                    select new { c, o, e },
                elementSorter: e => e.c.CustomerID + " " + e.o.OrderID + " " + e.e.EmployeeID,
                entryCount: 928);
        }

        [ConditionalFact]
        public virtual async Task Join_customers_orders_select()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o in os on c.CustomerID equals o.CustomerID
                    select new { c.ContactName, o.OrderID }
                    into p
                    select p,
                elementSorter: e => e.ContactName + " " + e.OrderID);
        }

        [ConditionalFact]
        public virtual async Task Join_customers_orders_with_subquery()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o1 in
                        (from o2 in os orderby o2.OrderID select o2) on c.CustomerID equals o1.CustomerID
                    where o1.CustomerID == "ALFKI"
                    select new { c.ContactName, o1.OrderID },
                elementSorter: e => e.ContactName + " " + e.OrderID);
        }

        [ConditionalFact]
        public virtual async Task Join_customers_orders_with_subquery_anonymous_property_method()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o1 in
                        (from o2 in os orderby o2.OrderID select new { o2 }) on c.CustomerID equals o1.o2.CustomerID
                    where EF.Property<string>(o1.o2, "CustomerID") == "ALFKI"
                    select new { o1, o1.o2, Shadow = EF.Property<DateTime?>(o1.o2, "OrderDate") },
                e => e.o1.o2.OrderID);
        }

        [ConditionalFact]
        public virtual async Task Join_customers_orders_with_subquery_predicate()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o1 in
                        (from o2 in os where o2.OrderID > 0 orderby o2.OrderID select o2) on c.CustomerID equals o1.CustomerID
                    where o1.CustomerID == "ALFKI"
                    select new { c.ContactName, o1.OrderID },
                elementSorter: e => e.ContactName + " " + e.OrderID);
        }

        [ConditionalFact]
        public virtual async Task Join_composite_key()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o in os on new { a = c.CustomerID, b = c.CustomerID }
                        equals new { a = o.CustomerID, b = o.CustomerID }
                    select new { c, o },
                elementSorter: e => e.c.CustomerID + " " + e.o.OrderID,
                entryCount: 919);
        }

        [ConditionalFact]
        public virtual async Task Join_client_new_expression()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o in os on new Foo { Bar = c.CustomerID } equals new Foo { Bar = o.CustomerID }
                    select new { c, o });
        }

        [ConditionalFact]
        public virtual async Task Join_Where_Count()
        {
            await AssertSingleResult<Customer, Order>(
                (cs, os) =>
                    (from c in cs
                     join o in os on c.CustomerID equals o.CustomerID
                     where c.CustomerID == "ALFKI"
                     select c).CountAsync());
        }

        [ConditionalFact]
        public virtual async Task Multiple_joins_Where_Order_Any()
        {
            await AssertSingleResult<Customer, Order, OrderDetail>(
                (cs, os, ods) =>
                    cs.Join(os, c => c.CustomerID, o => o.CustomerID, (cr, or) => new { cr, or })
                        .Join(ods, e => e.or.OrderID, od => od.OrderID, (e, od) => new { e.cr, e.or, od })
                        .Where(r => r.cr.City == "London").OrderBy(r => r.cr.CustomerID)
                        .AnyAsync());
        }

        [ConditionalFact]
        public virtual async Task Join_OrderBy_Count()
        {
            await AssertSingleResult<Customer, Order>(
                (cs, os) =>
                    (from c in cs
                     join o in os on c.CustomerID equals o.CustomerID
                     orderby c.CustomerID
                     select c).CountAsync());
        }

        private class Foo
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string Bar { get; set; }
        }

        [ConditionalFact]
        public virtual async Task GroupJoin_customers_orders()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o in os.OrderBy(o => o.OrderID) on c.CustomerID equals o.CustomerID into orders
                    select new { customer = c, orders = orders.ToList() },
                e => e.customer.CustomerID,
                elementAsserter: (e, a) =>
                    {
                        Assert.Equal(e.customer.CustomerID, a.customer.CustomerID);
                        CollectionAsserter<Order>(o => o.OrderID)(e.orders, a.orders);
                    },
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task GroupJoin_customers_orders_count()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o in os on c.CustomerID equals o.CustomerID into orders
                    select new { cust = c, ords = orders.Count() },
                elementSorter: e => e.cust.CustomerID,
                entryCount: 91);
        }

#if Test20
        private const int NonExistentID = -1;
#else
        private const uint NonExistentID = uint.MaxValue;
#endif

        [ConditionalFact]
        public virtual async Task Default_if_empty_top_level()
        {
            await AssertQuery<Employee>(
                es =>
                    from e in es.Where(c => c.EmployeeID == NonExistentID).DefaultIfEmpty()
                    select e);
        }

        [ConditionalFact]
        public virtual async Task Default_if_empty_top_level_arg()
        {
            await AssertQuery<Employee>(
                es =>
                    from e in es.Where(c => c.EmployeeID == NonExistentID).DefaultIfEmpty(new Employee())
                    select e,
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Default_if_empty_top_level_positive()
        {
            await AssertQuery<Employee>(
                es =>
                    from e in es.Where(c => c.EmployeeID > 0).DefaultIfEmpty()
                    select e,
                entryCount: 9);
        }

        [ConditionalFact]
        public virtual async Task Default_if_empty_top_level_projection()
        {
            await AssertQueryScalar<Employee>(
                es =>
                    from e in es.Where(e => e.EmployeeID == NonExistentID).Select(e => e.EmployeeID).DefaultIfEmpty()
                    select e);
        }

        [ConditionalFact]
        public virtual async Task GroupJoin_customers_employees_shadow()
        {
            await AssertQuery<Customer, Employee>(
                (cs, es) =>
                    (from c in cs
                     join e in es on c.City equals e.City into employees
                     select employees)
                    .SelectMany(emps => emps)
                    .Select(
                        e =>
                            new
                            {
                                Title = EF.Property<string>(e, "Title"),
                                Id = e.EmployeeID
                            }),
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
        public virtual async Task GroupJoin_customers_employees_subquery_shadow()
        {
            await AssertQuery<Customer, Employee>(
                (cs, es) =>
                    (from c in cs
                     join e in es.OrderBy(e => e.City) on c.City equals e.City into employees
                     select employees)
                    .SelectMany(emps => emps)
                    .Select(
                        e =>
                            new
                            {
                                Title = EF.Property<string>(e, "Title"),
                                Id = e.EmployeeID
                            }),
                elementSorter: e => e.Title + " " + e.Id);
        }

        [ConditionalFact]
        public virtual async Task SelectMany_customer_orders()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    from o in os
                    where c.CustomerID == o.CustomerID
                    select new { c.ContactName, o.OrderID },
                elementSorter: e => e.ContactName + " " + e.OrderID);
        }

        [ConditionalFact]
        public virtual async Task SelectMany_Count()
        {
            await AssertSingleResult<Customer, Order>(
                (cs, os) =>
                    (from c in cs
                     from o in os
                     select c.CustomerID).CountAsync());
        }

        [ConditionalFact]
        public virtual async Task SelectMany_LongCount()
        {
            await AssertSingleResult<Customer, Order>(
                (cs, os) =>
                    (from c in cs
                     from o in os
                     select c.CustomerID).LongCountAsync());
        }

        [ConditionalFact]
        public virtual async Task SelectMany_OrderBy_ThenBy_Any()
        {
            await AssertSingleResult<Customer, Order>(
                (cs, os) =>
                    (from c in cs
                     from o in os
                     orderby c.CustomerID, c.City
                     select c).AnyAsync());
        }

        // TODO: Composite keys, slow..

        //        [ConditionalFact]
        //        public virtual async Task Multiple_joins_with_join_conditions_in_where()
        //        {
        //            AssertQuery<Customer, Order, OrderDetail>((cs, os, ods) =>
        //                from c in cs
        //                from o in os.OrderBy(o1 => o1.OrderID).Take(10)
        //                from od in ods
        //                where o.CustomerID == c.CustomerID
        //                    && o.OrderID == od.OrderID
        //                where c.CustomerID == "ALFKI"
        //                select od.ProductID,
        //                assertOrder: true);
        //        }
        //        [ConditionalFact]
        //
        //        public virtual async Task TestMultipleJoinsWithMissingJoinCondition()
        //        {
        //            AssertQuery<Customer, Order, OrderDetail>((cs, os, ods) =>
        //                from c in cs
        //                from o in os
        //                from od in ods
        //                where o.CustomerID == c.CustomerID
        //                where c.CustomerID == "ALFKI"
        //                select od.ProductID
        //                );
        //        }

        [ConditionalFact]
        public virtual async Task OrderBy()
        {
            await AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID),
                assertOrder: true,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task OrderBy_client_mixed()
        {
            await AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.IsLondon).ThenBy(c => c.CompanyName),
                assertOrder: true,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task OrderBy_multiple_queries()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o in os on new Foo { Bar = c.CustomerID } equals new Foo { Bar = o.CustomerID }
                    orderby c.IsLondon, o.OrderDate
                    select new { c, o });
        }

        [ConditionalFact]
        public virtual async Task OrderBy_shadow()
        {
            await AssertQuery<Employee>(
                es => es.OrderBy(e => EF.Property<string>(e, "Title")).ThenBy(e => e.EmployeeID),
                assertOrder: true,
                entryCount: 9);
        }

        [ConditionalFact]
        public virtual async Task OrderBy_ThenBy_predicate()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == "London")
                    .OrderBy(c => c.City)
                    .ThenBy(c => c.CustomerID),
                assertOrder: true,
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual async Task OrderBy_correlated_subquery_lol()
        {
            await AssertQuery<Customer>(
                cs => from c in cs
                      orderby cs.Any(c2 => c2.CustomerID == c.CustomerID)
                      select c,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task OrderBy_Select()
        {
            await AssertQuery<Customer>(
                cs =>
                    cs.OrderBy(c => c.CustomerID)
                        .Select(c => c.ContactName),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual async Task OrderBy_multiple()
        {
            await AssertQuery<Customer>(
                cs =>
                    cs.OrderBy(c => c.CustomerID)
                        // ReSharper disable once MultipleOrderBy
                        .OrderBy(c => c.Country)
                        .Select(c => c.City),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual async Task OrderBy_ThenBy()
        {
            await AssertQuery<Customer>(
                cs =>
                    cs.OrderBy(c => c.CustomerID).ThenBy(c => c.Country).Select(c => c.City),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual async Task OrderByDescending()
        {
            await AssertQuery<Customer>(
                cs =>
                    cs.OrderByDescending(c => c.CustomerID).Select(c => c.City),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual async Task OrderByDescending_ThenBy()
        {
            await AssertQuery<Customer>(
                cs =>
                    cs.OrderByDescending(c => c.CustomerID).ThenBy(c => c.Country).Select(c => c.City),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual async Task OrderByDescending_ThenByDescending()
        {
            await AssertQuery<Customer>(
                cs =>
                    cs.OrderByDescending(c => c.CustomerID).ThenByDescending(c => c.Country).Select(c => c.City),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual async Task OrderBy_ThenBy_Any()
        {
            await AssertSingleResult<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).ThenBy(c => c.ContactName).AnyAsync());
        }

        [ConditionalFact]
        public virtual async Task OrderBy_Join()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs.OrderBy(c => c.CustomerID)
                    join o in os.OrderBy(o => o.OrderID) on c.CustomerID equals o.CustomerID
                    select new { c.CustomerID, o.OrderID },
                elementSorter: e => e.CustomerID + " " + e.OrderID);
        }

        [ConditionalFact]
        public virtual async Task OrderBy_SelectMany()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs.OrderBy(c => c.CustomerID)
                    from o in os.OrderBy(o => o.OrderID)
                    where c.CustomerID == o.CustomerID
                    select new { c.ContactName, o.OrderID },
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual async Task Sum_with_no_arg()
        {
            await AssertSingleResult<Order>(os => os.Select(o => o.OrderID).SumAsync());
        }

        [ConditionalFact]
        public virtual async Task Sum_with_binary_expression()
        {
            await AssertSingleResult<Order>(os => os.Select(o => o.OrderID * 2).SumAsync());
        }

        [ConditionalFact]
        public virtual async Task Sum_with_no_arg_empty()
        {
            await AssertSingleResult<Order>(os => os.Where(o => o.OrderID == 42).Select(o => o.OrderID).SumAsync());
        }

        [ConditionalFact]
        public virtual async Task Sum_with_arg()
        {
            await AssertSingleResult<Order>(os => os.SumAsync(o => o.OrderID));
        }

        [ConditionalFact]
        public virtual async Task Sum_with_arg_expression()
        {
            await AssertSingleResult<Order>(os => os.SumAsync(o => o.OrderID + o.OrderID));
        }

        [ConditionalFact]
        public virtual async Task Sum_with_coalesce()
        {
            await AssertSingleResult<Product>(ps => ps.Where(p => p.ProductID < 40).SumAsync(p => p.UnitPrice ?? 0));
        }

        [ConditionalFact]
        public virtual async Task Sum_over_subquery_is_client_eval()
        {
            await AssertSingleResult<Customer>(cs => cs.SumAsync(c => c.Orders.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task Average_with_no_arg()
        {
            await AssertSingleResult<Order>(os => os.Select(o => o.OrderID).AverageAsync());
        }

        [ConditionalFact]
        public virtual async Task Average_with_binary_expression()
        {
            await AssertSingleResult<Order>(os => os.Select(o => o.OrderID * 2).AverageAsync());
        }

        [ConditionalFact]
        public virtual async Task Average_with_arg()
        {
            await AssertSingleResult<Order>(os => os.AverageAsync(o => o.OrderID));
        }

        [ConditionalFact]
        public virtual async Task Average_with_arg_expression()
        {
            await AssertSingleResult<Order>(os => os.AverageAsync(o => o.OrderID + o.OrderID));
        }

        [ConditionalFact]
        public virtual async Task Average_with_coalesce()
        {
            await AssertSingleResult<Product>(
                ps => ps.Where(p => p.ProductID < 40).AverageAsync(p => p.UnitPrice ?? 0),
                asserter: (e, a) => Assert.InRange((decimal)e - (decimal)a, -0.1m, 0.1m));
        }

        [ConditionalFact]
        public virtual async Task Average_over_subquery_is_client_eval()
        {
            await AssertSingleResult<Customer>(cs => cs.AverageAsync(c => c.Orders.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task Min_with_no_arg()
        {
            await AssertSingleResult<Order>(os => os.Select(o => o.OrderID).MinAsync());
        }

        [ConditionalFact]
        public virtual async Task Min_with_arg()
        {
            await AssertSingleResult<Order>(os => os.MinAsync(o => o.OrderID));
        }

        [ConditionalFact]
        public virtual async Task Min_with_coalesce()
        {
            await AssertSingleResult<Product>(ps => ps.Where(p => p.ProductID < 40).MinAsync(p => p.UnitPrice ?? 0));
        }

        [ConditionalFact]
        public virtual async Task Min_over_subquery_is_client_eval()
        {
            await AssertSingleResult<Customer>(cs => cs.MinAsync(c => c.Orders.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task Max_with_no_arg()
        {
            await AssertSingleResult<Order>(os => os.Select(o => o.OrderID).MaxAsync());
        }

        [ConditionalFact]
        public virtual async Task Max_with_arg()
        {
            await AssertSingleResult<Order>(os => os.MaxAsync(o => o.OrderID));
        }

        [ConditionalFact]
        public virtual async Task Max_with_coalesce()
        {
            await AssertSingleResult<Product>(ps => ps.Where(p => p.ProductID < 40).MaxAsync(p => p.UnitPrice ?? 0));
        }

        [ConditionalFact]
        public virtual async Task Max_over_subquery_is_client_eval()
        {
            await AssertSingleResult<Customer>(cs => cs.MaxAsync(c => c.Orders.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual async Task Count_with_no_predicate()
        {
            await AssertSingleResult<Order>(os => os.CountAsync());
        }

        [ConditionalFact]
        public virtual async Task Count_with_predicate()
        {
            await AssertSingleResult<Order>(os => os.CountAsync(o => o.CustomerID == "ALFKI"));
        }

        [ConditionalFact]
        public virtual async Task Count_with_order_by()
        {
            await AssertSingleResult<Order>(os => os.OrderBy(o => o.CustomerID).CountAsync());
        }

        [ConditionalFact]
        public virtual async Task Where_OrderBy_Count()
        {
            await AssertSingleResult<Order>(os => os.Where(o => o.CustomerID == "ALFKI").OrderBy(o => o.OrderID).CountAsync());
        }

        [ConditionalFact]
        public virtual async Task OrderBy_Where_Count()
        {
            await AssertSingleResult<Order>(os => os.OrderBy(o => o.OrderID).Where(o => o.CustomerID == "ALFKI").CountAsync());
        }

        [ConditionalFact]
        public virtual async Task OrderBy_Count_with_predicate()
        {
            await AssertSingleResult<Order>(os => os.OrderBy(o => o.OrderID).CountAsync(o => o.CustomerID == "ALFKI"));
        }

        [ConditionalFact]
        public virtual async Task OrderBy_Where_Count_with_predicate()
        {
            await AssertSingleResult<Order>(os => os.OrderBy(o => o.OrderID).Where(o => o.OrderID > 10).CountAsync(o => o.CustomerID != "ALFKI"));
        }

        [ConditionalFact]
        public virtual async Task Where_OrderBy_Count_client_eval()
        {
            await AssertSingleResult<Order>(os => os.Where(o => ClientEvalPredicate(o)).OrderBy(o => ClientEvalSelectorStateless()).CountAsync());
        }

        [ConditionalFact]
        public virtual async Task Where_OrderBy_Count_client_eval_mixed()
        {
            await AssertSingleResult<Order>(os => os.Where(o => o.OrderID > 10).OrderBy(o => ClientEvalPredicate(o)).CountAsync());
        }

        [ConditionalFact]
        public virtual async Task OrderBy_Where_Count_client_eval()
        {
            await AssertSingleResult<Order>(os => os.OrderBy(o => ClientEvalSelectorStateless()).Where(o => ClientEvalPredicate(o)).CountAsync());
        }

        [ConditionalFact]
        public virtual async Task OrderBy_Where_Count_client_eval_mixed()
        {
            await AssertSingleResult<Order>(os => os.OrderBy(o => o.OrderID).Where(o => ClientEvalPredicate(o)).CountAsync());
        }

        [ConditionalFact]
        public virtual async Task OrderBy_Count_with_predicate_client_eval()
        {
            await AssertSingleResult<Order>(os => os.OrderBy(o => ClientEvalSelectorStateless()).CountAsync(o => ClientEvalPredicate(o)));
        }

        [ConditionalFact]
        public virtual async Task OrderBy_Count_with_predicate_client_eval_mixed()
        {
            await AssertSingleResult<Order>(os => os.OrderBy(o => o.OrderID).CountAsync(o => ClientEvalPredicateStateless()));
        }

        [ConditionalFact]
        public virtual async Task OrderBy_Where_Count_with_predicate_client_eval()
        {
            await AssertSingleResult<Order>(os => os.OrderBy(o => ClientEvalSelectorStateless()).Where(o => ClientEvalPredicateStateless()).CountAsync(o => ClientEvalPredicate(o)));
        }

        [ConditionalFact]
        public virtual async Task OrderBy_Where_Count_with_predicate_client_eval_mixed()
        {
            await AssertSingleResult<Order>(os => os.OrderBy(o => o.OrderID).Where(o => ClientEvalPredicate(o)).CountAsync(o => o.CustomerID != "ALFKI"));
        }

        public static bool ClientEvalPredicateStateless() => true;

        protected static bool ClientEvalPredicate(Order order) => order.OrderID > 10000;

        private static int ClientEvalSelectorStateless() => 42;

#if Test20
        protected internal int ClientEvalSelector(Order order) => order.EmployeeID % 10 ?? 0;
#else
        protected internal uint ClientEvalSelector(Order order) => order.EmployeeID % 10 ?? 0;
#endif

        [ConditionalFact]
        public virtual async Task Distinct()
        {
            await AssertQuery<Customer>(
                cs => cs.Distinct(),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task Distinct_Scalar()
        {
            await AssertQuery<Customer>(
                cs =>
                    cs.Select(c => c.City).Distinct());
        }

        [ConditionalFact]
        public virtual async Task OrderBy_Distinct()
        {
            await AssertQuery<Customer>(
                cs =>
                    cs.OrderBy(c => c.CustomerID).Select(c => c.City).Distinct());
        }

        [ConditionalFact]
        public virtual async Task Distinct_OrderBy()
        {
            await AssertQuery<Customer>(
                cs =>
                    cs.Select(c => c.Country).Distinct().OrderBy(c => c),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual async Task Distinct_Count()
        {
            await AssertSingleResult<Customer>(cs => cs.Distinct().CountAsync());
        }

        [ConditionalFact]
        public virtual async Task Select_Distinct_Count()
        {
            await AssertSingleResult<Customer>(cs => cs.Select(c => c.City).Distinct().CountAsync());
        }

        [ConditionalFact]
        public virtual async Task Select_Select_Distinct_Count()
        {
            await AssertSingleResult<Customer>(cs => cs.Select(c => c.City).Select(c => c).Distinct().CountAsync());
        }

        [ConditionalFact]
        public virtual async Task Single_Throws()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () =>
                    await AssertSingleResult<Customer, Customer>(
                        cs => cs.SingleAsync()));
        }

        [ConditionalFact]
        public virtual async Task Single_Predicate()
        {
            await AssertSingleResult<Customer, Customer>(
                cs => cs.SingleAsync(c => c.CustomerID == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_Single()
        {
            await AssertSingleResult<Customer, Customer>(
                // ReSharper disable once ReplaceWithSingleCallToSingle
                cs => cs.Where(c => c.CustomerID == "ALFKI").SingleAsync(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task SingleOrDefault_Throws()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () =>
                    await AssertSingleResult<Customer, Customer>(
                        cs => cs.SingleOrDefaultAsync()));
        }

        [ConditionalFact]
        public virtual async Task SingleOrDefault_Predicate()
        {
            await AssertSingleResult<Customer, Customer>(
                cs => cs.SingleOrDefaultAsync(c => c.CustomerID == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_SingleOrDefault()
        {
            await AssertSingleResult<Customer, Customer>(
                // ReSharper disable once ReplaceWithSingleCallToSingleOrDefault
                cs => cs.Where(c => c.CustomerID == "ALFKI").SingleOrDefaultAsync(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task FirstAsync()
        {
            await AssertSingleResult<Customer, Customer>(
                cs => cs.OrderBy(c => c.ContactName).FirstAsync(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task First_Predicate()
        {
            await AssertSingleResult<Customer, Customer>(
                cs => cs.OrderBy(c => c.ContactName).FirstAsync(c => c.City == "London"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_First()
        {
            await AssertSingleResult<Customer, Customer>(
                // ReSharper disable once ReplaceWithSingleCallToFirst
                cs => cs.OrderBy(c => c.ContactName).Where(c => c.City == "London").FirstAsync(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task FirstOrDefault()
        {
            await AssertSingleResult<Customer, Customer>(
                cs => cs.OrderBy(c => c.ContactName).FirstOrDefaultAsync(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task FirstOrDefault_Predicate()
        {
            await AssertSingleResult<Customer, Customer>(
                cs => cs.OrderBy(c => c.ContactName).FirstOrDefaultAsync(c => c.City == "London"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_FirstOrDefault()
        {
            await AssertSingleResult<Customer, Customer>(
                // ReSharper disable once ReplaceWithSingleCallToFirstOrDefault
                cs => cs.OrderBy(c => c.ContactName).Where(c => c.City == "London").FirstOrDefaultAsync(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Last()
        {
            await AssertSingleResult<Customer, Customer>(
                cs => cs.OrderBy(c => c.ContactName).LastAsync(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Last_when_no_order_by()
        {
            await AssertSingleResult<Customer, Customer>(
                // ReSharper disable once ReplaceWithSingleCallToLast
                cs => cs.Where(c => c.CustomerID == "ALFKI").LastAsync(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Last_Predicate()
        {
            await AssertSingleResult<Customer, Customer>(
                cs => cs.OrderBy(c => c.ContactName).LastAsync(c => c.City == "London"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_Last()
        {
            await AssertSingleResult<Customer, Customer>(
                // ReSharper disable once ReplaceWithSingleCallToLast
                cs => cs.OrderBy(c => c.ContactName).Where(c => c.City == "London").LastAsync(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task LastOrDefault()
        {
            await AssertSingleResult<Customer, Customer>(
                cs => cs.OrderBy(c => c.ContactName).LastOrDefaultAsync(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task LastOrDefault_Predicate()
        {
            await AssertSingleResult<Customer, Customer>(
                cs => cs.OrderBy(c => c.ContactName).LastOrDefaultAsync(c => c.City == "London"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_LastOrDefault()
        {
            await AssertSingleResult<Customer, Customer>(
                // ReSharper disable once ReplaceWithSingleCallToLastOrDefault
                cs => cs.OrderBy(c => c.ContactName).Where(c => c.City == "London").LastOrDefaultAsync(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task String_StartsWith_Literal()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.StartsWith("M")),
                entryCount: 12);
        }

        [ConditionalFact]
        public virtual async Task String_StartsWith_Identity()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.StartsWith(c.ContactName)),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task String_StartsWith_Column()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.StartsWith(c.ContactName)),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task String_StartsWith_MethodCall()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.StartsWith(LocalMethod1())),
                entryCount: 12);
        }

        [ConditionalFact]
        public virtual async Task String_EndsWith_Literal()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.EndsWith("b")),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task String_EndsWith_Identity()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.EndsWith(c.ContactName)),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task String_EndsWith_Column()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.EndsWith(c.ContactName)),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task String_EndsWith_MethodCall()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.EndsWith(LocalMethod2())),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task String_Contains_Literal()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains("M")),
                entryCount: 19);
        }

        [ConditionalFact]
        public virtual async Task String_Contains_Identity()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains(c.ContactName)),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task String_Contains_Column()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains(c.ContactName)),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task String_Contains_MethodCall()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains(LocalMethod1())),
                entryCount: 19);
        }

        protected static string LocalMethod1()
        {
            return "M";
        }

        protected static string LocalMethod2()
        {
            return "m";
        }

        [ConditionalFact]
        public virtual async Task GroupJoin_simple()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o in os on c.CustomerID equals o.CustomerID into orders
                    from o in orders
                    select o,
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual async Task GroupJoin_simple3()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o in os on c.CustomerID equals o.CustomerID into orders
                    from o in orders
                    select new { o.OrderID },
                elementSorter: e => e.OrderID);
        }

        [ConditionalFact]
        public virtual async Task GroupJoin_projection()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o in os on c.CustomerID equals o.CustomerID into orders
                    from o in orders
                    select new { c, o },
                elementSorter: e => e.c.CustomerID + " " + e.o.OrderID,
                entryCount: 919);
        }

        [ConditionalFact]
        public virtual async Task GroupJoin_DefaultIfEmpty()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o in os on c.CustomerID equals o.CustomerID into orders
                    from o in orders.DefaultIfEmpty()
                    select new { c, o },
                elementSorter: e => e.c.CustomerID + " " + e.o?.OrderID,
                entryCount: 921);
        }

        [ConditionalFact]
        public virtual async Task GroupJoin_DefaultIfEmpty2()
        {
            await AssertQuery<Employee, Order>(
                (es, os) =>
                    from e in es
                    join o in os on e.EmployeeID equals o.EmployeeID into orders
                    from o in orders.DefaultIfEmpty()
                    select new { e, o },
                elementSorter: e => e.e.EmployeeID + " " + e.o.OrderID,
                entryCount: 839);
        }

        [ConditionalFact]
        public virtual async Task GroupJoin_DefaultIfEmpty3()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs //.Take(1)
                    join o in os on c.CustomerID equals o.CustomerID into orders
                    from o in orders.DefaultIfEmpty()
                    select o,
                elementSorter: e => e?.OrderID,
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual async Task GroupJoin_tracking_groups()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o in os on c.CustomerID equals o.CustomerID into orders
                    select orders,
                elementSorter: CollectionSorter<Order>(),
                elementAsserter: CollectionAsserter<Order>(o => o.OrderID),
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual async Task GroupJoin_tracking_groups2()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o in os on c.CustomerID equals o.CustomerID into orders
                    select new { c, orders },
                elementSorter: e => e.c.CustomerID,
                elementAsserter: (e, a) =>
                    {
                        Assert.Equal(e.c.CustomerID, a.c.CustomerID);
                        CollectionAsserter<Order>(o => o.OrderID)(e.orders, a.orders);
                    },
                entryCount: 921);
        }

        [ConditionalFact]
        public virtual async Task SelectMany_Joined()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    from o in os.Where(o => o.CustomerID == c.CustomerID)
                    select new { c.ContactName, o.OrderDate },
                elementSorter: e => e.ContactName + " " + e.OrderDate);
        }

        [ConditionalFact]
        public virtual async Task SelectMany_Joined_DefaultIfEmpty()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    from o in os.Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                    select new { c.ContactName, o },
                elementSorter: e => e.ContactName + " " + e.o?.OrderID,
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual async Task SelectMany_Joined_DefaultIfEmpty2()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    from o in os.Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                    select o,
                elementSorter: e => e?.OrderID,
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual async Task Select_many_cross_join_same_collection()
        {
            await AssertQuery<Customer, Customer>(
                (cs1, cs2) => cs1.SelectMany(c => cs2),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task Join_same_collection_multiple()
        {
            await AssertQuery<Customer, Customer, Customer>(
                (cs1, cs2, cs3) => cs1
                    .Join(cs2, o => o.CustomerID, i => i.CustomerID, (c1, c2) => new { c1, c2 })
                    .Join(cs3, o => o.c1.CustomerID, i => i.CustomerID, (c12, c3) => c3),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task Join_same_collection_force_alias_uniquefication()
        {
            await AssertQuery<Order, Order>(
                (os1, os2) => os1.Join(os2, o => o.CustomerID, i => i.CustomerID, (_, o) => new { _, o }),
                elementSorter: e => e._.OrderID + " " + e.o.OrderID,
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual async Task Contains_with_subquery()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    cs.Where(c => os.Select(o => o.CustomerID).Contains(c.CustomerID)),
                entryCount: 89);
        }

        [ConditionalFact]
        public virtual async Task Contains_with_local_array_closure()
        {
            string[] ids = { "ABCDE", "ALFKI" };
            await AssertQuery<Customer>(
                cs =>
                    cs.Where(c => ids.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Contains_with_local_array_inline()
        {
            await AssertQuery<Customer>(
                cs =>
                    cs.Where(c => new[] { "ABCDE", "ALFKI" }.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Contains_with_local_list_closure()
        {
            var ids = new List<string> { "ABCDE", "ALFKI" };
            await AssertQuery<Customer>(
                cs =>
                    cs.Where(c => ids.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Contains_with_local_list_inline()
        {
            await AssertQuery<Customer>(
                cs =>
                    cs.Where(c => new List<string> { "ABCDE", "ALFKI" }.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Contains_with_local_list_inline_closure_mix()
        {
            var alfki = "ALFKI";
            await AssertQuery<Customer>(
                cs =>
                    cs.Where(c => new List<string> { "ABCDE", alfki }.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Contains_with_local_collection_false()
        {
            string[] ids = { "ABCDE", "ALFKI" };
            await AssertQuery<Customer>(
                cs =>
                    cs.Where(c => !ids.Contains(c.CustomerID)), entryCount: 90);
        }

        [ConditionalFact]
        public virtual async Task Contains_with_local_collection_complex_predicate_and()
        {
            string[] ids = { "ABCDE", "ALFKI" };
            await AssertQuery<Customer>(
                cs =>
                    cs.Where(c => (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE") && ids.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Contains_with_local_collection_complex_predicate_or()
        {
            string[] ids = { "ABCDE", "ALFKI" };
            await AssertQuery<Customer>(
                cs =>
                    cs.Where(c => ids.Contains(c.CustomerID) || c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE"), entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Contains_with_local_collection_complex_predicate_not_matching_ins1()
        {
            string[] ids = { "ABCDE", "ALFKI" };
            await AssertQuery<Customer>(
                cs =>
                    cs.Where(c => c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE" || !ids.Contains(c.CustomerID)), entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task Contains_with_local_collection_complex_predicate_not_matching_ins2()
        {
            string[] ids = { "ABCDE", "ALFKI" };
            await AssertQuery<Customer>(
                cs =>
                    cs.Where(c => ids.Contains(c.CustomerID) && c.CustomerID != "ALFKI" && c.CustomerID != "ABCDE"));
        }

        [ConditionalFact]
        public virtual async Task Contains_with_local_collection_sql_injection()
        {
            string[] ids = { "ALFKI", "ABC')); GO; DROP TABLE Orders; GO; --" };
            await AssertQuery<Customer>(
                cs =>
                    cs.Where(c => ids.Contains(c.CustomerID) || c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE"), entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Contains_with_local_collection_empty_closure()
        {
            var ids = Array.Empty<string>();

            await AssertQuery<Customer>(
                cs =>
                    cs.Where(c => ids.Contains(c.CustomerID)));
        }

        [ConditionalFact]
        public virtual async Task Contains_with_local_collection_empty_inline()
        {
            await AssertQuery<Customer>(
                cs =>
                    cs.Where(c => !new List<string>().Contains(c.CustomerID)), entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task Contains_top_level()
        {
            await AssertSingleResult<Customer>(cs => cs.Select(c => c.CustomerID).ContainsAsync("ALFKI"));
        }

        [ConditionalFact]
        public virtual async Task Where_chain()
        {
            await AssertQuery<Order>(
                order => order
                    .Where(o => o.CustomerID == "QUICK")
                    .Where(o => o.OrderDate > new DateTime(1998, 1, 1)), entryCount: 8);
        }

        [ConditionalFact]
        public virtual async Task Throws_on_concurrent_query_list()
        {
            using (var context = CreateContext())
            {
                using (var synchronizationEvent = new ManualResetEventSlim(false))
                {
                    using (var blockingSemaphore = new SemaphoreSlim(0))
                    {
                        var blockingTask = Task.Run(
                            () =>
                                context.Customers.Select(
                                    c => Process(c, synchronizationEvent, blockingSemaphore)).ToList());

                        var throwingTask = Task.Run(
                            async () =>
                                {
                                    synchronizationEvent.Wait();

                                    Assert.Equal(
                                        CoreStrings.ConcurrentMethodInvocation,
                                        (await Assert.ThrowsAsync<InvalidOperationException>(
                                            () => context.Customers.ToListAsync())).Message);
                                });

                        await throwingTask;

                        blockingSemaphore.Release(1);

                        await blockingTask;
                    }
                }
            }
        }

        [ConditionalFact]
        public virtual async Task Throws_on_concurrent_query_first()
        {
            using (var context = CreateContext())
            {
                using (var synchronizationEvent = new ManualResetEventSlim(false))
                {
                    using (var blockingSemaphore = new SemaphoreSlim(0))
                    {
                        var blockingTask = Task.Run(
                            () =>
                                context.Customers.Select(
                                    c => Process(c, synchronizationEvent, blockingSemaphore)).ToList());

                        var throwingTask = Task.Run(
                            async () =>
                                {
                                    synchronizationEvent.Wait();
                                    Assert.Equal(
                                        CoreStrings.ConcurrentMethodInvocation,
                                        (await Assert.ThrowsAsync<InvalidOperationException>(
                                            () => context.Customers.FirstAsync())).Message);
                                });

                        await throwingTask;

                        blockingSemaphore.Release(1);

                        await blockingTask;
                    }
                }
            }
        }

        private static Customer Process(Customer c, ManualResetEventSlim e, SemaphoreSlim s)
        {
            e.Set();
            s.Wait();
            s.Release(1);
            return c;
        }

        // Set Operations

        [ConditionalFact]
        public virtual async Task Concat_dbset()
        {
            using (var context = CreateContext())
            {
                var query = await context.Set<Customer>()
                    .Where(c => c.City == "México D.F.")
                    .Concat(
                        context.Set<Customer>())
                    .ToListAsync();

                Assert.Equal(96, query.Count);
            }
        }

        [ConditionalFact]
        public virtual async Task Concat_simple()
        {
            using (var context = CreateContext())
            {
                var query = await context.Set<Customer>()
                    .Where(c => c.City == "México D.F.")
                    .Concat(
                        context.Set<Customer>()
                            .Where(s => s.ContactTitle == "Owner"))
                    .ToListAsync();

                Assert.Equal(22, query.Count);
            }
        }

        [ConditionalFact]
        public virtual async Task Concat_nested()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == "México D.F.")
                    .Concat(cs.Where(s => s.City == "Berlin"))
                    .Concat(cs.Where(e => e.City == "London")),
                entryCount: 12);
        }

        [ConditionalFact]
        public virtual async Task Concat_non_entity()
        {
            using (var context = CreateContext())
            {
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
        }

        [ConditionalFact]
        public virtual async Task Except_dbset()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(s => s.ContactTitle == "Owner").Except(cs));
        }

        [ConditionalFact]
        public virtual async Task Except_simple()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(s => s.ContactTitle == "Owner")
                    .Except(cs.Where(c => c.City == "México D.F.")),
                entryCount: 14);
        }

        [ConditionalFact]
        public virtual async Task Except_nested()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(s => s.ContactTitle == "Owner")
                    .Except(cs.Where(s => s.City == "México D.F."))
                    .Except(cs.Where(e => e.City == "Seattle")),
                entryCount: 13);
        }

        [ConditionalFact]
        public virtual async Task Except_non_entity()
        {
            using (var context = CreateContext())
            {
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
        }

        [ConditionalFact]
        public virtual async Task Intersect_dbset()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == "México D.F.").Intersect(cs),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual async Task Intersect_simple()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == "México D.F.")
                    .Intersect(cs.Where(s => s.ContactTitle == "Owner")),
                entryCount: 3);
        }

        [ConditionalFact]
        public virtual async Task Intersect_nested()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(c => c.City == "México D.F.")
                    .Intersect(cs.Where(s => s.ContactTitle == "Owner"))
                    .Intersect(cs.Where(e => e.Fax != null)),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Intersect_non_entity()
        {
            using (var context = CreateContext())
            {
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
        }

        [ConditionalFact]
        public virtual async Task Union_dbset()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(s => s.ContactTitle == "Owner")
                    .Union(cs.Where(c => c.City == "México D.F.")),
                entryCount: 19);
        }

        [ConditionalFact]
        public virtual async Task Union_simple()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(s => s.ContactTitle == "Owner")
                    .Union(cs.Where(c => c.City == "México D.F.")),
                entryCount: 19);
        }

        [ConditionalFact]
        public virtual async Task Union_nested()
        {
            await AssertQuery<Customer>(
                cs => cs.Where(s => s.ContactTitle == "Owner")
                    .Union(cs.Where(s => s.City == "México D.F."))
                    .Union(cs.Where(e => e.City == "London")),
                entryCount: 25);
        }

        [ConditionalFact]
        public virtual async Task Union_non_entity()
        {
            using (var context = CreateContext())
            {
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
        }

        [ConditionalFact]
        public virtual async Task Where_bitwise_or()
        {
            await AssertQuery<Customer>(
                cs =>
                    cs.Where(c => c.CustomerID == "ALFKI" | c.CustomerID == "ANATR"),
                entryCount: 2);
        }

        [ConditionalFact]
        public virtual async Task Where_bitwise_and()
        {
            await AssertQuery<Customer>(
                cs =>
                    cs.Where(c => c.CustomerID == "ALFKI" & c.CustomerID == "ANATR"));
        }

        [ConditionalFact]
        public virtual async Task Select_bitwise_or()
        {
            using (var context = CreateContext())
            {
                var query = await context.Customers.OrderBy(c => c.CustomerID).Select(c => new { c.CustomerID, Value = c.CustomerID == "ALFKI" | c.CustomerID == "ANATR" }).ToListAsync();

                Assert.All(query.Take(2), t => Assert.True(t.Value));
                Assert.All(query.Skip(2), t => Assert.False(t.Value));
            }
        }

        [ConditionalFact]
        public virtual async Task Select_bitwise_or_multiple()
        {
            using (var context = CreateContext())
            {
                var query = await context.Customers.OrderBy(c => c.CustomerID)
                    .Select(c => new { c.CustomerID, Value = c.CustomerID == "ALFKI" | c.CustomerID == "ANATR" | c.CustomerID == "ANTON" }).ToListAsync();

                Assert.All(query.Take(3), t => Assert.True(t.Value));
                Assert.All(query.Skip(3), t => Assert.False(t.Value));
            }
        }

        [ConditionalFact]
        public virtual async Task Select_bitwise_and()
        {
            using (var context = CreateContext())
            {
                var query = await context.Customers.OrderBy(c => c.CustomerID).Select(c => new { c.CustomerID, Value = c.CustomerID == "ALFKI" & c.CustomerID == "ANATR" }).ToListAsync();

                Assert.All(query, t => Assert.False(t.Value));
            }
        }

        [ConditionalFact]
        public virtual async Task Select_bitwise_and_or()
        {
            using (var context = CreateContext())
            {
                var query = await context.Customers.OrderBy(c => c.CustomerID)
                    .Select(c => new { c.CustomerID, Value = c.CustomerID == "ALFKI" & c.CustomerID == "ANATR" | c.CustomerID == "ANTON" }).ToListAsync();

                Assert.All(query.Where(c => c.CustomerID != "ANTON"), t => Assert.False(t.Value));
            }
        }

        [ConditionalFact]
        public virtual async Task Where_bitwise_or_with_logical_or()
        {
            await AssertQuery<Customer>(
                cs =>
                    cs.Where(c => c.CustomerID == "ALFKI" | c.CustomerID == "ANATR" || c.CustomerID == "ANTON"),
                entryCount: 3);
        }

        [ConditionalFact]
        public virtual async Task Where_bitwise_and_with_logical_and()
        {
            await AssertQuery<Customer>(
                cs =>
                    cs.Where(c => c.CustomerID == "ALFKI" & c.CustomerID == "ANATR" && c.CustomerID == "ANTON"));
        }

        [ConditionalFact]
        public virtual async Task Where_bitwise_or_with_logical_and()
        {
            await AssertQuery<Customer>(
                cs =>
                    cs.Where(c => c.CustomerID == "ALFKI" | c.CustomerID == "ANATR" && c.Country == "Germany"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_bitwise_and_with_logical_or()
        {
            await AssertQuery<Customer>(
                cs =>
                    cs.Where(c => c.CustomerID == "ALFKI" & c.CustomerID == "ANATR" || c.CustomerID == "ANTON"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Select_bitwise_or_with_logical_or()
        {
            using (var context = CreateContext())
            {
                var query = await context.Customers.OrderBy(c => c.CustomerID).Select(
                    c => new
                    {
                        c.CustomerID,
                        Value = c.CustomerID == "ALFKI" | c.CustomerID == "ANATR" || c.CustomerID == "ANTON"
                    }).ToListAsync();

                Assert.All(query.Take(3), t => Assert.True(t.Value));
                Assert.All(query.Skip(3), t => Assert.False(t.Value));
            }
        }

        [ConditionalFact]
        public virtual async Task Select_bitwise_and_with_logical_and()
        {
            using (var context = CreateContext())
            {
                var query = await context.Customers.OrderBy(c => c.CustomerID).Select(
                    c => new
                    {
                        c.CustomerID,
                        Value = c.CustomerID == "ALFKI" & c.CustomerID == "ANATR" && c.CustomerID == "ANTON"
                    }).ToListAsync();

                Assert.All(query, t => Assert.False(t.Value));
            }
        }

        [ConditionalFact]
        public virtual async Task Skip_CountAsync()
        {
            await AssertSingleResult<Customer>(cs => cs.Skip(7).CountAsync());
        }

        [ConditionalFact]
        public virtual async Task Skip_LongCountAsync()
        {
            await AssertSingleResult<Customer>(cs => cs.Skip(7).LongCountAsync());
        }

        [ConditionalFact]
        public virtual async Task OrderBy_Skip_CountAsync()
        {
            await AssertSingleResult<Customer>(cs => cs.OrderBy(c => c.Country).Skip(7).CountAsync());
        }

        [ConditionalFact]
        public virtual async Task OrderBy_Skip_LongCountAsync()
        {
            await AssertSingleResult<Customer>(cs => cs.OrderBy(c => c.Country).Skip(7).LongCountAsync());
        }

        [ConditionalFact]
        public virtual async Task Contains_with_subquery_involving_join_binds_to_correct_table()
        {
            await AssertQuery<Order, OrderDetail>(
                (os, ods) =>
                    os.Where(
                        o => o.OrderID > 11000
                             && ods.Where(od => od.Product.ProductName == "Chai")
                                 .Select(od => od.OrderID)
                                 .Contains(o.OrderID)),
                entryCount: 8);
        }

        [ConditionalFact]
        public virtual async Task Cast_to_same_Type_CountAsync_works()
        {
            await AssertSingleResult<Customer>(cs => cs.Cast<Customer>().CountAsync());
        }

        [ConditionalFact]
        public virtual async Task Sum_with_no_data_nullable()
        {
            await AssertSingleResult<Order>(os => os.Where(o => o.OrderID < 0).Select(o => (int?)o.OrderID).SumAsync());
        }
    }
}
