// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents key facet overrides for a particular table-like store object.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class RuntimeRelationalKeyOverrides : AnnotatableBase, IRelationalKeyOverrides
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RuntimeRelationalKeyOverrides" /> class.
    /// </summary>
    /// <param name="key">The key for which the overrides are applied.</param>
    /// <param name="storeObject">The store object for which the configuration is applied.</param>
    /// <param name="isNameOverridden">Whether the key constraint name is overridden.</param>
    /// <param name="name">The key constraint name.</param>
    public RuntimeRelationalKeyOverrides(
        RuntimeKey key,
        in StoreObjectIdentifier storeObject,
        bool isNameOverridden,
        string? name)
    {
        Key = key;
        StoreObject = storeObject;
        if (isNameOverridden)
        {
            SetAnnotation(RelationalAnnotationNames.Name, name);
        }
    }

    /// <summary>
    ///     Gets the key for which the overrides are applied.
    /// </summary>
    public virtual RuntimeKey Key { get; }

    /// <inheritdoc />
    public virtual StoreObjectIdentifier StoreObject { get; }

    /// <inheritdoc />
    public override string ToString()
        => ((IRelationalKeyOverrides)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((IRelationalKeyOverrides)this).ToDebugString(),
            () => ((IRelationalKeyOverrides)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    IKey IRelationalKeyOverrides.Key
    {
        [DebuggerStepThrough]
        get => Key;
    }

    /// <inheritdoc />
    IReadOnlyKey IReadOnlyRelationalKeyOverrides.Key
    {
        [DebuggerStepThrough]
        get => Key;
    }

    /// <inheritdoc />
    string? IReadOnlyRelationalKeyOverrides.Name
    {
        [DebuggerStepThrough]
        get => (string?)this[RelationalAnnotationNames.Name];
    }

    /// <inheritdoc />
    bool IReadOnlyRelationalKeyOverrides.IsNameOverridden
    {
        [DebuggerStepThrough]
        get => FindAnnotation(RelationalAnnotationNames.Name) != null;
    }
}
