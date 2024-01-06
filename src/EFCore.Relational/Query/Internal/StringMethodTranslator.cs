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
public class StringMethodTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo IsNullOrEmptyMethodInfo
        = typeof(string).GetRuntimeMethod(nameof(string.IsNullOrEmpty), [typeof(string)])!;

    private static readonly MethodInfo ConcatMethodInfoTwoArgs
        = typeof(string).GetRuntimeMethod(nameof(string.Concat), [typeof(string), typeof(string)])!;

    private static readonly MethodInfo ConcatMethodInfoThreeArgs
        = typeof(string).GetRuntimeMethod(nameof(string.Concat), [typeof(string), typeof(string), typeof(string)])!;

    private static readonly MethodInfo ConcatMethodInfoFourArgs
        = typeof(string).GetRuntimeMethod(
            nameof(string.Concat), [typeof(string), typeof(string), typeof(string), typeof(string)])!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public StringMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (Equals(method, IsNullOrEmptyMethodInfo))
        {
            var argument = arguments[0];

            return _sqlExpressionFactory.OrElse(
                _sqlExpressionFactory.IsNull(argument),
                _sqlExpressionFactory.Equal(
                    argument,
                    _sqlExpressionFactory.Constant(string.Empty)));
        }

        if (Equals(method, ConcatMethodInfoTwoArgs))
        {
            return _sqlExpressionFactory.Add(
                arguments[0],
                arguments[1]);
        }

        if (Equals(method, ConcatMethodInfoThreeArgs))
        {
            return _sqlExpressionFactory.Add(
                arguments[0],
                _sqlExpressionFactory.Add(
                    arguments[1],
                    arguments[2]));
        }

        if (Equals(method, ConcatMethodInfoFourArgs))
        {
            return _sqlExpressionFactory.Add(
                arguments[0],
                _sqlExpressionFactory.Add(
                    arguments[1],
                    _sqlExpressionFactory.Add(
                        arguments[2],
                        arguments[3])));
        }

        return null;
    }
}
