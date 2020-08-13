// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    public class ShaperExpressionProcessingExpressionVisitor : ExpressionVisitor
    {
        private readonly InMemoryQueryExpression _queryExpression;
        private readonly ParameterExpression _valueBufferParameter;

        private readonly IDictionary<Expression, ParameterExpression> _mapping = new Dictionary<Expression, ParameterExpression>();
        private readonly List<ParameterExpression> _variables = new List<ParameterExpression>();
        private readonly List<Expression> _expressions = new List<Expression>();

        public ShaperExpressionProcessingExpressionVisitor(
            [CanBeNull] InMemoryQueryExpression queryExpression, [NotNull] ParameterExpression valueBufferParameter)
        {
            _queryExpression = queryExpression;
            _valueBufferParameter = valueBufferParameter;
        }

        public virtual Expression Inject(Expression expression)
        {
            var result = Visit(expression);
            _expressions.Add(result);
            result = Expression.Block(_variables, _expressions);

            return ConvertToLambda(result);
        }

        private LambdaExpression ConvertToLambda(Expression result)
            => Expression.Lambda(
                result,
                QueryCompilationContext.QueryContextParameter,
                _valueBufferParameter);

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
                    if (includeExpression.NavigationExpression is CollectionShaperExpression collectionShaper)
                    {
                        var innerLambda = (LambdaExpression)collectionShaper.InnerShaper;
                        var innerShaper = new ShaperExpressionProcessingExpressionVisitor(null, innerLambda.Parameters[0])
                            .Inject(innerLambda.Body);

                        _expressions.Add(
                            includeExpression.Update(
                                entity,
                                collectionShaper.Update(
                                    Visit(collectionShaper.Projection),
                                    innerShaper)));
                    }
                    else
                    {
                        _expressions.Add(
                            includeExpression.Update(
                                entity,
                                Visit(includeExpression.NavigationExpression)));
                    }

                    return entity;
                }

                case CollectionShaperExpression collectionShaperExpression:
                {
                    var key = GenerateKey((ProjectionBindingExpression)collectionShaperExpression.Projection);
                    if (!_mapping.TryGetValue(key, out var variable))
                    {
                        var projection = Visit(collectionShaperExpression.Projection);

                        variable = Expression.Parameter(collectionShaperExpression.Type);
                        _variables.Add(variable);

                        var innerLambda = (LambdaExpression)collectionShaperExpression.InnerShaper;
                        var innerShaper = new ShaperExpressionProcessingExpressionVisitor(null, innerLambda.Parameters[0])
                            .Inject(innerLambda.Body);

                        _expressions.Add(Expression.Assign(variable, collectionShaperExpression.Update(projection, innerShaper)));
                        _mapping[key] = variable;
                    }

                    return variable;
                }

                case SingleResultShaperExpression singleResultShaperExpression:
                {
                    var key = GenerateKey((ProjectionBindingExpression)singleResultShaperExpression.Projection);
                    if (!_mapping.TryGetValue(key, out var variable))
                    {
                        var projection = Visit(singleResultShaperExpression.Projection);

                        variable = Expression.Parameter(singleResultShaperExpression.Type);
                        _variables.Add(variable);

                        var innerLambda = (LambdaExpression)singleResultShaperExpression.InnerShaper;
                        var innerShaper = new ShaperExpressionProcessingExpressionVisitor(null, innerLambda.Parameters[0])
                            .Inject(innerLambda.Body);

                        _expressions.Add(Expression.Assign(variable, singleResultShaperExpression.Update(projection, innerShaper)));
                        _mapping[key] = variable;
                    }

                    return variable;
                }
            }

            return base.VisitExtension(extensionExpression);
        }

        private Expression GenerateKey(ProjectionBindingExpression projectionBindingExpression)
            => _queryExpression != null
                && projectionBindingExpression.ProjectionMember != null
                    ? _queryExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember)
                    : projectionBindingExpression;
    }
}
