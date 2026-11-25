// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteDynamicEqualsTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
{
    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method.DeclaringType != typeof(object) || method.Name != nameof(object.Equals))
        {
            return null;
        }

        var (left, right) = (instance, arguments) switch
        {
            (not null, [var argument]) => (instance, argument),
            (null, [var argument1, var argument2]) => (argument1, argument2),
            _ => (null, null)
        };

        if (left is null || right is null
            || (left.TypeMapping is not SqliteDynamicTypeMapping && right.TypeMapping is not SqliteDynamicTypeMapping))
        {
            return null;
        }

        if (right.TypeMapping is SqliteDynamicTypeMapping && left.TypeMapping is not SqliteDynamicTypeMapping)
        {
            (left, right) = (right, left);
        }

        if (right is SqlConstantExpression { Value: null })
        {
            return sqlExpressionFactory.IsNull(left);
        }

        right = sqlExpressionFactory.ApplyDefaultTypeMapping(right)!;
        if (right.TypeMapping?.Converter is not null)
        {
            throw new InvalidOperationException(CoreStrings.TranslationFailed(method.Name));
        }

        SqlExpression typeMatches;
        if (right.TypeMapping is SqliteDynamicTypeMapping)
        {
            if (left is not SqlFunctionExpression { Name: "json_extract" }
                || right is not SqlFunctionExpression { Name: "json_extract" })
            {
                throw new InvalidOperationException(CoreStrings.TranslationFailed(method.Name));
            }

            typeMatches = sqlExpressionFactory.Equal(SqliteTypeOf(left), SqliteTypeOf(right));
        }
        else
        {
            var storeType = right.Type.UnwrapNullableType() switch
            {
                var type when type == typeof(long) => "integer",
                var type when type == typeof(double) => "real",
                var type when type == typeof(string) => "text",
                _ => null
            };

            if (storeType is null)
            {
                return right.Type == typeof(object) || right.Type == typeof(byte[])
                    ? throw new InvalidOperationException(CoreStrings.TranslationFailed(method.Name))
                    : sqlExpressionFactory.AndAlso(sqlExpressionFactory.IsNull(left), sqlExpressionFactory.IsNull(right));
            }

            typeMatches = sqlExpressionFactory.Equal(SqliteTypeOf(left), sqlExpressionFactory.Constant(storeType));
        }

        return sqlExpressionFactory.OrElse(
            sqlExpressionFactory.AndAlso(sqlExpressionFactory.IsNull(left), sqlExpressionFactory.IsNull(right)),
            sqlExpressionFactory.AndAlso(typeMatches, sqlExpressionFactory.Equal(left, right)));

        SqlExpression SqliteTypeOf(SqlExpression expression)
            => sqlExpressionFactory.Function(
                "typeof",
                [expression],
                nullable: false,
                argumentsPropagateNullability: [false],
                typeof(string));
    }
}
