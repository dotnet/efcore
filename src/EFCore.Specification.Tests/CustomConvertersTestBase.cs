// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage.Converters;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class CustomConvertersTestBase<TFixture> : BuiltInDataTypesTestBase<TFixture>
        where TFixture : BuiltInDataTypesTestBase<TFixture>.BuiltInDataTypesFixtureBase, new()
    {
        protected CustomConvertersTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public virtual void Can_query_and_update_with_conversion_for_custom_type()
        {
            Guid id;
            using (var context = CreateContext())
            {
                var user = context.Set<User>().Add(
                    new User(Email.Create("eeky_bear@example.com"))).Entity;

                Assert.Equal(1, context.SaveChanges());

                id = user.Id;
            }

            using (var context = CreateContext())
            {
                var user = context.Set<User>().Single(e => e.Id == id && e.Email == "eeky_bear@example.com");

                Assert.Equal(id, user.Id);
                Assert.Equal("eeky_bear@example.com", user.Email);
            }
        }

        protected class User
        {
            public User(Email email)
            {
                Id = Guid.NewGuid();
                Email = email;
            }

            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
            public Guid Id { get; private set; }

            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
            public Email Email { get; private set; }
        }

        protected class Email
        {
            private readonly string _value;
            private Email(string value) => _value = value;

            public static Email Create(string value) => new Email(value);

            public static implicit operator string(Email email) => email._value;
        }

        public abstract class CustomConvertersFixtureBase : BuiltInDataTypesFixtureBase
        {
            protected override string StoreName { get; } = "CustomConverters";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder
                    .Entity<User>(
                        b =>
                        {
                            b.Property(x => x.Email).HasConversion(email => (string)email, value => Email.Create(value));
                            b.Property(e => e.Id).ValueGeneratedNever();
                        });

                modelBuilder.Entity<BuiltInDataTypes>(
                    b =>
                    {
                        b.Property(e => e.PartitionId).HasConversion(v => (long)v, v => (int)v);
                        b.Property(e => e.TestInt16).HasConversion(v => (long)v, v => (short)v);
                        b.Property(e => e.TestInt32).HasConversion(v => (long)v, v => (int)v);
                        b.Property(e => e.TestInt64).HasConversion(v => v, v => v);
                        b.Property(e => e.TestDecimal).HasConversion(NumberToBytesConverter<decimal>.DefaultInfo.Create());
                        b.Property(e => e.TestDateTime).HasConversion(v => v.ToBinary(), v => DateTime.FromBinary(v));
                        b.Property(e => e.TestTimeSpan).HasConversion(v => v.TotalMilliseconds, v => TimeSpan.FromMilliseconds(v));
                        b.Property(e => e.TestSingle).HasConversion(new CastingConverter<float, double>());
                        b.Property(e => e.TestBoolean).HasConversion(new BoolToTwoValuesConverter<string>("Nope", "Yep"));
                        b.Property(e => e.TestByte).HasConversion(v => (ushort)v, v => (byte)v);
                        b.Property(e => e.TestUnsignedInt16).HasConversion(v => (ulong)v, v => (ushort)v);
                        b.Property(e => e.TestUnsignedInt32).HasConversion(v => (ulong)v, v => (uint)v);
                        b.Property(e => e.TestUnsignedInt64).HasConversion(v => (long)v, v => (ulong)v);
                        b.Property(e => e.TestCharacter).HasConversion(v => (int)v, v => (char)v);
                        b.Property(e => e.TestSignedByte).HasConversion(v => (decimal)v, v => (sbyte)v);
                        b.Property(e => e.Enum64).HasConversion(v => (long)v, v => (Enum64)v);
                        b.Property(e => e.Enum32).HasConversion(v => (long)v, v => (Enum32)v);
                        b.Property(e => e.Enum16).HasConversion(v => (long)v, v => (Enum16)v);
                        b.Property(e => e.EnumU64).HasConversion(v => (ulong)v, v => (EnumU64)v);
                        b.Property(e => e.EnumU32).HasConversion(v => (ulong)v, v => (EnumU32)v);
                        b.Property(e => e.EnumU16).HasConversion(v => (ulong)v, v => (EnumU16)v);

                        b.Property(e => e.EnumS8).HasConversion(
                            v => v.ToString(),
                            v => v == nameof(EnumS8.SomeValue) ? EnumS8.SomeValue : default);

                        b.Property(e => e.Enum8).HasConversion(
                            v => v.ToString(),
                            v => v == nameof(Enum8.SomeValue) ? Enum8.SomeValue : default);

                        b.Property(e => e.TestDateTimeOffset).HasConversion(
                            v => v.ToUnixTimeMilliseconds(),
                            v => DateTimeOffset.FromUnixTimeMilliseconds(v));

                        b.Property(e => e.TestDouble).HasConversion(
                            new ValueConverter<double, decimal>(
                                v => (decimal)v,
                                v => (double)v,
                                new ConverterMappingHints(precision: 26, scale: 16)));
                    });

                modelBuilder.Entity<BuiltInNullableDataTypes>(
                    b =>
                    {
                        b.Property(e => e.PartitionId).HasConversion(v => (long)v, v => (int)v);
                        b.Property(e => e.TestNullableInt16).HasConversion(v => (long?)v, v => (short?)v);
                        b.Property(e => e.TestNullableInt32).HasConversion(v => (long?)v, v => (int?)v);
                        b.Property(e => e.TestNullableInt64).HasConversion(v => v, v => v);
                        b.Property(e => e.TestNullableDecimal).HasConversion(NumberToBytesConverter<decimal?>.DefaultInfo.Create());
                        b.Property(e => e.TestNullableSingle).HasConversion(new CastingConverter<float?, double?>());
                        b.Property(e => e.TestNullableBoolean).HasConversion(new BoolToTwoValuesConverter<string>("Nope", "Yep"));
                        b.Property(e => e.TestNullableByte).HasConversion(v => (ushort?)v, v => (byte?)v);
                        b.Property(e => e.TestNullableUnsignedInt16).HasConversion(v => (ulong?)v, v => (ushort?)v);
                        b.Property(e => e.TestNullableUnsignedInt32).HasConversion(v => (ulong?)v, v => (uint?)v);
                        b.Property(e => e.TestNullableUnsignedInt64).HasConversion(v => (long?)v, v => (ulong?)v);
                        b.Property(e => e.TestNullableCharacter).HasConversion(v => (int?)v, v => (char?)v);
                        b.Property(e => e.TestNullableSignedByte).HasConversion(v => (decimal?)v, v => (sbyte?)v);
                        b.Property(e => e.Enum64).HasConversion(v => (long?)v, v => (Enum64?)v);
                        b.Property(e => e.Enum32).HasConversion(v => (long?)v, v => (Enum32?)v);
                        b.Property(e => e.Enum16).HasConversion(v => (long?)v, v => (Enum16?)v);
                        b.Property(e => e.EnumU64).HasConversion(v => (ulong?)v, v => (EnumU64?)v);
                        b.Property(e => e.EnumU32).HasConversion(v => (ulong?)v, v => (EnumU32?)v);
                        b.Property(e => e.EnumU16).HasConversion(v => (ulong?)v, v => (EnumU16?)v);

                        b.Property(e => e.TestNullableDateTime).HasConversion(
                            v => v.Value.ToBinary(),
                            v => (DateTime?)DateTime.FromBinary(v));

                        b.Property(e => e.TestNullableTimeSpan).HasConversion(
                            v => v.Value.TotalMilliseconds,
                            v => (TimeSpan?)TimeSpan.FromMilliseconds(v));

                        b.Property(e => e.EnumS8).HasConversion(
                            v => v.ToString(),
                            v => v == nameof(EnumS8.SomeValue) ? (EnumS8?)EnumS8.SomeValue : null);

                        b.Property(e => e.Enum8).HasConversion(
                            v => v.ToString(),
                            v => v == nameof(Enum8.SomeValue) ? (Enum8?)Enum8.SomeValue : null);

                        b.Property(e => e.TestNullableDateTimeOffset).HasConversion(
                            v => v.Value.ToUnixTimeMilliseconds(),
                            v => (DateTimeOffset?)DateTimeOffset.FromUnixTimeMilliseconds(v));

                        b.Property(e => e.TestNullableDouble).HasConversion(
                            new ValueConverter<double?, decimal?>(
                                v => (decimal?)v, v => (double?)v,
                                new ConverterMappingHints(precision: 26, scale: 16)));
                    });

                modelBuilder.Entity<BuiltInDataTypesShadow>(
                    b =>
                    {
                        b.Property(nameof(BuiltInDataTypes.PartitionId)).HasConversion(new ValueConverter<int, long>(v => (long)v, v => (int)v));
                        b.Property(nameof(BuiltInDataTypes.TestInt16)).HasConversion(new ValueConverter<short, long>(v => (long)v, v => (short)v));
                        b.Property(nameof(BuiltInDataTypes.TestInt32)).HasConversion(new ValueConverter<int, long>(v => (long)v, v => (int)v));
                        b.Property(nameof(BuiltInDataTypes.TestInt64)).HasConversion(new ValueConverter<long, long>(v => v, v => v));
                        b.Property(nameof(BuiltInDataTypes.TestDecimal)).HasConversion(NumberToBytesConverter<decimal>.DefaultInfo.Create());
                        b.Property(nameof(BuiltInDataTypes.TestDateTime)).HasConversion(new ValueConverter<DateTime, long>(v => v.ToBinary(), v => DateTime.FromBinary(v)));
                        b.Property(nameof(BuiltInDataTypes.TestTimeSpan)).HasConversion(new ValueConverter<TimeSpan, double>(v => v.TotalMilliseconds, v => TimeSpan.FromMilliseconds(v)));
                        b.Property(nameof(BuiltInDataTypes.TestSingle)).HasConversion(new CastingConverter<float, double>());
                        b.Property(nameof(BuiltInDataTypes.TestBoolean)).HasConversion(new BoolToTwoValuesConverter<string>("Nope", "Yep"));
                        b.Property(nameof(BuiltInDataTypes.TestByte)).HasConversion(new ValueConverter<byte, ushort>(v => (ushort)v, v => (byte)v));
                        b.Property(nameof(BuiltInDataTypes.TestUnsignedInt16)).HasConversion(new ValueConverter<ushort, ulong>(v => (ulong)v, v => (ushort)v));
                        b.Property(nameof(BuiltInDataTypes.TestUnsignedInt32)).HasConversion(new ValueConverter<uint, ulong>(v => (ulong)v, v => (uint)v));
                        b.Property(nameof(BuiltInDataTypes.TestUnsignedInt64)).HasConversion(new ValueConverter<ulong, long>(v => (long)v, v => (ulong)v));
                        b.Property(nameof(BuiltInDataTypes.TestCharacter)).HasConversion(new ValueConverter<char, int>(v => (int)v, v => (char)v));
                        b.Property(nameof(BuiltInDataTypes.TestSignedByte)).HasConversion(new ValueConverter<sbyte, decimal>(v => (decimal)v, v => (sbyte)v));
                        b.Property(nameof(BuiltInDataTypes.Enum64)).HasConversion(new ValueConverter<Enum64, long>(v => (long)v, v => (Enum64)v));
                        b.Property(nameof(BuiltInDataTypes.Enum32)).HasConversion(new ValueConverter<Enum32, long>(v => (long)v, v => (Enum32)v));
                        b.Property(nameof(BuiltInDataTypes.Enum16)).HasConversion(new ValueConverter<Enum16, long>(v => (long)v, v => (Enum16)v));
                        b.Property(nameof(BuiltInDataTypes.EnumU64)).HasConversion(new ValueConverter<EnumU64, ulong>(v => (ulong)v, v => (EnumU64)v));
                        b.Property(nameof(BuiltInDataTypes.EnumU32)).HasConversion(new ValueConverter<EnumU32, ulong>(v => (ulong)v, v => (EnumU32)v));
                        b.Property(nameof(BuiltInDataTypes.EnumU16)).HasConversion(new ValueConverter<EnumU16, ulong>(v => (ulong)v, v => (EnumU16)v));

                        b.Property(nameof(BuiltInDataTypes.EnumS8)).HasConversion(
                            new ValueConverter<EnumS8, string>(
                                v => v.ToString(),
                                v => v == nameof(EnumS8.SomeValue) ? EnumS8.SomeValue : default));

                        b.Property(nameof(BuiltInDataTypes.Enum8)).HasConversion(
                            new ValueConverter<Enum8, string>(
                                v => v.ToString(),
                                v => v == nameof(Enum8.SomeValue) ? Enum8.SomeValue : default));

                        b.Property(nameof(BuiltInDataTypes.TestDateTimeOffset)).HasConversion(
                            new ValueConverter<DateTimeOffset, long>(
                                v => v.ToUnixTimeMilliseconds(),
                                v => DateTimeOffset.FromUnixTimeMilliseconds(v)));

                        b.Property(nameof(BuiltInDataTypes.TestDouble)).HasConversion(
                            new ValueConverter<double, decimal>(
                                v => (decimal)v,
                                v => (double)v,
                                new ConverterMappingHints(precision: 26, scale: 16)));
                    });

                modelBuilder.Entity<BuiltInNullableDataTypes>(
                    b =>
                    {
                        b.Property(nameof(BuiltInNullableDataTypes.PartitionId)).HasConversion(new ValueConverter<int, long>(v => (long)v, v => (int)v));
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableInt16)).HasConversion(new ValueConverter<short?, long?>(v => (long?)v, v => (short?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableInt32)).HasConversion(new ValueConverter<int?, long?>(v => (long?)v, v => (int?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableInt64)).HasConversion(new ValueConverter<long?, long?>(v => v, v => v));
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableDecimal)).HasConversion(NumberToBytesConverter<decimal?>.DefaultInfo.Create());
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableSingle)).HasConversion(new CastingConverter<float?, double?>());
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableBoolean)).HasConversion(new BoolToTwoValuesConverter<string>("Nope", "Yep"));
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableByte)).HasConversion(new ValueConverter<byte?, ushort?>(v => (ushort?)v, v => (byte?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt16)).HasConversion(new ValueConverter<ushort?, ulong?>(v => (ulong?)v, v => (ushort?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt32)).HasConversion(new ValueConverter<uint?, ulong?>(v => (ulong?)v, v => (uint?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt64)).HasConversion(new ValueConverter<ulong?, long?>(v => (long?)v, v => (ulong?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableCharacter)).HasConversion(new ValueConverter<char?, int?>(v => (int?)v, v => (char?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableSignedByte)).HasConversion(new ValueConverter<sbyte?, decimal?>(v => (decimal?)v, v => (sbyte?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.Enum64)).HasConversion(new ValueConverter<Enum64?, long?>(v => (long?)v, v => (Enum64?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.Enum32)).HasConversion(new ValueConverter<Enum32?, long?>(v => (long?)v, v => (Enum32?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.Enum16)).HasConversion(new ValueConverter<Enum16?, long?>(v => (long?)v, v => (Enum16?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.EnumU64)).HasConversion(new ValueConverter<EnumU64?, ulong?>(v => (ulong?)v, v => (EnumU64?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.EnumU32)).HasConversion(new ValueConverter<EnumU32?, ulong?>(v => (ulong?)v, v => (EnumU32?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.EnumU16)).HasConversion(new ValueConverter<EnumU16?, ulong?>(v => (ulong?)v, v => (EnumU16?)v));

                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableDateTime)).HasConversion(
                            new ValueConverter<DateTime?, long>(
                                v => v.Value.ToBinary(),
                                v => (DateTime?)DateTime.FromBinary(v)));

                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableTimeSpan)).HasConversion(
                            new ValueConverter<TimeSpan?, double>(
                                v => v.Value.TotalMilliseconds,
                                v => (TimeSpan?)TimeSpan.FromMilliseconds(v)));

                        b.Property(nameof(BuiltInNullableDataTypes.EnumS8)).HasConversion(
                            new ValueConverter<EnumS8?, string>(
                                v => v.ToString(),
                                v => v == nameof(EnumS8.SomeValue) ? (EnumS8?)EnumS8.SomeValue : null));

                        b.Property(nameof(BuiltInNullableDataTypes.Enum8)).HasConversion(
                            new ValueConverter<Enum8?, string>(
                                v => v.ToString(),
                                v => v == nameof(Enum8.SomeValue) ? (Enum8?)Enum8.SomeValue : null));

                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableDateTimeOffset)).HasConversion(
                            new ValueConverter<DateTimeOffset?, long>(
                                v => v.Value.ToUnixTimeMilliseconds(),
                                v => (DateTimeOffset?)DateTimeOffset.FromUnixTimeMilliseconds(v)));

                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableDouble)).HasConversion(
                            new ValueConverter<double?, decimal?>(
                                v => (decimal?)v, v => (double?)v,
                                new ConverterMappingHints(precision: 26, scale: 16)));
                    });

                modelBuilder.Entity<BinaryKeyDataType>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(
                            v => new byte[] { 4, 2, 0 }.Concat(v).ToArray(),
                            v => v.Skip(3).ToArray());
                    });

                modelBuilder.Entity<StringKeyDataType>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(
                            v => "KeyValue=" + v, v => v.Substring(9));
                    });

                modelBuilder.Entity<MaxLengthDataTypes>(
                    b =>
                    {
                        b.Property(e => e.String3).HasConversion(
                            new ValueConverter<string, string>(
                                v => "KeyValue=" + v, v => v.Substring(9),
                                new ConverterMappingHints(sizeFunction: s => s + 9)));

                        b.Property(e => e.String9000).HasConversion(
                            StringToBytesConverter.DefaultInfo.Create());

                        b.Property(e => e.ByteArray5).HasConversion(
                            new ValueConverter<byte[], byte[]>(
                                v => v.Reverse().Concat(new byte[] { 4, 20 }).ToArray(),
                                v => v.Reverse().Skip(2).ToArray(),
                                new ConverterMappingHints(sizeFunction: s => s + 2)));

                        b.Property(e => e.ByteArray9000).HasConversion(
                            BytesToStringConverter.DefaultInfo.Create());
                    });
            }
        }
    }
}
