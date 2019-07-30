// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class QueryableMethodTranslatingExpressionVisitor : ExpressionVisitor
    {
        private readonly bool _subquery;

        protected QueryableMethodTranslatingExpressionVisitor(
            QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
            bool subquery)
        {
            Dependencies = dependencies;
            _subquery = subquery;
        }

        protected virtual QueryableMethodTranslatingExpressionVisitorDependencies Dependencies { get; }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
            => constantExpression.IsEntityQueryable()
                ? CreateShapedQueryExpression(((IQueryable)constantExpression.Value).ElementType)
                : base.VisitConstant(constantExpression);

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType == typeof(Queryable)
                || methodCallExpression.Method.DeclaringType == typeof(QueryableExtensions))
            {
                var source = Visit(methodCallExpression.Arguments[0]);
                if (source is ShapedQueryExpression shapedQueryExpression)
                {
                    var argumentCount = methodCallExpression.Arguments.Count;
                    switch (methodCallExpression.Method.Name)
                    {
                        case nameof(Queryable.Aggregate):
                            // Don't know
                            break;

                        case nameof(Queryable.All):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateAll(
                                shapedQueryExpression,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());

                        case nameof(Queryable.Any):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateAny(
                                shapedQueryExpression,
                                methodCallExpression.Arguments.Count == 2
                                    ? methodCallExpression.Arguments[1].UnwrapLambdaFromQuote()
                                    : null);

                        case nameof(Queryable.AsQueryable):
                            return source;

                        case nameof(Queryable.Average):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateAverage(
                                shapedQueryExpression,
                                methodCallExpression.Arguments.Count == 2
                                    ? methodCallExpression.Arguments[1].UnwrapLambdaFromQuote()
                                    : null,
                                methodCallExpression.Type);

                        case nameof(Queryable.Cast):
                            return TranslateCast(shapedQueryExpression, methodCallExpression.Method.GetGenericArguments()[0]);

                        case nameof(Queryable.Concat):
                        {
                            var source2 = Visit(methodCallExpression.Arguments[1]);
                            if (source2 is ShapedQueryExpression innerShapedQueryExpression)
                            {
                                return TranslateConcat(
                                    shapedQueryExpression,
                                    innerShapedQueryExpression);
                            }
                        }

                            break;

                        case nameof(Queryable.Contains)
                            when argumentCount == 2:
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateContains(shapedQueryExpression, methodCallExpression.Arguments[1]);

                        case nameof(Queryable.Count):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateCount(
                                shapedQueryExpression,
                                methodCallExpression.Arguments.Count == 2
                                    ? methodCallExpression.Arguments[1].UnwrapLambdaFromQuote()
                                    : null);

                        case nameof(Queryable.DefaultIfEmpty):
                            return TranslateDefaultIfEmpty(
                                shapedQueryExpression,
                                methodCallExpression.Arguments.Count == 2
                                    ? methodCallExpression.Arguments[1]
                                    : null);

                        case nameof(Queryable.Distinct)
                            when argumentCount == 1:
                            return TranslateDistinct(shapedQueryExpression);

                        case nameof(Queryable.ElementAt):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateElementAtOrDefault(shapedQueryExpression, methodCallExpression.Arguments[1], false);

                        case nameof(Queryable.ElementAtOrDefault):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.SingleOrDefault;
                            return TranslateElementAtOrDefault(shapedQueryExpression, methodCallExpression.Arguments[1], true);

                        case nameof(Queryable.Except)
                            when argumentCount == 2:
                        {
                            var source2 = Visit(methodCallExpression.Arguments[1]);
                            if (source2 is ShapedQueryExpression innerShapedQueryExpression)
                            {
                                return TranslateExcept(
                                    shapedQueryExpression,
                                    innerShapedQueryExpression);
                            }
                        }

                            break;

                        case nameof(Queryable.First):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateFirstOrDefault(
                                shapedQueryExpression,
                                methodCallExpression.Arguments.Count == 2
                                    ? methodCallExpression.Arguments[1].UnwrapLambdaFromQuote()
                                    : null,
                                methodCallExpression.Type,
                                false);

                        case nameof(Queryable.FirstOrDefault):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.SingleOrDefault;
                            return TranslateFirstOrDefault(
                                shapedQueryExpression,
                                methodCallExpression.Arguments.Count == 2
                                    ? methodCallExpression.Arguments[1].UnwrapLambdaFromQuote()
                                    : null,
                                methodCallExpression.Type,
                                true);

                        case nameof(Queryable.GroupBy):
                        {
                            var keySelector = methodCallExpression.Arguments[1].UnwrapLambdaFromQuote();
                            if (methodCallExpression.Arguments[argumentCount - 1] is ConstantExpression)
                            {
                                // This means last argument is EqualityComparer on key
                                // which is not supported
                                break;
                            }

                            switch (argumentCount)
                            {
                                case 2:
                                    return TranslateGroupBy(
                                        shapedQueryExpression,
                                        keySelector,
                                        null,
                                        null);

                                case 3:
                                    var lambda = methodCallExpression.Arguments[2].UnwrapLambdaFromQuote();
                                    if (lambda.Parameters.Count == 1)
                                    {
                                        return TranslateGroupBy(
                                            shapedQueryExpression,
                                            keySelector,
                                            lambda,
                                            null);
                                    }
                                    else
                                    {
                                        return TranslateGroupBy(
                                            shapedQueryExpression,
                                            keySelector,
                                            null,
                                            lambda);
                                    }

                                case 4:
                                    return TranslateGroupBy(
                                        shapedQueryExpression,
                                        keySelector,
                                        methodCallExpression.Arguments[2].UnwrapLambdaFromQuote(),
                                        methodCallExpression.Arguments[3].UnwrapLambdaFromQuote());
                            }
                        }

                            break;

                        case nameof(Queryable.GroupJoin)
                            when argumentCount == 5:
                        {
                            var innerSource = Visit(methodCallExpression.Arguments[1]);
                            if (innerSource is ShapedQueryExpression innerShapedQueryExpression)
                            {
                                return TranslateGroupJoin(
                                    shapedQueryExpression,
                                    innerShapedQueryExpression,
                                    methodCallExpression.Arguments[2].UnwrapLambdaFromQuote(),
                                    methodCallExpression.Arguments[3].UnwrapLambdaFromQuote(),
                                    methodCallExpression.Arguments[4].UnwrapLambdaFromQuote());
                            }
                        }

                            break;

                        case nameof(Queryable.Intersect)
                            when argumentCount == 2:
                        {
                            var source2 = Visit(methodCallExpression.Arguments[1]);
                            if (source2 is ShapedQueryExpression innerShapedQueryExpression)
                            {
                                return TranslateIntersect(
                                    shapedQueryExpression,
                                    innerShapedQueryExpression);
                            }
                        }

                            break;

                        case nameof(Queryable.Join)
                            when argumentCount == 5:
                        {
                            var innerSource = Visit(methodCallExpression.Arguments[1]);
                            if (innerSource is ShapedQueryExpression innerShapedQueryExpression)
                            {
                                return TranslateJoin(
                                    shapedQueryExpression,
                                    innerShapedQueryExpression,
                                    methodCallExpression.Arguments[2].UnwrapLambdaFromQuote(),
                                    methodCallExpression.Arguments[3].UnwrapLambdaFromQuote(),
                                    methodCallExpression.Arguments[4].UnwrapLambdaFromQuote());
                            }
                        }

                            break;

                        case nameof(QueryableExtensions.LeftJoin)
                            when argumentCount == 5:
                        {
                            var innerSource = Visit(methodCallExpression.Arguments[1]);
                            if (innerSource is ShapedQueryExpression innerShapedQueryExpression)
                            {
                                return TranslateLeftJoin(
                                    shapedQueryExpression,
                                    innerShapedQueryExpression,
                                    methodCallExpression.Arguments[2].UnwrapLambdaFromQuote(),
                                    methodCallExpression.Arguments[3].UnwrapLambdaFromQuote(),
                                    methodCallExpression.Arguments[4].UnwrapLambdaFromQuote());
                            }
                        }

                            break;

                        case nameof(Queryable.Last):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateLastOrDefault(
                                shapedQueryExpression,
                                methodCallExpression.Arguments.Count == 2
                                    ? methodCallExpression.Arguments[1].UnwrapLambdaFromQuote()
                                    : null,
                                methodCallExpression.Type,
                                false);

                        case nameof(Queryable.LastOrDefault):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.SingleOrDefault;
                            return TranslateLastOrDefault(
                                shapedQueryExpression,
                                methodCallExpression.Arguments.Count == 2
                                    ? methodCallExpression.Arguments[1].UnwrapLambdaFromQuote()
                                    : null,
                                methodCallExpression.Type,
                                true);

                        case nameof(Queryable.LongCount):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateLongCount(
                                shapedQueryExpression,
                                methodCallExpression.Arguments.Count == 2
                                    ? methodCallExpression.Arguments[1].UnwrapLambdaFromQuote()
                                    : null);

                        case nameof(Queryable.Max):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateMax(
                                shapedQueryExpression,
                                methodCallExpression.Arguments.Count == 2
                                    ? methodCallExpression.Arguments[1].UnwrapLambdaFromQuote()
                                    : null,
                                methodCallExpression.Type);

                        case nameof(Queryable.Min):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateMin(
                                shapedQueryExpression,
                                methodCallExpression.Arguments.Count == 2
                                    ? methodCallExpression.Arguments[1].UnwrapLambdaFromQuote()
                                    : null,
                                methodCallExpression.Type);

                        case nameof(Queryable.OfType):
                            return TranslateOfType(shapedQueryExpression, methodCallExpression.Method.GetGenericArguments()[0]);

                        case nameof(Queryable.OrderBy)
                            when argumentCount == 2:
                            return TranslateOrderBy(
                                shapedQueryExpression,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                                true);

                        case nameof(Queryable.OrderByDescending)
                            when argumentCount == 2:
                            return TranslateOrderBy(
                                shapedQueryExpression,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                                false);

                        case nameof(Queryable.Reverse):
                            return TranslateReverse(shapedQueryExpression);

                        case nameof(Queryable.Select):
                            return TranslateSelect(
                                shapedQueryExpression,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());

                        case nameof(Queryable.SelectMany):
                            return methodCallExpression.Arguments.Count == 2
                                ? TranslateSelectMany(
                                    shapedQueryExpression,
                                    methodCallExpression.Arguments[1].UnwrapLambdaFromQuote())
                                : TranslateSelectMany(
                                    shapedQueryExpression,
                                    methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                                    methodCallExpression.Arguments[2].UnwrapLambdaFromQuote());

                        case nameof(Queryable.SequenceEqual):
                            // don't know
                            break;

                        case nameof(Queryable.Single):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateSingleOrDefault(
                                shapedQueryExpression,
                                methodCallExpression.Arguments.Count == 2
                                    ? methodCallExpression.Arguments[1].UnwrapLambdaFromQuote()
                                    : null,
                                methodCallExpression.Type,
                                false);

                        case nameof(Queryable.SingleOrDefault):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.SingleOrDefault;
                            return TranslateSingleOrDefault(
                                shapedQueryExpression,
                                methodCallExpression.Arguments.Count == 2
                                    ? methodCallExpression.Arguments[1].UnwrapLambdaFromQuote()
                                    : null,
                                methodCallExpression.Type,
                                true);

                        case nameof(Queryable.Skip):
                            return TranslateSkip(shapedQueryExpression, methodCallExpression.Arguments[1]);

                        case nameof(Queryable.SkipWhile):
                            return TranslateSkipWhile(
                                shapedQueryExpression,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());

                        case nameof(Queryable.Sum):
                            shapedQueryExpression.ResultCardinality = ResultCardinality.Single;
                            return TranslateSum(
                                shapedQueryExpression,
                                methodCallExpression.Arguments.Count == 2
                                    ? methodCallExpression.Arguments[1].UnwrapLambdaFromQuote()
                                    : null,
                                methodCallExpression.Type);

                        case nameof(Queryable.Take):
                            return TranslateTake(shapedQueryExpression, methodCallExpression.Arguments[1]);

                        case nameof(Queryable.TakeWhile):
                            return TranslateTakeWhile(
                                shapedQueryExpression,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());

                        case nameof(Queryable.ThenBy)
                            when argumentCount == 2:
                            return TranslateThenBy(
                                shapedQueryExpression,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                                true);

                        case nameof(Queryable.ThenByDescending)
                            when argumentCount == 2:
                            return TranslateThenBy(
                                shapedQueryExpression,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                                false);

                        case nameof(Queryable.Union)
                            when argumentCount == 2:
                        {
                            var source2 = Visit(methodCallExpression.Arguments[1]);
                            if (source2 is ShapedQueryExpression innerShapedQueryExpression)
                            {
                                return TranslateUnion(
                                    shapedQueryExpression,
                                    innerShapedQueryExpression);
                            }
                        }

                            break;

                        case nameof(Queryable.Where):
                            return TranslateWhere(
                                shapedQueryExpression,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());

                        case nameof(Queryable.Zip):
                            // Don't know
                            break;
                    }
                }
            }

            // TODO: Skip ToOrderedQueryable method. See Issue#15591
            if (methodCallExpression.Method.DeclaringType == typeof(NavigationExpansionReducingVisitor)
                && methodCallExpression.Method.Name == nameof(NavigationExpansionReducingVisitor.ToOrderedQueryable))
            {
                return Visit(methodCallExpression.Arguments[0]);
            }

            return _subquery
                ? (Expression)null
                : throw new NotImplementedException("Unhandled method: " + methodCallExpression.Method.Name);
        }

        private class EntityShaperNullableMarkingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitExtension(Expression extensionExpression)
            {
                if (extensionExpression is EntityShaperExpression entityShaper)
                {
                    return entityShaper.MarkAsNullable();
                }

                return base.VisitExtension(extensionExpression);
            }
        }

        protected virtual ShapedQueryExpression TranslateResultSelectorForJoin(
            ShapedQueryExpression outer,
            LambdaExpression resultSelector,
            Expression innerShaper,
            Type transparentIdentifierType,
            bool innerNullable)
        {
            if (innerNullable)
            {
                innerShaper = new EntityShaperNullableMarkingExpressionVisitor().Visit(innerShaper);
            }

            outer.ShaperExpression = CombineShapers(
                outer.QueryExpression,
                outer.ShaperExpression,
                innerShaper,
                transparentIdentifierType);

            var transparentIdentifierParameter = Expression.Parameter(transparentIdentifierType);

            Expression original1 = resultSelector.Parameters[0];
            Expression replacement1 = AccessOuterTransparentField(transparentIdentifierType, transparentIdentifierParameter);
            Expression original2 = resultSelector.Parameters[1];
            Expression replacement2 = AccessInnerTransparentField(transparentIdentifierType, transparentIdentifierParameter);
            var newResultSelector = Expression.Lambda(
                new ReplacingExpressionVisitor(
                    new Dictionary<Expression, Expression> {
                        { original1, replacement1 },
                        { original2, replacement2 }
                    }).Visit(resultSelector.Body),
                transparentIdentifierParameter);

            return TranslateSelect(outer, newResultSelector);
        }

        protected virtual ShapedQueryExpression TranslateResultSelectorForGroupJoin(
#pragma warning disable IDE0060 // Remove unused parameter
            ShapedQueryExpression outer,
            Expression innerShaper,
            LambdaExpression outerKeySelector,
            LambdaExpression innerKeySelector,
            LambdaExpression resultSelector,
            Type transparentIdentifierType)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            throw new NotImplementedException();
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
