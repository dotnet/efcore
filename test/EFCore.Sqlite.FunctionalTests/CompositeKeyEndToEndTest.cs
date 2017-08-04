// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Utilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class CompositeKeyEndToEndTest : IClassFixture<CompositeKeyEndToEndTest.CompositeKeyEndToEndFixture>
    {
        public CompositeKeyEndToEndTest(CompositeKeyEndToEndFixture fixture) => Fixture = fixture;

        private CompositeKeyEndToEndFixture Fixture { get; }

        [Fact]
        public async Task Can_use_two_non_generated_integers_as_composite_key_end_to_end()
        {
            var ticks = DateTime.UtcNow.Ticks;

            using (var context = CreateContext())
            {
                context.Add(new Pegasus { Id1 = ticks, Id2 = ticks + 1, Name = "Rainbow Dash" });
                await context.SaveChangesAsync();
            }

            using (var context = CreateContext())
            {
                var pegasus = context.Pegasuses.Single(e => e.Id1 == ticks && e.Id2 == ticks + 1);

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

        [Fact]
        public async Task Only_one_part_of_a_composite_key_needs_to_vary_for_uniqueness()
        {
            var ids = new int[3];

            using (var context = CreateContext())
            {
                var pony1 = context.Add(new EarthPony { Id1 = 1, Id2 = 7, Name = "Apple Jack 1" }).Entity;
                var pony2 = context.Add(new EarthPony { Id1 = 2, Id2 = 7, Name = "Apple Jack 2" }).Entity;
                var pony3 = context.Add(new EarthPony { Id1 = 3, Id2 = 7, Name = "Apple Jack 3" }).Entity;

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

        private BronieContext CreateContext() => (BronieContext)Fixture.CreateContext();

        public class CompositeKeyEndToEndFixture : SharedStoreFixtureBase<DbContext>
        {
            protected override string StoreName { get; } = "CompositeKeyEndToEndTest";
            protected override ITestStoreFactory<TestStore> TestStoreFactory => SqliteTestStoreFactory.Instance;
            protected override Type ContextType { get; } = typeof(BronieContext);
        }

        private class BronieContext : DbContext
        {
            public BronieContext(DbContextOptions options)
                : base(options)
            {
            }

            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public DbSet<Pegasus> Pegasuses { get; set; }
            public DbSet<EarthPony> EarthPonies { get; set; }
            // ReSharper restore UnusedAutoPropertyAccessor.Local

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Pegasus>(b =>
                    {
                        b.ToTable("Pegasus");
                        b.HasKey(e => new { e.Id1, e.Id2 });
                    });

                modelBuilder.Entity<EarthPony>(b =>
                    {
                        b.ToTable("EarthPony");
                        b.HasKey(e => new { e.Id1, e.Id2 });
                        b.Property(e => e.Id1);
                    });
            }
        }

        private class Pegasus
        {
            public long Id1 { get; set; }
            public long Id2 { get; set; }
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
