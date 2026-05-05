// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration;

public class InjectStringFunctionExpressionMutator(DbContext context) : ExpressionMutator(context)
{
    private readonly ExpressionFinder _expressionFinder = new();

    public override bool IsValid(Expression expression)
    {
        _expressionFinder.Visit(expression);

        return _expressionFinder.FoundExpressions.Any();
    }

    public override Expression Apply(Expression expression, Random random)
    {
        var i = random.Next(_expressionFinder.FoundExpressions.Count);
        var methodNames = new[] { nameof(string.ToLower), nameof(string.ToUpper), nameof(string.Trim) };

        var methodInfos = methodNames.Select(n => typeof(string).GetRuntimeMethod(n, [])!).ToList();
        var methodInfo = methodInfos[random.Next(methodInfos.Count)];

        var injector = new ExpressionInjector(_expressionFinder.FoundExpressions[i], e => Expression.Call(e, methodInfo));

        return injector.Visit(expression);
    }

    private class ExpressionFinder : ExpressionVisitor
    {
        private bool _insideLambda;
        private bool _insideEFProperty;

        public List<Expression> FoundExpressions { get; } = [];

        [return: NotNullIfNotNull(nameof(node))]
        public override Expression? Visit(Expression? node)
        {
            if (_insideLambda
                && !_insideEFProperty
                && node?.Type == typeof(string)
                && node.NodeType != ExpressionType.Parameter
                && (node.NodeType != ExpressionType.Constant || ((ConstantExpression)node)?.Value != null))
            {
                FoundExpressions.Add(node);
            }

            return base.Visit(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.IsEFPropertyMethod())
            {
                var oldInsideEFProperty = _insideEFProperty;
                _insideEFProperty = true;

                var result = base.VisitMethodCall(node);

                _insideEFProperty = oldInsideEFProperty;

                return result;
            }

            return base.VisitMethodCall(node);
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
