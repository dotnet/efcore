// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindSelectQueryRelationalTestBase<TFixture> : NorthwindSelectQueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected NorthwindSelectQueryRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        public override async Task Correlated_collection_after_groupby_with_complex_projection_not_containing_original_identifier(bool async)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
              () => base.Correlated_collection_after_groupby_with_complex_projection_not_containing_original_identifier(async))).Message;

            Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin, message);
        }

        public override Task Select_bool_closure_with_order_by_property_with_cast_to_nullable(bool async)
        {
            return AssertTranslationFailed(() => base.Select_bool_closure_with_order_by_property_with_cast_to_nullable(async));
        }

        public override Task Reverse_without_explicit_ordering(bool async)
        {
            return AssertTranslationFailedWithDetails(
                () => base.Reverse_without_explicit_ordering(async), RelationalStrings.MissingOrderingInSelectExpression);
        }

        protected virtual bool CanExecuteQueryString
            => false;

        protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
            => new RelationalQueryAsserter(
                fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression, canExecuteQueryString: CanExecuteQueryString);
    }
}
