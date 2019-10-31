// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.TestUtilities
{
    public class CosmosTestStore : TestStore
    {
        private readonly TestStoreContext _storeContext;
        private readonly string _dataFilePath;
        private readonly Action<CosmosDbContextOptionsBuilder> _configureCosmos;
        private bool _initialized;

        private static readonly Guid _runId = Guid.NewGuid();

        public static CosmosTestStore Create(string name, Action<CosmosDbContextOptionsBuilder> extensionConfiguration = null)
            => new CosmosTestStore(name, shared: false, extensionConfiguration: extensionConfiguration);

        public static CosmosTestStore CreateInitialized(string name, Action<CosmosDbContextOptionsBuilder> extensionConfiguration = null)
            => (CosmosTestStore)Create(name, extensionConfiguration).Initialize(null, (Func<DbContext>)null);

        public static CosmosTestStore GetOrCreate(string name) => new CosmosTestStore(name);

        public static CosmosTestStore GetOrCreate(string name, string dataFilePath)
            => new CosmosTestStore(name, dataFilePath: dataFilePath);

        private CosmosTestStore(
            string name, bool shared = true, string dataFilePath = null,
            Action<CosmosDbContextOptionsBuilder> extensionConfiguration = null)
            : base(CreateName(name), shared)
        {
            ConnectionUri = TestEnvironment.DefaultConnection;
            AuthToken = TestEnvironment.AuthToken;
            _configureCosmos = extensionConfiguration == null
                ? (Action<CosmosDbContextOptionsBuilder>)(b => b.ApplyConfiguration())
                : (b =>
                {
                    b.ApplyConfiguration();
                    extensionConfiguration(b);
                });

            _storeContext = new TestStoreContext(this);

            if (dataFilePath != null)
            {
                _dataFilePath = Path.Combine(
                    Path.GetDirectoryName(typeof(CosmosTestStore).GetTypeInfo().Assembly.Location),
                    dataFilePath);
            }
        }

        private static string CreateName(string name) => TestEnvironment.IsEmulator || name == "Northwind"
            ? name
            : (name + _runId);

        public string ConnectionUri { get; }
        public string AuthToken { get; }

        protected override DbContext CreateDefaultContext() => new TestStoreContext(this);

        public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
            => builder.UseCosmos(
                ConnectionUri,
                AuthToken,
                Name,
                _configureCosmos);

        protected override void Initialize(Func<DbContext> createContext, Action<DbContext> seed, Action<DbContext> clean)
        {
            _initialized = true;
            if (_dataFilePath == null)
            {
                base.Initialize(createContext ?? (() => _storeContext), seed, clean);
            }
            else
            {
                using (var context = createContext())
                {
                    CreateFromFile(context).GetAwaiter().GetResult();
                }
            }
        }

        private async Task CreateFromFile(DbContext context)
        {
            if (await context.Database.EnsureCreatedAsync())
            {
                var cosmosClient = context.GetService<CosmosClientWrapper>();
                var serializer = new JsonSerializer();
                using (var fs = new FileStream(_dataFilePath, FileMode.Open, FileAccess.Read))
                using (var sr = new StreamReader(fs))
                using (var reader = new JsonTextReader(sr))
                {
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonToken.StartArray)
                        {
                            NextEntityType:
                            while (reader.Read())
                            {
                                if (reader.TokenType == JsonToken.StartObject)
                                {
                                    string entityName = null;
                                    while (reader.Read())
                                    {
                                        if (reader.TokenType == JsonToken.PropertyName)
                                        {
                                            switch (reader.Value)
                                            {
                                                case "Name":
                                                    reader.Read();
                                                    entityName = (string)reader.Value;
                                                    break;
                                                case "Data":
                                                    while (reader.Read())
                                                    {
                                                        if (reader.TokenType == JsonToken.StartObject)
                                                        {
                                                            var document = serializer.Deserialize<JObject>(reader);

                                                            document["id"] = $"{entityName}|{document["id"]}";
                                                            document["Discriminator"] = entityName;

                                                            await cosmosClient.CreateItemAsync("NorthwindContext", document, null);
                                                        }
                                                        else if (reader.TokenType == JsonToken.EndObject)
                                                        {
                                                            goto NextEntityType;
                                                        }
                                                    }

                                                    break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void Clean(DbContext context)
            => CleanAsync(context).GetAwaiter().GetResult();

        public override async Task CleanAsync(DbContext context)
        {
            var cosmosClientWrapper = context.GetService<CosmosClientWrapper>();
            var created = await cosmosClientWrapper.CreateDatabaseIfNotExistsAsync();
            try
            {
                if (!created)
                {
                    var cosmosClient = context.Database.GetCosmosClient();
                    var database = cosmosClient.GetDatabase(Name);
                    var containerIterator = database.GetContainerQueryIterator<ContainerProperties>();
                    while (containerIterator.HasMoreResults)
                    {
                        foreach (var containerProperties in await containerIterator.ReadNextAsync())
                        {
                            var container = database.GetContainer(containerProperties.Id);
                            var itemIterator = container.GetItemQueryIterator<JObject>(
                                new QueryDefinition("SELECT c.id FROM c"));
                            var partitionKeyPath = containerProperties.PartitionKeyPath;

                            var items = new List<(string, string)>();
                            while (itemIterator.HasMoreResults)
                            {
                                foreach (var itemId in await itemIterator.ReadNextAsync())
                                {
                                    items.Add((itemId["id"].ToString(), itemId[partitionKeyPath]?.ToString()));
                                }
                            }

                            foreach (var item in items)
                            {
                                await container.DeleteItemAsync<object>(
                                    item.Item1,
                                    item.Item2 == null ? PartitionKey.None : new PartitionKey(item.Item1));
                            }
                        }
                    }

                    created = await context.Database.EnsureCreatedAsync();
                    if (!created)
                    {
                        var creator = (CosmosDatabaseCreator)context.GetService<IDatabaseCreator>();
                        await creator.SeedAsync();
                    }
                }
                else
                {
                    await context.Database.EnsureCreatedAsync();
                }
            }
            catch (Exception)
            {
                try
                {
                    await context.Database.EnsureDeletedAsync();
                }
                catch (Exception)
                {
                }

                throw;
            }
        }

        public override void Dispose()
            => throw new InvalidOperationException("Calling Dispose can cause deadlocks. Use DisposeAsync instead.");

        public override async Task DisposeAsync()
        {
            if (_initialized
                && _dataFilePath == null)
            {
                await _storeContext.Database.EnsureDeletedAsync();
            }

            _storeContext.Dispose();
        }

        private class TestStoreContext : DbContext
        {
            private readonly CosmosTestStore _testStore;

            public TestStoreContext(CosmosTestStore testStore)
            {
                _testStore = testStore;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseCosmos(_testStore.ConnectionUri, _testStore.AuthToken, _testStore.Name, _testStore._configureCosmos);
            }
        }
    }
}
