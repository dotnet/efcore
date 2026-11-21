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

    #region GroupBy element

    public override async Task Where_on_root_property_on_GroupBy_First()
    {
        await base.Where_on_root_property_on_GroupBy_First();

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
        SELECT TOP(1) [r3].[Id]
        FROM (
            SELECT [r4].[Id], [r4].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r4]
        ) AS [r3]
        WHERE ([r0].[Key] = [r3].[Key] OR ([r0].[Key] IS NULL AND [r3].[Key] IS NULL)) AND [r3].[Id] > 0
        ORDER BY [r3].[Id]) > 1
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

    public override async Task Select_multiple_associates_on_GroupBy_OrderByDescending_First()
    {
        await base.Select_multiple_associates_on_GroupBy_OrderByDescending_First();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT (
    SELECT TOP(1) [r7].[Id]
    FROM (
        SELECT [r8].[Id], [r8].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r8]
    ) AS [r7]
    WHERE [r9].[Key] = [r7].[Key] OR ([r9].[Key] IS NULL AND [r7].[Key] IS NULL)
    ORDER BY [r7].[Id] DESC), [r11].[Id], [r11].[Name], [r11].[c], [r11].[c0], [r11].[c1]
FROM (
    SELECT [r0].[Key], (
        SELECT TOP(1) JSON_VALUE([r3].[c1], '$.Int' RETURNING int)
        FROM (
            SELECT [r4].[Id], [r4].[RequiredAssociate] AS [c1], [r4].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r4]
        ) AS [r3]
        WHERE [r0].[Key] = [r3].[Key] OR ([r0].[Key] IS NULL AND [r3].[Key] IS NULL)
        ORDER BY [r3].[Id] DESC) AS [c], (
        SELECT TOP(1) [r5].[Id]
        FROM (
            SELECT [r6].[Id], [r6].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r6]
        ) AS [r5]
        WHERE [r0].[Key] = [r5].[Key] OR ([r0].[Key] IS NULL AND [r5].[Key] IS NULL)
        ORDER BY [r5].[Id] DESC) AS [c0]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r9]
LEFT JOIN (
    SELECT [r10].[Id], [r10].[Name], [r10].[c] AS [c], [r10].[c0] AS [c0], [r10].[c1] AS [c1], [r10].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[c] AS [c], [r1].[c0] AS [c0], [r1].[c1] AS [c1], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[AssociateCollection] AS [c], [r2].[OptionalAssociate] AS [c0], [r2].[RequiredAssociate] AS [c1], [r2].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
    ) AS [r10]
    WHERE [r10].[row] <= 1
) AS [r11] ON [r9].[Key] = [r11].[Key] OR ([r9].[Key] IS NULL AND [r11].[Key] IS NULL)
ORDER BY [r9].[c] DESC, [r9].[c0]
""");
        }
        else
        {
            AssertSql(
                """
SELECT (
    SELECT TOP(1) [r7].[Id]
    FROM (
        SELECT [r8].[Id], [r8].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r8]
    ) AS [r7]
    WHERE [r9].[Key] = [r7].[Key] OR ([r9].[Key] IS NULL AND [r7].[Key] IS NULL)
    ORDER BY [r7].[Id] DESC), [r11].[Id], [r11].[Name], [r11].[c], [r11].[c0], [r11].[c1]
FROM (
    SELECT [r0].[Key], (
        SELECT TOP(1) CAST(JSON_VALUE([r3].[c1], '$.Int') AS int)
        FROM (
            SELECT [r4].[Id], [r4].[RequiredAssociate] AS [c1], [r4].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r4]
        ) AS [r3]
        WHERE [r0].[Key] = [r3].[Key] OR ([r0].[Key] IS NULL AND [r3].[Key] IS NULL)
        ORDER BY [r3].[Id] DESC) AS [c], (
        SELECT TOP(1) [r5].[Id]
        FROM (
            SELECT [r6].[Id], [r6].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r6]
        ) AS [r5]
        WHERE [r0].[Key] = [r5].[Key] OR ([r0].[Key] IS NULL AND [r5].[Key] IS NULL)
        ORDER BY [r5].[Id] DESC) AS [c0]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r9]
