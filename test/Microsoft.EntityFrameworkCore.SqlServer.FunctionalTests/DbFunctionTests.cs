// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.TestModels;
using Xunit;
using Microsoft.EntityFrameworkCore.Specification.Tests;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class DbFunctionTests : IClassFixture<NorthwindDbFunctionSqlServerFixture>
    {
        private readonly NorthwindQuerySqlServerFixture _fixture;

        public DbFunctionTests(NorthwindDbFunctionSqlServerFixture fixture)
        {
            _fixture = fixture;
        }

        protected SqlServerDbFunctionsNorthwindContext CreateContext() => _fixture.CreateContext() as SqlServerDbFunctionsNorthwindContext;

        #region Scalar Tests

        private static int AddFive(int number)
        {
            return number + 5;
        }

        [Fact]
        public void Scalar_Function_ClientEval_Method__As_Translateable_Method_Parameter()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<NotImplementedException>(() => (from e in context.Employees
                                                              where e.EmployeeID == 5
                                                              select new
                                                              {
                                                                  FirstName = e.FirstName,
                                                                  OrderCount = context.EmployeeOrderCount(DbFunctionTests.AddFive(e.EmployeeID - 5))
                                                              }).Single());
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
                               FirstName = e.FirstName,
                               OrderCount = context.EmployeeOrderCount(e.EmployeeID)
                           }).Single();

                Assert.Equal("Steven", emp.FirstName);
                Assert.Equal(42, emp.OrderCount);
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
                               FirstName = e.FirstName,
                               OrderCount = context.EmployeeOrderCount(5)
                           }).Single();

                Assert.Equal("Steven", emp.FirstName);
                Assert.Equal(42, emp.OrderCount);
            }
        }

        [Fact]
        public void Scalar_Function_Anonymous_Type_Select_Parameter()
        {
            using (var context = CreateContext())
            {
                int employeeId = 5;

                var emp = (from e in context.Employees
                           where e.EmployeeID == employeeId
                           select new
                           {
                               FirstName = e.FirstName,
                               OrderCount = context.EmployeeOrderCount(employeeId)
                           }).Single();

                Assert.Equal("Steven", emp.FirstName);
                Assert.Equal(42, emp.OrderCount);
            }
        }

        [Fact]
        public void Scalar_Function_Anonymous_Type_Select_Nested()
        {
            using (var context = CreateContext())
            {
                int employeeId = 5;
                int starCount = 3;

                var emp = (from e in context.Employees
                           where e.EmployeeID == employeeId
                           select new
                           {
                               FirstName = e.FirstName,
                               OrderCount = context.StarValue(starCount, context.EmployeeOrderCount(employeeId))
                           }).Single();

                Assert.Equal("Steven", emp.FirstName);
                Assert.Equal("***42", emp.OrderCount);
            }
        }

        [Fact]
        public void Scalar_Function_Where_Correlated()
        {
            using (var context = CreateContext())
            {
                var emp = (from e in context.Employees
                           where context.IsTopEmployee(e.EmployeeID)
                           select e.EmployeeID.ToString().ToLower()).ToList();

                Assert.Equal(3, emp.Count);
            }
        }

        [Fact]
        public void Scalar_Function_Where_Not_Correlated()
        {
            using (var context = CreateContext())
            {
                DateTime startDate = DateTime.Parse("1/1/1998");

                var emp = (from e in context.Employees
                           where context.GetEmployeeWithMostOrdersAfterDate(startDate) == e.EmployeeID
                           select e).SingleOrDefault();

                Assert.NotNull(emp);
                Assert.True(emp.EmployeeID == 4);
            }
        }

        [Fact]
        public void Scalar_Function_Where_Parameter()
        {
            using (var context = CreateContext())
            {
                var period = SqlServerDbFunctionsNorthwindContext.ReportingPeriod.Winter;

                var emp = (from e in context.Employees
                           where e.EmployeeID == context.GetEmployeeWithMostOrdersAfterDate(
                                                    context.GetReportingPeriodStartDate(period))
                           select e).SingleOrDefault();

                Assert.NotNull(emp);
                Assert.True(emp.EmployeeID == 4);
            }
        }

        [Fact]
        public void Scalar_Function_Where_Nested()
        {
            using (var context = CreateContext())
            {
                var emp = (from e in context.Employees
                           where e.EmployeeID == context.GetEmployeeWithMostOrdersAfterDate(
                                                   context.GetReportingPeriodStartDate(
                                                       SqlServerDbFunctionsNorthwindContext.ReportingPeriod.Winter))
                           select e).SingleOrDefault();

                Assert.NotNull(emp);
                Assert.True(emp.EmployeeID == 4);
            }
        }

        [Fact]
        public void Scalar_Function_Let_Correlated()
        {
            using (var context = CreateContext())
            {
                var emp = (from e in context.Employees
                           let orderCount = context.EmployeeOrderCount(e.EmployeeID)
                           where e.EmployeeID == 5
                           select new
                           {
                               FirstName = e.FirstName,
                               OrderCount = orderCount
                           }).Single();

                Assert.Equal("Steven", emp.FirstName);
                Assert.Equal(42, emp.OrderCount);
            }
        }

        [Fact]
        public void Scalar_Function_Let_Not_Correlated()
        {
            using (var context = CreateContext())
            {
                var emp = (from e in context.Employees
                           let orderCount = context.EmployeeOrderCount(5)
                           where e.EmployeeID == 5
                           select new
                           {
                               FirstName = e.FirstName,
                               OrderCount = orderCount
                           }).Single();

                Assert.Equal("Steven", emp.FirstName);
                Assert.Equal(42, emp.OrderCount);
            }
        }

        [Fact]
        public void Scalar_Function_Let_Not_Parameter()
        {
            var employeeId = 5;

            using (var context = CreateContext())
            {
                var emp = (from e in context.Employees
                           let orderCount = context.EmployeeOrderCount(employeeId)
                           where e.EmployeeID == employeeId
                           select new
                           {
                               FirstName = e.FirstName,
                               OrderCount = orderCount
                           }).Single();

                Assert.Equal("Steven", emp.FirstName);
                Assert.Equal(42, emp.OrderCount);
            }
        }

        #endregion

        [Fact]
        public void Scalar_Function_Let_Nested()
        {
            using (var context = CreateContext())
            {
                int employeeId = 5;
                int starCount = 3;

                var emp = (from e in context.Employees
                           let orderCount = context.StarValue(starCount, context.EmployeeOrderCount(employeeId))
                           where e.EmployeeID == employeeId
                           select new
                           {
                               FirstName = e.FirstName,
                               OrderCount = orderCount
                           }).Single();

                Assert.Equal("Steven", emp.FirstName);
                Assert.Equal("***42", emp.OrderCount);
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
                               where 2 == DbFunctionTests.AddOne(e.EmployeeID)
                               select e.EmployeeID).Single();

                Assert.Equal(1, results);
                Assert.Equal(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]",
                    Sql);
            }
        }

        [Fact]
        public void Scalar_Nested__Function_Unwind_Client_Eval_OrderBy()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               orderby DbFunctionTests.AddOne(e.EmployeeID) 
                               select e.EmployeeID).ToList();

                Assert.Equal(9, results.Count);
                Assert.True(results.SequenceEqual(Enumerable.Range(1, 9))) ;

                Assert.Equal(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]",
                    Sql);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Unwind_Client_Eval_Select()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               orderby e.EmployeeID
                               select DbFunctionTests.AddOne(e.EmployeeID)).ToList();

                Assert.Equal(9, results.Count);
                Assert.True(results.SequenceEqual(Enumerable.Range(2, 9)));

                Assert.Equal(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID]",
                    Sql);
            }
        }
        
        [Fact]
        public void Scalar_Nested_Function_Client_BCL_UDF()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 128 == DbFunctionTests.AddOne(Math.Abs(context.EmployeeOrderCountWithClient(e.EmployeeID)))
                               select e.EmployeeID).Single();

                Assert.Equal(3, results);
                Assert.Equal(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]", 
                    Sql);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Client_UDF_BCL()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 128 == DbFunctionTests.AddOne(context.EmployeeOrderCountWithClient(Math.Abs(e.EmployeeID)))
                               select e.EmployeeID).Single();

                Assert.Equal(3, results);
                Assert.Equal(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]",
                    Sql);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_BCL_Client_UDF()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 128 == Math.Abs(DbFunctionTests.AddOne(context.EmployeeOrderCountWithClient(e.EmployeeID)))
                               select e.EmployeeID).Single();

                Assert.Equal(3, results);
                Assert.Equal(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]",
                    Sql);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_BCL_UDF_Client()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 127 == Math.Abs(context.EmployeeOrderCountWithClient(DbFunctionTests.AddOne(e.EmployeeID)))
                               select e.EmployeeID).Single();

                Assert.Equal(2, results);
                Assert.Equal(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]",
                    Sql);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_UDF_BCL_Client()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 127 == context.EmployeeOrderCountWithClient(Math.Abs(DbFunctionTests.AddOne(e.EmployeeID)))
                               select e.EmployeeID).Single();

                Assert.Equal(2, results);
                Assert.Equal(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]",
                    Sql);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_UDF_Client_BCL()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 127 == context.EmployeeOrderCountWithClient(DbFunctionTests.AddOne(Math.Abs(e.EmployeeID)))
                               select e.EmployeeID).Single();

                Assert.Equal(2, results);
                Assert.Equal(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]",
                    Sql);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Client_BCL()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 3 == DbFunctionTests.AddOne(Math.Abs(e.EmployeeID))
                               select e.EmployeeID).Single();

                Assert.Equal(2, results);
                Assert.Equal(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]",
                    Sql);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Client_UDF()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 128 == DbFunctionTests.AddOne(context.EmployeeOrderCountWithClient(e.EmployeeID))
                               select e.EmployeeID).Single();

                Assert.Equal(3, results);
                Assert.Equal(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]",
                    Sql);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_BCL_Client()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 3 == Math.Abs(DbFunctionTests.AddOne(e.EmployeeID))
                               select e.EmployeeID).Single();

                Assert.Equal(2, results);
                Assert.Equal(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]",
                    Sql);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_BCL_UDF()
        {
            using(var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 127 == Math.Abs(context.EmployeeOrderCountWithClient(e.EmployeeID))
                               select e.EmployeeID).Single();

                Assert.Equal(3, results);
                Assert.Equal(
                    @"SELECT TOP(2) [e].[EmployeeID]
FROM [Employees] AS [e]
WHERE 127 = Abs([dbo].EmployeeOrderCount([e].[EmployeeID]))",
                    Sql);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_UDF_Client()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 127 == context.EmployeeOrderCountWithClient(DbFunctionTests.AddOne(e.EmployeeID))
                               select e.EmployeeID).Single();

                Assert.Equal(2, results);
                Assert.Equal(
                    @"SELECT [e].[EmployeeID]
FROM [Employees] AS [e]",
                    Sql);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_UDF_BCL()
        {
            using (var context = CreateContext())
            {
                var results = (from e in context.Employees
                               where 127 == context.EmployeeOrderCountWithClient(Math.Abs(e.EmployeeID))
                               select e.EmployeeID).Single();

                Assert.Equal(3, results);
                Assert.Equal(
                    @"SELECT TOP(2) [e].[EmployeeID]
FROM [Employees] AS [e]
WHERE 127 = [dbo].EmployeeOrderCount(Abs([e].[EmployeeID]))",
                    Sql);
            }
        }

        private const string FileLineEnding = @"
";

        private static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);
    }
}


