// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Utilities;
using JetBrains.Annotations;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class SqlTranslatingExpressionTreeVisitor : ThrowingExpressionTreeVisitor
    {
        private readonly RelationalQueryModelVisitor _queryModelVisitor;
        private readonly Expression _topLevelPredicate;

        public SqlTranslatingExpressionTreeVisitor(
            [NotNull] RelationalQueryModelVisitor queryModelVisitor, 
            [CanBeNull] Expression topLevelPredicate = null)
        {
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));

            _queryModelVisitor = queryModelVisitor;
            _topLevelPredicate = topLevelPredicate;
        }

        public virtual Expression ClientEvalPredicate { get; private set; }

        protected override Expression VisitBinaryExpression([NotNull] BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            switch (binaryExpression.NodeType)
            {
                case ExpressionType.Coalesce:
                {
                    var left = VisitExpression(binaryExpression.Left);
                    var right = VisitExpression(binaryExpression.Right);

                    return new AliasExpression(binaryExpression.Update(left, binaryExpression.Conversion, right));
                }
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                {
                    var structuralComparisonExpression
                        = UnfoldStructuralComparison(
                            binaryExpression.NodeType,
                            ProcessComparisonExpression(binaryExpression));

                    return structuralComparisonExpression;
                }
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                {
                    return ProcessComparisonExpression(binaryExpression);
                }

                case ExpressionType.AndAlso:
                {
                    var left = VisitExpression(binaryExpression.Left);
                    var right = VisitExpression(binaryExpression.Right);

                    if (binaryExpression == _topLevelPredicate)
                    {
                        if (left != null && right != null)
                        {
                            return Expression.AndAlso(left, right);
                        }

                        if (left != null && right == null)
                        {
                            ClientEvalPredicate = binaryExpression.Right;
                            return left;
                        }

                        if (left == null && right != null)
                        {
                            ClientEvalPredicate = binaryExpression.Left;
                            return right;
                        }

                        return null;
                    }

                    return left != null && right != null
                        ? Expression.AndAlso(left, right)
                        : null;
                }

                case ExpressionType.OrElse:
                case ExpressionType.Add:
                case ExpressionType.Subtract:
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                {
                    var leftExpression = VisitExpression(binaryExpression.Left);
                    var rightExpression = VisitExpression(binaryExpression.Right);

                    return leftExpression != null
                           && rightExpression != null
                        ? Expression.MakeBinary(binaryExpression.NodeType, leftExpression, rightExpression)
                        : null;
                }
            }

            return null;
        }

        protected override Expression VisitConditionalExpression(ConditionalExpression expression)
        {
            var test = VisitExpression(expression.Test);
            var ifTrue = VisitExpression(expression.IfTrue);
            var ifFalse = VisitExpression(expression.IfFalse);

            return new CaseExpression(expression.Update(test, ifTrue, ifFalse));
        }

        private static Expression UnfoldStructuralComparison(ExpressionType expressionType, Expression expression)
        {
            var binaryExpression = expression as BinaryExpression;
            var leftConstantExpression = binaryExpression?.Left as ConstantExpression;
            var leftExpressions = leftConstantExpression?.Value as Expression[];

            if (leftExpressions != null)
            {
                var rightConstantExpression = binaryExpression.Right as ConstantExpression;

                var rightExpressions = rightConstantExpression?.Value as Expression[];

                if (rightExpressions != null
                    && leftExpressions.Length == rightExpressions.Length)
                {
                    return leftExpressions
                        .Zip(rightExpressions, (l, r) =>
                            Expression.MakeBinary(expressionType, l, r))
                        .Aggregate((e1, e2) =>
                            expressionType == ExpressionType.Equal
                                ? Expression.AndAlso(e1, e2)
                                : Expression.OrElse(e1, e2));
                }
            }

            return expression;
        }

        private Expression ProcessComparisonExpression(BinaryExpression binaryExpression)
        {
            var leftExpression = VisitExpression(binaryExpression.Left);
            var rightExpression = VisitExpression(binaryExpression.Right);

            if (leftExpression == null
                || rightExpression == null)
            {
                return null;
            }

            var nullExpression
                = TransformNullComparison(leftExpression, rightExpression, binaryExpression.NodeType);

            return nullExpression
                   ?? Expression.MakeBinary(binaryExpression.NodeType, leftExpression, rightExpression);
        }

        private static Expression TransformNullComparison(
            Expression left, Expression right, ExpressionType expressionType)
        {
            if (expressionType == ExpressionType.Equal
                || expressionType == ExpressionType.NotEqual)
            {
                var constantExpression
                    = right as ConstantExpression
                      ?? left as ConstantExpression;

                if (constantExpression != null
                    && constantExpression.Value == null)
                {
                    var columnExpression
                        = left.GetColumnExpression()
                          ?? right.GetColumnExpression();

                    if (columnExpression != null)
                    {
                        return expressionType == ExpressionType.Equal
                            ? (Expression)new IsNullExpression(columnExpression)
                            : Expression.Not(new IsNullExpression(columnExpression));
                    }
                }
            }

            return null;
        }

        protected override Expression VisitMethodCallExpression([NotNull] MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            var operand = VisitExpression(methodCallExpression.Object);

            if (operand != null)
            {
                var arguments
                    = methodCallExpression.Arguments
                        .Select(VisitExpression)
                        .Where(e => e != null)
                        .ToArray();

                if (arguments.Length == methodCallExpression.Arguments.Count)
                {
                    var boundExpression
                        = Expression.Call(
                            operand,
                            methodCallExpression.Method,
                            arguments);

                    return _queryModelVisitor.QueryCompilationContext.MethodCallTranslator
                        .Translate(boundExpression);
                }
            }
            else
            {
                return _queryModelVisitor
                    .BindMethodCallExpression(
                        methodCallExpression,
                        (property, querySource, selectExpression)
                            =>
                            {
                                return new AliasExpression(
                                    new ColumnExpression(
                                        _queryModelVisitor.QueryCompilationContext.GetColumnName(property),
                                        property,
                                        selectExpression.FindTableForQuerySource(querySource)));
                            });
            }

            return null;
        }

        protected override Expression VisitMemberExpression([NotNull] MemberExpression memberExpression)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));

            return _queryModelVisitor
                .BindMemberExpression(
                    memberExpression,
                    (property, querySource, selectExpression)
                        =>
                        {
                            return new AliasExpression(
                                new ColumnExpression(
                                    _queryModelVisitor.QueryCompilationContext.GetColumnName(property),
                                    property,
                                    selectExpression.FindTableForQuerySource(querySource)));
                        });
        }

        protected override Expression VisitUnaryExpression([NotNull] UnaryExpression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                {
                    var operand = VisitExpression(expression.Operand);

                    return Expression.Not(operand);
                }
                case ExpressionType.Convert:
                {
                    var operand = VisitExpression(expression.Operand);

                    if (operand != null)
                    {
                        return Expression.Convert(operand, expression.Type);
                    }
                }
                    break;
            }

            return null;
        }

        protected override Expression VisitNewExpression([NotNull] NewExpression newExpression)
        {
            Check.NotNull(newExpression, nameof(newExpression));

            if (newExpression.Members != null
                && newExpression.Arguments.Any()
                && newExpression.Arguments.Count == newExpression.Members.Count)
            {
                var memberBindings
                    = newExpression.Arguments
                        .Select(VisitExpression)
                        .Where(e => e != null)
                        .ToArray();

                if (memberBindings.Length == newExpression.Arguments.Count)
                {
                    return Expression.Constant(memberBindings);
                }
            }

            return null;
        }

        protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (expression.QueryModel.IsIdentityQuery()
                && expression.QueryModel.ResultOperators.Count == 1)
            {
                var contains = expression.QueryModel.ResultOperators.First() as ContainsResultOperator;
                if (contains != null)
                {
                    var parameter = expression.QueryModel.MainFromClause.FromExpression as ParameterExpression;
                    var memberItem = contains.Item as MemberExpression;
                    if (parameter != null && memberItem != null)
                    {
                        var aliasExpression = (AliasExpression)VisitMemberExpression(memberItem);

                        return new InExpression(aliasExpression, new[] { parameter });
                    }
                }
            }

            return null;
        }

        private static readonly Type[] _supportedConstantTypes =
            {
                typeof(bool),
                typeof(byte),
                typeof(byte[]),
                typeof(char),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(double),
                typeof(float),
                typeof(Guid),
                typeof(int),
                typeof(long),
                typeof(sbyte),
                typeof(short),
                typeof(string),
                typeof(uint),
                typeof(ulong),
                typeof(ushort)
            };

        protected override Expression VisitConstantExpression([NotNull] ConstantExpression constantExpression)
        {
            Check.NotNull(constantExpression, nameof(constantExpression));

            if (constantExpression.Value == null)
            {
                return constantExpression;
            }

            var underlyingType = constantExpression.Type.UnwrapNullableType().UnwrapEnumType();

            return _supportedConstantTypes.Contains(underlyingType) 
                ? constantExpression 
                : null;
        }

        protected override Expression VisitParameterExpression([NotNull] ParameterExpression parameterExpression)
        {
            Check.NotNull(parameterExpression, nameof(parameterExpression));

            var underlyingType = parameterExpression.Type.UnwrapNullableType().UnwrapEnumType();

            return _supportedConstantTypes.Contains(underlyingType) 
                ? parameterExpression 
                : null;
        }

        protected override TResult VisitUnhandledItem<TItem, TResult>(
            TItem unhandledItem, string visitMethod, Func<TItem, TResult> baseBehavior)
        {
            return default(TResult);
        }

        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
        {
            return null; // never called
        }
    }
}
