// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class OwnedComplexPropertiesQueryTestBase(NonSharedFixture fixture)
    : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string NonSharedStoreName
        => "OwnedComplexPropertiesQueryTests";

    protected void ClearLog()
        => ListLoggerFactory.Clear();

    [Fact]
    public virtual async Task Owned_reference()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.OwnedCollection);
                b.OwnsOne(e => e.Owned, ob => ob.ComplexProperty(o => o.Address));
            }),
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners.OrderBy(e => e.Id).ToListAsync();

        AssertOwners(owners);
    }

    [Fact]
    public virtual async Task Owned_collection()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.Owned);
                b.OwnsMany(
                    e => e.OwnedCollection, ob =>
                    {
                        ConfigureOwnedCollection(ob);
                        ob.ComplexProperty(o => o.Address);
                    });
            }),
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners.OrderBy(e => e.Id).ToListAsync();

        AssertOwnedCollections(owners);
    }

    [Fact]
    public virtual async Task Filter_on_complex_property_of_owned_reference()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: ConfigureOwnedReference,
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owner = await context.Owners.SingleAsync(e => e.Owned.Address.Country.Code == "DE");

        Assert.Equal(2, owner.Id);
    }

    [Fact]
    public virtual async Task Project_complex_property_of_owned_reference()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: ConfigureOwnedReference,
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var addresses = await context.Owners.OrderBy(e => e.Id).Select(e => e.Owned.Address).ToListAsync();

        Assert.Collection(
            addresses,
            a => Assert.Equal("Sydney", a.City),
            a => Assert.Equal("Berlin", a.City));
    }

    [Fact]
    public virtual async Task Project_nested_complex_property_of_owned_reference()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: ConfigureOwnedReference,
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var countries = await context.Owners.OrderBy(e => e.Id).Select(e => e.Owned.Address.Country).ToListAsync();

        Assert.Collection(
            countries,
            c => Assert.Equal("AU", c.Code),
            c => Assert.Equal("DE", c.Code));
    }

    [Fact]
    public virtual Task Owned_reference_on_TPH_owner()
        => AssertOwnedReferencesWithDerivedOwner(b => { });

    [Fact]
    public virtual async Task Optional_owned_reference_with_only_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb =>
            {
                mb.Ignore<Owner>();
                mb.Entity<OwnerOfOptional>(b => b.OwnsOne(e => e.Holder, ob => ob.ComplexProperty(o => o.Address)));
            },
            seed: context =>
            {
                context.AddRange(
                    new OwnerOfOptional
                    {
                        Id = 1,
                        Holder = new AddressHolder { Address = CreateOwned("", "1 Main St", "Sydney", "Australia", "AU").Address }
                    },
                    new OwnerOfOptional { Id = 2 });

                return context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Set<OwnerOfOptional>().OrderBy(e => e.Id).ToListAsync();

        Assert.Collection(
            owners,
            o => Assert.Equal("Sydney", o.Holder!.Address.City),
            o => Assert.Null(o.Holder));
    }

    [Fact]
    public virtual Task Update_complex_property_of_owned_reference()
        => AssertUpdateOwnedReference(ConfigureOwnedReference, nested: true);

    [Fact]
    public virtual Task Update_complex_property_of_owned_collection_element()
        => AssertUpdateOwnedCollectionElement(
            mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.Owned);
                b.OwnsMany(
                    e => e.OwnedCollection, ob =>
                    {
                        ConfigureOwnedCollection(ob);
                        ob.ComplexProperty(o => o.Address);
                    });
            }));

    [Fact]
    public virtual async Task Replace_complex_property_of_owned_reference()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: ConfigureOwnedReference,
            seed: Seed);

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);
            owner.Owned.Address = CreateOwned("", "5 Queen St", "Brisbane", "Australia", "AU").Address;

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);

            Assert.Equal("5 Queen St", owner.Owned.Address.Street);
            Assert.Equal("Brisbane", owner.Owned.Address.City);
        }
    }

    [Fact]
    public virtual async Task Replace_owned_reference_with_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: ConfigureOwnedReference,
            seed: Seed);

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);
            owner.Owned = CreateOwned("Owned1New", "5 Queen St", "Brisbane", "Australia", "AU");

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);

            Assert.Equal("Owned1New", owner.Owned.Name);
            Assert.Equal("Brisbane", owner.Owned.Address.City);
        }
    }

    [Fact]
    public virtual Task Value_converter_in_complex_type_on_owned_reference()
        => AssertValueConverterInComplexType(configureOwned: null, configureTag: null);

    [Fact]
    public virtual Task Value_comparer_in_complex_type_on_owned_reference()
        => AssertValueComparerInComplexType(configureOwned: null, configureTag: null);

    [Fact]
    public virtual async Task Delete_owner_with_owned_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: ConfigureOwnedReference,
            seed: Seed);

        using (var context = contextFactory.CreateDbContext())
        {
            context.Remove(await context.Owners.SingleAsync(e => e.Id == 1));

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            Assert.Equal(2, (await context.Owners.SingleAsync()).Id);
        }
    }

    protected async Task AssertValueConverterInComplexType(
        Action<OwnedNavigationBuilder<OwnerWithTag, TaggedOwned>>? configureOwned,
        Action<ComplexPropertyBuilder<Tag>>? configureTag)
    {
        var contextFactory = await InitializeTaggedOwners(
            configureOwned,
            tb =>
            {
                configureTag?.Invoke(tb);
                tb.PrimitiveCollection(t => t.RelatedIds).ElementType().HasConversion<TagIdConverter>();
            },
            c =>
            {
                c.Properties<TagId>().HaveConversion<TagIdConverter>();
                c.Properties<TagCode>().HaveConversion<TagCodeConverter>();
            },
            tb => tb.PrimitiveCollection(t => t.RelatedIds).ElementType().HasConversion<TagIdConverter>());

        using (var context = contextFactory.CreateDbContext())
        {
            var owners = await context.Set<OwnerWithTag>().OrderBy(e => e.Id).ToListAsync();
            Assert.Equal([new TagId(1), new TagId(2)], owners.Select(o => o.Owned.Tag.Id));
            Assert.Equal([new TagCode("A"), new TagCode("B")], owners.Select(o => o.Owned.Tag.Code));
            Assert.Equal([new TagId(5), new TagId(6)], owners[0].Owned.Tag.RelatedIds);
            Assert.Equal([new TagId(10), new TagId(20)], owners.Select(o => o.Owned.Tags.Single().Id));

            var owner = await context.Set<OwnerWithTag>().SingleAsync(e => e.Owned.Tag.Id == new TagId(2));
            Assert.Equal(2, owner.Id);

            owner = await context.Set<OwnerWithTag>().SingleAsync(e => e.Owned.Tag.Code == new TagCode("A"));
            Assert.Equal(1, owner.Id);

            var ids = await context.Set<OwnerWithTag>().OrderBy(e => e.Id).Select(e => e.Owned.Tag.Id).ToListAsync();
            Assert.Equal([new TagId(1), new TagId(2)], ids);

            owners[0].Owned.Tag.Id = new TagId(3);
            owners[0].Owned.Tag.RelatedIds.Add(new TagId(7));
            owners[0].Owned.Tags[0].Id = new TagId(30);

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Set<OwnerWithTag>().SingleAsync(e => e.Owned.Tag.Id == new TagId(3));
            Assert.Equal(1, owner.Id);
            Assert.Equal([new TagId(5), new TagId(6), new TagId(7)], owner.Owned.Tag.RelatedIds);
            Assert.Equal(new TagId(30), owner.Owned.Tags.Single().Id);
        }
    }

    protected async Task AssertValueComparerInComplexType(
        Action<OwnedNavigationBuilder<OwnerWithTag, TaggedOwned>>? configureOwned,
        Action<ComplexPropertyBuilder<Tag>>? configureTag)
    {
        var contextFactory = await InitializeTaggedOwners(
            configureOwned,
            configureTag,
            c =>
            {
                c.Properties<TagId>().HaveConversion<TagIdConverter>();
                c.Properties<TagCode>().HaveConversion<TagCodeConverter, TagCodeComparer>();
                c.Properties<List<TagId>>().HaveConversion<TagIdListConverter, TagIdListComparer>();
            });

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Set<OwnerWithTag>().SingleAsync(e => e.Id == 1);

            // Only differs in case, so the comparer treats it as unchanged
            owner.Owned.Tag.Code = new TagCode("a");
            owner.Owned.Tags[0].Code = new TagCode("a");
            context.ChangeTracker.DetectChanges();
            Assert.False(context.Entry(owner.Owned).ComplexProperty(o => o.Tag).Property(t => t.Code).IsModified);
            Assert.Equal(EntityState.Unchanged, context.Entry(owner.Owned).State);

            ClearLog();
            Assert.Equal(0, await context.SaveChangesAsync());
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Set<OwnerWithTag>().SingleAsync(e => e.Id == 1);

            // Mutated in place, so only detected by comparing with the comparer's snapshot
            owner.Owned.Tag.RelatedIds.Add(new TagId(7));
            context.ChangeTracker.DetectChanges();
            Assert.True(context.Entry(owner.Owned).ComplexProperty(o => o.Tag).Property(t => t.RelatedIds).IsModified);

            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Set<OwnerWithTag>().SingleAsync(e => e.Id == 1);
            Assert.Equal(new TagCode("A"), owner.Owned.Tag.Code);
            Assert.Equal([new TagId(5), new TagId(6), new TagId(7)], owner.Owned.Tag.RelatedIds);
        }
    }

    protected static void ConfigureTagConverters(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<TagId>().HaveConversion<TagIdConverter>();
        configurationBuilder.Properties<TagCode>().HaveConversion<TagCodeConverter>();
        configurationBuilder.Properties<List<TagId>>().HaveConversion<TagIdListConverter, TagIdListComparer>();
    }

    protected Task<ContextFactory<Context>> InitializeTaggedOwners(
        Action<OwnedNavigationBuilder<OwnerWithTag, TaggedOwned>>? configureOwned,
        Action<ComplexPropertyBuilder<Tag>>? configureTag,
        Action<ModelConfigurationBuilder> configureConventions,
        Action<ComplexCollectionBuilder<Tag>>? configureTags = null,
        Action<ModelBuilder>? onModelCreating = null)
        => InitializeNonSharedTest<Context>(
            onModelCreating: mb =>
            {
                mb.Ignore<Owner>();
                mb.Entity<OwnerWithTag>(b => b.OwnsOne(
                    e => e.Owned, ob =>
                    {
                        configureOwned?.Invoke(ob);
                        ob.ComplexProperty(o => o.Tag, tb => configureTag?.Invoke(tb));
                        ob.ComplexCollection(
                            o => o.Tags, tb =>
                            {
                                ConfigureTagCollection(tb);
                                configureTags?.Invoke(tb);
                            });
                    }));
                onModelCreating?.Invoke(mb);
            },
            configureConventions: configureConventions,
            seed: context =>
            {
                context.AddRange(
                    new OwnerWithTag
                    {
                        Id = 1,
                        Owned = new TaggedOwned
                        {
                            Tag = new Tag
                            {
                                Id = new TagId(1),
                                Label = "One",
                                Code = new TagCode("A"),
                                RelatedIds = [new TagId(5), new TagId(6)]
                            },
                            Tags = [new Tag { Id = new TagId(10), Label = "Ten", Code = new TagCode("A") }]
                        }
                    },
                    new OwnerWithTag
                    {
                        Id = 2,
                        Owned = new TaggedOwned
                        {
                            Tag = new Tag
                            {
                                Id = new TagId(2),
                                Label = "Two",
                                Code = new TagCode("B")
                            },
                            Tags = [new Tag { Id = new TagId(20), Label = "Twenty", Code = new TagCode("B") }]
                        }
                    });

                return context.SaveChangesAsync();
            });

    protected async Task AssertUpdateOwnedReference(Action<ModelBuilder> onModelCreating, bool nested)
    {
        var contextFactory = await InitializeNonSharedTest<Context>(onModelCreating: onModelCreating, seed: Seed);

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);
            owner.Owned.Address.City = "Melbourne";
            if (nested)
            {
                owner.Owned.Address.Country.Name = "Oz";
            }

            context.ChangeTracker.DetectChanges();
            var addressEntry = context.Entry(owner.Owned).ComplexProperty(o => o.Address);
            Assert.True(addressEntry.IsModified);
            Assert.Equal("Sydney", addressEntry.Property(a => a.City).OriginalValue);
            Assert.Equal(nested, addressEntry.ComplexProperty(a => a.Country).Property(c => c.Name).IsModified);

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);

            Assert.Equal("Melbourne", owner.Owned.Address.City);
            Assert.Equal(nested ? "Oz" : "Australia", owner.Owned.Address.Country.Name);
            Assert.Equal("1 Main St", owner.Owned.Address.Street);
        }
    }

    protected async Task AssertUpdateOwnedCollectionElement(
        Action<ModelBuilder> onModelCreating,
        Action<DbContextOptionsBuilder>? onConfiguring = null)
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: onModelCreating, onConfiguring: onConfiguring, seed: Seed);

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);
            owner.OwnedCollection.Single(e => e.Name == "Owned1b").Address.City = "Fremantle";

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Owners.SingleAsync(e => e.Id == 1);

            Assert.Equal(
                ["Sydney", "Fremantle"],
                owner.OwnedCollection.OrderBy(e => e.Name).Select(e => e.Address.City));
        }
    }

    protected static void ConfigureOwnedReference(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Owner>(b =>
        {
            b.Ignore(e => e.OwnedCollection);
            b.OwnsOne(e => e.Owned, ob => ob.ComplexProperty(o => o.Address));
        });

    protected virtual void ConfigureOwnedCollection(OwnedNavigationBuilder<Owner, Owned> builder)
    {
    }

    protected virtual void ConfigureTagCollection(ComplexCollectionBuilder<Tag> builder)
    {
    }

    protected async Task AssertOwnedReferencesWithDerivedOwner(
        Action<EntityTypeBuilder<Owner>> configureMappingStrategy,
        Action<DbContextOptionsBuilder>? onConfiguring = null)
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb =>
            {
                mb.Entity<Owner>(b =>
                {
                    configureMappingStrategy(b);
                    b.Ignore(e => e.OwnedCollection);
                    b.OwnsOne(e => e.Owned, ob => ob.ComplexProperty(o => o.Address));
                });
                mb.Entity<SpecialOwner>(b => b.OwnsOne(e => e.DerivedOwned, ob => ob.ComplexProperty(o => o.Address)));
            },
            onConfiguring: onConfiguring,
            seed: context =>
            {
                context.Owners.AddRange(
                    new Owner
                    {
                        Id = 1,
                        Name = "Owner1",
                        Owned = CreateOwned("Owned1", "1 Main St", "Sydney", "Australia", "AU")
                    },
                    new SpecialOwner
                    {
                        Id = 2,
                        Name = "Owner2",
                        Owned = CreateOwned("Owned2", "2 Hauptstrasse", "Berlin", "Germany", "DE"),
                        DerivedOwned = CreateOwned("Derived2", "4 Elizabeth St", "Hobart", "Australia", "AU")
                    });

                return context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners.OrderBy(e => e.Id).ToListAsync();

        AssertOwners(owners);
        Assert.Equal("Hobart", Assert.IsType<SpecialOwner>(owners[1]).DerivedOwned.Address.City);

        var cities = await context.Owners.OfType<SpecialOwner>().Select(e => e.DerivedOwned.Address.City).ToListAsync();

        Assert.Equal(["Hobart"], cities);
    }

    protected static void AssertOwners(List<Owner> owners)
        => Assert.Collection(
            owners,
            o =>
            {
                Assert.Equal("Owned1", o.Owned.Name);
                Assert.Equal("1 Main St", o.Owned.Address.Street);
                Assert.Equal("Sydney", o.Owned.Address.City);
                Assert.Equal("Australia", o.Owned.Address.Country.Name);
                Assert.Equal("AU", o.Owned.Address.Country.Code);
            },
            o =>
            {
                Assert.Equal("Owned2", o.Owned.Name);
                Assert.Equal("2 Hauptstrasse", o.Owned.Address.Street);
                Assert.Equal("Berlin", o.Owned.Address.City);
                Assert.Equal("Germany", o.Owned.Address.Country.Name);
                Assert.Equal("DE", o.Owned.Address.Country.Code);
            });

    protected static void AssertOwnedCollections(List<Owner> owners)
        => Assert.Collection(
            owners,
            o => Assert.Equal(
                ["Sydney", "Perth"],
                o.OwnedCollection.OrderBy(e => e.Name).Select(e => e.Address.City)),
            o => Assert.Equal(
                ["Berlin"],
                o.OwnedCollection.Select(e => e.Address.City)));

    protected static Task Seed(Context context)
    {
        context.Owners.AddRange(
            new Owner
            {
                Id = 1,
                Name = "Owner1",
                Owned = CreateOwned("Owned1", "1 Main St", "Sydney", "Australia", "AU"),
                OwnedCollection =
                [
                    CreateOwned("Owned1a", "1 Main St", "Sydney", "Australia", "AU"),
                    CreateOwned("Owned1b", "3 Hay St", "Perth", "Australia", "AU")
                ]
            },
            new Owner
            {
                Id = 2,
                Name = "Owner2",
                Owned = CreateOwned("Owned2", "2 Hauptstrasse", "Berlin", "Germany", "DE"),
                OwnedCollection = [CreateOwned("Owned2a", "2 Hauptstrasse", "Berlin", "Germany", "DE")]
            });

        return context.SaveChangesAsync();
    }

    protected static Owned CreateOwned(string name, string street, string city, string country, string code)
        => new()
        {
            Name = name,
            Address = new Address
            {
                Street = street,
                City = city,
                Country = new Country { Name = country, Code = code }
            }
        };

    protected class Context(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Owner> Owners { get; set; } = null!;
    }

    public class Owner
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public Owned Owned { get; set; } = null!;
        public List<Owned> OwnedCollection { get; set; } = [];
    }

    public class SpecialOwner : Owner
    {
        public Owned DerivedOwned { get; set; } = null!;
    }

    public class OwnerOfOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public AddressHolder? Holder { get; set; }
    }

    public class AddressHolder
    {
        public string? Note { get; set; }
        public Address Address { get; set; } = null!;
    }

    public class Owned
    {
        public string Name { get; set; } = null!;
        public Address Address { get; set; } = null!;
    }

    public class Address
    {
        public string Street { get; set; } = null!;
        public string City { get; set; } = null!;
        public Country Country { get; set; } = null!;
    }

    public class OwnerWithTag
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public TaggedOwned Owned { get; set; } = null!;
    }

    public class TaggedOwned
    {
        public Tag Tag { get; set; } = null!;
        public List<Tag> Tags { get; set; } = [];
    }

    public class Tag
    {
        public TagId Id { get; set; }
        public string Label { get; set; } = null!;
        public TagCode Code { get; set; }
        public List<TagId> RelatedIds { get; set; } = [];
    }

    public readonly record struct TagId(int Value);

    public readonly record struct TagCode(string Value);

    protected class TagIdConverter() : ValueConverter<TagId, int>(v => v.Value, v => new TagId(v));

    protected class TagCodeConverter() : ValueConverter<TagCode, string>(v => v.Value, v => new TagCode(v));

    protected class TagCodeComparer() : ValueComparer<TagCode>(
        (c1, c2) => string.Equals(c1.Value, c2.Value, StringComparison.OrdinalIgnoreCase),
        c => StringComparer.OrdinalIgnoreCase.GetHashCode(c.Value));

    protected class TagIdListConverter() : ValueConverter<List<TagId>, string>(
        v => string.Join(",", v.Select(i => i.Value)),
        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(i => new TagId(int.Parse(i))).ToList());

    protected class TagIdListComparer() : ValueComparer<List<TagId>>(
        (c1, c2) => c1!.SequenceEqual(c2!),
        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v)),
        c => c.ToList());

    public class Country
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
    }
}
