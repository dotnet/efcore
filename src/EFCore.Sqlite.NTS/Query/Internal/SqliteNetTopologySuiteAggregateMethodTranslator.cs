// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Union;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteNetTopologySuiteAggregateMethodTranslator : IAggregateMethodCallTranslator
{
    private static readonly MethodInfo GeometryCombineMethod
        = typeof(GeometryCombiner).GetRuntimeMethod(nameof(GeometryCombiner.Combine), [typeof(IEnumerable<Geometry>)])!;

    private static readonly MethodInfo ConvexHullMethod
        = typeof(ConvexHull).GetRuntimeMethod(nameof(ConvexHull.Create), [typeof(IEnumerable<Geometry>)])!;

    private static readonly MethodInfo UnionMethod
        = typeof(UnaryUnionOp).GetRuntimeMethod(nameof(UnaryUnionOp.Union), [typeof(IEnumerable<Geometry>)])!;

    private static readonly MethodInfo EnvelopeCombineMethod
        = typeof(EnvelopeCombiner).GetRuntimeMethod(nameof(EnvelopeCombiner.CombineAsGeometry), [typeof(IEnumerable<Geometry>)])!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteNetTopologySuiteAggregateMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
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
        if (source.Selector is not SqlExpression sqlExpression)
        {
            return null;
        }

        if (method == ConvexHullMethod)
        {
            CombineAggregateTerms();

            // Spatialite has no built-in aggregate convex hull, but we can simply apply Collect beforehand
            return _sqlExpressionFactory.Function(
                "ConvexHull",
                new[]
                {
                    _sqlExpressionFactory.Function(
                        "Collect",
                        new[] { sqlExpression },
                        nullable: true,
                        argumentsPropagateNullability: new[] { false },
                        typeof(Geometry))
                },
                nullable: true,
                argumentsPropagateNullability: new[] { true },
                typeof(Geometry));
        }

        var functionName = method == UnionMethod
            ? "GUnion"
            : method == GeometryCombineMethod
                ? "Collect"
                : method == EnvelopeCombineMethod
                    ? "Extent"
                    : null;

        if (functionName is null)
        {
            return null;
        }

        CombineAggregateTerms();

        return _sqlExpressionFactory.Function(
            functionName,
            new[] { sqlExpression },
            nullable: true,
            argumentsPropagateNullability: new[] { false },
            typeof(Geometry));

        void CombineAggregateTerms()
        {
            if (source.Predicate != null)
            {
                sqlExpression = _sqlExpressionFactory.Case(
                    new List<CaseWhenClause> { new(source.Predicate, sqlExpression) },
                    elseResult: null);
            }

            if (source.IsDistinct)
            {
                sqlExpression = new DistinctExpression(sqlExpression);
            }
        }
    }
}
