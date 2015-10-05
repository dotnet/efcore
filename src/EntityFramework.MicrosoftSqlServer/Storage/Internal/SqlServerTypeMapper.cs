// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class SqlServerTypeMapper : RelationalTypeMapper
    {
        private readonly SqlServerMaxLengthMapping _nvarcharmax = new SqlServerMaxLengthMapping("nvarchar(max)");
        private readonly SqlServerMaxLengthMapping _nvarchar450 = new SqlServerMaxLengthMapping("nvarchar(450)");
        private readonly SqlServerMaxLengthMapping _varbinarymax = new SqlServerMaxLengthMapping("varbinary(max)", DbType.Binary);
        private readonly SqlServerMaxLengthMapping _varbinary900 = new SqlServerMaxLengthMapping("varbinary(900)", DbType.Binary);
        private readonly RelationalSizedTypeMapping _rowversion = new RelationalSizedTypeMapping("rowversion", DbType.Binary, 8);
        private readonly RelationalTypeMapping _int = new RelationalTypeMapping("int", DbType.Int32);
        private readonly RelationalTypeMapping _bigint = new RelationalTypeMapping("bigint", DbType.Int64);
        private readonly RelationalTypeMapping _bit = new RelationalTypeMapping("bit");
        private readonly RelationalTypeMapping _smallint = new RelationalTypeMapping("smallint", DbType.Int16);
        private readonly RelationalTypeMapping _tinyint = new RelationalTypeMapping("tinyint", DbType.Byte);
        private readonly SqlServerMaxLengthMapping _nchar = new SqlServerMaxLengthMapping("nchar", DbType.StringFixedLength);
        private readonly SqlServerMaxLengthMapping _nvarchar = new SqlServerMaxLengthMapping("nvarchar");
        private readonly RelationalTypeMapping _varcharmax = new SqlServerMaxLengthMapping("varchar(max)", DbType.AnsiString);
        private readonly SqlServerMaxLengthMapping _char = new SqlServerMaxLengthMapping("char", DbType.AnsiStringFixedLength);
        private readonly SqlServerMaxLengthMapping _varchar = new SqlServerMaxLengthMapping("varchar", DbType.AnsiString);
        private readonly SqlServerMaxLengthMapping _varbinary = new SqlServerMaxLengthMapping("varbinary", DbType.Binary);
        private readonly RelationalTypeMapping _datetime2 = new RelationalTypeMapping("datetime2", DbType.DateTime2);
        private readonly RelationalTypeMapping _double = new RelationalTypeMapping("float");
        private readonly RelationalTypeMapping _datetimeoffset = new RelationalTypeMapping("datetimeoffset");
        private readonly RelationalTypeMapping _real = new RelationalTypeMapping("real");
        private readonly RelationalTypeMapping _uniqueidentifier = new RelationalTypeMapping("uniqueidentifier");
        private readonly RelationalTypeMapping _decimal = new RelationalTypeMapping("decimal(18, 2)");
        private readonly RelationalTypeMapping _time = new RelationalTypeMapping("time");

        private readonly Dictionary<string, RelationalTypeMapping> _simpleNameMappings;
        private readonly Dictionary<Type, RelationalTypeMapping> _simpleMappings;

        public SqlServerTypeMapper()
        {
            _simpleNameMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
                {
                    { "datetime2", _datetime2 },
                    { "char", _char },
                    { "character", _char },
                    { "varchar", _varchar },
                    { "char varying", _varchar },
                    { "character varying", _varchar },
                    { "varchar(max)", _varcharmax },
                    { "char varying(max)", _varcharmax },
                    { "character varying(max)", _varcharmax },
                    { "nchar", _nchar },
                    { "national character", _nchar },
                    { "nvarchar", _nvarchar },
                    { "national char varying", _nvarchar },
                    { "national character varying", _nvarchar },
                    { "text", _varchar },
                    { "ntext", _nvarchar },
                    { "binary", _varbinary },
                    { "varbinary", _varbinary },
                    { "binary varying", _varbinary },
                    { "rowversion", _rowversion },
                    { "timestamp", _rowversion },
                    { "decimal", _decimal },
                    { "dec", _decimal },
                    { "numeric", _decimal }
                };

            _simpleMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
                    { typeof(int), _int },
                    { typeof(long), _bigint },
                    { typeof(DateTime), _datetime2 },
                    { typeof(Guid), _uniqueidentifier },
                    { typeof(bool), _bit },
                    { typeof(byte), _tinyint },
                    { typeof(double), _double },
                    { typeof(DateTimeOffset), _datetimeoffset },
                    { typeof(char), _int },
                    { typeof(sbyte), new RelationalTypeMapping("smallint") },
                    { typeof(ushort), new RelationalTypeMapping("int") },
                    { typeof(uint), new RelationalTypeMapping("bigint") },
                    { typeof(ulong), new RelationalTypeMapping("numeric(20, 0)") },
                    { typeof(short), _smallint },
                    { typeof(float), _real },
                    { typeof(decimal), _decimal },
                    { typeof(TimeSpan), _time }
                };
        }

        protected override string GetColumnType(IProperty property) => property.SqlServer().ColumnType;

        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> SimpleMappings
            => _simpleMappings;

        protected override IReadOnlyDictionary<string, RelationalTypeMapping> SimpleNameMappings
            => _simpleNameMappings;

        public override RelationalTypeMapping GetMapping(Type clrType)
        {
            Check.NotNull(clrType, nameof(clrType));

            return clrType == typeof(string)
                ? _nvarcharmax
                : (clrType == typeof(byte[])
                    ? _varbinarymax
                    : base.GetMapping(clrType));
        }

        protected override RelationalTypeMapping GetCustomMapping(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var clrType = property.ClrType.UnwrapEnumType();

            return clrType == typeof(string)
                ? GetStringMapping(
                    property, 4000,
                    maxLength => new SqlServerMaxLengthMapping("nvarchar(" + maxLength + ")"),
                    _nvarcharmax, _nvarcharmax, _nvarchar450)
                : clrType == typeof(byte[])
                    ? GetByteArrayMapping(property, 8000,
                        maxLength => new SqlServerMaxLengthMapping("varbinary(" + maxLength + ")", DbType.Binary),
                        _varbinarymax, _varbinarymax, _varbinary900, _rowversion)
                    : base.GetCustomMapping(property);
        }
    }
}
