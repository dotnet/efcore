// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryFilterFuncletizationSqlServerTest
        : QueryFilterFuncletizationTestBase<QueryFilterFuncletizationSqlServerTest.QueryFilterFuncletizationSqlServerFixture>
    {
        public QueryFilterFuncletizationSqlServerTest(
            QueryFilterFuncletizationSqlServerFixture fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void DbContext_property_parameter_does_not_clash_with_closure_parameter_name()
        {
            base.DbContext_property_parameter_does_not_clash_with_closure_parameter_name();

            AssertSql(
                @"@__ef_filter__Field_0='False'
@__Field_0='False'

SELECT [f].[Id], [f].[IsEnabled]
FROM [FieldFilter] AS [f]
WHERE ([f].[IsEnabled] = @__ef_filter__Field_0) AND ([f].[IsEnabled] = @__Field_0)");
        }

        public override void DbContext_field_is_parameterized()
        {
            base.DbContext_field_is_parameterized();

            AssertSql(
                @"@__ef_filter__Field_0='False'

SELECT [f].[Id], [f].[IsEnabled]
FROM [FieldFilter] AS [f]
WHERE [f].[IsEnabled] = @__ef_filter__Field_0",
                //
                @"@__ef_filter__Field_0='True'

SELECT [f].[Id], [f].[IsEnabled]
FROM [FieldFilter] AS [f]
WHERE [f].[IsEnabled] = @__ef_filter__Field_0");
        }

        public override void DbContext_property_is_parameterized()
        {
            base.DbContext_property_is_parameterized();

            AssertSql(
                @"@__ef_filter__Property_0='False'

SELECT [p].[Id], [p].[IsEnabled]
FROM [PropertyFilter] AS [p]
WHERE [p].[IsEnabled] = @__ef_filter__Property_0",
                //
                @"@__ef_filter__Property_0='True'

SELECT [p].[Id], [p].[IsEnabled]
FROM [PropertyFilter] AS [p]
WHERE [p].[IsEnabled] = @__ef_filter__Property_0");
        }

        public override void DbContext_method_call_is_parameterized()
        {
            base.DbContext_method_call_is_parameterized();

            AssertSql(
                @"@__ef_filter__p_0='2'

SELECT [m].[Id], [m].[Tenant]
FROM [MethodCallFilter] AS [m]
WHERE [m].[Tenant] = @__ef_filter__p_0");
        }

        public override void DbContext_list_is_parameterized()
        {
            base.DbContext_list_is_parameterized();

            AssertSql(
                @"SELECT [l].[Id], [l].[Tenant]
FROM [ListFilter] AS [l]
WHERE 0 = 1",
                //
                @"SELECT [l].[Id], [l].[Tenant]
FROM [ListFilter] AS [l]
WHERE [l].[Tenant] IN (1)",
                //
                @"SELECT [l].[Id], [l].[Tenant]
FROM [ListFilter] AS [l]
WHERE [l].[Tenant] IN (2, 3)");
        }

        public override void DbContext_property_chain_is_parameterized()
        {
            base.DbContext_property_chain_is_parameterized();

            AssertSql(
                @"@__ef_filter__Enabled_0='False'

SELECT [p].[Id], [p].[IsEnabled]
FROM [PropertyChainFilter] AS [p]
WHERE [p].[IsEnabled] = @__ef_filter__Enabled_0",
                //
                @"@__ef_filter__Enabled_0='True'

SELECT [p].[Id], [p].[IsEnabled]
FROM [PropertyChainFilter] AS [p]
WHERE [p].[IsEnabled] = @__ef_filter__Enabled_0");
        }

        public override void DbContext_property_method_call_is_parameterized()
        {
            base.DbContext_property_method_call_is_parameterized();

            AssertSql(
                @"@__ef_filter__p_0='2'

SELECT [p].[Id], [p].[Tenant]
FROM [PropertyMethodCallFilter] AS [p]
WHERE [p].[Tenant] = @__ef_filter__p_0");
        }

        public override void DbContext_method_call_chain_is_parameterized()
        {
            base.DbContext_method_call_chain_is_parameterized();

            AssertSql(
                @"@__ef_filter__p_0='2'

SELECT [m].[Id], [m].[Tenant]
FROM [MethodCallChainFilter] AS [m]
WHERE [m].[Tenant] = @__ef_filter__p_0");
        }

        public override void DbContext_complex_expression_is_parameterized()
        {
            base.DbContext_complex_expression_is_parameterized();

            AssertSql(
                @"@__ef_filter__Property_0='False'
@__ef_filter__p_1='True'

SELECT [c].[Id], [c].[IsEnabled]
FROM [ComplexFilter] AS [c]
WHERE ([c].[IsEnabled] = @__ef_filter__Property_0) AND (@__ef_filter__p_1 = CAST(1 AS bit))",
                //
                @"@__ef_filter__Property_0='True'
@__ef_filter__p_1='True'

SELECT [c].[Id], [c].[IsEnabled]
FROM [ComplexFilter] AS [c]
WHERE ([c].[IsEnabled] = @__ef_filter__Property_0) AND (@__ef_filter__p_1 = CAST(1 AS bit))",
                //
                @"@__ef_filter__Property_0='True'
@__ef_filter__p_1='False'

SELECT [c].[Id], [c].[IsEnabled]
FROM [ComplexFilter] AS [c]
WHERE ([c].[IsEnabled] = @__ef_filter__Property_0) AND (@__ef_filter__p_1 = CAST(1 AS bit))");
        }

        public override void DbContext_property_based_filter_does_not_short_circuit()
        {
            base.DbContext_property_based_filter_does_not_short_circuit();

            AssertSql(
                @"@__ef_filter__p_0='False'
@__ef_filter__IsModerated_1='True' (Nullable = true)

SELECT [s].[Id], [s].[IsDeleted], [s].[IsModerated]
FROM [ShortCircuitFilter] AS [s]
WHERE ([s].[IsDeleted] <> CAST(1 AS bit)) AND ((@__ef_filter__p_0 = CAST(1 AS bit)) OR (@__ef_filter__IsModerated_1 = [s].[IsModerated]))",
                //
                @"@__ef_filter__p_0='False'
@__ef_filter__IsModerated_1='False' (Nullable = true)

SELECT [s].[Id], [s].[IsDeleted], [s].[IsModerated]
FROM [ShortCircuitFilter] AS [s]
WHERE ([s].[IsDeleted] <> CAST(1 AS bit)) AND ((@__ef_filter__p_0 = CAST(1 AS bit)) OR (@__ef_filter__IsModerated_1 = [s].[IsModerated]))",
                //
                @"@__ef_filter__p_0='True'

SELECT [s].[Id], [s].[IsDeleted], [s].[IsModerated]
FROM [ShortCircuitFilter] AS [s]
WHERE ([s].[IsDeleted] <> CAST(1 AS bit)) AND (@__ef_filter__p_0 = CAST(1 AS bit))");
        }

        public override void EntityTypeConfiguration_DbContext_field_is_parameterized()
        {
            base.EntityTypeConfiguration_DbContext_field_is_parameterized();

            AssertSql(
                @"@__ef_filter__Field_0='False'

SELECT [e].[Id], [e].[IsEnabled]
FROM [EntityTypeConfigurationFieldFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Field_0",
                //
                @"@__ef_filter__Field_0='True'

SELECT [e].[Id], [e].[IsEnabled]
FROM [EntityTypeConfigurationFieldFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Field_0");
        }

        public override void EntityTypeConfiguration_DbContext_property_is_parameterized()
        {
            base.EntityTypeConfiguration_DbContext_property_is_parameterized();

            AssertSql(
                @"@__ef_filter__Property_0='False'

SELECT [e].[Id], [e].[IsEnabled]
FROM [EntityTypeConfigurationPropertyFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Property_0",
                //
                @"@__ef_filter__Property_0='True'

SELECT [e].[Id], [e].[IsEnabled]
FROM [EntityTypeConfigurationPropertyFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Property_0");
        }

        public override void EntityTypeConfiguration_DbContext_method_call_is_parameterized()
        {
            base.EntityTypeConfiguration_DbContext_method_call_is_parameterized();

            AssertSql(
                @"@__ef_filter__p_0='2'

SELECT [e].[Id], [e].[Tenant]
FROM [EntityTypeConfigurationMethodCallFilter] AS [e]
WHERE [e].[Tenant] = @__ef_filter__p_0");
        }

        public override void EntityTypeConfiguration_DbContext_property_chain_is_parameterized()
        {
            base.EntityTypeConfiguration_DbContext_property_chain_is_parameterized();

            AssertSql(
                @"@__ef_filter__Enabled_0='False'

SELECT [e].[Id], [e].[IsEnabled]
FROM [EntityTypeConfigurationPropertyChainFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Enabled_0",
                //
                @"@__ef_filter__Enabled_0='True'

SELECT [e].[Id], [e].[IsEnabled]
FROM [EntityTypeConfigurationPropertyChainFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Enabled_0");
        }

        public override void Local_method_DbContext_field_is_parameterized()
        {
            base.Local_method_DbContext_field_is_parameterized();

            AssertSql(
                @"@__ef_filter__Field_0='False'

SELECT [l].[Id], [l].[IsEnabled]
FROM [LocalMethodFilter] AS [l]
WHERE [l].[IsEnabled] = @__ef_filter__Field_0",
                //
                @"@__ef_filter__Field_0='True'

SELECT [l].[Id], [l].[IsEnabled]
FROM [LocalMethodFilter] AS [l]
WHERE [l].[IsEnabled] = @__ef_filter__Field_0");
        }

        public override void Local_static_method_DbContext_property_is_parameterized()
        {
            base.Local_static_method_DbContext_property_is_parameterized();

            AssertSql(
                @"@__ef_filter__Property_0='False'

SELECT [l].[Id], [l].[IsEnabled]
FROM [LocalMethodParamsFilter] AS [l]
WHERE [l].[IsEnabled] = @__ef_filter__Property_0",
                //
                @"@__ef_filter__Property_0='True'

SELECT [l].[Id], [l].[IsEnabled]
FROM [LocalMethodParamsFilter] AS [l]
WHERE [l].[IsEnabled] = @__ef_filter__Property_0");
        }

        public override void Remote_method_DbContext_property_method_call_is_parameterized()
        {
            base.Remote_method_DbContext_property_method_call_is_parameterized();

            AssertSql(
                @"@__ef_filter__p_0='2'

SELECT [r].[Id], [r].[Tenant]
FROM [RemoteMethodParamsFilter] AS [r]
WHERE [r].[Tenant] = @__ef_filter__p_0");
        }

        public override void Extension_method_DbContext_field_is_parameterized()
        {
            base.Extension_method_DbContext_field_is_parameterized();

            AssertSql(
                @"@__ef_filter__Field_0='False'

SELECT [e].[Id], [e].[IsEnabled]
FROM [ExtensionBuilderFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Field_0",
                //
                @"@__ef_filter__Field_0='True'

SELECT [e].[Id], [e].[IsEnabled]
FROM [ExtensionBuilderFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Field_0");
        }

        public override void Extension_method_DbContext_property_chain_is_parameterized()
        {
            base.Extension_method_DbContext_property_chain_is_parameterized();

            AssertSql(
                @"@__ef_filter__Enabled_0='False'

SELECT [e].[Id], [e].[IsEnabled]
FROM [ExtensionContextFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Enabled_0",
                //
                @"@__ef_filter__Enabled_0='True'

SELECT [e].[Id], [e].[IsEnabled]
FROM [ExtensionContextFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Enabled_0");
        }

        public override void Using_DbSet_in_filter_works()
        {
            base.Using_DbSet_in_filter_works();

            AssertSql(
                @"@__ef_filter__Property_0='False'

SELECT [p].[Id], [p].[Filler]
FROM [PrincipalSetFilter] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM [Dependents] AS [d]
    WHERE EXISTS (
        SELECT 1
        FROM [MultiContextFilter] AS [m]
        WHERE (([m].[IsEnabled] = @__ef_filter__Property_0) AND ([m].[BossId] = 1)) AND ([m].[BossId] = [d].[PrincipalSetFilterId])) AND ([d].[PrincipalSetFilterId] = [p].[Id]))");
        }

        public override void Using_Context_set_method_in_filter_works()
        {
            base.Using_Context_set_method_in_filter_works();

            AssertSql(
                @"@__ef_filter__Property_0='False'

SELECT [d].[Id], [d].[PrincipalSetFilterId]
FROM [Dependents] AS [d]
WHERE EXISTS (
    SELECT 1
    FROM [MultiContextFilter] AS [m]
    WHERE (([m].[IsEnabled] = @__ef_filter__Property_0) AND ([m].[BossId] = 1)) AND ([m].[BossId] = [d].[PrincipalSetFilterId]))");
        }

        public override void Static_member_from_dbContext_is_inlined()
        {
            base.Static_member_from_dbContext_is_inlined();

            AssertSql(
                @"SELECT [d].[Id], [d].[UserId]
FROM [DbContextStaticMemberFilter] AS [d]
WHERE [d].[UserId] <> 1");
        }

        public override void Static_member_from_non_dbContext_is_inlined()
        {
            base.Static_member_from_non_dbContext_is_inlined();

            AssertSql(
                @"SELECT [s].[Id], [s].[IsEnabled]
FROM [StaticMemberFilter] AS [s]
WHERE [s].[IsEnabled] = CAST(1 AS bit)");
        }

        public override void Local_variable_from_OnModelCreating_is_inlined()
        {
            base.Local_variable_from_OnModelCreating_is_inlined();

            AssertSql(
                @"SELECT [l].[Id], [l].[IsEnabled]
FROM [LocalVariableFilter] AS [l]
WHERE [l].[IsEnabled] = CAST(1 AS bit)");
        }

        public override void Method_parameter_is_inlined()
        {
            base.Method_parameter_is_inlined();

            AssertSql(
                @"SELECT [p].[Id], [p].[Tenant]
FROM [ParameterFilter] AS [p]
WHERE [p].[Tenant] = 0");
        }

        public override void Using_multiple_context_in_filter_parametrize_only_current_context()
        {
            base.Using_multiple_context_in_filter_parametrize_only_current_context();

            AssertSql(
                @"@__ef_filter__Property_0='False'

SELECT [m].[Id], [m].[BossId], [m].[IsEnabled]
FROM [MultiContextFilter] AS [m]
WHERE ([m].[IsEnabled] = @__ef_filter__Property_0) AND ([m].[BossId] = 1)",
                //
                @"@__ef_filter__Property_0='True'

SELECT [m].[Id], [m].[BossId], [m].[IsEnabled]
FROM [MultiContextFilter] AS [m]
WHERE ([m].[IsEnabled] = @__ef_filter__Property_0) AND ([m].[BossId] = 1)");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class QueryFilterFuncletizationSqlServerFixture : QueryFilterFuncletizationRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
        }
    }
}
