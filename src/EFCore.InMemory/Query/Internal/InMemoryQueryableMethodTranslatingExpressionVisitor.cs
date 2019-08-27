// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    public class InMemoryQueryableMethodTranslatingExpressionVisitor : QueryableMethodTranslatingExpressionVisitor
    {
        private static readonly MethodInfo _efPropertyMethod = typeof(EF).GetTypeInfo().GetDeclaredMethod(nameof(EF.Property));

        private readonly InMemoryExpressionTranslatingExpressionVisitor _expressionTranslator;
        private readonly InMemoryProjectionBindingExpressionVisitor _projectionBindingExpressionVisitor;
        private readonly IModel _model;

        public InMemoryQueryableMethodTranslatingExpressionVisitor(
            QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
            IModel model)
            : base(dependencies, subquery: false)
        {
            _expressionTranslator = new InMemoryExpressionTranslatingExpressionVisitor(this);
            _projectionBindingExpressionVisitor = new InMemoryProjectionBindingExpressionVisitor(this, _expressionTranslator);
            _model = model;
        }

        protected InMemoryQueryableMethodTranslatingExpressionVisitor(
            InMemoryQueryableMethodTranslatingExpressionVisitor parentVisitor)
            : base(parentVisitor.Dependencies, subquery: true)
        {
            _expressionTranslator = parentVisitor._expressionTranslator;
            _projectionBindingExpressionVisitor = new InMemoryProjectionBindingExpressionVisitor(this, _expressionTranslator);
            _model = parentVisitor._model;
        }

        protected override QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
            => new InMemoryQueryableMethodTranslatingExpressionVisitor(this);

        protected override ShapedQueryExpression CreateShapedQueryExpression(Type elementType)
        {
            var entityType = _model.FindEntityType(elementType);
            var queryExpression = new InMemoryQueryExpression(entityType);

            return new ShapedQueryExpression(
                queryExpression,
                new EntityShaperExpression(
                    entityType,
                    new ProjectionBindingExpression(
                        queryExpression,
                        new ProjectionMember(),
                        typeof(ValueBuffer)),
                    false));
        }

        protected override ShapedQueryExpression TranslateAll(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;
            predicate = TranslateLambdaExpression(source, predicate);
            if (predicate == null)
            {
                return null;
            }

            inMemoryQueryExpression.ServerQueryExpression =
                Expression.Call(
                    InMemoryLinqOperatorProvider.All.MakeGenericMethod(typeof(ValueBuffer)),
                    inMemoryQueryExpression.ServerQueryExpression,
                    predicate);

            source.ShaperExpression = inMemoryQueryExpression.GetSingleScalarProjection();

            return source;
        }

        protected override ShapedQueryExpression TranslateAny(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;

            if (predicate == null)
            {
                inMemoryQueryExpression.ServerQueryExpression = Expression.Call(
                    InMemoryLinqOperatorProvider.Any.MakeGenericMethod(typeof(ValueBuffer)),
                    inMemoryQueryExpression.ServerQueryExpression);
            }
            else
            {
                predicate = TranslateLambdaExpression(source, predicate);
                if (predicate == null)
                {
                    return null;
                }

                inMemoryQueryExpression.ServerQueryExpression = Expression.Call(
                    InMemoryLinqOperatorProvider.AnyPredicate.MakeGenericMethod(typeof(ValueBuffer)),
                    inMemoryQueryExpression.ServerQueryExpression,
                    predicate);
            }

            source.ShaperExpression = inMemoryQueryExpression.GetSingleScalarProjection();

            return source;
        }

        protected override ShapedQueryExpression TranslateAverage(ShapedQueryExpression source, LambdaExpression selector, Type resultType)
            => TranslateScalarAggregate(source, selector, nameof(Enumerable.Average));

        protected override ShapedQueryExpression TranslateCast(ShapedQueryExpression source, Type resultType)
        {
            if (source.ShaperExpression.Type == resultType)
            {
                return source;
            }

            source.ShaperExpression = Expression.Convert(source.ShaperExpression, resultType);

            return source;
        }

        protected override ShapedQueryExpression TranslateConcat(ShapedQueryExpression source1, ShapedQueryExpression source2)
            => TranslateSetOperation(InMemoryLinqOperatorProvider.Concat, source1, source2);

        protected override ShapedQueryExpression TranslateContains(ShapedQueryExpression source, Expression item)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;
            item = TranslateExpression(item);
            if (item == null)
            {
                return null;
            }

            inMemoryQueryExpression.ServerQueryExpression =
                Expression.Call(
                    InMemoryLinqOperatorProvider.Contains.MakeGenericMethod(item.Type),
                    Expression.Call(
                        InMemoryLinqOperatorProvider.Select.MakeGenericMethod(typeof(ValueBuffer), item.Type),
                        inMemoryQueryExpression.ServerQueryExpression,
                        Expression.Lambda(
                            inMemoryQueryExpression.GetMappedProjection(new ProjectionMember()), inMemoryQueryExpression.ValueBufferParameter)),
                    item);

            source.ShaperExpression = inMemoryQueryExpression.GetSingleScalarProjection();

            return source;
        }

        protected override ShapedQueryExpression TranslateCount(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;

            if (predicate == null)
            {
                inMemoryQueryExpression.ServerQueryExpression =
                    Expression.Call(
                        InMemoryLinqOperatorProvider.Count.MakeGenericMethod(typeof(ValueBuffer)),
                        inMemoryQueryExpression.ServerQueryExpression);
            }
            else
            {
                predicate = TranslateLambdaExpression(source, predicate);
                if (predicate == null)
                {
                    return null;
                }

                inMemoryQueryExpression.ServerQueryExpression =
                    Expression.Call(
                        InMemoryLinqOperatorProvider.CountPredicate.MakeGenericMethod(typeof(ValueBuffer)),
                        inMemoryQueryExpression.ServerQueryExpression,
                        predicate);
            }

            source.ShaperExpression = inMemoryQueryExpression.GetSingleScalarProjection();

            return source;
        }

        protected override ShapedQueryExpression TranslateDefaultIfEmpty(ShapedQueryExpression source, Expression defaultValue)
            => null;

        protected override ShapedQueryExpression TranslateDistinct(ShapedQueryExpression source)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;

            inMemoryQueryExpression.PushdownIntoSubquery();
            inMemoryQueryExpression.ServerQueryExpression
                = Expression.Call(
                    InMemoryLinqOperatorProvider.Distinct.MakeGenericMethod(typeof(ValueBuffer)),
                    inMemoryQueryExpression.ServerQueryExpression);

            return source;
        }

        protected override ShapedQueryExpression TranslateElementAtOrDefault(ShapedQueryExpression source, Expression index, bool returnDefault)
            => null;

        protected override ShapedQueryExpression TranslateExcept(ShapedQueryExpression source1, ShapedQueryExpression source2)
            => TranslateSetOperation(InMemoryLinqOperatorProvider.Except, source1, source2);

        protected override ShapedQueryExpression TranslateFirstOrDefault(ShapedQueryExpression source, LambdaExpression predicate, Type returnType, bool returnDefault)
        {
            return TranslateSingleResultOperator(
                source,
                predicate,
                returnType,
                returnDefault
                    ? InMemoryLinqOperatorProvider.FirstOrDefault
                    : InMemoryLinqOperatorProvider.First);
        }

        protected override ShapedQueryExpression TranslateGroupBy(ShapedQueryExpression source, LambdaExpression keySelector, LambdaExpression elementSelector, LambdaExpression resultSelector)
            => null;

        protected override ShapedQueryExpression TranslateGroupJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector)
            => null;

        protected override ShapedQueryExpression TranslateIntersect(ShapedQueryExpression source1, ShapedQueryExpression source2)
            => TranslateSetOperation(InMemoryLinqOperatorProvider.Intersect, source1, source2);

        protected override ShapedQueryExpression TranslateJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector)
        {
            outerKeySelector = TranslateLambdaExpression(outer, outerKeySelector);
            innerKeySelector = TranslateLambdaExpression(inner, innerKeySelector);
            if (outerKeySelector == null || innerKeySelector == null)
            {
                return null;
            }

            var transparentIdentifierType = TransparentIdentifierFactory.Create(
                resultSelector.Parameters[0].Type,
                resultSelector.Parameters[1].Type);

            ((InMemoryQueryExpression)outer.QueryExpression).AddInnerJoin(
                (InMemoryQueryExpression)inner.QueryExpression,
                outerKeySelector,
                innerKeySelector,
                transparentIdentifierType);

            return TranslateResultSelectorForJoin(
                outer,
                resultSelector,
                inner.ShaperExpression,
                transparentIdentifierType);
        }

        protected override ShapedQueryExpression TranslateLastOrDefault(ShapedQueryExpression source, LambdaExpression predicate, Type returnType, bool returnDefault)
        {
            return TranslateSingleResultOperator(
                source,
                predicate,
                returnType,
                returnDefault
                    ? InMemoryLinqOperatorProvider.LastOrDefault
                    : InMemoryLinqOperatorProvider.Last);
        }

        protected override ShapedQueryExpression TranslateLeftJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector)
        {
            outerKeySelector = TranslateLambdaExpression(outer, outerKeySelector);
            innerKeySelector = TranslateLambdaExpression(inner, innerKeySelector);
            if (outerKeySelector == null || innerKeySelector == null)
            {
                return null;
            }

            var transparentIdentifierType = TransparentIdentifierFactory.Create(
                resultSelector.Parameters[0].Type,
                resultSelector.Parameters[1].Type);

            ((InMemoryQueryExpression)outer.QueryExpression).AddLeftJoin(
                (InMemoryQueryExpression)inner.QueryExpression,
                outerKeySelector,
                innerKeySelector,
                transparentIdentifierType);

            return TranslateResultSelectorForJoin(
                outer,
                resultSelector,
                MarkShaperNullable(inner.ShaperExpression),
                transparentIdentifierType);
        }

        protected override ShapedQueryExpression TranslateLongCount(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;

            if (predicate == null)
            {
                inMemoryQueryExpression.ServerQueryExpression =
                    Expression.Call(
                        InMemoryLinqOperatorProvider.LongCount.MakeGenericMethod(typeof(ValueBuffer)),
                        inMemoryQueryExpression.ServerQueryExpression);
            }
            else
            {
                predicate = TranslateLambdaExpression(source, predicate);
                if (predicate == null)
                {
                    return null;
                }

                inMemoryQueryExpression.ServerQueryExpression =
                    Expression.Call(
                        InMemoryLinqOperatorProvider.LongCountPredicate.MakeGenericMethod(typeof(ValueBuffer)),
                        inMemoryQueryExpression.ServerQueryExpression,
                        predicate);
            }

            source.ShaperExpression = inMemoryQueryExpression.GetSingleScalarProjection();

            return source;
        }

        protected override ShapedQueryExpression TranslateMax(ShapedQueryExpression source, LambdaExpression selector, Type resultType)
            => TranslateScalarAggregate(source, selector, nameof(Enumerable.Max));

        protected override ShapedQueryExpression TranslateMin(ShapedQueryExpression source, LambdaExpression selector, Type resultType)
            => TranslateScalarAggregate(source, selector, nameof(Enumerable.Min));

        protected override ShapedQueryExpression TranslateOfType(ShapedQueryExpression source, Type resultType)
        {
            if (source.ShaperExpression is EntityShaperExpression entityShaperExpression)
            {
                var entityType = entityShaperExpression.EntityType;
                if (entityType.ClrType == resultType)
                {
                    return source;
                }

                var baseType = entityType.GetAllBaseTypes().SingleOrDefault(et => et.ClrType == resultType);
                if (baseType != null)
                {
                    source.ShaperExpression = entityShaperExpression.WithEntityType(baseType);

                    return source;
                }

                var derivedType = entityType.GetDerivedTypes().SingleOrDefault(et => et.ClrType == resultType);
                if (derivedType != null)
                {
                    var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;
                    var discriminatorProperty = entityType.GetDiscriminatorProperty();
                    var parameter = Expression.Parameter(entityType.ClrType);

                    var callEFProperty = Expression.Call(
                        _efPropertyMethod.MakeGenericMethod(
                            discriminatorProperty.ClrType),
                        parameter,
                        Expression.Constant(discriminatorProperty.Name));

                    var equals = Expression.Equal(
                        callEFProperty,
                        Expression.Constant(derivedType.GetDiscriminatorValue(), discriminatorProperty.ClrType));

                    foreach (var derivedDerivedType in derivedType.GetDerivedTypes())
                    {
                        equals = Expression.OrElse(
                            equals,
                            Expression.Equal(
                                callEFProperty,
                                Expression.Constant(derivedDerivedType.GetDiscriminatorValue(), discriminatorProperty.ClrType)));
                    }

                    var predicate = TranslateLambdaExpression(source, Expression.Lambda(equals, parameter));
                    if (predicate == null)
                    {
                        return null;
                    }

                    inMemoryQueryExpression.ServerQueryExpression = Expression.Call(
                        InMemoryLinqOperatorProvider.Where.MakeGenericMethod(typeof(ValueBuffer)),
                        inMemoryQueryExpression.ServerQueryExpression,
                        predicate);

                    var projectionBindingExpression = (ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression;
                    var projectionMember = projectionBindingExpression.ProjectionMember;
                    var entityProjection = (EntityProjectionExpression)inMemoryQueryExpression.GetMappedProjection(projectionMember);

                    inMemoryQueryExpression.ReplaceProjectionMapping(
                        new Dictionary<ProjectionMember, Expression>
                        {
                            { projectionMember, entityProjection.UpdateEntityType(derivedType)}
                        });

                    source.ShaperExpression = entityShaperExpression.WithEntityType(derivedType);

                    return source;
                }
            }

            return null;
        }

        protected override ShapedQueryExpression TranslateOrderBy(ShapedQueryExpression source, LambdaExpression keySelector, bool ascending)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;

            keySelector = TranslateLambdaExpression(source, keySelector);
            if (keySelector == null)
            {
                return null;
            }

            inMemoryQueryExpression.ServerQueryExpression
                = Expression.Call(
                    (ascending ? InMemoryLinqOperatorProvider.OrderBy : InMemoryLinqOperatorProvider.OrderByDescending)
                        .MakeGenericMethod(typeof(ValueBuffer), keySelector.ReturnType),
                    inMemoryQueryExpression.ServerQueryExpression,
                    keySelector);

            return source;
        }

        protected override ShapedQueryExpression TranslateReverse(ShapedQueryExpression source)
            => null;

        protected override ShapedQueryExpression TranslateSelect(ShapedQueryExpression source, LambdaExpression selector)
        {
            if (selector.Body == selector.Parameters[0])
            {
                return source;
            }

            var newSelectorBody = ReplacingExpressionVisitor.Replace(
                selector.Parameters.Single(), source.ShaperExpression, selector.Body);

            source.ShaperExpression = _projectionBindingExpressionVisitor
                .Translate((InMemoryQueryExpression)source.QueryExpression, newSelectorBody);

            return source;
        }

        protected override ShapedQueryExpression TranslateSelectMany(
            ShapedQueryExpression source, LambdaExpression collectionSelector, LambdaExpression resultSelector)
        {
            var defaultIfEmpty = new DefaultIfEmptyFindingExpressionVisitor().IsOptional(collectionSelector);
            var collectionSelectorBody = RemapLambdaBody(source, collectionSelector);

            if (Visit(collectionSelectorBody) is ShapedQueryExpression inner)
            {
                var transparentIdentifierType = TransparentIdentifierFactory.Create(
                    resultSelector.Parameters[0].Type,
                    resultSelector.Parameters[1].Type);

                var innerShaperExpression = defaultIfEmpty
                    ? MarkShaperNullable(inner.ShaperExpression)
                    : inner.ShaperExpression;

                ((InMemoryQueryExpression)source.QueryExpression).AddSelectMany(
                    (InMemoryQueryExpression)inner.QueryExpression, transparentIdentifierType, defaultIfEmpty);

                return TranslateResultSelectorForJoin(
                    source,
                    resultSelector,
                    innerShaperExpression,
                    transparentIdentifierType);
            }

            return null;
        }

        private class DefaultIfEmptyFindingExpressionVisitor : ExpressionVisitor
        {
            private bool _defaultIfEmpty;

            public bool IsOptional(LambdaExpression lambdaExpression)
            {
                _defaultIfEmpty = false;

                Visit(lambdaExpression.Body);

                return _defaultIfEmpty;
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.DefaultIfEmptyWithoutArgument)
                {
                    _defaultIfEmpty = true;
                }

                return base.VisitMethodCall(methodCallExpression);
            }
        }

        protected override ShapedQueryExpression TranslateSelectMany(ShapedQueryExpression source, LambdaExpression selector)
        {
            var innerParameter = Expression.Parameter(selector.ReturnType.TryGetSequenceType(), "i");
            var resultSelector = Expression.Lambda(
                innerParameter,
                new[]
                {
                    Expression.Parameter(source.Type.TryGetSequenceType()),
                    innerParameter
                });

            return TranslateSelectMany(source, selector, resultSelector);
        }

        protected override ShapedQueryExpression TranslateSingleOrDefault(ShapedQueryExpression source, LambdaExpression predicate, Type returnType, bool returnDefault)
        {
            return TranslateSingleResultOperator(
                source,
                predicate,
                returnType,
                returnDefault
                    ? InMemoryLinqOperatorProvider.SingleOrDefault
                    : InMemoryLinqOperatorProvider.Single);
        }

        protected override ShapedQueryExpression TranslateSkip(ShapedQueryExpression source, Expression count)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;
            count = TranslateExpression(count);
            if (count == null)
            {
                return null;
            }

            inMemoryQueryExpression.ServerQueryExpression
                = Expression.Call(
                    InMemoryLinqOperatorProvider.Skip.MakeGenericMethod(typeof(ValueBuffer)),
                    inMemoryQueryExpression.ServerQueryExpression,
                    count);

            return source;
        }

        protected override ShapedQueryExpression TranslateSkipWhile(ShapedQueryExpression source, LambdaExpression predicate)
            => null;

        protected override ShapedQueryExpression TranslateSum(ShapedQueryExpression source, LambdaExpression selector, Type resultType)
            => TranslateScalarAggregate(source, selector, nameof(Enumerable.Sum));

        protected override ShapedQueryExpression TranslateTake(ShapedQueryExpression source, Expression count)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;
            count = TranslateExpression(count);
            if (count == null)
            {
                return null;
            }

            inMemoryQueryExpression.ServerQueryExpression
                = Expression.Call(
                    InMemoryLinqOperatorProvider.Take.MakeGenericMethod(typeof(ValueBuffer)),
                    inMemoryQueryExpression.ServerQueryExpression,
                    count);

            return source;
        }

        protected override ShapedQueryExpression TranslateTakeWhile(ShapedQueryExpression source, LambdaExpression predicate)
            => null;

        protected override ShapedQueryExpression TranslateThenBy(ShapedQueryExpression source, LambdaExpression keySelector, bool ascending)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;
            keySelector = TranslateLambdaExpression(source, keySelector);
            if (keySelector == null)
            {
                return null;
            }

            inMemoryQueryExpression.ServerQueryExpression
                = Expression.Call(
                    (ascending ? InMemoryLinqOperatorProvider.ThenBy : InMemoryLinqOperatorProvider.ThenByDescending)
                        .MakeGenericMethod(typeof(ValueBuffer), keySelector.ReturnType),
                    inMemoryQueryExpression.ServerQueryExpression,
                    keySelector);

            return source;
        }

        protected override ShapedQueryExpression TranslateUnion(ShapedQueryExpression source1, ShapedQueryExpression source2)
            => TranslateSetOperation(InMemoryLinqOperatorProvider.Union, source1, source2);

        protected override ShapedQueryExpression TranslateWhere(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;
            predicate = TranslateLambdaExpression(source, predicate);
            if (predicate == null)
            {
                return null;
            }

            inMemoryQueryExpression.ServerQueryExpression = Expression.Call(
                InMemoryLinqOperatorProvider.Where.MakeGenericMethod(typeof(ValueBuffer)),
                inMemoryQueryExpression.ServerQueryExpression,
                predicate);

            return source;
        }

        private Expression TranslateExpression(Expression expression)
        {
            return _expressionTranslator.Translate(expression);
        }

        private LambdaExpression TranslateLambdaExpression(
            ShapedQueryExpression shapedQueryExpression, LambdaExpression lambdaExpression)
        {
            var lambdaBody = TranslateExpression(RemapLambdaBody(shapedQueryExpression, lambdaExpression));

            return lambdaBody != null
                ? Expression.Lambda(lambdaBody,
                    ((InMemoryQueryExpression)shapedQueryExpression.QueryExpression).ValueBufferParameter)
                : null;
        }

        private static Expression RemapLambdaBody(ShapedQueryExpression shapedQueryExpression, LambdaExpression lambdaExpression)
        {
            return ReplacingExpressionVisitor.Replace(
                lambdaExpression.Parameters.Single(), shapedQueryExpression.ShaperExpression, lambdaExpression.Body);
        }

        private ShapedQueryExpression TranslateScalarAggregate(
            ShapedQueryExpression source, LambdaExpression selector, string methodName)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;

            selector = selector == null
                || selector.Body == selector.Parameters[0]
                ? Expression.Lambda(
                    inMemoryQueryExpression.GetMappedProjection(new ProjectionMember()),
                    inMemoryQueryExpression.ValueBufferParameter)
                : TranslateLambdaExpression(source, selector);

            if (selector == null)
            {
                return null;
            }

            inMemoryQueryExpression.ServerQueryExpression
                = Expression.Call(
                    InMemoryLinqOperatorProvider
                        .GetAggregateMethod(methodName, selector.ReturnType, parameterCount: 1)
                        .MakeGenericMethod(typeof(ValueBuffer)),
                    inMemoryQueryExpression.ServerQueryExpression,
                    selector);

            source.ShaperExpression = inMemoryQueryExpression.GetSingleScalarProjection();

            return source;
        }

        private ShapedQueryExpression TranslateSingleResultOperator(
            ShapedQueryExpression source, LambdaExpression predicate, Type returnType, MethodInfo method)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;

            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
                if (source == null)
                {
                    return null;
                }
            }

            inMemoryQueryExpression.ServerQueryExpression =
                Expression.Call(
                    method.MakeGenericMethod(typeof(ValueBuffer)),
                    inMemoryQueryExpression.ServerQueryExpression);

            inMemoryQueryExpression.ConvertToEnumerable();

            if (source.ShaperExpression.Type != returnType)
            {
                source.ShaperExpression = Expression.Convert(source.ShaperExpression, returnType);
            }

            return source;
        }

        private ShapedQueryExpression TranslateSetOperation(
            MethodInfo setOperationMethodInfo,
            ShapedQueryExpression source1,
            ShapedQueryExpression source2)
        {
            var inMemoryQueryExpression1 = (InMemoryQueryExpression)source1.QueryExpression;
            var inMemoryQueryExpression2 = (InMemoryQueryExpression)source2.QueryExpression;

            // Apply any pending selectors, ensuring that the shape of both expressions is identical
            // prior to applying the set operation.
            inMemoryQueryExpression1.PushdownIntoSubquery();
            inMemoryQueryExpression2.PushdownIntoSubquery();

            inMemoryQueryExpression1.ServerQueryExpression = Expression.Call(
                setOperationMethodInfo.MakeGenericMethod(typeof(ValueBuffer)),
                inMemoryQueryExpression1.ServerQueryExpression,
                inMemoryQueryExpression2.ServerQueryExpression);

            return source1;
        }
    }
}
