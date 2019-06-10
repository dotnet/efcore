// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using System.IO;
using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalQueryableMethodTranslatingExpressionVisitor : QueryableMethodTranslatingExpressionVisitor
    {
        private readonly RelationalSqlTranslatingExpressionVisitor _sqlTranslator;
        private readonly RelationalProjectionBindingExpressionVisitor _projectionBindingExpressionVisitor;
        private readonly IModel _model;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public RelationalQueryableMethodTranslatingExpressionVisitor(
            IModel model,
            IRelationalSqlTranslatingExpressionVisitorFactory relationalSqlTranslatingExpressionVisitorFactory,
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlTranslator = relationalSqlTranslatingExpressionVisitorFactory.Create(model, this);

            _projectionBindingExpressionVisitor = new RelationalProjectionBindingExpressionVisitor(this, _sqlTranslator);
            _model = model;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        private RelationalQueryableMethodTranslatingExpressionVisitor(
            IModel model,
            RelationalSqlTranslatingExpressionVisitor sqlTranslator,
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _model = model;
            _sqlTranslator = sqlTranslator;
            _projectionBindingExpressionVisitor = new RelationalProjectionBindingExpressionVisitor(this, sqlTranslator);
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public override ShapedQueryExpression TranslateSubquery(Expression expression)
        {
            return (ShapedQueryExpression)new RelationalQueryableMethodTranslatingExpressionVisitor(
                _model,
                _sqlTranslator,
                _sqlExpressionFactory).Visit(expression);

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

            if (selector != null)
            {
                source = TranslateSelect(source, selector);
            }

            var projection = (SqlExpression)selectExpression.GetMappedProjection(new ProjectionMember());

            var inputType = projection.Type.UnwrapNullableType();
            if (inputType == typeof(int)
                || inputType == typeof(long))
            {
                projection = _sqlExpressionFactory.ApplyDefaultTypeMapping(
                    _sqlExpressionFactory.Convert(projection, typeof(double)));
            }

            if (inputType == typeof(float))
            {
                projection = _sqlExpressionFactory.Convert(
                        _sqlExpressionFactory.Function(
                            "AVG", new[] { projection }, typeof(double), null),
                        projection.Type,
                        projection.TypeMapping);
            }
            else
            {
                projection = _sqlExpressionFactory.Function(
                    "AVG", new[] { projection }, projection.Type, projection.TypeMapping);
            }

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

        protected override ShapedQueryExpression TranslateConcat(ShapedQueryExpression source1, ShapedQueryExpression source2) => throw new NotImplementedException();

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

            var translation = _sqlExpressionFactory.ApplyDefaultTypeMapping(
                _sqlExpressionFactory.Function("COUNT", new[] { _sqlExpressionFactory.Fragment("*") }, typeof(int)));

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

        protected override ShapedQueryExpression TranslateExcept(ShapedQueryExpression source1, ShapedQueryExpression source2) => throw new NotImplementedException();

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

        protected override ShapedQueryExpression TranslateGroupBy(ShapedQueryExpression source, LambdaExpression keySelector, LambdaExpression elementSelector, LambdaExpression resultSelector) => throw new NotImplementedException();

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

        protected override ShapedQueryExpression TranslateIntersect(ShapedQueryExpression source1, ShapedQueryExpression source2) => throw new NotImplementedException();

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
            var outerKey = RemapLambdaBody(outer.ShaperExpression, outerKeySelector);
            var innerKey = RemapLambdaBody(inner.ShaperExpression, innerKeySelector);

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

            if (left != null && right != null)
            {
                return _sqlExpressionFactory.Equal(left, right);
            }

            return null;
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

            var translation = _sqlExpressionFactory.ApplyDefaultTypeMapping(
                _sqlExpressionFactory.Function("COUNT", new[] { _sqlExpressionFactory.Fragment("*") }, typeof(long)));
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

            if (selector != null)
            {
                source = TranslateSelect(source, selector);
            }

            var projection = (SqlExpression)selectExpression.GetMappedProjection(new ProjectionMember());

            projection = _sqlExpressionFactory.Function("MAX", new[] { projection }, resultType, projection.TypeMapping);

            return AggregateResultShaper(source, projection, throwOnNullResult: true, resultType);
        }

        protected override ShapedQueryExpression TranslateMin(ShapedQueryExpression source, LambdaExpression selector, Type resultType)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.PrepareForAggregate();

            if (selector != null)
            {
                source = TranslateSelect(source, selector);
            }

            var projection = (SqlExpression)selectExpression.GetMappedProjection(new ProjectionMember());

            projection = _sqlExpressionFactory.Function("MIN", new[] { projection }, resultType, projection.TypeMapping);

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
                    source.ShaperExpression = new EntityShaperExpression(
                        baseType, entityShaperExpression.ValueBufferExpression, entityShaperExpression.Nullable);

                    return source;
                }

                var derivedType = entityType.GetDerivedTypes().SingleOrDefault(et => et.ClrType == resultType);
                if (derivedType != null)
                {
                    var selectExpression = (SelectExpression)source.QueryExpression;
                    var concreteEntityTypes = derivedType.GetConcreteDerivedTypesInclusive().ToList();
                    var discriminatorColumn = selectExpression
                        .BindProperty(entityShaperExpression.ValueBufferExpression, entityType.GetDiscriminatorProperty());

                    var predicate = concreteEntityTypes.Count == 1
                        ? _sqlExpressionFactory.Equal(discriminatorColumn,
                            _sqlExpressionFactory.Constant(concreteEntityTypes[0].GetDiscriminatorValue()))
                        : (SqlExpression)_sqlExpressionFactory.In(discriminatorColumn,
                            _sqlExpressionFactory.Constant(concreteEntityTypes.Select(et => et.GetDiscriminatorValue())),
                            negated: false);

                    selectExpression.ApplyPredicate(predicate);

                    var projectionMember = entityShaperExpression.ValueBufferExpression.ProjectionMember;

                    Debug.Assert(new ProjectionMember().Equals(projectionMember),
                        "Invalid ProjectionMember when processing OfType");

                    var entityProjection = (EntityProjectionExpression)selectExpression.GetMappedProjection(projectionMember);

                    selectExpression.ReplaceProjectionMapping(
                        new Dictionary<ProjectionMember, Expression>
                        {
                            { projectionMember, entityProjection.UpdateEntityType(derivedType)}
                        });

                    source.ShaperExpression = new EntityShaperExpression(
                        derivedType, entityShaperExpression.ValueBufferExpression, entityShaperExpression.Nullable);

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

            var newSelectorBody = ReplacingExpressionVisitor.Replace(selector.Parameters.Single(), source.ShaperExpression, selector.Body);

            source.ShaperExpression = _projectionBindingExpressionVisitor
                .Translate(selectExpression, newSelectorBody);

            return source;
        }

        private static readonly MethodInfo _defaultIfEmptyWithoutArgMethodInfo = typeof(Enumerable).GetTypeInfo()
            .GetDeclaredMethods(nameof(Enumerable.DefaultIfEmpty)).Single(mi => mi.GetParameters().Length == 1);

        protected override ShapedQueryExpression TranslateSelectMany(ShapedQueryExpression source, LambdaExpression collectionSelector, LambdaExpression resultSelector)
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

            if (selector != null)
            {
                source = TranslateSelect(source, selector);
            }

            var serverOutputType = resultType.UnwrapNullableType();
            var projection = (SqlExpression)selectExpression.GetMappedProjection(new ProjectionMember());

            if (serverOutputType == typeof(float))
            {
                projection = _sqlExpressionFactory.Convert(
                        _sqlExpressionFactory.Function("SUM", new[] { projection }, typeof(double)),
                        serverOutputType,
                        projection.TypeMapping);
            }
            else
            {
                projection = _sqlExpressionFactory.Function(
                    "SUM", new[] { projection }, serverOutputType, projection.TypeMapping);
            }

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

        protected override ShapedQueryExpression TranslateUnion(ShapedQueryExpression source1, ShapedQueryExpression source2) => throw new NotImplementedException();

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
            var lambdaBody = RemapLambdaBody(shapedQueryExpression.ShaperExpression, lambdaExpression);

            return TranslateExpression(lambdaBody);
        }

        private Expression RemapLambdaBody(Expression shaperBody, LambdaExpression lambdaExpression)
        {
            return ReplacingExpressionVisitor.Replace(lambdaExpression.Parameters.Single(), shaperBody, lambdaExpression.Body);
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
