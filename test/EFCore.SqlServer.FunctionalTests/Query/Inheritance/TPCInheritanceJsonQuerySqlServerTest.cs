// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class TPCInheritanceJsonQuerySqlServerTest(TPCInheritanceJsonQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : TPCInheritanceJsonQueryRelationalTestBase<TPCInheritanceJsonQuerySqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Filter_on_complex_type_property_on_derived_type(bool async)
    {
        await base.Filter_on_complex_type_property_on_derived_type(async);

        var jsonValue = SqlServerTestEnvironment.IsAzureSql
            ? "JSON_VALUE([c].[ChildComplexType], '$.Int' RETURNING int)"
            : "CAST(JSON_VALUE([c].[ChildComplexType], '$.Int') AS int)";

        AssertSql(
            $$"""
SELECT [c].[Id], [c].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[Ints], [c].[SugarGrams], [c].[ComplexTypeCollection], [c].[ParentComplexType], [c].[ChildComplexType]
FROM [Coke] AS [c]
WHERE {{jsonValue}} = 10
""");
    }

    public override async Task Filter_on_complex_type_property_on_base_type(bool async)
    {
        await base.Filter_on_complex_type_property_on_base_type(async);

        var jsonValue = SqlServerTestEnvironment.IsAzureSql
            ? "JSON_VALUE([u].[ParentComplexType], '$.Int' RETURNING int)"
            : "CAST(JSON_VALUE([u].[ParentComplexType], '$.Int') AS int)";

        AssertSql(
            $$"""
SELECT [u].[Id], [u].[SortIndex], [u].[CaffeineGrams], [u].[CokeCO2], [u].[Ints], [u].[SugarGrams], [u].[LiltCO2], [u].[SugarGrams1], [u].[CaffeineGrams1], [u].[HasMilk], [u].[ComplexTypeCollection], [u].[ParentComplexType], [u].[ChildComplexType], [u].[ChildComplexType1], [u].[Discriminator]
FROM (
    SELECT [d].[Id], [d].[SortIndex], [d].[ComplexTypeCollection], [d].[ParentComplexType], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [Ints], NULL AS [SugarGrams], NULL AS [ChildComplexType], NULL AS [LiltCO2], NULL AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType1], N'Drink' AS [Discriminator]
    FROM [Drinks] AS [d]
    UNION ALL
    SELECT [c].[Id], [c].[SortIndex], [c].[ComplexTypeCollection], [c].[ParentComplexType], [c].[CaffeineGrams], [c].[CokeCO2], [c].[Ints], [c].[SugarGrams], [c].[ChildComplexType], NULL AS [LiltCO2], NULL AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType1], N'Coke' AS [Discriminator]
    FROM [Coke] AS [c]
    UNION ALL
    SELECT [l].[Id], [l].[SortIndex], [l].[ComplexTypeCollection], [l].[ParentComplexType], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [Ints], NULL AS [SugarGrams], NULL AS [ChildComplexType], [l].[LiltCO2], [l].[SugarGrams] AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType1], N'Lilt' AS [Discriminator]
    FROM [Lilt] AS [l]
    UNION ALL
    SELECT [t].[Id], [t].[SortIndex], [t].[ComplexTypeCollection], [t].[ParentComplexType], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [Ints], NULL AS [SugarGrams], NULL AS [ChildComplexType], NULL AS [LiltCO2], NULL AS [SugarGrams1], [t].[CaffeineGrams] AS [CaffeineGrams1], [t].[HasMilk], [t].[ChildComplexType] AS [ChildComplexType1], N'Tea' AS [Discriminator]
    FROM [Tea] AS [t]
) AS [u]
WHERE {{jsonValue}} = 8
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_derived_type(bool async)
    {
        await base.Filter_on_nested_complex_type_property_on_derived_type(async);

        var jsonValue = SqlServerTestEnvironment.IsAzureSql
            ? "JSON_VALUE([c].[ChildComplexType], '$.Nested.NestedInt' RETURNING int)"
            : "CAST(JSON_VALUE([c].[ChildComplexType], '$.Nested.NestedInt') AS int)";

        AssertSql(
            $$"""
SELECT [c].[Id], [c].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[Ints], [c].[SugarGrams], [c].[ComplexTypeCollection], [c].[ParentComplexType], [c].[ChildComplexType]
FROM [Coke] AS [c]
WHERE {{jsonValue}} = 58
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_base_type(bool async)
    {
        await base.Filter_on_nested_complex_type_property_on_base_type(async);

        var jsonValue = SqlServerTestEnvironment.IsAzureSql
            ? "JSON_VALUE([u].[ParentComplexType], '$.Nested.NestedInt' RETURNING int)"
            : "CAST(JSON_VALUE([u].[ParentComplexType], '$.Nested.NestedInt') AS int)";

        AssertSql(
            $$"""
SELECT [u].[Id], [u].[SortIndex], [u].[CaffeineGrams], [u].[CokeCO2], [u].[Ints], [u].[SugarGrams], [u].[LiltCO2], [u].[SugarGrams1], [u].[CaffeineGrams1], [u].[HasMilk], [u].[ComplexTypeCollection], [u].[ParentComplexType], [u].[ChildComplexType], [u].[ChildComplexType1], [u].[Discriminator]
FROM (
    SELECT [d].[Id], [d].[SortIndex], [d].[ComplexTypeCollection], [d].[ParentComplexType], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [Ints], NULL AS [SugarGrams], NULL AS [ChildComplexType], NULL AS [LiltCO2], NULL AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType1], N'Drink' AS [Discriminator]
    FROM [Drinks] AS [d]
    UNION ALL
    SELECT [c].[Id], [c].[SortIndex], [c].[ComplexTypeCollection], [c].[ParentComplexType], [c].[CaffeineGrams], [c].[CokeCO2], [c].[Ints], [c].[SugarGrams], [c].[ChildComplexType], NULL AS [LiltCO2], NULL AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType1], N'Coke' AS [Discriminator]
    FROM [Coke] AS [c]
    UNION ALL
    SELECT [l].[Id], [l].[SortIndex], [l].[ComplexTypeCollection], [l].[ParentComplexType], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [Ints], NULL AS [SugarGrams], NULL AS [ChildComplexType], [l].[LiltCO2], [l].[SugarGrams] AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType1], N'Lilt' AS [Discriminator]
    FROM [Lilt] AS [l]
    UNION ALL
    SELECT [t].[Id], [t].[SortIndex], [t].[ComplexTypeCollection], [t].[ParentComplexType], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [Ints], NULL AS [SugarGrams], NULL AS [ChildComplexType], NULL AS [LiltCO2], NULL AS [SugarGrams1], [t].[CaffeineGrams] AS [CaffeineGrams1], [t].[HasMilk], [t].[ChildComplexType] AS [ChildComplexType1], N'Tea' AS [Discriminator]
    FROM [Tea] AS [t]
) AS [u]
WHERE {{jsonValue}} = 50
""");
    }

    public override async Task Project_complex_type_on_derived_type(bool async)
    {
        await base.Project_complex_type_on_derived_type(async);

        AssertSql(
            """
SELECT [c].[ChildComplexType]
FROM [Coke] AS [c]
""");
    }

    public override async Task Project_complex_type_on_base_type(bool async)
    {
        await base.Project_complex_type_on_base_type(async);

        AssertSql(
            """
SELECT [u].[ParentComplexType]
FROM (
    SELECT [d].[ParentComplexType]
    FROM [Drinks] AS [d]
    UNION ALL
    SELECT [c].[ParentComplexType]
    FROM [Coke] AS [c]
    UNION ALL
    SELECT [l].[ParentComplexType]
    FROM [Lilt] AS [l]
    UNION ALL
    SELECT [t].[ParentComplexType]
    FROM [Tea] AS [t]
) AS [u]
""");
    }

    public override async Task Project_nested_complex_type_on_derived_type(bool async)
    {
        await base.Project_nested_complex_type_on_derived_type(async);

        AssertSql(
            """
SELECT JSON_QUERY([c].[ChildComplexType], '$.Nested')
FROM [Coke] AS [c]
""");
    }

    public override async Task Project_nested_complex_type_on_base_type(bool async)
    {
        await base.Project_nested_complex_type_on_base_type(async);

        AssertSql(
            """
SELECT JSON_QUERY([u].[ParentComplexType], '$.Nested')
FROM (
    SELECT [d].[ParentComplexType]
    FROM [Drinks] AS [d]
    UNION ALL
    SELECT [c].[ParentComplexType]
    FROM [Coke] AS [c]
    UNION ALL
    SELECT [l].[ParentComplexType]
    FROM [Lilt] AS [l]
    UNION ALL
    SELECT [t].[ParentComplexType]
    FROM [Tea] AS [t]
) AS [u]
""");
    }

    public override async Task Subquery_over_complex_collection(bool async)
    {
        await base.Subquery_over_complex_collection(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[SortIndex], [u].[CaffeineGrams], [u].[CokeCO2], [u].[Ints], [u].[SugarGrams], [u].[LiltCO2], [u].[SugarGrams1], [u].[CaffeineGrams1], [u].[HasMilk], [u].[ComplexTypeCollection], [u].[ParentComplexType], [u].[ChildComplexType], [u].[ChildComplexType1], [u].[Discriminator]
FROM (
    SELECT [d].[Id], [d].[SortIndex], [d].[ComplexTypeCollection], [d].[ParentComplexType], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [Ints], NULL AS [SugarGrams], NULL AS [ChildComplexType], NULL AS [LiltCO2], NULL AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType1], N'Drink' AS [Discriminator]
    FROM [Drinks] AS [d]
    UNION ALL
    SELECT [c].[Id], [c].[SortIndex], [c].[ComplexTypeCollection], [c].[ParentComplexType], [c].[CaffeineGrams], [c].[CokeCO2], [c].[Ints], [c].[SugarGrams], [c].[ChildComplexType], NULL AS [LiltCO2], NULL AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType1], N'Coke' AS [Discriminator]
    FROM [Coke] AS [c]
    UNION ALL
    SELECT [l].[Id], [l].[SortIndex], [l].[ComplexTypeCollection], [l].[ParentComplexType], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [Ints], NULL AS [SugarGrams], NULL AS [ChildComplexType], [l].[LiltCO2], [l].[SugarGrams] AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType1], N'Lilt' AS [Discriminator]
    FROM [Lilt] AS [l]
    UNION ALL
    SELECT [t].[Id], [t].[SortIndex], [t].[ComplexTypeCollection], [t].[ParentComplexType], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [Ints], NULL AS [SugarGrams], NULL AS [ChildComplexType], NULL AS [LiltCO2], NULL AS [SugarGrams1], [t].[CaffeineGrams] AS [CaffeineGrams1], [t].[HasMilk], [t].[ChildComplexType] AS [ChildComplexType1], N'Tea' AS [Discriminator]
    FROM [Tea] AS [t]
) AS [u]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([u].[ComplexTypeCollection], '$') WITH ([Int] int '$.Int') AS [c0]
    WHERE [c0].[Int] > 59) = 2
