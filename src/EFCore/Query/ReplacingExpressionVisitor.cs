// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ReplacingExpressionVisitor : ExpressionVisitor
    {
        private readonly bool _quirkMode;

        private readonly Expression[] _originals;
        private readonly Expression[] _replacements;

        private readonly IDictionary<Expression, Expression> _quirkReplacements;

        public static Expression Replace(Expression original, Expression replacement, Expression tree)
        {
            return new ReplacingExpressionVisitor(new[] { original }, new[] { replacement }).Visit(tree);
        }

        public ReplacingExpressionVisitor(Expression[] originals, Expression[] replacements)
        {
            _quirkMode = AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue19737", out var enabled) && enabled;

            if (_quirkMode)
            {
                _quirkReplacements = new Dictionary<Expression, Expression>();
                for (var i = 0; i < originals.Length; i++)
                {
                    _quirkReplacements[originals[i]] = replacements[i];
                }
            }
            else
            {
                _originals = originals;
                _replacements = replacements;
            }
        }

        public ReplacingExpressionVisitor(IDictionary<Expression, Expression> replacements)
        {
            _quirkMode = AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue19737", out var enabled) && enabled;

            if (_quirkMode)
            {
                _quirkReplacements = replacements;
            }
            else
            {
                _originals = replacements.Keys.ToArray();
                _replacements = replacements.Values.ToArray();
            }
        }

        public override Expression Visit(Expression expression)
        {
            if (expression == null)
            {
                return expression;
            }

            if (_quirkMode)
            {
                if (_quirkReplacements.TryGetValue(expression, out var replacement))
                {
                    return replacement;
                }
            }
            else
            {
                // We use two arrays rather than a dictionary because hash calculation here can be prohibitively expensive
                // for deep trees. Locality of reference makes arrays better for the small number of replacements anyway.
                for (var i = 0; i < _originals.Length; i++)
                {
                    if (expression.Equals(_originals[i]))
                    {
                        return _replacements[i];
                    }
                }
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
