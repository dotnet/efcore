// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class MappingQuerySqliteTest : MappingQueryTestBase, IClassFixture<MappingQuerySqliteFixture>
    {
        public override void All_customers()
        {
            base.All_customers();

            Assert.Equal(
                @"SELECT ""c"".""CustomerID"", ""c"".""CompanyName""
FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void All_employees()
        {
            base.All_employees();

            Assert.Equal(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City""
FROM ""Employees"" AS ""e""",
                Sql);
        }

        public override void All_orders()
        {
            base.All_orders();

            Assert.Equal(
                @"SELECT ""o"".""OrderID"", ""o"".""ShipVia""
FROM ""Orders"" AS ""o""",
                Sql);
        }

        public override void Project_nullable_enum()
        {
            base.Project_nullable_enum();

            Assert.Equal(
                @"SELECT ""o"".""ShipVia""
FROM ""Orders"" AS ""o""",
                Sql);
        }

        private readonly MappingQuerySqliteFixture _fixture;

        public MappingQuerySqliteTest(MappingQuerySqliteFixture fixture)
        {
            _fixture = fixture;
        }

        protected override DbContext CreateContext()
        {
            return _fixture.CreateContext();
        }

        private static string Sql
        {
            get { return TestSqlLoggerFactory.Sql; }
        }
    }
}
