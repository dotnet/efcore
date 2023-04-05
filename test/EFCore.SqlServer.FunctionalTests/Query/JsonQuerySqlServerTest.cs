// Licensed to the .NET Foundation under one or more agreements.
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
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Basic_json_projection_owner_entity(bool async)
    {
        await base.Basic_json_projection_owner_entity(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$')
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Basic_json_projection_owned_reference_root(bool async)
    {
        await base.Basic_json_projection_owned_reference_root(async);

        AssertSql(
"""
SELECT JSON_QUERY([j].[OwnedReferenceRoot],'$'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Basic_json_projection_owned_reference_duplicated(bool async)
    {
        await base.Basic_json_projection_owned_reference_duplicated(async);

        AssertSql(
"""
SELECT JSON_QUERY([j].[OwnedReferenceRoot],'$'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
ORDER BY [j].[Id]
""");
    }

    public override async Task Basic_json_projection_owned_collection_root(bool async)
    {
        await base.Basic_json_projection_owned_collection_root(async);

        AssertSql(
"""
SELECT JSON_QUERY([j].[OwnedCollectionRoot],'$'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Basic_json_projection_owned_reference_branch(bool async)
    {
        await base.Basic_json_projection_owned_reference_branch(async);

        AssertSql(
"""
SELECT JSON_QUERY([j].[OwnedReferenceRoot],'$.OwnedReferenceBranch'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Basic_json_projection_owned_collection_branch(bool async)
    {
        await base.Basic_json_projection_owned_collection_branch(async);

        AssertSql(
"""
SELECT JSON_QUERY([j].[OwnedReferenceRoot],'$.OwnedCollectionBranch'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Basic_json_projection_owned_reference_leaf(bool async)
    {
        await base.Basic_json_projection_owned_reference_leaf(async);

        AssertSql(
"""
SELECT JSON_QUERY([j].[OwnedReferenceRoot],'$.OwnedReferenceBranch.OwnedReferenceLeaf'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Basic_json_projection_owned_collection_leaf(bool async)
    {
        await base.Basic_json_projection_owned_collection_leaf(async);

        AssertSql(
"""
SELECT JSON_QUERY([j].[OwnedReferenceRoot],'$.OwnedReferenceBranch.OwnedCollectionLeaf'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Basic_json_projection_scalar(bool async)
    {
        await base.Basic_json_projection_scalar(async);

        AssertSql(
"""
SELECT CAST(JSON_VALUE([j].[OwnedReferenceRoot],'$.Name') AS nvarchar(max))
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
WHERE CAST(LEN(CAST(JSON_VALUE([j].[OwnedReferenceRoot],'$.Name') AS nvarchar(max))) AS int) > 2
""");
    }

    public override async Task Basic_json_projection_enum_inside_json_entity(bool async)
    {
        await base.Basic_json_projection_enum_inside_json_entity(async);

        AssertSql(
"""
SELECT [j].[Id], CAST(JSON_VALUE([j].[OwnedReferenceRoot],'$.OwnedReferenceBranch.Enum') AS nvarchar(max)) AS [Enum]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_projection_enum_with_custom_conversion(bool async)
    {
        await base.Json_projection_enum_with_custom_conversion(async);

        AssertSql(
"""
SELECT [j].[Id], CAST(JSON_VALUE([j].[json_reference_custom_naming],'$.CustomEnum') AS int) AS [Enum]
FROM [JsonEntitiesCustomNaming] AS [j]
""");
    }

    public override async Task Json_projection_with_deduplication(bool async)
    {
        await base.Json_projection_with_deduplication(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$'), CAST(JSON_VALUE([j].[OwnedReferenceRoot],'$.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething') AS nvarchar(max))
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_projection_with_deduplication_reverse_order(bool async)
    {
        await base.Json_projection_with_deduplication_reverse_order(async);

        AssertSql(
"""
SELECT JSON_QUERY([j].[OwnedReferenceRoot],'$'), [j].[Id], [j].[EntityBasicId], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$')
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
WHERE CAST(JSON_VALUE([j].[OwnedReferenceRoot],'$.OwnedReferenceBranch.Fraction') AS decimal(18,2)) < 20.5
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
        SELECT TOP(@__p_0) CAST(JSON_VALUE([j].[OwnedReferenceRoot],'$.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething') AS nvarchar(max)) AS [c]
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

SELECT JSON_QUERY([t0].[c],'$.OwnedReferenceBranch'), [t0].[Id]
FROM (
    SELECT DISTINCT JSON_QUERY([t].[c],'$') AS [c], [t].[Id]
    FROM (
        SELECT TOP(@__p_0) JSON_QUERY([j].[OwnedReferenceRoot],'$') AS [c], [j].[Id]
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

SELECT JSON_QUERY([t0].[c],'$.OwnedReferenceSharedBranch'), [t0].[Id], CAST(LEN([t0].[c0]) AS int)
FROM (
    SELECT DISTINCT JSON_QUERY([t].[c],'$') AS [c], [t].[Id], [t].[c0]
    FROM (
        SELECT TOP(@__p_0) JSON_QUERY([j].[json_reference_shared],'$') AS [c], [j].[Id], CAST(JSON_VALUE([j].[json_reference_shared],'$.OwnedReferenceSharedBranch.OwnedReferenceSharedLeaf.SomethingSomething') AS nvarchar(max)) AS [c0]
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

SELECT JSON_QUERY([t2].[c],'$.OwnedReferenceSharedLeaf'), [t2].[Id], JSON_QUERY([t2].[c],'$.OwnedCollectionSharedLeaf'), [t2].[Length]
FROM (
    SELECT DISTINCT JSON_QUERY([t1].[c],'$') AS [c], [t1].[Id], [t1].[Length]
    FROM (
        SELECT TOP(@__p_0) JSON_QUERY([t0].[c],'$.OwnedReferenceSharedBranch') AS [c], [t0].[Id], CAST(LEN([t0].[Scalar]) AS int) AS [Length]
        FROM (
            SELECT DISTINCT JSON_QUERY([t].[c],'$') AS [c], [t].[Id], [t].[Scalar]
            FROM (
                SELECT TOP(@__p_0) JSON_QUERY([j].[json_reference_shared],'$') AS [c], [j].[Id], CAST(JSON_VALUE([j].[json_reference_shared],'$.OwnedReferenceSharedBranch.OwnedReferenceSharedLeaf.SomethingSomething') AS nvarchar(max)) AS [Scalar]
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

SELECT JSON_QUERY([t2].[c],'$.OwnedReferenceLeaf'), [t2].[Id]
FROM (
    SELECT DISTINCT JSON_QUERY([t1].[c],'$') AS [c], [t1].[Id]
    FROM (
        SELECT TOP(@__p_0) JSON_QUERY([t0].[c],'$.OwnedReferenceBranch') AS [c], [t0].[Id]
        FROM (
            SELECT DISTINCT JSON_QUERY([t].[c],'$') AS [c], [t].[Id], [t].[c] AS [c0]
            FROM (
                SELECT TOP(@__p_0) JSON_QUERY([j].[OwnedReferenceRoot],'$') AS [c], [j].[Id]
                FROM [JsonEntitiesBasic] AS [j]
                ORDER BY [j].[Id]
            ) AS [t]
        ) AS [t0]
        ORDER BY CAST(JSON_VALUE([t0].[c0],'$.Name') AS nvarchar(max))
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

SELECT JSON_QUERY([t2].[c],'$.OwnedCollectionLeaf'), [t2].[Id]
FROM (
    SELECT DISTINCT JSON_QUERY([t1].[c],'$') AS [c], [t1].[Id]
    FROM (
        SELECT TOP(@__p_0) JSON_QUERY([t0].[c],'$.OwnedReferenceBranch') AS [c], [t0].[Id]
        FROM (
            SELECT DISTINCT JSON_QUERY([t].[c],'$') AS [c], [t].[Id], [t].[c] AS [c0]
            FROM (
                SELECT TOP(@__p_0) JSON_QUERY([j].[OwnedReferenceRoot],'$') AS [c], [j].[Id]
                FROM [JsonEntitiesBasic] AS [j]
                ORDER BY [j].[Id]
            ) AS [t]
        ) AS [t0]
        ORDER BY CAST(JSON_VALUE([t0].[c0],'$.Name') AS nvarchar(max))
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

SELECT CAST(JSON_VALUE([t0].[c],'$.SomethingSomething') AS nvarchar(max))
FROM (
    SELECT DISTINCT JSON_QUERY([t].[c],'$') AS [c], [t].[Id]
    FROM (
        SELECT TOP(@__p_0) JSON_QUERY([j].[OwnedReferenceRoot],'$.OwnedReferenceBranch.OwnedReferenceLeaf') AS [c], [j].[Id]
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
SELECT [j].[Id], [j].[Title], JSON_QUERY([j].[json_collection_custom_naming],'$'), JSON_QUERY([j].[json_reference_custom_naming],'$')
FROM [JsonEntitiesCustomNaming] AS [j]
""");
    }

    public override async Task Custom_naming_projection_owned_reference(bool async)
    {
        await base.Custom_naming_projection_owned_reference(async);

        AssertSql(
"""
SELECT JSON_QUERY([j].[json_reference_custom_naming],'$.CustomOwnedReferenceBranch'), [j].[Id]
FROM [JsonEntitiesCustomNaming] AS [j]
""");
    }

    public override async Task Custom_naming_projection_owned_collection(bool async)
    {
        await base.Custom_naming_projection_owned_collection(async);

        AssertSql(
"""
SELECT JSON_QUERY([j].[json_collection_custom_naming],'$'), [j].[Id]
FROM [JsonEntitiesCustomNaming] AS [j]
ORDER BY [j].[Id]
""");
    }

    public override async Task Custom_naming_projection_owned_scalar(bool async)
    {
        await base.Custom_naming_projection_owned_scalar(async);

        AssertSql(
"""
SELECT CAST(JSON_VALUE([j].[json_reference_custom_naming],'$.CustomOwnedReferenceBranch.CustomFraction') AS float)
FROM [JsonEntitiesCustomNaming] AS [j]
""");
    }

    public override async Task Custom_naming_projection_everything(bool async)
    {
        await base.Custom_naming_projection_everything(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[Title], JSON_QUERY([j].[json_collection_custom_naming],'$'), JSON_QUERY([j].[json_reference_custom_naming],'$'), CAST(JSON_VALUE([j].[json_reference_custom_naming],'$.CustomName') AS nvarchar(max)), CAST(JSON_VALUE([j].[json_reference_custom_naming],'$.CustomOwnedReferenceBranch.CustomFraction') AS float)
FROM [JsonEntitiesCustomNaming] AS [j]
""");
    }

    public override async Task Project_entity_with_single_owned(bool async)
    {
        await base.Project_entity_with_single_owned(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[Name], JSON_QUERY([j].[OwnedCollection],'$')
FROM [JsonEntitiesSingleOwned] AS [j]
""");
    }

    public override async Task Left_join_json_entities(bool async)
    {
        await base.Left_join_json_entities(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[Name], JSON_QUERY([j].[OwnedCollection],'$'), [j0].[Id], [j0].[EntityBasicId], [j0].[Name], JSON_QUERY([j0].[OwnedCollectionRoot],'$'), JSON_QUERY([j0].[OwnedReferenceRoot],'$')
FROM [JsonEntitiesSingleOwned] AS [j]
LEFT JOIN [JsonEntitiesBasic] AS [j0] ON [j].[Id] = [j0].[Id]
""");
    }

    public override async Task Left_join_json_entities_complex_projection(bool async)
    {
        await base.Left_join_json_entities_complex_projection(async);

        AssertSql(
"""
SELECT [j].[Id], [j0].[Id], [j0].[EntityBasicId], [j0].[Name], JSON_QUERY([j0].[OwnedCollectionRoot],'$'), JSON_QUERY([j0].[OwnedReferenceRoot],'$')
FROM [JsonEntitiesSingleOwned] AS [j]
LEFT JOIN [JsonEntitiesBasic] AS [j0] ON [j].[Id] = [j0].[Id]
""");
    }

    public override async Task Project_json_entity_FirstOrDefault_subquery(bool async)
    {
        await base.Project_json_entity_FirstOrDefault_subquery(async);

        AssertSql(
"""
SELECT JSON_QUERY([t].[c],'$'), [t].[Id]
FROM [JsonEntitiesBasic] AS [j]
OUTER APPLY (
    SELECT TOP(1) JSON_QUERY([j0].[OwnedReferenceRoot],'$.OwnedReferenceBranch') AS [c], [j0].[Id]
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
    SELECT TOP(1) CAST(JSON_VALUE([j0].[OwnedReferenceRoot],'$.OwnedReferenceBranch.Date') AS datetime2)
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
SELECT JSON_QUERY([t].[c],'$'), [t].[Id], JSON_QUERY([t].[c0],'$'), [t].[Id0], JSON_QUERY([t].[c1],'$'), [t].[c2], [t].[c3], [t].[c4]
FROM [JsonEntitiesBasic] AS [j]
OUTER APPLY (
    SELECT TOP(1) JSON_QUERY([j].[OwnedReferenceRoot],'$.OwnedCollectionBranch') AS [c], [j].[Id], JSON_QUERY([j0].[OwnedReferenceRoot],'$') AS [c0], [j0].[Id] AS [Id0], JSON_QUERY([j0].[OwnedReferenceRoot],'$.OwnedReferenceBranch') AS [c1], CAST(JSON_VALUE([j0].[OwnedReferenceRoot],'$.Name') AS nvarchar(max)) AS [c2], CAST(JSON_VALUE([j].[OwnedReferenceRoot],'$.OwnedReferenceBranch.Enum') AS nvarchar(max)) AS [c3], 1 AS [c4]
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
SELECT JSON_QUERY([t].[c],'$'), [t].[Id], JSON_QUERY([t].[c0],'$'), [t].[Id0], JSON_QUERY([t].[c1],'$'), [t].[c2], [t].[c3], [t].[c4]
FROM [JsonEntitiesBasic] AS [j]
OUTER APPLY (
    SELECT TOP(1) JSON_QUERY([j].[OwnedReferenceRoot],'$.OwnedCollectionBranch') AS [c], [j].[Id], JSON_QUERY([j0].[OwnedReferenceRoot],'$') AS [c0], [j0].[Id] AS [Id0], JSON_QUERY([j0].[OwnedReferenceRoot],'$.OwnedReferenceBranch') AS [c1], CAST(JSON_VALUE([j0].[OwnedReferenceRoot],'$.Name') AS nvarchar(max)) AS [c2], CAST(JSON_VALUE([j].[OwnedReferenceRoot],'$.OwnedReferenceBranch.Enum') AS nvarchar(max)) AS [c3], 1 AS [c4]
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
SELECT JSON_QUERY([t].[c],'$'), [t].[Id], [t].[c0]
FROM [JsonEntitiesBasic] AS [j]
OUTER APPLY (
    SELECT TOP(1) JSON_QUERY([j].[OwnedReferenceRoot],'$.OwnedCollectionBranch') AS [c], [j].[Id], 1 AS [c0]
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
SELECT [j].[Id], [j].[Discriminator], [j].[Name], [j].[Fraction], JSON_QUERY([j].[CollectionOnBase],'$'), JSON_QUERY([j].[ReferenceOnBase],'$'), JSON_QUERY([j].[CollectionOnDerived],'$'), JSON_QUERY([j].[ReferenceOnDerived],'$')
FROM [JsonEntitiesInheritance] AS [j]
""");
    }

    public override async Task Json_entity_with_inheritance_project_derived(bool async)
    {
        await base.Json_entity_with_inheritance_project_derived(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[Discriminator], [j].[Name], [j].[Fraction], JSON_QUERY([j].[CollectionOnBase],'$'), JSON_QUERY([j].[ReferenceOnBase],'$'), JSON_QUERY([j].[CollectionOnDerived],'$'), JSON_QUERY([j].[ReferenceOnDerived],'$')
FROM [JsonEntitiesInheritance] AS [j]
WHERE [j].[Discriminator] = N'JsonEntityInheritanceDerived'
""");
    }

    public override async Task Json_entity_with_inheritance_project_navigations(bool async)
    {
        await base.Json_entity_with_inheritance_project_navigations(async);

        AssertSql(
"""
SELECT [j].[Id], JSON_QUERY([j].[ReferenceOnBase],'$'), JSON_QUERY([j].[CollectionOnBase],'$')
FROM [JsonEntitiesInheritance] AS [j]
""");
    }

    public override async Task Json_entity_with_inheritance_project_navigations_on_derived(bool async)
    {
        await base.Json_entity_with_inheritance_project_navigations_on_derived(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[Discriminator], [j].[Name], [j].[Fraction], JSON_QUERY([j].[CollectionOnBase],'$'), JSON_QUERY([j].[ReferenceOnBase],'$'), JSON_QUERY([j].[CollectionOnDerived],'$'), JSON_QUERY([j].[ReferenceOnDerived],'$')
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

    public override async Task Json_collection_element_access_in_projection_basic(bool async)
    {
        await base.Json_collection_element_access_in_projection_basic(async);

        // array element access in projection is currently done on the client - issue 28648
        AssertSql(
"""
SELECT JSON_QUERY([j].[OwnedCollectionRoot],'$'), [j].[Id]
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_collection_element_access_in_predicate(bool async)
    {
        await base.Json_collection_element_access_in_predicate(async);

        AssertSql(
            @"");
    }

    public override async Task Json_scalar_required_null_semantics(bool async)
    {
        await base.Json_scalar_required_null_semantics(async);

        AssertSql(
"""
SELECT [j].[Name]
FROM [JsonEntitiesBasic] AS [j]
WHERE CAST(JSON_VALUE([j].[OwnedReferenceRoot],'$.Number') AS int) <> CAST(LEN(CAST(JSON_VALUE([j].[OwnedReferenceRoot],'$.Name') AS nvarchar(max))) AS int) OR (CAST(JSON_VALUE([j].[OwnedReferenceRoot],'$.Name') AS nvarchar(max)) IS NULL)
""");
    }

    public override async Task Json_scalar_optional_null_semantics(bool async)
    {
        await base.Json_scalar_optional_null_semantics(async);

        AssertSql(
"""
SELECT [j].[Name]
FROM [JsonEntitiesBasic] AS [j]
WHERE CAST(JSON_VALUE([j].[OwnedReferenceRoot],'$.Name') AS nvarchar(max)) = CAST(JSON_VALUE([j].[OwnedReferenceRoot],'$.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething') AS nvarchar(max)) OR ((CAST(JSON_VALUE([j].[OwnedReferenceRoot],'$.Name') AS nvarchar(max)) IS NULL) AND (CAST(JSON_VALUE([j].[OwnedReferenceRoot],'$.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething') AS nvarchar(max)) IS NULL))
""");
    }

    public override async Task Group_by_on_json_scalar(bool async)
    {
        await base.Group_by_on_json_scalar(async);

        AssertSql(
"""
SELECT [t].[Key], COUNT(*) AS [Count]
FROM (
    SELECT CAST(JSON_VALUE([j].[OwnedReferenceRoot],'$.Name') AS nvarchar(max)) AS [Key]
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
SELECT [t1].[Id], [t1].[EntityBasicId], [t1].[Name], JSON_QUERY([t1].[c],'$'), JSON_QUERY([t1].[c0],'$')
FROM (
    SELECT [t].[Key]
    FROM (
        SELECT CAST(JSON_VALUE([j].[OwnedReferenceRoot],'$.Name') AS nvarchar(max)) AS [Key]
        FROM [JsonEntitiesBasic] AS [j]
    ) AS [t]
    GROUP BY [t].[Key]
) AS [t0]
LEFT JOIN (
    SELECT [t2].[Id], [t2].[EntityBasicId], [t2].[Name], JSON_QUERY([t2].[c],'$') AS [c], JSON_QUERY([t2].[c0],'$') AS [c0], [t2].[Key]
    FROM (
        SELECT [t3].[Id], [t3].[EntityBasicId], [t3].[Name], JSON_QUERY([t3].[c],'$') AS [c], JSON_QUERY([t3].[c0],'$') AS [c0], [t3].[Key], ROW_NUMBER() OVER(PARTITION BY [t3].[Key] ORDER BY [t3].[Id]) AS [row]
        FROM (
            SELECT [j0].[Id], [j0].[EntityBasicId], [j0].[Name], JSON_QUERY([j0].[OwnedCollectionRoot],'$') AS [c], JSON_QUERY([j0].[OwnedReferenceRoot],'$') AS [c0], CAST(JSON_VALUE([j0].[OwnedReferenceRoot],'$.Name') AS nvarchar(max)) AS [Key]
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
SELECT [t1].[Id], [t1].[EntityBasicId], [t1].[Name], JSON_QUERY([t1].[c],'$'), JSON_QUERY([t1].[c0],'$')
FROM (
    SELECT [t].[Key]
    FROM (
        SELECT CAST(JSON_VALUE([j].[OwnedReferenceRoot],'$.Name') AS nvarchar(max)) AS [Key]
        FROM [JsonEntitiesBasic] AS [j]
    ) AS [t]
    GROUP BY [t].[Key]
) AS [t0]
LEFT JOIN (
    SELECT [t2].[Id], [t2].[EntityBasicId], [t2].[Name], JSON_QUERY([t2].[c],'$') AS [c], JSON_QUERY([t2].[c0],'$') AS [c0], [t2].[Key]
    FROM (
        SELECT [t3].[Id], [t3].[EntityBasicId], [t3].[Name], JSON_QUERY([t3].[c],'$') AS [c], JSON_QUERY([t3].[c0],'$') AS [c0], [t3].[Key], ROW_NUMBER() OVER(PARTITION BY [t3].[Key] ORDER BY [t3].[Id]) AS [row]
        FROM (
            SELECT [j0].[Id], [j0].[EntityBasicId], [j0].[Name], JSON_QUERY([j0].[OwnedCollectionRoot],'$') AS [c], JSON_QUERY([j0].[OwnedReferenceRoot],'$') AS [c0], CAST(JSON_VALUE([j0].[OwnedReferenceRoot],'$.Name') AS nvarchar(max)) AS [Key]
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
        SELECT CAST(JSON_VALUE([j].[OwnedReferenceRoot],'$.Name') AS nvarchar(max)) AS [Key]
        FROM [JsonEntitiesBasic] AS [j]
    ) AS [t]
    GROUP BY [t].[Key]
) AS [t0]
LEFT JOIN (
    SELECT [t2].[Id], [t2].[EntityBasicId], [t2].[Name], [t2].[c], [t2].[c0], [t2].[Key]
    FROM (
        SELECT [t3].[Id], [t3].[EntityBasicId], [t3].[Name], JSON_QUERY([t3].[c],'$') AS [c], JSON_QUERY([t3].[c0],'$') AS [c0], [t3].[Key], ROW_NUMBER() OVER(PARTITION BY [t3].[Key] ORDER BY [t3].[Id]) AS [row]
        FROM (
            SELECT [j0].[Id], [j0].[EntityBasicId], [j0].[Name], JSON_QUERY([j0].[OwnedCollectionRoot],'$') AS [c], JSON_QUERY([j0].[OwnedReferenceRoot],'$') AS [c0], CAST(JSON_VALUE([j0].[OwnedReferenceRoot],'$.Name') AS nvarchar(max)) AS [Key]
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
    SELECT TOP(1) CAST(JSON_VALUE([t0].[c0],'$.OwnedReferenceBranch.Enum') AS nvarchar(max))
    FROM (
        SELECT [j0].[Id], [j0].[EntityBasicId], [j0].[Name], JSON_QUERY([j0].[OwnedCollectionRoot],'$') AS [c], JSON_QUERY([j0].[OwnedReferenceRoot],'$') AS [c0], CAST(JSON_VALUE([j0].[OwnedReferenceRoot],'$.Name') AS nvarchar(max)) AS [Key]
        FROM [JsonEntitiesBasic] AS [j0]
    ) AS [t0]
    WHERE [t].[Key] = [t0].[Key] OR (([t].[Key] IS NULL) AND ([t0].[Key] IS NULL)))
FROM (
    SELECT CAST(JSON_VALUE([j].[OwnedReferenceRoot],'$.Name') AS nvarchar(max)) AS [Key]
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
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$')
FROM [JsonEntitiesBasic] AS [j]
""");
    }

    public override async Task Json_with_include_on_entity_reference(bool async)
    {
        await base.Json_with_include_on_entity_reference(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$'), [j0].[Id], [j0].[Name], [j0].[ParentId]
FROM [JsonEntitiesBasic] AS [j]
LEFT JOIN [JsonEntitiesBasicForReference] AS [j0] ON [j].[Id] = [j0].[ParentId]
""");
    }

    public override async Task Json_with_include_on_entity_collection(bool async)
    {
        await base.Json_with_include_on_entity_collection(async);

        AssertSql(
"""
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$'), [j0].[Id], [j0].[Name], [j0].[ParentId]
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
SELECT [e].[Id], [e].[Name], [j].[Id], [j].[EntityBasicId], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$')
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
SELECT [j].[Id], [j].[EntityBasicId], [j].[Name], JSON_QUERY([j].[OwnedCollectionRoot],'$'), JSON_QUERY([j].[OwnedReferenceRoot],'$'), [j0].[Id], [j0].[Name], [j0].[ParentId], [j1].[Id], [j1].[Name], [j1].[ParentId]
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
SELECT JSON_QUERY([j].[OwnedReferenceRoot],'$.OwnedReferenceBranch.OwnedReferenceLeaf'), [j].[Id], [j0].[Id], [j0].[Name], [j0].[ParentId]
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
SELECT JSON_QUERY([j].[OwnedReferenceRoot],'$'), [j].[Id], [j0].[Id], [j0].[Name], [j0].[ParentId]
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
SELECT JSON_QUERY([j].[OwnedReferenceRoot],'$'), [j].[Id], JSON_QUERY([j].[OwnedCollectionRoot],'$'), [j0].[Id], [j0].[Name], [j0].[ParentId]
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
SELECT JSON_QUERY([j].[OwnedReferenceRoot],'$.OwnedReferenceBranch.OwnedCollectionLeaf'), [j].[Id], [j0].[Id], [j0].[Name], [j0].[ParentId]
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
SELECT JSON_QUERY([j].[OwnedCollectionRoot],'$'), [j].[Id], [j0].[Id], [j0].[Name], [j0].[ParentId]
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
SELECT JSON_QUERY([j].[OwnedCollectionRoot],'$'), [j].[Id], [j0].[Id], [j0].[Name], [j0].[ParentId], [j1].[Id], [j1].[Name], [j1].[ParentId]
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
SELECT JSON_QUERY([j].[OwnedReferenceRoot],'$.OwnedReferenceBranch.OwnedCollectionLeaf'), [j].[Id], [j0].[Id], [j0].[Name], [j0].[ParentId], JSON_QUERY([j].[OwnedReferenceRoot],'$.OwnedReferenceBranch.OwnedReferenceLeaf'), [j1].[Id], [j1].[Name], [j1].[ParentId], JSON_QUERY([j].[OwnedReferenceRoot],'$.OwnedCollectionBranch'), JSON_QUERY([j].[OwnedCollectionRoot],'$')
FROM [JsonEntitiesBasic] AS [j]
LEFT JOIN [JsonEntitiesBasicForReference] AS [j0] ON [j].[Id] = [j0].[ParentId]
LEFT JOIN [JsonEntitiesBasicForCollection] AS [j1] ON [j].[Id] = [j1].[ParentId]
ORDER BY [j].[Id], [j0].[Id]
""");
    }

    public override async Task Json_all_types_entity_projection(bool async)
    {
        await base.Json_all_types_entity_projection(async);

        AssertSql(
"""
SELECT [j].[Id], JSON_QUERY([j].[Collection],'$'), JSON_QUERY([j].[Reference],'$')
FROM [JsonEntitiesAllTypes] AS [j]
""");
    }

    public override async Task Json_all_types_projection_individual_properties(bool async)
    {
        await base.Json_all_types_projection_individual_properties(async);

        AssertSql(
"""
SELECT CAST(JSON_VALUE([j].[Reference],'$.TestBoolean') AS bit) AS [TestBoolean], CAST(JSON_VALUE([j].[Reference],'$.TestByte') AS tinyint) AS [TestByte], CAST(JSON_VALUE([j].[Reference],'$.TestCharacter') AS nvarchar(1)) AS [TestCharacter], CAST(JSON_VALUE([j].[Reference],'$.TestDateTime') AS datetime2) AS [TestDateTime], CAST(JSON_VALUE([j].[Reference],'$.TestDateTimeOffset') AS datetimeoffset) AS [TestDateTimeOffset], CAST(JSON_VALUE([j].[Reference],'$.TestDecimal') AS decimal(18,3)) AS [TestDecimal], CAST(JSON_VALUE([j].[Reference],'$.TestDouble') AS float) AS [TestDouble], CAST(JSON_VALUE([j].[Reference],'$.TestGuid') AS uniqueidentifier) AS [TestGuid], CAST(JSON_VALUE([j].[Reference],'$.TestInt16') AS smallint) AS [TestInt16], CAST(JSON_VALUE([j].[Reference],'$.TestInt32') AS int) AS [TestInt32], CAST(JSON_VALUE([j].[Reference],'$.TestInt64') AS bigint) AS [TestInt64], CAST(JSON_VALUE([j].[Reference],'$.TestSignedByte') AS smallint) AS [TestSignedByte], CAST(JSON_VALUE([j].[Reference],'$.TestSingle') AS real) AS [TestSingle], CAST(JSON_VALUE([j].[Reference],'$.TestTimeSpan') AS time) AS [TestTimeSpan], CAST(JSON_VALUE([j].[Reference],'$.TestUnsignedInt16') AS int) AS [TestUnsignedInt16], CAST(JSON_VALUE([j].[Reference],'$.TestUnsignedInt32') AS bigint) AS [TestUnsignedInt32], CAST(JSON_VALUE([j].[Reference],'$.TestUnsignedInt64') AS decimal(20,0)) AS [TestUnsignedInt64], CAST(JSON_VALUE([j].[Reference],'$.TestNullableInt32') AS int) AS [TestNullableInt32], CAST(JSON_VALUE([j].[Reference],'$.TestEnum') AS nvarchar(max)) AS [TestEnum], CAST(JSON_VALUE([j].[Reference],'$.TestEnumWithIntConverter') AS int) AS [TestEnumWithIntConverter], CAST(JSON_VALUE([j].[Reference],'$.TestNullableEnum') AS nvarchar(max)) AS [TestNullableEnum], CAST(JSON_VALUE([j].[Reference],'$.TestNullableEnumWithIntConverter') AS int) AS [TestNullableEnumWithIntConverter], CAST(JSON_VALUE([j].[Reference],'$.TestNullableEnumWithConverterThatHandlesNulls') AS nvarchar(max)) AS [TestNullableEnumWithConverterThatHandlesNulls]
FROM [JsonEntitiesAllTypes] AS [j]
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSql_on_entity_with_json_basic(bool async)
    {
        await AssertQuery(
            async,
            ss => ((DbSet<JsonEntityBasic>)ss.Set<JsonEntityBasic>()).FromSqlRaw(
                Fixture.TestStore.NormalizeDelimitersInRawString("SELECT * FROM [JsonEntitiesBasic] AS j")),
            ss => ss.Set<JsonEntityBasic>(),
            entryCount: 40);

        AssertSql(
"""
SELECT [m].[Id], [m].[EntityBasicId], [m].[Name], JSON_QUERY([m].[OwnedCollectionRoot],'$'), JSON_QUERY([m].[OwnedReferenceRoot],'$')
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
                Fixture.TestStore.NormalizeDelimitersInInterpolatedString($"SELECT * FROM [JsonEntitiesBasic] AS j WHERE [j].[Id] = {parameter}")),
            ss => ss.Set<JsonEntityBasic>(),
            entryCount: 40);

        AssertSql(
"""
prm='1'

SELECT [m].[Id], [m].[EntityBasicId], [m].[Name], JSON_QUERY([m].[OwnedCollectionRoot],'$'), JSON_QUERY([m].[OwnedReferenceRoot],'$')
FROM (
    SELECT * FROM "JsonEntitiesBasic" AS j WHERE "j"."Id" = @prm
) AS [m]
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSql_on_entity_with_json_project_json_reference(bool async)
    {
        await AssertQuery(
            async,
            ss => ((DbSet<JsonEntityBasic>)ss.Set<JsonEntityBasic>()).FromSqlRaw(
                Fixture.TestStore.NormalizeDelimitersInRawString("SELECT * FROM [JsonEntitiesBasic] AS j"))
                .AsNoTracking()
                .Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch),
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot.OwnedReferenceBranch));

        AssertSql(
"""
SELECT JSON_QUERY([m].[OwnedReferenceRoot],'$.OwnedReferenceBranch'), [m].[Id]
FROM (
    SELECT * FROM "JsonEntitiesBasic" AS j
) AS [m]
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSql_on_entity_with_json_project_json_collection(bool async)
    {
        await AssertQuery(
            async,
            ss => ((DbSet<JsonEntityBasic>)ss.Set<JsonEntityBasic>()).FromSqlRaw(
                Fixture.TestStore.NormalizeDelimitersInRawString("SELECT * FROM [JsonEntitiesBasic] AS j"))
                .AsNoTracking()
                .Select(x => x.OwnedReferenceRoot.OwnedCollectionBranch),
            ss => ss.Set<JsonEntityBasic>().Select(x => x.OwnedReferenceRoot.OwnedCollectionBranch),
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => (ee.Date, ee.Enum, ee.Fraction)));

        AssertSql(
"""
SELECT JSON_QUERY([m].[OwnedReferenceRoot],'$.OwnedCollectionBranch'), [m].[Id]
FROM (
    SELECT * FROM "JsonEntitiesBasic" AS j
) AS [m]
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSql_on_entity_with_json_inheritance_on_base(bool async)
    {
        await AssertQuery(
            async,
            ss => ((DbSet<JsonEntityInheritanceBase>)ss.Set<JsonEntityInheritanceBase>()).FromSqlRaw(
                Fixture.TestStore.NormalizeDelimitersInRawString("SELECT * FROM [JsonEntitiesInheritance] AS j")),
            ss => ss.Set<JsonEntityInheritanceBase>(),
            entryCount: 38);

        AssertSql(
"""
SELECT [m].[Id], [m].[Discriminator], [m].[Name], [m].[Fraction], JSON_QUERY([m].[CollectionOnBase],'$'), JSON_QUERY([m].[ReferenceOnBase],'$'), JSON_QUERY([m].[CollectionOnDerived],'$'), JSON_QUERY([m].[ReferenceOnDerived],'$')
FROM (
    SELECT * FROM "JsonEntitiesInheritance" AS j
) AS [m]
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSql_on_entity_with_json_inheritance_on_derived(bool async)
    {
        await AssertQuery(
            async,
            ss => ((DbSet<JsonEntityInheritanceDerived>)ss.Set<JsonEntityInheritanceDerived>()).FromSqlRaw(
                Fixture.TestStore.NormalizeDelimitersInRawString("SELECT * FROM [JsonEntitiesInheritance] AS j")),
            ss => ss.Set<JsonEntityInheritanceDerived>(),
            entryCount: 25);

        AssertSql(
"""
SELECT [m].[Id], [m].[Discriminator], [m].[Name], [m].[Fraction], JSON_QUERY([m].[CollectionOnBase],'$'), JSON_QUERY([m].[ReferenceOnBase],'$'), JSON_QUERY([m].[CollectionOnDerived],'$'), JSON_QUERY([m].[ReferenceOnDerived],'$')
FROM (
    SELECT * FROM "JsonEntitiesInheritance" AS j
) AS [m]
WHERE [m].[Discriminator] = N'JsonEntityInheritanceDerived'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSql_on_entity_with_json_inheritance_project_reference_on_base(bool async)
    {
        await AssertQuery(
            async,
            ss => ((DbSet<JsonEntityInheritanceBase>)ss.Set<JsonEntityInheritanceBase>()).FromSqlRaw(
                Fixture.TestStore.NormalizeDelimitersInRawString("SELECT * FROM [JsonEntitiesInheritance] AS j"))
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(x => x.ReferenceOnBase),
            ss => ss.Set<JsonEntityInheritanceBase>().OrderBy(x => x.Id).Select(x => x.ReferenceOnBase),
            assertOrder: true);

        AssertSql(
"""
SELECT JSON_QUERY([m].[ReferenceOnBase],'$'), [m].[Id]
FROM (
    SELECT * FROM "JsonEntitiesInheritance" AS j
) AS [m]
ORDER BY [m].[Id]
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSql_on_entity_with_json_inheritance_project_reference_on_derived(bool async)
    {
        await AssertQuery(
            async,
            ss => ((DbSet<JsonEntityInheritanceDerived>)ss.Set<JsonEntityInheritanceDerived>()).FromSqlRaw(
                Fixture.TestStore.NormalizeDelimitersInRawString("SELECT * FROM [JsonEntitiesInheritance] AS j"))
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(x => x.CollectionOnDerived),
            ss => ss.Set<JsonEntityInheritanceDerived>().OrderBy(x => x.Id).Select(x => x.CollectionOnDerived),
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => (ee.Date, ee.Enum, ee.Fraction)),
            assertOrder: true);

        AssertSql(
"""
SELECT JSON_QUERY([m].[CollectionOnDerived],'$'), [m].[Id]
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
