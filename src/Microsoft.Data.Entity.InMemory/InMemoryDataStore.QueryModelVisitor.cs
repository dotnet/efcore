// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.InMemory
{
    public partial class InMemoryDataStore
    {
        private class QueryModelVisitor : EntityQueryModelVisitor
        {
            public QueryModelVisitor()
                : base(null)
            {
            }

            private QueryModelVisitor(EntityQueryModelVisitor parentQueryModelVisitor)
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
                = typeof(QueryModelVisitor).GetTypeInfo().GetDeclaredMethod("EntityScan");

            [UsedImplicitly]
            private static IEnumerable<TEntity> EntityScan<TEntity>(QueryContext queryContext)
            {
                var entityType = queryContext.Model.GetEntityType(typeof(TEntity));

                return ((InMemoryQueryContext)queryContext).Database.GetTable(entityType)
                    .Select(t => (TEntity)queryContext.StateManager
                        .GetOrMaterializeEntry(entityType, new ObjectArrayValueReader(t)).Entity);
            }

            private class InMemoryQueryingExpressionTreeVisitor : QueryingExpressionTreeVisitor
            {
                public InMemoryQueryingExpressionTreeVisitor(EntityQueryModelVisitor queryModelVisitor)
                    : base(queryModelVisitor)
                {
                }

                protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
                {
                    var queryModelVisitor = new QueryModelVisitor(_parentQueryModelVisitor);

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
                    return VisitProjectionSubQuery(expression, new QueryModelVisitor(_parentQueryModelVisitor));
                }
            }
        }
    }
}
