// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.ExpressionTreeVisitors;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class QueryFlatteningExpressionTreeVisitor : ExpressionTreeVisitorBase
    {
        private readonly IQuerySource _outerQuerySource;
        private readonly IQuerySource _innerQuerySource;
        private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;
        private readonly int _readerOffset;
        private readonly MethodInfo _operatorToFlatten;

        private MethodCallExpression _outerSelectManyExpression;
        private Expression _outerShaperExpression;
        private Expression _outerCommandBuilder;

        public QueryFlatteningExpressionTreeVisitor(
            [NotNull] IQuerySource outerQuerySource,
            [NotNull] IQuerySource innerQuerySource,
            [NotNull] RelationalQueryCompilationContext relationalQueryCompilationContext,
            int readerOffset,
            [NotNull] MethodInfo operatorToFlatten)
        {
            Check.NotNull(outerQuerySource, nameof(outerQuerySource));
            Check.NotNull(innerQuerySource, nameof(innerQuerySource));
            Check.NotNull(relationalQueryCompilationContext, nameof(relationalQueryCompilationContext));
            Check.NotNull(operatorToFlatten, nameof(operatorToFlatten));

            _outerQuerySource = outerQuerySource;
            _innerQuerySource = innerQuerySource;
            _relationalQueryCompilationContext = relationalQueryCompilationContext;
            _readerOffset = readerOffset;
            _operatorToFlatten = operatorToFlatten;
        }

        protected override Expression VisitMethodCallExpression([NotNull] MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            var newExpression
                = (MethodCallExpression)base.VisitMethodCallExpression(methodCallExpression);

            if (_outerShaperExpression != null)
            {
                if (_outerCommandBuilder == null)
                {
                    _outerCommandBuilder = methodCallExpression.Arguments[1];
                }
                else if (newExpression.Method.MethodIsClosedFormOf(
                    _relationalQueryCompilationContext.QueryMethodProvider.QueryMethod))
                {
                    newExpression
                        = Expression.Call(
                            newExpression.Method,
                            newExpression.Arguments[0],
                            _outerCommandBuilder,
                            newExpression.Arguments[2]);
                }
            }

            if (ReferenceEquals(newExpression.Method, RelationalQueryModelVisitor.CreateValueReaderMethodInfo)
                || newExpression.Method.MethodIsClosedFormOf(RelationalQueryModelVisitor.CreateEntityMethodInfo))
            {
                var constantExpression = (ConstantExpression)newExpression.Arguments[0];

                if (constantExpression.Value == _outerQuerySource)
                {
                    _outerShaperExpression = newExpression;
                }
                else if (constantExpression.Value == _innerQuerySource)
                {
                    var newArguments
                        = new List<Expression>(newExpression.Arguments)
                            {
                                [2] = _outerShaperExpression
                            };

                    if (newArguments.Count == RelationalQueryModelVisitor.CreateEntityMethodInfo.GetParameters().Length)
                    {
                        newArguments[5]
                            = Expression.Constant(
                                _readerOffset
                                + (int)((ConstantExpression)newArguments[5]).Value);
                    }

                    newExpression
                        = Expression.Call(newExpression.Method, newArguments);
                }
            }
            else if (_outerShaperExpression != null
                     && _outerSelectManyExpression == null
                     && newExpression.Method.MethodIsClosedFormOf(
                         _relationalQueryCompilationContext.LinqOperatorProvider.SelectMany))
            {
                _outerSelectManyExpression = newExpression;
            }
            else if (_outerSelectManyExpression != null
                     && newExpression.Method.MethodIsClosedFormOf(_operatorToFlatten))
            {
                newExpression
                    = Expression.Call(
                        _relationalQueryCompilationContext.LinqOperatorProvider.SelectMany
                            .MakeGenericMethod(
                                typeof(QuerySourceScope),
                                typeof(QuerySourceScope)),
                        _outerSelectManyExpression.Arguments[0],
                        newExpression.Arguments[1] is LambdaExpression
                            ? newExpression.Arguments[1]
                            : Expression.Lambda(
                                newExpression.Arguments[1],
                                EntityQueryModelVisitor.QuerySourceScopeParameter));
            }

            return newExpression;
        }
    }
}
