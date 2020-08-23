// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindWhereQueryCosmosTest : NorthwindWhereQueryTestBase<NorthwindQueryCosmosFixture<NoopModelCustomizer>>
    {
        public NorthwindWhereQueryCosmosTest(
            NorthwindQueryCosmosFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_add(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID + 10 == 10258),
                entryCount: 1);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND ((c[""OrderID""] + 10) = 10258))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_subtract(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID - 10 == 10238),
                entryCount: 1);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND ((c[""OrderID""] - 10) = 10238))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_multiply(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID * 1 == 10248),
                entryCount: 1);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND ((c[""OrderID""] * 1) = 10248))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_divide(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID / 1 == 10248),
                entryCount: 1);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND ((c[""OrderID""] / 1) = 10248))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_modulo(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID % 10248 == 0),
                entryCount: 1);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND ((c[""OrderID""] % 10248) = 0))");
        }

        [ConditionalTheory(Skip = "Issue #13168")]
        public override async Task Where_bitwise_or(bool async)
        {
            await base.Where_bitwise_or(async);

            AssertSql(" ");
        }

        public override async Task Where_bitwise_and(bool async)
        {
            await base.Where_bitwise_and(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((c[""CustomerID""] = ""ALFKI"") & (c[""CustomerID""] = ""ANATR"")))");
        }

        [ConditionalTheory(Skip = "Issue #13168")]
        public override async Task Where_bitwise_xor(bool async)
        {
            await base.Where_bitwise_xor(async);

            AssertSql(" ");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_bitwise_leftshift(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => (o.OrderID << 1) == 20496),
                entryCount: 1);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND ((c[""OrderID""] << 1) = 20496))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_bitwise_rightshift(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => (o.OrderID >> 1) == 5124),
                entryCount: 2);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND ((c[""OrderID""] >> 1) = 5124))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_logical_and(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.City == "Seattle" && c.ContactTitle == "Owner"),
                entryCount: 1);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((c[""City""] = ""Seattle"") AND (c[""ContactTitle""] = ""Owner"")))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_logical_or(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" || c.CustomerID == "ANATR"),
                entryCount: 2);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((c[""CustomerID""] = ""ALFKI"") OR (c[""CustomerID""] = ""ANATR"")))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_logical_not(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => !(c.City != "Seattle")),
                entryCount: 1);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND NOT((c[""City""] != ""Seattle"")))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_equality(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Employee>().Where(e => e.ReportsTo == 2),
                entryCount: 5);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] = 2))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_inequality(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Employee>().Where(e => e.ReportsTo != 2),
                entryCount: 4);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] != 2))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_greaterthan(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Employee>().Where(e => e.ReportsTo > 2),
                entryCount: 3);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] > 2))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_greaterthanorequal(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Employee>().Where(e => e.ReportsTo >= 2),
                entryCount: 8);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] >= 2))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_lessthan(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Employee>().Where(e => e.ReportsTo < 2));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] < 2))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_lessthanorequal(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Employee>().Where(e => e.ReportsTo <= 2),
                entryCount: 5);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] <= 2))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_string_concat(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID + "END" == "ALFKIEND"),
                entryCount: 1);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((c[""CustomerID""] || ""END"") = ""ALFKIEND""))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_unary_minus(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => -o.OrderID == -10248),
                entryCount: 1);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (-(c[""OrderID""]) = -10248))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_bitwise_not(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => ~o.OrderID == -10249),
                entryCount: 1);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (~(c[""OrderID""]) = -10249))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_ternary(bool async)
        {
            await AssertQuery(
                async,
#pragma warning disable IDE0029 // Use coalesce expression
                ss => ss.Set<Customer>().Where(c => (c.Region != null ? c.Region : "SP") == "BC"),
#pragma warning restore IDE0029 // Use coalesce expression
                entryCount: 2);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (((c[""Region""] != null) ? c[""Region""] : ""SP"") = ""BC""))");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_coalesce(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => (c.Region ?? "SP") == "BC"),
                entryCount: 2);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (((c[""Region""] != null) ? c[""Region""] : ""SP"") = ""BC""))");
        }

        public override async Task Where_simple(bool async)
        {
            await base.Where_simple(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""London""))");
        }

        private static readonly Expression<Func<Order, bool>> _filter = o => o.CustomerID == "ALFKI";

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_as_queryable_expression(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI").Where(c => c.Orders.AsQueryable().Any(_filter)),
                entryCount: 1);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task<string> Where_simple_closure(bool async)
        {
            var queryString = await base.Where_simple_closure(async);

            AssertSql(
                @"@__city_0='London'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__city_0))");

            Assert.Equal(
                @"-- @__city_0='London'
SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__city_0))", queryString, ignoreLineEndingDifferences: true,
                ignoreWhiteSpaceDifferences: true);

            return null;
        }

        public override async Task Where_indexer_closure(bool async)
        {
            await base.Where_indexer_closure(async);

            AssertSql(
                @"@__p_0='London'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__p_0))");
        }

        public override async Task Where_dictionary_key_access_closure(bool async)
        {
            await base.Where_dictionary_key_access_closure(async);

            AssertSql(
                @"@__get_Item_0='London'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__get_Item_0))");
        }

        public override async Task Where_tuple_item_closure(bool async)
        {
            await base.Where_tuple_item_closure(async);

            AssertSql(
                @"@__predicateTuple_Item2_0='London'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__predicateTuple_Item2_0))");
        }

        public override async Task Where_named_tuple_item_closure(bool async)
        {
            await base.Where_named_tuple_item_closure(async);

            AssertSql(
                @"@__predicateTuple_Item2_0='London'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__predicateTuple_Item2_0))");
        }

        public override async Task Where_simple_closure_constant(bool async)
        {
            await base.Where_simple_closure_constant(async);

            AssertSql(
                @"@__predicate_0='true'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND @__predicate_0)");
        }

        public override async Task Where_simple_closure_via_query_cache(bool async)
        {
            await base.Where_simple_closure_via_query_cache(async);

            AssertSql(
                @"@__city_0='London'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__city_0))",
                //
                @"@__city_0='Seattle'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__city_0))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_method_call_nullable_type_closure_via_query_cache(bool async)
        {
            await base.Where_method_call_nullable_type_closure_via_query_cache(async);

            AssertSql(
                @"@__p_0='2'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] = @__p_0))",
                //
                @"@__p_0='5'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] = @__p_0))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_method_call_nullable_type_reverse_closure_via_query_cache(bool async)
        {
            await base.Where_method_call_nullable_type_reverse_closure_via_query_cache(async);

            AssertSql(
                @"@__p_0='1'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""EmployeeID""] > @__p_0))",
                //
                @"@__p_0='5'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""EmployeeID""] > @__p_0))");
        }

        public override async Task Where_method_call_closure_via_query_cache(bool async)
        {
            await base.Where_method_call_closure_via_query_cache(async);

            AssertSql(
                @"@__GetCity_0='London'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__GetCity_0))",
                //
                @"@__GetCity_0='Seattle'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__GetCity_0))");
        }

        public override async Task Where_field_access_closure_via_query_cache(bool async)
        {
            await base.Where_field_access_closure_via_query_cache(async);

            AssertSql(
                @"@__city_InstanceFieldValue_0='London'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__city_InstanceFieldValue_0))",
                //
                @"@__city_InstanceFieldValue_0='Seattle'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__city_InstanceFieldValue_0))");
        }

        public override async Task Where_property_access_closure_via_query_cache(bool async)
        {
            await base.Where_property_access_closure_via_query_cache(async);

            AssertSql(
                @"@__city_InstancePropertyValue_0='London'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__city_InstancePropertyValue_0))",
                //
                @"@__city_InstancePropertyValue_0='Seattle'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__city_InstancePropertyValue_0))");
        }

        public override async Task Where_static_field_access_closure_via_query_cache(bool async)
        {
            await base.Where_static_field_access_closure_via_query_cache(async);

            AssertSql(
                @"@__StaticFieldValue_0='London'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__StaticFieldValue_0))",
                //
                @"@__StaticFieldValue_0='Seattle'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__StaticFieldValue_0))");
        }

        public override async Task Where_static_property_access_closure_via_query_cache(bool async)
        {
            await base.Where_static_property_access_closure_via_query_cache(async);

            AssertSql(
                @"@__StaticPropertyValue_0='London'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__StaticPropertyValue_0))",
                //
                @"@__StaticPropertyValue_0='Seattle'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__StaticPropertyValue_0))");
        }

        public override async Task Where_nested_field_access_closure_via_query_cache(bool async)
        {
            await base.Where_nested_field_access_closure_via_query_cache(async);

            AssertSql(
                @"@__city_Nested_InstanceFieldValue_0='London'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__city_Nested_InstanceFieldValue_0))",
                //
                @"@__city_Nested_InstanceFieldValue_0='Seattle'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__city_Nested_InstanceFieldValue_0))");
        }

        public override async Task Where_nested_property_access_closure_via_query_cache(bool async)
        {
            await base.Where_nested_property_access_closure_via_query_cache(async);

            AssertSql(
                @"@__city_Nested_InstancePropertyValue_0='London'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__city_Nested_InstancePropertyValue_0))",
                //
                @"@__city_Nested_InstancePropertyValue_0='Seattle'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__city_Nested_InstancePropertyValue_0))");
        }

        public override async Task Where_new_instance_field_access_query_cache(bool async)
        {
            await base.Where_new_instance_field_access_query_cache(async);

            AssertSql(
                @"@__InstanceFieldValue_0='London'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__InstanceFieldValue_0))",
                //
                @"@__InstanceFieldValue_0='Seattle'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__InstanceFieldValue_0))");
        }

        public override async Task Where_new_instance_field_access_closure_via_query_cache(bool async)
        {
            await base.Where_new_instance_field_access_closure_via_query_cache(async);

            AssertSql(
                @"@__InstanceFieldValue_0='London'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__InstanceFieldValue_0))",
                //
                @"@__InstanceFieldValue_0='Seattle'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = @__InstanceFieldValue_0))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_simple_closure_via_query_cache_nullable_type(bool async)
        {
            await base.Where_simple_closure_via_query_cache_nullable_type(async);

            AssertSql(
                @"@__p_0='2'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] = @__p_0))",
                //
                @"@__p_0='5'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] = @__p_0))",
                //
                @"@__p_0=null

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] = @__p_0))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_simple_closure_via_query_cache_nullable_type_reverse(bool async)
        {
            await base.Where_simple_closure_via_query_cache_nullable_type_reverse(async);

            AssertSql(
                @"@__p_0=null

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] = @__p_0))",
                //
                @"@__p_0='5'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] = @__p_0))",
                //
                @"@__p_0='2'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] = @__p_0))");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Where_subquery_closure_via_query_cache()
        {
            base.Where_subquery_closure_via_query_cache();

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_simple_shadow(bool async)
        {
            await base.Where_simple_shadow(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""Title""] = ""Sales Representative""))");
        }

        public override async Task Where_simple_shadow_projection(bool async)
        {
            await base.Where_simple_shadow_projection(async);

            AssertSql(
                @"SELECT c[""Title""]
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""Title""] = ""Sales Representative""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_simple_shadow_subquery(bool async)
        {
            return base.Where_simple_shadow_subquery(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_shadow_subquery_FirstOrDefault(bool async)
        {
            await base.Where_shadow_subquery_FirstOrDefault(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_client(bool async)
        {
            await base.Where_client(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_subquery_correlated(bool async)
        {
            await base.Where_subquery_correlated(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_subquery_correlated_client_eval(bool async)
        {
            await base.Where_subquery_correlated_client_eval(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_client_and_server_top_level(bool async)
        {
            await base.Where_client_and_server_top_level(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_client_or_server_top_level(bool async)
        {
            await base.Where_client_or_server_top_level(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_client_and_server_non_top_level(bool async)
        {
            await base.Where_client_and_server_non_top_level(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_client_deep_inside_predicate_and_server_top_level(bool async)
        {
            await base.Where_client_deep_inside_predicate_and_server_top_level(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_equals_method_string(bool async)
        {
            await base.Where_equals_method_string(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""London""))");
        }

        public override async Task Where_equals_method_int(bool async)
        {
            await base.Where_equals_method_int(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""EmployeeID""] = 1))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_equals_using_object_overload_on_mismatched_types(bool async)
        {
            await base.Where_equals_using_object_overload_on_mismatched_types(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        public override async Task Where_equals_using_int_overload_on_mismatched_types(bool async)
        {
            await base.Where_equals_using_int_overload_on_mismatched_types(async);

            AssertSql(
                @"@__p_0='1'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""EmployeeID""] = @__p_0))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_equals_on_mismatched_types_nullable_int_long(bool async)
        {
            await base.Where_equals_on_mismatched_types_nullable_int_long(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_equals_on_mismatched_types_nullable_long_nullable_int(bool async)
        {
            await base.Where_equals_on_mismatched_types_nullable_long_nullable_int(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_equals_on_mismatched_types_int_nullable_int(bool async)
        {
            await base.Where_equals_on_mismatched_types_int_nullable_int(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_equals_on_matched_nullable_int_types(bool async)
        {
            await base.Where_equals_on_matched_nullable_int_types(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_equals_on_null_nullable_int_types(bool async)
        {
            await base.Where_equals_on_null_nullable_int_types(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        public override async Task Where_comparison_nullable_type_not_null(bool async)
        {
            await base.Where_comparison_nullable_type_not_null(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] = 2))");
        }

        public override async Task Where_comparison_nullable_type_null(bool async)
        {
            await base.Where_comparison_nullable_type_null(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Employee"") AND (c[""ReportsTo""] = null))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_string_length(bool async)
        {
            await base.Where_string_length(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_string_indexof(bool async)
        {
            await base.Where_string_indexof(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_string_replace(bool async)
        {
            await base.Where_string_replace(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_string_substring(bool async)
        {
            await base.Where_string_substring(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_datetime_now(bool async)
        {
            await base.Where_datetime_now(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_datetime_utcnow(bool async)
        {
            await base.Where_datetime_utcnow(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_datetime_today(bool async)
        {
            await base.Where_datetime_today(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_datetime_date_component(bool async)
        {
            await base.Where_datetime_date_component(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_date_add_year_constant_component(bool async)
        {
            await base.Where_date_add_year_constant_component(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_datetime_year_component(bool async)
        {
            await base.Where_datetime_year_component(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_datetime_month_component(bool async)
        {
            await base.Where_datetime_month_component(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_datetime_dayOfYear_component(bool async)
        {
            await base.Where_datetime_dayOfYear_component(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_datetime_day_component(bool async)
        {
            await base.Where_datetime_day_component(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_datetime_hour_component(bool async)
        {
            await base.Where_datetime_hour_component(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_datetime_minute_component(bool async)
        {
            await base.Where_datetime_minute_component(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_datetime_second_component(bool async)
        {
            await base.Where_datetime_second_component(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_datetime_millisecond_component(bool async)
        {
            await base.Where_datetime_millisecond_component(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_datetimeoffset_now_component(bool async)
        {
            await base.Where_datetimeoffset_now_component(async);
            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_datetimeoffset_utcnow_component(bool async)
        {
            await base.Where_datetimeoffset_utcnow_component(async);
            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task Where_simple_reversed(bool async)
        {
            await base.Where_simple_reversed(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (""London"" = c[""City""]))");
        }

        public override async Task Where_is_null(bool async)
        {
            await base.Where_is_null(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = null))");
        }

        public override async Task Where_null_is_null(bool async)
        {
            await base.Where_null_is_null(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_constant_is_null(bool async)
        {
            await base.Where_constant_is_null(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND false)");
        }

        public override async Task Where_is_not_null(bool async)
        {
            await base.Where_is_not_null(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] != null))");
        }

        public override async Task Where_null_is_not_null(bool async)
        {
            await base.Where_null_is_not_null(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND false)");
        }

        public override async Task Where_constant_is_not_null(bool async)
        {
            await base.Where_constant_is_not_null(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_identity_comparison(bool async)
        {
            await base.Where_identity_comparison(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = c[""City""]))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_in_optimization_multiple(bool async)
        {
            await base.Where_in_optimization_multiple(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_not_in_optimization1(bool async)
        {
            await base.Where_not_in_optimization1(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_not_in_optimization2(bool async)
        {
            await base.Where_not_in_optimization2(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_not_in_optimization3(bool async)
        {
            await base.Where_not_in_optimization3(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_not_in_optimization4(bool async)
        {
            await base.Where_not_in_optimization4(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_select_many_and(bool async)
        {
            await base.Where_select_many_and(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_primitive(bool async)
        {
            await base.Where_primitive(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Employee"")");
        }

        public override async Task Where_bool_member(bool async)
        {
            await base.Where_bool_member(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND c[""Discontinued""])");
        }

        public override async Task Where_bool_member_false(bool async)
        {
            await base.Where_bool_member_false(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND NOT(c[""Discontinued""]))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_bool_client_side_negated(bool async)
        {
            await base.Where_bool_client_side_negated(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Product"")");
        }

        public override async Task Where_bool_member_negated_twice(bool async)
        {
            await base.Where_bool_member_negated_twice(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND NOT(NOT((c[""Discontinued""] = true))))");
        }

        public override async Task Where_bool_member_shadow(bool async)
        {
            await base.Where_bool_member_shadow(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND c[""Discontinued""])");
        }

        public override async Task Where_bool_member_false_shadow(bool async)
        {
            await base.Where_bool_member_false_shadow(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND NOT(c[""Discontinued""]))");
        }

        public override async Task Where_bool_member_equals_constant(bool async)
        {
            await base.Where_bool_member_equals_constant(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND (c[""Discontinued""] = true))");
        }

        public override async Task Where_bool_member_in_complex_predicate(bool async)
        {
            await base.Where_bool_member_in_complex_predicate(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND (((c[""ProductID""] > 100) AND c[""Discontinued""]) OR (c[""Discontinued""] = true)))");
        }

        public override async Task Where_bool_member_compared_to_binary_expression(bool async)
        {
            await base.Where_bool_member_compared_to_binary_expression(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND (c[""Discontinued""] = (c[""ProductID""] > 50)))");
        }

        public override async Task Where_not_bool_member_compared_to_not_bool_member(bool async)
        {
            await base.Where_not_bool_member_compared_to_not_bool_member(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND (NOT(c[""Discontinued""]) = NOT(c[""Discontinued""])))");
        }

        public override async Task Where_negated_boolean_expression_compared_to_another_negated_boolean_expression(bool async)
        {
            await base.Where_negated_boolean_expression_compared_to_another_negated_boolean_expression(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND (NOT((c[""ProductID""] > 50)) = NOT((c[""ProductID""] > 20))))");
        }

        public override async Task Where_not_bool_member_compared_to_binary_expression(bool async)
        {
            await base.Where_not_bool_member_compared_to_binary_expression(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND (NOT(c[""Discontinued""]) = (c[""ProductID""] > 50)))");
        }

        public override async Task Where_bool_parameter(bool async)
        {
            await base.Where_bool_parameter(async);

            AssertSql(
                @"@__prm_0='true'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND @__prm_0)");
        }

        public override async Task Where_bool_parameter_compared_to_binary_expression(bool async)
        {
            await base.Where_bool_parameter_compared_to_binary_expression(async);

            AssertSql(
                @"@__prm_0='true'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND ((c[""ProductID""] > 50) != @__prm_0))");
        }

        public override async Task Where_bool_member_and_parameter_compared_to_binary_expression_nested(bool async)
        {
            await base.Where_bool_member_and_parameter_compared_to_binary_expression_nested(async);

            AssertSql(
                @"@__prm_0='true'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND (c[""Discontinued""] = ((c[""ProductID""] > 50) != @__prm_0)))");
        }

        public override async Task Where_de_morgan_or_optimized(bool async)
        {
            await base.Where_de_morgan_or_optimized(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND NOT((c[""Discontinued""] OR (c[""ProductID""] < 20))))");
        }

        public override async Task Where_de_morgan_and_optimized(bool async)
        {
            await base.Where_de_morgan_and_optimized(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND NOT((c[""Discontinued""] AND (c[""ProductID""] < 20))))");
        }

        public override async Task Where_complex_negated_expression_optimized(bool async)
        {
            await base.Where_complex_negated_expression_optimized(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND NOT((NOT((NOT(c[""Discontinued""]) AND (c[""ProductID""] < 60))) OR NOT((c[""ProductID""] > 30)))))");
        }

        public override async Task Where_short_member_comparison(bool async)
        {
            await base.Where_short_member_comparison(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND (c[""UnitsInStock""] > 10))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_comparison_to_nullable_bool(bool async)
        {
            await base.Where_comparison_to_nullable_bool(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_true(bool async)
        {
            await base.Where_true(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_false(bool async)
        {
            await base.Where_false(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND false)");
        }

        public override async Task Where_bool_closure(bool async)
        {
            await base.Where_bool_closure(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND false)",
                //
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((c[""CustomerID""] = ""ALFKI"") AND true))");
        }

        public override async Task Where_default(bool async)
        {
            await base.Where_default(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""Fax""] = null))");
        }

        public override async Task Where_expression_invoke_1(bool async)
        {
            await base.Where_expression_invoke_1(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_expression_invoke_2(bool async)
        {
            await base.Where_expression_invoke_2(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Where_expression_invoke_3(bool async)
        {
            await base.Where_expression_invoke_3(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_concat_string_int_comparison1(bool async)
        {
            await base.Where_concat_string_int_comparison1(async);

            AssertSql(
                @"@__i_0='10'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((c[""CustomerID""] || @__i_0) = c[""CompanyName""]))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_concat_string_int_comparison2(bool async)
        {
            await base.Where_concat_string_int_comparison2(async);

            AssertSql(
                @"@__i_0='10'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((@__i_0 + c[""CustomerID""]) = c[""CompanyName""]))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_concat_string_int_comparison3(bool async)
        {
            await base.Where_concat_string_int_comparison3(async);

            AssertSql(
                @"@__p_0='30'
@__j_1='21'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((((@__p_0 + c[""CustomerID""]) || @__j_1) || 42) = c[""CompanyName""]))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_concat_string_int_comparison4(bool async)
        {
            return base.Where_concat_string_int_comparison4(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_string_concat_method_comparison(bool async)
        {
            return base.Where_string_concat_method_comparison(async);
        }

        public override async Task Where_ternary_boolean_condition_true(bool async)
        {
            await base.Where_ternary_boolean_condition_true(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND (c[""UnitsInStock""] >= 20))");
        }

        public override async Task Where_ternary_boolean_condition_false(bool async)
        {
            await base.Where_ternary_boolean_condition_false(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND (c[""UnitsInStock""] < 20))");
        }

        public override async Task Where_ternary_boolean_condition_with_another_condition(bool async)
        {
            await base.Where_ternary_boolean_condition_with_another_condition(async);

            AssertSql(
                @"@__productId_0='15'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND ((c[""ProductID""] < @__productId_0) AND (c[""UnitsInStock""] >= 20)))");
        }

        public override async Task Where_ternary_boolean_condition_with_false_as_result_true(bool async)
        {
            await base.Where_ternary_boolean_condition_with_false_as_result_true(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND (c[""UnitsInStock""] >= 20))");
        }

        public override async Task Where_ternary_boolean_condition_with_false_as_result_false(bool async)
        {
            await base.Where_ternary_boolean_condition_with_false_as_result_false(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND false)");
        }

        public override async Task Where_compare_constructed_equal(bool async)
        {
            await base.Where_compare_constructed_equal(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_compare_constructed_multi_value_equal(bool async)
        {
            await base.Where_compare_constructed_multi_value_equal(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_compare_constructed_multi_value_not_equal(bool async)
        {
            await base.Where_compare_constructed_multi_value_not_equal(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_compare_tuple_constructed_equal(bool async)
        {
            await base.Where_compare_tuple_constructed_equal(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_compare_tuple_constructed_multi_value_equal(bool async)
        {
            await base.Where_compare_tuple_constructed_multi_value_equal(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_compare_tuple_constructed_multi_value_not_equal(bool async)
        {
            await base.Where_compare_tuple_constructed_multi_value_not_equal(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_compare_tuple_create_constructed_equal(bool async)
        {
            await base.Where_compare_tuple_create_constructed_equal(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_compare_tuple_create_constructed_multi_value_equal(bool async)
        {
            await base.Where_compare_tuple_create_constructed_multi_value_equal(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_compare_tuple_create_constructed_multi_value_not_equal(bool async)
        {
            await base.Where_compare_tuple_create_constructed_multi_value_not_equal(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_compare_null(bool async)
        {
            await base.Where_compare_null(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND ((c[""City""] = null) AND (c[""Country""] = ""UK"")))");
        }

        public override async Task Where_Is_on_same_type(bool async)
        {
            await base.Where_Is_on_same_type(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        public override async Task Where_chain(bool async)
        {
            await base.Where_chain(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE (((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""QUICK"")) AND (c[""OrderDate""] > ""1998-01-01T00:00:00""))");
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Where_navigation_contains()
        {
            base.Where_navigation_contains();

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Where_array_index(bool async)
        {
            await base.Where_array_index(async);

            AssertSql(
                @"@__p_0='ALFKI'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = @__p_0))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_multiple_contains_in_subquery_with_or(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID < 10250).Where(
                    od => ss.Set<Product>().OrderBy(p => p.ProductID).Take(1).Select(p => p.ProductID).Contains(od.ProductID)
                        || ss.Set<Order>().OrderBy(o => o.OrderID).Take(1).Select(o => o.OrderID).Contains(od.OrderID)),
                entryCount: 3);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] < 10250))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_multiple_contains_in_subquery_with_and(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID < 10260).Where(
                    od => ss.Set<Product>().OrderBy(p => p.ProductID).Take(20).Select(p => p.ProductID).Contains(od.ProductID)
                        && ss.Set<Order>().OrderBy(o => o.OrderID).Take(10).Select(o => o.OrderID).Contains(od.OrderID)),
                entryCount: 5);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""OrderDetail"") AND (c[""OrderID""] < 10260))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_contains_on_navigation(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID > 10354 && o.OrderID < 10360)
                    .Where(
                        o => ss.Set<Customer>().Where(c => c.City == "London")
                            .Any(c => c.Orders.Contains(o))),
                entryCount: 2);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND ((c[""OrderID""] > 10354) AND (c[""OrderID""] < 10360)))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_subquery_FirstOrDefault_is_null(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "PARIS")
                    .Where(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault() == null),
                entryCount: 1);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""PARIS""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Where_subquery_FirstOrDefault_compared_to_entity(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI").Where(
                    c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault() == new Order { OrderID = 10243 }));

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        public override async Task Time_of_day_datetime(bool async)
        {
            await base.Time_of_day_datetime(async);

            AssertSql(
                @"SELECT c[""OrderDate""]
FROM root c
WHERE (c[""Discriminator""] = ""Order"")");
        }

        public override async Task TypeBinary_short_circuit(bool async)
        {
            await base.TypeBinary_short_circuit(async);

            AssertSql(
                @"@__p_0='false'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND @__p_0)");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Decimal_cast_to_double_works(bool async)
        {
            return base.Decimal_cast_to_double_works(async);
        }

        public override async Task Where_is_conditional(bool async)
        {
            await base.Where_is_conditional(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Product"") AND (true ? false : true))");
        }

        [ConditionalTheory(Skip = "Issue#17246")]
        public override Task Filter_non_nullable_value_after_FirstOrDefault_on_empty_collection(bool async)
        {
            return base.Filter_non_nullable_value_after_FirstOrDefault_on_empty_collection(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Like_with_non_string_column_using_ToString(bool async)
        {
            return base.Like_with_non_string_column_using_ToString(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Like_with_non_string_column_using_double_cast(bool async)
        {
            return base.Like_with_non_string_column_using_double_cast(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Using_same_parameter_twice_in_query_generates_one_sql_parameter(bool async)
        {
            await base.Using_same_parameter_twice_in_query_generates_one_sql_parameter(async);

            AssertSql(" ");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_Queryable_ToList_Count(bool async)
        {
            return base.Where_Queryable_ToList_Count(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_Queryable_ToList_Contains(bool async)
        {
            return base.Where_Queryable_ToList_Contains(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_Queryable_ToArray_Count(bool async)
        {
            return base.Where_Queryable_ToArray_Count(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_Queryable_ToArray_Contains(bool async)
        {
            return base.Where_Queryable_ToArray_Contains(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_Queryable_AsEnumerable_Count(bool async)
        {
            return base.Where_Queryable_AsEnumerable_Count(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_Queryable_AsEnumerable_Contains(bool async)
        {
            return base.Where_Queryable_AsEnumerable_Contains(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_Queryable_ToList_Count_member(bool async)
        {
            return base.Where_Queryable_ToList_Count_member(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_Queryable_ToArray_Length_member(bool async)
        {
            return base.Where_Queryable_ToArray_Length_member(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_collection_navigation_ToList_Count(bool async)
        {
            return base.Where_collection_navigation_ToList_Count(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_collection_navigation_ToList_Contains(bool async)
        {
            return base.Where_collection_navigation_ToList_Contains(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_collection_navigation_ToArray_Count(bool async)
        {
            return base.Where_collection_navigation_ToArray_Count(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_collection_navigation_ToArray_Contains(bool async)
        {
            return base.Where_collection_navigation_ToArray_Contains(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_collection_navigation_AsEnumerable_Count(bool async)
        {
            return base.Where_collection_navigation_AsEnumerable_Count(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_collection_navigation_AsEnumerable_Contains(bool async)
        {
            return base.Where_collection_navigation_AsEnumerable_Contains(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_collection_navigation_ToList_Count_member(bool async)
        {
            return base.Where_collection_navigation_ToList_Count_member(async);
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task Where_collection_navigation_ToArray_Length_member(bool async)
        {
            return base.Where_collection_navigation_ToArray_Length_member(async);
        }

        [ConditionalTheory(Skip = "Issue#17246 (Contains over subquery is not supported")]
        public override Task Where_Queryable_AsEnumerable_Contains_negated(bool async)
        {
            return base.Where_Queryable_AsEnumerable_Contains_negated(async);
        }

        public override async Task Where_list_object_contains_over_value_type(bool async)
        {
            await base.Where_list_object_contains_over_value_type(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND c[""OrderID""] IN (10248, 10249))");
        }

        public override async Task Where_array_of_object_contains_over_value_type(bool async)
        {
            await base.Where_array_of_object_contains_over_value_type(async);

            AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND c[""OrderID""] IN (10248, 10249))");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
