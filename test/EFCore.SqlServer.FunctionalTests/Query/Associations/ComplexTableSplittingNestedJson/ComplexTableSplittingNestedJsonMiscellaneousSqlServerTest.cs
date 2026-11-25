// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplittingNestedJson;

public class ComplexTableSplittingNestedJsonMiscellaneousSqlServerTest(ComplexTableSplittingNestedJsonSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexTableSplittingNestedJsonMiscellaneousRelationalTestBase<ComplexTableSplittingNestedJsonSqlServerFixture>(fixture, testOutputHelper)
{
    #region Simple filters

    public override async Task Where_on_associate_scalar_property()
    {
        await base.Where_on_associate_scalar_property();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredAssociate_Int] = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredAssociate_Int] = 8
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
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate]
FROM [RootEntity] AS [r]
WHERE [r].[OptionalAssociate_Int] = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate]
FROM [RootEntity] AS [r]
WHERE [r].[OptionalAssociate_Int] = 8
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
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate]
FROM [RootEntity] AS [r]
WHERE JSON_VALUE([r].[RequiredAssociate_RequiredNestedAssociate], '$.Int' RETURNING int) = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[RequiredAssociate_RequiredNestedAssociate], '$.Int') AS int) = 8
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
SELECT [v].[Id], [v].[Name], [v].[AssociateCollection], [v].[OptionalAssociate_Id], [v].[OptionalAssociate_Int], [v].[OptionalAssociate_Name], [v].[OptionalAssociate_String], [v].[OptionalAssociate_NestedCollection], [v].[OptionalAssociate_OptionalNested], [v].[OptionalAssociate_RequiredNested], [v].[RequiredAssociate_Id], [v].[RequiredAssociate_Int], [v].[RequiredAssociate_Name], [v].[RequiredAssociate_String], [v].[RequiredAssociate_NestedCollection], [v].[RequiredAssociate_OptionalNested], [v].[RequiredAssociate_RequiredNested]
FROM [ValueRootEntity] AS [v]
WHERE [v].[RequiredAssociate_Int] = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [v].[Id], [v].[Name], [v].[AssociateCollection], [v].[OptionalAssociate_Id], [v].[OptionalAssociate_Int], [v].[OptionalAssociate_Name], [v].[OptionalAssociate_String], [v].[OptionalAssociate_NestedCollection], [v].[OptionalAssociate_OptionalNested], [v].[OptionalAssociate_RequiredNested], [v].[RequiredAssociate_Id], [v].[RequiredAssociate_Int], [v].[RequiredAssociate_Name], [v].[RequiredAssociate_String], [v].[RequiredAssociate_NestedCollection], [v].[RequiredAssociate_OptionalNested], [v].[RequiredAssociate_RequiredNested]
FROM [ValueRootEntity] AS [v]
WHERE [v].[RequiredAssociate_Int] = 8
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
SELECT [v].[Id], [v].[Name], [v].[AssociateCollection], [v].[OptionalAssociate_Id], [v].[OptionalAssociate_Int], [v].[OptionalAssociate_Name], [v].[OptionalAssociate_String], [v].[OptionalAssociate_NestedCollection], [v].[OptionalAssociate_OptionalNested], [v].[OptionalAssociate_RequiredNested], [v].[RequiredAssociate_Id], [v].[RequiredAssociate_Int], [v].[RequiredAssociate_Name], [v].[RequiredAssociate_String], [v].[RequiredAssociate_NestedCollection], [v].[RequiredAssociate_OptionalNested], [v].[RequiredAssociate_RequiredNested]
FROM [ValueRootEntity] AS [v]
WHERE [v].[OptionalAssociate_Int] = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [v].[Id], [v].[Name], [v].[AssociateCollection], [v].[OptionalAssociate_Id], [v].[OptionalAssociate_Int], [v].[OptionalAssociate_Name], [v].[OptionalAssociate_String], [v].[OptionalAssociate_NestedCollection], [v].[OptionalAssociate_OptionalNested], [v].[OptionalAssociate_RequiredNested], [v].[RequiredAssociate_Id], [v].[RequiredAssociate_Int], [v].[RequiredAssociate_Name], [v].[RequiredAssociate_String], [v].[RequiredAssociate_NestedCollection], [v].[RequiredAssociate_OptionalNested], [v].[RequiredAssociate_RequiredNested]
FROM [ValueRootEntity] AS [v]
WHERE [v].[OptionalAssociate_Int] = 8
""");
        }
    }

    public override async Task Where_HasValue_on_nullable_value_type()
    {
        await base.Where_HasValue_on_nullable_value_type();

        AssertSql(
            """
