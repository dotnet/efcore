// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.TestUtilities
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

        public static CosmosTestStore GetOrCreate(string name)
            => new CosmosTestStore(name);

        public static CosmosTestStore GetOrCreate(string name, string dataFilePath)
            => new CosmosTestStore(name, dataFilePath: dataFilePath);

        private CosmosTestStore(
            string name,
            bool shared = true,
            string dataFilePath = null,
            Action<CosmosDbContextOptionsBuilder> extensionConfiguration = null)
            : base(CreateName(name), shared)
        {
            ConnectionUri = TestEnvironment.DefaultConnection;
            AuthToken = TestEnvironment.AuthToken;
            ConnectionString = TestEnvironment.ConnectionString;
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
                    Path.GetDirectoryName(typeof(CosmosTestStore).Assembly.Location),
                    dataFilePath);
            }
        }

        private static string CreateName(string name)
            => TestEnvironment.IsEmulator || name == "Northwind"
                ? name
                : name + _runId;

        public string ConnectionUri { get; }
        public string AuthToken { get; }
        public string ConnectionString { get; }

        protected override DbContext CreateDefaultContext()
            => new TestStoreContext(this);

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
                using var context = createContext();
                CreateFromFile(context).GetAwaiter().GetResult();
            }
        }

        private async Task CreateFromFile(DbContext context)
        {
            if (await context.Database.EnsureCreatedAsync())
            {
                var cosmosClient = context.GetService<CosmosClientWrapper>();
                var serializer = CosmosClientWrapper.Serializer;
                using var fs = new FileStream(_dataFilePath, FileMode.Open, FileAccess.Read);
                using var sr = new StreamReader(fs);
                using var reader = new JsonTextReader(sr);
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

                                                        await cosmosClient.CreateItemAsync(
                                                            "NorthwindContext", document, new FakeUpdateEntry());
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
                            var partitionKey = containerProperties.PartitionKeyPath[1..];
                            var itemIterator = container.GetItemQueryIterator<JObject>(
                                new QueryDefinition("SELECT * FROM c"));

                            var items = new List<(string Id, string PartitionKey)>();
                            while (itemIterator.HasMoreResults)
                            {
                                foreach (var item in await itemIterator.ReadNextAsync())
                                {
                                    items.Add((item["id"].ToString(), item[partitionKey]?.ToString()));
                                }
                            }

                            foreach (var item in items)
                            {
                                await container.DeleteItemAsync<object>(
                                    item.Id,
                                    item.PartitionKey == null ? PartitionKey.None : new PartitionKey(item.PartitionKey));
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

        private class FakeUpdateEntry : IUpdateEntry
        {
            public IEntityType EntityType
                => new FakeEntityType();

            public EntityState EntityState { get => EntityState.Added; set => throw new NotImplementedException(); }

            public IUpdateEntry SharedIdentityEntry
                => throw new NotImplementedException();

            public object GetCurrentValue(IPropertyBase propertyBase)
                => throw new NotImplementedException();

            public TProperty GetCurrentValue<TProperty>(IPropertyBase propertyBase)
                => throw new NotImplementedException();

            public object GetOriginalValue(IPropertyBase propertyBase)
                => throw new NotImplementedException();

            public TProperty GetOriginalValue<TProperty>(IProperty property)
                => throw new NotImplementedException();

            public bool HasTemporaryValue(IProperty property)
                => throw new NotImplementedException();

            public bool IsModified(IProperty property)
                => throw new NotImplementedException();

            public bool IsStoreGenerated(IProperty property)
                => throw new NotImplementedException();

            public void SetOriginalValue(IProperty property, object value)
                => throw new NotImplementedException();

            public void SetPropertyModified(IProperty property)
                => throw new NotImplementedException();

            public void SetStoreGeneratedValue(IProperty property, object value)
                => throw new NotImplementedException();

            public EntityEntry ToEntityEntry()
                => throw new NotImplementedException();

            public object GetRelationshipSnapshotValue(IPropertyBase propertyBase)
                => throw new NotImplementedException();

            public object GetPreStoreGeneratedCurrentValue(IPropertyBase propertyBase)
                => throw new NotImplementedException();

            public bool IsConceptualNull(IProperty property)
                => throw new NotImplementedException();
        }

        public class FakeEntityType : IEntityType
        {
            public object this[string name]
                => null;

            public IEntityType BaseType
                => throw new NotImplementedException();

            public string DefiningNavigationName
                => throw new NotImplementedException();

            public IEntityType DefiningEntityType
                => throw new NotImplementedException();

            public IModel Model
                => throw new NotImplementedException();

            public string Name
                => throw new NotImplementedException();

            public Type ClrType
                => throw new NotImplementedException();

            public bool HasSharedClrType
                => throw new NotImplementedException();

            public bool IsPropertyBag
                => throw new NotImplementedException();

            public IAnnotation FindAnnotation(string name)
                => throw new NotImplementedException();

            public IForeignKey FindForeignKey(IReadOnlyList<IProperty> properties, IKey principalKey, IEntityType principalEntityType)
                => throw new NotImplementedException();

            public IIndex FindIndex(IReadOnlyList<IProperty> properties)
                => throw new NotImplementedException();

            public IIndex FindIndex(string name)
                => throw new NotImplementedException();

            public IKey FindKey(IReadOnlyList<IProperty> properties)
                => throw new NotImplementedException();

            public IKey FindPrimaryKey()
                => throw new NotImplementedException();

            public IProperty FindProperty(string name)
                => null;

            public IServiceProperty FindServiceProperty(string name)
                => throw new NotImplementedException();

            public ISkipNavigation FindSkipNavigation(string name)
                => throw new NotImplementedException();

            public IEnumerable<IAnnotation> GetAnnotations()
                => throw new NotImplementedException();

            public IEnumerable<IForeignKey> GetForeignKeys()
                => throw new NotImplementedException();

            public IEnumerable<IIndex> GetIndexes()
                => throw new NotImplementedException();

            public IEnumerable<IKey> GetKeys()
                => throw new NotImplementedException();

            public IEnumerable<IProperty> GetProperties()
                => throw new NotImplementedException();

            public IEnumerable<IServiceProperty> GetServiceProperties()
                => throw new NotImplementedException();

            public IEnumerable<ISkipNavigation> GetSkipNavigations()
                => throw new NotImplementedException();
        }
    }
}
