// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class ByteArrayTranslationsSqliteTest : ByteArrayTranslationsTestBase<BasicTypesQuerySqliteFixture>
{
    public ByteArrayTranslationsSqliteTest(BasicTypesQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Length(bool async)
    {
        await base.Length(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE length("b"."ByteArray") = 4
""");
    }

    // Array access. Issue #16428.
    public override Task Index(bool async)
        => AssertTranslationFailed(() => base.Index(async));

    // Array access. Issue #16428.
    public override Task First(bool async)
        => AssertTranslationFailed(() => base.First(async));

    public override async Task Contains_with_constant(bool async)
    {
        await base.Contains_with_constant(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE instr("b"."ByteArray", X'01') > 0
""");
    }

    public override async Task Contains_with_parameter(bool async)
    {
        await base.Contains_with_parameter(async);

        AssertSql(
            """
@someByte='1'

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE instr("b"."ByteArray", char(@someByte)) > 0
""");
    }

    public override async Task Contains_with_column(bool async)
    {
        await base.Contains_with_column(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE instr("b"."ByteArray", char("b"."Byte")) > 0
""");
    }

    public override async Task SequenceEqual(bool async)
    {
        await base.SequenceEqual(async);

        AssertSql(
            """
@byteArrayParam='0xDEADBEEF' (Size = 4)

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."ByteArray" = @byteArrayParam
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
