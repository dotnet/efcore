// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.Redis.FunctionalTests
{
    public class DataTypesTest : IClassFixture<DataTypesFixture>
    {
        private readonly DbContext _context;

        public DataTypesTest(DataTypesFixture fixture)
        {
            _context = fixture.CreateContext();
        }

        [Fact]
        public void Can_insert_and_read_back_all_data_types_with_nullable_types_set_to_null()
        {
            var allDataTypes = _context.Set<AllDataTypes>().Add(
                new AllDataTypes
                {
                    TestInt32 = -123456789,
                    TestNullableInt32 = null,
                    TestString = "TestString",
                    TestInt64 = -1234567890123456789L,
                    TestNullableInt64 = null,
                    TestDouble = -1.23456789,
                    TestNullableDouble = null,
                    TestDecimal= -1234567890123456789.01234567890123456789M,
                    TestNullableDecimal = null,
                    TestDateTime = new DateTime(123456789L),
                    TestNullableDateTime = null,
                    TestDateTimeOffset = new DateTimeOffset(987654321L, TimeSpan.FromHours(-8.0)),
                    TestNullableDateTimeOffset = null,
                    TestSingle = -1.234F,
                    TestNullableSingle = null,
                    TestBoolean = true,
                    TestNullableBoolean = null,
                    TestByte = 255,
                    TestNullableByte = null,
                    TestUnsignedInt32 = 1234565789U,
                    TestNullableUnsignedInt32 = null,
                    TestUnsignedInt64 = 1234565789UL,
                    TestNullableUnsignedInt64 = null,
                    TestInt16 = -1234,
                    TestNullableInt16 = null,
                    TestUnsignedInt16 = 1234,
                    TestNullableUnsignedInt16 = null,
                    TestCharacter = 'a',
                    TestNullableCharacter = null,
                    TestSignedByte = -128,
                    TestNullableSignedByte = null,
                });

            var changes = _context.SaveChanges();
            Assert.Equal(1, changes);

            var adt1 = _context.Set<AllDataTypes>().Where(adt => adt.TestInt32 == -123456789).Single();

            Assert.Equal(-123456789, adt1.TestInt32);
            Assert.Null(adt1.TestNullableInt32);
            Assert.Equal("TestString", adt1.TestString);
            Assert.Equal(-1234567890123456789L, adt1.TestInt64);
            Assert.Null(adt1.TestNullableInt64);
            Assert.Equal(-1.23456789, adt1.TestDouble);
            Assert.Null(adt1.TestNullableDouble);
            Assert.Equal(-1234567890123456789.01234567890123456789M, adt1.TestDecimal);
            Assert.Null(adt1.TestNullableDecimal);
            Assert.Equal(new DateTime(123456789L), adt1.TestDateTime);
            Assert.Null(adt1.TestNullableDateTime);
            Assert.Equal(new DateTimeOffset(987654321L, TimeSpan.FromHours(-8.0)), adt1.TestDateTimeOffset);
            Assert.Null(adt1.TestNullableDateTimeOffset);
            Assert.Equal(-1.234F, adt1.TestSingle);
            Assert.Null(adt1.TestNullableSingle);
            Assert.Equal(true, adt1.TestBoolean);
            Assert.Null(adt1.TestNullableBoolean);
            Assert.Equal(255, adt1.TestByte);
            Assert.Null(adt1.TestNullableByte);
            Assert.Equal(1234565789U, adt1.TestUnsignedInt32);
            Assert.Null(adt1.TestNullableUnsignedInt32);
            Assert.Equal(1234565789UL, adt1.TestUnsignedInt64);
            Assert.Null(adt1.TestNullableUnsignedInt64);
            Assert.Equal(-1234, adt1.TestInt16);
            Assert.Null(adt1.TestNullableInt16);
            Assert.Equal(1234, adt1.TestUnsignedInt16);
            Assert.Null(adt1.TestNullableUnsignedInt16);
            Assert.Equal('a', adt1.TestCharacter);
            Assert.Null(adt1.TestNullableCharacter);
            Assert.Equal(-128, adt1.TestSignedByte);
            Assert.Null(adt1.TestNullableSignedByte);
        }

        [Fact]
        public void Can_insert_and_read_back_all_data_types_with_nullable_types_set_to_values()
        {
            var allDataTypes = _context.Set<AllDataTypes>().Add(
                new AllDataTypes
                {
                    TestInt32 = -123456789,
                    TestNullableInt32 = -123456789,
                    TestString = "TestString",
                    TestInt64 = -1234567890123456789L,
                    TestNullableInt64 = -1234567890123456789L,
                    TestDouble = -1.23456789,
                    TestNullableDouble = -1.23456789,
                    TestDecimal = -1234567890123456789.01234567890123456789M,
                    TestNullableDecimal = -1234567890123456789.01234567890123456789M,
                    TestDateTime = new DateTime(123456789L),
                    TestNullableDateTime = new DateTime(123456789L),
                    TestDateTimeOffset = new DateTimeOffset(987654321L, TimeSpan.FromHours(-8.0)),
                    TestNullableDateTimeOffset = new DateTimeOffset(987654321L, TimeSpan.FromHours(-8.0)),
                    TestSingle = -1.234F,
                    TestNullableSingle = -1.234F,
                    TestBoolean = false,
                    TestNullableBoolean = false,
                    TestByte = 255,
                    TestNullableByte = 255,
                    TestUnsignedInt32 = 1234565789U,
                    TestNullableUnsignedInt32 = 1234565789U,
                    TestUnsignedInt64 = 1234565789UL,
                    TestNullableUnsignedInt64 = 1234565789UL,
                    TestInt16 = -1234,
                    TestNullableInt16 = -1234,
                    TestUnsignedInt16 = 1234,
                    TestNullableUnsignedInt16 = 1234,
                    TestCharacter = 'a',
                    TestNullableCharacter = 'a',
                    TestSignedByte = -128,
                    TestNullableSignedByte = -128,
                });

            var changes = _context.SaveChanges();
            Assert.Equal(1, changes);

            var adt1 = _context.Set<AllDataTypes>().Where(adt => adt.TestInt32 == -123456789).Single();

            Assert.Equal(-123456789, adt1.TestInt32);
            Assert.Equal(-123456789, adt1.TestNullableInt32);
            Assert.Equal("TestString", adt1.TestString);
            Assert.Equal(-1234567890123456789L, adt1.TestInt64);
            Assert.Equal(-1234567890123456789L, adt1.TestNullableInt64);
            Assert.Equal(-1.23456789, adt1.TestDouble);
            Assert.Equal(-1.23456789, adt1.TestNullableDouble);
            Assert.Equal(-1234567890123456789.01234567890123456789M, adt1.TestDecimal);
            Assert.Equal(-1234567890123456789.01234567890123456789M, adt1.TestNullableDecimal);
            Assert.Equal(new DateTime(123456789L), adt1.TestDateTime);
            Assert.Equal(new DateTime(123456789L), adt1.TestNullableDateTime);
            Assert.Equal(new DateTimeOffset(987654321L, TimeSpan.FromHours(-8.0)), adt1.TestDateTimeOffset);
            Assert.Equal(new DateTimeOffset(987654321L, TimeSpan.FromHours(-8.0)), adt1.TestNullableDateTimeOffset);
            Assert.Equal(-1.234F, adt1.TestSingle);
            Assert.Equal(-1.234F, adt1.TestNullableSingle);
            Assert.Equal(false, adt1.TestBoolean);
            Assert.Equal(false, adt1.TestNullableBoolean);
            Assert.Equal(255, adt1.TestByte);
            Assert.Equal((byte)255, adt1.TestNullableByte);
            Assert.Equal(1234565789U, adt1.TestUnsignedInt32);
            Assert.Equal(1234565789U, adt1.TestNullableUnsignedInt32);
            Assert.Equal(1234565789UL, adt1.TestUnsignedInt64);
            Assert.Equal(1234565789UL, adt1.TestNullableUnsignedInt64);
            Assert.Equal(-1234, adt1.TestInt16);
            Assert.Equal((short)-1234, adt1.TestNullableInt16);
            Assert.Equal(1234, adt1.TestUnsignedInt16);
            Assert.Equal((ushort)1234, adt1.TestNullableUnsignedInt16);
            Assert.Equal('a', adt1.TestCharacter);
            Assert.Equal('a', adt1.TestNullableCharacter);
            Assert.Equal(-128, adt1.TestSignedByte);
            Assert.Equal((sbyte)-128, adt1.TestNullableSignedByte);
        }
    }
}
