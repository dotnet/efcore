// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;
using Xunit.Sdk;

#pragma warning disable EF8001 // Owned JSON entities are obsolete

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class JsonQuerySqliteTest : JsonQueryRelationalTestBase<JsonQuerySqliteFixture>
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

    //public override async Task Project_json_entity_FirstOrDefault_subquery(bool async)
    //    => Assert.Equal(
    //        SqliteStrings.ApplyNotSupported,
    //        (await Assert.ThrowsAsync<InvalidOperationException>(
    //            () => base.Project_json_entity_FirstOrDefault_subquery(async)))
    //        .Message);

    //public override async Task Project_json_entity_FirstOrDefault_subquery_deduplication(bool async)
    //    => Assert.Equal(
    //        SqliteStrings.ApplyNotSupported,
    //        (await Assert.ThrowsAsync<InvalidOperationException>(
    //            () => base.Project_json_entity_FirstOrDefault_subquery_deduplication(async)))
    //        .Message);

    //public override async Task Project_json_entity_FirstOrDefault_subquery_deduplication_and_outer_reference(bool async)
    //    => Assert.Equal(
    //        SqliteStrings.ApplyNotSupported,
    //        (await Assert.ThrowsAsync<InvalidOperationException>(
    //            () => base.Project_json_entity_FirstOrDefault_subquery_deduplication_and_outer_reference(async)))
    //        .Message);

    //public override async Task Project_json_entity_FirstOrDefault_subquery_deduplication_outer_reference_and_pruning(bool async)
    //    => Assert.Equal(
    //        SqliteStrings.ApplyNotSupported,
    //        (await Assert.ThrowsAsync<InvalidOperationException>(
    //            () => base.Project_json_entity_FirstOrDefault_subquery_deduplication_outer_reference_and_pruning(async)))
    //        .Message);

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
        SELECT "o"."value" ->> 'OwnedReferenceLeaf' AS "OwnedReferenceLeaf"
        FROM json_each("j"."OwnedReferenceRoot", '$.OwnedCollectionBranch') AS "o"
    ) AS "o0"
    WHERE "o0"."OwnedReferenceLeaf" ->> 'SomethingSomething' = 'e1_r_c1_r')
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
    SELECT "o0"."OwnedReferenceLeaf" ->> 'SomethingSomething'
    FROM (
        SELECT "o"."value" ->> 'Date' AS "Date", "o"."value" ->> 'Enum' AS "Enum", "o"."value" ->> 'Fraction' AS "Fraction", "o"."value" ->> 'Id' AS "Id", "o"."value" ->> 'OwnedReferenceLeaf' AS "OwnedReferenceLeaf", "o"."key"
        FROM json_each("j"."OwnedReferenceRoot", '$.OwnedCollectionBranch') AS "o"
    ) AS "o0"
    WHERE "o0"."Enum" = -3
    ORDER BY "o0"."key"
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
    SELECT "o1"."c"
    FROM (
        SELECT "o0"."OwnedReferenceLeaf" ->> 'SomethingSomething' AS "c", "o0"."key"
        FROM (
            SELECT "o"."value" ->> 'OwnedReferenceLeaf' AS "OwnedReferenceLeaf", "o"."key"
            FROM json_each("j"."OwnedReferenceRoot", '$.OwnedCollectionBranch') AS "o"
        ) AS "o0"
        ORDER BY "o0"."key"
        LIMIT -1 OFFSET 1
    ) AS "o1"
    ORDER BY "o1"."key"
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
    SELECT "o1"."c"
    FROM (
        SELECT "o0"."OwnedReferenceLeaf" ->> 'SomethingSomething' AS "c", "o0"."Date" AS "c0"
        FROM (
            SELECT "o"."value" ->> 'Date' AS "Date", "o"."value" ->> 'Enum' AS "Enum", "o"."value" ->> 'Fraction' AS "Fraction", "o"."value" ->> 'Id' AS "Id", "o"."value" ->> 'OwnedReferenceLeaf' AS "OwnedReferenceLeaf"
            FROM json_each("j"."OwnedReferenceRoot", '$.OwnedCollectionBranch') AS "o"
        ) AS "o0"
        ORDER BY "o0"."Date" DESC
        LIMIT -1 OFFSET 1
    ) AS "o1"
    ORDER BY "o1"."c0" DESC
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
        SELECT "o"."value" ->> 'OwnedCollectionBranch' AS "OwnedCollectionBranch"
        FROM json_each("j"."OwnedCollectionRoot", '$') AS "o"
    ) AS "o0"
    WHERE (
        SELECT COUNT(*)
        FROM (
            SELECT 1
            FROM json_each("o0"."OwnedCollectionBranch", '$') AS "o1"
        ) AS "o2") = 2)
