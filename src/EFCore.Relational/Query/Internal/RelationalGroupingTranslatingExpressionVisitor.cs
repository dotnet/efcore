// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class RelationalGroupingTranslatingExpressionVisitor : ExpressionVisitor
    {
        private readonly RelationalSqlTranslatingExpressionVisitor _sqlTranslator;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public RelationalGroupingTranslatingExpressionVisitor(
            [NotNull] RelationalSqlTranslatingExpressionVisitor sqlTranslator,
            [NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlTranslator = sqlTranslator;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression? TranslateGrouping([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            // Visit returns translated results, but we pass through SQL translation again to set the type mapping and validate.
            return Visit(expression) is SqlExpression sqlExpression ? sqlExpression : null;
        }

        /// <inheritdoc />
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            => (methodCallExpression.Method.DeclaringType == typeof(Enumerable)
                    ? methodCallExpression.Method.Name switch
                    {
                        nameof(Enumerable.Average) => TranslateAverage(methodCallExpression),
                        nameof(Enumerable.Count) => TranslateCount(methodCallExpression),
                        nameof(Enumerable.Distinct) => TranslateDistinct(methodCallExpression),
                        nameof(Enumerable.LongCount) => TranslateLongCount(methodCallExpression),
                        nameof(Enumerable.Max) => TranslateMax(methodCallExpression),
                        nameof(Enumerable.Min) => TranslateMin(methodCallExpression),
                        nameof(Enumerable.Select) => TranslateSelect(methodCallExpression),
                        nameof(Enumerable.Sum) => TranslateSum(methodCallExpression),
                        nameof(Enumerable.Where) => TranslateWhere(methodCallExpression),
                        _ => null
                    }
                    : null)
                ?? QueryCompilationContext.NotTranslatedExpression;

        /// <inheritdoc />
        protected override Expression VisitExtension(Expression extensionExpression)
            => Check.NotNull(extensionExpression, nameof(extensionExpression)) is GroupByShaperExpression groupByShaperExpression
                ? new GroupingExpression(groupByShaperExpression.ElementSelector)
                : QueryCompilationContext.NotTranslatedExpression;

        protected virtual SqlExpression? TranslateAverage(MethodCallExpression methodCallExpression)
        {
            if (Visit(methodCallExpression.Arguments[0]) is GroupingExpression groupingExpression)
            {
                if (methodCallExpression.Arguments.Count == 2)
                {
                    groupingExpression = ApplySelector(
                        groupingExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());
                }

                return GetExpressionForAggregation(groupingExpression) is SqlExpression averageExpression
                    ? _sqlTranslator.TranslateAverage(averageExpression)
                    : null;
            }

            return null;
        }

        protected virtual SqlExpression? TranslateCount(MethodCallExpression methodCallExpression)
        {
            if (Visit(methodCallExpression.Arguments[0]) is GroupingExpression groupingExpression)
            {
                if (methodCallExpression.Arguments.Count == 2)
                {
                    var newGroupingExpression = ApplyPredicate(
                        groupingExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());
                    if (newGroupingExpression == null)
                    {
                        return null;
                    }

                    groupingExpression = newGroupingExpression;
                }

                return _sqlTranslator.TranslateCount(GetExpressionForAggregation(groupingExpression, starProjection: true)!);
            }

            return null;
        }

        protected virtual Expression? TranslateDistinct(MethodCallExpression methodCallExpression)
        {
            if (Visit(methodCallExpression.Arguments[0]) is GroupingExpression groupingExpression)
            {
                return groupingExpression.Selector is EntityShaperExpression
                    ? groupingExpression
                    : groupingExpression.IsDistinct
                        ? null
                        : groupingExpression.ApplyDistinct();
            }

            return null;
        }

        protected virtual SqlExpression? TranslateLongCount(MethodCallExpression methodCallExpression)
        {
            if (Visit(methodCallExpression.Arguments[0]) is GroupingExpression groupingExpression)
            {
                if (methodCallExpression.Arguments.Count == 2)
                {
                    var newGroupingExpression = ApplyPredicate(
                        groupingExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());
                    if (newGroupingExpression == null)
                    {
                        return null;
                    }

                    groupingExpression = newGroupingExpression;
                }

                return _sqlTranslator.TranslateLongCount(GetExpressionForAggregation(groupingExpression, starProjection: true)!);
            }

            return null;
        }

        protected virtual SqlExpression? TranslateMax(MethodCallExpression methodCallExpression)
        {
            if (Visit(methodCallExpression.Arguments[0]) is GroupingExpression groupingExpression)
            {
                if (methodCallExpression.Arguments.Count == 2)
                {
                    groupingExpression = ApplySelector(
                        groupingExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());
                }

                return GetExpressionForAggregation(groupingExpression) is SqlExpression maxExpression
                    ? _sqlTranslator.TranslateMax(maxExpression)
                    : null;
            }

            return null;
        }

        protected virtual SqlExpression? TranslateMin(MethodCallExpression methodCallExpression)
        {
            if (Visit(methodCallExpression.Arguments[0]) is GroupingExpression groupingExpression)
            {
                if (methodCallExpression.Arguments.Count == 2)
                {
                    groupingExpression = ApplySelector(
                        groupingExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());
                }

                return GetExpressionForAggregation(groupingExpression) is SqlExpression maxExpression
                    ? _sqlTranslator.TranslateMin(maxExpression)
                    : null;
            }

            return null;
        }

        protected virtual Expression? TranslateSelect(MethodCallExpression methodCallExpression)
        {
            if (Visit(methodCallExpression.Arguments[0]) is GroupingExpression groupingExpression)
            {
                return ApplySelector(groupingExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());
            }

            return null;
        }

        protected virtual SqlExpression? TranslateSum(MethodCallExpression methodCallExpression)
        {
            if (Visit(methodCallExpression.Arguments[0]) is GroupingExpression groupingExpression)
            {
                if (methodCallExpression.Arguments.Count == 2)
                {
                    groupingExpression = ApplySelector(
                        groupingExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());
                }

                return GetExpressionForAggregation(groupingExpression) is SqlExpression sumExpression
                    ? _sqlTranslator.TranslateSum(sumExpression)
                    : null;
            }

            return null;
        }

        protected virtual Expression? TranslateWhere(MethodCallExpression methodCallExpression)
        {
            if (Visit(methodCallExpression.Arguments[0]) is GroupingExpression groupingExpression)
            {
                return ApplyPredicate(groupingExpression, methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());
            }

            return null;
        }

        private GroupingExpression? ApplyPredicate(GroupingExpression groupingExpression, LambdaExpression lambdaExpression)
        {
            var predicate = _sqlTranslator.Translate(RemapLambda(groupingExpression, lambdaExpression));

            return predicate == null
                ? null
                : groupingExpression.ApplyPredicate(predicate);
        }

        private GroupingExpression ApplySelector(
            GroupingExpression groupingExpression,
            LambdaExpression lambdaExpression)
        {
            var selector = RemapLambda(groupingExpression, lambdaExpression);

            return groupingExpression.ApplySelector(selector);
        }

        private static Expression RemapLambda(GroupingExpression groupingExpression, LambdaExpression lambdaExpression)
            => ReplacingExpressionVisitor.Replace(
                lambdaExpression.Parameters[0], groupingExpression.Selector, lambdaExpression.Body);

        private SqlExpression? GetExpressionForAggregation(GroupingExpression groupingExpression, bool starProjection = false)
        {
            var selector = _sqlTranslator.Translate(groupingExpression.Selector);
            if (selector == null)
            {
                if (starProjection)
                {
                    selector = _sqlExpressionFactory.Fragment("*");
                }
                else
                {
                    return null;
                }
            }

            if (groupingExpression.Predicate != null)
            {
                if (selector is SqlFragmentExpression)
                {
                    selector = _sqlExpressionFactory.Constant(1);
                }

                selector = _sqlExpressionFactory.Case(
                    new List<CaseWhenClause> { new(groupingExpression.Predicate, selector) },
                    elseResult: null);
            }

            if (groupingExpression.IsDistinct
                && !(selector is SqlFragmentExpression))
            {
                selector = new DistinctExpression(selector);
            }

            return selector;
        }

        sealed class GroupingExpression : Expression
        {
            public GroupingExpression(Expression selector)
                => Selector = selector;

            public Expression Selector { get; private set; }
            public bool IsDistinct { get; private set; }
            public SqlExpression? Predicate { get; private set; }

            public GroupingExpression ApplyDistinct()
            {
                IsDistinct = true;

                return this;
            }

            public GroupingExpression ApplySelector(Expression expression)
            {
                Selector = expression;

                return this;
            }

            public GroupingExpression ApplyPredicate(SqlExpression expression)
            {
                Check.NotNull(expression, nameof(expression));

                if (expression is SqlConstantExpression sqlConstant
                    && sqlConstant.Value is bool boolValue
                    && boolValue)
                {
                    return this;
                }

                Predicate = Predicate == null
                    ? expression
                    : new SqlBinaryExpression(
                        ExpressionType.AndAlso,
                        Predicate,
                        expression,
                        typeof(bool),
                        expression.TypeMapping);

                return this;
            }

            public override Type Type
                => typeof(IEnumerable<>).MakeGenericType(Selector.Type);

            public override ExpressionType NodeType
                => ExpressionType.Extension;
        }
    }
}
