// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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

        private const string FileLineEnding = @"
";

        private static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);
    }
}