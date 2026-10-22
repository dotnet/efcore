// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration;

public class InjectJoinWithSelfExpressionMutator(DbContext context) : ExpressionMutator(context)
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

        var expr = _expressionFinder.FoundExpressions[i];
        var elementType = expr.Type.GetGenericArguments()[0];

        var join = QueryableMethods.Join.MakeGenericMethod(elementType, elementType, elementType, elementType);

        var outerKeySelectorPrm = Expression.Parameter(elementType, "oks");
        var innerKeySelectorPrm = Expression.Parameter(elementType, "iks");

        var injector = new ExpressionInjector(
            _expressionFinder.FoundExpressions[i],
            e => Expression.Call(
                join,
                e,
                e,
                Expression.Lambda(outerKeySelectorPrm, outerKeySelectorPrm),
                Expression.Lambda(innerKeySelectorPrm, innerKeySelectorPrm),
                Expression.Lambda(outerKeySelectorPrm, outerKeySelectorPrm, innerKeySelectorPrm)));

        return injector.Visit(expression);
    }

    private class ExpressionFinder(InjectJoinWithSelfExpressionMutator mutator) : ExpressionVisitor
    {
        private readonly bool _insideThenBy = false;

        public List<Expression> FoundExpressions { get; } = [];

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name is nameof(Queryable.ThenBy)
                or nameof(Queryable.ThenByDescending)
                or nameof(EntityFrameworkQueryableExtensions.ThenInclude))
            {
                return node;
            }

            return base.VisitMethodCall(node);
        }

        [return: NotNullIfNotNull(nameof(node))]
        public override Expression? Visit(Expression? node)
        {
            if (node != null
                && !_insideThenBy
                && IsQueryableResult(node)
                && mutator.IsEntityType(node.Type.GetGenericArguments()[0]))
            {
                FoundExpressions.Add(node);
            }

            return base.Visit(node);
        }
    }
}
