// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class TPCInheritanceJsonQuerySqlServerTest(TPCInheritanceJsonQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : TPCInheritanceJsonQueryRelationalTestBase<TPCInheritanceJsonQuerySqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Filter_on_complex_type_property_on_derived_type(bool async)
    {
        await base.Filter_on_complex_type_property_on_derived_type(async);

        AssertSql(
            """
SELECT [c].[Id], [c].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[SugarGrams], [c].[ComplexTypeCollection], [c].[ParentComplexType], [c].[ChildComplexType]
FROM [Coke] AS [c]
WHERE CAST(JSON_VALUE([c].[ChildComplexType], '$.Int') AS int) = 10
""");
    }

    public override async Task Filter_on_complex_type_property_on_base_type(bool async)
    {
        await base.Filter_on_complex_type_property_on_base_type(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[SortIndex], [u].[CaffeineGrams], [u].[CokeCO2], [u].[SugarGrams], [u].[LiltCO2], [u].[SugarGrams1], [u].[CaffeineGrams1], [u].[HasMilk], [u].[ComplexTypeCollection], [u].[ParentComplexType], [u].[ChildComplexType], [u].[ChildComplexType1], [u].[Discriminator]
FROM (
    SELECT [d].[Id], [d].[SortIndex], [d].[ComplexTypeCollection], [d].[ParentComplexType], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [SugarGrams], NULL AS [ChildComplexType], NULL AS [LiltCO2], NULL AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType1], N'Drink' AS [Discriminator]
    FROM [Drinks] AS [d]
    UNION ALL
    SELECT [c].[Id], [c].[SortIndex], [c].[ComplexTypeCollection], [c].[ParentComplexType], [c].[CaffeineGrams], [c].[CokeCO2], [c].[SugarGrams], [c].[ChildComplexType], NULL AS [LiltCO2], NULL AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType1], N'Coke' AS [Discriminator]
    FROM [Coke] AS [c]
    UNION ALL
    SELECT [l].[Id], [l].[SortIndex], [l].[ComplexTypeCollection], [l].[ParentComplexType], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [SugarGrams], NULL AS [ChildComplexType], [l].[LiltCO2], [l].[SugarGrams] AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType1], N'Lilt' AS [Discriminator]
    FROM [Lilt] AS [l]
    UNION ALL
    SELECT [t].[Id], [t].[SortIndex], [t].[ComplexTypeCollection], [t].[ParentComplexType], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [SugarGrams], NULL AS [ChildComplexType], NULL AS [LiltCO2], NULL AS [SugarGrams1], [t].[CaffeineGrams] AS [CaffeineGrams1], [t].[HasMilk], [t].[ChildComplexType] AS [ChildComplexType1], N'Tea' AS [Discriminator]
    FROM [Tea] AS [t]
) AS [u]
WHERE CAST(JSON_VALUE([u].[ParentComplexType], '$.Int') AS int) = 8
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_derived_type(bool async)
    {
        await base.Filter_on_nested_complex_type_property_on_derived_type(async);

        AssertSql(
            """
SELECT [c].[Id], [c].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[SugarGrams], [c].[ComplexTypeCollection], [c].[ParentComplexType], [c].[ChildComplexType]
FROM [Coke] AS [c]
WHERE CAST(JSON_VALUE([c].[ChildComplexType], '$.Nested.NestedInt') AS int) = 58
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_base_type(bool async)
    {
        await base.Filter_on_nested_complex_type_property_on_base_type(async);

        AssertSql(
            """
SELECT [u].[Id], [u].[SortIndex], [u].[CaffeineGrams], [u].[CokeCO2], [u].[SugarGrams], [u].[LiltCO2], [u].[SugarGrams1], [u].[CaffeineGrams1], [u].[HasMilk], [u].[ComplexTypeCollection], [u].[ParentComplexType], [u].[ChildComplexType], [u].[ChildComplexType1], [u].[Discriminator]
FROM (
    SELECT [d].[Id], [d].[SortIndex], [d].[ComplexTypeCollection], [d].[ParentComplexType], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [SugarGrams], NULL AS [ChildComplexType], NULL AS [LiltCO2], NULL AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType1], N'Drink' AS [Discriminator]
    FROM [Drinks] AS [d]
    UNION ALL
    SELECT [c].[Id], [c].[SortIndex], [c].[ComplexTypeCollection], [c].[ParentComplexType], [c].[CaffeineGrams], [c].[CokeCO2], [c].[SugarGrams], [c].[ChildComplexType], NULL AS [LiltCO2], NULL AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType1], N'Coke' AS [Discriminator]
    FROM [Coke] AS [c]
    UNION ALL
    SELECT [l].[Id], [l].[SortIndex], [l].[ComplexTypeCollection], [l].[ParentComplexType], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [SugarGrams], NULL AS [ChildComplexType], [l].[LiltCO2], [l].[SugarGrams] AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType1], N'Lilt' AS [Discriminator]
    FROM [Lilt] AS [l]
    UNION ALL
    SELECT [t].[Id], [t].[SortIndex], [t].[ComplexTypeCollection], [t].[ParentComplexType], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [SugarGrams], NULL AS [ChildComplexType], NULL AS [LiltCO2], NULL AS [SugarGrams1], [t].[CaffeineGrams] AS [CaffeineGrams1], [t].[HasMilk], [t].[ChildComplexType] AS [ChildComplexType1], N'Tea' AS [Discriminator]
    FROM [Tea] AS [t]
) AS [u]
WHERE CAST(JSON_VALUE([u].[ParentComplexType], '$.Nested.NestedInt') AS int) = 50
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
SELECT [u].[Id], [u].[SortIndex], [u].[CaffeineGrams], [u].[CokeCO2], [u].[SugarGrams], [u].[LiltCO2], [u].[SugarGrams1], [u].[CaffeineGrams1], [u].[HasMilk], [u].[ComplexTypeCollection], [u].[ParentComplexType], [u].[ChildComplexType], [u].[ChildComplexType1], [u].[Discriminator]
FROM (
    SELECT [d].[Id], [d].[SortIndex], [d].[ComplexTypeCollection], [d].[ParentComplexType], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [SugarGrams], NULL AS [ChildComplexType], NULL AS [LiltCO2], NULL AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType1], N'Drink' AS [Discriminator]
    FROM [Drinks] AS [d]
    UNION ALL
    SELECT [c].[Id], [c].[SortIndex], [c].[ComplexTypeCollection], [c].[ParentComplexType], [c].[CaffeineGrams], [c].[CokeCO2], [c].[SugarGrams], [c].[ChildComplexType], NULL AS [LiltCO2], NULL AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType1], N'Coke' AS [Discriminator]
    FROM [Coke] AS [c]
    UNION ALL
    SELECT [l].[Id], [l].[SortIndex], [l].[ComplexTypeCollection], [l].[ParentComplexType], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [SugarGrams], NULL AS [ChildComplexType], [l].[LiltCO2], [l].[SugarGrams] AS [SugarGrams1], NULL AS [CaffeineGrams1], NULL AS [HasMilk], NULL AS [ChildComplexType1], N'Lilt' AS [Discriminator]
    FROM [Lilt] AS [l]
    UNION ALL
    SELECT [t].[Id], [t].[SortIndex], [t].[ComplexTypeCollection], [t].[ParentComplexType], NULL AS [CaffeineGrams], NULL AS [CokeCO2], NULL AS [SugarGrams], NULL AS [ChildComplexType], NULL AS [LiltCO2], NULL AS [SugarGrams1], [t].[CaffeineGrams] AS [CaffeineGrams1], [t].[HasMilk], [t].[ChildComplexType] AS [ChildComplexType1], N'Tea' AS [Discriminator]
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
