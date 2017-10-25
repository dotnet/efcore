// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract partial class SimpleQueryTestBase<TFixture>
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
                os => os.Select(o => new { A = o.OrderID / (o.OrderID / 2), B = o.OrderID / o.OrderID / 2 }),
                e => e.A + " " + e.B);
        }

        [ConditionalFact]
        public virtual void Projection_when_null_value()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => c.Region));
        }

        [ConditionalFact]
        public virtual void Projection_when_client_evald_subquery()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => string.Join(", ", c.Orders.Select(o => o.CustomerID).ToList())));
        }

        [ConditionalFact]
        public virtual void Project_to_object_array()
        {
            AssertQuery<Employee>(
                es => es.Where(e => e.EmployeeID == 1)
                    .Select(e => new object[] { e.EmployeeID, e.ReportsTo, EF.Property<string>(e, "Title") }),
                elementAsserter: (e, a) => AssertArrays<object>(e, a, 3));
        }

        [ConditionalFact]
        public virtual void Project_to_int_array()
        {
            AssertQuery<Employee>(
                es => es.Where(e => e.EmployeeID == 1)
                    .Select(e => new[] { e.EmployeeID, e.ReportsTo }),
#if Test20
                elementAsserter: (e, a) => AssertArrays<int?>(e, a, 2));
#else
                elementAsserter: (e, a) => AssertArrays<uint?>(e, a, 2));
#endif
        }

        private static void AssertArrays<T>(object e, object a, int count)
        {
            var expectedArray = (T[])e;
            var actualArray = (T[])a;

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
                cs => cs.Select(c => new { f = boolean }),
                e => e.f);

            boolean = true;

            AssertQuery<Customer>(
                cs => cs.Select(c => new { f = boolean }),
                e => e.f);
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
                cs => cs.Select(c => new { c.City }),
                e => e.City);
        }

        [ConditionalFact]
        public virtual void Select_anonymous_two()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.City, c.Phone }),
                e => e.Phone);
        }

        [ConditionalFact]
        public virtual void Select_anonymous_three()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.City, c.Phone, c.Country }),
                e => e.Phone);
        }

        [ConditionalFact]
        public virtual void Select_anonymous_bool_constant_true()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.CustomerID, ConstantTrue = true }),
                e => e.CustomerID);
        }

        [ConditionalFact]
        public virtual void Select_anonymous_constant_in_expression()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.CustomerID, Expression = c.CustomerID.Length + 5 }),
                e => e.CustomerID);
        }

        [ConditionalFact]
        public virtual void Select_anonymous_conditional_expression()
        {
            AssertQuery<Product>(
                ps => ps.Select(p => new { p.ProductID, IsAvailable = p.UnitsInStock > 0 }),
                e => e.ProductID);
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
                e => e.c.CustomerID,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Select_anonymous_nested()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.City, Country = new { c.Country } }),
                e => e.City);
        }

        [ConditionalFact]
        public virtual void Select_anonymous_empty()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { }),
                e => 1);
        }

        [ConditionalFact]
        public virtual void Select_anonymous_literal()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { X = 10 }),
                e => e.X);
        }

        [ConditionalFact]
        public virtual void Select_constant_int()
        {
            AssertQueryScalar<Customer>(cs => cs.Select(c => 0));
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

            AssertQueryScalar<Customer>(cs => cs.Select(c => x));
        }

        [ConditionalFact]
        public virtual void Select_scalar_primitive()
        {
            AssertQueryScalar<Employee>(
                es => es.Select(e => e.EmployeeID));
        }

        [ConditionalFact]
        public virtual void Select_scalar_primitive_after_take()
        {
            AssertQueryScalar<Employee>(
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
            AssertQuery<Customer, Order>(
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
        public virtual void Select_nested_collection_multi_level()
        {
            using (var context = CreateContext())
            {
                var customers = context.Customers
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .Select(
                        c => new
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
                    .Select(
                        c => new
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
                    .Select(
                        c => new
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
                    .Select(
                        c => new
                        {
                            Order = (int?)c.Orders
                                .Where(o => o.OrderID < 10500)
                                .Select(
                                    o => o.OrderDetails
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
                    .Select(
                        c => new
                        {
                            Order = (int?)c.Orders
                                .Where(o => o.OrderID < 10500)
                                .Select(
                                    o => o.OrderDetails
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
                    .Select(
                        c => new
                        {
                            Order = (int?)c.Orders
                                .Where(o => o.OrderID < 10500)
                                .Select(
                                    o => o.OrderDetails
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
        public virtual void Select_nested_collection_count_using_anonymous_type()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID.StartsWith("A"))
                    .Select(c => new { c.Orders.Count }),
                e => e.Count);
        }

        [ConditionalFact]
        public virtual void Select_nested_collection_deep()
        {
            AssertQuery<Customer, Order>(
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
        public virtual void New_date_time_in_anonymous_type_works()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      where c.CustomerID.StartsWith("A")
                      select new { A = new DateTime() },
                e => e.A);
        }

        [ConditionalFact]
        public virtual void Select_non_matching_value_types_int_to_long_introduces_explicit_cast()
        {
            AssertQueryScalar<Order>(
                os => os
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)o.OrderID),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Select_non_matching_value_types_nullable_int_to_long_introduces_explicit_cast()
        {
            AssertQueryScalar<Order>(
                os => os
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)o.EmployeeID),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Select_non_matching_value_types_nullable_int_to_int_doesnt_introduces_explicit_cast()
        {
            AssertQueryScalar<Order>(
                os => os
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
#if Test20
                    .Select(o => (int)o.EmployeeID),
#else
                    .Select(o => (uint)o.EmployeeID),
#endif
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Select_non_matching_value_types_int_to_nullable_int_doesnt_introduce_explicit_cast()
        {
            AssertQueryScalar<Order>(
                os => os
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (int?)o.OrderID),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Select_non_matching_value_types_from_binary_expression_introduces_explicit_cast()
        {
            AssertQueryScalar<Order>(
                os => os
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)(o.OrderID + o.OrderID)),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Select_non_matching_value_types_from_binary_expression_nested_introduces_top_level_explicit_cast()
        {
            AssertQueryScalar<Order>(
                os => os
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (short)((long)o.OrderID + (long)o.OrderID)),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast1()
        {
            AssertQueryScalar<Order>(
                os => os
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)-o.OrderID),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast2()
        {
            AssertQueryScalar<Order>(
                os => os
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => -((long)o.OrderID)),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Select_non_matching_value_types_from_length_introduces_explicit_cast()
        {
            AssertQueryScalar<Order>(
                os => os
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)o.CustomerID.Length),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Select_non_matching_value_types_from_method_call_introduces_explicit_cast()
        {
            AssertQueryScalar<Order>(
                os => os
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)Math.Abs(o.OrderID)),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Select_non_matching_value_types_from_anonymous_type_introduces_explicit_cast()
        {
            AssertQuery<Order>(
                os => os
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => new { LongOrder = (long)o.OrderID, ShortOrder = (short)o.OrderID, Order = o.OrderID }),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Select_conditional_with_null_comparison_in_test()
        {
            AssertQueryScalar<Order>(
                os => from o in os
                      where o.CustomerID == "ALFKI"
                      select o.CustomerID == null ? true : o.OrderID < 100);
        }

        [ConditionalFact]
        public virtual void Projection_in_a_subquery_should_be_liftable()
        {
            AssertQuery<Employee>(
                es => es.OrderBy(e => e.EmployeeID)
                    .Select(e => string.Format("{0}", e.EmployeeID))
                    .Skip(1));
        }
    }
}
