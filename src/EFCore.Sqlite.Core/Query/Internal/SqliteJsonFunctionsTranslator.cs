// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteJsonFunctionsTranslator : IMethodCallTranslator
{ 
    private static readonly Dictionary<MethodInfo, string> _methodInfoJsonFunctions
        = new()
        {
            {
                typeof(SqliteDbFunctionsExtensions).GetRuntimeMethod(nameof(SqliteDbFunctionsExtensions.JsonExtract), new[] { typeof(DbFunctions), typeof(object), typeof(string[]) })!,
                "JsonExtract_Paths"
            },
            {
                typeof(SqliteDbFunctionsExtensions).GetRuntimeMethod(nameof(SqliteDbFunctionsExtensions.JsonExtract), new[] { typeof(DbFunctions), typeof(object), typeof(string) })!,
                "JsonExtract_Path"
            },
        };

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteJsonFunctionsTranslator(
        ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if(_methodInfoJsonFunctions.TryGetValue(method,out _))
        {
            var expression = arguments[1];
            var pathArg = arguments[2];
            var functionArguments = new List<SqlExpression> { expression };
            var pathValue = ((SqlConstantExpression)pathArg)?.Value;

            //If return type is of string it means JSON_EXTRACT will return text which is a well-formed JSON array holding the various values.
            if (method.ReturnParameter?.ParameterType == typeof(string))
            {
                if (pathValue?.GetType() == typeof(string[]))
                {
                    var constantValues = pathValue is not null ? (string[])pathValue : Array.Empty<string>();

                    foreach (var path in constantValues)
                    {
                        functionArguments.Add(_sqlExpressionFactory.Constant(path));
                    }
                }
            }

            //If return type is of object it means JSON_EXTRACT will return either null, INTEGER or REAL for a JSON numeric value, an INTEGER zero for a JSON false value, an INTEGER one for a JSON true value.
            //the dequoted text for a JSON string value, and a text representation for JSON object and array values.
            if (method.ReturnParameter?.ParameterType == typeof(object))
                functionArguments.Add(_sqlExpressionFactory.Constant(pathValue));

            return _sqlExpressionFactory.Function(
                "JSON_EXTRACT",
                functionArguments,
                nullable: true,
                argumentsPropagateNullability: functionArguments.Select(_ => true).ToList(),
                typeof(string));
        }

        return null;
    }
}
