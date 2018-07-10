// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Storage.Internal;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.Expressions.Internal
{
    public class DocumentQueryExpression : Expression
    {
        private readonly bool _async;
        private readonly string _collectionId;
        private readonly SelectExpression _selectExpression;
        private readonly CosmosClient _cosmosClient;

        public DocumentQueryExpression(bool async, string collectionId, SelectExpression selectExpression, CosmosClient cosmosClient)
        {
            _async = async;
            _collectionId = collectionId;
            _selectExpression = selectExpression;
            _cosmosClient = cosmosClient;
        }

        public SelectExpression SelectExpression => _selectExpression;

        public override bool CanReduce => true;

        // TODO: Reduce based on sync/async
        public override Expression Reduce()
            => Call(
                typeof(DocumentQueryExpression).GetTypeInfo().GetDeclaredMethod(nameof(_Query)),
                Constant(_cosmosClient),
                Constant(_collectionId),
                Constant(_selectExpression));

        private static IEnumerable<JObject> _Query(
            CosmosClient cosmosClient,
            string collectionId,
            SelectExpression selectExpression)
            => new DocumentEnumerable(cosmosClient, collectionId, selectExpression);

        private class DocumentEnumerable : IEnumerable<JObject>
        {
            private readonly CosmosClient _cosmosClient;
            private readonly string _collectionId;
            private readonly SelectExpression _selectExpression;

            public DocumentEnumerable(CosmosClient cosmosClient, string collectionId, SelectExpression selectExpression)
            {
                _cosmosClient = cosmosClient;
                _collectionId = collectionId;
                _selectExpression = selectExpression;
            }

            public IEnumerator<JObject> GetEnumerator() => new Enumerator(this);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private class Enumerator : IEnumerator<JObject>
            {
                private IEnumerator<Document> _underlyingEnumerator;
                private readonly CosmosClient _cosmosClient;
                private readonly string _collectionId;
                private readonly SelectExpression _selectExpression;

                public Enumerator(DocumentEnumerable documentEnumerable)
                {
                    _cosmosClient = documentEnumerable._cosmosClient;
                    _collectionId = documentEnumerable._collectionId;
                    _selectExpression = documentEnumerable._selectExpression;
                }

                public JObject Current { get; private set; }

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                    _underlyingEnumerator.Dispose();
                }

                public bool MoveNext()
                {
                    if (_underlyingEnumerator == null)
                    {
                        _underlyingEnumerator = _cosmosClient.ExecuteSqlQuery(
                            _collectionId,
                            new SqlQuerySpec(_selectExpression.ToString()));
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

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        public override Type Type
            => (_async ? typeof(IAsyncEnumerable<>) : typeof(IEnumerable<>)).MakeGenericType(typeof(JObject));

        public override ExpressionType NodeType => ExpressionType.Extension;
    }
}
