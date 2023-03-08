// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     Describes metadata needed to decide on a relational type mapping for
///     a property, type, or provider-specific relational type name.
///     just some random stuff to trigger CI run
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
        _coreTypeMappingInfo = new TypeMappingInfo(null, false, unicode, size, null, precision, scale);
        StoreTypeName = storeTypeName;
        StoreTypeNameBase = storeTypeNameBase;
        IsFixedLength = null;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="RelationalTypeMappingInfo" />.
    /// </summary>
    /// <param name="member">The property or field for which mapping is needed.</param>
    /// <param name="storeTypeName">The provider-specific relational type name for which mapping is needed.</param>
    /// <param name="storeTypeNameBase">The provider-specific relational type name, with any facets removed.</param>
    /// <param name="unicode">Specifies Unicode or ANSI mapping, or <see langword="null" /> for default.</param>
    /// <param name="size">Specifies a size for the mapping, or <see langword="null" /> for default.</param>
    /// <param name="precision">Specifies a precision for the mapping, or <see langword="null" /> for default.</param>
    /// <param name="scale">Specifies a scale for the mapping, or <see langword="null" /> for default.</param>
    public RelationalTypeMappingInfo(
        MemberInfo member,
        string? storeTypeName = null,
        string? storeTypeNameBase = null,
        bool? unicode = null,
        int? size = null,
        int? precision = null,
        int? scale = null)
    {
        _coreTypeMappingInfo = new TypeMappingInfo(member, unicode, size, precision, scale);

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
    /// <param name="storeTypeName">The database type name.</param>
    /// <param name="storeTypeNameBase">The provider-specific relational type name, with any facets removed.</param>
    /// <param name="keyOrIndex">If <see langword="true" />, then a special mapping for a key or index may be returned.</param>
    /// <param name="unicode">Specifies Unicode or ANSI mapping, or <see langword="null" /> for default.</param>
    /// <param name="size">Specifies a size for the mapping, or <see langword="null" /> for default.</param>
    /// <param name="rowVersion">Specifies a row-version, or <see langword="null" /> for default.</param>
    /// <param name="fixedLength">Specifies a fixed length mapping, or <see langword="null" /> for default.</param>
    /// <param name="precision">Specifies a precision for the mapping, or <see langword="null" /> for default.</param>
    /// <param name="scale">Specifies a scale for the mapping, or <see langword="null" /> for default.</param>
    public RelationalTypeMappingInfo(
        Type type,
        string? storeTypeName = null,
        string? storeTypeNameBase = null,
        bool keyOrIndex = false,
        bool? unicode = null,
        int? size = null,
        bool? rowVersion = null,
        bool? fixedLength = null,
        int? precision = null,
        int? scale = null)
    {
        _coreTypeMappingInfo = new TypeMappingInfo(type, keyOrIndex, unicode, size, rowVersion, precision, scale);

        IsFixedLength = fixedLength;
        StoreTypeName = storeTypeName;
        StoreTypeNameBase = storeTypeNameBase;
    }

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
    ///     The <see cref="DbType"/> of the mapping.
    /// </summary>
    public DbType? DbType { get; init; }

    /// <summary>
    ///     Indicates whether or not the mapping is part of a key or index.
    /// </summary>
    public bool IsKeyOrIndex
    {
        get => _coreTypeMappingInfo.IsKeyOrIndex;
        init => _coreTypeMappingInfo = _coreTypeMappingInfo with { IsKeyOrIndex = value };
    }

    /// <summary>
    ///     Indicates whether or not the mapping should be compared, etc. as if it is a key.
    /// </summary>
    public bool HasKeySemantics
    {
        get => _coreTypeMappingInfo.HasKeySemantics;
        init => _coreTypeMappingInfo = _coreTypeMappingInfo with { HasKeySemantics = value };
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
    ///     Returns a new <see cref="TypeMappingInfo" /> with the given converter applied.
    /// </summary>
    /// <param name="converterInfo">The converter to apply.</param>
    /// <returns>The new mapping info.</returns>
    public RelationalTypeMappingInfo WithConverter(in ValueConverterInfo converterInfo)
        => new(this, converterInfo);
}
