// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class SelectExpression : TableExpressionBase
    {
        private readonly List<ColumnExpression> _projection = new List<ColumnExpression>();
        private readonly List<TableExpressionBase> _tables = new List<TableExpressionBase>();
        private readonly List<Ordering> _orderBy = new List<Ordering>();

        private int? _limit;
        private int? _offset;
        private bool _projectStar;

        private int? _subqueryDepth;

        private Expression _projectionExpression;
        private bool _isDistinct;

        public virtual Expression Predicate { get; [param: CanBeNull] set; }

        public SelectExpression()
            : base(null, null)
        {
        }

        public SelectExpression([NotNull] string alias)
            : base(null, Check.NotEmpty(alias, "alias"))
        {
        }

        public virtual SelectExpression Clone([NotNull] string alias)
        {
            Check.NotEmpty(alias, "alias");

            var selectExpression
                = new SelectExpression(alias)
                    {
                        _limit = _limit,
                        _offset = _offset,
                        _isDistinct = _isDistinct,
                        _subqueryDepth = _subqueryDepth,
                        _projectStar = _projectStar,
                        Predicate = Predicate
                    };

            selectExpression._projection.AddRange(_projection);

            selectExpression.AddTables(_tables);
            selectExpression.AddToOrderBy(_orderBy);

            return selectExpression;
        }

        public virtual IReadOnlyList<TableExpressionBase> Tables
        {
            get { return _tables; }
        }

        public virtual bool IsProjectStar
        {
            get { return _projectStar; }
        }

        public virtual void AddTable([NotNull] TableExpressionBase tableExpression)
        {
            Check.NotNull(tableExpression, "tableExpression");

            _tables.Add(tableExpression);
        }

        public virtual void AddTables([NotNull] IEnumerable<TableExpressionBase> tableExpressions)
        {
            Check.NotNull(tableExpressions, "tableExpressions");

            _tables.AddRange(tableExpressions);
        }

        public virtual void ClearTables()
        {
            _tables.Clear();
        }

        public virtual bool HandlesQuerySource([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, "querySource");

            return _tables.Any(tableExpression => tableExpression.QuerySource == querySource);
        }

        public virtual TableExpressionBase FindTableForQuerySource([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, "querySource");

            return _tables.Single(t => t.QuerySource == querySource);
        }

        public virtual bool IsDistinct
        {
            get { return _isDistinct; }
            set
            {
                if (_offset != null)
                {
                    PushDownSubquery();
                }

                _isDistinct = value;
            }
        }

        public virtual int? Limit
        {
            get { return _limit; }
            set
            {
                Check.NotNull(value, "value");

                PushDownIfLimit();

                _limit = value;
            }
        }

        public virtual int? Offset
        {
            get { return _offset; }
            set
            {
                Check.NotNull(value, "value");

                if (_limit != null)
                {
                    var subquery = PushDownSubquery();

                    subquery._offset = null;

                    foreach (var ordering in subquery.OrderBy)
                    {
                        var columnExpression = (ColumnExpression)ordering.Expression;

                        _orderBy.Add(
                            new Ordering(
                                new ColumnExpression(
                                    columnExpression.Name,
                                    columnExpression.Property,
                                    subquery),
                                ordering.OrderingDirection));
                    }
                }

                _offset = value;
            }
        }

        private void PushDownIfLimit()
        {
            if (_limit != null)
            {
                PushDownSubquery();
            }
        }

        private void PushDownIfDistinct()
        {
            if (_isDistinct)
            {
                PushDownSubquery();
            }
        }

        private SelectExpression PushDownSubquery()
        {
            _subqueryDepth = _subqueryDepth != null
                ? _subqueryDepth + 1
                : 0;

            var subquery
                = new SelectExpression("t" + _subqueryDepth);

            var columnAliasCounter = 0;

            foreach (var columnExpression in _projection)
            {
                if (subquery._projection.FindIndex(ce => ce.Name == columnExpression.Name) != -1)
                {
                    columnExpression.Alias = "c" + columnAliasCounter++;
                }

                subquery._projection.Add(columnExpression);
            }

            subquery.AddTables(_tables);
            subquery.AddToOrderBy(_orderBy);

            subquery._limit = _limit;
            subquery._offset = _offset;
            subquery._isDistinct = _isDistinct;
            subquery._subqueryDepth = _subqueryDepth;
            subquery._projectStar = _projectStar;

            _limit = null;
            _offset = null;
            _isDistinct = false;

            ClearTables();
            ClearProjection();
            ClearOrderBy();

            _projectStar = true;

            AddTable(subquery);

            return subquery;
        }

        public virtual IReadOnlyList<ColumnExpression> Projection
        {
            get { return _projection; }
        }

        public virtual Expression ProjectionExpression
        {
            get { return _projectionExpression; }
        }

        public virtual void AddToProjection(
            [NotNull] string column,
            [NotNull] IProperty property, 
            [NotNull] IQuerySource querySource)
        {
            Check.NotEmpty(column, "column");
            Check.NotNull(property, "property");
            Check.NotNull(querySource, "querySource");

            if (GetProjectionIndex(property, querySource) == -1)
            {
                _projection.Add(
                    new ColumnExpression(column, property, FindTableForQuerySource(querySource)));
            }
        }

        public virtual void AddToProjection([NotNull] ColumnExpression columnExpression)
        {
            Check.NotNull(columnExpression, "columnExpression");

            if (_projection
                .FindIndex(ce =>
                    ce.Property == columnExpression.Property
                    && ce.TableAlias == columnExpression.TableAlias) == -1)
            {
                _projection.Add(columnExpression);
            }
        }

        public virtual void SetProjectionCaseExpression([NotNull] CaseExpression caseExpression)
        {
            Check.NotNull(caseExpression, "caseExpression");

            ClearProjection();

            _projectionExpression = caseExpression;
        }

        public virtual void SetProjectionExpression([NotNull] Expression expression)
        {
            Check.NotNull(expression, "expression");

            PushDownIfLimit();
            PushDownIfDistinct();

            ClearProjection();

            _projectionExpression = expression;
        }

        public virtual void ClearProjection()
        {
            _projection.Clear();
        }

        public virtual void RemoveRangeFromProjection(int index)
        {
            if (index < _projection.Count)
            {
                _projection.RemoveRange(index, _projection.Count - index);
            }
        }

        public virtual void RemoveFromProjection([NotNull] IEnumerable<Ordering> orderBy)
        {
            Check.NotNull(orderBy, "orderBy");

            _projection.RemoveAll(ce => orderBy.Any(o => ReferenceEquals(o.Expression, ce)));
        }

//        public void RemoveFromProjection([NotNull] IQuerySource querySource)
//        {
//            Check.NotNull(querySource, "querySource");
//
//            _projection.RemoveAll(ce => ce.Table.QuerySource == querySource);
//        }

        public virtual int GetProjectionIndex([NotNull] IProperty property, [NotNull] IQuerySource querySource)
        {
            Check.NotNull(property, "property");
            Check.NotNull(querySource, "querySource");

            var table = FindTableForQuerySource(querySource);

            return _projection.FindIndex(ce => ce.Property == property && ce.TableAlias == table.Alias);
        }

        public virtual ColumnExpression AddToOrderBy(
            [NotNull] string column,
            [NotNull] IProperty property,
            [NotNull] IQuerySource querySource,
            OrderingDirection orderingDirection)
        {
            Check.NotEmpty(column, "column");
            Check.NotNull(property, "property");
            Check.NotNull(property, "querySource");

            var columnExpression
                = new ColumnExpression(
                    column,
                    property, 
                    FindTableForQuerySource(querySource));

            if (_orderBy.FindIndex(o => o.Expression.Equals(columnExpression)) == -1)
            {
                _orderBy.Add(new Ordering(columnExpression, orderingDirection));
            }

            return columnExpression;
        }

        public virtual void AddToOrderBy([NotNull] IEnumerable<Ordering> orderings)
        {
            Check.NotNull(orderings, "orderings");

            _orderBy.AddRange(orderings);
        }

        public virtual IReadOnlyList<Ordering> OrderBy
        {
            get { return _orderBy; }
        }

        public virtual void ClearOrderBy()
        {
            _orderBy.Clear();
        }

        public virtual void AddCrossJoin(
            [NotNull] TableExpressionBase tableExpression,
            [NotNull] IEnumerable<ColumnExpression> projection)
        {
            Check.NotNull(tableExpression, "tableExpression");
            Check.NotNull(projection, "projection");

            _tables.Add(new CrossJoinExpression(tableExpression));

            _projection.AddRange(projection);
        }

        public virtual JoinExpressionBase AddInnerJoin([NotNull] TableExpressionBase tableExpression)
        {
            Check.NotNull(tableExpression, "tableExpression");

            return AddInnerJoin(tableExpression, Enumerable.Empty<ColumnExpression>());
        }

        public virtual JoinExpressionBase AddInnerJoin(
            [NotNull] TableExpressionBase tableExpression,
            [NotNull] IEnumerable<ColumnExpression> projection)
        {
            Check.NotNull(tableExpression, "tableExpression");
            Check.NotNull(projection, "projection");

            var innerJoinExpression = new InnerJoinExpression(tableExpression);

            _tables.Add(innerJoinExpression);
            _projection.AddRange(projection);

            return innerJoinExpression;
        }

        public virtual JoinExpressionBase AddOuterJoin(
            [NotNull] TableExpressionBase tableExpression,
            [NotNull] IEnumerable<ColumnExpression> projection)
        {
            Check.NotNull(tableExpression, "tableExpression");
            Check.NotNull(projection, "projection");

            var outerJoinExpression = new LeftOuterJoinExpression(tableExpression);

            _tables.Add(outerJoinExpression);
            _projection.AddRange(projection);

            return outerJoinExpression;
        }

        public virtual void RemoveTable([NotNull] TableExpressionBase tableExpression)
        {
            Check.NotNull(tableExpression, "tableExpression");

            _tables.Remove(tableExpression);
        }

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitSelectExpression(this)
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            return this;
        }

        public override string ToString()
        {
            return new DefaultSqlQueryGenerator().GenerateSql(this);
        }
    }
}
