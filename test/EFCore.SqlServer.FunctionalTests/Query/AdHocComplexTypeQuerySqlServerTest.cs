// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocComplexTypeQuerySqlServerTest : AdHocComplexTypeQueryTestBase
{
    public override async Task Complex_type_equals_parameter_with_nested_types_with_property_of_same_name()
    {
        await base.Complex_type_equals_parameter_with_nested_types_with_property_of_same_name();

        AssertSql(
            """
@__entity_equality_container_0_Id='1' (Nullable = true)
@__entity_equality_container_0_Containee1_Id='2' (Nullable = true)
@__entity_equality_container_0_Containee2_Id='3' (Nullable = true)

SELECT TOP(2) [e].[Id], [e].[ComplexContainer_Id], [e].[ComplexContainer_Containee1_Id], [e].[ComplexContainer_Containee2_Id]
FROM [EntityType] AS [e]
WHERE [e].[ComplexContainer_Id] = @__entity_equality_container_0_Id AND [e].[ComplexContainer_Containee1_Id] = @__entity_equality_container_0_Containee1_Id AND [e].[ComplexContainer_Containee2_Id] = @__entity_equality_container_0_Containee2_Id
""");
    }

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;
}
