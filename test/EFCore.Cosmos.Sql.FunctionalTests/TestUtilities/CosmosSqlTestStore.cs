// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.TestUtilities
{
    public class CosmosSqlTestStore : TestStore
    {
        private readonly DocumentClient _documentClient;

        public static CosmosSqlTestStore Create(string name) => new CosmosSqlTestStore(name, shared: false);

        public static CosmosSqlTestStore CreateInitialized(string name)
            => (CosmosSqlTestStore)Create(name).Initialize(null, (Func<DbContext>)null, null);

        private CosmosSqlTestStore(string name, bool shared = true, string dataFilePath = null)
            : base(name, shared)
        {
            ConnectionUri = new Uri(TestEnvironment.DefaultConnection);
            AuthToken = TestEnvironment.AuthToken;

            _documentClient = new DocumentClient(ConnectionUri, AuthToken);
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
            DeleteDatabase();

            _documentClient.Dispose();
            base.Dispose();
        }
    }
}
