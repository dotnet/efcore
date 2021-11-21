// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Base type for navigations and properties.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public abstract class RuntimePropertyBase : AnnotatableBase, IRuntimePropertyBase
{
    private readonly PropertyInfo? _propertyInfo;
    private readonly FieldInfo? _fieldInfo;
    private readonly PropertyAccessMode _propertyAccessMode = Model.DefaultPropertyAccessMode;

    // Warning: Never access these fields directly as access needs to be thread-safe
    private IClrPropertyGetter? _getter;
    private IClrPropertySetter? _setter;
    private IClrPropertySetter? _materializationSetter;
    private PropertyAccessors? _accessors;
    private PropertyIndexes? _indexes;
    private IComparer<IUpdateEntry>? _currentValueComparer;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected RuntimePropertyBase(
        string name,
        PropertyInfo? propertyInfo,
        FieldInfo? fieldInfo,
        PropertyAccessMode propertyAccessMode)
    {
        Name = name;
        _propertyInfo = propertyInfo;
        _fieldInfo = fieldInfo;
        _propertyAccessMode = propertyAccessMode;
    }

    /// <summary>
    ///     Gets the name of this property-like object.
    /// </summary>
    public virtual string Name { [DebuggerStepThrough] get; }

    /// <summary>
    ///     Gets the type that this property-like object belongs to.
    /// </summary>
    public abstract RuntimeEntityType DeclaringEntityType { get; }

    /// <summary>
    ///     Gets the type of value that this property-like object holds.
    /// </summary>
    protected abstract Type ClrType { get; }

    /// <inheritdoc />
    PropertyInfo? IReadOnlyPropertyBase.PropertyInfo
    {
        [DebuggerStepThrough]
        get => _propertyInfo;
    }

    /// <inheritdoc />
    FieldInfo? IReadOnlyPropertyBase.FieldInfo
    {
        [DebuggerStepThrough]
        get => _fieldInfo;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    PropertyAccessMode IReadOnlyPropertyBase.GetPropertyAccessMode()
        => _propertyAccessMode;

    /// <inheritdoc />
    IReadOnlyTypeBase IReadOnlyPropertyBase.DeclaringType
    {
        [DebuggerStepThrough]
        get => DeclaringEntityType;
    }

    /// <inheritdoc />
    IClrPropertySetter IRuntimePropertyBase.Setter
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _setter, this, static property => new ClrPropertySetterFactory().Create(property));

    /// <inheritdoc />
    IClrPropertySetter IRuntimePropertyBase.MaterializationSetter
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _materializationSetter, this, static property =>
                new ClrPropertyMaterializationSetterFactory().Create(property));

    /// <inheritdoc />
    PropertyAccessors IRuntimePropertyBase.Accessors
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _accessors, this, static property =>
                new PropertyAccessorsFactory().Create(property));

    /// <inheritdoc />
    PropertyIndexes IRuntimePropertyBase.PropertyIndexes
    {
        get => NonCapturingLazyInitializer.EnsureInitialized(
            ref _indexes, this,
            static property =>
            {
                var _ = ((IRuntimeEntityType)property.DeclaringEntityType).Counts;
            });
        set => NonCapturingLazyInitializer.EnsureInitialized(ref _indexes, value);
    }

    /// <inheritdoc />
    Type IReadOnlyPropertyBase.ClrType
    {
        [DebuggerStepThrough]
        get => ClrType;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IClrPropertyGetter IPropertyBase.GetGetter()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _getter, this, static property => new ClrPropertyGetterFactory().Create(property));

    /// <inheritdoc />
    [DebuggerStepThrough]
    IComparer<IUpdateEntry> IPropertyBase.GetCurrentValueComparer()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _currentValueComparer, this, static property =>
                new CurrentValueComparerFactory().Create(property));
}
