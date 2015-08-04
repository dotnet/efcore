// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Tests;
using Xunit;

// ReSharper disable AccessToModifiedClosure

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class QueryTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryFixtureBase, new()
    {
        [Fact]
        public virtual void Queryable_simple()
        {
            AssertQuery<Customer>(
                cs => cs,
                entryCount: 91);
        }

        [Fact]
        public virtual void Queryable_simple_anonymous()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c }),
                entryCount: 91);
        }

        [Fact]
        public virtual void Queryable_simple_anonymous_projection_subquery()
        {
            AssertQuery<Customer>(
                cs => cs.Take(91).Select(c => new { c }).Select(a => a.c.City));
        }

        [Fact]
        public virtual void Queryable_simple_anonymous_subquery()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c }).Take(91).Select(a => a.c));
        }

        [Fact]
        public virtual void Queryable_nested_simple()
        {
            AssertQuery<Customer>(
                cs =>
                    from c1 in (from c2 in (from c3 in cs select c3) select c2) select c1,
                entryCount: 91);
        }

        [Fact]
        public virtual void Take_simple()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Take(10),
                assertOrder: true,
                entryCount: 10);
        }

        [Fact]
        public virtual void Take_simple_projection()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Select(c => c.City).Take(10),
                assertOrder: true);
        }

        [Fact]
        public virtual void Skip()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Skip(5),
                assertOrder: true,
                entryCount: 86);
        }

        [Fact]
        public virtual void Skip_no_orderby()
        {
            AssertQuery<Customer>(
                cs => cs.Skip(5),
                entryCount: 86,
                asserter: (_, __) => { /* non-deterministic */ });
        }

        [Fact]
        public virtual void Take_Skip()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Take(10).Skip(5),
                assertOrder: true,
                entryCount: 5);
        }

        [Fact]
        public virtual void Distinct_Skip()
        {
            AssertQuery<Customer>(
                cs => cs.Distinct().OrderBy(c => c.CustomerID).Skip(5),
                assertOrder: true,
                entryCount: 86);
        }

        [Fact]
        public virtual void Skip_Take()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Skip(5).Take(10),
                assertOrder: true,
                entryCount: 10);
        }

        [Fact]
        public virtual void Distinct_Skip_Take()
        {
            AssertQuery<Customer>(
                cs => cs.Distinct().OrderBy(c => c.ContactName).Skip(5).Take(10),
                assertOrder: true,
                entryCount: 10);
        }

        [Fact]
        public virtual void Skip_Distinct()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Skip(5).Distinct(),
                entryCount: 86);
        }

        [Fact]
        public virtual void Skip_Take_Distinct()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Skip(5).Take(10).Distinct(),
                entryCount: 10);
        }

        [Fact]
        public virtual void Take_Skip_Distinct()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Take(10).Skip(5).Distinct(),
                entryCount: 5);
        }

        [Fact]
        public virtual void Take_Distinct()
        {
            AssertQuery<Order>(
                os => os.OrderBy(o => o.OrderID).Take(5).Distinct(),
                entryCount: 5);
        }

        [Fact]
        public virtual void Distinct_Take()
        {
            AssertQuery<Order>(
                os => os.Distinct().OrderBy(o => o.OrderID).Take(5),
                assertOrder: true,
                entryCount: 5);
        }

        [Fact]
        public virtual void Distinct_Take_Count()
        {
            AssertQuery<Order>(
                os => os.Distinct().Take(5).Count());
        }

        [Fact]
        public virtual void Take_Distinct_Count()
        {
            AssertQuery<Order>(
                os => os.Take(5).Distinct().Count());
        }

        [Fact]
        public virtual void Take_Where_Distinct_Count()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.CustomerID == "FRANK").Take(5).Distinct().Count());
        }

        [Fact]
        public virtual void Any_simple()
        {
            AssertQuery<Customer>(
                cs => cs.Any());
        }

        [Fact]
        public virtual void OrderBy_Take_Count()
        {
            AssertQuery<Order>(
                   os => os.OrderBy(o => o.OrderID).Take(5).Count());
        }

        [Fact]
        public virtual void Take_OrderBy_Count()
        {
            AssertQuery<Order>(
                   os => os.Take(5).OrderBy(o => o.OrderID).Count());
        }

        [Fact]
        public virtual void Any_predicate()
        {
            AssertQuery<Customer>(
                cs => cs.Any(c => c.ContactName.StartsWith("A")));
        }

        [Fact]
        public virtual void All_top_level()
        {
            AssertQuery<Customer>(
                cs => cs.All(c => c.ContactName.StartsWith("A")));
        }

        [Fact]
        public virtual void All_top_level_subquery()
        {
            // ReSharper disable once PossibleUnintendedReferenceComparison
            AssertQuery<Customer>(
                cs => cs.All(c1 => cs.Any(c2 => cs.Any(c3 => c1 == c3))));
        }

        [Fact]
        public virtual void All_client()
        {
            AssertQuery<Customer>(
                cs => cs.All(c => c.IsLondon));
        }

        [Fact]
        public virtual void All_client_and_server_top_level()
        {
            AssertQuery<Customer>(
                cs => cs.All(c => c.CustomerID != "Foo" && c.IsLondon));
        }

        [Fact]
        public virtual void All_client_or_server_top_level()
        {
            AssertQuery<Customer>(
                cs => cs.All(c => c.CustomerID != "Foo" || c.IsLondon));
        }

        [Fact]
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

        [Fact]
        public virtual void Projection_when_arithmetic_expressions()
        {
            AssertQuery<Order>(
                os => os.Select(o => new
                {
                    o.OrderID,
                    Double = o.OrderID * 2,
                    Add = o.OrderID + 23,
                    Sub = 100000 - o.OrderID,
                    Divide = o.OrderID / (o.OrderID / 2),
                    Literal = 42,
                    o
                }),
                entryCount: 830);
        }

        [Fact]
        public virtual void Projection_when_arithmetic_mixed()
        {
            AssertQuery<Order, Employee>((os, es) =>
                from o in os
                from e in es
                select new
                {
                    Add = e.EmployeeID + o.OrderID,
                    o.OrderID,
                    o,
                    Literal = 42,
                    e.EmployeeID,
                    e
                });
        }

        [Fact]
        public virtual void Projection_when_arithmetic_mixed_subqueries()
        {
            AssertQuery<Order, Employee>((os, es) =>
                from o in os.Select(o2 => new { o2, Mod = o2.OrderID % 2 })
                from e in es.Select(e2 => new { e2, Square = e2.EmployeeID ^ 2 })
                select new
                {
                    Add = e.e2.EmployeeID + o.o2.OrderID,
                    e.Square,
                    e.e2,
                    Literal = 42,
                    o.o2,
                    o.Mod
                });
        }

        [Fact]
        public virtual void Projection_when_null_value()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => c.Region));
        }

        [Fact]
        public virtual void Take_with_single()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Take(1).Single());
        }

        [Fact]
        public virtual void Take_with_single_select_many()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                (from c in cs
                 from o in os
                 orderby c.CustomerID, o.OrderID
                 select new { c, o })
                    .Take(1)
                    .Cast<object>()
                    .Single());
        }

        [Fact]
        public virtual void Cast_results_to_object()
        {
            AssertQuery<Customer>(cs => from c in cs.Cast<object>() select c, entryCount: 91);
        }

        [Fact]
        public virtual void Where_simple()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == "London"),
                entryCount: 6);
        }

        [Fact]
        public virtual void Where_simple_closure()
        {
            // ReSharper disable once ConvertToConstant.Local
            var city = "London";

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == city),
                entryCount: 6);
        }

        [Fact]
        public virtual void Where_simple_closure_constant()
        {
            // ReSharper disable once ConvertToConstant.Local
            var predicate = true;

            AssertQuery<Customer>(
                cs => cs.Where(c => predicate),
                entryCount: 91);
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public virtual void Where_nested_field_access_closure_via_query_cache_error_null()
        {
            var city = new City();

            using (var context = CreateContext())
            {
                Assert.Throws<InvalidOperationException>(() =>
                    context.Set<Customer>()
                        .Where(c => c.City == city.Nested.InstanceFieldValue)
                        .ToList());
            }
        }

        [Fact]
        public virtual void Where_nested_field_access_closure_via_query_cache_error_method_null()
        {
            var city = new City();

            using (var context = CreateContext())
            {
                Assert.Throws<InvalidOperationException>(() =>
                    context.Set<Customer>()
                        .Where(c => c.City == city.Throw().InstanceFieldValue)
                        .ToList());
            }
        }

        [Fact]
        public virtual void Where_new_instance_field_access_closure_via_query_cache()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == new City { InstanceFieldValue = "London" }.InstanceFieldValue),
                entryCount: 6);

            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == new City { InstanceFieldValue = "Seattle" }.InstanceFieldValue),
                entryCount: 1);
        }

        [Fact]
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

        [Fact]
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

        [Fact]
        public virtual void Where_simple_shadow()
        {
            AssertQuery<Employee>(
                es => es.Where(e => EF.Property<string>(e, "Title") == "Sales Representative"),
                entryCount: 6);
        }

        [Fact]
        public virtual void Where_simple_shadow_projection()
        {
            AssertQuery<Employee>(
                es => es.Where(e => EF.Property<string>(e, "Title") == "Sales Representative")
                    .Select(e => EF.Property<string>(e, "Title")));
        }

        public virtual void Where_simple_shadow_projection_mixed()
        {
            AssertQuery<Employee>(
                es => es.Where(e => EF.Property<string>(e, "Title") == "Sales Representative")
                    .Select(e => new { e, Title = EF.Property<string>(e, "Title") }),
                entryCount: 6);
        }

        [Fact]
        public virtual void Where_simple_shadow_subquery()
        {
            AssertQuery<Employee>(
                es => from e in es.OrderBy(e => e.EmployeeID).Take(5)
                      where EF.Property<string>(e, "Title") == "Sales Representative"
                      select e,
                entryCount: 3);
        }

        [Fact]
        public virtual void Where_shadow_subquery_first()
        {
            AssertQuery<Employee>(es =>
                from e in es
                where EF.Property<string>(e, "Title")
                      == EF.Property<string>(es.OrderBy(e2 => EF.Property<string>(e2, "Title")).First(), "Title")
                select e,
                entryCount: 1);
        }

        [Fact]
        public virtual void Where_client()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.IsLondon),
                entryCount: 6);
        }

        [Fact]
        public virtual void Where_subquery_correlated()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c1 => cs.Any(c2 => c1.CustomerID == c2.CustomerID)),
                entryCount: 91);
        }

        [Fact]
        public virtual void Where_subquery_correlated_client_eval()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c1 => cs.Any(c2 => c1.CustomerID == c2.CustomerID && c2.IsLondon)),
                entryCount: 6);
        }

        [Fact]
        public virtual void Where_client_and_server_top_level()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.IsLondon && c.CustomerID != "AROUT"),
                entryCount: 5);
        }

        [Fact]
        public virtual void Where_client_or_server_top_level()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.IsLondon || c.CustomerID == "ALFKI"),
                entryCount: 7);
        }

        [Fact]
        public virtual void Where_client_and_server_non_top_level()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID != "ALFKI" == (c.IsLondon && c.CustomerID != "AROUT")),
                entryCount: 6);
        }

        [Fact]
        public virtual void Where_client_deep_inside_predicate_and_server_top_level()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID != "ALFKI" && (c.CustomerID == "MAUMAR" || (c.CustomerID != "AROUT" && c.IsLondon))),
                entryCount: 5);
        }

        [Fact]
        public virtual void First_client_predicate()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).First(c => c.IsLondon));
        }

        [Fact]
        public virtual void Where_equals_method_string()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City.Equals("London")),
                entryCount: 6);
        }

        [Fact]
        public virtual void Where_equals_method_int()
        {
            AssertQuery<Employee>(
                es => es.Where(e => e.EmployeeID.Equals(1)),
                entryCount: 1);
        }

        [Fact]
        public virtual void Where_equals_using_object_overload_on_mismatched_types()
        {
            long longPrm = 1;

            AssertQuery<Employee>(
                es => es.Where(e => e.EmployeeID.Equals(longPrm)),
                entryCount: 0);
        }

        [Fact]
        public virtual void Where_equals_using_int_overload_on_mismatched_types()
        {
            short shortPrm = 1;

            AssertQuery<Employee>(
                es => es.Where(e => e.EmployeeID.Equals(shortPrm)),
                entryCount: 1);
        }

        [Fact]
        public virtual void Where_equals_on_mismatched_types_nullable_int_long()
        {
            long longPrm = 2;

            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo.Equals(longPrm)),
                entryCount: 0);

            AssertQuery<Employee>(
                es => es.Where(e => longPrm.Equals(e.ReportsTo)),
                entryCount: 0);
        }

        [Fact]
        public virtual void Where_equals_on_mismatched_types_int_nullable_int()
        {
            int intPrm = 2;

            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo.Equals(intPrm)),
                entryCount: 5);

            AssertQuery<Employee>(
                es => es.Where(e => intPrm.Equals(e.ReportsTo)),
                entryCount: 5);

        }

        [Fact]
        public virtual void Where_equals_on_mismatched_types_nullable_long_nullable_int()
        {
            long? nullableLongPrm = 2;

            AssertQuery<Employee>(
                es => es.Where(e => nullableLongPrm.Equals(e.ReportsTo)));

            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo.Equals(nullableLongPrm)));
        }

        [Fact]
        public virtual void Where_equals_on_matched_nullable_int_types()
        {
            int? nullableIntPrm = 2;

            AssertQuery<Employee>(
                es => es.Where(e => nullableIntPrm.Equals(e.ReportsTo)),
                entryCount: 5);

            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo.Equals(nullableIntPrm)),
                entryCount: 5);
        }

        [Fact]
        public virtual void Where_comparison_nullable_type_not_null()
        {
            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == 2),
                entryCount: 5);
        }

        [Fact]
        public virtual void Where_comparison_nullable_type_null()
        {
            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == null),
                entryCount: 1);
        }

        [Fact]
        public virtual void Where_string_length()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City.Length == 6),
                entryCount: 20);
        }

        [Fact]
        public virtual void Where_datetime_now()
        {
            var myDatetime = new DateTime(2015, 4, 10);
            AssertQuery<Customer>(
                cs => cs.Where(c => DateTime.Now != myDatetime),
                entryCount: 91);
        }

        [Fact]
        public virtual void Where_simple_reversed()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => "London" == c.City),
                entryCount: 6);
        }

        [Fact]
        public virtual void Where_is_null()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == null));
        }

        [Fact]
        public virtual void Where_null_is_null()
        {
            // ReSharper disable once EqualExpressionComparison
            AssertQuery<Customer>(
                cs => cs.Where(c => null == null),
                entryCount: 91);
        }

        [Fact]
        public virtual void Where_constant_is_null()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => "foo" == null));
        }

        [Fact]
        public virtual void Where_is_not_null()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City != null),
                entryCount: 91);
        }

        [Fact]
        public virtual void Where_null_is_not_null()
        {
            // ReSharper disable once EqualExpressionComparison
            AssertQuery<Customer>(
                cs => cs.Where(c => null != null));
        }

        [Fact]
        public virtual void Where_constant_is_not_null()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => "foo" != null),
                entryCount: 91);
        }

        [Fact]
        public virtual void Where_identity_comparison()
        {
            // ReSharper disable once EqualExpressionComparison
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == c.City),
                entryCount: 91);
        }

        [Fact]
        public virtual void Where_select_many_or()
        {
            AssertQuery<Customer, Employee>((cs, es) =>
                from c in cs
                from e in es
                where c.City == "London"
                      || e.City == "London"
                select new { c, e });
        }

        [Fact]
        public virtual void Where_select_many_or2()
        {
            AssertQuery<Customer, Employee>((cs, es) =>
                from c in cs
                from e in es
                where c.City == "London"
                      || c.City == "Berlin"
                select new { c, e });
        }

        [Fact]
        public virtual void Where_select_many_or3()
        {
            AssertQuery<Customer, Employee>((cs, es) =>
                from c in cs
                from e in es
                where c.City == "London"
                      || c.City == "Berlin"
                      || c.City == "Seattle"
                select new { c, e });
        }

        [Fact]
        public virtual void Where_select_many_or4()
        {
            AssertQuery<Customer, Employee>((cs, es) =>
                from c in cs
                from e in es
                where c.City == "London"
                      || c.City == "Berlin"
                      || c.City == "Seattle"
                      || c.City == "Lisboa"
                select new { c, e });
        }

        [Fact]
        public virtual void Where_select_many_or_with_parameter()
        {
            var london = "London";
            var lisboa = "Lisboa";

            AssertQuery<Customer, Employee>((cs, es) =>
                from c in cs
                from e in es
                where c.City == london
                      || c.City == "Berlin"
                      || c.City == "Seattle"
                      || c.City == lisboa
                select new { c, e });
        }

        [Fact]
        public virtual void Where_in_optimization_multiple()
        {
            AssertQuery<Customer, Employee>((cs, es) =>
                from c in cs
                from e in es
                where c.City == "London"
                      || c.City == "Berlin"
                      || c.CustomerID == "ALFKI"
                      || c.CustomerID == "ABCDE"
                select new { c, e });
        }

        [Fact]
        public virtual void Where_not_in_optimization1()
        {
            AssertQuery<Customer, Employee>((cs, es) =>
                from c in cs
                from e in es
                where c.City != "London"
                      && e.City != "London"
                select new { c, e });
        }

        [Fact]
        public virtual void Where_not_in_optimization2()
        {
            AssertQuery<Customer, Employee>((cs, es) =>
                from c in cs
                from e in es
                where c.City != "London"
                      && c.City != "Berlin"
                select new { c, e });
        }

        [Fact]
        public virtual void Where_not_in_optimization3()
        {
            AssertQuery<Customer, Employee>((cs, es) =>
                from c in cs
                from e in es
                where c.City != "London"
                      && c.City != "Berlin"
                      && c.City != "Seattle"
                select new { c, e });
        }

        [Fact]
        public virtual void Where_not_in_optimization4()
        {
            AssertQuery<Customer, Employee>((cs, es) =>
                from c in cs
                from e in es
                where c.City != "London"
                      && c.City != "Berlin"
                      && c.City != "Seattle"
                      && c.City != "Lisboa"
                select new { c, e });
        }

        [Fact]
        public virtual void Where_select_many_and()
        {
            AssertQuery<Customer, Employee>((cs, es) =>
                from c in cs
                from e in es
                where (c.City == "London" && c.Country == "UK")
                      && (e.City == "London" && e.Country == "UK")
                select new { c, e });
        }

        [Fact]
        public virtual void Where_primitive()
        {
            AssertQuery<Employee>(
                es => es.Select(e => e.EmployeeID).Take(9).Where(i => i == 5));
        }

        [Fact]
        public virtual void Where_primitive_tracked()
        {
            AssertQuery<Employee>(
                es => es.Take(9).Where(e => e.EmployeeID == 5),
                entryCount: 1);
        }

        [Fact]
        public virtual void Where_primitive_tracked2()
        {
            AssertQuery<Employee>(
                es => es.Take(9).Select(e => new { e }).Where(e => e.e.EmployeeID == 5),
                entryCount: 1);
        }

        [Fact]
        public virtual void Where_subquery_anon()
        {
            AssertQuery<Employee, Order>((es, os) =>
                from e in es.Take(9).Select(e => new { e })
                from o in os.Take(1000).Select(o => new { o })
                where e.e.EmployeeID == o.o.EmployeeID
                select new { e, o });
        }

        [Fact]
        public virtual void Where_bool_member()
        {
            AssertQuery<Product>(ps => ps.Where(p => p.Discontinued), entryCount: 8);
        }

        [Fact]
        public virtual void Where_bool_member_false()
        {
            AssertQuery<Product>(ps => ps.Where(p => !p.Discontinued), entryCount: 69);
        }

        [Fact]
        public virtual void Where_bool_member_negated_twice()
        {
            // ReSharper disable once NegativeEqualityExpression
            // ReSharper disable once DoubleNegationOperator
            // ReSharper disable once RedundantBoolCompare
            AssertQuery<Product>(ps => ps.Where(p => !!(p.Discontinued == true)), entryCount: 8);
        }

        [Fact]
        public virtual void Where_bool_member_shadow()
        {
            AssertQuery<Product>(ps => ps.Where(p => EF.Property<bool>(p, "Discontinued")), entryCount: 8);
        }

        [Fact]
        public virtual void Where_bool_member_false_shadow()
        {
            AssertQuery<Product>(ps => ps.Where(p => !EF.Property<bool>(p, "Discontinued")), entryCount: 69);
        }

        [Fact]
        public virtual void Where_bool_member_equals_constant()
        {
            AssertQuery<Product>(ps => ps.Where(p => p.Discontinued.Equals(true)), entryCount: 8);
        }

        [Fact]
        public virtual void Where_bool_member_in_complex_predicate()
        {
            // ReSharper disable once RedundantBoolCompare
            AssertQuery<Product>(ps => ps.Where(p => p.ProductID > 100 && p.Discontinued || (p.Discontinued == true)), entryCount: 8);
        }

        [Fact]
        public virtual void Where_bool_member_compared_to_binary_expression()
        {
            AssertQuery<Product>(ps => ps.Where(p => p.Discontinued == (p.ProductID > 50)), entryCount: 44);
        }

        [Fact]
        public virtual void Where_not_bool_member_compared_to_not_bool_member()
        {
            AssertQuery<Product>(ps => ps.Where(p => !p.Discontinued == !p.Discontinued), entryCount: 77);
        }

        [Fact]
        public virtual void Where_negated_boolean_expression_compared_to_another_negated_boolean_expression()
        {
            AssertQuery<Product>(ps => ps.Where(p => !(p.ProductID > 50) == !(p.ProductID > 20)), entryCount: 47);
        }

        [Fact]
        public virtual void Where_not_bool_member_compared_to_binary_expression()
        {
            AssertQuery<Product>(ps => ps.Where(p => !p.Discontinued == (p.ProductID > 50)), entryCount: 33);
        }

        [Fact]
        public virtual void Where_bool_parameter_compared_to_binary_expression()
        {
            var prm = true;
            AssertQuery<Product>(ps => ps.Where(p => (p.ProductID > 50) != prm), entryCount: 50);
        }

        [Fact]
        public virtual void Where_bool_member_and_parameter_compared_to_binary_expression_nested()
        {
            var prm = true;
            AssertQuery<Product>(ps => ps.Where(p => p.Discontinued == ((p.ProductID > 50) != prm)), entryCount: 33);
        }

        [Fact]
        public virtual void Where_de_morgan_or_optimizated()
        {
            AssertQuery<Product>(ps => ps.Where(p => !(p.Discontinued || (p.ProductID < 20))), entryCount: 53);
        }

        [Fact]
        public virtual void Where_de_morgan_and_optimizated()
        {
            AssertQuery<Product>(ps => ps.Where(p => !(p.Discontinued && (p.ProductID < 20))), entryCount: 74);
        }

        [Fact]
        public virtual void Where_complex_negated_expression_optimized()
        {
            AssertQuery<Product>(ps => ps.Where(p => !(!(!p.Discontinued && (p.ProductID < 60)) || !(p.ProductID > 30))), entryCount: 27);
        }

        [Fact]
        public virtual void Where_short_member_comparison()
        {
            AssertQuery<Product>(ps => ps.Where(p => p.UnitsInStock > 10), entryCount: 63);
        }

        [Fact]
        public virtual void Where_true()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => true),
                entryCount: 91);
        }

        [Fact]
        public virtual void Where_false()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => false));
        }

        [Fact]
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

        [Fact]
        public virtual void Select_bool_closure()
        {
            var boolean = false;

            AssertQuery<Customer>(
                cs => cs.Select(c => new { f = boolean }));

            boolean = true;

            AssertQuery<Customer>(
                cs => cs.Select(c => new { f = boolean }));
        }

        // TODO: Re-write entity ref equality to identity equality.
        //
        // [Fact]
        // public virtual void Where_compare_entity_equal()
        // {
        //     var alfki = NorthwindData.Customers.Single(c => c.CustomerID == "ALFKI");
        //
        //     Assert.Equal(1,
        //         // ReSharper disable once PossibleUnintendedReferenceComparison
        //         AssertQuery<Customer>(cs => cs.Where(c => c == alfki)));
        // }
        //
        // [Fact]
        // public virtual void Where_compare_entity_not_equal()
        // {
        //     var alfki = new Customer { CustomerID = "ALFKI" };
        //
        //     Assert.Equal(90,
        //         // ReSharper disable once PossibleUnintendedReferenceComparison
        //         AssertQuery<Customer>(cs => cs.Where(c => c != alfki)));
        //
        // [Fact]
        // public virtual void Project_compare_entity_equal()
        // {
        //     var alfki = NorthwindData.Customers.Single(c => c.CustomerID == "ALFKI");
        //
        //     Assert.Equal(1,
        //         // ReSharper disable once PossibleUnintendedReferenceComparison
        //         AssertQuery<Customer>(cs => cs.Select(c => c == alfki)));
        // }
        //
        // [Fact]
        // public virtual void Project_compare_entity_not_equal()
        // {
        //     var alfki = new Customer { CustomerID = "ALFKI" };
        //
        //     Assert.Equal(90,
        //         // ReSharper disable once PossibleUnintendedReferenceComparison
        //         AssertQuery<Customer>(cs => cs.Select(c => c != alfki)));
        // }

        [Fact]
        public virtual void Where_compare_constructed_equal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => new { x = c.City } == new { x = "London" }));
        }

        [Fact]
        public virtual void Where_compare_constructed_multi_value_equal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => new { x = c.City, y = c.Country } == new { x = "London", y = "UK" }));
        }

        [Fact]
        public virtual void Where_compare_constructed_multi_value_not_equal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => new { x = c.City, y = c.Country } != new { x = "London", y = "UK" }),
                entryCount: 91);
        }

        [Fact]
        public virtual void Where_compare_constructed()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => new { x = c.City } == new { x = "London" }));
        }

        [Fact]
        public virtual void Where_compare_null()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == null && c.Country == "UK"));
        }

        [Fact]
        public virtual void Where_projection()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == "London").Select(c => c.CompanyName));
        }

        [Fact]
        public virtual void Select_scalar()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => c.City));
        }

        [Fact]
        public virtual void Select_anonymous_one()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.City }));
        }

        [Fact]
        public virtual void Select_anonymous_two()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.City, c.Phone }));
        }

        [Fact]
        public virtual void Select_anonymous_three()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.City, c.Phone, c.Country }));
        }

        [Fact]
        public virtual void Select_anonymous_bool_constant_true()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.CustomerID, ConstantTrue = true }));
        }

        [Fact]
        public virtual void Select_anonymous_bool_constant_in_expression()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.CustomerID, Expression = c.CustomerID.Length + 5 }));
        }

        [Fact]
        public virtual void Select_customer_table()
        {
            AssertQuery<Customer>(
                cs => cs,
                entryCount: 91);
        }

        [Fact]
        public virtual void Select_customer_identity()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => c),
                entryCount: 91);
        }

        [Fact]
        public virtual void Select_anonymous_with_object()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.City, c }),
                entryCount: 91);
        }

        [Fact]
        public virtual void Select_anonymous_nested()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.City, Country = new { c.Country } }));
        }

        [Fact]
        public virtual void Select_anonymous_empty()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { }));
        }

        [Fact]
        public virtual void Select_anonymous_literal()
        {
            AssertQuery<Customer>(cs => cs.Select(c => new { X = 10 }));
        }

        [Fact]
        public virtual void Select_constant_int()
        {
            AssertQuery<Customer>(cs => cs.Select(c => 0));
        }

        [Fact]
        public virtual void Select_constant_null_string()
        {
            AssertQuery<Customer>(cs => cs.Select(c => (string)null));
        }

        [Fact]
        public virtual void Select_local()
        {
            // ReSharper disable once ConvertToConstant.Local
            var x = 10;

            AssertQuery<Customer>(cs => cs.Select(c => x));
        }

        [Fact]
        public virtual void Select_scalar_primitive()
        {
            AssertQuery<Employee>(
                es => es.Select(e => e.EmployeeID));
        }

        [Fact]
        public virtual void Select_scalar_primitive_after_take()
        {
            AssertQuery<Employee>(
                es => es.Take(9).Select(e => e.EmployeeID));
        }

        [Fact]
        public virtual void Select_project_filter()
        {
            AssertQuery<Customer>(
                cs =>
                    from c in cs
                    where c.City == "London"
                    select c.CompanyName);
        }

        [Fact]
        public virtual void Select_project_filter2()
        {
            AssertQuery<Customer>(
                cs =>
                    from c in cs
                    where c.City == "London"
                    select c.City);
        }

        [Fact]
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
                                .SelectMany(q1 => ((IEnumerable<int>)q1));

                        var efObjects
                            = efResults
                                .SelectMany(q1 => ((IEnumerable<int>)q1));

                        Assert.Equal(l2oObjects, efObjects);
                    });
        }

        [Fact]
        public virtual void Select_correlated_subquery_projection()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                select os
                    .Where(o => o.CustomerID == c.CustomerID),
                asserter:
                    (l2oResults, efResults) =>
                    {
                        var l2oObjects
                            = l2oResults
                                .SelectMany(q1 => ((IEnumerable<Order>)q1))
                                .OrderBy(o => o.OrderID);

                        var efObjects
                            = efResults
                                .SelectMany(q1 => ((IEnumerable<Order>)q1))
                                .OrderBy(o => o.OrderID);

                        Assert.Equal(l2oObjects, efObjects);
                    });
        }

        [Fact]
        public virtual void Select_correlated_subquery_ordered()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                select os.Where(o => o.CustomerID == c.CustomerID),
                asserter:
                    (l2oResults, efResults) =>
                    {
                        var l2oObjects
                            = l2oResults
                                .SelectMany(q1 => ((IEnumerable<Order>)q1))
                                .OrderBy(o => o.OrderID);

                        var efObjects
                            = efResults
                                .SelectMany(q1 => ((IEnumerable<Order>)q1))
                                .OrderBy(o => o.OrderID);

                        Assert.Equal(l2oObjects, efObjects);
                    });
        }

        // TODO: Re-linq parser
        // [Fact]
        // public virtual void Select_nested_ordered_enumerable_collection()
        // {
        //     AssertQuery<Customer>(cs =>
        //         cs.Select(c => cs.AsEnumerable().OrderBy(c2 => c2.CustomerID)),
        //         assertOrder: true);
        // }

        [Fact]
        public virtual void Select_nested_collection_in_anonymous_type()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                where c.CustomerID == "ALFKI"
                select new
                {
                    CustomerId = c.CustomerID,
                    OrderIds
                            = os.Where(o => o.CustomerID == c.CustomerID
                                            && o.OrderDate.Value.Year == 1997)
                                .Select(o => o.OrderID)
                                .OrderBy(o => o),
                    Customer = c
                },
                asserter:
                    (l2oResults, efResults) =>
                    {
                        dynamic l2oResult = l2oResults.Single();
                        dynamic efResult = efResults.Single();

                        Assert.Equal(l2oResult.CustomerId, efResult.CustomerId);
                        Assert.Equal((IEnumerable<int>)l2oResult.OrderIds, (IEnumerable<int>)efResult.OrderIds);
                        Assert.Equal(l2oResult.Customer, efResult.Customer);
                    });
        }

        [Fact]
        public virtual void Select_subquery_recursive_trivial()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es
                    select (from e2 in es
                            select (from e3 in es
                                    orderby e3.EmployeeID
                                    select e3)),
                asserter:
                    (l2oResults, efResults) =>
                    {
                        var l2oObjects
                            = l2oResults
                                .SelectMany(q1 => ((IEnumerable<object>)q1)
                                    .SelectMany(q2 => (IEnumerable<object>)q2));

                        var efObjects
                            = efResults
                                .SelectMany(q1 => ((IEnumerable<object>)q1)
                                    .SelectMany(q2 => (IEnumerable<object>)q2));

                        Assert.Equal(l2oObjects, efObjects);
                    });
        }

        // TODO: [Fact] See #153
        public virtual void Where_subquery_on_collection()
        {
            AssertQuery<Product, OrderDetail>((pr, od) =>
                from p in pr
                where p.OrderDetails.Contains(od.FirstOrDefault(orderDetail => orderDetail.Discount == 0.1))
                select p);
        }

        [Fact]
        public virtual void Where_query_composition()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es
                    where e1.FirstName == es.OrderBy(e => e.EmployeeID).First().FirstName
                    select e1,
                entryCount: 1);
        }

        [Fact]
        public virtual void Where_query_composition_is_null()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es
                    where es.SingleOrDefault(e2 => e2.EmployeeID == e1.ReportsTo) == null
                    select e1,
                entryCount: 1);
        }

        [Fact]
        public virtual void Where_query_composition_is_not_null()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es
                    where es.SingleOrDefault(e2 => e2.EmployeeID == e1.ReportsTo) != null
                    select e1,
                entryCount: 8);
        }

        [Fact]
        public virtual void Where_query_composition_entity_equality()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es
                    where es.SingleOrDefault(e2 => e2.EmployeeID == e1.ReportsTo) == new Employee()
                    select e1);
        }

        [Fact]
        public virtual void Where_query_composition2()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es
                    where e1.FirstName ==
                        (from e2 in es.OrderBy(e => e.EmployeeID)
                         select new { Foo = e2 })
                            .First().Foo.FirstName
                    select e1,
                entryCount: 1);
        }

        [Fact]
        public virtual void Where_query_composition3()
        {
            AssertQuery<Customer>(
                cs =>
                    from c1 in cs
                    where c1.City == cs.OrderBy(c => c.CustomerID).First(c => c.IsLondon).City
                    select c1,
                entryCount: 6);
        }

        [Fact]
        public virtual void Where_query_composition4()
        {
            AssertQuery<Customer>(
                cs =>
                    from c1 in cs
                    where c1.City == (from c2 in cs.OrderBy(c => c.CustomerID)
                                      from c3 in cs.OrderBy(c => c.IsLondon).ThenBy(c => c.CustomerID)
                                      select new { c3 }).First().c3.City
                    select c1,
                entryCount: 1);
        }

        [Fact]
        public virtual void Where_query_composition5()
        {
            AssertQuery<Customer>(
                cs =>
                    from c1 in cs
                    where c1.IsLondon == cs.OrderBy(c => c.CustomerID).First().IsLondon
                    select c1,
                entryCount: 85);
        }

        [Fact]
        public virtual void Where_query_composition6()
        {
            AssertQuery<Customer>(
                cs =>
                    from c1 in cs
                    where c1.IsLondon ==
                        cs.OrderBy(c => c.CustomerID)
                          .Select(c => new { Foo = c })
                          .First().Foo.IsLondon
                    select c1,
                entryCount: 85);
        }

        [Fact]
        public virtual void Where_subquery_recursive_trivial()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es
                    where (from e2 in es
                           where (from e3 in es
                                  orderby e3.EmployeeID
                                  select e3).Any()
                           select e2).Any()
                    orderby e1.EmployeeID
                    select e1,
                assertOrder: true,
                entryCount: 9);
        }

        [Fact]
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

        [Fact]
        public virtual void OrderBy_scalar_primitive()
        {
            AssertQuery<Employee>(
                es =>
                    es.Select(e => e.EmployeeID).OrderBy(i => i),
                assertOrder: true);
        }

        [Fact]
        public virtual void SelectMany_mixed()
        {
            AssertQuery<Employee, Customer>(
                (es, cs) => from e1 in es
                            from s in new[] { "a", "b" }
                            from c in cs
                            select new { e1, s, c });
        }

        [Fact]
        public virtual void SelectMany_simple1()
        {
            AssertQuery<Employee, Customer>(
                (es, cs) => from e in es
                            from c in cs
                            select new { c, e });
        }

        [Fact]
        public virtual void SelectMany_simple_subquery()
        {
            AssertQuery<Employee, Customer>(
                (es, cs) => from e in es.Take(9)
                            from c in cs
                            select new { c, e });
        }

        [Fact]
        public virtual void SelectMany_simple2()
        {
            AssertQuery<Employee, Customer>(
                (es, cs) => from e1 in es
                            from c in cs
                            from e2 in es
                            select new { e1, c, e2.FirstName });
        }

        [Fact]
        public virtual void SelectMany_entity_deep()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es
                    from e2 in es
                    from e3 in es
                    select new { e2, e3, e1 },
                entryCount: 9);
        }

        [Fact]
        public virtual void SelectMany_projection1()
        {
            AssertQuery<Employee>(
                es => from e1 in es
                      from e2 in es
                      select new { e1.City, e2.Country });
        }

        [Fact]
        public virtual void SelectMany_projection2()
        {
            AssertQuery<Employee>(
                es => from e1 in es
                      from e2 in es
                      from e3 in es
                      select new { e1.City, e2.Country, e3.FirstName });
        }

        [Fact]
        public virtual void SelectMany_nested_simple()
        {
            AssertQuery<Customer>(
                cs =>
                    from c in cs
                    from c1 in
                        (from c2 in (from c3 in cs select c3) select c2)
                    orderby c1.CustomerID
                    select c1,
                assertOrder: true,
                entryCount: 91);
        }

        [Fact]
        public virtual void SelectMany_correlated_simple()
        {
            AssertQuery<Customer, Employee>((cs, es) =>
                from c in cs
                from e in es
                where c.City == e.City
                orderby c.CustomerID, e.EmployeeID
                select new { c, e },
                assertOrder: true);
        }

        [Fact]
        public virtual void SelectMany_correlated_subquery_simple()
        {
            AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es.Where(e => e.City == c.City)
                    orderby c.CustomerID, e.EmployeeID
                    select new { c, e },
                assertOrder: true);
        }

        [Fact]
        public virtual void SelectMany_correlated_subquery_hard()
        {
            AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c1 in
                        (from c2 in cs.Take(91) select c2.City).Distinct()
                    from e1 in
                        (from e2 in es where c1 == e2.City select new { e2.City, c1 }).Take(9)
                    from e2 in
                        (from e3 in es where e1.City == e3.City select c1).Take(9)
                    select new { c1, e1 });
        }

        [Fact]
        public virtual void SelectMany_cartesian_product_with_ordering()
        {
            AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == e.City
                    orderby e.City ascending, c.CustomerID descending
                    select new { c, e.City },
                assertOrder: true);
        }

        [Fact]
        public virtual void SelectMany_primitive()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es
                    from i in es.Select(e2 => e2.EmployeeID)
                    select i);
        }

        [Fact]
        public virtual void SelectMany_primitive_select_subquery()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es
                    from i in es.Select(e2 => e2.EmployeeID)
                    select es.Any());
        }

        [Fact]
        public virtual void Join_customers_orders_projection()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID
                select new { c.ContactName, o.OrderID });
        }

        [Fact]
        public virtual void Join_customers_orders_entities()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID
                select new { c, o });
        }

        [Fact]
        public virtual void Join_select_many()
        {
            AssertQuery<Customer, Order, Employee>((cs, os, es) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID
                from e in es
                select new { c, o, e });
        }

        [Fact]
        public virtual void Join_customers_orders_select()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID
                select new { c.ContactName, o.OrderID }
                into p
                select p);
        }

        [Fact]
        public virtual void Join_customers_orders_with_subquery()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o1 in
                    (from o2 in os orderby o2.OrderID select o2) on c.CustomerID equals o1.CustomerID
                where o1.CustomerID == "ALFKI"
                select new { c.ContactName, o1.OrderID });
        }

        [Fact]
        public virtual void Join_customers_orders_with_subquery_with_take()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o1 in
                    (from o2 in os orderby o2.OrderID select o2).Take(5) on c.CustomerID equals o1.CustomerID
                where o1.CustomerID == "ALFKI"
                select new { c.ContactName, o1.OrderID });
        }

        [Fact]
        public virtual void Join_customers_orders_with_subquery_anonymous_property_method()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o1 in
                    (from o2 in os orderby o2.OrderID select new { o2 }) on c.CustomerID equals o1.o2.CustomerID
                where EF.Property<string>(o1.o2, "CustomerID") == "ALFKI"
                select new { o1, o1.o2, Shadow = EF.Property<DateTime?>(o1.o2, "OrderDate") });
        }

        [Fact]
        public virtual void Join_customers_orders_with_subquery_anonymous_property_method_with_take()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o1 in
                    (from o2 in os orderby o2.OrderID select new { o2 }).Take(5) on c.CustomerID equals o1.o2.CustomerID
                where EF.Property<string>(o1.o2, "CustomerID") == "ALFKI"
                select new { o1, o1.o2, Shadow = EF.Property<DateTime?>(o1.o2, "OrderDate") });
        }

        [Fact]
        public virtual void Join_customers_orders_with_subquery_predicate()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o1 in
                    (from o2 in os where o2.OrderID > 0 orderby o2.OrderID select o2) on c.CustomerID equals o1.CustomerID
                where o1.CustomerID == "ALFKI"
                select new { c.ContactName, o1.OrderID });
        }

        [Fact]
        public virtual void Join_customers_orders_with_subquery_predicate_with_take()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o1 in
                    (from o2 in os where o2.OrderID > 0 orderby o2.OrderID select o2).Take(5) on c.CustomerID equals o1.CustomerID
                where o1.CustomerID == "ALFKI"
                select new { c.ContactName, o1.OrderID });
        }

        [Fact]
        public virtual void Join_composite_key()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on new { a = c.CustomerID, b = c.CustomerID }
                    equals new { a = o.CustomerID, b = o.CustomerID }
                select new { c, o });
        }

        [Fact]
        public virtual void Join_client_new_expression()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on new Foo { Bar = c.CustomerID } equals new Foo { Bar = o.CustomerID }
                select new { c, o });
        }

        [Fact]
        public virtual void Join_Where_Count()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                (from c in cs
                 join o in os on c.CustomerID equals o.CustomerID
                 where c.CustomerID == "ALFKI"
                 select c).Count());
        }

        [Fact]
        public virtual void Multiple_joins_Where_Order_Any()
        {
            AssertQuery<Customer, Order, OrderDetail>((cs, os, ods) =>
                cs.Join(os, c => c.CustomerID, o => o.CustomerID, (cr, or) => new { cr, or })
                    .Join(ods, e => e.or.OrderID, od => od.OrderID, (e, od) => new { e.cr, e.or, od })
                    .Where(r => r.cr.City == "London").OrderBy(r => r.cr.CustomerID)
                    .Any());
        }

        [Fact]
        public virtual void Join_OrderBy_Count()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                (from c in cs
                 join o in os on c.CustomerID equals o.CustomerID
                 orderby c.CustomerID
                 select c).Count());
        }

        [Fact]
        public virtual void Where_join_select()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                (from c in cs
                 where c.CustomerID == "ALFKI"
                 join o in os on c.CustomerID equals o.CustomerID
                 select c));
        }

        [Fact]
        public virtual void Where_orderby_join_select()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                (from c in cs
                 where c.CustomerID != "ALFKI"
                 orderby c.CustomerID
                 join o in os on c.CustomerID equals o.CustomerID
                 select c));
        }

        [Fact]
        public virtual void Where_join_orderby_join_select()
        {
            AssertQuery<Customer, Order, OrderDetail>((cs, os, ods) =>
                (from c in cs
                 where c.CustomerID != "ALFKI"
                 join o in os on c.CustomerID equals o.CustomerID
                 orderby c.CustomerID
                 join od in ods on o.OrderID equals od.OrderID
                 select c));
        }

        [Fact]
        public virtual void Where_select_many()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                (from c in cs
                 where c.CustomerID == "ALFKI"
                 from o in os
                 select c));
        }

        [Fact]
        public virtual void Where_orderby_select_many()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                (from c in cs
                 where c.CustomerID == "ALFKI"
                 orderby c.CustomerID
                 from o in os
                 select c));
        }

        private class Foo
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string Bar { get; set; }
        }

        [Fact]
        public virtual void GroupJoin_customers_orders()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os.OrderBy(o => o.OrderID) on c.CustomerID equals o.CustomerID into orders
                select new { customer = c, orders = orders.ToList() },
                asserter: (l2oItems, efItems) =>
                {
                    foreach (var pair in
                        from dynamic l2oItem in l2oItems
                        join dynamic efItem in efItems on l2oItem.customer equals efItem.customer
                        select new { l2oItem, efItem })
                    {
                        Assert.Equal(pair.l2oItem.orders, pair.efItem.orders);
                    }
                });
        }

        [Fact]
        public virtual void GroupJoin_customers_orders_count()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID into orders
                select new { cust = c, ords = orders.Count() });
        }

        [Fact]
        public virtual void Default_if_empty_top_level()
        {
            AssertQuery<Employee>(cs =>
                from c in cs.Where(c => c.EmployeeID == -1).DefaultIfEmpty()
                select c);
        }

        [Fact]
        public virtual void Default_if_empty_top_level_arg()
        {
            AssertQuery<Employee>(cs =>
                from c in cs.Where(c => c.EmployeeID == -1).DefaultIfEmpty(new Employee())
                select c);
        }

        [Fact]
        public virtual void GroupJoin_customers_employees_shadow()
        {
            AssertQuery<Customer, Employee>((cs, es) =>
                (from c in cs
                 join e in es on c.City equals e.City into employees
                 select employees)
                    .SelectMany(emps => emps)
                    .Select(e =>
                        new
                        {
                            Title = EF.Property<string>(e, "Title"),
                            Id = e.EmployeeID
                        }));
        }

        [Fact]
        public virtual void GroupJoin_customers_employees_subquery_shadow()
        {
            AssertQuery<Customer, Employee>((cs, es) =>
                (from c in cs
                 join e in es.OrderBy(e => e.City) on c.City equals e.City into employees
                 select employees)
                    .SelectMany(emps => emps)
                    .Select(e =>
                        new
                        {
                            Title = EF.Property<string>(e, "Title"),
                            Id = e.EmployeeID
                        }));
        }

        [Fact]
        public virtual void GroupJoin_customers_employees_subquery_shadow_take()
        {
            AssertQuery<Customer, Employee>((cs, es) =>
                (from c in cs
                 join e in es.OrderBy(e => e.City).Take(5) on c.City equals e.City into employees
                 select employees)
                    .SelectMany(emps => emps)
                    .Select(e =>
                        new
                        {
                            Title = EF.Property<string>(e, "Title"),
                            Id = e.EmployeeID
                        }));
        }

        [Fact]
        public virtual void SelectMany_customer_orders()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                from o in os
                where c.CustomerID == o.CustomerID
                select new { c.ContactName, o.OrderID });
        }

        [Fact]
        public virtual void SelectMany_Count()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                (from c in cs
                 from o in os
                 select c.CustomerID).Count());
        }

        [Fact]
        public virtual void SelectMany_LongCount()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                (from c in cs
                 from o in os
                 select c.CustomerID).LongCount());
        }

        [Fact]
        public virtual void SelectMany_OrderBy_ThenBy_Any()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                (from c in cs
                 from o in os
                 orderby c.CustomerID, c.City
                 select c).Any());
        }

        // TODO: Composite keys, slow..

        //        [Fact]
        //        public virtual void Multiple_joins_with_join_conditions_in_where()
        //        {
        //            AssertQuery<Customer, Order, OrderDetail>((cs, os, ods) =>
        //                from c in cs
        //                from o in os.OrderBy(o1 => o1.OrderID).Take(10)
        //                from od in ods
        //                where o.CustomerID == c.CustomerID
        //                    && o.OrderID == od.OrderID
        //                where c.CustomerID == "ALFKI"
        //                select od.ProductID,
        //                assertOrder: true);
        //        }
        //        [Fact]
        //
        //        public virtual void TestMultipleJoinsWithMissingJoinCondition()
        //        {
        //            AssertQuery<Customer, Order, OrderDetail>((cs, os, ods) =>
        //                from c in cs
        //                from o in os
        //                from od in ods
        //                where o.CustomerID == c.CustomerID
        //                where c.CustomerID == "ALFKI"
        //                select od.ProductID
        //                );
        //        }

        [Fact]
        public virtual void OrderBy()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID),
                assertOrder: true,
                entryCount: 91);
        }

        [Fact]
        public virtual void OrderBy_anon()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.CustomerID }).OrderBy(a => a.CustomerID),
                assertOrder: true);
        }

        [Fact]
        public virtual void OrderBy_anon2()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c }).OrderBy(a => a.c.CustomerID),
                assertOrder: true,
                entryCount: 91);
        }

        [Fact]
        public virtual void OrderBy_client_mixed()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.IsLondon).ThenBy(c => c.CompanyName),
                assertOrder: true,
                entryCount: 91);
        }

        [Fact]
        public virtual void OrderBy_multiple_queries()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on new Foo { Bar = c.CustomerID } equals new Foo { Bar = o.CustomerID }
                orderby c.IsLondon, o.OrderDate
                select new { c, o });
        }

        [Fact]
        public virtual void OrderBy_shadow()
        {
            AssertQuery<Employee>(
                es => es.OrderBy(e => EF.Property<string>(e, "Title")).ThenBy(e => e.EmployeeID),
                assertOrder: true,
                entryCount: 9);
        }

        [Fact]
        public virtual void OrderBy_ThenBy_predicate()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == "London")
                    .OrderBy(c => c.City)
                    .ThenBy(c => c.CustomerID),
                assertOrder: true,
                entryCount: 6);
        }

        [Fact]
        public virtual void OrderBy_correlated_subquery_lol()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      orderby cs.Any(c2 => c2.CustomerID == c.CustomerID)
                      select c,
                entryCount: 91);
        }

        [Fact]
        public virtual void OrderBy_Select()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.OrderBy(c => c.CustomerID)
                        .Select(c => c.ContactName),
                assertOrder: true);
        }

        [Fact]
        public virtual void OrderBy_multiple()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.OrderBy(c => c.CustomerID)
                        // ReSharper disable once MultipleOrderBy
                        .OrderBy(c => c.Country)
                        .Select(c => c.City),
                assertOrder: true);
        }

        [Fact]
        public virtual void OrderBy_ThenBy()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.OrderBy(c => c.CustomerID).ThenBy(c => c.Country).Select(c => c.City),
                assertOrder: true);
        }

        [Fact]
        public virtual void OrderByDescending()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.OrderByDescending(c => c.CustomerID).Select(c => c.City),
                assertOrder: true);
        }

        [Fact]
        public virtual void OrderByDescending_ThenBy()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.OrderByDescending(c => c.CustomerID).ThenBy(c => c.Country).Select(c => c.City),
                assertOrder: true);
        }

        [Fact]
        public virtual void OrderByDescending_ThenByDescending()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.OrderByDescending(c => c.CustomerID).ThenByDescending(c => c.Country).Select(c => c.City),
                assertOrder: true);
        }

        [Fact]
        public virtual void OrderBy_ThenBy_Any()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).ThenBy(c => c.ContactName).Any());
        }

        [Fact]
        public virtual void OrderBy_Join()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs.OrderBy(c => c.CustomerID)
                join o in os.OrderBy(o => o.OrderID) on c.CustomerID equals o.CustomerID
                select new { c.CustomerID, o.OrderID });
        }

        [Fact]
        public virtual void OrderBy_SelectMany()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs.OrderBy(c => c.CustomerID)
                from o in os.OrderBy(o => o.OrderID)
                where c.CustomerID == o.CustomerID
                select new { c.ContactName, o.OrderID },
                assertOrder: true);
        }

        // TODO: Need to figure out how to do this 
        //        [Fact]
        //        public virtual void GroupBy_anonymous()
        //        {
        //            AssertQuery<Customer>(cs =>
        //                cs.Select(c => new { c.City, c.CustomerID })
        //                    .GroupBy(a => a.City),
        //                assertOrder: true);
        //        }
        //
        //        [Fact]
        //        public virtual void GroupBy_anonymous_subquery()
        //        {
        //            AssertQuery<Customer>(cs =>
        //                cs.Select(c => new { c.City, c.CustomerID })
        //                    .GroupBy(a => from c2 in cs select c2),
        //                assertOrder: true);
        //        }
        //
        //        [Fact]
        //        public virtual void GroupBy_nested_order_by_enumerable()
        //        {
        //            AssertQuery<Customer>(cs =>
        //                cs.Select(c => new { c.City, c.CustomerID })
        //                    .OrderBy(a => a.City)
        //                    .GroupBy(a => a.City)
        //                    .Select(g => g.OrderBy(a => a.CustomerID)),
        //                assertOrder: true);
        //        }

        [Fact]
        public virtual void GroupBy_SelectMany()
        {
            AssertQuery<Customer>(
                cs => cs.GroupBy(c => c.City).SelectMany(g => g),
                entryCount: 91);
        }

        [Fact]
        public virtual void GroupBy_simple()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID),
                entryCount: 830,
                asserter: (l2oResults, efResults) =>
                {
                    var efGroupings = efResults.Cast<IGrouping<string, Order>>().ToList();

                    foreach (IGrouping<string, Order> l2oGrouping in l2oResults)
                    {
                        var efGrouping = efGroupings.Single(efg => efg.Key == l2oGrouping.Key);

                        Assert.Equal(l2oGrouping.OrderBy(o => o.OrderID), efGrouping.OrderBy(o => o.OrderID));
                    }
                });
        }

        [Fact]
        public virtual void GroupBy_simple2()
        {
            AssertQuery<Order>(
                os => os.GroupBy(o => o.CustomerID).Select(g => g),
                entryCount: 830,
                asserter: (l2oResults, efResults) =>
                {
                    var efGroupings = efResults.Cast<IGrouping<string, Order>>().ToList();

                    foreach (IGrouping<string, Order> l2oGrouping in l2oResults)
                    {
                        var efGrouping = efGroupings.Single(efg => efg.Key == l2oGrouping.Key);

                        Assert.Equal(l2oGrouping.OrderBy(o => o.OrderID), efGrouping.OrderBy(o => o.OrderID));
                    }
                });
        }

        [Fact]
        public virtual void GroupBy_first()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.CustomerID == "ALFKI").GroupBy(o => o.CustomerID).Cast<object>().First(),
                asserter: (l2oResult, efResult) =>
                {
                    var l2oGrouping = (IGrouping<string, Order>)l2oResult;
                    var efGrouping = (IGrouping<string, Order>)efResult;

                    Assert.Equal(l2oGrouping.Key, efGrouping.Key);
                    Assert.Equal(l2oGrouping.OrderBy(o => o.OrderID), efGrouping.OrderBy(o => o.OrderID));
                },
                entryCount: 6);
        }

        [Fact]
        public virtual void GroupBy_Sum()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID).Select(g => g.Sum(o => o.OrderID)));
        }

        [Fact]
        public virtual void GroupBy_Count()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID).Select(g => g.Count()));
        }

        [Fact]
        public virtual void GroupBy_LongCount()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID).Select(g => g.LongCount()));
        }

        [Fact]
        public virtual void GroupBy_Shadow()
        {
            AssertQuery<Employee>(es =>
                es.Where(e => EF.Property<string>(e, "Title") == "Sales Representative"
                              && e.EmployeeID == 1)
                    .GroupBy(e => EF.Property<string>(e, "Title"))
                    .Select(g => EF.Property<string>(g.First(), "Title")));
        }

        [Fact]
        public virtual void GroupBy_Shadow2()
        {
            AssertQuery<Employee>(es =>
                es.Where(e => EF.Property<string>(e, "Title") == "Sales Representative"
                              && e.EmployeeID == 1)
                    .GroupBy(e => EF.Property<string>(e, "Title"))
                    .Select(g => g.First()));
        }

        [Fact]
        public virtual void GroupBy_Shadow3()
        {
            AssertQuery<Employee>(es =>
                es.Where(e => e.EmployeeID == 1)
                    .GroupBy(e => e.EmployeeID)
                    .Select(g => EF.Property<string>(g.First(), "Title")));
        }

        [Fact]
        public virtual void GroupBy_Sum_Min_Max_Avg()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID).Select(g =>
                    new
                    {
                        Sum = g.Sum(o => o.OrderID),
                        Min = g.Min(o => o.OrderID),
                        Max = g.Max(o => o.OrderID),
                        Avg = g.Average(o => o.OrderID)
                    }));
        }

        [Fact]
        public virtual void GroupBy_with_result_selector()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID, (k, g) =>
                    new
                    {
                        Sum = g.Sum(o => o.OrderID),
                        Min = g.Min(o => o.OrderID),
                        Max = g.Max(o => o.OrderID),
                        Avg = g.Average(o => o.OrderID)
                    }));
        }

        [Fact]
        public virtual void GroupBy_with_element_selector_sum()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Sum()));
        }



        [Fact]
        public virtual void GroupBy_with_element_selector()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID, o => o.OrderID)
                    .OrderBy(g => g.Key)
                    .Select(g => g.OrderBy(o => o)),
                asserter:
                    (l2oResults, efResults) =>
                    {
                        var l2oObjects
                            = l2oResults
                                .SelectMany(q1 => ((IEnumerable<int>)q1));

                        var efObjects
                            = efResults
                                .SelectMany(q1 => ((IEnumerable<int>)q1));

                        Assert.Equal(l2oObjects, efObjects);
                    });
        }

        [Fact]
        public virtual void GroupBy_with_element_selector2()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID)
                    .OrderBy(g => g.Key)
                    .Select(g => g.OrderBy(o => o.OrderID)),
                asserter:
                    (l2oResults, efResults) =>
                    {
                        var l2oObjects
                            = l2oResults
                                .SelectMany(q1 => ((IEnumerable<Order>)q1));

                        var efObjects
                            = efResults
                                .SelectMany(q1 => ((IEnumerable<Order>)q1));

                        Assert.Equal(l2oObjects, efObjects);
                    });
        }

        [Fact]
        public virtual void GroupBy_with_element_selector3()
        {
            AssertQuery<Employee>(es =>
                es.GroupBy(e => e.EmployeeID)
                    .OrderBy(g => g.Key)
                    .Select(g => g.Select(e => new { Title = EF.Property<string>(e, "Title"), e }).ToList()),
                assertOrder: true);
        }

        [Fact]
        public virtual void GroupBy_with_element_selector_sum_max()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID, o => o.OrderID)
                    .Select(g => new { Sum = g.Sum(), Max = g.Max() }));
        }

        [Fact]
        public virtual void GroupBy_with_anonymous_element()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID, o => new { o.OrderID })
                    .Select(g => g.Sum(x => x.OrderID)));
        }

        [Fact]
        public virtual void GroupBy_with_two_part_key()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => new { o.CustomerID, o.OrderDate })
                    .Select(g => g.Sum(o => o.OrderID)));
        }

        [Fact]
        public virtual void OrderBy_GroupBy()
        {
            AssertQuery<Order>(os =>
                os.OrderBy(o => o.OrderID)
                    .GroupBy(o => o.CustomerID)
                    .Select(g => g.Sum(o => o.OrderID)));
        }

        [Fact]
        public virtual void OrderBy_GroupBy_SelectMany()
        {
            AssertQuery<Order>(os =>
                os.OrderBy(o => o.OrderID)
                    .GroupBy(o => o.CustomerID)
                    .SelectMany(g => g),
                entryCount: 830);
        }

        [Fact]
        public virtual void OrderBy_GroupBy_SelectMany_shadow()
        {
            AssertQuery<Employee>(es =>
                es.OrderBy(e => e.EmployeeID)
                    .GroupBy(e => e.EmployeeID)
                    .SelectMany(g => g)
                    .Select(g => EF.Property<string>(g, "Title")));
        }

        [Fact]
        public virtual void Sum_with_no_arg()
        {
            AssertQuery<Order>(os => os.Select(o => o.OrderID).Sum());
        }

        [Fact]
        public virtual void Sum_with_binary_expression()
        {
            AssertQuery<Order>(os => os.Select(o => o.OrderID * 2).Sum());
        }

        [Fact]
        public virtual void Sum_with_no_arg_empty()
        {
            AssertQuery<Order>(os => os.Where(o => o.OrderID == 42).Select(o => o.OrderID).Sum());
        }

        [Fact]
        public virtual void Sum_with_arg()
        {
            AssertQuery<Order>(os => os.Sum(o => o.OrderID));
        }

        [Fact]
        public virtual void Sum_with_arg_expression()
        {
            AssertQuery<Order>(os => os.Sum(o => o.OrderID + o.OrderID));
        }

        [Fact]
        public virtual void Min_with_no_arg()
        {
            AssertQuery<Order>(os => os.Select(o => o.OrderID).Min());
        }

        [Fact]
        public virtual void Min_with_arg()
        {
            AssertQuery<Order>(os => os.Min(o => o.OrderID));
        }

        [Fact]
        public virtual void Max_with_no_arg()
        {
            AssertQuery<Order>(os => os.Select(o => o.OrderID).Max());
        }

        [Fact]
        public virtual void Max_with_arg()
        {
            AssertQuery<Order>(os => os.Max(o => o.OrderID));
        }

        [Fact]
        public virtual void Count_with_no_predicate()
        {
            AssertQuery<Order>(os => os.Count());
        }

        [Fact]
        public virtual void Count_with_predicate()
        {
            AssertQuery<Order>(os =>
                os.Count(o => o.CustomerID == "ALFKI"));
        }

        [Fact]
        public virtual void Count_with_order_by()
        {
            AssertQuery<Order>(os => os.OrderBy(o => o.CustomerID).Count());
        }

        [Fact]
        public virtual void Where_OrderBy_Count()
        {
            AssertQuery<Order>(os => os.Where(o => o.CustomerID == "ALFKI").OrderBy(o => o.OrderID).Count());
        }

        [Fact]
        public virtual void OrderBy_Where_Count()
        {
            AssertQuery<Order>(os => os.OrderBy(o => o.OrderID).Where(o => o.CustomerID == "ALFKI").Count());
        }

        [Fact]
        public virtual void OrderBy_Count_with_predicate()
        {
            AssertQuery<Order>(os => os.OrderBy(o => o.OrderID).Count(o => o.CustomerID == "ALFKI"));
        }

        [Fact]
        public virtual void OrderBy_Where_Count_with_predicate()
        {
            AssertQuery<Order>(os => os.OrderBy(o => o.OrderID).Where(o => o.OrderID > 10).Count(o => o.CustomerID != "ALFKI"));
        }

        [Fact]
        public virtual void Where_OrderBy_Count_client_eval()
        {
            AssertQuery<Order>(os => os.Where(o => ClientEvalPredicate(o)).OrderBy(o => ClientEvalSelectorStateless()).Count());
        }

        [Fact]
        public virtual void Where_OrderBy_Count_client_eval_mixed()
        {
            AssertQuery<Order>(os => os.Where(o => o.OrderID > 10).OrderBy(o => ClientEvalPredicate(o)).Count());
        }

        [Fact]
        public virtual void OrderBy_Where_Count_client_eval()
        {
            AssertQuery<Order>(os => os.OrderBy(o => ClientEvalSelectorStateless()).Where(o => ClientEvalPredicate(o)).Count());
        }

        [Fact]
        public virtual void OrderBy_Where_Count_client_eval_mixed()
        {
            AssertQuery<Order>(os => os.OrderBy(o => o.OrderID).Where(o => ClientEvalPredicate(o)).Count());
        }

        [Fact]
        public virtual void OrderBy_Count_with_predicate_client_eval()
        {
            AssertQuery<Order>(os => os.OrderBy(o => ClientEvalSelectorStateless()).Count(o => ClientEvalPredicate(o)));
        }

        [Fact]
        public virtual void OrderBy_Count_with_predicate_client_eval_mixed()
        {
            AssertQuery<Order>(os => os.OrderBy(o => o.OrderID).Count(o => ClientEvalPredicateStateless()));
        }

        [Fact]
        public virtual void OrderBy_Where_Count_with_predicate_client_eval()
        {
            AssertQuery<Order>(os => os.OrderBy(o => ClientEvalSelectorStateless()).Where(o => ClientEvalPredicateStateless()).Count(o => ClientEvalPredicate(o)));
        }

        [Fact]
        public virtual void OrderBy_Where_Count_with_predicate_client_eval_mixed()
        {
            AssertQuery<Order>(os => os.OrderBy(o => o.OrderID).Where(o => ClientEvalPredicate(o)).Count(o => o.CustomerID != "ALFKI"));
        }

        public static bool ClientEvalPredicateStateless()
        {
            return true;
        }

        protected static bool ClientEvalPredicate(Order order)
        {
            return order.OrderID > 10000;
        }

        private static int ClientEvalSelectorStateless()
        {
            return 42;
        }

        protected internal int ClientEvalSelector(Order order)
        {
            return order.EmployeeID.HasValue ? order.EmployeeID.Value % 10 : 0;
        }

        [Fact]
        public virtual void Distinct()
        {
            AssertQuery<Customer>(
                cs => cs.Distinct(),
                entryCount: 91);
        }

        [Fact]
        public virtual void Distinct_Scalar()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => c.City).Distinct());
        }

        [Fact]
        public virtual void OrderBy_Distinct()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Select(c => c.City).Distinct());
        }

        [Fact]
        public virtual void Distinct_OrderBy()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => c.Country).Distinct().OrderBy(c => c),
                assertOrder: true);
        }

        [Fact]
        public virtual void Distinct_OrderBy2()
        {
            AssertQuery<Customer>(
                cs => cs.Distinct().OrderBy(c => c.CustomerID),
                assertOrder: true,
                entryCount: 91);
        }

        [Fact]
        public virtual void Distinct_OrderBy3()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.CustomerID }).Distinct().OrderBy(a => a.CustomerID),
                assertOrder: true);
        }

        [Fact]
        public virtual void Distinct_GroupBy()
        {
            AssertQuery<Order>(os =>
                os.Distinct()
                    .GroupBy(o => o.CustomerID)
                    .OrderBy(g => g.Key)
                    .Select(g => new { g.Key, c = g.Count() }),
                assertOrder: true);
        }

        [Fact]
        public virtual void GroupBy_Distinct()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID).Distinct().Select(g => g.Key));
        }

        [Fact]
        public virtual void Distinct_Count()
        {
            AssertQuery<Customer>(
                cs => cs.Distinct().Count());
        }

        [Fact]
        public virtual void Select_Distinct_Count()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => c.City).Distinct().Count());
        }

        [Fact]
        public virtual void Select_Select_Distinct_Count()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => c.City).Select(c => c).Distinct().Count());
        }

        [Fact]
        public virtual void Single_Throws()
        {
            Assert.Throws<InvalidOperationException>(() =>
                AssertQuery<Customer>(cs => cs.Single()));
        }

        [Fact]
        public virtual void Single_Predicate()
        {
            AssertQuery<Customer>(
                cs => cs.Single(c => c.CustomerID == "ALFKI"));
        }

        [Fact]
        public virtual void Where_Single()
        {
            AssertQuery<Customer>(
                // ReSharper disable once ReplaceWithSingleCallToSingle
                cs => cs.Where(c => c.CustomerID == "ALFKI").Single());
        }

        [Fact]
        public virtual void SingleOrDefault_Throws()
        {
            Assert.Throws<InvalidOperationException>(() =>
                AssertQuery<Customer>(
                    cs => cs.SingleOrDefault()));
        }

        [Fact]
        public virtual void SingleOrDefault_Predicate()
        {
            AssertQuery<Customer>(
                cs => cs.SingleOrDefault(c => c.CustomerID == "ALFKI"));
        }

        [Fact]
        public virtual void Where_SingleOrDefault()
        {
            AssertQuery<Customer>(
                // ReSharper disable once ReplaceWithSingleCallToSingleOrDefault
                cs => cs.Where(c => c.CustomerID == "ALFKI").SingleOrDefault());
        }

        [Fact]
        public virtual void First()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).First());
        }

        [Fact]
        public virtual void First_Predicate()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).First(c => c.City == "London"));
        }

        [Fact]
        public virtual void Where_First()
        {
            AssertQuery<Customer>(
                // ReSharper disable once ReplaceWithSingleCallToFirst
                cs => cs.OrderBy(c => c.ContactName).Where(c => c.City == "London").First());
        }

        [Fact]
        public virtual void FirstOrDefault()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).FirstOrDefault());
        }

        [Fact]
        public virtual void FirstOrDefault_Predicate()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).FirstOrDefault(c => c.City == "London"));
        }

        [Fact]
        public virtual void Where_FirstOrDefault()
        {
            AssertQuery<Customer>(
                // ReSharper disable once ReplaceWithSingleCallToFirstOrDefault
                cs => cs.OrderBy(c => c.ContactName).Where(c => c.City == "London").FirstOrDefault());
        }

        [Fact]
        public virtual void Last()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Last());
        }

        [Fact]
        public virtual void Last_when_no_order_by()
        {
            AssertQuery<Customer>(
                // ReSharper disable once ReplaceWithSingleCallToLast
                cs => cs.Where(c => c.CustomerID == "ALFKI").Last());
        }

        [Fact]
        public virtual void Last_Predicate()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Last(c => c.City == "London"));
        }

        [Fact]
        public virtual void Where_Last()
        {
            AssertQuery<Customer>(
                // ReSharper disable once ReplaceWithSingleCallToLast
                cs => cs.OrderBy(c => c.ContactName).Where(c => c.City == "London").Last());
        }

        [Fact]
        public virtual void LastOrDefault()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).LastOrDefault());
        }

        [Fact]
        public virtual void LastOrDefault_Predicate()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).LastOrDefault(c => c.City == "London"));
        }

        [Fact]
        public virtual void Where_LastOrDefault()
        {
            AssertQuery<Customer>(
                // ReSharper disable once ReplaceWithSingleCallToLastOrDefault
                cs => cs.OrderBy(c => c.ContactName).Where(c => c.City == "London").LastOrDefault());
        }

        [Fact]
        public virtual void String_StartsWith_Literal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.StartsWith("M")),
                entryCount: 12);
        }

        [Fact]
        public virtual void String_StartsWith_Identity()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.StartsWith(c.ContactName)),
                entryCount: 91);
        }

        [Fact]
        public virtual void String_StartsWith_Column()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.StartsWith(c.ContactName)),
                entryCount: 91);
        }

        [Fact]
        public virtual void String_StartsWith_MethodCall()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.StartsWith(LocalMethod1())),
                entryCount: 12);
        }

        [Fact]
        public virtual void String_EndsWith_Literal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.EndsWith("b")),
                entryCount: 1);
        }

        [Fact]
        public virtual void String_EndsWith_Identity()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.EndsWith(c.ContactName)),
                entryCount: 91);
        }

        [Fact]
        public virtual void String_EndsWith_Column()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.EndsWith(c.ContactName)),
                entryCount: 91);
        }

        [Fact]
        public virtual void String_EndsWith_MethodCall()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.EndsWith(LocalMethod2())),
                entryCount: 1);
        }

        [Fact]
        public virtual void String_Contains_Literal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains("M")),
                entryCount: 19);
        }

        [Fact]
        public virtual void String_Contains_Identity()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains(c.ContactName)),
                entryCount: 91);
        }

        [Fact]
        public virtual void String_Contains_Column()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains(c.ContactName)),
                entryCount: 91);
        }

        [Fact]
        public virtual void String_Contains_MethodCall()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains(LocalMethod1())),
                entryCount: 19);
        }

        protected static string LocalMethod1()
        {
            return "M";
        }

        protected static string LocalMethod2()
        {
            return "m";
        }

        [Fact]
        public virtual void Where_math_abs1()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Math.Abs(od.ProductID) > 10),
                entryCount: 1939);
        }

        [Fact]
        public virtual void Where_math_abs2()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Math.Abs(od.Quantity) > 10),
                entryCount: 1547);
        }

        [Fact]
        public virtual void Where_math_abs3()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Math.Abs(od.UnitPrice) > 10),
                entryCount: 1677);
        }

        [Fact]
        public virtual void Where_math_abs_uncorrelated()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Math.Abs(-10) < od.ProductID),
                entryCount: 1939);
        }

        [Fact]
        public virtual void Where_math_ceiling1()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Math.Ceiling(od.Discount) > 0),
                entryCount: 838);
        }

        [Fact]
        public virtual void Where_math_ceiling2()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Math.Ceiling(od.UnitPrice) > 10),
                entryCount: 1677);
        }

        [Fact]
        public virtual void Where_math_floor()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Math.Floor(od.UnitPrice) > 10),
                entryCount: 1658);
        }

        [Fact]
        public virtual void Where_math_power()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Math.Pow(od.Discount, 2) > 0.05f),
                entryCount: 154);
        }

        [Fact]
        public virtual void Where_math_round()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Math.Round(od.UnitPrice) > 10),
                entryCount: 1662);
        }

        [Fact]
        public virtual void Where_math_truncate()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Math.Truncate(od.UnitPrice) > 10),
                entryCount: 1658);
        }

        [Fact]
        public virtual void Where_guid_newguid()
        {
            AssertQuery<OrderDetail>(
                ods => ods.Where(od => Guid.NewGuid() != default(Guid)),
                entryCount: 2155);
        }

        [Fact]
        public virtual void Where_string_to_upper()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID.ToUpper() == "ALFKI"),
                entryCount: 1);
        }
        [Fact]
        public virtual void Where_string_to_lower()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID.ToLower() == "alfki"),
                entryCount: 1);
        }

        [Fact]
        public virtual void Where_functions_nested()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => Math.Pow(c.CustomerID.Length, 2) == 25),
                entryCount: 91);
        }

        [Fact]
        public virtual void Convert_ToByte()
        {
            var convertMethods = new List<Expression<Func<Order, bool>>>
            {
                o => Convert.ToByte(Convert.ToByte(o.OrderID % 1)) >= 0,
                o => Convert.ToByte(Convert.ToDecimal(o.OrderID % 1)) >= 0,
                o => Convert.ToByte(Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToByte((float)Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToByte(Convert.ToInt16(o.OrderID % 1)) >= 0,
                o => Convert.ToByte(Convert.ToInt32(o.OrderID % 1)) >= 0,
                o => Convert.ToByte(Convert.ToInt64(o.OrderID % 1)) >= 0,
                o => Convert.ToByte(Convert.ToString(o.OrderID % 1)) >= 0,
            };

            foreach (var convertMethod in convertMethods)
            {
                AssertQuery<Order>(
                    os => os.Where(o => o.CustomerID == "ALFKI")
                        .Where(convertMethod),
                    entryCount: 6);
            }
        }

        [Fact]
        public virtual void Convert_ToDecimal()
        {
            var convertMethods = new List<Expression<Func<Order, bool>>>
            {
                o => Convert.ToDecimal(Convert.ToByte(o.OrderID % 1)) >= 0,
                o => Convert.ToDecimal(Convert.ToDecimal(o.OrderID % 1)) >= 0,
                o => Convert.ToDecimal(Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToDecimal((float)Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToDecimal(Convert.ToInt16(o.OrderID % 1)) >= 0,
                o => Convert.ToDecimal(Convert.ToInt32(o.OrderID % 1)) >= 0,
                o => Convert.ToDecimal(Convert.ToInt64(o.OrderID % 1)) >= 0,
                o => Convert.ToDecimal(Convert.ToString(o.OrderID % 1)) >= 0,
            };

            foreach (var convertMethod in convertMethods)
            {
                AssertQuery<Order>(
                    os => os.Where(o => o.CustomerID == "ALFKI")
                        .Where(convertMethod),
                    entryCount: 6);
            }
        }

        [Fact]
        public virtual void Convert_ToDouble()
        {
            var convertMethods = new List<Expression<Func<Order, bool>>>
            {
                o => Convert.ToDouble(Convert.ToByte(o.OrderID % 1)) >= 0,
                o => Convert.ToDouble(Convert.ToDecimal(o.OrderID % 1)) >= 0,
                o => Convert.ToDouble(Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToDouble((float)Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToDouble(Convert.ToInt16(o.OrderID % 1)) >= 0,
                o => Convert.ToDouble(Convert.ToInt32(o.OrderID % 1)) >= 0,
                o => Convert.ToDouble(Convert.ToInt64(o.OrderID % 1)) >= 0,
                o => Convert.ToDouble(Convert.ToString(o.OrderID % 1)) >= 0,
            };

            foreach (var convertMethod in convertMethods)
            {
                AssertQuery<Order>(
                    os => os.Where(o => o.CustomerID == "ALFKI")
                        .Where(convertMethod),
                    entryCount: 6);
            }
        }

        [Fact]
        public virtual void Convert_ToInt16()
        {
            var convertMethods = new List<Expression<Func<Order, bool>>>
            {
                o => Convert.ToInt16(Convert.ToByte(o.OrderID % 1)) >= 0,
                o => Convert.ToInt16(Convert.ToDecimal(o.OrderID % 1)) >= 0,
                o => Convert.ToInt16(Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToInt16((float)Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToInt16(Convert.ToInt16(o.OrderID % 1)) >= 0,
                o => Convert.ToInt16(Convert.ToInt32(o.OrderID % 1)) >= 0,
                o => Convert.ToInt16(Convert.ToInt64(o.OrderID % 1)) >= 0,
                o => Convert.ToInt16(Convert.ToString(o.OrderID % 1)) >= 0,
            };

            foreach (var convertMethod in convertMethods)
            {
                AssertQuery<Order>(
                    os => os.Where(o => o.CustomerID == "ALFKI")
                        .Where(convertMethod),
                    entryCount: 6);
            }
        }

        [Fact]
        public virtual void Convert_ToInt32()
        {
            var convertMethods = new List<Expression<Func<Order, bool>>>
            {
                o => Convert.ToInt32(Convert.ToByte(o.OrderID % 1)) >= 0,
                o => Convert.ToInt32(Convert.ToDecimal(o.OrderID % 1)) >= 0,
                o => Convert.ToInt32(Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToInt32((float)Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToInt32(Convert.ToInt16(o.OrderID % 1)) >= 0,
                o => Convert.ToInt32(Convert.ToInt32(o.OrderID % 1)) >= 0,
                o => Convert.ToInt32(Convert.ToInt64(o.OrderID % 1)) >= 0,
                o => Convert.ToInt32(Convert.ToString(o.OrderID % 1)) >= 0,
            };

            foreach (var convertMethod in convertMethods)
            {
                AssertQuery<Order>(
                    os => os.Where(o => o.CustomerID == "ALFKI")
                        .Where(convertMethod),
                    entryCount: 6);
            }
        }

        [Fact]
        public virtual void Convert_ToInt64()
        {
            var convertMethods = new List<Expression<Func<Order, bool>>>
            {
                o => Convert.ToInt64(Convert.ToByte(o.OrderID % 1)) >= 0,
                o => Convert.ToInt64(Convert.ToDecimal(o.OrderID % 1)) >= 0,
                o => Convert.ToInt64(Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToInt64((float)Convert.ToDouble(o.OrderID % 1)) >= 0,
                o => Convert.ToInt64(Convert.ToInt16(o.OrderID % 1)) >= 0,
                o => Convert.ToInt64(Convert.ToInt32(o.OrderID % 1)) >= 0,
                o => Convert.ToInt64(Convert.ToInt64(o.OrderID % 1)) >= 0,
                o => Convert.ToInt64(Convert.ToString(o.OrderID % 1)) >= 0,
            };

            foreach (var convertMethod in convertMethods)
            {
                AssertQuery<Order>(
                    os => os.Where(o => o.CustomerID == "ALFKI")
                        .Where(convertMethod),
                    entryCount: 6);
            }
        }

        //[Fact]
        public virtual void Convert_ToString()
        {
            var convertMethods = new List<Expression<Func<Order, bool>>>
            {
                o => Convert.ToString(Convert.ToByte(o.OrderID % 1)) != "10",
                o => Convert.ToString(Convert.ToDecimal(o.OrderID % 1)) != "10",
                o => Convert.ToString(Convert.ToDouble(o.OrderID % 1)) != "10",
                o => Convert.ToString((float)Convert.ToDouble(o.OrderID % 1)) != "10",
                o => Convert.ToString(Convert.ToInt16(o.OrderID % 1)) != "10",
                o => Convert.ToString(Convert.ToInt32(o.OrderID % 1)) != "10",
                o => Convert.ToString(Convert.ToInt64(o.OrderID % 1)) != "10",
                o => Convert.ToString(Convert.ToString(o.OrderID % 1)) != "10",
            };

            foreach (var convertMethod in convertMethods)
            {
                AssertQuery<Order>(
                    os => os.Where(o => o.CustomerID == "ALFKI")
                        .Where(convertMethod),
                    entryCount: 6);
            }
        }

        [Fact]
        public virtual void GroupJoin_simple()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID into orders
                from o in orders
                select o);
        }

        [Fact]
        public virtual void GroupJoin_simple_subquery()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os.OrderBy(o => o.OrderID).Take(4) on c.CustomerID equals o.CustomerID into orders
                from o in orders
                select o);
        }

        [Fact]
        public virtual void GroupJoin_projection()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID into orders
                from o in orders
                select new { c, o });
        }

        [Fact]
        public virtual void GroupJoin_DefaultIfEmpty()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID into orders
                from o in orders.DefaultIfEmpty()
                select new { c, o });
        }

        [Fact]
        public virtual void GroupJoin_DefaultIfEmpty2()
        {
            AssertQuery<Employee, Order>((es, os) =>
                from e in es
                join o in os on e.EmployeeID equals o.EmployeeID into orders
                from o in orders.DefaultIfEmpty()
                select new { e, o });
        }

        [Fact]
        public virtual void GroupJoin_DefaultIfEmpty3()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs//.Take(1)
                join o in os on c.CustomerID equals o.CustomerID into orders
                from o in orders.DefaultIfEmpty()
                select o);
        }

        [Fact]
        public virtual void SelectMany_Joined()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                from o in os.Where(o => o.CustomerID == c.CustomerID)
                select new { c.ContactName, o.OrderDate });
        }

        [Fact]
        public virtual void SelectMany_Joined_DefaultIfEmpty()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                from o in os.Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                select new { c.ContactName, o });
        }

        [Fact]
        public virtual void SelectMany_Joined_Take()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                from o in os.Where(o => o.CustomerID == c.CustomerID).Take(1000)
                select new { c.ContactName, o });
        }

        [Fact]
        public virtual void SelectMany_Joined_DefaultIfEmpty2()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                from o in os.Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                select o);
        }

        [Fact]
        public virtual void Select_many_cross_join_same_collection()
        {
            AssertQuery<Customer, Customer>((cs1, cs2) =>
                cs1.SelectMany(c => cs2));
        }

        [Fact]
        public virtual void Join_same_collection_multiple()
        {
            AssertQuery<Customer, Customer, Customer>((cs1, cs2, cs3) =>
                cs1.Join(cs2, o => o.CustomerID, i => i.CustomerID, (c1, c2) => new { c1, c2 }).Join(cs3, o => o.c1.CustomerID, i => i.CustomerID, (c12, c3) => c3));
        }

        [Fact]
        public virtual void Join_same_collection_force_alias_uniquefication()
        {
            AssertQuery<Order, Order>((os1, os2) =>
                os1.Join(os2, o => o.CustomerID, i => i.CustomerID, (_, o) => new { _, o }));
        }

        [Fact]
        public virtual void Contains_with_subquery()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                cs.Where(c => os.Select(o => o.CustomerID).Contains(c.CustomerID)));
        }

        [Fact]
        public virtual void Contains_with_local_array_closure()
        {
            string[] ids = { "ABCDE", "ALFKI" };
            AssertQuery<Customer>(cs =>
                cs.Where(c => ids.Contains(c.CustomerID)), entryCount: 1);
        }

        [Fact]
        public virtual void Contains_with_local_array_inline()
        {
            AssertQuery<Customer>(cs =>
                cs.Where(c => new[] { "ABCDE", "ALFKI" }.Contains(c.CustomerID)), entryCount: 1);
        }

        [Fact]
        public virtual void Contains_with_local_list_closure()
        {
            var ids = new List<string> { "ABCDE", "ALFKI" };
            AssertQuery<Customer>(cs =>
                cs.Where(c => ids.Contains(c.CustomerID)), entryCount: 1);
        }

        [Fact]
        public virtual void Contains_with_local_list_inline()
        {
            AssertQuery<Customer>(cs =>
                cs.Where(c => new List<string> { "ABCDE", "ALFKI" }.Contains(c.CustomerID)), entryCount: 1);
        }

        [Fact]
        public virtual void Contains_with_local_list_inline_closure_mix()
        {
            var alfki = "ALFKI";
            AssertQuery<Customer>(cs =>
                cs.Where(c => new List<string> { "ABCDE", alfki }.Contains(c.CustomerID)), entryCount: 1);
        }

        [Fact]
        public virtual void Contains_with_local_collection_false()
        {
            string[] ids = { "ABCDE", "ALFKI" };
            AssertQuery<Customer>(cs =>
                cs.Where(c => !ids.Contains(c.CustomerID)), entryCount: 90);
        }

        [Fact]
        public virtual void Contains_with_local_collection_complex_predicate_and()
        {
            string[] ids = { "ABCDE", "ALFKI" };
            AssertQuery<Customer>(cs =>
                cs.Where(c => (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE") && ids.Contains(c.CustomerID)), entryCount: 1);
        }

        [Fact]
        public virtual void Contains_with_local_collection_complex_predicate_or()
        {
            string[] ids = { "ABCDE", "ALFKI" };
            AssertQuery<Customer>(cs =>
                cs.Where(c => ids.Contains(c.CustomerID) || (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE")), entryCount: 1);
        }

        [Fact]
        public virtual void Contains_with_local_collection_complex_predicate_not_matching_ins1()
        {
            string[] ids = { "ABCDE", "ALFKI" };
            AssertQuery<Customer>(cs =>
                cs.Where(c => (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE") || !ids.Contains(c.CustomerID)), entryCount: 91);
        }

        [Fact]
        public virtual void Contains_with_local_collection_complex_predicate_not_matching_ins2()
        {
            string[] ids = { "ABCDE", "ALFKI" };
            AssertQuery<Customer>(cs =>
                cs.Where(c => ids.Contains(c.CustomerID) && (c.CustomerID != "ALFKI" && c.CustomerID != "ABCDE")));
        }

        [Fact]
        public virtual void Contains_with_local_collection_sql_injection()
        {
            string[] ids = { "ALFKI", "ABC')); GO; DROP TABLE Orders; GO; --" };
            AssertQuery<Customer>(cs =>
                cs.Where(c => ids.Contains(c.CustomerID) || (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE")), entryCount: 1);
        }

        [Fact]
        public virtual void Contains_with_local_collection_empty_closure()
        {
            string[] ids = new string[0];

            AssertQuery<Customer>(cs =>
                cs.Where(c => ids.Contains(c.CustomerID)));
        }

        [Fact]
        public virtual void Contains_with_local_collection_empty_inline()
        {
            AssertQuery<Customer>(cs =>
                cs.Where(c => !(new List<string>().Contains(c.CustomerID))), entryCount: 91);
        }

        [Fact]
        public virtual void Contains_top_level()
        {
            AssertQuery<Customer>(cs =>
                cs.Select(c => c.CustomerID).Contains("ALFKI"));
        }

        [Fact]
        public virtual void Where_chain()
        {
            AssertQuery<Order>(order => order
                .Where(o => o.CustomerID == "QUICK")
                .Where(o => o.OrderDate > new DateTime(1998, 1, 1)), entryCount: 8);
        }


        [Fact]
        public virtual void OrderBy_null_coalesce_operator()
        {
            AssertQuery<Customer>(customer => customer
                .OrderBy(c => c.Region ?? "ZZ"),
                entryCount: 91);
        }

        [Fact] //Issue 1798
        public virtual void Select_null_coalesce_operator()
        {
            AssertQuery<Customer>(customer => customer
                .Select(c => new { c.CustomerID, c.CompanyName, Region = c.Region ?? "ZZ" }).OrderBy(o => o.Region));
        }

        [Fact]
        public virtual void OrderBy_conditional_operator()
        {
            AssertQuery<Customer>(customer => customer
                .OrderBy(c => c.Region == null ? "ZZ" : c.Region),
                entryCount: 91);
        }

        [Fact] //Issue 1798
        public virtual void Projection_null_coalesce_operator()
        {
            AssertQuery<Customer>(customer => customer
                .Select(c => new { c.CustomerID, c.CompanyName, Region = c.Region ?? "ZZ" }));
        }

        [Fact]
        public virtual void Filter_coalesce_operator()
        {
            AssertQuery<Customer>(customer => customer
                .Where(c => (c.CompanyName ?? c.ContactName) == "The Big Cheese"),
                entryCount: 1);
        }

        [Fact]
        public virtual void Take_skip_null_coalesce_operator()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.Region ?? "ZZ").Take(10).Skip(5).Distinct(),
                entryCount: 5);
        }

        [Fact]
        public virtual void Select_take_null_coalesce_operator()
        {
            AssertQuery<Customer>(
            cs => cs.Select(c => new { c.CustomerID, c.CompanyName, Region = c.Region ?? "ZZ" }).OrderBy(c => c.Region).Take(5));
        }

        [Fact]
        public virtual void Select_take_skip_null_coalesce_operator()
        {
            AssertQuery<Customer>(
            cs => cs.Select(c => new { c.CustomerID, c.CompanyName, Region = c.Region ?? "ZZ" }).OrderBy(c => c.Region).Take(10).Skip(5));
        }

        [Fact]
        public virtual void Select_Property_when_non_shadow()
        {
            AssertQuery<Order>(os =>
                from o in os
                select EF.Property<int>(o, "OrderID"));
        }

        [Fact]
        public virtual void Where_Property_when_non_shadow()
        {
            AssertQuery<Order>(os =>
                from o in os
                where EF.Property<int>(o, "OrderID") == 10248
                select o,
                entryCount: 1);
        }

        [Fact]
        public virtual void Select_Property_when_shadow()
        {
            AssertQuery<Employee>(es =>
                from e in es
                select EF.Property<string>(e, "Title"));
        }

        [Fact]
        public virtual void Where_Property_when_shadow()
        {
            AssertQuery<Employee>(es =>
                from e in es
                where EF.Property<string>(e, "Title") == "Sales Representative"
                select e,
                entryCount: 6);
        }

        [Fact]
        public virtual void Selected_column_can_coalesce()
        {
            using (var context = CreateContext())
            {
                var customers = (from c in context.Set<Customer>()
                                 orderby c.Region ?? "ZZ"
                                 select c)
                    .ToList();

                Assert.Equal(91, customers.Count);
            }
        }

        [Fact]
        public virtual void Can_cast_CreateQuery_result_to_IQueryable_T_bug_1730()
        {
            using (var context = CreateContext())
            {
                IQueryable<Product> products = context.Products;

                // ReSharper disable once RedundantAssignment
                products = (IQueryable<Product>)products.Provider.CreateQuery(products.Expression);
            }
        }

        [Fact]
        public virtual void Can_execute_non_generic()
        {
            using (var context = CreateContext())
            {
                IQueryable<Product> products = context.Products;

                Assert.NotNull(products.Provider.Execute(
                    Expression.Call(
                        new LinqOperatorProvider().First.MakeGenericMethod(typeof(Product)),
                        products.Expression)));
            }
        }
        
        [Fact]
        public virtual void Select_Where_Subquery_Deep()
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = (from od in context.Set<OrderDetail>()
                       where (
                           from o in context.Set<Order>()
                           where od.OrderID == o.OrderID
                           select (
                               from c in context.Set<Customer>()
                               where o.CustomerID == c.CustomerID
                               select c
                               ).Single()
                           ).Single()
                           .City == "Seattle"
                       select od)
                        .Take(2)
                        .ToList();

                Assert.Equal(2, orderDetails.Count);
            }
        }

        protected NorthwindContext CreateContext()
        {
            return Fixture.CreateContext();
        }

        protected QueryTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected TFixture Fixture { get; }

        private void AssertQuery<TItem>(
            Func<IQueryable<TItem>, int> query,
            bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { query(NorthwindData.Set<TItem>()) },
                    new[] { query(context.Set<TItem>()) },
                    assertOrder);
            }
        }

        private void AssertQuery<TItem>(
            Func<IQueryable<TItem>, bool> query,
            bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { query(NorthwindData.Set<TItem>()) },
                    new[] { query(context.Set<TItem>()) },
                    assertOrder);
            }
        }

        private void AssertQuery<TItem>(
            Func<IQueryable<TItem>, TItem> query,
            bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { query(NorthwindData.Set<TItem>()) },
                    new[] { query(context.Set<TItem>()) },
                    assertOrder);
            }
        }

        private void AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, object> query,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { query(NorthwindData.Set<TItem1>(), NorthwindData.Set<TItem2>()) },
                    new[] { query(context.Set<TItem1>(), context.Set<TItem2>()) },
                    assertOrder);
            }
        }

        private void AssertQuery<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, object> query,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { query(NorthwindData.Set<TItem1>(), NorthwindData.Set<TItem2>(), NorthwindData.Set<TItem3>()) },
                    new[] { query(context.Set<TItem1>(), context.Set<TItem2>(), context.Set<TItem3>()) },
                    assertOrder);
            }
        }

        private void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<IQueryable<object>>> query,
            bool assertOrder = false,
            Action<IList<IQueryable<object>>, IList<IQueryable<object>>> asserter = null)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    query(context.Set<TItem>()).ToArray(),
                    assertOrder,
                    asserter);
            }
        }

        protected void AssertQuery<TItem>(
             Func<IQueryable<TItem>, IQueryable<object>> query,
             bool assertOrder = false,
             int entryCount = 0,
             Action<IList<object>, IList<object>> asserter = null)
             where TItem : class
        {
            AssertQuery(query, query, assertOrder, entryCount, asserter);
        }

        protected void AssertQuery<TItem>(
            Func<IQueryable<TItem>, object> query,
            bool assertOrder = false,
            int entryCount = 0,
            Action<object, object> asserter = null)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { query(NorthwindData.Set<TItem>()) },
                    new[] { query(context.Set<TItem>()) },
                    assertOrder,
                    (l2os, efs) => asserter(l2os.Single(), efs.Single()));

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        private void AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> query,
            bool assertOrder = false,
            Action<IList<object>, IList<object>> asserter = null)
            where TItem1 : class
            where TItem2 : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(NorthwindData.Set<TItem1>(), NorthwindData.Set<TItem2>()).ToArray(),
                    query(context.Set<TItem1>(), context.Set<TItem2>()).ToArray(),
                    assertOrder,
                    asserter);
            }
        }

        private void AssertQuery<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<object>> query,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(NorthwindData.Set<TItem1>(), NorthwindData.Set<TItem2>(), NorthwindData.Set<TItem3>()).ToArray(),
                    query(context.Set<TItem1>(), context.Set<TItem2>(), context.Set<TItem3>()).ToArray(),
                    assertOrder);
            }
        }

        private void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<int>> query,
            bool assertOrder = false,
            int entryCount = 0)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    query(context.Set<TItem>()).ToArray(),
                    assertOrder);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        private void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<long>> query, bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    query(context.Set<TItem>()).ToArray(),
                    assertOrder);
            }
        }

        protected void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<object>> efQuery,
            Func<IQueryable<TItem>, IQueryable<object>> l2oQuery,
            bool assertOrder = false,
            int entryCount = 0,
            Action<IList<object>, IList<object>> asserter = null)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    l2oQuery(NorthwindData.Set<TItem>()).ToArray(),
                    efQuery(context.Set<TItem>()).ToArray(),
                    assertOrder,
                    asserter);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        private void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<bool>> query, bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    query(context.Set<TItem>()).ToArray(),
                    assertOrder);
            }
        }
    }
}
