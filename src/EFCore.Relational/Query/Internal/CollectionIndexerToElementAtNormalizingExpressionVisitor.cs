// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CollectionIndexerToElementAtNormalizingExpressionVisitor : ExpressionVisitor
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        // Convert list[x] to list.ElementAt(x)
        if (methodCallExpression.Method is { Name: "get_Item", IsStatic: false, DeclaringType: { IsGenericType: true } declaringType }
            && declaringType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var source = Visit(methodCallExpression.Object!);
            var index = Visit(methodCallExpression.Arguments[0]);
            var sourceTypeArgument = source.Type.GetSequenceType();

            return Expression.Call(
                QueryableMethods.ElementAt.MakeGenericMethod(sourceTypeArgument),
                    Expression.Call(
                        QueryableMethods.AsQueryable.MakeGenericMethod(sourceTypeArgument),
                        source),
                    index);
        }

        return base.VisitMethodCall(methodCallExpression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
        // Convert array[x] to array.ElementAt(x)
        if (binaryExpression.NodeType == ExpressionType.ArrayIndex)
        {
            var source = Visit(binaryExpression.Left);
            var index = Visit(binaryExpression.Right);
            var sourceTypeArgument = source.Type.GetSequenceType();

            return Expression.Call(
                QueryableMethods.ElementAt.MakeGenericMethod(sourceTypeArgument),
                    Expression.Call(
                        QueryableMethods.AsQueryable.MakeGenericMethod(sourceTypeArgument),
                        source),
                    index);
        }

        return base.VisitBinary(binaryExpression);
    }
}
