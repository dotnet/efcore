// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Storage.Internal
{
    public class DocumentCollectionFactory : IdentityMapFactoryFactoryBase, IDocumentCollectionFactory
    {
        private readonly CosmosClient _cosmosClient;

        public DocumentCollectionFactory(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
        }

        public IDocumentCollection Create(IEntityType entityType)
            => (IDocumentCollection)typeof(DocumentCollectionFactory).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateDocumentCollection))
                .MakeGenericMethod(GetKeyType(entityType.FindPrimaryKey()))
                .Invoke(null, new object[] { _cosmosClient, entityType });

        [UsedImplicitly]
        private static IDocumentCollection CreateDocumentCollection<TKey>(
            CosmosClient cosmosClient,
            IEntityType entityType)
            => new DocumentCollection<TKey>(
                cosmosClient,
                entityType,
                entityType.FindPrimaryKey().GetPrincipalKeyValueFactory<TKey>());
    }
}
