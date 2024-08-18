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
            && arguments.Count >= 1
            && arguments[0].Type == typeof(byte[]))
        {
            var methodDefinition = method.GetGenericMethodDefinition();
            if (methodDefinition.Equals(EnumerableMethods.Contains))
            {
                var source = arguments[0];
                var sourceTypeMapping = source.TypeMapping;

                var value = arguments[1] is SqlConstantExpression constantValue
                    ? _sqlExpressionFactory.Constant(new[] { (byte)constantValue.Value! }, sourceTypeMapping)
                    : _sqlExpressionFactory.Convert(arguments[1], typeof(byte[]), sourceTypeMapping);

                return _sqlExpressionFactory.GreaterThan(
                    _sqlExpressionFactory.Function(
                        "CHARINDEX",
                        [value, source],
                        nullable: true,
                        argumentsPropagateNullability: [true, true],
                        typeof(int)),
                    _sqlExpressionFactory.Constant(0));
            }

            if (methodDefinition.Equals(EnumerableMethods.FirstWithoutPredicate))
            {
                return _sqlExpressionFactory.Convert(
                _sqlExpressionFactory.Function(
                    "SUBSTRING",
                    [arguments[0], _sqlExpressionFactory.Constant(1), _sqlExpressionFactory.Constant(1)],
                    nullable: true,
                    argumentsPropagateNullability: [true, true, true],
                    typeof(byte[])),
                method.ReturnType);
            }

            if (methodDefinition.Equals(ArrayMethods.IndexOf))
            {
                return TranslateIndexOf(method, arguments[0], arguments[1], null);
            }

            if (methodDefinition.Equals(ArrayMethods.IndexOfWithStartingPosition))
            {
                return TranslateIndexOf(method, arguments[0], arguments[1], arguments[2]);
            }
        }

        return null;
    }

    private SqlExpression TranslateIndexOf(
        MethodInfo method,
        SqlExpression source,
        SqlExpression valueToSearch,
        SqlExpression? startIndex
    )
    {
        var sourceTypeMapping = source.TypeMapping;
        var sqlArguments = new List<SqlExpression>
        {
            valueToSearch is SqlConstantExpression { Value: byte constantValue }
            ? _sqlExpressionFactory.Constant(new byte[] { constantValue }, sourceTypeMapping)
            : _sqlExpressionFactory.Convert(valueToSearch, typeof(byte[]), sourceTypeMapping),
            source
        };

        if (startIndex is not null)
        {
            sqlArguments.Add(
                startIndex is SqlConstantExpression { Value : int index }
                ? _sqlExpressionFactory.Constant(index + 1, typeof(int))
                : _sqlExpressionFactory.Add(startIndex, _sqlExpressionFactory.Constant(1))
            );
        }

        var argumentsPropagateNullability = Enumerable.Repeat(true, sqlArguments.Count);

        SqlExpression charIndexExpr;
        var storeType = sourceTypeMapping?.StoreType;
        if (storeType == "varbinary(max)")
        {
            charIndexExpr = _sqlExpressionFactory.Function(
                "CHARINDEX",
                sqlArguments,
                nullable: true,
                argumentsPropagateNullability: argumentsPropagateNullability,
                typeof(long));

            charIndexExpr = _sqlExpressionFactory.Convert(charIndexExpr, typeof(int));
        }
        else
        {
            charIndexExpr = _sqlExpressionFactory.Function(
                "CHARINDEX",
                sqlArguments,
                nullable: true,
                argumentsPropagateNullability: argumentsPropagateNullability,
                method.ReturnType);
        }


        return _sqlExpressionFactory.Subtract(charIndexExpr, _sqlExpressionFactory.Constant(1));
    }
}
