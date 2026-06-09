' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.

Imports Microsoft.EntityFrameworkCore.Query
Imports Microsoft.EntityFrameworkCore.TestModels.Northwind
Imports System.Threading.Tasks
Imports Microsoft.EntityFrameworkCore.TestUtilities
Imports Xunit

<ConditionalClass(GetType(SqlServerTestEnvironment), NameOf(SqlServerTestEnvironment.SqlServerAvailable))>
Partial Public Class NorthwindQueryVisualBasicTest
    Inherits QueryTestBase(Of NorthwindVBQuerySqlServerFixture(Of NoopModelCustomizer))

    Public Sub New(fixture As NorthwindVBQuerySqlServerFixture(Of NoopModelCustomizer))
        MyBase.New(fixture)

        fixture.TestSqlLoggerFactory.Clear()
    End Sub

    <Theory>
    <MemberData(NameOf(IsAsyncData))>
    Public Async Function CompareString_Equals_Binary(async As Boolean) As Task
        Await AssertQuery(
            async,
            Function(ss) ss.Set(Of Customer).Where(Function(c) c.CustomerID = "ALFKI"))

        AssertSql(
            "SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'")
    End Function

    <Theory>
    <MemberData(NameOf(IsAsyncData))>
    Public Async Function CompareString_LessThanOrEqual_Binary(async As Boolean) As Task
        Await AssertQuery(
            async,
            Function(ss) ss.Set(Of Customer).Where(Function(c) c.CustomerID <= "ALFKI"))

        AssertSql(
            "SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] <= N'ALFKI'")
    End Function

    <Theory>
    <MemberData(NameOf(IsAsyncData))>
    Public Async Function AddChecked(async As Boolean) As Task
        Await AssertQuery(
            async,
            Function(ss) ss.Set(Of Product).Where(Function(p) p.UnitsInStock + 1 = 102))

        AssertSql(
            "SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[UnitsInStock] + CAST(1 AS smallint) = CAST(102 AS smallint)")
    End Function

    <Theory>
    <MemberData(NameOf(IsAsyncData))>
    Public Async Function SubtractChecked(async As Boolean) As Task
        Await AssertQuery(
            async,
            Function(ss) ss.Set(Of Product).Where(Function(p) p.UnitsInStock - 1 = 100))

        AssertSql(
            "SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[UnitsInStock] - CAST(1 AS smallint) = CAST(100 AS smallint)")
    End Function

    <Theory>
    <MemberData(NameOf(IsAsyncData))>
    Public Async Function MultiplyChecked(async As Boolean) As Task
        Await AssertQuery(
            async,
            Function(ss) ss.Set(Of Product).Where(Function(p) p.UnitsInStock * 1 = 101))

        AssertSql(
            "SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[UnitsInStock] * CAST(1 AS smallint) = CAST(101 AS smallint)")
    End Function

    <Theory>
    <MemberData(NameOf(IsAsyncData))>
    Public Async Function Parameter_name_gets_sanitized(async As Boolean) As Task
        Dim units = 101
        Await AssertQuery(
            async,
            Function(ss) ss.Set(Of Product).Where(Function(p) p.UnitsInStock = units))

        AssertSql(
            "@units='101'

SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE [p].[UnitsInStock] = @units")
    End Function

    Private Sub AssertSql(ParamArray expected As String())
        Fixture.TestSqlLoggerFactory.AssertBaseline(expected)
    End Sub
End Class
