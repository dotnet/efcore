// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplitting;

public class ComplexTableSplittingStructuralEqualitySqlServerTest(
    ComplexTableSplittingSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : ComplexTableSplittingStructuralEqualityRelationalTestBase<ComplexTableSplittingSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Two_associates()
    {
        await base.Two_associates();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredAssociate_Id] = [r].[OptionalAssociate_Id] AND [r].[RequiredAssociate_Int] = [r].[OptionalAssociate_Int] AND [r].[RequiredAssociate_Ints] = [r].[OptionalAssociate_Ints] AND [r].[RequiredAssociate_Name] = [r].[OptionalAssociate_Name] AND [r].[RequiredAssociate_String] = [r].[OptionalAssociate_String] AND ([r].[RequiredAssociate_OptionalNestedAssociate_Id] = [r].[OptionalAssociate_OptionalNestedAssociate_Id] OR ([r].[RequiredAssociate_OptionalNestedAssociate_Id] IS NULL AND [r].[OptionalAssociate_OptionalNestedAssociate_Id] IS NULL)) AND ([r].[RequiredAssociate_OptionalNestedAssociate_Int] = [r].[OptionalAssociate_OptionalNestedAssociate_Int] OR ([r].[RequiredAssociate_OptionalNestedAssociate_Int] IS NULL AND [r].[OptionalAssociate_OptionalNestedAssociate_Int] IS NULL)) AND ([r].[RequiredAssociate_OptionalNestedAssociate_Ints] = [r].[OptionalAssociate_OptionalNestedAssociate_Ints] OR ([r].[RequiredAssociate_OptionalNestedAssociate_Ints] IS NULL AND [r].[OptionalAssociate_OptionalNestedAssociate_Ints] IS NULL)) AND ([r].[RequiredAssociate_OptionalNestedAssociate_Name] = [r].[OptionalAssociate_OptionalNestedAssociate_Name] OR ([r].[RequiredAssociate_OptionalNestedAssociate_Name] IS NULL AND [r].[OptionalAssociate_OptionalNestedAssociate_Name] IS NULL)) AND ([r].[RequiredAssociate_OptionalNestedAssociate_String] = [r].[OptionalAssociate_OptionalNestedAssociate_String] OR ([r].[RequiredAssociate_OptionalNestedAssociate_String] IS NULL AND [r].[OptionalAssociate_OptionalNestedAssociate_String] IS NULL)) AND [r].[RequiredAssociate_RequiredNestedAssociate_Id] = [r].[OptionalAssociate_RequiredNestedAssociate_Id] AND [r].[RequiredAssociate_RequiredNestedAssociate_Int] = [r].[OptionalAssociate_RequiredNestedAssociate_Int] AND [r].[RequiredAssociate_RequiredNestedAssociate_Ints] = [r].[OptionalAssociate_RequiredNestedAssociate_Ints] AND [r].[RequiredAssociate_RequiredNestedAssociate_Name] = [r].[OptionalAssociate_RequiredNestedAssociate_Name] AND [r].[RequiredAssociate_RequiredNestedAssociate_String] = [r].[OptionalAssociate_RequiredNestedAssociate_String]
""");
    }

    public override async Task Two_nested_associates()
    {
        await base.Two_nested_associates();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredAssociate_RequiredNestedAssociate_Id] = [r].[OptionalAssociate_RequiredNestedAssociate_Id] AND [r].[RequiredAssociate_RequiredNestedAssociate_Int] = [r].[OptionalAssociate_RequiredNestedAssociate_Int] AND [r].[RequiredAssociate_RequiredNestedAssociate_Ints] = [r].[OptionalAssociate_RequiredNestedAssociate_Ints] AND [r].[RequiredAssociate_RequiredNestedAssociate_Name] = [r].[OptionalAssociate_RequiredNestedAssociate_Name] AND [r].[RequiredAssociate_RequiredNestedAssociate_String] = [r].[OptionalAssociate_RequiredNestedAssociate_String]
""");
    }

    public override async Task Not_equals()
    {
        await base.Not_equals();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredAssociate_Id] <> [r].[OptionalAssociate_Id] OR [r].[OptionalAssociate_Id] IS NULL OR [r].[RequiredAssociate_Int] <> [r].[OptionalAssociate_Int] OR [r].[OptionalAssociate_Int] IS NULL OR [r].[RequiredAssociate_Ints] <> [r].[OptionalAssociate_Ints] OR [r].[OptionalAssociate_Ints] IS NULL OR [r].[RequiredAssociate_Name] <> [r].[OptionalAssociate_Name] OR [r].[OptionalAssociate_Name] IS NULL OR [r].[RequiredAssociate_String] <> [r].[OptionalAssociate_String] OR [r].[OptionalAssociate_String] IS NULL OR (([r].[RequiredAssociate_OptionalNestedAssociate_Id] <> [r].[OptionalAssociate_OptionalNestedAssociate_Id] OR [r].[RequiredAssociate_OptionalNestedAssociate_Id] IS NULL OR [r].[OptionalAssociate_OptionalNestedAssociate_Id] IS NULL) AND ([r].[RequiredAssociate_OptionalNestedAssociate_Id] IS NOT NULL OR [r].[OptionalAssociate_OptionalNestedAssociate_Id] IS NOT NULL)) OR (([r].[RequiredAssociate_OptionalNestedAssociate_Int] <> [r].[OptionalAssociate_OptionalNestedAssociate_Int] OR [r].[RequiredAssociate_OptionalNestedAssociate_Int] IS NULL OR [r].[OptionalAssociate_OptionalNestedAssociate_Int] IS NULL) AND ([r].[RequiredAssociate_OptionalNestedAssociate_Int] IS NOT NULL OR [r].[OptionalAssociate_OptionalNestedAssociate_Int] IS NOT NULL)) OR (([r].[RequiredAssociate_OptionalNestedAssociate_Ints] <> [r].[OptionalAssociate_OptionalNestedAssociate_Ints] OR [r].[RequiredAssociate_OptionalNestedAssociate_Ints] IS NULL OR [r].[OptionalAssociate_OptionalNestedAssociate_Ints] IS NULL) AND ([r].[RequiredAssociate_OptionalNestedAssociate_Ints] IS NOT NULL OR [r].[OptionalAssociate_OptionalNestedAssociate_Ints] IS NOT NULL)) OR (([r].[RequiredAssociate_OptionalNestedAssociate_Name] <> [r].[OptionalAssociate_OptionalNestedAssociate_Name] OR [r].[RequiredAssociate_OptionalNestedAssociate_Name] IS NULL OR [r].[OptionalAssociate_OptionalNestedAssociate_Name] IS NULL) AND ([r].[RequiredAssociate_OptionalNestedAssociate_Name] IS NOT NULL OR [r].[OptionalAssociate_OptionalNestedAssociate_Name] IS NOT NULL)) OR (([r].[RequiredAssociate_OptionalNestedAssociate_String] <> [r].[OptionalAssociate_OptionalNestedAssociate_String] OR [r].[RequiredAssociate_OptionalNestedAssociate_String] IS NULL OR [r].[OptionalAssociate_OptionalNestedAssociate_String] IS NULL) AND ([r].[RequiredAssociate_OptionalNestedAssociate_String] IS NOT NULL OR [r].[OptionalAssociate_OptionalNestedAssociate_String] IS NOT NULL)) OR [r].[RequiredAssociate_RequiredNestedAssociate_Id] <> [r].[OptionalAssociate_RequiredNestedAssociate_Id] OR [r].[OptionalAssociate_RequiredNestedAssociate_Id] IS NULL OR [r].[RequiredAssociate_RequiredNestedAssociate_Int] <> [r].[OptionalAssociate_RequiredNestedAssociate_Int] OR [r].[OptionalAssociate_RequiredNestedAssociate_Int] IS NULL OR [r].[RequiredAssociate_RequiredNestedAssociate_Ints] <> [r].[OptionalAssociate_RequiredNestedAssociate_Ints] OR [r].[OptionalAssociate_RequiredNestedAssociate_Ints] IS NULL OR [r].[RequiredAssociate_RequiredNestedAssociate_Name] <> [r].[OptionalAssociate_RequiredNestedAssociate_Name] OR [r].[OptionalAssociate_RequiredNestedAssociate_Name] IS NULL OR [r].[RequiredAssociate_RequiredNestedAssociate_String] <> [r].[OptionalAssociate_RequiredNestedAssociate_String] OR [r].[OptionalAssociate_RequiredNestedAssociate_String] IS NULL
