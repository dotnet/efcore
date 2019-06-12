using System;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Microsoft.EntityFrameworkCore.Cosmos
{
    public class ConfigPatternsCosmosTest : IClassFixture<ConfigPatternsCosmosTest.CosmosFixture>
    {
        private const string DatabaseName = "ConfigPatternsCosmos";

        protected CosmosFixture Fixture { get; }

        public ConfigPatternsCosmosTest(CosmosFixture fixture)
        {
            Fixture = fixture;
        }

        [ConditionalFact]
        public void Cosmos_client_instance_is_shared_between_contexts()
        {
            CosmosClient client;
            using (var testDatabase = CosmosTestStore.CreateInitialized(DatabaseName))
            {
                var options = CreateOptions(testDatabase);

                using (var context = new CustomerContext(options))
                {
                    client = context.Database.GetCosmosClient();
                    Assert.NotNull(client);
                }

                using (var context = new CustomerContext(options))
                {
                    Assert.Same(client, context.Database.GetCosmosClient());
                }
            }

            using (var testDatabase = CosmosTestStore.CreateInitialized(DatabaseName, o => o.Region(CosmosRegions.AustraliaCentral)))
            {
                var options = CreateOptions(testDatabase);

                using (var context = new CustomerContext(options))
                {
                    Assert.NotSame(client, context.Database.GetCosmosClient());
                }
            }
        }

        [ConditionalFact]
        public void Should_not_throw_if_specified_region_is_right()
        {
            var regionName = CosmosRegions.AustraliaCentral;

            using (var testDatabase = CosmosTestStore.CreateInitialized(DatabaseName, o => o.Region(regionName)))
            {
                var options = CreateOptions(testDatabase);

                var customer = new Customer { Id = 42, Name = "Theon" };

                using (var context = new CustomerContext(options))
                {
                    context.Database.EnsureCreated();

                    context.Add(customer);

                    context.SaveChanges();
                }
            }
        }

        [ConditionalFact]
        public void Should_throw_if_specified_region_is_wrong()
        {
            var regionName = "FakeRegion";

            Action a = () =>
            {
                using (var testDatabase = CosmosTestStore.CreateInitialized(DatabaseName, o => o.Region(regionName)))
                {
                    var options = CreateOptions(testDatabase);

                    var customer = new Customer { Id = 42, Name = "Theon" };

                    using (var context = new CustomerContext(options))
                    {
                        context.Database.EnsureCreated();

                        context.Add(customer);

                        context.SaveChanges();
                    }
                }
            };

            var ex = Assert.Throws<ArgumentException>(a);
            Assert.Equal("Current location is not a valid Azure region.", ex.Message);
        }

        private DbContextOptions CreateOptions(CosmosTestStore testDatabase)
            => Fixture.AddOptions(testDatabase.AddProviderOptions(new DbContextOptionsBuilder()))
                .EnableDetailedErrors()
                .Options;

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class CustomerContext : DbContext
        {
            public CustomerContext(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>();
            }
        }

        public class CosmosFixture : ServiceProviderFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
        }
    }
}
