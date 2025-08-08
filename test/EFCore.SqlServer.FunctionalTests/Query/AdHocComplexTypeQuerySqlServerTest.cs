// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocComplexTypeQuerySqlServerTest(NonSharedFixture fixture) : AdHocComplexTypeQueryTestBase(fixture)
{
    public override async Task Complex_type_equals_parameter_with_nested_types_with_property_of_same_name()
    {
        await base.Complex_type_equals_parameter_with_nested_types_with_property_of_same_name();

        AssertSql(
            """
@entity_equality_container_Id='1' (Nullable = true)
@entity_equality_container_Containee1_Id='2' (Nullable = true)
@entity_equality_container_Containee2_Id='3' (Nullable = true)

SELECT TOP(2) [e].[Id], [e].[ComplexContainer_Id], [e].[ComplexContainer_Containee1_Id], [e].[ComplexContainer_Containee2_Id]
FROM [EntityType] AS [e]
WHERE [e].[ComplexContainer_Id] = @entity_equality_container_Id AND [e].[ComplexContainer_Containee1_Id] = @entity_equality_container_Containee1_Id AND [e].[ComplexContainer_Containee2_Id] = @entity_equality_container_Containee2_Id
""");
    }

    public override async Task Projecting_complex_property_does_not_auto_include_owned_types()
    {
        await base.Projecting_complex_property_does_not_auto_include_owned_types();

        AssertSql(
"""
SELECT [e].[Complex_Name], [e].[Complex_Number]
FROM [EntityType] AS [e]
""");
    }

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;
}