LEFT JOIN (
    SELECT [r10].[Id], [r10].[Name], [r10].[c] AS [c], [r10].[c0] AS [c0], [r10].[c1] AS [c1], [r10].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[c] AS [c], [r1].[c0] AS [c0], [r1].[c1] AS [c1], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[AssociateCollection] AS [c], [r2].[OptionalAssociate] AS [c0], [r2].[RequiredAssociate] AS [c1], [r2].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
    ) AS [r10]
    WHERE [r10].[row] <= 1
) AS [r11] ON [r9].[Key] = [r11].[Key] OR ([r9].[Key] IS NULL AND [r11].[Key] IS NULL)
ORDER BY [r9].[c] DESC, [r9].[c0]
""");
        }
    }

    public override async Task Select_multiple_associates_on_GroupBy_MaxBy()
    {
        await base.Select_multiple_associates_on_GroupBy_MaxBy();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT (
    SELECT TOP(1) [r7].[Id]
    FROM (
        SELECT [r8].[Id], [r8].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r8]
    ) AS [r7]
    WHERE [r9].[Key] = [r7].[Key] OR ([r9].[Key] IS NULL AND [r7].[Key] IS NULL)
    ORDER BY [r7].[Id] DESC), [r11].[Id], [r11].[Name], [r11].[c], [r11].[c0], [r11].[c1]
FROM (
    SELECT [r0].[Key], (
        SELECT TOP(1) JSON_VALUE([r3].[c1], '$.Int' RETURNING int)
        FROM (
            SELECT [r4].[Id], [r4].[RequiredAssociate] AS [c1], [r4].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r4]
        ) AS [r3]
        WHERE [r0].[Key] = [r3].[Key] OR ([r0].[Key] IS NULL AND [r3].[Key] IS NULL)
        ORDER BY [r3].[Id] DESC) AS [c], (
        SELECT TOP(1) [r5].[Id]
        FROM (
            SELECT [r6].[Id], [r6].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r6]
        ) AS [r5]
        WHERE [r0].[Key] = [r5].[Key] OR ([r0].[Key] IS NULL AND [r5].[Key] IS NULL)
        ORDER BY [r5].[Id] DESC) AS [c0]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r9]
LEFT JOIN (
    SELECT [r10].[Id], [r10].[Name], [r10].[c] AS [c], [r10].[c0] AS [c0], [r10].[c1] AS [c1], [r10].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[c] AS [c], [r1].[c0] AS [c0], [r1].[c1] AS [c1], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[AssociateCollection] AS [c], [r2].[OptionalAssociate] AS [c0], [r2].[RequiredAssociate] AS [c1], [r2].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
    ) AS [r10]
    WHERE [r10].[row] <= 1
) AS [r11] ON [r9].[Key] = [r11].[Key] OR ([r9].[Key] IS NULL AND [r11].[Key] IS NULL)
ORDER BY [r9].[c] DESC, [r9].[c0]
""");
        }
        else
        {
            AssertSql(
                """
SELECT (
    SELECT TOP(1) [r7].[Id]
    FROM (
        SELECT [r8].[Id], [r8].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r8]
    ) AS [r7]
    WHERE [r9].[Key] = [r7].[Key] OR ([r9].[Key] IS NULL AND [r7].[Key] IS NULL)
    ORDER BY [r7].[Id] DESC), [r11].[Id], [r11].[Name], [r11].[c], [r11].[c0], [r11].[c1]
FROM (
    SELECT [r0].[Key], (
        SELECT TOP(1) CAST(JSON_VALUE([r3].[c1], '$.Int') AS int)
        FROM (
            SELECT [r4].[Id], [r4].[RequiredAssociate] AS [c1], [r4].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r4]
        ) AS [r3]
        WHERE [r0].[Key] = [r3].[Key] OR ([r0].[Key] IS NULL AND [r3].[Key] IS NULL)
        ORDER BY [r3].[Id] DESC) AS [c], (
        SELECT TOP(1) [r5].[Id]
        FROM (
            SELECT [r6].[Id], [r6].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r6]
        ) AS [r5]
        WHERE [r0].[Key] = [r5].[Key] OR ([r0].[Key] IS NULL AND [r5].[Key] IS NULL)
        ORDER BY [r5].[Id] DESC) AS [c0]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r9]
LEFT JOIN (
    SELECT [r10].[Id], [r10].[Name], [r10].[c] AS [c], [r10].[c0] AS [c0], [r10].[c1] AS [c1], [r10].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[c] AS [c], [r1].[c0] AS [c0], [r1].[c1] AS [c1], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[AssociateCollection] AS [c], [r2].[OptionalAssociate] AS [c0], [r2].[RequiredAssociate] AS [c1], [r2].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
    ) AS [r10]
    WHERE [r10].[row] <= 1
) AS [r11] ON [r9].[Key] = [r11].[Key] OR ([r9].[Key] IS NULL AND [r11].[Key] IS NULL)
ORDER BY [r9].[c] DESC, [r9].[c0]
""");
        }
    }

    public override async Task OrderBy_associate_scalar_property_on_GroupBy_First()
    {
        await base.OrderBy_associate_scalar_property_on_GroupBy_First();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r9].[Id], [r9].[Name], [r9].[c], [r9].[c0], [r9].[c1]
