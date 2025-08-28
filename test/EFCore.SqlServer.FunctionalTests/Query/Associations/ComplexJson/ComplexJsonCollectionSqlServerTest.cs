// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

using Microsoft.Data.SqlClient;

public class ComplexJsonCollectionSqlServerTest(ComplexJsonSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonCollectionRelationalTestBase<ComplexJsonSqlServerFixture>(fixture, testOutputHelper)
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
    FROM OPENJSON([r].[RelatedCollection], '$') WITH ([Int] int '$.Int') AS [r0]
    WHERE [r0].[Int] <> 8) = 2
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
        [Int] int '$.Int'
    ) AS [r0]
    ORDER BY [r0].[Id]
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) = 8
""");
    }

    #region Distinct

    public override async Task Distinct()
    {
        if (Fixture.UsingJsonType)
        {
            // The json data type cannot be selected as DISTINCT because it is not comparable.
            await Assert.ThrowsAsync<SqlException>(base.Distinct);

            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [r0].[Id], [r0].[Int], [r0].[Name], [r0].[String], [r0].[NestedCollection] AS [c], [r0].[OptionalNested] AS [c0], [r0].[RequiredNested] AS [c1]
        FROM OPENJSON([r].[RelatedCollection], '$') WITH (
            [Id] int '$.Id',
            [Int] int '$.Int',
            [Name] nvarchar(max) '$.Name',
            [String] nvarchar(max) '$.String',
            [NestedCollection] json '$.NestedCollection' AS JSON,
            [OptionalNested] json '$.OptionalNested' AS JSON,
            [RequiredNested] json '$.RequiredNested' AS JSON
        ) AS [r0]
    ) AS [r1]) = 2
""");
        }
        else
        {
            await base.Distinct();

            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [r0].[Id], [r0].[Int], [r0].[Name], [r0].[String], [r0].[NestedCollection] AS [c], [r0].[OptionalNested] AS [c0], [r0].[RequiredNested] AS [c1]
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
    }

    public override async Task Distinct_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Distinct_projected(queryTrackingBehavior);

        AssertSql();
    }

    public override async Task Distinct_over_projected_nested_collection()
    {
        if (Fixture.UsingJsonType)
        {
            // The json data type cannot be selected as DISTINCT because it is not comparable.
            await Assert.ThrowsAsync<SqlException>(base.Distinct_over_projected_nested_collection);

            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [r0].[NestedCollection] AS [c]
        FROM OPENJSON([r].[RelatedCollection], '$') WITH ([NestedCollection] json '$.NestedCollection' AS JSON) AS [r0]
    ) AS [r1]) = 2
""");
        }
        else
        {
            await base.Distinct_over_projected_nested_collection();

            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [r0].[NestedCollection] AS [c]
        FROM OPENJSON([r].[RelatedCollection], '$') WITH ([NestedCollection] nvarchar(max) '$.NestedCollection' AS JSON) AS [r0]
    ) AS [r1]) = 2
""");
        }
    }

    public override async Task Distinct_over_projected_filtered_nested_collection()
    {
        await base.Distinct_over_projected_filtered_nested_collection();

        AssertSql();
    }

    #endregion Distinct

    #region Index

    public override async Task Index_constant()
    {
        await base.Index_constant();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE JSON_VALUE([r].[RelatedCollection], '$[0].Int' RETURNING int) = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[RelatedCollection], '$[0].Int') AS int) = 8
""");
        }
    }

    public override async Task Index_parameter()
    {
        await base.Index_parameter();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
@i='?' (DbType = Int32)

SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE JSON_VALUE([r].[RelatedCollection], '$[' + CAST(@i AS nvarchar(max)) + '].Int' RETURNING int) = 8
""");
        }
        else
        {
            AssertSql(
                """
@i='?' (DbType = Int32)

SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[RelatedCollection], '$[' + CAST(@i AS nvarchar(max)) + '].Int') AS int) = 8
""");
        }
    }

    public override async Task Index_column()
    {
        await base.Index_column();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE JSON_VALUE([r].[RelatedCollection], '$[' + CAST([r].[Id] - 1 AS nvarchar(max)) + '].Int' RETURNING int) = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[RelatedCollection], '$[' + CAST([r].[Id] - 1 AS nvarchar(max)) + '].Int') AS int) = 8
""");
        }
    }

    public override async Task Index_out_of_bounds()
    {
        await base.Index_out_of_bounds();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE JSON_VALUE([r].[RelatedCollection], '$[9999].Int' RETURNING int) = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[RelatedCollection], '$[9999].Int') AS int) = 8
""");
        }
    }

    #endregion Index

    #region GroupBy

    [ConditionalFact]
    public override async Task GroupBy()
    {
        await base.GroupBy();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated], [r].[RelatedCollection], [r].[RequiredRelated]
FROM [RootEntity] AS [r]
WHERE 16 IN (
    SELECT COALESCE(SUM([r0].[Int]), 0)
    FROM OPENJSON([r].[RelatedCollection], '$') WITH (
        [Int] int '$.Int',
        [String] nvarchar(max) '$.String'
    ) AS [r0]
    GROUP BY [r0].[String]
)
""");
    }

    #endregion GroupBy

    public override async Task Select_within_Select_within_Select_with_aggregates()
    {
        await base.Select_within_Select_within_Select_with_aggregates();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT (
    SELECT COALESCE(SUM([s].[value]), 0)
    FROM OPENJSON([r].[RelatedCollection], '$') WITH ([NestedCollection] json '$.NestedCollection' AS JSON) AS [r0]
    OUTER APPLY (
        SELECT MAX([n].[Int]) AS [value]
        FROM OPENJSON([r0].[NestedCollection], '$') WITH ([Int] int '$.Int') AS [n]
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
    FROM OPENJSON([r].[RelatedCollection], '$') WITH ([NestedCollection] nvarchar(max) '$.NestedCollection' AS JSON) AS [r0]
    OUTER APPLY (
        SELECT MAX([n].[Int]) AS [value]
        FROM OPENJSON([r0].[NestedCollection], '$') WITH ([Int] int '$.Int') AS [n]
    ) AS [s])
FROM [RootEntity] AS [r]
""");
        }
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
