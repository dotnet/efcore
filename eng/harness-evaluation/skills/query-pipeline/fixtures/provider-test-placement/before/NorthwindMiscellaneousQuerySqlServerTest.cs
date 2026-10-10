using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindMiscellaneousQuerySqlServerTest : NorthwindMiscellaneousQueryRelationalTestBase<
    NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
{
    public NorthwindMiscellaneousQuerySqlServerTest(
        NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Collection_navigation_equal_to_null_for_subquery(bool async)
    {
        await base.Collection_navigation_equal_to_null_for_subquery(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])
""");
    }

    public override async Task Dependent_to_principal_navigation_equal_to_null_for_subquery(bool async)
    {
        await base.Dependent_to_principal_navigation_equal_to_null_for_subquery(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT TOP(1) [c0].[CustomerID]
    FROM [Orders] AS [o]
    LEFT JOIN [Customers] AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]) IS NULL
""");
    }

    public override async Task Inner_parameter_in_nested_lambdas_gets_preserved(bool async)
    {
        await base.Inner_parameter_in_nested_lambdas_gets_preserved(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) > 0
""");
    }

    public override async Task Navigation_inside_interpolated_string_is_expanded(bool async)
    {
        await base.Navigation_inside_interpolated_string_is_expanded(async);

        AssertSql(
            """
SELECT [c].[City]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}