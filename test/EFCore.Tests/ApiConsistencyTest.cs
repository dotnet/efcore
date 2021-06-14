// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
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

            public override HashSet<Type> FluentApiTypes { get; } = new()
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

            public override HashSet<MethodInfo> NonVirtualMethods { get; } = new()
            {
                typeof(CompiledQueryCacheKeyGenerator).GetMethod("GenerateCacheKeyCore", AnyInstance),
                typeof(InternalEntityEntry).GetMethod("get_Item"),
                typeof(InternalEntityEntry).GetMethod("set_Item"),
                typeof(InternalEntityEntry).GetMethod(nameof(InternalEntityEntry.HasDefaultValue)),
                typeof(DiagnosticsLogger<>).GetMethod("DispatchEventData", AnyInstance),
                typeof(DiagnosticsLogger<>).GetMethod("ShouldLog", AnyInstance),
                typeof(DiagnosticsLogger<>).GetMethod("NeedsEventData", AnyInstance)
            };

            public override HashSet<MethodInfo> NotAnnotatedMethods { get; } = new()
            {
                typeof(DbContext).GetMethod(nameof(DbContext.OnConfiguring), AnyInstance),
                typeof(DbContext).GetMethod(nameof(DbContext.OnModelCreating), AnyInstance),
                typeof(IEntityTypeConfiguration<>).GetMethod(nameof(IEntityTypeConfiguration<Type>.Configure))
            };

            public override HashSet<MethodInfo> UnmatchedMetadataMethods { get; } = new()
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
                typeof(IReadOnlyAnnotatable).GetMethod(nameof(IReadOnlyAnnotatable.FindAnnotation)),
                typeof(IReadOnlyAnnotatable).GetMethod(nameof(IReadOnlyAnnotatable.GetAnnotations)),
                typeof(IReadOnlyAnnotatable).GetMethod(nameof(IReadOnlyAnnotatable.GetAnnotation)),
                typeof(IMutableAnnotatable).GetMethod("set_Item"),
                typeof(IConventionAnnotatable).GetMethod(nameof(IConventionAnnotatable.SetAnnotation)),
                typeof(IConventionAnnotatable).GetMethod(nameof(IConventionAnnotatable.SetOrRemoveAnnotation)),
                typeof(IConventionModelBuilder).GetMethod(nameof(IConventionModelBuilder.HasNoEntityType)),
                typeof(IReadOnlyEntityType).GetMethod(nameof(IReadOnlyEntityType.GetConcreteDerivedTypesInclusive)),
                typeof(IReadOnlyEntityType).GetMethod(nameof(IReadOnlyEntityType.FindClosestCommonParent)),
                typeof(IReadOnlyEntityType).GetMethod(nameof(IReadOnlyEntityType.LeastDerivedType)),
                typeof(IMutableEntityType).GetMethod(nameof(IMutableEntityType.AddData)),
                typeof(IReadOnlyNavigationBase).GetMethod("get_DeclaringEntityType"),
                typeof(IReadOnlyNavigationBase).GetMethod("get_TargetEntityType"),
                typeof(IReadOnlyNavigationBase).GetMethod("get_Inverse"),
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

            public override HashSet<MethodInfo> MetadataMethodExceptions { get; } = new()
            {
                typeof(IConventionAnnotatable).GetMethod(nameof(IConventionAnnotatable.SetAnnotation)),
                typeof(IConventionAnnotatable).GetMethod(nameof(IConventionAnnotatable.SetOrRemoveAnnotation)),
                typeof(IConventionAnnotatable).GetMethod(nameof(IConventionAnnotatable.AddAnnotations)),
                typeof(IMutableAnnotatable).GetMethod(nameof(IMutableAnnotatable.AddAnnotations)),
                typeof(IConventionModel).GetMethod(nameof(IConventionModel.IsIgnoredType)),
                typeof(IConventionModel).GetMethod(nameof(IConventionModel.IsShared)),
                typeof(IConventionModel).GetMethod(nameof(IConventionModel.AddOwned)),
                typeof(IConventionModel).GetMethod(nameof(IConventionModel.AddShared)),
                typeof(IMutableModel).GetMethod(nameof(IMutableModel.AddOwned)),
                typeof(IMutableModel).GetMethod(nameof(IMutableModel.AddShared)),
                typeof(IMutableEntityType).GetMethod(nameof(IMutableEntityType.AddData)),
                typeof(IConventionEntityType).GetMethod(nameof(IConventionEntityType.LeastDerivedType))
            };
        }
    }
}
