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
public class SqlServerByteArrayMethodTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
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
        if (method.IsGenericMethod
            && method.DeclaringType == typeof(Enumerable))
        {
            switch (method.Name)
            {
                case nameof(Enumerable.Contains) when arguments is [var source, var item] && source.Type == typeof(byte[]):
                    return sqlExpressionFactory.GreaterThan(
                        sqlExpressionFactory.Function(
                            "CHARINDEX",
                            [ToSingleByteArray(item, source), source],
                            nullable: true,
                            argumentsPropagateNullability: Statics.TrueArrays[2],
                            typeof(int)),
                        sqlExpressionFactory.Constant(0));

                // First without a predicate
                case nameof(Enumerable.First) when arguments is [var source] && source.Type == typeof(byte[]):
                    return sqlExpressionFactory.Convert(
                        sqlExpressionFactory.Function(
                            "SUBSTRING",
                            [source, sqlExpressionFactory.Constant(1), sqlExpressionFactory.Constant(1)],
                            nullable: true,
                            argumentsPropagateNullability: Statics.TrueArrays[3],
                            typeof(byte[])),
                        method.ReturnType);

                // Any without a predicate
                case nameof(Enumerable.Any) when arguments is [var source] && source.Type == typeof(byte[]):
                    return sqlExpressionFactory.GreaterThan(
                        sqlExpressionFactory.Function(
                            "DATALENGTH",
                            [source],
                            nullable: true,
                            argumentsPropagateNullability: Statics.TrueArrays[1],
                            typeof(int)),
                        sqlExpressionFactory.Constant(0));
            }
        }
        else if (method.IsGenericMethod
            && method.DeclaringType == typeof(Array)
            && method.Name == nameof(Array.IndexOf)
            && arguments is [var array, var searchItem]
            && array.Type == typeof(byte[]))
        {
            // CHARINDEX is 1-based and returns 0 when not found; Array.IndexOf is 0-based and returns -1.
            // CHARINDEX returns bigint when the searched expression is varbinary(max), so convert it back to int.
            return sqlExpressionFactory.Subtract(
                sqlExpressionFactory.Convert(
                    sqlExpressionFactory.Function(
                        "CHARINDEX",
                        [ToSingleByteArray(searchItem, array), array],
                        nullable: true,
                        argumentsPropagateNullability: Statics.TrueArrays[2],
                        typeof(long)),
                    typeof(int)),
                sqlExpressionFactory.Constant(1));
        }

        return null;
    }

    private SqlExpression ToSingleByteArray(SqlExpression item, SqlExpression source)
        => item is SqlConstantExpression constantValue
            ? sqlExpressionFactory.Constant(new[] { (byte)constantValue.Value! }, source.TypeMapping)
            : sqlExpressionFactory.Convert(item, typeof(byte[]), source.TypeMapping);
}
