// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

#pragma warning disable RCS1202 // Avoid NullReferenceException.

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindSelectQueryTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected NorthwindSelectQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected NorthwindContext CreateContext()
            => Fixture.CreateContext();

        protected virtual void ClearLog()
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_into(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      select c.CustomerID
                      into id
                      where id == "ALFKI"
                      select id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_when_arithmetic_expression_precedence(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Select(o => new { A = o.OrderID / (o.OrderID / 2), B = o.OrderID / o.OrderID / 2 }),
                e => (e.A, e.B));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_when_arithmetic_expressions(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Select(
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_when_arithmetic_mixed(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from o in ss.Set<Order>().OrderBy(o => o.OrderID).Take(10)
                    from e in ss.Set<Employee>().OrderBy(e => e.EmployeeID).Take(5)
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
                entryCount: 15);
        }

        [ConditionalTheory(Skip = "Issue#19247")]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Projection_when_arithmetic_mixed_subqueries(bool async)
        {
            Assert.Equal(
                "Unsupported Binary operator type specified.",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertQuery(
                        async,
                        ss =>
                            from o in ss.Set<Order>().OrderBy(o => o.OrderID).Take(3).Select(
                                o2 => new { o2, Mod = o2.OrderID % 2 })
                            from e in ss.Set<Employee>().OrderBy(e => e.EmployeeID).Take(2).Select(
                                e2 => new { e2, Square = e2.EmployeeID ^ 2 })
                            select new
                            {
                                Add = e.e2.EmployeeID + o.o2.OrderID,
                                e.Square,
                                e.e2,
                                Literal = 42,
                                o.o2,
                                o.Mod
                            },
                        elementSorter: e => (e.e2.EmployeeID, e.o2.OrderID),
                        entryCount: 3))).Message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_when_null_value(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => c.Region));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_when_client_evald_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => string.Join(", ", c.Orders.Select(o => o.CustomerID).ToList())));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_to_object_array(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Employee>().Where(e => e.EmployeeID == 1)
                    .Select(e => new object[] { e.EmployeeID, e.ReportsTo, EF.Property<string>(e, "Title") }),
                elementAsserter: (e, a) => AssertArrays(e, a, 3));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_of_entity_type_into_object_array(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Where(c => c.CustomerID.StartsWith("A"))
                    .Select(c => new object[] { c }),
                entryCount: 4,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_of_multiple_entity_types_into_object_array(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().OrderBy(o => o.OrderID).Where(o => o.OrderID < 10300)
                    .Select(o => new object[] { o, o.Customer }),
                entryCount: 87,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_of_entity_type_into_object_list(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Select(c => new List<object> { c }),
                entryCount: 91,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_to_int_array(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Employee>().Where(e => e.EmployeeID == 1)
                    .Select(e => new[] { e.EmployeeID, e.ReportsTo }),
                elementAsserter: (e, a) => AssertArrays(e, a, 2));
        }

        private static void AssertArrays<T>(T[] expectedArray, T[] actualArray, int count)
        {
            Assert.Equal(count, expectedArray.Length);
            Assert.Equal(count, actualArray.Length);

            for (var i = 0; i < expectedArray.Length; i++)
            {
                Assert.Same(expectedArray[i].GetType(), actualArray[i].GetType());
                Assert.Equal(expectedArray[i], actualArray[i]);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Select_bool_closure(bool async)
        {
            var boolean = false;

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => new { f = boolean }),
                e => e.f);

            boolean = true;

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => new { f = boolean }),
                e => e.f);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_bool_closure_with_order_by_property_with_cast_to_nullable(bool async)
        {
            var boolean = false;

            return AssertTranslationFailed(
                () => AssertQuery(
                    async,
                    ss => ss.Set<Customer>().Select(c => new { f = boolean }).OrderBy(e => (bool?)e.f),
                    assertOrder: true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Select_bool_closure_with_order_parameter_with_cast_to_nullable(bool async)
        {
            var boolean = false;
            await AssertQueryScalar(
                async,
                ss => ss.Set<Customer>().Select(c => boolean).OrderBy(e => (bool?)e),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_scalar(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => c.City));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_anonymous_one(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => new { c.City }),
                e => e.City);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_anonymous_two(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => new { c.City, c.Phone }),
                e => e.Phone);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_anonymous_three(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(
                    c => new
                    {
                        c.City,
                        c.Phone,
                        c.Country
                    }),
                e => e.Phone);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_anonymous_bool_constant_true(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => new { c.CustomerID, ConstantTrue = true }),
                e => e.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_anonymous_constant_in_expression(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => new { c.CustomerID, Expression = c.CustomerID.Length + 5 }),
                e => e.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_anonymous_conditional_expression(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Product>().Select(p => new { p.ProductID, IsAvailable = p.UnitsInStock > 0 }),
                e => e.ProductID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_customer_table(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>(),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_customer_identity(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => c),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_anonymous_with_object(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => new { c.City, c }),
                e => e.c.CustomerID,
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_anonymous_nested(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => new { c.City, Country = new { c.Country } }),
                e => e.City);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_anonymous_empty(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => new { }),
                e => 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_anonymous_literal(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => new { X = 10 }),
                e => e.X);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_constant_int(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Customer>().Select(c => 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_constant_null_string(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => (string)null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_local(bool async)
        {
            var x = 10;
            return AssertQueryScalar(
                async,
                ss => ss.Set<Customer>().Select(c => x));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_scalar_primitive(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Employee>().Select(e => e.EmployeeID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_scalar_primitive_after_take(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Employee>().Take(9).Select(e => e.EmployeeID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_project_filter(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      where c.City == "London"
                      select c.CompanyName);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_project_filter2(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      where c.City == "London"
                      select c.City);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_nested_collection(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>()
                    where c.City == "London"
                    orderby c.CustomerID
                    select ss.Set<Order>()
                        .Where(
                            o => o.CustomerID == c.CustomerID
                                && o.OrderDate.Value.Year == 1997)
                        .Select(o => o.OrderID)
                        .OrderBy(o => o)
                        .ToList(),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));
        }

        [ConditionalFact]
        public virtual void Select_nested_collection_multi_level()
        {
            using var context = CreateContext();
            var customers = context.Customers
                .Where(c => c.CustomerID.StartsWith("A"))
                .Select(
                    c => new
                    {
                        OrderDates = c.Orders
                            .Where(o => o.OrderID < 10500)
                            .Take(3)
                            .Select(
                                o => new { Date = o.OrderDate })
                    })
                .ToList();

            Assert.Equal(4, customers.Count);
            Assert.All(customers, t => Assert.True(t.OrderDates.Count() <= 3));
        }

        [ConditionalFact]
        public virtual void Select_nested_collection_multi_level2()
        {
            using var context = CreateContext();
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

        [ConditionalFact]
        public virtual void Select_nested_collection_multi_level3()
        {
            using var context = CreateContext();
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

        [ConditionalFact]
        public virtual void Select_nested_collection_multi_level4()
        {
            using var context = CreateContext();
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

        [ConditionalFact]
        public virtual void Select_nested_collection_multi_level5()
        {
            using var context = CreateContext();
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

        [ConditionalFact]
        public virtual void Select_nested_collection_multi_level6()
        {
            using var context = CreateContext();
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_nested_collection_count_using_anonymous_type(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))
                    .Select(c => new { c.Orders.Count }),
                e => e.Count);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_nested_collection_deep(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>()
                    where c.City == "London"
                    orderby c.CustomerID
                    select (from o1 in ss.Set<Order>()
                            where o1.CustomerID == c.CustomerID
                                && o1.OrderDate.Value.Year == 1997
                            orderby o1.OrderID
                            select (from o2 in ss.Set<Order>()
                                    where o1.CustomerID == c.CustomerID
                                    orderby o2.OrderID
                                    select o1.OrderID).ToList()).ToList(),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(
                    e,
                    a,
                    ordered: true,
                    elementAsserter: (ee, aa) => AssertCollection(ee, aa)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task New_date_time_in_anonymous_type_works(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      where c.CustomerID.StartsWith("A")
                      select new { A = new DateTime() },
                e => e.A);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_non_matching_value_types_int_to_long_introduces_explicit_cast(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>()
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)o.OrderID),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_non_matching_value_types_nullable_int_to_long_introduces_explicit_cast(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>()
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)o.EmployeeID),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_non_matching_value_types_nullable_int_to_int_doesnt_introduce_explicit_cast(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>()
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (uint)o.EmployeeID),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_non_matching_value_types_int_to_nullable_int_doesnt_introduce_explicit_cast(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>()
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (int?)o.OrderID),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_non_matching_value_types_from_binary_expression_introduces_explicit_cast(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>()
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)(o.OrderID + o.OrderID)),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_non_matching_value_types_from_binary_expression_nested_introduces_top_level_explicit_cast(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>()
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (short)(o.OrderID + (long)o.OrderID)),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast1(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>()
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)-o.OrderID),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast2(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>()
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => -((long)o.OrderID)),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_non_matching_value_types_from_length_introduces_explicit_cast(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>()
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)o.CustomerID.Length),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_non_matching_value_types_from_method_call_introduces_explicit_cast(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>()
                    .Where(o => o.CustomerID == "ALFKI")
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)Math.Abs(o.OrderID)),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_non_matching_value_types_from_anonymous_type_introduces_explicit_cast(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_conditional_with_null_comparison_in_test(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from o in ss.Set<Order>()
                      where o.CustomerID == "ALFKI"
                      select o.CustomerID == null ? true : o.OrderID < 100);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_over_10_nested_ternary_condition(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                os => from c in os.Set<Customer>()
                      select
                          c.CustomerID == "1"
                              ? "01"
                              : c.CustomerID == "2"
                                  ? "02"
                                  : c.CustomerID == "3"
                                      ? "03"
                                      : c.CustomerID == "4"
                                          ? "04"
                                          : c.CustomerID == "5"
                                              ? "05"
                                              : c.CustomerID == "6"
                                                  ? "06"
                                                  : c.CustomerID == "7"
                                                      ? "07"
                                                      : c.CustomerID == "8"
                                                          ? "08"
                                                          : c.CustomerID == "9"
                                                              ? "09"
                                                              : c.CustomerID == "10"
                                                                  ? "10"
                                                                  : c.CustomerID == "11"
                                                                      ? "11"
                                                                      : null);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_in_a_subquery_should_be_liftable(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Employee>().OrderBy(e => e.EmployeeID)
                    .Select(e => string.Format("{0}", e.EmployeeID))
                    .Skip(1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Reverse_changes_asc_order_to_desc(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Employee>().OrderBy(e => e.EmployeeID)
                    .Reverse()
                    .Select(e => $"{e.EmployeeID}"),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Reverse_changes_desc_order_to_asc(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Employee>().OrderByDescending(e => e.EmployeeID)
                    .Select(e => $"{e.EmployeeID}")
                    .Reverse(),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Reverse_without_explicit_ordering_throws(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Employee>().Reverse().Select(e => e.EmployeeID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_containing_DateTime_subtraction(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID < 10300)
                    .Select(o => o.OrderDate.Value - new DateTime(1997, 1, 1)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).Take(1).FirstOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_single_element_from_collection_with_OrderBy_Skip_and_FirstOrDefault(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).Skip(1).FirstOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Select(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).Distinct().FirstOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault_followed_by_projecting_length(
            bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Customer>()
                    .Select(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).Distinct().FirstOrDefault())
                    .Select(e => (int?)e.Length),
                ss => ss.Set<Customer>()
                    .Select(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).Distinct().FirstOrDefault())
                    .Select(e => e.MaybeScalar(e => e.Length)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_single_element_from_collection_with_OrderBy_Take_and_SingleOrDefault(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI")
                    .Select(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).Take(1).SingleOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault_with_parameter(bool async)
        {
            var i = 1;
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).Take(i).FirstOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(
                    c => c.Orders.OrderBy(o => o.OrderID)
                        .ThenByDescending(o => o.OrderDate)
                        .Select(o => o.CustomerID)
                        .Take(2)
                        .FirstOrDefault()));
        }

        [ConditionalTheory(Skip = "Issue#12597")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task
            Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_followed_by_projection_of_length_property(
                bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Customer>().Select(
                    c => c.Orders.OrderBy(o => o.OrderID)
                        .ThenByDescending(o => o.OrderDate)
                        .Select(o => o.CustomerID)
                        .Take(2)
                        .FirstOrDefault().Length));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(
                    c => c.Orders.OrderBy(o => o.CustomerID)
                        .ThenByDescending(o => o.OrderDate)
                        .Select(o => o.CustomerID)
                        .Take(2)
                        .FirstOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID < 10300)
                    .Select(o => o.OrderDetails.OrderBy(od => od.Product.ProductName).Select(od => od.OrderID).Take(1).FirstOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID < 10250)
                    .Select(o => o.OrderDetails.OrderBy(od => od.Product.ProductName).Take(1).FirstOrDefault()),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_datetime_year_component(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Select(o => o.OrderDate.Value.Year));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_datetime_month_component(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Select(o => o.OrderDate.Value.Month));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_datetime_day_of_year_component(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Select(o => o.OrderDate.Value.DayOfYear));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_datetime_day_component(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Select(o => o.OrderDate.Value.Day));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_datetime_hour_component(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Select(o => o.OrderDate.Value.Hour));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_datetime_minute_component(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Select(o => o.OrderDate.Value.Minute));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_datetime_second_component(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Select(o => o.OrderDate.Value.Second));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_datetime_millisecond_component(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Select(o => o.OrderDate.Value.Millisecond));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_datetime_DayOfWeek_component(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Select(o => (int)o.OrderDate.Value.DayOfWeek));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_datetime_Ticks_component(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Select(o => o.OrderDate.Value.Ticks));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_datetime_TimeOfDay_component(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Select(o => o.OrderDate.Value.TimeOfDay));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_byte_constant(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Customer>().Select(c => c.CustomerID == "ALFKI" ? (byte)1 : (byte)2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_short_constant(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Customer>().Select(c => c.CustomerID == "ALFKI" ? (short)1 : (short)2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_bool_constant(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Customer>().Select(c => c.CustomerID == "ALFKI" ? true : false));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Anonymous_projection_AsNoTracking_Selector(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Select(o => new { A = o.CustomerID, B = o.OrderDate })
                    .AsNoTracking() // Just to cause a subquery
                    .Select(e => e.B));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Anonymous_projection_with_repeated_property_being_ordered(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      orderby c.CustomerID
                      select new { A = c.CustomerID, B = c.CustomerID });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Anonymous_projection_with_repeated_property_being_ordered_2(bool async)
        {
            return AssertQuery(
                async,
                ss => from o in ss.Set<Order>()
                      orderby o.CustomerID
                      select new { A = o.Customer.CustomerID, B = o.CustomerID });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_GetValueOrDefault_on_DateTime(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Select(o => o.OrderDate.GetValueOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_GetValueOrDefault_on_DateTime_with_null_values(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from c in ss.Set<Customer>()
                      join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into grouping
                      from o in grouping.DefaultIfEmpty()
                      select o.OrderDate.GetValueOrDefault(new DateTime(1753, 1, 1)),
                ss => from c in ss.Set<Customer>()
                      join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into grouping
                      from o in grouping.DefaultIfEmpty()
                      select o != null ? o.OrderDate.Value : new DateTime(1753, 1, 1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Cast_on_top_level_projection_brings_explicit_Cast(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Select(o => (double?)o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Client_method_in_projection_requiring_materialization_1(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A")).Select(c => c.ToString()),
                entryCount: 4);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Client_method_in_projection_requiring_materialization_2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A")).Select(c => ClientMethod(c)),
                entryCount: 4);
        }

        private static string ClientMethod(Customer c)
            => c.CustomerID;

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projecting_nullable_struct(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Select(
                    o => new
                    {
                        One = o.CustomerID,
                        Two = o.CustomerID == "ALFKI"
                            ? new MyStruct { X = o.OrderID, Y = o.CustomerID.Length }
                            : (MyStruct?)null
                    }),
                elementSorter: e => (e.One, e.Two?.X));
        }

        public struct MyStruct
        {
            public int X, Y;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_select_many_with_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      from o in c.Orders
                      from od in o.OrderDetails
                      where od.Discount >= 0.25
                      select c,
                entryCount: 38);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_without_result_selector_naked_collection_navigation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().SelectMany(c => c.Orders),
                entryCount: 830);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_without_result_selector_collection_navigation_composed(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().SelectMany(c => c.Orders.Select(o => o.CustomerID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_correlated_with_outer_1(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      from o in ss.Set<Order>().Where(o => c.CustomerID == o.CustomerID).Select(o => c.City)
                      select new { c, o },
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_correlated_with_outer_2(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      from o in ss.Set<Order>().Where(o => c.CustomerID == o.CustomerID)
                          .OrderBy(o => c.City).ThenBy(o => o.OrderID).Take(2)
                      select new { c, o },
                elementSorter: e => (e.c.CustomerID, e.o.OrderID),
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.c, a.c);
                    AssertEqual(e.o, a.o);
                },
                entryCount: 266);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_correlated_with_outer_3(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      from o in ss.Set<Order>().Where(o => c.CustomerID == o.CustomerID).Select(o => c.City).DefaultIfEmpty()
                      select new { c, o },
                elementSorter: e => (e.c.CustomerID, e.o),
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.c, a.c);
                    Assert.Equal(e.o, a.o);
                },
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_correlated_with_outer_4(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      from o in ss.Set<Order>().Where(o => c.CustomerID == o.CustomerID)
                          .OrderBy(o => c.City).ThenBy(o => o.OrderID).Take(2).DefaultIfEmpty()
                      select new { c, o },
                elementSorter: e => (e.c.CustomerID, e.o?.OrderID),
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.c, a.c);
                    AssertEqual(e.o, a.o);
                },
                entryCount: 268);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_correlated_with_outer_5(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      from o in ss.Set<Order>().Where(o => c.CustomerID != o.CustomerID).Select(o => c.City).DefaultIfEmpty()
                      select new { c, o },
                elementSorter: e => (e.c.CustomerID, e.o),
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.c, a.c);
                    Assert.Equal(e.o, a.o);
                },
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_correlated_with_outer_6(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      from o in ss.Set<Order>().Where(o => c.CustomerID != o.CustomerID)
                          .OrderBy(o => c.City).ThenBy(o => o.OrderID).Take(2).DefaultIfEmpty()
                      select new { c, o },
                elementSorter: e => (e.c.CustomerID, e.o?.OrderID),
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.c, a.c);
                    AssertEqual(e.o, a.o);
                },
                entryCount: 94);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_correlated_with_outer_7(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      from o in ss.Set<Order>().Where(o => c.CustomerID.Length >= o.CustomerID.Length)
                          .OrderBy(o => c.City).ThenBy(o => o.OrderID).Take(2).DefaultIfEmpty()
                      select new { c, o },
                elementSorter: e => (e.c.CustomerID, e.o?.OrderID),
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.c, a.c);
                    AssertEqual(e.o, a.o);
                },
                entryCount: 93);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task FirstOrDefault_over_empty_collection_of_value_type_returns_correct_results(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      where c.CustomerID.Equals("FISSA")
                      select new { c.CustomerID, OrderId = c.Orders.OrderBy(o => o.OrderID).Select(o => o.OrderID).FirstOrDefault() });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_non_nullable_value_after_FirstOrDefault_on_empty_collection(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Customer>().Select(
                    c => (int?)ss.Set<Order>().Where(o => o.CustomerID == "John Doe").Select(o => o.CustomerID).FirstOrDefault().Length),
                ss => ss.Set<Customer>().Select(
                    c => ss.Set<Order>().Where(o => o.CustomerID == "John Doe").Select(o => o.CustomerID).FirstOrDefault()
                        .MaybeScalar(e => e.Length)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Member_binding_after_ctor_arguments_fails_with_client_eval(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => new CustomerListItem(c.CustomerID, c.City)).OrderBy(c => c.City),
                assertOrder: true);
        }

        protected class CustomerListItem
        {
            public CustomerListItem(string id, string city)
            {
                Id = id;
                City = city;
            }

            public string Id { get; }
            public string City { get; }

            public override bool Equals(object obj)
                => obj != null
                    && (ReferenceEquals(this, obj)
                        || obj is CustomerListItem customerListItem
                        && Id == customerListItem.Id
                        && City == customerListItem.City);

            public override int GetHashCode()
                => HashCode.Combine(Id, City);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Filtered_collection_projection_is_tracked(bool async)
        {
            using var context = CreateContext();
            var query = context.Customers
                .Where(c => c.CustomerID.StartsWith("A"))
                .Select(
                    c =>
                        new { Customer = c, FilteredOrders = c.Orders.Where(o => o.OrderID > 11000) });

            var result = async
                ? (await query.ToListAsync())
                : query.ToList();

            Assert.Equal(4, result.Count);
            Assert.True(result.All(r => (r.Customer.Orders?.Count ?? 0) == r.FilteredOrders.Count()));
            Assert.Equal(6, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Filtered_collection_projection_with_to_list_is_tracked(bool async)
        {
            using var context = CreateContext();
            var query = context.Customers
                .Where(c => c.CustomerID.StartsWith("A"))
                .Select(
                    c =>
                        new { Customer = c, FilteredOrders = c.Orders.Where(o => o.OrderID > 11000).ToList() });

            var result = async
                ? (await query.ToListAsync())
                : query.ToList();

            Assert.Equal(4, result.Count);
            Assert.True(result.All(r => (r.Customer.Orders?.Count ?? 0) == r.FilteredOrders.Count));
            Assert.Equal(6, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_with_collection_being_correlated_subquery_which_references_inner_and_outer_entity(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().SelectMany(
                    c => c.Orders.Select(o => new { OrderProperty = o.CustomerID, CustomerProperty = c.CustomerID })));
        }

        [ConditionalTheory(Skip = "issue #17763")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task
            SelectMany_with_collection_being_correlated_subquery_which_references_non_mapped_properties_from_inner_and_outer_entity(
                bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().SelectMany(
                    c => c.Orders.Select(o => new { OrderProperty = o.ShipName, CustomerProperty = c.ContactName })));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_with_complex_expression_that_can_be_funcletized(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI").Select(c => c.ContactName.IndexOf("")),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_chained_entity_navigation_doesnt_materialize_intermittent_entities(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().OrderBy(o => o.OrderID).Select(o => o.Customer.Orders),
                elementAsserter: (e, a) => AssertCollection(e, a),
                assertOrder: true,
                entryCount: 830);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_entity_compared_to_null(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from o in ss.Set<Order>()
                      where o.CustomerID == "ALFKI"
                      select o.Customer == null);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Explicit_cast_in_arithmetic_operation_is_preserved(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from o in ss.Set<Order>()
                      where o.OrderID == 10243
                      select (decimal)o.OrderID / (decimal)(o.OrderID + 1000));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_whose_selector_references_outer_source(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      from g in from o in ss.Set<Order>()
                                where c.CustomerID == o.CustomerID
                                select new { o.OrderDate, CustomerCity = c.City }
                      select g,
                elementSorter: e => (e.OrderDate, e.CustomerCity));
        }

        [ConditionalTheory(Skip = "Issue#12148")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_FirstOrDefault_with_entity_equality_check_in_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(
                    c => new { Order = (c.Orders.Any() ? c.Orders.FirstOrDefault() : null) == null ? null : new Order() }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_FirstOrDefault_with_nullable_unsigned_int_column(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Customer>().Select(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.EmployeeID).FirstOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ToList_Count_in_projection_works(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))
                    .Select(c => new { c, Count = c.Orders.ToList().Count() }),
                entryCount: 4,
                elementSorter: r => r.c.CustomerID,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.c, a.c);
                    Assert.Equal(e.Count, a.Count);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task LastOrDefault_member_access_in_projection_translates_to_server(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))
                    .Select(c => new { c, c.Orders.OrderByDescending(o => o.OrderID).LastOrDefault().OrderDate }),
                entryCount: 4);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_with_parameterized_constructor(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI").Select(c => new CustomerWrapper(c)),
                entryCount: 1,
                elementSorter: e => e.Customer.CustomerID,
                elementAsserter: (e, a) => Assert.Equal(e.Customer, a.Customer));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_with_parameterized_constructor_with_member_assignment(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI").Select(c => new CustomerWrapper(c) { City = c.City }),
                entryCount: 1,
                elementSorter: e => e.Customer.CustomerID,
                elementAsserter: (e, a) => Assert.Equal(e.Customer, a.Customer));
        }

        private class CustomerWrapper
        {
            public CustomerWrapper(Customer customer)
            {
                Customer = customer;
            }

            public string City { get; set; }
            public Customer Customer { get; }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_projection_AsNoTracking_OrderBy(bool async)
        {
            return AssertQuery(
                async,
                ss => (from c in ss.Set<Customer>()
                       select new { c.CustomerID, Orders = c.Orders.Select(o => o.OrderDate).ToList() })
                    .AsNoTracking()
                    .OrderBy(a => a.CustomerID),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.CustomerID, a.CustomerID);
                    AssertCollection(e.Orders, a.Orders, elementSorter: i => i, elementAsserter: (ie, ia) => Assert.Equal(ie, ia));
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Coalesce_over_nullable_uint(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Select(o => o.EmployeeID ?? 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_uint_through_collection_FirstOrDefault(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Customer>().Select(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault()).Select(e => e.EmployeeID),
                ss => ss.Set<Customer>().Select(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault())
                    .Select(e => e.MaybeScalar(x => x.EmployeeID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_keyless_entity_FirstOrDefault_without_orderby(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => ss.Set<CustomerQuery>().FirstOrDefault(cv => cv.CompanyName == c.CompanyName)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_AsEnumerable_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .OrderBy(c => c.CustomerID)
                    .Select(c => ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).AsEnumerable())
                    .Where(e => e.Where(o => o.OrderID < 11000).Count() > 0)
                    .Select(e => e.Where(o => o.OrderID < 10750)),
                assertOrder: true,
                entryCount: 18);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_custom_type_in_both_sides_of_ternary(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .OrderBy(c => c.CustomerID)
                    .Select(
                        c => c.City == "Seattle"
                            ? new IdName<string> { Id = "PAY", Name = "Pay" }
                            : new IdName<string> { Id = "REC", Name = "Receive" }),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Name, a.Name);
                });
        }

        private class IdName<T>
        {
            public T Id { get; set; }
            public string Name { get; set; }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projecting_multiple_collection_with_same_constant_works(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI")
                    .Select(c => new { O1 = c.Orders.Select(e => new { Value = 1 }), O2 = c.Orders.Select(e => new { AnotherValue = 1 }) }),
                assertOrder: true, //single element
                elementAsserter: (e, a) =>
                {
                    AssertCollection(e.O1, a.O1, ordered: true);
                    AssertCollection(e.O2, a.O2, ordered: true);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projecting_after_navigation_and_distinct_throws(bool async)
        {
            var filteredOrderIds = new[] { 10248, 10249, 10250 };

            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .Select(o => o.Customer)
                    .Distinct()
                    .Select(
                        c => new
                        {
                            c.CustomerID,
                            Orders = c.Orders.Where(x => filteredOrderIds.Contains(x.OrderID)).OrderBy(x => x.OrderID)
                                .Select(
                                    x => new
                                    {
                                        c.CustomerID,
                                        x.OrderID,
                                        x.OrderDate
                                    })
                        }),
                elementSorter: e => e.CustomerID,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.CustomerID, a.CustomerID);
                    AssertCollection(e.Orders, a.Orders, elementSorter: ee => ee.CustomerID);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Custom_projection_reference_navigation_PK_to_FK_optimization(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .Select(
                        o => new Order
                        {
                            OrderID = o.OrderID,
                            Customer = new Customer { CustomerID = o.Customer.CustomerID, City = o.Customer.City },
                            OrderDate = o.OrderDate
                        }),
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e, a);
                    AssertEqual(e.Customer, a.Customer);
                });
        }
    }
}
