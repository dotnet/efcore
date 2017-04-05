// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class WarningsSqlServerTest : WarningsTestBase<WarningsSqlServerFixture>
    {
        public WarningsSqlServerTest(WarningsSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public override void Does_not_throw_for_top_level_single()
        {
            base.Does_not_throw_for_top_level_single();

            Assert.Equal(
                @"SELECT TOP(2) [x].[OrderID], [x].[CustomerID], [x].[EmployeeID], [x].[OrderDate]
FROM [Orders] AS [x]
WHERE [x].[OrderID] = 10248",
                Sql);
        }

        public override void Paging_operation_without_orderby_issues_warning()
        {
            base.Paging_operation_without_orderby_issues_warning();

            Assert.True(TestSqlLoggerFactory.Log.Contains(CoreStrings.RowLimitingOperationWithoutOrderBy(
                "(from Customer <generated>_2 in DbSet<Customer> select [<generated>_2]).Skip(__p_0).Take(__p_1)")));
        }

        public override void FirstOrDefault_without_orderby_and_filter_issues_warning_subquery()
        {
            base.FirstOrDefault_without_orderby_and_filter_issues_warning_subquery();

            Assert.True(TestSqlLoggerFactory.Log.Contains(CoreStrings.FirstWithoutOrderByAndFilter(
                "(from Order <generated>_1 in [c].Orders select [<generated>_1].OrderID).FirstOrDefault()")));
        }

        public override void FirstOrDefault_without_orderby_but_with_filter_doesnt_issue_warning()
        {
            base.FirstOrDefault_without_orderby_but_with_filter_doesnt_issue_warning();

            Assert.False(TestSqlLoggerFactory.Log.Contains(CoreStrings.FirstWithoutOrderByAndFilter(
                @"(from Customer c in DbSet<Customer> where c.CustomerID == ""ALFKI"" select c).FirstOrDefault()")));
        }

        public override void Single_SingleOrDefault_without_orderby_doesnt_issue_warning()
        {
            base.Single_SingleOrDefault_without_orderby_doesnt_issue_warning();

            Assert.False(TestSqlLoggerFactory.Log.Contains(CoreStrings.FirstWithoutOrderByAndFilter(
                @"(from Customer c in DbSet<Customer> where c.CustomerID == ""ALFKI"" select c).Single()")));
        }

        private const string FileLineEnding = @"
";

        private static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);
    }
}
