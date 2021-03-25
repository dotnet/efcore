// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        public override async Task Complex_query_with_groupBy_in_subquery4(bool async)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Complex_query_with_groupBy_in_subquery4(async))).Message;

            Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyOuterElementOfCollectionJoin, message);
        }

        public override async Task Select_correlated_collection_after_GroupBy_aggregate_when_identifier_changes_to_complex(bool async)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Select_correlated_collection_after_GroupBy_aggregate_when_identifier_changes_to_complex(async))).Message;

            Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyOuterElementOfCollectionJoin, message);
        }

        protected virtual bool CanExecuteQueryString
            => false;

        protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
            => new RelationalQueryAsserter(
                fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression, canExecuteQueryString: CanExecuteQueryString);
    }
}
