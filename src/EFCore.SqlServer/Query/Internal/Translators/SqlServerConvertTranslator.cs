// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerConvertTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
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
        if (method.DeclaringType != typeof(Convert))
        {
            return null;
        }

        var sqlType = method.Name switch
        {
            nameof(Convert.ToBoolean) => "bit",
            nameof(Convert.ToByte) => "tinyint",
            nameof(Convert.ToDecimal) => "decimal(18, 2)",
            nameof(Convert.ToDouble) => "float",
            nameof(Convert.ToInt16) => "smallint",
            nameof(Convert.ToInt32) => "int",
            nameof(Convert.ToInt64) => "bigint",
            nameof(Convert.ToString) => "nvarchar(max)",
            _ => null
        };

        if (sqlType is null
            || method.GetParameters() is not [{ ParameterType: var paramType }]
            || !IsSupportedType(paramType))
        {
            return null;
        }

        return sqlExpressionFactory.Function(
            "CONVERT",
            [sqlExpressionFactory.Fragment(sqlType), arguments[0]],
            nullable: true,
            argumentsPropagateNullability: Statics.FalseTrue,
            method.ReturnType);
    }

    private static bool IsSupportedType(Type type)
        => type == typeof(bool)
            || type == typeof(byte)
            || type == typeof(DateTime)
            || type == typeof(decimal)
            || type == typeof(double)
            || type == typeof(float)
            || type == typeof(int)
            || type == typeof(long)
            || type == typeof(short)
            || type == typeof(string)
            || type == typeof(object);
}
