// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Linq;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Storage.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.Expressions.Internal
{
    public class DocumentQueryExpression : Expression
    {
        private readonly bool _async;
        private readonly string _collectionId;
        private readonly CosmosClient _cosmosClient;

        public DocumentQueryExpression(bool async, string collectionId, SelectExpression selectExpression, CosmosClient cosmosClient)
        {
            _async = async;
            _collectionId = collectionId;
            SelectExpression = selectExpression;
            _cosmosClient = cosmosClient;
        }

        public SelectExpression SelectExpression { get; }

        public override bool CanReduce => true;

        public override Expression Reduce()
            => Call(
                _async ? _queryAsyncMethodInfo : _queryMethodInfo,
                Constant(_cosmosClient),
                EntityQueryModelVisitor.QueryContextParameter,
                Constant(_collectionId),
                Constant(SelectExpression));

        private static readonly MethodInfo _queryMethodInfo
            = typeof(DocumentQueryExpression).GetTypeInfo().GetDeclaredMethod(nameof(_Query));

        [UsedImplicitly]
        private static IEnumerable<JObject> _Query(
            CosmosClient cosmosClient,
            QueryContext queryContext,
            string collectionId,
            SelectExpression selectExpression)
            => new DocumentEnumerable(cosmosClient, queryContext, collectionId, selectExpression);

        private static readonly MethodInfo _queryAsyncMethodInfo
            = typeof(DocumentQueryExpression).GetTypeInfo().GetDeclaredMethod(nameof(_QueryAsync));

        [UsedImplicitly]
        private static IAsyncEnumerable<JObject> _QueryAsync(
            CosmosClient cosmosClient,
            QueryContext queryContext,
            string collectionId,
            SelectExpression selectExpression)
            => new DocumentAsyncEnumerable(cosmosClient, queryContext, collectionId, selectExpression);

        private class DocumentEnumerable : IEnumerable<JObject>
        {
            private readonly CosmosClient _cosmosClient;
            private readonly QueryContext _queryContext;
            private readonly string _collectionId;
            private readonly SelectExpression _selectExpression;

            public DocumentEnumerable(
                CosmosClient cosmosClient, QueryContext queryContext, string collectionId, SelectExpression selectExpression)
            {
                _cosmosClient = cosmosClient;
                _queryContext = queryContext;
                _collectionId = collectionId;
                _selectExpression = selectExpression;
            }

            public IEnumerator<JObject> GetEnumerator() => new Enumerator(this);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private class Enumerator : IEnumerator<JObject>
            {
                private IEnumerator<Document> _underlyingEnumerator;
                private readonly CosmosClient _cosmosClient;
                private readonly QueryContext _queryContext;
                private readonly string _collectionId;
                private readonly SelectExpression _selectExpression;

                public Enumerator(DocumentEnumerable documentEnumerable)
                {
                    _cosmosClient = documentEnumerable._cosmosClient;
                    _queryContext = documentEnumerable._queryContext;
                    _collectionId = documentEnumerable._collectionId;
                    _selectExpression = documentEnumerable._selectExpression;
                }

                public JObject Current { get; private set; }

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                    _underlyingEnumerator?.Dispose();
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    if (_underlyingEnumerator == null)
                    {
                        _underlyingEnumerator = _cosmosClient.ExecuteSqlQuery(
                            _collectionId,
                            _selectExpression.ToSqlQuery(_queryContext.ParameterValues));
                    }

                    var hasNext = _underlyingEnumerator.MoveNext();

                    Current = hasNext
                        ? _underlyingEnumerator.Current.GetPropertyValue<JObject>("query")
                        : default;

                    return hasNext;
                }

                public void Reset() => throw new NotImplementedException();
            }
        }

        private class DocumentAsyncEnumerable : IAsyncEnumerable<JObject>
        {
            private readonly CosmosClient _cosmosClient;
            private readonly QueryContext _queryContext;
            private readonly string _collectionId;
            private readonly SelectExpression _selectExpression;

            public DocumentAsyncEnumerable(
                CosmosClient cosmosClient, QueryContext queryContext, string collectionId, SelectExpression selectExpression)
            {
                _cosmosClient = cosmosClient;
                _queryContext = queryContext;
                _collectionId = collectionId;
                _selectExpression = selectExpression;
            }

            public IAsyncEnumerator<JObject> GetEnumerator() => new AsyncEnumerator(this);

            IAsyncEnumerator<JObject> IAsyncEnumerable<JObject>.GetEnumerator() => GetEnumerator();

            private class AsyncEnumerator : IAsyncEnumerator<JObject>
            {
                private IDocumentQuery<Document> _underlyingQuery;
                private IEnumerator<Document> _underlyingEnumerator;
                private readonly CosmosClient _cosmosClient;
                private readonly QueryContext _queryContext;
                private readonly string _collectionId;
                private readonly SelectExpression _selectExpression;

                public AsyncEnumerator(DocumentAsyncEnumerable documentEnumerable)
                {
                    _cosmosClient = documentEnumerable._cosmosClient;
                    _queryContext = documentEnumerable._queryContext;
                    _collectionId = documentEnumerable._collectionId;
                    _selectExpression = documentEnumerable._selectExpression;
                }

                public JObject Current { get; private set; }

                public void Dispose()
                {
                    _underlyingEnumerator?.Dispose();
                    _underlyingQuery?.Dispose();
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (_underlyingQuery == null)
                    {
                        _underlyingQuery = _cosmosClient.ExecuteAsyncSqlQuery(
                            _collectionId,
                            _selectExpression.ToSqlQuery(_queryContext.ParameterValues));
                    }

                    if (_underlyingEnumerator == null)
                    {
                        if (_underlyingQuery.HasMoreResults)
                        {
                            _underlyingEnumerator = (await _underlyingQuery.ExecuteNextAsync<Document>(cancellationToken))
                                .GetEnumerator();
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
                        Current = _underlyingEnumerator.Current.GetPropertyValue<JObject>("query");
                        return true;
                    }
                    else
                    {
                        _underlyingEnumerator.Dispose();
                        _underlyingEnumerator = null;
                        return await MoveNext(cancellationToken);
                    }
                }

                public void Reset() => throw new NotImplementedException();
            }
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        public override Type Type
            => (_async ? typeof(IAsyncEnumerable<>) : typeof(IEnumerable<>)).MakeGenericType(typeof(JObject));

        public override ExpressionType NodeType => ExpressionType.Extension;
    }
}
