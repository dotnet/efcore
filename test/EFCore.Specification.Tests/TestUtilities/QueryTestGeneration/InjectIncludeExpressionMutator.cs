// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration;

#nullable disable

public class InjectIncludeExpressionMutator(DbContext context) : ExpressionMutator(context)
{
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

    private class ExpressionFinder(InjectIncludeExpressionMutator mutator) : ExpressionVisitor
    {
        private readonly InjectIncludeExpressionMutator _mutator = mutator;

        private readonly List<IEntityType> _topLevelEntityTypes = [];
        public readonly List<Expression> FoundExpressions = [];

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
