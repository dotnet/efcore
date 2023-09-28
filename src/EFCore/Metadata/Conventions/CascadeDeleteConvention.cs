// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that sets the delete behavior to <see cref="DeleteBehavior.Cascade" /> for required foreign keys
///     and <see cref="DeleteBehavior.ClientSetNull" /> for optional ones.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class CascadeDeleteConvention : IForeignKeyAddedConvention, IForeignKeyRequirednessChangedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="CascadeDeleteConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public CascadeDeleteConvention(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Called after a foreign key is added to the entity type.
    /// </summary>
    /// <param name="relationshipBuilder">The builder for the foreign key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessForeignKeyAdded(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext<IConventionForeignKeyBuilder> context)
    {
        var newRelationshipBuilder = relationshipBuilder.OnDelete(GetTargetDeleteBehavior(relationshipBuilder.Metadata));
        if (newRelationshipBuilder != null)
        {
            context.StopProcessingIfChanged(newRelationshipBuilder);
        }
    }

    /// <summary>
    ///     Called after the requiredness for a foreign key is changed.
    /// </summary>
    /// <param name="relationshipBuilder">The builder for the foreign key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessForeignKeyRequirednessChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext<bool?> context)
    {
        var newRelationshipBuilder = relationshipBuilder.OnDelete(GetTargetDeleteBehavior(relationshipBuilder.Metadata));
        if (newRelationshipBuilder != null)
        {
            context.StopProcessingIfChanged(newRelationshipBuilder.Metadata.IsRequired);
        }
    }

    /// <summary>
    ///     Returns the delete behavior to set for the given foreign key.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    protected virtual DeleteBehavior GetTargetDeleteBehavior(IConventionForeignKey foreignKey)
        => foreignKey.IsRequired ? DeleteBehavior.Cascade : DeleteBehavior.ClientSetNull;
}
