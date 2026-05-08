// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerVectorTranslator(
    ISqlExpressionFactory sqlExpressionFactory,
    IRelationalTypeMappingSource typeMappingSource)
    : IMethodCallTranslator, IMemberTranslator
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
        if (method.DeclaringType == typeof(SqlServerDbFunctionsExtensions))
        {
            switch (method.Name)
            {
                case nameof(SqlServerDbFunctionsExtensions.VectorDistance)
                    when arguments is [_, var distanceMetric, var vector1, var vector2]:
                {
                    var vectorTypeMapping = vector1.TypeMapping
                        ?? vector2.TypeMapping
                        ?? throw new InvalidOperationException(
                            "One of the arguments to EF.Functions.VectorDistance must be a vector column.");

                    return sqlExpressionFactory.Function(
                        "VECTOR_DISTANCE",
                        [
                            sqlExpressionFactory.ApplyTypeMapping(distanceMetric, typeMappingSource.FindMapping("varchar(max)")),
                            sqlExpressionFactory.ApplyTypeMapping(vector1, vectorTypeMapping),
                            sqlExpressionFactory.ApplyTypeMapping(vector2, vectorTypeMapping)
                        ],
                        nullable: true,
                        argumentsPropagateNullability: [true, true, true],
                        typeof(double),
                        typeMappingSource.FindMapping(typeof(double)));
                }
            }
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (member.DeclaringType == typeof(SqlVector<float>))
        {
            switch (member.Name)
            {
                case nameof(SqlVector<>.Length) when instance is not null:
                {
                    return sqlExpressionFactory.Function(
                        "VECTORPROPERTY",
                        [
                            instance,
                            sqlExpressionFactory.Constant("Dimensions", typeMappingSource.FindMapping("varchar(max)"))
                        ],
                        nullable: true,
                        argumentsPropagateNullability: [true, true],
                        typeof(int),
                        typeMappingSource.FindMapping(typeof(int)));
                }
            }
        }

        return null;
    }
}
