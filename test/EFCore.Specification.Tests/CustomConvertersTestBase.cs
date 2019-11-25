// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        [ConditionalFact]
        public virtual void Can_query_and_update_with_nullable_converter_on_unique_index()
        {
            using (var context = CreateContext())
            {
                context.AddRange(
                    new Person { Id = 1, Name = "Lewis" },
                    new Person
                    {
                        Id = 2,
                        Name = "Seb",
                        SSN = new SocialSecurityNumber { Number = 111111111 }
                    },
                    new Person
                    {
                        Id = 3,
                        Name = "Kimi",
                        SSN = new SocialSecurityNumber { Number = 222222222 }
                    },
                    new Person { Id = 4, Name = "Valtteri" });

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
                        Id = 5,
                        Name = "Charles",
                        SSN = new SocialSecurityNumber { Number = 222222222 }
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

        [ConditionalFact]
        public virtual void Can_query_and_update_with_nullable_converter_on_primary_key()
        {
            using (var context = CreateContext())
            {
                var principal = context.Add(
                        new NullablePrincipal
                        {
                            Id = 1, Dependents = new List<NonNullableDependent> { new NonNullableDependent { Id = 1 } }
                        })
                    .Entity;

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

        [ConditionalFact]
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

        [ConditionalFact]
        public virtual void Can_query_and_update_with_conversion_for_custom_struct()
        {
            using (var context = CreateContext())
            {
                var load = context.Set<Load>().Add(
                    new Load { LoadId = 1, Fuel = new Fuel(1.1) }).Entity;

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var load = context.Set<Load>().Single(e => e.LoadId == 1 && e.Fuel.Equals(new Fuel(1.1)));

                Assert.Equal(1, load.LoadId);
                Assert.Equal(1.1, load.Fuel.Volume);
            }
        }

        protected class Load
        {
            public int LoadId { get; set; }

            public Fuel Fuel { get; set; }
        }

        protected struct Fuel
        {
            public Fuel(double volume) => Volume = volume;
            public double Volume { get; }
        }

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_with_case_insensitive_string_key()
        {
            using (var context = CreateContext())
            {
                var principal = context.Set<StringKeyDataType>().Add(
                    new StringKeyDataType { Id = "Gumball!!" }).Entity;

                var dependent = context.Set<StringForeignKeyDataType>().Add(
                    new StringForeignKeyDataType { Id = 7767, StringKeyDataTypeId = "gumball!!" }).Entity;

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

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_with_string_list()
        {
            using (var context = CreateContext())
            {
                context.Set<StringListDataType>().Add(
                    new StringListDataType { Id = 1, Strings = new List<string> { "Gum", "Taffy" } });

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

        [ConditionalFact]
        public virtual void Can_insert_and_query_struct_to_string_converter_for_pk()
        {
            using (var context = CreateContext())
            {
                context.Set<Order>().Add(new Order { Id = OrderId.Parse("Id1") });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                // Inline
                var entity = context.Set<Order>().Where(o => (string)o.Id == "Id1").Single();

                // constant from closure
                const string idAsStringConstant = "Id1";
                entity = context.Set<Order>().Where(o => (string)o.Id == idAsStringConstant).Single();

                // Variable from closure
                var idAsStringVariable = "Id1";
                entity = context.Set<Order>().Where(o => (string)o.Id == idAsStringVariable).Single();

                // Inline parsing function
                entity = context.Set<Order>().Where(o => (string)o.Id == OrderId.Parse("Id1").StringValue).Single();
            }
        }

        public class Order
        {
            public OrderId Id { get; set; }
        }

        public struct OrderId
        {
            private OrderId(string stringValue)
            {
                StringValue = stringValue;
            }

            public string StringValue { get; }

            public static OrderId Parse(string stringValue)
            {
                return new OrderId(stringValue);
            }

            public static explicit operator string(OrderId orderId) => orderId.StringValue;
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Can_query_custom_type_not_mapped_by_default_equality(bool isAsync)
        {
            using (var context = CreateContext())
            {
                context.Set<SimpleCounter>().Add(new SimpleCounter { CounterId = 1, StyleKey = "Swag" });
                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                var query = context.Set<SimpleCounter>()
                    .Where(
                        c => c.StyleKey == "Swag"
                            && c.IsTest == false
                            && c.Discriminator == new Dictionary<string, string>());

                var result = isAsync ? await query.SingleAsync() : query.Single();
                Assert.NotNull(result);
                context.Remove(result);
                context.SaveChanges();
            }
        }

        public class SimpleCounter
        {
            public int CounterId { get; set; }
            public string StyleKey { get; set; }
            public bool IsTest { get; set; }
            public IDictionary<string, string> Discriminator { get; set; } = new Dictionary<string, string>();
        }

        [ConditionalFact]
        public virtual void Field_on_derived_type_retrieved_via_cast_applies_value_converter()
        {
            using (var context = CreateContext())
            {
                var query = context.Set<Blog>()
                    .Where(b => b.BlogId == 2)
                    .Select(
                        x => new
                        {
                            x.BlogId,
                            x.Url,
                            RssUrl = x is RssBlog ? ((RssBlog)x).RssUrl : null
                        }).ToList();

                var result = Assert.Single(query);
                Assert.Equal("http://rssblog.com/rss", result.RssUrl);
            }
        }

        [ConditionalFact]
        public virtual void Value_conversion_is_appropriately_used_for_join_condition()
        {
            using (var context = CreateContext())
            {
                var blogId = 1;
                var query = (from b in context.Set<Blog>()
                             join p in context.Set<Post>()
                                 on new
                                 {
                                     BlogId = (int?)b.BlogId,
                                     b.IsVisible,
                                     AnotherId = b.BlogId
                                 }
                                 equals new
                                 {
                                     p.BlogId,
                                     IsVisible = true,
                                     AnotherId = blogId
                                 }
                             where b.IsVisible
                             select b.Url).ToList();

                var result = Assert.Single(query);
                Assert.Equal("http://blog.com", result);
            }
        }

        [ConditionalFact]
        public virtual void Value_conversion_is_appropriately_used_for_left_join_condition()
        {
            using (var context = CreateContext())
            {
                var blogId = 1;
                var query = (from b in context.Set<Blog>()
                             join p in context.Set<Post>()
                                 on new
                                 {
                                     BlogId = (int?)b.BlogId,
                                     b.IsVisible,
                                     AnotherId = b.BlogId
                                 }
                                 equals new
                                 {
                                     p.BlogId,
                                     IsVisible = true,
                                     AnotherId = blogId
                                 } into g
                             from p in g.DefaultIfEmpty()
                             where b.IsVisible
                             select b.Url).ToList();

                var result = Assert.Single(query);
                Assert.Equal("http://blog.com", result);
            }
        }

        [ConditionalFact]
        public virtual void Where_bool_gets_converted_to_equality_when_value_conversion_is_used()
        {
            using (var context = CreateContext())
            {
                var query = context.Set<Blog>().Where(b => b.IsVisible).ToList();

                var result = Assert.Single(query);
                Assert.Equal("http://blog.com", result.Url);
            }
        }

        [ConditionalFact]
        public virtual void Value_conversion_with_property_named_value()
        {
            using var context = CreateContext();
            Assert.Throws<InvalidOperationException>(
                () => context.Set<EntityWithValueWrapper>().SingleOrDefault(e => e.Wrapper.Value == "foo"));
        }

        protected class Blog
        {
            public int BlogId { get; set; }
            public string Url { get; set; }
            public bool IsVisible { get; set; }
            public List<Post> Posts { get; set; }
        }

        protected class RssBlog : Blog
        {
            public string RssUrl { get; set; }
        }

        protected class Post
        {
            public int PostId { get; set; }
            public int? BlogId { get; set; }
            public Blog Blog { get; set; }
        }

        protected class EntityWithValueWrapper
        {
            public int Id { get; set; }
            public ValueWrapper Wrapper { get; set; }
        }

        protected class ValueWrapper
        {
            public string Value { get; set; }
        }

        [ConditionalFact]
        public virtual void Collection_property_as_scalar()
        {
            using var context = CreateContext();
            Assert.Equal(
                @"The LINQ expression 'DbSet<CollectionScalar>    .Where(c => c.Tags        .Any())' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to either AsEnumerable(), AsAsyncEnumerable(), ToList(), or ToListAsync(). See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.",
                Assert.Throws<InvalidOperationException>(
                    () => context.Set<CollectionScalar>().Where(e => e.Tags.Any()).ToList())
                    .Message.Replace("\r","").Replace("\n",""));
        }

        protected class CollectionScalar
        {
            public int Id { get; set; }
            public List<string> Tags { get; set; }
        }

        public abstract class CustomConvertersFixtureBase : BuiltInDataTypesFixtureBase
        {
            protected override string StoreName { get; } = "CustomConverters";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.Entity<Person>(
                    b =>
                    {
                        b.Property(p => p.SSN)
                            .HasConversion(
                                ssn => ssn.HasValue
                                    ? ssn.Value.Number
                                    : new int?(),
                                i => i.HasValue
                                    ? new SocialSecurityNumber { Number = i.Value }
                                    : new SocialSecurityNumber?());

                        b.Property(p => p.Id).ValueGeneratedNever();
                        b.HasIndex(p => p.SSN)
                            .IsUnique();
                    });

                modelBuilder.Entity<NullablePrincipal>(
                    b =>
                    {
                        b.HasMany(e => e.Dependents).WithOne(e => e.Principal).HasForeignKey(e => e.PrincipalId);
                        b.Property(e => e.Id).ValueGeneratedNever();
                        b.Property(e => e.Id).HasConversion(v => v, v => (int)v);
                    });

                modelBuilder.Entity<NonNullableDependent>(
                    b => b.Property(e => e.Id).ValueGeneratedNever());

                modelBuilder.Entity<User>(
                    b =>
                    {
                        b.Property(x => x.Email).HasConversion(email => (string)email, value => Email.Create(value));
                        b.Property(e => e.Id).ValueGeneratedNever();
                    });

                modelBuilder.Entity<Load>(
                    b =>
                    {
                        b.Property(x => x.Fuel).HasConversion(f => f.Volume, v => new Fuel(v));
                        b.Property(e => e.LoadId).ValueGeneratedNever();
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
                        b.Property(nameof(BuiltInDataTypes.PartitionId))
                            .HasConversion(new ValueConverter<int, long>(v => (long)v, v => (int)v));
                        b.Property(nameof(BuiltInDataTypes.TestInt16))
                            .HasConversion(new ValueConverter<short, long>(v => (long)v, v => (short)v));
                        b.Property(nameof(BuiltInDataTypes.TestInt32))
                            .HasConversion(new ValueConverter<int, long>(v => (long)v, v => (int)v));
                        b.Property(nameof(BuiltInDataTypes.TestInt64)).HasConversion(new ValueConverter<long, long>(v => v, v => v));
                        b.Property(nameof(BuiltInDataTypes.TestDecimal))
                            .HasConversion(NumberToBytesConverter<decimal>.DefaultInfo.Create());
                        b.Property(nameof(BuiltInDataTypes.TestDateTime)).HasConversion(
                            new ValueConverter<DateTime, long>(v => v.ToBinary(), v => DateTime.FromBinary(v)));
                        b.Property(nameof(BuiltInDataTypes.TestTimeSpan)).HasConversion(
                            new ValueConverter<TimeSpan, double>(v => v.TotalMilliseconds, v => TimeSpan.FromMilliseconds(v)));
                        b.Property(nameof(BuiltInDataTypes.TestSingle)).HasConversion(new CastingConverter<float, double>());
                        b.Property(nameof(BuiltInDataTypes.TestBoolean)).HasConversion(new BoolToTwoValuesConverter<string>("Nope", "Yep"));
                        b.Property(nameof(BuiltInDataTypes.TestByte))
                            .HasConversion(new ValueConverter<byte, ushort>(v => (ushort)v, v => (byte)v));
                        b.Property(nameof(BuiltInDataTypes.TestUnsignedInt16))
                            .HasConversion(new ValueConverter<ushort, ulong>(v => (ulong)v, v => (ushort)v));
                        b.Property(nameof(BuiltInDataTypes.TestUnsignedInt32))
                            .HasConversion(new ValueConverter<uint, ulong>(v => (ulong)v, v => (uint)v));
                        b.Property(nameof(BuiltInDataTypes.TestUnsignedInt64))
                            .HasConversion(new ValueConverter<ulong, long>(v => (long)v, v => (ulong)v));
                        b.Property(nameof(BuiltInDataTypes.TestCharacter))
                            .HasConversion(new ValueConverter<char, int>(v => (int)v, v => (char)v));
                        b.Property(nameof(BuiltInDataTypes.TestSignedByte))
                            .HasConversion(new ValueConverter<sbyte, decimal>(v => (decimal)v, v => (sbyte)v));
                        b.Property(nameof(BuiltInDataTypes.Enum64))
                            .HasConversion(new ValueConverter<Enum64, long>(v => (long)v, v => (Enum64)v));
                        b.Property(nameof(BuiltInDataTypes.Enum32))
                            .HasConversion(new ValueConverter<Enum32, long>(v => (long)v, v => (Enum32)v));
                        b.Property(nameof(BuiltInDataTypes.Enum16))
                            .HasConversion(new ValueConverter<Enum16, long>(v => (long)v, v => (Enum16)v));
                        b.Property(nameof(BuiltInDataTypes.EnumU64))
                            .HasConversion(new ValueConverter<EnumU64, ulong>(v => (ulong)v, v => (EnumU64)v));
                        b.Property(nameof(BuiltInDataTypes.EnumU32))
                            .HasConversion(new ValueConverter<EnumU32, ulong>(v => (ulong)v, v => (EnumU32)v));
                        b.Property(nameof(BuiltInDataTypes.EnumU16))
                            .HasConversion(new ValueConverter<EnumU16, ulong>(v => (ulong)v, v => (EnumU16)v));

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
                        b.Property(nameof(BuiltInNullableDataTypes.PartitionId))
                            .HasConversion(new ValueConverter<int, long>(v => (long)v, v => (int)v));
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableInt16))
                            .HasConversion(new ValueConverter<short?, long?>(v => (long?)v, v => (short?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableInt32))
                            .HasConversion(new ValueConverter<int?, long?>(v => (long?)v, v => (int?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableInt64))
                            .HasConversion(new ValueConverter<long?, long?>(v => v, v => v));
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableDecimal))
                            .HasConversion(NumberToBytesConverter<decimal?>.DefaultInfo.Create());
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableSingle))
                            .HasConversion(new CastingConverter<float?, double?>());
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableBoolean))
                            .HasConversion(new BoolToTwoValuesConverter<string>("Nope", "Yep"));
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableByte))
                            .HasConversion(new ValueConverter<byte?, ushort?>(v => (ushort?)v, v => (byte?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt16))
                            .HasConversion(new ValueConverter<ushort?, ulong?>(v => (ulong?)v, v => (ushort?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt32))
                            .HasConversion(new ValueConverter<uint?, ulong?>(v => (ulong?)v, v => (uint?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt64))
                            .HasConversion(new ValueConverter<ulong?, long?>(v => (long?)v, v => (ulong?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableCharacter))
                            .HasConversion(new ValueConverter<char?, int?>(v => (int?)v, v => (char?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.TestNullableSignedByte)).HasConversion(
                            new ValueConverter<sbyte?, decimal?>(v => (decimal?)v, v => (sbyte?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.Enum64))
                            .HasConversion(new ValueConverter<Enum64?, long?>(v => (long?)v, v => (Enum64?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.Enum32))
                            .HasConversion(new ValueConverter<Enum32?, long?>(v => (long?)v, v => (Enum32?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.Enum16))
                            .HasConversion(new ValueConverter<Enum16?, long?>(v => (long?)v, v => (Enum16?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.EnumU64))
                            .HasConversion(new ValueConverter<EnumU64?, ulong?>(v => (ulong?)v, v => (EnumU64?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.EnumU32))
                            .HasConversion(new ValueConverter<EnumU32?, ulong?>(v => (ulong?)v, v => (EnumU32?)v));
                        b.Property(nameof(BuiltInNullableDataTypes.EnumU16))
                            .HasConversion(new ValueConverter<EnumU16?, ulong?>(v => (ulong?)v, v => (EnumU16?)v));

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

                        var bytesComparer = new ValueComparer<byte[]>(
                            (v1, v2) => v1.SequenceEqual(v2),
                            v => v.GetHashCode());

                        b.Property(e => e.ByteArray5).Metadata.SetValueComparer(bytesComparer);
                        b.Property(e => e.ByteArray9000).Metadata.SetValueComparer(bytesComparer);
                    });

                modelBuilder.Entity<StringListDataType>(
                    b =>
                    {
                        b.Property(e => e.Strings).HasConversion(v => string.Join(",", v), v => v.Split(new[] { ',' }).ToList());
                        b.Property(e => e.Id).ValueGeneratedNever();

                        var comparer = new ValueComparer<IList<string>>(
                            (v1, v2) => v1.SequenceEqual(v2),
                            v => v.GetHashCode());

                        b.Property(e => e.Strings).Metadata.SetValueComparer(comparer);
                    });

                modelBuilder.Entity<Order>(
                    b =>
                    {
                        b.HasKey(o => o.Id);
                        b.Property(o => o.Id).HasConversion(new OrderIdEntityFrameworkValueConverter());
                    });

                modelBuilder.Entity<SimpleCounter>(
                    b =>
                    {
                        b.Property(e => e.CounterId).ValueGeneratedNever();
                        b.HasKey(c => c.CounterId);
                        b.Property(c => c.Discriminator).HasConversion(
                            d => StringToDictionarySerializer.Serialize(d),
                            json => StringToDictionarySerializer.Deserialize(json));

                        var comparer = new ValueComparer<IDictionary<string, string>>(
                            (v1, v2) => v1.SequenceEqual(v2),
                            v => v.GetHashCode());

                        b.Property(e => e.Discriminator).Metadata.SetValueComparer(comparer);
                    });

                var urlConverter = new UrlSchemeRemover();
                modelBuilder.Entity<Blog>(
                    b =>
                    {
                        b.Property(e => e.Url).HasConversion(urlConverter);
                        b.Property(e => e.IsVisible).HasConversion(new BoolToStringConverter("N", "Y"));
                        b.HasData(
                            new Blog
                            {
                                BlogId = 1,
                                Url = "http://blog.com",
                                IsVisible = true
                            });
                    });

                modelBuilder.Entity<RssBlog>(
                    b =>
                    {
                        b.Property(e => e.RssUrl).HasConversion(urlConverter);
                        b.HasData(
                            new RssBlog
                            {
                                BlogId = 2,
                                Url = "http://rssblog.com",
                                RssUrl = "http://rssblog.com/rss",
                                IsVisible = false
                            });
                    });

                modelBuilder.Entity<Post>()
                    .HasData(
                        new Post { PostId = 1, BlogId = 1 },
                        new Post { PostId = 2, BlogId = null });

                modelBuilder.Entity<EntityWithValueWrapper>(
                    e =>
                    {
                        e.Property(e => e.Wrapper).HasConversion
                        (
                            w => w.Value,
                            v => new ValueWrapper { Value = v }
                        );
                        e.HasData(new EntityWithValueWrapper { Id = 1, Wrapper = new ValueWrapper { Value = "foo" } });
                    });

                modelBuilder.Entity<CollectionScalar>(
                    b =>
                    {
                        b.Property(e => e.Tags).HasConversion(
                            c => string.Join(",", c),
                            s => s.Split(',', StringSplitOptions.None).ToList()).Metadata
                            .SetValueComparer(new ListOfStringComparer());

                        b.HasData(new CollectionScalar
                        {
                            Id = 1,
                            Tags = new List<string> { "A", "B", "C" }
                        });
                    });
            }

            private class ListOfStringComparer : ValueComparer<List<string>>
            {
                public ListOfStringComparer()
                    : base(favorStructuralComparisons: true)
                {
                }
            }

            private static class StringToDictionarySerializer
            {
                public static string Serialize(IDictionary<string, string> dictionary)
                {
                    return string.Join(Environment.NewLine, dictionary.Select(kvp => $"{{{kvp.Key},{kvp.Value}}}"));
                }

                public static IDictionary<string, string> Deserialize(string s)
                {
                    var dictionary = new Dictionary<string, string>();
                    var keyValuePairs = s.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var keyValuePair in keyValuePairs)
                    {
                        var parts = keyValuePair[1..^1].Split(",");
                        dictionary[parts[0]] = parts[1];
                    }

                    return dictionary;
                }
            }

            private class OrderIdEntityFrameworkValueConverter : ValueConverter<OrderId, string>
            {
                public OrderIdEntityFrameworkValueConverter()
                    : this(null)
                {
                }

                public OrderIdEntityFrameworkValueConverter(ConverterMappingHints mappingHints)
                    : base(
                        orderId => orderId.StringValue,
                        stringValue => OrderId.Parse(stringValue),
                        mappingHints
                    )
                {
                }
            }

            private class UrlSchemeRemover : ValueConverter<string, string>
            {
                public UrlSchemeRemover()
                    : base(x => x.Remove(0, 7), x => "http://" + x)
                {
                }
            }
        }
    }
}
