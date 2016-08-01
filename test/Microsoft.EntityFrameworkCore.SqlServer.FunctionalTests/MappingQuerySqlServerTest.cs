// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class MappingQuerySqlServerTest : MappingQueryTestBase, IClassFixture<MappingQuerySqlServerFixture>
    {
        public override void All_customers()
        {
            base.All_customers();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[CompanyName]
FROM [dbo].[Customers] AS [c]",
                Sql);
        }

        public override void All_employees()
        {
            base.All_employees();

            Assert.Equal(
                @"SELECT [e].[EmployeeID], [e].[City]
FROM [dbo].[Employees] AS [e]",
                Sql);
        }

        public override void All_orders()
        {
            base.All_orders();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[ShipVia]
FROM [dbo].[Orders] AS [o]",
                Sql);
        }

        public override void Project_nullable_enum()
        {
            base.Project_nullable_enum();

            Assert.Equal(
                @"SELECT [o].[ShipVia]
FROM [dbo].[Orders] AS [o]",
                Sql);
        }

        private readonly MappingQuerySqlServerFixture _fixture;

        public MappingQuerySqlServerTest(MappingQuerySqlServerFixture fixture)
        {
            _fixture = fixture;
        }

        protected override DbContext CreateContext() => _fixture.CreateContext();

        private const string FileLineEnding = @"
";

        private static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);
    }
}
