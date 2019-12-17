// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindDbFunctionsQuerySqlServerTest : NorthwindDbFunctionsQueryTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        public NorthwindDbFunctionsQuerySqlServerTest(
            NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Like_literal()
        {
            base.Like_literal();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE N'%M%'");
        }

        public override void Like_identity()
        {
            base.Like_identity();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE [c].[ContactName]");
        }

        public override void Like_literal_with_escape()
        {
            base.Like_literal_with_escape();

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE N'!%' ESCAPE N'!'");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public async Task FreeText_literal()
        {
            using var context = CreateContext();
            var result = await context.Employees
                .Where(c => EF.Functions.FreeText(c.Title, "Representative"))
                .ToListAsync();

            Assert.Equal(1u, result.First().EmployeeID);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE FREETEXT([e].[Title], N'Representative')");
        }

        [ConditionalFact]
        public void FreeText_client_eval_throws()
        {
            Assert.Throws<InvalidOperationException>(() => EF.Functions.FreeText("teststring", "teststring"));
            Assert.Throws<InvalidOperationException>(() => EF.Functions.FreeText("teststring", "teststring", 1033));
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_multiple_words()
        {
            using var context = CreateContext();
            var result = context.Employees
                .Where(c => EF.Functions.FreeText(c.Title, "Representative Sales"))
                .Count();

            Assert.Equal(9, result);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Employees] AS [e]
WHERE FREETEXT([e].[Title], N'Representative Sales')");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_with_language_term()
        {
            using var context = CreateContext();
            var result = context.Employees.SingleOrDefault(c => EF.Functions.FreeText(c.Title, "President", 1033));

            Assert.Equal(2u, result.EmployeeID);

            AssertSql(
                @"SELECT TOP(2) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE FREETEXT([e].[Title], N'President', LANGUAGE 1033)");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_with_multiple_words_and_language_term()
        {
            using var context = CreateContext();
            var result = context.Employees
                .Where(c => EF.Functions.FreeText(c.Title, "Representative President", 1033))
                .ToList();

            Assert.Equal(1u, result.First().EmployeeID);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE FREETEXT([e].[Title], N'Representative President', LANGUAGE 1033)");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_multiple_predicates()
        {
            using var context = CreateContext();
            var result = context.Employees
                .Where(
                    c => EF.Functions.FreeText(c.City, "London")
                        && EF.Functions.FreeText(c.Title, "Manager", 1033))
                .FirstOrDefault();

            Assert.Equal(5u, result.EmployeeID);

            AssertSql(
                @"SELECT TOP(1) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE FREETEXT([e].[City], N'London')) AND (FREETEXT([e].[Title], N'Manager', LANGUAGE 1033)");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_throws_for_no_FullText_index()
        {
            using var context = CreateContext();
            Assert.Throws<SqlException>(
                () => context.Employees.Where(c => EF.Functions.FreeText(c.FirstName, "Fred")).ToArray());
        }

        [ConditionalFact(Skip = "Issue #18199")]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_through_navigation()
        {
            using var context = CreateContext();
            var result = context.Employees
                .Where(
                    c => EF.Functions.FreeText(c.Manager.Title, "President")
                        && EF.Functions.FreeText(c.Title, "Inside")
                        && c.FirstName.Contains("Lau"))
                .LastOrDefault();

            Assert.Equal(8u, result.EmployeeID);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
LEFT JOIN [Employees] AS [c.Manager] ON [e].[ReportsTo] = [c.Manager].[EmployeeID]
WHERE ((FREETEXT([c.Manager].[Title], N'President')) AND (FREETEXT([e].[Title], N'Inside'))) AND (CHARINDEX(N'Lau', [e].[FirstName]) > 0)");
        }

        [ConditionalFact(Skip = "Issue #18199")]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_through_navigation_with_language_terms()
        {
            using var context = CreateContext();
            var result = context.Employees
                .Where(
                    c => EF.Functions.FreeText(c.Manager.Title, "President", 1033)
                        && EF.Functions.FreeText(c.Title, "Inside", 1031)
                        && c.FirstName.Contains("Lau"))
                .LastOrDefault();

            Assert.Equal(8u, result.EmployeeID);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
LEFT JOIN [Employees] AS [c.Manager] ON [e].[ReportsTo] = [c.Manager].[EmployeeID]
WHERE ((FREETEXT([c.Manager].[Title], N'President', LANGUAGE 1033)) AND (FREETEXT([e].[Title], N'Inside', LANGUAGE 1031))) AND (CHARINDEX(N'Lau', [e].[FirstName]) > 0)");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public async Task FreeText_throws_when_using_non_parameter_or_constant_for_freetext_string()
        {
            using var context = CreateContext();
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

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public async Task FreeText_throws_when_using_non_column_for_property_reference()
        {
            using var context = CreateContext();
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await context.Employees.FirstOrDefaultAsync(
                    e => EF.Functions.FreeText(e.City + "1", "President")));

            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await context.Employees.FirstOrDefaultAsync(
                    e => EF.Functions.FreeText(e.City.ToLower(), "President")));

            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await (from e1 in context.Employees
                                   join m1 in context.Employees.OrderBy(e => e.EmployeeID).Skip(0)
                                       on e1.ReportsTo equals m1.EmployeeID
                                   where EF.Functions.FreeText(m1.Title, "President")
                                   select e1).LastOrDefaultAsync());
        }

        [ConditionalFact]
        public void Contains_should_throw_on_client_eval()
        {
            var exNoLang = Assert.Throws<InvalidOperationException>(() => EF.Functions.Contains("teststring", "teststring"));
            Assert.Equal(
                SqlServerStrings.FunctionOnClient(nameof(SqlServerDbFunctionsExtensions.Contains)),
                exNoLang.Message);

            var exLang = Assert.Throws<InvalidOperationException>(() => EF.Functions.Contains("teststring", "teststring", 1033));
            Assert.Equal(
                SqlServerStrings.FunctionOnClient(nameof(SqlServerDbFunctionsExtensions.Contains)),
                exLang.Message);
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public async Task Contains_should_throw_when_using_non_parameter_or_constant_for_contains_string()
        {
            using var context = CreateContext();
            await Assert.ThrowsAsync<SqlException>(
                async () => await context.Employees.FirstOrDefaultAsync(
                    e => EF.Functions.Contains(e.City, e.FirstName)));

            await Assert.ThrowsAsync<SqlException>(
                async () => await context.Employees.FirstOrDefaultAsync(
                    e => EF.Functions.Contains(e.City, "")));

            await Assert.ThrowsAsync<SqlException>(
                async () => await context.Employees.FirstOrDefaultAsync(
                    e => EF.Functions.Contains(e.City, e.FirstName.ToUpper())));
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void Contains_should_throw_for_no_FullText_index()
        {
            using var context = CreateContext();
            Assert.Throws<SqlException>(
                () => context.Employees.Where(c => EF.Functions.Contains(c.FirstName, "Fred")).ToArray());
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public async Task Contains_literal()
        {
            using var context = CreateContext();
            var result = await context.Employees
                .Where(c => EF.Functions.Contains(c.Title, "Representative"))
                .ToListAsync();

            Assert.Equal(1u, result.First().EmployeeID);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CONTAINS([e].[Title], N'Representative')");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void Contains_with_language_term()
        {
            using var context = CreateContext();
            var result = context.Employees.SingleOrDefault(c => EF.Functions.Contains(c.Title, "President", 1033));

            Assert.Equal(2u, result.EmployeeID);

            AssertSql(
                @"SELECT TOP(2) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CONTAINS([e].[Title], N'President', LANGUAGE 1033)");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public async Task Contains_with_logical_operator()
        {
            using var context = CreateContext();
            var result = await context.Employees
                .Where(c => EF.Functions.Contains(c.Title, "Vice OR Inside"))
                .ToListAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal(2u, result.First().EmployeeID);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CONTAINS([e].[Title], N'Vice OR Inside')");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public async Task Contains_with_prefix_term_and_language_term()
        {
            using var context = CreateContext();
            var result = await context.Employees
                .SingleOrDefaultAsync(c => EF.Functions.Contains(c.Title, "\"Mana*\"", 1033));

            Assert.Equal(5u, result.EmployeeID);

            AssertSql(
                @"SELECT TOP(2) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CONTAINS([e].[Title], N'""Mana*""', LANGUAGE 1033)");
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public async Task Contains_with_proximity_term_and_language_term()
        {
            using var context = CreateContext();
            var result = await context.Employees
                .SingleOrDefaultAsync(c => EF.Functions.Contains(c.Title, "NEAR((Sales, President), 1)", 1033));

            Assert.Equal(2u, result.EmployeeID);

            AssertSql(
                @"SELECT TOP(2) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE CONTAINS([e].[Title], N'NEAR((Sales, President), 1)', LANGUAGE 1033)");
        }

        [ConditionalFact(Skip = "Issue #18199")]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void Contains_through_navigation()
        {
            using var context = CreateContext();
            var result = context.Employees
                .Where(
                    c => EF.Functions.Contains(c.Manager.Title, "President")
                        && EF.Functions.Contains(c.Title, "\"Ins*\""))
                .LastOrDefault();

            Assert.NotNull(result);
            Assert.Equal(8u, result.EmployeeID);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
LEFT JOIN [Employees] AS [c.Manager] ON [e].[ReportsTo] = [c.Manager].[EmployeeID]
WHERE (CONTAINS([c.Manager].[Title], N'President')) AND (CONTAINS([e].[Title], N'""Ins*""'))");
        }

        [ConditionalFact]
        public virtual void DateDiff_Year()
        {
            using var context = CreateContext();
            var count = context.Orders
                .Count(c => EF.Functions.DateDiffYear(c.OrderDate, DateTime.Now) == 0);

            Assert.Equal(0, count);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(YEAR, [o].[OrderDate], GETDATE()) = 0");
        }

        [ConditionalFact]
        public virtual void DateDiff_Month()
        {
            using var context = CreateContext();
            var count = context.Orders
                .Count(c => EF.Functions.DateDiffMonth(c.OrderDate, DateTime.Now) == 0);

            Assert.Equal(0, count);
            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(MONTH, [o].[OrderDate], GETDATE()) = 0");
        }

        [ConditionalFact]
        public virtual void DateDiff_Day()
        {
            using var context = CreateContext();
            var count = context.Orders
                .Count(c => EF.Functions.DateDiffDay(c.OrderDate, DateTime.Now) == 0);

            Assert.Equal(0, count);
            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(DAY, [o].[OrderDate], GETDATE()) = 0");
        }

        [ConditionalFact]
        public virtual void DateDiff_Hour()
        {
            using var context = CreateContext();
            var count = context.Orders
                .Count(c => EF.Functions.DateDiffHour(c.OrderDate, DateTime.Now) == 0);

            Assert.Equal(0, count);
            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(HOUR, [o].[OrderDate], GETDATE()) = 0");
        }

        [ConditionalFact]
        public virtual void DateDiff_Minute()
        {
            using var context = CreateContext();
            var count = context.Orders
                .Count(c => EF.Functions.DateDiffMinute(c.OrderDate, DateTime.Now) == 0);

            Assert.Equal(0, count);
            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(MINUTE, [o].[OrderDate], GETDATE()) = 0");
        }

        [ConditionalFact]
        public virtual void DateDiff_Second()
        {
            using var context = CreateContext();
            var count = context.Orders
                .Count(c => EF.Functions.DateDiffSecond(c.OrderDate, DateTime.Now) == 0);

            Assert.Equal(0, count);
            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(SECOND, [o].[OrderDate], GETDATE()) = 0");
        }

        [ConditionalFact]
        public virtual void DateDiff_Millisecond()
        {
            using var context = CreateContext();
            var count = context.Orders
                .Count(c => EF.Functions.DateDiffMillisecond(DateTime.Now, DateTime.Now.AddDays(1)) == 0);

            Assert.Equal(0, count);
            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(MILLISECOND, GETDATE(), DATEADD(day, CAST(1.0E0 AS int), GETDATE())) = 0");
        }

        [ConditionalFact]
        public virtual void DateDiff_Microsecond()
        {
            using var context = CreateContext();
            var count = context.Orders
                .Count(c => EF.Functions.DateDiffMicrosecond(DateTime.Now, DateTime.Now.AddSeconds(1)) == 0);

            Assert.Equal(0, count);
            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(MICROSECOND, GETDATE(), DATEADD(second, CAST(1.0E0 AS int), GETDATE())) = 0");
        }

        [ConditionalFact]
        public virtual void DateDiff_Nanosecond()
        {
            using var context = CreateContext();
            var count = context.Orders
                .Count(c => EF.Functions.DateDiffNanosecond(DateTime.Now, DateTime.Now.AddSeconds(1)) == 0);

            Assert.Equal(0, count);
            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(NANOSECOND, GETDATE(), DATEADD(second, CAST(1.0E0 AS int), GETDATE())) = 0");
        }

        [ConditionalFact]
        public virtual void DateDiff_Week_datetime()
        {
            using var context = CreateContext();
            var count = context.Orders
                .Count(c => EF.Functions.DateDiffWeek(
                    c.OrderDate,
                    new DateTime(1998, 5, 6, 0, 0, 0)) == 5);

            Assert.Equal(16, count);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(WEEK, [o].[OrderDate], '1998-05-06T00:00:00.000') = 5");
        }

        [ConditionalFact]
        public virtual void DateDiff_Week_datetimeoffset()
        {
            using var context = CreateContext();
            var count = context.Orders
                .Count(c => EF.Functions.DateDiffWeek(
                    c.OrderDate,
                    new DateTimeOffset(1998, 5, 6, 0, 0, 0, TimeSpan.Zero)) == 5);

            Assert.Equal(16, count);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE DATEDIFF(WEEK, CAST([o].[OrderDate] AS datetimeoffset), '1998-05-06T00:00:00.0000000+00:00') = 5");
        }

        [ConditionalFact]
        public virtual void DateDiff_Week_parameters_null()
        {
            using var context = CreateContext();
            var count = context.Orders
                .Count(c => EF.Functions.DateDiffWeek(
                    null,
                    new DateTimeOffset(1998, 5, 6, 0, 0, 0, TimeSpan.Zero)) == 5);

            Assert.Equal(0, count);

            AssertSql(
                @"@__p_0='False'

SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE @__p_0 = CAST(1 AS bit)");
        }

        [ConditionalFact]
        public virtual void DateDiff_Week_server_vs_client_eval_datetime()
        {
            using var context = CreateContext();
            var endDate = new DateTime(1998, 5, 6, 0, 0, 0);

            var orders = context.Orders
                .OrderBy(p => p.OrderID)
                .Take(200)
                .Select(c => new
                {
                    Weeks = EF.Functions.DateDiffWeek(c.OrderDate, endDate),
                    c.OrderDate
                });

            foreach (var order in orders)
            {
                var weeks = EF.Functions.DateDiffWeek(order.OrderDate, endDate);

                Assert.Equal(weeks, order.Weeks);
            }

            AssertSql(
                @"@__p_0='200'
@__endDate_2='1998-05-06T00:00:00' (Nullable = true) (DbType = DateTime)

SELECT TOP(@__p_0) DATEDIFF(WEEK, [o].[OrderDate], @__endDate_2) AS [Weeks], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[OrderID]");
        }

        [ConditionalFact]
        public virtual void DateDiff_Week_server_vs_client_eval_datetimeoffset()
        {
            using var context = CreateContext();
            var endDate = new DateTimeOffset(1998, 5, 6, 0, 0, 0, TimeSpan.Zero);

            var orders = context.Orders
                .OrderBy(p => p.OrderID)
                .Take(200)
                .Select(c => new
                {
                    Weeks = EF.Functions.DateDiffWeek(c.OrderDate, endDate),
                    c.OrderDate
                });

            foreach (var order in orders)
            {
                var weeks = EF.Functions.DateDiffWeek(order.OrderDate, endDate);

                Assert.Equal(weeks, order.Weeks);
            }

            AssertSql(
                @"@__p_0='200'
@__endDate_2='1998-05-06T00:00:00.0000000+00:00' (Nullable = true)

SELECT TOP(@__p_0) DATEDIFF(WEEK, CAST([o].[OrderDate] AS datetimeoffset), @__endDate_2) AS [Weeks], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[OrderID]");
        }
        [ConditionalFact]
        public virtual void IsDate_not_valid()
        {
            using var context = CreateContext();
            var actual = context
                .Orders
                .Where(c => !EF.Functions.IsDate(c.CustomerID))
                .Select(c => EF.Functions.IsDate(c.CustomerID))
                .FirstOrDefault();

            Assert.False(actual);

            AssertSql(
                @"SELECT TOP(1) CAST(ISDATE([o].[CustomerID]) AS bit)
FROM [Orders] AS [o]
WHERE CAST(ISDATE([o].[CustomerID]) AS bit) <> CAST(1 AS bit)");
        }

        [ConditionalFact]
        public virtual void IsDate_valid()
        {
            using var context = CreateContext();
            var actual = context
                .Orders
                .Where(c => EF.Functions.IsDate(c.OrderDate.Value.ToString()))
                .Select(c => EF.Functions.IsDate(c.OrderDate.Value.ToString()))
                .FirstOrDefault();

            Assert.True(actual);

            AssertSql(
                @"SELECT TOP(1) CAST(ISDATE(CONVERT(VARCHAR(100), [o].[OrderDate])) AS bit)
FROM [Orders] AS [o]
WHERE CAST(ISDATE(CONVERT(VARCHAR(100), [o].[OrderDate])) AS bit) = CAST(1 AS bit)");
        }

        [ConditionalFact]
        public virtual void IsDate_join_fields()
        {
            using var context = CreateContext();
            var count = context.Orders.Count(c => EF.Functions.IsDate(c.CustomerID + c.OrderID));

            Assert.Equal(0, count);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE CAST(ISDATE(COALESCE([o].[CustomerID], N'') + CAST([o].[OrderID] AS nchar(5))) AS bit) = CAST(1 AS bit)");
        }

        [ConditionalFact]
        public void IsDate_should_throw_on_client_eval()
        {
            var exIsDate = Assert.Throws<InvalidOperationException>(() => EF.Functions.IsDate("#ISDATE#"));

            Assert.Equal(
                SqlServerStrings.FunctionOnClient(nameof(SqlServerDbFunctionsExtensions.IsDate)),
                exIsDate.Message);
        }

        [ConditionalFact]
        public virtual void DateTimeFromParts_column_compare()
        {
            using (var context = CreateContext())
            {
                var count = context.Orders
                    .Count(c => c.OrderDate > EF.Functions.DateTimeFromParts(DateTime.Now.Year, 12, 31, 23, 59, 59, 999));

                Assert.Equal(0, count);

                AssertSql(
                    @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE [o].[OrderDate] > DATETIMEFROMPARTS(DATEPART(year, GETDATE()), 12, 31, 23, 59, 59, 999)");
            }
        }

        [ConditionalFact]
        public virtual void DateTimeFromParts_constant_compare()
        {
            using (var context = CreateContext())
            {
                var count = context.Orders
                    .Count(c => new DateTime(2018, 12, 29, 23, 20, 40) > EF.Functions.DateTimeFromParts(DateTime.Now.Year, 12, 31, 23, 59, 59, 999));

                Assert.Equal(0, count);

                AssertSql(
                    @"SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE '2018-12-29T23:20:40.000' > DATETIMEFROMPARTS(DATEPART(year, GETDATE()), 12, 31, 23, 59, 59, 999)");
            }
        }

        [ConditionalFact]
        public virtual void DateTimeFromParts_compare_with_local_variable()
        {
            var dateTime = new DateTime(1919, 12, 12, 10, 20, 15, 0);
            using (var context = CreateContext())
            {
                var count = context.Orders
                    .Count(c => dateTime > EF.Functions.DateTimeFromParts(DateTime.Now.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond));

                Assert.Equal(0, count);

                AssertSql(
                    @$"@__dateTime_0='1919-12-12T10:20:15' (DbType = DateTime)
@__dateTime_Month_2='12'
@__dateTime_Day_3='12'
@__dateTime_Hour_4='10'
@__dateTime_Minute_5='20'
@__dateTime_Second_6='15'
@__dateTime_Millisecond_7='0'

SELECT COUNT(*)
FROM [Orders] AS [o]
WHERE @__dateTime_0 > DATETIMEFROMPARTS(DATEPART(year, GETDATE()), @__dateTime_Month_2, @__dateTime_Day_3, @__dateTime_Hour_4, @__dateTime_Minute_5, @__dateTime_Second_6, @__dateTime_Millisecond_7)");
            }
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
