// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class CustomConvertersTestBase<TFixture> : BuiltInDataTypesTestBase<TFixture>
    where TFixture : BuiltInDataTypesTestBase<TFixture>.BuiltInDataTypesFixtureBase, new()
{
    protected CustomConvertersTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalFact]
    public virtual async Task Can_query_and_update_with_nullable_converter_on_unique_index()
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

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var drivers = await context.Set<Person>().OrderBy(p => p.Name).ToListAsync();

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

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var drivers = await context.Set<Person>().OrderBy(p => p.Name).ToListAsync();

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
    public virtual async Task Can_query_and_update_with_nullable_converter_on_primary_key()
    {
        using (var context = CreateContext())
        {
            var principal = context.Add(
                    new NullablePrincipal { Id = 1, Dependents = new List<NonNullableDependent> { new() { Id = 1 } } })
                .Entity;

            var pkEntry = context.Entry(principal).Property(e => e.Id);
            var fkEntry = context.Entry(principal.Dependents.Single()).Property(e => e.PrincipalId);

            Assert.Equal(1, fkEntry.CurrentValue);
            Assert.Equal(1, fkEntry.OriginalValue);
            Assert.Equal(1, pkEntry.CurrentValue);
            Assert.Equal(1, pkEntry.OriginalValue);

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var dependent = await context.Set<NonNullableDependent>().Include(e => e.Principal).SingleAsync();

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
    public virtual async Task Can_query_and_update_with_conversion_for_custom_type()
    {
        Guid id;
        using (var context = CreateContext())
        {
            var user = context.Set<User>().Add(
                new User(Email.Create("eeky_bear@example.com"))).Entity;

            Assert.Equal(1, await context.SaveChangesAsync());

            id = user.Id;
        }

        using (var context = CreateContext())
        {
            var user = await context.Set<User>().SingleAsync(e => e.Id == id && e.Email == "eeky_bear@example.com");

            Assert.Equal(id, user.Id);
            Assert.Equal("eeky_bear@example.com", user.Email);
        }
    }

    protected class User(Email email)
    {

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public Guid Id { get; private set; } = Guid.NewGuid();

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public Email Email { get; private set; } = email;
    }

    protected class Email
    {
        private readonly string _value;

        private Email(string value)
        {
            _value = value;
        }

        public override bool Equals(object obj)
            => _value == ((Email)obj)?._value;

        public override int GetHashCode()
            => _value.GetHashCode();

        public static Email Create(string value)
            => new(value);

        public static implicit operator string(Email email)
            => email._value;
    }

    [ConditionalFact]
    public virtual async Task Can_query_and_update_with_conversion_for_custom_struct()
    {
        using (var context = CreateContext())
        {
            var load = context.Set<Load>().Add(
                new Load { LoadId = 1, Fuel = new Fuel(1.1) }).Entity;

            Assert.Equal(1, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            var load = await context.Set<Load>().SingleAsync(e => e.LoadId == 1 && e.Fuel.Equals(new Fuel(1.1)));

            Assert.Equal(1, load.LoadId);
            Assert.Equal(1.1, load.Fuel.Volume);
        }
    }

    protected class Load
    {
        public int LoadId { get; set; }

        public Fuel Fuel { get; set; }
    }

    protected struct Fuel(double volume)
    {
        public double Volume { get; } = volume;
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_case_insensitive_string_key()
    {
        using (var context = CreateContext())
        {
            var principal = context.Set<StringKeyDataType>().Add(
                new StringKeyDataType { Id = "Gumball!!" }).Entity;

            var dependent = context.Set<StringForeignKeyDataType>().Add(
                new StringForeignKeyDataType { Id = 7767, StringKeyDataTypeId = "gumball!!" }).Entity;

            Assert.Same(principal, dependent.Principal);

            Assert.Equal(2, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            var entity = (await context
                .Set<StringKeyDataType>()
                .Include(e => e.Dependents)
                .Where(e => e.Id == "Gumball!!")
                .ToListAsync()).Single();

            Assert.Equal("Gumball!!", entity.Id);
            Assert.Equal("gumball!!", entity.Dependents.First().StringKeyDataTypeId);
        }

        using (var context = CreateContext())
        {
            var entity = (await context
                .Set<StringKeyDataType>()
                .Include(e => e.Dependents)
                .Where(e => e.Id == "gumball!!")
                .ToListAsync()).Single();

            Assert.Equal("Gumball!!", entity.Id);
            Assert.Equal("gumball!!", entity.Dependents.First().StringKeyDataTypeId);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_read_back_with_string_list()
    {
        using (var context = CreateContext())
        {
            context.Set<StringListDataType>().Add(
                new StringListDataType { Id = 1, Strings = new List<string> { "Gum", "Taffy" } });

            Assert.Equal(1, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            var entity = await context.Set<StringListDataType>().SingleAsync();

            Assert.Equal(new[] { "Gum", "Taffy" }, entity.Strings);
        }
    }

    protected class StringListDataType
    {
        public int Id { get; set; }

        public IList<string> Strings { get; set; }
    }

    [ConditionalFact]
    public virtual async Task Can_insert_and_query_struct_to_string_converter_for_pk()
    {
        using (var context = CreateContext())
        {
            context.Set<Order>().Add(new Order { Id = OrderId.Parse("Id1") });

            Assert.Equal(1, await context.SaveChangesAsync());
        }

        using (var context = CreateContext())
        {
            // Inline
            var entity = await context.Set<Order>().Where(o => (string)o.Id == "Id1").SingleAsync();

            // constant from closure
            const string idAsStringConstant = "Id1";
            entity = await context.Set<Order>().Where(o => (string)o.Id == idAsStringConstant).SingleAsync();

            // Variable from closure
            var idAsStringVariable = "Id1";
            entity = await context.Set<Order>().Where(o => (string)o.Id == idAsStringVariable).SingleAsync();

            // Inline parsing function
            entity = await context.Set<Order>().Where(o => (string)o.Id == OrderId.Parse("Id1").StringValue).SingleAsync();
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
            => new(stringValue);

        public static explicit operator string(OrderId orderId)
            => orderId.StringValue;
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Can_query_custom_type_not_mapped_by_default_equality(bool async)
    {
        using (var context = CreateContext())
        {
            context.Set<SimpleCounter>().Add(new SimpleCounter { CounterId = 1, StyleKey = "Swag" });
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var query = context.Set<SimpleCounter>()
                .Where(
                    c => c.StyleKey == "Swag"
                        && c.IsTest == false
                        && c.Discriminator == new Dictionary<string, string>());

            var result = async ? await query.SingleAsync() : query.Single();
            Assert.NotNull(result);
            context.Remove(result);
            await context.SaveChangesAsync();
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
    public virtual async Task Field_on_derived_type_retrieved_via_cast_applies_value_converter()
    {
        using var context = CreateContext();
        var query = await context.Set<Blog>()
            .Where(b => b.BlogId == 2)
            .Select(
                x => new
                {
                    x.BlogId,
                    x.Url,
                    RssUrl = x is RssBlog ? ((RssBlog)x).RssUrl : null
                }).ToListAsync();

        var result = Assert.Single(query);
        Assert.Equal("http://rssblog.com/rss", result.RssUrl);
    }

    [ConditionalFact]
    public virtual async Task Value_conversion_is_appropriately_used_for_join_condition()
    {
        using var context = CreateContext();
        var blogId = 1;
        var query = await ((from b in context.Set<Blog>()
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
                            select b.Url).ToListAsync());

        var result = Assert.Single(query);
        Assert.Equal("http://blog.com", result);
    }

    [ConditionalFact]
    public virtual async Task Value_conversion_is_appropriately_used_for_left_join_condition()
    {
        using var context = CreateContext();
        var blogId = 1;
        var query = await (from b in context.Set<Blog>()
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
                           select b.Url).ToListAsync();

        var result = Assert.Single(query);
        Assert.Equal("http://blog.com", result);
    }

    [ConditionalFact]
    public virtual async Task Where_bool_gets_converted_to_equality_when_value_conversion_is_used()
    {
        using var context = CreateContext();
        var query = await context.Set<Blog>().Where(b => b.IsVisible).ToListAsync();

        var result = Assert.Single(query);
        Assert.Equal("http://blog.com", result.Url);
    }

    [ConditionalFact]
    public virtual async Task Where_negated_bool_gets_converted_to_equality_when_value_conversion_is_used()
    {
        using var context = CreateContext();
        var query = await context.Set<Blog>().Where(b => !b.IsVisible).ToListAsync();

        var result = Assert.Single(query);
        Assert.Equal("http://rssblog.com", result.Url);
    }

    [ConditionalFact]
    public virtual async Task Where_bool_with_value_conversion_inside_comparison_doesnt_get_converted_twice()
    {
        using var context = CreateContext();
        var query1 = await context.Set<Blog>().Where(b => b.IsVisible).ToListAsync();
        var query2 = await context.Set<Blog>().Where(b => b.IsVisible != true).ToListAsync();

        var result1 = Assert.Single(query1);
        Assert.Equal("http://blog.com", result1.Url);

        var result2 = Assert.Single(query2);
        Assert.Equal("http://rssblog.com", result2.Url);
    }

    [ConditionalFact]
    public virtual async Task Select_bool_with_value_conversion_is_used()
    {
        using var context = CreateContext();
        var result = await context.Set<Blog>().Select(b => b.IsVisible).ToListAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(true, result);
        Assert.Contains(false, result);
    }

    [ConditionalFact]
    public virtual async Task Where_conditional_bool_with_value_conversion_is_used()
    {
        using var context = CreateContext();
        var query = await context.Set<Blog>().Where(b => (b.IsVisible ? "Foo" : "Bar") == "Foo").ToListAsync();

        var result = Assert.Single(query);
        Assert.Equal("http://blog.com", result.Url);
    }

    [ConditionalFact]
    public virtual async Task Select_conditional_bool_with_value_conversion_is_used()
    {
        using var context = CreateContext();
        var result = await context.Set<Blog>().Select(b => b.IsVisible ? "Foo" : "Bar").ToListAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains("Foo", result);
        Assert.Contains("Bar", result);
    }

    [ConditionalFact]
    public virtual async Task Where_bool_gets_converted_to_equality_when_value_conversion_is_used_using_EFProperty()
    {
        using var context = CreateContext();
        var query = await context.Set<Blog>().Where(b => EF.Property<bool>(b, "IsVisible")).ToListAsync();

        var result = Assert.Single(query);
        Assert.Equal("http://blog.com", result.Url);
    }

    [ConditionalFact]
    public virtual async Task Where_bool_gets_converted_to_equality_when_value_conversion_is_used_using_indexer()
    {
        using var context = CreateContext();
        var query = await context.Set<Blog>().Where(b => !(bool)b["IndexerVisible"]).ToListAsync();

        var result = Assert.Single(query);
        Assert.Equal("http://blog.com", result.Url);
    }

    [ConditionalFact]
    public virtual void Value_conversion_with_property_named_value()
    {
        using var context = CreateContext();
        Assert.Throws<InvalidOperationException>(
            () => context.Set<EntityWithValueWrapper>().SingleOrDefault(e => e.Wrapper.Value == "foo"));
    }

    [ConditionalFact]
    public virtual void Value_conversion_on_enum_collection_contains()
    {
        using var context = CreateContext();
        var group = MessageGroup.SomeGroup;
        var query = context.Set<User23059>()
            .Where(x => !x.IsSoftDeleted && (x.MessageGroups.Contains(group) || x.MessageGroups.Contains(MessageGroup.All)))
            .ToList();

        Assert.Single(query);
    }

    protected class User23059
    {
        public int Id { get; set; }
        public bool IsSoftDeleted { get; set; }
        public List<MessageGroup> MessageGroups { get; set; }
    }

    protected enum MessageGroup
    {
        All,
        SomeGroup
    }

    protected class Blog
    {
        private bool _indexerVisible;

        public int BlogId { get; set; }
        public string Url { get; set; }
        public bool IsVisible { get; set; }
        public List<Post> Posts { get; set; }

        public object this[string name]
        {
            get
            {
                if (!string.Equals(name, "IndexerVisible", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"Indexer property with key {name} is not defined on {nameof(Blog)}.");
                }

                return _indexerVisible;
            }

            set
            {
                if (!string.Equals(name, "IndexerVisible", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"Indexer property with key {name} is not defined on {nameof(Blog)}.");
                }

                _indexerVisible = (bool)value;
            }
        }
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
    public virtual void Collection_property_as_scalar_Any()
    {
        using var context = CreateContext();
        Assert.Contains(
            @"See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.",
            Assert.Throws<InvalidOperationException>(
                    () => context.Set<CollectionScalar>().Where(e => e.Tags.Any()).ToList())
                .Message.Replace("\r", "").Replace("\n", ""));
    }

    [ConditionalFact]
    public virtual void Collection_property_as_scalar_Count_member()
    {
        using var context = CreateContext();
        Assert.Equal(
            CoreStrings.TranslationFailed(
                @"DbSet<CollectionScalar>()    .Where(c => c.Tags.Count == 2)"),
            Assert.Throws<InvalidOperationException>(
                    () => context.Set<CollectionScalar>().Where(e => e.Tags.Count == 2).ToList())
                .Message.Replace("\r", "").Replace("\n", ""));
    }

    protected class CollectionScalar
    {
        public int Id { get; set; }
        public List<string> Tags { get; set; }
    }

    [ConditionalFact]
    public virtual void Collection_enum_as_string_Contains()
    {
        using var context = CreateContext();
        var sameRole = Roles.Seller;
        Assert.Contains(
            @"See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.",
            Assert.Throws<InvalidOperationException>(
                    () => context.Set<CollectionEnum>().Where(e => e.Roles.Contains(sameRole)).ToList())
                .Message.Replace("\r", "").Replace("\n", ""));
    }

    protected class CollectionEnum
    {
        public int Id { get; set; }
        public ICollection<Roles> Roles { get; set; }
    }

    protected enum Roles
    {
        Customer,
        Seller
    }

    public override Task Object_to_string_conversion()
        => Task.CompletedTask;

    [ConditionalFact]
    public virtual void Optional_owned_with_converter_reading_non_nullable_column()
    {
        using var context = CreateContext();
        Assert.Equal(
            "Nullable object must have a value.",
            Assert.Throws<InvalidOperationException>(
                () => context.Set<Parent>().Select(e => new { e.OwnedWithConverter.Value }).ToList()).Message);
    }

    protected class Parent
    {
        public int Id { get; set; }
        public OwnedWithConverter OwnedWithConverter { get; set; }
    }

    protected class OwnedWithConverter
    {
        public int Value { get; set; }
    }

    [ConditionalFact]
    public virtual async Task Id_object_as_entity_key()
    {
        using var context = CreateContext();
        var books = await context.Set<Book>().Where(b => b.Id == new BookId(1)).ToListAsync();

        Assert.Equal("Book1", Assert.Single(books).Value);
    }

    public class Book(BookId id)
    {
        public BookId Id { get; set; } = id;

        public string Value { get; set; }
    }

    public class BookId(int id)
    {
        public readonly int Id = id;

        public override bool Equals(object obj)
            => obj is BookId item && Id == item.Id;

        public override int GetHashCode()
            => Id.GetHashCode();
    }

    [ConditionalFact]
    public virtual void Composition_over_collection_of_complex_mapped_as_scalar()
    {
        using var context = CreateContext();
        Assert.Equal(
            CoreStrings.TranslationFailed(
                @"l => new {     H = l.Height,     W = l.Width }"),
            Assert.Throws<InvalidOperationException>(
                    () => context.Set<Dashboard>().AsNoTracking().Select(
                        d => new
                        {
                            d.Id,
                            d.Name,
                            Layouts = d.Layouts.Select(l => new { H = l.Height, W = l.Width }).ToList()
                        }).ToList())
                .Message.Replace("\r", "").Replace("\n", ""));
    }

    public class Dashboard
    {
        public Dashboard()
        {
            Layouts = [];
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public List<Layout> Layouts { get; set; }
    }

    public class Layout
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class HolderClass
    {
        public int Id { get; set; }
        public HoldingEnum HoldingEnum { get; set; }
    }

    public enum HoldingEnum
    {
        Value1,
        Value2
    }

    [ConditionalFact]
    public virtual void GroupBy_converted_enum()
    {
        using var context = CreateContext();
        var result = context.Set<Entity>().GroupBy(e => e.SomeEnum).ToList();

        Assert.Collection(
            result,
            t =>
            {
                Assert.Equal(SomeEnum.No, t.Key);
                Assert.Single(t);
            },
            t =>
            {
                Assert.Equal(SomeEnum.Yes, t.Key);
                Assert.Equal(2, t.Count());
            });
    }

    public class Entity
    {
        public int Id { get; set; }
        public SomeEnum SomeEnum { get; set; }
    }

    public enum SomeEnum
    {
        Yes,
        No
    }

    [ConditionalFact]
    public virtual void Infer_type_mapping_from_in_subquery_to_item()
    {
        using var context = CreateContext();
        var results = context.Set<BuiltInDataTypes>().Where(
            b =>
                context.Set<BuiltInDataTypes>().Select(bb => bb.TestBoolean).Contains(true) && b.Id == 13).ToList();

        Assert.Equal(1, results.Count);
    }

    public abstract class CustomConvertersFixtureBase : BuiltInDataTypesFixtureBase
    {
        protected override string StoreName
            => "CustomConverters";

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
            => configurationBuilder.DefaultTypeMapping<HoldingEnum>().HasConversion<string>();

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
                    b.Property(e => e.Id).HasConversion(v => v ?? 0, v => v);
                });

            modelBuilder.Entity<NonNullableDependent>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.PrincipalId).HasConversion(v => v, v => v);
                });

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
                    b.Property(e => e.TestDateOnly).HasConversion(v => v.ToShortDateString(), v => DateOnly.Parse(v));
                    b.Property(e => e.TestTimeSpan).HasConversion(v => v.TotalMilliseconds, v => TimeSpan.FromMilliseconds(v));
                    b.Property(e => e.TestTimeOnly).HasConversion(v => v.Ticks, v => new TimeOnly(v));
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
                        v => DateTimeOffset.FromUnixTimeMilliseconds(v).ToOffset(TimeSpan.FromHours(-8.0)));

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
                        v => DateTime.FromBinary(v));

                    b.Property(e => e.TestNullableDateOnly).HasConversion(
                        v => v.Value.ToShortDateString(),
                        v => DateOnly.Parse(v));

                    b.Property(e => e.TestNullableTimeSpan).HasConversion(
                        v => v.Value.TotalMilliseconds,
                        v => TimeSpan.FromMilliseconds(v));

                    b.Property(e => e.TestNullableTimeOnly).HasConversion(
                        v => v.Value.Ticks,
                        v => new TimeOnly(v));

                    b.Property(e => e.EnumS8).HasConversion(
                        v => v.ToString(),
                        v => v == nameof(EnumS8.SomeValue) ? EnumS8.SomeValue : null);

                    b.Property(e => e.Enum8).HasConversion(
                        v => v.ToString(),
                        v => v == nameof(Enum8.SomeValue) ? Enum8.SomeValue : null);

                    b.Property(e => e.TestNullableDateTimeOffset).HasConversion(
                        v => v.Value.ToUnixTimeMilliseconds(),
                        v => DateTimeOffset.FromUnixTimeMilliseconds(v).ToOffset(TimeSpan.FromHours(-8.0)));

                    b.Property(e => e.TestNullableDouble).HasConversion(
                        new ValueConverter<double?, decimal?>(
                            v => (decimal?)v, v => (double?)v,
                            new ConverterMappingHints(precision: 26, scale: 16)));
                });

            modelBuilder.Entity<BuiltInDataTypesShadow>(
                b =>
                {
                    b.Property(nameof(BuiltInDataTypes.PartitionId))
                        .HasConversion(new ValueConverter<int, long>(v => v, v => (int)v));
                    b.Property(nameof(BuiltInDataTypes.TestInt16))
                        .HasConversion(new ValueConverter<short, long>(v => v, v => (short)v));
                    b.Property(nameof(BuiltInDataTypes.TestInt32))
                        .HasConversion(new ValueConverter<int, long>(v => v, v => (int)v));
                    b.Property(nameof(BuiltInDataTypes.TestInt64)).HasConversion(new ValueConverter<long, long>(v => v, v => v));
                    b.Property(nameof(BuiltInDataTypes.TestDecimal))
                        .HasConversion(NumberToBytesConverter<decimal>.DefaultInfo.Create());
                    b.Property(nameof(BuiltInDataTypes.TestDateOnly)).HasConversion(
                        new ValueConverter<DateOnly, string>(v => v.ToShortDateString(), v => DateOnly.Parse(v)));
                    b.Property(nameof(BuiltInDataTypes.TestDateTime)).HasConversion(
                        new ValueConverter<DateTime, long>(v => v.ToBinary(), v => DateTime.FromBinary(v)));
                    b.Property(nameof(BuiltInDataTypes.TestTimeSpan)).HasConversion(
                        new ValueConverter<TimeSpan, double>(v => v.TotalMilliseconds, v => TimeSpan.FromMilliseconds(v)));
                    b.Property(nameof(BuiltInDataTypes.TestTimeOnly)).HasConversion(
                        new ValueConverter<TimeOnly, long>(v => v.Ticks, v => new TimeOnly(v)));
                    b.Property(nameof(BuiltInDataTypes.TestSingle)).HasConversion(new CastingConverter<float, double>());
                    b.Property(nameof(BuiltInDataTypes.TestBoolean)).HasConversion(new BoolToTwoValuesConverter<string>("Nope", "Yep"));
                    b.Property(nameof(BuiltInDataTypes.TestByte))
                        .HasConversion(new ValueConverter<byte, ushort>(v => v, v => (byte)v));
                    b.Property(nameof(BuiltInDataTypes.TestUnsignedInt16))
                        .HasConversion(new ValueConverter<ushort, ulong>(v => v, v => (ushort)v));
                    b.Property(nameof(BuiltInDataTypes.TestUnsignedInt32))
                        .HasConversion(new ValueConverter<uint, ulong>(v => v, v => (uint)v));
                    b.Property(nameof(BuiltInDataTypes.TestUnsignedInt64))
                        .HasConversion(new ValueConverter<ulong, long>(v => (long)v, v => (ulong)v));
                    b.Property(nameof(BuiltInDataTypes.TestCharacter))
                        .HasConversion(new ValueConverter<char, int>(v => v, v => (char)v));
                    b.Property(nameof(BuiltInDataTypes.TestSignedByte))
                        .HasConversion(new ValueConverter<sbyte, decimal>(v => v, v => (sbyte)v));
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
                            v => DateTimeOffset.FromUnixTimeMilliseconds(v).ToOffset(TimeSpan.FromHours(-8.0))));

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
                        .HasConversion(new ValueConverter<int, long>(v => v, v => (int)v));
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableInt16))
                        .HasConversion(new ValueConverter<short?, long?>(v => v, v => (short?)v));
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableInt32))
                        .HasConversion(new ValueConverter<int?, long?>(v => v, v => (int?)v));
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableInt64))
                        .HasConversion(new ValueConverter<long?, long?>(v => v, v => v));
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableDecimal))
                        .HasConversion(NumberToBytesConverter<decimal?>.DefaultInfo.Create());
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableSingle))
                        .HasConversion(new CastingConverter<float?, double?>());
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableBoolean))
                        .HasConversion(new BoolToTwoValuesConverter<string>("Nope", "Yep"));
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableByte))
                        .HasConversion(new ValueConverter<byte?, ushort?>(v => v, v => (byte?)v));
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt16))
                        .HasConversion(new ValueConverter<ushort?, ulong?>(v => v, v => (ushort?)v));
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt32))
                        .HasConversion(new ValueConverter<uint?, ulong?>(v => v, v => (uint?)v));
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt64))
                        .HasConversion(new ValueConverter<ulong?, long?>(v => (long?)v, v => (ulong?)v));
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableCharacter))
                        .HasConversion(new ValueConverter<char?, int?>(v => v, v => (char?)v));
                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableSignedByte)).HasConversion(
                        new ValueConverter<sbyte?, decimal?>(v => v, v => (sbyte?)v));
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
                            v => DateTime.FromBinary(v)));

                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableDateOnly)).HasConversion(
                        new ValueConverter<DateOnly?, string>(
                            v => v.Value.ToShortDateString(),
                            v => DateOnly.Parse(v)));

                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableTimeSpan)).HasConversion(
                        new ValueConverter<TimeSpan?, double>(
                            v => v.Value.TotalMilliseconds,
                            v => TimeSpan.FromMilliseconds(v)));

                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableTimeOnly)).HasConversion(
                        new ValueConverter<TimeOnly?, long>(
                            v => v.Value.Ticks,
                            v => new TimeOnly(v)));

                    b.Property(nameof(BuiltInNullableDataTypes.EnumS8)).HasConversion(
                        new ValueConverter<EnumS8?, string>(
                            v => v.ToString(),
                            v => v == nameof(EnumS8.SomeValue) ? EnumS8.SomeValue : null));

                    b.Property(nameof(BuiltInNullableDataTypes.Enum8)).HasConversion(
                        new ValueConverter<Enum8?, string>(
                            v => v.ToString(),
                            v => v == nameof(Enum8.SomeValue) ? Enum8.SomeValue : null));

                    b.Property(nameof(BuiltInNullableDataTypes.TestNullableDateTimeOffset)).HasConversion(
                        new ValueConverter<DateTimeOffset?, long>(
                            v => v.Value.ToUnixTimeMilliseconds(),
                            v => DateTimeOffset.FromUnixTimeMilliseconds(v).ToOffset(TimeSpan.FromHours(-8.0))));

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
                    var property = b.Property(e => e.Id)
                        .HasConversion(v => "KeyValue=" + v, v => v.Substring(9)).Metadata;
                });

            modelBuilder.Entity<StringForeignKeyDataType>(
                b =>
                {
                    b.Property(e => e.StringKeyDataTypeId)
                        .HasConversion(
                            v => "KeyValue=" + v,
                            v => v.Substring(9));
                });

            modelBuilder.Entity<MaxLengthDataTypes>(
                b =>
                {
                    var bytesComparer = new ValueComparer<byte[]>(
                        (v1, v2) => v1.SequenceEqual(v2),
                        v => v.GetHashCode());

                    b.Property(e => e.String3)
                        .HasConversion(
                            new ValueConverter<string, string>(
                                v => "KeyValue=" + v, v => v.Substring(9)))
                        .HasMaxLength(12);

                    b.Property(e => e.String9000).HasConversion(
                        StringToBytesConverter.DefaultInfo.Create());

                    b.Property(e => e.StringUnbounded).HasConversion(
                        StringToBytesConverter.DefaultInfo.Create());

                    b.Property(e => e.ByteArray5)
                        .HasConversion(
                            new ValueConverter<byte[], byte[]>(
                                v => v.Reverse().Concat(new byte[] { 4, 20 }).ToArray(),
                                v => v.Reverse().Skip(2).ToArray()),
                            bytesComparer)
                        .HasMaxLength(7);

                    b.Property(e => e.ByteArray9000)
                        .HasConversion(
                            BytesToStringConverter.DefaultInfo.Create(),
                            bytesComparer)
                        .HasMaxLength(LongStringLength * 2);
                });

            modelBuilder.Entity<StringListDataType>(
                b =>
                {
                    b.Property(e => e.Strings).HasConversion(
                        v => string.Join(",", v),
                        v => v.Split(new[] { ',' }).ToList(),
                        new ValueComparer<IList<string>>(
                            (v1, v2) => v1.SequenceEqual(v2),
                            v => v.GetHashCode()));

                    b.Property(e => e.Id).ValueGeneratedNever();
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
                        json => StringToDictionarySerializer.Deserialize(json),
                        new ValueComparer<IDictionary<string, string>>(
                            (v1, v2) => v1.SequenceEqual(v2),
                            v => v.GetHashCode(),
                            v => new Dictionary<string, string>(v)));
                });

            var urlConverter = new UrlSchemeRemover();
            modelBuilder.Entity<Blog>(
                b =>
                {
                    b.Property(e => e.Url).HasConversion(urlConverter);
                    b.Property(e => e.IsVisible).HasConversion(new BoolToStringConverter("N", "Y"));
                    b.IndexerProperty(typeof(bool), "IndexerVisible").HasConversion(new BoolToStringConverter("Nay", "Aye"));

                    b.HasData(
                        new
                        {
                            BlogId = 1,
                            Url = "http://blog.com",
                            IsVisible = true,
                            IndexerVisible = false,
                        });
                });

            modelBuilder.Entity<RssBlog>(
                b =>
                {
                    b.Property(e => e.RssUrl).HasConversion(urlConverter);
                    b.HasData(
                        new
                        {
                            BlogId = 2,
                            Url = "http://rssblog.com",
                            RssUrl = "http://rssblog.com/rss",
                            IsVisible = false,
                            IndexerVisible = true,
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
                        s => s.Split(',', StringSplitOptions.None).ToList(),
                        new ValueComparer<List<string>>(favorStructuralComparisons: true));

                    b.HasData(
                        new CollectionScalar
                        {
                            Id = 1,
                            Tags =
                            [
                                "A",
                                "B",
                                "C"
                            ]
                        });
                });

            modelBuilder.Entity<CollectionEnum>(
                b =>
                {
                    b.Property(e => e.Roles).HasConversion(
                        new RolesToStringConveter(),
                        new ValueComparer<ICollection<Roles>>(favorStructuralComparisons: true));

                    b.HasData(new CollectionEnum { Id = 1, Roles = new List<Roles> { Roles.Seller } });
                });

            modelBuilder.Entity<Parent>(
                b =>
                {
                    b.OwnsOne(
                        e => e.OwnedWithConverter,
                        ob =>
                        {
                            ob.Property(i => i.Value).HasConversion<string>();
                            ob.HasData(new { ParentId = 1, Value = 42 });
                        });

                    b.HasData(
                        new Parent { Id = 1 },
                        new Parent { Id = 2 });
                });

            modelBuilder.Entity<Book>(
                b =>
                {
                    b.HasKey(e => e.Id);
                    b.Property(e => e.Id).HasConversion(
                        e => e.Id,
                        e => new BookId(e));

                    b.HasData(new Book(new BookId(1)) { Value = "Book1" });
                });

            modelBuilder.Entity<User23059>(
                b =>
                {
                    b.Property(e => e.MessageGroups).HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => Enum.Parse<MessageGroup>(x)).ToList(),
                        new ValueComparer<List<MessageGroup>>(favorStructuralComparisons: true));

                    b.HasData(
                        new User23059
                        {
                            Id = 1,
                            IsSoftDeleted = true,
                            MessageGroups = [MessageGroup.SomeGroup]
                        },
                        new User23059
                        {
                            Id = 2,
                            IsSoftDeleted = false,
                            MessageGroups = [MessageGroup.SomeGroup]
                        });
                });

            modelBuilder.Entity<Dashboard>()
                .Property(e => e.Layouts).HasConversion(
                    v => LayoutsToStringSerializer.Serialize(v),
                    v => LayoutsToStringSerializer.Deserialize(v),
                    new ValueComparer<List<Layout>>(
                        (v1, v2) => v1.SequenceEqual(v2),
                        v => v.GetHashCode(),
                        v => new List<Layout>(v)));

            modelBuilder.Entity<HolderClass>().HasData(new HolderClass { Id = 1, HoldingEnum = HoldingEnum.Value2 });

            modelBuilder.Entity<Entity>().Property(e => e.SomeEnum).HasConversion(e => e.ToString(), e => Enum.Parse<SomeEnum>(e));
            modelBuilder.Entity<Entity>().HasData(
                new Entity { Id = 1, SomeEnum = SomeEnum.Yes },
                new Entity { Id = 2, SomeEnum = SomeEnum.No },
                new Entity { Id = 3, SomeEnum = SomeEnum.Yes });
        }

        private static class StringToDictionarySerializer
        {
            public static string Serialize(IDictionary<string, string> dictionary)
                => string.Join(Environment.NewLine, dictionary.Select(kvp => $"{{{kvp.Key},{kvp.Value}}}"));

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

        private static class LayoutsToStringSerializer
        {
            public static string Serialize(List<Layout> layouts)
                => string.Join(Environment.NewLine, layouts.Select(layout => $"({layout.Height},{layout.Width})"));

            public static List<Layout> Deserialize(string s)
            {
                var list = new List<Layout>();
                var keyValuePairs = s.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
                foreach (var keyValuePair in keyValuePairs)
                {
                    var parts = keyValuePair[1..^1].Split(",");
                    list.Add(
                        new Layout
                        {
                            Height = int.Parse(parts[0]), Width = int.Parse(parts[1]),
                        });
                }

                return list;
            }
        }

        private class OrderIdEntityFrameworkValueConverter(ConverterMappingHints mappingHints) : ValueConverter<OrderId, string>(
            orderId => orderId.StringValue,
            stringValue => OrderId.Parse(stringValue),
            mappingHints
        )
        {
            public OrderIdEntityFrameworkValueConverter()
                : this(null)
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

        private class RolesToStringConveter : ValueConverter<ICollection<Roles>, string>
        {
            public RolesToStringConveter()
                : base(
                    v => string.Join(";", v.Select(f => f.ToString())),
                    v => v.Length > 0
                        ? v.Split(new[] { ';' }).Select(f => (Roles)Enum.Parse(typeof(Roles), f)).ToList()
                        : new List<Roles>())
            {
            }
        }
    }
}
