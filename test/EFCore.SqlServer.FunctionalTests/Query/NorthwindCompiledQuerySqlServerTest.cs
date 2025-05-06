// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindCompiledQuerySqlServerTest : NorthwindCompiledQueryTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
{
    public NorthwindCompiledQuerySqlServerTest(
        NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
        fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override void DbSet_query()
    {
        base.DbSet_query();

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override void DbSet_query_first()
    {
        base.DbSet_query_first();

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override void Query_ending_with_include()
    {
        base.Query_ending_with_include();

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID]
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID]
""");
    }

    public override void Untyped_context()
    {
        base.Untyped_context();

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override void Query_with_single_parameter()
    {
        base.Query_with_single_parameter();

        AssertSql(
            """
@customerID='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @customerID
""",
            //
            """
@customerID='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @customerID
""");
    }

    public override void First_query_with_single_parameter()
    {
        base.First_query_with_single_parameter();

        AssertSql(
            """
@customerID='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @customerID
""",
            //
            """
@customerID='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @customerID
""");
    }

    public override void Query_with_two_parameters()
    {
        base.Query_with_two_parameters();

        AssertSql(
            """
@customerID='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @customerID
""",
            //
            """
@customerID='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @customerID
""");
    }

    public override void Query_with_three_parameters()
    {
        base.Query_with_three_parameters();

        AssertSql(
            """
@customerID='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @customerID
""",
            //
            """
@customerID='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @customerID
""");
    }

    public override void Query_with_contains()
    {
        base.Query_with_contains();

        AssertSql(
            """
@args1='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @args1
""",
            //
            """
@args1='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @args1
""");
    }

    public override void Query_with_closure()
    {
        base.Query_with_closure();

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override void Compiled_query_when_does_not_end_in_query_operator()
    {
        base.Compiled_query_when_does_not_end_in_query_operator();

        AssertSql(
            """
@customerID='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @customerID
""");
    }

    public override async Task Compiled_query_with_max_parameters()
    {
        await base.Compiled_query_with_max_parameters();

        AssertSql(
            """
@s1='ALFKI' (Size = 5) (DbType = StringFixedLength)
@s2='ANATR' (Size = 5) (DbType = StringFixedLength)
@s3='ANTON' (Size = 5) (DbType = StringFixedLength)
@s4='AROUT' (Size = 5) (DbType = StringFixedLength)
@s5='BERGS' (Size = 5) (DbType = StringFixedLength)
@s6='BLAUS' (Size = 5) (DbType = StringFixedLength)
@s7='BLONP' (Size = 5) (DbType = StringFixedLength)
@s8='BOLID' (Size = 5) (DbType = StringFixedLength)
@s9='BONAP' (Size = 5) (DbType = StringFixedLength)
@s10='BSBEV' (Size = 5) (DbType = StringFixedLength)
@s11='CACTU' (Size = 5) (DbType = StringFixedLength)
@s12='CENTC' (Size = 5) (DbType = StringFixedLength)
@s13='CHOPS' (Size = 5) (DbType = StringFixedLength)
@s14='CONSH' (Size = 5) (DbType = StringFixedLength)
@s15='RANDM' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @s1 OR [c].[CustomerID] = @s2 OR [c].[CustomerID] = @s3 OR [c].[CustomerID] = @s4 OR [c].[CustomerID] = @s5 OR [c].[CustomerID] = @s6 OR [c].[CustomerID] = @s7 OR [c].[CustomerID] = @s8 OR [c].[CustomerID] = @s9 OR [c].[CustomerID] = @s10 OR [c].[CustomerID] = @s11 OR [c].[CustomerID] = @s12 OR [c].[CustomerID] = @s13 OR [c].[CustomerID] = @s14 OR [c].[CustomerID] = @s15
""",
            //
            """
@s1='ALFKI' (Size = 5) (DbType = StringFixedLength)
@s2='ANATR' (Size = 5) (DbType = StringFixedLength)
@s3='ANTON' (Size = 5) (DbType = StringFixedLength)
@s4='AROUT' (Size = 5) (DbType = StringFixedLength)
@s5='BERGS' (Size = 5) (DbType = StringFixedLength)
@s6='BLAUS' (Size = 5) (DbType = StringFixedLength)
@s7='BLONP' (Size = 5) (DbType = StringFixedLength)
@s8='BOLID' (Size = 5) (DbType = StringFixedLength)
@s9='BONAP' (Size = 5) (DbType = StringFixedLength)
@s10='BSBEV' (Size = 5) (DbType = StringFixedLength)
@s11='CACTU' (Size = 5) (DbType = StringFixedLength)
@s12='CENTC' (Size = 5) (DbType = StringFixedLength)
@s13='CHOPS' (Size = 5) (DbType = StringFixedLength)
@s14='CONSH' (Size = 5) (DbType = StringFixedLength)
@s15='RANDM' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = @s1 OR [c].[CustomerID] = @s2 OR [c].[CustomerID] = @s3 OR [c].[CustomerID] = @s4 OR [c].[CustomerID] = @s5 OR [c].[CustomerID] = @s6 OR [c].[CustomerID] = @s7 OR [c].[CustomerID] = @s8 OR [c].[CustomerID] = @s9 OR [c].[CustomerID] = @s10 OR [c].[CustomerID] = @s11 OR [c].[CustomerID] = @s12 OR [c].[CustomerID] = @s13 OR [c].[CustomerID] = @s14 OR [c].[CustomerID] = @s15
ORDER BY [c].[CustomerID]
""",
            //
            """
@s1='ALFKI' (Size = 5) (DbType = StringFixedLength)
@s2='ANATR' (Size = 5) (DbType = StringFixedLength)
@s3='ANTON' (Size = 5) (DbType = StringFixedLength)
@s4='AROUT' (Size = 5) (DbType = StringFixedLength)
@s5='BERGS' (Size = 5) (DbType = StringFixedLength)
@s6='BLAUS' (Size = 5) (DbType = StringFixedLength)
@s7='BLONP' (Size = 5) (DbType = StringFixedLength)
@s8='BOLID' (Size = 5) (DbType = StringFixedLength)
@s9='BONAP' (Size = 5) (DbType = StringFixedLength)
@s10='BSBEV' (Size = 5) (DbType = StringFixedLength)
@s11='CACTU' (Size = 5) (DbType = StringFixedLength)
@s12='CENTC' (Size = 5) (DbType = StringFixedLength)
@s13='CHOPS' (Size = 5) (DbType = StringFixedLength)
@s14='CONSH' (Size = 5) (DbType = StringFixedLength)
@s15='RANDM' (Size = 5) (DbType = StringFixedLength)

SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @s1 OR [c].[CustomerID] = @s2 OR [c].[CustomerID] = @s3 OR [c].[CustomerID] = @s4 OR [c].[CustomerID] = @s5 OR [c].[CustomerID] = @s6 OR [c].[CustomerID] = @s7 OR [c].[CustomerID] = @s8 OR [c].[CustomerID] = @s9 OR [c].[CustomerID] = @s10 OR [c].[CustomerID] = @s11 OR [c].[CustomerID] = @s12 OR [c].[CustomerID] = @s13 OR [c].[CustomerID] = @s14 OR [c].[CustomerID] = @s15
""",
            //
            """
@s1='ALFKI' (Size = 5) (DbType = StringFixedLength)
@s2='ANATR' (Size = 5) (DbType = StringFixedLength)
@s3='ANTON' (Size = 5) (DbType = StringFixedLength)
@s4='AROUT' (Size = 5) (DbType = StringFixedLength)
@s5='BERGS' (Size = 5) (DbType = StringFixedLength)
@s6='BLAUS' (Size = 5) (DbType = StringFixedLength)
@s7='BLONP' (Size = 5) (DbType = StringFixedLength)
@s8='BOLID' (Size = 5) (DbType = StringFixedLength)
@s9='BONAP' (Size = 5) (DbType = StringFixedLength)
@s10='BSBEV' (Size = 5) (DbType = StringFixedLength)
@s11='CACTU' (Size = 5) (DbType = StringFixedLength)
@s12='CENTC' (Size = 5) (DbType = StringFixedLength)
@s13='CHOPS' (Size = 5) (DbType = StringFixedLength)
@s14='CONSH' (Size = 5) (DbType = StringFixedLength)
@s15='RANDM' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @s1 OR [c].[CustomerID] = @s2 OR [c].[CustomerID] = @s3 OR [c].[CustomerID] = @s4 OR [c].[CustomerID] = @s5 OR [c].[CustomerID] = @s6 OR [c].[CustomerID] = @s7 OR [c].[CustomerID] = @s8 OR [c].[CustomerID] = @s9 OR [c].[CustomerID] = @s10 OR [c].[CustomerID] = @s11 OR [c].[CustomerID] = @s12 OR [c].[CustomerID] = @s13 OR [c].[CustomerID] = @s14 OR [c].[CustomerID] = @s15
""",
            //
            """
@s1='ALFKI' (Size = 5) (DbType = StringFixedLength)
@s2='ANATR' (Size = 5) (DbType = StringFixedLength)
@s3='ANTON' (Size = 5) (DbType = StringFixedLength)
@s4='AROUT' (Size = 5) (DbType = StringFixedLength)
@s5='BERGS' (Size = 5) (DbType = StringFixedLength)
@s6='BLAUS' (Size = 5) (DbType = StringFixedLength)
@s7='BLONP' (Size = 5) (DbType = StringFixedLength)
@s8='BOLID' (Size = 5) (DbType = StringFixedLength)
@s9='BONAP' (Size = 5) (DbType = StringFixedLength)
@s10='BSBEV' (Size = 5) (DbType = StringFixedLength)
@s11='CACTU' (Size = 5) (DbType = StringFixedLength)
@s12='CENTC' (Size = 5) (DbType = StringFixedLength)
@s13='CHOPS' (Size = 5) (DbType = StringFixedLength)
@s14='CONSH' (Size = 5) (DbType = StringFixedLength)
@s15='RANDM' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = @s1 OR [c].[CustomerID] = @s2 OR [c].[CustomerID] = @s3 OR [c].[CustomerID] = @s4 OR [c].[CustomerID] = @s5 OR [c].[CustomerID] = @s6 OR [c].[CustomerID] = @s7 OR [c].[CustomerID] = @s8 OR [c].[CustomerID] = @s9 OR [c].[CustomerID] = @s10 OR [c].[CustomerID] = @s11 OR [c].[CustomerID] = @s12 OR [c].[CustomerID] = @s13 OR [c].[CustomerID] = @s14 OR [c].[CustomerID] = @s15
ORDER BY [c].[CustomerID]
""",
            //
            """
@s1='ALFKI' (Size = 5) (DbType = StringFixedLength)
@s2='ANATR' (Size = 5) (DbType = StringFixedLength)
@s3='ANTON' (Size = 5) (DbType = StringFixedLength)
@s4='AROUT' (Size = 5) (DbType = StringFixedLength)
@s5='BERGS' (Size = 5) (DbType = StringFixedLength)
@s6='BLAUS' (Size = 5) (DbType = StringFixedLength)
@s7='BLONP' (Size = 5) (DbType = StringFixedLength)
@s8='BOLID' (Size = 5) (DbType = StringFixedLength)
@s9='BONAP' (Size = 5) (DbType = StringFixedLength)
@s10='BSBEV' (Size = 5) (DbType = StringFixedLength)
@s11='CACTU' (Size = 5) (DbType = StringFixedLength)
@s12='CENTC' (Size = 5) (DbType = StringFixedLength)
@s13='CHOPS' (Size = 5) (DbType = StringFixedLength)
@s14='CONSH' (Size = 5) (DbType = StringFixedLength)
@s15='RANDM' (Size = 5) (DbType = StringFixedLength)

SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @s1 OR [c].[CustomerID] = @s2 OR [c].[CustomerID] = @s3 OR [c].[CustomerID] = @s4 OR [c].[CustomerID] = @s5 OR [c].[CustomerID] = @s6 OR [c].[CustomerID] = @s7 OR [c].[CustomerID] = @s8 OR [c].[CustomerID] = @s9 OR [c].[CustomerID] = @s10 OR [c].[CustomerID] = @s11 OR [c].[CustomerID] = @s12 OR [c].[CustomerID] = @s13 OR [c].[CustomerID] = @s14 OR [c].[CustomerID] = @s15
""",
            //
            """
@s1='ALFKI' (Size = 5) (DbType = StringFixedLength)
@s2='ANATR' (Size = 5) (DbType = StringFixedLength)
@s3='ANTON' (Size = 5) (DbType = StringFixedLength)
@s4='AROUT' (Size = 5) (DbType = StringFixedLength)
@s5='BERGS' (Size = 5) (DbType = StringFixedLength)
@s6='BLAUS' (Size = 5) (DbType = StringFixedLength)
@s7='BLONP' (Size = 5) (DbType = StringFixedLength)
@s8='BOLID' (Size = 5) (DbType = StringFixedLength)
@s9='BONAP' (Size = 5) (DbType = StringFixedLength)
@s10='BSBEV' (Size = 5) (DbType = StringFixedLength)
@s11='CACTU' (Size = 5) (DbType = StringFixedLength)
@s12='CENTC' (Size = 5) (DbType = StringFixedLength)
@s13='CHOPS' (Size = 5) (DbType = StringFixedLength)
@s14='CONSH' (Size = 5) (DbType = StringFixedLength)

SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @s1 OR [c].[CustomerID] = @s2 OR [c].[CustomerID] = @s3 OR [c].[CustomerID] = @s4 OR [c].[CustomerID] = @s5 OR [c].[CustomerID] = @s6 OR [c].[CustomerID] = @s7 OR [c].[CustomerID] = @s8 OR [c].[CustomerID] = @s9 OR [c].[CustomerID] = @s10 OR [c].[CustomerID] = @s11 OR [c].[CustomerID] = @s12 OR [c].[CustomerID] = @s13 OR [c].[CustomerID] = @s14
""");
    }

    public override void Query_with_array_parameter()
    {
        base.Query_with_array_parameter();

        AssertSql(
            """
@args='["ALFKI"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = JSON_VALUE(@args, '$[0]')
""",
            //
            """
@args='["ANATR"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = JSON_VALUE(@args, '$[0]')
""");
    }

    public override async Task Query_with_array_parameter_async()
    {
        await base.Query_with_array_parameter_async();

        AssertSql(
            """
@args='["ALFKI"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = JSON_VALUE(@args, '$[0]')
""",
            //
            """
@args='["ANATR"]' (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = JSON_VALUE(@args, '$[0]')
""");
    }

    public override void Multiple_queries()
    {
        base.Multiple_queries();

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""",
            //
            """
SELECT TOP(1) [o].[CustomerID]
FROM [Orders] AS [o]
ORDER BY [o].[CustomerID]
""",
            //
            """
SELECT TOP(1) [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""",
            //
            """
SELECT TOP(1) [o].[CustomerID]
FROM [Orders] AS [o]
ORDER BY [o].[CustomerID]
""");
    }

    public override void Compiled_query_when_using_member_on_context()
    {
        base.Compiled_query_when_using_member_on_context();

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A%'
""");
    }

    public override async Task First_query_with_cancellation_async()
    {
        await base.First_query_with_cancellation_async();

        AssertSql(
            """
@customerID='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @customerID
""",
            //
            """
@customerID='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @customerID
""");
    }

    public override async Task DbSet_query_first_async()
    {
        await base.DbSet_query_first_async();

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task First_query_with_single_parameter_async()
    {
        await base.First_query_with_single_parameter_async();

        AssertSql(
            """
@customerID='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @customerID
""",
            //
            """
@customerID='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @customerID
""");
    }

    public override async Task Keyless_query_first_async()
    {
        await base.Keyless_query_first_async();

        AssertSql(
            """
SELECT TOP(1) [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]
) AS [m]
ORDER BY [m].[CompanyName]
""");
    }

    public override async Task Query_with_closure_async_null()
    {
        await base.Query_with_closure_async_null();

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1
""");
    }

    public override async Task Query_with_three_parameters_async()
    {
        await base.Query_with_three_parameters_async();

        AssertSql(
            """
@customerID='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @customerID
""",
            //
            """
@customerID='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @customerID
""");
    }

    public override async Task Query_with_two_parameters_async()
    {
        await base.Query_with_two_parameters_async();

        AssertSql(
            """
@customerID='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @customerID
""",
            //
            """
@customerID='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @customerID
""");
    }

    public override async Task Keyless_query_async()
    {
        await base.Keyless_query_async();

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]
""");
    }

    public override async Task Query_with_single_parameter_async()
    {
        await base.Query_with_single_parameter_async();

        AssertSql(
            """
@customerID='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @customerID
""",
            //
            """
@customerID='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @customerID
""");
    }

    public override void Keyless_query_first()
    {
        base.Keyless_query_first();

        AssertSql(
            """
SELECT TOP(1) [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]
) AS [m]
ORDER BY [m].[CompanyName]
""");
    }

    public override void Query_with_closure_null()
    {
        base.Query_with_closure_null();

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1
""");
    }

    public override async Task Query_with_closure_async()
    {
        await base.Query_with_closure_async();

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Untyped_context_async()
    {
        await base.Untyped_context_async();

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override async Task DbSet_query_async()
    {
        await base.DbSet_query_async();

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override void Keyless_query()
    {
        base.Keyless_query();

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]
""");
    }

    public override void Query_with_single_parameter_with_include()
    {
        base.Query_with_single_parameter_with_include();

        AssertSql(
            """
@customerID='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = @customerID
ORDER BY [c].[CustomerID]
""",
            //
            """
@customerID='ANATR' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = @customerID
ORDER BY [c].[CustomerID]
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
