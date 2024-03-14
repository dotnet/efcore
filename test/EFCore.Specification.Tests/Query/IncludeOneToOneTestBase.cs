// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class IncludeOneToOneTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : IncludeOneToOneTestBase<TFixture>.OneToOneQueryFixtureBase, new()
{
    protected IncludeOneToOneTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    public TFixture Fixture { get; }

    [ConditionalFact]
    public virtual void Include_address()
    {
        using var context = CreateContext();
        var people
            = context.Set<Person>()
                .Include(p => p.Address)
                .ToList();

        Assert.Equal(4, people.Count);
        Assert.Equal(3, people.Count(p => p.Address != null));
        Assert.Equal(4 + 3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public virtual void Include_address_EF_Property()
    {
        using var context = CreateContext();
        var people
            = context.Set<Person>()
                .Include(p => EF.Property<Person>(p, "Address"))
                .ToList();

        Assert.Equal(4, people.Count);
        Assert.Equal(3, people.Count(p => p.Address != null));
        Assert.Equal(4 + 3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public virtual void Include_address_shadow()
    {
        using var context = CreateContext();
        var people
            = context.Set<Person2>()
                .Include(p => p.Address)
                .ToList();

        Assert.Equal(3, people.Count);
        Assert.True(people.All(p => p.Address != null));
        Assert.Equal(3 + 3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public virtual void Include_address_no_tracking()
    {
        using var context = CreateContext();
        var people
            = context.Set<Person>()
                .Include(p => p.Address)
                .AsNoTracking()
                .ToList();

        Assert.Equal(4, people.Count);
        Assert.Equal(3, people.Count(p => p.Address != null));
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [ConditionalFact]
    public virtual void Include_address_no_tracking_EF_Property()
    {
        using var context = CreateContext();
        var people
            = context.Set<Person>()
                .Include(p => EF.Property<Person>(p, "Address"))
                .AsNoTracking()
                .ToList();

        Assert.Equal(4, people.Count);
        Assert.Equal(3, people.Count(p => p.Address != null));
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [ConditionalFact]
    public virtual void Include_person()
    {
        using var context = CreateContext();
        var addresses
            = context.Set<Address>()
                .Include(a => a.Resident)
                .ToList();

        Assert.Equal(3, addresses.Count);
        Assert.True(addresses.All(p => p.Resident != null));
        Assert.Equal(3 + 3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public virtual void Include_person_EF_Property()
    {
        using var context = CreateContext();
        var addresses
            = context.Set<Address>()
                .Include(a => EF.Property<Address>(a, "Resident"))
                .ToList();

        Assert.Equal(3, addresses.Count);
        Assert.True(addresses.All(p => p.Resident != null));
        Assert.Equal(3 + 3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public virtual void Include_person_shadow()
    {
        using var context = CreateContext();
        var addresses
            = context.Set<Address2>()
                .Include(a => a.Resident)
                .ToList();

        Assert.Equal(3, addresses.Count);
        Assert.True(addresses.All(p => p.Resident != null));
        Assert.Equal(3 + 3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public virtual void Include_person_no_tracking()
    {
        using var context = CreateContext();
        var addresses
            = context.Set<Address>()
                .Include(a => a.Resident)
                .AsNoTracking()
                .ToList();

        Assert.Equal(3, addresses.Count);
        Assert.True(addresses.All(p => p.Resident != null));
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [ConditionalFact]
    public virtual void Include_person_no_tracking_EF_Property()
    {
        using var context = CreateContext();
        var addresses
            = context.Set<Address>()
                .Include(a => EF.Property<Address>(a, "Resident"))
                .AsNoTracking()
                .ToList();

        Assert.Equal(3, addresses.Count);
        Assert.True(addresses.All(p => p.Resident != null));
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [ConditionalFact]
    public virtual void Include_address_when_person_already_tracked()
    {
        using var context = CreateContext();
        var person
            = context.Set<Person>()
                .Single(p => p.Name == "John Snow");

        var people
            = context.Set<Person>()
                .Include(p => p.Address)
                .ToList();

        Assert.Equal(4, people.Count);
        Assert.Contains(person, people);
        Assert.Equal(3, people.Count(p => p.Address != null));
        Assert.Equal(4 + 3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public virtual void Include_person_when_address_already_tracked()
    {
        using var context = CreateContext();
        var address
            = context.Set<Address>()
                .Single(a => a.City == "Meereen");

        var addresses
            = context.Set<Address>()
                .Include(a => a.Resident)
                .ToList();

        Assert.Equal(3, addresses.Count);
        Assert.Contains(address, addresses);
        Assert.True(addresses.All(p => p.Resident != null));
        Assert.Equal(3 + 3, context.ChangeTracker.Entries().Count());
    }

    protected virtual DbContext CreateContext()
        => Fixture.CreateContext();

    public abstract class OneToOneQueryFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName
            => "OneToOneQueryTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder
                .Entity<Address>(
                    e => e.HasOne(a => a.Resident).WithOne(p => p.Address)
                        .HasPrincipalKey<Person>(person => person.Id));

            modelBuilder.Entity<Address2>().Property<int>("PersonId");

            modelBuilder
                .Entity<Person2>(
                    e => e.HasOne(p => p.Address).WithOne(a => a.Resident)
                        .HasForeignKey<Address2>("PersonId"));
        }

        protected override Task SeedAsync(PoolableDbContext context)
        {
            var address1 = new Address { Street = "3 Dragons Way", City = "Meereen" };
            var address2 = new Address { Street = "42 Castle Black", City = "The Wall" };
            var address3 = new Address { Street = "House of Black and White", City = "Braavos" };

            context.Set<Person>().AddRange(
                new Person { Name = "Daenerys Targaryen", Address = address1 },
                new Person { Name = "John Snow", Address = address2 },
                new Person { Name = "Arya Stark", Address = address3 },
                new Person { Name = "Harry Strickland" });

            context.Set<Address>().AddRange(address1, address2, address3);

            var address21 = new Address2
            {
                Id = "1",
                Street = "3 Dragons Way",
                City = "Meereen"
            };
            var address22 = new Address2
            {
                Id = "2",
                Street = "42 Castle Black",
                City = "The Wall"
            };
            var address23 = new Address2
            {
                Id = "3",
                Street = "House of Black and White",
                City = "Braavos"
            };

            context.Set<Person2>().AddRange(
                new Person2 { Name = "Daenerys Targaryen", Address = address21 },
                new Person2 { Name = "John Snow", Address = address22 },
                new Person2 { Name = "Arya Stark", Address = address23 });

            context.Set<Address2>().AddRange(address21, address22, address23);

            return context.SaveChangesAsync();
        }
    }

    protected class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Address Address { get; set; }
    }

    protected class Address
    {
        public int Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public Person Resident { get; set; }
    }

    protected class Person2
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // ReSharper disable once MemberHidesStaticFromOuterClass
        public Address2 Address { get; set; }
    }

    protected class Address2
    {
        public string Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public Person2 Resident { get; set; }
    }
}
