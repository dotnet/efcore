// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.TestUtilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Cosmos
{
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
            var options = Fixture.CreateOptions();
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
            var options = Fixture.CreateOptions();
            Operator firstOperator;
            using (var context = new EmbeddedTransportationContext(options))
            {
                firstOperator = context.Set<Vehicle>().OrderBy(o => o.Name).First().Operator;
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
            var options = Fixture.CreateOptions();
            Vehicle firstVehicle;
            using (var context = new EmbeddedTransportationContext(options))
            {
                firstVehicle = await context.Set<Vehicle>().OrderBy(o => o.Name).FirstAsync();

                await context.SaveChangesAsync();
            }

            using (var context = new EmbeddedTransportationContext(options))
            {
                //Issue #15289
                var firstVehicleEntry = context.Add(firstVehicle);
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

        [ConditionalFact]
        public virtual async Task Can_add_collection_dependent_to_owner()
        {
            var options = Fixture.CreateOptions(seed: false);

            Address existingAddress1Person2;
            Address existingAddress1Person3;
            Address existingAddress2Person3;
            Address addedAddress1;
            Address addedAddress2;
            Address addedAddress3;
            using (var context = new EmbeddedTransportationContext(options))
            {
                context.Add(new Person { Id = 1 });
                existingAddress1Person2 = new Address
                {
                    Street = "Second",
                    City = "Village",
                    Notes = new[] { new Note { Content = "First note" }, new Note { Content = "Second note" } }
                };
                context.Add(new Person { Id = 2, Addresses = new[] { existingAddress1Person2 } });
                existingAddress1Person3 = new Address { Street = "First", City = "City" };
                existingAddress2Person3 = new Address { Street = "Second", City = "City" };
                context.Add(new Person { Id = 3, Addresses = new[] { existingAddress1Person3, existingAddress2Person3 } });

                await context.SaveChangesAsync();
            }

            using (var context = new EmbeddedTransportationContext(options))
            {
                var people = await context.Set<Person>().ToListAsync();
                addedAddress1 = new Address { Street = "First", City = "Town" };
                people[0].Addresses.Add(addedAddress1);

                addedAddress2 = new Address { Street = "Another", City = "Village" };
                people[1].Addresses.Add(addedAddress2);

                var existingAddressEntry = context.Entry(people[1].Addresses.First());

                var addressJson = existingAddressEntry.Property<JObject>("__jObject").CurrentValue;

                Assert.Equal("Second", addressJson[nameof(Address.Street)]);
                addressJson["unmappedId"] = 2;

                existingAddressEntry.Property<JObject>("__jObject").IsModified = true;

                addedAddress3 = new Address { Street = "Another", City = "City" };
                var existingLastAddress = people[2].Addresses.Last();
                people[2].Addresses.Remove(existingLastAddress);
                people[2].Addresses.Add(addedAddress3);
                people[2].Addresses.Add(existingLastAddress);

                await context.SaveChangesAsync();
            }

            using (var context = new EmbeddedTransportationContext(options))
            {
                var people = await context.Set<Person>().OrderBy(o => o.Id).ToListAsync();
                var addresses = people[0].Addresses.ToList();
                Assert.Equal(addedAddress1.Street, addresses.Single().Street);
                Assert.Equal(addedAddress1.City, addresses.Single().City);
                Assert.Equal(addedAddress1.Notes, addresses.Single().Notes);

                addresses = people[1].Addresses.ToList();
                Assert.Equal(2, addresses.Count);

                Assert.Equal(existingAddress1Person2.Street, addresses[0].Street);
                Assert.Equal(existingAddress1Person2.City, addresses[0].City);
                Assert.Equal(
                    existingAddress1Person2.Notes.OrderBy(n => n.Content).Select(n => n.Content),
                    addresses[0].Notes.OrderBy(n => n.Content).Select(n => n.Content));

                Assert.Equal(addedAddress2.Street, addresses[1].Street);
                Assert.Equal(addedAddress2.City, addresses[1].City);

                var existingAddressEntry = context.Entry(people[1].Addresses.First());

                var addressJson = existingAddressEntry.Property<JObject>("__jObject").CurrentValue;

                Assert.Equal("Second", addressJson[nameof(Address.Street)]);
                Assert.Equal(4, addressJson.Count);
                Assert.Equal(2, addressJson["unmappedId"]);

                addresses = people[2].Addresses.ToList();
                Assert.Equal(3, addresses.Count);

                Assert.Equal(existingAddress1Person3.Street, addresses[0].Street);
                Assert.Equal(existingAddress1Person3.City, addresses[0].City);

                Assert.Equal(addedAddress3.Street, addresses[1].Street);
                Assert.Equal(addedAddress3.City, addresses[1].City);

                Assert.Equal(existingAddress2Person3.Street, addresses[2].Street);
                Assert.Equal(existingAddress2Person3.City, addresses[2].City);
            }
        }

        [ConditionalFact]
        public virtual async Task Can_use_non_int_keys_for_embedded_entities()
        {
            var options = Fixture.CreateOptions(
                modelBuilder =>
                {
                    modelBuilder.Entity<Person>(
                        eb => eb.OwnsMany(
                            v => v.Addresses, b =>
                            {
                                b.Property<Guid>("Id");
                            }));
                },
                additionalModelCacheKey: "Guid_key",
                seed: false);

            Address address;
            Guid addressGuid;
            await using (var context = new EmbeddedTransportationContext(options))
            {
                await context.Database.EnsureCreatedAsync();
                var person = new Person { Id = 1 };
                address = new Address { Street = "Second", City = "Village" };
                person.Addresses.Add(address);
                context.Add(person);

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
            var options = Fixture.CreateOptions();
            using (var context = new EmbeddedTransportationContext(options))
            {
                var missile = context.Set<Vehicle>().First(v => v.Name == "AIM-9M Sidewinder");

                Assert.Equal("Heat-seeking", missile.Operator.Details.Type);

                missile.Operator.Details.Type = "IR";

                await context.SaveChangesAsync();
            }

            using (var context = new EmbeddedTransportationContext(options))
            {
                var missile = context.Set<Vehicle>().First(v => v.Name == "AIM-9M Sidewinder");

                Assert.Equal("IR", missile.Operator.Details.Type);
            }
        }

        [ConditionalFact]
        public virtual async Task Can_query_just_embedded_reference()
        {
            var options = Fixture.CreateOptions();
            using (var context = new EmbeddedTransportationContext(options))
            {
                var firstOperator = await context.Set<Vehicle>().OrderBy(o => o.Name).Select(v => v.Operator)
                    .AsNoTracking().FirstAsync();

                Assert.Equal("Albert Williams", firstOperator.Name);
                Assert.Null(firstOperator.Vehicle);
            }
        }

        [ConditionalFact]
        public virtual async Task Can_query_just_embedded_collection()
        {
            var options = Fixture.CreateOptions(seed: false);

            using (var context = new EmbeddedTransportationContext(options))
            {
                context.Add(
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
            var options = Fixture.CreateOptions(seed: false);
            using (var context = new EmbeddedTransportationContext(options))
            {
                context.Add(
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
        }

        [ConditionalFact]
        public virtual async Task Can_change_nested_instance_non_derived()
        {
            var options = Fixture.CreateOptions();
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
            var options = Fixture.CreateOptions();
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
                context.Add(newBike);

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

        protected TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)Fixture.ListLoggerFactory;

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

            protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
            public virtual CosmosTestStore TestStore { get; }
            private Action<ModelBuilder> OnModelCreatingAction { get; set; }
            private object AdditionalModelCacheKey { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                OnModelCreatingAction?.Invoke(modelBuilder);
            }

            public DbContextOptions CreateOptions(
                Action<ModelBuilder> onModelCreating = null, object additionalModelCacheKey = null, bool seed = true)
            {
                OnModelCreatingAction = onModelCreating;
                AdditionalModelCacheKey = additionalModelCacheKey;
                var options = CreateOptions(TestStore);
                TestStore.Initialize(
                    ServiceProvider, () => new EmbeddedTransportationContext(options), c =>
                    {
                        if (seed)
                        {
                            ((TransportationContext)c).Seed();
                        }
                    });

                ListLoggerFactory.Clear();
                return options;
            }

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection)
                    .AddSingleton<IModelCacheKeyFactory>(new TestModelCacheKeyFactory(() => AdditionalModelCacheKey));

            public Task InitializeAsync() => Task.CompletedTask;

            public Task DisposeAsync() => TestStore.DisposeAsync();

            private class TestModelCacheKeyFactory : IModelCacheKeyFactory
            {
                private readonly Func<object> _getAdditionalKey;

                public TestModelCacheKeyFactory(Func<object> getAdditionalKey)
                {
                    _getAdditionalKey = getAdditionalKey;
                }

                public object Create(DbContext context) => Tuple.Create(context.GetType(), _getAdditionalKey());
            }
        }

        protected class EmbeddedTransportationContext : TransportationContext
        {
            public EmbeddedTransportationContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Vehicle>(
                    eb =>
                    {
                        eb.HasKey(e => e.Name);
                        eb.OwnsOne(v => v.Operator).OwnsOne(v => v.Details);
                    });

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
                            b.OwnsMany(a => a.Notes);
                        }));
            }
        }

        private abstract class PersonBase
        {
            public int Id { get; set; }
        }

        private class Person : PersonBase
        {
            public ICollection<Address> Addresses { get; set; } = new HashSet<Address>();
        }

        public class Address
        {
            public string Street { get; set; }
            public string City { get; set; }
            public ICollection<Note> Notes { get; set; } = new HashSet<Note>();
        }

        public class Note
        {
            public string Content { get; set; }
        }
    }
}
