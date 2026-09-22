// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration;

public class InjectOrderByPropertyExpressionMutator(DbContext context) : ExpressionMutator(context)
{
    private ExpressionFinder _expressionFinder = null!;

    public override bool IsValid(Expression expression)
    {
        _expressionFinder = new ExpressionFinder(this);
        _expressionFinder.Visit(expression);

        return _expressionFinder.FoundExpressions.Any();
    }

    public override Expression Apply(Expression expression, Random random)
    {
        var i = random.Next(_expressionFinder.FoundExpressions.Count);
        var expressionToInject = _expressionFinder.FoundExpressions.ToList()[i].Key;
        var j = random.Next(_expressionFinder.FoundExpressions[expressionToInject].Count);
        var property = _expressionFinder.FoundExpressions[expressionToInject][j];

        var typeArgument = expressionToInject.Type.GetGenericArguments()[0];

        var isDescending = random.Next(3) == 0;
        var orderBy = isDescending
            ? QueryableMethods.OrderByDescending.MakeGenericMethod(typeArgument, property.PropertyType)
            : QueryableMethods.OrderBy.MakeGenericMethod(typeArgument, property.PropertyType);

        var prm = Expression.Parameter(typeArgument, "prm");
        var lambdaBody = (Expression)Expression.Property(prm, property);

        if (property.PropertyType.IsValueType
            && !(property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)))
        {
            var nullablePropertyType = typeof(Nullable<>).MakeGenericType(property.PropertyType);

            orderBy = isDescending
                ? QueryableMethods.OrderByDescending.MakeGenericMethod(typeArgument, nullablePropertyType)
                : QueryableMethods.OrderBy.MakeGenericMethod(typeArgument, nullablePropertyType);

            lambdaBody = Expression.Convert(lambdaBody, nullablePropertyType);
        }

        if (typeArgument == typeof(string))
        {
            // string.Length - make it nullable in case we access optional argument
            orderBy = QueryableMethods.OrderBy.MakeGenericMethod(typeArgument, typeof(int?));
            lambdaBody = Expression.Convert(lambdaBody, typeof(int?));
        }

        var lambda = Expression.Lambda(lambdaBody, prm);
        var injector = new ExpressionInjector(expressionToInject, e => Expression.Call(orderBy, e, lambda));

        return injector.Visit(expression);
    }

    private class ExpressionFinder(InjectOrderByPropertyExpressionMutator mutator) : ExpressionVisitor
    {
        private List<PropertyInfo> GetValidPropertiesForOrderBy(Expression expression)
            => expression.Type.GetGenericArguments()[0].GetProperties().Where(p => !p.GetMethod!.IsStatic)
                .Where(p => IsOrderedableType(p.PropertyType)).ToList();

        private bool _insideThenInclude;

        public Dictionary<Expression, List<PropertyInfo>> FoundExpressions { get; } = new();

        [return: NotNullIfNotNull(nameof(expression))]
        public override Expression? Visit(Expression? expression)
        {
            // can't inject OrderBy inside of include - would have to rewrite the ThenInclude method to one that accepts ordered input
            var insideThenInclude = default(bool?);
            if (expression is MethodCallExpression { Method.Name: "ThenInclude" or "ThenBy" or "ThenByDescending" })
            {
                insideThenInclude = _insideThenInclude;
                _insideThenInclude = true;
            }

            if (!_insideThenInclude
                && expression != null
                && IsQueryableResult(expression)
                && !FoundExpressions.ContainsKey(expression))
            {
                var validProperties = GetValidPropertiesForOrderBy(expression);
                validProperties = mutator.FilterPropertyInfos(expression.Type.GetGenericArguments()[0], validProperties);

                if (validProperties.Any())
                {
                    FoundExpressions.Add(expression, validProperties);
                }
            }

            try
            {
                return base.Visit(expression);
            }
            finally
            {
                _insideThenInclude = insideThenInclude ?? _insideThenInclude;
            }
        }
    }
}
