// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerQueryableMethodTranslatingExpressionVisitor(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
        RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, relationalDependencies, queryCompilationContext)
    {
        _queryCompilationContext = queryCompilationContext;
        _typeMappingSource = relationalDependencies.TypeMappingSource;
        _sqlExpressionFactory = relationalDependencies.SqlExpressionFactory;
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
    protected override ShapedQueryExpression TranslateCollection(
        SqlExpression sqlExpression,
        RelationalTypeMapping? elementTypeMapping,
        string tableAlias)
    {
        var elementClrType = sqlExpression.Type.GetSequenceType();

        // Generate the OpenJson function expression, and wrap it in a SelectExpression.
        // Note that we want to preserve the ordering of the element's, i.e. for the rows coming out of OpenJson to be the same as the
        // element order in the original JSON array.
        // Unfortunately, OpenJson with an explicit schema (with the WITH clause) doesn't support this; so we use the variant with the
        // default schema, which returns a 'key' column containing the index, and order by that. This also means we need to explicitly
        // apply a conversion from the values coming out of OpenJson (always NVARCHAR(MAX)) to the required relational store type.
        var openJsonExpression = new TableValuedFunctionExpression(tableAlias, "OpenJson", new[] { sqlExpression });

        // TODO: When we have metadata to determine if the element is nullable, pass that here to SelectExpression
        var selectExpression = new SelectExpression(openJsonExpression, columnName: "value", columnType: elementClrType, columnTypeMapping: elementTypeMapping, isColumnNullable: null);

        if (elementTypeMapping is { StoreType: not "nvarchar(max)" })
        {
            // For columns (where we know the type mapping), we need to overwrite the projection in order to insert a CAST() to the actual
            // relational store type we expect out of the JSON array (e.g. OpenJson returns strings, we want datetime2).
            // For parameters (where we don't yet know the type mapping), we'll need to do that later, after the type mapping has been
            // inferred.
            // TODO: Need to pass through the type mapping API for converting the JSON value (nvarchar) to the relational store type (e.g.
            // datetime2), see #30677
            selectExpression.ReplaceProjection(
                new Dictionary<ProjectionMember, Expression>
                {
                    {
                        new ProjectionMember(), _sqlExpressionFactory.Convert(
                            selectExpression.CreateColumnExpression(
                                openJsonExpression,
                                "value",
                                typeof(string),
                                _typeMappingSource.FindMapping("nvarchar(max)"),
                                // TODO: When we have metadata to determine if the element is nullable, pass that here to
                                // SelectExpression
                                columnNullable: null),
                            elementClrType,
                            elementTypeMapping)
                    }
                });
        }

        // Append an ordering for the OpenJson 'key' column, converting it from nvarchar to int.
        selectExpression.AppendOrdering(
            new OrderingExpression(
            _sqlExpressionFactory.Convert(
                selectExpression.CreateColumnExpression(
                    openJsonExpression,
                    "key",
                    typeof(string),
                    typeMapping: _typeMappingSource.FindMapping("nvarchar(8000)"),
                    columnNullable: false),
                typeof(int),
                _typeMappingSource.FindMapping(typeof(int))),
            ascending: true));

        Expression shaperExpression = new ProjectionBindingExpression(
            selectExpression, new ProjectionMember(), elementClrType.MakeNullable());

        if (elementClrType != shaperExpression.Type)
        {
            Check.DebugAssert(
                elementClrType.MakeNullable() == shaperExpression.Type,
                "expression.Type must be nullable of targetType");

            shaperExpression = Expression.Convert(shaperExpression, elementClrType);
        }

        return new ShapedQueryExpression(selectExpression, shaperExpression);
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
        IReadOnlyDictionary<(TableExpressionBase, string), RelationalTypeMapping> inferredTypeMappings)
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
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private Dictionary<TableExpressionBase, RelationalTypeMapping>? _currentSelectInferredTypeMappings;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerInferredTypeMappingApplier(
            IRelationalTypeMappingSource typeMappingSource,
            ISqlExpressionFactory sqlExpressionFactory,
            IReadOnlyDictionary<(TableExpressionBase, string), RelationalTypeMapping> inferredTypeMappings)
            : base(inferredTypeMappings)
            => (_typeMappingSource, _sqlExpressionFactory) = (typeMappingSource, sqlExpressionFactory);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitExtension(Expression expression)
        {
            switch (expression)
            {
                case TableValuedFunctionExpression { Name: "OpenJson", Schema: null, IsBuiltIn: true } openJsonExpression
                    when InferredTypeMappings.TryGetValue((openJsonExpression, "value"), out var typeMapping):
                    return ApplyTypeMappingsOnOpenJsonExpression(openJsonExpression, new[] { typeMapping });

                // Above, we applied the type mapping the the parameter that OpenJson accepts as an argument.
                // But the inferred type mapping also needs to be applied as a SQL conversion on the column projections coming out of the
                // SelectExpression containing the OpenJson call. So we set state to know about OpenJson tables and their type mappings
                // in the immediate SelectExpression, and continue visiting down (see ColumnExpression visitation below).
                case SelectExpression selectExpression:
                {
                    Dictionary<TableExpressionBase, RelationalTypeMapping>? previousSelectInferredTypeMappings = null;

                    foreach (var table in selectExpression.Tables)
                    {
                        if (table is TableValuedFunctionExpression { Name: "OpenJson", Schema: null, IsBuiltIn: true } openJsonExpression
                            && InferredTypeMappings.TryGetValue((openJsonExpression, "value"), out var inferredTypeMapping))
                        {
                            if (previousSelectInferredTypeMappings is null)
                            {
                                previousSelectInferredTypeMappings = _currentSelectInferredTypeMappings;
                                _currentSelectInferredTypeMappings = new();
                            }

                            _currentSelectInferredTypeMappings![openJsonExpression] = inferredTypeMapping;
                        }
                    }

                    var visited = base.VisitExtension(expression);

                    _currentSelectInferredTypeMappings = previousSelectInferredTypeMappings;

                    return visited;
                }

                case ColumnExpression { Name: "value" } columnExpression
                    when _currentSelectInferredTypeMappings is not null
                    && _currentSelectInferredTypeMappings.TryGetValue(columnExpression.Table, out var inferredTypeMapping):
                    return ApplyTypeMappingOnColumn(columnExpression, inferredTypeMapping);

                default:
                    return base.VisitExtension(expression);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual TableValuedFunctionExpression ApplyTypeMappingsOnOpenJsonExpression(
            TableValuedFunctionExpression openJsonExpression,
            IReadOnlyList<RelationalTypeMapping> typeMappings)
        {
            Check.DebugAssert(typeMappings.Count == 1, "typeMappings.Count == 1");
            var elementTypeMapping = typeMappings[0];

            // Constant queryables are translated to VALUES, no need for JSON.
            // Column queryables have their type mapping from the model, so we don't ever need to apply an inferred mapping on them.
            if (openJsonExpression.Arguments[0] is not SqlParameterExpression parameterExpression)
            {
                return openJsonExpression;
            }

            // TODO: We shouldn't need to manually construct the JSON string type mapping this way; we need to be able to provide the
            // TODO: element's store type mapping as input to _typeMappingSource.FindMapping.
            // TODO: When this is done, revert converter equality check in QuerySqlGenerator.VisitSqlParameter back to reference equality,
            // since we'll always have the same instance of the type mapping returned from the type mapping source. Also remove
            // CollectionToJsonStringConverter.Equals etc.
            // TODO: Note: NpgsqlTypeMappingSource exposes FindContainerMapping() for this purpose.
            if (_typeMappingSource.FindMapping(typeof(string)) is not SqlServerStringTypeMapping parameterTypeMapping)
            {
                throw new InvalidOperationException("Type mapping for 'string' could not be found or was not a SqlServerStringTypeMapping");
            }

            parameterTypeMapping = (SqlServerStringTypeMapping)parameterTypeMapping
                .Clone(new CollectionToJsonStringConverter(parameterExpression.Type, elementTypeMapping));

            parameterTypeMapping = (SqlServerStringTypeMapping)parameterTypeMapping.CloneWithElementTypeMapping(elementTypeMapping);

            var arguments = openJsonExpression.Arguments.ToArray();
            arguments[0] = parameterExpression.ApplyTypeMapping(parameterTypeMapping);
            return openJsonExpression.Update(arguments);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression ApplyTypeMappingOnColumn(ColumnExpression columnExpression, RelationalTypeMapping typeMapping)
            // OpenJson's value column has type nvarchar(max); apply a CAST() unless that's the inferred element type mapping
            => typeMapping.StoreType is "nvarchar(max)"
                ? columnExpression
                : _sqlExpressionFactory.Convert(columnExpression, typeMapping.ClrType, typeMapping);
    }
}
