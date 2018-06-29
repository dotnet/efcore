// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract partial class SimpleQueryTestBase<TFixture>
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_into(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    from c in cs
                    select c.CustomerID
                    into id
                    where id == "ALFKI"
                    select id);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Projection_when_arithmetic_expression_precendence(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Select(
                    o => new
                    {
                        A = o.OrderID / (o.OrderID / 2),
                        B = o.OrderID / o.OrderID / 2
                    }),
                e => e.A + " " + e.B);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Projection_when_null_value(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(c => c.Region));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Projection_when_client_evald_subquery(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(c => string.Join(", ", c.Orders.Select(o => o.CustomerID).ToList())));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Project_to_object_array(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es => es.Where(e => e.EmployeeID == 1)
                    .Select(e => new object[] { e.EmployeeID, e.ReportsTo, EF.Property<string>(e, "Title") }),
                elementAsserter: (e, a) => AssertArrays<object>(e, a, 3));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Project_to_int_array(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es => es.Where(e => e.EmployeeID == 1)
                    .Select(e => new[] { e.EmployeeID, e.ReportsTo }),
                elementAsserter: (e, a) => AssertArrays<uint?>(e, a, 2));
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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Select_bool_closure(bool isAsync)
        {
            var boolean = false;

            await AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        f = boolean
                    }),
                e => e.f);

            boolean = true;

            await AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        f = boolean
                    }),
                e => e.f);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_scalar(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(c => c.City));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_anonymous_one(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        c.City
                    }),
                e => e.City);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_anonymous_two(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        c.City,
                        c.Phone
                    }),
                e => e.Phone);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_anonymous_three(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        c.City,
                        c.Phone,
                        c.Country
                    }),
                e => e.Phone);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_anonymous_bool_constant_true(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        c.CustomerID,
                        ConstantTrue = true
                    }),
                e => e.CustomerID);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_anonymous_constant_in_expression(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        c.CustomerID,
                        Expression = c.CustomerID.Length + 5
                    }),
                e => e.CustomerID);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_anonymous_conditional_expression(bool isAsync)
        {
            return AssertQuery<Product>(
                isAsync,
                ps => ps.Select(
                    p => new
                    {
                        p.ProductID,
                        IsAvailable = p.UnitsInStock > 0
                    }),
                e => e.ProductID);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_customer_table(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs,
                entryCount: 91);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_customer_identity(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(c => c),
                entryCount: 91);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_anonymous_with_object(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        c.City,
                        c
                    }),
                e => e.c.CustomerID,
                entryCount: 91);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_anonymous_nested(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        c.City,
                        Country = new
                        {
                            c.Country
                        }
                    }),
                e => e.City);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_anonymous_empty(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                    }),
                e => 1);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_anonymous_literal(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        X = 10
                    }),
                e => e.X);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_constant_int(bool isAsync)
        {
            return AssertQueryScalar<Customer>(
                isAsync,
                cs => cs.Select(c => 0));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_constant_null_string(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(c => (string)null));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_local(bool isAsync)
        {
            // ReSharper disable once ConvertToConstant.Local
            var x = 10;

            return AssertQueryScalar<Customer>(
                isAsync,
                cs => cs.Select(c => x));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_scalar_primitive(bool isAsync)
        {
            return AssertQueryScalar<Employee>(
                isAsync,
                es => es.Select(e => e.EmployeeID));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_scalar_primitive_after_take(bool isAsync)
        {
            return AssertQueryScalar<Employee>(
                isAsync,
                es => es.Take(9).Select(e => e.EmployeeID));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_project_filter(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    from c in cs
                    where c.City == "London"
                    select c.CompanyName);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_project_filter2(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    from c in cs
                    where c.City == "London"
                    select c.City);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_nested_collection(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
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
                                .Select(
                                    o => new
                                    {
                                        Date = o.OrderDate
                                    })
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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_nested_collection_count_using_anonymous_type(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID.StartsWith("A"))
                    .Select(
                        c => new
                        {
                            c.Orders.Count
                        }),
                e => e.Count);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_nested_collection_deep(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task New_date_time_in_anonymous_type_works(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs
                      where c.CustomerID.StartsWith("A")
                      select new
                      {
                          A = new DateTime()
                      },
                e => e.A);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_non_matching_value_types_int_to_long_introduces_explicit_cast(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)o.OrderID),
                assertOrder: true);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_non_matching_value_types_nullable_int_to_long_introduces_explicit_cast(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)o.EmployeeID),
                assertOrder: true);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_non_matching_value_types_nullable_int_to_int_doesnt_introduce_explicit_cast(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (uint)o.EmployeeID),
                assertOrder: true);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_non_matching_value_types_int_to_nullable_int_doesnt_introduce_explicit_cast(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (int?)o.OrderID),
                assertOrder: true);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_non_matching_value_types_from_binary_expression_introduces_explicit_cast(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)(o.OrderID + o.OrderID)),
                assertOrder: true);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_non_matching_value_types_from_binary_expression_nested_introduces_top_level_explicit_cast(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (short)((long)o.OrderID + (long)o.OrderID)),
                assertOrder: true);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast1(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)-o.OrderID),
                assertOrder: true);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast2(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => -((long)o.OrderID)),
                assertOrder: true);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_non_matching_value_types_from_length_introduces_explicit_cast(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)o.CustomerID.Length),
                assertOrder: true);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_non_matching_value_types_from_method_call_introduces_explicit_cast(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)Math.Abs(o.OrderID)),
                assertOrder: true);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_non_matching_value_types_from_anonymous_type_introduces_explicit_cast(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(
                        o => new
                        {
                            LongOrder = (long)o.OrderID,
                            ShortOrder = (short)o.OrderID,
                            Order = o.OrderID
                        }),
                assertOrder: true);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_conditional_with_null_comparison_in_test(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => from o in os
                      where o.CustomerID == "ALFKI"
                      select o.CustomerID == null ? true : o.OrderID < 100);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Projection_in_a_subquery_should_be_liftable(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es => es.OrderBy(e => e.EmployeeID)
                    .Select(e => string.Format("{0}", e.EmployeeID))
                    .Skip(1));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Projection_containing_DateTime_subtraction(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.Where(o => o.OrderID < 10300)
                    .Select(o => o.OrderDate.Value - new DateTime(1997, 1, 1)));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).Take(1).FirstOrDefault()));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Project_single_element_from_collection_with_OrderBy_Skip_and_FirstOrDefault(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).Skip(1).FirstOrDefault()));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).Distinct().FirstOrDefault()));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Project_single_element_from_collection_with_OrderBy_Take_and_SingleOrDefault(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID == "ALFKI")
                    .Select(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).Take(1).SingleOrDefault()));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault_with_parameter(bool isAsync)
        {
            var i = 1;
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).Take(i).FirstOrDefault()));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => c.Orders.OrderBy(o => o.OrderID)
                        .ThenByDescending(o => o.OrderDate)
                        .Select(o => o.CustomerID)
                        .Take(2)
                        .FirstOrDefault()));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_2(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => c.Orders.OrderBy(o => o.CustomerID)
                        .ThenByDescending(o => o.OrderDate)
                        .Select(o => o.CustomerID)
                        .Take(2)
                        .FirstOrDefault()));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.Where(o => o.OrderID < 10300)
                    .Select(
                        o => o.OrderDetails.OrderBy(od => od.Product.ProductName).Select(od => od.OrderID).Take(1).FirstOrDefault()));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderID < 10250)
                    .Select(
                        o => o.OrderDetails.OrderBy(od => od.Product.ProductName).Take(1).FirstOrDefault()));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_datetime_year_component(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.Select(o => o.OrderDate.Value.Year));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_datetime_month_component(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.Select(o => o.OrderDate.Value.Month));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_datetime_day_of_year_component(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.Select(o => o.OrderDate.Value.DayOfYear));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_datetime_day_component(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.Select(o => o.OrderDate.Value.Day));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_datetime_hour_component(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.Select(o => o.OrderDate.Value.Hour));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_datetime_minute_component(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.Select(o => o.OrderDate.Value.Minute));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_datetime_second_component(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.Select(o => o.OrderDate.Value.Second));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_datetime_millisecond_component(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.Select(o => o.OrderDate.Value.Millisecond));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_datetime_DayOfWeek_component(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.Select(o => (int)o.OrderDate.Value.DayOfWeek));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_datetime_Ticks_component(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.Select(o => o.OrderDate.Value.Ticks));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_datetime_TimeOfDay_component(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.Select(o => o.OrderDate.Value.TimeOfDay));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_byte_constant(bool isAsync)
        {
            return AssertQueryScalar<Customer, byte>(
                isAsync,
                cs => cs.Select(c => c.CustomerID == "ALFKI" ? (byte)1 : (byte)2));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_short_constant(bool isAsync)
        {
            return AssertQueryScalar<Customer, short>(
                isAsync,
                cs => cs.Select(c => c.CustomerID == "ALFKI" ? (short)1 : (short)2));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Select_bool_constant(bool isAsync)
        {
            return AssertQueryScalar<Customer, bool>(
                isAsync,
                cs => cs.Select(c => c.CustomerID == "ALFKI" ? true : false));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Anonymous_projection_AsNoTracking_Selector(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os => os.Select(
                        o => new
                        {
                            A = o.CustomerID,
                            B = o.OrderDate
                        })
                    .AsNoTracking() // Just to cause a subquery
                    .Select(e => e.B));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual Task Anonymous_projection_with_repeated_property_being_ordered(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs
                      orderby c.CustomerID
                      select new
                      {
                          A = c.CustomerID,
                          B = c.CustomerID
                      });
        }
    }
}
