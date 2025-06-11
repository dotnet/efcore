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
        if (method.DeclaringType != typeof(CosmosDbFunctionsExtensions))
        {
            return null;
        }

        return method.Name switch
        {
            nameof(CosmosDbFunctionsExtensions.FullTextContains)
                when arguments is [_, var property, var keyword] => sqlExpressionFactory.Function(
                    "FullTextContains",
                    [property, keyword],
                    typeof(bool),
                    typeMappingSource.FindMapping(typeof(bool))),

            nameof(CosmosDbFunctionsExtensions.FullTextScore)
                when arguments is [_, SqlExpression property, SqlConstantExpression { Type: var keywordClrType, Value: string[] values } keywords]
                    && keywordClrType == typeof(string[]) => sqlExpressionFactory.ScoringFunction(
                        "FullTextScore",
                        [property, .. values.Select(x => sqlExpressionFactory.Constant(x))],
                        typeof(double),
                        typeMappingSource.FindMapping(typeof(double))),

            nameof(CosmosDbFunctionsExtensions.FullTextScore)
                when arguments is [_, SqlExpression property, SqlParameterExpression { Type: var keywordClrType } keywords]
                    && keywordClrType == typeof(string[]) => sqlExpressionFactory.ScoringFunction(
                        "FullTextScore",
                        [property, keywords],
                        typeof(double),
                        typeMappingSource.FindMapping(typeof(double))),

            nameof(CosmosDbFunctionsExtensions.FullTextScore)
                when arguments is [_, SqlExpression property, ArrayConstantExpression keywords] => sqlExpressionFactory.ScoringFunction(
                    "FullTextScore",
                    [property, .. keywords.Items],
                    typeof(double),
                    typeMappingSource.FindMapping(typeof(double))),

            nameof(CosmosDbFunctionsExtensions.Rrf)
                when arguments is [_, ArrayConstantExpression functions] => sqlExpressionFactory.ScoringFunction(
                    "RRF",
                    functions.Items,
                    typeof(double),
                    typeMappingSource.FindMapping(typeof(double))),

            nameof(CosmosDbFunctionsExtensions.FullTextContainsAny) or nameof(CosmosDbFunctionsExtensions.FullTextContainsAll)
                when arguments is [_, SqlExpression property, SqlConstantExpression { Type: var keywordClrType, Value: string[] values } keywords]
                    && keywordClrType == typeof(string[]) => sqlExpressionFactory.Function(
                        method.Name == nameof(CosmosDbFunctionsExtensions.FullTextContainsAny) ? "FullTextContainsAny" : "FullTextContainsAll",
                        [property, .. values.Select(x => sqlExpressionFactory.Constant(x))],
                        typeof(bool),
                        typeMappingSource.FindMapping(typeof(bool))),

            nameof(CosmosDbFunctionsExtensions.FullTextContainsAny) or nameof(CosmosDbFunctionsExtensions.FullTextContainsAll)
                when arguments is [_, SqlExpression property, SqlParameterExpression { Type: var keywordClrType } keywords]
                    && keywordClrType == typeof(string[]) => sqlExpressionFactory.Function(
                        method.Name == nameof(CosmosDbFunctionsExtensions.FullTextContainsAny) ? "FullTextContainsAny" : "FullTextContainsAll",
                        [property, keywords],
                        typeof(bool),
                        typeMappingSource.FindMapping(typeof(bool))),

            nameof(CosmosDbFunctionsExtensions.FullTextContainsAny) or nameof(CosmosDbFunctionsExtensions.FullTextContainsAll)
                when arguments is [_, SqlExpression property, ArrayConstantExpression keywords] => sqlExpressionFactory.Function(
                    method.Name == nameof(CosmosDbFunctionsExtensions.FullTextContainsAny) ? "FullTextContainsAny" : "FullTextContainsAll",
                    [property, .. keywords.Items],
                    typeof(bool),
                    typeMappingSource.FindMapping(typeof(bool))),

            _ => null
        };
    }
}
