// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    public class InMemoryProjectionBindingExpressionVisitor : ExpressionVisitor
    {
        private readonly InMemoryQueryableMethodTranslatingExpressionVisitor _queryableMethodTranslatingExpressionVisitor;
        private readonly InMemoryExpressionTranslatingExpressionVisitor _expressionTranslatingExpressionVisitor;

        private InMemoryQueryExpression _queryExpression;
        private bool _clientEval;
        private readonly IDictionary<ProjectionMember, Expression> _projectionMapping
            = new Dictionary<ProjectionMember, Expression>();
        private readonly Stack<ProjectionMember> _projectionMembers = new Stack<ProjectionMember>();

        public InMemoryProjectionBindingExpressionVisitor(
            InMemoryQueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor,
            InMemoryExpressionTranslatingExpressionVisitor expressionTranslatingExpressionVisitor)
        {
            _queryableMethodTranslatingExpressionVisitor = queryableMethodTranslatingExpressionVisitor;
            _expressionTranslatingExpressionVisitor = expressionTranslatingExpressionVisitor;
        }

        public virtual Expression Translate(InMemoryQueryExpression queryExpression, Expression expression)
        {
            _queryExpression = queryExpression;
            _clientEval = false;

            _projectionMembers.Push(new ProjectionMember());

            var expandedExpression = _queryableMethodTranslatingExpressionVisitor.ExpandWeakEntities(_queryExpression, expression);
            var result = Visit(expandedExpression);

            if (result == null)
            {
                _clientEval = true;

                expandedExpression = _queryableMethodTranslatingExpressionVisitor.ExpandWeakEntities(_queryExpression, expression);
                result = Visit(expandedExpression);

                _projectionMapping.Clear();
            }

            _queryExpression.ReplaceProjectionMapping(_projectionMapping);
            _queryExpression = null;
            _projectionMapping.Clear();
            _projectionMembers.Clear();

            return result;
        }

        public override Expression Visit(Expression expression)
        {
            if (expression == null)
            {
                return null;
            }

            if (!(expression is NewExpression
                  || expression is MemberInitExpression
                  || expression is EntityShaperExpression
                  || expression is IncludeExpression))
            {
                // This skips the group parameter from GroupJoin
                if (expression is ParameterExpression parameter
                    && parameter.Type.IsGenericType
                    && parameter.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return parameter;
                }

                if (_clientEval)
                {
                    switch (expression)
                    {
                        case ConstantExpression _:
                            return expression;

                        case MaterializeCollectionNavigationExpression materializeCollectionNavigationExpression:
                            return AddCollectionProjection(
                                _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(
                                    materializeCollectionNavigationExpression.Subquery),
                                materializeCollectionNavigationExpression.Navigation,
                                null);

                        case MethodCallExpression methodCallExpression:
                        {
                            if (methodCallExpression.Method.IsGenericMethod
                                && methodCallExpression.Method.DeclaringType == typeof(Enumerable)
                                && methodCallExpression.Method.Name == nameof(Enumerable.ToList))
                            {
                                return AddCollectionProjection(
                                    _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(
                                        methodCallExpression.Arguments[0]),
                                    null,
                                    methodCallExpression.Method.GetGenericArguments()[0]);
                            }

                            var subquery = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(methodCallExpression);
                            if (subquery != null)
                            {
                                if (subquery.ResultCardinality == ResultCardinality.Enumerable)
                                {
                                    return AddCollectionProjection(subquery, null, subquery.ShaperExpression.Type);
                                }

                                return new SingleResultShaperExpression(
                                    new ProjectionBindingExpression(
                                        _queryExpression,
                                        _queryExpression.AddSubqueryProjection(subquery, out var innerShaper),
                                        typeof(ValueBuffer)),
                                    innerShaper,
                                    subquery.ShaperExpression.Type);
                            }

                            break;
                        }
                    }

                    var translation = _expressionTranslatingExpressionVisitor.Translate(expression);
                    if (translation == null)
                    {
                        return base.Visit(expression);
                    }

                    if (translation.Type != expression.Type)
                    {
                        translation = NullSafeConvert(translation, expression.Type);
                    }

                    return new ProjectionBindingExpression(_queryExpression, _queryExpression.AddToProjection(translation), expression.Type);
                }
                else
                {
                    var translation = _expressionTranslatingExpressionVisitor.Translate(expression);
                    if (translation == null)
                    {
                        return null;
                    }

                    if (translation.Type != expression.Type)
                    {
                        translation = NullSafeConvert(translation, expression.Type);
                    }

                    _projectionMapping[_projectionMembers.Peek()] = translation;

                    return new ProjectionBindingExpression(_queryExpression, _projectionMembers.Peek(), expression.Type);
                }
            }

            return base.Visit(expression);
        }

        private Expression NullSafeConvert(Expression expression, Type convertTo)
            => expression.Type.IsNullableType() && !convertTo.IsNullableType() && expression.Type.UnwrapNullableType() == convertTo
                ? (Expression)Expression.Coalesce(expression, Expression.Default(convertTo))
                : Expression.Convert(expression, convertTo);

        private CollectionShaperExpression AddCollectionProjection(
            ShapedQueryExpression subquery, INavigation navigation, Type elementType)
            => new CollectionShaperExpression(
                new ProjectionBindingExpression(
                    _queryExpression,
                    _queryExpression.AddSubqueryProjection(
                        subquery,
                        out var innerShaper),
                    typeof(IEnumerable<ValueBuffer>)),
                innerShaper,
                navigation,
                elementType);

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is EntityShaperExpression entityShaperExpression)
            {
                EntityProjectionExpression entityProjectionExpression;
                if (entityShaperExpression.ValueBufferExpression is ProjectionBindingExpression projectionBindingExpression)
                {
                    VerifyQueryExpression(projectionBindingExpression);
                    entityProjectionExpression = (EntityProjectionExpression)_queryExpression.GetMappedProjection(
                        projectionBindingExpression.ProjectionMember);
                }
                else
                {
                    entityProjectionExpression = (EntityProjectionExpression)entityShaperExpression.ValueBufferExpression;
                }

                if (_clientEval)
                {
                    return entityShaperExpression.Update(
                        new ProjectionBindingExpression(_queryExpression, _queryExpression.AddToProjection(entityProjectionExpression)));
                }
                else
                {
                    _projectionMapping[_projectionMembers.Peek()] = entityProjectionExpression;

                    return entityShaperExpression.Update(
                        new ProjectionBindingExpression(_queryExpression, _projectionMembers.Peek(), typeof(ValueBuffer)));
                }
            }

            if (extensionExpression is IncludeExpression includeExpression)
            {
                return _clientEval
                    ? base.VisitExtension(includeExpression)
                    : null;
            }

            throw new InvalidOperationException(CoreStrings.QueryFailed(extensionExpression.Print(), GetType().Name));
        }

        protected override Expression VisitNew(NewExpression newExpression)
        {
            if (newExpression.Arguments.Count == 0)
            {
                return newExpression;
            }

            if (!_clientEval
                && newExpression.Members == null)
            {
                return null;
            }

            var newArguments = new Expression[newExpression.Arguments.Count];
            for (var i = 0; i < newArguments.Length; i++)
            {
                if (_clientEval)
                {
                    newArguments[i] = Visit(newExpression.Arguments[i]);
                }
                else
                {
                    var projectionMember = _projectionMembers.Peek().Append(newExpression.Members[i]);
                    _projectionMembers.Push(projectionMember);
                    newArguments[i] = Visit(newExpression.Arguments[i]);
                    if (newArguments[i] == null)
                    {
                        return null;
                    }
                    _projectionMembers.Pop();
                }
            }

            return newExpression.Update(newArguments);
        }

        protected override Expression VisitMemberInit(MemberInitExpression memberInitExpression)
        {
            var newExpression = VisitAndConvert(memberInitExpression.NewExpression, nameof(VisitMemberInit));
            if (newExpression == null)
            {
                return null;
            }

            var newBindings = new MemberBinding[memberInitExpression.Bindings.Count];
            for (var i = 0; i < newBindings.Length; i++)
            {
                if (memberInitExpression.Bindings[i].BindingType != MemberBindingType.Assignment)
                {
                    return null;
                }

                newBindings[i] = VisitMemberBinding(memberInitExpression.Bindings[i]);
                if (newBindings[i] == null)
                {
                    return null;
                }
            }

            return memberInitExpression.Update(newExpression, newBindings);
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment memberAssignment)
        {
            if (_clientEval)
            {
                return memberAssignment.Update(Visit(memberAssignment.Expression));
            }

            var projectionMember = _projectionMembers.Peek().Append(memberAssignment.Member);
            _projectionMembers.Push(projectionMember);

            var visitedExpression = Visit(memberAssignment.Expression);
            if (visitedExpression == null)
            {
                return null;
            }

            _projectionMembers.Pop();
            return memberAssignment.Update(visitedExpression);
        }

        // TODO: Debugging
        private void VerifyQueryExpression(ProjectionBindingExpression projectionBindingExpression)
        {
            if (projectionBindingExpression.QueryExpression != _queryExpression)
            {
                throw new InvalidOperationException(CoreStrings.QueryFailed(projectionBindingExpression.Print(), GetType().Name));
            }
        }
    }
}
