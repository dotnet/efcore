// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Interfaces;
using Microsoft.Data.Entity.Query;
using Microsoft.WindowsAzure.Storage.Table;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    // TODO Add server-side eval of Take and Project
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

        protected ITableQuery GetTableQuery(IQuerySource key)
        {
            return _queriesBySource[key];
        }

        protected bool TryGetTableQuery(IQuerySource key, out ITableQuery query)
        {
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
                var queryConstructor = typeof(TableQuery<>).MakeGenericType(entityType.Type).GetConstructor(new Type[] { });
                if (queryConstructor == null)
                {
                    throw new NullReferenceException("Could not create table query for this element type");
                }
                var query = (ITableQuery)queryConstructor.Invoke(new object[] { });
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
                    return Expression.MakeBinary(expression.NodeType, VisitExpression(expression.Left), VisitExpression(expression.Right));
                }

                return base.VisitBinaryExpression(expression);
            }

            public static bool CanReduceBinaryExpression(BinaryExpression expression)
            {
                return expression.NodeType == ExpressionType.AndAlso;
            }
        }

        private static readonly MethodInfo _queryMethod = typeof(AzureTableStorageQueryModelVisitor).GetMethod("RunTableQuery", BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly MethodInfo _entityScanMethodInfo
            = typeof(AzureTableStorageQueryModelVisitor).GetTypeInfo().GetDeclaredMethod("EntityScan");

        private static IEnumerable<TEntity> EntityScan<TEntity>(QueryContext queryContext, ITableQuery tableQuery)
        {
            var type = queryContext.Model.GetEntityType(tableQuery.ResultType);
            var table = ((AzureTableStorageQueryContext)queryContext).Database.GetTableReference(type.StorageName);

            // TODO Need to cache generic method on entity type
            var queryMethod = _queryMethod.MakeGenericMethod(typeof(TEntity));
            var result = ((IEnumerable<TEntity>)queryMethod.Invoke(null, new object[] { table, tableQuery })).ToList();

            // TODO This should go thru stateManager.GetOrMaterializeEntry to get shadow state, identity resolution, etc.
            // TODO This should happen lazily as results are enumerated
            foreach (var item in result)
            {
                var entry = queryContext.StateManager.GetOrCreateEntry(item);
                queryContext.StateManager.StartTracking(entry);
                entry.EntityState = EntityState.Unchanged;
            }

            return result;
        }

        //TODO POCO support ==> constrain result to IAzureTableEntity
        private static IEnumerable<TResult> RunTableQuery<TResult>([NotNull] ICloudTable table, [NotNull] TableQuery<TResult> query)
            where TResult : ITableEntity, new()
        {
            return table.ExecuteQuery<TResult>(query.ToExecutableQuery());
        }
    }
}
