// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that creates linking relationships for entity splitting.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> and
///     <see href="https://aka.ms/efcore-docs-inheritance">Entity type hierarchy mapping</see> for more information and examples.
/// </remarks>
public class EntitySplittingConvention : IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="EntitySplittingConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public EntitySplittingConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
    {
        Dependencies = dependencies;
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            if (!entityType.GetMappingFragments().Any()
                || entityType.GetTableName() == null)
            {
                continue;
            }

            var pk = entityType.FindPrimaryKey();
            if (pk != null
                && !entityType.FindDeclaredForeignKeys(pk.Properties)
                    .Any(fk => fk.PrincipalKey.IsPrimaryKey()
                        && fk.PrincipalEntityType.IsAssignableFrom(entityType)
                        && fk.PrincipalEntityType != entityType))
            {
                entityType.Builder.HasRelationship(entityType, pk.Properties, pk)
                    ?.IsUnique(true)
                    ?.IsRequiredDependent(true);
            }
        }
    }
}
