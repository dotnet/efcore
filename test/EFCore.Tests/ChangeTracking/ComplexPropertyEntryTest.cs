// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

#pragma warning disable CS0649, CS0414
public class ComplexPropertyEntryTest
{
    [ConditionalFact]
    public void Can_obtain_underlying_state_entry()
    {
        using var context = new YogurtContext();
        var entity = context.Add(CreateYogurt()).Entity;
        var entry = context.GetService<IStateManager>().GetOrCreateEntry(entity);

        Assert.Same(entry, context.Entry(entity).ComplexProperty(e => e.Culture).GetInfrastructure());
        Assert.Same(entry, context.Entry(entity).ComplexProperty<Culture>("Culture").GetInfrastructure());
        Assert.Same(entry, context.Entry(entity).ComplexProperty("Culture").GetInfrastructure());
        Assert.Same(entry, context.Entry((object)entity).ComplexProperty("Culture").GetInfrastructure());
        Assert.Same(entry, context.Entry((object)entity).ComplexProperty("Culture").GetInfrastructure());

        Assert.Same(entry, context.Entry(entity).ComplexProperty(e => e.Culture).ComplexProperty(e => e.License).GetInfrastructure());
        Assert.Same(entry, context.Entry(entity).ComplexProperty<Culture>("Culture").ComplexProperty(e => e.License).GetInfrastructure());
        Assert.Same(entry, context.Entry(entity).ComplexProperty("Culture").ComplexProperty("License").GetInfrastructure());
        Assert.Same(entry, context.Entry((object)entity).ComplexProperty("Culture").ComplexProperty("License").GetInfrastructure());
        Assert.Same(entry, context.Entry((object)entity).ComplexProperty("Culture").ComplexProperty("License").GetInfrastructure());
    }

