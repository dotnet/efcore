// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query;

public class WindowFunctionSqlServerTest : WindowFunctionTestBase<WindowFunctionSqlServerTest.SqlServer>
{
    public WindowFunctionSqlServerTest(SqlServer fixture, ITestOutputHelper testOutputHelper) : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public class SqlServer : WindowFunctionFixture
    {
        protected override string StoreName => "WindowFunctionTests";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }

    #region Tests

    #region Window Functions Tests

    #region Max Tests

    public override void Max_Basic()
    {
        base.Max_Basic();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER () AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    public override void Max_Parition_Order_Rows()
    {
        base.Max_Parition_Order_Rows();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name] ROWS BETWEEN CURRENT ROW AND 5 FOLLOWING) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    public override void Max_Null()
    {
        base.Max_Null();

        AssertSql(
            """
SELECT [n].[Id], [n].[Name], MAX([n].[Salary]) OVER () AS [MaxSalary]
FROM [NullTestEmployees] AS [n]
""");
    }

    public override void Max_Filter()
    {
        base.Max_Filter();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], MAX(CASE
    WHEN [e].[Salary] > 100000.0 THEN [e].[Salary]
    ELSE NULL
END) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name]) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    #endregion

    #region Min Tests

    public override void Min_Basic()
    {
        base.Min_Basic();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], MIN([e].[Salary]) OVER () AS [MinSalary]
FROM [Employees] AS [e]
""");
    }

    public override void Min_Null()
    {
        base.Min_Null();

        AssertSql(
            """
SELECT [n].[Id], [n].[Name], MIN([n].[Salary]) OVER () AS [MinSalary]
FROM [NullTestEmployees] AS [n]
""");
    }

    public override void Min_Filter()
    {
        base.Min_Filter();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], MIN(CASE
    WHEN [e].[Salary] = 200000.0 THEN [e].[Salary]
    ELSE NULL
END) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name]) AS [MinSalary]
FROM [Employees] AS [e]
""");
    }

    #endregion

    #region Count Tests

    public override void Count_Star_Basic()
    {
        base.Count_Star_Basic();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], COUNT(*) OVER () AS [Count]
FROM [Employees] AS [e]
""");
    }

    public override void Count_Col_Basic()
    {
        base.Count_Col_Basic();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], COUNT([e].[Id]) OVER () AS [Count]
FROM [Employees] AS [e]
""");
    }

    public override void Count_Star_Filter()
    {
        base.Count_Star_Filter();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], COUNT(CASE
    WHEN [e].[Salary] <= 1200000.0 THEN N'1'
    ELSE NULL
END) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name]) AS [Count]
FROM [Employees] AS [e]
""");
    }

    public override void Count_Col_Filter()
    {
        base.Count_Col_Filter();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], COUNT(CASE
    WHEN [e].[Salary] <> 500000.0 THEN [e].[Salary]
    ELSE NULL
END) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name]) AS [Count]
FROM [Employees] AS [e]
""");
    }

    #endregion

    #region Average Tests

    public override void Avg_Decimal()
    {
        base.Avg_Decimal();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], AVG([e].[Salary]) OVER () AS [AverageSalary]
FROM [Employees] AS [e]
""");
    }

    public override void Avg_Int()
    {
        base.Avg_Int();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], AVG([e].[WorkExperience]) OVER () AS [AverageWork]
FROM [Employees] AS [e]
""");
    }

    public override void Avg_Decimal_Int_Cast_Decimal()
    {
        base.Avg_Decimal_Int_Cast_Decimal();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], AVG(CAST([e].[WorkExperience] AS decimal(18,2))) OVER () AS [AverageWork]
FROM [Employees] AS [e]
""");
    }

    public override void Avg_Null()
    {
        base.Avg_Null();

        AssertSql(
            """
SELECT [n].[Id], [n].[Name], AVG([n].[Salary]) OVER () AS [AverageSalary]
FROM [NullTestEmployees] AS [n]
""");
    }

    public override void Avg_Filter()
    {
        base.Avg_Filter();

        AssertSql(
            """
@__ids_1='[1,2,3]' (Size = 4000)

