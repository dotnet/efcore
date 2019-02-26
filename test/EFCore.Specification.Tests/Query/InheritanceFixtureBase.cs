// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.Inheritance;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class InheritanceFixtureBase : SharedStoreFixtureBase<InheritanceContext>
    {
        protected override string StoreName { get; } = "InheritanceTest";
        protected virtual bool EnableFilters => false;

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

            if (EnableFilters)
            {
                modelBuilder.Entity<Animal>().HasQueryFilter(a => a.CountryId == 1);
            }

            modelBuilder.Entity<AnimalQuery>().HasNoKey();
            modelBuilder.Entity<BirdQuery>();
            modelBuilder.Entity<KiwiQuery>();
        }

        protected override void Seed(InheritanceContext context) => InheritanceContext.SeedData(context);
    }
}
