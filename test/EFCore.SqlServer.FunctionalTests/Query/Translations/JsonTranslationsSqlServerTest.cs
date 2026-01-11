// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class JsonTranslationsSqlServerTest : JsonTranslationsRelationalTestBase<JsonTranslationsSqlServerTest.JsonTranslationsQuerySqlServerFixture>
{
    public JsonTranslationsSqlServerTest(JsonTranslationsQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact, SqlServerCondition(SqlServerCondition.SupportsFunctions2022)]
    public override async Task JsonExists_on_scalar_string_column()
    {
        await base.JsonExists_on_scalar_string_column();

        AssertSql(
            """
SELECT [j].[Id], [j].[JsonString], [j].[JsonComplexType]
FROM [JsonEntities] AS [j]
WHERE JSON_PATH_EXISTS([j].[JsonString], N'$.OptionalInt') = 1
""");
    }

    [ConditionalFact, SqlServerCondition(SqlServerCondition.SupportsFunctions2022)]
    public override async Task JsonExists_on_complex_property()
    {
        await base.JsonExists_on_complex_property();

        AssertSql(
            """
SELECT [j].[Id], [j].[JsonString], [j].[JsonComplexType]
FROM [JsonEntities] AS [j]
WHERE JSON_PATH_EXISTS([j].[JsonComplexType], N'$.OptionalInt') = 1
""");
    }

    public class JsonTranslationsQuerySqlServerFixture : JsonTranslationsQueryFixtureBase, ITestSqlLoggerFactory
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        // When testing against SQL Server 2025 or later, set the compatibility level to 170 to use the json type instead of nvarchar(max).
        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            var options = base.AddOptions(builder);

            return TestEnvironment.SqlServerMajorVersion < 17
                ? options
                : options.UseSqlServerCompatibilityLevel(170);
        }

        protected override string RemoveJsonProperty(string column, string jsonPath)
            => $"JSON_MODIFY({column}, '{jsonPath}', NULL)";
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
