// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public class CosmosQueryableMethodTranslatingExpressionVisitor : QueryableMethodTranslatingExpressionVisitor
    {
        private readonly IModel _model;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly CosmosSqlTranslatingExpressionVisitor _sqlTranslator;
        private readonly CosmosProjectionBindingExpressionVisitor _projectionBindingExpressionVisitor;

        public CosmosQueryableMethodTranslatingExpressionVisitor(
            IModel model,
            ISqlExpressionFactory sqlExpressionFactory,
            IMemberTranslatorProvider memberTranslatorProvider,
            IMethodCallTranslatorProvider methodCallTranslatorProvider)
            : base(subquery: false)
        {
            _model = model;
            _sqlExpressionFactory = sqlExpressionFactory;
            _sqlTranslator = new CosmosSqlTranslatingExpressionVisitor(
                model,
                sqlExpressionFactory,
                memberTranslatorProvider,
                methodCallTranslatorProvider);
            _projectionBindingExpressionVisitor = new CosmosProjectionBindingExpressionVisitor(_sqlTranslator);
        }

        public override ShapedQueryExpression TranslateSubquery(Expression expression)
        {
            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression CreateShapedQueryExpression(Type elementType)
        {
            var entityType = _model.FindEntityType(elementType);
            var selectExpression = _sqlExpressionFactory.Select(entityType);

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
            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateAny(ShapedQueryExpression source, LambdaExpression predicate)
        {
            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateAverage(ShapedQueryExpression source, LambdaExpression selector, Type resultType)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            if (selectExpression.IsDistinct
                || selectExpression.Limit != null
                || selectExpression.Offset != null)
            {
                throw new InvalidOperationException();
            }

            if (selector != null)
            {
                source = TranslateSelect(source, selector);
            }

            var projection = (SqlExpression)selectExpression.GetMappedProjection(new ProjectionMember());
            projection = _sqlExpressionFactory.Function(
                "AVG", new[] { projection }, projection.Type, projection.TypeMapping);

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
            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateContains(ShapedQueryExpression source, Expression item)
        {
            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateCount(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            if (selectExpression.IsDistinct
                || selectExpression.Limit != null
                || selectExpression.Offset != null)
            {
                throw new InvalidOperationException();
            }

            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
            }

            var translation = _sqlExpressionFactory.ApplyDefaultTypeMapping(
                _sqlExpressionFactory.Function("COUNT", new[] { _sqlExpressionFactory.Constant(1) }, typeof(int)));

            var projectionMapping = new Dictionary<ProjectionMember, Expression>
            {
                { new ProjectionMember(), translation }
            };

            selectExpression.ClearOrdering();
            selectExpression.ReplaceProjectionMapping(projectionMapping);
            source.ShaperExpression = new ProjectionBindingExpression(source.QueryExpression, new ProjectionMember(), typeof(int));

            return source;
        }

        protected override ShapedQueryExpression TranslateDefaultIfEmpty(ShapedQueryExpression source, Expression defaultValue)
        {
            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateDistinct(ShapedQueryExpression source)
        {
            ((SelectExpression)source.QueryExpression).ApplyDistinct();

            return source;
        }

        protected override ShapedQueryExpression TranslateElementAtOrDefault(ShapedQueryExpression source, Expression index, bool returnDefault)
        {
            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateExcept(ShapedQueryExpression source1, ShapedQueryExpression source2)
        {
            throw new NotImplementedException();
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

        protected override ShapedQueryExpression TranslateGroupBy(ShapedQueryExpression source, LambdaExpression keySelector, LambdaExpression elementSelector, LambdaExpression resultSelector)
        {
            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateGroupJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector)
        {
            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateIntersect(ShapedQueryExpression source1, ShapedQueryExpression source2)
        {
            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector)
        {
            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateLastOrDefault(ShapedQueryExpression source, LambdaExpression predicate, Type returnType, bool returnDefault)
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

        protected override ShapedQueryExpression TranslateLeftJoin(ShapedQueryExpression outer, ShapedQueryExpression inner, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector)
        {
            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateLongCount(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            if (selectExpression.IsDistinct
                || selectExpression.Limit != null
                || selectExpression.Offset != null)
            {
                throw new InvalidOperationException();
            }

            if (predicate != null)
            {
                source = TranslateWhere(source, predicate);
            }

            var translation = _sqlExpressionFactory.ApplyDefaultTypeMapping(
                _sqlExpressionFactory.Function("COUNT", new[] { _sqlExpressionFactory.Constant(1) }, typeof(long)));
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
            if (selectExpression.IsDistinct
                || selectExpression.Limit != null
                || selectExpression.Offset != null)
            {
                throw new InvalidOperationException();
            }

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
            if (selectExpression.IsDistinct
                || selectExpression.Limit != null
                || selectExpression.Offset != null)
            {
                throw new InvalidOperationException();
            }

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

        protected override ShapedQueryExpression TranslateReverse(ShapedQueryExpression source)
        {
            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateSelect(ShapedQueryExpression source, LambdaExpression selector)
        {
            if (selector.Body == selector.Parameters[0])
            {
                return source;
            }

            var selectExpression = (SelectExpression)source.QueryExpression;
            if (selectExpression.IsDistinct)
            {
                throw new InvalidOperationException();
            }

            var newSelectorBody = ReplacingExpressionVisitor.Replace(selector.Parameters.Single(), source.ShaperExpression, selector.Body);

            source.ShaperExpression = _projectionBindingExpressionVisitor
                .Translate(selectExpression, newSelectorBody);

            return source;
        }

        protected override ShapedQueryExpression TranslateSelectMany(ShapedQueryExpression source, LambdaExpression collectionSelector, LambdaExpression resultSelector)
        {
            throw new NotImplementedException();
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

        protected override ShapedQueryExpression TranslateSkipWhile(ShapedQueryExpression source, LambdaExpression predicate)
        {
            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateSum(ShapedQueryExpression source, LambdaExpression selector, Type resultType)
        {
            var selectExpression = (SelectExpression)source.QueryExpression;
            if (selectExpression.IsDistinct
                || selectExpression.Limit != null
                || selectExpression.Offset != null)
            {
                throw new InvalidOperationException();
            }

            if (selector != null)
            {
                source = TranslateSelect(source, selector);
            }

            var serverOutputType = resultType.UnwrapNullableType();
            var projection = (SqlExpression)selectExpression.GetMappedProjection(new ProjectionMember());

            projection = _sqlExpressionFactory.Function(
                "SUM", new[] { projection }, serverOutputType, projection.TypeMapping);

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

        protected override ShapedQueryExpression TranslateTakeWhile(ShapedQueryExpression source, LambdaExpression predicate)
        {
            throw new NotImplementedException();
        }

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
            throw new NotImplementedException();
        }

        protected override ShapedQueryExpression TranslateWhere(ShapedQueryExpression source, LambdaExpression predicate)
        {
            var translation = TranslateLambdaExpression(source, predicate);
            if (translation != null)
            {
                ((SelectExpression)source.QueryExpression).ApplyPredicate(translation);

                return source;
            }

            throw new InvalidOperationException("Unable to translate Where expression: " + new ExpressionPrinter().Print(predicate));
        }

        private SqlExpression TranslateExpression(Expression expression)
            => _sqlTranslator.Translate(expression);

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
                                // TODO: See issue#16164
                                Expression.Constant("Insert exception message here")),
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
