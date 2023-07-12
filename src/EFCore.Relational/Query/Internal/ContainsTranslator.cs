// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
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
        // Note that almost all forms of Contains are queryable (e.g. over inline/parameter collections), and translated in
        // RelationalQueryableMethodTranslatingExpressionVisitor.TranslateContains.
        // This enumerable Contains translation is still needed for entity Contains (#30712)
        SqlExpression? itemExpression = null, valuesExpression = null;

        // Identify static Enumerable.Contains and instance List.Contains
        if (method.IsGenericMethod
            && method.GetGenericMethodDefinition() == EnumerableMethods.Contains
            && ValidateValues(arguments[0]))
        {
            (itemExpression, valuesExpression) = (RemoveObjectConvert(arguments[1]), arguments[0]);
        }

        if (arguments.Count == 1
            && method.IsContainsMethod()
            && instance != null
            && ValidateValues(instance))
        {
            (itemExpression, valuesExpression) = (RemoveObjectConvert(arguments[0]), instance);
        }

        if (itemExpression is not null && valuesExpression is not null)
        {
            switch (valuesExpression)
            {
                case SqlParameterExpression parameter:
                    return _sqlExpressionFactory.In(itemExpression, parameter);

                case SqlConstantExpression { Value: IEnumerable values }:
                    var valuesExpressions = new List<SqlExpression>();

                    foreach (var value in values)
                    {
                        valuesExpressions.Add(_sqlExpressionFactory.Constant(value));
                    }

                    return _sqlExpressionFactory.In(itemExpression, valuesExpressions);
            }
        }

        return null;
    }

    private static bool ValidateValues(SqlExpression values)
        => values is SqlConstantExpression or SqlParameterExpression;

    private static SqlExpression RemoveObjectConvert(SqlExpression expression)
        => expression is SqlUnaryExpression { OperatorType: ExpressionType.Convert } sqlUnaryExpression
            && sqlUnaryExpression.Type == typeof(object)
                ? sqlUnaryExpression.Operand
                : expression;
}
