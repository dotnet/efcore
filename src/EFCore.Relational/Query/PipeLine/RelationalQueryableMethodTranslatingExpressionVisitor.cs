// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalQueryableMethodTranslatingExpressionVisitor : QueryableMethodTranslatingExpressionVisitor
    {
        private readonly RelationalSqlTranslatingExpressionVisitor _sqlTranslator;
        private readonly RelationalProjectionBindingExpressionVisitor _projectionBindingExpressionVisitor;
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        public RelationalQueryableMethodTranslatingExpressionVisitor(
            IRelationalTypeMappingSource typeMappingSource,
            ITypeMappingApplyingExpressionVisitor typeMappingApplyingExpressionVisitor,
            IMemberTranslatorProvider memberTranslatorProvider,
            IMethodCallTranslatorProvider methodCallTranslatorProvider)
        {
            _sqlTranslator = new RelationalSqlTranslatingExpressionVisitor(
                typeMappingSource,
                typeMappingApplyingExpressionVisitor,
                memberTranslatorProvider,
                methodCallTranslatorProvider);

            _projectionBindingExpressionVisitor = new RelationalProjectionBindingExpressionVisitor(_sqlTranslator);
            _typeMappingSource = typeMappingSource;
        }

        protected override ShapedQueryExpression TranslateAll(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var translation = TranslateLambdaExpression(source, predicate, true);

            if (translation != null)
            {
                var selectExpression = (SelectExpression)source.QueryExpression;

                selectExpression.ApplyPredicate(
                    new SqlNotExpression(translation, _typeMappingSource.FindMapping(typeof(bool))));

                selectExpression.ApplyProjection(new Dictionary<ProjectionMember, Expression>());

                if (selectExpression.Limit == null
                    && selectExpression.Offset == null)
                {
                    selectExpression.ClearOrdering();
                }

                translation = new ExistsExpression(
                    selectExpression,
                    true,
                    _typeMappingSource.FindMapping(typeof(bool)))
                        .ConvertToValue(true);

                var projectionMapping = new Dictionary<ProjectionMember, Expression>
                {
                    { new ProjectionMember(), translation }
                };

                source.QueryExpression = new SelectExpression(
                    null,
                    projectionMapping,
                    new List<ProjectionExpression>(),
                    new List<TableExpressionBase>());

                source.ShaperExpression
                    = Expression.Lambda(
                        new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), typeof(bool)),
                        source.ShaperExpression.Parameters);

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

            selectExpression.ApplyProjection(new Dictionary<ProjectionMember, Expression>());

            if (selectExpression.Limit == null
                && selectExpression.Offset == null)
            {
                selectExpression.ClearOrdering();
            }

            var translation = new ExistsExpression(
                selectExpression,
                false,
                _typeMappingSource.FindMapping(typeof(bool)))
                    .ConvertToValue(true);

            var projectionMapping = new Dictionary<ProjectionMember, Expression>
            {
                { new ProjectionMember(), translation }
            };

            source.QueryExpression = new SelectExpression(
                "",
                projectionMapping,
                new List<ProjectionExpression>(),
                new List<TableExpressionBase>());

            source.ShaperExpression
                = Expression.Lambda(
                    new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), typeof(bool)),
                    source.ShaperExpression.Parameters);

            return source;
        }

        protected override ShapedQueryExpression TranslateAverage(ShapedQueryExpression source, LambdaExpression selector, Type resultType)
        {
            if (selector != null)
            {
                source = TranslateSelect(source, selector);
            }

            var projection = (SqlExpression)((SelectExpression)source.QueryExpression)
                .GetProjectionExpression(new ProjectionMember());

            var inputType = projection.Type.UnwrapNullableType();
            if (inputType == typeof(int)
                || inputType == typeof(long))
            {
                projection = new SqlCastExpression(projection, typeof(double), _typeMappingSource.FindMapping(typeof(double)));
            }

            if (projection.Type.UnwrapNullableType() == typeof(float))
            {
                projection = new SqlCastExpression(
                    new SqlFunctionExpression(
                        "AVG",
                        new[]
                        {
                            projection
                        },
                        typeof(double),
                        _typeMappingSource.FindMapping(typeof(double)),
                        false),
                    projection.Type,
                    projection.TypeMapping);
            }
            else
            {
                projection = new SqlFunctionExpression(
                    "AVG",
                    new[]
                    {
                        projection
                    },
                    projection.Type,
                    projection.TypeMapping,
                    false);
            }

            return AggregateResultShaper(source, projection, throwOnNullResult: true, resultType);
        }

        protected override ShapedQueryExpression TranslateCast(ShapedQueryExpression source, Type resultType)
        {
            if (source.ShaperExpression.ReturnType == resultType)
            {
                return source;
            }

            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateConcat(ShapedQueryExpression source1, ShapedQueryExpression source2) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateContains(ShapedQueryExpression source, Expression item)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            var translation = TranslateExpression(selectExpression, item, false);

            if (translation != null)
            {
                if (selectExpression.Limit == null
                    && selectExpression.Offset == null)
                {
                    selectExpression.ClearOrdering();
                }

                selectExpression.ApplyProjection();

                translation = new InExpression(
                    translation,
                    false,
                    selectExpression,
                    _typeMappingSource.FindMapping(typeof(bool)))
                        .ConvertToValue(true);

                var projectionMapping = new Dictionary<ProjectionMember, Expression>
                {
                    { new ProjectionMember(), translation }
                };


                source.QueryExpression = new SelectExpression(
                    "",
                    projectionMapping,
                    new List<ProjectionExpression>(),
                    new List<TableExpressionBase>());

                source.ShaperExpression
                    = Expression.Lambda(
                        new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), typeof(bool)),
                        source.ShaperExpression.Parameters);

                return source;
            }


            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateCount(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;

            if (selectExpression.IsDistinct
                || selectExpression.Limit != null
                || selectExpression.Offset != null)
            {
                selectExpression.PushdownIntoSubQuery();
            }

            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
            }

            var translation = new SqlFunctionExpression(
                    "COUNT",
                    new[]
                    {
                        new SqlFragmentExpression("*")
                    },
                    typeof(int),
                    _typeMappingSource.FindMapping(typeof(int)),
                    false);

            var _projectionMapping = new Dictionary<ProjectionMember, Expression>
            {
                { new ProjectionMember(), translation }
            };

            selectExpression.ClearOrdering();
            selectExpression.ApplyProjection(_projectionMapping);

            source.ShaperExpression
                = Expression.Lambda(
                    new ProjectionBindingExpression(selectExpression, new ProjectionMember(), typeof(int)),
                    source.ShaperExpression.Parameters);

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

        protected override ShapedQueryExpression TranslateFirstOrDefault(ShapedQueryExpression source, LambdaExpression predicate, bool returnDefault)
        {
            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
            }

            var selectExpression = (SelectExpression)source.QueryExpression;

            selectExpression.ApplyLimit(
                new SqlConstantExpression(
                    Expression.Constant(1),
                    _typeMappingSource.FindMapping(typeof(int))));

            return source;
        }

        protected override ShapedQueryExpression TranslateGroupBy(ShapedQueryExpression source, LambdaExpression keySelector, LambdaExpression elementSelector, LambdaExpression resultSelector) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateGroupJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateIntersect(ShapedQueryExpression source1, ShapedQueryExpression source2) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateLastOrDefault(ShapedQueryExpression source, LambdaExpression predicate, bool returnDefault)
        {
            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
            }

            var selectExpression = (SelectExpression)source.QueryExpression;

            if (selectExpression.Limit != null
                || selectExpression.Offset != null)
            {
                selectExpression.PushdownIntoSubQuery();
            }

            selectExpression.Reverse();

            selectExpression.ApplyLimit(
                new SqlConstantExpression(
                    Expression.Constant(1),
                    _typeMappingSource.FindMapping(typeof(int))));

            return source;
        }

        protected override ShapedQueryExpression TranslateLongCount(ShapedQueryExpression source, LambdaExpression predicate) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateMax(ShapedQueryExpression source, LambdaExpression selector, Type resultType)
        {
            if (selector != null)
            {
                source = TranslateSelect(source, selector);
            }

            var projection = (SqlExpression)((SelectExpression)source.QueryExpression)
                .GetProjectionExpression(new ProjectionMember());

            projection = new SqlFunctionExpression(
                "MAX",
                new[]
                {
                    projection
                },
                resultType,
                projection.TypeMapping,
                false);

            return AggregateResultShaper(source, projection, throwOnNullResult: true, resultType);
        }

        protected override ShapedQueryExpression TranslateMin(ShapedQueryExpression source, LambdaExpression selector, Type resultType)
        {
            if (selector != null)
            {
                source = TranslateSelect(source, selector);
            }

            var projection = (SqlExpression)((SelectExpression)source.QueryExpression)
                .GetProjectionExpression(new ProjectionMember());

            projection = new SqlFunctionExpression(
                "MIN",
                new[]
                {
                    projection
                },
                resultType,
                projection.TypeMapping,
                false);

            return AggregateResultShaper(source, projection, throwOnNullResult: true, resultType);
        }

        protected override ShapedQueryExpression TranslateOfType(ShapedQueryExpression source, Type resultType) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateOrderBy(ShapedQueryExpression source, LambdaExpression keySelector, bool ascending)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            if (selectExpression.IsDistinct)
            {
                selectExpression.PushdownIntoSubQuery();
            }

            var translation = TranslateLambdaExpression(source, keySelector, false);

            if (translation != null)
            {
                selectExpression.ApplyOrderBy(new OrderingExpression(translation, ascending));

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

            var parameterBindings = new Dictionary<Expression, Expression>
            {
                { selector.Parameters.Single(), source.ShaperExpression.Body }
            };

            var newSelectorBody = new ReplacingExpressionVisitor(parameterBindings).Visit(selector.Body);

            newSelectorBody = _projectionBindingExpressionVisitor
                .Translate((SelectExpression)source.QueryExpression, newSelectorBody);

            source.ShaperExpression = Expression.Lambda(newSelectorBody, source.ShaperExpression.Parameters);

            return source;
        }

        protected override ShapedQueryExpression TranslateSelectMany(ShapedQueryExpression source, LambdaExpression collectionSelector, LambdaExpression resultSelector) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateSelectMany(ShapedQueryExpression source, LambdaExpression selector) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateSingleOrDefault(ShapedQueryExpression source, LambdaExpression predicate, bool returnDefault)
        {
            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
            }

            var selectExpression = (SelectExpression)source.QueryExpression;

            selectExpression.ApplyLimit(
                new SqlConstantExpression(
                    Expression.Constant(1),
                    _typeMappingSource.FindMapping(typeof(int))));

            return source;
        }

        protected override ShapedQueryExpression TranslateSkip(ShapedQueryExpression source, Expression count)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            var translation = TranslateExpression(selectExpression, count, false);

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
            if (selector != null)
            {
                source = TranslateSelect(source, selector);
            }

            var serverOutputType = resultType.UnwrapNullableType();
            var projection = (SqlExpression)((SelectExpression)source.QueryExpression)
                .GetProjectionExpression(new ProjectionMember());

            if (serverOutputType == typeof(float))
            {
                projection = new SqlCastExpression(
                    new SqlFunctionExpression(
                        "SUM",
                        new[]
                        {
                            projection
                        },
                        typeof(double),
                        _typeMappingSource.FindMapping(typeof(double)),
                        false),
                    serverOutputType,
                    projection.TypeMapping);
            }
            else
            {
                projection = new SqlFunctionExpression(
                    "SUM",
                    new[]
                    {
                        projection
                    },
                    serverOutputType,
                    projection.TypeMapping,
                    false);
            }

            return AggregateResultShaper(source, projection, throwOnNullResult: false, resultType);
        }

        protected override ShapedQueryExpression TranslateTake(ShapedQueryExpression source, Expression count)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            var translation = TranslateExpression(selectExpression, count, false);

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
            var translation = TranslateLambdaExpression(source, keySelector, false);

            if (translation != null)
            {
                ((SelectExpression)source.QueryExpression).ApplyThenBy(new OrderingExpression(translation, ascending));

                return source;
            }

            throw new InvalidOperationException();
        }

        protected override ShapedQueryExpression TranslateUnion(ShapedQueryExpression source1, ShapedQueryExpression source2) => throw new NotImplementedException();

        protected override ShapedQueryExpression TranslateWhere(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var translation = TranslateLambdaExpression(source, predicate, true);

            if (translation != null)
            {
                ((SelectExpression)source.QueryExpression).ApplyPredicate(translation);

                return source;
            }

            throw new InvalidOperationException();
        }

        private SqlExpression TranslateExpression(SelectExpression selectExpression, Expression expression, bool condition)
        {
            return _sqlTranslator.Translate(selectExpression, expression)?.ConvertToValue(!condition);
        }

        private SqlExpression TranslateLambdaExpression(
            ShapedQueryExpression shapedQueryExpression, LambdaExpression lambdaExpression, bool condition)
        {
            var parameterBindings = new Dictionary<Expression, Expression>
            {
                { lambdaExpression.Parameters.Single(), shapedQueryExpression.ShaperExpression.Body }
            };

            var lambdaBody = new ReplacingExpressionVisitor(parameterBindings).Visit(lambdaExpression.Body);

            return TranslateExpression((SelectExpression)shapedQueryExpression.QueryExpression, lambdaBody, condition);
        }

        private ShapedQueryExpression AggregateResultShaper(
            ShapedQueryExpression source, Expression projection, bool throwOnNullResult, Type resultType)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            selectExpression.ApplyProjection(
                new Dictionary<ProjectionMember, Expression>
                {
                    { new ProjectionMember(), projection }
                });

            selectExpression.ClearOrdering();

            Expression shaper = new ProjectionBindingExpression(selectExpression, new ProjectionMember(), projection.Type);

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
                            projection.Type),
                        resultType != resultVariable.Type
                            ? Expression.Convert(resultVariable, resultType)
                            : (Expression)resultVariable));
            }
            else if (resultType.IsNullableType())
            {
                shaper = Expression.Convert(shaper, resultType);
            }

            source.ShaperExpression
                = Expression.Lambda(
                    shaper,
                    source.ShaperExpression.Parameters);

            return source;
        }
    }
}
