// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class TPTInheritanceTableSplittingQuerySqlServerTest(
    TPTInheritanceQuerySqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : TPTInheritanceTableSplittingQueryRelationalTestBase<TPTInheritanceQuerySqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Filter_on_complex_type_property_on_derived_type(bool async)
    {
        await base.Filter_on_complex_type_property_on_derived_type(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[Ints], [c].[SugarGrams], [d].[ComplexTypeCollection], [d].[ParentComplexType_Int], [d].[ParentComplexType_UniqueInt], [d].[ParentComplexType_Nested_NestedInt], [d].[ParentComplexType_Nested_UniqueInt], [c].[ChildComplexType_Int], [c].[ChildComplexType_UniqueInt], [c].[ChildComplexType_Nested_NestedInt], [c].[ChildComplexType_Nested_UniqueInt]
FROM [Drinks] AS [d]
INNER JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
WHERE [c].[ChildComplexType_Int] = 10
""");
    }

    public override async Task Filter_on_complex_type_property_on_base_type(bool async)
    {
        await base.Filter_on_complex_type_property_on_base_type(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[Ints], [c].[SugarGrams], [l].[LiltCO2], [l].[SugarGrams], [t].[CaffeineGrams], [t].[HasMilk], [d].[ComplexTypeCollection], [d].[ParentComplexType_Int], [d].[ParentComplexType_UniqueInt], [d].[ParentComplexType_Nested_NestedInt], [d].[ParentComplexType_Nested_UniqueInt], [c].[ChildComplexType_Int], [c].[ChildComplexType_UniqueInt], [c].[ChildComplexType_Nested_NestedInt], [c].[ChildComplexType_Nested_UniqueInt], [t].[ChildComplexType_Int], [t].[ChildComplexType_UniqueInt], [t].[ChildComplexType_Nested_NestedInt], [t].[ChildComplexType_Nested_UniqueInt], CASE
    WHEN [t].[Id] IS NOT NULL THEN N'Tea'
    WHEN [l].[Id] IS NOT NULL THEN N'Lilt'
    WHEN [c].[Id] IS NOT NULL THEN N'Coke'
END AS [Discriminator]
FROM [Drinks] AS [d]
LEFT JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
LEFT JOIN [Lilt] AS [l] ON [d].[Id] = [l].[Id]
LEFT JOIN [Tea] AS [t] ON [d].[Id] = [t].[Id]
WHERE [d].[ParentComplexType_Int] = 8
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_derived_type(bool async)
    {
        await base.Filter_on_nested_complex_type_property_on_derived_type(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[Ints], [c].[SugarGrams], [d].[ComplexTypeCollection], [d].[ParentComplexType_Int], [d].[ParentComplexType_UniqueInt], [d].[ParentComplexType_Nested_NestedInt], [d].[ParentComplexType_Nested_UniqueInt], [c].[ChildComplexType_Int], [c].[ChildComplexType_UniqueInt], [c].[ChildComplexType_Nested_NestedInt], [c].[ChildComplexType_Nested_UniqueInt]
FROM [Drinks] AS [d]
INNER JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
WHERE [c].[ChildComplexType_Nested_NestedInt] = 58
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_base_type(bool async)
    {
        await base.Filter_on_nested_complex_type_property_on_base_type(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[Ints], [c].[SugarGrams], [l].[LiltCO2], [l].[SugarGrams], [t].[CaffeineGrams], [t].[HasMilk], [d].[ComplexTypeCollection], [d].[ParentComplexType_Int], [d].[ParentComplexType_UniqueInt], [d].[ParentComplexType_Nested_NestedInt], [d].[ParentComplexType_Nested_UniqueInt], [c].[ChildComplexType_Int], [c].[ChildComplexType_UniqueInt], [c].[ChildComplexType_Nested_NestedInt], [c].[ChildComplexType_Nested_UniqueInt], [t].[ChildComplexType_Int], [t].[ChildComplexType_UniqueInt], [t].[ChildComplexType_Nested_NestedInt], [t].[ChildComplexType_Nested_UniqueInt], CASE
    WHEN [t].[Id] IS NOT NULL THEN N'Tea'
    WHEN [l].[Id] IS NOT NULL THEN N'Lilt'
    WHEN [c].[Id] IS NOT NULL THEN N'Coke'
END AS [Discriminator]
FROM [Drinks] AS [d]
LEFT JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
LEFT JOIN [Lilt] AS [l] ON [d].[Id] = [l].[Id]
LEFT JOIN [Tea] AS [t] ON [d].[Id] = [t].[Id]
WHERE [d].[ParentComplexType_Nested_NestedInt] = 50
""");
    }

    public override async Task Project_complex_type_on_derived_type(bool async)
    {
        await base.Project_complex_type_on_derived_type(async);

        AssertSql(
            """
SELECT [c].[ChildComplexType_Int], [c].[ChildComplexType_UniqueInt], [c].[ChildComplexType_Nested_NestedInt], [c].[ChildComplexType_Nested_UniqueInt]
FROM [Drinks] AS [d]
INNER JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
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
SELECT [c].[ChildComplexType_Nested_NestedInt], [c].[ChildComplexType_Nested_UniqueInt]
FROM [Drinks] AS [d]
INNER JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
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
SELECT [d].[Id], [d].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[Ints], [c].[SugarGrams], [l].[LiltCO2], [l].[SugarGrams], [t].[CaffeineGrams], [t].[HasMilk], [d].[ComplexTypeCollection], [d].[ParentComplexType_Int], [d].[ParentComplexType_UniqueInt], [d].[ParentComplexType_Nested_NestedInt], [d].[ParentComplexType_Nested_UniqueInt], [c].[ChildComplexType_Int], [c].[ChildComplexType_UniqueInt], [c].[ChildComplexType_Nested_NestedInt], [c].[ChildComplexType_Nested_UniqueInt], [t].[ChildComplexType_Int], [t].[ChildComplexType_UniqueInt], [t].[ChildComplexType_Nested_NestedInt], [t].[ChildComplexType_Nested_UniqueInt], CASE
    WHEN [t].[Id] IS NOT NULL THEN N'Tea'
    WHEN [l].[Id] IS NOT NULL THEN N'Lilt'
    WHEN [c].[Id] IS NOT NULL THEN N'Coke'
END AS [Discriminator]
FROM [Drinks] AS [d]
LEFT JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
LEFT JOIN [Lilt] AS [l] ON [d].[Id] = [l].[Id]
LEFT JOIN [Tea] AS [t] ON [d].[Id] = [t].[Id]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([d].[ComplexTypeCollection], '$') WITH ([Int] int '$.Int') AS [c0]
    WHERE [c0].[Int] > 59) = 2
""");
    }

    public override async Task Owned_reference_with_complex_type_on_base_and_derived_types(bool async)
    {
        await base.Owned_reference_with_complex_type_on_base_and_derived_types(async);

        AssertSql(
            """
SELECT [m].[Id], [m].[Name], CASE
    WHEN [b].[Id] IS NOT NULL THEN N'Brewery'
END AS [Discriminator], [m].[Headquarters_Name], [m].[Headquarters_Address_City], [m].[Headquarters_Address_Street], [m].[Headquarters_Address_Country_Code], [m].[Headquarters_Address_Country_Name], [b].[Id], [b].[Brewhouse_Name], [b].[Brewhouse_Address_City], [b].[Brewhouse_Address_Street], [b].[Brewhouse_Address_Country_Code], [b].[Brewhouse_Address_Country_Name]
FROM [Manufacturer] AS [m]
LEFT JOIN [Brewery] AS [b] ON [m].[Id] = [b].[Id]
""");
    }

    public override async Task Filter_on_complex_type_property_of_owned_reference_on_base_type(bool async)
    {
        await base.Filter_on_complex_type_property_of_owned_reference_on_base_type(async);

        AssertSql(
            """
SELECT [m].[Id], [m].[Name], CASE
    WHEN [b].[Id] IS NOT NULL THEN N'Brewery'
END AS [Discriminator], [m].[Headquarters_Name], [m].[Headquarters_Address_City], [m].[Headquarters_Address_Street], [m].[Headquarters_Address_Country_Code], [m].[Headquarters_Address_Country_Name], [b].[Id], [b].[Brewhouse_Name], [b].[Brewhouse_Address_City], [b].[Brewhouse_Address_Street], [b].[Brewhouse_Address_Country_Code], [b].[Brewhouse_Address_Country_Name]
FROM [Manufacturer] AS [m]
LEFT JOIN [Brewery] AS [b] ON [m].[Id] = [b].[Id]
WHERE [m].[Headquarters_Address_Country_Code] = N'DE'
""");
    }

    public override async Task Filter_on_complex_type_property_of_owned_reference_on_derived_type(bool async)
    {
        await base.Filter_on_complex_type_property_of_owned_reference_on_derived_type(async);

        AssertSql(
            """
SELECT [m].[Id], [m].[Name], [m].[Headquarters_Name], [m].[Headquarters_Address_City], [m].[Headquarters_Address_Street], [m].[Headquarters_Address_Country_Code], [m].[Headquarters_Address_Country_Name], [b].[Id], [b].[Brewhouse_Name], [b].[Brewhouse_Address_City], [b].[Brewhouse_Address_Street], [b].[Brewhouse_Address_Country_Code], [b].[Brewhouse_Address_Country_Name]
FROM [Manufacturer] AS [m]
INNER JOIN [Brewery] AS [b] ON [m].[Id] = [b].[Id]
WHERE [b].[Brewhouse_Address_Country_Code] = N'DE'
""");
    }

    public override async Task Project_complex_type_of_owned_reference_on_base_type(bool async)
    {
        await base.Project_complex_type_of_owned_reference_on_base_type(async);

        AssertSql(
            """
SELECT [m].[Headquarters_Address_City], [m].[Headquarters_Address_Street], [m].[Headquarters_Address_Country_Code], [m].[Headquarters_Address_Country_Name]
FROM [Manufacturer] AS [m]
""");
    }

    public override async Task Project_complex_type_of_owned_reference_on_derived_type(bool async)
    {
        await base.Project_complex_type_of_owned_reference_on_derived_type(async);

        AssertSql(
            """
SELECT [b].[Brewhouse_Address_City], [b].[Brewhouse_Address_Street], [b].[Brewhouse_Address_Country_Code], [b].[Brewhouse_Address_Country_Name]
FROM [Manufacturer] AS [m]
LEFT JOIN [Brewery] AS [b] ON [m].[Id] = [b].[Id]
WHERE [b].[Id] IS NOT NULL
""");
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