""");
    }

    public override async Task Owned_reference_with_complex_type_on_base_and_derived_types(bool async)
    {
        await base.Owned_reference_with_complex_type_on_base_and_derived_types(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[Discriminator], [m0].[ManufacturerId], [m0].[Name], [m0].[Address], [b0].[Id], [b0].[Brewhouse_Name], [b0].[Brewhouse_Address]
FROM (
    SELECT [m].[Id], [m].[Name], N'Manufacturer' AS [Discriminator]
    FROM [Manufacturer] AS [m]
    UNION ALL
    SELECT [b].[Id], [b].[Name], N'Brewery' AS [Discriminator]
    FROM [Brewery] AS [b]
) AS [u]
LEFT JOIN [ManufacturerHeadquarters] AS [m0] ON [u].[Id] = [m0].[ManufacturerId]
LEFT JOIN [Brewery] AS [b0] ON [u].[Id] = [b0].[Id]
""");
    }

    public override async Task Filter_on_complex_type_property_of_owned_reference_on_base_type(bool async)
    {
        await base.Filter_on_complex_type_property_of_owned_reference_on_base_type(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[Name], [u].[Discriminator], [m0].[ManufacturerId], [m0].[Name], [m0].[Address], [b0].[Id], [b0].[Brewhouse_Name], [b0].[Brewhouse_Address]
FROM (
    SELECT [m].[Id], [m].[Name], N'Manufacturer' AS [Discriminator]
    FROM [Manufacturer] AS [m]
    UNION ALL
    SELECT [b].[Id], [b].[Name], N'Brewery' AS [Discriminator]
    FROM [Brewery] AS [b]
) AS [u]
LEFT JOIN [ManufacturerHeadquarters] AS [m0] ON [u].[Id] = [m0].[ManufacturerId]
LEFT JOIN [Brewery] AS [b0] ON [u].[Id] = [b0].[Id]
WHERE JSON_VALUE([m0].[Address], '$.Country.Code') = N'DE'
""");
    }

    public override async Task Filter_on_complex_type_property_of_owned_reference_on_derived_type(bool async)
    {
        await base.Filter_on_complex_type_property_of_owned_reference_on_derived_type(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [m].[ManufacturerId], [m].[Name], [m].[Address], [b].[Id], [b].[Brewhouse_Name], [b].[Brewhouse_Address]
FROM [Brewery] AS [b]
LEFT JOIN [ManufacturerHeadquarters] AS [m] ON [b].[Id] = [m].[ManufacturerId]
WHERE JSON_VALUE([b].[Brewhouse_Address], '$.Country.Code') = N'DE'
""");
    }

    public override async Task Project_complex_type_of_owned_reference_on_base_type(bool async)
    {
        await base.Project_complex_type_of_owned_reference_on_base_type(async);

        AssertSql(
            """
SELECT [m0].[Address]
FROM (
    SELECT [m].[Id]
    FROM [Manufacturer] AS [m]
    UNION ALL
    SELECT [b].[Id]
    FROM [Brewery] AS [b]
) AS [u]
LEFT JOIN [ManufacturerHeadquarters] AS [m0] ON [u].[Id] = [m0].[ManufacturerId]
""");
    }

    public override async Task Project_complex_type_of_owned_reference_on_derived_type(bool async)
    {
        await base.Project_complex_type_of_owned_reference_on_derived_type(async);

        AssertSql(
            """
SELECT [b0].[Brewhouse_Address]
FROM (
    SELECT [b].[Id]
    FROM [Brewery] AS [b]
) AS [u]
LEFT JOIN [Brewery] AS [b0] ON [u].[Id] = [b0].[Id]
""");
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
