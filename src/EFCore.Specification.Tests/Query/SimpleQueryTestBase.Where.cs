// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

// ReSharper disable AccessToModifiedClosure
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract partial class SimpleQueryTestBase<TFixture>
    {
        [ConditionalFact]
        public virtual void Where_simple()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == "London"),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual void Where_simple_closure()
        {
            // ReSharper disable once ConvertToConstant.Local
            var city = "London";

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual void Where_indexer_closure()
        {
            var cities = new[] { "London" };

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == cities[0]),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual void Where_dictionary_key_access_closure()
        {
            var predicateMap = new Dictionary<string, string> { ["City"] = "London" };

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == predicateMap["City"]),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual void Where_tuple_item_closure()
        {
            var predicateTuple = new Tuple<string, string>("ALFKI", "London");

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == predicateTuple.Item2),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual void Where_named_tuple_item_closure()
        {
            (string CustomerID, string City) predicateTuple = ("ALFKI", "London");

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == predicateTuple.City),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual void Where_simple_closure_constant()
        {
            // ReSharper disable once ConvertToConstant.Local
            var predicate = true;

            AssertQuery<Customer>(
                cs => cs.Where(c => predicate),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Where_simple_closure_via_query_cache()
        {
            var city = "London";

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city),
                entryCount: 6);

            city = "Seattle";

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_method_call_nullable_type_closure_via_query_cache()
        {
            var city = new City { Int = 2 };

            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == city.Int),
                entryCount: 5);

            city.Int = 5;

            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == city.Int),
                entryCount: 3);
        }

        [ConditionalFact]
        public virtual void Where_method_call_nullable_type_reverse_closure_via_query_cache()
        {
            var city = new City { NullableInt = 1 };

            AssertQuery<Employee>(
                es => es.Where(e => e.EmployeeID > city.NullableInt),
                entryCount: 8);

            city.NullableInt = 5;

            AssertQuery<Employee>(
                es => es.Where(e => e.EmployeeID > city.NullableInt),
                entryCount: 4);
        }

        [ConditionalFact]
        public virtual void Where_method_call_closure_via_query_cache()
        {
            var city = new City { InstanceFieldValue = "London" };

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city.GetCity()),
                entryCount: 6);

            city.InstanceFieldValue = "Seattle";

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city.GetCity()),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_field_access_closure_via_query_cache()
        {
            var city = new City { InstanceFieldValue = "London" };

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city.InstanceFieldValue),
                entryCount: 6);

            city.InstanceFieldValue = "Seattle";

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city.InstanceFieldValue),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_property_access_closure_via_query_cache()
        {
            var city = new City { InstancePropertyValue = "London" };

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city.InstancePropertyValue),
                entryCount: 6);

            city.InstancePropertyValue = "Seattle";

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city.InstancePropertyValue),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_static_field_access_closure_via_query_cache()
        {
            City.StaticFieldValue = "London";

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == City.StaticFieldValue),
                entryCount: 6);

            City.StaticFieldValue = "Seattle";

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == City.StaticFieldValue),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_static_property_access_closure_via_query_cache()
        {
            City.StaticPropertyValue = "London";

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == City.StaticPropertyValue),
                entryCount: 6);

            City.StaticPropertyValue = "Seattle";

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == City.StaticPropertyValue),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_nested_field_access_closure_via_query_cache()
        {
            var city = new City { Nested = new City { InstanceFieldValue = "London" } };

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city.Nested.InstanceFieldValue),
                entryCount: 6);

            city.Nested.InstanceFieldValue = "Seattle";

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city.Nested.InstanceFieldValue),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_nested_property_access_closure_via_query_cache()
        {
            var city = new City { Nested = new City { InstancePropertyValue = "London" } };

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city.Nested.InstancePropertyValue),
                entryCount: 6);

            city.Nested.InstancePropertyValue = "Seattle";

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city.Nested.InstancePropertyValue),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_nested_field_access_closure_via_query_cache_error_null()
        {
            var city = new City();

            using (var context = CreateContext())
            {
                Assert.Throws<InvalidOperationException>(
                    () =>
                        context.Set<Customer>()
                            .Where(c => c.City == city.Nested.InstanceFieldValue)
                            .ToList());
            }
        }

        [ConditionalFact]
        public virtual void Where_nested_field_access_closure_via_query_cache_error_method_null()
        {
            var city = new City();

            using (var context = CreateContext())
            {
                Assert.Throws<InvalidOperationException>(
                    () =>
                        context.Set<Customer>()
                            .Where(c => c.City == city.Throw().InstanceFieldValue)
                            .ToList());
            }
        }

        [ConditionalFact]
        public virtual void Where_new_instance_field_access_closure_via_query_cache()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == new City { InstanceFieldValue = "London" }.InstanceFieldValue),
                entryCount: 6);

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == new City { InstanceFieldValue = "Seattle" }.InstanceFieldValue),
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
        public virtual void Where_simple_closure_via_query_cache_nullable_type()
        {
            int? reportsTo = 2;

            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == reportsTo),
                entryCount: 5);

            reportsTo = 5;

            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == reportsTo),
                entryCount: 3);

            reportsTo = null;

            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == reportsTo),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_simple_closure_via_query_cache_nullable_type_reverse()
        {
            int? reportsTo = null;

            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == reportsTo),
                entryCount: 1);

            reportsTo = 5;

            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == reportsTo),
                entryCount: 3);

            reportsTo = 2;

            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == reportsTo),
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

                Assert.Equal(1, customers.Count);

                customerID = "ANATR";

                customers = context.Customers.Where(c => orders.Any(o => o.CustomerID == c.CustomerID)).ToList();

                Assert.Equal("ANATR", customers.Single().CustomerID);
            }
        }

        [ConditionalFact]
        public virtual void Where_simple_shadow()
        {
            AssertQuery<Employee>(
                es => es.Where(e => EF.Property<string>(e, "Title") == "Sales Representative"),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual void Where_simple_shadow_projection()
        {
            AssertQuery<Employee>(
                es => es.Where(e => EF.Property<string>(e, "Title") == "Sales Representative")
                    .Select(e => EF.Property<string>(e, "Title")));
        }

        [ConditionalFact]
        public virtual void Where_simple_shadow_projection_mixed()
        {
            AssertQuery<Employee>(
                es => es.Where(e => EF.Property<string>(e, "Title") == "Sales Representative")
                    .Select(e => new { e, Title = EF.Property<string>(e, "Title") }),
                e => e.e.EmployeeID,
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual void Where_simple_shadow_subquery()
        {
            AssertQuery<Employee>(
                es => from e in es.OrderBy(e => e.EmployeeID).Take(5)
                      where EF.Property<string>(e, "Title") == "Sales Representative"
                      select e,
                entryCount: 3);
        }

        [ConditionalFact]
        public virtual void Where_shadow_subquery_FirstOrDefault()
        {
            AssertQuery<Employee>(
                es =>
                    from e in es
                    where EF.Property<string>(e, "Title")
                          == EF.Property<string>(es.OrderBy(e2 => EF.Property<string>(e2, "Title")).FirstOrDefault(), "Title")
                    select e,
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_client()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.IsLondon),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual void Where_subquery_correlated()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c1 => cs.Any(c2 => c1.CustomerID == c2.CustomerID)),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Where_subquery_correlated_client_eval()
        {
            AssertQuery<Customer>(
                cs => cs.Take(5).OrderBy(c1 => c1.CustomerID).Where(c1 => cs.Any(c2 => c1.CustomerID == c2.CustomerID && c2.IsLondon)),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_client_and_server_top_level()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.IsLondon && c.CustomerID != "AROUT"),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual void Where_client_or_server_top_level()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.IsLondon || c.CustomerID == "ALFKI"),
                entryCount: 7);
        }

        [ConditionalFact]
        public virtual void Where_client_and_server_non_top_level()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID != "ALFKI" == (c.IsLondon && c.CustomerID != "AROUT")),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual void Where_client_deep_inside_predicate_and_server_top_level()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID != "ALFKI" && (c.CustomerID == "MAUMAR" || (c.CustomerID != "AROUT" && c.IsLondon))),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual void Where_equals_method_string()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City.Equals("London")),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual void Where_equals_method_int()
        {
            AssertQuery<Employee>(
                es => es.Where(e => e.EmployeeID.Equals(1)),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_equals_using_object_overload_on_mismatched_types()
        {
#if Test20
            long longPrm = 1;
#else
            ulong longPrm = 1;
#endif

            AssertQuery<Employee>(
                es => es.Where(e => e.EmployeeID.Equals(longPrm)));
        }

        [ConditionalFact]
        public virtual void Where_equals_using_int_overload_on_mismatched_types()
        {
#if Test20
            short shortPrm = 1;
#else
            ushort shortPrm = 1;
#endif

            AssertQuery<Employee>(
                es => es.Where(e => e.EmployeeID.Equals(shortPrm)),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_equals_on_mismatched_types_nullable_int_long()
        {
#if Test20
            long longPrm = 2;
#else
            ulong longPrm = 2;
#endif

            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo.Equals(longPrm)));

            AssertQuery<Employee>(
                es => es.Where(e => longPrm.Equals(e.ReportsTo)));
        }

        [ConditionalFact]
        public virtual void Where_equals_on_mismatched_types_int_nullable_int()
        {
#if Test20
            var intPrm = 2;
#else
            uint intPrm = 2;
#endif

            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo.Equals(intPrm)),
                entryCount: 5);

            AssertQuery<Employee>(
                es => es.Where(e => intPrm.Equals(e.ReportsTo)),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual void Where_equals_on_mismatched_types_nullable_long_nullable_int()
        {
#if Test20
            ulong? nullableLongPrm = 2;
#else
            ulong? nullableLongPrm = 2;
#endif

            AssertQuery<Employee>(
                es => es.Where(e => nullableLongPrm.Equals(e.ReportsTo)));

            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo.Equals(nullableLongPrm)));
        }

        [ConditionalFact]
        public virtual void Where_equals_on_matched_nullable_int_types()
        {
#if Test20
            int? nullableIntPrm = 2;
#else
            uint? nullableIntPrm = 2;
#endif

            AssertQuery<Employee>(
                es => es.Where(e => nullableIntPrm.Equals(e.ReportsTo)),
                entryCount: 5);

            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo.Equals(nullableIntPrm)),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual void Where_equals_on_null_nullable_int_types()
        {
#if Test20
            int? nullableIntPrm = null;
#else
            uint? nullableIntPrm = null;
#endif

            AssertQuery<Employee>(
                es => es.Where(e => nullableIntPrm.Equals(e.ReportsTo)),
                entryCount: 1);

            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo.Equals(nullableIntPrm)),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_comparison_nullable_type_not_null()
        {
            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == 2),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual void Where_comparison_nullable_type_null()
        {
            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == null),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_string_length()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City.Length == 6),
                entryCount: 20);
        }

        [ConditionalFact]
        public virtual void Where_string_indexof()
        {
            // ReSharper disable once StringIndexOfIsCultureSpecific.1
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City.IndexOf("Sea") != -1),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_string_replace()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City.Replace("Sea", "Rea") == "Reattle"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_string_substring()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City.Substring(1, 2) == "ea"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_datetime_now()
        {
            var myDatetime = new DateTime(2015, 4, 10);
            AssertQuery<Customer>(
                cs => cs.Where(c => DateTime.Now != myDatetime),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Where_datetime_utcnow()
        {
            var myDatetime = new DateTime(2015, 4, 10);
            AssertQuery<Customer>(
                cs => cs.Where(c => DateTime.UtcNow != myDatetime),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Where_datetime_today()
        {
            AssertQuery<Employee>(
                es => es.Where(e => DateTime.Now.Date == DateTime.Today),
                entryCount: 9);
        }

        [ConditionalFact]
        public virtual void Where_datetime_date_component()
        {
            var myDatetime = new DateTime(1998, 5, 4);
            AssertQuery<Order>(
                oc => oc.Where(
                    o =>
                        o.OrderDate.Value.Date == myDatetime),
                entryCount: 3);
        }

        [ConditionalFact]
        public virtual void Where_date_add_year_constant_component()
        {
            AssertQuery<Order>(
                oc => oc.Where(o => o.OrderDate.Value.AddYears(-1).Year == 1997),
                entryCount: 270);
        }

        [ConditionalFact]
        public virtual void Where_datetime_year_component()
        {
            AssertQuery<Order>(
                oc => oc.Where(o => o.OrderDate.Value.Year == 1998),
                entryCount: 270);
        }

        [ConditionalFact]
        public virtual void Where_datetime_month_component()
        {
            AssertQuery<Order>(
                oc => oc.Where(o => o.OrderDate.Value.Month == 4),
                entryCount: 105);
        }

        [ConditionalFact]
        public virtual void Where_datetime_dayOfYear_component()
        {
            AssertQuery<Order>(
                oc => oc.Where(o => o.OrderDate.Value.DayOfYear == 68),
                entryCount: 3);
        }

        [ConditionalFact]
        public virtual void Where_datetime_day_component()
        {
            AssertQuery<Order>(
                oc => oc.Where(o => o.OrderDate.Value.Day == 4),
                entryCount: 27);
        }

        [ConditionalFact]
        public virtual void Where_datetime_hour_component()
        {
            AssertQuery<Order>(
                oc => oc.Where(o => o.OrderDate.Value.Hour == 14));
        }

        [ConditionalFact]
        public virtual void Where_datetime_minute_component()
        {
            AssertQuery<Order>(
                oc => oc.Where(o => o.OrderDate.Value.Minute == 23));
        }

        [ConditionalFact]
        public virtual void Where_datetime_second_component()
        {
            AssertQuery<Order>(
                oc => oc.Where(o => o.OrderDate.Value.Second == 44));
        }

        [ConditionalFact]
        public virtual void Where_datetime_millisecond_component()
        {
            AssertQuery<Order>(
                oc => oc.Where(o => o.OrderDate.Value.Millisecond == 88));
        }

        [ConditionalFact]
        public virtual void Where_simple_reversed()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => "London" == c.City),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual void Where_is_null()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == null));
        }

        [ConditionalFact]
        public virtual void Where_null_is_null()
        {
            // ReSharper disable once EqualExpressionComparison
            AssertQuery<Customer>(
                cs => cs.Where(c => null == null),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Where_constant_is_null()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => "foo" == null));
        }

        [ConditionalFact]
        public virtual void Where_is_not_null()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City != null),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Where_null_is_not_null()
        {
            // ReSharper disable once EqualExpressionComparison
            AssertQuery<Customer>(
                cs => cs.Where(c => null != null));
        }

        [ConditionalFact]
        public virtual void Where_constant_is_not_null()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => "foo" != null),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Where_identity_comparison()
        {
            // ReSharper disable once EqualExpressionComparison
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == c.City),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Where_in_optimization_multiple()
        {
            AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == "London"
                          || c.City == "Berlin"
                          || c.CustomerID == "ALFKI"
                          || c.CustomerID == "ABCDE"
                    select new { c, e },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 16);
        }

        [ConditionalFact]
        public virtual void Where_not_in_optimization1()
        {
            AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City != "London"
                          && e.City != "London"
                    select new { c, e },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 90);
        }

        [ConditionalFact]
        public virtual void Where_not_in_optimization2()
        {
            AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City != "London"
                          && c.City != "Berlin"
                    select new { c, e },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 93);
        }

        [ConditionalFact]
        public virtual void Where_not_in_optimization3()
        {
            AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City != "London"
                          && c.City != "Berlin"
                          && c.City != "Seattle"
                    select new { c, e },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 92);
        }

        [ConditionalFact]
        public virtual void Where_not_in_optimization4()
        {
            AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City != "London"
                          && c.City != "Berlin"
                          && c.City != "Seattle"
                          && c.City != "Lisboa"
                    select new { c, e },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 90);
        }

        [ConditionalFact]
        public virtual void Where_select_many_and()
        {
            AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                        // ReSharper disable ArrangeRedundantParentheses
                    where (c.City == "London" && c.Country == "UK")
                          && (e.City == "London" && e.Country == "UK")
                    select new { c, e },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 10);
        }

        [ConditionalFact]
        public virtual void Where_primitive()
        {
            AssertQueryScalar<Employee>(
                es => es.Select(e => e.EmployeeID).Take(9).Where(i => i == 5));
        }

        [ConditionalFact]
        public virtual void Where_primitive_tracked()
        {
            AssertQuery<Employee>(
                es => es.Take(9).Where(e => e.EmployeeID == 5),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_primitive_tracked2()
        {
            AssertQuery<Employee>(
                es => es.Take(9).Select(e => new { e }).Where(e => e.e.EmployeeID == 5),
                e => e.e.EmployeeID,
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_bool_member()
        {
            AssertQuery<Product>(ps => ps.Where(p => p.Discontinued), entryCount: 8);
        }

        [ConditionalFact]
        public virtual void Where_bool_member_false()
        {
            AssertQuery<Product>(ps => ps.Where(p => !p.Discontinued), entryCount: 69);
        }

        [ConditionalFact]
        public virtual void Where_bool_client_side_negated()
        {
            AssertQuery<Product>(ps => ps.Where(p => !ClientFunc(p.ProductID) && p.Discontinued), entryCount: 8);
        }

        private static bool ClientFunc(int id)
        {
            return false;
        }

        [ConditionalFact]
        public virtual void Where_bool_member_negated_twice()
        {
            // ReSharper disable once NegativeEqualityExpression
            // ReSharper disable once DoubleNegationOperator
            // ReSharper disable once RedundantBoolCompare
            AssertQuery<Product>(ps => ps.Where(p => !!(p.Discontinued == true)), entryCount: 8);
        }

        [ConditionalFact]
        public virtual void Where_bool_member_shadow()
        {
            AssertQuery<Product>(ps => ps.Where(p => EF.Property<bool>(p, "Discontinued")), entryCount: 8);
        }

        [ConditionalFact]
        public virtual void Where_bool_member_false_shadow()
        {
            AssertQuery<Product>(ps => ps.Where(p => !EF.Property<bool>(p, "Discontinued")), entryCount: 69);
        }

        [ConditionalFact]
        public virtual void Where_bool_member_equals_constant()
        {
            AssertQuery<Product>(ps => ps.Where(p => p.Discontinued.Equals(true)), entryCount: 8);
        }

        [ConditionalFact]
        public virtual void Where_bool_member_in_complex_predicate()
        {
            // ReSharper disable once RedundantBoolCompare
            AssertQuery<Product>(ps => ps.Where(p => p.ProductID > 100 && p.Discontinued || (p.Discontinued == true)), entryCount: 8);
        }

        [ConditionalFact]
        public virtual void Where_bool_member_compared_to_binary_expression()
        {
            AssertQuery<Product>(ps => ps.Where(p => p.Discontinued == (p.ProductID > 50)), entryCount: 44);
        }

        [ConditionalFact]
        public virtual void Where_not_bool_member_compared_to_not_bool_member()
        {
            AssertQuery<Product>(ps => ps.Where(p => !p.Discontinued == !p.Discontinued), entryCount: 77);
        }

        [ConditionalFact]
        public virtual void Where_negated_boolean_expression_compared_to_another_negated_boolean_expression()
        {
            AssertQuery<Product>(ps => ps.Where(p => !(p.ProductID > 50) == !(p.ProductID > 20)), entryCount: 47);
        }

        [ConditionalFact]
        public virtual void Where_not_bool_member_compared_to_binary_expression()
        {
            AssertQuery<Product>(ps => ps.Where(p => !p.Discontinued == (p.ProductID > 50)), entryCount: 33);
        }

        [ConditionalFact]
        public virtual void Where_bool_parameter()
        {
            var prm = true;
            AssertQuery<Product>(ps => ps.Where(p => prm), entryCount: 77);
        }

        [ConditionalFact]
        public virtual void Where_bool_parameter_compared_to_binary_expression()
        {
            var prm = true;
            AssertQuery<Product>(ps => ps.Where(p => (p.ProductID > 50) != prm), entryCount: 50);
        }

        [ConditionalFact]
        public virtual void Where_bool_member_and_parameter_compared_to_binary_expression_nested()
        {
            var prm = true;
            AssertQuery<Product>(ps => ps.Where(p => p.Discontinued == ((p.ProductID > 50) != prm)), entryCount: 33);
        }

        [ConditionalFact]
        public virtual void Where_de_morgan_or_optimizated()
        {
            AssertQuery<Product>(ps => ps.Where(p => !(p.Discontinued || (p.ProductID < 20))), entryCount: 53);
        }

        [ConditionalFact]
        public virtual void Where_de_morgan_and_optimizated()
        {
            AssertQuery<Product>(ps => ps.Where(p => !(p.Discontinued && (p.ProductID < 20))), entryCount: 74);
        }

        [ConditionalFact]
        public virtual void Where_complex_negated_expression_optimized()
        {
            AssertQuery<Product>(ps => ps.Where(p => !(!(!p.Discontinued && (p.ProductID < 60)) || !(p.ProductID > 30))), entryCount: 27);
        }

        [ConditionalFact]
        public virtual void Where_short_member_comparison()
        {
            AssertQuery<Product>(ps => ps.Where(p => p.UnitsInStock > 10), entryCount: 63);
        }

        [ConditionalFact]
        public virtual void Where_comparison_to_nullable_bool()
        {
            AssertQuery<Customer>(cs => cs.Where(c => c.CustomerID.EndsWith("KI") == ((bool?)true)), entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_true()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => true),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Where_false()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => false));
        }

        [ConditionalFact]
        public virtual void Where_bool_closure()
        {
            var boolean = false;

            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID == "ALFKI" && boolean));

            boolean = true;

            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID == "ALFKI" && boolean),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_poco_closure()
        {
            var customer = new Customer { CustomerID = "ALFKI" };

            AssertQuery<Customer>(
                cs => cs.Where(c => c.Equals(customer)).Select(c => c.CustomerID));

            customer = new Customer { CustomerID = "ANATR" };

            AssertQuery<Customer>(
                cs => cs.Where(c => c.Equals(customer)).Select(c => c.CustomerID));
        }

        [ConditionalFact]
        public virtual void Where_default()
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

            AssertQuery<Customer>(
                es => es.Where(defaultExpression),
                entryCount: 22);
        }

        [ConditionalFact]
        public virtual void Where_expression_invoke()
        {
            Expression<Func<Customer, bool>> expression = c => c.CustomerID == "ALFKI";
            var parameter = Expression.Parameter(typeof(Customer), "c");

            AssertQuery<Customer>(
                cs => cs.Where(
                    Expression.Lambda<Func<Customer, bool>>(Expression.Invoke(expression, parameter), parameter)),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_concat_string_int_comparison1()
        {
            var i = 10;
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID + i == c.CompanyName).Select(c => c.CustomerID));
        }

        [ConditionalFact]
        public virtual void Where_concat_string_int_comparison2()
        {
            var i = 10;
            AssertQuery<Customer>(
                cs => cs.Where(c => i + c.CustomerID == c.CompanyName).Select(c => c.CustomerID));
        }

        [ConditionalFact]
        public virtual void Where_concat_string_int_comparison3()
        {
            var i = 10;
            var j = 21;
            AssertQuery<Customer>(
                cs => cs.Where(c => i + 20 + c.CustomerID + j + 42 == c.CompanyName).Select(c => c.CustomerID));
        }

        [ConditionalFact]
        public virtual void Where_ternary_boolean_condition_true()
        {
            var flag = true;

            AssertQuery<Product>(
                ps => ps
                    .Where(p => flag ? p.UnitsInStock >= 20 : p.UnitsInStock < 20),
                entryCount: 51);
        }

        [ConditionalFact]
        public virtual void Where_ternary_boolean_condition_false()
        {
            var flag = false;

            AssertQuery<Product>(
                ps => ps
                    .Where(p => flag ? p.UnitsInStock >= 20 : p.UnitsInStock < 20),
                entryCount: 26);
        }

        [ConditionalFact]
        public virtual void Where_ternary_boolean_condition_with_another_condition()
        {
            var flag = true;
            var productId = 15;

            AssertQuery<Product>(
                ps => ps
                    .Where(
                        p => p.ProductID < productId
                             && (flag ? p.UnitsInStock >= 20 : p.UnitsInStock < 20)),
                entryCount: 9);
        }

        [ConditionalFact]
        public virtual void Where_ternary_boolean_condition_with_false_as_result_true()
        {
            var flag = true;

            AssertQuery<Product>(
                ps => ps
                    // ReSharper disable once SimplifyConditionalTernaryExpression
                    .Where(p => flag ? p.UnitsInStock >= 20 : false),
                entryCount: 51);
        }

        [ConditionalFact]
        public virtual void Where_ternary_boolean_condition_with_false_as_result_false()
        {
            var flag = false;

            AssertQuery<Product>(
                ps => ps
                    // ReSharper disable once SimplifyConditionalTernaryExpression
                    .Where(p => flag ? p.UnitsInStock >= 20 : false));
        }

        // TODO: Re-write entity ref equality to identity equality.
        //
        // [ConditionalFact]
        // public virtual void Where_compare_entity_equal()
        // {
        //     var alfki = NorthwindData.Customers.Single(c => c.CustomerID == "ALFKI");
        //
        //     Assert.Equal(1,
        //         // ReSharper disable once PossibleUnintendedReferenceComparison
        //         AssertQuery<Customer>(cs => cs.Where(c => c == alfki)));
        // }
        //
        // [ConditionalFact]
        // public virtual void Where_compare_entity_not_equal()
        // {
        //     var alfki = new Customer { CustomerID = "ALFKI" };
        //
        //     Assert.Equal(90,
        //         // ReSharper disable once PossibleUnintendedReferenceComparison
        //         AssertQuery<Customer>(cs => cs.Where(c => c != alfki)));
        //
        // [ConditionalFact]
        // public virtual void Project_compare_entity_equal()
        // {
        //     var alfki = NorthwindData.Customers.Single(c => c.CustomerID == "ALFKI");
        //
        //     Assert.Equal(1,
        //         // ReSharper disable once PossibleUnintendedReferenceComparison
        //         AssertQuery<Customer>(cs => cs.Select(c => c == alfki)));
        // }
        //
        // [ConditionalFact]
        // public virtual void Project_compare_entity_not_equal()
        // {
        //     var alfki = new Customer { CustomerID = "ALFKI" };
        //
        //     Assert.Equal(90,
        //         // ReSharper disable once PossibleUnintendedReferenceComparison
        //         AssertQuery<Customer>(cs => cs.Select(c => c != alfki)));
        // }

        [ConditionalFact]
        public virtual void Where_compare_constructed_equal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => new { x = c.City } == new { x = "London" }));
        }

        [ConditionalFact]
        public virtual void Where_compare_constructed_multi_value_equal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => new { x = c.City, y = c.Country } == new { x = "London", y = "UK" }));
        }

        [ConditionalFact]
        public virtual void Where_compare_constructed_multi_value_not_equal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => new { x = c.City, y = c.Country } != new { x = "London", y = "UK" }),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Where_compare_tuple_constructed_equal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => new Tuple<string>(c.City) == new Tuple<string>("London")));
        }

        [ConditionalFact]
        public virtual void Where_compare_tuple_constructed_multi_value_equal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => new Tuple<string, string>(c.City, c.Country) == new Tuple<string, string>("London", "UK")));
        }

        [ConditionalFact]
        public virtual void Where_compare_tuple_constructed_multi_value_not_equal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => new Tuple<string, string>(c.City, c.Country) != new Tuple<string, string>("London", "UK")),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Where_compare_tuple_create_constructed_equal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => Tuple.Create(c.City) == Tuple.Create("London")));
        }

        [ConditionalFact]
        public virtual void Where_compare_tuple_create_constructed_multi_value_equal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => Tuple.Create(c.City, c.Country) == Tuple.Create("London", "UK")));
        }

        [ConditionalFact]
        public virtual void Where_compare_tuple_create_constructed_multi_value_not_equal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => Tuple.Create(c.City, c.Country) != Tuple.Create("London", "UK")),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Where_compare_null()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == null && c.Country == "UK"));
        }

        [ConditionalFact]
        public virtual void Where_projection()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == "London").Select(c => c.CompanyName));
        }

        [ConditionalFact]
        public virtual void Where_Is_on_same_type()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c is Customer),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Where_chain()
        {
            AssertQuery<Order>(
                order => order
                    .Where(o => o.CustomerID == "QUICK")
                    .Where(o => o.OrderDate > new DateTime(1998, 1, 1)), entryCount: 8);
        }

        [ConditionalFact]
        public virtual void Where_navigation_contains()
        {
            using (var context = CreateContext())
            {
                var cusotmer = context.Customers.Include(c => c.Orders).Single(c => c.CustomerID == "ALFKI");
                var orderDetails = context.OrderDetails.Where(od => cusotmer.Orders.Contains(od.Order)).ToList();

                Assert.Equal(12, orderDetails.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_array_index()
        {
            var customers = new[] { "ALFKI", "ANATR" };

            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID == customers[0]),
                entryCount: 1);
        }
    }
}
