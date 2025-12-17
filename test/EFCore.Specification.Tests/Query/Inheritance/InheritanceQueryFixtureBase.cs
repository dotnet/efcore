// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

#nullable disable

public abstract class InheritanceQueryFixtureBase : SharedStoreFixtureBase<InheritanceContext>, IFilteredQueryFixtureBase
{
    private readonly Dictionary<bool, ISetSource> _expectedDataCache = new();

    protected override string StoreName
        => "InheritanceTest";

    public virtual bool EnableFilters
        => false;

    public virtual bool IsDiscriminatorMappingComplete
        => true;

    public virtual bool HasDiscriminator
        => true;

    public virtual bool UseGeneratedKeys
        => true;

    public virtual bool EnableComplexTypes
        => true;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(w => w.Ignore(
            CoreEventId.MappedEntityTypeIgnoredWarning,
            CoreEventId.MappedPropertyIgnoredWarning,
            CoreEventId.MappedNavigationIgnoredWarning));

    public Func<DbContext> GetContextCreator()
        => CreateContext;

    public virtual ISetSource GetExpectedData()
        => UseGeneratedKeys
            ? InheritanceData.GeneratedKeysInstance
            : InheritanceData.Instance;

    public virtual ISetSource GetFilteredExpectedData(DbContext context)
    {
        if (_expectedDataCache.TryGetValue(EnableFilters, out var cachedResult))
        {
            return cachedResult;
        }

        var expectedData = new InheritanceData(UseGeneratedKeys);
        if (EnableFilters)
        {
            var animals = expectedData.Animals.Where(a => a.CountryId == 1).ToList();
            var animalQueries = expectedData.AnimalQueries.Where(a => a.CountryId == 1).ToList();
            expectedData = new InheritanceData(
                animals, animalQueries, expectedData.Countries, expectedData.Drinks, expectedData.Plants);
        }

        _expectedDataCache[EnableFilters] = expectedData;

        return expectedData;
    }

