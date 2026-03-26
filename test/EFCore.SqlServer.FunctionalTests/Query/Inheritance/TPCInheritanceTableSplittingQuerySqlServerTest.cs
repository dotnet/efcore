// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class TPCInheritanceTableSplittingQuerySqlServerTest(TPCInheritanceQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : TPCInheritanceTableSplittingQueryRelationalTestBase<TPCInheritanceQuerySqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Filter_on_complex_type_property_on_derived_type(bool async)
    {
        await base.Filter_on_complex_type_property_on_derived_type(async);

        AssertSql(
            """
SELECT [c].[Id], [c].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[Ints], [c].[SugarGrams], [c].[ComplexTypeCollection], [c].[Int], [c].[UniqueInt], [c].[NestedInt], [c].[NestedComplexType_UniqueInt], [c].[ChildComplexType_Int], [c].[ChildComplexType_UniqueInt], [c].[ChildComplexType_Nested_NestedInt], [c].[ChildComplexType_Nested_UniqueInt]
FROM [Coke] AS [c]
WHERE [c].[ChildComplexType_Int] = 10
""");
    }

    public override async Task Filter_on_complex_type_property_on_base_type(bool async)
    {
        await base.Filter_on_complex_type_property_on_base_type(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[SortIndex], [u].[CaffeineGrams], [u].[CokeCO2], [u].[Ints], [u].[SugarGrams], [u].[LiltCO2], [u].[SugarGrams1], [u].[CaffeineGrams1], [u].[HasMilk], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueInt], [u].[ParentComplexType_Nested_NestedInt], [u].[ParentComplexType_Nested_UniqueInt], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueInt], [u].[ChildComplexType_Nested_NestedInt], [u].[ChildComplexType_Nested_UniqueInt], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueInt1], [u].[ChildComplexType_Nested_NestedInt1], [u].[ChildComplexType_Nested_UniqueInt1], [u].[Discriminator]
