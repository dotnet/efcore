// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public class ComplexJsonPrimitiveCollectionSqlServerTest(ComplexJsonSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonPrimitiveCollectionRelationalTestBase<ComplexJsonSqlServerFixture>(fixture, testOutputHelper)
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
    FROM OPENJSON(JSON_QUERY([r].[RequiredAssociate], '$.Ints')) AS [i]) = 3
""");
    }

    public override async Task Index()
    {
        await base.Index();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE JSON_VALUE([r].[RequiredAssociate], '$.Ints[0]' RETURNING int) = 1
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[RequiredAssociate], '$.Ints[0]') AS int) = 1
""");
        }
    }

    public override async Task Contains()
    {
        await base.Contains();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE JSON_CONTAINS(JSON_QUERY([r].[RequiredAssociate], '$.Ints'), 3) = 1
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE 3 IN (
    SELECT [i].[value]
    FROM OPENJSON(JSON_QUERY([r].[RequiredAssociate], '$.Ints')) WITH ([value] int '$') AS [i]
)
""");
        }
    }

    public override async Task Any_predicate()
    {
        await base.Any_predicate();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE JSON_CONTAINS(JSON_QUERY([r].[RequiredAssociate], '$.Ints'), 2) = 1
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE 2 IN (
    SELECT [i].[value]
    FROM OPENJSON(JSON_QUERY([r].[RequiredAssociate], '$.Ints')) WITH ([value] int '$') AS [i]
)
""");
        }
    }

    public override async Task Nested_Count()
    {
        await base.Nested_Count();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON(JSON_QUERY([r].[RequiredAssociate], '$.RequiredNestedAssociate.Ints')) AS [i]) = 3
""");
    }

    public override async Task Select_Sum()
    {
        await base.Select_Sum();

        AssertSql(
            """
SELECT (
    SELECT ISNULL(SUM([i0].[value]), 0)
    FROM OPENJSON(JSON_QUERY([r].[RequiredAssociate], '$.Ints')) WITH ([value] int '$') AS [i0])
FROM [RootEntity] AS [r]
WHERE (
    SELECT ISNULL(SUM([i].[value]), 0)
    FROM OPENJSON(JSON_QUERY([r].[RequiredAssociate], '$.Ints')) WITH ([value] int '$') AS [i]) >= 6
""");
    }

    public override async Task Any_predicate_on_GroupBy_First()
    {
        await base.Any_predicate_on_GroupBy_First();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r7].[Id], [r7].[Name], [r7].[c], [r7].[c0], [r7].[c1]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
    HAVING JSON_CONTAINS((
        SELECT TOP(1) JSON_QUERY([r3].[c1], '$.Ints')
        FROM (
            SELECT [r4].[Id], [r4].[RequiredAssociate] AS [c1], [r4].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r4]
        ) AS [r3]
        WHERE ([r0].[Key] = [r3].[Key] OR ([r0].[Key] IS NULL AND [r3].[Key] IS NULL)) AND [r3].[Id] > 0
        ORDER BY [r3].[Id]), 2) = 1
) AS [r5]
LEFT JOIN (
    SELECT [r6].[Id], [r6].[Name], [r6].[c] AS [c], [r6].[c0] AS [c0], [r6].[c1] AS [c1], [r6].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[c] AS [c], [r1].[c0] AS [c0], [r1].[c1] AS [c1], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id]) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[AssociateCollection] AS [c], [r2].[OptionalAssociate] AS [c0], [r2].[RequiredAssociate] AS [c1], [r2].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
        WHERE [r1].[Id] > 0
    ) AS [r6]
    WHERE [r6].[row] <= 1
) AS [r7] ON [r5].[Key] = [r7].[Key] OR ([r5].[Key] IS NULL AND [r7].[Key] IS NULL)
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r7].[Id], [r7].[Name], [r7].[c], [r7].[c0], [r7].[c1]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
    HAVING 2 IN (
        SELECT [i].[value]
        FROM OPENJSON((
            SELECT TOP(1) JSON_QUERY([r3].[c1], '$.Ints')
            FROM (
                SELECT [r4].[Id], [r4].[RequiredAssociate] AS [c1], [r4].[Id] % 2 AS [Key]
                FROM [RootEntity] AS [r4]
            ) AS [r3]
            WHERE ([r0].[Key] = [r3].[Key] OR ([r0].[Key] IS NULL AND [r3].[Key] IS NULL)) AND [r3].[Id] > 0
            ORDER BY [r3].[Id])) WITH ([value] int '$') AS [i]
    )
) AS [r5]
LEFT JOIN (
    SELECT [r6].[Id], [r6].[Name], [r6].[c] AS [c], [r6].[c0] AS [c0], [r6].[c1] AS [c1], [r6].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[c] AS [c], [r1].[c0] AS [c0], [r1].[c1] AS [c1], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id]) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[AssociateCollection] AS [c], [r2].[OptionalAssociate] AS [c0], [r2].[RequiredAssociate] AS [c1], [r2].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
        WHERE [r1].[Id] > 0
    ) AS [r6]
    WHERE [r6].[row] <= 1
) AS [r7] ON [r5].[Key] = [r7].[Key] OR ([r5].[Key] IS NULL AND [r7].[Key] IS NULL)
""");
        }
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
