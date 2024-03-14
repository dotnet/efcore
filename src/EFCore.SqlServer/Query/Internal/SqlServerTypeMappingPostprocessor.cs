// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerTypeMappingPostprocessor : RelationalTypeMappingPostprocessor
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerTypeMappingPostprocessor(
        QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
        RelationalQueryCompilationContext queryCompilationContext)
        : base(dependencies, relationalDependencies, queryCompilationContext)
        => _typeMappingSource = relationalDependencies.TypeMappingSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression expression)
        => expression switch
        {
            SqlServerOpenJsonExpression openJsonExpression
                when TryGetInferredTypeMapping(openJsonExpression.Alias, "value", out var typeMapping)
                => ApplyTypeMappingsOnOpenJsonExpression(openJsonExpression, new[] { typeMapping }),

            _ => base.VisitExtension(expression)
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SqlServerOpenJsonExpression ApplyTypeMappingsOnOpenJsonExpression(
        SqlServerOpenJsonExpression openJsonExpression,
        IReadOnlyList<RelationalTypeMapping> typeMappings)
    {
        Check.DebugAssert(typeMappings.Count == 1, "typeMappings.Count == 1");
        var elementTypeMapping = typeMappings[0];

        // Constant queryables are translated to VALUES, no need for JSON.
        // Column queryables have their type mapping from the model, so we don't ever need to apply an inferred mapping on them.
        if (openJsonExpression.JsonExpression is not SqlParameterExpression { TypeMapping: null } parameterExpression)
        {
            Check.DebugAssert(
                openJsonExpression.JsonExpression.TypeMapping is not null,
                "Non-parameter expression without a type mapping in ApplyTypeMappingsOnOpenJsonExpression");
            return openJsonExpression;
        }

        Check.DebugAssert(
            openJsonExpression.Path is null, "OpenJsonExpression path is non-null when applying an inferred type mapping");
        Check.DebugAssert(
            openJsonExpression.ColumnInfos is null, "OpenJsonExpression has no ColumnInfos when applying an inferred type mapping");

        // We need to apply the inferred type mapping in two places: the collection type mapping on the parameter expanded by OPENJSON,
        // and on the WITH clause determining the conversion out on the SQL Server side

        // First, find the collection type mapping and apply it to the parameter
        if (_typeMappingSource.FindMapping(parameterExpression.Type, Model, elementTypeMapping) is not SqlServerStringTypeMapping
                {
                    ElementTypeMapping: not null
                }
                parameterTypeMapping)
        {
            throw new UnreachableException("A SqlServerStringTypeMapping collection type mapping could not be found");
        }

        return openJsonExpression.Update(
            parameterExpression.ApplyTypeMapping(parameterTypeMapping),
            path: null,
            new[] { new SqlServerOpenJsonExpression.ColumnInfo("value", elementTypeMapping, []) });
    }
}