""");
    }

    public override async Task Json_collection_Select_entity_with_initializer_ElementAt(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(()
                => base.Json_collection_Select_entity_with_initializer_ElementAt(async)))
            .Message);

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSqlInterpolated_on_entity_with_json_with_predicate(bool async)
    {
        var parameter = new SqliteParameter { ParameterName = "prm", Value = 1 };
        await AssertQuery(
            async,
            ss => ((DbSet<JsonEntityBasic>)ss.Set<JsonEntityBasic>()).FromSql(
                Fixture.TestStore.NormalizeDelimitersInInterpolatedString(
                    $"SELECT * FROM [JsonEntitiesBasic] AS j WHERE [j].[Id] = {parameter}")),
            ss => ss.Set<JsonEntityBasic>());

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
@prm='0'

SELECT "j"."Id", "j"."EntityBasicId", "j"."Name", "j"."OwnedCollectionRoot", "j"."OwnedReferenceRoot"
FROM "JsonEntitiesBasic" AS "j"
WHERE "j"."OwnedCollectionRoot" ->> '$[1].OwnedCollectionBranch' ->> @prm ->> 'OwnedCollectionLeaf' ->> ("j"."Id" - 1) ->> 'SomethingSomething' = 'e1_c2_c1_c1'
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

    // #33522
    public override Task Json_predicate_on_byte_array(bool async)
        => Assert.ThrowsAsync<EqualException>(() => base.Json_predicate_on_byte_array(async));

    public override async Task Json_collection_in_projection_with_anonymous_projection_of_scalars(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(()
                => base.Json_collection_in_projection_with_anonymous_projection_of_scalars(async)))
            .Message);

    public override async Task Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_scalars(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(()
                => base.Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_scalars(async)))
            .Message);

    public override async Task Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_primitive_arrays(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(()
                => base.Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_primitive_arrays(async)))
            .Message);

    public override async Task Json_collection_Select_entity_in_anonymous_object_ElementAt(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(()
                => base.Json_collection_Select_entity_in_anonymous_object_ElementAt(async)))
            .Message);

    public override async Task Json_collection_skip_take_in_projection_project_into_anonymous_type(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(()
                => base.Json_collection_skip_take_in_projection_project_into_anonymous_type(async)))
            .Message);

    public override async Task Json_collection_skip_take_in_projection_with_json_reference_access_as_final_operation(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(()
                => base.Json_collection_skip_take_in_projection_with_json_reference_access_as_final_operation(async)))
            .Message);

    public override async Task Json_collection_distinct_in_projection(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Json_collection_distinct_in_projection(async)))
            .Message);

    public override async Task Json_collection_filter_in_projection(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Json_collection_filter_in_projection(async)))
            .Message);

    public override async Task Json_collection_leaf_filter_in_projection(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Json_collection_leaf_filter_in_projection(async)))
            .Message);

    public override async Task Json_branch_collection_distinct_and_other_collection(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Json_branch_collection_distinct_and_other_collection(async)))
            .Message);

    public override async Task Json_leaf_collection_distinct_and_other_collection(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Json_leaf_collection_distinct_and_other_collection(async)))
            .Message);

    public override async Task Json_multiple_collection_projections(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Json_multiple_collection_projections(async)))
            .Message);

    //public override async Task Json_collection_SelectMany(bool async)
    //    => Assert.Equal(
    //        SqliteStrings.ApplyNotSupported,
    //        (await Assert.ThrowsAsync<InvalidOperationException>(
    //            () => base.Json_collection_SelectMany(async)))
    //        .Message);

    public override async Task Json_collection_skip_take_in_projection(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Json_collection_skip_take_in_projection(async)))
            .Message);

    public override async Task Json_nested_collection_anonymous_projection_in_projection(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(()
                => base.Json_nested_collection_anonymous_projection_in_projection(async)))
            .Message);

    public override async Task Json_nested_collection_filter_in_projection(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Json_nested_collection_filter_in_projection(async)))
            .Message);

    //public override async Task Json_nested_collection_SelectMany(bool async)
    //    => Assert.Equal(
    //        SqliteStrings.ApplyNotSupported,
    //        (await Assert.ThrowsAsync<InvalidOperationException>(
    //            () => base.Json_nested_collection_SelectMany(async)))
    //        .Message);

    public override async Task Json_collection_of_primitives_SelectMany(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Json_collection_of_primitives_SelectMany(async)))
            .Message);

    public override async Task Json_collection_index_in_projection_using_untranslatable_client_method(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(()
            => base.Json_collection_index_in_projection_using_untranslatable_client_method(async))).Message;

        Assert.Contains(
            CoreStrings.QueryUnableToTranslateMethod(
                "Microsoft.EntityFrameworkCore.Query.JsonQueryTestBase<Microsoft.EntityFrameworkCore.Query.JsonQuerySqliteFixture>",
                "MyMethod"),
            message);
    }

    public override async Task Json_collection_index_in_projection_using_untranslatable_client_method2(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(()
            => base.Json_collection_index_in_projection_using_untranslatable_client_method2(async))).Message;

        Assert.Contains(
            CoreStrings.QueryUnableToTranslateMethod(
                "Microsoft.EntityFrameworkCore.Query.JsonQueryTestBase<Microsoft.EntityFrameworkCore.Query.JsonQuerySqliteFixture>",
                "MyMethod"),
            message);
    }

    public override async Task Custom_naming_projection_everything(bool async)
    {
        await base.Custom_naming_projection_everything(async);

        AssertSql(
            """
