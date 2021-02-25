// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindGroupByQueryRelationalTestBase<TFixture> : NorthwindGroupByQueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected NorthwindGroupByQueryRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Select_uncorrelated_collection_with_groupby_when_outer_is_distinct(bool async)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .Select(c => c.Customer.City)
                    .Distinct()
                    .Select(c => new
                    {
                        c1 = ss.Set<Product>().GroupBy(p => p.ProductID).Select(g => g.Key).ToArray(),
                        c2 = ss.Set<Product>().GroupBy(p => p.ProductID).Select(g => g.Count()).ToArray()
                    }),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    AssertCollection(e.c1, a.c1);
                    AssertCollection(e.c2, a.c2);
                }))).Message;

            Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyOuterElementOfCollectionJoin, message);
        }


        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Complex_query_with_groupBy_in_subquery4(bool async)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .Select(
                        c => new
                        {
                            Key = c.CustomerID,
                            Subquery = c.Orders
                                .Select(o => new { First = o.OrderID, Second = o.Customer.City + o.CustomerID })
                                .GroupBy(x => x.Second)
                                .Select(g => new { Sum = g.Sum(x => x.First), Count = g.Count(x => x.Second.StartsWith("Lon")) }).ToList()
                        }),
                elementSorter: e => e.Key,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Key, a.Key);
                    AssertCollection(e.Subquery, a.Subquery);
                }))).Message;

            Assert.Equal(RelationalStrings.MissingIdentifyingProjectionInDistinctGroupBySubquery("o.OrderID"), message);
        }


        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Select_nested_collection_with_distinct(bool async)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                async,
                ss => ss.Set<Customer>()
                    .OrderBy(c => c.CustomerID)
                    .Where(c => c.CustomerID.StartsWith("A"))
                    .Select(
                        c => c.Orders.Any()
                            ? c.Orders.Select(o => o.CustomerID).Distinct().ToArray()
                            : Array.Empty<string>()),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a)))).Message;

            Assert.Equal(RelationalStrings.MissingIdentifyingProjectionInDistinctGroupBySubquery("o.OrderID"), message);
        }

        protected virtual bool CanExecuteQueryString
            => false;

        protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
            => new RelationalQueryAsserter(
                fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression, canExecuteQueryString: CanExecuteQueryString);
    }
}
