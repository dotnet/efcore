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
public class SqliteByteArrayMethodTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo ArrayIndexOf
        = typeof(Array).GetMethod(nameof(Array.IndexOf), 1, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, CallingConventions.Any, [Type.MakeGenericMethodParameter(0).MakeArrayType(), Type.MakeGenericMethodParameter(0)], null)!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteByteArrayMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
            && arguments.Count >= 1
            && arguments[0].Type == typeof(byte[]))
        {
            var genericMethodDefinition = method.GetGenericMethodDefinition();
            if (genericMethodDefinition.Equals(EnumerableMethods.Contains))
            {
                return _sqlExpressionFactory.GreaterThan(
                        GetInStrSqlFunctionExpression(arguments[0], arguments[1]),
                    _sqlExpressionFactory.Constant(0));

            }

            if (genericMethodDefinition.Equals(ArrayIndexOf))
            {
                return _sqlExpressionFactory.Subtract(
                        GetInStrSqlFunctionExpression(arguments[0], arguments[1]),
                    _sqlExpressionFactory.Constant(1));
            }

            // NOTE: IndexOf Method with a starting position is not supported by SQLite
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

        return null;

        SqlExpression GetInStrSqlFunctionExpression(SqlExpression source, SqlExpression valueToSearch)
        {
            var value = valueToSearch is SqlConstantExpression { Value: byte constantValue }
                ? _sqlExpressionFactory.Constant(new byte[] { constantValue }, source.TypeMapping)
                : _sqlExpressionFactory.Function(
                    "char",
                    [valueToSearch],
                    nullable: false,
                    argumentsPropagateNullability: [false],
                    typeof(string));

            return _sqlExpressionFactory.Function(
                "instr",
                [source, value],
                nullable: true,
                argumentsPropagateNullability: [true, true],
                typeof(int));
        }
    }
}
