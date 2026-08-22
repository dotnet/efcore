// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that ensures that the declaring foreign key is current for the foreign key overrides.
/// </summary>
/// <remarks>
///     <para>
///         Foreign keys are routinely detached and re-created while the model is being built (for example when a
///         base type is assigned or a relationship is reconfigured). The internal <c>Attach</c> machinery creates
///         the new foreign key and copies annotations from the detached one via <c>MergeAnnotationsFrom</c>, but
///         that copy is a raw annotation-value copy: it does not know that a
///         <see cref="RelationalForeignKeyOverrides" /> value embeds a back-pointer to the foreign key it was
///         created for. When the target foreign key already exists (a matching relationship is found and reused
///         rather than a new one being added) no <see cref="IForeignKeyAddedConvention" /> is raised at all, so
///         this convention also reacts to <see cref="IForeignKeyAnnotationChangedConvention" /> for the overrides
///         annotation specifically, which fires whenever <c>MergeAnnotationsFrom</c> copies it onto either a new
///         or a reused foreign key.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> and
///         <see href="https://aka.ms/efcore-docs-relationships">Relationships</see> for more information and examples.
///     </para>
/// </remarks>
public class ForeignKeyOverridesConvention : IForeignKeyAddedConvention, IForeignKeyAnnotationChangedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="ForeignKeyOverridesConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public ForeignKeyOverridesConvention(
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
    public virtual void ProcessForeignKeyAdded(
        IConventionForeignKeyBuilder foreignKeyBuilder,
        IConventionContext<IConventionForeignKeyBuilder> context)
        => ReattachStaleOverrides(foreignKeyBuilder.Metadata);

    /// <inheritdoc />
    public virtual void ProcessForeignKeyAnnotationChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (name == RelationalAnnotationNames.ForeignKeyOverrides)
        {
            ReattachStaleOverrides(relationshipBuilder.Metadata);
        }
    }

    private static void ReattachStaleOverrides(IConventionForeignKey foreignKey)
    {
        List<IConventionRelationalForeignKeyOverrides>? overridesToReattach = null;
        foreach (var overrides in RelationalForeignKeyOverrides.Get(foreignKey) ?? [])
        {
            var conventionOverrides = (IConventionRelationalForeignKeyOverrides)overrides;
            if (conventionOverrides.ForeignKey == foreignKey)
            {
                continue;
            }

            overridesToReattach ??= [];
            overridesToReattach.Add(conventionOverrides);
        }

        if (overridesToReattach == null)
        {
            return;
        }

        foreach (var overrides in overridesToReattach)
        {
            var removedOverrides = RelationalForeignKeyOverrides.Remove((IMutableForeignKey)foreignKey, overrides.StoreObjects);
            if (removedOverrides != null)
            {
                RelationalForeignKeyOverrides.Attach(foreignKey, removedOverrides);
            }
        }
    }
}
