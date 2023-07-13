﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerQueryableMethodTranslatingExpressionVisitor : RelationalQueryableMethodTranslatingExpressionVisitor
{
    private readonly QueryCompilationContext _queryCompilationContext;
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly int _sqlServerCompatibilityLevel;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerQueryableMethodTranslatingExpressionVisitor(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
        RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
        QueryCompilationContext queryCompilationContext,
        ISqlServerSingletonOptions sqlServerSingletonOptions)
        : base(dependencies, relationalDependencies, queryCompilationContext)
    {
        _queryCompilationContext = queryCompilationContext;
        _typeMappingSource = relationalDependencies.TypeMappingSource;
        _sqlExpressionFactory = relationalDependencies.SqlExpressionFactory;

        _sqlServerCompatibilityLevel = sqlServerSingletonOptions.CompatibilityLevel;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlServerQueryableMethodTranslatingExpressionVisitor(
        SqlServerQueryableMethodTranslatingExpressionVisitor parentVisitor)
        : base(parentVisitor)
    {
        _queryCompilationContext = parentVisitor._queryCompilationContext;
        _typeMappingSource = parentVisitor._typeMappingSource;
        _sqlExpressionFactory = parentVisitor._sqlExpressionFactory;

        _sqlServerCompatibilityLevel = parentVisitor._sqlServerCompatibilityLevel;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
        => new SqlServerQueryableMethodTranslatingExpressionVisitor(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        if (extensionExpression is TemporalQueryRootExpression queryRootExpression)
        {
            var selectExpression = RelationalDependencies.SqlExpressionFactory.Select(queryRootExpression.EntityType);
            Func<TableExpression, TableExpressionBase> annotationApplyingFunc = queryRootExpression switch
            {
                TemporalAllQueryRootExpression => te => te
                    .AddAnnotation(SqlServerAnnotationNames.TemporalOperationType, TemporalOperationType.All),
                TemporalAsOfQueryRootExpression asOf => te => te
                    .AddAnnotation(SqlServerAnnotationNames.TemporalOperationType, TemporalOperationType.AsOf)
                    .AddAnnotation(SqlServerAnnotationNames.TemporalAsOfPointInTime, asOf.PointInTime),
                TemporalBetweenQueryRootExpression between => te => te
                    .AddAnnotation(SqlServerAnnotationNames.TemporalOperationType, TemporalOperationType.Between)
                    .AddAnnotation(SqlServerAnnotationNames.TemporalRangeOperationFrom, between.From)
                    .AddAnnotation(SqlServerAnnotationNames.TemporalRangeOperationTo, between.To),
                TemporalContainedInQueryRootExpression containedIn => te => te
                    .AddAnnotation(SqlServerAnnotationNames.TemporalOperationType, TemporalOperationType.ContainedIn)
                    .AddAnnotation(SqlServerAnnotationNames.TemporalRangeOperationFrom, containedIn.From)
                    .AddAnnotation(SqlServerAnnotationNames.TemporalRangeOperationTo, containedIn.To),
                TemporalFromToQueryRootExpression fromTo => te => te
                    .AddAnnotation(SqlServerAnnotationNames.TemporalOperationType, TemporalOperationType.FromTo)
                    .AddAnnotation(SqlServerAnnotationNames.TemporalRangeOperationFrom, fromTo.From)
                    .AddAnnotation(SqlServerAnnotationNames.TemporalRangeOperationTo, fromTo.To),
                _ => throw new InvalidOperationException(queryRootExpression.Print()),
            };

            selectExpression = (SelectExpression)new TemporalAnnotationApplyingExpressionVisitor(annotationApplyingFunc)
                .Visit(selectExpression);

            return new ShapedQueryExpression(
                selectExpression,
                new RelationalEntityShaperExpression(
                    queryRootExpression.EntityType,
                    new ProjectionBindingExpression(
                        selectExpression,
                        new ProjectionMember(),
                        typeof(ValueBuffer)),
                    false));
        }

        return base.VisitExtension(extensionExpression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateCollection(
        SqlExpression sqlExpression,
        RelationalTypeMapping? elementTypeMapping,
        string tableAlias)
    {
        if (_sqlServerCompatibilityLevel < 130)
        {
            AddTranslationErrorDetails(SqlServerStrings.CompatibilityLevelTooLowForScalarCollections(_sqlServerCompatibilityLevel));

            return null;
        }

        // Generate the OPENJSON function expression, and wrap it in a SelectExpression.

        // Note that where the elementTypeMapping is known (i.e. collection columns), we immediately generate OPENJSON with a WITH clause
        // (i.e. with a columnInfo), which determines the type conversion to apply to the JSON elements coming out.
        // For parameter collections, the element type mapping will only be inferred and applied later (see
        // SqlServerInferredTypeMappingApplier below), at which point the we'll apply it to add the WITH clause.
        var openJsonExpression = elementTypeMapping is null
            ? new SqlServerOpenJsonExpression(tableAlias, sqlExpression)
            : new SqlServerOpenJsonExpression(
                tableAlias, sqlExpression, columnInfos: new[]
                {
                    new SqlServerOpenJsonExpression.ColumnInfo { Name = "value", StoreType = elementTypeMapping.StoreType, Path = "$" }
                });

        // TODO: This is a temporary CLR type-based check; when we have proper metadata to determine if the element is nullable, use it here
        var elementClrType = sqlExpression.Type.GetSequenceType();
        var isColumnNullable = elementClrType.IsNullableType();

        var selectExpression = new SelectExpression(
            openJsonExpression, columnName: "value", columnType: elementClrType, columnTypeMapping: elementTypeMapping, isColumnNullable);

        // OPENJSON doesn't guarantee the ordering of the elements coming out; when using OPENJSON without WITH, a [key] column is returned
        // with the JSON array's ordering, which we can ORDER BY; this option doesn't exist with OPENJSON with WITH, unfortunately.
        // However, OPENJSON with WITH has better performance, and also applies JSON-specific conversions we cannot be done otherwise
        // (e.g. OPENJSON with WITH does base64 decoding for VARBINARY).
        // Here we generate OPENJSON with WITH, but also add an ordering by [key] - this is a temporary invalid representation.
        // In SqlServerQueryTranslationPostprocessor, we'll post-process the expression; if the ORDER BY was stripped (e.g. because of
        // IN, EXISTS or a set operation), we'll just leave the OPENJSON with WITH. If not, we'll convert the OPENJSON with WITH to an
        // OPENJSON without WITH.
        // Note that the OPENJSON 'key' column is an nvarchar - we convert it to an int before sorting.
        selectExpression.AppendOrdering(
            new OrderingExpression(
                _sqlExpressionFactory.Convert(
                    selectExpression.CreateColumnExpression(
                        openJsonExpression,
                        "key",
                        typeof(string),
                        typeMapping: _typeMappingSource.FindMapping("nvarchar(4000)"),
                        columnNullable: false),
                    typeof(int),
                    _typeMappingSource.FindMapping(typeof(int))),
                ascending: true));

        var shaperExpression = new ProjectionBindingExpression(selectExpression, new ProjectionMember(), elementClrType);

        return new ShapedQueryExpression(selectExpression, shaperExpression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override ShapedQueryExpression? TranslateElementAtOrDefault(
        ShapedQueryExpression source,
        Expression index,
        bool returnDefault)
    {
        // TODO: Make sure we want to actually transform to JSON_VALUE, #30981
        if (!returnDefault
            && source.QueryExpression is SelectExpression
            {
                Tables: [SqlServerOpenJsonExpression { Arguments: [var jsonArrayColumn] } openJsonExpression],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null,
                // We can only apply the indexing if the JSON array is ordered by its natural ordered, i.e. by the "key" column that
                // we created in TranslateCollection. For example, if another ordering has been applied (e.g. by the JSON elements
                // themselves), we can no longer simply index into the original array.
                Orderings:
                [
                    {
                        Expression: SqlUnaryExpression
                        {
                            OperatorType: ExpressionType.Convert,
                            Operand: ColumnExpression { Name: "key", Table: var orderingTable }
                        }
                    }
                ]
            } selectExpression
            && TranslateExpression(index) is { } translatedIndex
            && orderingTable == openJsonExpression)
        {
            // Index on JSON array

            // Extract the column projected out of the source, and simplify the subquery to a simple JsonScalarExpression
            var shaperExpression = source.ShaperExpression;
            if (shaperExpression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression
                && unaryExpression.Operand.Type.IsNullableType()
                && unaryExpression.Operand.Type.UnwrapNullableType() == unaryExpression.Type)
            {
                shaperExpression = unaryExpression.Operand;
            }

            if (shaperExpression is ProjectionBindingExpression projectionBindingExpression
                && selectExpression.GetProjection(projectionBindingExpression) is SqlExpression projection)
            {
                // OPENJSON's value column is an nvarchar(max); if this is a collection column whose type mapping is know, the projection
                // contains a CAST node which we unwrap
                var projectionColumn = projection switch
                {
                    ColumnExpression c => c,
                    SqlUnaryExpression { OperatorType: ExpressionType.Convert, Operand: ColumnExpression c } => c,
                    _ => null
                };

                if (projectionColumn is not null)
                {
                    // If the inner expression happens to itself be a JsonScalarExpression, simply append the two paths to avoid creating
                    // JSON_VALUE within JSON_VALUE.
                    var (json, path) = jsonArrayColumn is JsonScalarExpression innerJsonScalarExpression
                        ? (innerJsonScalarExpression.Json, innerJsonScalarExpression.Path.Append(new(translatedIndex)).ToArray())
                        : (jsonArrayColumn, new PathSegment[] { new(translatedIndex) });

                    var translation = new JsonScalarExpression(
                        json,
                        path,
                        projection.Type,
                        projection.TypeMapping,
                        projectionColumn.IsNullable);

                    return source.UpdateQueryExpression(_sqlExpressionFactory.Select(translation));
                }
            }
        }

        return base.TranslateElementAtOrDefault(source, index, returnDefault);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool IsValidSelectExpressionForExecuteDelete(
        SelectExpression selectExpression,
        EntityShaperExpression entityShaperExpression,
        [NotNullWhen(true)] out TableExpression? tableExpression)
    {
        if (selectExpression.Offset == null
            && (!selectExpression.IsDistinct || entityShaperExpression.EntityType.FindPrimaryKey() != null)
            && selectExpression.GroupBy.Count == 0
            && selectExpression.Having == null
            && selectExpression.Orderings.Count == 0)
        {
            TableExpressionBase table;
            if (selectExpression.Tables.Count == 1)
            {
                table = selectExpression.Tables[0];
            }
            else
            {
                var projectionBindingExpression = (ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression;
                var entityProjectionExpression = (EntityProjectionExpression)selectExpression.GetProjection(projectionBindingExpression);
                var column = entityProjectionExpression.BindProperty(entityShaperExpression.EntityType.GetProperties().First());
                table = column.Table;
                if (table is JoinExpressionBase joinExpressionBase)
                {
                    table = joinExpressionBase.Table;
                }
            }

            if (table is TableExpression te)
            {
                tableExpression = te;
                return true;
            }
        }

        tableExpression = null;
        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool IsValidSelectExpressionForExecuteUpdate(
        SelectExpression selectExpression,
        EntityShaperExpression entityShaperExpression,
        [NotNullWhen(true)] out TableExpression? tableExpression)
    {
        if (selectExpression.Offset == null
            // If entity type has primary key then Distinct is no-op
            && (!selectExpression.IsDistinct || entityShaperExpression.EntityType.FindPrimaryKey() != null)
            && selectExpression.GroupBy.Count == 0
            && selectExpression.Having == null
            && selectExpression.Orderings.Count == 0)
        {
            TableExpressionBase table;
            if (selectExpression.Tables.Count == 1)
            {
                table = selectExpression.Tables[0];
            }
            else
            {
                var projectionBindingExpression = (ProjectionBindingExpression)entityShaperExpression.ValueBufferExpression;
                var entityProjectionExpression = (EntityProjectionExpression)selectExpression.GetProjection(projectionBindingExpression);
                var column = entityProjectionExpression.BindProperty(entityShaperExpression.EntityType.GetProperties().First());
                table = column.Table;
                if (table is JoinExpressionBase joinExpressionBase)
                {
                    table = joinExpressionBase.Table;
                }
            }

            if (table is TableExpression te)
            {
                tableExpression = te;
                return true;
            }
        }

        tableExpression = null;
        return false;
    }

    private sealed class TemporalAnnotationApplyingExpressionVisitor : ExpressionVisitor
    {
        private readonly Func<TableExpression, TableExpressionBase> _annotationApplyingFunc;

        public TemporalAnnotationApplyingExpressionVisitor(Func<TableExpression, TableExpressionBase> annotationApplyingFunc)
        {
            _annotationApplyingFunc = annotationApplyingFunc;
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
            => expression is TableExpression tableExpression
                ? _annotationApplyingFunc(tableExpression)
                : base.Visit(expression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression ApplyInferredTypeMappings(
        Expression expression,
        IReadOnlyDictionary<(TableExpressionBase, string), RelationalTypeMapping?> inferredTypeMappings)
        => new SqlServerInferredTypeMappingApplier(_typeMappingSource, _sqlExpressionFactory, inferredTypeMappings).Visit(expression);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected class SqlServerInferredTypeMappingApplier : RelationalInferredTypeMappingApplier
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerInferredTypeMappingApplier(
            IRelationalTypeMappingSource typeMappingSource,
            ISqlExpressionFactory sqlExpressionFactory,
            IReadOnlyDictionary<(TableExpressionBase, string), RelationalTypeMapping?> inferredTypeMappings)
            : base(sqlExpressionFactory, inferredTypeMappings)
            => _typeMappingSource = typeMappingSource;

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
                    when TryGetInferredTypeMapping(openJsonExpression, "value", out var typeMapping)
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

            Check.DebugAssert(openJsonExpression.Path is null, "openJsonExpression.Path is null");
            Check.DebugAssert(openJsonExpression.ColumnInfos is null, "Invalid SqlServerOpenJsonExpression");

            // We need to apply the inferred type mapping in two places: the collection type mapping on the parameter expanded by OPENJSON,
            // and on the WITH clause determining the conversion out on the SQL Server side

            // First, find the collection type mapping and apply it to the parameter

            // TODO: We shouldn't need to manually construct the JSON string type mapping this way; we need to be able to provide the
            // TODO: element's store type mapping as input to _typeMappingSource.FindMapping.
            // TODO: When this is done, revert converter equality check in QuerySqlGenerator.VisitSqlParameter back to reference equality,
            // since we'll always have the same instance of the type mapping returned from the type mapping source. Also remove
            // CollectionToJsonStringConverter.Equals etc.
            // TODO: Note: NpgsqlTypeMappingSource exposes FindContainerMapping() for this purpose.
            // #30730
            if (_typeMappingSource.FindMapping(typeof(string)) is not SqlServerStringTypeMapping parameterTypeMapping)
            {
                throw new InvalidOperationException("Type mapping for 'string' could not be found or was not a SqlServerStringTypeMapping");
            }

            parameterTypeMapping = (SqlServerStringTypeMapping)parameterTypeMapping
                .Clone(new CollectionToJsonStringConverter(parameterExpression.Type, elementTypeMapping));

            parameterTypeMapping = (SqlServerStringTypeMapping)parameterTypeMapping.CloneWithElementTypeMapping(elementTypeMapping);

            return openJsonExpression.Update(
                parameterExpression.ApplyTypeMapping(parameterTypeMapping),
                path: null,
                new[] { new SqlServerOpenJsonExpression.ColumnInfo("value", elementTypeMapping.StoreType, "$") });
        }
    }
}
