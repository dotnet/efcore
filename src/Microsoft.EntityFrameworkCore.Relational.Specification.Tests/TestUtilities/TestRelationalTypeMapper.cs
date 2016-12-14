// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities
{
    public class TestRelationalTypeMapper : RelationalTypeMapper
    {
        private static readonly RelationalTypeMapping _string = new RelationalTypeMapping("just_string(2000)", typeof(string));
        private static readonly RelationalTypeMapping _unboundedString = new RelationalTypeMapping("just_string(max)", typeof(string));
        private static readonly RelationalTypeMapping _stringKey = new RelationalTypeMapping("just_string(450)", typeof(string), dbType: null, unicode: true, size: 450);
        private static readonly RelationalTypeMapping _ansiStringKey = new RelationalTypeMapping("ansi_string(900)", typeof(string), dbType: null, unicode: true, size: 450);
        private static readonly RelationalTypeMapping _unboundedBinary = new RelationalTypeMapping("just_binary(max)", typeof(byte[]), DbType.Binary);
        private static readonly RelationalTypeMapping _binary = new RelationalTypeMapping("just_binary(max)", typeof(byte[]), DbType.Binary);
        private static readonly RelationalTypeMapping _binaryKey = new RelationalTypeMapping("just_binary(900)", typeof(byte[]), DbType.Binary, unicode: true, size: 900);
        private static readonly RelationalTypeMapping _rowversion = new RelationalTypeMapping("rowversion", typeof(byte[]), DbType.Binary, unicode: true, size: 8);
        private static readonly RelationalTypeMapping _defaultIntMapping
            = new RelationalTypeMapping("default_int_mapping", typeof(int), dbType: DbType.Int32);
        private static readonly RelationalTypeMapping _defaultLongMapping
            = new RelationalTypeMapping("default_long_mapping", typeof(long), dbType: DbType.Int64);
        private static readonly RelationalTypeMapping _defaultShortMapping
            = new RelationalTypeMapping("default_short_mapping", typeof(short), dbType: DbType.Int16);
        private static readonly RelationalTypeMapping _defaultByteMapping
            = new RelationalTypeMapping("default_byte_mapping", typeof(byte), dbType: DbType.Byte);
        private static readonly RelationalTypeMapping _defaultBoolMapping = new RelationalTypeMapping("default_bool_mapping", typeof(bool));
        private static readonly RelationalTypeMapping _someIntMapping = new RelationalTypeMapping("some_int_mapping", typeof(int));
        private static readonly RelationalTypeMapping _defaultDecimalMapping =
            new RelationalTypeMapping("default_decimal_mapping", typeof(decimal));
        private static readonly RelationalTypeMapping _defaultDateTimeMapping
            = new RelationalTypeMapping("default_datetime_mapping", typeof(DateTime), dbType: DbType.DateTime2);
        private static readonly RelationalTypeMapping _defaultDoubleMapping
            = new RelationalTypeMapping("default_double_mapping", typeof(double));
        private static readonly RelationalTypeMapping _defaultDateTimeOffsetMapping
            = new RelationalTypeMapping("default_datetimeoffset_mapping", typeof(DateTimeOffset));
        private static readonly RelationalTypeMapping _defaultFloatMapping
            = new RelationalTypeMapping("default_float_mapping", typeof(float));
        private static readonly RelationalTypeMapping _defaultGuidMapping
            = new RelationalTypeMapping("default_guid_mapping", typeof(Guid));
        private static readonly RelationalTypeMapping _defaultTimeSpanMapping
            = new RelationalTypeMapping("default_timespan_mapping", typeof(TimeSpan));

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
                _rowversion, size => new RelationalTypeMapping(
                    "just_binary(" + size + ")",
                    typeof(byte[]),
                    DbType.Binary,
                    unicode: false,
                    size: size,
                    hasNonDefaultUnicode: false,
                    hasNonDefaultSize: true));

        public override IStringRelationalTypeMapper StringMapper { get; }
            = new StringRelationalTypeMapper(
                2000,
                _string,
                _unboundedString,
                _ansiStringKey,
                size => new RelationalTypeMapping(
                    "just_string(" + size + ")",
                    typeof(string),
                    dbType: DbType.AnsiString,
                    unicode: false,
                    size: size,
                    hasNonDefaultUnicode: true,
                    hasNonDefaultSize: true),
                2000,
                _string,
                _unboundedString,
                _stringKey,
                size => new RelationalTypeMapping(
                    "just_string(" + size + ")",
                    typeof(string),
                    dbType: null,
                    unicode: true,
                    size: size,
                    hasNonDefaultUnicode: false,
                    hasNonDefaultSize: true));

        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings()
            => _simpleMappings;

        protected override IReadOnlyDictionary<string, RelationalTypeMapping> GetStoreTypeMappings()
            => _simpleNameMappings;
    }
}
