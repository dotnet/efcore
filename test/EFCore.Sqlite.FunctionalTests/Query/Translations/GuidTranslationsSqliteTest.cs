// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class GuidTranslationsSqliteTest : GuidTranslationsTestBase<BasicTypesQuerySqliteFixture>
{
    public GuidTranslationsSqliteTest(BasicTypesQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task New_with_constant(bool async)
    {
        await base.New_with_constant(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."Guid" = 'DF36F493-463F-4123-83F9-6B135DEEB7BA'
""");
    }

    public override async Task New_with_parameter(bool async)
    {
        await base.New_with_parameter(async);

        AssertSql(
            """
@p='df36f493-463f-4123-83f9-6b135deeb7ba'

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."Guid" = @p
""");
    }

    public override async Task ToString_projection(bool async)
    {
        await base.ToString_projection(async);

        AssertSql(
            """
SELECT CAST("b"."Guid" AS TEXT)
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override Task NewGuid(bool async)
        => AssertTranslationFailed(() => base.NewGuid(async));

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
