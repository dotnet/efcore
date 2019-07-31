// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
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
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class CosmosClientWrapper
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static readonly JsonSerializer Serializer = new JsonSerializer();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static readonly string DefaultPartitionKey = "__partitionKey";

        private readonly SingletonCosmosClientWrapper _singletonWrapper;
        private readonly string _databaseId;
        private readonly IExecutionStrategyFactory _executionStrategyFactory;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> _commandLogger;

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
            [NotNull] SingletonCosmosClientWrapper singletonWrapper,
            [NotNull] IDbContextOptions dbContextOptions,
            [NotNull] IExecutionStrategyFactory executionStrategyFactory,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger)
        {
            var options = dbContextOptions.FindExtension<CosmosOptionsExtension>();

            _singletonWrapper = singletonWrapper;
            _databaseId = options.DatabaseName;
            _executionStrategyFactory = executionStrategyFactory;
            _commandLogger = commandLogger;
        }

        private CosmosClient Client => _singletonWrapper.Client;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CreateDatabaseIfNotExists()
            => _executionStrategyFactory.Create().Execute(
                (object)null, CreateDatabaseIfNotExistsOnce, null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CreateDatabaseIfNotExistsOnce(
            DbContext context,
            object state)
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
                (object)null, CreateDatabaseIfNotExistsOnceAsync, null, cancellationToken);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async Task<bool> CreateDatabaseIfNotExistsOnceAsync(
            DbContext _,
            object __,
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
            => _executionStrategyFactory.Create().Execute((object)null, DeleteDatabaseOnce, null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool DeleteDatabaseOnce(
            DbContext context,
            object state)
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
                (object)null, DeleteDatabaseOnceAsync, null, cancellationToken);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async Task<bool> DeleteDatabaseOnceAsync(
            DbContext _,
            object __,
            CancellationToken cancellationToken = default)
        {
            var response = await Client.GetDatabase(_databaseId).DeleteStreamAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

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
            var response = await Client.GetDatabase(_databaseId).CreateContainerIfNotExistsAsync(
                new ContainerProperties(parameters.ContainerId, "/" + parameters.PartitionKey)
                {
                    PartitionKeyDefinitionVersion = PartitionKeyDefinitionVersion.V2
                }, cancellationToken: cancellationToken)
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
            string partitionKey)
            => _executionStrategyFactory.Create().Execute(
                (containerId, document, partitionKey), CreateItemOnce, null);

        private bool CreateItemOnce(
            DbContext context,
            (string ContainerId, JToken Document, string PartitionKey) parameters)
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
            string partitionKey,
            CancellationToken cancellationToken = default)
            => _executionStrategyFactory.Create().ExecuteAsync(
                (containerId, document, partitionKey), CreateItemOnceAsync, null, cancellationToken);

        private async Task<bool> CreateItemOnceAsync(
            DbContext _,
            (string ContainerId, JToken Document, string PartitionKey) parameters,
            CancellationToken cancellationToken = default)
        {
            await using (var stream = new MemoryStream())
            await using (var writer = new StreamWriter(stream, new UTF8Encoding(), bufferSize: 1024, leaveOpen: false))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                JsonSerializer.Create().Serialize(jsonWriter, parameters.Document);
                await jsonWriter.FlushAsync(cancellationToken);

                var container = Client.GetDatabase(_databaseId).GetContainer(parameters.ContainerId);
                var partitionKey = CreatePartitionKey(parameters.PartitionKey);
                using (var response = await container.CreateItemStreamAsync(stream, partitionKey, null, cancellationToken))
                {
                    return response.StatusCode == HttpStatusCode.Created;
                }
            }
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
            string partitionKey)
            => _executionStrategyFactory.Create().Execute(
                (collectionId, documentId, document, partitionKey), ReplaceItemOnce, null);

        private bool ReplaceItemOnce(
            DbContext context,
            (string ContainerId, string ItemId, JObject Document, string PartitionKey) parameters)
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
            string partitionKey,
            CancellationToken cancellationToken = default)
            => _executionStrategyFactory.Create().ExecuteAsync(
                (collectionId, documentId, document, partitionKey), ReplaceItemOnceAsync, null, cancellationToken);

        private async Task<bool> ReplaceItemOnceAsync(
            DbContext _,
            (string ContainerId, string ItemId, JObject Document, string PartitionKey) parameters,
            CancellationToken cancellationToken = default)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, new UTF8Encoding(), bufferSize: 1024, leaveOpen: false);
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                JsonSerializer.Create().Serialize(jsonWriter, parameters.Document);
                await jsonWriter.FlushAsync(cancellationToken);

                var container = Client.GetDatabase(_databaseId).GetContainer(parameters.ContainerId);
                var partitionKey = CreatePartitionKey(parameters.PartitionKey);
                using (var response = await container.ReplaceItemStreamAsync(
                     stream, parameters.ItemId, partitionKey, null, cancellationToken))
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
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
            string partitionKey)
            => _executionStrategyFactory.Create().Execute(
                (containerId, documentId, partitionKey), DeleteItemOnce, null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool DeleteItemOnce(
            DbContext context,
            (string ContainerId, string DocumentId, string PartitionKey) parameters)
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
            string partitionKey,
            CancellationToken cancellationToken = default)
            => _executionStrategyFactory.Create().ExecuteAsync(
                (containerId, documentId, partitionKey), DeleteItemOnceAsync, null, cancellationToken);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async Task<bool> DeleteItemOnceAsync(
            DbContext _,
            (string ContainerId, string DocumentId, string PartitionKey) parameters,
            CancellationToken cancellationToken = default)
        {
            var items = Client.GetDatabase(_databaseId).GetContainer(parameters.ContainerId);
            var partitionKey = CreatePartitionKey(parameters.PartitionKey);
            using (var response = await items.DeleteItemStreamAsync(parameters.DocumentId, partitionKey, cancellationToken: cancellationToken))
            {
                return response.StatusCode == HttpStatusCode.NoContent;
            }
        }

        private PartitionKey CreatePartitionKey(string partitionKey)
            => partitionKey == null
                    ? PartitionKey.None
                    : new PartitionKey(partitionKey);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<JObject> ExecuteSqlQuery(
            string containerId,
            [NotNull] CosmosSqlQuery query)
        {
            _commandLogger.ExecutingSqlQuery(query);

            return new DocumentEnumerable(this, containerId, query);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IAsyncEnumerable<JObject> ExecuteSqlQueryAsync(
            string containerId,
            [NotNull] CosmosSqlQuery query)
        {
            _commandLogger.ExecutingSqlQuery(query);

            return new DocumentAsyncEnumerable(this, containerId, query);
        }

        private FeedIterator CreateQuery(
            string containerId,
            CosmosSqlQuery query)
        {
            var container = Client.GetDatabase(_databaseId).GetContainer(containerId);
            var queryDefinition = new QueryDefinition(query.Query);
            foreach (var parameter in query.Parameters)
            {
                queryDefinition = queryDefinition.WithParameter(parameter.Name, parameter.Value);
            }

            return container.GetItemQueryStreamIterator(queryDefinition);
        }

        private class DocumentEnumerable : IEnumerable<JObject>
        {
            private readonly CosmosClientWrapper _cosmosClient;
            private readonly string _containerId;
            private readonly CosmosSqlQuery _cosmosSqlQuery;

            public DocumentEnumerable(
                CosmosClientWrapper cosmosClient,
                string containerId,
                CosmosSqlQuery cosmosSqlQuery)
            {
                _cosmosClient = cosmosClient;
                _containerId = containerId;
                _cosmosSqlQuery = cosmosSqlQuery;
            }

            public IEnumerator<JObject> GetEnumerator() => new Enumerator(this);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private class Enumerator : IEnumerator<JObject>
            {
                private FeedIterator _query;
                private Stream _responseStream;
                private StreamReader _reader;
                private JsonTextReader _jsonReader;
                private readonly CosmosClientWrapper _cosmosClient;
                private readonly string _containerId;
                private readonly CosmosSqlQuery _cosmosSqlQuery;

                public Enumerator(DocumentEnumerable documentEnumerable)
                {
                    _cosmosClient = documentEnumerable._cosmosClient;
                    _containerId = documentEnumerable._containerId;
                    _cosmosSqlQuery = documentEnumerable._cosmosSqlQuery;
                }

                public JObject Current { get; private set; }

                object IEnumerator.Current => Current;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    if (_jsonReader == null)
                    {
                        if (_query == null)
                        {
                            _query = _cosmosClient.CreateQuery(_containerId, _cosmosSqlQuery);
                        }

                        if (!_query.HasMoreResults)
                        {
                            Current = default;
                            return false;
                        }

                        _responseStream = _query.ReadNextAsync().GetAwaiter().GetResult().Content;
                        _reader = new StreamReader(_responseStream);
                        _jsonReader = new JsonTextReader(_reader);

                        while (_jsonReader.Read())
                        {
                            if (_jsonReader.TokenType == JsonToken.StartObject)
                            {
                                while (_jsonReader.Read())
                                {
                                    if (_jsonReader.TokenType == JsonToken.StartArray)
                                    {
                                        goto ObjectFound;
                                    }
                                }
                            }
                        }

                        ObjectFound:
                        ;
                    }

                    while (_jsonReader.Read())
                    {
                        if (_jsonReader.TokenType == JsonToken.StartObject)
                        {
                            Current = new JsonSerializer().Deserialize<JObject>(_jsonReader);

                            return true;
                        }
                    }

                    _jsonReader.Close();
                    _jsonReader = null;
                    _reader.Dispose();
                    _reader = null;
                    _responseStream.Dispose();
                    _responseStream = null;

                    return MoveNext();
                }

                public void Dispose()
                {
                    _jsonReader?.Close();
                    _jsonReader = null;
                    _reader?.Dispose();
                    _reader = null;
                    _responseStream?.Dispose();
                    _responseStream = null;
                }

                public void Reset() => throw new NotImplementedException();
            }
        }

        private class DocumentAsyncEnumerable : IAsyncEnumerable<JObject>
        {
            private readonly CosmosClientWrapper _cosmosClient;
            private readonly string _containerId;
            private readonly CosmosSqlQuery _cosmosSqlQuery;

            public DocumentAsyncEnumerable(
                CosmosClientWrapper cosmosClient,
                string containerId,
                CosmosSqlQuery cosmosSqlQuery)
            {
                _cosmosClient = cosmosClient;
                _containerId = containerId;
                _cosmosSqlQuery = cosmosSqlQuery;
            }

            public IAsyncEnumerator<JObject> GetAsyncEnumerator(CancellationToken cancellationToken = default)
                => new AsyncEnumerator(this, cancellationToken);

            private class AsyncEnumerator : IAsyncEnumerator<JObject>
            {
                private FeedIterator _query;
                private ResponseMessage _responseMessage;
                private Stream _responseStream;
                private StreamReader _reader;
                private JsonTextReader _jsonReader;
                private readonly CosmosClientWrapper _cosmosClient;
                private readonly string _containerId;
                private readonly CosmosSqlQuery _cosmosSqlQuery;
                private readonly CancellationToken _cancellationToken;

                public AsyncEnumerator(DocumentAsyncEnumerable documentEnumerable, CancellationToken cancellationToken)
                {
                    _cosmosClient = documentEnumerable._cosmosClient;
                    _containerId = documentEnumerable._containerId;
                    _cosmosSqlQuery = documentEnumerable._cosmosSqlQuery;
                    _cancellationToken = cancellationToken;
                }

                public JObject Current { get; private set; }


                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public async ValueTask<bool> MoveNextAsync()
                {
                    _cancellationToken.ThrowIfCancellationRequested();

                    if (_jsonReader == null)
                    {
                        if (_query == null)
                        {
                            _query = _cosmosClient.CreateQuery(_containerId, _cosmosSqlQuery);
                        }

                        if (!_query.HasMoreResults)
                        {
                            Current = default;
                            return false;
                        }

                        _responseMessage = await _query.ReadNextAsync(_cancellationToken);
                        _responseStream = _responseMessage.Content;
                        _reader = new StreamReader(_responseStream);
                        _jsonReader = new JsonTextReader(_reader);

                        while (_jsonReader.Read())
                        {
                            if (_jsonReader.TokenType == JsonToken.StartObject)
                            {
                                while (_jsonReader.Read())
                                {
                                    if (_jsonReader.TokenType == JsonToken.StartArray)
                                    {
                                        goto ObjectFound;
                                    }
                                }
                            }
                        }

                        ObjectFound:
                        ;
                    }

                    while (_jsonReader.Read())
                    {
                        if (_jsonReader.TokenType == JsonToken.StartObject)
                        {
                            Current = new JsonSerializer().Deserialize<JObject>(_jsonReader);
                            return true;
                        }
                    }

                    await DisposeAsync();

                    return await MoveNextAsync();
                }

                public async ValueTask DisposeAsync()
                {
                    _jsonReader?.Close();
                    _jsonReader = null;
                    await _reader.DisposeAsyncIfAvailable();
                    _reader = null;
                    await _responseStream.DisposeAsync();
                    _responseStream = null;
                    await _responseMessage.DisposeAsyncIfAvailable();
                    _responseMessage = null;
                }
            }
        }
    }
}
