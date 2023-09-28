// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents entity type mapping for a particular table-like store object.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class RuntimeEntityTypeMappingFragment : AnnotatableBase, IEntityTypeMappingFragment
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RuntimeEntityTypeMappingFragment" /> class.
    /// </summary>
    /// <param name="entityType">The entity type for which the fragment is defined.</param>
    /// <param name="storeObject">The store object for which the configuration is applied.</param>
    /// <param name="isTableExcludedFromMigrations">
    ///     A value indicating whether the associated table is ignored by Migrations.
    /// </param>
    public RuntimeEntityTypeMappingFragment(
        RuntimeEntityType entityType,
        in StoreObjectIdentifier storeObject,
        bool? isTableExcludedFromMigrations)
    {
        EntityType = entityType;
        StoreObject = storeObject;
        if (isTableExcludedFromMigrations != null)
        {
            SetAnnotation(RelationalAnnotationNames.IsTableExcludedFromMigrations, isTableExcludedFromMigrations.Value);
        }
    }

    /// <summary>
    ///     Gets the entity type for which the fragment is defined.
    /// </summary>
    public virtual RuntimeEntityType EntityType { get; }

    /// <inheritdoc />
    public virtual StoreObjectIdentifier StoreObject { get; }

    /// <inheritdoc />
    public virtual bool? IsTableExcludedFromMigrations
        => (bool?)this[RelationalAnnotationNames.IsTableExcludedFromMigrations];

    /// <inheritdoc />
    public override string ToString()
        => ((IEntityTypeMappingFragment)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((IEntityTypeMappingFragment)this).ToDebugString(),
            () => ((IEntityTypeMappingFragment)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    IEntityType IEntityTypeMappingFragment.EntityType
    {
        [DebuggerStepThrough]
        get => EntityType;
    }

    /// <inheritdoc />
    IReadOnlyEntityType IReadOnlyEntityTypeMappingFragment.EntityType
    {
        [DebuggerStepThrough]
        get => EntityType;
    }
}
