// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosVectorSearchTranslator(ISqlExpressionFactory sqlExpressionFactory, ITypeMappingSource typeMappingSource)
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
        if (method.DeclaringType != typeof(CosmosDbFunctionsExtensions)
            || method.Name != nameof(CosmosDbFunctionsExtensions.VectorDistance))
        {
            return null;
        }

        if (arguments is not [_, var vector1, var vector2, var useBruteForceExpression, var optionsExpression])
        {
            throw new UnreachableException();
        }

        if (useBruteForceExpression is not SqlConstantExpression { Value: var useBruteForceValue })
        {
            throw new InvalidOperationException(
                CoreStrings.ArgumentNotConstant("useBruteForce", nameof(CosmosDbFunctionsExtensions.VectorDistance)));
        }

        if (optionsExpression is not SqlConstantExpression { Value: var optionsValue })
        {
            throw new InvalidOperationException(
                CoreStrings.ArgumentNotConstant("options", nameof(CosmosDbFunctionsExtensions.VectorDistance)));
        }

        var options = (VectorDistanceOptions?)optionsValue;

        var vectorMapping = vector1.TypeMapping as CosmosVectorTypeMapping
            ?? vector2.TypeMapping as CosmosVectorTypeMapping
            ?? throw new InvalidOperationException(CosmosStrings.VectorSearchRequiresVector);

        var vectorType = vectorMapping.VectorType;

        List<Expression> newArguments =
        [
            sqlExpressionFactory.ApplyTypeMapping(vector1, vectorMapping), sqlExpressionFactory.ApplyTypeMapping(vector2, vectorMapping)
        ];

        if (useBruteForceValue is not null)
        {
            newArguments.Add(useBruteForceExpression);
        }

        if (options is not null)
        {
            // If the options are provided but not useBruteForce, we need to explicitly specify the default for the
            // latter (false)
            if (useBruteForceValue is null)
            {
                newArguments.Add(sqlExpressionFactory.ApplyDefaultTypeMapping(new SqlConstantExpression(false, typeMapping: null)));
            }

            var optionsBuilder = new StringBuilder("{ ");

            var requireComma = false;

            if (options.DistanceFunction is { } distanceFunction)
            {
                optionsBuilder
                    .Append("'distanceFunction': '")
                    .Append(distanceFunction.ToString().ToLower())
                    .Append('\'');

                vectorType = vectorType with { DistanceFunction = distanceFunction };

                requireComma = true;
            }

            if (options.DataType is not null)
            {
                if (requireComma)
                {
                    optionsBuilder.Append(", ");
                }

                optionsBuilder
                    .Append("'dataType': '")
                    .Append(options.DataType.ToLower())
                    .Append('\'');

                requireComma = true;
            }

            if (options.SearchListSizeMultiplier is not null)
            {
                if (requireComma)
                {
                    optionsBuilder.Append(", ");
                }

                optionsBuilder
                    .Append("'searchListSizeMultiplier': ")
                    .Append(options.SearchListSizeMultiplier.Value);

                requireComma = true;
            }

            if (options.QuantizedVectorListMultiplier is not null)
            {
                if (requireComma)
                {
                    optionsBuilder.Append(", ");
                }

                optionsBuilder
                    .Append("'quantizedVectorListMultiplier': ")
                    .Append(options.QuantizedVectorListMultiplier.Value);
            }

            optionsBuilder.Append(" }");

            var optionsString = optionsBuilder.ToString();

            newArguments.Add(new FragmentExpression(optionsString));
        }

        return sqlExpressionFactory.Function(
            "VectorDistance",
            newArguments,
            typeof(double),
            typeMappingSource.FindMapping(typeof(double))!);
    }
}
