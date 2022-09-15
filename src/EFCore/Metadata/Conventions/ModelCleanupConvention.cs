// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that removes any state that is only used during model building.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class ModelCleanupConvention :
    IForeignKeyRemovedConvention,
    IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="ModelCleanupConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public ModelCleanupConvention(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessForeignKeyRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionForeignKey foreignKey,
        IConventionContext<IConventionForeignKey> context)
    {
        var principalKey = foreignKey.PrincipalKey;
        if (principalKey.IsInModel
            && !principalKey.IsPrimaryKey()
            && !principalKey.GetReferencingForeignKeys().Any())
        {
            principalKey.DeclaringEntityType.Builder.HasNoKey(principalKey);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        RemoveEntityTypesUnreachableByNavigations(modelBuilder);
        RemoveNavigationlessForeignKeys(modelBuilder);
    }

    private static void RemoveEntityTypesUnreachableByNavigations(
        IConventionModelBuilder modelBuilder)
    {
        var model = modelBuilder.Metadata;
        var rootEntityTypes = GetRoots(model, ConfigurationSource.DataAnnotation);

        foreach (var orphan in new GraphAdapter(model).GetUnreachableVertices(rootEntityTypes))
        {
            modelBuilder.HasNoEntityType(orphan, fromDataAnnotation: true);
        }
    }

    private static IReadOnlyList<IConventionEntityType> GetRoots(IConventionModel model, ConfigurationSource configurationSource)
    {
        var roots = new List<IConventionEntityType>();
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var entityType in model.GetEntityTypes())
        {
            var currentConfigurationSource = entityType.GetConfigurationSource();
            if (currentConfigurationSource.Overrides(configurationSource))
            {
                roots.Add(entityType);
            }
        }

        return roots;
    }

    private static void RemoveNavigationlessForeignKeys(IConventionModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var foreignKey in entityType.GetDeclaredForeignKeys().ToList())
            {
                if (foreignKey.PrincipalToDependent == null
                    && foreignKey.DependentToPrincipal == null
                    && !foreignKey.GetReferencingSkipNavigations().Any())
                {
                    entityType.Builder.HasNoRelationship(foreignKey, fromDataAnnotation: true);
                }
            }
        }
    }

    private sealed class GraphAdapter : Graph<IConventionEntityType>
    {
        private readonly IConventionModel _model;

        public GraphAdapter(IConventionModel model)
        {
            _model = model;
        }

        public override IEnumerable<IConventionEntityType> Vertices
            => _model.GetEntityTypes();

        public override IEnumerable<IConventionEntityType> GetOutgoingNeighbors(IConventionEntityType from)
            => from.GetForeignKeys().Where(fk => fk.DependentToPrincipal != null).Select(fk => fk.PrincipalEntityType)
                .Union(
                    from.GetReferencingForeignKeys().Where(fk => fk.PrincipalToDependent != null).Select(fk => fk.DeclaringEntityType))
                .Union(from.GetSkipNavigations().Where(sn => sn.ForeignKey != null).Select(sn => sn.ForeignKey!.DeclaringEntityType))
                .Union(from.GetSkipNavigations().Select(sn => sn.TargetEntityType));

        public override IEnumerable<IConventionEntityType> GetIncomingNeighbors(IConventionEntityType to)
            => to.GetForeignKeys().Where(fk => fk.PrincipalToDependent != null).Select(fk => fk.PrincipalEntityType)
                .Union(to.GetReferencingForeignKeys().Where(fk => fk.DependentToPrincipal != null).Select(fk => fk.DeclaringEntityType))
                .Union(to.GetSkipNavigations().Where(sn => sn.ForeignKey != null).Select(sn => sn.ForeignKey!.DeclaringEntityType))
                .Union(to.GetSkipNavigations().Select(sn => sn.TargetEntityType));

        public override void Clear()
        {
        }
    }
}
