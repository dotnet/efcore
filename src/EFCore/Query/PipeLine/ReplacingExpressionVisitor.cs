// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
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

            if (innerExpression is NewExpression newExpression)
            {
                var index = newExpression.Members.IndexOf(memberExpression.Member);

                return newExpression.Arguments[index];
            }

            if (innerExpression is MemberInitExpression memberInitExpression)
            {
                return ((MemberAssignment)memberInitExpression.Bindings
                    .Single(mb => mb.Member == memberExpression.Member)).Expression;
            }

            return memberExpression.Update(innerExpression);
        }
    }

}
