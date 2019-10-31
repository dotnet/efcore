// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class RelationalProjectionBindingExpressionVisitor : ExpressionVisitor
    {
        private readonly RelationalQueryableMethodTranslatingExpressionVisitor _queryableMethodTranslatingExpressionVisitor;
        private readonly RelationalSqlTranslatingExpressionVisitor _sqlTranslator;

        private SelectExpression _selectExpression;
        private bool _clientEval;

        private readonly IDictionary<ProjectionMember, Expression> _projectionMapping
            = new Dictionary<ProjectionMember, Expression>();

        private readonly Stack<ProjectionMember> _projectionMembers = new Stack<ProjectionMember>();

        public RelationalProjectionBindingExpressionVisitor(
            RelationalQueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor,
            RelationalSqlTranslatingExpressionVisitor sqlTranslatingExpressionVisitor)
        {
            _queryableMethodTranslatingExpressionVisitor = queryableMethodTranslatingExpressionVisitor;
            _sqlTranslator = sqlTranslatingExpressionVisitor;
        }

        public virtual Expression Translate(SelectExpression selectExpression, Expression expression)
        {
            _selectExpression = selectExpression;
            _clientEval = false;

            _projectionMembers.Push(new ProjectionMember());

            var expandedExpression = _queryableMethodTranslatingExpressionVisitor.ExpandWeakEntities(_selectExpression, expression);
            var result = Visit(expandedExpression);

            if (result == null)
            {
                _clientEval = true;

                expandedExpression = _queryableMethodTranslatingExpressionVisitor.ExpandWeakEntities(_selectExpression, expression);
                result = Visit(expandedExpression);

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

                                var result = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(
                                    methodCallExpression.Arguments[0]);

                                return _selectExpression.AddCollectionProjection(result, null, elementType);
                            }

                            var subquery = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(methodCallExpression);

                            if (subquery != null)
                            {
                                if (subquery.ResultCardinality == ResultCardinality.Enumerable)
                                {
                                    return _selectExpression.AddCollectionProjection(subquery, null, subquery.ShaperExpression.Type);
                                }

                                static bool IsAggregateResultWithCustomShaper(MethodInfo method)
                                {
                                    if (method.IsGenericMethod)
                                    {
                                        method = method.GetGenericMethodDefinition();
                                    }

                                    return QueryableMethods.IsAverageWithoutSelector(method)
                                        || QueryableMethods.IsAverageWithSelector(method)
                                        || method == QueryableMethods.MaxWithoutSelector
                                        || method == QueryableMethods.MaxWithSelector
                                        || method == QueryableMethods.MinWithoutSelector
                                        || method == QueryableMethods.MinWithSelector
                                        || QueryableMethods.IsSumWithoutSelector(method)
                                        || QueryableMethods.IsSumWithSelector(method);
                                }

                                if (!(subquery.ShaperExpression is ProjectionBindingExpression
                                    || IsAggregateResultWithCustomShaper(methodCallExpression.Method)))
                                {
                                    return _selectExpression.AddSingleProjection(subquery);
                                }
                            }

                            break;
                        }
                    }

                    var translation = _sqlTranslator.Translate(expression);
                    return translation == null
                        ? base.Visit(expression)
                        : new ProjectionBindingExpression(
                            _selectExpression, _selectExpression.AddToProjection(translation), expression.Type);
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

                _projectionMapping[_projectionMembers.Peek()] = entityProjectionExpression;

                return entityShaperExpression.Update(
                    new ProjectionBindingExpression(_selectExpression, _projectionMembers.Peek(), typeof(ValueBuffer)));
            }

            if (extensionExpression is IncludeExpression includeExpression)
            {
                return _clientEval
                    ? base.VisitExtension(includeExpression)
                    : null;
            }

            throw new InvalidOperationException(
                CoreStrings.QueryFailed(extensionExpression.Print(), GetType().Name));
        }

        protected override Expression VisitNew(NewExpression newExpression)
        {
            // For .NET Framework only. If ctor is null that means the type is struct and has no ctor args.
            if (newExpression.Constructor == null)
            {
                return newExpression;
            }

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
        private void VerifySelectExpression(ProjectionBindingExpression projectionBindingExpression)
        {
            if (projectionBindingExpression.QueryExpression != _selectExpression)
            {
                throw new InvalidOperationException(CoreStrings.QueryFailed(projectionBindingExpression.Print(), GetType().Name));
            }
        }
    }
}