FROM (
    SELECT [d].[Id], [d].[SortIndex], [d].[ComplexTypeCollection], [d].[ParentComplexType_Int], [d].[ParentComplexType_UniqueInt], [d].[ParentComplexType_Nested_NestedInt], [d].[ParentComplexType_Nested_UniqueInt], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [Ints], NULL AS [SugarGrams], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueInt], NULL AS [ChildComplexType_Nested_NestedInt], NULL AS [ChildComplexType_Nested_UniqueInt], NULL AS [LiltCO2], NULL AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueInt1], NULL AS [ChildComplexType_Nested_NestedInt1], NULL AS [ChildComplexType_Nested_UniqueInt1], N'Drink' AS [Discriminator]
    FROM [Drinks] AS [d]
    UNION ALL
    SELECT [c].[Id], [c].[SortIndex], [c].[ComplexTypeCollection], [c].[Int] AS [ParentComplexType_Int], [c].[UniqueInt] AS [ParentComplexType_UniqueInt], [c].[NestedInt] AS [ParentComplexType_Nested_NestedInt], [c].[NestedComplexType_UniqueInt] AS [ParentComplexType_Nested_UniqueInt], [c].[CaffeineGrams], [c].[CokeCO2], [c].[Ints], [c].[SugarGrams], [c].[ChildComplexType_Int], [c].[ChildComplexType_UniqueInt], [c].[ChildComplexType_Nested_NestedInt], [c].[ChildComplexType_Nested_UniqueInt], NULL AS [LiltCO2], NULL AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueInt1], NULL AS [ChildComplexType_Nested_NestedInt1], NULL AS [ChildComplexType_Nested_UniqueInt1], N'Coke' AS [Discriminator]
    FROM [Coke] AS [c]
    UNION ALL
    SELECT [l].[Id], [l].[SortIndex], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[UniqueInt] AS [ParentComplexType_UniqueInt], [l].[NestedInt] AS [ParentComplexType_Nested_NestedInt], [l].[NestedComplexType_UniqueInt] AS [ParentComplexType_Nested_UniqueInt], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [Ints], NULL AS [SugarGrams], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueInt], NULL AS [ChildComplexType_Nested_NestedInt], NULL AS [ChildComplexType_Nested_UniqueInt], [l].[LiltCO2], [l].[SugarGrams] AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueInt1], NULL AS [ChildComplexType_Nested_NestedInt1], NULL AS [ChildComplexType_Nested_UniqueInt1], N'Lilt' AS [Discriminator]
    FROM [Lilt] AS [l]
    UNION ALL
    SELECT [t].[Id], [t].[SortIndex], [t].[ComplexTypeCollection], [t].[Int] AS [ParentComplexType_Int], [t].[UniqueInt] AS [ParentComplexType_UniqueInt], [t].[NestedInt] AS [ParentComplexType_Nested_NestedInt], [t].[NestedComplexType_UniqueInt] AS [ParentComplexType_Nested_UniqueInt], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [Ints], NULL AS [SugarGrams], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueInt], NULL AS [ChildComplexType_Nested_NestedInt], NULL AS [ChildComplexType_Nested_UniqueInt], NULL AS [LiltCO2], NULL AS [SugarGrams1], [t].[CaffeineGrams] AS [CaffeineGrams1], [t].[HasMilk], [t].[ChildComplexType_Int] AS [ChildComplexType_Int1], [t].[ChildComplexType_UniqueInt] AS [ChildComplexType_UniqueInt1], [t].[ChildComplexType_Nested_NestedInt] AS [ChildComplexType_Nested_NestedInt1], [t].[ChildComplexType_Nested_UniqueInt] AS [ChildComplexType_Nested_UniqueInt1], N'Tea' AS [Discriminator]
    FROM [Tea] AS [t]
) AS [u]
WHERE [u].[ParentComplexType_Int] = 8
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_derived_type(bool async)
    {
        await base.Filter_on_nested_complex_type_property_on_derived_type(async);

        AssertSql(
            """
SELECT [c].[Id], [c].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[Ints], [c].[SugarGrams], [c].[ComplexTypeCollection], [c].[Int], [c].[UniqueInt], [c].[NestedInt], [c].[NestedComplexType_UniqueInt], [c].[ChildComplexType_Int], [c].[ChildComplexType_UniqueInt], [c].[ChildComplexType_Nested_NestedInt], [c].[ChildComplexType_Nested_UniqueInt]
FROM [Coke] AS [c]
WHERE [c].[ChildComplexType_Nested_NestedInt] = 58
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_base_type(bool async)
    {
        await base.Filter_on_nested_complex_type_property_on_base_type(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[SortIndex], [u].[CaffeineGrams], [u].[CokeCO2], [u].[Ints], [u].[SugarGrams], [u].[LiltCO2], [u].[SugarGrams1], [u].[CaffeineGrams1], [u].[HasMilk], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueInt], [u].[ParentComplexType_Nested_NestedInt], [u].[ParentComplexType_Nested_UniqueInt], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueInt], [u].[ChildComplexType_Nested_NestedInt], [u].[ChildComplexType_Nested_UniqueInt], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueInt1], [u].[ChildComplexType_Nested_NestedInt1], [u].[ChildComplexType_Nested_UniqueInt1], [u].[Discriminator]
FROM (
    SELECT [d].[Id], [d].[SortIndex], [d].[ComplexTypeCollection], [d].[ParentComplexType_Int], [d].[ParentComplexType_UniqueInt], [d].[ParentComplexType_Nested_NestedInt], [d].[ParentComplexType_Nested_UniqueInt], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [Ints], NULL AS [SugarGrams], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueInt], NULL AS [ChildComplexType_Nested_NestedInt], NULL AS [ChildComplexType_Nested_UniqueInt], NULL AS [LiltCO2], NULL AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueInt1], NULL AS [ChildComplexType_Nested_NestedInt1], NULL AS [ChildComplexType_Nested_UniqueInt1], N'Drink' AS [Discriminator]
    FROM [Drinks] AS [d]
    UNION ALL
    SELECT [c].[Id], [c].[SortIndex], [c].[ComplexTypeCollection], [c].[Int] AS [ParentComplexType_Int], [c].[UniqueInt] AS [ParentComplexType_UniqueInt], [c].[NestedInt] AS [ParentComplexType_Nested_NestedInt], [c].[NestedComplexType_UniqueInt] AS [ParentComplexType_Nested_UniqueInt], [c].[CaffeineGrams], [c].[CokeCO2], [c].[Ints], [c].[SugarGrams], [c].[ChildComplexType_Int], [c].[ChildComplexType_UniqueInt], [c].[ChildComplexType_Nested_NestedInt], [c].[ChildComplexType_Nested_UniqueInt], NULL AS [LiltCO2], NULL AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueInt1], NULL AS [ChildComplexType_Nested_NestedInt1], NULL AS [ChildComplexType_Nested_UniqueInt1], N'Coke' AS [Discriminator]
    FROM [Coke] AS [c]
    UNION ALL
    SELECT [l].[Id], [l].[SortIndex], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[UniqueInt] AS [ParentComplexType_UniqueInt], [l].[NestedInt] AS [ParentComplexType_Nested_NestedInt], [l].[NestedComplexType_UniqueInt] AS [ParentComplexType_Nested_UniqueInt], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [Ints], NULL AS [SugarGrams], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueInt], NULL AS [ChildComplexType_Nested_NestedInt], NULL AS [ChildComplexType_Nested_UniqueInt], [l].[LiltCO2], [l].[SugarGrams] AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueInt1], NULL AS [ChildComplexType_Nested_NestedInt1], NULL AS [ChildComplexType_Nested_UniqueInt1], N'Lilt' AS [Discriminator]
    FROM [Lilt] AS [l]
    UNION ALL
    SELECT [t].[Id], [t].[SortIndex], [t].[ComplexTypeCollection], [t].[Int] AS [ParentComplexType_Int], [t].[UniqueInt] AS [ParentComplexType_UniqueInt], [t].[NestedInt] AS [ParentComplexType_Nested_NestedInt], [t].[NestedComplexType_UniqueInt] AS [ParentComplexType_Nested_UniqueInt], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [Ints], NULL AS [SugarGrams], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueInt], NULL AS [ChildComplexType_Nested_NestedInt], NULL AS [ChildComplexType_Nested_UniqueInt], NULL AS [LiltCO2], NULL AS [SugarGrams1], [t].[CaffeineGrams] AS [CaffeineGrams1], [t].[HasMilk], [t].[ChildComplexType_Int] AS [ChildComplexType_Int1], [t].[ChildComplexType_UniqueInt] AS [ChildComplexType_UniqueInt1], [t].[ChildComplexType_Nested_NestedInt] AS [ChildComplexType_Nested_NestedInt1], [t].[ChildComplexType_Nested_UniqueInt] AS [ChildComplexType_Nested_UniqueInt1], N'Tea' AS [Discriminator]
    FROM [Tea] AS [t]
) AS [u]
WHERE [u].[ParentComplexType_Nested_NestedInt] = 50
""");
    }

    public override async Task Project_complex_type_on_derived_type(bool async)
    {
        await base.Project_complex_type_on_derived_type(async);

        AssertSql(
            """
SELECT [c].[ChildComplexType_Int], [c].[ChildComplexType_UniqueInt], [c].[ChildComplexType_Nested_NestedInt], [c].[ChildComplexType_Nested_UniqueInt]
FROM [Coke] AS [c]
""");
    }

    public override async Task Project_complex_type_on_base_type(bool async)
    {
        await base.Project_complex_type_on_base_type(async);

        AssertSql(
            """
SELECT [d].[ParentComplexType_Int], [d].[ParentComplexType_UniqueInt], [d].[ParentComplexType_Nested_NestedInt], [d].[ParentComplexType_Nested_UniqueInt]
FROM [Drinks] AS [d]
UNION ALL
SELECT [c].[Int] AS [ParentComplexType_Int], [c].[UniqueInt] AS [ParentComplexType_UniqueInt], [c].[NestedInt] AS [ParentComplexType_Nested_NestedInt], [c].[NestedComplexType_UniqueInt] AS [ParentComplexType_Nested_UniqueInt]
FROM [Coke] AS [c]
UNION ALL
SELECT [l].[Int] AS [ParentComplexType_Int], [l].[UniqueInt] AS [ParentComplexType_UniqueInt], [l].[NestedInt] AS [ParentComplexType_Nested_NestedInt], [l].[NestedComplexType_UniqueInt] AS [ParentComplexType_Nested_UniqueInt]
FROM [Lilt] AS [l]
UNION ALL
SELECT [t].[Int] AS [ParentComplexType_Int], [t].[UniqueInt] AS [ParentComplexType_UniqueInt], [t].[NestedInt] AS [ParentComplexType_Nested_NestedInt], [t].[NestedComplexType_UniqueInt] AS [ParentComplexType_Nested_UniqueInt]
FROM [Tea] AS [t]
""");
    }

    public override async Task Project_nested_complex_type_on_derived_type(bool async)
    {
        await base.Project_nested_complex_type_on_derived_type(async);

        AssertSql(
            """
SELECT [c].[ChildComplexType_Nested_NestedInt], [c].[ChildComplexType_Nested_UniqueInt]
FROM [Coke] AS [c]
""");
    }

    public override async Task Project_nested_complex_type_on_base_type(bool async)
    {
        await base.Project_nested_complex_type_on_base_type(async);

        AssertSql(
            """
SELECT [d].[ParentComplexType_Nested_NestedInt], [d].[ParentComplexType_Nested_UniqueInt]
FROM [Drinks] AS [d]
UNION ALL
SELECT [c].[NestedInt] AS [ParentComplexType_Nested_NestedInt], [c].[NestedComplexType_UniqueInt] AS [ParentComplexType_Nested_UniqueInt]
FROM [Coke] AS [c]
UNION ALL
SELECT [l].[NestedInt] AS [ParentComplexType_Nested_NestedInt], [l].[NestedComplexType_UniqueInt] AS [ParentComplexType_Nested_UniqueInt]
FROM [Lilt] AS [l]
UNION ALL
SELECT [t].[NestedInt] AS [ParentComplexType_Nested_NestedInt], [t].[NestedComplexType_UniqueInt] AS [ParentComplexType_Nested_UniqueInt]
FROM [Tea] AS [t]
""");
    }

    public override async Task Subquery_over_complex_collection(bool async)
    {
        await base.Subquery_over_complex_collection(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[SortIndex], [u].[CaffeineGrams], [u].[CokeCO2], [u].[Ints], [u].[SugarGrams], [u].[LiltCO2], [u].[SugarGrams1], [u].[CaffeineGrams1], [u].[HasMilk], [u].[ComplexTypeCollection], [u].[ParentComplexType_Int], [u].[ParentComplexType_UniqueInt], [u].[ParentComplexType_Nested_NestedInt], [u].[ParentComplexType_Nested_UniqueInt], [u].[ChildComplexType_Int], [u].[ChildComplexType_UniqueInt], [u].[ChildComplexType_Nested_NestedInt], [u].[ChildComplexType_Nested_UniqueInt], [u].[ChildComplexType_Int1], [u].[ChildComplexType_UniqueInt1], [u].[ChildComplexType_Nested_NestedInt1], [u].[ChildComplexType_Nested_UniqueInt1], [u].[Discriminator]
FROM (
    SELECT [d].[Id], [d].[SortIndex], [d].[ComplexTypeCollection], [d].[ParentComplexType_Int], [d].[ParentComplexType_UniqueInt], [d].[ParentComplexType_Nested_NestedInt], [d].[ParentComplexType_Nested_UniqueInt], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [Ints], NULL AS [SugarGrams], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueInt], NULL AS [ChildComplexType_Nested_NestedInt], NULL AS [ChildComplexType_Nested_UniqueInt], NULL AS [LiltCO2], NULL AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueInt1], NULL AS [ChildComplexType_Nested_NestedInt1], NULL AS [ChildComplexType_Nested_UniqueInt1], N'Drink' AS [Discriminator]
    FROM [Drinks] AS [d]
    UNION ALL
    SELECT [c].[Id], [c].[SortIndex], [c].[ComplexTypeCollection], [c].[Int] AS [ParentComplexType_Int], [c].[UniqueInt] AS [ParentComplexType_UniqueInt], [c].[NestedInt] AS [ParentComplexType_Nested_NestedInt], [c].[NestedComplexType_UniqueInt] AS [ParentComplexType_Nested_UniqueInt], [c].[CaffeineGrams], [c].[CokeCO2], [c].[Ints], [c].[SugarGrams], [c].[ChildComplexType_Int], [c].[ChildComplexType_UniqueInt], [c].[ChildComplexType_Nested_NestedInt], [c].[ChildComplexType_Nested_UniqueInt], NULL AS [LiltCO2], NULL AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueInt1], NULL AS [ChildComplexType_Nested_NestedInt1], NULL AS [ChildComplexType_Nested_UniqueInt1], N'Coke' AS [Discriminator]
    FROM [Coke] AS [c]
    UNION ALL
    SELECT [l].[Id], [l].[SortIndex], [l].[ComplexTypeCollection], [l].[Int] AS [ParentComplexType_Int], [l].[UniqueInt] AS [ParentComplexType_UniqueInt], [l].[NestedInt] AS [ParentComplexType_Nested_NestedInt], [l].[NestedComplexType_UniqueInt] AS [ParentComplexType_Nested_UniqueInt], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [Ints], NULL AS [SugarGrams], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueInt], NULL AS [ChildComplexType_Nested_NestedInt], NULL AS [ChildComplexType_Nested_UniqueInt], [l].[LiltCO2], [l].[SugarGrams] AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType_Int1], NULL AS [ChildComplexType_UniqueInt1], NULL AS [ChildComplexType_Nested_NestedInt1], NULL AS [ChildComplexType_Nested_UniqueInt1], N'Lilt' AS [Discriminator]
    FROM [Lilt] AS [l]
    UNION ALL
    SELECT [t].[Id], [t].[SortIndex], [t].[ComplexTypeCollection], [t].[Int] AS [ParentComplexType_Int], [t].[UniqueInt] AS [ParentComplexType_UniqueInt], [t].[NestedInt] AS [ParentComplexType_Nested_NestedInt], [t].[NestedComplexType_UniqueInt] AS [ParentComplexType_Nested_UniqueInt], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [Ints], NULL AS [SugarGrams], NULL AS [ChildComplexType_Int], NULL AS [ChildComplexType_UniqueInt], NULL AS [ChildComplexType_Nested_NestedInt], NULL AS [ChildComplexType_Nested_UniqueInt], NULL AS [LiltCO2], NULL AS [SugarGrams1], [t].[CaffeineGrams] AS [CaffeineGrams1], [t].[HasMilk], [t].[ChildComplexType_Int] AS [ChildComplexType_Int1], [t].[ChildComplexType_UniqueInt] AS [ChildComplexType_UniqueInt1], [t].[ChildComplexType_Nested_NestedInt] AS [ChildComplexType_Nested_NestedInt1], [t].[ChildComplexType_Nested_UniqueInt] AS [ChildComplexType_Nested_UniqueInt1], N'Tea' AS [Discriminator]
    FROM [Tea] AS [t]
) AS [u]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([u].[ComplexTypeCollection], '$') WITH ([Int] int '$.Int') AS [c0]
    WHERE [c0].[Int] > 59) = 2
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