SELECT [v].[Id], [v].[Name], [v].[AssociateCollection], [v].[OptionalAssociate_Id], [v].[OptionalAssociate_Int], [v].[OptionalAssociate_Name], [v].[OptionalAssociate_String], [v].[OptionalAssociate_NestedCollection], [v].[OptionalAssociate_OptionalNested], [v].[OptionalAssociate_RequiredNested], [v].[RequiredAssociate_Id], [v].[RequiredAssociate_Int], [v].[RequiredAssociate_Name], [v].[RequiredAssociate_String], [v].[RequiredAssociate_NestedCollection], [v].[RequiredAssociate_OptionalNested], [v].[RequiredAssociate_RequiredNested]
FROM [ValueRootEntity] AS [v]
WHERE [v].[OptionalAssociate_Id] IS NOT NULL
""");
    }

    #endregion Value types

    public override async Task FromSql_on_root()
    {
        await base.FromSql_on_root();

        AssertSql(
            """
SELECT [m].[Id], [m].[Name], [m].[AssociateCollection], [m].[OptionalAssociate_Id], [m].[OptionalAssociate_Int], [m].[OptionalAssociate_Ints], [m].[OptionalAssociate_Name], [m].[OptionalAssociate_String], [m].[OptionalAssociate_NestedCollection], [m].[OptionalAssociate_OptionalNestedAssociate], [m].[OptionalAssociate_RequiredNestedAssociate], [m].[RequiredAssociate_Id], [m].[RequiredAssociate_Int], [m].[RequiredAssociate_Ints], [m].[RequiredAssociate_Name], [m].[RequiredAssociate_String], [m].[RequiredAssociate_NestedCollection], [m].[RequiredAssociate_OptionalNestedAssociate], [m].[RequiredAssociate_RequiredNestedAssociate]
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
SELECT [m].[Id], [m].[Name], [m].[AssociateCollection], [m].[OptionalAssociate_Id], [m].[OptionalAssociate_Int], [m].[OptionalAssociate_Ints], [m].[OptionalAssociate_Name], [m].[OptionalAssociate_String], [m].[OptionalAssociate_NestedCollection], [m].[OptionalAssociate_OptionalNestedAssociate], [m].[OptionalAssociate_RequiredNestedAssociate], [m].[RequiredAssociate_Id], [m].[RequiredAssociate_Int], [m].[RequiredAssociate_Ints], [m].[RequiredAssociate_Name], [m].[RequiredAssociate_String], [m].[RequiredAssociate_NestedCollection], [m].[RequiredAssociate_OptionalNestedAssociate], [m].[RequiredAssociate_RequiredNestedAssociate]
FROM (
    SELECT * FROM "RootEntity"
) AS [m]
WHERE [m].[RequiredAssociate_Int] = 8
""");
        }
        else
        {
            AssertSql(
                """
