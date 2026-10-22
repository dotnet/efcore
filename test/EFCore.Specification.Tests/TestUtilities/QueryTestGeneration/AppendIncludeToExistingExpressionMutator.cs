// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration;

public class AppendIncludeToExistingExpressionMutator(DbContext context) : ExpressionMutator(context)
{
    private ExpressionFinder _expressionFinder = null!;

    public override bool IsValid(Expression expression)
    {
        _expressionFinder = new ExpressionFinder();
        _expressionFinder.Visit(expression);

        return _expressionFinder.FoundExpressions.Any();
    }

    public override Expression Apply(Expression expression, Random random)
    {
        var i = random.Next(_expressionFinder.FoundExpressions.Count);
        var expr = _expressionFinder.FoundExpressions[i];
        var thenInclude = random.Next(3) > 0;

        var entityType = expr.Type.GetGenericArguments()[0];
        var propertyType = expr.Type.GetGenericArguments()[1];
        var propertyElementType = IsEnumerableType(propertyType) || propertyType.GetInterfaces().Any(ii => IsEnumerableType(ii))
            ? propertyType.GetGenericArguments()[0]
            : propertyType;

        var navigations = thenInclude
            ? Context.Model.FindEntityType(propertyElementType)?.GetNavigations().ToList()
            : Context.Model.FindEntityType(entityType)?.GetNavigations().ToList();

        var prm = thenInclude
            ? Expression.Parameter(propertyElementType, "prm")
            : Expression.Parameter(entityType, "prm");

        if (navigations != null
            && navigations.Any())
        {
            var j = random.Next(navigations.Count);
            var navigation = navigations[j];

            if (thenInclude)
            {
                var thenIncludeMethod = IsEnumerableType(propertyType) || propertyType.GetInterfaces().Any(ii => IsEnumerableType(ii))
                    ? ThenIncludeCollectionMethodInfo.MakeGenericMethod(entityType, propertyElementType, navigation.ClrType)
                    : ThenIncludeReferenceMethodInfo.MakeGenericMethod(entityType, propertyElementType, navigation.ClrType);

                var injector = new ExpressionInjector(
                    _expressionFinder.FoundExpressions[i],
                    e => Expression.Call(
                        thenIncludeMethod,
                        e,
                        Expression.Lambda(Expression.Property(prm, navigation.Name), prm)));

                return injector.Visit(expression);
            }
            else
            {
                var includeMethod = IncludeMethodInfo.MakeGenericMethod(entityType, navigation.ClrType);

                var injector = new ExpressionInjector(
                    _expressionFinder.FoundExpressions[i],
                    e => Expression.Call(
                        includeMethod,
                        e,
                        Expression.Lambda(Expression.Property(prm, navigation.Name), prm)));

                return injector.Visit(expression);
            }
        }

        return expression;
    }

    private class ExpressionFinder : ExpressionVisitor
    {
        public readonly List<Expression> FoundExpressions = [];

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            // can't handle string overloads = need type information to construct Expression calls.
            if (node.Method.MethodIsClosedFormOf(IncludeMethodInfo)
                || node.Method.MethodIsClosedFormOf(ThenIncludeReferenceMethodInfo)
                || node.Method.MethodIsClosedFormOf(ThenIncludeCollectionMethodInfo))
            {
                FoundExpressions.Add(node);

                // need to short-circuit on ThenInclude, if we inject include before, it could change the IIncludeQueryable type that this ThenInclude is expecting
                if (node.Method.Name == nameof(EntityFrameworkQueryableExtensions.ThenInclude))
                {
                    return node;
                }
            }

            return base.VisitMethodCall(node);
        }
    }
}
