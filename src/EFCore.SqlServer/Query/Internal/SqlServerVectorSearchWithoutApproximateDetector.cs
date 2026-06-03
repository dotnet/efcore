// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Extensions.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     Walks the expression tree looking for SelectExpressions that contain a VECTOR_SEARCH TVF without
///     a <see cref="WithApproximateExpression" /> as their Limit, and emits a warning for each.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class SqlServerVectorSearchWithoutApproximateDetector(SqlServerQueryCompilationContext queryCompilationContext)
    : ExpressionVisitor
{
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
            case ShapedQueryExpression shapedQuery:
                Visit(shapedQuery.QueryExpression);
                return node;

            case SelectExpression { Limit: not WithApproximateExpression } select:
                var relationalModel = queryCompilationContext.Model.GetRelationalModel();

                foreach (var table in select.Tables)
                {
#pragma warning disable EF9105 // IsVectorIndex is experimental
                    if (table is TableValuedFunctionExpression
                        {
                            Name: "VECTOR_SEARCH",
                            Arguments: [TableExpression tableExpr, ColumnExpression columnExpr, ..]
                        }
                        && relationalModel.FindTable(tableExpr.Name, tableExpr.Schema) is { } relationalTable
                        && relationalTable.Indexes.Any(
                            i => i.Columns is [{ Name: var c }] && c == columnExpr.Name
                                && i.MappedIndexes.Any(mi => mi.IsVectorIndex())))
                    {
                        var entityType = relationalTable.EntityTypeMappings.FirstOrDefault()?.TypeBase;

                        queryCompilationContext.Logger.VectorSearchWithoutApproximateWarning(
                            columnExpr.Name,
                            entityType?.DisplayName() ?? tableExpr.Name);
                    }
#pragma warning restore EF9105
                }

                return base.VisitExtension(node);

            default:
                return base.VisitExtension(node);
        }
    }
}
