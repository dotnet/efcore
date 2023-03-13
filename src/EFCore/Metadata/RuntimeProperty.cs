// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a scalar property of an entity type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class RuntimeProperty : RuntimePropertyBase, IProperty
{
    private readonly bool _isNullable;
    private readonly ValueGenerated _valueGenerated;
    private readonly bool _isConcurrencyToken;
    private readonly PropertySaveBehavior _beforeSaveBehavior;
    private readonly PropertySaveBehavior _afterSaveBehavior;
    private readonly Func<IProperty, IEntityType, ValueGenerator>? _valueGeneratorFactory;
    private readonly ValueConverter? _valueConverter;
    private ValueComparer? _valueComparer;
    private ValueComparer? _keyValueComparer;
    private readonly ValueComparer? _providerValueComparer;
    private CoreTypeMapping? _typeMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public RuntimeProperty(
        string name,
        Type clrType,
        PropertyInfo? propertyInfo,
        FieldInfo? fieldInfo,
        RuntimeEntityType declaringEntityType,
        PropertyAccessMode propertyAccessMode,
        bool nullable,
        bool concurrencyToken,
        ValueGenerated valueGenerated,
        PropertySaveBehavior beforeSaveBehavior,
        PropertySaveBehavior afterSaveBehavior,
        int? maxLength,
        bool? unicode,
        int? precision,
        int? scale,
        Type? providerClrType,
        Func<IProperty, IEntityType, ValueGenerator>? valueGeneratorFactory,
        ValueConverter? valueConverter,
        ValueComparer? valueComparer,
        ValueComparer? keyValueComparer,
        ValueComparer? providerValueComparer,
        CoreTypeMapping? typeMapping)
        : base(name, propertyInfo, fieldInfo, propertyAccessMode)
    {
        DeclaringEntityType = declaringEntityType;
        ClrType = clrType;
        _isNullable = nullable;
        _isConcurrencyToken = concurrencyToken;
        _valueGenerated = valueGenerated;
        _beforeSaveBehavior = beforeSaveBehavior;
        _afterSaveBehavior = afterSaveBehavior;
        _valueGeneratorFactory = valueGeneratorFactory;
        _valueConverter = valueConverter;

        if (maxLength != null)
        {
            SetAnnotation(CoreAnnotationNames.MaxLength, maxLength);
        }

        if (unicode != null)
        {
            SetAnnotation(CoreAnnotationNames.Unicode, unicode);
        }

        if (precision != null)
        {
            SetAnnotation(CoreAnnotationNames.Precision, precision);
        }

        if (scale != null)
        {
            SetAnnotation(CoreAnnotationNames.Scale, scale);
        }

        if (providerClrType != null)
        {
            SetAnnotation(CoreAnnotationNames.ProviderClrType, providerClrType);
        }

        _typeMapping = typeMapping;
        _valueComparer = valueComparer;
        _keyValueComparer = keyValueComparer ?? valueComparer;
        _providerValueComparer = providerValueComparer;
    }

    /// <summary>
    ///     Gets the type of value that this property-like object holds.
    /// </summary>
    [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)]
    protected override Type ClrType { get; }

    /// <summary>
    ///     Gets the type that this property belongs to.
    /// </summary>
    public override RuntimeEntityType DeclaringEntityType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual RuntimeKey? PrimaryKey { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual List<RuntimeKey>? Keys { get; set; }

    private IEnumerable<RuntimeKey> GetContainingKeys()
        => Keys ?? Enumerable.Empty<RuntimeKey>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual List<RuntimeForeignKey>? ForeignKeys { get; set; }

    private IEnumerable<RuntimeForeignKey> GetContainingForeignKeys()
        => ForeignKeys ?? Enumerable.Empty<RuntimeForeignKey>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual List<RuntimeIndex>? Indexes { get; set; }

    private IEnumerable<RuntimeIndex> GetContainingIndexes()
        => Indexes ?? Enumerable.Empty<RuntimeIndex>();

    /// <summary>
    ///     Gets or sets the type mapping for this property.
    /// </summary>
    /// <returns>The type mapping.</returns>
    public virtual CoreTypeMapping TypeMapping
    {
        get => NonCapturingLazyInitializer.EnsureInitialized(
            ref _typeMapping, (IProperty)this,
            static property =>
                property.DeclaringEntityType.Model.GetModelDependencies().TypeMappingSource.FindMapping(property)!);
        set => _typeMapping = value;
    }

    private ValueComparer GetValueComparer()
        => (GetValueComparer(null) ?? TypeMapping.Comparer)
            .ToNullableComparer(this)!;

    private ValueComparer GetKeyValueComparer()
        => (GetKeyValueComparer(null) ?? TypeMapping.KeyComparer)
            .ToNullableComparer(this)!;

    private ValueComparer? GetValueComparer(HashSet<IReadOnlyProperty>? checkedProperties)
    {
        if (_valueComparer != null)
        {
            return _valueComparer;
        }

        var principal = (RuntimeProperty?)this.FindFirstDifferentPrincipal();
        if (principal == null)
        {
            return null;
        }

        if (checkedProperties == null)
        {
            checkedProperties = new HashSet<IReadOnlyProperty>();
        }
        else if (checkedProperties.Contains(this))
        {
            return null;
        }

        checkedProperties.Add(this);
        return principal.GetValueComparer(checkedProperties);
    }

    private ValueComparer? GetKeyValueComparer(HashSet<IReadOnlyProperty>? checkedProperties)
    {
        if ( _keyValueComparer != null)
        {
            return _keyValueComparer;
        }

        var principal = (RuntimeProperty?)this.FindFirstDifferentPrincipal();
        if (principal == null)
        {
            return null;
        }

        if (checkedProperties == null)
        {
            checkedProperties = new HashSet<IReadOnlyProperty>();
        }
        else if (checkedProperties.Contains(this))
        {
            return null;
        }

        checkedProperties.Add(this);
        return principal.GetKeyValueComparer(checkedProperties);
    }

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
        => ((IProperty)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((IProperty)this).ToDebugString(),
            () => ((IProperty)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    bool IReadOnlyProperty.IsNullable
    {
        [DebuggerStepThrough]
        get => _isNullable;
    }

    /// <inheritdoc />
    ValueGenerated IReadOnlyProperty.ValueGenerated
    {
        [DebuggerStepThrough]
        get => _valueGenerated;
    }

    /// <inheritdoc />
    bool IReadOnlyProperty.IsConcurrencyToken
    {
        [DebuggerStepThrough]
        get => _isConcurrencyToken;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    int? IReadOnlyProperty.GetMaxLength()
        => (int?)this[CoreAnnotationNames.MaxLength];

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool? IReadOnlyProperty.IsUnicode()
        => (bool?)this[CoreAnnotationNames.Unicode];

    /// <inheritdoc />
    [DebuggerStepThrough]
    int? IReadOnlyProperty.GetPrecision()
        => (int?)this[CoreAnnotationNames.Precision];

    /// <inheritdoc />
    [DebuggerStepThrough]
    int? IReadOnlyProperty.GetScale()
        => (int?)this[CoreAnnotationNames.Scale];

    /// <inheritdoc />
    [DebuggerStepThrough]
    PropertySaveBehavior IReadOnlyProperty.GetBeforeSaveBehavior()
        => _beforeSaveBehavior;

    /// <inheritdoc />
    [DebuggerStepThrough]
    PropertySaveBehavior IReadOnlyProperty.GetAfterSaveBehavior()
        => _afterSaveBehavior;

    /// <inheritdoc />
    [DebuggerStepThrough]
    Func<IProperty, IEntityType, ValueGenerator>? IReadOnlyProperty.GetValueGeneratorFactory()
        => _valueGeneratorFactory;

    /// <inheritdoc />
    [DebuggerStepThrough]
    ValueConverter? IReadOnlyProperty.GetValueConverter()
        => _valueConverter;

    /// <inheritdoc />
    [DebuggerStepThrough]
    Type? IReadOnlyProperty.GetProviderClrType()
        => (Type?)this[CoreAnnotationNames.ProviderClrType];

    /// <inheritdoc />
    IReadOnlyEntityType IReadOnlyProperty.DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => DeclaringEntityType;
    }

    /// <inheritdoc />
    IEntityType IProperty.DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => DeclaringEntityType;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    CoreTypeMapping? IReadOnlyProperty.FindTypeMapping()
        => TypeMapping;

    /// <inheritdoc />
    [DebuggerStepThrough]
    ValueComparer? IReadOnlyProperty.GetValueComparer()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _valueComparer, this,
            static property => property.GetValueComparer());

    /// <inheritdoc />
    [DebuggerStepThrough]
    ValueComparer IProperty.GetValueComparer()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _valueComparer, this,
            static property => property.GetValueComparer());

    /// <inheritdoc />
    [DebuggerStepThrough]
    ValueComparer? IReadOnlyProperty.GetKeyValueComparer()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _keyValueComparer, this,
            static property => property.GetKeyValueComparer());

    /// <inheritdoc />
    [DebuggerStepThrough]
    ValueComparer IProperty.GetKeyValueComparer()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _keyValueComparer, this,
            static property => property.GetKeyValueComparer());

    /// <inheritdoc />
    [DebuggerStepThrough]
    ValueComparer? IReadOnlyProperty.GetProviderValueComparer()
        => _providerValueComparer ?? TypeMapping.ProviderValueComparer;

    /// <inheritdoc />
    [DebuggerStepThrough]
    ValueComparer IProperty.GetProviderValueComparer()
        => _providerValueComparer ?? TypeMapping.ProviderValueComparer;

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IReadOnlyProperty.IsForeignKey()
        => ForeignKeys != null;

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyForeignKey> IReadOnlyProperty.GetContainingForeignKeys()
        => GetContainingForeignKeys();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IForeignKey> IProperty.GetContainingForeignKeys()
        => GetContainingForeignKeys();

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IReadOnlyProperty.IsIndex()
        => Indexes != null;

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyIndex> IReadOnlyProperty.GetContainingIndexes()
        => GetContainingIndexes();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IIndex> IProperty.GetContainingIndexes()
        => GetContainingIndexes();

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IReadOnlyProperty.IsKey()
        => Keys != null;

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyKey> IReadOnlyProperty.GetContainingKeys()
        => GetContainingKeys();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IKey> IProperty.GetContainingKeys()
        => GetContainingKeys();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyKey? IReadOnlyProperty.FindContainingPrimaryKey()
        => PrimaryKey;
}
