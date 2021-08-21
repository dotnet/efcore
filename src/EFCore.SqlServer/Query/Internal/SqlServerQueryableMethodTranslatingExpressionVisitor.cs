// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
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
                // sql server model validator will throw if entity is mapped to multiple tables
                var table = queryRootExpression.EntityType.GetTableMappings().Single().Table;
                var temporalTableExpression = queryRootExpression switch
                {
                    TemporalAllQueryRootExpression _ => (TemporalTableExpression)new TemporalAllTableExpression(table),
                    TemporalAsOfQueryRootExpression asOf => new TemporalAsOfTableExpression(table, asOf.PointInTime),
                    TemporalBetweenQueryRootExpression between => new TemporalBetweenTableExpression(table, between.From, between.To),
                    TemporalContainedInQueryRootExpression containedIn => new TemporalContainedInTableExpression(table, containedIn.From, containedIn.To),
                    TemporalFromToQueryRootExpression fromTo => new TemporalFromToTableExpression(table, fromTo.From, fromTo.To),
                    _ => throw new InvalidOperationException(queryRootExpression.Print())
                };

                var selectExpression = RelationalDependencies.SqlExpressionFactory.Select(
                    queryRootExpression.EntityType,
                    temporalTableExpression);

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
    }
}
