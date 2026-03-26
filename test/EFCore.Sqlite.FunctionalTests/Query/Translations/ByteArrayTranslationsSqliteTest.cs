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

    public override async Task Length()
    {
        await base.Length();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE length("b"."ByteArray") = 4
""");
    }

    // Array access. Issue #16428.
    public override Task Index()
        => AssertTranslationFailed(() => base.Index());

    // Array access. Issue #16428.
    public override Task First()
        => AssertTranslationFailed(() => base.First());

    public override async Task Contains_with_constant()
    {
        await base.Contains_with_constant();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE instr("b"."ByteArray", X'01') > 0
""");
    }

    public override async Task Contains_with_parameter()
    {
        await base.Contains_with_parameter();

        AssertSql(
            """
@someByte='1'

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE instr("b"."ByteArray", char(@someByte)) > 0
""");
    }

    public override async Task Contains_with_column()
    {
        await base.Contains_with_column();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE instr("b"."ByteArray", char("b"."Byte")) > 0
""");
    }

    public override async Task SequenceEqual()
    {
        await base.SequenceEqual();

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
