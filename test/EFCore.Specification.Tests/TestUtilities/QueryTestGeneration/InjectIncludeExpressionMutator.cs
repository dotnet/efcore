// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration
{
    public class InjectIncludeExpressionMutator : ExpressionMutator
    {
        public InjectIncludeExpressionMutator(DbContext context)
            : base(context)
        {
        }

        private ExpressionFinder _expressionFinder;

        public override bool IsValid(Expression expression)
        {
            _expressionFinder = new ExpressionFinder(this);
            if (IsQueryableResult(expression))
            {
                _expressionFinder.TryAddType(expression.Type.GetGenericArguments()[0]);
                _expressionFinder.Visit(expression);
            }

            return _expressionFinder.FoundExpressions.Any();
        }

        public override Expression Apply(Expression expression, Random random)
        {
            var i = random.Next(_expressionFinder.FoundExpressions.Count);
            var expr = _expressionFinder.FoundExpressions[i];

            var entityType = expr.Type.GetGenericArguments()[0];
            var navigations = Context.Model.FindEntityType(entityType)?.GetNavigations().ToList();

            var prm = Expression.Parameter(entityType, "prm");

            if (navigations != null
                && navigations.Any())
            {
                var j = random.Next(navigations.Count);
                var navigation = navigations[j];

                var includeMethod = IncludeMethodInfo.MakeGenericMethod(entityType, navigation.ClrType);

                var injector = new ExpressionInjector(
                    _expressionFinder.FoundExpressions[i],
                    e => Expression.Call(
                        includeMethod,
                        e,
                        Expression.Lambda(Expression.Property(prm, navigation.Name), prm)));

                return injector.Visit(expression);
            }

            return expression;
        }

        private class ExpressionFinder : ExpressionVisitor
        {
            private readonly InjectIncludeExpressionMutator _mutator;

            private readonly List<IEntityType> _topLevelEntityTypes = new List<IEntityType>();

            public ExpressionFinder(InjectIncludeExpressionMutator mutator)
            {
                _mutator = mutator;
            }

            public readonly List<Expression> FoundExpressions = new List<Expression>();

            private int _depth;

            private const int MaxDepth = 5;

            public void TryAddType(Type type)
            {
                if (!type.IsValueType
                    && _depth < MaxDepth)
                {
                    var entityType = _mutator.Context.Model.FindEntityType(type);
                    if (entityType != null)
                    {
                        _topLevelEntityTypes.Add(entityType);
                    }
                    else
                    {
                        var properties = type.GetProperties().ToList();
                        foreach (var property in properties)
                        {
                            _depth++;
                            TryAddType(property.PropertyType);
                            _depth--;
                        }
                    }
                }
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node.Type.IsGenericType
                    && node.Type.GetGenericTypeDefinition() == typeof(EntityQueryable<>))
                {
                    FoundExpressions.Add(node);
                }

                return base.VisitConstant(node);
            }
        }
    }
}
