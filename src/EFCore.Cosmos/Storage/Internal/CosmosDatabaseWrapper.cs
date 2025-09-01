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
        var groups = CreateBatches(entries);

        foreach (var write in groups.SingleUpdateEntries)
        {
            Save(write);
            rowsAffected++;
        }

        foreach (var batch in groups.Batches)
        {
            var transaction = CreateTransaction(batch);
            var response = _cosmosClient.ExecuteBatch(transaction);

            if (!response.IsSuccess)
            {
                var exception = CreateUpdateException(response.StatusCode, response.ErroredEntries!);
                if (exception is not DbUpdateConcurrencyException
                    || !Dependencies.Logger.OptimisticConcurrencyException(
                            response.ErroredEntries!.First().Context, response.ErroredEntries!, (DbUpdateConcurrencyException)exception, null).IsSuppressed)
                {
                    throw exception;
                }
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
        var groups = CreateBatches(entries);

        foreach (var write in groups.SingleUpdateEntries)
        {
            await SaveAsync(write, cancellationToken).ConfigureAwait(false);
            rowsAffected++;
        }

        foreach (var batch in groups.Batches)
        {
            var transaction = CreateTransaction(batch);
            var response = await _cosmosClient.ExecuteBatchAsync(transaction, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccess)
            {
                var exception = CreateUpdateException(response.StatusCode, response.ErroredEntries!);
                if (exception is not DbUpdateConcurrencyException
                    || !(await Dependencies.Logger.OptimisticConcurrencyExceptionAsync(
                            response.ErroredEntries!.First().Context, response.ErroredEntries!, (DbUpdateConcurrencyException)exception, null, cancellationToken)
                        .ConfigureAwait(false)).IsSuppressed)
                {
                    throw exception;
                }
            }

            rowsAffected += batch.Items.Count;
        }

        return rowsAffected;
    }

    private ICosmosTransactionalBatchWrapper CreateTransaction((Grouping Key, List<PreppedWrite> Items) batch)
    {
        var transaction = _cosmosClient.CreateTransactionalBatch(batch.Key.ContainerId, batch.Key.PartitionKeyValue);

        foreach (var write in batch.Items)
        {
            switch (write.State)
            {
                case EntityState.Added:
                    transaction.CreateItem(write.Document!, write.Entry);
                    break;
                case EntityState.Modified:
                    transaction.ReplaceItem(
                        write.DocumentSource.GetId(write.Entry.SharedIdentityEntry ?? write.Entry),
                        write.Document!,
                        write.Entry);
                    break;
                case EntityState.Deleted:
                    transaction.DeleteItem(write.DocumentSource.GetId(write.Entry), write.Entry);
                    break;
                default:
                    throw new UnreachableException();
            }
        }

        return transaction;
    }

    private SaveGroups CreateBatches(IList<IUpdateEntry> entries)
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

        var writes = rootEntriesToSave.Select(x => PrepareWrite(x)).Where(x => x != null).Cast<PreppedWrite>().ToArray();

        if (_currentDbContext.Context.Database.AutoTransactionBehavior == AutoTransactionBehavior.Never)
        {
            return new SaveGroups
            {
                Batches = [],
                SingleUpdateEntries = writes
            };
        }

        var writesWithTriggers = new List<PreppedWrite>();
        var writesWithoutTriggers = new List<PreppedWrite>();
        foreach (var write in writes)
        {
            if (write.Entry.EntityType.GetTriggers().Any())
            {
                writesWithTriggers.Add(write);
            }
            else
            {
                writesWithoutTriggers.Add(write);
            }
        }

        if (_currentDbContext.Context.Database.AutoTransactionBehavior == AutoTransactionBehavior.Always && writesWithTriggers.Count >= 1 && rootEntriesToSave.Count >= 2)
        {
            // @TODO: string
            throw new InvalidOperationException("Multiple entities with triggers cannot be saved atomically.");
        }

        const int maxOperationsPerBatch = 100;
        var batches =
            GroupByWithMaxSize(
                writesWithoutTriggers,
                x => new Grouping
                {
                    ContainerId = x.CollectionId,
                    PartitionKeyValue = _cosmosClient.GetPartitionKeyValue(x.Entry)
                },
                maxOperationsPerBatch)
            .ToArray(); // @TODO optimize memory usage? cleanup PrepareWrite?

        if (_currentDbContext.Context.Database.AutoTransactionBehavior == AutoTransactionBehavior.Always && batches.Length > 1)
        {
            // @TODO: string
            throw new InvalidOperationException(""); // CosmosStrings.CrossPartitionSaveChangesDisabled
        }

        return new SaveGroups
        {
            Batches = batches,
            SingleUpdateEntries = writesWithTriggers
        };
    }

    // @TODO: Cleanup
    private sealed class SaveGroups
    {
        public required IEnumerable<PreppedWrite> SingleUpdateEntries { get; init; }

        public required IEnumerable<(Grouping Key, List<PreppedWrite> Items)> Batches { get; init; }
    }

    private readonly struct Grouping
    {
        public required string ContainerId { get; init; }
        public required PartitionKey PartitionKeyValue { get; init; }
    }

    private static IEnumerable<(TKey Key, List<T> Items)> GroupByWithMaxSize<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector, int maxGroupSize = 100)
        where TKey : notnull
    {
        var buckets = new Dictionary<TKey, List<T>>();

        foreach (var item in source)
        {
            var key = keySelector(item);

            ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(buckets, key, out var exists);
            if (!exists || list is null)
            {
                list = new List<T>(5);
            }

            list.Add(item);

            if (list.Count == maxGroupSize)
            {
                var listCopy = list;
                list = null;
                yield return (key, listCopy);
            }
        }

        foreach (var kvp in buckets)
        {
            if (kvp.Value.Count > 0)
            {
                yield return (kvp.Key, kvp.Value);
            }
        }
    }

    private PreppedWrite? PrepareWrite(IUpdateEntry entry)
    {
        var entityType = entry.EntityType;
        var documentSource = GetDocumentSource(entityType);
        var collectionId = documentSource.GetContainerId();
        var state = entry.EntityState;
        JObject? document = null;

        if (entry.SharedIdentityEntry != null)
        {
            if (entry.EntityState == EntityState.Deleted)
            {
                return null;
            }

            if (state == EntityState.Added)
            {
                state = EntityState.Modified;
            }
        }

        switch (state)
        {
            case EntityState.Added:
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

            case EntityState.Modified:
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

            case EntityState.Deleted:
                break;

            default:
                return null;
        }

        return new PreppedWrite
        {
            CollectionId = collectionId,
            Document = document,
            DocumentSource = documentSource,
            Entry = entry,
            State = state
        };
    }

    private sealed class PreppedWrite
    {
        public required IUpdateEntry Entry { get; init; }
        public required EntityState State { get; init; }
        public required string CollectionId { get; init; }
        public required DocumentSource DocumentSource { get; init; }

        public required JObject? Document { get; init; }
    }

    private bool Save(PreppedWrite preppedWrite)
        => preppedWrite.State switch
    {
        EntityState.Added => _cosmosClient.CreateItem(
                            preppedWrite.CollectionId, preppedWrite.Document!, preppedWrite.Entry),
        EntityState.Modified => _cosmosClient.ReplaceItem(
                            preppedWrite.CollectionId,
                            preppedWrite.DocumentSource.GetId(preppedWrite.Entry.SharedIdentityEntry ?? preppedWrite.Entry),
                            preppedWrite.Document!,
                            preppedWrite.Entry),
        EntityState.Deleted => _cosmosClient.DeleteItem(preppedWrite.CollectionId, preppedWrite.DocumentSource.GetId(preppedWrite.Entry), preppedWrite.Entry),
        _ => throw new UnreachableException(),
    };

    private Task<bool> SaveAsync(PreppedWrite preppedWrite, CancellationToken cancellationToken)
        => preppedWrite.State switch
    {
        EntityState.Added => _cosmosClient.CreateItemAsync(
                            preppedWrite.CollectionId, preppedWrite.Document!, preppedWrite.Entry, cancellationToken),
        EntityState.Modified => _cosmosClient.ReplaceItemAsync(
                            preppedWrite.CollectionId,
                            preppedWrite.DocumentSource.GetId(preppedWrite.Entry.SharedIdentityEntry ?? preppedWrite.Entry),
                            preppedWrite.Document!,
                            preppedWrite.Entry,
                            cancellationToken),
        EntityState.Deleted => _cosmosClient.DeleteItemAsync(preppedWrite.CollectionId, preppedWrite.DocumentSource.GetId(preppedWrite.Entry), preppedWrite.Entry, cancellationToken),
        _ => throw new UnreachableException(),
    };

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

    private DbUpdateException CreateUpdateException(HttpStatusCode statusCode, IReadOnlyList<IUpdateEntry> updateEntries)
    {
        var entry = updateEntries[0];
        var documentSource = GetDocumentSource(entry.EntityType);
        var id = documentSource.GetId(entry.SharedIdentityEntry ?? entry);

        return statusCode switch
        {
            HttpStatusCode.PreconditionFailed => new DbUpdateConcurrencyException(CosmosStrings.UpdateConflict(id), null, updateEntries),
            HttpStatusCode.Conflict => new DbUpdateException(CosmosStrings.UpdateConflict(id), null, updateEntries),
            _ => new DbUpdateException(CosmosStrings.UpdateStoreException(id), null, updateEntries)
        };
    }
}
