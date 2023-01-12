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

    private ConventionSet ConventionSet
    {
        get
        {
            if (_conventionSet == null)
            {
                _conventionSet = _modelCreationDependencies.ConventionSetBuilder.CreateConventionSet();
                _conventionSet.Remove(typeof(DbSetFindingConvention));
                _conventionSet.Remove(typeof(RelationshipDiscoveryConvention));
                _conventionSet.Remove(typeof(KeyDiscoveryConvention));
                _conventionSet.Remove(typeof(CascadeDeleteConvention));
                _conventionSet.Remove(typeof(ChangeTrackingStrategyConvention));
                _conventionSet.Remove(typeof(DeleteBehaviorAttributeConvention));
                _conventionSet.Remove(typeof(ForeignKeyAttributeConvention));
                _conventionSet.Remove(typeof(ForeignKeyIndexConvention));
                _conventionSet.Remove(typeof(ForeignKeyPropertyDiscoveryConvention));
                _conventionSet.Remove(typeof(IndexAttributeConvention));
                _conventionSet.Remove(typeof(KeyAttributeConvention));
                _conventionSet.Remove(typeof(KeylessEntityTypeAttributeConvention));
                _conventionSet.Remove(typeof(ManyToManyJoinEntityTypeConvention));
                _conventionSet.Remove(typeof(RequiredNavigationAttributeConvention));
                _conventionSet.Remove(typeof(NavigationBackingFieldAttributeConvention));
                _conventionSet.Remove(typeof(InversePropertyAttributeConvention));
                _conventionSet.Remove(typeof(NavigationEagerLoadingConvention));
                _conventionSet.Remove(typeof(NonNullableNavigationConvention));
                _conventionSet.Remove(typeof(NotMappedEntityTypeAttributeConvention));
                _conventionSet.Remove(typeof(OwnedEntityTypeAttributeConvention));
                _conventionSet.Remove(typeof(QueryFilterRewritingConvention));
                _conventionSet.Remove(typeof(ServicePropertyDiscoveryConvention));
                _conventionSet.Remove(typeof(ValueGenerationConvention));
                _conventionSet.Remove(typeof(BaseTypeDiscoveryConvention));
                _conventionSet.Remove(typeof(DiscriminatorConvention));
            }

            return _conventionSet;
        }
    }

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
