// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;
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
                @"SELECT ""c"".""CustomerID"", ""c"".""CompanyName""" + EOL + @"FROM ""Customers"" AS ""c""",
                Sql);
        }

        public override void All_employees()
        {
            base.All_employees();

            Assert.Contains(
                @"SELECT ""e"".""EmployeeID"", ""e"".""City""" + EOL + @"FROM ""Employees"" AS ""e""",
                Sql);
        }

        public override void All_orders()
        {
            base.All_orders();

            Assert.Contains(
                @"SELECT ""o"".""OrderID"", ""o"".""ShipVia""" + EOL + @"FROM ""Orders"" AS ""o""",
                Sql);
        }

        public override void Project_nullable_enum()
        {
            base.Project_nullable_enum();

            Assert.Contains(
                @"SELECT ""o"".""ShipVia""" + EOL + @"FROM ""Orders"" AS ""o""",
                Sql);
        }

        private static readonly string EOL = Environment.NewLine;

        private string Sql => Fixture.TestSqlLoggerFactory.Sql;

        public class MappingQuerySqliteFixture : MappingQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => SqliteNorthwindTestStoreFactory.Instance;

            protected override string DatabaseSchema { get; } = null;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.Entity<MappedCustomer>(
                    e =>
                    {
                        e.Property(c => c.CompanyName2).Metadata.SetColumnName("CompanyName");
                        e.Metadata.SetTableName("Customers");
                    });
            }
        }
    }
}
