// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class NorthwindWhereQueryRelationalTestBase<TFixture> : NorthwindWhereQueryTestBase<TFixture>
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    protected NorthwindWhereQueryRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    public override Task Where_bool_client_side_negated(bool async)
        => AssertTranslationFailed(() => base.Where_bool_client_side_negated(async));

    public override Task Where_equals_method_string_with_ignore_case(bool async)
        => AssertTranslationFailed(() => base.Where_equals_method_string_with_ignore_case(async));

    protected virtual bool CanExecuteQueryString
        => false;

    protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
        => new RelationalQueryAsserter(
            fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression, canExecuteQueryString: CanExecuteQueryString);
}
