// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a scalar property of an structural type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class RuntimeProperty : RuntimePropertyBase, IRuntimeProperty
{
    private readonly bool _isNullable;
    private readonly ValueGenerated _valueGenerated;
    private readonly bool _isConcurrencyToken;
    private object? _sentinel;
    private object? _sentinelFromProviderValue;
    private readonly PropertySaveBehavior _beforeSaveBehavior;
    private readonly PropertySaveBehavior _afterSaveBehavior;
    private readonly Func<IProperty, ITypeBase, ValueGenerator>? _valueGeneratorFactory;
    private ValueConverter? _valueConverter;
    private readonly ValueComparer? _customValueComparer;
    private ValueComparer? _valueComparer;
    private ValueComparer? _keyValueComparer;
    private ValueComparer? _providerValueComparer;
    private readonly JsonValueReaderWriter? _jsonValueReaderWriter;
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
        RuntimeTypeBase declaringType,
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
        Func<IProperty, ITypeBase, ValueGenerator>? valueGeneratorFactory,
        ValueConverter? valueConverter,
        ValueComparer? valueComparer,
        ValueComparer? keyValueComparer,
        ValueComparer? providerValueComparer,
        JsonValueReaderWriter? jsonValueReaderWriter,
        CoreTypeMapping? typeMapping,
        object? sentinel)
        : base(name, propertyInfo, fieldInfo, propertyAccessMode)
    {
        DeclaringType = declaringType;
        ClrType = clrType;
        _sentinel = sentinel;
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
        _customValueComparer = valueComparer;
        _valueComparer = valueComparer;
        _keyValueComparer = keyValueComparer ?? valueComparer;
        _providerValueComparer = providerValueComparer;
        _jsonValueReaderWriter = jsonValueReaderWriter;
    }

    /// <summary>
    ///     Sets the <see cref="Sentinel" /> value, converting from the provider type if needed.
    /// </summary>
    /// <param name="providerValue">The value, as a provider value if a value converter is being used.</param>
    public virtual void SetSentinelFromProviderValue(object? providerValue)
        => _sentinelFromProviderValue = providerValue;

    /// <summary>
    ///     Sets the element type for this property.
    /// </summary>
    /// <param name="clrType">The type of value the property will hold.</param>
    /// <param name="nullable">A value indicating whether this property can contain <see langword="null" />.</param>
    /// <param name="maxLength">The maximum length of data that is allowed in this property.</param>
    /// <param name="unicode">A value indicating whether or not the property can persist Unicode characters.</param>
    /// <param name="precision">The precision of data that is allowed in this property.</param>
    /// <param name="scale">The scale of data that is allowed in this property.</param>
    /// <param name="providerClrType">
    ///     The type that the property value will be converted to before being sent to the database provider.
    /// </param>
    /// <param name="valueConverter">The custom <see cref="ValueConverter" /> set for this property.</param>
    /// <param name="valueComparer">The <see cref="ValueComparer" /> for this property.</param>
    /// <param name="jsonValueReaderWriter">The <see cref="JsonValueReaderWriter" /> for this property.</param>
    /// <param name="typeMapping">The <see cref="CoreTypeMapping" /> for this property.</param>
    /// <returns>The newly created property.</returns>
    public virtual RuntimeElementType SetElementType(
        Type clrType,
        bool nullable = false,
        int? maxLength = null,
        bool? unicode = null,
        int? precision = null,
        int? scale = null,
        Type? providerClrType = null,
        ValueConverter? valueConverter = null,
        ValueComparer? valueComparer = null,
        JsonValueReaderWriter? jsonValueReaderWriter = null,
        CoreTypeMapping? typeMapping = null)
    {
        var elementType = new RuntimeElementType(
            clrType,
            this,
            nullable,
            maxLength,
            unicode,
            precision,
            scale,
            providerClrType,
            valueConverter,
            valueComparer,
            jsonValueReaderWriter,
            typeMapping);

        SetAnnotation(CoreAnnotationNames.ElementType, elementType);

        IsPrimitiveCollection = true;

        return elementType;
    }

    /// <summary>
    ///     Gets the type of value that this property-like object holds.
    /// </summary>
    [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)]
    protected override Type ClrType { get; }

    /// <inheritdoc />
    public override RuntimeTypeBase DeclaringType { get; }

    /// <inheritdoc />
    public override bool IsCollection => IsPrimitiveCollection;

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
    public virtual ISet<RuntimeForeignKey>? ForeignKeys { get; set; }

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
                RuntimeFeature.IsDynamicCodeSupported
                    ? property.DeclaringType.Model.GetModelDependencies().TypeMappingSource.FindMapping(property)!
                    : throw new InvalidOperationException(CoreStrings.NativeAotNoCompiledModel));
        set => _typeMapping = value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual void SetValueConverter(ValueConverter converter)
        => _valueConverter = converter;

    /// <inheritdoc />
    public virtual ValueComparer GetValueComparer()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _valueComparer, this,
            static property => (property.GetValueComparer(null) ?? property.TypeMapping.Comparer)
                .ToNullableComparer(property.ClrType)!);

    private ValueComparer? GetValueComparer(HashSet<IReadOnlyProperty>? checkedProperties)
    {
        if (_customValueComparer != null)
        {
            return _customValueComparer is IInfrastructure<ValueComparer> underlyingValueComparer
                ? underlyingValueComparer.Instance
                : _customValueComparer;
        }

        var principal = (RuntimeProperty?)this.FindFirstDifferentPrincipal();
        if (principal == null)
        {
            return null;
        }

        if (checkedProperties == null)
        {
            checkedProperties = [];
        }
        else if (checkedProperties.Contains(this))
        {
            return null;
        }

        checkedProperties.Add(this);
        return principal.GetValueComparer(checkedProperties);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual ValueComparer SetComparer(ValueComparer valueComparer)
        => _valueComparer = valueComparer;

    /// <inheritdoc />
    public virtual ValueComparer GetKeyValueComparer()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _keyValueComparer, this,
            static property => (property.GetValueComparer(null) ?? property.TypeMapping.KeyComparer)
                .ToNullableComparer(property.ClrType)!);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual ValueComparer SetKeyComparer(ValueComparer valueComparer)
        => _keyValueComparer = valueComparer;

    private ValueComparer GetProviderValueComparer()
        => _providerValueComparer ??=
            (TypeMapping.Converter?.ProviderClrType ?? ClrType).UnwrapNullableType() == ClrType.UnwrapNullableType()
                ? GetKeyValueComparer()
                : TypeMapping.ProviderValueComparer;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual ValueComparer SetProviderValueComparer(ValueComparer valueComparer)
        => _providerValueComparer = valueComparer;

    /// <inheritdoc />
    public override object? Sentinel
    {
        get
        {
            if (_sentinelFromProviderValue != null)
            {
                var providerValue = _sentinelFromProviderValue;
                _sentinelFromProviderValue = null;
                _sentinel = TypeMapping.Converter!.ConvertFromProvider(providerValue);
            }

            return _sentinel;
        }
    }

    /// <summary>
    ///     Gets the <see cref="JsonValueReaderWriter" /> for this property, or <see langword="null" /> if none is set.
    /// </summary>
    /// <returns>The reader/writer, or <see langword="null" /> if none has been set.</returns>
    public virtual JsonValueReaderWriter? GetJsonValueReaderWriter()
        => _jsonValueReaderWriter;

    /// <summary>
    ///     Gets the configuration for elements of the primitive collection represented by this property.
    /// </summary>
    /// <returns>The configuration for the elements.</returns>
    public virtual IElementType? GetElementType()
        => (IElementType?)this[CoreAnnotationNames.ElementType];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsPrimitiveCollection { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DebugView DebugView
        => new(
            () => ((IReadOnlyProperty)this).ToDebugString(),
            () => ((IReadOnlyProperty)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IReadOnlyProperty)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <inheritdoc />
    IReadOnlyElementType? IReadOnlyProperty.GetElementType()
        => GetElementType();

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
    Func<IProperty, ITypeBase, ValueGenerator>? IReadOnlyProperty.GetValueGeneratorFactory()
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
    [DebuggerStepThrough]
    CoreTypeMapping? IReadOnlyProperty.FindTypeMapping()
        => TypeMapping;

    /// <inheritdoc />
    [DebuggerStepThrough]
    ValueComparer? IReadOnlyProperty.GetValueComparer()
        => GetValueComparer();

    /// <inheritdoc />
    [DebuggerStepThrough]
    ValueComparer? IReadOnlyProperty.GetKeyValueComparer()
        => GetKeyValueComparer();

    /// <inheritdoc />
    [DebuggerStepThrough]
    ValueComparer? IReadOnlyProperty.GetProviderValueComparer()
        => GetProviderValueComparer();

    /// <inheritdoc />
    [DebuggerStepThrough]
    ValueComparer IProperty.GetProviderValueComparer()
        => GetProviderValueComparer();

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
