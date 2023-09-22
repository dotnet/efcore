// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class TableSharingConcurrencyTokenConventionTest
{
    [ConditionalFact]
    public virtual void Missing_concurrency_token_property_is_created_on_the_base_type()
    {
        var modelBuilder = GetModelBuilder();
        modelBuilder.Entity<Person>().HasKey(a => a.Id);
        modelBuilder.Entity<Person>().ToTable(nameof(Animal))
            .Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");
        modelBuilder.Entity<Animal>().HasKey(a => a.Id);
        modelBuilder.Entity<Animal>().HasOne(a => a.FavoritePerson).WithOne().HasForeignKey<Person>(p => p.Id);
        modelBuilder.Entity<Cat>()
            .HasBaseType<Animal>()
            .Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");

        var model = modelBuilder.Model;
        model.FinalizeModel();

        var animal = model.FindEntityType(typeof(Animal));
        var concurrencyProperty = animal.FindProperty("_TableSharingConcurrencyTokenConvention_Version");
        Assert.True(concurrencyProperty.IsConcurrencyToken);
        Assert.True(concurrencyProperty.IsShadowProperty());
        Assert.Equal("Version", concurrencyProperty.GetColumnName());
        Assert.Equal(ValueGenerated.OnAddOrUpdate, concurrencyProperty.ValueGenerated);
    }

    [ConditionalFact]
    public virtual void Missing_concurrency_token_property_is_not_created_for_TPT()
    {
        var modelBuilder = GetModelBuilder();
        modelBuilder.Entity<Animal>().HasKey(a => a.Id);
        modelBuilder.Entity<Animal>().Ignore(a => a.FavoritePerson)
            .Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");
        modelBuilder.Entity<Cat>().HasOne(a => a.FavoritePerson).WithOne().HasForeignKey<Person>(p => p.Id);
        modelBuilder.Entity<Cat>().ToTable(nameof(Cat))
            .HasBaseType<Animal>();
        modelBuilder.Entity<Person>().ToTable(nameof(Cat));
        modelBuilder.Entity<Person>().HasKey(a => a.Id);

        var model = modelBuilder.Model;
        model.FinalizeModel();

        var person = model.FindEntityType(typeof(Person));
        Assert.DoesNotContain(person.GetProperties(), p => p.IsConcurrencyToken);
    }

    [ConditionalFact]
    public virtual void Missing_concurrency_token_property_is_created_for_TPT_same_table()
    {
        var modelBuilder = GetModelBuilder();
        modelBuilder.Entity<Animal>().HasKey(a => a.Id);
        modelBuilder.Entity<Animal>().Ignore(a => a.FavoritePerson);
        modelBuilder.Entity<Cat>().HasOne(a => a.FavoritePerson).WithOne().HasForeignKey<Person>(p => p.Id);
        modelBuilder.Entity<Cat>().ToTable(nameof(Cat))
            .HasBaseType<Animal>()
            .Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");
        modelBuilder.Entity<Person>().ToTable(nameof(Cat));
        modelBuilder.Entity<Person>().HasKey(a => a.Id);

        var model = modelBuilder.Model;
        model.FinalizeModel();

        var person = model.FindEntityType(typeof(Person));
        Assert.Contains(person.GetProperties(), p => p.IsConcurrencyToken);
    }

    [ConditionalFact]
    public virtual void Missing_concurrency_token_property_is_not_created_for_TPH()
    {
        var modelBuilder = GetModelBuilder();
        modelBuilder.Entity<Animal>().HasKey(a => a.Id);
        modelBuilder.Entity<Animal>().Ignore(a => a.FavoritePerson);
        modelBuilder.Entity<Cat>()
            .HasBaseType<Animal>()
            .Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");

        var model = modelBuilder.Model;
        model.FinalizeModel();

        var person = model.FindEntityType(typeof(Animal));
        Assert.DoesNotContain(person.GetProperties(), p => p.IsConcurrencyToken);
    }

    [ConditionalFact]
    public virtual void Missing_concurrency_token_properties_are_created_on_the_base()
    {
        var modelBuilder = GetModelBuilder();
        modelBuilder.Entity<Person>().HasKey(a => a.Id);
        modelBuilder.Entity<Person>().ToTable(nameof(Animal)).Property<byte[]>("Version")
            .HasColumnName("Version").ValueGeneratedOnUpdate().IsConcurrencyToken();
        modelBuilder.Entity<Animal>().HasKey(a => a.Id);
        modelBuilder.Entity<Animal>().HasOne(a => a.FavoritePerson).WithOne().HasForeignKey<Person>(p => p.Id);
        modelBuilder.Entity<Animal>().HasOne(a => a.Dwelling).WithOne().HasForeignKey<AnimalHouse>(p => p.Id);
        modelBuilder.Entity<Cat>()
            .HasBaseType<Animal>();
        modelBuilder.Entity<AnimalHouse>().HasKey(a => a.Id);
        modelBuilder.Entity<AnimalHouse>().ToTable(nameof(Animal));
        modelBuilder.Entity<TheMovie>()
            .HasBaseType<AnimalHouse>();

        var model = modelBuilder.Model;
        model.FinalizeModel();

        var animal = model.FindEntityType(typeof(Animal));
        var concurrencyProperty = animal.FindProperty("_TableSharingConcurrencyTokenConvention_Version");
        Assert.True(concurrencyProperty.IsConcurrencyToken);
        Assert.True(concurrencyProperty.IsShadowProperty());
        Assert.Equal("Version", concurrencyProperty.GetColumnName());
        Assert.Equal(ValueGenerated.OnUpdate, concurrencyProperty.ValueGenerated);

        var cat = model.FindEntityType(typeof(Cat));
        Assert.DoesNotContain(cat.GetDeclaredProperties(), p => p.Name == "_TableSharingConcurrencyTokenConvention_Version");

        var animalHouse = model.FindEntityType(typeof(AnimalHouse));
        concurrencyProperty = animalHouse.FindProperty("_TableSharingConcurrencyTokenConvention_Version");
        Assert.True(concurrencyProperty.IsConcurrencyToken);
        Assert.True(concurrencyProperty.IsShadowProperty());
        Assert.Equal("Version", concurrencyProperty.GetColumnName());
        Assert.Equal(ValueGenerated.OnUpdate, concurrencyProperty.ValueGenerated);

        var theMovie = model.FindEntityType(typeof(TheMovie));
        Assert.DoesNotContain(theMovie.GetDeclaredProperties(), p => p.Name == "_TableSharingConcurrencyTokenConvention_Version");
    }

    [ConditionalFact]
    public virtual void Missing_concurrency_token_property_is_created_on_the_sharing_type()
    {
        var modelBuilder = GetModelBuilder();
        modelBuilder.Entity<Person>().HasKey(a => a.Id);
        modelBuilder.Entity<Person>().ToTable(nameof(Animal));
        modelBuilder.Entity<Animal>().HasKey(a => a.Id);
        modelBuilder.Entity<Animal>().HasOne(a => a.FavoritePerson).WithOne().HasForeignKey<Person>(p => p.Id);
        modelBuilder.Entity<Animal>().Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");

        var model = modelBuilder.Model;
        model.FinalizeModel();

        var personEntityType = model.FindEntityType(typeof(Person));
        var concurrencyProperty = personEntityType.FindProperty("_TableSharingConcurrencyTokenConvention_Version");
        Assert.True(concurrencyProperty.IsConcurrencyToken);
        Assert.True(concurrencyProperty.IsShadowProperty());
        Assert.Equal("Version", concurrencyProperty.GetColumnName());
        Assert.Equal(ValueGenerated.OnAddOrUpdate, concurrencyProperty.ValueGenerated);
    }

    [ConditionalFact]
    public virtual void Missing_concurrency_token_property_is_created_on_the_sharing_type_with_complex_property()
    {
        var modelBuilder = GetModelBuilder();
        modelBuilder.Entity<Person>().HasKey(a => a.Id);
        modelBuilder.Entity<Person>().ToTable(nameof(Animal));
        modelBuilder.Entity<Animal>(
            ab =>
            {
                ab.HasKey(a => a.Id);
                ab.HasOne(a => a.FavoritePerson).WithOne().HasForeignKey<Person>(p => p.Id);
                ab.ComplexProperty(a => a.Dwelling)
                    .Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");
            });

        var model = modelBuilder.Model;
        model.FinalizeModel();

        var personEntityType = model.FindEntityType(typeof(Person));
        var concurrencyProperty = personEntityType.FindProperty("_TableSharingConcurrencyTokenConvention_Version");
        Assert.True(concurrencyProperty.IsConcurrencyToken);
        Assert.True(concurrencyProperty.IsShadowProperty());
        Assert.Equal("Version", concurrencyProperty.GetColumnName());
        Assert.Equal(ValueGenerated.OnAddOrUpdate, concurrencyProperty.ValueGenerated);

        var animalEntityType = model.FindEntityType(typeof(Animal));
        Assert.All(animalEntityType.GetProperties(), p => Assert.NotEqual(typeof(byte[]), p.ClrType));
    }

    [ConditionalFact]
    public virtual void Concurrency_token_property_is_not_created_on_the_sharing_when_on_complex_property()
    {
        var modelBuilder = GetModelBuilder();
        modelBuilder.Entity<Person>().HasKey(a => a.Id);
        modelBuilder.Entity<Person>().ToTable(nameof(Animal));
        modelBuilder.Entity<Person>().Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");
        modelBuilder.Entity<Animal>(
            ab =>
            {
                ab.HasKey(a => a.Id);
                ab.HasOne(a => a.FavoritePerson).WithOne().HasForeignKey<Person>(p => p.Id);
                ab.ComplexProperty(a => a.Dwelling)
                    .Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");
            });

        var model = modelBuilder.Model;
        model.FinalizeModel();

        var animalEntityType = model.FindEntityType(typeof(Animal));
        Assert.All(animalEntityType.GetProperties(), p => Assert.NotEqual(typeof(byte[]), p.ClrType));
    }

    protected class Animal
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Person FavoritePerson { get; set; }
        public AnimalHouse Dwelling { get; set; }
    }

    protected class Cat : Animal
    {
        public string Breed { get; set; }

        [NotMapped]
        public string Type { get; set; }

        public int Identity { get; set; }
    }

    protected class AnimalHouse
    {
        public int Id { get; set; }
    }

    protected class TheMovie : AnimalHouse
    {
        public bool CanHaveAnother { get; set; }
    }

    protected class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FavoriteBreed { get; set; }
    }

    private ModelBuilder GetModelBuilder(DbContext dbContext = null)
    {
        var conventionSet = new ConventionSet();

        var dependencies = CreateDependencies()
            .With(new CurrentDbContext(dbContext ?? new DbContext(new DbContextOptions<DbContext>())));
        var relationalDependencies = CreateRelationalDependencies();
        var tableSharingConcurrencyTokenConvention = new TableSharingConcurrencyTokenConvention(dependencies, relationalDependencies);
        conventionSet.ModelFinalizingConventions.Add(tableSharingConcurrencyTokenConvention);

        return new ModelBuilder(conventionSet);
    }

    private ProviderConventionSetBuilderDependencies CreateDependencies()
        => FakeRelationalTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

    private RelationalConventionSetBuilderDependencies CreateRelationalDependencies()
        => FakeRelationalTestHelpers.Instance.CreateContextServices().GetRequiredService<RelationalConventionSetBuilderDependencies>();
}
