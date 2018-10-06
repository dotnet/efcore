// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Expressions.Internal
{
    public class DocumentQueryExpression : Expression
    {
        private readonly bool _async;
        private readonly string _collectionId;

        public DocumentQueryExpression(bool async, string collectionId, SelectExpression selectExpression)
        {
            _async = async;
            _collectionId = collectionId;
            SelectExpression = selectExpression;
        }

        public SelectExpression SelectExpression { get; }

        public override bool CanReduce => true;

        public override Expression Reduce()
            => Call(
                _async ? _queryAsyncMethodInfo : _queryMethodInfo,
                EntityQueryModelVisitor.QueryContextParameter,
                Constant(_collectionId),
                Constant(SelectExpression));

        private static readonly MethodInfo _queryMethodInfo
            = typeof(DocumentQueryExpression).GetTypeInfo().GetDeclaredMethod(nameof(_Query));

        [UsedImplicitly]
        private static IEnumerable<JObject> _Query(
            QueryContext queryContext,
            string collectionId,
            SelectExpression selectExpression)
            => ((CosmosQueryContext)queryContext).CosmosClient.ExecuteSqlQuery(
                collectionId,
                selectExpression.ToSqlQuery(queryContext.ParameterValues));

        private static readonly MethodInfo _queryAsyncMethodInfo
            = typeof(DocumentQueryExpression).GetTypeInfo().GetDeclaredMethod(nameof(_QueryAsync));

        [UsedImplicitly]
        private static IAsyncEnumerable<JObject> _QueryAsync(
            QueryContext queryContext,
            string collectionId,
            SelectExpression selectExpression)
            => ((CosmosQueryContext)queryContext).CosmosClient.ExecuteSqlQueryAsync(
                collectionId,
                selectExpression.ToSqlQuery(queryContext.ParameterValues));

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        public override Type Type
            => (_async ? typeof(IAsyncEnumerable<>) : typeof(IEnumerable<>)).MakeGenericType(typeof(JObject));

        public override ExpressionType NodeType => ExpressionType.Extension;
    }
}
