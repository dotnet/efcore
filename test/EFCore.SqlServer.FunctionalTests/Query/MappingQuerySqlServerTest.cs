// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class MappingQuerySqlServerTest : MappingQueryTestBase<MappingQuerySqlServerTest.MappingQuerySqlServerFixture>
{
    public override void All_customers()
    {
        base.All_customers();

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[CompanyName]
FROM [dbo].[Customers] AS [c]
""");
    }

    public override void All_employees()
    {
        base.All_employees();

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City]
FROM [dbo].[Employees] AS [e]
""");
    }

    public override void All_orders()
    {
        base.All_orders();

        AssertSql(
            """
SELECT [o].[OrderID], [o].[ShipVia]
FROM [dbo].[Orders] AS [o]
""");
    }

    public override void Project_nullable_enum()
    {
        base.Project_nullable_enum();

        AssertSql(
            """
SELECT [o].[ShipVia]
FROM [dbo].[Orders] AS [o]
""");
    }

    public MappingQuerySqlServerTest(MappingQuerySqlServerFixture fixture)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class MappingQuerySqlServerFixture : MappingQueryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerNorthwindTestStoreFactory.Instance;

        protected override string DatabaseSchema { get; } = "dbo";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<MappedCustomer>(
                e =>
                {
                    e.Property(c => c.CompanyName2).Metadata.SetColumnName("CompanyName");
                    e.Metadata.SetTableName("Customers");
                    e.Metadata.SetSchema("dbo");
                });

            modelBuilder.Entity<MappedEmployee>()
                .Property(c => c.EmployeeID)
                .HasColumnType("int");
        }
    }
}
