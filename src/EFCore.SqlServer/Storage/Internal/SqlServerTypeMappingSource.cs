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
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
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
                size: 8,
                comparer: new ValueComparer<byte[]>(
                    (v1, v2) => StructuralComparisons.StructuralEqualityComparer.Equals(v1, v2),
                    v => StructuralComparisons.StructuralEqualityComparer.GetHashCode(v),
                    v => v == null ? null : v.ToArray()),
                storeTypePostfix: StoreTypePostfix.None);

        private readonly IntTypeMapping _int
            = new IntTypeMapping("int", DbType.Int32);

        private readonly BoolTypeMapping _bool
            = new SqlServerBoolTypeMapping("bit");

        private readonly SqlServerStringTypeMapping _fixedLengthUnicodeString
            = new SqlServerStringTypeMapping(unicode: true, fixedLength: true);

        private readonly SqlServerStringTypeMapping _textUnicodeString
            = new SqlServerStringTypeMapping("ntext", unicode: true, sqlDbType: SqlDbType.NText);

        private readonly SqlServerStringTypeMapping _variableLengthUnicodeString
            = new SqlServerStringTypeMapping(unicode: true);

        private readonly SqlServerStringTypeMapping _variableLengthMaxUnicodeString
            = new SqlServerStringTypeMapping("nvarchar(max)", unicode: true, storeTypePostfix: StoreTypePostfix.None);

        private readonly SqlServerStringTypeMapping _fixedLengthAnsiString
            = new SqlServerStringTypeMapping(fixedLength: true);

        private readonly SqlServerStringTypeMapping _textAnsiString
            = new SqlServerStringTypeMapping("text", sqlDbType: SqlDbType.Text);

        private readonly SqlServerStringTypeMapping _variableLengthAnsiString
            = new SqlServerStringTypeMapping();

        private readonly SqlServerStringTypeMapping _variableLengthMaxAnsiString
            = new SqlServerStringTypeMapping("varchar(max)", storeTypePostfix: StoreTypePostfix.None);

        private readonly SqlServerByteArrayTypeMapping _variableLengthBinary
            = new SqlServerByteArrayTypeMapping();

        private readonly SqlServerByteArrayTypeMapping _imageBinary
            = new SqlServerByteArrayTypeMapping("image", sqlDbType: SqlDbType.Image);

        private readonly SqlServerByteArrayTypeMapping _variableLengthMaxBinary
            = new SqlServerByteArrayTypeMapping("varbinary(max)", storeTypePostfix: StoreTypePostfix.None);

        private readonly SqlServerByteArrayTypeMapping _fixedLengthBinary
            = new SqlServerByteArrayTypeMapping(fixedLength: true);

        private readonly SqlServerDateTimeTypeMapping _date
            = new SqlServerDateTimeTypeMapping("date", DbType.Date);

        private readonly SqlServerDateTimeTypeMapping _datetime
            = new SqlServerDateTimeTypeMapping("datetime", DbType.DateTime);

        private readonly SqlServerDateTimeTypeMapping _datetime2
            = new SqlServerDateTimeTypeMapping("datetime2", DbType.DateTime2);

        private readonly DoubleTypeMapping _double
            = new SqlServerDoubleTypeMapping("float");

        private readonly SqlServerDateTimeOffsetTypeMapping _datetimeoffset
            = new SqlServerDateTimeOffsetTypeMapping("datetimeoffset");

        private readonly GuidTypeMapping _uniqueidentifier
            = new GuidTypeMapping("uniqueidentifier", DbType.Guid);

        private readonly DecimalTypeMapping _decimal
            = new SqlServerDecimalTypeMapping("decimal(18, 2)", precision: 18, scale: 2, storeTypePostfix: StoreTypePostfix.PrecisionAndScale);

        private readonly DecimalTypeMapping _money
            = new SqlServerDecimalTypeMapping("money");

        private readonly TimeSpanTypeMapping _time
            = new SqlServerTimeSpanTypeMapping("time");

        private readonly SqlServerStringTypeMapping _xml
            = new SqlServerStringTypeMapping("xml", unicode: true, storeTypePostfix: StoreTypePostfix.None);

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
                { "Microsoft.SqlServer.Types.SqlHierarchyId", t => SqlServerUdtTypeMapping.CreateSqlHierarchyIdMapping(t) },
                { "Microsoft.SqlServer.Types.SqlGeography", t => SqlServerUdtTypeMapping.CreateSqlSpatialMapping(t, "geography") },
                { "Microsoft.SqlServer.Types.SqlGeometry", t => SqlServerUdtTypeMapping.CreateSqlSpatialMapping(t, "geometry") }
            };

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
                    { "image", _imageBinary },
                    { "int", _int },
                    { "money", _money },
                    { "national char varying", _variableLengthUnicodeString },
                    { "national character varying", _variableLengthUnicodeString },
                    { "national character", _fixedLengthUnicodeString },
                    { "nchar", _fixedLengthUnicodeString },
                    { "ntext", _textUnicodeString },
                    { "numeric", _decimal },
                    { "nvarchar", _variableLengthUnicodeString },
                    { "nvarchar(max)", _variableLengthMaxUnicodeString },
                    { "real", _real },
                    { "rowversion", _rowversion },
                    { "smalldatetime", _datetime },
                    { "smallint", _short },
                    { "smallmoney", _money },
                    { "sql_variant", _sqlVariant },
                    { "text", _textAnsiString },
                    { "time", _time },
                    { "timestamp", _rowversion },
                    { "tinyint", _byte },
                    { "uniqueidentifier", _uniqueidentifier },
                    { "varbinary", _variableLengthBinary },
                    { "varbinary(max)", _variableLengthMaxBinary },
                    { "varchar", _variableLengthAnsiString },
                    { "varchar(max)", _variableLengthMaxAnsiString },
                    { "xml", _xml }
                };
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void ValidateMapping(CoreTypeMapping mapping, IProperty property)
        {
            var relationalMapping = mapping as RelationalTypeMapping;

            if (_disallowedMappings.Contains(relationalMapping?.StoreType))
            {
                if (property == null)
                {
                    throw new ArgumentException(SqlServerStrings.UnqualifiedDataType(relationalMapping.StoreType));
                }

                throw new ArgumentException(SqlServerStrings.UnqualifiedDataTypeOnProperty(relationalMapping.StoreType, property.Name));
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
            => FindRawMapping(mappingInfo)?.Clone(mappingInfo)
               ?? base.FindMapping(mappingInfo);

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

                if (_namedClrMappings.TryGetValue(clrType.FullName, out var mappingFunc))
                {
                    return mappingFunc(clrType);
                }

                if (clrType == typeof(string))
                {
                    var isAnsi = mappingInfo.IsUnicode == false;
                    var isFixedLength = mappingInfo.IsFixedLength == true;
                    var maxSize = isAnsi ? 8000 : 4000;

                    var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? (int?)(isAnsi ? 900 : 450) : null);
                    if (size > maxSize)
                    {
                        size = isFixedLength ? maxSize : (int?)null;
                    }

                    return size == null
                        ? isAnsi ? _variableLengthMaxAnsiString : _variableLengthMaxUnicodeString
                        : new SqlServerStringTypeMapping(
                            unicode: !isAnsi,
                            size: size,
                            fixedLength: isFixedLength);
                }

                if (clrType == typeof(byte[]))
                {
                    if (mappingInfo.IsRowVersion == true)
                    {
                        return _rowversion;
                    }

                    var isFixedLength = mappingInfo.IsFixedLength == true;

                    var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? (int?)900 : null);
                    if (size > 8000)
                    {
                        size = isFixedLength ? 8000 : (int?)null;
                    }

                    return size == null
                        ? _variableLengthMaxBinary
                        : new SqlServerByteArrayTypeMapping(size: size, fixedLength: isFixedLength);
                }
            }

            return null;
        }
    }
}
