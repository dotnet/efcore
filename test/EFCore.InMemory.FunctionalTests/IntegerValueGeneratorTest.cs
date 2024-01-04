// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class IntegerValueGeneratorTest
{
    [ConditionalFact]
    public void Each_property_gets_its_own_generator()
    {
        var macs = new Mac[4];
        var toasts = new Toast[4];

        using (var context = new PetsContext("Dance"))
        {
            macs[0] = context.Add(new Mac()).Entity;
            toasts[0] = context.Add(new Toast()).Entity;

            Assert.Equal(1, macs[0].Id);
            Assert.Equal(1, toasts[0].Id);

            macs[1] = context.Add(new Mac()).Entity;
            toasts[1] = context.Add(new Toast()).Entity;

            Assert.Equal(2, macs[1].Id);
            Assert.Equal(2, toasts[1].Id);

            context.SaveChanges();

            Assert.Equal(1, macs[0].Id);
            Assert.Equal(1, toasts[0].Id);
            Assert.Equal(2, macs[1].Id);
            Assert.Equal(2, toasts[1].Id);

            macs[2] = context.Add(new Mac()).Entity;
            toasts[2] = context.Add(new Toast()).Entity;

            Assert.Equal(3, macs[2].Id);
            Assert.Equal(3, toasts[2].Id);

            context.SaveChanges();
        }

        using (var context = new PetsContext("Dance"))
        {
            macs[3] = context.Add(new Mac()).Entity;
            toasts[3] = context.Add(new Toast()).Entity;

            Assert.Equal(4, macs[3].Id);
            Assert.Equal(4, toasts[3].Id);

            context.SaveChanges();
        }

        Assert.Equal(1, macs[0].Id);
        Assert.Equal(1, toasts[0].Id);
        Assert.Equal(2, macs[1].Id);
        Assert.Equal(2, toasts[1].Id);
        Assert.Equal(3, macs[2].Id);
        Assert.Equal(3, toasts[2].Id);
        Assert.Equal(4, macs[3].Id);
        Assert.Equal(4, toasts[3].Id);

        using (var context = new PetsContext("Dance"))
        {
            macs = context.Macs.OrderBy(e => e.Id).ToArray();
            toasts = context.CookedBreads.OrderBy(e => e.Id).ToArray();
        }

        Assert.Equal(1, macs[0].Id);
        Assert.Equal(1, toasts[0].Id);
        Assert.Equal(2, macs[1].Id);
        Assert.Equal(2, toasts[1].Id);
        Assert.Equal(3, macs[2].Id);
        Assert.Equal(3, toasts[2].Id);
        Assert.Equal(4, macs[3].Id);
        Assert.Equal(4, toasts[3].Id);
    }

    [ConditionalFact]
    public void Each_property_gets_its_own_generator_with_seeding()
    {
        var macs = new Mac[4];
        var toasts = new Toast[4];

        using (var context = new PetsContextWithData("Pets II"))
        {
            context.Database.EnsureCreated();

            var savedMacs = context.Macs.OrderBy(e => e.Id).ToList();
            var savedToasts = context.CookedBreads.OrderBy(e => e.Id).ToList();

            Assert.Equal(2, savedMacs.Count);
            Assert.Single(savedToasts);

            Assert.Equal(1, savedMacs[0].Id);
            Assert.Equal(2, savedMacs[1].Id);
            Assert.Equal(1, savedToasts[0].Id);
        }

        using (var context = new PetsContextWithData("Pets II"))
        {
            macs[0] = context.Add(new Mac()).Entity;
            toasts[0] = context.Add(new Toast()).Entity;

            Assert.Equal(3, macs[0].Id);
            Assert.Equal(2, toasts[0].Id);

            macs[1] = context.Add(new Mac()).Entity;
            toasts[1] = context.Add(new Toast()).Entity;

            Assert.Equal(4, macs[1].Id);
            Assert.Equal(3, toasts[1].Id);

            context.SaveChanges();

            Assert.Equal(3, macs[0].Id);
            Assert.Equal(2, toasts[0].Id);
            Assert.Equal(4, macs[1].Id);
            Assert.Equal(3, toasts[1].Id);

            macs[2] = context.Add(new Mac()).Entity;
            toasts[2] = context.Add(new Toast()).Entity;

            Assert.Equal(5, macs[2].Id);
            Assert.Equal(4, toasts[2].Id);

            context.SaveChanges();
        }

        using (var context = new PetsContextWithData("Pets II"))
        {
            macs[3] = context.Add(new Mac()).Entity;
            toasts[3] = context.Add(new Toast()).Entity;

            Assert.Equal(6, macs[3].Id);
            Assert.Equal(5, toasts[3].Id);

            context.SaveChanges();
        }

        Assert.Equal(3, macs[0].Id);
        Assert.Equal(2, toasts[0].Id);
        Assert.Equal(4, macs[1].Id);
        Assert.Equal(3, toasts[1].Id);
        Assert.Equal(5, macs[2].Id);
        Assert.Equal(4, toasts[2].Id);
        Assert.Equal(6, macs[3].Id);
        Assert.Equal(5, toasts[3].Id);

        using (var context = new PetsContextWithData("Pets II"))
        {
            var savedMacs = context.Macs.OrderBy(e => e.Id).ToList();
            var savedToasts = context.CookedBreads.OrderBy(e => e.Id).ToList();

            Assert.Equal(6, savedMacs.Count);
            Assert.Equal(5, savedToasts.Count);

            for (var i = 0; i < 5; i++)
            {
                Assert.Equal(i + 1, savedMacs[i].Id);
                Assert.Equal(i + 1, savedToasts[i].Id);
            }

            Assert.Equal(6, savedMacs[5].Id);
        }
    }

    [ConditionalFact]
    public void Generators_are_associated_with_database_root()
    {
        var serviceProvider1 = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider(validateScopes: true);

        var serviceProvider2 = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider(validateScopes: true);

        var root = new InMemoryDatabaseRoot();

        var macs = new Mac[2];
        var toasts = new Toast[2];

        using (var context = new PetsContext("Drink", root, serviceProvider1))
        {
            macs[0] = context.Add(new Mac()).Entity;
            toasts[0] = context.Add(new Toast()).Entity;

            Assert.Equal(1, macs[0].Id);
            Assert.Equal(1, toasts[0].Id);

            context.SaveChanges();
        }

        using (var context = new PetsContext("Drink", root, serviceProvider2))
        {
            macs[1] = context.Add(new Mac()).Entity;
            toasts[1] = context.Add(new Toast()).Entity;

            Assert.Equal(2, macs[1].Id);
            Assert.Equal(2, toasts[1].Id);

            context.SaveChanges();
        }

        Assert.Equal(1, macs[0].Id);
        Assert.Equal(1, toasts[0].Id);
        Assert.Equal(2, macs[1].Id);
        Assert.Equal(2, toasts[1].Id);
    }

    [ConditionalFact]
    public void Mixing_explicit_values_with_generated_values_with_care_works()
    {
        var macs = new Mac[4];
        var toasts = new Toast[4];

        using var context = new PetsContext("Wercs");
        macs[0] = context.Add(new Mac { Id = 10 }).Entity;
        toasts[0] = context.Add(new Toast { Id = 100 }).Entity;

        context.SaveChanges();

        macs[1] = context.Add(new Mac()).Entity;
        toasts[1] = context.Add(new Toast()).Entity;

        context.SaveChanges();

        Assert.Equal(10, macs[0].Id);
        Assert.Equal(100, toasts[0].Id);
        Assert.Equal(11, macs[1].Id);
        Assert.Equal(101, toasts[1].Id);

        macs[2] = context.Add(new Mac { Id = 20 }).Entity;
        toasts[2] = context.Add(new Toast { Id = 200 }).Entity;

        context.SaveChanges();

        macs[3] = context.Add(new Mac()).Entity;
        toasts[3] = context.Add(new Toast()).Entity;

        context.SaveChanges();

        Assert.Equal(20, macs[2].Id);
        Assert.Equal(200, toasts[2].Id);
        Assert.Equal(21, macs[3].Id);
        Assert.Equal(201, toasts[3].Id);
    }

    [ConditionalFact]
    public void Each_database_gets_its_own_generators()
    {
        var macs = new List<Mac>();
        var toasts = new List<Toast>();

        using (var context = new PetsContext("Nothing"))
        {
            macs.Add(context.Add(new Mac()).Entity);
            toasts.Add(context.Add(new Toast()).Entity);

            Assert.Equal(1, macs[0].Id);
            Assert.Equal(1, toasts[0].Id);

            context.SaveChanges();
        }

        using (var context = new PetsContext("Else"))
        {
            macs.Add(context.Add(new Mac()).Entity);
            toasts.Add(context.Add(new Toast()).Entity);

            Assert.Equal(1, macs[1].Id);
            Assert.Equal(1, toasts[1].Id);

            context.SaveChanges();
        }

        Assert.Equal(1, macs[0].Id);
        Assert.Equal(1, toasts[0].Id);
        Assert.Equal(1, macs[1].Id);
        Assert.Equal(1, toasts[1].Id);
    }

    [ConditionalFact]
    public void Each_root_gets_its_own_generators()
    {
        var macs = new List<Mac>();
        var toasts = new List<Toast>();

        using (var context = new PetsContext("To", new InMemoryDatabaseRoot()))
        {
            macs.Add(context.Add(new Mac()).Entity);
            toasts.Add(context.Add(new Toast()).Entity);

            Assert.Equal(1, macs[0].Id);
            Assert.Equal(1, toasts[0].Id);

            context.SaveChanges();
        }

        using (var context = new PetsContext("To", new InMemoryDatabaseRoot()))
        {
            macs.Add(context.Add(new Mac()).Entity);
            toasts.Add(context.Add(new Toast()).Entity);

            Assert.Equal(1, macs[1].Id);
            Assert.Equal(1, toasts[1].Id);

            context.SaveChanges();
        }

        Assert.Equal(1, macs[0].Id);
        Assert.Equal(1, toasts[0].Id);
        Assert.Equal(1, macs[1].Id);
        Assert.Equal(1, toasts[1].Id);
    }

    [ConditionalFact]
    public void EnsureDeleted_resets_generators()
    {
        var macs = new List<Mac>();
        var toasts = new List<Toast>();

        using (var context = new PetsContext("Do"))
        {
            macs.Add(context.Add(new Mac()).Entity);
            toasts.Add(context.Add(new Toast()).Entity);

            Assert.Equal(1, macs[0].Id);
            Assert.Equal(1, toasts[0].Id);

            context.SaveChanges();
        }

        using (var context = new PetsContext("Do"))
        {
            context.Database.EnsureDeleted();

            macs.Add(context.Add(new Mac()).Entity);
            toasts.Add(context.Add(new Toast()).Entity);

            Assert.Equal(1, macs[1].Id);
            Assert.Equal(1, toasts[1].Id);

            context.SaveChanges();
        }

        Assert.Equal(1, macs[0].Id);
        Assert.Equal(1, toasts[0].Id);
        Assert.Equal(1, macs[1].Id);
        Assert.Equal(1, toasts[1].Id);
    }

    private class PetsContext(
        string databaseName,
        InMemoryDatabaseRoot root = null,
        IServiceProvider internalServiceProvider = null) : DbContext
    {
        private readonly string _databaseName = databaseName;
        private readonly InMemoryDatabaseRoot _root = root;
        private readonly IServiceProvider _internalServiceProvider = internalServiceProvider;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInternalServiceProvider(_internalServiceProvider);

            if (_root == null)
            {
                optionsBuilder.UseInMemoryDatabase(_databaseName);
            }
            else
            {
                optionsBuilder.UseInMemoryDatabase(_databaseName, _root);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cat>();
            modelBuilder.Entity<Dog>();
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Toast> CookedBreads { get; set; }
        public DbSet<Olive> Olives { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Mac> Macs { get; set; }
        public DbSet<Smokey> Smokeys { get; set; }
        public DbSet<Alice> Alices { get; set; }
    }

    private class PetsContextWithData(
        string databaseName,
        InMemoryDatabaseRoot root = null,
        IServiceProvider internalServiceProvider = null) : PetsContext(databaseName, root, internalServiceProvider)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Toast>().HasData(new Toast { Id = 1 });
            modelBuilder.Entity<Mac>().HasData(new Mac { Id = 1 }, new Mac { Id = 2 });
        }
    }

    private class Dog
    {
        public int Id { get; set; }
    }

    private class Toast : Dog;

    private class Olive : Dog;

    private class Cat
    {
        public int Id { get; set; }
    }

    private class Mac : Cat;

    private class Smokey : Cat;

    private class Alice : Cat;
}
