// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplittingNestedJson;

public class ComplexTableSplittingNestedJsonProjectionSqlServerTest(ComplexTableSplittingNestedJsonSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexTableSplittingNestedJsonProjectionRelationalTestBase<ComplexTableSplittingNestedJsonSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Select_complex_properties_with_identity_resolution(bool async)
    {
        await base.Select_complex_properties_with_identity_resolution(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_complex_properties_with_identity_resolution_member_init(bool async)
    {
        await base.Select_complex_properties_with_identity_resolution_member_init(async);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_root(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate]
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
SELECT [r].[RequiredAssociate_String]
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[RequiredAssociate_String]
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
SELECT [r].[OptionalAssociate_String]
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[OptionalAssociate_String]
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
SELECT [r].[OptionalAssociate_Int]
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[OptionalAssociate_Int]
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
SELECT [r].[OptionalAssociate_Int]
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[OptionalAssociate_Int]
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
SELECT [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_required_nested_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredAssociate_RequiredNestedAssociate]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_nested_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredAssociate_OptionalNestedAssociate]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_required_nested_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalAssociate_RequiredNestedAssociate]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_nested_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalAssociate_OptionalNestedAssociate]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_required_associate_via_optional_navigation(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_associate_via_optional_navigation(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r0].[RequiredAssociate_Id], [r0].[RequiredAssociate_Int], [r0].[RequiredAssociate_Ints], [r0].[RequiredAssociate_Name], [r0].[RequiredAssociate_String], [r0].[RequiredAssociate_NestedCollection], [r0].[RequiredAssociate_OptionalNestedAssociate], [r0].[RequiredAssociate_RequiredNestedAssociate]
FROM [RootReferencingEntity] AS [r]
LEFT JOIN [RootEntity] AS [r0] ON [r].[RootEntityId] = [r0].[Id]
""");
    }

    public override async Task Select_unmapped_associate_scalar_property(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_unmapped_associate_scalar_property(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate]
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
SELECT [r].[RequiredAssociate_Int]
FROM [RootEntity] AS [r]
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[RequiredAssociate_Int]
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
SELECT [r].[RequiredAssociate_NestedCollection]
FROM [RootEntity] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_nested_collection_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalAssociate_NestedCollection]
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
SELECT [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String]
FROM [RootEntity] AS [r]
CROSS APPLY OPENJSON([r].[RequiredAssociate_NestedCollection], '$') WITH (
    [Id] int '$.Id',
    [Int] int '$.Int',
    [Ints] json '$.Ints' AS JSON,
    [Name] nvarchar(max) '$.Name',
    [String] nvarchar(max) '$.String'
) AS [r0]
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String]
FROM [RootEntity] AS [r]
CROSS APPLY OPENJSON([r].[RequiredAssociate_NestedCollection], '$') WITH (
    [Id] int '$.Id',
    [Int] int '$.Int',
    [Ints] nvarchar(max) '$.Ints' AS JSON,
    [Name] nvarchar(max) '$.Name',
    [String] nvarchar(max) '$.String'
) AS [r0]
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
SELECT [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String]
FROM [RootEntity] AS [r]
CROSS APPLY OPENJSON([r].[OptionalAssociate_NestedCollection], '$') WITH (
    [Id] int '$.Id',
    [Int] int '$.Int',
    [Ints] json '$.Ints' AS JSON,
    [Name] nvarchar(max) '$.Name',
    [String] nvarchar(max) '$.String'
) AS [o]
""");
        }
        else
        {
            AssertSql(
                """
SELECT [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String]
FROM [RootEntity] AS [r]
CROSS APPLY OPENJSON([r].[OptionalAssociate_NestedCollection], '$') WITH (
    [Id] int '$.Id',
    [Int] int '$.Int',
    [Ints] nvarchar(max) '$.Ints' AS JSON,
    [Name] nvarchar(max) '$.Name',
    [String] nvarchar(max) '$.String'
) AS [o]
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
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate], [r].[AssociateCollection], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_required_associate_duplicated(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_associate_duplicated(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_required_associate_and_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_associate_and_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_associate_and_ints(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_associate_and_ints(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate], [r].[RequiredAssociate_Ints] AS [Ints]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_associate_and_target_to_index_based_binding_via_closure(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate_and_target_to_index_based_binding_via_closure(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate]
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
    SELECT TOP(1) [r0].[RequiredAssociate_RequiredNestedAssociate] AS [c]
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
    SELECT TOP(1) [r0].[OptionalAssociate_RequiredNestedAssociate] AS [c]
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
SELECT [v].[Id], [v].[Name], [v].[AssociateCollection], [v].[OptionalAssociate_Id], [v].[OptionalAssociate_Int], [v].[OptionalAssociate_Name], [v].[OptionalAssociate_String], [v].[OptionalAssociate_NestedCollection], [v].[OptionalAssociate_OptionalNested], [v].[OptionalAssociate_RequiredNested], [v].[RequiredAssociate_Id], [v].[RequiredAssociate_Int], [v].[RequiredAssociate_Name], [v].[RequiredAssociate_String], [v].[RequiredAssociate_NestedCollection], [v].[RequiredAssociate_OptionalNested], [v].[RequiredAssociate_RequiredNested]
FROM [ValueRootEntity] AS [v]
""");
    }

    public override async Task Select_non_nullable_value_type(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_non_nullable_value_type(queryTrackingBehavior);

        AssertSql(
            """
SELECT [v].[RequiredAssociate_Id], [v].[RequiredAssociate_Int], [v].[RequiredAssociate_Name], [v].[RequiredAssociate_String], [v].[RequiredAssociate_NestedCollection], [v].[RequiredAssociate_OptionalNested], [v].[RequiredAssociate_RequiredNested]
FROM [ValueRootEntity] AS [v]
ORDER BY [v].[Id]
""");
    }

    public override async Task Select_nullable_value_type(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type(queryTrackingBehavior);

        AssertSql(
            """
SELECT [v].[OptionalAssociate_Id], [v].[OptionalAssociate_Int], [v].[OptionalAssociate_Name], [v].[OptionalAssociate_String], [v].[OptionalAssociate_NestedCollection], [v].[OptionalAssociate_OptionalNested], [v].[OptionalAssociate_RequiredNested]
FROM [ValueRootEntity] AS [v]
ORDER BY [v].[Id]
""");
    }

    public override async Task Select_nullable_value_type_with_Value(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type_with_Value(queryTrackingBehavior);

        AssertSql(
            """
SELECT [v].[OptionalAssociate_Id], [v].[OptionalAssociate_Int], [v].[OptionalAssociate_Name], [v].[OptionalAssociate_String], [v].[OptionalAssociate_NestedCollection], [v].[OptionalAssociate_OptionalNested], [v].[OptionalAssociate_RequiredNested]
FROM [ValueRootEntity] AS [v]
ORDER BY [v].[Id]
""");
    }

    #endregion Value types

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
