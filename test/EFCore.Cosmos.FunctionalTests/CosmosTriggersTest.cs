// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;

namespace Microsoft.EntityFrameworkCore;

public class CosmosTriggersTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string StoreName
        => "CosmosTriggersTest";

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;

    [ConditionalFact]
    public async Task Triggers_are_executed_on_SaveChanges()
    {
        var contextFactory = await InitializeAsync<TriggersContext>(
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported)));

        using (var context = contextFactory.CreateContext())
        {
            await CreateTriggersInCosmosAsync(context);

            Assert.Empty(await context.Set<TriggerExecutionLog>().ToListAsync());

            var product = new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 10.00m
            };
            context.Products.Add(product);

            await context.SaveChangesAsync();

            var logs = await context.Set<TriggerExecutionLog>().ToListAsync();

            Assert.Contains(logs, l => l.TriggerName == "PreInsertTrigger" && l.Operation == "INSERT");
        }

        using (var context = contextFactory.CreateContext())
        {
            var product = await context.Products.SingleAsync();
            product.Name = "Updated Product";

            await context.SaveChangesAsync();

            var logs = await context.Set<TriggerExecutionLog>().Where(l => l.Operation == "UPDATE").ToListAsync();

            Assert.Contains(logs, l => l.TriggerName == "UpdateTrigger" && l.Operation == "UPDATE");
        }

        using (var context = contextFactory.CreateContext())
        {
            var product = await context.Products.SingleAsync();
            context.Products.Remove(product);

            await context.SaveChangesAsync();

            var logs = await context.Set<TriggerExecutionLog>().Where(l => l.Operation == "DELETE").ToListAsync();

            Assert.Contains(logs, l => l.TriggerName == "PostDeleteTrigger" && l.Operation == "DELETE");
        }
    }

    private async Task CreateTriggersInCosmosAsync(TriggersContext context)
    {
        await context.Database.EnsureCreatedAsync();

        var cosmosClient = context.Database.GetCosmosClient();
        var databaseId = context.Database.GetCosmosDatabaseId();
        var database = cosmosClient.GetDatabase(databaseId);

        // Get the container name from the Product entity type metadata
        var productEntityType = context.Model.FindEntityType(typeof(Product));
        var containerName = productEntityType!.GetContainer()!;
        var container = database.GetContainer(containerName);

        var preInsertTriggerDefinition = new TriggerProperties
        {
            Id = "PreInsertTrigger",
            TriggerType = TriggerType.Pre,
            TriggerOperation = TriggerOperation.Create,
            Body = @"
function preInsertTrigger() {
    var context = getContext();
    var request = context.getRequest();
    var doc = request.getBody();
    
    // Log the trigger execution using the same partition key as the document being created
    var logEntry = {
        id: 'log_' + Math.random().toString().replace('.', ''),
        $type: 'TriggerExecutionLog',
        TriggerName: 'PreInsertTrigger',
        Operation: 'INSERT',
        DocumentId: doc.id,
        ExecutedAt: new Date().toISOString(),
        PartitionKey: doc.PartitionKey // Use the same partition key as the document
    };
    
    // Create a separate document to track trigger execution
    var collection = context.getCollection();
    var accepted = collection.createDocument(collection.getSelfLink(), logEntry);
    if (!accepted) throw new Error('Failed to log trigger execution');
}"
        };

        try
        {
            await container.Scripts.CreateTriggerAsync(preInsertTriggerDefinition);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            // Trigger already exists, replace it
            await container.Scripts.ReplaceTriggerAsync(preInsertTriggerDefinition);
        }

        var postDeleteTriggerDefinition = new TriggerProperties
        {
            Id = "PostDeleteTrigger",
            TriggerType = TriggerType.Post,
            TriggerOperation = TriggerOperation.Delete,
            Body = @"
function postDeleteTrigger() {
    var context = getContext();
    
    // For delete operations, we can't access the deleted document
    // So we'll just create a log entry with a timestamp-based ID
    var logEntry = {
        id: 'log_' + Math.random().toString().replace('.', ''),
        $type: 'TriggerExecutionLog',
        TriggerName: 'PostDeleteTrigger',
        Operation: 'DELETE',
        DocumentId: 'deleted_document',
        ExecutedAt: new Date().toISOString(),
        PartitionKey: 'Products' // Use the same partition key as Product documents
    };
    
    // Create a separate document to track trigger execution
    var collection = context.getCollection();
    var accepted = collection.createDocument(collection.getSelfLink(), logEntry);
    if (!accepted) throw new Error('Failed to log trigger execution');
}"
        };

        try
        {
            await container.Scripts.CreateTriggerAsync(postDeleteTriggerDefinition);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            // Trigger already exists, replace it
            await container.Scripts.ReplaceTriggerAsync(postDeleteTriggerDefinition);
        }

        var updateTriggerDefinition = new TriggerProperties
        {
            Id = "UpdateTrigger",
            TriggerType = TriggerType.Pre,
            TriggerOperation = TriggerOperation.Replace,
            Body = @"
function updateTrigger() {
    var context = getContext();
    var request = context.getRequest();
    var doc = request.getBody();
    
    // Log the trigger execution using the same partition key as the document being updated
    var logEntry = {
        id: 'log_' + Math.random().toString().replace('.', ''),
        $type: 'TriggerExecutionLog',
        TriggerName: 'UpdateTrigger',
        Operation: 'UPDATE',
        DocumentId: doc.id,
        ExecutedAt: new Date().toISOString(),
        PartitionKey: doc.PartitionKey // Use the same partition key as the document
    };
    
    // Create a separate document to track trigger execution
    var collection = context.getCollection();
    var accepted = collection.createDocument(collection.getSelfLink(), logEntry);
    if (!accepted) throw new Error('Failed to log trigger execution');
}"
        };

        try
        {
            await container.Scripts.CreateTriggerAsync(updateTriggerDefinition);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            // Trigger already exists, replace it
            await container.Scripts.ReplaceTriggerAsync(updateTriggerDefinition);
        }
    }

    protected class TriggersContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Id);
                entity.HasPartitionKey(e => e.PartitionKey);
                entity.HasTrigger("PreInsertTrigger", TriggerType.Pre, TriggerOperation.Create);
                entity.HasTrigger("PostDeleteTrigger", TriggerType.Post, TriggerOperation.Delete);
                entity.HasTrigger("UpdateTrigger", TriggerType.Pre, TriggerOperation.Replace);
            });

            modelBuilder.Entity<TriggerExecutionLog>(entity =>
            {
                entity.HasPartitionKey(e => e.PartitionKey);
            });
        }
    }

    protected class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public string PartitionKey { get; set; } = "Products";
    }

    protected class TriggerExecutionLog
    {
        public string Id { get; set; } = null!;
        public string TriggerName { get; set; } = null!;
        public string Operation { get; set; } = null!;
        public string DocumentId { get; set; } = null!;
        public DateTime ExecutedAt { get; set; }
        public string PartitionKey { get; set; } = "Products";
    }
}
