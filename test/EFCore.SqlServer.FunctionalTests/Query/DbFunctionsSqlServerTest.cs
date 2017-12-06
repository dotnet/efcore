// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class DbFunctionsSqlServerTest : DbFunctionsTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        public DbFunctionsSqlServerTest(NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        public override void String_Like_Literal()
        {
            base.String_Like_Literal();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE N'%M%'");
        }

        public override void String_Like_Identity()
        {
            base.String_Like_Identity();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE [c].[ContactName]");
        }

        public override void String_Like_Literal_With_Escape()
        {
            base.String_Like_Literal_With_Escape();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE N'!%' ESCAPE N'!'");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public async void FreeText_Search_Literal()
        {
            using (var context = CreateContext())
            {
                var result = await context.Employees
                    .Where(c => EF.Functions.FreeText(c.Title, "Representative"))
                    .ToListAsync();

                Assert.Equal(result.First().EmployeeID, 1u);

                AssertSql(
                    @"SELECT [c].[EmployeeID], [c].[City], [c].[Country], [c].[FirstName], [c].[ReportsTo], [c].[Title]
FROM [Employees] AS [c]
WHERE FREETEXT([c].[Title], N'Representative')");
            }
        }

        [ConditionalFact]
        public void FreeText_InMemoryUse_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => EF.Functions.FreeText("teststring", "teststring"));
            Assert.Throws<InvalidOperationException>(() => EF.Functions.FreeText("teststring", "teststring", 1033));
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_Search_Multiple_Words()
        {
            using (var context = CreateContext())
            {
                var result = context.Employees
                    .Where(c => EF.Functions.FreeText(c.Title, "Representative Sales"))
                    .Count();

                Assert.Equal(result, 9);

                AssertSql(
                    @"SELECT COUNT(*)
FROM [Employees] AS [c]
WHERE FREETEXT([c].[Title], N'Representative Sales')");
            }
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_Search_With_Language()
        {
            using (var context = CreateContext())
            {
                var result = context.Employees.SingleOrDefault(c => EF.Functions.FreeText(c.Title, "President", 1033));

                Assert.Equal(result.EmployeeID, 2u);

                AssertSql(
                    @"SELECT TOP(2) [c].[EmployeeID], [c].[City], [c].[Country], [c].[FirstName], [c].[ReportsTo], [c].[Title]
FROM [Employees] AS [c]
WHERE FREETEXT([c].[Title], N'President', LANGUAGE 1033)");
            }
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_Search_Multiple_Words_With_Language()
        {
            using (var context = CreateContext())
            {
                var result = context.Employees
                    .Where(c => EF.Functions.FreeText(c.Title, "Representative President", 1033))
                    .ToList();

                Assert.Equal(result.First().EmployeeID, 1u);

                AssertSql(
                    @"SELECT [c].[EmployeeID], [c].[City], [c].[Country], [c].[FirstName], [c].[ReportsTo], [c].[Title]
FROM [Employees] AS [c]
WHERE FREETEXT([c].[Title], N'Representative President', LANGUAGE 1033)");
            }
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_Search_Multiple_FullText_Calls()
        {
            using (var context = CreateContext())
            {
                var result = context.Employees
                    .Where(c => EF.Functions.FreeText(c.City, "London")
                        && EF.Functions.FreeText(c.Title, "Manager", 1033))
                    .FirstOrDefault();

                Assert.Equal(result.EmployeeID, 5u);

                AssertSql(
                    @"SELECT TOP(1) [c].[EmployeeID], [c].[City], [c].[Country], [c].[FirstName], [c].[ReportsTo], [c].[Title]
FROM [Employees] AS [c]
WHERE (FREETEXT([c].[City], N'London')) AND (FREETEXT([c].[Title], N'Manager', LANGUAGE 1033))");
            }
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_Search_Throws_With_No_FullText_Index()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<SqlException>(
                    () => context.Employees.Where(c => EF.Functions.FreeText(c.FirstName, "Fred")).ToArray());
            }
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_Search_Navigation_Property()
        {
            using (var context = CreateContext())
            {
                var result = context.Employees
                    .Where(c => EF.Functions.FreeText(c.Manager.Title, "President")
                        && EF.Functions.FreeText(c.Title, "Inside")
                        && c.FirstName.Contains("Lau"))
                    .LastOrDefault();

                Assert.Equal(result.EmployeeID, 8u);

                AssertSql(
                    @"SELECT [c].[EmployeeID], [c].[City], [c].[Country], [c].[FirstName], [c].[ReportsTo], [c].[Title]
FROM [Employees] AS [c]
LEFT JOIN [Employees] AS [c.Manager] ON [c].[ReportsTo] = [c.Manager].[EmployeeID]
WHERE ((FREETEXT([c.Manager].[Title], N'President')) AND (FREETEXT([c].[Title], N'Inside'))) AND (CHARINDEX(N'Lau', [c].[FirstName]) > 0)");
            }
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_Search_Navigation_Property_With_Languages()
        {
            using (var context = CreateContext())
            {
                var result = context.Employees
                    .Where(c => EF.Functions.FreeText(c.Manager.Title, "President", 1033)
                        && EF.Functions.FreeText(c.Title, "Inside", 1031)
                        && c.FirstName.Contains("Lau"))
                    .LastOrDefault();

                Assert.Equal(result.EmployeeID, 8u);

                AssertSql(
                    @"SELECT [c].[EmployeeID], [c].[City], [c].[Country], [c].[FirstName], [c].[ReportsTo], [c].[Title]
FROM [Employees] AS [c]
LEFT JOIN [Employees] AS [c.Manager] ON [c].[ReportsTo] = [c.Manager].[EmployeeID]
WHERE ((FREETEXT([c.Manager].[Title], N'President', LANGUAGE 1033)) AND (FREETEXT([c].[Title], N'Inside', LANGUAGE 1031))) AND (CHARINDEX(N'Lau', [c].[FirstName]) > 0)");
            }
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public async void FreeText_Search_Throws_When_Using_Non_Parameter_Or_Constant_Expression()
        {
            using (var context = CreateContext())
            {
                await Assert.ThrowsAsync<SqlException>(
                    async () => await context.Employees.FirstOrDefaultAsync(
                        e => EF.Functions.FreeText(e.City, e.FirstName)));

                await Assert.ThrowsAsync<SqlException>(
                    async () => await context.Employees.FirstOrDefaultAsync(
                        e => EF.Functions.FreeText(e.City, "")));

                await Assert.ThrowsAsync<SqlException>(
                    async () => await context.Employees.FirstOrDefaultAsync(
                        e => EF.Functions.FreeText(e.City, e.FirstName.ToUpper())));
            }
        }

        public override void String_DateDiff_Day()
        {
            base.String_DateDiff_Day();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [c]
WHERE DATEDIFF(DAY, [c].[OrderDate], GETDATE()) = 0");
        }

        public override void String_DateDiff_Month()
        {
            base.String_DateDiff_Month();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [c]
WHERE DATEDIFF(MONTH, [c].[OrderDate], GETDATE()) = 0");
        }

        public override void String_DateDiff_Year()
        {
            base.String_DateDiff_Year();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [c]
WHERE DATEDIFF(YEAR, [c].[OrderDate], GETDATE()) = 0");
        }

        public override void String_DateDiff_Hour()
        {
            base.String_DateDiff_Hour();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [c]
WHERE DATEDIFF(HOUR, [c].[OrderDate], GETDATE()) = 0");
        }

        public override void String_DateDiff_Minute()
        {
            base.String_DateDiff_Minute();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [c]
WHERE DATEDIFF(MINUTE, [c].[OrderDate], GETDATE()) = 0");
        }

        public override void String_DateDiff_Second()
        {
            base.String_DateDiff_Second();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [c]
WHERE DATEDIFF(SECOND, [c].[OrderDate], GETDATE()) = 0");
        }

        public override void String_DateDiff_Millisecond()
        {
            base.String_DateDiff_Millisecond();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [c]
WHERE DATEDIFF(MILLISECOND, GETDATE(), DATEADD(day, 1E0, GETDATE())) = 0");
        }

        public override void String_DateDiff_Microsecond()
        {
            base.String_DateDiff_Microsecond();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [c]
WHERE DATEDIFF(MICROSECOND, GETDATE(), DATEADD(second, 1E0, GETDATE())) = 0");
        }

        public override void String_DateDiff_Nanosecond()
        {
            base.String_DateDiff_Nanosecond();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [c]
WHERE DATEDIFF(NANOSECOND, GETDATE(), DATEADD(second, 1E0, GETDATE())) = 0");
        }

        public override void String_DateDiff_Convert_To_Date()
        {
            base.String_DateDiff_Convert_To_Date();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [c]
WHERE DATEDIFF(DAY, [c].[OrderDate], CONVERT(date, GETDATE())) = 0");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
