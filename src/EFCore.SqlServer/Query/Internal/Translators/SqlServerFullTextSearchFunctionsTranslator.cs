// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerFullTextSearchFunctionsTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
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
        if (method.DeclaringType != typeof(SqlServerDbFunctionsExtensions))
        {
            return null;
        }

        // Note: the table-valued FREETEXTTABLE and CONTAINSTABLE functions are handled in SqlServerQueryableMethodTranslatingExpressionVisitor
        var functionName = method.Name switch
        {
            nameof(SqlServerDbFunctionsExtensions.FreeText) => "FREETEXT",
            nameof(SqlServerDbFunctionsExtensions.Contains) => "CONTAINS",
            _ => null
        };

        if (functionName is null)
        {
            return null;
        }

        var propertyReference = arguments[1];
        if (propertyReference is not ColumnExpression)
        {
            throw new InvalidOperationException(SqlServerStrings.InvalidColumnNameForFreeText);
        }

        var freeText = sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]);

        var functionArguments = new List<SqlExpression> { propertyReference, freeText };

        if (arguments.Count == 4)
        {
            functionArguments.Add(
                sqlExpressionFactory.Fragment($"LANGUAGE {((SqlConstantExpression)arguments[3]).Value}"));
        }

        return sqlExpressionFactory.Function(
            functionName,
            functionArguments,
            nullable: true,
            // TODO: don't propagate for now
            argumentsPropagateNullability: functionArguments.Select(_ => false).ToList(),
            typeof(bool));
    }
}
