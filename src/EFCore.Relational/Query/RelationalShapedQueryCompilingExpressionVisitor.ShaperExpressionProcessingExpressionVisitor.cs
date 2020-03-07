// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class RelationalShapedQueryCompilingExpressionVisitor
    {
        private class ShaperExpressionProcessingExpressionVisitor : ExpressionVisitor
        {
            private static readonly MemberInfo _resultContextValuesMemberInfo
                = typeof(ResultContext).GetTypeInfo().GetMember(nameof(ResultContext.Values))[0];

            private static readonly MemberInfo _resultCoordinatorResultReadyMemberInfo
                = typeof(ResultCoordinator).GetTypeInfo().GetMember(nameof(ResultCoordinator.ResultReady))[0];

            private readonly SelectExpression _selectExpression;
            private readonly ParameterExpression _dataReaderParameter;
            private readonly ParameterExpression _resultContextParameter;
            private readonly ParameterExpression _resultCoordinatorParameter;
            private readonly ParameterExpression _indexMapParameter;

            private readonly IDictionary<Expression, Expression> _mapping = new Dictionary<Expression, Expression>();

            // There are always entity variables to avoid materializing same entity twice
            private readonly List<ParameterExpression> _variables = new List<ParameterExpression>();
            private readonly List<Expression> _expressions = new List<Expression>();

            // IncludeExpressions are added at the end in case they are using ValuesArray
            private readonly List<Expression> _includeExpressions = new List<Expression>();

            // If there is collection shaper then we need to construct ValuesArray to store values temporarily in ResultContext
            private List<CollectionPopulatingExpression> _collectionPopulatingExpressions;
            private Expression _valuesArrayExpression;
            private List<Expression> _valuesArrayInitializers;

            private bool _containsCollectionMaterialization;

            public ShaperExpressionProcessingExpressionVisitor(
                SelectExpression selectExpression,
                ParameterExpression dataReaderParameter,
                ParameterExpression resultCoordinatorParameter,
                ParameterExpression indexMapParameter)
            {
                _selectExpression = selectExpression;
                _dataReaderParameter = dataReaderParameter;
                _resultContextParameter = Expression.Parameter(typeof(ResultContext), "resultContext");
                _resultCoordinatorParameter = resultCoordinatorParameter;
                _indexMapParameter = indexMapParameter;
            }

            private class CollectionShaperFindingExpressionVisitor : ExpressionVisitor
            {
                private bool _containsCollection;

                public bool ContainsCollectionMaterialization(Expression expression)
                {
                    _containsCollection = false;

                    Visit(expression);

                    return _containsCollection;
                }

                public override Expression Visit(Expression expression)
                {
                    if (_containsCollection)
                    {
                        return expression;
                    }

                    if (expression is RelationalCollectionShaperExpression)
                    {
                        _containsCollection = true;

                        return expression;
                    }

                    return base.Visit(expression);
                }
            }

            public Expression Inject(Expression expression)
            {
                _containsCollectionMaterialization = new CollectionShaperFindingExpressionVisitor()
                    .ContainsCollectionMaterialization(expression);

                if (_containsCollectionMaterialization)
                {
                    _valuesArrayExpression = Expression.MakeMemberAccess(_resultContextParameter, _resultContextValuesMemberInfo);
                    _collectionPopulatingExpressions = new List<CollectionPopulatingExpression>();
                    _valuesArrayInitializers = new List<Expression>();
                }

                var result = Visit(expression);

                if (_containsCollectionMaterialization)
                {
                    var valueArrayInitializationExpression = Expression.Assign(
                        _valuesArrayExpression,
                        Expression.NewArrayInit(
                            typeof(object),
                            _valuesArrayInitializers));

                    _expressions.Add(valueArrayInitializationExpression);
                    _expressions.AddRange(_includeExpressions);

                    var initializationBlock = Expression.Block(
                        _variables,
                        _expressions);

                    var conditionalMaterializationExpressions = new List<Expression>();

                    conditionalMaterializationExpressions.Add(
                        Expression.IfThen(
                            Expression.Equal(_valuesArrayExpression, Expression.Constant(null, typeof(object[]))),
                            initializationBlock));

                    conditionalMaterializationExpressions.AddRange(_collectionPopulatingExpressions);

                    conditionalMaterializationExpressions.Add(
                        Expression.Condition(
                            Expression.IsTrue(
                                Expression.MakeMemberAccess(
                                    _resultCoordinatorParameter, _resultCoordinatorResultReadyMemberInfo)),
                            result,
                            Expression.Default(result.Type)));

                    result = Expression.Block(conditionalMaterializationExpressions);
                }
                else
                {
                    _expressions.AddRange(_includeExpressions);
                    _expressions.Add(result);
                    result = Expression.Block(_variables, _expressions);
                }

                return ConvertToLambda(result);
            }

            private LambdaExpression ConvertToLambda(Expression result)
                => _indexMapParameter != null
                    ? Expression.Lambda(
                        result,
                        QueryCompilationContext.QueryContextParameter,
                        _dataReaderParameter,
                        _resultContextParameter,
                        _indexMapParameter,
                        _resultCoordinatorParameter)
                    : Expression.Lambda(
                        result,
                        QueryCompilationContext.QueryContextParameter,
                        _dataReaderParameter,
                        _resultContextParameter,
                        _resultCoordinatorParameter);

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                switch (extensionExpression)
                {
                    case EntityShaperExpression entityShaperExpression:
                    {
                        var key = GenerateKey((ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression);
                        if (!_mapping.TryGetValue(key, out var accessor))
                        {
                            var entityParameter = Expression.Parameter(entityShaperExpression.Type);
                            _variables.Add(entityParameter);
                            _expressions.Add(Expression.Assign(entityParameter, entityShaperExpression));

                            if (_containsCollectionMaterialization)
                            {
                                _valuesArrayInitializers.Add(entityParameter);
                                accessor = Expression.Convert(
                                    Expression.ArrayIndex(
                                        _valuesArrayExpression,
                                        Expression.Constant(_valuesArrayInitializers.Count - 1)),
                                    entityShaperExpression.Type);
                            }
                            else
                            {
                                accessor = entityParameter;
                            }

                            _mapping[key] = accessor;
                        }

                        return accessor;
                    }

                    case ProjectionBindingExpression projectionBindingExpression:
                    {
                        var key = GenerateKey(projectionBindingExpression);
                        if (_mapping.TryGetValue(key, out var accessor))
                        {
                            return accessor;
                        }

                        var valueParameter = Expression.Parameter(projectionBindingExpression.Type);
                        _variables.Add(valueParameter);
                        _expressions.Add(Expression.Assign(valueParameter, projectionBindingExpression));

                        if (_containsCollectionMaterialization)
                        {
                            var expressionToAdd = (Expression)valueParameter;
                            if (expressionToAdd.Type.IsValueType)
                            {
                                expressionToAdd = Expression.Convert(expressionToAdd, typeof(object));
                            }

                            _valuesArrayInitializers.Add(expressionToAdd);
                            accessor = Expression.Convert(
                                Expression.ArrayIndex(
                                    _valuesArrayExpression,
                                    Expression.Constant(_valuesArrayInitializers.Count - 1)),
                                projectionBindingExpression.Type);
                        }
                        else
                        {
                            accessor = valueParameter;
                        }

                        _mapping[key] = accessor;

                        return accessor;
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

                            _collectionPopulatingExpressions.Add(
                                new CollectionPopulatingExpression(
                                    relationalCollectionShaperExpression.Update(
                                        relationalCollectionShaperExpression.ParentIdentifier,
                                        relationalCollectionShaperExpression.OuterIdentifier,
                                        relationalCollectionShaperExpression.SelfIdentifier,
                                        innerShaper),
                                    includeExpression.Navigation.ClrType,
                                    true));

                            _includeExpressions.Add(
                                new CollectionInitializingExpression(
                                    relationalCollectionShaperExpression.CollectionId,
                                    entity,
                                    relationalCollectionShaperExpression.ParentIdentifier,
                                    relationalCollectionShaperExpression.OuterIdentifier,
                                    includeExpression.Navigation,
                                    includeExpression.Navigation.ClrType));
                        }
                        else
                        {
                            _includeExpressions.Add(
                                includeExpression.Update(
                                    entity,
                                    Visit(includeExpression.NavigationExpression)));
                        }

                        return entity;
                    }

                    case RelationalCollectionShaperExpression relationalCollectionShaperExpression:
                    {
                        var key = GenerateKey(relationalCollectionShaperExpression);
                        if (!_mapping.TryGetValue(key, out var accessor))
                        {
                            var innerShaper = new ShaperExpressionProcessingExpressionVisitor(
                                    _selectExpression, _dataReaderParameter, _resultCoordinatorParameter, null)
                                .Inject(relationalCollectionShaperExpression.InnerShaper);

                            _collectionPopulatingExpressions.Add(
                                new CollectionPopulatingExpression(
                                    relationalCollectionShaperExpression.Update(
                                        relationalCollectionShaperExpression.ParentIdentifier,
                                        relationalCollectionShaperExpression.OuterIdentifier,
                                        relationalCollectionShaperExpression.SelfIdentifier,
                                        innerShaper),
                                    relationalCollectionShaperExpression.Type,
                                    false));

                            var collectionParameter = Expression.Parameter(relationalCollectionShaperExpression.Type);
                            _variables.Add(collectionParameter);
                            _expressions.Add(
                                Expression.Assign(
                                    collectionParameter,
                                    new CollectionInitializingExpression(
                                        relationalCollectionShaperExpression.CollectionId,
                                        null,
                                        relationalCollectionShaperExpression.ParentIdentifier,
                                        relationalCollectionShaperExpression.OuterIdentifier,
                                        relationalCollectionShaperExpression.Navigation,
                                        relationalCollectionShaperExpression.Type)));

                            _valuesArrayInitializers.Add(collectionParameter);
                            accessor = Expression.Convert(
                                Expression.ArrayIndex(
                                    _valuesArrayExpression,
                                    Expression.Constant(_valuesArrayInitializers.Count - 1)),
                                relationalCollectionShaperExpression.Type);

                            _mapping[key] = accessor;
                        }

                        return accessor;
                    }
                }

                return base.VisitExtension(extensionExpression);
            }

            private Expression GenerateKey(Expression expression)
                => expression is ProjectionBindingExpression projectionBindingExpression
                    && projectionBindingExpression.ProjectionMember != null
                        ? _selectExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember)
                        : expression;
        }
    }
}
