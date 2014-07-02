// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Requests;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    public class AtsQueryModelVisitor : EntityQueryModelVisitor
    {
        private readonly Dictionary<IQuerySource, AtsTableQuery> _queriesBySource = new Dictionary<IQuerySource, AtsTableQuery>();
        private readonly AtsQueryCompilationContext _queryCompilationContext;

        public AtsQueryModelVisitor([NotNull] AtsQueryCompilationContext queryCompilationContext)
            : base(queryCompilationContext)
        {
            _queryCompilationContext = queryCompilationContext;
        }

        private AtsQueryModelVisitor(AtsQueryModelVisitor visitor)
            : this(visitor._queryCompilationContext)
        {
        }

        [NotNull]
        public virtual AtsTableQuery GetTableQuery([NotNull] IQuerySource key)
        {
            Check.NotNull(key, "key");
            return _queriesBySource[key];
        }

        public virtual bool TryGetTableQuery([NotNull] IQuerySource key, [CanBeNull] out AtsTableQuery query)
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
            private readonly AtsQueryModelVisitor _parent;
            private readonly IQuerySource _querySource;
            private readonly IEntityType _entityType;

            public AzureTableStorageQueryingExpressionTreeVisitor(AtsQueryModelVisitor parent, IQuerySource querySource)
                : base(parent.QueryCompilationContext)
            {
                _parent = parent;
                _querySource = querySource;
                _entityType = _parent._queryCompilationContext.Model.TryGetEntityType(_querySource.ItemType);
            }

            protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
            {
                var visitor = new AtsQueryModelVisitor(_parent);
                visitor.VisitQueryModel(expression.QueryModel);
                return visitor.Expression;
            }

            protected override Expression VisitEntityQueryable(Type elementType)
            {
                var query = new AtsTableQuery();
                _parent._queriesBySource[_querySource] = query;

                var entityType = _parent.QueryCompilationContext.Model.GetEntityType(elementType);

                return Expression.Call(
                    _entityScanMethodInfo.MakeGenericMethod(elementType),
                    QueryContextParameter,
                    Expression.Constant(query),
                    Expression.Constant(entityType)
                    );
            }

            protected override Expression VisitUnaryExpression(UnaryExpression expression)
            {
                var filter = _parent._queryCompilationContext.TableFilterFactory.TryCreate(expression, _entityType);
                AtsTableQuery query;
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
                var filter = _parent._queryCompilationContext.TableFilterFactory.TryCreate(expression, _entityType);
                AtsTableQuery query;
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
                var filter = _parent._queryCompilationContext.TableFilterFactory.TryCreate(expression, _entityType);
                AtsTableQuery query;
                if (filter != null
                    && _parent._queriesBySource.TryGetValue(_querySource, out query))
                {
                    query.WithFilter(filter);
                }
                if (expression.NodeType == ExpressionType.AndAlso)
                {
                    //TODO add support composite logical statements
                    return Expression.MakeBinary(expression.NodeType, VisitExpression(expression.Left), VisitExpression(expression.Right));
                }
                else if (expression.NodeType == ExpressionType.OrElse)
                {
                    //TODO refactor to handle else statements
                    return expression;
                }
                //TODO replace expression
                return base.VisitBinaryExpression(expression);
            }
        }

        private static readonly MethodInfo _entityScanMethodInfo
            = typeof(AtsQueryModelVisitor).GetTypeInfo().GetDeclaredMethod("EntityScan");

        [UsedImplicitly]
        private static IEnumerable<TResult> EntityScan<TResult>(QueryContext queryContext, AtsTableQuery tableQuery, IEntityType entityType)
            where TResult : class, new()
        {
            var context = ((AtsQueryContext)queryContext);
            var table = new AtsTable(entityType.StorageName);
            var request = new QueryTableRequest<TResult>(
                table,
                tableQuery, s =>
                    (TResult)context.StateManager.GetOrMaterializeEntry(
                        entityType,
                        context.ValueReaderFactory.Create(entityType, s)
                        ).Entity);

            return context.GetOrAddQueryResults(request);
        }
    }
}
