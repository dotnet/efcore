// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

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

            modelBuilder.Entity<Item>(
                eb =>
                {
                    eb.HasOne(i => i.Game).WithMany(g => g.Items).HasConstraintName("FK_GameEntity_Game_GameId");
                    eb.HasOne(i => i.Level).WithMany(g => g.Items).HasConstraintName("FK_GameEntity_Level_GameId_LevelId");
                    eb.HasIndex(
                        i => new { i.GameId, i.LevelId }, "IX_GameEntity_GameId_LevelId");
                });

            modelBuilder.Entity<Actor>(
                eb =>
                {
                    eb.HasOne(a => a.Game).WithMany(g => g.Actors).HasConstraintName("FK_GameEntity_Game_GameId");
                    eb.HasOne(a => a.Level).WithMany(g => g.Actors).HasConstraintName("FK_GameEntity_Level_GameId_LevelId");
                    eb.HasIndex(
                        a => new { a.GameId, a.LevelId }, "IX_GameEntity_GameId_LevelId");
                });
        }
    }
}