SELECT "j"."Id", "j"."Title", "j"."json_collection_custom_naming", "j"."json_reference_custom_naming", "j"."json_reference_custom_naming", "j"."json_reference_custom_naming" ->> '"Custom#OwnedReferenceBranch\u0060-=[]\\;\u0027,./~!@#$%^\u0026*()_\u002B{}|:\u0022\u003C\u003E?\u72EC\u89D2\u517D\u03C0\u7368\u89D2\u7378"', "j"."json_collection_custom_naming", "j"."json_reference_custom_naming" ->> 'CustomOwnedCollectionBranch', "j"."json_reference_custom_naming" ->> 'CustomName', "j"."json_reference_custom_naming" ->> '$."Custom#OwnedReferenceBranch\u0060-=[]\\;\u0027,./~!@#$%^\u0026*()_\u002B{}|:\u0022\u003C\u003E?\u72EC\u89D2\u517D\u03C0\u7368\u89D2\u7378"."\u30E6\u30CB\u30B3\u30FC\u30F3Fraction\u4E00\u89D2\u7363"'
FROM "JsonEntitiesCustomNaming" AS "j"
""");
    }

    public override async Task Custom_naming_projection_owned_scalar(bool async)
    {
        await base.Custom_naming_projection_owned_scalar(async);

        AssertSql(
            """
SELECT "j"."json_reference_custom_naming" ->> '$."Custom#OwnedReferenceBranch\u0060-=[]\\;\u0027,./~!@#$%^\u0026*()_\u002B{}|:\u0022\u003C\u003E?\u72EC\u89D2\u517D\u03C0\u7368\u89D2\u7378"."\u30E6\u30CB\u30B3\u30FC\u30F3Fraction\u4E00\u89D2\u7363"'
FROM "JsonEntitiesCustomNaming" AS "j"
""");
    }

    public override async Task Custom_naming_projection_owned_reference(bool async)
    {
        await base.Custom_naming_projection_owned_reference(async);

        AssertSql(
            """
SELECT "j"."json_reference_custom_naming" ->> '"Custom#OwnedReferenceBranch\u0060-=[]\\;\u0027,./~!@#$%^\u0026*()_\u002B{}|:\u0022\u003C\u003E?\u72EC\u89D2\u517D\u03C0\u7368\u89D2\u7378"', "j"."Id"
FROM "JsonEntitiesCustomNaming" AS "j"
""");
    }

    public override async Task Json_nested_collection_anonymous_projection_of_primitives_in_projection_NoTrackingWithIdentityResolution(
        bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(()
                => base.Json_nested_collection_anonymous_projection_of_primitives_in_projection_NoTrackingWithIdentityResolution(async)))
            .Message);

    // Sqlit throws APPLY error, but base expects different exception
    public override Task Json_branch_collection_distinct_and_other_collection_AsNoTrackingWithIdentityResolution(bool async)
        => Task.CompletedTask;

    public override Task Json_collection_SelectMany_AsNoTrackingWithIdentityResolution(bool async)
        => Task.CompletedTask;

    public override Task Json_nested_collection_anonymous_projection_in_projection_NoTrackingWithIdentityResolution(bool async)
        => Task.CompletedTask;

    public override Task Json_projection_using_queryable_methods_on_top_of_JSON_collection_AsNoTrackingWithIdentityResolution(bool async)
        => Task.CompletedTask;


    protected override async Task Seed21006(Context21006 context)
    {
        await base.Seed21006(context);

        // missing scalar on top level
        await context.Database.ExecuteSqlAsync(
            $$$"""
