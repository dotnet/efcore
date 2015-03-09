// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.ExpressionTreeVisitors;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class RelationalProjectionExpressionTreeVisitor : ProjectionExpressionTreeVisitor
    {
        private readonly IQuerySource _querySource;
        private readonly SqlTranslatingExpressionTreeVisitor _sqlTranslatingExpressionTreeVisitor;

        private bool _requiresClientEval;

        public RelationalProjectionExpressionTreeVisitor(
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [NotNull] IQuerySource querySource)
            : base(Check.NotNull(queryModelVisitor, nameof(queryModelVisitor)))
        {
            _querySource = querySource;

            _sqlTranslatingExpressionTreeVisitor
                = new SqlTranslatingExpressionTreeVisitor(queryModelVisitor);
        }

        private new RelationalQueryModelVisitor QueryModelVisitor
            => (RelationalQueryModelVisitor)base.QueryModelVisitor;

        public virtual bool RequiresClientEval => _requiresClientEval;

        public override Expression VisitExpression(Expression expression)
        {
            if (!(expression is QuerySourceReferenceExpression))
            {
                var sqlExpression
                    = _sqlTranslatingExpressionTreeVisitor.VisitExpression(expression);

                if (sqlExpression == null)
                {
                    _requiresClientEval = true;
                }
                else
                {
                    var selectExpression
                        = QueryModelVisitor.TryGetQuery(_querySource);

                    Debug.Assert(selectExpression != null);

                    var columnExpression = sqlExpression as ColumnExpression;

                    if (columnExpression != null)
                    {
                        selectExpression.AddToProjection(columnExpression);
                    }
                }
            }

            return base.VisitExpression(expression);
        }


    }
}
