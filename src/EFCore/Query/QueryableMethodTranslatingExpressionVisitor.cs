// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
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
                            return TranslateAll(shapedQueryExpression, GetLambdaExpressionFromArgument(1));

                        case nameof(Queryable.Any)
                        when genericMethod == QueryableMethods.AnyWithoutPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateAny(shapedQueryExpression, null);

                        case nameof(Queryable.Any)
                        when genericMethod == QueryableMethods.AnyWithPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateAny(shapedQueryExpression, GetLambdaExpressionFromArgument(1));

                        case nameof(Queryable.AsQueryable)
                        when genericMethod == QueryableMethods.AsQueryable:
                            return source;

                        case nameof(Queryable.Average)
                        when QueryableMethods.IsAverageWithoutSelector(method):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateAverage(shapedQueryExpression, null, methodCallExpression.Type);

                        case nameof(Queryable.Average)
                        when QueryableMethods.IsAverageWithSelector(method):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateAverage(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type);

                        case nameof(Queryable.Cast)
                        when genericMethod == QueryableMethods.Cast:
                            return TranslateCast(shapedQueryExpression, method.GetGenericArguments()[0]);

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
                            return TranslateContains(shapedQueryExpression, methodCallExpression.Arguments[1]);

                        case nameof(Queryable.Count)
                        when genericMethod == QueryableMethods.CountWithoutPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateCount(shapedQueryExpression, null);

                        case nameof(Queryable.Count)
                        when genericMethod == QueryableMethods.CountWithPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateCount(shapedQueryExpression, GetLambdaExpressionFromArgument(1));

                        case nameof(Queryable.DefaultIfEmpty)
                        when genericMethod == QueryableMethods.DefaultIfEmptyWithoutArgument:
                            return TranslateDefaultIfEmpty(shapedQueryExpression, null);

                        case nameof(Queryable.DefaultIfEmpty)
                        when genericMethod == QueryableMethods.DefaultIfEmptyWithArgument:
                            return TranslateDefaultIfEmpty(shapedQueryExpression, methodCallExpression.Arguments[1]);

                        case nameof(Queryable.Distinct)
                        when genericMethod == QueryableMethods.Distinct:
                            return TranslateDistinct(shapedQueryExpression);

                        case nameof(Queryable.ElementAt)
                        when genericMethod == QueryableMethods.ElementAt:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateElementAtOrDefault(shapedQueryExpression, methodCallExpression.Arguments[1], false);

                        case nameof(Queryable.ElementAtOrDefault)
                        when genericMethod == QueryableMethods.ElementAtOrDefault:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.SingleOrDefault;
                            return TranslateElementAtOrDefault(shapedQueryExpression, methodCallExpression.Arguments[1], true);

                        case nameof(Queryable.Except)
                        when genericMethod == QueryableMethods.Except:
                        {
                            var source2 = Visit(methodCallExpression.Arguments[1]);
                            if (source2 is ShapedQueryExpression innerShapedQueryExpression)
                            {
                                return TranslateExcept(
                                    shapedQueryExpression,
                                    innerShapedQueryExpression);
                            }
                            break;
                        }

                        case nameof(Queryable.First)
                        when genericMethod == QueryableMethods.FirstWithoutPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateFirstOrDefault(shapedQueryExpression, null, methodCallExpression.Type, false);

                        case nameof(Queryable.First)
                        when genericMethod == QueryableMethods.FirstWithPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateFirstOrDefault(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, false);

                        case nameof(Queryable.FirstOrDefault)
                        when genericMethod == QueryableMethods.FirstOrDefaultWithoutPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.SingleOrDefault;
                            return TranslateFirstOrDefault(shapedQueryExpression, null, methodCallExpression.Type, true);

                        case nameof(Queryable.FirstOrDefault)
                        when genericMethod == QueryableMethods.FirstOrDefaultWithPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.SingleOrDefault;
                            return TranslateFirstOrDefault(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, true);

                        case nameof(Queryable.GroupBy)
                        when genericMethod == QueryableMethods.GroupByWithKeySelector:
                            return TranslateGroupBy(shapedQueryExpression, GetLambdaExpressionFromArgument(1), null, null);

                        case nameof(Queryable.GroupBy)
                        when genericMethod == QueryableMethods.GroupByWithKeyElementSelector:
                            return TranslateGroupBy(shapedQueryExpression, GetLambdaExpressionFromArgument(1), GetLambdaExpressionFromArgument(2), null);

                        case nameof(Queryable.GroupBy)
                        when genericMethod == QueryableMethods.GroupByWithKeyElementResultSelector:
                            return TranslateGroupBy(shapedQueryExpression, GetLambdaExpressionFromArgument(1), GetLambdaExpressionFromArgument(2), GetLambdaExpressionFromArgument(3));

                        case nameof(Queryable.GroupBy)
                        when genericMethod == QueryableMethods.GroupByWithKeyResultSelector:
                            return TranslateGroupBy(shapedQueryExpression, GetLambdaExpressionFromArgument(1), null, GetLambdaExpressionFromArgument(2));

                        case nameof(Queryable.GroupJoin)
                        when genericMethod == QueryableMethods.GroupJoin:
                        {
                            if (Visit(methodCallExpression.Arguments[1]) is ShapedQueryExpression innerShapedQueryExpression)
                            {
                                return TranslateGroupJoin(shapedQueryExpression, innerShapedQueryExpression, GetLambdaExpressionFromArgument(2), GetLambdaExpressionFromArgument(3), GetLambdaExpressionFromArgument(4));
                            }
                            break;
                        }

                        case nameof(Queryable.Intersect)
                        when genericMethod == QueryableMethods.Intersect:
                        {
                            if (Visit(methodCallExpression.Arguments[1]) is ShapedQueryExpression innerShapedQueryExpression)
                            {
                                return TranslateIntersect(
                                    shapedQueryExpression,
                                    innerShapedQueryExpression);
                            }
                            break;
                        }

                        case nameof(Queryable.Join)
                        when genericMethod == QueryableMethods.Join:
                        {
                            if (Visit(methodCallExpression.Arguments[1]) is ShapedQueryExpression innerShapedQueryExpression)
                            {
                                return TranslateJoin(shapedQueryExpression, innerShapedQueryExpression, GetLambdaExpressionFromArgument(2), GetLambdaExpressionFromArgument(3), GetLambdaExpressionFromArgument(4));
                            }
                            break;
                        }

                        case nameof(QueryableExtensions.LeftJoin)
                        when genericMethod == QueryableExtensions.LeftJoinMethodInfo:
                        {
                            if (Visit(methodCallExpression.Arguments[1]) is ShapedQueryExpression innerShapedQueryExpression)
                            {
                                return TranslateLeftJoin(shapedQueryExpression, innerShapedQueryExpression, GetLambdaExpressionFromArgument(2), GetLambdaExpressionFromArgument(3), GetLambdaExpressionFromArgument(4));
                            }
                            break;
                        }

                        case nameof(Queryable.Last)
                        when genericMethod == QueryableMethods.LastWithoutPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateLastOrDefault(shapedQueryExpression, null, methodCallExpression.Type, false);

                        case nameof(Queryable.Last)
                        when genericMethod == QueryableMethods.LastWithPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateLastOrDefault(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, false);

                        case nameof(Queryable.LastOrDefault)
                        when genericMethod == QueryableMethods.LastOrDefaultWithoutPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.SingleOrDefault;
                            return TranslateLastOrDefault(shapedQueryExpression, null, methodCallExpression.Type, true);

                        case nameof(Queryable.LastOrDefault)
                        when genericMethod == QueryableMethods.LastOrDefaultWithPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.SingleOrDefault;
                            return TranslateLastOrDefault(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, true);

                        case nameof(Queryable.LongCount)
                        when genericMethod == QueryableMethods.LongCountWithoutPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateLongCount(shapedQueryExpression, null);

                        case nameof(Queryable.LongCount)
                        when genericMethod == QueryableMethods.LongCountWithPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateLongCount(shapedQueryExpression, GetLambdaExpressionFromArgument(1));

                        case nameof(Queryable.Max)
                        when genericMethod == QueryableMethods.MaxWithoutSelector:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateMax(shapedQueryExpression, null, methodCallExpression.Type);

                        case nameof(Queryable.Max)
                        when genericMethod == QueryableMethods.MaxWithSelector:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateMax(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type);

                        case nameof(Queryable.Min)
                        when genericMethod == QueryableMethods.MinWithoutSelector:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateMin(shapedQueryExpression, null, methodCallExpression.Type);

                        case nameof(Queryable.Min)
                        when genericMethod == QueryableMethods.MinWithSelector:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateMin(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type);

                        case nameof(Queryable.OfType)
                        when genericMethod == QueryableMethods.OfType:
                            return TranslateOfType(shapedQueryExpression, method.GetGenericArguments()[0]);

                        case nameof(Queryable.OrderBy)
                        when genericMethod == QueryableMethods.OrderBy:
                            return TranslateOrderBy(shapedQueryExpression, GetLambdaExpressionFromArgument(1), true);

                        case nameof(Queryable.OrderByDescending)
                        when genericMethod == QueryableMethods.OrderByDescending:
                            return TranslateOrderBy(shapedQueryExpression, GetLambdaExpressionFromArgument(1), false);

                        case nameof(Queryable.Reverse)
                        when genericMethod == QueryableMethods.Reverse:
                            return TranslateReverse(shapedQueryExpression);

                        case nameof(Queryable.Select)
                        when genericMethod == QueryableMethods.Select:
                            return TranslateSelect(shapedQueryExpression, GetLambdaExpressionFromArgument(1));

                        case nameof(Queryable.SelectMany)
                        when genericMethod == QueryableMethods.SelectManyWithoutCollectionSelector:
                            return TranslateSelectMany(shapedQueryExpression, GetLambdaExpressionFromArgument(1));

                        case nameof(Queryable.SelectMany)
                        when genericMethod == QueryableMethods.SelectManyWithCollectionSelector:
                            return TranslateSelectMany(shapedQueryExpression, GetLambdaExpressionFromArgument(1), GetLambdaExpressionFromArgument(2));

                        case nameof(Queryable.Single)
                        when genericMethod == QueryableMethods.SingleWithoutPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateSingleOrDefault(shapedQueryExpression, null, methodCallExpression.Type, false);

                        case nameof(Queryable.Single)
                        when genericMethod == QueryableMethods.SingleWithPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateSingleOrDefault(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, false);

                        case nameof(Queryable.SingleOrDefault)
                        when genericMethod == QueryableMethods.SingleOrDefaultWithoutPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.SingleOrDefault;
                            return TranslateSingleOrDefault(shapedQueryExpression, null, methodCallExpression.Type, true);

                        case nameof(Queryable.SingleOrDefault)
                        when genericMethod == QueryableMethods.SingleOrDefaultWithPredicate:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.SingleOrDefault;
                            return TranslateSingleOrDefault(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, true);

                        case nameof(Queryable.Skip)
                        when genericMethod == QueryableMethods.Skip:
                            return TranslateSkip(shapedQueryExpression, methodCallExpression.Arguments[1]);

                        case nameof(Queryable.SkipWhile)
                        when genericMethod == QueryableMethods.SkipWhile:
                            return TranslateSkipWhile(shapedQueryExpression, GetLambdaExpressionFromArgument(1));

                        case nameof(Queryable.Sum)
                        when QueryableMethods.IsSumWithoutSelector(method):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateSum(shapedQueryExpression, null, methodCallExpression.Type);

                        case nameof(Queryable.Sum)
                        when QueryableMethods.IsSumWithSelector(method):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateSum(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type);

                        case nameof(Queryable.Take)
                        when genericMethod == QueryableMethods.Take:
                            return TranslateTake(shapedQueryExpression, methodCallExpression.Arguments[1]);

                        case nameof(Queryable.TakeWhile)
                        when genericMethod == QueryableMethods.TakeWhile:
                            return TranslateTakeWhile(shapedQueryExpression, GetLambdaExpressionFromArgument(1));

                        case nameof(Queryable.ThenBy)
                        when genericMethod == QueryableMethods.ThenBy:
                            return TranslateThenBy(shapedQueryExpression, GetLambdaExpressionFromArgument(1), true);

                        case nameof(Queryable.ThenByDescending)
                        when genericMethod == QueryableMethods.ThenByDescending:
                            return TranslateThenBy(shapedQueryExpression, GetLambdaExpressionFromArgument(1), false);

                        case nameof(Queryable.Union)
                        when genericMethod == QueryableMethods.Union:
                        {
                            if (Visit(methodCallExpression.Arguments[1]) is ShapedQueryExpression innerShapedQueryExpression)
                            {
                                return TranslateUnion(shapedQueryExpression, innerShapedQueryExpression);
                            }
                            break;
                        }

                        case nameof(Queryable.Where)
                        when genericMethod == QueryableMethods.Where:
                            return TranslateWhere(shapedQueryExpression, GetLambdaExpressionFromArgument(1));

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
        public abstract ShapedQueryExpression TranslateSubquery(Expression expression);
    }
}
