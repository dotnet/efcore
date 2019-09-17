// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.TestUtilities;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Cosmos
{
    public class EmbeddedDocumentsTest
    {
        public EmbeddedDocumentsTest(ITestOutputHelper testOutputHelper)
        {
            TestSqlLoggerFactory = (TestSqlLoggerFactory)TestStoreFactory.CreateListLoggerFactory(_ => true);
            //TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalFact(Skip = "Issue #17670")]
        public virtual async Task Can_update_dependents()
        {
            await using (var testDatabase = CreateTestStore())
            {
                Operator firstOperator;
                Engine firstEngine;
                using (var context = CreateContext())
                {
                    firstOperator = context.Set<Vehicle>().Select(v => v.Operator).OrderBy(o => o.VehicleName).First();
                    firstOperator.Name += "1";
                    firstEngine = context.Set<PoweredVehicle>().Select(v => v.Engine).OrderBy(o => o.VehicleName).First();
                    firstEngine.Description += "1";

                    context.SaveChanges();
                }

                using (var context = CreateContext())
                {
                    Assert.Equal(
                        firstOperator.Name,
                        context.Set<Vehicle>().Select(v => v.Operator).OrderBy(o => o.VehicleName).First().Name);
                    Assert.Equal(
                        firstEngine.Description,
                        context.Set<PoweredVehicle>().Select(v => v.Engine).OrderBy(o => o.VehicleName).First().Description);
                }
            }
        }

        [ConditionalFact]
        public virtual async Task Can_update_owner_with_dependents()
        {
            await using (var testDatabase = CreateTestStore())
            {
                Operator firstOperator;
                using (var context = CreateContext())
                {
                    firstOperator = context.Set<Vehicle>().OrderBy(o => o.Name).First().Operator;
                    firstOperator.Name += "1";

                    context.SaveChanges();
                }

                using (var context = CreateContext())
                {
                    Assert.Equal(
                        firstOperator.Name,
                        context.Set<Vehicle>().OrderBy(o => o.Name).First().Operator.Name);
                }
            }
        }

        [ConditionalFact]
        public virtual async Task Can_attach_owner_with_dependents()
        {
            await using (var testDatabase = CreateTestStore())
            {
                Vehicle firstVehicle;

                using (var context = CreateContext())
                {
                    firstVehicle = context.Set<Vehicle>().OrderBy(o => o.Name).First();

                    context.SaveChanges();
                }

                using (var context = CreateContext())
                {
                    //Issue #15289
                    var firstVehicleEntry = context.Add(firstVehicle);
                    firstVehicleEntry.State = EntityState.Unchanged;
                    firstVehicle.Operator.Name += "1";

                    context.SaveChanges();
                }

                using (var context = CreateContext())
                {
                    Assert.Equal(
                        firstVehicle.Operator.Name,
                        context.Set<Vehicle>().OrderBy(o => o.Name).First().Operator.Name);
                }
            }
        }

        [ConditionalFact]
        public virtual async Task Can_add_collection_dependent_to_owner()
        {
            await using (var testDatabase = CreateTestStore(seed: false))
            {
                Address existingAddress1Person2;
                Address existingAddress1Person3;
                Address existingAddress2Person3;
                Address addedAddress1;
                Address addedAddress2;
                Address addedAddress3;
                using (var context = CreateContext())
                {
                    context.Add(new Person { Id = 1 });
                    existingAddress1Person2 = new Address { Street = "Second", City = "Village" };
                    context.Add(new Person { Id = 2, Addresses = new[] { existingAddress1Person2 } });
                    existingAddress1Person3 = new Address { Street = "First", City = "City" };
                    existingAddress2Person3 = new Address { Street = "Second", City = "City" };
                    context.Add(new Person { Id = 3, Addresses = new[] { existingAddress1Person3, existingAddress2Person3 } });

                    context.SaveChanges();
                }

                using (var context = CreateContext())
                {
                    var people = context.Set<Person>().ToList();
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

                    context.SaveChanges();
                }

                using (var context = CreateContext())
                {
                    var people = context.Set<Person>().OrderBy(o => o.Id).ToList();
                    var addresses = people[0].Addresses.ToList();
                    Assert.Equal(addedAddress1.Street, addresses.Single().Street);
                    Assert.Equal(addedAddress1.City, addresses.Single().City);

                    addresses = people[1].Addresses.ToList();
                    Assert.Equal(2, addresses.Count);

                    Assert.Equal(existingAddress1Person2.Street, addresses[0].Street);
                    Assert.Equal(existingAddress1Person2.City, addresses[0].City);

                    Assert.Equal(addedAddress2.Street, addresses[1].Street);
                    Assert.Equal(addedAddress2.City, addresses[1].City);

                    var existingAddressEntry = context.Entry(people[1].Addresses.First());

                    var addressJson = existingAddressEntry.Property<JObject>("__jObject").CurrentValue;

                    Assert.Equal("Second", addressJson[nameof(Address.Street)]);
                    Assert.Equal(3, addressJson.Count);
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
        }

        [ConditionalFact]
        public virtual async Task Can_use_non_int_keys_for_embedded_entities()
        {
            await using (var testDatabase = CreateTestStore(
                modelBuilder =>
                {
                    modelBuilder.Entity<Person>(
                        eb => eb.OwnsMany(
                            v => v.Addresses, b =>
                            {
                                b.Property<Guid>("Id");
                            }));
                }, seed: false))
            {
                Address address;
                Guid addressGuid;
                await using (var context = CreateContext())
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

                await using (var context = CreateContext())
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
        }

        [ConditionalFact(Skip = "Issue #17733")]
        public virtual async Task Can_query_nested_embedded_types()
        {
            await using (var testDatabase = CreateTestStore())
            {
                using (var context = CreateContext())
                {
                    var missile = context.Set<Vehicle>().First(v => v.Name == "AIM-9M Sidewinder");

                    Assert.Equal("Heat-seeking", missile.Operator.Details.Type);
                }
            }
        }

        [ConditionalFact]
        public virtual async Task Can_query_just_nested_reference()
        {
            await using (var testDatabase = CreateTestStore())
            {
                using (var context = CreateContext())
                {
                    var firstOperator = context.Set<Vehicle>().OrderBy(o => o.Name).Select(v => v.Operator)
                        .AsNoTracking().First();

                    Assert.Equal("Albert Williams", firstOperator.Name);
                    Assert.Null(firstOperator.Vehicle);
                }
            }
        }

        [ConditionalFact]
        public virtual async Task Can_query_just_nested_collection()
        {
            await using (var testDatabase = CreateTestStore(seed: false))
            {
                using (var context = CreateContext())
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

                    context.SaveChanges();
                }

                using (var context = CreateContext())
                {
                    var addresses = context.Set<Person>().Select(p => p.Addresses).AsNoTracking().First();

                    Assert.Equal(2, addresses.Count);
                }
            }
        }

        [ConditionalFact]
        public virtual async Task Inserting_dependent_without_principal_throws()
        {
            await using (var testDatabase = CreateTestStore(seed: false))
            {
                using (var context = CreateContext())
                {
                    context.Add(
                        new LicensedOperator { Name = "Jack Jackson", LicenseType = "Class A CDC", VehicleName = "Fuel transport" });

                    Assert.Equal(
                        CosmosStrings.OrphanedNestedDocumentSensitive(
                            nameof(Operator), nameof(Vehicle), "{VehicleName: Fuel transport}"),
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                }
            }
        }

        [ConditionalFact]
        public virtual async Task Can_change_nested_instance_non_derived()
        {
            await using (var testDatabase = CreateTestStore())
            {
                using (var context = CreateContext())
                {
                    var bike = context.Vehicles.Single(v => v.Name == "Trek Pro Fit Madone 6 Series");

                    bike.Operator = new Operator { Name = "Chris Horner" };

                    context.ChangeTracker.DetectChanges();

                    bike.Operator = new LicensedOperator { Name = "repairman" };

                    TestSqlLoggerFactory.Clear();
                    context.SaveChanges();
                }

                using (var context = CreateContext())
                {
                    var bike = context.Vehicles.Single(v => v.Name == "Trek Pro Fit Madone 6 Series");
                    Assert.Equal("repairman", bike.Operator.Name);
                }
            }
        }

        [ConditionalFact]
        public virtual async Task Can_change_principal_instance_non_derived()
        {
            await using (var testDatabase = CreateTestStore())
            {
                using (var context = CreateContext())
                {
                    var bike = context.Vehicles.Single(v => v.Name == "Trek Pro Fit Madone 6 Series");

                    var newBike = new Vehicle { Name = "Trek Pro Fit Madone 6 Series", Operator = bike.Operator, SeatingCapacity = 2 };

                    var oldEntry = context.Remove(bike);
                    var newEntry = context.Add(newBike);

                    TestSqlLoggerFactory.Clear();
                    context.SaveChanges();
                }

                using (var context = CreateContext())
                {
                    var bike = context.Vehicles.Single(v => v.Name == "Trek Pro Fit Madone 6 Series");

                    Assert.Equal(2, bike.SeatingCapacity);
                    Assert.NotNull(bike.Operator);
                }
            }
        }

        protected readonly string DatabaseName = "NestedDocumentsTest";
        protected TestStore TestStore { get; set; }
        protected ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
        protected IServiceProvider ServiceProvider { get; set; }
        protected TestSqlLoggerFactory TestSqlLoggerFactory { get; }

        protected void AssertSql(params string[] expected)
            => TestSqlLoggerFactory.AssertBaseline(expected);

        protected void AssertContainsSql(params string[] expected)
            => TestSqlLoggerFactory.AssertBaseline(expected, assertOrder: false);

        protected TestStore CreateTestStore(Action<ModelBuilder> onModelCreating = null, bool seed = true)
        {
            TestStore = TestStoreFactory.Create(DatabaseName);

            ServiceProvider = TestStoreFactory.AddProviderServices(new ServiceCollection())
                .AddSingleton(TestModelSource.GetFactory(onModelCreating ?? (_ => { })))
                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                .BuildServiceProvider(validateScopes: true);


            TestStore.Initialize(ServiceProvider, CreateContext, c =>
            {
                if (seed)
                {
                    ((TransportationContext)c).Seed();
                }
            });

            TestSqlLoggerFactory.Clear();

            return TestStore;
        }

        protected virtual DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => builder
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(
                    b => b.Default(WarningBehavior.Throw)
                        .Log(CoreEventId.SensitiveDataLoggingEnabledWarning)
                        .Log(CoreEventId.PossibleUnintendedReferenceComparisonWarning));

        protected virtual EmbeddedTransportationContext CreateContext()
        {
            var options = AddOptions(TestStore.AddProviderOptions(new DbContextOptionsBuilder()))
                .UseInternalServiceProvider(ServiceProvider).Options;
            return new EmbeddedTransportationContext(options);
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

                modelBuilder.Entity<Person>(
                    eb => eb.OwnsMany(
                        v => v.Addresses, b =>
                        {
                            b.ToJsonProperty("Stored Addresses");
                        }));
            }
        }

        private class Person
        {
            public int Id { get; set; }
            public ICollection<Address> Addresses { get; set; } = new HashSet<Address>();
        }

        public class Address
        {
            public string Street { get; set; }
            public string City { get; set; }
        }
    }
}
