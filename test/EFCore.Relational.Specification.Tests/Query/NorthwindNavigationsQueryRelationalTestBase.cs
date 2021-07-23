﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindNavigationsQueryRelationalTestBase<TFixture> : NorthwindNavigationsQueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected NorthwindNavigationsQueryRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        public override Task Where_subquery_on_navigation_client_eval(bool async)
        {
            return AssertTranslationFailed(() => base.Where_subquery_on_navigation_client_eval(async));
        }

        protected virtual bool CanExecuteQueryString
            => false;

        protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
            => new RelationalQueryAsserter(
                fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression, canExecuteQueryString: CanExecuteQueryString);
    }
}
