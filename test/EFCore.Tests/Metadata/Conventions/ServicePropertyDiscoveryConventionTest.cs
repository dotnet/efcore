// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class ServicePropertyDiscoveryConventionTest
    {
        [ConditionalFact]
        public void Finds_service_properties_in_hierarchy()
        {
            using (var context = new ServicePropertiesContext())
            {
                var entityTypes = context.Model.GetEntityTypes().ToList();
                Assert.Equal(13, entityTypes.Count);

                foreach (var entityType in entityTypes)
                {
                    ValidateServiceProperty<DbContext, DbContext>(entityType, "Context");
                    ValidateServiceProperty<DbContext, DbContext>(entityType, "Context2");
                    ValidateServiceProperty<IEntityType, IEntityType>(entityType, "EntityType");
                    ValidateServiceProperty<IEntityType, IEntityType>(entityType, "EntityType2");
                    ValidateServiceProperty<ILazyLoader, ILazyLoader>(entityType, "ALazyLoader");
                    ValidateServiceProperty<ILazyLoader, ILazyLoader>(entityType, "ALazyLoader2");
                    ValidateServiceProperty<Action<object, string>, ILazyLoader>(entityType, "LazyLoader");

                    var clrType = entityType.ClrType;
                    while (clrType!.BaseType != typeof(object))
                    {
                        clrType = clrType.BaseType;
                    }

                    var contextProperty = clrType.GetAnyProperty("Context");
                    var context2Property = clrType.GetAnyProperty("Context2");
                    var entityTypeProperty = clrType.GetAnyProperty("EntityType");
                    var entityType2Property = clrType.GetAnyProperty("EntityType2");
                    var lazyLoaderProperty = clrType.GetAnyProperty("ALazyLoader");
                    var lazyLoader2Property = clrType.GetAnyProperty("ALazyLoader2");
                    var lazyLoaderServiceProperty = clrType.GetAnyProperty("LazyLoader");

                    var entity = Activator.CreateInstance(entityType.ClrType);

                    Assert.Null(contextProperty!.GetValue(entity));
                    Assert.Null(context2Property!.GetValue(entity));
                    Assert.Null(entityTypeProperty!.GetValue(entity));
                    Assert.Null(entityType2Property!.GetValue(entity));
                    Assert.Null(lazyLoaderProperty!.GetValue(entity));
                    Assert.Null(lazyLoader2Property!.GetValue(entity));
                    Assert.Null(lazyLoaderServiceProperty!.GetValue(entity));

                    context.Add(entity!);

                    Assert.Same(context, contextProperty!.GetValue(entity));
                    Assert.Same(context, context2Property!.GetValue(entity));
                    Assert.Same(entityType, entityTypeProperty!.GetValue(entity));
                    Assert.Same(entityType, entityType2Property!.GetValue(entity));
                    Assert.NotNull(lazyLoaderProperty!.GetValue(entity));
                    Assert.NotNull(lazyLoader2Property!.GetValue(entity));
                    Assert.NotNull(lazyLoaderServiceProperty!.GetValue(entity));
                }

                context.SaveChanges();
            }

            using (var context = new ServicePropertiesContext())
            {
                context.PrivateUnmappedBaseSupers.Load();
                context.PrivateMappedBases.Load();
                context.PublicUnmappedBaseSupers.Load();
                context.PublicMappedBases.Load();

                Assert.Equal(10, context.ChangeTracker.Entries().Count());

                foreach (var entry in context.ChangeTracker.Entries())
                {
                    var clrType = entry.Metadata.ClrType;

                    if (!clrType.Name.StartsWith("PrivateWithDuplicates", StringComparison.Ordinal))
                    {
                        while (clrType!.BaseType != typeof(object))
                        {
                            clrType = clrType.BaseType;
                        }
                    }

                    if (clrType == typeof(PublicUnmappedBaseSuper))
                    {
                        Assert.True(((PublicUnmappedBase)entry.Entity).ConstructorCalled);
                    }

                    Assert.Same(context, clrType.GetAnyProperty("Context")!.GetValue(entry.Entity));
                    Assert.Same(context, clrType.GetAnyProperty("Context2")!.GetValue(entry.Entity));
                    Assert.Same(entry.Metadata, clrType.GetAnyProperty("EntityType")!.GetValue(entry.Entity));
                    Assert.Same(entry.Metadata, clrType.GetAnyProperty("EntityType2")!.GetValue(entry.Entity));
                    Assert.NotNull(clrType.GetAnyProperty("ALazyLoader")!.GetValue(entry.Entity));
                    Assert.NotNull(clrType.GetAnyProperty("ALazyLoader2")!.GetValue(entry.Entity));
                    Assert.NotNull(clrType.GetAnyProperty("LazyLoader")!.GetValue(entry.Entity));
                }
            }
        }

        private static void ValidateServiceProperty<TProperty, TService>(IEntityType entityType, string propertyName)
        {
            var serviceProperty = entityType!.FindServiceProperty(propertyName);
            var binding = serviceProperty!.ParameterBinding;

            Assert.Equal(typeof(TProperty), binding.ParameterType);
            Assert.Equal(typeof(TService), binding.ServiceType);
        }

        [ConditionalFact]
        public void Finds_one_service_property()
        {
            var entityType = RunConvention<BlogOneService>();

            var serviceProperty = entityType.FindServiceProperty(nameof(BlogOneService.Loader));
            var binding = serviceProperty!.ParameterBinding;

            Assert.Equal(typeof(ILazyLoader), binding!.ParameterType);
            Assert.Equal(typeof(ILazyLoader), binding.ServiceType);
        }

        [ConditionalFact]
        public void Does_not_find_service_property_configured_as_property()
        {
            var entityType = new Model().AddEntityType(typeof(BlogOneService), owned: false, ConfigurationSource.Explicit);
            entityType!.Builder.Property(typeof(ILazyLoader), nameof(BlogOneService.Loader), ConfigurationSource.Explicit)
                !.HasConversion(typeof(string), ConfigurationSource.Explicit);

            RunConvention(entityType);

            Assert.NotNull(entityType.FindProperty(nameof(BlogOneService.Loader)));
            Assert.Null(entityType.FindServiceProperty(nameof(BlogOneService.Loader)));
        }

        [ConditionalFact]
        public void Does_not_find_service_property_configured_as_navigation()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(BlogOneService), owned: false, ConfigurationSource.Explicit);
            entityType!.Builder.HasRelationship(
                model.AddEntityType(typeof(LazyLoader), owned: false, ConfigurationSource.Explicit)!,
                nameof(BlogOneService.Loader), ConfigurationSource.Explicit);

            RunConvention(entityType);

            Assert.NotNull(entityType.FindNavigation(nameof(BlogOneService.Loader)));
            Assert.Null(entityType.FindServiceProperty(nameof(BlogOneService.Loader)));
        }

        [ConditionalFact]
        public void Finds_service_property_duplicate_ignored()
        {
            var entityType = RunConvention<BlogDuplicateService>();

            entityType.Builder.Ignore(nameof(BlogDuplicateService.ContextTwo), ConfigurationSource.Convention);

            Assert.NotNull(entityType.FindServiceProperty(nameof(BlogDuplicateService.ContextOne)));
            Assert.Null(entityType.FindServiceProperty(nameof(BlogDuplicateService.ContextTwo)));
        }

        private EntityType RunConvention<TEntity>()
            => RunConvention(new Model().AddEntityType(typeof(TEntity), owned: false, ConfigurationSource.Explicit)!);

        private EntityType RunConvention(EntityType entityType)
        {
            entityType.AddProperty(nameof(Blog.Id), typeof(int), ConfigurationSource.Explicit, ConfigurationSource.Explicit);

            var context = new ConventionContext<IConventionEntityTypeBuilder>(entityType.Model.ConventionDispatcher);
            CreateServicePropertyDiscoveryConvention().ProcessEntityTypeAdded(entityType.Builder, context);

            return context.ShouldStopProcessing() ? (EntityType)context.Result!.Metadata : entityType;
        }

        private ServicePropertyDiscoveryConvention CreateServicePropertyDiscoveryConvention()
            => new ServicePropertyDiscoveryConvention(CreateDependencies());

        private ProviderConventionSetBuilderDependencies CreateDependencies()
            => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

        private class BlogOneService : Blog
        {
            public ILazyLoader? Loader { get; set; }
        }

        private class BlogDuplicateService : Blog
        {
            public DbContext? ContextOne { get; set; }
            public DbContext? ContextTwo { get; set; }
        }

        private abstract class Blog
        {
            public int Id { get; set; }
        }

        private class ServicePropertiesContext : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseInMemoryDatabase(GetType().Name);

            public DbSet<PrivateUnmappedBaseSuper> PrivateUnmappedBaseSupers
                => Set<PrivateUnmappedBaseSuper>();

            public DbSet<PrivateUnmappedBaseSub> PrivateUnmappedBaseSubs
                => Set<PrivateUnmappedBaseSub>();

            public DbSet<PrivateMappedBase> PrivateMappedBases
                => Set<PrivateMappedBase>();

            public DbSet<PrivateMappedBaseSuper> PrivateMappedBaseSupers
                => Set<PrivateMappedBaseSuper>();

            public DbSet<PrivateMappedBaseSub> PrivateMappedBaseSubs
                => Set<PrivateMappedBaseSub>();

            public DbSet<PublicUnmappedBaseSuper> PublicUnmappedBaseSupers
                => Set<PublicUnmappedBaseSuper>();

            public DbSet<PublicUnmappedBaseSub> PublicUnmappedBaseSubs
                => Set<PublicUnmappedBaseSub>();

            public DbSet<PublicMappedBase> PublicMappedBases
                => Set<PublicMappedBase>();

            public DbSet<PublicMappedBaseSuper> PublicMappedBaseSupers
                => Set<PublicMappedBaseSuper>();

            public DbSet<PublicMappedBaseSub> PublicMappedBaseSubs
                => Set<PublicMappedBaseSub>();

            public DbSet<PrivateWithDuplicatesBase> PrivateWithDuplicatesBases
                => Set<PrivateWithDuplicatesBase>();

            public DbSet<PrivateWithDuplicatesSuper> PrivateWithDuplicatesSupers
                => Set<PrivateWithDuplicatesSuper>();

            public DbSet<PrivateWithDuplicatesSub> PrivateWithDuplicatesSubs
                => Set<PrivateWithDuplicatesSub>();

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<PrivateUnmappedBaseSuper>(
                    b =>
                    {
                        // Because private properties on un-mapped base types are not found by convention
                        b.Metadata.AddServiceProperty(typeof(PrivateUnmappedBase).GetAnyProperty("Context")!);
                        b.Metadata.AddServiceProperty(typeof(PrivateUnmappedBase).GetAnyProperty("Context2")!);
                        b.Metadata.AddServiceProperty(typeof(PrivateUnmappedBase).GetAnyProperty("EntityType")!);
                        b.Metadata.AddServiceProperty(typeof(PrivateUnmappedBase).GetAnyProperty("EntityType2")!);
                        b.Metadata.AddServiceProperty(typeof(PrivateUnmappedBase).GetAnyProperty("ALazyLoader")!);
                        b.Metadata.AddServiceProperty(typeof(PrivateUnmappedBase).GetAnyProperty("ALazyLoader2")!);
                        b.Metadata.AddServiceProperty(typeof(PrivateUnmappedBase).GetAnyProperty("LazyLoader")!);
                    });
            }
        }

        protected class PrivateUnmappedBase
        {
            public int Id { get; set; }
            private DbContext? Context { get; set; }
            private DbContext? Context2 { get; set; }
            private IEntityType? EntityType { get; set; }
            private IEntityType? EntityType2 { get; set; }
            private ILazyLoader? ALazyLoader { get; set; }
            private ILazyLoader? ALazyLoader2 { get; set; }
            private Action<object, string>? LazyLoader { get; set; }
        }

        protected class PrivateUnmappedBaseSuper : PrivateUnmappedBase
        {
        }

        protected class PrivateUnmappedBaseSub : PrivateUnmappedBaseSuper
        {
        }

        protected class PrivateMappedBase
        {
            public int Id { get; set; }
            private DbContext? Context { get; set; }
            private DbContext? Context2 { get; set; }
            private IEntityType? EntityType { get; set; }
            private IEntityType? EntityType2 { get; set; }
            private ILazyLoader? ALazyLoader { get; set; }
            private ILazyLoader? ALazyLoader2 { get; set; }
            private Action<object, string>? LazyLoader { get; set; }
        }

        protected class PrivateMappedBaseSuper : PrivateMappedBase
        {
        }

        protected class PrivateMappedBaseSub : PrivateMappedBaseSuper
        {
        }

        protected class PublicUnmappedBase
        {
            public PublicUnmappedBase()
            {
            }

            public PublicUnmappedBase(
                int id,
                DbContext? context,
                DbContext? context2,
                IEntityType? entityType,
                IEntityType? entityType2,
                ILazyLoader? aLazyLoader,
                ILazyLoader? aLazyLoader2,
                Action<object, string>? lazyLoader)
            {
                Id = id;
                Context = context;
                Context2 = context2;
                EntityType = entityType;
                EntityType2 = entityType2;
                ALazyLoader = aLazyLoader;
                ALazyLoader2 = aLazyLoader2;
                LazyLoader = lazyLoader;

                Assert.NotNull(context);
                Assert.NotNull(context2);
                Assert.NotNull(entityType);
                Assert.NotNull(entityType2);
                Assert.NotNull(aLazyLoader);
                Assert.NotNull(aLazyLoader2);
                Assert.NotNull(lazyLoader);

                ConstructorCalled = true;
            }

            public int Id { get; set; }
            public DbContext? Context { get; set; }
            public DbContext? Context2 { get; set; }
            public IEntityType? EntityType { get; set; }
            public IEntityType? EntityType2 { get; set; }
            public ILazyLoader? ALazyLoader { get; set; }
            public ILazyLoader? ALazyLoader2 { get; set; }
            public Action<object, string>? LazyLoader { get; set; }

            [NotMapped]
            public bool ConstructorCalled { get; }
        }

        protected class PublicUnmappedBaseSuper : PublicUnmappedBase
        {
            public PublicUnmappedBaseSuper()
            {
            }

            public PublicUnmappedBaseSuper(
                int id,
                DbContext? context,
                DbContext? context2,
                IEntityType? entityType,
                IEntityType? entityType2,
                ILazyLoader? aLazyLoader,
                ILazyLoader? aLazyLoader2,
                Action<object, string>? lazyLoader)
                : base(id, context, context2, entityType, entityType2, aLazyLoader, aLazyLoader2, lazyLoader)
            {
            }
        }

        protected class PublicUnmappedBaseSub : PublicUnmappedBaseSuper
        {
            public PublicUnmappedBaseSub()
            {
            }

            public PublicUnmappedBaseSub(
                int id,
                DbContext? context,
                DbContext? context2,
                IEntityType? entityType,
                IEntityType? entityType2,
                ILazyLoader? aLazyLoader,
                ILazyLoader? aLazyLoader2,
                Action<object, string>? lazyLoader)
                : base(id, context, context2, entityType, entityType2, aLazyLoader, aLazyLoader2, lazyLoader)
            {
            }
        }

        protected class PublicMappedBase
        {
            public int Id { get; set; }
            public DbContext? Context { get; set; }
            public DbContext? Context2 { get; set; }
            public IEntityType? EntityType { get; set; }
            public IEntityType? EntityType2 { get; set; }
            public ILazyLoader? ALazyLoader { get; set; }
            public ILazyLoader? ALazyLoader2 { get; set; }
            public Action<object, string>? LazyLoader { get; set; }
        }

        protected class PublicMappedBaseSuper : PublicMappedBase
        {
        }

        protected class PublicMappedBaseSub : PublicMappedBaseSuper
        {
        }

        protected class PrivateWithDuplicatesBase
        {
            public int Id { get; set; }
            private DbContext? Context { get; set; }
            private DbContext? Context2 { get; set; }
            private IEntityType? EntityType { get; set; }
            private IEntityType? EntityType2 { get; set; }
            private ILazyLoader? ALazyLoader { get; set; }
            private ILazyLoader? ALazyLoader2 { get; set; }
            private Action<object, string>? LazyLoader { get; set; }
        }

        protected class PrivateWithDuplicatesSuper : PrivateWithDuplicatesBase
        {
            private DbContext? Context { get; set; }
            private DbContext? Context2 { get; set; }
            private IEntityType? EntityType { get; set; }
            private IEntityType? EntityType2 { get; set; }
            private ILazyLoader? ALazyLoader { get; set; }
            private ILazyLoader? ALazyLoader2 { get; set; }
            private Action<object, string>? LazyLoader { get; set; }
        }

        protected class PrivateWithDuplicatesSub : PrivateWithDuplicatesSuper
        {
            private DbContext? Context { get; set; }
            private DbContext? Context2 { get; set; }
            private IEntityType? EntityType { get; set; }
            private IEntityType? EntityType2 { get; set; }
            private ILazyLoader? ALazyLoader { get; set; }
            private ILazyLoader? ALazyLoader2 { get; set; }
            private Action<object, string>? LazyLoader { get; set; }
        }
    }
}

#nullable restore
