// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerSqlNullabilityProcessor : SqlNullabilityProcessor
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public const string OpenJsonParameterTableName = "__openjson";

    private readonly ISqlServerSingletonOptions _sqlServerSingletonOptions;

    private int _openJsonAliasCounter;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerSqlNullabilityProcessor(
        RelationalParameterBasedSqlProcessorDependencies dependencies,
        RelationalParameterBasedSqlProcessorParameters parameters,
        ISqlServerSingletonOptions sqlServerSingletonOptions)
        : base(dependencies, parameters)
    {
        _sqlServerSingletonOptions = sqlServerSingletonOptions;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression Process(Expression queryExpression, CacheSafeParameterFacade parametersFacade)
    {
        var result = base.Process(queryExpression, parametersFacade);
        _openJsonAliasCounter = 0;
        return result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override SqlExpression VisitCustomSqlExpression(
        SqlExpression sqlExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
        => sqlExpression switch
        {
            SqlServerAggregateFunctionExpression aggregateFunctionExpression
                => VisitSqlServerAggregateFunction(aggregateFunctionExpression, allowOptimizedExpansion, out nullable),

            _ => base.VisitCustomSqlExpression(sqlExpression, allowOptimizedExpansion, out nullable)
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SqlExpression VisitSqlServerAggregateFunction(
        SqlServerAggregateFunctionExpression aggregateFunctionExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
    {
        nullable = aggregateFunctionExpression.IsNullable;

        SqlExpression[]? arguments = null;
        for (var i = 0; i < aggregateFunctionExpression.Arguments.Count; i++)
        {
            var visitedArgument = Visit(aggregateFunctionExpression.Arguments[i], out _);
            if (visitedArgument != aggregateFunctionExpression.Arguments[i] && arguments is null)
            {
                arguments = new SqlExpression[aggregateFunctionExpression.Arguments.Count];

                for (var j = 0; j < i; j++)
                {
                    arguments[j] = aggregateFunctionExpression.Arguments[j];
                }
            }

            if (arguments is not null)
            {
                arguments[i] = visitedArgument;
            }
        }

        OrderingExpression[]? orderings = null;
        for (var i = 0; i < aggregateFunctionExpression.Orderings.Count; i++)
        {
            var ordering = aggregateFunctionExpression.Orderings[i];
            var visitedOrdering = ordering.Update(Visit(ordering.Expression, out _));
            if (visitedOrdering != aggregateFunctionExpression.Orderings[i] && orderings is null)
            {
                orderings = new OrderingExpression[aggregateFunctionExpression.Orderings.Count];

                for (var j = 0; j < i; j++)
                {
                    orderings[j] = aggregateFunctionExpression.Orderings[j];
                }
            }

            if (orderings is not null)
            {
                orderings[i] = visitedOrdering;
            }
        }

        return arguments is not null || orderings is not null
            ? aggregateFunctionExpression.Update(
                arguments ?? aggregateFunctionExpression.Arguments,
                orderings ?? aggregateFunctionExpression.Orderings)
            : aggregateFunctionExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool PreferExistsToInWithCoalesce
        => true;

#pragma warning disable EF1001
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool IsCollectionTable(TableExpressionBase table, [NotNullWhen(true)] out Expression? collection)
    {
        if (table is SqlServerOpenJsonExpression { Arguments: [var argument] })
        {
            collection = argument;
            return true;
        }

        return base.IsCollectionTable(table, out collection);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override TableExpressionBase UpdateParameterCollection(
        TableExpressionBase table,
        SqlParameterExpression newCollectionParameter)
        => table is SqlServerOpenJsonExpression { Arguments: [SqlParameterExpression] } openJsonExpression
            ? openJsonExpression.Update(newCollectionParameter, path: null)
            : base.UpdateParameterCollection(table, newCollectionParameter);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression node)
    {
        switch (node)
        {
            case ValuesExpression { ValuesParameter: SqlParameterExpression valuesParameter } valuesExpression
                when ParameterizedCollectionMode is ParameterizedCollectionMode.MultipleParameters:
            {
                Check.DebugAssert(valuesParameter.TypeMapping is not null);
                Check.DebugAssert(valuesParameter.TypeMapping.ElementTypeMapping is not null);
                var elementTypeMapping = (RelationalTypeMapping)valuesParameter.TypeMapping.ElementTypeMapping;

                if (TryHandleOverLimitParameters(
                    valuesParameter,
                    elementTypeMapping,
                    valuesExpression,
                    out var openJson,
                    out var constants,
                    out _))
                {
                    switch (openJson, constants)
                    {
                        case (not null, null):
                            return openJson;

                        case (null, not null):
                            Check.DebugAssert(constants.All(x => x is RowValueExpression));
                            return valuesExpression.Update([.. constants.Cast<RowValueExpression>()]);

                        default:
                            throw new UnreachableException();
                    }
                }
                return base.VisitExtension(node);
            }

            default:
                return base.VisitExtension(node);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override SqlExpression VisitIn(InExpression inExpression, bool allowOptimizedExpansion, out bool nullable)
    {
        switch (inExpression.ValuesParameter)
        {
            case SqlParameterExpression valuesParameter
                when ParameterizedCollectionMode is ParameterizedCollectionMode.MultipleParameters:
            {
                Check.DebugAssert(valuesParameter.TypeMapping is not null);
                Check.DebugAssert(valuesParameter.TypeMapping.ElementTypeMapping is not null);
                var elementTypeMapping = (RelationalTypeMapping)valuesParameter.TypeMapping.ElementTypeMapping;

                if (TryHandleOverLimitParameters(
                    valuesParameter,
                    elementTypeMapping,
                    valuesExpression: null,
                    out var openJson,
                    out var constants,
                    out var containsNulls))
                {
                    inExpression = (openJson, constants) switch
                    {
                        (not null, null)
                            => inExpression.Update(
                                inExpression.Item,
                                SelectExpression.CreateImmutable(
                                    null!,
                                    [openJson],
                                    [
                                        new ProjectionExpression(
                                            new ColumnExpression(
                                                "value",
                                                openJson.Alias,
                                                valuesParameter.Type.GetSequenceType(),
                                                elementTypeMapping,
                                                containsNulls!.Value),
                                            "value")
                                    ],
                                    null!)),

                        (null, not null) => inExpression.Update(inExpression.Item, constants),

                        _ => throw new UnreachableException(),
                    };
                }
                return base.VisitIn(inExpression, allowOptimizedExpansion, out nullable);
            }

            default:
                return base.VisitIn(inExpression, allowOptimizedExpansion, out nullable);
        }
    }

    private bool TryHandleOverLimitParameters(
        SqlParameterExpression valuesParameter,
        RelationalTypeMapping typeMapping,
        ValuesExpression? valuesExpression,
        out SqlServerOpenJsonExpression? openJsonResult,
        out List<SqlExpression>? constantsResult,
        out bool? containsNulls)
    {
        var parameters = ParametersFacade.GetParametersAndDisableSqlCaching();
        var values = ((IEnumerable?)parameters[valuesParameter.Name])?.Cast<object>().ToList() ?? [];

        // SQL Server has limit on number of parameters in a query.
        // If we're over that limit, we switch to using single parameter
        // and processing it through JSON functions.
        if (values.Count > 2098)
        {
            if (_sqlServerSingletonOptions.SupportsJsonFunctions)
            {
                var openJsonExpression = new SqlServerOpenJsonExpression(
                    valuesExpression?.Alias ?? $"{OpenJsonParameterTableName}{_openJsonAliasCounter++}",
                    valuesParameter,
                    columnInfos:
                    [
                        new SqlServerOpenJsonExpression.ColumnInfo
                        {
                            Name = "value",
                            TypeMapping = typeMapping,
                            Path = [],
                        }
                    ]);
                var jsonPostprocessor = new SqlServerJsonPostprocessor(
                    Dependencies.TypeMappingSource,
                    Dependencies.SqlExpressionFactory,
                    sqlAliasManager: null);
                openJsonResult = (SqlServerOpenJsonExpression)jsonPostprocessor.Process(openJsonExpression);
                constantsResult = default;
                containsNulls = values.Any(static x => x is null);
                return true;
            }
            else
            {
                var intTypeMapping = (IntTypeMapping)Dependencies.TypeMappingSource.FindMapping(typeof(int))!;
                var counter = 1;

                constantsResult = new List<SqlExpression>();
                foreach (var value in values)
                {
                    constantsResult.Add(
                        valuesExpression is not null
                            ? new RowValueExpression(
                                ProcessValuesOrderingColumn(
                                    valuesExpression,
                                    [Dependencies.SqlExpressionFactory.Constant(value, value?.GetType() ?? typeof(object), sensitive: true, typeMapping)],
                                    intTypeMapping,
                                    ref counter))
                            : Dependencies.SqlExpressionFactory.Constant(value, value?.GetType() ?? typeof(object), sensitive: true, typeMapping));
                }

                openJsonResult = default;
                containsNulls = default;
                return true;
            }
        }
        openJsonResult = default;
        constantsResult = default;
        containsNulls = default;
        return false;
    }
#pragma warning restore EF1001
}
