// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Nodes;

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
    public override async Task JsonPathExists_on_scalar_string_column()
    {
        await base.JsonPathExists_on_scalar_string_column();

        AssertSql(
            """
SELECT [j].[Id], [j].[JsonString], [j].[JsonComplexType], [j].[JsonOwnedType]
FROM [JsonEntities] AS [j]
WHERE JSON_PATH_EXISTS([j].[JsonString], N'$.OptionalInt') = 1
""");
    }

    [ConditionalFact, SqlServerCondition(SqlServerCondition.SupportsFunctions2022)]
    public override async Task JsonPathExists_on_complex_property()
    {
        await base.JsonPathExists_on_complex_property();

        AssertSql(
            """
SELECT [j].[Id], [j].[JsonString], [j].[JsonComplexType], [j].[JsonOwnedType]
FROM [JsonEntities] AS [j]
WHERE JSON_PATH_EXISTS([j].[JsonComplexType], N'$.OptionalInt') = 1
""");
    }

    [ConditionalFact, SqlServerCondition(SqlServerCondition.SupportsFunctions2022)]
    public override async Task JsonPathExists_on_owned_entity()
    {
        await base.JsonPathExists_on_owned_entity();

        AssertSql(
            """
SELECT [j].[Id], [j].[JsonString], [j].[JsonComplexType], [j].[JsonOwnedType]
FROM [JsonEntities] AS [j]
WHERE JSON_PATH_EXISTS([j].[JsonOwnedType], N'$.OptionalInt') = 1
""");
    }

#pragma warning disable EF9106 // JsonContains is experimental
    [ConditionalFact, SqlServerCondition(SqlServerCondition.SupportsJsonType)]
    public async Task JsonContains_on_scalar_string_column()
    {
        await AssertQuery(
            ss => ss.Set<JsonTranslationsEntity>()
                .Where(b => EF.Functions.JsonContains(b.JsonString, 8, "$.OptionalInt") == 1),
            ss => ss.Set<JsonTranslationsEntity>()
                .Where(b => JsonNode.Parse(b.JsonString ?? "{}")!.AsObject().ContainsKey("OptionalInt")
                    && JsonNode.Parse(b.JsonString ?? "{}")!.AsObject()["OptionalInt"] != null
                    && ((JsonValue)JsonNode.Parse(b.JsonString ?? "{}")!.AsObject()["OptionalInt"]!).GetValue<int?>() == 8));

        AssertSql(
            """
SELECT [j].[Id], [j].[JsonString], [j].[JsonComplexType], [j].[JsonOwnedType]
FROM [JsonEntities] AS [j]
WHERE JSON_CONTAINS([j].[JsonString], 8, N'$.OptionalInt') = 1
""");
    }

    [ConditionalFact, SqlServerCondition(SqlServerCondition.SupportsJsonType)]
    public async Task JsonContains_on_complex_property()
    {
        await AssertQuery(
            ss => ss.Set<JsonTranslationsEntity>()
                .Where(b => EF.Functions.JsonContains(b.JsonComplexType, 8, "$.OptionalInt") == 1),
            ss => ss.Set<JsonTranslationsEntity>()
                .Where(b => b.JsonComplexType.OptionalInt == 8));

        AssertSql(
            """
SELECT [j].[Id], [j].[JsonString], [j].[JsonComplexType], [j].[JsonOwnedType]
FROM [JsonEntities] AS [j]
WHERE JSON_CONTAINS([j].[JsonComplexType], 8, N'$.OptionalInt') = 1
""");
    }

    [ConditionalFact, SqlServerCondition(SqlServerCondition.SupportsJsonType)]
    public async Task JsonContains_on_owned_entity()
    {
        await AssertQuery(
            ss => ss.Set<JsonTranslationsEntity>()
                .Where(b => EF.Functions.JsonContains(b.JsonOwnedType, 8, "$.OptionalInt") == 1),
            ss => ss.Set<JsonTranslationsEntity>()
                .Where(b => b.JsonOwnedType.OptionalInt == 8));

        AssertSql(
            """
SELECT [j].[Id], [j].[JsonString], [j].[JsonComplexType], [j].[JsonOwnedType]
FROM [JsonEntities] AS [j]
WHERE JSON_CONTAINS([j].[JsonOwnedType], 8, N'$.OptionalInt') = 1
""");
    }
#pragma warning restore EF9106 // JsonContains is experimental

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

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            if (TestEnvironment.IsJsonTypeSupported)
            {
                modelBuilder.Entity<JsonTranslationsEntity>().Property(e => e.JsonString).HasColumnType("json");
            }
        }

        protected override string RemoveJsonProperty(string column, string property)
            => $"JSON_MODIFY({column}, '$.{property}', NULL)";
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
