// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class QueryModelPrinter : IQueryModelPrinter
    {
        private IndentedStringBuilder _stringBuilder;
        private QueryModelExpressionPrinter _expressionPrinter;
        private QueryModelPrintingVisitor _queryModelPrintingVisitor;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public QueryModelPrinter()
        {
            _expressionPrinter = new QueryModelExpressionPrinter();
            _stringBuilder = _expressionPrinter.StringBuilder;
            _queryModelPrintingVisitor = new QueryModelPrintingVisitor(_expressionPrinter);
            _expressionPrinter.SetQueryModelPrintingVisitor(_queryModelPrintingVisitor);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Print([NotNull] QueryModel queryModel)
        {
            _stringBuilder.Clear();

            _queryModelPrintingVisitor.VisitQueryModel(queryModel);

            return _stringBuilder.ToString();
        }

        private class QueryModelExpressionPrinter : ExpressionPrinter
        {
            private QueryModelPrintingVisitor _queryModelPrintingVisitor;

            public void SetQueryModelPrintingVisitor(QueryModelPrintingVisitor queryModelPrintingVisitor)
            {
                _queryModelPrintingVisitor = queryModelPrintingVisitor;
            }

            protected override Expression VisitExtension(Expression node)
            {
                var subquery = node as SubQueryExpression;
                if (subquery != null)
                {
                    using (StringBuilder.Indent())
                    {
                        var isSubquery = _queryModelPrintingVisitor.IsSubquery;
                        _queryModelPrintingVisitor.IsSubquery = true;
                        _queryModelPrintingVisitor.VisitQueryModel(subquery.QueryModel);
                        _queryModelPrintingVisitor.IsSubquery = isSubquery;
                    }

                    return node;
                }

                return base.VisitExtension(node);
            }
        }

        private class QueryModelPrintingVisitor : ExpressionTransformingQueryModelVisitor<ExpressionPrinter>
        {
            public QueryModelPrintingVisitor([NotNull] ExpressionPrinter expressionPrinter) 
                : base(expressionPrinter)
            {
            }

            public bool IsSubquery { get; set; }

            public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
            {
                if (IsSubquery)
                {
                    TransformingVisitor.StringBuilder.AppendLine();
                }

                if (queryModel.ResultOperators.Count > 0)
                {
                    TransformingVisitor.StringBuilder.Append("(");
                }

                TransformingVisitor.StringBuilder.Append($"from {fromClause.ItemType.ShortDisplayName()} {fromClause.ItemName} in ");
                base.VisitMainFromClause(fromClause, queryModel);
            }

            public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
            {
                TransformingVisitor.StringBuilder.AppendLine();
                TransformingVisitor.StringBuilder.Append($"from {fromClause.ItemType.ShortDisplayName()} {fromClause.ItemName} in ");
                base.VisitAdditionalFromClause(fromClause, queryModel, index);
            }

            public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
            {
                base.VisitJoinClause(joinClause, queryModel, index);
            }

            public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause)
            {
                TransformingVisitor.StringBuilder.AppendLine();
                TransformingVisitor.StringBuilder.Append("on ");
                TransformingVisitor.Visit(joinClause.OuterKeySelector);
                TransformingVisitor.StringBuilder.Append(" equals ");
                TransformingVisitor.Visit(joinClause.InnerKeySelector);
            }

            public override void VisitGroupJoinClause(GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
            {
                TransformingVisitor.StringBuilder.AppendLine();
                TransformingVisitor.StringBuilder.Append($"join {groupJoinClause.ItemType.ShortDisplayName()} {groupJoinClause.ItemName}");
                base.VisitGroupJoinClause(groupJoinClause, queryModel, index);
                TransformingVisitor.StringBuilder.Append($" into {groupJoinClause.ItemName}");
            }

            public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
            {
                TransformingVisitor.StringBuilder.AppendLine();
                TransformingVisitor.StringBuilder.Append($"where ");
                base.VisitWhereClause(whereClause, queryModel, index);
            }

            public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
            {
                TransformingVisitor.StringBuilder.AppendLine();
                TransformingVisitor.StringBuilder.Append("order by ");

                var first = true;
                foreach (var ordering in orderByClause.Orderings)
                {
                    VisitOrdering(ordering, queryModel, orderByClause, index);
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        TransformingVisitor.StringBuilder.Append(", ");
                    }
                }
            }

            public override void VisitOrdering(Ordering ordering, QueryModel queryModel, OrderByClause orderByClause, int index)
            {
                base.VisitOrdering(ordering, queryModel, orderByClause, index);

                TransformingVisitor.StringBuilder.Append($" {ordering.OrderingDirection.ToString().ToLower()}");
            }

            protected override void VisitResultOperators(ObservableCollection<ResultOperatorBase> resultOperators, QueryModel queryModel)
            {
                if (resultOperators.Count > 0)
                {
                    TransformingVisitor.StringBuilder.Append(")");
                }

                base.VisitResultOperators(resultOperators, queryModel);
            }

            public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
            {
                TransformingVisitor.StringBuilder.AppendLine();
                TransformingVisitor.StringBuilder.Append($".{resultOperator.ToString()}");
            }

            public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
            {
                TransformingVisitor.StringBuilder.AppendLine();
                TransformingVisitor.StringBuilder.Append("select ");
                base.VisitSelectClause(selectClause, queryModel);
            }
        }
    }
}
