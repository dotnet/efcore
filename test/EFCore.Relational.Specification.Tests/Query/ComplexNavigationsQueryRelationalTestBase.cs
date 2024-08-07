﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class ComplexNavigationsQueryRelationalTestBase<TFixture>(TFixture fixture)
    : ComplexNavigationsQueryTestBase<TFixture>(fixture)
    where TFixture : ComplexNavigationsQueryFixtureBase, new()
{
    public override Task Complex_query_with_optional_navigations_and_client_side_evaluation(bool async)
        => AssertTranslationFailed(() => base.Complex_query_with_optional_navigations_and_client_side_evaluation(async));

    protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
        => new RelationalQueryAsserter(
            fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);
}
