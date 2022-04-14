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
public class ComparisonTranslator : IMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ComparisonTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (method.ReturnType == typeof(int))
        {
            SqlExpression? left = null;
            SqlExpression? right = null;
            if (method.Name == nameof(string.Compare)
                && arguments.Count == 2
                && arguments[0].Type == arguments[1].Type)
            {
                left = arguments[0];
                right = arguments[1];
            }
            else if (method.Name == nameof(string.CompareTo)
                     && arguments.Count == 1
                     && instance != null
                     && instance.Type == arguments[0].Type)
            {
                left = instance;
                right = arguments[0];
            }

            if (left != null
                && right != null)
            {
                return _sqlExpressionFactory.Case(
                    new[]
                    {
                        new CaseWhenClause(
                            _sqlExpressionFactory.Equal(left, right), _sqlExpressionFactory.Constant(0)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.GreaterThan(left, right), _sqlExpressionFactory.Constant(1)),
                        new CaseWhenClause(
                            _sqlExpressionFactory.LessThan(left, right), _sqlExpressionFactory.Constant(-1))
                    },
                    null);
            }
        }

        return null;
    }
}
