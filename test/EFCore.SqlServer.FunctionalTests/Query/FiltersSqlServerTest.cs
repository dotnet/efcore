// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class FiltersSqlServerTest : FiltersTestBase<NorthwindQuerySqlServerFixture<NorthwindFiltersCustomizer>>
    {
        public FiltersSqlServerTest(NorthwindQuerySqlServerFixture<NorthwindFiltersCustomizer> fixture, ITestOutputHelper testOutputHelper)
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
WHERE ([c].[CompanyName] LIKE @__ef_filter__TenantPrefix_0 + N'%' AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0)) OR (@__ef_filter__TenantPrefix_0 = N'')");
        }

        public override void Client_eval()
        {
            base.Client_eval();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]");
        }

        public override void Materialized_query()
        {
            base.Materialized_query();

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[CompanyName] LIKE @__ef_filter__TenantPrefix_0 + N'%' AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0)) OR (@__ef_filter__TenantPrefix_0 = N'')");
        }

        public override void Find()
        {
            base.Find();

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)
@__p_0='ALFKI' (Size = 5)

SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[CompanyName] LIKE @__ef_filter__TenantPrefix_0 + N'%' AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0)) OR (@__ef_filter__TenantPrefix_0 = N'')) AND ([c].[CustomerID] = @__p_0)");
        }

        public override void Materialized_query_parameter()
        {
            base.Materialized_query_parameter();

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='F' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[CompanyName] LIKE @__ef_filter__TenantPrefix_0 + N'%' AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0)) OR (@__ef_filter__TenantPrefix_0 = N'')");
        }

        public override void Materialized_query_parameter_new_context()
        {
            base.Materialized_query_parameter_new_context();

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[CompanyName] LIKE @__ef_filter__TenantPrefix_0 + N'%' AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0)) OR (@__ef_filter__TenantPrefix_0 = N'')",
                //
                @"@__ef_filter__TenantPrefix_0='T' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[CompanyName] LIKE @__ef_filter__TenantPrefix_0 + N'%' AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0)) OR (@__ef_filter__TenantPrefix_0 = N'')");
        }

        public override void Projection_query_parameter()
        {
            base.Projection_query_parameter();

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='F' (Size = 4000)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE ([c].[CompanyName] LIKE @__ef_filter__TenantPrefix_0 + N'%' AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0)) OR (@__ef_filter__TenantPrefix_0 = N'')");
        }

        public override void Projection_query()
        {
            base.Projection_query();

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE ([c].[CompanyName] LIKE @__ef_filter__TenantPrefix_0 + N'%' AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0)) OR (@__ef_filter__TenantPrefix_0 = N'')");
        }

        public override void Include_query()
        {
            base.Include_query();

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[CompanyName] LIKE @__ef_filter__TenantPrefix_0 + N'%' AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0)) OR (@__ef_filter__TenantPrefix_0 = N'')
ORDER BY [c].[CustomerID]",
                //
                @"@__ef_filter__TenantPrefix_1='B' (Size = 4000)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
    WHERE ([c0].[CompanyName] LIKE @__ef_filter__TenantPrefix_1 + N'%' AND (LEFT([c0].[CompanyName], LEN(@__ef_filter__TenantPrefix_1)) = @__ef_filter__TenantPrefix_1)) OR (@__ef_filter__TenantPrefix_1 = N'')
) AS [t] ON [o].[CustomerID] = [t].[CustomerID]
WHERE [o.Customer].[CompanyName] IS NOT NULL
ORDER BY [t].[CustomerID]");
        }

        public override void Include_query_opt_out()
        {
            base.Include_query_opt_out();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]");
        }

        public override void Included_many_to_one_query()
        {
            base.Included_many_to_one_query();

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
LEFT JOIN (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE ([c].[CompanyName] LIKE @__ef_filter__TenantPrefix_0 + N'%' AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0)) OR (@__ef_filter__TenantPrefix_0 = N'')
) AS [t] ON [o].[CustomerID] = [t].[CustomerID]
WHERE [o.Customer].[CompanyName] IS NOT NULL");
        }

        public override void Included_one_to_many_query_with_client_eval()
        {
            base.Included_one_to_many_query_with_client_eval();

            AssertContainsSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
ORDER BY [p].[ProductID]",
                //
                @"SELECT [p1].[ProductID], [p1].[Discontinued], [p1].[ProductName], [p1].[SupplierID], [p1].[UnitPrice], [p1].[UnitsInStock]
FROM [Products] AS [p1]",
                //
                @"@__ef_filter___quantity_0='50' (DbType = Int16)

SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
WHERE [od].[Quantity] > @__ef_filter___quantity_0");
        }

        public override void Navs_query()
        {
            base.Navs_query();

            AssertSql(
                @"@__ef_filter___quantity_1='50' (DbType = Int16)
@__ef_filter__TenantPrefix_0='B' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o].*
    FROM [Orders] AS [o]
    LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
    WHERE [o.Customer].[CompanyName] IS NOT NULL
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]
INNER JOIN (
    SELECT [od].*
    FROM [Order Details] AS [od]
    WHERE [od].[Quantity] > @__ef_filter___quantity_1
) AS [t0] ON [t].[OrderID] = [t0].[OrderID]
WHERE (([c].[CompanyName] LIKE @__ef_filter__TenantPrefix_0 + N'%' AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0)) OR (@__ef_filter__TenantPrefix_0 = N'')) AND ([t0].[Discount] < CAST(10 AS real))");
        }

        [ConditionalFact]
        public void FromSql_is_composed()
        {
            using (var context = CreateContext())
            {
                var results = context.Customers.FromSql("select * from Customers").ToList();

                Assert.Equal(7, results.Count);
            }

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    select * from Customers
) AS [c]
WHERE ([c].[CompanyName] LIKE @__ef_filter__TenantPrefix_0 + N'%' AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0)) OR (@__ef_filter__TenantPrefix_0 = N'')");
        }

        public override void Compiled_query()
        {
            base.Compiled_query();

            AssertSql(
                @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)
@__customerID='BERGS' (Size = 5)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[CompanyName] LIKE @__ef_filter__TenantPrefix_0 + N'%' AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0)) OR (@__ef_filter__TenantPrefix_0 = N'')) AND ([c].[CustomerID] = @__customerID)",
                //
                @"@__ef_filter__TenantPrefix_0='B' (Size = 4000)
@__customerID='BLAUS' (Size = 5)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (([c].[CompanyName] LIKE @__ef_filter__TenantPrefix_0 + N'%' AND (LEFT([c].[CompanyName], LEN(@__ef_filter__TenantPrefix_0)) = @__ef_filter__TenantPrefix_0)) OR (@__ef_filter__TenantPrefix_0 = N'')) AND ([c].[CustomerID] = @__customerID)");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        private void AssertContainsSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, assertOrder: false);
    }
}
