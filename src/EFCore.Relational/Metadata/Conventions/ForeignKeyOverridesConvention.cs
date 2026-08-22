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
///         base type is assigned or a relationship is reconfigured). The internal <c>Attach</c> machinery
///         (<c>InternalForeignKeyBuilder.Attach</c>, via <c>ReplaceForeignKey</c>) copies annotations from the
///         detached foreign key onto the target via <c>MergeAnnotationsFrom</c> whenever the target differs from
///         the source object — whether that target is a newly added foreign key or an existing, compatible
///         relationship that got reused instead. That copy is a raw annotation-value copy: it does not know that a
///         <see cref="RelationalForeignKeyOverrides" /> value embeds a back-pointer to the foreign key it was
///         created for, so the copied value's <see cref="IConventionRelationalForeignKeyOverrides.ForeignKey" />
///         can end up pointing at the dead, detached foreign key. This convention reacts to
///         <see cref="IForeignKeyAnnotationChangedConvention" /> for the overrides annotation specifically: that
///         annotation write is what <c>MergeAnnotationsFrom</c> performs whenever a foreign key carrying
///         configured overrides is attached, and it fires for both the newly-added and the reused-relationship
///         case alike, which is what makes it a reliable single signal that a stale override may need to be
///         re-pointed at the current foreign key.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> and
///         <see href="https://aka.ms/efcore-docs-relationships">Relationships</see> for more information and examples.
///     </para>
/// </remarks>
public class ForeignKeyOverridesConvention : IForeignKeyAnnotationChangedConvention
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
    public virtual void ProcessForeignKeyAnnotationChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (name != RelationalAnnotationNames.ForeignKeyOverrides)
        {
            return;
        }

        var foreignKey = relationshipBuilder.Metadata;

        List<IConventionRelationalForeignKeyOverrides>? overridesToReattach = null;
        foreach (var overrides in foreignKey.GetOverrides())
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
            var removedOverrides = ((IMutableForeignKey)foreignKey).RemoveOverrides(
                overrides.StoreObjects.DependentStoreObject, overrides.StoreObjects.PrincipalStoreObject);
            if (removedOverrides != null)
            {
                RelationalForeignKeyOverrides.Attach(foreignKey, (IConventionRelationalForeignKeyOverrides)removedOverrides);
            }
        }
    }
}
