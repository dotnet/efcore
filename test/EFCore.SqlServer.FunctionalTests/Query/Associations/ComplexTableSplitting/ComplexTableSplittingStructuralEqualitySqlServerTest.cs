// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplitting;

public class ComplexTableSplittingStructuralEqualitySqlServerTest(
    ComplexTableSplittingSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : ComplexTableSplittingStructuralEqualityRelationalTestBase<ComplexTableSplittingSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Two_related()
    {
        await base.Two_related();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredRelated_Id] = [r].[OptionalRelated_Id] AND [r].[RequiredRelated_Int] = [r].[OptionalRelated_Int] AND [r].[RequiredRelated_Name] = [r].[OptionalRelated_Name] AND [r].[RequiredRelated_String] = [r].[OptionalRelated_String] AND ([r].[RequiredRelated_OptionalNested_Id] = [r].[RequiredRelated_OptionalNested_Id] OR [r].[RequiredRelated_OptionalNested_Id] IS NULL) AND ([r].[RequiredRelated_OptionalNested_Int] = [r].[RequiredRelated_OptionalNested_Int] OR [r].[RequiredRelated_OptionalNested_Int] IS NULL) AND ([r].[RequiredRelated_OptionalNested_Name] = [r].[RequiredRelated_OptionalNested_Name] OR [r].[RequiredRelated_OptionalNested_Name] IS NULL) AND ([r].[RequiredRelated_OptionalNested_String] = [r].[RequiredRelated_OptionalNested_String] OR [r].[RequiredRelated_OptionalNested_String] IS NULL) AND [r].[RequiredRelated_RequiredNested_Id] = [r].[RequiredRelated_RequiredNested_Id] AND [r].[RequiredRelated_RequiredNested_Int] = [r].[RequiredRelated_RequiredNested_Int] AND [r].[RequiredRelated_RequiredNested_Name] = [r].[RequiredRelated_RequiredNested_Name] AND [r].[RequiredRelated_RequiredNested_String] = [r].[RequiredRelated_RequiredNested_String]
""");
    }

    public override async Task Two_nested()
    {
        await base.Two_nested();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredRelated_RequiredNested_Id] = [r].[OptionalRelated_RequiredNested_Id] AND [r].[RequiredRelated_RequiredNested_Int] = [r].[OptionalRelated_RequiredNested_Int] AND [r].[RequiredRelated_RequiredNested_Name] = [r].[OptionalRelated_RequiredNested_Name] AND [r].[RequiredRelated_RequiredNested_String] = [r].[OptionalRelated_RequiredNested_String]
""");
    }

    public override async Task Not_equals()
    {
        await base.Not_equals();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredRelated_Id] <> [r].[OptionalRelated_Id] OR [r].[OptionalRelated_Id] IS NULL OR [r].[RequiredRelated_Int] <> [r].[OptionalRelated_Int] OR [r].[OptionalRelated_Int] IS NULL OR [r].[RequiredRelated_Name] <> [r].[OptionalRelated_Name] OR [r].[OptionalRelated_Name] IS NULL OR [r].[RequiredRelated_String] <> [r].[OptionalRelated_String] OR [r].[OptionalRelated_String] IS NULL OR (([r].[RequiredRelated_OptionalNested_Id] <> [r].[RequiredRelated_OptionalNested_Id] OR [r].[RequiredRelated_OptionalNested_Id] IS NULL) AND [r].[RequiredRelated_OptionalNested_Id] IS NOT NULL) OR (([r].[RequiredRelated_OptionalNested_Int] <> [r].[RequiredRelated_OptionalNested_Int] OR [r].[RequiredRelated_OptionalNested_Int] IS NULL) AND [r].[RequiredRelated_OptionalNested_Int] IS NOT NULL) OR (([r].[RequiredRelated_OptionalNested_Name] <> [r].[RequiredRelated_OptionalNested_Name] OR [r].[RequiredRelated_OptionalNested_Name] IS NULL) AND [r].[RequiredRelated_OptionalNested_Name] IS NOT NULL) OR (([r].[RequiredRelated_OptionalNested_String] <> [r].[RequiredRelated_OptionalNested_String] OR [r].[RequiredRelated_OptionalNested_String] IS NULL) AND [r].[RequiredRelated_OptionalNested_String] IS NOT NULL) OR [r].[RequiredRelated_RequiredNested_Id] <> [r].[RequiredRelated_RequiredNested_Id] OR [r].[RequiredRelated_RequiredNested_Id] IS NULL OR [r].[RequiredRelated_RequiredNested_Int] <> [r].[RequiredRelated_RequiredNested_Int] OR [r].[RequiredRelated_RequiredNested_Int] IS NULL OR [r].[RequiredRelated_RequiredNested_Name] <> [r].[RequiredRelated_RequiredNested_Name] OR [r].[RequiredRelated_RequiredNested_Name] IS NULL OR [r].[RequiredRelated_RequiredNested_String] <> [r].[RequiredRelated_RequiredNested_String] OR [r].[RequiredRelated_RequiredNested_String] IS NULL
