// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
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
        private readonly WeakEntityExpandingExpressionVisitor _weakEntityExpandingExpressionVisitor;
        private readonly InMemoryProjectionBindingExpressionVisitor _projectionBindingExpressionVisitor;
        private readonly IModel _model;

        public InMemoryQueryableMethodTranslatingExpressionVisitor(
            QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
            IModel model)
            : base(dependencies, subquery: false)
        {
            _expressionTranslator = new InMemoryExpressionTranslatingExpressionVisitor(this);
            _weakEntityExpandingExpressionVisitor = new WeakEntityExpandingExpressionVisitor(_expressionTranslator);
            _projectionBindingExpressionVisitor = new InMemoryProjectionBindingExpressionVisitor(this, _expressionTranslator);
            _model = model;
        }

        protected InMemoryQueryableMethodTranslatingExpressionVisitor(
            InMemoryQueryableMethodTranslatingExpressionVisitor parentVisitor)
            : base(parentVisitor.Dependencies, subquery: true)
        {
            _expressionTranslator = parentVisitor._expressionTranslator;
            _weakEntityExpandingExpressionVisitor = parentVisitor._weakEntityExpandingExpressionVisitor;
            _projectionBindingExpressionVisitor = new InMemoryProjectionBindingExpressionVisitor(this, _expressionTranslator);
            _model = parentVisitor._model;
        }

        protected override QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
            => new InMemoryQueryableMethodTranslatingExpressionVisitor(this);

        protected override ShapedQueryExpression CreateShapedQueryExpression(Type elementType)
            => CreateShapedQueryExpression(_model.FindEntityType(elementType));

        private static ShapedQueryExpression CreateShapedQueryExpression(IEntityType entityType)
        {
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
            predicate = TranslateLambdaExpression(source, predicate, preserveType: true);
            if (predicate == null)
            {
                return null;
            }

            inMemoryQueryExpression.ServerQueryExpression =
                Expression.Call(
                    EnumerableMethods.All.MakeGenericMethod(inMemoryQueryExpression.CurrentParameter.Type),
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
                    EnumerableMethods.AnyWithoutPredicate.MakeGenericMethod(inMemoryQueryExpression.CurrentParameter.Type),
                    inMemoryQueryExpression.ServerQueryExpression);
            }
            else
            {
                predicate = TranslateLambdaExpression(source, predicate, preserveType: true);
                if (predicate == null)
                {
                    return null;
                }

                inMemoryQueryExpression.ServerQueryExpression = Expression.Call(
                    EnumerableMethods.AnyWithPredicate.MakeGenericMethod(inMemoryQueryExpression.CurrentParameter.Type),
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
            => TranslateSetOperation(EnumerableMethods.Concat, source1, source2);

        protected override ShapedQueryExpression TranslateContains(ShapedQueryExpression source, Expression item)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;
            item = TranslateExpression(item, preserveType: true);
            if (item == null)
            {
                return null;
            }

            inMemoryQueryExpression.ServerQueryExpression =
                Expression.Call(
                    EnumerableMethods.Contains.MakeGenericMethod(item.Type),
                    Expression.Call(
                        EnumerableMethods.Select.MakeGenericMethod(inMemoryQueryExpression.CurrentParameter.Type, item.Type),
                        inMemoryQueryExpression.ServerQueryExpression,
                        Expression.Lambda(
                            inMemoryQueryExpression.GetMappedProjection(new ProjectionMember()), inMemoryQueryExpression.CurrentParameter)),
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
                        EnumerableMethods.CountWithoutPredicate.MakeGenericMethod(inMemoryQueryExpression.CurrentParameter.Type),
                        inMemoryQueryExpression.ServerQueryExpression);
            }
            else
            {
                predicate = TranslateLambdaExpression(source, predicate, preserveType: true);
                if (predicate == null)
                {
                    return null;
                }

                inMemoryQueryExpression.ServerQueryExpression =
                    Expression.Call(
                        EnumerableMethods.CountWithPredicate.MakeGenericMethod(inMemoryQueryExpression.CurrentParameter.Type),
                        inMemoryQueryExpression.ServerQueryExpression,
                        predicate);
            }

            source.ShaperExpression = inMemoryQueryExpression.GetSingleScalarProjection();

            return source;
        }

        protected override ShapedQueryExpression TranslateDefaultIfEmpty(ShapedQueryExpression source, Expression defaultValue)
        {
            if (defaultValue == null)
            {
                ((InMemoryQueryExpression)source.QueryExpression).ApplyDefaultIfEmpty();
                source.ShaperExpression = MarkShaperNullable(source.ShaperExpression);

                return source;
            }

            return null;
        }

        protected override ShapedQueryExpression TranslateDistinct(ShapedQueryExpression source)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;

            inMemoryQueryExpression.PushdownIntoSubquery();
            inMemoryQueryExpression.ServerQueryExpression
                = Expression.Call(
                    EnumerableMethods.Distinct.MakeGenericMethod(inMemoryQueryExpression.CurrentParameter.Type),
                    inMemoryQueryExpression.ServerQueryExpression);

            return source;
        }

        protected override ShapedQueryExpression TranslateElementAtOrDefault(
            ShapedQueryExpression source, Expression index, bool returnDefault)
            => null;

        protected override ShapedQueryExpression TranslateExcept(ShapedQueryExpression source1, ShapedQueryExpression source2)
            => TranslateSetOperation(EnumerableMethods.Except, source1, source2);

        protected override ShapedQueryExpression TranslateFirstOrDefault(
            ShapedQueryExpression source, LambdaExpression predicate, Type returnType, bool returnDefault)
        {
            return TranslateSingleResultOperator(
                source,
                predicate,
                returnType,
                returnDefault
                    ? EnumerableMethods.FirstOrDefaultWithoutPredicate
                    : EnumerableMethods.FirstWithoutPredicate);
        }

        protected override ShapedQueryExpression TranslateGroupBy(
            ShapedQueryExpression source, LambdaExpression keySelector, LambdaExpression elementSelector, LambdaExpression resultSelector)
        {
            var remappedKeySelector = RemapLambdaBody(source, keySelector);

            var translatedKey = TranslateGroupingKey(remappedKeySelector);
            if (translatedKey != null)
            {
                if (elementSelector != null)
                {
                    source = TranslateSelect(source, elementSelector);
                }

                var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;
                source.ShaperExpression = inMemoryQueryExpression.ApplyGrouping(translatedKey, source.ShaperExpression);

                if (resultSelector == null)
                {
                    return source;
                }

                var original1 = resultSelector.Parameters[0];
                var original2 = resultSelector.Parameters[1];

                var newResultSelectorBody = new ReplacingExpressionVisitor(
                    new Dictionary<Expression, Expression>
                    {
                        { original1, ((GroupByShaperExpression)source.ShaperExpression).KeySelector },
                        { original2, source.ShaperExpression }
                    }).Visit(resultSelector.Body);

                newResultSelectorBody = ExpandWeakEntities(inMemoryQueryExpression, newResultSelectorBody);

                source.ShaperExpression = _projectionBindingExpressionVisitor.Translate(inMemoryQueryExpression, newResultSelectorBody);

                inMemoryQueryExpression.PushdownIntoSubquery();

                return source;
            }

            return null;
        }

        private Expression TranslateGroupingKey(Expression expression)
        {
            switch (expression)
            {
                case NewExpression newExpression:
                    // For .NET Framework only. If ctor is null that means the type is struct and has no ctor args.
                    if (newExpression.Constructor == null)
                    {
                        return newExpression;
                    }

                    if (newExpression.Arguments.Count == 0)
                    {
                        return newExpression;
                    }

                    var newArguments = new Expression[newExpression.Arguments.Count];
                    for (var i = 0; i < newArguments.Length; i++)
                    {
                        newArguments[i] = TranslateGroupingKey(newExpression.Arguments[i]);
                        if (newArguments[i] == null)
                        {
                            return null;
                        }
                    }

                    return newExpression.Update(newArguments);

                case MemberInitExpression memberInitExpression:
                    var updatedNewExpression = (NewExpression)TranslateGroupingKey(memberInitExpression.NewExpression);
                    if (updatedNewExpression == null)
                    {
                        return null;
                    }

                    var newBindings = new MemberAssignment[memberInitExpression.Bindings.Count];
                    for (var i = 0; i < newBindings.Length; i++)
                    {
                        var memberAssignment = (MemberAssignment)memberInitExpression.Bindings[i];
                        var visitedExpression = TranslateGroupingKey(memberAssignment.Expression);
                        if (visitedExpression == null)
                        {
                            return null;
                        }

                        newBindings[i] = memberAssignment.Update(visitedExpression);
                    }

                    return memberInitExpression.Update(updatedNewExpression, newBindings);

                default:
                    var translation = _expressionTranslator.Translate(expression);
                    if (translation == null)
                    {
                        return null;
                    }

                    return translation.Type == expression.Type
                        ? translation
                        : Expression.Convert(translation, expression.Type);
            }
        }

        protected override ShapedQueryExpression TranslateGroupJoin(
            ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector,
            LambdaExpression resultSelector)
            => null;

        protected override ShapedQueryExpression TranslateIntersect(ShapedQueryExpression source1, ShapedQueryExpression source2)
            => TranslateSetOperation(EnumerableMethods.Intersect, source1, source2);

        protected override ShapedQueryExpression TranslateJoin(
            ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector,
            LambdaExpression resultSelector)
        {
            outerKeySelector = TranslateLambdaExpression(outer, outerKeySelector);
            innerKeySelector = TranslateLambdaExpression(inner, innerKeySelector);
            if (outerKeySelector == null
                || innerKeySelector == null)
            {
                return null;
            }

            (outerKeySelector, innerKeySelector) = AlignKeySelectorTypes(outerKeySelector, innerKeySelector);

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

        private static (LambdaExpression OuterKeySelector, LambdaExpression InnerKeySelector)
            AlignKeySelectorTypes(LambdaExpression outerKeySelector, LambdaExpression innerKeySelector)
        {
            static bool isConvertedToNullable(Expression outer, Expression inner)
                => outer.Type.IsNullableType()
                    && !inner.Type.IsNullableType()
                    && outer.Type.UnwrapNullableType() == inner.Type;

            if (outerKeySelector.Body.Type != innerKeySelector.Body.Type)
            {
                if (isConvertedToNullable(outerKeySelector.Body, innerKeySelector.Body))
                {
                    innerKeySelector = Expression.Lambda(
                        Expression.Convert(innerKeySelector.Body, outerKeySelector.Body.Type), innerKeySelector.Parameters);
                }
                else if (isConvertedToNullable(innerKeySelector.Body, outerKeySelector.Body))
                {
                    outerKeySelector = Expression.Lambda(
                        Expression.Convert(outerKeySelector.Body, innerKeySelector.Body.Type), outerKeySelector.Parameters);
                }
            }

            return (outerKeySelector, innerKeySelector);
        }

        protected override ShapedQueryExpression TranslateLastOrDefault(
            ShapedQueryExpression source, LambdaExpression predicate, Type returnType, bool returnDefault)
        {
            return TranslateSingleResultOperator(
                source,
                predicate,
                returnType,
                returnDefault
                    ? EnumerableMethods.LastOrDefaultWithoutPredicate
                    : EnumerableMethods.LastWithoutPredicate);
        }

        protected override ShapedQueryExpression TranslateLeftJoin(
            ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector,
            LambdaExpression resultSelector)
        {
            outerKeySelector = TranslateLambdaExpression(outer, outerKeySelector);
            innerKeySelector = TranslateLambdaExpression(inner, innerKeySelector);
            if (outerKeySelector == null
                || innerKeySelector == null)
            {
                return null;
            }

            (outerKeySelector, innerKeySelector) = AlignKeySelectorTypes(outerKeySelector, innerKeySelector);

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
                        EnumerableMethods.LongCountWithoutPredicate.MakeGenericMethod(
                            inMemoryQueryExpression.CurrentParameter.Type),
                        inMemoryQueryExpression.ServerQueryExpression);
            }
            else
            {
                predicate = TranslateLambdaExpression(source, predicate, preserveType: true);
                if (predicate == null)
                {
                    return null;
                }

                inMemoryQueryExpression.ServerQueryExpression =
                    Expression.Call(
                        EnumerableMethods.LongCountWithPredicate.MakeGenericMethod(
                            inMemoryQueryExpression.CurrentParameter.Type),
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

                    var discriminatorPredicate = TranslateLambdaExpression(source, Expression.Lambda(equals, parameter));
                    if (discriminatorPredicate == null)
                    {
                        return null;
                    }

                    inMemoryQueryExpression.ServerQueryExpression = Expression.Call(
                        EnumerableMethods.Where.MakeGenericMethod(typeof(ValueBuffer)),
                        inMemoryQueryExpression.ServerQueryExpression,
                        discriminatorPredicate);

                    var projectionBindingExpression = (ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression;
                    var projectionMember = projectionBindingExpression.ProjectionMember;
                    var entityProjection = (EntityProjectionExpression)inMemoryQueryExpression.GetMappedProjection(projectionMember);

                    inMemoryQueryExpression.ReplaceProjectionMapping(
                        new Dictionary<ProjectionMember, Expression>
                        {
                            { projectionMember, entityProjection.UpdateEntityType(derivedType) }
                        });

                    source.ShaperExpression = entityShaperExpression.WithEntityType(derivedType);

                    return source;
                }
            }

            return null;
        }

        protected override ShapedQueryExpression TranslateOrderBy(
            ShapedQueryExpression source, LambdaExpression keySelector, bool ascending)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;

            keySelector = TranslateLambdaExpression(source, keySelector);
            if (keySelector == null)
            {
                return null;
            }

            var orderBy = ascending ? EnumerableMethods.OrderBy : EnumerableMethods.OrderByDescending;
            inMemoryQueryExpression.ServerQueryExpression
                = Expression.Call(
                    orderBy.MakeGenericMethod(inMemoryQueryExpression.CurrentParameter.Type, keySelector.ReturnType),
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

            var groupByQuery = source.ShaperExpression is GroupByShaperExpression;
            var queryExpression = (InMemoryQueryExpression)source.QueryExpression;

            source.ShaperExpression = _projectionBindingExpressionVisitor.Translate(queryExpression, newSelectorBody);

            if (groupByQuery)
            {
                queryExpression.PushdownIntoSubquery();
            }

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
                innerParameter, Expression.Parameter(source.Type.TryGetSequenceType()), innerParameter);

            return TranslateSelectMany(source, selector, resultSelector);
        }

        protected override ShapedQueryExpression TranslateSingleOrDefault(
            ShapedQueryExpression source, LambdaExpression predicate, Type returnType, bool returnDefault)
        {
            return TranslateSingleResultOperator(
                source,
                predicate,
                returnType,
                returnDefault
                    ? EnumerableMethods.SingleOrDefaultWithoutPredicate
                    : EnumerableMethods.SingleWithoutPredicate);
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
                    EnumerableMethods.Skip.MakeGenericMethod(inMemoryQueryExpression.CurrentParameter.Type),
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
                    EnumerableMethods.Take.MakeGenericMethod(inMemoryQueryExpression.CurrentParameter.Type),
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
                    (ascending ? EnumerableMethods.ThenBy : EnumerableMethods.ThenByDescending)
                    .MakeGenericMethod(inMemoryQueryExpression.CurrentParameter.Type, keySelector.ReturnType),
                    inMemoryQueryExpression.ServerQueryExpression,
                    keySelector);

            return source;
        }

        protected override ShapedQueryExpression TranslateUnion(ShapedQueryExpression source1, ShapedQueryExpression source2)
            => TranslateSetOperation(EnumerableMethods.Union, source1, source2);

        protected override ShapedQueryExpression TranslateWhere(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;
            predicate = TranslateLambdaExpression(source, predicate, preserveType: true);
            if (predicate == null)
            {
                return null;
            }

            inMemoryQueryExpression.ServerQueryExpression = Expression.Call(
                EnumerableMethods.Where.MakeGenericMethod(inMemoryQueryExpression.CurrentParameter.Type),
                inMemoryQueryExpression.ServerQueryExpression,
                predicate);

            return source;
        }

        private Expression TranslateExpression(Expression expression, bool preserveType = false)
        {
            var result = _expressionTranslator.Translate(expression);

            if (expression != null
                && result != null
                && preserveType
                && expression.Type != result.Type)
            {
                result = expression.Type == typeof(bool)
                    ? Expression.Equal(result, Expression.Constant(true, result.Type))
                    : (Expression)Expression.Convert(result, expression.Type);
            }

            return result;
        }

        private LambdaExpression TranslateLambdaExpression(
            ShapedQueryExpression shapedQueryExpression,
            LambdaExpression lambdaExpression,
            bool preserveType = false)
        {
            var lambdaBody = TranslateExpression(RemapLambdaBody(shapedQueryExpression, lambdaExpression), preserveType);

            return lambdaBody != null
                ? Expression.Lambda(
                    lambdaBody,
                    ((InMemoryQueryExpression)shapedQueryExpression.QueryExpression).CurrentParameter)
                : null;
        }

        private Expression RemapLambdaBody(ShapedQueryExpression shapedQueryExpression, LambdaExpression lambdaExpression)
        {
            var lambdaBody = ReplacingExpressionVisitor.Replace(
                lambdaExpression.Parameters.Single(), shapedQueryExpression.ShaperExpression, lambdaExpression.Body);

            return ExpandWeakEntities((InMemoryQueryExpression)shapedQueryExpression.QueryExpression, lambdaBody);
        }

        internal Expression ExpandWeakEntities(InMemoryQueryExpression queryExpression, Expression lambdaBody)
            => _weakEntityExpandingExpressionVisitor.Expand(queryExpression, lambdaBody);

        private class WeakEntityExpandingExpressionVisitor : ExpressionVisitor
        {
            private InMemoryQueryExpression _queryExpression;
            private readonly InMemoryExpressionTranslatingExpressionVisitor _expressionTranslator;

            public WeakEntityExpandingExpressionVisitor(InMemoryExpressionTranslatingExpressionVisitor expressionTranslator)
            {
                _expressionTranslator = expressionTranslator;
            }

            public virtual Expression Expand(InMemoryQueryExpression queryExpression, Expression lambdaBody)
            {
                _queryExpression = queryExpression;

                return Visit(lambdaBody);
            }

            protected override Expression VisitMember(MemberExpression memberExpression)
            {
                var innerExpression = Visit(memberExpression.Expression);

                return TryExpand(innerExpression, MemberIdentity.Create(memberExpression.Member))
                    ?? memberExpression.Update(innerExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var navigationName))
                {
                    source = Visit(source);

                    return TryExpand(source, MemberIdentity.Create(navigationName))
                        ?? methodCallExpression.Update(null, new[] { source, methodCallExpression.Arguments[1] });
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            protected override Expression VisitExtension(Expression extensionExpression)
                => extensionExpression is EntityShaperExpression
                    ? extensionExpression
                    : base.VisitExtension(extensionExpression);

            private Expression TryExpand(Expression source, MemberIdentity member)
            {
                source = source.UnwrapTypeConversion(out var convertedType);
                if (!(source is EntityShaperExpression entityShaperExpression))
                {
                    return null;
                }

                var entityType = entityShaperExpression.EntityType;
                if (convertedType != null)
                {
                    entityType = entityType.GetRootType().GetDerivedTypesInclusive()
                        .FirstOrDefault(et => et.ClrType == convertedType);

                    if (entityType == null)
                    {
                        return null;
                    }
                }

                var navigation = member.MemberInfo != null
                    ? entityType.FindNavigation(member.MemberInfo)
                    : entityType.FindNavigation(member.Name);

                if (navigation == null)
                {
                    return null;
                }

                var targetEntityType = navigation.GetTargetType();
                if (targetEntityType == null
                    || (!targetEntityType.HasDefiningNavigation()
                        && !targetEntityType.IsOwned()))
                {
                    return null;
                }

                var foreignKey = navigation.ForeignKey;
                if (navigation.IsCollection())
                {
                    var innerShapedQuery = CreateShapedQueryExpression(targetEntityType);
                    var innerQueryExpression = (InMemoryQueryExpression)innerShapedQuery.QueryExpression;

                    var makeNullable = foreignKey.PrincipalKey.Properties
                        .Concat(foreignKey.Properties)
                        .Select(p => p.ClrType)
                        .Any(t => t.IsNullableType());

                    var outerKey = entityShaperExpression.CreateKeyAccessExpression(
                        navigation.IsDependentToPrincipal()
                            ? foreignKey.Properties
                            : foreignKey.PrincipalKey.Properties,
                        makeNullable);
                    var innerKey = innerShapedQuery.ShaperExpression.CreateKeyAccessExpression(
                        navigation.IsDependentToPrincipal()
                            ? foreignKey.PrincipalKey.Properties
                            : foreignKey.Properties,
                        makeNullable);

                    var outerKeyFirstProperty = outerKey is NewExpression newExpression
                        ? ((UnaryExpression)((NewArrayExpression)newExpression.Arguments[0]).Expressions[0]).Operand
                        : outerKey;

                    var predicate = outerKeyFirstProperty.Type.IsNullableType()
                        ? Expression.AndAlso(
                            Expression.NotEqual(outerKeyFirstProperty, Expression.Constant(null, outerKeyFirstProperty.Type)),
                            Expression.Equal(outerKey, innerKey))
                        : Expression.Equal(outerKey, innerKey);

                    var correlationPredicate = _expressionTranslator.Translate(predicate);
                    innerQueryExpression.ServerQueryExpression = Expression.Call(
                        EnumerableMethods.Where.MakeGenericMethod(innerQueryExpression.CurrentParameter.Type),
                        innerQueryExpression.ServerQueryExpression,
                        Expression.Lambda(correlationPredicate, innerQueryExpression.CurrentParameter));

                    return innerShapedQuery;
                }

                var entityProjectionExpression
                    = (EntityProjectionExpression)(entityShaperExpression.ValueBufferExpression is
                        ProjectionBindingExpression projectionBindingExpression
                        ? _queryExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember)
                        : entityShaperExpression.ValueBufferExpression);

                var innerShaper = entityProjectionExpression.BindNavigation(navigation);
                if (innerShaper == null)
                {
                    var innerShapedQuery = CreateShapedQueryExpression(targetEntityType);
                    var innerQueryExpression = (InMemoryQueryExpression)innerShapedQuery.QueryExpression;

                    var makeNullable = foreignKey.PrincipalKey.Properties
                        .Concat(foreignKey.Properties)
                        .Select(p => p.ClrType)
                        .Any(t => t.IsNullableType());

                    var outerKey = entityShaperExpression.CreateKeyAccessExpression(
                        navigation.IsDependentToPrincipal()
                            ? foreignKey.Properties
                            : foreignKey.PrincipalKey.Properties,
                        makeNullable);
                    var innerKey = innerShapedQuery.ShaperExpression.CreateKeyAccessExpression(
                        navigation.IsDependentToPrincipal()
                            ? foreignKey.PrincipalKey.Properties
                            : foreignKey.Properties,
                        makeNullable);

                    var outerKeySelector = Expression.Lambda(_expressionTranslator.Translate(outerKey), _queryExpression.CurrentParameter);
                    var innerKeySelector = Expression.Lambda(
                        _expressionTranslator.Translate(innerKey), innerQueryExpression.CurrentParameter);
                    (outerKeySelector, innerKeySelector) = AlignKeySelectorTypes(outerKeySelector, innerKeySelector);
                    innerShaper = _queryExpression.AddNavigationToWeakEntityType(
                        entityProjectionExpression, navigation, innerQueryExpression, outerKeySelector, innerKeySelector);
                }

                return innerShaper;
            }
        }

        private ShapedQueryExpression TranslateScalarAggregate(
            ShapedQueryExpression source, LambdaExpression selector, string methodName)
        {
            var inMemoryQueryExpression = (InMemoryQueryExpression)source.QueryExpression;

            selector = selector == null
                || selector.Body == selector.Parameters[0]
                    ? Expression.Lambda(
                        inMemoryQueryExpression.GetMappedProjection(new ProjectionMember()),
                        inMemoryQueryExpression.CurrentParameter)
                    : TranslateLambdaExpression(source, selector, preserveType: true);

            if (selector == null)
            {
                return null;
            }

            var method = GetMethod();
            method = method.GetGenericArguments().Length == 2
                ? method.MakeGenericMethod(typeof(ValueBuffer), selector.ReturnType)
                : method.MakeGenericMethod(typeof(ValueBuffer));

            inMemoryQueryExpression.ServerQueryExpression
                = Expression.Call(
                    method,
                    inMemoryQueryExpression.ServerQueryExpression,
                    selector);

            source.ShaperExpression = inMemoryQueryExpression.GetSingleScalarProjection();

            return source;

            MethodInfo GetMethod()
                => methodName switch
                {
                    nameof(Enumerable.Average) => EnumerableMethods.GetAverageWithSelector(selector.ReturnType),
                    nameof(Enumerable.Max) => EnumerableMethods.GetMaxWithSelector(selector.ReturnType),
                    nameof(Enumerable.Min) => EnumerableMethods.GetMinWithSelector(selector.ReturnType),
                    nameof(Enumerable.Sum) => EnumerableMethods.GetSumWithSelector(selector.ReturnType),
                    _ => throw new InvalidOperationException("Invalid Aggregate Operator encountered."),
                };
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
                    method.MakeGenericMethod(inMemoryQueryExpression.CurrentParameter.Type),
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
