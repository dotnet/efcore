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
        [InlineData("", typeof(string))]
        public void It_maps_strings_to_not_null_types(string typeName, Type clrType)
        {
            Assert.Equal(clrType, CreateTypeMapper().FindMapping(typeName).ClrType);
        }

        private static IRelationalTypeMappingSource CreateTypeMapper()
            => TestServiceFactory.Instance.Create<SqliteTypeMappingSource>();

        public static RelationalTypeMapping GetMapping(
            Type type)
            => CreateTypeMapper().FindMapping(type);

        public override void Char_literal_generated_correctly()
        {
            Test_GenerateSqlLiteral_helper(new SqliteCharTypeMapping("TEXT"), 'A', "65");
        }

        public override void DateTimeOffset_literal_generated_correctly()
        {
            Test_GenerateSqlLiteral_helper(
                GetMapping(typeof(DateTimeOffset)),
                new DateTimeOffset(2015, 3, 12, 13, 36, 37, 371, new TimeSpan(-7, 0, 0)),
                "'2015-03-12 13:36:37.371-07:00'");
        }

        public override void DateTime_literal_generated_correctly()
        {
            Test_GenerateSqlLiteral_helper(
                GetMapping(typeof(DateTime)),
                new DateTime(2015, 3, 12, 13, 36, 37, 371, DateTimeKind.Utc),
                "'2015-03-12 13:36:37.371'");
        }

        public override void Decimal_literal_generated_correctly()
        {
            var typeMapping = new SqliteDecimalTypeMapping("TEXT");

            Test_GenerateSqlLiteral_helper(typeMapping, decimal.MinValue, "'-79228162514264337593543950335.0'");
            Test_GenerateSqlLiteral_helper(typeMapping, decimal.MaxValue, "'79228162514264337593543950335.0'");
        }

        public override void Guid_literal_generated_correctly()
        {
            Test_GenerateSqlLiteral_helper(
                GetMapping(typeof(Guid)),
                new Guid("c6f43a9e-91e1-45ef-a320-832ea23b7292"),
                "X'9E3AF4C6E191EF45A320832EA23B7292'");
        }

        public override void ULong_literal_generated_correctly()
        {
            var typeMapping = new SqliteULongTypeMapping("INTEGER");

            Test_GenerateSqlLiteral_helper(typeMapping, ulong.MinValue, "0");
            Test_GenerateSqlLiteral_helper(typeMapping, ulong.MaxValue, "-1");
            Test_GenerateSqlLiteral_helper(typeMapping, long.MaxValue + 1ul, "-9223372036854775808");
        }

        protected override DbContextOptions ContextOptions { get; }
            = new DbContextOptionsBuilder().UseSqlite("Filename=dummmy.db").Options;
    }
}
