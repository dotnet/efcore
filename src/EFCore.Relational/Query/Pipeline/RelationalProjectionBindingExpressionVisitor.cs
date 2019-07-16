// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.NavigationExpansion;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalProjectionBindingExpressionVisitor : ExpressionVisitor
    {
        private readonly QueryableMethodTranslatingExpressionVisitor _queryableMethodTranslatingExpressionVisitor;
        private readonly RelationalSqlTranslatingExpressionVisitor _sqlTranslator;

        private SelectExpression _selectExpression;
        private bool _clientEval;
        private readonly IDictionary<ProjectionMember, Expression> _projectionMapping
            = new Dictionary<ProjectionMember, Expression>();
        private readonly Stack<ProjectionMember> _projectionMembers = new Stack<ProjectionMember>();

        public RelationalProjectionBindingExpressionVisitor(
            QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor,
            RelationalSqlTranslatingExpressionVisitor sqlTranslatingExpressionVisitor)
        {
            _queryableMethodTranslatingExpressionVisitor = queryableMethodTranslatingExpressionVisitor;
            _sqlTranslator = sqlTranslatingExpressionVisitor;
        }

        public Expression Translate(SelectExpression selectExpression, Expression expression)
        {
            _selectExpression = selectExpression;
            _clientEval = false;

            _projectionMembers.Push(new ProjectionMember());

            var result = Visit(expression);

            if (result == null)
            {
                _clientEval = true;

                result = Visit(expression);

                _projectionMapping.Clear();
            }

            _selectExpression.ReplaceProjectionMapping(_projectionMapping);
            _selectExpression = null;
            _projectionMembers.Clear();
            _projectionMapping.Clear();

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

                        case ParameterExpression parameterExpression:
                            return Expression.Call(
                                _getParameterValueMethodInfo.MakeGenericMethod(parameterExpression.Type),
                                QueryCompilationContext.QueryContextParameter,
                                Expression.Constant(parameterExpression.Name));

                        case MaterializeCollectionNavigationExpression materializeCollectionNavigationExpression:
                            return _selectExpression.AddCollectionProjection(
                                _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(
                                materializeCollectionNavigationExpression.Subquery),
                                materializeCollectionNavigationExpression.Navigation, null);

                        case MethodCallExpression methodCallExpression:
                            {
                                if (methodCallExpression.Method.IsGenericMethod
                                    && methodCallExpression.Method.DeclaringType == typeof(Enumerable)
                                    && methodCallExpression.Method.Name == nameof(Enumerable.ToList))
                                {
                                    var elementType = methodCallExpression.Method.GetGenericArguments()[0];

                                    var result = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(methodCallExpression.Arguments[0]);

                                    return _selectExpression.AddCollectionProjection(result, null, elementType);
                                }

                                var subquery = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(methodCallExpression);

                                if (subquery != null)
                                {
                                    if (subquery.ResultType == ResultType.Enumerable)
                                    {
                                        return _selectExpression.AddCollectionProjection(subquery, null, subquery.ShaperExpression.Type);
                                    }
                                }

                                break;
                            }
                    }

                    var translation = _sqlTranslator.Translate(expression);
                    return translation == null
                        ? base.Visit(expression)
                        : new ProjectionBindingExpression(_selectExpression, _selectExpression.AddToProjection(translation), expression.Type);
                }
                else
                {
                    var translation = _sqlTranslator.Translate(expression);
                    if (translation == null)
                    {
                        return null;
                    }

                    _projectionMapping[_projectionMembers.Peek()] = translation;

                    return new ProjectionBindingExpression(_selectExpression, _projectionMembers.Peek(), expression.Type);
                }
            }

            return base.Visit(expression);
        }

        private static readonly MethodInfo _getParameterValueMethodInfo
            = typeof(RelationalProjectionBindingExpressionVisitor)
                .GetTypeInfo().GetDeclaredMethod(nameof(GetParameterValue));

#pragma warning disable IDE0052 // Remove unread private members
        private static T GetParameterValue<T>(QueryContext queryContext, string parameterName)
#pragma warning restore IDE0052 // Remove unread private members
            => (T)queryContext.ParameterValues[parameterName];

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is EntityShaperExpression entityShaperExpression)
            {
                EntityProjectionExpression entityProjectionExpression;
                if (entityShaperExpression.ValueBufferExpression is ProjectionBindingExpression projectionBindingExpression)
                {
                    VerifySelectExpression(projectionBindingExpression);
                    entityProjectionExpression = (EntityProjectionExpression)_selectExpression.GetMappedProjection(
                        projectionBindingExpression.ProjectionMember);
                }
                else
                {
                    entityProjectionExpression = (EntityProjectionExpression)entityShaperExpression.ValueBufferExpression;
                }

                if (_clientEval)
                {
                    return entityShaperExpression.Update(
                        new ProjectionBindingExpression(_selectExpression, _selectExpression.AddToProjection(entityProjectionExpression)));
                }
                else
                {
                    _projectionMapping[_projectionMembers.Peek()] = entityProjectionExpression;

                    return entityShaperExpression.Update(
                        new ProjectionBindingExpression(_selectExpression, _projectionMembers.Peek(), typeof(ValueBuffer)));
                }
            }

            if (extensionExpression is IncludeExpression includeExpression)
            {
                return _clientEval
                    ? base.VisitExtension(includeExpression)
                    : null;
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
        private void VerifySelectExpression(ProjectionBindingExpression projectionBindingExpression)
        {
            if (projectionBindingExpression.QueryExpression != _selectExpression)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
