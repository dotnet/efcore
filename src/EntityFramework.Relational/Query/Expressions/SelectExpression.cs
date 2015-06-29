// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.Expressions
{
    public class SelectExpression : TableExpressionBase
    {
#if DEBUG
        internal string DebugView => ToString();
#endif

        private readonly List<Expression> _projection = new List<Expression>();
        private readonly List<TableExpressionBase> _tables = new List<TableExpressionBase>();
        private readonly List<Ordering> _orderBy = new List<Ordering>();

        private int? _limit;
        private int? _offset;
        private bool _projectStar;

        private int? _subqueryDepth;

        private bool _isDistinct;

        public virtual Expression Predicate { get; [param: CanBeNull] set; }

        public SelectExpression()
            : base(null, null)
        {
        }

        public SelectExpression([NotNull] string alias)
            : base(null, Check.NotEmpty(alias, nameof(alias)))
        {
        }

        public virtual SelectExpression Clone([NotNull] string alias)
        {
            Check.NotEmpty(alias, nameof(alias));

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

        public virtual IReadOnlyList<TableExpressionBase> Tables => _tables;

        public virtual bool IsProjectStar => _projectStar;

        public virtual void AddTable([NotNull] TableExpressionBase tableExpression)
            => _tables.Add(Check.NotNull(tableExpression, nameof(tableExpression)));

        public virtual void AddTables([NotNull] IEnumerable<TableExpressionBase> tableExpressions)
            => _tables.AddRange(Check.NotNull(tableExpressions, nameof(tableExpressions)));

        public virtual void ClearTables() => _tables.Clear();

        public virtual bool HandlesQuerySource([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            return _tables.Any(te
                => te.QuerySource == querySource
                   || ((te as SelectExpression)?.HandlesQuerySource(querySource) ?? false));
        }

        public virtual TableExpressionBase GetTableForQuerySource([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            return _tables.First(te
                => te.QuerySource == querySource
                   || ((te as SelectExpression)?.HandlesQuerySource(querySource) ?? false));
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
                Check.NotNull(value, nameof(value));

                PushDownIfLimit();

                _limit = value;
            }
        }

        public virtual int? Offset
        {
            get { return _offset; }
            set
            {
                Check.NotNull(value, nameof(value));

                if (_limit != null)
                {
                    var subquery = PushDownSubquery();

                    subquery._offset = null;

                    foreach (var ordering in subquery.OrderBy)
                    {
                        var aliasExpression = ordering.Expression as AliasExpression;

                        if (aliasExpression != null)
                        {
                            var expression = UpdateColumnExpression(aliasExpression.Expression, subquery);

                            _orderBy.Add(
                                new Ordering(
                                    new AliasExpression(aliasExpression.Alias, expression),
                                    ordering.OrderingDirection));
                        }
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

        public virtual SelectExpression PushDownSubquery()
        {
            _subqueryDepth
                = _subqueryDepth != null
                    ? _subqueryDepth + 1
                    : 0;

            var subquery
                = new SelectExpression("t" + _subqueryDepth);

            var columnAliasCounter = 0;

            // TODO: Only AliasExpressions? Don't unique-ify here.
            foreach (var aliasExpression in _projection.OfType<AliasExpression>())
            {
                var columnExpression = ((Expression)aliasExpression).TryGetColumnExpression();

                if (columnExpression != null
                    && subquery._projection.OfType<AliasExpression>().Any(ae =>
                        ae.TryGetColumnExpression()?.Name == columnExpression.Name))
                {
                    aliasExpression.Alias = "c" + columnAliasCounter++;
                }

                subquery._projection.Add(aliasExpression);
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

        public virtual IReadOnlyList<Expression> Projection => _projection;

        public virtual int AddToProjection(
            [NotNull] string column,
            [NotNull] IProperty property,
            [NotNull] IQuerySource querySource)
        {
            Check.NotEmpty(column, nameof(column));
            Check.NotNull(property, nameof(property));
            Check.NotNull(querySource, nameof(querySource));

            var projectionIndex = GetProjectionIndex(property, querySource);

            if (projectionIndex == -1)
            {
                projectionIndex = _projection.Count;

                AddAliasToProjection(
                    alias: null,
                    expression: new ColumnExpression(column, property, GetTableForQuerySource(querySource)));
            }

            return projectionIndex;
        }

        public virtual int AddToProjection([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var columnExpression = expression as ColumnExpression;
            var aliasExpression = expression as AliasExpression;

            if (columnExpression != null)
            {
                return AddToProjection(columnExpression);
            }

            if (aliasExpression != null)
            {
                return AddToProjection(aliasExpression);
            }

            _projection.Add(expression);

            _projectStar = false;

            return _projection.Count - 1;
        }

        public virtual int AddToProjection([NotNull] AliasExpression aliasExpression)
        {
            return AddAliasToProjection(aliasExpression.Alias, aliasExpression.Expression);
        }

        public virtual int AddAliasToProjection([CanBeNull] string alias, [NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var columnExpression = expression as ColumnExpression;

            var projectionIndex
                = _projection
                    .FindIndex(e =>
                        {
                            var ae = e as AliasExpression;
                            var ce = e.TryGetColumnExpression();

                            return (ce != null && ce.Property == columnExpression?.Property
                                    && ce.TableAlias == columnExpression?.TableAlias)
                                   || ae?.Expression == expression;
                        });

            if (projectionIndex == -1)
            {
                if (Alias != null
                    || columnExpression == null)
                {
                    var currentAlias = alias ?? columnExpression?.Name ?? expression.NodeType.ToString();
                    var uniqueAlias = CreateUniqueProjectionAlias(currentAlias);

                    if (columnExpression == null
                        || !string.Equals(currentAlias, uniqueAlias, StringComparison.OrdinalIgnoreCase))
                    {
                        alias = uniqueAlias;
                    }
                }

                projectionIndex = _projection.Count;

                if (alias != null)
                {
                    foreach (var orderByAliasExpression in _orderBy.Select(o => o.Expression).OfType<AliasExpression>())
                    {
                        if (orderByAliasExpression.Expression.ToString() == expression.ToString())
                        {
                            orderByAliasExpression.Alias = alias;
                            orderByAliasExpression.Projected = true;
                        }
                    }
                }

                _projection.Add(new AliasExpression(alias, expression));

                _projectStar = false;
            }

            return projectionIndex;
        }

        public virtual int AddToProjection([NotNull] ColumnExpression columnExpression)
        {
            Check.NotNull(columnExpression, nameof(columnExpression));

            var projectionIndex
                = _projection
                    .FindIndex(e =>
                        {
                            var ce = e.TryGetColumnExpression();

                            return ce?.Property == columnExpression.Property
                                   && ce?.TableAlias == columnExpression.TableAlias;
                        });

            if (projectionIndex == -1)
            {
                var aliasExpression = new AliasExpression(columnExpression);

                if (Alias != null)
                {
                    var currentAlias = columnExpression.Name;
                    var uniqueAlias = CreateUniqueProjectionAlias(currentAlias);

                    if (!string.Equals(currentAlias, uniqueAlias, StringComparison.OrdinalIgnoreCase))
                    {
                        aliasExpression.Alias = uniqueAlias;
                    }
                }

                projectionIndex = _projection.Count;

                _projection.Add(aliasExpression);

                _projectStar = false;
            }

            return projectionIndex;
        }

        public virtual IEnumerable<Type> GetProjectionTypes()
        {
            if (_projection.Any()
                || !IsProjectStar)
            {
                return _projection.Select(e => e.Type);
            }

            return _tables.OfType<SelectExpression>().SelectMany(e => e.GetProjectionTypes());
        }

        public virtual void SetProjectionConditionalExpression([NotNull] ConditionalExpression conditionalExpression)
        {
            Check.NotNull(conditionalExpression, nameof(conditionalExpression));

            ClearProjection();
            AddToProjection(conditionalExpression);
        }

        public virtual void SetProjectionExpression([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            PushDownIfLimit();
            PushDownIfDistinct();

            ClearProjection();
            AddToProjection(expression);
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
            Check.NotNull(orderBy, nameof(orderBy));

            _projection.RemoveAll(ce => orderBy.Any(o => ReferenceEquals(o.Expression, ce)));
        }

        public virtual int GetProjectionIndex([NotNull] IProperty property, [NotNull] IQuerySource querySource)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(querySource, nameof(querySource));

            var table = GetTableForQuerySource(querySource);

            return _projection
                .FindIndex(e =>
                    {
                        var ce = e.TryGetColumnExpression();

                        return ce?.Property == property
                               && ce.TableAlias == table.Alias;
                    });
        }

        public virtual AliasExpression AddToOrderBy(
            [NotNull] string column,
            [NotNull] IProperty property,
            [NotNull] TableExpressionBase table,
            OrderingDirection orderingDirection)
        {
            Check.NotEmpty(column, nameof(column));
            Check.NotNull(property, nameof(property));
            Check.NotNull(table, nameof(table));

            var columnExpression = new ColumnExpression(column, property, table);
            var aliasExpression = new AliasExpression(columnExpression);

            if (_orderBy.FindIndex(o => o.Expression.TryGetColumnExpression()?.Equals(columnExpression) ?? false) == -1)
            {
                _orderBy.Add(new Ordering(aliasExpression, orderingDirection));
            }

            return aliasExpression;
        }

        public virtual void AddToOrderBy([NotNull] IEnumerable<Ordering> orderings)
        {
            Check.NotNull(orderings, nameof(orderings));

            foreach (var ordering in orderings)
            {
                var aliasExpression = ordering.Expression as AliasExpression;
                var columnExpression = ordering.Expression as ColumnExpression;

                if (aliasExpression != null)
                {
                    var newAlias = new AliasExpression(aliasExpression.Alias, aliasExpression.Expression);
                    _orderBy.Add(new Ordering(newAlias, ordering.OrderingDirection));
                }
                else if (columnExpression != null)
                {
                    _orderBy.Add(new Ordering(new AliasExpression(columnExpression), ordering.OrderingDirection));
                }
                else
                {
                    _orderBy.Add(ordering);
                }
            }
        }

        public virtual void AddToOrderBy([NotNull] Ordering ordering)
        {
            Check.NotNull(ordering, nameof(ordering));

            var columnExpression = ordering.Expression.TryGetColumnExpression();

            if (_orderBy.FindIndex(o => o.Expression.TryGetColumnExpression()?.Equals(columnExpression) ?? false) == -1)
            {
                _orderBy.Add(ordering);
            }
        }

        public virtual void PrependToOrderBy([NotNull] IEnumerable<Ordering> orderings)
        {
            Check.NotNull(orderings, nameof(orderings));

            _orderBy.InsertRange(0, orderings);
        }

        public virtual IReadOnlyList<Ordering> OrderBy => _orderBy;

        public virtual void ClearOrderBy()
        {
            _orderBy.Clear();
        }

        public virtual void AddCrossJoin(
            [NotNull] TableExpressionBase tableExpression,
            [NotNull] IEnumerable<Expression> projection)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));
            Check.NotNull(projection, nameof(projection));

            tableExpression.Alias = CreateUniqueTableAlias(tableExpression.Alias);

            _tables.Add(new CrossJoinExpression(tableExpression));
            _projection.AddRange(projection);
        }

        public virtual void ExplodeStarProjection()
        {
            if (_projectStar)
            {
                var subquery = (SelectExpression)_tables.Single();

                foreach (var aliasExpression in subquery._projection.Cast<AliasExpression>())
                {
                    var expression = UpdateColumnExpression(aliasExpression.Expression, subquery);

                    _projection.Add(new AliasExpression(aliasExpression.Alias, expression));
                }

                _projectStar = false;
            }
        }

        public virtual JoinExpressionBase AddInnerJoin([NotNull] TableExpressionBase tableExpression)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));

            return AddInnerJoin(tableExpression, Enumerable.Empty<AliasExpression>());
        }

        public virtual JoinExpressionBase AddInnerJoin(
            [NotNull] TableExpressionBase tableExpression,
            [NotNull] IEnumerable<Expression> projection)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));
            Check.NotNull(projection, nameof(projection));

            tableExpression.Alias = CreateUniqueTableAlias(tableExpression.Alias);

            var innerJoinExpression = new InnerJoinExpression(tableExpression);

            _tables.Add(innerJoinExpression);
            _projection.AddRange(projection);

            return innerJoinExpression;
        }

        public virtual JoinExpressionBase AddOuterJoin([NotNull] TableExpressionBase tableExpression)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));

            tableExpression.Alias = CreateUniqueTableAlias(tableExpression.Alias);

            var outerJoinExpression = new LeftOuterJoinExpression(tableExpression);

            _tables.Add(outerJoinExpression);

            return outerJoinExpression;
        }

        private string CreateUniqueTableAlias(string currentAlias)
        {
            var uniqueAlias = currentAlias;
            var counter = 0;

            while (_tables.Any(t => string.Equals(t.Alias, uniqueAlias, StringComparison.OrdinalIgnoreCase)))
            {
                uniqueAlias = currentAlias + counter++;
            }

            return uniqueAlias;
        }

        private string CreateUniqueProjectionAlias(string currentAlias)
        {
            var uniqueAlias = currentAlias ?? "A";
            var counter = 0;

            while (_projection
                .OfType<AliasExpression>()
                .Any(p => string.Equals(p.Alias ?? p.TryGetColumnExpression()?.Name, uniqueAlias, StringComparison.OrdinalIgnoreCase)))
            {
                uniqueAlias = currentAlias + counter++;
            }

            return uniqueAlias;
        }

        public virtual void RemoveTable([NotNull] TableExpressionBase tableExpression)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));

            _tables.Remove(tableExpression);
        }

        protected override Expression Accept([NotNull] ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitSelect(this)
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            return this;
        }

        public override string ToString()
        {
            return new DefaultQuerySqlGenerator(this, null).GenerateSql(new Dictionary<string, object>());
        }

        public virtual void UpdateOrderByColumnBinding([NotNull] IEnumerable<Ordering> orderBy, [NotNull] JoinExpressionBase innerJoinExpression)
        {
            foreach (var ordering in orderBy)
            {
                var aliasExpression = ordering.Expression as AliasExpression;
                var columnExpression = ordering.Expression.TryGetColumnExpression();

                if (columnExpression != null)
                {
                    var matchingExpression = ((SelectExpression)innerJoinExpression.TableExpression).Projection.OfType<AliasExpression>()
                        .SingleOrDefault(ae => ae.TryGetColumnExpression().Equals(columnExpression));

                    AddToOrderBy(
                        matchingExpression?.Alias ?? aliasExpression.Alias ?? columnExpression.Name,
                        columnExpression.Property,
                        innerJoinExpression,
                        ordering.OrderingDirection);
                }
                else
                {
                    AddToOrderBy(UpdateColumnExpression(ordering, innerJoinExpression));
                }
            }
        }

        private static Ordering UpdateColumnExpression(Ordering ordering, TableExpressionBase tableExpression)
        {
            var newExpression = UpdateColumnExpression(ordering.Expression, tableExpression);
            var newOrdering = new Ordering(newExpression, ordering.OrderingDirection);

            return newOrdering;
        }

        private static Expression UpdateColumnExpression(Expression expression, TableExpressionBase tableExpression)
        {
            var aliasExpression = expression as AliasExpression;
            var columnExpression = expression as ColumnExpression;

            if (columnExpression != null)
            {
                return new ColumnExpression(columnExpression.Name, columnExpression.Property, tableExpression);
            }
            if (aliasExpression != null)
            {
                return new AliasExpression(aliasExpression.Alias, UpdateColumnExpression(aliasExpression.Expression, tableExpression));
            }
            switch (expression.NodeType)
            {
                case ExpressionType.Coalesce:
                {
                    var binaryExpression = expression as BinaryExpression;
                    var left = UpdateColumnExpression(binaryExpression.Left, tableExpression);
                    var right = UpdateColumnExpression(binaryExpression.Right, tableExpression);
                    return binaryExpression.Update(left, binaryExpression.Conversion, right);
                }
                case ExpressionType.Conditional:
                {
                    var conditionalExpression = expression as ConditionalExpression;
                    var test = UpdateColumnExpression(conditionalExpression.Test, tableExpression);
                    var ifTrue = UpdateColumnExpression(conditionalExpression.IfTrue, tableExpression);
                    var ifFalse = UpdateColumnExpression(conditionalExpression.IfFalse, tableExpression);
                    return conditionalExpression.Update(test, ifTrue, ifFalse);
                }
            }
            return expression;
        }
    }
}
