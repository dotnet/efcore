// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.TestUtilities
{
    public class CosmosSqlTestStore : TestStore
    {
        private readonly DocumentClient _documentClient;
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

            _documentClient = new DocumentClient(ConnectionUri, AuthToken);

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
        {
            return builder.UseCosmosSql(
                ConnectionUri,
                AuthToken,
                Name);
        }

        protected override void Initialize(Func<DbContext> createContext, Action<DbContext> seed)
        {
            if (_dataFilePath == null)
            {
                base.Initialize(createContext, seed);
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
                var seedData = JArray.Parse(File.ReadAllText(_dataFilePath));

                foreach (var entityData in seedData)
                {
                    var entityName = (string)entityData["Name"];
                    // TODO: Update the collection name here once there is model builder config
                    var collectionUri = UriFactory.CreateDocumentCollectionUri(Name, "Unicorn");

                    foreach (var document in entityData["Data"])
                    {
                        document["id"] = $"{entityName}|{document["id"]}";
                        document["Discriminator"] = entityName;
                        await _documentClient.CreateDocumentAsync(collectionUri, document);
                    }
                }
            }
        }

        public override void Clean(DbContext context)
        {
            context.Database.EnsureDeletedAsync().GetAwaiter().GetResult();
            context.Database.EnsureCreatedAsync().GetAwaiter().GetResult();
        }

        private void DeleteDatabase()
        {
            try
            {
                _documentClient.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(Name)).GetAwaiter().GetResult();
            }
            catch (DocumentClientException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Ignore as database may not have existed.
            }
        }

        public override void Dispose()
        {
            if (_dataFilePath == null)
            {
                DeleteDatabase();
            }

            _documentClient.Dispose();
            base.Dispose();
        }
    }
}
