// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a navigation property which can be used to navigate a relationship.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class RuntimeNavigation : RuntimePropertyBase, INavigation
{
    // Warning: Never access these fields directly as access needs to be thread-safe
    private IClrCollectionAccessor? _collectionAccessor;
    private bool _collectionAccessorInitialized;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public RuntimeNavigation(
        string name,
        Type clrType,
        PropertyInfo? propertyInfo,
        FieldInfo? fieldInfo,
        RuntimeForeignKey foreignKey,
        PropertyAccessMode propertyAccessMode,
        bool eagerLoaded,
        bool lazyLoadingEnabled)
        : base(name, propertyInfo, fieldInfo, propertyAccessMode)
    {
        ClrType = clrType;
        ForeignKey = foreignKey;
        if (eagerLoaded)
        {
            SetAnnotation(CoreAnnotationNames.EagerLoaded, true);
        }

        if (!lazyLoadingEnabled)
        {
            SetAnnotation(CoreAnnotationNames.LazyLoadingEnabled, false);
        }
    }

    /// <summary>
    ///     Gets the type of value that this navigation holds.
    /// </summary>
    [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)]
    protected override Type ClrType { get; }

    /// <summary>
    ///     Gets the foreign key that defines the relationship this navigation property will navigate.
    /// </summary>
    public virtual RuntimeForeignKey ForeignKey { get; }

    /// <summary>
    ///     Gets the entity type that this navigation property belongs to.
    /// </summary>
    public virtual RuntimeEntityType DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => ((IReadOnlyNavigation)this).IsOnDependent ? ForeignKey.DeclaringEntityType : ForeignKey.PrincipalEntityType;
    }

    /// <inheritdoc />
    public override RuntimeTypeBase DeclaringType
        => DeclaringEntityType;

    /// <inheritdoc />
    public override object? Sentinel
        => null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetCollectionAccessor<TEntity, TCollection, TElement>(
        Func<TEntity, TCollection>? getCollection,
        Action<TEntity, TCollection>? setCollection,
        Action<TEntity, TCollection>? setCollectionForMaterialization,
        Func<TEntity, Action<TEntity, TCollection>, TCollection>? createAndSetCollection,
        Func<TCollection>? createCollection)
        where TEntity : class
        where TCollection : class, IEnumerable<TElement>
        where TElement : class
    {
        _collectionAccessor = new ClrICollectionAccessor<TEntity, TCollection, TElement>(
            Name,
            ((INavigation)this).IsShadowProperty(),
            getCollection,
            setCollection,
            setCollectionForMaterialization,
            createAndSetCollection,
            createCollection);
        _collectionAccessorInitialized = true;
    }

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
        => ((IReadOnlyNavigation)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((IReadOnlyNavigation)this).ToDebugString(),
            () => ((IReadOnlyNavigation)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    IReadOnlyForeignKey IReadOnlyNavigation.ForeignKey
    {
        [DebuggerStepThrough]
        get => ForeignKey;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IClrCollectionAccessor? INavigationBase.GetCollectionAccessor()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _collectionAccessor,
            ref _collectionAccessorInitialized,
            this,
            static navigation => ((INavigationBase)navigation).IsCollection
                ? RuntimeFeature.IsDynamicCodeSupported
                    ? ClrCollectionAccessorFactory.Instance.Create(navigation)
                    : throw new InvalidOperationException(CoreStrings.NativeAotNoCompiledModel)
                : null);
}
