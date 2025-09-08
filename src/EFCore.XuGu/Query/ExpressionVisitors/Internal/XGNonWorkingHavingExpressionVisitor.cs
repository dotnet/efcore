// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.XuGu.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal
{
    // TODO: 9.0
    // Remove from codebase.
    public class XGNonWorkingHavingExpressionVisitor : ExpressionVisitor
    {
        private readonly XGSqlExpressionFactory _sqlExpressionFactory;
        private readonly SqlAliasManager _sqlAliasManager;
        private XGContainsAggregateFunctionExpressionVisitor _containsAggregateFunctionExpressionVisitor;

        public XGNonWorkingHavingExpressionVisitor(XGSqlExpressionFactory sqlExpressionFactory, SqlAliasManager sqlAliasManager)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _sqlAliasManager = sqlAliasManager;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
            => extensionExpression switch
            {
                SelectExpression selectExpression => VisitSelectMutable(selectExpression),
                // SelectExpression selectExpression => VisitSelectImmutable(selectExpression),
                ShapedQueryExpression shapedQueryExpression => VisitShapedQuery(shapedQueryExpression),
                _ => base.VisitExtension(extensionExpression)
            };

        private ShapedQueryExpression VisitShapedQuery(ShapedQueryExpression shapedQueryExpression)
            => shapedQueryExpression.Update(
                (SelectExpression)Visit(shapedQueryExpression.QueryExpression),
                Visit(shapedQueryExpression.ShaperExpression));

        /// <summary>
        /// This might work, if we would know if the outer query is supposed to be mutable or not (which we cannot directly find out
        /// because `SelectExpression.IsMutable` is internal) and if we could copy or recreate the projection mappings of the outer query.
        /// </summary>
        protected virtual Expression VisitSelectMutable(SelectExpression selectExpression)
        {
            // MySQL & MariaDB currently do not support complex expressions in HAVING clauses (e.g. function calls).
            // Instead, they want you to reference SELECT aliases for those expressions in the HAVING clause.
            // See https://bugs.mysql.com/bug.php?id=103961
            // This is only an issue for HAVING expressions that do not contain any aggregate functions.
            var havingExpression = selectExpression.Having;
            if (havingExpression is not null &&
                havingExpression is not SqlConstantExpression &&
                havingExpression is not XGColumnAliasReferenceExpression)
            {
                _containsAggregateFunctionExpressionVisitor ??= new XGContainsAggregateFunctionExpressionVisitor();
                if (!_containsAggregateFunctionExpressionVisitor.ProcessUntilSelect(havingExpression))
                {
                    var newSelectExpression = selectExpression.Clone();

                    newSelectExpression.PushdownIntoSubquery();

                    var subQuery = (SelectExpression)newSelectExpression.Tables.Single();
                    var projectionIndex = subQuery.AddToProjection(havingExpression);
                    var alias = subQuery.Projection[projectionIndex].Alias;

                    var columnAliasReferenceExpression = _sqlExpressionFactory.ColumnAliasReference(
                        alias,
                        havingExpression,
                        havingExpression.Type,
                        havingExpression.TypeMapping);

                    // Having expressions, not containing an aggregate function, need to be part of the GROUP BY clause, because they now also
                    // appear as part of the SELECT clause.
                    var groupBy = subQuery.GroupBy.ToList();
                    groupBy.Add(columnAliasReferenceExpression);

                    subQuery = new SelectExpression(
                        subQuery.Alias,
                        subQuery.Tables.ToList(),
                        subQuery.Predicate,
                        groupBy,
                        columnAliasReferenceExpression,
                        subQuery.Projection.ToList(),
                        subQuery.IsDistinct,
                        subQuery.Orderings.ToList(),
                        subQuery.Offset,
                        subQuery.Limit,
                        subQuery.Tags,
                        subQuery.GetAnnotations().ToDictionary(a => a.Name, a => a),
                        _sqlAliasManager,
                        isMutable: false
                    );

                    newSelectExpression = new SelectExpression(
                        newSelectExpression.Alias,
                        [subQuery],
                        newSelectExpression.Predicate,
                        newSelectExpression.GroupBy.ToList(),
                        newSelectExpression.Having,
                        projections: [],
                        newSelectExpression.IsDistinct,
                        newSelectExpression.Orderings.ToList(),
                        newSelectExpression.Offset,
                        newSelectExpression.Limit,
                        newSelectExpression.Tags,
                        newSelectExpression.GetAnnotations().ToDictionary(a => a.Name, a => a),
                        _sqlAliasManager,
                        isMutable: true
                    );

                    //
                    // UNSOLVED: Somehow recreate projection mappings here.
                    //

                    selectExpression = newSelectExpression;
                }
            }

            return base.VisitExtension(selectExpression);
        }

        /// <summary>
        /// This basically needs to reimplement `SelectExpression.PushdownIntoSubquery()`, which we are definitely not going to do.
        /// </summary>
        protected virtual Expression VisitSelectImmutable(SelectExpression selectExpression)
        {
            // MySQL & MariaDB currently do not support complex expressions in HAVING clauses (e.g. function calls).
            // Instead, they want you to reference SELECT aliases for those expressions in the HAVING clause.
            // See https://bugs.mysql.com/bug.php?id=103961
            // This is only an issue for HAVING expressions that do not contain any aggregate functions.
            var havingExpression = selectExpression.Having;
            if (havingExpression is not null &&
                havingExpression is not SqlConstantExpression &&
                havingExpression is not XGColumnAliasReferenceExpression)
            {
                _containsAggregateFunctionExpressionVisitor ??= new XGContainsAggregateFunctionExpressionVisitor();
                if (!_containsAggregateFunctionExpressionVisitor.ProcessUntilSelect(havingExpression))
                {
                    var subquery = selectExpression.Clone();
                    subquery.ReplaceProjection([]);

                    var alias = "having";
                    var havingProjectionExpression = new ProjectionExpression(havingExpression, alias);
                    var columnAliasReferenceExpression = _sqlExpressionFactory.ColumnAliasReference(
                        alias,
                        havingExpression,
                        havingExpression.Type,
                        havingExpression.TypeMapping);

                    // Having expressions, not containing an aggregate function, need to be part of the GROUP BY clause, because they now also
                    // appear as part of the SELECT clause.
                    subquery = subquery.Update(
                        subquery.Tables,
                        subquery.Predicate,
                        subquery.GroupBy.Append(columnAliasReferenceExpression).ToList(),
                        columnAliasReferenceExpression,
                        subquery.Projection.Append(havingProjectionExpression).ToList(),
                        subquery.Limit is not null || subquery.Offset is not null
                            ? subquery.Orderings
                            : [],
                        subquery.Offset, // Offset/limit parameters got switched around between EF Core 8 and 9 for no good reason.
                        subquery.Limit);


                    var outerSelectOrderings = selectExpression.Orderings;

                    // foreach (var ordering in subquery.Orderings)
                    // {
                    //     var orderingExpression = ordering.Expression;
                    //     if (liftOrderings && projectionMap.TryGetValue(orderingExpression, out var outerColumn))
                    //     {
                    //         _orderings.Add(ordering.Update(outerColumn));
                    //     }
                    //     else if (liftOrderings
                    //              && (!IsDistinct
                    //                  && GroupBy.Count == 0
                    //                  || GroupBy.Contains(orderingExpression)))
                    //     {
                    //         _orderings.Add(
                    //             ordering.Update(
                    //                 subquery.GenerateOuterColumn(subqueryAlias, orderingExpression)));
                    //     }
                    //     else
                    //     {
                    //         _orderings.Clear();
                    //         break;
                    //     }
                    // }


                    selectExpression = selectExpression.Update(
                        [subquery],
                        selectExpression.Predicate,
                        groupBy: [],
                        having: null,
                        subquery.Projection/*.Select(p => new ProjectionExpression(new ColumnExpression(p.Alias, subquery.Alias, p.Type, null)))*/,
                        outerSelectOrderings,
                        null,
                        null);
                }
            }

            return base.VisitExtension(selectExpression);
        }

        // private ColumnExpression GenerateOuterColumn(
        //     SelectExpression subquery,
        //     string tableAlias,
        //     SqlExpression projection/*,
        //     string columnAlias = null*/)
        // {
        //     // TODO: Add check if we can add projection in subquery to generate out column
        //     // Subquery having Distinct or GroupBy can block it.
        //     var index = subquery.AddToProjection(projection);
        //     var projectionExpression = subquery.Projection[index];
        //     return CreateColumnExpression(projectionExpression, tableAlias);
        // }
        //
        // private static ColumnExpression CreateColumnExpression(ProjectionExpression subqueryProjection, string tableAlias)
        //     => new(
        //         subqueryProjection.Alias,
        //         tableAlias,
        //         subqueryProjection.Type,
        //         subqueryProjection.Expression.TypeMapping!,
        //         subqueryProjection.Expression switch
        //         {
        //             ColumnExpression columnExpression => columnExpression.IsNullable,
        //             SqlConstantExpression sqlConstantExpression => sqlConstantExpression.Value == null,
        //             _ => true
        //         });
    }
}
