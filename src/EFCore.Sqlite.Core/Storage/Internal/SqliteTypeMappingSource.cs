// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteTypeMappingSource : RelationalTypeMappingSource
    {
        private static readonly HashSet<string> _spatialiteTypes
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "GEOMETRY",
                "GEOMETRYCOLLECTION",
                "LINESTRING",
                "MULTILINESTRING",
                "MULTIPOINT",
                "MULTIPOLYGON",
                "POINT",
                "POLYGON"
            };

        private const string IntegerTypeName = "INTEGER";
        private const string RealTypeName = "REAL";
        private const string BlobTypeName = "BLOB";
        private const string TextTypeName = "TEXT";

        private static readonly LongTypeMapping _integer = new LongTypeMapping(IntegerTypeName);
        private static readonly DoubleTypeMapping _real = new DoubleTypeMapping(RealTypeName);
        private static readonly ByteArrayTypeMapping _blob = new ByteArrayTypeMapping(BlobTypeName);
        private static readonly StringTypeMapping _text = new StringTypeMapping(TextTypeName);

        private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings
            = new Dictionary<Type, RelationalTypeMapping>
            {
                { typeof(string), _text },
                { typeof(byte[]), _blob },
                { typeof(bool), new BoolTypeMapping(IntegerTypeName) },
                { typeof(byte), new ByteTypeMapping(IntegerTypeName) },
                { typeof(char), new SqliteCharTypeMapping(IntegerTypeName) },
                { typeof(int), new IntTypeMapping(IntegerTypeName) },
                { typeof(long), _integer },
                { typeof(sbyte), new SByteTypeMapping(IntegerTypeName) },
                { typeof(short), new ShortTypeMapping(IntegerTypeName) },
                { typeof(uint), new UIntTypeMapping(IntegerTypeName) },
                { typeof(ulong), new SqliteULongTypeMapping(IntegerTypeName) },
                { typeof(ushort), new UShortTypeMapping(IntegerTypeName) },
                { typeof(DateTime), new SqliteDateTimeTypeMapping(TextTypeName) },
                { typeof(DateTimeOffset), new SqliteDateTimeOffsetTypeMapping(TextTypeName) },
                { typeof(TimeSpan), new TimeSpanTypeMapping(TextTypeName) },
                { typeof(decimal), new SqliteDecimalTypeMapping(TextTypeName) },
                { typeof(double), _real },
                { typeof(float), new FloatTypeMapping(RealTypeName) },
                { typeof(Guid), new SqliteGuidTypeMapping(BlobTypeName) }
            };

        private readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings
            = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
            {
                { IntegerTypeName, _integer },
                { RealTypeName, _real },
                { BlobTypeName, _blob },
                { TextTypeName, _text },
            };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqliteTypeMappingSource(
            [NotNull] TypeMappingSourceDependencies dependencies,
            [NotNull] RelationalTypeMappingSourceDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsSpatialiteType(string columnType)
            => _spatialiteTypes.Contains(columnType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            if (clrType != null
                && _clrTypeMappings.TryGetValue(clrType, out var mapping))
            {
                return mapping;
            }

            var storeTypeName = mappingInfo.StoreTypeName;
            if (storeTypeName != null
                && _storeTypeMappings.TryGetValue(storeTypeName, out mapping))
            {
                return mapping;
            }

            mapping = base.FindMapping(mappingInfo);

            return mapping != null
                ? mapping
                : storeTypeName != null
                    ? storeTypeName.Length != 0
                        ? _typeRules.Select(r => r(storeTypeName)).FirstOrDefault(r => r != null) ?? _text
                        : _text // This may seem odd, but it's okay because we are matching SQLite's loose typing.
                    : null;
        }

        private readonly Func<string, RelationalTypeMapping>[] _typeRules =
        {
            name => Contains(name, "INT") ? _integer : null,
            name => Contains(name, "CHAR")
                    || Contains(name, "CLOB")
                    || Contains(name, "TEXT")
                ? _text
                : null,
            name => Contains(name, "BLOB") ? _blob : null,
            name => Contains(name, "REAL")
                    || Contains(name, "FLOA")
                    || Contains(name, "DOUB")
                ? _real
                : null
        };

        private static bool Contains(string haystack, string needle)
            => haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
