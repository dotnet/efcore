// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations.Operators;

public class BitwiseOperatorTranslationsSqliteTest : BitwiseOperatorTranslationsTestBase<BasicTypesQuerySqliteFixture>
{
    public BitwiseOperatorTranslationsSqliteTest(BasicTypesQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Or()
    {
        await base.Or();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST("b"."Int" AS INTEGER) | "b"."Long" = 7
""",
            //
            """
SELECT CAST("b"."Int" AS INTEGER) | "b"."Long"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task Or_over_boolean()
    {
        await base.Or_over_boolean();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."Int" = 12 OR "b"."String" = 'Seattle'
""",
            //
            """
SELECT "b"."Int" = 12 OR "b"."String" = 'Seattle'
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task Or_multiple()
    {
        await base.Or_multiple();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST("b"."Int" | "b"."Short" AS INTEGER) | "b"."Long" = 7
""");
    }

    public override async Task And()
    {
        await base.And();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."Int" & "b"."Short" = 2
""",
            //
            """
SELECT "b"."Int" & "b"."Short"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task And_over_boolean()
    {
        await base.And_over_boolean();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."Int" = 8 AND "b"."String" = 'Seattle'
""",
            //
            """
SELECT "b"."Int" = 8 AND "b"."String" = 'Seattle'
FROM "BasicTypesEntities" AS "b"
""");
    }

    [ConditionalFact(Skip = "Issue #16645 bitwise xor support")]
    public override Task Xor()
        => AssertTranslationFailed(() => base.Xor());

    [ConditionalFact(Skip = "Issue #16645 bitwise xor support")]
    public override Task Xor_over_boolean()
        => AssertTranslationFailed(() => base.Xor_over_boolean());

    public override async Task Complement()
    {
        await base.Complement();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE ~"b"."Int" = -9
""");
    }

    public override async Task And_or_over_boolean()
    {
        await base.And_or_over_boolean();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE ("b"."Int" = 12 AND "b"."Short" = 12) OR "b"."String" = 'Seattle'
""");
    }

    public override async Task Or_with_logical_or()
    {
        await base.Or_with_logical_or();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."Int" = 12 OR "b"."Short" = 12 OR "b"."String" = 'Seattle'
""");
    }

    public override async Task And_with_logical_and()
    {
        await base.And_with_logical_and();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."Int" = 8 AND "b"."Short" = 8 AND "b"."String" = 'Seattle'
""");
    }

    public override async Task Or_with_logical_and()
    {
        await base.Or_with_logical_and();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE ("b"."Int" = 8 OR "b"."Short" = 9) AND "b"."String" = 'Seattle'
""");
    }

    public override async Task And_with_logical_or()
    {
        await base.And_with_logical_or();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE ("b"."Int" = 12 AND "b"."Short" = 12) OR "b"."String" = 'Seattle'
""");
    }

    public override Task Left_shift()
        => AssertTranslationFailed(() => base.Left_shift());

    public override Task Right_shift()
        => AssertTranslationFailed(() => base.Right_shift());

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
