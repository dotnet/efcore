// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ConditionalOptimizingExpressionVisitor : ExpressionVisitorBase
    {
        private readonly NullCheckRewriteTester _nullCheckRewriteTester = new NullCheckRewriteTester();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            if (conditionalExpression.Test is BinaryExpression binaryExpression)
            {
                // Converts '[q] != null ? [q] : [s]' into '[q] ?? [s]'
                if (binaryExpression.NodeType == ExpressionType.NotEqual
                    && binaryExpression.Left is QuerySourceReferenceExpression querySourceReferenceExpression1
                    && binaryExpression.Right.IsNullConstantExpression()
                    && ReferenceEquals(conditionalExpression.IfTrue, querySourceReferenceExpression1))
                {
                    return Expression.Coalesce(conditionalExpression.IfTrue, conditionalExpression.IfFalse);
                }

                // Converts 'null != [q] ? [q] : [s]' into '[q] ?? [s]'
                if (binaryExpression.NodeType == ExpressionType.NotEqual
                    && binaryExpression.Right is QuerySourceReferenceExpression querySourceReferenceExpression2
                    && binaryExpression.Left.IsNullConstantExpression()
                    && ReferenceEquals(conditionalExpression.IfTrue, querySourceReferenceExpression2))
                {
                    return Expression.Coalesce(conditionalExpression.IfTrue, conditionalExpression.IfFalse);
                }

                // Converts '[q] == null ? [s] : [q]' into '[s] ?? [q]'
                if (binaryExpression.NodeType == ExpressionType.Equal
                    && binaryExpression.Left is QuerySourceReferenceExpression querySourceReferenceExpression3
                    && binaryExpression.Right.IsNullConstantExpression()
                    && ReferenceEquals(conditionalExpression.IfFalse, querySourceReferenceExpression3))
                {
                    return Expression.Coalesce(conditionalExpression.IfTrue, conditionalExpression.IfFalse);
                }

                // Converts 'null == [q] ? [s] : [q]' into '[s] ?? [q]'
                if (binaryExpression.NodeType == ExpressionType.Equal
                    && binaryExpression.Right is QuerySourceReferenceExpression querySourceReferenceExpression4
                    && binaryExpression.Left.IsNullConstantExpression()
                    && ReferenceEquals(conditionalExpression.IfFalse, querySourceReferenceExpression4))
                {
                    return Expression.Coalesce(conditionalExpression.IfTrue, conditionalExpression.IfFalse);
                }
            }

            return conditionalExpression.IsNullPropagationCandidate(out var testExpression, out var resultExpression)
                && _nullCheckRewriteTester.CanRewriteNullCheck(testExpression, resultExpression)
                ? new NullConditionalExpression(testExpression, resultExpression)
                : base.VisitConditional(conditionalExpression);
        }

        private class NullCheckRewriteTester
        {
            private IQuerySource _querySource;
            private readonly IList<string> _propertyNames = new List<string>();
            private bool _additionalPropertyInResultExpression;
            private bool? _canRewriteNullCheck;

            public bool CanRewriteNullCheck(Expression testExpression, Expression resultExpression)
            {
                // rewrite following patterns:
                // same qsre/member ---> a != null ? a : null
                // one additional member ---> a != null ? a.Property : null
                // nested ---> a.b.c != null ? a.b.c : null
                // nested with additional member ---> a.b.c != null ? a.b.c.Property : null

                // don't rewrite patterns like:
                // member-qsre ---> a.Property != null ? a : null
                // different qsres ---> a != null ? b : null
                // different members ---> a.Property1 != null ? a.Property2 : null
                // two+ additional members ---> a != null ? a.Property.Length : null
                // different nesting ---> b.c != null ? a.b.c : null
                AnalyzeTestExpression(testExpression);
                if (_querySource == null)
                {
                    return false;
                }

                AnalyzeResultExpression(resultExpression);

                return _canRewriteNullCheck ?? false;
            }

            private void AnalyzeTestExpression(Expression expression)
            {
                var processedExpression = expression.RemoveConvert();
                if (processedExpression is QuerySourceReferenceExpression qsre)
                {
                    _querySource = qsre.ReferencedQuerySource;

                    return;
                }

                if (processedExpression is MemberExpression memberExpression)
                {
                    _propertyNames.Add(memberExpression.Member.Name);
                    AnalyzeTestExpression(memberExpression.Expression);

                    return;
                }

                if (processedExpression is MethodCallExpression methodCallExpression
                    && methodCallExpression.Method.IsEFPropertyMethod())
                {
                    _propertyNames.Add((string)((ConstantExpression)methodCallExpression.Arguments[1]).Value);
                    AnalyzeTestExpression(methodCallExpression.Arguments[0]);
                }
            }

            private void AnalyzeResultExpression(Expression expression)
            {
                var processedExpression = expression.RemoveConvert();
                if (processedExpression is QuerySourceReferenceExpression qsre)
                {
                    _canRewriteNullCheck = qsre.ReferencedQuerySource == _querySource
                                           && _propertyNames.Count == 0;

                    return;
                }

                if (processedExpression is MemberExpression memberExpression)
                {
                    if (_propertyNames.Count > 0
                        && _propertyNames[0] == memberExpression.Member.Name)
                    {
                        _propertyNames.RemoveAt(0);
                        AnalyzeResultExpression(memberExpression.Expression);
                    }
                    else if (!_additionalPropertyInResultExpression)
                    {
                        _additionalPropertyInResultExpression = true;
                        AnalyzeResultExpression(memberExpression.Expression);
                    }

                    return;
                }

                if (processedExpression is MethodCallExpression methodCallExpression
                    && methodCallExpression.IsEFProperty())
                {
                    var propertyName = (string)((ConstantExpression)methodCallExpression.Arguments[1]).Value;
                    if (_propertyNames.Count > 0
                        && _propertyNames[0] == propertyName)
                    {
                        _propertyNames.RemoveAt(0);
                        AnalyzeResultExpression(methodCallExpression.Arguments[0]);
                    }
                    else if (!_additionalPropertyInResultExpression)
                    {
                        _additionalPropertyInResultExpression = true;
                        AnalyzeResultExpression(methodCallExpression.Arguments[0]);
                    }

                    return;
                }

                if (processedExpression is UnaryExpression unary
                    && processedExpression.NodeType == ExpressionType.TypeAs)
                {
                    AnalyzeResultExpression(unary.Operand);
                }
            }
        }
    }
}
