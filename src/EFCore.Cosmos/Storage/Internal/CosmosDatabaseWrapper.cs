// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Update.Internal;
using Newtonsoft.Json.Linq;
using Database = Microsoft.EntityFrameworkCore.Storage.Database;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosDatabaseWrapper : Database, IResettableService
{
    private readonly Dictionary<IEntityType, DocumentSource> _documentCollections = new();

    private readonly ICosmosClientWrapper _cosmosClient;
    private readonly bool _sensitiveLoggingEnabled;
    private readonly bool _bulkExecutionEnabled;

    private readonly ICurrentDbContext _currentDbContext;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosDatabaseWrapper(
        DatabaseDependencies dependencies,
        ICurrentDbContext currentDbContext,
        ICosmosClientWrapper cosmosClient,
        ICosmosSingletonOptions cosmosSingletonOptions,
        ISessionTokenStorageFactory sessionTokenStorageFactory,
        ILoggingOptions loggingOptions)
        : base(dependencies)
    {
        _currentDbContext = currentDbContext;
        _cosmosClient = cosmosClient;
        _bulkExecutionEnabled = cosmosSingletonOptions.EnableBulkExecution == true;
        SessionTokenStorage = sessionTokenStorageFactory.Create(currentDbContext.Context);

        if (loggingOptions.IsSensitiveDataLoggingEnabled)
        {
            _sensitiveLoggingEnabled = true;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ISessionTokenStorage SessionTokenStorage { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override async Task<int> SaveChangesAsync(
        IList<IUpdateEntry> entries,
        CancellationToken cancellationToken = default)
    {
        if (entries.Count == 0)
        {
            return 0;
        }

        var rowsAffected = 0;
        var groups = CreateSaveGroups(entries);

        if (_bulkExecutionEnabled)
        {
            var tasks = new List<Task<bool>>();
            foreach (var write in groups.SingleUpdateEntries)
            {
                tasks.Add(SaveAsync(write, cancellationToken));
            }
            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            foreach (var result in results)
            {
                if (result)
                {
                    rowsAffected++;
                }
            }
        }
        else
        {
            foreach (var write in groups.SingleUpdateEntries)
            {
                if (await SaveAsync(write, cancellationToken).ConfigureAwait(false))
                {
                    rowsAffected++;
                }
            }
        }

        foreach (var batch in groups.BatchableUpdateEntries)
        {
            if (batch.UpdateEntries.Count == 1 && _currentDbContext.Context.Database.AutoTransactionBehavior != AutoTransactionBehavior.Always)
            {
                if (await SaveAsync(batch.UpdateEntries[0], cancellationToken).ConfigureAwait(false))
                {
                    rowsAffected++;
                }

                continue;
            }

            foreach (var transaction in CreateTransactions(batch))
            {
                try
                {
                    var response = await _cosmosClient.ExecuteTransactionalBatchAsync(transaction, SessionTokenStorage, cancellationToken).ConfigureAwait(false);
                    if (!response.IsSuccess)
                    {
                        var exception = WrapUpdateException(response.Exception, response.ErroredEntries);
                        if (exception is not DbUpdateConcurrencyException
                            || !(await Dependencies.Logger.OptimisticConcurrencyExceptionAsync(
                                    transaction.Entries.First().Entry.Context, transaction.Entries.Select(x => x.Entry).ToArray(), (DbUpdateConcurrencyException)exception, null, cancellationToken)
                                .ConfigureAwait(false)).IsSuppressed)
                        {
                            throw exception;
                        }
                    }
                }
                catch (Exception ex) when (ex is not DbUpdateException and not OperationCanceledException)
                {
                    var exception = WrapUpdateException(ex, transaction.Entries.Select(x => x.Entry).ToArray());
                    throw exception;
                }

                rowsAffected += transaction.Entries.Count;
            }
        }

        return rowsAffected;
    }

    private SaveGroups CreateSaveGroups(IList<IUpdateEntry> entries)
    {
        if (_bulkExecutionEnabled && _currentDbContext.Context.Database.AutoTransactionBehavior != AutoTransactionBehavior.Never)
        {
            Dependencies.Logger.BulkExecutionWithTransactionalBatch(_currentDbContext.Context.Database.AutoTransactionBehavior);
        }

        var count = entries.Count;
        var rootEntriesToSave = new HashSet<IUpdateEntry>();

        for (var i = 0; i < count; i++)
        {
            var entry = entries[i];
            Check.DebugAssert(!entry.EntityType.IsAbstract(), $"{entry.EntityType} is abstract");

            if (!entry.EntityType.IsDocumentRoot())
            {
                var root = GetRootDocument((InternalEntityEntry)entry);
                if (rootEntriesToSave.Add(root)
                    && root.EntityState == EntityState.Unchanged)
                {
#pragma warning disable EF1001 // Internal EF Core API usage.
                    // #16707
                    ((InternalEntityEntry)root).SetEntityState(EntityState.Modified);
#pragma warning restore EF1001 // Internal EF Core API usage.
                    entries.Add(root);
                }

                continue;
            }

            rootEntriesToSave.Add(entry);
        }

        var cosmosUpdateEntries = rootEntriesToSave.Select(x => CreateCosmosUpdateEntry(x)!).Where(x => x != null).ToList();

        if (cosmosUpdateEntries.Count == 0 ||
            _currentDbContext.Context.Database.AutoTransactionBehavior == AutoTransactionBehavior.Never ||
            (cosmosUpdateEntries.Count <= 1 && _currentDbContext.Context.Database.AutoTransactionBehavior != AutoTransactionBehavior.Always))
        {
            return new SaveGroups
            {
                BatchableUpdateEntries = Array.Empty<(Grouping Key, List<CosmosUpdateEntry> UpdateEntries)>(),
                SingleUpdateEntries = cosmosUpdateEntries
            };
        }

        var singleUpdateEntries = new List<CosmosUpdateEntry>();
        var batchableEntries = new List<CosmosUpdateEntry>();
        foreach (var entry in cosmosUpdateEntries)
        {
            if (entry.Entry.EntityType.GetTriggers().Any())
            {
                singleUpdateEntries.Add(entry);
            }
            else
            {
                batchableEntries.Add(entry);
            }
        }

        if (_currentDbContext.Context.Database.AutoTransactionBehavior == AutoTransactionBehavior.Always)
        {
            if (singleUpdateEntries.Count >= 1)
            {
                if (rootEntriesToSave.Count >= 2)
                {
                    throw new InvalidOperationException(CosmosStrings.SaveChangesAutoTransactionBehaviorAlwaysTriggerAtomicity);
                }

                // There is only 1 entry, and it has a trigger
                return new SaveGroups
                {
                    BatchableUpdateEntries = [],
                    SingleUpdateEntries = singleUpdateEntries
                };
            }

            var firstEntry = batchableEntries[0];
            var key = new Grouping(firstEntry.CollectionId, _cosmosClient.GetPartitionKeyValue(firstEntry.Entry));
            if (batchableEntries.Count > 100 ||
                !batchableEntries.All(entry =>
                    entry.CollectionId == key.ContainerId &&
                    _cosmosClient.GetPartitionKeyValue(entry.Entry) == key.PartitionKeyValue))
            {
                throw new InvalidOperationException(CosmosStrings.SaveChangesAutoTransactionBehaviorAlwaysAtomicity);
            }

            return new SaveGroups
            {
                BatchableUpdateEntries = [(key, batchableEntries)],
                SingleUpdateEntries = []
            };
        }

        var batches = CreateBatches(batchableEntries);

        // For bulk it is important that single writes are always classified as singleUpdateEntries so that they will be executed in parallel
        if (_bulkExecutionEnabled && _currentDbContext.Context.Database.AutoTransactionBehavior != AutoTransactionBehavior.Always)
        {
            for (var i = batches.Count - 1; i >= 0; i--)
            {
                var batch = batches[i];
                if (batch.UpdateEntries.Count == 1)
                {
                    batches.RemoveAt(i);
                    singleUpdateEntries.Add(batch.UpdateEntries[0]);
                }
            }
        }

        return new SaveGroups
        {
            BatchableUpdateEntries = batches,
            SingleUpdateEntries = singleUpdateEntries
        };
    }

    private List<(Grouping Key, List<CosmosUpdateEntry> UpdateEntries)> CreateBatches(List<CosmosUpdateEntry> entries)
    {
        var results = new List<(Grouping Key, List<CosmosUpdateEntry> UpdateEntries)>();
        var buckets = new Dictionary<Grouping, List<CosmosUpdateEntry>>();

        foreach (var entry in entries)
        {
            var key = new Grouping(entry.CollectionId, _cosmosClient.GetPartitionKeyValue(entry.Entry));

            ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(buckets, key, out var exists);
            if (!exists || list is null)
            {
                list = [];
                results.Add((key, list));
            }

            list.Add(entry);
        }

        return results;
    }

    private CosmosUpdateEntry? CreateCosmosUpdateEntry(IUpdateEntry entry)
    {
        var entityType = entry.EntityType;
        var documentSource = GetDocumentSource(entityType);
        var collectionId = documentSource.GetContainerId();
        var operation = entry.EntityState switch
        {
            EntityState.Added => CosmosCudOperation.Create,
            EntityState.Modified => CosmosCudOperation.Update,
            EntityState.Deleted => CosmosCudOperation.Delete,
            _ => (CosmosCudOperation?)null
        };

        if (operation == null)
        {
            return null;
        }

        JObject? document = null;

        if (entry.SharedIdentityEntry != null)
        {
            if (entry.EntityState == EntityState.Deleted)
            {
                return null;
            }

            if (operation == CosmosCudOperation.Create)
            {
                operation = CosmosCudOperation.Update;
            }
        }

        switch (operation)
        {
            case CosmosCudOperation.Create:
                var primaryKey = entityType.FindPrimaryKey();
                if (primaryKey != null)
                {
                    // The code below checks for primary key properties that are not configured for value generation but have not
                    // had a non-sentinel (effectively, non-CLR default) value set. For composite keys, we only check if at least
                    // one property has value generation or a value set, since it is normal to have non-value generated parts of composite
                    // keys where one part is the CLR default. However, on Cosmos, we exclude the partition key properties from this
                    // check to ensure that, even if partition key properties have been set, at least one other primary key property is
                    // also set.
                    var partitionPropertyNeedsValue = true;
                    var propertyNeedsValue = true;
                    var allPkPropertiesAreFk = true;
                    IProperty? firstNonPartitionKeyProperty = null;

                    var partitionKeyProperties = entityType.GetPartitionKeyProperties();
                    foreach (var property in primaryKey.Properties)
                    {
                        if (property.IsForeignKey())
                        {
                            // FK properties conceptually get their value from the associated principal key, which can be handled
                            // automatically by the update pipeline in some cases, so exclude from this check.
                            continue;
                        }

                        allPkPropertiesAreFk = false;

                        var isPartitionKeyProperty = partitionKeyProperties.Contains(property);
                        if (!isPartitionKeyProperty)
                        {
                            firstNonPartitionKeyProperty = property;
                        }

                        if (property.ValueGenerated != ValueGenerated.Never
                            || entry.HasExplicitValue(property))
                        {
                            if (!isPartitionKeyProperty)
                            {
                                propertyNeedsValue = false;
                                break;
                            }

                            partitionPropertyNeedsValue = false;
                        }
                    }

                    if (!allPkPropertiesAreFk)
                    {
                        try
                        {
                            if (firstNonPartitionKeyProperty != null
                            && propertyNeedsValue)
                            {
                                // There were non-partition key properties, so only throw if it is one of these that is not set,
                                // ignoring partition key properties.
                                Dependencies.Logger.PrimaryKeyValueNotSet(firstNonPartitionKeyProperty!);
                            }
                            else if (firstNonPartitionKeyProperty == null
                                     && partitionPropertyNeedsValue)
                            {
                                // There were no non-partition key properties in the primary key, so in this case check if any of these is not set.
                                Dependencies.Logger.PrimaryKeyValueNotSet(primaryKey.Properties[0]);
                            }
                        }
                        catch (InvalidOperationException ex)
                        {
                            throw WrapUpdateException(ex, [entry]);
                        }
                    }
                }

                document = documentSource.GetCurrentDocument(entry);
                if (document != null)
                {
                    documentSource.UpdateDocument(document, entry);
                }
                else
                {
                    document = documentSource.CreateDocument(entry);
                }
                break;

            case CosmosCudOperation.Update:
                document = documentSource.GetCurrentDocument(entry);
                if (document != null)
                {
                    if (documentSource.UpdateDocument(document, entry) == null)
                    {
                        return null;
                    }
                }
                else
                {
                    document = documentSource.CreateDocument(entry);

                    var propertyName = entityType.FindDiscriminatorProperty()?.GetJsonPropertyName();
                    if (propertyName != null)
                    {
                        document[propertyName] =
                            JToken.FromObject(entityType.GetDiscriminatorValue()!, CosmosClientWrapper.Serializer);
                    }
                }
                break;

            case CosmosCudOperation.Delete:
                break;

            default:
                throw new UnreachableException();
        }

        return new CosmosUpdateEntry
        {
            CollectionId = collectionId,
            Document = document,
            DocumentSource = documentSource,
            Entry = entry,
            Operation = operation.Value
        };
    }

    private IEnumerable<ICosmosTransactionalBatchWrapper> CreateTransactions((Grouping Key, List<CosmosUpdateEntry> UpdateEntries) batch)
    {
        const int maxOperationsPerBatch = 100;

        // We turn off size checking in EF for AutoTransactionBehavior.Always as all entities will always go in a single transaction.
        // Cosmos will throw if the request is too large.
        var checkSize = _currentDbContext.Context.Database.AutoTransactionBehavior != AutoTransactionBehavior.Always;
        var transaction = _cosmosClient.CreateTransactionalBatch(batch.Key.ContainerId, batch.Key.PartitionKeyValue, checkSize);

        foreach (var updateEntry in batch.UpdateEntries)
        {
            // Stream is disposed by Transaction.ExecuteAsync
            var stream = updateEntry.Document != null ? CosmosClientWrapper.Serialize(updateEntry.Document) : null;

            // With AutoTransactionBehavior.Always, AddToTransaction will always return true.
            if (!AddToTransaction(transaction, updateEntry, stream))
            {
                yield return transaction;
                transaction = _cosmosClient.CreateTransactionalBatch(batch.Key.ContainerId, batch.Key.PartitionKeyValue, checkSize);
                AddToTransaction(transaction, updateEntry, stream);
                continue;
            }

            if (checkSize && transaction.Entries.Count == maxOperationsPerBatch)
            {
                yield return transaction;
                transaction = _cosmosClient.CreateTransactionalBatch(batch.Key.ContainerId, batch.Key.PartitionKeyValue, checkSize);
            }
        }

        if (transaction.Entries.Count != 0)
        {
            yield return transaction;
        }
    }

    private bool AddToTransaction(ICosmosTransactionalBatchWrapper transaction, CosmosUpdateEntry updateEntry, Stream? stream)
    {
        var id = updateEntry.DocumentSource.GetId(updateEntry.Entry.SharedIdentityEntry ?? updateEntry.Entry);
        return updateEntry.Operation switch
        {
            CosmosCudOperation.Create => transaction.CreateItem(id, stream!, updateEntry.Entry),
            CosmosCudOperation.Update => transaction.ReplaceItem(id, stream!, updateEntry.Entry),
            CosmosCudOperation.Delete => transaction.DeleteItem(id, updateEntry.Entry),
            _ => throw new UnreachableException(),
        };
    }

    private async Task<bool> SaveAsync(CosmosUpdateEntry updateEntry, CancellationToken cancellationToken)
    {
        try
        {
            return updateEntry.Operation switch
            {
                CosmosCudOperation.Create => await _cosmosClient.CreateItemAsync(
                                    updateEntry.CollectionId,
                                    updateEntry.Document!,
                                    updateEntry.Entry,
                                    SessionTokenStorage,
                                    cancellationToken).ConfigureAwait(false),
                CosmosCudOperation.Update => await _cosmosClient.ReplaceItemAsync(
                                    updateEntry.CollectionId,
                                    updateEntry.DocumentSource.GetId(updateEntry.Entry.SharedIdentityEntry ?? updateEntry.Entry),
                                    updateEntry.Document!,
                                    updateEntry.Entry,
                                    SessionTokenStorage,
                                    cancellationToken).ConfigureAwait(false),
                CosmosCudOperation.Delete => await _cosmosClient.DeleteItemAsync(
                                    updateEntry.CollectionId,
                                    updateEntry.DocumentSource.GetId(updateEntry.Entry),
                                    updateEntry.Entry,
                                    SessionTokenStorage,
                                    cancellationToken).ConfigureAwait(false),
                _ => throw new UnreachableException(),
            };
        }
        catch (Exception ex) when (ex is not DbUpdateException and not UnreachableException and not OperationCanceledException)
        {
            var errorEntries = new[] { updateEntry.Entry };
            var exception = WrapUpdateException(ex, errorEntries);

            if (exception is not DbUpdateConcurrencyException
                || !(await Dependencies.Logger.OptimisticConcurrencyExceptionAsync(
                        updateEntry.Entry.Context, errorEntries, (DbUpdateConcurrencyException)exception, null, cancellationToken)
                    .ConfigureAwait(false)).IsSuppressed)
            {
                throw exception;
            }

            return false;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DocumentSource GetDocumentSource(IEntityType entityType)
    {
        if (!_documentCollections.TryGetValue(entityType, out var documentSource))
        {
            _documentCollections.Add(
                entityType, documentSource = new DocumentSource(entityType, this));
        }

        return documentSource;
    }

#pragma warning disable EF1001 // Internal EF Core API usage.
    // Issue #16707
    private IUpdateEntry GetRootDocument(InternalEntityEntry entry)
    {
        var stateManager = entry.StateManager;
        var ownership = entry.EntityType.FindOwnership()!;
        var principal = stateManager.FindPrincipal(entry, ownership);
        if (principal == null)
        {
            if (_sensitiveLoggingEnabled)
            {
                throw new InvalidOperationException(
                    CosmosStrings.OrphanedNestedDocumentSensitive(
                        entry.EntityType.DisplayName(),
                        ownership.PrincipalEntityType.DisplayName(),
                        entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey()!.Properties)));
            }

            throw new InvalidOperationException(
                CosmosStrings.OrphanedNestedDocument(
                    entry.EntityType.DisplayName(),
                    ownership.PrincipalEntityType.DisplayName()));
        }

        return principal.EntityType.IsDocumentRoot() ? principal : GetRootDocument(principal);
    }
#pragma warning restore EF1001 // Internal EF Core API usage.

    private DbUpdateException WrapUpdateException(Exception exception, IReadOnlyList<IUpdateEntry> entries)
    {
        var entry = entries[0];
        var documentSource = GetDocumentSource(entry.EntityType);
        var id = documentSource.GetId(entry.SharedIdentityEntry ?? entry);

        return exception switch
        {
            CosmosException { StatusCode: HttpStatusCode.PreconditionFailed }
                => new DbUpdateConcurrencyException(CosmosStrings.UpdateConflict(id), exception, entries),
            CosmosException { StatusCode: HttpStatusCode.Conflict }
                => new DbUpdateException(CosmosStrings.UpdateConflict(id), exception, entries),
            _ => new DbUpdateException(CosmosStrings.UpdateStoreException(id), exception, entries)
        };
    }

    void IResettableService.ResetState()
    {
        SessionTokenStorage.Clear();
    }

    Task IResettableService.ResetStateAsync(CancellationToken cancellationToken)
    {
        ((IResettableService)this).ResetState();
        return Task.CompletedTask;
    }

    private sealed class SaveGroups
    {
        public required IEnumerable<CosmosUpdateEntry> SingleUpdateEntries { get; init; }

        public required IEnumerable<(Grouping Key, List<CosmosUpdateEntry> UpdateEntries)> BatchableUpdateEntries { get; init; }
    }

    private sealed class CosmosUpdateEntry
    {
        public required IUpdateEntry Entry { get; init; }
        public required CosmosCudOperation Operation { get; init; }
        public required string CollectionId { get; init; }
        public required DocumentSource DocumentSource { get; init; }
        public required JObject? Document { get; init; }
    }

    private sealed record Grouping(string ContainerId, PartitionKey PartitionKeyValue);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override int SaveChanges(IList<IUpdateEntry> entries)
        => throw new InvalidOperationException(CosmosStrings.SyncNotSupported);
}
