// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public abstract class StringTranslationsRelationalTestBase<TFixture>(TFixture fixture) : StringTranslationsTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    #region Case sensitivity

    // All the following tests specify case sensitivity (via StringComparison), which isn't supported in relational databases, where the
    // column collation is used to control comparison semantics.

    public override Task Equals_with_OrdinalIgnoreCase(bool async)
        => AssertTranslationFailed(() => base.Equals_with_OrdinalIgnoreCase(async));

    public override Task Equals_with_Ordinal(bool async)
        => AssertTranslationFailed(() => base.Equals_with_OrdinalIgnoreCase(async));

    public override Task Static_Equals_with_OrdinalIgnoreCase(bool async)
        => AssertTranslationFailed(() => base.Static_Equals_with_OrdinalIgnoreCase(async));

    public override Task Static_Equals_with_Ordinal(bool async)
        => AssertTranslationFailed(() => base.Static_Equals_with_Ordinal(async));

    public override Task StartsWith_with_StringComparison_Ordinal(bool async)
        => AssertTranslationFailed(() => base.StartsWith_with_StringComparison_Ordinal(async));

    public override Task StartsWith_with_StringComparison_OrdinalIgnoreCase(bool async)
        => AssertTranslationFailed(() => base.StartsWith_with_StringComparison_OrdinalIgnoreCase(async));

    public override Task EndsWith_with_StringComparison_Ordinal(bool async)
        => AssertTranslationFailed(() => base.EndsWith_with_StringComparison_Ordinal(async));

    public override Task EndsWith_with_StringComparison_OrdinalIgnoreCase(bool async)
        => AssertTranslationFailed(() => base.EndsWith_with_StringComparison_OrdinalIgnoreCase(async));

    public override Task Contains_with_StringComparison_Ordinal(bool async)
        => AssertTranslationFailed(() => base.Contains_with_StringComparison_Ordinal(async));

    public override Task Contains_with_StringComparison_OrdinalIgnoreCase(bool async)
        => AssertTranslationFailed(() => base.Contains_with_StringComparison_OrdinalIgnoreCase(async));

    #endregion Case sensitivity

    #region Like

    [ConditionalTheory] // #26661, precedence/parentheses - belongs in OperatorsQueryTestBase
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Like_and_comparison(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => EF.Functions.Like(c.String, "S%") && c.Int == 8),
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.StartsWith("S") && c.Int == 8));

    [ConditionalTheory] // #26661, precedence/parentheses - belongs in OperatorsQueryTestBase
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Like_or_comparison(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => EF.Functions.Like(c.String, "S%") || c.Int == int.MaxValue),
            ss => ss.Set<BasicTypesEntity>().Where(c => c.String.StartsWith("S") || c.Id == int.MaxValue));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Like_with_non_string_column_using_ToString(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => EF.Functions.Like(o.Int.ToString(), "%5%")),
            ss => ss.Set<BasicTypesEntity>().Where(o => o.Int.ToString().Contains("5")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Like_with_non_string_column_using_double_cast(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => EF.Functions.Like((string)(object)o.Int, "%5%")),
            ss => ss.Set<BasicTypesEntity>().Where(o => o.Int.ToString().Contains("5")));

    #endregion Like
}
