// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
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
public abstract class RuntimePropertyBase : RuntimeAnnotatableBase, IRuntimePropertyBase
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
    public abstract RuntimeTypeBase DeclaringType { get; }

    /// <summary>
    ///     Gets the type of value that this property-like object holds.
    /// </summary>
    [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)]
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
    public abstract object? Sentinel { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetPropertyIndexes(int index, int originalValueIndex, int shadowIndex, int relationshipIndex, int storeGenerationIndex)
        => _indexes = new(index, originalValueIndex, shadowIndex, relationshipIndex, storeGenerationIndex);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetAccessors<TProperty>(
        Func<InternalEntityEntry, TProperty> currentValueGetter,
        Func<InternalEntityEntry, TProperty> preStoreGeneratedCurrentValueGetter,
        Func<InternalEntityEntry, TProperty>? originalValueGetter,
        Func<InternalEntityEntry, TProperty> relationshipSnapshotGetter,
        Func<ValueBuffer, object>? valueBufferGetter)
        => _accessors = new(
            currentValueGetter,
            preStoreGeneratedCurrentValueGetter,
            originalValueGetter,
            relationshipSnapshotGetter,
            valueBufferGetter);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetMaterializationSetter<TEntity, TValue>(Action<TEntity, TValue> setter)
        where TEntity : class
        => _materializationSetter = new ClrPropertySetter<TEntity, TValue>(setter);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetSetter<TEntity, TValue>(Action<TEntity, TValue> setter)
        where TEntity : class
        => _setter = new ClrPropertySetter<TEntity, TValue>(setter);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetGetter<TEntity, TStructuralType, TValue>(
        Func<TEntity, TValue> getter,
        Func<TEntity, bool> hasDefaultValue,
        Func<TStructuralType, TValue> structuralTypeGetter,
        Func<TStructuralType, bool> hasStructuralTypeSentinelValue)
        where TEntity : class
        => _getter = new ClrPropertyGetter<TEntity, TStructuralType, TValue>(
            getter, hasDefaultValue, structuralTypeGetter, hasStructuralTypeSentinelValue);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    public virtual void SetCurrentValueComparer(IComparer<IUpdateEntry> comparer)
        => _currentValueComparer = comparer;

    /// <inheritdoc />
    [DebuggerStepThrough]
    IComparer<IUpdateEntry> IPropertyBase.GetCurrentValueComparer()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _currentValueComparer, this, static property =>
                CurrentValueComparerFactory.Instance.Create(property));

    /// <inheritdoc />
    IReadOnlyTypeBase IReadOnlyPropertyBase.DeclaringType
    {
        [DebuggerStepThrough]
        get => DeclaringType;
    }

    /// <inheritdoc />
    IClrPropertySetter IRuntimePropertyBase.MaterializationSetter
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _materializationSetter, this, static property =>
                RuntimeFeature.IsDynamicCodeSupported
                    ? ClrPropertyMaterializationSetterFactory.Instance.Create(property)
                    : throw new InvalidOperationException(CoreStrings.NativeAotNoCompiledModel));

    /// <inheritdoc />
    PropertyAccessors IRuntimePropertyBase.Accessors
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _accessors, this, static property =>
                RuntimeFeature.IsDynamicCodeSupported
                    ? PropertyAccessorsFactory.Instance.Create(property)
                    : throw new InvalidOperationException(CoreStrings.NativeAotNoCompiledModel));

    /// <inheritdoc />
    PropertyIndexes IRuntimePropertyBase.PropertyIndexes
    {
        get => NonCapturingLazyInitializer.EnsureInitialized(
            ref _indexes, this,
            static property =>
            {
                _ = ((IRuntimeEntityType)((IRuntimeTypeBase)property.DeclaringType).ContainingEntityType).Counts;
            });
        set => NonCapturingLazyInitializer.EnsureInitialized(ref _indexes, value);
    }

    /// <inheritdoc />
    [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)]
    Type IReadOnlyPropertyBase.ClrType
    {
        [DebuggerStepThrough]
        get => ClrType;
    }

    /// <inheritdoc />
    IClrPropertySetter IRuntimePropertyBase.GetSetter()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _setter, this, static property => RuntimeFeature.IsDynamicCodeSupported
                ? ClrPropertySetterFactory.Instance.Create(property)
                : throw new InvalidOperationException(CoreStrings.NativeAotNoCompiledModel));

    /// <inheritdoc />
    [DebuggerStepThrough]
    IClrPropertyGetter IPropertyBase.GetGetter()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _getter, this, static property => RuntimeFeature.IsDynamicCodeSupported
                ? ClrPropertyGetterFactory.Instance.Create(property)
                : throw new InvalidOperationException(CoreStrings.NativeAotNoCompiledModel));
}
