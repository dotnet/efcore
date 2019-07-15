// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.NavigationExpansion;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public class CosmosProjectionBindingExpressionVisitor : ExpressionVisitor
    {
        private readonly CosmosSqlTranslatingExpressionVisitor _sqlTranslator;
        private SelectExpression _selectExpression;
        private bool _clientEval;
        private readonly IDictionary<ProjectionMember, Expression> _projectionMapping
            = new Dictionary<ProjectionMember, Expression>();
        private readonly Stack<ProjectionMember> _projectionMembers = new Stack<ProjectionMember>();

        public CosmosProjectionBindingExpressionVisitor(CosmosSqlTranslatingExpressionVisitor sqlTranslator)
        {
            _sqlTranslator = sqlTranslator;
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

            if (expression is NewExpression
                || expression is MemberInitExpression
                || expression is EntityShaperExpression)
            {
                return base.Visit(expression);
            }

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
                        return base.Visit(expression);
                    //return _selectExpression.AddCollectionProjection(
                    //    _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(
                    //        materializeCollectionNavigationExpression.Subquery),
                    //    materializeCollectionNavigationExpression.Navigation, null);
                }

                var translation = _sqlTranslator.Translate(expression);
                if (translation == null)
                {
                    return base.Visit(expression);
                }

                return new ProjectionBindingExpression(
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

        private static readonly MethodInfo _getParameterValueMethodInfo
            = typeof(CosmosProjectionBindingExpressionVisitor)
                .GetTypeInfo().GetDeclaredMethod(nameof(GetParameterValue));

#pragma warning disable IDE0052 // Remove unread private members
        private static T GetParameterValue<T>(QueryContext queryContext, string parameterName)
#pragma warning restore IDE0052 // Remove unread private members
            => (T)queryContext.ParameterValues[parameterName];

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            if (!_clientEval)
            {
                return null;
            }

            var innerExpression = Visit(memberExpression.Expression);

            EntityShaperExpression shaperExpression;
            switch (innerExpression)
            {
                case EntityShaperExpression shaper:
                    shaperExpression = shaper;
                    break;

                case UnaryExpression unaryExpression:
                    shaperExpression = unaryExpression.Operand as EntityShaperExpression;
                    if (shaperExpression == null)
                    {
                        return memberExpression.Update(innerExpression);
                    }
                    break;

                default:
                    return memberExpression.Update(innerExpression);
            }

            EntityProjectionExpression innerEntityProjection;
            switch (shaperExpression.ValueBufferExpression)
            {
                case ProjectionBindingExpression innerProjectionBindingExpression:
                    innerEntityProjection = (EntityProjectionExpression)_selectExpression.Projection[
                        innerProjectionBindingExpression.Index.Value].Expression;
                    break;

                case UnaryExpression unaryExpression:
                    innerEntityProjection = (EntityProjectionExpression)((UnaryExpression)unaryExpression.Operand).Operand;
                    break;

                default:
                    throw new InvalidOperationException();
            }

            var navigationProjection = innerEntityProjection.BindMember(memberExpression.Member, innerExpression.Type, out var propertyBase);

            if (!(propertyBase is INavigation navigation)
                || !navigation.IsEmbedded())
            {
                return memberExpression.Update(innerExpression);
            }

            switch (navigationProjection)
            {
                case EntityProjectionExpression entityProjection:
                    return new EntityShaperExpression(
                        navigation.GetTargetType(),
                        Expression.Convert(Expression.Convert(entityProjection, typeof(object)), typeof(ValueBuffer)),
                        nullable: true);

                case ObjectArrayProjectionExpression objectArrayProjectionExpression:
                {
                    var innerShaperExpression = new EntityShaperExpression(
                        navigation.GetTargetType(),
                        Expression.Convert(
                            Expression.Convert(objectArrayProjectionExpression.InnerProjection, typeof(object)), typeof(ValueBuffer)),
                        nullable: true);

                    return new CollectionShaperExpression(
                        objectArrayProjectionExpression,
                        innerShaperExpression,
                        navigation,
                        innerShaperExpression.EntityType.ClrType);
                }

                default:
                    throw new InvalidOperationException();
            }
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case EntityShaperExpression entityShaperExpression:
                {
                    var projectionBindingExpression = (ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression;
                    VerifySelectExpression(projectionBindingExpression);

                    if (_clientEval)
                    {
                        var entityProjection = (EntityProjectionExpression)_selectExpression.GetMappedProjection(
                            projectionBindingExpression.ProjectionMember);

                        return entityShaperExpression.Update(
                            new ProjectionBindingExpression(
                                _selectExpression, _selectExpression.AddToProjection(entityProjection), typeof(ValueBuffer)));
                    }

                    _projectionMapping[_projectionMembers.Peek()]
                        = _selectExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember);

                    return entityShaperExpression.Update(
                        new ProjectionBindingExpression(_selectExpression, _projectionMembers.Peek(), typeof(ValueBuffer)));
                }

                case MaterializeCollectionNavigationExpression materializeCollectionNavigationExpression:
                    return materializeCollectionNavigationExpression.Navigation.IsEmbedded()
                        ? base.Visit(materializeCollectionNavigationExpression.Subquery)
                        : base.VisitExtension(materializeCollectionNavigationExpression);

                case IncludeExpression includeExpression:
                    return _clientEval ? base.VisitExtension(includeExpression) : null;

                default:
                    throw new InvalidOperationException(new ExpressionPrinter().Print(extensionExpression));
            }
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
