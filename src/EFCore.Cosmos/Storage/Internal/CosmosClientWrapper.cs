// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
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

    private CosmosClient Client
        => _singletonWrapper.Client;

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
        using var response = await wrapper.Client.GetDatabase(wrapper._databaseId).CreateContainerStreamAsync(
                new Azure.Cosmos.ContainerProperties(parameters.Id, "/" + parameters.PartitionKey)
                {
                    PartitionKeyDefinitionVersion = PartitionKeyDefinitionVersion.V2,
                    DefaultTimeToLive = parameters.DefaultTimeToLive,
                    AnalyticalStoreTimeToLiveInSeconds = parameters.AnalyticalStoreTimeToLiveInSeconds
                },
                parameters.Throughput,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
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
        var partitionKey = CreatePartitionKey(entry);

        var response = await container.CreateItemStreamAsync(
                stream,
                partitionKey == null ? PartitionKey.None : new PartitionKey(partitionKey),
                itemRequestOptions,
                cancellationToken)
            .ConfigureAwait(false);

        wrapper._commandLogger.ExecutedCreateItem(
            response.Diagnostics.GetClientElapsedTime(),
            response.Headers.RequestCharge,
            response.Headers.ActivityId,
            parameters.Document["id"].ToString(),
            parameters.ContainerId,
            partitionKey);

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
        var partitionKey = CreatePartitionKey(entry);

        using var response = await container.ReplaceItemStreamAsync(
                stream,
                parameters.ResourceId,
                partitionKey == null ? PartitionKey.None : new PartitionKey(partitionKey),
                itemRequestOptions,
                cancellationToken)
            .ConfigureAwait(false);

        wrapper._commandLogger.ExecutedReplaceItem(
            response.Diagnostics.GetClientElapsedTime(),
            response.Headers.RequestCharge,
            response.Headers.ActivityId,
            parameters.ResourceId,
            parameters.ContainerId,
            partitionKey);

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
        var partitionKey = CreatePartitionKey(entry);

        using var response = await items.DeleteItemStreamAsync(
                parameters.ResourceId,
                partitionKey == null ? PartitionKey.None : new PartitionKey(partitionKey),
                itemRequestOptions,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        wrapper._commandLogger.ExecutedDeleteItem(
            response.Diagnostics.GetClientElapsedTime(),
            response.Headers.RequestCharge,
            response.Headers.ActivityId,
            parameters.ResourceId,
            parameters.ContainerId,
            partitionKey);

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
                    var jObjectProperty = entry.EntityType.FindProperty(StoreKeyConvention.JObjectPropertyName);
                    enabledContentResponse = (jObjectProperty?.ValueGenerated & ValueGenerated.OnUpdate) == ValueGenerated.OnUpdate;
                    break;
                }
                case EntityState.Added:
                {
                    var jObjectProperty = entry.EntityType.FindProperty(StoreKeyConvention.JObjectPropertyName);
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

    private static string? CreatePartitionKey(IUpdateEntry entry)
    {
        object? partitionKey = null;
        var partitionKeyPropertyName = entry.EntityType.GetPartitionKeyPropertyName();
        if (partitionKeyPropertyName != null)
        {
            var partitionKeyProperty = entry.EntityType.FindProperty(partitionKeyPropertyName)!;
            partitionKey = entry.GetCurrentValue(partitionKeyProperty);

            var converter = partitionKeyProperty.GetTypeMapping().Converter;
            if (converter != null)
            {
                partitionKey = converter.ConvertToProvider(partitionKey);
            }
        }

        return (string?)partitionKey;
    }

    private static void ProcessResponse(ResponseMessage response, IUpdateEntry entry)
    {
        response.EnsureSuccessStatusCode();
        var etagProperty = entry.EntityType.GetETagProperty();
        if (etagProperty != null && entry.EntityState != EntityState.Deleted)
        {
            entry.SetStoreGeneratedValue(etagProperty, response.Headers.ETag);
        }

        var jObjectProperty = entry.EntityType.FindProperty(StoreKeyConvention.JObjectPropertyName);
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
    public virtual IEnumerable<JObject> ExecuteSqlQuery(
        string containerId,
        string? partitionKey,
        CosmosSqlQuery query)
    {
        _databaseLogger.SyncNotSupported();

        _commandLogger.ExecutingSqlQuery(containerId, partitionKey, query);

        return new DocumentEnumerable(this, containerId, partitionKey, query);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IAsyncEnumerable<JObject> ExecuteSqlQueryAsync(
        string containerId,
        string? partitionKey,
        CosmosSqlQuery query)
    {
        _commandLogger.ExecutingSqlQuery(containerId, partitionKey, query);

        return new DocumentAsyncEnumerable(this, containerId, partitionKey, query);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual JObject? ExecuteReadItem(
        string containerId,
        string? partitionKey,
        string resourceId)
    {
        _databaseLogger.SyncNotSupported();

        _commandLogger.ExecutingReadItem(containerId, partitionKey, resourceId);

        var response = _executionStrategy.Execute((containerId, partitionKey, resourceId, this), CreateSingleItemQuery, null);

        _commandLogger.ExecutedReadItem(
            response.Diagnostics.GetClientElapsedTime(),
            response.Headers.RequestCharge,
            response.Headers.ActivityId,
            resourceId,
            containerId,
            partitionKey);

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
        string? partitionKey,
        string resourceId,
        CancellationToken cancellationToken = default)
    {
        _commandLogger.ExecutingReadItem(containerId, partitionKey, resourceId);

        var response = await _executionStrategy.ExecuteAsync(
                (containerId, partitionKey, resourceId, this),
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
            partitionKey);

        return JObjectFromReadItemResponseMessage(response);
    }

    private static ResponseMessage CreateSingleItemQuery(
        DbContext? context,
        (string ContainerId, string? PartitionKey, string ResourceId, CosmosClientWrapper Wrapper) parameters)
        => CreateSingleItemQueryAsync(context, parameters).GetAwaiter().GetResult();

    private static Task<ResponseMessage> CreateSingleItemQueryAsync(
        DbContext? _,
        (string ContainerId, string? PartitionKey, string ResourceId, CosmosClientWrapper Wrapper) parameters,
        CancellationToken cancellationToken = default)
    {
        var (containerId, partitionKey, resourceId, wrapper) = parameters;
        var container = wrapper.Client.GetDatabase(wrapper._databaseId).GetContainer(containerId);

        return container.ReadItemStreamAsync(
            resourceId,
            string.IsNullOrEmpty(partitionKey) ? PartitionKey.None : new PartitionKey(partitionKey),
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

        var jObject = Serializer.Deserialize<JObject>(jsonReader);

        return new JObject(new JProperty("c", jObject));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual FeedIterator CreateQuery(
        string containerId,
        string? partitionKey,
        CosmosSqlQuery query)
    {
        var container = Client.GetDatabase(_databaseId).GetContainer(containerId);
        var queryDefinition = new QueryDefinition(query.Query);

        queryDefinition = query.Parameters
            .Aggregate(
                queryDefinition,
                (current, parameter) => current.WithParameter(parameter.Name, parameter.Value));

        if (string.IsNullOrEmpty(partitionKey))
        {
            return container.GetItemQueryStreamIterator(queryDefinition);
        }

        var queryRequestOptions = new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey) };

        return container.GetItemQueryStreamIterator(queryDefinition, requestOptions: queryRequestOptions);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static JsonTextReader CreateJsonReader(TextReader reader)
    {
        var jsonReader = new JsonTextReader(reader);

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryReadJObject(JsonTextReader jsonReader, [NotNullWhen(true)] out JObject? jObject)
    {
        jObject = null;

        while (jsonReader.Read())
        {
            if (jsonReader.TokenType == JsonToken.StartObject)
            {
                jObject = Serializer.Deserialize<JObject>(jsonReader);
                return true;
            }
        }

        return false;
    }

    private sealed class DocumentEnumerable : IEnumerable<JObject>
    {
        private readonly CosmosClientWrapper _cosmosClient;
        private readonly string _containerId;
        private readonly string? _partitionKey;
        private readonly CosmosSqlQuery _cosmosSqlQuery;

        public DocumentEnumerable(
            CosmosClientWrapper cosmosClient,
            string containerId,
            string? partitionKey,
            CosmosSqlQuery cosmosSqlQuery)
        {
            _cosmosClient = cosmosClient;
            _containerId = containerId;
            _partitionKey = partitionKey;
            _cosmosSqlQuery = cosmosSqlQuery;
        }

        public IEnumerator<JObject> GetEnumerator()
            => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private sealed class Enumerator : IEnumerator<JObject>
        {
            private readonly CosmosClientWrapper _cosmosClientWrapper;
            private readonly string _containerId;
            private readonly string? _partitionKey;
            private readonly CosmosSqlQuery _cosmosSqlQuery;

            private JObject? _current;
            private ResponseMessage? _responseMessage;
            private Stream? _responseStream;
            private StreamReader? _reader;
            private JsonTextReader? _jsonReader;

            private FeedIterator? _query;

            public Enumerator(DocumentEnumerable documentEnumerable)
            {
                _cosmosClientWrapper = documentEnumerable._cosmosClient;
                _containerId = documentEnumerable._containerId;
                _partitionKey = documentEnumerable._partitionKey;
                _cosmosSqlQuery = documentEnumerable._cosmosSqlQuery;
            }

            public JObject Current
                => _current ?? throw new InvalidOperationException();

            object IEnumerator.Current
                => Current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (_jsonReader == null)
                {
                    _query ??= _cosmosClientWrapper.CreateQuery(_containerId, _partitionKey, _cosmosSqlQuery);

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
                        _partitionKey,
                        _cosmosSqlQuery);

                    _responseMessage.EnsureSuccessStatusCode();

                    _responseStream = _responseMessage.Content;
                    _reader = new StreamReader(_responseStream);
                    _jsonReader = CreateJsonReader(_reader);
                }

                if (TryReadJObject(_jsonReader, out var jObject))
                {
                    _current = jObject;
                    return true;
                }

                ResetRead();

                return MoveNext();
            }

            private void ResetRead()
            {
                _jsonReader?.Close();
                _jsonReader = null;
                _reader?.Dispose();
                _reader = null;
                _responseStream?.Dispose();
                _responseStream = null;
            }

            public void Dispose()
            {
                ResetRead();

                _responseMessage?.Dispose();
                _responseMessage = null;
            }

            public void Reset()
                => throw new NotSupportedException(CoreStrings.EnumerableResetNotSupported);
        }
    }

    private sealed class DocumentAsyncEnumerable : IAsyncEnumerable<JObject>
    {
        private readonly CosmosClientWrapper _cosmosClient;
        private readonly string _containerId;
        private readonly string? _partitionKey;
        private readonly CosmosSqlQuery _cosmosSqlQuery;

        public DocumentAsyncEnumerable(
            CosmosClientWrapper cosmosClient,
            string containerId,
            string? partitionKey,
            CosmosSqlQuery cosmosSqlQuery)
        {
            _cosmosClient = cosmosClient;
            _containerId = containerId;
            _partitionKey = partitionKey;
            _cosmosSqlQuery = cosmosSqlQuery;
        }

        public IAsyncEnumerator<JObject> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new AsyncEnumerator(this, cancellationToken);

        private sealed class AsyncEnumerator : IAsyncEnumerator<JObject>
        {
            private readonly CosmosClientWrapper _cosmosClientWrapper;
            private readonly string _containerId;
            private readonly string? _partitionKey;
            private readonly CosmosSqlQuery _cosmosSqlQuery;
            private readonly CancellationToken _cancellationToken;

            private JObject? _current;
            private ResponseMessage? _responseMessage;
            private Stream? _responseStream;
            private StreamReader? _reader;
            private JsonTextReader? _jsonReader;

            private FeedIterator? _query;

            public JObject Current
                => _current ?? throw new InvalidOperationException();

            public AsyncEnumerator(DocumentAsyncEnumerable documentEnumerable, CancellationToken cancellationToken)
            {
                _cosmosClientWrapper = documentEnumerable._cosmosClient;
                _containerId = documentEnumerable._containerId;
                _partitionKey = documentEnumerable._partitionKey;
                _cosmosSqlQuery = documentEnumerable._cosmosSqlQuery;
                _cancellationToken = cancellationToken;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public async ValueTask<bool> MoveNextAsync()
            {
                _cancellationToken.ThrowIfCancellationRequested();

                if (_jsonReader == null)
                {
                    _query ??= _cosmosClientWrapper.CreateQuery(_containerId, _partitionKey, _cosmosSqlQuery);

                    if (!_query.HasMoreResults)
                    {
                        _current = null;
                        return false;
                    }

                    _responseMessage = await _query.ReadNextAsync(_cancellationToken).ConfigureAwait(false);

                    _cosmosClientWrapper._commandLogger.ExecutedReadNext(
                        _responseMessage.Diagnostics.GetClientElapsedTime(),
                        _responseMessage.Headers.RequestCharge,
                        _responseMessage.Headers.ActivityId,
                        _containerId,
                        _partitionKey,
                        _cosmosSqlQuery);

                    _responseMessage.EnsureSuccessStatusCode();

                    _responseStream = _responseMessage.Content;
                    _reader = new StreamReader(_responseStream);
                    _jsonReader = CreateJsonReader(_reader);
                }

                if (TryReadJObject(_jsonReader, out var jObject))
                {
                    _current = jObject;
                    return true;
                }

                await ResetReadAsync().ConfigureAwait(false);

                return await MoveNextAsync().ConfigureAwait(false);
            }

            private async Task ResetReadAsync()
            {
                _jsonReader?.Close();
                _jsonReader = null;
                await _reader.DisposeAsyncIfAvailable().ConfigureAwait(false);
                _reader = null;
                await _responseStream.DisposeAsyncIfAvailable().ConfigureAwait(false);
                _responseStream = null;
            }

            public async ValueTask DisposeAsync()
            {
                await ResetReadAsync().ConfigureAwait(false);

                await _responseMessage.DisposeAsyncIfAvailable().ConfigureAwait(false);
                _responseMessage = null;
            }
        }
    }
}
