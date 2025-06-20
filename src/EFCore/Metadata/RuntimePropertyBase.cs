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
    private IClrIndexedCollectionAccessor? _clrIndexedCollectionAccessor;
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

    /// <inheritdoc />
    public abstract bool IsCollection { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetPropertyIndexes(
        int index,
        int originalValueIndex,
        int shadowIndex,
        int relationshipIndex,
        int storeGenerationIndex)
        => _indexes = new PropertyIndexes(index, originalValueIndex, shadowIndex, relationshipIndex, storeGenerationIndex);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetAccessors<TProperty>(
        Func<IInternalEntry, TProperty> currentValueGetter,
        Func<IInternalEntry, TProperty> preStoreGeneratedCurrentValueGetter,
        Func<IInternalEntry, TProperty>? originalValueGetter,
        Func<IInternalEntry, TProperty> relationshipSnapshotGetter)
        => _accessors = new PropertyAccessors(
            this,
            currentValueGetter,
            preStoreGeneratedCurrentValueGetter,
            originalValueGetter,
            relationshipSnapshotGetter);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetMaterializationSetter<TEntity, TValue>(
        Action<TEntity, IReadOnlyList<int>, TValue> setClrValueUsingContainingEntity)
        where TEntity : class
        => _materializationSetter = new ClrPropertySetter<TEntity, TValue>(setClrValueUsingContainingEntity);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetMaterializationSetter<TEntity, TValue>(
        Action<TEntity, TValue> setClrValue)
        where TEntity : class
    {
        Check.DebugAssert(DeclaringType is IEntityType, $"Declaring type for {Name} is not an IEntityType");

        _materializationSetter = new ClrPropertySetter<TEntity, TValue>((e, _, v) => setClrValue(e, v));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetIndexedCollectionAccessor<TEntity, TElement>(
        Func<TEntity, int, TElement> get,
        Action<TEntity, int, TElement> set,
        Action<TEntity, int, TElement> setForMaterialization)
        where TEntity : class
        => _clrIndexedCollectionAccessor = new ClrIndexedCollectionAccessor<TEntity, TElement>(
            Name,
            ((IReadOnlyPropertyBase)this).IsShadowProperty(),
            get,
            set,
            setForMaterialization);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetSetter<TEntity, TValue>(
        Action<TEntity, IReadOnlyList<int>, TValue> setClrValueUsingContainingEntity)
        where TEntity : class
        => _setter = new ClrPropertySetter<TEntity, TValue>(setClrValueUsingContainingEntity);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetSetter<TEntity, TValue>(
        Action<TEntity, TValue> setClrValue)
        where TEntity : class
    {
        Check.DebugAssert(DeclaringType is IEntityType, $"Declaring type for {Name} is not an IEntityType");

        _setter = new ClrPropertySetter<TEntity, TValue>((e, _, v) => setClrValue(e, v));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetGetter<TEntity, TValue>(
        Func<TEntity, TValue> getClrValue,
        Func<TEntity, bool> hasSentinel)
        where TEntity : class
        => SetGetter<TEntity, TEntity, TValue>(
            (e, _) => getClrValue(e),
            (e, _) => hasSentinel(e),
            getClrValue,
            hasSentinel);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetGetter<TEntity, TStructural, TValue>(
        Func<TEntity, IReadOnlyList<int>, TValue> getClrValueUsingContainingEntity,
        Func<TEntity, IReadOnlyList<int>, bool> hasSentinelUsingContainingEntity,
        Func<TStructural, TValue> getClrValue,
        Func<TStructural, bool> hasSentinel)
        where TEntity : class
        => _getter = new ClrPropertyGetter<TEntity, TStructural, TValue>(
            getClrValueUsingContainingEntity, hasSentinelUsingContainingEntity, getClrValue, hasSentinel);

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
    [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)]
    Type IReadOnlyPropertyBase.ClrType
    {
        [DebuggerStepThrough]
        get => ClrType;
    }

    /// <inheritdoc />
    PropertyIndexes IRuntimePropertyBase.PropertyIndexes
    {
        get => NonCapturingLazyInitializer.EnsureInitialized(
            ref _indexes, this,
            static property => ((IRuntimeEntityType)((IRuntimeTypeBase)property.DeclaringType).ContainingEntityType).CalculateCounts());
        set => NonCapturingLazyInitializer.EnsureInitialized(ref _indexes, value);
    }

    /// <inheritdoc />
    PropertyAccessors IRuntimePropertyBase.Accessors
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _accessors, this, static property =>
                RuntimeFeature.IsDynamicCodeSupported
                    ? PropertyAccessorsFactory.Instance.Create(property)
                    : throw new InvalidOperationException(CoreStrings.NativeAotNoCompiledModel));

    /// <inheritdoc />
    IClrPropertySetter IRuntimePropertyBase.MaterializationSetter
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _materializationSetter, this, static property =>
                RuntimeFeature.IsDynamicCodeSupported
                    ? ClrPropertyMaterializationSetterFactory.Instance.Create(property)
                    : throw new InvalidOperationException(CoreStrings.NativeAotNoCompiledModel));

    /// <inheritdoc />
    IClrIndexedCollectionAccessor? IRuntimePropertyBase.GetIndexedCollectionAccessor()
        => IsCollection
            ? NonCapturingLazyInitializer.EnsureInitialized(
                ref _clrIndexedCollectionAccessor, this, static property =>
                RuntimeFeature.IsDynamicCodeSupported
                    ? ClrIndexedCollectionAccessorFactory.Instance.Create(property)!
                    : throw new InvalidOperationException(CoreStrings.NativeAotNoCompiledModel))
            : null;

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
