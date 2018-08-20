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
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
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
                    Customer Query(NorthwindContext c2) =>
                        (from c in context1.Customers
                         from o in c2.Orders
                         select c).First();

                    Assert.Equal(
                        CoreStrings.ErrorInvalidQueryable,
                        Assert.Throws<InvalidOperationException>(
                            () => Query(context2)).Message);
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
            }
        }

        private class Repository<T>
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
                            select new { CustomerID = EF.Property<string>(c1, "CustomerID") })
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

        [ConditionalFact]
        public virtual void Local_array()
        {
            var context = new Context();
            context.Arguments.Add("customerId", "ALFKI");

            AssertSingleResult<Customer>(
                cs => cs.Single(c => c.CustomerID == (string)context.Arguments["customerId"]),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Method_with_constant_queryable_arg()
        {
            using (var context = CreateContext())
            {
                var cache = (MemoryCache)typeof(CompiledQueryCache).GetTypeInfo()
                        .GetField("_memoryCache", BindingFlags.Instance | BindingFlags.NonPublic)
                        ?.GetValue(((IInfrastructure<IServiceProvider>)context).GetService<ICompiledQueryCache>());

                cache.Compact(1);

                var count = QueryableArgQuery(context, new[] { "ALFKI" }.AsQueryable()).Count();
                Assert.Equal(1, count);
                Assert.Equal(1, cache.Count);

                count = QueryableArgQuery(context, new[] { "FOO" }.AsQueryable()).Count();
                Assert.Equal(1, cache.Count);
                Assert.Equal(0, count);
            }
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

        private class InMemoryCheck
        {
            // ReSharper disable once UnusedParameter.Local
            public static bool Check(string input1, string input2)
            {
                return false;
            }
        }

        [ConditionalFact]
        public virtual void Entity_equality_self()
        {
            AssertQuery<Customer>(
                cs =>
                    from c in cs
#pragma warning disable CS1718 // Comparison made to same variable
                        // ReSharper disable once EqualExpressionComparison
                    where c == c
#pragma warning restore CS1718 // Comparison made to same variable
                    select c.CustomerID);
        }

        [ConditionalFact]
        public virtual void Entity_equality_local()
        {
            var local = new Customer { CustomerID = "ANATR" };

            AssertQuery<Customer>(
                cs =>
                    from c in cs
                    where c == local
                    select c.CustomerID);
        }

        [ConditionalFact]
        public virtual void Entity_equality_local_inline()
        {
            AssertQuery<Customer>(
                cs =>
                    from c in cs
                    where c == new Customer { CustomerID = "ANATR" }
                    select c.CustomerID);
        }

        [ConditionalFact]
        public virtual void Entity_equality_null()
        {
            AssertQuery<Customer>(
                cs =>
                    from c in cs
                    where c == null
                    select c.CustomerID);
        }

        [ConditionalFact]
        public virtual void Entity_equality_not_null()
        {
            AssertQuery<Customer>(
                cs =>
                    from c in cs
                    where c != null
                    select c.CustomerID);
        }

        [ConditionalFact]
        public virtual void Null_conditional_simple()
        {
            var c = Expression.Parameter(typeof(Customer));

            var predicate
                = Expression.Lambda<Func<Customer, bool>>(
                    Expression.Equal(
                        new NullConditionalExpression(c, Expression.Property(c, "CustomerID")),
                        Expression.Constant("ALFKI")),
                    c);

            AssertQuery<Customer>(
                cs => cs.Where(predicate),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Null_conditional_deep()
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

            AssertQuery<Customer>(
                cs => cs.Where(predicate),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Queryable_simple()
        {
            AssertQuery<Customer>(
                cs => cs,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Queryable_simple_anonymous()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c }),
                e => e.c.CustomerID,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Queryable_simple_anonymous_projection_subquery()
        {
            AssertQuery<Customer>(
                cs => cs.Take(91).Select(c => new { c }).Select(a => a.c.City));
        }

        [ConditionalFact]
        public virtual void Queryable_simple_anonymous_subquery()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c }).Take(91).Select(a => a.c));
        }

        [ConditionalFact]
        public virtual void Queryable_reprojection()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.IsLondon)
                    .Select(
                        c => new Customer
                        {
                            CustomerID = "Foo",
                            City = c.City
                        }));
        }

        [ConditionalFact]
        public virtual void Queryable_nested_simple()
        {
            AssertQuery<Customer>(
                cs =>
                    from c1 in (from c2 in (from c3 in cs select c3) select c2) select c1,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Take_simple()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Take(10),
                assertOrder: true,
                entryCount: 10);
        }

        [ConditionalFact]
        public virtual void Take_simple_parameterized()
        {
            var take = 10;

            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Take(take),
                assertOrder: true,
                entryCount: 10);
        }

        [ConditionalFact]
        public virtual void Take_simple_projection()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Select(c => c.City).Take(10),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Take_subquery_projection()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Take(2).Select(c => c.City),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Skip()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Skip(5),
                cs => cs.OrderBy(c => c.CustomerID, StringComparer.Ordinal).Skip(5),
                assertOrder: true,
                entryCount: 86);
        }

        [ConditionalFact]
        public virtual void Skip_no_orderby()
        {
            AssertQuery<Customer>(
                cs => cs.Skip(5),
                entryCount: 86,
                elementAsserter: (_, __) =>
                    {
                        /* non-deterministic */
                    });
        }

        [ConditionalFact]
        public virtual void Take_Skip()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Take(10).Skip(5),
                assertOrder: true,
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual void Distinct_Skip()
        {
            AssertQuery<Customer>(
                cs => cs.Distinct().OrderBy(c => c.CustomerID).Skip(5),
                cs => cs.Distinct().OrderBy(c => c.CustomerID, StringComparer.Ordinal).Skip(5),
                assertOrder: true,
                entryCount: 86);
        }

        [ConditionalFact]
        public virtual void Skip_Take()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Skip(5).Take(10),
                assertOrder: true,
                entryCount: 10);
        }

        [ConditionalFact]
        public virtual void Join_Customers_Orders_Skip_Take()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    (from c in cs
                     join o in os on c.CustomerID equals o.CustomerID
                     orderby o.OrderID
                     select new { c.ContactName, o.OrderID }).Skip(10).Take(5),
                e => e.ContactName);
        }

        [ConditionalFact]
        public virtual void Join_Customers_Orders_Projection_With_String_Concat_Skip_Take()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    (from c in cs
                     join o in os on c.CustomerID equals o.CustomerID
                     orderby o.OrderID
                     select new { Contact = c.ContactName + " " + c.ContactTitle, o.OrderID }).Skip(10).Take(5),
                e => e.Contact);
        }

        [ConditionalFact]
        public virtual void Join_Customers_Orders_Orders_Skip_Take_Same_Properties()
        {
            AssertQuery<Customer, Order>(
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

        [ConditionalFact]
        public virtual void Distinct_Skip_Take()
        {
            AssertQuery<Customer>(
                cs => cs.Distinct().OrderBy(c => c.ContactName).Skip(5).Take(10),
                assertOrder: true,
                entryCount: 10);
        }

        [ConditionalFact]
        public virtual void Skip_Distinct()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Skip(5).Distinct(),
                entryCount: 86);
        }

        [ConditionalFact]
        public virtual void Skip_Take_Distinct()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Skip(5).Take(10).Distinct(),
                entryCount: 10);
        }

        [ConditionalFact]
        public virtual void Skip_Take_Any()
        {
            AssertSingleResult<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Skip(5).Take(10).Any());
        }

        [ConditionalFact]
        public virtual void Skip_Take_All()
        {
            AssertSingleResult<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Skip(4).Take(7).All(p => p.CustomerID.StartsWith("B")));
        }

        [ConditionalFact]
        public virtual void Take_All()
        {
            AssertSingleResult<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Take(4).All(p => p.CustomerID.StartsWith("A")));
        }

        [ConditionalFact]
        public virtual void Skip_Take_Any_with_predicate()
        {
            AssertSingleResult<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Skip(5).Take(7).Any(p => p.CustomerID.StartsWith("C")));
        }

        [ConditionalFact]
        public virtual void Take_Any_with_predicate()
        {
            AssertSingleResult<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Take(5).Any(p => p.CustomerID.StartsWith("B")));
        }

        [ConditionalFact]
        public virtual void Take_Skip_Distinct()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Take(10).Skip(5).Distinct(),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual void Take_Skip_Distinct_Caching()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Take(10).Skip(5).Distinct(),
                entryCount: 5);

            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Take(15).Skip(10).Distinct(),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual void Take_Distinct()
        {
            AssertQuery<Order>(
                os => os.OrderBy(o => o.OrderID).Take(5).Distinct(),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual void Distinct_Take()
        {
            AssertQuery<Order>(
                os => os.Distinct().OrderBy(o => o.OrderID).Take(5),
                assertOrder: true,
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual void Distinct_Take_Count()
        {
            AssertSingleResult<Order>(os => os.Distinct().Take(5).Count());
        }

        [ConditionalFact]
        public virtual void Take_Distinct_Count()
        {
            AssertSingleResult<Order>(os => os.Take(5).Distinct().Count());
        }

        [ConditionalFact]
        public virtual void Take_Where_Distinct_Count()
        {
            AssertSingleResult<Order>(
                os => os.Where(o => o.CustomerID == "FRANK").Take(5).Distinct().Count());
        }

        [ConditionalFact]
        public virtual void Any_simple()
        {
            AssertSingleResult<Customer>(cs => cs.Any());
        }

        [ConditionalFact]
        public virtual void OrderBy_Take_Count()
        {
            AssertSingleResult<Order>(os => os.OrderBy(o => o.OrderID).Take(5).Count());
        }

        [ConditionalFact]
        public virtual void Take_OrderBy_Count()
        {
            AssertSingleResult<Order>(os => os.Take(5).OrderBy(o => o.OrderID).Count());
        }

        [ConditionalFact]
        public virtual void Any_predicate()
        {
            AssertSingleResult<Customer>(
                cs => cs.Any(c => c.ContactName.StartsWith("A")));
        }

        [ConditionalFact]
        public virtual void Any_nested_negated()
        {
            AssertQuery<Customer, Order>(
                (cs, os) => cs.Where(c => !os.Any(o => o.CustomerID.StartsWith("A"))));
        }

        [ConditionalFact]
        public virtual void Any_nested_negated2()
        {
            AssertQuery<Customer, Order>(
                (cs, os) => cs.Where(
                    c => c.City != "London"
                         && !os.Any(o => o.CustomerID.StartsWith("A"))));
        }

        [ConditionalFact]
        public virtual void Any_nested_negated3()
        {
            AssertQuery<Customer, Order>(
                (cs, os) => cs.Where(
                    c => !os.Any(o => o.CustomerID.StartsWith("A"))
                         && c.City != "London"));
        }

        [ConditionalFact]
        public virtual void Any_nested()
        {
            AssertQuery<Customer, Order>(
                (cs, os) => cs.Where(c => os.Any(o => o.CustomerID.StartsWith("A"))),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Any_nested2()
        {
            AssertQuery<Customer, Order>(
                (cs, os) => cs.Where(c => c.City != "London" && os.Any(o => o.CustomerID.StartsWith("A"))),
                entryCount: 85);
        }

        [ConditionalFact]
        public virtual void Any_nested3()
        {
            AssertQuery<Customer, Order>(
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

        [ConditionalFact]
        public virtual void All_top_level()
        {
            AssertSingleResult<Customer>(
                cs => cs.All(c => c.ContactName.StartsWith("A")));
        }

        [ConditionalFact]
        public virtual void All_top_level_column()
        {
            AssertSingleResult<Customer>(
                cs => cs.All(c => c.ContactName.StartsWith(c.ContactName)));
        }

        [ConditionalFact]
        public virtual void All_top_level_subquery()
        {
            AssertSingleResult<Customer>(
                cs => cs.All(c1 => cs.Any(c2 => cs.Any(c3 => c1.CustomerID == c3.CustomerID))));
        }

        [ConditionalFact]
        public virtual void All_top_level_subquery_ef_property()
        {
            AssertSingleResult<Customer>(
                cs => cs.All(c1 => cs.Any(c2 => cs.Any(c3 => EF.Property<string>(c1, "CustomerID") == c3.CustomerID))));
        }

        [ConditionalFact]
        public virtual void All_client()
        {
            AssertSingleResult<Customer>(
                cs => cs.All(c => c.IsLondon));
        }

        [ConditionalFact]
        public virtual void All_client_and_server_top_level()
        {
            AssertSingleResult<Customer>(
                cs => cs.All(c => c.CustomerID != "Foo" && c.IsLondon));
        }

        [ConditionalFact]
        public virtual void All_client_or_server_top_level()
        {
            AssertSingleResult<Customer>(
                cs => cs.All(c => c.CustomerID != "Foo" || c.IsLondon));
        }

        [ConditionalFact]
        public virtual void Projection_when_arithmetic_expressions()
        {
            AssertQuery<Order>(
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

        [ConditionalFact]
        public virtual void Projection_when_arithmetic_mixed()
        {
            AssertQuery<Order, Employee>(
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

        [ConditionalFact]
        public virtual void Projection_when_arithmetic_mixed_subqueries()
        {
            AssertQuery<Order, Employee>(
                (os, es) =>
                    from o in os.OrderBy(o => o.OrderID).Take(3).Select(o2 => new { o2, Mod = o2.OrderID % 2 })
                    from e in es.OrderBy(e => e.EmployeeID).Take(2).Select(e2 => new { e2, Square = e2.EmployeeID ^ 2 })
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
                // issue #8956
                entryCount: 3);
        }

        [ConditionalFact]
        public virtual void Take_with_single()
        {
            AssertSingleResult<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Take(1).Single(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Take_with_single_select_many()
        {
            AssertSingleResult<Customer, Order>(
                (cs, os) =>
                    (from c in cs
                     from o in os
                     orderby c.CustomerID, o.OrderID
                     select new { c, o })
                        .Take(1)
                        .Cast<object>()
                        .Single(),
                entryCount: 2);
        }

        [ConditionalFact]
        public virtual void Cast_results_to_object()
        {
            AssertQuery<Customer>(cs => from c in cs.Cast<object>() select c, entryCount: 91);
        }

        [ConditionalFact]
        public virtual void First_client_predicate()
        {
            AssertSingleResult<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).First(c => c.IsLondon),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_select_many_or()
        {
            AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == "London"
                          || e.City == "London"
                    select new { c, e },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 100);
        }

        [ConditionalFact]
        public virtual void Where_select_many_or2()
        {
            AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == "London"
                          || c.City == "Berlin"
                    select new { c, e },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 16);
        }

        [ConditionalFact]
        public virtual void Where_select_many_or3()
        {
            AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == "London"
                          || c.City == "Berlin"
                          || c.City == "Seattle"
                    select new { c, e },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 17);
        }

        [ConditionalFact]
        public virtual void Where_select_many_or4()
        {
            AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == "London"
                          || c.City == "Berlin"
                          || c.City == "Seattle"
                          || c.City == "Lisboa"
                    select new { c, e },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 19);
        }

        [ConditionalFact]
        public virtual void Where_select_many_or_with_parameter()
        {
            var london = "London";
            var lisboa = "Lisboa";

            AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == london
                          || c.City == "Berlin"
                          || c.City == "Seattle"
                          || c.City == lisboa
                    select new { c, e },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 19);
        }

        [ConditionalFact(Skip = "issue #8956")]
        public virtual void Where_subquery_anon()
        {
            AssertQuery<Employee, Order>(
                (es, os) =>
                    from e in es.OrderBy(ee => ee.EmployeeID).Take(3).Select(e => new { e })
                    from o in os.OrderBy(oo => oo.OrderID).Take(5).Select(o => new { o })
                    where e.e.EmployeeID == o.o.EmployeeID
                    select new { e, o },
                entryCount: 2);
        }

        [ConditionalFact(Skip = "issue #8956")]
        public virtual void Where_subquery_anon_nested()
        {
            AssertQuery<Employee, Order, Customer>(
                (es, os, cs) =>
                    from t in (
                        from e in es.OrderBy(ee => ee.EmployeeID).Take(3).Select(e => new { e }).Where(e => e.e.City == "Seattle")
                        from o in os.OrderBy(oo => oo.OrderID).Take(5).Select(o => new { o })
                        select new { e, o })
                    from c in cs.Take(2).Select(c => new { c })
                    select new { t.e, t.o, c });
        }

        [ConditionalFact]
        public virtual void Where_subquery_expression()
        {
            AssertQuery<Order, Order>(
                (o1, o2) =>
                    {
                        var firstOrder = o1.First();
                        Expression<Func<Order, bool>> expr = z => z.OrderID == firstOrder.OrderID;
                        return o1.Where(x => o2.Where(expr).Any());
                    },
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual void Where_subquery_expression_same_parametername()
        {
            AssertQuery<Order, Order>(
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
        public virtual void Select_DTO_with_member_init_distinct_translated_to_server()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Order>()
                    .Where(o => o.OrderID < 10300)
                    .Select(o => new OrderCountDTO { Id = o.CustomerID, Count = o.OrderID })
                    .Distinct().ToList().OrderBy(e => e.Count).ToList();

                var expected = Fixture.QueryAsserter.ExpectedData.Set<Order>()
                    .Where(o => o.OrderID < 10300)
                    .Select(o => new OrderCountDTO { Id = o.CustomerID, Count = o.OrderID })
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
                    .Select(c => new OrderCountDTO { Id = c.CustomerID, Count = c.Orders.Count })
                    .ToList().OrderBy(e => e.Id).ToList();

                var expected = Fixture.QueryAsserter.ExpectedData.Set<Customer>()
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .Select(c => new OrderCountDTO { Id = c.CustomerID, Count = c.Orders.Count })
                    .ToList().OrderBy(e => e.Id).ToList();

                Assert.Equal(expected.Count, actual.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i], actual[i]);
                }
            }
        }

        [ConditionalFact]
        public virtual void Select_DTO_with_member_init_distinct_in_subquery_translated_to_server()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from o in os.Where(o => o.OrderID < 10300)
                        .Select(o => new OrderCountDTO { Id = o.CustomerID, Count = o.OrderID })
                        .Distinct()
                    from c in cs.Where(c => c.CustomerID == o.Id)
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
                                  .Select(o => new OrderCountDTO { Id = o.CustomerID, Count = o.OrderID })
                                  .Distinct()
                              select new { c, o }).ToList().OrderBy(e => e.c.CustomerID + " " + e.o.Count).ToList();

                var expected = (from c in Fixture.QueryAsserter.ExpectedData.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))
                                from o in Fixture.QueryAsserter.ExpectedData.Set<Order>().Where(o => o.OrderID < 10300)
                                    .Select(o => new OrderCountDTO { Id = o.CustomerID, Count = o.OrderID })
                                    .Distinct()
                                select new { c, o }).ToList().OrderBy(e => e.c.CustomerID + " " + e.o.Count).ToList();

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

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                return obj.GetType() == GetType() && Equals((OrderCountDTO)obj);
            }

            private bool Equals(OrderCountDTO other)
            {
                return string.Equals(Id, other.Id) && Count == other.Count;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    // ReSharper disable NonReadonlyMemberInGetHashCode
                    return ((Id?.GetHashCode() ?? 0) * 397) ^ Count;
                    // ReSharper restore NonReadonlyMemberInGetHashCode
                }
            }
        }

        [ConditionalFact]
        public virtual void Select_correlated_subquery_projection()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs.OrderBy(cc => cc.CustomerID).Take(3)
                    select os.Where(o => o.CustomerID == c.CustomerID),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Order>(o => o.OrderID));
        }

        [ConditionalFact]
        public virtual void Select_correlated_subquery_filtered()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    where c.CustomerID.StartsWith("A")
                    orderby c.CustomerID
                    select os.Where(o => o.CustomerID == c.CustomerID),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Order>(o => o.OrderID));
        }

        [ConditionalFact]
        public virtual void Select_correlated_subquery_ordered()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs.Take(3)
                    select os.OrderBy(o => c.CustomerID).Skip(100).Take(2),
                elementSorter: CollectionSorter<Order>(),
                elementAsserter: CollectionAsserter<Order>());
        }

        // TODO: Re-linq parser
        // [ConditionalFact]
        // public virtual void Select_nested_ordered_enumerable_collection()
        // {
        //     AssertQuery<Customer>(cs =>
        //         cs.Select(c => cs.AsEnumerable().OrderBy(c2 => c2.CustomerID)),
        //         assertOrder: true);
        // }

        [ConditionalFact]
        public virtual void Select_nested_collection_in_anonymous_type()
        {
            AssertQuery<Customer, Order>(
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

        [ConditionalFact]
        public virtual void Select_subquery_recursive_trivial()
        {
            AssertQuery<Employee>(
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

        [ConditionalFact]
        public virtual void Where_subquery_on_bool()
        {
            AssertQuery<Product, Product>(
                (pr, pr2) =>
                    from p in pr
                    where pr2.Select(p2 => p2.ProductName).Contains("Chai")
                    select p,
                entryCount: 77);
        }

        [ConditionalFact]
        public virtual void Where_subquery_on_collection()
        {
            AssertQuery<Product, OrderDetail>(
                (pr, od) =>
                    pr.Where(
                        p => od
                            .Where(o => o.ProductID == p.ProductID)
                            .Select(odd => odd.Quantity).Contains<short>(5)),
                entryCount: 43);
        }

        [ConditionalFact]
        public virtual void Where_query_composition()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es
                    where e1.FirstName == es.OrderBy(e => e.EmployeeID).FirstOrDefault().FirstName
                    select e1,
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_query_composition_is_null()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es.OrderBy(e => e.EmployeeID).Take(3)
                    where es.SingleOrDefault(e2 => e2.EmployeeID == e1.ReportsTo) == null
                    select e1,
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_query_composition_is_not_null()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es.OrderBy(e => e.EmployeeID).Skip(4).Take(3)
                    where es.SingleOrDefault(e2 => e2.EmployeeID == e1.ReportsTo) != null
                    select e1,
                entryCount: 3);
        }

        [ConditionalFact]
        public virtual void Where_query_composition_entity_equality_one_element_SingleOrDefault()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es.Take(3)
                    where es.SingleOrDefault(e2 => e2.EmployeeID == e1.ReportsTo) == new Employee()
                    select e1);
        }

        [ConditionalFact]
        public virtual void Where_query_composition_entity_equality_one_element_FirstOrDefault()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es
                    where es.FirstOrDefault(e2 => e2.EmployeeID == e1.ReportsTo) == new Employee()
                    select e1);
        }

        [ConditionalFact]
        public virtual void Where_query_composition_entity_equality_no_elements_SingleOrDefault()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es.Take(3)
                    where es.SingleOrDefault(e2 => e2.EmployeeID == 42) == new Employee()
                    select e1);
        }

        [ConditionalFact]
        public virtual void Where_query_composition_entity_equality_no_elements_Single()
        {
            using (var ctx = CreateContext())
            {
                var query = from e1 in ctx.Set<Employee>().Take(5)
                            where ctx.Set<Employee>().Single(e2 => e2.EmployeeID == 42) == new Employee()
                            select e1;

                Assert.Throws<InvalidOperationException>(() => query.ToList());
            }
        }

        [ConditionalFact]
        public virtual void Where_query_composition_entity_equality_no_elements_FirstOrDefault()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es
                    where es.FirstOrDefault(e2 => e2.EmployeeID == 42) == new Employee()
                    select e1);
        }

        [ConditionalFact]
        public virtual void Where_query_composition_entity_equality_multiple_elements_SingleOrDefault()
        {
            using (var ctx = CreateContext())
            {
                var query = from e1 in ctx.Set<Employee>()
                            where ctx.Set<Employee>().SingleOrDefault(e2 => e2.EmployeeID != e1.ReportsTo) == new Employee()
                            select e1;

                Assert.Throws<InvalidOperationException>(() => query.ToList());
            }
        }

        [ConditionalFact]
        public virtual void Where_query_composition_entity_equality_multiple_elements_FirstOrDefault()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es
                    where es.FirstOrDefault(e2 => e2.EmployeeID != e1.ReportsTo) == new Employee()
                    select e1);
        }

        [ConditionalFact]
        public virtual void Where_query_composition2()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es.Take(3)
                    where e1.FirstName ==
                          (from e2 in es.OrderBy(e => e.EmployeeID)
                           select new { Foo = e2 })
                              .First().Foo.FirstName
                    select e1,
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_query_composition2_FirstOrDefault()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es.Take(3)
                    where e1.FirstName ==
                          (from e2 in es.OrderBy(e => e.EmployeeID)
                           select e2)
                              .FirstOrDefault().FirstName
                    select e1,
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_query_composition2_FirstOrDefault_with_anonymous()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es.Take(3)
                    where e1.FirstName ==
                          (from e2 in es.OrderBy(e => e.EmployeeID)
                           select new { Foo = e2 })
                              .FirstOrDefault().Foo.FirstName
                    select e1,
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_query_composition3()
        {
            AssertQuery<Customer>(
                cs =>
                    from c1 in cs
                    where c1.City == cs.OrderBy(c => c.CustomerID).First(c => c.IsLondon).City
                    select c1,
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual void Where_query_composition4()
        {
            AssertQuery<Customer>(
                cs =>
                    from c1 in cs.OrderBy(c => c.CustomerID).Take(2)
                    where c1.City == (from c2 in cs.OrderBy(c => c.CustomerID)
                                      from c3 in cs.OrderBy(c => c.IsLondon).ThenBy(c => c.CustomerID)
                                      select new { c3 }).First().c3.City
                    select c1,
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_query_composition5()
        {
            AssertQuery<Customer>(
                cs =>
                    from c1 in cs
                    where c1.IsLondon == cs.OrderBy(c => c.CustomerID).First().IsLondon
                    select c1,
                entryCount: 85);
        }

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void OrderBy_scalar_primitive()
        {
            AssertQueryScalar<Employee>(
                es => es.Select(e => e.EmployeeID).OrderBy(i => i),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void SelectMany_mixed()
        {
            AssertQuery<Employee, Customer>(
                (es, cs) =>
                    from e1 in es.OrderBy(e => e.EmployeeID).Take(2)
                    from s in new[] { "a", "b" }
                    from c in cs.OrderBy(c => c.CustomerID).Take(2)
                    select new { e1, s, c },
                e => e.e1.EmployeeID + " " + e.c.CustomerID,
                entryCount: 4);
        }

        [ConditionalFact]
        public virtual void SelectMany_simple1()
        {
            AssertQuery<Employee, Customer>(
                (es, cs) =>
                    from e in es
                    from c in cs
                    select new { c, e },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 100);
        }

        [ConditionalFact]
        public virtual void SelectMany_simple_subquery()
        {
            AssertQuery<Employee, Customer>(
                (es, cs) =>
                    from e in es.Take(9)
                    from c in cs
                    select new { c, e },
                e => e.c.CustomerID + " " + e.e.EmployeeID,
                entryCount: 100);
        }

        [ConditionalFact]
        public virtual void SelectMany_simple2()
        {
            AssertQuery<Employee, Customer>(
                (es, cs) =>
                    from e1 in es
                    from c in cs
                    from e2 in es
                    select new { e1, c, e2.FirstName },
                e => e.e1.EmployeeID + " " + e.c.CustomerID + " " + e.FirstName,
                entryCount: 100);
        }

        [ConditionalFact]
        public virtual void SelectMany_entity_deep()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es
                    from e2 in es
                    from e3 in es
                    from e4 in es
                    select new { e2, e3, e1, e4 },
                e => e.e2.EmployeeID + " " + e.e3.EmployeeID + " " + e.e1.EmployeeID + e.e4.EmployeeID,
                entryCount: 9);
        }

        [ConditionalFact]
        public virtual void SelectMany_projection1()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es
                    from e2 in es
                    select new { e1.City, e2.Country },
                e => e.City + " " + e.Country);
        }

        [ConditionalFact]
        public virtual void SelectMany_projection2()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es
                    from e2 in es
                    from e3 in es
                    select new { e1.City, e2.Country, e3.FirstName },
                e => e.City + " " + e.Country + " " + e.FirstName);
        }

        [ConditionalFact]
        public virtual void SelectMany_nested_simple()
        {
            AssertQuery<Customer>(
                cs =>
                    from c in cs
                    from c1 in
                        (from c2 in (from c3 in cs select c3) select c2)
                    orderby c1.CustomerID
                    select c1,
                cs => cs.SelectMany(
                    c => (from c2 in (from c3 in cs select c3) select c2),
                    (c, c1) => new { c, c1 }).OrderBy(t => t.c1.CustomerID, StringComparer.Ordinal)
                    .Select(t => t.c1),
                assertOrder: true,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void SelectMany_correlated_simple()
        {
            AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == e.City
                    orderby c.CustomerID, e.EmployeeID
                    select new { c, e },
                assertOrder: true,
                entryCount: 15);
        }

        [ConditionalFact]
        public virtual void SelectMany_correlated_subquery_simple()
        {
            AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es.Where(e => e.City == c.City)
                    orderby c.CustomerID, e.EmployeeID
                    select new { c, e },
                assertOrder: true,
                entryCount: 15);
        }

        [ConditionalFact]
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
                    select new { c1, e1 },
                e => e.c1 + " " + e.e1.City + " " + e.e1.c1);
        }

        [ConditionalFact]
        public virtual void SelectMany_cartesian_product_with_ordering()
        {
            AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == e.City
                    orderby e.City, c.CustomerID descending
                    select new { c, e.City },
                assertOrder: true,
                entryCount: 8);
        }

        [ConditionalFact]
        public virtual void SelectMany_primitive()
        {
            AssertQueryScalar<Employee>(
                es => from e1 in es
                      from i in es.Select(e2 => e2.EmployeeID)
                      select i);
        }

        [ConditionalFact]
        public virtual void SelectMany_primitive_select_subquery()
        {
            AssertQueryScalar<Employee>(
                es => from e1 in es
                      from i in es.Select(e2 => e2.EmployeeID)
                      select es.Any());
        }

        [ConditionalFact]
        public virtual void Join_Where_Count()
        {
            AssertSingleResult<Customer, Order>(
                (cs, os) =>
                    (from c in cs
                     join o in os on c.CustomerID equals o.CustomerID
                     where c.CustomerID == "ALFKI"
                     select c).Count());
        }

        [ConditionalFact]
        public virtual void Where_Join_Any()
        {
            AssertSingleResult<Customer>(
                cs => cs.Where(c => c.CustomerID == "ALFKI" && c.Orders.Any(o => o.OrderDate == new DateTime(2008, 10, 24))));
        }

        [ConditionalFact]
        public virtual void Where_Join_Exists()
        {
            AssertSingleResult<Customer>(
                cs => cs.Where(c => c.CustomerID == "ALFKI" && c.Orders.Exists(o => o.OrderDate == new DateTime(2008, 10, 24))));
        }

        [ConditionalFact]
        public virtual void Where_Join_Exists_Inequality()
        {
            AssertSingleResult<Customer>(
                cs => cs.Where(c => c.CustomerID == "ALFKI" && c.Orders.Exists(o => o.OrderDate != new DateTime(2008, 10, 24))),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_Join_Exists_Constant()
        {
            AssertSingleResult<Customer>(
                cs => cs.Where(c => c.CustomerID == "ALFKI" && c.Orders.Exists(o => false)));
        }

        [ConditionalFact]
        public virtual void Where_Join_Not_Exists()
        {
            AssertSingleResult<Customer>(
                cs => cs.Where(c => c.CustomerID == "ALFKI" && !c.Orders.Exists(o => false)),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Multiple_joins_Where_Order_Any()
        {
            AssertSingleResult<Customer, Order, OrderDetail>(
                (cs, os, ods) =>
                    cs.Join(os, c => c.CustomerID, o => o.CustomerID, (cr, or) => new { cr, or })
                        .Join(ods, e => e.or.OrderID, od => od.OrderID, (e, od) => new { e.cr, e.or, od })
                        .Where(r => r.cr.City == "London").OrderBy(r => r.cr.CustomerID)
                        .Any());
        }

        [ConditionalFact]
        public virtual void Join_OrderBy_Count()
        {
            AssertSingleResult<Customer, Order>(
                (cs, os) =>
                    (from c in cs
                     join o in os on c.CustomerID equals o.CustomerID
                     orderby c.CustomerID
                     select c).Count());
        }

        [ConditionalFact]
        public virtual void Where_join_select()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    (from c in cs
                     where c.CustomerID == "ALFKI"
                     join o in os on c.CustomerID equals o.CustomerID
                     select c),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_orderby_join_select()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    (from c in cs
                     where c.CustomerID != "ALFKI"
                     orderby c.CustomerID
                     join o in os on c.CustomerID equals o.CustomerID
                     select c),
                entryCount: 88);
        }

        [ConditionalFact]
        public virtual void Where_join_orderby_join_select()
        {
            AssertQuery<Customer, Order, OrderDetail>(
                (cs, os, ods) =>
                    (from c in cs
                     where c.CustomerID != "ALFKI"
                     join o in os on c.CustomerID equals o.CustomerID
                     orderby c.CustomerID
                     join od in ods on o.OrderID equals od.OrderID
                     select c),
                entryCount: 88);
        }

        [ConditionalFact]
        public virtual void Where_select_many()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    (from c in cs
                     where c.CustomerID == "ALFKI"
                     from o in os
                     select c),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_orderby_select_many()
        {
            AssertQuery<Customer, Order>(
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

#if Test20
        protected const int NonExistentID = -1;
#else
        protected const uint NonExistentID = uint.MaxValue;
#endif

        [ConditionalFact]
        public virtual void Default_if_empty_top_level()
        {
            AssertQuery<Employee>(
                es =>
                    from e in es.Where(c => c.EmployeeID == NonExistentID).DefaultIfEmpty()
                    select e);
        }

        [ConditionalFact]
        public virtual void Default_if_empty_top_level_arg()
        {
            AssertQuery<Employee>(
                es =>
                    from e in es.Where(c => c.EmployeeID == NonExistentID).DefaultIfEmpty(new Employee())
                    select e,
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Default_if_empty_top_level_positive()
        {
            AssertQuery<Employee>(
                es =>
                    from e in es.Where(c => c.EmployeeID > 0).DefaultIfEmpty()
                    select e,
                entryCount: 9);
        }

        [ConditionalFact]
        public virtual void Default_if_empty_top_level_projection()
        {
            AssertQueryScalar<Employee>(
                es =>
                    from e in es.Where(e => e.EmployeeID == NonExistentID).Select(e => e.EmployeeID).DefaultIfEmpty()
                    select e);
        }

        [ConditionalFact]
        public virtual void SelectMany_customer_orders()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    from o in os
                    where c.CustomerID == o.CustomerID
                    select new { c.ContactName, o.OrderID },
                e => e.OrderID);
        }

        [ConditionalFact]
        public virtual void SelectMany_Count()
        {
            AssertSingleResult<Customer, Order>(
                (cs, os) =>
                    (from c in cs
                     from o in os
                     select c.CustomerID).Count());
        }

        [ConditionalFact]
        public virtual void SelectMany_LongCount()
        {
            AssertSingleResult<Customer, Order>(
                (cs, os) =>
                    (from c in cs
                     from o in os
                     select c.CustomerID).LongCount());
        }

        [ConditionalFact]
        public virtual void SelectMany_OrderBy_ThenBy_Any()
        {
            AssertSingleResult<Customer, Order>(
                (cs, os) =>
                    (from c in cs
                     from o in os
                     orderby c.CustomerID, c.City
                     select c).Any());
        }

        // TODO: Composite keys, slow..

        //        [ConditionalFact]
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
        //        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void OrderBy()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID),
                cs => cs.OrderBy(c => c.CustomerID, StringComparer.Ordinal),
                assertOrder: true,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void OrderBy_true()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => true),
                assertOrder: false,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void OrderBy_integer()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => 3),
                assertOrder: false,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void OrderBy_parameter()
        {
            var param = 5;
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => param),
                assertOrder: false,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void OrderBy_anon()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.CustomerID }).OrderBy(a => a.CustomerID),
                cs => cs.Select(c => new { c.CustomerID }).OrderBy(a => a.CustomerID, StringComparer.Ordinal),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void OrderBy_anon2()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c }).OrderBy(a => a.c.CustomerID),
                cs => cs.Select(c => new { c }).OrderBy(a => a.c.CustomerID, StringComparer.Ordinal),
                assertOrder: true,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void OrderBy_client_mixed()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.IsLondon).ThenBy(c => c.CompanyName),
                assertOrder: true,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void OrderBy_multiple_queries()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o in os on new Foo { Bar = c.CustomerID } equals new Foo { Bar = o.CustomerID }
                    orderby c.IsLondon, o.OrderDate
                    select new { c, o });
        }

        [ConditionalFact]
        public virtual void OrderBy_shadow()
        {
            AssertQuery<Employee>(
                es => es.OrderBy(e => EF.Property<string>(e, "Title")).ThenBy(e => e.EmployeeID),
                assertOrder: true,
                entryCount: 9);
        }

        [ConditionalFact]
        public virtual void OrderBy_ThenBy_predicate()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == "London")
                    .OrderBy(c => c.City)
                    .ThenBy(c => c.CustomerID),
                assertOrder: true,
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual void OrderBy_correlated_subquery1()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      where c.CustomerID.StartsWith("A")
                      orderby cs.Any(c2 => c2.CustomerID == c.CustomerID)
                      select c,
                entryCount: 4);
        }

        [ConditionalFact]
        public virtual void OrderBy_correlated_subquery2()
        {
            AssertQuery<Order, Customer>(
                (os, cs) => os.Where(
                    o => o.OrderID <= 10250
                         && cs.OrderBy(
                             c => cs.Any(
                                 c2 => c2.CustomerID == "ALFKI"))
                             .FirstOrDefault().City != "Nowhere"),
                entryCount: 3);
        }

        [ConditionalFact]
        public virtual void OrderBy_Select()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID)
                    .Select(c => c.ContactName),
                cs => cs.OrderBy(c => c.CustomerID, StringComparer.Ordinal)
                    .Select(c => c.ContactName),
                assertOrder: true);
        }

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void OrderBy_ThenBy()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID)
                    .ThenBy(c => c.Country)
                    .Select(c => c.City),
                cs => cs.OrderBy(c => c.CustomerID, StringComparer.Ordinal)
                    .ThenBy(c => c.Country, StringComparer.Ordinal)
                    .Select(c => c.City),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void OrderByDescending()
        {
            AssertQuery<Customer>(
                cs => cs.OrderByDescending(c => c.CustomerID).Select(c => c.City),
                cs => cs.OrderByDescending(c => c.CustomerID, StringComparer.Ordinal).Select(c => c.City),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void OrderByDescending_ThenBy()
        {
            AssertQuery<Customer>(
                cs => cs.OrderByDescending(c => c.CustomerID)
                    .ThenBy(c => c.Country)
                    .Select(c => c.City),
                cs => cs.OrderByDescending(c => c.CustomerID, StringComparer.Ordinal)
                    .ThenBy(c => c.Country, StringComparer.Ordinal)
                    .Select(c => c.City),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void OrderByDescending_ThenByDescending()
        {
            AssertQuery<Customer>(
                cs => cs.OrderByDescending(c => c.CustomerID)
                    .ThenByDescending(c => c.Country)
                    .Select(c => c.City),
                cs => cs.OrderByDescending(c => c.CustomerID, StringComparer.Ordinal)
                    .ThenByDescending(c => c.Country, StringComparer.Ordinal)
                    .Select(c => c.City),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void OrderBy_ThenBy_Any()
        {
            AssertSingleResult<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).ThenBy(c => c.ContactName).Any());
        }

        [ConditionalFact]
        public virtual void OrderBy_Join()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs.OrderBy(c => c.CustomerID)
                    join o in os.OrderBy(o => o.OrderID) on c.CustomerID equals o.CustomerID
                    select new { c.CustomerID, o.OrderID },
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void OrderBy_SelectMany()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs.OrderBy(c => c.CustomerID)
                    from o in os.OrderBy(o => o.OrderID).Take(3)
                    where c.CustomerID == o.CustomerID
                    select new { c.ContactName, o.OrderID },
                (cs, os) =>
                    cs.OrderBy(c => c.CustomerID, StringComparer.Ordinal)
                        .SelectMany(
                            c => os.OrderBy(o => o.OrderID).Take(3),
                            (c, o) => new { c, o }).Where(t => t.c.CustomerID == t.o.CustomerID)
                        .Select(t => new { t.c.ContactName, t.o.OrderID }),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Let_any_subquery_anonymous()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    let hasOrders = os.Any(o => o.CustomerID == c.CustomerID)
                    where c.CustomerID.StartsWith("A")
                    orderby c.CustomerID
                    select new { c, hasOrders },
                assertOrder: true,
                entryCount: 4);
        }

        [ConditionalFact]
        public virtual void OrderBy_arithmetic()
        {
            AssertQuery<Employee>(
                es => es.OrderBy(e => e.EmployeeID - e.EmployeeID),
                entryCount: 9);
        }

        [ConditionalFact]
        public virtual void OrderBy_condition_comparison()
        {
            AssertQuery<Product>(
                ps => ps.OrderBy(p => p.UnitsInStock > 0).ThenBy(p => p.ProductID),
                assertOrder: true,
                entryCount: 77);
        }

        [ConditionalFact]
        public virtual void OrderBy_ternary_conditions()
        {
            AssertQuery<Product>(
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

        [ConditionalFact]
        public virtual void SelectMany_Joined()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    from o in os.Where(o => o.CustomerID == c.CustomerID)
                    select new { c.ContactName, o.OrderDate },
                e => e.ContactName + " " + e.OrderDate);
        }

        [ConditionalFact]
        public virtual void SelectMany_Joined_DefaultIfEmpty()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    from o in os.Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                    select new { c.ContactName, o },
                e => e.ContactName + " " + e.o?.OrderID,
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual void SelectMany_Joined_Take()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    from o in os.Where(o => o.CustomerID == c.CustomerID).Take(1000)
                    select new { c.ContactName, o },
                e => e.o.OrderID,
                entryCount: 830);
        }

        [ConditionalFact]
        public virtual void SelectMany_Joined_DefaultIfEmpty2()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    from o in os.Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                    select o,
                e => e?.OrderID,
                entryCount: 830);

            var fooo = "foob";
            var blah = fooo.TrimStart('o', 'f');

            var foo = new string(new[] { 'a', 'c', 'd' });
        }

        [ConditionalFact]
        public virtual void Select_many_cross_join_same_collection()
        {
            AssertQuery<Customer, Customer>(
                (cs1, cs2) => cs1.SelectMany(c => cs2),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void OrderBy_null_coalesce_operator()
        {
            AssertQuery<Customer>(
                customer => customer
                    .OrderBy(c => c.Region ?? "ZZ"),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Select_null_coalesce_operator()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.CustomerID, c.CompanyName, Region = c.Region ?? "ZZ" }).OrderBy(o => o.Region),
                e => e.CustomerID);
        }

        [ConditionalFact]
        public virtual void OrderBy_conditional_operator()
        {
            AssertQuery<Customer>(
                customer => customer
                    // ReSharper disable once ConvertConditionalTernaryToNullCoalescing
                    // ReSharper disable once MergeConditionalExpression
#pragma warning disable IDE0029 // Use coalesce expression
                    .OrderBy(c => c.Region == null ? "ZZ" : c.Region),
#pragma warning restore IDE0029 // Use coalesce expression
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void OrderBy_conditional_operator_where_condition_null()
        {
            var fakeCustomer = new Customer();
            AssertQuery<Customer>(
                customer => customer
                    .OrderBy(c => fakeCustomer.City == "London" ? "ZZ" : c.City),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void OrderBy_comparison_operator()
        {
            AssertQuery<Customer>(
                customer => customer
                    // ReSharper disable once ConvertConditionalTernaryToNullCoalescing
                    .OrderBy(c => c.Region == "ASK"),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Projection_null_coalesce_operator()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.CustomerID, c.CompanyName, Region = c.Region ?? "ZZ" }),
                e => e.CustomerID);
        }

        [ConditionalFact]
        public virtual void Filter_coalesce_operator()
        {
            AssertQuery<Customer>(
                customer => customer
                    .Where(c => (c.CompanyName ?? c.ContactName) == "The Big Cheese"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Take_skip_null_coalesce_operator()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.Region ?? "ZZ").Take(10).Skip(5).Distinct(),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual void Select_take_null_coalesce_operator()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.CustomerID, c.CompanyName, Region = c.Region ?? "ZZ" }).OrderBy(c => c.Region).Take(5),
                e => e.CustomerID);
        }

        [ConditionalFact]
        public virtual void Select_take_skip_null_coalesce_operator()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.CustomerID, c.CompanyName, Region = c.Region ?? "ZZ" }).OrderBy(c => c.Region).Take(10).Skip(5),
                e => e.CustomerID);
        }

        [ConditionalFact]
        public virtual void Select_take_skip_null_coalesce_operator2()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.CustomerID, c.CompanyName, c.Region }).OrderBy(c => c.Region ?? "ZZ").Take(10).Skip(5),
                e => e.CustomerID);
        }

        [ConditionalFact]
        public virtual void Select_take_skip_null_coalesce_operator3()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.Region ?? "ZZ").Take(10).Skip(5),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual void Select_Property_when_non_shadow()
        {
            AssertQueryScalar<Order>(
                os =>
                    from o in os
                    select EF.Property<int>(o, "OrderID"));
        }

        [ConditionalFact]
        public virtual void Where_Property_when_non_shadow()
        {
            AssertQuery<Order>(
                os =>
                    from o in os
                    where EF.Property<int>(o, "OrderID") == 10248
                    select o,
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Select_Property_when_shadow()
        {
            AssertQuery<Employee>(
                es =>
                    from e in es
                    select EF.Property<string>(e, "Title"));
        }

        [ConditionalFact]
        public virtual void Where_Property_when_shadow()
        {
            AssertQuery<Employee>(
                es =>
                    from e in es
                    where EF.Property<string>(e, "Title") == "Sales Representative"
                    select e,
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual void Select_Property_when_shaow_unconstrained_generic_method()
        {
            AssertQuery<Employee>(
                es =>
                    ShadowPropertySelect<Employee, string>(es, "Title"));
        }

        [ConditionalFact]
        public virtual void Where_Property_when_shaow_unconstrained_generic_method()
        {
            AssertQuery<Employee>(
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

        [ConditionalFact]
        public virtual void Where_Property_shadow_closure()
        {
            var propertyName = "Title";
            var value = "Sales Representative";

            AssertQuery<Employee>(
                es => es.Where(e => EF.Property<string>(e, propertyName) == value),
                entryCount: 6);

            propertyName = "FirstName";
            value = "Steven";

            AssertQuery<Employee>(
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

        [ConditionalFact]
        public virtual void Can_execute_non_generic()
        {
            using (var context = CreateContext())
            {
                IQueryable<Product> products = context.Products;

                Assert.NotNull(
                    products.Provider.Execute(
                        Expression.Call(
                            new LinqOperatorProvider().First.MakeGenericMethod(typeof(Product)),
                            products.Expression)));
            }
        }

        [ConditionalFact]
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
                    = (from o in context.Orders.Take(1)
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

        [ConditionalFact]
        public virtual void Throws_on_concurrent_query_list()
        {
            using (var context = CreateContext())
            {
                context.Database.EnsureCreated();

                using (var synchronizationEvent = new ManualResetEventSlim(false))
                {
                    using (var blockingSemaphore = new SemaphoreSlim(0))
                    {
                        var blockingTask = Task.Run(() =>
                            context.Customers.Select(
                                c => Process(c, synchronizationEvent, blockingSemaphore)).ToList());

                        var throwingTask = Task.Run(() =>
                            {
                                synchronizationEvent.Wait();
                                Assert.Equal(
                                    CoreStrings.ConcurrentMethodInvocation,
                                    Assert.Throws<InvalidOperationException>(
                                        () => context.Customers.ToList()).Message);
                            });

                        throwingTask.Wait();

                        blockingSemaphore.Release(1);

                        blockingTask.Wait();
                    }
                }
            }
        }

        [ConditionalFact]
        public virtual void Throws_on_concurrent_query_first()
        {
            using (var context = CreateContext())
            {
                context.Database.EnsureCreated();

                using (var synchronizationEvent = new ManualResetEventSlim(false))
                {
                    using (var blockingSemaphore = new SemaphoreSlim(0))
                    {
                        var blockingTask = Task.Run(() =>
                            context.Customers.Select(
                                c => Process(c, synchronizationEvent, blockingSemaphore)).ToList());

                        var throwingTask = Task.Run(() =>
                            {
                                synchronizationEvent.Wait();
                                Assert.Equal(
                                    CoreStrings.ConcurrentMethodInvocation,
                                    Assert.Throws<InvalidOperationException>(
                                        () => context.Customers.First()).Message);
                            });

                        throwingTask.Wait();

                        blockingSemaphore.Release(1);

                        blockingTask.Wait();
                    }
                }
            }
        }

        private static Customer Process(Customer c, ManualResetEventSlim e, SemaphoreSlim s)
        {
            e.Set();
            s.Wait();
            s.Release(1);
            return c;
        }

        [ConditionalFact]
        public virtual void DateTime_parse_is_parameterized()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderDate > DateTime.Parse("1/1/1998 12:00:00 PM")),
                entryCount: 267);
        }

        [ConditionalFact]
        public virtual void Random_next_is_not_funcletized_1()
        {
            using (var context = CreateContext())
            {
                var query = context.Orders
                    .Where(o => o.OrderID > new Random().Next())
                    .ToList();
            }
        }

        [ConditionalFact]
        public virtual void Random_next_is_not_funcletized_2()
        {
            using (var context = CreateContext())
            {
                var query = context.Orders
                    .Where(o => o.OrderID > new Random().Next(5))
                    .ToList();
            }
        }

        [ConditionalFact]
        public virtual void Random_next_is_not_funcletized_3()
        {
            using (var context = CreateContext())
            {
                var query = context.Orders
                    .Where(o => o.OrderID > new Random().Next(0, 10))
                    .ToList();
            }
        }

        [ConditionalFact]
        public virtual void Random_next_is_not_funcletized_4()
        {
            using (var context = CreateContext())
            {
                var query = context.Orders
                    .Where(o => o.OrderID > new Random(15).Next())
                    .ToList();
            }
        }

        [ConditionalFact]
        public virtual void Random_next_is_not_funcletized_5()
        {
            using (var context = CreateContext())
            {
                var query = context.Orders
                    .Where(o => o.OrderID > new Random(15).Next(5))
                    .ToList();
            }
        }

        [ConditionalFact]
        public virtual void Random_next_is_not_funcletized_6()
        {
            using (var context = CreateContext())
            {
                var query = context.Orders
                    .Where(o => o.OrderID > new Random(15).Next(0, 10))
                    .ToList();
            }
        }

        [ConditionalFact]
        public virtual void Environment_newline_is_funcletized()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID.Contains(Environment.NewLine)));
        }

        [ConditionalFact]
        public virtual void String_concat_with_navigation1()
        {
            AssertQuery<Order>(
                os => os.Select(o => o.CustomerID + " " + o.Customer.City));
        }

        [ConditionalFact]
        public virtual void String_concat_with_navigation2()
        {
            AssertQuery<Order>(
                os => os.Select(o => o.Customer.City + " " + o.Customer.City));
        }

        [ConditionalFact]
        public virtual void Where_bitwise_or()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.Where(c => c.CustomerID == "ALFKI" | c.CustomerID == "ANATR"),
                entryCount: 2);
        }

        [ConditionalFact]
        public virtual void Where_bitwise_and()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.Where(c => c.CustomerID == "ALFKI" & c.CustomerID == "ANATR"));
        }

        [ConditionalFact]
        public virtual void Select_bitwise_or()
        {
            using (var context = CreateContext())
            {
                var query = context.Customers.OrderBy(c => c.CustomerID).Select(c => new { c.CustomerID, Value = c.CustomerID == "ALFKI" | c.CustomerID == "ANATR" }).ToList();

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
                    .Select(c => new { c.CustomerID, Value = c.CustomerID == "ALFKI" | c.CustomerID == "ANATR" | c.CustomerID == "ANTON" }).ToList();

                Assert.All(query.Take(3), t => Assert.True(t.Value));
                Assert.All(query.Skip(3), t => Assert.False(t.Value));
            }
        }

        [ConditionalFact]
        public virtual void Select_bitwise_and()
        {
            using (var context = CreateContext())
            {
                var query = context.Customers.OrderBy(c => c.CustomerID).Select(c => new { c.CustomerID, Value = c.CustomerID == "ALFKI" & c.CustomerID == "ANATR" }).ToList();

                Assert.All(query, t => Assert.False(t.Value));
            }
        }

        [ConditionalFact]
        public virtual void Select_bitwise_and_or()
        {
            using (var context = CreateContext())
            {
                var query = context.Customers.OrderBy(c => c.CustomerID)
                    .Select(c => new { c.CustomerID, Value = c.CustomerID == "ALFKI" & c.CustomerID == "ANATR" | c.CustomerID == "ANTON" }).ToList();

                Assert.All(query.Where(c => c.CustomerID != "ANTON"), t => Assert.False(t.Value));
            }
        }

        [ConditionalFact]
        public virtual void Where_bitwise_or_with_logical_or()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.Where(c => c.CustomerID == "ALFKI" | c.CustomerID == "ANATR" || c.CustomerID == "ANTON"),
                entryCount: 3);
        }

        [ConditionalFact]
        public virtual void Where_bitwise_and_with_logical_and()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.Where(c => c.CustomerID == "ALFKI" & c.CustomerID == "ANATR" && c.CustomerID == "ANTON"));
        }

        [ConditionalFact]
        public virtual void Where_bitwise_or_with_logical_and()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.Where(c => c.CustomerID == "ALFKI" | c.CustomerID == "ANATR" && c.Country == "Germany"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_bitwise_and_with_logical_or()
        {
            AssertQuery<Customer>(
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

        [ConditionalFact]
        public virtual void Handle_materialization_properly_when_more_than_two_query_sources_are_involved()
        {
            AssertSingleResult<Customer, Order, Employee>(
                (cs, os, es) =>
                    (from c in cs.OrderBy(c => c.CustomerID)
                     from o in os
                     from e in es
                     select new { c }).FirstOrDefault(),
                entryCount: 1);
        }

        // ReSharper disable ArrangeRedundantParentheses
        [ConditionalFact]
        public virtual void Parameter_extraction_short_circuits_1()
        {
            DateTime? dateFilter = new DateTime(1996, 7, 15);

            AssertQuery<Order>(
                os =>
                    os.Where(
                        o => (o.OrderID < 10400)
                             && ((dateFilter == null)
                                 || (o.OrderDate.HasValue
                                     && o.OrderDate.Value.Month == dateFilter.Value.Month
                                     && o.OrderDate.Value.Year == dateFilter.Value.Year))),
                entryCount: 22);

            dateFilter = null;

            AssertQuery<Order>(
                os =>
                    os.Where(
                        o => (o.OrderID < 10400)
                             && ((dateFilter == null)
                                 || (o.OrderDate.HasValue
                                     && o.OrderDate.Value.Month == dateFilter.Value.Month
                                     && o.OrderDate.Value.Year == dateFilter.Value.Year))),
                entryCount: 152);
        }

        [ConditionalFact]
        public virtual void Parameter_extraction_short_circuits_2()
        {
            DateTime? dateFilter = new DateTime(1996, 7, 15);

            AssertQuery<Order>(
                os =>
                    os.Where(
                        o => (o.OrderID < 10400)
                             && (dateFilter.HasValue)
                             && (o.OrderDate.HasValue
                                 && o.OrderDate.Value.Month == dateFilter.Value.Month
                                 && o.OrderDate.Value.Year == dateFilter.Value.Year)),
                entryCount: 22);

            dateFilter = null;

            AssertQuery<Order>(
                os =>
                    os.Where(
                        o => (o.OrderID < 10400)
                             && (dateFilter.HasValue)
                             && (o.OrderDate.HasValue
                                 && o.OrderDate.Value.Month == dateFilter.Value.Month
                                 && o.OrderDate.Value.Year == dateFilter.Value.Year)));
        }

        [ConditionalFact]
        public virtual void Parameter_extraction_short_circuits_3()
        {
            DateTime? dateFilter = new DateTime(1996, 7, 15);

            AssertQuery<Order>(
                os =>
                    os.Where(
                        o => (o.OrderID < 10400)
                             || (dateFilter == null)
                             || (o.OrderDate.HasValue
                                 && o.OrderDate.Value.Month == dateFilter.Value.Month
                                 && o.OrderDate.Value.Year == dateFilter.Value.Year)),
                entryCount: 152);

            dateFilter = null;

            AssertQuery<Order>(
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
                    () =>
                        context.Customers.Where(c => Equals(c.Orders.First(), customer.Orders.First())).ToList());
            }
        }

        [ConditionalFact]
        public virtual void Parameter_extraction_can_throw_exception_from_user_code_2()
        {
            using (var context = CreateContext())
            {
                DateTime? dateFilter = null;

                Assert.Throws<InvalidOperationException>(
                    () =>
                        context.Orders
                            .Where(
                                o => (o.OrderID < 10400)
                                     && ((o.OrderDate.HasValue
                                          && o.OrderDate.Value.Month == dateFilter.Value.Month
                                          && o.OrderDate.Value.Year == dateFilter.Value.Year)))
                            .ToList());
            }
        }

        [ConditionalFact]
        public virtual void Subquery_member_pushdown_does_not_change_original_subquery_model()
        {
            AssertQuery<Order, Customer>(
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

        [ConditionalFact]
        public virtual void Query_expression_with_to_string_and_contains()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderDate != null && o.EmployeeID.Value.ToString().Contains("10"))
                    .Select(
                        o => new Order
                        {
                            CustomerID = o.CustomerID
                        }),
                e => e.CustomerID);
        }

        [ConditionalFact]
        public virtual void Select_expression_other_to_string()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            ShipName = o.OrderDate.Value.ToString()
                        }),
                e => e.ShipName);
        }

        [ConditionalFact]
        public virtual void Select_expression_long_to_string()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            ShipName = ((long)o.OrderID).ToString()
                        }),
                e => e.ShipName);
        }

        [ConditionalFact]
        public virtual void Select_expression_int_to_string()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            ShipName = o.OrderID.ToString()
                        }),
                e => e.ShipName);
        }

        [ConditionalFact]
        public virtual void ToString_with_formatter_is_evaluated_on_the_client()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            ShipName = o.OrderID.ToString("X")
                        }),
                e => e.ShipName);

            AssertQuery<Order>(
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            ShipName = o.OrderID.ToString(new CultureInfo("en-US"))
                        }),
                e => e.ShipName);
        }

        [ConditionalFact]
        public virtual void Select_expression_date_add_year()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            OrderDate = o.OrderDate.Value.AddYears(1)
                        }),
                e => e.OrderDate);
        }

        [ConditionalFact]
        public virtual void Select_expression_date_add_milliseconds_above_the_range()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            OrderDate = o.OrderDate.Value.AddMilliseconds(1000000000000)
                        }),
                e => e.OrderDate);
        }

        [ConditionalFact]
        public virtual void Select_expression_date_add_milliseconds_below_the_range()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderDate != null)
                    .Select(
                        o => new Order
                        {
                            OrderDate = o.OrderDate.Value.AddMilliseconds(-1000000000000)
                        }));
        }

        [ConditionalFact]
        public virtual void Select_expression_date_add_milliseconds_large_number_divided()
        {
            var millisecondsPerDay = 86400000L;
            AssertQuery<Order>(
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

        [ConditionalFact]
        public virtual void Select_expression_references_are_updated_correctly_with_subquery()
        {
            var nextYear = 2017;
            AssertQueryScalar<Order>(
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

        [ConditionalFact]
        public virtual void DefaultIfEmpty_in_subquery()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    (from c in cs
                     from o in os.Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                     where o != null
                     select new { c.CustomerID, o.OrderID }),
                e => e.CustomerID + " " + e.OrderID);
        }

        [ConditionalFact]
        public virtual void DefaultIfEmpty_in_subquery_nested()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    (from c in cs.Where(c => c.City == "Seattle")
                     from o1 in os.Where(o => o.OrderID > 11000).DefaultIfEmpty()
                     from o2 in os.Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                     where o1 != null && o2 != null
                     orderby o1.OrderID, o2.OrderDate
                     select new { c.CustomerID, o1.OrderID, o2.OrderDate }),
                e => e.CustomerID + " " + e.OrderID);
        }

        [ConditionalFact]
        public virtual void OrderBy_skip_take()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactTitle)
                    .ThenBy(c => c.ContactName)
                    .Skip(5)
                    .Take(8),
                assertOrder: true,
                entryCount: 8);
        }

        [ConditionalFact]
        public virtual void OrderBy_skip_skip_take()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactTitle)
                    .ThenBy(c => c.ContactName)
                    .Skip(5)
                    .Skip(8)
                    .Take(3),
                assertOrder: true,
                entryCount: 3);
        }

        [ConditionalFact]
        public virtual void OrderBy_skip_take_take()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactTitle)
                    .ThenBy(c => c.ContactName)
                    .Skip(5)
                    .Take(8)
                    .Take(3),
                assertOrder: true,
                entryCount: 3);
        }

        [ConditionalFact]
        public virtual void OrderBy_skip_take_take_take_take()
        {
            AssertQuery<Customer>(
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

        [ConditionalFact]
        public virtual void OrderBy_skip_take_skip_take_skip()
        {
            AssertQuery<Customer>(
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

        [ConditionalFact]
        public virtual void OrderBy_skip_take_distinct()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactTitle)
                    .ThenBy(c => c.ContactName)
                    .Skip(5)
                    .Take(15)
                    .Distinct(),
                assertOrder: false,
                entryCount: 15);
        }

        [ConditionalFact]
        public virtual void OrderBy_coalesce_take_distinct()
        {
            AssertQuery<Product>(
                ps => ps.OrderBy(p => p.UnitPrice ?? 0)
                    .Take(15)
                    .Distinct(),
                assertOrder: false,
                entryCount: 15);
        }

        [ConditionalFact]
        public virtual void OrderBy_coalesce_skip_take_distinct()
        {
            AssertQuery<Product>(
                ps => ps.OrderBy(p => p.UnitPrice ?? 0)
                    .Skip(5)
                    .Take(15)
                    .Distinct(),
                assertOrder: false,
                entryCount: 15);
        }

        [ConditionalFact]
        public virtual void OrderBy_coalesce_skip_take_distinct_take()
        {
            AssertQuery<Product>(
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

        [ConditionalFact]
        public virtual void OrderBy_skip_take_distinct_orderby_take()
        {
            AssertQuery<Customer>(
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

        [ConditionalFact]
        public virtual void No_orderby_added_for_fully_translated_manually_constructed_LOJ()
        {
            AssertQuery<Employee>(
                es => from e1 in es
                      join e2 in es on e1.EmployeeID equals e2.ReportsTo into grouping
                      from e2 in grouping.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                      select new { City1 = e1.City, City2 = e2 != null ? e2.City : null },
#pragma warning restore IDE0031 // Use null propagation
                e => e.City1 + " " + e.City2);
        }

        [ConditionalFact]
        public virtual void No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from o in os
                    join c in cs on o.CustomerID equals c.CustomerID into grouping
                    from c in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                    select new { Id1 = o.CustomerID, Id2 = c != null ? c.CustomerID : null },
#pragma warning restore IDE0031 // Use null propagation
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalFact]
        public virtual void No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from o in os
                    join c in cs on new { o.CustomerID, o.OrderID } equals new { c.CustomerID, OrderID = 10000 } into grouping
                    from c in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                    select new { Id1 = o.CustomerID, Id2 = c != null ? c.CustomerID : null },
#pragma warning restore IDE0031 // Use null propagation
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalFact]
        public virtual void No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from o in os
                    join c in cs on new { o.OrderID, o.CustomerID } equals new { OrderID = 10000, c.CustomerID } into grouping
                    from c in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                    select new { Id1 = o.CustomerID, Id2 = c != null ? c.CustomerID : null },
#pragma warning restore IDE0031 // Use null propagation
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalFact]
        public virtual void Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ()
        {
            AssertQuery<Employee>(
                es => from e1 in es
                      join e2 in es on e1.EmployeeID equals e2.ReportsTo into grouping
                      from e2 in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                      select new { City1 = e1.City, City2 = e2 != null ? e2.City : null },
#pragma warning restore IDE0031 // Use null propagation
                e => e.City1 + " " + e.City2);
        }

        [ConditionalFact]
        public virtual void Contains_with_DateTime_Date()
        {
            var dates = new[] { new DateTime(1996, 07, 04), new DateTime(1996, 07, 16) };

            AssertQuery<Order>(
                es =>
                    es.Where(e => dates.Contains(e.OrderDate.Value.Date)), entryCount: 2);

            dates = new[] { new DateTime(1996, 07, 04) };

            AssertQuery<Order>(
                es =>
                    es.Where(e => dates.Contains(e.OrderDate.Value.Date)), entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Contains_with_subquery_involving_join_binds_to_correct_table()
        {
            AssertQuery<Order, OrderDetail>(
                (os, ods) =>
                    os.Where(
                        o => o.OrderID > 11000
                             && ods.Where(od => od.Product.ProductName == "Chai")
                                 .Select(od => od.OrderID)
                                 .Contains(o.OrderID)),
                entryCount: 8);
        }

        [ConditionalFact]
        public virtual void Anonymous_member_distinct_where()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.CustomerID }).Distinct().Where(n => n.CustomerID == "ALFKI"),
                e => e.CustomerID);
        }

        [ConditionalFact]
        public virtual void Anonymous_member_distinct_orderby()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.CustomerID }).Distinct().OrderBy(n => n.CustomerID),
                e => e.CustomerID);
        }

        [ConditionalFact]
        public virtual void Anonymous_member_distinct_result()
        {
            AssertSingleResult<Customer>(
                cs => cs.Select(c => new { c.CustomerID }).Distinct().Count(n => n.CustomerID.StartsWith("A")));
        }

        [ConditionalFact]
        public virtual void Anonymous_complex_distinct_where()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { A = c.CustomerID + c.City }).Distinct().Where(n => n.A == "ALFKIBerlin"),
                e => e.A);
        }

        [ConditionalFact]
        public virtual void Anonymous_complex_distinct_orderby()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { A = c.CustomerID + c.City }).Distinct().OrderBy(n => n.A),
                e => e.A);
        }

        [ConditionalFact]
        public virtual void Anonymous_complex_distinct_result()
        {
            AssertSingleResult<Customer>(
                cs => cs.Select(c => new { A = c.CustomerID + c.City }).Distinct().Count(n => n.A.StartsWith("A")));
        }

        [ConditionalFact]
        public virtual void Anonymous_complex_orderby()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { A = c.CustomerID + c.City }).OrderBy(n => n.A),
                e => e.A);
        }

        [ConditionalFact]
        public virtual void Anonymous_subquery_orderby()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.Orders.Count > 1).Select(
                    c => new
                    {
                        A = c.Orders.OrderByDescending(o => o.OrderID).FirstOrDefault().OrderDate
                    }).OrderBy(n => n.A),
                e => e.A);
        }

        private class DTO<T>
        {
            public T Property { get; set; }

            public override bool Equals(object obj)
            {
                if (obj is null)
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                return obj.GetType() == GetType() && Equals((DTO<T>)obj);
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

        [ConditionalFact]
        public virtual void DTO_member_distinct_where()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new DTO<string> { Property = c.CustomerID }).Distinct().Where(n => n.Property == "ALFKI"),
                e => e.Property,
                elementAsserter: (e, a) => Assert.Equal(e.Property, a.Property));
        }

        [ConditionalFact]
        public virtual void DTO_member_distinct_orderby()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new DTO<string> { Property = c.CustomerID }).Distinct().OrderBy(n => n.Property),
                assertOrder: true,
                elementAsserter: (e, a) => Assert.Equal(e.Property, a.Property));
        }

        [ConditionalFact]
        public virtual void DTO_member_distinct_result()
        {
            AssertSingleResult<Customer>(
                cs => cs.Select(c => new DTO<string> { Property = c.CustomerID }).Distinct().Count(n => n.Property.StartsWith("A")));
        }

        [ConditionalFact]
        public virtual void DTO_complex_distinct_where()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new DTO<string> { Property = c.CustomerID + c.City }).Distinct().Where(n => n.Property == "ALFKIBerlin"),
                e => e.Property,
                elementAsserter: (e, a) => Assert.Equal(e.Property, a.Property));
        }

        [ConditionalFact]
        public virtual void DTO_complex_distinct_orderby()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new DTO<string> { Property = c.CustomerID + c.City }).Distinct().OrderBy(n => n.Property),
                assertOrder: true,
                elementAsserter: (e, a) => Assert.Equal(e.Property, a.Property));
        }

        [ConditionalFact]
        public virtual void DTO_complex_distinct_result()
        {
            AssertSingleResult<Customer>(
                cs => cs.Select(c => new DTO<string> { Property = c.CustomerID + c.City }).Distinct().Count(n => n.Property.StartsWith("A")));
        }

        [ConditionalFact]
        public virtual void DTO_complex_orderby()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new DTO<string> { Property = c.CustomerID + c.City }).OrderBy(n => n.Property),
                assertOrder: true,
                elementAsserter: (e, a) => Assert.Equal(e.Property, a.Property));
        }

        [ConditionalFact]
        public virtual void DTO_subquery_orderby()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.Orders.Count > 1).Select(
                    c => new DTO<DateTime?>
                    {
                        Property = c.Orders.OrderByDescending(o => o.OrderID).FirstOrDefault().OrderDate
                    }).OrderBy(n => n.Property),
                assertOrder: true,
                elementAsserter: (e, a) => Assert.Equal(e.Property, a.Property));
        }

        [ConditionalFact]
        public virtual void Include_with_orderby_skip_preserves_ordering()
        {
            AssertQuery<Customer>(
                cs => cs.Include(c => c.Orders).Where(c => c.CustomerID != "VAFFE" && c.CustomerID != "DRACD").OrderBy(c => c.City).Skip(40).Take(5),
                entryCount: 48,
                assertOrder: true);
        }

        private static IEnumerable<TElement> ClientDefaultIfEmpty<TElement>(IEnumerable<TElement> source)
        {
            return source?.Count() == 0 ? new[] { default(TElement) } : source;
        }

        [ConditionalFact]
        public virtual void Complex_query_with_repeated_query_model_compiles_correctly()
        {
            AssertQuery<Customer>(
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

        [ConditionalFact]
        public virtual void Complex_query_with_repeated_nested_query_model_compiles_correctly()
        {
            AssertQuery<Customer>(
                cs => cs
                    .Where(outer => outer.CustomerID == "ALFKI")
                    .Where(
                        outer =>
                            (from c in cs
                             let customers = cs.Where(cc => cs.OrderBy(inner => inner.CustomerID).Take(10).Distinct().Any()).Select(cc => cc.CustomerID)
                             where customers.Any()
                             select customers).Any()),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Int16_parameter_can_be_used_for_int_column()
        {
            const ushort parameter = 10300;
            AssertQuery<Order>(os => os.Where(o => o.OrderID == parameter), entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Subquery_is_null_translated_correctly()
        {
            AssertQuery<Customer>(
                cs =>
                    from c in cs
                    let lastOrder = c.Orders.OrderByDescending(o => o.OrderID)
                        .Select(o => o.CustomerID)
                        .FirstOrDefault()
                    where lastOrder == null
                    select c,
                entryCount: 2);
        }

        [ConditionalFact]
        public virtual void Subquery_is_not_null_translated_correctly()
        {
            AssertQuery<Customer>(
                cs =>
                    from c in cs
                    let lastOrder = c.Orders.OrderByDescending(o => o.OrderID)
                        .Select(o => o.CustomerID)
                        .FirstOrDefault()
                    where lastOrder != null
                    select c,
                entryCount: 89);
        }

        [ConditionalFact]
        public virtual void Select_take_average()
        {
            AssertSingleResult<Order>(os => os.OrderBy(o => o.OrderID).Select(o => o.OrderID).Take(10).Average());
        }

        [ConditionalFact]
        public virtual void Select_take_count()
        {
            AssertSingleResult<Customer>(cs => cs.Take(7).Count());
        }

        [ConditionalFact]
        public virtual void Select_orderBy_take_count()
        {
            AssertSingleResult<Customer>(cs => cs.OrderBy(c => c.Country).Take(7).Count());
        }

        [ConditionalFact]
        public virtual void Select_take_long_count()
        {
            AssertSingleResult<Customer>(cs => cs.Take(7).LongCount());
        }

        [ConditionalFact]
        public virtual void Select_orderBy_take_long_count()
        {
            AssertSingleResult<Customer>(cs => cs.OrderBy(c => c.Country).Take(7).LongCount());
        }

        [ConditionalFact]
        public virtual void Select_take_max()
        {
            AssertSingleResult<Order>(os => os.OrderBy(o => o.OrderID).Select(o => o.OrderID).Take(10).Max());
        }

        [ConditionalFact]
        public virtual void Select_take_min()
        {
            AssertSingleResult<Order>(os => os.OrderBy(o => o.OrderID).Select(o => o.OrderID).Take(10).Min());
        }

        [ConditionalFact]
        public virtual void Select_take_sum()
        {
            AssertSingleResult<Order>(os => os.OrderBy(o => o.OrderID).Select(o => o.OrderID).Take(10).Sum());
        }

        [ConditionalFact]
        public virtual void Select_skip_average()
        {
            AssertSingleResult<Order>(os => os.OrderBy(o => o.OrderID).Select(o => o.OrderID).Skip(10).Average());
        }

        [ConditionalFact]
        public virtual void Select_skip_count()
        {
            AssertSingleResult<Customer>(cs => cs.Skip(7).Count());
        }

        [ConditionalFact]
        public virtual void Select_orderBy_skip_count()
        {
            AssertSingleResult<Customer>(cs => cs.OrderBy(c => c.Country).Skip(7).Count());
        }

        [ConditionalFact]
        public virtual void Select_skip_long_count()
        {
            AssertSingleResult<Customer>(cs => cs.Skip(7).LongCount());
        }

        [ConditionalFact]
        public virtual void Select_orderBy_skip_long_count()
        {
            AssertSingleResult<Customer>(cs => cs.OrderBy(c => c.Country).Skip(7).LongCount());
        }

        [ConditionalFact]
        public virtual void Select_skip_max()
        {
            AssertSingleResult<Order>(os => os.OrderBy(o => o.OrderID).Select(o => o.OrderID).Skip(10).Max());
        }

        [ConditionalFact]
        public virtual void Select_skip_min()
        {
            AssertSingleResult<Order>(os => os.OrderBy(o => o.OrderID).Select(o => o.OrderID).Skip(10).Min());
        }

        [ConditionalFact]
        public virtual void Select_skip_sum()
        {
            AssertSingleResult<Order>(os => os.OrderBy(o => o.OrderID).Select(o => o.OrderID).Skip(10).Sum());
        }

        [ConditionalFact]
        public virtual void Select_distinct_average()
        {
            AssertSingleResult<Order>(os => os.Select(o => o.OrderID).Distinct().Average());
        }

        [ConditionalFact]
        public virtual void Select_distinct_count()
        {
            AssertSingleResult<Customer>(cs => cs.Distinct().Count());
        }

        [ConditionalFact]
        public virtual void Select_distinct_long_count()
        {
            AssertSingleResult<Customer>(cs => cs.Distinct().LongCount());
        }

        [ConditionalFact]
        public virtual void Select_distinct_max()
        {
            AssertSingleResult<Order>(os => os.Select(o => o.OrderID).Distinct().Max());
        }

        [ConditionalFact]
        public virtual void Select_distinct_min()
        {
            AssertSingleResult<Order>(os => os.Select(o => o.OrderID).Distinct().Min());
        }

        [ConditionalFact]
        public virtual void Select_distinct_sum()
        {
            AssertSingleResult<Order>(os => os.Select(o => o.OrderID).Distinct().Sum());
        }

        [ConditionalFact]
        public virtual void Comparing_to_fixed_string_parameter()
        {
            AssertQuery<Customer>(cs => FindLike(cs, "A"));
        }

        private static IQueryable<string> FindLike(IQueryable<Customer> cs, string prefix)
        {
            return from c in cs
                   where c.CustomerID.StartsWith(prefix)
                   select c.CustomerID;
        }

        [ConditionalFact]
        public virtual void Comparing_entities_using_Equals()
        {
            AssertQuery<Customer, Customer>(
                (cs1, cs2) => from c1 in cs1
                              from c2 in cs2
                              where c1.CustomerID.StartsWith("ALFKI")
                              where c1.Equals(c2)
                              orderby c1.CustomerID
                              select new { Id1 = c1.CustomerID, Id2 = c2.CustomerID });
        }

        [ConditionalFact]
        public virtual void Comparing_different_entity_types_using_Equals()
        {
            AssertQuery<Customer, Order>(
                (cs, os) => from c in cs
                            from o in os
                            where c.CustomerID == " ALFKI" && o.CustomerID == "ALFKI"
                            where c.Equals(o)
                            select c.CustomerID);
        }

        [ConditionalFact]
        public virtual void Comparing_entity_to_null_using_Equals()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      where c.CustomerID.StartsWith("A")
                      where !Equals(null, c)
                      orderby c.CustomerID
                      select c.CustomerID);
        }

        [ConditionalFact]
        public virtual void Comparing_navigations_using_Equals()
        {
            AssertQuery<Order, Order>(
                (os1, os2) =>
                    from o1 in os1
                    from o2 in os2
                    where o1.CustomerID.StartsWith("A")
                    where o1.Customer.Equals(o2.Customer)
                    orderby o1.OrderID, o2.OrderID
                    select new { Id1 = o1.OrderID, Id2 = o2.OrderID },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalFact]
        public virtual void Comparing_navigations_using_static_Equals()
        {
            AssertQuery<Order, Order>(
                (os1, os2) =>
                    from o1 in os1
                    from o2 in os2
                    where o1.CustomerID.StartsWith("A")
                    where Equals(o1.Customer, o2.Customer)
                    orderby o1.OrderID, o2.OrderID
                    select new { Id1 = o1.OrderID, Id2 = o2.OrderID },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalFact]
        public virtual void Comparing_non_matching_entities_using_Equals()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    from o in os
                    where c.CustomerID == "ALFKI"
                    where Equals(c, o)
                    select new { Id1 = c.CustomerID, Id2 = o.OrderID },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalFact]
        public virtual void Comparing_non_matching_collection_navigations_using_Equals()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    from o in os
                    where c.CustomerID == "ALFKI"
                    where c.Orders.Equals(o.OrderDetails)
                    select new { Id1 = c.CustomerID, Id2 = o.OrderID },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalFact]
        public virtual void Comparing_collection_navigation_to_null()
        {
            AssertQuery<Customer>(cs => cs.Where(c => c.Orders == null).Select(c => c.CustomerID));
        }

        [ConditionalFact]
        public virtual void Comparing_collection_navigation_to_null_complex()
        {
            AssertQuery<OrderDetail>(
                ods => ods
                    .Where(od => od.OrderID < 10250)
                    .Where(od => od.Order.Customer.Orders != null)
                    .OrderBy(od => od.OrderID)
                    .ThenBy(od => od.ProductID)
                    .Select(od => new { od.ProductID, od.OrderID }),
                e => e.ProductID + " " + e.OrderID);
        }

        [ConditionalFact]
        public virtual void Compare_collection_navigation_with_itself()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      where c.CustomerID.StartsWith("A")
                      where c.Orders == c.Orders
                      select c.CustomerID);
        }

        [ConditionalFact]
        public virtual void Compare_two_collection_navigations_with_different_query_sources()
        {
            AssertQuery<Customer, Customer>(
                (cs1, cs2) =>
                    from c1 in cs1
                    from c2 in cs2
                    where c1.CustomerID == "ALFKI" && c2.CustomerID == "ALFKI"
                    where c1.Orders == c2.Orders
                    select new { Id1 = c1.CustomerID, Id2 = c2.CustomerID },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalFact(Skip = "issue #8366")]
        public virtual void Compare_two_collection_navigations_using_equals()
        {
            AssertQuery<Customer, Customer>(
                (cs1, cs2) =>
                    from c1 in cs1
                    from c2 in cs2
                    where c1.CustomerID == "ALFKI" && c2.CustomerID == "ALFKI"
                    where Equals(c1.Orders, c2.Orders)
                    select new { Id1 = c1.CustomerID, Id2 = c2.CustomerID },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalFact]
        public virtual void Compare_two_collection_navigations_with_different_property_chains()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    from o in os
                    where c.CustomerID == "ALFKI"
                    where c.Orders == o.Customer.Orders
                    orderby c.CustomerID, o.OrderID
                    select new { Id1 = c.CustomerID, Id2 = o.OrderID },
                e => e.Id1 + " " + e.Id2);
        }

        [ConditionalFact]
        public virtual void OrderBy_ThenBy_same_column_different_direction()
        {
            AssertQuery<Customer>(
                cs => cs
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .OrderBy(c => c.CustomerID)
                    .ThenByDescending(c => c.CustomerID)
                    .Select(c => c.CustomerID),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void OrderBy_OrderBy_same_column_different_direction()
        {
            AssertQuery<Customer>(
                cs => cs
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .OrderBy(c => c.CustomerID)
                    .OrderByDescending(c => c.CustomerID)
                    .Select(c => c.CustomerID),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Complex_nested_query_doesnt_try_binding_to_grandparent_when_parent_returns_complex_result()
        {
            AssertQuery<Customer>(cs =>
                cs.Where(c => c.CustomerID == "ALFKI")
                    .Select(c => new
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

        [ConditionalFact]
        public virtual void Complex_nested_query_properly_binds_to_grandparent_when_parent_returns_scalar_result()
        {
            AssertQuery<Customer>(cs =>
                cs.Where(c => c.CustomerID == "ALFKI")
                    .Select(c => new
                    {
                        c.CustomerID,
                        OuterOrders = c.Orders.Count(o => c.Orders.Count() > 0)
                    }));
        }

        [ConditionalFact]
        public virtual void OrderBy_Dto_projection_skip_take()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID)
                    .Select(c => new
                    {
                        Id = c.CustomerID
                    })
                    .Skip(5)
                    .Take(10),
                elementSorter: e => e.Id);
        }

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void Join_take_count_works()
        {
            AssertSingleResult<Order, Customer>(
                (os, cs) =>
                (from o in os.Where(o => o.OrderID > 690 && o.OrderID < 710)
                 join c in cs.Where(c => c.CustomerID == "ALFKI")
                  on o.CustomerID equals c.CustomerID
                 select o)
                 .Take(5)
                 .Count());
        }

        [ConditionalFact]
        public virtual void OrderBy_empty_list_contains()
        {
            var list = new List<string>();
            AssertQuery<Customer>(cs => cs.OrderBy(c => list.Contains(c.CustomerID)),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void OrderBy_empty_list_does_not_contains()
        {
            var list = new List<string>();
            AssertQuery<Customer>(cs => cs.OrderBy(c => !list.Contains(c.CustomerID)),
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
                        Expression.Equal(
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

        [ConditionalFact]
        public virtual void Let_subquery_with_multiple_occurences()
        {
            AssertQuery<Order>(
                os => from o in os
                      let details =
                            from od in o.OrderDetails
                            where od.Quantity < 10
                            select od.Quantity
                      where details.Any()
                      select new { Count = details.Count() });
        }

        [ConditionalFact]
        public virtual void Let_entity_equality_to_null()
        {
            AssertQuery<Customer>(
                cs => from c in cs.Where(c => c.CustomerID.StartsWith("A"))
                      let o = c.Orders.OrderBy(e => e.OrderDate).FirstOrDefault()
                      where o != null
                      select new
                      {
                          c.CustomerID,
                          o.OrderDate
                      });
        }

        [ConditionalFact]
        public virtual void Let_entity_equality_to_other_entity()
        {
            AssertQuery<Customer>(
                cs => from c in cs.Where(c => c.CustomerID.StartsWith("A"))
                      let o = c.Orders.OrderBy(e => e.OrderDate).FirstOrDefault()
                      where o != new Order()
                      select new
                      {
                          c.CustomerID,
                          A = (o != null ? o.OrderDate : null)
                      });
        }

        [ConditionalFact]
        public virtual void Collection_navigation_equal_to_null_for_subquery()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault().OrderDetails == null),
                cs => cs.Where(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault() == null),
                entryCount: 2);
        }

        [ConditionalFact]
        public virtual void Dependent_to_principal_navigation_equal_to_null_for_subquery()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault().Customer == null),
                cs => cs.Where(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).FirstOrDefault() == null),
                entryCount: 2);
        }

        [ConditionalFact]
        public virtual void Collection_navigation_equality_rewrite_for_subquery()
        {
            AssertQuery<Customer, Order>(
                (cs, os) => cs.Where(c => c.CustomerID.StartsWith("A")
                    && os.Where(o => o.OrderID < 10300).OrderBy(o => o.OrderID).FirstOrDefault().OrderDetails
                        == os.Where(o => o.OrderID > 10500).OrderBy(o => o.OrderID).FirstOrDefault().OrderDetails));
        }
    }
}
