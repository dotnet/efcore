// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class SqliteTypeMapper : RelationalTypeMapper
    {
        private static readonly RelationalTypeMapping _integer = new RelationalTypeMapping("INTEGER", typeof(long));
        private static readonly RelationalTypeMapping _real = new RelationalTypeMapping("REAL", typeof(double));
        private static readonly RelationalTypeMapping _blob = new RelationalTypeMapping("BLOB", typeof(byte[]));
        private static readonly RelationalTypeMapping _text = new RelationalTypeMapping("TEXT", typeof(string));

        private readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings;
        private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;

        public SqliteTypeMapper()
        {
            _storeTypeMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase);

            _clrTypeMappings
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

        /// <summary>
        ///     Returns a clr type for a SQLite column type. Defaults to typeof(string).
        ///     It uses the same heuristics from
        ///     <see href="https://www.sqlite.org/datatype3.html">"2.1 Determination of Column Affinity"</see>
        /// </summary>
        public override RelationalTypeMapping FindMapping(string storeType)
        {
            Check.NotNull(storeType, nameof(storeType));

            if (storeType.Length == 0)
            {
                // This may seem odd, but it's okay because we are matching SQLite's loose typing.
                return _text;
            }

            foreach (var rules in _typeRules)
            {
                var mapping = rules(storeType);
                if (mapping != null)
                {
                    return mapping;
                }
            }

            return _text;
        }

        private readonly Func<string, RelationalTypeMapping>[] _typeRules =
        {
            name => Contains(name, "INT") ? _integer : null,
            name => Contains(name, "CHAR")
                    || Contains(name, "CLOB")
                    || Contains(name, "TEXT") ? _text : null,
            name => Contains(name, "BLOB") ? _blob : null,
            name => Contains(name, "REAL")
                    || Contains(name, "FLOA")
                    || Contains(name, "DOUB") ? _real : null
        };

        private static bool Contains(string haystack, string needle)
            => haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;

        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings()
            => _clrTypeMappings;

        protected override IReadOnlyDictionary<string, RelationalTypeMapping> GetStoreTypeMappings()
            => _storeTypeMappings;
    }
}