""");
    }

    public override async Task Associate_with_inline_null()
    {
        await base.Associate_with_inline_null();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
WHERE [r].[OptionalAssociate_Id] IS NULL
""");
    }

    public override async Task Associate_with_parameter_null()
    {
        await base.Associate_with_parameter_null();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
WHERE [r].[OptionalAssociate_Id] IS NULL AND [r].[OptionalAssociate_Int] IS NULL AND [r].[OptionalAssociate_Ints] IS NULL AND [r].[OptionalAssociate_Name] IS NULL AND [r].[OptionalAssociate_String] IS NULL AND [r].[OptionalAssociate_OptionalNestedAssociate_Id] IS NULL AND [r].[OptionalAssociate_OptionalNestedAssociate_Int] IS NULL AND [r].[OptionalAssociate_OptionalNestedAssociate_Ints] IS NULL AND [r].[OptionalAssociate_OptionalNestedAssociate_Name] IS NULL AND [r].[OptionalAssociate_OptionalNestedAssociate_String] IS NULL AND [r].[OptionalAssociate_RequiredNestedAssociate_Id] IS NULL AND [r].[OptionalAssociate_RequiredNestedAssociate_Int] IS NULL AND [r].[OptionalAssociate_RequiredNestedAssociate_Ints] IS NULL AND [r].[OptionalAssociate_RequiredNestedAssociate_Name] IS NULL AND [r].[OptionalAssociate_RequiredNestedAssociate_String] IS NULL
""");
    }

    public override async Task Nested_associate_with_inline_null()
    {
        await base.Nested_associate_with_inline_null();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredAssociate_OptionalNestedAssociate_Id] IS NULL
""");
    }

    public override async Task Nested_associate_with_inline()
    {
        await base.Nested_associate_with_inline();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredAssociate_RequiredNestedAssociate_Id] = 1000 AND [r].[RequiredAssociate_RequiredNestedAssociate_Int] = 8 AND [r].[RequiredAssociate_RequiredNestedAssociate_Ints] = N'[1,2,3]' AND [r].[RequiredAssociate_RequiredNestedAssociate_Name] = N'Root1_RequiredAssociate_RequiredNestedAssociate' AND [r].[RequiredAssociate_RequiredNestedAssociate_String] = N'foo'
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

