// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public class ComplexTypeBulkUpdatesSqlServerTest(
    ComplexTypeBulkUpdatesSqlServerTest.ComplexTypeBulkUpdatesSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper) : ComplexTypeBulkUpdatesRelationalTestBase<
    ComplexTypeBulkUpdatesSqlServerTest.ComplexTypeBulkUpdatesSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Delete_entity_type_with_complex_type(bool async)
    {
        await base.Delete_entity_type_with_complex_type(async);

        AssertSql(
            """
DELETE FROM [c]
FROM [Customer] AS [c]
WHERE [c].[Name] = N'Monty Elias'
""");
    }

    public override async Task Delete_complex_type(bool async)
    {
        await base.Delete_complex_type(async);

        AssertSql();
    }

    public override async Task Update_property_inside_complex_type(bool async)
    {
        await base.Update_property_inside_complex_type(async);

        AssertExecuteUpdateSql(
            """
@p='12345'

UPDATE [c]
SET [c].[ShippingAddress_ZipCode] = @p
FROM [Customer] AS [c]
WHERE [c].[ShippingAddress_ZipCode] = 7728
""");
    }

    public override async Task Update_property_inside_nested_complex_type(bool async)
    {
        await base.Update_property_inside_nested_complex_type(async);

        AssertExecuteUpdateSql(
            """
@p='United States Modified' (Size = 4000)

UPDATE [c]
SET [c].[ShippingAddress_Country_FullName] = @p
FROM [Customer] AS [c]
WHERE [c].[ShippingAddress_Country_Code] = N'US'
""");
    }

    public override async Task Update_multiple_properties_inside_multiple_complex_types_and_on_entity_type(bool async)
    {
        await base.Update_multiple_properties_inside_multiple_complex_types_and_on_entity_type(async);

        AssertExecuteUpdateSql(
            """
@p='54321'

UPDATE [c]
SET [c].[Name] = [c].[Name] + N'Modified',
    [c].[ShippingAddress_ZipCode] = [c].[BillingAddress_ZipCode],
    [c].[BillingAddress_ZipCode] = @p
FROM [Customer] AS [c]
WHERE [c].[ShippingAddress_ZipCode] = 7728
""");
    }

    public override async Task Update_projected_complex_type(bool async)
    {
        await base.Update_projected_complex_type(async);

        AssertExecuteUpdateSql(
            """
@p='12345'

UPDATE [c]
SET [c].[ShippingAddress_ZipCode] = @p
FROM [Customer] AS [c]
""");
    }

    public override async Task Update_multiple_projected_complex_types_via_anonymous_type(bool async)
    {
        await base.Update_multiple_projected_complex_types_via_anonymous_type(async);

        AssertExecuteUpdateSql(
            """
@p='54321'

UPDATE [c]
SET [c].[ShippingAddress_ZipCode] = [c].[BillingAddress_ZipCode],
    [c].[BillingAddress_ZipCode] = @p
FROM [Customer] AS [c]
""");
    }

    public override async Task Update_projected_complex_type_via_OrderBy_Skip(bool async)
    {
        await base.Update_projected_complex_type_via_OrderBy_Skip(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_complex_type_to_parameter(bool async)
    {
        await base.Update_complex_type_to_parameter(async);

        AssertExecuteUpdateSql(
            """
@complex_type_p_AddressLine1='New AddressLine1' (Size = 4000)
@complex_type_p_AddressLine2='New AddressLine2' (Size = 4000)
@complex_type_p_Tags='["new_tag1","new_tag2"]' (Size = 4000)
@complex_type_p_ZipCode='99999' (Nullable = true)
@complex_type_p_Code='FR' (Size = 4000)
@complex_type_p_FullName='France' (Size = 4000)

UPDATE [c]
SET [c].[ShippingAddress_AddressLine1] = @complex_type_p_AddressLine1,
    [c].[ShippingAddress_AddressLine2] = @complex_type_p_AddressLine2,
    [c].[ShippingAddress_Tags] = @complex_type_p_Tags,
    [c].[ShippingAddress_ZipCode] = @complex_type_p_ZipCode,
    [c].[ShippingAddress_Country_Code] = @complex_type_p_Code,
    [c].[ShippingAddress_Country_FullName] = @complex_type_p_FullName
FROM [Customer] AS [c]
""");
    }

    public override async Task Update_nested_complex_type_to_parameter(bool async)
    {
        await base.Update_nested_complex_type_to_parameter(async);

        AssertExecuteUpdateSql(
            """
@complex_type_p_Code='FR' (Size = 4000)
@complex_type_p_FullName='France' (Size = 4000)

UPDATE [c]
SET [c].[ShippingAddress_Country_Code] = @complex_type_p_Code,
    [c].[ShippingAddress_Country_FullName] = @complex_type_p_FullName
FROM [Customer] AS [c]
""");
    }

    public override async Task Update_complex_type_to_another_database_complex_type(bool async)
    {
        await base.Update_complex_type_to_another_database_complex_type(async);

        AssertExecuteUpdateSql(
            """
UPDATE [c]
SET [c].[ShippingAddress_AddressLine1] = [c].[BillingAddress_AddressLine1],
    [c].[ShippingAddress_AddressLine2] = [c].[BillingAddress_AddressLine2],
    [c].[ShippingAddress_Tags] = [c].[BillingAddress_Tags],
    [c].[ShippingAddress_ZipCode] = [c].[BillingAddress_ZipCode],
    [c].[ShippingAddress_Country_Code] = [c].[ShippingAddress_Country_Code],
    [c].[ShippingAddress_Country_FullName] = [c].[ShippingAddress_Country_FullName]
FROM [Customer] AS [c]
""");
    }

    public override async Task Update_complex_type_to_inline_without_lambda(bool async)
    {
        await base.Update_complex_type_to_inline_without_lambda(async);

        AssertExecuteUpdateSql(
            """
@complex_type_p_AddressLine1='New AddressLine1' (Size = 4000)
@complex_type_p_AddressLine2='New AddressLine2' (Size = 4000)
@complex_type_p_Tags='["new_tag1","new_tag2"]' (Size = 4000)
@complex_type_p_ZipCode='99999' (Nullable = true)
@complex_type_p_Code='FR' (Size = 4000)
@complex_type_p_FullName='France' (Size = 4000)

UPDATE [c]
SET [c].[ShippingAddress_AddressLine1] = @complex_type_p_AddressLine1,
    [c].[ShippingAddress_AddressLine2] = @complex_type_p_AddressLine2,
    [c].[ShippingAddress_Tags] = @complex_type_p_Tags,
    [c].[ShippingAddress_ZipCode] = @complex_type_p_ZipCode,
    [c].[ShippingAddress_Country_Code] = @complex_type_p_Code,
    [c].[ShippingAddress_Country_FullName] = @complex_type_p_FullName
FROM [Customer] AS [c]
""");
    }

    public override async Task Update_complex_type_to_inline_with_lambda(bool async)
    {
        await base.Update_complex_type_to_inline_with_lambda(async);

        AssertExecuteUpdateSql(
            """
UPDATE [c]
SET [c].[ShippingAddress_AddressLine1] = N'New AddressLine1',
    [c].[ShippingAddress_AddressLine2] = N'New AddressLine2',
    [c].[ShippingAddress_Tags] = N'["new_tag1","new_tag2"]',
    [c].[ShippingAddress_ZipCode] = 99999,
    [c].[ShippingAddress_Country_Code] = N'FR',
    [c].[ShippingAddress_Country_FullName] = N'France'
FROM [Customer] AS [c]
""");
    }

    public override async Task Update_complex_type_to_another_database_complex_type_with_subquery(bool async)
    {
        await base.Update_complex_type_to_another_database_complex_type_with_subquery(async);

        AssertExecuteUpdateSql(
            """
@p='1'

UPDATE [c0]
SET [c0].[ShippingAddress_AddressLine1] = [c1].[BillingAddress_AddressLine1],
    [c0].[ShippingAddress_AddressLine2] = [c1].[BillingAddress_AddressLine2],
    [c0].[ShippingAddress_Tags] = [c1].[BillingAddress_Tags],
    [c0].[ShippingAddress_ZipCode] = [c1].[BillingAddress_ZipCode],
    [c0].[ShippingAddress_Country_Code] = [c1].[ShippingAddress_Country_Code],
    [c0].[ShippingAddress_Country_FullName] = [c1].[ShippingAddress_Country_FullName]
FROM [Customer] AS [c0]
INNER JOIN (
    SELECT [c].[Id], [c].[BillingAddress_AddressLine1], [c].[BillingAddress_AddressLine2], [c].[BillingAddress_Tags], [c].[BillingAddress_ZipCode], [c].[ShippingAddress_Country_Code], [c].[ShippingAddress_Country_FullName]
    FROM [Customer] AS [c]
    ORDER BY [c].[Id]
    OFFSET @p ROWS
) AS [c1] ON [c0].[Id] = [c1].[Id]
""");
    }

    public override async Task Update_collection_inside_complex_type(bool async)
    {
        await base.Update_collection_inside_complex_type(async);

        AssertExecuteUpdateSql(
            """
@p='["new_tag1","new_tag2"]' (Size = 4000)

UPDATE [c]
SET [c].[ShippingAddress_Tags] = @p
FROM [Customer] AS [c]
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertExecuteUpdateSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    public class ComplexTypeBulkUpdatesSqlServerFixture : ComplexTypeBulkUpdatesRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
