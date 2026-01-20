// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Extensions;

public class CosmosDatabaseFacadeExtensionsDeserializeTest(CosmosDatabaseFacadeExtensionsDeserializeTest.CosmosFixture fixture) : IClassFixture<CosmosDatabaseFacadeExtensionsDeserializeTest.CosmosFixture>
{
    protected CosmosFixture Fixture { get; } = fixture;

    [ConditionalFact]
    public virtual async Task Deserializes()
    {
        using var context = Fixture.CreateContext();

        var customer = new Customer { Id = "0", Name = "Customer 1", PartitionKey = "1" };
        customer.Children.Add(new DummyChild { Id = "1", Name = "Child 1" });
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var client = context.Database.GetCosmosClient();
        var container = client.GetContainer(
            context.Database.GetCosmosDatabaseId(),
            context.Model.GetEntityTypes().First(x => x.IsDocumentRoot()).GetContainer());

        var response = await container.ReadItemAsync<JObject>("0", new Microsoft.Azure.Cosmos.PartitionKey("1"));
        var jObject = response.Resource;

        var deserialized = context.Database.Deserialize(jObject);
        var deserializedCustomer = Assert.IsType<Customer>(deserialized);

        Assert.Equal("Customer 1", deserializedCustomer.Name);
        Assert.Equal("0", deserializedCustomer.Id);
    }

    [ConditionalFact]
    public virtual async Task Deserializes_IdDiscriminator()
    {
        using var context = Fixture.CreateContext();

        var customer = new CustomerWithIdDiscriminator { Id = "1", Name = "Customer 1", PartitionKey = "1" };
        context.Set<CustomerWithIdDiscriminator>().Add(customer);
        await context.SaveChangesAsync();

        var client = context.Database.GetCosmosClient();
        var container = client.GetContainer(
            context.Database.GetCosmosDatabaseId(),
            context.Model.GetEntityTypes().First(x => x.IsDocumentRoot()).GetContainer());

        var response = await container.ReadItemAsync<JObject>("CustomerWithIdDiscriminator|1", new Microsoft.Azure.Cosmos.PartitionKey("1"));
        var jObject = response.Resource;

        var deserialized = context.Database.Deserialize(jObject);
        var deserializedCustomer = Assert.IsType<CustomerWithIdDiscriminator>(deserialized);

        Assert.Equal("Customer 1", deserializedCustomer.Name);
        Assert.Equal("1", deserializedCustomer.Id);
    }

    [ConditionalFact]
    public virtual async Task NonRootType_Fails()
    {
        using var context = Fixture.CreateContext();

        var customer = new Customer { Id = "2", Name = "Customer 2", PartitionKey = "2" };
        customer.Children.Add(new DummyChild { Id = "2", Name = "Child 2" });
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var client = context.Database.GetCosmosClient();
        var container = client.GetContainer(
            context.Database.GetCosmosDatabaseId(),
            context.Model.GetEntityTypes().First(x => x.IsDocumentRoot()).GetContainer());

        var response = await container.ReadItemAsync<JObject>("2", new Microsoft.Azure.Cosmos.PartitionKey("2"));
        var jObject = response.Resource;

        jObject["$type"] = "DummyChild";

        var exception = Assert.Throws<InvalidOperationException>(
            () => context.Database.Deserialize(jObject));
        Assert.Equal("Unable to determine entity type.", exception.Message);
    }

    [ConditionalFact]
    public virtual async Task Mismatch_discriminator_fails()
    {
        using var context = Fixture.CreateContext();

        var customer = new Customer { Id = "3", Name = "Customer 3", PartitionKey = "3" };
        customer.Children.Add(new DummyChild { Id = "3", Name = "Child 3" });
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var client = context.Database.GetCosmosClient();
        var container = client.GetContainer(
            context.Database.GetCosmosDatabaseId(),
            context.Model.GetEntityTypes().First(x => x.IsDocumentRoot()).GetContainer());

        var response = await container.ReadItemAsync<JObject>("3", new Microsoft.Azure.Cosmos.PartitionKey("3"));
        var jObject = response.Resource;

        jObject["$type"] = "NotACustomer";

        var exception = Assert.Throws<InvalidOperationException>(
            () => context.Database.Deserialize<Customer>(jObject));
        Assert.Equal(CoreStrings.UnableToDiscriminate(nameof(Customer), "NotACustomer"), exception.Message);
    }

