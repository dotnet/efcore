// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class ShaperExpressionDedupingExpressionVisitor : ExpressionVisitor
    {
        private SelectExpression _selectExpression;
        private IDictionary<Expression, IList<Expression>> _duplicateShapers;

        public Expression Process(Expression expression)
        {
            if (expression is ShapedQueryExpression shapedQueryExpression)
            {
                _selectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;
                _duplicateShapers = new Dictionary<Expression, IList<Expression>>();
                Visit(shapedQueryExpression.ShaperExpression);

                var variables = new List<ParameterExpression>();
                var expressions = new List<Expression>();
                var replacements = new Dictionary<Expression, Expression>();
                var index = 0;

                foreach (var kvp in _duplicateShapers)
                {
                    if (kvp.Value.Count > 1)
                    {
                        var firstShaper = kvp.Value[0];
                        var entityParameter = Expression.Parameter(firstShaper.Type, $"entity{index++}");
                        variables.Add(entityParameter);
                        expressions.Add(Expression.Assign(
                            entityParameter,
                            firstShaper));

                        foreach (var shaper in kvp.Value)
                        {
                            replacements[shaper] = entityParameter;
                        }
                    }
                }

                if (variables.Count == 0)
                {
                    return shapedQueryExpression;
                }

                expressions.Add(new ReplacingExpressionVisitor(replacements)
                        .Visit(shapedQueryExpression.ShaperExpression));

                shapedQueryExpression.ShaperExpression = Expression.Block(variables, expressions);

                return shapedQueryExpression;
            }

            return expression;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is EntityShaperExpression entityShaperExpression)
            {
                var serverProjection = _selectExpression.GetProjectionExpression(
                    entityShaperExpression.ValueBufferExpression.ProjectionMember);

                if (_duplicateShapers.ContainsKey(serverProjection))
                {
                    _duplicateShapers[serverProjection].Add(entityShaperExpression);
                }
                else
                {
                    _duplicateShapers[serverProjection] = new List<Expression> { entityShaperExpression };
                }

                return entityShaperExpression;
            }

            return base.VisitExtension(extensionExpression);
        }
    }
}
