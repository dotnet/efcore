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
public class SqlServerStringAggregateMethodTranslator(
    ISqlExpressionFactory sqlExpressionFactory,
    IRelationalTypeMappingSource typeMappingSource) : IAggregateMethodCallTranslator
{
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
            || method.DeclaringType != typeof(string))
        {
            return null;
        }

        SqlExpression separator;
        switch (method.Name)
        {
            case nameof(string.Concat) when arguments is []:
                separator = sqlExpressionFactory.Constant(string.Empty, typeof(string));
                break;
            case nameof(string.Join) when arguments is [var sep]:
                separator = sep;
                break;
            default:
                return null;
        }

        // STRING_AGG enlarges the return type size (e.g. for input VARCHAR(5), it returns VARCHAR(8000)).
        // See https://docs.microsoft.com/sql/t-sql/functions/string-agg-transact-sql#return-types
        var resultTypeMapping = sqlExpression.TypeMapping;
        if (resultTypeMapping?.Size != null)
        {
            if (resultTypeMapping is { IsUnicode: true, Size: < 4000 })
            {
                resultTypeMapping = typeMappingSource.FindMapping(
                    typeof(string),
                    resultTypeMapping.StoreTypeNameBase,
                    unicode: true,
                    size: 4000);
            }
            else if (resultTypeMapping is { IsUnicode: false, Size: < 8000 })
            {
                resultTypeMapping = typeMappingSource.FindMapping(
                    typeof(string),
                    resultTypeMapping.StoreTypeNameBase,
                    unicode: false,
                    size: 8000);
            }
        }

        // STRING_AGG filters out nulls, but string.Join treats them as empty strings.
        sqlExpression = sqlExpressionFactory.Coalesce(
            sqlExpression,
            sqlExpressionFactory.Constant(string.Empty, typeof(string)));

        // STRING_AGG returns null when there are no rows (or non-null values), but string.Join returns an empty string.
        return
            sqlExpressionFactory.Coalesce(
                SqlServerExpression.AggregateFunctionWithOrdering(
                    sqlExpressionFactory,
                    "STRING_AGG",
                    [
                        sqlExpression,
                        sqlExpressionFactory.ApplyTypeMapping(separator, sqlExpression.TypeMapping)
                    ],
                    source,
                    enumerableArgumentIndex: 0,
                    nullable: true,
                    argumentsPropagateNullability: Statics.FalseArrays[2],
                    typeof(string)),
                sqlExpressionFactory.Constant(string.Empty, typeof(string)),
                resultTypeMapping);
    }
}
