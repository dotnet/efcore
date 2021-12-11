// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ContainsTranslator : IMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ContainsTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

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
        if (method.IsGenericMethod
            && method.GetGenericMethodDefinition().Equals(EnumerableMethods.Contains)
            && ValidateValues(arguments[0]))
        {
            return _sqlExpressionFactory.In(RemoveObjectConvert(arguments[1]), arguments[0], negated: false);
        }

        if (arguments.Count == 1
            && method.IsContainsMethod()
            && instance != null
            && ValidateValues(instance))
        {
            return _sqlExpressionFactory.In(RemoveObjectConvert(arguments[0]), instance, negated: false);
        }

        return null;
    }

    private static bool ValidateValues(SqlExpression values)
        => values is SqlConstantExpression || values is SqlParameterExpression;

    private static SqlExpression RemoveObjectConvert(SqlExpression expression)
        => expression is SqlUnaryExpression sqlUnaryExpression
            && sqlUnaryExpression.OperatorType == ExpressionType.Convert
            && sqlUnaryExpression.Type == typeof(object)
                ? sqlUnaryExpression.Operand
                : expression;
}
