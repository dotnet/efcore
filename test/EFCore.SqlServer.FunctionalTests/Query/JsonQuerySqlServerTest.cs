﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

namespace Microsoft.EntityFrameworkCore.Query;

public class JsonQuerySqlServerTest : JsonQueryTestBase<JsonQuerySqlServerFixture>
{
    public JsonQuerySqlServerTest(JsonQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Basic_json_projection_owner_entity(bool async)
    {
        await base.Basic_json_projection_owner_entity(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Basic_json_projection_owner_entity_NoTracking(bool async)
    {
        await base.Basic_json_projection_owner_entity_NoTracking(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Basic_json_projection_owner_entity_duplicated(bool async)
    {
        await base.Basic_json_projection_owner_entity_duplicated(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Basic_json_projection_owner_entity_duplicated_NoTracking(bool async)
    {
        await base.Basic_json_projection_owner_entity_duplicated_NoTracking(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[Name], [j].[OwnedCollection], [j].[OwnedCollection]
FROM [JsonEntitiesSingleOwned] AS [j]
""");
    }

    public override async Task Basic_json_projection_owner_entity_twice(bool async)
    {
        await base.Basic_json_projection_owner_entity_twice(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Basic_json_projection_owner_entity_twice_NoTracking(bool async)
    {
        await base.Basic_json_projection_owner_entity_twice_NoTracking(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Basic_json_projection_owned_reference_root(bool async)
    {
        await base.Basic_json_projection_owned_reference_root(async);

        AssertSql(
            """
SELECT [j].[OwnedReferenceRoot], [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Basic_json_projection_owned_reference_duplicated2(bool async)
    {
        await base.Basic_json_projection_owned_reference_duplicated2(async);

        AssertSql(
            """
SELECT [j].[OwnedReferenceRoot], [j].[Id], JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedReferenceLeaf'), [j].[OwnedReferenceRoot], JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedReferenceLeaf')
FROM [JsonEntitiesBasic] AS [j]
ORDER BY [j].[Id]
""");
    }

    public override async Task Basic_json_projection_owned_reference_duplicated(bool async)
    {
        await base.Basic_json_projection_owned_reference_duplicated(async);

        AssertSql(
            """
SELECT [j].[OwnedReferenceRoot], [j].[Id], JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch'), [j].[OwnedReferenceRoot], JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch')
FROM [JsonEntitiesBasic] AS [j]
ORDER BY [j].[Id]
""");
    }

    public override async Task Basic_json_projection_owned_collection_root(bool async)
    {
        await base.Basic_json_projection_owned_collection_root(async);

        AssertSql(
            """
SELECT [j].[OwnedCollectionRoot], [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Basic_json_projection_owned_reference_branch(bool async)
    {
        await base.Basic_json_projection_owned_reference_branch(async);

        AssertSql(
            """
SELECT JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Basic_json_projection_owned_collection_branch(bool async)
    {
        await base.Basic_json_projection_owned_collection_branch(async);

        AssertSql(
            """
SELECT JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedCollectionBranch'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Basic_json_projection_owned_reference_leaf(bool async)
    {
        await base.Basic_json_projection_owned_reference_leaf(async);

        AssertSql(
            """
SELECT JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedReferenceLeaf'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Basic_json_projection_owned_collection_leaf(bool async)
    {
        await base.Basic_json_projection_owned_collection_leaf(async);

        AssertSql(
            """
SELECT JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedCollectionLeaf'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Basic_json_projection_scalar(bool async)
    {
        await base.Basic_json_projection_scalar(async);

        AssertSql(
            """
SELECT JSON_VALUE([j].[OwnedReferenceRoot], '$.Name')
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_scalar_length(bool async)
    {
        await base.Json_scalar_length(async);

        AssertSql(
            """
SELECT [j].[Name]
FROM [JsonEntitiesBasic] AS [j]
WHERE CAST(LEN(JSON_VALUE([j].[OwnedReferenceRoot], '$.Name')) AS int) > 2
""");
    }

    public override async Task Basic_json_projection_enum_inside_json_entity(bool async)
    {
        await base.Basic_json_projection_enum_inside_json_entity(async);

        AssertSql(
"""
SELECT [j].[Id], CAST(JSON_VALUE([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.Enum') AS int) AS [Enum]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_projection_enum_with_custom_conversion(bool async)
    {
        await base.Json_projection_enum_with_custom_conversion(async);

        AssertSql(
            """
SELECT [j].[Id], CAST(JSON_VALUE([j].[json_reference_custom_naming], '$.CustomEnum') AS int) AS [Enum]
FROM [JsonEntitiesCustomNaming] AS [j]
""");
    }

    public override async Task Json_projection_with_deduplication(bool async)
    {
        await base.Json_projection_with_deduplication(async);

        AssertSql(
"""
SELECT [j].[Id], JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch'), JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedReferenceLeaf'), JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedCollectionLeaf'), JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedCollectionBranch'), JSON_VALUE([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething')
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_projection_with_deduplication_reverse_order(bool async)
    {
        await base.Json_projection_with_deduplication_reverse_order(async);

        AssertSql(
            """
SELECT JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedReferenceLeaf'), [j].[Id], [j].[OwnedReferenceRoot], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_property_in_predicate(bool async)
    {
        await base.Json_property_in_predicate(async);

        AssertSql(
            """
SELECT [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
WHERE CAST(JSON_VALUE([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.Fraction') AS decimal(18,2)) < 20.5
""");
    }

    public override async Task Json_subquery_property_pushdown_length(bool async)
    {
        await base.Json_subquery_property_pushdown_length(async);

        AssertSql(
            """
@__p_0='3'

SELECT CAST(LEN([t0].[c]) AS int)
FROM (
    SELECT DISTINCT [t].[c]
    FROM (
        SELECT TOP(@__p_0) JSON_VALUE([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething') AS [c]
        FROM [JsonEntitiesBasic] AS [j]
        ORDER BY [j].[Id]
    ) AS [t]
) AS [t0]
""");
    }

    public override async Task Json_subquery_reference_pushdown_reference(bool async)
    {
        await base.Json_subquery_reference_pushdown_reference(async);

        AssertSql(
            """
@__p_0='10'

SELECT JSON_QUERY([t0].[c], '$.OwnedReferenceBranch'), [t0].[Id]
FROM (
    SELECT DISTINCT [t].[c] AS [c], [t].[Id]
    FROM (
        SELECT TOP(@__p_0) [j].[OwnedReferenceRoot] AS [c], [j].[Id]
        FROM [JsonEntitiesBasic] AS [j]
        ORDER BY [j].[Id]
    ) AS [t]
) AS [t0]
""");
    }

    public override async Task Json_subquery_reference_pushdown_reference_anonymous_projection(bool async)
    {
        await base.Json_subquery_reference_pushdown_reference_anonymous_projection(async);

        AssertSql(
            """
@__p_0='10'

SELECT JSON_QUERY([t0].[c], '$.OwnedReferenceSharedBranch'), [t0].[Id], CAST(LEN([t0].[c0]) AS int)
FROM (
    SELECT DISTINCT JSON_QUERY([t].[c],'$') AS [c], [t].[Id], [t].[c0]
    FROM (
        SELECT TOP(@__p_0) JSON_QUERY([j].[json_reference_shared], '$') AS [c], [j].[Id], CAST(JSON_VALUE([j].[json_reference_shared], '$.OwnedReferenceSharedBranch.OwnedReferenceSharedLeaf.SomethingSomething') AS nvarchar(max)) AS [c0]
        FROM [JsonEntitiesBasic] AS [j]
        ORDER BY [j].[Id]
    ) AS [t]
) AS [t0]
""");
    }

    public override async Task Json_subquery_reference_pushdown_reference_pushdown_anonymous_projection(bool async)
    {
        await base.Json_subquery_reference_pushdown_reference_pushdown_anonymous_projection(async);

        AssertSql(
            """
@__p_0='10'

SELECT JSON_QUERY([t2].[c],'$.OwnedReferenceSharedLeaf'), [t2].[Id], JSON_QUERY([t2].[c], '$.OwnedCollectionSharedLeaf'), [t2].[Length]
FROM (
    SELECT DISTINCT JSON_QUERY([t1].[c],'$') AS [c], [t1].[Id], [t1].[Length]
    FROM (
        SELECT TOP(@__p_0) JSON_QUERY([t0].[c], '$.OwnedReferenceSharedBranch') AS [c], [t0].[Id], CAST(LEN([t0].[Scalar]) AS int) AS [Length]
        FROM (
            SELECT DISTINCT JSON_QUERY([t].[c],'$') AS [c], [t].[Id], [t].[Scalar]
            FROM (
                SELECT TOP(@__p_0) JSON_QUERY([j].[json_reference_shared], '$') AS [c], [j].[Id], CAST(JSON_VALUE([j].[json_reference_shared], '$.OwnedReferenceSharedBranch.OwnedReferenceSharedLeaf.SomethingSomething') AS nvarchar(max)) AS [Scalar]
                FROM [JsonEntitiesBasic] AS [j]
                ORDER BY [j].[Id]
            ) AS [t]
        ) AS [t0]
        ORDER BY CAST(LEN([t0].[Scalar]) AS int)
    ) AS [t1]
) AS [t2]
""");
    }

    public override async Task Json_subquery_reference_pushdown_reference_pushdown_reference(bool async)
    {
        await base.Json_subquery_reference_pushdown_reference_pushdown_reference(async);

        AssertSql(
            """
@__p_0='10'

SELECT JSON_QUERY([t2].[c], '$.OwnedReferenceLeaf'), [t2].[Id]
FROM (
    SELECT DISTINCT [t1].[c] AS [c], [t1].[Id]
    FROM (
        SELECT TOP(@__p_0) JSON_QUERY([t0].[c], '$.OwnedReferenceBranch') AS [c], [t0].[Id]
        FROM (
            SELECT DISTINCT [t].[c] AS [c], [t].[Id], [t].[c] AS [c0]
            FROM (
                SELECT TOP(@__p_0) [j].[OwnedReferenceRoot] AS [c], [j].[Id]
                FROM [JsonEntitiesBasic] AS [j]
                ORDER BY [j].[Id]
            ) AS [t]
        ) AS [t0]
        ORDER BY JSON_VALUE([t0].[c0], '$.Name')
    ) AS [t1]
) AS [t2]
""");
    }

    public override async Task Json_subquery_reference_pushdown_reference_pushdown_collection(bool async)
    {
        await base.Json_subquery_reference_pushdown_reference_pushdown_collection(async);

        AssertSql(
            """
@__p_0='10'

SELECT JSON_QUERY([t2].[c], '$.OwnedCollectionLeaf'), [t2].[Id]
FROM (
    SELECT DISTINCT [t1].[c] AS [c], [t1].[Id]
    FROM (
        SELECT TOP(@__p_0) JSON_QUERY([t0].[c], '$.OwnedReferenceBranch') AS [c], [t0].[Id]
        FROM (
            SELECT DISTINCT [t].[c] AS [c], [t].[Id], [t].[c] AS [c0]
            FROM (
                SELECT TOP(@__p_0) [j].[OwnedReferenceRoot] AS [c], [j].[Id]
                FROM [JsonEntitiesBasic] AS [j]
                ORDER BY [j].[Id]
            ) AS [t]
        ) AS [t0]
        ORDER BY JSON_VALUE([t0].[c0], '$.Name')
    ) AS [t1]
) AS [t2]
""");
    }

    public override async Task Json_subquery_reference_pushdown_property(bool async)
    {
        await base.Json_subquery_reference_pushdown_property(async);

        AssertSql(
            """
@__p_0='10'

SELECT JSON_VALUE([t0].[c], '$.SomethingSomething')
FROM (
    SELECT DISTINCT [t].[c] AS [c], [t].[Id]
    FROM (
        SELECT TOP(@__p_0) JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedReferenceLeaf') AS [c], [j].[Id]
        FROM [JsonEntitiesBasic] AS [j]
        ORDER BY [j].[Id]
    ) AS [t]
) AS [t0]
""");
    }

    public override async Task Custom_naming_projection_owner_entity(bool async)
    {
        await base.Custom_naming_projection_owner_entity(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[Title], [j].[json_collection_custom_naming], [j].[json_reference_custom_naming]
FROM [JsonEntitiesCustomNaming] AS [j]
""");
    }

    public override async Task Custom_naming_projection_owned_reference(bool async)
    {
        await base.Custom_naming_projection_owned_reference(async);

        AssertSql(
            """
SELECT JSON_QUERY([j].[json_reference_custom_naming], '$.CustomOwnedReferenceBranch'), [j].[Id]
FROM [JsonEntitiesCustomNaming] AS [j]
""");
    }

    public override async Task Custom_naming_projection_owned_collection(bool async)
    {
        await base.Custom_naming_projection_owned_collection(async);

        AssertSql(
            """
SELECT [j].[json_collection_custom_naming], [j].[Id]
FROM [JsonEntitiesCustomNaming] AS [j]
ORDER BY [j].[Id]
""");
    }

    public override async Task Custom_naming_projection_owned_scalar(bool async)
    {
        await base.Custom_naming_projection_owned_scalar(async);

        AssertSql(
            """
SELECT CAST(JSON_VALUE([j].[json_reference_custom_naming], '$.CustomOwnedReferenceBranch.CustomFraction') AS float)
FROM [JsonEntitiesCustomNaming] AS [j]
""");
    }

    public override async Task Custom_naming_projection_everything(bool async)
    {
        await base.Custom_naming_projection_everything(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[Title], [j].[json_collection_custom_naming], [j].[json_reference_custom_naming], [j].[json_reference_custom_naming], JSON_QUERY([j].[json_reference_custom_naming], '$.CustomOwnedReferenceBranch'), [j].[json_collection_custom_naming], JSON_QUERY([j].[json_reference_custom_naming], '$.CustomOwnedCollectionBranch'), JSON_VALUE([j].[json_reference_custom_naming], '$.CustomName'), CAST(JSON_VALUE([j].[json_reference_custom_naming], '$.CustomOwnedReferenceBranch.CustomFraction') AS float)
FROM [JsonEntitiesCustomNaming] AS [j]
""");
    }

    public override async Task Project_entity_with_single_owned(bool async)
    {
        await base.Project_entity_with_single_owned(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[Name], [j].[OwnedCollection]
FROM [JsonEntitiesSingleOwned] AS [j]
""");
    }

    public override async Task Left_join_json_entities(bool async)
    {
        await base.Left_join_json_entities(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[Name], [j].[OwnedCollection], [j0].[Id], [j0].[EntityBasicId], [j0].[Name], [j0].[OwnedCollectionRoot], [j0].[OwnedReferenceRoot]
FROM [JsonEntitiesSingleOwned] AS [j]
LEFT JOIN [JsonEntitiesBasic] AS [j0] ON [j].[Id] = [j0].[Id]
""");
    }

    public override async Task Left_join_json_entities_complex_projection(bool async)
    {
        await base.Left_join_json_entities_complex_projection(async);

        AssertSql(
"""
SELECT [j].[Id], [j0].[Id], [j0].[OwnedReferenceRoot], JSON_QUERY([j0].[OwnedReferenceRoot], '$.OwnedReferenceBranch'), JSON_QUERY([j0].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedReferenceLeaf'), JSON_QUERY([j0].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedCollectionLeaf')
FROM [JsonEntitiesSingleOwned] AS [j]
LEFT JOIN [JsonEntitiesBasic] AS [j0] ON [j].[Id] = [j0].[Id]
""");
    }

    public override async Task Left_join_json_entities_json_being_inner(bool async)
    {
        await base.Left_join_json_entities_json_being_inner(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot], [j0].[Id], [j0].[Name], [j0].[OwnedCollection]
FROM [JsonEntitiesBasic] AS [j]
LEFT JOIN [JsonEntitiesSingleOwned] AS [j0] ON [j].[Id] = [j0].[Id]
""");
    }

    public override async Task Left_join_json_entities_complex_projection_json_being_inner(bool async)
    {
        await base.Left_join_json_entities_complex_projection_json_being_inner(async);

        AssertSql(
"""
SELECT [j].[Id], [j0].[Id], [j].[OwnedReferenceRoot], JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch'), JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedReferenceLeaf'), JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedCollectionLeaf'), [j0].[Name]
FROM [JsonEntitiesBasic] AS [j]
LEFT JOIN [JsonEntitiesSingleOwned] AS [j0] ON [j].[Id] = [j0].[Id]
""");
    }

    public override async Task Project_json_entity_FirstOrDefault_subquery(bool async)
    {
        await base.Project_json_entity_FirstOrDefault_subquery(async);

        AssertSql(
            """
SELECT [t].[c], [t].[Id]
FROM [JsonEntitiesBasic] AS [j]
OUTER APPLY (
    SELECT TOP(1) JSON_QUERY([j0].[OwnedReferenceRoot], '$.OwnedReferenceBranch') AS [c], [j0].[Id]
    FROM [JsonEntitiesBasic] AS [j0]
    ORDER BY [j0].[Id]
) AS [t]
ORDER BY [j].[Id]
""");
    }

    public override async Task Project_json_entity_FirstOrDefault_subquery_with_binding_on_top(bool async)
    {
        await base.Project_json_entity_FirstOrDefault_subquery_with_binding_on_top(async);

        AssertSql(
            """
SELECT (
    SELECT TOP(1) CAST(JSON_VALUE([j0].[OwnedReferenceRoot], '$.OwnedReferenceBranch.Date') AS datetime2)
    FROM [JsonEntitiesBasic] AS [j0]
    ORDER BY [j0].[Id])
FROM [JsonEntitiesBasic] AS [j]
ORDER BY [j].[Id]
""");
    }

    public override async Task Project_json_entity_FirstOrDefault_subquery_with_entity_comparison_on_top(bool async)
    {
        await base.Project_json_entity_FirstOrDefault_subquery_with_entity_comparison_on_top(async);

        AssertSql(
            @"");
    }

    public override async Task Project_json_entity_FirstOrDefault_subquery_deduplication(bool async)
    {
        await base.Project_json_entity_FirstOrDefault_subquery_deduplication(async);

        AssertSql(
"""
SELECT [t].[c], [t].[Id], [t].[c0], [t].[Id0], [t].[c1], [t].[c2], [t].[c3], [t].[c4]
FROM [JsonEntitiesBasic] AS [j]
OUTER APPLY (
    SELECT TOP(1) JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedCollectionBranch') AS [c], [j].[Id], [j0].[OwnedReferenceRoot] AS [c0], [j0].[Id] AS [Id0], JSON_QUERY([j0].[OwnedReferenceRoot], '$.OwnedReferenceBranch') AS [c1], JSON_VALUE([j0].[OwnedReferenceRoot], '$.Name') AS [c2], CAST(JSON_VALUE([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.Enum') AS int) AS [c3], 1 AS [c4]
    FROM [JsonEntitiesBasic] AS [j0]
    ORDER BY [j0].[Id]
) AS [t]
ORDER BY [j].[Id]
""");
    }

    public override async Task Project_json_entity_FirstOrDefault_subquery_deduplication_and_outer_reference(bool async)
    {
        await base.Project_json_entity_FirstOrDefault_subquery_deduplication_and_outer_reference(async);

        AssertSql(
"""
SELECT [t].[c], [t].[Id], [t].[c0], [t].[Id0], [t].[c1], [t].[c2], [t].[c3], [t].[c4]
FROM [JsonEntitiesBasic] AS [j]
OUTER APPLY (
    SELECT TOP(1) JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedCollectionBranch') AS [c], [j].[Id], [j0].[OwnedReferenceRoot] AS [c0], [j0].[Id] AS [Id0], JSON_QUERY([j0].[OwnedReferenceRoot], '$.OwnedReferenceBranch') AS [c1], JSON_VALUE([j0].[OwnedReferenceRoot], '$.Name') AS [c2], CAST(JSON_VALUE([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.Enum') AS int) AS [c3], 1 AS [c4]
    FROM [JsonEntitiesBasic] AS [j0]
    ORDER BY [j0].[Id]
) AS [t]
ORDER BY [j].[Id]
""");
    }

    public override async Task Project_json_entity_FirstOrDefault_subquery_deduplication_outer_reference_and_pruning(bool async)
    {
        await base.Project_json_entity_FirstOrDefault_subquery_deduplication_outer_reference_and_pruning(async);

        AssertSql(
            """
SELECT [t].[c], [t].[Id], [t].[c0]
FROM [JsonEntitiesBasic] AS [j]
OUTER APPLY (
    SELECT TOP(1) JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedCollectionBranch') AS [c], [j].[Id], 1 AS [c0]
    FROM [JsonEntitiesBasic] AS [j0]
    ORDER BY [j0].[Id]
) AS [t]
ORDER BY [j].[Id]
""");
    }

    public override async Task Json_entity_with_inheritance_basic_projection(bool async)
    {
        await base.Json_entity_with_inheritance_basic_projection(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[Discriminator], [j].[Name], [j].[Fraction], [j].[CollectionOnBase], [j].[ReferenceOnBase], [j].[CollectionOnDerived], [j].[ReferenceOnDerived]
FROM [JsonEntitiesInheritance] AS [j]
""");
    }

    public override async Task Json_entity_with_inheritance_project_derived(bool async)
    {
        await base.Json_entity_with_inheritance_project_derived(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[Discriminator], [j].[Name], [j].[Fraction], [j].[CollectionOnBase], [j].[ReferenceOnBase], [j].[CollectionOnDerived], [j].[ReferenceOnDerived]
FROM [JsonEntitiesInheritance] AS [j]
WHERE [j].[Discriminator] = N'JsonEntityInheritanceDerived'
""");
    }

    public override async Task Json_entity_with_inheritance_project_navigations(bool async)
    {
        await base.Json_entity_with_inheritance_project_navigations(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[ReferenceOnBase], [j].[CollectionOnBase]
FROM [JsonEntitiesInheritance] AS [j]
""");
    }

    public override async Task Json_entity_with_inheritance_project_navigations_on_derived(bool async)
    {
        await base.Json_entity_with_inheritance_project_navigations_on_derived(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[ReferenceOnBase], [j].[ReferenceOnDerived], [j].[CollectionOnBase], [j].[CollectionOnDerived]
FROM [JsonEntitiesInheritance] AS [j]
WHERE [j].[Discriminator] = N'JsonEntityInheritanceDerived'
""");
    }

    public override async Task Json_entity_backtracking(bool async)
    {
        await base.Json_entity_backtracking(async);

        AssertSql(
            @"");
    }

    public override async Task Json_collection_index_in_projection_basic(bool async)
    {
        await base.Json_collection_index_in_projection_basic(async);

        AssertSql(
            """
SELECT JSON_QUERY([j].[OwnedCollectionRoot], '$[1]'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_collection_ElementAt_in_projection(bool async)
    {
        await base.Json_collection_ElementAt_in_projection(async);

        AssertSql(
            """
SELECT JSON_QUERY([j].[OwnedCollectionRoot], '$[1]'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_collection_ElementAtOrDefault_in_projection(bool async)
    {
        await base.Json_collection_ElementAtOrDefault_in_projection(async);

        AssertSql(
            """
SELECT JSON_QUERY([j].[OwnedCollectionRoot], '$[1]'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_collection_index_in_projection_project_collection(bool async)
    {
        await base.Json_collection_index_in_projection_project_collection(async);

        AssertSql(
            """
SELECT JSON_QUERY([j].[OwnedCollectionRoot], '$[1].OwnedCollectionBranch'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_collection_ElementAt_project_collection(bool async)
    {
        await base.Json_collection_ElementAt_project_collection(async);

        AssertSql(
            """
SELECT JSON_QUERY([j].[OwnedCollectionRoot], '$[1].OwnedCollectionBranch'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_collection_ElementAtOrDefault_project_collection(bool async)
    {
        await base.Json_collection_ElementAtOrDefault_project_collection(async);

        AssertSql(
            """
SELECT JSON_QUERY([j].[OwnedCollectionRoot], '$[1].OwnedCollectionBranch'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_collection_index_in_projection_using_parameter(bool async)
    {
        await base.Json_collection_index_in_projection_using_parameter(async);

        AssertSql(
            """
@__prm_0='0'

SELECT JSON_QUERY([j].[OwnedCollectionRoot], '$[' + CAST(@__prm_0 AS nvarchar(max)) + ']'), [j].[Id], @__prm_0
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_collection_index_in_projection_using_column(bool async)
    {
        await base.Json_collection_index_in_projection_using_column(async);

        AssertSql(
            """
SELECT JSON_QUERY([j].[OwnedCollectionRoot], '$[' + CAST([j].[Id] AS nvarchar(max)) + ']'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_collection_index_in_projection_using_untranslatable_client_method(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Json_collection_index_in_projection_using_untranslatable_client_method(async))).Message;

        Assert.Contains(CoreStrings.QueryUnableToTranslateMethod(
            "Microsoft.EntityFrameworkCore.Query.JsonQueryTestBase<Microsoft.EntityFrameworkCore.Query.JsonQuerySqlServerFixture>",
            "MyMethod"),
            message);
    }

    public override async Task Json_collection_index_in_projection_using_untranslatable_client_method2(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Json_collection_index_in_projection_using_untranslatable_client_method2(async))).Message;

        Assert.Contains(CoreStrings.QueryUnableToTranslateMethod(
            "Microsoft.EntityFrameworkCore.Query.JsonQueryTestBase<Microsoft.EntityFrameworkCore.Query.JsonQuerySqlServerFixture>",
            "MyMethod"),
            message);
    }

    public override async Task Json_collection_index_outside_bounds(bool async)
    {
        await base.Json_collection_index_outside_bounds(async);

        AssertSql(
            """
SELECT JSON_QUERY([j].[OwnedCollectionRoot], '$[25]'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_collection_index_outside_bounds2(bool async)
    {
        await base.Json_collection_index_outside_bounds2(async);

        AssertSql(
            """
SELECT JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedCollectionLeaf[25]'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_collection_index_outside_bounds_with_property_access(bool async)
    {
        await base.Json_collection_index_outside_bounds_with_property_access(async);

        AssertSql(
            """
SELECT CAST(JSON_VALUE([j].[OwnedCollectionRoot], '$[25].Number') AS int)
FROM [JsonEntitiesBasic] AS [j]
ORDER BY [j].[Id]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_collection_index_in_projection_nested(bool async)
    {
        await base.Json_collection_index_in_projection_nested(async);

        AssertSql(
            """
@__prm_0='1'

SELECT JSON_QUERY([j].[OwnedCollectionRoot], '$[0].OwnedCollectionBranch[' + CAST(@__prm_0 AS nvarchar(max)) + ']'), [j].[Id], @__prm_0
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_collection_index_in_projection_nested_project_scalar(bool async)
    {
        await base.Json_collection_index_in_projection_nested_project_scalar(async);

        AssertSql(
            """
@__prm_0='1'

SELECT CAST(JSON_VALUE([j].[OwnedCollectionRoot], '$[0].OwnedCollectionBranch[' + CAST(@__prm_0 AS nvarchar(max)) + '].Date') AS datetime2)
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_collection_index_in_projection_nested_project_reference(bool async)
    {
        await base.Json_collection_index_in_projection_nested_project_reference(async);

        AssertSql(
            """
@__prm_0='1'

SELECT JSON_QUERY([j].[OwnedCollectionRoot], '$[0].OwnedCollectionBranch[' + CAST(@__prm_0 AS nvarchar(max)) + '].OwnedReferenceLeaf'), [j].[Id], @__prm_0
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_collection_index_in_projection_nested_project_collection(bool async)
    {
        await base.Json_collection_index_in_projection_nested_project_collection(async);

        AssertSql(
            """
@__prm_0='1'

SELECT JSON_QUERY([j].[OwnedCollectionRoot], '$[0].OwnedCollectionBranch[' + CAST(@__prm_0 AS nvarchar(max)) + '].OwnedCollectionLeaf'), [j].[Id], @__prm_0
FROM [JsonEntitiesBasic] AS [j]
ORDER BY [j].[Id]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_collection_index_in_projection_nested_project_collection_anonymous_projection(bool async)
    {
        await base.Json_collection_index_in_projection_nested_project_collection_anonymous_projection(async);

        AssertSql(
            """
@__prm_0='1'

SELECT [j].[Id], JSON_QUERY([j].[OwnedCollectionRoot], '$[0].OwnedCollectionBranch[' + CAST(@__prm_0 AS nvarchar(max)) + '].OwnedCollectionLeaf'), @__prm_0
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_collection_index_in_predicate_using_constant(bool async)
    {
        await base.Json_collection_index_in_predicate_using_constant(async);

        AssertSql(
            """
SELECT [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
WHERE JSON_VALUE([j].[OwnedCollectionRoot], '$[0].Name') <> N'Foo' OR JSON_VALUE([j].[OwnedCollectionRoot], '$[0].Name') IS NULL
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_collection_index_in_predicate_using_variable(bool async)
    {
        await base.Json_collection_index_in_predicate_using_variable(async);

        AssertSql(
            """
@__prm_0='1'

SELECT [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
WHERE JSON_VALUE([j].[OwnedCollectionRoot], '$[' + CAST(@__prm_0 AS nvarchar(max)) + '].Name') <> N'Foo' OR JSON_VALUE([j].[OwnedCollectionRoot], '$[' + CAST(@__prm_0 AS nvarchar(max)) + '].Name') IS NULL
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_collection_index_in_predicate_using_column(bool async)
    {
        await base.Json_collection_index_in_predicate_using_column(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE JSON_VALUE([j].[OwnedCollectionRoot], '$[' + CAST([j].[Id] AS nvarchar(max)) + '].Name') = N'e1_c2'
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_collection_index_in_predicate_using_complex_expression1(bool async)
    {
        await base.Json_collection_index_in_predicate_using_complex_expression1(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE JSON_VALUE([j].[OwnedCollectionRoot], '$[' + CAST(CASE
    WHEN [j].[Id] = 1 THEN 0
    ELSE 1
END AS nvarchar(max)) + '].Name') = N'e1_c1'
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_collection_index_in_predicate_using_complex_expression2(bool async)
    {
        await base.Json_collection_index_in_predicate_using_complex_expression2(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE JSON_VALUE([j].[OwnedCollectionRoot], '$[' + CAST((
    SELECT MAX([j0].[Id])
    FROM [JsonEntitiesBasic] AS [j0]) AS nvarchar(max)) + '].Name') = N'e1_c2'
""");
    }

    public override async Task Json_collection_ElementAt_in_predicate(bool async)
    {
        await base.Json_collection_ElementAt_in_predicate(async);

        AssertSql(
            """
SELECT [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
WHERE JSON_VALUE([j].[OwnedCollectionRoot], '$[1].Name') <> N'Foo' OR JSON_VALUE([j].[OwnedCollectionRoot], '$[1].Name') IS NULL
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_collection_index_in_predicate_nested_mix(bool async)
    {
        await base.Json_collection_index_in_predicate_nested_mix(async);

        AssertSql(
            """
@__prm_0='0'

SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE JSON_VALUE([j].[OwnedCollectionRoot], '$[1].OwnedCollectionBranch[' + CAST(@__prm_0 AS nvarchar(max)) + '].OwnedCollectionLeaf[' + CAST([j].[Id] - 1 AS nvarchar(max)) + '].SomethingSomething') = N'e1_c2_c1_c1'
""");
    }

    public override async Task Json_collection_ElementAt_and_pushdown(bool async)
    {
        await base.Json_collection_ElementAt_and_pushdown(async);

        AssertSql(
            """
SELECT [j].[Id], CAST(JSON_VALUE([j].[OwnedCollectionRoot], '$[0].Number') AS int) AS [CollectionElement]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_collection_Any_with_predicate(bool async)
    {
        await base.Json_collection_Any_with_predicate(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON([j].[OwnedReferenceRoot], '$.OwnedCollectionBranch') WITH (
        [Date] datetime2 '$.Date',
        [Enum] int '$.Enum',
        [Enums] nvarchar(max) '$.Enums',
        [Fraction] decimal(18,2) '$.Fraction',
        [NullableEnum] int '$.NullableEnum',
        [NullableEnums] nvarchar(max) '$.NullableEnums',
        [OwnedCollectionLeaf] nvarchar(max) '$.OwnedCollectionLeaf' AS JSON,
        [OwnedReferenceLeaf] nvarchar(max) '$.OwnedReferenceLeaf' AS JSON
    ) AS [o]
    WHERE JSON_VALUE([o].[OwnedReferenceLeaf], '$.SomethingSomething') = N'e1_c2_c1_c1')
""");
    }

    public override async Task Json_collection_Where_ElementAt(bool async)
    {
        await base.Json_collection_Where_ElementAt(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE (
    SELECT JSON_VALUE([o].[value], '$.OwnedReferenceLeaf.SomethingSomething')
    FROM OPENJSON([j].[OwnedReferenceRoot], '$.OwnedCollectionBranch') AS [o]
    WHERE CAST(JSON_VALUE([o].[value], '$.Enum') AS int) = 2
    ORDER BY CAST([o].[key] AS int)
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) = N'e1_r_c2_r'
""");
    }

    public override async Task Json_collection_Skip(bool async)
    {
        await base.Json_collection_Skip(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE (
    SELECT [t].[c]
    FROM (
        SELECT JSON_VALUE([o].[value], '$.OwnedReferenceLeaf.SomethingSomething') AS [c], [o].[key], CAST([o].[key] AS int) AS [c0]
        FROM OPENJSON([j].[OwnedReferenceRoot], '$.OwnedCollectionBranch') AS [o]
        ORDER BY CAST([o].[key] AS int)
        OFFSET 1 ROWS
    ) AS [t]
    ORDER BY [t].[c0]
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) = N'e1_r_c2_r'
""");
    }

    public override async Task Json_collection_OrderByDescending_Skip_ElementAt(bool async)
    {
        await base.Json_collection_OrderByDescending_Skip_ElementAt(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE (
    SELECT [t].[c]
    FROM (
        SELECT JSON_VALUE([o].[value], '$.OwnedReferenceLeaf.SomethingSomething') AS [c], [o].[key], CAST(JSON_VALUE([o].[value], '$.Date') AS datetime2) AS [c0]
        FROM OPENJSON([j].[OwnedReferenceRoot], '$.OwnedCollectionBranch') AS [o]
        ORDER BY CAST(JSON_VALUE([o].[value], '$.Date') AS datetime2) DESC
        OFFSET 1 ROWS
    ) AS [t]
    ORDER BY [t].[c0] DESC
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) = N'e1_r_c1_r'
""");
    }

    public override async Task Json_collection_Distinct_Count_with_predicate(bool async)
    {
        await base.Json_collection_Distinct_Count_with_predicate(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [j].[Id], [o].[Date], [o].[Enum], [o].[Enums], [o].[Fraction], [o].[NullableEnum], [o].[NullableEnums], [o].[OwnedCollectionLeaf] AS [c], [o].[OwnedReferenceLeaf] AS [c0]
        FROM OPENJSON([j].[OwnedReferenceRoot], '$.OwnedCollectionBranch') WITH (
            [Date] datetime2 '$.Date',
            [Enum] int '$.Enum',
            [Enums] nvarchar(max) '$.Enums',
            [Fraction] decimal(18,2) '$.Fraction',
            [NullableEnum] int '$.NullableEnum',
            [NullableEnums] nvarchar(max) '$.NullableEnums',
            [OwnedCollectionLeaf] nvarchar(max) '$.OwnedCollectionLeaf' AS JSON,
            [OwnedReferenceLeaf] nvarchar(max) '$.OwnedReferenceLeaf' AS JSON
        ) AS [o]
        WHERE JSON_VALUE([o].[OwnedReferenceLeaf], '$.SomethingSomething') = N'e1_r_c2_r'
    ) AS [t]) = 1
""");
    }

    public override async Task Json_collection_within_collection_Count(bool async)
    {
        await base.Json_collection_within_collection_Count(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON([j].[OwnedCollectionRoot], '$') WITH (
        [Name] nvarchar(max) '$.Name',
        [Names] nvarchar(max) '$.Names',
        [Number] int '$.Number',
        [Numbers] nvarchar(max) '$.Numbers',
        [OwnedCollectionBranch] nvarchar(max) '$.OwnedCollectionBranch' AS JSON,
        [OwnedReferenceBranch] nvarchar(max) '$.OwnedReferenceBranch' AS JSON
    ) AS [o]
    WHERE (
        SELECT COUNT(*)
        FROM OPENJSON([o].[OwnedCollectionBranch], '$') WITH (
            [Date] datetime2 '$.Date',
            [Enum] int '$.Enum',
            [Enums] nvarchar(max) '$.Enums',
            [Fraction] decimal(18,2) '$.Fraction',
            [NullableEnum] int '$.NullableEnum',
            [NullableEnums] nvarchar(max) '$.NullableEnums',
            [OwnedCollectionLeaf] nvarchar(max) '$.OwnedCollectionLeaf' AS JSON,
            [OwnedReferenceLeaf] nvarchar(max) '$.OwnedReferenceLeaf' AS JSON
        ) AS [o0]) = 2)
""");
    }

    public override async Task Json_collection_in_projection_with_composition_count(bool async)
    {
        await base.Json_collection_in_projection_with_composition_count(async);

        AssertSql(
"""
SELECT (
    SELECT COUNT(*)
    FROM OPENJSON([j].[OwnedCollectionRoot], '$') WITH (
        [Name] nvarchar(max) '$.Name',
        [Names] nvarchar(max) '$.Names',
        [Number] int '$.Number',
        [Numbers] nvarchar(max) '$.Numbers',
        [OwnedCollectionBranch] nvarchar(max) '$.OwnedCollectionBranch' AS JSON,
        [OwnedReferenceBranch] nvarchar(max) '$.OwnedReferenceBranch' AS JSON
    ) AS [o])
FROM [JsonEntitiesBasic] AS [j]
ORDER BY [j].[Id]
""");
    }

    public override async Task Json_collection_in_projection_with_anonymous_projection_of_scalars(bool async)
    {
        await base.Json_collection_in_projection_with_anonymous_projection_of_scalars(async);

        AssertSql(
"""
SELECT [j].[Id], JSON_VALUE([o].[value], '$.Name'), CAST(JSON_VALUE([o].[value], '$.Number') AS int), [o].[key]
FROM [JsonEntitiesBasic] AS [j]
OUTER APPLY OPENJSON([j].[OwnedCollectionRoot], '$') AS [o]
ORDER BY [j].[Id], CAST([o].[key] AS int)
""");
    }

    public override async Task Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_scalars(bool async)
    {
        await base.Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_scalars(async);

        AssertSql(
"""
SELECT [j].[Id], [t].[Name], [t].[Number], [t].[key]
FROM [JsonEntitiesBasic] AS [j]
OUTER APPLY (
    SELECT JSON_VALUE([o].[value], '$.Name') AS [Name], CAST(JSON_VALUE([o].[value], '$.Number') AS int) AS [Number], [o].[key], CAST([o].[key] AS int) AS [c]
    FROM OPENJSON([j].[OwnedCollectionRoot], '$') AS [o]
    WHERE JSON_VALUE([o].[value], '$.Name') = N'Foo'
) AS [t]
ORDER BY [j].[Id], [t].[c]
""");
    }

    public override async Task Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_primitive_arrays(bool async)
    {
        await base.Json_collection_in_projection_with_composition_where_and_anonymous_projection_of_primitive_arrays(async);

        AssertSql(
"""
SELECT [j].[Id], [t].[Names], [t].[Numbers], [t].[key]
FROM [JsonEntitiesBasic] AS [j]
OUTER APPLY (
    SELECT JSON_QUERY([o].[value], '$.Names') AS [Names], JSON_QUERY([o].[value], '$.Numbers') AS [Numbers], [o].[key], CAST([o].[key] AS int) AS [c]
    FROM OPENJSON([j].[OwnedCollectionRoot], '$') AS [o]
    WHERE JSON_VALUE([o].[value], '$.Name') = N'Foo'
) AS [t]
ORDER BY [j].[Id], [t].[c]
""");
    }

    public override async Task Json_collection_filter_in_projection(bool async)
    {
        await base.Json_collection_filter_in_projection(async);

        AssertSql("");
    }

    public override async Task Json_collection_skip_take_in_projection(bool async)
    {
        await base.Json_collection_skip_take_in_projection(async);

        AssertSql("");
    }

    public override async Task Json_collection_skip_take_in_projection_project_into_anonymous_type(bool async)
    {
        await base.Json_collection_skip_take_in_projection_project_into_anonymous_type(async);

        AssertSql(
"""
SELECT [j].[Id], [t].[c], [t].[c0], [t].[c1], [t].[c2], [t].[c3], [t].[Id], [t].[c4], [t].[key]
FROM [JsonEntitiesBasic] AS [j]
OUTER APPLY (
    SELECT JSON_VALUE([o].[value], '$.Name') AS [c], JSON_QUERY([o].[value], '$.Names') AS [c0], CAST(JSON_VALUE([o].[value], '$.Number') AS int) AS [c1], JSON_QUERY([o].[value], '$.Numbers') AS [c2], JSON_QUERY([o].[value], '$.OwnedCollectionBranch') AS [c3], [j].[Id], JSON_QUERY([o].[value], '$.OwnedReferenceBranch') AS [c4], [o].[key]
    FROM OPENJSON([j].[OwnedCollectionRoot], '$') AS [o]
    ORDER BY JSON_VALUE([o].[value], '$.Name')
    OFFSET 1 ROWS FETCH NEXT 5 ROWS ONLY
) AS [t]
ORDER BY [j].[Id], [t].[c]
""");
    }

    public override async Task Json_collection_skip_take_in_projection_with_json_reference_access_as_final_operation(bool async)
    {
        await base.Json_collection_skip_take_in_projection_with_json_reference_access_as_final_operation(async);

        AssertSql(
"""
SELECT [j].[Id], [t].[c], [t].[Id], [t].[key]
FROM [JsonEntitiesBasic] AS [j]
OUTER APPLY (
    SELECT JSON_QUERY([o].[value], '$.OwnedReferenceBranch') AS [c], [j].[Id], [o].[key], JSON_VALUE([o].[value], '$.Name') AS [c0]
    FROM OPENJSON([j].[OwnedCollectionRoot], '$') AS [o]
    ORDER BY JSON_VALUE([o].[value], '$.Name')
    OFFSET 1 ROWS FETCH NEXT 5 ROWS ONLY
) AS [t]
ORDER BY [j].[Id], [t].[c0]
""");
    }

    public override async Task Json_collection_distinct_in_projection(bool async)
    {
        await base.Json_collection_distinct_in_projection(async);

        AssertSql("");
    }

    public override async Task Json_collection_leaf_filter_in_projection(bool async)
    {
        await base.Json_collection_leaf_filter_in_projection(async);

        AssertSql("");
    }

    public override async Task Json_collection_SelectMany(bool async)
    {
        await base.Json_collection_SelectMany(async);

        AssertSql("");
    }

    public override async Task Json_nested_collection_SelectMany(bool async)
    {
        await base.Json_nested_collection_SelectMany(async);

        AssertSql("");
    }

    public override async Task Json_collection_of_primitives_SelectMany(bool async)
    {
        await base.Json_collection_of_primitives_SelectMany(async);

        AssertSql("");
    }

    public override async Task Json_collection_of_primitives_index_used_in_predicate(bool async)
    {
        await base.Json_collection_of_primitives_index_used_in_predicate(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE JSON_VALUE([j].[OwnedReferenceRoot], '$.Names[0]') = N'e1_r1'
""");
    }

    public override async Task Json_collection_of_primitives_index_used_in_projection(bool async)
    {
        await base.Json_collection_of_primitives_index_used_in_projection(async);

        AssertSql(
"""
SELECT CAST(JSON_VALUE([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.Enums[0]') AS int)
FROM [JsonEntitiesBasic] AS [j]
ORDER BY [j].[Id]
""");
    }

    public override async Task Json_collection_of_primitives_index_used_in_orderby(bool async)
    {
        await base.Json_collection_of_primitives_index_used_in_orderby(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
ORDER BY CAST(JSON_VALUE([j].[OwnedReferenceRoot], '$.Numbers[0]') AS int)
""");
    }

    public override async Task Json_collection_of_primitives_contains_in_predicate(bool async)
    {
        await base.Json_collection_of_primitives_contains_in_predicate(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
WHERE N'e1_r1' IN (
    SELECT [n].[value]
    FROM OPENJSON(JSON_QUERY([j].[OwnedReferenceRoot], '$.Names')) WITH ([value] nvarchar(max) '$') AS [n]
)
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_collection_index_with_parameter_Select_ElementAt(bool async)
    {
        await base.Json_collection_index_with_parameter_Select_ElementAt(async);

        AssertSql(
"""
@__prm_0='0'

SELECT [j].[Id], (
    SELECT N'Foo'
    FROM OPENJSON([j].[OwnedCollectionRoot], '$[' + CAST(@__prm_0 AS nvarchar(max)) + '].OwnedCollectionBranch') AS [o]
    ORDER BY CAST([o].[key] AS int)
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) AS [CollectionElement]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_collection_index_with_expression_Select_ElementAt(bool async)
    {
        await base.Json_collection_index_with_expression_Select_ElementAt(async);

        AssertSql(
"""
@__prm_0='0'

SELECT JSON_VALUE([j].[OwnedCollectionRoot], '$[' + CAST(@__prm_0 + [j].[Id] AS nvarchar(max)) + '].OwnedCollectionBranch[0].OwnedReferenceLeaf.SomethingSomething')
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_collection_Select_entity_collection_ElementAt(bool async)
    {
        await base.Json_collection_Select_entity_collection_ElementAt(async);

        AssertSql(
"""
SELECT JSON_QUERY([j].[OwnedCollectionRoot], '$[0].OwnedCollectionBranch'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_collection_Select_entity_ElementAt(bool async)
    {
        await base.Json_collection_Select_entity_ElementAt(async);

        AssertSql(
"""
SELECT JSON_QUERY([j].[OwnedCollectionRoot], '$[0].OwnedReferenceBranch'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_collection_Select_entity_in_anonymous_object_ElementAt(bool async)
    {
        await base.Json_collection_Select_entity_in_anonymous_object_ElementAt(async);

        AssertSql(
"""
SELECT [t].[c], [t].[Id], [t].[c0]
FROM [JsonEntitiesBasic] AS [j]
OUTER APPLY (
    SELECT JSON_QUERY([o].[value], '$.OwnedReferenceBranch') AS [c], [j].[Id], 1 AS [c0]
    FROM OPENJSON([j].[OwnedCollectionRoot], '$') AS [o]
    ORDER BY CAST([o].[key] AS int)
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY
) AS [t]
ORDER BY [j].[Id]
""");
    }

    public override async Task Json_collection_Select_entity_with_initializer_ElementAt(bool async)
    {
        await base.Json_collection_Select_entity_with_initializer_ElementAt(async);

        AssertSql(
"""
SELECT [t].[Id], [t].[c]
FROM [JsonEntitiesBasic] AS [j]
OUTER APPLY (
    SELECT [j].[Id], 1 AS [c]
    FROM OPENJSON([j].[OwnedCollectionRoot], '$') AS [o]
    ORDER BY CAST([o].[key] AS int)
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY
) AS [t]
""");
    }

    public override async Task Json_projection_deduplication_with_collection_indexer_in_original(bool async)
    {
        await base.Json_projection_deduplication_with_collection_indexer_in_original(async);

        AssertSql(
            """
SELECT [j].[Id], JSON_QUERY([j].[OwnedCollectionRoot], '$[0].OwnedReferenceBranch'), JSON_QUERY([j].[OwnedCollectionRoot], '$[0]'), JSON_QUERY([j].[OwnedCollectionRoot], '$[0].OwnedReferenceBranch.OwnedCollectionLeaf')
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_projection_deduplication_with_collection_indexer_in_target(bool async)
    {
        await base.Json_projection_deduplication_with_collection_indexer_in_target(async);

        AssertSql(
            """
@__prm_0='1'

SELECT [j].[Id], JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedCollectionBranch[1]'), [j].[OwnedReferenceRoot], JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedCollectionLeaf[' + CAST(@__prm_0 AS nvarchar(max)) + ']'), @__prm_0
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_projection_deduplication_with_collection_in_original_and_collection_indexer_in_target(bool async)
    {
        await base.Json_projection_deduplication_with_collection_in_original_and_collection_indexer_in_target(async);

        AssertSql(
            """
@__prm_0='1'

SELECT JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedCollectionBranch[0].OwnedCollectionLeaf[' + CAST(@__prm_0 AS nvarchar(max)) + ']'), [j].[Id], @__prm_0, JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedCollectionBranch[' + CAST(@__prm_0 AS nvarchar(max)) + ']'), JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedCollectionBranch'), JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedCollectionBranch[0]')
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_collection_index_in_projection_using_constant_when_owner_is_present(bool async)
    {
        await base.Json_collection_index_in_projection_using_constant_when_owner_is_present(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot], JSON_QUERY([j].[OwnedCollectionRoot], '$[1]')
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_collection_index_in_projection_using_constant_when_owner_is_not_present(bool async)
    {
        await base.Json_collection_index_in_projection_using_constant_when_owner_is_not_present(async);

        AssertSql(
"""
SELECT [j].[Id], JSON_QUERY([j].[OwnedCollectionRoot], '$[1]')
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_collection_index_in_projection_using_parameter_when_owner_is_present(bool async)
    {
        await base.Json_collection_index_in_projection_using_parameter_when_owner_is_present(async);

        AssertSql(
            """
@__prm_0='1'

SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot], JSON_QUERY([j].[OwnedCollectionRoot], '$[' + CAST(@__prm_0 AS nvarchar(max)) + ']'), @__prm_0
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_collection_index_in_projection_using_parameter_when_owner_is_not_present(bool async)
    {
        await base.Json_collection_index_in_projection_using_parameter_when_owner_is_not_present(async);

        AssertSql(
"""
@__prm_0='1'

SELECT [j].[Id], JSON_QUERY([j].[OwnedCollectionRoot], '$[' + CAST(@__prm_0 AS nvarchar(max)) + ']'), @__prm_0
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_collection_after_collection_index_in_projection_using_constant_when_owner_is_present(bool async)
    {
        await base.Json_collection_after_collection_index_in_projection_using_constant_when_owner_is_present(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot], JSON_QUERY([j].[OwnedCollectionRoot], '$[1].OwnedCollectionBranch')
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_collection_after_collection_index_in_projection_using_constant_when_owner_is_not_present(bool async)
    {
        await base.Json_collection_after_collection_index_in_projection_using_constant_when_owner_is_not_present(async);

        AssertSql(
"""
SELECT [j].[Id], JSON_QUERY([j].[OwnedCollectionRoot], '$[1].OwnedCollectionBranch')
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_collection_after_collection_index_in_projection_using_parameter_when_owner_is_present(bool async)
    {
        await base.Json_collection_after_collection_index_in_projection_using_parameter_when_owner_is_present(async);

        AssertSql(
"""
@__prm_0='1'

SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot], JSON_QUERY([j].[OwnedCollectionRoot], '$[' + CAST(@__prm_0 AS nvarchar(max)) + '].OwnedCollectionBranch'), @__prm_0
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_collection_after_collection_index_in_projection_using_parameter_when_owner_is_not_present(bool async)
    {
        await base.Json_collection_after_collection_index_in_projection_using_parameter_when_owner_is_not_present(async);

        AssertSql(
"""
@__prm_0='1'

SELECT [j].[Id], JSON_QUERY([j].[OwnedCollectionRoot], '$[' + CAST(@__prm_0 AS nvarchar(max)) + '].OwnedCollectionBranch'), @__prm_0
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_collection_index_in_projection_when_owner_is_present_misc1(bool async)
    {
        await base.Json_collection_index_in_projection_when_owner_is_present_misc1(async);

        AssertSql(
            """
@__prm_0='1'

SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot], JSON_QUERY([j].[OwnedCollectionRoot], '$[1].OwnedCollectionBranch[' + CAST(@__prm_0 AS nvarchar(max)) + ']'), @__prm_0
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_collection_index_in_projection_when_owner_is_not_present_misc1(bool async)
    {
        await base.Json_collection_index_in_projection_when_owner_is_not_present_misc1(async);

        AssertSql(
"""
@__prm_0='1'

SELECT [j].[Id], JSON_QUERY([j].[OwnedCollectionRoot], '$[1].OwnedCollectionBranch[' + CAST(@__prm_0 AS nvarchar(max)) + ']'), @__prm_0
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_collection_index_in_projection_when_owner_is_present_misc2(bool async)
    {
        await base.Json_collection_index_in_projection_when_owner_is_present_misc2(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot], JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedCollectionLeaf[1]')
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_collection_index_in_projection_when_owner_is_not_present_misc2(bool async)
    {
        await base.Json_collection_index_in_projection_when_owner_is_not_present_misc2(async);

        AssertSql(
"""
SELECT [j].[Id], JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedCollectionLeaf[1]')
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_collection_index_in_projection_when_owner_is_present_multiple(bool async)
    {
        await base.Json_collection_index_in_projection_when_owner_is_present_multiple(async);

        AssertSql(
            """
@__prm_0='1'

SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot], JSON_QUERY([j].[OwnedCollectionRoot], '$[' + CAST(@__prm_0 AS nvarchar(max)) + '].OwnedCollectionBranch[1]'), @__prm_0, JSON_QUERY([j].[OwnedCollectionRoot], '$[1].OwnedCollectionBranch[1].OwnedReferenceLeaf'), JSON_QUERY([j].[OwnedCollectionRoot], '$[1].OwnedReferenceBranch'), JSON_QUERY([j].[OwnedCollectionRoot], '$[' + CAST(@__prm_0 AS nvarchar(max)) + '].OwnedReferenceBranch'), JSON_QUERY([j].[OwnedCollectionRoot], '$[' + CAST(@__prm_0 AS nvarchar(max)) + '].OwnedCollectionBranch[' + CAST([j].[Id] AS nvarchar(max)) + ']'), JSON_QUERY([j].[OwnedCollectionRoot], '$[' + CAST([j].[Id] AS nvarchar(max)) + '].OwnedCollectionBranch[1].OwnedReferenceLeaf'), JSON_QUERY([j].[OwnedCollectionRoot], '$[1].OwnedReferenceBranch'), JSON_QUERY([j].[OwnedCollectionRoot], '$[' + CAST([j].[Id] AS nvarchar(max)) + '].OwnedReferenceBranch'), JSON_QUERY([j].[OwnedCollectionRoot], '$[' + CAST([j].[Id] AS nvarchar(max)) + '].OwnedCollectionBranch[' + CAST([j].[Id] AS nvarchar(max)) + ']')
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Json_collection_index_in_projection_when_owner_is_not_present_multiple(bool async)
    {
        await base.Json_collection_index_in_projection_when_owner_is_not_present_multiple(async);

        AssertSql(
"""
@__prm_0='1'

SELECT [j].[Id], JSON_QUERY([j].[OwnedCollectionRoot], '$[' + CAST(@__prm_0 AS nvarchar(max)) + '].OwnedCollectionBranch[1]'), @__prm_0, JSON_QUERY([j].[OwnedCollectionRoot], '$[1].OwnedCollectionBranch[1].OwnedReferenceLeaf'), JSON_QUERY([j].[OwnedCollectionRoot], '$[1].OwnedReferenceBranch'), JSON_QUERY([j].[OwnedCollectionRoot], '$[' + CAST(@__prm_0 AS nvarchar(max)) + '].OwnedReferenceBranch'), JSON_QUERY([j].[OwnedCollectionRoot], '$[' + CAST(@__prm_0 AS nvarchar(max)) + '].OwnedCollectionBranch[' + CAST([j].[Id] AS nvarchar(max)) + ']'), JSON_QUERY([j].[OwnedCollectionRoot], '$[' + CAST([j].[Id] AS nvarchar(max)) + '].OwnedCollectionBranch[1].OwnedReferenceLeaf'), JSON_QUERY([j].[OwnedCollectionRoot], '$[1].OwnedReferenceBranch'), JSON_QUERY([j].[OwnedCollectionRoot], '$[' + CAST([j].[Id] AS nvarchar(max)) + '].OwnedReferenceBranch'), JSON_QUERY([j].[OwnedCollectionRoot], '$[' + CAST([j].[Id] AS nvarchar(max)) + '].OwnedCollectionBranch[' + CAST([j].[Id] AS nvarchar(max)) + ']')
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_scalar_required_null_semantics(bool async)
    {
        await base.Json_scalar_required_null_semantics(async);

        AssertSql(
            """
SELECT [j].[Name]
FROM [JsonEntitiesBasic] AS [j]
WHERE CAST(JSON_VALUE([j].[OwnedReferenceRoot], '$.Number') AS int) <> CAST(LEN(JSON_VALUE([j].[OwnedReferenceRoot], '$.Name')) AS int) OR JSON_VALUE([j].[OwnedReferenceRoot], '$.Name') IS NULL
""");
    }

    public override async Task Json_scalar_optional_null_semantics(bool async)
    {
        await base.Json_scalar_optional_null_semantics(async);

        AssertSql(
            """
SELECT [j].[Name]
FROM [JsonEntitiesBasic] AS [j]
WHERE JSON_VALUE([j].[OwnedReferenceRoot], '$.Name') = JSON_VALUE([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething') OR (JSON_VALUE([j].[OwnedReferenceRoot], '$.Name') IS NULL AND JSON_VALUE([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething') IS NULL)
""");
    }

    public override async Task Group_by_on_json_scalar(bool async)
    {
        await base.Group_by_on_json_scalar(async);

        AssertSql(
            """
SELECT [t].[Key], COUNT(*) AS [Count]
FROM (
    SELECT JSON_VALUE([j].[OwnedReferenceRoot], '$.Name') AS [Key]
    FROM [JsonEntitiesBasic] AS [j]
) AS [t]
GROUP BY [t].[Key]
""");
    }

    public override async Task Group_by_on_json_scalar_using_collection_indexer(bool async)
    {
        await base.Group_by_on_json_scalar_using_collection_indexer(async);

        AssertSql(
"""
SELECT [t].[Key], COUNT(*) AS [Count]
FROM (
    SELECT JSON_VALUE([j].[OwnedCollectionRoot], '$[0].Name') AS [Key]
    FROM [JsonEntitiesBasic] AS [j]
) AS [t]
GROUP BY [t].[Key]
""");
    }

    public override async Task Group_by_First_on_json_scalar(bool async)
    {
        await base.Group_by_First_on_json_scalar(async);

        AssertSql(
            """
SELECT [t1].[Id], [t1].[EntityBasicId], [t1].[Name], [t1].[c], [t1].[c0]
FROM (
    SELECT [t].[Key]
    FROM (
        SELECT JSON_VALUE([j].[OwnedReferenceRoot], '$.Name') AS [Key]
        FROM [JsonEntitiesBasic] AS [j]
    ) AS [t]
    GROUP BY [t].[Key]
) AS [t0]
LEFT JOIN (
    SELECT [t2].[Id], [t2].[EntityBasicId], [t2].[Name], [t2].[c] AS [c], [t2].[c0] AS [c0], [t2].[Key]
    FROM (
        SELECT [t3].[Id], [t3].[EntityBasicId], [t3].[Name], [t3].[c] AS [c], [t3].[c0] AS [c0], [t3].[Key], ROW_NUMBER() OVER(PARTITION BY [t3].[Key] ORDER BY [t3].[Id]) AS [row]
        FROM (
            SELECT [j0].[Id], [j0].[EntityBasicId], [j0].[Name], [j0].[OwnedCollectionRoot] AS [c], [j0].[OwnedReferenceRoot] AS [c0], JSON_VALUE([j0].[OwnedReferenceRoot], '$.Name') AS [Key]
            FROM [JsonEntitiesBasic] AS [j0]
        ) AS [t3]
    ) AS [t2]
    WHERE [t2].[row] <= 1
) AS [t1] ON [t0].[Key] = [t1].[Key]
""");
    }

    public override async Task Group_by_FirstOrDefault_on_json_scalar(bool async)
    {
        await base.Group_by_FirstOrDefault_on_json_scalar(async);

        AssertSql(
            """
SELECT [t1].[Id], [t1].[EntityBasicId], [t1].[Name], [t1].[c], [t1].[c0]
FROM (
    SELECT [t].[Key]
    FROM (
        SELECT JSON_VALUE([j].[OwnedReferenceRoot], '$.Name') AS [Key]
        FROM [JsonEntitiesBasic] AS [j]
    ) AS [t]
    GROUP BY [t].[Key]
) AS [t0]
LEFT JOIN (
    SELECT [t2].[Id], [t2].[EntityBasicId], [t2].[Name], [t2].[c] AS [c], [t2].[c0] AS [c0], [t2].[Key]
    FROM (
        SELECT [t3].[Id], [t3].[EntityBasicId], [t3].[Name], [t3].[c] AS [c], [t3].[c0] AS [c0], [t3].[Key], ROW_NUMBER() OVER(PARTITION BY [t3].[Key] ORDER BY [t3].[Id]) AS [row]
        FROM (
            SELECT [j0].[Id], [j0].[EntityBasicId], [j0].[Name], [j0].[OwnedCollectionRoot] AS [c], [j0].[OwnedReferenceRoot] AS [c0], JSON_VALUE([j0].[OwnedReferenceRoot], '$.Name') AS [Key]
            FROM [JsonEntitiesBasic] AS [j0]
        ) AS [t3]
    ) AS [t2]
    WHERE [t2].[row] <= 1
) AS [t1] ON [t0].[Key] = [t1].[Key]
""");
    }

    public override async Task Group_by_Skip_Take_on_json_scalar(bool async)
    {
        await base.Group_by_Skip_Take_on_json_scalar(async);

        AssertSql(
            """
SELECT [t0].[Key], [t1].[Id], [t1].[EntityBasicId], [t1].[Name], [t1].[c], [t1].[c0]
FROM (
    SELECT [t].[Key]
    FROM (
        SELECT JSON_VALUE([j].[OwnedReferenceRoot], '$.Name') AS [Key]
        FROM [JsonEntitiesBasic] AS [j]
    ) AS [t]
    GROUP BY [t].[Key]
) AS [t0]
LEFT JOIN (
    SELECT [t2].[Id], [t2].[EntityBasicId], [t2].[Name], [t2].[c], [t2].[c0], [t2].[Key]
    FROM (
        SELECT [t3].[Id], [t3].[EntityBasicId], [t3].[Name], [t3].[c] AS [c], [t3].[c0] AS [c0], [t3].[Key], ROW_NUMBER() OVER(PARTITION BY [t3].[Key] ORDER BY [t3].[Id]) AS [row]
        FROM (
            SELECT [j0].[Id], [j0].[EntityBasicId], [j0].[Name], [j0].[OwnedCollectionRoot] AS [c], [j0].[OwnedReferenceRoot] AS [c0], JSON_VALUE([j0].[OwnedReferenceRoot], '$.Name') AS [Key]
            FROM [JsonEntitiesBasic] AS [j0]
        ) AS [t3]
    ) AS [t2]
    WHERE 1 < [t2].[row] AND [t2].[row] <= 6
) AS [t1] ON [t0].[Key] = [t1].[Key]
ORDER BY [t0].[Key], [t1].[Key], [t1].[Id]
""");
    }

    public override async Task Group_by_json_scalar_Orderby_json_scalar_FirstOrDefault(bool async)
    {
        await base.Group_by_json_scalar_Orderby_json_scalar_FirstOrDefault(async);

        AssertSql(
            @"");
    }

    public override async Task Group_by_json_scalar_Skip_First_project_json_scalar(bool async)
    {
        await base.Group_by_json_scalar_Skip_First_project_json_scalar(async);

        AssertSql(
"""
SELECT (
    SELECT TOP(1) CAST(JSON_VALUE([t0].[c0], '$.OwnedReferenceBranch.Enum') AS int)
    FROM (
        SELECT [j0].[Id], [j0].[EntityBasicId], [j0].[Name], [j0].[OwnedCollectionRoot] AS [c], [j0].[OwnedReferenceRoot] AS [c0], JSON_VALUE([j0].[OwnedReferenceRoot], '$.Name') AS [Key]
        FROM [JsonEntitiesBasic] AS [j0]
    ) AS [t0]
    WHERE [t].[Key] = [t0].[Key] OR ([t].[Key] IS NULL AND [t0].[Key] IS NULL))
FROM (
    SELECT JSON_VALUE([j].[OwnedReferenceRoot], '$.Name') AS [Key]
    FROM [JsonEntitiesBasic] AS [j]
) AS [t]
GROUP BY [t].[Key]
""");
    }

    public override async Task Json_with_include_on_json_entity(bool async)
    {
        await base.Json_with_include_on_json_entity(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_with_include_on_entity_reference(bool async)
    {
        await base.Json_with_include_on_entity_reference(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot], [j0].[Id], [j0].[Name], [j0].[ParentId]
FROM [JsonEntitiesBasic] AS [j]
LEFT JOIN [JsonEntitiesBasicForReference] AS [j0] ON [j].[Id] = [j0].[ParentId]
""");
    }

    public override async Task Json_with_include_on_entity_collection(bool async)
    {
        await base.Json_with_include_on_entity_collection(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot], [j0].[Id], [j0].[Name], [j0].[ParentId]
FROM [JsonEntitiesBasic] AS [j]
LEFT JOIN [JsonEntitiesBasicForCollection] AS [j0] ON [j].[Id] = [j0].[ParentId]
ORDER BY [j].[Id]
""");
    }

    public override async Task Entity_including_collection_with_json(bool async)
    {
        await base.Entity_including_collection_with_json(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot]
FROM [EntitiesBasic] AS [e]
LEFT JOIN [JsonEntitiesBasic] AS [j] ON [e].[Id] = [j].[EntityBasicId]
ORDER BY [e].[Id]
""");
    }

    public override async Task Json_with_include_on_entity_collection_and_reference(bool async)
    {
        await base.Json_with_include_on_entity_collection_and_reference(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], [j].[OwnedCollectionRoot], [j].[OwnedReferenceRoot], [j0].[Id], [j0].[Name], [j0].[ParentId], [j1].[Id], [j1].[Name], [j1].[ParentId]
FROM [JsonEntitiesBasic] AS [j]
LEFT JOIN [JsonEntitiesBasicForReference] AS [j0] ON [j].[Id] = [j0].[ParentId]
LEFT JOIN [JsonEntitiesBasicForCollection] AS [j1] ON [j].[Id] = [j1].[ParentId]
ORDER BY [j].[Id], [j0].[Id]
""");
    }

    public override async Task Json_with_projection_of_json_reference_leaf_and_entity_collection(bool async)
    {
        await base.Json_with_projection_of_json_reference_leaf_and_entity_collection(async);

        AssertSql(
            """
SELECT JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedReferenceLeaf'), [j].[Id], [j0].[Id], [j0].[Name], [j0].[ParentId]
FROM [JsonEntitiesBasic] AS [j]
LEFT JOIN [JsonEntitiesBasicForCollection] AS [j0] ON [j].[Id] = [j0].[ParentId]
ORDER BY [j].[Id]
""");
    }

    public override async Task Json_with_projection_of_json_reference_and_entity_collection(bool async)
    {
        await base.Json_with_projection_of_json_reference_and_entity_collection(async);

        AssertSql(
            """
SELECT [j].[OwnedReferenceRoot], [j].[Id], [j0].[Id], [j0].[Name], [j0].[ParentId]
FROM [JsonEntitiesBasic] AS [j]
LEFT JOIN [JsonEntitiesBasicForCollection] AS [j0] ON [j].[Id] = [j0].[ParentId]
ORDER BY [j].[Id]
""");
    }

    public override async Task Json_with_projection_of_multiple_json_references_and_entity_collection(bool async)
    {
        await base.Json_with_projection_of_multiple_json_references_and_entity_collection(async);

        AssertSql(
            """
SELECT [j].[OwnedReferenceRoot], [j].[Id], JSON_QUERY([j].[OwnedCollectionRoot], '$[0].OwnedReferenceBranch'), [j0].[Id], [j0].[Name], [j0].[ParentId], JSON_QUERY([j].[OwnedCollectionRoot], '$[1].OwnedReferenceBranch.OwnedReferenceLeaf'), JSON_QUERY([j].[OwnedCollectionRoot], '$[0].OwnedCollectionBranch[0].OwnedReferenceLeaf')
FROM [JsonEntitiesBasic] AS [j]
LEFT JOIN [JsonEntitiesBasicForCollection] AS [j0] ON [j].[Id] = [j0].[ParentId]
ORDER BY [j].[Id]
""");
    }

    public override async Task Json_with_projection_of_json_collection_leaf_and_entity_collection(bool async)
    {
        await base.Json_with_projection_of_json_collection_leaf_and_entity_collection(async);

        AssertSql(
            """
SELECT JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedCollectionLeaf'), [j].[Id], [j0].[Id], [j0].[Name], [j0].[ParentId]
FROM [JsonEntitiesBasic] AS [j]
LEFT JOIN [JsonEntitiesBasicForCollection] AS [j0] ON [j].[Id] = [j0].[ParentId]
ORDER BY [j].[Id]
""");
    }

    public override async Task Json_with_projection_of_json_collection_and_entity_collection(bool async)
    {
        await base.Json_with_projection_of_json_collection_and_entity_collection(async);

        AssertSql(
            """
SELECT [j].[OwnedCollectionRoot], [j].[Id], [j0].[Id], [j0].[Name], [j0].[ParentId]
FROM [JsonEntitiesBasic] AS [j]
LEFT JOIN [JsonEntitiesBasicForCollection] AS [j0] ON [j].[Id] = [j0].[ParentId]
ORDER BY [j].[Id]
""");
    }

    public override async Task Json_with_projection_of_json_collection_element_and_entity_collection(bool async)
    {
        await base.Json_with_projection_of_json_collection_element_and_entity_collection(async);

        AssertSql(
            """
SELECT JSON_QUERY([j].[OwnedCollectionRoot], '$[0]'), [j].[Id], [j0].[Id], [j0].[Name], [j0].[ParentId], [j1].[Id], [j1].[Name], [j1].[ParentId]
FROM [JsonEntitiesBasic] AS [j]
LEFT JOIN [JsonEntitiesBasicForReference] AS [j0] ON [j].[Id] = [j0].[ParentId]
LEFT JOIN [JsonEntitiesBasicForCollection] AS [j1] ON [j].[Id] = [j1].[ParentId]
ORDER BY [j].[Id], [j0].[Id]
""");
    }

    public override async Task Json_with_projection_of_mix_of_json_collections_json_references_and_entity_collection(bool async)
    {
        await base.Json_with_projection_of_mix_of_json_collections_json_references_and_entity_collection(async);

        AssertSql(
            """
SELECT JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedCollectionLeaf'), [j].[Id], [j0].[Id], [j0].[Name], [j0].[ParentId], JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedReferenceLeaf'), [j1].[Id], [j1].[Name], [j1].[ParentId], JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedCollectionLeaf[0]'), JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedCollectionBranch'), [j].[OwnedCollectionRoot], JSON_QUERY([j].[OwnedCollectionRoot], '$[0].OwnedReferenceBranch'), JSON_QUERY([j].[OwnedCollectionRoot], '$[0].OwnedCollectionBranch')
FROM [JsonEntitiesBasic] AS [j]
LEFT JOIN [JsonEntitiesBasicForReference] AS [j0] ON [j].[Id] = [j0].[ParentId]
LEFT JOIN [JsonEntitiesBasicForCollection] AS [j1] ON [j].[Id] = [j1].[ParentId]
ORDER BY [j].[Id], [j0].[Id]
""");

//        AssertSql(
//"""
//SELECT JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedCollectionLeaf'), [j].[Id], [j0].[Id], [j0].[Name], [j0].[ParentId], JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedReferenceBranch.OwnedReferenceLeaf'), [j1].[Id], [j1].[Name], [j1].[ParentId], JSON_QUERY([j].[OwnedReferenceRoot], '$.OwnedCollectionBranch'), [j].[OwnedCollectionRoot]
//FROM [JsonEntitiesBasic] AS [j]
//LEFT JOIN [JsonEntitiesBasicForReference] AS [j0] ON [j].[Id] = [j0].[ParentId]
//LEFT JOIN [JsonEntitiesBasicForCollection] AS [j1] ON [j].[Id] = [j1].[ParentId]
//ORDER BY [j].[Id], [j0].[Id]
//""");
    }

    public override async Task Json_all_types_entity_projection(bool async)
    {
        await base.Json_all_types_entity_projection(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
""");
    }

    public override async Task Json_all_types_projection_from_owned_entity_reference(bool async)
    {
        await base.Json_all_types_projection_from_owned_entity_reference(async);

        AssertSql(
            """
SELECT [j].[Reference], [j].[Id]
FROM [JsonEntitiesAllTypes] AS [j]
""");
    }

    public override async Task Json_all_types_projection_individual_properties(bool async)
    {
        await base.Json_all_types_projection_individual_properties(async);

        AssertSql(
"""
SELECT JSON_VALUE([j].[Reference], '$.TestDefaultString') AS [TestDefaultString], JSON_VALUE([j].[Reference], '$.TestMaxLengthString') AS [TestMaxLengthString], CAST(JSON_VALUE([j].[Reference], '$.TestBoolean') AS bit) AS [TestBoolean], CAST(JSON_VALUE([j].[Reference], '$.TestByte') AS tinyint) AS [TestByte], JSON_VALUE([j].[Reference], '$.TestCharacter') AS [TestCharacter], CAST(JSON_VALUE([j].[Reference], '$.TestDateTime') AS datetime2) AS [TestDateTime], CAST(JSON_VALUE([j].[Reference], '$.TestDateTimeOffset') AS datetimeoffset) AS [TestDateTimeOffset], CAST(JSON_VALUE([j].[Reference], '$.TestDecimal') AS decimal(18,3)) AS [TestDecimal], CAST(JSON_VALUE([j].[Reference], '$.TestDouble') AS float) AS [TestDouble], CAST(JSON_VALUE([j].[Reference], '$.TestGuid') AS uniqueidentifier) AS [TestGuid], CAST(JSON_VALUE([j].[Reference], '$.TestInt16') AS smallint) AS [TestInt16], CAST(JSON_VALUE([j].[Reference], '$.TestInt32') AS int) AS [TestInt32], CAST(JSON_VALUE([j].[Reference], '$.TestInt64') AS bigint) AS [TestInt64], CAST(JSON_VALUE([j].[Reference], '$.TestSignedByte') AS smallint) AS [TestSignedByte], CAST(JSON_VALUE([j].[Reference], '$.TestSingle') AS real) AS [TestSingle], CAST(JSON_VALUE([j].[Reference], '$.TestTimeSpan') AS time) AS [TestTimeSpan], CAST(JSON_VALUE([j].[Reference], '$.TestUnsignedInt16') AS int) AS [TestUnsignedInt16], CAST(JSON_VALUE([j].[Reference], '$.TestUnsignedInt32') AS bigint) AS [TestUnsignedInt32], CAST(JSON_VALUE([j].[Reference], '$.TestUnsignedInt64') AS decimal(20,0)) AS [TestUnsignedInt64], CAST(JSON_VALUE([j].[Reference], '$.TestEnum') AS int) AS [TestEnum], CAST(JSON_VALUE([j].[Reference], '$.TestEnumWithIntConverter') AS int) AS [TestEnumWithIntConverter], CAST(JSON_VALUE([j].[Reference], '$.TestNullableEnum') AS int) AS [TestNullableEnum], CAST(JSON_VALUE([j].[Reference], '$.TestNullableEnumWithIntConverter') AS int) AS [TestNullableEnumWithIntConverter], JSON_VALUE([j].[Reference], '$.TestNullableEnumWithConverterThatHandlesNulls') AS [TestNullableEnumWithConverterThatHandlesNulls]
FROM [JsonEntitiesAllTypes] AS [j]
""");
    }

    public override async Task Json_boolean_predicate(bool async)
    {
        await base.Json_boolean_predicate(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestBoolean') AS bit) = CAST(1 AS bit)
""");
    }

    public override async Task Json_boolean_predicate_negated(bool async)
    {
        await base.Json_boolean_predicate_negated(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestBoolean') AS bit) = CAST(0 AS bit)
""");
    }

    public override async Task Json_boolean_projection(bool async)
    {
        await base.Json_boolean_projection(async);

        AssertSql(
            """
SELECT CAST(JSON_VALUE([j].[Reference], '$.TestBoolean') AS bit)
FROM [JsonEntitiesAllTypes] AS [j]
""");
    }

    public override async Task Json_boolean_projection_negated(bool async)
    {
        await base.Json_boolean_projection_negated(async);

        AssertSql(
            """
SELECT CASE
    WHEN CAST(JSON_VALUE([j].[Reference], '$.TestBoolean') AS bit) = CAST(0 AS bit) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [JsonEntitiesAllTypes] AS [j]
""");
    }

    public override async Task Json_predicate_on_default_string(bool async)
    {
        await base.Json_predicate_on_default_string(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE JSON_VALUE([j].[Reference], '$.TestDefaultString') <> N'MyDefaultStringInReference1' OR JSON_VALUE([j].[Reference], '$.TestDefaultString') IS NULL
""");
    }

    public override async Task Json_predicate_on_max_length_string(bool async)
    {
        await base.Json_predicate_on_max_length_string(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE JSON_VALUE([j].[Reference], '$.TestMaxLengthString') <> N'Foo' OR JSON_VALUE([j].[Reference], '$.TestMaxLengthString') IS NULL
""");
    }

    public override async Task Json_predicate_on_string_condition(bool async)
    {
        await base.Json_predicate_on_string_condition(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CASE
    WHEN CAST(JSON_VALUE([j].[Reference], '$.TestBoolean') AS bit) = CAST(0 AS bit) THEN JSON_VALUE([j].[Reference], '$.TestMaxLengthString')
    ELSE JSON_VALUE([j].[Reference], '$.TestDefaultString')
END = N'MyDefaultStringInReference1'
""");
    }

    public override async Task Json_predicate_on_byte(bool async)
    {
        await base.Json_predicate_on_byte(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestByte') AS tinyint) <> CAST(3 AS tinyint) OR CAST(JSON_VALUE([j].[Reference], '$.TestByte') AS tinyint) IS NULL
""");
    }

    public override async Task Json_predicate_on_character(bool async)
    {
        await base.Json_predicate_on_character(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE JSON_VALUE([j].[Reference], '$.TestCharacter') <> N'z' OR JSON_VALUE([j].[Reference], '$.TestCharacter') IS NULL
""");
    }

    public override async Task Json_predicate_on_datetime(bool async)
    {
        await base.Json_predicate_on_datetime(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestDateTime') AS datetime2) <> '2000-01-03T00:00:00.0000000' OR CAST(JSON_VALUE([j].[Reference], '$.TestDateTime') AS datetime2) IS NULL
""");
    }

    public override async Task Json_predicate_on_datetimeoffset(bool async)
    {
        await base.Json_predicate_on_datetimeoffset(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestDateTimeOffset') AS datetimeoffset) <> '2000-01-04T00:00:00.0000000+03:02' OR CAST(JSON_VALUE([j].[Reference], '$.TestDateTimeOffset') AS datetimeoffset) IS NULL
""");
    }

    public override async Task Json_predicate_on_decimal(bool async)
    {
        await base.Json_predicate_on_decimal(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestDecimal') AS decimal(18,3)) <> 1.35 OR CAST(JSON_VALUE([j].[Reference], '$.TestDecimal') AS decimal(18,3)) IS NULL
""");
    }

    public override async Task Json_predicate_on_double(bool async)
    {
        await base.Json_predicate_on_double(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestDouble') AS float) <> 33.25E0 OR CAST(JSON_VALUE([j].[Reference], '$.TestDouble') AS float) IS NULL
""");
    }

    public override async Task Json_predicate_on_enum(bool async)
    {
        await base.Json_predicate_on_enum(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestEnum') AS int) <> 1 OR CAST(JSON_VALUE([j].[Reference], '$.TestEnum') AS int) IS NULL
""");
    }

    public override async Task Json_predicate_on_enumwithintconverter(bool async)
    {
        await base.Json_predicate_on_enumwithintconverter(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestEnumWithIntConverter') AS int) <> 2 OR CAST(JSON_VALUE([j].[Reference], '$.TestEnumWithIntConverter') AS int) IS NULL
""");
    }

    public override async Task Json_predicate_on_guid(bool async)
    {
        await base.Json_predicate_on_guid(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestGuid') AS uniqueidentifier) <> '00000000-0000-0000-0000-000000000000' OR CAST(JSON_VALUE([j].[Reference], '$.TestGuid') AS uniqueidentifier) IS NULL
""");
    }

    public override async Task Json_predicate_on_int16(bool async)
    {
        await base.Json_predicate_on_int16(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestInt16') AS smallint) <> CAST(3 AS smallint) OR CAST(JSON_VALUE([j].[Reference], '$.TestInt16') AS smallint) IS NULL
""");
    }

    public override async Task Json_predicate_on_int32(bool async)
    {
        await base.Json_predicate_on_int32(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestInt32') AS int) <> 33 OR CAST(JSON_VALUE([j].[Reference], '$.TestInt32') AS int) IS NULL
""");
    }

    public override async Task Json_predicate_on_int64(bool async)
    {
        await base.Json_predicate_on_int64(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestInt64') AS bigint) <> CAST(333 AS bigint) OR CAST(JSON_VALUE([j].[Reference], '$.TestInt64') AS bigint) IS NULL
""");
    }

    public override async Task Json_predicate_on_nullableenum1(bool async)
    {
        await base.Json_predicate_on_nullableenum1(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestNullableEnum') AS int) <> 0 OR CAST(JSON_VALUE([j].[Reference], '$.TestNullableEnum') AS int) IS NULL
""");
    }

    public override async Task Json_predicate_on_nullableenum2(bool async)
    {
        await base.Json_predicate_on_nullableenum2(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestNullableEnum') AS int) IS NOT NULL
""");
    }

    public override async Task Json_predicate_on_nullableenumwithconverter1(bool async)
    {
        await base.Json_predicate_on_nullableenumwithconverter1(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestNullableEnumWithIntConverter') AS int) <> 1 OR CAST(JSON_VALUE([j].[Reference], '$.TestNullableEnumWithIntConverter') AS int) IS NULL
""");
    }

    public override async Task Json_predicate_on_nullableenumwithconverter2(bool async)
    {
        await base.Json_predicate_on_nullableenumwithconverter2(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestNullableEnumWithIntConverter') AS int) IS NOT NULL
""");
    }

    public override async Task Json_predicate_on_nullableenumwithconverterthathandlesnulls1(bool async)
    {
        await base.Json_predicate_on_nullableenumwithconverterthathandlesnulls1(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE JSON_VALUE([j].[Reference], '$.TestNullableEnumWithConverterThatHandlesNulls') <> N'One' OR JSON_VALUE([j].[Reference], '$.TestNullableEnumWithConverterThatHandlesNulls') IS NULL
""");
    }

    public override async Task Json_predicate_on_nullableenumwithconverterthathandlesnulls2(bool async)
    {
        await base.Json_predicate_on_nullableenumwithconverterthathandlesnulls2(async);

        AssertSql(
            """
x
""");
    }

    public override async Task Json_predicate_on_nullableint321(bool async)
    {
        await base.Json_predicate_on_nullableint321(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestNullableInt32') AS int) <> 100 OR CAST(JSON_VALUE([j].[Reference], '$.TestNullableInt32') AS int) IS NULL
""");
    }

    public override async Task Json_predicate_on_nullableint322(bool async)
    {
        await base.Json_predicate_on_nullableint322(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestNullableInt32') AS int) IS NOT NULL
""");
    }

    public override async Task Json_predicate_on_signedbyte(bool async)
    {
        await base.Json_predicate_on_signedbyte(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestSignedByte') AS smallint) <> CAST(100 AS smallint) OR CAST(JSON_VALUE([j].[Reference], '$.TestSignedByte') AS smallint) IS NULL
""");
    }

    public override async Task Json_predicate_on_single(bool async)
    {
        await base.Json_predicate_on_single(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestSingle') AS real) <> CAST(10.4 AS real) OR CAST(JSON_VALUE([j].[Reference], '$.TestSingle') AS real) IS NULL
""");
    }

    public override async Task Json_predicate_on_timespan(bool async)
    {
        await base.Json_predicate_on_timespan(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestTimeSpan') AS time) <> '03:02:00' OR CAST(JSON_VALUE([j].[Reference], '$.TestTimeSpan') AS time) IS NULL
""");
    }

    public override async Task Json_predicate_on_unisgnedint16(bool async)
    {
        await base.Json_predicate_on_unisgnedint16(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestUnsignedInt16') AS int) <> 100 OR CAST(JSON_VALUE([j].[Reference], '$.TestUnsignedInt16') AS int) IS NULL
""");
    }

    public override async Task Json_predicate_on_unsignedint32(bool async)
    {
        await base.Json_predicate_on_unsignedint32(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestUnsignedInt32') AS bigint) <> CAST(1000 AS bigint) OR CAST(JSON_VALUE([j].[Reference], '$.TestUnsignedInt32') AS bigint) IS NULL
""");
    }

    public override async Task Json_predicate_on_unsignedint64(bool async)
    {
        await base.Json_predicate_on_unsignedint64(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[TestBooleanCollection], [j].[TestByteCollection], [j].[TestCharacterCollection], [j].[TestDateTimeCollection], [j].[TestDateTimeOffsetCollection], [j].[TestDecimalCollection], [j].[TestDefaultStringCollection], [j].[TestDoubleCollection], [j].[TestEnumCollection], [j].[TestEnumWithIntConverterCollection], [j].[TestGuidCollection], [j].[TestInt16Collection], [j].[TestInt32Collection], [j].[TestInt64Collection], [j].[TestMaxLengthStringCollection], [j].[TestNullableEnumCollection], [j].[TestNullableEnumWithConverterThatHandlesNullsCollection], [j].[TestNullableEnumWithIntConverterCollection], [j].[TestNullableInt32Collection], [j].[TestSignedByteCollection], [j].[TestSingleCollection], [j].[TestTimeSpanCollection], [j].[TestUnsignedInt16Collection], [j].[TestUnsignedInt32Collection], [j].[TestUnsignedInt64Collection], [j].[Collection], [j].[Reference]
FROM [JsonEntitiesAllTypes] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.TestUnsignedInt64') AS decimal(20,0)) <> 10000.0 OR CAST(JSON_VALUE([j].[Reference], '$.TestUnsignedInt64') AS decimal(20,0)) IS NULL
""");
    }

    public override async Task Json_predicate_on_bool_converted_to_int_zero_one(bool async)
    {
        await base.Json_predicate_on_bool_converted_to_int_zero_one(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[Reference]
FROM [JsonEntitiesConverters] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.BoolConvertedToIntZeroOne') AS int) = 1
""");
    }

    public override async Task Json_predicate_on_bool_converted_to_int_zero_one_with_explicit_comparison(bool async)
    {
        await base.Json_predicate_on_bool_converted_to_int_zero_one_with_explicit_comparison(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[Reference]
FROM [JsonEntitiesConverters] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.BoolConvertedToIntZeroOne') AS int) = 0
""");
    }

    public override async Task Json_predicate_on_bool_converted_to_string_True_False(bool async)
    {
        await base.Json_predicate_on_bool_converted_to_string_True_False(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[Reference]
FROM [JsonEntitiesConverters] AS [j]
WHERE JSON_VALUE([j].[Reference], '$.BoolConvertedToStringTrueFalse') = N'True'
""");
    }

    public override async Task Json_predicate_on_bool_converted_to_string_True_False_with_explicit_comparison(bool async)
    {
        await base.Json_predicate_on_bool_converted_to_string_True_False_with_explicit_comparison(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[Reference]
FROM [JsonEntitiesConverters] AS [j]
WHERE JSON_VALUE([j].[Reference], '$.BoolConvertedToStringTrueFalse') = N'True'
""");
    }

    public override async Task Json_predicate_on_bool_converted_to_string_Y_N(bool async)
    {
        await base.Json_predicate_on_bool_converted_to_string_Y_N(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[Reference]
FROM [JsonEntitiesConverters] AS [j]
WHERE JSON_VALUE([j].[Reference], '$.BoolConvertedToStringYN') = N'Y'
""");
    }

    public override async Task Json_predicate_on_bool_converted_to_string_Y_N_with_explicit_comparison(bool async)
    {
        await base.Json_predicate_on_bool_converted_to_string_Y_N_with_explicit_comparison(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[Reference]
FROM [JsonEntitiesConverters] AS [j]
WHERE JSON_VALUE([j].[Reference], '$.BoolConvertedToStringYN') = N'N'
""");
    }

    public override async Task Json_predicate_on_int_zero_one_converted_to_bool(bool async)
    {
        await base.Json_predicate_on_int_zero_one_converted_to_bool(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[Reference]
FROM [JsonEntitiesConverters] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.IntZeroOneConvertedToBool') AS bit) = CAST(1 AS bit)
""");
    }

    public override async Task Json_predicate_on_string_True_False_converted_to_bool(bool async)
    {
        await base.Json_predicate_on_string_True_False_converted_to_bool(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[Reference]
FROM [JsonEntitiesConverters] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.StringTrueFalseConvertedToBool') AS bit) = CAST(0 AS bit)
""");
    }

    public override async Task Json_predicate_on_string_Y_N_converted_to_bool(bool async)
    {
        await base.Json_predicate_on_string_Y_N_converted_to_bool(async);

        AssertSql(
            """
SELECT [j].[Id], [j].[Reference]
FROM [JsonEntitiesConverters] AS [j]
WHERE CAST(JSON_VALUE([j].[Reference], '$.StringYNConvertedToBool') AS bit) = CAST(0 AS bit)
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task FromSql_on_entity_with_json_basic(bool async)
    {
        await base.FromSql_on_entity_with_json_basic(async);

        AssertSql(
            """
SELECT [m].[Id], [m].[EntityBasicId], [m].[Name], [m].[OwnedCollectionRoot], [m].[OwnedReferenceRoot]
FROM (
    SELECT * FROM "JsonEntitiesBasic" AS j
) AS [m]
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSqlInterpolated_on_entity_with_json_with_predicate(bool async)
    {
        var parameter = new SqlParameter { ParameterName = "prm", Value = 1 };
        await AssertQuery(
            async,
            ss => ((DbSet<JsonEntityBasic>)ss.Set<JsonEntityBasic>()).FromSql(
                Fixture.TestStore.NormalizeDelimitersInInterpolatedString(
                    $"SELECT * FROM [JsonEntitiesBasic] AS j WHERE [j].[Id] = {parameter}")),
            ss => ss.Set<JsonEntityBasic>(),
            entryCount: 40);

        AssertSql(
            """
prm='1'

SELECT [m].[Id], [m].[EntityBasicId], [m].[Name], [m].[OwnedCollectionRoot], [m].[OwnedReferenceRoot]
FROM (
    SELECT * FROM "JsonEntitiesBasic" AS j WHERE "j"."Id" = @prm
) AS [m]
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task FromSql_on_entity_with_json_project_json_reference(bool async)
    {
        await base.FromSql_on_entity_with_json_project_json_reference(async);

        AssertSql(
            """
SELECT JSON_QUERY([m].[OwnedReferenceRoot], '$.OwnedReferenceBranch'), [m].[Id]
FROM (
    SELECT * FROM "JsonEntitiesBasic" AS j
) AS [m]
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task FromSql_on_entity_with_json_project_json_collection(bool async)
    {
        await base.FromSql_on_entity_with_json_project_json_collection(async);

        AssertSql(
            """
SELECT JSON_QUERY([m].[OwnedReferenceRoot], '$.OwnedCollectionBranch'), [m].[Id]
FROM (
    SELECT * FROM "JsonEntitiesBasic" AS j
) AS [m]
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task FromSql_on_entity_with_json_inheritance_on_base(bool async)
    {
        await base.FromSql_on_entity_with_json_inheritance_on_base(async);

        AssertSql(
            """
SELECT [m].[Id], [m].[Discriminator], [m].[Name], [m].[Fraction], [m].[CollectionOnBase], [m].[ReferenceOnBase], [m].[CollectionOnDerived], [m].[ReferenceOnDerived]
FROM (
    SELECT * FROM "JsonEntitiesInheritance" AS j
) AS [m]
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task FromSql_on_entity_with_json_inheritance_on_derived(bool async)
    {
        await base.FromSql_on_entity_with_json_inheritance_on_derived(async);

        AssertSql(
            """
SELECT [m].[Id], [m].[Discriminator], [m].[Name], [m].[Fraction], [m].[CollectionOnBase], [m].[ReferenceOnBase], [m].[CollectionOnDerived], [m].[ReferenceOnDerived]
FROM (
    SELECT * FROM "JsonEntitiesInheritance" AS j
) AS [m]
WHERE [m].[Discriminator] = N'JsonEntityInheritanceDerived'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task FromSql_on_entity_with_json_inheritance_project_reference_on_base(bool async)
    {
        await base.FromSql_on_entity_with_json_inheritance_project_reference_on_base(async);

        AssertSql(
            """
SELECT [m].[ReferenceOnBase], [m].[Id]
FROM (
    SELECT * FROM "JsonEntitiesInheritance" AS j
) AS [m]
ORDER BY [m].[Id]
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task FromSql_on_entity_with_json_inheritance_project_reference_on_derived(bool async)
    {
        await base.FromSql_on_entity_with_json_inheritance_project_reference_on_derived(async);

        AssertSql(
            """
SELECT [m].[CollectionOnDerived], [m].[Id]
FROM (
    SELECT * FROM "JsonEntitiesInheritance" AS j
) AS [m]
WHERE [m].[Discriminator] = N'JsonEntityInheritanceDerived'
ORDER BY [m].[Id]
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
