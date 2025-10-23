// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosTransactionalBatchWrapper : ICosmosTransactionalBatchWrapper
{
    private const int OperationSerializationOverheadOverEstimateInBytes = 200;
    private const int MaxSize = 2_097_152; // 2MiB

    private long _size;

    private readonly TransactionalBatch _transactionalBatch;
    private readonly string _collectionId;
    private readonly PartitionKey _partitionKeyValue;
    private readonly bool _checkSize;
    private readonly bool? _enableContentResponseOnWrite;
    private readonly List<CosmosTransactionalBatchEntry> _entries = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosTransactionalBatchWrapper(
        TransactionalBatch transactionalBatch,
        string collectionId,
        PartitionKey partitionKeyValue,
        bool checkSize,
        bool? enableContentResponseOnWrite)
    {
        _transactionalBatch = transactionalBatch;
        _collectionId = collectionId;
        _partitionKeyValue = partitionKeyValue;
        _checkSize = checkSize;
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
    public bool CreateItem(string id, Stream stream, IUpdateEntry updateEntry)
    {
        var itemRequestOptions = CreateItemRequestOptions(updateEntry, _enableContentResponseOnWrite, out var itemRequestOptionsLength);

        if (_checkSize)
        {
            var size = stream.Length + itemRequestOptionsLength + OperationSerializationOverheadOverEstimateInBytes;

            if (_size + size > MaxSize && _size != 0)
            {
                return false;
            }
            _size += size;
        }

        _transactionalBatch.CreateItemStream(stream, itemRequestOptions);
        _entries.Add(new CosmosTransactionalBatchEntry(updateEntry, CosmosCudOperation.Create, id));

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool ReplaceItem(string documentId, Stream stream, IUpdateEntry updateEntry)
    {
        var itemRequestOptions = CreateItemRequestOptions(updateEntry, _enableContentResponseOnWrite, out var itemRequestOptionsLength);

        if (_checkSize)
        {
            var size = stream.Length + itemRequestOptionsLength + OperationSerializationOverheadOverEstimateInBytes + Encoding.UTF8.GetByteCount(documentId);

            if (_size + size > MaxSize && _size != 0)
            {
                return false;
            }
            _size += size;
        }

        _transactionalBatch.ReplaceItemStream(documentId, stream, itemRequestOptions);
        _entries.Add(new CosmosTransactionalBatchEntry(updateEntry, CosmosCudOperation.Update, documentId));

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool DeleteItem(string documentId, IUpdateEntry updateEntry)
    {
        var itemRequestOptions = CreateItemRequestOptions(updateEntry, _enableContentResponseOnWrite, out var itemRequestOptionsLength);

        if (_checkSize)
        {
            var size = itemRequestOptionsLength + OperationSerializationOverheadOverEstimateInBytes + Encoding.UTF8.GetByteCount(documentId);

            if (_size + size > MaxSize && _size != 0)
            {
                return false;
            }
            _size += size;
        }

        _transactionalBatch.DeleteItem(documentId, itemRequestOptions);
        _entries.Add(new CosmosTransactionalBatchEntry(updateEntry, CosmosCudOperation.Delete, documentId));

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TransactionalBatch GetTransactionalBatch() => _transactionalBatch;

    private TransactionalBatchItemRequestOptions? CreateItemRequestOptions(IUpdateEntry entry, bool? enableContentResponseOnWrite, out int size)
    {
        var helper = RequestOptionsHelper.Create(entry, enableContentResponseOnWrite);
        size = 0;

        if (helper == null)
        {
            return null;
        }

        if (_checkSize && helper.IfMatchEtag != null)
        {
            // Or .Lenght?
            size += Encoding.UTF8.GetByteCount(helper.IfMatchEtag); // @TODO: Currently always a guid...
        }

        // EnableContentResponseOnWrite is a header so no request body size for that.
        return new TransactionalBatchItemRequestOptions { IfMatchEtag = helper.IfMatchEtag, EnableContentResponseOnWrite = helper.EnableContentResponseOnWrite };
    }
}
