// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Storage.Internal
{
    public class CosmosSqlDatabase : Database
    {
        private readonly CosmosClient _cosmosClient;
        private readonly IDocumentCollectionFactory _documentCollectionFactory;
        private Dictionary<IEntityType, IDocumentCollection> _documentCollections
            = new Dictionary<IEntityType, IDocumentCollection>();

        public CosmosSqlDatabase(
            [NotNull] DatabaseDependencies dependencies,
            CosmosClient cosmosClient,
            IDocumentCollectionFactory documentCollectionFactory)
            : base(dependencies)
        {
            _cosmosClient = cosmosClient;
            _documentCollectionFactory = documentCollectionFactory;
        }

        public override int SaveChanges(IReadOnlyList<IUpdateEntry> entries)
        {
            throw new NotImplementedException();
        }

        public override async Task<int> SaveChangesAsync(
            IReadOnlyList<IUpdateEntry> entries, CancellationToken cancellationToken = default)
        {
            var rowsAffected = 0;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var entityType = entry.EntityType;

                Debug.Assert(!entityType.IsAbstract());

                if (!_documentCollections.TryGetValue(entityType, out var documentCollection))
                {
                    _documentCollections.Add(
                        entityType, documentCollection = _documentCollectionFactory.Create(entityType));
                }

                await documentCollection.SaveAsync(entry, cancellationToken);

                rowsAffected++;
            }

            return rowsAffected;
        }
    }
}