FROM (
    SELECT [r0].[Key], (
        SELECT TOP(1) JSON_VALUE([r3].[c1], '$.Int' RETURNING int)
        FROM (
            SELECT [r4].[Id], [r4].[RequiredAssociate] AS [c1], [r4].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r4]
        ) AS [r3]
        WHERE [r0].[Key] = [r3].[Key] OR ([r0].[Key] IS NULL AND [r3].[Key] IS NULL)
        ORDER BY [r3].[Id] DESC) AS [c], (
        SELECT TOP(1) [r5].[Id]
        FROM (
            SELECT [r6].[Id], [r6].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r6]
        ) AS [r5]
        WHERE [r0].[Key] = [r5].[Key] OR ([r0].[Key] IS NULL AND [r5].[Key] IS NULL)
        ORDER BY [r5].[Id] DESC) AS [c0]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r7]
LEFT JOIN (
    SELECT [r8].[Id], [r8].[Name], [r8].[c] AS [c], [r8].[c0] AS [c0], [r8].[c1] AS [c1], [r8].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[c] AS [c], [r1].[c0] AS [c0], [r1].[c1] AS [c1], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[AssociateCollection] AS [c], [r2].[OptionalAssociate] AS [c0], [r2].[RequiredAssociate] AS [c1], [r2].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
    ) AS [r8]
    WHERE [r8].[row] <= 1
) AS [r9] ON [r7].[Key] = [r9].[Key] OR ([r7].[Key] IS NULL AND [r9].[Key] IS NULL)
ORDER BY [r7].[c], [r7].[c0]
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r9].[Id], [r9].[Name], [r9].[c], [r9].[c0], [r9].[c1]
FROM (
    SELECT [r0].[Key], (
        SELECT TOP(1) CAST(JSON_VALUE([r3].[c1], '$.Int') AS int)
        FROM (
            SELECT [r4].[Id], [r4].[RequiredAssociate] AS [c1], [r4].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r4]
        ) AS [r3]
        WHERE [r0].[Key] = [r3].[Key] OR ([r0].[Key] IS NULL AND [r3].[Key] IS NULL)
        ORDER BY [r3].[Id] DESC) AS [c], (
        SELECT TOP(1) [r5].[Id]
        FROM (
            SELECT [r6].[Id], [r6].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r6]
        ) AS [r5]
        WHERE [r0].[Key] = [r5].[Key] OR ([r0].[Key] IS NULL AND [r5].[Key] IS NULL)
        ORDER BY [r5].[Id] DESC) AS [c0]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r7]
LEFT JOIN (
    SELECT [r8].[Id], [r8].[Name], [r8].[c] AS [c], [r8].[c0] AS [c0], [r8].[c1] AS [c1], [r8].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[c] AS [c], [r1].[c0] AS [c0], [r1].[c1] AS [c1], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[AssociateCollection] AS [c], [r2].[OptionalAssociate] AS [c0], [r2].[RequiredAssociate] AS [c1], [r2].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
    ) AS [r8]
    WHERE [r8].[row] <= 1
) AS [r9] ON [r7].[Key] = [r9].[Key] OR ([r7].[Key] IS NULL AND [r9].[Key] IS NULL)
ORDER BY [r7].[c], [r7].[c0]
""");
        }
    }

    #endregion GroupBy element

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
