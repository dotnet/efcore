// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class EnumTranslationsSqliteTest : EnumTranslationsTestBase<BasicTypesQuerySqliteFixture>
{
    public EnumTranslationsSqliteTest(BasicTypesQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
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
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."Enum" = 0
""");
    }

    public override async Task Equality_to_parameter()
    {
        await base.Equality_to_parameter();

        AssertSql(
            """
@basicEnum='0'

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."Enum" = @basicEnum
""");
    }

    public override async Task Equality_nullable_enum_to_constant()
    {
        await base.Equality_nullable_enum_to_constant();

        AssertSql(
            """
SELECT "n"."Id", "n"."Bool", "n"."Byte", "n"."ByteArray", "n"."DateOnly", "n"."DateTime", "n"."DateTimeOffset", "n"."Decimal", "n"."Double", "n"."Enum", "n"."FlagsEnum", "n"."Float", "n"."Guid", "n"."Int", "n"."Long", "n"."Short", "n"."String", "n"."TimeOnly", "n"."TimeSpan"
FROM "NullableBasicTypesEntities" AS "n"
WHERE "n"."Enum" = 0
""");
    }

    public override async Task Equality_nullable_enum_to_parameter()
    {
        await base.Equality_nullable_enum_to_parameter();

        AssertSql(
            """
@basicEnum='0'

SELECT "n"."Id", "n"."Bool", "n"."Byte", "n"."ByteArray", "n"."DateOnly", "n"."DateTime", "n"."DateTimeOffset", "n"."Decimal", "n"."Double", "n"."Enum", "n"."FlagsEnum", "n"."Float", "n"."Guid", "n"."Int", "n"."Long", "n"."Short", "n"."String", "n"."TimeOnly", "n"."TimeSpan"
FROM "NullableBasicTypesEntities" AS "n"
WHERE "n"."Enum" = @basicEnum
""");
    }

    public override async Task Equality_nullable_enum_to_null_constant()
    {
        await base.Equality_nullable_enum_to_null_constant();

        AssertSql(
            """
SELECT "n"."Id", "n"."Bool", "n"."Byte", "n"."ByteArray", "n"."DateOnly", "n"."DateTime", "n"."DateTimeOffset", "n"."Decimal", "n"."Double", "n"."Enum", "n"."FlagsEnum", "n"."Float", "n"."Guid", "n"."Int", "n"."Long", "n"."Short", "n"."String", "n"."TimeOnly", "n"."TimeSpan"
FROM "NullableBasicTypesEntities" AS "n"
WHERE "n"."Enum" IS NULL
""");
    }

    public override async Task Equality_nullable_enum_to_null_parameter()
    {
        await base.Equality_nullable_enum_to_null_parameter();

        AssertSql(
            """
SELECT "n"."Id", "n"."Bool", "n"."Byte", "n"."ByteArray", "n"."DateOnly", "n"."DateTime", "n"."DateTimeOffset", "n"."Decimal", "n"."Double", "n"."Enum", "n"."FlagsEnum", "n"."Float", "n"."Guid", "n"."Int", "n"."Long", "n"."Short", "n"."String", "n"."TimeOnly", "n"."TimeSpan"
FROM "NullableBasicTypesEntities" AS "n"
WHERE "n"."Enum" IS NULL
""");
    }

    public override async Task Equality_nullable_enum_to_nullable_parameter()
    {
        await base.Equality_nullable_enum_to_nullable_parameter();

        AssertSql(
            """
@basicEnum='0' (Nullable = true)

SELECT "n"."Id", "n"."Bool", "n"."Byte", "n"."ByteArray", "n"."DateOnly", "n"."DateTime", "n"."DateTimeOffset", "n"."Decimal", "n"."Double", "n"."Enum", "n"."FlagsEnum", "n"."Float", "n"."Guid", "n"."Int", "n"."Long", "n"."Short", "n"."String", "n"."TimeOnly", "n"."TimeSpan"
FROM "NullableBasicTypesEntities" AS "n"
WHERE "n"."Enum" = @basicEnum
""");
    }

    #endregion Equality

    public override async Task Bitwise_and_enum_constant()
    {
        await base.Bitwise_and_enum_constant();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."FlagsEnum" & 1 > 0
""",
            //
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."FlagsEnum" & 1 = 1
""");
    }

    public override async Task Bitwise_and_integral_constant()
    {
        await base.Bitwise_and_integral_constant();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."FlagsEnum" & 8 = 8
""",
            //
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST("b"."FlagsEnum" AS INTEGER) & 8 = 8
""",
            //
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST("b"."FlagsEnum" AS INTEGER) & 8 = 8
""");
    }

    public override async Task Bitwise_and_nullable_enum_with_constant()
    {
        await base.Bitwise_and_nullable_enum_with_constant();

        AssertSql(
            """
SELECT "n"."Id", "n"."Bool", "n"."Byte", "n"."ByteArray", "n"."DateOnly", "n"."DateTime", "n"."DateTimeOffset", "n"."Decimal", "n"."Double", "n"."Enum", "n"."FlagsEnum", "n"."Float", "n"."Guid", "n"."Int", "n"."Long", "n"."Short", "n"."String", "n"."TimeOnly", "n"."TimeSpan"
FROM "NullableBasicTypesEntities" AS "n"
WHERE "n"."FlagsEnum" & 8 > 0
""");
    }

    public override async Task Where_bitwise_and_nullable_enum_with_null_constant()
    {
        await base.Where_bitwise_and_nullable_enum_with_null_constant();

        AssertSql(
            """
SELECT "n"."Id", "n"."Bool", "n"."Byte", "n"."ByteArray", "n"."DateOnly", "n"."DateTime", "n"."DateTimeOffset", "n"."Decimal", "n"."Double", "n"."Enum", "n"."FlagsEnum", "n"."Float", "n"."Guid", "n"."Int", "n"."Long", "n"."Short", "n"."String", "n"."TimeOnly", "n"."TimeSpan"
FROM "NullableBasicTypesEntities" AS "n"
WHERE "n"."FlagsEnum" & NULL > 0
""");
    }

    public override async Task Where_bitwise_and_nullable_enum_with_non_nullable_parameter()
    {
        await base.Where_bitwise_and_nullable_enum_with_non_nullable_parameter();

        AssertSql(
            """
@flagsEnum='8'

SELECT "n"."Id", "n"."Bool", "n"."Byte", "n"."ByteArray", "n"."DateOnly", "n"."DateTime", "n"."DateTimeOffset", "n"."Decimal", "n"."Double", "n"."Enum", "n"."FlagsEnum", "n"."Float", "n"."Guid", "n"."Int", "n"."Long", "n"."Short", "n"."String", "n"."TimeOnly", "n"."TimeSpan"
FROM "NullableBasicTypesEntities" AS "n"
WHERE "n"."FlagsEnum" & @flagsEnum > 0
""");
    }

    public override async Task Where_bitwise_and_nullable_enum_with_nullable_parameter()
    {
        await base.Where_bitwise_and_nullable_enum_with_nullable_parameter();

        AssertSql(
            """
@flagsEnum='8' (Nullable = true)

SELECT "n"."Id", "n"."Bool", "n"."Byte", "n"."ByteArray", "n"."DateOnly", "n"."DateTime", "n"."DateTimeOffset", "n"."Decimal", "n"."Double", "n"."Enum", "n"."FlagsEnum", "n"."Float", "n"."Guid", "n"."Int", "n"."Long", "n"."Short", "n"."String", "n"."TimeOnly", "n"."TimeSpan"
FROM "NullableBasicTypesEntities" AS "n"
WHERE "n"."FlagsEnum" & @flagsEnum > 0
""",
            //
            """
SELECT "n"."Id", "n"."Bool", "n"."Byte", "n"."ByteArray", "n"."DateOnly", "n"."DateTime", "n"."DateTimeOffset", "n"."Decimal", "n"."Double", "n"."Enum", "n"."FlagsEnum", "n"."Float", "n"."Guid", "n"."Int", "n"."Long", "n"."Short", "n"."String", "n"."TimeOnly", "n"."TimeSpan"
FROM "NullableBasicTypesEntities" AS "n"
WHERE "n"."FlagsEnum" & NULL > 0
""");
    }

    public override async Task Bitwise_or()
    {
        await base.Bitwise_or();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."FlagsEnum" | 8 > 0
""");
    }

    public override async Task Bitwise_projects_values_in_select()
    {
        await base.Bitwise_projects_values_in_select();

        AssertSql(
            """
SELECT "b"."FlagsEnum" & 8 = 8 AS "BitwiseTrue", "b"."FlagsEnum" & 8 = 4 AS "BitwiseFalse", "b"."FlagsEnum" & 8 AS "BitwiseValue"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."FlagsEnum" & 8 = 8
LIMIT 1
""");
    }

    public override async Task HasFlag()
    {
        await base.HasFlag();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."FlagsEnum" & 8 = 8
""",
            //
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."FlagsEnum" & 12 = 12
""",
            //
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."FlagsEnum" & 8 = 8
""",
            //
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."FlagsEnum" & 8 = 8
""",
            //
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE 8 & "b"."FlagsEnum" = "b"."FlagsEnum"
""",
            //
            """
SELECT "b"."FlagsEnum" & 8 = 8 AS "hasFlagTrue", "b"."FlagsEnum" & 4 = 4 AS "hasFlagFalse"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."FlagsEnum" & 8 = 8
LIMIT 1
""");
    }

    public override async Task HasFlag_with_non_nullable_parameter()
    {
        await base.HasFlag_with_non_nullable_parameter();

        AssertSql(
            """
@flagsEnum='8'

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."FlagsEnum" & @flagsEnum = @flagsEnum
""");
    }

    public override async Task HasFlag_with_nullable_parameter()
    {
        await base.HasFlag_with_nullable_parameter();

        AssertSql(
            """
@flagsEnum='8' (Nullable = true)

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."FlagsEnum" & @flagsEnum = @flagsEnum
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
