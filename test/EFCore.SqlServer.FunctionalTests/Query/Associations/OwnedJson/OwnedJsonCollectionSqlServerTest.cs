// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedJson;

public class OwnedJsonCollectionSqlServerTest(OwnedJsonSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : OwnedJsonCollectionRelationalTestBase<OwnedJsonSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Count()
    {
        await base.Count();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([r].[AssociateCollection], '$') AS [a]) = 2
""");
    }

    public override async Task Where()
    {
        await base.Where();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([r].[AssociateCollection], '$') WITH (
        [Id] int '$.Id',
        [Int] int '$.Int',
        [Ints] json '$.Ints' AS JSON,
        [Name] nvarchar(max) '$.Name',
        [String] nvarchar(max) '$.String'
    ) AS [a]
    WHERE [a].[Int] <> 8) = 2
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([r].[AssociateCollection], '$') WITH (
        [Id] int '$.Id',
        [Int] int '$.Int',
        [Ints] nvarchar(max) '$.Ints' AS JSON,
        [Name] nvarchar(max) '$.Name',
        [String] nvarchar(max) '$.String'
    ) AS [a]
    WHERE [a].[Int] <> 8) = 2
""");
        }
    }

    public override async Task OrderBy_ElementAt()
    {
        await base.OrderBy_ElementAt();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE (
    SELECT [a].[Int]
    FROM OPENJSON([r].[AssociateCollection], '$') WITH (
        [Id] int '$.Id',
        [Int] int '$.Int',
        [Ints] json '$.Ints' AS JSON,
        [Name] nvarchar(max) '$.Name',
        [String] nvarchar(max) '$.String'
    ) AS [a]
    ORDER BY [a].[Id]
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE (
    SELECT [a].[Int]
    FROM OPENJSON([r].[AssociateCollection], '$') WITH (
        [Id] int '$.Id',
        [Int] int '$.Int',
        [Ints] nvarchar(max) '$.Ints' AS JSON,
        [Name] nvarchar(max) '$.Name',
        [String] nvarchar(max) '$.String'
    ) AS [a]
    ORDER BY [a].[Id]
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) = 8
""");
        }
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
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [r].[Id], [a].[Id] AS [Id0], [a].[Int], [a].[Ints], [a].[Name], [a].[String], [a].[NestedCollection] AS [c], [a].[OptionalNestedAssociate] AS [c0], [a].[RequiredNestedAssociate] AS [c1]
        FROM OPENJSON([r].[AssociateCollection], '$') WITH (
            [Id] int '$.Id',
            [Int] int '$.Int',
            [Ints] json '$.Ints' AS JSON,
            [Name] nvarchar(max) '$.Name',
            [String] nvarchar(max) '$.String',
            [NestedCollection] json '$.NestedCollection' AS JSON,
            [OptionalNestedAssociate] json '$.OptionalNestedAssociate' AS JSON,
            [RequiredNestedAssociate] json '$.RequiredNestedAssociate' AS JSON
        ) AS [a]
    ) AS [a0]) = 2
""");
        }
        else
        {
            await base.Distinct();

            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [r].[Id], [a].[Id] AS [Id0], [a].[Int], [a].[Ints], [a].[Name], [a].[String], [a].[NestedCollection] AS [c], [a].[OptionalNestedAssociate] AS [c0], [a].[RequiredNestedAssociate] AS [c1]
        FROM OPENJSON([r].[AssociateCollection], '$') WITH (
            [Id] int '$.Id',
            [Int] int '$.Int',
            [Ints] nvarchar(max) '$.Ints' AS JSON,
            [Name] nvarchar(max) '$.Name',
            [String] nvarchar(max) '$.String',
            [NestedCollection] nvarchar(max) '$.NestedCollection' AS JSON,
            [OptionalNestedAssociate] nvarchar(max) '$.OptionalNestedAssociate' AS JSON,
            [RequiredNestedAssociate] nvarchar(max) '$.RequiredNestedAssociate' AS JSON
        ) AS [a]
    ) AS [a0]) = 2
""");
        }
    }

    public override async Task Distinct_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            await base.Distinct_projected(queryTrackingBehavior);
        }
        else if (Fixture.UsingJsonType)
        {
            // The json data type cannot be selected as DISTINCT because it is not comparable.
            await Assert.ThrowsAsync<SqlException>(() => base.Distinct_projected(queryTrackingBehavior));

            AssertSql(
                """
SELECT [r].[Id], [a0].[Id], [a0].[Id0], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[String], [a0].[c], [a0].[c0], [a0].[c1]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT DISTINCT [r].[Id], [a].[Id] AS [Id0], [a].[Int], [a].[Ints], [a].[Name], [a].[String], [a].[NestedCollection] AS [c], [a].[OptionalNestedAssociate] AS [c0], [a].[RequiredNestedAssociate] AS [c1]
    FROM OPENJSON([r].[AssociateCollection], '$') WITH (
        [Id] int '$.Id',
        [Int] int '$.Int',
        [Ints] json '$.Ints' AS JSON,
        [Name] nvarchar(max) '$.Name',
        [String] nvarchar(max) '$.String',
        [NestedCollection] json '$.NestedCollection' AS JSON,
        [OptionalNestedAssociate] json '$.OptionalNestedAssociate' AS JSON,
        [RequiredNestedAssociate] json '$.RequiredNestedAssociate' AS JSON
    ) AS [a]
) AS [a0]
ORDER BY [r].[Id], [a0].[Id0], [a0].[Int], [a0].[Ints], [a0].[Name]
""");
        }
        else
        {
            await base.Distinct_projected(queryTrackingBehavior);

            AssertSql(
                """
