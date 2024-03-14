// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;
using Newtonsoft.Json.Linq;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class EmbeddedDocumentsTest : IClassFixture<EmbeddedDocumentsTest.CosmosFixture>
{
    private const string DatabaseName = "EmbeddedDocumentsTest";

    protected CosmosFixture Fixture { get; }

    public EmbeddedDocumentsTest(CosmosFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact(Skip = "Issue #17670")]
    public virtual async Task Can_update_dependents()
    {
        var options = await Fixture.CreateOptions();
        Operator firstOperator;
        Engine firstEngine;
        using (var context = new EmbeddedTransportationContext(options))
        {
            firstOperator = context.Set<Vehicle>().Select(v => v.Operator).OrderBy(o => o.VehicleName).First();
            firstOperator.Name += "1";
            firstEngine = context.Set<PoweredVehicle>().Select(v => v.Engine).OrderBy(o => o.VehicleName).First();
            firstEngine.Description += "1";

            await context.SaveChangesAsync();
        }

        using (var context = new EmbeddedTransportationContext(options))
        {
            var @operator = await context.Set<Vehicle>()
                .Select(v => v.Operator).OrderBy(o => o.VehicleName).FirstAsync();
            Assert.Equal(firstOperator.Name, @operator.Name);

            var engine = await context.Set<PoweredVehicle>()
                .Select(v => v.Engine).OrderBy(o => o.VehicleName).FirstAsync();
            Assert.Equal(firstEngine.Description, engine.Description);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_update_owner_with_dependents()
    {
        var options = await Fixture.CreateOptions();
        Operator firstOperator;
        using (var context = new EmbeddedTransportationContext(options))
        {
            firstOperator = (await context.Set<Vehicle>().OrderBy(o => o.Name).FirstAsync()).Operator;
            firstOperator.Name += "1";

            await context.SaveChangesAsync();
        }

        using (var context = new EmbeddedTransportationContext(options))
        {
            var vehicle = await context.Set<Vehicle>().OrderBy(o => o.Name).FirstAsync();
            Assert.Equal(firstOperator.Name, vehicle.Operator.Name);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_attach_owner_with_dependents()
    {
        var options = await Fixture.CreateOptions();
        Vehicle firstVehicle;
        using (var context = new EmbeddedTransportationContext(options))
        {
            firstVehicle = await context.Set<Vehicle>().OrderBy(o => o.Name).FirstAsync();

            await context.SaveChangesAsync();
        }

        using (var context = new EmbeddedTransportationContext(options))
        {
            //Issue #15289
            var firstVehicleEntry = await context.AddAsync(firstVehicle);
            firstVehicleEntry.State = EntityState.Unchanged;
            firstVehicle.Operator.Name += "1";

            await context.SaveChangesAsync();
        }

        using (var context = new EmbeddedTransportationContext(options))
        {
            var vehicle = await context.Set<Vehicle>().OrderBy(o => o.Name).FirstAsync();
            Assert.Equal(firstVehicle.Operator.Name, vehicle.Operator.Name);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Can_manipulate_embedded_collections(bool useIds)
    {
        var options = await Fixture.CreateOptions(seed: false);
        var swappedOptions = await Fixture.CreateOptions(
            modelBuilder => modelBuilder.Entity<Person>(
                eb => eb.OwnsMany(
                    v => v.Addresses, b =>
                    {
                        b.OwnsMany(a => a.Notes).ToJsonProperty("IdNotes");
                        b.OwnsMany(a => a.IdNotes).ToJsonProperty("Notes");
                    })),
            seed: false);

        Address existingAddress1Person2;
        Address existingAddress1Person3;
        Address existingAddress2Person3;
        Address addedAddress1;
        Address addedAddress2;
        Address addedAddress3;
        using (var context = new EmbeddedTransportationContext(options))
        {
            await context.AddAsync(new Person { Id = 1 });
            existingAddress1Person2 = new Address { Street = "Second", City = "Village" };
            if (useIds)
            {
                existingAddress1Person2.IdNotes = new List<NoteWithId>
                {
                    new() { Id = 4, Content = "First note" }, new() { Id = 3, Content = "Second note" }
                };
            }
            else
            {
                existingAddress1Person2.Notes = new List<Note> { new() { Content = "First note" }, new() { Content = "Second note" } };
            }

            var existingAddress2Person2 = new Address { Street = "First", City = "Village" };
            await context.AddAsync(
                new Person { Id = 2, Addresses = new List<Address> { existingAddress1Person2, existingAddress2Person2 } });
            existingAddress1Person3 = new Address
            {
                Street = "First",
                City = "City",
                AddressTitle = new AddressTitle { Title = "P3 Shipping" },
            };
            if (useIds)
            {
                existingAddress1Person3.IdNotes = new List<NoteWithId> { new() { Id = 2, Content = "First City note" } };
            }
            else
            {
                existingAddress1Person3.Notes = new List<Note> { new() { Content = "First City note" } };
            }

            existingAddress2Person3 = new Address
            {
                Street = "Second",
                City = "City",
                AddressTitle = new AddressTitle { Title = "P3 Billing" }
            };

            await context.AddAsync(
                new Person { Id = 3, Addresses = new List<Address> { existingAddress1Person3, existingAddress2Person3 } });

            await context.SaveChangesAsync();

            var people = await context.Set<Person>().ToListAsync();

            Assert.Empty(people[0].Addresses);

            Assert.Equal(2, people[1].Addresses.Count);
            Assert.Same(existingAddress1Person2, people[1].Addresses.First());
            Assert.Same(existingAddress2Person2, people[1].Addresses.Last());

            if (useIds)
            {
                Assert.Equal(2, existingAddress1Person2.IdNotes.Count);
            }
            else
            {
                Assert.Equal(2, existingAddress1Person2.Notes.Count);
            }

            Assert.Same(existingAddress1Person3, people[2].Addresses.First());
            Assert.Same(existingAddress2Person3, people[2].Addresses.Last());

            Assert.Equal(2, people[2].Addresses.Count);
            Assert.Same(existingAddress1Person3, people[2].Addresses.First());
            Assert.Same(existingAddress2Person3, people[2].Addresses.Last());
        }

        using (var context = new EmbeddedTransportationContext(options))
        {
            var people = await context.Set<Person>().ToListAsync();
            addedAddress1 = new Address
            {
                Street = "First",
                City = "Town",
                AddressTitle = new AddressTitle { Title = "P1" }
            };
            people[0].Addresses.Add(addedAddress1);

            addedAddress2 = new Address
            {
                Street = "Another",
                City = "Village",
                AddressTitle = new AddressTitle { Title = "P2" }
            };
            if (useIds)
            {
                addedAddress2.IdNotes = existingAddress1Person2.IdNotes;
                foreach (var note in addedAddress2.IdNotes)
                {
                    note.AddressId = 0;
                }
            }
            else
            {
                addedAddress2.Notes = existingAddress1Person2.Notes;
            }

            people[1].Addresses.Remove(people[1].Addresses.First());
            people[1].Addresses.Add(addedAddress2);

            addedAddress3 = new Address
            {
                Street = "Another",
                City = "City",
                AddressTitle = new AddressTitle { Title = "P3 Alternative" }
            };
            if (useIds)
            {
                addedAddress3.IdNotes = new List<NoteWithId> { new() { Id = -1, Content = "Another note" } };
            }
            else
            {
                addedAddress3.Notes = new List<Note> { new() { Content = "Another note" } };
            }

            var existingFirstAddressEntry = context.Entry(people[2].Addresses.First());

            var addressJson = existingFirstAddressEntry.Property<JObject>("__jObject").CurrentValue;

            Assert.Equal("First", addressJson[nameof(Address.Street)]);
            addressJson["unmappedId"] = 2;

            existingFirstAddressEntry.Property<JObject>("__jObject").IsModified = true;

            existingAddress1Person3 = people[2].Addresses.First();
            existingAddress2Person3 = people[2].Addresses.Last();
            people[2].Addresses.Remove(existingAddress2Person3);
            people[2].Addresses.Add(addedAddress3);
            people[2].Addresses.Add(existingAddress2Person3);

            if (useIds)
            {
                existingAddress1Person3.IdNotes = new List<NoteWithId> { new() { Id = 1, Content = "Some City note" } };
            }
            else
            {
                existingAddress1Person3.Notes = new List<Note> { new() { Content = "Some City note" } };
            }

            if (useIds)
            {
                existingAddress2Person3.IdNotes.Add(new NoteWithId { Id = 4, Content = "City note" });
            }
            else
            {
                existingAddress2Person3.Notes.Add(new Note { Content = "City note" });
            }

            await context.SaveChangesAsync();

            await AssertState(context, useIds);
        }

        using (var context = new EmbeddedTransportationContext(options))
        {
            await AssertState(context, useIds);
        }

        async Task AssertState(EmbeddedTransportationContext context, bool useIds)
        {
            var people = await context.Set<Person>().OrderBy(o => o.Id).ToListAsync();
            var firstAddress = people[0].Addresses.Single();
            Assert.Equal("First", firstAddress.Street);
            Assert.Equal("Town", firstAddress.City);
            Assert.Equal("P1", firstAddress.AddressTitle.Title);
            Assert.Empty(firstAddress.Notes);
            Assert.Empty(firstAddress.IdNotes);

            var addresses = people[1].Addresses.ToList();
            Assert.Equal(2, addresses.Count);

            Assert.Equal("First", addresses[0].Street);
            Assert.Equal("Village", addresses[0].City);
            Assert.Null(addresses[0].AddressTitle);
            Assert.Empty(addresses[0].Notes);
            Assert.Empty(addresses[0].IdNotes);

            Assert.Equal("Another", addresses[1].Street);
            Assert.Equal("Village", addresses[1].City);
            Assert.Equal("P2", addresses[1].AddressTitle.Title);
            if (useIds)
            {
                var notes = addresses[1].IdNotes;
                Assert.Equal(2, notes.Count);
                if (useIds)
                {
                    Assert.Equal(4, notes.First().Id);
                    Assert.Equal(3, notes.Last().Id);
                }
                else
                {
                    Assert.Equal(1, notes.First().Id);
                    Assert.Equal(2, notes.Last().Id);
                }

                Assert.Equal("First note", notes.First().Content);
                Assert.Equal("Second note", notes.Last().Content);
            }
            else
            {
                var notes = addresses[1].Notes;
                Assert.Equal(2, notes.Count);
                Assert.Equal("First note", notes.First().Content);
                Assert.Equal("Second note", notes.Last().Content);
            }

            addresses = people[2].Addresses.ToList();
            Assert.Equal(3, addresses.Count);

            Assert.Equal("First", addresses[0].Street);
            Assert.Equal("City", addresses[0].City);
            Assert.Equal("P3 Shipping", addresses[0].AddressTitle.Title);
            if (useIds)
            {
                Assert.Equal(1, addresses[0].IdNotes.Count);
                Assert.Equal(1, addresses[0].IdNotes.First().Id);
                Assert.Equal("Some City note", addresses[0].IdNotes.First().Content);
            }
            else
            {
                Assert.Equal(1, addresses[0].Notes.Count);
                Assert.Equal("Some City note", addresses[0].Notes.First().Content);
            }

            var existingAddressEntry = context.Entry(addresses[0]);

            var addressJson = existingAddressEntry.Property<JObject>("__jObject").CurrentValue;

            Assert.Equal("First", addressJson[nameof(Address.Street)]);
            Assert.Equal(6, addressJson.Count);
            Assert.Equal(2, addressJson["unmappedId"]);

            Assert.Equal("Another", addresses[1].Street);
            Assert.Equal("City", addresses[1].City);
            Assert.Equal("P3 Alternative", addresses[1].AddressTitle.Title);
            if (useIds)
            {
                Assert.Equal(1, addresses[1].IdNotes.Count);
                Assert.Equal(-1, addresses[1].IdNotes.First().Id);
                Assert.Equal("Another note", addresses[1].IdNotes.First().Content);
            }
            else
            {
                Assert.Equal(1, addresses[1].Notes.Count);
                Assert.Equal("Another note", addresses[1].Notes.First().Content);
            }

            Assert.Equal("Second", addresses[2].Street);
            Assert.Equal("City", addresses[2].City);
            Assert.Equal("P3 Billing", addresses[2].AddressTitle.Title);
            if (useIds)
            {
                Assert.Equal(1, addresses[2].IdNotes.Count);
                Assert.Equal(4, addresses[2].IdNotes.First().Id);
                Assert.Equal("City note", addresses[2].IdNotes.First().Content);
            }
            else
            {
                Assert.Equal(1, addresses[2].Notes.Count);
                Assert.Equal("City note", addresses[2].Notes.First().Content);
            }
        }
    }

    [ConditionalFact]
    public virtual async Task Old_still_works()
    {
        var options = await Fixture.CreateOptions(seed: false);
        var swappedOptions = await Fixture.CreateOptions(
            modelBuilder => modelBuilder.Entity<Person>(
                eb => eb.OwnsMany(
                    v => v.Addresses, b =>
                    {
                        b.OwnsMany(a => a.Notes).ToJsonProperty("IdNotes");
                        b.OwnsMany(a => a.IdNotes).ToJsonProperty("Notes");
                    })),
            seed: false);

        using (var context = new EmbeddedTransportationContext(options))
        {
            await context.AddAsync(
                new Person
                {
                    Id = 1,
                    Addresses = new List<Address>
                    {
                        new()
                        {
                            Street = "Second",
                            City = "Village",
                            Notes = new List<Note> { new() { Content = "First note" } },
                            IdNotes = new List<NoteWithId> { new() { Id = 3, Content = "Second note" } }
                        }
                    }
                });

            await context.SaveChangesAsync();
        }

        using (var context = new EmbeddedTransportationContext(options))
        {
            var people = await context.Set<Person>().ToListAsync();
            var address = people.Single().Addresses.Single();

            Assert.Equal("First note", address.Notes.Single().Content);

            var idNote = address.IdNotes.Single();
            Assert.Equal(3, idNote.Id);
            Assert.Equal("Second note", idNote.Content);

            var noteEntry = context.Entry(idNote);
            var noteJson = noteEntry.Property<JObject>("__jObject").CurrentValue;

            Assert.Equal(3, noteJson[nameof(NoteWithId.Id)]);
            Assert.Null(noteJson[nameof(NoteWithId.AddressId)]);
        }

        using (var context = new EmbeddedTransportationContext(swappedOptions))
        {
            var people = await context.Set<Person>().ToListAsync();
            var address = people.Single().Addresses.Single();

            Assert.Equal("Second note", address.Notes.Single().Content);
            Assert.Equal("First note", address.IdNotes.Single().Content);
        }
    }

    public record struct CosmosPage<T>(List<T> Results, string ContinuationToken);

    [ConditionalFact]
    public virtual async Task Properties_on_owned_types_can_be_client_generated()
    {
        var options = await Fixture.CreateOptions(seed: false);

        using (var context = new EmbeddedTransportationContext(options))
        {
            var address = new Address
            {
                Street = "First",
                City = "City",
                AddressTitle = new AddressTitle()
            };

            await context.AddAsync(new Person { Id = 1, Addresses = new List<Address> { address } });
            Assert.Equal("DefaultTitle", address.AddressTitle.Title);

            await context.SaveChangesAsync();

            var people = await context.Set<Person>().ToListAsync();
            Assert.Same(address, people[0].Addresses.Single());
        }
    }

    [ConditionalFact]
    public virtual async Task Can_use_non_int_keys_for_embedded_entities()
    {
        var options = await Fixture.CreateOptions(
            modelBuilder =>
            {
                modelBuilder.Entity<Person>(
                    eb => eb.OwnsMany(
                        v => v.Addresses, b =>
                        {
                            b.Property<Guid>("Id");
                            b.Ignore(a => a.IdNotes);
                        }));
            },
            seed: false);

        Address address;
        Guid addressGuid;
        await using (var context = new EmbeddedTransportationContext(options))
        {
            await context.Database.EnsureCreatedAsync();
            var person = new Person { Id = 1 };
            address = new Address { Street = "Second", City = "Village" };
            person.Addresses.Add(address);
            await context.AddAsync(person);

            var addressEntry = context.Entry(address);
            addressGuid = (Guid)addressEntry.Property("Id").CurrentValue;

            await context.SaveChangesAsync();
        }

        await using (var context = new EmbeddedTransportationContext(options))
        {
            var people = await context.Set<Person>().OrderBy(o => o.Id).ToListAsync();
            var addresses = people[0].Addresses.ToList();
            Assert.Single(addresses);

            Assert.Equal(address.Street, addresses[0].Street);
            Assert.Equal(address.City, addresses[0].City);

            var addressEntry = context.Entry(addresses[0]);
            Assert.Equal(addressGuid, (Guid)addressEntry.Property("Id").CurrentValue);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_query_and_modify_nested_embedded_types()
    {
        var options = await Fixture.CreateOptions();
        using (var context = new EmbeddedTransportationContext(options))
        {
            var missile = await context.Set<Vehicle>().FirstAsync(v => v.Name == "AIM-9M Sidewinder");

            Assert.Equal("Heat-seeking", missile.Operator.Details.Type);

            missile.Operator.Details.Type = "IR";

            await context.SaveChangesAsync();
        }

        using (var context = new EmbeddedTransportationContext(options))
        {
            var missile = await context.Set<Vehicle>().FirstAsync(v => v.Name == "AIM-9M Sidewinder");

            Assert.Equal("IR", missile.Operator.Details.Type);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_query_just_embedded_reference()
    {
        var options = await Fixture.CreateOptions();
        using var context = new EmbeddedTransportationContext(options);
        var firstOperator = await context.Set<Vehicle>().OrderBy(o => o.Name).Select(v => v.Operator)
            .AsNoTracking().FirstAsync();

        Assert.Equal("Albert Williams", firstOperator.Name);
        Assert.Null(firstOperator.Vehicle);
    }

    [ConditionalFact]
    public virtual async Task Can_query_just_embedded_collection()
    {
        var options = await Fixture.CreateOptions(seed: false);

        using (var context = new EmbeddedTransportationContext(options))
        {
            await context.AddAsync(
                new Person
                {
                    Id = 3,
                    Addresses = new[]
                    {
                        new Address { Street = "First", City = "City" }, new Address { Street = "Second", City = "City" }
                    }
                });

            await context.SaveChangesAsync();
        }

        using (var context = new EmbeddedTransportationContext(options))
        {
            var addresses = await context.Set<Person>().Select(p => p.Addresses).AsNoTracking().FirstAsync();

            Assert.Equal(2, addresses.Count);
        }
    }

    [ConditionalFact]
    public virtual async Task Inserting_dependent_without_principal_throws()
    {
        var options = await Fixture.CreateOptions(seed: false);
        using var context = new EmbeddedTransportationContext(options);
        await context.AddAsync(
            new LicensedOperator
            {
                Name = "Jack Jackson",
                LicenseType = "Class A CDC",
                VehicleName = "Fuel transport"
            });

        Assert.Equal(
            CosmosStrings.OrphanedNestedDocumentSensitive(
                nameof(Operator), nameof(Vehicle), "{VehicleName: Fuel transport}"),
            (await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync())).Message);
    }

    [ConditionalFact]
    public virtual async Task Can_change_nested_instance_non_derived()
    {
        var options = await Fixture.CreateOptions();
        using (var context = new EmbeddedTransportationContext(options))
        {
            var bike = await context.Vehicles.SingleAsync(v => v.Name == "Trek Pro Fit Madone 6 Series");

            bike.Operator = new Operator { Name = "Chris Horner" };

            context.ChangeTracker.DetectChanges();

            bike.Operator = new LicensedOperator { Name = "repairman" };

            TestSqlLoggerFactory.Clear();
            await context.SaveChangesAsync();
        }

        using (var context = new EmbeddedTransportationContext(options))
        {
            var bike = await context.Vehicles.SingleAsync(v => v.Name == "Trek Pro Fit Madone 6 Series");
            Assert.Equal("repairman", bike.Operator.Name);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_change_principal_instance_non_derived()
    {
        var options = await Fixture.CreateOptions();
        using (var context = new EmbeddedTransportationContext(options))
        {
            var bike = await context.Vehicles.SingleAsync(v => v.Name == "Trek Pro Fit Madone 6 Series");

            var newBike = new Vehicle
            {
                Name = "Trek Pro Fit Madone 6 Series",
                Operator = bike.Operator,
                SeatingCapacity = 2
            };

            context.Remove(bike);
            await context.AddAsync(newBike);

            TestSqlLoggerFactory.Clear();
            await context.SaveChangesAsync();
        }

        using (var context = new EmbeddedTransportationContext(options))
        {
            var bike = await context.Vehicles.SingleAsync(v => v.Name == "Trek Pro Fit Madone 6 Series");

            Assert.Equal(2, bike.SeatingCapacity);
            Assert.NotNull(bike.Operator);
        }
    }

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)Fixture.ListLoggerFactory;

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    protected void AssertContainsSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected, assertOrder: false);

    public class CosmosFixture : ServiceProviderFixtureBase, IAsyncLifetime
    {
        public CosmosFixture()
        {
            TestStore = CosmosTestStore.Create(DatabaseName);
        }

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        public virtual CosmosTestStore TestStore { get; }

        public async Task<EmbeddedTransportationContextOptions> CreateOptions(
            Action<ModelBuilder> onModelCreating = null,
            bool seed = true)
        {
            var options = CreateOptions(TestStore);
            var embeddedOptions = new EmbeddedTransportationContextOptions(options, onModelCreating);
            await TestStore.InitializeAsync(
                ServiceProvider, () => new EmbeddedTransportationContext(embeddedOptions), async c =>
                {
                    if (seed)
                    {
                        await ((TransportationContext)c).SeedAsync();
                    }
                });

            ListLoggerFactory.Clear();
            return embeddedOptions;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            => ((EmbeddedTransportationContext)context).Options.OnModelCreating?.Invoke(modelBuilder);

        protected override object GetAdditionalModelCacheKey(DbContext context)
        {
            var options = ((EmbeddedTransportationContext)context).Options;
            return options.OnModelCreating == null
                ? null
                : options;
        }

        public Task InitializeAsync()
            => Task.CompletedTask;

        public Task DisposeAsync()
            => TestStore.DisposeAsync();
    }

    public record class EmbeddedTransportationContextOptions(DbContextOptions Options, Action<ModelBuilder> OnModelCreating);

    protected class EmbeddedTransportationContext(EmbeddedTransportationContextOptions options) : TransportationContext(options.Options)
    {
        public EmbeddedTransportationContextOptions Options { get; } = options;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vehicle>(
                eb =>
                {
                    eb.HasKey(e => e.Name);
                    eb.OwnsOne(v => v.Operator).OwnsOne(v => v.Details);
                });
            modelBuilder.Entity<PoweredVehicle>();

            modelBuilder.Entity<Engine>(
                eb =>
                {
                    eb.HasKey(e => e.VehicleName);
                    eb.HasOne(e => e.Vehicle)
                        .WithOne(e => e.Engine)
                        .HasForeignKey<Engine>(e => e.VehicleName);
                });

            modelBuilder.Entity<FuelTank>(
                eb =>
                {
                    eb.HasKey(e => e.VehicleName);
                    eb.HasOne(e => e.Engine)
                        .WithOne(e => e.FuelTank)
                        .HasForeignKey<FuelTank>(e => e.VehicleName)
                        .OnDelete(DeleteBehavior.Restrict);
                    eb.HasOne(e => e.Vehicle)
                        .WithOne()
                        .HasForeignKey<FuelTank>("VehicleName1");
                });

            modelBuilder.Entity<ContinuousCombustionEngine>();
            modelBuilder.Entity<IntermittentCombustionEngine>();

            modelBuilder.Ignore<SolidFuelTank>();
            modelBuilder.Ignore<SolidRocket>();

            modelBuilder.Entity<PersonBase>();
            modelBuilder.Entity<Person>(
                eb => eb.OwnsMany(
                    v => v.Addresses, b =>
                    {
                        b.ToJsonProperty("Stored Addresses");
                        b.OwnsOne(a => a.AddressTitle).Property(a => a.Title).HasValueGenerator<TitleGenerator>().IsRequired();
                    }));
        }
    }

    private class TitleGenerator : ValueGenerator<string>
    {
        public override bool GeneratesTemporaryValues
            => false;

        public override string Next(EntityEntry entry)
            => "DefaultTitle";
    }

    private abstract class PersonBase
    {
        public int Id { get; set; }
    }

    private class Person : PersonBase
    {
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }

    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public AddressTitle AddressTitle { get; set; }
        public ICollection<Note> Notes { get; set; } = new List<Note>();
        public ICollection<NoteWithId> IdNotes { get; set; } = new List<NoteWithId>();
    }

    public class AddressTitle
    {
        public string Title { get; set; }
    }

    public class Note
    {
        public string Content { get; set; }
    }

    public class NoteWithId
    {
        public int Id { get; set; }
        public int AddressId { get; set; }
        public string Content { get; set; }
    }
}
