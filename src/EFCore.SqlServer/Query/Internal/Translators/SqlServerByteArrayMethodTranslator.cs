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
                {
                    var sourceTypeMapping = source.TypeMapping;

                    var value = item is SqlConstantExpression constantValue
                        ? sqlExpressionFactory.Constant(new[] { (byte)constantValue.Value! }, sourceTypeMapping)
                        : sqlExpressionFactory.Convert(item, typeof(byte[]), sourceTypeMapping);

                    return sqlExpressionFactory.GreaterThan(
                        sqlExpressionFactory.Function(
                            "CHARINDEX",
                            [value, source],
                            nullable: true,
                            argumentsPropagateNullability: Statics.TrueArrays[2],
                            typeof(int)),
                        sqlExpressionFactory.Constant(0));
                }

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
            }
        }

        return null;
    }
}