SELECT [e].[Id], [e].[Name], AVG(CASE
    WHEN [e].[EmployeeId] IN (
        SELECT [i].[value]
        FROM OPENJSON(@__ids_1) WITH ([value] int '$') AS [i]
    ) THEN [e].[Salary]
    ELSE NULL
END) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name]) AS [Avg]
FROM [Employees] AS [e]
""");
    }

    #endregion

    #region Sum Tests

    public override void Sum_Decimal()
    {
        base.Sum_Decimal();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], SUM([e].[Salary]) OVER () AS [SumSalary]
FROM [Employees] AS [e]
""");
    }

    public override void Sum_Int()
    {
        base.Sum_Int();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], SUM([e].[WorkExperience]) OVER () AS [SumWorkExperience]
FROM [Employees] AS [e]
""");
    }

    public override void Sum_Null()
    {
        base.Sum_Null();

        AssertSql(
            """
SELECT [n].[Id], [n].[Name], SUM([n].[Salary]) OVER () AS [Sum]
FROM [NullTestEmployees] AS [n]
""");
    }

    public override void Sum_Filter()
    {
        base.Sum_Filter();

        AssertSql(
            """
@__ids_1='[1,2,3]' (Size = 4000)

SELECT [e].[Id], [e].[Name], SUM(CASE
    WHEN [e].[EmployeeId] IN (
        SELECT [i].[value]
        FROM OPENJSON(@__ids_1) WITH ([value] int '$') AS [i]
    ) THEN [e].[Salary]
    ELSE NULL
END) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name]) AS [Sum]
FROM [Employees] AS [e]
""");
    }

    #endregion

    public override void RowNumber_Basic()
    {
        base.RowNumber_Basic();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], ROW_NUMBER() OVER ( ORDER BY [e].[Name]) AS [RowNumber]
FROM [Employees] AS [e]
""");
    }

    public override void First_Value_OderByEnd_Basic()
    {
        base.First_Value_OderByEnd_Basic();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], FIRST_VALUE([e].[Name]) OVER ( ORDER BY [e].[Salary]) AS [FirstValue]
FROM [Employees] AS [e]
""");
    }

    public override void First_Value_FrameEnd_Basic()
    {
        base.First_Value_FrameEnd_Basic();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], FIRST_VALUE([e].[Name]) OVER ( ORDER BY [e].[Salary] ROWS BETWEEN CURRENT ROW AND UNBOUNDED FOLLOWING) AS [FirstValue]
FROM [Employees] AS [e]
""");
    }

    public override void First_Value_Null()
    {
        base.First_Value_Null();

        AssertSql(
            """
SELECT [n].[Id], [n].[Name], FIRST_VALUE([n].[Salary]) OVER ( ORDER BY [n].[WorkExperience]) AS [FirstValue]
FROM [NullTestEmployees] AS [n]
""");
    }

    public override void Last_Value_OderByEnd_Basic()
    {
        base.Last_Value_OderByEnd_Basic();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], LAST_VALUE([e].[Name]) OVER ( ORDER BY [e].[Salary]) AS [LastValue]
FROM [Employees] AS [e]
""");
    }

    public override void Last_Value_FrameEnd_Basic()
    {
        base.Last_Value_FrameEnd_Basic();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], LAST_VALUE([e].[Name]) OVER ( ORDER BY [e].[Salary] ROWS BETWEEN CURRENT ROW AND UNBOUNDED FOLLOWING) AS [LastValue]
FROM [Employees] AS [e]
""");
    }

    public override void Last_Value_Null()
    {
        base.Last_Value_Null();

        AssertSql(
            """
SELECT [n].[Id], [n].[Name], LAST_VALUE([n].[Salary]) OVER ( ORDER BY [n].[WorkExperience]) AS [LastValue]
FROM [NullTestEmployees] AS [n]
""");
    }

    public override void Rank_Basic()
    {
        base.Rank_Basic();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], RANK() OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[WorkExperience]) AS [Rank]
