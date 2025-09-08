// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    public class XGEvaluatableExpressionFilter : RelationalEvaluatableExpressionFilter
    {
        private readonly IEnumerable<IXGEvaluatableExpressionFilter> _xgEvaluatableExpressionFilters;

        public XGEvaluatableExpressionFilter(
            [NotNull] EvaluatableExpressionFilterDependencies dependencies,
            [NotNull] RelationalEvaluatableExpressionFilterDependencies relationalDependencies,
            [NotNull] IEnumerable<IXGEvaluatableExpressionFilter> xgEvaluatableExpressionFilters)
            : base(dependencies, relationalDependencies)
        {
            _xgEvaluatableExpressionFilters = xgEvaluatableExpressionFilters;
        }

        public override bool IsEvaluatableExpression(Expression expression, IModel model)
        {
            foreach (var evaluatableExpressionFilter in _xgEvaluatableExpressionFilters)
            {
                var evaluatable = evaluatableExpressionFilter.IsEvaluatableExpression(expression, model);
                if (evaluatable.HasValue)
                {
                    return evaluatable.Value;
                }
            }

            if (expression is MethodCallExpression methodCallExpression)
            {
                var declaringType = methodCallExpression.Method.DeclaringType;

                if (declaringType == typeof(XGDbFunctionsExtensions) ||
                    declaringType == typeof(XGJsonDbFunctionsExtensions))
                {
                    return false;
                }
            }

            return base.IsEvaluatableExpression(expression, model);
        }
    }
}
