// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindCompiledQuerySqlServerTest : NorthwindCompiledQueryTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        public NorthwindCompiledQuerySqlServerTest(
            NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
            //fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void DbSet_query()
        {
            base.DbSet_query();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void DbSet_query_first()
        {
            base.DbSet_query_first();

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void Query_ending_with_include()
        {
            base.Query_ending_with_include();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID], [o].[OrderID]",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID], [o].[OrderID]");
        }

        public override void Untyped_context()
        {
            base.Untyped_context();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Query_with_single_parameter()
        {
            base.Query_with_single_parameter();

            AssertSql(
                @"@__customerID='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__customerID",
                //
                @"@__customerID='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__customerID");
        }

        public override void First_query_with_single_parameter()
        {
            base.First_query_with_single_parameter();

            AssertSql(
                @"@__customerID='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__customerID",
                //
                @"@__customerID='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__customerID");
        }

        public override void Query_with_two_parameters()
        {
            base.Query_with_two_parameters();

            AssertSql(
                @"@__customerID='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__customerID",
                //
                @"@__customerID='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__customerID");
        }

        public override void Query_with_three_parameters()
        {
            base.Query_with_three_parameters();

            AssertSql(
                @"@__customerID='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__customerID",
                //
                @"@__customerID='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__customerID");
        }

        public override void Query_with_contains()
        {
            base.Query_with_contains();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ANATR'");
        }

        public override void Query_with_closure()
        {
            base.Query_with_closure();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override void Compiled_query_when_does_not_end_in_query_operator()
        {
            base.Compiled_query_when_does_not_end_in_query_operator();

            AssertSql(
                @"@__customerID='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__customerID");
        }

        public override async Task Compiled_query_with_max_parameters()
        {
            await base.Compiled_query_with_max_parameters();

            AssertSql(
                @"@__s1='ALFKI' (Size = 5) (DbType = StringFixedLength)
@__s2='ANATR' (Size = 5) (DbType = StringFixedLength)
@__s3='ANTON' (Size = 5) (DbType = StringFixedLength)
@__s4='AROUT' (Size = 5) (DbType = StringFixedLength)
@__s5='BERGS' (Size = 5) (DbType = StringFixedLength)
@__s6='BLAUS' (Size = 5) (DbType = StringFixedLength)
@__s7='BLONP' (Size = 5) (DbType = StringFixedLength)
@__s8='BOLID' (Size = 5) (DbType = StringFixedLength)
@__s9='BONAP' (Size = 5) (DbType = StringFixedLength)
@__s10='BSBEV' (Size = 5) (DbType = StringFixedLength)
@__s11='CACTU' (Size = 5) (DbType = StringFixedLength)
@__s12='CENTC' (Size = 5) (DbType = StringFixedLength)
@__s13='CHOPS' (Size = 5) (DbType = StringFixedLength)
@__s14='CONSH' (Size = 5) (DbType = StringFixedLength)
@__s15='RANDM' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (((((((((((((([c].[CustomerID] = @__s1) OR ([c].[CustomerID] = @__s2)) OR ([c].[CustomerID] = @__s3)) OR ([c].[CustomerID] = @__s4)) OR ([c].[CustomerID] = @__s5)) OR ([c].[CustomerID] = @__s6)) OR ([c].[CustomerID] = @__s7)) OR ([c].[CustomerID] = @__s8)) OR ([c].[CustomerID] = @__s9)) OR ([c].[CustomerID] = @__s10)) OR ([c].[CustomerID] = @__s11)) OR ([c].[CustomerID] = @__s12)) OR ([c].[CustomerID] = @__s13)) OR ([c].[CustomerID] = @__s14)) OR ([c].[CustomerID] = @__s15)",
                //
                @"@__s1='ALFKI' (Size = 5) (DbType = StringFixedLength)
@__s2='ANATR' (Size = 5) (DbType = StringFixedLength)
@__s3='ANTON' (Size = 5) (DbType = StringFixedLength)
@__s4='AROUT' (Size = 5) (DbType = StringFixedLength)
@__s5='BERGS' (Size = 5) (DbType = StringFixedLength)
@__s6='BLAUS' (Size = 5) (DbType = StringFixedLength)
@__s7='BLONP' (Size = 5) (DbType = StringFixedLength)
@__s8='BOLID' (Size = 5) (DbType = StringFixedLength)
@__s9='BONAP' (Size = 5) (DbType = StringFixedLength)
@__s10='BSBEV' (Size = 5) (DbType = StringFixedLength)
@__s11='CACTU' (Size = 5) (DbType = StringFixedLength)
@__s12='CENTC' (Size = 5) (DbType = StringFixedLength)
@__s13='CHOPS' (Size = 5) (DbType = StringFixedLength)
@__s14='CONSH' (Size = 5) (DbType = StringFixedLength)
@__s15='RANDM' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE (((((((((((((([c].[CustomerID] = @__s1) OR ([c].[CustomerID] = @__s2)) OR ([c].[CustomerID] = @__s3)) OR ([c].[CustomerID] = @__s4)) OR ([c].[CustomerID] = @__s5)) OR ([c].[CustomerID] = @__s6)) OR ([c].[CustomerID] = @__s7)) OR ([c].[CustomerID] = @__s8)) OR ([c].[CustomerID] = @__s9)) OR ([c].[CustomerID] = @__s10)) OR ([c].[CustomerID] = @__s11)) OR ([c].[CustomerID] = @__s12)) OR ([c].[CustomerID] = @__s13)) OR ([c].[CustomerID] = @__s14)) OR ([c].[CustomerID] = @__s15)
ORDER BY [c].[CustomerID], [o].[OrderID]",
                //
                @"@__s1='ALFKI' (Size = 5) (DbType = StringFixedLength)
@__s2='ANATR' (Size = 5) (DbType = StringFixedLength)
@__s3='ANTON' (Size = 5) (DbType = StringFixedLength)
@__s4='AROUT' (Size = 5) (DbType = StringFixedLength)
@__s5='BERGS' (Size = 5) (DbType = StringFixedLength)
@__s6='BLAUS' (Size = 5) (DbType = StringFixedLength)
@__s7='BLONP' (Size = 5) (DbType = StringFixedLength)
@__s8='BOLID' (Size = 5) (DbType = StringFixedLength)
@__s9='BONAP' (Size = 5) (DbType = StringFixedLength)
@__s10='BSBEV' (Size = 5) (DbType = StringFixedLength)
@__s11='CACTU' (Size = 5) (DbType = StringFixedLength)
@__s12='CENTC' (Size = 5) (DbType = StringFixedLength)
@__s13='CHOPS' (Size = 5) (DbType = StringFixedLength)
@__s14='CONSH' (Size = 5) (DbType = StringFixedLength)
@__s15='RANDM' (Size = 5) (DbType = StringFixedLength)

SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE (((((((((((((([c].[CustomerID] = @__s1) OR ([c].[CustomerID] = @__s2)) OR ([c].[CustomerID] = @__s3)) OR ([c].[CustomerID] = @__s4)) OR ([c].[CustomerID] = @__s5)) OR ([c].[CustomerID] = @__s6)) OR ([c].[CustomerID] = @__s7)) OR ([c].[CustomerID] = @__s8)) OR ([c].[CustomerID] = @__s9)) OR ([c].[CustomerID] = @__s10)) OR ([c].[CustomerID] = @__s11)) OR ([c].[CustomerID] = @__s12)) OR ([c].[CustomerID] = @__s13)) OR ([c].[CustomerID] = @__s14)) OR ([c].[CustomerID] = @__s15)",
                //
                @"@__s1='ALFKI' (Size = 5) (DbType = StringFixedLength)
@__s2='ANATR' (Size = 5) (DbType = StringFixedLength)
@__s3='ANTON' (Size = 5) (DbType = StringFixedLength)
@__s4='AROUT' (Size = 5) (DbType = StringFixedLength)
@__s5='BERGS' (Size = 5) (DbType = StringFixedLength)
@__s6='BLAUS' (Size = 5) (DbType = StringFixedLength)
@__s7='BLONP' (Size = 5) (DbType = StringFixedLength)
@__s8='BOLID' (Size = 5) (DbType = StringFixedLength)
@__s9='BONAP' (Size = 5) (DbType = StringFixedLength)
@__s10='BSBEV' (Size = 5) (DbType = StringFixedLength)
@__s11='CACTU' (Size = 5) (DbType = StringFixedLength)
@__s12='CENTC' (Size = 5) (DbType = StringFixedLength)
@__s13='CHOPS' (Size = 5) (DbType = StringFixedLength)
@__s14='CONSH' (Size = 5) (DbType = StringFixedLength)
@__s15='RANDM' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (((((((((((((([c].[CustomerID] = @__s1) OR ([c].[CustomerID] = @__s2)) OR ([c].[CustomerID] = @__s3)) OR ([c].[CustomerID] = @__s4)) OR ([c].[CustomerID] = @__s5)) OR ([c].[CustomerID] = @__s6)) OR ([c].[CustomerID] = @__s7)) OR ([c].[CustomerID] = @__s8)) OR ([c].[CustomerID] = @__s9)) OR ([c].[CustomerID] = @__s10)) OR ([c].[CustomerID] = @__s11)) OR ([c].[CustomerID] = @__s12)) OR ([c].[CustomerID] = @__s13)) OR ([c].[CustomerID] = @__s14)) OR ([c].[CustomerID] = @__s15)",
                //
                @"@__s1='ALFKI' (Size = 5) (DbType = StringFixedLength)
@__s2='ANATR' (Size = 5) (DbType = StringFixedLength)
@__s3='ANTON' (Size = 5) (DbType = StringFixedLength)
@__s4='AROUT' (Size = 5) (DbType = StringFixedLength)
@__s5='BERGS' (Size = 5) (DbType = StringFixedLength)
@__s6='BLAUS' (Size = 5) (DbType = StringFixedLength)
@__s7='BLONP' (Size = 5) (DbType = StringFixedLength)
@__s8='BOLID' (Size = 5) (DbType = StringFixedLength)
@__s9='BONAP' (Size = 5) (DbType = StringFixedLength)
@__s10='BSBEV' (Size = 5) (DbType = StringFixedLength)
@__s11='CACTU' (Size = 5) (DbType = StringFixedLength)
@__s12='CENTC' (Size = 5) (DbType = StringFixedLength)
@__s13='CHOPS' (Size = 5) (DbType = StringFixedLength)
@__s14='CONSH' (Size = 5) (DbType = StringFixedLength)
@__s15='RANDM' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE (((((((((((((([c].[CustomerID] = @__s1) OR ([c].[CustomerID] = @__s2)) OR ([c].[CustomerID] = @__s3)) OR ([c].[CustomerID] = @__s4)) OR ([c].[CustomerID] = @__s5)) OR ([c].[CustomerID] = @__s6)) OR ([c].[CustomerID] = @__s7)) OR ([c].[CustomerID] = @__s8)) OR ([c].[CustomerID] = @__s9)) OR ([c].[CustomerID] = @__s10)) OR ([c].[CustomerID] = @__s11)) OR ([c].[CustomerID] = @__s12)) OR ([c].[CustomerID] = @__s13)) OR ([c].[CustomerID] = @__s14)) OR ([c].[CustomerID] = @__s15)
ORDER BY [c].[CustomerID], [o].[OrderID]",
                //
                @"@__s1='ALFKI' (Size = 5) (DbType = StringFixedLength)
@__s2='ANATR' (Size = 5) (DbType = StringFixedLength)
@__s3='ANTON' (Size = 5) (DbType = StringFixedLength)
@__s4='AROUT' (Size = 5) (DbType = StringFixedLength)
@__s5='BERGS' (Size = 5) (DbType = StringFixedLength)
@__s6='BLAUS' (Size = 5) (DbType = StringFixedLength)
@__s7='BLONP' (Size = 5) (DbType = StringFixedLength)
@__s8='BOLID' (Size = 5) (DbType = StringFixedLength)
@__s9='BONAP' (Size = 5) (DbType = StringFixedLength)
@__s10='BSBEV' (Size = 5) (DbType = StringFixedLength)
@__s11='CACTU' (Size = 5) (DbType = StringFixedLength)
@__s12='CENTC' (Size = 5) (DbType = StringFixedLength)
@__s13='CHOPS' (Size = 5) (DbType = StringFixedLength)
@__s14='CONSH' (Size = 5) (DbType = StringFixedLength)
@__s15='RANDM' (Size = 5) (DbType = StringFixedLength)

SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE (((((((((((((([c].[CustomerID] = @__s1) OR ([c].[CustomerID] = @__s2)) OR ([c].[CustomerID] = @__s3)) OR ([c].[CustomerID] = @__s4)) OR ([c].[CustomerID] = @__s5)) OR ([c].[CustomerID] = @__s6)) OR ([c].[CustomerID] = @__s7)) OR ([c].[CustomerID] = @__s8)) OR ([c].[CustomerID] = @__s9)) OR ([c].[CustomerID] = @__s10)) OR ([c].[CustomerID] = @__s11)) OR ([c].[CustomerID] = @__s12)) OR ([c].[CustomerID] = @__s13)) OR ([c].[CustomerID] = @__s14)) OR ([c].[CustomerID] = @__s15)",
                //
                @"@__s1='ALFKI' (Size = 5) (DbType = StringFixedLength)
@__s2='ANATR' (Size = 5) (DbType = StringFixedLength)
@__s3='ANTON' (Size = 5) (DbType = StringFixedLength)
@__s4='AROUT' (Size = 5) (DbType = StringFixedLength)
@__s5='BERGS' (Size = 5) (DbType = StringFixedLength)
@__s6='BLAUS' (Size = 5) (DbType = StringFixedLength)
@__s7='BLONP' (Size = 5) (DbType = StringFixedLength)
@__s8='BOLID' (Size = 5) (DbType = StringFixedLength)
@__s9='BONAP' (Size = 5) (DbType = StringFixedLength)
@__s10='BSBEV' (Size = 5) (DbType = StringFixedLength)
@__s11='CACTU' (Size = 5) (DbType = StringFixedLength)
@__s12='CENTC' (Size = 5) (DbType = StringFixedLength)
@__s13='CHOPS' (Size = 5) (DbType = StringFixedLength)
@__s14='CONSH' (Size = 5) (DbType = StringFixedLength)

SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE ((((((((((((([c].[CustomerID] = @__s1) OR ([c].[CustomerID] = @__s2)) OR ([c].[CustomerID] = @__s3)) OR ([c].[CustomerID] = @__s4)) OR ([c].[CustomerID] = @__s5)) OR ([c].[CustomerID] = @__s6)) OR ([c].[CustomerID] = @__s7)) OR ([c].[CustomerID] = @__s8)) OR ([c].[CustomerID] = @__s9)) OR ([c].[CustomerID] = @__s10)) OR ([c].[CustomerID] = @__s11)) OR ([c].[CustomerID] = @__s12)) OR ([c].[CustomerID] = @__s13)) OR ([c].[CustomerID] = @__s14)");
        }

        public override void MakeBinary_does_not_throw_for_unsupported_operator()
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("DbSet<Customer>()    .Where(c => c.CustomerID == (string)(__parameters[0]))"),
                Assert.Throws<InvalidOperationException>(
                    () => base.MakeBinary_does_not_throw_for_unsupported_operator()).Message.Replace("\r", "").Replace("\n", ""));
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
