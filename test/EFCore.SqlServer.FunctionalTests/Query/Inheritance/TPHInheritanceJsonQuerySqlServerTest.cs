// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class TPHInheritanceJsonQuerySqlServerTest(TPHInheritanceJsonQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : TPHInheritanceJsonQueryRelationalTestBase<TPHInheritanceJsonQuerySqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Filter_on_complex_type_property_on_derived_type(bool async)
    {
        await base.Filter_on_complex_type_property_on_derived_type(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Discriminator], [d].[SortIndex], [d].[CaffeineGrams], [d].[CokeCO2], [d].[Ints], [d].[SugarGrams], [d].[ComplexTypeCollection], [d].[ParentComplexType], [d].[ChildComplexType]
FROM [Drinks] AS [d]
WHERE [d].[Discriminator] = 1 AND CAST(JSON_VALUE([d].[ChildComplexType], '$.Int') AS int) = 10
""");
    }

    public override async Task Filter_on_complex_type_property_on_base_type(bool async)
    {
        await base.Filter_on_complex_type_property_on_base_type(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Discriminator], [d].[SortIndex], [d].[CaffeineGrams], [d].[CokeCO2], [d].[Ints], [d].[SugarGrams], [d].[LiltCO2], [d].[HasMilk], [d].[ComplexTypeCollection], [d].[ParentComplexType], [d].[ChildComplexType], [d].[ChildComplexType]
FROM [Drinks] AS [d]
WHERE CAST(JSON_VALUE([d].[ParentComplexType], '$.Int') AS int) = 8
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_derived_type(bool async)
    {
        await base.Filter_on_nested_complex_type_property_on_derived_type(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Discriminator], [d].[SortIndex], [d].[CaffeineGrams], [d].[CokeCO2], [d].[Ints], [d].[SugarGrams], [d].[ComplexTypeCollection], [d].[ParentComplexType], [d].[ChildComplexType]
FROM [Drinks] AS [d]
WHERE [d].[Discriminator] = 1 AND CAST(JSON_VALUE([d].[ChildComplexType], '$.Nested.NestedInt') AS int) = 58
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_base_type(bool async)
    {
        await base.Filter_on_nested_complex_type_property_on_base_type(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Discriminator], [d].[SortIndex], [d].[CaffeineGrams], [d].[CokeCO2], [d].[Ints], [d].[SugarGrams], [d].[LiltCO2], [d].[HasMilk], [d].[ComplexTypeCollection], [d].[ParentComplexType], [d].[ChildComplexType], [d].[ChildComplexType]
FROM [Drinks] AS [d]
WHERE CAST(JSON_VALUE([d].[ParentComplexType], '$.Nested.NestedInt') AS int) = 50
""");
    }

    public override async Task Project_complex_type_on_derived_type(bool async)
    {
        await base.Project_complex_type_on_derived_type(async);

        AssertSql(
            """
SELECT [d].[ChildComplexType]
FROM [Drinks] AS [d]
WHERE [d].[Discriminator] = 1
""");
    }

    public override async Task Project_complex_type_on_base_type(bool async)
    {
        await base.Project_complex_type_on_base_type(async);

        AssertSql(
            """
SELECT [d].[ParentComplexType]
FROM [Drinks] AS [d]
""");
    }

    public override async Task Project_nested_complex_type_on_derived_type(bool async)
    {
        await base.Project_nested_complex_type_on_derived_type(async);

        AssertSql(
            """
SELECT JSON_QUERY([d].[ChildComplexType], '$.Nested')
FROM [Drinks] AS [d]
WHERE [d].[Discriminator] = 1
""");
    }

    public override async Task Project_nested_complex_type_on_base_type(bool async)
    {
        await base.Project_nested_complex_type_on_base_type(async);

        AssertSql(
            """
SELECT JSON_QUERY([d].[ParentComplexType], '$.Nested')
FROM [Drinks] AS [d]
""");
    }

    public override async Task Subquery_over_complex_collection(bool async)
    {
        await base.Subquery_over_complex_collection(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[Discriminator], [d].[SortIndex], [d].[CaffeineGrams], [d].[CokeCO2], [d].[Ints], [d].[SugarGrams], [d].[LiltCO2], [d].[HasMilk], [d].[ComplexTypeCollection], [d].[ParentComplexType], [d].[ChildComplexType], [d].[ChildComplexType]
FROM [Drinks] AS [d]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([d].[ComplexTypeCollection], '$') WITH ([Int] int '$.Int') AS [c]
    WHERE [c].[Int] > 59) = 2
""");
    }

    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
