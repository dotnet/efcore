// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Azure.Cosmos.Scripts;
using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosClientWrapper : ICosmosClientWrapper
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly JsonWriterOptions JsonWriterOptions = new() { Indented = false, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly string DefaultPartitionKey = "__partitionKey";

    private const string SubStatusCodeHeaderName = "x-ms-substatus";

    private readonly ISingletonCosmosClientWrapper _singletonWrapper;
    private readonly string _databaseId;
    private readonly IExecutionStrategy _executionStrategy;
    private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> _commandLogger;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosClientWrapper(
        ISingletonCosmosClientWrapper singletonWrapper,
        IDbContextOptions dbContextOptions,
        IExecutionStrategy executionStrategy,
        IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger)
    {
        var options = dbContextOptions.FindExtension<CosmosOptionsExtension>();

        _singletonWrapper = singletonWrapper;
        _databaseId = options!.DatabaseName;
        _executionStrategy = executionStrategy;
        _commandLogger = commandLogger;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private CosmosClient Client
        => _singletonWrapper.Client;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<bool> CreateDatabaseIfNotExistsAsync(
        ThroughputProperties? throughput,
        CancellationToken cancellationToken = default)
        => _executionStrategy.ExecuteAsync(
            (throughput, this), CreateDatabaseIfNotExistsOnceAsync, null, cancellationToken);

    private static async Task<bool> CreateDatabaseIfNotExistsOnceAsync(
        DbContext? _,
        (ThroughputProperties? Throughput, CosmosClientWrapper Wrapper) parameters,
        CancellationToken cancellationToken = default)
    {
        var (throughput, wrapper) = parameters;
        var response = await wrapper.Client.CreateDatabaseIfNotExistsAsync(
                wrapper._databaseId, throughput, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return response.StatusCode == HttpStatusCode.Created;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<bool> DeleteDatabaseAsync(
        CancellationToken cancellationToken = default)
        => _executionStrategy.ExecuteAsync(this, DeleteDatabaseOnceAsync, null, cancellationToken);

    private static async Task<bool> DeleteDatabaseOnceAsync(
        DbContext? _,
        CosmosClientWrapper wrapper,
        CancellationToken cancellationToken = default)
    {
        using var response = await wrapper.Client.GetDatabase(wrapper._databaseId)
            .DeleteStreamAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return response.StatusCode == HttpStatusCode.NoContent;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<bool> CreateContainerIfNotExistsAsync(
        ContainerProperties properties,
        CancellationToken cancellationToken = default)
        => _executionStrategy.ExecuteAsync((properties, this), CreateContainerIfNotExistsOnceAsync, null, cancellationToken);

    private static async Task<bool> CreateContainerIfNotExistsOnceAsync(
        DbContext _,
        (ContainerProperties Parameters, CosmosClientWrapper Wrapper) parametersTuple,
        CancellationToken cancellationToken = default)
    {
        var (parameters, wrapper) = parametersTuple;
        var partitionKeyPaths = parameters.PartitionKeyStoreNames.Select(e => "/" + e).ToList();

        var vectorIndexes = new Collection<VectorIndexPath>();
        var fullTextIndexPaths = new Collection<FullTextIndexPath>();
        var includedPaths = new Collection<IncludedPath>();
        var conventionIncludedPaths = new Collection<IncludedPath>();
        var compositeIndexes = new Collection<Collection<CompositePath>>();
        var seenIncludedPaths = new HashSet<string>();
        var seenCompositeIndexes = new HashSet<string>();
        foreach (var index in parameters.Indexes)
        {
            var vectorIndexType = index.GetVectorIndexType();
            if (vectorIndexType != null)
            {
                if (index.Properties.Count > 1)
                {
                    throw new InvalidOperationException(
                        CosmosStrings.CompositeVectorIndex(
                            index.DeclaringEntityType.DisplayName(),
                            string.Join(",", index.Properties.Select(e => e.Name))));
                }

                vectorIndexes.Add(
                    new VectorIndexPath { Path = GetJsonPropertyPathFromRoot(index.Properties[0]), Type = vectorIndexType.Value });
                continue;
            }

            if (index.IsFullTextIndex() == true)
            {
                if (index.Properties.Count > 1)
                {
                    throw new InvalidOperationException(
                        CosmosStrings.CompositeFullTextIndex(
                            index.DeclaringEntityType.DisplayName(),
                            string.Join(",", index.Properties.Select(e => e.Name))));
                }

                fullTextIndexPaths.Add(
                    new FullTextIndexPath { Path = GetJsonPropertyPathFromRoot(index.Properties[0]) });
                continue;
            }

            var isConvention = index is IConventionIndex convIndex
                && convIndex.GetConfigurationSource() == ConfigurationSource.Convention;

            if (index.Properties.Count == 1)
            {
                var path = GetJsonPropertyPathFromRoot(index.Properties[0]) + "/?";
                if (seenIncludedPaths.Add(path))
                {
                    (isConvention ? conventionIncludedPaths : includedPaths)
                        .Add(new IncludedPath { Path = path });
                }
            }
            else
            {
                // Composite indexes are additive: they coexist with automatic indexing and don't affect /* emission.
                var compositePaths = new Collection<CompositePath>();
                var key = new StringBuilder();
                for (var i = 0; i < index.Properties.Count; i++)
                {
                    var path = GetJsonPropertyPathFromRoot(index.Properties[i]);
                    var order = index.IsDescending?[i] == true
                        ? CompositePathSortOrder.Descending
                        : CompositePathSortOrder.Ascending;
                    compositePaths.Add(new CompositePath { Path = path, Order = order });
                    key.Append(path).Append('=').Append(order).Append('|');
                }

                if (seenCompositeIndexes.Add(key.ToString()))
                {
                    compositeIndexes.Add(compositePaths);
                }
            }
        }

        var fullTextPaths = new Collection<FullTextPath>();
        foreach (var (property, language) in parameters.FullTextProperties)
        {
            if (property.ClrType != typeof(string))
            {
                throw new InvalidOperationException(
                    CosmosStrings.FullTextSearchConfiguredForUnsupportedPropertyType(
                        property.DeclaringType.DisplayName(),
                        property.Name,
                        property.ClrType.Name));
            }

            fullTextPaths.Add(
                new FullTextPath
                {
                    Path = GetJsonPropertyPathFromRoot(property),
                    // TODO: remove the fallback once Cosmos SDK allows optional language (see #35939)
                    Language = language ?? parameters.DefaultFullTextLanguage ?? "en-US"
                });
        }

        var embeddings = new Collection<Embedding>();
        foreach (var (property, vectorType) in parameters.Vectors)
        {
            embeddings.Add(
                new Embedding
                {
                    Path = GetJsonPropertyPathFromRoot(property),
                    DataType = CosmosVectorType.CreateDefaultVectorDataType(property.ClrType),
                    Dimensions = vectorType.Dimensions,
                    DistanceFunction = vectorType.DistanceFunction
                });
        }

        var containerProperties = new Azure.Cosmos.ContainerProperties(parameters.Id, partitionKeyPaths)
        {
            PartitionKeyDefinitionVersion = PartitionKeyDefinitionVersion.V2,
            DefaultTimeToLive = parameters.DefaultTimeToLive,
            AnalyticalStoreTimeToLiveInSeconds = parameters.AnalyticalStoreTimeToLiveInSeconds,
        };

        if (embeddings.Count != 0)
        {
            containerProperties.VectorEmbeddingPolicy = new VectorEmbeddingPolicy(embeddings);
        }

        // Tri-state interpretation for automatic indexing:
        //   - true                → explicit opt-in: emit /* + exceptions; explicit single-prop included paths are
        //                           redundant and skipped.
        //   - false               → explicit opt-out: don't emit /*; only the explicit included paths are indexed.
        //   - null (unconfigured) → default to enabled, but if any explicit single-property indexes were declared,
        //                           treat as disabled so those declarations don't get silently overridden by /*.
        // Convention single-property indexes (e.g. FK indexes) don't influence the heuristic, but are emitted as
        // included paths whenever automatic indexing is off.
        // Composite, vector and full-text indexes are always emitted.
        // TODO: Calculate this using a convention when #15898 is implemented.
        var automaticIndexingEnabled = parameters.AutomaticIndexingEnabled
            ?? includedPaths.Count == 0;

        if (vectorIndexes.Count != 0
            || fullTextIndexPaths.Count != 0
            || compositeIndexes.Count != 0
            || includedPaths.Count != 0
            || !automaticIndexingEnabled
            || parameters.AutomaticIndexingExceptions is not null)
        {
            var indexingPolicy = new IndexingPolicy
            {
                VectorIndexes = vectorIndexes,
                FullTextIndexes = fullTextIndexPaths
            };

            if (automaticIndexingEnabled)
            {
                indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/*" });
                if (parameters.AutomaticIndexingExceptions is not null)
                {
                    foreach (var path in parameters.AutomaticIndexingExceptions)
                    {
                        indexingPolicy.ExcludedPaths.Add(new ExcludedPath { Path = path });
                    }
                }
            }
            else
            {
                foreach (var includedPath in includedPaths)
                {
                    indexingPolicy.IncludedPaths.Add(includedPath);
                }

                foreach (var includedPath in conventionIncludedPaths)
                {
                    indexingPolicy.IncludedPaths.Add(includedPath);
                }

                // Cosmos requires the mandatory "/" path to be covered by either includedPaths or excludedPaths.
                // With automatic indexing disabled, exclude "/*" so only the explicit paths above are indexed.
                indexingPolicy.ExcludedPaths.Add(new ExcludedPath { Path = "/*" });
            }

            foreach (var compositeIndex in compositeIndexes)
            {
                indexingPolicy.CompositeIndexes.Add(compositeIndex);
            }

            containerProperties.IndexingPolicy = indexingPolicy;
        }

        if (fullTextPaths.Count != 0)
        {
            containerProperties.FullTextPolicy = new FullTextPolicy
            {
                DefaultLanguage = parameters.DefaultFullTextLanguage, FullTextPaths = fullTextPaths
            };
        }

        var response = await wrapper.Client.GetDatabase(wrapper._databaseId).CreateContainerIfNotExistsAsync(
                containerProperties,
                throughput: parameters.Throughput?.Throughput,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return response.StatusCode == HttpStatusCode.Created;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static string GetJsonPropertyPathFromRoot(IReadOnlyPropertyBase property)
    {
        var builder = new StringBuilder();
        AppendTypePathFromRoot(builder, property.DeclaringType);
        AppendPropertyBaseSegment(builder, property);
        return builder.ToString();
    }

    private static void AppendPropertyBaseSegment(StringBuilder builder, IReadOnlyPropertyBase property)
    {
        switch (property)
        {
            case IReadOnlyProperty scalar:
                builder.Append('/');
                AppendEscapedPathSegment(builder, scalar.GetJsonPropertyName());
                break;
            case IReadOnlyComplexProperty complexProperty:
                AppendComplexPropertySegment(builder, complexProperty);
                break;
            default:
                throw new UnreachableException(
                    $"Unsupported property base '{property.GetType().ShortDisplayName()}' on '{property.DeclaringType.DisplayName()}.{property.Name}'.");
        }
    }

    private static void AppendTypePathFromRoot(StringBuilder builder, IReadOnlyTypeBase declaringType)
    {
        switch (declaringType)
        {
            case IReadOnlyComplexType complexType:
            {
                var complexProperty = complexType.ComplexProperty;
                AppendTypePathFromRoot(builder, complexProperty.DeclaringType);
                AppendComplexPropertySegment(builder, complexProperty);
                break;
            }
            case IReadOnlyEntityType entityType when entityType.IsOwned():
            {
                var ownership = entityType.FindOwnership()!;
                var containingPropertyName = ownership.GetNavigation(pointsToPrincipal: false)!
                    .TargetEntityType.GetContainingPropertyName()
                    ?? throw new UnreachableException("Containing property name should not be null for owned entity types.");

                AppendTypePathFromRoot(builder, ownership.PrincipalEntityType);
                builder.Append('/');
                AppendEscapedPathSegment(builder, containingPropertyName);

                if (!ownership.IsUnique)
                {
                    throw new NotSupportedException(
                        CosmosStrings.CreatingContainerWithFullTextOrVectorOnCollectionNotSupported(builder.ToString()));
                }

                break;
            }
        }
    }

    private static void AppendComplexPropertySegment(StringBuilder builder, IReadOnlyComplexProperty complexProperty)
    {
        builder.Append('/');
        AppendEscapedPathSegment(builder, complexProperty.GetJsonPropertyName());
        if (complexProperty.IsCollection)
        {
            builder.Append("/[]");
        }
    }

    private static void AppendEscapedPathSegment(StringBuilder builder, string segment)
    {
        // Cosmos indexing-policy paths support identifier-like segments unquoted; anything else must be wrapped
        // in double-quotes with embedded '"' and '\' escaped.
        // See https://learn.microsoft.com/azure/cosmos-db/index-policy#path-formatting.
        if (segment.Length == 0 || char.IsDigit(segment[0]) || !segment.All(static c => char.IsLetterOrDigit(c) || c == '_'))
        {
            builder.Append('"');
            foreach (var c in segment)
            {
                if (c is '\\' or '"')
                {
                    builder.Append('\\');
                }

                builder.Append(c);
            }

            builder.Append('"');
        }
        else
        {
            builder.Append(segment);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<bool> CreateItemAsync(
        string containerId,
        string documentId,
        ReadOnlyMemory<byte> document,
        IUpdateEntry updateEntry,
        ISessionTokenStorage sessionTokenStorage,
        CancellationToken cancellationToken = default)
        => _executionStrategy.ExecuteAsync((containerId, documentId, document, updateEntry, sessionTokenStorage, this), CreateItemOnceAsync, null, cancellationToken);

    private static async Task<bool> CreateItemOnceAsync(
        DbContext _,
        (string ContainerId, string DocumentId, ReadOnlyMemory<byte> Document, IUpdateEntry Entry, ISessionTokenStorage SessionTokenStorage, CosmosClientWrapper Wrapper) parameters,
        CancellationToken cancellationToken = default)
    {
        var containerId = parameters.ContainerId;
        var documentId = parameters.DocumentId;
        var entry = parameters.Entry;
        var wrapper = parameters.Wrapper;
        var sessionTokenStorage = parameters.SessionTokenStorage;
        var container = wrapper.Client.GetDatabase(wrapper._databaseId).GetContainer(parameters.ContainerId);
        var itemRequestOptions = CreateItemRequestOptions(entry, sessionTokenStorage.GetSessionToken(containerId));
        var partitionKeyValue = ExtractPartitionKeyValue(entry);
        var preTriggers = GetTriggers(entry, TriggerType.Pre, TriggerOperation.Create);
        var postTriggers = GetTriggers(entry, TriggerType.Post, TriggerOperation.Create);
        if (preTriggers != null || postTriggers != null)
        {
            if (preTriggers != null)
            {
                itemRequestOptions.PreTriggers = preTriggers;
            }

            if (postTriggers != null)
            {
                itemRequestOptions.PostTriggers = postTriggers;
            }
        }

        if (!MemoryMarshal.TryGetArray(parameters.Document, out var segment) || segment.Array == null)
        {
            throw new UnreachableException("ReadOnlyMemory should have an underlying array.");
        }

        using var stream = new MemoryStream(segment.Array, segment.Offset, segment.Count);
        using var response = await container.CreateItemStreamAsync(
                stream,
                partitionKeyValue,
                itemRequestOptions,
                cancellationToken)
            .ConfigureAwait(false);

        wrapper._commandLogger.ExecutedCreateItem(
            response.Diagnostics.GetClientElapsedTime(),
            response.Headers.RequestCharge,
            response.Headers.ActivityId,
            documentId,
            containerId,
            partitionKeyValue);

        ProcessWriteResponse(containerId, response, entry, sessionTokenStorage);

        return response.StatusCode == HttpStatusCode.Created;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<bool> ReplaceItemAsync(
        string collectionId,
        string documentId,
        ReadOnlyMemory<byte> document,
        IUpdateEntry updateEntry,
        ISessionTokenStorage sessionTokenStorage,
        CancellationToken cancellationToken = default)
        => _executionStrategy.ExecuteAsync(
            (collectionId, documentId, document, updateEntry, sessionTokenStorage, this), ReplaceItemOnceAsync, null, cancellationToken);

    private static async Task<bool> ReplaceItemOnceAsync(
        DbContext _,
        (string ContainerId, string ResourceId, ReadOnlyMemory<byte> Document, IUpdateEntry Entry, ISessionTokenStorage SessionTokenStorage, CosmosClientWrapper Wrapper) parameters,
        CancellationToken cancellationToken = default)
    {
        var containerId = parameters.ContainerId;
        var entry = parameters.Entry;
        var wrapper = parameters.Wrapper;
        var sessionTokenStorage = parameters.SessionTokenStorage;
        var container = wrapper.Client.GetDatabase(wrapper._databaseId).GetContainer(parameters.ContainerId);
        var itemRequestOptions = CreateItemRequestOptions(entry, sessionTokenStorage.GetSessionToken(containerId));
        var partitionKeyValue = ExtractPartitionKeyValue(entry);
        var preTriggers = GetTriggers(entry, TriggerType.Pre, TriggerOperation.Replace);
        var postTriggers = GetTriggers(entry, TriggerType.Post, TriggerOperation.Replace);
        if (preTriggers != null || postTriggers != null)
        {
            if (preTriggers != null)
            {
                itemRequestOptions.PreTriggers = preTriggers;
            }

            if (postTriggers != null)
            {
                itemRequestOptions.PostTriggers = postTriggers;
            }
        }

        if (!MemoryMarshal.TryGetArray(parameters.Document, out var segment) || segment.Array == null)
        {
            throw new UnreachableException("ReadOnlyMemory should have an underlying array.");
        }

        using var stream = new MemoryStream(segment.Array, segment.Offset, segment.Count);
        using var response = await container.ReplaceItemStreamAsync(
                stream,
                parameters.ResourceId,
                partitionKeyValue,
                itemRequestOptions,
                cancellationToken)
            .ConfigureAwait(false);

        wrapper._commandLogger.ExecutedReplaceItem(
            response.Diagnostics.GetClientElapsedTime(),
            response.Headers.RequestCharge,
            response.Headers.ActivityId,
            parameters.ResourceId,
            containerId,
            partitionKeyValue);

        ProcessWriteResponse(containerId, response, entry, sessionTokenStorage);

        return response.StatusCode == HttpStatusCode.OK;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<bool> DeleteItemAsync(
        string containerId,
        string documentId,
        IUpdateEntry entry,
        ISessionTokenStorage sessionTokenStorage,
        CancellationToken cancellationToken = default)
        => _executionStrategy.ExecuteAsync((containerId, documentId, entry, sessionTokenStorage, this), DeleteItemOnceAsync, null, cancellationToken);

    private static async Task<bool> DeleteItemOnceAsync(
        DbContext? _,
        (string ContainerId, string ResourceId, IUpdateEntry Entry, ISessionTokenStorage SessionTokenStorage, CosmosClientWrapper Wrapper) parameters,
        CancellationToken cancellationToken = default)
    {
        var containerId = parameters.ContainerId;
        var entry = parameters.Entry;
        var wrapper = parameters.Wrapper;
        var sessionTokenStorage = parameters.SessionTokenStorage;
        var items = wrapper.Client.GetDatabase(wrapper._databaseId).GetContainer(parameters.ContainerId);

        var itemRequestOptions = CreateItemRequestOptions(entry, sessionTokenStorage.GetSessionToken(containerId));
        var partitionKeyValue = ExtractPartitionKeyValue(entry);
        var preTriggers = GetTriggers(entry, TriggerType.Pre, TriggerOperation.Delete);
        var postTriggers = GetTriggers(entry, TriggerType.Post, TriggerOperation.Delete);
        if (preTriggers != null || postTriggers != null)
        {
            if (preTriggers != null)
            {
                itemRequestOptions.PreTriggers = preTriggers;
            }

            if (postTriggers != null)
            {
                itemRequestOptions.PostTriggers = postTriggers;
            }
        }

        using var response = await items.DeleteItemStreamAsync(
                parameters.ResourceId,
                partitionKeyValue,
                itemRequestOptions,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        wrapper._commandLogger.ExecutedDeleteItem(
            response.Diagnostics.GetClientElapsedTime(),
            response.Headers.RequestCharge,
            response.Headers.ActivityId,
            parameters.ResourceId,
            containerId,
            partitionKeyValue);

        ProcessWriteResponse(containerId, response, entry, sessionTokenStorage);

        return response.StatusCode == HttpStatusCode.NoContent;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual PartitionKey GetPartitionKeyValue(IUpdateEntry updateEntry)
        => ExtractPartitionKeyValue(updateEntry);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ICosmosTransactionalBatchWrapper CreateTransactionalBatch(string containerId, PartitionKey partitionKeyValue, bool checkSize)
    {
        var container = Client.GetDatabase(_databaseId).GetContainer(containerId);

        var batch = container.CreateTransactionalBatch(partitionKeyValue);

        return new CosmosTransactionalBatchWrapper(batch, containerId, partitionKeyValue, checkSize);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<CosmosTransactionalBatchResult> ExecuteTransactionalBatchAsync(ICosmosTransactionalBatchWrapper batch, ISessionTokenStorage sessionTokenStorage, CancellationToken cancellationToken = default)
        => _executionStrategy.ExecuteAsync((batch, sessionTokenStorage, this), ExecuteTransactionalBatchOnceAsync, null, cancellationToken);

    private static async Task<CosmosTransactionalBatchResult> ExecuteTransactionalBatchOnceAsync(DbContext _,
        (ICosmosTransactionalBatchWrapper Batch, ISessionTokenStorage SessionTokenStorage, CosmosClientWrapper Wrapper) parameters,
        CancellationToken cancellationToken = default)
    {
        var batch = parameters.Batch;
        var transactionalBatch = batch.GetTransactionalBatch();
        var wrapper = parameters.Wrapper;
        var sessionTokenStorage = parameters.SessionTokenStorage;

        var options = new TransactionalBatchRequestOptions
        {
            SessionToken = sessionTokenStorage.GetSessionToken(batch.CollectionId)
        };

        using var response = await transactionalBatch.ExecuteAsync(options, cancellationToken).ConfigureAwait(false);

        wrapper._commandLogger.ExecutedTransactionalBatch(
            response.Diagnostics.GetClientElapsedTime(),
            response.Headers.RequestCharge,
            response.Headers.ActivityId,
            batch.CollectionId,
            batch.PartitionKeyValue,
            "[ \"" + string.Join("\", \"", batch.Entries.Select(x => x.Id)) + "\" ]");

        return ProcessBatchResponse(batch.CollectionId, response, batch.Entries, sessionTokenStorage);
    }

    private static ItemRequestOptions CreateItemRequestOptions(IUpdateEntry entry, string? sessionToken)
    {
        var helper = RequestOptionsHelper.Create(entry);

        var itemRequestOptions = new ItemRequestOptions
        {
            SessionToken = sessionToken
        };

        if (helper != null)
        {
            itemRequestOptions.IfMatchEtag = helper.IfMatchEtag;
        }

        return itemRequestOptions;
    }

    private static IReadOnlyList<string>? GetTriggers(IUpdateEntry entry, TriggerType type, TriggerOperation operation)
    {
        var preTriggers = entry.EntityType.GetTriggers()
            .Where(t => t.GetTriggerType() == type && ShouldExecuteTrigger(t, operation))
            .Select(t => t.ModelName)
            .ToList();

        return preTriggers.Count > 0 ? preTriggers : null;
    }

    private static bool ShouldExecuteTrigger(ITrigger trigger, TriggerOperation currentOperation)
    {
        var triggerOperation = trigger.GetTriggerOperation();
        return triggerOperation == null || triggerOperation == TriggerOperation.All || triggerOperation == currentOperation;
    }

    private static PartitionKey ExtractPartitionKeyValue(IUpdateEntry entry)
    {
        var partitionKeyProperties = entry.EntityType.GetPartitionKeyProperties();
        if (!partitionKeyProperties.Any())
        {
            return PartitionKey.None;
        }

        var builder = new PartitionKeyBuilder();
        foreach (var property in partitionKeyProperties)
        {
            builder.Add(entry.GetCurrentValue(property), property);
        }

        return builder.Build();
    }

    private static void ProcessWriteResponse(string containerId, ResponseMessage response, IUpdateEntry entry, ISessionTokenStorage sessionTokenStorage)
    {
        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (CosmosException)
        {
            TryTrackSessionTokenFromFailure(containerId, response.StatusCode, response.Headers, sessionTokenStorage);
            throw;
        }

        sessionTokenStorage.TrackSessionToken(containerId, response.Headers.Session);

        ProcessWriteResponse(entry, response.Headers.ETag, response.Content);
    }

    private static void TryTrackSessionTokenFromFailure(string containerId, HttpStatusCode statusCode, Headers headers, ISessionTokenStorage sessionTokenStorage)
    {
        // Some failures indicate document changes on the server that should be reflected in the session token to avoid subsequent stale reads.
        const string readSessionNotAvailableSubStatusCode = "1002";
        if (statusCode == HttpStatusCode.Conflict || statusCode == HttpStatusCode.PreconditionFailed ||
            (statusCode == HttpStatusCode.NotFound && (!headers.TryGetValue(SubStatusCodeHeaderName, out var subStatusCode) || subStatusCode != readSessionNotAvailableSubStatusCode)))
        {
            sessionTokenStorage.TrackSessionToken(containerId, headers.Session);
        }
    }

    private static CosmosTransactionalBatchResult ProcessBatchResponse(string containerId, TransactionalBatchResponse response, IReadOnlyList<CosmosTransactionalBatchEntry> entries, ISessionTokenStorage sessionTokenStorage)
    {
        if (!response.IsSuccessStatusCode)
        {
            TryTrackSessionTokenFromFailure(containerId, response.StatusCode, response.Headers, sessionTokenStorage);

            var errorCode = response.StatusCode;
            var errorEntries = response
                .Select((opResult, index) => (opResult, index))
                .Where(r => r.opResult.StatusCode == errorCode)
                .Select(r => entries[r.index].Entry)
                .ToList();

            var exception = new CosmosException(response.ErrorMessage, errorCode, 0, response.ActivityId, response.RequestCharge);
            return new CosmosTransactionalBatchResult(errorEntries, exception);
        }

        sessionTokenStorage.TrackSessionToken(containerId, response.Headers.Session);

        for (var i = 0; i < response.Count; i++)
        {
            var entry = entries[i];
            var item = response[i];

            ProcessWriteResponse(entry.Entry, item.ETag, item.ResourceStream);
        }

        return CosmosTransactionalBatchResult.Success;
    }

    private static void ProcessWriteResponse(IUpdateEntry entry, string eTag, Stream? content)
    {
        if (entry.EntityState == EntityState.Deleted)
        {
            return;
        }

        var etagProperty = entry.EntityType.GetETagProperty();
        if (etagProperty != null)
        {
            entry.SetStoreGeneratedValue(etagProperty, eTag);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IAsyncEnumerable<ReadOnlyMemory<byte>> ExecuteSqlQueryAsync(
        string containerId,
        PartitionKey partitionKeyValue,
        CosmosSqlQuery query,
        ISessionTokenStorage sessionTokenStorage)
    {
        _commandLogger.ExecutingSqlQuery(containerId, partitionKeyValue, query);

        return new ResponseAsyncEnumerable(this, containerId, partitionKeyValue, query, sessionTokenStorage);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual async Task<ReadOnlyMemory<byte>?> ExecuteReadItemAsync(
        string containerId,
        PartitionKey partitionKeyValue,
        string resourceId,
        ISessionTokenStorage sessionTokenStorage,
        CancellationToken cancellationToken = default)
    {
        _commandLogger.ExecutingReadItem(containerId, partitionKeyValue, resourceId);

        var response = await _executionStrategy.ExecuteAsync(
                (containerId, partitionKeyValue, resourceId, sessionTokenStorage, this),
                CreateSingleItemQueryAsync,
                null,
                cancellationToken)
            .ConfigureAwait(false);

        _commandLogger.ExecutedReadItem(
            response.Diagnostics.GetClientElapsedTime(),
            response.Headers.RequestCharge,
            response.Headers.ActivityId,
            resourceId,
            containerId,
            partitionKeyValue);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            // We get no sub-status code if document not found, other not found errors (like session or container) have a sub status code
            if (!response.Headers.TryGetValue(SubStatusCodeHeaderName, out var subStatusCode) || string.IsNullOrWhiteSpace(subStatusCode) || subStatusCode == "0")
            {
                // Track session token to ensure subsequent requests will not read stale data where the document might still exist.
                sessionTokenStorage.TrackSessionToken(containerId, response.Headers.Session);

                return null;
            }
        }

        response.EnsureSuccessStatusCode();

        sessionTokenStorage.TrackSessionToken(containerId, response.Headers.Session);

        return CosmosResponseStreamHelper.ExtractContentAsMemory(response.Content);
    }

    private static async Task<ResponseMessage> CreateSingleItemQueryAsync(
        DbContext? _,
        (string ContainerId, PartitionKey PartitionKeyValue, string ResourceId, ISessionTokenStorage SessionTokenStorage, CosmosClientWrapper Wrapper) parameters,
        CancellationToken cancellationToken = default)
    {
        var (containerId, partitionKeyValue, resourceId, sessionTokenStorage, wrapper) = parameters;
        var container = wrapper.Client.GetDatabase(wrapper._databaseId).GetContainer(containerId);

        var itemRequestOptions = new ItemRequestOptions { SessionToken = sessionTokenStorage.GetSessionToken(containerId) };

        var response = await container.ReadItemStreamAsync(
            resourceId,
            partitionKeyValue,
            itemRequestOptions,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return response;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual FeedIterator CreateQuery(
        string containerId,
        CosmosSqlQuery query,
        ISessionTokenStorage sessionTokenStorage,
        string? continuationToken = null,
        QueryRequestOptions? queryRequestOptions = null)
    {
        var container = Client.GetDatabase(_databaseId).GetContainer(containerId);
        var queryDefinition = new QueryDefinition(query.Query);

        queryDefinition = query.Parameters
            .Aggregate(
                queryDefinition,
                (current, parameter) => parameter.Apply(current));

        return new CosmosFeedIteratorWrapper(container.GetItemQueryStreamIterator(queryDefinition, continuationToken, queryRequestOptions), containerId, sessionTokenStorage);
    }

    private sealed class ResponseAsyncEnumerable(
        CosmosClientWrapper cosmosClient,
        string containerId,
        PartitionKey partitionKeyValue,
        CosmosSqlQuery cosmosSqlQuery,
        ISessionTokenStorage sessionTokenStorage)
        : IAsyncEnumerable<ReadOnlyMemory<byte>>
    {
        private readonly CosmosClientWrapper _cosmosClient = cosmosClient;
        private readonly string _containerId = containerId;
        private readonly PartitionKey _partitionKeyValue = partitionKeyValue;
        private readonly CosmosSqlQuery _cosmosSqlQuery = cosmosSqlQuery;
        private readonly ISessionTokenStorage _sessionTokenStorage = sessionTokenStorage;

        public IAsyncEnumerator<ReadOnlyMemory<byte>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new AsyncEnumerator(this, cancellationToken);

        private sealed class AsyncEnumerator(ResponseAsyncEnumerable documentEnumerable, CancellationToken cancellationToken)
            : IAsyncEnumerator<ReadOnlyMemory<byte>>
        {
            private readonly CosmosClientWrapper _cosmosClientWrapper = documentEnumerable._cosmosClient;
            private readonly string _containerId = documentEnumerable._containerId;
            private readonly PartitionKey _partitionKeyValue = documentEnumerable._partitionKeyValue;
            private readonly CosmosSqlQuery _cosmosSqlQuery = documentEnumerable._cosmosSqlQuery;
            private readonly ISessionTokenStorage _sessionTokenStorage = documentEnumerable._sessionTokenStorage;

            private FeedIterator? _query;
            private ReadOnlyMemory<byte>? _current;

            public ReadOnlyMemory<byte> Current
                => _current ?? throw new InvalidOperationException();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public async ValueTask<bool> MoveNextAsync()
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_query is null)
                {
                    var queryRequestOptions = new QueryRequestOptions();
                    if (_partitionKeyValue != PartitionKey.None)
                    {
                        queryRequestOptions.PartitionKey = _partitionKeyValue;
                    }

                    queryRequestOptions.SessionToken = _sessionTokenStorage.GetSessionToken(_containerId);

                    _query = _cosmosClientWrapper.CreateQuery(
                        _containerId, _cosmosSqlQuery, _sessionTokenStorage, continuationToken: null, queryRequestOptions);
                }

                if (!_query.HasMoreResults)
                {
                    _current = null;
                    return false;
                }

                using var responseMessage = await _cosmosClientWrapper._executionStrategy.ExecuteAsync(
                    (_query, _cosmosClientWrapper),
                    static (_, state, cancellationToken) => state._query.ReadNextAsync(cancellationToken),
                    null,
                    cancellationToken).ConfigureAwait(false);

                _cosmosClientWrapper._commandLogger.ExecutedReadNext(
                    responseMessage.Diagnostics.GetClientElapsedTime(),
                    responseMessage.Headers.RequestCharge,
                    responseMessage.Headers.ActivityId,
                    _containerId,
                    _partitionKeyValue,
                    _cosmosSqlQuery);

                responseMessage.EnsureSuccessStatusCode();

                _current = CosmosResponseStreamHelper.ExtractContentAsMemory(responseMessage.Content);

                return true;
            }

            public async ValueTask DisposeAsync()
            {
                _query?.Dispose();
            }
        }
    }

    private sealed class CosmosFeedIteratorWrapper : FeedIterator
    {
        private readonly FeedIterator _inner;
        private readonly string _containerName;
        private readonly ISessionTokenStorage _sessionTokenStorage;

        public CosmosFeedIteratorWrapper(FeedIterator inner, string containerName, ISessionTokenStorage sessionTokenStorage)
        {
            _inner = inner;
            _containerName = containerName;
            _sessionTokenStorage = sessionTokenStorage;
        }

        public override bool HasMoreResults => _inner.HasMoreResults;

        public override async Task<ResponseMessage> ReadNextAsync(CancellationToken cancellationToken = default)
        {
            var response = await _inner.ReadNextAsync(cancellationToken).ConfigureAwait(false);
            _sessionTokenStorage.TrackSessionToken(_containerName, response.Headers.Session);
            return response;
        }
    }
}
