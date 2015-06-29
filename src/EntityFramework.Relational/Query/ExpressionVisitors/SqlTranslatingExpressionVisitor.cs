// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing;

// ReSharper disable AssignNullToNotNullAttribute

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class SqlTranslatingExpressionVisitor : ThrowingExpressionVisitor
    {
        private readonly RelationalQueryModelVisitor _queryModelVisitor;
        private readonly Expression _topLevelPredicate;

        public SqlTranslatingExpressionVisitor(
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [CanBeNull] Expression topLevelPredicate = null)
        {
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));

            _queryModelVisitor = queryModelVisitor;
            _topLevelPredicate = topLevelPredicate;
        }

        public virtual Expression ClientEvalPredicate { get; private set; }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            switch (binaryExpression.NodeType)
            {
                case ExpressionType.Coalesce:
                {
                    var left = Visit(binaryExpression.Left);
                    var right = Visit(binaryExpression.Right);

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
                    var left = Visit(binaryExpression.Left);
                    var right = Visit(binaryExpression.Right);

                    if (binaryExpression == _topLevelPredicate)
                    {
                        if (left != null
                            && right != null)
                        {
                            return Expression.AndAlso(left, right);
                        }

                        if (left != null)
                        {
                            ClientEvalPredicate = binaryExpression.Right;
                            return left;
                        }

                        if (right != null)
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
                    var leftExpression = Visit(binaryExpression.Left);
                    var rightExpression = Visit(binaryExpression.Right);

                    return leftExpression != null
                           && rightExpression != null
                        ? Expression.MakeBinary(binaryExpression.NodeType, leftExpression, rightExpression)
                        : null;
                }
            }

            return null;
        }

        protected override Expression VisitConditional(ConditionalExpression expression)
        {
            var test = Visit(expression.Test);
            var ifTrue = Visit(expression.IfTrue);
            var ifFalse = Visit(expression.IfFalse);

            if (test != null
                && ifTrue != null
                && ifFalse != null)
            {
                return expression.Update(test, ifTrue, ifFalse);
            }

            return null;
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
            var leftExpression = Visit(binaryExpression.Left);
            var rightExpression = Visit(binaryExpression.Right);

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
                        = left.TryGetColumnExpression()
                          ?? right.TryGetColumnExpression();

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

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            var operand = Visit(methodCallExpression.Object);

            if (operand != null
                || methodCallExpression.Object == null)
            {
                var arguments
                    = methodCallExpression.Arguments
                        .Where(e => !(e is QuerySourceReferenceExpression))
                        .Select(Visit)
                        .Where(e => e != null)
                        .ToArray();

                if (arguments.Length == methodCallExpression.Arguments.Count)
                {
                    var boundExpression
                        = operand != null
                            ? Expression.Call(operand, methodCallExpression.Method, arguments)
                            : Expression.Call(methodCallExpression.Method, arguments);

                    var translatedExpression =
                        _queryModelVisitor.QueryCompilationContext.CompositeMethodCallTranslator
                            .Translate(boundExpression);

                    if (translatedExpression != null)
                    {
                        return translatedExpression;
                    }
                }
            }

            return _queryModelVisitor
                .BindMethodCallExpression(
                    methodCallExpression,
                    (property, querySource, selectExpression)
                        => CreateAliasedColumnExpression(property, querySource, selectExpression));
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));

            if (!(memberExpression.Expression is QuerySourceReferenceExpression))
            {
                var newExpression = Visit(memberExpression.Expression);

                if (newExpression != null
                    || memberExpression.Expression == null)
                {
                    var newMemberExpression
                        = newExpression != memberExpression.Expression
                            ? Expression.Property(newExpression, memberExpression.Member.Name)
                            : memberExpression;

                    var translatedExpression
                        = _queryModelVisitor.QueryCompilationContext.CompositeMemberTranslator
                            .Translate(newMemberExpression);

                    if (translatedExpression != null)
                    {
                        return translatedExpression;
                    }
                }
            }

            return _queryModelVisitor
                .BindMemberExpression(
                    memberExpression,
                    (property, querySource, selectExpression)
                        => CreateAliasedColumnExpression(property, querySource, selectExpression));
        }

        private AliasExpression CreateAliasedColumnExpression(
            IProperty property, IQuerySource querySource, SelectExpression selectExpression)
        {
            return new AliasExpression(
                new ColumnExpression(
                    _queryModelVisitor.QueryCompilationContext.GetColumnName(property),
                    property,
                    selectExpression.GetTableForQuerySource(querySource)));
        }

        protected override Expression VisitUnary(UnaryExpression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                {
                    var operand = Visit(expression.Operand);

                    return Expression.Not(operand);
                }
                case ExpressionType.Convert:
                {
                    var operand = Visit(expression.Operand);

                    if (operand != null)
                    {
                        return Expression.Convert(operand, expression.Type);
                    }
                }
                    break;
            }

            return null;
        }

        protected override Expression VisitNew(NewExpression newExpression)
        {
            Check.NotNull(newExpression, nameof(newExpression));

            if (newExpression.Members != null
                && newExpression.Arguments.Any()
                && newExpression.Arguments.Count == newExpression.Members.Count)
            {
                var memberBindings
                    = newExpression.Arguments
                        .Select(Visit)
                        .Where(e => e != null)
                        .ToArray();

                if (memberBindings.Length == newExpression.Arguments.Count)
                {
                    return Expression.Constant(memberBindings);
                }
            }

            return null;
        }

        protected override Expression VisitSubQuery(SubQueryExpression expression)
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

                    if (parameter != null
                        && memberItem != null)
                    {
                        var aliasExpression = (AliasExpression)VisitMember(memberItem);

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
            typeof(decimal),
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

        protected override Expression VisitConstant(ConstantExpression constantExpression)
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

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            Check.NotNull(parameterExpression, nameof(parameterExpression));

            var underlyingType = parameterExpression.Type.UnwrapNullableType().UnwrapEnumType();

            return _supportedConstantTypes.Contains(underlyingType)
                ? parameterExpression
                : null;
        }

        protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression querySourceReferenceExpression)
        {
            var selector
                = ((querySourceReferenceExpression.ReferencedQuerySource as FromClauseBase)
                    ?.FromExpression as SubQueryExpression)
                    ?.QueryModel.SelectClause.Selector;

            return selector != null ? Visit(selector) : null;
        }

        protected override TResult VisitUnhandledItem<TItem, TResult>(
            TItem unhandledItem, string visitMethod, Func<TItem, TResult> baseBehavior)
            => default(TResult);

        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
            => null; // Never called
    }
}
