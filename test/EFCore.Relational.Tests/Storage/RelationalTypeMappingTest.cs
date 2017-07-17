// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public abstract class RelationalTypeMappingTest
    {
        [Fact]
        public void Can_create_simple_parameter()
        {
            using (var command = CreateTestCommand())
            {
                var parameter = new IntTypeMapping("int")
                    .CreateParameter(command, "Name", 17, nullable: false);

                Assert.Equal(ParameterDirection.Input, parameter.Direction);
                Assert.Equal("Name", parameter.ParameterName);
                Assert.Equal(17, parameter.Value);
                Assert.Equal(DefaultParameterType, parameter.DbType);
                Assert.False(parameter.IsNullable);
            }
        }

        [Fact]
        public void Can_create_simple_nullable_parameter()
        {
            using (var command = CreateTestCommand())
            {
                var parameter = new IntTypeMapping("int")
                    .CreateParameter(command, "Name", 17, nullable: true);

                Assert.Equal(ParameterDirection.Input, parameter.Direction);
                Assert.Equal("Name", parameter.ParameterName);
                Assert.Equal(17, parameter.Value);
                Assert.Equal(DefaultParameterType, parameter.DbType);
                Assert.True(parameter.IsNullable);
            }
        }

        [Fact]
        public void Can_create_simple_parameter_with_DbType()
        {
            using (var command = CreateTestCommand())
            {
                var parameter = new IntTypeMapping("int", DbType.Int32)
                    .CreateParameter(command, "Name", 17, nullable: false);

                Assert.Equal(ParameterDirection.Input, parameter.Direction);
                Assert.Equal("Name", parameter.ParameterName);
                Assert.Equal(17, parameter.Value);
                Assert.Equal(DbType.Int32, parameter.DbType);
                Assert.False(parameter.IsNullable);
            }
        }

        [Fact]
        public void Can_create_simple_nullable_parameter_with_DbType()
        {
            using (var command = CreateTestCommand())
            {
                var parameter = new IntTypeMapping("int", DbType.Int32)
                    .CreateParameter(command, "Name", 17, nullable: true);

                Assert.Equal(ParameterDirection.Input, parameter.Direction);
                Assert.Equal("Name", parameter.ParameterName);
                Assert.Equal(17, parameter.Value);
                Assert.Equal(DbType.Int32, parameter.DbType);
                Assert.True(parameter.IsNullable);
            }
        }

        [Fact]
        public void Can_create_required_string_parameter()
        {
            using (var command = CreateTestCommand())
            {
                var parameter = new StringTypeMapping("nvarchar(23)", DbType.String, unicode: true, size: 23)
                    .CreateParameter(command, "Name", "Value", nullable: false);

                Assert.Equal(ParameterDirection.Input, parameter.Direction);
                Assert.Equal("Name", parameter.ParameterName);
                Assert.Equal("Value", parameter.Value);
                Assert.Equal(DbType.String, parameter.DbType);
                Assert.False(parameter.IsNullable);
                Assert.Equal(23, parameter.Size);
            }
        }

        [Fact]
        public void Can_create_string_parameter()
        {
            using (var command = CreateTestCommand())
            {
                var parameter = new StringTypeMapping("nvarchar(23)", DbType.String, unicode: true, size: 23)
                    .CreateParameter(command, "Name", "Value", nullable: true);

                Assert.Equal(ParameterDirection.Input, parameter.Direction);
                Assert.Equal("Name", parameter.ParameterName);
                Assert.Equal("Value", parameter.Value);
                Assert.Equal(DbType.String, parameter.DbType);
                Assert.True(parameter.IsNullable);
                Assert.Equal(23, parameter.Size);
            }
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_bool_literal_when_true()
        {
            var literal = new BoolTypeMapping("bool").GenerateSqlLiteral(true);
            Assert.Equal("1", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_bool_literal_when_false()
        {
            var literal = new BoolTypeMapping("bool").GenerateSqlLiteral(false);
            Assert.Equal("0", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_char_literal()
        {
            var literal = new CharTypeMapping("char").GenerateSqlLiteral('A');
            Assert.Equal("'A'", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_ByteArray_literal()
        {
            var literal = new ByteArrayTypeMapping("byte[]").GenerateSqlLiteral(new byte[] { 0xDA, 0x7A });
            Assert.Equal("X'DA7A'", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_Guid_literal()
        {
            var value = new Guid("c6f43a9e-91e1-45ef-a320-832ea23b7292");
            var literal = new GuidTypeMapping("guid").GenerateSqlLiteral(value);
            Assert.Equal("'c6f43a9e-91e1-45ef-a320-832ea23b7292'", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_DateTime_literal()
        {
            var value = new DateTime(2015, 3, 12, 13, 36, 37, 371);
            var literal = new DateTimeTypeMapping("DateTime").GenerateSqlLiteral(value);
            Assert.Equal("TIMESTAMP '2015-03-12 13:36:37.3710000'", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_DateTimeOffset_literal()
        {
            var value = new DateTimeOffset(2015, 3, 12, 13, 36, 37, 371, new TimeSpan(-7, 0, 0));
            var literal = new DateTimeOffsetTypeMapping("DateTimeOffset").GenerateSqlLiteral(value);
            Assert.Equal("TIMESTAMP '2015-03-12 13:36:37.3710000-07:00'", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_NullableInt_literal_when_null()
        {
            var literal = new IntTypeMapping("int?", DbType.Int32).GenerateSqlLiteral(default(int?));
            Assert.Equal("NULL", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_NullableInt_literal_when_not_null()
        {
            var literal = new IntTypeMapping("int?", DbType.Int32).GenerateSqlLiteral((int?)123);
            Assert.Equal("123", literal);
        }
        [Fact]
        public virtual void GenerateSqlLiteral_for_Byte_works_for_range_limits()
        {
            var typeMapping = new ByteTypeMapping("byte", DbType.Byte);
            var literal = typeMapping.GenerateSqlLiteral(byte.MinValue);
            Assert.Equal("0", literal);

            literal = typeMapping.GenerateSqlLiteral(byte.MaxValue);
            Assert.Equal("255", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_for_SByte_works_for_range_limits()
        {
            var typeMapping = new SByteTypeMapping("sbyte", DbType.SByte);
            var literal = typeMapping.GenerateSqlLiteral(sbyte.MinValue);
            Assert.Equal("-128", literal);

            literal = typeMapping.GenerateSqlLiteral(sbyte.MaxValue);
            Assert.Equal("127", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_for_Short_works_for_range_limits()
        {
            var typeMapping = new ShortTypeMapping("short", DbType.Int16);
            var literal = typeMapping.GenerateSqlLiteral(short.MinValue);
            Assert.Equal("-32768", literal);

            literal = typeMapping.GenerateSqlLiteral(short.MaxValue);
            Assert.Equal("32767", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_for_UShort_works_for_range_limits()
        {
            var typeMapping = new UShortTypeMapping("ushort", DbType.UInt16);
            var literal = typeMapping.GenerateSqlLiteral(ushort.MinValue);
            Assert.Equal("0", literal);

            literal = typeMapping.GenerateSqlLiteral(ushort.MaxValue);
            Assert.Equal("65535", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_for_Int_works_for_range_limits()
        {
            var typeMapping = new IntTypeMapping("int", DbType.Int32);
            var literal = typeMapping.GenerateSqlLiteral(int.MinValue);
            Assert.Equal("-2147483648", literal);

            literal = typeMapping.GenerateSqlLiteral(int.MaxValue);
            Assert.Equal("2147483647", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_for_UInt_works_for_range_limits()
        {
            var typeMapping = new UIntTypeMapping("uint", DbType.UInt32);
            var literal = typeMapping.GenerateSqlLiteral(uint.MinValue);
            Assert.Equal("0", literal);

            literal = typeMapping.GenerateSqlLiteral(uint.MaxValue);
            Assert.Equal("4294967295", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_for_Long_works_for_range_limits()
        {
            var typeMapping = new LongTypeMapping("long", DbType.Int64);
            var literal = typeMapping.GenerateSqlLiteral(long.MinValue);
            Assert.Equal("-9223372036854775808", literal);

            literal = typeMapping.GenerateSqlLiteral(long.MaxValue);
            Assert.Equal("9223372036854775807", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_for_ULong_works_for_range_limits()
        {
            var typeMapping = new ULongTypeMapping("ulong", DbType.UInt64);
            var literal = typeMapping.GenerateSqlLiteral(ulong.MinValue);
            Assert.Equal("0", literal);

            literal = typeMapping.GenerateSqlLiteral(ulong.MaxValue);
            Assert.Equal("18446744073709551615", literal);
        }

        protected abstract DbCommand CreateTestCommand();

        protected abstract DbType DefaultParameterType { get; }
    }
}
