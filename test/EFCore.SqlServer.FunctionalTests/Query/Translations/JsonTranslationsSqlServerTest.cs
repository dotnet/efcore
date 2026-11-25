// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Nodes;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class JsonTranslationsSqlServerTest : JsonTranslationsRelationalTestBase<
    JsonTranslationsSqlServerTest.JsonTranslationsQuerySqlServerFixture>
{
    public JsonTranslationsSqlServerTest(JsonTranslationsQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task JsonPathExists_on_scalar_string_column()
    {
        if (!SqlServerTestEnvironment.IsFunctions2022Supported)

        {
            throw SkipException.ForSkip("Requires IsFunctions2022Supported");
        }

        await base.JsonPathExists_on_scalar_string_column();

        AssertSql(
            """
SELECT [j].[Id], [j].[JsonString], [j].[JsonComplexType], [j].[JsonOwnedType]
FROM [JsonEntities] AS [j]
WHERE JSON_PATH_EXISTS([j].[JsonString], N'$.OptionalInt') = 1
""");
    }

    public override async Task JsonPathExists_on_complex_property()
    {
        if (!SqlServerTestEnvironment.IsFunctions2022Supported)

        {
            throw SkipException.ForSkip("Requires IsFunctions2022Supported");
        }

        await base.JsonPathExists_on_complex_property();

        AssertSql(
            """
SELECT [j].[Id], [j].[JsonString], [j].[JsonComplexType], [j].[JsonOwnedType]
FROM [JsonEntities] AS [j]
WHERE JSON_PATH_EXISTS([j].[JsonComplexType], N'$.OptionalInt') = 1
""");
    }

    public override async Task JsonPathExists_on_owned_entity()
    {
        if (!SqlServerTestEnvironment.IsFunctions2022Supported)

        {
            throw SkipException.ForSkip("Requires IsFunctions2022Supported");
        }

        await base.JsonPathExists_on_owned_entity();

        AssertSql(
            """
SELECT [j].[Id], [j].[JsonString], [j].[JsonComplexType], [j].[JsonOwnedType]
FROM [JsonEntities] AS [j]
WHERE JSON_PATH_EXISTS([j].[JsonOwnedType], N'$.OptionalInt') = 1
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public async Task JsonValue_scalar_and_parameterized_path(bool async)
    {
        await using var context = CreateContext();
        var path = "$.OptionalInt";
        var query = context.JsonEntities.OrderBy(e => e.Id)
            .Select(e => EF.Functions.JsonValue(e.JsonString, path));

        var results = async ? await query.ToListAsync() : query.ToList();

        Assert.Equal(new string?[] { "8", "9", null, null }, results);
        AssertSql(
            """
@path='$.OptionalInt' (Size = 4000)

SELECT JSON_VALUE([j].[JsonString], @path)
FROM [JsonEntities] AS [j]
ORDER BY [j].[Id]
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public async Task JsonValue_literal_document_and_computed_path(bool async)
    {
        await using var context = CreateContext();
        var query = context.JsonEntities.OrderBy(e => e.Id)
            .Select(e => EF.Functions.JsonValue("""{"value":"constant"}""", e.Id == 1 ? "$.value" : "$.missing"));

        var results = async ? await query.ToListAsync() : query.ToList();

        Assert.Equal(new string?[] { "constant", null, null, null }, results);
        AssertSql(
            """
SELECT JSON_VALUE(N'{"value":"constant"}', CASE
    WHEN [j].[Id] = 1 THEN N'$.value'
    ELSE N'$.missing'
END)
FROM [JsonEntities] AS [j]
ORDER BY [j].[Id]
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public async Task JsonValue_complex_and_owned_json(bool async)
    {
        await using var context = CreateContext();
        var query = context.JsonEntities.OrderBy(e => e.Id)
            .Select(e => new
            {
                Complex = EF.Functions.JsonValue(e.JsonComplexType, "$.RequiredInt"),
                Owned = EF.Functions.JsonValue(e.JsonOwnedType, "$.RequiredInt")
            });

        var results = async ? await query.ToListAsync() : query.ToList();

        Assert.Equal(new[] { "8", "9", "10", "10" }, results.Select(e => e.Complex));
        Assert.Equal(new[] { "8", "9", "10", "10" }, results.Select(e => e.Owned));
        AssertSql(
            """
SELECT JSON_VALUE([j].[JsonComplexType], N'$.RequiredInt') AS [Complex], JSON_VALUE([j].[JsonOwnedType], N'$.RequiredInt') AS [Owned]
FROM [JsonEntities] AS [j]
ORDER BY [j].[Id]
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public async Task JsonQuery_and_null_comparison(bool async)
    {
        await using var context = CreateContext();
        var query = context.JsonEntities
            .Where(e => EF.Functions.JsonValue(e.JsonString, "$.OptionalInt") == null)
            .OrderBy(e => e.Id)
            .Select(e => new { e.Id, Json = EF.Functions.JsonQuery(e.JsonString, "$") });

        var results = async ? await query.ToListAsync() : query.ToList();

        Assert.Equal(new[] { 3, 4 }, results.Select(e => e.Id));
        Assert.Equal(10, JsonNode.Parse(results[0].Json!)!["RequiredInt"]!.GetValue<int>());
        Assert.Equal(10, JsonNode.Parse(results[1].Json!)!["RequiredInt"]!.GetValue<int>());
        AssertSql(
            """
SELECT [j].[Id], JSON_QUERY([j].[JsonString], N'$') AS [Json]
FROM [JsonEntities] AS [j]
WHERE JSON_VALUE([j].[JsonString], N'$.OptionalInt') IS NULL
ORDER BY [j].[Id]
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public async Task JsonValue_and_JsonQuery_with_constant_document(bool async)
    {
        await using var context = CreateContext();
        var json = """{"number":42,"text":"hello","array":[1,2],"object":{"x":1},"null":null}""";
        var query = context.JsonEntities.Where(e => e.Id == 1).Select(e => new
        {
            Number = EF.Functions.JsonValue(json, "$.number"),
            Text = EF.Functions.JsonValue(json, "$.text"),
            ArrayAsScalar = EF.Functions.JsonValue(json, "$.array"),
            Missing = EF.Functions.JsonValue(json, "$.missing"),
            Null = EF.Functions.JsonValue(json, "$.null"),
            Array = EF.Functions.JsonQuery(json, "$.array"),
            Object = EF.Functions.JsonQuery(json, "$.object"),
            ScalarAsJson = EF.Functions.JsonQuery(json, "$.number")
        });

        var result = async ? await query.SingleAsync() : query.Single();

        Assert.Equal("42", result.Number);
        Assert.Equal("hello", result.Text);
        Assert.Null(result.ArrayAsScalar);
        Assert.Null(result.Missing);
        Assert.Null(result.Null);
        Assert.Equal("[1,2]", result.Array);
        Assert.Equal("""{"x":1}""", result.Object);
        Assert.Null(result.ScalarAsJson);
        AssertSql(
            """
@json='{"number":42,"text":"hello","array":[1,2],"object":{"x":1},"null":null}' (Size = 4000)

SELECT TOP(2) JSON_VALUE(@json, N'$.number') AS [Number], JSON_VALUE(@json, N'$.text') AS [Text], JSON_VALUE(@json, N'$.array') AS [ArrayAsScalar], JSON_VALUE(@json, N'$.missing') AS [Missing], JSON_VALUE(@json, N'$.null') AS [Null], JSON_QUERY(@json, N'$.array') AS [Array], JSON_QUERY(@json, N'$.object') AS [Object], JSON_QUERY(@json, N'$.number') AS [ScalarAsJson]
FROM [JsonEntities] AS [j]
WHERE [j].[Id] = 1
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public async Task JsonValue_on_value_converted_column(bool async)
    {
        await using var context = CreateContext();
        var query = context.Set<JsonConvertedEntity>().Select(e => EF.Functions.JsonValue(e.Json, "$.value"));

        var result = async ? await query.SingleAsync() : query.Single();

        Assert.Equal("converted", result);
        AssertSql(
            """
SELECT TOP(2) JSON_VALUE([j].[Json], N'$.value')
FROM [JsonConvertedEntity] AS [j]
""");
    }

#pragma warning disable EF9106 // JsonContains is experimental
    [ConditionalFact(typeof(SqlServerTestEnvironment), nameof(SqlServerTestEnvironment.IsJsonTypeSupported))]
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

    [ConditionalFact(typeof(SqlServerTestEnvironment), nameof(SqlServerTestEnvironment.IsJsonTypeSupported))]
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

    [ConditionalFact(typeof(SqlServerTestEnvironment), nameof(SqlServerTestEnvironment.IsJsonTypeSupported))]
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

            return SqlServerTestEnvironment.SqlServerMajorVersion < 17
                ? options
                : options.UseSqlServerCompatibilityLevel(170);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            if (SqlServerTestEnvironment.IsJsonTypeSupported)
            {
                modelBuilder.Entity<JsonTranslationsEntity>().Property(e => e.JsonString).HasColumnType("json");
            }
        }

        protected override string RemoveJsonProperty(string column, string property)
            => $"JSON_MODIFY({column}, '$.{property}', NULL)";
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
