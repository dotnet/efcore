// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Pipeline
{
    public class InMemoryProjectionBindingExpressionVisitor : ExpressionVisitor
    {
        private InMemoryQueryExpression _queryExpression;
        private readonly IDictionary<ProjectionMember, Expression> _projectionMapping
            = new Dictionary<ProjectionMember, Expression>();

        private readonly Stack<ProjectionMember> _projectionMembers = new Stack<ProjectionMember>();
        private readonly InMemoryExpressionTranslatingExpressionVisitor _expressionTranslatingExpressionVisitor;

        public InMemoryProjectionBindingExpressionVisitor(
            InMemoryExpressionTranslatingExpressionVisitor expressionTranslatingExpressionVisitor)
        {
            _expressionTranslatingExpressionVisitor = expressionTranslatingExpressionVisitor;
        }

        public Expression Translate(InMemoryQueryExpression queryExpression, Expression expression)
        {
            _queryExpression = queryExpression;

            _projectionMembers.Push(new ProjectionMember());

            var result = Visit(expression);

            _queryExpression.ApplyProjection(_projectionMapping);

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
                  || expression is EntityShaperExpression))
            {
                var translation = _expressionTranslatingExpressionVisitor.Translate(_queryExpression, expression);

                _projectionMapping[_projectionMembers.Peek()] = translation;

                return new ProjectionBindingExpression(_queryExpression, _projectionMembers.Peek(), expression.Type);
            }

            return base.Visit(expression);
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is EntityShaperExpression entityShaperExpression)
            {
                _projectionMapping[_projectionMembers.Peek()]
                    = _queryExpression.GetProjectionExpression(
                        entityShaperExpression.ValueBufferExpression.ProjectionMember);

                return entityShaperExpression.Update(
                    new ProjectionBindingExpression(_queryExpression, _projectionMembers.Peek(), typeof(ValueBuffer)));
            }

            throw new InvalidOperationException();
        }

        protected override Expression VisitNew(NewExpression newExpression)
        {
            var newArguments = new Expression[newExpression.Arguments.Count];
            for (var i = 0; i < newExpression.Arguments.Count; i++)
            {
                // TODO: Members can be null????
                var projectionMember = _projectionMembers.Peek().AddMember(newExpression.Members[i]);
                _projectionMembers.Push(projectionMember);

                newArguments[i] = Visit(newExpression.Arguments[i]);
                _projectionMembers.Pop();
            }

            return newExpression.Update(newArguments);
        }

        protected override Expression VisitMemberInit(MemberInitExpression memberInitExpression)
        {
            var newExpression = (NewExpression)Visit(memberInitExpression.NewExpression);
            var newBindings = new MemberAssignment[memberInitExpression.Bindings.Count];
            for (var i = 0; i < newBindings.Length; i++)
            {
                // TODO: Members can be null????
                var memberAssignment = (MemberAssignment)memberInitExpression.Bindings[i];

                var projectionMember = _projectionMembers.Peek().AddMember(memberAssignment.Member);
                _projectionMembers.Push(projectionMember);

                newBindings[i] = memberAssignment.Update(Visit(memberAssignment.Expression));
                _projectionMembers.Pop();
            }

            return memberInitExpression.Update(newExpression, newBindings);
        }
    }
}
