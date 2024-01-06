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
public class SqlServerStringAggregateMethodTranslator : IAggregateMethodCallTranslator
{
    private static readonly MethodInfo StringConcatMethod
        = typeof(string).GetRuntimeMethod(nameof(string.Concat), [typeof(IEnumerable<string>)])!;

    private static readonly MethodInfo StringJoinMethod
        = typeof(string).GetRuntimeMethod(nameof(string.Join), [typeof(string), typeof(IEnumerable<string>)])!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerStringAggregateMethodTranslator(
        ISqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _typeMappingSource = typeMappingSource;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        MethodInfo method,
        EnumerableExpression source,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        // Docs: https://docs.microsoft.com/sql/t-sql/functions/string-agg-transact-sql

        if (source.Selector is not SqlExpression sqlExpression
            || (method != StringJoinMethod && method != StringConcatMethod))
        {
            return null;
        }

        // STRING_AGG enlarges the return type size (e.g. for input VARCHAR(5), it returns VARCHAR(8000)).
        // See https://docs.microsoft.com/sql/t-sql/functions/string-agg-transact-sql#return-types
        var resultTypeMapping = sqlExpression.TypeMapping;
        if (resultTypeMapping?.Size != null)
        {
            if (resultTypeMapping is { IsUnicode: true, Size: < 4000 })
            {
                resultTypeMapping = _typeMappingSource.FindMapping(
                    typeof(string),
                    resultTypeMapping.StoreTypeNameBase,
                    unicode: true,
                    size: 4000);
            }
            else if (resultTypeMapping is { IsUnicode: false, Size: < 8000 })
            {
                resultTypeMapping = _typeMappingSource.FindMapping(
                    typeof(string),
                    resultTypeMapping.StoreTypeNameBase,
                    unicode: false,
                    size: 8000);
            }
        }

        // STRING_AGG filters out nulls, but string.Join treats them as empty strings; coalesce unless we know we're aggregating over
        // a non-nullable column.
        if (sqlExpression is not ColumnExpression { IsNullable: false })
        {
            sqlExpression = _sqlExpressionFactory.Coalesce(
                sqlExpression,
                _sqlExpressionFactory.Constant(string.Empty, typeof(string)));
        }

        // STRING_AGG returns null when there are no rows (or non-null values), but string.Join returns an empty string.
        return
            _sqlExpressionFactory.Coalesce(
                SqlServerExpression.AggregateFunctionWithOrdering(
                    _sqlExpressionFactory,
                    "STRING_AGG",
                    new[]
                    {
                        sqlExpression,
                        _sqlExpressionFactory.ApplyTypeMapping(
                            method == StringJoinMethod ? arguments[0] : _sqlExpressionFactory.Constant(string.Empty, typeof(string)),
                            sqlExpression.TypeMapping)
                    },
                    source,
                    enumerableArgumentIndex: 0,
                    nullable: true,
                    argumentsPropagateNullability: new[] { false, true },
                    typeof(string)),
                _sqlExpressionFactory.Constant(string.Empty, typeof(string)),
                resultTypeMapping);
    }
}
