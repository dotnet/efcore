// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class BuiltInDataTypesTestBase<TTestStore, TFixture> : IClassFixture<TFixture>, IDisposable
        where TTestStore : TestStore
        where TFixture : BuiltInDataTypesFixtureBase<TTestStore>, new()
    {
        [Fact]
        public virtual void Can_insert_and_read_back_all_non_nullable_data_types()
        {
            using (var context = CreateContext())
            {
                context.Set<BuiltInNonNullableDataTypes>().Add(
                    new BuiltInNonNullableDataTypes
                        {
                            Id = 1,
                            PartitionId = 1,
                            TestInt16 = -1234,
                            TestInt32 = -123456789,
                            TestInt64 = -1234567890123456789L,
                            TestDouble = -1.23456789,
                            // TODO: SQL Server default precision is 18 (use max precision, 38,
                            // when available but no way to define that in the model at the moment)
                            TestDecimal = -1234567890.01M,
                            TestDateTime = DateTime.Parse("01/01/2000 12:34:56"),
                            TestDateTimeOffset = new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                            TestSingle = -1.234F,
                            TestBoolean = true,
                            TestByte = 255,
                            TestUnsignedInt16 = 1234,
                            TestUnsignedInt32 = 1234565789U,
                            TestUnsignedInt64 = 1234567890123456789UL,
                            TestCharacter = 'a',
                            TestSignedByte = -128
                        });

                var changes = context.SaveChanges();
                Assert.Equal(1, changes);
            }

            using (var context = CreateContext())
            {
                var dt = context.Set<BuiltInNonNullableDataTypes>().Single(nndt => nndt.Id == 1);

                var entityType = context.Model.GetEntityType(typeof(BuiltInNonNullableDataTypes));
                Assert.Equal(-1234, dt.TestInt16);
                Assert.Equal(-123456789, dt.TestInt32);
                Assert.Equal(-1234567890123456789L, dt.TestInt64);
                Assert.Equal(-1.23456789, dt.TestDouble);
                Assert.Equal(-1234567890.01M, dt.TestDecimal);
                Assert.Equal(DateTime.Parse("01/01/2000 12:34:56"), dt.TestDateTime);
                Assert.Equal(new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)), dt.TestDateTimeOffset);
                Assert.Equal(-1.234F, dt.TestSingle);
                Assert.Equal(true, dt.TestBoolean);
                Assert.Equal(255, dt.TestByte);
                if (entityType.FindProperty("TestUnsignedInt16") != null)
                {
                    Assert.Equal(1234, dt.TestUnsignedInt16);
                }
                if (entityType.FindProperty("TestUnsignedInt32") != null)
                {
                    Assert.Equal(1234565789U, dt.TestUnsignedInt32);
                }
                if (entityType.FindProperty("TestUnsignedInt64") != null)
                {
                    Assert.Equal(1234567890123456789UL, dt.TestUnsignedInt64);
                }
                if (entityType.FindProperty("TestCharacter") != null)
                {
                    Assert.Equal('a', dt.TestCharacter);
                }
                if (entityType.FindProperty("TestSignedByte") != null)
                {
                    Assert.Equal(-128, dt.TestSignedByte);
                }
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_nullable_data_types_with_values_set_to_null()
        {
            using (var context = CreateContext())
            {
                context.Set<BuiltInNullableDataTypes>().Add(
                    new BuiltInNullableDataTypes
                        {
                            Id = 100,
                            PartitionId = 100,
                            TestString = null,
                            TestNullableInt16 = null,
                            TestNullableInt32 = null,
                            TestNullableInt64 = null,
                            TestNullableDouble = null,
                            TestNullableDecimal = null,
                            TestNullableDateTime = null,
                            TestNullableDateTimeOffset = null,
                            TestNullableSingle = null,
                            TestNullableBoolean = null,
                            TestNullableByte = null,
                            TestNullableUnsignedInt16 = null,
                            TestNullableUnsignedInt32 = null,
                            TestNullableUnsignedInt64 = null,
                            TestNullableCharacter = null,
                            TestNullableSignedByte = null
                        });

                var changes = context.SaveChanges();
                Assert.Equal(1, changes);
            }

            using (var context = CreateContext())
            {
                var dt = context.Set<BuiltInNullableDataTypes>().Single(ndt => ndt.Id == 100);

                Assert.Null(dt.TestString);
                Assert.Null(dt.TestNullableInt16);
                Assert.Null(dt.TestNullableInt32);
                Assert.Null(dt.TestNullableInt64);
                Assert.Null(dt.TestNullableDouble);
                Assert.Null(dt.TestNullableDecimal);
                Assert.Null(dt.TestNullableDateTime);
                Assert.Null(dt.TestNullableDateTimeOffset);
                Assert.Null(dt.TestNullableSingle);
                Assert.Null(dt.TestNullableBoolean);
                Assert.Null(dt.TestNullableByte);
                Assert.Null(dt.TestNullableUnsignedInt16);
                Assert.Null(dt.TestNullableUnsignedInt32);
                Assert.Null(dt.TestNullableUnsignedInt64);
                Assert.Null(dt.TestNullableCharacter);
                Assert.Null(dt.TestNullableSignedByte);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_nullable_data_types_with_values_set_to_non_null()
        {
            using (var context = CreateContext())
            {
                context.Set<BuiltInNullableDataTypes>().Add(
                    new BuiltInNullableDataTypes
                        {
                            Id = 101,
                            PartitionId = 101,
                            TestString = "TestString",
                            TestNullableInt16 = -1234,
                            TestNullableInt32 = -123456789,
                            TestNullableInt64 = -1234567890123456789L,
                            TestNullableDouble = -1.23456789,
                            // TODO: SQL Server default precision is 18 (use max precision, 38, 
                            // when available but no way to define that in the model at the moment)
                            TestNullableDecimal = -1234567890.01M,
                            TestNullableDateTime = DateTime.Parse("01/01/2000 12:34:56"),
                            TestNullableDateTimeOffset = new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                            TestNullableSingle = -1.234F,
                            TestNullableBoolean = false,
                            TestNullableByte = 255,
                            TestNullableUnsignedInt16 = 1234,
                            TestNullableUnsignedInt32 = 1234565789U,
                            TestNullableUnsignedInt64 = 1234567890123456789UL,
                            TestNullableCharacter = 'a',
                            TestNullableSignedByte = -128
                        });

                var changes = context.SaveChanges();
                Assert.Equal(1, changes);
            }

            using (var context = CreateContext())
            {
                var dt = context.Set<BuiltInNullableDataTypes>().Single(ndt => ndt.Id == 101);

                var entityType = context.Model.GetEntityType(typeof(BuiltInNonNullableDataTypes));
                Assert.Equal("TestString", dt.TestString);
                Assert.Equal((short)-1234, dt.TestNullableInt16);
                Assert.Equal(-123456789, dt.TestNullableInt32);
                Assert.Equal(-1234567890123456789L, dt.TestNullableInt64);
                Assert.Equal(-1.23456789, dt.TestNullableDouble);
                Assert.Equal(-1234567890.01M, dt.TestNullableDecimal);
                Assert.Equal(DateTime.Parse("01/01/2000 12:34:56"), dt.TestNullableDateTime);
                Assert.Equal(new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)), dt.TestNullableDateTimeOffset);
                Assert.Equal(-1.234F, dt.TestNullableSingle);
                Assert.Equal(false, dt.TestNullableBoolean);
                Assert.Equal((byte)255, dt.TestNullableByte);
                if (entityType.FindProperty("TestNullableUnsignedInt16") != null)
                {
                    Assert.Equal((ushort)1234, dt.TestNullableUnsignedInt16);
                }
                if (entityType.FindProperty("TestNullableUnsignedInt32") != null)
                {
                    Assert.Equal(1234565789U, dt.TestNullableUnsignedInt32);
                }
                if (entityType.FindProperty("TestNullableUnsignedInt64") != null)
                {
                    Assert.Equal(1234567890123456789UL, dt.TestNullableUnsignedInt64);
                }
                if (entityType.FindProperty("TestNullableCharacter") != null)
                {
                    Assert.Equal('a', dt.TestNullableCharacter);
                }
                if (entityType.FindProperty("TestNullableSignedByte") != null)
                {
                    Assert.Equal((sbyte)-128, dt.TestNullableSignedByte);
                }
            }
        }

        protected BuiltInDataTypesTestBase(TFixture fixture)
        {
            Fixture = fixture;

            TestStore = Fixture.CreateTestStore();
        }

        protected TFixture Fixture { get; }

        protected TTestStore TestStore { get; }

        public void Dispose()
        {
            TestStore.Dispose();
        }

        protected DbContext CreateContext()
        {
            return Fixture.CreateContext(TestStore);
        }
    }
}
