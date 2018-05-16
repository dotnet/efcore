// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Azure.Documents.Client;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Storage
{
    public class CosmosClient
    {
        private readonly DocumentClient _documentClient;
        private readonly string _databaseId;

        // TODO: Do caching here.
        public CosmosClient(IDbContextOptions dbContextOptions)
        {
            var options = dbContextOptions.Extensions.OfType<CosmosSqlDbOptionsExtension>().Single();

            _databaseId = options.DatabaseName;
            _documentClient = new DocumentClient(options.ServiceEndPoint, options.AuthKeyOrResourceToken);
        }

        public virtual DocumentClient DocumentClient => _documentClient;

        public virtual string DatabaseId => _databaseId;
    }
}
