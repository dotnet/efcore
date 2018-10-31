// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{

    public class RelationalEvaluatableExpressionFilter : EvaluatableExpressionFilter
    {
        private readonly IModel _model;

        public RelationalEvaluatableExpressionFilter([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            _model = model;
        }

        public override bool IsEvaluatableExpression(Expression expression)
        {
            if (expression is MethodCallExpression methodCallExpression
                && _model.Relational().FindDbFunction(methodCallExpression.Method) != null)
            {
                return false;
            }

            return base.IsEvaluatableExpression(expression);
        }
    }
}
