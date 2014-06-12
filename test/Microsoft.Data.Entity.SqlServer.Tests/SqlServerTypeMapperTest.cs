// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using Microsoft.Data.Entity.Relational.Model;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerTypeMapperTest
    {
        [Fact]
        public void Does_simple_SQL_Server_mappings_to_DDL_types()
        {
            Assert.Equal("int", GetTypeMapping(typeof(int)).StoreTypeName);
            Assert.Equal("datetime2", GetTypeMapping(typeof(DateTime)).StoreTypeName);
            Assert.Equal("uniqueidentifier", GetTypeMapping(typeof(Guid)).StoreTypeName);
            Assert.Equal("int", GetTypeMapping(typeof(char)).StoreTypeName);
            Assert.Equal("tinyint", GetTypeMapping(typeof(byte)).StoreTypeName);
            Assert.Equal("float", GetTypeMapping(typeof(double)).StoreTypeName);
            Assert.Equal("smallint", GetTypeMapping(typeof(sbyte)).StoreTypeName);
            Assert.Equal("int", GetTypeMapping(typeof(ushort)).StoreTypeName);
            Assert.Equal("bigint", GetTypeMapping(typeof(uint)).StoreTypeName);
            Assert.Equal("numeric(20, 0)", GetTypeMapping(typeof(ulong)).StoreTypeName);
            Assert.Equal("bit", GetTypeMapping(typeof(bool)).StoreTypeName);
            Assert.Equal("smallint", GetTypeMapping(typeof(short)).StoreTypeName);
            Assert.Equal("bigint", GetTypeMapping(typeof(long)).StoreTypeName);
            Assert.Equal("real", GetTypeMapping(typeof(float)).StoreTypeName);
            Assert.Equal("datetimeoffset", GetTypeMapping(typeof(DateTimeOffset)).StoreTypeName);
        }

        [Fact]
        public void Does_simple_SQL_Server_mappings_for_nullable_CLR_types_to_DDL_types()
        {
            Assert.Equal("int", GetTypeMapping(typeof(int?)).StoreTypeName);
            Assert.Equal("datetime2", GetTypeMapping(typeof(DateTime?)).StoreTypeName);
            Assert.Equal("uniqueidentifier", GetTypeMapping(typeof(Guid?)).StoreTypeName);
            Assert.Equal("int", GetTypeMapping(typeof(char?)).StoreTypeName);
            Assert.Equal("tinyint", GetTypeMapping(typeof(byte?)).StoreTypeName);
            Assert.Equal("float", GetTypeMapping(typeof(double?)).StoreTypeName);
            Assert.Equal("smallint", GetTypeMapping(typeof(sbyte?)).StoreTypeName);
            Assert.Equal("int", GetTypeMapping(typeof(ushort?)).StoreTypeName);
            Assert.Equal("bigint", GetTypeMapping(typeof(uint?)).StoreTypeName);
            Assert.Equal("numeric(20, 0)", GetTypeMapping(typeof(ulong?)).StoreTypeName);
            Assert.Equal("bit", GetTypeMapping(typeof(bool?)).StoreTypeName);
            Assert.Equal("smallint", GetTypeMapping(typeof(short?)).StoreTypeName);
            Assert.Equal("bigint", GetTypeMapping(typeof(long?)).StoreTypeName);
            Assert.Equal("real", GetTypeMapping(typeof(float?)).StoreTypeName);
            Assert.Equal("datetimeoffset", GetTypeMapping(typeof(DateTimeOffset?)).StoreTypeName);
        }

        [Fact]
        public void Does_simple_SQL_Server_mappings_for_enums_to_DDL_types()
        {
            Assert.Equal("int", GetTypeMapping(typeof(IntEnum)).StoreTypeName);
            Assert.Equal("tinyint", GetTypeMapping(typeof(ByteEnum)).StoreTypeName);
            Assert.Equal("smallint", GetTypeMapping(typeof(SByteEnum)).StoreTypeName);
            Assert.Equal("int", GetTypeMapping(typeof(UShortEnum)).StoreTypeName);
            Assert.Equal("bigint", GetTypeMapping(typeof(UIntEnum)).StoreTypeName);
            Assert.Equal("numeric(20, 0)", GetTypeMapping(typeof(ULongEnum)).StoreTypeName);
            Assert.Equal("smallint", GetTypeMapping(typeof(ShortEnum)).StoreTypeName);
            Assert.Equal("bigint", GetTypeMapping(typeof(LongEnum)).StoreTypeName);
            Assert.Equal("int", GetTypeMapping(typeof(IntEnum?)).StoreTypeName);
            Assert.Equal("tinyint", GetTypeMapping(typeof(ByteEnum?)).StoreTypeName);
            Assert.Equal("smallint", GetTypeMapping(typeof(SByteEnum?)).StoreTypeName);
            Assert.Equal("int", GetTypeMapping(typeof(UShortEnum?)).StoreTypeName);
            Assert.Equal("bigint", GetTypeMapping(typeof(UIntEnum?)).StoreTypeName);
            Assert.Equal("numeric(20, 0)", GetTypeMapping(typeof(ULongEnum?)).StoreTypeName);
            Assert.Equal("smallint", GetTypeMapping(typeof(ShortEnum?)).StoreTypeName);
            Assert.Equal("bigint", GetTypeMapping(typeof(LongEnum?)).StoreTypeName);
        }

        [Fact]
        public void Does_simple_SQL_Server_mappings_to_DbTypes()
        {
            Assert.Equal(DbType.Int32, GetTypeMapping(typeof(int)).StoreType);
            Assert.Equal(DbType.DateTime2, GetTypeMapping(typeof(DateTime)).StoreType);
            Assert.Equal(DbType.Guid, GetTypeMapping(typeof(Guid)).StoreType);
            Assert.Equal(DbType.Int32, GetTypeMapping(typeof(char)).StoreType);
            Assert.Equal(DbType.Byte, GetTypeMapping(typeof(byte)).StoreType);
            Assert.Equal(DbType.Double, GetTypeMapping(typeof(double)).StoreType);
            Assert.Equal(DbType.SByte, GetTypeMapping(typeof(sbyte)).StoreType);
            Assert.Equal(DbType.UInt16, GetTypeMapping(typeof(ushort)).StoreType);
            Assert.Equal(DbType.UInt32, GetTypeMapping(typeof(uint)).StoreType);
            Assert.Equal(DbType.UInt64, GetTypeMapping(typeof(ulong)).StoreType);
            Assert.Equal(DbType.Boolean, GetTypeMapping(typeof(bool)).StoreType);
            Assert.Equal(DbType.Int16, GetTypeMapping(typeof(short)).StoreType);
            Assert.Equal(DbType.Int64, GetTypeMapping(typeof(long)).StoreType);
            Assert.Equal(DbType.Single, GetTypeMapping(typeof(float)).StoreType);
            Assert.Equal(DbType.DateTimeOffset, GetTypeMapping(typeof(DateTimeOffset)).StoreType);
        }

        [Fact]
        public void Does_simple_SQL_Server_mappings_for_nullable_CLR_types_to_DbTypes()
        {
            Assert.Equal(DbType.Int32, GetTypeMapping(typeof(int?)).StoreType);
            Assert.Equal(DbType.DateTime2, GetTypeMapping(typeof(DateTime?)).StoreType);
            Assert.Equal(DbType.Guid, GetTypeMapping(typeof(Guid?)).StoreType);
            Assert.Equal(DbType.Int32, GetTypeMapping(typeof(char?)).StoreType);
            Assert.Equal(DbType.Byte, GetTypeMapping(typeof(byte?)).StoreType);
            Assert.Equal(DbType.Double, GetTypeMapping(typeof(double?)).StoreType);
            Assert.Equal(DbType.SByte, GetTypeMapping(typeof(sbyte?)).StoreType);
            Assert.Equal(DbType.UInt16, GetTypeMapping(typeof(ushort?)).StoreType);
            Assert.Equal(DbType.UInt32, GetTypeMapping(typeof(uint?)).StoreType);
            Assert.Equal(DbType.UInt64, GetTypeMapping(typeof(ulong?)).StoreType);
            Assert.Equal(DbType.Boolean, GetTypeMapping(typeof(bool?)).StoreType);
            Assert.Equal(DbType.Int16, GetTypeMapping(typeof(short?)).StoreType);
            Assert.Equal(DbType.Int64, GetTypeMapping(typeof(long?)).StoreType);
            Assert.Equal(DbType.Single, GetTypeMapping(typeof(float?)).StoreType);
            Assert.Equal(DbType.DateTimeOffset, GetTypeMapping(typeof(DateTimeOffset?)).StoreType);
        }

        [Fact]
        public void Does_simple_SQL_Server_mappings_for_enums_to_DbTypes()
        {
            Assert.Equal(DbType.Int32, GetTypeMapping(typeof(IntEnum)).StoreType);
            Assert.Equal(DbType.Byte, GetTypeMapping(typeof(ByteEnum)).StoreType);
            Assert.Equal(DbType.SByte, GetTypeMapping(typeof(SByteEnum)).StoreType);
            Assert.Equal(DbType.UInt16, GetTypeMapping(typeof(UShortEnum)).StoreType);
            Assert.Equal(DbType.UInt32, GetTypeMapping(typeof(UIntEnum)).StoreType);
            Assert.Equal(DbType.UInt64, GetTypeMapping(typeof(ULongEnum)).StoreType);
            Assert.Equal(DbType.Int16, GetTypeMapping(typeof(ShortEnum)).StoreType);
            Assert.Equal(DbType.Int64, GetTypeMapping(typeof(LongEnum)).StoreType);
            Assert.Equal(DbType.Int32, GetTypeMapping(typeof(IntEnum?)).StoreType);
            Assert.Equal(DbType.Byte, GetTypeMapping(typeof(ByteEnum?)).StoreType);
            Assert.Equal(DbType.SByte, GetTypeMapping(typeof(SByteEnum?)).StoreType);
            Assert.Equal(DbType.UInt16, GetTypeMapping(typeof(UShortEnum?)).StoreType);
            Assert.Equal(DbType.UInt32, GetTypeMapping(typeof(UIntEnum?)).StoreType);
            Assert.Equal(DbType.UInt64, GetTypeMapping(typeof(ULongEnum?)).StoreType);
            Assert.Equal(DbType.Int16, GetTypeMapping(typeof(ShortEnum?)).StoreType);
            Assert.Equal(DbType.Int64, GetTypeMapping(typeof(LongEnum?)).StoreType);
        }

        [Fact]
        public void Does_decimal_mapping()
        {
            var typeMapping = (RelationalDecimalTypeMapping)GetTypeMapping(typeof(decimal));

            Assert.Equal(DbType.Decimal, typeMapping.StoreType);
            Assert.Equal(18, typeMapping.Precision);
            Assert.Equal(2, typeMapping.Scale);
            Assert.Equal("decimal(18, 2)", typeMapping.StoreTypeName);
        }

        [Fact]
        public void Does_decimal_mapping_for_nullable_CLR_types()
        {
            var typeMapping = (RelationalDecimalTypeMapping)GetTypeMapping(typeof(decimal?));

            Assert.Equal(DbType.Decimal, typeMapping.StoreType);
            Assert.Equal(18, typeMapping.Precision);
            Assert.Equal(2, typeMapping.Scale);
            Assert.Equal("decimal(18, 2)", typeMapping.StoreTypeName);
        }

        [Fact]
        public void Does_non_key_SQL_Server_string_mapping()
        {
            var typeMapping = new SqlServerTypeMapper()
                .GetTypeMapping(null, "MyColumn", typeof(string), isKey: false, isConcurrencyToken: false);

            Assert.Equal(DbType.String, typeMapping.StoreType);
            Assert.Equal("nvarchar(max)", typeMapping.StoreTypeName);
        }

        [Fact]
        public void Does_key_SQL_Server_string_mapping()
        {
            var typeMapping = (RelationalSizedTypeMapping)new SqlServerTypeMapper()
                .GetTypeMapping(null, "MyColumn", typeof(string), isKey: true, isConcurrencyToken: false);

            Assert.Equal(DbType.String, typeMapping.StoreType);
            Assert.Equal(128, typeMapping.Size);
            Assert.Equal("nvarchar(128)", typeMapping.StoreTypeName);
        }

        [Fact]
        public void Does_rowversion_mapping()
        {
            var typeMapping = (RelationalSizedTypeMapping)new SqlServerTypeMapper()
                .GetTypeMapping(null, "MyColumn", typeof(byte[]), isKey: false, isConcurrencyToken: true);

            Assert.Equal(DbType.Binary, typeMapping.StoreType);
            Assert.Equal(8, typeMapping.Size);
            Assert.Equal("rowversion", typeMapping.StoreTypeName);
        }

        private static RelationalTypeMapping GetTypeMapping(Type propertyType)
        {
            return new SqlServerTypeMapper().GetTypeMapping(null, "MyColumn", propertyType, isKey: false, isConcurrencyToken: false);
        }

        private enum LongEnum : long
        {
        }

        private enum IntEnum : int
        {
        }

        private enum ShortEnum : short
        {
        }

        private enum ByteEnum : byte
        {
        }

        private enum ULongEnum : ulong
        {
        }

        private enum UIntEnum : uint
        {
        }

        private enum UShortEnum : ushort
        {
        }

        private enum SByteEnum : sbyte
        {
        }
    }
}
