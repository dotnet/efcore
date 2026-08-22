// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that ensures that the declaring key is current for the key overrides.
/// </summary>
/// <remarks>
///     <para>
///         Keys are routinely detached and re-created while the model is being built (for example when a base
///         type is assigned). <see cref="InternalKeyBuilder.Attach" /> unconditionally copies annotations from the
///         detached key onto the target key via <c>MergeAnnotationsFrom</c> — whether that target is a newly added
///         key or an existing declared key that got reused instead. That copy is a raw annotation-value copy: it
///         does not know that a <see cref="RelationalKeyOverrides" /> value embeds a back-pointer to the key it was
///         created for, so the copied value's <see cref="IConventionRelationalKeyOverrides.Key" /> can end up
///         pointing at the dead, detached key. This convention reacts to <see cref="IKeyAnnotationChangedConvention" />
///         for the overrides annotation specifically: that annotation write is what <c>MergeAnnotationsFrom</c>
///         performs whenever a key carrying configured overrides is attached, and it fires for both the
///         newly-added-key and the reused-key case alike, which is what makes it a reliable single signal that a
///         stale override may need to be re-pointed at the current key.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> and
///         <see href="https://aka.ms/efcore-docs-keys">Keys</see> for more information and examples.
///     </para>
/// </remarks>
public class KeyOverridesConvention : IKeyAnnotationChangedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="KeyOverridesConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public KeyOverridesConvention(
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
    public virtual void ProcessKeyAnnotationChanged(
        IConventionKeyBuilder keyBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (name != RelationalAnnotationNames.KeyOverrides)
        {
            return;
        }

        var key = keyBuilder.Metadata;

        List<IConventionRelationalKeyOverrides>? overridesToReattach = null;
        foreach (var overrides in key.GetOverrides())
        {
            var conventionOverrides = (IConventionRelationalKeyOverrides)overrides;
            if (conventionOverrides.Key == key)
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
            var removedOverrides = ((IMutableKey)key).RemoveOverrides(overrides.StoreObject);
            if (removedOverrides != null)
            {
                RelationalKeyOverrides.Attach(key, (IConventionRelationalKeyOverrides)removedOverrides);
            }
        }
    }
}
