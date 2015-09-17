// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class SqliteTypeMapper : RelationalTypeMapper
    {
        private readonly RelationalTypeMapping _integer = new RelationalTypeMapping("INTEGER");
        private readonly RelationalTypeMapping _real = new RelationalTypeMapping("REAL");
        private readonly RelationalTypeMapping _blob = new RelationalTypeMapping("BLOB");
        private readonly RelationalTypeMapping _text = new RelationalTypeMapping("TEXT");

        private readonly Dictionary<string, RelationalTypeMapping> _simpleNameMappings;

        private readonly Dictionary<Type, RelationalTypeMapping> _simpleMappings;

        public SqliteTypeMapper()
        {
            _simpleNameMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase);

            _simpleMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
                    { typeof(string), _text },
                    { typeof(byte[]), _blob },
                    { typeof(bool), _integer },
                    { typeof(byte), _integer },
                    { typeof(char), _integer },
                    { typeof(int), _integer },
                    { typeof(long), _integer },
                    { typeof(sbyte), _integer },
                    { typeof(short), _integer },
                    { typeof(uint), _integer },
                    { typeof(ulong), _integer },
                    { typeof(ushort), _integer },
                    { typeof(DateTime), _text },
                    { typeof(DateTimeOffset), _text },
                    { typeof(TimeSpan), _text },
                    { typeof(decimal), _text },
                    { typeof(double), _real },
                    { typeof(float), _real },
                    { typeof(Guid), _blob }
                };
        }

        protected override string GetColumnType(IProperty property) => property.Sqlite().ColumnType;

        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> SimpleMappings
            => _simpleMappings;

        protected override IReadOnlyDictionary<string, RelationalTypeMapping> SimpleNameMappings
            => _simpleNameMappings;
    }
}
