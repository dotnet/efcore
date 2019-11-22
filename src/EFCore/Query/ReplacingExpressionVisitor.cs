// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ReplacingExpressionVisitor : ExpressionVisitor
    {
        private readonly IDictionary<Expression, Expression> _replacements;

        public static Expression Replace(Expression original, Expression replacement, Expression tree)
        {
            return new ReplacingExpressionVisitor(
                new Dictionary<Expression, Expression> { { original, replacement } }).Visit(tree);
        }

        public ReplacingExpressionVisitor(IDictionary<Expression, Expression> replacements)
        {
            _replacements = replacements;
        }

        public override Expression Visit(Expression expression)
        {
            if (expression == null)
            {
                return expression;
            }

            if (_replacements.TryGetValue(expression, out var replacement))
            {
                return replacement;
            }

            return base.Visit(expression);
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var innerExpression = Visit(memberExpression.Expression);

            if (innerExpression is GroupByShaperExpression groupByShaperExpression
                && memberExpression.Member.Name == nameof(IGrouping<int, int>.Key))
            {
                return groupByShaperExpression.KeySelector;
            }

            if (innerExpression is NewExpression newExpression)
            {
                var index = newExpression.Members?.IndexOf(memberExpression.Member);
                if (index >= 0)
                {
                    return newExpression.Arguments[index.Value];
                }
            }

            if (innerExpression is MemberInitExpression memberInitExpression
                && memberInitExpression.Bindings.SingleOrDefault(
                    mb => mb.Member == memberExpression.Member) is MemberAssignment memberAssignment)
            {
                return memberAssignment.Expression;
            }

            return memberExpression.Update(innerExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.TryGetEFPropertyArguments(out var entityExpression, out var propertyName))
            {
                var newEntityExpression = Visit(entityExpression);
                if (newEntityExpression is NewExpression newExpression)
                {
                    var index = newExpression.Members?.Select(m => m.Name).IndexOf(propertyName);
                    if (index >= 0)
                    {
                        return newExpression.Arguments[index.Value];
                    }
                }

                if (newEntityExpression is MemberInitExpression memberInitExpression
                    && memberInitExpression.Bindings.SingleOrDefault(
                        mb => mb.Member.Name == propertyName) is MemberAssignment memberAssignment)
                {
                    return memberAssignment.Expression;
                }

                return methodCallExpression.Update(null, new[] { newEntityExpression, methodCallExpression.Arguments[1] });
            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }
}
