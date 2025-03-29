// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Extensions;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosFullTextSearchTranslator(ISqlExpressionFactory sqlExpressionFactory, ITypeMappingSource typeMappingSource)
    : IMethodCallTranslator
{
    private static readonly MethodInfo _fullTextContainsMethod
        = typeof(CosmosDbFunctionsExtensions).GetRuntimeMethod(nameof(CosmosDbFunctionsExtensions.FullTextContains), [typeof(DbFunctions), typeof(string), typeof(string)])!;

    private static readonly MethodInfo _fullTextContainsAllMethod
        = typeof(CosmosDbFunctionsExtensions).GetRuntimeMethod(nameof(CosmosDbFunctionsExtensions.FullTextContainsAll), [typeof(DbFunctions), typeof(string), typeof(string[])])!;

    private static readonly MethodInfo _fullTextContainsAnyMethod
        = typeof(CosmosDbFunctionsExtensions).GetRuntimeMethod(nameof(CosmosDbFunctionsExtensions.FullTextContainsAny), [typeof(DbFunctions), typeof(string), typeof(string[])])!;

    private static readonly MethodInfo _fullTextScoreMethod
        = typeof(CosmosDbFunctionsExtensions).GetRuntimeMethod(nameof(CosmosDbFunctionsExtensions.FullTextScore), [typeof(DbFunctions), typeof(string), typeof(string[])])!;

    private static readonly MethodInfo _rrfMethod
        = typeof(CosmosDbFunctionsExtensions).GetRuntimeMethod(nameof(CosmosDbFunctionsExtensions.Rrf), [typeof(DbFunctions), typeof(double[])])!;

    private static readonly Dictionary<MethodInfo, string> SupportedMethodsMap = new()
    {
        { _fullTextContainsMethod, nameof(CosmosDbFunctionsExtensions.FullTextContains) },
        { _fullTextContainsAllMethod, nameof(CosmosDbFunctionsExtensions.FullTextContainsAll) },
        { _fullTextContainsAnyMethod, nameof(CosmosDbFunctionsExtensions.FullTextContainsAny) },
        { _fullTextScoreMethod, nameof(CosmosDbFunctionsExtensions.FullTextScore) },
        { _rrfMethod, nameof(CosmosDbFunctionsExtensions.Rrf).ToUpper() },
    };

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
        if (!SupportedMethodsMap.ContainsKey(method))
        {
            return null;
        }

        var typeMapping = arguments[1].TypeMapping;
        if (method == _fullTextContainsMethod)
        {
            return sqlExpressionFactory.Function(
                SupportedMethodsMap[method],
                [
                    arguments[1],
                    sqlExpressionFactory.ApplyTypeMapping(arguments[2], typeMapping)
                ],
                typeof(bool),
                typeMappingSource.FindMapping(typeof(bool)));
        }

        if (method == _fullTextScoreMethod)
        {
            return new SqlFunctionExpression(
                SupportedMethodsMap[method],
                isScoringFunction: true,
                [
                    arguments[1] is SqlExpression sqlArgument1
                        ? sqlExpressionFactory.ApplyDefaultTypeMapping(sqlArgument1)
                        : arguments[1],
                    arguments[2] is SqlExpression sqlArgument2
                        ? sqlExpressionFactory.ApplyDefaultTypeMapping(sqlArgument2)
                        : arguments[2],
                ],
                typeof(double),
                typeMappingSource.FindMapping(typeof(double)));
        }

        if (method == _rrfMethod)
        {
            var functionAguments = new List<SqlExpression>();

            var arrayArgument = (ArrayConstantExpression)arguments[1];
            foreach (var item in arrayArgument.Items)
            {
                functionAguments.Add(item is SqlExpression sqlArgument
                    ? sqlExpressionFactory.ApplyDefaultTypeMapping(sqlArgument)
                    : item);
            }

            return new SqlFunctionExpression(
                SupportedMethodsMap[method],
                isScoringFunction: true,
                functionAguments,
                typeof(double),
                typeMappingSource.FindMapping(typeof(double)));
        }

        if (method == _fullTextContainsAnyMethod || method == _fullTextContainsAllMethod)
        {
            var resultAguments = new List<SqlExpression>
            {
                arguments[1]
            };

            var paramsArgument = (ArrayConstantExpression)arguments[2];
            foreach (var item in paramsArgument.Items)
            {
                resultAguments.Add(item);
            }

            return sqlExpressionFactory.Function(
                SupportedMethodsMap[method],
                resultAguments,
                typeof(bool),
                typeMappingSource.FindMapping(typeof(bool)));
        }

        throw new UnreachableException($"No translation for supported method: '{method.Name}'");
    }
}