INSERT INTO "Entities" ("Collection", "OptionalReference", "RequiredReference", "Id", "Name")
VALUES (
'[{"Text":"e2 c1","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e2 c1 c1"},{"DoB":"2000-01-01T00:00:00","Text":"e2 c1 c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e2 c1 nor"},"NestedRequiredReference":{"DoB":"2000-01-01T00:00:00","Text":"e2 c1 nrr"}},{"Text":"e2 c2","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e2 c2 c1"},{"DoB":"2000-01-01T00:00:00","Text":"e2 c2 c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e2 c2 nor"},"NestedRequiredReference":{"DoB":"2000-01-01T00:00:00","Text":"e2 c2 nrr"}}]',
'{"Text":"e2 or","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e2 or c1"},{"DoB":"2000-01-01T00:00:00","Text":"e2 or c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e2 or nor"},"NestedRequiredReference":{"DoB":"2000-01-01T00:00:00","Text":"e2 or nrr"}}',
'{"Text":"e2 rr","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e2 rr c1"},{"DoB":"2000-01-01T00:00:00","Text":"e2 rr c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e2 rr nor"},"NestedRequiredReference":{"DoB":"2000-01-01T00:00:00","Text":"e2 rr nrr"}}',
2,
'e2')
""");

        // missing scalar on nested level
        await context.Database.ExecuteSqlAsync(
            $$$"""
INSERT INTO "Entities" ("Collection", "OptionalReference", "RequiredReference", "Id", "Name")
VALUES (
'[{"Number":7,"Text":"e3 c1","NestedCollection":[{"Text":"e3 c1 c1"},{"Text":"e3 c1 c2"}],"NestedOptionalReference":{"Text":"e3 c1 nor"},"NestedRequiredReference":{"Text":"e3 c1 nrr"}},{"Number":7,"Text":"e3 c2","NestedCollection":[{"Text":"e3 c2 c1"},{"Text":"e3 c2 c2"}],"NestedOptionalReference":{"Text":"e3 c2 nor"},"NestedRequiredReference":{"Text":"e3 c2 nrr"}}]',
'{"Number":7,"Text":"e3 or","NestedCollection":[{"Text":"e3 or c1"},{"Text":"e3 or c2"}],"NestedOptionalReference":{"Text":"e3 or nor"},"NestedRequiredReference":{"Text":"e3 or nrr"}}',
'{"Number":7,"Text":"e3 rr","NestedCollection":[{"Text":"e3 rr c1"},{"Text":"e3 rr c2"}],"NestedOptionalReference":{"Text":"e3 rr nor"},"NestedRequiredReference":{"Text":"e3 rr nrr"}}',
3,
'e3')
""");

        // null scalar on top level
        await context.Database.ExecuteSqlAsync(
            $$$"""
INSERT INTO [Entities] ("Collection", "OptionalReference", "RequiredReference", "Id", "Name")
VALUES (
'[{"Number":null,"Text":"e4 c1","NestedCollection":[{"Text":"e4 c1 c1"},{"Text":"e4 c1 c2"}],"NestedOptionalReference":{"Text":"e4 c1 nor"},"NestedRequiredReference":{"Text":"e4 c1 nrr"}},{"Number":null,"Text":"e4 c2","NestedCollection":[{"Text":"e4 c2 c1"},{"Text":"e4 c2 c2"}],"NestedOptionalReference":{"Text":"e4 c2 nor"},"NestedRequiredReference":{"Text":"e4 c2 nrr"}}]',
'{"Number":null,"Text":"e4 or","NestedCollection":[{"Text":"e4 or c1"},{"Text":"e4 or c2"}],"NestedOptionalReference":{"Text":"e4 or nor"},"NestedRequiredReference":{"Text":"e4 or nrr"}}',
'{"Number":null,"Text":"e4 rr","NestedCollection":[{"Text":"e4 rr c1"},{"Text":"e4 rr c2"}],"NestedOptionalReference":{"Text":"e4 rr nor"},"NestedRequiredReference":{"Text":"e4 rr nrr"}}',
4,
'e4')
""");

        // missing required navigation
        await context.Database.ExecuteSqlAsync(
            $$$"""
INSERT INTO "Entities" ("Collection", "OptionalReference", "RequiredReference", "Id", "Name")
VALUES (
'[{"Number":7,"Text":"e5 c1","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e5 c1 c1"},{"DoB":"2000-01-01T00:00:00","Text":"e5 c1 c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e5 c1 nor"}},{"Number":7,"Text":"e5 c2","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e5 c2 c1"},{"DoB":"2000-01-01T00:00:00","Text":"e5 c2 c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e5 c2 nor"}}]',
'{"Number":7,"Text":"e5 or","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e5 or c1"},{"DoB":"2000-01-01T00:00:00","Text":"e5 or c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e5 or nor"}}',
'{"Number":7,"Text":"e5 rr","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e5 rr c1"},{"DoB":"2000-01-01T00:00:00","Text":"e5 rr c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e5 rr nor"}}',
5,
'e5')
""");

        // null required navigation
        await context.Database.ExecuteSqlAsync(
            $$$"""
