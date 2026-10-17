// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public class ComplexJsonSetOperationsSqlServerTest(ComplexJsonSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonSetOperationsRelationalTestBase<ComplexJsonSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Over_associate_collections()
    {
        await base.Over_associate_collections();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1 AS empty
        FROM OPENJSON([r].[AssociateCollection], '$') WITH ([Int] int '$.Int') AS [a]
        WHERE [a].[Int] = 8
        UNION ALL
        SELECT 1 AS empty
        FROM OPENJSON([r].[AssociateCollection], '$') WITH ([String] nvarchar(max) '$.String') AS [a0]
        WHERE [a0].[String] = N'foo'
    ) AS [u]) = 4
""");
    }

    public override async Task Over_associate_collection_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Over_associate_collection_projected(queryTrackingBehavior);

        AssertSql();
    }

    public override async Task Over_assocate_collection_Select_nested_with_aggregates_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Over_assocate_collection_Select_nested_with_aggregates_projected(queryTrackingBehavior);

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT (
    SELECT COALESCE(SUM([s].[value]), 0)
    FROM (
        SELECT [a].[NestedCollection] AS [NestedCollection]
        FROM OPENJSON([r].[AssociateCollection], '$') WITH (
            [Int] int '$.Int',
            [NestedCollection] json '$.NestedCollection' AS JSON
        ) AS [a]
        WHERE [a].[Int] = 8
        UNION ALL
        SELECT [a0].[NestedCollection] AS [NestedCollection]
        FROM OPENJSON([r].[AssociateCollection], '$') WITH (
            [String] nvarchar(max) '$.String',
            [NestedCollection] json '$.NestedCollection' AS JSON
        ) AS [a0]
        WHERE [a0].[String] = N'foo'
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
        SELECT [a].[NestedCollection] AS [NestedCollection]
        FROM OPENJSON([r].[AssociateCollection], '$') WITH (
            [Int] int '$.Int',
            [NestedCollection] nvarchar(max) '$.NestedCollection' AS JSON
        ) AS [a]
        WHERE [a].[Int] = 8
        UNION ALL
        SELECT [a0].[NestedCollection] AS [NestedCollection]
        FROM OPENJSON([r].[AssociateCollection], '$') WITH (
            [String] nvarchar(max) '$.String',
            [NestedCollection] nvarchar(max) '$.NestedCollection' AS JSON
        ) AS [a0]
        WHERE [a0].[String] = N'foo'
    ) AS [u]
    OUTER APPLY (
        SELECT COALESCE(SUM([n].[Int]), 0) AS [value]
        FROM OPENJSON([u].[NestedCollection], '$') WITH ([Int] int '$.Int') AS [n]
    ) AS [s])
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Over_nested_associate_collection()
    {
        await base.Over_nested_associate_collection();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1 AS empty
        FROM OPENJSON([r].[RequiredAssociate], '$.NestedCollection') WITH ([Int] int '$.Int') AS [n]
        WHERE [n].[Int] = 8
        UNION ALL
        SELECT 1 AS empty
        FROM OPENJSON([r].[RequiredAssociate], '$.NestedCollection') WITH ([String] nvarchar(max) '$.String') AS [n0]
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
