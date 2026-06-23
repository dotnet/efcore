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
@basicEnum='0'

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
@basicEnum='0'

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
@basicEnum=null

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
@basicEnum='0'

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
        await base.Bitwise_and_integral_constant();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["FlagsEnum"] & 8) = 8)
""",
            //
            """
SELECT VALUE c
FROM root c
WHERE ((c["FlagsEnum"] & 8) = 8)
""",
            //
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
@flagsEnum='8'

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
@flagsEnum='8'

SELECT VALUE c
FROM root c
WHERE ((c["FlagsEnum"] & @flagsEnum) > 0)
""",
            //
            """
@flagsEnum=null

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
    public override Task HasFlag(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.HasFlag(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["FlagsEnum"] & 8) = 8)
""",
                    //
                    """
SELECT VALUE c
FROM root c
WHERE ((c["FlagsEnum"] & 12) = 12)
""",
                    //
                    """
SELECT VALUE c
FROM root c
WHERE ((c["FlagsEnum"] & 8) = 8)
""",
                    //
                    """
SELECT VALUE c
FROM root c
WHERE ((c["FlagsEnum"] & 8) = 8)
""",
                    //
                    """
SELECT VALUE c
FROM root c
WHERE ((8 & c["FlagsEnum"]) = c["FlagsEnum"])
""",
                    //
                    """
SELECT VALUE
{
    "hasFlagTrue" : ((c["FlagsEnum"] & 8) = 8),
    "hasFlagFalse" : ((c["FlagsEnum"] & 4) = 4)
}
FROM root c
WHERE ((c["FlagsEnum"] & 8) = 8)
OFFSET 0 LIMIT 1
""");
            });

    // #35317
    public override Task HasFlag_with_non_nullable_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.HasFlag_with_non_nullable_parameter(a);

                AssertSql(
                    """
@flagsEnum=?

SELECT VALUE c
FROM root c
WHERE ((c["FlagsEnum"] & @flagsEnum) = @flagsEnum)
""");
            });

    // #35317
    public override Task HasFlag_with_nullable_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.HasFlag_with_nullable_parameter(a);

                AssertSql(
                    """
@flagsEnum=?

SELECT VALUE c
FROM root c
WHERE ((c["FlagsEnum"] & @flagsEnum) = @flagsEnum)
""");
            });


    public override Task ToString_enum_contains(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.ToString_enum_contains(a);

                AssertSql(
                    """

""");
            });

    public override Task ToString_nullable_enum_contains(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.ToString_nullable_enum_contains(a);

                AssertSql(
                    """

""");
            });

    public override Task ToString_enum_property_projection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.ToString_enum_property_projection(a);

                AssertSql(
                    """

""");
            });

    public override Task ToString_nullable_enum_property_projection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.ToString_nullable_enum_property_projection(a);

                AssertSql(
                    """
                    
                    """);
            });

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
