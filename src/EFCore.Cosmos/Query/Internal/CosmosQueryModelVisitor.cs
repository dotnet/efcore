// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Cosmos.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json.Linq;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    public class CosmosQueryModelVisitor : EntityQueryModelVisitor
    {
        public CosmosQueryModelVisitor(EntityQueryModelVisitorDependencies dependencies,
            QueryCompilationContext queryCompilationContext)
            : base(dependencies, queryCompilationContext)
        {
        }

        public bool AllMembersBoundToJObject { get; set; } = true;

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            if (Expression is QueryShaperExpression queryShaperExpression)
            {
                if (queryShaperExpression.QueryExpression is DocumentQueryExpression documentQueryExpression)
                {
                    var selectExpression = documentQueryExpression.SelectExpression;
                    var sqlTranslatingExpressionVisitor = new SqlTranslatingExpressionVisitor(
                        selectExpression, QueryCompilationContext);
                    var sqlPredicate = sqlTranslatingExpressionVisitor.Visit(whereClause.Predicate);

                    if (sqlTranslatingExpressionVisitor.Translated)
                    {
                        selectExpression.AddToPredicate(sqlPredicate);

                        return;
                    }
                }

                if (AllMembersBoundToJObject)
                {
                    var fromClause = queryModel.MainFromClause;
                    var previousParameterType = CurrentParameter.Type;

                    // Temporarily change current parameter to JObject to try binding to it without materializing the entity
                    UpdateCurrentParameter(fromClause, typeof(JObject));

                    var predicate = ReplaceClauseReferences(whereClause.Predicate);

                    if (AllMembersBoundToJObject)
                    {
                        Expression = new QueryShaperExpression(
                            QueryCompilationContext.IsAsyncQuery,
                            Expression.Call(LinqOperatorProvider.Where.MakeGenericMethod(CurrentParameter.Type),
                            queryShaperExpression.QueryExpression,
                            Expression.Lambda(predicate, CurrentParameter)),
                            queryShaperExpression.Shaper);

                        if (CurrentParameter.Type == typeof(JObject))
                        {
                            UpdateCurrentParameter(fromClause, previousParameterType);
                        }
                        return;
                    }

                    UpdateCurrentParameter(fromClause, previousParameterType);
                }
            }

            base.VisitWhereClause(whereClause, queryModel, index);
        }

        private void UpdateCurrentParameter(IQuerySource querySource, Type type)
        {
            CurrentParameter = Expression.Parameter(type, querySource.ItemName);

            QueryCompilationContext.AddOrUpdateMapping(querySource, CurrentParameter);
        }
    }
}