FROM [Employees] AS [e]
""");
    }

    public override void Dense_Rank_Basic()
    {
        base.Dense_Rank_Basic();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], DENSE_RANK() OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[WorkExperience]) AS [Rank]
FROM [Employees] AS [e]
""");
    }

    public override void NTile_Basic()
    {
        base.NTile_Basic();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], NTILE(3) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[WorkExperience]) AS [Rank]
FROM [Employees] AS [e]
""");
    }

    public override void Percent_Rank_Basic()
    {
        base.Percent_Rank_Basic();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], PERCENT_RANK() OVER ( ORDER BY [e].[Salary]) AS [PercentRank]
FROM [Employees] AS [e]
""");
    }

    public override void Cume_Dist_Basic()
    {
        base.Cume_Dist_Basic();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], CUME_DIST() OVER ( ORDER BY [e].[Salary]) AS [CumeDist]
FROM [Employees] AS [e]
""");
    }

    public override void Lag_Decimal_Basic()
    {
        base.Lag_Decimal_Basic();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], LAG([e].[Salary], 1, 0.0) OVER ( ORDER BY [e].[Salary]) AS [PreviousSalary]
FROM [Employees] AS [e]
""");
    }

    public override void Lag_Int_Basic()
    {
        base.Lag_Int_Basic();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], LAG([e].[Id], 1, 0) OVER ( ORDER BY [e].[Id]) AS [PreviousId]
FROM [Employees] AS [e]
""");
    }

    public override void Lag_String_Basic()
    {
        base.Lag_String_Basic();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], LAG([e].[Name], 1, N'test') OVER ( ORDER BY [e].[Name]) AS [PreviousName]
FROM [Employees] AS [e]
""");
    }

    public override void Lead_Decimal_Basic()
    {
        base.Lead_Decimal_Basic();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], LEAD([e].[Salary], 1, 0.0) OVER ( ORDER BY [e].[Salary]) AS [NextSalary]
