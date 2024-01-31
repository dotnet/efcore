// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     Describes metadata needed to decide on a relational type mapping for
///     a property, type, or provider-specific relational type name.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public readonly record struct RelationalTypeMappingInfo
{
    private readonly TypeMappingInfo _coreTypeMappingInfo;

    /// <summary>
    ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" />.
    /// </summary>
    /// <param name="property">The property for which mapping is needed.</param>
    public RelationalTypeMappingInfo(IProperty property)
        : this(property.GetPrincipals())
    {
    }

    /// <summary>
    ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" />.
    /// </summary>
    /// <param name="elementType">The collection element for which mapping is needed.</param>
    /// <param name="storeTypeName">The provider-specific relational type name for which mapping is needed.</param>
    /// <param name="storeTypeNameBase">The provider-specific relational type name, with any facets removed.</param>
    /// <param name="fallbackUnicode">Specifies Unicode or ANSI for the mapping or <see langword="null" /> for the default.</param>
    /// <param name="fallbackFixedLength">Specifies a fixed length mapping, or <see langword="null" /> for the default.</param>
    /// <param name="fallbackSize">
    ///     Specifies a size for the mapping, in case one isn't found at the core level, or <see langword="null" /> for the
    ///     default.
    /// </param>
    /// <param name="fallbackPrecision">
    ///     Specifies a precision for the mapping, in case one isn't found at the core level, or
    ///     <see langword="null" /> for the default.
    /// </param>
    /// <param name="fallbackScale">
    ///     Specifies a scale for the mapping, in case one isn't found at the core level, or <see langword="null" /> for
    ///     the default.
    /// </param>
    public RelationalTypeMappingInfo(
        IElementType elementType,
        string? storeTypeName = null,
        string? storeTypeNameBase = null,
        bool? fallbackUnicode = null,
        bool? fallbackFixedLength = null,
        int? fallbackSize = null,
        int? fallbackPrecision = null,
        int? fallbackScale = null)
    {
        _coreTypeMappingInfo = new TypeMappingInfo(elementType, fallbackUnicode, fallbackSize, fallbackPrecision, fallbackScale);

        fallbackFixedLength ??= elementType.IsFixedLength();
        storeTypeName ??= (string?)elementType[RelationalAnnotationNames.StoreType];

        var customConverter = elementType.GetValueConverter();
        var mappingHints = customConverter?.MappingHints;

        IsFixedLength = fallbackFixedLength ?? (mappingHints as RelationalConverterMappingHints)?.IsFixedLength;
        DbType = (mappingHints as RelationalConverterMappingHints)?.DbType;
        StoreTypeName = storeTypeName;
        StoreTypeNameBase = storeTypeNameBase;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" />.
    /// </summary>
    /// <param name="principals">The principal property chain for the property for which mapping is needed.</param>
    /// <param name="storeTypeName">The provider-specific relational type name for which mapping is needed.</param>
    /// <param name="storeTypeNameBase">The provider-specific relational type name, with any facets removed.</param>
    /// <param name="fallbackUnicode">
    ///     Specifies Unicode or ANSI for the mapping or <see langword="null" /> for default.
    /// </param>
    /// <param name="fallbackFixedLength">Specifies a fixed length mapping, or <see langword="null" /> for default.</param>
    /// <param name="fallbackSize">
    ///     Specifies a size for the mapping, in case one isn't found at the core level, or <see langword="null" /> for default.
    /// </param>
    /// <param name="fallbackPrecision">
    ///     Specifies a precision for the mapping, in case one isn't found at the core level, or <see langword="null" /> for default.
    /// </param>
    /// <param name="fallbackScale">
    ///     Specifies a scale for the mapping, in case one isn't found at the core level, or <see langword="null" /> for default.
    /// </param>
    public RelationalTypeMappingInfo(
        IReadOnlyList<IProperty> principals,
        string? storeTypeName = null,
        string? storeTypeNameBase = null,
        bool? fallbackUnicode = null,
        bool? fallbackFixedLength = null,
        int? fallbackSize = null,
        int? fallbackPrecision = null,
        int? fallbackScale = null)
    {
        _coreTypeMappingInfo = new TypeMappingInfo(principals, fallbackUnicode, fallbackSize, fallbackPrecision, fallbackScale);

        ValueConverter? customConverter = null;
        for (var i = 0; i < principals.Count; i++)
        {
            var principal = principals[i];
            if (customConverter == null)
            {
                var converter = principal.GetValueConverter();
                if (converter != null)
                {
                    customConverter = converter;
                }
            }

            if (fallbackFixedLength == null)
            {
                var fixedLength = principal.IsFixedLength();
                if (fixedLength != null)
                {
                    fallbackFixedLength = fixedLength;
                }
            }
        }

        var mappingHints = customConverter?.MappingHints;

        IsFixedLength = fallbackFixedLength ?? (mappingHints as RelationalConverterMappingHints)?.IsFixedLength;
        DbType = (mappingHints as RelationalConverterMappingHints)?.DbType;
        StoreTypeName = storeTypeName;
        StoreTypeNameBase = storeTypeNameBase;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" />.
    /// </summary>
    /// <param name="storeTypeName">The provider-specific relational type name for which mapping is needed.</param>
    /// <param name="storeTypeNameBase">The provider-specific relational type name, with any facets removed.</param>
    /// <param name="unicode">Specifies Unicode or ANSI mapping, or <see langword="null" /> for default.</param>
    /// <param name="size">Specifies a size for the mapping, or <see langword="null" /> for default.</param>
    /// <param name="precision">Specifies a precision for the mapping, or <see langword="null" /> for default.</param>
    /// <param name="scale">Specifies a scale for the mapping, or <see langword="null" /> for default.</param>
    public RelationalTypeMappingInfo(
        string storeTypeName,
        string storeTypeNameBase,
        bool? unicode,
        int? size,
        int? precision,
        int? scale)
    {
        // Note: Empty string is allowed for store type name because SQLite
        _coreTypeMappingInfo = new TypeMappingInfo(null, null, false, unicode, size, null, precision, scale, false);
        StoreTypeName = storeTypeName;
        StoreTypeNameBase = storeTypeNameBase;
        IsFixedLength = null;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" />.
    /// </summary>
    /// <param name="member">The property or field for which mapping is needed.</param>
    /// <param name="elementTypeMapping">The type mapping for elements, if known.</param>
    /// <param name="storeTypeName">The provider-specific relational type name for which mapping is needed.</param>
    /// <param name="storeTypeNameBase">The provider-specific relational type name, with any facets removed.</param>
    /// <param name="unicode">Specifies Unicode or ANSI mapping, or <see langword="null" /> for default.</param>
    /// <param name="size">Specifies a size for the mapping, or <see langword="null" /> for default.</param>
    /// <param name="precision">Specifies a precision for the mapping, or <see langword="null" /> for default.</param>
    /// <param name="scale">Specifies a scale for the mapping, or <see langword="null" /> for default.</param>
    public RelationalTypeMappingInfo(
        MemberInfo member,
        RelationalTypeMapping? elementTypeMapping = null,
        string? storeTypeName = null,
        string? storeTypeNameBase = null,
        bool? unicode = null,
        int? size = null,
        int? precision = null,
        int? scale = null)
    {
        _coreTypeMappingInfo = new TypeMappingInfo(member, elementTypeMapping, unicode, size, precision, scale);

        StoreTypeName = storeTypeName;
        StoreTypeNameBase = storeTypeNameBase;
        IsFixedLength = null;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" /> with the given <see cref="ValueConverterInfo" />.
    /// </summary>
    /// <param name="source">The source info.</param>
    /// <param name="converter">The converter to apply.</param>
    public RelationalTypeMappingInfo(
        in RelationalTypeMappingInfo source,
        in ValueConverterInfo converter)
    {
        _coreTypeMappingInfo = new TypeMappingInfo(
            source._coreTypeMappingInfo,
            converter,
            source.IsUnicode,
            source.Size,
            source.Precision,
            source.Scale);

        var mappingHints = converter.MappingHints;

        StoreTypeName = source.StoreTypeName;
        StoreTypeNameBase = source.StoreTypeNameBase;
        IsFixedLength = source.IsFixedLength ?? (mappingHints as RelationalConverterMappingHints)?.IsFixedLength;
        DbType = source.DbType ?? (mappingHints as RelationalConverterMappingHints)?.DbType;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="TypeMappingInfo" />.
    /// </summary>
    /// <param name="type">The CLR type in the model for which mapping is needed.</param>
    /// <param name="elementTypeMapping">The type mapping for elements, if known.</param>
    /// <param name="storeTypeName">The database type name.</param>
    /// <param name="storeTypeNameBase">The provider-specific relational type name, with any facets removed.</param>
    /// <param name="keyOrIndex">If <see langword="true" />, then a special mapping for a key or index may be returned.</param>
    /// <param name="unicode">Specifies Unicode or ANSI mapping, or <see langword="null" /> for default.</param>
    /// <param name="size">Specifies a size for the mapping, or <see langword="null" /> for default.</param>
    /// <param name="rowVersion">Specifies a row-version, or <see langword="null" /> for default.</param>
    /// <param name="fixedLength">Specifies a fixed length mapping, or <see langword="null" /> for default.</param>
    /// <param name="precision">Specifies a precision for the mapping, or <see langword="null" /> for default.</param>
    /// <param name="scale">Specifies a scale for the mapping, or <see langword="null" /> for default.</param>
    /// <param name="dbType">The suggested <see cref="DbType" />, or <see langword="null" /> for default.</param>
    /// <param name="key">If <see langword="true" />, then a special mapping for a key may be returned.</param>
    public RelationalTypeMappingInfo(
        Type? type = null,
        RelationalTypeMapping? elementTypeMapping = null,
        string? storeTypeName = null,
        string? storeTypeNameBase = null,
        bool keyOrIndex = false,
        bool? unicode = null,
        int? size = null,
        bool? rowVersion = null,
        bool? fixedLength = null,
        int? precision = null,
        int? scale = null,
        DbType? dbType = null,
        bool key = false)
    {
        _coreTypeMappingInfo = new TypeMappingInfo(type, elementTypeMapping, keyOrIndex, unicode, size, rowVersion, precision, scale, key);

        IsFixedLength = fixedLength;
        StoreTypeName = storeTypeName;
        StoreTypeNameBase = storeTypeNameBase;
        DbType = dbType;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="TypeMappingInfo" />.
    /// </summary>
    /// <param name="type">The CLR type in the model for which mapping is needed.</param>
    /// <param name="typeMappingConfiguration">The type mapping configuration.</param>
    /// <param name="elementTypeMapping">The type mapping for elements, if known.</param>
    /// <param name="storeTypeName">The database type name.</param>
    /// <param name="storeTypeNameBase">The provider-specific relational type name, with any facets removed.</param>
    /// <param name="unicode">Specifies Unicode or ANSI mapping, or <see langword="null" /> for default.</param>
    /// <param name="size">Specifies a size for the mapping, or <see langword="null" /> for default.</param>
    /// <param name="precision">Specifies a precision for the mapping, or <see langword="null" /> for default.</param>
    /// <param name="scale">Specifies a scale for the mapping, or <see langword="null" /> for default.</param>
    public RelationalTypeMappingInfo(
        Type type,
        ITypeMappingConfiguration typeMappingConfiguration,
        RelationalTypeMapping? elementTypeMapping = null,
        string? storeTypeName = null,
        string? storeTypeNameBase = null,
        bool? unicode = null,
        int? size = null,
        int? precision = null,
        int? scale = null)
    {
        _coreTypeMappingInfo = new TypeMappingInfo(
            typeMappingConfiguration.GetValueConverter()?.ProviderClrType ?? type,
            elementTypeMapping,
            keyOrIndex: false,
            unicode ?? typeMappingConfiguration.IsUnicode(),
            size ?? typeMappingConfiguration.GetMaxLength(),
            rowVersion: false,
            precision ?? typeMappingConfiguration.GetPrecision(),
            scale ?? typeMappingConfiguration.GetScale(),
            key: false);

        IsFixedLength = (bool?)typeMappingConfiguration[RelationalAnnotationNames.IsFixedLength];
        StoreTypeName = storeTypeName;
        StoreTypeNameBase = storeTypeNameBase;
    }

    /// <summary>
    ///     The core type mapping info.
    /// </summary>
    public TypeMappingInfo CoreTypeMappingInfo
        => _coreTypeMappingInfo;

    /// <summary>
    ///     The provider-specific relational type name for which mapping is needed.
    /// </summary>
    public string? StoreTypeName { get; init; }

    /// <summary>
    ///     The provider-specific relational type name, with any facets removed.
    /// </summary>
    public string? StoreTypeNameBase { get; init; }

    /// <summary>
    ///     Indicates the store-size to use for the mapping, or <see langword="null" /> if none.
    /// </summary>
    public int? Size
    {
        get => _coreTypeMappingInfo.Size;
        init => _coreTypeMappingInfo = _coreTypeMappingInfo with { Size = value };
    }

    /// <summary>
    ///     The suggested precision of the mapped data type.
    /// </summary>
    public int? Precision
    {
        get => _coreTypeMappingInfo.Precision;
        init => _coreTypeMappingInfo = _coreTypeMappingInfo with { Precision = value };
    }

    /// <summary>
    ///     The suggested scale of the mapped data type.
    /// </summary>
    public int? Scale
    {
        get => _coreTypeMappingInfo.Scale;
        init => _coreTypeMappingInfo = _coreTypeMappingInfo with { Scale = value };
    }

    /// <summary>
    ///     Whether or not the mapped data type is fixed length.
    /// </summary>
    public bool? IsFixedLength { get; init; }

    /// <summary>
    ///     The <see cref="DbType" /> of the mapping.
    /// </summary>
    public DbType? DbType { get; init; }

    /// <summary>
    ///     Indicates whether or not the mapping is part of a key or foreign key.
    /// </summary>
    public bool IsKey
    {
        get => _coreTypeMappingInfo.IsKey;
        init => _coreTypeMappingInfo = _coreTypeMappingInfo with { IsKey = value };
    }

    /// <summary>
    ///     Indicates whether or not the mapping is part of a key, foreign key, or index.
    /// </summary>
    public bool IsKeyOrIndex
    {
        get => _coreTypeMappingInfo.IsKeyOrIndex;
        init => _coreTypeMappingInfo = _coreTypeMappingInfo with { IsKeyOrIndex = value };
    }

    /// <summary>
    ///     Indicates whether or not the mapping supports Unicode, or <see langword="null" /> if not defined.
    /// </summary>
    public bool? IsUnicode
    {
        get => _coreTypeMappingInfo.IsUnicode;
        init => _coreTypeMappingInfo = _coreTypeMappingInfo with { IsUnicode = value };
    }

    /// <summary>
    ///     Indicates whether or not the mapping will be used for a row version, or <see langword="null" /> if not defined.
    /// </summary>
    public bool? IsRowVersion
    {
        get => _coreTypeMappingInfo.IsRowVersion;
        init => _coreTypeMappingInfo = _coreTypeMappingInfo with { IsRowVersion = value };
    }

    /// <summary>
    ///     The CLR type in the model.
    /// </summary>
    public Type? ClrType
    {
        get => _coreTypeMappingInfo.ClrType;
        init => _coreTypeMappingInfo = _coreTypeMappingInfo with { ClrType = value };
    }

    /// <summary>
    ///     The JSON reader/writer, if one has been provided, or <see langword="null" /> otherwise.
    /// </summary>
    public JsonValueReaderWriter? JsonValueReaderWriter
    {
        get => _coreTypeMappingInfo.JsonValueReaderWriter;
        init => _coreTypeMappingInfo = _coreTypeMappingInfo with { JsonValueReaderWriter = value };
    }

    /// <summary>
    ///     The element type of the mapping, if any.
    /// </summary>
    public RelationalTypeMapping? ElementTypeMapping
    {
        get => (RelationalTypeMapping?)_coreTypeMappingInfo.ElementTypeMapping;
        init => _coreTypeMappingInfo = _coreTypeMappingInfo with { ElementTypeMapping = value };
    }

    /// <summary>
    ///     Returns a new <see cref="RelationalTypeMappingInfo" /> with the given converter applied.
    /// </summary>
    /// <param name="converterInfo">The converter to apply.</param>
    /// <returns>The new mapping info.</returns>
    public RelationalTypeMappingInfo WithConverter(in ValueConverterInfo converterInfo)
        => new(this, converterInfo);
}
