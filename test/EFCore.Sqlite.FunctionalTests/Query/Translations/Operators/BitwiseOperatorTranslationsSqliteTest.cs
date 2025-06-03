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

    public override async Task Or(bool async)
    {
        await base.Or(async);

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

    public override async Task Or_over_boolean(bool async)
    {
        await base.Or_over_boolean(async);

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

    public override async Task Or_multiple(bool async)
    {
        await base.Or_multiple(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST("b"."Int" | "b"."Short" AS INTEGER) | "b"."Long" = 7
""");
    }

    public override async Task And(bool async)
    {
        await base.And(async);

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

    public override async Task And_over_boolean(bool async)
    {
        await base.And_over_boolean(async);

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

    [ConditionalTheory(Skip = "Issue #16645 bitwise xor support")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Xor(bool async)
        => AssertTranslationFailed(() => base.Xor(async));

    [ConditionalTheory(Skip = "Issue #16645 bitwise xor support")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Xor_over_boolean(bool async)
        => AssertTranslationFailed(() => base.Xor_over_boolean(async));

    public override async Task Complement(bool async)
    {
        await base.Complement(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE ~"b"."Int" = -9
""");
    }

    public override async Task And_or_over_boolean(bool async)
    {
        await base.And_or_over_boolean(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE ("b"."Int" = 12 AND "b"."Short" = 12) OR "b"."String" = 'Seattle'
""");
    }

    public override async Task Or_with_logical_or(bool async)
    {
        await base.Or_with_logical_or(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."Int" = 12 OR "b"."Short" = 12 OR "b"."String" = 'Seattle'
""");
    }

    public override async Task And_with_logical_and(bool async)
    {
        await base.And_with_logical_and(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."Int" = 8 AND "b"."Short" = 8 AND "b"."String" = 'Seattle'
""");
    }

    public override async Task Or_with_logical_and(bool async)
    {
        await base.Or_with_logical_and(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE ("b"."Int" = 8 OR "b"."Short" = 9) AND "b"."String" = 'Seattle'
""");
    }

    public override async Task And_with_logical_or(bool async)
    {
        await base.And_with_logical_or(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE ("b"."Int" = 12 AND "b"."Short" = 12) OR "b"."String" = 'Seattle'
""");
    }

    public override Task Left_shift(bool async)
        => AssertTranslationFailed(() => base.Left_shift(async));

    public override Task Right_shift(bool async)
        => AssertTranslationFailed(() => base.Right_shift(async));

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
