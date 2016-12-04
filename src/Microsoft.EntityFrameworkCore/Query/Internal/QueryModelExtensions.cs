// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class QueryModelExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string Print([NotNull] this QueryModel queryModel, bool removeFormatting = false, int? characterLimit = null)
            => new QueryModelPrinter().Print(queryModel, removeFormatting, characterLimit);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static int CountQuerySourceReferences(
            [NotNull] this QueryModel queryModel, [NotNull] IQuerySource querySource)
        {
            var visitor = new ReferenceFindingExpressionVisitor(querySource);

            queryModel.TransformExpressions(visitor.Visit);

            return visitor.Count;
        }

        private class ReferenceFindingExpressionVisitor : ExpressionVisitorBase
        {
            private readonly IQuerySource _querySource;

            public ReferenceFindingExpressionVisitor(IQuerySource querySource)
            {
                _querySource = querySource;
            }

            public int Count { get; private set; }

            protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
            {
                if (expression.ReferencedQuerySource == _querySource)
                {
                    Count++;
                }

                return expression;
            }

            protected override Expression VisitSubQuery(SubQueryExpression expression)
            {
                expression.QueryModel.TransformExpressions(Visit);

                return expression;
            }
        }
    }
}
