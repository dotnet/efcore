// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexJson;

public class ComplexJsonProjectionSqlServerTest(ComplexJsonSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonProjectionRelationalTestBase<ComplexJsonSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Select_root(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
""");
    }

    #region Simple properties

    public override async Task Select_property_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_property_on_required_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT JSON_VALUE([r].[RequiredRelated], '$.String')
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_property_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_property_on_optional_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT JSON_VALUE([r].[OptionalRelated], '$.String')
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_value_type_property_on_null_related_throws(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_value_type_property_on_null_related_throws(queryTrackingBehavior);

        AssertSql(
            """
SELECT CAST(JSON_VALUE([r].[OptionalRelated], '$.Int') AS int)
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_nullable_value_type_property_on_null_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type_property_on_null_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT CAST(JSON_VALUE([r].[OptionalRelated], '$.Int') AS int)
FROM [RootEntity] AS [r]
""");
    }

    #endregion Simple properties

    #region Non-collection

    public override async Task Select_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredRelated]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalRelated]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_required_nested_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_required_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT JSON_QUERY([r].[RequiredRelated], '$.RequiredNested')
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_nested_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_required_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT JSON_QUERY([r].[RequiredRelated], '$.OptionalNested')
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_required_nested_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_optional_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT JSON_QUERY([r].[OptionalRelated], '$.RequiredNested')
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_nested_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_optional_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT JSON_QUERY([r].[OptionalRelated], '$.OptionalNested')
FROM [RootEntity] AS [r]
""");
    }

    #endregion Non-collection

    #region Collection

    public override async Task Select_related_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_related_collection(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RelatedCollection]
FROM [RootEntity] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_nested_collection_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_required_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT JSON_QUERY([r].[RequiredRelated], '$.NestedCollection')
FROM [RootEntity] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_nested_collection_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_optional_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT JSON_QUERY([r].[OptionalRelated], '$.NestedCollection')
FROM [RootEntity] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task SelectMany_related_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_related_collection(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r0].[Id], [r0].[Int], [r0].[Name], [r0].[String], [r0].[NestedCollection], [r0].[OptionalNested], [r0].[RequiredNested]
FROM [RootEntity] AS [r]
CROSS APPLY OPENJSON([r].[RelatedCollection], '$') WITH (
    [Id] int '$.Id',
    [Int] int '$.Int',
    [Name] nvarchar(max) '$.Name',
    [String] nvarchar(max) '$.String',
    [NestedCollection] nvarchar(max) '$.NestedCollection' AS JSON,
    [OptionalNested] nvarchar(max) '$.OptionalNested' AS JSON,
    [RequiredNested] nvarchar(max) '$.RequiredNested' AS JSON
) AS [r0]
""");
    }

    public override async Task SelectMany_nested_collection_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_required_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [n].[Id], [n].[Int], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
CROSS APPLY OPENJSON([r].[RequiredRelated], '$.NestedCollection') WITH (
    [Id] int '$.Id',
    [Int] int '$.Int',
    [Name] nvarchar(max) '$.Name',
    [String] nvarchar(max) '$.String'
) AS [n]
""");
    }

    public override async Task SelectMany_nested_collection_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_optional_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [n].[Id], [n].[Int], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
CROSS APPLY OPENJSON([r].[OptionalRelated], '$.NestedCollection') WITH (
    [Id] int '$.Id',
    [Int] int '$.Int',
    [Name] nvarchar(max) '$.Name',
    [String] nvarchar(max) '$.String'
) AS [n]
""");
    }

    #endregion Collection

    #region Multiple

    public override async Task Select_root_duplicated(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root_duplicated(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
""");
    }

    #endregion Multiple

    #region Subquery

    public override async Task Select_subquery_required_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_required_related_FirstOrDefault(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r1].[c]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT TOP(1) JSON_QUERY([r0].[RequiredRelated], '$.RequiredNested') AS [c]
    FROM [RootEntity] AS [r0]
    ORDER BY [r0].[Id]
) AS [r1]
""");
    }

    public override async Task Select_subquery_optional_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_optional_related_FirstOrDefault(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r1].[c]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT TOP(1) JSON_QUERY([r0].[OptionalRelated], '$.RequiredNested') AS [c]
    FROM [RootEntity] AS [r0]
    ORDER BY [r0].[Id]
) AS [r1]
""");
    }

    #endregion Subquery

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
