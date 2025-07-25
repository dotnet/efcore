// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedJson;

public class OwnedJsonCollectionSqlServerTest(OwnedJsonSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : OwnedJsonCollectionRelationalTestBase<OwnedJsonSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Count()
    {
        await base.Count();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([r].[RelatedCollection], '$') AS [r0]) = 2
""");
    }

    public override async Task Where()
    {
        await base.Where();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([r].[RelatedCollection], '$') WITH (
        [Id] int '$.Id',
        [Int] int '$.Int',
        [Name] nvarchar(max) '$.Name',
        [String] nvarchar(max) '$.String'
    ) AS [r0]
    WHERE [r0].[Int] <> 8) = 2
""");
    }

    public override async Task Index_constant()
    {
        await base.Index_constant();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[RelatedCollection], '$[0].Int') AS int) = 8
""");
    }

    public override async Task OrderBy_ElementAt()
    {
        await base.OrderBy_ElementAt();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE (
    SELECT [r0].[Int]
    FROM OPENJSON([r].[RelatedCollection], '$') WITH (
        [Id] int '$.Id',
        [Int] int '$.Int',
        [Name] nvarchar(max) '$.Name',
        [String] nvarchar(max) '$.String'
    ) AS [r0]
    ORDER BY [r0].[Id]
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) = 8
""");
    }

    #region Distinct

    public override async Task Distinct()
    {
        await base.Distinct();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [r].[Id], [r0].[Id] AS [Id0], [r0].[Int], [r0].[Name], [r0].[String], [r0].[NestedCollection] AS [c], [r0].[OptionalNested] AS [c0], [r0].[RequiredNested] AS [c1]
        FROM OPENJSON([r].[RelatedCollection], '$') WITH (
            [Id] int '$.Id',
            [Int] int '$.Int',
            [Name] nvarchar(max) '$.Name',
            [String] nvarchar(max) '$.String',
            [NestedCollection] nvarchar(max) '$.NestedCollection' AS JSON,
            [OptionalNested] nvarchar(max) '$.OptionalNested' AS JSON,
            [RequiredNested] nvarchar(max) '$.RequiredNested' AS JSON
        ) AS [r0]
    ) AS [r1]) = 2
""");
    }

    public override async Task Distinct_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Distinct_projected(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r].[Id], [r1].[Id], [r1].[Id0], [r1].[Int], [r1].[Name], [r1].[String], [r1].[c], [r1].[c0], [r1].[c1]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT DISTINCT [r].[Id], [r0].[Id] AS [Id0], [r0].[Int], [r0].[Name], [r0].[String], [r0].[NestedCollection] AS [c], [r0].[OptionalNested] AS [c0], [r0].[RequiredNested] AS [c1]
    FROM OPENJSON([r].[RelatedCollection], '$') WITH (
        [Id] int '$.Id',
        [Int] int '$.Int',
        [Name] nvarchar(max) '$.Name',
        [String] nvarchar(max) '$.String',
        [NestedCollection] nvarchar(max) '$.NestedCollection' AS JSON,
        [OptionalNested] nvarchar(max) '$.OptionalNested' AS JSON,
        [RequiredNested] nvarchar(max) '$.RequiredNested' AS JSON
    ) AS [r0]
) AS [r1]
ORDER BY [r].[Id], [r1].[Id0], [r1].[Int], [r1].[Name]
""");

        }
    }

    public override async Task Distinct_over_projected_nested_collection()
    {
        await base.Distinct_over_projected_nested_collection();

        AssertSql();
    }

    public override async Task Distinct_over_projected_filtered_nested_collection()
    {
        await base.Distinct_over_projected_filtered_nested_collection();

        AssertSql();
    }

    #endregion Distinct

    #region Index

    public override async Task Index_parameter()
    {
        await base.Index_parameter();

        AssertSql(
            """
@i='?' (DbType = Int32)

SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[RelatedCollection], '$[' + CAST(@i AS nvarchar(max)) + '].Int') AS int) = 8
""");
    }

    public override async Task Index_column()
    {
        await base.Index_column();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[RelatedCollection], '$[' + CAST([r].[Id] - 1 AS nvarchar(max)) + '].Int') AS int) = 8
""");
    }

    public override async Task Index_out_of_bounds()
    {
        await base.Index_out_of_bounds();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[RelatedCollection], '$[9999].Int') AS int) = 8
""");
    }

    #endregion Index

    public override async Task Select_within_Select_within_Select_with_aggregates()
    {
        await base.Select_within_Select_within_Select_with_aggregates();

        AssertSql(
        """
SELECT (
    SELECT COALESCE(SUM([s].[value]), 0)
    FROM OPENJSON([r].[RelatedCollection], '$') WITH ([NestedCollection] nvarchar(max) '$.NestedCollection' AS JSON) AS [r0]
    OUTER APPLY (
        SELECT MAX([n].[Int]) AS [value]
        FROM OPENJSON([r0].[NestedCollection], '$') WITH (
            [Id] int '$.Id',
            [Int] int '$.Int',
            [Name] nvarchar(max) '$.Name',
            [String] nvarchar(max) '$.String'
        ) AS [n]
    ) AS [s])
FROM [RootEntity] AS [r]
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
