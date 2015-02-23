// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
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
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT * FROM Customers
) AS [c]",
                Sql);
        }

        public override void From_sql_queryable_filter()
        {
            base.From_sql_queryable_filter();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT * FROM Customers WHERE Customers.ContactName LIKE '%z%'
) AS [c]",
                Sql);
        }
        public override void From_sql_queryable_cached_by_query()
        {
            base.From_sql_queryable_cached_by_query();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT * FROM Customers WHERE Customers.City = 'London'
) AS [c]

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT * FROM Customers WHERE Customers.City = 'Seattle'
) AS [c]",
                Sql);
        }

        public override void From_sql_queryable_where_simple_closure_via_query_cache()
        {
            base.From_sql_queryable_where_simple_closure_via_query_cache();

            Assert.Equal(
                @"__title_0: Sales Associate

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT * FROM Customers WHERE Customers.ContactName LIKE '%o%'
) AS [c]
WHERE [c].[ContactTitle] = @__title_0

__title_0: Sales Manager

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT * FROM Customers WHERE Customers.ContactName LIKE '%o%'
) AS [c]
WHERE [c].[ContactTitle] = @__title_0",
                Sql);
        }

        public override void From_sql_queryable_with_multiple_line_query()
        {
            base.From_sql_queryable_with_multiple_line_query();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT *
    FROM Customers
    WHERE Customers.City = 'London'
) AS [c]",
                Sql);
        }

        public override void From_sql_queryable_with_params_parameters()
        {
            base.From_sql_queryable_with_params_parameters();

            Assert.Equal(
                @"p0: London
p1: Sales Representative

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT * FROM Customers WHERE City = @p0 AND ContactTitle = @p1
) AS [c]",
                Sql);
        }

        public override void From_sql_queryable_with_params_parameters_does_not_collide_with_cache()
        {
            base.From_sql_queryable_with_params_parameters_does_not_collide_with_cache();

            Assert.Equal(
                @"p0: London
p1: Sales Representative

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT * FROM Customers WHERE City = @p0 AND ContactTitle = @p1
) AS [c]

p0: Madrid
p1: Accounting Manager

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT * FROM Customers WHERE City = @p0 AND ContactTitle = @p1
) AS [c]",
                Sql);
        }


        public override void From_sql_annotations_do_not_modify_successive_calls()
        {
            base.From_sql_annotations_do_not_modify_successive_calls();

            Assert.Equal(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT * FROM Customers WHERE Customers.ContactName LIKE '%z%'
) AS [c]

SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[CustomerID], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        public FromSqlQuerySqlServerTest(NorthwindQuerySqlServerFixture fixture)
            : base(fixture)
        {
        }

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
