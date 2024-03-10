// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindChangeTrackingQuerySqlServerTest : NorthwindChangeTrackingQueryTestBase<
    NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
{
    public NorthwindChangeTrackingQuerySqlServerTest(NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override void Entity_reverts_when_state_set_to_unchanged()
    {
        base.Entity_reverts_when_state_set_to_unchanged();

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override void Entity_does_not_revert_when_attached_on_DbSet()
    {
        base.Entity_does_not_revert_when_attached_on_DbSet();

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override void AsTracking_switches_tracking_on_when_off_in_options()
    {
        base.AsTracking_switches_tracking_on_when_off_in_options();

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
""");
    }

    public override void Can_disable_and_reenable_query_result_tracking_query_caching_using_options()
    {
        base.Can_disable_and_reenable_query_result_tracking_query_caching_using_options();

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
""",
            //
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
""");
    }

    public override void Can_disable_and_reenable_query_result_tracking()
    {
        base.Can_disable_and_reenable_query_result_tracking();

        AssertSql(
            """
@__p_0='1'

SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID]
""",
            //
            """
@__p_0='1'

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY
""",
            //
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID]
""");
    }

    public override void Entity_range_does_not_revert_when_attached_dbSet()
    {
        base.Entity_range_does_not_revert_when_attached_dbSet();

        AssertSql(
            """
@__p_0='2'

SELECT TOP(1) [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0]
ORDER BY [c0].[CustomerID]
""",
            //
            """
@__p_0='2'
@__p_1='1'

SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0]
ORDER BY [c0].[CustomerID]
OFFSET @__p_1 ROWS FETCH NEXT 1 ROWS ONLY
""",
            //
            """
@__p_0='2'

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override void Precedence_of_tracking_modifiers5()
    {
        base.Precedence_of_tracking_modifiers5();

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override void Precedence_of_tracking_modifiers2()
    {
        base.Precedence_of_tracking_modifiers2();

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
""");
    }

    public override void Can_disable_and_reenable_query_result_tracking_query_caching()
    {
        base.Can_disable_and_reenable_query_result_tracking_query_caching();

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
""",
            //
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
""");
    }

    public override void Entity_range_does_not_revert_when_attached_dbContext()
    {
        base.Entity_range_does_not_revert_when_attached_dbContext();

        AssertSql(
            """
@__p_0='2'

SELECT TOP(1) [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0]
ORDER BY [c0].[CustomerID]
""",
            //
            """
@__p_0='2'
@__p_1='1'

SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [c0]
ORDER BY [c0].[CustomerID]
OFFSET @__p_1 ROWS FETCH NEXT 1 ROWS ONLY
""",
            //
            """
@__p_0='2'

SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
""");
    }

    public override void Precedence_of_tracking_modifiers()
    {
        base.Precedence_of_tracking_modifiers();

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
""");
    }

    public override void Precedence_of_tracking_modifiers3()
    {
        base.Precedence_of_tracking_modifiers3();

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    public override void Can_disable_and_reenable_query_result_tracking_starting_with_NoTracking()
    {
        base.Can_disable_and_reenable_query_result_tracking_starting_with_NoTracking();

        AssertSql(
            """
@__p_0='1'

SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID]
""",
            //
            """
@__p_0='1'

SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
ORDER BY [e].[EmployeeID]
OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY
""");
    }

    public override void Entity_does_not_revert_when_attached_on_DbContext()
    {
        base.Entity_does_not_revert_when_attached_on_DbContext();

        AssertSql(
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override void Can_disable_and_reenable_query_result_tracking_query_caching_single_context()
    {
        base.Can_disable_and_reenable_query_result_tracking_query_caching_single_context();

        AssertSql(
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
""",
            //
            """
SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
""");
    }

    public override void Multiple_entities_can_revert()
    {
        base.Multiple_entities_can_revert();

        AssertSql(
            """
SELECT [c].[PostalCode]
FROM [Customers] AS [c]
""",
            //
            """
SELECT [c].[Region]
FROM [Customers] AS [c]
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""",
            //
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""",
            //
            """
SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
""",
            //
            """
SELECT [c].[PostalCode]
FROM [Customers] AS [c]
""",
            //
            """
SELECT [c].[Region]
FROM [Customers] AS [c]
""");
    }

    public override void Precedence_of_tracking_modifiers4()
    {
        base.Precedence_of_tracking_modifiers4();

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override NorthwindContext CreateNoTrackingContext()
        => new NorthwindSqlServerContext(
            new DbContextOptionsBuilder(Fixture.CreateOptions())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options);
}
