// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class MappingQuerySqliteTest : MappingQueryTestBase, IClassFixture<MappingQuerySqliteFixture>
    {
        public override void All_customers()
        {
            base.All_customers();

            Assert.Contains(
                @"SELECT ""c"".""CustomerID"", ""c"".""CompanyName""
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void All_employees()
        {
            base.All_employees();

            Assert.Contains(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City""
FROM ""Employees"" AS ""e""",
                Sql);
        }

        public override void All_orders()
        {
            base.All_orders();

            Assert.Contains(
                @"SELECT ""o"".""OrderID"", ""o"".""ShipVia""
FROM ""Orders"" AS ""o""",
                Sql);
        }

        public override void Project_nullable_enum()
        {
            base.Project_nullable_enum();

            Assert.Contains(
                @"SELECT ""o"".""ShipVia""
FROM ""Orders"" AS ""o""",
                Sql);
        }

        private readonly MappingQuerySqliteFixture _fixture;

        public MappingQuerySqliteTest(MappingQuerySqliteFixture fixture)
        {
            _fixture = fixture;
        }

        protected override DbContext CreateContext() => _fixture.CreateContext();

        private const string FileLineEnding = @"
";

        private string Sql => _fixture.TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);
    }
}
