// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Interfaces;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Query;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    public class AzureTableStorageQueryModelVisitor : EntityQueryModelVisitor
    {
        private readonly Dictionary<IQuerySource, ITableQuery> _queriesBySource = new Dictionary<IQuerySource, ITableQuery>();

        public AzureTableStorageQueryModelVisitor([NotNull] AzureTableStorageQueryCompilationContext queryCompilationContext)
            : base(queryCompilationContext)
        {
        }

        private AzureTableStorageQueryModelVisitor(AzureTableStorageQueryModelVisitor visitor)
            : base(visitor.QueryCompilationContext)
        {
        }

        public ITableQuery GetTableQuery([NotNull] IQuerySource key)
        {
            Check.NotNull(key, "key");
            return _queriesBySource[key];
        }

        public bool TryGetTableQuery([NotNull] IQuerySource key, out ITableQuery query)
        {
            Check.NotNull(key, "key");
            return _queriesBySource.TryGetValue(key, out query);
        }

        protected override ExpressionTreeVisitor CreateQueryingExpressionTreeVisitor(IQuerySource querySource)
        {
            return new AzureTableStorageQueryingExpressionTreeVisitor(this, querySource);
        }

        protected class AzureTableStorageQueryingExpressionTreeVisitor : QueryingExpressionTreeVisitor
        {
            private readonly AzureTableStorageQueryModelVisitor _parent;
            private readonly IQuerySource _querySource;

            public AzureTableStorageQueryingExpressionTreeVisitor(AzureTableStorageQueryModelVisitor parent, IQuerySource querySource)
                : base(parent.QueryCompilationContext)
            {
                _parent = parent;
                _querySource = querySource;
            }

            protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
            {
                var visitor = new AzureTableStorageQueryModelVisitor(_parent);
                visitor.VisitQueryModel(expression.QueryModel);
                return visitor.Expression;
            }

            protected override Expression VisitEntityQueryable(Type elementType)
            {
                var entityType = _parent.QueryCompilationContext.Model.GetEntityType(elementType);
                var adapterType = typeof(PocoTableEntityAdapter<>).MakeGenericType(entityType.Type);
                var queryConstructor = typeof(TableQuery<>).MakeGenericType(adapterType).GetConstructor(Type.EmptyTypes);
                if (queryConstructor == null)
                {
                    throw new NullReferenceException("Could not create table query for this element type");
                }
                var query = (ITableQuery)queryConstructor.Invoke(null);
                _parent._queriesBySource[_querySource] = query;
                return Expression.Call(
                    _entityScanMethodInfo.MakeGenericMethod(elementType),
                    QueryContextParameter,
                    Expression.Constant(query)
                    );
            }

            protected override Expression VisitUnaryExpression(UnaryExpression expression)
            {
                var filter = TableFilter.FromUnaryExpression(expression);
                ITableQuery query;
                if (filter != null
                    && _parent._queriesBySource.TryGetValue(_querySource, out query))
                {
                    query.WithFilter(filter);
                    return expression;
                }
                return base.VisitUnaryExpression(expression);
            }

            protected override Expression VisitMemberExpression(MemberExpression expression)
            {
                var filter = TableFilter.FromMemberExpression(expression);
                ITableQuery query;
                if (filter != null
                    && _parent._queriesBySource.TryGetValue(_querySource, out query))
                {
                    //TODO replace expression
                    query.WithFilter(filter);
                }
                return base.VisitMemberExpression(expression);
            }

            protected override Expression VisitBinaryExpression(BinaryExpression expression)
            {
                var filter = TableFilter.FromBinaryExpression(expression);
                ITableQuery query;
                if (filter != null
                    && _parent._queriesBySource.TryGetValue(_querySource, out query))
                {
                    query.WithFilter(filter);
                }
                if (CanReduceBinaryExpression(expression))
                {
                    //TODO add support composite logical statements
                    return Expression.MakeBinary(expression.NodeType, VisitExpression(expression.Left), VisitExpression(expression.Right));
                }

                //TODO replace expression
                return base.VisitBinaryExpression(expression);
            }

            public static bool CanReduceBinaryExpression(BinaryExpression expression)
            {
                return expression.NodeType == ExpressionType.AndAlso;
            }
        }

        private static readonly MethodInfo _entityScanMethodInfo
            = typeof(AzureTableStorageQueryModelVisitor).GetTypeInfo().GetDeclaredMethod("EntityScan");

        [UsedImplicitly]
        private static IEnumerable<TResult> EntityScan<TResult>(QueryContext queryContext, TableQuery<PocoTableEntityAdapter<TResult>> tableQuery)
            where TResult : class, new()
        {
            var type = queryContext.Model.GetEntityType(tableQuery.ResultType.GetGenericArguments()[0]);
            var table = ((AzureTableStorageQueryContext)queryContext).Database.GetTableReference(type.StorageName);

            return table.ExecuteQuery(tableQuery.ToExecutableQuery()).Select(item =>
                {
                    //TODO use GetOrMaterialize instead -> support ShadowState
                    var entry = queryContext.StateManager.GetOrCreateEntry(item.ClrInstance);
                    queryContext.StateManager.StartTracking(entry);
                    entry.EntityState = EntityState.Unchanged;
                    return item.ClrInstance;
                });
        }
    }
}
