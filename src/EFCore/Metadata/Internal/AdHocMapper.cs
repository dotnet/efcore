// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class AdHocMapper : IAdHocMapper
{
    private readonly IModel _model;
    private readonly ModelCreationDependencies _modelCreationDependencies;
    private ConventionSet? _conventionSet;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public AdHocMapper(
        IModel model,
        ModelCreationDependencies modelCreationDependencies)
    {
        _model = model;
        _modelCreationDependencies = modelCreationDependencies;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConventionSet BuildConventionSet()
    {
        var conventionSet = _modelCreationDependencies.ConventionSetBuilder.CreateConventionSet();
        conventionSet.Remove(typeof(DbSetFindingConvention));
        conventionSet.Remove(typeof(RelationshipDiscoveryConvention));
        conventionSet.Remove(typeof(KeyDiscoveryConvention));
        conventionSet.Remove(typeof(CascadeDeleteConvention));
        conventionSet.Remove(typeof(ChangeTrackingStrategyConvention));
        conventionSet.Remove(typeof(DeleteBehaviorAttributeConvention));
        conventionSet.Remove(typeof(ForeignKeyAttributeConvention));
        conventionSet.Remove(typeof(ForeignKeyIndexConvention));
        conventionSet.Remove(typeof(ForeignKeyPropertyDiscoveryConvention));
        conventionSet.Remove(typeof(IndexAttributeConvention));
        conventionSet.Remove(typeof(KeyAttributeConvention));
        conventionSet.Remove(typeof(KeylessAttributeConvention));
        conventionSet.Remove(typeof(ManyToManyJoinEntityTypeConvention));
        conventionSet.Remove(typeof(RequiredNavigationAttributeConvention));
        conventionSet.Remove(typeof(NavigationBackingFieldAttributeConvention));
        conventionSet.Remove(typeof(InversePropertyAttributeConvention));
        conventionSet.Remove(typeof(NavigationEagerLoadingConvention));
        conventionSet.Remove(typeof(NonNullableNavigationConvention));
        conventionSet.Remove(typeof(NotMappedTypeAttributeConvention));
        conventionSet.Remove(typeof(OwnedAttributeConvention));
        conventionSet.Remove(typeof(QueryFilterRewritingConvention));
        conventionSet.Remove(typeof(ServicePropertyDiscoveryConvention));
        conventionSet.Remove(typeof(ValueGenerationConvention));
        conventionSet.Remove(typeof(BaseTypeDiscoveryConvention));
        conventionSet.Remove(typeof(DiscriminatorConvention));

        return conventionSet;
    }

    private ConventionSet ConventionSet
        => (_conventionSet ??= BuildConventionSet());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual RuntimeEntityType GetOrAddEntityType(Type clrType)
    {
        Check.DebugAssert(_model is RuntimeModel, "Ad-hoc entity types can only be used at runtime.");

        return ((RuntimeModel)_model).FindAdHocEntityType(clrType) ?? AddEntityType(clrType);
    }

    private RuntimeEntityType AddEntityType(Type clrType)
    {
        var modelBuilder = new ModelBuilder(ConventionSet, _modelCreationDependencies.ModelDependencies);
        modelBuilder.HasAnnotation(CoreAnnotationNames.AdHocModel, true);
        modelBuilder.Entity(clrType).HasNoKey();
        var finalizedModel = modelBuilder.FinalizeModel();
        var runtimeModel = _modelCreationDependencies.ModelRuntimeInitializer.Initialize(
            finalizedModel, designTime: false, _modelCreationDependencies.ValidationLogger);

        return ((RuntimeModel)_model).GetOrAddAdHocEntityType((RuntimeEntityType)runtimeModel.FindEntityType(clrType)!);
    }
}
