// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class QueryFlatteningExpressionVisitor : ExpressionVisitorBase
    {
        private readonly IQuerySource _outerQuerySource;
        private readonly IQuerySource _innerQuerySource;
        private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;
        private readonly int _readerOffset;
        private readonly MethodInfo _operatorToFlatten;

        private MethodCallExpression _outerSelectManyExpression;
        private Expression _outerShaperExpression;
        private Expression _outerCommandBuilder;

        public QueryFlatteningExpressionVisitor(
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

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            var newExpression
                = (MethodCallExpression)base.VisitMethodCall(methodCallExpression);

            if (_outerShaperExpression != null)
            {
                if (_outerCommandBuilder == null)
                {
                    _outerCommandBuilder = methodCallExpression.Arguments[1];
                }
                else if (newExpression.Method.MethodIsClosedFormOf(
                    _relationalQueryCompilationContext.QueryMethodProvider.ShapedQueryMethod))
                {
                    newExpression
                        = Expression.Call(
                            newExpression.Method,
                            newExpression.Arguments[0],
                            _outerCommandBuilder,
                            newExpression.Arguments[2]);
                }
            }

            if (ReferenceEquals(newExpression.Method, RelationalQueryModelVisitor.CreateValueBufferMethodInfo)
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
                        var oldBufferOffset = (int)((ConstantExpression)newArguments[4]).Value;

                        newArguments[4] = Expression.Constant(oldBufferOffset + _readerOffset);
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
                                typeof(QueryResultScope),
                                typeof(QueryResultScope)),
                        _outerSelectManyExpression.Arguments[0],
                        newExpression.Arguments[1] is LambdaExpression
                            ? newExpression.Arguments[1]
                            : Expression.Lambda(
                                newExpression.Arguments[1],
                                EntityQueryModelVisitor.QueryResultScopeParameter));
            }

            return newExpression;
        }
    }
}
