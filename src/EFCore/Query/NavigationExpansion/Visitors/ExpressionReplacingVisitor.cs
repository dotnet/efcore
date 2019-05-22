// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Visitors
{
    public class ExpressionReplacingVisitor : ExpressionVisitor
    {
        private Expression _searchedFor;
        private Expression _replaceWith;

        public ExpressionReplacingVisitor(Expression searchedFor, Expression replaceWith)
        {
            _searchedFor = searchedFor;
            _replaceWith = replaceWith;
        }

        protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
        {
            var newParameters = new List<ParameterExpression>();
            var parameterChanged = false;

            foreach (var parameter in lambdaExpression.Parameters)
            {
                var newParameter = (ParameterExpression)Visit(parameter);
                newParameters.Add(newParameter);
                if (newParameter != parameter)
                {
                    parameterChanged = true;
                }
            }

            var newBody = Visit(lambdaExpression.Body);

            return parameterChanged || newBody != lambdaExpression.Body
                ? Expression.Lambda(newBody, newParameters)
                : lambdaExpression;
        }

        public override Expression Visit(Expression expression)
            => expression == _searchedFor
            ? _replaceWith
            : base.Visit(expression);
    }
}
