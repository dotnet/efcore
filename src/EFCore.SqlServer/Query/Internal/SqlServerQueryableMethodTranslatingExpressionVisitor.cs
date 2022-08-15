// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
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
        {
            return expression is TableExpression tableExpression
                ? _annotationApplyingFunc(tableExpression)
                : base.Visit(expression);
        }
    }
}
