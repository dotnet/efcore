// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public abstract class RelationalTypeMappingTest
    {
        [Fact]
        public void Can_create_simple_parameter()
        {
            var parameter = new RelationalTypeMapping("int", typeof(int))
                .CreateParameter(CreateTestCommand(), "Name", 17, nullable: false);

            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal("Name", parameter.ParameterName);
            Assert.Equal(17, parameter.Value);
            Assert.Equal(DefaultParameterType, parameter.DbType);
            Assert.False(parameter.IsNullable);
        }

        [Fact]
        public void Can_create_simple_nullable_parameter()
        {
            var parameter = new RelationalTypeMapping("int", typeof(int))
                .CreateParameter(CreateTestCommand(), "Name", 17, nullable: true);

            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal("Name", parameter.ParameterName);
            Assert.Equal(17, parameter.Value);
            Assert.Equal(DefaultParameterType, parameter.DbType);
            Assert.True(parameter.IsNullable);
        }

        [Fact]
        public void Can_create_simple_parameter_with_DbType()
        {
            var parameter = new RelationalTypeMapping("int", typeof(int), DbType.Int32)
                .CreateParameter(CreateTestCommand(), "Name", 17, nullable: false);

            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal("Name", parameter.ParameterName);
            Assert.Equal(17, parameter.Value);
            Assert.Equal(DbType.Int32, parameter.DbType);
            Assert.False(parameter.IsNullable);
        }

        [Fact]
        public void Can_create_simple_nullable_parameter_with_DbType()
        {
            var parameter = new RelationalTypeMapping("int", typeof(int), DbType.Int32)
                .CreateParameter(CreateTestCommand(), "Name", 17, nullable: true);

            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal("Name", parameter.ParameterName);
            Assert.Equal(17, parameter.Value);
            Assert.Equal(DbType.Int32, parameter.DbType);
            Assert.True(parameter.IsNullable);
        }

        [Fact]
        public void Can_create_required_string_parameter()
        {
            var parameter = new RelationalTypeMapping("nvarchar(23)", typeof(string), DbType.String, unicode: true, size: 23)
                .CreateParameter(CreateTestCommand(), "Name", "Value", nullable: false);

            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal("Name", parameter.ParameterName);
            Assert.Equal("Value", parameter.Value);
            Assert.Equal(DbType.String, parameter.DbType);
            Assert.False(parameter.IsNullable);
            Assert.Equal(23, parameter.Size);
        }

        [Fact]
        public void Can_create_string_parameter()
        {
            var parameter = new RelationalTypeMapping("nvarchar(23)", typeof(string), DbType.String, unicode: true, size: 23)
                .CreateParameter(CreateTestCommand(), "Name", "Value", nullable: true);

            Assert.Equal(ParameterDirection.Input, parameter.Direction);
            Assert.Equal("Name", parameter.ParameterName);
            Assert.Equal("Value", parameter.Value);
            Assert.Equal(DbType.String, parameter.DbType);
            Assert.True(parameter.IsNullable);
            Assert.Equal(23, parameter.Size);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_bool_literal_when_true()
        {
            var literal = new RelationalTypeMapping("bool", typeof(bool)).GenerateSqlLiteral(true);
            Assert.Equal("1", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_bool_literal_when_false()
        {
            var literal = new RelationalTypeMapping("bool", typeof(bool)).GenerateSqlLiteral(false);
            Assert.Equal("0", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_char_literal()
        {
            var literal = new RelationalTypeMapping("char", typeof(char)).GenerateSqlLiteral('A');
            Assert.Equal("'A'", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_ByteArray_literal()
        {
            var literal = new RelationalTypeMapping("byte[]", typeof(byte[])).GenerateSqlLiteral(new byte[] { 0xDA, 0x7A });
            Assert.Equal("X'DA7A'", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_object_literal_when_null()
        {
            var literal = new RelationalTypeMapping("object", typeof(object)).GenerateSqlLiteral(default(object));
            Assert.Equal("NULL", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_object_literal_when_not_null()
        {
            var literal = new RelationalTypeMapping("object", typeof(object)).GenerateSqlLiteral(42);
            Assert.Equal("42", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_Guid_literal()
        {
            var value = new Guid("c6f43a9e-91e1-45ef-a320-832ea23b7292");
            var literal = new RelationalTypeMapping("guid", typeof(Guid)).GenerateSqlLiteral(value);
            Assert.Equal("'c6f43a9e-91e1-45ef-a320-832ea23b7292'", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_DateTime_literal()
        {
            var value = new DateTime(2015, 3, 12, 13, 36, 37, 371);
            var literal = new RelationalTypeMapping("DateTime", typeof(DateTime)).GenerateSqlLiteral(value);
            Assert.Equal("TIMESTAMP '2015-03-12 13:36:37.3710000'", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_DateTimeOffset_literal()
        {
            var value = new DateTimeOffset(2015, 3, 12, 13, 36, 37, 371, new TimeSpan(-7, 0, 0));
            var literal = new RelationalTypeMapping("DateTimeOffset", typeof(DateTimeOffset)).GenerateSqlLiteral(value);
            Assert.Equal("TIMESTAMP '2015-03-12 13:36:37.3710000-07:00'", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_NullableInt_literal_when_null()
        {
            var literal = new RelationalTypeMapping("int?", typeof(int?)).GenerateSqlLiteral(default(int?));
            Assert.Equal("NULL", literal);
        }

        [Fact]
        public virtual void GenerateSqlLiteral_returns_NullableInt_literal_when_not_null()
        {
            var literal = new RelationalTypeMapping("char?", typeof(char?)).GenerateSqlLiteral((char?)'A');
            Assert.Equal("'A'", literal);
        }

        protected abstract DbCommand CreateTestCommand();

        protected abstract DbType DefaultParameterType { get; }
    }
}
