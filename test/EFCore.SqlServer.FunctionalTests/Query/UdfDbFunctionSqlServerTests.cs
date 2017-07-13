// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class UdfDbFunctionSqlServerTests : IClassFixture<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        public UdfDbFunctionSqlServerTests(NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture)
        {
            Fixture = fixture;

            Fixture.TestSqlLoggerFactory.Clear();
        }

        protected NorthwindQuerySqlServerFixture<NoopModelCustomizer> Fixture { get; }

        protected NorthwindRelationalContext CreateContext() => Fixture.CreateContext() as NorthwindRelationalContext;

        #region Scalar Tests

        private static int AddFive(int number)
        {
            return number + 5;
        }

        [Fact]
        private void Scalar_Function_Extension_Method()
        {
            using (var context = CreateContext())
            {
                var len = context.Employees.Count(e => NorthwindRelationalContext.IsDate(e.FirstName) == false);

                Assert.Equal(9, len);

                AssertSql(
                    @"SELECT COUNT(*)
FROM [Employees] AS [e]
WHERE CASE
    WHEN IsDate([e].[FirstName]) = 1
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = 0");
            }
        }

        [Fact]
        private void Scalar_Function_With_Translator_Translates()
        {
            using (var context = CreateContext())
            {
                var employeeId = 5;

                var len = context.Employees.Where(e => e.EmployeeID == employeeId).Select(e => NorthwindRelationalContext.MyCustomLength(e.FirstName)).Single();

                Assert.Equal(6, len);

                AssertSql(
                    @"@__employeeId_0='5'

SELECT TOP(2) len([e].[FirstName])
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = @__employeeId_0");
            }
        }

        [Fact]
        public void Scalar_Function_ClientEval_Method_As_Translateable_Method_Parameter()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<NotImplementedException>(() => (from e in context.Employees
                                                              where e.EmployeeID == 5
                                                              select new
                                                              {
                                                                  e.FirstName,
                                                                  OrderCount = NorthwindRelationalContext.EmployeeOrderCount(AddFive(e.EmployeeID - 5))
                                                              }).Single());
            }
        }

        [Fact]
        public void Scalar_Function_Constant_Parameter()
        {
            using (var context = CreateContext())
            {
                var employeeId = 5;

                var emps = context.Employees.Select(e => NorthwindRelationalContext.EmployeeOrderCount(employeeId)).ToList();

                Assert.Equal(9, emps.Count);

                AssertSql(
                    @"@__employeeId_0='5'

SELECT [dbo].EmployeeOrderCount(@__employeeId_0)
FROM [Employees] AS [e]");
            }
        }

        [Fact]
        public void Scalar_Function_Anonymous_Type_Select_Correlated()
        {
            using (var context = CreateContext())
            {
                var emp = (from e in context.Employees
                           where e.EmployeeID == 5
                           select new
                           {
                               e.FirstName,
                               OrderCount = NorthwindRelationalContext.EmployeeOrderCount(e.EmployeeID)
                           }).Single();

                Assert.Equal("Steven", emp.FirstName);
                Assert.Equal(42, emp.OrderCount);

                AssertSql(
                    @"SELECT TOP(2) [e].[FirstName], [dbo].EmployeeOrderCount([e].[EmployeeID]) AS [OrderCount]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 5");
            }
        }

        [Fact]
        public void Scalar_Function_Anonymous_Type_Select_Not_Correlated()
        {
            using (var context = CreateContext())
            {
                var emp = (from e in context.Employees
                           where e.EmployeeID == 5
                           select new
                           {
                               e.FirstName,
                               OrderCount = NorthwindRelationalContext.EmployeeOrderCount(5)
                           }).Single();

                Assert.Equal("Steven", emp.FirstName);
                Assert.Equal(42, emp.OrderCount);

                AssertSql(
                    @"SELECT TOP(2) [e].[FirstName], [dbo].EmployeeOrderCount(5) AS [OrderCount]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 5");
            }
        }

        [Fact]
        public void Scalar_Function_Anonymous_Type_Select_Parameter()
        {
            using (var context = CreateContext())
            {
                var employeeId = 5;

                var emp = (from e in context.Employees
                           where e.EmployeeID == employeeId
                           select new
                           {
                               e.FirstName,
                               OrderCount = NorthwindRelationalContext.EmployeeOrderCount(employeeId)
                           }).Single();

                Assert.Equal("Steven", emp.FirstName);
                Assert.Equal(42, emp.OrderCount);

                AssertSql(
                    @"@__employeeId_1='5'
@__employeeId_0='5'

SELECT TOP(2) [e].[FirstName], [dbo].EmployeeOrderCount(@__employeeId_1) AS [OrderCount]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = @__employeeId_0");
            }
        }

        [Fact]
        public void Scalar_Function_Anonymous_Type_Select_Nested()
        {
            using (var context = CreateContext())
            {
                var employeeId = 5;
                var starCount = 3;

                var emp = (from e in context.Employees
                           where e.EmployeeID == employeeId
                           select new
                           {
                               e.FirstName,
                               OrderCount = NorthwindRelationalContext.StarValue(starCount, NorthwindRelationalContext.EmployeeOrderCount(employeeId))
                           }).Single();

                Assert.Equal("Steven", emp.FirstName);
                Assert.Equal("***42", emp.OrderCount);

                AssertSql(
                    @"@__starCount_1='3'
@__employeeId_2='5'
@__employeeId_0='5'

SELECT TOP(2) [e].[FirstName], [dbo].StarValue(@__starCount_1, [dbo].EmployeeOrderCount(@__employeeId_2)) AS [OrderCount]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = @__employeeId_0");
            }
        }

        [Fact]
        public void Scalar_Function_Where_Correlated()
        {
            using (var context = CreateContext())
            {
                var emp = (from e in context.Employees
                           where NorthwindRelationalContext.IsTopEmployee(e.EmployeeID)
                           select e.EmployeeID.ToString().ToLower()).ToList();

                Assert.Equal(3, emp.Count);

                AssertSql(
                    @"SELECT LOWER(CONVERT(VARCHAR(11), [e].[EmployeeID]))
FROM [Employees] AS [e]
WHERE [dbo].IsTopEmployee([e].[EmployeeID]) = 1");
            }
        }

        [Fact]
        public void Scalar_Function_Where_Not_Correlated()
        {
            using (var context = CreateContext())
            {
                var startDate = DateTime.Parse("1/1/1998");

                var emp = (from e in context.Employees
                           where NorthwindRelationalContext.GetEmployeeWithMostOrdersAfterDate(startDate) == e.EmployeeID
                           select e).SingleOrDefault();

                Assert.NotNull(emp);
                Assert.True(emp.EmployeeID == 4);

                AssertSql(
                    @"@__startDate_0='01/01/1998 00:00:00'

SELECT TOP(2) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [dbo].GetEmployeeWithMostOrdersAfterDate(@__startDate_0) = [e].[EmployeeID]");
            }
        }

        [Fact]
        public void Scalar_Function_Where_Parameter()
        {
            using (var context = CreateContext())
            {
                var period = NorthwindRelationalContext.ReportingPeriod.Winter;

                var emp = (from e in context.Employees
                           where e.EmployeeID == NorthwindRelationalContext.GetEmployeeWithMostOrdersAfterDate(
                               NorthwindRelationalContext.GetReportingPeriodStartDate(period))
                           select e).SingleOrDefault();

                Assert.NotNull(emp);
                Assert.True(emp.EmployeeID == 4);

                AssertSql(
                    @"@__period_0='Winter'

SELECT TOP(2) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = [dbo].GetEmployeeWithMostOrdersAfterDate([dbo].GetReportingPeriodStartDate(@__period_0))");
            }
        }

        [Fact]
        public void Scalar_Function_Where_Nested()
        {
            using (var context = CreateContext())
            {
                var emp = (from e in context.Employees
                           where e.EmployeeID == NorthwindRelationalContext.GetEmployeeWithMostOrdersAfterDate(
                               NorthwindRelationalContext.GetReportingPeriodStartDate(
                                   NorthwindRelationalContext.ReportingPeriod.Winter))
                           select e).SingleOrDefault();

                Assert.NotNull(emp);
                Assert.True(emp.EmployeeID == 4);

                AssertSql(
                    @"SELECT TOP(2) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = [dbo].GetEmployeeWithMostOrdersAfterDate([dbo].GetReportingPeriodStartDate(0))");
            }
        }

        [Fact]
        public void Scalar_Function_Let_Correlated()
        {
            using (var context = CreateContext())
            {
                var emp = (from e in context.Employees
                           let orderCount = NorthwindRelationalContext.EmployeeOrderCount(e.EmployeeID)
                           where e.EmployeeID == 5
                           select new
                           {
                               e.FirstName,
                               OrderCount = orderCount
                           }).Single();

                Assert.Equal("Steven", emp.FirstName);
                Assert.Equal(42, emp.OrderCount);

                AssertSql(
                    @"SELECT TOP(2) [e].[FirstName], [dbo].EmployeeOrderCount([e].[EmployeeID]) AS [OrderCount]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 5");
            }
        }

        [Fact]
        public void Scalar_Function_Let_Not_Correlated()
        {
            using (var context = CreateContext())
            {
                var emp = (from e in context.Employees
                           let orderCount = NorthwindRelationalContext.EmployeeOrderCount(5)
                           where e.EmployeeID == 5
                           select new
                           {
                               e.FirstName,
                               OrderCount = orderCount
                           }).Single();

                Assert.Equal("Steven", emp.FirstName);
                Assert.Equal(42, emp.OrderCount);

                AssertSql(
                    @"SELECT TOP(2) [e].[FirstName], [dbo].EmployeeOrderCount(5) AS [OrderCount]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = 5");
            }
        }

        [Fact]
        public void Scalar_Function_Let_Not_Parameter()
        {
            var employeeId = 5;

            using (var context = CreateContext())
            {
                var emp = (from e in context.Employees
                           let orderCount = NorthwindRelationalContext.EmployeeOrderCount(employeeId)
                           where e.EmployeeID == employeeId
                           select new
                           {
                               e.FirstName,
                               OrderCount = orderCount
                           }).Single();

                Assert.Equal("Steven", emp.FirstName);
                Assert.Equal(42, emp.OrderCount);

                AssertSql(
                    @"@__employeeId_0='5'
@__employeeId_1='5'

SELECT TOP(2) [e].[FirstName], [dbo].EmployeeOrderCount(@__employeeId_0) AS [OrderCount]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = @__employeeId_1");
            }
        }

        #endregion

        [Fact]
        public void Scalar_Function_Let_Nested()
        {
            using (var context = CreateContext())
            {
                var employeeId = 5;
                var starCount = 3;

                var emp = (from e in context.Employees
                           let orderCount = NorthwindRelationalContext.StarValue(starCount, NorthwindRelationalContext.EmployeeOrderCount(employeeId))
                           where e.EmployeeID == employeeId
                           select new
                           {
                               e.FirstName,
                               OrderCount = orderCount
                           }).Single();

                Assert.Equal("Steven", emp.FirstName);
                Assert.Equal("***42", emp.OrderCount);

                AssertSql(
                    @"@__starCount_0='3'
@__employeeId_1='5'
@__employeeId_2='5'

SELECT TOP(2) [e].[FirstName], [dbo].StarValue(@__starCount_0, [dbo].EmployeeOrderCount(@__employeeId_1)) AS [OrderCount]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = @__employeeId_2");
            }
        }

        public static int AddOne(int num)
        {
            return num + 1;
        }

        [Fact]
        public void Scalar_Nested_Function_Unwind_Client_Eval_Where()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 2 == AddOne(e.EmployeeID)
                               select e.EmployeeID).Single();

                Assert.Equal(1, results);
                AssertSql(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]");
            }
        }

        [Fact]
        public void Scalar_Nested__Function_Unwind_Client_Eval_OrderBy()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               orderby AddOne(e.EmployeeID)
                               select e.EmployeeID).ToList();

                Assert.Equal(9, results.Count);
                Assert.True(results.SequenceEqual(Enumerable.Range(1, 9)));

                AssertSql(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Unwind_Client_Eval_Select()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               orderby e.EmployeeID
                               select AddOne(e.EmployeeID)).ToList();

                Assert.Equal(9, results.Count);
                Assert.True(results.SequenceEqual(Enumerable.Range(2, 9)));

                AssertSql(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Client_BCL_UDF()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 128 == AddOne(Math.Abs(NorthwindRelationalContext.EmployeeOrderCountWithClient(e.EmployeeID)))
                               select e.EmployeeID).Single();

                Assert.Equal(3, results);
                AssertSql(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Client_UDF_BCL()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 128 == AddOne(NorthwindRelationalContext.EmployeeOrderCountWithClient(Math.Abs(e.EmployeeID)))
                               select e.EmployeeID).Single();

                Assert.Equal(3, results);
                AssertSql(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_BCL_Client_UDF()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 128 == Math.Abs(AddOne(NorthwindRelationalContext.EmployeeOrderCountWithClient(e.EmployeeID)))
                               select e.EmployeeID).Single();

                Assert.Equal(3, results);
                AssertSql(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_BCL_UDF_Client()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 127 == Math.Abs(NorthwindRelationalContext.EmployeeOrderCountWithClient(AddOne(e.EmployeeID)))
                               select e.EmployeeID).Single();

                Assert.Equal(2, results);
                AssertSql(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_UDF_BCL_Client()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 127 == NorthwindRelationalContext.EmployeeOrderCountWithClient(Math.Abs(AddOne(e.EmployeeID)))
                               select e.EmployeeID).Single();

                Assert.Equal(2, results);
                AssertSql(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_UDF_Client_BCL()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 127 == NorthwindRelationalContext.EmployeeOrderCountWithClient(AddOne(Math.Abs(e.EmployeeID)))
                               select e.EmployeeID).Single();

                Assert.Equal(2, results);
                AssertSql(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Client_BCL()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 3 == AddOne(Math.Abs(e.EmployeeID))
                               select e.EmployeeID).Single();

                Assert.Equal(2, results);
                AssertSql(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Client_UDF()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 128 == AddOne(NorthwindRelationalContext.EmployeeOrderCountWithClient(e.EmployeeID))
                               select e.EmployeeID).Single();

                Assert.Equal(3, results);
                AssertSql(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_BCL_Client()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 3 == Math.Abs(AddOne(e.EmployeeID))
                               select e.EmployeeID).Single();

                Assert.Equal(2, results);
                AssertSql(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_BCL_UDF()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 127 == Math.Abs(NorthwindRelationalContext.EmployeeOrderCountWithClient(e.EmployeeID))
                               select e.EmployeeID).Single();

                Assert.Equal(3, results);
                AssertSql(
                    @"SELECT TOP(2) [e].[EmployeeID]
FROM [Employees] AS [e]
WHERE 127 = ABS([dbo].EmployeeOrderCount([e].[EmployeeID]))");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_UDF_Client()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 127 == NorthwindRelationalContext.EmployeeOrderCountWithClient(AddOne(e.EmployeeID))
                               select e.EmployeeID).Single();

                Assert.Equal(2, results);
                AssertSql(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_UDF_BCL()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 127 == NorthwindRelationalContext.EmployeeOrderCountWithClient(Math.Abs(e.EmployeeID))
                               select e.EmployeeID).Single();

                Assert.Equal(3, results);
                AssertSql(
                    @"SELECT TOP(2) [e].[EmployeeID]
FROM [Employees] AS [e]
WHERE 127 = [dbo].EmployeeOrderCount(ABS([e].[EmployeeID]))");
            }
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
