// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using Xunit;

namespace Microsoft.Data.Sqlite.Utilities
{
    public class SqliteTypeMapTest
    {
        [Theory]
        [InlineData(true)]
        [InlineData((byte)1)]
        [InlineData((sbyte)1)]
        [InlineData(1)]
        [InlineData(1u)]
        [InlineData(1L)]
        [InlineData(1ul)]
        [InlineData((short)1)]
        [InlineData((ushort)1)]
        public void FromClrType_maps_integers(object value)
        {
            var map = SqliteTypeMap.FromClrType(value.GetType());

            Assert.Equal(value.GetType(), map.ClrType);
            Assert.Equal(SqliteType.Integer, map.SqliteType);
            Assert.Equal(1L, map.ToInterop(value));
            Assert.Equal(value, map.FromInterop(1L));
        }

        [Theory]
        [InlineData(3.14)]
        [InlineData(3.14f)]
        public void FromClrType_maps_floats(object value)
        {
            var map = SqliteTypeMap.FromClrType(value.GetType());

            Assert.Equal(SqliteType.Float, map.SqliteType);
            Assert.Equal(3.14, (double)map.ToInterop(value), precision: 6);
            Assert.Equal(value, map.FromInterop(3.14));
        }

        [Fact]
        public void FromClrType_maps_string_to_text()
        {
            var value = "test";

            var map = SqliteTypeMap.FromClrType<string>();

            Assert.Equal(typeof(string), map.ClrType);
            Assert.Equal(SqliteType.Text, map.SqliteType);
            Assert.Equal("test", map.ToInterop(value));
            Assert.Equal(value, map.FromInterop("test"));
        }

        [Fact]
        public void FromClrType_maps_byteArray_to_blob()
        {
            var value = new byte[] { 0x7e, 0x57 };

            var map = SqliteTypeMap.FromClrType<byte[]>();

            Assert.Equal(typeof(byte[]), map.ClrType);
            Assert.Equal(SqliteType.Blob, map.SqliteType);
            Assert.Equal(new byte[] { 0x7e, 0x57 }, map.ToInterop(value));
            Assert.Equal(value, map.FromInterop(new byte[] { 0x7e, 0x57 }));
        }

        [Fact]
        public void FromClrType_maps_dbNull_to_null()
        {
            var value = DBNull.Value;

            var map = SqliteTypeMap.FromClrType<DBNull>();

            Assert.Equal(typeof(DBNull), map.ClrType);
            Assert.Equal(SqliteType.Null, map.SqliteType);
            Assert.Equal(DBNull.Value, map.ToInterop(value));
            Assert.Equal(value, map.FromInterop(DBNull.Value));
        }

        [Fact]
        public void FromClrType_throws_when_unknown()
        {
            var ex = Assert.Throws<ArgumentException>(() => SqliteTypeMap.FromClrType(GetType()));
            Assert.Equal(Strings.FormatUnknownDataType(GetType()), ex.Message);
        }

        [Fact]
        public void FromClrType_handles_nullable_types()
        {
            Assert.Equal(typeof(int), SqliteTypeMap.FromClrType(typeof(int?)).ClrType);
        }

        [Fact]
        public void FromClrType_handles_enum_types()
        {
            Assert.Equal(typeof(int), SqliteTypeMap.FromClrType(typeof(StringComparison)).ClrType);
        }