""");
    }

    public override async Task Related_with_inline_null()
    {
        await base.Related_with_inline_null();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
WHERE [r].[OptionalRelated_Id] IS NULL
""");
    }

    public override async Task Related_with_parameter_null()
    {
        await base.Related_with_parameter_null();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
WHERE [r].[OptionalRelated_Id] IS NULL AND [r].[OptionalRelated_Int] IS NULL AND [r].[OptionalRelated_Name] IS NULL AND [r].[OptionalRelated_String] IS NULL AND [r].[OptionalRelated_OptionalNested_Id] IS NULL AND [r].[OptionalRelated_OptionalNested_Int] IS NULL AND [r].[OptionalRelated_OptionalNested_Name] IS NULL AND [r].[OptionalRelated_OptionalNested_String] IS NULL AND [r].[OptionalRelated_RequiredNested_Id] IS NULL AND [r].[OptionalRelated_RequiredNested_Int] IS NULL AND [r].[OptionalRelated_RequiredNested_Name] IS NULL AND [r].[OptionalRelated_RequiredNested_String] IS NULL
""");
    }

    public override async Task Nested_with_inline_null()
    {
        await base.Nested_with_inline_null();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredRelated_OptionalNested_Id] IS NULL
""");
    }

    public override async Task Nested_with_inline()
    {
        await base.Nested_with_inline();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredRelated_RequiredNested_Id] = 1000 AND [r].[RequiredRelated_RequiredNested_Int] = 8 AND [r].[RequiredRelated_RequiredNested_Name] = N'Root1_RequiredRelated_RequiredNested' AND [r].[RequiredRelated_RequiredNested_String] = N'foo'
""");
    }

    public override async Task Nested_with_parameter()
    {
        await base.Nested_with_parameter();

        AssertSql(
            """
@entity_equality_nested_Id='?' (DbType = Int32)
@entity_equality_nested_Int='?' (DbType = Int32)
@entity_equality_nested_Name='?' (Size = 4000)
@entity_equality_nested_String='?' (Size = 4000)

SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredRelated_RequiredNested_Id] = @entity_equality_nested_Id AND [r].[RequiredRelated_RequiredNested_Int] = @entity_equality_nested_Int AND [r].[RequiredRelated_RequiredNested_Name] = @entity_equality_nested_Name AND [r].[RequiredRelated_RequiredNested_String] = @entity_equality_nested_String
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
SELECT [v].[Id], [v].[Name], [v].[OptionalRelated_Id], [v].[OptionalRelated_Int], [v].[OptionalRelated_Name], [v].[OptionalRelated_String], [v].[OptionalRelated_OptionalNested_Id], [v].[OptionalRelated_OptionalNested_Int], [v].[OptionalRelated_OptionalNested_Name], [v].[OptionalRelated_OptionalNested_String], [v].[OptionalRelated_RequiredNested_Id], [v].[OptionalRelated_RequiredNested_Int], [v].[OptionalRelated_RequiredNested_Name], [v].[OptionalRelated_RequiredNested_String], [v].[RequiredRelated_Id], [v].[RequiredRelated_Int], [v].[RequiredRelated_Name], [v].[RequiredRelated_String], [v].[RequiredRelated_OptionalNested_Id], [v].[RequiredRelated_OptionalNested_Int], [v].[RequiredRelated_OptionalNested_Name], [v].[RequiredRelated_OptionalNested_String], [v].[RequiredRelated_RequiredNested_Id], [v].[RequiredRelated_RequiredNested_Int], [v].[RequiredRelated_RequiredNested_Name], [v].[RequiredRelated_RequiredNested_String]
FROM [ValueRootEntity] AS [v]
WHERE [v].[OptionalRelated_Id] IS NULL
""");
    }

    #endregion Value types

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
