// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider
{
    public class FakeRelationalTypeMapper : RelationalTypeMapper
    {
        private static readonly RelationalTypeMapping _int = new IntTypeMapping("int", DbType.Int32);
        private static readonly RelationalTypeMapping _long = new LongTypeMapping("DefaultLong", DbType.Int64);
        private static readonly RelationalTypeMapping _string = new StringTypeMapping("DefaultString", DbType.String);

        protected override string GetColumnType(IProperty property) => property.TestProvider().ColumnType;

        public FakeRelationalTypeMapper(RelationalTypeMapperDependencies dependencies)
            : base(dependencies)
        {
        }

        private readonly IReadOnlyDictionary<Type, RelationalTypeMapping> _simpleMappings
            = new Dictionary<Type, RelationalTypeMapping>
            {
                { typeof(int), _int },
                { typeof(long), _long },
                { typeof(string), _string }
            };

        private readonly IReadOnlyDictionary<string, RelationalTypeMapping> _simpleNameMappings
            = new Dictionary<string, RelationalTypeMapping>
            {
                { "DefaultInt", _int },
                { "DefaultLong", _long },
                { "DefaultString", _string }
            };

        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings()
            => _simpleMappings;

        protected override IReadOnlyDictionary<string, RelationalTypeMapping> GetStoreTypeMappings()
            => _simpleNameMappings;
    }
}