        [Theory]
        [InlineData("BIT", SqliteType.Integer, typeof(bool), DbType.Boolean)]
        [InlineData("BLOB", SqliteType.Blob, typeof(byte[]), DbType.Binary)]
        [InlineData("CHAR", SqliteType.Text, typeof(string), DbType.String)]
        [InlineData("DATETIME", SqliteType.Text, typeof(DateTime), DbType.DateTime)]
        [InlineData("DATETIMEOFFSET", SqliteType.Text, typeof(DateTimeOffset), DbType.DateTimeOffset)]
        [InlineData("DECIMAL", SqliteType.Text, typeof(decimal), DbType.Decimal)]
        [InlineData("FLOAT", SqliteType.Float, typeof(double), DbType.Double)]
        [InlineData("INT", SqliteType.Integer, typeof(int), DbType.Int32)]
        [InlineData("INT8", SqliteType.Integer, typeof(sbyte), DbType.SByte)]
        [InlineData("INTEGER", SqliteType.Integer, typeof(long), DbType.Int64)]
        [InlineData("INTERVAL", SqliteType.Text, typeof(TimeSpan), DbType.Time)]
        [InlineData("NCHAR", SqliteType.Text, typeof(string), DbType.String)]
        [InlineData("NVARCHAR", SqliteType.Text, typeof(string), DbType.String)]
        [InlineData("REAL", SqliteType.Float, typeof(double), DbType.Double)]
        [InlineData("SINGLE", SqliteType.Float, typeof(float), DbType.Single)]
        [InlineData("SMALLINT", SqliteType.Integer, typeof(short), DbType.Int16)]
        [InlineData("TINYINT", SqliteType.Integer, typeof(byte), DbType.Byte)]
        [InlineData("UINT", SqliteType.Integer, typeof(uint), DbType.UInt32)]
        [InlineData("UINT16", SqliteType.Integer, typeof(ushort), DbType.UInt16)]
        [InlineData("ULONG", SqliteType.Integer, typeof(ulong), DbType.UInt64)]
        [InlineData("UNIQUEIDENTIFIER", SqliteType.Blob, typeof(Guid), DbType.Guid)]
        [InlineData("VARCHAR", SqliteType.Text, typeof(string), DbType.String)]
        public void FromDeclaredType_maps_types(string declaredType, int sqliteType, Type clrType, DbType dbType)
        {
            var map = SqliteTypeMap.FromDeclaredType(declaredType, (SqliteType)sqliteType);

            Assert.Equal(clrType, map.ClrType);
            Assert.Equal(dbType, map.DbType);
        }

        [Fact]
        public void FromDeclaredType_ignores_facets()
        {
            var map = SqliteTypeMap.FromDeclaredType("NVARCHAR(4000)", SqliteType.Text);

            Assert.Equal(typeof(string), map.ClrType);
        }

        [Fact]
        public void FromDeclaredType_ignores_case()
        {
            var map = SqliteTypeMap.FromDeclaredType("int", SqliteType.Integer);

            Assert.Equal(typeof(int), map.ClrType);
        }

        [Fact]
        public void FromDeclaredType_ignores_sqlitetype()
        {
            var map = SqliteTypeMap.FromDeclaredType("INTEGER", SqliteType.Text);

            Assert.Equal(typeof(long), map.ClrType);
        }

        [Fact]
        public void FromDeclaredType_falls_back_using_sqlitetype()
        {
            var map = SqliteTypeMap.FromDeclaredType("UNKNOWN", SqliteType.Integer);

            Assert.Equal(typeof(long), map.ClrType);
        }

        [Theory]
        [InlineData(SqliteType.Null, typeof(DBNull))]
        [InlineData(SqliteType.Integer, typeof(long))]
        [InlineData(SqliteType.Float, typeof(double))]
        [InlineData(SqliteType.Text, typeof(string))]
        [InlineData(SqliteType.Blob, typeof(byte[]))]
        public void FromSqliteType_maps_types(int sqliteType, Type clrType)
        {
            var map = SqliteTypeMap.FromSqliteType((SqliteType)sqliteType);

            Assert.Equal(clrType, map.ClrType);
        }

        [Fact]
        public void ToInterop_converts_to_string_when_DateTime()
        {
            var value = new DateTime(2014, 3, 19, 14, 18, 58, 213);

            var map = SqliteTypeMap.FromClrType<DateTime>();

            Assert.Equal(SqliteType.Text, map.SqliteType);
            Assert.Equal("2014-03-19T14:18:58.2130000", map.ToInterop(value));
        }

        [Fact]
        public void ToInterop_converts_to_string_when_DateTimeOffset()
        {
            var value = new DateTimeOffset(2014, 3, 19, 14, 18, 58, 213, new TimeSpan(-7, 0, 0));

            var map = SqliteTypeMap.FromClrType<DateTimeOffset>();

            Assert.Equal(SqliteType.Text, map.SqliteType);
            Assert.Equal("2014-03-19T14:18:58.2130000-07:00", map.ToInterop(value));
        }

