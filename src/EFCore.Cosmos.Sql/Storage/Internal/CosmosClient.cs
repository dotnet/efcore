// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Azure.Documents.Client;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Storage.Internal
{
    public class CosmosClient
    {
        // TODO: Do caching here.
        public CosmosClient(IDbContextOptions dbContextOptions)
        {
            var options = dbContextOptions.Extensions.OfType<CosmosSqlDbOptionsExtension>().Single();

            DatabaseId = options.DatabaseName;
            DocumentClient = new DocumentClient(options.ServiceEndPoint, options.AuthKeyOrResourceToken);
        }

        public virtual DocumentClient DocumentClient { get; }

        public virtual string DatabaseId { get; }
    }
}
