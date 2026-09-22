// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents the elements of a collection property.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class RuntimeElementType : RuntimeAnnotatableBase, IElementType
{
    private readonly bool _isNullable;
    private readonly ValueConverter? _valueConverter;
    private readonly ValueComparer? _valueComparer;
    private readonly JsonValueReaderWriter? _jsonValueReaderWriter;
    private readonly CoreTypeMapping? _typeMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public RuntimeElementType(
        Type clrType,
        RuntimeProperty collectionProperty,
        bool nullable,
        int? maxLength,
        bool? unicode,
        int? precision,
        int? scale,
        Type? providerClrType,
        ValueConverter? valueConverter,
        ValueComparer? valueComparer,
        JsonValueReaderWriter? jsonValueReaderWriter,
        CoreTypeMapping? typeMapping)
    {
        CollectionProperty = collectionProperty;
        ClrType = clrType;
        _isNullable = nullable;
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
        _jsonValueReaderWriter = jsonValueReaderWriter;
    }

    /// <summary>
    ///     Gets the collection property for which this represents the element.
    /// </summary>
    public virtual IProperty CollectionProperty { get; }

    /// <inheritdoc />
    public virtual Type ClrType { get; }

    /// <summary>
    ///     Gets a value indicating whether elements of the collection can be <see langword="null" />.
    /// </summary>
    public virtual bool IsNullable
        => _isNullable;

    /// <summary>
    ///     Returns the type mapping for elements of the collection.
    /// </summary>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    public virtual CoreTypeMapping? FindTypeMapping()
        => _typeMapping;

    /// <summary>
    ///     Gets the maximum length of data that is allowed in elements of the collection. For example, if the element type is
    ///     a <see cref="string" /> then this is the maximum number of characters.
    /// </summary>
    /// <returns>
    ///     The maximum length, <c>-1</c> if the property has no maximum length, or <see langword="null" /> if the maximum length hasn't been
    ///     set.
    /// </returns>
    [DebuggerStepThrough]
    public virtual int? GetMaxLength()
        => (int?)this[CoreAnnotationNames.MaxLength];

    /// <summary>
    ///     Gets the precision of data that is allowed in elements of the collection.
    ///     For example, if the element type is a <see cref="decimal" />, then this is the maximum number of digits.
    /// </summary>
    /// <returns>The precision, or <see langword="null" /> if none is defined.</returns>
    [DebuggerStepThrough]
    public virtual int? GetPrecision()
        => (int?)this[CoreAnnotationNames.Precision];

    /// <summary>
    ///     Gets the scale of data that is allowed in this elements of the collection.
    ///     For example, if the element type is a <see cref="decimal" />, then this is the maximum number of decimal places.
    /// </summary>
    /// <returns>The scale, or <see langword="null" /> if none is defined.</returns>
    [DebuggerStepThrough]
    public virtual int? GetScale()
        => (int?)this[CoreAnnotationNames.Scale];

    /// <summary>
    ///     Gets a value indicating whether elements of the collection can persist Unicode characters.
    /// </summary>
    /// <returns>The Unicode setting, or <see langword="null" /> if none is defined.</returns>
    [DebuggerStepThrough]
    public virtual bool? IsUnicode()
        => (bool?)this[CoreAnnotationNames.Unicode];

    /// <summary>
    ///     Gets the custom <see cref="ValueConverter" /> for this elements of the collection.
    /// </summary>
    /// <returns>The converter, or <see langword="null" /> if none has been set.</returns>
    [DebuggerStepThrough]
    public virtual ValueConverter? GetValueConverter()
        => _valueConverter;

    /// <summary>
    ///     Gets the custom <see cref="ValueComparer" /> for elements of the collection.
    /// </summary>
    /// <returns>The comparer, or <see langword="null" /> if none has been set.</returns>
    [DebuggerStepThrough]
    public virtual ValueComparer? GetValueComparer()
        => _valueComparer;

    /// <summary>
    ///     Gets the type that the elements of the collection will be converted to before being sent to the database provider.
    /// </summary>
    /// <returns>The provider type, or <see langword="null" /> if none has been set.</returns>
    public virtual Type? GetProviderClrType()
        => (Type?)FindAnnotation(CoreAnnotationNames.ProviderClrType)?.Value;

    /// <inheritdoc />
    public virtual JsonValueReaderWriter? GetJsonValueReaderWriter()
        => _jsonValueReaderWriter;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DebugView DebugView
        => new(
            () => ((IReadOnlyElementType)this).ToDebugString(),
            () => ((IReadOnlyElementType)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IReadOnlyElementType)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <inheritdoc />
    IReadOnlyProperty IReadOnlyElementType.CollectionProperty
        => CollectionProperty;

    /// <inheritdoc />
    bool IReadOnlyElementType.IsNullable
    {
        [DebuggerStepThrough]
        get => _isNullable;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    int? IReadOnlyElementType.GetMaxLength()
        => (int?)this[CoreAnnotationNames.MaxLength];

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool? IReadOnlyElementType.IsUnicode()
        => (bool?)this[CoreAnnotationNames.Unicode];

    /// <inheritdoc />
    [DebuggerStepThrough]
    int? IReadOnlyElementType.GetPrecision()
        => (int?)this[CoreAnnotationNames.Precision];

    /// <inheritdoc />
    [DebuggerStepThrough]
    int? IReadOnlyElementType.GetScale()
        => (int?)this[CoreAnnotationNames.Scale];

    /// <inheritdoc />
    [DebuggerStepThrough]
    ValueConverter? IReadOnlyElementType.GetValueConverter()
        => _valueConverter;

    /// <inheritdoc />
    [DebuggerStepThrough]
    Type? IReadOnlyElementType.GetProviderClrType()
        => (Type?)this[CoreAnnotationNames.ProviderClrType];

    /// <inheritdoc />
    [DebuggerStepThrough]
    CoreTypeMapping IReadOnlyElementType.FindTypeMapping()
        => FindTypeMapping()!;
}
