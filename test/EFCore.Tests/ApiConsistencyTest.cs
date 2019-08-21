// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class ApiConsistencyTest : ApiConsistencyTestBase
    {
        private static readonly Type[] _fluentApiTypes =
        {
            typeof(ModelBuilder),
            typeof(CollectionNavigationBuilder),
            typeof(CollectionNavigationBuilder<SampleEntity, SampleEntity>),
            typeof(DataBuilder),
            typeof(DataBuilder<SampleEntity>),
            typeof(DiscriminatorBuilder),
            typeof(DiscriminatorBuilder<>),
            typeof(EntityTypeBuilder),
            typeof(EntityTypeBuilder<SampleEntity>),
            typeof(IndexBuilder),
            typeof(IndexBuilder<SampleEntity>),
            typeof(KeyBuilder),
            typeof(OwnedNavigationBuilder),
            typeof(OwnedNavigationBuilder<SampleEntity, SampleEntity>),
            typeof(OwnedEntityTypeBuilder),
            typeof(OwnedEntityTypeBuilder<SampleEntity>),
            typeof(OwnershipBuilder),
            typeof(OwnershipBuilder<SampleEntity, SampleEntity>),
            typeof(PropertyBuilder),
            typeof(PropertyBuilder<>),
            typeof(ReferenceCollectionBuilder),
            typeof(ReferenceCollectionBuilder<SampleEntity, SampleEntity>),
            typeof(ReferenceNavigationBuilder),
            typeof(ReferenceNavigationBuilder<SampleEntity, SampleEntity>),
            typeof(ReferenceReferenceBuilder),
            typeof(ReferenceReferenceBuilder<SampleEntity, SampleEntity>),
            typeof(DbContextOptionsBuilder),
            typeof(DbContextOptionsBuilder<DbContext>),
            typeof(EntityFrameworkServiceCollectionExtensions)
        };

        private static Dictionary<Type, (Type Mutable, Type Convention)> _metadataTypes
            => new Dictionary<Type, (Type, Type)>
            {
                { typeof(IModel), (typeof(IMutableModel), typeof(IConventionModel)) },
                { typeof(IAnnotatable), (typeof(IMutableAnnotatable), typeof(IConventionAnnotatable)) },
                { typeof(IEntityType), (typeof(IMutableEntityType), typeof(IConventionEntityType)) },
                { typeof(ITypeBase), (typeof(IMutableTypeBase), typeof(IConventionTypeBase)) },
                { typeof(IKey), (typeof(IMutableKey), typeof(IConventionKey)) },
                { typeof(IForeignKey), (typeof(IMutableForeignKey), typeof(IConventionForeignKey)) },
                { typeof(IIndex), (typeof(IMutableIndex), typeof(IConventionIndex)) },
                { typeof(IProperty), (typeof(IMutableProperty), typeof(IConventionProperty)) },
                { typeof(INavigation), (typeof(IMutableNavigation), typeof(IConventionNavigation)) },
                { typeof(IServiceProperty), (typeof(IMutableServiceProperty), typeof(IConventionServiceProperty)) },
                { typeof(IPropertyBase), (typeof(IMutablePropertyBase), typeof(IConventionPropertyBase)) },
                { typeof(ModelExtensions), (typeof(MutableModelExtensions), typeof(ConventionModelExtensions)) },
                { typeof(AnnotatableExtensions), (typeof(MutableAnnotatableExtensions), typeof(ConventionAnnotatableExtensions)) },
                { typeof(EntityTypeExtensions), (typeof(MutableEntityTypeExtensions), typeof(ConventionEntityTypeExtensions)) },
                { typeof(TypeBaseExtensions), (typeof(MutableTypeBaseExtensions), typeof(ConventionTypeBaseExtensions)) },
                { typeof(KeyExtensions), (typeof(MutableKeyExtensions), typeof(ConventionKeyExtensions)) },
                { typeof(ForeignKeyExtensions), (typeof(MutableForeignKeyExtensions), typeof(ConventionForeignKeyExtensions)) },
                { typeof(PropertyExtensions), (typeof(MutablePropertyExtensions), typeof(ConventionPropertyExtensions)) },
                { typeof(NavigationExtensions), (typeof(MutableNavigationExtensions), typeof(ConventionNavigationExtensions)) },
                { typeof(PropertyBaseExtensions), (typeof(MutablePropertyBaseExtensions), typeof(ConventionPropertyBaseExtensions)) }
            };

        protected override void AddServices(ServiceCollection serviceCollection)
            => new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();

        public class SampleEntity
        {
        }

        protected override bool ShouldHaveNotNullAnnotation(MethodBase method, Type type)
            => base.ShouldHaveNotNullAnnotation(method, type)
               && method.Name != nameof(DbContext.OnConfiguring)
               && method.Name != nameof(DbContext.OnModelCreating)
               && !(type == typeof(IEntityTypeConfiguration<>)
                    && method.Name == nameof(IEntityTypeConfiguration<object>.Configure));

        protected override bool ShouldHaveVirtualMethods(Type type) => type != typeof(InternalEntityEntry);

        protected override IEnumerable<Type> FluentApiTypes => _fluentApiTypes;

        protected override Dictionary<Type, (Type Mutable, Type Convention)> MetadataTypes
            => _metadataTypes;

        protected override Assembly TargetAssembly => typeof(EntityType).GetTypeInfo().Assembly;
    }
}
