// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Tests;

namespace Microsoft.Data.Entity.TestUtilities.FakeProvider
{
    public class FakeRelationalTypeMapper : RelationalTypeMapper
    {
        private static readonly RelationalTypeMapping _int = new RelationalTypeMapping("DefaultInt", DbType.Int32);
        private static readonly RelationalTypeMapping _long = new RelationalTypeMapping("DefaultLong", DbType.Int64);
        private static readonly RelationalTypeMapping _string = new RelationalTypeMapping("DefaultString", DbType.String);

        protected override string GetColumnType(IProperty property) => property.TestProvider().ColumnType;

        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> SimpleMappings { get; }
            = new Dictionary<Type, RelationalTypeMapping>
                {
                    { typeof(int), _int },
                    { typeof(long), _long },
                    { typeof(string), _string }
                };

        protected override IReadOnlyDictionary<string, RelationalTypeMapping> SimpleNameMappings { get; }
            = new Dictionary<string, RelationalTypeMapping>
            {
                { "DefaultInt", _int },
                { "DefaultLong", _long },
                { "DefaultString", _string}
            };
    }
}
