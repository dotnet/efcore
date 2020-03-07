// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class QueryableMethodTranslatingExpressionVisitor : ExpressionVisitor
    {
        private readonly bool _subquery;
        private readonly EntityShaperNullableMarkingExpressionVisitor _entityShaperNullableMarkingExpressionVisitor;

        protected QueryableMethodTranslatingExpressionVisitor(
            [NotNull] QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
            bool subquery)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
            _subquery = subquery;
            _entityShaperNullableMarkingExpressionVisitor = new EntityShaperNullableMarkingExpressionVisitor();
        }

        protected virtual QueryableMethodTranslatingExpressionVisitorDependencies Dependencies { get; }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            Check.NotNull(extensionExpression, nameof(extensionExpression));

            return extensionExpression switch
            {
                ShapedQueryExpression _ => extensionExpression,
                QueryRootExpression queryRootExpression => CreateShapedQueryExpression(queryRootExpression.EntityType),
                _ => base.VisitExtension(extensionExpression),
            };
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            ShapedQueryExpression CheckTranslated(ShapedQueryExpression translated)
            {
                return translated ?? throw new InvalidOperationException(CoreStrings.TranslationFailed(methodCallExpression.Print()));
            }

            var method = methodCallExpression.Method;
            if (method.DeclaringType == typeof(Queryable)
                || method.DeclaringType == typeof(QueryableExtensions))
            {
                var source = Visit(methodCallExpression.Arguments[0]);
                if (source is ShapedQueryExpression shapedQueryExpression)
                {
                    var genericMethod = method.IsGenericMethod ? method.GetGenericMethodDefinition() : null;
                    switch (method.Name)
                    {
                        case nameof(Queryable.All)
                            when genericMethod == QueryableMethods.All:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(TranslateAll(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                        case nameof(Queryable.Any)
                            when genericMethod == QueryableMethods.AnyWithoutPredicate:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(TranslateAny(shapedQueryExpression, null));

                        case nameof(Queryable.Any)
                            when genericMethod == QueryableMethods.AnyWithPredicate:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(TranslateAny(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                        case nameof(Queryable.AsQueryable)
                            when genericMethod == QueryableMethods.AsQueryable:
                            return source;

                        case nameof(Queryable.Average)
                            when QueryableMethods.IsAverageWithoutSelector(method):
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(TranslateAverage(shapedQueryExpression, null, methodCallExpression.Type));

                        case nameof(Queryable.Average)
                            when QueryableMethods.IsAverageWithSelector(method):
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(
                                TranslateAverage(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type));

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
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(TranslateContains(shapedQueryExpression, methodCallExpression.Arguments[1]));

                        case nameof(Queryable.Count)
                            when genericMethod == QueryableMethods.CountWithoutPredicate:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(TranslateCount(shapedQueryExpression, null));

                        case nameof(Queryable.Count)
                            when genericMethod == QueryableMethods.CountWithPredicate:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
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
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(
                                TranslateElementAtOrDefault(shapedQueryExpression, methodCallExpression.Arguments[1], false));

                        case nameof(Queryable.ElementAtOrDefault)
                            when genericMethod == QueryableMethods.ElementAtOrDefault:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.SingleOrDefault);
                            return CheckTranslated(
                                TranslateElementAtOrDefault(shapedQueryExpression, methodCallExpression.Arguments[1], true));

                        case nameof(Queryable.Except)
                            when genericMethod == QueryableMethods.Except:
                        {
                            var source2 = Visit(methodCallExpression.Arguments[1]);
                            if (source2 is ShapedQueryExpression innerShapedQueryExpression)
                            {
                                return CheckTranslated(
                                    TranslateExcept(
                                        shapedQueryExpression,
                                        innerShapedQueryExpression));
                            }

                            break;
                        }

                        case nameof(Queryable.First)
                            when genericMethod == QueryableMethods.FirstWithoutPredicate:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(TranslateFirstOrDefault(shapedQueryExpression, null, methodCallExpression.Type, false));

                        case nameof(Queryable.First)
                            when genericMethod == QueryableMethods.FirstWithPredicate:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(
                                TranslateFirstOrDefault(
                                    shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, false));

                        case nameof(Queryable.FirstOrDefault)
                            when genericMethod == QueryableMethods.FirstOrDefaultWithoutPredicate:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.SingleOrDefault);
                            return CheckTranslated(TranslateFirstOrDefault(shapedQueryExpression, null, methodCallExpression.Type, true));

                        case nameof(Queryable.FirstOrDefault)
                            when genericMethod == QueryableMethods.FirstOrDefaultWithPredicate:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.SingleOrDefault);
                            return CheckTranslated(
                                TranslateFirstOrDefault(
                                    shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, true));

                        case nameof(Queryable.GroupBy)
                            when genericMethod == QueryableMethods.GroupByWithKeySelector:
                            return CheckTranslated(TranslateGroupBy(shapedQueryExpression, GetLambdaExpressionFromArgument(1), null, null));

                        case nameof(Queryable.GroupBy)
                            when genericMethod == QueryableMethods.GroupByWithKeyElementSelector:
                            return CheckTranslated(
                                TranslateGroupBy(
                                    shapedQueryExpression, GetLambdaExpressionFromArgument(1), GetLambdaExpressionFromArgument(2), null));

                        case nameof(Queryable.GroupBy)
                            when genericMethod == QueryableMethods.GroupByWithKeyElementResultSelector:
                            return CheckTranslated(
                                TranslateGroupBy(
                                    shapedQueryExpression, GetLambdaExpressionFromArgument(1), GetLambdaExpressionFromArgument(2),
                                    GetLambdaExpressionFromArgument(3)));

                        case nameof(Queryable.GroupBy)
                            when genericMethod == QueryableMethods.GroupByWithKeyResultSelector:
                            return CheckTranslated(
                                TranslateGroupBy(
                                    shapedQueryExpression, GetLambdaExpressionFromArgument(1), null, GetLambdaExpressionFromArgument(2)));

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
                                return CheckTranslated(
                                    TranslateJoin(
                                        shapedQueryExpression, innerShapedQueryExpression, GetLambdaExpressionFromArgument(2),
                                        GetLambdaExpressionFromArgument(3), GetLambdaExpressionFromArgument(4)));
                            }

                            break;
                        }

                        case nameof(QueryableExtensions.LeftJoin)
                            when genericMethod == QueryableExtensions.LeftJoinMethodInfo:
                        {
                            if (Visit(methodCallExpression.Arguments[1]) is ShapedQueryExpression innerShapedQueryExpression)
                            {
                                return CheckTranslated(
                                    TranslateLeftJoin(
                                        shapedQueryExpression, innerShapedQueryExpression, GetLambdaExpressionFromArgument(2),
                                        GetLambdaExpressionFromArgument(3), GetLambdaExpressionFromArgument(4)));
                            }

                            break;
                        }

                        case nameof(Queryable.Last)
                            when genericMethod == QueryableMethods.LastWithoutPredicate:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(TranslateLastOrDefault(shapedQueryExpression, null, methodCallExpression.Type, false));

                        case nameof(Queryable.Last)
                            when genericMethod == QueryableMethods.LastWithPredicate:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(
                                TranslateLastOrDefault(
                                    shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, false));

                        case nameof(Queryable.LastOrDefault)
                            when genericMethod == QueryableMethods.LastOrDefaultWithoutPredicate:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.SingleOrDefault);
                            return CheckTranslated(TranslateLastOrDefault(shapedQueryExpression, null, methodCallExpression.Type, true));

                        case nameof(Queryable.LastOrDefault)
                            when genericMethod == QueryableMethods.LastOrDefaultWithPredicate:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.SingleOrDefault);
                            return CheckTranslated(
                                TranslateLastOrDefault(
                                    shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, true));

                        case nameof(Queryable.LongCount)
                            when genericMethod == QueryableMethods.LongCountWithoutPredicate:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(TranslateLongCount(shapedQueryExpression, null));

                        case nameof(Queryable.LongCount)
                            when genericMethod == QueryableMethods.LongCountWithPredicate:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(TranslateLongCount(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                        case nameof(Queryable.Max)
                            when genericMethod == QueryableMethods.MaxWithoutSelector:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(TranslateMax(shapedQueryExpression, null, methodCallExpression.Type));

                        case nameof(Queryable.Max)
                            when genericMethod == QueryableMethods.MaxWithSelector:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(
                                TranslateMax(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type));

                        case nameof(Queryable.Min)
                            when genericMethod == QueryableMethods.MinWithoutSelector:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(TranslateMin(shapedQueryExpression, null, methodCallExpression.Type));

                        case nameof(Queryable.Min)
                            when genericMethod == QueryableMethods.MinWithSelector:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(
                                TranslateMin(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type));

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
                            return CheckTranslated(
                                TranslateSelectMany(
                                    shapedQueryExpression, GetLambdaExpressionFromArgument(1), GetLambdaExpressionFromArgument(2)));

                        case nameof(Queryable.Single)
                            when genericMethod == QueryableMethods.SingleWithoutPredicate:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(TranslateSingleOrDefault(shapedQueryExpression, null, methodCallExpression.Type, false));

                        case nameof(Queryable.Single)
                            when genericMethod == QueryableMethods.SingleWithPredicate:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(
                                TranslateSingleOrDefault(
                                    shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, false));

                        case nameof(Queryable.SingleOrDefault)
                            when genericMethod == QueryableMethods.SingleOrDefaultWithoutPredicate:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.SingleOrDefault);
                            return CheckTranslated(TranslateSingleOrDefault(shapedQueryExpression, null, methodCallExpression.Type, true));

                        case nameof(Queryable.SingleOrDefault)
                            when genericMethod == QueryableMethods.SingleOrDefaultWithPredicate:
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.SingleOrDefault);
                            return CheckTranslated(
                                TranslateSingleOrDefault(
                                    shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, true));

                        case nameof(Queryable.Skip)
                            when genericMethod == QueryableMethods.Skip:
                            return CheckTranslated(TranslateSkip(shapedQueryExpression, methodCallExpression.Arguments[1]));

                        case nameof(Queryable.SkipWhile)
                            when genericMethod == QueryableMethods.SkipWhile:
                            return CheckTranslated(TranslateSkipWhile(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                        case nameof(Queryable.Sum)
                            when QueryableMethods.IsSumWithoutSelector(method):
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(TranslateSum(shapedQueryExpression, null, methodCallExpression.Type));

                        case nameof(Queryable.Sum)
                            when QueryableMethods.IsSumWithSelector(method):
                            shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                            return CheckTranslated(
                                TranslateSum(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type));

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

                            LambdaExpression GetLambdaExpressionFromArgument(int argumentIndex) =>
                                methodCallExpression.Arguments[argumentIndex].UnwrapLambdaFromQuote();
                    }
                }
            }

            return _subquery
                ? (Expression)null
                : throw new NotImplementedException(CoreStrings.UnhandledMethod(method.Name));
        }

        private sealed class EntityShaperNullableMarkingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitExtension(Expression extensionExpression)
            {
                Check.NotNull(extensionExpression, nameof(extensionExpression));

                return extensionExpression is EntityShaperExpression entityShaper
                    ? entityShaper.MarkAsNullable()
                    : base.VisitExtension(extensionExpression);
            }
        }

        protected virtual Expression MarkShaperNullable([NotNull] Expression shaperExpression)
        {
            Check.NotNull(shaperExpression, nameof(shaperExpression));

            return _entityShaperNullableMarkingExpressionVisitor.Visit(shaperExpression);
        }

        protected virtual ShapedQueryExpression TranslateResultSelectorForJoin(
            [NotNull] ShapedQueryExpression outer,
            [NotNull] LambdaExpression resultSelector,
            [NotNull] Expression innerShaper,
            [CanBeNull] Type transparentIdentifierType)
        {
            Check.NotNull(outer, nameof(outer));
            Check.NotNull(resultSelector, nameof(resultSelector));
            Check.NotNull(innerShaper, nameof(innerShaper));

            outer = outer.UpdateShaperExpression(
                CombineShapers(outer.QueryExpression, outer.ShaperExpression, innerShaper, transparentIdentifierType));

            var transparentIdentifierParameter = Expression.Parameter(transparentIdentifierType);

            Expression original1 = resultSelector.Parameters[0];
            var replacement1 = AccessOuterTransparentField(transparentIdentifierType, transparentIdentifierParameter);
            Expression original2 = resultSelector.Parameters[1];
            var replacement2 = AccessInnerTransparentField(transparentIdentifierType, transparentIdentifierParameter);
            var newResultSelector = Expression.Lambda(
                new ReplacingExpressionVisitor(
                    new[] { original1, original2 }, new[] { replacement1, replacement2 })
                    .Visit(resultSelector.Body),
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
                new[] { outerShaper, innerShaper }, outerMemberInfo, innerMemberInfo);
        }

        private sealed class MemberAccessShiftingExpressionVisitor : ExpressionVisitor
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
                Check.NotNull(node, nameof(node));

                return node is ProjectionBindingExpression projectionBindingExpression
                    ? new ProjectionBindingExpression(
                        _queryExpression,
                        projectionBindingExpression.ProjectionMember.Prepend(_memberShift),
                        projectionBindingExpression.Type)
                    : base.VisitExtension(node);
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

        public virtual ShapedQueryExpression TranslateSubquery([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            return (ShapedQueryExpression)CreateSubqueryVisitor().Visit(expression);
        }

        protected abstract QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor();

        [Obsolete("Use overload which takes IEntityType.")]
        protected abstract ShapedQueryExpression CreateShapedQueryExpression([NotNull] Type elementType);
        protected abstract ShapedQueryExpression CreateShapedQueryExpression([NotNull] IEntityType entityType);
        protected abstract ShapedQueryExpression TranslateAll([NotNull] ShapedQueryExpression source, [NotNull] LambdaExpression predicate);
        protected abstract ShapedQueryExpression TranslateAny([NotNull] ShapedQueryExpression source, [NotNull] LambdaExpression predicate);

        protected abstract ShapedQueryExpression TranslateAverage(
            [NotNull] ShapedQueryExpression source, [CanBeNull] LambdaExpression selector, [NotNull] Type resultType);

        protected abstract ShapedQueryExpression TranslateCast([NotNull] ShapedQueryExpression source, [NotNull] Type resultType);

        protected abstract ShapedQueryExpression TranslateConcat(
            [NotNull] ShapedQueryExpression source1, [NotNull] ShapedQueryExpression source2);
        protected abstract ShapedQueryExpression TranslateContains([NotNull] ShapedQueryExpression source, [NotNull] Expression item);

        protected abstract ShapedQueryExpression TranslateCount(
            [NotNull] ShapedQueryExpression source, [CanBeNull] LambdaExpression predicate);

        protected abstract ShapedQueryExpression TranslateDefaultIfEmpty(
            [NotNull] ShapedQueryExpression source, [CanBeNull] Expression defaultValue);

        protected abstract ShapedQueryExpression TranslateDistinct([NotNull] ShapedQueryExpression source);

        protected abstract ShapedQueryExpression TranslateElementAtOrDefault(
            [NotNull] ShapedQueryExpression source, [NotNull] Expression index, bool returnDefault);

        protected abstract ShapedQueryExpression TranslateExcept(
            [NotNull] ShapedQueryExpression source1, [NotNull] ShapedQueryExpression source2);

        protected abstract ShapedQueryExpression TranslateFirstOrDefault(
            [NotNull] ShapedQueryExpression source, [CanBeNull] LambdaExpression predicate, [NotNull] Type returnType, bool returnDefault);

        protected abstract ShapedQueryExpression TranslateGroupBy(
            [NotNull] ShapedQueryExpression source,
            [NotNull] LambdaExpression keySelector,
            [CanBeNull] LambdaExpression elementSelector,
            [CanBeNull] LambdaExpression resultSelector);

        protected abstract ShapedQueryExpression TranslateGroupJoin(
            [NotNull] ShapedQueryExpression outer,
            [NotNull] ShapedQueryExpression inner,
            [NotNull] LambdaExpression outerKeySelector,
            [NotNull] LambdaExpression innerKeySelector,
            [NotNull] LambdaExpression resultSelector);

        protected abstract ShapedQueryExpression TranslateIntersect(
            [NotNull] ShapedQueryExpression source1, [NotNull] ShapedQueryExpression source2);

        protected abstract ShapedQueryExpression TranslateJoin(
            [NotNull] ShapedQueryExpression outer,
            [NotNull] ShapedQueryExpression inner,
            [CanBeNull] LambdaExpression outerKeySelector,
            [CanBeNull] LambdaExpression innerKeySelector,
            [NotNull] LambdaExpression resultSelector);

        protected abstract ShapedQueryExpression TranslateLeftJoin(
            [NotNull] ShapedQueryExpression outer,
            [NotNull] ShapedQueryExpression inner,
            [CanBeNull] LambdaExpression outerKeySelector,
            [CanBeNull] LambdaExpression innerKeySelector,
            [NotNull] LambdaExpression resultSelector);

        protected abstract ShapedQueryExpression TranslateLastOrDefault(
            [NotNull] ShapedQueryExpression source, [CanBeNull] LambdaExpression predicate, [NotNull] Type returnType, bool returnDefault);

        protected abstract ShapedQueryExpression TranslateLongCount(
            [NotNull] ShapedQueryExpression source, [CanBeNull] LambdaExpression predicate);

        protected abstract ShapedQueryExpression TranslateMax(
            [NotNull] ShapedQueryExpression source, [CanBeNull] LambdaExpression selector, [NotNull] Type resultType);

        protected abstract ShapedQueryExpression TranslateMin(
            [NotNull] ShapedQueryExpression source, [CanBeNull] LambdaExpression selector, [NotNull] Type resultType);

        protected abstract ShapedQueryExpression TranslateOfType([NotNull] ShapedQueryExpression source, [NotNull] Type resultType);

        protected abstract ShapedQueryExpression TranslateOrderBy(
            [NotNull] ShapedQueryExpression source, [NotNull] LambdaExpression keySelector, bool ascending);

        protected abstract ShapedQueryExpression TranslateReverse([NotNull] ShapedQueryExpression source);

        protected abstract ShapedQueryExpression TranslateSelect(
            [NotNull] ShapedQueryExpression source, [NotNull] LambdaExpression selector);

        protected abstract ShapedQueryExpression TranslateSelectMany(
            [NotNull] ShapedQueryExpression source, [NotNull] LambdaExpression collectionSelector, [NotNull] LambdaExpression resultSelector);

        protected abstract ShapedQueryExpression TranslateSelectMany(
            [NotNull] ShapedQueryExpression source, [NotNull] LambdaExpression selector);

        protected abstract ShapedQueryExpression TranslateSingleOrDefault(
            [NotNull] ShapedQueryExpression source, [CanBeNull] LambdaExpression predicate, [NotNull] Type returnType, bool returnDefault);

        protected abstract ShapedQueryExpression TranslateSkip(
            [NotNull] ShapedQueryExpression source, [NotNull] Expression count);

        protected abstract ShapedQueryExpression TranslateSkipWhile(
            [NotNull] ShapedQueryExpression source, [NotNull] LambdaExpression predicate);

        protected abstract ShapedQueryExpression TranslateSum(
            [NotNull] ShapedQueryExpression source, [CanBeNull] LambdaExpression selector, [NotNull] Type resultType);

        protected abstract ShapedQueryExpression TranslateTake([NotNull] ShapedQueryExpression source, [NotNull] Expression count);

        protected abstract ShapedQueryExpression TranslateTakeWhile(
            [NotNull] ShapedQueryExpression source, [NotNull] LambdaExpression predicate);

        protected abstract ShapedQueryExpression TranslateThenBy(
            [NotNull] ShapedQueryExpression source, [NotNull] LambdaExpression keySelector, bool ascending);

        protected abstract ShapedQueryExpression TranslateUnion(
            [NotNull] ShapedQueryExpression source1, [NotNull] ShapedQueryExpression source2);

        protected abstract ShapedQueryExpression TranslateWhere(
            [NotNull] ShapedQueryExpression source, [NotNull] LambdaExpression predicate);
    }
}
