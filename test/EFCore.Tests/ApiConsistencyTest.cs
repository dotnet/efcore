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
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class ApiConsistencyTest : ApiConsistencyTestBase<ApiConsistencyTest.ApiConsistencyFixture>
    {
        public ApiConsistencyTest(ApiConsistencyFixture fixture)
            : base(fixture)
        {
        }

        protected override Assembly TargetAssembly
            => typeof(EntityType).Assembly;

        protected override void AddServices(ServiceCollection serviceCollection)
            => new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();

        public class ApiConsistencyFixture : ApiConsistencyFixtureBase
        {
            protected override void Initialize()
            {
                AddInstanceMethods(MetadataTypes);

                base.Initialize();
            }

            public override bool TryGetProviderOptionsDelegate(out Action<DbContextOptionsBuilder> configureOptions)
            {
                configureOptions = null;
                return false;
            }

            public override HashSet<Type> FluentApiTypes { get; } = new HashSet<Type>
            {
                typeof(ModelBuilder),
                typeof(CollectionCollectionBuilder),
                typeof(CollectionCollectionBuilder<,>),
                typeof(CollectionNavigationBuilder),
                typeof(CollectionNavigationBuilder<,>),
                typeof(DataBuilder),
                typeof(DataBuilder<>),
                typeof(DiscriminatorBuilder),
                typeof(DiscriminatorBuilder<>),
                typeof(EntityTypeBuilder),
                typeof(EntityTypeBuilder<>),
                typeof(IndexBuilder),
                typeof(IndexBuilder<>),
                typeof(InvertibleRelationshipBuilderBase),
                typeof(KeyBuilder),
                typeof(KeyBuilder<>),
                typeof(NavigationBuilder),
                typeof(NavigationBuilder<,>),
                typeof(OwnedNavigationBuilder),
                typeof(OwnedNavigationBuilder<,>),
                typeof(OwnedEntityTypeBuilder),
                typeof(OwnedEntityTypeBuilder<>),
                typeof(OwnershipBuilder),
                typeof(OwnershipBuilder<,>),
                typeof(PropertyBuilder),
                typeof(PropertyBuilder<>),
                typeof(ReferenceCollectionBuilder),
                typeof(ReferenceCollectionBuilder<,>),
                typeof(ReferenceNavigationBuilder),
                typeof(ReferenceNavigationBuilder<,>),
                typeof(ReferenceReferenceBuilder),
                typeof(ReferenceReferenceBuilder<,>),
                typeof(RelationshipBuilderBase),
                typeof(DbContextOptionsBuilder),
                typeof(DbContextOptionsBuilder<>),
                typeof(EntityFrameworkServiceCollectionExtensions)
            };

            public override
                List<(Type Type, Type ReadonlyExtensions, Type MutableExtensions, Type ConventionExtensions, Type
                    ConventionBuilderExtensions)> MetadataExtensionTypes { get; }
                = new List<(Type, Type, Type, Type, Type)>
                {
                    (typeof(IModel), typeof(ModelExtensions), typeof(MutableModelExtensions), typeof(ConventionModelExtensions), null),
                    (typeof(IAnnotatable), typeof(AnnotatableExtensions), typeof(MutableAnnotatableExtensions),
                        typeof(ConventionAnnotatableExtensions), null),
                    (typeof(IEntityType), typeof(EntityTypeExtensions), typeof(MutableEntityTypeExtensions),
                        typeof(ConventionEntityTypeExtensions), null),
                    (typeof(ITypeBase), typeof(TypeBaseExtensions), typeof(MutableTypeBaseExtensions),
                        typeof(ConventionTypeBaseExtensions), null),
                    (typeof(IKey), typeof(KeyExtensions), typeof(MutableKeyExtensions), typeof(ConventionKeyExtensions), null),
                    (typeof(IForeignKey), typeof(ForeignKeyExtensions), typeof(MutableForeignKeyExtensions),
                        typeof(ConventionForeignKeyExtensions), null),
                    (typeof(IProperty), typeof(PropertyExtensions), typeof(MutablePropertyExtensions),
                        typeof(ConventionPropertyExtensions), null),
                    (typeof(INavigation), typeof(NavigationExtensions), typeof(MutableNavigationExtensions),
                        typeof(ConventionNavigationExtensions), null),
                    (typeof(IPropertyBase), typeof(PropertyBaseExtensions), typeof(MutablePropertyBaseExtensions),
                        typeof(ConventionPropertyBaseExtensions), null)
                };

            public override HashSet<MethodInfo> NonVirtualMethods { get; } = new HashSet<MethodInfo>
            {
                typeof(CompiledQueryCacheKeyGenerator).GetMethod("GenerateCacheKeyCore", AnyInstance),
                typeof(InternalEntityEntry).GetMethod("get_Item"),
                typeof(InternalEntityEntry).GetMethod("set_Item"),
                typeof(InternalEntityEntry).GetMethod(nameof(InternalEntityEntry.HasDefaultValue))
            };

            public override HashSet<MethodInfo> NotAnnotatedMethods { get; } = new HashSet<MethodInfo>
            {
                typeof(DbContext).GetMethod(nameof(DbContext.OnConfiguring), AnyInstance),
                typeof(DbContext).GetMethod(nameof(DbContext.OnModelCreating), AnyInstance),
                typeof(IEntityTypeConfiguration<>).GetMethod(nameof(IEntityTypeConfiguration<Type>.Configure))
            };

            public override HashSet<MethodInfo> UnmatchedMetadataMethods { get; } = new HashSet<MethodInfo>
            {
                typeof(OwnedNavigationBuilder<,>).GetMethod(
                    nameof(OwnedNavigationBuilder.OwnsOne), 0, new[] { typeof(string), typeof(string) }),
                typeof(OwnedNavigationBuilder<,>).GetMethod(
                    nameof(OwnedNavigationBuilder.OwnsOne), 0, new[] { typeof(string), typeof(Type), typeof(string) }),
                typeof(OwnedNavigationBuilder<,>).GetMethod(
                    nameof(OwnedNavigationBuilder.OwnsOne), 0, new[] { typeof(Type), typeof(string) }),
                typeof(OwnedNavigationBuilder<,>).GetMethod(
                    nameof(OwnedNavigationBuilder.OwnsMany), 0, new[] { typeof(string), typeof(string) }),
                typeof(OwnedNavigationBuilder<,>).GetMethod(
                    nameof(OwnedNavigationBuilder.OwnsMany), 0, new[] { typeof(string), typeof(Type), typeof(string) }),
                typeof(OwnedNavigationBuilder<,>).GetMethod(
                    nameof(OwnedNavigationBuilder.OwnsMany), 0, new[] { typeof(Type), typeof(string) }),
                typeof(IConventionPropertyBase).GetMethod(nameof(IConventionPropertyBase.SetField), new[] { typeof(string), typeof(bool) }),
                typeof(IAnnotatable).GetMethod(nameof(IAnnotatable.FindAnnotation)),
                typeof(IAnnotatable).GetMethod(nameof(IAnnotatable.GetAnnotations)),
                typeof(IMutableAnnotatable).GetMethod("set_Item"),
                typeof(IConventionAnnotatable).GetMethod(nameof(IConventionAnnotatable.SetAnnotation)),
                typeof(ConventionAnnotatableExtensions).GetMethod(nameof(ConventionAnnotatableExtensions.SetOrRemoveAnnotation)),
                typeof(ModelExtensions).GetMethod(nameof(ModelExtensions.FindRuntimeEntityType)),
                typeof(EntityTypeExtensions).GetMethod(nameof(EntityTypeExtensions.GetAllBaseTypesInclusive)),
                typeof(EntityTypeExtensions).GetMethod(nameof(EntityTypeExtensions.GetAllBaseTypesInclusiveAscending)),
                typeof(EntityTypeExtensions).GetMethod(nameof(EntityTypeExtensions.GetConcreteDerivedTypesInclusive)),
                typeof(EntityTypeExtensions).GetMethod(nameof(EntityTypeExtensions.GetClosestCommonParent)),
                typeof(EntityTypeExtensions).GetMethod(nameof(EntityTypeExtensions.GetProperty)),
                typeof(EntityTypeExtensions).GetMethod(nameof(EntityTypeExtensions.LeastDerivedType)),
                typeof(IConventionModelBuilder).GetMethod(nameof(IConventionModelBuilder.HasNoEntityType)),
                typeof(INavigationBase).GetMethod("get_DeclaringEntityType"),
                typeof(INavigationBase).GetMethod("get_TargetEntityType"),
                typeof(INavigationBase).GetMethod("get_Inverse"),
                typeof(IConventionAnnotatableBuilder).GetMethod(nameof(IConventionAnnotatableBuilder.HasNonNullAnnotation)),
                typeof(IConventionEntityTypeBuilder).GetMethod(nameof(IConventionEntityTypeBuilder.RemoveUnusedImplicitProperties)),
                typeof(IConventionEntityTypeBuilder).GetMethod(nameof(IConventionEntityTypeBuilder.Ignore)),
                typeof(IConventionModelBuilder).GetMethod(nameof(IConventionModelBuilder.Ignore), new[] { typeof(Type), typeof(bool) }),
                typeof(IConventionModelBuilder).GetMethod(nameof(IConventionModelBuilder.Ignore), new[] { typeof(string), typeof(bool) }),
                typeof(IConventionPropertyBuilder).GetMethod(
                    nameof(IConventionPropertyBuilder.HasField), new[] { typeof(string), typeof(bool) }),
                typeof(IConventionPropertyBuilder).GetMethod(
                    nameof(IConventionPropertyBuilder.HasField), new[] { typeof(FieldInfo), typeof(bool) }),
                typeof(IConventionPropertyBuilder).GetMethod(
                    nameof(IConventionPropertyBuilder.UsePropertyAccessMode), new[] { typeof(PropertyAccessMode), typeof(bool) }),
                typeof(IConventionServicePropertyBuilder).GetMethod(
                    nameof(IConventionServicePropertyBuilder.HasField), new[] { typeof(string), typeof(bool) }),
                typeof(IConventionServicePropertyBuilder).GetMethod(
                    nameof(IConventionServicePropertyBuilder.HasField), new[] { typeof(FieldInfo), typeof(bool) }),
                typeof(IConventionServicePropertyBuilder).GetMethod(
                    nameof(IConventionServicePropertyBuilder.UsePropertyAccessMode), new[] { typeof(PropertyAccessMode), typeof(bool) }),
                typeof(IConventionNavigationBuilder).GetMethod(
                    nameof(IConventionNavigationBuilder.HasField), new[] { typeof(string), typeof(bool) }),
                typeof(IConventionNavigationBuilder).GetMethod(
                    nameof(IConventionNavigationBuilder.HasField), new[] { typeof(FieldInfo), typeof(bool) }),
                typeof(IConventionNavigationBuilder).GetMethod(
                    nameof(IConventionNavigationBuilder.UsePropertyAccessMode), new[] { typeof(PropertyAccessMode), typeof(bool) }),
                typeof(IConventionSkipNavigationBuilder).GetMethod(
                    nameof(IConventionSkipNavigationBuilder.HasField), new[] { typeof(string), typeof(bool) }),
                typeof(IConventionSkipNavigationBuilder).GetMethod(
                    nameof(IConventionSkipNavigationBuilder.HasField), new[] { typeof(FieldInfo), typeof(bool) }),
                typeof(IConventionSkipNavigationBuilder).GetMethod(
                    nameof(IConventionSkipNavigationBuilder.UsePropertyAccessMode), new[] { typeof(PropertyAccessMode), typeof(bool) }),
            };

            public override HashSet<MethodInfo> MetadataMethodExceptions { get; } = new HashSet<MethodInfo>
            {
                typeof(IConventionAnnotatable).GetMethod(nameof(IConventionAnnotatable.SetAnnotation)),
                typeof(ConventionAnnotatableExtensions).GetMethod(nameof(ConventionAnnotatableExtensions.SetOrRemoveAnnotation)),
                typeof(ConventionAnnotatableExtensions).GetMethod(nameof(ConventionAnnotatableExtensions.AddAnnotations)),
                typeof(MutableAnnotatableExtensions).GetMethod(nameof(MutableAnnotatableExtensions.AddAnnotations)),
                typeof(IConventionModel).GetMethod(nameof(IConventionModel.IsShared)),
                typeof(ConventionModelExtensions).GetMethod(nameof(ConventionModelExtensions.AddOwned)),
                typeof(ConventionModelExtensions).GetMethod(nameof(ConventionModelExtensions.AddShared)),
                typeof(MutableModelExtensions).GetMethod(nameof(ConventionModelExtensions.AddOwned)),
                typeof(MutableModelExtensions).GetMethod(nameof(ConventionModelExtensions.AddShared)),
            };
        }
    }
}
