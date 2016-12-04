// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class BuiltInDataTypesTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : BuiltInDataTypesFixtureBase, new()
    {
        protected BuiltInDataTypesTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected TFixture Fixture { get; }

        protected DbContext CreateContext() => Fixture.CreateContext();

        [Fact]
        public virtual void Can_perform_query_with_max_length()
        {
            var shortString = "Sky";
            var shortBinary = new byte[] { 8, 8, 7, 8, 7 };
            var longString = new string('X', 9000);
            var longBinary = new byte[9000];
            for (var i = 0; i < longBinary.Length; i++)
            {
                longBinary[i] = (byte)i;
            }

            using (var context = CreateContext())
            {
                context.Set<MaxLengthDataTypes>().Add(
                    new MaxLengthDataTypes
                    {
                        Id = 799,
                        String3 = shortString,
                        ByteArray5 = shortBinary,
                        String9000 = longString,
                        ByteArray9000 = longBinary
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                Assert.NotNull(context.Set<MaxLengthDataTypes>().SingleOrDefault(e => e.Id == 799 && e.String3 == shortString));
                Assert.NotNull(context.Set<MaxLengthDataTypes>().SingleOrDefault(e => e.Id == 799 && e.ByteArray5 == shortBinary));

                Assert.NotNull(context.Set<MaxLengthDataTypes>().SingleOrDefault(e => e.Id == 799 && e.String9000 == longString));
                Assert.NotNull(context.Set<MaxLengthDataTypes>().SingleOrDefault(e => e.Id == 799 && e.ByteArray9000 == longBinary));
            }
        }

        public virtual void Can_perform_query_with_ansi_strings(bool supportsAnsi)
        {
            var shortString = "Ϩky";
            var longString = new string('Ϩ', 9000);

            using (var context = CreateContext())
            {
                context.Set<UnicodeDataTypes>().Add(
                    new UnicodeDataTypes
                    {
                        Id = 799,
                        StringDefault = shortString,
                        StringAnsi = shortString,
                        StringAnsi3 = shortString,
                        StringAnsi9000 = longString,
                        StringUnicode = shortString
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                Assert.NotNull(context.Set<UnicodeDataTypes>().SingleOrDefault(e => e.Id == 799 && e.StringDefault == shortString));
                Assert.NotNull(context.Set<UnicodeDataTypes>().SingleOrDefault(e => e.Id == 799 && e.StringAnsi == shortString));
                Assert.NotNull(context.Set<UnicodeDataTypes>().SingleOrDefault(e => e.Id == 799 && e.StringAnsi3 == shortString));
                Assert.NotNull(context.Set<UnicodeDataTypes>().SingleOrDefault(e => e.Id == 799 && e.StringAnsi9000 == longString));
                Assert.NotNull(context.Set<UnicodeDataTypes>().SingleOrDefault(e => e.Id == 799 && e.StringUnicode == shortString));

                var entity = context.Set<UnicodeDataTypes>().SingleOrDefault(e => e.Id == 799);

                Assert.Equal(shortString, entity.StringDefault);
                Assert.Equal(shortString, entity.StringUnicode);

                if (supportsAnsi)
                {
                    Assert.NotEqual(shortString, entity.StringAnsi);
                    Assert.NotEqual(shortString, entity.StringAnsi3);
                    Assert.NotEqual(longString, entity.StringAnsi9000);
                }
                else
                {
                    Assert.Equal(shortString, entity.StringAnsi);
                    Assert.Equal(shortString, entity.StringAnsi3);
                    Assert.Equal(longString, entity.StringAnsi9000);
                }
            }
        }

        [Fact]
        public virtual void Can_query_using_any_data_type()
        {
            using (var context = CreateContext())
            {
                context.Set<BuiltInDataTypes>().Add(
                    new BuiltInDataTypes
                    {
                        Id = 11,
                        PartitionId = 1,
                        TestInt16 = -1234,
                        TestInt32 = -123456789,
                        TestInt64 = -1234567890123456789L,
                        TestDouble = -1.23456789,
                        TestDecimal = -1234567890.01M,
                        TestDateTime = Fixture.DefaultDateTime,
                        TestDateTimeOffset = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0)),
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
                var entity = context.Set<BuiltInDataTypes>().Single(e => e.Id == 11);
                var entityType = context.Model.FindEntityType(typeof(BuiltInDataTypes));

                var param1 = (short)-1234;
                Assert.Same(entity, context.Set<BuiltInDataTypes>().Single(e => e.Id == 11 && e.TestInt16 == param1));

                var param2 = -123456789;
                Assert.Same(entity, context.Set<BuiltInDataTypes>().Single(e => e.Id == 11 && e.TestInt32 == param2));

                var param3 = -1234567890123456789L;
                Assert.Same(entity, context.Set<BuiltInDataTypes>().Single(e => e.Id == 11 && e.TestInt64 == param3));

                var param4 = -1.23456789;
                Assert.Same(entity, context.Set<BuiltInDataTypes>().Single(e => e.Id == 11 && e.TestDouble == param4));

                var param5 = -1234567890.01M;
                Assert.Same(entity, context.Set<BuiltInDataTypes>().Single(e => e.Id == 11 && e.TestDecimal == param5));

                var param6 = Fixture.DefaultDateTime;
                Assert.Same(entity, context.Set<BuiltInDataTypes>().Single(e => e.Id == 11 && e.TestDateTime == param6));

                if (entityType.FindProperty("TestDateTimeOffset") != null)
                {
                    var param7 = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0));
                    Assert.Same(entity, context.Set<BuiltInDataTypes>().Single(e => e.Id == 11 && e.TestDateTimeOffset == param7));
                }

                if (entityType.FindProperty("TestTimeSpan") != null)
                {
                    var param8 = new TimeSpan(0, 10, 9, 8, 7);
                    Assert.Same(entity, context.Set<BuiltInDataTypes>().Single(e => e.Id == 11 && e.TestTimeSpan == param8));
                }

                var param9 = -1.234F;
                Assert.Same(entity, context.Set<BuiltInDataTypes>().Single(e => e.Id == 11 && e.TestSingle == param9));

                var param10 = true;
                Assert.Same(entity, context.Set<BuiltInDataTypes>().Single(e => e.Id == 11 && e.TestBoolean == param10));

                if (entityType.FindProperty("TestByte") != null)
                {
                    var param11 = (byte)255;
                    Assert.Same(entity, context.Set<BuiltInDataTypes>().Single(e => e.Id == 11 && e.TestByte == param11));
                }

                var param12 = Enum64.SomeValue;
                Assert.Same(entity, context.Set<BuiltInDataTypes>().Single(e => e.Id == 11 && e.Enum64 == param12));

                var param13 = Enum32.SomeValue;
                Assert.Same(entity, context.Set<BuiltInDataTypes>().Single(e => e.Id == 11 && e.Enum32 == param13));

                var param14 = Enum16.SomeValue;
                Assert.Same(entity, context.Set<BuiltInDataTypes>().Single(e => e.Id == 11 && e.Enum16 == param14));

                if (entityType.FindProperty("TestEnum8") != null)
                {
                    var param15 = Enum8.SomeValue;
                    Assert.Same(entity, context.Set<BuiltInDataTypes>().Single(e => e.Id == 11 && e.Enum8 == param15));
                }

                if (entityType.FindProperty("TestUnsignedInt16") != null)
                {
                    var param16 = (ushort)1234;
                    Assert.Same(entity, context.Set<BuiltInDataTypes>().Single(e => e.Id == 11 && e.TestUnsignedInt16 == param16));
                }

                if (entityType.FindProperty("TestUnsignedInt32") != null)
                {
                    var param17 = 1234565789U;
                    Assert.Same(entity, context.Set<BuiltInDataTypes>().Single(e => e.Id == 11 && e.TestUnsignedInt32 == param17));
                }

                if (entityType.FindProperty("TestUnsignedInt64") != null)
                {
                    var param18 = 1234567890123456789UL;
                    Assert.Same(entity, context.Set<BuiltInDataTypes>().Single(e => e.Id == 11 && e.TestUnsignedInt64 == param18));
                }

                if (entityType.FindProperty("TestCharacter") != null)
                {
                    var param19 = 'a';
                    Assert.Same(entity, context.Set<BuiltInDataTypes>().Single(e => e.Id == 11 && e.TestCharacter == param19));
                }

                if (entityType.FindProperty("TestSignedByte") != null)
                {
                    var param20 = (sbyte)-128;
                    Assert.Same(entity, context.Set<BuiltInDataTypes>().Single(e => e.Id == 11 && e.TestSignedByte == param20));
                }
            }
        }

        [Fact]
        public virtual void Can_query_using_any_nullable_data_type()
        {
            using (var context = CreateContext())
            {
                context.Set<BuiltInNullableDataTypes>().Add(
                    new BuiltInNullableDataTypes
                    {
                        Id = 11,
                        PartitionId = 1,
                        TestNullableInt16 = -1234,
                        TestNullableInt32 = -123456789,
                        TestNullableInt64 = -1234567890123456789L,
                        TestNullableDouble = -1.23456789,
                        TestNullableDecimal = -1234567890.01M,
                        TestNullableDateTime = Fixture.DefaultDateTime,
                        TestNullableDateTimeOffset = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0)),
                        TestNullableTimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                        TestNullableSingle = -1.234F,
                        TestNullableBoolean = true,
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
                var entity = context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 11);
                var entityType = context.Model.FindEntityType(typeof(BuiltInNullableDataTypes));

                short? param1 = -1234;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 11 && e.TestNullableInt16 == param1));

                int? param2 = -123456789;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 11 && e.TestNullableInt32 == param2));

                long? param3 = -1234567890123456789L;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 11 && e.TestNullableInt64 == param3));

                double? param4 = -1.23456789;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 11 && e.TestNullableDouble == param4));

                decimal? param5 = -1234567890.01M;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 11 && e.TestNullableDecimal == param5));

                DateTime? param6 = Fixture.DefaultDateTime;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 11 && e.TestNullableDateTime == param6));

                if (entityType.FindProperty("TestNullableDateTimeOffset") != null)
                {
                    DateTimeOffset? param7 = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0));
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 11 && e.TestNullableDateTimeOffset == param7));
                }

                if (entityType.FindProperty("TestNullableTimeSpan") != null)
                {
                    TimeSpan? param8 = new TimeSpan(0, 10, 9, 8, 7);
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 11 && e.TestNullableTimeSpan == param8));
                }

                float? param9 = -1.234F;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 11 && e.TestNullableSingle == param9));

                bool? param10 = true;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 11 && e.TestNullableBoolean == param10));

                if (entityType.FindProperty("TestNullableByte") != null)
                {
                    byte? param11 = 255;
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 11 && e.TestNullableByte == param11));
                }

                Enum64? param12 = Enum64.SomeValue;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 11 && e.Enum64 == param12));

                Enum32? param13 = Enum32.SomeValue;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 11 && e.Enum32 == param13));

                Enum16? param14 = Enum16.SomeValue;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 11 && e.Enum16 == param14));

                if (entityType.FindProperty("Enum8") != null)
                {
                    Enum8? param15 = Enum8.SomeValue;
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 11 && e.Enum8 == param15));
                }

                if (entityType.FindProperty("TestUnsignedInt16") != null)
                {
                    ushort? param16 = 1234;
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 11 && e.TestNullableUnsignedInt16 == param16));
                }

                if (entityType.FindProperty("TestUnsignedInt32") != null)
                {
                    uint? param17 = 1234565789U;
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 11 && e.TestNullableUnsignedInt32 == param17));
                }

                if (entityType.FindProperty("TestUnsignedInt64") != null)
                {
                    ulong? param18 = 1234567890123456789UL;
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 11 && e.TestNullableUnsignedInt64 == param18));
                }

                if (entityType.FindProperty("TestCharacter") != null)
                {
                    char? param19 = 'a';
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 11 && e.TestNullableCharacter == param19));
                }

                if (entityType.FindProperty("TestSignedByte") != null)
                {
                    sbyte? param20 = -128;
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 11 && e.TestNullableSignedByte == param20));
                }
            }
        }

        [Fact]
        public virtual void Can_query_with_null_parameters_using_any_nullable_data_type()
        {
            using (var context = CreateContext())
            {
                context.Set<BuiltInNullableDataTypes>().Add(
                    new BuiltInNullableDataTypes
                    {
                        Id = 711
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 711);

                short? param1 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 711 && e.TestNullableInt16 == param1));
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 711 && (long?)(int?)e.TestNullableInt16 == param1));

                int? param2 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 711 && e.TestNullableInt32 == param2));

                long? param3 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 711 && e.TestNullableInt64 == param3));

                double? param4 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 711 && e.TestNullableDouble == param4));

                decimal? param5 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 711 && e.TestNullableDecimal == param5));

                DateTime? param6 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 711 && e.TestNullableDateTime == param6));

                DateTimeOffset? param7 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 711 && e.TestNullableDateTimeOffset == param7));

                TimeSpan? param8 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 711 && e.TestNullableTimeSpan == param8));

                float? param9 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 711 && e.TestNullableSingle == param9));

                bool? param10 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 711 && e.TestNullableBoolean == param10));

                byte? param11 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 711 && e.TestNullableByte == param11));

                Enum64? param12 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 711 && e.Enum64 == param12));

                Enum32? param13 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 711 && e.Enum32 == param13));

                Enum16? param14 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 711 && e.Enum16 == param14));

                Enum8? param15 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 711 && e.Enum8 == param15));

                var entityType = context.Model.FindEntityType(typeof(BuiltInNullableDataTypes));
                if (entityType.FindProperty("TestUnsignedInt16") != null)
                {
                    ushort? param16 = null;
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 711 && e.TestNullableUnsignedInt16 == param16));
                }

                if (entityType.FindProperty("TestUnsignedInt32") != null)
                {
                    uint? param17 = null;
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 711 && e.TestNullableUnsignedInt32 == param17));
                }

                if (entityType.FindProperty("TestUnsignedInt64") != null)
                {
                    ulong? param18 = null;
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 711 && e.TestNullableUnsignedInt64 == param18));
                }

                if (entityType.FindProperty("TestCharacter") != null)
                {
                    char? param19 = null;
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 711 && e.TestNullableCharacter == param19));
                }

                if (entityType.FindProperty("TestSignedByte") != null)
                {
                    sbyte? param20 = null;
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Single(e => e.Id == 711 && e.TestNullableSignedByte == param20));
                }
            }
        }

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

                var entityType = context.Model.FindEntityType(typeof(BuiltInDataTypes));
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
            const string shortString = "Sky";
            var shortBinary = new byte[] { 8, 8, 7, 8, 7 };

            var longString = new string('X', 9000);
            var longBinary = new byte[9000];
            for (var i = 0; i < longBinary.Length; i++)
            {
                longBinary[i] = (byte)i;
            }

            using (var context = CreateContext())
            {
                context.Set<MaxLengthDataTypes>().Add(
                    new MaxLengthDataTypes
                    {
                        Id = 79,
                        String3 = shortString,
                        ByteArray5 = shortBinary,
                        String9000 = longString,
                        ByteArray9000 = longBinary
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var dt = context.Set<MaxLengthDataTypes>().Single(e => e.Id == 79);

                Assert.Equal(shortString, dt.String3);
                Assert.Equal(shortBinary, dt.ByteArray5);
                Assert.Equal(longString, dt.String9000);
                Assert.Equal(longBinary, dt.ByteArray9000);
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

                var entityType = context.Model.FindEntityType(typeof(BuiltInDataTypes));
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
    }
}
