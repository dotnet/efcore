// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindWhereQueryRelationalTestBase<TFixture> : NorthwindWhereQueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected NorthwindWhereQueryRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        public override Task Where_bool_client_side_negated(bool async)
        {
            return AssertTranslationFailed(() => base.Where_bool_client_side_negated(async));
        }

        public override Task Where_equals_method_string_with_ignore_case(bool async)
        {
            return AssertTranslationFailed(() => base.Where_equals_method_string_with_ignore_case(async));
        }

        protected virtual bool CanExecuteQueryString
            => false;

        protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
            => new RelationalQueryAsserter(
                fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression, canExecuteQueryString: CanExecuteQueryString);
    }
}
