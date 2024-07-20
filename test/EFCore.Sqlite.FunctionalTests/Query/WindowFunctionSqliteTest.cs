// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Sqlite.Query;

namespace Microsoft.EntityFrameworkCore.Query;

public class WindowFunctionSqliteTest : WindowFunctionTestBase<WindowFunctionSqliteTest.Sqlite>
{
    public WindowFunctionSqliteTest(Sqlite fixture, ITestOutputHelper testOutputHelper) : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public class Sqlite : WindowFunctionFixture
    {
        protected override string StoreName => "WindowFunctionTests";

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }

    #region Tests

    #region Base Window Functions Tests

    #region Max Tests

    public override void Max_Basic()
    {
        base.Max_Basic();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER () AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    public override void Max_Parition_Order_Rows()
    {
        base.Max_Parition_Order_Rows();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" ROWS BETWEEN CURRENT ROW AND 5 FOLLOWING) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    public override void Max_Null()
    {
        base.Max_Null();

        AssertSql(
            """
SELECT "n"."Id", "n"."Name", MAX("n"."Salary") OVER () AS "MaxSalary"
FROM "NullTestEmployees" AS "n"
""");
    }

    public override void Max_Filter()
    {
        base.Max_Filter();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", MAX(CASE
    WHEN ef_compare("e"."Salary", '100000.0') > 0 THEN "e"."Salary"
    ELSE NULL
END) OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name") AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    #endregion

    #region Min Tests

    public override void Min_Basic()
    {
        base.Min_Basic();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", MIN("e"."Salary") OVER () AS "MinSalary"
FROM "Employees" AS "e"
""");
    }

    public override void Min_Null()
    {
        base.Min_Null();

        AssertSql(
            """
SELECT "n"."Id", "n"."Name", MIN("n"."Salary") OVER () AS "MinSalary"
FROM "NullTestEmployees" AS "n"
""");
    }

    public override void Min_Filter()
    {
        base.Min_Filter();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", MIN(CASE
    WHEN "e"."Salary" = '200000.0' THEN "e"."Salary"
    ELSE NULL
END) OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name") AS "MinSalary"
FROM "Employees" AS "e"
""");
    }

    #endregion

    #region Count Tests

    public override void Count_Star_Basic()
    {
        base.Count_Star_Basic();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", COUNT(*) OVER () AS "Count"
FROM "Employees" AS "e"
""");
    }

    public override void Count_Col_Basic()
    {
        base.Count_Col_Basic();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", COUNT("e"."Id") OVER () AS "Count"
FROM "Employees" AS "e"
""");
    }

