// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedComplexProperties;

public class OwnedComplexPropertiesStructuralEqualitySqlServerTest(OwnedComplexPropertiesSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : OwnedComplexPropertiesStructuralEqualityRelationalTestBase<OwnedComplexPropertiesSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Two_associates()
    {
        await base.Two_associates();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[RootEntityId], [r0].[String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String], [r0].[NestedCollection], [r0].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
WHERE 0 = 1
ORDER BY [r].[Id]
""");
    }

    public override async Task Two_nested_associates()
    {
        await base.Two_nested_associates();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[RootEntityId], [r0].[String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String], [r0].[NestedCollection], [r0].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
WHERE [r].[RequiredAssociate_RequiredNestedAssociate_Id] = [r].[OptionalAssociate_RequiredNestedAssociate_Id] AND [r].[RequiredAssociate_RequiredNestedAssociate_Int] = [r].[OptionalAssociate_RequiredNestedAssociate_Int] AND [r].[RequiredAssociate_RequiredNestedAssociate_Ints] = [r].[OptionalAssociate_RequiredNestedAssociate_Ints] AND [r].[RequiredAssociate_RequiredNestedAssociate_Name] = [r].[OptionalAssociate_RequiredNestedAssociate_Name] AND [r].[RequiredAssociate_RequiredNestedAssociate_String] = [r].[OptionalAssociate_RequiredNestedAssociate_String]
ORDER BY [r].[Id]
""");
    }

    public override async Task Not_equals()
    {
        await base.Not_equals();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[RootEntityId], [r0].[String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String], [r0].[NestedCollection], [r0].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
WHERE 0 = 1
ORDER BY [r].[Id]
""");
    }

    public override async Task Associate_with_inline_null()
    {
        await base.Associate_with_inline_null();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[RootEntityId], [r0].[String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String], [r0].[NestedCollection], [r0].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
WHERE [r].[OptionalAssociate_Id] IS NULL OR [r].[OptionalAssociate_Int] IS NULL OR [r].[OptionalAssociate_Ints] IS NULL OR [r].[OptionalAssociate_Name] IS NULL OR [r].[OptionalAssociate_String] IS NULL OR [r].[OptionalAssociate_NestedCollection] IS NULL
ORDER BY [r].[Id]
""");
    }

    public override async Task Associate_with_parameter_null()
    {
        await base.Associate_with_parameter_null();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[RootEntityId], [r0].[String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String], [r0].[NestedCollection], [r0].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
WHERE CASE
    WHEN [r].[OptionalAssociate_Id] IS NOT NULL AND [r].[OptionalAssociate_Int] IS NOT NULL AND [r].[OptionalAssociate_Ints] IS NOT NULL AND [r].[OptionalAssociate_Name] IS NOT NULL AND [r].[OptionalAssociate_String] IS NOT NULL AND [r].[OptionalAssociate_NestedCollection] IS NOT NULL THEN [r].[Id]
END IS NULL
ORDER BY [r].[Id]
""");
    }

    public override async Task Nested_associate_with_inline_null()
    {
        await base.Nested_associate_with_inline_null();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[RootEntityId], [r0].[String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String], [r0].[NestedCollection], [r0].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
WHERE [r].[RequiredAssociate_OptionalNestedAssociate] IS NULL
ORDER BY [r].[Id]
""");
    }

    public override async Task Nested_associate_with_inline()
    {
        await base.Nested_associate_with_inline();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[RootEntityId], [r0].[String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String], [r0].[NestedCollection], [r0].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
WHERE [r].[RequiredAssociate_RequiredNestedAssociate_Id] = 1000 AND [r].[RequiredAssociate_RequiredNestedAssociate_Int] = 8 AND [r].[RequiredAssociate_RequiredNestedAssociate_Ints] = N'[1,2,3]' AND [r].[RequiredAssociate_RequiredNestedAssociate_Name] = N'Root1_RequiredAssociate_RequiredNestedAssociate' AND [r].[RequiredAssociate_RequiredNestedAssociate_String] = N'foo'
ORDER BY [r].[Id]
""");
    }

    public override async Task Nested_associate_with_parameter()
    {
        await base.Nested_associate_with_parameter();

        AssertSql(
            """
@entity_equality_nested_Id='1000' (Nullable = true)
@entity_equality_nested_Int='8' (Nullable = true)
@entity_equality_nested_Ints='[1,2,3]' (Size = 4000)
@entity_equality_nested_Name='Root1_RequiredAssociate_RequiredNestedAssociate' (Size = 4000)
@entity_equality_nested_String='foo' (Size = 4000)

SELECT [r].[Id], [r].[Name], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[RootEntityId], [r0].[String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String], [r0].[NestedCollection], [r0].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
WHERE [r].[RequiredAssociate_RequiredNestedAssociate_Id] = @entity_equality_nested_Id AND [r].[RequiredAssociate_RequiredNestedAssociate_Int] = @entity_equality_nested_Int AND [r].[RequiredAssociate_RequiredNestedAssociate_Ints] = @entity_equality_nested_Ints AND [r].[RequiredAssociate_RequiredNestedAssociate_Name] = @entity_equality_nested_Name AND [r].[RequiredAssociate_RequiredNestedAssociate_String] = @entity_equality_nested_String
ORDER BY [r].[Id]
""");
    }

    public override async Task Two_nested_collections()
    {
        await base.Two_nested_collections();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[RootEntityId], [r0].[String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String], [r0].[NestedCollection], [r0].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
WHERE [r].[RequiredAssociate_NestedCollection] = [r].[OptionalAssociate_NestedCollection]
ORDER BY [r].[Id]
""");
    }

    public override async Task Nested_collection_with_inline()
    {
        await base.Nested_collection_with_inline();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[RootEntityId], [r0].[String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String], [r0].[NestedCollection], [r0].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
WHERE [r].[RequiredAssociate_NestedCollection] = '[{"Id":1002,"Int":8,"Ints":[1,2,3],"Name":"Root1_RequiredAssociate_NestedCollection_1","String":"foo"},{"Id":1003,"Int":8,"Ints":[1,2,3],"Name":"Root1_RequiredAssociate_NestedCollection_2","String":"foo"}]'
ORDER BY [r].[Id]
""");
    }

    public override async Task Nested_collection_with_parameter()
    {
        await base.Nested_collection_with_parameter();

        AssertSql(
            """
@entity_equality_nestedCollection='[{"Id":1002,"Int":8,"Ints":[1,2,3],"Name":"Root1_RequiredAssociate_NestedCollection_1","String":"foo"},{"Id":1003,"Int":8,"Ints":[1,2,3],"Name":"Root1_RequiredAssociate_NestedCollection_2","String":"foo"}]' (Size = 205)

SELECT [r].[Id], [r].[Name], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[RootEntityId], [r0].[String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String], [r0].[NestedCollection], [r0].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
WHERE [r].[RequiredAssociate_NestedCollection] = @entity_equality_nestedCollection
ORDER BY [r].[Id]
""");
    }

    public override async Task Contains_with_inline()
    {
        await base.Contains_with_inline();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[RootEntityId], [r1].[String], [r1].[RequiredNestedAssociate_Id], [r1].[RequiredNestedAssociate_Int], [r1].[RequiredNestedAssociate_Ints], [r1].[RequiredNestedAssociate_Name], [r1].[RequiredNestedAssociate_String], [r1].[NestedCollection], [r1].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r1] ON [r].[Id] = [r1].[RootEntityId]
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON([r].[RequiredAssociate_NestedCollection], '$') WITH (
        [Id] int '$.Id',
        [Int] int '$.Int',
        [Ints] nvarchar(max) '$.Ints' AS JSON,
        [Name] nvarchar(max) '$.Name',
        [String] nvarchar(max) '$.String'
    ) AS [r0]
    WHERE [r0].[Id] = 1002 AND [r0].[Int] = 8 AND [r0].[Ints] = N'[1,2,3]' AND [r0].[Name] = N'Root1_RequiredAssociate_NestedCollection_1' AND [r0].[String] = N'foo')
ORDER BY [r].[Id]
""");
    }

    public override async Task Contains_with_parameter()
    {
        await base.Contains_with_parameter();

        AssertSql(
            """
@entity_equality_nested_Id='1002' (Nullable = true)
@entity_equality_nested_Int='8' (Nullable = true)
@entity_equality_nested_Ints='[1,2,3]' (Size = 4000)
@entity_equality_nested_Name='Root1_RequiredAssociate_NestedCollection_1' (Size = 4000)
@entity_equality_nested_String='foo' (Size = 4000)

SELECT [r].[Id], [r].[Name], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[RootEntityId], [r1].[String], [r1].[RequiredNestedAssociate_Id], [r1].[RequiredNestedAssociate_Int], [r1].[RequiredNestedAssociate_Ints], [r1].[RequiredNestedAssociate_Name], [r1].[RequiredNestedAssociate_String], [r1].[NestedCollection], [r1].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r1] ON [r].[Id] = [r1].[RootEntityId]
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON([r].[RequiredAssociate_NestedCollection], '$') WITH (
        [Id] int '$.Id',
        [Int] int '$.Int',
        [Ints] nvarchar(max) '$.Ints' AS JSON,
        [Name] nvarchar(max) '$.Name',
        [String] nvarchar(max) '$.String'
    ) AS [r0]
    WHERE [r0].[Id] = @entity_equality_nested_Id AND [r0].[Int] = @entity_equality_nested_Int AND [r0].[Ints] = @entity_equality_nested_Ints AND [r0].[Name] = @entity_equality_nested_Name AND [r0].[String] = @entity_equality_nested_String)
ORDER BY [r].[Id]
""");
    }

    public override async Task Contains_with_operators_composed_on_the_collection()
    {
        await base.Contains_with_operators_composed_on_the_collection();

        AssertSql(
            """
@get_Item_Int='106'
@entity_equality_get_Item_Id='3003' (Nullable = true)
@entity_equality_get_Item_Int='108' (Nullable = true)
@entity_equality_get_Item_Ints='[8,9,109]' (Size = 4000)
@entity_equality_get_Item_Name='Root3_RequiredAssociate_NestedCollection_2' (Size = 4000)
@entity_equality_get_Item_String='foo104' (Size = 4000)

SELECT [r].[Id], [r].[Name], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[RootEntityId], [r1].[String], [r1].[RequiredNestedAssociate_Id], [r1].[RequiredNestedAssociate_Int], [r1].[RequiredNestedAssociate_Ints], [r1].[RequiredNestedAssociate_Name], [r1].[RequiredNestedAssociate_String], [r1].[NestedCollection], [r1].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r1] ON [r].[Id] = [r1].[RootEntityId]
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON([r].[RequiredAssociate_NestedCollection], '$') WITH (
        [Id] int '$.Id',
        [Int] int '$.Int',
        [Ints] nvarchar(max) '$.Ints' AS JSON,
        [Name] nvarchar(max) '$.Name',
        [String] nvarchar(max) '$.String'
    ) AS [r0]
    WHERE [r0].[Int] > @get_Item_Int AND [r0].[Id] = @entity_equality_get_Item_Id AND [r0].[Int] = @entity_equality_get_Item_Int AND [r0].[Ints] = @entity_equality_get_Item_Ints AND [r0].[Name] = @entity_equality_get_Item_Name AND [r0].[String] = @entity_equality_get_Item_String)
ORDER BY [r].[Id]
""");
    }

    public override async Task Contains_with_nested_and_composed_operators()
    {
        await base.Contains_with_nested_and_composed_operators();

        AssertSql(
            """
@get_Item_Id='302'
@entity_equality_get_Item_RequiredNestedAssociate_Id='3012' (Nullable = true)

SELECT [r].[Id], [r].[Name], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[RootEntityId], [r1].[String], [r1].[RequiredNestedAssociate_Id], [r1].[RequiredNestedAssociate_Int], [r1].[RequiredNestedAssociate_Ints], [r1].[RequiredNestedAssociate_Name], [r1].[RequiredNestedAssociate_String], [r1].[NestedCollection], [r1].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r1] ON [r].[Id] = [r1].[RootEntityId]
WHERE EXISTS (
    SELECT 1
    FROM [RootEntity_AssociateCollection] AS [r0]
    WHERE [r].[Id] = [r0].[RootEntityId] AND [r0].[Id] > @get_Item_Id AND [r0].[RequiredNestedAssociate_Id] = @entity_equality_get_Item_RequiredNestedAssociate_Id)
ORDER BY [r].[Id]
""");
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
