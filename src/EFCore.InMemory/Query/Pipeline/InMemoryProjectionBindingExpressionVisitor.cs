// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.NavigationExpansion;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Pipeline
{
    public class InMemoryProjectionBindingExpressionVisitor : ExpressionVisitor
    {
        private readonly QueryableMethodTranslatingExpressionVisitor _queryableMethodTranslatingExpressionVisitor;
        private readonly InMemoryExpressionTranslatingExpressionVisitor _expressionTranslatingExpressionVisitor;

        private InMemoryQueryExpression _queryExpression;
        private bool _clientEval;
        private readonly IDictionary<ProjectionMember, Expression> _projectionMapping
            = new Dictionary<ProjectionMember, Expression>();
        private readonly Stack<ProjectionMember> _projectionMembers = new Stack<ProjectionMember>();

        public InMemoryProjectionBindingExpressionVisitor(
            QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor,
            InMemoryExpressionTranslatingExpressionVisitor expressionTranslatingExpressionVisitor)
        {
            _queryableMethodTranslatingExpressionVisitor = queryableMethodTranslatingExpressionVisitor;
            _expressionTranslatingExpressionVisitor = expressionTranslatingExpressionVisitor;
        }

        public Expression Translate(InMemoryQueryExpression queryExpression, Expression expression)
        {
            _queryExpression = queryExpression;
            _clientEval = false;

            _projectionMembers.Push(new ProjectionMember());

            var result = Visit(expression);

            if (result == null)
            {
                _clientEval = true;

                result = Visit(expression);

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
                            //return _selectExpression.AddCollectionProjection(
                            //    _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(
                            //    materializeCollectionNavigationExpression.Subquery),
                            //    materializeCollectionNavigationExpression.Navigation, null);
                            throw new NotImplementedException();

                        case MethodCallExpression methodCallExpression:
                            {
                                if (methodCallExpression.Method.IsGenericMethod
                                    && methodCallExpression.Method.DeclaringType == typeof(Enumerable)
                                    && methodCallExpression.Method.Name == nameof(Enumerable.ToList))
                                {
                                    //var elementType = methodCallExpression.Method.GetGenericArguments()[0];

                                    //var result = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(methodCallExpression.Arguments[0]);

                                    //return _selectExpression.AddCollectionProjection(result, null, elementType);
                                    throw new NotImplementedException();
                                }

                                var subquery = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(methodCallExpression);

                                if (subquery != null)
                                {
                                    //if (subquery.ResultType == ResultType.Enumerable)
                                    //{
                                    //    return _selectExpression.AddCollectionProjection(subquery, null, subquery.ShaperExpression.Type);
                                    //}
                                    throw new NotImplementedException();
                                }

                                break;
                            }
                    }


                    var translation = _expressionTranslatingExpressionVisitor.Translate(expression);
                    return translation == null
                        ? base.Visit(expression)
                        : new ProjectionBindingExpression(_queryExpression, _queryExpression.AddToProjection(translation), expression.Type);
                }
                else
                {
                    var translation = _expressionTranslatingExpressionVisitor.Translate(expression);
                    if (translation == null)
                    {
                        return null;
                    }

                    _projectionMapping[_projectionMembers.Peek()] = translation;

                    return new ProjectionBindingExpression(_queryExpression, _projectionMembers.Peek(), expression.Type);
                }

            }

            return base.Visit(expression);
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is EntityShaperExpression entityShaperExpression)
            {
                var projectionBindingExpression = (ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression;
                VerifyQueryExpression(projectionBindingExpression);

                if (_clientEval)
                {
                    var entityProjection = (EntityProjectionExpression)_queryExpression.GetMappedProjection(
                        projectionBindingExpression.ProjectionMember);

                    return entityShaperExpression.Update(
                        new ProjectionBindingExpression(_queryExpression, _queryExpression.AddToProjection(entityProjection)));
                }
                else
                {
                    _projectionMapping[_projectionMembers.Peek()]
                        = _queryExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember);

                    return entityShaperExpression.Update(
                        new ProjectionBindingExpression(_queryExpression, _projectionMembers.Peek(), typeof(ValueBuffer)));
                }
            }

            throw new InvalidOperationException();
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
                    var projectionMember = _projectionMembers.Peek().AddMember(newExpression.Members[i]);
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
            var newExpression = (NewExpression)Visit(memberInitExpression.NewExpression);
            if (newExpression == null)
            {
                return null;
            }

            var newBindings = new MemberAssignment[memberInitExpression.Bindings.Count];
            for (var i = 0; i < newBindings.Length; i++)
            {
                var memberAssignment = (MemberAssignment)memberInitExpression.Bindings[i];
                if (_clientEval)
                {
                    newBindings[i] = memberAssignment.Update(Visit(memberAssignment.Expression));
                }
                else
                {
                    var projectionMember = _projectionMembers.Peek().AddMember(memberAssignment.Member);
                    _projectionMembers.Push(projectionMember);

                    var visitedExpression = Visit(memberAssignment.Expression);
                    if (visitedExpression == null)
                    {
                        return null;
                    }

                    newBindings[i] = memberAssignment.Update(visitedExpression);
                    _projectionMembers.Pop();
                }
            }

            return memberInitExpression.Update(newExpression, newBindings);
        }

        // TODO: Debugging
        private void VerifyQueryExpression(ProjectionBindingExpression projectionBindingExpression)
        {
            if (projectionBindingExpression.QueryExpression != _queryExpression)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
