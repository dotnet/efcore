// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestRelationalTypeMappingSource(
    TypeMappingSourceDependencies dependencies,
    RelationalTypeMappingSourceDependencies relationalDependencies) : RelationalTypeMappingSource(dependencies, relationalDependencies)
{
    private static readonly RelationalTypeMapping _string
        = new StringTypeMapping("just_string(2000)", DbType.String);

    private static readonly RelationalTypeMapping _binary
        = new ByteArrayTypeMapping("just_binary(max)", dbType: DbType.Binary);

    private static readonly RelationalTypeMapping _rowversion
        = new ByteArrayTypeMapping("rowversion", dbType: DbType.Binary, size: 8);

    private static readonly RelationalTypeMapping _defaultIntMapping
        = new IntTypeMapping("default_int_mapping", dbType: DbType.Int32);

    private static readonly RelationalTypeMapping _defaultCharMapping
        = new CharTypeMapping("default_char_mapping", dbType: DbType.Int32);

    private static readonly RelationalTypeMapping _defaultLongMapping
        = new LongTypeMapping("default_long_mapping", dbType: DbType.Int64);

    private static readonly RelationalTypeMapping _defaultShortMapping
        = new ShortTypeMapping("default_short_mapping", dbType: DbType.Int16);

    private static readonly RelationalTypeMapping _defaultByteMapping
        = new ByteTypeMapping("default_byte_mapping", dbType: DbType.Byte);

    private static readonly RelationalTypeMapping _defaultBoolMapping
        = new BoolTypeMapping("default_bool_mapping");

    private static readonly RelationalTypeMapping _someIntMapping
        = new IntTypeMapping("some_int_mapping");

    private class IntArrayTypeMapping : RelationalTypeMapping
    {
        public IntArrayTypeMapping()
            : base(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(
                        typeof(int[]),
                        null,
                        new ValueComparer<int[]>(
                            (v1, v2) => v1.SequenceEqual(v2),
                            v => v.Aggregate(0, (t, e) => (t * 397) ^ e),
                            v => v.ToArray())),
                    "some_int_array_mapping"))
        {
        }

        private IntArrayTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new IntArrayTypeMapping(parameters);
    }

    private static readonly RelationalTypeMapping _intArray
        = new IntArrayTypeMapping();

    private static readonly RelationalTypeMapping _defaultDecimalMapping
        = new DecimalTypeMapping("default_decimal_mapping");

    private static readonly RelationalTypeMapping _defaultDateTimeMapping
        = new DateTimeTypeMapping("default_datetime_mapping", dbType: DbType.DateTime2);

    private static readonly RelationalTypeMapping _defaultDoubleMapping
        = new DoubleTypeMapping("default_double_mapping");

    private static readonly RelationalTypeMapping _defaultDateTimeOffsetMapping
        = new DateTimeOffsetTypeMapping("default_datetimeoffset_mapping");

    private static readonly RelationalTypeMapping _defaultFloatMapping
        = new FloatTypeMapping("default_float_mapping");

    private static readonly RelationalTypeMapping _defaultGuidMapping
        = new GuidTypeMapping("default_guid_mapping");

    private static readonly RelationalTypeMapping _defaultTimeSpanMapping
        = new TimeSpanTypeMapping("default_timespan_mapping");

    private readonly IReadOnlyDictionary<Type, RelationalTypeMapping> _simpleMappings
        = new Dictionary<Type, RelationalTypeMapping>
        {
            { typeof(int), _defaultIntMapping },
            { typeof(long), _defaultLongMapping },
            { typeof(DateTime), _defaultDateTimeMapping },
            { typeof(Guid), _defaultGuidMapping },
            { typeof(bool), _defaultBoolMapping },
            { typeof(byte), _defaultByteMapping },
            { typeof(double), _defaultDoubleMapping },
            { typeof(DateTimeOffset), _defaultDateTimeOffsetMapping },
            { typeof(char), _defaultCharMapping },
            { typeof(short), _defaultShortMapping },
            { typeof(float), _defaultFloatMapping },
            { typeof(decimal), _defaultDecimalMapping },
            { typeof(TimeSpan), _defaultTimeSpanMapping },
            { typeof(string), _string },
            { typeof(int[]), _intArray }
        };

    private readonly IReadOnlyDictionary<string, RelationalTypeMapping> _simpleNameMappings
        = new Dictionary<string, RelationalTypeMapping>
        {
            { "some_int_mapping", _someIntMapping },
            { "some_string(max)", _string },
            { "some_binary(max)", _binary },
            { "money", _defaultDecimalMapping },
            { "dec", _defaultDecimalMapping }
        };

    private class TestStringTypeMapping(
        string storeType,
        DbType? dbType,
        bool unicode = false,
        int? size = null,
        bool fixedLength = false) : StringTypeMapping(
            new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(string)),
                    storeType,
                    StoreTypePostfix.None,
                    dbType,
                    unicode,
                    size,
                    fixedLength))
    {
        protected override string ProcessStoreType(
            RelationalTypeMappingParameters parameters,
            string storeType,
            string storeTypeNameBase)
            => storeTypeNameBase == "some_string"
                && parameters.Size != null
                    ? $"({parameters.Size})some_string"
                    : storeType;
    }

    protected override RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType;
        var storeTypeName = mappingInfo.StoreTypeName;

        if (clrType != null)
        {
            if (clrType == typeof(string))
            {
                var isAnsi = mappingInfo.IsUnicode == false;
                var isFixedLength = mappingInfo.IsFixedLength == true;
                var baseName = (isAnsi ? "ansi_" : "just_") + (isFixedLength ? "string_fixed" : "string");
                var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? isAnsi ? 900 : 450 : null);

                return new TestStringTypeMapping(
                    storeTypeName ?? baseName + "(" + (size == null ? "max" : size.ToString()) + ")",
                    isAnsi ? DbType.AnsiString : null,
                    !isAnsi,
                    size,
                    isFixedLength);
            }

            if (clrType == typeof(byte[]))
            {
                if (mappingInfo.IsRowVersion == true)
                {
                    return _rowversion;
                }

                var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? 900 : null);
                var isFixedLength = mappingInfo.IsFixedLength == true;

                return new ByteArrayTypeMapping(
                    storeTypeName
                    ?? (isFixedLength ? "just_binary_fixed(" : "just_binary(") + (size == null ? "max" : size.ToString()) + ")",
                    DbType.Binary,
                    size);
            }

            if (clrType == typeof(decimal)
                && !string.Equals("money", storeTypeName, StringComparison.Ordinal))
            {
                var precision = mappingInfo.Precision;
                var scale = mappingInfo.Scale;
                if (precision == _defaultDecimalMapping.Precision
                    && scale == _defaultDecimalMapping.Scale)
                {
                    return _defaultDecimalMapping;
                }

                if (scale is null or 0)
                {
                    return new DecimalTypeMapping(
                        "decimal_mapping(" + precision + ")",
                        precision: precision);
                }

                return new DecimalTypeMapping(
                    "decimal_mapping(" + precision + "," + scale + ")",
                    precision: precision,
                    scale: scale);
            }

            if (_simpleMappings.TryGetValue(clrType, out var mapping))
            {
                return storeTypeName != null
                    && !mapping.StoreType.Equals(storeTypeName, StringComparison.Ordinal)
                        ? mapping.WithStoreTypeAndSize(storeTypeName, mapping.Size)
                        : mapping;
            }
        }

        return storeTypeName != null
            && _simpleNameMappings.TryGetValue(storeTypeName, out var mappingFromName)
            && (clrType == null || mappingFromName.ClrType == clrType)
                ? mappingFromName
                : null;
    }

    protected override string ParseStoreTypeName(
        string storeTypeName,
        ref bool? unicode,
        ref int? size,
        ref int? precision,
        ref int? scale)
    {
        var parsedName = base.ParseStoreTypeName(storeTypeName, ref unicode, ref size, ref precision, ref scale);

        if (size.HasValue
            && storeTypeName?.StartsWith("default_decimal_mapping", StringComparison.OrdinalIgnoreCase) == true)
        {
            precision = size;
            size = null;
            scale = 0;
        }

        if (storeTypeName?.StartsWith("ansi_string", StringComparison.OrdinalIgnoreCase) == true)
        {
            unicode = false;
        }

        return parsedName;
    }
}
