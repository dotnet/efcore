// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestRelationalTypeMappingSource : RelationalTypeMappingSource
    {
        private static readonly RelationalTypeMapping _string
            = new StringTypeMapping("just_string(2000)");

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

        public TestRelationalTypeMappingSource(
            TypeMappingSourceDependencies dependencies,
            RelationalTypeMappingSourceDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        private class TestStringTypeMapping : StringTypeMapping
        {
            public TestStringTypeMapping(
                string storeType,
                DbType? dbType,
                bool unicode = false,
                int? size = null,
                bool fixedLength = false)
                : base(
                    new RelationalTypeMappingParameters(
                        new CoreTypeMappingParameters(typeof(string)),
                        storeType,
                        StoreTypePostfix.None,
                        dbType,
                        unicode,
                        size,
                        fixedLength))
            {
            }

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
                    var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? (int?)(isAnsi ? 900 : 450) : null);

                    return new TestStringTypeMapping(
                        storeTypeName ?? baseName + "(" + (size == null ? "max" : size.ToString()) + ")",
                        isAnsi ? DbType.AnsiString : (DbType?)null,
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

                    var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? (int?)900 : null);

                    return new ByteArrayTypeMapping(
                        storeTypeName ?? "just_binary(" + (size == null ? "max" : size.ToString()) + ")",
                        DbType.Binary,
                        size);
                }

                if (_simpleMappings.TryGetValue(clrType, out var mapping))
                {
                    return storeTypeName != null
                           && !mapping.StoreType.Equals(storeTypeName, StringComparison.Ordinal)
                        ? mapping.Clone(storeTypeName, mapping.Size)
                        : mapping;
                }
            }

            return storeTypeName != null
                   && _simpleNameMappings.TryGetValue(storeTypeName, out var mappingFromName)
                   && (clrType == null || mappingFromName.ClrType == clrType)
                ? mappingFromName
                : null;
        }
    }
}
