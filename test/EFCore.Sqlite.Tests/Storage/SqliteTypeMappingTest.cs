// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class SqliteTypeMappingTest : RelationalTypeMappingTest
    {
        protected override DbCommand CreateTestCommand()
            => new SqliteCommand();

        protected override DbType DefaultParameterType
            => DbType.String;

        [InlineData(typeof(SqliteCharTypeMapping), typeof(char))]
        [InlineData(typeof(SqliteDateTimeOffsetTypeMapping), typeof(DateTimeOffset))]
        [InlineData(typeof(SqliteDateTimeTypeMapping), typeof(DateTime))]
        [InlineData(typeof(SqliteDecimalTypeMapping), typeof(decimal))]
        [InlineData(typeof(SqliteGuidTypeMapping), typeof(Guid))]
        [InlineData(typeof(SqliteULongTypeMapping), typeof(ulong))]
        public override void Create_and_clone_with_converter(Type mappingType, Type clrType)
        {
            base.Create_and_clone_with_converter(mappingType, clrType);
        }

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
        public void It_maps_strings_to_not_null_types(string typeName, Type clrType)
        {
            Assert.Equal(clrType, CreateTypeMapper().FindMapping(typeName).ClrType);
        }

        private static IRelationalTypeMappingSource CreateTypeMapper()
            => TestServiceFactory.Instance.Create<SqliteTypeMappingSource>();

        public static RelationalTypeMapping GetMapping(
            Type type)
            => CreateTypeMapper().FindMapping(type);

        public override void GenerateSqlLiteral_returns_char_literal()
        {
            var literal = new SqliteCharTypeMapping("TEXT").GenerateSqlLiteral('A');
            Assert.Equal("65", literal);
        }

        public override void GenerateSqlLiteral_returns_DateTime_literal()
        {
            var value = new DateTime(2015, 3, 12, 13, 36, 37, 371);
            var literal = GetMapping(typeof(DateTime)).GenerateSqlLiteral(value);
            Assert.Equal("'2015-03-12 13:36:37.371'", literal);
        }

        public override void GenerateSqlLiteral_returns_DateTimeOffset_literal()
        {
            var value = new DateTimeOffset(2015, 3, 12, 13, 36, 37, 371, new TimeSpan(-7, 0, 0));
            var literal = GetMapping(typeof(DateTimeOffset)).GenerateSqlLiteral(value);
            Assert.Equal("'2015-03-12 13:36:37.371-07:00'", literal);
        }

        public override void GenerateSqlLiteral_returns_Guid_literal()
        {
            var value = new Guid("c6f43a9e-91e1-45ef-a320-832ea23b7292");
            var literal = GetMapping(typeof(Guid)).GenerateSqlLiteral(value);
            Assert.Equal("X'9E3AF4C6E191EF45A320832EA23B7292'", literal);
        }

        public override void GenerateSqlLiteral_for_ULong_works_for_range_limits()
        {
            var typeMapping = new SqliteULongTypeMapping("INTEGER");
            var literal = typeMapping.GenerateSqlLiteral(ulong.MinValue);
            Assert.Equal("0", literal);

            literal = typeMapping.GenerateSqlLiteral(long.MaxValue + 1ul);
            Assert.Equal("-9223372036854775808", literal);

            literal = typeMapping.GenerateSqlLiteral(ulong.MaxValue);
            Assert.Equal("-1", literal);
        }

        [Fact]
        public override void GenerateSqlLiteral_for_Decimal_works_for_range_limits()
        {
            var typeMapping = new SqliteDecimalTypeMapping("TEXT");
            var literal = typeMapping.GenerateSqlLiteral(decimal.MinValue);
            Assert.Equal("'-79228162514264337593543950335.0'", literal);

            literal = typeMapping.GenerateSqlLiteral(decimal.MaxValue);
            Assert.Equal("'79228162514264337593543950335.0'", literal);
        }

        protected override DbContextOptions ContextOptions { get; }
            = new DbContextOptionsBuilder().UseSqlite("Filename=dummmy.db").Options;
    }
}
