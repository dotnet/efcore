// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public class ComplexJsonProjectionSqlServerTest(ComplexJsonSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonProjectionRelationalTestBase<ComplexJsonSqlServerFixture>(fixture, testOutputHelper)
{
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

        if (Fixture.UsingJsonType)
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

        if (Fixture.UsingJsonType)
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

        if (Fixture.UsingJsonType)
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

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
