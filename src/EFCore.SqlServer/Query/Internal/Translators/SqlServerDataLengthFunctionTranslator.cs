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
public class SqlServerDataLengthFunctionTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
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
        if (method.DeclaringType != typeof(SqlServerDbFunctionsExtensions)
            || method.Name != nameof(SqlServerDbFunctionsExtensions.DataLength))
        {
            return null;
        }

        var argument = arguments[1];
        if (argument.TypeMapping == null)
        {
            argument = sqlExpressionFactory.ApplyDefaultTypeMapping(argument);
        }

        if (argument.TypeMapping!.StoreType is "nvarchar(max)" or "varchar(max)" or "varbinary(max)")
        {
            var result = sqlExpressionFactory.Function(
                "DATALENGTH",
                arguments.Skip(1),
                nullable: true,
                argumentsPropagateNullability: Statics.TrueArrays[1],
                typeof(long));

            return sqlExpressionFactory.Convert(result, method.ReturnType.UnwrapNullableType());
        }

        return sqlExpressionFactory.Function(
            "DATALENGTH",
            arguments.Skip(1),
            nullable: true,
            argumentsPropagateNullability: Statics.TrueArrays[1],
            method.ReturnType.UnwrapNullableType());
    }
}
