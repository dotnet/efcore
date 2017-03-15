// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class QuerySourceExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool HasGeneratedItemName([NotNull] this IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotEmpty(querySource.ItemName, nameof(querySource.ItemName));

            return querySource.ItemName.StartsWith("<generated>_", StringComparison.Ordinal);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void VisitUnderlyingQuerySources([NotNull] this IQuerySource querySource, Action<IQuerySource> action)
        {
            switch (querySource)
            {
                case GroupResultOperator groupResultOperator:
                    VisitUnderlyingQuerySources(groupResultOperator.KeySelector, action);
                    VisitUnderlyingQuerySources(groupResultOperator.ElementSelector, action);
                    break;
                case GroupJoinClause groupJoinClause:
                    action(groupJoinClause.JoinClause);
                    break;
                case JoinClause joinClause:
                    VisitUnderlyingQuerySources(joinClause.InnerSequence, action);
                    break;
                case FromClauseBase fromClauseBase:
                    VisitUnderlyingQuerySources(fromClauseBase.FromExpression, action);
                    break;
            }
        }

        private static void VisitUnderlyingQuerySources(Expression expression, Action<IQuerySource> action)
        {
            switch (expression)
            {
                case QuerySourceReferenceExpression querySourceReferenceExpression:
                    action(querySourceReferenceExpression.ReferencedQuerySource);
                    break;
                case SubQueryExpression subQueryExpression:
                    VisitUnderlyingQuerySources(subQueryExpression, action);
                    break;
            }
        }

        private static void VisitUnderlyingQuerySources(SubQueryExpression subQueryExpression, Action<IQuerySource> action)
        {
            if (subQueryExpression.QueryModel.ResultOperators.LastOrDefault() is IQuerySource querySourceResultOperator)
            {
                action(querySourceResultOperator);
            }
            else
            {
                VisitUnderlyingQuerySources(subQueryExpression.QueryModel.SelectClause.Selector, action);
            }
        }
    }
}
