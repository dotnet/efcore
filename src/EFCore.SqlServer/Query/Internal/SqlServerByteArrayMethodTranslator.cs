// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerByteArrayMethodTranslator : IMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerByteArrayMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
            && arguments[0].Type == typeof(byte[]))
        {
            var source = arguments[0];
            var sourceTypeMapping = source.TypeMapping;

            var value = arguments[1] is SqlConstantExpression constantValue
                ? (SqlExpression)_sqlExpressionFactory.Constant(new[] { (byte)constantValue.Value! }, sourceTypeMapping)
                : _sqlExpressionFactory.Convert(arguments[1], typeof(byte[]), sourceTypeMapping);

            return _sqlExpressionFactory.GreaterThan(
                _sqlExpressionFactory.Function(
                    "CHARINDEX",
                    new[] { value, source },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true, true },
                    typeof(int)),
                _sqlExpressionFactory.Constant(0));
        }

        if (method.IsGenericMethod
            && method.GetGenericMethodDefinition().Equals(EnumerableMethods.FirstWithoutPredicate)
            && arguments[0].Type == typeof(byte[]))
        {
            return _sqlExpressionFactory.Convert(
                _sqlExpressionFactory.Function(
                    "SUBSTRING",
                    new[] { arguments[0], _sqlExpressionFactory.Constant(1), _sqlExpressionFactory.Constant(1) },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true, true, true },
                    typeof(byte[])),
                method.ReturnType);
        }

        return null;
    }
}
