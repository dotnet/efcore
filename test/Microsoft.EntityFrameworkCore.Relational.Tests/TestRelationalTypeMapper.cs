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
        private static readonly RelationalTypeMapping _unboundedStrng = new RelationalTypeMapping("just_string(max)", typeof(string));
        private static readonly RelationalTypeMapping _stringKey = new RelationalSizedTypeMapping("just_string(450)", typeof(string), 450);
        private static readonly RelationalTypeMapping _unboundedBinary = new RelationalTypeMapping("just_binary(max)", typeof(byte[]), DbType.Binary);
        private static readonly RelationalTypeMapping _binary = new RelationalTypeMapping("just_binary(max)", typeof(byte[]), DbType.Binary);
        private static readonly RelationalTypeMapping _binaryKey = new RelationalSizedTypeMapping("just_binary(900)", typeof(byte[]), DbType.Binary, 900);
        private static readonly RelationalTypeMapping _rowversion = new RelationalSizedTypeMapping("rowversion", typeof(byte[]), DbType.Binary, 8);
        private static readonly RelationalTypeMapping _defaultIntMapping = new RelationalTypeMapping("default_int_mapping", typeof(int));
        private static readonly RelationalTypeMapping _someIntMapping = new RelationalTypeMapping("some_int_mapping", typeof(int));

        protected override string GetColumnType(IProperty property) => property.TestProvider().ColumnType;

        private readonly IReadOnlyDictionary<Type, RelationalTypeMapping> _simpleMappings
        = new Dictionary<Type, RelationalTypeMapping>
            {
                { typeof(int), _defaultIntMapping },
                { typeof(string), _string }
            };

        private readonly IReadOnlyDictionary<string, RelationalTypeMapping> _simpleNameMappings
        = new Dictionary<string, RelationalTypeMapping>
            {
                { "some_int_mapping", _someIntMapping },
                { "some_string(max)", _string },
                { "some_binary(max)", _binary }
            };

        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetSimpleMappings()
            => _simpleMappings;

        protected override IReadOnlyDictionary<string, RelationalTypeMapping> GetSimpleNameMappings()
            => _simpleNameMappings;

        protected override RelationalTypeMapping FindCustomMapping(IProperty property)
        {
            var clrType = property.ClrType.UnwrapNullableType();

            return clrType == typeof(string)
                ? GetByteArrayMapping(
                    property, 2000,
                    l => new RelationalSizedTypeMapping("just_string(" + l + ")", typeof(string), l),
                    _unboundedStrng, _string, _stringKey)
                : clrType == typeof(byte[])
                    ? GetByteArrayMapping(
                        property, 2000,
                        l => new RelationalSizedTypeMapping("just_binary(" + l + ")", typeof(string), l),
                        _unboundedBinary, _binary, _binaryKey, _rowversion)
                    : base.FindCustomMapping(property);
        }
    }
}
