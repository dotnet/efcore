// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Azure.Documents.Client;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class DocumentCollectionServiceFactory : IdentityMapFactoryFactoryBase, IDocumentCollectionServiceFactory
    {
        private readonly ConcurrentDictionary<IKey, Func<IDocumentCollectionService>> _factories
            = new ConcurrentDictionary<IKey, Func<IDocumentCollectionService>>();

        public virtual IDocumentCollectionService Create(
            IDocumentDbClientService documentDbClientService, IEntityType entityType)
            => _factories.GetOrAdd(
                entityType.FindPrimaryKey(), (key) => Create(key, documentDbClientService, entityType))();

        private Func<IDocumentCollectionService> Create(
            IKey key, IDocumentDbClientService documentDbClientService, IEntityType entityType)
            => (Func<IDocumentCollectionService>)typeof(DocumentCollectionServiceFactory).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateFactory))
                .MakeGenericMethod(GetKeyType(key))
                .Invoke(null, new object[] {
                    documentDbClientService.Client, documentDbClientService.DatabaseId, entityType, key });

        [UsedImplicitly]
        private static Func<IDocumentCollectionService> CreateFactory<TKey>(
            DocumentClient documentClient, string databaseId, IEntityType entityType, IKey key)
            => () => new DocumentCollectionService<TKey>(
                documentClient, databaseId, entityType, key.GetPrincipalKeyValueFactory<TKey>());
    }
}