FROM [Employees] AS [e]
""");
    }

    public override void Lead_Int_Basic()
    {
        base.Lead_Int_Basic();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], LEAD([e].[Id], 1, 0) OVER ( ORDER BY [e].[Id]) AS [NextId]
FROM [Employees] AS [e]
""");
    }

    #endregion

    #region WindowOverExpression Equality tests

    public override void Multiple_Aggregates_Basic_NoDup_Query()
    {
        base.Multiple_Aggregates_Basic_NoDup_Query();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER () AS [MaxSalary], MIN([e].[Salary]) OVER () AS [MinSalary]
FROM [Employees] AS [e]
""");
    }

    public override void Multiple_Aggregates_Basic_Dup_Query()
    {
        base.Multiple_Aggregates_Basic_Dup_Query();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER () AS [MaxSalary1]
FROM [Employees] AS [e]
""");
    }

    #endregion

    #region SQL Server Specific

    [ConditionalFact]
    public void Count_Big_Star_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            Count = EF.Functions.Over().CountBig()
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(8, results[0].Count);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], COUNT_BIG(*) OVER () AS [Count]
FROM [Employees] AS [e]
""");
    }

    [ConditionalFact]
    public void Count_Big_Col_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            Count = EF.Functions.Over().CountBig(e.Id)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(8, results[0].Count);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], COUNT_BIG([e].[Id]) OVER () AS [Count]
FROM [Employees] AS [e]
""");
    }

    [ConditionalFact]
    public void Count_Big_Star_Filter()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            Count = EF.Functions.Over().CountBig(() => e.Salary > 10m)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(8, results[0].Count);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], COUNT_BIG(CASE
    WHEN [e].[Salary] > 10.0 THEN N'1'
    ELSE NULL
END) OVER () AS [Count]
FROM [Employees] AS [e]
""");
    }

    [ConditionalFact]
    public void Count_Big_Col_Filter()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            Count = EF.Functions.Over().CountBig(e.Id, () => e.Salary > 10m)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(8, results[0].Count);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], COUNT_BIG(CASE
    WHEN [e].[Salary] > 10.0 THEN [e].[Id]
    ELSE NULL
END) OVER () AS [Count]
FROM [Employees] AS [e]
""");
    }

    [ConditionalFact]
    public void Stdev_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            StdDev = EF.Functions.Over().OrderBy(e.WorkExperience).ThenBy(e.Name).Stdev(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);

        Assert.Null(results[0].StdDev);
        Assert.Equal(17677.58468d, Math.Round(results[1].StdDev!.Value, 5));
        Assert.Equal(180854.55298d, Math.Round(results[2].StdDev!.Value, 5));
        Assert.Equal(149129.50692d, Math.Round(results[3].StdDev!.Value, 5));
        Assert.Equal(132759.24601d, Math.Round(results[4].StdDev!.Value, 5));
        Assert.Equal(187361.07404d, Math.Round(results[5].StdDev!.Value, 5));
        Assert.Equal(608789.76503d, Math.Round(results[6].StdDev!.Value, 5));
        Assert.Equal(599171.71693d, Math.Round(results[7].StdDev!.Value, 5));

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], STDEV([e].[Salary]) OVER ( ORDER BY [e].[WorkExperience], [e].[Name]) AS [StdDev]
FROM [Employees] AS [e]
""");
    }

    [ConditionalFact]
    public void Stdev_Filter()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            StdDev = EF.Functions.Over().OrderBy(e.WorkExperience).ThenBy(e.Name).Stdev(e.Salary, () => e.Salary > 100000m)
        }).ToList();

        Assert.Equal(8, results.Count);

        Assert.Null(results[0].StdDev);
        Assert.Null(results[1].StdDev);
        Assert.Null(results[2].StdDev);
        Assert.Null(results[3].StdDev);
        Assert.Equal(106066.18688d, Math.Round(results[4].StdDev!.Value, 5));
        Assert.Equal(150000.00000d, Math.Round(results[5].StdDev!.Value, 5));
        Assert.Equal(710633.48078d, Math.Round(results[6].StdDev!.Value, 5));
        Assert.Equal(629880.95256d, Math.Round(results[7].StdDev!.Value, 5));

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], STDEV(CASE
    WHEN [e].[Salary] > 100000.0 THEN [e].[Salary]
    ELSE NULL
END) OVER ( ORDER BY [e].[WorkExperience], [e].[Name]) AS [StdDev]
FROM [Employees] AS [e]
""");
    }

    [ConditionalFact]
    public void StdevP_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            StdDevP = EF.Functions.Over().OrderBy(e.WorkExperience).ThenBy(e.Name).StdevP(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);

        Assert.Equal(0f, Math.Round(results[0].StdDevP ?? 0, 5));
        Assert.Equal(12499.94d, Math.Round(results[1].StdDevP ?? 0, 5));
        Assert.Equal(147667.12415d, Math.Round(results[2].StdDevP ?? 0, 5));
        Assert.Equal(129149.94144d, Math.Round(results[3].StdDevP ?? 0, 5));
        Assert.Equal(118743.47948d, Math.Round(results[4].StdDevP ?? 0, 5));
        Assert.Equal(171036.47775d, Math.Round(results[5].StdDevP ?? 0, 5));
        Assert.Equal(563629.80100d, Math.Round(results[6].StdDevP ?? 0, 5));
        Assert.Equal(560473.82015d, Math.Round(results[7].StdDevP ?? 0, 5));

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], STDEVP([e].[Salary]) OVER ( ORDER BY [e].[WorkExperience], [e].[Name]) AS [StdDevP]
FROM [Employees] AS [e]
""");
    }


    [ConditionalFact]
    public void StdevP_Filter()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            StdDev = EF.Functions.Over().OrderBy(e.WorkExperience).ThenBy(e.Name).StdevP(e.Salary, () => e.Salary > 100000m)
        }).ToList();

        Assert.Equal(8, results.Count);

        Assert.Null(results[0].StdDev);
        Assert.Null(results[1].StdDev);
        Assert.Equal(0, results[2].StdDev);
        Assert.Equal(0, results[3].StdDev);
        Assert.Equal(75000.12d, Math.Round(results[4].StdDev!.Value, 5));
        Assert.Equal(122474.48714d, Math.Round(results[5].StdDev!.Value, 5));
        Assert.Equal(615426.64713d, Math.Round(results[6].StdDev!.Value, 5));
        Assert.Equal(563382.65106d, Math.Round(results[7].StdDev!.Value, 5));

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], STDEVP(CASE
    WHEN [e].[Salary] > 100000.0 THEN [e].[Salary]
    ELSE NULL
END) OVER ( ORDER BY [e].[WorkExperience], [e].[Name]) AS [StdDev]
FROM [Employees] AS [e]
""");
    }

    [ConditionalFact]
    public void Var_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            Var = EF.Functions.Over().OrderBy(e.WorkExperience).ThenBy(e.Name).Var(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);

        Assert.Null(results[0].Var);
        Assert.Equal(312497000.007d, Math.Round(results[1].Var!.Value, 3));
        Assert.Equal(32708369333.348d, Math.Round(results[2].Var!.Value, 3));
        Assert.Equal(22239609833.347d, Math.Round(results[3].Var!.Value, 3));
        Assert.Equal(17625017400.012d, Math.Round(results[4].Var!.Value, 3));
        Assert.Equal(35104172066.677d, Math.Round(results[5].Var!.Value, 3));
        Assert.Equal(370624978000.009d, Math.Round(results[6].Var!.Value, 3));
        Assert.Equal(359006746366.108d, Math.Round(results[7].Var!.Value, 3));

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], VAR([e].[Salary]) OVER ( ORDER BY [e].[WorkExperience], [e].[Name]) AS [Var]
FROM [Employees] AS [e]
""");
    }

    [ConditionalFact]
    public void Var_Filter()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            StdDev = EF.Functions.Over().OrderBy(e.WorkExperience).ThenBy(e.Name).Var(e.Salary, () => e.Salary > 100000m)
        }).ToList();

        Assert.Equal(8, results.Count);

        Assert.Null(results[0].StdDev);
        Assert.Null(results[1].StdDev);
        Assert.Null(results[2].StdDev);
        Assert.Null(results[3].StdDev);
        Assert.Equal(11250036000.02878d, Math.Round(results[4].StdDev!.Value, 5));
        Assert.Equal(22500000000.0192d, Math.Round(results[5].StdDev!.Value, 5));
        Assert.Equal(504999944000.01434d, Math.Round(results[6].StdDev!.Value, 5));
        Assert.Equal(396750014400.05493d, Math.Round(results[7].StdDev!.Value, 5));

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], VAR(CASE
    WHEN [e].[Salary] > 100000.0 THEN [e].[Salary]
    ELSE NULL
END) OVER ( ORDER BY [e].[WorkExperience], [e].[Name]) AS [StdDev]
FROM [Employees] AS [e]
""");
    }

    [ConditionalFact]
    public void VarP_Basic()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            VarP = EF.Functions.Over().OrderBy(e.WorkExperience).ThenBy(e.Name).VarP(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);

        Assert.Equal(0d, Math.Round(results[0].VarP ?? 0, 3));
        Assert.Equal(156248500.004d, Math.Round(results[1].VarP ?? 0, 3));
        Assert.Equal(21805579555.565d, Math.Round(results[2].VarP ?? 0, 3));
        Assert.Equal(16679707375.01d, Math.Round(results[3].VarP ?? 0, 3));
        Assert.Equal(14100013920.009d, Math.Round(results[4].VarP ?? 0, 3));
        Assert.Equal(29253476722.231d, Math.Round(results[5].VarP ?? 0, 3));
        Assert.Equal(317678552571.436d, Math.Round(results[6].VarP ?? 0, 3));
        Assert.Equal(314130903070.344d, Math.Round(results[7].VarP ?? 0, 3));

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], VARP([e].[Salary]) OVER ( ORDER BY [e].[WorkExperience], [e].[Name]) AS [VarP]
FROM [Employees] AS [e]
""");
    }

    [ConditionalFact]
    public void VarP_Filter()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            StdDev = EF.Functions.Over().OrderBy(e.WorkExperience).ThenBy(e.Name).VarP(e.Salary, () => e.Salary > 100000m)
        }).ToList();

        Assert.Equal(8, results.Count);

        Assert.Null(results[0].StdDev);
        Assert.Null(results[1].StdDev);
        Assert.Equal(0, results[2].StdDev);
        Assert.Equal(0, results[3].StdDev);
        Assert.Equal(5625018000.01439d, Math.Round(results[4].StdDev!.Value, 5));
        Assert.Equal(15000000000.0128d, Math.Round(results[5].StdDev!.Value, 5));
        Assert.Equal(378749958000.01074d, Math.Round(results[6].StdDev!.Value, 5));
        Assert.Equal(317400011520.04395d, Math.Round(results[7].StdDev!.Value, 5));

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], VARP(CASE
    WHEN [e].[Salary] > 100000.0 THEN [e].[Salary]
    ELSE NULL
END) OVER ( ORDER BY [e].[WorkExperience], [e].[Name]) AS [StdDev]
FROM [Employees] AS [e]
""");
    }

    #endregion

    #region Rows / Range Tests

    #region Rows(int preceding)

    public override void Rows_Preceding_X()
    {
        base.Rows_Preceding_X();

        AssertSql(
         """
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name] ROWS 2 PRECEDING) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    #endregion

    #region Rows(RowsPreceding preceding)

    [ConditionalFact]
    public override void Rows_Preceding_CurrentRow()
    {
        base.Rows_Preceding_CurrentRow();

        AssertSql(
 """
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name] ROWS CURRENT ROW) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    [ConditionalFact]
    public override void Rows_Preceding_UnboundedPreceding()
    {
        base.Rows_Preceding_UnboundedPreceding();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name] ROWS UNBOUNDED PRECEDING) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    #endregion

    #region Rows(int preceding, int following)

    [ConditionalFact]
    public override void Rows_Preceding_X_Following_X()
    {
        base.Rows_Preceding_X_Following_X();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name] ROWS BETWEEN 1 PRECEDING AND 2 FOLLOWING) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    #endregion

    #region Rows(RowsPreceding preceding, int following)

    [ConditionalFact]
    public override void Rows_Preceding_CurrentRow_Following_X()
    {
        base.Rows_Preceding_CurrentRow_Following_X();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name] ROWS BETWEEN CURRENT ROW AND 2 FOLLOWING) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    [ConditionalFact]
    public override void Rows_Preceding_UnboundedPreceding_Following_X()
    {
        base.Rows_Preceding_UnboundedPreceding_Following_X();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name] ROWS BETWEEN UNBOUNDED PRECEDING AND 2 FOLLOWING) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    #endregion

    #region Rows(int preceding, RowsFollowing following)

    [ConditionalFact]
    public override void Rows_Preceding_X_Following_CurrentRow()
    {
        base.Rows_Preceding_X_Following_CurrentRow();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name] ROWS BETWEEN 2 PRECEDING AND CURRENT ROW) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    [ConditionalFact]
    public override void Rows_Preceding_X_Following_UnboundedFollowing()
    {
        base.Rows_Preceding_X_Following_UnboundedFollowing();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name] ROWS BETWEEN 2 PRECEDING AND UNBOUNDED FOLLOWING) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    #endregion

    #region Rows(RowsPreceding preceding, RowsFollowing following)

    [ConditionalFact]
    public override void Rows_Preceding_CurrentRow_Following_CurrentRow()
    {
        base.Rows_Preceding_CurrentRow_Following_CurrentRow();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name] ROWS BETWEEN CURRENT ROW AND CURRENT ROW) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    [ConditionalFact]
    public override void Rows_Preceding_CurrentRow_Following_UnboundedFollowing()
    {
        base.Rows_Preceding_CurrentRow_Following_UnboundedFollowing();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name] ROWS BETWEEN CURRENT ROW AND UNBOUNDED FOLLOWING) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    [ConditionalFact]
    public override void Rows_Preceding_UnboundedPreceding_Following_CurrentRow()
    {
        base.Rows_Preceding_UnboundedPreceding_Following_CurrentRow();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name] ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    [ConditionalFact]
    public override void Rows_Preceding_UnboundedPreceding_Following_UnboundedFollowing()
    {
        base.Rows_Preceding_UnboundedPreceding_Following_UnboundedFollowing();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name] ROWS BETWEEN UNBOUNDED PRECEDING AND UNBOUNDED FOLLOWING) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    #endregion Range(RowsPreceding preceding)

    #region Range(RowsPreceding preceding)

    [ConditionalFact]
    public override void Range_CurrentRow()
    {
        base.Range_CurrentRow();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name] RANGE CURRENT ROW) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    [ConditionalFact]
    public override void Range_UnboundedPreceding()
    {
        base.Range_UnboundedPreceding();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name] RANGE UNBOUNDED PRECEDING) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    #endregion

    #region Range(RowsPreceding preceding, RowsFollowing following)

    #endregion

    [ConditionalFact]
    public override void Range_Preceding_CurrentRow_Following_CurrentRow()
    {
        base.Range_Preceding_CurrentRow_Following_CurrentRow();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name] RANGE BETWEEN CURRENT ROW AND CURRENT ROW) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    [ConditionalFact]
    public override void Range_Preceding_CurrentRow_Following_UnboundedFollowing()
    {
        base.Range_Preceding_CurrentRow_Following_UnboundedFollowing();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name] RANGE BETWEEN CURRENT ROW AND UNBOUNDED FOLLOWING) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    [ConditionalFact]
    public override void Range_Preceding_UnboundedPreceding_Following_CurrentRow()
    {
        base.Range_Preceding_UnboundedPreceding_Following_CurrentRow();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name] RANGE BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    [ConditionalFact]
    public override void Range_Preceding_UnboundedPreceding_Following_UnboundedFollowing()
    {
        base.Range_Preceding_UnboundedPreceding_Following_UnboundedFollowing();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name] RANGE BETWEEN UNBOUNDED PRECEDING AND UNBOUNDED FOLLOWING) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    #endregion

    #region Partition / Order By

    public override void Rows_No_Parition()
    {
        base.Rows_No_Parition();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER ( ORDER BY [e].[Name] ROWS BETWEEN 1 PRECEDING AND 2 FOLLOWING) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    public override void Range_No_Parition()
    {
        base.Range_No_Parition();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER ( ORDER BY [e].[Name] RANGE BETWEEN CURRENT ROW AND UNBOUNDED FOLLOWING) AS [MaxSalary]
FROM [Employees] AS [e]
ORDER BY [e].[Name]
""");
    }

    public override void OrderBy_No_Parition()
    {
        base.OrderBy_No_Parition();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER ( ORDER BY [e].[Name]) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    public override void OrderBy_Desc_No_Parition()
    {
        base.OrderBy_Desc_No_Parition();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER ( ORDER BY [e].[Name] DESC) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    public override void Partition_No_OrderBy_No_Frame()
    {
        base.Partition_No_OrderBy_No_Frame();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER (PARTITION BY [e].[DepartmentName]) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    public override void Partition_MultipleColumns()
    {
        base.Partition_MultipleColumns();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER (PARTITION BY [e].[DepartmentName], [e].[WorkExperience]) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    public override void Partition_ColumnModified()
    {
        base.Partition_ColumnModified();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER (PARTITION BY [e].[WorkExperience] / 10) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    public override void OrderBy_Desc_Rows()
    {
        base.OrderBy_Desc_Rows();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], MAX([e].[Salary]) OVER ( ORDER BY [e].[Name] DESC ROWS 1 PRECEDING) AS [MaxSalary]
FROM [Employees] AS [e]
""");
    }

    public override void Outer_Order_By_Sql()
    {
        base.Outer_Order_By_Sql();

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], RANK() OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[WorkExperience], [e].[Name]) AS [Rank]
FROM [Employees] AS [e]
ORDER BY [e].[Name]
""");
    }

    #endregion

    #endregion

    #region Where

    #endregion

    #region Order By

    public override void WindowFunctionInOrderBy()
    {
        base.WindowFunctionInOrderBy();

        AssertSql(
            """
SELECT [e].[Id], [e].[Name]
FROM [Employees] AS [e]
ORDER BY ROW_NUMBER() OVER (PARTITION BY [e].[DepartmentName] ORDER BY [e].[Name])
""");
    }

    #endregion

    public void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