SELECT [r].[Id], [a0].[Id], [a0].[Id0], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[String], [a0].[c], [a0].[c0], [a0].[c1]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT DISTINCT [r].[Id], [a].[Id] AS [Id0], [a].[Int], [a].[Ints], [a].[Name], [a].[String], [a].[NestedCollection] AS [c], [a].[OptionalNestedAssociate] AS [c0], [a].[RequiredNestedAssociate] AS [c1]
    FROM OPENJSON([r].[AssociateCollection], '$') WITH (
        [Id] int '$.Id',
        [Int] int '$.Int',
        [Ints] nvarchar(max) '$.Ints' AS JSON,
        [Name] nvarchar(max) '$.Name',
        [String] nvarchar(max) '$.String',
        [NestedCollection] nvarchar(max) '$.NestedCollection' AS JSON,
        [OptionalNestedAssociate] nvarchar(max) '$.OptionalNestedAssociate' AS JSON,
        [RequiredNestedAssociate] nvarchar(max) '$.RequiredNestedAssociate' AS JSON
    ) AS [a]
) AS [a0]
ORDER BY [r].[Id], [a0].[Id0], [a0].[Int], [a0].[Ints], [a0].[Name]
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

    public override async Task Index_constant()
    {
        await base.Index_constant();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE JSON_VALUE([r].[AssociateCollection], '$[0].Int' RETURNING int) = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[AssociateCollection], '$[0].Int') AS int) = 8
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
@i='0'

SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE JSON_VALUE([r].[AssociateCollection], '$[' + CAST(@i AS nvarchar(max)) + '].Int' RETURNING int) = 8
""");
        }
        else
        {
            AssertSql(
                """
@i='0'

SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[AssociateCollection], '$[' + CAST(@i AS nvarchar(max)) + '].Int') AS int) = 8
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
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE JSON_VALUE([r].[AssociateCollection], '$[' + CAST([r].[Id] - 1 AS nvarchar(max)) + '].Int' RETURNING int) = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[AssociateCollection], '$[' + CAST([r].[Id] - 1 AS nvarchar(max)) + '].Int') AS int) = 8
""");
        }
    }

    public override async Task Index_on_nested_collection()
    {
        await base.Index_on_nested_collection();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE JSON_VALUE([r].[RequiredAssociate], '$.NestedCollection[0].Int' RETURNING int) = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[RequiredAssociate], '$.NestedCollection[0].Int') AS int) = 8
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
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE JSON_VALUE([r].[AssociateCollection], '$[9999].Int' RETURNING int) = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[AssociateCollection], '$[9999].Int') AS int) = 8
""");
        }
    }

    #endregion Index

    #region GroupBy

    [ConditionalFact]
    public override async Task GroupBy()
    {
        await base.GroupBy();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE 16 IN (
    SELECT COALESCE(SUM([a0].[Int]), 0)
    FROM (
        SELECT [a].[Id] AS [Id0], [a].[Int], [a].[Ints], [a].[Name], [a].[String], [a].[String] AS [Key0]
        FROM OPENJSON([r].[AssociateCollection], '$') WITH (
            [Id] int '$.Id',
            [Int] int '$.Int',
            [Ints] json '$.Ints' AS JSON,
            [Name] nvarchar(max) '$.Name',
            [String] nvarchar(max) '$.String'
        ) AS [a]
    ) AS [a0]
    GROUP BY [a0].[Key0]
)
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE 16 IN (
    SELECT COALESCE(SUM([a0].[Int]), 0)
    FROM (
        SELECT [a].[Id] AS [Id0], [a].[Int], [a].[Ints], [a].[Name], [a].[String], [a].[String] AS [Key0]
        FROM OPENJSON([r].[AssociateCollection], '$') WITH (
            [Id] int '$.Id',
            [Int] int '$.Int',
            [Ints] nvarchar(max) '$.Ints' AS JSON,
            [Name] nvarchar(max) '$.Name',
            [String] nvarchar(max) '$.String'
        ) AS [a]
    ) AS [a0]
    GROUP BY [a0].[Key0]
)
""");
        }
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
    FROM OPENJSON([r].[AssociateCollection], '$') WITH ([NestedCollection] json '$.NestedCollection' AS JSON) AS [a]
    OUTER APPLY (
        SELECT MAX([n].[Int]) AS [value]
        FROM OPENJSON([a].[NestedCollection], '$') WITH (
            [Id] int '$.Id',
            [Int] int '$.Int',
            [Ints] json '$.Ints' AS JSON,
            [Name] nvarchar(max) '$.Name',
            [String] nvarchar(max) '$.String'
        ) AS [n]
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
    FROM OPENJSON([r].[AssociateCollection], '$') WITH ([NestedCollection] nvarchar(max) '$.NestedCollection' AS JSON) AS [a]
    OUTER APPLY (
        SELECT MAX([n].[Int]) AS [value]
        FROM OPENJSON([a].[NestedCollection], '$') WITH (
            [Id] int '$.Id',
            [Int] int '$.Int',
            [Ints] nvarchar(max) '$.Ints' AS JSON,
            [Name] nvarchar(max) '$.Name',
            [String] nvarchar(max) '$.String'
        ) AS [n]
    ) AS [s])
FROM [RootEntity] AS [r]
""");
        }
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
