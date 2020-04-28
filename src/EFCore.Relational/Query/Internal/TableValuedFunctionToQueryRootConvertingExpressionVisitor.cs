// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class TableValuedFunctionToQueryRootConvertingExpressionVisitor : ExpressionVisitor
    {
        private readonly IModel _model;

        public TableValuedFunctionToQueryRootConvertingExpressionVisitor([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            _model = model;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var function = _model.FindDbFunction(methodCallExpression.Method);

            return function?.IsScalar == false
                ? CreateTableValuedFunctionQueryRootExpression(function, methodCallExpression.Arguments)
                : base.VisitMethodCall(methodCallExpression);
        }

        private Expression CreateTableValuedFunctionQueryRootExpression(
            IDbFunction function, IReadOnlyCollection<Expression> arguments)
            => new TableValuedFunctionQueryRootExpression(function.ReturnEntityType, function, arguments);
    }
}
