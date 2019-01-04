// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

// ReSharper disable InconsistentNaming
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
        public virtual void Can_query_and_update_with_nullable_converter_on_unique_index()
        {
            using (var context = CreateContext())
            {
                context.AddRange(
                    new Person
                    {
                        Name = "Lewis"
                    },
                    new Person
                    {
                        Name = "Seb",
                        SSN = new SocialSecurityNumber
                        {
                            Number = 111111111
                        }
                    },
                    new Person
                    {
                        Name = "Kimi",
                        SSN = new SocialSecurityNumber
                        {
                            Number = 222222222
                        }
                    },
                    new Person
                    {
                        Name = "Valtteri"
                    });

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                var drivers = context.Set<Person>().OrderBy(p => p.Name).ToList();

                Assert.Equal(4, drivers.Count);

                Assert.Equal("Kimi", drivers[0].Name);
                Assert.Equal(222222222, drivers[0].SSN.Value.Number);

                Assert.Equal("Lewis", drivers[1].Name);
                Assert.False(drivers[1].SSN.HasValue);

                Assert.Equal("Seb", drivers[2].Name);
                Assert.Equal(111111111, drivers[2].SSN.Value.Number);

                Assert.Equal("Valtteri", drivers[3].Name);
                Assert.False(drivers[3].SSN.HasValue);

                context.Remove(drivers[0]);

                context.Add(
                    new Person
                    {
                        Name = "Charles",
                        SSN = new SocialSecurityNumber
                        {
                            Number = 222222222
                        }
                    });

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                var drivers = context.Set<Person>().OrderBy(p => p.Name).ToList();

                Assert.Equal(4, drivers.Count);

                Assert.Equal("Charles", drivers[0].Name);
                Assert.Equal(222222222, drivers[0].SSN.Value.Number);

                Assert.Equal("Lewis", drivers[1].Name);
                Assert.False(drivers[1].SSN.HasValue);

                Assert.Equal("Seb", drivers[2].Name);
                Assert.Equal(111111111, drivers[2].SSN.Value.Number);

                Assert.Equal("Valtteri", drivers[3].Name);
                Assert.False(drivers[3].SSN.HasValue);

                context.Remove(drivers[0]);
            }
        }

        protected struct SocialSecurityNumber : IEquatable<SocialSecurityNumber>
        {
            public int Number { get; set; }

            public bool Equals(SocialSecurityNumber other)
                => Number == other.Number;
        }

        protected class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public SocialSecurityNumber? SSN { get; set; }
        }

        [Fact]
        public virtual void Can_query_and_update_with_nullable_converter_on_primary_key()
        {
            using (var context = CreateContext())
            {
                var principal = context.Add(
                    new NullablePrincipal
                    {
                        Id = 1,
                        Dependents = new List<NonNullableDependent>
                        {
                            new NonNullableDependent()
                        }
                    }).Entity;

                var pkEntry = context.Entry(principal).Property(e => e.Id);
                var fkEntry = context.Entry(principal.Dependents.Single()).Property(e => e.PrincipalId);

                Assert.Equal(1, fkEntry.CurrentValue);
                Assert.Equal(1, fkEntry.OriginalValue);
                Assert.Equal(1, pkEntry.CurrentValue);
                Assert.Equal(1, pkEntry.OriginalValue);

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                var dependent = context.Set<NonNullableDependent>().Include(e => e.Principal).Single();

                Assert.Equal(1, dependent.PrincipalId);
                Assert.Equal(1, dependent.Principal.Id);

                var fkEntry = context.Entry(dependent).Property(e => e.PrincipalId);
                var pkEntry = context.Entry(dependent.Principal).Property(e => e.Id);

                Assert.Equal(1, fkEntry.CurrentValue);
                Assert.Equal(1, fkEntry.OriginalValue);
                Assert.Equal(1, pkEntry.CurrentValue);
                Assert.Equal(1, pkEntry.OriginalValue);
            }
        }

        protected class NullablePrincipal
        {
            public int? Id { get; set; }

            public ICollection<NonNullableDependent> Dependents { get; set; }
        }

        protected class NonNullableDependent
        {
            public int Id { get; set; }

            public int PrincipalId { get; set; }
            public NullablePrincipal Principal { get; set; }
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

        [Fact]
        public virtual void Can_query_and_update_with_conversion_for_custom_struct()
        {
            int id;
            using (var context = CreateContext())
            {
                var load = context.Set<Load>().Add(
                    new Load { Fuel = new Fuel(1.1) }).Entity;

                Assert.Equal(1, context.SaveChanges());

                id = load.LoadId;
            }

            using (var context = CreateContext())
            {
                var load = context.Set<Load>().Single(e => e.LoadId == id && e.Fuel.Equals(new Fuel(1.1)));

                Assert.Equal(id, load.LoadId);
                Assert.Equal(1.1, load.Fuel.Volume);
            }
        }

        protected class Load
        {
            public int LoadId { get; private set; }

            public Fuel Fuel { get; set; }
        }

        protected struct Fuel
        {
            public Fuel(double volume) => Volume = volume;
            public double Volume { get; }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_with_case_insensitive_string_key()
        {
            using (var context = CreateContext())
            {
                var principal = context.Set<StringKeyDataType>().Add(
                    new StringKeyDataType
                    {
                        Id = "Gumball!!"
                    }).Entity;

                var dependent = context.Set<StringForeignKeyDataType>().Add(
                    new StringForeignKeyDataType
                    {
                        Id = 7767,
                        StringKeyDataTypeId = "gumball!!"
                    }).Entity;

                Assert.Same(principal, dependent.Principal);

                Assert.Equal(2, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context
                    .Set<StringKeyDataType>()
                    .Include(e => e.Dependents)
                    .Where(e => e.Id == "Gumball!!")
                    .ToList().Single();

                Assert.Equal("Gumball!!", entity.Id);
                Assert.Equal("gumball!!", entity.Dependents.First().StringKeyDataTypeId);
            }

            using (var context = CreateContext())
            {
                var entity = context
                    .Set<StringKeyDataType>()
                    .Include(e => e.Dependents)
                    .Where(e => e.Id == "gumball!!")
                    .ToList().Single();

                Assert.Equal("Gumball!!", entity.Id);
                Assert.Equal("gumball!!", entity.Dependents.First().StringKeyDataTypeId);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_with_string_list()
        {
            using (var context = CreateContext())
            {
                context.Set<StringListDataType>().Add(
                    new StringListDataType
                    {
                        Strings = new List<string>
                        {
                            "Gum",
                            "Taffy"
                        }
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<StringListDataType>().Single();

                Assert.Equal(new[] { "Gum", "Taffy" }, entity.Strings);
            }
        }

        protected class StringListDataType
        {
            public int Id { get; set; }

            public IList<string> Strings { get; set; }
        }

        public abstract class CustomConvertersFixtureBase : BuiltInDataTypesFixtureBase
        {
            protected override string StoreName { get; } = "CustomConverters";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.Entity<Person>()
                    .Property(p => p.SSN)
                    .HasConversion(
                        ssn => ssn.HasValue
                            ? ssn.Value.Number
                            : new int?(),
                        i => i.HasValue
                            ? new SocialSecurityNumber
                            {
                                Number = i.Value
                            }
                            : new SocialSecurityNumber?());

                modelBuilder.Entity<Person>()
                    .HasIndex(p => p.SSN)
                    .IsUnique();

                modelBuilder.Entity<NullablePrincipal>(
                    b =>
                    {
                        b.HasMany(e => e.Dependents).WithOne(e => e.Principal).HasForeignKey(e => e.PrincipalId);
                        b.Property(e => e.Id).ValueGeneratedNever();
                        b.Property(e => e.Id).HasConversion(v => v, v => (int)v);
                    });

                modelBuilder.Entity<User>(
                    b =>
                    {
                        b.Property(x => x.Email).HasConversion(email => (string)email, value => Email.Create(value));
                        b.Property(e => e.Id).ValueGeneratedNever();
                    });

                modelBuilder.Entity<Load>(
                    b => b.Property(x => x.Fuel).HasConversion(f => f.Volume, v => new Fuel(v)));

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
                        b.Property(e => e.TestBoolean).HasConversion(new BoolToTwoValuesConverter<string>("Nope", "Yeps")).HasMaxLength(4);
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

                        b.Property(e => e.EnumS8)
                            .HasConversion(
                                v => v.ToString(),
                                v => v == nameof(EnumS8.SomeValue) ? EnumS8.SomeValue : default)
                            .HasMaxLength(24);

                        b.Property(e => e.Enum8).HasConversion(
                                v => v.ToString(),
                                v => v == nameof(Enum8.SomeValue) ? Enum8.SomeValue : default)
                            .IsUnicode(false);

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

                var caseInsensitiveComparer = new ValueComparer<string>(
                    (l, r) => (l == null || r == null) ? (l == r) : l.Equals(r, StringComparison.InvariantCultureIgnoreCase),
                    v => StringComparer.InvariantCultureIgnoreCase.GetHashCode(v),
                    v => v);

                modelBuilder.Entity<StringKeyDataType>(
                    b =>
                    {
                        var property = b.Property(e => e.Id)
                            .HasConversion(v => "KeyValue=" + v, v => v.Substring(9)).Metadata;

                        property.SetKeyValueComparer(caseInsensitiveComparer);
                    });

                modelBuilder.Entity<StringForeignKeyDataType>(
                    b =>
                    {
                        var property = b.Property(e => e.StringKeyDataTypeId)
                            .HasConversion(v => "KeyValue=" + v, v => v.Substring(9)).Metadata;

                        property.SetKeyValueComparer(caseInsensitiveComparer);
                    });

                modelBuilder.Entity<MaxLengthDataTypes>(
                    b =>
                    {
                        b.Property(e => e.String3)
                            .HasConversion(
                                new ValueConverter<string, string>(
                                    v => "KeyValue=" + v, v => v.Substring(9)))
                            .HasMaxLength(12);

                        b.Property(e => e.String9000).HasConversion(
                            StringToBytesConverter.DefaultInfo.Create());

                        b.Property(e => e.ByteArray5)
                            .HasConversion(
                                new ValueConverter<byte[], byte[]>(
                                    v => v.Reverse().Concat(new byte[] { 4, 20 }).ToArray(),
                                    v => v.Reverse().Skip(2).ToArray()))
                            .HasMaxLength(7);

                        b.Property(e => e.ByteArray9000)
                            .HasConversion(
                                BytesToStringConverter.DefaultInfo.Create())
                            .HasMaxLength(LongStringLength * 2);
                    });

                modelBuilder.Entity<StringListDataType>(b => b.Property(e => e.Strings).HasConversion(v => string.Join(",", v), v => v.Split(new[] { ',' }).ToList()));
            }
        }
    }
}
