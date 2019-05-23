// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.NavigationExpansion;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class ShaperExpressionProcessingExpressionVisitor : ExpressionVisitor
    {
        private readonly SelectExpression _selectExpression;
        private IDictionary<Expression, ParameterExpression> _mapping = new Dictionary<Expression, ParameterExpression>();
        private List<ParameterExpression> _variables = new List<ParameterExpression>();
        private List<Expression> _expressions = new List<Expression>();

        public ShaperExpressionProcessingExpressionVisitor(SelectExpression selectExpression)
        {
            _selectExpression = selectExpression;
        }

        public BlockExpression Inject(Expression expression)
        {
            _expressions.Add(Visit(expression));

            return Expression.Block(_variables, _expressions);
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is EntityShaperExpression entityShaperExpression)
            {
                var key = GenerateKey(entityShaperExpression.ValueBufferExpression);
                if (!_mapping.TryGetValue(key, out var variable))
                {
                    variable = Expression.Parameter(entityShaperExpression.EntityType.ClrType);
                    _variables.Add(variable);
                    _expressions.Add(Expression.Assign(variable, entityShaperExpression));
                    _mapping[key] = variable;
                }

                return variable;
            }

            if (extensionExpression is ProjectionBindingExpression projectionBindingExpression)
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

            if (extensionExpression is IncludeExpression includeExpression)
            {
                var entity = Visit(includeExpression.EntityExpression);
                var innerShaper = new ShaperExpressionProcessingExpressionVisitor(_selectExpression)
                    .Inject(includeExpression.NavigationExpression);
                _expressions.Add(includeExpression.Update(entity, innerShaper));

                return entity;
            }

            return base.VisitExtension(extensionExpression);
        }

        private Expression GenerateKey(ProjectionBindingExpression projectionBindingExpression)
        {
            return projectionBindingExpression.ProjectionMember != null
                ? _selectExpression.GetProjectionExpression(projectionBindingExpression.ProjectionMember)
                : projectionBindingExpression;
        }
    }
}
