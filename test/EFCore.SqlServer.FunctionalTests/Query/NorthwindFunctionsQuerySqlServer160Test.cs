// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

[SqlServerCondition(SqlServerCondition.SupportsFunctions2022)]
public class NorthwindFunctionsQuerySqlServer160Test
    : NorthwindFunctionsQueryRelationalTestBase<NorthwindFunctionsQuerySqlServer160Test.Fixture160>
{
    public NorthwindFunctionsQuerySqlServer160Test(Fixture160 fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Client_evaluation_of_uncorrelated_method_call(bool async)
    {
        await base.Client_evaluation_of_uncorrelated_method_call(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE [o].[UnitPrice] < 7.0 AND 10 < [o].[ProductID]
""");
    }

    public override async Task Sum_over_round_works_correctly_in_projection(bool async)
    {
        await base.Sum_over_round_works_correctly_in_projection(async);

        AssertSql(
            """
SELECT [o].[OrderID], (
    SELECT COALESCE(SUM(ROUND([o0].[UnitPrice], 2)), 0.0)
    FROM [Order Details] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) AS [Sum]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300
""");
    }

    public override async Task Sum_over_round_works_correctly_in_projection_2(bool async)
    {
        await base.Sum_over_round_works_correctly_in_projection_2(async);

        AssertSql(
            """
SELECT [o].[OrderID], (
    SELECT COALESCE(SUM(ROUND([o0].[UnitPrice] * [o0].[UnitPrice], 2)), 0.0)
    FROM [Order Details] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) AS [Sum]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300
""");
    }

    public override async Task Sum_over_truncate_works_correctly_in_projection(bool async)
    {
        await base.Sum_over_truncate_works_correctly_in_projection(async);

        AssertSql(
            """
SELECT [o].[OrderID], (
    SELECT COALESCE(SUM(ROUND([o0].[UnitPrice], 0, 1)), 0.0)
    FROM [Order Details] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) AS [Sum]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300
""");
    }

    public override async Task Sum_over_truncate_works_correctly_in_projection_2(bool async)
    {
        await base.Sum_over_truncate_works_correctly_in_projection_2(async);

        AssertSql(
            """
SELECT [o].[OrderID], (
    SELECT COALESCE(SUM(ROUND([o0].[UnitPrice] * [o0].[UnitPrice], 0, 1)), 0.0)
    FROM [Order Details] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) AS [Sum]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300
""");
    }

    public override async Task Where_functions_nested(bool async)
    {
        await base.Where_functions_nested(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE POWER(CAST(CAST(LEN([c].[CustomerID]) AS int) AS float), 2.0E0) = 25.0E0
""");
    }

    public override async Task Order_by_length_twice(bool async)
    {
        await base.Order_by_length_twice(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY CAST(LEN([c].[CustomerID]) AS int), [c].[CustomerID]
""");
    }

    public override async Task Order_by_length_twice_followed_by_projection_of_naked_collection_navigation(bool async)
    {
        await base.Order_by_length_twice_followed_by_projection_of_naked_collection_navigation(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY CAST(LEN([c].[CustomerID]) AS int), [c].[CustomerID]
""");
    }

    public override async Task Static_equals_nullable_datetime_compared_to_non_nullable(bool async)
    {
        await base.Static_equals_nullable_datetime_compared_to_non_nullable(async);

        AssertSql(
            """
@arg='1996-07-04T00:00:00.0000000' (DbType = DateTime)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] = @arg
""");
    }

    public override async Task Static_equals_int_compared_to_long(bool async)
    {
        await base.Static_equals_int_compared_to_long(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE 0 = 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task StandardDeviation(bool async)
    {
        await using var ctx = CreateContext();

        var query = ctx.Set<OrderDetail>()
            .GroupBy(od => od.ProductID)
            .Select(
                g => new
                {
                    ProductID = g.Key,
                    SampleStandardDeviation = EF.Functions.StandardDeviationSample(g.Select(od => od.UnitPrice)),
                    PopulationStandardDeviation = EF.Functions.StandardDeviationPopulation(g.Select(od => od.UnitPrice))
                });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        var product9 = results.Single(r => r.ProductID == 9);
        Assert.Equal(8.675943752699023, product9.SampleStandardDeviation.Value, 5);
        Assert.Equal(7.759999999999856, product9.PopulationStandardDeviation.Value, 5);

        AssertSql(
            """
SELECT [o].[ProductID], STDEV([o].[UnitPrice]) AS [SampleStandardDeviation], STDEVP([o].[UnitPrice]) AS [PopulationStandardDeviation]
FROM [Order Details] AS [o]
GROUP BY [o].[ProductID]
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Variance(bool async)
    {
        await using var ctx = CreateContext();

        var query = ctx.Set<OrderDetail>()
            .GroupBy(od => od.ProductID)
            .Select(
                g => new
                {
                    ProductID = g.Key,
                    SampleStandardDeviation = EF.Functions.VarianceSample(g.Select(od => od.UnitPrice)),
                    PopulationStandardDeviation = EF.Functions.VariancePopulation(g.Select(od => od.UnitPrice))
                });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        var product9 = results.Single(r => r.ProductID == 9);
        Assert.Equal(75.2719999999972, product9.SampleStandardDeviation.Value, 5);
        Assert.Equal(60.217599999997766, product9.PopulationStandardDeviation.Value, 5);

        AssertSql(
            """
SELECT [o].[ProductID], VAR([o].[UnitPrice]) AS [SampleStandardDeviation], VARP([o].[UnitPrice]) AS [PopulationStandardDeviation]
FROM [Order Details] AS [o]
GROUP BY [o].[ProductID]
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    public class Fixture160 : NorthwindQuerySqlServerFixture<NoopModelCustomizer>
    {
        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).UseSqlServer(b => b.UseCompatibilityLevel(160));
    }
}