    public override void Count_Star_Filter()
    {
        base.Count_Star_Filter();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", COUNT(CASE
    WHEN ef_compare("e"."Salary", '1200000.0') <= 0 THEN '1'
    ELSE NULL
END) OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name") AS "Count"
FROM "Employees" AS "e"
""");
    }

    public override void Count_Col_Filter()
    {
        base.Count_Col_Filter();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", COUNT(CASE
    WHEN "e"."Salary" <> '500000.0' THEN "e"."Salary"
    ELSE NULL
END) OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name") AS "Count"
FROM "Employees" AS "e"
""");
    }

    #endregion

    #region Average Tests

    public override void Avg_Decimal()
    {
        base.Avg_Decimal();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", AVG("e"."Salary") OVER () AS "AverageSalary"
FROM "Employees" AS "e"
""");
    }

    public override void Avg_Int()
    {
        base.Avg_Int();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", AVG("e"."WorkExperience") OVER () AS "AverageWork"
FROM "Employees" AS "e"
""");
    }

    public override void Avg_Decimal_Int_Cast_Decimal()
    {
        base.Avg_Decimal_Int_Cast_Decimal();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", AVG(CAST("e"."WorkExperience" AS TEXT)) OVER () AS "AverageWork"
FROM "Employees" AS "e"
""");
    }

    public override void Avg_Null()
    {
        base.Avg_Null();

        AssertSql(
            """
SELECT "n"."Id", "n"."Name", AVG("n"."Salary") OVER () AS "AverageSalary"
FROM "NullTestEmployees" AS "n"
""");
    }

    public override void Avg_Filter()
    {
        base.Avg_Filter();

        AssertSql(
            """
@__ids_1='[1,2,3]' (Size = 7)

SELECT "e"."Id", "e"."Name", AVG(CASE
    WHEN "e"."EmployeeId" IN (
        SELECT "i"."value"
        FROM json_each(@__ids_1) AS "i"
    ) THEN "e"."Salary"
    ELSE NULL
END) OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name") AS "Avg"
FROM "Employees" AS "e"
""");
    }

    #endregion

    #region Sum Tests

    public override void Sum_Decimal()
    {
        base.Sum_Decimal();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", SUM("e"."Salary") OVER () AS "SumSalary"
FROM "Employees" AS "e"
""");
    }

    public override void Sum_Int()
    {
        base.Sum_Int();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", SUM("e"."WorkExperience") OVER () AS "SumWorkExperience"
FROM "Employees" AS "e"
""");
    }

    public override void Sum_Null()
    {
        base.Sum_Null();

        AssertSql(
            """
SELECT "n"."Id", "n"."Name", SUM("n"."Salary") OVER () AS "Sum"
FROM "NullTestEmployees" AS "n"
""");
    }

    public override void Sum_Filter()
    {
        base.Sum_Filter();

        AssertSql(
            """
@__ids_1='[1,2,3]' (Size = 7)

SELECT "e"."Id", "e"."Name", SUM(CASE
    WHEN "e"."EmployeeId" IN (
        SELECT "i"."value"
        FROM json_each(@__ids_1) AS "i"
    ) THEN "e"."Salary"
    ELSE NULL
END) OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name") AS "Sum"
FROM "Employees" AS "e"
""");
    }

    #endregion

    public override void RowNumber_Basic()
    {
        base.RowNumber_Basic();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", ROW_NUMBER() OVER ( ORDER BY "e"."Name") AS "RowNumber"
FROM "Employees" AS "e"
""");
    }

    public override void First_Value_OderByEnd_Basic()
    {
        base.First_Value_OderByEnd_Basic();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", FIRST_VALUE("e"."Name") OVER ( ORDER BY "e"."Salary") AS "FirstValue"
FROM "Employees" AS "e"
""");
    }

    public override void First_Value_FrameEnd_Basic()
    {
        base.First_Value_FrameEnd_Basic();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", FIRST_VALUE("e"."Name") OVER ( ORDER BY "e"."Salary" ROWS BETWEEN CURRENT ROW AND UNBOUNDED FOLLOWING) AS "FirstValue"
FROM "Employees" AS "e"
""");
    }

    public override void First_Value_Null()
    {
        base.First_Value_Null();

        AssertSql(
            """
SELECT "n"."Id", "n"."Name", FIRST_VALUE("n"."Salary") OVER ( ORDER BY "n"."WorkExperience") AS "FirstValue"
FROM "NullTestEmployees" AS "n"
""");
    }

    public override void Last_Value_OderByEnd_Basic()
    {
        base.Last_Value_OderByEnd_Basic();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", LAST_VALUE("e"."Name") OVER ( ORDER BY "e"."Salary") AS "LastValue"
FROM "Employees" AS "e"
""");
    }

    public override void Last_Value_FrameEnd_Basic()
    {
        base.Last_Value_FrameEnd_Basic();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", LAST_VALUE("e"."Name") OVER ( ORDER BY "e"."Salary" ROWS BETWEEN CURRENT ROW AND UNBOUNDED FOLLOWING) AS "LastValue"
FROM "Employees" AS "e"
""");
    }

    public override void Last_Value_Null()
    {
        base.Last_Value_Null();

        AssertSql(
            """
SELECT "n"."Id", "n"."Name", LAST_VALUE("n"."Salary") OVER ( ORDER BY "n"."WorkExperience") AS "LastValue"
FROM "NullTestEmployees" AS "n"
""");
    }

    public override void Rank_Basic()
    {
        base.Rank_Basic();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", RANK() OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."WorkExperience") AS "Rank"
FROM "Employees" AS "e"
""");
    }

    public override void Dense_Rank_Basic()
    {
        base.Dense_Rank_Basic();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", DENSE_RANK() OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."WorkExperience") AS "Rank"
FROM "Employees" AS "e"
""");
    }

    public override void NTile_Basic()
    {
        base.NTile_Basic();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", NTILE(3) OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."WorkExperience") AS "Rank"
FROM "Employees" AS "e"
""");
    }

    public override void Percent_Rank_Basic()
    {
        base.Percent_Rank_Basic();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", PERCENT_RANK() OVER ( ORDER BY "e"."Salary") AS "PercentRank"
FROM "Employees" AS "e"
""");
    }

    public override void Cume_Dist_Basic()
    {
        base.Cume_Dist_Basic();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", CUME_DIST() OVER ( ORDER BY "e"."Salary") AS "CumeDist"
FROM "Employees" AS "e"
""");
    }

    public override void Lag_Decimal_Basic()
    {
        base.Lag_Decimal_Basic();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", LAG("e"."Salary", 1, '0.0') OVER ( ORDER BY "e"."Salary") AS "PreviousSalary"
FROM "Employees" AS "e"
""");
    }

    public override void Lag_Int_Basic()
    {
        base.Lag_Int_Basic();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", LAG("e"."Id", 1, 0) OVER ( ORDER BY "e"."Id") AS "PreviousId"
FROM "Employees" AS "e"
""");
    }

    public override void Lead_Decimal_Basic()
    {
        base.Lead_Decimal_Basic();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", LEAD("e"."Salary", 1, '0.0') OVER ( ORDER BY "e"."Salary") AS "NextSalary"
FROM "Employees" AS "e"
""");
    }

    public override void Lead_Int_Basic()
    {
        base.Lead_Int_Basic();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", LEAD("e"."Id", 1, 0) OVER ( ORDER BY "e"."Id") AS "NextId"
FROM "Employees" AS "e"
""");
    }

    public override void Lag_String_Basic()
    {
        base.Lag_String_Basic();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", LAG("e"."Name", 1, 'test') OVER ( ORDER BY "e"."Name") AS "PreviousName"
FROM "Employees" AS "e"
""");
    }

    #endregion

    #region WindowOverExpression Equality tests

    public override void Multiple_Aggregates_Basic_NoDup_Query()
    {
        base.Multiple_Aggregates_Basic_NoDup_Query();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER () AS "MaxSalary", MIN("e"."Salary") OVER () AS "MinSalary"
FROM "Employees" AS "e"
""");
    }

    public override void Multiple_Aggregates_Basic_Dup_Query()
    {
        base.Multiple_Aggregates_Basic_Dup_Query();

        AssertSql(
            """
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER () AS "MaxSalary1"
FROM "Employees" AS "e"
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
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" ROWS 2 PRECEDING) AS "MaxSalary"
FROM "Employees" AS "e"
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
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" ROWS CURRENT ROW) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    [ConditionalFact]
    public override void Rows_Preceding_UnboundedPreceding()
    {
        base.Rows_Preceding_UnboundedPreceding();

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" ROWS UNBOUNDED PRECEDING) AS "MaxSalary"
FROM "Employees" AS "e"
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
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" ROWS BETWEEN 1 PRECEDING AND 2 FOLLOWING) AS "MaxSalary"
FROM "Employees" AS "e"
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
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" ROWS BETWEEN CURRENT ROW AND 2 FOLLOWING) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    [ConditionalFact]
    public override void Rows_Preceding_UnboundedPreceding_Following_X()
    {
        base.Rows_Preceding_UnboundedPreceding_Following_X();

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" ROWS BETWEEN UNBOUNDED PRECEDING AND 2 FOLLOWING) AS "MaxSalary"
FROM "Employees" AS "e"
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
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" ROWS BETWEEN 2 PRECEDING AND CURRENT ROW) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    [ConditionalFact]
    public override void Rows_Preceding_X_Following_UnboundedFollowing()
    {
        base.Rows_Preceding_X_Following_UnboundedFollowing();

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" ROWS BETWEEN 2 PRECEDING AND UNBOUNDED FOLLOWING) AS "MaxSalary"
FROM "Employees" AS "e"
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
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" ROWS BETWEEN CURRENT ROW AND CURRENT ROW) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    [ConditionalFact]
    public override void Rows_Preceding_CurrentRow_Following_UnboundedFollowing()
    {
        base.Rows_Preceding_CurrentRow_Following_UnboundedFollowing();

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" ROWS BETWEEN CURRENT ROW AND UNBOUNDED FOLLOWING) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    [ConditionalFact]
    public override void Rows_Preceding_UnboundedPreceding_Following_CurrentRow()
    {
        base.Rows_Preceding_UnboundedPreceding_Following_CurrentRow();

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    [ConditionalFact]
    public override void Rows_Preceding_UnboundedPreceding_Following_UnboundedFollowing()
    {
        base.Rows_Preceding_UnboundedPreceding_Following_UnboundedFollowing();

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" ROWS BETWEEN UNBOUNDED PRECEDING AND UNBOUNDED FOLLOWING) AS "MaxSalary"
FROM "Employees" AS "e"
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
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" RANGE CURRENT ROW) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    [ConditionalFact]
    public override void Range_UnboundedPreceding()
    {
        base.Range_UnboundedPreceding();

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" RANGE UNBOUNDED PRECEDING) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    #endregion

    #region Range(RowsPreceding preceding, RowsFollowing following)

    [ConditionalFact]
    public override void Range_Preceding_CurrentRow_Following_CurrentRow()
    {
        base.Range_Preceding_CurrentRow_Following_CurrentRow();

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" RANGE BETWEEN CURRENT ROW AND CURRENT ROW) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    [ConditionalFact]
    public override void Range_Preceding_CurrentRow_Following_UnboundedFollowing()
    {
        base.Range_Preceding_CurrentRow_Following_UnboundedFollowing();

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" RANGE BETWEEN CURRENT ROW AND UNBOUNDED FOLLOWING) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    [ConditionalFact]
    public override void Range_Preceding_UnboundedPreceding_Following_CurrentRow()
    {
        base.Range_Preceding_UnboundedPreceding_Following_CurrentRow();

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" RANGE BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    [ConditionalFact]
    public override void Range_Preceding_UnboundedPreceding_Following_UnboundedFollowing()
    {
        base.Range_Preceding_UnboundedPreceding_Following_UnboundedFollowing();

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" RANGE BETWEEN UNBOUNDED PRECEDING AND UNBOUNDED FOLLOWING) AS "MaxSalary"
FROM "Employees" AS "e"
""");

    #endregion

    }

    #endregion

    #region Partition / Order By

    public override void Rows_No_Parition()
    {
        base.Rows_No_Parition();

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER ( ORDER BY "e"."Name" ROWS BETWEEN 1 PRECEDING AND 2 FOLLOWING) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    public override void Range_No_Parition()
    {
        base.Range_No_Parition();

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER ( ORDER BY "e"."Name" RANGE BETWEEN CURRENT ROW AND UNBOUNDED FOLLOWING) AS "MaxSalary"
FROM "Employees" AS "e"
ORDER BY "e"."Name"
""");
    }

    public override void OrderBy_No_Parition()
    {
        base.OrderBy_No_Parition();

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER ( ORDER BY "e"."Name") AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    public override void OrderBy_Desc_No_Parition()
    {
        base.OrderBy_Desc_No_Parition();

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER ( ORDER BY "e"."Name" DESC) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    public override void OrderBy_Desc_Rows()
    {
        base.OrderBy_Desc_Rows();

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER ( ORDER BY "e"."Name" DESC ROWS 1 PRECEDING) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    public override void Outer_Order_By_Sql()
    {
        base.Outer_Order_By_Sql();

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", RANK() OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."WorkExperience", "e"."Name") AS "Rank"
FROM "Employees" AS "e"
ORDER BY "e"."Name"
""");
    }

    public override void Partition_No_OrderBy_No_Frame()
    {
        base.Partition_No_OrderBy_No_Frame();

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName") AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    public override void Partition_MultipleColumns()
    {
        base.Partition_MultipleColumns();

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName", "e"."WorkExperience") AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    public override void Partition_ColumnModified()
    {
        base.Partition_ColumnModified();

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."WorkExperience" / 10) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    #endregion

    #region SQLLite Specific

    #region Range(int preceding, int following)

    [ConditionalFact]
    public virtual void Range_Preceding_X_Following_X()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Range(1, 2).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);

        AssertSql(
           """
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" RANGE BETWEEN 1 PRECEDING AND 2 FOLLOWING) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    #endregion

    #region Range(RowsPreceding preceding, int following)

    [ConditionalFact]
    public virtual void Range_Preceding_CurrentRow_Following_X()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Range(RowsPreceding.CurrentRow, 2).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);

        AssertSql(
           """
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" RANGE BETWEEN CURRENT ROW AND 2 FOLLOWING) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    [ConditionalFact]
    public virtual void Range_Preceding_UnboundedPreceding_Following_X()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Range(RowsPreceding.UnboundedPreceding, 2).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);

        AssertSql(
   """
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" RANGE BETWEEN UNBOUNDED PRECEDING AND 2 FOLLOWING) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    #endregion

    #region Range(int preceding, RowsFollowing following)

    [ConditionalFact]
    public virtual void Range_Preceding_X_Following_CurrentRow()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Range(2, RowsFollowing.CurrentRow).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" RANGE BETWEEN 2 PRECEDING AND CURRENT ROW) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    [ConditionalFact]
    public virtual void Range_Preceding_X_Following_UnboundedFollowing()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Range(2, RowsFollowing.UnboundedFollowing).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" RANGE BETWEEN 2 PRECEDING AND UNBOUNDED FOLLOWING) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    #endregion

    #region Groups

    #region Groups(int preceding)

    [ConditionalFact]
    public virtual void Groups_Preceding_X()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Groups(2).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);

        AssertSql(
         """
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" GROUPS 2 PRECEDING) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    #endregion

    #region Groups(GroupsPreceding preceding)

    [ConditionalFact]
    public virtual void Groups_Preceding_CurrentRow()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Groups(RowsPreceding.CurrentRow).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);

        AssertSql(
 """
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" GROUPS CURRENT ROW) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    [ConditionalFact]
    public virtual void Groups_Preceding_UnboundedPreceding()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Groups(RowsPreceding.UnboundedPreceding).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" GROUPS UNBOUNDED PRECEDING) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    #endregion

    #region Groups(int preceding, int following)

    [ConditionalFact]
    public virtual void Groups_Preceding_X_Following_X()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Groups(1, 2).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" GROUPS BETWEEN 1 PRECEDING AND 2 FOLLOWING) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    #endregion

    #region Groups(GroupsPreceding preceding, int following)

    [ConditionalFact]
    public virtual void Groups_Preceding_CurrentRow_Following_X()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Groups(RowsPreceding.CurrentRow, 2).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" GROUPS BETWEEN CURRENT ROW AND 2 FOLLOWING) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    [ConditionalFact]
    public virtual void Groups_Preceding_UnboundedPreceding_Following_X()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Groups(RowsPreceding.UnboundedPreceding, 2).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" GROUPS BETWEEN UNBOUNDED PRECEDING AND 2 FOLLOWING) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    #endregion

    #region Groups(int preceding, GroupsFollowing following)

    [ConditionalFact]
    public virtual void Groups_Preceding_X_Following_CurrentRow()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Rows(2, RowsFollowing.CurrentRow).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" ROWS BETWEEN 2 PRECEDING AND CURRENT ROW) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    [ConditionalFact]
    public virtual void Groups_Preceding_X_Following_UnboundedFollowing()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Groups(2, RowsFollowing.UnboundedFollowing).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" GROUPS BETWEEN 2 PRECEDING AND UNBOUNDED FOLLOWING) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    #endregion

    #region Groups(GroupsPreceding preceding, GroupsFollowing following)

    [ConditionalFact]
    public virtual void Groups_Preceding_CurrentRow_Following_CurrentRow()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Groups(RowsPreceding.CurrentRow, RowsFollowing.CurrentRow).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" GROUPS BETWEEN CURRENT ROW AND CURRENT ROW) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    [ConditionalFact]
    public virtual void Groups_Preceding_CurrentRow_Following_UnboundedFollowing()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Groups(RowsPreceding.CurrentRow, RowsFollowing.UnboundedFollowing).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" GROUPS BETWEEN CURRENT ROW AND UNBOUNDED FOLLOWING) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    [ConditionalFact]
    public virtual void Groups_Preceding_UnboundedPreceding_Following_CurrentRow()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Groups(RowsPreceding.UnboundedPreceding, RowsFollowing.CurrentRow).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" GROUPS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    [ConditionalFact]
    public virtual void Groups_Preceding_UnboundedPreceding_Following_UnboundedFollowing()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Groups(RowsPreceding.UnboundedPreceding, RowsFollowing.UnboundedFollowing).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);

        AssertSql(
"""
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" GROUPS BETWEEN UNBOUNDED PRECEDING AND UNBOUNDED FOLLOWING) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    #endregion

    #endregion

    #region Exclude

    [ConditionalFact]
    public virtual void Exclude_NoOthers()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Rows(2).Exclude(FrameExclude.NoOthers).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);

        AssertSql(
         """
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" ROWS 2 PRECEDING EXCLUDE NO OTHERS) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    [ConditionalFact]
    public virtual void Exclude_CurrentRow()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Rows(2).Exclude(FrameExclude.CurrentRow).Max<decimal?>(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Null(results[0].MaxSalary);

        AssertSql(
         """
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" ROWS 2 PRECEDING EXCLUDE CURRENT ROW) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    [ConditionalFact]
    public virtual void Exclude_Group()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Rows(2).Exclude(FrameExclude.Group).Max<decimal?>(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Null(results[0].MaxSalary);

        AssertSql(
         """
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" ROWS 2 PRECEDING EXCLUDE GROUP) AS "MaxSalary"
FROM "Employees" AS "e"
""");
    }

    [ConditionalFact]
    public virtual void Exclude_Ties()
    {
        using var context = CreateContext();

        var results = context.Employees.Select(e => new
        {
            e.Id,
            e.Name,
            MaxSalary = EF.Functions.Over().PartitionBy(e.DepartmentName).OrderBy(e.Name).Rows(2).Exclude(FrameExclude.Ties).Max(e.Salary)
        }).ToList();

        Assert.Equal(8, results.Count);
        Assert.Equal(1000000.53m, results[0].MaxSalary);

        AssertSql(
         """
SELECT "e"."Id", "e"."Name", MAX("e"."Salary") OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name" ROWS 2 PRECEDING EXCLUDE TIES) AS "MaxSalary"
FROM "Employees" AS "e"
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
SELECT "e"."Id", "e"."Name"
FROM "Employees" AS "e"
ORDER BY ROW_NUMBER() OVER (PARTITION BY "e"."DepartmentName" ORDER BY "e"."Name")
""");
    }

    #endregion

    #endregion

    public void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
