// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

// ReSharper disable RedundantBoolCompare
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract partial class SimpleQueryTestBase<TFixture>
    {
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_simple(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == "London"),
                entryCount: 6);
        }

        private static readonly Expression<Func<Order, bool>> _filter = o => o.CustomerID == "ALFKI";

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_as_queryable_expression(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.Orders.AsQueryable().Any(_filter)),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_simple_closure(bool isAsync)
        {
            var city = "London";

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == city),
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_indexer_closure(bool isAsync)
        {
            var cities = new[] { "London" };

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == cities[0]),
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_dictionary_key_access_closure(bool isAsync)
        {
            var predicateMap = new Dictionary<string, string> { ["City"] = "London" };

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == predicateMap["City"]),
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_tuple_item_closure(bool isAsync)
        {
            var predicateTuple = new Tuple<string, string>("ALFKI", "London");

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == predicateTuple.Item2),
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_named_tuple_item_closure(bool isAsync)
        {
            (string CustomerID, string City) predicateTuple = ("ALFKI", "London");

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == predicateTuple.City),
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_simple_closure_constant(bool isAsync)
        {
            var predicate = true;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => predicate),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_simple_closure_via_query_cache(bool isAsync)
        {
            var city = "London";

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == city),
                entryCount: 6);

            city = "Seattle";

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == city),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_method_call_nullable_type_closure_via_query_cache(bool isAsync)
        {
            var city = new City { Int = 2 };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => e.ReportsTo == city.Int),
                entryCount: 5);

            city.Int = 5;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => e.ReportsTo == city.Int),
                entryCount: 3);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_method_call_nullable_type_reverse_closure_via_query_cache(bool isAsync)
        {
            var city = new City { NullableInt = 1 };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => e.EmployeeID > city.NullableInt),
                entryCount: 8);

            city.NullableInt = 5;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => e.EmployeeID > city.NullableInt),
                entryCount: 4);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_method_call_closure_via_query_cache(bool isAsync)
        {
            var city = new City { InstanceFieldValue = "London" };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == city.GetCity()),
                entryCount: 6);

            city.InstanceFieldValue = "Seattle";

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == city.GetCity()),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_field_access_closure_via_query_cache(bool isAsync)
        {
            var city = new City { InstanceFieldValue = "London" };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == city.InstanceFieldValue),
                entryCount: 6);

            city.InstanceFieldValue = "Seattle";

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == city.InstanceFieldValue),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_property_access_closure_via_query_cache(bool isAsync)
        {
            var city = new City { InstancePropertyValue = "London" };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == city.InstancePropertyValue),
                entryCount: 6);

            city.InstancePropertyValue = "Seattle";

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == city.InstancePropertyValue),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_static_field_access_closure_via_query_cache(bool isAsync)
        {
            City.StaticFieldValue = "London";

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == City.StaticFieldValue),
                entryCount: 6);

            City.StaticFieldValue = "Seattle";

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == City.StaticFieldValue),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_static_property_access_closure_via_query_cache(bool isAsync)
        {
            City.StaticPropertyValue = "London";

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == City.StaticPropertyValue),
                entryCount: 6);

            City.StaticPropertyValue = "Seattle";

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == City.StaticPropertyValue),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_nested_field_access_closure_via_query_cache(bool isAsync)
        {
            var city = new City { Nested = new City { InstanceFieldValue = "London" } };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == city.Nested.InstanceFieldValue),
                entryCount: 6);

            city.Nested.InstanceFieldValue = "Seattle";

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == city.Nested.InstanceFieldValue),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_nested_property_access_closure_via_query_cache(bool isAsync)
        {
            var city = new City { Nested = new City { InstancePropertyValue = "London" } };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == city.Nested.InstancePropertyValue),
                entryCount: 6);

            city.Nested.InstancePropertyValue = "Seattle";

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == city.Nested.InstancePropertyValue),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_nested_field_access_closure_via_query_cache_error_null()
        {
            var city = new City();

            using (var context = CreateContext())
            {
                Assert.Throws<InvalidOperationException>(
                    () => context.Set<Customer>()
                        .Where(c => c.City == city.Nested.InstanceFieldValue)
                        .ToList());
            }
        }

        [ConditionalFact]
        public virtual async Task Where_nested_field_access_closure_via_query_cache_error_null_async()
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
        public virtual void Where_nested_field_access_closure_via_query_cache_error_method_null()
        {
            var city = new City();

            using (var context = CreateContext())
            {
                Assert.Throws<InvalidOperationException>(
                    () => context.Set<Customer>()
                        .Where(c => c.City == city.Throw().InstanceFieldValue)
                        .ToList());
            }
        }

        [ConditionalFact]
        public virtual async Task Where_nested_field_access_closure_via_query_cache_error_method_null_async()
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_new_instance_field_access_query_cache(bool isAsync)
        {
            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(
                    c => c.City == new City { InstanceFieldValue = "London" }.InstanceFieldValue),
                entryCount: 6);

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(
                    c => c.City == new City { InstanceFieldValue = "Seattle" }.InstanceFieldValue),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_new_instance_field_access_closure_via_query_cache(bool isAsync)
        {
            var city = "London";
            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(
                    c => c.City == new City { InstanceFieldValue = city }.InstanceFieldValue),
                entryCount: 6);

            city = "Seattle";
            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(
                    c => c.City == new City { InstanceFieldValue = city }.InstanceFieldValue),
                entryCount: 1);
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
            {
                throw new NotImplementedException();
            }

            public string GetCity()
            {
                return InstanceFieldValue;
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_simple_closure_via_query_cache_nullable_type(bool isAsync)
        {
            int? reportsTo = 2;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => e.ReportsTo == reportsTo),
                entryCount: 5);

            reportsTo = 5;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => e.ReportsTo == reportsTo),
                entryCount: 3);

            reportsTo = null;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => e.ReportsTo == reportsTo),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_simple_closure_via_query_cache_nullable_type_reverse(bool isAsync)
        {
            int? reportsTo = null;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => e.ReportsTo == reportsTo),
                entryCount: 1);

            reportsTo = 5;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => e.ReportsTo == reportsTo),
                entryCount: 3);

            reportsTo = 2;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => e.ReportsTo == reportsTo),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual void Where_subquery_closure_via_query_cache()
        {
            using (var context = CreateContext())
            {
                string customerID = null;

                var orders = context.Orders.Where(o => o.CustomerID == customerID);

                customerID = "ALFKI";

                var customers = context.Customers.Where(c => orders.Any(o => o.CustomerID == c.CustomerID)).ToList();

                Assert.Single(customers);

                customerID = "ANATR";

                customers = context.Customers.Where(c => orders.Any(o => o.CustomerID == c.CustomerID)).ToList();

                Assert.Equal("ANATR", customers.Single().CustomerID);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bitwise_or(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" | c.CustomerID == "ANATR"),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bitwise_and(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" & c.CustomerID == "ANATR"));
        }

        [ConditionalTheory(Skip = "Issue #16645. Cannot eval 'where (([c].CustomerID == \"ALFKI\") ^ True)'")]
        [InlineData(false)]
        public virtual Task Where_bitwise_xor(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => (c.CustomerID == "ALFKI") ^ true),
                entryCount: 90);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_simple_shadow(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => EF.Property<string>(e, "Title") == "Sales Representative"),
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_simple_shadow_projection(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => EF.Property<string>(e, "Title") == "Sales Representative")
                    .Select(e => EF.Property<string>(e, "Title")));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_simple_shadow_projection_mixed(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => EF.Property<string>(e, "Title") == "Sales Representative")
                    .Select(
                        e => new { e, Title = EF.Property<string>(e, "Title") }),
                e => e.e.EmployeeID,
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_simple_shadow_subquery(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from e in ss.Set<Employee>().OrderBy(e => e.EmployeeID).Take(5)
                      where EF.Property<string>(e, "Title") == "Sales Representative"
                      select e,
                entryCount: 3);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_shadow_subquery_FirstOrDefault(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from e in ss.Set<Employee>()
                      where EF.Property<string>(e, "Title")
                          == EF.Property<string>(
                              ss.Set<Employee>().OrderBy(e2 => EF.Property<string>(e2, "Title")).FirstOrDefault(), "Title")
                      select e,
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_client(bool isAsync)
        {
            return AssertTranslationFailed(
                () => AssertQuery(
                    isAsync,
                    ss => ss.Set<Customer>().Where(c => c.IsLondon),
                    entryCount: 6));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_correlated(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c1 => ss.Set<Customer>().Any(c2 => c1.CustomerID == c2.CustomerID)),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_correlated_client_eval(bool isAsync)
        {
            return AssertTranslationFailed(
                () => AssertQuery(
                    isAsync,
                    ss => ss.Set<Customer>()
                        .OrderBy(c1 => c1.CustomerID)
                        .Take(5)
                        .Where(c1 => ss.Set<Customer>().Any(c2 => c1.CustomerID == c2.CustomerID && c2.IsLondon)),
                    entryCount: 1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_client_and_server_top_level(bool isAsync)
        {
            return AssertTranslationFailed(
                () => AssertQuery(
                    isAsync,
                    ss => ss.Set<Customer>().Where(c => c.IsLondon && c.CustomerID != "AROUT"),
                    entryCount: 5));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_client_or_server_top_level(bool isAsync)
        {
            return AssertTranslationFailed(
                () => AssertQuery(
                    isAsync,
                    ss => ss.Set<Customer>().Where(c => c.IsLondon || c.CustomerID == "ALFKI"),
                    entryCount: 7));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_client_and_server_non_top_level(bool isAsync)
        {
            return AssertTranslationFailed(
                () => AssertQuery(
                    isAsync,
                    ss => ss.Set<Customer>().Where(c => c.CustomerID != "ALFKI" == (c.IsLondon && c.CustomerID != "AROUT")),
                    entryCount: 6));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_client_deep_inside_predicate_and_server_top_level(bool isAsync)
        {
            return AssertTranslationFailed(
                () => AssertQuery(
                    isAsync,
                    ss => ss.Set<Customer>()
                        .Where(c => c.CustomerID != "ALFKI" && (c.CustomerID == "MAUMAR" || (c.CustomerID != "AROUT" && c.IsLondon))),
                    entryCount: 5));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_equals_method_string(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City.Equals("London")),
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_equals_method_string_with_ignore_case(bool isAsync)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    isAsync,
                    ss => ss.Set<Customer>().Where(c => c.City.Equals("London", StringComparison.OrdinalIgnoreCase)),
                    entryCount: 6));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_equals_method_int(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => e.EmployeeID.Equals(1)),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_equals_using_object_overload_on_mismatched_types(bool isAsync)
        {
            ulong longPrm = 1;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => e.EmployeeID.Equals(longPrm)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_equals_using_int_overload_on_mismatched_types(bool isAsync)
        {
            ushort shortPrm = 1;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => e.EmployeeID.Equals(shortPrm)),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_equals_on_mismatched_types_nullable_int_long(bool isAsync)
        {
            ulong longPrm = 2;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => e.ReportsTo.Equals(longPrm)));

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => longPrm.Equals(e.ReportsTo)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_equals_on_mismatched_types_int_nullable_int(bool isAsync)
        {
            uint intPrm = 2;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => e.ReportsTo.Equals(intPrm)),
                entryCount: 5);

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => intPrm.Equals(e.ReportsTo)),
                entryCount: 5);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_equals_on_mismatched_types_nullable_long_nullable_int(bool isAsync)
        {
            ulong? nullableLongPrm = 2;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => nullableLongPrm.Equals(e.ReportsTo)));

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => e.ReportsTo.Equals(nullableLongPrm)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_equals_on_matched_nullable_int_types(bool isAsync)
        {
            uint? nullableIntPrm = 2;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => nullableIntPrm.Equals(e.ReportsTo)),
                entryCount: 5);

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => e.ReportsTo.Equals(nullableIntPrm)),
                entryCount: 5);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_equals_on_null_nullable_int_types(bool isAsync)
        {
            uint? nullableIntPrm = null;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => nullableIntPrm.Equals(e.ReportsTo)),
                entryCount: 1);

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => e.ReportsTo.Equals(nullableIntPrm)),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_comparison_nullable_type_not_null(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => e.ReportsTo == 2),
                entryCount: 5);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_comparison_nullable_type_null(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => e.ReportsTo == null),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_string_length(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City.Length == 6),
                entryCount: 20);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_string_indexof(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City.IndexOf("Sea") != -1),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_string_replace(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City.Replace("Sea", "Rea") == "Reattle"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_string_substring(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City.Substring(1, 2) == "ea"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetime_now(bool isAsync)
        {
            var myDatetime = new DateTime(2015, 4, 10);

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => DateTime.Now != myDatetime),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetime_utcnow(bool isAsync)
        {
            var myDatetime = new DateTime(2015, 4, 10);

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => DateTime.UtcNow != myDatetime),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetime_today(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => DateTime.Now.Date == DateTime.Today),
                entryCount: 9);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetime_date_component(bool isAsync)
        {
            var myDatetime = new DateTime(1998, 5, 4);

            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderDate.Value.Date == myDatetime),
                entryCount: 3);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_date_add_year_constant_component(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderDate.Value.AddYears(-1).Year == 1997),
                entryCount: 270);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetime_year_component(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderDate.Value.Year == 1998),
                entryCount: 270);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetime_month_component(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderDate.Value.Month == 4),
                entryCount: 105);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetime_dayOfYear_component(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderDate.Value.DayOfYear == 68),
                entryCount: 3);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetime_day_component(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderDate.Value.Day == 4),
                entryCount: 27);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetime_hour_component(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderDate.Value.Hour == 14));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetime_minute_component(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderDate.Value.Minute == 23));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetime_second_component(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderDate.Value.Second == 44));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetime_millisecond_component(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderDate.Value.Millisecond == 88));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_now_component(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderDate == DateTimeOffset.Now));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_datetimeoffset_utcnow_component(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderDate == DateTimeOffset.UtcNow));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_simple_reversed(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => "London" == c.City),
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_is_null(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_null_is_null(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => null == null),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_constant_is_null(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => "foo" == null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_is_not_null(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City != null),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_null_is_not_null(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => null != null));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_constant_is_not_null(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => "foo" != null),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_identity_comparison(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == c.City),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_in_optimization_multiple(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    from e in ss.Set<Employee>()
                    where c.City == "London"
                        || c.City == "Berlin"
                        || c.CustomerID == "ALFKI"
                        || c.CustomerID == "ABCDE"
                    select new { c, e },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 16);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_not_in_optimization1(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    from e in ss.Set<Employee>()
                    where c.City != "London"
                        && e.City != "London"
                    select new { c, e },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 90);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_not_in_optimization2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    from e in ss.Set<Employee>()
                    where c.City != "London"
                        && c.City != "Berlin"
                    select new { c, e },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 93);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_not_in_optimization3(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    from e in ss.Set<Employee>()
                    where c.City != "London"
                        && c.City != "Berlin"
                        && c.City != "Seattle"
                    select new { c, e },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 92);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_not_in_optimization4(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    from e in ss.Set<Employee>()
                    where c.City != "London"
                        && c.City != "Berlin"
                        && c.City != "Seattle"
                        && c.City != "Lisboa"
                    select new { c, e },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 90);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_select_many_and(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    from e in ss.Set<Employee>()
                    // ReSharper disable ArrangeRedundantParentheses
#pragma warning disable RCS1032 // Remove redundant parentheses.
                    where (c.City == "London" && c.Country == "UK")
                        && (e.City == "London" && e.Country == "UK")
#pragma warning restore RCS1032 // Remove redundant parentheses.
                    select new { c, e },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 10);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_primitive(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Employee>().Select(e => e.EmployeeID).Take(9).Where(i => i == 5));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_primitive_tracked(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Take(9).Where(e => e.EmployeeID == 5),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_primitive_tracked2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Take(9).Select(e => new { e }).Where(e => e.e.EmployeeID == 5),
                e => e.e.EmployeeID,
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bool_member(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => p.Discontinued), entryCount: 8);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bool_member_false(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => !p.Discontinued), entryCount: 69);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bool_client_side_negated(bool isAsync)
        {
            return AssertTranslationFailed(
                () => AssertQuery(
                    isAsync,
                    ss => ss.Set<Product>().Where(p => !ClientFunc(p.ProductID) && p.Discontinued), entryCount: 8));
        }

        private static bool ClientFunc(int id)
        {
            return false;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bool_member_negated_twice(bool isAsync)
        {
            return AssertQuery(
                isAsync,
#pragma warning disable RCS1068 // Simplify logical negation.
#pragma warning disable RCS1033 // Remove redundant boolean literal.
                ss => ss.Set<Product>().Where(p => !!(p.Discontinued == true)), entryCount: 8);
#pragma warning restore RCS1033 // Remove redundant boolean literal.
#pragma warning restore RCS1068 // Simplify logical negation.
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bool_member_shadow(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => EF.Property<bool>(p, "Discontinued")), entryCount: 8);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bool_member_false_shadow(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => !EF.Property<bool>(p, "Discontinued")), entryCount: 69);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bool_member_equals_constant(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => p.Discontinued.Equals(true)), entryCount: 8);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bool_member_in_complex_predicate(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => p.ProductID > 100 && p.Discontinued || (p.Discontinued == true)), entryCount: 8);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bool_member_compared_to_binary_expression(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => p.Discontinued == (p.ProductID > 50)), entryCount: 44);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_not_bool_member_compared_to_not_bool_member(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => !p.Discontinued == !p.Discontinued), entryCount: 77);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_negated_boolean_expression_compared_to_another_negated_boolean_expression(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => !(p.ProductID > 50) == !(p.ProductID > 20)), entryCount: 47);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_not_bool_member_compared_to_binary_expression(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => !p.Discontinued == (p.ProductID > 50)), entryCount: 33);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bool_parameter(bool isAsync)
        {
            var prm = true;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => prm), entryCount: 77);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bool_parameter_compared_to_binary_expression(bool isAsync)
        {
            var prm = true;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => (p.ProductID > 50) != prm), entryCount: 50);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bool_member_and_parameter_compared_to_binary_expression_nested(bool isAsync)
        {
            var prm = true;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => p.Discontinued == ((p.ProductID > 50) != prm)),
                entryCount: 33);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_de_morgan_or_optimized(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => !(p.Discontinued || (p.ProductID < 20))), entryCount: 53);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_de_morgan_and_optimized(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => !(p.Discontinued && (p.ProductID < 20))), entryCount: 74);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_complex_negated_expression_optimized(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => !(!(!p.Discontinued && (p.ProductID < 60)) || !(p.ProductID > 30))), entryCount: 27);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_short_member_comparison(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => p.UnitsInStock > 10), entryCount: 63);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_comparison_to_nullable_bool(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.EndsWith("KI") == ((bool?)true)), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_true(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => true),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_false(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => false));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_bool_closure(bool isAsync)
        {
            var boolean = false;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" && boolean));

            boolean = true;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" && boolean),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_poco_closure(bool isAsync)
        {
            var customer = new Customer { CustomerID = "ALFKI" };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.Equals(customer)).Select(c => c.CustomerID));

            customer = new Customer { CustomerID = "ANATR" };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.Equals(customer)).Select(c => c.CustomerID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_default(bool isAsync)
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
                isAsync,
                ss => ss.Set<Customer>().Where(defaultExpression),
                entryCount: 22);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_expression_invoke_1(bool isAsync)
        {
            Expression<Func<Customer, bool>> expression = c => c.CustomerID == "ALFKI";
            var parameter = Expression.Parameter(typeof(Customer), "c");

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(
                    Expression.Lambda<Func<Customer, bool>>(Expression.Invoke(expression, parameter), parameter)),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_expression_invoke_2(bool isAsync)
        {
            Expression<Func<Order, Customer>> customer = o => o.Customer;
            Expression<Func<Customer, bool>> predicate = c => c.CustomerID == "ALFKI";
            var exp = Expression.Lambda<Func<Order, bool>>(
                Expression.Invoke(predicate, customer.Body),
                customer.Parameters);

            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(exp),
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_concat_string_int_comparison1(bool isAsync)
        {
            var i = 10;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.CustomerID + i == c.CompanyName).Select(c => c.CustomerID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_concat_string_int_comparison2(bool isAsync)
        {
            var i = 10;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => i + c.CustomerID == c.CompanyName).Select(c => c.CustomerID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_concat_string_int_comparison3(bool isAsync)
        {
            var i = 10;
            var j = 21;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => i + 20 + c.CustomerID + j + 42 == c.CompanyName).Select(c => c.CustomerID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_concat_string_int_comparison4(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderID + o.CustomerID == o.CustomerID).Select(c => c.CustomerID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_concat_string_string_comparison(bool isAsync)
        {
            var i = "A";

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => i + c.CustomerID == c.CompanyName).Select(c => c.CustomerID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_string_concat_method_comparison(bool isAsync)
        {
            var i = "A";

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => string.Concat(i, c.CustomerID) == c.CompanyName).Select(c => c.CustomerID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_ternary_boolean_condition_true(bool isAsync)
        {
            var flag = true;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => flag ? p.UnitsInStock >= 20 : p.UnitsInStock < 20),
                entryCount: 51);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_ternary_boolean_condition_false(bool isAsync)
        {
            var flag = false;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => flag ? p.UnitsInStock >= 20 : p.UnitsInStock < 20),
                entryCount: 26);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_ternary_boolean_condition_with_another_condition(bool isAsync)
        {
            var flag = true;
            var productId = 15;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => p.ProductID < productId && (flag ? p.UnitsInStock >= 20 : p.UnitsInStock < 20)),
                entryCount: 9);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_ternary_boolean_condition_with_false_as_result_true(bool isAsync)
        {
            var flag = true;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => flag ? p.UnitsInStock >= 20 : false),
                entryCount: 51);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_ternary_boolean_condition_with_false_as_result_false(bool isAsync)
        {
            var flag = false;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => flag ? p.UnitsInStock >= 20 : false));
        }

        [ConditionalTheory(Skip = "Issue #14672. Cannot eval 'where (new <>f__AnonymousType409`1(x = [c].City) == { x = London })'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_compare_constructed_equal(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(
                    c => new { x = c.City } == new { x = "London" }));
        }

        [ConditionalTheory(
            Skip =
                "Issue #14672. Cannot eval 'where (new <>f__AnonymousType410`2(x = [c].City, y = [c].Country) == { x = London, y = UK })'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_compare_constructed_multi_value_equal(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(
                    c => new { x = c.City, y = c.Country } == new { x = "London", y = "UK" }));
        }

        [ConditionalTheory(
            Skip =
                "Issue #14672. Cannot eval 'where (new <>f__AnonymousType410`2(x = [c].City, y = [c].Country) != { x = London, y = UK })'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_compare_constructed_multi_value_not_equal(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(
                    c => new { x = c.City, y = c.Country } != new { x = "London", y = "UK" }),
                entryCount: 91);
        }

        [ConditionalTheory(Skip = "Issue #14672. Cannot eval 'where (new Tuple`1(Item1 = [c].City) == (London))'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_compare_tuple_constructed_equal(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => new Tuple<string>(c.City) == new Tuple<string>("London")));
        }

        [ConditionalTheory(Skip = "Issue #14672. Cannot eval 'where (new Tuple`2(Item1 = [c].City, Item2 = [c].Country) == (London, UK))'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_compare_tuple_constructed_multi_value_equal(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(
                    c => new Tuple<string, string>(c.City, c.Country) == new Tuple<string, string>("London", "UK")));
        }

        [ConditionalTheory(Skip = "Issue #14672. Cannot eval 'where (new Tuple`2(Item1 = [c].City, Item2 = [c].Country) != (London, UK))'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_compare_tuple_constructed_multi_value_not_equal(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(
                    c => new Tuple<string, string>(c.City, c.Country) != new Tuple<string, string>("London", "UK")),
                entryCount: 91);
        }

        [ConditionalTheory(Skip = "Issue #14672. Cannot eval 'where (Create([c].City) == (London))'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_compare_tuple_create_constructed_equal(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => Tuple.Create(c.City) == Tuple.Create("London")));
        }

        [ConditionalTheory(Skip = "Issue #14672. Cannot eval 'where (Create([c].City, [c].Country) == (London, UK))'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_compare_tuple_create_constructed_multi_value_equal(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => Tuple.Create(c.City, c.Country) == Tuple.Create("London", "UK")));
        }

        [ConditionalTheory(Skip = "Issue #14672. Cannot eval 'where (Create([c].City, [c].Country) != (London, UK))'")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_compare_tuple_create_constructed_multi_value_not_equal(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => Tuple.Create(c.City, c.Country) != Tuple.Create("London", "UK")),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_compare_null(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == null && c.Country == "UK"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_projection(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == "London").Select(c => c.CompanyName));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_Is_on_same_type(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c is Customer),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_chain(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>()
                    .Where(o => o.CustomerID == "QUICK")
                    .Where(o => o.OrderDate > new DateTime(1998, 1, 1)),
                entryCount: 8);
        }

        [ConditionalFact]
        public virtual void Where_navigation_contains()
        {
            using (var context = CreateContext())
            {
                var customer = context.Customers.Include(c => c.Orders).Single(c => c.CustomerID == "ALFKI");
                var orderDetails = context.OrderDetails.Where(od => customer.Orders.Contains(od.Order)).ToList();

                Assert.Equal(12, orderDetails.Count);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_array_index(bool isAsync)
        {
            var customers = new[] { "ALFKI", "ANATR" };

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == customers[0]),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_contains_in_subquery_with_or(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<OrderDetail>().Where(
                    od => ss.Set<Product>().OrderBy(p => p.ProductID).Take(1).Select(p => p.ProductID).Contains(od.ProductID)
                        || ss.Set<Order>().OrderBy(o => o.OrderID).Take(1).Select(o => o.OrderID).Contains(od.OrderID)),
                entryCount: 41);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_multiple_contains_in_subquery_with_and(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<OrderDetail>().Where(
                    od => ss.Set<Product>().OrderBy(p => p.ProductID).Take(20).Select(p => p.ProductID).Contains(od.ProductID)
                        && ss.Set<Order>().OrderBy(o => o.OrderID).Take(10).Select(o => o.OrderID).Contains(od.OrderID)),
                entryCount: 5);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_contains_on_navigation(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => ss.Set<Customer>().Any(c => c.Orders.Contains(o))),
                entryCount: 830);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_FirstOrDefault_is_null(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault() == null),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_FirstOrDefault_compared_to_entity(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(
                    c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault() == new Order { OrderID = 10243 }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Time_of_day_datetime(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Order>().Select(o => o.OrderDate.Value.TimeOfDay));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task TypeBinary_short_circuit(bool isAsync)
        {
            var customer = new Customer();

            return AssertQuery(
                isAsync,
#pragma warning disable CS0184 // 'is' expression's given expression is never of the provided type
                ss => ss.Set<Order>().Where(o => (customer is Order)));
#pragma warning restore CS0184 // 'is' expression's given expression is never of the provided type
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Decimal_cast_to_double_works(bool isAsync)
        {
            var customer = new Customer();

            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => (double?)p.UnitPrice > 100),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_is_conditional(bool isAsync)
        {
            var customer = new Customer();

            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Where(p => p is Product ? false : true));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Enclosing_class_settable_member_generates_parameter(bool isAsync)
        {
            SettableProperty = 4;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderID == SettableProperty));

            SettableProperty = 10;

            await AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderID == SettableProperty));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Enclosing_class_readonly_member_generates_parameter(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderID == ReadOnlyProperty));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Enclosing_class_const_member_does_not_generate_parameter(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderID == ConstantProperty));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Generic_Ilist_contains_translates_to_server(bool isAsync)
        {
            var cities = new List<string> { "Seattle" } as IList<string>;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => cities.Contains(c.City)),
                entryCount: 1);
        }

        private int SettableProperty { get; set; }
        private int ReadOnlyProperty => 5;
        private const int ConstantProperty = 1;

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filter_non_nullable_value_after_FirstOrDefault_on_empty_collection(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(
                    c => ss.Set<Order>().Where(o => o.CustomerID == "John Doe").Select(o => o.CustomerID).FirstOrDefault().Length == 0),
                ss => ss.Set<Customer>().Where(c => false));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Like_with_non_string_column_using_ToString(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => EF.Functions.Like(o.OrderID.ToString(), "%20%")),
                entryCount: 8);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Like_with_non_string_column_using_double_cast(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => EF.Functions.Like((string)(object)o.OrderID, "%20%")),
                ss => ss.Set<Order>().Where(o => EF.Functions.Like(o.OrderID.ToString(), "%20%")),
                entryCount: 8);
        }
    }
}
