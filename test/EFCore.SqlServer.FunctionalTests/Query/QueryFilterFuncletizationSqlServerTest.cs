// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class QueryFilterFuncletizationSqlServerTest
    : QueryFilterFuncletizationTestBase<QueryFilterFuncletizationSqlServerTest.QueryFilterFuncletizationSqlServerFixture>
{
    public QueryFilterFuncletizationSqlServerTest(
        QueryFilterFuncletizationSqlServerFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override void DbContext_property_parameter_does_not_clash_with_closure_parameter_name()
    {
        base.DbContext_property_parameter_does_not_clash_with_closure_parameter_name();

        AssertSql(
            """
@__ef_filter__Field_0='False'
@__Field_0='False'

SELECT [f].[Id], [f].[IsEnabled]
FROM [FieldFilter] AS [f]
WHERE [f].[IsEnabled] = @__ef_filter__Field_0 AND [f].[IsEnabled] = @__Field_0
""");
    }

    public override void DbContext_field_is_parameterized()
    {
        base.DbContext_field_is_parameterized();

        AssertSql(
            """
@__ef_filter__Field_0='False'

SELECT [f].[Id], [f].[IsEnabled]
FROM [FieldFilter] AS [f]
WHERE [f].[IsEnabled] = @__ef_filter__Field_0
""",
            //
            """
@__ef_filter__Field_0='True'

SELECT [f].[Id], [f].[IsEnabled]
FROM [FieldFilter] AS [f]
WHERE [f].[IsEnabled] = @__ef_filter__Field_0
""");
    }

    public override void DbContext_property_is_parameterized()
    {
        base.DbContext_property_is_parameterized();

        AssertSql(
            """
@__ef_filter__Property_0='False'

SELECT [p].[Id], [p].[IsEnabled]
FROM [PropertyFilter] AS [p]
WHERE [p].[IsEnabled] = @__ef_filter__Property_0
""",
            //
            """
@__ef_filter__Property_0='True'

SELECT [p].[Id], [p].[IsEnabled]
FROM [PropertyFilter] AS [p]
WHERE [p].[IsEnabled] = @__ef_filter__Property_0
""");
    }

    public override void DbContext_method_call_is_parameterized()
    {
        base.DbContext_method_call_is_parameterized();

        AssertSql(
            """
@__ef_filter__p_0='2'

SELECT [m].[Id], [m].[Tenant]
FROM [MethodCallFilter] AS [m]
WHERE [m].[Tenant] = @__ef_filter__p_0
""");
    }

    public override void DbContext_list_is_parameterized()
    {
        base.DbContext_list_is_parameterized();

        AssertSql(
            """
SELECT [l].[Id], [l].[Tenant]
FROM [ListFilter] AS [l]
WHERE [l].[Tenant] IN (
    SELECT [e].[value]
    FROM OPENJSON(NULL) AS [e]
)
""",
            //
            """
@__ef_filter__TenantIds_0='[]' (Size = 4000)

SELECT [l].[Id], [l].[Tenant]
FROM [ListFilter] AS [l]
WHERE [l].[Tenant] IN (
    SELECT [e].[value]
    FROM OPENJSON(@__ef_filter__TenantIds_0) WITH ([value] int '$') AS [e]
)
""",
            //
            """
@__ef_filter__TenantIds_0='[1]' (Size = 4000)

SELECT [l].[Id], [l].[Tenant]
FROM [ListFilter] AS [l]
WHERE [l].[Tenant] IN (
    SELECT [e].[value]
    FROM OPENJSON(@__ef_filter__TenantIds_0) WITH ([value] int '$') AS [e]
)
""",
            //
            """
@__ef_filter__TenantIds_0='[2,3]' (Size = 4000)

SELECT [l].[Id], [l].[Tenant]
FROM [ListFilter] AS [l]
WHERE [l].[Tenant] IN (
    SELECT [e].[value]
    FROM OPENJSON(@__ef_filter__TenantIds_0) WITH ([value] int '$') AS [e]
)
""");
    }

    public override void DbContext_property_chain_is_parameterized()
    {
        base.DbContext_property_chain_is_parameterized();

        AssertSql(
            """
@__ef_filter__Enabled_0='False'

SELECT [p].[Id], [p].[IsEnabled]
FROM [PropertyChainFilter] AS [p]
WHERE [p].[IsEnabled] = @__ef_filter__Enabled_0
""",
            //
            """
@__ef_filter__Enabled_0='True'

SELECT [p].[Id], [p].[IsEnabled]
FROM [PropertyChainFilter] AS [p]
WHERE [p].[IsEnabled] = @__ef_filter__Enabled_0
""");
    }

    public override void DbContext_property_method_call_is_parameterized()
    {
        base.DbContext_property_method_call_is_parameterized();

        AssertSql(
            """
@__ef_filter__p_0='2'

SELECT [p].[Id], [p].[Tenant]
FROM [PropertyMethodCallFilter] AS [p]
WHERE [p].[Tenant] = @__ef_filter__p_0
""");
    }

    public override void DbContext_method_call_chain_is_parameterized()
    {
        base.DbContext_method_call_chain_is_parameterized();

        AssertSql(
            """
@__ef_filter__p_0='2'

SELECT [m].[Id], [m].[Tenant]
FROM [MethodCallChainFilter] AS [m]
WHERE [m].[Tenant] = @__ef_filter__p_0
""");
    }

    public override void DbContext_complex_expression_is_parameterized()
    {
        base.DbContext_complex_expression_is_parameterized();

        AssertSql(
            """
@__ef_filter__Property_0='False'
@__ef_filter__p_1='True'

SELECT [c].[Id], [c].[IsEnabled]
FROM [ComplexFilter] AS [c]
WHERE [c].[IsEnabled] = @__ef_filter__Property_0 AND @__ef_filter__p_1 = CAST(1 AS bit)
""",
            //
            """
@__ef_filter__Property_0='True'
@__ef_filter__p_1='True'

SELECT [c].[Id], [c].[IsEnabled]
FROM [ComplexFilter] AS [c]
WHERE [c].[IsEnabled] = @__ef_filter__Property_0 AND @__ef_filter__p_1 = CAST(1 AS bit)
""",
            //
            """
@__ef_filter__Property_0='True'
@__ef_filter__p_1='False'

SELECT [c].[Id], [c].[IsEnabled]
FROM [ComplexFilter] AS [c]
WHERE [c].[IsEnabled] = @__ef_filter__Property_0 AND @__ef_filter__p_1 = CAST(1 AS bit)
""");
    }

    public override void DbContext_property_based_filter_does_not_short_circuit()
    {
        base.DbContext_property_based_filter_does_not_short_circuit();

        AssertSql(
            """
@__ef_filter__p_1='False'
@__ef_filter__IsModerated_0='True' (Nullable = true)

SELECT [s].[Id], [s].[IsDeleted], [s].[IsModerated]
FROM [ShortCircuitFilter] AS [s]
WHERE [s].[IsDeleted] = CAST(0 AS bit) AND (@__ef_filter__p_1 = CAST(1 AS bit) OR @__ef_filter__IsModerated_0 = [s].[IsModerated])
""",
            //
            """
@__ef_filter__p_1='False'
@__ef_filter__IsModerated_0='False' (Nullable = true)

SELECT [s].[Id], [s].[IsDeleted], [s].[IsModerated]
FROM [ShortCircuitFilter] AS [s]
WHERE [s].[IsDeleted] = CAST(0 AS bit) AND (@__ef_filter__p_1 = CAST(1 AS bit) OR @__ef_filter__IsModerated_0 = [s].[IsModerated])
""",
            //
            """
@__ef_filter__p_1='True'

SELECT [s].[Id], [s].[IsDeleted], [s].[IsModerated]
FROM [ShortCircuitFilter] AS [s]
WHERE [s].[IsDeleted] = CAST(0 AS bit) AND @__ef_filter__p_1 = CAST(1 AS bit)
""");
    }

    public override void EntityTypeConfiguration_DbContext_field_is_parameterized()
    {
        base.EntityTypeConfiguration_DbContext_field_is_parameterized();

        AssertSql(
            """
@__ef_filter__Field_0='False'

SELECT [e].[Id], [e].[IsEnabled]
FROM [EntityTypeConfigurationFieldFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Field_0
""",
            //
            """
@__ef_filter__Field_0='True'

SELECT [e].[Id], [e].[IsEnabled]
FROM [EntityTypeConfigurationFieldFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Field_0
""");
    }

    public override void EntityTypeConfiguration_DbContext_property_is_parameterized()
    {
        base.EntityTypeConfiguration_DbContext_property_is_parameterized();

        AssertSql(
            """
@__ef_filter__Property_0='False'

SELECT [e].[Id], [e].[IsEnabled]
FROM [EntityTypeConfigurationPropertyFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Property_0
""",
            //
            """
@__ef_filter__Property_0='True'

SELECT [e].[Id], [e].[IsEnabled]
FROM [EntityTypeConfigurationPropertyFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Property_0
""");
    }

    public override void EntityTypeConfiguration_DbContext_method_call_is_parameterized()
    {
        base.EntityTypeConfiguration_DbContext_method_call_is_parameterized();

        AssertSql(
            """
@__ef_filter__p_0='2'

SELECT [e].[Id], [e].[Tenant]
FROM [EntityTypeConfigurationMethodCallFilter] AS [e]
WHERE [e].[Tenant] = @__ef_filter__p_0
""");
    }

    public override void EntityTypeConfiguration_DbContext_property_chain_is_parameterized()
    {
        base.EntityTypeConfiguration_DbContext_property_chain_is_parameterized();

        AssertSql(
            """
@__ef_filter__Enabled_0='False'

SELECT [e].[Id], [e].[IsEnabled]
FROM [EntityTypeConfigurationPropertyChainFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Enabled_0
""",
            //
            """
@__ef_filter__Enabled_0='True'

SELECT [e].[Id], [e].[IsEnabled]
FROM [EntityTypeConfigurationPropertyChainFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Enabled_0
""");
    }

    public override void Local_method_DbContext_field_is_parameterized()
    {
        base.Local_method_DbContext_field_is_parameterized();

        AssertSql(
            """
@__ef_filter__Field_0='False'

SELECT [l].[Id], [l].[IsEnabled]
FROM [LocalMethodFilter] AS [l]
WHERE [l].[IsEnabled] = @__ef_filter__Field_0
""",
            //
            """
@__ef_filter__Field_0='True'

SELECT [l].[Id], [l].[IsEnabled]
FROM [LocalMethodFilter] AS [l]
WHERE [l].[IsEnabled] = @__ef_filter__Field_0
""");
    }

    public override void Local_static_method_DbContext_property_is_parameterized()
    {
        base.Local_static_method_DbContext_property_is_parameterized();

        AssertSql(
            """
@__ef_filter__Property_0='False'

SELECT [l].[Id], [l].[IsEnabled]
FROM [LocalMethodParamsFilter] AS [l]
WHERE [l].[IsEnabled] = @__ef_filter__Property_0
""",
            //
            """
@__ef_filter__Property_0='True'

SELECT [l].[Id], [l].[IsEnabled]
FROM [LocalMethodParamsFilter] AS [l]
WHERE [l].[IsEnabled] = @__ef_filter__Property_0
""");
    }

    public override void Remote_method_DbContext_property_method_call_is_parameterized()
    {
        base.Remote_method_DbContext_property_method_call_is_parameterized();

        AssertSql(
            """
@__ef_filter__p_0='2'

SELECT [r].[Id], [r].[Tenant]
FROM [RemoteMethodParamsFilter] AS [r]
WHERE [r].[Tenant] = @__ef_filter__p_0
""");
    }

    public override void Extension_method_DbContext_field_is_parameterized()
    {
        base.Extension_method_DbContext_field_is_parameterized();

        AssertSql(
            """
@__ef_filter__Field_0='False'

SELECT [e].[Id], [e].[IsEnabled]
FROM [ExtensionBuilderFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Field_0
""",
            //
            """
@__ef_filter__Field_0='True'

SELECT [e].[Id], [e].[IsEnabled]
FROM [ExtensionBuilderFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Field_0
""");
    }

    public override void Extension_method_DbContext_property_chain_is_parameterized()
    {
        base.Extension_method_DbContext_property_chain_is_parameterized();

        AssertSql(
            """
@__ef_filter__Enabled_0='False'

SELECT [e].[Id], [e].[IsEnabled]
FROM [ExtensionContextFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Enabled_0
""",
            //
            """
@__ef_filter__Enabled_0='True'

SELECT [e].[Id], [e].[IsEnabled]
FROM [ExtensionContextFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Enabled_0
""");
    }

    public override void Using_DbSet_in_filter_works()
    {
        base.Using_DbSet_in_filter_works();

        AssertSql(
            """
@__ef_filter__Property_0='False'

SELECT [p].[Id], [p].[Filler]
FROM [PrincipalSetFilter] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM [Dependents] AS [d]
    WHERE EXISTS (
        SELECT 1
        FROM [MultiContextFilter] AS [m]
        WHERE [m].[IsEnabled] = @__ef_filter__Property_0 AND [m].[BossId] = 1 AND [m].[BossId] = [d].[PrincipalSetFilterId]) AND [d].[PrincipalSetFilterId] = [p].[Id])
""");
    }

    public override void Using_Context_set_method_in_filter_works()
    {
        base.Using_Context_set_method_in_filter_works();

        AssertSql(
            """
@__ef_filter__Property_0='False'

SELECT [d].[Id], [d].[PrincipalSetFilterId]
FROM [Dependents] AS [d]
WHERE EXISTS (
    SELECT 1
    FROM [MultiContextFilter] AS [m]
    WHERE [m].[IsEnabled] = @__ef_filter__Property_0 AND [m].[BossId] = 1 AND [m].[BossId] = [d].[PrincipalSetFilterId])
""");
    }

    public override void Static_member_from_dbContext_is_inlined()
    {
        base.Static_member_from_dbContext_is_inlined();

        AssertSql(
            """
SELECT [d].[Id], [d].[UserId]
FROM [DbContextStaticMemberFilter] AS [d]
WHERE [d].[UserId] <> 1
""");
    }

    public override void Static_member_from_non_dbContext_is_inlined()
    {
        base.Static_member_from_non_dbContext_is_inlined();

        AssertSql(
            """
SELECT [s].[Id], [s].[IsEnabled]
FROM [StaticMemberFilter] AS [s]
WHERE [s].[IsEnabled] = CAST(1 AS bit)
""");
    }

    public override void Local_variable_from_OnModelCreating_is_inlined()
    {
        base.Local_variable_from_OnModelCreating_is_inlined();

        AssertSql(
            """
SELECT [l].[Id], [l].[IsEnabled]
FROM [LocalVariableFilter] AS [l]
WHERE [l].[IsEnabled] = CAST(1 AS bit)
""");
    }

    public override void Method_parameter_is_inlined()
    {
        base.Method_parameter_is_inlined();

        AssertSql(
            """
SELECT [p].[Id], [p].[Tenant]
FROM [ParameterFilter] AS [p]
WHERE [p].[Tenant] = 0
""");
    }

    public override void Using_multiple_context_in_filter_parametrize_only_current_context()
    {
        base.Using_multiple_context_in_filter_parametrize_only_current_context();

        AssertSql(
            """
@__ef_filter__Property_0='False'

SELECT [m].[Id], [m].[BossId], [m].[IsEnabled]
FROM [MultiContextFilter] AS [m]
WHERE [m].[IsEnabled] = @__ef_filter__Property_0 AND [m].[BossId] = 1
""",
            //
            """
@__ef_filter__Property_0='True'

SELECT [m].[Id], [m].[BossId], [m].[IsEnabled]
FROM [MultiContextFilter] AS [m]
WHERE [m].[IsEnabled] = @__ef_filter__Property_0 AND [m].[BossId] = 1
""");
    }

    public override void Using_multiple_entities_with_filters_reuses_parameters()
    {
        base.Using_multiple_entities_with_filters_reuses_parameters();

        AssertSql(
            """
@__ef_filter__Tenant_0='1'
@__ef_filter__Tenant_0_1='1' (DbType = Int16)

SELECT [d].[Id], [d].[Tenant], [d2].[Id], [d2].[DeDupeFilter1Id], [d2].[TenantX], [d3].[Id], [d3].[DeDupeFilter1Id], [d3].[Tenant]
FROM [DeDupeFilter1] AS [d]
LEFT JOIN (
    SELECT [d0].[Id], [d0].[DeDupeFilter1Id], [d0].[TenantX]
    FROM [DeDupeFilter2] AS [d0]
    WHERE [d0].[TenantX] = @__ef_filter__Tenant_0
) AS [d2] ON [d].[Id] = [d2].[DeDupeFilter1Id]
LEFT JOIN (
    SELECT [d1].[Id], [d1].[DeDupeFilter1Id], [d1].[Tenant]
    FROM [DeDupeFilter3] AS [d1]
    WHERE [d1].[Tenant] = @__ef_filter__Tenant_0_1
) AS [d3] ON [d].[Id] = [d3].[DeDupeFilter1Id]
WHERE [d].[Tenant] = @__ef_filter__Tenant_0
ORDER BY [d].[Id], [d2].[Id]
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class QueryFilterFuncletizationSqlServerFixture : QueryFilterFuncletizationRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
