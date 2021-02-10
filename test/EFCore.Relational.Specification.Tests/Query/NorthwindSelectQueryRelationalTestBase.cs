// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindSelectQueryRelationalTestBase<TFixture> : NorthwindSelectQueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected NorthwindSelectQueryRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
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