    public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object, object>>
    {
        { typeof(Animal), e => ((Animal)e)?.Species },
        { typeof(Bird), e => ((Bird)e)?.Species },
        { typeof(Kiwi), e => ((Kiwi)e)?.Species },
        { typeof(Eagle), e => ((Eagle)e)?.Species },
        { typeof(AnimalQuery), e => ((AnimalQuery)e)?.Name },
        { typeof(BirdQuery), e => ((BirdQuery)e)?.Name },
        { typeof(KiwiQuery), e => ((KiwiQuery)e)?.Name },
        { typeof(EagleQuery), e => ((EagleQuery)e)?.Name },
        { typeof(Plant), e => ((Plant)e)?.Species },
        { typeof(Flower), e => ((Flower)e)?.Species },
        { typeof(Daisy), e => ((Daisy)e)?.Species },
        { typeof(Rose), e => ((Rose)e)?.Species },
        { typeof(Country), e => ((Country)e)?.Id },
        { typeof(Drink), e => ((Drink)e)?.SortIndex },
        { typeof(Coke), e => ((Coke)e)?.SortIndex },
        { typeof(Lilt), e => ((Lilt)e)?.SortIndex },
        { typeof(Tea), e => ((Tea)e)?.SortIndex },
        { typeof(ComplexType), e => ((ComplexType)e)?.UniqueInt },
        { typeof(NestedComplexType), e => ((NestedComplexType)e)?.UniqueInt },
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public IReadOnlyDictionary<Type, object> EntityAsserters { get; }

    public InheritanceQueryFixtureBase()
        => EntityAsserters = new Dictionary<Type, Action<object, object>>
        {
            {
                typeof(Animal), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (Animal)e;
                        var aa = (Animal)a;

                        Assert.Equal(ee.Species, aa.Species);
                        Assert.Equal(ee.Name, aa.Name);
                        Assert.Equal(ee.CountryId, aa.CountryId);
                    }
                }
            },
            {
                typeof(Bird), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (Bird)e;
                        var aa = (Bird)a;

                        Assert.Equal(ee.Species, aa.Species);
                        Assert.Equal(ee.Name, aa.Name);
                        Assert.Equal(ee.CountryId, aa.CountryId);
                        Assert.Equal(ee.IsFlightless, aa.IsFlightless);
                    }
                }
            },
            {
                typeof(Eagle), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (Eagle)e;
                        var aa = (Eagle)a;

                        Assert.Equal(ee.Species, aa.Species);
                        Assert.Equal(ee.Name, aa.Name);
                        Assert.Equal(ee.CountryId, aa.CountryId);
                        Assert.Equal(ee.IsFlightless, aa.IsFlightless);
                        Assert.Equal(ee.Group, aa.Group);
                    }
                }
            },
            {
                typeof(Kiwi), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (Kiwi)e;
                        var aa = (Kiwi)a;

                        Assert.Equal(ee.Species, aa.Species);
                        Assert.Equal(ee.Name, aa.Name);
                        Assert.Equal(ee.CountryId, aa.CountryId);
                        Assert.Equal(ee.IsFlightless, aa.IsFlightless);
                        Assert.Equal(ee.FoundOn, aa.FoundOn);
                    }
                }
            },
            {
                typeof(AnimalQuery), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (AnimalQuery)e;
                        var aa = (AnimalQuery)a;

                        Assert.Equal(ee.Name, aa.Name);
                        Assert.Equal(ee.CountryId, aa.CountryId);
                    }
                }
            },
            {
                typeof(BirdQuery), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (BirdQuery)e;
                        var aa = (BirdQuery)a;

                        Assert.Equal(ee.Name, aa.Name);
                        Assert.Equal(ee.CountryId, aa.CountryId);
                        Assert.Equal(ee.IsFlightless, aa.IsFlightless);
                        Assert.Equal(ee.EagleId, aa.EagleId);
                    }
                }
            },
            {
                typeof(EagleQuery), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (EagleQuery)e;
                        var aa = (EagleQuery)a;

                        Assert.Equal(ee.Name, aa.Name);
                        Assert.Equal(ee.CountryId, aa.CountryId);
                        Assert.Equal(ee.IsFlightless, aa.IsFlightless);
                        Assert.Equal(ee.EagleId, aa.EagleId);
                        Assert.Equal(ee.Group, aa.Group);
                    }
                }
            },
            {
                typeof(KiwiQuery), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (KiwiQuery)e;
                        var aa = (KiwiQuery)a;

                        Assert.Equal(ee.Name, aa.Name);
                        Assert.Equal(ee.CountryId, aa.CountryId);
                        Assert.Equal(ee.IsFlightless, aa.IsFlightless);
                        Assert.Equal(ee.EagleId, aa.EagleId);
                        Assert.Equal(ee.FoundOn, aa.FoundOn);
                    }
                }
            },
            {
                typeof(Plant), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (Plant)e;
                        var aa = (Plant)a;

                        Assert.Equal(ee.Species, aa.Species);
                        Assert.Equal(ee.Name, aa.Name);
                        Assert.Equal(ee.Genus, aa.Genus);
                    }
                }
            },
            {
                typeof(Flower), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (Flower)e;
                        var aa = (Flower)a;

                        Assert.Equal(ee.Species, aa.Species);
                        Assert.Equal(ee.Name, aa.Name);
                        Assert.Equal(ee.Genus, aa.Genus);
                    }
                }
            },
            {
                typeof(Daisy), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (Daisy)e;
                        var aa = (Daisy)a;

                        Assert.Equal(ee.Species, aa.Species);
                        Assert.Equal(ee.Name, aa.Name);
                        Assert.Equal(ee.Genus, aa.Genus);
                    }
                }
            },
            {
                typeof(Rose), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (Rose)e;
                        var aa = (Rose)a;

                        Assert.Equal(ee.Species, aa.Species);
                        Assert.Equal(ee.Name, aa.Name);
                        Assert.Equal(ee.Genus, aa.Genus);
                        Assert.Equal(ee.HasThorns, aa.HasThorns);
                    }
                }
            },
            {
                typeof(Country), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (Country)e;
                        var aa = (Country)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.Name, aa.Name);
                    }
                }
            },
            {
                typeof(Drink), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (Drink)e;
                        var aa = (Drink)a;

                        Assert.Equal(ee.SortIndex, aa.SortIndex);

                        AssertComplexType(ee.ParentComplexType, aa.ParentComplexType);
                        AssertComplexTypes(ee.ComplexTypeCollection, aa.ComplexTypeCollection);
                    }
                }
            },
            {
                typeof(Coke), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (Coke)e;
                        var aa = (Coke)a;

                        Assert.Equal(ee.SortIndex, aa.SortIndex);
                        Assert.Equal(ee.SugarGrams, aa.SugarGrams);
                        Assert.Equal(ee.CaffeineGrams, aa.CaffeineGrams);
                        Assert.Equal(ee.Carbonation, aa.Carbonation);

                        AssertComplexType(ee.ParentComplexType, aa.ParentComplexType);
                        AssertComplexType(ee.ChildComplexType, aa.ChildComplexType);
                        AssertComplexTypes(ee.ComplexTypeCollection, aa.ComplexTypeCollection);
                    }
                }
            },
            {
                typeof(Lilt), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (Lilt)e;
                        var aa = (Lilt)a;

                        Assert.Equal(ee.SortIndex, aa.SortIndex);
                        Assert.Equal(ee.SugarGrams, aa.SugarGrams);
                        Assert.Equal(ee.Carbonation, aa.Carbonation);

                        AssertComplexType(ee.ParentComplexType, aa.ParentComplexType);
                        AssertComplexTypes(ee.ComplexTypeCollection, aa.ComplexTypeCollection);
                    }
                }
            },
            {
                typeof(Tea), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (Tea)e;
                        var aa = (Tea)a;

                        Assert.Equal(ee.SortIndex, aa.SortIndex);
                        Assert.Equal(ee.HasMilk, aa.HasMilk);
                        Assert.Equal(ee.CaffeineGrams, aa.CaffeineGrams);

                        AssertComplexType(ee.ParentComplexType, aa.ParentComplexType);
                        AssertComplexType(ee.ChildComplexType, aa.ChildComplexType);
                        AssertComplexTypes(ee.ComplexTypeCollection, aa.ComplexTypeCollection);
                    }
                }
            },
            {
                typeof(ComplexType), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (ComplexType)e;
                        var aa = (ComplexType)a;

                        AssertComplexType(ee, aa);
                    }
                }
            },
            {
                typeof(NestedComplexType), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (NestedComplexType)e;
                        var aa = (NestedComplexType)a;

                        AssertNestedComplexType(ee, aa);
                    }
                }
            },
        }.ToDictionary(e => e.Key, e => (object)e.Value);

    private void AssertComplexType(ComplexType e, ComplexType a)
    {
        if (!EnableComplexTypes)
        {
            return;
        }

        Assert.Equal(e is null, a is null);

        if (e is not null)
        {
            Assert.Equal(e.UniqueInt, a.UniqueInt);
            Assert.Equal(e.Int, a.Int);

            Assert.Equal(e.Nested is null, a.Nested is null);
            if (e.Nested is not null)
            {
                AssertNestedComplexType(e.Nested, a.Nested);
            }
        }
    }

    private void AssertNestedComplexType(NestedComplexType e, NestedComplexType a)
    {
        if (!EnableComplexTypes)
        {
            return;
        }

        Assert.Equal(e is null, a is null);

        if (e is not null)
        {
            Assert.Equal(e.UniqueInt, a.UniqueInt);
            Assert.Equal(e.NestedInt, a.NestedInt);
        }
    }

    private void AssertComplexTypes(List<ComplexType> e, List<ComplexType> a)
    {
        if (!EnableComplexTypes)
        {
            return;
        }

        Assert.NotNull(e);
        Assert.NotNull(a);

        Assert.Equal(e.Count, a.Count);
        for (var i = 0; i < e.Count; i++)
        {
            AssertComplexType(e[i], a[i]);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<Kiwi>();
        modelBuilder.Entity<Eagle>();
        modelBuilder.Entity<Bird>();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Rose>();
        modelBuilder.Entity<Daisy>();
        modelBuilder.Entity<Flower>();
        modelBuilder.Entity<Plant>().HasKey(e => e.Species);
        modelBuilder.Entity<Country>();
        modelBuilder.Entity<Drink>();
        modelBuilder.Entity<Tea>();
        modelBuilder.Entity<Lilt>();
        modelBuilder.Entity<Coke>();

        if (HasDiscriminator)
        {
            modelBuilder.Entity<Bird>().HasDiscriminator<string>("Discriminator").IsComplete(IsDiscriminatorMappingComplete);

            modelBuilder.Entity<Drink>()
                .HasDiscriminator(e => e.Discriminator)
                .HasValue<Drink>(DrinkType.Drink)
                .HasValue<Coke>(DrinkType.Coke)
                .HasValue<Lilt>(DrinkType.Lilt)
                .HasValue<Tea>(DrinkType.Tea)
                .IsComplete(IsDiscriminatorMappingComplete);
        }
        else
        {
            modelBuilder.Entity<Drink>().Ignore(e => e.Discriminator);
        }

        modelBuilder.Entity<KiwiQuery>().HasDiscriminator().IsComplete(IsDiscriminatorMappingComplete);

        if (EnableFilters)
        {
            modelBuilder.Entity<Animal>().HasQueryFilter(a => a.CountryId == 1);
        }

        modelBuilder.Entity<AnimalQuery>().HasNoKey();
        modelBuilder.Entity<BirdQuery>();
        modelBuilder.Entity<KiwiQuery>();

        if (EnableComplexTypes)
        {
            modelBuilder.Entity<Drink>(b =>
            {
                b.ComplexProperty(d => d.ParentComplexType);
                b.ComplexCollection(d => d.ComplexTypeCollection);
            });

            modelBuilder.Entity<Coke>().ComplexProperty(c => c.ChildComplexType);
            modelBuilder.Entity<Tea>().ComplexProperty(t => t.ChildComplexType);
        }
        else
        {
            modelBuilder.Entity<Drink>(b =>
            {
                b.Ignore(d => d.ParentComplexType);
                b.Ignore(d => d.ComplexTypeCollection);
            });

            modelBuilder.Entity<Coke>().Ignore(c => c.ChildComplexType);
            modelBuilder.Entity<Tea>().Ignore(t => t.ChildComplexType);
        }
    }

    protected override Task SeedAsync(InheritanceContext context)
        => InheritanceContext.SeedAsync(context, UseGeneratedKeys);
}
