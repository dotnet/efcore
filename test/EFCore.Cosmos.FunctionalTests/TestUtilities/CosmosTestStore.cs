// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.TestUtilities
{
    public class CosmosTestStore : TestStore
    {
        private readonly TestStoreContext _storeContext;
        private readonly string _dataFilePath;

        public static CosmosTestStore Create(string name) => new CosmosTestStore(name, shared: false);

        public static CosmosTestStore CreateInitialized(string name)
            => (CosmosTestStore)Create(name).Initialize(null, (Func<DbContext>)null, null);

        public static CosmosTestStore GetOrCreate(string name) => new CosmosTestStore(name);

        public static CosmosTestStore GetOrCreate(string name, string dataFilePath)
            => new CosmosTestStore(name, dataFilePath: dataFilePath);

        private CosmosTestStore(string name, bool shared = true, string dataFilePath = null)
            : base(name, shared)
        {
            ConnectionUri = TestEnvironment.DefaultConnection;
            AuthToken = TestEnvironment.AuthToken;

            _storeContext = new TestStoreContext(this);

            if (dataFilePath != null)
            {
                _dataFilePath = Path.Combine(
                    Path.GetDirectoryName(typeof(CosmosTestStore).GetTypeInfo().Assembly.Location),
                    dataFilePath);
            }
        }

        public string ConnectionUri { get; }
        public string AuthToken { get; }

        public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
            => builder.UseCosmos(
                ConnectionUri,
                AuthToken,
                Name);

        protected override void Initialize(Func<DbContext> createContext, Action<DbContext> seed)
        {
            if (_dataFilePath == null)
            {
                base.Initialize(createContext ?? (() => _storeContext), seed);
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
                var cosmosClient = context.GetService<CosmosClient>();
                var seedData = JArray.Parse(File.ReadAllText(_dataFilePath));

                foreach (var entityData in seedData)
                {
                    var entityName = (string)entityData["Name"];

                    foreach (var document in entityData["Data"])
                    {
                        document["id"] = $"{entityName}|{document["id"]}";
                        document["Discriminator"] = entityName;
                        // TODO: Stream the document
                        await cosmosClient.CreateDocumentAsync("NorthwindContext", document);
                    }
                }
            }
        }

        public override void Clean(DbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        public override void Dispose()
        {
            if (_dataFilePath == null)
            {
                _storeContext.Database.EnsureDeleted();
            }

            _storeContext.Dispose();
            base.Dispose();
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
                optionsBuilder.UseCosmos(_testStore.ConnectionUri, _testStore.AuthToken, _testStore.Name);
            }
        }
    }
}
