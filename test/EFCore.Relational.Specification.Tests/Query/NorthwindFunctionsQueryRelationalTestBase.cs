// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NorthwindFunctionsQueryRelationalTestBase<TFixture>(TFixture fixture)
    : NorthwindFunctionsQueryTestBase<TFixture>(fixture)
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    // StartsWith with StringComparison not supported in relational databases, where the column collation is used to control comparison
    // semantics.
    public override Task String_StartsWith_with_StringComparison_Ordinal(bool async)
        => AssertTranslationFailed(() => base.String_StartsWith_with_StringComparison_Ordinal(async));

    // StartsWith with StringComparison not supported in relational databases, where the column collation is used to control comparison
    // semantics.
    public override Task String_StartsWith_with_StringComparison_OrdinalIgnoreCase(bool async)
        => AssertTranslationFailed(() => base.String_StartsWith_with_StringComparison_OrdinalIgnoreCase(async));

    // EndsWith with StringComparison not supported in relational databases, where the column collation is used to control comparison
    // semantics.
    public override Task String_EndsWith_with_StringComparison_Ordinal(bool async)
        => AssertTranslationFailed(() => base.String_EndsWith_with_StringComparison_Ordinal(async));

    // EndsWith with StringComparison not supported in relational databases, where the column collation is used to control comparison
    // semantics.
    public override Task String_EndsWith_with_StringComparison_OrdinalIgnoreCase(bool async)
        => AssertTranslationFailed(() => base.String_EndsWith_with_StringComparison_OrdinalIgnoreCase(async));

    // Contains with StringComparison not supported in relational databases, where the column collation is used to control comparison
    // semantics.
    public override Task String_Contains_with_StringComparison_Ordinal(bool async)
        => AssertTranslationFailed(() => base.String_Contains_with_StringComparison_Ordinal(async));

    // Contains with StringComparison not supported in relational databases, where the column collation is used to control comparison
    // semantics.
    public override Task String_Contains_with_StringComparison_OrdinalIgnoreCase(bool async)
        => AssertTranslationFailed(() => base.String_Contains_with_StringComparison_OrdinalIgnoreCase(async));

    protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
        => new RelationalQueryAsserter(
            fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);
}
