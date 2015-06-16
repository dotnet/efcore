// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class BuiltInDataTypesTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : BuiltInDataTypesFixtureBase, new()
    {
        [Fact]
        public virtual void Can_insert_and_read_back_all_non_nullable_data_types()
        {
            using (var context = CreateContext())
            {
                context.Set<BuiltInDataTypes>().Add(
                    new BuiltInDataTypes
                        {
                            Id = 1,
                            PartitionId = 1,
                            TestInt16 = -1234,
                            TestInt32 = -123456789,
                            TestInt64 = -1234567890123456789L,
                            TestDouble = -1.23456789,
                            TestDecimal = -1234567890.01M,
                            TestDateTime = DateTime.Parse("01/01/2000 12:34:56"),
                            TestDateTimeOffset = new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                            TestTimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                            TestSingle = -1.234F,
                            TestBoolean = true,
                            TestByte = 255,
                            TestUnsignedInt16 = 1234,
                            TestUnsignedInt32 = 1234565789U,
                            TestUnsignedInt64 = 1234567890123456789UL,
                            TestCharacter = 'a',
                            TestSignedByte = -128,
                            Enum64 = Enum64.SomeValue,
                            Enum32 = Enum32.SomeValue,
                            Enum16 = Enum16.SomeValue,
                            Enum8 = Enum8.SomeValue
                        });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var dt = context.Set<BuiltInDataTypes>().Single(e => e.Id == 1);

                var entityType = context.Model.GetEntityType(typeof(BuiltInDataTypes));
                AssertEqualIfMapped(entityType, (short)-1234, () => dt.TestInt16);
                AssertEqualIfMapped(entityType, -123456789, () => dt.TestInt32);
                AssertEqualIfMapped(entityType, -1234567890123456789L, () => dt.TestInt64);
                AssertEqualIfMapped(entityType, -1.23456789, () => dt.TestDouble);
                AssertEqualIfMapped(entityType, -1234567890.01M, () => dt.TestDecimal);
                AssertEqualIfMapped(entityType, DateTime.Parse("01/01/2000 12:34:56"), () => dt.TestDateTime);
                AssertEqualIfMapped(entityType, new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)), () => dt.TestDateTimeOffset);
                AssertEqualIfMapped(entityType, new TimeSpan(0, 10, 9, 8, 7), () => dt.TestTimeSpan);
                AssertEqualIfMapped(entityType, -1.234F, () => dt.TestSingle);
                AssertEqualIfMapped(entityType, true, () => dt.TestBoolean);
                AssertEqualIfMapped(entityType, (byte)255, () => dt.TestByte);
                AssertEqualIfMapped(entityType, Enum64.SomeValue, () => dt.Enum64);
                AssertEqualIfMapped(entityType, Enum32.SomeValue, () => dt.Enum32);
                AssertEqualIfMapped(entityType, Enum16.SomeValue, () => dt.Enum16);
                AssertEqualIfMapped(entityType, Enum8.SomeValue, () => dt.Enum8);
                AssertEqualIfMapped(entityType, (ushort)1234, () => dt.TestUnsignedInt16);
                AssertEqualIfMapped(entityType, 1234565789U, () => dt.TestUnsignedInt32);
                AssertEqualIfMapped(entityType, 1234567890123456789UL, () => dt.TestUnsignedInt64);
                AssertEqualIfMapped(entityType, 'a', () => dt.TestCharacter);
                AssertEqualIfMapped(entityType, (sbyte)-128, () => dt.TestSignedByte);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_with_max_length_set()
        {
            using (var context = CreateContext())
            {
                context.Set<MaxLengthDataTypes>().Add(
                    new MaxLengthDataTypes
                        {
                            Id = 79,
                            String3 = "Skywacker",
                            ByteArray5 = new byte[] { 8, 8, 7, 8, 7, 6 }
                        });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var dt = context.Set<MaxLengthDataTypes>().Single(e => e.Id == 79);

                Assert.Equal(Fixture.SupportsMaxLength ? "Sky" : "Skywacker", dt.String3);
                Assert.Equal(Fixture.SupportsMaxLength ? new byte[] { 8, 8, 7, 8, 7 } : new byte[] { 8, 8, 7, 8, 7, 6 }, dt.ByteArray5);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_with_binary_key()
        {
            if (!Fixture.SupportsBinaryKeys)
            {
                return;
            }

            using (var context = CreateContext())
            {
                context.Set<BinaryKeyDataType>().Add(
                    new BinaryKeyDataType
                        {
                            Id = new byte[] { 1, 2, 3 }
                        });

                context.Set<BinaryForeignKeyDataType>().Add(
                    new BinaryForeignKeyDataType
                        {
                            Id = 77,
                            BinaryKeyDataTypeId = new byte[] { 1, 2, 3 }
                        });

                Assert.Equal(2, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context
                    .Set<BinaryKeyDataType>()
                    .Include(e => e.Dependents)
                    .Single(e => e.Id == new byte[] { 1, 2, 3 });

                Assert.Equal(new byte[] { 1, 2, 3 }, entity.Id);
                Assert.Equal(new byte[] { 1, 2, 3 }, entity.Dependents.First().BinaryKeyDataTypeId);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_with_null_binary_foreign_key()
        {
            using (var context = CreateContext())
            {
                context.Set<BinaryForeignKeyDataType>().Add(
                    new BinaryForeignKeyDataType
                        {
                            Id = 78
                        });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<BinaryForeignKeyDataType>().Single(e => e.Id == 78);

                Assert.Null(entity.BinaryKeyDataTypeId);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_with_string_key()
        {
            using (var context = CreateContext())
            {
                context.Set<StringKeyDataType>().Add(
                    new StringKeyDataType
                        {
                            Id = "Gumball!"
                        });

                context.Set<StringForeignKeyDataType>().Add(
                    new StringForeignKeyDataType
                        {
                            Id = 77,
                            StringKeyDataTypeId = "Gumball!"
                        });

                Assert.Equal(2, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context
                    .Set<StringKeyDataType>()
                    .Include(e => e.Dependents)
                    .Single(e => e.Id == "Gumball!");

                Assert.Equal("Gumball!", entity.Id);
                Assert.Equal("Gumball!", entity.Dependents.First().StringKeyDataTypeId);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_with_null_string_foreign_key()
        {
            using (var context = CreateContext())
            {
                context.Set<StringForeignKeyDataType>().Add(
                    new StringForeignKeyDataType
                        {
                            Id = 78
                        });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<StringForeignKeyDataType>().Single(e => e.Id == 78);

                Assert.Null(entity.StringKeyDataTypeId);
            }
        }

        private static void AssertEqualIfMapped<T>(IEntityType entityType, T expected, Expression<Func<T>> actual)
        {
            if (entityType.FindProperty(((MemberExpression)actual.Body).Member.Name) != null)
            {
                Assert.Equal(expected, actual.Compile()());
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
                            PartitionId = 100
                        });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var dt = context.Set<BuiltInNullableDataTypes>().Single(ndt => ndt.Id == 100);

                Assert.Null(dt.TestString);
                Assert.Null(dt.TestByteArray);
                Assert.Null(dt.TestNullableInt16);
                Assert.Null(dt.TestNullableInt32);
                Assert.Null(dt.TestNullableInt64);
                Assert.Null(dt.TestNullableDouble);
                Assert.Null(dt.TestNullableDecimal);
                Assert.Null(dt.TestNullableDateTime);
                Assert.Null(dt.TestNullableDateTimeOffset);
                Assert.Null(dt.TestNullableTimeSpan);
                Assert.Null(dt.TestNullableSingle);
                Assert.Null(dt.TestNullableBoolean);
                Assert.Null(dt.TestNullableByte);
                Assert.Null(dt.TestNullableUnsignedInt16);
                Assert.Null(dt.TestNullableUnsignedInt32);
                Assert.Null(dt.TestNullableUnsignedInt64);
                Assert.Null(dt.TestNullableCharacter);
                Assert.Null(dt.TestNullableSignedByte);
                Assert.Null(dt.Enum64);
                Assert.Null(dt.Enum32);
                Assert.Null(dt.Enum16);
                Assert.Null(dt.Enum8);
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
                            TestByteArray = new byte[] { 10, 9, 8, 7, 6 },
                            TestNullableInt16 = -1234,
                            TestNullableInt32 = -123456789,
                            TestNullableInt64 = -1234567890123456789L,
                            TestNullableDouble = -1.23456789,
                            TestNullableDecimal = -1234567890.01M,
                            TestNullableDateTime = DateTime.Parse("01/01/2000 12:34:56"),
                            TestNullableDateTimeOffset = new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                            TestNullableTimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                            TestNullableSingle = -1.234F,
                            TestNullableBoolean = false,
                            TestNullableByte = 255,
                            TestNullableUnsignedInt16 = 1234,
                            TestNullableUnsignedInt32 = 1234565789U,
                            TestNullableUnsignedInt64 = 1234567890123456789UL,
                            TestNullableCharacter = 'a',
                            TestNullableSignedByte = -128,
                            Enum64 = Enum64.SomeValue,
                            Enum32 = Enum32.SomeValue,
                            Enum16 = Enum16.SomeValue,
                            Enum8 = Enum8.SomeValue
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var dt = context.Set<BuiltInNullableDataTypes>().Single(ndt => ndt.Id == 101);

                var entityType = context.Model.GetEntityType(typeof(BuiltInDataTypes));
                AssertEqualIfMapped(entityType, "TestString", () => dt.TestString);
                AssertEqualIfMapped(entityType, new byte[] { 10, 9, 8, 7, 6 }, () => dt.TestByteArray);
                AssertEqualIfMapped(entityType, (short)-1234, () => dt.TestNullableInt16);
                AssertEqualIfMapped(entityType, -123456789, () => dt.TestNullableInt32);
                AssertEqualIfMapped(entityType, -1234567890123456789L, () => dt.TestNullableInt64);
                AssertEqualIfMapped(entityType, -1.23456789, () => dt.TestNullableDouble);
                AssertEqualIfMapped(entityType, -1234567890.01M, () => dt.TestNullableDecimal);
                AssertEqualIfMapped(entityType, DateTime.Parse("01/01/2000 12:34:56"), () => dt.TestNullableDateTime);
                AssertEqualIfMapped(entityType, new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)), () => dt.TestNullableDateTimeOffset);
                AssertEqualIfMapped(entityType, new TimeSpan(0, 10, 9, 8, 7), () => dt.TestNullableTimeSpan);
                AssertEqualIfMapped(entityType, -1.234F, () => dt.TestNullableSingle);
                AssertEqualIfMapped(entityType, false, () => dt.TestNullableBoolean);
                AssertEqualIfMapped(entityType, (byte)255, () => dt.TestNullableByte);
                AssertEqualIfMapped(entityType, Enum64.SomeValue, () => dt.Enum64);
                AssertEqualIfMapped(entityType, Enum32.SomeValue, () => dt.Enum32);
                AssertEqualIfMapped(entityType, Enum16.SomeValue, () => dt.Enum16);
                AssertEqualIfMapped(entityType, Enum8.SomeValue, () => dt.Enum8);
                AssertEqualIfMapped(entityType, (ushort)1234, () => dt.TestNullableUnsignedInt16);
                AssertEqualIfMapped(entityType, 1234565789U, () => dt.TestNullableUnsignedInt32);
                AssertEqualIfMapped(entityType, 1234567890123456789UL, () => dt.TestNullableUnsignedInt64);
                AssertEqualIfMapped(entityType, 'a', () => dt.TestNullableCharacter);
                AssertEqualIfMapped(entityType, (sbyte)-128, () => dt.TestNullableSignedByte);
            }
        }

        protected BuiltInDataTypesTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected TFixture Fixture { get; }

        protected DbContext CreateContext()
        {
            return Fixture.CreateContext();
        }
    }
}
