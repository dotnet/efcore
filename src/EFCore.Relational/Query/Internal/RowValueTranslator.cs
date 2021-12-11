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
public class RowValueTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo _lessThanMethodInfo =
        typeof(RelationalDbFunctionsExtensions).GetRequiredRuntimeMethod(
            nameof(RelationalDbFunctionsExtensions.LessThan),
            typeof(DbFunctions), typeof(object[]), typeof(object[]));

    private static readonly MethodInfo _lessThanOrEqualMethodInfo =
        typeof(RelationalDbFunctionsExtensions).GetRequiredRuntimeMethod(
            nameof(RelationalDbFunctionsExtensions.LessThanOrEqual),
            typeof(DbFunctions), typeof(object[]), typeof(object[]));

    private static readonly MethodInfo _greaterThanMethodInfo =
        typeof(RelationalDbFunctionsExtensions).GetRequiredRuntimeMethod(
            nameof(RelationalDbFunctionsExtensions.GreaterThan),
            typeof(DbFunctions), typeof(object[]), typeof(object[]));

    private static readonly MethodInfo _greaterThanOrEqualMethodInfo =
        typeof(RelationalDbFunctionsExtensions).GetRequiredRuntimeMethod(
            nameof(RelationalDbFunctionsExtensions.GreaterThanOrEqual),
            typeof(DbFunctions), typeof(object[]), typeof(object[]));

    private static readonly Dictionary<MethodInfo, ExpressionType> _methodInfoOperatorTypeMap =
        new()
        {
            { _lessThanMethodInfo, ExpressionType.LessThan },
            { _lessThanOrEqualMethodInfo, ExpressionType.LessThanOrEqual },
            { _greaterThanMethodInfo, ExpressionType.GreaterThan },
            { _greaterThanOrEqualMethodInfo, ExpressionType.GreaterThanOrEqual },
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RowValueTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        SqlExpressionFactory = sqlExpressionFactory;
    }

    /// <summary>
    /// The <see cref="ISqlExpressionFactory"/>.
    /// </summary>
    protected ISqlExpressionFactory SqlExpressionFactory { get; }

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
        if (_methodInfoOperatorTypeMap.TryGetValue(method, out var operatorType))
        {
            var columns = UnwrapColumns(arguments[1]);
            var values = UnwrapValues(arguments[2]);
            return CreateSqlExpression(operatorType, columns, values);
        }

        return null;
    }

    private IReadOnlyList<SqlExpression> UnwrapColumns(SqlExpression sqlExpression)
    {
        return ((ArrayExpression)sqlExpression).Values
            .Select(x => RemoveObjectConvert(x))
            .ToList();
    }

    private IReadOnlyList<object> UnwrapValues(SqlExpression sqlExpression)
    {
        var constantValue = ((SqlConstantExpression)sqlExpression).Value!;
        var valuesArray = (object[])constantValue;
        return valuesArray.ToList();
    }

    /// <summary>
    ///     Creates the sql expression from the row value info.
    /// </summary>
    protected virtual SqlExpression? CreateSqlExpression(
        ExpressionType operatorType,
        IReadOnlyList<SqlExpression> columns,
        IReadOnlyList<object> values)
        => SqlExpressionFactory.RowValue(operatorType, columns, values);

    private SqlExpression RemoveObjectConvert(SqlExpression expression)
        => expression is SqlUnaryExpression sqlUnaryExpression
            && sqlUnaryExpression.OperatorType == ExpressionType.Convert
            && sqlUnaryExpression.Type == typeof(object)
                ? sqlUnaryExpression.Operand
                : expression;
}
