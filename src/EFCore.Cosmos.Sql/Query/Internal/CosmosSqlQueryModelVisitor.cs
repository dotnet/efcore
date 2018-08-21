// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json.Linq;
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

        public bool AllMembersBoundToJObject { get; set; }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            Debug.Assert(Expression is QueryShaperExpression, "Invalid Expression encountered");

            var queryShaperExpression = (QueryShaperExpression)Expression;

            if (queryShaperExpression.QueryExpression is DocumentQueryExpression documentQueryExpression)
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

            var fromClause = queryModel.MainFromClause;

            // Change current parameter to JObject
            var previousParameterType = CurrentParameter.Type;
            UpdateCurrentParameter(fromClause, typeof(JObject));
            var parentBindingState = AllMembersBoundToJObject;
            AllMembersBoundToJObject = true;

            var predicate = ReplaceClauseReferences(whereClause.Predicate);

            if (AllMembersBoundToJObject)
            {
                Expression = new QueryShaperExpression(
                    Expression.Call(LinqOperatorProvider.Where.MakeGenericMethod(CurrentParameter.Type),
                    queryShaperExpression.QueryExpression,
                    Expression.Lambda(predicate, CurrentParameter)),
                    queryShaperExpression.Shaper);
                AllMembersBoundToJObject = parentBindingState;

                UpdateCurrentParameter(fromClause, previousParameterType);
            }
            else
            {
                UpdateCurrentParameter(fromClause, previousParameterType);

                base.VisitWhereClause(whereClause, queryModel, index);
                AllMembersBoundToJObject = false;
            }
        }

        private void UpdateCurrentParameter(IQuerySource querySource, Type type)
        {
            CurrentParameter = Expression.Parameter(type, querySource.ItemName);

            QueryCompilationContext.AddOrUpdateMapping(querySource, CurrentParameter);
        }
    }
}