INSERT INTO "Entities" ("Collection", "OptionalReference", "RequiredReference", "Id", "Name")
VALUES (
'[{"Number":7,"Text":"e6 c1","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e6 c1 c1"},{"DoB":"2000-01-01T00:00:00","Text":"e6 c1 c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e6 c1 nor"},"NestedRequiredReference":null},{"Number":7,"Text":"e6 c2","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e6 c2 c1"},{"DoB":"2000-01-01T00:00:00","Text":"e6 c2 c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e6 c2 nor"},"NestedRequiredReference":null}]',
'{"Number":7,"Text":"e6 or","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e6 or c1"},{"DoB":"2000-01-01T00:00:00","Text":"e6 or c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e6 or nor"},"NestedRequiredReference":null}',
'{"Number":7,"Text":"e6 rr","NestedCollection":[{"DoB":"2000-01-01T00:00:00","Text":"e6 rr c1"},{"DoB":"2000-01-01T00:00:00","Text":"e6 rr c2"}],"NestedOptionalReference":{"DoB":"2000-01-01T00:00:00","Text":"e6 rr nor"},"NestedRequiredReference":null}',
6,
'e6')
""");
    }

    protected override async Task Seed29219(DbContext ctx)
    {
        await base.Seed29219(ctx);

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO "Entities" ("Id", "Reference", "Collection")
VALUES(3, '{ "NonNullableScalar" : 30 }', '[{ "NonNullableScalar" : 10001 }]')
""");
    }

    protected override async Task Seed30028(DbContext ctx)
    {
        // complete
        await ctx.Database.ExecuteSqlAsync(
            $$$$"""
INSERT INTO "Entities" ("Id", "Json")
VALUES(
1,
'{"RootName":"e1","Collection":[{"BranchName":"e1 c1","Nested":{"LeafName":"e1 c1 l"}},{"BranchName":"e1 c2","Nested":{"LeafName":"e1 c2 l"}}],"OptionalReference":{"BranchName":"e1 or","Nested":{"LeafName":"e1 or l"}},"RequiredReference":{"BranchName":"e1 rr","Nested":{"LeafName":"e1 rr l"}}}')
""");

        // missing collection
        await ctx.Database.ExecuteSqlAsync(
            $$$$"""
INSERT INTO "Entities" ("Id", "Json")
VALUES(
2,
'{"RootName":"e2","OptionalReference":{"BranchName":"e2 or","Nested":{"LeafName":"e2 or l"}},"RequiredReference":{"BranchName":"e2 rr","Nested":{"LeafName":"e2 rr l"}}}')
""");

        // missing optional reference
        await ctx.Database.ExecuteSqlAsync(
            $$$$"""
INSERT INTO "Entities" ("Id", "Json")
VALUES(
3,
'{"RootName":"e3","Collection":[{"BranchName":"e3 c1","Nested":{"LeafName":"e3 c1 l"}},{"BranchName":"e3 c2","Nested":{"LeafName":"e3 c2 l"}}],"RequiredReference":{"BranchName":"e3 rr","Nested":{"LeafName":"e3 rr l"}}}')
""");

        // missing required reference
        await ctx.Database.ExecuteSqlAsync(
            $$$$"""
INSERT INTO "Entities" ("Id", "Json")
VALUES(
4,
'{"RootName":"e4","Collection":[{"BranchName":"e4 c1","Nested":{"LeafName":"e4 c1 l"}},{"BranchName":"e4 c2","Nested":{"LeafName":"e4 c2 l"}}],"OptionalReference":{"BranchName":"e4 or","Nested":{"LeafName":"e4 or l"}}}')
""");
    }

    protected override async Task Seed33046(DbContext ctx)
        => await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO "Reviews" ("Rounds", "Id")
VALUES('[{"RoundNumber":11,"SubRounds":[{"SubRoundNumber":111},{"SubRoundNumber":112}]}]', 1)
""");

    protected override async Task Seed34960(Context34960 ctx)
    {
        await base.Seed34960(ctx);

        // JSON nulls
        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO "Entities" ("Collection", "Reference", "Id")
VALUES(
'null',
'null',
4)
""");

        // JSON object where collection should be
        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO "Junk" ("Collection", "Reference", "Id")
