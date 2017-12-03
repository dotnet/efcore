// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class DbFunctionsSqlServerTest : DbFunctionsTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        public DbFunctionsSqlServerTest(NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public override void String_Like_Literal()
        {
            base.String_Like_Literal();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE N'%M%'",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void String_Like_Identity()
        {
            base.String_Like_Identity();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE [c].[ContactName]",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void String_Like_Literal_With_Escape()
        {
            base.String_Like_Literal_With_Escape();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE N'!%' ESCAPE N'!'",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void String_DateDiff_Day()
        {
            base.String_DateDiff_Day();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Orders] AS [c]
WHERE DATEDIFF(DAY, [c].[OrderDate], GETDATE()) = 0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void String_DateDiff_Month()
        {
            base.String_DateDiff_Month();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Orders] AS [c]
WHERE DATEDIFF(MONTH, [c].[OrderDate], GETDATE()) = 0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void String_DateDiff_Year()
        {
            base.String_DateDiff_Year();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Orders] AS [c]
WHERE DATEDIFF(YEAR, [c].[OrderDate], GETDATE()) = 0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void String_DateDiff_Hour()
        {
            base.String_DateDiff_Hour();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Orders] AS [c]
WHERE DATEDIFF(HOUR, [c].[OrderDate], GETDATE()) = 0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void String_DateDiff_Minute()
        {
            base.String_DateDiff_Minute();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Orders] AS [c]
WHERE DATEDIFF(MINUTE, [c].[OrderDate], GETDATE()) = 0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void String_DateDiff_Second()
        {
            base.String_DateDiff_Second();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Orders] AS [c]
WHERE DATEDIFF(SECOND, [c].[OrderDate], GETDATE()) = 0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void String_DateDiff_Millisecond()
        {
            base.String_DateDiff_Millisecond();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Orders] AS [c]
WHERE DATEDIFF(MILLISECOND, GETDATE(), DATEADD(day, 1E0, GETDATE())) = 0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void String_DateDiff_Microsecond()
        {
            base.String_DateDiff_Microsecond();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Orders] AS [c]
WHERE DATEDIFF(MICROSECOND, GETDATE(), DATEADD(second, 1E0, GETDATE())) = 0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        public override void String_DateDiff_Convert_To_Date()
        {
            base.String_DateDiff_Convert_To_Date();

            Assert.Equal(
                @"SELECT COUNT(*)
FROM [Orders] AS [c]
WHERE DATEDIFF(DAY, [c].[OrderDate], CONVERT(date, GETDATE())) = 0",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        private string Sql => Fixture.TestSqlLoggerFactory.Sql;
    }
}
