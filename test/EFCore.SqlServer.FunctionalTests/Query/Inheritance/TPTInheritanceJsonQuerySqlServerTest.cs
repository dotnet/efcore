// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class TPTInheritanceJsonQuerySqlServerTest(TPTInheritanceJsonQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : TPTInheritanceJsonQueryRelationalTestBase<TPTInheritanceJsonQuerySqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Filter_on_complex_type_property_on_derived_type(bool async)
    {
        await base.Filter_on_complex_type_property_on_derived_type(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[Ints], [c].[SugarGrams], [d].[ComplexTypeCollection], [d].[ParentComplexType], [c].[ChildComplexType]
FROM [Drinks] AS [d]
INNER JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
WHERE CAST(JSON_VALUE([c].[ChildComplexType], '$.Int') AS int) = 10
""");
    }

    public override async Task Filter_on_complex_type_property_on_base_type(bool async)
    {
        await base.Filter_on_complex_type_property_on_base_type(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[Ints], [c].[SugarGrams], [l].[LiltCO2], [l].[SugarGrams], [t].[CaffeineGrams], [t].[HasMilk], [d].[ComplexTypeCollection], [d].[ParentComplexType], [c].[ChildComplexType], [t].[ChildComplexType], CASE
    WHEN [t].[Id] IS NOT NULL THEN N'Tea'
    WHEN [l].[Id] IS NOT NULL THEN N'Lilt'
    WHEN [c].[Id] IS NOT NULL THEN N'Coke'
END AS [Discriminator]
FROM [Drinks] AS [d]
LEFT JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
LEFT JOIN [Lilt] AS [l] ON [d].[Id] = [l].[Id]
LEFT JOIN [Tea] AS [t] ON [d].[Id] = [t].[Id]
WHERE CAST(JSON_VALUE([d].[ParentComplexType], '$.Int') AS int) = 8
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_derived_type(bool async)
    {
        await base.Filter_on_nested_complex_type_property_on_derived_type(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[Ints], [c].[SugarGrams], [d].[ComplexTypeCollection], [d].[ParentComplexType], [c].[ChildComplexType]
FROM [Drinks] AS [d]
INNER JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
WHERE CAST(JSON_VALUE([c].[ChildComplexType], '$.Nested.NestedInt') AS int) = 58
""");
    }

    public override async Task Filter_on_nested_complex_type_property_on_base_type(bool async)
    {
        await base.Filter_on_nested_complex_type_property_on_base_type(async);

        AssertSql(
            """
SELECT [d].[Id], [d].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[Ints], [c].[SugarGrams], [l].[LiltCO2], [l].[SugarGrams], [t].[CaffeineGrams], [t].[HasMilk], [d].[ComplexTypeCollection], [d].[ParentComplexType], [c].[ChildComplexType], [t].[ChildComplexType], CASE
    WHEN [t].[Id] IS NOT NULL THEN N'Tea'
    WHEN [l].[Id] IS NOT NULL THEN N'Lilt'
    WHEN [c].[Id] IS NOT NULL THEN N'Coke'
END AS [Discriminator]
FROM [Drinks] AS [d]
LEFT JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
LEFT JOIN [Lilt] AS [l] ON [d].[Id] = [l].[Id]
LEFT JOIN [Tea] AS [t] ON [d].[Id] = [t].[Id]
WHERE CAST(JSON_VALUE([d].[ParentComplexType], '$.Nested.NestedInt') AS int) = 50
""");
    }

    public override async Task Project_complex_type_on_derived_type(bool async)
    {
        await base.Project_complex_type_on_derived_type(async);

        AssertSql(
            """
SELECT [c].[ChildComplexType]
FROM [Drinks] AS [d]
INNER JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
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
SELECT JSON_QUERY([c].[ChildComplexType], '$.Nested')
FROM [Drinks] AS [d]
INNER JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
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
SELECT [d].[Id], [d].[SortIndex], [c].[CaffeineGrams], [c].[CokeCO2], [c].[Ints], [c].[SugarGrams], [l].[LiltCO2], [l].[SugarGrams], [t].[CaffeineGrams], [t].[HasMilk], [d].[ComplexTypeCollection], [d].[ParentComplexType], [c].[ChildComplexType], [t].[ChildComplexType], CASE
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

    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
