// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RelationalDbFunctionTransformer : IExpressionTransformer<MethodCallExpression>
    {
        private readonly IModel _model;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalDbFunctionTransformer([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            _model = model;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Transform(MethodCallExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (_model.Relational().FindDbFunction(expression.Method)?.IsIQueryable == true)
            {
                return new DbFunctionSourceExpression(expression, _model);
            }

            return expression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ExpressionType[] SupportedExpressionTypes => new[] { ExpressionType.Call };
    }
}
