// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class SqliteTypeMappingTest : RelationalTypeMappingTest
    {
        protected override DbCommand CreateTestCommand()
            => new SqliteCommand();

        protected override DbType DefaultParameterType
            => DbType.String;

        [Theory]
        [InlineData("TEXT", typeof(string))]
        [InlineData("Integer", typeof(long))]
        [InlineData("Blob", typeof(byte[]))]
        [InlineData("numeric", typeof(string))]
        [InlineData("real", typeof(double))]
        [InlineData("doub", typeof(double))]
        [InlineData("int", typeof(long))]
        [InlineData("SMALLINT", typeof(long))]
        [InlineData("UNSIGNED BIG INT", typeof(long))]
        [InlineData("VARCHAR(255)", typeof(string))]
        [InlineData("nchar(55)", typeof(string))]
        [InlineData("datetime", typeof(string))]
        [InlineData("decimal(10,4)", typeof(string))]
        [InlineData("boolean", typeof(string))]
        [InlineData("unknown_type", typeof(string))]
        [InlineData("", typeof(string))]
        public void It_maps_strings_to_not_null_types(string typeName, Type clrType)
        {
            Assert.Equal(clrType, new SqliteTypeMapper(new RelationalTypeMapperDependencies()).GetMapping(typeName).ClrType);
        }

        [Fact]
        public override void GenerateSqlLiteral_returns_DateTime_literal()
        {
            var value = new DateTime(2015, 3, 12, 13, 36, 37, 371);
            var literal = new SqliteTypeMapper(new RelationalTypeMapperDependencies())
                .GetMapping(typeof(DateTime)).GenerateSqlLiteral(value);
            Assert.Equal("'2015-03-12 13:36:37.371'", literal);
        }

        [Fact]
        public override void GenerateSqlLiteral_returns_DateTimeOffset_literal()
        {
            var value = new DateTimeOffset(2015, 3, 12, 13, 36, 37, 371, new TimeSpan(-7, 0, 0));
            var literal = new SqliteTypeMapper(new RelationalTypeMapperDependencies())
                .GetMapping(typeof(DateTimeOffset)).GenerateSqlLiteral(value);
            Assert.Equal("'2015-03-12 13:36:37.371-07:00'", literal);
        }

        [Fact]
        public override void GenerateSqlLiteral_returns_Guid_literal()
        {
            var value = new Guid("c6f43a9e-91e1-45ef-a320-832ea23b7292");
            var literal = new SqliteTypeMapper(new RelationalTypeMapperDependencies())
                .GetMapping(typeof(Guid)).GenerateSqlLiteral(value);
            Assert.Equal("X'9E3AF4C6E191EF45A320832EA23B7292'", literal);
        }
    }
}