SELECT [r].[Id], [r].[Name], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredAssociate_RequiredNestedAssociate_Id] = @entity_equality_nested_Id AND [r].[RequiredAssociate_RequiredNestedAssociate_Int] = @entity_equality_nested_Int AND [r].[RequiredAssociate_RequiredNestedAssociate_Ints] = @entity_equality_nested_Ints AND [r].[RequiredAssociate_RequiredNestedAssociate_Name] = @entity_equality_nested_Name AND [r].[RequiredAssociate_RequiredNestedAssociate_String] = @entity_equality_nested_String
""");
    }

    public override async Task Two_nested_collections()
    {
        await base.Two_nested_collections();

        AssertSql();
    }

    public override async Task Nested_collection_with_inline()
    {
        await base.Nested_collection_with_inline();

        AssertSql();
    }

    public override async Task Nested_collection_with_parameter()
    {
        await base.Nested_collection_with_parameter();

        AssertSql();
    }

    #region Contains

    public override async Task Contains_with_inline()
    {
        await base.Contains_with_inline();

        AssertSql();
    }

    public override async Task Contains_with_parameter()
    {
        await base.Contains_with_parameter();

        AssertSql();
    }

    public override async Task Contains_with_operators_composed_on_the_collection()
    {
        await base.Contains_with_operators_composed_on_the_collection();

        AssertSql();
    }

    public override async Task Contains_with_nested_and_composed_operators()
    {
        await base.Contains_with_nested_and_composed_operators();

        AssertSql();
    }

    #endregion Contains

    #region Value types

    public override async Task Nullable_value_type_with_null()
    {
        await base.Nullable_value_type_with_null();

        AssertSql(
            """
SELECT [v].[Id], [v].[Name], [v].[OptionalAssociate_Id], [v].[OptionalAssociate_Int], [v].[OptionalAssociate_Name], [v].[OptionalAssociate_String], [v].[OptionalAssociate_OptionalNested_Id], [v].[OptionalAssociate_OptionalNested_Int], [v].[OptionalAssociate_OptionalNested_Name], [v].[OptionalAssociate_OptionalNested_String], [v].[OptionalAssociate_RequiredNested_Id], [v].[OptionalAssociate_RequiredNested_Int], [v].[OptionalAssociate_RequiredNested_Name], [v].[OptionalAssociate_RequiredNested_String], [v].[RequiredAssociate_Id], [v].[RequiredAssociate_Int], [v].[RequiredAssociate_Name], [v].[RequiredAssociate_String], [v].[RequiredAssociate_OptionalNested_Id], [v].[RequiredAssociate_OptionalNested_Int], [v].[RequiredAssociate_OptionalNested_Name], [v].[RequiredAssociate_OptionalNested_String], [v].[RequiredAssociate_RequiredNested_Id], [v].[RequiredAssociate_RequiredNested_Int], [v].[RequiredAssociate_RequiredNested_Name], [v].[RequiredAssociate_RequiredNested_String]
FROM [ValueRootEntity] AS [v]
WHERE [v].[OptionalAssociate_Id] IS NULL
""");
    }

    #endregion Value types

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
