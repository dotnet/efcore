// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class BuiltInDataTypesTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : BuiltInDataTypesTestBase<TFixture>.BuiltInDataTypesFixtureBase, new()
    {
        protected BuiltInDataTypesTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        protected DbContext CreateContext() => Fixture.CreateContext();

        [Fact]
        public virtual void Can_perform_query_with_max_length()
        {
            var shortString = "Sky";
            var shortBinary = new byte[] { 8, 8, 7, 8, 7 };
            var longString = new string('X', Fixture.LongStringLength);
            var longBinary = new byte[Fixture.LongStringLength];
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
                Assert.NotNull(context.Set<MaxLengthDataTypes>().Where(e => e.Id == 799 && e.String3 == shortString).ToList().SingleOrDefault());
                Assert.NotNull(context.Set<MaxLengthDataTypes>().Where(e => e.Id == 799 && e.ByteArray5 == shortBinary).ToList().SingleOrDefault());

                Assert.NotNull(context.Set<MaxLengthDataTypes>().Where(e => e.Id == 799 && e.String9000 == longString).ToList().SingleOrDefault());
                Assert.NotNull(context.Set<MaxLengthDataTypes>().Where(e => e.Id == 799 && e.ByteArray9000 == longBinary).ToList().SingleOrDefault());
            }
        }

        [Fact]
        public virtual void Can_perform_query_with_ansi_strings_test()
        {
            var shortString = Fixture.SupportsUnicodeToAnsiConversion ? "Ϩky" : "sky";
            var longString = new string('Ϩ', Fixture.LongStringLength);

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
                Assert.NotNull(context.Set<UnicodeDataTypes>().Where(e => e.Id == 799 && e.StringDefault == shortString).ToList().SingleOrDefault());
                Assert.NotNull(context.Set<UnicodeDataTypes>().Where(e => e.Id == 799 && e.StringAnsi == shortString).ToList().SingleOrDefault());
                Assert.NotNull(context.Set<UnicodeDataTypes>().Where(e => e.Id == 799 && e.StringAnsi3 == shortString).ToList().SingleOrDefault());

                if (Fixture.SupportsLargeStringComparisons)
                {
                    Assert.NotNull(context.Set<UnicodeDataTypes>().Where(e => e.Id == 799 && e.StringAnsi9000 == longString).ToList().SingleOrDefault());
                }

                Assert.NotNull(context.Set<UnicodeDataTypes>().Where(e => e.Id == 799 && e.StringUnicode == shortString).ToList().SingleOrDefault());

                var entity = context.Set<UnicodeDataTypes>().Where(e => e.Id == 799).ToList().Single();

                Assert.Equal(shortString, entity.StringDefault);
                Assert.Equal(shortString, entity.StringUnicode);

                if (Fixture.SupportsAnsi && Fixture.SupportsUnicodeToAnsiConversion)
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
                var source = AddTestBuiltInDataTypes(context.Set<BuiltInDataTypes>());

                Assert.Equal(1, context.SaveChanges());

                QueryBuiltInDataTypesTest(source);
            }
        }

        [Fact]
        public virtual void Can_query_using_any_data_type_shadow()
        {
            using (var context = CreateContext())
            {
                var source = AddTestBuiltInDataTypes(context.Set<BuiltInDataTypesShadow>());

                Assert.Equal(1, context.SaveChanges());

                QueryBuiltInDataTypesTest(source);
            }
        }

        private void QueryBuiltInDataTypesTest<TEntity>(EntityEntry<TEntity> source)
            where TEntity : BuiltInDataTypesBase
        {
            using (var context = CreateContext())
            {
                var set = context.Set<TEntity>();
                var entity = set.Where(e => e.Id == 11).ToList().Single();
                var entityType = context.Model.FindEntityType(typeof(TEntity));

                var param1 = (short)-1234;
                Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<short>(e, nameof(BuiltInDataTypes.TestInt16)) == param1).ToList().Single());

                var param2 = -123456789;
                Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<int>(e, nameof(BuiltInDataTypes.TestInt32)) == param2).ToList().Single());

                var param3 = -1234567890123456789L;
                Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<long>(e, nameof(BuiltInDataTypes.TestInt64)) == param3).ToList().Single());

                var param4 = -1.23456789;
                Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<double>(e, nameof(BuiltInDataTypes.TestDouble)) == param4).ToList().Single());

                var param5 = -1234567890.01M;
                Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<decimal>(e, nameof(BuiltInDataTypes.TestDecimal)) == param5).ToList().Single());

                var param6 = Fixture.DefaultDateTime;
                Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<DateTime>(e, nameof(BuiltInDataTypes.TestDateTime)) == param6).ToList().Single());

                if (entityType.FindProperty(nameof(BuiltInDataTypes.TestDateTimeOffset)) != null)
                {
                    var param7 = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0));
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<DateTimeOffset>(e, nameof(BuiltInDataTypes.TestDateTimeOffset)) == param7).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInDataTypes.TestTimeSpan)) != null)
                {
                    var param8 = new TimeSpan(0, 10, 9, 8, 7);
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<TimeSpan>(e, nameof(BuiltInDataTypes.TestTimeSpan)) == param8).ToList().Single());
                }

                var param9 = -1.234F;
                Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<float>(e, nameof(BuiltInDataTypes.TestSingle)) == param9).ToList().Single());

                var param10 = true;
                Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<bool>(e, nameof(BuiltInDataTypes.TestBoolean)) == param10).ToList().Single());

                if (entityType.FindProperty(nameof(BuiltInDataTypes.TestByte)) != null)
                {
                    var param11 = (byte)255;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<byte>(e, nameof(BuiltInDataTypes.TestByte)) == param11).ToList().Single());
                }

                var param12 = Enum64.SomeValue;
                Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<Enum64>(e, nameof(BuiltInDataTypes.Enum64)) == param12).ToList().Single());

                var param13 = Enum32.SomeValue;
                Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<Enum32>(e, nameof(BuiltInDataTypes.Enum32)) == param13).ToList().Single());

                var param14 = Enum16.SomeValue;
                Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<Enum16>(e, nameof(BuiltInDataTypes.Enum16)) == param14).ToList().Single());

                if (entityType.FindProperty(nameof(BuiltInDataTypes.Enum8)) != null)
                {
                    var param15 = Enum8.SomeValue;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<Enum8>(e, nameof(BuiltInDataTypes.Enum8)) == param15).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInDataTypes.TestUnsignedInt16)) != null)
                {
                    var param16 = (ushort)1234;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<ushort>(e, nameof(BuiltInDataTypes.TestUnsignedInt16)) == param16).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInDataTypes.TestUnsignedInt32)) != null)
                {
                    var param17 = 1234565789U;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<uint>(e, nameof(BuiltInDataTypes.TestUnsignedInt32)) == param17).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInDataTypes.TestUnsignedInt64)) != null)
                {
                    var param18 = 1234567890123456789UL;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<ulong>(e, nameof(BuiltInDataTypes.TestUnsignedInt64)) == param18).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInDataTypes.TestCharacter)) != null)
                {
                    var param19 = 'a';
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<char>(e, nameof(BuiltInDataTypes.TestCharacter)) == param19).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInDataTypes.TestSignedByte)) != null)
                {
                    var param20 = (sbyte)-128;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<sbyte>(e, nameof(BuiltInDataTypes.TestSignedByte)) == param20).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInDataTypes.EnumU64)) != null)
                {
                    var param21 = EnumU64.SomeValue;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<EnumU64>(e, nameof(BuiltInDataTypes.EnumU64)) == param21).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInDataTypes.EnumU32)) != null)
                {
                    var param22 = EnumU32.SomeValue;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<EnumU32>(e, nameof(BuiltInDataTypes.EnumU32)) == param22).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInDataTypes.EnumU16)) != null)
                {
                    var param23 = EnumU16.SomeValue;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<EnumU16>(e, nameof(BuiltInDataTypes.EnumU16)) == param23).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInDataTypes.EnumS8)) != null)
                {
                    var param24 = EnumS8.SomeValue;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<EnumS8>(e, nameof(BuiltInDataTypes.EnumS8)) == param24).ToList().Single());
                }

                if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInDataTypes.Enum64))?.GetProviderClrType()) == typeof(long))
                {
                    var param25 = 1;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<Enum64>(e, nameof(BuiltInDataTypes.Enum64)) == (Enum64)param25).ToList().Single());
                    Assert.Same(entity, set.Where(e => e.Id == 11 && (int)EF.Property<Enum64>(e, nameof(BuiltInDataTypes.Enum64)) == param25).ToList().Single());

                }

                if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInDataTypes.Enum32))?.GetProviderClrType()) == typeof(int))
                {
                    var param26 = 1;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<Enum32>(e, nameof(BuiltInDataTypes.Enum32)) == (Enum32)param26).ToList().Single());
                    Assert.Same(entity, set.Where(e => e.Id == 11 && (int)EF.Property<Enum32>(e, nameof(BuiltInDataTypes.Enum32)) == param26).ToList().Single());

                }

                if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInDataTypes.Enum16))?.GetProviderClrType()) == typeof(short))
                {
                    var param27 = 1;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<Enum16>(e, nameof(BuiltInDataTypes.Enum16)) == (Enum16)param27).ToList().Single());
                    Assert.Same(entity, set.Where(e => e.Id == 11 && (int)EF.Property<Enum16>(e, nameof(BuiltInDataTypes.Enum16)) == param27).ToList().Single());

                }

                if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInDataTypes.Enum8))?.GetProviderClrType()) == typeof(byte))
                {
                    var param28 = 1;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<Enum8>(e, nameof(BuiltInDataTypes.Enum8)) == (Enum8)param28).ToList().Single());
                    Assert.Same(entity, set.Where(e => e.Id == 11 && (int)EF.Property<Enum8>(e, nameof(BuiltInDataTypes.Enum8)) == param28).ToList().Single());

                }

                foreach (var propertyEntry in context.Entry(entity).Properties)
                {
                    Assert.Equal(
                        source.Property(propertyEntry.Metadata.Name).CurrentValue,
                        propertyEntry.CurrentValue);
                }
            }
        }

        private EntityEntry<TEntity> AddTestBuiltInDataTypes<TEntity>(DbSet<TEntity> set)
            where TEntity : BuiltInDataTypesBase, new()
        {
            var entityEntry = set.Add(new TEntity { Id = 11 });

            entityEntry.CurrentValues.SetValues(
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
                    Enum8 = Enum8.SomeValue,
                    EnumU64 = EnumU64.SomeValue,
                    EnumU32 = EnumU32.SomeValue,
                    EnumU16 = EnumU16.SomeValue,
                    EnumS8 = EnumS8.SomeValue
                });

            return entityEntry;
        }

        [Fact]
        public virtual void Can_query_using_any_nullable_data_type()
        {
            using (var context = CreateContext())
            {
                var source = AddTestBuiltInNullableDataTypes(context.Set<BuiltInNullableDataTypes>());

                Assert.Equal(1, context.SaveChanges());

                QueryBuiltInNullableDataTypesTest(source);
            }
        }

        [Fact]
        public virtual void Can_query_using_any_data_type_nullable_shadow()
        {
            using (var context = CreateContext())
            {
                var source = AddTestBuiltInNullableDataTypes(context.Set<BuiltInNullableDataTypesShadow>());

                Assert.Equal(1, context.SaveChanges());

                QueryBuiltInNullableDataTypesTest(source);
            }
        }

        private void QueryBuiltInNullableDataTypesTest<TEntity>(EntityEntry<TEntity> source)
            where TEntity : BuiltInNullableDataTypesBase
        {
            using (var context = CreateContext())
            {
                var set = context.Set<TEntity>();
                var entity = set.Where(e => e.Id == 11).ToList().Single();
                var entityType = context.Model.FindEntityType(typeof(TEntity));

                short? param1 = -1234;
                Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<short?>(e, nameof(BuiltInNullableDataTypes.TestNullableInt16)) == param1).ToList().Single());

                int? param2 = -123456789;
                Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<int?>(e, nameof(BuiltInNullableDataTypes.TestNullableInt32)) == param2).ToList().Single());

                long? param3 = -1234567890123456789L;
                Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<long?>(e, nameof(BuiltInNullableDataTypes.TestNullableInt64)) == param3).ToList().Single());

                double? param4 = -1.23456789;
                Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<double?>(e, nameof(BuiltInNullableDataTypes.TestNullableDouble)) == param4).ToList().Single());

                decimal? param5 = -1234567890.01M;
                Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<decimal?>(e, nameof(BuiltInNullableDataTypes.TestNullableDecimal)) == param5).ToList().Single());

                DateTime? param6 = Fixture.DefaultDateTime;
                Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<DateTime?>(e, nameof(BuiltInNullableDataTypes.TestNullableDateTime)) == param6).ToList().Single());

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableDateTimeOffset)) != null)
                {
                    DateTimeOffset? param7 = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0));
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<DateTimeOffset?>(e, nameof(BuiltInNullableDataTypes.TestNullableDateTimeOffset)) == param7).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableTimeSpan)) != null)
                {
                    TimeSpan? param8 = new TimeSpan(0, 10, 9, 8, 7);
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<TimeSpan?>(e, nameof(BuiltInNullableDataTypes.TestNullableTimeSpan)) == param8).ToList().Single());
                }

                float? param9 = -1.234F;
                Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<float?>(e, nameof(BuiltInNullableDataTypes.TestNullableSingle)) == param9).ToList().Single());

                bool? param10 = true;
                Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<bool?>(e, nameof(BuiltInNullableDataTypes.TestNullableBoolean)) == param10).ToList().Single());

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableByte)) != null)
                {
                    byte? param11 = 255;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<byte?>(e, nameof(BuiltInNullableDataTypes.TestNullableByte)) == param11).ToList().Single());
                }

                Enum64? param12 = Enum64.SomeValue;
                Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<Enum64>(e, nameof(BuiltInNullableDataTypes.Enum64)) == param12).ToList().Single());

                Enum32? param13 = Enum32.SomeValue;
                Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<Enum32>(e, nameof(BuiltInNullableDataTypes.Enum32)) == param13).ToList().Single());

                Enum16? param14 = Enum16.SomeValue;
                Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<Enum16>(e, nameof(BuiltInNullableDataTypes.Enum16)) == param14).ToList().Single());

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.Enum8)) != null)
                {
                    Enum8? param15 = Enum8.SomeValue;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<Enum8>(e, nameof(BuiltInNullableDataTypes.Enum8)) == param15).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt16)) != null)
                {
                    ushort? param16 = 1234;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<ushort?>(e, nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt16)) == param16).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt32)) != null)
                {
                    uint? param17 = 1234565789U;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<uint?>(e, nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt32)) == param17).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt64)) != null)
                {
                    ulong? param18 = 1234567890123456789UL;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<ulong?>(e, nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt64)) == param18).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableCharacter)) != null)
                {
                    char? param19 = 'a';
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<char?>(e, nameof(BuiltInNullableDataTypes.TestNullableCharacter)) == param19).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableSignedByte)) != null)
                {
                    sbyte? param20 = -128;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<sbyte?>(e, nameof(BuiltInNullableDataTypes.TestNullableSignedByte)) == param20).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU64)) != null)
                {
                    var param21 = EnumU64.SomeValue;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<EnumU64>(e, nameof(BuiltInNullableDataTypes.EnumU64)) == param21).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU32)) != null)
                {
                    var param22 = EnumU32.SomeValue;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<EnumU32>(e, nameof(BuiltInNullableDataTypes.EnumU32)) == param22).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU16)) != null)
                {
                    var param23 = EnumU16.SomeValue;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<EnumU16>(e, nameof(BuiltInNullableDataTypes.EnumU16)) == param23).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumS8)) != null)
                {
                    var param24 = EnumS8.SomeValue;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<EnumS8>(e, nameof(BuiltInNullableDataTypes.EnumS8)) == param24).ToList().Single());
                }

                if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInNullableDataTypes.Enum64))?.GetProviderClrType()) == typeof(long))
                {
                    int? param25 = 1;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<Enum64?>(e, nameof(BuiltInNullableDataTypes.Enum64)) == (Enum64)param25).ToList().Single());
                    Assert.Same(entity, set.Where(e => e.Id == 11 && (int)EF.Property<Enum64?>(e, nameof(BuiltInNullableDataTypes.Enum64)) == param25).ToList().Single());
                }

                if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInNullableDataTypes.Enum32))?.GetProviderClrType()) == typeof(int))
                {
                    int? param26 = 1;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<Enum32?>(e, nameof(BuiltInNullableDataTypes.Enum32)) == (Enum32)param26).ToList().Single());
                    Assert.Same(entity, set.Where(e => e.Id == 11 && (int)EF.Property<Enum32?>(e, nameof(BuiltInNullableDataTypes.Enum32)) == param26).ToList().Single());

                }

                if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInNullableDataTypes.Enum16))?.GetProviderClrType()) == typeof(short))
                {
                    int? param27 = 1;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<Enum16?>(e, nameof(BuiltInNullableDataTypes.Enum16)) == (Enum16)param27).ToList().Single());
                    Assert.Same(entity, set.Where(e => e.Id == 11 && (int)EF.Property<Enum16?>(e, nameof(BuiltInNullableDataTypes.Enum16)) == param27).ToList().Single());

                }

                if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInNullableDataTypes.Enum8))?.GetProviderClrType()) == typeof(byte))
                {
                    int? param28 = 1;
                    Assert.Same(entity, set.Where(e => e.Id == 11 && EF.Property<Enum8?>(e, nameof(BuiltInNullableDataTypes.Enum8)) == (Enum8)param28).ToList().Single());
                    Assert.Same(entity, set.Where(e => e.Id == 11 && (int)EF.Property<Enum8?>(e, nameof(BuiltInNullableDataTypes.Enum8)) == param28).ToList().Single());

                }

                foreach (var propertyEntry in context.Entry(entity).Properties)
                {
                    Assert.Equal(
                        source.Property(propertyEntry.Metadata.Name).CurrentValue,
                        propertyEntry.CurrentValue);
                }
            }
        }

        private static Type UnwrapNullableType(Type type)
            => type == null ? null : Nullable.GetUnderlyingType(type) ?? type;


        private EntityEntry<TEntity> AddTestBuiltInNullableDataTypes<TEntity>(DbSet<TEntity> set)
            where TEntity : BuiltInNullableDataTypesBase, new()
        {
            var entityEntry = set.Add(new TEntity { Id = 11 });

            entityEntry.CurrentValues.SetValues(
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
                    Enum8 = Enum8.SomeValue,
                    EnumU64 = EnumU64.SomeValue,
                    EnumU32 = EnumU32.SomeValue,
                    EnumU16 = EnumU16.SomeValue,
                    EnumS8 = EnumS8.SomeValue
                });

            return entityEntry;
        }

        [Fact]
        public virtual void Can_query_using_any_nullable_data_type_as_literal()
        {
            using (var context = CreateContext())
            {
                context.Set<BuiltInNullableDataTypes>().Add(
                    new BuiltInNullableDataTypes
                    {
                        Id = 12,
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
                        Enum8 = Enum8.SomeValue,
                        EnumU64 = EnumU64.SomeValue,
                        EnumU32 = EnumU32.SomeValue,
                        EnumU16 = EnumU16.SomeValue,
                        EnumS8 = EnumS8.SomeValue
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12).ToList().Single();
                var entityType = context.Model.FindEntityType(typeof(BuiltInNullableDataTypes));

                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableInt16 == -1234).ToList().Single());

                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableInt32 == -123456789).ToList().Single());

                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableInt64 == -1234567890123456789L).ToList().Single());

                Assert.Same(
                    entity,
                    Fixture.StrictEquality
                        ? context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableDouble == -1.23456789).ToList().Single()
                        : context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && -e.TestNullableDouble + -1.23456789 < 1E-5).ToList().Single());

                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableDouble != 1E18).ToList().Single());

                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableDecimal == -1234567890.01M).ToList().Single());

                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableDateTime == Fixture.DefaultDateTime).ToList().Single());

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableDateTimeOffset)) != null)
                {
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableDateTimeOffset == new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0))).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableTimeSpan)) != null)
                {
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableTimeSpan == new TimeSpan(0, 10, 9, 8, 7)).ToList().Single());
                }

                Assert.Same(
                    entity,
                    Fixture.StrictEquality
                        ? context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableSingle == -1.234F).ToList().Single()
                        : context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && -e.TestNullableSingle + -1.234F < 1E-5).ToList().Single());

                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableSingle != 1E-8).ToList().Single());

                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableBoolean == true).ToList().Single());

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableByte)) != null)
                {
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableByte == 255).ToList().Single());
                }

                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.Enum64 == Enum64.SomeValue).ToList().Single());

                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.Enum32 == Enum32.SomeValue).ToList().Single());

                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.Enum16 == Enum16.SomeValue).ToList().Single());

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.Enum8)) != null)
                {
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.Enum8 == Enum8.SomeValue).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt16)) != null)
                {
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableUnsignedInt16 == 1234).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt32)) != null)
                {
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableUnsignedInt32 == 1234565789U).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt64)) != null)
                {
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableUnsignedInt64 == 1234567890123456789UL).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableCharacter)) != null)
                {
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableCharacter == 'a').ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableSignedByte)) != null)
                {
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.TestNullableSignedByte == -128).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU64)) != null)
                {
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.EnumU64 == EnumU64.SomeValue).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU32)) != null)
                {
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.EnumU32 == EnumU32.SomeValue).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU16)) != null)
                {
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.EnumU16 == EnumU16.SomeValue).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumS8)) != null)
                {
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 12 && e.EnumS8 == EnumS8.SomeValue).ToList().Single());
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
                var entity = context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711).ToList().Single();

                short? param1 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableInt16 == param1).ToList().Single());
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && (long?)(int?)e.TestNullableInt16 == param1).ToList().Single());

                int? param2 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableInt32 == param2).ToList().Single());

                long? param3 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableInt64 == param3).ToList().Single());

                double? param4 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableDouble == param4).ToList().Single());

                decimal? param5 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableDecimal == param5).ToList().Single());

                DateTime? param6 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableDateTime == param6).ToList().Single());

                DateTimeOffset? param7 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableDateTimeOffset == param7).ToList().Single());

                TimeSpan? param8 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableTimeSpan == param8).ToList().Single());

                float? param9 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableSingle == param9).ToList().Single());

                bool? param10 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableBoolean == param10).ToList().Single());

                byte? param11 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableByte == param11).ToList().Single());

                Enum64? param12 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.Enum64 == param12).ToList().Single());

                Enum32? param13 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.Enum32 == param13).ToList().Single());

                Enum16? param14 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.Enum16 == param14).ToList().Single());

                Enum8? param15 = null;
                Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.Enum8 == param15).ToList().Single());

                var entityType = context.Model.FindEntityType(typeof(BuiltInNullableDataTypes));
                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt16)) != null)
                {
                    ushort? param16 = null;
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableUnsignedInt16 == param16).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt32)) != null)
                {
                    uint? param17 = null;
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableUnsignedInt32 == param17).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt64)) != null)
                {
                    ulong? param18 = null;
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableUnsignedInt64 == param18).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableCharacter)) != null)
                {
                    char? param19 = null;
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableCharacter == param19).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableSignedByte)) != null)
                {
                    sbyte? param20 = null;
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableSignedByte == param20).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU64)) != null)
                {
                    EnumU64? param21 = null;
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.EnumU64 == param21).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU32)) != null)
                {
                    EnumU32? param22 = null;
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.EnumU32 == param22).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU16)) != null)
                {
                    EnumU16? param23 = null;
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.EnumU16 == param23).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumS8)) != null)
                {
                    EnumS8? param24 = null;
                    Assert.Same(entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.EnumS8 == param24).ToList().Single());
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
                        Enum8 = Enum8.SomeValue,
                        EnumU64 = EnumU64.SomeValue,
                        EnumU32 = EnumU32.SomeValue,
                        EnumU16 = EnumU16.SomeValue,
                        EnumS8 = EnumS8.SomeValue
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var dt = context.Set<BuiltInDataTypes>().Where(e => e.Id == 1).ToList().Single();

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
                AssertEqualIfMapped(entityType, EnumU64.SomeValue, () => dt.EnumU64);
                AssertEqualIfMapped(entityType, EnumU32.SomeValue, () => dt.EnumU32);
                AssertEqualIfMapped(entityType, EnumU16.SomeValue, () => dt.EnumU16);
                AssertEqualIfMapped(entityType, EnumS8.SomeValue, () => dt.EnumS8);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_with_max_length_set()
        {
            const string shortString = "Sky";
            var shortBinary = new byte[] { 8, 8, 7, 8, 7 };

            var longString = new string('X', Fixture.LongStringLength);
            var longBinary = new byte[Fixture.LongStringLength];
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
                var dt = context.Set<MaxLengthDataTypes>().Where(e => e.Id == 79).ToList().Single();

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
                    .Where(e => e.Id == new byte[] { 1, 2, 3 })
                    .ToList().Single();

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
                var entity = context.Set<BinaryForeignKeyDataType>().Where(e => e.Id == 78).ToList().Single();

                Assert.Null(entity.BinaryKeyDataTypeId);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_with_string_key()
        {
            using (var context = CreateContext())
            {
                var principal = context.Set<StringKeyDataType>().Add(
                    new StringKeyDataType
                    {
                        Id = "Gumball!"
                    }).Entity;

                var dependent = context.Set<StringForeignKeyDataType>().Add(
                    new StringForeignKeyDataType
                    {
                        Id = 77,
                        StringKeyDataTypeId = "Gumball!"
                    }).Entity;

                Assert.Same(principal, dependent.Principal);

                Assert.Equal(2, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context
                    .Set<StringKeyDataType>()
                    .Include(e => e.Dependents)
                    .Where(e => e.Id == "Gumball!")
                    .ToList().Single();

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
                var entity = context.Set<StringForeignKeyDataType>().Where(e => e.Id == 78).ToList().Single();

                Assert.Null(entity.StringKeyDataTypeId);
            }
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
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
                var dt = context.Set<BuiltInNullableDataTypes>().Where(ndt => ndt.Id == 100).ToList().Single();

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
                Assert.Null(dt.EnumU64);
                Assert.Null(dt.EnumU32);
                Assert.Null(dt.EnumU16);
                Assert.Null(dt.EnumS8);
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
                        Enum8 = Enum8.SomeValue,
                        EnumU64 = EnumU64.SomeValue,
                        EnumU32 = EnumU32.SomeValue,
                        EnumU16 = EnumU16.SomeValue,
                        EnumS8 = EnumS8.SomeValue
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var dt = context.Set<BuiltInNullableDataTypes>().Where(ndt => ndt.Id == 101).ToList().Single();

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
                AssertEqualIfMapped(entityType, EnumU64.SomeValue, () => dt.EnumU64);
                AssertEqualIfMapped(entityType, EnumU32.SomeValue, () => dt.EnumU32);
                AssertEqualIfMapped(entityType, EnumU16.SomeValue, () => dt.EnumU16);
                AssertEqualIfMapped(entityType, EnumS8.SomeValue, () => dt.EnumS8);
            }
        }

        public abstract class BuiltInDataTypesFixtureBase : SharedStoreFixtureBase<DbContext>
        {
            protected override string StoreName { get; } = "BuiltInDataTypes";

            public virtual int LongStringLength => 9000;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<BinaryKeyDataType>();
                modelBuilder.Entity<StringKeyDataType>();
                modelBuilder.Entity<BuiltInDataTypes>(eb =>
                {
                    eb.HasData(
                        new BuiltInDataTypes
                        {
                            Id = 13,
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
                            Enum8 = Enum8.SomeValue,
                            EnumU64 = EnumU64.SomeValue,
                            EnumU32 = EnumU32.SomeValue,
                            EnumU16 = EnumU16.SomeValue,
                            EnumS8 = EnumS8.SomeValue
                        });
                    eb.Property(e => e.Id).ValueGeneratedNever();
                });
                modelBuilder.Entity<BuiltInDataTypesShadow>().Property(e => e.Id).ValueGeneratedNever();
                modelBuilder.Entity<BuiltInNullableDataTypes>(eb =>
                {
                    eb.HasData(
                        new BuiltInNullableDataTypes
                        {
                            Id = 13,
                            PartitionId = 1,
                            TestNullableInt16 = -1234,
                            TestNullableInt32 = -123456789,
                            TestNullableInt64 = -1234567890123456789L,
                            TestNullableDouble = -1.23456789,
                            TestNullableDecimal = -1234567890.01M,
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
                            Enum8 = Enum8.SomeValue,
                            EnumU64 = EnumU64.SomeValue,
                            EnumU32 = EnumU32.SomeValue,
                            EnumU16 = EnumU16.SomeValue,
                            EnumS8 = EnumS8.SomeValue
                        });
                    eb.Property(e => e.Id).ValueGeneratedNever();
                });
                modelBuilder.Entity<BuiltInNullableDataTypesShadow>().Property(e => e.Id).ValueGeneratedNever();
                modelBuilder.Entity<BinaryForeignKeyDataType>().Property(e => e.Id).ValueGeneratedNever();
                modelBuilder.Entity<StringForeignKeyDataType>().Property(e => e.Id).ValueGeneratedNever();
                MakeRequired<BuiltInDataTypes>(modelBuilder);
                MakeRequired<BuiltInDataTypesShadow>(modelBuilder);

                modelBuilder.Entity<MaxLengthDataTypes>(b =>
                    {
                        b.Property(e => e.Id).ValueGeneratedNever();
                        b.Property(e => e.ByteArray5).HasMaxLength(5);
                        b.Property(e => e.String3).HasMaxLength(3);
                        b.Property(e => e.ByteArray9000).HasMaxLength(LongStringLength);
                        b.Property(e => e.String9000).HasMaxLength(LongStringLength);
                    });

                modelBuilder.Entity<UnicodeDataTypes>(b =>
                    {
                        b.Property(e => e.Id).ValueGeneratedNever();
                        b.Property(e => e.StringAnsi).IsUnicode(false);
                        b.Property(e => e.StringAnsi3).HasMaxLength(3).IsUnicode(false);
                        b.Property(e => e.StringAnsi9000).IsUnicode(false).HasMaxLength(LongStringLength);
                        b.Property(e => e.StringUnicode).IsUnicode();
                    });

                modelBuilder.Entity<BuiltInDataTypesShadow>(
                    b =>
                    {
                        foreach (var property in modelBuilder.Entity<BuiltInDataTypes>().Metadata
                            .GetProperties().Where(p => p.Name != "Id"))
                        {
                            b.Property(property.ClrType, property.Name);
                        }
                    });

                modelBuilder.Entity<BuiltInNullableDataTypesShadow>(
                    b =>
                    {
                        foreach (var property in modelBuilder.Entity<BuiltInNullableDataTypes>().Metadata
                            .GetProperties().Where(p => p.Name != "Id"))
                        {
                            b.Property(property.ClrType, property.Name);
                        }
                    });
            }

            protected static void MakeRequired<TEntity>(ModelBuilder modelBuilder) where TEntity : class
            {
                var entityType = modelBuilder.Entity<TEntity>().Metadata;

                foreach (var propertyInfo in entityType.ClrType.GetTypeInfo().DeclaredProperties)
                {
                    entityType.GetOrAddProperty(propertyInfo).IsNullable = false;
                }
            }

            public abstract bool StrictEquality { get; }

            public abstract bool SupportsAnsi { get; }

            public abstract bool SupportsUnicodeToAnsiConversion { get; }

            public abstract bool SupportsLargeStringComparisons { get; }

            public abstract bool SupportsBinaryKeys { get; }

            public abstract DateTime DefaultDateTime { get; }

        }

        protected class BuiltInDataTypesBase
        {
            public int Id { get; set; }
        }

        protected class BuiltInDataTypes : BuiltInDataTypesBase
        {
            public int PartitionId { get; set; }
            public short TestInt16 { get; set; }
            public int TestInt32 { get; set; }
            public long TestInt64 { get; set; }
            public double TestDouble { get; set; }
            public decimal TestDecimal { get; set; }
            public DateTime TestDateTime { get; set; }
            public DateTimeOffset TestDateTimeOffset { get; set; }
            public TimeSpan TestTimeSpan { get; set; }
            public float TestSingle { get; set; }
            public bool TestBoolean { get; set; }
            public byte TestByte { get; set; }
            public ushort TestUnsignedInt16 { get; set; }
            public uint TestUnsignedInt32 { get; set; }
            public ulong TestUnsignedInt64 { get; set; }
            public char TestCharacter { get; set; }
            public sbyte TestSignedByte { get; set; }
            public Enum64 Enum64 { get; set; }
            public Enum32 Enum32 { get; set; }
            public Enum16 Enum16 { get; set; }
            public Enum8 Enum8 { get; set; }
            public EnumU64 EnumU64 { get; set; }
            public EnumU32 EnumU32 { get; set; }
            public EnumU16 EnumU16 { get; set; }
            public EnumS8 EnumS8 { get; set; }
        }

        protected class BuiltInDataTypesShadow : BuiltInDataTypesBase
        {
        }

        protected enum Enum64 : long
        {
            SomeValue = 1
        }

        protected enum Enum32
        {
            SomeValue = 1
        }

        protected enum Enum16 : short
        {
            SomeValue = 1
        }

        protected enum Enum8 : byte
        {
            SomeValue = 1
        }

        protected enum EnumU64 : ulong
        {
            SomeValue = 1234567890123456789UL
        }

        protected enum EnumU32 : uint
        {
            SomeValue = uint.MaxValue
        }

        protected enum EnumU16 : ushort
        {
            SomeValue = ushort.MaxValue
        }

        protected enum EnumS8 : sbyte
        {
            SomeValue = sbyte.MinValue
        }

        protected class MaxLengthDataTypes
        {
            public int Id { get; set; }
            public string String3 { get; set; }
            public byte[] ByteArray5 { get; set; }
            public string String9000 { get; set; }
            public byte[] ByteArray9000 { get; set; }
        }

        protected class UnicodeDataTypes
        {
            public int Id { get; set; }
            public string StringDefault { get; set; }
            public string StringAnsi { get; set; }
            public string StringAnsi3 { get; set; }
            public string StringAnsi9000 { get; set; }
            public string StringUnicode { get; set; }
        }

        protected class BinaryKeyDataType
        {
            public byte[] Id { get; set; }

            public ICollection<BinaryForeignKeyDataType> Dependents { get; set; }
        }

        protected class BinaryForeignKeyDataType
        {
            public int Id { get; set; }
            public byte[] BinaryKeyDataTypeId { get; set; }

            public BinaryKeyDataType Principal { get; set; }
        }

        protected class StringKeyDataType
        {
            public string Id { get; set; }

            public ICollection<StringForeignKeyDataType> Dependents { get; set; }
        }

        protected class StringForeignKeyDataType
        {
            public int Id { get; set; }
            public string StringKeyDataTypeId { get; set; }

            public StringKeyDataType Principal { get; set; }
        }

        protected class BuiltInNullableDataTypesBase
        {
            public int Id { get; set; }
        }

        protected class BuiltInNullableDataTypes : BuiltInNullableDataTypesBase
        {
            public int PartitionId { get; set; }
            public string TestString { get; set; }
            public byte[] TestByteArray { get; set; }
            public short? TestNullableInt16 { get; set; }
            public int? TestNullableInt32 { get; set; }
            public long? TestNullableInt64 { get; set; }
            public double? TestNullableDouble { get; set; }
            public decimal? TestNullableDecimal { get; set; }
            public DateTime? TestNullableDateTime { get; set; }
            public DateTimeOffset? TestNullableDateTimeOffset { get; set; }
            public TimeSpan? TestNullableTimeSpan { get; set; }
            public float? TestNullableSingle { get; set; }
            public bool? TestNullableBoolean { get; set; }
            public byte? TestNullableByte { get; set; }
            public ushort? TestNullableUnsignedInt16 { get; set; }
            public uint? TestNullableUnsignedInt32 { get; set; }
            public ulong? TestNullableUnsignedInt64 { get; set; }
            public char? TestNullableCharacter { get; set; }
            public sbyte? TestNullableSignedByte { get; set; }
            // ReSharper disable MemberHidesStaticFromOuterClass
            public Enum64? Enum64 { get; set; }
            public Enum32? Enum32 { get; set; }
            public Enum16? Enum16 { get; set; }
            public Enum8? Enum8 { get; set; }
            public EnumU64? EnumU64 { get; set; }
            public EnumU32? EnumU32 { get; set; }
            public EnumU16? EnumU16 { get; set; }
            public EnumS8? EnumS8 { get; set; }
            // ReSharper restore MemberHidesStaticFromOuterClass
        }

        protected class BuiltInNullableDataTypesShadow : BuiltInNullableDataTypesBase
        {
        }
    }
}
