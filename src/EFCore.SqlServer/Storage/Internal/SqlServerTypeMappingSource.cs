// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerTypeMappingSource : RelationalTypeMappingSource
    {
        private readonly RelationalTypeMapping _sqlVariant
            = new SqlServerSqlVariantTypeMapping("sql_variant");

        private readonly FloatTypeMapping _real
            = new SqlServerFloatTypeMapping("real");

        private readonly ByteTypeMapping _byte
            = new SqlServerByteTypeMapping("tinyint", DbType.Byte);

        private readonly ShortTypeMapping _short
            = new SqlServerShortTypeMapping("smallint", DbType.Int16);

        private readonly LongTypeMapping _long
            = new SqlServerLongTypeMapping("bigint", DbType.Int64);

        private readonly SqlServerByteArrayTypeMapping _rowversion
            = new SqlServerByteArrayTypeMapping(
                "rowversion",
                DbType.Binary,
                size: 8,
                comparer: new ValueComparer<byte[]>(
                    (v1, v2) => StructuralComparisons.StructuralEqualityComparer.Equals(v1, v2),
                    v => StructuralComparisons.StructuralEqualityComparer.GetHashCode(v),
                    v => v == null ? null : v.ToArray()),
                storeTypePostfix: StoreTypePostfix.None);

        private readonly IntTypeMapping _int
            = new IntTypeMapping("int", DbType.Int32);

        private readonly BoolTypeMapping _bool
            = new BoolTypeMapping("bit");

        private readonly SqlServerStringTypeMapping _fixedLengthUnicodeString
            = new SqlServerStringTypeMapping("nchar", dbType: DbType.String, unicode: true, fixedLength: true);

        private readonly SqlServerStringTypeMapping _variableLengthUnicodeString
            = new SqlServerStringTypeMapping("nvarchar", dbType: null, unicode: true);

        private readonly SqlServerStringTypeMapping _fixedLengthAnsiString
            = new SqlServerStringTypeMapping("char", dbType: DbType.AnsiString, fixedLength: true);

        private readonly SqlServerStringTypeMapping _variableLengthAnsiString
            = new SqlServerStringTypeMapping("varchar", dbType: DbType.AnsiString);

        private readonly SqlServerByteArrayTypeMapping _variableLengthBinary
            = new SqlServerByteArrayTypeMapping("varbinary");

        private readonly SqlServerByteArrayTypeMapping _fixedLengthBinary
            = new SqlServerByteArrayTypeMapping("binary", fixedLength: true);

        private readonly SqlServerDateTimeTypeMapping _date
            = new SqlServerDateTimeTypeMapping("date", dbType: DbType.Date);

        private readonly SqlServerDateTimeTypeMapping _datetime
            = new SqlServerDateTimeTypeMapping("datetime", dbType: DbType.DateTime);

        private readonly SqlServerDateTimeTypeMapping _datetime2
            = new SqlServerDateTimeTypeMapping("datetime2", dbType: DbType.DateTime2);

        private readonly DoubleTypeMapping _double
            = new SqlServerDoubleTypeMapping("float");

        private readonly SqlServerDateTimeOffsetTypeMapping _datetimeoffset
            = new SqlServerDateTimeOffsetTypeMapping("datetimeoffset");

        private readonly GuidTypeMapping _uniqueidentifier
            = new GuidTypeMapping("uniqueidentifier", DbType.Guid);

        private readonly DecimalTypeMapping _decimal
            = new SqlServerDecimalTypeMapping("decimal(18, 2)", null, 18, 2);

        private readonly TimeSpanTypeMapping _time
            = new SqlServerTimeSpanTypeMapping("time");

        private readonly SqlServerStringTypeMapping _xml
            = new SqlServerStringTypeMapping("xml", dbType: null, unicode: true);

        private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;

        private readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings;

        // These are disallowed only if specified without any kind of length specified in parenthesis.
        private readonly HashSet<string> _disallowedMappings
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "binary",
                "binary varying",
                "varbinary",
                "char",
                "character",
                "char varying",
                "character varying",
                "varchar",
                "national char",
                "national character",
                "nchar",
                "national char varying",
                "national character varying",
                "nvarchar"
            };

        private readonly IReadOnlyDictionary<string, Func<Type, RelationalTypeMapping>> _namedClrMappings
            = new Dictionary<string, Func<Type, RelationalTypeMapping>>(StringComparer.Ordinal)
            {
                { "Microsoft.SqlServer.Types.SqlHierarchyId", t => new SqlServerUdtTypeMapping(t, "hierarchyid") },
                { "Microsoft.SqlServer.Types.SqlGeography", t => new SqlServerUdtTypeMapping(t, "geography") },
                { "Microsoft.SqlServer.Types.SqlGeometry", t => new SqlServerUdtTypeMapping(t, "geometry") }
            };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerTypeMappingSource(
            [NotNull] TypeMappingSourceDependencies dependencies,
            [NotNull] RelationalTypeMappingSourceDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
            _clrTypeMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
                    { typeof(int), _int },
                    { typeof(long), _long },
                    { typeof(DateTime), _datetime2 },
                    { typeof(Guid), _uniqueidentifier },
                    { typeof(bool), _bool },
                    { typeof(byte), _byte },
                    { typeof(double), _double },
                    { typeof(DateTimeOffset), _datetimeoffset },
                    { typeof(short), _short },
                    { typeof(float), _real },
                    { typeof(decimal), _decimal },
                    { typeof(TimeSpan), _time }
                };

            _storeTypeMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
                {
                    { "bigint", _long },
                    { "binary varying", _variableLengthBinary },
                    { "binary", _fixedLengthBinary },
                    { "bit", _bool },
                    { "char varying", _variableLengthAnsiString },
                    { "char", _fixedLengthAnsiString },
                    { "character varying", _variableLengthAnsiString },
                    { "character", _fixedLengthAnsiString },
                    { "date", _date },
                    { "datetime", _datetime },
                    { "datetime2", _datetime2 },
                    { "datetimeoffset", _datetimeoffset },
                    { "dec", _decimal },
                    { "decimal", _decimal },
                    { "double precision", _double },
                    { "float", _double },
                    { "image", _variableLengthBinary },
                    { "int", _int },
                    { "money", _decimal },
                    { "national char varying", _variableLengthUnicodeString },
                    { "national character varying", _variableLengthUnicodeString },
                    { "national character", _fixedLengthUnicodeString },
                    { "nchar", _fixedLengthUnicodeString },
                    { "ntext", _variableLengthUnicodeString },
                    { "numeric", _decimal },
                    { "nvarchar", _variableLengthUnicodeString },
                    { "real", _real },
                    { "rowversion", _rowversion },
                    { "smalldatetime", _datetime },
                    { "smallint", _short },
                    { "smallmoney", _decimal },
                    { "sql_variant", _sqlVariant },
                    { "text", _variableLengthAnsiString },
                    { "time", _time },
                    { "timestamp", _rowversion },
                    { "tinyint", _byte },
                    { "uniqueidentifier", _uniqueidentifier },
                    { "varbinary", _variableLengthBinary },
                    { "varchar", _variableLengthAnsiString },
                    { "xml", _xml }
                };
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void ValidateMapping(CoreTypeMapping mapping, IProperty property)
        {
            var relationalMapping = mapping as RelationalTypeMapping;

            if (_disallowedMappings.Contains(relationalMapping?.StoreType))
            {
                if (property== null)
                {
                    throw new ArgumentException(SqlServerStrings.UnqualifiedDataType(relationalMapping.StoreType));
                }

                throw new ArgumentException(SqlServerStrings.UnqualifiedDataTypeOnProperty(relationalMapping.StoreType, property.Name));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
            => FindRawMapping(mappingInfo)?.Clone(mappingInfo);

        private RelationalTypeMapping FindRawMapping(RelationalTypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            var storeTypeName = mappingInfo.StoreTypeName;
            var storeTypeNameBase = mappingInfo.StoreTypeNameBase;

            if (storeTypeName != null)
            {
                if (clrType == typeof(float)
                    && mappingInfo.Size != null
                    && mappingInfo.Size <= 24
                    && (storeTypeNameBase.Equals("float", StringComparison.OrdinalIgnoreCase)
                        || storeTypeNameBase.Equals("double precision", StringComparison.OrdinalIgnoreCase)))
                {
                    return _real;
                }

                if (_storeTypeMappings.TryGetValue(storeTypeName, out var mapping)
                    || _storeTypeMappings.TryGetValue(storeTypeNameBase, out mapping))
                {
                    return clrType == null
                           || mapping.ClrType == clrType
                        ? mapping
                        : null;
                }
            }

            if (clrType != null)
            {
                if (_clrTypeMappings.TryGetValue(clrType, out var mapping))
                {
                    return mapping;
                }

                if(_namedClrMappings.TryGetValue(clrType.FullName, out var mappingFunc))
                {
                    return mappingFunc(clrType);
                }

                if (clrType == typeof(string))
                {
                    var isAnsi = mappingInfo.IsUnicode == false;
                    var isFixedLength = mappingInfo.IsFixedLength == true;
                    var baseName = (isAnsi ? "" : "n") + (isFixedLength ? "char" : "varchar");
                    var maxSize = isAnsi ? 8000 : 4000;

                    var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? (int?)(isAnsi ? 900 : 450) : null);
                    if (size > maxSize)
                    {
                        size = null;
                    }

                    var dbType = isAnsi
                        ? (isFixedLength ? DbType.AnsiStringFixedLength : DbType.AnsiString)
                        : (isFixedLength ? DbType.StringFixedLength : (DbType?)null);

                    return new SqlServerStringTypeMapping(
                        baseName + "(" + (size == null ? "max" : size.ToString()) + ")",
                        dbType,
                        !isAnsi,
                        size,
                        isFixedLength);
                }

                if (clrType == typeof(byte[]))
                {
                    if (mappingInfo.IsRowVersion == true)
                    {
                        return _rowversion;
                    }

                    var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? (int?)900 : null);
                    if (size > 8000)
                    {
                        size = null;
                    }

                    var isFixedLength = mappingInfo.IsFixedLength == true;

                    return new SqlServerByteArrayTypeMapping(
                        (isFixedLength ? "binary(" : "varbinary(") + (size == null ? "max" : size.ToString()) + ")",
                        DbType.Binary,
                        size);
                }
            }

            return null;
        }
    }
}
