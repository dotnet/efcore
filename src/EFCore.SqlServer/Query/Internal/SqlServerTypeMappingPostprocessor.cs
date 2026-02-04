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
    private readonly IModel _model;
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
    {
        _model = queryCompilationContext.Model;
        _typeMappingSource = relationalDependencies.TypeMappingSource;
    }

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
                => ApplyTypeMappingsOnOpenJsonExpression(openJsonExpression),

            _ => base.VisitExtension(expression)
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual SqlServerOpenJsonExpression ApplyTypeMappingsOnOpenJsonExpression(SqlServerOpenJsonExpression openJsonExpression)
    {
        // Constant queryables are translated to VALUES, no need for JSON.
        // Column queryables have their type mapping from the model, so we don't ever need to apply an inferred mapping on them.
        if (openJsonExpression.Json is not SqlParameterExpression { TypeMapping: null } parameterExpression)
        {
            Check.DebugAssert(
                openJsonExpression.Json.TypeMapping is not null,
                "Non-parameter expression without a type mapping in ApplyTypeMappingsOnOpenJsonExpression");
            return openJsonExpression;
        }

        Check.DebugAssert(
            openJsonExpression.Path is null, "OpenJsonExpression path is non-null when applying an inferred type mapping");
        Check.DebugAssert(
            openJsonExpression.ColumnInfos is null, "OpenJsonExpression has no ColumnInfos when applying an inferred type mapping");

        // In the usual case, some operation performed against the elements of the collection (e.g. comparison to a column) provides us with
        // an element type mapping; infer the collection's type mapping from that.
        // NOTE: This assumes that the OPENJSON always returns only a single column, which is currently true but won't always be.
        RelationalTypeMapping? parameterTypeMapping;

        if (TryGetInferredTypeMapping(openJsonExpression.Alias, "value", out var elementTypeMapping))
        {
            // We need to apply the inferred type mapping in two places: the collection type mapping on the parameter expanded by OPENJSON,
            // and on the WITH clause determining the conversion out on the SQL Server side

            // First, find the collection type mapping and apply it to the parameter
            parameterTypeMapping = _typeMappingSource.FindMapping(parameterExpression.Type, _model, elementTypeMapping);
        }
        else
        {
            // We have no inferred type mapping for the element type. This means that there was nothing in the query done
            // against the elements of the collection (e.g. comparison to a column), which tells us what type mapping it is.
            // In normal circumstances, such an expression would get client-evaluated in the funceltizer (no reference to a
            // column/database-side object), but with compiled queries the collection parameter gets preserved as-is.
            // The only thing we can do is apply the default type mapping.
            parameterTypeMapping = _typeMappingSource.FindMapping(parameterExpression.Type, QueryCompilationContext.Model);

            if (parameterTypeMapping is not { ElementTypeMapping: RelationalTypeMapping e })
            {
                throw new UnreachableException("Default type mapping has no element type mapping");
            }

            elementTypeMapping = e;
        }

        if (parameterTypeMapping is not SqlServerStringTypeMapping { ElementTypeMapping: not null })
        {
            throw new UnreachableException("A SqlServerStringTypeMapping collection type mapping could not be found");
        }

        return openJsonExpression.Update(
            parameterExpression.ApplyTypeMapping(parameterTypeMapping),
            path: null,
            [new SqlServerOpenJsonExpression.ColumnInfo("value", elementTypeMapping, [])]);
    }
}
