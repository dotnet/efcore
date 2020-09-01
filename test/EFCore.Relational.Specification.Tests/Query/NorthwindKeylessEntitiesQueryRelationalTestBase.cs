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
    public abstract class NorthwindKeylessEntitiesQueryRelationalTestBase<TFixture> : NorthwindKeylessEntitiesQueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected NorthwindKeylessEntitiesQueryRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected virtual bool CanExecuteQueryString
            => false;

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Projecting_collection_correlated_with_keyless_entity_throws(bool async)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<CustomerQuery>().Select(
                        cq => new
                        {
                            cq.City,
                            cq.CompanyName,
                            OrderDetailIds = ss.Set<Customer>().Where(c => c.City == cq.City).ToList()
                        }).OrderBy(x => x.City).Take(2)))).Message;

            Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyOuterElementOfCollectionJoin, message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Collection_of_entities_projecting_correlated_collection_of_keyless_entities(bool async)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Select(c => new
                    {
                        c.City,
                        Collection = ss.Set<CustomerQuery>().Where(cq => cq.City == c.City).ToList(),
                    })))).Message;

            Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyOuterElementOfCollectionJoin, message);
        }

        protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
            => new RelationalQueryAsserter(
                fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression, canExecuteQueryString: CanExecuteQueryString);
    }
}
