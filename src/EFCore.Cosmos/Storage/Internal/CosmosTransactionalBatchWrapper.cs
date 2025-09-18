// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CosmosTransactionalBatchWrapper : ICosmosTransactionalBatchWrapper
    {
        private readonly TransactionalBatch _transactionalBatch;
        private readonly string _collectionId;
        private readonly PartitionKey _partitionKeyValue;
        private readonly bool? _enableContentResponseOnWrite;
        private readonly List<CosmosTransactionalBatchEntry> _entries = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CosmosTransactionalBatchWrapper(TransactionalBatch transactionalBatch, string collectionId, PartitionKey partitionKeyValue, bool? enableContentResponseOnWrite)
        {
            _transactionalBatch = transactionalBatch;
            _collectionId = collectionId;
            _partitionKeyValue = partitionKeyValue;
            _enableContentResponseOnWrite = enableContentResponseOnWrite;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public IReadOnlyList<CosmosTransactionalBatchEntry> Entries => _entries;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public PartitionKey PartitionKeyValue => _partitionKeyValue;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public string CollectionId => _collectionId;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void CreateItem(JToken document, IUpdateEntry updateEntry)
        {
            // stream is disposed by TransactionalBatch.ExecuteAsync
            var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, new UTF8Encoding(), bufferSize: 1024, leaveOpen: true);

            using var jsonWriter = new JsonTextWriter(writer);
            CosmosClientWrapper.Serializer.Serialize(jsonWriter, document);
            jsonWriter.Flush();

            var itemRequestOptions = CreateItemRequestOptions(updateEntry, _enableContentResponseOnWrite);
            _transactionalBatch.CreateItemStream(stream, itemRequestOptions);
            _entries.Add(new CosmosTransactionalBatchEntry(updateEntry, CosmosCudOperation.Create, document["id"]!.ToString()));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void ReplaceItem(string documentId, JToken document, IUpdateEntry updateEntry)
        {
            // stream is disposed by TransactionalBatch.ExecuteAsync
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, new UTF8Encoding(), bufferSize: 1024, leaveOpen: true);
            using var _ = writer;
            using var jsonWriter = new JsonTextWriter(writer);
            CosmosClientWrapper.Serializer.Serialize(jsonWriter, document);
            jsonWriter.Flush();

            var itemRequestOptions = CreateItemRequestOptions(updateEntry, _enableContentResponseOnWrite);

            _transactionalBatch.ReplaceItemStream(documentId, stream, itemRequestOptions);
            _entries.Add(new CosmosTransactionalBatchEntry(updateEntry, CosmosCudOperation.Update, document["id"]!.ToString()));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void DeleteItem(string documentId, IUpdateEntry updateEntry)
        {
            var itemRequestOptions = CreateItemRequestOptions(updateEntry, _enableContentResponseOnWrite);
            _transactionalBatch.DeleteItem(documentId, itemRequestOptions);
            _entries.Add(new CosmosTransactionalBatchEntry(updateEntry, CosmosCudOperation.Delete, documentId));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public TransactionalBatch GetTransactionalBatch() => _transactionalBatch;

        private static TransactionalBatchItemRequestOptions? CreateItemRequestOptions(IUpdateEntry entry, bool? enableContentResponseOnWrite)
        {
            var helper = RequestOptionsHelper.Create(entry, enableContentResponseOnWrite);

            return helper == null
                ? null
                : new TransactionalBatchItemRequestOptions { IfMatchEtag = helper.IfMatchEtag, EnableContentResponseOnWrite = helper.EnableContentResponseOnWrite };
        }
    }
}
