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

        var customer = new Customer { Id = Guid.NewGuid().ToString(), Name = "Customer 1", PartitionKey = "1" };
        customer.Children.Add(new DummyChild { Id = "1", Name = "Child 1" });
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var client = context.Database.GetCosmosClient();
        var container = client.GetContainer(
            context.Database.GetCosmosDatabaseId(),
            context.Model.GetEntityTypes().First(x => x.IsDocumentRoot()).GetContainer());

        var response = await container.ReadItemAsync<JObject>(customer.Id, new Microsoft.Azure.Cosmos.PartitionKey(customer.PartitionKey));
        var jObject = response.Resource;

        var deserialized = context.Database.Deserialize<Customer>(jObject);

        Assert.Equal(customer.Name, deserialized.Name);
        Assert.Equal(customer.Id, deserialized.Id);
    }

    [ConditionalFact]
    public virtual async Task Deserializes_SharedClrType()
    {
        using var context = Fixture.CreateContext();

        var customer = new CustomerWithSharedClrType { Id = "1", Name = "Customer 1", PartitionKey = "1" };
        context.Set<CustomerWithSharedClrType>("1").Add(customer);
        await context.SaveChangesAsync();

        var client = context.Database.GetCosmosClient();
        var container = client.GetContainer(
            context.Database.GetCosmosDatabaseId(),
            context.Model.GetEntityTypes().First(x => x.IsDocumentRoot()).GetContainer());

        var response = await container.ReadItemAsync<JObject>(customer.Id, new Microsoft.Azure.Cosmos.PartitionKey(customer.PartitionKey));
        var jObject = response.Resource;

        var deserialized = context.Database.Deserialize<CustomerWithSharedClrType>("1", jObject);

        Assert.Equal(customer.Name, deserialized.Name);
        Assert.Equal(customer.Id, deserialized.Id);
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

        var response = await container.ReadItemAsync<JObject>("CustomerWithIdDiscriminator|" + customer.Id, new Microsoft.Azure.Cosmos.PartitionKey(customer.PartitionKey));
        var jObject = response.Resource;

        var deserialized = context.Database.Deserialize<CustomerWithIdDiscriminator>(jObject);

        Assert.Equal(customer.Name, deserialized.Name);
        Assert.Equal(customer.Id, deserialized.Id);
    }

    [ConditionalFact]
    public virtual async Task Mismatch_discriminator_fails()
    {
        using var context = Fixture.CreateContext();

        var customer = new Customer { Id = Guid.NewGuid().ToString(), Name = "Customer 3", PartitionKey = "1" };
        customer.Children.Add(new DummyChild { Id = "3", Name = "Child 3" });
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var client = context.Database.GetCosmosClient();
        var container = client.GetContainer(
            context.Database.GetCosmosDatabaseId(),
            context.Model.GetEntityTypes().First(x => x.IsDocumentRoot()).GetContainer());

        var response = await container.ReadItemAsync<JObject>(customer.Id, new Microsoft.Azure.Cosmos.PartitionKey(customer.PartitionKey));
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

        var customer = new Customer { Id = Guid.NewGuid().ToString(), Name = "Customer 3", PartitionKey = "1" };
        customer.Children.Add(new DummyChild { Id = "3", Name = "Child 3" });
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var client = context.Database.GetCosmosClient();
        var container = client.GetContainer(
            context.Database.GetCosmosDatabaseId(),
            context.Model.GetEntityTypes().First(x => x.IsDocumentRoot()).GetContainer());

        var response = await container.ReadItemAsync<JObject>(customer.Id, new Microsoft.Azure.Cosmos.PartitionKey(customer.PartitionKey));
        var jObject = response.Resource;

        jObject.Remove("$type");

        var exception = Assert.Throws<InvalidOperationException>(
            () => context.Database.Deserialize<Customer>(jObject));
        Assert.Equal(CoreStrings.UnableToDiscriminate(nameof(Customer), ""), exception.Message);
    }

    [ConditionalFact]
    public virtual async Task Changefeed()
    {
        using var context = Fixture.CreateContext();

        var client = context.Database.GetCosmosClient();
        var container = client.GetContainer(
            context.Database.GetCosmosDatabaseId(),
            context.Model.GetEntityTypes().First(x => x.IsDocumentRoot()).GetContainer());

        Customer? deserialized = null;
        var processor = container.GetChangeFeedProcessorBuilder("processor", async (IReadOnlyCollection<JObject> changes, CancellationToken ctx) =>
        {
            using var context = Fixture.CreateContext();
            deserialized = context.Database.Deserialize<Customer>(changes.Single());
        }).WithInMemoryLeaseContainer().Build();

        await processor.StartAsync();
        try
        {
            var customer = new Customer { Id = Guid.NewGuid().ToString(), Name = "Customer 1", PartitionKey = "1" };
            customer.Children.Add(new DummyChild { Id = "1", Name = "Child 1" });
            context.Customers.Add(customer);

            await context.SaveChangesAsync();

            var counter = 0;
            while (deserialized == null && counter++ < 30)
            {
                await Task.Delay(1000);
            }
            Assert.NotNull(deserialized);

            Assert.Equal(customer.Name, deserialized.Name);
            Assert.Equal(customer.Id, deserialized.Id);
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
                    b.HasKey(c => c.Id);
                    b.Property(c => c.Name).ToJsonProperty("custom");
                    b.HasPartitionKey(c => c.PartitionKey);
                    b.HasDiscriminatorInJsonId();
                });

            builder.SharedTypeEntity<CustomerWithSharedClrType>("1",
                b =>
                {
                    b.HasRootDiscriminatorInJsonId();
                    b.HasKey(c => c.Id);
                    b.Property(c => c.Name).ToJsonProperty("custom1");
                    b.HasPartitionKey(c => c.PartitionKey);
                });
            builder.SharedTypeEntity<CustomerWithSharedClrType>("2",
                b =>
                {
                    b.HasRootDiscriminatorInJsonId();
                    b.HasKey(c => c.Id);
                    b.Property(c => c.Name).ToJsonProperty("custom2");
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

    public class CustomerWithSharedClrType
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? PartitionKey { get; set; }
    }
}
