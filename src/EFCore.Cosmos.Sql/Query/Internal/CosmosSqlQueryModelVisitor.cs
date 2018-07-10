// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.Internal
{
    public class CosmosSqlQueryModelVisitor : EntityQueryModelVisitor
    {
        public CosmosSqlQueryModelVisitor(EntityQueryModelVisitorDependencies dependencies,
            QueryCompilationContext queryCompilationContext)
            : base(dependencies, queryCompilationContext)
        {
        }

        protected override void TrackEntitiesInResults<TResult>([NotNull] QueryModel queryModel)
        {
            // Disable tracking from here and enable that from EntityShaperExpression directly
            //base.TrackEntitiesInResults<TResult>(queryModel);
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            if (Expression is QueryShaperExpression queryShaperExpression
                && queryShaperExpression.QueryExpression is DocumentQueryExpression documentQueryExpression)
            {
                var selectExpression = documentQueryExpression.SelectExpression;
                var sqlTranslatingExpressionVisitor = new SqlTranslatingExpressionVisitor(
                    selectExpression, QueryCompilationContext);
                var sqlPredicate = sqlTranslatingExpressionVisitor.Visit(whereClause.Predicate);

                if (sqlPredicate != null)
                {
                    selectExpression.AddToPredicate(sqlPredicate);

                    return;
                }
            }

            base.VisitWhereClause(whereClause, queryModel, index);
        }
    }
}
