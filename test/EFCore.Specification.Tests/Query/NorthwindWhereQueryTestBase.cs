// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

// ReSharper disable ConvertToConstant.Local
// ReSharper disable RedundantBoolCompare
// ReSharper disable InconsistentNaming

public abstract class NorthwindWhereQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    protected NorthwindWhereQueryTestBase(TFixture fixture)
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
    public virtual Task Where_simple(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == "London"));

    private static readonly Expression<Func<Order, bool>> _filter = o => o.CustomerID == "ALFKI";

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_as_queryable_expression(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.AsQueryable().Any(_filter)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task<string> Where_simple_closure(bool async)
    {
        var city = "London";

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == city));

        using var context = CreateContext();
        return context.Set<Customer>().Where(c => c.City == city).ToQueryString();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_indexer_closure(bool async)
    {
        var cities = new[] { "London" };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == cities[0]));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_dictionary_key_access_closure(bool async)
    {
        var predicateMap = new Dictionary<string, string> { ["City"] = "London" };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == predicateMap["City"]));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_tuple_item_closure(bool async)
    {
        var predicateTuple = new Tuple<string, string>("ALFKI", "London");

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == predicateTuple.Item2));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_named_tuple_item_closure(bool async)
    {
        (string CustomerID, string City) predicateTuple = ("ALFKI", "London");

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == predicateTuple.City));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_simple_closure_constant(bool async)
    {
        var predicate = true;

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => predicate));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_simple_closure_via_query_cache(bool async)
    {
        var city = "London";

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == city));

        city = "Seattle";

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == city));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_method_call_nullable_type_closure_via_query_cache(bool async)
    {
        var city = new City { Int = 2 };

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.ReportsTo == city.Int));

        city.Int = 5;

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.ReportsTo == city.Int));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_method_call_nullable_type_reverse_closure_via_query_cache(bool async)
    {
        var city = new City { NullableInt = 1 };

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.EmployeeID > city.NullableInt));

        city.NullableInt = 5;

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.EmployeeID > city.NullableInt));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_method_call_closure_via_query_cache(bool async)
    {
        var city = new City { InstanceFieldValue = "London" };

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == city.GetCity()));

        city.InstanceFieldValue = "Seattle";

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == city.GetCity()));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_field_access_closure_via_query_cache(bool async)
    {
        var city = new City { InstanceFieldValue = "London" };

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == city.InstanceFieldValue));

        city.InstanceFieldValue = "Seattle";

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == city.InstanceFieldValue));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_property_access_closure_via_query_cache(bool async)
    {
        var city = new City { InstancePropertyValue = "London" };

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == city.InstancePropertyValue));

        city.InstancePropertyValue = "Seattle";

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == city.InstancePropertyValue));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_static_field_access_closure_via_query_cache(bool async)
    {
        City.StaticFieldValue = "London";

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == City.StaticFieldValue));

        City.StaticFieldValue = "Seattle";

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == City.StaticFieldValue));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_static_property_access_closure_via_query_cache(bool async)
    {
        City.StaticPropertyValue = "London";

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == City.StaticPropertyValue));

        City.StaticPropertyValue = "Seattle";

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == City.StaticPropertyValue));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_nested_field_access_closure_via_query_cache(bool async)
    {
        var city = new City { Nested = new City { InstanceFieldValue = "London" } };

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == city.Nested.InstanceFieldValue));

        city.Nested.InstanceFieldValue = "Seattle";

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == city.Nested.InstanceFieldValue));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_nested_property_access_closure_via_query_cache(bool async)
    {
        var city = new City { Nested = new City { InstancePropertyValue = "London" } };

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == city.Nested.InstancePropertyValue));

        city.Nested.InstancePropertyValue = "Seattle";

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == city.Nested.InstancePropertyValue));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_nested_field_access_closure_via_query_cache_error_null(bool async)
    {
        var city = new City();

        return Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.City == city.Nested.InstanceFieldValue)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_nested_field_access_closure_via_query_cache_error_method_null(bool async)
    {
        var city = new City();

        return Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.City == city.Throw().InstanceFieldValue)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_new_instance_field_access_query_cache(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => c.City == new City { InstanceFieldValue = "London" }.InstanceFieldValue));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => c.City == new City { InstanceFieldValue = "Seattle" }.InstanceFieldValue));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_new_instance_field_access_closure_via_query_cache(bool async)
    {
        var city = "London";
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => c.City == new City { InstanceFieldValue = city }.InstanceFieldValue));

        city = "Seattle";
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => c.City == new City { InstanceFieldValue = city }.InstanceFieldValue));
    }

    private class City
    {
        public static string StaticFieldValue;
        public static string StaticPropertyValue { get; set; }

        public string InstanceFieldValue;
        public string InstancePropertyValue { get; set; }

        public int Int { get; set; }

        public int? NullableInt { get; set; }

        public City Nested;

        public City Throw()
            => throw new NotImplementedException();

        public string GetCity()
            => InstanceFieldValue;
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_simple_closure_via_query_cache_nullable_type(bool async)
    {
        int? reportsTo = 2;

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.ReportsTo == reportsTo));

        reportsTo = 5;

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.ReportsTo == reportsTo));

        reportsTo = null;

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.ReportsTo == reportsTo));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_simple_closure_via_query_cache_nullable_type_reverse(bool async)
    {
        int? reportsTo = null;

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.ReportsTo == reportsTo));

        reportsTo = 5;

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.ReportsTo == reportsTo));

        reportsTo = 2;

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.ReportsTo == reportsTo));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_subquery_closure_via_query_cache(bool async)
    {
        using var context = CreateContext();
        string customerID = null;

        var orders = context.Orders.Where(o => o.CustomerID == customerID);

        customerID = "ALFKI";

        var customers = context.Customers.Where(c => orders.Any(o => o.CustomerID == c.CustomerID));
        var result = async ? await customers.ToListAsync() : customers.ToList();

        Assert.Single(result);

        customerID = "ANATR";

        customers = context.Customers.Where(c => orders.Any(o => o.CustomerID == c.CustomerID));
        result = async ? await customers.ToListAsync() : customers.ToList();

        Assert.Equal("ANATR", result.Single().CustomerID);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bitwise_or(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" | c.CustomerID == "ANATR"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bitwise_and(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" & c.CustomerID == "ANATR"),
            assertEmpty: true);

    [ConditionalTheory]
    [InlineData(false)]
    public virtual Task Where_bitwise_xor(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => (c.CustomerID == "ALFKI") ^ true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_simple_shadow(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => EF.Property<string>(e, "Title") == "Sales Representative"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_simple_shadow_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => EF.Property<string>(e, "Title") == "Sales Representative")
                .Select(e => EF.Property<string>(e, "Title")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_simple_shadow_projection_mixed(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => EF.Property<string>(e, "Title") == "Sales Representative")
                .Select(
                    e => new { e, Title = EF.Property<string>(e, "Title") }),
            e => e.e.EmployeeID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_simple_shadow_subquery(bool async)
        => AssertQuery(
            async,
            ss => from e in ss.Set<Employee>().OrderBy(e => e.EmployeeID).Take(5)
                  where EF.Property<string>(e, "Title") == "Sales Representative"
                  select e);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_shadow_subquery_FirstOrDefault(bool async)
        => AssertQuery(
            async,
            ss => from e in ss.Set<Employee>()
                  where EF.Property<string>(e, "Title")
                      == EF.Property<string>(
                          ss.Set<Employee>().OrderBy(e2 => EF.Property<string>(e2, "Title")).FirstOrDefault(), "Title")
                  select e);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_client(bool async)
        => AssertTranslationFailedWithDetails(
            () => AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.IsLondon)),
            CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_correlated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c1 => ss.Set<Customer>().Any(c2 => c1.CustomerID == c2.CustomerID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_correlated_client_eval(bool async)
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .OrderBy(c1 => c1.CustomerID)
                    .Take(5)
                    .Where(c1 => ss.Set<Customer>().Any(c2 => c1.CustomerID == c2.CustomerID && c2.IsLondon))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_client_and_server_top_level(bool async)
        => AssertTranslationFailedWithDetails(
            () => AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.IsLondon && c.CustomerID != "AROUT")),
            CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_client_or_server_top_level(bool async)
        => AssertTranslationFailedWithDetails(
            () => AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.IsLondon || c.CustomerID == "ALFKI")),
            CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_client_and_server_non_top_level(bool async)
        => AssertTranslationFailedWithDetails(
            () => AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID != "ALFKI" == (c.IsLondon && c.CustomerID != "AROUT"))),
            CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_client_deep_inside_predicate_and_server_top_level(bool async)
        => AssertTranslationFailedWithDetails(
            () => AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.CustomerID != "ALFKI" && (c.CustomerID == "MAUMAR" || (c.CustomerID != "AROUT" && c.IsLondon)))),
            CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_equals_method_string(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City.Equals("London")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_equals_method_string_with_ignore_case(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City.Equals("London", StringComparison.OrdinalIgnoreCase)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_equals_method_int(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.EmployeeID.Equals(1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_equals_using_object_overload_on_mismatched_types(bool async)
    {
        ulong longPrm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.EmployeeID.Equals(longPrm)),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_equals_using_int_overload_on_mismatched_types(bool async)
    {
        ushort shortPrm = 1;

        return AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.EmployeeID.Equals(shortPrm)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_equals_on_mismatched_types_nullable_int_long(bool async)
    {
        ulong longPrm = 2;

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.ReportsTo.Equals(longPrm)),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => longPrm.Equals(e.ReportsTo)),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_equals_on_mismatched_types_int_nullable_int(bool async)
    {
        uint intPrm = 2;

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.ReportsTo.Equals(intPrm)));

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => intPrm.Equals(e.ReportsTo)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_equals_on_mismatched_types_nullable_long_nullable_int(bool async)
    {
        ulong? nullableLongPrm = 2;

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => nullableLongPrm.Equals(e.ReportsTo)),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.ReportsTo.Equals(nullableLongPrm)),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_equals_on_matched_nullable_int_types(bool async)
    {
        uint? nullableIntPrm = 2;

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => nullableIntPrm.Equals(e.ReportsTo)));

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.ReportsTo.Equals(nullableIntPrm)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_equals_on_null_nullable_int_types(bool async)
    {
        uint? nullableIntPrm = null;

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => nullableIntPrm.Equals(e.ReportsTo)),
            ss => ss.Set<Employee>().Where(e => e.ReportsTo == nullableIntPrm));

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.ReportsTo.Equals(nullableIntPrm)),
            ss => ss.Set<Employee>().Where(e => e.ReportsTo == nullableIntPrm));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_comparison_nullable_type_not_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.ReportsTo == 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_comparison_nullable_type_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => e.ReportsTo == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_string_length(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City.Length == 6));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_string_indexof(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City.IndexOf("Sea") != -1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_string_replace(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City.Replace("Sea", "Rea") == "Reattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_string_substring(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City.Substring(1, 2) == "ea"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetime_now(bool async)
    {
        var myDatetime = new DateTime(2015, 4, 10);

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => DateTime.Now != myDatetime));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetime_utcnow(bool async)
    {
        var myDatetime = new DateTime(2015, 4, 10);

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => DateTime.UtcNow != myDatetime));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetimeoffset_utcnow(bool async)
    {
        var myDatetimeOffset = new DateTimeOffset(2015, 4, 10, 0, 0, 0, new TimeSpan(-8, 0, 0));

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => DateTimeOffset.UtcNow != myDatetimeOffset));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetime_today(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => DateTime.Now.Date == DateTime.Today));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetime_date_component(bool async)
    {
        var myDatetime = new DateTime(1998, 5, 4);

        return AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate.Value.Date == myDatetime));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_date_add_year_constant_component(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate.Value.AddYears(-1).Year == 1997));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetime_year_component(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate.Value.Year == 1998));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetime_month_component(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate.Value.Month == 4));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetime_dayOfYear_component(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate.Value.DayOfYear == 68));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetime_day_component(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate.Value.Day == 4));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetime_hour_component(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate.Value.Hour == 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetime_minute_component(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate.Value.Minute == 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetime_second_component(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate.Value.Second == 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetime_millisecond_component(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate.Value.Millisecond == 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetimeoffset_now_component(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate < DateTimeOffset.Now));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_datetimeoffset_utcnow_component(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate != DateTimeOffset.UtcNow));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_simple_reversed(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => "London" == c.City));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_is_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Region == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_null_is_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => null == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_constant_is_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => "foo" == null),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_is_not_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City != null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_null_is_not_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => null != null),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_constant_is_not_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => "foo" != null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_identity_comparison(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == c.City));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_in_optimization_multiple(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from e in ss.Set<Employee>()
                where c.City == "London"
                    || c.City == "Berlin"
                    || c.CustomerID == "ALFKI"
                    || c.CustomerID == "ABCDE"
                select new { c, e },
            e => e.c.CustomerID + " " + e.e.EmployeeID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_not_in_optimization1(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from e in ss.Set<Employee>()
                where c.City != "London"
                    && e.City != "London"
                select new { c, e },
            e => e.c.CustomerID + " " + e.e.EmployeeID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_not_in_optimization2(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from e in ss.Set<Employee>()
                where c.City != "London"
                    && c.City != "Berlin"
                select new { c, e },
            e => e.c.CustomerID + " " + e.e.EmployeeID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_not_in_optimization3(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from e in ss.Set<Employee>()
                where c.City != "London"
                    && c.City != "Berlin"
                    && c.City != "Seattle"
                select new { c, e },
            e => e.c.CustomerID + " " + e.e.EmployeeID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_not_in_optimization4(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from e in ss.Set<Employee>()
                where c.City != "London"
                    && c.City != "Berlin"
                    && c.City != "Seattle"
                    && c.City != "Lisboa"
                select new { c, e },
            e => e.c.CustomerID + " " + e.e.EmployeeID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_select_many_and(bool async)
    {
        return AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from e in ss.Set<Employee>()
                // ReSharper disable ArrangeRedundantParentheses
#pragma warning disable RCS1032 // Remove redundant parentheses.
                where (c.City == "London" && c.Country == "UK")
                    && (e.City == "London" && e.Country == "UK")
#pragma warning restore RCS1032 // Remove redundant parentheses.
                select new { c, e },
            e => e.c.CustomerID + " " + e.e.EmployeeID);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_primitive(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Employee>().Select(e => e.EmployeeID).Take(9).Where(i => i == 5));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_primitive_tracked(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Employee>().Take(9).Where(e => e.EmployeeID == 5));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_primitive_tracked2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Employee>().Take(9).Select(e => new { e }).Where(e => e.e.EmployeeID == 5),
            e => e.e.EmployeeID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bool_member(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => p.Discontinued));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bool_member_false(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => !p.Discontinued));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bool_client_side_negated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => !ClientFunc(p.ProductID) && p.Discontinued));

#pragma warning disable IDE0060 // Remove unused parameter
    private static bool ClientFunc(int id)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        return false;
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bool_member_negated_twice(bool async)
    {
        return AssertQuery(
            async,
#pragma warning disable RCS1068 // Simplify logical negation.
#pragma warning disable RCS1033 // Remove redundant boolean literal.
            ss => ss.Set<Product>().Where(p => !!(p.Discontinued == true)));
#pragma warning restore RCS1033 // Remove redundant boolean literal.
#pragma warning restore RCS1068 // Simplify logical negation.
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bool_member_shadow(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => EF.Property<bool>(p, "Discontinued")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bool_member_false_shadow(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => !EF.Property<bool>(p, "Discontinued")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bool_member_equals_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => p.Discontinued.Equals(true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bool_member_in_complex_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => p.ProductID > 100 && p.Discontinued || (p.Discontinued == true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bool_member_compared_to_binary_expression(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => p.Discontinued == (p.ProductID > 50)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_not_bool_member_compared_to_not_bool_member(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => !p.Discontinued == !p.Discontinued));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_negated_boolean_expression_compared_to_another_negated_boolean_expression(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => !(p.ProductID > 50) == !(p.ProductID > 20)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_not_bool_member_compared_to_binary_expression(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => !p.Discontinued == (p.ProductID > 50)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bool_parameter(bool async)
    {
        var prm = true;

        return AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => prm));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bool_parameter_compared_to_binary_expression(bool async)
    {
        var prm = true;

        return AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => (p.ProductID > 50) != prm));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bool_member_and_parameter_compared_to_binary_expression_nested(bool async)
    {
        var prm = true;

        return AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => p.Discontinued == ((p.ProductID > 50) != prm)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_de_morgan_or_optimized(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => !(p.Discontinued || (p.ProductID < 20))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_de_morgan_and_optimized(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => !(p.Discontinued && (p.ProductID < 20))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_complex_negated_expression_optimized(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => !(!(!p.Discontinued && (p.ProductID < 60)) || !(p.ProductID > 30))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_short_member_comparison(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => p.UnitsInStock > 10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_comparison_to_nullable_bool(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.EndsWith("KI") == ((bool?)true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_true(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_false(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => false),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_bool_closure(bool async)
    {
        var boolean = false;

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" && boolean),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" || boolean));

        boolean = true;

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" && boolean));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" || boolean));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_poco_closure(bool async)
    {
        var customer = new Customer { CustomerID = "ALFKI" };

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Equals(customer)).Select(c => c.CustomerID));

        customer = new Customer { CustomerID = "ANATR" };

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Equals(customer)).Select(c => c.CustomerID));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_default(bool async)
    {
        var parameter = Expression.Parameter(typeof(Customer), "c");

        var defaultExpression =
            Expression.Lambda<Func<Customer, bool>>(
                Expression.Equal(
                    Expression.Property(
                        parameter,
                        "Fax"),
                    Expression.Default(typeof(string))),
                parameter);

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(defaultExpression));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_expression_invoke_1(bool async)
    {
        Expression<Func<Customer, bool>> expression = c => c.CustomerID == "ALFKI";
        var parameter = Expression.Parameter(typeof(Customer), "c");

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                Expression.Lambda<Func<Customer, bool>>(Expression.Invoke(expression, parameter), parameter)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_expression_invoke_2(bool async)
    {
        Expression<Func<Order, Customer>> customer = o => o.Customer;
        Expression<Func<Customer, bool>> predicate = c => c.CustomerID == "ALFKI";
        var exp = Expression.Lambda<Func<Order, bool>>(
            Expression.Invoke(predicate, customer.Body),
            customer.Parameters);

        return AssertQuery(
            async,
            ss => ss.Set<Order>().Where(exp));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_expression_invoke_3(bool async)
    {
        Expression<Func<Customer, bool>> lambda3 = c => c.CustomerID == "ALFKI";
        var customerParameter2 = Expression.Parameter(typeof(Customer));
        var lambda2 = Expression.Lambda<Func<Customer, bool>>(
            Expression.Invoke(lambda3, customerParameter2),
            customerParameter2);

        var customerParameter = Expression.Parameter(typeof(Customer));
        var lambda = Expression.Lambda<Func<Customer, bool>>(
            Expression.Invoke(lambda2, customerParameter),
            customerParameter);
        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(lambda));
    }

    //see issue #31917
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_concat_string_int_comparison1(bool async)
    {
        var i = 10;

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID + i == c.CompanyName).Select(c => c.CustomerID),
            assertEmpty: true);
    }

    //see issue #31917
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_concat_string_int_comparison2(bool async)
    {
        var i = 10;

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => i + c.CustomerID == c.CompanyName).Select(c => c.CustomerID),
            assertEmpty: true);
    }

    //see issue #31917
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_concat_string_int_comparison3(bool async)
    {
        var i = 10;
        var j = 21;

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => i + 20 + c.CustomerID + j + 42 == c.CompanyName).Select(c => c.CustomerID),
            assertEmpty: true);
    }

    //see issue #31917
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_concat_string_int_comparison4(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID + o.CustomerID == o.CustomerID).Select(c => c.CustomerID),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_concat_string_string_comparison(bool async)
    {
        var i = "A";

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => i + c.CustomerID == "AALFKI").Select(c => c.CustomerID));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_string_concat_method_comparison(bool async)
    {
        var i = "A";

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.Concat(i, c.CustomerID) == "AAROUT").Select(c => c.CustomerID));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_string_concat_method_comparison_2(bool async)
    {
        var i = "A";
        var j = "B";

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.Concat(i, j, c.CustomerID) == "ABANATR").Select(c => c.CustomerID));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_string_concat_method_comparison_3(bool async)
    {
        var i = "A";
        var j = "B";
        var k = "C";

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.Concat(i, j, k, c.CustomerID) == "ABCANTON").Select(c => c.CustomerID));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_ternary_boolean_condition_true(bool async)
    {
        var flag = true;

        return AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => flag ? p.UnitsInStock >= 20 : p.UnitsInStock < 20));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_ternary_boolean_condition_false(bool async)
    {
        var flag = false;

        return AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => flag ? p.UnitsInStock >= 20 : p.UnitsInStock < 20));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_ternary_boolean_condition_with_another_condition(bool async)
    {
        var flag = true;
        var productId = 15;

        return AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => p.ProductID < productId && (flag ? p.UnitsInStock >= 20 : p.UnitsInStock < 20)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_ternary_boolean_condition_with_false_as_result_true(bool async)
    {
        var flag = true;

        return AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => flag ? p.UnitsInStock >= 20 : false));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_ternary_boolean_condition_with_false_as_result_false(bool async)
    {
        var flag = false;

        return AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => flag ? p.UnitsInStock >= 20 : false),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_compare_constructed_equal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => new { x = c.City } == new { x = "London" }),
            ss => ss.Set<Customer>().Where(c => c.City == "London"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_compare_constructed_multi_value_equal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => new { x = c.City, y = c.Country } == new { x = "London", y = "UK" }),
            ss => ss.Set<Customer>().Where(c => c.City == "London" && c.Country == "UK"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_compare_constructed_multi_value_not_equal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => new { x = c.City, y = c.Country } != new { x = "London", y = "UK" }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_compare_tuple_constructed_equal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => new Tuple<string>(c.City) == new Tuple<string>("London")),
            ss => ss.Set<Customer>().Where(c => c.City == "London"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_compare_tuple_constructed_multi_value_equal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => new Tuple<string, string>(c.City, c.Country) == new Tuple<string, string>("London", "UK")),
            ss => ss.Set<Customer>().Where(c => c.City == "London" && c.Country == "UK"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_compare_tuple_constructed_multi_value_not_equal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => new Tuple<string, string>(c.City, c.Country) != new Tuple<string, string>("London", "UK")),
            ss => ss.Set<Customer>().Where(c => c.City != "London" || c.Country != "UK"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_compare_tuple_create_constructed_equal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => Tuple.Create(c.City) == Tuple.Create("London")),
            ss => ss.Set<Customer>().Where(c => c.City == "London"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_compare_tuple_create_constructed_multi_value_equal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => Tuple.Create(c.City, c.Country) == Tuple.Create("London", "UK")),
            ss => ss.Set<Customer>().Where(c => c.City == "London" && c.Country == "UK"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_compare_tuple_create_constructed_multi_value_not_equal(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => Tuple.Create(c.City, c.Country) != Tuple.Create("London", "UK")),
            ss => ss.Set<Customer>().Where(c => c.City != "London" || c.Country != "UK"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_compare_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Region == null && c.Country == "UK"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_compare_null_with_cast_to_object(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => (object)c.Region == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_compare_with_both_cast_to_object(bool async)
        => AssertQuery(
            async,
            // ReSharper disable twice RedundantCast
            ss => ss.Set<Customer>().Where(c => (object)c.City == (object)"London"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == "London").Select(c => c.CompanyName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Is_on_same_type(bool async)
        => AssertQuery(
            async,
            // ReSharper disable once ConvertTypeCheckToNullCheck
            ss => ss.Set<Customer>().Where(c => c is Customer));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_chain(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.CustomerID == "QUICK")
                .Where(o => o.OrderDate > new DateTime(1998, 1, 1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_navigation_contains(bool async)
    {
        using var context = CreateContext();
        var customer = context.Customers.Include(c => c.Orders).Single(c => c.CustomerID == "ALFKI");
        var orderDetails = context.OrderDetails.Where(od => customer.Orders.Contains(od.Order));

        var result = async ? await orderDetails.ToListAsync() : orderDetails.ToList();

        Assert.Equal(12, result.Count);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_array_index(bool async)
    {
        var customers = new[] { "ALFKI", "ANATR" };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == customers[0]));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_multiple_contains_in_subquery_with_or(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(
                od => ss.Set<Product>().OrderBy(p => p.ProductID).Take(1).Select(p => p.ProductID).Contains(od.ProductID)
                    || ss.Set<Order>().OrderBy(o => o.OrderID).Take(1).Select(o => o.OrderID).Contains(od.OrderID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_multiple_contains_in_subquery_with_and(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(
                od => ss.Set<Product>().OrderBy(p => p.ProductID).Take(20).Select(p => p.ProductID).Contains(od.ProductID)
                    && ss.Set<Order>().OrderBy(o => o.OrderID).Take(10).Select(o => o.OrderID).Contains(od.OrderID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_contains_on_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => ss.Set<Customer>().Any(c => c.Orders.Contains(o))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_FirstOrDefault_is_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault() == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_FirstOrDefault_compared_to_entity(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault() == new Order { OrderID = 10276 }),

            ss => ss.Set<Customer>().Where(
                c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault().OrderID == 10276));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Time_of_day_datetime(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderDate.Value.TimeOfDay));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TypeBinary_short_circuit(bool async)
    {
        var customer = new Customer();

        return AssertQuery(
            async,
#pragma warning disable CS0184 // 'is' expression's given expression is never of the provided type
            ss => ss.Set<Order>().Where(o => (customer is Order)),
#pragma warning restore CS0184 // 'is' expression's given expression is never of the provided type
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Decimal_cast_to_double_works(bool async)
    {
        var customer = new Customer();

        return AssertQuery(
            async,
            ss => ss.Set<Product>().Where(p => (double?)p.UnitPrice > 100));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_is_conditional(bool async)
        => AssertQuery(
            async,
            // ReSharper disable once ConvertTypeCheckToNullCheck
            // ReSharper disable once SimplifyConditionalTernaryExpression
            ss => ss.Set<Product>().Where(p => p is Product ? false : true),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Enclosing_class_settable_member_generates_parameter(bool async)
    {
        SettableProperty = 10274;

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID == SettableProperty));

        SettableProperty = 10275;

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID == SettableProperty));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Enclosing_class_readonly_member_generates_parameter(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID == ReadOnlyProperty));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Enclosing_class_const_member_does_not_generate_parameter(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID == ConstantProperty));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Generic_Ilist_contains_translates_to_server(bool async)
    {
        var cities = new List<string> { "Seattle" } as IList<string>;

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => cities.Contains(c.City)));
    }

    private int SettableProperty { get; set; }

    private int ReadOnlyProperty
        => 10275;

    private const int ConstantProperty = 10274;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_non_nullable_value_after_FirstOrDefault_on_empty_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => ss.Set<Order>().Where(o => o.CustomerID == "John Doe").Select(o => o.CustomerID).FirstOrDefault().Length == 0),
            ss => ss.Set<Customer>().Where(c => false),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Like_with_non_string_column_using_ToString(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => EF.Functions.Like(o.OrderID.ToString(), "%20%")),
            ss => ss.Set<Order>().Where(o => o.OrderID.ToString().Contains("20")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Like_with_non_string_column_using_double_cast(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => EF.Functions.Like((string)(object)o.OrderID, "%20%")),
            ss => ss.Set<Order>().Where(o => o.OrderID.ToString().Contains("20")));

    // see issue #31917
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Using_same_parameter_twice_in_query_generates_one_sql_parameter(bool async)
    {
        var i = 10;
        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => i + c.CustomerID + i == "10ALFKI10")
                .Select(c => c.CustomerID));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Queryable_ToList_Count(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID)
                .Select(c => ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).ToList())
                .Where(e => e.Count() == 0),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Queryable_ToList_Contains(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Select(c => ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).Select(o => o.CustomerID).ToList())
                .Where(e => e.Contains("ALFKI")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Queryable_ToArray_Count(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID)
                .Select(c => ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).ToArray())
                .Where(e => e.Count() == 0),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Queryable_ToArray_Contains(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Select(c => ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).Select(o => o.CustomerID).ToArray())
                .Where(e => e.Contains("ALFKI")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Queryable_AsEnumerable_Count(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID)
                .Select(c => ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).AsEnumerable())
                .Where(e => e.Count() == 0),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Queryable_AsEnumerable_Contains(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Select(c => ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).Select(o => o.CustomerID).AsEnumerable())
                .Where(e => e.Contains("ALFKI")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Queryable_AsEnumerable_Contains_negated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Select(
                    c => new
                    {
                        c.CustomerID,
                        Subquery = ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).Select(o => o.CustomerID).AsEnumerable()
                    })
                .Where(e => !e.Subquery.Contains("ALFKI")),
            elementSorter: e => e.CustomerID,
            elementAsserter: (e, a) => AssertCollection(e.Subquery, a.Subquery));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Queryable_ToList_Count_member(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID)
                .Select(c => ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).ToList())
                .Where(e => e.Count == 0),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Queryable_ToArray_Length_member(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID)
                .Select(c => ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).ToArray())
                .Where(e => e.Length == 0),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_collection_navigation_ToList_Count(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .OrderBy(o => o.OrderID)
                .Select(o => o.OrderDetails.ToList())
                .Where(e => e.Count() == 4),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_collection_navigation_ToList_Contains(bool async)
    {
        var order = new Order { OrderID = 10248 };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Select(c => c.Orders.OrderBy(o => o.OrderID).ToList())
                .Where(e => e.Contains(order)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_collection_navigation_ToArray_Count(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .OrderBy(o => o.OrderID)
                .Select(o => o.OrderDetails.ToArray())
                .Where(e => e.Count() == 4),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_collection_navigation_ToArray_Contains(bool async)
    {
        var order = new Order { OrderID = 10248 };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Select(c => c.Orders.AsEnumerable().OrderBy(o => o.OrderID).ToArray())
                .Where(e => e.Contains(order)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_collection_navigation_AsEnumerable_Count(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .OrderBy(o => o.OrderID)
                .Select(o => o.OrderDetails.AsEnumerable())
                .Where(e => e.Count() == 5),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_collection_navigation_AsEnumerable_Contains(bool async)
    {
        var order = new Order { OrderID = 10248 };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Select(c => c.Orders.OrderBy(o => o.OrderID).AsEnumerable())
                .Where(e => e.Contains(order)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_collection_navigation_ToList_Count_member(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .OrderBy(o => o.OrderID)
                .Select(o => o.OrderDetails.ToList())
                .Where(e => e.Count == 3),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_collection_navigation_ToArray_Length_member(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .OrderBy(o => o.OrderID)
                .Select(o => o.OrderDetails.ToArray())
                .Where(e => e.Length == 3),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_list_object_contains_over_value_type(bool async)
    {
        var orderIds = new List<object> { 10248, 10249 };
        return AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => orderIds.Contains(o.OrderID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_array_of_object_contains_over_value_type(bool async)
    {
        var orderIds = new object[] { 10248, 10249 };
        return AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => orderIds.Contains(o.OrderID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_OrElse_on_same_column_converted_to_in_with_overlap(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => c.CustomerID == "ALFKI" || c.CustomerID == "ANATR" || c.CustomerID == "ANTON" || c.CustomerID == "ANATR"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_OrElse_on_same_column_with_null_constant_comparison_converted_to_in(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Region == "WA" || c.Region == "OR" || c.Region == null || c.Region == "BC"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Constant_array_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => new[] { "ALFKI", "ANATR" }.Contains(c.CustomerID) || c.CustomerID == "ANTON"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Constant_array_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in_with_overlap(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => c.CustomerID == "ANTON" || new[] { "ALFKI", "ANATR" }.Contains(c.CustomerID) || c.CustomerID == "ALFKI"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Constant_array_Contains_OrElse_another_Contains_gets_combined_to_one_in_with_overlap(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => new[] { "ALFKI", "ANATR" }.Contains(c.CustomerID) || new[] { "ALFKI", "ANTON" }.Contains(c.CustomerID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Constant_array_Contains_AndAlso_another_Contains_gets_combined_to_one_in_with_overlap(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => !new[] { "ALFKI", "ANATR" }.Contains(c.CustomerID) && !new[] { "ALFKI", "ANTON" }.Contains(c.CustomerID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_AndAlso_on_same_column_converted_to_in_using_parameters(bool async)
    {
        var prm1 = "ALFKI";
        var prm2 = "ANATR";
        var prm3 = "ANTON";

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID != prm1 && c.CustomerID != prm2 && c.CustomerID != prm3));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Array_of_parameters_Contains_OrElse_comparison_with_constant_gets_combined_to_one_in(bool async)
    {
        var prm1 = "ALFKI";
        var prm2 = "ANATR";

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => new[] { prm1, prm2 }.Contains(c.CustomerID) || c.CustomerID == "ANTON"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_OrElse_on_same_column_with_null_parameter_comparison_converted_to_in(bool async)
    {
        string prm = null;

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Region == "WA" || c.Region == "OR" || c.Region == prm || c.Region == "BC"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_array_Contains_OrElse_comparison_with_constant(bool async)
    {
        var array = new[] { "ALFKI", "ANATR" };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => array.Contains(c.CustomerID) || c.CustomerID == "ANTON"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_array_Contains_OrElse_comparison_with_parameter_with_overlap(bool async)
    {
        var array = new[] { "ALFKI", "ANATR" };
        var prm1 = "ANTON";
        var prm2 = "ALFKI";

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == prm1 || array.Contains(c.CustomerID) || c.CustomerID == prm2));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Two_sets_of_comparison_combine_correctly(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => new[] { "ALFKI", "ANATR" }.Contains(c.CustomerID) && (c.CustomerID == "ANATR" || c.CustomerID == "ANTON")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Two_sets_of_comparison_combine_correctly2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => (c.Region != "WA" && c.Region != "OR" && c.Region != null) || (c.Region != "WA" && c.Region != null)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_with_property_compared_to_null_wrapped_in_explicit_convert_to_object(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Region == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_with_EF_Property_using_closure_for_property_name(bool async)
    {
        var id = "CustomerID";

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => EF.Property<string>(c, id) == "ALFKI"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_with_EF_Property_using_function_for_property_name(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => EF.Property<string>(c, StringMethod("CustomerID")) == "ALFKI"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FirstOrDefault_over_scalar_projection_compared_to_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => (int?)o.OrderID).FirstOrDefault() == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FirstOrDefault_over_scalar_projection_compared_to_not_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => (int?)o.OrderID).FirstOrDefault() != null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FirstOrDefault_over_custom_projection_compared_to_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).FirstOrDefault() == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FirstOrDefault_over_custom_projection_compared_to_not_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).FirstOrDefault() != null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SingleOrDefault_over_custom_projection_compared_to_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).SingleOrDefault() == null),
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).FirstOrDefault() == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SingleOrDefault_over_custom_projection_compared_to_not_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).SingleOrDefault() != null),
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).FirstOrDefault() != null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task LastOrDefault_over_custom_projection_compared_to_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).LastOrDefault() == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task LastOrDefault_over_custom_projection_compared_to_not_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).LastOrDefault() != null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task First_over_custom_projection_compared_to_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).First() == null),
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).FirstOrDefault() == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task First_over_custom_projection_compared_to_not_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).First() != null),
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).FirstOrDefault() != null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ElementAt_over_custom_projection_compared_to_not_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).ElementAt(3) != null),
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).ElementAtOrDefault(3) != null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ElementAtOrDefault_over_custom_projection_compared_to_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).ElementAtOrDefault(7) == null),
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).ElementAtOrDefault(7) == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Single_over_custom_projection_compared_to_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).Single() == null),
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).FirstOrDefault() == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Single_over_custom_projection_compared_to_not_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).Single() != null),
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).FirstOrDefault() != null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Last_over_custom_projection_compared_to_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).Last() == null),
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).LastOrDefault() == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Last_over_custom_projection_compared_to_not_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).Last() != null),
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => new { o.OrderID }).LastOrDefault() != null));

    private string StringMethod(string arg)
        => arg;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Contains_and_comparison(bool async)
    {
        var customerIds = new[] { "ALFKI", "FISSA", "WHITC" };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => customerIds.Contains(c.CustomerID) && c.City == "Seattle"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Contains_or_comparison(bool async)
    {
        var customerIds = new[] { "ALFKI", "FISSA" };
        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => customerIds.Contains(c.CustomerID) || c.City == "Seattle"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Like_and_comparison(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => EF.Functions.Like(c.CustomerID, "F%") && c.City == "Seattle"),
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F") && c.City == "Seattle"),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Like_or_comparison(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => EF.Functions.Like(c.CustomerID, "F%") || c.City == "Seattle"),
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F") || c.City == "Seattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetType_on_non_hierarchy1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.GetType() == typeof(Customer)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetType_on_non_hierarchy2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.GetType() != typeof(Customer)),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetType_on_non_hierarchy3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.GetType() == typeof(Order)),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetType_on_non_hierarchy4(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.GetType() != typeof(Order)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Case_block_simplification_works_correctly(bool async)
#pragma warning disable IDE0029 // Use coalesce expression
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => (c.Region == null ? "OR" : c.Region) == "OR"));
#pragma warning restore IDE0029 // Use coalesce expression

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task EF_Constant(bool async)
    {
        var id = "ALFKI";

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == EF.Constant(id)),
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task EF_Constant_with_subtree(bool async)
    {
        var i = "ALF";
        var j = "KI";

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == EF.Constant(i + j)),
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task EF_Constant_does_not_parameterized_as_part_of_bigger_subtree(bool async)
    {
        var id = "ALF";

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == EF.Constant(id) + "KI"),
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALF" + "KI"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task EF_Constant_with_non_evaluatable_argument_throws(bool async)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == EF.Constant(c.CustomerID))));

        Assert.Equal(CoreStrings.EFConstantWithNonEvaluatableArgument, exception.Message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task EF_Parameter(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == EF.Parameter("ALFKI")),
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task EF_Parameter_with_subtree(bool async)
    {
        // This is a somewhat silly scenario: a subtree would get parameterized anyway, with or without EF.Parameter().
        // But including for completeness.
        var i = "ALF";
        var j = "KI";

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == EF.Parameter(i + j)),
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task EF_Parameter_does_not_parameterized_as_part_of_bigger_subtree(bool async)
    {
        var id = "ALF";

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == EF.Parameter(id) + "KI"),
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALF" + "KI"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task EF_Parameter_with_non_evaluatable_argument_throws(bool async)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == EF.Parameter(c.CustomerID))));

        Assert.Equal(CoreStrings.EFParameterWithNonEvaluatableArgument, exception.Message);
    }

    private class EntityWithImplicitCast(int value)
    {
        private readonly int _value = value;

        public string Value
            => _value.ToString();

        public override string ToString()
            => Value;

        public static implicit operator string(EntityWithImplicitCast entity)
            => entity.Value;

        public EntityWithImplicitCast Clone()
            => new(_value);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Implicit_cast_in_predicate(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(x => x.CustomerID == new EntityWithImplicitCast(1337)),
            assertEmpty: true);

        var prm = new EntityWithImplicitCast(1337);

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(x => x.CustomerID == prm.Value),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(x => x.CustomerID == prm.ToString()),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(x => x.CustomerID == prm),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(x => x.CustomerID == new EntityWithImplicitCast(1337).Clone()),
            assertEmpty: true);
    }

    public interface IHaveId
    {
        int Id { get; }
    }

    public class DtoWithInterface : IHaveId
    {
        public int Id { get; set; }
    }

    public static IQueryable<T> AddFilter<T>(IQueryable<T> query, int id)
        where T : IHaveId
        => query.Where(a => a.Id == id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Interface_casting_though_generic_method(bool async)
    {
        await AssertQuery(
            async,
            ss => AddFilter(ss.Set<Order>().Select(x => new DtoWithInterface { Id = x.OrderID }), 10252),
            elementAsserter: (e, a) => AssertEqual(e.Id, a.Id));

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Select(x => new DtoWithInterface { Id = x.OrderID }).Where<IHaveId>(x => x.Id == 10252),
            elementAsserter: (e, a) => AssertEqual(e.Id, a.Id));

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Select(x => new DtoWithInterface { Id = x.OrderID }).Where(x => ((IHaveId)x).Id == 10252),
            elementAsserter: (e, a) => AssertEqual(e.Id, a.Id));

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Select(x => new DtoWithInterface { Id = x.OrderID }).Where(x => (x as IHaveId).Id == 10252),
            elementAsserter: (e, a) => AssertEqual(e.Id, a.Id));
    }
}
