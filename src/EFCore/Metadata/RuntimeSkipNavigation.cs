// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a navigation property that is part of a relationship
///     that is forwarded through a third entity type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class RuntimeSkipNavigation : RuntimePropertyBase, IRuntimeSkipNavigation
{
    private readonly RuntimeForeignKey _foreignKey;
    private readonly bool _isOnDependent;
    private readonly bool _isCollection;

    // Warning: Never access these fields directly as access needs to be thread-safe
    private IClrCollectionAccessor? _collectionAccessor;
    private bool _collectionAccessorInitialized;
    private ICollectionLoader? _manyToManyLoader;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public RuntimeSkipNavigation(
        string name,
        Type clrType,
        PropertyInfo? propertyInfo,
        FieldInfo? fieldInfo,
        RuntimeEntityType declaringEntityType,
        RuntimeEntityType targetEntityType,
        RuntimeForeignKey foreignKey,
        bool collection,
        bool onDependent,
        PropertyAccessMode propertyAccessMode,
        bool eagerLoaded,
        bool lazyLoadingEnabled)
        : base(name, propertyInfo, fieldInfo, propertyAccessMode)
    {
        ClrType = clrType;
        DeclaringEntityType = declaringEntityType;
        TargetEntityType = targetEntityType;
        _foreignKey = foreignKey;
        if (foreignKey.ReferencingSkipNavigations == null)
        {
            foreignKey.ReferencingSkipNavigations = new SortedSet<RuntimeSkipNavigation>(SkipNavigationComparer.Instance) { this };
        }
        else
        {
            foreignKey.ReferencingSkipNavigations.Add(this);
        }

        _isCollection = collection;
        _isOnDependent = onDependent;
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
    ///     Gets the type that this property belongs to.
    /// </summary>
    public override RuntimeEntityType DeclaringEntityType { get; }

    /// <summary>
    ///     Gets the entity type that this navigation property will hold an instance(s) of.
    /// </summary>
    public virtual RuntimeEntityType TargetEntityType { get; }

    /// <summary>
    ///     Gets or sets the inverse navigation.
    /// </summary>
    [DisallowNull]
    public virtual RuntimeSkipNavigation? Inverse { get; set; }

    /// <inheritdoc />
    public override object? Sentinel
        => null;

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
        => ((IReadOnlySkipNavigation)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DebugView DebugView
        => new(
            () => ((IReadOnlySkipNavigation)this).ToDebugString(),
            () => ((IReadOnlySkipNavigation)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    IReadOnlyEntityType IReadOnlyNavigationBase.DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => DeclaringEntityType;
    }

    /// <inheritdoc />
    IReadOnlyEntityType IReadOnlyNavigationBase.TargetEntityType
    {
        [DebuggerStepThrough]
        get => TargetEntityType;
    }

    /// <inheritdoc />
    IReadOnlyForeignKey IReadOnlySkipNavigation.ForeignKey
    {
        [DebuggerStepThrough]
        get => _foreignKey;
    }

    /// <inheritdoc />
    IReadOnlySkipNavigation IReadOnlySkipNavigation.Inverse
    {
        [DebuggerStepThrough]
        get => Inverse!;
    }

    /// <inheritdoc />
    bool IReadOnlySkipNavigation.IsOnDependent
    {
        [DebuggerStepThrough]
        get => _isOnDependent;
    }

    /// <inheritdoc />
    bool IReadOnlyNavigationBase.IsCollection
    {
        [DebuggerStepThrough]
        get => _isCollection;
    }

    /// <inheritdoc />
    IClrCollectionAccessor? INavigationBase.GetCollectionAccessor()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _collectionAccessor,
            ref _collectionAccessorInitialized,
            this,
            static navigation =>
            {
                navigation.EnsureReadOnly();
                return new ClrCollectionAccessorFactory().Create(navigation);
            });

    /// <inheritdoc />
    ICollectionLoader IRuntimeSkipNavigation.GetManyToManyLoader()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _manyToManyLoader, this, static navigation =>
            {
                navigation.EnsureReadOnly();
                return new ManyToManyLoaderFactory().Create(navigation);
            });
}
