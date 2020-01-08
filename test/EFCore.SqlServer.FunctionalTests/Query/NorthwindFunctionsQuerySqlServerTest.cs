// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindFunctionsQuerySqlServerTest : NorthwindFunctionsQueryTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        public NorthwindFunctionsQuerySqlServerTest(
#pragma warning disable IDE0060 // Remove unused parameter
            NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task String_StartsWith_Literal(bool async)
        {
            await base.String_StartsWith_Literal(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] IS NOT NULL AND ([c].[ContactName] LIKE N'M%')");
        }

        public override async Task String_StartsWith_Identity(bool async)
        {
            await base.String_StartsWith_Identity(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[ContactName] = N'') OR ([c].[ContactName] IS NOT NULL AND (LEFT([c].[ContactName], LEN([c].[ContactName])) = [c].[ContactName]))");
        }

        public override async Task String_StartsWith_Column(bool async)
        {
            await base.String_StartsWith_Column(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[ContactName] = N'') OR ([c].[ContactName] IS NOT NULL AND (LEFT([c].[ContactName], LEN([c].[ContactName])) = [c].[ContactName]))");
        }

        public override async Task String_StartsWith_MethodCall(bool async)
        {
            await base.String_StartsWith_MethodCall(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] IS NOT NULL AND ([c].[ContactName] LIKE N'M%')");
        }

        public override async Task String_EndsWith_Literal(bool async)
        {
            await base.String_EndsWith_Literal(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] IS NOT NULL AND ([c].[ContactName] LIKE N'%b')");
        }

        public override async Task String_EndsWith_Identity(bool async)
        {
            await base.String_EndsWith_Identity(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[ContactName] = N'') OR ([c].[ContactName] IS NOT NULL AND (RIGHT([c].[ContactName], LEN([c].[ContactName])) = [c].[ContactName]))");
        }

        public override async Task String_EndsWith_Column(bool async)
        {
            await base.String_EndsWith_Column(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[ContactName] = N'') OR ([c].[ContactName] IS NOT NULL AND (RIGHT([c].[ContactName], LEN([c].[ContactName])) = [c].[ContactName]))");
        }

        public override async Task String_EndsWith_MethodCall(bool async)
        {
            await base.String_EndsWith_MethodCall(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] IS NOT NULL AND ([c].[ContactName] LIKE N'%m')");
        }

        public override async Task String_Contains_Literal(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactName.Contains("M")), // case-insensitive
                ss => ss.Set<Customer>().Where(c => c.ContactName.Contains("M") || c.ContactName.Contains("m")), // case-sensitive
                entryCount: 34);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CHARINDEX(N'M', [c].[ContactName]) > 0");
        }

        public override async Task String_Contains_Identity(bool async)
        {
            await base.String_Contains_Identity(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[ContactName] = N'') OR (CHARINDEX([c].[ContactName], [c].[ContactName]) > 0)");
        }

        public override async Task String_Contains_Column(bool async)
        {
            await base.String_Contains_Column(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[ContactName] = N'') OR (CHARINDEX([c].[ContactName], [c].[ContactName]) > 0)");
        }

        public override async Task String_Contains_MethodCall(bool async)
        {
            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.ContactName.Contains(LocalMethod1())), // case-insensitive
                ss => ss.Set<Customer>().Where(
                    c => c.ContactName.Contains(LocalMethod1().ToLower())
                        || c.ContactName.Contains(LocalMethod1().ToUpper())), // case-sensitive
                entryCount: 34);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CHARINDEX(N'M', [c].[ContactName]) > 0");
        }

        public override async Task String_Compare_simple_zero(bool async)
        {
            await base.String_Compare_simple_zero(async);

            // issue #16092
//            AssertSql(
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] = N'ALFKI'",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] <> N'ALFKI'",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] > N'ALFKI'",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] <= N'ALFKI'",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] > N'ALFKI'",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] <= N'ALFKI'");
        }

        public override async Task String_Compare_simple_one(bool async)
        {
            await base.String_Compare_simple_one(async);

            // issue #16092
//            AssertSql(
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] > N'ALFKI'",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] < N'ALFKI'",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] <= N'ALFKI'",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] <= N'ALFKI'",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] >= N'ALFKI'",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] >= N'ALFKI'");
        }

        public override async Task String_compare_with_parameter(bool async)
        {
            await base.String_compare_with_parameter(async);

            // issue #16092
//            AssertSql(
//                @"@__customer_CustomerID_0='ALFKI' (Size = 4000)

//SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] > @__customer_CustomerID_0",
//                //
//                @"@__customer_CustomerID_0='ALFKI' (Size = 4000)

//SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] < @__customer_CustomerID_0",
//                //
//                @"@__customer_CustomerID_0='ALFKI' (Size = 4000)

//SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] <= @__customer_CustomerID_0",
//                //
//                @"@__customer_CustomerID_0='ALFKI' (Size = 4000)

//SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] <= @__customer_CustomerID_0",
//                //
//                @"@__customer_CustomerID_0='ALFKI' (Size = 4000)

//SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] >= @__customer_CustomerID_0",
//                //
//                @"@__customer_CustomerID_0='ALFKI' (Size = 4000)

//SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] >= @__customer_CustomerID_0");
        }

        public override async Task String_Compare_simple_more_than_one(bool async)
        {
            await base.String_Compare_simple_more_than_one(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN 0
    WHEN [c].[CustomerID] > N'ALFKI' THEN 1
    WHEN [c].[CustomerID] < N'ALFKI' THEN -1
END = 42",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN 0
    WHEN [c].[CustomerID] > N'ALFKI' THEN 1
    WHEN [c].[CustomerID] < N'ALFKI' THEN -1
END > 42",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 42 > CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN 0
    WHEN [c].[CustomerID] > N'ALFKI' THEN 1
    WHEN [c].[CustomerID] < N'ALFKI' THEN -1
END");
        }

        public override async Task String_Compare_nested(bool async)
        {
            await base.String_Compare_nested(async);

            // issue #16092
//            AssertSql(
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] = N'M' + [c].[CustomerID]",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] <> UPPER([c].[CustomerID])",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] > REPLACE(N'ALFKI', N'ALF', [c].[CustomerID])",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] <= N'M' + [c].[CustomerID]",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] > UPPER([c].[CustomerID])",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] < REPLACE(N'ALFKI', N'ALF', [c].[CustomerID])");
        }

        public override async Task String_Compare_multi_predicate(bool async)
        {
            await base.String_Compare_multi_predicate(async);

            // issue #16092
//            AssertSql(
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] >= N'ALFKI' AND [c].[CustomerID] < N'CACTU'",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[ContactTitle] = N'Owner' AND [c].[Country] <> N'USA'");
        }

        public override async Task String_Compare_to_simple_zero(bool async)
        {
            await base.String_Compare_to_simple_zero(async);

            // issue #16092
//            AssertSql(
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] = N'ALFKI'",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] <> N'ALFKI'",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] > N'ALFKI'",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] <= N'ALFKI'",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] > N'ALFKI'",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] <= N'ALFKI'");
        }

        public override async Task String_Compare_to_simple_one(bool async)
        {
            await base.String_Compare_to_simple_one(async);

            // issue #16092
//            AssertSql(
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] > N'ALFKI'",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] < N'ALFKI'",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] <= N'ALFKI'",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] <= N'ALFKI'",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] >= N'ALFKI'",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] >= N'ALFKI'");
        }

        public override async Task String_compare_to_with_parameter(bool async)
        {
            await base.String_compare_to_with_parameter(async);

            // issue #16092
//            AssertSql(
//                @"@__customer_CustomerID_0='ALFKI' (Size = 4000)

//SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] > @__customer_CustomerID_0",
//                //
//                @"@__customer_CustomerID_0='ALFKI' (Size = 4000)

//SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] < @__customer_CustomerID_0",
//                //
//                @"@__customer_CustomerID_0='ALFKI' (Size = 4000)

//SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] <= @__customer_CustomerID_0",
//                //
//                @"@__customer_CustomerID_0='ALFKI' (Size = 4000)

//SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] <= @__customer_CustomerID_0",
//                //
//                @"@__customer_CustomerID_0='ALFKI' (Size = 4000)

//SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] >= @__customer_CustomerID_0",
//                //
//                @"@__customer_CustomerID_0='ALFKI' (Size = 4000)

//SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] >= @__customer_CustomerID_0");
        }

        public override async Task String_Compare_to_simple_more_than_one(bool async)
        {
            await base.String_Compare_to_simple_more_than_one(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN 0
    WHEN [c].[CustomerID] > N'ALFKI' THEN 1
    WHEN [c].[CustomerID] < N'ALFKI' THEN -1
END = 42",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN 0
    WHEN [c].[CustomerID] > N'ALFKI' THEN 1
    WHEN [c].[CustomerID] < N'ALFKI' THEN -1
END > 42",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 42 > CASE
    WHEN [c].[CustomerID] = N'ALFKI' THEN 0
    WHEN [c].[CustomerID] > N'ALFKI' THEN 1
    WHEN [c].[CustomerID] < N'ALFKI' THEN -1
END");
        }

        public override async Task String_Compare_to_nested(bool async)
        {
            await base.String_Compare_to_nested(async);

            //issue #16092
//            AssertSql(
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] = N'M' + [c].[CustomerID]",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] <> UPPER([c].[CustomerID])",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] > REPLACE(N'ALFKI', N'ALF', [c].[CustomerID])",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] <= N'M' + [c].[CustomerID]",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] > UPPER([c].[CustomerID])",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] < REPLACE(N'ALFKI', N'ALF', [c].[CustomerID])");
        }

        public override async Task String_Compare_to_multi_predicate(bool async)
        {
            await base.String_Compare_to_multi_predicate(async);

            // issue #16092
//            AssertSql(
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[CustomerID] >= N'ALFKI' AND [c].[CustomerID] < N'CACTU'",
//                //
//                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
//FROM [Customers] AS [c]
//WHERE [c].[ContactTitle] = N'Owner' AND [c].[Country] <> N'USA'");
        }

        public override async Task DateTime_Compare_to_simple_zero(bool async, bool compareTo)
        {
            await base.DateTime_Compare_to_simple_zero(async, compareTo);

            // issue #16092
//            AssertSql(
//                @"@__myDatetime_0='1998-05-04T00:00:00'

//SELECT [c].[OrderID], [c].[CustomerID], [c].[EmployeeID], [c].[OrderDate]
//FROM [Orders] AS [c]
//WHERE [c].[OrderDate] = @__myDatetime_0",
//                //
//                @"@__myDatetime_0='1998-05-04T00:00:00'

//SELECT [c].[OrderID], [c].[CustomerID], [c].[EmployeeID], [c].[OrderDate]
//FROM [Orders] AS [c]
//WHERE [c].[OrderDate] <> @__myDatetime_0",
//                //
//                @"@__myDatetime_0='1998-05-04T00:00:00'

//SELECT [c].[OrderID], [c].[CustomerID], [c].[EmployeeID], [c].[OrderDate]
//FROM [Orders] AS [c]
//WHERE [c].[OrderDate] > @__myDatetime_0",
//                //
//                @"@__myDatetime_0='1998-05-04T00:00:00'

//SELECT [c].[OrderID], [c].[CustomerID], [c].[EmployeeID], [c].[OrderDate]
//FROM [Orders] AS [c]
//WHERE [c].[OrderDate] <= @__myDatetime_0",
//                //
//                @"@__myDatetime_0='1998-05-04T00:00:00'

//SELECT [c].[OrderID], [c].[CustomerID], [c].[EmployeeID], [c].[OrderDate]
//FROM [Orders] AS [c]
//WHERE [c].[OrderDate] > @__myDatetime_0",
//                //
//                @"@__myDatetime_0='1998-05-04T00:00:00'

//SELECT [c].[OrderID], [c].[CustomerID], [c].[EmployeeID], [c].[OrderDate]
//FROM [Orders] AS [c]
//WHERE [c].[OrderDate] <= @__myDatetime_0");
        }

        public override async Task Int_Compare_to_simple_zero(bool async)
        {
            await base.Int_Compare_to_simple_zero(async);

            // issue #16092
//            AssertSql(
//                @"@__orderId_0='10250'

//SELECT [c].[OrderID], [c].[CustomerID], [c].[EmployeeID], [c].[OrderDate]
//FROM [Orders] AS [c]
//WHERE [c].[OrderID] = @__orderId_0",
//                //
//                @"@__orderId_0='10250'

//SELECT [c].[OrderID], [c].[CustomerID], [c].[EmployeeID], [c].[OrderDate]
//FROM [Orders] AS [c]
//WHERE [c].[OrderID] <> @__orderId_0",
//                //
//                @"@__orderId_0='10250'

//SELECT [c].[OrderID], [c].[CustomerID], [c].[EmployeeID], [c].[OrderDate]
//FROM [Orders] AS [c]
//WHERE [c].[OrderID] > @__orderId_0",
//                //
//                @"@__orderId_0='10250'

//SELECT [c].[OrderID], [c].[CustomerID], [c].[EmployeeID], [c].[OrderDate]
//FROM [Orders] AS [c]
//WHERE [c].[OrderID] <= @__orderId_0",
//                //
//                @"@__orderId_0='10250'

//SELECT [c].[OrderID], [c].[CustomerID], [c].[EmployeeID], [c].[OrderDate]
//FROM [Orders] AS [c]
//WHERE [c].[OrderID] > @__orderId_0",
//                //
//                @"@__orderId_0='10250'

//SELECT [c].[OrderID], [c].[CustomerID], [c].[EmployeeID], [c].[OrderDate]
//FROM [Orders] AS [c]
//WHERE [c].[OrderID] <= @__orderId_0");
        }

        public override async Task Where_math_abs1(bool async)
        {
            await base.Where_math_abs1(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ABS([o].[ProductID]) > 10");
        }

        public override async Task Where_math_abs2(bool async)
        {
            await base.Where_math_abs2(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ABS([o].[Quantity]) > CAST(10 AS smallint)");
        }

        public override async Task Where_math_abs3(bool async)
        {
            await base.Where_math_abs3(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ABS([o].[UnitPrice]) > 10.0");
        }

        public override async Task Where_math_abs_uncorrelated(bool async)
        {
            await base.Where_math_abs_uncorrelated(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE 10 < [o].[ProductID]");
        }

        public override async Task Where_math_ceiling1(bool async)
        {
            await base.Where_math_ceiling1(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE CEILING(CAST([o].[Discount] AS float)) > 0.0E0");
        }

        public override async Task Where_math_ceiling2(bool async)
        {
            await base.Where_math_ceiling2(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE CEILING([o].[UnitPrice]) > 10.0");
        }

        public override async Task Where_math_floor(bool async)
        {
            await base.Where_math_floor(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE FLOOR([o].[UnitPrice]) > 10.0");
        }

        public override async Task Where_math_power(bool async)
        {
            await base.Where_math_power(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE POWER(CAST([o].[Discount] AS float), 2.0E0) > 0.05000000074505806E0");
        }

        public override async Task Where_math_round(bool async)
        {
            await base.Where_math_round(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ROUND([o].[UnitPrice], 0) > 10.0");
        }

        public override async Task Select_math_round_int(bool async)
        {
            await base.Select_math_round_int(async);

            AssertSql(
                @"SELECT ROUND(CAST([o].[OrderID] AS float), 0) AS [A]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10250");
        }

        public override async Task Select_math_truncate_int(bool async)
        {
            await base.Select_math_truncate_int(async);

            AssertSql(
                @"SELECT ROUND(CAST([o].[OrderID] AS float), 0, 1) AS [A]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10250");
        }

        public override async Task Where_math_round2(bool async)
        {
            await base.Where_math_round2(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ROUND([o].[UnitPrice], 2) > 100.0");
        }

        public override async Task Where_math_truncate(bool async)
        {
            await base.Where_math_truncate(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ROUND([o].[UnitPrice], 0, 1) > 10.0");
        }

        public override async Task Where_math_exp(bool async)
        {
            await base.Where_math_exp(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (EXP(CAST([o].[Discount] AS float)) > 1.0E0)");
        }

        public override async Task Where_math_log10(bool async)
        {
            await base.Where_math_log10(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE (([o].[OrderID] = 11077) AND ([o].[Discount] > CAST(0 AS real))) AND (LOG10(CAST([o].[Discount] AS float)) < 0.0E0)");
        }

        public override async Task Where_math_log(bool async)
        {
            await base.Where_math_log(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE (([o].[OrderID] = 11077) AND ([o].[Discount] > CAST(0 AS real))) AND (LOG(CAST([o].[Discount] AS float)) < 0.0E0)");
        }

        public override async Task Where_math_log_new_base(bool async)
        {
            await base.Where_math_log_new_base(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE (([o].[OrderID] = 11077) AND ([o].[Discount] > CAST(0 AS real))) AND (LOG(CAST([o].[Discount] AS float), 7.0E0) < 0.0E0)");
        }

        public override async Task Where_math_sqrt(bool async)
        {
            await base.Where_math_sqrt(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (SQRT(CAST([o].[Discount] AS float)) > 0.0E0)");
        }

        public override async Task Where_math_acos(bool async)
        {
            await base.Where_math_acos(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (ACOS(CAST([o].[Discount] AS float)) > 1.0E0)");
        }

        public override async Task Where_math_asin(bool async)
        {
            await base.Where_math_asin(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (ASIN(CAST([o].[Discount] AS float)) > 0.0E0)");
        }

        public override async Task Where_math_atan(bool async)
        {
            await base.Where_math_atan(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (ATAN(CAST([o].[Discount] AS float)) > 0.0E0)");
        }

        public override async Task Where_math_atan2(bool async)
        {
            await base.Where_math_atan2(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (ATN2(CAST([o].[Discount] AS float), 1.0E0) > 0.0E0)");
        }

        public override async Task Where_math_cos(bool async)
        {
            await base.Where_math_cos(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (COS(CAST([o].[Discount] AS float)) > 0.0E0)");
        }

        public override async Task Where_math_sin(bool async)
        {
            await base.Where_math_sin(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (SIN(CAST([o].[Discount] AS float)) > 0.0E0)");
        }

        public override async Task Where_math_tan(bool async)
        {
            await base.Where_math_tan(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (TAN(CAST([o].[Discount] AS float)) > 0.0E0)");
        }

        public override async Task Where_math_sign(bool async)
        {
            await base.Where_math_sign(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (SIGN([o].[Discount]) > 0)");
        }

        [ConditionalTheory(Skip = "Issue#17328")]
        public override Task Where_math_min(bool async) => base.Where_math_min(async);

        [ConditionalTheory(Skip = "Issue#17328")]
        public override Task Where_math_max(bool async) => base.Where_math_max(async);

        public override async Task Where_guid_newguid(bool async)
        {
            await base.Where_guid_newguid(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE (NEWID() <> '00000000-0000-0000-0000-000000000000') OR NEWID() IS NULL");
        }

        public override async Task Where_string_to_upper(bool async)
        {
            await base.Where_string_to_upper(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE UPPER([c].[CustomerID]) = N'ALFKI'");
        }

        public override async Task Where_string_to_lower(bool async)
        {
            await base.Where_string_to_lower(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE LOWER([c].[CustomerID]) = N'alfki'");
        }

        public override async Task Where_functions_nested(bool async)
        {
            await base.Where_functions_nested(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE POWER(CAST(CAST(LEN([c].[CustomerID]) AS int) AS float), 2.0E0) = 25.0E0");
        }

        public override async Task Convert_ToByte(bool async)
        {
            await base.Convert_ToByte(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(tinyint, [o].[OrderID] % 1)) >= CAST(0 AS tinyint))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(decimal(18, 2), [o].[OrderID] % 1)) >= CAST(0 AS tinyint))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(float, [o].[OrderID] % 1)) >= CAST(0 AS tinyint))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CAST(CONVERT(float, [o].[OrderID] % 1) AS real)) >= CAST(0 AS tinyint))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(smallint, [o].[OrderID] % 1)) >= CAST(0 AS tinyint))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(int, [o].[OrderID] % 1)) >= CAST(0 AS tinyint))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(bigint, [o].[OrderID] % 1)) >= CAST(0 AS tinyint))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(nvarchar(max), [o].[OrderID] % 1)) >= CAST(0 AS tinyint))");
        }

        public override async Task Convert_ToDecimal(bool async)
        {
            await base.Convert_ToDecimal(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal(18, 2), CONVERT(tinyint, [o].[OrderID] % 1)) >= 0.0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal(18, 2), CONVERT(decimal(18, 2), [o].[OrderID] % 1)) >= 0.0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal(18, 2), CONVERT(float, [o].[OrderID] % 1)) >= 0.0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal(18, 2), CAST(CONVERT(float, [o].[OrderID] % 1) AS real)) >= 0.0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal(18, 2), CONVERT(smallint, [o].[OrderID] % 1)) >= 0.0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal(18, 2), CONVERT(int, [o].[OrderID] % 1)) >= 0.0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal(18, 2), CONVERT(bigint, [o].[OrderID] % 1)) >= 0.0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal(18, 2), CONVERT(nvarchar(max), [o].[OrderID] % 1)) >= 0.0)");
        }

        public override async Task Convert_ToDouble(bool async)
        {
            await base.Convert_ToDouble(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(tinyint, [o].[OrderID] % 1)) >= 0.0E0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(decimal(18, 2), [o].[OrderID] % 1)) >= 0.0E0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(float, [o].[OrderID] % 1)) >= 0.0E0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CAST(CONVERT(float, [o].[OrderID] % 1) AS real)) >= 0.0E0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(smallint, [o].[OrderID] % 1)) >= 0.0E0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(int, [o].[OrderID] % 1)) >= 0.0E0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(bigint, [o].[OrderID] % 1)) >= 0.0E0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(nvarchar(max), [o].[OrderID] % 1)) >= 0.0E0)");
        }

        public override async Task Convert_ToInt16(bool async)
        {
            await base.Convert_ToInt16(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(tinyint, [o].[OrderID] % 1)) >= CAST(0 AS smallint))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(decimal(18, 2), [o].[OrderID] % 1)) >= CAST(0 AS smallint))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(float, [o].[OrderID] % 1)) >= CAST(0 AS smallint))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CAST(CONVERT(float, [o].[OrderID] % 1) AS real)) >= CAST(0 AS smallint))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(smallint, [o].[OrderID] % 1)) >= CAST(0 AS smallint))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(int, [o].[OrderID] % 1)) >= CAST(0 AS smallint))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(bigint, [o].[OrderID] % 1)) >= CAST(0 AS smallint))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(nvarchar(max), [o].[OrderID] % 1)) >= CAST(0 AS smallint))");
        }

        public override async Task Convert_ToInt32(bool async)
        {
            await base.Convert_ToInt32(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(tinyint, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(decimal(18, 2), [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(float, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CAST(CONVERT(float, [o].[OrderID] % 1) AS real)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(smallint, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(int, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(bigint, [o].[OrderID] % 1)) >= 0)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(nvarchar(max), [o].[OrderID] % 1)) >= 0)");
        }

        public override async Task Convert_ToInt64(bool async)
        {
            await base.Convert_ToInt64(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(tinyint, [o].[OrderID] % 1)) >= CAST(0 AS bigint))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(decimal(18, 2), [o].[OrderID] % 1)) >= CAST(0 AS bigint))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(float, [o].[OrderID] % 1)) >= CAST(0 AS bigint))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CAST(CONVERT(float, [o].[OrderID] % 1) AS real)) >= CAST(0 AS bigint))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(smallint, [o].[OrderID] % 1)) >= CAST(0 AS bigint))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(int, [o].[OrderID] % 1)) >= CAST(0 AS bigint))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(bigint, [o].[OrderID] % 1)) >= CAST(0 AS bigint))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(nvarchar(max), [o].[OrderID] % 1)) >= CAST(0 AS bigint))");
        }

        public override async Task Convert_ToString(bool async)
        {
            await base.Convert_ToString(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND ((CONVERT(nvarchar(max), CONVERT(tinyint, [o].[OrderID] % 1)) <> N'10') OR CONVERT(nvarchar(max), CONVERT(tinyint, [o].[OrderID] % 1)) IS NULL)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND ((CONVERT(nvarchar(max), CONVERT(decimal(18, 2), [o].[OrderID] % 1)) <> N'10') OR CONVERT(nvarchar(max), CONVERT(decimal(18, 2), [o].[OrderID] % 1)) IS NULL)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND ((CONVERT(nvarchar(max), CONVERT(float, [o].[OrderID] % 1)) <> N'10') OR CONVERT(nvarchar(max), CONVERT(float, [o].[OrderID] % 1)) IS NULL)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND ((CONVERT(nvarchar(max), CAST(CONVERT(float, [o].[OrderID] % 1) AS real)) <> N'10') OR CONVERT(nvarchar(max), CAST(CONVERT(float, [o].[OrderID] % 1) AS real)) IS NULL)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND ((CONVERT(nvarchar(max), CONVERT(smallint, [o].[OrderID] % 1)) <> N'10') OR CONVERT(nvarchar(max), CONVERT(smallint, [o].[OrderID] % 1)) IS NULL)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND ((CONVERT(nvarchar(max), CONVERT(int, [o].[OrderID] % 1)) <> N'10') OR CONVERT(nvarchar(max), CONVERT(int, [o].[OrderID] % 1)) IS NULL)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND ((CONVERT(nvarchar(max), CONVERT(bigint, [o].[OrderID] % 1)) <> N'10') OR CONVERT(nvarchar(max), CONVERT(bigint, [o].[OrderID] % 1)) IS NULL)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND ((CONVERT(nvarchar(max), CONVERT(nvarchar(max), [o].[OrderID] % 1)) <> N'10') OR CONVERT(nvarchar(max), CONVERT(nvarchar(max), [o].[OrderID] % 1)) IS NULL)",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND ((CHARINDEX(N'1997', CONVERT(nvarchar(max), [o].[OrderDate])) > 0) OR (CHARINDEX(N'1998', CONVERT(nvarchar(max), [o].[OrderDate])) > 0))");
        }

        public override async Task Indexof_with_emptystring(bool async)
        {
            await base.Indexof_with_emptystring(async);

            AssertSql(
                @"SELECT CASE
    WHEN 1 = 1 THEN 0
    ELSE CAST(CHARINDEX(N'', [c].[ContactName]) AS int) - 1
END
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Replace_with_emptystring(bool async)
        {
            await base.Replace_with_emptystring(async);

            AssertSql(
                @"SELECT REPLACE([c].[ContactName], N'ari', N'')
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Substring_with_zero_startindex(bool async)
        {
            await base.Substring_with_zero_startindex(async);

            AssertSql(
                @"SELECT SUBSTRING([c].[ContactName], 0 + 1, 3)
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Substring_with_zero_length(bool async)
        {
            await base.Substring_with_zero_length(async);

            AssertSql(
                @"SELECT SUBSTRING([c].[ContactName], 2 + 1, 0)
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Substring_with_constant(bool async)
        {
            await base.Substring_with_constant(async);

            AssertSql(
                @"SELECT SUBSTRING([c].[ContactName], 1 + 1, 3)
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Substring_with_closure(bool async)
        {
            await base.Substring_with_closure(async);

            AssertSql(
                @"@__start_0='2'

SELECT SUBSTRING([c].[ContactName], @__start_0 + 1, 3)
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Substring_with_Index_of(bool async)
        {
            await base.Substring_with_Index_of(async);

            AssertSql(
                @"SELECT SUBSTRING([c].[ContactName], CASE
    WHEN N'a' = N'' THEN 0
    ELSE CAST(CHARINDEX(N'a', [c].[ContactName]) AS int) - 1
END + 1, 3)
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task IsNullOrEmpty_in_predicate(bool async)
        {
            await base.IsNullOrEmpty_in_predicate(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[Region] IS NULL OR ([c].[Region] = N'')");
        }

        public override void IsNullOrEmpty_in_projection()
        {
            base.IsNullOrEmpty_in_projection();

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Id], CASE
    WHEN [c].[Region] IS NULL OR (([c].[Region] = N'') AND [c].[Region] IS NOT NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Value]
FROM [Customers] AS [c]");
        }

        public override void IsNullOrEmpty_negated_in_projection()
        {
            base.IsNullOrEmpty_negated_in_projection();

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Id], CASE
    WHEN [c].[Region] IS NOT NULL AND (([c].[Region] <> N'') OR [c].[Region] IS NULL) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Value]
FROM [Customers] AS [c]");
        }

        public override async Task IsNullOrWhiteSpace_in_predicate(bool async)
        {
            await base.IsNullOrWhiteSpace_in_predicate(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[Region] IS NULL OR (LTRIM(RTRIM([c].[Region])) = N'')");
        }

        public override async Task IsNullOrWhiteSpace_in_predicate_on_non_nullable_column(bool async)
        {
            await base.IsNullOrWhiteSpace_in_predicate_on_non_nullable_column(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE LTRIM(RTRIM([c].[CustomerID])) = N''");
        }

        public override async Task TrimStart_without_arguments_in_predicate(bool async)
        {
            await base.TrimStart_without_arguments_in_predicate(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE LTRIM([c].[ContactTitle]) = N'Owner'");
        }

        [ConditionalTheory(Skip = "Issue#17328")]
        public override Task TrimStart_with_char_argument_in_predicate(bool async)
            => base.TrimStart_with_char_argument_in_predicate(async);

        [ConditionalTheory(Skip = "Issue#17328")]
        public override Task TrimStart_with_char_array_argument_in_predicate(bool async)
            => base.TrimStart_with_char_array_argument_in_predicate(async);

        public override async Task TrimEnd_without_arguments_in_predicate(bool async)
        {
            await base.TrimEnd_without_arguments_in_predicate(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE RTRIM([c].[ContactTitle]) = N'Owner'");
        }

        [ConditionalTheory(Skip = "Issue#17328")]
        public override Task TrimEnd_with_char_argument_in_predicate(bool async)
            => base.TrimEnd_with_char_argument_in_predicate(async);

        [ConditionalTheory(Skip = "Issue#17328")]
        public override Task TrimEnd_with_char_array_argument_in_predicate(bool async)
            => base.TrimEnd_with_char_array_argument_in_predicate(async);

        public override async Task Trim_without_argument_in_predicate(bool async)
        {
            await base.Trim_without_argument_in_predicate(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE LTRIM(RTRIM([c].[ContactTitle])) = N'Owner'");
        }

        [ConditionalTheory(Skip = "Issue#17328")]
        public override Task Trim_with_char_argument_in_predicate(bool async)
            => base.Trim_with_char_argument_in_predicate(async);

        [ConditionalTheory(Skip = "Issue#17328")]
        public override Task Trim_with_char_array_argument_in_predicate(bool async)
            => base.Trim_with_char_array_argument_in_predicate(async);

        public override async Task Order_by_length_twice(bool async)
        {
            await base.Order_by_length_twice(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY CAST(LEN([c].[CustomerID]) AS int), [c].[CustomerID]");
        }

        public override async Task Order_by_length_twice_followed_by_projection_of_naked_collection_navigation(bool async)
        {
            await base.Order_by_length_twice_followed_by_projection_of_naked_collection_navigation(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY CAST(LEN([c].[CustomerID]) AS int), [c].[CustomerID], [o].[OrderID]");
        }

        public override async Task Static_string_equals_in_predicate(bool async)
        {
            await base.Static_string_equals_in_predicate(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ANATR'");
        }

        public override async Task Static_equals_nullable_datetime_compared_to_non_nullable(bool async)
        {
            await base.Static_equals_nullable_datetime_compared_to_non_nullable(async);

            AssertSql(
                @"@__arg_0='1996-07-04T00:00:00'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] = @__arg_0");
        }

        public override async Task Static_equals_int_compared_to_long(bool async)
        {
            await base.Static_equals_int_compared_to_long(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE 0 = 1");
        }

        public override async Task Projecting_Math_Truncate_and_ordering_by_it_twice(bool async)
        {
            await base.Projecting_Math_Truncate_and_ordering_by_it_twice(async);

            // issue #16038
//            AssertSql(
//                @"SELECT ROUND(CAST([o].[OrderID] AS float), 0, 1) AS [A]
//FROM [Orders] AS [o]
//WHERE [o].[OrderID] < 10250
//ORDER BY [A]");
        }

        public override async Task Projecting_Math_Truncate_and_ordering_by_it_twice2(bool async)
        {
            await base.Projecting_Math_Truncate_and_ordering_by_it_twice2(async);

            // issue #16038
//            AssertSql(
//                @"SELECT ROUND(CAST([o].[OrderID] AS float), 0, 1) AS [A]
//FROM [Orders] AS [o]
//WHERE [o].[OrderID] < 10250
//ORDER BY [A] DESC");
        }

        public override async Task Projecting_Math_Truncate_and_ordering_by_it_twice3(bool async)
        {
            await base.Projecting_Math_Truncate_and_ordering_by_it_twice3(async);

            // issue #16038
//            AssertSql(
//                @"SELECT ROUND(CAST([o].[OrderID] AS float), 0, 1) AS [A]
//FROM [Orders] AS [o]
//WHERE [o].[OrderID] < 10250
//ORDER BY [A] DESC");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
