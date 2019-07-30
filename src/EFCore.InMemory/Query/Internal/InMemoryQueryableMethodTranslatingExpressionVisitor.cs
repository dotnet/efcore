// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    public class InMemoryQueryableMethodTranslatingExpressionVisitor : QueryableMethodTranslatingExpressionVisitor
    {
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

        public InMemoryQueryableMethodTranslatingExpressionVisitor(
            QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
            IModel model,
            InMemoryExpressionTranslatingExpressionVisitor expressionTranslator)
            : base(dependencies, subquery: true)
        {
            _expressionTranslator = expressionTranslator;
            _projectionBindingExpressionVisitor = new InMemoryProjectionBindingExpressionVisitor(this, expressionTranslator);
            _model = model;
        }

        private static Type CreateTransparentIdentifierType(Type outerType, Type innerType)
            => typeof(TransparentIdentifier<,>).MakeGenericType(outerType, innerType);

        public override ShapedQueryExpression TranslateSubquery(Expression expression)
        {
            return (ShapedQueryExpression)new InMemoryQueryableMethodTranslatingExpressionVisitor(
                    Dependencies,
                    _model,
                    _expressionTranslator)
                .Visit(expression);
        }

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

            inMemoryQueryExpression.ServerQueryExpression =
                Expression.Call(
                    InMemoryLinqOperatorProvider.All.MakeGenericMethod(typeof(ValueBuffer)),
                    inMemoryQueryExpression.ServerQueryExpression,
                    TranslateLambdaExpression(source, predicate));

            source.ShaperExpression = inMemoryQueryExpression.GetSingleScalarProjection();

            return source;
        }

        protected override ShapedQueryExpression TranslateAny(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;

            inMemoryQueryExpression.ServerQueryExpression = predicate == null
                ? Expression.Call(
                    InMemoryLinqOperatorProvider.Any.MakeGenericMethod(typeof(ValueBuffer)),
                    inMemoryQueryExpression.ServerQueryExpression)
                : Expression.Call(
                    InMemoryLinqOperatorProvider.AnyPredicate.MakeGenericMethod(typeof(ValueBuffer)),
                    inMemoryQueryExpression.ServerQueryExpression,
                    TranslateLambdaExpression(source, predicate));

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

        protected override ShapedQueryExpression TranslateConcat(ShapedQueryExpression source1, ShapedQueryExpression source2) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateContains(ShapedQueryExpression source, Expression item)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;

            item = TranslateExpression(item);

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
                inMemoryQueryExpression.ServerQueryExpression =
                    Expression.Call(
                        InMemoryLinqOperatorProvider.CountPredicate.MakeGenericMethod(typeof(ValueBuffer)),
                        inMemoryQueryExpression.ServerQueryExpression,
                        TranslateLambdaExpression(source, predicate));
            }

            source.ShaperExpression = inMemoryQueryExpression.GetSingleScalarProjection();

            return source;
        }

        protected override ShapedQueryExpression TranslateDefaultIfEmpty(ShapedQueryExpression source, Expression defaultValue) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateDistinct(ShapedQueryExpression source)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;

            inMemoryQueryExpression.ApplyPendingSelector();
            inMemoryQueryExpression.ServerQueryExpression
                = Expression.Call(
                    InMemoryLinqOperatorProvider.Distinct.MakeGenericMethod(typeof(ValueBuffer)),
                    inMemoryQueryExpression.ServerQueryExpression);

            return source;
        }

        protected override ShapedQueryExpression TranslateElementAtOrDefault(ShapedQueryExpression source, Expression index, bool returnDefault) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateExcept(ShapedQueryExpression source1, ShapedQueryExpression source2) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateFirstOrDefault(ShapedQueryExpression source, LambdaExpression predicate, Type returnType, bool returnDefault)
        {
            return TranslateSingleResultOperator(
                source,
                predicate,
                returnType,
                returnDefault
                    ? InMemoryLinqOperatorProvider.FirstOrDefaultPredicate
                    : InMemoryLinqOperatorProvider.FirstPredicate);
        }

        protected override ShapedQueryExpression TranslateGroupBy(ShapedQueryExpression source, LambdaExpression keySelector, LambdaExpression elementSelector, LambdaExpression resultSelector) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateGroupJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateIntersect(ShapedQueryExpression source1, ShapedQueryExpression source2) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector)
        {
            outerKeySelector = TranslateLambdaExpression(outer, outerKeySelector);
            innerKeySelector = TranslateLambdaExpression(inner, innerKeySelector);

            var transparentIdentifierType = CreateTransparentIdentifierType(
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
                transparentIdentifierType,
                false);
        }

        protected override ShapedQueryExpression TranslateLastOrDefault(ShapedQueryExpression source, LambdaExpression predicate, Type returnType, bool returnDefault)
        {
            return TranslateSingleResultOperator(
                source,
                predicate,
                returnType,
                returnDefault
                    ? InMemoryLinqOperatorProvider.LastOrDefaultPredicate
                    : InMemoryLinqOperatorProvider.LastPredicate);
        }

        protected override ShapedQueryExpression TranslateLeftJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector)
        {
            outerKeySelector = TranslateLambdaExpression(outer, outerKeySelector);
            innerKeySelector = TranslateLambdaExpression(inner, innerKeySelector);

            var transparentIdentifierType = CreateTransparentIdentifierType(
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
                inner.ShaperExpression,
                transparentIdentifierType,
                true);
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
                inMemoryQueryExpression.ServerQueryExpression =
                    Expression.Call(
                        InMemoryLinqOperatorProvider.LongCountPredicate.MakeGenericMethod(typeof(ValueBuffer)),
                        inMemoryQueryExpression.ServerQueryExpression,
                        TranslateLambdaExpression(source, predicate));
            }

            source.ShaperExpression = inMemoryQueryExpression.GetSingleScalarProjection();

            return source;
        }

        protected override ShapedQueryExpression TranslateMax(ShapedQueryExpression source, LambdaExpression selector, Type resultType)
            => TranslateScalarAggregate(source, selector, nameof(Enumerable.Max));

        protected override ShapedQueryExpression TranslateMin(ShapedQueryExpression source, LambdaExpression selector, Type resultType)
            => TranslateScalarAggregate(source, selector, nameof(Enumerable.Min));

        protected override ShapedQueryExpression TranslateOfType(ShapedQueryExpression source, Type resultType) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateOrderBy(ShapedQueryExpression source, LambdaExpression keySelector, bool ascending)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;

            keySelector = TranslateLambdaExpression(source, keySelector);

            inMemoryQueryExpression.ServerQueryExpression
                = Expression.Call(
                    (ascending ? InMemoryLinqOperatorProvider.OrderBy : InMemoryLinqOperatorProvider.OrderByDescending)
                        .MakeGenericMethod(typeof(ValueBuffer), keySelector.ReturnType),
                    inMemoryQueryExpression.ServerQueryExpression,
                    keySelector);

            return source;
        }

        protected override ShapedQueryExpression TranslateReverse(ShapedQueryExpression source) => throw new NotImplementedException();

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

        private static readonly MethodInfo _defaultIfEmptyWithoutArgMethodInfo = typeof(Enumerable).GetTypeInfo()
            .GetDeclaredMethods(nameof(Enumerable.DefaultIfEmpty)).Single(mi => mi.GetParameters().Length == 1);

        protected override ShapedQueryExpression TranslateSelectMany(
            ShapedQueryExpression source, LambdaExpression collectionSelector, LambdaExpression resultSelector)
        {
            var collectionSelectorBody = collectionSelector.Body;
            //var defaultIfEmpty = false;

            if (collectionSelectorBody is MethodCallExpression collectionEndingMethod
                && collectionEndingMethod.Method.IsGenericMethod
                && collectionEndingMethod.Method.GetGenericMethodDefinition() == _defaultIfEmptyWithoutArgMethodInfo)
            {
                //defaultIfEmpty = true;
                collectionSelectorBody = collectionEndingMethod.Arguments[0];
            }

            var correlated = new CorrelationFindingExpressionVisitor().IsCorrelated(collectionSelectorBody, collectionSelector.Parameters[0]);
            if (correlated)
            {
                // TODO visit inner with outer parameter;
                throw new NotImplementedException();
            }
            else
            {
                if (Visit(collectionSelectorBody) is ShapedQueryExpression inner)
                {
                    var transparentIdentifierType = CreateTransparentIdentifierType(
                        resultSelector.Parameters[0].Type,
                        resultSelector.Parameters[1].Type);

                    ((InMemoryQueryExpression)source.QueryExpression).AddCrossJoin(
                        (InMemoryQueryExpression)inner.QueryExpression, transparentIdentifierType);

                    return TranslateResultSelectorForJoin(
                        source,
                        resultSelector,
                        inner.ShaperExpression,
                        transparentIdentifierType,
                        false);
                }
            }

            throw new NotImplementedException();
        }

        private class CorrelationFindingExpressionVisitor : ExpressionVisitor
        {
            private ParameterExpression _outerParameter;
            private bool _isCorrelated;
            public bool IsCorrelated(Expression tree, ParameterExpression outerParameter)
            {
                _isCorrelated = false;
                _outerParameter = outerParameter;

                Visit(tree);

                return _isCorrelated;
            }

            protected override Expression VisitParameter(ParameterExpression parameterExpression)
            {
                if (parameterExpression == _outerParameter)
                {
                    _isCorrelated = true;
                }

                return base.VisitParameter(parameterExpression);
            }
        }

        protected override ShapedQueryExpression TranslateSelectMany(ShapedQueryExpression source, LambdaExpression selector) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateSingleOrDefault(ShapedQueryExpression source, LambdaExpression predicate, Type returnType, bool returnDefault)
        {
            return TranslateSingleResultOperator(
                source,
                predicate,
                returnType,
                returnDefault
                    ? InMemoryLinqOperatorProvider.SingleOrDefaultPredicate
                    : InMemoryLinqOperatorProvider.SinglePredicate);
        }

        protected override ShapedQueryExpression TranslateSkip(ShapedQueryExpression source, Expression count)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;

            inMemoryQueryExpression.ServerQueryExpression
                = Expression.Call(
                    InMemoryLinqOperatorProvider.Skip.MakeGenericMethod(typeof(ValueBuffer)),
                    inMemoryQueryExpression.ServerQueryExpression,
                    TranslateExpression(count));

            return source;
        }

        protected override ShapedQueryExpression TranslateSkipWhile(ShapedQueryExpression source, LambdaExpression predicate) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateSum(ShapedQueryExpression source, LambdaExpression selector, Type resultType)
            => TranslateScalarAggregate(source, selector, nameof(Enumerable.Sum));

        protected override ShapedQueryExpression TranslateTake(ShapedQueryExpression source, Expression count)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;

            inMemoryQueryExpression.ServerQueryExpression
                = Expression.Call(
                    InMemoryLinqOperatorProvider.Take.MakeGenericMethod(typeof(ValueBuffer)),
                    inMemoryQueryExpression.ServerQueryExpression,
                    TranslateExpression(count));

            return source;
        }

        protected override ShapedQueryExpression TranslateTakeWhile(ShapedQueryExpression source, LambdaExpression predicate) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateThenBy(ShapedQueryExpression source, LambdaExpression keySelector, bool ascending)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;

            keySelector = TranslateLambdaExpression(source, keySelector);

            inMemoryQueryExpression.ServerQueryExpression
                = Expression.Call(
                    (ascending ? InMemoryLinqOperatorProvider.ThenBy : InMemoryLinqOperatorProvider.ThenByDescending)
                        .MakeGenericMethod(typeof(ValueBuffer), keySelector.ReturnType),
                    inMemoryQueryExpression.ServerQueryExpression,
                    keySelector);

            return source;
        }

        protected override ShapedQueryExpression TranslateUnion(ShapedQueryExpression source1, ShapedQueryExpression source2) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateWhere(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;
            inMemoryQueryExpression.ServerQueryExpression = Expression.Call(
                InMemoryLinqOperatorProvider.Where.MakeGenericMethod(typeof(ValueBuffer)),
                inMemoryQueryExpression.ServerQueryExpression,
                TranslateLambdaExpression(source, predicate));

            return source;
        }

        private Expression TranslateExpression(Expression expression)
        {
            return _expressionTranslator.Translate(expression);
        }

        private LambdaExpression TranslateLambdaExpression(
            ShapedQueryExpression shapedQueryExpression, LambdaExpression lambdaExpression)
        {
            var lambdaBody = ReplacingExpressionVisitor.Replace(
                lambdaExpression.Parameters.Single(), shapedQueryExpression.ShaperExpression, lambdaExpression.Body);

            return Expression.Lambda(TranslateExpression(lambdaBody),
                ((InMemoryQueryExpression)shapedQueryExpression.QueryExpression).ValueBufferParameter);
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

            predicate = predicate == null
                ? Expression.Lambda(Expression.Constant(true), Expression.Parameter(typeof(ValueBuffer)))
                : TranslateLambdaExpression(source, predicate);

            inMemoryQueryExpression.ServerQueryExpression =
                Expression.Call(
                    method.MakeGenericMethod(typeof(ValueBuffer)),
                    inMemoryQueryExpression.ServerQueryExpression,
                    predicate);

            inMemoryQueryExpression.ConvertToEnumerable();

            if (source.ShaperExpression.Type != returnType)
            {
                source.ShaperExpression = Expression.Convert(source.ShaperExpression, returnType);
            }

            return source;
        }
    }
}
