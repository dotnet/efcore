// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.WindowsAzure.Storage.Table;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    class AzureTableStorageQueryModelVisitor : EntityQueryModelVisitor
    {
        private static readonly MethodInfo _queryMethod = typeof(AzureTableStorageQueryModelVisitor).GetMethod("RunTableQuery", BindingFlags.NonPublic | BindingFlags.Static);

        public AzureTableStorageQueryModelVisitor()
            : base(null)
        {
        }


        private AzureTableStorageQueryModelVisitor(EntityQueryModelVisitor parentQueryModelVisitor)
            : base(parentQueryModelVisitor)
        {
        }


        protected override ExpressionTreeVisitor CreateQueryingExpressionTreeVisitor(EntityQueryModelVisitor parentQueryModelVisitor)
        {
            return new InMemoryQueryingExpressionTreeVisitor(parentQueryModelVisitor);
        }


        protected override ExpressionTreeVisitor CreateProjectionExpressionTreeVisitor(EntityQueryModelVisitor parentQueryModelVisitor)
        {
            return new InMemoryProjectionSubQueryExpressionTreeVisitor(parentQueryModelVisitor);
        }


        private static readonly MethodInfo _entityScanMethodInfo
            = typeof(AzureTableStorageQueryModelVisitor).GetTypeInfo().GetDeclaredMethod("EntityScan");

        private static IEnumerable<TEntity> EntityScan<TEntity>(QueryContext queryContext)
        {
            var type = queryContext.Model.GetEntityType(typeof(TEntity));
            var table = ((AzureTableStorageQueryContext)queryContext).Database.GetTableReference(type.StorageName);

            // TODO Need to cache generic method on entity type
            // TODO Apply store side evaluation
            var queryMethod = _queryMethod.MakeGenericMethod(typeof(TEntity));
            var result = ((IEnumerable<TEntity>)queryMethod.Invoke(null, new object[] { table })).ToList();

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

        private class InMemoryQueryingExpressionTreeVisitor : QueryingExpressionTreeVisitor
        {
            public InMemoryQueryingExpressionTreeVisitor(EntityQueryModelVisitor queryModelVisitor)
                : base(queryModelVisitor)
            {
            }


            protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
            {
                var queryModelVisitor = new AzureTableStorageQueryModelVisitor(_parentQueryModelVisitor);


                queryModelVisitor.VisitQueryModel(expression.QueryModel);


                return queryModelVisitor._expression;
            }


            protected override Expression VisitEntityQueryable(Type elementType)
            {
                return Expression.Call(
                    _entityScanMethodInfo.MakeGenericMethod(elementType),
                    _queryContextParameter);
            }
        }


        private class InMemoryProjectionSubQueryExpressionTreeVisitor : InMemoryQueryingExpressionTreeVisitor
        {
            public InMemoryProjectionSubQueryExpressionTreeVisitor(EntityQueryModelVisitor queryModelVisitor)
                : base(queryModelVisitor)
            {
            }


            protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
            {
                return VisitProjectionSubQuery(expression, new AzureTableStorageQueryModelVisitor(_parentQueryModelVisitor));
            }
        }

        private static IEnumerable<TResult> RunTableQuery<TResult>(CloudTable table)
           where TResult : ITableEntity, new()
        {
            var query = new TableQuery<TResult>();
            return table.ExecuteQuery(query);
        }
    }

}
