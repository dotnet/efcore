// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
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
public class CosmosDatabaseWrapper : Database
{
    private readonly Dictionary<IEntityType, DocumentSource> _documentCollections = new();

    private readonly ICosmosClientWrapper _cosmosClient;
    private readonly bool _sensitiveLoggingEnabled;
    private readonly bool _throwOnCrossPartitionSaveChanges;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosDatabaseWrapper(
        DatabaseDependencies dependencies,
        IDbContextOptions dbContextOptions,
        ICosmosClientWrapper cosmosClient,
        ILoggingOptions loggingOptions)
        : base(dependencies)
    {
        _cosmosClient = cosmosClient;

        if (loggingOptions.IsSensitiveDataLoggingEnabled)
        {
            _sensitiveLoggingEnabled = true;
        }

        var options = dbContextOptions.FindExtension<CosmosOptionsExtension>()!;
        _throwOnCrossPartitionSaveChanges = options.ThrowOnCrossPartitionSaveChanges;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override int SaveChanges(IList<IUpdateEntry> entries)
    {
        var rowsAffected = 0;
        var batches = CreateBatches(entries);

        foreach (var batch in batches)
        {
            var transaction = GetTransaction(batch, out var transactionRows);
            rowsAffected += transactionRows;
            var response = _cosmosClient.ExecuteBatch(transaction);

            if (!response.IsSuccess)
            {
                var exception = CreateUpdateException(response);
                if (exception is not DbUpdateConcurrencyException
                    || !Dependencies.Logger.OptimisticConcurrencyException(
                            response.ErroredEntries!.First().Context, response.ErroredEntries!, (DbUpdateConcurrencyException)exception, null)
                       .IsSuppressed)
                {
                    throw exception;
                }
            }
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
        var batches = CreateBatches(entries);

        foreach (var batch in batches)
        {
            var transaction = GetTransaction(batch, out var transactionRows);
            rowsAffected += transactionRows;
            var response = await _cosmosClient.ExecuteBatchAsync(transaction, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccess)
            {
                var exception = CreateUpdateException(response);
                if (exception is not DbUpdateConcurrencyException
                    || !(await Dependencies.Logger.OptimisticConcurrencyExceptionAsync(
                            response.ErroredEntries!.First().Context, response.ErroredEntries!, (DbUpdateConcurrencyException)exception, null, cancellationToken)
                        .ConfigureAwait(false)).IsSuppressed)
                {
                    throw exception;
                }
            }
        }

        return rowsAffected;
    }

    private IGrouping<Grouping, RootEntryToSave>[] CreateBatches(IList<IUpdateEntry> entries)
    {
        var rootEntriesToSave = new HashSet<IUpdateEntry>(entries.Count);
        foreach (var entry in entries)
        {
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

        var batches = rootEntriesToSave
            .Select(x => new RootEntryToSave
            {
                Entry = x,
                DocumentSource = GetDocumentSource(x.EntityType)
            })
            .GroupBy(x => new Grouping
            {
                ContainerId = x.DocumentSource.GetContainerId(),
                PartitionKeyValue = _cosmosClient.GetPartitionKeyValue(x.Entry)
            }).ToArray();

        if (_throwOnCrossPartitionSaveChanges && batches.Length > 1)
        {
            throw new InvalidOperationException(CosmosStrings.CrossPartitionSaveChangesDisabled);
        }

        return batches;
    }

    private ICosmosTransactionalBatchWrapper GetTransaction(IGrouping<Grouping, RootEntryToSave> batch, out int transactionRows)
    {
        transactionRows = 0;

        var transaction = _cosmosClient.CreateTransactionalBatch(batch.Key.ContainerId, batch.Key.PartitionKeyValue);

        // ReSharper disable once ForCanBeConvertedToForeach
        foreach (var entry in batch)
        {
            if (AddToTransaction(transaction, entry))
            {
                transactionRows++;
            }
        }

        return transaction;
    }

    private class RootEntryToSave
    {
        public required IUpdateEntry Entry { get; init; }
        public required DocumentSource DocumentSource { get; init; }
    }

    private readonly struct Grouping
    {
        public required string ContainerId { get; init; }
        public required PartitionKey PartitionKeyValue { get; init; }
    }

    private bool AddToTransaction(ICosmosTransactionalBatchWrapper batch, RootEntryToSave rootEntry)
    {
        var entry = rootEntry.Entry;
        var entityType = entry.EntityType;
        var documentSource = rootEntry.DocumentSource;
        var state = entry.EntityState;

        if (entry.SharedIdentityEntry != null)
        {
            if (entry.EntityState == EntityState.Deleted)
            {
                return false;
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

                var newDocument = documentSource.GetCurrentDocument(entry);
                if (newDocument != null)
                {
                    documentSource.UpdateDocument(newDocument, entry);
                }
                else
                {
                    newDocument = documentSource.CreateDocument(entry);
                }

                batch.CreateItem(newDocument, entry);
                break;

            case EntityState.Modified:
                var document = documentSource.GetCurrentDocument(entry);
                if (document != null)
                {
                    if (documentSource.UpdateDocument(document, entry) == null)
                    {
                        return false;
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

                batch.ReplaceItem(
                    documentSource.GetId(entry.SharedIdentityEntry ?? entry),
                    document,
                    entry);
                break;

            case EntityState.Deleted:
                batch.DeleteItem(
                    documentSource.GetId(entry), entry);
                break;

            default:
                return false;
        }

        return true;
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

    private DbUpdateException CreateUpdateException(CosmosTransactionalBatchResult response)
    {
        var entry = response.ErroredEntries![0];
        var documentSource = GetDocumentSource(entry.EntityType);

        var id = documentSource.GetId(entry.SharedIdentityEntry ?? entry);

        return response.StatusCode switch
        {
            HttpStatusCode.PreconditionFailed => new DbUpdateConcurrencyException(CosmosStrings.UpdateConflict(id), null, response.ErroredEntries!),
            HttpStatusCode.Conflict => new DbUpdateException(CosmosStrings.UpdateConflict(id), null, response.ErroredEntries!),
            _ => new DbUpdateException(CosmosStrings.UpdateStoreException(id), null, response.ErroredEntries!)
        };
    }
}
