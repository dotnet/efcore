// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable StringStartsWithIsCultureSpecific

#pragma warning disable RCS1202 // Avoid NullReferenceException.

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindIncludeQueryTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected NorthwindIncludeQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_and_collection_order_by(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Include(o => o.Customer.Orders).OrderBy(o => o.OrderID),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<Order>(o => o.Customer), new ExpectedInclude<Customer>(c => c.Orders, "Customer")),
                assertOrder: true,
                entryCount: 919);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_references_then_include_collection(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Order>().Include(o => o.Customer).ThenInclude(c => c.Orders),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<Order>(o => o.Customer),
                    new ExpectedInclude<Customer>(c => c.Orders, "Customer")),
                entryCount: 919);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_property_after_navigation(bool async)
        {
            Assert.Equal(
                CoreStrings.InvalidIncludeExpression("o.Customer.CustomerID"),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertQuery(
                        async,
                        ss => ss.Set<Order>().Include(o => o.Customer.CustomerID)))).Message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_property(bool async)
        {
            Assert.Equal(
                CoreStrings.InvalidIncludeExpression("o.OrderDate"),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertQuery(
                        async,
                        ss => ss.Set<Order>().Include(o => o.OrderDate)))).Message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_property_expression_invalid(bool async)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<Order>().Include(o => new { o.Customer, o.OrderDetails })));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Then_include_collection_order_by_collection_column(bool async)
        {
            return AssertFirstOrDefault(
                async,
                ss => ss.Set<Customer>()
                    .Include(c => c.Orders)
                    .ThenInclude(o => o.OrderDetails)
                    .Where(c => c.CustomerID.StartsWith("W"))
                    .OrderByDescending(c => c.Orders.OrderByDescending(oo => oo.OrderDate).FirstOrDefault().OrderDate),
                asserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<Customer>(c => c.Orders),
                    new ExpectedInclude<Order>(o => o.OrderDetails, "Orders")),
                entryCount: 55);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Then_include_property_expression_invalid(bool async)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<Customer>()
                        .Include(o => o.Orders)
                        .ThenInclude(o => new { o.Customer, o.OrderDetails })));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_closes_reader(bool async)
        {
            using var context = CreateContext();
            if (async)
            {
                Assert.NotNull(await context.Set<Customer>().Include(c => c.Orders).FirstOrDefaultAsync());
                Assert.NotNull(await context.Set<Product>().ToListAsync());
            }
            else
            {
                Assert.NotNull(context.Set<Customer>().Include(c => c.Orders).FirstOrDefault());
                Assert.NotNull(context.Set<Product>().ToList());
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_when_result_operator(bool async)
        {
            return AssertAny(
                async,
                ss => ss.Set<Customer>().Include(c => c.Orders));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Include(c => c.Orders),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 921);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_then_reference(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Product>().Include(p => p.OrderDetails).ThenInclude(od => od.Order),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<Product>(p => p.OrderDetails),
                    new ExpectedInclude<OrderDetail>(od => od.Order, "OrderDetails")),
                entryCount: 3062);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_last(bool async)
        {
            return AssertLast(
                async,
                ss => ss.Set<Customer>().Include(c => c.Orders).OrderBy(c => c.CompanyName),
                asserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 8);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_last_no_orderby(bool async)
        {
            return AssertLast(
                async,
                ss => ss.Set<Customer>().Include(c => c.Orders),
                entryCount: 8);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_skip_no_order_by(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Skip(10).Include(c => c.Orders),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 811);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_take_no_order_by(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Take(10).Include(c => c.Orders),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 110);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_skip_take_no_order_by(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Skip(10).Take(5).Include(c => c.Orders),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 35);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_list(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Product>().Include(p => p.OrderDetails).ThenInclude(od => od.Order),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<Product>(p => p.OrderDetails),
                    new ExpectedInclude<OrderDetail>(od => od.Order, "OrderDetails")),
                entryCount: 3062);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_alias_generation(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Include(o => o.OrderDetails),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Order>(o => o.OrderDetails)),
                entryCount: 2985);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_and_reference(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Include(o => o.OrderDetails).Include(o => o.Customer),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<Order>(o => o.OrderDetails), new ExpectedInclude<Order>(o => o.Customer)),
                entryCount: 3074);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_orderby_take(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(5).Include(c => c.Orders),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 53);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_collection_dependent_already_tracked(bool async)
        {
            using var context = CreateContext();
            var orders = context.Set<Order>().Where(o => o.CustomerID == "ALFKI").ToList();
            Assert.Equal(6, context.ChangeTracker.Entries().Count());

            var customer
                = async
                    ? await context.Set<Customer>()
                        .Include(c => c.Orders)
                        .SingleAsync(c => c.CustomerID == "ALFKI")
                    : context.Set<Customer>()
                        .Include(c => c.Orders)
                        .Single(c => c.CustomerID == "ALFKI");

            Assert.Equal(orders, customer.Orders, LegacyReferenceEqualityComparer.Instance);
            Assert.Equal(6, customer.Orders.Count);
            Assert.True(orders.All(o => ReferenceEquals(o.Customer, customer)));
            Assert.Equal(6 + 1, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_on_additional_from_clause(bool async)
        {
            return AssertQuery(
                async,
                ss => from c1 in ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(5)
                      from c2 in ss.Set<Customer>().Include(c2 => c2.Orders)
                      select c2,
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 921);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_on_additional_from_clause_with_filter(bool async)
        {
            return AssertQuery(
                async,
                ss => from c1 in ss.Set<Customer>()
                      from c2 in ss.Set<Customer>().Include(c => c.Orders).Where(c => c.CustomerID == "ALFKI")
                      select c2,
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 7);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_on_additional_from_clause2(bool async)
        {
            return AssertQuery(
                async,
                ss => from c1 in ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(5)
                      from c2 in ss.Set<Customer>().Include(c2 => c2.Orders)
                      select c1,
                entryCount: 5);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_where_skip_take_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>()
                    .Include(od => od.Order)
                    .Where(od => od.Quantity == 10)
                    .OrderBy(od => od.OrderID)
                    .ThenBy(od => od.ProductID)
                    .Skip(1)
                    .Take(2)
                    .Select(od => new { od.Order.CustomerID }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_join_clause_with_filter(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>().Include(c => c.Orders)
                      join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                      where c.CustomerID.StartsWith("F")
                      select c,
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 70);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_left_join_clause_with_filter(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>().Include(c => c.Orders)
                      join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into grouping
                      from o in grouping.DefaultIfEmpty()
                      where c.CustomerID.StartsWith("F")
                      select c,
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 71);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_cross_join_clause_with_filter(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>().Include(c => c.Orders)
                      from o in ss.Set<Order>().OrderBy(o => o.OrderID).Take(5)
                      where c.CustomerID.StartsWith("F")
                      select c,
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 71);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_cross_apply_with_filter(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>().Include(c => c.Orders)
                      from o in ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).OrderBy(o => c.CustomerID).Take(5)
                      where c.CustomerID.StartsWith("F")
                      select c,
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 70);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_outer_apply_with_filter(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>().Include(c => c.Orders)
                      from o in ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID)
                          .OrderBy(o => c.CustomerID).Take(5).DefaultIfEmpty()
                      where c.CustomerID.StartsWith("F")
                      select c,
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 71);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_outer_apply_with_filter_non_equality(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>().Include(c => c.Orders)
                      from o in ss.Set<Order>().Where(o => o.CustomerID != c.CustomerID)
                          .OrderBy(o => c.CustomerID).Take(5).DefaultIfEmpty()
                      where c.CustomerID.StartsWith("F")
                      select c,
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 71);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_on_join_clause_with_order_by_and_filter(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>().Include(c => c.Orders)
                      join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                      where c.CustomerID == "ALFKI"
                      orderby c.City
                      select c,
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                assertOrder: true,
                entryCount: 7);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_order_by_collection_column(bool async)
        {
            return AssertFirstOrDefault(
                async,
                ss => ss.Set<Customer>()
                    .Include(c => c.Orders)
                    .Where(c => c.CustomerID.StartsWith("W"))
                    .OrderByDescending(c => c.Orders.OrderByDescending(oo => oo.OrderDate).FirstOrDefault().OrderDate),
                asserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 15);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_order_by_key(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Include(c => c.Orders).OrderBy(c => c.CustomerID),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                assertOrder: true,
                entryCount: 921);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_order_by_non_key(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Include(c => c.Orders).OrderBy(c => c.PostalCode),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                assertOrder: true,
                entryCount: 921);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_order_by_non_key_with_take(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Include(c => c.Orders).OrderBy(c => c.ContactTitle).Take(10),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 126);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_order_by_non_key_with_skip(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Include(c => c.Orders).OrderBy(c => c.ContactTitle).Skip(10),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 795);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_order_by_non_key_with_first_or_default(bool async)
        {
            return AssertFirstOrDefault(
                async,
                ss => ss.Set<Customer>().Include(c => c.Orders).OrderByDescending(c => c.CompanyName),
                asserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 8);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_order_by_subquery(bool async)
        {
            return AssertFirstOrDefault(
                async,
                ss => ss.Set<Customer>()
                    .Include(c => c.Orders)
                    .Where(c => c.CustomerID == "ALFKI")
                    .OrderBy(c => c.Orders.OrderBy(o => o.EmployeeID).Select(o => o.OrderDate).FirstOrDefault()),
                asserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 7);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_collection_principal_already_tracked(bool async)
        {
            using var context = CreateContext();
            var customer1 = context.Set<Customer>().Single(c => c.CustomerID == "ALFKI");
            Assert.Single(context.ChangeTracker.Entries());

            var customer2
                = async
                    ? await context.Set<Customer>()
                        .Include(c => c.Orders)
                        .SingleAsync(c => c.CustomerID == "ALFKI")
                    : context.Set<Customer>()
                        .Include(c => c.Orders)
                        .Single(c => c.CustomerID == "ALFKI");

            Assert.Same(customer1, customer2);
            Assert.Equal(6, customer2.Orders.Count);
            Assert.True(customer2.Orders.All(o => o.Customer != null));
            Assert.Equal(7, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_single_or_default_no_result(bool async)
        {
            return AssertSingleOrDefault(
                async,
                ss => ss.Set<Customer>().Include(c => c.Orders),
                c => c.CustomerID == "ALFKI ?");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_when_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Include("Orders").Select(c => c.CustomerID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_filter(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Include(c => c.Orders).Where(c => c.CustomerID == "ALFKI"),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 7);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_filter_reordered(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI").Include(c => c.Orders),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 7);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_duplicate_collection(bool async)
        {
            return AssertQuery(
                async,
                ss => from c1 in ss.Set<Customer>().Include(c => c.Orders).OrderBy(c => c.CustomerID).Take(2)
                      from c2 in ss.Set<Customer>().Include(c => c.Orders).OrderBy(c => c.CustomerID).Skip(2).Take(2)
                      select new { c1, c2 },
                elementSorter: e => (e.c1.CustomerID, e.c2.CustomerID),
                elementAsserter: (e, a) =>
                {
                    AssertInclude(e.c1, a.c1, new ExpectedInclude<Customer>(c => c.Orders));
                    AssertInclude(e.c2, a.c2, new ExpectedInclude<Customer>(c => c.Orders));
                },
                entryCount: 34);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_duplicate_collection_result_operator(bool async)
        {
            return AssertQuery(
                async,
                ss => (from c1 in ss.Set<Customer>().Include(c => c.Orders).OrderBy(c => c.CustomerID).Take(2)
                       from c2 in ss.Set<Customer>().Include(c => c.Orders).OrderBy(c => c.CustomerID).Skip(2).Take(2)
                       select new { c1, c2 }).Take(1),
                elementSorter: e => (e.c1.CustomerID, e.c2.CustomerID),
                elementAsserter: (e, a) =>
                {
                    AssertInclude(e.c1, a.c1, new ExpectedInclude<Customer>(c => c.Orders));
                    AssertInclude(e.c2, a.c2, new ExpectedInclude<Customer>(c => c.Orders));
                },
                entryCount: 15);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_duplicate_collection_result_operator2(bool async)
        {
            return AssertQuery(
                async,
                ss => (from c1 in ss.Set<Customer>().Include(c => c.Orders).OrderBy(c => c.CustomerID).Take(2)
                       from c2 in ss.Set<Customer>().OrderBy(c => c.CustomerID).Skip(2).Take(2)
                       select new { c1, c2 }).Take(1),
                elementSorter: e => (e.c1.CustomerID, e.c2.CustomerID),
                elementAsserter: (e, a) =>
                {
                    AssertInclude(e.c1, a.c1, new ExpectedInclude<Customer>(c => c.Orders));
                    AssertEqual(e.c2, a.c2);
                },
                entryCount: 8);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_duplicate_reference(bool async)
        {
            return AssertQuery(
                async,
                ss => from o1 in ss.Set<Order>().Include(o => o.Customer).OrderBy(o => o.CustomerID).ThenBy(o => o.OrderID).Take(2)
                      from o2 in ss.Set<Order>().Include(o => o.Customer).OrderBy(o => o.CustomerID).ThenBy(o => o.OrderID).Skip(2).Take(2)
                      select new { o1, o2 },
                elementSorter: e => (e.o1.OrderID, e.o2.OrderID),
                elementAsserter: (e, a) =>
                {
                    AssertInclude(e.o1, a.o1, new ExpectedInclude<Order>(c => c.Customer));
                    AssertInclude(e.o2, a.o2, new ExpectedInclude<Order>(c => c.Customer));
                },
                entryCount: 5);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_duplicate_reference2(bool async)
        {
            return AssertQuery(
                async,
                ss => from o1 in ss.Set<Order>().Include(o => o.Customer).OrderBy(o => o.OrderID).Take(2)
                      from o2 in ss.Set<Order>().OrderBy(o => o.OrderID).Skip(2).Take(2)
                      select new { o1, o2 },
                elementSorter: e => (e.o1.OrderID, e.o2.OrderID),
                elementAsserter: (e, a) =>
                {
                    AssertInclude(e.o1, a.o1, new ExpectedInclude<Order>(c => c.Customer));
                    AssertEqual(e.o2, a.o2);
                },
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_duplicate_reference3(bool async)
        {
            return AssertQuery(
                async,
                ss => from o1 in ss.Set<Order>().OrderBy(o => o.OrderID).Take(2)
                      from o2 in ss.Set<Order>().OrderBy(o => o.OrderID).Include(o => o.Customer).Skip(2).Take(2)
                      select new { o1, o2 },
                elementSorter: e => (e.o1.OrderID, e.o2.OrderID),
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.o1, a.o1);
                    AssertInclude(e.o2, a.o2, new ExpectedInclude<Order>(c => c.Customer));
                },
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_collection_with_client_filter(bool async)
        {
            Assert.Contains(
                CoreStrings.TranslationFailedWithDetails(
                    "",
                    CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer))).Substring(21),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertQuery(
                        async,
                        ss => ss.Set<Customer>().Include(c => c.Orders).Where(c => c.IsLondon))))
                .Message.Replace("\r", "").Replace("\n", ""));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_multi_level_reference_and_collection_predicate(bool async)
        {
            return AssertSingle(
                async,
                ss => ss.Set<Order>().Include(o => o.Customer.Orders),
                o => o.OrderID == 10248,
                asserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<Order>(o => o.Customer),
                    new ExpectedInclude<Customer>(c => c.Orders, "Customer")),
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_multi_level_collection_and_then_include_reference_predicate(bool async)
        {
            return AssertSingle(
                async,
                ss => ss.Set<Order>().Include(o => o.OrderDetails).ThenInclude(od => od.Product),
                o => o.OrderID == 10248,
                asserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<Order>(o => o.OrderDetails),
                    new ExpectedInclude<OrderDetail>(od => od.Product, "OrderDetails")),
                entryCount: 7);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_multiple_references(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Include(o => o.Order).Include(o => o.Product),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<OrderDetail>(od => od.Order),
                    new ExpectedInclude<OrderDetail>(od => od.Product)),
                entryCount: 3062);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_multiple_references_and_collection_multi_level(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Include(od => od.Order.Customer.Orders).Include(od => od.Product),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<OrderDetail>(od => od.Order),
                    new ExpectedInclude<Order>(o => o.Customer, "Order"),
                    new ExpectedInclude<Customer>(c => c.Orders, "Order.Customer"),
                    new ExpectedInclude<OrderDetail>(od => od.Product)),
                entryCount: 3151);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_multiple_references_and_collection_multi_level_reverse(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Include(od => od.Product).Include(od => od.Order.Customer.Orders),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<OrderDetail>(od => od.Order),
                    new ExpectedInclude<Order>(o => o.Customer, "Order"),
                    new ExpectedInclude<Customer>(c => c.Orders, "Order.Customer"),
                    new ExpectedInclude<OrderDetail>(od => od.Product)),
                entryCount: 3151);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_multiple_references_multi_level(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Include(o => o.Order.Customer).Include(o => o.Product),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<OrderDetail>(od => od.Order),
                    new ExpectedInclude<Order>(o => o.Customer, "Order"),
                    new ExpectedInclude<OrderDetail>(od => od.Product)),
                entryCount: 3151);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_multiple_references_multi_level_reverse(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Include(o => o.Product).Include(o => o.Order.Customer),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<OrderDetail>(od => od.Order),
                    new ExpectedInclude<Order>(o => o.Customer, "Order"),
                    new ExpectedInclude<OrderDetail>(od => od.Product)),
                entryCount: 3151);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Include(o => o.Customer),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Order>(o => o.Customer)),
                entryCount: 919);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_reference_alias_generation(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Include(o => o.Order),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<OrderDetail>(od => od.Order)),
                entryCount: 2985);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_and_collection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Include(o => o.Customer).Include(o => o.OrderDetails),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<Order>(o => o.Customer),
                    new ExpectedInclude<Order>(o => o.OrderDetails)),
                entryCount: 3074);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_force_alias_uniquefication(bool async)
        {
            return AssertQuery(
                async,
                ss => from o in ss.Set<Order>().Include(o => o.OrderDetails)
                      where o.CustomerID == "ALFKI"
                      select o,
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Order>(o => o.OrderDetails)),
                entryCount: 18);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_reference_dependent_already_tracked(bool async)
        {
            using var context = CreateContext();
            var customer = context.Set<Customer>().Single(o => o.CustomerID == "ALFKI");
            Assert.Single(context.ChangeTracker.Entries());

            var orders
                = async
                    ? await context.Set<Order>().Include(o => o.Customer).Where(o => o.CustomerID == "ALFKI").ToListAsync()
                    : context.Set<Order>().Include(o => o.Customer).Where(o => o.CustomerID == "ALFKI").ToList();

            Assert.Equal(6, orders.Count);
            Assert.True(orders.All(o => ReferenceEquals(o.Customer, customer)));
            Assert.Equal(7, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_single_or_default_when_no_result(bool async)
        {
            return AssertSingleOrDefault(
                async,
                ss => ss.Set<Order>().Include(o => o.Customer),
                o => o.OrderID == -1,
                asserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Order>(o => o.Customer)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_when_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Include(o => o.Customer).Select(o => o.CustomerID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_when_entity_in_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Include(o => o.Customer).Select(o => new { o, o.CustomerID }),
                elementSorter: e => e.o.OrderID,
                elementAsserter: (e, a) =>
                {
                    AssertInclude(e.o, a.o, new ExpectedInclude<Order>(o => o.Customer));
                    AssertEqual(e.CustomerID, a.CustomerID);
                },
                entryCount: 919);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_with_filter(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Include(o => o.Customer).Where(o => o.CustomerID == "ALFKI"),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Order>(o => o.Customer)),
                entryCount: 7);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_with_filter_reordered(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.CustomerID == "ALFKI").Include(o => o.Customer),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Order>(o => o.Customer)),
                entryCount: 7);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_references_and_collection_multi_level(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Include(o => o.Order.Customer.Orders),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<OrderDetail>(od => od.Order),
                    new ExpectedInclude<Order>(o => o.Customer, "Order"),
                    new ExpectedInclude<Customer>(c => c.Orders, "Order.Customer")),
                entryCount: 3074);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_then_include_collection(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Include(c => c.Orders).ThenInclude(o => o.OrderDetails),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<Customer>(c => c.Orders),
                    new ExpectedInclude<Order>(o => o.OrderDetails, "Orders")),
                entryCount: 3076);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_then_include_collection_then_include_reference(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Include(c => c.Orders).ThenInclude(o => o.OrderDetails).ThenInclude(od => od.Product),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<Customer>(c => c.Orders),
                    new ExpectedInclude<Order>(o => o.OrderDetails, "Orders"),
                    new ExpectedInclude<OrderDetail>(od => od.Product, "Orders.OrderDetails")),
                entryCount: 3153);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_then_include_collection_predicate(bool async)
        {
            return AssertSingleOrDefault(
                async,
                ss => ss.Set<Customer>().Include(c => c.Orders).ThenInclude(o => o.OrderDetails),
                c => c.CustomerID == "ALFKI",
                asserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<Customer>(c => c.Orders),
                    new ExpectedInclude<Order>(o => o.OrderDetails, "Orders")),
                entryCount: 19);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_references_and_collection_multi_level_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Include(od => od.Order.Customer.Orders).Where(od => od.OrderID == 10248),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<OrderDetail>(od => od.Order),
                    new ExpectedInclude<Order>(o => o.Customer, "Order"),
                    new ExpectedInclude<Customer>(c => c.Orders, "Order.Customer")),
                entryCount: 9);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_references_multi_level(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Include(o => o.Order.Customer),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<OrderDetail>(od => od.Order),
                    new ExpectedInclude<Order>(o => o.Customer, "Order")),
                entryCount: 3074);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_multi_level_reference_then_include_collection_predicate(bool async)
        {
            return AssertSingle(
                async,
                ss => ss.Set<Order>().Include(o => o.Customer).ThenInclude(c => c.Orders),
                o => o.OrderID == 10248,
                asserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<Order>(o => o.Customer),
                    new ExpectedInclude<Customer>(c => c.Orders, "Customer")),
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_multiple_references_then_include_collection_multi_level(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>()
                    .Include(od => od.Order).ThenInclude(o => o.Customer).ThenInclude(c => c.Orders)
                    .Include(od => od.Product),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<OrderDetail>(od => od.Order),
                    new ExpectedInclude<Order>(o => o.Customer, "Order"),
                    new ExpectedInclude<Customer>(c => c.Orders, "Order.Customer"),
                    new ExpectedInclude<OrderDetail>(od => od.Product)),
                entryCount: 3151);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_multiple_references_then_include_collection_multi_level_reverse(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>()
                    .Include(od => od.Product)
                    .Include(od => od.Order).ThenInclude(o => o.Customer).ThenInclude(c => c.Orders),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<OrderDetail>(od => od.Order),
                    new ExpectedInclude<Order>(o => o.Customer, "Order"),
                    new ExpectedInclude<Customer>(c => c.Orders, "Order.Customer"),
                    new ExpectedInclude<OrderDetail>(od => od.Product)),
                entryCount: 3151);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_multiple_references_then_include_multi_level(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>()
                    .Include(od => od.Order).ThenInclude(o => o.Customer)
                    .Include(od => od.Product),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<OrderDetail>(od => od.Order),
                    new ExpectedInclude<Order>(o => o.Customer, "Order"),
                    new ExpectedInclude<OrderDetail>(od => od.Product)),
                entryCount: 3151);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_multiple_references_then_include_multi_level_reverse(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>()
                    .Include(od => od.Product)
                    .Include(od => od.Order).ThenInclude(o => o.Customer),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<OrderDetail>(od => od.Order),
                    new ExpectedInclude<Order>(o => o.Customer, "Order"),
                    new ExpectedInclude<OrderDetail>(od => od.Product)),
                entryCount: 3151);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_references_then_include_collection_multi_level(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>()
                    .Include(od => od.Order).ThenInclude(o => o.Customer).ThenInclude(c => c.Orders),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<OrderDetail>(od => od.Order),
                    new ExpectedInclude<Order>(o => o.Customer, "Order"),
                    new ExpectedInclude<Customer>(c => c.Orders, "Order.Customer")),
                entryCount: 3074);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_references_then_include_collection_multi_level_predicate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>()
                    .Include(od => od.Order).ThenInclude(o => o.Customer).ThenInclude(c => c.Orders)
                    .Where(od => od.OrderID == 10248),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<OrderDetail>(od => od.Order),
                    new ExpectedInclude<Order>(o => o.Customer, "Order"),
                    new ExpectedInclude<Customer>(c => c.Orders, "Order.Customer")),
                entryCount: 9);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_references_then_include_multi_level(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>()
                    .Include(od => od.Order).ThenInclude(o => o.Customer),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedInclude<OrderDetail>(od => od.Order),
                    new ExpectedInclude<Order>(o => o.Customer, "Order")),
                entryCount: 3074);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_complex_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => from o in ss.Set<Order>().Include(o => o.Customer)
                      select new { CustomerId = new { Id = o.Customer.CustomerID } });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_complex_projection_does_not_change_ordering_of_projection(bool async)
        {
            return AssertQuery(
                async,
                ss => (from c in ss.Set<Customer>().Include(c => c.Orders).Where(c => c.ContactTitle == "Owner").OrderBy(c => c.CustomerID)
                       select new { Id = c.CustomerID, TotalOrders = c.Orders.Count })
                    .Where(e => e.TotalOrders > 2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_take(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().OrderByDescending(c => c.ContactName).Include(c => c.Orders).Take(10),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 75);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_skip(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Include(c => c.Orders).OrderBy(c => c.ContactName).Skip(80),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 106);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_multiple_conditional_order_by(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .Include(c => c.OrderDetails)
                    .OrderBy(o => o.OrderID > 0)
                    .ThenBy(o => o.Customer != null ? o.Customer.City : string.Empty)
                    .Take(5),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Order>(o => o.OrderDetails)),
                entryCount: 14);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_with_conditional_order_by(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Include(c => c.Orders).OrderBy(c => c.CustomerID.StartsWith("S") ? 1 : 2),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                assertOrder: true,
                entryCount: 921);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_specified_on_non_entity_not_supported(bool async)
        {
            Assert.Equal(
                CoreStrings.IncludeOnNonEntity("t => t.Item1.Orders"),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertQuery(
                        async,
                        ss => ss.Set<Customer>().Select(c => new Tuple<Customer, int>(c, 5)).Include(t => t.Item1.Orders)))).Message);
        }

        [ConditionalTheory(Skip = "Issue #12088")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_GroupBy_Select(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .Where(o => o.OrderID == 10248)
                    .Include(o => o.OrderDetails)
                    .GroupBy(e => e.OrderID)
                    .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault()));
        }

        [ConditionalTheory(Skip = "Issue #12088")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_GroupBy_Select(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .Where(o => o.OrderID == 10248)
                    .Include(o => o.Customer)
                    .GroupBy(e => e.OrderID)
                    .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault()));
        }

        [ConditionalTheory(Skip = "Issue #12088")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_Join_GroupBy_Select(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .Where(o => o.OrderID == 10248)
                    .Include(o => o.OrderDetails)
                    .Join(
                        ss.Set<OrderDetail>(),
                        o => o.OrderID,
                        od => od.OrderID,
                        (o, od) => o)
                    .GroupBy(e => e.OrderID)
                    .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault()));
        }

        [ConditionalTheory(Skip = "Issue #12088")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_Join_GroupBy_Select(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .Where(o => o.OrderID == 10248)
                    .Include(o => o.Customer)
                    .Join(
                        ss.Set<OrderDetail>(),
                        o => o.OrderID,
                        od => od.OrderID,
                        (o, od) => o)
                    .GroupBy(e => e.OrderID)
                    .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault()));
        }

        [ConditionalTheory(Skip = "Issue #12088")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_Include_collection_GroupBy_Select(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>()
                    .Where(od => od.OrderID == 10248)
                    .Join(
                        ss.Set<Order>().Include(o => o.OrderDetails),
                        od => od.OrderID,
                        o => o.OrderID,
                        (od, o) => o)
                    .GroupBy(e => e.OrderID)
                    .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault()));
        }

        [ConditionalTheory(Skip = "Issue #12088")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_Include_reference_GroupBy_Select(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>()
                    .Join(
                        ss.Set<Order>().Include(o => o.Customer),
                        od => od.OrderID,
                        o => o.OrderID,
                        (od, o) => o)
                    .GroupBy(e => e.OrderID)
                    .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault()));
        }

        [ConditionalTheory(Skip = "Issue #12088")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_SelectMany_GroupBy_Select(bool async)
        {
            return AssertQuery(
                async,
                ss => (from o in ss.Set<Order>().Include(o => o.OrderDetails).Where(o => o.OrderID == 10248)
                       from od in ss.Set<OrderDetail>()
                       select o)
                    .GroupBy(e => e.OrderID)
                    .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault()));
        }

        [ConditionalTheory(Skip = "Issue #12088")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_SelectMany_GroupBy_Select(bool async)
        {
            return AssertQuery(
                async,
                ss => (from o in ss.Set<Order>().Include(o => o.Customer).Where(o => o.OrderID == 10248)
                       from od in ss.Set<OrderDetail>()
                       select o)
                    .GroupBy(e => e.OrderID)
                    .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault()));
        }

        [ConditionalTheory(Skip = "Issue #12088")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_Include_collection_GroupBy_Select(bool async)
        {
            return AssertQuery(
                async,
                ss => (from od in ss.Set<OrderDetail>().Where(od => od.OrderID == 10248)
                       from o in ss.Set<Order>().Include(o => o.OrderDetails)
                       select o)
                    .GroupBy(e => e.OrderID)
                    .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault()));
        }

        [ConditionalTheory(Skip = "Issue #12088")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SelectMany_Include_reference_GroupBy_Select(bool async)
        {
            return AssertQuery(
                async,
                ss => (from od in ss.Set<OrderDetail>().Where(od => od.OrderID == 10248)
                       from o in ss.Set<Order>().Include(o => o.Customer)
                       select o)
                    .GroupBy(e => e.OrderID)
                    .Select(e => e.OrderBy(o => o.OrderID).FirstOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_reference_distinct_is_server_evaluated(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID < 10250).Include(o => o.Customer).Distinct(),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Order>(o => o.Customer)),
                entryCount: 4);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_distinct_is_server_evaluated(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .Include(o => o.Orders)
                    .Distinct(),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 34);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_OrderBy_object(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .Where(o => o.OrderID < 10250)
                    .Include(o => o.OrderDetails)
                    .OrderBy<Order, object>(c => c.OrderID),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Order>(o => o.OrderDetails)),
                assertOrder: true,
                entryCount: 7);
        }

        [ConditionalTheory(Skip = "Issue#15713")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_OrderBy_empty_list_contains(bool async)
        {
            var list = new List<string>();
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Include(c => c.Orders)
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .OrderBy(c => list.Contains(c.CustomerID))
                    .Skip(1),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)));
        }

        [ConditionalTheory(Skip = "Issue#15713")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_OrderBy_empty_list_does_not_contains(bool async)
        {
            var list = new List<string>();
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Include(c => c.Orders)
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .OrderBy(c => !list.Contains(c.CustomerID))
                    .Skip(1),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_OrderBy_list_contains(bool async)
        {
            var list = new List<string> { "ALFKI" };
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Include(c => c.Orders)
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .OrderBy(c => list.Contains(c.CustomerID))
                    .Skip(1),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 29);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_collection_OrderBy_list_does_not_contains(bool async)
        {
            var list = new List<string> { "ALFKI" };
            return AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Include(c => c.Orders)
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .OrderBy(c => !list.Contains(c.CustomerID))
                    .Skip(1),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(c => c.Orders)),
                entryCount: 27);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_empty_reference_sets_IsLoaded(bool async)
        {
            return AssertFirst(
                async,
                ss => ss.Set<Employee>().Include(e => e.Manager),
                e => e.Manager == null,
                asserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Employee>(emp => emp.Manager)),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_is_not_ignored_when_projection_contains_client_method_and_complex_expression(bool async)
        {
            return AssertQuery(
                async,
                ss => from e in ss.Set<Employee>().Include(e => e.Manager)
                      where e.EmployeeID == 1 || e.EmployeeID == 2
                      orderby e.EmployeeID
                      select e.Manager != null ? "Employee " + ClientMethod(e) : "",
                entryCount: 2);
        }

        private static string ClientMethod(Employee e)
            => e.FirstName + " reports to " + e.Manager.FirstName;

        // Issue#18672
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multi_level_includes_are_applied_with_skip(bool async)
        {
            return AssertFirst(
                async,
                ss => (from c in ss.Set<Customer>().Include(e => e.Orders).ThenInclude(e => e.OrderDetails)
                       where c.CustomerID.StartsWith("A")
                       orderby c.CustomerID
                       select new { c.CustomerID, Orders = c.Orders.ToList() }).Skip(1),
                asserter: (e, a) =>
                {
                    AssertEqual(e.CustomerID, a.CustomerID);
                    AssertCollection(
                        e.Orders, a.Orders,
                        elementAsserter: (eo, ao) => AssertInclude(eo, ao, new ExpectedInclude<Order>(o => o.OrderDetails)));
                },
                entryCount: 14);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multi_level_includes_are_applied_with_take(bool async)
        {
            return AssertFirst(
                async,
                ss => (from c in ss.Set<Customer>().Include(e => e.Orders).ThenInclude(e => e.OrderDetails)
                       where c.CustomerID.StartsWith("A")
                       orderby c.CustomerID
                       select new { c.CustomerID, Orders = c.Orders.ToList() }).Take(1),
                asserter: (e, a) =>
                {
                    AssertEqual(e.CustomerID, a.CustomerID);
                    AssertCollection(
                        e.Orders, a.Orders,
                        elementAsserter: (eo, ao) => AssertInclude(eo, ao, new ExpectedInclude<Order>(o => o.OrderDetails)));
                },
                entryCount: 18);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multi_level_includes_are_applied_with_skip_take(bool async)
        {
            return AssertFirst(
                async,
                ss => (from c in ss.Set<Customer>().Include(e => e.Orders).ThenInclude(e => e.OrderDetails)
                       where c.CustomerID.StartsWith("A")
                       orderby c.CustomerID
                       select new { c.CustomerID, Orders = c.Orders.ToList() }).Skip(1).Take(1),
                asserter: (e, a) =>
                {
                    AssertEqual(e.CustomerID, a.CustomerID);
                    AssertCollection(
                        e.Orders, a.Orders,
                        elementAsserter: (eo, ao) => AssertInclude(eo, ao, new ExpectedInclude<Order>(o => o.OrderDetails)));
                },
                entryCount: 14);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Filtered_include_with_multiple_ordering(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                    .Include(c => c.Orders.OrderBy(o => o.OrderID).Skip(1).OrderByDescending(o => o.OrderDate)),
                elementAsserter: (e, a) => AssertInclude(
                    e, a,
                    new ExpectedFilteredInclude<Customer, Order>(
                        c => c.Orders,
                        includeFilter: os => os.OrderBy(o => o.OrderID).Skip(1).OrderByDescending(o => o.OrderDate),
                        assertOrder: true)),
                entryCount: 64);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_cycle_does_not_throw_when_AsNoTrackingWithIdentityResolution(bool async)
        {
            return AssertQuery(
                async,
                ss => (from i in ss.Set<Order>().Include(o => o.Customer.Orders)
                       where i.OrderID < 10800
                       select i)
                    .AsNoTrackingWithIdentityResolution());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Outer_idenfier_correctly_determined_when_doing_include_on_right_side_of_left_join(bool async)
        {
            return AssertQuery(
                async,
                ss => from cust in ss.Set<Customer>()
                      join order in ss.Set<Order>().Include(f => f.OrderDetails)
                          on cust.CustomerID equals order.CustomerID into group1
                      from order in group1.DefaultIfEmpty()
                      where cust.City == "Seattle"
                      select new { cust, order },
                elementSorter: e => (e.cust.CustomerID, e.order?.OrderID),
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.cust, a.cust);
                    AssertInclude(e.order, a.order, new ExpectedInclude<Order>(e => e.OrderDetails));
                },
                entryCount: 55);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_in_let_followed_by_FirstOrDefault(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      let order = ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID)
                        .OrderBy(o => o.OrderDate)
                        .Include(o => o.OrderDetails)
                        .FirstOrDefault()
                      where c.CustomerID.StartsWith("F")
                      select new
                      {
                          c.CustomerID,
                          Order = order
                      },
                elementSorter: e => e.CustomerID,
                elementAsserter: (e, a) => AssertEqual(e.Order, a.Order),
                entryCount: 26);
        }

        protected virtual void ClearLog()
        {
        }

        protected NorthwindContext CreateContext()
            => Fixture.CreateContext();
    }
}
