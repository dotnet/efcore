// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
#pragma warning disable CS0414 // Field is assigned but its value is never used

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

public class MemberEntryTest
{
    [ConditionalFact]
    public void Can_get_back_reference_property()
    {
        using var context = new FreezerContext();
        var entity = CreateChunky();
        context.Add(entity);

        var entityEntry = context.Entry(entity);
        Assert.Same(entityEntry.Entity, entityEntry.Member("Monkey").EntityEntry.Entity);
    }

    [ConditionalFact]
    public void Can_get_back_reference_reference()
    {
        using var context = new FreezerContext();
        var entity = CreateChunky();
        context.Add(entity);

        var entityEntry = context.Entry(entity);
        Assert.Same(entityEntry.Entity, entityEntry.Member("Garcia").EntityEntry.Entity);
    }

    [ConditionalFact]
    public void Can_get_back_reference_collection()
    {
        using var context = new FreezerContext();
        var entity = CreateCherry();
        context.Add(entity);

        var entityEntry = context.Entry(entity);
        Assert.Same(entityEntry.Entity, entityEntry.Member("Monkeys").EntityEntry.Entity);
    }

    [ConditionalFact]
    public void Can_get_metadata_property()
    {
        using var context = new FreezerContext();
        var entity = CreateChunky();
        context.Add(entity);

        Assert.Equal("Monkey", context.Entry(entity).Member("Monkey").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_metadata_reference()
    {
        using var context = new FreezerContext();
        var entity = CreateChunky();
        context.Add(entity);

        Assert.Equal("Garcia", context.Entry(entity).Member("Garcia").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_metadata_collection()
    {
        using var context = new FreezerContext();
        var entity = CreateCherry();
        context.Add(entity);

        Assert.Equal("Monkeys", context.Entry(entity).Member("Monkeys").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_and_set_current_value_property()
    {
        using var context = new FreezerContext();
        var entity = CreateChunky();
        context.Add(entity);

        var property = context.Entry(entity).Member("GarciaId");

        Assert.Null(property.CurrentValue);

        property.CurrentValue = 77;
        Assert.Equal(77, property.CurrentValue);

        property.CurrentValue = null;
        Assert.Null(property.CurrentValue);
    }

    [ConditionalFact]
    public void Can_get_and_set_current_value_reference()
    {
        using var context = new FreezerContext();
        var cherry = CreateCherry();
        var chunky = CreateChunky();
        context.AddRange(chunky, cherry);

        var reference = context.Entry(chunky).Member("Garcia");

        Assert.Null(reference.CurrentValue);

        reference.CurrentValue = cherry;

        Assert.Same(cherry, chunky.Garcia);
        Assert.Same(chunky, cherry.Monkeys!.Single());
        Assert.Equal(cherry.Id, chunky.GarciaId);
        Assert.Same(cherry, reference.CurrentValue);

        reference.CurrentValue = null;

        Assert.Null(chunky.Garcia);
        Assert.Empty(cherry.Monkeys!);
        Assert.Null(chunky.GarciaId);
        Assert.Null(reference.CurrentValue);
    }

    [ConditionalFact]
    public void Can_get_and_set_current_value_collection()
    {
        using var context = new FreezerContext();
        var cherry = CreateCherry();
        var chunky = CreateChunky();
        context.AddRange(chunky, cherry);

        var collection = context.Entry(cherry).Member("Monkeys");

        Assert.Null(collection.CurrentValue);

        collection.CurrentValue = new List<Chunky> { chunky };

        Assert.Same(cherry, chunky.Garcia);
        Assert.Same(chunky, cherry.Monkeys!.Single());
        Assert.Equal(cherry.Id, chunky.GarciaId);
        Assert.Same(chunky, ((ICollection<Chunky>)collection.CurrentValue).Single());

        collection.CurrentValue = null;

        Assert.Null(chunky.Garcia);
        Assert.Null(cherry.Monkeys);
        Assert.Null(chunky.GarciaId);
        Assert.Null(collection.CurrentValue);
    }

    [ConditionalFact]
    public void IsModified_tracks_state_of_FK_property_reference()
    {
        using var context = new FreezerContext();
        var cherry = CreateCherry();
        var chunky = CreateChunky();
        chunky.Garcia = cherry;
        cherry.Monkeys = new List<Chunky> { chunky };
        context.AttachRange(cherry, chunky);

        var reference = context.Entry(chunky).Member("Garcia");

        Assert.False(reference.IsModified);

        chunky.GarciaId = null;
        context.ChangeTracker.DetectChanges();

        Assert.True(reference.IsModified);

        context.Entry(chunky).State = EntityState.Unchanged;

        Assert.False(reference.IsModified);
    }

    [ConditionalFact]
    public void IsModified_tracks_state_of_owned_entity()
    {
        using var context = new FreezerContext();
        var chunky = new Chunky
        {
            Chunk = new Chunk { Size = 1, Shape = "Sphere" },
            Culture = new Culture
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            Milk = new Milk
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            }
        };
        context.Add(chunky);

        var reference = context.Entry(chunky).Member(nameof(Chunky.Chunk));

        Assert.True(reference.IsModified);

        context.SaveChanges();

        Assert.False(reference.IsModified);

        chunky.Chunk = new Chunk { Size = 2, Shape = "Cube" };
        context.ChangeTracker.DetectChanges();

        Assert.True(reference.IsModified);

        context.SaveChanges();

        Assert.False(reference.IsModified);
    }

    [ConditionalFact]
    public void IsModified_can_set_fk_to_modified_collection()
    {
        using var context = new FreezerContext();
        var cherry = CreateCherry();
        var chunky1 = CreateChunky();
        chunky1.Garcia = cherry;
        var chunky2 = CreateChunky();
        chunky2.Garcia = cherry;
        cherry.Monkeys = new List<Chunky> { chunky1, chunky2 };
        context.AttachRange(cherry, chunky1, chunky2);

        var collection = context.Entry(cherry).Member("Monkeys");

        Assert.False(collection.IsModified);

        collection.IsModified = true;

        Assert.True(collection.IsModified);
        Assert.True(context.Entry(chunky1).Property(e => e.GarciaId).IsModified);
        Assert.True(context.Entry(chunky2).Property(e => e.GarciaId).IsModified);

        collection.IsModified = false;

        Assert.False(collection.IsModified);
        Assert.False(context.Entry(chunky1).Property(e => e.GarciaId).IsModified);
        Assert.False(context.Entry(chunky2).Property(e => e.GarciaId).IsModified);
        Assert.Equal(EntityState.Unchanged, context.Entry(chunky1).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(chunky2).State);
    }

    [ConditionalFact]
    public void Can_get_back_complex_property()
    {
        using var context = new FreezerContext();
        var entity = CreateChunky();
        context.Add(entity);

        var entityEntry = context.Entry(entity);
        Assert.Same(entityEntry.Entity, entityEntry.Member("Culture").EntityEntry.Entity);
        Assert.Same(entityEntry.Entity, entityEntry.Member("Milk").EntityEntry.Entity);
    }

    [ConditionalFact]
    public void Can_get_metadata_complex_property()
    {
        using var context = new FreezerContext();
        var entity = CreateChunky();
        context.Add(entity);

        Assert.Equal("Culture", context.Entry(entity).Member("Culture").Metadata.Name);
        Assert.Equal("Milk", context.Entry(entity).Member("Milk").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_and_set_current_value_complex_property()
    {
        using var context = new FreezerContext();
        var entity = CreateChunky();
        context.Add(entity);

        var property = context.Entry(entity).Member("Milk");

        Assert.Equal(entity.Milk, property.CurrentValue);

        property.CurrentValue = new Milk { Species = "L. delbrueckii" };
        Assert.Equal("L. delbrueckii", ((Milk)property.CurrentValue).Species);

        property.CurrentValue = null;
        Assert.Null(property.CurrentValue);
    }

    [ConditionalFact]
    public void Can_get_and_set_current_value_struct_complex_property()
    {
        using var context = new FreezerContext();
        var entity = CreateChunky();
        context.Add(entity);

        var property = context.Entry(entity).Member("Culture");

        Assert.Equal(entity.Culture, property.CurrentValue);

        property.CurrentValue = new Culture { Species = "L. delbrueckii" };
        Assert.Equal("L. delbrueckii", ((Culture)property.CurrentValue).Species);
    }

    [ConditionalFact]
    public void Can_get_back_complex_property_using_fields()
    {
        using var context = new FreezerContext();
        var entity = CreateCherry();
        context.Add(entity);

        var entityEntry = context.Entry(entity);
        Assert.Same(entityEntry.Entity, entityEntry.Member("Culture").EntityEntry.Entity);
        Assert.Same(entityEntry.Entity, entityEntry.Member("Milk").EntityEntry.Entity);
    }

    [ConditionalFact]
    public void Can_get_metadata_complex_property_using_fields()
    {
        using var context = new FreezerContext();
        var entity = CreateCherry();
        context.Add(entity);

        Assert.Equal("Culture", context.Entry(entity).Member("Culture").Metadata.Name);
        Assert.Equal("Milk", context.Entry(entity).Member("Milk").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_and_set_current_value_complex_property_using_fields()
    {
        using var context = new FreezerContext();
        var entity = CreateCherry();
        context.Add(entity);

        var property = context.Entry(entity).Member("Milk");

        Assert.Equal(entity.Milk, property.CurrentValue);

        property.CurrentValue = new FieldMilk { Species = "L. delbrueckii" };
        Assert.Equal("L. delbrueckii", ((FieldMilk)property.CurrentValue).Species);

        property.CurrentValue = null;
        Assert.Null(property.CurrentValue);
    }

    [ConditionalFact]
    public void Can_get_and_set_current_value_struct_complex_property_using_fields()
    {
        using var context = new FreezerContext();
        var entity = CreateCherry();
        context.Add(entity);

        var property = context.Entry(entity).Member("Culture");

        Assert.Equal(entity.Culture, property.CurrentValue);

        property.CurrentValue = new FieldCulture { Species = "L. delbrueckii" };
        Assert.Equal("L. delbrueckii", ((FieldCulture)property.CurrentValue).Species);
    }

    [Owned]
    public class Chunk
    {
        public int Size { get; set; }
        public string? Shape { get; set; }
    }

    private class Chunky
    {
        public int Monkey { get; set; }
        public int Id { get; set; }
        public Culture Culture { get; set; }
        public Milk Milk { get; set; } = null!;

        public int? GarciaId { get; set; }
        public Cherry? Garcia { get; set; }

        public Chunk? Chunk { get; set; }
    }

    private class Cherry
    {
        public int Garcia { get; set; }
        public int Id { get; set; }
        public FieldCulture Culture;
        public FieldMilk Milk = null!;

        public ICollection<Chunky>? Monkeys { get; set; }
    }

    private static Chunky CreateChunky(int id = 0)
        => new()
        {
            Id = id,
            Chunk = new Chunk { Size = 1, Shape = "Sphere" },
            Culture = new Culture
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            Milk = new Milk
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            }
        };

    private static Cherry CreateCherry(int id = 0)
        => new()
        {
            Id = id,
            Culture = new FieldCulture
            {
                License = new FieldLicense
                {
                    Charge = 1.0m,
                    Tag = new FieldTag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new FieldTog { Text = "To1" }
                },
                Manufacturer = new FieldManufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new FieldTag { Text = "Ta2" },
                    Tog = new FieldTog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            Milk = new FieldMilk
            {
                License = new FieldLicense
                {
                    Charge = 1.0m,
                    Tag = new FieldTag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new FieldTog { Text = "To1" }
                },
                Manufacturer = new FieldManufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new FieldTag { Text = "Ta2" },
                    Tog = new FieldTog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            }
        };

    private struct Culture
    {
        public string Species { get; set; }
        public string? Subspecies { get; set; }
        public int Rating { get; set; }
        public bool? Validation { get; set; }
        public Manufacturer Manufacturer { get; set; }
        public License License { get; set; }
    }

    private class Milk
    {
        public string Species { get; set; } = null!;
        public string? Subspecies { get; set; }
        public int Rating { get; set; }
        public bool? Validation { get; set; }
        public Manufacturer Manufacturer { get; set; } = null!;
        public License License { get; set; }
    }

    private class Manufacturer
    {
        public string? Name { get; set; }
        public int Rating { get; set; }
        public Tag Tag { get; set; } = null!;
        public Tog Tog { get; set; }
    }

    private struct License
    {
        public string Title { get; set; }
        public decimal Charge { get; set; }
        public Tag Tag { get; set; }
        public Tog Tog { get; set; }
    }

    private class Tag
    {
        public string? Text { get; set; }
    }

    private struct Tog
    {
        public string? Text { get; set; }
    }

    private struct FieldCulture
    {
        public string Species;
        public string? Subspecies;
        public int Rating;
        public bool? Validation;
        public FieldManufacturer Manufacturer;
        public FieldLicense License;
    }

    private class FieldMilk
    {
        public string Species = null!;
        public string? Subspecies;
        public int Rating;
        public bool? Validation;
        public FieldManufacturer Manufacturer = null!;
        public FieldLicense License;
    }

    private class FieldManufacturer
    {
        public string? Name;
        public int Rating;
        public FieldTag Tag = null!;
        public FieldTog Tog;
    }

    private struct FieldLicense
    {
        public string Title;
        public decimal Charge;
        public FieldTag Tag;
        public FieldTog Tog;
    }

    private class FieldTag
    {
        public string? Text;
    }

    private struct FieldTog
    {
        public string? Text;
    }

    private class FreezerContext : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(nameof(FreezerContext));

        public DbSet<Chunky> Icecream { get; set; } = null!;

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chunky>(
                b =>
                {
                    b.ComplexProperty(
                        e => e.Culture, b =>
                        {
                            b.ComplexProperty(
                                e => e.License, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                            b.ComplexProperty(
                                e => e.Manufacturer, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                        });

                    b.ComplexProperty(
                        e => e.Milk, b =>
                        {
                            b.ComplexProperty(
                                e => e.License, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                            b.ComplexProperty(
                                e => e.Manufacturer, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                        });
                });

            modelBuilder.Entity<Cherry>(
                b =>
                {
                    b.ComplexProperty(
                        e => e.Culture, b =>
                        {
                            b.ComplexProperty(
                                e => e.License, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                            b.ComplexProperty(
                                e => e.Manufacturer, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                        });

                    b.ComplexProperty(
                        e => e.Milk, b =>
                        {
                            b.ComplexProperty(
                                e => e.License, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                            b.ComplexProperty(
                                e => e.Manufacturer, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                        });
                });
        }
    }
}
