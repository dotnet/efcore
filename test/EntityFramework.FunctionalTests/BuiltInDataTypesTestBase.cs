// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    /// <summary>
    /// See also <see cref="SupplementalBuiltInDataTypesTestBase" />.
    /// Not all built-in data types are supported on all providers yet.
    /// At the same time, not all conventions (e.g. Ignore) are available yet.
    /// So this class provides a base test class for those data types which are
    /// supported on all current providers.
    /// Over time, the aim is to transfer as many tests as possible into
    /// this class and ultimately to delete <see cref="SupplementalBuiltInDataTypesTestBase" />.
    /// </summary>
    public abstract class BuiltInDataTypesTestBase
    {
        protected DbContext _context;

        [Fact]
        public virtual void Can_insert_and_read_back_all_non_nullable_data_types()
        {
            var allDataTypes = _context.Set<BuiltInNonNullableDataTypes>().Add(
                new BuiltInNonNullableDataTypes
                {
                    Id = 0,
                    TestInt32 = -123456789,
                    TestInt64 = -1234567890123456789L,
                    TestDouble = -1.23456789,
                    // TODO: SQL Server default precision is 18 (use max precision, 38, 
                    // when available but no way to define that in the model at the moment)
                    TestDecimal = -1234567890.012345678M,
                    TestDateTime = new DateTime(123456789L),
                    TestDateTimeOffset = new DateTimeOffset(987654321L, TimeSpan.FromHours(-8.0)),
                    TestSingle = -1.234F,
                    TestBoolean = true,
                    TestByte = 255,
                    TestUnsignedInt32 = 1234565789U,
                    TestUnsignedInt64 = 1234565789UL,
                    TestInt16 = -1234,
                });

            var changes = _context.SaveChanges();
            Assert.Equal(1, changes);

            var dt = _context.Set<BuiltInNonNullableDataTypes>().Where(nndt => nndt.Id == 0).Single();

            Assert.Equal(-123456789, dt.TestInt32);
            Assert.Equal(-1234567890123456789L, dt.TestInt64);
            Assert.Equal(-1.23456789, dt.TestDouble);
            Assert.Equal(-1234567890.012345678M, dt.TestDecimal);
            Assert.Equal(new DateTime(123456789L), dt.TestDateTime);
            Assert.Equal(new DateTimeOffset(987654321L, TimeSpan.FromHours(-8.0)), dt.TestDateTimeOffset);
            Assert.Equal(-1.234F, dt.TestSingle);
            Assert.Equal(true, dt.TestBoolean);
            Assert.Equal(255, dt.TestByte);
            Assert.Equal(1234565789U, dt.TestUnsignedInt32);
            Assert.Equal(1234565789UL, dt.TestUnsignedInt64);
            Assert.Equal(-1234, dt.TestInt16);
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_nullable_data_types_with_values_set_to_null()
        {
            var allDataTypes = _context.Set<BuiltInNullableDataTypes>().Add(
                new BuiltInNullableDataTypes
                {
                    Id = 0,
                    TestNullableInt32 = null,
                    TestString = null,
                    TestNullableInt64 = null,
                    TestNullableDouble = null,
                    TestNullableDecimal = null,
                    TestNullableDateTime = null,
                    TestNullableDateTimeOffset = null,
                    TestNullableSingle = null,
                    TestNullableBoolean = null,
                    TestNullableByte = null,
                    TestNullableInt16 = null,
                });

            var changes = _context.SaveChanges();
            Assert.Equal(1, changes);

            var dt = _context.Set<BuiltInNullableDataTypes>().Where(ndt => ndt.Id == 0).Single();

            Assert.Null(dt.TestNullableInt32);
            Assert.Null(dt.TestNullableInt64);
            Assert.Null(dt.TestNullableDouble);
            Assert.Null(dt.TestNullableDecimal);
            Assert.Null(dt.TestNullableDateTime);
            Assert.Null(dt.TestNullableDateTimeOffset);
            Assert.Null(dt.TestNullableSingle);
            Assert.Null(dt.TestNullableBoolean);
            Assert.Null(dt.TestNullableByte);
            Assert.Null(dt.TestNullableInt16);
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_nullable_data_types_with_values_set_to_non_null()
        {
            var allDataTypes = _context.Set<BuiltInNullableDataTypes>().Add(
                new BuiltInNullableDataTypes
                {
                    Id = 1,
                    TestNullableInt32 = -123456789,
                    TestString = "TestString",
                    TestNullableInt64 = -1234567890123456789L,
                    TestNullableDouble = -1.23456789,
                    // TODO: SQL Server default precision is 18 (use max precision, 38, 
                    // when available but no way to define that in the model at the moment)
                    TestNullableDecimal = -1234567890.012345678M,
                    TestNullableDateTime = new DateTime(123456789L),
                    TestNullableDateTimeOffset = new DateTimeOffset(987654321L, TimeSpan.FromHours(-8.0)),
                    TestNullableSingle = -1.234F,
                    TestNullableBoolean = false,
                    TestNullableByte = 255,
                    TestNullableInt16 = -1234,
                });

            var changes = _context.SaveChanges();
            Assert.Equal(1, changes);

            var dt = _context.Set<BuiltInNullableDataTypes>().Where(ndt => ndt.Id == 1).Single();

            Assert.Equal(-123456789, dt.TestNullableInt32);
            Assert.Equal("TestString", dt.TestString);
            Assert.Equal(-1234567890123456789L, dt.TestNullableInt64);
            Assert.Equal(-1.23456789, dt.TestNullableDouble);
            Assert.Equal(-1234567890.012345678M, dt.TestNullableDecimal);
            Assert.Equal(new DateTime(123456789L), dt.TestNullableDateTime);
            Assert.Equal(new DateTimeOffset(987654321L, TimeSpan.FromHours(-8.0)), dt.TestNullableDateTimeOffset);
            Assert.Equal(-1.234F, dt.TestNullableSingle);
            Assert.Equal(false, dt.TestNullableBoolean);
            Assert.Equal((byte)255, dt.TestNullableByte);
            Assert.Equal((short)-1234, dt.TestNullableInt16);
        }
    }
}

