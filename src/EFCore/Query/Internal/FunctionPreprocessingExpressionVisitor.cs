// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class FunctionPreprocessingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo _startsWithMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) });

        private static readonly MethodInfo _endsWithMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) });

        private static readonly Expression _constantNullString = Expression.Constant(null, typeof(string));

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (_startsWithMethodInfo.Equals(methodCallExpression.Method)
                || _endsWithMethodInfo.Equals(methodCallExpression.Method))
            {
                if (methodCallExpression.Arguments[0] is ConstantExpression constantArgument
                    && (string)constantArgument.Value == string.Empty)
                {
                    // every string starts/ends with empty string.
                    return Expression.Constant(true);
                }

                var newObject = Visit(methodCallExpression.Object);
                var newArgument = Visit(methodCallExpression.Arguments[0]);

                var result = Expression.AndAlso(
                    Expression.NotEqual(newObject, _constantNullString),
                    Expression.AndAlso(
                        Expression.NotEqual(newArgument, _constantNullString),
                        methodCallExpression.Update(newObject, new[] { newArgument })));

                return newArgument is ConstantExpression
                    ? result
                    : Expression.OrElse(
                        Expression.Equal(
                            newArgument,
                            Expression.Constant(string.Empty)),
                        result);
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            if (unaryExpression.NodeType == ExpressionType.Not
                && unaryExpression.Operand is MethodCallExpression innerMethodCall
                && (_startsWithMethodInfo.Equals(innerMethodCall.Method)
                || _endsWithMethodInfo.Equals(innerMethodCall.Method)))
            {
                if (innerMethodCall.Arguments[0] is ConstantExpression constantArgument
                    && (string)constantArgument.Value == string.Empty)
                {
                    // every string starts/ends with empty string.
                    return Expression.Constant(false);
                }

                var newObject = Visit(innerMethodCall.Object);
                var newArgument = Visit(innerMethodCall.Arguments[0]);

                var result = Expression.AndAlso(
                    Expression.NotEqual(newObject, _constantNullString),
                    Expression.AndAlso(
                        Expression.NotEqual(newArgument, _constantNullString),
                        Expression.Not(innerMethodCall.Update(newObject, new[] { newArgument }))));

                return newArgument is ConstantExpression
                    ? result
                    : Expression.AndAlso(
                        Expression.NotEqual(
                            newArgument,
                            Expression.Constant(string.Empty)),
                        result);
            }

            return base.VisitUnary(unaryExpression);
        }
    }
}
