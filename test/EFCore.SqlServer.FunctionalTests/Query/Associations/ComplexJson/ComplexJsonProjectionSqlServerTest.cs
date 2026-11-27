// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public class ComplexJsonProjectionSqlServerTest(ComplexJsonSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonProjectionRelationalTestBase<ComplexJsonSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Select_complex_properties_with_identity_resolution(bool async)
    {
        await base.Select_complex_properties_with_identity_resolution(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[RequiredAssociate], [r].[OptionalAssociate]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_complex_properties_with_identity_resolution_member_init(bool async)
    {
        await base.Select_complex_properties_with_identity_resolution_member_init(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[RequiredAssociate], [r].[OptionalAssociate]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_root(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
""");
    }

    #region Scalar properties

    public override async Task Select_scalar_property_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_scalar_property_on_required_associate(queryTrackingBehavior);

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT JSON_VALUE([r].[RequiredAssociate], '$.String' RETURNING nvarchar(max))
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertSql(
                """
SELECT JSON_VALUE([r].[RequiredAssociate], '$.String')
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Select_property_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_property_on_optional_associate(queryTrackingBehavior);

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT JSON_VALUE([r].[OptionalAssociate], '$.String' RETURNING nvarchar(max))
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertSql(
                """
SELECT JSON_VALUE([r].[OptionalAssociate], '$.String')
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Select_value_type_property_on_null_associate_throws(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_value_type_property_on_null_associate_throws(queryTrackingBehavior);

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT JSON_VALUE([r].[OptionalAssociate], '$.Int' RETURNING int)
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertSql(
                """
SELECT CAST(JSON_VALUE([r].[OptionalAssociate], '$.Int') AS int)
FROM [RootEntity] AS [r]
""");
        }
    }

    public override async Task Select_nullable_value_type_property_on_null_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type_property_on_null_associate(queryTrackingBehavior);

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT JSON_VALUE([r].[OptionalAssociate], '$.Int' RETURNING int)
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertSql(
                """
SELECT CAST(JSON_VALUE([r].[OptionalAssociate], '$.Int') AS int)
FROM [RootEntity] AS [r]
""");
        }
    }

    #endregion Scalar properties

    #region Structural properties

    public override async Task Select_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalAssociate]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_required_nested_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT JSON_QUERY([r].[RequiredAssociate], '$.RequiredNestedAssociate')
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_nested_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT JSON_QUERY([r].[RequiredAssociate], '$.OptionalNestedAssociate')
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_required_nested_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT JSON_QUERY([r].[OptionalAssociate], '$.RequiredNestedAssociate')
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_nested_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT JSON_QUERY([r].[OptionalAssociate], '$.OptionalNestedAssociate')
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_required_associate_via_optional_navigation(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_associate_via_optional_navigation(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r0].[RequiredAssociate]
FROM [RootReferencingEntity] AS [r]
LEFT JOIN [RootEntity] AS [r0] ON [r].[RootEntityId] = [r0].[Id]
""");
    }

    public override async Task Select_unmapped_associate_scalar_property(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_unmapped_associate_scalar_property(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_untranslatable_method_on_associate_scalar_property(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_untranslatable_method_on_associate_scalar_property(queryTrackingBehavior);

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT JSON_VALUE([r].[RequiredAssociate], '$.Int' RETURNING int)
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertSql(
                """
SELECT CAST(JSON_VALUE([r].[RequiredAssociate], '$.Int') AS int)
FROM [RootEntity] AS [r]
""");
        }
    }

    #endregion Structural properties

    #region Structural collection properties

    public override async Task Select_associate_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate_collection(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[AssociateCollection]
FROM [RootEntity] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_nested_collection_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT JSON_QUERY([r].[RequiredAssociate], '$.NestedCollection')
FROM [RootEntity] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_nested_collection_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT JSON_QUERY([r].[OptionalAssociate], '$.NestedCollection')
FROM [RootEntity] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task SelectMany_associate_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_associate_collection(queryTrackingBehavior);

        if (Fixture.UsingJsonType && !SqlServerTestEnvironment.IsAzureSql)
        {
            AssertSql(
                """
SELECT [a].[Id], [a].[Int], [a].[Ints], [a].[Name], [a].[String], [a].[NestedCollection], [a].[OptionalNestedAssociate], [a].[RequiredNestedAssociate]
FROM [RootEntity] AS [r]
CROSS APPLY OPENJSON([r].[AssociateCollection], '$') WITH (
    [Id] int '$.Id',
    [Int] int '$.Int',
    [Ints] json '$.Ints' AS JSON,
    [Name] nvarchar(max) '$.Name',
    [String] nvarchar(max) '$.String',
    [NestedCollection] json '$.NestedCollection' AS JSON,
    [OptionalNestedAssociate] json '$.OptionalNestedAssociate' AS JSON,
    [RequiredNestedAssociate] json '$.RequiredNestedAssociate' AS JSON
) AS [a]
""");
        }
        else
        {
            AssertSql(
                """
SELECT [a].[Id], [a].[Int], [a].[Ints], [a].[Name], [a].[String], [a].[NestedCollection], [a].[OptionalNestedAssociate], [a].[RequiredNestedAssociate]
FROM [RootEntity] AS [r]
CROSS APPLY OPENJSON([r].[AssociateCollection], '$') WITH (
    [Id] int '$.Id',
    [Int] int '$.Int',
    [Ints] nvarchar(max) '$.Ints' AS JSON,
    [Name] nvarchar(max) '$.Name',
    [String] nvarchar(max) '$.String',
    [NestedCollection] nvarchar(max) '$.NestedCollection' AS JSON,
    [OptionalNestedAssociate] nvarchar(max) '$.OptionalNestedAssociate' AS JSON,
    [RequiredNestedAssociate] nvarchar(max) '$.RequiredNestedAssociate' AS JSON
) AS [a]
""");
        }
    }

    public override async Task SelectMany_nested_collection_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_required_associate(queryTrackingBehavior);

        if (Fixture.UsingJsonType && !SqlServerTestEnvironment.IsAzureSql)
        {
            AssertSql(
                """
SELECT [n].[Id], [n].[Int], [n].[Ints], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
CROSS APPLY OPENJSON([r].[RequiredAssociate], '$.NestedCollection') WITH (
    [Id] int '$.Id',
    [Int] int '$.Int',
    [Ints] json '$.Ints' AS JSON,
    [Name] nvarchar(max) '$.Name',
    [String] nvarchar(max) '$.String'
) AS [n]
""");
        }
        else
        {
            AssertSql(
                """
SELECT [n].[Id], [n].[Int], [n].[Ints], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
CROSS APPLY OPENJSON([r].[RequiredAssociate], '$.NestedCollection') WITH (
    [Id] int '$.Id',
    [Int] int '$.Int',
    [Ints] nvarchar(max) '$.Ints' AS JSON,
    [Name] nvarchar(max) '$.Name',
    [String] nvarchar(max) '$.String'
) AS [n]
""");
        }
    }

    public override async Task SelectMany_nested_collection_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_optional_associate(queryTrackingBehavior);

        if (Fixture.UsingJsonType && !SqlServerTestEnvironment.IsAzureSql)
        {
            AssertSql(
                """
SELECT [n].[Id], [n].[Int], [n].[Ints], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
CROSS APPLY OPENJSON([r].[OptionalAssociate], '$.NestedCollection') WITH (
    [Id] int '$.Id',
    [Int] int '$.Int',
    [Ints] json '$.Ints' AS JSON,
    [Name] nvarchar(max) '$.Name',
    [String] nvarchar(max) '$.String'
) AS [n]
""");
        }
        else
        {
            AssertSql(
                """
SELECT [n].[Id], [n].[Int], [n].[Ints], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
CROSS APPLY OPENJSON([r].[OptionalAssociate], '$.NestedCollection') WITH (
    [Id] int '$.Id',
    [Int] int '$.Int',
    [Ints] nvarchar(max) '$.Ints' AS JSON,
    [Name] nvarchar(max) '$.Name',
    [String] nvarchar(max) '$.String'
) AS [n]
""");
        }
    }

    #endregion Structural collection properties

    #region Multiple

    public override async Task Select_root_duplicated(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root_duplicated(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate], [r].[AssociateCollection], [r].[OptionalAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_required_associate_duplicated(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_associate_duplicated(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredAssociate], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_required_associate_and_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_associate_and_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredAssociate], [r].[OptionalAssociate]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_associate_and_ints(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_associate_and_ints(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalAssociate], JSON_QUERY([r].[RequiredAssociate], '$.Ints') AS [Ints]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_associate_and_target_to_index_based_binding_via_closure(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate_and_target_to_index_based_binding_via_closure(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[RequiredAssociate]
FROM [RootEntity] AS [r]
""");
    }

    #endregion Multiple

    #region Subquery

    public override async Task Select_subquery_FirstOrDefault_complex_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_FirstOrDefault_complex_collection(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r1].[c], [r1].[c0]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT TOP(1) [r0].[AssociateCollection] AS [c], 1 AS [c0]
    FROM [RootEntity] AS [r0]
    ORDER BY [r0].[Id]
) AS [r1]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_subquery_required_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_required_related_FirstOrDefault(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r1].[c]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT TOP(1) JSON_QUERY([r0].[RequiredAssociate], '$.RequiredNestedAssociate') AS [c]
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
    SELECT TOP(1) JSON_QUERY([r0].[OptionalAssociate], '$.RequiredNestedAssociate') AS [c]
    FROM [RootEntity] AS [r0]
    ORDER BY [r0].[Id]
) AS [r1]
""");
    }

    public override async Task Select_associate_on_GroupBy_First(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate_on_GroupBy_First(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r5].[Id], [r5].[Name], [r5].[c], [r5].[c0], [r5].[c1]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r3]
LEFT JOIN (
    SELECT [r4].[Id], [r4].[Name], [r4].[c] AS [c], [r4].[c0] AS [c0], [r4].[c1] AS [c1], [r4].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[c] AS [c], [r1].[c0] AS [c0], [r1].[c1] AS [c1], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id]) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[AssociateCollection] AS [c], [r2].[OptionalAssociate] AS [c0], [r2].[RequiredAssociate] AS [c1], [r2].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
        WHERE [r1].[Id] > 0
    ) AS [r4]
    WHERE [r4].[row] <= 1
) AS [r5] ON [r3].[Key] = [r5].[Key] OR ([r3].[Key] IS NULL AND [r5].[Key] IS NULL)
""");
    }

    public override async Task Select_nested_associate_scalar_property_on_GroupBy_First(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_associate_scalar_property_on_GroupBy_First(queryTrackingBehavior);

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT (
    SELECT TOP(1) JSON_VALUE([r1].[c1], '$.RequiredNestedAssociate.Int' RETURNING int)
    FROM (
        SELECT [r2].[Id], [r2].[RequiredAssociate] AS [c1], [r2].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r2]
    ) AS [r1]
    WHERE ([r0].[Key] = [r1].[Key] OR ([r0].[Key] IS NULL AND [r1].[Key] IS NULL)) AND [r1].[Id] > 0
    ORDER BY [r1].[Id])
FROM (
    SELECT [r].[Id] % 2 AS [Key]
    FROM [RootEntity] AS [r]
) AS [r0]
GROUP BY [r0].[Key]
""");
        }
        else
        {
            AssertSql(
                """
SELECT (
    SELECT TOP(1) CAST(JSON_VALUE([r1].[c1], '$.RequiredNestedAssociate.Int') AS int)
    FROM (
        SELECT [r2].[Id], [r2].[RequiredAssociate] AS [c1], [r2].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r2]
    ) AS [r1]
    WHERE ([r0].[Key] = [r1].[Key] OR ([r0].[Key] IS NULL AND [r1].[Key] IS NULL)) AND [r1].[Id] > 0
    ORDER BY [r1].[Id])
FROM (
    SELECT [r].[Id] % 2 AS [Key]
    FROM [RootEntity] AS [r]
) AS [r0]
GROUP BY [r0].[Key]
""");
        }
    }

    public override async Task Select_optional_associate_scalar_property_on_GroupBy_First(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_associate_scalar_property_on_GroupBy_First(queryTrackingBehavior);

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT (
    SELECT TOP(1) JSON_VALUE([r0].[OptionalAssociate], '$.Int' RETURNING int)
    FROM [RootEntity] AS [r0]
    WHERE [r].[Id] = [r0].[Id] AND [r0].[Id] > 0
    ORDER BY [r0].[Id])
FROM [RootEntity] AS [r]
GROUP BY [r].[Id]
""");
        }
        else
        {
            AssertSql(
                """
SELECT (
    SELECT TOP(1) CAST(JSON_VALUE([r0].[OptionalAssociate], '$.Int') AS int)
    FROM [RootEntity] AS [r0]
    WHERE [r].[Id] = [r0].[Id] AND [r0].[Id] > 0
    ORDER BY [r0].[Id])
FROM [RootEntity] AS [r]
GROUP BY [r].[Id]
""");
        }
    }

    public override async Task Select_optional_associate_nested_scalar_property_on_GroupBy_First(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_associate_nested_scalar_property_on_GroupBy_First(queryTrackingBehavior);

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT (
    SELECT TOP(1) JSON_VALUE([r0].[OptionalAssociate], '$.RequiredNestedAssociate.Int' RETURNING int)
    FROM [RootEntity] AS [r0]
    WHERE [r].[Id] = [r0].[Id] AND [r0].[Id] > 0
    ORDER BY [r0].[Id])
FROM [RootEntity] AS [r]
GROUP BY [r].[Id]
""");
        }
        else
        {
            AssertSql(
                """
SELECT (
    SELECT TOP(1) CAST(JSON_VALUE([r0].[OptionalAssociate], '$.RequiredNestedAssociate.Int') AS int)
    FROM [RootEntity] AS [r0]
    WHERE [r].[Id] = [r0].[Id] AND [r0].[Id] > 0
    ORDER BY [r0].[Id])
FROM [RootEntity] AS [r]
GROUP BY [r].[Id]
""");
        }
    }

    public override async Task Select_associate_collection_on_GroupBy_First(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate_collection_on_GroupBy_First(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r5].[Id], [r5].[Name], [r5].[c], [r5].[c0], [r5].[c1]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r3]
