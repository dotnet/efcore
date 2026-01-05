// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class TPHInheritanceTableSplittingQuerySqlServerTest(
    TPHInheritanceQuerySqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : TPHInheritanceTableSplittingQueryRelationalTestBase<TPHInheritanceQuerySqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Filter_on_complex_type_property_on_derived_type(bool async)
    {
        await base.Filter_on_complex_type_property_on_derived_type(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Discriminator], [d].[SortIndex], [d].[CaffeineGrams], [d].[CokeCO2], [d].[SugarGrams], [d].[ComplexTypeCollection], [d].[ParentComplexType_Int], [d].[ParentComplexType_UniqueInt], [d].[ParentComplexType_Nested_NestedInt], [d].[ParentComplexType_Nested_UniqueInt], [d].[ChildComplexType_Int], [d].[ChildComplexType_UniqueInt], [d].[ChildComplexType_Nested_NestedInt], [d].[ChildComplexType_Nested_UniqueInt]
FROM [Drinks] AS [d]
WHERE [d].[Discriminator] = 1 AND [d].[ChildComplexType_Int] = 10
""");
    }

    public override async Task Filter_on_complex_type_property_on_base_type(bool async)
    {
        await base.Filter_on_complex_type_property_on_base_type(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Discriminator], [d].[SortIndex], [d].[CaffeineGrams], [d].[CokeCO2], [d].[SugarGrams], [d].[LiltCO2], [d].[HasMilk], [d].[ComplexTypeCollection], [d].[ParentComplexType_Int], [d].[ParentComplexType_UniqueInt], [d].[ParentComplexType_Nested_NestedInt], [d].[ParentComplexType_Nested_UniqueInt], [d].[ChildComplexType_Int], [d].[ChildComplexType_UniqueInt], [d].[ChildComplexType_Nested_NestedInt], [d].[ChildComplexType_Nested_UniqueInt], [d].[Tea_ChildComplexType_Int], [d].[Tea_ChildComplexType_UniqueInt], [d].[Tea_ChildComplexType_Nested_NestedInt], [d].[Tea_ChildComplexType_Nested_UniqueInt]
FROM [Drinks] AS [d]
WHERE [d].[ParentComplexType_Int] = 8
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_derived_type(bool async)
    {
        await base.Filter_on_nested_complex_type_property_on_derived_type(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Discriminator], [d].[SortIndex], [d].[CaffeineGrams], [d].[CokeCO2], [d].[SugarGrams], [d].[ComplexTypeCollection], [d].[ParentComplexType_Int], [d].[ParentComplexType_UniqueInt], [d].[ParentComplexType_Nested_NestedInt], [d].[ParentComplexType_Nested_UniqueInt], [d].[ChildComplexType_Int], [d].[ChildComplexType_UniqueInt], [d].[ChildComplexType_Nested_NestedInt], [d].[ChildComplexType_Nested_UniqueInt]
FROM [Drinks] AS [d]
WHERE [d].[Discriminator] = 1 AND [d].[ChildComplexType_Nested_NestedInt] = 58
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_base_type(bool async)
    {
        await base.Filter_on_nested_complex_type_property_on_base_type(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Discriminator], [d].[SortIndex], [d].[CaffeineGrams], [d].[CokeCO2], [d].[SugarGrams], [d].[LiltCO2], [d].[HasMilk], [d].[ComplexTypeCollection], [d].[ParentComplexType_Int], [d].[ParentComplexType_UniqueInt], [d].[ParentComplexType_Nested_NestedInt], [d].[ParentComplexType_Nested_UniqueInt], [d].[ChildComplexType_Int], [d].[ChildComplexType_UniqueInt], [d].[ChildComplexType_Nested_NestedInt], [d].[ChildComplexType_Nested_UniqueInt], [d].[Tea_ChildComplexType_Int], [d].[Tea_ChildComplexType_UniqueInt], [d].[Tea_ChildComplexType_Nested_NestedInt], [d].[Tea_ChildComplexType_Nested_UniqueInt]
FROM [Drinks] AS [d]
WHERE [d].[ParentComplexType_Nested_NestedInt] = 50
""");
    }

    public override async Task Project_complex_type_on_derived_type(bool async)
    {
        await base.Project_complex_type_on_derived_type(async);

        AssertSql(
            """
SELECT [d].[ChildComplexType_Int], [d].[ChildComplexType_UniqueInt], [d].[ChildComplexType_Nested_NestedInt], [d].[ChildComplexType_Nested_UniqueInt]
FROM [Drinks] AS [d]
WHERE [d].[Discriminator] = 1
""");
    }

    public override async Task Project_complex_type_on_base_type(bool async)
    {
        await base.Project_complex_type_on_base_type(async);

        AssertSql(
            """
SELECT [d].[ParentComplexType_Int], [d].[ParentComplexType_UniqueInt], [d].[ParentComplexType_Nested_NestedInt], [d].[ParentComplexType_Nested_UniqueInt]
FROM [Drinks] AS [d]
""");
    }

    public override async Task Project_nested_complex_type_on_derived_type(bool async)
    {
        await base.Project_nested_complex_type_on_derived_type(async);

        AssertSql(
            """
SELECT [d].[ChildComplexType_Nested_NestedInt], [d].[ChildComplexType_Nested_UniqueInt]
FROM [Drinks] AS [d]
WHERE [d].[Discriminator] = 1
""");
    }

    public override async Task Project_nested_complex_type_on_base_type(bool async)
    {
        await base.Project_nested_complex_type_on_base_type(async);

        AssertSql(
            """
SELECT [d].[ParentComplexType_Nested_NestedInt], [d].[ParentComplexType_Nested_UniqueInt]
FROM [Drinks] AS [d]
""");
    }

    public override async Task Subquery_over_complex_collection(bool async)
    {
        await base.Subquery_over_complex_collection(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Discriminator], [d].[SortIndex], [d].[CaffeineGrams], [d].[CokeCO2], [d].[SugarGrams], [d].[LiltCO2], [d].[HasMilk], [d].[ComplexTypeCollection], [d].[ParentComplexType_Int], [d].[ParentComplexType_UniqueInt], [d].[ParentComplexType_Nested_NestedInt], [d].[ParentComplexType_Nested_UniqueInt], [d].[ChildComplexType_Int], [d].[ChildComplexType_UniqueInt], [d].[ChildComplexType_Nested_NestedInt], [d].[ChildComplexType_Nested_UniqueInt], [d].[Tea_ChildComplexType_Int], [d].[Tea_ChildComplexType_UniqueInt], [d].[Tea_ChildComplexType_Nested_NestedInt], [d].[Tea_ChildComplexType_Nested_UniqueInt]
FROM [Drinks] AS [d]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([d].[ComplexTypeCollection], '$') WITH ([Int] int '$.Int') AS [c]
    WHERE [c].[Int] > 59) = 2
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
