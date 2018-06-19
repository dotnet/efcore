// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
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
        public virtual Task Where_simple()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == "London"),
                entryCount: 6);
        }

        private static readonly Expression<Func<Order, bool>> _filter = o => o.CustomerID == "ALFKI";

        [ConditionalFact]
        public virtual Task Where_as_queryable_expression()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.Orders.AsQueryable().Any(_filter)),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual Task Where_simple_closure()
        {
            // ReSharper disable once ConvertToConstant.Local
            var city = "London";

            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == city),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual Task Where_indexer_closure()
        {
            var cities = new[] { "London" };

            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == cities[0]),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual Task Where_dictionary_key_access_closure()
        {
            var predicateMap = new Dictionary<string, string>
            {
                ["City"] = "London"
            };

            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == predicateMap["City"]),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual Task Where_tuple_item_closure()
        {
            var predicateTuple = new Tuple<string, string>("ALFKI", "London");

            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == predicateTuple.Item2),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual Task Where_named_tuple_item_closure()
        {
            (string CustomerID, string City) predicateTuple = ("ALFKI", "London");

            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == predicateTuple.City),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual Task Where_simple_closure_constant()
        {
            // ReSharper disable once ConvertToConstant.Local
            var predicate = true;

            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => predicate),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual async Task Where_simple_closure_via_query_cache()
        {
            var city = "London";

            await AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == city),
                entryCount: 6);

            city = "Seattle";

            await AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == city),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_method_call_nullable_type_closure_via_query_cache()
        {
            var city = new City
            {
                Int = 2
            };

            await AssertQueryAsync<Employee>(
                es => es.Where(e => e.ReportsTo == city.Int),
                entryCount: 5);

            city.Int = 5;

            await AssertQueryAsync<Employee>(
                es => es.Where(e => e.ReportsTo == city.Int),
                entryCount: 3);
        }

        [ConditionalFact]
        public virtual async Task Where_method_call_nullable_type_reverse_closure_via_query_cache()
        {
            var city = new City
            {
                NullableInt = 1
            };

            await AssertQueryAsync<Employee>(
                es => es.Where(e => e.EmployeeID > city.NullableInt),
                entryCount: 8);

            city.NullableInt = 5;

            await AssertQueryAsync<Employee>(
                es => es.Where(e => e.EmployeeID > city.NullableInt),
                entryCount: 4);
        }

        [ConditionalFact]
        public virtual async Task Where_method_call_closure_via_query_cache()
        {
            var city = new City
            {
                InstanceFieldValue = "London"
            };

            await AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == city.GetCity()),
                entryCount: 6);

            city.InstanceFieldValue = "Seattle";

            await AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == city.GetCity()),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_field_access_closure_via_query_cache()
        {
            var city = new City
            {
                InstanceFieldValue = "London"
            };

            await AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == city.InstanceFieldValue),
                entryCount: 6);

            city.InstanceFieldValue = "Seattle";

            await AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == city.InstanceFieldValue),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_property_access_closure_via_query_cache()
        {
            var city = new City
            {
                InstancePropertyValue = "London"
            };

            await AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == city.InstancePropertyValue),
                entryCount: 6);

            city.InstancePropertyValue = "Seattle";

            await AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == city.InstancePropertyValue),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_static_field_access_closure_via_query_cache()
        {
            City.StaticFieldValue = "London";

            await AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == City.StaticFieldValue),
                entryCount: 6);

            City.StaticFieldValue = "Seattle";

            await AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == City.StaticFieldValue),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_static_property_access_closure_via_query_cache()
        {
            City.StaticPropertyValue = "London";

            await AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == City.StaticPropertyValue),
                entryCount: 6);

            City.StaticPropertyValue = "Seattle";

            await AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == City.StaticPropertyValue),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_nested_field_access_closure_via_query_cache()
        {
            var city = new City
            {
                Nested = new City
                {
                    InstanceFieldValue = "London"
                }
            };

            await AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == city.Nested.InstanceFieldValue),
                entryCount: 6);

            city.Nested.InstanceFieldValue = "Seattle";

            await AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == city.Nested.InstanceFieldValue),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_nested_property_access_closure_via_query_cache()
        {
            var city = new City
            {
                Nested = new City
                {
                    InstancePropertyValue = "London"
                }
            };

            await AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == city.Nested.InstancePropertyValue),
                entryCount: 6);

            city.Nested.InstancePropertyValue = "Seattle";

            await AssertQueryAsync<Customer>(
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
                    () => context.Set<Customer>()
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
                    () => context.Set<Customer>()
                        .Where(c => c.City == city.Throw().InstanceFieldValue)
                        .ToList());
            }
        }

        [ConditionalFact]
        public virtual async Task Where_new_instance_field_access_closure_via_query_cache()
        {
            await AssertQueryAsync<Customer>(
                cs => cs.Where(
                    c => c.City == new City
                    {
                        InstanceFieldValue = "London"
                    }.InstanceFieldValue),
                entryCount: 6);

            await AssertQueryAsync<Customer>(
                cs => cs.Where(
                    c => c.City == new City
                    {
                        InstanceFieldValue = "Seattle"
                    }.InstanceFieldValue),
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
        public virtual async Task Where_simple_closure_via_query_cache_nullable_type()
        {
            int? reportsTo = 2;

            await AssertQueryAsync<Employee>(
                es => es.Where(e => e.ReportsTo == reportsTo),
                entryCount: 5);

            reportsTo = 5;

            await AssertQueryAsync<Employee>(
                es => es.Where(e => e.ReportsTo == reportsTo),
                entryCount: 3);

            reportsTo = null;

            await AssertQueryAsync<Employee>(
                es => es.Where(e => e.ReportsTo == reportsTo),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_simple_closure_via_query_cache_nullable_type_reverse()
        {
            int? reportsTo = null;

            await AssertQueryAsync<Employee>(
                es => es.Where(e => e.ReportsTo == reportsTo),
                entryCount: 1);

            reportsTo = 5;

            await AssertQueryAsync<Employee>(
                es => es.Where(e => e.ReportsTo == reportsTo),
                entryCount: 3);

            reportsTo = 2;

            await AssertQueryAsync<Employee>(
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
        public virtual Task Where_simple_shadow()
        {
            return AssertQueryAsync<Employee>(
                es => es.Where(e => EF.Property<string>(e, "Title") == "Sales Representative"),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual Task Where_simple_shadow_projection()
        {
            return AssertQueryAsync<Employee>(
                es => es.Where(e => EF.Property<string>(e, "Title") == "Sales Representative")
                    .Select(e => EF.Property<string>(e, "Title")));
        }

        [ConditionalFact]
        public virtual Task Where_simple_shadow_projection_mixed()
        {
            return AssertQueryAsync<Employee>(
                es => es.Where(e => EF.Property<string>(e, "Title") == "Sales Representative")
                    .Select(
                        e => new
                        {
                            e,
                            Title = EF.Property<string>(e, "Title")
                        }),
                e => e.e.EmployeeID,
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual Task Where_simple_shadow_subquery()
        {
            return AssertQueryAsync<Employee>(
                es => from e in es.OrderBy(e => e.EmployeeID).Take(5)
                      where EF.Property<string>(e, "Title") == "Sales Representative"
                      select e,
                entryCount: 3);
        }

        [ConditionalFact]
        public virtual Task Where_shadow_subquery_FirstOrDefault()
        {
            return AssertQueryAsync<Employee>(
                es =>
                    from e in es
                    where EF.Property<string>(e, "Title")
                          == EF.Property<string>(es.OrderBy(e2 => EF.Property<string>(e2, "Title")).FirstOrDefault(), "Title")
                    select e,
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual Task Where_client()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.IsLondon),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual Task Where_subquery_correlated()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c1 => cs.Any(c2 => c1.CustomerID == c2.CustomerID)),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual Task Where_subquery_correlated_client_eval()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Take(5).OrderBy(c1 => c1.CustomerID).Where(c1 => cs.Any(c2 => c1.CustomerID == c2.CustomerID && c2.IsLondon)),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual Task Where_client_and_server_top_level()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.IsLondon && c.CustomerID != "AROUT"),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual Task Where_client_or_server_top_level()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.IsLondon || c.CustomerID == "ALFKI"),
                entryCount: 7);
        }

        [ConditionalFact]
        public virtual Task Where_client_and_server_non_top_level()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.CustomerID != "ALFKI" == (c.IsLondon && c.CustomerID != "AROUT")),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual Task Where_client_deep_inside_predicate_and_server_top_level()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.CustomerID != "ALFKI" && (c.CustomerID == "MAUMAR" || (c.CustomerID != "AROUT" && c.IsLondon))),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual Task Where_equals_method_string()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City.Equals("London")),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual Task Where_equals_method_int()
        {
            return AssertQueryAsync<Employee>(
                es => es.Where(e => e.EmployeeID.Equals(1)),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual Task Where_equals_using_object_overload_on_mismatched_types()
        {
            ulong longPrm = 1;

            return AssertQueryAsync<Employee>(
                es => es.Where(e => e.EmployeeID.Equals(longPrm)));
        }

        [ConditionalFact]
        public virtual Task Where_equals_using_int_overload_on_mismatched_types()
        {
            ushort shortPrm = 1;

            return AssertQueryAsync<Employee>(
                es => es.Where(e => e.EmployeeID.Equals(shortPrm)),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_equals_on_mismatched_types_nullable_int_long()
        {
            ulong longPrm = 2;

            await AssertQueryAsync<Employee>(
                es => es.Where(e => e.ReportsTo.Equals(longPrm)));

            await AssertQueryAsync<Employee>(
                es => es.Where(e => longPrm.Equals(e.ReportsTo)));
        }

        [ConditionalFact]
        public virtual async Task Where_equals_on_mismatched_types_int_nullable_int()
        {
            uint intPrm = 2;

            await AssertQueryAsync<Employee>(
                es => es.Where(e => e.ReportsTo.Equals(intPrm)),
                entryCount: 5);

            await AssertQueryAsync<Employee>(
                es => es.Where(e => intPrm.Equals(e.ReportsTo)),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual async Task Where_equals_on_mismatched_types_nullable_long_nullable_int()
        {
            ulong? nullableLongPrm = 2;

            await AssertQueryAsync<Employee>(
                es => es.Where(e => nullableLongPrm.Equals(e.ReportsTo)));

            await AssertQueryAsync<Employee>(
                es => es.Where(e => e.ReportsTo.Equals(nullableLongPrm)));
        }

        [ConditionalFact]
        public virtual async Task Where_equals_on_matched_nullable_int_types()
        {
            uint? nullableIntPrm = 2;

            await AssertQueryAsync<Employee>(
                es => es.Where(e => nullableIntPrm.Equals(e.ReportsTo)),
                entryCount: 5);

            await AssertQueryAsync<Employee>(
                es => es.Where(e => e.ReportsTo.Equals(nullableIntPrm)),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual async Task Where_equals_on_null_nullable_int_types()
        {
            uint? nullableIntPrm = null;

            await AssertQueryAsync<Employee>(
                es => es.Where(e => nullableIntPrm.Equals(e.ReportsTo)),
                entryCount: 1);

            await AssertQueryAsync<Employee>(
                es => es.Where(e => e.ReportsTo.Equals(nullableIntPrm)),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual Task Where_comparison_nullable_type_not_null()
        {
            return AssertQueryAsync<Employee>(
                es => es.Where(e => e.ReportsTo == 2),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual Task Where_comparison_nullable_type_null()
        {
            return AssertQueryAsync<Employee>(
                es => es.Where(e => e.ReportsTo == null),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual Task Where_string_length()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City.Length == 6),
                entryCount: 20);
        }

        [ConditionalFact]
        public virtual Task Where_string_indexof()
        {
            // ReSharper disable once StringIndexOfIsCultureSpecific.1
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City.IndexOf("Sea") != -1),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual Task Where_string_replace()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City.Replace("Sea", "Rea") == "Reattle"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual Task Where_string_substring()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City.Substring(1, 2) == "ea"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual Task Where_datetime_now()
        {
            var myDatetime = new DateTime(2015, 4, 10);
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => DateTime.Now != myDatetime),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual Task Where_datetime_utcnow()
        {
            var myDatetime = new DateTime(2015, 4, 10);
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => DateTime.UtcNow != myDatetime),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual Task Where_datetime_today()
        {
            return AssertQueryAsync<Employee>(
                es => es.Where(e => DateTime.Now.Date == DateTime.Today),
                entryCount: 9);
        }

        [ConditionalFact]
        public virtual Task Where_datetime_date_component()
        {
            var myDatetime = new DateTime(1998, 5, 4);
            return AssertQueryAsync<Order>(
                oc => oc.Where(
                    o =>
                        o.OrderDate.Value.Date == myDatetime),
                entryCount: 3);
        }

        [ConditionalFact]
        public virtual Task Where_date_add_year_constant_component()
        {
            return AssertQueryAsync<Order>(
                oc => oc.Where(o => o.OrderDate.Value.AddYears(-1).Year == 1997),
                entryCount: 270);
        }

        [ConditionalFact]
        public virtual Task Where_datetime_year_component()
        {
            return AssertQueryAsync<Order>(
                oc => oc.Where(o => o.OrderDate.Value.Year == 1998),
                entryCount: 270);
        }

        [ConditionalFact]
        public virtual Task Where_datetime_month_component()
        {
            return AssertQueryAsync<Order>(
                oc => oc.Where(o => o.OrderDate.Value.Month == 4),
                entryCount: 105);
        }

        [ConditionalFact]
        public virtual Task Where_datetime_dayOfYear_component()
        {
            return AssertQueryAsync<Order>(
                oc => oc.Where(o => o.OrderDate.Value.DayOfYear == 68),
                entryCount: 3);
        }

        [ConditionalFact]
        public virtual Task Where_datetime_day_component()
        {
            return AssertQueryAsync<Order>(
                oc => oc.Where(o => o.OrderDate.Value.Day == 4),
                entryCount: 27);
        }

        [ConditionalFact]
        public virtual Task Where_datetime_hour_component()
        {
            return AssertQueryAsync<Order>(
                oc => oc.Where(o => o.OrderDate.Value.Hour == 14));
        }

        [ConditionalFact]
        public virtual Task Where_datetime_minute_component()
        {
            return AssertQueryAsync<Order>(
                oc => oc.Where(o => o.OrderDate.Value.Minute == 23));
        }

        [ConditionalFact]
        public virtual Task Where_datetime_second_component()
        {
            return AssertQueryAsync<Order>(
                oc => oc.Where(o => o.OrderDate.Value.Second == 44));
        }

        [ConditionalFact]
        public virtual Task Where_datetime_millisecond_component()
        {
            return AssertQueryAsync<Order>(
                oc => oc.Where(o => o.OrderDate.Value.Millisecond == 88));
        }

        [ConditionalFact]
        public virtual Task Where_datetimeoffset_now_component()
        {
            return AssertQueryAsync<Order>(
                oc => oc.Where(o => o.OrderDate == DateTimeOffset.Now));
        }

        [ConditionalFact]
        public virtual Task Where_datetimeoffset_utcnow_component()
        {
            return AssertQueryAsync<Order>(
                oc => oc.Where(o => o.OrderDate == DateTimeOffset.UtcNow));
        }

        [ConditionalFact]
        public virtual Task Where_simple_reversed()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => "London" == c.City),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual Task Where_is_null()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == null));
        }

        [ConditionalFact]
        public virtual Task Where_null_is_null()
        {
            // ReSharper disable once EqualExpressionComparison
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => null == null),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual Task Where_constant_is_null()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => "foo" == null));
        }

        [ConditionalFact]
        public virtual Task Where_is_not_null()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City != null),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual Task Where_null_is_not_null()
        {
            // ReSharper disable once EqualExpressionComparison
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => null != null));
        }

        [ConditionalFact]
        public virtual Task Where_constant_is_not_null()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => "foo" != null),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual Task Where_identity_comparison()
        {
            // ReSharper disable once EqualExpressionComparison
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == c.City),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual Task Where_in_optimization_multiple()
        {
            return AssertQueryAsync<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == "London"
                          || c.City == "Berlin"
                          || c.CustomerID == "ALFKI"
                          || c.CustomerID == "ABCDE"
                    select new
                    {
                        c,
                        e
                    },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 16);
        }

        [ConditionalFact]
        public virtual Task Where_not_in_optimization1()
        {
            return AssertQueryAsync<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City != "London"
                          && e.City != "London"
                    select new
                    {
                        c,
                        e
                    },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 90);
        }

        [ConditionalFact]
        public virtual Task Where_not_in_optimization2()
        {
            return AssertQueryAsync<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City != "London"
                          && c.City != "Berlin"
                    select new
                    {
                        c,
                        e
                    },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 93);
        }

        [ConditionalFact]
        public virtual Task Where_not_in_optimization3()
        {
            return AssertQueryAsync<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City != "London"
                          && c.City != "Berlin"
                          && c.City != "Seattle"
                    select new
                    {
                        c,
                        e
                    },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 92);
        }

        [ConditionalFact]
        public virtual Task Where_not_in_optimization4()
        {
            return AssertQueryAsync<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City != "London"
                          && c.City != "Berlin"
                          && c.City != "Seattle"
                          && c.City != "Lisboa"
                    select new
                    {
                        c,
                        e
                    },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 90);
        }

        [ConditionalFact]
        public virtual Task Where_select_many_and()
        {
            return AssertQueryAsync<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    // ReSharper disable ArrangeRedundantParentheses
                    where (c.City == "London" && c.Country == "UK")
                          && (e.City == "London" && e.Country == "UK")
                    select new
                    {
                        c,
                        e
                    },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 10);
        }

        [ConditionalFact]
        public virtual Task Where_primitive()
        {
            return AssertQueryScalarAsync<Employee>(
                es => es.Select(e => e.EmployeeID).Take(9).Where(i => i == 5));
        }

        [ConditionalFact]
        public virtual Task Where_primitive_tracked()
        {
            return AssertQueryAsync<Employee>(
                es => es.Take(9).Where(e => e.EmployeeID == 5),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual Task Where_primitive_tracked2()
        {
            return AssertQueryAsync<Employee>(
                es => es.Take(9).Select(
                    e => new
                    {
                        e
                    }).Where(e => e.e.EmployeeID == 5),
                e => e.e.EmployeeID,
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual Task Where_bool_member()
        {
            return AssertQueryAsync<Product>(ps => ps.Where(p => p.Discontinued), entryCount: 8);
        }

        [ConditionalFact]
        public virtual Task Where_bool_member_false()
        {
            return AssertQueryAsync<Product>(ps => ps.Where(p => !p.Discontinued), entryCount: 69);
        }

        [ConditionalFact]
        public virtual Task Where_bool_client_side_negated()
        {
            return AssertQueryAsync<Product>(ps => ps.Where(p => !ClientFunc(p.ProductID) && p.Discontinued), entryCount: 8);
        }

        private static bool ClientFunc(int id)
        {
            return false;
        }

        [ConditionalFact]
        public virtual Task Where_bool_member_negated_twice()
        {
            // ReSharper disable once NegativeEqualityExpression
            // ReSharper disable once DoubleNegationOperator
            // ReSharper disable once RedundantBoolCompare
            return AssertQueryAsync<Product>(ps => ps.Where(p => !!(p.Discontinued == true)), entryCount: 8);
        }

        [ConditionalFact]
        public virtual Task Where_bool_member_shadow()
        {
            return AssertQueryAsync<Product>(ps => ps.Where(p => EF.Property<bool>(p, "Discontinued")), entryCount: 8);
        }

        [ConditionalFact]
        public virtual Task Where_bool_member_false_shadow()
        {
            return AssertQueryAsync<Product>(ps => ps.Where(p => !EF.Property<bool>(p, "Discontinued")), entryCount: 69);
        }

        [ConditionalFact]
        public virtual Task Where_bool_member_equals_constant()
        {
            return AssertQueryAsync<Product>(ps => ps.Where(p => p.Discontinued.Equals(true)), entryCount: 8);
        }

        [ConditionalFact]
        public virtual Task Where_bool_member_in_complex_predicate()
        {
            // ReSharper disable once RedundantBoolCompare
            return AssertQueryAsync<Product>(ps => ps.Where(p => p.ProductID > 100 && p.Discontinued || (p.Discontinued == true)), entryCount: 8);
        }

        [ConditionalFact]
        public virtual Task Where_bool_member_compared_to_binary_expression()
        {
            return AssertQueryAsync<Product>(ps => ps.Where(p => p.Discontinued == (p.ProductID > 50)), entryCount: 44);
        }

        [ConditionalFact]
        public virtual Task Where_not_bool_member_compared_to_not_bool_member()
        {
            return AssertQueryAsync<Product>(ps => ps.Where(p => !p.Discontinued == !p.Discontinued), entryCount: 77);
        }

        [ConditionalFact]
        public virtual Task Where_negated_boolean_expression_compared_to_another_negated_boolean_expression()
        {
            return AssertQueryAsync<Product>(ps => ps.Where(p => !(p.ProductID > 50) == !(p.ProductID > 20)), entryCount: 47);
        }

        [ConditionalFact]
        public virtual Task Where_not_bool_member_compared_to_binary_expression()
        {
            return AssertQueryAsync<Product>(ps => ps.Where(p => !p.Discontinued == (p.ProductID > 50)), entryCount: 33);
        }

        [ConditionalFact]
        public virtual Task Where_bool_parameter()
        {
            var prm = true;
            return AssertQueryAsync<Product>(ps => ps.Where(p => prm), entryCount: 77);
        }

        [ConditionalFact]
        public virtual Task Where_bool_parameter_compared_to_binary_expression()
        {
            var prm = true;
            return AssertQueryAsync<Product>(ps => ps.Where(p => (p.ProductID > 50) != prm), entryCount: 50);
        }

        [ConditionalFact]
        public virtual Task Where_bool_member_and_parameter_compared_to_binary_expression_nested()
        {
            var prm = true;
            return AssertQueryAsync<Product>(ps => ps.Where(p => p.Discontinued == ((p.ProductID > 50) != prm)), entryCount: 33);
        }

        [ConditionalFact]
        public virtual Task Where_de_morgan_or_optimizated()
        {
            return AssertQueryAsync<Product>(ps => ps.Where(p => !(p.Discontinued || (p.ProductID < 20))), entryCount: 53);
        }

        [ConditionalFact]
        public virtual Task Where_de_morgan_and_optimizated()
        {
            return AssertQueryAsync<Product>(ps => ps.Where(p => !(p.Discontinued && (p.ProductID < 20))), entryCount: 74);
        }

        [ConditionalFact]
        public virtual Task Where_complex_negated_expression_optimized()
        {
            return AssertQueryAsync<Product>(ps => ps.Where(p => !(!(!p.Discontinued && (p.ProductID < 60)) || !(p.ProductID > 30))), entryCount: 27);
        }

        [ConditionalFact]
        public virtual Task Where_short_member_comparison()
        {
            return AssertQueryAsync<Product>(ps => ps.Where(p => p.UnitsInStock > 10), entryCount: 63);
        }

        [ConditionalFact]
        public virtual Task Where_comparison_to_nullable_bool()
        {
            return AssertQueryAsync<Customer>(cs => cs.Where(c => c.CustomerID.EndsWith("KI") == ((bool?)true)), entryCount: 1);
        }

        [ConditionalFact]
        public virtual Task Where_true()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => true),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual Task Where_false()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => false));
        }

        [ConditionalFact]
        public virtual async Task Where_bool_closure()
        {
            var boolean = false;

            await AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.CustomerID == "ALFKI" && boolean));

            boolean = true;

            await AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.CustomerID == "ALFKI" && boolean),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Where_poco_closure()
        {
            var customer = new Customer
            {
                CustomerID = "ALFKI"
            };

            await AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.Equals(customer)).Select(c => c.CustomerID));

            customer = new Customer
            {
                CustomerID = "ANATR"
            };

            await AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.Equals(customer)).Select(c => c.CustomerID));
        }

        [ConditionalFact]
        public virtual Task Where_default()
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

            return AssertQueryAsync<Customer>(
                es => es.Where(defaultExpression),
                entryCount: 22);
        }

        [ConditionalFact]
        public virtual Task Where_expression_invoke()
        {
            Expression<Func<Customer, bool>> expression = c => c.CustomerID == "ALFKI";
            var parameter = Expression.Parameter(typeof(Customer), "c");

            return AssertQueryAsync<Customer>(
                cs => cs.Where(
                    Expression.Lambda<Func<Customer, bool>>(Expression.Invoke(expression, parameter), parameter)),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual Task Where_concat_string_int_comparison1()
        {
            var i = 10;
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.CustomerID + i == c.CompanyName).Select(c => c.CustomerID));
        }

        [ConditionalFact]
        public virtual Task Where_concat_string_int_comparison2()
        {
            var i = 10;
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => i + c.CustomerID == c.CompanyName).Select(c => c.CustomerID));
        }

        [ConditionalFact]
        public virtual Task Where_concat_string_int_comparison3()
        {
            var i = 10;
            var j = 21;
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => i + 20 + c.CustomerID + j + 42 == c.CompanyName).Select(c => c.CustomerID));
        }

        [ConditionalFact]
        public virtual Task Where_ternary_boolean_condition_true()
        {
            var flag = true;

            return AssertQueryAsync<Product>(
                ps => ps
                    .Where(p => flag ? p.UnitsInStock >= 20 : p.UnitsInStock < 20),
                entryCount: 51);
        }

        [ConditionalFact]
        public virtual Task Where_ternary_boolean_condition_false()
        {
            var flag = false;

            return AssertQueryAsync<Product>(
                ps => ps
                    .Where(p => flag ? p.UnitsInStock >= 20 : p.UnitsInStock < 20),
                entryCount: 26);
        }

        [ConditionalFact]
        public virtual Task Where_ternary_boolean_condition_with_another_condition()
        {
            var flag = true;
            var productId = 15;

            return AssertQueryAsync<Product>(
                ps => ps
                    .Where(
                        p => p.ProductID < productId
                             && (flag ? p.UnitsInStock >= 20 : p.UnitsInStock < 20)),
                entryCount: 9);
        }

        [ConditionalFact]
        public virtual Task Where_ternary_boolean_condition_with_false_as_result_true()
        {
            var flag = true;

            return AssertQueryAsync<Product>(
                ps => ps
                    // ReSharper disable once SimplifyConditionalTernaryExpression
                    .Where(p => flag ? p.UnitsInStock >= 20 : false),
                entryCount: 51);
        }

        [ConditionalFact]
        public virtual Task Where_ternary_boolean_condition_with_false_as_result_false()
        {
            var flag = false;

            return AssertQueryAsync<Product>(
                ps => ps
                    // ReSharper disable once SimplifyConditionalTernaryExpression
                    .Where(p => flag ? p.UnitsInStock >= 20 : false));
        }

        // TODO: Re-write entity ref equality to identity equality.
        //
        // [ConditionalFact]
        // public virtual Task Where_compare_entity_equal()
        // {
        //     var alfki = NorthwindData.Customers.Single(c => c.CustomerID == "ALFKI");
        //
        //     Assert.Equal(1,
        //         // ReSharper disable once PossibleUnintendedReferenceComparison
        //         AssertQuery<Customer>(cs => cs.Where(c => c == alfki)));
        // }
        //
        // [ConditionalFact]
        // public virtual Task Where_compare_entity_not_equal()
        // {
        //     var alfki = new Customer { CustomerID = "ALFKI" };
        //
        //     Assert.Equal(90,
        //         // ReSharper disable once PossibleUnintendedReferenceComparison
        //         AssertQuery<Customer>(cs => cs.Where(c => c != alfki)));
        //
        // [ConditionalFact]
        // public virtual Task Project_compare_entity_equal()
        // {
        //     var alfki = NorthwindData.Customers.Single(c => c.CustomerID == "ALFKI");
        //
        //     Assert.Equal(1,
        //         // ReSharper disable once PossibleUnintendedReferenceComparison
        //         AssertQuery<Customer>(cs => cs.Select(c => c == alfki)));
        // }
        //
        // [ConditionalFact]
        // public virtual Task Project_compare_entity_not_equal()
        // {
        //     var alfki = new Customer { CustomerID = "ALFKI" };
        //
        //     Assert.Equal(90,
        //         // ReSharper disable once PossibleUnintendedReferenceComparison
        //         AssertQuery<Customer>(cs => cs.Select(c => c != alfki)));
        // }

        [ConditionalFact]
        public virtual Task Where_compare_constructed_equal()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(
                    c => new
                    {
                        x = c.City
                    } == new
                    {
                        x = "London"
                    }));
        }

        [ConditionalFact]
        public virtual Task Where_compare_constructed_multi_value_equal()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(
                    c => new
                    {
                        x = c.City,
                        y = c.Country
                    } == new
                    {
                        x = "London",
                        y = "UK"
                    }));
        }

        [ConditionalFact]
        public virtual Task Where_compare_constructed_multi_value_not_equal()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(
                    c => new
                    {
                        x = c.City,
                        y = c.Country
                    } != new
                    {
                        x = "London",
                        y = "UK"
                    }),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual Task Where_compare_tuple_constructed_equal()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => new Tuple<string>(c.City) == new Tuple<string>("London")));
        }

        [ConditionalFact]
        public virtual Task Where_compare_tuple_constructed_multi_value_equal()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => new Tuple<string, string>(c.City, c.Country) == new Tuple<string, string>("London", "UK")));
        }

        [ConditionalFact]
        public virtual Task Where_compare_tuple_constructed_multi_value_not_equal()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => new Tuple<string, string>(c.City, c.Country) != new Tuple<string, string>("London", "UK")),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual Task Where_compare_tuple_create_constructed_equal()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => Tuple.Create(c.City) == Tuple.Create("London")));
        }

        [ConditionalFact]
        public virtual Task Where_compare_tuple_create_constructed_multi_value_equal()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => Tuple.Create(c.City, c.Country) == Tuple.Create("London", "UK")));
        }

        [ConditionalFact]
        public virtual Task Where_compare_tuple_create_constructed_multi_value_not_equal()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => Tuple.Create(c.City, c.Country) != Tuple.Create("London", "UK")),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual Task Where_compare_null()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == null && c.Country == "UK"));
        }

        [ConditionalFact]
        public virtual Task Where_projection()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.City == "London").Select(c => c.CompanyName));
        }

        [ConditionalFact]
        public virtual Task Where_Is_on_same_type()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c is Customer),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual Task Where_chain()
        {
            return AssertQueryAsync<Order>(
                order => order
                    .Where(o => o.CustomerID == "QUICK")
                    .Where(o => o.OrderDate > new DateTime(1998, 1, 1)), entryCount: 8);
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

        [ConditionalFact]
        public virtual Task Where_array_index()
        {
            var customers = new[] { "ALFKI", "ANATR" };

            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.CustomerID == customers[0]),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual Task Where_multiple_contains_in_subquery_with_or()
        {
            return AssertQueryAsync<OrderDetail, Product, Order>(
                (ods, ps, os) =>
                    ods.Where(
                        od =>
                            ps.OrderBy(p => p.ProductID).Take(1).Select(p => p.ProductID).Contains(od.ProductID)
                            || os.OrderBy(o => o.OrderID).Take(1).Select(o => o.OrderID).Contains(od.OrderID)),
                entryCount: 41);
        }

        [ConditionalFact]
        public virtual Task Where_multiple_contains_in_subquery_with_and()
        {
            return AssertQueryAsync<OrderDetail, Product, Order>(
                (ods, ps, os) =>
                    ods.Where(
                        od =>
                            ps.OrderBy(p => p.ProductID).Take(20).Select(p => p.ProductID).Contains(od.ProductID)
                            && os.OrderBy(o => o.OrderID).Take(10).Select(o => o.OrderID).Contains(od.OrderID)),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual Task Where_contains_on_navigation()
        {
            return AssertQueryAsync<Order, Customer>(
                (os, cs) => os.Where(o => cs.Any(c => c.Orders.Contains(o))),
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual Task Where_subquery_FirstOrDefault_is_null()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault() == null),
                entryCount: 2);
        }

        [ConditionalFact]
        public virtual Task Where_subquery_FirstOrDefault_compared_to_entity()
        {
            return AssertQueryAsync<Customer>(
                cs => cs.Where(
                    c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault() == new Order
                    {
                        OrderID = 10243
                    }));
        }

        [ConditionalFact]
        public virtual Task Time_of_day_datetime()
        {
            return AssertQueryScalarAsync<Order>(
                o => o.Select(c => c.OrderDate.Value.TimeOfDay));
        }
    }
}
