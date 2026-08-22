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
///         type is assigned). <see cref="InternalKeyBuilder.Attach" /> creates the new key and copies annotations
///         from the detached key via <c>MergeAnnotationsFrom</c>, but that copy is a raw annotation-value copy: it
///         does not know that a <see cref="RelationalKeyOverrides" /> value embeds a back-pointer to the key it was
///         created for. When the target key already exists (<c>InternalKeyBuilder.Attach</c> finds a matching
///         declared key and reuses it rather than adding a new one) no <see cref="IKeyAddedConvention" /> is raised
///         at all, so this convention also reacts to <see cref="IKeyAnnotationChangedConvention" /> for the
///         overrides annotation specifically, which fires whenever <c>MergeAnnotationsFrom</c> copies it onto
///         either a new or a reused key.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> and
///         <see href="https://aka.ms/efcore-docs-keys">Keys</see> for more information and examples.
///     </para>
/// </remarks>
public class KeyOverridesConvention : IKeyAddedConvention, IKeyAnnotationChangedConvention
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
    public virtual void ProcessKeyAdded(
        IConventionKeyBuilder keyBuilder,
        IConventionContext<IConventionKeyBuilder> context)
        => ReattachStaleOverrides(keyBuilder.Metadata);

    /// <inheritdoc />
    public virtual void ProcessKeyAnnotationChanged(
        IConventionKeyBuilder keyBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (name == RelationalAnnotationNames.KeyOverrides)
        {
            ReattachStaleOverrides(keyBuilder.Metadata);
        }
    }

    private static void ReattachStaleOverrides(IConventionKey key)
    {
        List<IConventionRelationalKeyOverrides>? overridesToReattach = null;
        foreach (var overrides in RelationalKeyOverrides.Get(key) ?? [])
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
            var removedOverrides = RelationalKeyOverrides.Remove((IMutableKey)key, overrides.StoreObject);
            if (removedOverrides != null)
            {
                RelationalKeyOverrides.Attach(key, removedOverrides);
            }
        }
    }
}
