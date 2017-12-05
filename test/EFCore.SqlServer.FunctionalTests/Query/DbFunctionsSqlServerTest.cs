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

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public async void FreeText_Search_Literal()
        {
            using (var context = CreateContext())
            {
                var result = await context.Employees.Where(c => EF.Functions.FreeText(c.Title, "Representative")).ToListAsync(); 

                Assert.Equal(result.First().EmployeeID, 1u);

                Assert.Equal(
                    @"SELECT [c].[EmployeeID], [c].[City], [c].[Country], [c].[FirstName], [c].[ReportsTo], [c].[Title]
FROM [Employees] AS [c]
WHERE FREETEXT([c].[Title], N'Representative')",
                    Sql,
                    ignoreLineEndingDifferences: true,
                    ignoreWhiteSpaceDifferences: true);
            }
        }

        [ConditionalFact]
        public void FreeText_InMemoryUse_Throws()
        {
            Assert.Throws<NotImplementedException>(() => EF.Functions.FreeText("teststring", "teststring"));
            Assert.Throws<NotImplementedException>(() => EF.Functions.FreeText("teststring", "teststring", 1033));
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_Search_Multiple_Words()
        {
            using (var context = CreateContext())
            {
                var result = context.Employees.Where(c => EF.Functions.FreeText(c.Title, "Representative Sales")).Count();

                Assert.Equal(result, 9);

                Assert.Equal(
                    @"SELECT COUNT(*)
FROM [Employees] AS [c]
WHERE FREETEXT([c].[Title], N'Representative Sales')",
                    Sql,
                    ignoreLineEndingDifferences: true,
                    ignoreWhiteSpaceDifferences: true
                    );
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

                Assert.Equal(
                    @"SELECT TOP(2) [c].[EmployeeID], [c].[City], [c].[Country], [c].[FirstName], [c].[ReportsTo], [c].[Title]
FROM [Employees] AS [c]
WHERE FREETEXT([c].[Title], N'President', LANGUAGE 1033)",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_Search_Multiple_Words_With_Language()
        {
            using (var context = CreateContext())
            {
                var result = context.Employees.Where(c => EF.Functions.FreeText(c.Title, "Representative President", 1033)).ToList();

                Assert.Equal(result.First().EmployeeID, 1u);

                Assert.Equal(
                    @"SELECT [c].[EmployeeID], [c].[City], [c].[Country], [c].[FirstName], [c].[ReportsTo], [c].[Title]
FROM [Employees] AS [c]
WHERE FREETEXT([c].[Title], N'Representative President', LANGUAGE 1033)",
                    Sql,
                    ignoreLineEndingDifferences: true,
                    ignoreWhiteSpaceDifferences: true
                    );
            }
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_Search_Multiple_FullText_Calls()
        {
            using (var context = CreateContext())
            {
                var result = context.Employees.Where(c => EF.Functions.FreeText(c.City, "London") && EF.Functions.FreeText(c.Title, "Manager", 1033)).FirstOrDefault();

                Assert.Equal(result.EmployeeID, 5u);

                Assert.Equal(
                    @"SELECT TOP(1) [c].[EmployeeID], [c].[City], [c].[Country], [c].[FirstName], [c].[ReportsTo], [c].[Title]
FROM [Employees] AS [c]
WHERE (FREETEXT([c].[City], N'London')) AND (FREETEXT([c].[Title], N'Manager', LANGUAGE 1033))",
                    Sql,
                    ignoreLineEndingDifferences: true,
                    ignoreWhiteSpaceDifferences: true);
            }
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_Search_Throws_With_No_FullText_Index()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<SqlException>(() => context.Employees.Where(c => EF.Functions.FreeText(c.FirstName, "Fred")).ToArray());
            }
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_Search_Navigation_Property()
        {
            using (var context = CreateContext())
            {
                var result = context.Employees.Where(c => EF.Functions.FreeText(c.Manager.Title, "President") && EF.Functions.FreeText(c.Title, "Inside") && c.FirstName.Contains("Lau")).LastOrDefault();

                Assert.Equal(result.EmployeeID, 8u);

                Assert.Equal(@"SELECT [c].[EmployeeID], [c].[City], [c].[Country], [c].[FirstName], [c].[ReportsTo], [c].[Title]
FROM [Employees] AS [c]
LEFT JOIN [Employees] AS [c.Manager] ON [c].[ReportsTo] = [c.Manager].[EmployeeID]
WHERE ((FREETEXT([c.Manager].[Title], N'President')) AND (FREETEXT([c].[Title], N'Inside'))) AND (CHARINDEX(N'Lau', [c].[FirstName]) > 0)",
                            Sql,
                            ignoreLineEndingDifferences: true,
                            ignoreWhiteSpaceDifferences: true);
            }
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public void FreeText_Search_Navigation_Property_With_Languages()
        {
            using (var context = CreateContext())
            {
                var result = context.Employees.Where(c => EF.Functions.FreeText(c.Manager.Title, "President", 1033) && EF.Functions.FreeText(c.Title, "Inside", 1031) && c.FirstName.Contains("Lau")).LastOrDefault();

                Assert.Equal(result.EmployeeID, 8u);

                Assert.Equal(@"SELECT [c].[EmployeeID], [c].[City], [c].[Country], [c].[FirstName], [c].[ReportsTo], [c].[Title]
FROM [Employees] AS [c]
LEFT JOIN [Employees] AS [c.Manager] ON [c].[ReportsTo] = [c.Manager].[EmployeeID]
WHERE ((FREETEXT([c.Manager].[Title], N'President', LANGUAGE 1033)) AND (FREETEXT([c].[Title], N'Inside', LANGUAGE 1031))) AND (CHARINDEX(N'Lau', [c].[FirstName]) > 0)",
                            Sql,
                            ignoreLineEndingDifferences: true,
                            ignoreWhiteSpaceDifferences: true);
            }
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsFullTextSearch)]
        public async void FreeText_Search_Throws_When_Using_Non_Parameter_Or_Constant_Expression()
        {
            using (var context = CreateContext())
            {
                await Assert.ThrowsAsync<SqlException>(async () => await context.Employees.FirstOrDefaultAsync(e => EF.Functions.FreeText(e.City, e.FirstName)));
                await Assert.ThrowsAsync<SqlException>(async () => await context.Employees.FirstOrDefaultAsync(e => EF.Functions.FreeText(e.City, "")));
                await Assert.ThrowsAsync<SqlException>(async () => await context.Employees.FirstOrDefaultAsync(e => EF.Functions.FreeText(e.City, e.FirstName.ToUpper())));
            }
        }

        private string Sql => Fixture.TestSqlLoggerFactory.Sql;
    }
}
