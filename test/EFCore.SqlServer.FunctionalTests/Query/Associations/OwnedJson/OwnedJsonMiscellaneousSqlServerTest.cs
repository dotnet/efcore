// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedJson;

public class OwnedJsonMiscellaneousSqlServerTest(
    OwnedJsonSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : OwnedJsonMiscellaneousRelationalTestBase<OwnedJsonSqlServerFixture>(fixture, testOutputHelper)
{
    #region Simple filters

    public override async Task Where_on_associate_scalar_property()
    {
        await base.Where_on_associate_scalar_property();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE JSON_VALUE([r].[RequiredAssociate], '$.Int' RETURNING int) = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[RequiredAssociate], '$.Int') AS int) = 8
""");
        }
    }

    public override async Task Where_on_optional_associate_scalar_property()
    {
        await base.Where_on_optional_associate_scalar_property();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE JSON_VALUE([r].[OptionalAssociate], '$.Int' RETURNING int) = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[OptionalAssociate], '$.Int') AS int) = 8
""");
        }
    }

    public override async Task Where_on_nested_associate_scalar_property()
    {
        await base.Where_on_nested_associate_scalar_property();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE JSON_VALUE([r].[RequiredAssociate], '$.RequiredNestedAssociate.Int' RETURNING int) = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[RequiredAssociate], '$.RequiredNestedAssociate.Int') AS int) = 8
""");
        }
    }

    #endregion Simple filters

    public override async Task FromSql_on_root()
    {
        await base.FromSql_on_root();

        AssertSql(
            """
SELECT [m].[Id], [m].[Name], [m].[AssociateCollection], [m].[OptionalAssociate], [m].[RequiredAssociate]
FROM (
    SELECT * FROM [RootEntity]
) AS [m]
""");
    }

    #region GroupBy

    public override async Task GroupBy_OrderByDescending_First_on_associate_scalar_property()
    {
        await base.GroupBy_OrderByDescending_First_on_associate_scalar_property();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r1].[Id], [r1].[Name], [r1].[c], [r1].[c0], [r1].[c1]
FROM (
    SELECT [r0].[Id], [r0].[Name], [r0].[c] AS [c], [r0].[c0] AS [c0], [r0].[c1] AS [c1], ROW_NUMBER() OVER(PARTITION BY [r0].[Key] ORDER BY JSON_VALUE([r0].[c1], '$.Int' RETURNING int) DESC) AS [row]
    FROM (
        SELECT [r].[Id], [r].[Name], [r].[AssociateCollection] AS [c], [r].[OptionalAssociate] AS [c0], [r].[RequiredAssociate] AS [c1], [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
) AS [r1]
WHERE [r1].[row] <= 1
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r1].[Id], [r1].[Name], [r1].[c], [r1].[c0], [r1].[c1]
FROM (
    SELECT [r0].[Id], [r0].[Name], [r0].[c] AS [c], [r0].[c0] AS [c0], [r0].[c1] AS [c1], ROW_NUMBER() OVER(PARTITION BY [r0].[Key] ORDER BY CAST(JSON_VALUE([r0].[c1], '$.Int') AS int) DESC) AS [row]
    FROM (
        SELECT [r].[Id], [r].[Name], [r].[AssociateCollection] AS [c], [r].[OptionalAssociate] AS [c0], [r].[RequiredAssociate] AS [c1], [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
) AS [r1]
WHERE [r1].[row] <= 1
""");
        }
    }

    public override async Task GroupBy_MaxBy_on_associate_scalar_property()
    {
        await base.GroupBy_MaxBy_on_associate_scalar_property();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r1].[Id], [r1].[Name], [r1].[c], [r1].[c0], [r1].[c1]
FROM (
    SELECT [r0].[Id], [r0].[Name], [r0].[c] AS [c], [r0].[c0] AS [c0], [r0].[c1] AS [c1], ROW_NUMBER() OVER(PARTITION BY [r0].[Key] ORDER BY JSON_VALUE([r0].[c1], '$.Int' RETURNING int) DESC) AS [row]
    FROM (
        SELECT [r].[Id], [r].[Name], [r].[AssociateCollection] AS [c], [r].[OptionalAssociate] AS [c0], [r].[RequiredAssociate] AS [c1], [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
) AS [r1]
WHERE [r1].[row] <= 1
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r1].[Id], [r1].[Name], [r1].[c], [r1].[c0], [r1].[c1]
FROM (
    SELECT [r0].[Id], [r0].[Name], [r0].[c] AS [c], [r0].[c0] AS [c0], [r0].[c1] AS [c1], ROW_NUMBER() OVER(PARTITION BY [r0].[Key] ORDER BY CAST(JSON_VALUE([r0].[c1], '$.Int') AS int) DESC) AS [row]
    FROM (
        SELECT [r].[Id], [r].[Name], [r].[AssociateCollection] AS [c], [r].[OptionalAssociate] AS [c0], [r].[RequiredAssociate] AS [c1], [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
) AS [r1]
WHERE [r1].[row] <= 1
""");
        }
    }

    public override async Task GroupBy_Max_on_associate_scalar_property()
    {
        await base.GroupBy_Max_on_associate_scalar_property();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r0].[Key], MAX(JSON_VALUE([r0].[c1], '$.Int' RETURNING int)) AS [Max]
FROM (
    SELECT [r].[RequiredAssociate] AS [c1], [r].[Id] % 2 AS [Key]
    FROM [RootEntity] AS [r]
) AS [r0]
GROUP BY [r0].[Key]
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r0].[Key], MAX(CAST(JSON_VALUE([r0].[c1], '$.Int') AS int)) AS [Max]
FROM (
    SELECT [r].[RequiredAssociate] AS [c1], [r].[Id] % 2 AS [Key]
    FROM [RootEntity] AS [r]
) AS [r0]
GROUP BY [r0].[Key]
""");
        }
    }

    public override async Task GroupBy_Count_with_predicate_on_associate_scalar_property()
    {
        await base.GroupBy_Count_with_predicate_on_associate_scalar_property();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r0].[Key], COUNT(CASE
    WHEN JSON_VALUE([r0].[c1], '$.Int' RETURNING int) = 8 THEN 1
END) AS [Count]
FROM (
    SELECT [r].[RequiredAssociate] AS [c1], [r].[Id] % 2 AS [Key]
    FROM [RootEntity] AS [r]
) AS [r0]
GROUP BY [r0].[Key]
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r0].[Key], COUNT(CASE
    WHEN CAST(JSON_VALUE([r0].[c1], '$.Int') AS int) = 8 THEN 1
END) AS [Count]
FROM (
    SELECT [r].[RequiredAssociate] AS [c1], [r].[Id] % 2 AS [Key]
    FROM [RootEntity] AS [r]
) AS [r0]
GROUP BY [r0].[Key]
""");
        }
    }

    #endregion GroupBy

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