        [Fact]
        public void ToInterop_converts_to_string_when_decimal()
        {
            var value = 3.14m;

            var map = SqliteTypeMap.FromClrType<decimal>();

            Assert.Equal(SqliteType.Text, map.SqliteType);
            Assert.Equal("3.14", map.ToInterop(value));
        }

        [Fact]
        public void ToInterop_converts_to_blob_when_Giud()
        {
            var value = new Guid("36127aab-3769-45b5-8804-f2d447dc001a");

            var map = SqliteTypeMap.FromClrType<Guid>();

            Assert.Equal(SqliteType.Blob, map.SqliteType);
            Assert.Equal(
                new byte[]
                    {
                        0xab, 0x7a, 0x12, 0x36,
                        0x69, 0x37,
                        0xb5, 0x45,
                        0x88, 0x04,
                        0xf2, 0xd4, 0x47, 0xdc, 0x00, 0x1a
                    },
                map.ToInterop(value));
        }

        [Fact]
        public void ToInterop_converts_to_string_when_TimeSpan()
        {
            var value = new TimeSpan(19, 14, 18, 58, 213);

            var map = SqliteTypeMap.FromClrType<TimeSpan>();

            Assert.Equal(SqliteType.Text, map.SqliteType);
            Assert.Equal("19.14:18:58.2130000", map.ToInterop(value));
        }

        [Fact]
        public void ToInterop_overflows_when_ulong()
        {
            var value = 0xFFFFFFFFFFFFFFFF;

            var map = SqliteTypeMap.FromClrType<ulong>();

            Assert.Equal(-1L, map.ToInterop(value));
        }

        [Fact]
        public void FromInterop_converts_to_string_when_DateTime()
        {
            var value = new DateTime(2014, 3, 19, 14, 18, 58);

            var map = SqliteTypeMap.FromClrType<DateTime>();

            Assert.Equal(value, map.FromInterop("2014-03-19 14:18:58"));
        }

        [Fact]
        public void FromInterop_converts_to_string_when_DateTimeOffset()
        {
            var value = new DateTimeOffset(2014, 3, 19, 14, 18, 58, 213, new TimeSpan(-7, 0, 0));

            var map = SqliteTypeMap.FromClrType<DateTimeOffset>();

            Assert.Equal(value, map.FromInterop("2014-03-19T14:18:58.2130000-07:00"));
        }

        [Fact]
        public void FromInterop_converts_to_string_when_decimal()
        {
            var value = 3.14m;

            var map = SqliteTypeMap.FromClrType<decimal>();

            Assert.Equal(value, map.FromInterop("3.14"));
        }

        [Fact]
        public void FromInterop_converts_to_blob_when_Giud()
        {
            var value = new Guid("36127aab-3769-45b5-8804-f2d447dc001a");

            var map = SqliteTypeMap.FromClrType<Guid>();

            Assert.Equal(
                value,
                map.FromInterop(
                    new byte[]
                        {
                            0xab, 0x7a, 0x12, 0x36,
                            0x69, 0x37,
                            0xb5, 0x45,
                            0x88, 0x04,
                            0xf2, 0xd4, 0x47, 0xdc, 0x00, 0x1a
                        }));
        }

        [Fact]
        public void FromInterop_converts_to_string_when_TimeSpan()
        {
            var value = new TimeSpan(14, 18, 58);

            var map = SqliteTypeMap.FromClrType<TimeSpan>();

            Assert.Equal(SqliteType.Text, map.SqliteType);
            Assert.Equal(value, map.FromInterop("14:18:58"));
        }

        [Fact]
        public void FromInterop_overflows_when_ulong()
        {
            var value = 0xFFFFFFFFFFFFFFFF;

            var map = SqliteTypeMap.FromClrType<ulong>();

            Assert.Equal(value, map.FromInterop(-1L));
        }
    }
}
