// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    public class ShaperExpressionProcessingExpressionVisitor : ExpressionVisitor
    {
        private readonly InMemoryQueryExpression _queryExpression;

        private readonly IDictionary<Expression, ParameterExpression> _mapping = new Dictionary<Expression, ParameterExpression>();
        private readonly List<ParameterExpression> _variables = new List<ParameterExpression>();
        private readonly List<Expression> _expressions = new List<Expression>();

        public ShaperExpressionProcessingExpressionVisitor(
            InMemoryQueryExpression queryExpression)
        {
            _queryExpression = queryExpression;
        }

        public virtual Expression Inject(Expression expression)
        {
            var result = Visit(expression);

            if (_expressions.All(e => e.NodeType == ExpressionType.Assign))
            {
                result = new ReplacingExpressionVisitor(
                    _expressions.Cast<BinaryExpression>()
                        .ToDictionary(e => e.Left, e => e.Right)).Visit(result);
            }
            else
            {
                _expressions.Add(result);
                result = Expression.Block(_variables, _expressions);
            }

            return ConvertToLambda(result, Expression.Parameter(result.Type, "result"));
        }

        private LambdaExpression ConvertToLambda(Expression result, ParameterExpression resultParameter)
            => Expression.Lambda(
                result,
                QueryCompilationContext.QueryContextParameter,
                _queryExpression.ValueBufferParameter);

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case EntityShaperExpression entityShaperExpression:
                {
                    var key = GenerateKey((ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression);
                    if (!_mapping.TryGetValue(key, out var variable))
                    {
                        variable = Expression.Parameter(entityShaperExpression.EntityType.ClrType);
                        _variables.Add(variable);
                        _expressions.Add(Expression.Assign(variable, entityShaperExpression));
                        _mapping[key] = variable;
                    }

                    return variable;
                }

                case ProjectionBindingExpression projectionBindingExpression:
                {
                    var key = GenerateKey(projectionBindingExpression);
                    if (!_mapping.TryGetValue(key, out var variable))
                    {
                        variable = Expression.Parameter(projectionBindingExpression.Type);
                        _variables.Add(variable);
                        _expressions.Add(Expression.Assign(variable, projectionBindingExpression));
                        _mapping[key] = variable;
                    }

                    return variable;
                }

                case IncludeExpression includeExpression:
                {
                    var entity = Visit(includeExpression.EntityExpression);
                    _expressions.Add(
                        includeExpression.Update(
                            entity,
                            Visit(includeExpression.NavigationExpression)));

                    return entity;
                }
            }

            return base.VisitExtension(extensionExpression);
        }

        private Expression GenerateKey(ProjectionBindingExpression projectionBindingExpression)
            => projectionBindingExpression.ProjectionMember != null
                ? _queryExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember)
                : projectionBindingExpression;
    }
}
