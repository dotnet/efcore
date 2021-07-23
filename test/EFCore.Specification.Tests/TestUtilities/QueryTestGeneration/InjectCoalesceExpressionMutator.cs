﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration
{
    public class InjectCoalesceExpressionMutator : ExpressionMutator
    {
        private readonly ExpressionFinder _expressionFinder = new();

        public InjectCoalesceExpressionMutator(DbContext context)
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

            var injector = new ExpressionInjector(
                _expressionFinder.FoundExpressions[i],
                e => Expression.Convert(
                    Expression.Coalesce(
                        e,
                        Expression.Default(e.Type.GetGenericArguments()[0])),
                    e.Type));

            return injector.Visit(expression);
        }

        private class ExpressionFinder : ExpressionVisitor
        {
            private bool _insideLambda;

            public List<Expression> FoundExpressions { get; } = new();

            public override Expression Visit(Expression node)
            {
                if (_insideLambda
                    && node != null
                    && node.NodeType != ExpressionType.Parameter
                    && node.Type.IsGenericType
                    && node.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
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
        }
    }
}