    [ConditionalFact]
    public void Can_get_metadata()
    {
        using var context = new YogurtContext();
        var entity = context.Add(CreateYogurt()).Entity;

        Assert.Equal("Culture", context.Entry(entity).ComplexProperty(e => e.Culture).Metadata.Name);
        Assert.Equal("Culture", context.Entry(entity).ComplexProperty<Culture>("Culture").Metadata.Name);
        Assert.Equal("Culture", context.Entry(entity).ComplexProperty("Culture").Metadata.Name);
        Assert.Equal("Culture", context.Entry((object)entity).ComplexProperty("Culture").Metadata.Name);
        Assert.Equal("Culture", context.Entry((object)entity).ComplexProperty("Culture").Metadata.Name);

        Assert.Equal("License", context.Entry(entity).ComplexProperty(e => e.Culture).ComplexProperty(e => e.License).Metadata.Name);
        Assert.Equal("License", context.Entry(entity).ComplexProperty<Culture>("Culture").ComplexProperty(e => e.License).Metadata.Name);
        Assert.Equal("License", context.Entry(entity).ComplexProperty("Culture").ComplexProperty("License").Metadata.Name);
        Assert.Equal("License", context.Entry((object)entity).ComplexProperty("Culture").ComplexProperty("License").Metadata.Name);
        Assert.Equal("License", context.Entry((object)entity).ComplexProperty("Culture").ComplexProperty("License").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_property_entry_by_name()
    {
        using var context = new YogurtContext();
        var entity = context.Add(CreateYogurt()).Entity;

        Assert.Equal("Rating", context.Entry(entity).ComplexProperty(e => e.Culture).Property(e => e.Rating).Metadata.Name);
        Assert.Equal("Rating", context.Entry(entity).ComplexProperty<Culture>("Culture").Property(e => e.Rating).Metadata.Name);
        Assert.Equal("Rating", context.Entry(entity).ComplexProperty("Culture").Property("Rating").Metadata.Name);
        Assert.Equal("Rating", context.Entry((object)entity).ComplexProperty("Culture").Property("Rating").Metadata.Name);
        Assert.Equal("Rating", context.Entry((object)entity).ComplexProperty("Culture").Property("Rating").Metadata.Name);

        Assert.Equal(
            "Charge",
            context.Entry(entity).ComplexProperty(e => e.Culture).ComplexProperty(e => e.License).Property(e => e.Charge).Metadata.Name);
        Assert.Equal(
            "Charge",
            context.Entry(entity).ComplexProperty<Culture>("Culture").ComplexProperty(e => e.License).Property(e => e.Charge).Metadata
                .Name);
        Assert.Equal(
            "Charge", context.Entry(entity).ComplexProperty("Culture").ComplexProperty("License").Property("Charge").Metadata.Name);
        Assert.Equal(
            "Charge", context.Entry((object)entity).ComplexProperty("Culture").ComplexProperty("License").Property("Charge").Metadata.Name);
        Assert.Equal(
            "Charge", context.Entry((object)entity).ComplexProperty("Culture").ComplexProperty("License").Property("Charge").Metadata.Name);
    }

    [ConditionalFact]
    public void Throws_when_wrong_generic_type_is_used_while_getting_property_entry_by_name()
    {
        using var context = new YogurtContext();
        var complexEntry = context.Add(CreateYogurt()).ComplexProperty(e => e.Culture);

        Assert.Equal(
            CoreStrings.WrongGenericPropertyType("Rating", nameof(Culture), "int", "string"),
            Assert.Throws<ArgumentException>(() => complexEntry.Property<string>("Rating")).Message);
    }

    [ConditionalFact]
    public void Throws_when_wrong_property_name_is_used_while_getting_property_entry_by_name()
    {
        using var context = new YogurtContext();
        var complexEntry = context.Add(CreateYogurt()).ComplexProperty(e => e.Culture);

        Assert.Equal(
            CoreStrings.PropertyNotFound("Chimp", complexEntry.Metadata.ComplexType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => complexEntry.Property("Chimp").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.PropertyNotFound("Chimp", complexEntry.Metadata.ComplexType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => complexEntry.Property("Chimp").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.PropertyNotFound("Chimp", complexEntry.Metadata.ComplexType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => complexEntry.Property<int>("Chimp").Metadata.Name).Message);
    }

    [ConditionalFact]
    public void Can_get_all_modified_properties()
    {
        using var context = new YogurtContext();
        var complexEntry = context.Attach(CreateYogurt()).ComplexProperty(e => e.Culture);

        var modified = complexEntry.Properties.Where(e => e.IsModified).Select(e => e.Metadata.Name).ToList();

        Assert.Empty(modified);

        complexEntry.Property(e => e.Species).CurrentValue = "S";
        complexEntry.Property(e => e.Subspecies).CurrentValue = "SS";

        modified = complexEntry.Properties.Where(e => e.IsModified).Select(e => e.Metadata.Name).ToList();

        Assert.Equal(["Species", "Subspecies"], modified);
    }

    [ConditionalFact]
    public void Can_get_all_property_entries()
    {
        using var context = new YogurtContext();
        var complexEntry = context.Add(CreateYogurt()).ComplexProperty(e => e.Culture);

        Assert.Equal(
            [
                "Rating",
                "Species",
                "Subspecies",
                "Validation"
            ],
            complexEntry.Properties.Select(e => e.Metadata.Name).ToList());
    }

    [ConditionalFact]
    public void Throws_when_wrong_generic_type_is_used_while_getting_complex_property_entry_by_name()
    {
        using var context = new YogurtContext();
        var complexEntry = context.Add(CreateYogurt()).ComplexProperty(e => e.Culture);

        Assert.Equal(
            CoreStrings.WrongGenericPropertyType("License", "Culture", "License", "string"),
            Assert.Throws<ArgumentException>(() => complexEntry.ComplexProperty<string>("License")).Message);

        var nestedComplexEntry = complexEntry.ComplexProperty(e => e.License);

        Assert.Equal(
            CoreStrings.WrongGenericPropertyType("Tag", "License", "Tag", "string"),
            Assert.Throws<ArgumentException>(() => nestedComplexEntry.ComplexProperty<string>("Tag")).Message);
    }

    [ConditionalFact]
    public void Throws_when_wrong_complex_property_name_is_used_while_getting_property_entry_by_name()
    {
        using var context = new YogurtContext();
        var complexEntry = context.Add(CreateYogurt()).ComplexProperty(e => e.Culture);

        Assert.Equal(
            CoreStrings.ComplexPropertyNotFound(complexEntry.Metadata.ComplexType.DisplayName(), "Chimp"),
            Assert.Throws<InvalidOperationException>(() => complexEntry.ComplexProperty("Chimp").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.ComplexPropertyNotFound(complexEntry.Metadata.ComplexType.DisplayName(), "Chimp"),
            Assert.Throws<InvalidOperationException>(() => complexEntry.ComplexProperty("Chimp").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.ComplexPropertyNotFound(complexEntry.Metadata.ComplexType.DisplayName(), "Chimp"),
            Assert.Throws<InvalidOperationException>(() => complexEntry.ComplexProperty<int>("Chimp").Metadata.Name).Message);
    }

    [ConditionalFact]
    public void Can_get_all_complex_property_entries()
    {
        using var context = new YogurtContext();
        var complexEntry = context.Add(CreateYogurt()).ComplexProperty(e => e.Culture);

        Assert.Equal(
            ["License", "Manufacturer"],
            complexEntry.ComplexProperties.Select(e => e.Metadata.Name).ToList());

        var nestedComplexEntry = complexEntry.ComplexProperty(e => e.License);

        Assert.Equal(
            ["Tag", "Tog"],
            nestedComplexEntry.ComplexProperties.Select(e => e.Metadata.Name).ToList());
    }

    [ConditionalFact]
    public void Can_obtain_underlying_state_entry_with_fields()
    {
        using var context = new YogurtContext();
        var entity = context.Add(CreateYogurt()).Entity;
        var entry = context.GetService<IStateManager>().GetOrCreateEntry(entity);

        Assert.Same(entry, context.Entry(entity).ComplexProperty(e => e.FieldCulture).GetInfrastructure());
        Assert.Same(entry, context.Entry(entity).ComplexProperty<FieldCulture>("FieldCulture").GetInfrastructure());
        Assert.Same(entry, context.Entry(entity).ComplexProperty("FieldCulture").GetInfrastructure());
        Assert.Same(entry, context.Entry((object)entity).ComplexProperty("FieldCulture").GetInfrastructure());
        Assert.Same(entry, context.Entry((object)entity).ComplexProperty("FieldCulture").GetInfrastructure());

        Assert.Same(entry, context.Entry(entity).ComplexProperty(e => e.FieldCulture).ComplexProperty(e => e.License).GetInfrastructure());
        Assert.Same(
            entry, context.Entry(entity).ComplexProperty<FieldCulture>("FieldCulture").ComplexProperty(e => e.License).GetInfrastructure());
        Assert.Same(entry, context.Entry(entity).ComplexProperty("FieldCulture").ComplexProperty("License").GetInfrastructure());
        Assert.Same(entry, context.Entry((object)entity).ComplexProperty("FieldCulture").ComplexProperty("License").GetInfrastructure());
        Assert.Same(entry, context.Entry((object)entity).ComplexProperty("FieldCulture").ComplexProperty("License").GetInfrastructure());
    }

    [ConditionalFact]
    public void Can_get_metadata_with_fields()
    {
        using var context = new YogurtContext();
        var entity = context.Add(CreateYogurt()).Entity;

        Assert.Equal("FieldCulture", context.Entry(entity).ComplexProperty(e => e.FieldCulture).Metadata.Name);
        Assert.Equal("FieldCulture", context.Entry(entity).ComplexProperty<FieldCulture>("FieldCulture").Metadata.Name);
        Assert.Equal("FieldCulture", context.Entry(entity).ComplexProperty("FieldCulture").Metadata.Name);
        Assert.Equal("FieldCulture", context.Entry((object)entity).ComplexProperty("FieldCulture").Metadata.Name);
        Assert.Equal("FieldCulture", context.Entry((object)entity).ComplexProperty("FieldCulture").Metadata.Name);

        Assert.Equal("License", context.Entry(entity).ComplexProperty(e => e.FieldCulture).ComplexProperty(e => e.License).Metadata.Name);
        Assert.Equal(
            "License", context.Entry(entity).ComplexProperty<FieldCulture>("FieldCulture").ComplexProperty(e => e.License).Metadata.Name);
        Assert.Equal("License", context.Entry(entity).ComplexProperty("FieldCulture").ComplexProperty("License").Metadata.Name);
        Assert.Equal("License", context.Entry((object)entity).ComplexProperty("FieldCulture").ComplexProperty("License").Metadata.Name);
        Assert.Equal("License", context.Entry((object)entity).ComplexProperty("FieldCulture").ComplexProperty("License").Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_property_entry_by_name_with_fields()
    {
        using var context = new YogurtContext();
        var entity = context.Add(CreateYogurt()).Entity;

        Assert.Equal("Rating", context.Entry(entity).ComplexProperty(e => e.FieldCulture).Property(e => e.Rating).Metadata.Name);
        Assert.Equal("Rating", context.Entry(entity).ComplexProperty<FieldCulture>("FieldCulture").Property(e => e.Rating).Metadata.Name);
        Assert.Equal("Rating", context.Entry(entity).ComplexProperty("FieldCulture").Property("Rating").Metadata.Name);
        Assert.Equal("Rating", context.Entry((object)entity).ComplexProperty("FieldCulture").Property("Rating").Metadata.Name);
        Assert.Equal("Rating", context.Entry((object)entity).ComplexProperty("FieldCulture").Property("Rating").Metadata.Name);

        Assert.Equal(
            "Charge",
            context.Entry(entity).ComplexProperty(e => e.FieldCulture).ComplexProperty(e => e.License).Property(e => e.Charge).Metadata
                .Name);
        Assert.Equal(
            "Charge",
            context.Entry(entity).ComplexProperty<FieldCulture>("FieldCulture").ComplexProperty(e => e.License).Property(e => e.Charge)
                .Metadata.Name);
        Assert.Equal(
            "Charge", context.Entry(entity).ComplexProperty("FieldCulture").ComplexProperty("License").Property("Charge").Metadata.Name);
        Assert.Equal(
            "Charge",
            context.Entry((object)entity).ComplexProperty("FieldCulture").ComplexProperty("License").Property("Charge").Metadata.Name);
        Assert.Equal(
            "Charge",
            context.Entry((object)entity).ComplexProperty("FieldCulture").ComplexProperty("License").Property("Charge").Metadata.Name);
    }

    [ConditionalFact]
    public void Throws_when_wrong_generic_type_is_used_while_getting_property_entry_by_name_with_fields()
    {
        using var context = new YogurtContext();
        var complexEntry = context.Add(CreateYogurt()).ComplexProperty(e => e.FieldCulture);

        Assert.Equal(
            CoreStrings.WrongGenericPropertyType("Rating", nameof(FieldCulture), "int", "string"),
            Assert.Throws<ArgumentException>(() => complexEntry.Property<string>("Rating")).Message);
    }

    [ConditionalFact]
    public void Throws_when_wrong_property_name_is_used_while_getting_property_entry_by_name_with_fields()
    {
        using var context = new YogurtContext();
        var complexEntry = context.Add(CreateYogurt()).ComplexProperty(e => e.FieldCulture);

        Assert.Equal(
            CoreStrings.PropertyNotFound("Chimp", complexEntry.Metadata.ComplexType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => complexEntry.Property("Chimp").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.PropertyNotFound("Chimp", complexEntry.Metadata.ComplexType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => complexEntry.Property("Chimp").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.PropertyNotFound("Chimp", complexEntry.Metadata.ComplexType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => complexEntry.Property<int>("Chimp").Metadata.Name).Message);
    }

    [ConditionalFact]
    public void Can_get_all_modified_properties_with_fields()
    {
        using var context = new YogurtContext();
        var complexEntry = context.Attach(CreateYogurt()).ComplexProperty(e => e.FieldCulture);

        var modified = complexEntry.Properties.Where(e => e.IsModified).Select(e => e.Metadata.Name).ToList();

        Assert.Empty(modified);

        complexEntry.Property(e => e.Species).CurrentValue = "S";
        complexEntry.Property(e => e.Subspecies).CurrentValue = "SS";

        modified = complexEntry.Properties.Where(e => e.IsModified).Select(e => e.Metadata.Name).ToList();

        Assert.Equal(["Species", "Subspecies"], modified);
    }

    [ConditionalFact]
    public void Can_get_all_property_entries_with_fields()
    {
        using var context = new YogurtContext();
        var complexEntry = context.Add(CreateYogurt()).ComplexProperty(e => e.FieldCulture);

        Assert.Equal(
            [
                "Rating",
                "Species",
                "Subspecies",
                "Validation"
            ],
            complexEntry.Properties.Select(e => e.Metadata.Name).ToList());
    }

    [ConditionalFact]
    public void Throws_when_wrong_generic_type_is_used_while_getting_complex_property_entry_by_name_with_fields()
    {
        using var context = new YogurtContext();
        var complexEntry = context.Add(CreateYogurt()).ComplexProperty(e => e.FieldCulture);

        Assert.Equal(
            CoreStrings.WrongGenericPropertyType("License", "FieldCulture", "FieldLicense", "string"),
            Assert.Throws<ArgumentException>(() => complexEntry.ComplexProperty<string>("License")).Message);

        var nestedComplexEntry = complexEntry.ComplexProperty(e => e.License);

        Assert.Equal(
            CoreStrings.WrongGenericPropertyType("Tag", "FieldLicense", "FieldTag", "string"),
            Assert.Throws<ArgumentException>(() => nestedComplexEntry.ComplexProperty<string>("Tag")).Message);
    }

    [ConditionalFact]
    public void Throws_when_wrong_complex_property_name_is_used_while_getting_property_entry_by_name_with_fields()
    {
        using var context = new YogurtContext();
        var complexEntry = context.Add(CreateYogurt()).ComplexProperty(e => e.FieldCulture);

        Assert.Equal(
            CoreStrings.ComplexPropertyNotFound(complexEntry.Metadata.ComplexType.DisplayName(), "Chimp"),
            Assert.Throws<InvalidOperationException>(() => complexEntry.ComplexProperty("Chimp").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.ComplexPropertyNotFound(complexEntry.Metadata.ComplexType.DisplayName(), "Chimp"),
            Assert.Throws<InvalidOperationException>(() => complexEntry.ComplexProperty("Chimp").Metadata.Name).Message);
        Assert.Equal(
            CoreStrings.ComplexPropertyNotFound(complexEntry.Metadata.ComplexType.DisplayName(), "Chimp"),
            Assert.Throws<InvalidOperationException>(() => complexEntry.ComplexProperty<int>("Chimp").Metadata.Name).Message);
    }

    [ConditionalFact]
    public void Can_get_all_complex_property_entries_with_fields()
    {
        using var context = new YogurtContext();
        var complexEntry = context.Add(CreateYogurt()).ComplexProperty(e => e.FieldCulture);

        Assert.Equal(
            ["License", "Manufacturer"],
            complexEntry.ComplexProperties.Select(e => e.Metadata.Name).ToList());

        var nestedComplexEntry = complexEntry.ComplexProperty(e => e.License);

        Assert.Equal(
            ["Tag", "Tog"],
            nestedComplexEntry.ComplexProperties.Select(e => e.Metadata.Name).ToList());
    }

    private static Yogurt CreateYogurt()
        => new()
        {
            Id = Guid.NewGuid(),
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
            FieldCulture = new FieldCulture
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
        };

    private class Yogurt
    {
        public Guid Id { get; set; }
        public Culture Culture { get; set; }
        public FieldCulture FieldCulture;
    }

    private class YogurtContext : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(GetType().FullName!);

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Yogurt>(
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
                        e => e.FieldCulture, b =>
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

    private struct Culture
    {
        public string Species { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string? Subspecies { get; set; }
        public int Rating { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public bool? Validation  { get; set; }
        public Manufacturer Manufacturer { get; set; }
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
}
