// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Cosmos.Sql.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Update.Internal
{
    public class DocumentCollectionUpdaterFactory
    {
        private readonly CosmosClient _cosmosClient;

        public DocumentCollectionUpdaterFactory(CosmosClient cosmosClient)
            => _cosmosClient = cosmosClient;

        public DocumentCollectionUpdater Create(IEntityType entityType)
            => new DocumentCollectionUpdater(_cosmosClient, entityType);
    }
}
