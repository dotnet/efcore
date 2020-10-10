// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable StaticMemberInGenericType
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToExpressionBodyWhenPossible
// ReSharper disable ConvertMethodToExpressionBody
// ReSharper disable StringStartsWithIsCultureSpecific
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindQueryFiltersQueryTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NorthwindQueryFiltersCustomizer>, new()
    {
        protected NorthwindQueryFiltersQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Count_query(bool async)
        {
            return AssertCount(
                async,
                ss => ss.Set<Customer>());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Materialized_query(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>(),
                entryCount: 7);
        }

        [ConditionalFact]
        public virtual void Find()
        {
            using var context = Fixture.CreateContext();

            Assert.Null(context.Find<Customer>("ALFKI"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Client_eval(bool async)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("DbSet<Product>()    .Where(p => NorthwindContext.ClientMethod(p))"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertQuery(
                            async,
                            ss => ss.Set<Product>()))).Message));
        }

        [ConditionalFact]
        public virtual void Materialized_query_parameter()
        {
            using var context = Fixture.CreateContext();
            context.TenantPrefix = "F";

            Assert.Equal(8, context.Customers.ToList().Count);
        }

        [ConditionalFact]
        public virtual void Materialized_query_parameter_new_context()
        {
            using var context1 = Fixture.CreateContext();
            Assert.Equal(7, context1.Customers.ToList().Count);

            using var context2 = Fixture.CreateContext();
            context2.TenantPrefix = "T";

            Assert.Equal(6, context2.Customers.ToList().Count);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projection_query(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => c.CustomerID));
        }

        [ConditionalFact]
        public virtual void Projection_query_parameter()
        {
            using var context = Fixture.CreateContext();
            context.TenantPrefix = "F";

            Assert.Equal(8, context.Customers.Select(c => c.CustomerID).ToList().Count);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_query(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Include(c => c.Orders),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(x => x.Orders)),
                entryCount: 87);
        }

        [ConditionalFact]
        public virtual void Include_query_opt_out()
        {
            using var context = Fixture.CreateContext();
            var results = context.Customers.Include(c => c.Orders).IgnoreQueryFilters().ToList();

            Assert.Equal(91, results.Count);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Included_many_to_one_query2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Include(o => o.Customer),
                entryCount: 87);
        }

        [ConditionalFact]
        public virtual void Included_many_to_one_query()
        {
            using var context = Fixture.CreateContext();
            var results = context.Orders.Include(o => o.Customer).ToList();

            Assert.Equal(80, results.Count);
            Assert.True(results.All(o => o.Customer == null || o.CustomerID.StartsWith("B")));
        }

        [ConditionalFact]
        public virtual void Project_reference_that_itself_has_query_filter_with_another_reference()
        {
            using var context = Fixture.CreateContext();
            var results = context.OrderDetails.Select(od => od.Order).ToList();

            Assert.Equal(5, results.Count);
            Assert.True(results.All(o => o.Customer == null || o.CustomerID.StartsWith("B")));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_reference_that_itself_has_query_filter_with_another_reference2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Select(od => od.Order),
                // expected query doesn't distinguish between inner join and left join, so the filtered elements are returned as nulls
                ss => ss.Set<OrderDetail>().Select(od => od.Order).Where(x => x != null),
                entryCount: 5);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Included_one_to_many_query_with_client_eval(bool async)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("DbSet<Product>()    .Where(p => NorthwindContext.ClientMethod(p))"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertQuery(
                            async,
                            ss => ss.Set<Product>().Include(p => p.OrderDetails)))).Message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navs_query(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      from o in c.Orders
                      from od in o.OrderDetails
                      where od.Discount < 10
                      select c,
                entryCount: 3);
        }

        [ConditionalFact]
        public virtual void Compiled_query()
        {
            var query = EF.CompileQuery(
                (NorthwindContext context, string customerID)
                    => context.Customers.Where(c => c.CustomerID == customerID));

            using var context1 = Fixture.CreateContext();
            Assert.Equal("BERGS", query(context1, "BERGS").First().CustomerID);

            using var context2 = Fixture.CreateContext();
            Assert.Equal("BLAUS", query(context2, "BLAUS").First().CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Entity_Equality(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>(),
                entryCount: 80);
        }

        protected override Expression RewriteExpectedQueryExpression(Expression expectedQueryExpression, IModel model)
        {
            expectedQueryExpression = new QueryFiltersExpectedQueryRewritingVisitor(model).Visit(expectedQueryExpression);

            return base.RewriteExpectedQueryExpression(expectedQueryExpression, model);
        }

        private string RemoveNewLines(string message)
            => message.Replace("\n", "").Replace("\r", "");
    }
}
