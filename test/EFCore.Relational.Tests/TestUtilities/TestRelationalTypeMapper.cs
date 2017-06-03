// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestRelationalTypeMapper : RelationalTypeMapper
    {
        private static readonly RelationalTypeMapping _string = new StringTypeMapping("just_string(2000)");
        private static readonly RelationalTypeMapping _unboundedString = new StringTypeMapping("just_string(max)");
        private static readonly RelationalTypeMapping _stringKey = new StringTypeMapping("just_string(450)", dbType: null, unicode: true, size: 450);
        private static readonly RelationalTypeMapping _ansiStringKey = new StringTypeMapping("ansi_string(900)", dbType: null, unicode: true, size: 450);
        private static readonly RelationalTypeMapping _unboundedBinary = new ByteArrayTypeMapping("just_binary(max)", dbType: DbType.Binary);
        private static readonly RelationalTypeMapping _binary = new ByteArrayTypeMapping("just_binary(max)", dbType: DbType.Binary);
        private static readonly RelationalTypeMapping _binaryKey = new ByteArrayTypeMapping("just_binary(900)", dbType: DbType.Binary, size: 900);
        private static readonly RelationalTypeMapping _rowversion = new ByteArrayTypeMapping("rowversion", dbType: DbType.Binary, size: 8);

        private static readonly RelationalTypeMapping _defaultIntMapping
            = new IntTypeMapping("default_int_mapping", dbType: DbType.Int32);

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

        public TestRelationalTypeMapper(RelationalTypeMapperDependencies dependencies)
            : base(dependencies)
        {
        }

        protected override string GetColumnType(IProperty property) => property.TestProvider().ColumnType;

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
                { typeof(char), _defaultIntMapping },
                { typeof(short), _defaultShortMapping },
                { typeof(float), _defaultFloatMapping },
                { typeof(decimal), _defaultDecimalMapping },
                { typeof(TimeSpan), _defaultTimeSpanMapping },
                { typeof(string), _string }
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

        public override IByteArrayRelationalTypeMapper ByteArrayMapper { get; }
            = new ByteArrayRelationalTypeMapper(
                2000,
                _binary,
                _unboundedBinary,
                _binaryKey,
                _rowversion, size => new ByteArrayTypeMapping(
                    "just_binary(" + size + ")",
                    DbType.Binary,
                    size: size));

        public override IStringRelationalTypeMapper StringMapper { get; }
            = new StringRelationalTypeMapper(
                2000,
                _string,
                _unboundedString,
                _ansiStringKey,
                size => new StringTypeMapping(
                    "just_string(" + size + ")",
                    dbType: DbType.AnsiString,
                    unicode: false,
                    size: size),
                2000,
                _string,
                _unboundedString,
                _stringKey,
                size => new StringTypeMapping(
                    "just_string(" + size + ")",
                    dbType: null,
                    unicode: true,
                    size: size));

        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings()
            => _simpleMappings;

        protected override IReadOnlyDictionary<string, RelationalTypeMapping> GetStoreTypeMappings()
            => _simpleNameMappings;

        public override RelationalTypeMapping FindMapping(Type clrType)
        {
            clrType = clrType.UnwrapNullableType().UnwrapEnumType();

            return clrType == typeof(string)
                ? _string
                : (clrType == typeof(byte[])
                    ? _binary
                    : base.FindMapping(clrType));
        }
    }
}
