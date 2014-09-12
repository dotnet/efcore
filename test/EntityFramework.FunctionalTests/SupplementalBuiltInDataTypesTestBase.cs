// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    /// <summary>
    /// See also <see cref="BuiltInDataTypesTestBase" />.
    /// Not all built-in data types are supported on all providers yet.
    /// At the same time, not all conventions (e.g. Ignore) are available yet.
    /// So this class provides a base test class for those data types which are
    /// only supported on some providers.
    /// Over time, the aim is to transfer as many tests as possible into
    /// BuiltInDataTypesTestBase and ultimately to delete this class.
    /// </summary>
    public abstract class SupplementalBuiltInDataTypesTestBase
    {
        protected DbContext _context;

        [Fact]
        public virtual void Can_insert_and_read_back_all_supplemental_non_nullable_data_types()
        {
            var allDataTypes = _context.Set<SupplementalBuiltInNonNullableDataTypes>().Add(
                new SupplementalBuiltInNonNullableDataTypes
                {
                    Id = 0,
                    TestUnsignedInt32 = 1234565789U,
                    TestUnsignedInt64 = 1234567890123456789UL,
                    TestUnsignedInt16 = 1234,
                    TestCharacter = 'a',
                    TestSignedByte = -128,
                });

            var changes = _context.SaveChanges();
            Assert.Equal(1, changes);

            var dt = _context.Set<SupplementalBuiltInNonNullableDataTypes>().Where(nndt => nndt.Id == 0).Single();

            Assert.Equal(1234565789U, dt.TestUnsignedInt32);
            Assert.Equal(1234567890123456789UL, dt.TestUnsignedInt64);
            Assert.Equal(1234, dt.TestUnsignedInt16);
            Assert.Equal('a', dt.TestCharacter);
            Assert.Equal(-128, dt.TestSignedByte);
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_supplemental_nullable_data_types_with_values_set_to_null()
        {
            var allDataTypes = _context.Set<SupplementalBuiltInNullableDataTypes>().Add(
                new SupplementalBuiltInNullableDataTypes
                {
                    Id = 0,
                    TestNullableUnsignedInt32 = null,
                    TestNullableUnsignedInt64 = null,
                    TestNullableInt16 = null,
                    TestNullableUnsignedInt16 = null,
                    TestNullableCharacter = null,
                    TestNullableSignedByte = null,
                });

            var changes = _context.SaveChanges();
            Assert.Equal(1, changes);

            var dt = _context.Set<SupplementalBuiltInNullableDataTypes>().Where(ndt => ndt.Id == 0).Single();

            Assert.Null(dt.TestNullableUnsignedInt32);
            Assert.Null(dt.TestNullableUnsignedInt64);
            Assert.Null(dt.TestNullableInt16);
            Assert.Null(dt.TestNullableUnsignedInt16);
            Assert.Null(dt.TestNullableCharacter);
            Assert.Null(dt.TestNullableSignedByte);
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_supplemental_nullable_data_types_with_values_set_to_non_null()
        {
            var allDataTypes = _context.Set<SupplementalBuiltInNullableDataTypes>().Add(
                new SupplementalBuiltInNullableDataTypes
                {
                    Id = 1,
                    TestNullableUnsignedInt32 = 1234565789U,
                    TestNullableUnsignedInt64 = 1234567890123456789UL,
                    TestNullableInt16 = -1234,
                    TestNullableUnsignedInt16 = 1234,
                    TestNullableCharacter = 'a',
                    TestNullableSignedByte = -128,
                });

            var changes = _context.SaveChanges();
            Assert.Equal(1, changes);

            var dt = _context.Set<SupplementalBuiltInNullableDataTypes>().Where(ndt => ndt.Id == 1).Single();

            Assert.Equal(1234565789U, dt.TestNullableUnsignedInt32);
            Assert.Equal(1234567890123456789UL, dt.TestNullableUnsignedInt64);
            Assert.Equal((short)-1234, dt.TestNullableInt16);
            Assert.Equal((ushort)1234, dt.TestNullableUnsignedInt16);
            Assert.Equal('a', dt.TestNullableCharacter);
            Assert.Equal((sbyte)-128, dt.TestNullableSignedByte);
        }
    }
}

