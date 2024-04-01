// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

#pragma warning disable RCS1202 // Avoid NullReferenceException.

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

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
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  select c.CustomerID
                  into id
                  where id == "ALFKI"
                  select id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_when_arithmetic_expression_precedence(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Select(o => new { A = o.OrderID / (o.OrderID / 2), B = o.OrderID / o.OrderID / 2 }),
            e => (e.A, e.B));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_when_arithmetic_expressions(bool async)
        => AssertQuery(
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
            elementSorter: e => e.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_when_arithmetic_mixed(bool async)
        => AssertQuery(
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
            elementSorter: e => e.OrderID + " " + e.EmployeeID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_when_arithmetic_mixed_subqueries(bool async)
        => AssertQuery(
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
            elementSorter: e => (e.e2.EmployeeID, e.o2.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_when_null_value(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.Region));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_when_client_evald_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => string.Join(", ", c.Orders.Select(o => o.CustomerID).ToList())));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_to_object_array(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.EmployeeID == 1)
                .Select(e => new object[] { e.EmployeeID, e.ReportsTo, EF.Property<string>(e, "Title") }),
            elementAsserter: (e, a) => AssertArrays(e, a, 3));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_of_entity_type_into_object_array(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Where(c => c.CustomerID.StartsWith("A"))
                .Select(c => new object[] { c }),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_of_multiple_entity_types_into_object_array(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().OrderBy(o => o.OrderID).Where(o => o.OrderID < 10300)
                .Select(o => new object[] { o, o.Customer }),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_of_entity_type_into_object_list(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Select(c => new List<object> { c }),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_to_int_array(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.EmployeeID == 1)
                .Select(e => new[] { e.EmployeeID, e.ReportsTo }),
            elementAsserter: (e, a) => AssertArrays(e, a, 2));

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

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new { f = boolean }).OrderBy(e => (bool?)e.f),
            assertOrder: true);
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
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.City));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_anonymous_one(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new { c.City }),
            e => e.City);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_anonymous_two(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new { c.City, c.Phone }),
            e => e.Phone);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_anonymous_three(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(
                c => new
                {
                    c.City,
                    c.Phone,
                    c.Country
                }),
            e => e.Phone);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_anonymous_bool_constant_true(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new { c.CustomerID, ConstantTrue = true }),
            e => e.CustomerID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_anonymous_constant_in_expression(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new { c.CustomerID, Expression = c.CustomerID.Length + 5 }),
            e => e.CustomerID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_anonymous_conditional_expression(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>().Select(p => new { p.ProductID, IsAvailable = p.UnitsInStock > 0 }),
            e => e.ProductID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_customer_table(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_customer_identity(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_anonymous_with_object(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new { c.City, c }),
            e => e.c.CustomerID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_anonymous_nested(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new { c.City, Country = new { c.Country } }),
            e => e.City);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_anonymous_empty(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new { }),
            e => 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_anonymous_literal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new { X = 10 }),
            e => e.X);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_constant_int(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>().Select(c => 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_constant_null_string(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => (string)null));

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
        => AssertQueryScalar(
            async,
            ss => ss.Set<Employee>().Select(e => e.EmployeeID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_scalar_primitive_after_take(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Employee>().Take(9).Select(e => e.EmployeeID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_project_filter(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where c.City == "London"
                  select c.CompanyName);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_project_filter2(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where c.City == "London"
                  select c.City);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nested_collection(bool async)
        => AssertQuery(
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nested_collection_multi_level(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Where(c => c.CustomerID.StartsWith("A"))
                .Select(
                    c => new
                    {
                        OrderDates = c.Orders
                            .Where(o => o.OrderID < 10500)
                            .OrderBy(o => o.OrderID)
                            .Take(3)
                            .Select(o => new { Date = o.OrderDate })
                    }),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e.OrderDates, a.OrderDates, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nested_collection_multi_level2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Where(c => c.CustomerID.StartsWith("A"))
                .Select(
                    c => new
                    {
                        OrderDates = c.Orders
                            .OrderBy(o => o.OrderID)
                            .Where(o => o.OrderID < 10500)
                            .Select(o => o.OrderDate)
                            .FirstOrDefault()
                    }),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nested_collection_multi_level3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Where(c => c.CustomerID.StartsWith("A"))
                .Select(
                    c => new
                    {
                        OrderDates = ss.Set<Order>()
                            .OrderBy(o => o.OrderID)
                            .Where(o => o.OrderID < 10500)
                            .Where(o => c.CustomerID == o.CustomerID)
                            .Select(o => o.OrderDate)
                            .FirstOrDefault()
                    }),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nested_collection_multi_level4(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Where(c => c.CustomerID.StartsWith("A"))
                .Select(
                    c => new
                    {
                        Order = (int?)c.Orders
                            .OrderBy(o => o.OrderID)
                            .Where(o => o.OrderID < 10500)
                            .Select(
                                o => o.OrderDetails
                                    .Where(od => od.OrderID > 10)
                                    .Select(od => od.ProductID)
                                    .Count())
                            .FirstOrDefault()
                    }),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nested_collection_multi_level5(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Where(c => c.CustomerID.StartsWith("A"))
                .Select(
                    c => new
                    {
                        Order = (int?)c.Orders
                            .OrderBy(o => o.OrderID)
                            .Where(o => o.OrderID < 10500)
                            .Select(
                                o => o.OrderDetails
                                    .OrderBy(od => od.OrderID)
                                    .ThenBy(od => od.ProductID)
                                    .Where(od => od.OrderID != c.Orders.Count)
                                    .Select(od => od.ProductID)
                                    .FirstOrDefault())
                            .FirstOrDefault()
                    }),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nested_collection_multi_level6(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Where(c => c.CustomerID.StartsWith("A"))
                .Select(
                    c => new
                    {
                        Order = (int?)c.Orders
                            .OrderBy(o => o.OrderID)
                            .Where(o => o.OrderID < 10500)
                            .Select(
                                o => o.OrderDetails
                                    .OrderBy(od => od.OrderID)
                                    .ThenBy(od => od.ProductID)
                                    .Where(od => od.OrderID != c.CustomerID.Length)
                                    .Select(od => od.ProductID)
                                    .FirstOrDefault())
                            .FirstOrDefault()
                    }),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nested_collection_count_using_anonymous_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))
                .Select(c => new { c.Orders.Count }),
            e => e.Count);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nested_collection_deep(bool async)
        => AssertQuery(
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nested_collection_deep_distinct_no_identifiers(bool async)
        => AssertQuery(
            async,
            ss =>
                (from c in ss.Set<Customer>()
                 where c.City == "London"
                 orderby c.CustomerID
                 select new { c.City }).Distinct().Select(
                    x =>
                        ((from o1 in ss.Set<Order>()
                          where o1.CustomerID == x.City
                              && o1.OrderDate.Value.Year == 1997
                          orderby o1.OrderID
                          select o1).Distinct().Select(
                            xx =>
                                (from o2 in ss.Set<Order>()
                                 where xx.CustomerID == x.City
                                 orderby o2.OrderID
                                 select xx.OrderID).ToList()).ToList())),
            elementSorter: e => e.Count,
            elementAsserter: (e, a) => AssertCollection(
                e,
                a,
                ordered: true,
                elementAsserter: (ee, aa) => AssertCollection(ee, aa)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task New_date_time_in_anonymous_type_works(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where c.CustomerID.StartsWith("A")
                  select new { A = new DateTime() },
            e => e.A);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_non_matching_value_types_int_to_long_introduces_explicit_cast(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.CustomerID == "ALFKI")
                .OrderBy(o => o.OrderID)
                .Select(o => (long)o.OrderID),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_non_matching_value_types_nullable_int_to_long_introduces_explicit_cast(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.CustomerID == "ALFKI")
                .OrderBy(o => o.OrderID)
                .Select(o => (long)o.EmployeeID),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_non_matching_value_types_nullable_int_to_int_doesnt_introduce_explicit_cast(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.CustomerID == "ALFKI")
                .OrderBy(o => o.OrderID)
                .Select(o => (uint)o.EmployeeID),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_non_matching_value_types_int_to_nullable_int_doesnt_introduce_explicit_cast(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.CustomerID == "ALFKI")
                .OrderBy(o => o.OrderID)
                .Select(o => (int?)o.OrderID),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_non_matching_value_types_from_binary_expression_introduces_explicit_cast(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.CustomerID == "ALFKI")
                .OrderBy(o => o.OrderID)
                .Select(o => (long)(o.OrderID + o.OrderID)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_non_matching_value_types_from_binary_expression_nested_introduces_top_level_explicit_cast(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.CustomerID == "ALFKI")
                .OrderBy(o => o.OrderID)
                .Select(o => (short)(o.OrderID + (long)o.OrderID)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast1(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.CustomerID == "ALFKI")
                .OrderBy(o => o.OrderID)
                .Select(o => (long)-o.OrderID),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast2(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.CustomerID == "ALFKI")
                .OrderBy(o => o.OrderID)
                .Select(o => -((long)o.OrderID)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_non_matching_value_types_from_length_introduces_explicit_cast(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.CustomerID == "ALFKI")
                .OrderBy(o => o.OrderID)
                .Select(o => (long)o.CustomerID.Length),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_non_matching_value_types_from_method_call_introduces_explicit_cast(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.CustomerID == "ALFKI")
                .OrderBy(o => o.OrderID)
                .Select(o => (long)Math.Abs(o.OrderID)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_non_matching_value_types_from_anonymous_type_introduces_explicit_cast(bool async)
        => AssertQuery(
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_conditional_with_null_comparison_in_test(bool async)
        => AssertQueryScalar(
            async,
            ss => from o in ss.Set<Order>()
                  where o.CustomerID == "ALFKI"
                  select o.CustomerID == null ? true : o.OrderID < 100);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_over_10_nested_ternary_condition(bool isAsync)
        => AssertQuery(
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_in_a_subquery_should_be_liftable(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Employee>().OrderBy(e => e.EmployeeID)
                .Select(e => string.Format("{0}", e.EmployeeID))
                .Skip(1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Reverse_changes_asc_order_to_desc(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Employee>().OrderBy(e => e.EmployeeID)
                .Reverse()
                .Select(e => $"{e.EmployeeID}"),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Reverse_changes_desc_order_to_asc(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Employee>().OrderByDescending(e => e.EmployeeID)
                .Select(e => $"{e.EmployeeID}")
                .Reverse(),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Reverse_without_explicit_ordering(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Employee>().Reverse().Select(e => e.EmployeeID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Reverse_after_multiple_orderbys(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Employee>()
                .OrderBy(e => e.City)
                .OrderByDescending(e => e.EmployeeID)
                .Reverse()
                .Select(e => e.EmployeeID),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Reverse_after_orderby_thenby(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Employee>()
                .OrderBy(e => e.EmployeeID)
                .ThenByDescending(e => e.City)
                .Select(e => e.EmployeeID)
                .Reverse(),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Reverse_in_subquery_via_pushdown(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Employee>()
                .OrderBy(e => e.EmployeeID)
                .Reverse()
                .Take(5)
                .Distinct()
                .Select(e => new { e.EmployeeID, e.City }),
            elementSorter: e => e.EmployeeID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Reverse_after_orderBy_and_take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Employee>()
                .OrderBy(e => e.EmployeeID)
                .Take(5)
                .Reverse()
                .Select(e => new { e.EmployeeID, e.City }),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Reverse_in_join_outer(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderByDescending(c => c.City)
                .ThenBy(c => c.CustomerID)
                .Reverse()
                .Join(
                    ss.Set<Order>().OrderBy(o => o.OrderID),
                    o => o.CustomerID,
                    i => i.CustomerID,
                    (o, i) => new { o.CustomerID, i.OrderID }),
            elementSorter: e => (e.CustomerID, e.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Reverse_in_join_outer_with_take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderByDescending(c => c.CustomerID)
                .Reverse()
                .Take(20)
                .Join(
                    ss.Set<Order>().OrderBy(o => o.OrderID),
                    o => o.CustomerID,
                    i => i.CustomerID,
                    (o, i) => new { o.CustomerID, i.OrderID }),
            elementSorter: e => (e.CustomerID, e.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Reverse_in_join_inner(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>().OrderBy(x => x.CustomerID)
                  join o in ss.Set<Order>().OrderByDescending(x => x.OrderDate).Reverse() on c.CustomerID equals o.CustomerID into
                      grouping
                  from o in grouping.DefaultIfEmpty()
                  select new { c.CustomerID, OrderID = (int?)o.OrderID },
            elementSorter: e => (e.CustomerID, e.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Reverse_in_join_inner_with_skip(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>().OrderBy(x => x.CustomerID)
                  join o in ss.Set<Order>().OrderByDescending(x => x.OrderID).Skip(2).Reverse() on c.CustomerID equals o.CustomerID into
                      grouping
                  from o in grouping.DefaultIfEmpty()
                  select new { c.CustomerID, OrderID = (int?)o.OrderID },
            elementSorter: e => (e.CustomerID, e.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Reverse_in_SelectMany(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Reverse()
                .SelectMany(c => c.Orders.OrderByDescending(o => o.OrderID).Reverse()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Reverse_in_SelectMany_with_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Reverse()
                .Take(20)
                .SelectMany(c => c.Orders.OrderByDescending(o => o.OrderID).Take(30).Reverse()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Reverse_in_projection_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Select(c => ss.Set<Order>().OrderBy(o => o.OrderDate).ThenByDescending(o => o.OrderID).Reverse().ToList()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Reverse_in_projection_subquery_single_result(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Select(c => ss.Set<Order>().OrderBy(o => o.OrderDate).ThenByDescending(o => o.OrderID).Reverse().FirstOrDefault()),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Reverse_in_projection_scalar_subquery(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Select(
                    c => ss.Set<Order>().OrderBy(o => o.OrderDate).ThenByDescending(o => o.OrderID).Reverse().Select(o => o.OrderID)
                        .FirstOrDefault()),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_containing_DateTime_subtraction(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID < 10300)
                .Select(o => o.OrderDate.Value - new DateTime(1997, 1, 1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).Take(1).FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_single_element_from_collection_with_OrderBy_Skip_and_FirstOrDefault(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).Skip(1).FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Select(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).Distinct().FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault_followed_by_projecting_length(
        bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>()
                .Select(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).Distinct().FirstOrDefault())
                .Select(e => (int?)e.Length),
            ss => ss.Set<Customer>()
                .Select(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).Distinct().FirstOrDefault())
                .Select(e => e.MaybeScalar(e => e.Length)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_single_element_from_collection_with_OrderBy_Take_and_SingleOrDefault(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI")
                .Select(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).Take(1).SingleOrDefault()));

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
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(
                c => c.Orders.OrderBy(o => o.OrderID)
                    .ThenByDescending(o => o.OrderDate)
                    .Select(o => o.CustomerID)
                    .Take(2)
                    .FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task
        Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_followed_by_projection_of_length_property(
            bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>().Select(
                c => (int?)c.Orders.OrderBy(o => o.OrderID)
                    .ThenByDescending(o => o.OrderDate)
                    .Select(o => o.CustomerID)
                    .Take(2)
                    .FirstOrDefault().Length),
            ss => ss.Set<Customer>().Select(
                c => c.Orders.OrderBy(o => o.OrderID)
                    .ThenByDescending(o => o.OrderDate)
                    .Select(o => o.CustomerID)
                    .Take(2)
                    .FirstOrDefault().MaybeScalar(x => x.Length)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(
                c => c.Orders.OrderBy(o => o.CustomerID)
                    .ThenByDescending(o => o.OrderDate)
                    .Select(o => o.CustomerID)
                    .Take(2)
                    .FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID < 10300)
                .Select(o => o.OrderDetails.OrderBy(od => od.Product.ProductName).Select(od => od.OrderID).Take(1).FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID < 10250)
                .Select(o => o.OrderDetails.OrderBy(od => od.Product.ProductName).Take(1).FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_datetime_year_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderDate.Value.Year));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_datetime_month_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderDate.Value.Month));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_datetime_day_of_year_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderDate.Value.DayOfYear));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_datetime_day_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderDate.Value.Day));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_datetime_hour_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderDate.Value.Hour));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_datetime_minute_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderDate.Value.Minute));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_datetime_second_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderDate.Value.Second));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_datetime_millisecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderDate.Value.Millisecond));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_datetime_DayOfWeek_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Select(o => (int)o.OrderDate.Value.DayOfWeek));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_datetime_Ticks_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderDate.Value.Ticks));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_datetime_TimeOfDay_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderDate.Value.TimeOfDay));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_byte_constant(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>().Select(c => c.CustomerID == "ALFKI" ? (byte)1 : (byte)2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_short_constant(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>().Select(c => c.CustomerID == "ALFKI" ? (short)1 : (short)2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_bool_constant(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>().Select(c => c.CustomerID == "ALFKI" ? true : false));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Anonymous_projection_AsNoTracking_Selector(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Select(o => new { A = o.CustomerID, B = o.OrderDate })
                .AsNoTracking() // Just to cause a subquery
                .Select(e => e.B));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Anonymous_projection_with_repeated_property_being_ordered(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  orderby c.CustomerID
                  select new { A = c.CustomerID, B = c.CustomerID });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Anonymous_projection_with_repeated_property_being_ordered_2(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<Order>()
                  orderby o.CustomerID
                  select new { A = o.Customer.CustomerID, B = o.CustomerID });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_GetValueOrDefault_on_DateTime(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderDate.GetValueOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_GetValueOrDefault_on_DateTime_with_null_values(bool async)
        => AssertQueryScalar(
            async,
            ss => from c in ss.Set<Customer>()
                  join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into grouping
                  from o in grouping.DefaultIfEmpty()
                  select o.OrderDate.GetValueOrDefault(new DateTime(1753, 1, 1)),
            ss => from c in ss.Set<Customer>()
                  join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into grouping
                  from o in grouping.DefaultIfEmpty()
                  select o != null ? o.OrderDate.Value : new DateTime(1753, 1, 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Cast_on_top_level_projection_brings_explicit_Cast(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Select(o => (double?)o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Client_method_in_projection_requiring_materialization_1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A")).Select(c => c.ToString()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Client_method_in_projection_requiring_materialization_2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A")).Select(c => ClientMethod(c)));

    private static string ClientMethod(Customer c)
        => c.CustomerID;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_nullable_struct(bool async)
        => AssertQuery(
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

    public struct MyStruct
    {
        public int X, Y;
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_select_many_with_predicate(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  from o in c.Orders
                  from od in o.OrderDetails
                  where od.Discount >= 0.25
                  select c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_without_result_selector_naked_collection_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().SelectMany(c => c.Orders));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_without_result_selector_collection_navigation_composed(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().SelectMany(c => c.Orders.Select(o => o.CustomerID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_correlated_with_outer_1(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  from o in ss.Set<Order>().Where(o => c.CustomerID == o.CustomerID).Select(o => c.City)
                  select new { c, o });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_correlated_with_outer_2(bool async)
        => AssertQuery(
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
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_correlated_with_outer_3(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  from o in ss.Set<Order>().Where(o => c.CustomerID == o.CustomerID).Select(o => c.City).DefaultIfEmpty()
                  select new { c, o },
            elementSorter: e => (e.c.CustomerID, e.o),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.c, a.c);
                Assert.Equal(e.o, a.o);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_correlated_with_outer_4(bool async)
        => AssertQuery(
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
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_correlated_with_outer_5(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  from o in ss.Set<Order>().Where(o => c.CustomerID != o.CustomerID).Select(o => c.City).DefaultIfEmpty()
                  select new { c, o },
            elementSorter: e => (e.c.CustomerID, e.o),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.c, a.c);
                Assert.Equal(e.o, a.o);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_correlated_with_outer_6(bool async)
        => AssertQuery(
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
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_correlated_with_outer_7(bool async)
        => AssertQuery(
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
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FirstOrDefault_over_empty_collection_of_value_type_returns_correct_results(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where c.CustomerID.Equals("FISSA")
                  select new { c.CustomerID, OrderId = c.Orders.OrderBy(o => o.OrderID).Select(o => o.OrderID).FirstOrDefault() });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_non_nullable_value_after_FirstOrDefault_on_empty_collection(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>().Select(
                c => (int?)ss.Set<Order>().Where(o => o.CustomerID == "John Doe").Select(o => o.CustomerID).FirstOrDefault().Length),
            ss => ss.Set<Customer>().Select(
                c => ss.Set<Order>().Where(o => o.CustomerID == "John Doe").Select(o => o.CustomerID).FirstOrDefault()
                    .MaybeScalar(e => e.Length)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Member_binding_after_ctor_arguments_fails_with_client_eval(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new CustomerListItem(c.CustomerID, c.City)).OrderBy(c => c.City),
            assertOrder: true);

    protected class CustomerListItem(string id, string city)
    {
        public string Id { get; } = id;
        public string City { get; } = city;

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
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().SelectMany(
                c => c.Orders.Select(o => new { OrderProperty = o.CustomerID, CustomerProperty = c.CustomerID })));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task
        SelectMany_with_collection_being_correlated_subquery_which_references_non_mapped_properties_from_inner_and_outer_entity(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().SelectMany(
                c => c.Orders.Select(o => new { OrderProperty = o.ShipName, CustomerProperty = c.ContactName })));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_with_complex_expression_that_can_be_funcletized(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI").Select(c => c.ContactName.IndexOf("")),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_chained_entity_navigation_doesnt_materialize_intermittent_entities(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().OrderBy(o => o.OrderID).Select(o => o.Customer.Orders),
            elementAsserter: (e, a) => AssertCollection(e, a),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_entity_compared_to_null(bool async)
        => AssertQueryScalar(
            async,
            ss => from o in ss.Set<Order>()
                  where o.CustomerID == "ALFKI"
                  select o.Customer == null);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Explicit_cast_in_arithmetic_operation_is_preserved(bool async)
        => AssertQueryScalar(
            async,
            ss => from o in ss.Set<Order>()
                  where o.OrderID == 10250
                  select o.OrderID / (decimal)(o.OrderID + 1000),
            asserter: (e, a) => Assert.InRange(a, e - 0.0001M, e + 0.0001M));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_whose_selector_references_outer_source(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  from g in from o in ss.Set<Order>()
                            where c.CustomerID == o.CustomerID
                            select new { o.OrderDate, CustomerCity = c.City }
                  select g,
            elementSorter: e => (e.OrderDate, e.CustomerCity));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_FirstOrDefault_with_entity_equality_check_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(
                c => new { Order = (c.Orders.Any() ? c.Orders.FirstOrDefault() : null) == null ? null : new Order() }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_FirstOrDefault_with_nullable_unsigned_int_column(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>().Select(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.EmployeeID).FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ToList_Count_in_projection_works(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))
                .Select(c => new { c, Count = c.Orders.ToList().Count() }),
            elementSorter: r => r.c.CustomerID,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.c, a.c);
                Assert.Equal(e.Count, a.Count);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task LastOrDefault_member_access_in_projection_translates_to_server(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))
                .Select(c => new { c, c.Orders.OrderByDescending(o => o.OrderID).LastOrDefault().OrderDate }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_with_parameterized_constructor(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI").Select(c => new CustomerWrapper(c)),
            elementSorter: e => e.Customer.CustomerID,
            elementAsserter: (e, a) => Assert.Equal(e.Customer, a.Customer));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_with_parameterized_constructor_with_member_assignment(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI").Select(c => new CustomerWrapper(c) { City = c.City }),
            elementSorter: e => e.Customer.CustomerID,
            elementAsserter: (e, a) => Assert.Equal(e.Customer, a.Customer));

    private class CustomerWrapper(Customer customer)
    {
        public string City { get; set; }
        public Customer Customer { get; } = customer;
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_projection_AsNoTracking_OrderBy(bool async)
        => AssertQuery(
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Coalesce_over_nullable_uint(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Select(o => o.EmployeeID ?? 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_uint_through_collection_FirstOrDefault(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>().Select(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault()).Select(e => e.EmployeeID),
            ss => ss.Set<Customer>().Select(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault())
                .Select(e => e.MaybeScalar(x => x.EmployeeID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_keyless_entity_FirstOrDefault_without_orderby(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => ss.Set<CustomerQuery>().FirstOrDefault(cv => cv.CompanyName == c.CompanyName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_AsEnumerable_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.CustomerID.StartsWith("A"))
                .OrderBy(c => c.CustomerID)
                .Select(c => ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).AsEnumerable())
                .Where(e => e.Where(o => o.OrderID < 11000).Count() > 0)
                .Select(e => e.Where(o => o.OrderID < 10750)),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_custom_type_in_both_sides_of_ternary(bool async)
        => AssertQuery(
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

    private class IdName<T>
    {
        public T Id { get; set; }
        public string Name { get; set; }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_multiple_collection_with_same_constant_works(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI")
                .Select(c => new { O1 = c.Orders.Select(e => new { Value = 1 }), O2 = c.Orders.Select(e => new { AnotherValue = 1 }) }),
            assertOrder: true, //single element
            elementAsserter: (e, a) =>
            {
                AssertCollection(e.O1, a.O1, ordered: true);
                AssertCollection(e.O2, a.O2, ordered: true);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_after_navigation_and_distinct(bool async)
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
    public virtual Task Correlated_collection_after_distinct_with_complex_projection_containing_original_identifier(bool async)
    {
        var filteredOrderIds = new[] { 10248, 10249, 10250 };

        return AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Select(o => new { o.OrderID, Complex = o.OrderDate.Value.Month })
                .Distinct()
                .Select(
                    c => new
                    {
                        c.OrderID,
                        c.Complex,
                        Subquery = (from x in ss.Set<Order>()
                                    where x.OrderID == c.OrderID && filteredOrderIds.Contains(x.OrderID)
                                    select new
                                    {
                                        Outer = c.OrderID,
                                        Inner = x.OrderID,
                                        x.OrderDate
                                    }).ToList()
                    }),
            elementSorter: e => e.OrderID,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.OrderID, a.OrderID);
                Assert.Equal(e.Complex, a.Complex);
                AssertCollection(e.Subquery, a.Subquery, elementSorter: ee => ee.Outer);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_after_distinct_not_containing_original_identifier(bool async)
    {
        var filteredOrderIds = new[] { 10248, 10249, 10250 };

        return AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Select(o => new { o.OrderDate, o.CustomerID })
                .Distinct()
                .Select(
                    c => new
                    {
                        c.OrderDate,
                        c.CustomerID,
                        Subquery = (from x in ss.Set<Order>()
                                    where x.CustomerID == c.CustomerID && filteredOrderIds.Contains(x.OrderID)
                                    select new
                                    {
                                        Outer1 = c.OrderDate,
                                        Outer2 = c.CustomerID,
                                        Inner = x.OrderID,
                                        x.OrderDate
                                    }).ToList()
                    }),
            elementSorter: e => (e.OrderDate, e.CustomerID),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.OrderDate, a.OrderDate);
                Assert.Equal(e.CustomerID, a.CustomerID);
                AssertCollection(e.Subquery, a.Subquery, elementSorter: ee => (ee.Outer1, ee.Outer2, ee.Inner, ee.OrderDate));
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_after_distinct_with_complex_projection_not_containing_original_identifier(bool async)
    {
        var filteredOrderIds = new[] { 10248, 10249, 10250 };

        return AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Select(
                    o => new
                    {
                        o.OrderDate,
                        o.CustomerID,
                        Complex = o.OrderDate.Value.Month
                    })
                .Distinct()
                .Select(
                    c => new
                    {
                        c.OrderDate,
                        c.CustomerID,
                        c.Complex,
                        Subquery = (from x in ss.Set<Order>()
                                    where x.CustomerID == c.CustomerID && filteredOrderIds.Contains(x.OrderID)
                                    select new
                                    {
                                        Outer1 = c.OrderDate,
                                        Outer2 = c.CustomerID,
                                        Outer3 = c.Complex,
                                        Inner = x.OrderID,
                                        x.OrderDate
                                    }).ToList()
                    }),
            elementSorter: e => (e.OrderDate, e.CustomerID, e.Complex),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.OrderDate, a.OrderDate);
                Assert.Equal(e.CustomerID, a.CustomerID);
                Assert.Equal(e.Complex, a.Complex);
                AssertCollection(
                    e.Subquery, a.Subquery, elementSorter: ee => (ee.Outer1, ee.Outer2, ee.Outer3, ee.Inner, ee.OrderDate));
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_after_groupby_with_complex_projection_containing_original_identifier(bool async)
    {
        var filteredOrderIds = new[] { 10248, 10249, 10250 };

        return AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => new { o.OrderID, Complex = o.OrderDate.Value.Month })
                .Select(g => new { g.Key, Aggregate = g.Count() })
                .Select(
                    c => new
                    {
                        c.Key.OrderID,
                        c.Key.Complex,
                        Subquery = (from x in ss.Set<Order>()
                                    where x.OrderID == c.Key.OrderID && filteredOrderIds.Contains(x.OrderID)
                                    select new
                                    {
                                        Outer = c.Key.OrderID,
                                        Inner = x.OrderID,
                                        x.OrderDate
                                    }).ToList()
                    }),
            elementSorter: e => e.OrderID,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.OrderID, a.OrderID);
                Assert.Equal(e.Complex, a.Complex);
                AssertCollection(e.Subquery, a.Subquery, elementSorter: ee => ee.Outer);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Custom_projection_reference_navigation_PK_to_FK_optimization(bool async)
        => AssertQuery(
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_Length_of_a_string_property_after_FirstOrDefault_on_correlated_collection(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Select(c => (int?)c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).FirstOrDefault().Length),
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Select(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).FirstOrDefault().MaybeScalar(x => x.Length)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_count_of_navigation_which_is_generic_list(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Select(c => c.Orders.Count),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_count_of_navigation_which_is_generic_collection(bool async)
    {
        var collectionCount = typeof(ICollection<Order>).GetProperty("Count");

        var prm = Expression.Parameter(typeof(Customer), "c");
        var selector = Expression.Lambda<Func<Customer, int>>(
            Expression.Property(
                Expression.Property(prm, "Orders"),
                collectionCount),
            prm);

        return AssertQueryScalar(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Select(selector),
            assertOrder: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_count_of_navigation_which_is_generic_collection_using_convert(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Select(c => ((ICollection<Order>)c.Orders).Count),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_take_projection_doesnt_project_intermittent_column(bool async)
        => AssertQuery(
            async,
            ss => ss
                .Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Select(
                    c => new
                    {
                        c.CustomerID,
                        c.City,
                        c.CompanyName
                    })
                .Take(10)
                .Select(x => new { Aggregate = x.CustomerID + " " + x.City }),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Do_not_erase_projection_mapping_when_adding_single_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10350)
                .Include(e => e.OrderDetails).ThenInclude(e => e.Product)
                .Select(
                    o => new
                    {
                        o.OrderID,
                        Order = o,
                        Property1 = o.OrderDetails.FirstOrDefault(e => e.UnitPrice > 10),
                        Property2 = o.OrderDetails.Where(e => e.UnitPrice < 10),
                    }),
            elementSorter: e => e.OrderID,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.OrderID, a.OrderID);
                AssertInclude(
                    e.Order, a.Order,
                    new ExpectedInclude<Order>(e => e.OrderDetails),
                    new ExpectedInclude<OrderDetail>(e => e.Product, "OrderDetails"));
                AssertInclude(e.Property1, a.Property1, new ExpectedInclude<OrderDetail>(e => e.Product));
                AssertCollection(
                    e.Property2, a.Property2,
                    elementAsserter: (ei, ai) => AssertInclude(ei, ai, new ExpectedInclude<OrderDetail>(e => e.Product)));
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_skip_projection_doesnt_project_intermittent_column(bool async)
        => AssertQuery(
            async,
            ss => ss
                .Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Select(
                    c => new
                    {
                        c.CustomerID,
                        c.City,
                        c.CompanyName
                    })
                .Skip(7)
                .Select(x => new { Aggregate = x.CustomerID + " " + x.City }),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_Distinct_projection_preserves_columns_used_for_distinct_in_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss
                .Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Select(
                    c => new
                    {
                        c.CustomerID,
                        FirstLetter = c.CustomerID.Substring(0, 1),
                        Foo = "Foo"
                    })
                .Distinct()
                .Select(x => new { Aggregate = x.FirstLetter + " " + x.Foo }),
            elementSorter: e => e.Aggregate);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_take_predicate_projection(bool async)
        => AssertQuery(
            async,
            ss => ss
                .Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Select(
                    c => new
                    {
                        c.CustomerID,
                        c.City,
                        c.CompanyName
                    })
                .Take(10)
                .Where(x => x.CustomerID.StartsWith("A"))
                .Select(x => new { Aggregate = x.CustomerID + " " + x.City }),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Ternary_in_client_eval_assigns_correct_types(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .OrderBy(e => e.OrderID)
                .Select(
                    o => new
                    {
                        CustomerID = ClientMethod(o.CustomerID),
                        OrderDate = o.OrderDate.HasValue ? o.OrderDate.Value : new DateTime(o.OrderID - 10000, 1, 1),
                        OrderDate2 = o.OrderDate.HasValue == false ? new DateTime(o.OrderID - 10000, 1, 1) : o.OrderDate.Value
                    }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.CustomerID, a.CustomerID);
                AssertEqual(e.OrderDate, a.OrderDate);
                AssertEqual(e.OrderDate2, a.OrderDate2);
            });

    private static string ClientMethod(string s)
        => s;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task VisitLambda_should_not_be_visited_trivially(bool async)
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss =>
                {
                    var orders = ss.Set<Order>().Where(o => o.CustomerID.StartsWith("A")).ToList();

                    return ss.Set<Customer>()
                        .Select(c => new { Customer = c, HasOrder = orders.Any(o => o.CustomerID == c.CustomerID) });
                },
                elementSorter: e => e.Customer.CustomerID,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.Customer, a.Customer);
                    AssertEqual(e.HasOrder, a.HasOrder);
                }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_after_groupby_with_complex_projection_not_containing_original_identifier(bool async)
    {
        var filteredOrderIds = new[] { 10248, 10249, 10250 };

        return AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => new { o.CustomerID, Complex = o.OrderDate.Value.Month })
                .Select(g => new { g.Key, Aggregate = g.Count() })
                .Select(
                    c => new
                    {
                        c.Key.CustomerID,
                        c.Key.Complex,
                        Subquery = (from x in ss.Set<Order>()
                                    where x.CustomerID == c.Key.CustomerID && filteredOrderIds.Contains(x.OrderID)
                                    select new
                                    {
                                        Outer = c.Key.CustomerID,
                                        Inner = x.OrderID,
                                        x.OrderDate
                                    }).ToList()
                    }),
            elementSorter: e => (e.CustomerID, e.Complex),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.CustomerID, a.CustomerID);
                Assert.Equal(e.Complex, a.Complex);
                AssertCollection(e.Subquery, a.Subquery, elementSorter: ee => ee.Outer);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_include_over_result_of_single_non_scalar(bool async)
        => AssertQuery(
            async,
            ss =>
                ss.Set<Customer>().Include(c => c.Orders).ThenInclude(o => o.OrderDetails)
                    .Where(c => c.CustomerID.StartsWith("F"))
                    .Select(c => new { c, SingleOrder = c.Orders.OrderBy(o => o.OrderDate).FirstOrDefault() }),
            elementSorter: e => e.c.CustomerID,
            elementAsserter: (e, a) =>
            {
                AssertInclude(
                    e.c, a.c,
                    new ExpectedInclude<Customer>(c => c.Orders),
                    new ExpectedInclude<Order>(o => o.OrderDetails, "Orders"));
                AssertInclude(e.SingleOrder, a.SingleOrder, new ExpectedInclude<Order>(o => o.OrderDetails));
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_projection_selecting_outer_element_followed_by_take(bool async)
        => AssertQuery(
            async,
            ss =>
                ss.Set<Customer>().Include(c => c.Orders)
                    .Where(c => c.CustomerID.StartsWith("F"))
                    .OrderBy(e => e.CustomerID)
                    .Select(c => new { Customer = c.Orders.Select(o => c) })
                    .Take(10),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertCollection(
                    e.Customer, a.Customer,
                    elementAsserter: (ee, aa) => AssertInclude(ee, aa, new ExpectedInclude<Customer>(i => i.Orders)));
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_on_top_level_and_on_collection_projection_with_outer_apply(bool async)
        => AssertFirstOrDefault(
            async,
            ss =>
                ss.Set<Order>()
                    .Where(o => o.CustomerID.StartsWith("F"))
                    .Select(
                        o => new Order
                        {
                            OrderID = o.OrderID,
                            OrderDate = o.OrderDate,
                            OrderDetails = o.OrderDetails.Select(
                                    e => new OrderDetail
                                    {
                                        OrderID = e.OrderID,
                                        Product = e.Product,
                                        UnitPrice = e.UnitPrice
                                    })
                                .OrderByDescending(e => e.OrderID)
                                .Skip(0)
                                .Take(10)
                                .ToList()
                        }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_on_correlated_collection_in_first(bool async)
        => AssertFirstOrDefault(
            async,
            ss =>
                ss.Set<Customer>()
                    .Where(o => o.CustomerID.StartsWith("F"))
                    .OrderBy(e => e.CustomerID)
                    .Select(
                        o => new
                        {
                            Orders = o.Orders.OrderBy(a => a.OrderDate).Take(1)
                                .Select(e => new { Title = e.CustomerID == e.Customer.CustomerID ? "A" : "B" }).ToList()
                        }),
            asserter: (e, a) => AssertCollection(
                e.Orders, a.Orders, ordered: true,
                elementAsserter: (ee, aa) => AssertEqual(ee.Title, aa.Title)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Client_projection_via_ctor_arguments(bool async)
        => AssertSingle(
            async,
            ss =>
                ss.Set<Customer>()
                    .Where(c => c.CustomerID == "ALFKI")
                    .Include(c => c.Orders)
                    .Select(
                        c => new CustomerDetailsWithCount(
                            c.CustomerID, c.City,
                            c.Orders.Select(o => new OrderInfo(o.OrderID, o.OrderDate)).ToList(), c.Orders.Count)),
            asserter: (e, a) =>
            {
                Assert.Equal(e.CustomerID, a.CustomerID);
                Assert.Equal(e.City, a.City);
                AssertCollection(
                    e.OrderInfos, a.OrderInfos,
                    elementSorter: i => i.OrderID,
                    elementAsserter: (ie, ia) =>
                    {
                        Assert.Equal(ie.OrderID, ia.OrderID);
                        Assert.Equal(ie.OrderDate, ia.OrderDate);
                    });
                Assert.Equal(e.OrderCount, a.OrderCount);
            });

    private class CustomerDetailsWithCount(string customerID, string city, List<OrderInfo> orderInfos, int orderCount)
    {
        public string CustomerID { get; } = customerID;
        public string City { get; } = city;
        public List<OrderInfo> OrderInfos { get; } = orderInfos;
        public int OrderCount { get; } = orderCount;
    }

    private class OrderInfo(int orderID, DateTime? orderDate)
    {
        public int OrderID { get; } = orderID;
        public DateTime? OrderDate { get; } = orderDate;
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Client_projection_with_string_initialization_with_scalar_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.CustomerID.StartsWith("F"))
                .Select(
                    c => new
                    {
                        c.CustomerID,
                        Order = c.Orders.FirstOrDefault(o => o.OrderID < 11000).OrderDate,
                        InterpolatedString = $"test{c.City}",
                        NonInterpolatedString = "test" + c.City,
                        Collection = new List<string>
                        {
                            $"{c.CustomerID}@test1.com",
                            $"{c.CustomerID}@test2.com",
                            $"{c.CustomerID}@test3.com",
                            $"{c.CustomerID}@test4.com"
                        }
                    }),
            ss => ss.Set<Customer>()
                .Where(c => c.CustomerID.StartsWith("F"))
                .Select(
                    c => new
                    {
                        c.CustomerID,
                        Order = c.Orders.FirstOrDefault(o => o.OrderID < 11000).MaybeScalar(e => e.OrderDate),
                        InterpolatedString = $"test{c.City}",
                        NonInterpolatedString = "test" + c.City,
                        Collection = new List<string>
                        {
                            $"{c.CustomerID}@test1.com",
                            $"{c.CustomerID}@test2.com",
                            $"{c.CustomerID}@test3.com",
                            $"{c.CustomerID}@test4.com"
                        }
                    }),
            elementSorter: e => e.CustomerID,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.CustomerID, a.CustomerID);
                AssertEqual(e.Order, a.Order);
                AssertEqual(e.InterpolatedString, a.InterpolatedString);
                AssertEqual(e.NonInterpolatedString, a.NonInterpolatedString);
                AssertCollection(e.Collection, a.Collection, ordered: true);
            });

    private class OrderDto;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task MemberInit_in_projection_without_arguments(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.CustomerID.StartsWith("F"))
                .Select(c => new { c.CustomerID, Orders = c.Orders.Select(o => new OrderDto()) }),
            elementSorter: e => e.CustomerID,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.CustomerID, a.CustomerID);
                AssertEqual(e.Orders.Count(), a.Orders.Count());
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task List_of_list_of_anonymous_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.CustomerID.StartsWith("F"))
                .Select(
                    c => new
                    {
                        c.CustomerID,
                        ListWithSubList = c.Orders.OrderBy(e => e.OrderID)
                            .Select(o => o.OrderDetails.Select(e => new { e.OrderID, e.ProductID }))
                    }),
            elementSorter: e => e.CustomerID,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.CustomerID, a.CustomerID);
                AssertCollection(
                    e.ListWithSubList, a.ListWithSubList, ordered: true,
                    elementAsserter: (ee, aa) => AssertCollection(ee, aa, elementSorter: i => (i.OrderID, i.ProductID)));
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Using_enumerable_parameter_in_projection(bool async)
    {
        var customersToLoad = new List<string> { "A" };
        var results = new List<OrderDto>();

        return AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.CustomerID.StartsWith("F"))
                .Select(
                    c => new
                    {
                        c.CustomerID,
                        Orders = customersToLoad.Contains("FISSA")
                            ? c.Orders.Select(e => new OrderDto())
                            : results
                    }),
            elementSorter: e => e.CustomerID,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.CustomerID, a.CustomerID);
                AssertEqual(e.Orders.Count(), a.Orders.Count());
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task List_from_result_of_single_result(bool async)
        => AssertFirstOrDefault(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Select(c => c.Orders.Select(e => e.OrderID)),
            asserter: (e, a) => AssertCollection(e, a, elementSorter: e => e, elementAsserter: (ee, aa) => AssertEqual(ee, aa)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task List_from_result_of_single_result_2(bool async)
        => AssertFirstOrDefault(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Select(c => c.Orders.Select(e => new { e.OrderID, e.OrderDate })),
            asserter: (e, a) => AssertCollection(
                e, a, elementSorter: e => e.OrderID,
                elementAsserter: (ee, aa) =>
                {
                    AssertEqual(ee.OrderID, aa.OrderID);
                    AssertEqual(ee.OrderDate, aa.OrderDate);
                }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task List_from_result_of_single_result_3(bool async)
        => AssertFirstOrDefault(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Select(
                    c => c.Orders.OrderBy(o => o.OrderDate)
                        .Select(e => e.OrderDetails.Select(od => od.ProductID)).FirstOrDefault()),
            asserter: (e, a) => AssertCollection(e, a, elementSorter: e => e, elementAsserter: (ee, aa) => AssertEqual(ee, aa)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_passed_to_DTO_constructor_works(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(x => new CustomerDtoWithEntityInCtor(x)),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) => Assert.Equal(e.Id, a.Id));

    public class CustomerDtoWithEntityInCtor(Customer customer)
    {
        public string Id { get; } = customer.CustomerID;
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Set_operation_in_pending_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(x => x.CustomerID)
                .Select(x => new
                {
                    OrderIds = (from o1 in ss.Set<Order>()
                                where o1.CustomerID == x.CustomerID
                                select o1.OrderID)
                        .Union(from o2 in ss.Set<Order>()
                               where o2.CustomerID == x.CustomerID
                               select o2.OrderID)
                    .ToList()
                }).Take(5),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e.OrderIds, a.OrderIds, elementSorter: ee => ee));
}
