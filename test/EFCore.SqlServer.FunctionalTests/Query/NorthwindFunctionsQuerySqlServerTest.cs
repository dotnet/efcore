﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindFunctionsQuerySqlServerTest : NorthwindFunctionsQueryRelationalTestBase<
        NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        public NorthwindFunctionsQuerySqlServerTest(
#pragma warning disable IDE0060 // Remove unused parameter
            NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
            : base(fixture)
        {
            ClearLog();
            Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override bool CanExecuteQueryString
            => true;

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
WHERE [c].[ContactName] LIKE N'%M%'");
        }

        public override async Task String_Contains_Identity(bool async)
        {
            await base.String_Contains_Identity(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[ContactName] LIKE N'') OR (CHARINDEX([c].[ContactName], [c].[ContactName]) > 0)");
        }

        public override async Task String_Contains_Column(bool async)
        {
            await base.String_Contains_Column(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[ContactName] LIKE N'') OR (CHARINDEX([c].[ContactName], [c].[ContactName]) > 0)");
        }

        public override async Task String_Contains_constant_with_whitespace(bool async)
        {
            await base.String_Contains_constant_with_whitespace(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE N'%     %'");
        }

        public override async Task String_Contains_parameter_with_whitespace(bool async)
        {
            await base.String_Contains_parameter_with_whitespace(async);

            AssertSql(
                @"@__pattern_0='     ' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (@__pattern_0 LIKE N'') OR (CHARINDEX(@__pattern_0, [c].[ContactName]) > 0)");
        }

        public override async Task String_FirstOrDefault_MethodCall(bool async)
        {
            await base.String_FirstOrDefault_MethodCall(async);
            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE SUBSTRING([c].[ContactName], 1, 1) = N'A'");
        }

        public override async Task String_LastOrDefault_MethodCall(bool async)
        {
            await base.String_LastOrDefault_MethodCall(async);
            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE SUBSTRING([c].[ContactName], LEN([c].[ContactName]), 1) = N's'");
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
WHERE [c].[ContactName] LIKE N'%M%'");
        }

        public override async Task String_Compare_simple_zero(bool async)
        {
            await base.String_Compare_simple_zero(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <> N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'ALFKI'");
        }

        public override async Task String_Compare_simple_one(bool async)
        {
            await base.String_Compare_simple_one(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] < N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] >= N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] >= N'ALFKI'");
        }

        public override async Task String_compare_with_parameter(bool async)
        {
            await base.String_compare_with_parameter(async);

            AssertSql(
                @"@__customer_CustomerID_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > @__customer_CustomerID_0",
                //
                @"@__customer_CustomerID_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] < @__customer_CustomerID_0",
                //
                @"@__customer_CustomerID_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= @__customer_CustomerID_0",
                //
                @"@__customer_CustomerID_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= @__customer_CustomerID_0",
                //
                @"@__customer_CustomerID_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] >= @__customer_CustomerID_0",
                //
                @"@__customer_CustomerID_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] >= @__customer_CustomerID_0");
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

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = (N'M' + [c].[CustomerID])",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <> UPPER([c].[CustomerID])",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > REPLACE(N'ALFKI', N'ALF', [c].[CustomerID])",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= (N'M' + [c].[CustomerID])",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > UPPER([c].[CustomerID])",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] < REPLACE(N'ALFKI', N'ALF', [c].[CustomerID])");
        }

        public override async Task String_Compare_multi_predicate(bool async)
        {
            await base.String_Compare_multi_predicate(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] >= N'ALFKI') AND ([c].[CustomerID] < N'CACTU')",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[ContactTitle] = N'Owner') AND (([c].[Country] <> N'USA') OR [c].[Country] IS NULL)");
        }

        public override async Task String_Compare_to_simple_zero(bool async)
        {
            await base.String_Compare_to_simple_zero(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <> N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'ALFKI'");
        }

        public override async Task String_Compare_to_simple_one(bool async)
        {
            await base.String_Compare_to_simple_one(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] < N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] >= N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] >= N'ALFKI'");
        }

        public override async Task String_compare_to_with_parameter(bool async)
        {
            await base.String_compare_to_with_parameter(async);

            AssertSql(
                @"@__customer_CustomerID_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > @__customer_CustomerID_0",
                //
                @"@__customer_CustomerID_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] < @__customer_CustomerID_0",
                //
                @"@__customer_CustomerID_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= @__customer_CustomerID_0",
                //
                @"@__customer_CustomerID_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= @__customer_CustomerID_0",
                //
                @"@__customer_CustomerID_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] >= @__customer_CustomerID_0",
                //
                @"@__customer_CustomerID_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] >= @__customer_CustomerID_0");
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

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = (N'M' + [c].[CustomerID])",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <> UPPER([c].[CustomerID])",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > REPLACE(N'ALFKI', N'ALF', [c].[CustomerID])",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= (N'M' + [c].[CustomerID])",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] > UPPER([c].[CustomerID])",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] < REPLACE(N'ALFKI', N'ALF', [c].[CustomerID])");
        }

        public override async Task String_Compare_to_multi_predicate(bool async)
        {
            await base.String_Compare_to_multi_predicate(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] >= N'ALFKI') AND ([c].[CustomerID] < N'CACTU')",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE ([c].[ContactTitle] = N'Owner') AND (([c].[Country] <> N'USA') OR [c].[Country] IS NULL)");
        }

        public override async Task DateTime_Compare_to_simple_zero(bool async, bool compareTo)
        {
            await base.DateTime_Compare_to_simple_zero(async, compareTo);

            AssertSql(
                @"@__myDatetime_0='1998-05-04T00:00:00.0000000' (DbType = DateTime)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] = @__myDatetime_0",
                //
                @"@__myDatetime_0='1998-05-04T00:00:00.0000000' (DbType = DateTime)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[OrderDate] <> @__myDatetime_0) OR [o].[OrderDate] IS NULL",
                //
                @"@__myDatetime_0='1998-05-04T00:00:00.0000000' (DbType = DateTime)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] > @__myDatetime_0",
                //
                @"@__myDatetime_0='1998-05-04T00:00:00.0000000' (DbType = DateTime)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] <= @__myDatetime_0",
                //
                @"@__myDatetime_0='1998-05-04T00:00:00.0000000' (DbType = DateTime)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] > @__myDatetime_0",
                //
                @"@__myDatetime_0='1998-05-04T00:00:00.0000000' (DbType = DateTime)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] <= @__myDatetime_0");
        }

        public override async Task Int_Compare_to_simple_zero(bool async)
        {
            await base.Int_Compare_to_simple_zero(async);

            AssertSql(
                @"@__orderId_0='10250'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = @__orderId_0",
                //
                @"@__orderId_0='10250'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] <> @__orderId_0",
                //
                @"@__orderId_0='10250'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] > @__orderId_0",
                //
                @"@__orderId_0='10250'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] <= @__orderId_0",
                //
                @"@__orderId_0='10250'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] > @__orderId_0",
                //
                @"@__orderId_0='10250'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] <= @__orderId_0");
        }

        public override async Task Where_math_abs1(bool async)
        {
            await base.Where_math_abs1(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ABS([p].[ProductID]) > 10");
        }

        public override async Task Where_math_abs2(bool async)
        {
            await base.Where_math_abs2(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[UnitPrice] < 7.0) AND (ABS([o].[Quantity]) > CAST(10 AS smallint))");
        }

        public override async Task Where_math_abs3(bool async)
        {
            await base.Where_math_abs3(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[Quantity] < CAST(5 AS smallint)) AND (ABS([o].[UnitPrice]) > 10.0)");
        }

        public override async Task Where_math_abs_uncorrelated(bool async)
        {
            await base.Where_math_abs_uncorrelated(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[UnitPrice] < 7.0) AND (10 < [o].[ProductID])");
        }

        public override async Task Where_math_ceiling1(bool async)
        {
            await base.Where_math_ceiling1(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[UnitPrice] < 7.0) AND (CEILING(CAST([o].[Discount] AS float)) > 0.0E0)");
        }

        public override async Task Where_math_ceiling2(bool async)
        {
            await base.Where_math_ceiling2(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[Quantity] < CAST(5 AS smallint)) AND (CEILING([o].[UnitPrice]) > 10.0)");
        }

        public override async Task Where_math_floor(bool async)
        {
            await base.Where_math_floor(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[Quantity] < CAST(5 AS smallint)) AND (FLOOR([o].[UnitPrice]) > 10.0)");
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
WHERE ([o].[Quantity] < CAST(5 AS smallint)) AND (ROUND([o].[UnitPrice], 0) > 10.0)");
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
WHERE ([o].[Quantity] < CAST(5 AS smallint)) AND (ROUND([o].[UnitPrice], 0, 1) > 10.0)");
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
        public override Task Where_math_min(bool async)
            => base.Where_math_min(async);

        [ConditionalTheory(Skip = "Issue#17328")]
        public override Task Where_math_max(bool async)
            => base.Where_math_max(async);

        public override async Task Where_mathf_abs1(bool async)
        {
            await base.Where_mathf_abs1(async);

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE ABS(CAST([p].[ProductID] AS real)) > CAST(10 AS real)");
        }
        
        public override async Task Where_mathf_ceiling1(bool async)
        {
            await base.Where_mathf_ceiling1(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[UnitPrice] < 7.0) AND (CEILING([o].[Discount]) > CAST(0 AS real))");
        }
        
        public override async Task Where_mathf_floor(bool async)
        {
            await base.Where_mathf_floor(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[Quantity] < CAST(5 AS smallint)) AND (FLOOR(CAST([o].[UnitPrice] AS real)) > CAST(10 AS real))");
        }

        public override async Task Where_mathf_power(bool async)
        {
            await base.Where_mathf_power(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE POWER([o].[Discount], CAST(2 AS real)) > CAST(0.05 AS real)");
        }
        
        public override async Task Where_mathf_round2(bool async)
        {
            await base.Where_mathf_round2(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ROUND(CAST([o].[UnitPrice] AS real), 2) > CAST(100 AS real)");
        }

        public override async Task Where_mathf_truncate(bool async)
        {
            await base.Where_mathf_truncate(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[Quantity] < CAST(5 AS smallint)) AND (ROUND(CAST([o].[UnitPrice] AS real), 0, 1) > CAST(10 AS real))");
        }

        public override async Task Where_mathf_exp(bool async)
        {
            await base.Where_mathf_exp(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (EXP([o].[Discount]) > CAST(1 AS real))");
        }

        public override async Task Where_mathf_log10(bool async)
        {
            await base.Where_mathf_log10(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE (([o].[OrderID] = 11077) AND ([o].[Discount] > CAST(0 AS real))) AND (LOG10([o].[Discount]) < CAST(0 AS real))");
        }

        public override async Task Where_mathf_log(bool async)
        {
            await base.Where_mathf_log(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE (([o].[OrderID] = 11077) AND ([o].[Discount] > CAST(0 AS real))) AND (LOG([o].[Discount]) < CAST(0 AS real))");
        }

        public override async Task Where_mathf_log_new_base(bool async)
        {
            await base.Where_mathf_log_new_base(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE (([o].[OrderID] = 11077) AND ([o].[Discount] > CAST(0 AS real))) AND (LOG([o].[Discount], CAST(7 AS real)) < CAST(0 AS real))");
        }

        public override async Task Where_mathf_sqrt(bool async)
        {
            await base.Where_mathf_sqrt(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (SQRT([o].[Discount]) > CAST(0 AS real))");
        }

        public override async Task Where_mathf_acos(bool async)
        {
            await base.Where_mathf_acos(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (ACOS([o].[Discount]) > CAST(1 AS real))");
        }

        public override async Task Where_mathf_asin(bool async)
        {
            await base.Where_mathf_asin(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (ASIN([o].[Discount]) > CAST(0 AS real))");
        }

        public override async Task Where_mathf_atan(bool async)
        {
            await base.Where_mathf_atan(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (ATAN([o].[Discount]) > CAST(0 AS real))");
        }

        public override async Task Where_mathf_atan2(bool async)
        {
            await base.Where_mathf_atan2(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (ATN2([o].[Discount], CAST(1 AS real)) > CAST(0 AS real))");
        }

        public override async Task Where_mathf_cos(bool async)
        {
            await base.Where_mathf_cos(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (COS([o].[Discount]) > CAST(0 AS real))");
        }

        public override async Task Where_mathf_sin(bool async)
        {
            await base.Where_mathf_sin(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (SIN([o].[Discount]) > CAST(0 AS real))");
        }

        public override async Task Where_mathf_tan(bool async)
        {
            await base.Where_mathf_tan(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (TAN([o].[Discount]) > CAST(0 AS real))");
        }

        public override async Task Where_mathf_sign(bool async)
        {
            await base.Where_mathf_sign(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 11077) AND (SIGN([o].[Discount]) > 0)");
        }

        public override async Task Where_guid_newguid(bool async)
        {
            await base.Where_guid_newguid(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NEWID() <> '00000000-0000-0000-0000-000000000000'");
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

        public override async Task Convert_ToBoolean(bool async)
        {
            await base.Convert_ToBoolean(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bit, CONVERT(bit, [o].[OrderID] % 3)) = CAST(1 AS bit))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bit, CONVERT(tinyint, [o].[OrderID] % 3)) = CAST(1 AS bit))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bit, CONVERT(decimal(18, 2), [o].[OrderID] % 3)) = CAST(1 AS bit))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bit, CONVERT(float, [o].[OrderID] % 3)) = CAST(1 AS bit))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bit, CAST(CONVERT(float, [o].[OrderID] % 3) AS real)) = CAST(1 AS bit))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bit, CONVERT(smallint, [o].[OrderID] % 3)) = CAST(1 AS bit))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bit, CONVERT(int, [o].[OrderID] % 3)) = CAST(1 AS bit))",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bit, CONVERT(bigint, [o].[OrderID] % 3)) = CAST(1 AS bit))");
        }

        public override async Task Convert_ToByte(bool async)
        {
            await base.Convert_ToByte(async);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(tinyint, CONVERT(bit, [o].[OrderID] % 1)) >= CAST(0 AS tinyint))",
                //
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
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(decimal(18, 2), CONVERT(bit, [o].[OrderID] % 1)) >= 0.0)",
                //
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
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(float, CONVERT(bit, [o].[OrderID] % 1)) >= 0.0E0)",
                //
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
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(smallint, CONVERT(bit, [o].[OrderID] % 1)) >= CAST(0 AS smallint))",
                //
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
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(int, CONVERT(bit, [o].[OrderID] % 1)) >= 0)",
                //
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
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(bigint, CONVERT(bit, [o].[OrderID] % 1)) >= CAST(0 AS bigint))",
                //
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
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar(max), CONVERT(bit, [o].[OrderID] % 1)) <> N'10')",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar(max), CONVERT(tinyint, [o].[OrderID] % 1)) <> N'10')",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar(max), CONVERT(decimal(18, 2), [o].[OrderID] % 1)) <> N'10')",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar(max), CONVERT(float, [o].[OrderID] % 1)) <> N'10')",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar(max), CAST(CONVERT(float, [o].[OrderID] % 1) AS real)) <> N'10')",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar(max), CONVERT(smallint, [o].[OrderID] % 1)) <> N'10')",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar(max), CONVERT(int, [o].[OrderID] % 1)) <> N'10')",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar(max), CONVERT(bigint, [o].[OrderID] % 1)) <> N'10')",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND (CONVERT(nvarchar(max), CONVERT(nvarchar(max), [o].[OrderID] % 1)) <> N'10')",
                //
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[CustomerID] = N'ALFKI') AND ((CONVERT(nvarchar(max), [o].[OrderDate]) LIKE N'%1997%') OR (CONVERT(nvarchar(max), [o].[OrderDate]) LIKE N'%1998%'))");
        }

        public override async Task Indexof_with_emptystring(bool async)
        {
            await base.Indexof_with_emptystring(async);

            AssertSql(
                @"SELECT 0
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

        public override async Task Substring_with_one_arg_with_zero_startindex(bool async)
        {
            await base.Substring_with_one_arg_with_zero_startindex(async);

            AssertSql(
                @"SELECT [c].[ContactName]
FROM [Customers] AS [c]
WHERE SUBSTRING([c].[CustomerID], 0 + 1, LEN([c].[CustomerID])) = N'ALFKI'");
        }

        public override async Task Substring_with_one_arg_with_constant(bool async)
        {
            await base.Substring_with_one_arg_with_constant(async);

            AssertSql(
                @"SELECT [c].[ContactName]
FROM [Customers] AS [c]
WHERE SUBSTRING([c].[CustomerID], 1 + 1, LEN([c].[CustomerID])) = N'LFKI'");
        }

        public override async Task Substring_with_one_arg_with_closure(bool async)
        {
            await base.Substring_with_one_arg_with_closure(async);

            AssertSql(
                @"@__start_0='2'

SELECT [c].[ContactName]
FROM [Customers] AS [c]
WHERE SUBSTRING([c].[CustomerID], @__start_0 + 1, LEN([c].[CustomerID])) = N'FKI'");
        }

        public override async Task Substring_with_two_args_with_zero_startindex(bool async)
        {
            await base.Substring_with_two_args_with_zero_startindex(async);

            AssertSql(
                @"SELECT SUBSTRING([c].[ContactName], 0 + 1, 3)
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Substring_with_two_args_with_zero_length(bool async)
        {
            await base.Substring_with_two_args_with_zero_length(async);

            AssertSql(
                @"SELECT SUBSTRING([c].[ContactName], 2 + 1, 0)
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Substring_with_two_args_with_constant(bool async)
        {
            await base.Substring_with_two_args_with_constant(async);

            AssertSql(
                @"SELECT SUBSTRING([c].[ContactName], 1 + 1, 3)
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Substring_with_two_args_with_closure(bool async)
        {
            await base.Substring_with_two_args_with_closure(async);

            AssertSql(
                @"@__start_0='2'

SELECT SUBSTRING([c].[ContactName], @__start_0 + 1, 3)
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Substring_with_two_args_with_Index_of(bool async)
        {
            await base.Substring_with_two_args_with_Index_of(async);

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
WHERE [c].[Region] IS NULL OR ([c].[Region] LIKE N'')");
        }

        public override void IsNullOrEmpty_in_projection()
        {
            base.IsNullOrEmpty_in_projection();

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Id], CASE
    WHEN [c].[Region] IS NULL OR ([c].[Region] LIKE N'') THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Value]
FROM [Customers] AS [c]");
        }

        public override void IsNullOrEmpty_negated_in_projection()
        {
            base.IsNullOrEmpty_negated_in_projection();

            AssertSql(
                @"SELECT [c].[CustomerID] AS [Id], CASE
    WHEN NOT ([c].[Region] IS NULL OR ([c].[Region] LIKE N'')) THEN CAST(1 AS bit)
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
WHERE [c].[Region] IS NULL OR ([c].[Region] = N'')");
        }

        public override async Task IsNullOrWhiteSpace_in_predicate_on_non_nullable_column(bool async)
        {
            await base.IsNullOrWhiteSpace_in_predicate_on_non_nullable_column(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N''");
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
                @"@__arg_0='1996-07-04T00:00:00.0000000' (DbType = DateTime)

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

        public override Task Regex_IsMatch_MethodCall(bool async)
        {
            return AssertTranslationFailed(() => base.Regex_IsMatch_MethodCall(async));
        }

        public override Task Regex_IsMatch_MethodCall_constant_input(bool async)
        {
            return AssertTranslationFailed(() => base.Regex_IsMatch_MethodCall_constant_input(async));
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
