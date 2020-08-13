// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryFilterFuncletizationContext : DbContext
    {
        public static int AdminId = 1;

        public QueryFilterFuncletizationContext(DbContextOptions options)
            : base(options)
        {
        }

        public bool Field;
        public bool Property { get; set; }
        public bool? IsModerated { get; set; }
        public int Tenant { get; set; }
        public int GetId() => 2;
        public Indirection GetFlag() => new Indirection();
        public List<int> TenantIds { get; set; }
        public Indirection IndirectionFlag { get; set; }

        public DbSet<DependentSetFilter> Dependents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Parametrize
            // Filters defined in OnModelCreating
            modelBuilder.Entity<FieldFilter>().HasQueryFilter(e => e.IsEnabled == Field);
            modelBuilder.Entity<PropertyFilter>().HasQueryFilter(e => e.IsEnabled == Property);
            modelBuilder.Entity<MethodCallFilter>().HasQueryFilter(e => e.Tenant == GetId());
            modelBuilder.Entity<ListFilter>().HasQueryFilter(e => TenantIds.Contains(e.Tenant));
            modelBuilder.Entity<PropertyChainFilter>().HasQueryFilter(e => e.IsEnabled == IndirectionFlag.Enabled);
            modelBuilder.Entity<PropertyMethodCallFilter>().HasQueryFilter(e => e.Tenant == IndirectionFlag.GetId());
            modelBuilder.Entity<MethodCallChainFilter>().HasQueryFilter(e => e.Tenant == GetFlag().GetId());
            modelBuilder.Entity<ComplexFilter>().HasQueryFilter(x => x.IsEnabled == Property && (Tenant + GetId() > 0));
            modelBuilder.Entity<ShortCircuitFilter>()
                .HasQueryFilter(x => !x.IsDeleted && (IsModerated == null || IsModerated == x.IsModerated));
            modelBuilder.Entity<PrincipalSetFilter>()
                .HasQueryFilter(p => Dependents.Any(d => d.PrincipalSetFilterId == p.Id));

            // Filters defined through EntityTypeConfiguration
            modelBuilder.ApplyConfiguration(new FieldConfiguration(this));
            modelBuilder.ApplyConfiguration(new PropertyConfiguration(this));
            modelBuilder.ApplyConfiguration(new MethodCallConfiguration(new DbContextWrapper(this)));
            modelBuilder.ApplyConfiguration(new PropertyChainConfiguration(new DbContextWrapper(this)));

            // Filters defined through methods (local/remote/extensions)
            ConfigureFilter(modelBuilder.Entity<LocalMethodFilter>());
            ConfigureFilterParams(modelBuilder.Entity<LocalMethodParamsFilter>(), this);
            Indirection.ConfigureFilter(modelBuilder.Entity<RemoteMethodParamsFilter>(), new DbContextWrapper(this));
            modelBuilder.Entity<ExtensionBuilderFilter>().BuilderFilter(this);
            new DbContextWrapper(this).ContextFilter(modelBuilder.Entity<ExtensionContextFilter>());
            SetDependentFilter(modelBuilder, this);

            // Inline
            modelBuilder.Entity<DbContextStaticMemberFilter>().HasQueryFilter(e => e.UserId != AdminId);
            modelBuilder.Entity<StaticMemberFilter>()
                .HasQueryFilter(b => b.IsEnabled == StaticMemberFilter.DefaultEnabled);
            var enabled = true;
            modelBuilder.Entity<LocalVariableFilter>().HasQueryFilter(e => e.IsEnabled == enabled);
            Indirection flag = null;
            modelBuilder.Entity<LocalVariableErrorFilter>().HasQueryFilter(e => e.IsEnabled == flag.Enabled);
            IncorrectFilter(modelBuilder.Entity<ParameterFilter>(), Tenant);

            // Multiple context used in filter
            modelBuilder.Entity<MultiContextFilter>()
                .HasQueryFilter(e => e.IsEnabled == Property && e.BossId == new IncorrectDbContext().BossId);
        }

        private void ConfigureFilter(EntityTypeBuilder<LocalMethodFilter> builder)
        {
            builder.HasQueryFilter(e => e.IsEnabled == Field);
        }

        private static void ConfigureFilterParams(
            EntityTypeBuilder<LocalMethodParamsFilter> builder, QueryFilterFuncletizationContext context)
        {
            builder.HasQueryFilter(e => e.IsEnabled == context.Property);
        }

        private static void IncorrectFilter(EntityTypeBuilder<ParameterFilter> builder, int tenant)
        {
            builder.HasQueryFilter(e => e.Tenant == tenant);
        }

        private static void SetDependentFilter(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<DependentSetFilter>()
                .HasQueryFilter(p => context.Set<MultiContextFilter>().Any(b => b.BossId == p.PrincipalSetFilterId));
        }

        #region EntityTypeConfigs

        public class FieldConfiguration : IEntityTypeConfiguration<EntityTypeConfigurationFieldFilter>
        {
            public FieldConfiguration(QueryFilterFuncletizationContext context)
            {
                Context = context;
            }

            public QueryFilterFuncletizationContext Context { get; }

            public void Configure(EntityTypeBuilder<EntityTypeConfigurationFieldFilter> builder)
            {
                builder.HasQueryFilter(e => e.IsEnabled == Context.Field);
            }
        }

        public class PropertyConfiguration : IEntityTypeConfiguration<EntityTypeConfigurationPropertyFilter>
        {
            private readonly QueryFilterFuncletizationContext _context;

            public PropertyConfiguration(QueryFilterFuncletizationContext context)
            {
                _context = context;
            }

            public void Configure(EntityTypeBuilder<EntityTypeConfigurationPropertyFilter> builder)
            {
                builder.HasQueryFilter(e => e.IsEnabled == _context.Property);
            }
        }

        public class MethodCallConfiguration : IEntityTypeConfiguration<EntityTypeConfigurationMethodCallFilter>
        {
            public MethodCallConfiguration(DbContextWrapper wrapper)
            {
                Wrapper = wrapper;
            }

            public DbContextWrapper Wrapper { get; }

            public void Configure(EntityTypeBuilder<EntityTypeConfigurationMethodCallFilter> builder)
            {
                builder.HasQueryFilter(e => e.Tenant == Wrapper.Context.GetId());
            }
        }

        public class PropertyChainConfiguration : IEntityTypeConfiguration<EntityTypeConfigurationPropertyChainFilter>
        {
            private readonly DbContextWrapper _wrapper;

            public PropertyChainConfiguration(DbContextWrapper wrapper)
            {
                _wrapper = wrapper;
            }

            public void Configure(EntityTypeBuilder<EntityTypeConfigurationPropertyChainFilter> builder)
            {
                builder.HasQueryFilter(e => e.IsEnabled == _wrapper.Context.IndirectionFlag.Enabled);
            }
        }

        #endregion

        public static void SeedData(QueryFilterFuncletizationContext context)
        {
            context.AddRange(
                new FieldFilter { IsEnabled = true },
                new FieldFilter { IsEnabled = false },
                new PropertyFilter { IsEnabled = true },
                new PropertyFilter { IsEnabled = false },
                new MethodCallFilter { Tenant = 1 },
                new MethodCallFilter { Tenant = 2 },
                new ListFilter { Tenant = 1 },
                new ListFilter { Tenant = 2 },
                new ListFilter { Tenant = 3 },
                new ListFilter { Tenant = 4 },
                new PropertyChainFilter { IsEnabled = true },
                new PropertyChainFilter { IsEnabled = false },
                new PropertyMethodCallFilter { Tenant = 1 },
                new PropertyMethodCallFilter { Tenant = 2 },
                new MethodCallChainFilter { Tenant = 1 },
                new MethodCallChainFilter { Tenant = 2 },
                new ComplexFilter { IsEnabled = true },
                new ComplexFilter { IsEnabled = false },
                new ShortCircuitFilter { IsDeleted = false, IsModerated = false },
                new ShortCircuitFilter { IsDeleted = true, IsModerated = false },
                new ShortCircuitFilter { IsDeleted = false, IsModerated = true },
                new ShortCircuitFilter { IsDeleted = true, IsModerated = true },
                new DbContextStaticMemberFilter { UserId = 1 },
                new DbContextStaticMemberFilter { UserId = 2 },
                new DbContextStaticMemberFilter { UserId = 3 },
                new StaticMemberFilter { IsEnabled = true },
                new StaticMemberFilter { IsEnabled = false },
                new LocalVariableFilter { IsEnabled = true },
                new LocalVariableFilter { IsEnabled = false },
                new EntityTypeConfigurationFieldFilter { IsEnabled = true },
                new EntityTypeConfigurationFieldFilter { IsEnabled = false },
                new EntityTypeConfigurationPropertyFilter { IsEnabled = true },
                new EntityTypeConfigurationPropertyFilter { IsEnabled = false },
                new EntityTypeConfigurationMethodCallFilter { Tenant = 1 },
                new EntityTypeConfigurationMethodCallFilter { Tenant = 2 },
                new EntityTypeConfigurationPropertyChainFilter { IsEnabled = true },
                new EntityTypeConfigurationPropertyChainFilter { IsEnabled = false },
                new LocalMethodFilter { IsEnabled = true },
                new LocalMethodFilter { IsEnabled = false },
                new LocalMethodParamsFilter { IsEnabled = true },
                new LocalMethodParamsFilter { IsEnabled = false },
                new RemoteMethodParamsFilter { Tenant = 1 },
                new RemoteMethodParamsFilter { Tenant = 2 },
                new ExtensionBuilderFilter { IsEnabled = true },
                new ExtensionBuilderFilter { IsEnabled = false },
                new ExtensionContextFilter { IsEnabled = true },
                new ExtensionContextFilter { IsEnabled = false },
                new ParameterFilter { Tenant = 1 },
                new ParameterFilter { Tenant = 2 },
                new PrincipalSetFilter { Dependents = new List<DependentSetFilter> { new DependentSetFilter(), new DependentSetFilter() } },
                new PrincipalSetFilter(),
                new MultiContextFilter { BossId = 1, IsEnabled = true },
                new MultiContextFilter { BossId = 1, IsEnabled = false },
                new MultiContextFilter { BossId = 1, IsEnabled = true },
                new MultiContextFilter { BossId = 2, IsEnabled = true },
                new MultiContextFilter { BossId = 2, IsEnabled = false }
            );

            context.SaveChanges();
        }
    }

    #region HelperClasses

    public class DbContextWrapper
    {
        public DbContextWrapper(QueryFilterFuncletizationContext context)
        {
            Context = context;
        }

        public QueryFilterFuncletizationContext Context { get; }
    }

    public static class FilterExtensions
    {
        public static void BuilderFilter(
            this EntityTypeBuilder<ExtensionBuilderFilter> builder, QueryFilterFuncletizationContext context)
        {
            builder.HasQueryFilter(e => e.IsEnabled == context.Field);
        }

        public static void ContextFilter(
            this DbContextWrapper wrapper, EntityTypeBuilder<ExtensionContextFilter> builder)
        {
            builder.HasQueryFilter(e => e.IsEnabled == wrapper.Context.IndirectionFlag.Enabled);
        }
    }

    public class Indirection
    {
        public bool Enabled { get; set; }
        public int GetId() => 2;

        public static void ConfigureFilter(EntityTypeBuilder<RemoteMethodParamsFilter> builder, DbContextWrapper wrapper)
        {
            builder.HasQueryFilter(e => e.Tenant == wrapper.Context.IndirectionFlag.GetId());
        }
    }

    public class IncorrectDbContext : DbContext
    {
        public int BossId => 1;
    }

    #endregion

    #region EntityTypes

    public class FieldFilter
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class PropertyFilter
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class MethodCallFilter
    {
        public int Id { get; set; }
        public int Tenant { get; set; }
    }

    public class ListFilter
    {
        public int Id { get; set; }
        public int Tenant { get; set; }
    }

    public class PropertyChainFilter
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class PropertyMethodCallFilter
    {
        public int Id { get; set; }
        public int Tenant { get; set; }
    }

    public class MethodCallChainFilter
    {
        public int Id { get; set; }
        public int Tenant { get; set; }
    }

    public class ComplexFilter
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class ShortCircuitFilter
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsModerated { get; set; }
    }

    public class EntityTypeConfigurationFieldFilter
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class EntityTypeConfigurationPropertyFilter
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class EntityTypeConfigurationMethodCallFilter
    {
        public int Id { get; set; }
        public int Tenant { get; set; }
    }

    public class EntityTypeConfigurationPropertyChainFilter
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class LocalMethodFilter
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class LocalMethodParamsFilter
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class RemoteMethodParamsFilter
    {
        public int Id { get; set; }
        public int Tenant { get; set; }
    }

    public class ExtensionBuilderFilter
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class ExtensionContextFilter
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class DbContextStaticMemberFilter
    {
        public int Id { get; set; }
        public int UserId { get; set; }
    }

    public class StaticMemberFilter
    {
        public static bool DefaultEnabled = true;
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class LocalVariableFilter
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class LocalVariableErrorFilter
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class ParameterFilter
    {
        public int Id { get; set; }
        public int Tenant { get; set; }
    }

    public class PrincipalSetFilter
    {
        public int Id { get; set; }
        public bool Filler { get; set; }
        public ICollection<DependentSetFilter> Dependents { get; set; }
    }

    public class DependentSetFilter
    {
        public int Id { get; set; }
        public int PrincipalSetFilterId { get; set; }
    }

    public class MultiContextFilter
    {
        public int Id { get; set; }
        public int BossId { get; set; }
        public bool IsEnabled { get; set; }
    }

    #endregion
}
