// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

// ReSharper disable ReplaceWithSingleCallToAny
// ReSharper disable SpecifyACultureInStringConversionExplicitly
// ReSharper disable PossibleLossOfFraction
// ReSharper disable SuspiciousTypeConversion.Global
// ReSharper disable UnusedVariable
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable PossibleUnintendedReferenceComparison
// ReSharper disable InconsistentNaming
// ReSharper disable AccessToDisposedClosure
// ReSharper disable StringCompareIsCultureSpecific.1
// ReSharper disable StringEndsWithIsCultureSpecific
// ReSharper disable ReplaceWithSingleCallToCount
// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable AccessToModifiedClosure

#pragma warning disable RCS1202 // Avoid NullReferenceException.

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract partial class SimpleQueryTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected SimpleQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected NorthwindContext CreateContext()
        {
            return Fixture.CreateContext();
        }

        protected virtual void ClearLog()
        {
        }

        [ConditionalFact]
        public virtual void Multiple_context_instances()
        {
            using (var context1 = CreateContext())
            {
                using (var context2 = CreateContext())
                {
                    Assert.Equal(
                        CoreStrings.ErrorInvalidQueryable,
                        Assert.Throws<InvalidOperationException>(
                            () =>
                                (from c in context1.Customers
                                 from o in context2.Orders
                                 select c).First()).Message);
                }
            }
        }

        [ConditionalFact]
        public virtual void Multiple_context_instances_2()
        {
            using (var context1 = CreateContext())
            {
                using (var context2 = CreateContext())
                {
                    Assert.Equal(
                        CoreStrings.ErrorInvalidQueryable,
                        Assert.Throws<InvalidOperationException>(
                            () =>
                                (from c in context1.Customers
                                 from o in context2.Set<Order>()
                                 select c).First()).Message);
                }
            }
        }

        [ConditionalFact]
        public virtual void Multiple_context_instances_set()
        {
            using (var context1 = CreateContext())
            {
                using (var context2 = CreateContext())
                {
                    var set = context2.Orders;

                    Assert.Equal(
                        CoreStrings.ErrorInvalidQueryable,
                        Assert.Throws<InvalidOperationException>(
                            () => (from c in context1.Customers
                                   from o in set
                                   select c).First()).Message);
                }
            }
        }

        [ConditionalFact]
        public virtual void Multiple_context_instances_parameter()
        {
            using (var context1 = CreateContext())
            {
                using (var context2 = CreateContext())
                {
                    Customer query(NorthwindContext c2) =>
                        (from c in context1.Customers
                         from o in c2.Orders
                         select c).First();

                    Assert.Equal(
                        CoreStrings.ErrorInvalidQueryable,
                        Assert.Throws<InvalidOperationException>(
                            () => query(context2)).Message);
                }
            }
        }

        [ConditionalFact]
        public virtual void Query_when_evaluatable_queryable_method_call_with_repository()
        {
            using (var context = CreateContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                var customerRepository = new Repository<Customer>(context);
                var orderRepository = new Repository<Order>(context);

                var results
                    = customerRepository.Find()
                        .Where(c => orderRepository.Find().Any(o => o.CustomerID == c.CustomerID))
                        .ToList();

                Assert.Equal(89, results.Count);

                results
                    = (from c in customerRepository.Find()
                       where orderRepository.Find().Any(o => o.CustomerID == c.CustomerID)
                       select c)
                    .ToList();

                Assert.Equal(89, results.Count);

                var orderQuery = orderRepository.Find();

                results = customerRepository.Find()
                    .Where(c => orderQuery.Any(o => o.CustomerID == c.CustomerID))
                    .ToList();

                Assert.Equal(89, results.Count);

                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            }
        }

        protected class Repository<T>
            where T : class
        {
            private readonly NorthwindContext _context;

            public Repository(NorthwindContext bloggingContext)
            {
                _context = bloggingContext;
            }

            public IQueryable<T> Find()
            {
                return _context.Set<T>().AsQueryable();
            }
        }

        [ConditionalFact]
        public virtual void Lifting_when_subquery_nested_order_by_simple()
        {
            using (var context = CreateContext())
            {
                var results
                    = (from c1_Orders in context.Orders
                       join _c1 in
                           (from c1 in
                                (from c in context.Customers
                                 orderby c.CustomerID
                                 select c)
                                .Take(2)
                            from c2 in context.Customers
                            select EF.Property<string>(c1, "CustomerID"))
                           .Distinct()
                           on EF.Property<string>(c1_Orders, "CustomerID") equals _c1
                       orderby _c1
                       select c1_Orders).ToList();

                Assert.Equal(10, results.Count);
            }
        }

        [ConditionalFact]
        public virtual void Lifting_when_subquery_nested_order_by_anonymous()
        {
            using (var context = CreateContext())
            {
                var results
                    = (from c1_Orders in context.Orders
                       join _c1 in
                           (from c1 in
                                (from c in context.Customers
                                 orderby c.CustomerID
                                 select c)
                                .Take(2)
                            from c2 in context.Customers
                            select new
                            {
                                CustomerID = EF.Property<string>(c1, "CustomerID")
                            })
                           .Distinct()
                           on EF.Property<string>(c1_Orders, "CustomerID") equals _c1.CustomerID
                       orderby _c1.CustomerID
                       select c1_Orders).ToList();

                Assert.Equal(10, results.Count);
            }
        }

        private class Context
        {
            public readonly Dictionary<string, object> Arguments = new Dictionary<string, object>();
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Local_dictionary(bool isAsync)
        {
            var context = new Context();
            context.Arguments.Add("customerId", "ALFKI");

            return AssertSingle<Customer, Customer>(
                isAsync,
                cs => cs,
                predicate: c => c.CustomerID == (string)context.Arguments["customerId"],
                entryCount: 1);
        }

        private static IQueryable<Customer> QueryableArgQuery(NorthwindContext context, IQueryable<string> ids)
        {
            return context.Customers.Where(c => ids.Contains(c.CustomerID));
        }

        [ConditionalFact]
        public void Query_composition_against_ienumerable_set()
        {
            using (var context = CreateContext())
            {
                IEnumerable<Order> orders = context.Orders;

                var results
                    = orders
                        .Where(x => x.OrderDate < new DateTime(1996, 7, 12) && x.OrderDate > new DateTime(1996, 7, 4))
                        .OrderBy(x => x.ShippedDate)
                        .GroupBy(x => x.ShipName)
                        .ToList();

                Assert.Equal(1, results.Count);
            }
        }

        [ConditionalFact]
        public virtual void Shaper_command_caching_when_parameter_names_different()
        {
            using (var context = CreateContext())
            {
                var variableName = "test";
                var differentVariableName = "test";

                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                context.Set<Customer>().Where(e => e.CustomerID == "ALFKI")
                    .Where(e2 => InMemoryCheck.Check(variableName, e2.CustomerID) || true).Count();

                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                context.Set<Customer>().Where(e => e.CustomerID == "ALFKI")
                    .Where(e2 => InMemoryCheck.Check(differentVariableName, e2.CustomerID) || true).Count();
            }
        }

        [ConditionalFact] // See issue #12771
        public virtual void Can_convert_manually_build_expression_with_default()
        {
            using (var context = CreateContext())
            {
                var parameter = Expression.Parameter(typeof(Customer));
                var defaultExpression =
                    Expression.Lambda<Func<Customer, bool>>(
                        Expression.NotEqual(
                            Expression.Property(
                                parameter,
                                "CustomerID"),
                            Expression.Default(typeof(string))),
                        parameter);

                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                context.Set<Customer>().Where(defaultExpression).Count();

                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                context.Set<Customer>().Count(defaultExpression);
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private static class InMemoryCheck
        {
            // ReSharper disable once UnusedParameter.Local
            public static bool Check(string input1, string input2)
            {
                return false;
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Entity_equality_self(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    from c in cs
#pragma warning disable CS1718 // Comparison made to same variable
                        // ReSharper disable once EqualExpressionComparison
                    where c == c
#pragma warning restore CS1718 // Comparison made to same variable
                    select c.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Entity_equality_local(bool isAsync)
        {
            var local = new Customer
            {
                CustomerID = "ANATR"
            };

            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    from c in cs
                    where c == local
                    select c.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Entity_equality_local_composite_key(bool isAsync)
        {
            var local = new OrderDetail
            {
                OrderID = 10248,
                ProductID = 11
            };

            return AssertQuery<OrderDetail>(
                isAsync,
                odt =>
                    from od in odt
                    where od.Equals(local)
                    select od,
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Entity_equality_local_double_check(bool isAsync)
        {
            var local = new Customer
            {
                CustomerID = "ANATR"
            };

            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    from c in cs
                    where c == local && local == c
                    select c.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_with_entity_equality_local_on_both_sources(bool isAsync)
        {
            var local = new Customer
            {
                CustomerID = "ANATR"
            };

            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    (from c1 in cs
                     where c1 == local
                     select c1).Join(
                        from c2 in cs
                        where c2 == local
                        select c2, o => o, i => i, (o, i) => o).Select(e => e.CustomerID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Entity_equality_local_inline(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    from c in cs
                    where c == new Customer
                    {
                        CustomerID = "ANATR"
                    }
                    select c.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Entity_equality_local_inline_composite_key(bool isAsync)
            => AssertQuery<OrderDetail>(
                isAsync,
                odt =>
                    from od in odt
                    where od.Equals(new OrderDetail
                    {
                        OrderID = 10248,
                        ProductID = 11
                    })
                    select od,
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Entity_equality_null(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    from c in cs
                    where c == null
                    select c.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Entity_equality_null_composite_key(bool isAsync)
            => AssertQuery<OrderDetail>(
                isAsync,
                odt =>
                    from od in odt
                    where od == null
                    select od);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Entity_equality_not_null(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    from c in cs
                    where c != null
                    select c.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Entity_equality_not_null_composite_key(bool isAsync)
            => AssertQuery<OrderDetail>(
                isAsync,
                odt =>
                    from od in odt
                    where od != null
                    select od,
                entryCount: 2155);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Entity_equality_through_nested_anonymous_type_projection(bool isAsync)
            => AssertQuery<Order>(
                isAsync,
                o => o
                    .Select(x => new
                    {
                        CustomerInfo = new
                        {
                            x.Customer
                        }
                    })
                    .Where(x => x.CustomerInfo.Customer != null),
                entryCount: 89);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Entity_equality_through_DTO_projection(bool isAsync)
            => AssertQuery<Order>(
                isAsync,
                o => o
                    .Select(o => new CustomerWrapper { Customer = o.Customer })
                    .Where(x => x.Customer != null),
                entryCount: 89);

        private class CustomerWrapper
        {
            public Customer Customer { get; set; }
            public override bool Equals(object obj) => obj is CustomerWrapper other && other.Customer.Equals(Customer);
            public override int GetHashCode() => Customer.GetHashCode();
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Entity_equality_through_subquery(bool isAsync)
            => AssertQuery<Customer>(
                isAsync,
                cs =>
                    from c in cs
                    where c.Orders.FirstOrDefault() != null
                    select c.CustomerID);

        [ConditionalFact]
        public virtual void Entity_equality_through_subquery_composite_key()
        {
            Assert.Throws<InvalidOperationException>(() =>
                CreateContext().Orders
                    .Where(o => o.OrderDetails.FirstOrDefault() == new OrderDetail
                    {
                        OrderID = 10248,
                        ProductID = 11
                    })
                    .ToList());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Entity_equality_through_include(bool isAsync)
            => AssertQuery<Customer>(
                isAsync,
                cs =>
                    from c in cs.Include(c => c.Orders)
                    where c == null
                    select c.CustomerID);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Entity_equality_orderby(bool isAsync)
            => AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c),
                entryCount: 91,
                assertOrder: true);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Entity_equality_orderby_descending_composite_key(bool isAsync)
            => AssertQuery<OrderDetail>(
                isAsync,
                od => od.OrderByDescending(o => o),
                entryCount: 2155,
                assertOrder: true);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_conditional_simple(bool isAsync)
        {
            var c = Expression.Parameter(typeof(Customer));

            var predicate
                = Expression.Lambda<Func<Customer, bool>>(
                    Expression.Equal(
                        new NullConditionalExpression(c, Expression.Property(c, "CustomerID")),
                        Expression.Constant("ALFKI")),
                    c);

            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(predicate),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_conditional_deep(bool isAsync)
        {
            var c = Expression.Parameter(typeof(Customer));

            var nullConditionalExpression
                = new NullConditionalExpression(c, Expression.Property(c, "CustomerID"));

            nullConditionalExpression
                = new NullConditionalExpression(
                    nullConditionalExpression,
                    Expression.Property(nullConditionalExpression, "Length"));

            var predicate
                = Expression.Lambda<Func<Customer, bool>>(
                    Expression.Equal(
                        nullConditionalExpression,
                        Expression.Constant(5, typeof(int?))),
                    c);

            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(predicate),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Queryable_simple(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs,
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Queryable_simple_anonymous(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        c
                    }),
                e => e.c.CustomerID,
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Queryable_simple_anonymous_projection_subquery(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Take(91).Select(
                    c => new
                    {
                        c
                    }).Select(a => a.c.City));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Queryable_simple_anonymous_subquery(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        c
                    }).Take(91).Select(a => a.c),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Queryable_reprojection(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("Where<Customer>(    source: DbSet<Customer>,     predicate: (c) => c.IsLondon)"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertQuery<Customer>(
                            isAsync,
                            cs => cs.Where(c => c.IsLondon)
                                .Select(
                                    c => new Customer
                                    {
                                        CustomerID = "Foo", City = c.City
                                    })))).Message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Queryable_nested_simple(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    from c1 in (from c2 in (from c3 in cs select c3) select c2) select c1,
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Take_simple(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).Take(10),
                assertOrder: true,
                entryCount: 10);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Take_simple_parameterized(bool isAsync)
        {
            var take = 10;

            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).Take(take),
                assertOrder: true,
                entryCount: 10);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Take_simple_projection(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).Select(c => c.City).Take(10),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Take_subquery_projection(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).Take(2).Select(c => c.City),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).Skip(5),
                cs => cs.OrderBy(c => c.CustomerID, StringComparer.Ordinal).Skip(5),
                assertOrder: true,
                entryCount: 86);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_no_orderby(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Skip(5),
                entryCount: 86,
                elementAsserter: (_, __) =>
                {
                    /* non-deterministic */
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_orderby_const(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => true).Skip(5),
                entryCount: 86,
                elementAsserter: (_, __) =>
                {
                    /* non-deterministic */
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Take_Skip(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactName).Take(10).Skip(5),
                assertOrder: true,
                entryCount: 5);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_Skip(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Distinct().OrderBy(c => c.CustomerID).Skip(5),
                cs => cs.Distinct().OrderBy(c => c.CustomerID, StringComparer.Ordinal).Skip(5),
                assertOrder: true,
                entryCount: 86);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_Take(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactName).Skip(5).Take(10),
                assertOrder: true,
                entryCount: 10);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_Customers_Orders_Skip_Take(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    (from c in cs
                     join o in os on c.CustomerID equals o.CustomerID
                     orderby o.OrderID
                     select new
                     {
                         c.ContactName,
                         o.OrderID
                     }).Skip(10).Take(5),
                e => e.ContactName);
        }

        // issue #12574
        //[ConditionalTheory]
        //[MemberData(nameof(IsAsyncData))]
        public virtual Task Join_Customers_Orders_Skip_Take_followed_by_constant_projection(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    (from c in cs
                     join o in os on c.CustomerID equals o.CustomerID
                     orderby o.OrderID
                     select new
                     {
                         c.ContactName,
                         o.OrderID
                     }).Skip(10).Take(5).Select(e => "Foo"),
                e => e.ContactName);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_Customers_Orders_Projection_With_String_Concat_Skip_Take(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    (from c in cs
                     join o in os on c.CustomerID equals o.CustomerID
                     orderby o.OrderID
                     select new
                     {
                         Contact = c.ContactName + " " + c.ContactTitle,
                         o.OrderID
                     }).Skip(10).Take(5),
                e => e.Contact);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_Customers_Orders_Orders_Skip_Take_Same_Properties(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    (from o in os
                     join ca in cs on o.CustomerID equals ca.CustomerID
                     join cb in cs on o.CustomerID equals cb.CustomerID
                     orderby o.OrderID
                     select new
                     {
                         o.OrderID,
                         CustomerIDA = ca.CustomerID,
                         CustomerIDB = cb.CustomerID,
                         ContactNameA = ca.ContactName,
                         ContactNameB = cb.ContactName
                     }).Skip(10).Take(5),
                e => e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Ternary_should_not_evaluate_both_sides(bool isAsync)
        {
            Customer customer = null;
            bool hasData = !(customer is null);

            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        c.CustomerID,
                        Data1 = hasData ? customer.CustomerID : "none",
                        Data2 = customer != null ? customer.CustomerID : "none",
                        Data3 = !hasData ? "none" : customer.CustomerID
                    }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Null_Coalesce_Short_Circuit(bool isAsync)
        {
            List<int> values = null;
            bool? test = false;

            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Distinct().Select(c => new { Customer = c, Test = (test ?? values.Contains(1)) }),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_Skip_Take(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Distinct().OrderBy(c => c.ContactName).Skip(5).Take(10),
                assertOrder: true,
                entryCount: 10);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_Distinct(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactName).Skip(5).Distinct(),
                entryCount: 86);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_Take_Distinct(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactName).Skip(5).Take(10).Distinct(),
                entryCount: 10);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_Take_Any(bool isAsync)
        {
            return AssertAny<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactName).Skip(5).Take(10));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_Take_All(bool isAsync)
        {
            return AssertAll<Customer, Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).Skip(4).Take(7),
                predicate: p => p.CustomerID.StartsWith("B"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Take_All(bool isAsync)
        {
            return AssertAll<Customer, Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).Take(4),
                predicate: p => p.CustomerID.StartsWith("A"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_Take_Any_with_predicate(bool isAsync)
        {
            return AssertAny<Customer, Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).Skip(5).Take(7),
                predicate: p => p.CustomerID.StartsWith("C"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Take_Any_with_predicate(bool isAsync)
        {
            return AssertAny<Customer, Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).Take(5),
                predicate: p => p.CustomerID.StartsWith("B"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Take_Skip_Distinct(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactName).Take(10).Skip(5).Distinct(),
                entryCount: 5);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Take_Skip_Distinct_Caching(bool isAsync)
        {
            await AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactName).Take(10).Skip(5).Distinct(),
                entryCount: 5);

            await AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactName).Take(15).Skip(10).Distinct(),
                entryCount: 5);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Take_Distinct(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.OrderBy(o => o.OrderID).Take(5).Distinct(),
                entryCount: 5);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_Take(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Distinct().OrderBy(o => o.OrderID).Take(5),
                assertOrder: true,
                entryCount: 5);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_Take_Count(bool isAsync)
        {
            return AssertCount<Order>(
                isAsync,
                os => os.Distinct().Take(5));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Take_Distinct_Count(bool isAsync)
        {
            return AssertCount<Order>(
                isAsync,
                os => os.Take(5).Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Take_Where_Distinct_Count(bool isAsync)
        {
            return AssertCount<Order>(
                isAsync,
                os => os.Where(o => o.CustomerID == "FRANK").Take(5).Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Any_simple(bool isAsync)
        {
            return AssertAny<Customer>(
                isAsync,
                cs => cs);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Take_Count(bool isAsync)
        {
            return AssertCount<Order>(
                isAsync,
                os => os.OrderBy(o => o.OrderID).Take(5));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Take_OrderBy_Count(bool isAsync)
        {
            return AssertCount<Order>(
                isAsync,
                os => os.Take(5).OrderBy(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Any_predicate(bool isAsync)
        {
            return AssertAny<Customer, Customer>(
                isAsync,
                cs => cs,
                predicate: c => c.ContactName.StartsWith("A"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Any_nested_negated(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) => cs.Where(c => !os.Any(o => o.CustomerID.StartsWith("A"))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Any_nested_negated2(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) => cs.Where(
                    c => c.City != "London"
                         && !os.Any(o => o.CustomerID.StartsWith("A"))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Any_nested_negated3(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) => cs.Where(
                    c => !os.Any(o => o.CustomerID.StartsWith("A"))
                         && c.City != "London"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Any_nested(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) => cs.Where(c => os.Any(o => o.CustomerID.StartsWith("A"))),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Any_nested2(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) => cs.Where(c => c.City != "London" && os.Any(o => o.CustomerID.StartsWith("A"))),
                entryCount: 85);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Any_nested3(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) => cs.Where(c => os.Any(o => o.CustomerID.StartsWith("A")) && c.City != "London"),
                entryCount: 85);
        }

        [ConditionalFact]
        public virtual void Any_with_multiple_conditions_still_uses_exists()
        {
            using (var context = CreateContext())
            {
                var query = context.Customers
                    .Where(c => c.City == "London" && c.Orders.Any(o => o.EmployeeID == 1))
                    .ToList();

                Assert.Equal(4, query.Count);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task All_top_level(bool isAsync)
        {
            return AssertAll<Customer, Customer>(
                isAsync,
                cs => cs,
                predicate: c => c.ContactName.StartsWith("A"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task All_top_level_column(bool isAsync)
        {
            return AssertAll<Customer, Customer>(
                isAsync,
                cs => cs,
                predicate: c => c.ContactName.StartsWith(c.ContactName));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task All_top_level_subquery(bool isAsync)
        {
            return AssertSingleResult<Customer>(
                isAsync,
                syncQuery: cs => cs.All(c1 => cs.Any(c2 => cs.Any(c3 => c1.CustomerID == c3.CustomerID))),
                asyncQuery: cs => cs.AllAsync(c1 => cs.Any(c2 => cs.Any(c3 => c1.CustomerID == c3.CustomerID))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task All_top_level_subquery_ef_property(bool isAsync)
        {
            return AssertSingleResult<Customer>(
                isAsync,
                syncQuery: cs => cs.All(c1 => cs.Any(c2 => cs.Any(c3 => EF.Property<string>(c1, "CustomerID") == c3.CustomerID))),
                asyncQuery: cs => cs.AllAsync(c1 => cs.Any(c2 => cs.Any(c3 => EF.Property<string>(c1, "CustomerID") == c3.CustomerID))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task All_client(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("All<Customer>(    source: DbSet<Customer>,     predicate: (c) => c.IsLondon)"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertAll<Customer, Customer>(
                            isAsync,
                            cs => cs,
                            predicate: c => c.IsLondon))).Message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task All_client_and_server_top_level(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("All<Customer>(    source: DbSet<Customer>,     predicate: (c) => c.CustomerID != \"Foo\" && c.IsLondon)"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertAll<Customer, Customer>(
                            isAsync,
                            cs => cs,
                            predicate: c => c.CustomerID != "Foo" && c.IsLondon))).Message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task All_client_or_server_top_level(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("All<Customer>(    source: DbSet<Customer>,     predicate: (c) => c.CustomerID != \"Foo\" || c.IsLondon)"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertAll<Customer, Customer>(
                            isAsync,
                            cs => cs,
                            predicate: c => c.CustomerID != "Foo" || c.IsLondon))).Message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_when_arithmetic_expressions(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Select(
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
        public virtual Task Projection_when_arithmetic_mixed(bool isAsync)
        {
            return AssertQuery<Order, Employee>(
                isAsync,
                (os, es) =>
                    from o in os.OrderBy(o => o.OrderID).Take(10)
                    from e in es.OrderBy(e => e.EmployeeID).Take(5)
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Projection_when_arithmetic_mixed_subqueries(bool isAsync)
        {
            Assert.Equal(
                "Unsupported Binary operator type specified.",
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertQuery<Order, Employee>(
                            isAsync,
                            (os, es) =>
                                from o in os.OrderBy(o => o.OrderID).Take(3).Select(
                                    o2 => new
                                    {
                                        o2, Mod = o2.OrderID % 2
                                    })
                                from e in es.OrderBy(e => e.EmployeeID).Take(2).Select(
                                    e2 => new
                                    {
                                        e2, Square = e2.EmployeeID ^ 2
                                    })
                                select new
                                {
                                    Add = e.e2.EmployeeID + o.o2.OrderID,
                                    e.Square,
                                    e.e2,
                                    Literal = 42,
                                    o.o2,
                                    o.Mod
                                },
                            elementSorter: e => e.e2.EmployeeID + " " + e.o2.OrderID,
                            entryCount: 3))).Message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Take_with_single(bool isAsync)
        {
            return AssertSingle<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).Take(1),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Take_with_single_select_many(bool isAsync)
        {
            return AssertSingle<Customer, Order>(
                isAsync,
                (cs, os) =>
                    (from c in cs
                     from o in os
                     orderby c.CustomerID, o.OrderID
                     select new
                     {
                         c,
                         o
                     })
                    .Take(1)
                    .Cast<object>(),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Cast_results_to_object(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs.Cast<object>() select c, entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task First_client_predicate(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("Where<Customer>(    source: OrderBy<Customer, string>(        source: DbSet<Customer>,         keySelector: (c) => c.CustomerID),     predicate: (c) => c.IsLondon)"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertFirst<Customer, Customer>(
                            isAsync,
                            cs => cs.OrderBy(c => c.CustomerID),
                            predicate: c => c.IsLondon,
                            entryCount: 1))).Message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_select_many_or(bool isAsync)
        {
            return AssertQuery<Customer, Employee>(
                isAsync,
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == "London"
                          || e.City == "London"
                    select new
                    {
                        c,
                        e
                    },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 100);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_select_many_or2(bool isAsync)
        {
            return AssertQuery<Customer, Employee>(
                isAsync,
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == "London"
                          || c.City == "Berlin"
                    select new
                    {
                        c,
                        e
                    },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 16);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_select_many_or3(bool isAsync)
        {
            return AssertQuery<Customer, Employee>(
                isAsync,
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == "London"
                          || c.City == "Berlin"
                          || c.City == "Seattle"
                    select new
                    {
                        c,
                        e
                    },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 17);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_select_many_or4(bool isAsync)
        {
            return AssertQuery<Customer, Employee>(
                isAsync,
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == "London"
                          || c.City == "Berlin"
                          || c.City == "Seattle"
                          || c.City == "Lisboa"
                    select new
                    {
                        c,
                        e
                    },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 19);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_select_many_or_with_parameter(bool isAsync)
        {
            var london = "London";
            var lisboa = "Lisboa";

            return AssertQuery<Customer, Employee>(
                isAsync,
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == london
                          || c.City == "Berlin"
                          || c.City == "Seattle"
                          || c.City == lisboa
                    select new
                    {
                        c,
                        e
                    },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 19);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_anon(bool isAsync)
        {
            return AssertQuery<Employee, Order>(
                isAsync,
                (es, os) =>
                    from e in es.OrderBy(ee => ee.EmployeeID).Take(3).Select(
                        e => new
                        {
                            e
                        })
                    from o in os.OrderBy(oo => oo.OrderID).Take(5).Select(
                        o => new
                        {
                            o
                        })
                    where e.e.EmployeeID == o.o.EmployeeID
                    select new
                    {
                        e,
                        o
                    },
                entryCount: 2);
        }

        [ConditionalTheory(Skip = "Issue#16157")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_anon_nested(bool isAsync)
        {
            return AssertQuery<Employee, Order, Customer>(
                isAsync,
                (es, os, cs) =>
                    from t in (
                        from e in es.OrderBy(ee => ee.EmployeeID).Take(3).Select(
                            e => new
                            {
                                e
                            }).Where(e => e.e.City == "Seattle")
                        from o in os.OrderBy(oo => oo.OrderID).Take(5).Select(
                            o => new
                            {
                                o
                            })
                        select new
                        {
                            e,
                            o
                        })
                    from c in cs.Take(2).Select(
                        c => new
                        {
                            c
                        })
                    select new
                    {
                        t.e,
                        t.o,
                        c
                    },
                entryCount: 8);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_expression(bool isAsync)
        {
            return AssertQuery<Order, Order>(
                isAsync,
                (o1, o2) =>
                {
                    var firstOrder = o1.First();
                    Expression<Func<Order, bool>> expr = z => z.OrderID == firstOrder.OrderID;
                    return o1.Where(x => o2.Where(expr).Any());
                },
                entryCount: 830);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_expression_same_parametername(bool isAsync)
        {
            return AssertQuery<Order, Order>(
                isAsync,
                (o1, o2) =>
                {
                    var firstOrder = o1.OrderBy(o => o.OrderID).First();
                    Expression<Func<Order, bool>> expr = x => x.OrderID == firstOrder.OrderID;
                    return o1.Where(x => o2.Where(expr).Where(o => o.CustomerID == x.CustomerID).Any());
                },
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual void Select_DTO_distinct_translated_to_server()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Order>()
                    .Where(o => o.OrderID < 10300)
                    .Select(o => new OrderCountDTO())
                    .Distinct().ToList().OrderBy(e => e.Id).ToList();

                var expected = Fixture.QueryAsserter.ExpectedData.Set<Order>()
                    .Where(o => o.OrderID < 10300)
                    .Select(o => new OrderCountDTO())
                    .Distinct().ToList().OrderBy(e => e.Id).ToList();

                Assert.Equal(expected.Count, actual.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Id, actual[i].Id);
                    Assert.Equal(expected[i].Count, actual[i].Count);
                }
            }
        }

        [ConditionalFact]
        public virtual void Select_DTO_constructor_distinct_translated_to_server()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Order>()
                    .Where(o => o.OrderID < 10300)
                    .Select(o => new OrderCountDTO(o.CustomerID))
                    .Distinct().ToList().OrderBy(e => e.Id).ToList();

                var expected = Fixture.QueryAsserter.ExpectedData.Set<Order>()
                    .Where(o => o.OrderID < 10300)
                    .Select(o => new OrderCountDTO(o.CustomerID))
                    .Distinct().ToList().OrderBy(e => e.Id).ToList();

                Assert.Equal(expected.Count, actual.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Id, actual[i].Id);
                    Assert.Equal(expected[i].Count, actual[i].Count);
                }
            }
        }

        [ConditionalFact]
        public virtual void Select_DTO_constructor_distinct_with_navigation_translated_to_server()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Order>()
                    .Where(o => o.OrderID < 10300)
                    .Select(o => new OrderCountDTO(o.Customer.City))
                    .Distinct().ToList().OrderBy(e => e.Id).ToList();

                var expected = Fixture.QueryAsserter.ExpectedData.Set<Order>()
                    .Where(o => o.OrderID < 10300)
                    .Select(o => new OrderCountDTO(o.Customer.City))
                    .Distinct().ToList().OrderBy(e => e.Id).ToList();

                Assert.Equal(expected.Count, actual.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Id, actual[i].Id);
                    Assert.Equal(expected[i].Count, actual[i].Count);
                }
            }
        }

        [ConditionalFact]
        public virtual void Select_DTO_with_member_init_distinct_translated_to_server()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Order>()
                    .Where(o => o.OrderID < 10300)
                    .Select(
                        o => new OrderCountDTO
                        {
                            Id = o.CustomerID,
                            Count = o.OrderID
                        })
                    .Distinct().ToList().OrderBy(e => e.Count).ToList();

                var expected = Fixture.QueryAsserter.ExpectedData.Set<Order>()
                    .Where(o => o.OrderID < 10300)
                    .Select(
                        o => new OrderCountDTO
                        {
                            Id = o.CustomerID,
                            Count = o.OrderID
                        })
                    .Distinct().ToList().OrderBy(e => e.Count).ToList();

                Assert.Equal(expected.Count, actual.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Id, actual[i].Id);
                    Assert.Equal(expected[i].Count, actual[i].Count);
                }
            }
        }

        [ConditionalFact]
        public virtual void Select_nested_collection_count_using_DTO()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>()
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .Select(
                        c => new OrderCountDTO
                        {
                            Id = c.CustomerID,
                            Count = c.Orders.Count
                        })
                    .ToList().OrderBy(e => e.Id).ToList();

                var expected = Fixture.QueryAsserter.ExpectedData.Set<Customer>()
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .Select(
                        c => new OrderCountDTO
                        {
                            Id = c.CustomerID,
                            Count = c.Orders.Count
                        })
                    .ToList().OrderBy(e => e.Id).ToList();

                Assert.Equal(expected.Count, actual.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i], actual[i]);
                }
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_DTO_with_member_init_distinct_in_subquery_translated_to_server(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from o in os.Where(o => o.OrderID < 10300)
                        .Select(
                            o => new OrderCountDTO
                            {
                                Id = o.CustomerID,
                                Count = o.OrderID
                            })
                        .Distinct()
                    from c in cs.Where(c => c.CustomerID == o.Id)
                    select c,
                entryCount: 35);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_DTO_with_member_init_distinct_in_subquery_translated_to_server_2(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from o in os.Where(o => o.OrderID < 10300)
                        .Select(
                            o => new OrderCountDTO
                            {
                                Id = o.CustomerID,
                                Count = o.OrderID
                            })
                        .Distinct()
                    from c in cs.Where(c => o.Id == c.CustomerID)
                    select c,
                entryCount: 35);
        }

        [ConditionalFact]
        public virtual void Select_DTO_with_member_init_distinct_in_subquery_used_in_projection_translated_to_server()
        {
            using (var context = CreateContext())
            {
                var actual = (from c in context.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))
                              from o in context.Set<Order>().Where(o => o.OrderID < 10300)
                                  .Select(
                                      o => new OrderCountDTO
                                      {
                                          Id = o.CustomerID,
                                          Count = o.OrderID
                                      })
                                  .Distinct()
                              select new
                              {
                                  c,
                                  o
                              }).ToList().OrderBy(e => e.c.CustomerID + " " + e.o.Count).ToList();

                var expected = (from c in Fixture.QueryAsserter.ExpectedData.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))
                                from o in Fixture.QueryAsserter.ExpectedData.Set<Order>().Where(o => o.OrderID < 10300)
                                    .Select(
                                        o => new OrderCountDTO
                                        {
                                            Id = o.CustomerID,
                                            Count = o.OrderID
                                        })
                                    .Distinct()
                                select new
                                {
                                    c,
                                    o
                                }).ToList().OrderBy(e => e.c.CustomerID + " " + e.o.Count).ToList();

                Assert.Equal(expected.Count, actual.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].c.CustomerID, actual[i].c.CustomerID);
                    Assert.Equal(expected[i].o.Id, actual[i].o.Id);
                    Assert.Equal(expected[i].o.Count, actual[i].o.Count);
                }
            }
        }

        private class OrderCountDTO
        {
            public string Id { get; set; }
            public int Count { get; set; }

            public OrderCountDTO()
            {
            }

            public OrderCountDTO(string id)
            {
                Id = id;
                Count = 0;
            }

            public override bool Equals(object obj)
            {
                if (obj is null)
                {
                    return false;
                }

                return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((OrderCountDTO)obj);
            }

            private bool Equals(OrderCountDTO other)
            {
                return string.Equals(Id, other.Id) && Count == other.Count;
            }

            public override int GetHashCode() => HashCode.Combine(Id, Count);
        }

        [ConditionalTheory(Skip = "Issue#16314")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_correlated_subquery_projection(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from c in cs.OrderBy(cc => cc.CustomerID).Take(3)
                    select os.Where(o => o.CustomerID == c.CustomerID),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Order>(o => o.OrderID));
        }

        [ConditionalTheory(Skip = "Issue#16314")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_correlated_subquery_filtered(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from c in cs
                    where c.CustomerID.StartsWith("A")
                    orderby c.CustomerID
                    select os.Where(o => o.CustomerID == c.CustomerID),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Order>(o => o.OrderID));
        }

        [ConditionalTheory(Skip = "Issue #17241")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_correlated_subquery_ordered(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from c in cs.OrderBy(c => c.CustomerID).Take(3)
                    select os.OrderBy(o => o.OrderID).ThenBy(o => c.CustomerID).Skip(100).Take(2),
                elementSorter: CollectionSorter<Order>(),
                elementAsserter: CollectionAsserter<Order>());
        }

        // TODO: Re-linq parser
        // [ConditionalFact]
        // public virtual Task Select_nested_ordered_enumerable_collection()
        // {
        //     AssertQuery<Customer>(cs =>
        //         cs.Select(c => cs.AsEnumerable().OrderBy(c2 => c2.CustomerID)),
        //         assertOrder: true);
        // }

        [ConditionalTheory(Skip = "Issue#16314")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_nested_collection_in_anonymous_type(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from c in cs
                    where c.CustomerID == "ALFKI"
                    select new
                    {
                        CustomerId = c.CustomerID,
                        OrderIds
                            = os.Where(
                                    o => o.CustomerID == c.CustomerID
                                         && o.OrderDate.Value.Year == 1997)
                                .Select(o => o.OrderID)
                                .OrderBy(o => o),
                        Customer = c
                    },
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.CustomerId, a.CustomerId);
                    Assert.Equal((IEnumerable<int>)e.OrderIds, (IEnumerable<int>)a.OrderIds);
                    Assert.Equal(e.Customer, a.Customer);
                },
                entryCount: 1);
        }

        [ConditionalTheory(Skip = "Issue#16314")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_subquery_recursive_trivial(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e1 in es
                    select (from e2 in es
                            select (from e3 in es
                                    orderby e3.EmployeeID
                                    select e3)),
                e => ((IEnumerable<IEnumerable<Employee>>)e).Count(),
                elementAsserter: (e, a) =>
                {
                    var expected = ((IEnumerable<IEnumerable<Employee>>)e).SelectMany(i => i).ToList();
                    var actual = ((IEnumerable<IEnumerable<Employee>>)e).SelectMany(i => i).ToList();

                    Assert.Equal(expected, actual);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_on_bool(bool isAsync)
        {
            return AssertQuery<Product, Product>(
                isAsync,
                (pr, pr2) =>
                    from p in pr
                    where pr2.Select(p2 => p2.ProductName).Contains("Chai")
                    select p,
                entryCount: 77);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_on_collection(bool isAsync)
        {
            return AssertQuery<Product, OrderDetail>(
                isAsync,
                (pr, od) =>
                    pr.Where(
                        p => od
                            .Where(o => o.ProductID == p.ProductID)
                            .Select(odd => odd.Quantity).Contains<short>(5)),
                entryCount: 43);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_query_composition(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e1 in es
                    where e1.FirstName == es.OrderBy(e => e.EmployeeID).FirstOrDefault().FirstName
                    select e1,
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_query_composition_is_null(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e1 in es.OrderBy(e => e.EmployeeID).Take(3)
                    where es.SingleOrDefault(e2 => e2.EmployeeID == e1.ReportsTo) == null
                    select e1,
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_query_composition_is_not_null(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e1 in es.OrderBy(e => e.EmployeeID).Skip(4).Take(3)
                    where es.SingleOrDefault(e2 => e2.EmployeeID == e1.ReportsTo) != null
                    select e1,
                entryCount: 3);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_query_composition_entity_equality_one_element_SingleOrDefault(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e1 in es
                    where es.SingleOrDefault(e2 => e2.EmployeeID == e1.ReportsTo) == new Employee()
                    select e1,
                es =>
                    from e1 in es
                    where es.FirstOrDefault(e2 => e2.EmployeeID == e1.ReportsTo) == new Employee()
                    select e1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_query_composition_entity_equality_one_element_Single(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e1 in es
                    where es.Single(e2 => e2.EmployeeID == e1.ReportsTo) == new Employee()
                    select e1,
                es =>
                    from e1 in es
                    where es.FirstOrDefault(e2 => e2.EmployeeID == e1.ReportsTo) == new Employee()
                    select e1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_query_composition_entity_equality_one_element_FirstOrDefault(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e1 in es
                    where es.FirstOrDefault(e2 => e2.EmployeeID == e1.ReportsTo) == new Employee()
                    select e1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_query_composition_entity_equality_one_element_First(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e1 in es
                    where es.First(e2 => e2.EmployeeID == e1.ReportsTo) == new Employee()
                    select e1,
                es =>
                    from e1 in es
                    where es.FirstOrDefault(e2 => e2.EmployeeID == e1.ReportsTo) == new Employee()
                    select e1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_query_composition_entity_equality_no_elements_SingleOrDefault(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e1 in es
                    where es.SingleOrDefault(e2 => e2.EmployeeID == 42) == new Employee()
                    select e1,
                es =>
                    from e1 in es
                    where es.FirstOrDefault(e2 => e2.EmployeeID == 42) == new Employee()
                    select e1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_query_composition_entity_equality_no_elements_Single(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e1 in es
                    where es.Single(e2 => e2.EmployeeID == 42) == new Employee()
                    select e1,
                es =>
                    from e1 in es
                    where es.FirstOrDefault(e2 => e2.EmployeeID == 42) == new Employee()
                    select e1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_query_composition_entity_equality_no_elements_FirstOrDefault(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e1 in es
                    where es.FirstOrDefault(e2 => e2.EmployeeID == 42) == new Employee()
                    select e1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_query_composition_entity_equality_no_elements_First(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e1 in es
                    where es.First(e2 => e2.EmployeeID == 42) == new Employee()
                    select e1,
                es =>
                    from e1 in es
                    where es.FirstOrDefault(e2 => e2.EmployeeID == 42) == new Employee()
                    select e1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e1 in es
                    where es.SingleOrDefault(e2 => e2.EmployeeID != e1.ReportsTo) == new Employee()
                    select e1,
                es =>
                    from e1 in es
                    where es.FirstOrDefault(e2 => e2.EmployeeID != e1.ReportsTo) == new Employee()
                    select e1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_query_composition_entity_equality_multiple_elements_Single(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e1 in es
                    where es.Single(e2 => e2.EmployeeID != e1.ReportsTo) == new Employee()
                    select e1,
                es =>
                    from e1 in es
                    where es.FirstOrDefault(e2 => e2.EmployeeID != e1.ReportsTo) == new Employee()
                    select e1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_query_composition_entity_equality_multiple_elements_FirstOrDefault(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e1 in es
                    where es.FirstOrDefault(e2 => e2.EmployeeID != e1.ReportsTo) == new Employee()
                    select e1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_query_composition_entity_equality_multiple_elements_First(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e1 in es
                    where es.First(e2 => e2.EmployeeID != e1.ReportsTo) == new Employee()
                    select e1,
                es =>
                    from e1 in es
                    where es.FirstOrDefault(e2 => e2.EmployeeID != e1.ReportsTo) == new Employee()
                    select e1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_query_composition2(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e1 in es.Take(3)
                    where e1.FirstName
                          == (from e2 in es.OrderBy(e => e.EmployeeID)
                              select new
                              {
                                  Foo = e2
                              })
                          .First().Foo.FirstName
                    select e1,
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_query_composition2_FirstOrDefault(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e1 in es.Take(3)
                    where e1.FirstName
                          == (from e2 in es.OrderBy(e => e.EmployeeID)
                              select e2)
                          .FirstOrDefault().FirstName
                    select e1,
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_query_composition2_FirstOrDefault_with_anonymous(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e1 in es.Take(3)
                    where e1.FirstName
                          == (from e2 in es.OrderBy(e => e.EmployeeID)
                              select new
                              {
                                  Foo = e2
                              })
                          .FirstOrDefault().Foo.FirstName
                    select e1,
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_query_composition3(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("Where<Customer>(    source: OrderBy<Customer, string>(        source: DbSet<Customer>,         keySelector: (c0) => c0.CustomerID),     predicate: (c0) => c0.IsLondon)"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertQuery<Customer>(
                            isAsync,
                            cs =>
                                from c1 in cs
                                where c1.City == cs.OrderBy(c => c.CustomerID).First(c => c.IsLondon).City
                                select c1,
                            entryCount: 6))).Message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_query_composition4(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("OrderBy<Customer, bool>(    source: DbSet<Customer>,     keySelector: (c1) => c1.IsLondon)"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertQuery<Customer>(
                            isAsync,
                            cs =>
                                from c1 in cs.OrderBy(c => c.CustomerID).Take(2)
                                where c1.City == (from c2 in cs.OrderBy(c => c.CustomerID)
                                                  from c3 in cs.OrderBy(c => c.IsLondon).ThenBy(c => c.CustomerID)
                                                  select new
                                                  {
                                                      c3
                                                  }).First().c3.City
                                select c1,
                            entryCount: 1))).Message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_query_composition5(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed(
                    "Where<Customer>(    source: DbSet<Customer>,     predicate: (c) => c.IsLondon == First<bool>(Select<Customer, bool>(        source: OrderBy<Customer, string>(            source: DbSet<Customer>,             keySelector: (c0) => c0.CustomerID),         selector: (c0) => c0.IsLondon)))"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertQuery<Customer>(
                            isAsync,
                            cs =>
                                from c1 in cs
                                where c1.IsLondon == cs.OrderBy(c => c.CustomerID).First().IsLondon
                                select c1,
                            entryCount: 85))).Message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_query_composition6(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("Where<Customer>(    source: DbSet<Customer>,     predicate: (c) => c.IsLondon == First<bool>(Select<Customer, bool>(        source: OrderBy<Customer, string>(            source: DbSet<Customer>,             keySelector: (c0) => c0.CustomerID),         selector: (c0) => c0.IsLondon)))"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertQuery<Customer>(
                            isAsync,
                            cs =>
                                from c1 in cs
                                where c1.IsLondon
                                      == cs.OrderBy(c => c.CustomerID)
                                          .Select(
                                              c => new
                                              {
                                                  Foo = c
                                              })
                                          .First().Foo.IsLondon
                                select c1,
                            entryCount: 85))).Message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_recursive_trivial(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_scalar_primitive(bool isAsync)
        {
            return AssertQueryScalar<Employee>(
                isAsync,
                es => es.Select(e => e.EmployeeID).OrderBy(i => i),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task SelectMany_mixed(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.QueryFailed("(e1) => string[] { \"a\", \"b\", }", "NavigationExpandingExpressionVisitor"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertQuery<Employee, Customer>(
                            isAsync,
                            (es, cs) =>
                                from e1 in es.OrderBy(e => e.EmployeeID).Take(2)
                                from s in new[]
                                {
                                    "a", "b"
                                }
                                from c in cs.OrderBy(c => c.CustomerID).Take(2)
                                select new
                                {
                                    e1, s, c
                                },
                            e => e.e1.EmployeeID + " " + e.c.CustomerID,
                            entryCount: 4))).Message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_simple1(bool isAsync)
        {
            return AssertQuery<Employee, Customer>(
                isAsync,
                (es, cs) =>
                    from e in es
                    from c in cs
                    select new
                    {
                        c,
                        e
                    },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 100);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_simple_subquery(bool isAsync)
        {
            return AssertQuery<Employee, Customer>(
                isAsync,
                (es, cs) =>
                    from e in es.Take(9)
                    from c in cs
                    select new
                    {
                        c,
                        e
                    },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 100);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_simple2(bool isAsync)
        {
            return AssertQuery<Employee, Customer>(
                isAsync,
                (es, cs) =>
                    from e1 in es
                    from c in cs
                    from e2 in es
                    select new
                    {
                        e1,
                        c,
                        e2.FirstName
                    },
                e => e.e1.EmployeeID + " " + e.c.CustomerID + " " + e.FirstName,
                entryCount: 100);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_entity_deep(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e1 in es
                    from e2 in es
                    from e3 in es
                    from e4 in es
                    select new
                    {
                        e2,
                        e3,
                        e1,
                        e4
                    },
                e => e.e2.EmployeeID + " " + e.e3.EmployeeID + " " + e.e1.EmployeeID + e.e4.EmployeeID,
                entryCount: 9);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_projection1(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e1 in es
                    from e2 in es
                    select new
                    {
                        e1.City,
                        e2.Country
                    },
                e => e.City + " " + e.Country);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_projection2(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e1 in es
                    from e2 in es
                    from e3 in es
                    select new
                    {
                        e1.City,
                        e2.Country,
                        e3.FirstName
                    },
                e => e.City + " " + e.Country + " " + e.FirstName);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_nested_simple(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    from c in cs
                    from c1 in
                        (from c2 in (from c3 in cs select c3) select c2)
                    orderby c1.CustomerID
                    select c1,
                cs => cs.SelectMany(
                        c => (from c2 in (from c3 in cs select c3) select c2),
                        (c, c1) => new
                        {
                            c,
                            c1
                        }).OrderBy(t => t.c1.CustomerID, StringComparer.Ordinal)
                    .Select(t => t.c1),
                assertOrder: true,
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_correlated_simple(bool isAsync)
        {
            return AssertQuery<Customer, Employee>(
                isAsync,
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == e.City
                    orderby c.CustomerID, e.EmployeeID
                    select new
                    {
                        c,
                        e
                    },
                assertOrder: true,
                entryCount: 15);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_correlated_subquery_simple(bool isAsync)
        {
            return AssertQuery<Customer, Employee>(
                isAsync,
                (cs, es) =>
                    from c in cs
                    from e in es.Where(e => e.City == c.City)
                    orderby c.CustomerID, e.EmployeeID
                    select new
                    {
                        c,
                        e
                    },
                assertOrder: true,
                entryCount: 15);
        }

        [ConditionalTheory(Skip = "Issue #17240")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_correlated_subquery_hard(bool isAsync)
        {
            return AssertQuery<Customer, Employee>(
                isAsync,
                (cs, es) =>
                    from c1 in
                        (from c2 in cs.Take(91) select c2.City).Distinct()
                    from e1 in
                        (from e2 in es
                         where c1 == e2.City
                         select new
                         {
                             e2.City,
                             c1
                         }).Take(9)
                    from e2 in
                        (from e3 in es where e1.City == e3.City select c1).Take(9)
                    select new
                    {
                        c1,
                        e1
                    },
                e => e.c1 + " " + e.e1.City + " " + e.e1.c1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_cartesian_product_with_ordering(bool isAsync)
        {
            return AssertQuery<Customer, Employee>(
                isAsync,
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == e.City
                    orderby e.City, c.CustomerID descending
                    select new
                    {
                        c,
                        e.City
                    },
                assertOrder: true,
                entryCount: 8);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_primitive(bool isAsync)
        {
            return AssertQueryScalar<Employee>(
                isAsync,
                es => from e1 in es
                      from i in es.Select(e2 => e2.EmployeeID)
                      select i);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_primitive_select_subquery(bool isAsync)
        {
            return AssertQueryScalar<Employee>(
                isAsync,
                es => from e1 in es
                      from i in es.Select(e2 => e2.EmployeeID)
                      select es.Any());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_Where_Count(bool isAsync)
        {
            return AssertCount<Customer, Order>(
                isAsync,
                (cs, os) =>
                    (from c in cs
                     join o in os on c.CustomerID equals o.CustomerID
                     where c.CustomerID == "ALFKI"
                     select c));
        }

        [ConditionalTheory(Skip = "TaskItem#6")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_Join_Any(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID == "ALFKI" && c.Orders.Any(o => o.OrderDate == new DateTime(2008, 10, 24))));
        }

        [ConditionalTheory(Skip = "TaskItem#6")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_Join_Exists(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID == "ALFKI" && c.Orders.Exists(o => o.OrderDate == new DateTime(2008, 10, 24))));
        }

        [ConditionalTheory(Skip = "TaskItem#6")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_Join_Exists_Inequality(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID == "ALFKI" && c.Orders.Exists(o => o.OrderDate != new DateTime(2008, 10, 24))),
                entryCount: 1);
        }

        [ConditionalTheory(Skip = "TaskItem#6")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_Join_Exists_Constant(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID == "ALFKI" && c.Orders.Exists(o => false)));
        }

        [ConditionalTheory(Skip = "TaskItem#6")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_Join_Not_Exists(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID == "ALFKI" && !c.Orders.Exists(o => false)),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_joins_Where_Order_Any(bool isAsync)
        {
            return AssertAny<Customer, Order, OrderDetail>(
                isAsync,
                (cs, os, ods) =>
                    cs.Join(
                            os, c => c.CustomerID, o => o.CustomerID, (cr, or) => new
                            {
                                cr,
                                or
                            })
                        .Join(
                            ods, e => e.or.OrderID, od => od.OrderID, (e, od) => new
                            {
                                e.cr,
                                e.or,
                                od
                            })
                        .Where(r => r.cr.City == "London").OrderBy(r => r.cr.CustomerID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_OrderBy_Count(bool isAsync)
        {
            return AssertCount<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from c in cs
                    join o in os on c.CustomerID equals o.CustomerID
                    orderby c.CustomerID
                    select c);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_join_select(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    (from c in cs
                     where c.CustomerID == "ALFKI"
                     join o in os on c.CustomerID equals o.CustomerID
                     select c),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_orderby_join_select(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    (from c in cs
                     where c.CustomerID != "ALFKI"
                     orderby c.CustomerID
                     join o in os on c.CustomerID equals o.CustomerID
                     select c),
                entryCount: 88);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_join_orderby_join_select(bool isAsync)
        {
            return AssertQuery<Customer, Order, OrderDetail>(
                isAsync,
                (cs, os, ods) =>
                    (from c in cs
                     where c.CustomerID != "ALFKI"
                     join o in os on c.CustomerID equals o.CustomerID
                     orderby c.CustomerID
                     join od in ods on o.OrderID equals od.OrderID
                     select c),
                entryCount: 88);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_select_many(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    (from c in cs
                     where c.CustomerID == "ALFKI"
                     from o in os
                     select c),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_orderby_select_many(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    (from c in cs
                     where c.CustomerID == "ALFKI"
                     orderby c.CustomerID
                     from o in os
                     select c),
                entryCount: 1);
        }

        private class Foo
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string Bar { get; set; }
        }

        protected const uint NonExistentID = uint.MaxValue;

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Default_if_empty_top_level(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e in es.Where(c => c.EmployeeID == NonExistentID).DefaultIfEmpty()
                    select e);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_with_default_if_empty_on_both_sources(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    (from e in es.Where(c => c.EmployeeID == NonExistentID).DefaultIfEmpty()
                     select e).Join(
                        from e in es.Where(c => c.EmployeeID == NonExistentID).DefaultIfEmpty()
                        select e, o => o, i => i, (o, i) => o));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Default_if_empty_top_level_followed_by_projecting_constant(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e in es.Where(c => c.EmployeeID == NonExistentID).DefaultIfEmpty()
                    select "Foo");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Default_if_empty_top_level_arg(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.QueryFailed("DefaultIfEmpty<Employee>(    source: Where<Employee>(        source: DbSet<Employee>,         predicate: (c) => c.EmployeeID == 4294967295),     defaultValue: (Unhandled parameter: __p_0))", "NavigationExpandingExpressionVisitor"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertQuery<Employee>(
                            isAsync,
                            es =>
                                from e in es.Where(c => c.EmployeeID == NonExistentID).DefaultIfEmpty(new Employee())
                                select e,
                            entryCount: 1))).Message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Default_if_empty_top_level_arg_followed_by_projecting_constant(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.QueryFailed("DefaultIfEmpty<Employee>(    source: Where<Employee>(        source: DbSet<Employee>,         predicate: (c) => c.EmployeeID == 4294967295),     defaultValue: (Unhandled parameter: __p_0))", "NavigationExpandingExpressionVisitor"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertQueryScalar<Employee>(
                            isAsync,
                            es =>
                                from e in es.Where(c => c.EmployeeID == NonExistentID).DefaultIfEmpty(new Employee())
                                select 42))).Message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Default_if_empty_top_level_positive(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e in es.Where(c => c.EmployeeID > 0).DefaultIfEmpty()
                    select e,
                entryCount: 9);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Default_if_empty_top_level_projection(bool isAsync)
        {
            return AssertQueryScalar<Employee>(
                isAsync,
                es =>
                    from e in es.Where(e => e.EmployeeID == NonExistentID).Select(e => e.EmployeeID).DefaultIfEmpty()
                    select e);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_customer_orders(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from c in cs
                    from o in os
                    where c.CustomerID == o.CustomerID
                    select new
                    {
                        c.ContactName,
                        o.OrderID
                    },
                e => e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_Count(bool isAsync)
        {
            return AssertCount<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from c in cs
                    from o in os
                    select c.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_LongCount(bool isAsync)
        {
            return AssertLongCount<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from c in cs
                    from o in os
                    select c.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_OrderBy_ThenBy_Any(bool isAsync)
        {
            return AssertAny<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from c in cs
                    from o in os
                    orderby c.CustomerID, c.City
                    select c);
        }

        // TODO: Composite keys, slow..

        //        [ConditionalFact]
        //        public virtual Task Multiple_joins_with_join_conditions_in_where()
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
        //        [ConditionalFact]
        //
        //        public virtual Task TestMultipleJoinsWithMissingJoinCondition()
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID),
                cs => cs.OrderBy(c => c.CustomerID, StringComparer.Ordinal),
                assertOrder: true,
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_true(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => true).Select(c => c),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_integer(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => 3).Select(c => c),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_parameter(bool isAsync)
        {
            var param = 5;
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => param).Select(c => c),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_anon(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        c.CustomerID
                    }).OrderBy(a => a.CustomerID),
                cs => cs.Select(
                    c => new
                    {
                        c.CustomerID
                    }).OrderBy(a => a.CustomerID, StringComparer.Ordinal),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_anon2(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        c
                    }).OrderBy(a => a.c.CustomerID),
                cs => cs.Select(
                    c => new
                    {
                        c
                    }).OrderBy(a => a.c.CustomerID, StringComparer.Ordinal),
                assertOrder: true,
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task OrderBy_client_mixed(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("OrderBy<Customer, bool>(    source: DbSet<Customer>,     keySelector: (c) => c.IsLondon)"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertQuery<Customer>(
                            isAsync,
                            cs => cs.OrderBy(c => c.IsLondon).ThenBy(c => c.CompanyName),
                            assertOrder: true,
                            entryCount: 91))).Message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task OrderBy_multiple_queries(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("Join<Customer, Order, Foo, TransparentIdentifier<Customer, Order>>(    outer: DbSet<Customer>,     inner: DbSet<Order>,     outerKeySelector: (c) => new Foo{ Bar = c.CustomerID }    ,     innerKeySelector: (o) => new Foo{ Bar = o.CustomerID }    ,     resultSelector: (c, o) => new TransparentIdentifier<Customer, Order>(        Outer = c,         Inner = o    ))"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertQuery<Customer, Order>(
                            isAsync,
                            (cs, os) =>
                                from c in cs
                                join o in os on new Foo
                                {
                                    Bar = c.CustomerID
                                } equals new Foo
                                {
                                    Bar = o.CustomerID
                                }
                                orderby c.IsLondon, o.OrderDate
                                select new
                                {
                                    c, o
                                }))).Message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_shadow(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es => es.OrderBy(e => EF.Property<string>(e, "Title")).ThenBy(e => e.EmployeeID),
                assertOrder: true,
                entryCount: 9);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_ThenBy_predicate(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.City == "London")
                    .OrderBy(c => c.City)
                    .ThenBy(c => c.CustomerID),
                assertOrder: true,
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_correlated_subquery1(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs
                      where c.CustomerID.StartsWith("A")
                      orderby cs.Any(c2 => c2.CustomerID == c.CustomerID), c.CustomerID
                      select c,
                assertOrder: true,
                entryCount: 4);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_correlated_subquery2(bool isAsync)
        {
            return AssertQuery<Order, Customer>(
                isAsync,
                (os, cs) => os.Where(
                    o => o.OrderID <= 10250
                         && cs.OrderBy(
                                 c => cs.Any(
                                     c2 => c2.CustomerID == "ALFKI"))
                             .FirstOrDefault().City != "Nowhere"),
                entryCount: 3);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Select(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID)
                    .Select(c => c.ContactName),
                cs => cs.OrderBy(c => c.CustomerID, StringComparer.Ordinal)
                    .Select(c => c.ContactName),
                assertOrder: true);
        }

        [ConditionalTheory(Skip = "OrderByOrderBy should ignore inner ordering")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_multiple(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    cs.OrderBy(c => c.CustomerID)
                        // ReSharper disable once MultipleOrderBy
                        .OrderBy(c => c.Country)
                        .Select(c => c.City),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_ThenBy(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID)
                    .ThenBy(c => c.Country)
                    .Select(c => c.City),
                cs => cs.OrderBy(c => c.CustomerID, StringComparer.Ordinal)
                    .ThenBy(c => c.Country, StringComparer.Ordinal)
                    .Select(c => c.City),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderByDescending(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderByDescending(c => c.CustomerID).Select(c => c.City),
                cs => cs.OrderByDescending(c => c.CustomerID, StringComparer.Ordinal).Select(c => c.City),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderByDescending_ThenBy(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderByDescending(c => c.CustomerID)
                    .ThenBy(c => c.Country)
                    .Select(c => c.City),
                cs => cs.OrderByDescending(c => c.CustomerID, StringComparer.Ordinal)
                    .ThenBy(c => c.Country, StringComparer.Ordinal)
                    .Select(c => c.City),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderByDescending_ThenByDescending(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderByDescending(c => c.CustomerID)
                    .ThenByDescending(c => c.Country)
                    .Select(c => c.City),
                cs => cs.OrderByDescending(c => c.CustomerID, StringComparer.Ordinal)
                    .ThenByDescending(c => c.Country, StringComparer.Ordinal)
                    .Select(c => c.City),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_ThenBy_Any(bool isAsync)
        {
            return AssertAny<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).ThenBy(c => c.ContactName));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Join(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from c in cs.OrderBy(c => c.CustomerID)
                    join o in os.OrderBy(o => o.OrderID) on c.CustomerID equals o.CustomerID
                    select new
                    {
                        c.CustomerID,
                        o.OrderID
                    },
                assertOrder: false);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_SelectMany(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from c in cs.OrderBy(c => c.CustomerID)
                    from o in os.OrderBy(o => o.OrderID).Take(3)
                    where c.CustomerID == o.CustomerID
                    select new
                    {
                        c.ContactName,
                        o.OrderID
                    },
                (cs, os) =>
                    cs.OrderBy(c => c.CustomerID, StringComparer.Ordinal)
                        .SelectMany(
                            _ => os.OrderBy(o => o.OrderID).Take(3),
                            (c, o) => new
                            {
                                c,
                                o
                            }).Where(t => t.c.CustomerID == t.o.CustomerID)
                        .Select(
                            t => new
                            {
                                t.c.ContactName,
                                t.o.OrderID
                            }),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Let_any_subquery_anonymous(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from c in cs
                    let hasOrders = os.Any(o => o.CustomerID == c.CustomerID)
                    where c.CustomerID.StartsWith("A")
                    orderby c.CustomerID
                    select new
                    {
                        c,
                        hasOrders
                    },
                assertOrder: true,
                entryCount: 4);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_arithmetic(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es => es.OrderBy(e => e.EmployeeID - e.EmployeeID).Select(e => e),
                entryCount: 9);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_condition_comparison(bool isAsync)
        {
            return AssertQuery<Product>(
                isAsync,
                ps => ps.OrderBy(p => p.UnitsInStock > 0).ThenBy(p => p.ProductID),
                assertOrder: true,
                entryCount: 77);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_ternary_conditions(bool isAsync)
        {
            return AssertQuery<Product>(
                isAsync,
                ps => ps.OrderBy(p => p.UnitsInStock > 10 ? p.ProductID > 40 : p.ProductID <= 40).ThenBy(p => p.ProductID),
                assertOrder: true,
                entryCount: 77);
        }

        [ConditionalFact]
        public virtual void OrderBy_any()
        {
            using (var context = CreateContext())
            {
                var query = context.Customers.OrderBy(p => p.Orders.Any(o => o.OrderID > 11000)).ThenBy(p => p.CustomerID).ToList();

                Assert.Equal(91, query.Count);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_Joined(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from c in cs
                    from o in os.Where(o => o.CustomerID == c.CustomerID)
                    select new
                    {
                        c.ContactName,
                        o.OrderDate
                    },
                e => e.ContactName + " " + e.OrderDate);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_Joined_DefaultIfEmpty(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from c in cs
                    from o in os.Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                    select new
                    {
                        c.ContactName,
                        o
                    },
                e => e.ContactName + " " + e.o?.OrderID,
                entryCount: 830);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_Joined_Take(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from c in cs
                    from o in os.Where(o => o.CustomerID == c.CustomerID).Take(4)
                    select new
                    {
                        c.ContactName,
                        o
                    },
                e => e.o.OrderID,
                entryCount: 342);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_Joined_DefaultIfEmpty2(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from c in cs
                    from o in os.Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                    select o,
                e => e?.OrderID,
                entryCount: 830);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_many_cross_join_same_collection(bool isAsync)
        {
            return AssertQuery<Customer, Customer>(
                isAsync,
                (cs1, cs2) => cs1.SelectMany(c => cs2),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_null_coalesce_operator(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                customer => customer
                    .OrderBy(c => c.Region ?? "ZZ").ThenBy(c => c.CustomerID),
                assertOrder: true,
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_null_coalesce_operator(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        c.CustomerID,
                        c.CompanyName,
                        Region = c.Region ?? "ZZ"
                    }).OrderBy(o => o.Region).ThenBy(o => o.CustomerID),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_conditional_operator(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                customer => customer
                    // ReSharper disable once ConvertConditionalTernaryToNullCoalescing
                    // ReSharper disable once MergeConditionalExpression
#pragma warning disable IDE0029 // Use coalesce expression
                    .OrderBy(c => c.Region == null ? "ZZ" : c.Region).ThenBy(c => c.CustomerID),
#pragma warning restore IDE0029 // Use coalesce expression
                assertOrder: true,
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_conditional_operator_where_condition_false(bool isAsync)
        {
            var fakeCustomer = new Customer();
            return AssertQuery<Customer>(
                isAsync,
                customer => customer
                    .OrderBy(c => fakeCustomer.City == "London" ? "ZZ" : c.City)
                    .Select(c => c),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_comparison_operator(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                customer => customer
                    // ReSharper disable once ConvertConditionalTernaryToNullCoalescing
                    .OrderBy(c => c.Region == "ASK").Select(c => c),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_null_coalesce_operator(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        c.CustomerID,
                        c.CompanyName,
                        Region = c.Region ?? "ZZ"
                    }),
                e => e.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filter_coalesce_operator(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                customer => customer
                    .Where(c => (c.CompanyName ?? c.ContactName) == "The Big Cheese"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Take_skip_null_coalesce_operator(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.Region ?? "ZZ").Take(10).Skip(5).Distinct(),
                entryCount: 5);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_take_null_coalesce_operator(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        c.CustomerID,
                        c.CompanyName,
                        Region = c.Region ?? "ZZ"
                    }).OrderBy(c => c.Region).Take(5),
                e => e.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_take_skip_null_coalesce_operator(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        c.CustomerID,
                        c.CompanyName,
                        Region = c.Region ?? "ZZ"
                    }).OrderBy(c => c.Region).Take(10).Skip(5),
                e => e.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_take_skip_null_coalesce_operator2(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        c.CustomerID,
                        c.CompanyName,
                        c.Region
                    }).OrderBy(c => c.Region ?? "ZZ").Take(10).Skip(5),
                e => e.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_take_skip_null_coalesce_operator3(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.Region ?? "ZZ").Take(10).Skip(5),
                entryCount: 5);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Property_when_non_shadow(bool isAsync)
        {
            return AssertQueryScalar<Order>(
                isAsync,
                os =>
                    from o in os
                    select EF.Property<int>(o, "OrderID"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_Property_when_non_shadow(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os =>
                    from o in os
                    where EF.Property<int>(o, "OrderID") == 10248
                    select o,
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Property_when_shadow(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e in es
                    select EF.Property<string>(e, "Title"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_Property_when_shadow(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    from e in es
                    where EF.Property<string>(e, "Title") == "Sales Representative"
                    select e,
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Property_when_shadow_unconstrained_generic_method(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    ShadowPropertySelect<Employee, string>(es, "Title"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_Property_when_shadow_unconstrained_generic_method(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es =>
                    ShadowPropertyWhere(es, "Title", "Sales Representative"),
                entryCount: 6);
        }

        protected IQueryable<TOut> ShadowPropertySelect<TIn, TOut>(IQueryable<TIn> source, object column)
        {
            return source.Select(e => EF.Property<TOut>(e, (string)column));
        }

        protected IQueryable<T> ShadowPropertyWhere<T>(IQueryable<T> source, object column, string value)
        {
            return source.Where(e => EF.Property<string>(e, (string)column) == value);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_Property_shadow_closure(bool isAsync)
        {
            var propertyName = "Title";
            var value = "Sales Representative";

            await AssertQuery<Employee>(
                isAsync,
                es => es.Where(e => EF.Property<string>(e, propertyName) == value),
                entryCount: 6);

            propertyName = "FirstName";
            value = "Steven";

            await AssertQuery<Employee>(
                isAsync,
                es => es.Where(e => EF.Property<string>(e, propertyName) == value),
                entryCount: 1);
        }

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void Can_cast_CreateQuery_result_to_IQueryable_T_bug_1730()
        {
            using (var context = CreateContext())
            {
                IQueryable<Product> products = context.Products;

                // ReSharper disable once RedundantAssignment
                products = (IQueryable<Product>)products.Provider.CreateQuery(products.Expression);
            }
        }

        //[ConditionalFact]
        //public virtual void Can_execute_non_generic()
        //{
        //    using (var context = CreateContext())
        //    {
        //        IQueryable<Product> products = context.Products;

        //        Assert.NotNull(
        //            products.Provider.Execute(
        //                Expression.Call(
        //                    new LinqOperatorProvider().First.MakeGenericMethod(typeof(Product)),
        //                    products.Expression)));
        //    }
        //}

        [ConditionalFact(Skip = "Issue #17242")]
        public virtual void Select_Subquery_Single()
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = (from od in context.Set<OrderDetail>()
                       orderby od.ProductID, od.OrderID
                       select (from o in context.Set<Order>()
                               where od.OrderID == o.OrderID
                               orderby o.OrderID
                               select o).First())
                    .Take(2)
                    .ToList();

                Assert.Equal(2, orderDetails.Count);
            }
        }

        [ConditionalFact]
        public virtual void Select_Where_Subquery_Deep_Single()
        {
            using (var context = CreateContext())
            {
                var orderDetails
                    = (from od in context.Set<OrderDetail>().Where(od => od.OrderID == 10344)
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

        [ConditionalFact]
        public virtual void Select_Where_Subquery_Deep_First()
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
                                 ).FirstOrDefault()
                             ).FirstOrDefault()
                             .City == "Seattle"
                       select od)
                    .Take(2)
                    .ToList();

                Assert.Equal(2, orderDetails.Count);
            }
        }

        [ConditionalFact]
        public virtual void Select_Where_Subquery_Equality()
        {
            using (var context = CreateContext())
            {
                var orders
                    = (from o in context.Orders.OrderBy(o => o.OrderID).Take(1)
                           // ReSharper disable once UseMethodAny.0
                       where (from od in context.OrderDetails.OrderBy(od => od.OrderID).Take(2)
                              where (from c in context.Set<Customer>()
                                     where c.CustomerID == o.CustomerID
                                     orderby c.CustomerID
                                     select c).First().Country
                                    == (from o2 in context.Set<Order>()
                                        join c in context.Set<Customer>() on o2.CustomerID equals c.CustomerID
                                        where o2.OrderID == od.OrderID
                                        orderby o2.OrderID, c.CustomerID
                                        select c).First().Country
                              orderby od.ProductID, od.OrderID
                              select od).Count() > 0
                       orderby o.OrderID
                       select o).ToList();

                Assert.Equal(1, orders.Count);
            }
        }

        [ConditionalFact(Skip = "Issue#17019")]
        public virtual void Throws_on_concurrent_query_list()
        {
            using (var context = CreateContext())
            {
                context.Database.EnsureCreatedResiliently();

                using (var synchronizationEvent = new ManualResetEventSlim(false))
                {
                    using (var blockingSemaphore = new SemaphoreSlim(0))
                    {
                        var blockingTask = Task.Run(
                            () =>
                            {
                                try
                                {
                                    context.Customers.Select(
                                        c => Process(c, synchronizationEvent, blockingSemaphore)).ToList();
                                }
                                finally
                                {
                                    synchronizationEvent.Set();
                                }
                            });

                        var throwingTask = Task.Run(
                            () =>
                            {
                                synchronizationEvent.Wait(TimeSpan.FromMinutes(5));
                                Assert.Equal(
                                    CoreStrings.ConcurrentMethodInvocation,
                                    Assert.Throws<InvalidOperationException>(
                                        () => context.Customers.ToList()).Message);
                            });

                        throwingTask.Wait(TimeSpan.FromMinutes(5));

                        blockingSemaphore.Release(1);

                        blockingTask.Wait(TimeSpan.FromMinutes(5));
                    }
                }
            }
        }

        [ConditionalFact(Skip = "Issue#17019")]
        public virtual void Throws_on_concurrent_query_first()
        {
            using (var context = CreateContext())
            {
                context.Database.EnsureCreatedResiliently();

                using (var synchronizationEvent = new ManualResetEventSlim(false))
                {
                    using (var blockingSemaphore = new SemaphoreSlim(0))
                    {
                        var blockingTask = Task.Run(
                            () =>
                            {
                                try
                                {
                                    context.Customers.Select(
                                        c => Process(c, synchronizationEvent, blockingSemaphore)).ToList();
                                }
                                finally
                                {
                                    synchronizationEvent.Set();
                                }
                            });

                        var throwingTask = Task.Run(
                            () =>
                            {
                                synchronizationEvent.Wait(TimeSpan.FromMinutes(5));
                                Assert.Equal(
                                    CoreStrings.ConcurrentMethodInvocation,
                                    Assert.Throws<InvalidOperationException>(
                                        () => context.Customers.First()).Message);
                            });

                        throwingTask.Wait(TimeSpan.FromMinutes(5));

                        blockingSemaphore.Release(1);

                        blockingTask.Wait(TimeSpan.FromMinutes(5));
                    }
                }
            }
        }

        private static Customer Process(Customer c, ManualResetEventSlim e, SemaphoreSlim s)
        {
            e.Set();
            s.Wait(TimeSpan.FromMinutes(5));
            s.Release(1);
            return c;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DateTime_parse_is_inlined(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate > DateTime.Parse("1/1/1998 12:00:00 PM")),
                entryCount: 267);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DateTime_parse_is_parameterized_when_from_closure(bool isAsync)
        {
            var date = "1/1/1998 12:00:00 PM";

            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate > DateTime.Parse(date)),
                entryCount: 267);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task New_DateTime_is_inlined(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate > new DateTime(1998, 1, 1, 12, 0, 0)),
                entryCount: 267);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task New_DateTime_is_parameterized_when_from_closure(bool isAsync)
        {
            var year = 1998;
            var month = 1;
            var date = 1;
            var hour = 12;

            await AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate > new DateTime(year, month, date, hour, 0, 0)),
                entryCount: 267);

            hour = 11;

            await AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate > new DateTime(year, month, date, hour, 0, 0)),
                entryCount: 267);
        }

        [ConditionalFact]
        public virtual void Random_next_is_not_funcletized_1()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.TranslationFailed("Where<Order>(    source: DbSet<Order>,     predicate: (o) => o.OrderID > new Random().Next())"),
                    RemoveNewLines(
                        Assert.Throws<InvalidOperationException>(
                            () => context.Orders
                                .Where(o => o.OrderID > new Random().Next())
                                .ToList()).Message));
            }
        }

        [ConditionalFact]
        public virtual void Random_next_is_not_funcletized_2()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.TranslationFailed("Where<Order>(    source: DbSet<Order>,     predicate: (o) => o.OrderID > new Random().Next(5))"),
                    RemoveNewLines(
                        Assert.Throws<InvalidOperationException>(
                            () => context.Orders
                                .Where(o => o.OrderID > new Random().Next(5))
                                .ToList()).Message));
            }
        }

        [ConditionalFact]
        public virtual void Random_next_is_not_funcletized_3()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.TranslationFailed("Where<Order>(    source: DbSet<Order>,     predicate: (o) => o.OrderID > new Random().Next(        minValue: 0,         maxValue: 10))"),
                    RemoveNewLines(
                        Assert.Throws<InvalidOperationException>(
                            () => context.Orders
                                .Where(o => o.OrderID > new Random().Next(0, 10))
                                .ToList()).Message));
            }
        }

        [ConditionalFact]
        public virtual void Random_next_is_not_funcletized_4()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.TranslationFailed("Where<Order>(    source: DbSet<Order>,     predicate: (o) => o.OrderID > new Random(15).Next())"),
                    RemoveNewLines(
                        Assert.Throws<InvalidOperationException>(
                            () => context.Orders
                                .Where(o => o.OrderID > new Random(15).Next())
                                .ToList()).Message));
            }
        }

        [ConditionalFact]
        public virtual void Random_next_is_not_funcletized_5()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.TranslationFailed("Where<Order>(    source: DbSet<Order>,     predicate: (o) => o.OrderID > new Random(15).Next(5))"),
                    RemoveNewLines(
                        Assert.Throws<InvalidOperationException>(
                            () => context.Orders
                                .Where(o => o.OrderID > new Random(15).Next(5))
                                .ToList()).Message));
            }
        }

        [ConditionalFact]
        public virtual void Random_next_is_not_funcletized_6()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.TranslationFailed("Where<Order>(    source: DbSet<Order>,     predicate: (o) => o.OrderID > new Random(15).Next(        minValue: 0,         maxValue: 10))"),
                    RemoveNewLines(
                        Assert.Throws<InvalidOperationException>(
                            () => context.Orders
                                .Where(o => o.OrderID > new Random(15).Next(0, 10))
                                .ToList()).Message));
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Environment_newline_is_funcletized(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID.Contains(Environment.NewLine)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_concat_with_navigation1(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Select(o => o.CustomerID + " " + o.Customer.City));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_concat_with_navigation2(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Select(o => o.Customer.City + " " + o.Customer.City));
        }

        [ConditionalFact]
        public virtual void Select_bitwise_or()
        {
            using (var context = CreateContext())
            {
                var query = context.Customers.OrderBy(c => c.CustomerID).Select(
                    c => new
                    {
                        c.CustomerID,
                        Value = c.CustomerID == "ALFKI" | c.CustomerID == "ANATR"
                    }).ToList();

                Assert.All(query.Take(2), t => Assert.True(t.Value));
                Assert.All(query.Skip(2), t => Assert.False(t.Value));
            }
        }

        [ConditionalFact]
        public virtual void Select_bitwise_or_multiple()
        {
            using (var context = CreateContext())
            {
                var query = context.Customers.OrderBy(c => c.CustomerID)
                    .Select(
                        c => new
                        {
                            c.CustomerID,
                            Value = c.CustomerID == "ALFKI" | c.CustomerID == "ANATR" | c.CustomerID == "ANTON"
                        }).ToList();

                Assert.All(query.Take(3), t => Assert.True(t.Value));
                Assert.All(query.Skip(3), t => Assert.False(t.Value));
            }
        }

        [ConditionalFact]
        public virtual void Select_bitwise_and()
        {
            using (var context = CreateContext())
            {
                var query = context.Customers.OrderBy(c => c.CustomerID).Select(
                    c => new
                    {
                        c.CustomerID,
                        Value = c.CustomerID == "ALFKI" & c.CustomerID == "ANATR"
                    }).ToList();

                Assert.All(query, t => Assert.False(t.Value));
            }
        }

        [ConditionalFact]
        public virtual void Select_bitwise_and_or()
        {
            using (var context = CreateContext())
            {
                var query = context.Customers.OrderBy(c => c.CustomerID)
                    .Select(
                        c => new
                        {
                            c.CustomerID,
                            Value = c.CustomerID == "ALFKI" & c.CustomerID == "ANATR" | c.CustomerID == "ANTON"
                        }).ToList();

                Assert.All(query.Where(c => c.CustomerID != "ANTON"), t => Assert.False(t.Value));
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bitwise_or_with_logical_or(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    cs.Where(c => c.CustomerID == "ALFKI" | c.CustomerID == "ANATR" || c.CustomerID == "ANTON"),
                entryCount: 3);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bitwise_and_with_logical_and(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    cs.Where(c => c.CustomerID == "ALFKI" & c.CustomerID == "ANATR" && c.CustomerID == "ANTON"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bitwise_or_with_logical_and(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    cs.Where(c => c.CustomerID == "ALFKI" | c.CustomerID == "ANATR" && c.Country == "Germany"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_bitwise_and_with_logical_or(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    cs.Where(c => c.CustomerID == "ALFKI" & c.CustomerID == "ANATR" || c.CustomerID == "ANTON"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Select_bitwise_or_with_logical_or()
        {
            using (var context = CreateContext())
            {
                var query = context.Customers.OrderBy(c => c.CustomerID).Select(
                    c => new
                    {
                        c.CustomerID,
                        Value = c.CustomerID == "ALFKI" | c.CustomerID == "ANATR" || c.CustomerID == "ANTON"
                    }).ToList();

                Assert.All(query.Take(3), t => Assert.True(t.Value));
                Assert.All(query.Skip(3), t => Assert.False(t.Value));
            }
        }

        [ConditionalFact]
        public virtual void Select_bitwise_and_with_logical_and()
        {
            using (var context = CreateContext())
            {
                var query = context.Customers.OrderBy(c => c.CustomerID).Select(
                    c => new
                    {
                        c.CustomerID,
                        Value = c.CustomerID == "ALFKI" & c.CustomerID == "ANATR" && c.CustomerID == "ANTON"
                    }).ToList();

                Assert.All(query, t => Assert.False(t.Value));
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Handle_materialization_properly_when_more_than_two_query_sources_are_involved(bool isAsync)
        {
            return AssertFirstOrDefault<Customer, Order, Employee>(
                isAsync,
                (cs, os, es) =>
                    from c in cs.OrderBy(c => c.CustomerID)
                    from o in os
                    from e in es
                    select new
                    {
                        c
                    },
                entryCount: 1);
        }

        // ReSharper disable ArrangeRedundantParentheses
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Parameter_extraction_short_circuits_1(bool isAsync)
        {
            DateTime? dateFilter = new DateTime(1996, 7, 15);

            await AssertQuery<Order>(
                isAsync,
                os =>
                    os.Where(
                        o => (o.OrderID < 10400)
                             && ((dateFilter == null)
                                 || (o.OrderDate.HasValue
                                     && o.OrderDate.Value.Month == dateFilter.Value.Month
                                     && o.OrderDate.Value.Year == dateFilter.Value.Year))),
                entryCount: 22);

            dateFilter = null;

            await AssertQuery<Order>(
                isAsync,
                os =>
                    os.Where(
                        o => (o.OrderID < 10400)
                             && ((dateFilter == null)
                                 || (o.OrderDate.HasValue
                                     && o.OrderDate.Value.Month == dateFilter.Value.Month
                                     && o.OrderDate.Value.Year == dateFilter.Value.Year))),
                entryCount: 152);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Parameter_extraction_short_circuits_2(bool isAsync)
        {
            DateTime? dateFilter = new DateTime(1996, 7, 15);

            await AssertQuery<Order>(
                isAsync,
                os => os.Where(
                    o => (o.OrderID < 10400)
                         && (dateFilter.HasValue)
                         && (o.OrderDate.HasValue
                             && o.OrderDate.Value.Month == dateFilter.Value.Month
                             && o.OrderDate.Value.Year == dateFilter.Value.Year)),
                entryCount: 22);

            dateFilter = null;

            await AssertQuery<Order>(
                isAsync,
                os => os.Where(
                    o => (o.OrderID < 10400)
                         && (dateFilter.HasValue)
                         && (o.OrderDate.HasValue
                             && o.OrderDate.Value.Month == dateFilter.Value.Month
                             && o.OrderDate.Value.Year == dateFilter.Value.Year)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Parameter_extraction_short_circuits_3(bool isAsync)
        {
            DateTime? dateFilter = new DateTime(1996, 7, 15);

            await AssertQuery<Order>(
                isAsync,
                os =>
                    os.Where(
                        o => (o.OrderID < 10400)
                             || (dateFilter == null)
                             || (o.OrderDate.HasValue
                                 && o.OrderDate.Value.Month == dateFilter.Value.Month
                                 && o.OrderDate.Value.Year == dateFilter.Value.Year)),
                entryCount: 152);

            dateFilter = null;

            await AssertQuery<Order>(
                isAsync,
                os =>
                    os.Where(
                        o => (o.OrderID < 10400)
                             || (dateFilter == null)
                             || (o.OrderDate.HasValue
                                 && o.OrderDate.Value.Month == dateFilter.Value.Month
                                 && o.OrderDate.Value.Year == dateFilter.Value.Year)),
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual void Parameter_extraction_can_throw_exception_from_user_code()
        {
            using (var context = CreateContext())
            {
                var customer = new Customer();

                Assert.Throws<InvalidOperationException>(
                    () => context.Customers.Where(c => Equals(c.Orders.First(), customer.Orders.First())).ToList());
            }
        }

        [ConditionalFact]
        public virtual void Parameter_extraction_can_throw_exception_from_user_code_2()
        {
            using (var context = CreateContext())
            {
                DateTime? dateFilter = null;

                Assert.Throws<InvalidOperationException>(
                    () => context.Orders
                        .Where(
                            o => (o.OrderID < 10400)
                                 && ((o.OrderDate.HasValue
                                      && o.OrderDate.Value.Month == dateFilter.Value.Month
                                      && o.OrderDate.Value.Year == dateFilter.Value.Year)))
                        .ToList());
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Subquery_member_pushdown_does_not_change_original_subquery_model(bool isAsync)
        {
            return AssertQuery<Order, Customer>(
                isAsync,
                (os, cs) =>
                    os.OrderBy(o => o.OrderID)
                        .Take(3)
                        .Select(
                            o => new
                            {
                                OrderId = o.OrderID,
                                cs.SingleOrDefault(c => c.CustomerID == o.CustomerID).City
                            })
                        .OrderBy(o => o.City),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Subquery_member_pushdown_does_not_change_original_subquery_model2(bool isAsync)
        {
            return AssertQuery<Order, Customer>(
                isAsync,
                (os, cs) =>
                    os.OrderBy(o => o.OrderID)
                        .Take(3)
                        .Select(
                            o => new
                            {
                                OrderId = o.OrderID,
                                City = EF.Property<string>(cs.SingleOrDefault(c => c.CustomerID == o.CustomerID), "City")
                            })
                        .OrderBy(o => o.City),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Query_expression_with_to_string_and_contains(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate != null && o.EmployeeID.Value.ToString().Contains("10"))
                    .Select(
                        o => new Order
                        {
                            CustomerID = o.CustomerID
                        }),
                e => e.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_expression_other_to_string(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            ShipName = o.OrderDate.Value.ToString()
                        }),
                e => e.ShipName);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_expression_long_to_string(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            ShipName = ((long)o.OrderID).ToString()
                        }),
                e => e.ShipName);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_expression_int_to_string(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            ShipName = o.OrderID.ToString()
                        }),
                e => e.ShipName);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task ToString_with_formatter_is_evaluated_on_the_client(bool isAsync)
        {
            await AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            ShipName = o.OrderID.ToString("X")
                        }),
                e => e.ShipName);

            await AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            ShipName = o.OrderID.ToString(new CultureInfo("en-US"))
                        }),
                e => e.ShipName);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_expression_date_add_year(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            OrderDate = o.OrderDate.Value.AddYears(1)
                        }),
                e => e.OrderDate);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_expression_datetime_add_month(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            OrderDate = o.OrderDate.Value.AddMonths(1)
                        }),
                e => e.OrderDate);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_expression_datetime_add_hour(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            OrderDate = o.OrderDate.Value.AddHours(1)
                        }),
                e => e.OrderDate);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_expression_datetime_add_minute(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            OrderDate = o.OrderDate.Value.AddMinutes(1)
                        }),
                e => e.OrderDate);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_expression_datetime_add_second(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            OrderDate = o.OrderDate.Value.AddSeconds(1)
                        }),
                e => e.OrderDate);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_expression_datetime_add_ticks(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            OrderDate = o.OrderDate.Value.AddTicks(TimeSpan.TicksPerMillisecond)
                        }),
                e => e.OrderDate);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_expression_date_add_milliseconds_above_the_range(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            OrderDate = o.OrderDate.Value.AddMilliseconds(1000000000000)
                        }),
                e => e.OrderDate);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_expression_date_add_milliseconds_below_the_range(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            OrderDate = o.OrderDate.Value.AddMilliseconds(-1000000000000)
                        }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_expression_date_add_milliseconds_large_number_divided(bool isAsync)
        {
            var millisecondsPerDay = 86400000L;
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            OrderDate = o.OrderDate.Value
                                .AddDays(o.OrderDate.Value.Millisecond / millisecondsPerDay)
                                .AddMilliseconds(o.OrderDate.Value.Millisecond % millisecondsPerDay)
                        }),
                e => e.OrderDate);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_expression_references_are_updated_correctly_with_subquery(bool isAsync)
        {
            var nextYear = 2017;

            return AssertQueryScalar<Order>(
                isAsync,
                os => os.Where(o => o.OrderDate != null)
                    .Select(o => o.OrderDate.Value.Year)
                    .Distinct()
                    .Where(x => x < nextYear));
        }

        [ConditionalFact]
        public virtual void DefaultIfEmpty_without_group_join()
        {
            using (var context = CreateContext())
            {
                var query = context.Customers
                    .Where(c => c.City == "London")
                    .DefaultIfEmpty()
                    .Where(d => d != null)
                    .Select(d => d.CustomerID)
                    .ToList();

                Assert.Equal(6, query.Count);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DefaultIfEmpty_in_subquery(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    (from c in cs
                     from o in os.Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                     where o != null
                     select new
                     {
                         c.CustomerID,
                         o.OrderID
                     }),
                e => e.CustomerID + " " + e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DefaultIfEmpty_in_subquery_not_correlated(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    (from c in cs
                     from o in os.Where(o => o.OrderID > 15000).DefaultIfEmpty()
                     select new
                     {
                         c.CustomerID,
                         OrderID = o != null ? o.OrderID : (int?)null
                     }),
                e => e.CustomerID + " " + e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DefaultIfEmpty_in_subquery_nested(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    (from c in cs.Where(c => c.City == "Seattle")
                     from o1 in os.Where(o => o.OrderID > 15000).DefaultIfEmpty()
                     from o2 in os.Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                     where o1 != null && o2 != null
                     orderby o1.OrderID, o2.OrderDate
                     select new
                     {
                         c.CustomerID,
                         o1.OrderID,
                         o2.OrderDate
                     }),
                e => e.CustomerID + " " + e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_skip_take(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactTitle)
                    .ThenBy(c => c.ContactName)
                    .Skip(5)
                    .Take(8),
                assertOrder: true,
                entryCount: 8);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_skip_skip_take(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactTitle)
                    .ThenBy(c => c.ContactName)
                    .Skip(5)
                    .Skip(8)
                    .Take(3),
                assertOrder: true,
                entryCount: 3);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_skip_take_take(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactTitle)
                    .ThenBy(c => c.ContactName)
                    .Skip(5)
                    .Take(8)
                    .Take(3),
                assertOrder: true,
                entryCount: 3);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_skip_take_take_take_take(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactTitle)
                    .ThenBy(c => c.ContactName)
                    .Skip(5)
                    .Take(15)
                    .Take(10)
                    .Take(8)
                    .Take(5),
                assertOrder: true,
                entryCount: 5);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_skip_take_skip_take_skip(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactTitle)
                    .ThenBy(c => c.ContactName)
                    .Skip(5)
                    .Take(15)
                    .Skip(2)
                    .Take(8)
                    .Skip(5),
                assertOrder: true,
                entryCount: 3);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_skip_take_distinct(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactTitle)
                    .ThenBy(c => c.ContactName)
                    .Skip(5)
                    .Take(15)
                    .Distinct(),
                assertOrder: false,
                entryCount: 15);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_coalesce_take_distinct(bool isAsync)
        {
            return AssertQuery<Product>(
                isAsync,
                ps => ps.OrderBy(p => p.UnitPrice ?? 0)
                    .Take(15)
                    .Distinct(),
                assertOrder: false,
                entryCount: 15);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_coalesce_skip_take_distinct(bool isAsync)
        {
            return AssertQuery<Product>(
                isAsync,
                ps => ps.OrderBy(p => p.UnitPrice ?? 0)
                    .Skip(5)
                    .Take(15)
                    .Distinct(),
                assertOrder: false,
                entryCount: 15);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_coalesce_skip_take_distinct_take(bool isAsync)
        {
            return AssertQuery<Product>(
                isAsync,
                ps => ps.OrderBy(p => p.UnitPrice ?? 0)
                    .Skip(5)
                    .Take(15)
                    .Distinct()
                    .Take(5),
                elementAsserter: (_, __) =>
                {
                    /* non-deterministic */
                },
                entryCount: 5);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_skip_take_distinct_orderby_take(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactTitle)
                    .ThenBy(c => c.ContactName)
                    .Skip(5)
                    .Take(15)
                    .Distinct()
                    .OrderBy(c => c.ContactTitle)
                    .Take(8),
                assertOrder: false,
                entryCount: 8);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task No_orderby_added_for_fully_translated_manually_constructed_LOJ(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es => from e1 in es
                      join e2 in es on e1.EmployeeID equals e2.ReportsTo into grouping
                      from e2 in grouping.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                      select new
                      {
                          City1 = e1.City,
                          City2 = e2 != null ? e2.City : null
                      },
#pragma warning restore IDE0031 // Use null propagation
                e => e.City1 + " " + e.City2);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from o in os
                    join c in cs on o.CustomerID equals c.CustomerID into grouping
                    from c in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                    select new
                    {
                        Id1 = o.CustomerID,
                        Id2 = c != null ? c.CustomerID : null
                    },
#pragma warning restore IDE0031 // Use null propagation
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1(
            bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from o in os
                    join c in cs on new
                    {
                        o.CustomerID,
                        o.OrderID
                    } equals new
                    {
                        c.CustomerID,
                        OrderID = 10000
                    } into grouping
                    from c in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                    select new
                    {
                        Id1 = o.CustomerID,
                        Id2 = c != null ? c.CustomerID : null
                    },
#pragma warning restore IDE0031 // Use null propagation
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2(
            bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from o in os
                    join c in cs on new
                    {
                        o.OrderID,
                        o.CustomerID
                    } equals new
                    {
                        OrderID = 10000,
                        c.CustomerID
                    } into grouping
                    from c in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                    select new
                    {
                        Id1 = o.CustomerID,
                        Id2 = c != null ? c.CustomerID : null
                    },
#pragma warning restore IDE0031 // Use null propagation
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es => from e1 in es
                      join e2 in es on e1.EmployeeID equals e2.ReportsTo into grouping
                      from e2 in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                      select new
                      {
                          City1 = e1.City,
                          City2 = e2 != null ? e2.City : null
                      },
#pragma warning restore IDE0031 // Use null propagation
                e => e.City1 + " " + e.City2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Contains_with_DateTime_Date(bool isAsync)
        {
            var dates = new[] { new DateTime(1996, 07, 04), new DateTime(1996, 07, 16) };

            await AssertQuery<Order>(
                isAsync,
                es => es.Where(e => dates.Contains(e.OrderDate.Value.Date)), entryCount: 2);

            dates = new[] { new DateTime(1996, 07, 04) };

            await AssertQuery<Order>(
                isAsync,
                es => es.Where(e => dates.Contains(e.OrderDate.Value.Date)), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_subquery_involving_join_binds_to_correct_table(bool isAsync)
        {
            return AssertQuery<Order, OrderDetail>(
                isAsync,
                (os, ods) =>
                    os.Where(
                        o => o.OrderID > 11000
                             && ods.Where(od => od.Product.ProductName == "Chai")
                                 .Select(od => od.OrderID)
                                 .Contains(o.OrderID)),
                entryCount: 8);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Anonymous_member_distinct_where(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        c.CustomerID
                    }).Distinct().Where(n => n.CustomerID == "ALFKI"),
                e => e.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Anonymous_member_distinct_orderby(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        c.CustomerID
                    }).Distinct().OrderBy(n => n.CustomerID),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Anonymous_member_distinct_result(bool isAsync)
        {
            return AssertSingleResult<Customer>(
                isAsync,
                syncQuery: cs => cs.Select(
                    c => new
                    {
                        c.CustomerID
                    }).Distinct().Count(n => n.CustomerID.StartsWith("A")),
                asyncQuery: cs => cs.Select(
                    c => new
                    {
                        c.CustomerID
                    }).Distinct().CountAsync(n => n.CustomerID.StartsWith("A")));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Anonymous_complex_distinct_where(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        A = c.CustomerID + c.City
                    }).Distinct().Where(n => n.A == "ALFKIBerlin"),
                e => e.A);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Anonymous_complex_distinct_orderby(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        A = c.CustomerID + c.City
                    }).Distinct().OrderBy(n => n.A),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Anonymous_complex_distinct_result(bool isAsync)
        {
            return AssertSingleResult<Customer>(
                isAsync,
                syncQuery: cs => cs.Select(
                    c => new
                    {
                        A = c.CustomerID + c.City
                    }).Distinct().Count(n => n.A.StartsWith("A")),
                asyncQuery: cs => cs.Select(
                    c => new
                    {
                        A = c.CustomerID + c.City
                    }).Distinct().CountAsync(n => n.A.StartsWith("A")));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Anonymous_complex_orderby(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        A = c.CustomerID + c.City
                    }).OrderBy(n => n.A),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Anonymous_subquery_orderby(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.Orders.Count > 1).Select(
                    c => new
                    {
                        A = c.Orders.OrderByDescending(o => o.OrderID).FirstOrDefault().OrderDate
                    }).OrderBy(n => n.A),
                assertOrder: true);
        }

        protected class DTO<T>
        {
            public T Property { get; set; }

            public override bool Equals(object obj)
            {
                if (obj is null)
                {
                    return false;
                }

                return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((DTO<T>)obj);
            }

            private bool Equals(DTO<T> other)
            {
                return EqualityComparer<T>.Default.Equals(Property, other.Property);
            }

            public override int GetHashCode()
            {
                return Property.GetHashCode();
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DTO_member_distinct_where(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new DTO<string>
                    {
                        Property = c.CustomerID
                    }).Distinct().Where(n => n.Property == "ALFKI"),
                e => e.Property,
                elementAsserter: (e, a) => Assert.Equal(e.Property, a.Property));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DTO_member_distinct_orderby(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new DTO<string>
                    {
                        Property = c.CustomerID
                    }).Distinct().OrderBy(n => n.Property),
                assertOrder: true,
                elementAsserter: (e, a) => Assert.Equal(e.Property, a.Property));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DTO_member_distinct_result(bool isAsync)
        {
            return AssertSingleResult<Customer>(
                isAsync,
                syncQuery: cs => cs.Select(
                    c => new DTO<string>
                    {
                        Property = c.CustomerID
                    }).Distinct().Count(n => n.Property.StartsWith("A")),
                asyncQuery: cs => cs.Select(
                    c => new DTO<string>
                    {
                        Property = c.CustomerID
                    }).Distinct().CountAsync(n => n.Property.StartsWith("A")));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DTO_complex_distinct_where(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new DTO<string>
                    {
                        Property = c.CustomerID + c.City
                    }).Distinct().Where(n => n.Property == "ALFKIBerlin"),
                e => e.Property,
                elementAsserter: (e, a) => Assert.Equal(e.Property, a.Property));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DTO_complex_distinct_orderby(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new DTO<string>
                    {
                        Property = c.CustomerID + c.City
                    }).Distinct().OrderBy(n => n.Property),
                assertOrder: true,
                elementAsserter: (e, a) => Assert.Equal(e.Property, a.Property));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DTO_complex_distinct_result(bool isAsync)
        {
            return AssertSingleResult<Customer>(
                isAsync,
                syncQuery: cs => cs.Select(
                    c => new DTO<string>
                    {
                        Property = c.CustomerID + c.City
                    }).Distinct().Count(n => n.Property.StartsWith("A")),
                asyncQuery: cs => cs.Select(
                    c => new DTO<string>
                    {
                        Property = c.CustomerID + c.City
                    }).Distinct().CountAsync(n => n.Property.StartsWith("A")));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DTO_complex_orderby(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new DTO<string>
                    {
                        Property = c.CustomerID + c.City
                    }).OrderBy(n => n.Property),
                assertOrder: true,
                elementAsserter: (e, a) => Assert.Equal(e.Property, a.Property));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DTO_subquery_orderby(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.Orders.Count > 1).Select(
                    c => new DTO<DateTime?>
                    {
                        Property = c.Orders.OrderByDescending(o => o.OrderID).FirstOrDefault().OrderDate
                    }).OrderBy(n => n.Property),
                assertOrder: true,
                elementAsserter: (e, a) => Assert.Equal(e.Property, a.Property));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_orderby_skip_preserves_ordering(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Include(c => c.Orders)
                    .Where(c => c.CustomerID != "VAFFE" && c.CustomerID != "DRACD")
                    .OrderBy(c => c.City)
                    .ThenBy(c => c.CustomerID)
                    .Skip(40)
                    .Take(5),
                entryCount: 48,
                assertOrder: true);
        }

        private static IEnumerable<TElement> ClientDefaultIfEmpty<TElement>(IEnumerable<TElement> source)
        {
            return source?.Count() == 0 ? new[] { default(TElement) } : source;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Complex_query_with_repeated_query_model_compiles_correctly(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs
                    .Where(outer => outer.CustomerID == "ALFKI")
                    .Where(
                        outer =>
                            (from c in cs
                             let customers = cs.Select(cc => cc.CustomerID)
                             where customers.Any()
                             select customers).Any()),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Complex_query_with_repeated_nested_query_model_compiles_correctly(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs
                    .Where(outer => outer.CustomerID == "ALFKI")
                    .Where(
                        outer =>
                            (from c in cs
                             let customers = cs.Where(cc => cs.OrderBy(inner => inner.CustomerID).Take(10).Distinct().Any())
                                 .Select(cc => cc.CustomerID)
                             where customers.Any()
                             select customers).Any()),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Int16_parameter_can_be_used_for_int_column(bool isAsync)
        {
            const ushort parameter = 10300;

            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderID == parameter), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Subquery_is_null_translated_correctly(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    from c in cs
                    let lastOrder = c.Orders.OrderByDescending(o => o.OrderID)
                        .Select(o => o.CustomerID)
                        .FirstOrDefault()
                    where lastOrder == null
                    select c,
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Subquery_is_not_null_translated_correctly(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    from c in cs
                    let lastOrder = c.Orders.OrderByDescending(o => o.OrderID)
                        .Select(o => o.CustomerID)
                        .FirstOrDefault()
                    where lastOrder != null
                    select c,
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_take_average(bool isAsync)
        {
            return AssertAverage<Order>(
                isAsync,
                os => os.OrderBy(o => o.OrderID).Select(o => o.OrderID).Take(10));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_take_count(bool isAsync)
        {
            return AssertCount<Customer>(
                isAsync,
                cs => cs.Take(7));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_orderBy_take_count(bool isAsync)
        {
            return AssertCount<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.Country).Take(7));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_take_long_count(bool isAsync)
        {
            return AssertLongCount<Customer>(
                isAsync,
                cs => cs.Take(7));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_orderBy_take_long_count(bool isAsync)
        {
            return AssertLongCount<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.Country).Take(7));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_take_max(bool isAsync)
        {
            return AssertMax<Order>(
                isAsync,
                os => os.OrderBy(o => o.OrderID).Select(o => o.OrderID).Take(10));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_take_min(bool isAsync)
        {
            return AssertMin<Order>(
                isAsync,
                os => os.OrderBy(o => o.OrderID).Select(o => o.OrderID).Take(10));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_take_sum(bool isAsync)
        {
            return AssertSum<Order>(
                isAsync,
                os => os.OrderBy(o => o.OrderID).Select(o => o.OrderID).Take(10));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_skip_average(bool isAsync)
        {
            return AssertAverage<Order>(
                isAsync,
                os => os.OrderBy(o => o.OrderID).Select(o => o.OrderID).Skip(10));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_skip_count(bool isAsync)
        {
            return AssertCount<Customer>(
                isAsync,
                cs => cs.Skip(7));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_orderBy_skip_count(bool isAsync)
        {
            return AssertCount<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.Country).Skip(7));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_skip_long_count(bool isAsync)
        {
            return AssertLongCount<Customer>(
                isAsync,
                cs => cs.Skip(7));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_orderBy_skip_long_count(bool isAsync)
        {
            return AssertLongCount<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.Country).Skip(7));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_skip_max(bool isAsync)
        {
            return AssertMax<Order>(
                isAsync,
                os => os.OrderBy(o => o.OrderID).Select(o => o.OrderID).Skip(10));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_skip_min(bool isAsync)
        {
            return AssertMin<Order>(
                isAsync,
                os => os.OrderBy(o => o.OrderID).Select(o => o.OrderID).Skip(10));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_skip_sum(bool isAsync)
        {
            return AssertSum<Order>(
                isAsync,
                os => os.OrderBy(o => o.OrderID).Select(o => o.OrderID).Skip(10));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_distinct_average(bool isAsync)
        {
            return AssertAverage<Order>(
                isAsync,
                os => os.Select(o => o.OrderID).Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_distinct_count(bool isAsync)
        {
            return AssertCount<Customer>(
                isAsync,
                cs => cs.Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_distinct_long_count(bool isAsync)
        {
            return AssertLongCount<Customer>(
                isAsync,
                cs => cs.Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_distinct_max(bool isAsync)
        {
            return AssertMax<Order>(
                isAsync,
                os => os.Select(o => o.OrderID).Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_distinct_min(bool isAsync)
        {
            return AssertMin<Order>(
                isAsync,
                os => os.Select(o => o.OrderID).Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_distinct_sum(bool isAsync)
        {
            return AssertSum<Order>(
                isAsync,
                os => os.Select(o => o.OrderID).Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Comparing_to_fixed_string_parameter(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => FindLike(cs, "A"));
        }

        private static IQueryable<string> FindLike(IQueryable<Customer> cs, string prefix)
        {
            return from c in cs
                   where c.CustomerID.StartsWith(prefix)
                   select c.CustomerID;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Comparing_entities_using_Equals(bool isAsync)
        {
            return AssertQuery<Customer, Customer>(
                isAsync,
                (cs1, cs2) => from c1 in cs1
                              from c2 in cs2
                              where c1.CustomerID.StartsWith("ALFKI")
                              where c1.Equals(c2)
                              orderby c1.CustomerID
                              select new
                              {
                                  Id1 = c1.CustomerID,
                                  Id2 = c2.CustomerID
                              });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Comparing_different_entity_types_using_Equals(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) => from c in cs
                            from o in os
                            where c.CustomerID == "ALFKI" && o.CustomerID == "ALFKI"
                            where c.Equals(o)
                            select c.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Comparing_entity_to_null_using_Equals(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs
                      where c.CustomerID.StartsWith("A")
                      where !Equals(null, c)
                      orderby c.CustomerID
                      select c.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Comparing_navigations_using_Equals(bool isAsync)
        {
            return AssertQuery<Order, Order>(
                isAsync,
                (os1, os2) =>
                    from o1 in os1
                    from o2 in os2
                    where o1.CustomerID.StartsWith("A")
                    where o1.Customer.Equals(o2.Customer)
                    orderby o1.OrderID, o2.OrderID
                    select new
                    {
                        Id1 = o1.OrderID,
                        Id2 = o2.OrderID
                    },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Comparing_navigations_using_static_Equals(bool isAsync)
        {
            return AssertQuery<Order, Order>(
                isAsync,
                (os1, os2) =>
                    from o1 in os1
                    from o2 in os2
                    where o1.CustomerID.StartsWith("A")
                    where Equals(o1.Customer, o2.Customer)
                    orderby o1.OrderID, o2.OrderID
                    select new
                    {
                        Id1 = o1.OrderID,
                        Id2 = o2.OrderID
                    },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Comparing_non_matching_entities_using_Equals(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from c in cs
                    from o in os
                    where c.CustomerID == "ALFKI"
                    where Equals(c, o)
                    select new
                    {
                        Id1 = c.CustomerID,
                        Id2 = o.OrderID
                    },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Comparing_non_matching_collection_navigations_using_Equals(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from c in cs
                    from o in os
                    where c.CustomerID == "ALFKI"
                    where c.Orders.Equals(o.OrderDetails)
                    select new
                    {
                        Id1 = c.CustomerID,
                        Id2 = o.OrderID
                    },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Comparing_collection_navigation_to_null(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.Orders == null).Select(c => c.CustomerID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Comparing_collection_navigation_to_null_complex(bool isAsync)
        {
            return AssertQuery<OrderDetail>(
                isAsync,
                ods => ods
                    .Where(od => od.OrderID < 10250)
                    .Where(od => od.Order.Customer.Orders != null)
                    .OrderBy(od => od.OrderID)
                    .ThenBy(od => od.ProductID)
                    .Select(
                        od => new
                        {
                            od.ProductID,
                            od.OrderID
                        }),
                e => e.ProductID + " " + e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Compare_collection_navigation_with_itself(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs
                      where c.CustomerID.StartsWith("A")
                      where c.Orders == c.Orders
                      select c.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Compare_two_collection_navigations_with_different_query_sources(bool isAsync)
        {
            return AssertQuery<Customer, Customer>(
                isAsync,
                (cs1, cs2) =>
                    from c1 in cs1
                    from c2 in cs2
                    where c1.CustomerID == "ALFKI" && c2.CustomerID == "ALFKI"
                    where c1.Orders == c2.Orders
                    select new
                    {
                        Id1 = c1.CustomerID,
                        Id2 = c2.CustomerID
                    },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalTheory(Skip = "issue #8366")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Compare_two_collection_navigations_using_equals(bool isAsync)
        {
            return AssertQuery<Customer, Customer>(
                isAsync,
                (cs1, cs2) =>
                    from c1 in cs1
                    from c2 in cs2
                    where c1.CustomerID == "ALFKI" && c2.CustomerID == "ALFKI"
                    where Equals(c1.Orders, c2.Orders)
                    select new
                    {
                        Id1 = c1.CustomerID,
                        Id2 = c2.CustomerID
                    },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Compare_two_collection_navigations_with_different_property_chains(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from c in cs
                    where c.CustomerID == "ALFKI"
                    from o in os
                    where c.Orders == o.Customer.Orders
                    orderby c.CustomerID, o.OrderID
                    select new
                    {
                        Id1 = c.CustomerID,
                        Id2 = o.OrderID
                    },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_ThenBy_same_column_different_direction(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .OrderBy(c => c.CustomerID)
                    .ThenByDescending(c => c.CustomerID)
                    .Select(c => c.CustomerID),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_OrderBy_same_column_different_direction(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .OrderBy(c => c.CustomerID)
                    .OrderByDescending(c => c.CustomerID)
                    .Select(c => c.CustomerID),
                assertOrder: true);
        }

        [ConditionalTheory(Skip = "Complex Query")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Complex_nested_query_doesnt_try_binding_to_grandparent_when_parent_returns_complex_result(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    cs.Where(c => c.CustomerID == "ALFKI")
                        .Select(
                            c => new
                            {
                                c.CustomerID,
                                OuterOrders = c.Orders.Select(
                                    o => new
                                    {
                                        InnerOrder = c.Orders.Count(),
                                        Id = c.CustomerID
                                    }).ToList()
                            }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.CustomerID, a.CustomerID);
                    Assert.Equal(e.OuterOrders.Count, a.OuterOrders.Count);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Complex_nested_query_properly_binds_to_grandparent_when_parent_returns_scalar_result(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    cs.Where(c => c.CustomerID == "ALFKI")
                        .Select(
                            c => new
                            {
                                c.CustomerID,
                                OuterOrders = c.Orders.Count(o => c.Orders.Count() > 0)
                            }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Dto_projection_skip_take(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID)
                    .Select(
                        c => new
                        {
                            Id = c.CustomerID
                        })
                    .Skip(5)
                    .Take(10),
                elementSorter: e => e.Id);
        }

        [ConditionalFact(Skip = "Issue #17243")]
        public virtual void Streaming_chained_sync_query()
        {
            using (var context = CreateContext())
            {
                var results
                    = (context.Customers
                        .Select(
                            c => new
                            {
                                c.CustomerID,
                                Orders = context.Orders.Where(o => o.Customer.CustomerID == c.CustomerID)
                            }).ToList())
                    .Select(
                        x => new
                        {
                            Orders = x.Orders
                                .GroupJoin(
                                    new[] { "ALFKI" }, y => x.CustomerID, y => y, (h, id) => new
                                    {
                                        h.Customer
                                    })
                        })
                    .ToList();

                Assert.Equal(830, results.SelectMany(r => r.Orders).ToList().Count);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_take_count_works(bool isAsync)
        {
            return AssertCount<Order, Customer>(
                isAsync,
                (os, cs) =>
                    (from o in os.Where(o => o.OrderID > 690 && o.OrderID < 710)
                     join c in cs.Where(c => c.CustomerID == "ALFKI")
                         on o.CustomerID equals c.CustomerID
                     select o)
                    .Take(5));
        }

        [ConditionalTheory(Skip = "Issue#15713")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_empty_list_contains(bool isAsync)
        {
            var list = new List<string>();

            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => list.Contains(c.CustomerID)).Select(c => c),
                entryCount: 91);
        }

        [ConditionalTheory(Skip = "Issue#15713")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_empty_list_does_not_contains(bool isAsync)
        {
            var list = new List<string>();

            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => !list.Contains(c.CustomerID)).Select(c => c),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Manual_expression_tree_typed_null_equality()
        {
            using (var context = CreateContext())
            {
                var orderParameter = Expression.Parameter(typeof(Order), "o");
                var orderCustomer = Expression.MakeMemberAccess(
                    orderParameter, typeof(Order).GetMember(nameof(Order.Customer))[0]);

                var selector = Expression.Lambda<Func<Order, string>>(
                    Expression.Condition(
                        Expression.NotEqual(
                            orderCustomer,
                            Expression.Constant(null, typeof(Customer))),
                        Expression.MakeMemberAccess(
                            orderCustomer,
                            typeof(Customer).GetMember(nameof(Customer.City))[0]),
                        Expression.Constant(null, typeof(string))),
                    orderParameter);

                var query = context.Orders
                    .Where(o => o.OrderID < 10300)
                    .Select(selector).ToList();

                // No verification. Query Compilation check.
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Let_subquery_with_multiple_occurrences(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => from o in os
                      let details =
                          from od in o.OrderDetails
                          where od.Quantity < 10
                          select od.Quantity
                      where details.Any()
                      select new
                      {
                          Count = details.Count()
                      });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Let_entity_equality_to_null(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs.Where(c => c.CustomerID.StartsWith("A"))
                      let o = c.Orders.OrderBy(e => e.OrderDate).FirstOrDefault()
                      where o != null
                      select new
                      {
                          c.CustomerID,
                          o.OrderDate
                      });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Let_entity_equality_to_other_entity(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs.Where(c => c.CustomerID.StartsWith("A"))
                      let o = c.Orders.OrderBy(e => e.OrderDate).FirstOrDefault()
                      where o != new Order()
                      select new
                      {
                          c.CustomerID,
#pragma warning disable IDE0031 // Use null propagation
                          A = (o != null ? o.OrderDate : null)
#pragma warning restore IDE0031 // Use null propagation
                      });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task SelectMany_after_client_method(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("OrderBy<Customer, string>(    source: DbSet<Customer>,     keySelector: (c) => ClientOrderBy(c))"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertQueryScalar<Customer>(
                            isAsync,
                            cs => cs.OrderBy(c => ClientOrderBy(c))
                                .SelectMany(c => c.Orders)
                                .Distinct()
                                .Select(o => o.OrderDate)))).Message));
        }

        private static string ClientOrderBy(Customer c)
        {
            return c.CustomerID;
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Client_where_GroupBy_Group_ordering_works(bool isAsync)
        {
            List<Order> orders = null;
            using (var context = CreateContext())
            {
                orders = context.Orders.Where(o => o.OrderID < 10300).ToList();
            }

            return AssertQuery<Order>(
                isAsync,
                os => from o in os
                      where orders.Select(t => t.OrderID).Contains(o.OrderID)
                      group o by o.CustomerID
                      into g
                      orderby g.Key
                      select g.OrderByDescending(x => x.OrderID),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Order>(elementAsserter: (e, a) => Assert.Equal(e.OrderID, a.OrderID)));
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Client_where_GroupBy_Group_ordering_works_2(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => from o in os
                      where ClientEvalPredicate(o)
                      group o by o.CustomerID
                      into g
                      orderby g.Key
                      select g.OrderByDescending(x => x.OrderID),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Order>(elementAsserter: (e, a) => Assert.Equal(e.OrderID, a.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Client_OrderBy_GroupBy_Group_ordering_works(bool isAsync)
        {
            Assert.StartsWith("The LINQ expression ",
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertQuery<Order>(
                            isAsync,
                            os => from o in os
                                  orderby ClientEvalSelector(o)
                                  group o by o.CustomerID
                                  into g
                                  orderby g.Key
                                  select g.OrderByDescending(x => x.OrderID),
                            assertOrder: true,
                            elementAsserter: CollectionAsserter<Order>(elementAsserter: (e, a) => Assert.Equal(e.OrderID, a.OrderID)))))
                    .Message));
        }


        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_navigation_equal_to_null_for_subquery(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault().OrderDetails == null),
                cs => cs.Where(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault() == null),
                entryCount: 2);
        }

        [ConditionalTheory(Skip = "Needs AsQueryable")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Dependent_to_principal_navigation_equal_to_null_for_subquery(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault().Customer == null),
                cs => cs.Where(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).FirstOrDefault() == null),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_navigation_equality_rewrite_for_subquery(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) => cs.Where(
                    c => c.CustomerID.StartsWith("A")
                         && os.Where(o => o.OrderID < 10300).OrderBy(o => o.OrderID).FirstOrDefault().OrderDetails
                         == os.Where(o => o.OrderID > 10500).OrderBy(o => o.OrderID).FirstOrDefault().OrderDetails));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Inner_parameter_in_nested_lambdas_gets_preserved(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.Orders.Where(o => c == new Customer { CustomerID = o.CustomerID }).Count() > 0),
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Convert_to_nullable_on_nullable_value_is_ignored(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Select(o => new Order { OrderDate = o.OrderDate.Value }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_inside_interpolated_string_is_expanded(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Select(o => $"CustomerCity:{o.Customer.City}"));
        }

        [ConditionalFact]
        public virtual void Client_code_using_instance_method_throws()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<InvalidOperationException>(
                    () => context.Customers.Select(c => InstanceMethod(c)).ToList());
            }
        }

        private string InstanceMethod(Customer c) => c.City;

        [ConditionalFact]
        public virtual void Client_code_using_instance_in_static_method()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<InvalidOperationException>(
                    () => context.Customers.Select(c => StaticMethod(this, c)).ToList());
            }
        }

        private static string StaticMethod(SimpleQueryTestBase<TFixture> containingClass, Customer c) => c.City;

        [ConditionalFact]
        public virtual void Client_code_using_instance_in_anonymous_type()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<InvalidOperationException>(
                    () => context.Customers.Select(c => new { A = this }).ToList());
            }
        }
    }
}
