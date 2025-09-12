// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class EnumDiscriminatorCosmosTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string StoreName
        => "EnumDiscriminatorCosmosTest";

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;
    [ConditionalFact]
    public async Task Enum_discriminator_saved_as_string_consistently()
    {
        var contextFactory = await InitializeAsync<MyDbContext>(
            shouldLogCategory: _ => true);

        var thingy = new DerivedThingy2
        {
            Id = "A",
            Name = "A",
            Type = ThingyType.ThingyTwo
        };

        using (var context = contextFactory.CreateContext())
        {
            context.Things.Add(thingy);
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateContext())
        {
            var entity = context.Things.Single(x => x.Id == "A");
            entity.Name = "A updated";
            await context.SaveChangesAsync();

            // Verify entity can still be found after update
            var reloadedEntity = context.Things.Single(x => x.Id == "A");
            Assert.Equal("A updated", reloadedEntity.Name);
            Assert.Equal(ThingyType.ThingyTwo, reloadedEntity.Type);
        }
    }

    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Thingy> Things { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Thingy>(b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.Type)
                    .HasConversion<string>();
                b.HasDiscriminator(e => e.Type)
                    .HasValue<DerivedThingy2>(ThingyType.ThingyTwo);
            });

            modelBuilder.Entity<DerivedThingy2>();
        }
    }

    public enum ThingyType
    {
        ThingyOne = 0,
        ThingyTwo = 1
    }

    public class Thingy
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ThingyType Type { get; set; }
    }

    public class DerivedThingy2 : Thingy
    {
        public string SomethingForB { get; set; }
    }
}