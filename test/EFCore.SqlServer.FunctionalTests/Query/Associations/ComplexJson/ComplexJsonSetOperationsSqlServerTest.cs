// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public class ComplexJsonSetOperationsSqlServerTest(ComplexJsonSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonSetOperationsRelationalTestBase<ComplexJsonSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task On_related()
    {
        await base.On_related();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1 AS empty
        FROM OPENJSON([r].[RelatedCollection], '$') WITH ([Int] int '$.Int') AS [r0]
        WHERE [r0].[Int] = 8
        UNION ALL
        SELECT 1 AS empty
        FROM OPENJSON([r].[RelatedCollection], '$') WITH ([String] nvarchar(max) '$.String') AS [r1]
        WHERE [r1].[String] = N'foo'
    ) AS [u]) = 4
""");
    }

    public override async Task On_related_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.On_related_projected(queryTrackingBehavior);

        AssertSql();
    }

    public override async Task On_related_Select_nested_with_aggregates(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.On_related_Select_nested_with_aggregates(queryTrackingBehavior);

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT (
    SELECT COALESCE(SUM([s].[value]), 0)
    FROM (
        SELECT [r0].[NestedCollection] AS [NestedCollection]
        FROM OPENJSON([r].[RelatedCollection], '$') WITH (
            [Int] int '$.Int',
            [NestedCollection] json '$.NestedCollection' AS JSON
        ) AS [r0]
        WHERE [r0].[Int] = 8
        UNION ALL
        SELECT [r1].[NestedCollection] AS [NestedCollection]
        FROM OPENJSON([r].[RelatedCollection], '$') WITH (
            [String] nvarchar(max) '$.String',
            [NestedCollection] json '$.NestedCollection' AS JSON
        ) AS [r1]
        WHERE [r1].[String] = N'foo'
    ) AS [u]
    OUTER APPLY (
        SELECT COALESCE(SUM([n].[Int]), 0) AS [value]
        FROM OPENJSON([u].[NestedCollection], '$') WITH ([Int] int '$.Int') AS [n]
    ) AS [s])
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertSql(
                """
SELECT (
    SELECT COALESCE(SUM([s].[value]), 0)
    FROM (
        SELECT [r0].[NestedCollection] AS [NestedCollection]
        FROM OPENJSON([r].[RelatedCollection], '$') WITH (
            [Int] int '$.Int',
            [NestedCollection] nvarchar(max) '$.NestedCollection' AS JSON
        ) AS [r0]
        WHERE [r0].[Int] = 8
        UNION ALL
        SELECT [r1].[NestedCollection] AS [NestedCollection]
        FROM OPENJSON([r].[RelatedCollection], '$') WITH (
            [String] nvarchar(max) '$.String',
            [NestedCollection] nvarchar(max) '$.NestedCollection' AS JSON
        ) AS [r1]
        WHERE [r1].[String] = N'foo'
    ) AS [u]
    OUTER APPLY (
        SELECT COALESCE(SUM([n].[Int]), 0) AS [value]
        FROM OPENJSON([u].[NestedCollection], '$') WITH ([Int] int '$.Int') AS [n]
    ) AS [s])
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task On_nested()
    {
        await base.On_nested();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1 AS empty
        FROM OPENJSON([r].[RequiredRelated], '$.NestedCollection') WITH ([Int] int '$.Int') AS [n]
        WHERE [n].[Int] = 8
        UNION ALL
        SELECT 1 AS empty
        FROM OPENJSON([r].[RequiredRelated], '$.NestedCollection') WITH ([String] nvarchar(max) '$.String') AS [n0]
        WHERE [n0].[String] = N'foo'
    ) AS [u]) = 4
""");
    }

    public override async Task Over_different_collection_properties()
    {
        await base.Over_different_collection_properties();

        AssertSql();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
