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
        private static readonly RelationalTypeMapping _int = new RelationalTypeMapping("DefaultInt", typeof(int), DbType.Int32);
        private static readonly RelationalTypeMapping _long = new RelationalTypeMapping("DefaultLong", typeof(long), DbType.Int64);
        private static readonly RelationalTypeMapping _string = new RelationalTypeMapping("DefaultString", typeof(string), DbType.String);

        protected override string GetColumnType(IProperty property) => property.TestProvider().ColumnType;

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
                    { "DefaultString", _string}
                };

        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetSimpleMappings()
            => _simpleMappings;

        protected override IReadOnlyDictionary<string, RelationalTypeMapping> GetSimpleNameMappings()
            => _simpleNameMappings;
    }
}
