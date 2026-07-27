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
public class SqliteObjectToStringTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
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
        if (instance == null || method.Name != nameof(ToString) || arguments.Count != 0)
        {
            return null;
        }

        if (instance.TypeMapping?.ClrType == typeof(string))
        {
            return instance;
        }

        if (instance.Type == typeof(bool))
        {
            if (instance is not ColumnExpression { IsNullable: false })
            {
                return sqlExpressionFactory.Case(
                    instance,
                    [
                        new CaseWhenClause(
                            sqlExpressionFactory.Constant(false),
                            sqlExpressionFactory.Constant(false.ToString())),
                        new CaseWhenClause(
                            sqlExpressionFactory.Constant(true),
                            sqlExpressionFactory.Constant(true.ToString()))
                    ],
                    sqlExpressionFactory.Constant(string.Empty));
            }

            return sqlExpressionFactory.Case(
                [
                    new CaseWhenClause(
                        instance,
                        sqlExpressionFactory.Constant(true.ToString()))
                ],
                sqlExpressionFactory.Constant(false.ToString()));
        }

        // Enums are handled by EnumMethodTranslator

        return IsSupportedType(instance.Type)
            ? sqlExpressionFactory.Coalesce(
                sqlExpressionFactory.Convert(instance, typeof(string)),
                sqlExpressionFactory.Constant(string.Empty))
            : null;
    }

    private static bool IsSupportedType(Type type)
        => type == typeof(byte)
            || type == typeof(byte[])
            || type == typeof(char)
            || type == typeof(DateOnly)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(decimal)
            || type == typeof(double)
            || type == typeof(float)
            || type == typeof(Guid)
            || type == typeof(int)
            || type == typeof(long)
            || type == typeof(sbyte)
            || type == typeof(short)
            || type == typeof(TimeOnly)
            || type == typeof(TimeSpan)
            || type == typeof(uint)
            || type == typeof(ushort);
}
