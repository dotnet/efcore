// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore
{
    public abstract class StoreGeneratedFixupRelationalTestBase<TFixture> : StoreGeneratedFixupTestBase<TFixture>
        where TFixture : StoreGeneratedFixupRelationalTestBase<TFixture>.StoreGeneratedFixupRelationalFixtureBase, new()
    {
        protected StoreGeneratedFixupRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        public abstract class StoreGeneratedFixupRelationalFixtureBase : StoreGeneratedFixupFixtureBase
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.Entity<Item>().HasOne(i => i.Game).WithMany(g => g.Items).HasConstraintName("FK_GameEntity_Game_GameId");
                modelBuilder.Entity<Actor>().HasOne(i => i.Game).WithMany(g => g.Actors).HasConstraintName("FK_GameEntity_Game_GameId");
            }
        }
    }
}
