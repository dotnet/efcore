// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class InheritanceQueryFixtureBase : SharedStoreFixtureBase<InheritanceContext>, IQueryFixtureBase
    {
        protected override string StoreName { get; } = "InheritanceTest";

        protected virtual bool EnableFilters
            => false;

        protected virtual bool IsDiscriminatorMappingComplete
            => true;

        protected virtual bool HasDiscriminator
            => true;

        public Func<DbContext> GetContextCreator()
            => () => CreateContext();

        public ISetSource GetExpectedData()
            => new InheritanceData();

        public IReadOnlyDictionary<Type, object> GetEntitySorters()
            => new Dictionary<Type, Func<object, object>>
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
                { typeof(Drink), e => ((Drink)e)?.Id },
                { typeof(Coke), e => ((Coke)e)?.Id },
                { typeof(Lilt), e => ((Lilt)e)?.Id },
                { typeof(Tea), e => ((Tea)e)?.Id },
            }.ToDictionary(e => e.Key, e => (object)e.Value);

        public IReadOnlyDictionary<Type, object> GetEntityAsserters()
            => new Dictionary<Type, Action<object, object>>
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
                            Assert.Equal(ee.EagleId, aa.EagleId);
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
                            Assert.Equal(ee.EagleId, aa.EagleId);
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
                            Assert.Equal(ee.EagleId, aa.EagleId);
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

                            Assert.Equal(ee.Id, aa.Id);
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

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.SugarGrams, aa.SugarGrams);
                            Assert.Equal(ee.CaffeineGrams, aa.CaffeineGrams);
                            Assert.Equal(ee.Carbonation, aa.Carbonation);
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

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.SugarGrams, aa.SugarGrams);
                            Assert.Equal(ee.Carbonation, aa.Carbonation);
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

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.HasMilk, aa.HasMilk);
                            Assert.Equal(ee.CaffeineGrams, aa.CaffeineGrams);
                        }
                    }
                },
            }.ToDictionary(e => e.Key, e => (object)e.Value);

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<Kiwi>();
            modelBuilder.Entity<Eagle>();
            modelBuilder.Entity<Bird>();
            modelBuilder.Entity<Animal>().HasKey(e => e.Species);
            modelBuilder.Entity<Rose>();
            modelBuilder.Entity<Daisy>();
            modelBuilder.Entity<Flower>();
            modelBuilder.Entity<Plant>().HasKey(e => e.Species);
            modelBuilder.Entity<Country>();
            modelBuilder.Entity<Drink>();
            modelBuilder.Entity<Tea>();
            modelBuilder.Entity<Lilt>();
            modelBuilder.Entity<Coke>();

            modelBuilder.Entity<Drink>().Property(m => m.Id).ValueGeneratedNever();

            if (HasDiscriminator)
            {
                modelBuilder.Entity<Bird>().HasDiscriminator<string>("Discriminator").IsComplete(IsDiscriminatorMappingComplete);
                modelBuilder.Entity<Drink>().HasDiscriminator().IsComplete(IsDiscriminatorMappingComplete);
            }

            modelBuilder.Entity<KiwiQuery>().HasDiscriminator().IsComplete(IsDiscriminatorMappingComplete);

            if (EnableFilters)
            {
                modelBuilder.Entity<Animal>().HasQueryFilter(a => a.CountryId == 1);
            }

            modelBuilder.Entity<AnimalQuery>().HasNoKey();
            modelBuilder.Entity<BirdQuery>();
            modelBuilder.Entity<KiwiQuery>();
        }

        protected override void Seed(InheritanceContext context)
            => InheritanceContext.Seed(context);

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder);
    }
}
