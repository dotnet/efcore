// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerQueryableMethodTranslatingExpressionVisitor : RelationalQueryableMethodTranslatingExpressionVisitor
{
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

            var tableExpressions = ExtractTableExpressions(selectExpression);
            ValidateAllTablesHaveSameAnnotations(tableExpressions);
            foreach (var tableExpression in tableExpressions)
            {
                switch (queryRootExpression)
                {
                    case TemporalAllQueryRootExpression:
                        tableExpression[SqlServerAnnotationNames.TemporalOperationType] = TemporalOperationType.All;
                        break;

                    case TemporalAsOfQueryRootExpression asOf:
                        tableExpression[SqlServerAnnotationNames.TemporalOperationType] = TemporalOperationType.AsOf;
                        tableExpression[SqlServerAnnotationNames.TemporalAsOfPointInTime] = asOf.PointInTime;
                        break;

                    case TemporalBetweenQueryRootExpression between:
                        tableExpression[SqlServerAnnotationNames.TemporalOperationType] = TemporalOperationType.Between;
                        tableExpression[SqlServerAnnotationNames.TemporalRangeOperationFrom] = between.From;
                        tableExpression[SqlServerAnnotationNames.TemporalRangeOperationTo] = between.To;
                        break;

                    case TemporalContainedInQueryRootExpression containedIn:
                        tableExpression[SqlServerAnnotationNames.TemporalOperationType] = TemporalOperationType.ContainedIn;
                        tableExpression[SqlServerAnnotationNames.TemporalRangeOperationFrom] = containedIn.From;
                        tableExpression[SqlServerAnnotationNames.TemporalRangeOperationTo] = containedIn.To;
                        break;

                    case TemporalFromToQueryRootExpression fromTo:
                        tableExpression[SqlServerAnnotationNames.TemporalOperationType] = TemporalOperationType.FromTo;
                        tableExpression[SqlServerAnnotationNames.TemporalRangeOperationFrom] = fromTo.From;
                        tableExpression[SqlServerAnnotationNames.TemporalRangeOperationTo] = fromTo.To;
                        break;

                    default:
                        throw new InvalidOperationException(queryRootExpression.Print());
                }
            }

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

    private List<TableExpressionBase> ExtractTableExpressions(TableExpressionBase tableExpressionBase)
    {
        if (tableExpressionBase is JoinExpressionBase joinExpression)
        {
            tableExpressionBase = joinExpression.Table;
        }

        if (tableExpressionBase is TableExpression tableExpression)
        {
            return new List<TableExpressionBase> { tableExpression };
        }

        if (tableExpressionBase is SelectExpression selectExpression)
        {
            var result = new List<TableExpressionBase>();
            foreach (var table in selectExpression.Tables)
            {
                result.AddRange(ExtractTableExpressions(table));
            }

            return result;
        }

        if (tableExpressionBase is SetOperationBase setOperationBase)
        {
            var result = new List<TableExpressionBase>();
            result.AddRange(ExtractTableExpressions(setOperationBase.Source1));
            result.AddRange(ExtractTableExpressions(setOperationBase.Source2));

            return result;
        }

        throw new InvalidOperationException("Unsupported table expression base type.");
    }

    private void ValidateAllTablesHaveSameAnnotations(List<TableExpressionBase> tableExpressions)
    {
        List<IAnnotation>? expectedAnnotations = null;
        foreach (var tableExpression in tableExpressions)
        {
            if (expectedAnnotations == null)
            {
                expectedAnnotations = new List<IAnnotation>(tableExpression.GetAnnotations().OrderBy(x => x.Name));
            }
            else
            {
                var annotations = tableExpression.GetAnnotations().OrderBy(x => x.Name).ToList();
                if (expectedAnnotations.Count != annotations.Count
                    || expectedAnnotations.Zip(annotations, (e, a) => e.Name != a.Name || e.Value != a.Value).Any())
                {
                    throw new InvalidOperationException("Annotations for all tables representing an entity type must match.");
                }
            }
        }
    }
}
