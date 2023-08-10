﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

namespace Microsoft.EntityFrameworkCore.Query;

public class JsonQuerySqliteTest : JsonQueryTestBase<JsonQuerySqliteFixture>
{
    public JsonQuerySqliteTest(JsonQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Json_scalar_length(bool async)
    {
        await base.Json_scalar_length(async);

        AssertSql(
"""
SELECT "j"."Name"
FROM "JsonEntitiesBasic" AS "j"
WHERE length("j"."OwnedReferenceRoot" ->> 'Name') > 2
""");
    }

    public override async Task Basic_json_projection_enum_inside_json_entity(bool async)
    {
        await base.Basic_json_projection_enum_inside_json_entity(async);

        AssertSql(
"""
SELECT "j"."Id", "j"."OwnedReferenceRoot" ->> '$.OwnedReferenceBranch.Enum' AS "Enum"
FROM "JsonEntitiesBasic" AS "j"
""");
    }

    public override async Task Project_json_entity_FirstOrDefault_subquery(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_json_entity_FirstOrDefault_subquery(async)))
            .Message);

    public override async Task Project_json_entity_FirstOrDefault_subquery_deduplication(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_json_entity_FirstOrDefault_subquery_deduplication(async)))
            .Message);

    public override async Task Project_json_entity_FirstOrDefault_subquery_deduplication_and_outer_reference(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_json_entity_FirstOrDefault_subquery_deduplication_and_outer_reference(async)))
            .Message);

    public override async Task Project_json_entity_FirstOrDefault_subquery_deduplication_outer_reference_and_pruning(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_json_entity_FirstOrDefault_subquery_deduplication_outer_reference_and_pruning(async)))
            .Message);

    public override async Task Json_collection_Any_with_predicate(bool async)
    {
        await base.Json_collection_Any_with_predicate(async);

        AssertSql(
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT "o"."value" ->> 'Date' AS "Date", "o"."value" ->> 'Enum' AS "Enum", "o"."value" ->> 'Enums' AS "Enums", "o"."value" ->> 'Fraction' AS "Fraction", "o"."value" ->> 'NullableEnum' AS "NullableEnum", "o"."value" ->> 'NullableEnums' AS "NullableEnums", "o"."value" ->> 'OwnedCollectionLeaf' AS "OwnedCollectionLeaf", "o"."value" ->> 'OwnedReferenceLeaf' AS "OwnedReferenceLeaf", "o"."key"
        FROM json_each("j"."OwnedReferenceRoot", '$.OwnedCollectionBranch') AS "o"
    ) AS "t"
    WHERE "t"."OwnedReferenceLeaf" ->> 'SomethingSomething' = 'e1_c2_c1_c1')
