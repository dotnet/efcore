// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

// ReSharper disable AccessToDisposedClosure
// ReSharper disable PossibleNullReferenceException
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class QueryFilterFuncletizationTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : QueryFilterFuncletizationFixtureBase, new()
    {
        protected QueryFilterFuncletizationTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        protected QueryFilterFuncletizationContext CreateContext() => Fixture.CreateContext();

        [ConditionalFact]
        public virtual void DbContext_property_parameter_does_not_clash_with_closure_parameter_name()
        {
            using (var context = CreateContext())
            {
                var Field = false;
                Assert.Single(context.Set<FieldFilter>().Where(e => e.IsEnabled == Field).ToList());
            }
        }

        [ConditionalFact]
        public virtual void DbContext_field_is_parameterized()
        {
            using (var context = CreateContext())
            {
                var entity = Assert.Single(context.Set<FieldFilter>().ToList());
                Assert.False(entity.IsEnabled);

                context.Field = true;

                entity = Assert.Single(context.Set<FieldFilter>().ToList());
                Assert.True(entity.IsEnabled);
            }
        }

        [ConditionalFact]
        public virtual void DbContext_property_is_parameterized()
        {
            using (var context = CreateContext())
            {
                var entity = Assert.Single(context.Set<PropertyFilter>().ToList());
                Assert.False(entity.IsEnabled);

                context.Property = true;

                entity = Assert.Single(context.Set<PropertyFilter>().ToList());
                Assert.True(entity.IsEnabled);
            }
        }

        [ConditionalFact]
        public virtual void DbContext_method_call_is_parameterized()
        {
            using (var context = CreateContext())
            {
                var entity = Assert.Single(context.Set<MethodCallFilter>().ToList());
                Assert.Equal(2, entity.Tenant);
            }
        }

        [ConditionalFact]
        public virtual void DbContext_list_is_parameterized()
        {
            using (var context = CreateContext())
            {
                // This throws because the default value of TenantIds is null which is NRE
                Assert.Throws<NullReferenceException>(() => context.Set<ListFilter>().ToList());

                context.TenantIds = new List<int>();
                var query = context.Set<ListFilter>().ToList();
                Assert.Empty(query);

                context.TenantIds = new List<int> { 1 };
                query = context.Set<ListFilter>().ToList();
                Assert.Single(query);

                context.TenantIds = new List<int> { 2, 3 };
                query = context.Set<ListFilter>().ToList();
                Assert.Equal(2, query.Count);
            }
        }

        [ConditionalFact]
        public virtual void DbContext_property_chain_is_parameterized()
        {
            using (var context = CreateContext())
            {
                // This throws because IndirectionFlag is null
                Assert.Throws<NullReferenceException>(() => context.Set<PropertyChainFilter>().ToList());

                context.IndirectionFlag = new Indirection { Enabled = false };
                var entity = Assert.Single(context.Set<PropertyChainFilter>().ToList());
                Assert.False(entity.IsEnabled);

                context.IndirectionFlag = new Indirection { Enabled = true };
                entity = Assert.Single(context.Set<PropertyChainFilter>().ToList());
                Assert.True(entity.IsEnabled);
            }
        }

        [ConditionalFact]
        public virtual void DbContext_property_method_call_is_parameterized()
        {
            using (var context = CreateContext())
            {
                // This throws because IndirectionFlag is null
                Assert.Throws<NullReferenceException>(() => context.Set<PropertyMethodCallFilter>().ToList());

                context.IndirectionFlag = new Indirection();
                var entity = Assert.Single(context.Set<PropertyMethodCallFilter>().ToList());
                Assert.Equal(2, entity.Tenant);
            }
        }

        [ConditionalFact]
        public virtual void DbContext_method_call_chain_is_parameterized()
        {
            using (var context = CreateContext())
            {
                var entity = Assert.Single(context.Set<MethodCallChainFilter>().ToList());
                Assert.Equal(2, entity.Tenant);
            }
        }

        [ConditionalFact]
        public virtual void DbContext_complex_expression_is_parameterized()
        {
            using (var context = CreateContext())
            {
                var entity = Assert.Single(context.Set<ComplexFilter>().ToList());
                Assert.False(entity.IsEnabled);

                context.Property = true;

                entity = Assert.Single(context.Set<ComplexFilter>().ToList());
                Assert.True(entity.IsEnabled);

                context.Tenant = -3;
                Assert.Empty(context.Set<ComplexFilter>().ToList());
            }
        }

        [ConditionalFact]
        public virtual void DbContext_property_based_filter_does_not_short_circuit()
        {
            using (var context = CreateContext())
            {
                context.IsModerated = true;
                Assert.Single(context.Set<ShortCircuitFilter>().ToList());

                context.IsModerated = false;
                Assert.Single(context.Set<ShortCircuitFilter>().ToList());

                context.IsModerated = null;
                var query = context.Set<ShortCircuitFilter>().ToList();
                Assert.Equal(2, query.Count);
            }
        }

        [ConditionalFact]
        public virtual void EntityTypeConfiguration_DbContext_field_is_parameterized()
        {
            using (var context = CreateContext())
            {
                var entity = Assert.Single(context.Set<EntityTypeConfigurationFieldFilter>().ToList());
                Assert.False(entity.IsEnabled);

                context.Field = true;

                entity = Assert.Single(context.Set<EntityTypeConfigurationFieldFilter>().ToList());
                Assert.True(entity.IsEnabled);
            }
        }

        [ConditionalFact]
        public virtual void EntityTypeConfiguration_DbContext_property_is_parameterized()
        {
            using (var context = CreateContext())
            {
                var entity = Assert.Single(context.Set<EntityTypeConfigurationPropertyFilter>().ToList());
                Assert.False(entity.IsEnabled);

                context.Property = true;

                entity = Assert.Single(context.Set<EntityTypeConfigurationPropertyFilter>().ToList());
                Assert.True(entity.IsEnabled);
            }
        }

        [ConditionalFact]
        public virtual void EntityTypeConfiguration_DbContext_method_call_is_parameterized()
        {
            using (var context = CreateContext())
            {
                var entity = Assert.Single(context.Set<EntityTypeConfigurationMethodCallFilter>().ToList());
                Assert.Equal(2, entity.Tenant);
            }
        }

        [ConditionalFact]
        public virtual void EntityTypeConfiguration_DbContext_property_chain_is_parameterized()
        {
            using (var context = CreateContext())
            {
                // This throws because IndirectionFlag is null
                Assert.Throws<NullReferenceException>(() => context.Set<EntityTypeConfigurationPropertyChainFilter>().ToList());

                context.IndirectionFlag = new Indirection { Enabled = false };
                var entity = Assert.Single(context.Set<EntityTypeConfigurationPropertyChainFilter>().ToList());
                Assert.False(entity.IsEnabled);

                context.IndirectionFlag = new Indirection { Enabled = true };
                entity = Assert.Single(context.Set<EntityTypeConfigurationPropertyChainFilter>().ToList());
                Assert.True(entity.IsEnabled);
            }
        }

        [ConditionalFact]
        public virtual void Local_method_DbContext_field_is_parameterized()
        {
            using (var context = CreateContext())
            {
                var entity = Assert.Single(context.Set<LocalMethodFilter>().ToList());
                Assert.False(entity.IsEnabled);

                context.Field = true;

                entity = Assert.Single(context.Set<LocalMethodFilter>().ToList());
                Assert.True(entity.IsEnabled);
            }
        }

        [ConditionalFact]
        public virtual void Local_static_method_DbContext_property_is_parameterized()
        {
            using (var context = CreateContext())
            {
                var entity = Assert.Single(context.Set<LocalMethodParamsFilter>().ToList());
                Assert.False(entity.IsEnabled);

                context.Property = true;

                entity = Assert.Single(context.Set<LocalMethodParamsFilter>().ToList());
                Assert.True(entity.IsEnabled);
            }
        }

        [ConditionalFact]
        public virtual void Remote_method_DbContext_property_method_call_is_parameterized()
        {
            using (var context = CreateContext())
            {
                // This throws because IndirectionFlag is null
                Assert.Throws<NullReferenceException>(() => context.Set<RemoteMethodParamsFilter>().ToList());

                context.IndirectionFlag = new Indirection();
                var entity = Assert.Single(context.Set<RemoteMethodParamsFilter>().ToList());
                Assert.Equal(2, entity.Tenant);
            }
        }

        [ConditionalFact]
        public virtual void Extension_method_DbContext_field_is_parameterized()
        {
            using (var context = CreateContext())
            {
                var entity = Assert.Single(context.Set<ExtensionBuilderFilter>().ToList());
                Assert.False(entity.IsEnabled);

                context.Field = true;

                entity = Assert.Single(context.Set<ExtensionBuilderFilter>().ToList());
                Assert.True(entity.IsEnabled);
            }
        }

        [ConditionalFact]
        public virtual void Extension_method_DbContext_property_chain_is_parameterized()
        {
            using (var context = CreateContext())
            {
                // This throws because IndirectionFlag is null
                Assert.Throws<NullReferenceException>(() => context.Set<ExtensionContextFilter>().ToList());

                context.IndirectionFlag = new Indirection { Enabled = false };
                var entity = Assert.Single(context.Set<ExtensionContextFilter>().ToList());
                Assert.False(entity.IsEnabled);

                context.IndirectionFlag = new Indirection { Enabled = true };
                entity = Assert.Single(context.Set<ExtensionContextFilter>().ToList());
                Assert.True(entity.IsEnabled);
            }
        }

        [ConditionalFact]
        public virtual void Using_DbSet_in_filter_works()
        {
            using (var context = CreateContext())
            {
                Assert.Single(context.Set<PrincipalSetFilter>().ToList());
            }
        }

        [ConditionalFact]
        public virtual void Using_Context_set_method_in_filter_works()
        {
            using (var context = CreateContext())
            {
                var query = context.Dependents.ToList();

                Assert.Equal(2, query.Count);
            }
        }

        [ConditionalFact]
        public virtual void Static_member_from_dbContext_is_inlined()
        {
            using (var context = CreateContext())
            {
                var query = context.Set<DbContextStaticMemberFilter>().ToList();

                Assert.Equal(2, query.Count);
            }
        }

        [ConditionalFact]
        public virtual void Static_member_from_non_dbContext_is_inlined()
        {
            using (var context = CreateContext())
            {
                var query = context.Set<StaticMemberFilter>().ToList();

                var entity = Assert.Single(query);
            }
        }

        [ConditionalFact]
        public virtual void Local_variable_from_OnModelCreating_is_inlined()
        {
            using (var context = CreateContext())
            {
                var query = context.Set<LocalVariableFilter>().ToList();

                var entity = Assert.Single(query);
            }
        }

        [ConditionalFact]
        public virtual void Local_variable_from_OnModelCreating_can_throw_exception()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.ExpressionParameterizationExceptionSensitive(
                        "value(Microsoft.EntityFrameworkCore.Query.QueryFilterFuncletizationContext+<>c__DisplayClass29_0).flag.Enabled"),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Set<LocalVariableErrorFilter>().ToList()).Message);
            }
        }

        [ConditionalFact]
        public virtual void Method_parameter_is_inlined()
        {
            using (var context = CreateContext())
            {
                Assert.Empty(context.Set<ParameterFilter>().ToList());
            }
        }

        [ConditionalFact]
        public virtual void Using_multiple_context_in_filter_parametrize_only_current_context()
        {
            using (var context = CreateContext())
            {
                var query = context.Set<MultiContextFilter>().ToList();
                Assert.Single(query);

                context.Property = true;

                query = context.Set<MultiContextFilter>().ToList();
                Assert.Equal(2, query.Count);
            }
        }
    }
}
