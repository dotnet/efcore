// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Infrastructure;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Storage.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.TestUtilities
{
    public class CosmosSqlTestStore : TestStore
    {
        private readonly TestStoreContext _storeContext;
        private readonly string _dataFilePath;

        public static CosmosSqlTestStore Create(string name) => new CosmosSqlTestStore(name, shared: false);

        public static CosmosSqlTestStore CreateInitialized(string name)
            => (CosmosSqlTestStore)Create(name).Initialize(null, (Func<DbContext>)null, null);

        public static CosmosSqlTestStore GetOrCreate(string name) => new CosmosSqlTestStore(name);

        public static CosmosSqlTestStore GetOrCreate(string name, string dataFilePath)
            => new CosmosSqlTestStore(name, dataFilePath: dataFilePath);

        private CosmosSqlTestStore(string name, bool shared = true, string dataFilePath = null)
            : base(name, shared)
        {
            ConnectionUri = new Uri(TestEnvironment.DefaultConnection);
            AuthToken = TestEnvironment.AuthToken;

            _storeContext = new TestStoreContext(this);

            if (dataFilePath != null)
            {
                _dataFilePath = Path.Combine(
                    Path.GetDirectoryName(typeof(CosmosSqlTestStore).GetTypeInfo().Assembly.Location),
                    dataFilePath);
            }
        }

        public Uri ConnectionUri { get; private set; }
        public string AuthToken { get; private set; }

        public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
            => builder.UseCosmosSql(
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
                        // TODO: Update the collection name here once there is model builder config
                        // TODO: Stream the document
                        await cosmosClient.CreateDocumentAsync("Unicorn", document);
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
            private readonly CosmosSqlTestStore _testStore;

            public TestStoreContext(CosmosSqlTestStore testStore)
            {
                _testStore = testStore;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseCosmosSql(_testStore.ConnectionUri, _testStore.AuthToken, _testStore.Name);
            }
        }
    }
}
