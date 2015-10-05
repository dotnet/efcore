// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Internal;
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
            var typeMapping = GetTypeMapping(typeof(decimal));

            Assert.Null(typeMapping.StoreType);
            Assert.Equal("decimal(18, 2)", typeMapping.DefaultTypeName);
        }

        [Fact]
        public void Does_decimal_mapping_for_nullable_CLR_types()
        {
            var typeMapping = GetTypeMapping(typeof(decimal?));

            Assert.Null(typeMapping.StoreType);
            Assert.Equal("decimal(18, 2)", typeMapping.DefaultTypeName);
        }

        [Fact]
        public void Does_non_key_SQL_Server_string_mapping()
        {
            var typeMapping = GetTypeMapping(typeof(string));

            Assert.Null(typeMapping.StoreType);
            Assert.Equal("nvarchar(max)", typeMapping.DefaultTypeName);
            Assert.Equal(4000, typeMapping.CreateParameter(new TestCommand(), "Name", "Value").Size);
        }

        [Fact]
        public void Does_non_key_SQL_Server_string_mapping_with_max_length()
        {
            var typeMapping = GetTypeMapping(typeof(string), null, 3);

            Assert.Null(typeMapping.StoreType);
            Assert.Equal("nvarchar(3)", typeMapping.DefaultTypeName);
            Assert.Equal(4000, typeMapping.CreateParameter(new TestCommand(), "Name", "Value").Size);
        }

        [Fact]
        public void Does_non_key_SQL_Server_string_mapping_with_long_string()
        {
            var typeMapping = GetTypeMapping(typeof(string));

            Assert.Null(typeMapping.StoreType);
            Assert.Equal("nvarchar(max)", typeMapping.DefaultTypeName);
            Assert.Equal(-1, typeMapping.CreateParameter(new TestCommand(), "Name", new string('X', 4001)).Size);
        }

        [Fact]
        public void Does_non_key_SQL_Server_string_mapping_with_max_length_with_long_string()
        {
            var typeMapping = GetTypeMapping(typeof(string), null, 3);

            Assert.Null(typeMapping.StoreType);
            Assert.Equal("nvarchar(3)", typeMapping.DefaultTypeName);
            Assert.Equal(-1, typeMapping.CreateParameter(new TestCommand(), "Name", new string('X', 4001)).Size);
        }

        [Fact]
        public void Does_non_key_SQL_Server_required_string_mapping()
        {
            var typeMapping = GetTypeMapping(typeof(string), isNullable: false);

            Assert.Null(typeMapping.StoreType);
            Assert.Equal("nvarchar(max)", typeMapping.DefaultTypeName);
            Assert.Equal(4000, typeMapping.CreateParameter(new TestCommand(), "Name", "Value").Size);
        }

        [Fact]
        public void Does_key_SQL_Server_string_mapping()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(string));
            property.DeclaringEntityType.SetPrimaryKey(property);
            property.IsNullable = false;

            var typeMapping = new SqlServerTypeMapper().GetMapping(property);

            Assert.Null(typeMapping.StoreType);
            Assert.Equal("nvarchar(450)", typeMapping.DefaultTypeName);
            Assert.Equal(4000, typeMapping.CreateParameter(new TestCommand(), "Name", "Value").Size);
        }

        [Fact]
        public void Does_foreign_key_SQL_Server_string_mapping()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(string));
            var fkProperty = property.DeclaringEntityType.AddProperty("FK", typeof(string));
            var pk = property.DeclaringEntityType.SetPrimaryKey(property);
            property.DeclaringEntityType.AddForeignKey(fkProperty, pk, property.DeclaringEntityType);

            var typeMapping = new SqlServerTypeMapper().GetMapping(fkProperty);

            Assert.Null(typeMapping.StoreType);
            Assert.Equal("nvarchar(450)", typeMapping.DefaultTypeName);
            Assert.Equal(4000, typeMapping.CreateParameter(new TestCommand(), "Name", "Value").Size);
        }

        [Fact]
        public void Does_required_foreign_key_SQL_Server_string_mapping()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(string));
            var fkProperty = property.DeclaringEntityType.AddProperty("FK", typeof(string));
            var pk = property.DeclaringEntityType.SetPrimaryKey(property);
            property.DeclaringEntityType.AddForeignKey(fkProperty, pk, property.DeclaringEntityType);
            fkProperty.IsNullable = false;

            var typeMapping = new SqlServerTypeMapper().GetMapping(fkProperty);

            Assert.Null(typeMapping.StoreType);
            Assert.Equal("nvarchar(450)", typeMapping.DefaultTypeName);
            Assert.Equal(4000, typeMapping.CreateParameter(new TestCommand(), "Name", "Value").Size);
        }

        [Fact]
        public void Does_non_key_SQL_Server_binary_mapping()
        {
            var typeMapping = GetTypeMapping(typeof(byte[]));

            Assert.Equal(DbType.Binary, typeMapping.StoreType);
            Assert.Equal("varbinary(max)", typeMapping.DefaultTypeName);
            Assert.Equal(8000, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[3]).Size);
        }

        [Fact]
        public void Does_non_key_SQL_Server_binary_mapping_with_max_length()
        {
            var typeMapping = GetTypeMapping(typeof(byte[]), null, 3);

            Assert.Equal(DbType.Binary, typeMapping.StoreType);
            Assert.Equal("varbinary(3)", typeMapping.DefaultTypeName);
            Assert.Equal(8000, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[3]).Size);
        }

        [Fact]
        public void Does_non_key_SQL_Server_binary_mapping_with_long_string()
        {
            var typeMapping = GetTypeMapping(typeof(byte[]));

            Assert.Equal(DbType.Binary, typeMapping.StoreType);
            Assert.Equal("varbinary(max)", typeMapping.DefaultTypeName);
            Assert.Equal(-1, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[8001]).Size);
        }

        [Fact]
        public void Does_non_key_SQL_Server_binary_mapping_with_max_length_with_long_string()
        {
            var typeMapping = GetTypeMapping(typeof(byte[]), null, 3);

            Assert.Equal(DbType.Binary, typeMapping.StoreType);
            Assert.Equal("varbinary(3)", typeMapping.DefaultTypeName);
            Assert.Equal(-1, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[8001]).Size);
        }

        [Fact]
        public void Does_non_key_SQL_Server_required_binary_mapping()
        {
            var typeMapping = GetTypeMapping(typeof(byte[]), isNullable: false);

            Assert.Equal(DbType.Binary, typeMapping.StoreType);
            Assert.Equal("varbinary(max)", typeMapping.DefaultTypeName);
            Assert.Equal(8000, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[3]).Size);
        }

        [Fact]
        public void Does_key_SQL_Server_binary_mapping()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(byte[]));
            property.DeclaringEntityType.SetPrimaryKey(property);
            property.IsNullable = false;

            var typeMapping = new SqlServerTypeMapper().GetMapping(property);

            Assert.Equal(DbType.Binary, typeMapping.StoreType);
            Assert.Equal("varbinary(900)", typeMapping.DefaultTypeName);
            Assert.Equal(8000, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[3]).Size);
        }

        [Fact]
        public void Does_foreign_key_SQL_Server_binary_mapping()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(byte[]));
            var fkProperty = property.DeclaringEntityType.AddProperty("FK", typeof(byte[]));
            var pk = property.DeclaringEntityType.SetPrimaryKey(property);
            property.DeclaringEntityType.AddForeignKey(fkProperty, pk, property.DeclaringEntityType);

            var typeMapping = new SqlServerTypeMapper().GetMapping(fkProperty);

            Assert.Equal(DbType.Binary, typeMapping.StoreType);
            Assert.Equal("varbinary(900)", typeMapping.DefaultTypeName);
            Assert.Equal(8000, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[3]).Size);
        }

        [Fact]
        public void Does_required_foreign_key_SQL_Server_binary_mapping()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(byte[]));
            var fkProperty = property.DeclaringEntityType.AddProperty("FK", typeof(byte[]));
            var pk = property.DeclaringEntityType.SetPrimaryKey(property);
            property.DeclaringEntityType.AddForeignKey(fkProperty, pk, property.DeclaringEntityType);
            fkProperty.IsNullable = false;

            var typeMapping = new SqlServerTypeMapper().GetMapping(fkProperty);

            Assert.Equal(DbType.Binary, typeMapping.StoreType);
            Assert.Equal("varbinary(900)", typeMapping.DefaultTypeName);
            Assert.Equal(8000, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[3]).Size);
        }

        [Fact]
        public void Does_non_key_SQL_Server_rowversion_mapping()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(byte[]));
            property.IsConcurrencyToken = true;
            property.ValueGenerated = ValueGenerated.OnAddOrUpdate;

            var typeMapping = (RelationalSizedTypeMapping)new SqlServerTypeMapper().GetMapping(property);

            Assert.Equal(DbType.Binary, typeMapping.StoreType);
            Assert.Equal("rowversion", typeMapping.DefaultTypeName);
            Assert.Equal(8, typeMapping.Size);
            Assert.Equal(8, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[8]).Size);
        }

        [Fact]
        public void Does_non_key_SQL_Server_required_rowversion_mapping()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(byte[]));
            property.IsConcurrencyToken = true;
            property.ValueGenerated = ValueGenerated.OnAddOrUpdate;
            property.IsNullable = false;

            var typeMapping = (RelationalSizedTypeMapping)new SqlServerTypeMapper().GetMapping(property);

            Assert.Equal(DbType.Binary, typeMapping.StoreType);
            Assert.Equal("rowversion", typeMapping.DefaultTypeName);
            Assert.Equal(8, typeMapping.Size);
            Assert.Equal(8, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[8]).Size);
        }

        [Fact]
        public void Does_not_do_rowversion_mapping_for_non_computed_concurrency_tokens()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(byte[]));
            property.IsConcurrencyToken = true;

            var typeMapping = (SqlServerMaxLengthMapping)new SqlServerTypeMapper().GetMapping(property);

            Assert.Equal(DbType.Binary, typeMapping.StoreType);
            Assert.Equal("varbinary(max)", typeMapping.DefaultTypeName);
        }

        private static RelationalTypeMapping GetTypeMapping(Type propertyType, bool? isNullable = null, int? maxLength = null)
        {
            var property = CreateEntityType().AddProperty("MyProp", propertyType);

            if (isNullable.HasValue)
            {
                property.IsNullable = isNullable;
            }

            if (maxLength.HasValue)
            {
                property.SetMaxLength(maxLength);
            }

            return new SqlServerTypeMapper().GetMapping(property);
        }

        private static EntityType CreateEntityType() => new Model().AddEntityType("MyType");

        [Fact]
        public void Does_default_mappings_for_sequence_types()
        {
            Assert.Equal("int", new SqlServerTypeMapper().GetMapping(typeof(int)).DefaultTypeName);
            Assert.Equal("smallint", new SqlServerTypeMapper().GetMapping(typeof(short)).DefaultTypeName);
            Assert.Equal("bigint", new SqlServerTypeMapper().GetMapping(typeof(long)).DefaultTypeName);
            Assert.Equal("tinyint", new SqlServerTypeMapper().GetMapping(typeof(byte)).DefaultTypeName);
        }

        [Fact]
        public void Does_default_mappings_for_strings_and_byte_arrays()
        {
            Assert.Equal("nvarchar(max)", new SqlServerTypeMapper().GetMapping(typeof(string)).DefaultTypeName);
            Assert.Equal("varbinary(max)", new SqlServerTypeMapper().GetMapping(typeof(byte[])).DefaultTypeName);
        }

        [Fact]
        public void Does_default_mappings_for_values()
        {
            Assert.Equal("nvarchar(max)", new SqlServerTypeMapper().GetMapping("Cheese").DefaultTypeName);
            Assert.Equal("varbinary(max)", new SqlServerTypeMapper().GetMapping(new byte[1]).DefaultTypeName);
            Assert.Equal("datetime2", new SqlServerTypeMapper().GetMapping(new DateTime()).DefaultTypeName);
        }

        [Fact]
        public void Does_default_mappings_for_null_values()
        {
            Assert.Equal("NULL", new SqlServerTypeMapper().GetMapping((object)null).DefaultTypeName);
            Assert.Equal("NULL", new SqlServerTypeMapper().GetMapping(DBNull.Value).DefaultTypeName);
            Assert.Equal("NULL", RelationalTypeMapperExtensions.GetMapping(null, "Itz").DefaultTypeName);
        }

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

        private class TestParameter : DbParameter
        {
            public override void ResetDbType()
            {
            }

            public override DbType DbType { get; set; }
            public override ParameterDirection Direction { get; set; }
            public override bool IsNullable { get; set; }
            public override string ParameterName { get; set; }
            public override string SourceColumn { get; set; }
            public override DataRowVersion SourceVersion { get; set; }
            public override object Value { get; set; }
            public override bool SourceColumnNullMapping { get; set; }
            public override int Size { get; set; }
        }

        private class TestCommand : DbCommand
        {
            public override void Prepare()
            {
            }

            public override string CommandText { get; set; }
            public override int CommandTimeout { get; set; }
            public override CommandType CommandType { get; set; }
            public override UpdateRowSource UpdatedRowSource { get; set; }
            protected override DbConnection DbConnection { get; set; }
            protected override DbParameterCollection DbParameterCollection { get; }
            protected override DbTransaction DbTransaction { get; set; }
            public override bool DesignTimeVisible { get; set; }

            public override void Cancel()
            {
            }

            protected override DbParameter CreateDbParameter()
            {
                return new TestParameter();
            }

            protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
            {
                throw new NotImplementedException();
            }

            public override int ExecuteNonQuery()
            {
                throw new NotImplementedException();
            }

            public override object ExecuteScalar()
            {
                throw new NotImplementedException();
            }
        }
    }
}
