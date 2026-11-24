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

    [Fact]
    public override async Task JsonPathExists_on_scalar_string_column()
    {
        await base.JsonPathExists_on_scalar_string_column();

        AssertSql(
            """
SELECT "j"."Id", "j"."JsonString", "j"."JsonComplexType", "j"."JsonOwnedType"
FROM "JsonEntities" AS "j"
WHERE json_type("j"."JsonString", '$.OptionalInt') IS NOT NULL
""");
    }

    [Fact]
    public override async Task JsonPathExists_on_complex_property()
    {
        await base.JsonPathExists_on_complex_property();

        AssertSql(
            """
SELECT "j"."Id", "j"."JsonString", "j"."JsonComplexType", "j"."JsonOwnedType"
FROM "JsonEntities" AS "j"
WHERE json_type("j"."JsonComplexType", '$.OptionalInt') IS NOT NULL
""");
    }

    [Fact]
    public override async Task JsonPathExists_on_owned_entity()
    {
        await base.JsonPathExists_on_owned_entity();

        AssertSql(
            """
SELECT "j"."Id", "j"."JsonString", "j"."JsonComplexType", "j"."JsonOwnedType"
FROM "JsonEntities" AS "j"
WHERE json_type("j"."JsonOwnedType", '$.OptionalInt') IS NOT NULL
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public async Task JsonExtract_single_path(bool async)
    {
        await using var context = CreateContext();
        var path = "$.OptionalInt";
        var query = context.JsonEntities.OrderBy(e => e.Id)
            .Select(e => EF.Functions.JsonExtract(e.JsonString, path));

        var results = async ? await query.ToListAsync() : query.ToList();

        Assert.Equal(new object?[] { 8L, 9L, null, null }, results);
        AssertSql(
            """
@path='$.OptionalInt' (Size = 13)

SELECT json_extract("j"."JsonString", @path)
FROM "JsonEntities" AS "j"
ORDER BY "j"."Id"
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public async Task JsonExtract_multiple_paths(bool async)
    {
        await using var context = CreateContext();
        var paths = new[] { "$.RequiredInt", "$.OptionalInt" };
        var query = context.JsonEntities.OrderBy(e => e.Id)
            .Select(e => EF.Functions.JsonExtract(e.JsonString, paths));

        var results = async ? await query.ToListAsync() : query.ToList();

        Assert.Equal(new[] { "[8,8]", "[9,9]", "[10,null]", "[10,null]" }, results);
        AssertSql(
            """
SELECT json_extract("j"."JsonString", '$.RequiredInt', '$.OptionalInt')
FROM "JsonEntities" AS "j"
ORDER BY "j"."Id"
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public async Task JsonExtract_complex_and_owned_json(bool async)
    {
        await using var context = CreateContext();
        var query = context.JsonEntities.OrderBy(e => e.Id)
            .Select(e => new
            {
                Complex = EF.Functions.JsonExtract(e.JsonComplexType, "$.RequiredInt"),
                Owned = EF.Functions.JsonExtract(e.JsonOwnedType, "$.RequiredInt")
            });

        var results = async ? await query.ToListAsync() : query.ToList();

        Assert.Equal(new object?[] { 8L, 9L, 10L, 10L }, results.Select(e => e.Complex));
        Assert.Equal(new object?[] { 8L, 9L, 10L, 10L }, results.Select(e => e.Owned));
        AssertSql(
            """
SELECT json_extract("j"."JsonComplexType", '$.RequiredInt') AS "Complex", json_extract("j"."JsonOwnedType", '$.RequiredInt') AS "Owned"
FROM "JsonEntities" AS "j"
ORDER BY "j"."Id"
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public async Task JsonExtract_null_comparison(bool async)
    {
        await using var context = CreateContext();
        var query = context.JsonEntities
            .Where(e => EF.Functions.JsonExtract(e.JsonString, "$.OptionalInt") == null)
            .OrderBy(e => e.Id)
            .Select(e => e.Id);

        var results = async ? await query.ToListAsync() : query.ToList();

        Assert.Equal(new[] { 3, 4 }, results);
        AssertSql(
            """
SELECT "j"."Id"
FROM "JsonEntities" AS "j"
WHERE json_extract("j"."JsonString", '$.OptionalInt') IS NULL
ORDER BY "j"."Id"
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public async Task JsonExtract_result_types(bool async)
    {
        await using var context = CreateContext();
        var json = """{"number":42,"real":1.5,"true":true,"false":false,"text":"hello","array":[1,2],"object":{"x":1},"null":null}""";
        var query = context.JsonEntities.Where(e => e.Id == 1).Select(e => new
        {
            Number = EF.Functions.JsonExtract(json, "$.number"),
            Real = EF.Functions.JsonExtract(json, "$.real"),
            True = EF.Functions.JsonExtract(json, "$.true"),
            False = EF.Functions.JsonExtract(json, "$.false"),
            Text = EF.Functions.JsonExtract(json, "$.text"),
            Array = EF.Functions.JsonExtract(json, "$.array"),
            Object = EF.Functions.JsonExtract(json, "$.object"),
            Null = EF.Functions.JsonExtract(json, "$.null"),
            Missing = EF.Functions.JsonExtract(json, "$.missing")
        });

        var result = async ? await query.SingleAsync() : query.Single();

        Assert.Equal(42L, result.Number);
        Assert.Equal(1.5, result.Real);
        Assert.Equal(1L, result.True);
        Assert.Equal(0L, result.False);
        Assert.Equal("hello", result.Text);
        Assert.Equal("[1,2]", result.Array);
        Assert.Equal("""{"x":1}""", result.Object);
        Assert.Null(result.Null);
        Assert.Null(result.Missing);
        AssertSql(
            """
@json='{"number":42,"real":1.5,"true":true,"false":false,"text":"hello","array":[1,2],"object":{"x":1},"null":null}' (Size = 108)

SELECT json_extract(@json, '$.number') AS "Number", json_extract(@json, '$.real') AS "Real", json_extract(@json, '$.true') AS "True", json_extract(@json, '$.false') AS "False", json_extract(@json, '$.text') AS "Text", json_extract(@json, '$.array') AS "Array", json_extract(@json, '$.object') AS "Object", json_extract(@json, '$.null') AS "Null", json_extract(@json, '$.missing') AS "Missing"
FROM "JsonEntities" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public async Task JsonExtract_multiple_paths_with_computed_path(bool async)
    {
        await using var context = CreateContext();
        var query = context.JsonEntities.OrderBy(e => e.Id)
            .Select(e => EF.Functions.JsonExtract(e.JsonString, "$.RequiredInt", e.Id == 1 ? "$.OptionalInt" : "$.missing"));

        var results = async ? await query.ToListAsync() : query.ToList();

        Assert.Equal(new[] { "[8,8]", "[9,null]", "[10,null]", "[10,null]" }, results);
        AssertSql(
            """
SELECT json_extract("j"."JsonString", '$.RequiredInt', CASE
    WHEN "j"."Id" = 1 THEN '$.OptionalInt'
    ELSE '$.missing'
END)
FROM "JsonEntities" AS "j"
ORDER BY "j"."Id"
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public async Task JsonExtract_on_value_converted_column(bool async)
    {
        await using var context = CreateContext();
        var query = context.Set<JsonConvertedEntity>().Select(e => EF.Functions.JsonExtract(e.Json, "$.value"));

        var result = async ? await query.SingleAsync() : query.Single();

        Assert.Equal("converted", result);
        AssertSql(
            """
SELECT json_extract("j"."Json", '$.value')
FROM "JsonConvertedEntity" AS "j"
LIMIT 2
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public async Task JsonExtract_numeric_predicate(bool async)
    {
        await using var context = CreateContext();
        var query = context.JsonEntities
            .Where(e => (long)EF.Functions.JsonExtract(e.JsonString, "$.RequiredInt")! > 8)
            .OrderBy(e => e.Id)
            .Select(e => e.Id);

        var results = async ? await query.ToListAsync() : query.ToList();

        Assert.Equal(new[] { 2, 3, 4 }, results);
        AssertSql(
            """
SELECT "j"."Id"
FROM "JsonEntities" AS "j"
WHERE CAST(json_extract("j"."JsonString", '$.RequiredInt') AS INTEGER) > 8
ORDER BY "j"."Id"
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public async Task JsonExtract_path_array_edge_cases(bool async)
    {
        await using var context = CreateContext();
        string[] empty = [];
        string[] single = ["$.RequiredInt"];
        string[] withNull = ["$.RequiredInt", null!];
        var query = context.JsonEntities.Where(e => e.Id == 1).Select(e => new
        {
            Empty = EF.Functions.JsonExtract(e.JsonString, empty),
            Single = EF.Functions.JsonExtract(e.JsonString, single),
            NullPath = EF.Functions.JsonExtract(e.JsonString, withNull)
        });

        var result = async ? await query.SingleAsync() : query.Single();

        Assert.Null(result.Empty);
        Assert.Equal("8", result.Single);
        Assert.Null(result.NullPath);
        AssertSql(
            """
SELECT json_extract("j"."JsonString") AS "Empty", json_extract("j"."JsonString", '$.RequiredInt') AS "Single", json_extract("j"."JsonString", '$.RequiredInt', NULL) AS "NullPath"
FROM "JsonEntities" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    public class JsonTranslationsQuerySqliteFixture : JsonTranslationsQueryFixtureBase, ITestSqlLoggerFactory
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        protected override string RemoveJsonProperty(string column, string property)
            => $"json_remove({column}, '$.{property}')";
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
