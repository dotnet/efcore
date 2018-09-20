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

SELECT [e].[Id], [e].[IsEnabled]
FROM [FieldFilter] AS [e]
WHERE ([e].[IsEnabled] = @__ef_filter__Field_0) AND ([e].[IsEnabled] = @__Field_0)");
        }

        public override void DbContext_field_is_parameterized()
        {
            base.DbContext_field_is_parameterized();

            AssertSql(
                @"@__ef_filter__Field_0='False'

SELECT [e].[Id], [e].[IsEnabled]
FROM [FieldFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Field_0",
                //
                @"@__ef_filter__Field_0='True'

SELECT [e].[Id], [e].[IsEnabled]
FROM [FieldFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Field_0");
        }

        public override void DbContext_property_is_parameterized()
        {
            base.DbContext_property_is_parameterized();

            AssertSql(
                @"@__ef_filter__Property_0='False'

SELECT [e].[Id], [e].[IsEnabled]
FROM [PropertyFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Property_0",
                //
                @"@__ef_filter__Property_0='True'

SELECT [e].[Id], [e].[IsEnabled]
FROM [PropertyFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Property_0");
        }

        public override void DbContext_method_call_is_parameterized()
        {
            base.DbContext_method_call_is_parameterized();

            AssertSql(
                @"@__ef_filter__ef_filter_0='2'

SELECT [e].[Id], [e].[Tenant]
FROM [MethodCallFilter] AS [e]
WHERE [e].[Tenant] = @__ef_filter__ef_filter_0");
        }

        public override void DbContext_list_is_parameterized()
        {
            base.DbContext_list_is_parameterized();

            AssertSql(
                @"SELECT [e].[Id], [e].[Tenant]
FROM [ListFilter] AS [e]
WHERE 0 = 1",
                //
                @"SELECT [e].[Id], [e].[Tenant]
FROM [ListFilter] AS [e]
WHERE [e].[Tenant] IN (1)",
                //
                @"SELECT [e].[Id], [e].[Tenant]
FROM [ListFilter] AS [e]
WHERE [e].[Tenant] IN (2, 3)");
        }

        public override void DbContext_property_chain_is_parameterized()
        {
            base.DbContext_property_chain_is_parameterized();

            AssertSql(
                @"@__ef_filter__Enabled_0='False'

SELECT [e].[Id], [e].[IsEnabled]
FROM [PropertyChainFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Enabled_0",
                //
                @"@__ef_filter__Enabled_0='True'

SELECT [e].[Id], [e].[IsEnabled]
FROM [PropertyChainFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Enabled_0");
        }

        public override void DbContext_property_method_call_is_parameterized()
        {
            base.DbContext_property_method_call_is_parameterized();

            AssertSql(
                @"@__ef_filter__ef_filter_0='2'

SELECT [e].[Id], [e].[Tenant]
FROM [PropertyMethodCallFilter] AS [e]
WHERE [e].[Tenant] = @__ef_filter__ef_filter_0");
        }

        public override void DbContext_method_call_chain_is_parameterized()
        {
            base.DbContext_method_call_chain_is_parameterized();

            AssertSql(
                @"@__ef_filter__ef_filter_0='2'

SELECT [e].[Id], [e].[Tenant]
FROM [MethodCallChainFilter] AS [e]
WHERE [e].[Tenant] = @__ef_filter__ef_filter_0");
        }

        public override void DbContext_complex_expression_is_parameterized()
        {
            base.DbContext_complex_expression_is_parameterized();

            AssertSql(
                @"@__ef_filter__Property_0='False'
@__ef_filter__Tenant_1='0'
@__ef_filter__ef_filter_2='2'

SELECT [x].[Id], [x].[IsEnabled]
FROM [ComplexFilter] AS [x]
WHERE ([x].[IsEnabled] = @__ef_filter__Property_0) AND ((@__ef_filter__Tenant_1 + @__ef_filter__ef_filter_2) > 0)",
                //
                @"@__ef_filter__Property_0='True'
@__ef_filter__Tenant_1='0'
@__ef_filter__ef_filter_2='2'

SELECT [x].[Id], [x].[IsEnabled]
FROM [ComplexFilter] AS [x]
WHERE ([x].[IsEnabled] = @__ef_filter__Property_0) AND ((@__ef_filter__Tenant_1 + @__ef_filter__ef_filter_2) > 0)",
                //
                @"@__ef_filter__Property_0='True'
@__ef_filter__Tenant_1='-3'
@__ef_filter__ef_filter_2='2'

SELECT [x].[Id], [x].[IsEnabled]
FROM [ComplexFilter] AS [x]
WHERE ([x].[IsEnabled] = @__ef_filter__Property_0) AND ((@__ef_filter__Tenant_1 + @__ef_filter__ef_filter_2) > 0)");
        }

        public override void DbContext_property_based_filter_does_not_short_circuit()
        {
            base.DbContext_property_based_filter_does_not_short_circuit();

            AssertSql(
                @"@__ef_filter__IsModerated_0='True' (Nullable = true)

SELECT [x].[Id], [x].[IsDeleted], [x].[IsModerated]
FROM [ShortCircuitFilter] AS [x]
WHERE ([x].[IsDeleted] = 0) AND (@__ef_filter__IsModerated_0 IS NULL OR (@__ef_filter__IsModerated_0 = [x].[IsModerated]))",
                //
                @"@__ef_filter__IsModerated_0='False' (Nullable = true)

SELECT [x].[Id], [x].[IsDeleted], [x].[IsModerated]
FROM [ShortCircuitFilter] AS [x]
WHERE ([x].[IsDeleted] = 0) AND (@__ef_filter__IsModerated_0 IS NULL OR (@__ef_filter__IsModerated_0 = [x].[IsModerated]))",
                //
                @"@__ef_filter__IsModerated_0=''

SELECT [x].[Id], [x].[IsDeleted], [x].[IsModerated]
FROM [ShortCircuitFilter] AS [x]
WHERE ([x].[IsDeleted] = 0) AND (@__ef_filter__IsModerated_0 IS NULL OR [x].[IsModerated] IS NULL)");
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
                @"@__ef_filter__ef_filter_0='2'

SELECT [e].[Id], [e].[Tenant]
FROM [EntityTypeConfigurationMethodCallFilter] AS [e]
WHERE [e].[Tenant] = @__ef_filter__ef_filter_0");
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

SELECT [e].[Id], [e].[IsEnabled]
FROM [LocalMethodFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Field_0",
                //
                @"@__ef_filter__Field_0='True'

SELECT [e].[Id], [e].[IsEnabled]
FROM [LocalMethodFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Field_0");
        }

        public override void Local_static_method_DbContext_property_is_parameterized()
        {
            base.Local_static_method_DbContext_property_is_parameterized();

            AssertSql(
                @"@__ef_filter__Property_0='False'

SELECT [e].[Id], [e].[IsEnabled]
FROM [LocalMethodParamsFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Property_0",
                //
                @"@__ef_filter__Property_0='True'

SELECT [e].[Id], [e].[IsEnabled]
FROM [LocalMethodParamsFilter] AS [e]
WHERE [e].[IsEnabled] = @__ef_filter__Property_0");
        }

        public override void Remote_method_DbContext_property_method_call_is_parameterized()
        {
            base.Remote_method_DbContext_property_method_call_is_parameterized();

            AssertSql(
                @"@__ef_filter__ef_filter_0='2'

SELECT [e].[Id], [e].[Tenant]
FROM [RemoteMethodParamsFilter] AS [e]
WHERE [e].[Tenant] = @__ef_filter__ef_filter_0");
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
                @"SELECT [p].[Id], [p].[Filler]
FROM [PrincipalSetFilter] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM [Dependents] AS [d]
    WHERE [d].[PrincipalSetFilterId] = [p].[Id])");
        }

        public override void Using_Context_set_method_in_filter_works()
        {
            base.Using_Context_set_method_in_filter_works();

            AssertSql(
                @"SELECT [p].[Id], [p].[PrincipalSetFilterId]
FROM [Dependents] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM [MultiContextFilter] AS [b]
    WHERE [b].[BossId] = [p].[PrincipalSetFilterId])");
        }

        public override void Static_member_from_dbContext_is_inlined()
        {
            base.Static_member_from_dbContext_is_inlined();

            AssertSql(
                @"SELECT [e].[Id], [e].[UserId]
FROM [DbContextStaticMemberFilter] AS [e]
WHERE [e].[UserId] <> 1");
        }

        public override void Static_member_from_non_dbContext_is_inlined()
        {
            base.Static_member_from_non_dbContext_is_inlined();

            AssertSql(
                @"SELECT [b].[Id], [b].[IsEnabled]
FROM [StaticMemberFilter] AS [b]
WHERE [b].[IsEnabled] = 1");
        }

        public override void Local_variable_from_OnModelCreating_is_inlined()
        {
            base.Local_variable_from_OnModelCreating_is_inlined();

            AssertSql(
                @"SELECT [e].[Id], [e].[IsEnabled]
FROM [LocalVariableFilter] AS [e]
WHERE [e].[IsEnabled] = 1");
        }

        public override void Method_parameter_is_inlined()
        {
            base.Method_parameter_is_inlined();

            AssertSql(
                @"SELECT [e].[Id], [e].[Tenant]
FROM [ParameterFilter] AS [e]
WHERE [e].[Tenant] = 0");
        }

        public override void Using_multiple_context_in_filter_parametrize_only_current_context()
        {
            base.Using_multiple_context_in_filter_parametrize_only_current_context();

            AssertSql(
                @"@__ef_filter__Property_0='False'

SELECT [e].[Id], [e].[BossId], [e].[IsEnabled]
FROM [MultiContextFilter] AS [e]
WHERE ([e].[IsEnabled] = @__ef_filter__Property_0) AND ([e].[BossId] = 1)",
                //
                @"@__ef_filter__Property_0='True'

SELECT [e].[Id], [e].[BossId], [e].[IsEnabled]
FROM [MultiContextFilter] AS [e]
WHERE ([e].[IsEnabled] = @__ef_filter__Property_0) AND ([e].[BossId] = 1)");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class QueryFilterFuncletizationSqlServerFixture : QueryFilterFuncletizationRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
        }
    }
}
