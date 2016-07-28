// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Tests
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
        private static readonly RelationalTypeMapping _defaultIntMapping = new RelationalTypeMapping("default_int_mapping", typeof(int));
        private static readonly RelationalTypeMapping _defaultBoolMapping = new RelationalTypeMapping("default_bool_mapping", typeof(bool));
        private static readonly RelationalTypeMapping _someIntMapping = new RelationalTypeMapping("some_int_mapping", typeof(int));
        private static readonly RelationalTypeMapping _decimal = new RelationalTypeMapping("decimal(18, 2)", typeof(decimal));

        protected override string GetColumnType(IProperty property) => property.TestProvider().ColumnType;

        private readonly IReadOnlyDictionary<Type, RelationalTypeMapping> _simpleMappings
            = new Dictionary<Type, RelationalTypeMapping>
            {
                { typeof(int), _defaultIntMapping },
                { typeof(bool), _defaultBoolMapping },
                { typeof(string), _string }
            };

        private readonly IReadOnlyDictionary<string, RelationalTypeMapping> _simpleNameMappings
            = new Dictionary<string, RelationalTypeMapping>
            {
                { "some_int_mapping", _someIntMapping },
                { "some_string(max)", _string },
                { "some_binary(max)", _binary },
                { "money", _decimal },
                { "dec", _decimal }
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
