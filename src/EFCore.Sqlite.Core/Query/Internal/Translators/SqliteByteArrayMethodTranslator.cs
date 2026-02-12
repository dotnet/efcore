// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteByteArrayMethodTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
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
            && method.DeclaringType == typeof(Enumerable)
            && method.Name == nameof(Enumerable.Contains)
            && arguments is [var source, var item]
            && source.Type == typeof(byte[]))
        {
            var value = item is SqlConstantExpression constantValue
                ? sqlExpressionFactory.Constant(new[] { (byte)constantValue.Value! }, source.TypeMapping)
                : sqlExpressionFactory.Function(
                    "char",
                    [item],
                    nullable: false,
                    argumentsPropagateNullability: Statics.FalseArrays[1],
                    typeof(string));

            return sqlExpressionFactory.GreaterThan(
                sqlExpressionFactory.Function(
                    "instr",
                    [source, value],
                    nullable: true,
                    argumentsPropagateNullability: Statics.TrueArrays[2],
                    typeof(int)),
                sqlExpressionFactory.Constant(0));
        }

        return null;
    }

    // See issue#16428
    //if (method.IsGenericMethod
    //    && method.GetGenericMethodDefinition().Equals(EnumerableMethods.FirstWithoutPredicate)
    //    && arguments[0].Type == typeof(byte[]))
    //{
    //    return _sqlExpressionFactory.Function(
    //        "unicode",
    //        new SqlExpression[]
    //        {
    //            _sqlExpressionFactory.Function(
    //                "substr",
    //                new SqlExpression[]
    //                {
    //                    arguments[0],
    //                    _sqlExpressionFactory.Constant(1),
    //                    _sqlExpressionFactory.Constant(1)
    //                },
    //                nullable: true,
    //                argumentsPropagateNullability: new[] { true, true, true },
    //                typeof(byte[]))
    //        },
    //        nullable: true,
    //        argumentsPropagateNullability: new[] { true },
    //        method.ReturnType);
    //}
}
