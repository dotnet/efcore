// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindMiscellaneousQueryRelationalTestBase<TFixture> : NorthwindMiscellaneousQueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected NorthwindMiscellaneousQueryRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projecting_collection_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")).OrderBy(e => e.CustomerID).AsSplitQuery()
                    .Select(c => c.Orders),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a),
                entryCount: 63);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projecting_collection_then_include_split(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                    .Include(c => c.Orders).ThenInclude(o => o.OrderDetails)
                    .OrderBy(e => e.CustomerID).AsSplitQuery().Select(c => c.Orders),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(
                    e, a,
                    elementAsserter: (eo, ao) => AssertInclude(eo, ao, new ExpectedInclude<Order>(o => o.OrderDetails))),
                entryCount: 227);
        }

        protected virtual bool CanExecuteQueryString
            => false;

        protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
            => new RelationalQueryAsserter(
                fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression, canExecuteQueryString: CanExecuteQueryString);
    }
}
