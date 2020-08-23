// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindQueryFiltersQuerySqlServerTest : NorthwindQueryFiltersQueryTestBase<
        NorthwindQuerySqlServerFixture<NorthwindQueryFiltersCustomizer>>
    {
        public NorthwindQueryFiltersQuerySqlServerTest(
            NorthwindQuerySqlServerFixture<NorthwindQueryFiltersCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
            //fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Count_query()
        {
            base.Count_query();

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)

SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE (@__ef_filter__TenantPrefix_0 = N'') OR ([c].[CompanyName] IS NOT NULL AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0))");
        }

        public override void Materialized_query()
        {
            base.Materialized_query();

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (@__ef_filter__TenantPrefix_0 = N'') OR ([c].[CompanyName] IS NOT NULL AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0))");
        }

        public override void Find()
        {
            base.Find();

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)
@__p_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ((@__ef_filter__TenantPrefix_0 = N'') OR ([c].[CompanyName] IS NOT NULL AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0))) AND ([c].[CustomerID] = @__p_0)");
        }

        public override void Materialized_query_parameter()
        {
            base.Materialized_query_parameter();

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='F' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (@__ef_filter__TenantPrefix_0 = N'') OR ([c].[CompanyName] IS NOT NULL AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0))");
        }

        public override void Materialized_query_parameter_new_context()
        {
            base.Materialized_query_parameter_new_context();

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (@__ef_filter__TenantPrefix_0 = N'') OR ([c].[CompanyName] IS NOT NULL AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0))",
                //
                @"@__ef_filter__TenantPrefix_0='T' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (@__ef_filter__TenantPrefix_0 = N'') OR ([c].[CompanyName] IS NOT NULL AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0))");
        }

        public override void Projection_query_parameter()
        {
            base.Projection_query_parameter();

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='F' (Size = 4000)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE (@__ef_filter__TenantPrefix_0 = N'') OR ([c].[CompanyName] IS NOT NULL AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0))");
        }

        public override void Projection_query()
        {
            base.Projection_query();

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE (@__ef_filter__TenantPrefix_0 = N'') OR ([c].[CompanyName] IS NOT NULL AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0))");
        }

        public override void Include_query()
        {
            base.Include_query();

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate], [t0].[CustomerID0]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [t].[CustomerID] AS [CustomerID0]
    FROM [Orders] AS [o]
    LEFT JOIN (
        SELECT [c0].[CustomerID], [c0].[CompanyName]
        FROM [Customers] AS [c0]
        WHERE (@__ef_filter__TenantPrefix_0 = N'') OR ([c0].[CompanyName] IS NOT NULL AND (LEFT([c0].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0))
    ) AS [t] ON [o].[CustomerID] = [t].[CustomerID]
    WHERE [t].[CustomerID] IS NOT NULL AND [t].[CompanyName] IS NOT NULL
) AS [t0] ON [c].[CustomerID] = [t0].[CustomerID]
WHERE (@__ef_filter__TenantPrefix_0 = N'') OR ([c].[CompanyName] IS NOT NULL AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0))
ORDER BY [c].[CustomerID], [t0].[OrderID], [t0].[CustomerID0]");
        }

        public override void Include_query_opt_out()
        {
            base.Include_query_opt_out();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID], [o].[OrderID]");
        }

        public override void Included_many_to_one_query()
        {
            base.Included_many_to_one_query();

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM [Orders] AS [o]
LEFT JOIN (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE (@__ef_filter__TenantPrefix_0 = N'') OR ([c].[CompanyName] IS NOT NULL AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0))
) AS [t] ON [o].[CustomerID] = [t].[CustomerID]
WHERE [t].[CustomerID] IS NOT NULL AND [t].[CompanyName] IS NOT NULL");
        }

        public override void Project_reference_that_itself_has_query_filter_with_another_reference()
        {
            base.Project_reference_that_itself_has_query_filter_with_another_reference();

            AssertSql(
                @"@__ef_filter__TenantPrefix_1='B' (Size = 4000)
@__ef_filter___quantity_0='50'

SELECT [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate]
FROM [Order Details] AS [o]
INNER JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    LEFT JOIN (
        SELECT [c].[CustomerID], [c].[CompanyName]
        FROM [Customers] AS [c]
        WHERE (@__ef_filter__TenantPrefix_1 = N'') OR ([c].[CompanyName] IS NOT NULL AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_1)) = @__ef_filter__TenantPrefix_1))
    ) AS [t] ON [o0].[CustomerID] = [t].[CustomerID]
    WHERE [t].[CustomerID] IS NOT NULL AND [t].[CompanyName] IS NOT NULL
) AS [t0] ON [o].[OrderID] = [t0].[OrderID]
WHERE [o].[Quantity] > @__ef_filter___quantity_0");
        }

        public override void Navs_query()
        {
            base.Navs_query();

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)
@__ef_filter___quantity_1='50'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    LEFT JOIN (
        SELECT [c0].[CustomerID], [c0].[CompanyName]
        FROM [Customers] AS [c0]
        WHERE (@__ef_filter__TenantPrefix_0 = N'') OR ([c0].[CompanyName] IS NOT NULL AND (LEFT([c0].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0))
    ) AS [t] ON [o].[CustomerID] = [t].[CustomerID]
    WHERE [t].[CustomerID] IS NOT NULL AND [t].[CompanyName] IS NOT NULL
) AS [t0] ON [c].[CustomerID] = [t0].[CustomerID]
INNER JOIN (
    SELECT [o0].[OrderID], [o0].[Discount]
    FROM [Order Details] AS [o0]
    WHERE [o0].[Quantity] > @__ef_filter___quantity_1
) AS [t1] ON [t0].[OrderID] = [t1].[OrderID]
WHERE ((@__ef_filter__TenantPrefix_0 = N'') OR ([c].[CompanyName] IS NOT NULL AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0))) AND ([t1].[Discount] < CAST(10 AS real))");
        }

        [ConditionalFact]
        public void FromSql_is_composed()
        {
            using (var context = CreateContext())
            {
                var results = context.Customers.FromSqlRaw("select * from Customers").ToList();

                Assert.Equal(7, results.Count);
            }

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    select * from Customers
) AS [c]
WHERE (@__ef_filter__TenantPrefix_0 = N'') OR ([c].[CompanyName] IS NOT NULL AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0))");
        }

        [ConditionalFact]
        public void FromSql_is_composed_when_filter_has_navigation()
        {
            using (var context = CreateContext())
            {
                var results = context.Orders.FromSqlRaw("select * from Orders").ToList();

                Assert.Equal(80, results.Count);
            }

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    select * from Orders
) AS [o]
LEFT JOIN (
    SELECT [c].[CustomerID], [c].[CompanyName]
    FROM [Customers] AS [c]
    WHERE (@__ef_filter__TenantPrefix_0 = N'') OR ([c].[CompanyName] IS NOT NULL AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0))
) AS [t] ON [o].[CustomerID] = [t].[CustomerID]
WHERE [t].[CustomerID] IS NOT NULL AND [t].[CompanyName] IS NOT NULL");
        }

        public override void Compiled_query()
        {
            base.Compiled_query();

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)
@__customerID='BERGS' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ((@__ef_filter__TenantPrefix_0 = N'') OR ([c].[CompanyName] IS NOT NULL AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0))) AND ([c].[CustomerID] = @__customerID)",
                //
                @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)
@__customerID='BLAUS' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ((@__ef_filter__TenantPrefix_0 = N'') OR ([c].[CompanyName] IS NOT NULL AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0))) AND ([c].[CustomerID] = @__customerID)");
        }

        public override void Entity_Equality()
        {
            base.Entity_Equality();

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN (
    SELECT [c].[CustomerID], [c].[CompanyName]
    FROM [Customers] AS [c]
    WHERE (@__ef_filter__TenantPrefix_0 = N'') OR ([c].[CompanyName] IS NOT NULL AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0))
) AS [t] ON [o].[CustomerID] = [t].[CustomerID]
WHERE [t].[CustomerID] IS NOT NULL AND [t].[CompanyName] IS NOT NULL");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
