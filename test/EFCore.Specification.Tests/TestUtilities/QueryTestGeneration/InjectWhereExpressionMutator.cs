﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration
{
    public class InjectWhereExpressionMutator : ExpressionMutator
    {
        private ExpressionFinder _expressionFinder;

        public InjectWhereExpressionMutator(DbContext context)
            : base(context)
        {
        }

        public override bool IsValid(Expression expression)
        {
            _expressionFinder = new ExpressionFinder(this);
            _expressionFinder.Visit(expression);

            return _expressionFinder.FoundExpressions.Any();
        }

        public override Expression Apply(Expression expression, Random random)
        {
            var i = random.Next(_expressionFinder.FoundExpressions.Count);
            var expressionToInject = _expressionFinder.FoundExpressions[i];

            var typeArgument = expressionToInject.Type.GetGenericArguments()[0];
            var prm = Expression.Parameter(typeArgument, "prm");

            var candidateExpressions = new List<Expression> { Expression.Constant(random.Choose(new List<bool> { true, false })) };

            if (typeArgument == typeof(bool))
            {
                candidateExpressions.Add(prm);
            }

            var properties = typeArgument.GetProperties().Where(p => !p.GetMethod.IsStatic).ToList();
            properties = FilterPropertyInfos(typeArgument, properties);

            var boolProperties = properties.Where(p => p.PropertyType == typeof(bool)).ToList();
            if (boolProperties.Any())
            {
                candidateExpressions.Add(Expression.Property(prm, random.Choose(boolProperties)));
            }

            // compare two properties
            var propertiesOfTheSameType = properties.GroupBy(p => p.PropertyType).Where(g => g.Count() > 1).ToList();
            if (propertiesOfTheSameType.Any())
            {
                var propertyGroup = random.Choose(propertiesOfTheSameType).ToList();

                var firstProperty = random.Choose(propertyGroup);
                var secondProperty = random.Choose(propertyGroup.Where(p => p != firstProperty).ToList());

                candidateExpressions.Add(
                    Expression.NotEqual(Expression.Property(prm, firstProperty), Expression.Property(prm, secondProperty)));
            }

            // compare property to constant
            if (properties.Any())
            {
                var property = random.Choose(properties);
                candidateExpressions.Add(
                    Expression.NotEqual(
                        Expression.Property(prm, property),
                        Expression.Default(property.PropertyType)));
            }

            if (IsEntityType(typeArgument))
            {
                var entityType = Context.Model.FindEntityType(typeArgument);
                var navigations = entityType.GetNavigations().ToList();
                var collectionNavigations = navigations.Where(n => n.IsCollection).ToList();

                var collectionNavigation = random.Choose(collectionNavigations);
                if (collectionNavigation != null)
                {
                    var any = EnumerableMethods.AnyWithoutPredicate.MakeGenericMethod(
                        collectionNavigation.ForeignKey.DeclaringEntityType.ClrType);

                    // collection.Any()
                    candidateExpressions.Add(
                        Expression.Call(
                            any,
                            Expression.Property(prm, collectionNavigation.PropertyInfo)));
                }

                var navigation = random.Choose(navigations);
            }

            var lambdaBody = random.Choose(candidateExpressions);

            var negated = random.Next(6) > 3;
            if (negated)
            {
                lambdaBody = Expression.Not(lambdaBody);
            }

            var where = QueryableMethods.Where.MakeGenericMethod(typeArgument);
            var lambda = Expression.Lambda(lambdaBody, prm);
            var injector = new ExpressionInjector(expressionToInject, e => Expression.Call(where, e, lambda));

            return injector.Visit(expression);
        }

        private class ExpressionFinder : ExpressionVisitor
        {
            private readonly InjectWhereExpressionMutator _mutator;

            public ExpressionFinder(InjectWhereExpressionMutator mutator)
            {
                _mutator = mutator;
            }

            public List<Expression> FoundExpressions { get; } = new();

            public override Expression Visit(Expression expression)
            {
                if (expression is MethodCallExpression methodCallExpression
                    && (methodCallExpression.Method.Name == "ThenInclude"
                        || methodCallExpression.Method.Name == "ThenBy"
                        || methodCallExpression.Method.Name == "ThenByDescending"
                        || methodCallExpression.Method.Name == "Skip"
                        || methodCallExpression.Method.Name == "Take"))
                {
                    return expression;
                }

                if (expression != null
                    && IsQueryableResult(expression))
                {
                    FoundExpressions.Add(expression);
                }

                return base.Visit(expression);
            }
        }
    }
}
