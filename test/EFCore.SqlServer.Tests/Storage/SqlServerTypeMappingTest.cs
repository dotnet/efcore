// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class SqlServerTypeMappingTest : RelationalTypeMappingTest
    {
        protected override DbCommand CreateTestCommand()
            => new SqlCommand();

        protected override DbType DefaultParameterType
            => DbType.Int32;

        [InlineData(typeof(SqlServerDateTimeOffsetTypeMapping), typeof(DateTimeOffset))]
        [InlineData(typeof(SqlServerDateTimeTypeMapping), typeof(DateTime))]
        [InlineData(typeof(SqlServerDoubleTypeMapping), typeof(double))]
        [InlineData(typeof(SqlServerFloatTypeMapping), typeof(float))]
        [InlineData(typeof(SqlServerTimeSpanTypeMapping), typeof(TimeSpan))]
        public override void Create_and_clone_with_converter(Type mappingType, Type clrType)
        {
            base.Create_and_clone_with_converter(mappingType, clrType);
        }

        [InlineData(typeof(SqlServerByteArrayTypeMapping), typeof(byte[]))]
        public override void Create_and_clone_sized_mappings_with_converter(Type mappingType, Type clrType)
        {
            base.Create_and_clone_sized_mappings_with_converter(mappingType, clrType);
        }

        [InlineData(typeof(SqlServerStringTypeMapping), typeof(string))]
        public override void Create_and_clone_unicode_sized_mappings_with_converter(Type mappingType, Type clrType)
        {
            base.Create_and_clone_unicode_sized_mappings_with_converter(mappingType, clrType);
        }

        [Fact]
        public virtual void Create_and_clone_UDT_mapping_with_converter()
        {
            var mapping = new SqlServerUdtTypeMapping(
                "storeType",
                typeof(object),
                "udtType",
                new FakeValueConverter(),
                DbType.VarNumeric,
                false,
                33);

            var clone = (SqlServerUdtTypeMapping)mapping.Clone("<clone>", 66);

            Assert.NotSame(mapping, clone);
            Assert.Same(mapping.GetType(), clone.GetType());
            Assert.Equal("storeType", mapping.StoreType);
            Assert.Equal("<clone>", clone.StoreType);
            Assert.Equal("udtType", mapping.UdtTypeName);
            Assert.Equal("udtType", clone.UdtTypeName);
            Assert.Equal(DbType.VarNumeric, clone.DbType);
            Assert.Equal(33, mapping.Size);
            Assert.Equal(66, clone.Size);
            Assert.False(mapping.IsUnicode);
            Assert.False(clone.IsUnicode);
            Assert.NotNull(mapping.Converter);
            Assert.Same(mapping.Converter, clone.Converter);
            Assert.Same(typeof(object), clone.ClrType);

            var newConverter = new FakeValueConverter();
            clone = (SqlServerUdtTypeMapping)mapping.Clone(newConverter);

            Assert.NotSame(mapping, clone);
            Assert.Same(mapping.GetType(), clone.GetType());
            Assert.Equal("storeType", mapping.StoreType);
            Assert.Equal("storeType", clone.StoreType);
            Assert.Equal("udtType", mapping.UdtTypeName);
            Assert.Equal("udtType", clone.UdtTypeName);
            Assert.Equal(DbType.VarNumeric, clone.DbType);
            Assert.Equal(33, mapping.Size);
            Assert.Equal(33, clone.Size);
            Assert.False(mapping.IsUnicode);
            Assert.False(clone.IsUnicode);
            Assert.NotSame(mapping.Converter, clone.Converter);
            Assert.Same(typeof(object), clone.ClrType);
        }

        public override void GenerateSqlLiteral_returns_ByteArray_literal()
        {
            var value = new byte[] { 0xDA, 0x7A };
            var literal = new SqlServerTypeMapper(new CoreTypeMapperDependencies(), new RelationalTypeMapperDependencies())
                .GetMapping(typeof(byte[])).GenerateSqlLiteral(value);
            Assert.Equal("0xDA7A", literal);
        }

        public override void GenerateSqlLiteral_returns_DateTime_literal()
        {
            var value = new DateTime(2015, 3, 12, 13, 36, 37, 371);
            var literal = new SqlServerTypeMapper(new CoreTypeMapperDependencies(), new RelationalTypeMapperDependencies())
                .GetMapping(typeof(DateTime)).GenerateSqlLiteral(value);

            Assert.Equal("'2015-03-12T13:36:37.371'", literal);
        }

        public override void GenerateSqlLiteral_returns_DateTimeOffset_literal()
        {
            var value = new DateTimeOffset(2015, 3, 12, 13, 36, 37, 371, new TimeSpan(-7, 0, 0));
            var literal = new SqlServerTypeMapper(new CoreTypeMapperDependencies(), new RelationalTypeMapperDependencies())
                .GetMapping(typeof(DateTimeOffset)).GenerateSqlLiteral(value);

            Assert.Equal("'2015-03-12T13:36:37.371-07:00'", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteralValue_returns_Unicode_String_literal()
        {
            var literal = new SqlServerTypeMapper(new CoreTypeMapperDependencies(), new RelationalTypeMapperDependencies())
                .GetMapping("nvarchar(max)").GenerateSqlLiteral("A Unicode String");
            Assert.Equal("N'A Unicode String'", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteralValue_returns_NonUnicode_String_literal()
        {
            var literal = new SqlServerTypeMapper(new CoreTypeMapperDependencies(), new RelationalTypeMapperDependencies())
                .GetMapping("varchar(max)").GenerateSqlLiteral("A Non-Unicode String");
            Assert.Equal("'A Non-Unicode String'", literal);
        }

        [Theory]
        [InlineData("Microsoft.SqlServer.Types.SqlHierarchyId", "hierarchyid")]
        [InlineData("Microsoft.SqlServer.Types.SqlGeography", "geography")]
        [InlineData("Microsoft.SqlServer.Types.SqlGeometry", "geometry")]
        public virtual void Get_named_mappings_for_sql_type(string typeName, string udtName)
        {
            var mappings = new TestSqlServerTypeMapper(new CoreTypeMapperDependencies(), new RelationalTypeMapperDependencies())
                .GetClrTypeNameMappings();

            var mapping = mappings[typeName](typeof(Random));

            Assert.Equal(udtName, mapping.StoreType);
            Assert.Equal(udtName, ((SqlServerUdtTypeMapping)mapping).UdtTypeName);
            Assert.Same(typeof(Random), mapping.ClrType);
        }

        private class TestSqlServerTypeMapper : SqlServerTypeMapper
        {
            public TestSqlServerTypeMapper(
                CoreTypeMapperDependencies coreDependencies,
                RelationalTypeMapperDependencies dependencies)
                : base(coreDependencies, dependencies)
            {
            }

            public new IReadOnlyDictionary<string, Func<Type, RelationalTypeMapping>> GetClrTypeNameMappings()
                => base.GetClrTypeNameMappings();
        }

        protected override DbContextOptions ContextOptions { get; }
            = new DbContextOptionsBuilder().UseSqlServer("Server=Dummy").Options;
    }
}
