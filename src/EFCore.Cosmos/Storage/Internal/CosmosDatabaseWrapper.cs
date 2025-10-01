// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
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
public class CosmosDatabaseWrapper : Database
{
    private readonly Dictionary<IEntityType, DocumentSource> _documentCollections = new();

    private readonly ICosmosClientWrapper _cosmosClient;
    private readonly bool _sensitiveLoggingEnabled;

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
        ILoggingOptions loggingOptions)
        : base(dependencies)
    {
        _currentDbContext = currentDbContext;
        _cosmosClient = cosmosClient;

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
    public override int SaveChanges(IList<IUpdateEntry> entries)
    {
        if (entries.Count == 0)
        {
            return 0;
        }

        var rowsAffected = 0;
        var groups = CreateSaveGroups(entries);

        foreach (var write in groups.SingleUpdateEntries)
        {
            if (Save(write))
            {
                rowsAffected++;
            }
        }

        foreach (var batch in groups.Batches)
        {
            var transaction = CreateTransaction(batch);

            try
            {
                var response = _cosmosClient.ExecuteBatch(transaction);
                if (!response.IsSuccess)
                {
                    var exception = WrapUpdateException(response.Exception, response.ErroredEntries);
                    if (exception is not DbUpdateConcurrencyException
                        || !Dependencies.Logger.OptimisticConcurrencyException(
                                batch.Items.First().Entry.Context, batch.Items.Select(x => x.Entry).ToArray(), (DbUpdateConcurrencyException)exception, null).IsSuppressed)
                    {
                        throw exception;
                    }
                }
            }
            catch (Exception ex) when (ex is not DbUpdateException and not OperationCanceledException)
            {
                var exception = WrapUpdateException(ex, batch.Items.Select(x => x.Entry).ToArray());
                throw exception;
            }

            rowsAffected += batch.Items.Count;
        }

        return rowsAffected;
    }

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

        foreach (var write in groups.SingleUpdateEntries)
        {
            if (await SaveAsync(write, cancellationToken).ConfigureAwait(false))
            {
                rowsAffected++;
            }
        }

        foreach (var batch in groups.Batches)
        {
            var transaction = CreateTransaction(batch);

            try
            {
                var response = await _cosmosClient.ExecuteBatchAsync(transaction, cancellationToken).ConfigureAwait(false);
                if (!response.IsSuccess)
                {
                    var exception = WrapUpdateException(response.Exception, response.ErroredEntries);
                    if (exception is not DbUpdateConcurrencyException
                        || !(await Dependencies.Logger.OptimisticConcurrencyExceptionAsync(
                                batch.Items.First().Entry.Context, batch.Items.Select(x => x.Entry).ToArray(), (DbUpdateConcurrencyException)exception, null, cancellationToken)
                            .ConfigureAwait(false)).IsSuppressed)
                    {
                        throw exception;
                    }
                }
            }
            catch (Exception ex) when (ex is not DbUpdateException and not OperationCanceledException)
            {
                var exception = WrapUpdateException(ex, batch.Items.Select(x => x.Entry).ToArray());
                throw exception;
            }

            rowsAffected += batch.Items.Count;
        }

        return rowsAffected;
    }

    private SaveGroups CreateSaveGroups(IList<IUpdateEntry> entries)
    {
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

        var cosmosUpdateEntries = rootEntriesToSave.Select(x => CreateCosmosUpdateEntry(x)!).Where(x => x != null);

        if (_currentDbContext.Context.Database.AutoTransactionBehavior == AutoTransactionBehavior.Never)
        {
            return new SaveGroups
            {
                Batches = [],
                SingleUpdateEntries = cosmosUpdateEntries
            };
        }

        var entriesWithTriggers = new List<CosmosUpdateEntry>();
        var entriesWithoutTriggers = new List<CosmosUpdateEntry>();
        foreach (var entry in cosmosUpdateEntries)
        {
            if (entry.Entry.EntityType.GetTriggers().Any())
            {
                entriesWithTriggers.Add(entry);
            }
            else
            {
                entriesWithoutTriggers.Add(entry);
            }
        }

        if (_currentDbContext.Context.Database.AutoTransactionBehavior == AutoTransactionBehavior.Always &&
            entriesWithTriggers.Count >= 1 && rootEntriesToSave.Count >= 2)
        {
            throw new InvalidOperationException(CosmosStrings.SaveChangesAutoTransactionBehaviorAlwaysTriggerAtomicity);
        }

        var batches = CreateBatches(entriesWithoutTriggers).ToArray();

        if (_currentDbContext.Context.Database.AutoTransactionBehavior == AutoTransactionBehavior.Always &&
            batches.Length > 1)
        {
            throw new InvalidOperationException(CosmosStrings.SaveChangesAutoTransactionBehaviorAlwaysAtomicity);
        }

        return new SaveGroups
        {
            Batches = batches,
            SingleUpdateEntries = entriesWithTriggers
        };
    }

    private IEnumerable<(Grouping Key, List<CosmosUpdateEntry> Items)> CreateBatches(List<CosmosUpdateEntry> entries)
    {
        const int maxOperationsPerBatch = 100;
        var buckets = new Dictionary<Grouping, List<CosmosUpdateEntry>>();

        foreach (var entry in entries)
        {
            var key = new Grouping(entry.CollectionId, _cosmosClient.GetPartitionKeyValue(entry.Entry));

            ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(buckets, key, out var exists);
            if (!exists || list is null)
            {
                list = new();
            }

            list.Add(entry);

            if (list.Count == maxOperationsPerBatch)
            {
                var listCopy = list;
                list = null;
                yield return (key, listCopy);
            }
        }

        foreach (var kvp in buckets)
        {
            if (kvp.Value != null)
            {
                yield return (kvp.Key, kvp.Value);
            }
        }
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
                        catch(InvalidOperationException ex)
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

    private ICosmosTransactionalBatchWrapper CreateTransaction((Grouping Key, List<CosmosUpdateEntry> Items) batch)
    {
        var transaction = _cosmosClient.CreateTransactionalBatch(batch.Key.ContainerId, batch.Key.PartitionKeyValue);

        foreach (var updateEntry in batch.Items)
        {
            switch (updateEntry.Operation)
            {
                case CosmosCudOperation.Create:
                    transaction.CreateItem(updateEntry.Document!, updateEntry.Entry);
                    break;
                case CosmosCudOperation.Update:
                    transaction.ReplaceItem(
                        updateEntry.DocumentSource.GetId(updateEntry.Entry.SharedIdentityEntry ?? updateEntry.Entry),
                        updateEntry.Document!,
                        updateEntry.Entry);
                    break;
                case CosmosCudOperation.Delete:
                    transaction.DeleteItem(updateEntry.DocumentSource.GetId(updateEntry.Entry), updateEntry.Entry);
                    break;
                default:
                    throw new UnreachableException();
            }
        }

        return transaction;
    }

    private bool Save(CosmosUpdateEntry updateEntry)
    {
        try
        {
            return updateEntry.Operation switch
            {
                CosmosCudOperation.Create => _cosmosClient.CreateItem(
                                    updateEntry.CollectionId, updateEntry.Document!, updateEntry.Entry),
                CosmosCudOperation.Update => _cosmosClient.ReplaceItem(
                                    updateEntry.CollectionId,
                                    updateEntry.DocumentSource.GetId(updateEntry.Entry.SharedIdentityEntry ?? updateEntry.Entry),
                                    updateEntry.Document!,
                                    updateEntry.Entry),
                CosmosCudOperation.Delete => _cosmosClient.DeleteItem(updateEntry.CollectionId, updateEntry.DocumentSource.GetId(updateEntry.Entry), updateEntry.Entry),
                _ => throw new UnreachableException(),
            };
        }
        catch (Exception ex) when (ex is not DbUpdateException and not UnreachableException and not OperationCanceledException)
        {
            var errorEntries = new[] { updateEntry.Entry };
            var exception = WrapUpdateException(ex, errorEntries);

            if (exception is not DbUpdateConcurrencyException
                || !Dependencies.Logger.OptimisticConcurrencyException(
                        updateEntry.Entry.Context, errorEntries, (DbUpdateConcurrencyException)exception, null).IsSuppressed)
            {
                throw exception;
            }

            return false;
        }
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
                                    cancellationToken).ConfigureAwait(false),
                CosmosCudOperation.Update => await _cosmosClient.ReplaceItemAsync(
                                    updateEntry.CollectionId,
                                    updateEntry.DocumentSource.GetId(updateEntry.Entry.SharedIdentityEntry ?? updateEntry.Entry),
                                    updateEntry.Document!,
                                    updateEntry.Entry,
                                    cancellationToken).ConfigureAwait(false),
                CosmosCudOperation.Delete => await _cosmosClient.DeleteItemAsync(
                                    updateEntry.CollectionId,
                                    updateEntry.DocumentSource.GetId(updateEntry.Entry),
                                    updateEntry.Entry,
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

    private sealed class SaveGroups
    {
        public required IEnumerable<CosmosUpdateEntry> SingleUpdateEntries { get; init; }

        public required IEnumerable<(Grouping Key, List<CosmosUpdateEntry> Items)> Batches { get; init; }
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
}
