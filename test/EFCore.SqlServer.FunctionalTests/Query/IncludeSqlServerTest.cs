// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IncludeSqlServerTest : IncludeTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        private bool SupportsOffset => TestEnvironment.GetFlag(nameof(SqlServerCondition.SupportsOffset)) ?? true;

        // ReSharper disable once UnusedParameter.Local
        public IncludeSqlServerTest(NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Include_list(bool useString)
        {
            base.Include_list(useString);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock], [t].[OrderID], [t].[ProductID], [t].[Discount], [t].[Quantity], [t].[UnitPrice], [t].[OrderID0], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM [Products] AS [p]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID] AS [OrderID0], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Order Details] AS [o]
    INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
) AS [t] ON [p].[ProductID] = [t].[ProductID]
ORDER BY [p].[ProductID], [t].[OrderID], [t].[ProductID], [t].[OrderID0]");
        }

        public override void Include_reference(bool useString)
        {
            base.Include_reference(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]");
        }

        public override void Include_when_result_operator(bool useString)
        {
            base.Include_when_result_operator(useString);

            AssertSql(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c]) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END");
        }

        public override void Include_collection(bool useString)
        {
            base.Include_collection(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID], [o].[OrderID]");
        }

        public override void Include_collection_with_last(bool useString)
        {
            base.Include_collection_with_last(useString);

            AssertSql(
                @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CompanyName] DESC
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[CompanyName] DESC, [t].[CustomerID], [o].[OrderID]");
        }

        public override void Include_collection_skip_no_order_by(bool useString)
        {
            base.Include_collection_skip_no_order_by(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='10'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY (SELECT 1)
    OFFSET @__p_0 ROWS
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[CustomerID], [o].[OrderID]");
            }
        }

        public override void Include_collection_take_no_order_by(bool useString)
        {
            base.Include_collection_take_no_order_by(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='10'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[CustomerID], [o].[OrderID]");
            }
        }

        public override void Include_collection_skip_take_no_order_by(bool useString)
        {
            base.Include_collection_skip_take_no_order_by(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='10'
@__p_1='5'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY (SELECT 1)
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[CustomerID], [o].[OrderID]");
            }
        }

        public override void Include_reference_and_collection(bool useString)
        {
            base.Include_reference_and_collection(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
LEFT JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
ORDER BY [o].[OrderID], [o0].[OrderID], [o0].[ProductID]");
        }

        public override void Include_references_multi_level(bool useString)
        {
            base.Include_references_multi_level(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]");
        }

        public override void Include_multiple_references_multi_level(bool useString)
        {
            base.Include_multiple_references_multi_level(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]");
        }

        public override void Include_multiple_references_multi_level_reverse(bool useString)
        {
            base.Include_multiple_references_multi_level_reverse(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Order Details] AS [o]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]");
        }

        public override void Include_references_and_collection_multi_level(bool useString)
        {
            base.Include_references_and_collection_multi_level(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
LEFT JOIN [Orders] AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
ORDER BY [o].[OrderID], [o].[ProductID], [o0].[OrderID], [o1].[OrderID]");
        }

        public override void Include_multi_level_reference_and_collection_predicate(bool useString)
        {
            base.Include_multi_level_reference_and_collection_predicate(useString);

            AssertSql(
                @"SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [t].[CustomerID0], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM (
    SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID] AS [CustomerID0], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Orders] AS [o]
    LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
    WHERE [o].[OrderID] = 10248
) AS [t]
LEFT JOIN [Orders] AS [o0] ON [t].[CustomerID0] = [o0].[CustomerID]
ORDER BY [t].[OrderID], [o0].[OrderID]");
        }

        public override void Include_multi_level_collection_and_then_include_reference_predicate(bool useString)
        {
            base.Include_multi_level_collection_and_then_include_reference_predicate(useString);

            AssertSql(
                @"SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [t0].[OrderID], [t0].[ProductID], [t0].[Discount], [t0].[Quantity], [t0].[UnitPrice], [t0].[ProductID0], [t0].[Discontinued], [t0].[ProductName], [t0].[SupplierID], [t0].[UnitPrice0], [t0].[UnitsInStock]
FROM (
    SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] = 10248
) AS [t]
LEFT JOIN (
    SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [p].[ProductID] AS [ProductID0], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice] AS [UnitPrice0], [p].[UnitsInStock]
    FROM [Order Details] AS [o0]
    INNER JOIN [Products] AS [p] ON [o0].[ProductID] = [p].[ProductID]
) AS [t0] ON [t].[OrderID] = [t0].[OrderID]
ORDER BY [t].[OrderID], [t0].[OrderID], [t0].[ProductID], [t0].[ProductID0]");
        }

        public override void Include_collection_alias_generation(bool useString)
        {
            base.Include_collection_alias_generation(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Orders] AS [o]
LEFT JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
ORDER BY [o].[OrderID], [o0].[OrderID], [o0].[ProductID]");
        }

        public override void Include_collection_order_by_collection_column(bool useString)
        {
            base.Include_collection_order_by_collection_column(useString);

            AssertSql(
                @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM (
    SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], (
        SELECT TOP(1) [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderDate] DESC) AS [c]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'W%'
    ORDER BY (
        SELECT TOP(1) [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderDate] DESC) DESC
) AS [t]
LEFT JOIN [Orders] AS [o0] ON [t].[CustomerID] = [o0].[CustomerID]
ORDER BY [t].[c] DESC, [t].[CustomerID], [o0].[OrderID]");
        }

        public override void Include_collection_order_by_key(bool useString)
        {
            base.Include_collection_order_by_key(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID], [o].[OrderID]");
        }

        public override void Include_collection_order_by_non_key(bool useString)
        {
            base.Include_collection_order_by_non_key(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[City], [c].[CustomerID], [o].[OrderID]");
        }

        public override void Include_collection_order_by_non_key_with_take(bool useString)
        {
            base.Include_collection_order_by_non_key_with_take(useString);

            AssertSql(
                @"@__p_0='10'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactTitle]
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[ContactTitle], [t].[CustomerID], [o].[OrderID]");
        }

        public override void Include_collection_order_by_non_key_with_skip(bool useString)
        {
            base.Include_collection_order_by_non_key_with_skip(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='10'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactTitle]
    OFFSET @__p_0 ROWS
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[ContactTitle], [t].[CustomerID], [o].[OrderID]");
            }
        }

        public override void Include_collection_order_by_non_key_with_first_or_default(bool useString)
        {
            base.Include_collection_order_by_non_key_with_first_or_default(useString);

            AssertSql(
                @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CompanyName] DESC
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[CompanyName] DESC, [t].[CustomerID], [o].[OrderID]");
        }

        public override void Include_collection_order_by_subquery(bool useString)
        {
            base.Include_collection_order_by_subquery(useString);

            AssertSql(
                @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM (
    SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], (
        SELECT TOP(1) [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[EmployeeID]) AS [c]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'ALFKI'
    ORDER BY (
        SELECT TOP(1) [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[EmployeeID])
) AS [t]
LEFT JOIN [Orders] AS [o0] ON [t].[CustomerID] = [o0].[CustomerID]
ORDER BY [t].[c], [t].[CustomerID], [o0].[OrderID]");
        }

        public override void Include_collection_as_no_tracking(bool useString)
        {
            base.Include_collection_as_no_tracking(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID], [o].[OrderID]");
        }

        public override void Include_collection_principal_already_tracked(bool useString)
        {
            base.Include_collection_principal_already_tracked(useString);

            AssertSql(
                @"SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'",
                //
                @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'ALFKI'
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[CustomerID], [o].[OrderID]");
        }

        public override void Include_collection_principal_already_tracked_as_no_tracking(bool useString)
        {
            base.Include_collection_principal_already_tracked_as_no_tracking(useString);

            AssertSql(
                @"SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'",
                //
                @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'ALFKI'
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[CustomerID], [o].[OrderID]");
        }

        public override void Include_collection_with_filter(bool useString)
        {
            base.Include_collection_with_filter(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID], [o].[OrderID]");
        }

        public override void Include_collection_with_filter_reordered(bool useString)
        {
            base.Include_collection_with_filter_reordered(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID], [o].[OrderID]");
        }

        public override void Include_collection_then_include_collection(bool useString)
        {
            base.Include_collection_then_include_collection(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [t].[OrderID0], [t].[ProductID], [t].[Discount], [t].[Quantity], [t].[UnitPrice]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o0].[OrderID] AS [OrderID0], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
    FROM [Orders] AS [o]
    LEFT JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]
ORDER BY [c].[CustomerID], [t].[OrderID], [t].[OrderID0], [t].[ProductID]");
        }

        public override void Include_collection_then_include_collection_then_include_reference(bool useString)
        {
            base.Include_collection_then_include_collection_then_include_reference(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate], [t0].[OrderID0], [t0].[ProductID], [t0].[Discount], [t0].[Quantity], [t0].[UnitPrice], [t0].[ProductID0], [t0].[Discontinued], [t0].[ProductName], [t0].[SupplierID], [t0].[UnitPrice0], [t0].[UnitsInStock]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [t].[OrderID] AS [OrderID0], [t].[ProductID], [t].[Discount], [t].[Quantity], [t].[UnitPrice], [t].[ProductID0], [t].[Discontinued], [t].[ProductName], [t].[SupplierID], [t].[UnitPrice0], [t].[UnitsInStock]
    FROM [Orders] AS [o]
    LEFT JOIN (
        SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [p].[ProductID] AS [ProductID0], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice] AS [UnitPrice0], [p].[UnitsInStock]
        FROM [Order Details] AS [o0]
        INNER JOIN [Products] AS [p] ON [o0].[ProductID] = [p].[ProductID]
    ) AS [t] ON [o].[OrderID] = [t].[OrderID]
) AS [t0] ON [c].[CustomerID] = [t0].[CustomerID]
ORDER BY [c].[CustomerID], [t0].[OrderID], [t0].[OrderID0], [t0].[ProductID], [t0].[ProductID0]");
        }

        public override void Include_collection_when_projection(bool useString)
        {
            base.Include_collection_when_projection(useString);

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]");
        }

        public override void Include_collection_on_join_clause_with_filter(bool useString)
        {
            base.Include_collection_on_join_clause_with_filter(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
LEFT JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID], [o].[OrderID], [o0].[OrderID]");
        }

        public override void Include_collection_on_additional_from_clause_with_filter(bool useString)
        {
            base.Include_collection_on_additional_from_clause_with_filter(useString);

            AssertSql(
                @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [c].[CustomerID], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID], [t].[CustomerID], [o].[OrderID]");
        }

        public override void Include_collection_on_additional_from_clause(bool useString)
        {
            base.Include_collection_on_additional_from_clause(useString);

            AssertSql(
                @"@__p_0='5'

SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region], [t].[CustomerID], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t]
CROSS JOIN [Customers] AS [c0]
LEFT JOIN [Orders] AS [o] ON [c0].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[CustomerID], [c0].[CustomerID], [o].[OrderID]");
        }

        public override void Include_duplicate_collection(bool useString)
        {
            base.Include_duplicate_collection(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='2'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t]
CROSS JOIN (
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [t0]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
LEFT JOIN [Orders] AS [o0] ON [t0].[CustomerID] = [o0].[CustomerID]
ORDER BY [t].[CustomerID], [t0].[CustomerID], [o].[OrderID], [o0].[OrderID]");
            }
        }

        public override void Include_duplicate_collection_result_operator(bool useString)
        {
            base.Include_duplicate_collection_result_operator(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_1='1'
@__p_0='2'

SELECT [t1].[CustomerID], [t1].[Address], [t1].[City], [t1].[CompanyName], [t1].[ContactName], [t1].[ContactTitle], [t1].[Country], [t1].[Fax], [t1].[Phone], [t1].[PostalCode], [t1].[Region], [t1].[CustomerID0], [t1].[Address0], [t1].[City0], [t1].[CompanyName0], [t1].[ContactName0], [t1].[ContactTitle0], [t1].[Country0], [t1].[Fax0], [t1].[Phone0], [t1].[PostalCode0], [t1].[Region0], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM (
    SELECT TOP(@__p_1) [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [t0].[CustomerID] AS [CustomerID0], [t0].[Address] AS [Address0], [t0].[City] AS [City0], [t0].[CompanyName] AS [CompanyName0], [t0].[ContactName] AS [ContactName0], [t0].[ContactTitle] AS [ContactTitle0], [t0].[Country] AS [Country0], [t0].[Fax] AS [Fax0], [t0].[Phone] AS [Phone0], [t0].[PostalCode] AS [PostalCode0], [t0].[Region] AS [Region0]
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[CustomerID]
    ) AS [t]
    CROSS JOIN (
        SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
        FROM [Customers] AS [c0]
        ORDER BY [c0].[CustomerID]
        OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [t0]
    ORDER BY [t].[CustomerID]
) AS [t1]
LEFT JOIN [Orders] AS [o] ON [t1].[CustomerID] = [o].[CustomerID]
LEFT JOIN [Orders] AS [o0] ON [t1].[CustomerID0] = [o0].[CustomerID]
ORDER BY [t1].[CustomerID], [t1].[CustomerID0], [o].[OrderID], [o0].[OrderID]");
            }
        }

        public override void Include_collection_on_join_clause_with_order_by_and_filter(bool useString)
        {
            base.Include_collection_on_join_clause_with_order_by_and_filter(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
LEFT JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[City], [c].[CustomerID], [o].[OrderID], [o0].[OrderID]");
        }

        public override void Include_collection_when_groupby(bool useString)
        {
            base.Include_collection_when_groupby(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[City], [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID], [c0].[City]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[City], [t].[CustomerID]");
        }

        public override void Include_collection_when_groupby_subquery(bool useString)
        {
            base.Include_collection_when_groupby_subquery(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
    ORDER BY [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID], [c.Orders].[OrderID]",
                //
                @"SELECT [c.Orders.OrderDetails].[OrderID], [c.Orders.OrderDetails].[ProductID], [c.Orders.OrderDetails].[Discount], [c.Orders.OrderDetails].[Quantity], [c.Orders.OrderDetails].[UnitPrice], [o.Product].[ProductID], [o.Product].[Discontinued], [o.Product].[ProductName], [o.Product].[SupplierID], [o.Product].[UnitPrice], [o.Product].[UnitsInStock]
FROM [Order Details] AS [c.Orders.OrderDetails]
INNER JOIN [Products] AS [o.Product] ON [c.Orders.OrderDetails].[ProductID] = [o.Product].[ProductID]
INNER JOIN (
    SELECT DISTINCT [c.Orders0].[OrderID], [t0].[CustomerID]
    FROM [Orders] AS [c.Orders0]
    INNER JOIN (
        SELECT TOP(1) [c1].[CustomerID]
        FROM [Customers] AS [c1]
        WHERE [c1].[CustomerID] = N'ALFKI'
        ORDER BY [c1].[CustomerID]
    ) AS [t0] ON [c.Orders0].[CustomerID] = [t0].[CustomerID]
) AS [t1] ON [c.Orders.OrderDetails].[OrderID] = [t1].[OrderID]
ORDER BY [t1].[CustomerID], [t1].[OrderID]");
        }

        public override void Include_collection_on_additional_from_clause2(bool useString)
        {
            base.Include_collection_on_additional_from_clause2(useString);

            AssertSql(
                @"@__p_0='5'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t]
CROSS JOIN [Customers] AS [c0]
ORDER BY [t].[CustomerID]");
        }

        public override void Include_where_skip_take_projection(bool useString)
        {
            base.Include_where_skip_take_projection(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='1'
@__p_1='2'

SELECT [o0].[CustomerID]
FROM (
    SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
    FROM [Order Details] AS [o]
    WHERE [o].[Quantity] = CAST(10 AS smallint)
    ORDER BY [o].[OrderID], [o].[ProductID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
INNER JOIN [Orders] AS [o0] ON [t].[OrderID] = [o0].[OrderID]
ORDER BY [t].[OrderID], [t].[ProductID]");
            }
        }

        public override void Include_duplicate_collection_result_operator2(bool useString)
        {
            base.Include_duplicate_collection_result_operator2(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_1='1'
@__p_0='2'

SELECT [t1].[CustomerID], [t1].[Address], [t1].[City], [t1].[CompanyName], [t1].[ContactName], [t1].[ContactTitle], [t1].[Country], [t1].[Fax], [t1].[Phone], [t1].[PostalCode], [t1].[Region], [t1].[CustomerID0], [t1].[Address0], [t1].[City0], [t1].[CompanyName0], [t1].[ContactName0], [t1].[ContactTitle0], [t1].[Country0], [t1].[Fax0], [t1].[Phone0], [t1].[PostalCode0], [t1].[Region0], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT TOP(@__p_1) [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [t0].[CustomerID] AS [CustomerID0], [t0].[Address] AS [Address0], [t0].[City] AS [City0], [t0].[CompanyName] AS [CompanyName0], [t0].[ContactName] AS [ContactName0], [t0].[ContactTitle] AS [ContactTitle0], [t0].[Country] AS [Country0], [t0].[Fax] AS [Fax0], [t0].[Phone] AS [Phone0], [t0].[PostalCode] AS [PostalCode0], [t0].[Region] AS [Region0]
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[CustomerID]
    ) AS [t]
    CROSS JOIN (
        SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
        FROM [Customers] AS [c0]
        ORDER BY [c0].[CustomerID]
        OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [t0]
    ORDER BY [t].[CustomerID]
) AS [t1]
LEFT JOIN [Orders] AS [o] ON [t1].[CustomerID] = [o].[CustomerID]
ORDER BY [t1].[CustomerID], [t1].[CustomerID0], [o].[OrderID]");
            }
        }

        public override void Include_multiple_references(bool useString)
        {
            base.Include_multiple_references(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]");
        }

        public override void Include_reference_alias_generation(bool useString)
        {
            base.Include_reference_alias_generation(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]");
        }

        public override void Include_duplicate_reference(bool useString)
        {
            base.Include_duplicate_reference(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='2'

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate], [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[CustomerID], [o].[OrderID]
) AS [t]
CROSS JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    ORDER BY [o0].[CustomerID], [o0].[OrderID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [t0]
LEFT JOIN [Customers] AS [c] ON [t].[CustomerID] = [c].[CustomerID]
LEFT JOIN [Customers] AS [c0] ON [t0].[CustomerID] = [c0].[CustomerID]
ORDER BY [t].[CustomerID], [t].[OrderID]");
            }
        }

        public override void Include_duplicate_reference2(bool useString)
        {
            base.Include_duplicate_reference2(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='2'

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]
CROSS JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    ORDER BY [o0].[OrderID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [t0]
LEFT JOIN [Customers] AS [c] ON [t].[CustomerID] = [c].[CustomerID]
ORDER BY [t].[OrderID]");
            }
        }

        public override void Include_duplicate_reference3(bool useString)
        {
            base.Include_duplicate_reference3(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='2'

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t]
CROSS JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    ORDER BY [o0].[OrderID]
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY
) AS [t0]
LEFT JOIN [Customers] AS [c] ON [t0].[CustomerID] = [c].[CustomerID]
ORDER BY [t].[OrderID]");
            }
        }

        public override void Include_reference_when_projection(bool useString)
        {
            base.Include_reference_when_projection(useString);

            AssertSql(
                @"SELECT [o].[CustomerID]
FROM [Orders] AS [o]");
        }

        public override void Include_reference_with_filter_reordered(bool useString)
        {
            base.Include_reference_with_filter_reordered(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override void Include_reference_with_filter(bool useString)
        {
            base.Include_reference_with_filter(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override void Include_collection_dependent_already_tracked_as_no_tracking(bool useString)
        {
            base.Include_collection_dependent_already_tracked_as_no_tracking(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'",
                //
                @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'ALFKI'
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[CustomerID], [o].[OrderID]");
        }

        public override void Include_collection_dependent_already_tracked(bool useString)
        {
            base.Include_collection_dependent_already_tracked(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'",
                //
                @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT TOP(2) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'ALFKI'
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[CustomerID], [o].[OrderID]");
        }

        public override void Include_reference_dependent_already_tracked(bool useString)
        {
            base.Include_reference_dependent_already_tracked(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]");
        }

        public override void Include_reference_as_no_tracking(bool useString)
        {
            base.Include_reference_as_no_tracking(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]");
        }

        public override void Include_collection_as_no_tracking2(bool useString)
        {
            base.Include_collection_as_no_tracking2(useString);

            AssertSql(
                @"@__p_0='5'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[CustomerID], [o].[OrderID]");
        }

        public override void Include_with_complex_projection(bool useString)
        {
            base.Include_with_complex_projection(useString);

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Id]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]");
        }

        public override void Include_with_complex_projection_does_not_change_ordering_of_projection(bool useString)
        {
            base.Include_with_complex_projection_does_not_change_ordering_of_projection(useString);

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Id], (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) AS [TotalOrders]
FROM [Customers] AS [c]
WHERE ([c].[ContactTitle] = N'Owner') AND ((
    SELECT COUNT(*)
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] = [o0].[CustomerID]) > 2)
ORDER BY [c].[CustomerID]");
        }

        public override void Include_with_take(bool useString)
        {
            base.Include_with_take(useString);

            AssertSql(
                @"@__p_0='10'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[City] DESC
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[City] DESC, [t].[CustomerID], [o].[OrderID]");
        }

        public override void Include_with_skip(bool useString)
        {
            base.Include_with_skip(useString);

            if (SupportsOffset)
            {
                AssertSql(
                    @"@__p_0='80'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactName]
    OFFSET @__p_0 ROWS
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[ContactName], [t].[CustomerID], [o].[OrderID]");
            }
        }

        public override void Include_collection_with_multiple_conditional_order_by(bool useString)
        {
            base.Include_collection_with_multiple_conditional_order_by(useString);

            AssertSql(
                @"@__p_0='5'

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], CASE
        WHEN [o].[OrderID] > 0 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [c], CASE
        WHEN [c].[CustomerID] IS NOT NULL THEN [c].[City]
        ELSE N''
    END AS [c0]
    FROM [Orders] AS [o]
    LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
    ORDER BY CASE
        WHEN [o].[OrderID] > 0 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END, CASE
        WHEN [c].[CustomerID] IS NOT NULL THEN [c].[City]
        ELSE N''
    END
) AS [t]
LEFT JOIN [Order Details] AS [o0] ON [t].[OrderID] = [o0].[OrderID]
ORDER BY [t].[c], [t].[c0], [t].[OrderID], [o0].[OrderID], [o0].[ProductID]");
        }

        public override void Then_include_collection_order_by_collection_column(bool useString)
        {
            base.Then_include_collection_order_by_collection_column(useString);

            AssertSql(
                @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [t0].[OrderID], [t0].[CustomerID], [t0].[EmployeeID], [t0].[OrderDate], [t0].[OrderID0], [t0].[ProductID], [t0].[Discount], [t0].[Quantity], [t0].[UnitPrice]
FROM (
    SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], (
        SELECT TOP(1) [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderDate] DESC) AS [c]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'W%'
    ORDER BY (
        SELECT TOP(1) [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderDate] DESC) DESC
) AS [t]
LEFT JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [o1].[OrderID] AS [OrderID0], [o1].[ProductID], [o1].[Discount], [o1].[Quantity], [o1].[UnitPrice]
    FROM [Orders] AS [o0]
    LEFT JOIN [Order Details] AS [o1] ON [o0].[OrderID] = [o1].[OrderID]
) AS [t0] ON [t].[CustomerID] = [t0].[CustomerID]
ORDER BY [t].[c] DESC, [t].[CustomerID], [t0].[OrderID], [t0].[OrderID0], [t0].[ProductID]");
        }

        public override void Include_collection_with_conditional_order_by(bool useString)
        {
            base.Include_collection_with_conditional_order_by(useString);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY CASE
    WHEN [c].[CustomerID] LIKE N'S%' THEN 1
    ELSE 2
END, [c].[CustomerID], [o].[OrderID]");
        }

        public override void Include_reference_distinct_is_server_evaluated(bool useString)
        {
            base.Include_reference_distinct_is_server_evaluated(useString);

            AssertSql(
                @"SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    SELECT DISTINCT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10250
) AS [t]
LEFT JOIN [Customers] AS [c] ON [t].[CustomerID] = [c].[CustomerID]");
        }

        public override void Include_collection_distinct_is_server_evaluated(bool useString)
        {
            base.Include_collection_distinct_is_server_evaluated(useString);

            AssertSql(
                @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'A%'
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[CustomerID], [o].[OrderID]");
        }

        public override void Include_collection_OrderBy_object(bool useString)
        {
            base.Include_collection_OrderBy_object(useString);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Orders] AS [o]
LEFT JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE [o].[OrderID] < 10250
ORDER BY [o].[OrderID], [o0].[OrderID], [o0].[ProductID]");
        }

        public override void Include_collection_OrderBy_empty_list_contains(bool useString)
        {
            base.Include_collection_OrderBy_empty_list_contains(useString);

            AssertSql(
                @"@__p_1='1'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY (SELECT 1), [c].[CustomerID]
OFFSET @__p_1 ROWS",
                //
                @"@__p_1='1'

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID], CAST(0 AS bit) AS [c]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] LIKE N'A%'
    ORDER BY [c], [c0].[CustomerID]
    OFFSET @__p_1 ROWS
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[c], [t].[CustomerID]");
        }

        public override void Include_collection_OrderBy_empty_list_does_not_contains(bool useString)
        {
            base.Include_collection_OrderBy_empty_list_does_not_contains(useString);

            AssertSql(
                @"@__p_1='1'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
ORDER BY (SELECT 1), [c].[CustomerID]
OFFSET @__p_1 ROWS",
                //
                @"@__p_1='1'

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID], CAST(1 AS bit) AS [c]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] LIKE N'A%'
    ORDER BY [c], [c0].[CustomerID]
    OFFSET @__p_1 ROWS
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[c], [t].[CustomerID]");
        }

        public override void Include_collection_OrderBy_list_contains(bool useString)
        {
            base.Include_collection_OrderBy_list_contains(useString);

            AssertSql(
                @"@__p_1='1'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], CASE
        WHEN [c].[CustomerID] IN (N'ALFKI') THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [c]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'A%'
    ORDER BY CASE
        WHEN [c].[CustomerID] IN (N'ALFKI') THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    OFFSET @__p_1 ROWS
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[c], [t].[CustomerID], [o].[OrderID]");
        }

        public override void Include_collection_OrderBy_list_does_not_contains(bool useString)
        {
            base.Include_collection_OrderBy_list_does_not_contains(useString);

            AssertSql(
                @"@__p_1='1'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], CASE
        WHEN [c].[CustomerID] NOT IN (N'ALFKI') THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [c]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'A%'
    ORDER BY CASE
        WHEN [c].[CustomerID] NOT IN (N'ALFKI') THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    OFFSET @__p_1 ROWS
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[c], [t].[CustomerID], [o].[OrderID]");
        }

        public override async Task Include_is_not_ignored_when_projection_contains_client_method_and_complex_expression(
            bool useString, bool async)
        {
            await base.Include_is_not_ignored_when_projection_contains_client_method_and_complex_expression(useString, async);

            AssertSql(
                @"SELECT CASE
    WHEN [e0].[EmployeeID] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title], [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
FROM [Employees] AS [e]
LEFT JOIN [Employees] AS [e0] ON [e].[ReportsTo] = [e0].[EmployeeID]
WHERE ([e].[EmployeeID] = 1) OR ([e].[EmployeeID] = 2)");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
