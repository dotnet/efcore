// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

// TODO: #35025
internal class TPTInheritanceTableSplittingQuerySqlServerTest(
    TPTInheritanceQuerySqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : TPTInheritanceTableSplittingQueryRelationalTestBase<TPTInheritanceQuerySqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Filter_on_complex_type_property_on_derived_type(bool async)
    {
        await base.Filter_on_complex_type_property_on_derived_type(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[SugarGrams], [d].[AdditionalParentInfo_Int], [d].[AdditionalParentInfo_UniqueInt], [d].[AdditionalParentInfo_Nested_NestedInt], [d].[AdditionalParentInfo_Nested_UniqueInt], [c].[AdditionalChildInfo_Int], [c].[AdditionalChildInfo_UniqueInt], [c].[AdditionalChildInfo_Nested_NestedInt], [c].[AdditionalChildInfo_Nested_UniqueInt]
FROM [Drinks] AS [d]
INNER JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
WHERE [c].[AdditionalChildInfo_Int] = 10
""");
    }

    public override async Task Filter_on_complex_type_property_on_base_type(bool async)
    {
        await base.Filter_on_complex_type_property_on_base_type(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[SugarGrams], [l].[LiltCO2], [l].[SugarGrams], [t].[CaffeineGrams], [t].[HasMilk], [d].[AdditionalParentInfo_Int], [d].[AdditionalParentInfo_UniqueInt], [d].[AdditionalParentInfo_Nested_NestedInt], [d].[AdditionalParentInfo_Nested_UniqueInt], [c].[AdditionalChildInfo_Int], [c].[AdditionalChildInfo_UniqueInt], [c].[AdditionalChildInfo_Nested_NestedInt], [c].[AdditionalChildInfo_Nested_UniqueInt], [t].[AdditionalChildInfo_Int], [t].[AdditionalChildInfo_UniqueInt], [t].[AdditionalChildInfo_Nested_NestedInt], [t].[AdditionalChildInfo_Nested_UniqueInt], CASE
    WHEN [t].[Id] IS NOT NULL THEN N'Tea'
    WHEN [l].[Id] IS NOT NULL THEN N'Lilt'
    WHEN [c].[Id] IS NOT NULL THEN N'Coke'
END AS [Discriminator]
FROM [Drinks] AS [d]
LEFT JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
LEFT JOIN [Lilt] AS [l] ON [d].[Id] = [l].[Id]
LEFT JOIN [Tea] AS [t] ON [d].[Id] = [t].[Id]
WHERE [d].[AdditionalParentInfo_Int] = 8
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_derived_type(bool async)
    {
        await base.Filter_on_nested_complex_type_property_on_derived_type(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[SugarGrams], [d].[AdditionalParentInfo_Int], [d].[AdditionalParentInfo_UniqueInt], [d].[AdditionalParentInfo_Nested_NestedInt], [d].[AdditionalParentInfo_Nested_UniqueInt], [c].[AdditionalChildInfo_Int], [c].[AdditionalChildInfo_UniqueInt], [c].[AdditionalChildInfo_Nested_NestedInt], [c].[AdditionalChildInfo_Nested_UniqueInt]
FROM [Drinks] AS [d]
INNER JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
WHERE [c].[AdditionalChildInfo_Nested_NestedInt] = 52
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_base_type(bool async)
    {
        await base.Filter_on_nested_complex_type_property_on_base_type(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[SugarGrams], [l].[LiltCO2], [l].[SugarGrams], [t].[CaffeineGrams], [t].[HasMilk], [d].[AdditionalParentInfo_Int], [d].[AdditionalParentInfo_UniqueInt], [d].[AdditionalParentInfo_Nested_NestedInt], [d].[AdditionalParentInfo_Nested_UniqueInt], [c].[AdditionalChildInfo_Int], [c].[AdditionalChildInfo_UniqueInt], [c].[AdditionalChildInfo_Nested_NestedInt], [c].[AdditionalChildInfo_Nested_UniqueInt], [t].[AdditionalChildInfo_Int], [t].[AdditionalChildInfo_UniqueInt], [t].[AdditionalChildInfo_Nested_NestedInt], [t].[AdditionalChildInfo_Nested_UniqueInt], CASE
    WHEN [t].[Id] IS NOT NULL THEN N'Tea'
    WHEN [l].[Id] IS NOT NULL THEN N'Lilt'
    WHEN [c].[Id] IS NOT NULL THEN N'Coke'
END AS [Discriminator]
FROM [Drinks] AS [d]
LEFT JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
LEFT JOIN [Lilt] AS [l] ON [d].[Id] = [l].[Id]
LEFT JOIN [Tea] AS [t] ON [d].[Id] = [t].[Id]
WHERE [d].[AdditionalParentInfo_Nested_NestedInt] = 50
""");
    }

    public override async Task Project_complex_type_on_derived_type(bool async)
    {
        await base.Project_complex_type_on_derived_type(async);

        AssertSql(
            """
SELECT [c].[AdditionalChildInfo_Int], [c].[AdditionalChildInfo_UniqueInt], [c].[AdditionalChildInfo_Nested_NestedInt], [c].[AdditionalChildInfo_Nested_UniqueInt]
FROM [Drinks] AS [d]
INNER JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
""");
    }

    public override async Task Project_complex_type_on_base_type(bool async)
    {
        await base.Project_complex_type_on_base_type(async);

        AssertSql(
            """
SELECT [d].[AdditionalParentInfo_Int], [d].[AdditionalParentInfo_UniqueInt], [d].[AdditionalParentInfo_Nested_NestedInt], [d].[AdditionalParentInfo_Nested_UniqueInt]
FROM [Drinks] AS [d]
""");
    }

    public override async Task Project_nested_complex_type_on_derived_type(bool async)
    {
        await base.Project_nested_complex_type_on_derived_type(async);

        AssertSql(
            """
SELECT [c].[AdditionalChildInfo_Nested_NestedInt], [c].[AdditionalChildInfo_Nested_UniqueInt]
FROM [Drinks] AS [d]
INNER JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
""");
    }

    public override async Task Project_nested_complex_type_on_base_type(bool async)
    {
        await base.Project_nested_complex_type_on_base_type(async);

        AssertSql(
            """
SELECT [d].[AdditionalParentInfo_Nested_NestedInt], [d].[AdditionalParentInfo_Nested_UniqueInt]
FROM [Drinks] AS [d]
""");
    }

    public override async Task Subquery_over_complex_collection(bool async)
    {
        await base.Subquery_over_complex_collection(async);

        AssertSql();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
