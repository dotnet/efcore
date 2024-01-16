// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Creates ad-hoc mappings of CLR types to entity types after the model has been built.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public class AdHocMapper : IAdHocMapper
{
    private ConventionSet? _conventionSet;

    /// <summary>
    ///     Do not call this constructor directly from either provider or application code as it may change
    ///     as new dependencies are added. Instead, use this type in your constructor so that an instance
    ///     will be created and injected automatically by the dependency injection container. To create
    ///     an instance with some dependent services replaced, first resolve the object from the dependency
    ///     injection container, then replace selected services using the C# 'with' operator. Do not call
    ///     the constructor at any point in this process.
    /// </summary>
    public AdHocMapper(AdHocMapperDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual AdHocMapperDependencies Dependencies { get; }

    /// <summary>
    ///     Builds the convention set to be used by the ad-hoc mapper.
    /// </summary>
    /// <returns>The convention set.</returns>
    public virtual ConventionSet BuildConventionSet()
    {
        var conventionSet = Dependencies.ModelCreationDependencies.ConventionSetBuilder.CreateConventionSet();
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

    /// <inheritdoc />
    public virtual RuntimeEntityType GetOrAddEntityType(Type clrType)
    {
        Check.DebugAssert(Dependencies.Model is RuntimeModel, "Ad-hoc entity types can only be used at runtime.");

        return ((RuntimeModel)Dependencies.Model).FindAdHocEntityType(clrType) ?? AddEntityType(clrType);
    }

    private RuntimeEntityType AddEntityType(Type clrType)
    {
        var modelCreationDependencies = Dependencies.ModelCreationDependencies;
        var modelBuilder = new ModelBuilder(ConventionSet, modelCreationDependencies.ModelDependencies);
        modelBuilder.HasAnnotation(CoreAnnotationNames.AdHocModel, true);
        modelBuilder.Entity(clrType).HasNoKey();
        var finalizedModel = modelBuilder.FinalizeModel();
        var runtimeModel = modelCreationDependencies.ModelRuntimeInitializer.Initialize(
            finalizedModel, designTime: false, modelCreationDependencies.ValidationLogger);

        return ((RuntimeModel)Dependencies.Model).GetOrAddAdHocEntityType((RuntimeEntityType)runtimeModel.FindEntityType(clrType)!);
    }
}
