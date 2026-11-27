// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InMemoryQueryTranslationPreprocessor : QueryTranslationPreprocessor
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InMemoryQueryTranslationPreprocessor(
        QueryTranslationPreprocessorDependencies dependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, queryCompilationContext)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression Process(Expression query)
    {
        var result = base.Process(query);

        return result is MethodCallExpression { Method.IsGenericMethod: true } methodCallExpression
            && (methodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.GroupByWithKeySelector
                || methodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.GroupByWithKeyElementSelector)
                ? throw new InvalidOperationException(
                    CoreStrings.TranslationFailedWithDetails(methodCallExpression.Print(), InMemoryStrings.NonComposedGroupByNotSupported))
                : result;
    }

#pragma warning disable EF1001, EF9002 // Use the core query normalizer and the provider's constant support.
    /// <inheritdoc />
    public override Expression NormalizeQueryableMethod(Expression expression)
        => ProcessQueryRoots(new InMemoryNormalizingVisitor(QueryCompilationContext, IsEfConstantSupported).Normalize(expression));

    private sealed class InMemoryNormalizingVisitor(QueryCompilationContext context, bool supportsConstants)
        : QueryableMethodNormalizingExpressionVisitor(context, supportsConstants)
    {
        protected override MethodCallExpression TryNormalizeMaxByMinBy(MethodCallExpression call)
        {
            var method = call.Method.GetGenericMethodDefinition();
            if (method != QueryableMethods.MinBy && method != QueryableMethods.MaxBy)
            {
                return call;
            }

            var selector = call.Arguments[1].UnwrapLambdaFromQuote();
            var sourceType = call.Method.GetGenericArguments()[0];
            var minimum = method == QueryableMethods.MinBy;
            // Descending CLR ordering already puts nulls last. For ascending order, construct
            // the composite sort key inside one call to avoid evaluating the selector twice.
            var key = minimum && selector.ReturnType.IsNullableType()
                ? Expression.Lambda(
                    Expression.Call(typeof(InMemoryNormalizingVisitor), nameof(NullsLast), [selector.ReturnType], selector.Body),
                    selector.Parameters)
                : selector;
            var order = minimum ? QueryableMethods.OrderBy : QueryableMethods.OrderByDescending;
            var first = sourceType.IsNullableType() ? QueryableMethods.FirstOrDefaultWithoutPredicate : QueryableMethods.FirstWithoutPredicate;
            return Expression.Call(
                first.MakeGenericMethod(sourceType),
                Expression.Call(order.MakeGenericMethod(sourceType, key.ReturnType), call.Arguments[0], Expression.Quote(key)));
        }

        private static (bool, TKey) NullsLast<TKey>(TKey key)
            => (key is null, key);
    }

#pragma warning restore EF1001, EF9002

    /// <inheritdoc />
    protected override bool IsEfConstantSupported
        => true;
}