SELECT [m].[Id], [m].[Name], [m].[AssociateCollection], [m].[OptionalAssociate_Id], [m].[OptionalAssociate_Int], [m].[OptionalAssociate_Ints], [m].[OptionalAssociate_Name], [m].[OptionalAssociate_String], [m].[OptionalAssociate_NestedCollection], [m].[OptionalAssociate_OptionalNestedAssociate], [m].[OptionalAssociate_RequiredNestedAssociate], [m].[RequiredAssociate_Id], [m].[RequiredAssociate_Int], [m].[RequiredAssociate_Ints], [m].[RequiredAssociate_Name], [m].[RequiredAssociate_String], [m].[RequiredAssociate_NestedCollection], [m].[RequiredAssociate_OptionalNestedAssociate], [m].[RequiredAssociate_RequiredNestedAssociate]
FROM (
    SELECT * FROM "RootEntity"
) AS [m]
WHERE [m].[RequiredAssociate_Int] = 8
""");
        }
    }

    public override async Task Two_associates_equality()
    {
        await base.Two_associates_equality();

        if (Fixture.UsingJsonType)
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredAssociate_Id] = [r].[OptionalAssociate_Id] AND [r].[RequiredAssociate_Int] = [r].[OptionalAssociate_Int] AND CAST([r].[RequiredAssociate_Ints] AS nvarchar(max)) = CAST([r].[OptionalAssociate_Ints] AS nvarchar(max)) AND [r].[RequiredAssociate_Name] = [r].[OptionalAssociate_Name] AND [r].[RequiredAssociate_String] = [r].[OptionalAssociate_String] AND CAST([r].[RequiredAssociate_NestedCollection] AS nvarchar(max)) = CAST([r].[OptionalAssociate_NestedCollection] AS nvarchar(max)) AND (CAST([r].[RequiredAssociate_OptionalNestedAssociate] AS nvarchar(max)) = CAST([r].[OptionalAssociate_OptionalNestedAssociate] AS nvarchar(max)) OR ([r].[RequiredAssociate_OptionalNestedAssociate] IS NULL AND [r].[OptionalAssociate_OptionalNestedAssociate] IS NULL)) AND CAST([r].[RequiredAssociate_RequiredNestedAssociate] AS nvarchar(max)) = CAST([r].[OptionalAssociate_RequiredNestedAssociate] AS nvarchar(max))
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id], [r].[Name], [r].[AssociateCollection], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredAssociate_Id] = [r].[OptionalAssociate_Id] AND [r].[RequiredAssociate_Int] = [r].[OptionalAssociate_Int] AND [r].[RequiredAssociate_Ints] = [r].[OptionalAssociate_Ints] AND [r].[RequiredAssociate_Name] = [r].[OptionalAssociate_Name] AND [r].[RequiredAssociate_String] = [r].[OptionalAssociate_String] AND [r].[RequiredAssociate_NestedCollection] = [r].[OptionalAssociate_NestedCollection] AND ([r].[RequiredAssociate_OptionalNestedAssociate] = [r].[OptionalAssociate_OptionalNestedAssociate] OR ([r].[RequiredAssociate_OptionalNestedAssociate] IS NULL AND [r].[OptionalAssociate_OptionalNestedAssociate] IS NULL)) AND [r].[RequiredAssociate_RequiredNestedAssociate] = [r].[OptionalAssociate_RequiredNestedAssociate]
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
WHERE [r0].[RequiredAssociate_Int] <> 8 OR [r0].[RequiredAssociate_Int] IS NULL
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity] AS [r0] ON [r].[Id] + 1 = [r0].[Id]
WHERE [r0].[RequiredAssociate_Int] <> 8 OR [r0].[RequiredAssociate_Int] IS NULL
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
WHERE JSON_VALUE([r0].[RequiredAssociate_RequiredNestedAssociate], '$.Int' RETURNING int) <> 8 OR JSON_VALUE([r0].[RequiredAssociate_RequiredNestedAssociate], '$.Int' RETURNING int) IS NULL
""");
        }
        else
        {
            AssertSql(
                """
SELECT [r].[Id]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity] AS [r0] ON [r].[Id] + 1 = [r0].[Id]
WHERE CAST(JSON_VALUE([r0].[RequiredAssociate_RequiredNestedAssociate], '$.Int') AS int) <> 8 OR CAST(JSON_VALUE([r0].[RequiredAssociate_RequiredNestedAssociate], '$.Int') AS int) IS NULL
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
    SELECT TOP(@p) [r0].[Id], [r0].[RequiredAssociate_Int]
    FROM [RootEntity] AS [r0]
    ORDER BY [r0].[Id]
) AS [r1] ON [r].[Id] + 1 = [r1].[Id]
WHERE [r1].[RequiredAssociate_Int] <> 8 OR [r1].[RequiredAssociate_Int] IS NULL
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
    SELECT TOP(@p) [r0].[Id], [r0].[RequiredAssociate_Int]
    FROM [RootEntity] AS [r0]
    ORDER BY [r0].[Id]
) AS [r1] ON [r].[Id] + 1 = [r1].[Id]
WHERE [r1].[RequiredAssociate_Int] <> 8 OR [r1].[RequiredAssociate_Int] IS NULL
""");
        }
    }

    #endregion Left join

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
