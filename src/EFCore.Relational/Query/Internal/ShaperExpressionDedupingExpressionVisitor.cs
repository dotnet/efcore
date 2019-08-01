// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class ShaperExpressionProcessingExpressionVisitor : ExpressionVisitor
    {
        private readonly SelectExpression _selectExpression;
        private readonly ParameterExpression _dataReaderParameter;
        private readonly ParameterExpression _resultCoordinatorParameter;
        private readonly ParameterExpression _indexMapParameter;
        private readonly IDictionary<Expression, ParameterExpression> _mapping = new Dictionary<Expression, ParameterExpression>();
        private readonly List<ParameterExpression> _variables = new List<ParameterExpression>();
        private readonly List<Expression> _expressions = new List<Expression>();
        private readonly List<CollectionPopulatingExpression> _collectionPopulatingExpressions = new List<CollectionPopulatingExpression>();

        public ShaperExpressionProcessingExpressionVisitor(
            SelectExpression selectExpression,
            ParameterExpression dataReaderParameter,
            ParameterExpression resultCoordinatorParameter,
            ParameterExpression indexMapParameter)
        {
            _selectExpression = selectExpression;
            _dataReaderParameter = dataReaderParameter;
            _resultCoordinatorParameter = resultCoordinatorParameter;
            _indexMapParameter = indexMapParameter;
        }

        public virtual Expression Inject(Expression expression)
        {
            var result = Visit(expression);

            if (_collectionPopulatingExpressions.Count > 0)
            {
                _expressions.Add(result);
                result = Expression.Block(_variables, _expressions);
                _expressions.Clear();
                _variables.Clear();

                var resultParameter = Expression.Parameter(result.Type, "result");

                _expressions.Add(
                    Expression.IfThen(
                        Expression.Equal(resultParameter, Expression.Default(result.Type)),
                        Expression.Assign(resultParameter, result)));
                _expressions.AddRange(_collectionPopulatingExpressions);
                _expressions.Add(resultParameter);

                return ConvertToLambda(Expression.Block(_expressions), resultParameter);
            }
            else if (_expressions.All(e => e.NodeType == ExpressionType.Assign))
            {
                result = new ReplacingExpressionVisitor(_expressions.Cast<BinaryExpression>()
                    .ToDictionary(e => e.Left, e => e.Right)).Visit(result);
            }
            else
            {
                _expressions.Add(result);
                result = Expression.Block(_variables, _expressions);
            }

            return ConvertToLambda(result, Expression.Parameter(result.Type, "result"));
        }

        private LambdaExpression ConvertToLambda(Expression result, ParameterExpression resultParameter)
            => _indexMapParameter != null
                ? Expression.Lambda(
                    result,
                    QueryCompilationContext.QueryContextParameter,
                    _dataReaderParameter,
                    resultParameter,
                    _indexMapParameter,
                    _resultCoordinatorParameter)
                : Expression.Lambda(
                    result,
                    QueryCompilationContext.QueryContextParameter,
                    _dataReaderParameter,
                    resultParameter,
                    _resultCoordinatorParameter);

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case EntityShaperExpression entityShaperExpression:
                    {
                        var key = GenerateKey((ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression);
                        if (!_mapping.TryGetValue(key, out var variable))
                        {
                            variable = Expression.Parameter(entityShaperExpression.EntityType.ClrType);
                            _variables.Add(variable);
                            _expressions.Add(Expression.Assign(variable, entityShaperExpression));
                            _mapping[key] = variable;
                        }

                        return variable;
                    }

                case ProjectionBindingExpression projectionBindingExpression:
                    {
                        var key = GenerateKey(projectionBindingExpression);
                        if (!_mapping.TryGetValue(key, out var variable))
                        {
                            variable = Expression.Parameter(projectionBindingExpression.Type);
                            _variables.Add(variable);
                            _expressions.Add(Expression.Assign(variable, projectionBindingExpression));
                            _mapping[key] = variable;
                        }

                        return variable;
                    }

                case IncludeExpression includeExpression:
                    {
                        var entity = Visit(includeExpression.EntityExpression);
                        if (includeExpression.NavigationExpression is RelationalCollectionShaperExpression
                            relationalCollectionShaperExpression)
                        {
                            var innerShaper = new ShaperExpressionProcessingExpressionVisitor(
                                _selectExpression, _dataReaderParameter, _resultCoordinatorParameter, null)
                                        .Inject(relationalCollectionShaperExpression.InnerShaper);

                            _collectionPopulatingExpressions.Add(new CollectionPopulatingExpression(
                                    relationalCollectionShaperExpression.Update(
                                        relationalCollectionShaperExpression.ParentIdentifier,
                                        relationalCollectionShaperExpression.OuterIdentifier,
                                        relationalCollectionShaperExpression.SelfIdentifier,
                                        innerShaper),
                                    includeExpression.Navigation.ClrType,
                                    true));

                            _expressions.Add(new CollectionInitializingExpression(
                                relationalCollectionShaperExpression.CollectionId,
                                entity,
                                relationalCollectionShaperExpression.ParentIdentifier,
                                relationalCollectionShaperExpression.OuterIdentifier,
                                includeExpression.Navigation,
                                includeExpression.Navigation.ClrType));
                        }
                        else
                        {
                            _expressions.Add(includeExpression.Update(
                                entity,
                                Visit(includeExpression.NavigationExpression)));
                        }

                        return entity;
                    }

                case RelationalCollectionShaperExpression relationalCollectionShaperExpression2:
                    {
                        var innerShaper = new ShaperExpressionProcessingExpressionVisitor(
                            _selectExpression, _dataReaderParameter, _resultCoordinatorParameter, null)
                                .Inject(relationalCollectionShaperExpression2.InnerShaper);

                        _collectionPopulatingExpressions.Add(new CollectionPopulatingExpression(
                                relationalCollectionShaperExpression2.Update(
                                    relationalCollectionShaperExpression2.ParentIdentifier,
                                    relationalCollectionShaperExpression2.OuterIdentifier,
                                    relationalCollectionShaperExpression2.SelfIdentifier,
                                    innerShaper),
                                relationalCollectionShaperExpression2.Type,
                                false));

                        return new CollectionInitializingExpression(
                            relationalCollectionShaperExpression2.CollectionId,
                            null,
                            relationalCollectionShaperExpression2.ParentIdentifier,
                            relationalCollectionShaperExpression2.OuterIdentifier,
                            relationalCollectionShaperExpression2.Navigation,
                            relationalCollectionShaperExpression2.Type);
                    }
            }

            return base.VisitExtension(extensionExpression);
        }

        private Expression GenerateKey(ProjectionBindingExpression projectionBindingExpression)
            => projectionBindingExpression.ProjectionMember != null
                ? _selectExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember)
                : projectionBindingExpression;
    }
}
