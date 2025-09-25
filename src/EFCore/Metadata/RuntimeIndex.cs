// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents an index on a set of properties.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class RuntimeIndex : RuntimeAnnotatableBase, IIndex
{
    private readonly bool _isUnique;

    // Warning: Never access these fields directly as access needs to be thread-safe
    private object? _nullableValueFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public RuntimeIndex(
        IReadOnlyList<RuntimeProperty> properties,
        RuntimeEntityType declaringEntityType,
        string? name,
        bool unique)
    {
        Properties = properties;
        Name = name;
        DeclaringEntityType = declaringEntityType;
        _isUnique = unique;
    }

    /// <summary>
    ///     Gets the properties that this index is defined on.
    /// </summary>
    public virtual IReadOnlyList<RuntimeProperty> Properties { get; }

    /// <summary>
    ///     Gets the name of this index.
    /// </summary>
    public virtual string? Name { get; }

    /// <summary>
    ///     Gets the entity type the index is defined on. This may be different from the type that <see cref="Properties" />
    ///     are defined on when the index is defined a derived type in an inheritance hierarchy (since the properties
    ///     may be defined on a base type).
    /// </summary>
    public virtual RuntimeEntityType DeclaringEntityType { get; }

    /// <summary>
    ///     Always returns an empty array for <see cref="RuntimeIndex" />.
    /// </summary>
    IReadOnlyList<bool> IReadOnlyIndex.IsDescending
    {
        [DebuggerStepThrough]
        get => throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
    }

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
        => ((IIndex)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((IIndex)this).ToDebugString(),
            () => ((IIndex)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    IReadOnlyList<IReadOnlyProperty> IReadOnlyIndex.Properties
    {
        [DebuggerStepThrough]
        get => Properties;
    }

    /// <inheritdoc />
    IReadOnlyEntityType IReadOnlyIndex.DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => DeclaringEntityType;
    }

    /// <inheritdoc />
    IReadOnlyList<IProperty> IIndex.Properties
    {
        [DebuggerStepThrough]
        get => Properties;
    }

    /// <inheritdoc />
    IEntityType IIndex.DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => DeclaringEntityType;
    }

    /// <inheritdoc />
    bool IReadOnlyIndex.IsUnique
    {
        [DebuggerStepThrough]
        get => _isUnique;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IDependentKeyValueFactory<TKey> IIndex.GetNullableValueFactory<TKey>()
        => (IDependentKeyValueFactory<TKey>)NonCapturingLazyInitializer.EnsureInitialized(
            ref _nullableValueFactory, this, static index => new CompositeValueFactory(index.Properties));
}