VALUES(
'{ "DoB":"2000-01-01T00:00:00","Text":"junk" }',
NULL,
1)
""");

        // JSON array where entity should be
        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO "Junk" ("Collection", "Reference", "Id")
VALUES(
NULL,
'[{ "DoB":"2000-01-01T00:00:00","Text":"junk" }]',
2)
""");
    }

    protected override Task SeedJunkInJson(DbContext ctx)
        => ctx.Database.ExecuteSqlAsync(
            $$$"""
INSERT INTO "Entities" ("Collection", "CollectionWithCtor", "Reference", "ReferenceWithCtor", "Id")
VALUES(
'[{"JunkReference":{"Something":"SomeValue" },"Name":"c11","JunkProperty1":50,"Number":11.5,"JunkCollection1":[],"JunkCollection2":[{"Foo":"junk value"}],"NestedCollection":[{"DoB":"2002-04-01T00:00:00","DummyProp":"Dummy value"},{"DoB":"2002-04-02T00:00:00","DummyReference":{"Foo":5}}],"NestedReference":{"DoB":"2002-03-01T00:00:00"}},{"Name":"c12","Number":12.5,"NestedCollection":[{"DoB":"2002-06-01T00:00:00"},{"DoB":"2002-06-02T00:00:00"}],"NestedDummy":59,"NestedReference":{"DoB":"2002-05-01T00:00:00"}}]',
'[{"MyBool":true,"Name":"c11 ctor","JunkReference":{"Something":"SomeValue","JunkCollection":[{"Foo":"junk value"}]},"NestedCollection":[{"DoB":"2002-08-01T00:00:00"},{"DoB":"2002-08-02T00:00:00"}],"NestedReference":{"DoB":"2002-07-01T00:00:00"}},{"MyBool":false,"Name":"c12 ctor","NestedCollection":[{"DoB":"2002-10-01T00:00:00"},{"DoB":"2002-10-02T00:00:00"}],"JunkCollection":[{"Foo":"junk value"}],"NestedReference":{"DoB":"2002-09-01T00:00:00"}}]',
'{"Name":"r1","JunkCollection":[{"Foo":"junk value"}],"JunkReference":{"Something":"SomeValue" },"Number":1.5,"NestedCollection":[{"DoB":"2000-02-01T00:00:00","JunkReference":{"Something":"SomeValue"}},{"DoB":"2000-02-02T00:00:00"}],"NestedReference":{"DoB":"2000-01-01T00:00:00"}}',
'{"MyBool":true,"JunkCollection":[{"Foo":"junk value"}],"Name":"r1 ctor","JunkReference":{"Something":"SomeValue" },"NestedCollection":[{"DoB":"2001-02-01T00:00:00"},{"DoB":"2001-02-02T00:00:00"}],"NestedReference":{"JunkCollection":[{"Foo":"junk value"}],"DoB":"2001-01-01T00:00:00"}}',
1)
""");

    protected override Task SeedTrickyBuffering(DbContext ctx)
        => ctx.Database.ExecuteSqlAsync(
            $$$"""
INSERT INTO "Entities" ("Reference", "Id")
VALUES(
'{"Name": "r1", "Number": 7, "JunkReference":{"Something": "SomeValue" }, "JunkCollection": [{"Foo": "junk value"}], "NestedReference": {"DoB": "2000-01-01T00:00:00"}, "NestedCollection": [{"DoB": "2000-02-01T00:00:00", "JunkReference": {"Something": "SomeValue"}}, {"DoB": "2000-02-02T00:00:00"}]}',1)
""");

    protected override Task SeedShadowProperties(DbContext ctx)
        => ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO "Entities" ("Collection", "CollectionWithCtor", "Reference", "ReferenceWithCtor", "Id", "Name")
