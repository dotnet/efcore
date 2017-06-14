// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;
// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract partial class QueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase, new()
    {
        [ConditionalFact]
        public virtual void Select_into()
        {
            AssertQuery<Customer>(
                cs =>
                    from c in cs
                    select c.CustomerID
                    into id
                    where id == "ALFKI"
                    select id);
        }

        [ConditionalFact]
        public virtual void Projection_when_arithmetic_expression_precendence()
        {
            AssertQuery<Order>(
                os => os.Select(o => new { A = o.OrderID / (o.OrderID / 2), B = o.OrderID / o.OrderID / 2 }));
        }

        [ConditionalFact]
        public virtual void Projection_when_null_value()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => c.Region));
        }

        [ConditionalFact]
        public virtual void Project_to_object_array()
        {
            AssertQuery<Employee>(
                es => es.Where(e => e.EmployeeID == 1)
                    .Select(e => new object[] { e.EmployeeID, e.ReportsTo, EF.Property<string>(e, "Title") }),
                entryCount: 0,
                asserter: (e, a) => AssertArrays<object>(e, a, 3));
        }

        [ConditionalFact]
        public virtual void Project_to_int_array()
        {
            AssertQuery<Employee>(
                es => es.Where(e => e.EmployeeID == 1)
                    .Select(e => new[] { e.EmployeeID, e.ReportsTo }),
                entryCount: 0,
                asserter: (e, a) => AssertArrays<int?>(e, a, 2));
        }

        private static void AssertArrays<T>(IList<object> e, IList<object> a, int count)
        {
            Assert.Equal(1, e.Count);
            Assert.Equal(1, a.Count);

            var expectedArray = (T[])e[0];
            var actualArray = (T[])a[0];

            Assert.Equal(count, expectedArray.Length);
            Assert.Equal(count, actualArray.Length);

            for (var i = 0; i < expectedArray.Length; i++)
            {
                Assert.Same(expectedArray[i].GetType(), actualArray[i].GetType());
                Assert.Equal(expectedArray[i], actualArray[i]);
            }
        }

        [ConditionalFact]
        public virtual void Select_bool_closure()
        {
            var boolean = false;

            AssertQuery<Customer>(
                cs => cs.Select(c => new { f = boolean }));

            boolean = true;

            AssertQuery<Customer>(
                cs => cs.Select(c => new { f = boolean }));
        }

        [ConditionalFact]
        public virtual void Select_scalar()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => c.City));
        }

        [ConditionalFact]
        public virtual void Select_anonymous_one()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.City }));
        }

        [ConditionalFact]
        public virtual void Select_anonymous_two()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.City, c.Phone }));
        }

        [ConditionalFact]
        public virtual void Select_anonymous_three()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.City, c.Phone, c.Country }));
        }

        [ConditionalFact]
        public virtual void Select_anonymous_bool_constant_true()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.CustomerID, ConstantTrue = true }));
        }

        [ConditionalFact]
        public virtual void Select_anonymous_constant_in_expression()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.CustomerID, Expression = c.CustomerID.Length + 5 }));
        }

        [ConditionalFact]
        public virtual void Select_anonymous_conditional_expression()
        {
            AssertQuery<Product>(
                ps => ps.Select(p => new { p.ProductID, IsAvailable = p.UnitsInStock > 0 }));
        }

        [ConditionalFact]
        public virtual void Select_customer_table()
        {
            AssertQuery<Customer>(
                cs => cs,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Select_customer_identity()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => c),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Select_anonymous_with_object()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.City, c }),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Select_anonymous_nested()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.City, Country = new { c.Country } }));
        }

        [ConditionalFact]
        public virtual void Select_anonymous_empty()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { }));
        }

        [ConditionalFact]
        public virtual void Select_anonymous_literal()
        {
            AssertQuery<Customer>(cs => cs.Select(c => new { X = 10 }));
        }

        [ConditionalFact]
        public virtual void Select_constant_int()
        {
            AssertQuery<Customer>(cs => cs.Select(c => 0));
        }

        [ConditionalFact]
        public virtual void Select_constant_null_string()
        {
            AssertQuery<Customer>(cs => cs.Select(c => (string)null));
        }

        [ConditionalFact]
        public virtual void Select_local()
        {
            // ReSharper disable once ConvertToConstant.Local
            var x = 10;

            AssertQuery<Customer>(cs => cs.Select(c => x));
        }

        [ConditionalFact]
        public virtual void Select_scalar_primitive()
        {
            AssertQuery<Employee>(
                es => es.Select(e => e.EmployeeID));
        }

        [ConditionalFact]
        public virtual void Select_scalar_primitive_after_take()
        {
            AssertQuery<Employee>(
                es => es.Take(9).Select(e => e.EmployeeID));
        }

        [ConditionalFact]
        public virtual void Select_project_filter()
        {
            AssertQuery<Customer>(
                cs =>
                    from c in cs
                    where c.City == "London"
                    select c.CompanyName);
        }

        [ConditionalFact]
        public virtual void Select_project_filter2()
        {
            AssertQuery<Customer>(
                cs =>
                    from c in cs
                    where c.City == "London"
                    select c.City);
        }

        [ConditionalFact]
        public virtual void Select_nested_collection()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                    from c in cs
                    where c.City == "London"
                    orderby c.CustomerID
                    select os
                        .Where(o => o.CustomerID == c.CustomerID
                                    && o.OrderDate.Value.Year == 1997)
                        .Select(o => o.OrderID)
                        .OrderBy(o => o),
                asserter:
                (l2oResults, efResults) =>
                    {
                        var l2oObjects
                            = l2oResults
                                .SelectMany(q1 => (IEnumerable<int>)q1);

                        var efObjects
                            = efResults
                                .SelectMany(q1 => (IEnumerable<int>)q1);

                        Assert.Equal(l2oObjects, efObjects);
                    });
        }

        [ConditionalFact]
        public virtual void Select_nested_collection_multi_level()
        {
            using (var context = CreateContext())
            {
                var customers = context.Customers
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .Select(c => new
                    {
                        OrderDates = c.Orders
                            .Where(o => o.OrderID < 10500)
                            .Take(3)
                            .Select(o => new { Date = o.OrderDate })
                    })
                    .ToList();

                Assert.Equal(4, customers.Count);
                Assert.All(customers, t => Assert.True(t.OrderDates.Count() <= 3));
            }
        }

        [ConditionalFact]
        public virtual void Select_nested_collection_multi_level2()
        {
            using (var context = CreateContext())
            {
                var customers = context.Customers
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .Select(c => new
                    {
                        OrderDates = c.Orders
                            .Where(o => o.OrderID < 10500)
                            .Select(o => o.OrderDate)
                            .FirstOrDefault()
                    })
                    .ToList();

                Assert.Equal(4, customers.Count);
                Assert.Equal(3, customers.Count(c => c.OrderDates != null));
            }
        }

        [ConditionalFact]
        public virtual void Select_nested_collection_multi_level3()
        {
            using (var context = CreateContext())
            {
                var customers = context.Customers
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .Select(c => new
                    {
                        OrderDates = context.Orders
                            .Where(o => o.OrderID < 10500)
                            .Where(o => c.CustomerID == o.CustomerID)
                            .Select(o => o.OrderDate)
                            .FirstOrDefault()
                    })
                    .ToList();

                Assert.Equal(4, customers.Count);
                Assert.Equal(3, customers.Count(c => c.OrderDates != null));
            }
        }

        [ConditionalFact]
        public virtual void Select_nested_collection_multi_level4()
        {
            using (var context = CreateContext())
            {
                var customers = context.Customers
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .Select(c => new
                    {
                        Order = (int?)c.Orders
                            .Where(o => o.OrderID < 10500)
                            .Select(o => o.OrderDetails
                                .Where(od => od.OrderID > 10)
                                .Select(od => od.ProductID)
                                .Count())
                            .FirstOrDefault()
                    })
                    .ToList();

                Assert.Equal(4, customers.Count);
                Assert.Equal(3, customers.Count(c => c.Order != null && c.Order != 0));
            }
        }

        [ConditionalFact]
        public virtual void Select_nested_collection_multi_level5()
        {
            using (var context = CreateContext())
            {
                var customers = context.Customers
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .Select(c => new
                    {
                        Order = (int?)c.Orders
                            .Where(o => o.OrderID < 10500)
                            .Select(o => o.OrderDetails
                                .Where(od => od.OrderID != c.Orders.Count)
                                .Select(od => od.ProductID)
                                .FirstOrDefault())
                            .FirstOrDefault()
                    })
                    .ToList();

                Assert.Equal(4, customers.Count);
                Assert.Equal(3, customers.Count(c => c.Order != null && c.Order != 0));
            }
        }

        [ConditionalFact]
        public virtual void Select_nested_collection_multi_level6()
        {
            using (var context = CreateContext())
            {
                var customers = context.Customers
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .Select(c => new
                    {
                        Order = (int?)c.Orders
                            .Where(o => o.OrderID < 10500)
                            .Select(o => o.OrderDetails
                                .Where(od => od.OrderID != c.CustomerID.Length)
                                .Select(od => od.ProductID)
                                .FirstOrDefault())
                            .FirstOrDefault()
                    })
                    .ToList();

                Assert.Equal(4, customers.Count);
                Assert.Equal(3, customers.Count(c => c.Order != null && c.Order != 0));
            }
        }

        [ConditionalFact]
        public virtual void Select_nested_collection_with_groupby()
        {
            using (var context = CreateContext())
            {
                var expected = context.Customers
                    .Include(c => c.Orders)
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .ToList()
                    .Select(c => c.Orders.Any()
                        ? c.Orders.GroupBy(o => o.OrderID).Select(g => g.Key).ToArray()
                        : new int[0]).ToList();

                ClearLog();

                var query = context.Customers
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .Select(c => c.Orders.Any()
                        ? c.Orders.GroupBy(o => o.OrderID).Select(g => g.Key).ToArray()
                        : new int[0]);

                var result = query.ToList();

                Assert.Equal(expected.Count, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Select_nested_collection_count_using_anonymous_type()
        {
            AssertQuery<Customer>(cs => cs
                .Where(c => c.CustomerID.StartsWith("A"))
                .Select(c => new { c.Orders.Count }));
        }

        [ConditionalFact]
        public virtual void Select_nested_collection_count_using_DTO()
        {
            AssertQuery<Customer>(
                cs => cs
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .Select(c => new OrderCountDTO { Id = c.CustomerID, Count = c.Orders.Count }));
        }

        [ConditionalFact]
        public virtual void Select_nested_collection_deep()
        {
            AssertQuery<Customer, Order>((cs, os) =>
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
                asserter:
                (l2oResults, efResults) =>
                    {
                        var l2oObjects
                            = l2oResults
                                .SelectMany(q1 => ((IEnumerable<object>)q1)
                                    .SelectMany(q2 => (IEnumerable<int>)q2));

                        var efObjects
                            = efResults
                                .SelectMany(q1 => ((IEnumerable<object>)q1)
                                    .SelectMany(q2 => (IEnumerable<int>)q2));

                        Assert.Equal(l2oObjects, efObjects);
                    });
        }

        [ConditionalFact]
        public virtual void New_date_time_in_anonymous_type_works()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      where c.CustomerID.StartsWith("A")
                      select new { A = new DateTime() });
        }
    }
}
