// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal
{
    public class CosmosClient : IDisposable
    {
        private readonly string _databaseId;
        private readonly Uri _baseUri;
        private readonly string _masterKey;
        private readonly IExecutionStrategyFactory _executionStrategyFactory;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> _commandLogger;
        private readonly HttpClient _client;
        private static readonly string _userAgent = "Microsoft.EntityFrameworkCore.Cosmos/" + ProductInfo.GetVersion();

        public CosmosClient(
            [NotNull] IDbContextOptions dbContextOptions,
            [NotNull] IExecutionStrategyFactory executionStrategyFactory,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger)
        {
            var options = dbContextOptions.FindExtension<CosmosDbOptionsExtension>();

            _databaseId = options.DatabaseName;
            _baseUri = options.ServiceEndPoint;
            _masterKey = options.AuthKeyOrResourceToken;
            _executionStrategyFactory = executionStrategyFactory;
            _commandLogger = commandLogger;

            _client = new HttpClient
            {
                // TODO: configure in options
                Timeout = new TimeSpan(0, 0, 60)
            };

            WebRequest.DefaultWebProxy = new WebProxy();
        }

        public bool CreateDatabaseIfNotExists()
            => _executionStrategyFactory.Create().Execute(
                (object)null, CreateDatabaseIfNotExistsOnce, null);

        public bool CreateDatabaseIfNotExistsOnce(
            DbContext _,
            object __)
        {
            var request = CreateHttpWebRequest(HttpMethod.Post, resourceId: "", resourceType: "dbs", uriId: "dbs");

            request.ContentType = "application/json";
            WriteJson(new
            {
                id = _databaseId
            }, request);

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Conflict)
                {
                    return false;
                }
                throw;
            }

            return response.StatusCode == HttpStatusCode.Created;
        }

        public Task<bool> CreateDatabaseIfNotExistsAsync(
            CancellationToken cancellationToken = default)
            => _executionStrategyFactory.Create().ExecuteAsync(
                (object)null, CreateDatabaseIfNotExistsOnceAsync, null, cancellationToken);

        public async Task<bool> CreateDatabaseIfNotExistsOnceAsync(
            DbContext _,
            object __,
            CancellationToken cancellationToken = default)
        {
            var request = CreateHttpRequestMessage(HttpMethod.Post, resourceId: "", resourceType: "dbs", uriId: "dbs");
            request.Content = new JsonStringContent(new
            {
                id = _databaseId
            });

            var response = await _client.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                return false;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpException(response);
            }

            return response.StatusCode == HttpStatusCode.Created;
        }

        public bool DeleteDatabase()
            => _executionStrategyFactory.Create().Execute((object)null, DeleteDatabaseOnce, null);

        public bool DeleteDatabaseOnce(
            DbContext _,
            object __)
        {
            var resourceId = string.Format(CultureInfo.InvariantCulture, "dbs/{0}", _databaseId);
            var request = CreateHttpWebRequest(HttpMethod.Delete, resourceId, resourceType: "dbs");

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }
                throw;
            }

            return response.StatusCode == HttpStatusCode.NoContent;
        }

        public Task<bool> DeleteDatabaseAsync(
            CancellationToken cancellationToken = default)
            => _executionStrategyFactory.Create().ExecuteAsync(
                (object)null, DeleteDatabaseOnceAsync, null, cancellationToken);

        public async Task<bool> DeleteDatabaseOnceAsync(
            DbContext _,
            object __,
            CancellationToken cancellationToken = default)
        {
            var resourceId = string.Format(CultureInfo.InvariantCulture, "dbs/{0}", _databaseId);
            var request = CreateHttpRequestMessage(HttpMethod.Delete, resourceId, resourceType: "dbs");

            var response = await _client.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpException(response);
            }

            return response.StatusCode == HttpStatusCode.NoContent;
        }

        public bool CreateDocumentCollectionIfNotExists(
            string collectionId)
            => _executionStrategyFactory.Create().Execute(
                collectionId, CreateDocumentCollectionIfNotExistsOnce, null);

        public bool CreateDocumentCollectionIfNotExistsOnce(
            DbContext _,
            string collectionId)
        {
            var resourceId = string.Format(CultureInfo.InvariantCulture, "dbs/{0}", _databaseId);
            var request = CreateHttpWebRequest(HttpMethod.Post, resourceId, resourceType: "colls", uriId: "/colls");
            WriteJson(new
            {
                id = collectionId
            }, request);

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Conflict)
                {
                    return false;
                }
                throw;
            }

            return response.StatusCode == HttpStatusCode.Created;
        }

        public Task<bool> CreateDocumentCollectionIfNotExistsAsync(
            string collectionId,
            CancellationToken cancellationToken = default)
            => _executionStrategyFactory.Create().ExecuteAsync(
                collectionId, CreateDocumentCollectionIfNotExistsOnceAsync, null, cancellationToken);

        public async Task<bool> CreateDocumentCollectionIfNotExistsOnceAsync(
            DbContext _,
            string collectionId,
            CancellationToken cancellationToken = default)
        {
            var resourceId = string.Format(CultureInfo.InvariantCulture, "dbs/{0}", _databaseId);
            var request = CreateHttpRequestMessage(HttpMethod.Post, resourceId, resourceType: "colls", uriId: "/colls");
            request.Content = new JsonStringContent(new
            {
                id = collectionId
            });

            var response = await _client.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                return false;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpException(response);
            }

            return response.StatusCode == HttpStatusCode.Created;
        }

        public bool CreateDocument(
            string collectionId,
            JToken document)
            => _executionStrategyFactory.Create().Execute(
                (collectionId, document), CreateDocumentOnce, null);

        public bool CreateDocumentOnce(
            DbContext _,
            (string, JToken) parameters)
        {
            var (collectionId, document) = parameters;

            var resourceId = string.Format(CultureInfo.InvariantCulture, "dbs/{0}/colls/{1}", _databaseId, collectionId);
            var request = CreateHttpWebRequest(HttpMethod.Post, resourceId, resourceType: "docs", uriId: "/docs");
            WriteJson(document, request);
            request.Accept = "application/json";

            var response = (HttpWebResponse)request.GetResponse();

            return response.StatusCode == HttpStatusCode.Created;
        }

        public Task<bool> CreateDocumentAsync(
            string collectionId,
            JToken document,
            CancellationToken cancellationToken = default)
            => _executionStrategyFactory.Create().ExecuteAsync(
                (collectionId, document), CreateDocumentOnceAsync, null, cancellationToken);

        public async Task<bool> CreateDocumentOnceAsync(
            DbContext _,
            (string, JToken) parameters,
            CancellationToken cancellationToken = default)
        {
            var (collectionId, document) = parameters;

            var resourceId = string.Format(CultureInfo.InvariantCulture, "dbs/{0}/colls/{1}", _databaseId, collectionId);
            var request = CreateHttpRequestMessage(HttpMethod.Post, resourceId, resourceType: "docs", uriId: "/docs");
            request.Content = new JsonContent(document);
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpException(response);
            }

            return response.StatusCode == HttpStatusCode.Created;
        }

        public bool ReplaceDocument(
            string collectionId,
            string documentId,
            JObject document)
            => _executionStrategyFactory.Create().Execute(
                (collectionId, documentId, document), ReplaceDocumentOnce, null);

        public bool ReplaceDocumentOnce(
            DbContext _,
            (string, string, JObject) parameters)
        {
            var (collectionId, documentId, document) = parameters;

            var resourceId = string.Format(CultureInfo.InvariantCulture, "dbs/{0}/colls/{1}/docs/{2}", _databaseId, collectionId, documentId);
            var request = CreateHttpWebRequest(HttpMethod.Put, resourceId, resourceType: "docs");
            WriteJson(document, request);
            request.Accept = "application/json";

            var response = (HttpWebResponse)request.GetResponse();

            return response.StatusCode == HttpStatusCode.OK;
        }

        public Task<bool> ReplaceDocumentAsync(
            string collectionId,
            string documentId,
            JObject document,
            CancellationToken cancellationToken = default)
            => _executionStrategyFactory.Create().ExecuteAsync(
                (collectionId, documentId, document), ReplaceDocumentOnceAsync, null, cancellationToken);

        public async Task<bool> ReplaceDocumentOnceAsync(
            DbContext _,
            (string, string, JObject) parameters,
            CancellationToken cancellationToken = default)
        {
            var (collectionId, documentId, document) = parameters;

            var resourceId = string.Format(CultureInfo.InvariantCulture, "dbs/{0}/colls/{1}/docs/{2}", _databaseId, collectionId, documentId);
            var request = CreateHttpRequestMessage(HttpMethod.Put, resourceId, resourceType: "docs");
            request.Content = new JsonContent(document);
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpException(response);
            }

            return response.StatusCode == HttpStatusCode.OK;
        }

        public bool DeleteDocument(
            string collectionId,
            string documentId)
            => _executionStrategyFactory.Create().Execute(
                (collectionId, documentId), DeleteDocumentOnce, null);

        public bool DeleteDocumentOnce(
            DbContext _,
            (string, string) parameters)
        {
            var (collectionId, documentId) = parameters;

            var resourceId = string.Format(CultureInfo.InvariantCulture, "dbs/{0}/colls/{1}/docs/{2}", _databaseId, collectionId, documentId);
            var request = CreateHttpWebRequest(HttpMethod.Delete, resourceId, resourceType: "docs");

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }
                throw;
            }

            return response.StatusCode == HttpStatusCode.NoContent;
        }

        public Task<bool> DeleteDocumentAsync(
            string collectionId,
            string documentId,
            CancellationToken cancellationToken = default)
            => _executionStrategyFactory.Create().ExecuteAsync(
                (collectionId, documentId), DeleteDocumentOnceAsync, null, cancellationToken);

        public async Task<bool> DeleteDocumentOnceAsync(
            DbContext _,
            (string, string) parameters,
            CancellationToken cancellationToken = default)
        {
            var (collectionId, documentId) = parameters;

            var resourceId = string.Format(CultureInfo.InvariantCulture, "dbs/{0}/colls/{1}/docs/{2}", _databaseId, collectionId, documentId);
            var request = CreateHttpRequestMessage(HttpMethod.Delete, resourceId, resourceType: "docs");

            var response = await _client.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpException(response);
            }

            return response.StatusCode == HttpStatusCode.NoContent;
        }

        public IEnumerable<JObject> ExecuteSqlQuery(
            string collectionId,
            [NotNull] CosmosSqlQuery query)
        {
            _commandLogger.ExecutingSqlQuery(query);

            return new DocumentEnumerable(this, collectionId, query);
        }

        public IAsyncEnumerable<JObject> ExecuteSqlQueryAsync(
            string collectionId,
            [NotNull] CosmosSqlQuery query)
        {
            _commandLogger.ExecutingSqlQuery(query);

            return new DocumentAsyncEnumerable(this, collectionId, query);
        }

        private HttpWebResponse SendQuery(
            string collectionId,
            CosmosSqlQuery query,
            string continuation)
            => _executionStrategyFactory.Create().Execute(
                (collectionId, query, continuation), SendQueryOnce, null);

        private HttpWebResponse SendQueryOnce(
            DbContext _,
            (string, CosmosSqlQuery, string) parameters)
        {
            var (collectionId, query, continuation) = parameters;

            var resourceId =
                string.Format(CultureInfo.InvariantCulture, "dbs/{0}/colls/{1}", _databaseId, collectionId);

            var request = CreateHttpWebRequest(HttpMethod.Post, resourceId, resourceType: "docs", uriId: "/docs");

            request.Accept = "application/json";
            request.Headers.Add("x-ms-documentdb-isquery", "True");
            request.Headers.Add("x-ms-query-enable-crosspartition", "true");
            if (continuation != null)
            {
                request.Headers.Add("x-ms-continuation", continuation);
            }

            request.ContentType = "application/query+json";
            WriteJson(query, request);

            return (HttpWebResponse)request.GetResponse();
        }

        private Task<HttpResponseMessage> SendQueryAsync(
            string collectionId,
            CosmosSqlQuery query,
            string continuation,
            CancellationToken cancellationToken)
            => _executionStrategyFactory.Create().ExecuteAsync(
                (collectionId, query, continuation), SendQueryOnceAsync, null, cancellationToken);

        private async Task<HttpResponseMessage> SendQueryOnceAsync(
            DbContext _,
            (string, CosmosSqlQuery, string) parameters,
            CancellationToken cancellationToken)
        {
            var (collectionId, query, continuation) = parameters;

            var resourceId = string.Format(CultureInfo.InvariantCulture, "dbs/{0}/colls/{1}", _databaseId, collectionId);
            var request = CreateHttpRequestMessage(HttpMethod.Post, resourceId, resourceType: "docs", uriId: "/docs");

            request.Headers.Add("x-ms-documentdb-isquery", "True");
            request.Headers.Add("x-ms-query-enable-crosspartition", "true");
            if (continuation != null)
            {
                request.Headers.Add("x-ms-continuation", continuation);
            }

            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new CosmosQueryContent(query);

            var response = await _client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpException(response);
            }

            return response;
        }

        private HttpWebRequest CreateHttpWebRequest(HttpMethod method, string resourceId, string resourceType, string uriId = "")
        {
            var requestUri = new Uri(_baseUri, Uri.EscapeUriString(resourceId) + uriId);
            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = method.ToString();

            var date = DateTime.UtcNow;
            var dateString = date.ToString("r");
            var authHeader = GenerateMasterKeyAuthorizationSignature(method, resourceId, resourceType, dateString);

            request.Headers.Add("authorization", authHeader);
            request.Headers.Add("x-ms-date", dateString);
            request.Date = date;
            request.Headers.Add("x-ms-version", "2017-02-22");
            request.Headers.Add("User-Agent", _userAgent);
            // TODO: configure in options
            request.Timeout = (int)new TimeSpan(0, 0, 60).TotalMilliseconds;
            return request;
        }

        private void WriteJson(object obj, HttpWebRequest request)
        {
            var requestStream = request.GetRequestStream();
            using (var writer = new StreamWriter(requestStream, new UTF8Encoding()))
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    JsonSerializer.Create().Serialize(jsonWriter, obj);
                }
            }
        }

        private HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, string resourceId, string resourceType, string uriId = "")
        {
            var requestUri = new Uri(_baseUri, Uri.EscapeUriString(resourceId) + uriId);
            var request = new HttpRequestMessage
            {
                Method = method,
                RequestUri = requestUri
            };

            var date = DateTime.UtcNow;
            var dateString = date.ToString("r");
            var authHeader = GenerateMasterKeyAuthorizationSignature(method, resourceId, resourceType, dateString);
            request.Headers.Add("authorization", authHeader);
            request.Headers.Add("x-ms-date", dateString);
            request.Headers.Date = date;
            request.Headers.Add("x-ms-version", "2017-02-22");
            request.Headers.Add("User-Agent", _userAgent);
            return request;
        }

        private string GenerateMasterKeyAuthorizationSignature(
            HttpMethod method, string resourceId, string resourceType, string date)
        {
            var hmacSha256 = new System.Security.Cryptography.HMACSHA256
            {
                Key = Convert.FromBase64String(_masterKey)
            };

            var payLoad = string.Format(CultureInfo.InvariantCulture, "{0}\n{1}\n{2}\n{3}\n{4}\n",
                method.ToString().ToLowerInvariant(),
                resourceType.ToLowerInvariant(),
                resourceId,
                date.ToLowerInvariant(),
                ""
                );

            var keyType = "master";
            var tokenVersion = "1.0";
            var hashPayLoad = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(payLoad));
            var signature = Convert.ToBase64String(hashPayLoad);
            return System.Web.HttpUtility.UrlEncode(string.Format(CultureInfo.InvariantCulture, "type={0}&ver={1}&sig={2}",
                keyType,
                tokenVersion,
                signature));
        }

        private class DocumentEnumerable : IEnumerable<JObject>
        {
            private readonly CosmosClient _cosmosClient;
            private readonly string _collectionId;
            private readonly CosmosSqlQuery _cosmosSqlQuery;

            public DocumentEnumerable(
                CosmosClient cosmosClient,
                string collectionId,
                CosmosSqlQuery cosmosSqlQuery)
            {
                _cosmosClient = cosmosClient;
                _collectionId = collectionId;
                _cosmosSqlQuery = cosmosSqlQuery;
            }

            public IEnumerator<JObject> GetEnumerator() => new Enumerator(this);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private class Enumerator : IEnumerator<JObject>
            {
                private string _queryContinuation;
                private IEnumerator<JObject> _underlyingEnumerator;
                private readonly CosmosClient _cosmosClient;
                private readonly string _collectionId;
                private readonly CosmosSqlQuery _cosmosSqlQuery;

                public Enumerator(DocumentEnumerable documentEnumerable)
                {
                    _cosmosClient = documentEnumerable._cosmosClient;
                    _collectionId = documentEnumerable._collectionId;
                    _cosmosSqlQuery = documentEnumerable._cosmosSqlQuery;
                }

                public JObject Current { get; private set; }

                object IEnumerator.Current => Current;

                public void Dispose() => _underlyingEnumerator?.Dispose();

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    if (_underlyingEnumerator == null)
                    {
                        if (_queryContinuation != "")
                        {
                            var response = _cosmosClient.SendQuery(_collectionId, _cosmosSqlQuery, _queryContinuation);

                            _queryContinuation = response.Headers.GetValues("x-ms-continuation")?.Single() ?? "";

                            using (var responseStream = response.GetResponseStream())
                            {
                                using (var reader = new StreamReader(responseStream))
                                {
                                    using (var jsonReader = new JsonTextReader(reader))
                                    {
                                        _underlyingEnumerator = new JsonSerializer().Deserialize<JObject>(jsonReader)
                                            ["Documents"].ToObject<List<JObject>>().GetEnumerator();
                                    }
                                }
                            }

                            response.Dispose();
                        }
                        else
                        {
                            Current = default;
                            return false;
                        }
                    }

                    var hasNext = _underlyingEnumerator.MoveNext();
                    if (hasNext)
                    {
                        Current = (JObject)_underlyingEnumerator.Current.First.First;
                        return true;
                    }

                    _underlyingEnumerator.Dispose();
                    _underlyingEnumerator = null;
                    return MoveNext();
                }

                public void Reset() => throw new NotImplementedException();
            }
        }

        private class DocumentAsyncEnumerable : IAsyncEnumerable<JObject>
        {
            private readonly CosmosClient _cosmosClient;
            private readonly string _collectionId;
            private readonly CosmosSqlQuery _cosmosSqlQuery;

            public DocumentAsyncEnumerable(
                CosmosClient cosmosClient,
                string collectionId,
                CosmosSqlQuery cosmosSqlQuery)
            {
                _cosmosClient = cosmosClient;
                _collectionId = collectionId;
                _cosmosSqlQuery = cosmosSqlQuery;
            }

            public IAsyncEnumerator<JObject> GetEnumerator() => new AsyncEnumerator(this);

            private class AsyncEnumerator : IAsyncEnumerator<JObject>
            {
                private string _queryContinuation;
                private IEnumerator<JObject> _underlyingEnumerator;
                private readonly CosmosClient _cosmosClient;
                private readonly string _collectionId;
                private readonly CosmosSqlQuery _cosmosSqlQuery;

                public AsyncEnumerator(DocumentAsyncEnumerable documentEnumerable)
                {
                    _cosmosClient = documentEnumerable._cosmosClient;
                    _collectionId = documentEnumerable._collectionId;
                    _cosmosSqlQuery = documentEnumerable._cosmosSqlQuery;
                }

                public JObject Current { get; private set; }

                public void Dispose() => _underlyingEnumerator?.Dispose();

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (_underlyingEnumerator == null)
                    {
                        if (_queryContinuation != "")
                        {
                            var response = await _cosmosClient.SendQueryAsync(
                                _collectionId, _cosmosSqlQuery, _queryContinuation, cancellationToken);

                            _queryContinuation = response.Headers.TryGetValues("x-ms-continuation", out var values) ? values.Single() : "";

                            using (var responseStream = await response.Content.ReadAsStreamAsync())
                            {
                                using (var reader = new StreamReader(responseStream))
                                {
                                    using (var jsonReader = new JsonTextReader(reader))
                                    {
                                        _underlyingEnumerator = new JsonSerializer().Deserialize<JObject>(jsonReader)
                                            ["Documents"].ToObject<List<JObject>>().GetEnumerator();
                                    }
                                }
                            }

                            response.RequestMessage.Dispose();
                            response.Dispose();
                        }
                        else
                        {
                            Current = default;
                            return false;
                        }
                    }

                    var hasNext = _underlyingEnumerator.MoveNext();
                    if (hasNext)
                    {
                        Current = (JObject)_underlyingEnumerator.Current.First.First;
                        return true;
                    }

                    _underlyingEnumerator.Dispose();
                    _underlyingEnumerator = null;
                    return await MoveNext(cancellationToken);
                }

                public void Reset() => throw new NotImplementedException();
            }
        }

        public void Dispose() => _client.Dispose();
    }
}
