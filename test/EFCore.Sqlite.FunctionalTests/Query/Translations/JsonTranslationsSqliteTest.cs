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
    public async Task JsonExtract_single_path_array_string_predicate(bool async)
    {
        await using var context = CreateContext();
        string[] paths = ["$.RequiredInt"];
        var query = context.JsonEntities
            .Where(e => EF.Functions.JsonExtract(e.JsonString, paths) == "8")
            .Select(e => e.Id);

        var results = async ? await query.ToListAsync() : query.ToList();

        Assert.Equal(new[] { 1 }, results);
        AssertSql(
            """
SELECT "j"."Id"
FROM "JsonEntities" AS "j"
WHERE CAST(json_extract("j"."JsonString", '$.RequiredInt') AS TEXT) = '8'
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public async Task JsonExtract_object_equals_numeric_predicate(bool async)
    {
        await using var context = CreateContext();
        var query = context.JsonEntities
            .Where(e => object.Equals(EF.Functions.JsonExtract(e.JsonString, "$.RequiredInt"), 8L))
            .Select(e => e.Id);

        var results = async ? await query.ToListAsync() : query.ToList();

        Assert.Equal(new[] { 1 }, results);
        AssertSql(
            """
SELECT "j"."Id"
FROM "JsonEntities" AS "j"
WHERE typeof(json_extract("j"."JsonString", '$.RequiredInt')) = 'integer' AND json_extract("j"."JsonString", '$.RequiredInt') = 8
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public async Task JsonExtract_object_equals_respects_result_types_and_nulls(bool async)
    {
        await using var context = CreateContext();
        var json = """{"integer":8,"real":8.0,"text":"8","null":null}""";
        var query = context.JsonEntities.Where(e => e.Id == 1).Select(e => new
        {
            EqualInteger = object.Equals(EF.Functions.JsonExtract(json, "$.integer"), 8L),
            DifferentIntegerType = object.Equals(EF.Functions.JsonExtract(json, "$.integer"), 8),
            DifferentNumericType = object.Equals(EF.Functions.JsonExtract(json, "$.integer"), 8.0),
            EqualReal = object.Equals(EF.Functions.JsonExtract(json, "$.real"), 8.0),
            EqualText = object.Equals(EF.Functions.JsonExtract(json, "$.text"), "8"),
            InstanceEquals = EF.Functions.JsonExtract(json, "$.integer")!.Equals(8L),
            DifferentDynamicTypes = object.Equals(
                EF.Functions.JsonExtract(json, "$.integer"), EF.Functions.JsonExtract(json, "$.real")),
            EqualDynamicValues = object.Equals(
                EF.Functions.JsonExtract(json, "$.integer"), EF.Functions.JsonExtract(json, "$.integer")),
            EqualNulls = object.Equals(
                EF.Functions.JsonExtract(json, "$.null"), EF.Functions.JsonExtract(json, "$.missing"))
        });

        var result = async ? await query.SingleAsync() : query.Single();

        Assert.True(result.EqualInteger);
        Assert.False(result.DifferentIntegerType);
        Assert.False(result.DifferentNumericType);
        Assert.True(result.EqualReal);
        Assert.True(result.EqualText);
        Assert.True(result.InstanceEquals);
        Assert.False(result.DifferentDynamicTypes);
        Assert.True(result.EqualDynamicValues);
        Assert.True(result.EqualNulls);
        AssertSql(
            """
@json='{"integer":8,"real":8.0,"text":"8","null":null}' (Size = 47)

SELECT typeof(json_extract(@json, '$.integer')) = 'integer' AND json_extract(@json, '$.integer') = 8 AND json_extract(@json, '$.integer') IS NOT NULL AS "EqualInteger", 0 AS "DifferentIntegerType", typeof(json_extract(@json, '$.integer')) = 'real' AND json_extract(@json, '$.integer') = 8.0 AND json_extract(@json, '$.integer') IS NOT NULL AS "DifferentNumericType", typeof(json_extract(@json, '$.real')) = 'real' AND json_extract(@json, '$.real') = 8.0 AND json_extract(@json, '$.real') IS NOT NULL AS "EqualReal", typeof(json_extract(@json, '$.text')) = 'text' AND json_extract(@json, '$.text') = '8' AND json_extract(@json, '$.text') IS NOT NULL AS "EqualText", (json_extract(@json, '$.integer') IS NULL AND json_extract(@json, '$.real') IS NULL) OR (typeof(json_extract(@json, '$.integer')) = typeof(json_extract(@json, '$.real')) AND ((json_extract(@json, '$.integer') = json_extract(@json, '$.real') AND json_extract(@json, '$.integer') IS NOT NULL AND json_extract(@json, '$.real') IS NOT NULL) OR (json_extract(@json, '$.integer') IS NULL AND json_extract(@json, '$.real') IS NULL))) AS "DifferentDynamicTypes", json_extract(@json, '$.integer') IS NULL OR (json_extract(@json, '$.integer') = json_extract(@json, '$.integer') AND json_extract(@json, '$.integer') IS NOT NULL) OR json_extract(@json, '$.integer') IS NULL AS "EqualDynamicValues", (json_extract(@json, '$.null') IS NULL AND json_extract(@json, '$.missing') IS NULL) OR (typeof(json_extract(@json, '$.null')) = typeof(json_extract(@json, '$.missing')) AND ((json_extract(@json, '$.null') = json_extract(@json, '$.missing') AND json_extract(@json, '$.null') IS NOT NULL AND json_extract(@json, '$.missing') IS NOT NULL) OR (json_extract(@json, '$.null') IS NULL AND json_extract(@json, '$.missing') IS NULL))) AS "EqualNulls"
FROM "JsonEntities" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public async Task JsonExtract_object_equals_with_blob_fallback_is_not_translated(bool async)
    {
        await using var context = CreateContext();
        byte[] fallback = [1, 2];
        var query = context.JsonEntities.Select(e =>
            object.Equals(EF.Functions.JsonExtract(e.JsonString, "$.missing") ?? fallback, fallback));

        if (async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync());
        }
        else
        {
            Assert.Throws<InvalidOperationException>(() => query.ToList());
        }
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public async Task JsonExtract_object_equals_dynamic_fallback_is_not_translated(bool async)
    {
        await using var context = CreateContext();
        byte[] fallback = [1, 2];
        var query = context.JsonEntities.Select(e =>
            object.Equals(
                EF.Functions.JsonExtract(e.JsonString, "$.missing") ?? fallback,
                EF.Functions.JsonExtract(e.JsonString, "$.missing") ?? fallback));

        if (async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync());
        }
        else
        {
            Assert.Throws<InvalidOperationException>(() => query.ToList());
        }
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public async Task JsonExtract_coalesce_string_literal(bool async)
    {
        await using var context = CreateContext();
        var query = context.JsonEntities.Where(e => e.Id == 1)
            .Select(e => EF.Functions.JsonExtract(e.JsonString, "$.missing") ?? "don't'); SELECT 1; --");

        var result = async ? await query.SingleAsync() : query.Single();

        Assert.Equal("don't'); SELECT 1; --", result);
        AssertSql(
            """
SELECT COALESCE(json_extract("j"."JsonString", '$.missing'), 'don''t''); SELECT 1; --')
FROM "JsonEntities" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
""");
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public async Task JsonExtract_coalesce_numeric_literals(bool async)
    {
        await using var context = CreateContext();
        var query = context.JsonEntities.Where(e => e.Id == 1).Select(e => new
        {
            Integer = EF.Functions.JsonExtract(e.JsonString, "$.missing") ?? 8L,
            Real = EF.Functions.JsonExtract(e.JsonString, "$.missing") ?? 1.5
        });

        var result = async ? await query.SingleAsync() : query.Single();

        Assert.Equal(8L, Assert.IsType<long>(result.Integer));
        Assert.Equal(1.5, Assert.IsType<double>(result.Real));
        AssertSql(
            """
SELECT COALESCE(json_extract("j"."JsonString", '$.missing'), 8) AS "Integer", COALESCE(json_extract("j"."JsonString", '$.missing'), 1.5) AS "Real"
FROM "JsonEntities" AS "j"
WHERE "j"."Id" = 1
LIMIT 2
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
SELECT json_extract("j"."JsonString") AS "Empty", CAST(json_extract("j"."JsonString", '$.RequiredInt') AS TEXT) AS "Single", json_extract("j"."JsonString", '$.RequiredInt', NULL) AS "NullPath"
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
