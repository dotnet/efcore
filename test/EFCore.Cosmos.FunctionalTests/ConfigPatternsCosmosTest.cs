// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;

// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Microsoft.EntityFrameworkCore.Cosmos;

public class ConfigPatternsCosmosTest : IClassFixture<ConfigPatternsCosmosTest.CosmosFixture>
{
    private const string DatabaseName = "ConfigPatternsCosmos";

    protected CosmosFixture Fixture { get; }

    public ConfigPatternsCosmosTest(CosmosFixture fixture)
    {
        Fixture = fixture;
    }

    [ConditionalFact]
    public async Task Cosmos_client_instance_is_shared_between_contexts()
    {
        await using var testDatabase = CosmosTestStore.CreateInitialized(DatabaseName);
        var options = CreateOptions(testDatabase);

        CosmosClient client;
        using (var context = new CustomerContext(options))
        {
            client = context.Database.GetCosmosClient();
            Assert.NotNull(client);
            Assert.Equal(DatabaseName, context.Database.GetCosmosDatabaseId());
            Assert.True(context.Database.IsCosmos());
        }

        using (var context = new CustomerContext(options))
        {
            Assert.Same(client, context.Database.GetCosmosClient());
        }

        await using var testDatabase2 = CosmosTestStore.CreateInitialized(DatabaseName, o => o.Region(Regions.AustraliaCentral));
        options = CreateOptions(testDatabase2);

        using (var context = new CustomerContext(options))
        {
            Assert.NotSame(client, context.Database.GetCosmosClient());
        }
    }

    [ConditionalFact]
    public async Task Should_not_throw_if_specified_region_is_right()
    {
        var regionName = Regions.AustraliaCentral;

        await using var testDatabase = CosmosTestStore.CreateInitialized(DatabaseName, o => o.Region(regionName));
        var options = CreateOptions(testDatabase);

        var customer = new Customer { Id = 42, Name = "Theon" };

        using var context = new CustomerContext(options);
        context.Database.EnsureCreated();

        context.Add(customer);

        context.SaveChanges();
    }

    [ConditionalFact]
    public async Task Should_throw_if_specified_region_is_wrong()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () =>
            {
                await using var testDatabase = CosmosTestStore.CreateInitialized(DatabaseName, o => o.Region("FakeRegion"));
                var options = CreateOptions(testDatabase);

                var customer = new Customer { Id = 42, Name = "Theon" };

                using var context = new CustomerContext(options);
                context.Database.EnsureCreated();

                context.Add(customer);

                context.SaveChanges();
            });

        Assert.Equal("ApplicationRegion configuration 'FakeRegion' is not a valid Azure region or the current SDK version does not recognize it. If the value represents a valid region, make sure you are using the latest SDK version.", exception.Message);
    }

    [ConditionalFact]
    public async Task Should_not_throw_if_specified_connection_mode_is_right()
    {
        var connectionMode = ConnectionMode.Direct;

        await using var testDatabase = CosmosTestStore.CreateInitialized(DatabaseName, o => o.ConnectionMode(connectionMode));
        var options = CreateOptions(testDatabase);

        var customer = new Customer { Id = 42, Name = "Theon" };

        using var context = new CustomerContext(options);
        context.Database.EnsureCreated();

        context.Add(customer);

        context.SaveChanges();
    }

    [ConditionalFact]
    public async Task Should_throw_if_specified_connection_mode_is_wrong()
    {
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () =>
            {
                await using var testDatabase = CosmosTestStore.CreateInitialized(
                    DatabaseName, o => o.ConnectionMode((ConnectionMode)123456));
                var options = CreateOptions(testDatabase);

                var customer = new Customer { Id = 42, Name = "Theon" };

                using var context = new CustomerContext(options);
                context.Database.EnsureCreated();

                context.Add(customer);

                context.SaveChanges();
            });
    }

    private DbContextOptions CreateOptions(CosmosTestStore testDatabase, Action<DbContextOptionsBuilder> configure = null)
    {
        var builder = Fixture.AddOptions(testDatabase.AddProviderOptions(new DbContextOptionsBuilder()))
            .EnableDetailedErrors();
        configure?.Invoke(builder);
        return builder.Options;
    }

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
            => modelBuilder.Entity<Customer>();
    }

    public class CosmosFixture : ServiceProviderFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;
    }
}
