// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalQueryableMethodTranslatingExpressionVisitor : QueryableMethodTranslatingExpressionVisitor
    {
        private readonly RelationalSqlTranslatingExpressionVisitor _sqlTranslator;
        private readonly WeakEntityExpandingExpressionVisitor _weakEntityExpandingExpressionVisitor;
        private readonly RelationalProjectionBindingExpressionVisitor _projectionBindingExpressionVisitor;
        private readonly IModel _model;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public RelationalQueryableMethodTranslatingExpressionVisitor(
            IModel model,
            IRelationalSqlTranslatingExpressionVisitorFactory relationalSqlTranslatingExpressionVisitorFactory,
            ISqlExpressionFactory sqlExpressionFactory)
            : base(subquery: false)
        {
            _sqlTranslator = relationalSqlTranslatingExpressionVisitorFactory.Create(model, this);
            _weakEntityExpandingExpressionVisitor = new WeakEntityExpandingExpressionVisitor(_sqlTranslator, sqlExpressionFactory);
            _projectionBindingExpressionVisitor = new RelationalProjectionBindingExpressionVisitor(this, _sqlTranslator);
            _model = model;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        private RelationalQueryableMethodTranslatingExpressionVisitor(
            IModel model,
            RelationalSqlTranslatingExpressionVisitor sqlTranslator,
            WeakEntityExpandingExpressionVisitor weakEntityExpandingExpressionVisitor,
            ISqlExpressionFactory sqlExpressionFactory)
            : base(subquery: true)
        {
            _model = model;
            _sqlTranslator = sqlTranslator;
            _weakEntityExpandingExpressionVisitor = weakEntityExpandingExpressionVisitor;
            _projectionBindingExpressionVisitor = new RelationalProjectionBindingExpressionVisitor(this, sqlTranslator);
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType == typeof(RelationalQueryableExtensions)
                && methodCallExpression.Method.Name == nameof(RelationalQueryableExtensions.FromSqlOnQueryable))
            {
                var sql = (string)((ConstantExpression)methodCallExpression.Arguments[1]).Value;
                var queryable = (IQueryable)((ConstantExpression)methodCallExpression.Arguments[0]).Value;
                return CreateShapedQueryExpression(queryable.ElementType, sql, methodCallExpression.Arguments[2]);
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        public override ShapedQueryExpression TranslateSubquery(Expression expression)
            => (ShapedQueryExpression)new RelationalQueryableMethodTranslatingExpressionVisitor(
                _model,
                _sqlTranslator,
                _weakEntityExpandingExpressionVisitor,
                _sqlExpressionFactory).Visit(expression);

        protected override ShapedQueryExpression CreateShapedQueryExpression(Type elementType)
        {
            var entityType = _model.FindEntityType(elementType);
            var queryExpression = _sqlExpressionFactory.Select(entityType);

            return CreateShapedQueryExpression(entityType, queryExpression);
        }

        private ShapedQueryExpression CreateShapedQueryExpression(Type elementType, string sql, Expression arguments)
        {
            var entityType = _model.FindEntityType(elementType);
            var queryExpression = _sqlExpressionFactory.Select(entityType, sql, arguments);

            return CreateShapedQueryExpression(entityType, queryExpression);
        }

        private static ShapedQueryExpression CreateShapedQueryExpression(IEntityType entityType, SelectExpression selectExpression)
        {
            return new ShapedQueryExpression(
                selectExpression,
                new EntityShaperExpression(
                entityType,
                new ProjectionBindingExpression(
                    selectExpression,
                    new ProjectionMember(),
                    typeof(ValueBuffer)),
                false));
        }

        protected override ShapedQueryExpression TranslateAll(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var translation = TranslateLambdaExpression(source, predicate);

            if (translation != null)
            {
                var selectExpression = (SelectExpression)source.QueryExpression;
                selectExpression.ApplyPredicate(_sqlExpressionFactory.Not(translation));
                selectExpression.ReplaceProjectionMapping(new Dictionary<ProjectionMember, Expression>());
                if (selectExpression.Limit == null
                    && selectExpression.Offset == null)
                {
                    selectExpression.ClearOrdering();
                }

                translation = _sqlExpressionFactory.Exists(selectExpression, true);
                source.QueryExpression = _sqlExpressionFactory.Select(translation);
                source.ShaperExpression = new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), typeof(bool));

                return source;
            }

            throw new InvalidOperationException();
        }

        protected override ShapedQueryExpression TranslateAny(ShapedQueryExpression source, LambdaExpression predicate)
        {
            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
            }

            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.ReplaceProjectionMapping(new Dictionary<ProjectionMember, Expression>());
            if (selectExpression.Limit == null
                && selectExpression.Offset == null)
            {
                selectExpression.ClearOrdering();
            }

            var translation = _sqlExpressionFactory.Exists(selectExpression, false);
            source.QueryExpression = _sqlExpressionFactory.Select(translation);
            source.ShaperExpression = new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), typeof(bool));

            return source;
        }

        protected override ShapedQueryExpression TranslateAverage(ShapedQueryExpression source, LambdaExpression selector, Type resultType)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.PrepareForAggregate();

            var newSelector = selector == null
                || selector.Body == selector.Parameters[0]
                ? selectExpression.GetMappedProjection(new ProjectionMember())
                : ReplacingExpressionVisitor.Replace(selector.Parameters.Single(), source.ShaperExpression, selector.Body);

            var projection = _sqlTranslator.TranslateAverage(newSelector);

            return AggregateResultShaper(source, projection, throwOnNullResult: true, resultType);
        }

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
        {
            var operand1 = (SelectExpression)source1.QueryExpression;
            var operand2 = (SelectExpression)source2.QueryExpression;
            source1.ShaperExpression = operand1.ApplySetOperation(SetOperationType.UnionAll, operand2, source1.ShaperExpression);
            return source1;
        }

        protected override ShapedQueryExpression TranslateContains(ShapedQueryExpression source, Expression item)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            var translation = TranslateExpression(item);

            if (translation != null)
            {
                if (selectExpression.Limit == null
                    && selectExpression.Offset == null)
                {
                    selectExpression.ClearOrdering();
                }

                selectExpression.ApplyProjection();
                translation = _sqlExpressionFactory.In(translation, selectExpression, false);
                source.QueryExpression = _sqlExpressionFactory.Select(translation);
                source.ShaperExpression = new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), typeof(bool));

                return source;
            }

            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateCount(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.PrepareForAggregate();

            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
            }

            var translation = _sqlTranslator.TranslateCount();

            var projectionMapping = new Dictionary<ProjectionMember, Expression>
            {
                { new ProjectionMember(), translation }
            };

            selectExpression.ClearOrdering();
            selectExpression.ReplaceProjectionMapping(projectionMapping);
            source.ShaperExpression = new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), typeof(int));

            return source;
        }

        protected override ShapedQueryExpression TranslateDefaultIfEmpty(ShapedQueryExpression source, Expression defaultValue) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateDistinct(ShapedQueryExpression source)
        {
            ((SelectExpression)source.QueryExpression).ApplyDistinct();

            return source;
        }

        protected override ShapedQueryExpression TranslateElementAtOrDefault(ShapedQueryExpression source, Expression index, bool returnDefault) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateExcept(ShapedQueryExpression source1, ShapedQueryExpression source2)
        {
            var operand1 = (SelectExpression)source1.QueryExpression;
            var operand2 = (SelectExpression)source2.QueryExpression;
            source1.ShaperExpression = operand1.ApplySetOperation(SetOperationType.Except, operand2, source1.ShaperExpression);
            return source1;
        }

        protected override ShapedQueryExpression TranslateFirstOrDefault(ShapedQueryExpression source, LambdaExpression predicate, Type returnType, bool returnDefault)
        {
            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
            }

            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.ApplyLimit(TranslateExpression(Expression.Constant(1)));

            if (source.ShaperExpression.Type != returnType)
            {
                source.ShaperExpression = Expression.Convert(source.ShaperExpression, returnType);
            }

            return source;
        }

        protected override ShapedQueryExpression TranslateGroupBy(
            ShapedQueryExpression source,
            LambdaExpression keySelector,
            LambdaExpression elementSelector,
            LambdaExpression resultSelector)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.PrepareForAggregate();

            var remappedKeySelector = RemapLambdaBody(source, keySelector);

            var translatedKey = TranslateGroupingKey(remappedKeySelector)
                ?? (remappedKeySelector is ConstantExpression ? remappedKeySelector : null);
            if (translatedKey != null)
            {
                if (elementSelector != null)
                {
                    source = TranslateSelect(source, elementSelector);
                }

                var sqlKeySelector = translatedKey is ConstantExpression
                    ? _sqlExpressionFactory.ApplyDefaultTypeMapping(_sqlExpressionFactory.Constant(1))
                    : translatedKey;

                var appliedKeySelector = selectExpression.ApplyGrouping(sqlKeySelector);
                translatedKey = translatedKey is ConstantExpression ? translatedKey : appliedKeySelector;

                source.ShaperExpression = new GroupByShaperExpression(translatedKey, source.ShaperExpression);

                if (resultSelector == null)
                {
                    return source;
                }

                var keyAccessExpression = Expression.MakeMemberAccess(
                    source.ShaperExpression,
                    source.ShaperExpression.Type.GetTypeInfo().GetMember(nameof(IGrouping<int, int>.Key))[0]);

                var newResultSelectorBody = ReplacingExpressionVisitor.Replace(
                    resultSelector.Parameters[0], keyAccessExpression,
                    resultSelector.Parameters[1], source.ShaperExpression,
                    resultSelector.Body);

                source.ShaperExpression = _projectionBindingExpressionVisitor.Translate(selectExpression, newResultSelectorBody);

                return source;
            }

            throw new InvalidOperationException();
        }

        private Expression TranslateGroupingKey(Expression expression)
        {
            if (expression is NewExpression newExpression)
            {
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
            }

            if (expression is MemberInitExpression memberInitExpression)
            {
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
            }

            return _sqlTranslator.Translate(expression);
        }

        protected override ShapedQueryExpression TranslateGroupJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector)
        {
            //var outerSelectExpression = (SelectExpression)outer.QueryExpression;
            //if (outerSelectExpression.Limit != null
            //    || outerSelectExpression.Offset != null
            //    || outerSelectExpression.IsDistinct)
            //{
            //    outerSelectExpression.PushdownIntoSubQuery();
            //}

            //var innerSelectExpression = (SelectExpression)inner.QueryExpression;
            //if (innerSelectExpression.Orderings.Any()
            //    || innerSelectExpression.Limit != null
            //    || innerSelectExpression.Offset != null
            //    || innerSelectExpression.IsDistinct
            //    || innerSelectExpression.Predicate != null
            //    || innerSelectExpression.Tables.Count > 1)
            //{
            //    innerSelectExpression.PushdownIntoSubQuery();
            //}

            //var joinPredicate = CreateJoinPredicate(outer, outerKeySelector, inner, innerKeySelector);
            //if (joinPredicate != null)
            //{
            //    outer = TranslateThenBy(outer, outerKeySelector, true);

            //    var innerTransparentIdentifierType = CreateTransparentIdentifierType(
            //        resultSelector.Parameters[0].Type,
            //        resultSelector.Parameters[1].Type.TryGetSequenceType());

            //    outerSelectExpression.AddLeftJoin(
            //        innerSelectExpression, joinPredicate, innerTransparentIdentifierType);

            //    return TranslateResultSelectorForGroupJoin(
            //        outer,
            //        inner.ShaperExpression,
            //        outerKeySelector,
            //        innerKeySelector,
            //        resultSelector,
            //        innerTransparentIdentifierType);
            //}

            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateIntersect(ShapedQueryExpression source1, ShapedQueryExpression source2)
        {
            var operand1 = (SelectExpression)source1.QueryExpression;
            var operand2 = (SelectExpression)source2.QueryExpression;
            source1.ShaperExpression = operand1.ApplySetOperation(SetOperationType.Intersect, operand2, source1.ShaperExpression);
            return source1;
        }

        protected override ShapedQueryExpression TranslateJoin(
            ShapedQueryExpression outer,
            ShapedQueryExpression inner,
            LambdaExpression outerKeySelector,
            LambdaExpression innerKeySelector,
            LambdaExpression resultSelector)
        {
            var joinPredicate = CreateJoinPredicate(outer, outerKeySelector, inner, innerKeySelector);
            if (joinPredicate != null)
            {
                var transparentIdentifierType = CreateTransparentIdentifierType(
                    resultSelector.Parameters[0].Type,
                    resultSelector.Parameters[1].Type);

                ((SelectExpression)outer.QueryExpression).AddInnerJoin(
                    (SelectExpression)inner.QueryExpression, joinPredicate, transparentIdentifierType);

                return TranslateResultSelectorForJoin(
                    outer,
                    resultSelector,
                    inner.ShaperExpression,
                    transparentIdentifierType,
                    false);
            }

            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateLeftJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector)
        {
            var joinPredicate = CreateJoinPredicate(outer, outerKeySelector, inner, innerKeySelector);
            if (joinPredicate != null)
            {
                var transparentIdentifierType = CreateTransparentIdentifierType(
                    resultSelector.Parameters[0].Type,
                    resultSelector.Parameters[1].Type);

                ((SelectExpression)outer.QueryExpression).AddLeftJoin(
                    (SelectExpression)inner.QueryExpression, joinPredicate, transparentIdentifierType);

                return TranslateResultSelectorForJoin(
                    outer,
                    resultSelector,
                    inner.ShaperExpression,
                    transparentIdentifierType,
                    true);
            }

            throw new NotImplementedException();
        }

        private SqlBinaryExpression CreateJoinPredicate(
            ShapedQueryExpression outer,
            LambdaExpression outerKeySelector,
            ShapedQueryExpression inner,
            LambdaExpression innerKeySelector)
        {
            var outerKey = RemapLambdaBody(outer, outerKeySelector);
            var innerKey = RemapLambdaBody(inner, innerKeySelector);

            if (outerKey is NewExpression outerNew)
            {
                var innerNew = (NewExpression)innerKey;

                return outerNew.Type == typeof(AnonymousObject)
                    ? CreateJoinPredicate(
                        ((NewArrayExpression)outerNew.Arguments[0]).Expressions,
                        ((NewArrayExpression)innerNew.Arguments[0]).Expressions)
                    : CreateJoinPredicate(outerNew.Arguments, innerNew.Arguments);
            }

            return CreateJoinPredicate(outerKey, innerKey);
        }

        private SqlBinaryExpression CreateJoinPredicate(
            IList<Expression> outerExpressions,
            IList<Expression> innerExpressions)
        {
            SqlBinaryExpression result = null;
            for (var i = 0; i < outerExpressions.Count; i++)
            {
                result = result == null
                    ? CreateJoinPredicate(outerExpressions[i], innerExpressions[i])
                    : _sqlExpressionFactory.AndAlso(
                        result,
                        CreateJoinPredicate(outerExpressions[i], innerExpressions[i]));
            }

            return result;
        }

        private SqlBinaryExpression CreateJoinPredicate(
            Expression outerKey,
            Expression innerKey)
        {
            var left = TranslateExpression(outerKey);
            var right = TranslateExpression(innerKey);

            return left != null && right != null
                ? _sqlExpressionFactory.Equal(left, right)
                : null;
        }

        protected override ShapedQueryExpression TranslateLastOrDefault(
            ShapedQueryExpression source, LambdaExpression predicate, Type returnType, bool returnDefault)
        {
            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
            }

            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.ReverseOrderings();
            selectExpression.ApplyLimit(TranslateExpression(Expression.Constant(1)));

            if (source.ShaperExpression.Type != returnType)
            {
                source.ShaperExpression = Expression.Convert(source.ShaperExpression, returnType);
            }

            return source;
        }

        protected override ShapedQueryExpression TranslateLongCount(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.PrepareForAggregate();

            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
            }

            var translation = _sqlTranslator.TranslateLongCount();
            var projectionMapping = new Dictionary<ProjectionMember, Expression>
            {
                { new ProjectionMember(), translation }
            };

            selectExpression.ClearOrdering();
            selectExpression.ReplaceProjectionMapping(projectionMapping);
            source.ShaperExpression = new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), typeof(long));

            return source;
        }

        protected override ShapedQueryExpression TranslateMax(ShapedQueryExpression source, LambdaExpression selector, Type resultType)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.PrepareForAggregate();

            var newSelector = selector == null
                || selector.Body == selector.Parameters[0]
                ? selectExpression.GetMappedProjection(new ProjectionMember())
                : ReplacingExpressionVisitor.Replace(selector.Parameters.Single(), source.ShaperExpression, selector.Body);

            var projection = _sqlTranslator.TranslateMax(newSelector);

            return AggregateResultShaper(source, projection, throwOnNullResult: true, resultType);
        }

        protected override ShapedQueryExpression TranslateMin(ShapedQueryExpression source, LambdaExpression selector, Type resultType)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.PrepareForAggregate();

            var newSelector = selector == null
                 || selector.Body == selector.Parameters[0]
                 ? selectExpression.GetMappedProjection(new ProjectionMember())
                 : ReplacingExpressionVisitor.Replace(selector.Parameters.Single(), source.ShaperExpression, selector.Body);

            var projection = _sqlTranslator.TranslateMin(newSelector);

            return AggregateResultShaper(source, projection, throwOnNullResult: true, resultType);
        }

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
                    var selectExpression = (SelectExpression)source.QueryExpression;
                    var concreteEntityTypes = derivedType.GetConcreteDerivedTypesInclusive().ToList();
                    var projectionBindingExpression = (ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression;
                    var entityProjectionExpression = (EntityProjectionExpression)selectExpression.GetMappedProjection(
                        projectionBindingExpression.ProjectionMember);
                    var discriminatorColumn = entityProjectionExpression.BindProperty(entityType.GetDiscriminatorProperty());

                    var predicate = concreteEntityTypes.Count == 1
                        ? _sqlExpressionFactory.Equal(discriminatorColumn,
                            _sqlExpressionFactory.Constant(concreteEntityTypes[0].GetDiscriminatorValue()))
                        : (SqlExpression)_sqlExpressionFactory.In(discriminatorColumn,
                            _sqlExpressionFactory.Constant(concreteEntityTypes.Select(et => et.GetDiscriminatorValue())),
                            negated: false);

                    selectExpression.ApplyPredicate(predicate);

                    var projectionMember = projectionBindingExpression.ProjectionMember;

                    Debug.Assert(new ProjectionMember().Equals(projectionMember),
                        "Invalid ProjectionMember when processing OfType");

                    var entityProjection = (EntityProjectionExpression)selectExpression.GetMappedProjection(projectionMember);

                    selectExpression.ReplaceProjectionMapping(
                        new Dictionary<ProjectionMember, Expression>
                        {
                            { projectionMember, entityProjection.UpdateEntityType(derivedType)}
                        });

                    source.ShaperExpression = entityShaperExpression.WithEntityType(derivedType);

                    return source;
                }

                // If the resultType is not part of hierarchy then we don't know how to materialize.
            }

            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateOrderBy(ShapedQueryExpression source, LambdaExpression keySelector, bool ascending)
        {
            var translation = TranslateLambdaExpression(source, keySelector);
            if (translation != null)
            {
                ((SelectExpression)source.QueryExpression).ApplyOrdering(new OrderingExpression(translation, ascending));

                return source;
            }

            throw new InvalidOperationException();
        }

        protected override ShapedQueryExpression TranslateReverse(ShapedQueryExpression source) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateSelect(ShapedQueryExpression source, LambdaExpression selector)
        {
            if (selector.Body == selector.Parameters[0])
            {
                return source;
            }

            var selectExpression = (SelectExpression)source.QueryExpression;
            if (selectExpression.IsDistinct)
            {
                selectExpression.PushdownIntoSubquery();
            }

            var newSelectorBody = RemapLambdaBody(source, selector);
            source.ShaperExpression = _projectionBindingExpressionVisitor.Translate(selectExpression, newSelectorBody);

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

                    ((SelectExpression)source.QueryExpression).AddCrossJoin(
                        (SelectExpression)inner.QueryExpression, transparentIdentifierType);

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

        protected override ShapedQueryExpression TranslateSelectMany(ShapedQueryExpression source, LambdaExpression selector)
        {
            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateSingleOrDefault(ShapedQueryExpression source, LambdaExpression predicate, Type returnType, bool returnDefault)
        {
            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
            }

            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.ApplyLimit(TranslateExpression(Expression.Constant(2)));

            if (source.ShaperExpression.Type != returnType)
            {
                source.ShaperExpression = Expression.Convert(source.ShaperExpression, returnType);
            }

            return source;
        }

        protected override ShapedQueryExpression TranslateSkip(ShapedQueryExpression source, Expression count)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            var translation = TranslateExpression(count);

            if (translation != null)
            {
                selectExpression.ApplyOffset(translation);

                return source;
            }

            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateSkipWhile(ShapedQueryExpression source, LambdaExpression predicate) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateSum(ShapedQueryExpression source, LambdaExpression selector, Type resultType)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.PrepareForAggregate();
            var newSelector = selector == null
                || selector.Body == selector.Parameters[0]
                ? selectExpression.GetMappedProjection(new ProjectionMember())
                : ReplacingExpressionVisitor.Replace(selector.Parameters.Single(), source.ShaperExpression, selector.Body);

            var projection = _sqlTranslator.TranslateSum(newSelector);

            return AggregateResultShaper(source, projection, throwOnNullResult: false, resultType);
        }

        protected override ShapedQueryExpression TranslateTake(ShapedQueryExpression source, Expression count)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            var translation = TranslateExpression(count);

            if (translation != null)
            {
                selectExpression.ApplyLimit(translation);

                return source;
            }

            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateTakeWhile(ShapedQueryExpression source, LambdaExpression predicate) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateThenBy(ShapedQueryExpression source, LambdaExpression keySelector, bool ascending)
        {
            var translation = TranslateLambdaExpression(source, keySelector);
            if (translation != null)
            {
                ((SelectExpression)source.QueryExpression).AppendOrdering(new OrderingExpression(translation, ascending));

                return source;
            }

            throw new InvalidOperationException();
        }

        protected override ShapedQueryExpression TranslateUnion(ShapedQueryExpression source1, ShapedQueryExpression source2)
        {
            var operand1 = (SelectExpression)source1.QueryExpression;
            var operand2 = (SelectExpression)source2.QueryExpression;
            source1.ShaperExpression = operand1.ApplySetOperation(SetOperationType.Union, operand2, source1.ShaperExpression);
            return source1;
        }

        protected override ShapedQueryExpression TranslateWhere(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var translation = TranslateLambdaExpression(source, predicate);
            if (translation != null)
            {
                ((SelectExpression)source.QueryExpression).ApplyPredicate(translation);

                return source;
            }

            throw new InvalidOperationException();
        }

        private SqlExpression TranslateExpression(Expression expression)
        {
            return _sqlTranslator.Translate(expression);
        }

        private SqlExpression TranslateLambdaExpression(
            ShapedQueryExpression shapedQueryExpression, LambdaExpression lambdaExpression)
        {
            var lambdaBody = RemapLambdaBody(shapedQueryExpression, lambdaExpression);

            return TranslateExpression(lambdaBody);
        }

        private Expression RemapLambdaBody(ShapedQueryExpression shapedQueryExpression, LambdaExpression lambdaExpression)
        {
            var lambdaBody = ReplacingExpressionVisitor.Replace(
                lambdaExpression.Parameters.Single(), shapedQueryExpression.ShaperExpression, lambdaExpression.Body);

            var selectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;
            lambdaBody = _weakEntityExpandingExpressionVisitor.Expand(selectExpression, lambdaBody);

            return lambdaBody;
        }

        public class WeakEntityExpandingExpressionVisitor : ExpressionVisitor
        {
            private SelectExpression _selectExpression;
            private readonly RelationalSqlTranslatingExpressionVisitor _sqlTranslator;
            private readonly ISqlExpressionFactory _sqlExpressionFactory;

            public WeakEntityExpandingExpressionVisitor(
                RelationalSqlTranslatingExpressionVisitor sqlTranslator, ISqlExpressionFactory sqlExpressionFactory)
            {
                _sqlTranslator = sqlTranslator;
                _sqlExpressionFactory = sqlExpressionFactory;
            }

            public Expression Expand(SelectExpression selectExpression, Expression lambdaBody)
            {
                _selectExpression = selectExpression;

                return Visit(lambdaBody);
            }

            protected override Expression VisitMember(MemberExpression memberExpression)
            {
                var innerExpression = Visit(memberExpression.Expression);

                if (innerExpression is EntityShaperExpression
                    || (innerExpression is UnaryExpression innerUnaryExpression
                        && innerUnaryExpression.NodeType == ExpressionType.Convert
                        && innerUnaryExpression.Operand is EntityShaperExpression))
                {
                    var collectionNavigation = Expand(innerExpression, MemberIdentity.Create(memberExpression.Member));
                    if (collectionNavigation != null)
                    {
                        return collectionNavigation;
                    }
                }

                return memberExpression.Update(innerExpression);
            }

            protected override Expression VisitExtension(Expression extensionExpression)
                => extensionExpression is EntityShaperExpression
                    ? extensionExpression
                    : base.VisitExtension(extensionExpression);

            private Expression Expand(Expression source, MemberIdentity member)
            {
                Type convertedType = null;
                if (source is UnaryExpression unaryExpression
                    && unaryExpression.NodeType == ExpressionType.Convert)
                {
                    source = unaryExpression.Operand;
                    if (unaryExpression.Type != typeof(object))
                    {
                        convertedType = unaryExpression.Type;
                    }
                }

                if (source is EntityShaperExpression entityShaperExpression)
                {
                    var entityType = entityShaperExpression.EntityType;
                    if (convertedType != null)
                    {
                        entityType = entityType.RootType().GetDerivedTypesInclusive()
                            .FirstOrDefault(et => et.ClrType == convertedType);

                        if (entityType == null)
                        {
                            return null;
                        }
                    }

                    var navigation = member.MemberInfo != null
                        ? entityType.FindNavigation(member.MemberInfo)
                        : entityType.FindNavigation(member.Name);

                    if (navigation != null)
                    {
                        if (navigation.IsCollection())
                        {
                            return CreateShapedQueryExpression(
                                navigation.GetTargetType(),
                                _sqlExpressionFactory.Select(navigation.GetTargetType()));
                        }

                        var entityProjectionExpression = (EntityProjectionExpression)
                            (entityShaperExpression.ValueBufferExpression is ProjectionBindingExpression projectionBindingExpression
                                ? _selectExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember)
                                : entityShaperExpression.ValueBufferExpression);

                        var innerShaper = entityProjectionExpression.BindNavigation(navigation);
                        if (innerShaper == null)
                        {
                            var targetEntityType = navigation.GetTargetType();
                            var innerSelectExpression = _sqlExpressionFactory.Select(targetEntityType);
                            var innerShapedQuery = CreateShapedQueryExpression(targetEntityType, innerSelectExpression);

                            var makeNullable = navigation.ForeignKey.PrincipalKey.Properties
                                .Concat(navigation.ForeignKey.Properties)
                                .Select(p => p.ClrType)
                                .Any(t => t.IsNullableType());

                            var outerKey = CreateKeyAccessExpression(
                                entityShaperExpression, navigation.ForeignKey.PrincipalKey.Properties, makeNullable);
                            var innerKey = CreateKeyAccessExpression(
                                innerShapedQuery.ShaperExpression, navigation.ForeignKey.Properties, makeNullable);

                            var joinPredicate = _sqlTranslator.Translate(Expression.Equal(outerKey, innerKey));
                            _selectExpression.AddLeftJoin(innerSelectExpression, joinPredicate, null);
                            var leftJoinTable = ((LeftJoinExpression)_selectExpression.Tables.Last()).Table;
                            innerShaper = new EntityShaperExpression(targetEntityType,
                                new EntityProjectionExpression(targetEntityType, leftJoinTable, true),
                                true);
                            entityProjectionExpression.AddNavigationBinding(navigation, innerShaper);
                        }

                        return innerShaper;
                    }
                }

                return null;
            }

            public static Expression CreateKeyAccessExpression(
                Expression target, IReadOnlyList<IProperty> properties, bool makeNullable = false)
                => properties.Count == 1
                    ? target.CreateEFPropertyExpression(properties[0], makeNullable)
                    : Expression.New(
                        AnonymousObject.AnonymousObjectCtor,
                        Expression.NewArrayInit(
                            typeof(object),
                            properties
                                .Select(p => Expression.Convert(target.CreateEFPropertyExpression(p, makeNullable), typeof(object)))
                                .Cast<Expression>()
                                .ToArray()));
        }

        private ShapedQueryExpression AggregateResultShaper(
            ShapedQueryExpression source, Expression projection, bool throwOnNullResult, Type resultType)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.ReplaceProjectionMapping(
                new Dictionary<ProjectionMember, Expression>
                {
                    { new ProjectionMember(), projection }
                });

            selectExpression.ClearOrdering();

            Expression shaper = new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), projection.Type);

            if (throwOnNullResult)
            {
                var resultVariable = Expression.Variable(projection.Type, "result");

                shaper = Expression.Block(
                    new[] { resultVariable },
                    Expression.Assign(resultVariable, shaper),
                    Expression.Condition(
                        Expression.Equal(resultVariable, Expression.Default(projection.Type)),
                        Expression.Throw(
                            Expression.New(
                                typeof(InvalidOperationException).GetConstructors()
                                    .Single(ci => ci.GetParameters().Length == 1),
                                Expression.Constant(RelationalStrings.NoElements)),
                            resultType),
                        resultType != resultVariable.Type
                            ? Expression.Convert(resultVariable, resultType)
                            : (Expression)resultVariable));
            }
            else if (resultType.IsNullableType())
            {
                shaper = Expression.Convert(shaper, resultType);
            }

            source.ShaperExpression = shaper;

            return source;
        }
    }
}
