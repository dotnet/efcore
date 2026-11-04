// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class EnumTranslationsCosmosTest : EnumTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public EnumTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Equality

    public override async Task Equality_to_constant()
    {
        await base.Equality_to_constant();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Enum"] = 0)
""");
    }

    public override async Task Equality_to_parameter()
    {
        await base.Equality_to_parameter();

        AssertSql(
            """
@basicEnum=?

SELECT VALUE c
FROM root c
WHERE (c["Enum"] = @basicEnum)
""");
    }

    public override async Task Equality_nullable_enum_to_constant()
    {
        await base.Equality_nullable_enum_to_constant();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Enum"] = 0)
""");
    }

    public override async Task Equality_nullable_enum_to_parameter()
    {
        await base.Equality_nullable_enum_to_parameter();

        AssertSql(
            """
@basicEnum=?

SELECT VALUE c
FROM root c
WHERE (c["Enum"] = @basicEnum)
""");
    }

    public override async Task Equality_nullable_enum_to_null_constant()
    {
        await base.Equality_nullable_enum_to_null_constant();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Enum"] = null)
""");
    }

    public override async Task Equality_nullable_enum_to_null_parameter()
    {
        await base.Equality_nullable_enum_to_null_parameter();

        AssertSql(
            """
@basicEnum=?

SELECT VALUE c
FROM root c
WHERE (c["Enum"] = @basicEnum)
""");
    }

    public override async Task Equality_nullable_enum_to_nullable_parameter()
    {
        await base.Equality_nullable_enum_to_nullable_parameter();

        AssertSql(
            """
@basicEnum=?

SELECT VALUE c
FROM root c
WHERE (c["Enum"] = @basicEnum)
""");
    }

    #endregion Equality

    public override async Task Bitwise_and_enum_constant()
    {
        await base.Bitwise_and_enum_constant();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["FlagsEnum"] & 1) > 0)
""",
            //
            """
SELECT VALUE c
FROM root c
WHERE ((c["FlagsEnum"] & 1) = 1)
""");
    }

    public override async Task Bitwise_and_integral_constant()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Bitwise_and_integral_constant());

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["FlagsEnum"] & 8) = 8)
""");
    }

    public override async Task Bitwise_and_nullable_enum_with_constant()
    {
        await base.Bitwise_and_nullable_enum_with_constant();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["FlagsEnum"] & 8) > 0)
""");
    }

    public override async Task Where_bitwise_and_nullable_enum_with_null_constant()
    {
        await base.Where_bitwise_and_nullable_enum_with_null_constant();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["FlagsEnum"] & null) > 0)
""");
    }

    public override async Task Where_bitwise_and_nullable_enum_with_non_nullable_parameter()
    {
        await base.Where_bitwise_and_nullable_enum_with_non_nullable_parameter();

        AssertSql(
            """
@flagsEnum=?

SELECT VALUE c
FROM root c
WHERE ((c["FlagsEnum"] & @flagsEnum) > 0)
""");
    }

    public override async Task Where_bitwise_and_nullable_enum_with_nullable_parameter()
    {
        await base.Where_bitwise_and_nullable_enum_with_nullable_parameter();

        AssertSql(
            """
@flagsEnum=?

SELECT VALUE c
FROM root c
WHERE ((c["FlagsEnum"] & @flagsEnum) > 0)
""",
            //
            """
@flagsEnum=?

SELECT VALUE c
FROM root c
WHERE ((c["FlagsEnum"] & @flagsEnum) > 0)
""");
    }

    public override async Task Bitwise_or()
    {
        await base.Bitwise_or();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["FlagsEnum"] | 8) > 0)
""");
    }

    public override async Task Bitwise_projects_values_in_select()
    {
        await base.Bitwise_projects_values_in_select();

        AssertSql(
            """
SELECT VALUE
{
    "BitwiseTrue" : ((c["FlagsEnum"] & 8) = 8),
    "BitwiseFalse" : ((c["FlagsEnum"] & 8) = 4),
    "BitwiseValue" : (c["FlagsEnum"] & 8)
}
FROM root c
WHERE ((c["FlagsEnum"] & 8) = 8)
OFFSET 0 LIMIT 1
""");
    }

    // #35317
    public override Task HasFlag()
        => AssertTranslationFailed(() => base.HasFlag());

    // #35317
    public override Task HasFlag_with_non_nullable_parameter()
        => AssertTranslationFailed(() => base.HasFlag());

    // #35317
    public override Task HasFlag_with_nullable_parameter()
        => AssertTranslationFailed(() => base.HasFlag());

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
