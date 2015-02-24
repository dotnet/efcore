// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionTreeVisitors
{
    public class DefaultQueryExpressionTreeVisitor : ExpressionTreeVisitorBase
    {
        private readonly EntityQueryModelVisitor _entityQueryModelVisitor;

        public DefaultQueryExpressionTreeVisitor([NotNull] EntityQueryModelVisitor entityQueryModelVisitor)
        {
            Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor));

            _entityQueryModelVisitor = entityQueryModelVisitor;
        }

        public virtual EntityQueryModelVisitor QueryModelVisitor => _entityQueryModelVisitor;

        protected override Expression VisitSubQueryExpression(SubQueryExpression subQueryExpression)
        {
            Check.NotNull(subQueryExpression, nameof(subQueryExpression));

            var queryModelVisitor = CreateQueryModelVisitor();

            queryModelVisitor.VisitQueryModel(subQueryExpression.QueryModel);

            return queryModelVisitor.Expression;
        }

        protected EntityQueryModelVisitor CreateQueryModelVisitor()
        {
            return QueryModelVisitor.QueryCompilationContext
                .CreateQueryModelVisitor(_entityQueryModelVisitor);
        }

        protected override Expression VisitParameterExpression(ParameterExpression parameterExpression)
        {
            if (parameterExpression.Name != null
                && parameterExpression.Name
                    .StartsWith(CompiledQueryCache.CompiledQueryParameterPrefix))
            {
                return Expression.Call(
                    _getParameterValueMethodInfo.MakeGenericMethod(parameterExpression.Type),
                    EntityQueryModelVisitor.QueryContextParameter,
                    Expression.Constant(parameterExpression.Name));
            }

            return parameterExpression;
        }

        private static readonly MethodInfo _getParameterValueMethodInfo
            = typeof(DefaultQueryExpressionTreeVisitor)
                .GetTypeInfo().GetDeclaredMethod("GetParameterValue");

        [UsedImplicitly]
        private static T GetParameterValue<T>(QueryContext queryContext, string parameterName)
        {
            return (T)queryContext.ParameterValues[parameterName];
        }
    }
}
