// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class CosmosClientWrapper : ICosmosClientWrapper
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static readonly JsonSerializer Serializer = JsonSerializer.Create();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static readonly string DefaultPartitionKey = "__partitionKey";

        private readonly ISingletonCosmosClientWrapper _singletonWrapper;
        private readonly string _databaseId;
        private readonly IExecutionStrategyFactory _executionStrategyFactory;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> _commandLogger;
        private readonly bool? _enableContentResponseOnWrite;

        static CosmosClientWrapper()
        {
            Serializer.Converters.Add(new ByteArrayConverter());
            Serializer.DateFormatHandling = DateFormatHandling.IsoDateFormat;
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
            IExecutionStrategyFactory executionStrategyFactory,
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger)
        {
            var options = dbContextOptions.FindExtension<CosmosOptionsExtension>();

            _singletonWrapper = singletonWrapper;
            _databaseId = options!.DatabaseName;
            _executionStrategyFactory = executionStrategyFactory;
            _commandLogger = commandLogger;
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
        public virtual bool CreateDatabaseIfNotExists()
            => _executionStrategyFactory.Create().Execute(
                (object?)null, CreateDatabaseIfNotExistsOnce, null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CreateDatabaseIfNotExistsOnce(
            DbContext? context,
            object? state)
            => CreateDatabaseIfNotExistsOnceAsync(context, state).GetAwaiter().GetResult();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Task<bool> CreateDatabaseIfNotExistsAsync(
            CancellationToken cancellationToken = default)
            => _executionStrategyFactory.Create().ExecuteAsync(
                (object?)null, CreateDatabaseIfNotExistsOnceAsync, null, cancellationToken);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async Task<bool> CreateDatabaseIfNotExistsOnceAsync(
            DbContext? _,
            object? __,
            CancellationToken cancellationToken = default)
        {
            var response = await Client.CreateDatabaseIfNotExistsAsync(_databaseId, cancellationToken: cancellationToken)
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
            => _executionStrategyFactory.Create().Execute((object?)null, DeleteDatabaseOnce, null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool DeleteDatabaseOnce(
            DbContext? context,
            object? state)
            => DeleteDatabaseOnceAsync(context, state).GetAwaiter().GetResult();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Task<bool> DeleteDatabaseAsync(
            CancellationToken cancellationToken = default)
            => _executionStrategyFactory.Create().ExecuteAsync(
                (object?)null, DeleteDatabaseOnceAsync, null, cancellationToken);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async Task<bool> DeleteDatabaseOnceAsync(
            DbContext? _,
            object? __,
            CancellationToken cancellationToken = default)
        {
            using var response = await Client.GetDatabase(_databaseId).DeleteStreamAsync(cancellationToken: cancellationToken)
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
        public virtual bool CreateContainerIfNotExists(
            string containerId,
            string partitionKey)
            => _executionStrategyFactory.Create().Execute(
                (containerId, partitionKey), CreateContainerIfNotExistsOnce, null);

        private bool CreateContainerIfNotExistsOnce(
            DbContext context,
            (string ContainerId, string PartitionKey) parameters)
            => CreateContainerIfNotExistsOnceAsync(context, parameters).GetAwaiter().GetResult();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Task<bool> CreateContainerIfNotExistsAsync(
            string containerId,
            string partitionKey,
            CancellationToken cancellationToken = default)
            => _executionStrategyFactory.Create().ExecuteAsync(
                (containerId, partitionKey), CreateContainerIfNotExistsOnceAsync, null, cancellationToken);

        private async Task<bool> CreateContainerIfNotExistsOnceAsync(
            DbContext _,
            (string ContainerId, string PartitionKey) parameters,
            CancellationToken cancellationToken = default)
        {
            using var response = await Client.GetDatabase(_databaseId).CreateContainerStreamAsync(
                    new ContainerProperties(parameters.ContainerId, "/" + parameters.PartitionKey)
                    {
                        PartitionKeyDefinitionVersion = PartitionKeyDefinitionVersion.V2
                    },
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
            => _executionStrategyFactory.Create().Execute(
                (containerId, document, entry), CreateItemOnce, null);

        private bool CreateItemOnce(
            DbContext context,
            (string ContainerId, JToken Document, IUpdateEntry Entry) parameters)
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
            => _executionStrategyFactory.Create().ExecuteAsync(
                (containerId, document, updateEntry), CreateItemOnceAsync, null, cancellationToken);

        private async Task<bool> CreateItemOnceAsync(
            DbContext _,
            (string ContainerId, JToken Document, IUpdateEntry Entry) parameters,
            CancellationToken cancellationToken = default)
        {
            await using var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream, new UTF8Encoding(), bufferSize: 1024, leaveOpen: false);
            using var jsonWriter = new JsonTextWriter(writer);
            Serializer.Serialize(jsonWriter, parameters.Document);
            await jsonWriter.FlushAsync(cancellationToken).ConfigureAwait(false);

            var entry = parameters.Entry;
            var container = Client.GetDatabase(_databaseId).GetContainer(parameters.ContainerId);
            var itemRequestOptions = CreateItemRequestOptions(entry);
            var partitionKey = CreatePartitionKey(entry);

            using var response = await container.CreateItemStreamAsync(stream, partitionKey, itemRequestOptions, cancellationToken)
                .ConfigureAwait(false);
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
            => _executionStrategyFactory.Create().Execute(
                (collectionId, documentId, document, entry),
                ReplaceItemOnce,
                null);

        private bool ReplaceItemOnce(
            DbContext context,
            (string ContainerId, string ItemId, JObject Document, IUpdateEntry Entry) parameters)
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
            => _executionStrategyFactory.Create().ExecuteAsync(
                (collectionId, documentId, document, updateEntry),
                ReplaceItemOnceAsync,
                null,
                cancellationToken);

        private async Task<bool> ReplaceItemOnceAsync(
            DbContext _,
            (string ContainerId, string ItemId, JObject Document, IUpdateEntry Entry) parameters,
            CancellationToken cancellationToken = default)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, new UTF8Encoding(), bufferSize: 1024, leaveOpen: false);
            using var jsonWriter = new JsonTextWriter(writer);
            Serializer.Serialize(jsonWriter, parameters.Document);
            await jsonWriter.FlushAsync(cancellationToken).ConfigureAwait(false);

            var entry = parameters.Entry;
            var container = Client.GetDatabase(_databaseId).GetContainer(parameters.ContainerId);
            var itemRequestOptions = CreateItemRequestOptions(entry);
            var partitionKey = CreatePartitionKey(entry);

            using var response = await container.ReplaceItemStreamAsync(
                    stream, parameters.ItemId, partitionKey, itemRequestOptions, cancellationToken)
                .ConfigureAwait(false);
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
            => _executionStrategyFactory.Create().Execute(
                (containerId, documentId, entry), DeleteItemOnce, null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool DeleteItemOnce(
            DbContext context,
            (string ContainerId, string DocumentId, IUpdateEntry Entry) parameters)
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
            => _executionStrategyFactory.Create().ExecuteAsync(
                (containerId, documentId, entry), DeleteItemOnceAsync, null, cancellationToken);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async Task<bool> DeleteItemOnceAsync(
            DbContext? _,
            (string ContainerId, string DocumentId, IUpdateEntry Entry) parameters,
            CancellationToken cancellationToken = default)
        {
            var entry = parameters.Entry;
            var items = Client.GetDatabase(_databaseId).GetContainer(parameters.ContainerId);

            var itemRequestOptions = CreateItemRequestOptions(entry);
            var partitionKey = CreatePartitionKey(entry);

            using var response = await items.DeleteItemStreamAsync(
                    parameters.DocumentId, partitionKey, itemRequestOptions, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            ProcessResponse(response, entry);

            return response.StatusCode == HttpStatusCode.NoContent;
        }

        private ItemRequestOptions? CreateItemRequestOptions(IUpdateEntry entry)
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

            var enabledContentResponse = _enableContentResponseOnWrite
                ?? entry.EntityType.FindProperty(StoreKeyConvention.JObjectPropertyName)?.ValueGenerated == ValueGenerated.OnAddOrUpdate;

            return new ItemRequestOptions { IfMatchEtag = (string?)etag, EnableContentResponseOnWrite = enabledContentResponse };
        }

        private static PartitionKey CreatePartitionKey(IUpdateEntry entry)
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

            return partitionKey == null ? PartitionKey.None : new PartitionKey((string)partitionKey);
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
            if (jObjectProperty != null
                && jObjectProperty.ValueGenerated == ValueGenerated.OnAddOrUpdate
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
        public virtual JObject ExecuteReadItem(
            string containerId,
            string? partitionKey,
            string resourceId)
        {
            _commandLogger.ExecutingReadItem(containerId, partitionKey, resourceId);

            var responseMessage = CreateSingleItemQuery(
                containerId, partitionKey, resourceId).GetAwaiter().GetResult();

            return JObjectFromReadItemResponseMessage(responseMessage);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async Task<JObject> ExecuteReadItemAsync(
            string containerId,
            string? partitionKey,
            string resourceId,
            CancellationToken cancellationToken = default)
        {
            _commandLogger.ExecutingReadItem(containerId, partitionKey, resourceId);

            var responseMessage = await CreateSingleItemQuery(
                    containerId, partitionKey, resourceId, cancellationToken)
                .ConfigureAwait(false);

            return JObjectFromReadItemResponseMessage(responseMessage);
        }

        private static JObject JObjectFromReadItemResponseMessage(ResponseMessage responseMessage)
        {
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

        private async Task<ResponseMessage> CreateSingleItemQuery(
            string containerId,
            string? partitionKey,
            string resourceId,
            CancellationToken cancellationToken = default)
        {
            var container = Client.GetDatabase(_databaseId).GetContainer(containerId);

            return await container.ReadItemStreamAsync(
                    resourceId,
                    string.IsNullOrEmpty(partitionKey) ? PartitionKey.None : new PartitionKey(partitionKey),
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
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
            private readonly ICosmosClientWrapper _cosmosClient;
            private readonly string _containerId;
            private readonly string? _partitionKey;
            private readonly CosmosSqlQuery _cosmosSqlQuery;

            public DocumentEnumerable(
                ICosmosClientWrapper cosmosClient,
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
                private readonly ICosmosClientWrapper _cosmosClientWrapper;
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

                public JObject Current => _current ?? throw new InvalidOperationException();

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
            private readonly ICosmosClientWrapper _cosmosClient;
            private readonly string _containerId;
            private readonly string? _partitionKey;
            private readonly CosmosSqlQuery _cosmosSqlQuery;

            public DocumentAsyncEnumerable(
                ICosmosClientWrapper cosmosClient,
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
                private readonly ICosmosClientWrapper _cosmosClientWrapper;
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

                public JObject Current => _current ?? throw new InvalidOperationException();

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
}
