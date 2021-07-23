// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlServerQuerySqlGenerator : QuerySqlGenerator
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerQuerySqlGenerator(QuerySqlGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void GenerateTop(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            if (selectExpression.Limit != null
                && selectExpression.Offset == null)
            {
                Sql.Append("TOP(");

                Visit(selectExpression.Limit);

                Sql.Append(") ");
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void GenerateOrderings(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            base.GenerateOrderings(selectExpression);

            // In SQL Server, if an offset is specified, then an ORDER BY clause must also exist.
            // Generate a fake one.
            if (!selectExpression.Orderings.Any() && selectExpression.Offset != null)
            {
                Sql.AppendLine().Append("ORDER BY (SELECT 1)");
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void GenerateLimitOffset(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            // Note: For Limit without Offset, SqlServer generates TOP()
            if (selectExpression.Offset != null)
            {
                Sql.AppendLine()
                    .Append("OFFSET ");

                Visit(selectExpression.Offset);

                Sql.Append(" ROWS");

                if (selectExpression.Limit != null)
                {
                    Sql.Append(" FETCH NEXT ");

                    Visit(selectExpression.Limit);

                    Sql.Append(" ROWS ONLY");
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is TemporalTableExpression temporalTableExpression)
            {
                Sql
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(temporalTableExpression.Name, temporalTableExpression.Schema))
                    .Append(" FOR SYSTEM_TIME ");

                switch (temporalTableExpression.TemporalOperationType)
                {
                    case TemporalOperationType.AsOf:
                        Sql
                            .Append("AS OF ")
                            .Append(Sql.TypeMappingSource.GetMapping(typeof(DateTime)).GenerateSqlLiteral(temporalTableExpression.PointInTime));
                        break;

                    case TemporalOperationType.FromTo:
                        Sql
                            .Append("FROM ")
                            .Append(Sql.TypeMappingSource.GetMapping(typeof(DateTime)).GenerateSqlLiteral(temporalTableExpression.From))
                            .Append(" TO ")
                            .Append(Sql.TypeMappingSource.GetMapping(typeof(DateTime)).GenerateSqlLiteral(temporalTableExpression.To));
                        break;

                    case TemporalOperationType.Between:
                        Sql
                            .Append("BETWEEN ")
                            .Append(Sql.TypeMappingSource.GetMapping(typeof(DateTime)).GenerateSqlLiteral(temporalTableExpression.From))
                            .Append(" AND ")
                            .Append(Sql.TypeMappingSource.GetMapping(typeof(DateTime)).GenerateSqlLiteral(temporalTableExpression.To));
                        break;

                    case TemporalOperationType.ContainedIn:
                        Sql
                            .Append("CONTAINED IN (")
                            .Append(Sql.TypeMappingSource.GetMapping(typeof(DateTime)).GenerateSqlLiteral(temporalTableExpression.From))
                            .Append(", ")
                            .Append(Sql.TypeMappingSource.GetMapping(typeof(DateTime)).GenerateSqlLiteral(temporalTableExpression.To))
                            .Append(")");
                        break;

                    default:
                        Sql.Append("ALL");
                        break;
                }

                if (temporalTableExpression.Alias != null)
                {
                    Sql
                        .Append(AliasSeparator)
                        .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(temporalTableExpression.Alias));
                }

                return temporalTableExpression;
            }

            return base.VisitExtension(extensionExpression);
        }
    }
}