VALUES(
'[{"Name":"e1_c1","ShadowDouble":5.5},{"ShadowDouble":20.5,"Name":"e1_c2"}]',
'[{"Name":"e1_c1 ctor","ShadowNullableByte":6},{"ShadowNullableByte":null,"Name":"e1_c2 ctor"}]',
'{"Name":"e1_r", "ShadowString":"Foo"}',
'{"ShadowInt":143,"Name":"e1_r ctor"}',
1,
'e1')
""");

    protected override async Task SeedNotICollection(DbContext ctx)
    {
        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO "Entities" ("Json", "Id")
VALUES(
'{"Collection":[{"Bar":11,"Foo":"c11"},{"Bar":12,"Foo":"c12"},{"Bar":13,"Foo":"c13"}]}',
1)
""");

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO "Entities" ("Json", "Id")
VALUES(
'{"Collection":[{"Bar":21,"Foo":"c21"},{"Bar":22,"Foo":"c22"}]}',
2)
""");
    }

    protected override async Task SeedBadJsonProperties(ContextBadJsonProperties ctx)
    {
        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO Entities (Id, Scenario, OptionalReference, RequiredReference, Collection)
VALUES(
1,
'baseline',
'{"NestedOptional": { "Text":"or no" }, "NestedRequired": { "Text":"or nr" }, "NestedCollection": [ { "Text":"or nc 1" }, { "Text":"or nc 2" } ] }',
'{"NestedOptional": { "Text":"rr no" }, "NestedRequired": { "Text":"rr nr" }, "NestedCollection": [ { "Text":"rr nc 1" }, { "Text":"rr nc 2" } ] }',
'[
{"NestedOptional": { "Text":"c 1 no" }, "NestedRequired": { "Text":"c 1 nr" }, "NestedCollection": [ { "Text":"c 1 nc 1" }, { "Text":"c 1 nc 2" } ] },
{"NestedOptional": { "Text":"c 2 no" }, "NestedRequired": { "Text":"c 2 nr" }, "NestedCollection": [ { "Text":"c 2 nc 1" }, { "Text":"c 2 nc 2" } ] }
]')
""");

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO Entities (Id, Scenario, OptionalReference, RequiredReference, Collection)
VALUES(
2,
'duplicated navigations',
'{"NestedOptional": { "Text":"or no" }, "NestedOptional": { "Text":"or no dupnav" }, "NestedRequired": { "Text":"or nr" }, "NestedCollection": [ { "Text":"or nc 1" }, { "Text":"or nc 2" } ], "NestedCollection": [ { "Text":"or nc 1 dupnav" }, { "Text":"or nc 2 dupnav" } ], "NestedRequired": { "Text":"or nr dupnav" } }',
'{"NestedOptional": { "Text":"rr no" }, "NestedOptional": { "Text":"rr no dupnav" }, "NestedRequired": { "Text":"rr nr" }, "NestedCollection": [ { "Text":"rr nc 1" }, { "Text":"rr nc 2" } ], "NestedCollection": [ { "Text":"rr nc 1 dupnav" }, { "Text":"rr nc 2 dupnav" } ], "NestedRequired": { "Text":"rr nr dupnav" } }',
'[
{"NestedOptional": { "Text":"c 1 no" }, "NestedOptional": { "Text":"c 1 no dupnav" }, "NestedRequired": { "Text":"c 1 nr" }, "NestedCollection": [ { "Text":"c 1 nc 1" }, { "Text":"c 1 nc 2" } ], "NestedCollection": [ { "Text":"c 1 nc 1 dupnav" }, { "Text":"c 1 nc 2 dupnav" } ], "NestedRequired": { "Text":"c 1 nr dupnav" } },
{"NestedOptional": { "Text":"c 2 no" }, "NestedOptional": { "Text":"c 2 no dupnav" }, "NestedRequired": { "Text":"c 2 nr" }, "NestedCollection": [ { "Text":"c 2 nc 1" }, { "Text":"c 2 nc 2" } ], "NestedCollection": [ { "Text":"c 2 nc 1 dupnav" }, { "Text":"c 2 nc 2 dupnav" } ], "NestedRequired": { "Text":"c 2 nr dupnav" } }
]')
""");

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO Entities (Id, Scenario, OptionalReference, RequiredReference, Collection)
VALUES(
3,
'duplicated scalars',
'{"NestedOptional": { "Text":"or no", "Text":"or no dupprop" }, "NestedRequired": { "Text":"or nr", "Text":"or nr dupprop" }, "NestedCollection": [ { "Text":"or nc 1", "Text":"or nc 1 dupprop" }, { "Text":"or nc 2", "Text":"or nc 2 dupprop" } ] }',
'{"NestedOptional": { "Text":"rr no", "Text":"rr no dupprop" }, "NestedRequired": { "Text":"rr nr", "Text":"rr nr dupprop" }, "NestedCollection": [ { "Text":"rr nc 1", "Text":"rr nc 1 dupprop" }, { "Text":"rr nc 2", "Text":"rr nc 2 dupprop" } ] }',
'[
{"NestedOptional": { "Text":"c 1 no", "Text":"c 1 no dupprop" }, "NestedRequired": { "Text":"c 1 nr", "Text":"c 1 nr dupprop" }, "NestedCollection": [ { "Text":"c 1 nc 1", "Text":"c 1 nc 1 dupprop" }, { "Text":"c 1 nc 2", "Text":"c 1 nc 2 dupprop" } ] },
{"NestedOptional": { "Text":"c 2 no", "Text":"c 2 no dupprop" }, "NestedRequired": { "Text":"c 2 nr", "Text":"c 2 nr dupprop" }, "NestedCollection": [ { "Text":"c 2 nc 1", "Text":"c 2 nc 1 dupprop" }, { "Text":"c 2 nc 2", "Text":"c 2 nc 2 dupprop" } ] }
]')
""");

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO Entities (Id, Scenario, OptionalReference, RequiredReference, Collection)
VALUES(
4,
'empty navigation property names',
'{"": { "Text":"or no" }, "": { "Text":"or nr" }, "": [ { "Text":"or nc 1" }, { "Text":"or nc 2" } ] }',
'{"": { "Text":"rr no" }, "": { "Text":"rr nr" }, "": [ { "Text":"rr nc 1" }, { "Text":"rr nc 2" } ] }',
'[
{"": { "Text":"c 1 no" }, "": { "Text":"c 1 nr" }, "": [ { "Text":"c 1 nc 1" }, { "Text":"c 1 nc 2" } ] },
{"": { "Text":"c 2 no" }, "": { "Text":"c 2 nr" }, "": [ { "Text":"c 2 nc 1" }, { "Text":"c 2 nc 2" } ] }
]')
""");

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO Entities (Id, Scenario, OptionalReference, RequiredReference, Collection)
VALUES(
5,
'empty scalar property names',
'{"NestedOptional": { "":"or no" }, "NestedRequired": { "":"or nr" }, "NestedCollection": [ { "":"or nc 1" }, { "":"or nc 2" } ] }',
'{"NestedOptional": { "":"rr no" }, "NestedRequired": { "":"rr nr" }, "NestedCollection": [ { "":"rr nc 1" }, { "":"rr nc 2" } ] }',
'[
{"NestedOptional": { "":"c 1 no" }, "NestedRequired": { "":"c 1 nr" }, "NestedCollection": [ { "":"c 1 nc 1" }, { "":"c 1 nc 2" } ] },
{"NestedOptional": { "":"c 2 no" }, "NestedRequired": { "":"c 2 nr" }, "NestedCollection": [ { "":"c 2 nc 1" }, { "":"c 2 nc 2" } ] }
]')
""");

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO Entities (Id, Scenario, OptionalReference, RequiredReference, Collection)
VALUES(
10,
'null navigation property names',
'{null: { "Text":"or no" }, null: { "Text":"or nr" }, null: [ { "Text":"or nc 1" }, { "Text":"or nc 2" } ] }',
'{null: { "Text":"rr no" }, null: { "Text":"rr nr" }, null: [ { "Text":"rr nc 1" }, { "Text":"rr nc 2" } ] }',
'[
{null: { "Text":"c 1 no" }, null: { "Text":"c 1 nr" }, null: [ { "Text":"c 1 nc 1" }, { "Text":"c 1 nc 2" } ] },
{null: { "Text":"c 2 no" }, null: { "Text":"c 2 nr" }, null: [ { "Text":"c 2 nc 1" }, { "Text":"c 2 nc 2" } ] }
]')
""");

        await ctx.Database.ExecuteSqlAsync(
            $$"""
