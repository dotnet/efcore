// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    /// <summary>
    ///     See also <see cref="SupplementalBuiltInDataTypesTestBase" />.
    ///     Not all built-in data types are supported on all providers yet.
    ///     At the same time, not all conventions (e.g. Ignore) are available yet.
    ///     So this class provides a base test class for those data types which are
    ///     supported on all current providers.
    ///     Over time, the aim is to transfer as many tests as possible into
    ///     this class and ultimately to delete <see cref="SupplementalBuiltInDataTypesTestBase" />.
    /// </summary>
    public abstract class BuiltInDataTypesTestBase
    {
        protected BuiltInDataTypesFixtureBase _fixture;

        [Fact]
        public virtual void Can_insert_and_read_back_all_non_nullable_data_types()
        {
            using (var context = _fixture.CreateContext())
            {
                Test_insert_and_read_back_all_non_nullable_data_types(context);
            }
        }

        public void Test_insert_and_read_back_all_non_nullable_data_types(DbContext context)
        {
            var allDataTypes = context.Set<BuiltInNonNullableDataTypes>().Add(
                new BuiltInNonNullableDataTypes
                    {
                        Id0 = 0,
                        Id1 = 0,
                        TestInt32 = -123456789,
                        TestInt64 = -1234567890123456789L,
                        TestDouble = -1.23456789,
                        // TODO: SQL Server default precision is 18 (use max precision, 38,
                        // when available but no way to define that in the model at the moment)
                        TestDecimal = -1234567890.012345678M,
                        TestDateTime = DateTime.Parse("01/01/2000 12:34:56"),
                        TestDateTimeOffset = new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                        TestSingle = -1.234F,
                        TestBoolean = true,
                        TestByte = 255,
                        TestUnsignedInt32 = 1234565789U,
                        TestUnsignedInt64 = 1234565789UL,
                        TestInt16 = -1234,
                    });

            var changes = context.SaveChanges();
            Assert.Equal(1, changes);

            var dt = context.Set<BuiltInNonNullableDataTypes>().Where(nndt => nndt.Id0 == 0).Single();

            Assert.Equal(-123456789, dt.TestInt32);
            Assert.Equal(-1234567890123456789L, dt.TestInt64);
            Assert.Equal(-1.23456789, dt.TestDouble);
            Assert.Equal(-1234567890.012345678M, dt.TestDecimal);
            Assert.Equal(DateTime.Parse("01/01/2000 12:34:56"), dt.TestDateTime);
            Assert.Equal(new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)), dt.TestDateTimeOffset);
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
            using (var context = _fixture.CreateContext())
            {
                Test_insert_and_read_back_all_nullable_data_types_with_values_set_to_null(context);
            }
        }

        public void Test_insert_and_read_back_all_nullable_data_types_with_values_set_to_null(DbContext context)
        {
            var allDataTypes = context.Set<BuiltInNullableDataTypes>().Add(
                new BuiltInNullableDataTypes
                    {
                        Id0 = 100,
                        Id1 = 100,
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

            var changes = context.SaveChanges();
            Assert.Equal(1, changes);

            var dt = context.Set<BuiltInNullableDataTypes>().Where(ndt => ndt.Id0 == 100).Single();

            Assert.Null(dt.TestNullableInt32);
            Assert.Null(dt.TestString);
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
            using (var context = _fixture.CreateContext())
            {
                Test_insert_and_read_back_all_nullable_data_types_with_values_set_to_non_null(context);
            }
        }

        public void Test_insert_and_read_back_all_nullable_data_types_with_values_set_to_non_null(DbContext context)
        {
            var allDataTypes = context.Set<BuiltInNullableDataTypes>().Add(
                new BuiltInNullableDataTypes
                    {
                        Id0 = 101,
                        Id1 = 101,
                        TestNullableInt32 = -123456789,
                        TestString = "TestString",
                        TestNullableInt64 = -1234567890123456789L,
                        TestNullableDouble = -1.23456789,
                        // TODO: SQL Server default precision is 18 (use max precision, 38, 
                        // when available but no way to define that in the model at the moment)
                        TestNullableDecimal = -1234567890.012345678M,
                        TestNullableDateTime = DateTime.Parse("01/01/2000 12:34:56"),
                        TestNullableDateTimeOffset = new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                        TestNullableSingle = -1.234F,
                        TestNullableBoolean = false,
                        TestNullableByte = 255,
                        TestNullableInt16 = -1234,
                    });

            var changes = context.SaveChanges();
            Assert.Equal(1, changes);

            var dt = context.Set<BuiltInNullableDataTypes>().Where(ndt => ndt.Id0 == 101).Single();

            Assert.Equal(-123456789, dt.TestNullableInt32);
            Assert.Equal("TestString", dt.TestString);
            Assert.Equal(-1234567890123456789L, dt.TestNullableInt64);
            Assert.Equal(-1.23456789, dt.TestNullableDouble);
            Assert.Equal(-1234567890.012345678M, dt.TestNullableDecimal);
            Assert.Equal(DateTime.Parse("01/01/2000 12:34:56"), dt.TestNullableDateTime);
            Assert.Equal(new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)), dt.TestNullableDateTimeOffset);
            Assert.Equal(-1.234F, dt.TestNullableSingle);
            Assert.Equal(false, dt.TestNullableBoolean);
            Assert.Equal((byte)255, dt.TestNullableByte);
            Assert.Equal((short)-1234, dt.TestNullableInt16);
        }
    }
}
