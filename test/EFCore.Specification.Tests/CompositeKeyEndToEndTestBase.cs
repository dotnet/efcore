// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class CompositeKeyEndToEndTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : CompositeKeyEndToEndTestBase<TFixture>.CompositeKeyEndToEndFixtureBase
{
    protected CompositeKeyEndToEndTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    private TFixture Fixture { get; }

    [ConditionalFact]
    public virtual async Task Can_use_two_non_generated_integers_as_composite_key_end_to_end()
    {
        var ticks = DateTime.UtcNow.Ticks;

        using (var context = CreateContext())
        {
            var pegasus = await context.AddAsync(
                new Pegasus
                {
                    Id1 = ticks,
                    Id2 = ticks + 1,
                    Name = "Rainbow Dash"
                });

            Assert.Equal("Pegasus", pegasus.Entity.Discriminator);

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var pegasus = context.Pegasuses.Single(e => e.Id1 == ticks && e.Id2 == ticks + 1);

            Assert.Equal("Pegasus", pegasus.Discriminator);
            pegasus.Name = "Rainbow Crash";

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var pegasus = context.Pegasuses.Single(e => e.Id1 == ticks && e.Id2 == ticks + 1);

            Assert.Equal("Rainbow Crash", pegasus.Name);

            context.Pegasuses.Remove(pegasus);

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            Assert.Equal(0, context.Pegasuses.Count(e => e.Id1 == ticks && e.Id2 == ticks + 1));
        }
    }

    [ConditionalFact]
    public virtual async Task Can_use_generated_values_in_composite_key_end_to_end()
    {
        long id1;
        var id2 = DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture);
        Guid id3;

        using (var context = CreateContext())
        {
            context.Database.EnsureCreatedResiliently();

            var added = (await context.AddAsync(
                new Unicorn { Id2 = id2, Name = "Rarity" })).Entity;

            await context.SaveChangesAsync();

            Assert.True(added.Id1 > 0);
            Assert.NotEqual(Guid.Empty, added.Id3);

            id1 = added.Id1;
            id3 = added.Id3;
        }

        using (var context = CreateContext())
        {
            Assert.Equal(1, context.Unicorns.Count(e => e.Id1 == id1 && e.Id2 == id2 && e.Id3 == id3));
        }

        using (var context = CreateContext())
        {
            var unicorn = context.Unicorns.Single(e => e.Id1 == id1 && e.Id2 == id2 && e.Id3 == id3);

            unicorn.Name = "Bad Hair Day";

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var unicorn = context.Unicorns.Single(e => e.Id1 == id1 && e.Id2 == id2 && e.Id3 == id3);

            Assert.Equal("Bad Hair Day", unicorn.Name);

            context.Unicorns.Remove(unicorn);

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            Assert.Equal(0, context.Unicorns.Count(e => e.Id1 == id1 && e.Id2 == id2 && e.Id3 == id3));
        }
    }

    [ConditionalFact]
    public virtual async Task Only_one_part_of_a_composite_key_needs_to_vary_for_uniqueness()
    {
        var ids = new int[3];

        using (var context = CreateContext())
        {
            var pony1 = (await context.AddAsync(
                new EarthPony
                {
                    Id1 = 1,
                    Id2 = 7,
                    Name = "Apple Jack 1"
                })).Entity;
            var pony2 = (await context.AddAsync(
                new EarthPony
                {
                    Id1 = 2,
                    Id2 = 7,
                    Name = "Apple Jack 2"
                })).Entity;
            var pony3 = (await context.AddAsync(
                new EarthPony
                {
                    Id1 = 3,
                    Id2 = 7,
                    Name = "Apple Jack 3"
                })).Entity;

            await context.SaveChangesAsync();

            ids[0] = pony1.Id1;
            ids[1] = pony2.Id1;
            ids[2] = pony3.Id1;
        }

        using (var context = CreateContext())
        {
            var ponies = context.EarthPonies.ToList();
            Assert.Equal(ponies.Count, ponies.Count(e => e.Name == "Apple Jack 1") * 3);

            Assert.Equal("Apple Jack 1", ponies.Single(e => e.Id1 == ids[0]).Name);
            Assert.Equal("Apple Jack 2", ponies.Single(e => e.Id1 == ids[1]).Name);
            Assert.Equal("Apple Jack 3", ponies.Single(e => e.Id1 == ids[2]).Name);

            ponies.Single(e => e.Id1 == ids[1]).Name = "Pinky Pie 2";

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            var ponies = context.EarthPonies.ToArray();
            Assert.Equal(ponies.Length, ponies.Count(e => e.Name == "Apple Jack 1") * 3);

            Assert.Equal("Apple Jack 1", ponies.Single(e => e.Id1 == ids[0]).Name);
            Assert.Equal("Pinky Pie 2", ponies.Single(e => e.Id1 == ids[1]).Name);
            Assert.Equal("Apple Jack 3", ponies.Single(e => e.Id1 == ids[2]).Name);

            context.EarthPonies.RemoveRange(ponies);

            await context.SaveChangesAsync();
        }

        using (var context = CreateContext())
        {
            Assert.Equal(0, context.EarthPonies.Count());
        }
    }

    protected BronieContext CreateContext()
        => (BronieContext)Fixture.CreateContext();

    public abstract class CompositeKeyEndToEndFixtureBase : SharedStoreFixtureBase<DbContext>
    {
        protected override string StoreName
            => "CompositeKeyEndToEndTest";

        protected override Type ContextType { get; } = typeof(BronieContext);
    }

    protected class BronieContext(DbContextOptions options) : PoolableDbContext(options)
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public DbSet<Pegasus> Pegasuses { get; set; }
        public DbSet<Unicorn> Unicorns { get; set; }

        public DbSet<EarthPony> EarthPonies { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Local

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Flyer>(
                b =>
                {
                    b.HasKey(
                        e => new
                        {
                            e.Id1,
                            e.Id2,
                            e.Discriminator
                        });
                });

            modelBuilder.Entity<Pegasus>();

            modelBuilder.Entity<Unicorn>(
                b =>
                {
                    b.HasKey(
                        e => new
                        {
                            e.Id1,
                            e.Id2,
                            e.Id3
                        });
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id3).ValueGeneratedOnAdd();
                });

            modelBuilder.Entity<EarthPony>(
                b =>
                {
                    b.HasKey(
                        e => new { e.Id1, e.Id2 });
                    b.Property(e => e.Id1);
                });
        }
    }

    protected abstract class Flyer
    {
        public string Discriminator { get; set; }
        public long Id1 { get; set; }
        public long Id2 { get; set; }
    }

    protected class Pegasus : Flyer
    {
        public string Name { get; set; }
    }

    protected class Unicorn
    {
        public int Id1 { get; set; }
        public string Id2 { get; set; }
        public Guid Id3 { get; set; }
        public string Name { get; set; }
    }

    protected class EarthPony
    {
        public int Id1 { get; set; }
        public int Id2 { get; set; }
        public string Name { get; set; }
    }
}
