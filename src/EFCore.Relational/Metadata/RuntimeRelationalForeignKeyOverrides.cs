// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents foreign key facet overrides for a particular pair of table-like store objects.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class RuntimeRelationalForeignKeyOverrides : AnnotatableBase, IRelationalForeignKeyOverrides
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RuntimeRelationalForeignKeyOverrides" /> class.
    /// </summary>
    /// <param name="foreignKey">The foreign key for which the overrides are applied.</param>
    /// <param name="storeObjects">The pair of store objects for which the configuration is applied.</param>
    /// <param name="isNameOverridden">Whether the foreign key constraint name is overridden.</param>
    /// <param name="name">The foreign key constraint name.</param>
    public RuntimeRelationalForeignKeyOverrides(
        RuntimeForeignKey foreignKey,
        in StoreObjectPair storeObjects,
        bool isNameOverridden,
        string? name)
    {
        ForeignKey = foreignKey;
        StoreObjects = storeObjects;
        if (isNameOverridden)
        {
            SetAnnotation(RelationalAnnotationNames.Name, name);
        }
    }

    /// <summary>
    ///     Gets the foreign key for which the overrides are applied.
    /// </summary>
    public virtual RuntimeForeignKey ForeignKey { get; }

    /// <inheritdoc />
    public virtual StoreObjectPair StoreObjects { get; }

    /// <inheritdoc />
    public override string ToString()
        => ((IRelationalForeignKeyOverrides)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((IRelationalForeignKeyOverrides)this).ToDebugString(),
            () => ((IRelationalForeignKeyOverrides)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    IForeignKey IRelationalForeignKeyOverrides.ForeignKey
    {
        [DebuggerStepThrough]
        get => ForeignKey;
    }

    /// <inheritdoc />
    IReadOnlyForeignKey IReadOnlyRelationalForeignKeyOverrides.ForeignKey
    {
        [DebuggerStepThrough]
        get => ForeignKey;
    }

    /// <inheritdoc />
    string? IReadOnlyRelationalForeignKeyOverrides.Name
    {
        [DebuggerStepThrough]
        get => (string?)this[RelationalAnnotationNames.Name];
    }

    /// <inheritdoc />
    bool IReadOnlyRelationalForeignKeyOverrides.IsNameOverridden
    {
        [DebuggerStepThrough]
        get => FindAnnotation(RelationalAnnotationNames.Name) != null;
    }
}