""");
    }

    public override async Task Json_collection_Where_ElementAt(bool async)
    {
        await base.Json_collection_Where_ElementAt(async);

        AssertSql(
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
WHERE (
    SELECT "t"."OwnedReferenceLeaf" ->> 'SomethingSomething'
    FROM (
        SELECT "o"."value" ->> 'Date' AS "Date", "o"."value" ->> 'Enum' AS "Enum", "o"."value" ->> 'Enums' AS "Enums", "o"."value" ->> 'Fraction' AS "Fraction", "o"."value" ->> 'NullableEnum' AS "NullableEnum", "o"."value" ->> 'NullableEnums' AS "NullableEnums", "o"."value" ->> 'OwnedCollectionLeaf' AS "OwnedCollectionLeaf", "o"."value" ->> 'OwnedReferenceLeaf' AS "OwnedReferenceLeaf", "o"."key"
        FROM json_each("j"."OwnedReferenceRoot", '$.OwnedCollectionBranch') AS "o"
    ) AS "t"
    WHERE "t"."Enum" = 2
    ORDER BY "t"."key"
    LIMIT 1 OFFSET 0) = 'e1_r_c2_r'
""");
    }

    public override async Task Json_collection_Skip(bool async)
    {
        await base.Json_collection_Skip(async);

        AssertSql(
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
WHERE (
    SELECT "t0"."c"
    FROM (
        SELECT "t"."OwnedReferenceLeaf" ->> 'SomethingSomething' AS "c", "t"."key", "t"."key" AS "key0"
        FROM (
            SELECT "o"."value" ->> 'Date' AS "Date", "o"."value" ->> 'Enum' AS "Enum", "o"."value" ->> 'Enums' AS "Enums", "o"."value" ->> 'Fraction' AS "Fraction", "o"."value" ->> 'NullableEnum' AS "NullableEnum", "o"."value" ->> 'NullableEnums' AS "NullableEnums", "o"."value" ->> 'OwnedCollectionLeaf' AS "OwnedCollectionLeaf", "o"."value" ->> 'OwnedReferenceLeaf' AS "OwnedReferenceLeaf", "o"."key"
            FROM json_each("j"."OwnedReferenceRoot", '$.OwnedCollectionBranch') AS "o"
        ) AS "t"
        ORDER BY "t"."key"
        LIMIT -1 OFFSET 1
    ) AS "t0"
    ORDER BY "t0"."key0"
    LIMIT 1 OFFSET 0) = 'e1_r_c2_r'
""");
    }

    public override async Task Json_collection_OrderByDescending_Skip_ElementAt(bool async)
    {
        await base.Json_collection_OrderByDescending_Skip_ElementAt(async);

        AssertSql(
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
WHERE (
    SELECT "t0"."c"
    FROM (
        SELECT "t"."OwnedReferenceLeaf" ->> 'SomethingSomething' AS "c", "t"."key", "t"."Date" AS "c0"
        FROM (
            SELECT "o"."value" ->> 'Date' AS "Date", "o"."value" ->> 'Enum' AS "Enum", "o"."value" ->> 'Enums' AS "Enums", "o"."value" ->> 'Fraction' AS "Fraction", "o"."value" ->> 'NullableEnum' AS "NullableEnum", "o"."value" ->> 'NullableEnums' AS "NullableEnums", "o"."value" ->> 'OwnedCollectionLeaf' AS "OwnedCollectionLeaf", "o"."value" ->> 'OwnedReferenceLeaf' AS "OwnedReferenceLeaf", "o"."key"
            FROM json_each("j"."OwnedReferenceRoot", '$.OwnedCollectionBranch') AS "o"
        ) AS "t"
        ORDER BY "t"."Date" DESC
        LIMIT -1 OFFSET 1
    ) AS "t0"
    ORDER BY "t0"."c0" DESC
    LIMIT 1 OFFSET 0) = 'e1_r_c1_r'
""");
    }

    public override async Task Json_collection_within_collection_Count(bool async)
    {
        await base.Json_collection_within_collection_Count(async);

        AssertSql(
"""
SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT "o"."value" ->> 'Name' AS "Name", "o"."value" ->> 'Names' AS "Names", "o"."value" ->> 'Number' AS "Number", "o"."value" ->> 'Numbers' AS "Numbers", "o"."value" ->> 'OwnedCollectionBranch' AS "OwnedCollectionBranch", "o"."value" ->> 'OwnedReferenceBranch' AS "OwnedReferenceBranch", "o"."key"
        FROM json_each("j"."OwnedCollectionRoot", '$') AS "o"
    ) AS "t"
    WHERE (
        SELECT COUNT(*)
        FROM (
            SELECT "o0"."value" ->> 'Date' AS "Date", "o0"."value" ->> 'Enum' AS "Enum", "o0"."value" ->> 'Enums' AS "Enums", "o0"."value" ->> 'Fraction' AS "Fraction", "o0"."value" ->> 'NullableEnum' AS "NullableEnum", "o0"."value" ->> 'NullableEnums' AS "NullableEnums", "o0"."value" ->> 'OwnedCollectionLeaf' AS "OwnedCollectionLeaf", "o0"."value" ->> 'OwnedReferenceLeaf' AS "OwnedReferenceLeaf", "o0"."key"
            FROM json_each("t"."OwnedCollectionBranch", '$') AS "o0"
        ) AS "t0") = 2)
""");
    }

    public override async Task Json_collection_Select_entity_with_initializer_ElementAt(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Json_collection_Select_entity_with_initializer_ElementAt(async)))
            .Message);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSqlInterpolated_on_entity_with_json_with_predicate(bool async)
    {
        var parameter = new SqliteParameter { ParameterName = "prm", Value = 1 };
        await AssertQuery(
            async,
            ss => ((DbSet<JsonEntityBasic>)ss.Set<JsonEntityBasic>()).FromSql(
                Fixture.TestStore.NormalizeDelimitersInInterpolatedString($"SELECT * FROM [JsonEntitiesBasic] AS j WHERE [j].[Id] = {parameter}")),
            ss => ss.Set<JsonEntityBasic>(),
            entryCount: 40);

        AssertSql(
"""
prm='1' (DbType = String)

SELECT "m"."Id", "m"."EntityBasicId", "m"."Name", "m"."OwnedCollectionRoot", "m"."OwnedReferenceRoot"
FROM (
    SELECT * FROM "JsonEntitiesBasic" AS j WHERE "j"."Id" = @prm
) AS "m"
""");
    }

    public override async Task Json_collection_index_in_predicate_nested_mix(bool async)
    {
        await base.Json_collection_index_in_predicate_nested_mix(async);

        AssertSql(
"""
@__prm_0='0'

SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
WHERE "j"."OwnedCollectionRoot" ->> '$[1].OwnedCollectionBranch' ->> @__prm_0 ->> 'OwnedCollectionLeaf' ->> ("j"."Id" - 1) ->> 'SomethingSomething' = 'e1_c2_c1_c1'
""");
    }

    public override async Task Json_predicate_on_bool_converted_to_int_zero_one(bool async)
    {
        await base.Json_predicate_on_bool_converted_to_int_zero_one(async);

        AssertSql(
"""
SELECT "j"."Id", "j"."Reference"
FROM "JsonEntitiesConverters" AS "j"
WHERE "j"."Reference" ->> 'BoolConvertedToIntZeroOne' = 1
""");
    }

    public override async Task Json_predicate_on_bool_converted_to_string_True_False(bool async)
    {
        await base.Json_predicate_on_bool_converted_to_string_True_False(async);

        AssertSql(
"""
SELECT "j"."Id", "j"."Reference"
FROM "JsonEntitiesConverters" AS "j"
WHERE "j"."Reference" ->> 'BoolConvertedToStringTrueFalse' = 'True'
""");
    }

    public override async Task Json_predicate_on_bool_converted_to_string_Y_N(bool async)
    {
        await base.Json_predicate_on_bool_converted_to_string_Y_N(async);

        AssertSql(
"""
SELECT "j"."Id", "j"."Reference"
FROM "JsonEntitiesConverters" AS "j"
WHERE "j"."Reference" ->> 'BoolConvertedToStringYN' = 'Y'
""");
    }

    public override async Task Json_collection_in_projection_with_anonymous_projection_of_scalars(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Json_collection_in_projection_with_anonymous_projection_of_scalars(async)))
            .Message);

    public override async Task Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_scalars(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_scalars(async)))
            .Message);

    public override async Task Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_primitive_arrays(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_primitive_arrays(async)))
            .Message);

    public override async Task Json_collection_Select_entity_in_anonymous_object_ElementAt(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Json_collection_Select_entity_in_anonymous_object_ElementAt(async)))
            .Message);

    public override async Task Json_collection_skip_take_in_projection_project_into_anonymous_type(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Json_collection_skip_take_in_projection_project_into_anonymous_type(async)))
            .Message);

    public override async Task Json_collection_skip_take_in_projection_with_json_reference_access_as_final_operation(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Json_collection_skip_take_in_projection_with_json_reference_access_as_final_operation(async)))
            .Message);

    public override async Task Json_collection_index_in_projection_using_untranslatable_client_method(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Json_collection_index_in_projection_using_untranslatable_client_method(async))).Message;

        Assert.Contains(CoreStrings.QueryUnableToTranslateMethod(
            "Microsoft.EntityFrameworkCore.Query.JsonQueryTestBase<Microsoft.EntityFrameworkCore.Query.JsonQuerySqliteFixture>",
            "MyMethod"),
            message);
    }

    public override async Task Json_collection_index_in_projection_using_untranslatable_client_method2(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Json_collection_index_in_projection_using_untranslatable_client_method2(async))).Message;

        Assert.Contains(CoreStrings.QueryUnableToTranslateMethod(
            "Microsoft.EntityFrameworkCore.Query.JsonQueryTestBase<Microsoft.EntityFrameworkCore.Query.JsonQuerySqliteFixture>",
            "MyMethod"),
            message);
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
