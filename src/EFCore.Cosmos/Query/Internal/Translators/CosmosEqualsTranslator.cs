// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosEqualsTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        SqlExpression? left = null;
        SqlExpression? right = null;

        if (method.Name == nameof(object.Equals)
            && instance != null
            && arguments.Count == 1)
        {
            left = instance;
            right = arguments[0];
        }
        else if (instance == null
                 && method.Name == nameof(object.Equals)
                 && arguments.Count == 2)
        {
            left = arguments[0];
            right = arguments[1];
        }

        if (left != null
            && right != null)
        {
            return left.Type.UnwrapNullableType() == right.Type.UnwrapNullableType()
                || (right.Type == typeof(object) && (right is SqlParameterExpression or SqlConstantExpression))
                || (left.Type == typeof(object) && (left is SqlParameterExpression or SqlConstantExpression))
                    ? sqlExpressionFactory.Equal(left, right)
                    : sqlExpressionFactory.Constant(false);
        }

        return null;
    }
}
