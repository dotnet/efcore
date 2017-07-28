// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Utilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class MappingQuerySqliteTest : MappingQueryTestBase<MappingQuerySqliteTest.MappingQuerySqliteFixture>
    {
        public MappingQuerySqliteTest(MappingQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

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

        private const string FileLineEnding = @"
";

        private string Sql => Fixture.TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);

        public class MappingQuerySqliteFixture : MappingQueryFixtureBase
        {
            protected override ITestStoreFactory<TestStore> TestStoreFactory => SqliteNorthwindTestStoreFactory.Instance;

            protected override string DatabaseSchema { get; } = null;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);
                
                modelBuilder.Entity<MappedCustomer>(e =>
                    {
                        e.Property(c => c.CompanyName2).Metadata.Relational().ColumnName = "CompanyName";
                        e.Metadata.Relational().TableName = "Customers";
                    });
            }
        }
    }
}