INSERT INTO Entities (Id, Scenario, OptionalReference, RequiredReference, Collection)
VALUES(
11,
'null scalar property names',
'{"NestedOptional": { null:"or no", "Text":"or no nonnull" }, "NestedRequired": { null:"or nr", "Text":"or nr nonnull" }, "NestedCollection": [ { null:"or nc 1", "Text":"or nc 1 nonnull" }, { null:"or nc 2", "Text":"or nc 2 nonnull" } ] }',
'{"NestedOptional": { null:"rr no", "Text":"rr no nonnull" }, "NestedRequired": { null:"rr nr", "Text":"rr nr nonnull" }, "NestedCollection": [ { null:"rr nc 1", "Text":"rr nc 1 nonnull" }, { null:"rr nc 2", "Text":"rr nc 2 nonnull" } ] }',
'[
{"NestedOptional": { null:"c 1 no", "Text":"c 1 no nonnull" }, "NestedRequired": { null:"c 1 nr", "Text":"c 1 nr nonnull" }, "NestedCollection": [ { null:"c 1 nc 1", "Text":"c 1 nc 1 nonnull" }, { null:"c 1 nc 2", "Text":"c 1 nc 2 nonnull" } ] },
{"NestedOptional": { null:"c 2 no", "Text":"c 2 no nonnull" }, "NestedRequired": { null:"c 2 nr", "Text":"c 2 nr nonnull" }, "NestedCollection": [ { null:"c 2 nc 1", "Text":"c 2 nc 1 nonnull" }, { null:"c 2 nc 2", "Text":"c 2 nc 2 nonnull" } ] }
]')
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
