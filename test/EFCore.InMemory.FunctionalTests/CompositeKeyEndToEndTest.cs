// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class CompositeKeyEndToEndTest
    {
        [Fact]
        public async Task Can_use_two_non_generated_integers_as_composite_key_end_to_end()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var ticks = DateTime.UtcNow.Ticks;

            using (var context = new BronieContext(serviceProvider))
            {
                context.Add(
                    new Pegasus
                    {
                        Id1 = ticks,
                        Id2 = ticks + 1,
                        Name = "Rainbow Dash"
                    });
                await context.SaveChangesAsync();
            }

            using (var context = new BronieContext(serviceProvider))
            {
                var pegasus = context.Pegasuses.Single(e => (e.Id1 == ticks) && (e.Id2 == ticks + 1));

                pegasus.Name = "Rainbow Crash";

                await context.SaveChangesAsync();
            }

            using (var context = new BronieContext(serviceProvider))
            {
                var pegasus = context.Pegasuses.Single(e => (e.Id1 == ticks) && (e.Id2 == ticks + 1));

                Assert.Equal("Rainbow Crash", pegasus.Name);

                context.Pegasuses.Remove(pegasus);

                await context.SaveChangesAsync();
            }

            using (var context = new BronieContext(serviceProvider))
            {
                Assert.Equal(0, context.Pegasuses.Count(e => (e.Id1 == ticks) && (e.Id2 == ticks + 1)));
            }
        }

        [Fact]
        public async Task Can_use_generated_values_in_composite_key_end_to_end()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            long id1;
            var id2 = DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture);
            Guid id3;

            using (var context = new BronieContext(serviceProvider))
            {
                var added = context.Add(
                    new Unicorn
                    {
                        Id2 = id2,
                        Name = "Rarity"
                    }).Entity;

                Assert.True(added.Id1 > 0);
                Assert.NotEqual(Guid.Empty, added.Id3);

                await context.SaveChangesAsync();

                id1 = added.Id1;
                id3 = added.Id3;
            }

            using (var context = new BronieContext(serviceProvider))
            {
                Assert.Equal(1, context.Unicorns.Count(e => (e.Id1 == id1) && (e.Id2 == id2) && (e.Id3 == id3)));
            }

            using (var context = new BronieContext(serviceProvider))
            {
                var unicorn = context.Unicorns.Single(e => (e.Id1 == id1) && (e.Id2 == id2) && (e.Id3 == id3));

                unicorn.Name = "Bad Hair Day";

                await context.SaveChangesAsync();
            }

            using (var context = new BronieContext(serviceProvider))
            {
                var unicorn = context.Unicorns.Single(e => (e.Id1 == id1) && (e.Id2 == id2) && (e.Id3 == id3));

                Assert.Equal("Bad Hair Day", unicorn.Name);

                context.Unicorns.Remove(unicorn);

                await context.SaveChangesAsync();
            }

            using (var context = new BronieContext(serviceProvider))
            {
                Assert.Equal(0, context.Unicorns.Count(e => (e.Id1 == id1) && (e.Id2 == id2) && (e.Id3 == id3)));
            }
        }

        [Fact]
        public async Task Only_one_part_of_a_composite_key_needs_to_vary_for_uniquness()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var ids = new int[3];

            using (var context = new BronieContext(serviceProvider))
            {
                var pony1 = context.Add(
                    new EarthPony
                    {
                        Id2 = 7,
                        Name = "Apple Jack 1"
                    }).Entity;
                var pony2 = context.Add(
                    new EarthPony
                    {
                        Id2 = 7,
                        Name = "Apple Jack 2"
                    }).Entity;
                var pony3 = context.Add(
                    new EarthPony
                    {
                        Id2 = 7,
                        Name = "Apple Jack 3"
                    }).Entity;

                await context.SaveChangesAsync();

                ids[0] = pony1.Id1;
                ids[1] = pony2.Id1;
                ids[2] = pony3.Id1;
            }

            using (var context = new BronieContext(serviceProvider))
            {
                var ponies = context.EarthPonies.ToList();
                Assert.Equal(ponies.Count, ponies.Count(e => e.Name == "Apple Jack 1") * 3);

                Assert.Equal("Apple Jack 1", ponies.Single(e => e.Id1 == ids[0]).Name);
                Assert.Equal("Apple Jack 2", ponies.Single(e => e.Id1 == ids[1]).Name);
                Assert.Equal("Apple Jack 3", ponies.Single(e => e.Id1 == ids[2]).Name);

                ponies.Single(e => e.Id1 == ids[1]).Name = "Pinky Pie 2";

                await context.SaveChangesAsync();
            }

            using (var context = new BronieContext(serviceProvider))
            {
                var ponies = context.EarthPonies.ToArray();
                Assert.Equal(ponies.Length, ponies.Count(e => e.Name == "Apple Jack 1") * 3);

                Assert.Equal("Apple Jack 1", ponies.Single(e => e.Id1 == ids[0]).Name);
                Assert.Equal("Pinky Pie 2", ponies.Single(e => e.Id1 == ids[1]).Name);
                Assert.Equal("Apple Jack 3", ponies.Single(e => e.Id1 == ids[2]).Name);

                context.EarthPonies.RemoveRange(ponies);

                await context.SaveChangesAsync();
            }

            using (var context = new BronieContext(serviceProvider))
            {
                Assert.Equal(0, context.EarthPonies.Count());
            }
        }

        private class BronieContext : PoolableDbContext
        {
            private readonly IServiceProvider _serviceProvider;

            public BronieContext(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public DbSet<Pegasus> Pegasuses { get; set; }
            public DbSet<Unicorn> Unicorns { get; set; }
            public DbSet<EarthPony> EarthPonies { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseInMemoryDatabase(nameof(BronieContext)).UseInternalServiceProvider(_serviceProvider);

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Pegasus>().HasKey(
                    e => new
                    {
                        e.Id1,
                        e.Id2
                    });
                modelBuilder
                    .Entity<Pegasus>(
                        b =>
                        {
                            b.HasKey(
                                e => new
                                {
                                    e.Id1,
                                    e.Id2
                                });
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd();
                        });

                modelBuilder.Entity<Unicorn>().HasKey(
                    e => new
                    {
                        e.Id1,
                        e.Id2,
                        e.Id3
                    });
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

                modelBuilder.Entity<EarthPony>().HasKey(
                    e => new
                    {
                        e.Id1,
                        e.Id2
                    });
                modelBuilder.Entity<EarthPony>(
                    b =>
                    {
                        b.HasKey(
                            e => new
                            {
                                e.Id1,
                                e.Id2
                            });
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd();
                    });
            }
        }

        private class Pegasus
        {
            public long Id1 { get; set; }
            public long Id2 { get; set; }
            public string Name { get; set; }
        }

        private class Unicorn
        {
            public int Id1 { get; set; }
            public string Id2 { get; set; }
            public Guid Id3 { get; set; }
            public string Name { get; set; }
        }

        private class EarthPony
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }
            public string Name { get; set; }
        }
    }
}
