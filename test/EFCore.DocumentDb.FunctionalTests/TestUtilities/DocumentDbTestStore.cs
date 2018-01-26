// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Extensions;
using Microsoft.Azure.Documents.Client;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;
using Microsoft.Azure.Documents;
using System.Text;
using System.Net;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class DocumentDbTestStore : TestStore
    {
        private readonly string _scriptPath;

        public DocumentDbTestStore(string name = null, bool shared = true, string scriptPath = null)
            : base(name, shared)
        {
            if (scriptPath != null)
            {
                _scriptPath = Path.Combine(
                    Path.GetDirectoryName(typeof(DocumentDbTestStore).GetTypeInfo().Assembly.Location), scriptPath);
            }
        }

        public static DocumentDbTestStore GetOrCreate(string name)
            => new DocumentDbTestStore(name);

        public static DocumentDbTestStore GetOrCreate(string name, string scriptPath)
            => new DocumentDbTestStore(name, scriptPath: scriptPath);

        public static DocumentDbTestStore GetOrCreateInitialized(string name)
            => new DocumentDbTestStore(name).InitializeDocumentDb(null, (Func<DbContext>)null, null);

        public static DocumentDbTestStore Create(string name)
            => new DocumentDbTestStore(name, shared: false);

        public static DocumentDbTestStore CreateInitialized(string name)
            => new DocumentDbTestStore(name, shared: false).InitializeDocumentDb(null, (Func<DbContext>)null, null);

        public DocumentDbTestStore InitializeDocumentDb(
            IServiceProvider serviceProvider, Func<DbContext> createContext, Action<DbContext> seed)
            => (DocumentDbTestStore)Initialize(serviceProvider, createContext, seed);

        public DocumentDbTestStore InitializeDocumentDb(
            IServiceProvider serviceProvider, Func<DocumentDbTestStore, DbContext> createContext, Action<DbContext> seed)
            => (DocumentDbTestStore)Initialize(serviceProvider, () => createContext(this), seed);

        protected override TestStoreIndex GetTestStoreIndex(IServiceProvider serviceProvider)
            => serviceProvider == null
                ? base.GetTestStoreIndex(null)
                : serviceProvider.GetRequiredService<TestStoreIndex>();

        public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
            => builder.UseDocumentDb(
                TestEnvironment.DefaultServiceEndPoint,
                TestEnvironment.DefaultAuthKey,
                Name);

        protected override void Initialize(Func<DbContext> createContext, Action<DbContext> seed)
        {
            if (_scriptPath != null)
            {
                CreateDatabaseFromScriptPathAsync().GetAwaiter().GetResult();
            }
            else
            {
                base.Initialize(createContext, seed);
            }
        }

        public override void Clean(DbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        private async Task CreateDatabaseFromScriptPathAsync()
        {
            var client = new DocumentClient(TestEnvironment.DefaultServiceEndPoint, TestEnvironment.DefaultAuthKey);

            var databaseUri = UriFactory.CreateDatabaseUri(Name);

            if (await ExistAsync(client, databaseUri))
            {
                return;
            }

            await client.CreateDatabaseAsync(new Database { Id = Name });

            var seedData = JArray.Parse(File.ReadAllText(_scriptPath));
            foreach (var collectionData in seedData)
            {
                var collectionName = (string)collectionData["Name"];
                var collectionUri = UriFactory.CreateDocumentCollectionUri(Name, collectionName);

                await client.CreateDocumentCollectionIfNotExistsAsync(
                    databaseUri,
                    new DocumentCollection { Id = collectionName });

                foreach (var document in collectionData["Data"])
                {
                    await client.CreateDocumentAsync(collectionUri, document);
                }
            }
        }

        private async Task<bool> ExistAsync(DocumentClient client, Uri databaseUri)
        {
            try
            {
                await client.ReadDatabaseAsync(databaseUri);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }

                throw;
            }

            return true;
        }
    }
}
