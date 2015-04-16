// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class NullableExpressionsExtractingVisitor : ExpressionTreeVisitor
    {
        public NullableExpressionsExtractingVisitor()
        {
            NullableExpressions = new List<Expression>();
        }

        public virtual List<Expression> NullableExpressions { get; private set; }

        protected override Expression VisitConstantExpression(ConstantExpression expression)
        {
            if (expression.Value == null)
            {
                NullableExpressions.Add(expression);
            }

            return base.VisitConstantExpression(expression);
        }

        protected override Expression VisitExtensionExpression(ExtensionExpression expression)
        {
            var notNullableExpression = expression as NotNullableExpression;
            if (notNullableExpression != null)
            {
                return expression;
            }

            var columnExpression = expression as ColumnExpression 
                ?? expression.GetColumnExpression();

            if (columnExpression != null && columnExpression.Property.IsNullable)
            {
                NullableExpressions.Add(expression);

                return expression;
            }

            var isNullExpression = expression as IsNullExpression;
            if (isNullExpression != null)
            {
                return expression;
            }

            var inExpression = expression as InExpression;
            if (inExpression != null)
            {
                return expression;
            }

            var caseExpression = expression as CaseExpression;
            if (caseExpression != null)
            {
                // TODO: for now, case expression always returns 0 or 1, therefore it can't be nullable
                // this will change in the future when we support full expressiveness of case statements
                return expression;
            }

            return base.VisitExtensionExpression(expression);
        }
    }
}