LEFT JOIN (
    SELECT [r4].[Id], [r4].[Name], [r4].[c] AS [c], [r4].[c0] AS [c0], [r4].[c1] AS [c1], [r4].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[c] AS [c], [r1].[c0] AS [c0], [r1].[c1] AS [c1], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id]) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[AssociateCollection] AS [c], [r2].[OptionalAssociate] AS [c0], [r2].[RequiredAssociate] AS [c1], [r2].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
        WHERE [r1].[Id] > 0
    ) AS [r4]
    WHERE [r4].[row] <= 1
) AS [r5] ON [r3].[Key] = [r5].[Key] OR ([r3].[Key] IS NULL AND [r5].[Key] IS NULL)
""");
    }

    public override async Task Select_associate_collection_Count_on_GroupBy_First()
    {
        await base.Select_associate_collection_Count_on_GroupBy_First();

        AssertSql(
            """
SELECT [r5].[Id], [r5].[Name], [r5].[c], [r5].[c0], [r5].[c1]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r3]
LEFT JOIN (
    SELECT [r4].[Id], [r4].[Name], [r4].[c] AS [c], [r4].[c0] AS [c0], [r4].[c1] AS [c1], [r4].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[c] AS [c], [r1].[c0] AS [c0], [r1].[c1] AS [c1], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id]) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[AssociateCollection] AS [c], [r2].[OptionalAssociate] AS [c0], [r2].[RequiredAssociate] AS [c1], [r2].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
        WHERE [r1].[Id] > 0
    ) AS [r4]
    WHERE [r4].[row] <= 1
) AS [r5] ON [r3].[Key] = [r5].[Key] OR ([r3].[Key] IS NULL AND [r5].[Key] IS NULL)
""");
    }

    #endregion Subquery

    #region Value types

    public override async Task Select_root_with_value_types(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root_with_value_types(queryTrackingBehavior);

        AssertSql(
            """
SELECT [v].[Id], [v].[Name], [v].[AssociateCollection], [v].[OptionalAssociate], [v].[RequiredAssociate]
FROM [ValueRootEntity] AS [v]
""");
    }

    public override async Task Select_non_nullable_value_type(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_non_nullable_value_type(queryTrackingBehavior);

        AssertSql(
            """
SELECT [v].[RequiredAssociate]
FROM [ValueRootEntity] AS [v]
ORDER BY [v].[Id]
""");
    }

    public override async Task Select_nullable_value_type(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type(queryTrackingBehavior);

        AssertSql(
            """
SELECT [v].[OptionalAssociate]
FROM [ValueRootEntity] AS [v]
ORDER BY [v].[Id]
""");
    }

    public override async Task Select_nullable_value_type_with_Value(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type_with_Value(queryTrackingBehavior);

        AssertSql(
            """
SELECT [v].[OptionalAssociate]
FROM [ValueRootEntity] AS [v]
ORDER BY [v].[Id]
""");
    }

    #endregion Value types

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
