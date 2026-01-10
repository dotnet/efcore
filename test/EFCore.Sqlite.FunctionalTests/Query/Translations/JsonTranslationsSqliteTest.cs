// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class JsonTranslationsSqliteTest : JsonTranslationsRelationalTestBase<JsonTranslationsSqliteTest.JsonTranslationsQuerySqliteFixture>
{
    public JsonTranslationsSqliteTest(JsonTranslationsQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public override async Task JsonExists_on_scalar_string_column()
    {
        await base.JsonExists_on_scalar_string_column();

        AssertSql(
            """
SELECT "j"."Id", "j"."JsonString", "j"."JsonComplexType"
FROM "JsonEntities" AS "j"
WHERE json_type("j"."JsonString", '$.OptionalInt') IS NOT NULL
""");
    }

    [ConditionalFact]
    public override async Task JsonExists_on_complex_property()
    {
        await base.JsonExists_on_complex_property();

        AssertSql(
            """
SELECT "j"."Id", "j"."JsonString", "j"."JsonComplexType"
FROM "JsonEntities" AS "j"
WHERE json_type("j"."JsonComplexType", '$.OptionalInt') IS NOT NULL
""");
    }

    public class JsonTranslationsQuerySqliteFixture : JsonTranslationsQueryFixtureBase, ITestSqlLoggerFactory
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        protected override string RemoveJsonProperty(string column, string jsonPath)
            => $"json_remove({column}, '{jsonPath}')";
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
