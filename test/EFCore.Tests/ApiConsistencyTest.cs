// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public class ApiConsistencyTest(ApiConsistencyTest.ApiConsistencyFixture fixture) : ApiConsistencyTestBase<ApiConsistencyTest.ApiConsistencyFixture>(fixture)
{
    protected override Assembly TargetAssembly
        => typeof(EntityType).Assembly;

    protected override void AddServices(ServiceCollection serviceCollection)
        => new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();

    public class ApiConsistencyFixture : ApiConsistencyFixtureBase
    {
        protected override void Initialize()
        {
            AddInstanceMethods(MetadataTypes);

            MirrorTypes.Add(typeof(PropertyBuilder), typeof(ComplexTypePropertyBuilder));

            base.Initialize();
        }

        public override HashSet<Type> FluentApiTypes { get; } =
        [
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
            typeof(ElementTypeBuilder),
            typeof(ComplexPropertyBuilder),
            typeof(ComplexPropertyBuilder<>),
            typeof(ComplexTypePrimitiveCollectionBuilder),
            typeof(ComplexTypePrimitiveCollectionBuilder<>),
            typeof(IndexBuilder),
            typeof(IndexBuilder<>),
            typeof(TriggerBuilder),
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
            typeof(PrimitiveCollectionBuilder),
            typeof(PrimitiveCollectionBuilder<>),
            typeof(ComplexTypePropertyBuilder),
            typeof(ComplexTypePropertyBuilder<>),
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
        ];

        public override HashSet<MethodInfo> NonVirtualMethods { get; } =
        [
            typeof(CompiledQueryCacheKeyGenerator).GetMethod("GenerateCacheKeyCore", AnyInstance),
            typeof(InternalEntityEntry).GetMethod("get_Item"),
            typeof(InternalEntityEntry).GetMethod("set_Item"),
            typeof(InternalEntityEntry).GetMethod(nameof(InternalEntityEntry.HasExplicitValue)),
            typeof(DiagnosticsLogger<>).GetMethod("DispatchEventData", AnyInstance),
            typeof(DiagnosticsLogger<>).GetMethod("ShouldLog", AnyInstance),
            typeof(DiagnosticsLogger<>).GetMethod("NeedsEventData", AnyInstance),
            typeof(ChangeDetector).GetMethod("DetectValueChange"),
            typeof(ChangeDetector).GetMethod("DetectNavigationChange"),
            typeof(StateManager).GetMethod("get_ChangeDetector"),
            typeof(JsonValueReaderWriter<>).GetMethod(nameof(JsonValueReaderWriter.FromJson)),
            typeof(JsonValueReaderWriter<>).GetMethod(nameof(JsonValueReaderWriter.ToJson)),
            typeof(JsonValueReaderWriter<>).GetMethod("get_ValueType"),
            typeof(JsonValueReaderWriter).GetMethod(nameof(JsonValueReaderWriter.FromJsonString)),
            typeof(JsonValueReaderWriter).GetMethod(nameof(JsonValueReaderWriter.ToJsonString))
        ];

        public override HashSet<MethodInfo> NotAnnotatedMethods { get; } =
        [
            typeof(DbContext).GetMethod(nameof(DbContext.OnConfiguring), AnyInstance),
            typeof(DbContext).GetMethod(nameof(DbContext.OnModelCreating), AnyInstance),
            typeof(IEntityTypeConfiguration<>).GetMethod(nameof(IEntityTypeConfiguration<Type>.Configure))
        ];

        public override Dictionary<MethodInfo, string> MetadataMethodNameTransformers { get; } = new()
        {
            {
                typeof(IConventionNavigationBuilder).GetMethod(
                    nameof(IConventionNavigationBuilder.EnableLazyLoading), [typeof(bool?), typeof(bool)])!,
                "LazyLoadingEnabled"
            },
            {
                typeof(IConventionSkipNavigationBuilder).GetMethod(
                    nameof(IConventionSkipNavigationBuilder.EnableLazyLoading), [typeof(bool?), typeof(bool)])!,
                "LazyLoadingEnabled"
            }
        };

        public override HashSet<MethodInfo> UnmatchedMetadataMethods { get; } =
        [
            typeof(PropertyBuilder).GetMethod(
                nameof(PropertyBuilder.HasValueGenerator), 0, [typeof(Func<IProperty, ITypeBase, ValueGenerator>)]),
            typeof(ComplexPropertyBuilder).GetMethod(
                nameof(ComplexPropertyBuilder.ComplexProperty), 0, [typeof(string)]),
            typeof(ComplexPropertyBuilder).GetMethod(
                nameof(ComplexPropertyBuilder.ComplexProperty), 0, [typeof(Type), typeof(string)]),
            typeof(ComplexPropertyBuilder).GetMethod(
                nameof(ComplexPropertyBuilder.ComplexProperty), 0, [typeof(Type), typeof(string), typeof(string)]),
            typeof(OwnedNavigationBuilder).GetMethod(
                nameof(OwnedNavigationBuilder.OwnsOne), 0, [typeof(string), typeof(string)]),
            typeof(OwnedNavigationBuilder).GetMethod(
                nameof(OwnedNavigationBuilder.OwnsOne), 0, [typeof(string), typeof(Type), typeof(string)]),
            typeof(OwnedNavigationBuilder).GetMethod(
                nameof(OwnedNavigationBuilder.OwnsOne), 0, [typeof(Type), typeof(string)]),
            typeof(OwnedNavigationBuilder).GetMethod(
                nameof(OwnedNavigationBuilder.OwnsMany), 0, [typeof(string), typeof(string)]),
            typeof(OwnedNavigationBuilder).GetMethod(
                nameof(OwnedNavigationBuilder.OwnsMany), 0, [typeof(string), typeof(Type), typeof(string)]),
            typeof(OwnedNavigationBuilder).GetMethod(
                nameof(OwnedNavigationBuilder.OwnsMany), 0, [typeof(Type), typeof(string)]),
            typeof(OwnedNavigationBuilder).GetMethod(
                nameof(OwnedNavigationBuilder.OwnsOne), 0,
                [typeof(string), typeof(string), typeof(Action<OwnedNavigationBuilder>)]),
            typeof(OwnedNavigationBuilder).GetMethod(
                nameof(OwnedNavigationBuilder.OwnsOne), 0,
                [typeof(string), typeof(Type), typeof(string), typeof(Action<OwnedNavigationBuilder>)]),
            typeof(OwnedNavigationBuilder).GetMethod(
                nameof(OwnedNavigationBuilder.OwnsOne), 0, [typeof(Type), typeof(string), typeof(Action<OwnedNavigationBuilder>)]),
            typeof(OwnedNavigationBuilder).GetMethod(
                nameof(OwnedNavigationBuilder.OwnsMany), 0,
                [typeof(string), typeof(string), typeof(Action<OwnedNavigationBuilder>)]),
            typeof(OwnedNavigationBuilder).GetMethod(
                nameof(OwnedNavigationBuilder.OwnsMany), 0,
                [typeof(string), typeof(Type), typeof(string), typeof(Action<OwnedNavigationBuilder>)]),
            typeof(OwnedNavigationBuilder).GetMethod(
                nameof(OwnedNavigationBuilder.OwnsMany), 0, [typeof(Type), typeof(string), typeof(Action<OwnedNavigationBuilder>)]),
            typeof(IConventionPropertyBase).GetMethod(nameof(IConventionPropertyBase.SetField), [typeof(string), typeof(bool)]),
            typeof(IReadOnlyAnnotatable).GetMethod(nameof(IReadOnlyAnnotatable.FindAnnotation)),
            typeof(IReadOnlyAnnotatable).GetMethod(nameof(IReadOnlyAnnotatable.GetAnnotations)),
            typeof(IReadOnlyAnnotatable).GetMethod(nameof(IReadOnlyAnnotatable.GetAnnotation)),
            typeof(IMutableAnnotatable).GetMethod("set_Item"),
            typeof(IConventionAnnotatable).GetMethod(nameof(IConventionAnnotatable.SetAnnotation)),
            typeof(IConventionAnnotatable).GetMethod(nameof(IConventionAnnotatable.SetOrRemoveAnnotation)),
            typeof(IConventionModelBuilder).GetMethod(nameof(IConventionModelBuilder.HasNoEntityType)),
            typeof(IConventionModelBuilder).GetMethod(nameof(IConventionModelBuilder.ComplexType)),
            typeof(IReadOnlyEntityType).GetMethod(nameof(IReadOnlyEntityType.GetConcreteDerivedTypesInclusive)),
            typeof(IMutableEntityType).GetMethod(nameof(IMutableEntityType.AddData)),
            typeof(IReadOnlyNavigationBase).GetMethod("get_DeclaringEntityType"),
            typeof(IReadOnlyNavigationBase).GetMethod("get_TargetEntityType"),
            typeof(IReadOnlyNavigationBase).GetMethod("get_Inverse"),
            typeof(IConventionAnnotatableBuilder).GetMethod(nameof(IConventionAnnotatableBuilder.HasNonNullAnnotation)),
            typeof(IConventionEntityTypeBuilder).GetMethod(nameof(IConventionEntityTypeBuilder.RemoveUnusedImplicitProperties)),
            typeof(IConventionTypeBaseBuilder).GetMethod(nameof(IConventionTypeBaseBuilder.RemoveUnusedImplicitProperties)),
            typeof(IConventionEntityTypeBuilder).GetMethod(nameof(IConventionEntityTypeBuilder.GetTargetEntityTypeBuilder)),
            typeof(IConventionPropertyBuilder).GetMethod(
                nameof(IConventionPropertyBuilder.HasField), [typeof(string), typeof(bool)]),
            typeof(IConventionPropertyBuilder).GetMethod(
                nameof(IConventionPropertyBuilder.HasField), [typeof(FieldInfo), typeof(bool)]),
            typeof(IConventionPropertyBuilder).GetMethod(
                nameof(IConventionPropertyBuilder.UsePropertyAccessMode), [typeof(PropertyAccessMode), typeof(bool)]),
            typeof(IConventionServicePropertyBuilder).GetMethod(
                nameof(IConventionServicePropertyBuilder.HasField), [typeof(string), typeof(bool)]),
            typeof(IConventionServicePropertyBuilder).GetMethod(
                nameof(IConventionServicePropertyBuilder.HasField), [typeof(FieldInfo), typeof(bool)]),
            typeof(IConventionServicePropertyBuilder).GetMethod(
                nameof(IConventionServicePropertyBuilder.UsePropertyAccessMode), [typeof(PropertyAccessMode), typeof(bool)]),
            typeof(IConventionNavigationBuilder).GetMethod(
                nameof(IConventionNavigationBuilder.HasField), [typeof(string), typeof(bool)]),
            typeof(IConventionNavigationBuilder).GetMethod(
                nameof(IConventionNavigationBuilder.HasField), [typeof(FieldInfo), typeof(bool)]),
            typeof(IConventionNavigationBuilder).GetMethod(
                nameof(IConventionNavigationBuilder.UsePropertyAccessMode), [typeof(PropertyAccessMode), typeof(bool)]),
            typeof(IConventionSkipNavigationBuilder).GetMethod(
                nameof(IConventionSkipNavigationBuilder.HasField), [typeof(string), typeof(bool)]),
            typeof(IConventionSkipNavigationBuilder).GetMethod(
                nameof(IConventionSkipNavigationBuilder.HasField), [typeof(FieldInfo), typeof(bool)]),
            typeof(IConventionSkipNavigationBuilder).GetMethod(
                nameof(IConventionSkipNavigationBuilder.UsePropertyAccessMode), [typeof(PropertyAccessMode), typeof(bool)])
        ];

        public override HashSet<MethodInfo> MetadataMethodExceptions { get; } =
        [
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
        ];
    }
}
