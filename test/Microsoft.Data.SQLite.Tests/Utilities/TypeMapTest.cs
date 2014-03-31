// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.Data.SQLite.Utilities
{
    public class TypeMapTest
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
            var map = TypeMap.FromClrType(value);

            Assert.Equal(SQLiteType.Integer, map.SQLiteType);
            Assert.Equal(1L, map.ToInterop(value));
        }

        [Theory]
        [InlineData(3.14)]
        [InlineData(3.14f)]
        public void FromClrType_maps_floats(object value)
        {
            var map = TypeMap.FromClrType(value);

            Assert.Equal(SQLiteType.Float, map.SQLiteType);
            Assert.Equal(3.14, (double)map.ToInterop(value), precision: 6);
        }

        [Fact]
        public void FromClrType_maps_string_to_text()
        {
            var value = "test";

            var map = TypeMap.FromClrType(value);

            Assert.Equal(SQLiteType.Text, map.SQLiteType);
            Assert.Equal("test", map.ToInterop(value));
        }

        [Fact]
        public void FromClrType_maps_byteArray_to_blob()
        {
            var value = new byte[] { 0x7e, 0x57 };

            var map = TypeMap.FromClrType(value);

            Assert.Equal(SQLiteType.Blob, map.SQLiteType);
            Assert.Equal(new byte[] { 0x7e, 0x57 }, map.ToInterop(value));
        }

        [Fact]
        public void FromClrType_maps_dbNull_to_null()
        {
            var value = DBNull.Value;

            var map = TypeMap.FromClrType(value);

            Assert.Equal(SQLiteType.Null, map.SQLiteType);
            Assert.Equal(DBNull.Value, map.ToInterop(value));
        }

        [Fact]
        public void FromClrType_throws_when_unknown()
        {
            var ex = Assert.Throws<ArgumentException>(() => TypeMap.FromClrType(this));
            Assert.Equal(Strings.FormatUnknownDataType(GetType()), ex.Message);
        }

        [Fact]
        public void ToInterop_converts_to_string_when_DateTime()
        {
            var value = new DateTime(2014, 3, 19, 14, 18, 58, 213);

            var map = TypeMap.FromClrType(value);

            Assert.Equal(SQLiteType.Text, map.SQLiteType);
            Assert.Equal("2014-03-19T14:18:58.2130000", map.ToInterop(value));
        }

        [Fact]
        public void ToInterop_converts_to_string_when_DateTimeOffset()
        {
            var value = new DateTimeOffset(2014, 3, 19, 14, 18, 58, 213, new TimeSpan(-7, 0, 0));

            var map = TypeMap.FromClrType(value);

            Assert.Equal(SQLiteType.Text, map.SQLiteType);
            Assert.Equal("2014-03-19T14:18:58.2130000-07:00", map.ToInterop(value));
        }

        [Fact]
        public void ToInterop_converts_to_string_when_decimal()
        {
            var value = 3.14m;

            var map = TypeMap.FromClrType(value);

            Assert.Equal(SQLiteType.Text, map.SQLiteType);
            Assert.Equal("3.14", map.ToInterop(value));
        }

        [Fact]
        public void ToInterop_converts_to_blob_when_Giud()
        {
            var value = new Guid("36127aab-3769-45b5-8804-f2d447dc001a");

            var map = TypeMap.FromClrType(value);

            Assert.Equal(SQLiteType.Blob, map.SQLiteType);
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

            var map = TypeMap.FromClrType(value);

            Assert.Equal(SQLiteType.Text, map.SQLiteType);
            Assert.Equal("19.14:18:58.2130000", map.ToInterop(value));
        }

        [Fact]
        public void ToInterop_overflows_when_ulong()
        {
            var value = 0xFFFFFFFFFFFFFFFF;

            var map = TypeMap.FromClrType(value);

            Assert.Equal(-1L, map.ToInterop(value));
        }
    }
}
