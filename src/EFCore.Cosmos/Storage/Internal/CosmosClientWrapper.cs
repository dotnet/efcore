// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.ObjectModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
    public static readonly JsonSerializer Serializer;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly string DefaultPartitionKey = "__partitionKey";

    private readonly ISingletonCosmosClientWrapper _singletonWrapper;
    private readonly string _databaseId;
    private readonly IExecutionStrategy _executionStrategy;
    private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> _commandLogger;
    private readonly IDiagnosticsLogger<DbLoggerCategory.Database> _databaseLogger;
    private readonly bool? _enableContentResponseOnWrite;

    static CosmosClientWrapper()
    {
        Serializer = JsonSerializer.Create();
        Serializer.Converters.Add(new ByteArrayConverter());
        Serializer.DateFormatHandling = DateFormatHandling.IsoDateFormat;
        Serializer.DateParseHandling = DateParseHandling.None;
    }

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
        IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger,
        IDiagnosticsLogger<DbLoggerCategory.Database> databaseLogger)
    {
        var options = dbContextOptions.FindExtension<CosmosOptionsExtension>();

        _singletonWrapper = singletonWrapper;
        _databaseId = options!.DatabaseName;
        _executionStrategy = executionStrategy;
        _commandLogger = commandLogger;
        _databaseLogger = databaseLogger;
        _enableContentResponseOnWrite = options.EnableContentResponseOnWrite;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private CosmosClient Client
        => _singletonWrapper.Client;

    private static bool TryDeserializeNextToken(JsonTextReader jsonReader, out JToken? token)
    {
        switch (jsonReader.TokenType)
        {
            case JsonToken.StartObject:
                token = Serializer.Deserialize<JObject>(jsonReader);
                return true;
            case JsonToken.StartArray:
                token = Serializer.Deserialize<JArray>(jsonReader);
                return true;
            case JsonToken.Date:
            case JsonToken.Bytes:
            case JsonToken.Float:
            case JsonToken.String:
            case JsonToken.Boolean:
            case JsonToken.Integer:
            case JsonToken.Null:
                token = Serializer.Deserialize<JValue>(jsonReader);
                return true;
            case JsonToken.EndArray:
            default:
                token = null;
                return false;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CreateDatabaseIfNotExists(ThroughputProperties? throughput)
    {
        _databaseLogger.SyncNotSupported();

        return _executionStrategy.Execute((throughput, this), CreateDatabaseIfNotExistsOnce, null);
    }

    private static bool CreateDatabaseIfNotExistsOnce(
        DbContext? context,
        (ThroughputProperties? Throughput, CosmosClientWrapper Wrapper) parameters)
        => CreateDatabaseIfNotExistsOnceAsync(context, parameters).GetAwaiter().GetResult();

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
    public virtual bool DeleteDatabase()
    {
        _databaseLogger.SyncNotSupported();

        return _executionStrategy.Execute(this, DeleteDatabaseOnce, null);
    }

    private static bool DeleteDatabaseOnce(
        DbContext? context,
        CosmosClientWrapper wrapper)
        => DeleteDatabaseOnceAsync(context, wrapper).GetAwaiter().GetResult();

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
    public virtual bool CreateContainerIfNotExists(ContainerProperties properties)
    {
        _databaseLogger.SyncNotSupported();

        return _executionStrategy.Execute((properties, this), CreateContainerIfNotExistsOnce, null);
    }

    private static bool CreateContainerIfNotExistsOnce(
        DbContext context,
        (ContainerProperties Parameters, CosmosClientWrapper Wrapper) parametersTuple)
        => CreateContainerIfNotExistsOnceAsync(context, parametersTuple).GetAwaiter().GetResult();

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
        foreach (var index in parameters.Indexes)
        {
            var vectorIndexType = (VectorIndexType?)index.FindAnnotation(CosmosAnnotationNames.VectorIndexType)?.Value;
            if (vectorIndexType != null)
            {
                // Model validation will ensure there is only one property.
                Check.DebugAssert(index.Properties.Count == 1, "Vector index must have one property.");

                vectorIndexes.Add(
                    new VectorIndexPath { Path = "/" + index.Properties[0].GetJsonPropertyName(), Type = vectorIndexType.Value });
            }
        }

        var embeddings = new Collection<Embedding>();
        foreach (var tuple in parameters.Vectors)
        {
            embeddings.Add(
                new Embedding
                {
                    Path = "/" + tuple.Property.GetJsonPropertyName(),
                    DataType = CosmosVectorType.CreateDefaultVectorDataType(tuple.Property.ClrType),
                    Dimensions = tuple.VectorType.Dimensions,
                    DistanceFunction = tuple.VectorType.DistanceFunction
                });
        }

        var containerProperties = new Azure.Cosmos.ContainerProperties(parameters.Id, partitionKeyPaths)
        {
            PartitionKeyDefinitionVersion = PartitionKeyDefinitionVersion.V2,
            DefaultTimeToLive = parameters.DefaultTimeToLive,
            AnalyticalStoreTimeToLiveInSeconds = parameters.AnalyticalStoreTimeToLiveInSeconds,
        };

        if (embeddings.Any())
        {
            containerProperties.VectorEmbeddingPolicy = new VectorEmbeddingPolicy(embeddings);
        }

        if (vectorIndexes.Any())
        {
            containerProperties.IndexingPolicy = new IndexingPolicy { VectorIndexes = vectorIndexes };
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
    public virtual bool CreateItem(
        string containerId,
        JToken document,
        IUpdateEntry entry)
    {
        _databaseLogger.SyncNotSupported();

        return _executionStrategy.Execute((containerId, document, entry, this), CreateItemOnce, null);
    }

    private static bool CreateItemOnce(
        DbContext context,
        (string ContainerId, JToken Document, IUpdateEntry Entry, CosmosClientWrapper Wrapper) parameters)
        => CreateItemOnceAsync(context, parameters).GetAwaiter().GetResult();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<bool> CreateItemAsync(
        string containerId,
        JToken document,
        IUpdateEntry updateEntry,
        CancellationToken cancellationToken = default)
        => _executionStrategy.ExecuteAsync((containerId, document, updateEntry, this), CreateItemOnceAsync, null, cancellationToken);

    private static async Task<bool> CreateItemOnceAsync(
        DbContext _,
        (string ContainerId, JToken Document, IUpdateEntry Entry, CosmosClientWrapper Wrapper) parameters,
        CancellationToken cancellationToken = default)
    {
        var stream = new MemoryStream();
        await using var __ = stream.ConfigureAwait(false);
        var writer = new StreamWriter(stream, new UTF8Encoding(), bufferSize: 1024, leaveOpen: false);
        await using var ___ = writer.ConfigureAwait(false);

        using var jsonWriter = new JsonTextWriter(writer);
        Serializer.Serialize(jsonWriter, parameters.Document);
        await jsonWriter.FlushAsync(cancellationToken).ConfigureAwait(false);

        var entry = parameters.Entry;
        var wrapper = parameters.Wrapper;
        var container = wrapper.Client.GetDatabase(wrapper._databaseId).GetContainer(parameters.ContainerId);
        var itemRequestOptions = CreateItemRequestOptions(entry, wrapper._enableContentResponseOnWrite);
        var partitionKeyValue = ExtractPartitionKeyValue(entry);

        var response = await container.CreateItemStreamAsync(
                stream,
                partitionKeyValue,
                itemRequestOptions,
                cancellationToken)
            .ConfigureAwait(false);

        wrapper._commandLogger.ExecutedCreateItem(
            response.Diagnostics.GetClientElapsedTime(),
            response.Headers.RequestCharge,
            response.Headers.ActivityId,
            parameters.Document["id"]!.ToString(),
            parameters.ContainerId,
            partitionKeyValue);

        ProcessResponse(response, entry);

        return response.StatusCode == HttpStatusCode.Created;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool ReplaceItem(
        string collectionId,
        string documentId,
        JObject document,
        IUpdateEntry entry)
    {
        _databaseLogger.SyncNotSupported();

        return _executionStrategy.Execute((collectionId, documentId, document, entry, this), ReplaceItemOnce, null);
    }

    private static bool ReplaceItemOnce(
        DbContext context,
        (string ContainerId, string ItemId, JObject Document, IUpdateEntry Entry, CosmosClientWrapper Wrapper) parameters)
        => ReplaceItemOnceAsync(context, parameters).GetAwaiter().GetResult();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<bool> ReplaceItemAsync(
        string collectionId,
        string documentId,
        JObject document,
        IUpdateEntry updateEntry,
        CancellationToken cancellationToken = default)
        => _executionStrategy.ExecuteAsync(
            (collectionId, documentId, document, updateEntry, this), ReplaceItemOnceAsync, null, cancellationToken);

    private static async Task<bool> ReplaceItemOnceAsync(
        DbContext _,
        (string ContainerId, string ResourceId, JObject Document, IUpdateEntry Entry, CosmosClientWrapper Wrapper) parameters,
        CancellationToken cancellationToken = default)
    {
        var stream = new MemoryStream();
        await using var __ = stream.ConfigureAwait(false);
        var writer = new StreamWriter(stream, new UTF8Encoding(), bufferSize: 1024, leaveOpen: false);
        await using var ___ = writer.ConfigureAwait(false);
        using var jsonWriter = new JsonTextWriter(writer);
        Serializer.Serialize(jsonWriter, parameters.Document);
        await jsonWriter.FlushAsync(cancellationToken).ConfigureAwait(false);

        var entry = parameters.Entry;
        var wrapper = parameters.Wrapper;
        var container = wrapper.Client.GetDatabase(wrapper._databaseId).GetContainer(parameters.ContainerId);
        var itemRequestOptions = CreateItemRequestOptions(entry, wrapper._enableContentResponseOnWrite);
        var partitionKeyValue = ExtractPartitionKeyValue(entry);

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
            parameters.ContainerId,
            partitionKeyValue);

        ProcessResponse(response, entry);

        return response.StatusCode == HttpStatusCode.OK;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool DeleteItem(
        string containerId,
        string documentId,
        IUpdateEntry entry)
    {
        _databaseLogger.SyncNotSupported();

        return _executionStrategy.Execute((containerId, documentId, entry, this), DeleteItemOnce, null);
    }

    private static bool DeleteItemOnce(
        DbContext context,
        (string ContainerId, string DocumentId, IUpdateEntry Entry, CosmosClientWrapper Wrapper) parameters)
        => DeleteItemOnceAsync(context, parameters).GetAwaiter().GetResult();

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
        CancellationToken cancellationToken = default)
        => _executionStrategy.ExecuteAsync((containerId, documentId, entry, this), DeleteItemOnceAsync, null, cancellationToken);

    private static async Task<bool> DeleteItemOnceAsync(
        DbContext? _,
        (string ContainerId, string ResourceId, IUpdateEntry Entry, CosmosClientWrapper Wrapper) parameters,
        CancellationToken cancellationToken = default)
    {
        var entry = parameters.Entry;
        var wrapper = parameters.Wrapper;
        var items = wrapper.Client.GetDatabase(wrapper._databaseId).GetContainer(parameters.ContainerId);

        var itemRequestOptions = CreateItemRequestOptions(entry, wrapper._enableContentResponseOnWrite);
        var partitionKeyValue = ExtractPartitionKeyValue(entry);

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
            parameters.ContainerId,
            partitionKeyValue);

        ProcessResponse(response, entry);

        return response.StatusCode == HttpStatusCode.NoContent;
    }

    private static ItemRequestOptions? CreateItemRequestOptions(IUpdateEntry entry, bool? enableContentResponseOnWrite)
    {
        var etagProperty = entry.EntityType.GetETagProperty();
        if (etagProperty == null)
        {
            return null;
        }

        var etag = entry.GetOriginalValue(etagProperty);
        var converter = etagProperty.GetTypeMapping().Converter;
        if (converter != null)
        {
            etag = converter.ConvertToProvider(etag);
        }

        bool enabledContentResponse;
        if (enableContentResponseOnWrite.HasValue)
        {
            enabledContentResponse = enableContentResponseOnWrite.Value;
        }
        else
        {
            switch (entry.EntityState)
            {
                case EntityState.Modified:
                {
                    var jObjectProperty = entry.EntityType.FindProperty(CosmosPartitionKeyInPrimaryKeyConvention.JObjectPropertyName);
                    enabledContentResponse = (jObjectProperty?.ValueGenerated & ValueGenerated.OnUpdate) == ValueGenerated.OnUpdate;
                    break;
                }
                case EntityState.Added:
                {
                    var jObjectProperty = entry.EntityType.FindProperty(CosmosPartitionKeyInPrimaryKeyConvention.JObjectPropertyName);
                    enabledContentResponse = (jObjectProperty?.ValueGenerated & ValueGenerated.OnAdd) == ValueGenerated.OnAdd;
                    break;
                }
                default:
                    enabledContentResponse = false;
                    break;
            }
        }

        return new ItemRequestOptions { IfMatchEtag = (string?)etag, EnableContentResponseOnWrite = enabledContentResponse };
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

    private static void ProcessResponse(ResponseMessage response, IUpdateEntry entry)
    {
        response.EnsureSuccessStatusCode();
        var etagProperty = entry.EntityType.GetETagProperty();
        if (etagProperty != null && entry.EntityState != EntityState.Deleted)
        {
            entry.SetStoreGeneratedValue(etagProperty, response.Headers.ETag);
        }

        var jObjectProperty = entry.EntityType.FindProperty(CosmosPartitionKeyInPrimaryKeyConvention.JObjectPropertyName);
        if (jObjectProperty is { ValueGenerated: ValueGenerated.OnAddOrUpdate }
            && response.Content != null)
        {
            using var responseStream = response.Content;
            using var reader = new StreamReader(responseStream);
            using var jsonReader = new JsonTextReader(reader);

            var createdDocument = Serializer.Deserialize<JObject>(jsonReader);

            entry.SetStoreGeneratedValue(jObjectProperty, createdDocument);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<JToken> ExecuteSqlQuery(
        string containerId,
        PartitionKey partitionKeyValue,
        CosmosSqlQuery query)
    {
        _databaseLogger.SyncNotSupported();

        _commandLogger.ExecutingSqlQuery(containerId, partitionKeyValue, query);

        return new DocumentEnumerable(this, containerId, partitionKeyValue, query);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IAsyncEnumerable<JToken> ExecuteSqlQueryAsync(
        string containerId,
        PartitionKey partitionKeyValue,
        CosmosSqlQuery query)
    {
        _commandLogger.ExecutingSqlQuery(containerId, partitionKeyValue, query);

        return new DocumentAsyncEnumerable(this, containerId, partitionKeyValue, query);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual JObject? ExecuteReadItem(
        string containerId,
        PartitionKey partitionKeyValue,
        string resourceId)
    {
        _databaseLogger.SyncNotSupported();

        _commandLogger.ExecutingReadItem(containerId, partitionKeyValue, resourceId);

        var response = _executionStrategy.Execute((containerId, partitionKeyValue, resourceId, this), CreateSingleItemQuery, null);

        _commandLogger.ExecutedReadItem(
            response.Diagnostics.GetClientElapsedTime(),
            response.Headers.RequestCharge,
            response.Headers.ActivityId,
            resourceId,
            containerId,
            partitionKeyValue);

        return JObjectFromReadItemResponseMessage(response);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual async Task<JObject?> ExecuteReadItemAsync(
        string containerId,
        PartitionKey partitionKeyValue,
        string resourceId,
        CancellationToken cancellationToken = default)
    {
        _commandLogger.ExecutingReadItem(containerId, partitionKeyValue, resourceId);

        var response = await _executionStrategy.ExecuteAsync(
                (containerId, partitionKeyValue, resourceId, this),
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

        return JObjectFromReadItemResponseMessage(response);
    }

    private static ResponseMessage CreateSingleItemQuery(
        DbContext? context,
        (string ContainerId, PartitionKey PartitionKeyValue, string ResourceId, CosmosClientWrapper Wrapper) parameters)
        => CreateSingleItemQueryAsync(context, parameters).GetAwaiter().GetResult();

    private static Task<ResponseMessage> CreateSingleItemQueryAsync(
        DbContext? _,
        (string ContainerId, PartitionKey PartitionKeyValue, string ResourceId, CosmosClientWrapper Wrapper) parameters,
        CancellationToken cancellationToken = default)
    {
        var (containerId, partitionKeyValue, resourceId, wrapper) = parameters;
        var container = wrapper.Client.GetDatabase(wrapper._databaseId).GetContainer(containerId);

        return container.ReadItemStreamAsync(
            resourceId,
            partitionKeyValue,
            cancellationToken: cancellationToken);
    }

    private static JObject? JObjectFromReadItemResponseMessage(ResponseMessage responseMessage)
    {
        if (responseMessage.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        responseMessage.EnsureSuccessStatusCode();

        var responseStream = responseMessage.Content;
        using var reader = new StreamReader(responseStream);
        using var jsonReader = new JsonTextReader(reader);

        return Serializer.Deserialize<JObject>(jsonReader);
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
        string? continuationToken = null,
        QueryRequestOptions? queryRequestOptions = null)
    {
        var container = Client.GetDatabase(_databaseId).GetContainer(containerId);
        var queryDefinition = new QueryDefinition(query.Query);

        queryDefinition = query.Parameters
            .Aggregate(
                queryDefinition,
                (current, parameter) => current.WithParameter(parameter.Name, parameter.Value));

        return container.GetItemQueryStreamIterator(queryDefinition, continuationToken, queryRequestOptions);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static JsonTextReader CreateJsonReader(TextReader reader)
    {
        var jsonReader = new JsonTextReader(reader);
        jsonReader.DateParseHandling = DateParseHandling.None;

        while (jsonReader.Read())
        {
            if (jsonReader.TokenType == JsonToken.StartObject)
            {
                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType == JsonToken.StartArray)
                    {
                        return jsonReader;
                    }
                }
            }
        }

        return jsonReader;
    }

    private sealed class DocumentEnumerable(
        CosmosClientWrapper cosmosClient,
        string containerId,
        PartitionKey partitionKeyValue,
        CosmosSqlQuery cosmosSqlQuery)
        : IEnumerable<JToken>
    {
        private readonly CosmosClientWrapper _cosmosClient = cosmosClient;
        private readonly string _containerId = containerId;
        private readonly PartitionKey _partitionKeyValue = partitionKeyValue;
        private readonly CosmosSqlQuery _cosmosSqlQuery = cosmosSqlQuery;

        public IEnumerator<JToken> GetEnumerator()
            => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private sealed class Enumerator(DocumentEnumerable documentEnumerable) : IEnumerator<JToken>
        {
            private readonly CosmosClientWrapper _cosmosClientWrapper = documentEnumerable._cosmosClient;
            private readonly string _containerId = documentEnumerable._containerId;
            private readonly PartitionKey _partitionKeyValue = documentEnumerable._partitionKeyValue;
            private readonly CosmosSqlQuery _cosmosSqlQuery = documentEnumerable._cosmosSqlQuery;

            private JToken? _current;
            private ResponseMessage? _responseMessage;
            private IEnumerator<JToken>? _responseMessageEnumerator;

            private FeedIterator? _query;

            public JToken Current
                => _current ?? throw new InvalidOperationException();

            object IEnumerator.Current
                => Current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (_responseMessageEnumerator == null)
                {
                    if (_query is null)
                    {
                        var queryRequestOptions = new QueryRequestOptions();
                        if (_partitionKeyValue != PartitionKey.None)
                        {
                            queryRequestOptions.PartitionKey = _partitionKeyValue;
                        }

                        _query = _cosmosClientWrapper.CreateQuery(
                            _containerId, _cosmosSqlQuery, continuationToken: null, queryRequestOptions);
                    }

                    if (!_query.HasMoreResults)
                    {
                        _current = null;
                        return false;
                    }

                    _responseMessage = _query.ReadNextAsync().GetAwaiter().GetResult();

                    _cosmosClientWrapper._commandLogger.ExecutedReadNext(
                        _responseMessage.Diagnostics.GetClientElapsedTime(),
                        _responseMessage.Headers.RequestCharge,
                        _responseMessage.Headers.ActivityId,
                        _containerId,
                        _partitionKeyValue,
                        _cosmosSqlQuery);

                    _responseMessage.EnsureSuccessStatusCode();

                    _responseMessageEnumerator = new ResponseMessageEnumerable(_responseMessage).GetEnumerator();
                }

                if (_responseMessageEnumerator.MoveNext())
                {
                    _current = _responseMessageEnumerator.Current;
                    return true;
                }

                ResetRead();

                return MoveNext();
            }

            private void ResetRead()
            {
                _responseMessageEnumerator?.Dispose();
                _responseMessageEnumerator = null;
                _responseMessage?.Dispose();
            }

            public void Dispose()
            {
                ResetRead();
                _query?.Dispose();
            }

            public void Reset()
                => throw new NotSupportedException(CoreStrings.EnumerableResetNotSupported);
        }
    }

    private sealed class DocumentAsyncEnumerable(
        CosmosClientWrapper cosmosClient,
        string containerId,
        PartitionKey partitionKeyValue,
        CosmosSqlQuery cosmosSqlQuery)
        : IAsyncEnumerable<JToken>
    {
        private readonly CosmosClientWrapper _cosmosClient = cosmosClient;
        private readonly string _containerId = containerId;
        private readonly PartitionKey _partitionKeyValue = partitionKeyValue;
        private readonly CosmosSqlQuery _cosmosSqlQuery = cosmosSqlQuery;

        public IAsyncEnumerator<JToken> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new AsyncEnumerator(this, cancellationToken);

        private sealed class AsyncEnumerator(DocumentAsyncEnumerable documentEnumerable, CancellationToken cancellationToken)
            : IAsyncEnumerator<JToken>
        {
            private readonly CosmosClientWrapper _cosmosClientWrapper = documentEnumerable._cosmosClient;
            private readonly string _containerId = documentEnumerable._containerId;
            private readonly PartitionKey _partitionKeyValue = documentEnumerable._partitionKeyValue;
            private readonly CosmosSqlQuery _cosmosSqlQuery = documentEnumerable._cosmosSqlQuery;

            private JToken? _current;
            private ResponseMessage? _responseMessage;
            private IAsyncEnumerator<JToken>? _responseMessageEnumerator;

            private FeedIterator? _query;

            public JToken Current
                => _current ?? throw new InvalidOperationException();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public async ValueTask<bool> MoveNextAsync()
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_responseMessageEnumerator == null)
                {
                    if (_query is null)
                    {
                        var queryRequestOptions = new QueryRequestOptions();
                        if (_partitionKeyValue != PartitionKey.None)
                        {
                            queryRequestOptions.PartitionKey = _partitionKeyValue;
                        }

                        _query = _cosmosClientWrapper.CreateQuery(
                            _containerId, _cosmosSqlQuery, continuationToken: null, queryRequestOptions);
                    }

                    if (!_query.HasMoreResults)
                    {
                        _current = null;
                        return false;
                    }

                    _responseMessage = await _query.ReadNextAsync(cancellationToken).ConfigureAwait(false);

                    _cosmosClientWrapper._commandLogger.ExecutedReadNext(
                        _responseMessage.Diagnostics.GetClientElapsedTime(),
                        _responseMessage.Headers.RequestCharge,
                        _responseMessage.Headers.ActivityId,
                        _containerId,
                        _partitionKeyValue,
                        _cosmosSqlQuery);

                    _responseMessage.EnsureSuccessStatusCode();

                    _responseMessageEnumerator = new ResponseMessageEnumerable(_responseMessage).GetAsyncEnumerator(cancellationToken);
                }

                if (await _responseMessageEnumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    _current = _responseMessageEnumerator.Current;
                    return true;
                }

                await ResetReadAsync().ConfigureAwait(false);

                return await MoveNextAsync().ConfigureAwait(false);
            }

            private async Task ResetReadAsync()
            {
                if (_responseMessageEnumerator is not null)
                {
                    await _responseMessageEnumerator.DisposeAsync().ConfigureAwait(false);
                    _responseMessageEnumerator = null;
                }

                _responseMessage?.Dispose();
            }

            public async ValueTask DisposeAsync()
            {
                await ResetReadAsync().ConfigureAwait(false);
                _query?.Dispose();
            }
        }
    }

    #region ResponseMessageEnumerable

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<JToken> GetResponseMessageEnumerable(ResponseMessage responseMessage)
        => new ResponseMessageEnumerable(responseMessage);

    private sealed class ResponseMessageEnumerable(ResponseMessage responseMessage) : IEnumerable<JToken>, IAsyncEnumerable<JToken>
    {
        public IEnumerator<JToken> GetEnumerator()
            => new ResponseMessageEnumerator(responseMessage);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public IAsyncEnumerator<JToken> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new ResponseMessageAsyncEnumerator(responseMessage);
    }

    private sealed class ResponseMessageEnumerator : IEnumerator<JToken>
    {
        private readonly Stream _responseStream;
        private readonly StreamReader _reader;
        private readonly JsonTextReader _jsonReader;

        private JToken? _current;

        public ResponseMessageEnumerator(ResponseMessage responseMessage)
        {
            _responseStream = responseMessage.Content;
            _reader = new StreamReader(_responseStream);
            _jsonReader = CreateJsonReader(_reader);
        }

        public bool MoveNext()
        {
            while (_jsonReader.Read())
            {
                return TryDeserializeNextToken(_jsonReader, out _current);
            }

            return false;
        }

        public JToken Current
            => _current ?? throw new InvalidOperationException();

        object IEnumerator.Current
            => Current;

        public void Dispose()
        {
            _jsonReader.Close();
            _reader.Dispose();
            _responseStream.Dispose();
        }

        public void Reset()
            => throw new NotSupportedException();
    }

    private sealed class ResponseMessageAsyncEnumerator : IAsyncEnumerator<JToken>
    {
        private readonly Stream _responseStream;
        private readonly StreamReader _reader;
        private readonly JsonTextReader _jsonReader;

        private JToken? _current;

        public ResponseMessageAsyncEnumerator(ResponseMessage responseMessage)
        {
            _responseStream = responseMessage.Content;
            _reader = new StreamReader(_responseStream);
            _jsonReader = CreateJsonReader(_reader);
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            while (await _jsonReader.ReadAsync().ConfigureAwait(false))
            {
                return TryDeserializeNextToken(_jsonReader, out _current);
            }

            return false;
        }

        public JToken Current
            => _current ?? throw new InvalidOperationException();

        public async ValueTask DisposeAsync()
        {
            _jsonReader.Close();
            _reader.Dispose();
            await _responseStream.DisposeAsync().ConfigureAwait(false);
        }
    }

    #endregion ResponseMessageEnumerable
}
