// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public partial class IncludeCompiler
    {
        private sealed class CollectionQueryModelRewritingExpressionVisitor : RelinqExpressionVisitor
        {
            private static readonly ExpressionEqualityComparer _expressionEqualityComparer
                = new ExpressionEqualityComparer();

            private readonly QueryCompilationContext _queryCompilationContext;
            private readonly QueryModel _parentQueryModel;
            private readonly IncludeCompiler _includeCompiler;

            public CollectionQueryModelRewritingExpressionVisitor(
                QueryCompilationContext queryCompilationContext,
                QueryModel parentQueryModel,
                IncludeCompiler includeCompiler)
            {
                _queryCompilationContext = queryCompilationContext;
                _parentQueryModel = parentQueryModel;
                _includeCompiler = includeCompiler;
            }

            public List<Ordering> ParentOrderings { get; } = new List<Ordering>();

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (typeof(IQueryBuffer).GetTypeInfo()
                        .IsAssignableFrom(methodCallExpression.Object?.Type.GetTypeInfo())
                    && methodCallExpression.Method.Name
                        .StartsWith(nameof(IQueryBuffer.IncludeCollection), StringComparison.Ordinal)
                    && (int)((ConstantExpression)methodCallExpression.Arguments[0]).Value != -1) // -1 == unable to optimize (GJ)
                {
                    var lambaArgument = methodCallExpression.Arguments[8];
                    var convertExpression = lambaArgument as UnaryExpression;

                    var subQueryExpression
                        = (SubQueryExpression)
                        ((LambdaExpression)(convertExpression?.Operand ?? lambaArgument))
                        .Body.RemoveConvert();

                    var navigation
                        = (INavigation)
                        ((ConstantExpression)methodCallExpression.Arguments[1])
                        .Value;

                    Rewrite(subQueryExpression.QueryModel, navigation);

                    _includeCompiler.RewriteCollectionQueries(subQueryExpression.QueryModel);

                    var newArguments = methodCallExpression.Arguments.ToArray();

                    Expression newLambdaExpression = Expression.Lambda(subQueryExpression);

                    if (convertExpression != null)
                    {
                        newLambdaExpression = convertExpression.Update(newLambdaExpression);
                    }

                    newArguments[8] = newLambdaExpression;

                    return methodCallExpression.Update(methodCallExpression.Object, newArguments);
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            private void Rewrite(QueryModel collectionQueryModel, INavigation navigation)
            {
                var querySourceReferenceFindingExpressionTreeVisitor
                    = new QuerySourceReferenceFindingExpressionTreeVisitor();

                var whereClause = collectionQueryModel.BodyClauses
                    .OfType<WhereClause>()
                    .Single();

                whereClause.TransformExpressions(querySourceReferenceFindingExpressionTreeVisitor.Visit);

                collectionQueryModel.BodyClauses.Remove(whereClause);

                var parentQuerySourceReferenceExpression
                    = querySourceReferenceFindingExpressionTreeVisitor.QuerySourceReferenceExpression;

                var parentQuerySource = parentQuerySourceReferenceExpression.ReferencedQuerySource;

                BuildParentOrderings(
                    _parentQueryModel,
                    navigation,
                    parentQuerySourceReferenceExpression,
                    ParentOrderings);

                var querySourceMapping = new QuerySourceMapping();
                var clonedParentQueryModel = _parentQueryModel.Clone(querySourceMapping);
                _queryCompilationContext.UpdateMapping(querySourceMapping);

                _queryCompilationContext.CloneAnnotations(querySourceMapping, clonedParentQueryModel);

                var clonedParentQuerySourceReferenceExpression
                    = (QuerySourceReferenceExpression)querySourceMapping.GetExpression(parentQuerySource);

                var clonedParentQuerySource
                    = clonedParentQuerySourceReferenceExpression.ReferencedQuerySource;

                AdjustPredicate(
                    clonedParentQueryModel,
                    clonedParentQuerySource,
                    clonedParentQuerySourceReferenceExpression);

                clonedParentQueryModel.SelectClause
                    = new SelectClause(Expression.Default(typeof(AnonymousObject)));

                var subQueryProjection = new List<Expression>();

                var lastResultOperator = ProcessResultOperators(clonedParentQueryModel);

                clonedParentQueryModel.ResultTypeOverride
                    = typeof(IQueryable<>).MakeGenericType(clonedParentQueryModel.SelectClause.Selector.Type);

                var parentItemName
                    = parentQuerySource.HasGeneratedItemName()
                        ? navigation.DeclaringEntityType.DisplayName()[0].ToString().ToLowerInvariant()
                        : parentQuerySource.ItemName;

                collectionQueryModel.MainFromClause.ItemName = $"{parentItemName}.{navigation.Name}";

                var collectionQuerySourceReferenceExpression
                    = new QuerySourceReferenceExpression(collectionQueryModel.MainFromClause);

                var joinQuerySourceReferenceExpression
                    = CreateJoinToParentQuery(
                        clonedParentQueryModel,
                        clonedParentQuerySourceReferenceExpression,
                        collectionQuerySourceReferenceExpression,
                        navigation.ForeignKey,
                        collectionQueryModel,
                        subQueryProjection);

                ApplyParentOrderings(
                    ParentOrderings,
                    clonedParentQueryModel,
                    querySourceMapping,
                    lastResultOperator);

                LiftOrderBy(
                    clonedParentQuerySource,
                    joinQuerySourceReferenceExpression,
                    clonedParentQueryModel,
                    collectionQueryModel,
                    subQueryProjection);

                clonedParentQueryModel.SelectClause.Selector
                    = Expression.New(
                        AnonymousObject.AnonymousObjectCtor,
                        Expression.NewArrayInit(
                            typeof(object),
                            subQueryProjection));
            }

            private static void BuildParentOrderings(
                QueryModel queryModel,
                INavigation navigation,
                QuerySourceReferenceExpression querySourceReferenceExpression,
                ICollection<Ordering> parentOrderings)
            {
                var orderings = parentOrderings;

                var orderByClause
                    = queryModel.BodyClauses.OfType<OrderByClause>().LastOrDefault();

                if (orderByClause != null)
                {
                    orderings = orderings.Concat(orderByClause.Orderings).ToArray();
                }

                foreach (var property in navigation.ForeignKey.PrincipalKey.Properties)
                {
                    var propertyExpression = querySourceReferenceExpression.CreateEFPropertyExpression(property);

                    var orderingExpression = Expression.Convert(
                        new NullConditionalExpression(
                            querySourceReferenceExpression,
                            propertyExpression),
                        propertyExpression.Type);

                    if (!orderings.Any(
                        o => _expressionEqualityComparer.Equals(o.Expression, orderingExpression)
                             || (o.Expression.RemoveConvert() is MemberExpression memberExpression1
                                 && propertyExpression is MethodCallExpression methodCallExpression
                                 && MatchEfPropertyToMemberExpression(memberExpression1, methodCallExpression))
                             || (o.Expression.RemoveConvert() is NullConditionalExpression nullConditionalExpression
                                 && nullConditionalExpression.AccessOperation is MemberExpression memberExpression
                                 && propertyExpression is MethodCallExpression methodCallExpression1
                                 && MatchEfPropertyToMemberExpression(memberExpression, methodCallExpression1))))
                    {
                        parentOrderings.Add(new Ordering(orderingExpression, OrderingDirection.Asc));
                    }
                }
            }

            private static bool MatchEfPropertyToMemberExpression(MemberExpression memberExpression, MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.IsEFProperty())
                {
                    var propertyName = (string)((ConstantExpression)methodCallExpression.Arguments[1]).Value;

                    return memberExpression.Member.Name.Equals(propertyName)
                           && _expressionEqualityComparer.Equals(memberExpression.Expression, methodCallExpression.Arguments[0]);
                }

                return false;
            }

            private static void AdjustPredicate(
                QueryModel queryModel,
                IQuerySource parentQuerySource,
                Expression targetParentExpression)
            {
                var querySourcePriorityAnalyzer
                    = new QuerySourcePriorityAnalyzer(queryModel.SelectClause.Selector);

                Expression predicate = null;

                if (querySourcePriorityAnalyzer.AreLowerPriorityQuerySources(parentQuerySource))
                {
                    predicate
                        = Expression.NotEqual(
                            targetParentExpression,
                            Expression.Constant(null, targetParentExpression.Type));
                }

                predicate
                    = querySourcePriorityAnalyzer.GetHigherPriorityQuerySources(parentQuerySource)
                        .Select(qs => new QuerySourceReferenceExpression(qs))
                        .Select(qsre => Expression.Equal(qsre, Expression.Constant(null, qsre.Type)))
                        .Aggregate(
                            predicate,
                            (current, nullCheck)
                                => current == null
                                    ? nullCheck
                                    : Expression.AndAlso(current, nullCheck));

                if (predicate != null)
                {
                    var whereClause = queryModel.BodyClauses.OfType<WhereClause>().LastOrDefault();

                    if (whereClause == null)
                    {
                        queryModel.BodyClauses.Add(new WhereClause(predicate));
                    }
                    else
                    {
                        whereClause.Predicate = Expression.AndAlso(whereClause.Predicate, predicate);
                    }
                }
            }

            private sealed class QuerySourcePriorityAnalyzer : RelinqExpressionVisitor
            {
                private readonly List<IQuerySource> _querySources = new List<IQuerySource>();

                public QuerySourcePriorityAnalyzer(Expression expression) => Visit(expression);

                public bool AreLowerPriorityQuerySources(IQuerySource querySource)
                {
                    var index = _querySources.IndexOf(querySource);

                    return index != -1 && index < _querySources.Count - 1;
                }

                public IEnumerable<IQuerySource> GetHigherPriorityQuerySources(IQuerySource querySource)
                {
                    var index = _querySources.IndexOf(querySource);

                    if (index != -1)
                    {
                        for (var i = 0; i < index; i++)
                        {
                            yield return _querySources[i];
                        }
                    }
                }

                protected override Expression VisitBinary(BinaryExpression node)
                {
                    IQuerySource querySource;

                    if (node.NodeType == ExpressionType.Coalesce
                        && (querySource = ExtractQuerySource(node.Left)) != null)
                    {
                        _querySources.Add(querySource);

                        if ((querySource = ExtractQuerySource(node.Right)) != null)
                        {
                            _querySources.Add(querySource);
                        }
                        else
                        {
                            Visit(node.Right);

                            return node;
                        }
                    }

                    return base.VisitBinary(node);
                }

                private static IQuerySource ExtractQuerySource(Expression expression)
                {
                    switch (expression)
                    {
                        case QuerySourceReferenceExpression querySourceReferenceExpression:
                            return querySourceReferenceExpression.ReferencedQuerySource;
                        case MethodCallExpression methodCallExpression
                        when IsIncludeMethod(methodCallExpression):
                            return ((QuerySourceReferenceExpression)methodCallExpression.Arguments[1]).ReferencedQuerySource;
                    }

                    return null;
                }
            }

            private static bool ProcessResultOperators(QueryModel queryModel)
            {
                var lastResultOperator = false;

                if (queryModel.ResultOperators.LastOrDefault() is ChoiceResultOperatorBase choiceResultOperator)
                {
                    queryModel.ResultOperators.Remove(choiceResultOperator);
                    queryModel.ResultOperators.Add(new TakeResultOperator(Expression.Constant(1)));

                    lastResultOperator = choiceResultOperator is LastResultOperator;
                }

                foreach (var groupResultOperator
                    in queryModel.ResultOperators.OfType<GroupResultOperator>()
                        .ToArray())
                {
                    queryModel.ResultOperators.Remove(groupResultOperator);

                    var orderByClause = queryModel.BodyClauses.OfType<OrderByClause>().LastOrDefault();

                    if (orderByClause == null)
                    {
                        queryModel.BodyClauses.Add(orderByClause = new OrderByClause());
                    }

                    orderByClause.Orderings.Add(new Ordering(groupResultOperator.KeySelector, OrderingDirection.Asc));
                }

                if (queryModel.BodyClauses
                        .Count(
                            bc => bc is AdditionalFromClause
                                  || bc is JoinClause
                                  || bc is GroupJoinClause) > 0)
                {
                    queryModel.ResultOperators.Add(new DistinctResultOperator());
                }

                return lastResultOperator;
            }

            private static QuerySourceReferenceExpression CreateJoinToParentQuery(
                QueryModel parentQueryModel,
                QuerySourceReferenceExpression parentQuerySourceReferenceExpression,
                Expression outerTargetExpression,
                IForeignKey foreignKey,
                QueryModel targetQueryModel,
                ICollection<Expression> subQueryProjection)
            {
                var subQueryExpression = new SubQueryExpression(parentQueryModel);
                var parentQuerySource = parentQuerySourceReferenceExpression.ReferencedQuerySource;

                var joinClause
                    = new JoinClause(
                        "_" + parentQuerySource.ItemName,
                        typeof(AnonymousObject),
                        subQueryExpression,
                        outerTargetExpression.CreateKeyAccessExpression(foreignKey.Properties),
                        Expression.Constant(null));

                var joinQuerySourceReferenceExpression = new QuerySourceReferenceExpression(joinClause);
                var innerKeyExpressions = new List<Expression>();

                foreach (var principalKeyProperty in foreignKey.PrincipalKey.Properties)
                {
                    innerKeyExpressions.Add(
                        Expression.Convert(
                            Expression.Call(
                                joinQuerySourceReferenceExpression,
                                AnonymousObject.GetValueMethodInfo,
                                Expression.Constant(subQueryProjection.Count)),
                            principalKeyProperty.ClrType.MakeNullable()));

                    var propertyExpression
                        = parentQuerySourceReferenceExpression.CreateEFPropertyExpression(principalKeyProperty);

                    subQueryProjection.Add(
                        Expression.Convert(
                            new NullConditionalExpression(
                                parentQuerySourceReferenceExpression,
                                propertyExpression),
                            typeof(object)));
                }

                joinClause.InnerKeySelector
                    = innerKeyExpressions.Count == 1
                        ? innerKeyExpressions[0]
                        : Expression.New(
                            AnonymousObject.AnonymousObjectCtor,
                            Expression.NewArrayInit(
                                typeof(object),
                                innerKeyExpressions.Select(e => Expression.Convert(e, typeof(object)))));

                targetQueryModel.BodyClauses.Add(joinClause);

                return joinQuerySourceReferenceExpression;
            }

            private static void ApplyParentOrderings(
                IEnumerable<Ordering> parentOrderings,
                QueryModel queryModel,
                QuerySourceMapping querySourceMapping,
                bool reverseOrdering)
            {
                var orderByClause = queryModel.BodyClauses.OfType<OrderByClause>().LastOrDefault();

                if (orderByClause == null)
                {
                    queryModel.BodyClauses.Add(orderByClause = new OrderByClause());
                }

                foreach (var ordering in parentOrderings)
                {
                    var newExpression
                        = CloningExpressionVisitor
                            .AdjustExpressionAfterCloning(ordering.Expression, querySourceMapping);

                    if (newExpression is MethodCallExpression methodCallExpression
                        && methodCallExpression.Method.IsEFPropertyMethod())
                    {
                        newExpression
                            = new NullConditionalExpression(
                                methodCallExpression.Arguments[0],
                                methodCallExpression);
                    }

                    orderByClause.Orderings
                        .Add(new Ordering(newExpression, ordering.OrderingDirection));
                }

                if (reverseOrdering)
                {
                    foreach (var ordering in orderByClause.Orderings)
                    {
                        ordering.OrderingDirection
                            = ordering.OrderingDirection == OrderingDirection.Asc
                                ? OrderingDirection.Desc
                                : OrderingDirection.Asc;
                    }
                }
            }

            private static void LiftOrderBy(
                IQuerySource querySource,
                Expression targetExpression,
                QueryModel fromQueryModel,
                QueryModel toQueryModel,
                List<Expression> subQueryProjection)
            {
                var canRemove
                    = !fromQueryModel.ResultOperators
                        .Any(r => r is SkipResultOperator || r is TakeResultOperator);

                foreach (var orderByClause
                    in fromQueryModel.BodyClauses.OfType<OrderByClause>().ToArray())
                {
                    var outerOrderByClause = new OrderByClause();

                    foreach (var ordering in orderByClause.Orderings)
                    {
                        int projectionIndex;
                        var orderingExpression = ordering.Expression;
                        if (ordering.Expression.RemoveConvert() is NullConditionalExpression nullConditionalExpression)
                        {
                            orderingExpression = nullConditionalExpression.AccessOperation;
                        }

                        if (orderingExpression.RemoveConvert() is MemberExpression memberExpression
                            && memberExpression.Expression is QuerySourceReferenceExpression memberQsre
                            && memberQsre.ReferencedQuerySource == querySource)
                        {
                            projectionIndex
                                = subQueryProjection
                                    .FindIndex(
                                        e =>
                                            {
                                                var expressionWithoutConvert = e.RemoveConvert();
                                                var projectionExpression = (expressionWithoutConvert as NullConditionalExpression)?.AccessOperation
                                                                           ?? expressionWithoutConvert;

                                                if (projectionExpression is MethodCallExpression methodCall
                                                    && methodCall.Method.IsEFPropertyMethod())
                                                {
                                                    var properyQsre = (QuerySourceReferenceExpression)methodCall.Arguments[0];
                                                    var propertyName = (string)((ConstantExpression)methodCall.Arguments[1]).Value;

                                                    return properyQsre.ReferencedQuerySource == memberQsre.ReferencedQuerySource
                                                           && propertyName == memberExpression.Member.Name;
                                                }

                                                if (projectionExpression is MemberExpression projectionMemberExpression)
                                                {
                                                    var projectionMemberQsre = (QuerySourceReferenceExpression)projectionMemberExpression.Expression;

                                                    return projectionMemberQsre.ReferencedQuerySource == memberQsre.ReferencedQuerySource
                                                           && projectionMemberExpression.Member.Name == memberExpression.Member.Name;
                                                }

                                                return false;
                                            });
                        }
                        else
                        {
                            projectionIndex
                                = subQueryProjection
                                    // Do NOT use orderingExpression variable here
                                    .FindIndex(e => _expressionEqualityComparer.Equals(e.RemoveConvert(), ordering.Expression.RemoveConvert()));
                        }

                        if (projectionIndex == -1)
                        {
                            projectionIndex = subQueryProjection.Count;

                            subQueryProjection.Add(
                                Expression.Convert(
                                    // Workaround re-linq#RMLNQ-111 - When this is fixed the Clone can go away
                                    CloningExpressionVisitor.AdjustExpressionAfterCloning(
                                        ordering.Expression,
                                        new QuerySourceMapping()),
                                    typeof(object)));
                        }

                        var newExpression
                            = Expression.Call(
                                targetExpression,
                                AnonymousObject.GetValueMethodInfo,
                                Expression.Constant(projectionIndex));

                        outerOrderByClause.Orderings
                            .Add(new Ordering(newExpression, ordering.OrderingDirection));
                    }

                    toQueryModel.BodyClauses.Add(outerOrderByClause);

                    if (canRemove)
                    {
                        fromQueryModel.BodyClauses.Remove(orderByClause);
                    }
                }
            }

            protected override Expression VisitSubQuery(SubQueryExpression subQueryExpression)
            {
                subQueryExpression.QueryModel.TransformExpressions(Visit);

                return subQueryExpression;
            }
        }
    }
}
