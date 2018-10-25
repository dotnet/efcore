// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Extensions.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration
{
    public class StringConcatWithSelfExpressionMutator : ExpressionMutator
    {
        private ExpressionFinder _expressionFinder = new ExpressionFinder();

        public StringConcatWithSelfExpressionMutator(DbContext context)
            : base(context)
        {
        }

        public override bool IsValid(Expression expression)
        {
            _expressionFinder.Visit(expression);

            return _expressionFinder.FoundExpressions.Any();
        }

        public override Expression Apply(Expression expression, Random random)
        {
            var i = random.Next(_expressionFinder.FoundExpressions.Count);

            var stringConcatMethodInfo
                = typeof(string).GetRuntimeMethod(
                    nameof(string.Concat),
                    new[] { typeof(string), typeof(string) });

            var injector = new ExpressionInjector(_expressionFinder.FoundExpressions[i], e => Expression.Add(e, e, stringConcatMethodInfo));

            return injector.Visit(expression);
        }

        private class ExpressionFinder : ExpressionVisitor
        {
            private bool _insideLambda = false;

            public List<Expression> FoundExpressions { get; } = new List<Expression>();

            public override Expression Visit(Expression node)
            {
                if (_insideLambda && node?.Type == typeof(string) && node.NodeType != ExpressionType.Parameter)
                {
                    FoundExpressions.Add(node);
                }

                return base.Visit(node);
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                var oldInsideLambda = _insideLambda;
                _insideLambda = true;

                var result = base.VisitLambda(node);

                _insideLambda = oldInsideLambda;

                return result;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node != null && node.IsEFProperty())
                {
                    return node;
                }

                return base.VisitMethodCall(node);
            }
        }
    }
}
