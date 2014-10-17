// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Query;
using Northwind;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class NorthwindQueryTestBase
    {
        [Fact]
        public virtual void Queryable_simple()
        {
            AssertQuery<Customer>(
                cs => cs,
                stateEntryCount: 91);
        }

        [Fact]
        public virtual void Queryable_simple_anonymous()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c }),
                stateEntryCount: 91);
        }

        [Fact]
        public virtual void Queryable_nested_simple()
        {
            AssertQuery<Customer>(
                cs =>
                    from c1 in (from c2 in (from c3 in cs select c3) select c2) select c1,
                stateEntryCount: 91);
        }

        [Fact]
        public virtual void Take_simple()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Take(10),
                assertOrder: true,
                stateEntryCount: 10);
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
                stateEntryCount: 86);
        }

        [Fact]
        public virtual void Take_Skip()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Take(10).Skip(5),
                assertOrder: true,
                stateEntryCount: 5);
        }

        [Fact]
        public virtual void Distinct_Skip()
        {
            AssertQuery<Customer>(
                cs => cs.Distinct().OrderBy(c => c.ContactName).Skip(5),
                assertOrder: true,
                stateEntryCount: 86);
        }

        [Fact]
        public virtual void Skip_Take()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Skip(5).Take(10),
                assertOrder: true,
                stateEntryCount: 10);
        }

        [Fact]
        public virtual void Distinct_Skip_Take()
        {
            AssertQuery<Customer>(
                cs => cs.Distinct().OrderBy(c => c.ContactName).Skip(5).Take(10),
                assertOrder: true,
                stateEntryCount: 10);
        }

        [Fact]
        public virtual void Skip_Distinct()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Skip(5).Distinct(),
                stateEntryCount: 86);
        }

        [Fact]
        public virtual void Skip_Take_Distinct()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Skip(5).Take(10).Distinct(),
                stateEntryCount: 10);
        }

        [Fact]
        public virtual void Take_Skip_Distinct()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Take(10).Skip(5).Distinct(),
                stateEntryCount: 5);
        }

        [Fact]
        public virtual void Take_Distinct()
        {
            AssertQuery<Order>(
                os => os.OrderBy(o => o.OrderID).Take(5).Distinct(),
                stateEntryCount: 5);
        }

        [Fact]
        public virtual void Distinct_Take()
        {
            AssertQuery<Order>(
                os => os.Distinct().OrderBy(o => o.OrderID).Take(5),
                assertOrder: true,
                stateEntryCount: 5);
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
        public virtual void Any_simple()
        {
            AssertQuery<Customer>(
                cs => cs.Any());
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
                    .Single());
        }

        [Fact]
        public virtual void Where_simple()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == "London"),
                stateEntryCount: 6);
        }

        [Fact]
        public virtual void Where_simple_shadow()
        {
            AssertQuery<Employee>(
                es => es.Where(e => e.Property<string>("Title") == "Sales Representative"),
                stateEntryCount: 6);
        }

        [Fact]
        public virtual void Where_simple_shadow_projection()
        {
            AssertQuery<Employee>(
                es => es.Where(e => e.Property<string>("Title") == "Sales Representative")
                    .Select(e => e.Property<string>("Title")));
        }

        [Fact]
        public virtual void Where_client()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.IsLondon),
                stateEntryCount: 6);
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
                stateEntryCount: 6);
        }

        [Fact]
        public virtual void Where_equals_method_int()
        {
            AssertQuery<Employee>(
                es => es.Where(e => e.EmployeeID.Equals(1)),
                stateEntryCount: 1);
        }

        [Fact]
        public virtual void Where_comparison_nullable_type_not_null()
        {
            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == 2),
                stateEntryCount: 5);
        }

        [Fact]
        public virtual void Where_comparison_nullable_type_null()
        {
            AssertQuery<Employee>(
                es => es.Where(e => e.ReportsTo == null),
                stateEntryCount: 1);
        }

        [Fact]
        public virtual void Where_string_length()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City.Length == 6),
                stateEntryCount: 20);
        }

        [Fact]
        public virtual void Where_simple_reversed()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => "London" == c.City),
                stateEntryCount: 6);
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
                stateEntryCount: 91);
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
                stateEntryCount: 91);
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
                stateEntryCount: 91);
        }

        [Fact]
        public virtual void Where_identity_comparison()
        {
            // ReSharper disable once EqualExpressionComparison
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == c.City),
                stateEntryCount: 91);
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
                es =>
                    es.Select(e => e.EmployeeID).Take(9).Where(i => i == 5));
        }

        [Fact]
        public virtual void Where_true()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => true),
                stateEntryCount: 91);
        }

        [Fact]
        public virtual void Where_false()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => false));
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
                stateEntryCount: 91);
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
        public virtual void Select_customer_table()
        {
            AssertQuery<Customer>(
                cs => cs,
                stateEntryCount: 91);
        }

        [Fact]
        public virtual void Select_customer_identity()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => c),
                stateEntryCount: 91);
        }

        [Fact]
        public virtual void Select_anonymous_with_object()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.City, c }),
                stateEntryCount: 91);
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
            AssertQuery<Customer>(
                cs => cs.Select(c => new { X = 10 }));
        }

        [Fact]
        public virtual void Select_constant_int()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => 0));
        }

        [Fact]
        public virtual void Select_constant_null_string()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => (string)null));
        }

        [Fact]
        public virtual void Select_local()
        {
            // ReSharper disable once ConvertToConstant.Local
            var x = 10;

            AssertQuery<Customer>(
                cs => cs.Select(c => x));
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
                select os
                    .OrderBy(o => c.CustomerID),
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
                stateEntryCount: 1);
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
                stateEntryCount: 9);
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
                stateEntryCount: 9);
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
                stateEntryCount: 91);
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
        public virtual void GroupJoin_default_if_empty()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID into orders
                from o1 in orders.DefaultIfEmpty()
                select new { c, o1 });
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
                stateEntryCount: 91);
        }

        [Fact]
        public virtual void OrderBy_client_mixed()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.IsLondon).ThenBy(c => c.CompanyName),
                assertOrder: true,
                stateEntryCount: 91);
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
                es => es.OrderBy(e => e.Property<string>("Title")).ThenBy(e => e.EmployeeID),
                assertOrder: true,
                stateEntryCount: 9);
        }

        [Fact]
        public virtual void OrderBy_ThenBy_predicate()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == "London")
                    .OrderBy(c => c.City)
                    .ThenBy(c => c.CustomerID),
                assertOrder: true,
                stateEntryCount: 6);
        }

        [Fact]
        public virtual void OrderBy_correlated_subquery_lol()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                    orderby cs.Any(c2 => c2.CustomerID == c.CustomerID)
                    select c,
                stateEntryCount: 91);
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
                stateEntryCount: 91);
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
                stateEntryCount: 830);
        }

        [Fact]
        public virtual void Sum_with_no_arg()
        {
            AssertQuery<Order>(os => os.Select(o => o.OrderID).Sum());
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
        public virtual void Distinct()
        {
            AssertQuery<Customer>(
                cs => cs.Distinct(),
                stateEntryCount: 91);
        }

        [Fact]
        public virtual void Distinct_Scalar()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.Select(c => c.City).Distinct());
        }

        [Fact]
        public virtual void OrderBy_Distinct()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.OrderBy(c => c.CustomerID).Select(c => c.City).Distinct());
        }

        [Fact]
        public virtual void Distinct_OrderBy()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.Select(c => c.City).Distinct().OrderBy(c => c),
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
                cs =>
                    cs.Select(c => c.City).Distinct().Count());
        }

        [Fact]
        public virtual void Select_Select_Distinct_Count()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.Select(c => c.City).Select(c => c).Distinct().Count());
        }

        [Fact]
        public virtual void Single_Throws()
        {
            Assert.Throws<InvalidOperationException>(() =>
                AssertQuery<Customer>(
                    cs => cs.Single()));
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
                stateEntryCount: 12);
        }

        [Fact]
        public virtual void String_StartsWith_Identity()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.StartsWith(c.ContactName)),
                stateEntryCount: 91);
        }

        [Fact]
        public virtual void String_StartsWith_Column()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.StartsWith(c.ContactName)),
                stateEntryCount: 91);
        }

        [Fact]
        public virtual void String_StartsWith_MethodCall()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.StartsWith(LocalMethod1())),
                stateEntryCount: 12);
        }

        [Fact]
        public virtual void String_EndsWith_Literal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.EndsWith("b")),
                stateEntryCount: 1);
        }

        [Fact]
        public virtual void String_EndsWith_Identity()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.EndsWith(c.ContactName)),
                stateEntryCount: 91);
        }

        [Fact]
        public virtual void String_EndsWith_Column()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.EndsWith(c.ContactName)),
                stateEntryCount: 91);
        }

        [Fact]
        public virtual void String_EndsWith_MethodCall()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.EndsWith(LocalMethod2())),
                stateEntryCount: 1);
        }

        private static string LocalMethod1()
        {
            return "M";
        }

        private static string LocalMethod2()
        {
            return "m";
        }

        [Fact]
        public virtual void JoinInto_DefaultIfEmpty()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID into ords
                from o in ords.DefaultIfEmpty()
                select new { c, o });
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

        protected abstract DbContext CreateContext();

        private void AssertQuery<TItem>(
            Func<IQueryable<TItem>, int> query,
            bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                AssertResults(
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
                AssertResults(
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
                AssertResults(
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
                AssertResults(
                    new[] { query(NorthwindData.Set<TItem1>(), NorthwindData.Set<TItem2>()) },
                    new[] { query(context.Set<TItem1>(), context.Set<TItem2>()) },
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
                AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    query(context.Set<TItem>()).ToArray(),
                    assertOrder,
                    asserter);
            }
        }

        protected void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<object>> query,
            bool assertOrder = false,
            int stateEntryCount = 0)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    query(context.Set<TItem>()).ToArray(),
                    assertOrder);

                Assert.Equal(stateEntryCount, context.ChangeTracker.Entries().Count());
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
                AssertResults(
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
                AssertResults(
                    query(NorthwindData.Set<TItem1>(), NorthwindData.Set<TItem2>(), NorthwindData.Set<TItem3>()).ToArray(),
                    query(context.Set<TItem1>(), context.Set<TItem2>(), context.Set<TItem3>()).ToArray(),
                    assertOrder);
            }
        }

        private void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<int>> query, bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    query(context.Set<TItem>()).ToArray(),
                    assertOrder);
            }
        }

        private void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<long>> query, bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    query(context.Set<TItem>()).ToArray(),
                    assertOrder);
            }
        }

        private void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<bool>> query, bool assertOrder = false)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    query(context.Set<TItem>()).ToArray(),
                    assertOrder);
            }
        }

        private static void AssertResults<T>(
            IList<T> l2oItems,
            IList<T> efItems,
            bool assertOrder,
            Action<IList<T>, IList<T>> asserter = null)
        {
            Assert.Equal(l2oItems.Count, efItems.Count);

            if (asserter != null)
            {
                asserter(l2oItems, efItems);
            }
            else
            {
                if (assertOrder)
                {
                    Assert.Equal(l2oItems, efItems);
                }
                else
                {
                    foreach (var l2oItem in l2oItems)
                    {
                        Assert.True(
                            efItems.Contains(l2oItem),
                            string.Format(
                                "\r\nL2o item: [{0}] not found in EF results: [{1}]...",
                                l2oItem,
                                string.Join(", ", efItems.Take(10))));
                    }
                }
            }
        }
    }
}
