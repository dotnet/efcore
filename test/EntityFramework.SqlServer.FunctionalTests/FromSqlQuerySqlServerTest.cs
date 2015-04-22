// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class FromSqlQuerySqlServerTest : FromSqlQueryTestBase<NorthwindQuerySqlServerFixture>
    {
        public override void From_sql_queryable_simple()
        {
            base.From_sql_queryable_simple();

            Assert.Equal(
                @"SELECT * FROM Customers",
                Sql);
        }

        public override void From_sql_queryable_filter()
        {
            base.From_sql_queryable_filter();

            Assert.Equal(
                @"SELECT * FROM Customers WHERE Customers.ContactName LIKE '%z%'",
                Sql);
        }

        public override void From_sql_queryable_composed()
        {
            base.From_sql_queryable_composed();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT * FROM Customers
) AS [c]
WHERE [c].[ContactName] LIKE ('%' + 'z' + '%')",
                Sql);
        }

        public override void From_sql_queryable_multiple_line_query()
        {
            base.From_sql_queryable_multiple_line_query();

            Assert.Equal(
                @"SELECT *
FROM Customers
WHERE Customers.City = 'London'",
                Sql);
        }

        public override void From_sql_queryable_composed_multiple_line_query()
        {
            base.From_sql_queryable_composed_multiple_line_query();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT *
    FROM Customers
) AS [c]
WHERE [c].[City] = 'London'",
                Sql);
        }

        public override void From_sql_queryable_with_columns_reordered()
        {
            base.From_sql_queryable_with_columns_reordered();

            Assert.Equal(@"SELECT
    Address, City, CompanyName, ContactName, ContactTitle, Country, CustomerID, Fax, Phone, PostalCode, Region
FROM
    Customers
WHERE
    CustomerID = 'ALFKI'

SELECT
    Region, PostalCode, Phone, Fax, CustomerID, Country, ContactTitle, ContactName, CompanyName, City, Address
FROM
    Customers
WHERE
    CustomerID = 'ALFKI'",
                Sql);
}

        public override void From_sql_queryable_with_parameters()
        {
            base.From_sql_queryable_with_parameters();

            Assert.Equal(
                @"p0: London
p1: Sales Representative

SELECT * FROM Customers WHERE City = @p0 AND ContactTitle = @p1",
                Sql);
        }

        public override void From_sql_queryable_with_parameters_and_closure()
        {
            base.From_sql_queryable_with_parameters_and_closure();

            Assert.Equal(
                @"p0: London
__contactTitle_0: Sales Representative

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT * FROM Customers WHERE City = @p0
) AS [c]
WHERE [c].[ContactTitle] = @__contactTitle_0",
                Sql);
        }

        public override void From_sql_queryable_simple_cache_key_includes_query_string()
        {
            base.From_sql_queryable_simple_cache_key_includes_query_string();

            Assert.Equal(
                @"SELECT * FROM Customers WHERE Customers.City = 'London'

SELECT * FROM Customers WHERE Customers.City = 'Seattle'",
                Sql);
        }

        public override void From_sql_queryable_with_parameters_cache_key_includes_parameters()
        {
            base.From_sql_queryable_with_parameters_cache_key_includes_parameters();

            Assert.Equal(
                @"p0: London
p1: Sales Representative

SELECT * FROM Customers WHERE City = @p0 AND ContactTitle = @p1

p0: Madrid
p1: Accounting Manager

SELECT * FROM Customers WHERE City = @p0 AND ContactTitle = @p1",
                Sql);
        }

        public override void From_sql_queryable_simple_as_no_tracking_not_composed()
        {
            base.From_sql_queryable_simple_as_no_tracking_not_composed();

            Assert.Equal(
                @"SELECT * FROM Customers",
                Sql);
        }

        public override void From_sql_queryable_simple_include()
        {
            base.From_sql_queryable_simple_include();

            Assert.Equal(
                @"SELECT * FROM Customers

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [c].[CustomerID]
    FROM (
        SELECT * FROM Customers
    ) AS [c]
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void From_sql_queryable_simple_composed_include()
        {
            base.From_sql_queryable_simple_composed_include();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT * FROM Customers
) AS [c]
WHERE [c].[City] = 'London'
ORDER BY [c].[CustomerID]

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN (
    SELECT DISTINCT [c].[CustomerID]
    FROM (
        SELECT * FROM Customers
    ) AS [c]
    WHERE [c].[City] = 'London'
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[CustomerID]",
                Sql);
        }

        public override void From_sql_annotations_do_not_affect_successive_calls()
        {
            base.From_sql_annotations_do_not_affect_successive_calls();

            Assert.Equal(
                @"SELECT * FROM Customers WHERE Customers.ContactName LIKE '%z%'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public override void From_sql_composed_with_nullable_predicate()
        {
            base.From_sql_composed_with_nullable_predicate();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT * FROM Customers
) AS [c]
WHERE ([c].[ContactName] = [c].[CompanyName] OR ([c].[ContactName] IS NULL AND [c].[CompanyName] IS NULL))",
                Sql);
        }

        public override void From_sql_composed_with_relational_null_comparison()
        {
            base.From_sql_composed_with_relational_null_comparison();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT * FROM Customers
) AS [c]
WHERE [c].[ContactName] = [c].[CompanyName]",
                Sql);
        }

        public FromSqlQuerySqlServerTest(NorthwindQuerySqlServerFixture fixture)
            : base(fixture)
        {
        }

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
