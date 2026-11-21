// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public class ComplexJsonMiscellaneousSqlServerTest(ComplexJsonSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonMiscellaneousRelationalTestBase<ComplexJsonSqlServerFixture>(fixture, testOutputHelper)
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

    #region Value types

    public override async Task Where_property_on_non_nullable_value_type()
    {
        await base.Where_property_on_non_nullable_value_type();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [v].[Id], [v].[Name], [v].[AssociateCollection], [v].[OptionalAssociate], [v].[RequiredAssociate]
FROM [ValueRootEntity] AS [v]
WHERE JSON_VALUE([v].[RequiredAssociate], '$.Int' RETURNING int) = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [v].[Id], [v].[Name], [v].[AssociateCollection], [v].[OptionalAssociate], [v].[RequiredAssociate]
FROM [ValueRootEntity] AS [v]
WHERE CAST(JSON_VALUE([v].[RequiredAssociate], '$.Int') AS int) = 8
""");
        }
    }

    public override async Task Where_property_on_nullable_value_type_Value()
    {
        await base.Where_property_on_nullable_value_type_Value();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [v].[Id], [v].[Name], [v].[AssociateCollection], [v].[OptionalAssociate], [v].[RequiredAssociate]
FROM [ValueRootEntity] AS [v]
WHERE JSON_VALUE([v].[OptionalAssociate], '$.Int' RETURNING int) = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [v].[Id], [v].[Name], [v].[AssociateCollection], [v].[OptionalAssociate], [v].[RequiredAssociate]
FROM [ValueRootEntity] AS [v]
WHERE CAST(JSON_VALUE([v].[OptionalAssociate], '$.Int') AS int) = 8
""");
        }
    }

    public override async Task Where_HasValue_on_nullable_value_type()
    {
        await base.Where_HasValue_on_nullable_value_type();

        AssertSql(
            """
SELECT [v].[Id], [v].[Name], [v].[AssociateCollection], [v].[OptionalAssociate], [v].[RequiredAssociate]
FROM [ValueRootEntity] AS [v]
WHERE [v].[OptionalAssociate] IS NOT NULL
""");
    }

    #endregion Value types

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

    public override async Task FromSql_on_root_composed(bool async)
    {
        await base.FromSql_on_root_composed(async);

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [m].[Id], [m].[Name], [m].[AssociateCollection], [m].[OptionalAssociate], [m].[RequiredAssociate]
FROM (
    SELECT * FROM "RootEntity"
) AS [m]
WHERE JSON_VALUE([m].[RequiredAssociate], '$.Int' RETURNING int) = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [m].[Id], [m].[Name], [m].[AssociateCollection], [m].[OptionalAssociate], [m].[RequiredAssociate]
FROM (
    SELECT * FROM "RootEntity"
) AS [m]
WHERE CAST(JSON_VALUE([m].[RequiredAssociate], '$.Int') AS int) = 8
""");
        }
    }

    #region Left join

    public override async Task Where_on_associate_scalar_property_over_left_join()
    {
        await base.Where_on_associate_scalar_property_over_left_join();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity] AS [r0] ON [r].[Id] + 1 = [r0].[Id]
WHERE JSON_VALUE([r0].[RequiredAssociate], '$.Int' RETURNING int) <> 8 OR JSON_VALUE([r0].[RequiredAssociate], '$.Int' RETURNING int) IS NULL
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity] AS [r0] ON [r].[Id] + 1 = [r0].[Id]
WHERE CAST(JSON_VALUE([r0].[RequiredAssociate], '$.Int') AS int) <> 8 OR CAST(JSON_VALUE([r0].[RequiredAssociate], '$.Int') AS int) IS NULL
""");
        }
    }

    public override async Task Where_on_nested_associate_scalar_property_over_left_join()
    {
        await base.Where_on_nested_associate_scalar_property_over_left_join();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity] AS [r0] ON [r].[Id] + 1 = [r0].[Id]
WHERE JSON_VALUE([r0].[RequiredAssociate], '$.RequiredNestedAssociate.Int' RETURNING int) <> 8 OR JSON_VALUE([r0].[RequiredAssociate], '$.RequiredNestedAssociate.Int' RETURNING int) IS NULL
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity] AS [r0] ON [r].[Id] + 1 = [r0].[Id]
WHERE CAST(JSON_VALUE([r0].[RequiredAssociate], '$.RequiredNestedAssociate.Int') AS int) <> 8 OR CAST(JSON_VALUE([r0].[RequiredAssociate], '$.RequiredNestedAssociate.Int') AS int) IS NULL
""");
        }
    }

    public override async Task Where_on_associate_scalar_property_over_left_join_subquery()
    {
        await base.Where_on_associate_scalar_property_over_left_join_subquery();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
@p='5'

SELECT [r].[Id]
FROM [RootEntity] AS [r]
LEFT JOIN (
    SELECT TOP(@p) [r0].[Id], [r0].[RequiredAssociate] AS [c1]
    FROM [RootEntity] AS [r0]
    ORDER BY [r0].[Id]
) AS [r1] ON [r].[Id] + 1 = [r1].[Id]
WHERE JSON_VALUE([r1].[c1], '$.Int' RETURNING int) <> 8 OR JSON_VALUE([r1].[c1], '$.Int' RETURNING int) IS NULL
""");
        }
        else
        {
            AssertSql(
                """
@p='5'

SELECT [r].[Id]
FROM [RootEntity] AS [r]
LEFT JOIN (
    SELECT TOP(@p) [r0].[Id], [r0].[RequiredAssociate] AS [c1]
    FROM [RootEntity] AS [r0]
    ORDER BY [r0].[Id]
) AS [r1] ON [r].[Id] + 1 = [r1].[Id]
WHERE CAST(JSON_VALUE([r1].[c1], '$.Int') AS int) <> 8 OR CAST(JSON_VALUE([r1].[c1], '$.Int') AS int) IS NULL
""");
        }
    }

    #endregion Left join

    #region Subquery

    public override async Task Where_on_associate_scalar_property_on_GroupBy_First()
    {
        await base.Where_on_associate_scalar_property_on_GroupBy_First();

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
    HAVING (
        SELECT TOP(1) JSON_VALUE([r3].[c1], '$.Int' RETURNING int)
        FROM (
            SELECT [r4].[Id], [r4].[RequiredAssociate] AS [c1], [r4].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r4]
        ) AS [r3]
        WHERE ([r0].[Key] = [r3].[Key] OR ([r0].[Key] IS NULL AND [r3].[Key] IS NULL)) AND [r3].[Id] > 0
        ORDER BY [r3].[Id]) = 8
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
    HAVING (
        SELECT TOP(1) CAST(JSON_VALUE([r3].[c1], '$.Int') AS int)
        FROM (
            SELECT [r4].[Id], [r4].[RequiredAssociate] AS [c1], [r4].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r4]
        ) AS [r3]
        WHERE ([r0].[Key] = [r3].[Key] OR ([r0].[Key] IS NULL AND [r3].[Key] IS NULL)) AND [r3].[Id] > 0
        ORDER BY [r3].[Id]) = 8
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

    #endregion Subquery

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
