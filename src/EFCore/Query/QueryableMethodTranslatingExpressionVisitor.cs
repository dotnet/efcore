// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class QueryableMethodTranslatingExpressionVisitor : ExpressionVisitor
    {
        private readonly bool _subquery;
        private readonly EntityShaperNullableMarkingExpressionVisitor _entityShaperNullableMarkingExpressionVisitor;

        protected QueryableMethodTranslatingExpressionVisitor(
            QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
            bool subquery)
        {
            Dependencies = dependencies;
            _subquery = subquery;
            _entityShaperNullableMarkingExpressionVisitor = new EntityShaperNullableMarkingExpressionVisitor();
        }

        protected virtual QueryableMethodTranslatingExpressionVisitorDependencies Dependencies { get; }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
            => constantExpression.IsEntityQueryable()
                ? CreateShapedQueryExpression(((IQueryable)constantExpression.Value).ElementType)
                : base.VisitConstant(constantExpression);

        protected override Expression VisitExtension(Expression expression)
            => expression is ShapedQueryExpression
                ? expression
                : base.VisitExtension(expression);

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            ShapedQueryExpression CheckTranslated(ShapedQueryExpression translated)
            {
                if (translated == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.TranslationFailed(methodCallExpression.Print()));
                }

                return translated;
            }

            var method = methodCallExpression.Method;
            if (method.DeclaringType == typeof(Queryable) || method.DeclaringType == typeof(QueryableExtensions))
            {
                var source = Visit(methodCallExpression.Arguments[0]);
                if (source is ShapedQueryExpression shapedQueryExpression)
                {
                    var genericMethod = method.IsGenericMethod ? method.GetGenericMethodDefinition() : null;
                    switch (method.Name)
                    {
                        case nameof(Queryable.All)
                        when genericMethod == QueryableMethods.All:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateAll(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                        case nameof(Queryable.Any)
                        when genericMethod == QueryableMethods.AnyWithoutPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateAny(shapedQueryExpression, null));

                        case nameof(Queryable.Any)
                        when genericMethod == QueryableMethods.AnyWithPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateAny(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                        case nameof(Queryable.AsQueryable)
                        when genericMethod == QueryableMethods.AsQueryable:
                            return source;

                        case nameof(Queryable.Average)
                        when QueryableMethods.IsAverageWithoutSelector(method):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateAverage(shapedQueryExpression, null, methodCallExpression.Type));

                        case nameof(Queryable.Average)
                        when QueryableMethods.IsAverageWithSelector(method):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateAverage(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type));

                        case nameof(Queryable.Cast)
                        when genericMethod == QueryableMethods.Cast:
                            return CheckTranslated(TranslateCast(shapedQueryExpression, method.GetGenericArguments()[0]));

                        case nameof(Queryable.Concat)
                        when genericMethod == QueryableMethods.Concat:
                        {
                            var source2 = Visit(methodCallExpression.Arguments[1]);
                            if (source2 is ShapedQueryExpression innerShapedQueryExpression)
                            {
                                return TranslateConcat(
                                    shapedQueryExpression,
                                    innerShapedQueryExpression);
                            }
                            break;
                        }

                        case nameof(Queryable.Contains)
                        when genericMethod == QueryableMethods.Contains:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateContains(shapedQueryExpression, methodCallExpression.Arguments[1]));

                        case nameof(Queryable.Count)
                        when genericMethod == QueryableMethods.CountWithoutPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateCount(shapedQueryExpression, null));

                        case nameof(Queryable.Count)
                        when genericMethod == QueryableMethods.CountWithPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateCount(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                        case nameof(Queryable.DefaultIfEmpty)
                        when genericMethod == QueryableMethods.DefaultIfEmptyWithoutArgument:
                            return CheckTranslated(TranslateDefaultIfEmpty(shapedQueryExpression, null));

                        case nameof(Queryable.DefaultIfEmpty)
                        when genericMethod == QueryableMethods.DefaultIfEmptyWithArgument:
                            return CheckTranslated(TranslateDefaultIfEmpty(shapedQueryExpression, methodCallExpression.Arguments[1]));

                        case nameof(Queryable.Distinct)
                        when genericMethod == QueryableMethods.Distinct:
                            return CheckTranslated(TranslateDistinct(shapedQueryExpression));

                        case nameof(Queryable.ElementAt)
                        when genericMethod == QueryableMethods.ElementAt:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateElementAtOrDefault(shapedQueryExpression, methodCallExpression.Arguments[1], false));

                        case nameof(Queryable.ElementAtOrDefault)
                        when genericMethod == QueryableMethods.ElementAtOrDefault:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.SingleOrDefault;
                            return CheckTranslated(TranslateElementAtOrDefault(shapedQueryExpression, methodCallExpression.Arguments[1], true));

                        case nameof(Queryable.Except)
                        when genericMethod == QueryableMethods.Except:
                        {
                            var source2 = Visit(methodCallExpression.Arguments[1]);
                            if (source2 is ShapedQueryExpression innerShapedQueryExpression)
                            {
                                return CheckTranslated(TranslateExcept(
                                    shapedQueryExpression,
                                    innerShapedQueryExpression));
                            }
                            break;
                        }

                        case nameof(Queryable.First)
                        when genericMethod == QueryableMethods.FirstWithoutPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateFirstOrDefault(shapedQueryExpression, null, methodCallExpression.Type, false));

                        case nameof(Queryable.First)
                        when genericMethod == QueryableMethods.FirstWithPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateFirstOrDefault(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, false));

                        case nameof(Queryable.FirstOrDefault)
                        when genericMethod == QueryableMethods.FirstOrDefaultWithoutPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.SingleOrDefault;
                            return CheckTranslated(TranslateFirstOrDefault(shapedQueryExpression, null, methodCallExpression.Type, true));

                        case nameof(Queryable.FirstOrDefault)
                        when genericMethod == QueryableMethods.FirstOrDefaultWithPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.SingleOrDefault;
                            return CheckTranslated(TranslateFirstOrDefault(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, true));

                        case nameof(Queryable.GroupBy)
                            when genericMethod == QueryableMethods.GroupByWithKeySelector:
                            return CheckTranslated(TranslateGroupBy(shapedQueryExpression, GetLambdaExpressionFromArgument(1), null, null));

                        case nameof(Queryable.GroupBy)
                            when genericMethod == QueryableMethods.GroupByWithKeyElementSelector:
                            return CheckTranslated(TranslateGroupBy(shapedQueryExpression, GetLambdaExpressionFromArgument(1), GetLambdaExpressionFromArgument(2), null));

                        case nameof(Queryable.GroupBy)
                        when genericMethod == QueryableMethods.GroupByWithKeyElementResultSelector:
                            return CheckTranslated(TranslateGroupBy(shapedQueryExpression, GetLambdaExpressionFromArgument(1), GetLambdaExpressionFromArgument(2), GetLambdaExpressionFromArgument(3)));

                        case nameof(Queryable.GroupBy)
                        when genericMethod == QueryableMethods.GroupByWithKeyResultSelector:
                            return CheckTranslated(TranslateGroupBy(shapedQueryExpression, GetLambdaExpressionFromArgument(1), null, GetLambdaExpressionFromArgument(2)));

                        case nameof(Queryable.GroupJoin)
                        when genericMethod == QueryableMethods.GroupJoin:
                        {
                            if (Visit(methodCallExpression.Arguments[1]) is ShapedQueryExpression innerShapedQueryExpression)
                            {
                                return CheckTranslated(
                                    TranslateGroupJoin(
                                        shapedQueryExpression,
                                        innerShapedQueryExpression,
                                        GetLambdaExpressionFromArgument(2),
                                        GetLambdaExpressionFromArgument(3),
                                        GetLambdaExpressionFromArgument(4)));
                            }
                            break;
                        }

                        case nameof(Queryable.Intersect)
                        when genericMethod == QueryableMethods.Intersect:
                        {
                            if (Visit(methodCallExpression.Arguments[1]) is ShapedQueryExpression innerShapedQueryExpression)
                            {
                                return CheckTranslated(TranslateIntersect(shapedQueryExpression, innerShapedQueryExpression));
                            }
                            break;
                        }

                        case nameof(Queryable.Join)
                        when genericMethod == QueryableMethods.Join:
                        {
                            if (Visit(methodCallExpression.Arguments[1]) is ShapedQueryExpression innerShapedQueryExpression)
                            {
                                return CheckTranslated(TranslateJoin(shapedQueryExpression, innerShapedQueryExpression, GetLambdaExpressionFromArgument(2), GetLambdaExpressionFromArgument(3), GetLambdaExpressionFromArgument(4)));
                            }
                            break;
                        }

                        case nameof(QueryableExtensions.LeftJoin)
                        when genericMethod == QueryableExtensions.LeftJoinMethodInfo:
                        {
                            if (Visit(methodCallExpression.Arguments[1]) is ShapedQueryExpression innerShapedQueryExpression)
                            {
                                return CheckTranslated(TranslateLeftJoin(shapedQueryExpression, innerShapedQueryExpression, GetLambdaExpressionFromArgument(2), GetLambdaExpressionFromArgument(3), GetLambdaExpressionFromArgument(4)));
                            }
                            break;
                        }

                        case nameof(Queryable.Last)
                        when genericMethod == QueryableMethods.LastWithoutPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateLastOrDefault(shapedQueryExpression, null, methodCallExpression.Type, false));

                        case nameof(Queryable.Last)
                        when genericMethod == QueryableMethods.LastWithPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateLastOrDefault(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, false));

                        case nameof(Queryable.LastOrDefault)
                        when genericMethod == QueryableMethods.LastOrDefaultWithoutPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.SingleOrDefault;
                            return CheckTranslated(TranslateLastOrDefault(shapedQueryExpression, null, methodCallExpression.Type, true));

                        case nameof(Queryable.LastOrDefault)
                        when genericMethod == QueryableMethods.LastOrDefaultWithPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.SingleOrDefault;
                            return CheckTranslated(TranslateLastOrDefault(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, true));

                        case nameof(Queryable.LongCount)
                        when genericMethod == QueryableMethods.LongCountWithoutPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateLongCount(shapedQueryExpression, null));

                        case nameof(Queryable.LongCount)
                        when genericMethod == QueryableMethods.LongCountWithPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateLongCount(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                        case nameof(Queryable.Max)
                        when genericMethod == QueryableMethods.MaxWithoutSelector:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateMax(shapedQueryExpression, null, methodCallExpression.Type));

                        case nameof(Queryable.Max)
                        when genericMethod == QueryableMethods.MaxWithSelector:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateMax(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type));

                        case nameof(Queryable.Min)
                        when genericMethod == QueryableMethods.MinWithoutSelector:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateMin(shapedQueryExpression, null, methodCallExpression.Type));

                        case nameof(Queryable.Min)
                        when genericMethod == QueryableMethods.MinWithSelector:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateMin(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type));

                        case nameof(Queryable.OfType)
                        when genericMethod == QueryableMethods.OfType:
                            return CheckTranslated(TranslateOfType(shapedQueryExpression, method.GetGenericArguments()[0]));

                        case nameof(Queryable.OrderBy)
                        when genericMethod == QueryableMethods.OrderBy:
                            return CheckTranslated(TranslateOrderBy(shapedQueryExpression, GetLambdaExpressionFromArgument(1), true));

                        case nameof(Queryable.OrderByDescending)
                        when genericMethod == QueryableMethods.OrderByDescending:
                            return CheckTranslated(TranslateOrderBy(shapedQueryExpression, GetLambdaExpressionFromArgument(1), false));

                        case nameof(Queryable.Reverse)
                        when genericMethod == QueryableMethods.Reverse:
                            return CheckTranslated(TranslateReverse(shapedQueryExpression));

                        case nameof(Queryable.Select)
                        when genericMethod == QueryableMethods.Select:
                            return CheckTranslated(TranslateSelect(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                        case nameof(Queryable.SelectMany)
                        when genericMethod == QueryableMethods.SelectManyWithoutCollectionSelector:
                            return CheckTranslated(TranslateSelectMany(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                        case nameof(Queryable.SelectMany)
                        when genericMethod == QueryableMethods.SelectManyWithCollectionSelector:
                            return CheckTranslated(TranslateSelectMany(shapedQueryExpression, GetLambdaExpressionFromArgument(1), GetLambdaExpressionFromArgument(2)));

                        case nameof(Queryable.Single)
                        when genericMethod == QueryableMethods.SingleWithoutPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateSingleOrDefault(shapedQueryExpression, null, methodCallExpression.Type, false));

                        case nameof(Queryable.Single)
                        when genericMethod == QueryableMethods.SingleWithPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateSingleOrDefault(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, false));

                        case nameof(Queryable.SingleOrDefault)
                        when genericMethod == QueryableMethods.SingleOrDefaultWithoutPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.SingleOrDefault;
                            return CheckTranslated(TranslateSingleOrDefault(shapedQueryExpression, null, methodCallExpression.Type, true));

                        case nameof(Queryable.SingleOrDefault)
                        when genericMethod == QueryableMethods.SingleOrDefaultWithPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.SingleOrDefault;
                            return CheckTranslated(TranslateSingleOrDefault(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, true));

                        case nameof(Queryable.Skip)
                        when genericMethod == QueryableMethods.Skip:
                            return CheckTranslated(TranslateSkip(shapedQueryExpression, methodCallExpression.Arguments[1]));

                        case nameof(Queryable.SkipWhile)
                        when genericMethod == QueryableMethods.SkipWhile:
                            return CheckTranslated(TranslateSkipWhile(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                        case nameof(Queryable.Sum)
                        when QueryableMethods.IsSumWithoutSelector(method):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateSum(shapedQueryExpression, null, methodCallExpression.Type));

                        case nameof(Queryable.Sum)
                        when QueryableMethods.IsSumWithSelector(method):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return CheckTranslated(TranslateSum(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type));

                        case nameof(Queryable.Take)
                        when genericMethod == QueryableMethods.Take:
                            return CheckTranslated(TranslateTake(shapedQueryExpression, methodCallExpression.Arguments[1]));

                        case nameof(Queryable.TakeWhile)
                        when genericMethod == QueryableMethods.TakeWhile:
                            return CheckTranslated(TranslateTakeWhile(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                        case nameof(Queryable.ThenBy)
                        when genericMethod == QueryableMethods.ThenBy:
                            return CheckTranslated(TranslateThenBy(shapedQueryExpression, GetLambdaExpressionFromArgument(1), true));

                        case nameof(Queryable.ThenByDescending)
                        when genericMethod == QueryableMethods.ThenByDescending:
                            return CheckTranslated(TranslateThenBy(shapedQueryExpression, GetLambdaExpressionFromArgument(1), false));

                        case nameof(Queryable.Union)
                        when genericMethod == QueryableMethods.Union:
                        {
                            if (Visit(methodCallExpression.Arguments[1]) is ShapedQueryExpression innerShapedQueryExpression)
                            {
                                return CheckTranslated(TranslateUnion(shapedQueryExpression, innerShapedQueryExpression));
                            }
                            break;
                        }

                        case nameof(Queryable.Where)
                        when genericMethod == QueryableMethods.Where:
                            return CheckTranslated(TranslateWhere(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                        LambdaExpression GetLambdaExpressionFromArgument(int argumentIndex) => methodCallExpression.Arguments[argumentIndex].UnwrapLambdaFromQuote();
                    }
                }
            }

            return _subquery
                ? (Expression)null
                : throw new NotImplementedException("Unhandled method: " + method.Name);
        }

        private class EntityShaperNullableMarkingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitExtension(Expression extensionExpression)
            {
                return extensionExpression is EntityShaperExpression entityShaper
                    ? entityShaper.MarkAsNullable()
                    : base.VisitExtension(extensionExpression);
            }
        }

        protected virtual Expression MarkShaperNullable(Expression shaperExpression)
            => _entityShaperNullableMarkingExpressionVisitor.Visit(shaperExpression);

        protected virtual ShapedQueryExpression TranslateResultSelectorForJoin(
            ShapedQueryExpression outer,
            LambdaExpression resultSelector,
            Expression innerShaper,
            Type transparentIdentifierType)
        {
            outer.ShaperExpression = CombineShapers(
                outer.QueryExpression,
                outer.ShaperExpression,
                innerShaper,
                transparentIdentifierType);

            var transparentIdentifierParameter = Expression.Parameter(transparentIdentifierType);

            Expression original1 = resultSelector.Parameters[0];
            var replacement1 = AccessOuterTransparentField(transparentIdentifierType, transparentIdentifierParameter);
            Expression original2 = resultSelector.Parameters[1];
            var replacement2 = AccessInnerTransparentField(transparentIdentifierType, transparentIdentifierParameter);
            var newResultSelector = Expression.Lambda(
                new ReplacingExpressionVisitor(
                    new Dictionary<Expression, Expression> {
                        { original1, replacement1 },
                        { original2, replacement2 }
                    }).Visit(resultSelector.Body),
                transparentIdentifierParameter);

            return TranslateSelect(outer, newResultSelector);
        }

        private Expression CombineShapers(
            Expression queryExpression,
            Expression outerShaper,
            Expression innerShaper,
            Type transparentIdentifierType)
        {
            var outerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer");
            var innerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner");
            outerShaper = new MemberAccessShiftingExpressionVisitor(queryExpression, outerMemberInfo).Visit(outerShaper);
            innerShaper = new MemberAccessShiftingExpressionVisitor(queryExpression, innerMemberInfo).Visit(innerShaper);

            return Expression.New(
                transparentIdentifierType.GetTypeInfo().DeclaredConstructors.Single(),
                new[] { outerShaper, innerShaper },
                new[] { outerMemberInfo, innerMemberInfo });
        }

        private class MemberAccessShiftingExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _queryExpression;
            private readonly MemberInfo _memberShift;

            public MemberAccessShiftingExpressionVisitor(Expression queryExpression, MemberInfo memberShift)
            {
                _queryExpression = queryExpression;
                _memberShift = memberShift;
            }

            protected override Expression VisitExtension(Expression node)
            {
                if (node is ProjectionBindingExpression projectionBindingExpression)
                {
                    return new ProjectionBindingExpression(
                        _queryExpression,
                        projectionBindingExpression.ProjectionMember.Prepend(_memberShift),
                        projectionBindingExpression.Type);
                }

                return base.VisitExtension(node);
            }
        }

        private static Expression AccessOuterTransparentField(
            Type transparentIdentifierType,
            Expression targetExpression)
        {
            var fieldInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer");

            return Expression.Field(targetExpression, fieldInfo);
        }

        private static Expression AccessInnerTransparentField(
            Type transparentIdentifierType,
            Expression targetExpression)
        {
            var fieldInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner");

            return Expression.Field(targetExpression, fieldInfo);
        }

        public virtual ShapedQueryExpression TranslateSubquery(Expression expression)
            => (ShapedQueryExpression)CreateSubqueryVisitor().Visit(expression);

        protected abstract QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor();

        protected abstract ShapedQueryExpression CreateShapedQueryExpression(Type elementType);
        protected abstract ShapedQueryExpression TranslateAll(ShapedQueryExpression source, LambdaExpression predicate);
        protected abstract ShapedQueryExpression TranslateAny(ShapedQueryExpression source, LambdaExpression predicate);
        protected abstract ShapedQueryExpression TranslateAverage(ShapedQueryExpression source, LambdaExpression selector, Type resultType);
        protected abstract ShapedQueryExpression TranslateCast(ShapedQueryExpression source, Type resultType);
        protected abstract ShapedQueryExpression TranslateConcat(ShapedQueryExpression source1, ShapedQueryExpression source2);
        protected abstract ShapedQueryExpression TranslateContains(ShapedQueryExpression source, Expression item);
        protected abstract ShapedQueryExpression TranslateCount(ShapedQueryExpression source, LambdaExpression predicate);
        protected abstract ShapedQueryExpression TranslateDefaultIfEmpty(ShapedQueryExpression source, Expression defaultValue);
        protected abstract ShapedQueryExpression TranslateDistinct(ShapedQueryExpression source);
        protected abstract ShapedQueryExpression TranslateElementAtOrDefault(ShapedQueryExpression source, Expression index, bool returnDefault);
        protected abstract ShapedQueryExpression TranslateExcept(ShapedQueryExpression source1, ShapedQueryExpression source2);
        protected abstract ShapedQueryExpression TranslateFirstOrDefault(ShapedQueryExpression source, LambdaExpression predicate, Type returnType, bool returnDefault);
        protected abstract ShapedQueryExpression TranslateGroupBy(ShapedQueryExpression source, LambdaExpression keySelector, LambdaExpression elementSelector, LambdaExpression resultSelector);
        protected abstract ShapedQueryExpression TranslateGroupJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector);
        protected abstract ShapedQueryExpression TranslateIntersect(ShapedQueryExpression source1, ShapedQueryExpression source2);
        protected abstract ShapedQueryExpression TranslateJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector);
        protected abstract ShapedQueryExpression TranslateLeftJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector);
        protected abstract ShapedQueryExpression TranslateLastOrDefault(ShapedQueryExpression source, LambdaExpression predicate, Type returnType, bool returnDefault);
        protected abstract ShapedQueryExpression TranslateLongCount(ShapedQueryExpression source, LambdaExpression predicate);
        protected abstract ShapedQueryExpression TranslateMax(ShapedQueryExpression source, LambdaExpression selector, Type resultType);
        protected abstract ShapedQueryExpression TranslateMin(ShapedQueryExpression source, LambdaExpression selector, Type resultType);
        protected abstract ShapedQueryExpression TranslateOfType(ShapedQueryExpression source, Type resultType);
        protected abstract ShapedQueryExpression TranslateOrderBy(ShapedQueryExpression source, LambdaExpression keySelector, bool ascending);
        protected abstract ShapedQueryExpression TranslateReverse(ShapedQueryExpression source);
        protected abstract ShapedQueryExpression TranslateSelect(ShapedQueryExpression source, LambdaExpression selector);
        protected abstract ShapedQueryExpression TranslateSelectMany(ShapedQueryExpression source, LambdaExpression collectionSelector, LambdaExpression resultSelector);
        protected abstract ShapedQueryExpression TranslateSelectMany(ShapedQueryExpression source, LambdaExpression selector);
        protected abstract ShapedQueryExpression TranslateSingleOrDefault(ShapedQueryExpression source, LambdaExpression predicate, Type returnType, bool returnDefault);
        protected abstract ShapedQueryExpression TranslateSkip(ShapedQueryExpression source, Expression count);
        protected abstract ShapedQueryExpression TranslateSkipWhile(ShapedQueryExpression source, LambdaExpression predicate);
        protected abstract ShapedQueryExpression TranslateSum(ShapedQueryExpression source, LambdaExpression selector, Type resultType);
        protected abstract ShapedQueryExpression TranslateTake(ShapedQueryExpression source, Expression count);
        protected abstract ShapedQueryExpression TranslateTakeWhile(ShapedQueryExpression source, LambdaExpression predicate);
        protected abstract ShapedQueryExpression TranslateThenBy(ShapedQueryExpression source, LambdaExpression keySelector, bool ascending);
        protected abstract ShapedQueryExpression TranslateUnion(ShapedQueryExpression source1, ShapedQueryExpression source2);
        protected abstract ShapedQueryExpression TranslateWhere(ShapedQueryExpression source, LambdaExpression predicate);
    }
}