    [ConditionalFact]
    public virtual async Task No_discriminator_fails()
    {
        using var context = Fixture.CreateContext();

        var customer = new Customer { Id = "5", Name = "Customer 3", PartitionKey = "3" };
        customer.Children.Add(new DummyChild { Id = "3", Name = "Child 3" });
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var client = context.Database.GetCosmosClient();
        var container = client.GetContainer(
            context.Database.GetCosmosDatabaseId(),
            context.Model.GetEntityTypes().First(x => x.IsDocumentRoot()).GetContainer());

        var response = await container.ReadItemAsync<JObject>("5", new Microsoft.Azure.Cosmos.PartitionKey("3"));
        var jObject = response.Resource;

        jObject.Remove("$type");

        var exception = Assert.Throws<InvalidOperationException>(
            () => context.Database.Deserialize<Customer>(jObject));
        Assert.Equal(CoreStrings.UnableToDiscriminate(nameof(Customer), ""), exception.Message);
    }

    [ConditionalFact]
    public virtual async Task Changefeed_deserialize()
    {
        using var context = Fixture.CreateContext();

        var client = context.Database.GetCosmosClient();
        var container = client.GetContainer(
            context.Database.GetCosmosDatabaseId(),
            context.Model.GetEntityTypes().First(x => x.IsDocumentRoot()).GetContainer());

        object? deserialized = null;
        var processor = container.GetChangeFeedProcessorBuilder("processor", async (IReadOnlyCollection<JObject> changes, CancellationToken ctx) =>
        {
            using var context = Fixture.CreateContext();
            deserialized = context.Database.Deserialize(changes.Single());
        }).WithInMemoryLeaseContainer().Build();

        await processor.StartAsync();
        try
        {
            var customer = new Customer { Id = "4", Name = "Customer 1", PartitionKey = "1" };
            customer.Children.Add(new DummyChild { Id = "1", Name = "Child 1" });
            context.Customers.Add(customer);

            await context.SaveChangesAsync();

            var counter = 0;
            while (deserialized == null && counter++ < 30)
            {
                await Task.Delay(1000);
            }
            Assert.NotNull(deserialized);

            var deserializedCustomer = Assert.IsType<Customer>(deserialized!);

            Assert.Equal("Customer 1", deserializedCustomer.Name);
            Assert.Equal("4", deserializedCustomer.Id);
        }
        finally
        {
            await processor.StopAsync();
        }
    }

    public class CosmosFixture : SharedStoreFixtureBase<CosmosDeserializeContext>
    {
        protected override string StoreName
            => nameof(CosmosDatabaseFacadeExtensionsDeserializeTest);

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;
    }

    public class CosmosDeserializeContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<Customer> Customers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Customer>(
                b =>
                {
                    b.HasKey(c => c.Id);
                    b.Property(c => c.Name).ToJsonProperty("custom");
                    b.OwnsMany(x => x.Children);
                    b.HasPartitionKey(c => c.PartitionKey);
                });

            builder.Entity<CustomerWithIdDiscriminator>(
                b =>
                {
                    b.HasRootDiscriminatorInJsonId();
                    b.HasKey(c => c.Id);
                    b.Property(c => c.Name).ToJsonProperty("custom");
                    b.HasPartitionKey(c => c.PartitionKey);
                });
        }
    }

    public class Customer
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? PartitionKey { get; set; }

        public ICollection<DummyChild> Children { get; } = new HashSet<DummyChild>();
    }

    public class DummyChild
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
    }

    public class CustomerWithIdDiscriminator
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? PartitionKey { get; set; }
    }
}
