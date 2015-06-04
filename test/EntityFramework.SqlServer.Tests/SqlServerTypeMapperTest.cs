// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerTypeMapperTest
    {
        [Fact]
        public void Does_simple_SQL_Server_mappings_to_DDL_types()
        {
            Assert.Equal("int", GetTypeMapping(typeof(int)).DefaultTypeName);
            Assert.Equal("datetime2", GetTypeMapping(typeof(DateTime)).DefaultTypeName);
            Assert.Equal("uniqueidentifier", GetTypeMapping(typeof(Guid)).DefaultTypeName);
            Assert.Equal("int", GetTypeMapping(typeof(char)).DefaultTypeName);
            Assert.Equal("tinyint", GetTypeMapping(typeof(byte)).DefaultTypeName);
            Assert.Equal("float", GetTypeMapping(typeof(double)).DefaultTypeName);
            Assert.Equal("smallint", GetTypeMapping(typeof(sbyte)).DefaultTypeName);
            Assert.Equal("int", GetTypeMapping(typeof(ushort)).DefaultTypeName);
            Assert.Equal("bigint", GetTypeMapping(typeof(uint)).DefaultTypeName);
            Assert.Equal("numeric(20, 0)", GetTypeMapping(typeof(ulong)).DefaultTypeName);
            Assert.Equal("bit", GetTypeMapping(typeof(bool)).DefaultTypeName);
            Assert.Equal("smallint", GetTypeMapping(typeof(short)).DefaultTypeName);
            Assert.Equal("bigint", GetTypeMapping(typeof(long)).DefaultTypeName);
            Assert.Equal("real", GetTypeMapping(typeof(float)).DefaultTypeName);
            Assert.Equal("datetimeoffset", GetTypeMapping(typeof(DateTimeOffset)).DefaultTypeName);
        }

        [Fact]
        public void Does_simple_SQL_Server_mappings_for_nullable_CLR_types_to_DDL_types()
        {
            Assert.Equal("int", GetTypeMapping(typeof(int?)).DefaultTypeName);
            Assert.Equal("datetime2", GetTypeMapping(typeof(DateTime?)).DefaultTypeName);
            Assert.Equal("uniqueidentifier", GetTypeMapping(typeof(Guid?)).DefaultTypeName);
            Assert.Equal("int", GetTypeMapping(typeof(char?)).DefaultTypeName);
            Assert.Equal("tinyint", GetTypeMapping(typeof(byte?)).DefaultTypeName);
            Assert.Equal("float", GetTypeMapping(typeof(double?)).DefaultTypeName);
            Assert.Equal("smallint", GetTypeMapping(typeof(sbyte?)).DefaultTypeName);
            Assert.Equal("int", GetTypeMapping(typeof(ushort?)).DefaultTypeName);
            Assert.Equal("bigint", GetTypeMapping(typeof(uint?)).DefaultTypeName);
            Assert.Equal("numeric(20, 0)", GetTypeMapping(typeof(ulong?)).DefaultTypeName);
            Assert.Equal("bit", GetTypeMapping(typeof(bool?)).DefaultTypeName);
            Assert.Equal("smallint", GetTypeMapping(typeof(short?)).DefaultTypeName);
            Assert.Equal("bigint", GetTypeMapping(typeof(long?)).DefaultTypeName);
            Assert.Equal("real", GetTypeMapping(typeof(float?)).DefaultTypeName);
            Assert.Equal("datetimeoffset", GetTypeMapping(typeof(DateTimeOffset?)).DefaultTypeName);
        }

        [Fact]
        public void Does_simple_SQL_Server_mappings_for_enums_to_DDL_types()
        {
            Assert.Equal("int", GetTypeMapping(typeof(IntEnum)).DefaultTypeName);
            Assert.Equal("tinyint", GetTypeMapping(typeof(ByteEnum)).DefaultTypeName);
            Assert.Equal("smallint", GetTypeMapping(typeof(SByteEnum)).DefaultTypeName);
            Assert.Equal("int", GetTypeMapping(typeof(UShortEnum)).DefaultTypeName);
            Assert.Equal("bigint", GetTypeMapping(typeof(UIntEnum)).DefaultTypeName);
            Assert.Equal("numeric(20, 0)", GetTypeMapping(typeof(ULongEnum)).DefaultTypeName);
            Assert.Equal("smallint", GetTypeMapping(typeof(ShortEnum)).DefaultTypeName);
            Assert.Equal("bigint", GetTypeMapping(typeof(LongEnum)).DefaultTypeName);
            Assert.Equal("int", GetTypeMapping(typeof(IntEnum?)).DefaultTypeName);
            Assert.Equal("tinyint", GetTypeMapping(typeof(ByteEnum?)).DefaultTypeName);
            Assert.Equal("smallint", GetTypeMapping(typeof(SByteEnum?)).DefaultTypeName);
            Assert.Equal("int", GetTypeMapping(typeof(UShortEnum?)).DefaultTypeName);
            Assert.Equal("bigint", GetTypeMapping(typeof(UIntEnum?)).DefaultTypeName);
            Assert.Equal("numeric(20, 0)", GetTypeMapping(typeof(ULongEnum?)).DefaultTypeName);
            Assert.Equal("smallint", GetTypeMapping(typeof(ShortEnum?)).DefaultTypeName);
            Assert.Equal("bigint", GetTypeMapping(typeof(LongEnum?)).DefaultTypeName);
        }

        [Fact]
        public void Does_simple_SQL_Server_mappings_to_DbTypes()
        {
            Assert.Equal(DbType.Int32, GetTypeMapping(typeof(int)).StoreType);
            Assert.Null(GetTypeMapping(typeof(string)).StoreType);
            Assert.Equal(DbType.Binary, GetTypeMapping(typeof(byte[])).StoreType);
            Assert.Null(GetTypeMapping(typeof(TimeSpan)).StoreType);
            Assert.Null(GetTypeMapping(typeof(Guid)).StoreType);
            Assert.Equal(DbType.Int32, GetTypeMapping(typeof(char)).StoreType);
            Assert.Equal(DbType.Byte, GetTypeMapping(typeof(byte)).StoreType);
            Assert.Null(GetTypeMapping(typeof(double)).StoreType);
            Assert.Null(GetTypeMapping(typeof(sbyte)).StoreType);
            Assert.Null(GetTypeMapping(typeof(ushort)).StoreType);
            Assert.Null(GetTypeMapping(typeof(uint)).StoreType);
            Assert.Null(GetTypeMapping(typeof(ulong)).StoreType);
            Assert.Null(GetTypeMapping(typeof(bool)).StoreType);
            Assert.Equal(DbType.Int16, GetTypeMapping(typeof(short)).StoreType);
            Assert.Equal(DbType.Int64, GetTypeMapping(typeof(long)).StoreType);
            Assert.Null(GetTypeMapping(typeof(float)).StoreType);
            Assert.Null(GetTypeMapping(typeof(DateTimeOffset)).StoreType);
        }

        [Fact]
        public void Does_simple_SQL_Server_mappings_for_nullable_CLR_types_to_DbTypes()
        {
            Assert.Equal(DbType.Int32, GetTypeMapping(typeof(int?)).StoreType);
            Assert.Null(GetTypeMapping(typeof(string)).StoreType);
            Assert.Equal(DbType.Binary, GetTypeMapping(typeof(byte[])).StoreType);
            Assert.Null(GetTypeMapping(typeof(TimeSpan?)).StoreType);
            Assert.Null(GetTypeMapping(typeof(Guid?)).StoreType);
            Assert.Equal(DbType.Int32, GetTypeMapping(typeof(char?)).StoreType);
            Assert.Equal(DbType.Byte, GetTypeMapping(typeof(byte?)).StoreType);
            Assert.Null(GetTypeMapping(typeof(double?)).StoreType);
            Assert.Null(GetTypeMapping(typeof(sbyte?)).StoreType);
            Assert.Null(GetTypeMapping(typeof(ushort?)).StoreType);
            Assert.Null(GetTypeMapping(typeof(uint?)).StoreType);
            Assert.Null(GetTypeMapping(typeof(ulong?)).StoreType);
            Assert.Null(GetTypeMapping(typeof(bool?)).StoreType);
            Assert.Equal(DbType.Int16, GetTypeMapping(typeof(short?)).StoreType);
            Assert.Equal(DbType.Int64, GetTypeMapping(typeof(long?)).StoreType);
            Assert.Null(GetTypeMapping(typeof(float?)).StoreType);
            Assert.Null(GetTypeMapping(typeof(DateTimeOffset?)).StoreType);
        }

        [Fact]
        public void Does_simple_SQL_Server_mappings_for_enums_to_DbTypes()
        {
            Assert.Equal(DbType.Int32, GetTypeMapping(typeof(IntEnum)).StoreType);
            Assert.Equal(DbType.Byte, GetTypeMapping(typeof(ByteEnum)).StoreType);
            Assert.Null(GetTypeMapping(typeof(SByteEnum)).StoreType);
            Assert.Null(GetTypeMapping(typeof(UShortEnum)).StoreType);
            Assert.Null(GetTypeMapping(typeof(UIntEnum)).StoreType);
            Assert.Null(GetTypeMapping(typeof(ULongEnum)).StoreType);
            Assert.Equal(DbType.Int16, GetTypeMapping(typeof(ShortEnum)).StoreType);
            Assert.Equal(DbType.Int64, GetTypeMapping(typeof(LongEnum)).StoreType);
            Assert.Equal(DbType.Int32, GetTypeMapping(typeof(IntEnum?)).StoreType);
            Assert.Equal(DbType.Byte, GetTypeMapping(typeof(ByteEnum?)).StoreType);
            Assert.Null(GetTypeMapping(typeof(SByteEnum?)).StoreType);
            Assert.Null(GetTypeMapping(typeof(UShortEnum?)).StoreType);
            Assert.Null(GetTypeMapping(typeof(UIntEnum?)).StoreType);
            Assert.Null(GetTypeMapping(typeof(ULongEnum?)).StoreType);
            Assert.Equal(DbType.Int16, GetTypeMapping(typeof(ShortEnum?)).StoreType);
            Assert.Equal(DbType.Int64, GetTypeMapping(typeof(LongEnum?)).StoreType);
        }

        [Fact]
        public void Does_decimal_mapping()
        {
            var typeMapping = (RelationalScaledTypeMapping)GetTypeMapping(typeof(decimal));

            Assert.Null(typeMapping.StoreType);
            Assert.Equal(18, typeMapping.Precision);
            Assert.Equal(2, typeMapping.Scale.Value);
            Assert.Equal("decimal(18, 2)", typeMapping.DefaultTypeName);
        }

        [Fact]
        public void Does_decimal_mapping_for_nullable_CLR_types()
        {
            var typeMapping = (RelationalScaledTypeMapping)GetTypeMapping(typeof(decimal?));

            Assert.Null(typeMapping.StoreType);
            Assert.Equal(18, typeMapping.Precision);
            Assert.Equal(2, typeMapping.Scale.Value);
            Assert.Equal("decimal(18, 2)", typeMapping.DefaultTypeName);
        }

        [Fact]
        public void Does_non_key_SQL_Server_string_mapping()
        {
            var typeMapping = GetTypeMapping(typeof(string));

            Assert.Null(typeMapping.StoreType);
            Assert.Equal("nvarchar(max)", typeMapping.DefaultTypeName);
        }

        [Fact]
        public void Does_non_key_SQL_Server_required_string_mapping()
        {
            var typeMapping = GetTypeMapping(typeof(string), isNullable: false);

            Assert.Null(typeMapping.StoreType);
            Assert.Equal("nvarchar(max)", typeMapping.DefaultTypeName);
        }

        [Fact]
        public void Does_key_SQL_Server_string_mapping()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(string), shadowProperty: true);
            property.EntityType.SetPrimaryKey(property);
            property.IsNullable = false;

            var typeMapping = (RelationalSizedTypeMapping)new SqlServerTypeMapper().MapPropertyType(property);

            Assert.Null(typeMapping.StoreType);
            Assert.Equal("nvarchar(450)", typeMapping.DefaultTypeName);
            Assert.Equal(450, typeMapping.Size);
        }

        [Fact]
        public void Does_foreign_key_SQL_Server_string_mapping()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(string), shadowProperty: true);
            var fkProperty = property.EntityType.AddProperty("FK", typeof(string), shadowProperty: true);
            var pk = property.EntityType.SetPrimaryKey(property);
            property.EntityType.AddForeignKey(fkProperty, pk);

            var typeMapping = (RelationalSizedTypeMapping)new SqlServerTypeMapper().MapPropertyType(fkProperty);

            Assert.Null(typeMapping.StoreType);
            Assert.Equal("nvarchar(450)", typeMapping.DefaultTypeName);
            Assert.Equal(450, typeMapping.Size);
        }

        [Fact]
        public void Does_required_foreign_key_SQL_Server_string_mapping()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(string), shadowProperty: true);
            var fkProperty = property.EntityType.AddProperty("FK", typeof(string), shadowProperty: true);
            var pk = property.EntityType.SetPrimaryKey(property);
            property.EntityType.AddForeignKey(fkProperty, pk);
            fkProperty.IsNullable = false;

            var typeMapping = (RelationalSizedTypeMapping)new SqlServerTypeMapper().MapPropertyType(fkProperty);

            Assert.Null(typeMapping.StoreType);
            Assert.Equal("nvarchar(450)", typeMapping.DefaultTypeName);
            Assert.Equal(450, typeMapping.Size);
        }

        [Fact]
        public void Does_non_key_SQL_Server_binary_mapping()
        {
            var typeMapping = GetTypeMapping(typeof(byte[]));

            Assert.Equal(DbType.Binary, typeMapping.StoreType);
            Assert.Equal("varbinary(max)", typeMapping.DefaultTypeName);
        }

        [Fact]
        public void Does_non_key_SQL_Server_required_binary_mapping()
        {
            var typeMapping = GetTypeMapping(typeof(byte[]), isNullable: false);

            Assert.Equal(DbType.Binary, typeMapping.StoreType);
            Assert.Equal("varbinary(max)", typeMapping.DefaultTypeName);
        }

        [Fact]
        public void Does_key_SQL_Server_binary_mapping()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(byte[]), shadowProperty: true);
            property.EntityType.SetPrimaryKey(property);
            property.IsNullable = false;

            var typeMapping = (RelationalSizedTypeMapping)new SqlServerTypeMapper().MapPropertyType(property);

            Assert.Equal(DbType.Binary, typeMapping.StoreType);
            Assert.Equal("varbinary(900)", typeMapping.DefaultTypeName);
            Assert.Equal(900, typeMapping.Size);
        }

        [Fact]
        public void Does_foreign_key_SQL_Server_binary_mapping()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(byte[]), shadowProperty: true);
            var fkProperty = property.EntityType.AddProperty("FK", typeof(byte[]), shadowProperty: true);
            var pk = property.EntityType.SetPrimaryKey(property);
            property.EntityType.AddForeignKey(fkProperty, pk);

            var typeMapping = (RelationalSizedTypeMapping)new SqlServerTypeMapper().MapPropertyType(fkProperty);

            Assert.Equal(DbType.Binary, typeMapping.StoreType);
            Assert.Equal("varbinary(900)", typeMapping.DefaultTypeName);
            Assert.Equal(900, typeMapping.Size);
        }

        [Fact]
        public void Does_required_foreign_key_SQL_Server_binary_mapping()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(byte[]), shadowProperty: true);
            var fkProperty = property.EntityType.AddProperty("FK", typeof(byte[]), shadowProperty: true);
            var pk = property.EntityType.SetPrimaryKey(property);
            property.EntityType.AddForeignKey(fkProperty, pk);
            fkProperty.IsNullable = false;

            var typeMapping = (RelationalSizedTypeMapping)new SqlServerTypeMapper().MapPropertyType(fkProperty);

            Assert.Equal(DbType.Binary, typeMapping.StoreType);
            Assert.Equal("varbinary(900)", typeMapping.DefaultTypeName);
            Assert.Equal(900, typeMapping.Size);
        }

        [Fact]
        public void Does_non_key_SQL_Server_rowversion_mapping()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(byte[]), shadowProperty: true);
            property.IsConcurrencyToken = true;

            var typeMapping = new SqlServerTypeMapper().MapPropertyType(property);

            Assert.Equal(DbType.Binary, typeMapping.StoreType);
            Assert.Equal("rowversion", typeMapping.DefaultTypeName);
        }

        [Fact]
        public void Does_non_key_SQL_Server_required_rowversion_mapping()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(byte[]), shadowProperty: true);
            property.IsConcurrencyToken = true;
            property.IsNullable = false;

            var typeMapping = new SqlServerTypeMapper().MapPropertyType(property);

            Assert.Equal(DbType.Binary, typeMapping.StoreType);
            Assert.Equal("rowversion", typeMapping.DefaultTypeName);
        }

        private static RelationalTypeMapping GetTypeMapping(Type propertyType, bool? isNullable = null)
        {
            var property = CreateEntityType().AddProperty("MyProp", propertyType, shadowProperty: true);

            if (isNullable.HasValue)
            {
                property.IsNullable = isNullable;
            }

            return new SqlServerTypeMapper().MapPropertyType(property);
        }

        private static EntityType CreateEntityType() => new Model().AddEntityType("MyType");

        [Fact]
        public void Does_simple_mappings_for_sequences()
        {
            Assert.Equal("int", GetSequenceTypeMapping(typeof(int)).DefaultTypeName);
            Assert.Equal("smallint", GetSequenceTypeMapping(typeof(short)).DefaultTypeName);
            Assert.Equal("bigint", GetSequenceTypeMapping(typeof(long)).DefaultTypeName);
            Assert.Equal("tinyint", GetSequenceTypeMapping(typeof(byte)).DefaultTypeName);
        }

        private static RelationalTypeMapping GetSequenceTypeMapping(Type sequenceType)
            => new SqlServerTypeMapper().MapSequenceType(new Sequence("MySequence", type: sequenceType));

        private enum LongEnum : long
        {
        }

        private enum IntEnum
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
