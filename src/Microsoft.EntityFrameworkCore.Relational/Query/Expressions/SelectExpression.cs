// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     Represents a SQL SELECT expression.
    /// </summary>
    public class SelectExpression : TableExpressionBase
    {
        private const string SystemAliasPrefix = "t";
#if DEBUG
        internal string DebugView => ToString();
#endif

        private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
        private readonly RelationalQueryCompilationContext _queryCompilationContext;
        private readonly List<Expression> _projection = new List<Expression>();
        private readonly List<TableExpressionBase> _tables = new List<TableExpressionBase>();
        private readonly List<Ordering> _orderBy = new List<Ordering>();

        private Expression _limit;
        private Expression _offset;

        private int _subqueryDepth = -1;

        private bool _isDistinct;

        /// <summary>
        ///     Creates a new instance of SelectExpression.
        /// </summary>
        /// <param name="querySqlGeneratorFactory"> The query SQL generator factory. </param>
        /// <param name="queryCompilationContext"> Context for the query compilation. </param>
        public SelectExpression([NotNull] IQuerySqlGeneratorFactory querySqlGeneratorFactory,
            [NotNull] RelationalQueryCompilationContext queryCompilationContext)
            : base(null, null)
        {
            Check.NotNull(querySqlGeneratorFactory, nameof(querySqlGeneratorFactory));
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));

            _querySqlGeneratorFactory = querySqlGeneratorFactory;
            _queryCompilationContext = queryCompilationContext;
        }

        /// <summary>
        ///     Creates a new instance of SelectExpression.
        /// </summary>
        /// <param name="querySqlGeneratorFactory"> The query SQL generator factory. </param>
        /// <param name="queryCompilationContext"> Context for the query compilation. </param>
        /// <param name="alias"> The alias. </param>
        public SelectExpression(
            [NotNull] IQuerySqlGeneratorFactory querySqlGeneratorFactory,
            [NotNull] RelationalQueryCompilationContext queryCompilationContext,
            [NotNull] string alias)
            : this(querySqlGeneratorFactory, queryCompilationContext)
        {
            Check.NotNull(alias, nameof(alias));

            // When assigning alias to select expression make it unique
            Alias = queryCompilationContext.CreateUniqueTableAlias(alias);
        }

        /// <summary>
        ///     Gets or sets the predicate corresponding to the WHERE part of the SELECT expression.
        /// </summary>
        /// <value>
        ///     The predicate.
        /// </value>
        public virtual Expression Predicate { get; [param: CanBeNull] set; }

        /// <summary>
        ///     Type of this expression.
        /// </summary>
        public override Type Type => _projection.Count == 1
            ? _projection[0].Type
            : base.Type;

        /// <summary>
        ///     Makes a copy of this SelectExpression.
        /// </summary>
        /// <param name="alias"> The alias. </param>
        /// <returns>
        ///     A copy of this SelectExpression.
        /// </returns>
        public virtual SelectExpression Clone([CanBeNull] string alias = null)
        {
            var selectExpression
                = new SelectExpression(_querySqlGeneratorFactory, _queryCompilationContext)
                {
                    _limit = _limit,
                    _offset = _offset,
                    _isDistinct = _isDistinct,
                    _subqueryDepth = _subqueryDepth,
                    IsProjectStar = IsProjectStar,
                    Predicate = Predicate
                };

            if (alias != null)
            {
                selectExpression.Alias = _queryCompilationContext.CreateUniqueTableAlias(alias);
            }

            selectExpression._projection.AddRange(_projection);

            selectExpression.AddTables(_tables);
            selectExpression.AddToOrderBy(_orderBy);

            return selectExpression;
        }

        /// <summary>
        ///     The tables making up the FROM part of the SELECT expression.
        /// </summary>
        public virtual IReadOnlyList<TableExpressionBase> Tables => _tables;

        /// <summary>
        ///     Gets or sets a value indicating whether this expression projects a single wildcard ('*').
        /// </summary>
        /// <value>
        ///     true if this SelectExpression is project star, false if not.
        /// </value>
        public virtual bool IsProjectStar { get; set; }

        /// <summary>
        ///     Adds a table to this SelectExpression.
        /// </summary>
        /// <param name="tableExpression"> The table expression. </param>
        /// <param name="createUniqueAlias"> true to create unique alias. </param>
        public virtual void AddTable([NotNull] TableExpressionBase tableExpression, bool createUniqueAlias = true)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));

            if (createUniqueAlias)
            {
                tableExpression.Alias = _queryCompilationContext.CreateUniqueTableAlias(tableExpression.Alias);
            }
            _tables.Add(tableExpression);
        }

        /// <summary>
        ///     Adds tables to this SelectExprssion.
        /// </summary>
        /// <param name="tableExpressions"> The table expressions. </param>
        public virtual void AddTables([NotNull] IEnumerable<TableExpressionBase> tableExpressions)
        {
            Check.NotNull(tableExpressions, nameof(tableExpressions));

            // Multiple tables are added while moving current select expression inside subquery hence it does not need to generate unique alias
            foreach (var tableExpression in tableExpressions.ToList())
            {
                AddTable(tableExpression, createUniqueAlias: false);
            }
        }

        /// <summary>
        ///     Removes any tables added to this SelectExpression.
        /// </summary>
        public virtual void ClearTables() => _tables.Clear();

        /// <summary>
        ///     Determines if this SelectExpression contains any correlated subqueries.
        /// </summary>
        /// <returns>
        ///     true if correlated, false if not.
        /// </returns>
        public virtual bool IsCorrelated() => new CorrelationFindingExpressionVisitor().IsCorrelated(this);

        private class CorrelationFindingExpressionVisitor : ExpressionVisitor
        {
            private SelectExpression _selectExpression;
            private bool _correlated;

            public bool IsCorrelated(SelectExpression selectExpression)
            {
                _selectExpression = selectExpression;

                Visit(_selectExpression);

                return _correlated;
            }

            public override Expression Visit(Expression expression)
            {
                if (!_correlated)
                {
                    var columnExpression = expression as ColumnExpression;

                    if (columnExpression?.Table.QuerySource != null
                        && !_selectExpression.HandlesQuerySource(columnExpression.Table.QuerySource))
                    {
                        _correlated = true;
                    }
                    else
                    {
                        return base.Visit(expression);
                    }
                }

                return expression;
            }
        }

        /// <summary>
        ///     Determines whether or not this SelectExpression handles the given query source.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        /// <returns>
        ///     true if the supplied query source is handled by this SelectExpression; otherwise false.
        /// </returns>
        public virtual bool HandlesQuerySource([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            return _tables.Any(te
                => te.QuerySource == querySource
                   || ((te as SelectExpression)?.HandlesQuerySource(querySource) ?? false));
        }

        /// <summary>
        ///     Gets the table corresponding to the supplied query source.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        /// <returns>
        ///     The table for query source.
        /// </returns>
        public virtual TableExpressionBase GetTableForQuerySource([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            return _tables.FirstOrDefault(te
                => te.QuerySource == querySource
                   || ((te as SelectExpression)?.HandlesQuerySource(querySource) ?? false))
                   ?? _tables.Last();
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this SelectExpression is DISTINCT.
        /// </summary>
        /// <value>
        ///     true if this SelectExpression is distinct, false if not.
        /// </value>
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

        /// <summary>
        ///     Gets or sets the LIMIT of this SelectExpression.
        /// </summary>
        /// <value>
        ///     The limit.
        /// </value>
        public virtual Expression Limit
        {
            get { return _limit; }
            [param: CanBeNull]
            set
            {
                Check.NotNull(value, nameof(value));

                PushDownIfLimit();

                _limit = value;
            }
        }

        /// <summary>
        ///     Gets or sets the OFFSET of this SelectExpression.
        /// </summary>
        /// <value>
        ///     The offset.
        /// </value>
        public virtual Expression Offset
        {
            get { return _offset; }
            [param: CanBeNull]
            set
            {
                if (_limit != null
                    && value != null)
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

        /// <summary>
        ///     Creates a subquery based on this SelectExpression and makes that table the single entry in
        ///     <see cref="Tables"/>. Clears all other top-level aspects of this SelectExpression.
        /// </summary>
        /// <returns>
        ///     A SelectExpression.
        /// </returns>
        public virtual SelectExpression PushDownSubquery()
        {
            _subqueryDepth++;

            var subquery = new SelectExpression(_querySqlGeneratorFactory, _queryCompilationContext, SystemAliasPrefix);

            var columnAliasCounter = 0;

            foreach (var expression in _projection)
            {
                var aliasExpression = expression as AliasExpression;

                if (aliasExpression != null)
                {
                    var columnExpression = aliasExpression.TryGetColumnExpression();

                    if (columnExpression != null
                        && subquery._projection.OfType<AliasExpression>()
                            .Any(ae => (ae.Alias ?? ae.TryGetColumnExpression()?.Name) == (aliasExpression.Alias ?? columnExpression.Name)))
                    {
                        aliasExpression.Alias = "c" + columnAliasCounter++;
                    }
                }
                else
                {
                    aliasExpression = new AliasExpression("c" + columnAliasCounter++, expression);
                }

                subquery._projection.Add(aliasExpression);
            }

            subquery.AddTables(_tables);
            subquery.AddToOrderBy(_orderBy);

            subquery.Predicate = Predicate;

            subquery._limit = _limit;
            subquery._offset = _offset;
            subquery._isDistinct = _isDistinct;
            subquery._subqueryDepth = _subqueryDepth;
            subquery.IsProjectStar = IsProjectStar || !subquery._projection.Any();

            _limit = null;
            _offset = null;
            _isDistinct = false;

            Predicate = null;

            ClearTables();
            ClearProjection();
            ClearOrderBy();

            AddTable(subquery, createUniqueAlias: false);

            return subquery;
        }

        /// <summary>
        ///     The projection of this SelectExpression.
        /// </summary>
        public virtual IReadOnlyList<Expression> Projection => _projection;

        /// <summary>
        ///     Adds a column to the projection.
        /// </summary>
        /// <param name="column"> The column name. </param>
        /// <param name="property"> The corresponding EF property. </param>
        /// <param name="querySource"> The originating query source. </param>
        /// <returns>
        ///     The corresponding index of the added expression in <see cref="Projection"/>.
        /// </returns>
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

        /// <summary>
        ///     Adds an expression to the projection.
        /// </summary>
        /// <param name="expression"> The expression. </param>
        /// <returns>
        ///     The corresponding index of the added expression in <see cref="Projection"/>.
        /// </returns>
        public virtual int AddToProjection([NotNull] Expression expression)
            => AddToProjection(expression, true);

        /// <summary>
        ///     Adds an expression to the projection.
        /// </summary>
        /// <param name="expression"> The expression. </param>
        /// <param name="resetProjectStar"> true to reset the value of <see cref="IsProjectStar"/>. </param>
        /// <returns>
        ///     The corresponding index of the added expression in <see cref="Projection"/>.
        /// </returns>
        public virtual int AddToProjection([NotNull] Expression expression, bool resetProjectStar)
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

            if (resetProjectStar)
            {
                IsProjectStar = false;
            }

            return _projection.Count - 1;
        }

        /// <summary>
        ///     Adds an <see cref="AliasExpression"/> to the projection.
        /// </summary>
        /// <param name="aliasExpression"> The alias expression. </param>
        /// <returns>
        ///     The corresponding index of the added expression in <see cref="Projection"/>.
        /// </returns>
        public virtual int AddToProjection([NotNull] AliasExpression aliasExpression)
            => AddAliasToProjection(aliasExpression.Alias, aliasExpression.Expression);

        /// <summary>
        ///     Adds an expression with an alias to the projection.
        /// </summary>
        /// <param name="alias"> The alias. </param>
        /// <param name="expression"> The expression. </param>
        /// <returns>
        ///     The corresponding index of the added expression in <see cref="Projection"/>.
        /// </returns>
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

                            return ce != null && ce.Property == columnExpression?.Property
                                   && ce.TableAlias == columnExpression?.TableAlias
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
                    foreach (var orderByAliasExpression
                        in _orderBy.Select(o => o.Expression).OfType<AliasExpression>())
                    {
                        if (orderByAliasExpression.TryGetColumnExpression() == null)
                        {
                            // TODO: This seems bad
                            if (orderByAliasExpression.Expression.ToString() == expression.ToString())
                            {
                                orderByAliasExpression.Alias = alias;
                                orderByAliasExpression.IsProjected = true;
                            }
                        }
                    }
                }

                _projection.Add(new AliasExpression(alias, expression));

                IsProjectStar = false;
            }

            return projectionIndex;
        }

        /// <summary>
        ///     Adds a ColumnExpression to the projection.
        /// </summary>
        /// <param name="columnExpression"> The column expression. </param>
        /// <returns>
        ///     The corresponding index of the added expression in <see cref="Projection"/>.
        /// </returns>
        public virtual int AddToProjection([NotNull] ColumnExpression columnExpression)
        {
            Check.NotNull(columnExpression, nameof(columnExpression));

            var projectionIndex
                = _projection
                    .FindIndex(e =>
                        {
                            var ce = e.TryGetColumnExpression();

                            return ce?.Property == columnExpression.Property
                                   && ce?.Type == columnExpression.Type
                                   && ce.TableAlias == columnExpression.TableAlias;
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

                IsProjectStar = false;
            }

            return projectionIndex;
        }

        /// <summary>
        ///     Gets the types of the expressions in <see cref="Projection"/>.
        /// </summary>
        /// <returns>
        ///     The types of the expressions in <see cref="Projection"/>.
        /// </returns>
        public virtual IEnumerable<Type> GetProjectionTypes()
        {
            if (_projection.Any()
                || !IsProjectStar)
            {
                return _projection.Select(e => e.Type);
            }

            return _tables.OfType<SelectExpression>().SelectMany(e => e.GetProjectionTypes());
        }

        /// <summary>
        ///     Sets a <see cref="ConditionalExpression"/> as the single projected expression
        ///     in this SelectExpression.
        /// </summary>
        /// <param name="conditionalExpression"> The conditional expression. </param>
        public virtual void SetProjectionConditionalExpression([NotNull] ConditionalExpression conditionalExpression)
        {
            Check.NotNull(conditionalExpression, nameof(conditionalExpression));

            ClearProjection();
            AddToProjection(conditionalExpression);
        }

        /// <summary>
        ///     Sets an expression as the single projected expression in this SelectExpression.
        /// </summary>
        /// <param name="expression"> The expression. </param>
        public virtual void SetProjectionExpression([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            PushDownIfLimit();
            PushDownIfDistinct();

            ClearProjection();
            AddToProjection(expression);
        }

        /// <summary>
        ///     Clears the projection.
        /// </summary>
        public virtual void ClearProjection()
        {
            _projection.Clear();
            IsProjectStar = true;
        }

        /// <summary>
        ///     Clears the column expressions from the projection.
        /// </summary>
        public virtual void ClearColumnProjections()
        {
            for (var i = _projection.Count - 1; i >= 0; i--)
            {
                var aliasExpression = _projection[i] as AliasExpression;
                if (aliasExpression?.Expression is ColumnExpression)
                {
                    _projection.RemoveAt(i);
                }
            }

            if (_projection.Count == 0)
            {
                IsProjectStar = true;
            }
        }

        /// <summary>
        ///     Removes a range from the projection.
        /// </summary>
        /// <param name="index"> Zero-based index of the start of the range to remove. </param>
        public virtual void RemoveRangeFromProjection(int index)
        {
            if (index < _projection.Count)
            {
                _projection.RemoveRange(index, _projection.Count - index);
            }
        }

        /// <summary>
        ///     Removes expressions from the projection corresponding to the
        ///     supplied <see cref="Ordering"/> expressions.
        /// </summary>
        /// <param name="orderBy"> The Orderings to remove from the projection. </param>
        public virtual void RemoveFromProjection([NotNull] IEnumerable<Ordering> orderBy)
        {
            Check.NotNull(orderBy, nameof(orderBy));

            _projection.RemoveAll(ce => orderBy.Any(o => ReferenceEquals(o.Expression, ce)));
        }

        /// <summary>
        ///     Computes the index in <see cref="Projection"/> corresponding to the supplied property and query source.
        /// </summary>
        /// <param name="property"> The corresponding EF property. </param>
        /// <param name="querySource"> The originating query source. </param>
        /// <returns>
        ///     The projection index.
        /// </returns>
        public virtual int GetProjectionIndex(
            [NotNull] IProperty property, [NotNull] IQuerySource querySource)
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

        /// <summary>
        ///     Adds a column to the ORDER BY of this SelectExpression.
        /// </summary>
        /// <param name="column"> The column name. </param>
        /// <param name="property"> The corresponding EF property. </param>
        /// <param name="table"> The target table. </param>
        /// <param name="orderingDirection"> The ordering direction. </param>
        /// <returns>
        ///     An AliasExpression corresponding to the expression added to the ORDER BY.
        /// </returns>
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

        /// <summary>
        ///     Adds multiple expressions to the ORDER BY of this SelectExpression.
        /// </summary>
        /// <param name="orderings"> The orderings expressions. </param>
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

        /// <summary>
        ///     Adds a single <see cref="Ordering"/> to the order by.
        /// </summary>
        /// <param name="ordering"> The ordering. </param>
        public virtual void AddToOrderBy([NotNull] Ordering ordering)
        {
            Check.NotNull(ordering, nameof(ordering));

            if (_orderBy.FindIndex(o => o.Expression.Equals(ordering.Expression)) == -1)
            {
                _orderBy.Add(ordering);
            }
        }

        /// <summary>
        ///     Prepends multiple ordering expressions to the ORDER BY of this SelectExpression.
        /// </summary>
        /// <param name="orderings"> The orderings expressions. </param>
        public virtual void PrependToOrderBy([NotNull] IEnumerable<Ordering> orderings)
        {
            Check.NotNull(orderings, nameof(orderings));

            var oldOrderBy = _orderBy.ToList();

            _orderBy.Clear();
            _orderBy.AddRange(orderings);

            foreach (var ordering in oldOrderBy)
            {
                AddToOrderBy(ordering);
            }
        }

        /// <summary>
        ///     The SQL ORDER BY of this SelectExpression.
        /// </summary>
        public virtual IReadOnlyList<Ordering> OrderBy => _orderBy;

        /// <summary>
        ///     Clears the ORDER BY of this SelectExpression.
        /// </summary>
        public virtual void ClearOrderBy() => _orderBy.Clear();

        /// <summary>
        ///     Transforms the projection of this SelectExpression by expanding the wildcard ('*') projection
        ///     into individual explicit projection expressions.
        /// </summary>
        public virtual void ExplodeStarProjection()
        {
            if (IsProjectStar)
            {
                var subquery = (SelectExpression)_tables.Single();

                foreach (var aliasExpression in subquery._projection.Cast<AliasExpression>())
                {
                    var expression = UpdateColumnExpression(aliasExpression.Expression, subquery);

                    _projection.Add(
                        new AliasExpression(aliasExpression.Alias, expression)
                        {
                            SourceMember = aliasExpression.SourceMember
                        });
                }

                IsProjectStar = false;
            }
        }

        /// <summary>
        ///     Adds a SQL CROSS JOIN to this SelectExpression.
        /// </summary>
        /// <param name="tableExpression"> The target table expression. </param>
        /// <param name="projection"> A sequence of expressions that should be added to the projection. </param>
        public virtual void AddCrossJoin(
            [NotNull] TableExpressionBase tableExpression,
            [NotNull] IEnumerable<Expression> projection)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));
            Check.NotNull(projection, nameof(projection));

            _tables.Add(new CrossJoinExpression(tableExpression));
            _projection.AddRange(projection);
        }

        /// <summary>
        ///     Adds a SQL LATERAL JOIN to this SelectExpression.
        /// </summary>
        /// <param name="tableExpression"> The target table expression. </param>
        /// <param name="projection"> A sequence of expressions that should be added to the projection. </param>
        public virtual void AddLateralJoin(
            [NotNull] TableExpressionBase tableExpression,
            [NotNull] IEnumerable<Expression> projection)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));
            Check.NotNull(projection, nameof(projection));

            _tables.Add(new LateralJoinExpression(tableExpression));
            _projection.AddRange(projection);
        }

        /// <summary>
        ///     Adds a SQL INNER JOIN to this SelectExpression.
        /// </summary>
        /// <param name="tableExpression"> The target table expression. </param>
        public virtual JoinExpressionBase AddInnerJoin([NotNull] TableExpressionBase tableExpression)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));

            return AddInnerJoin(tableExpression, Enumerable.Empty<AliasExpression>());
        }

        /// <summary>
        ///     Adds a SQL INNER JOIN to this SelectExpression.
        /// </summary>
        /// <param name="tableExpression"> The target table expression. </param>
        /// <param name="projection"> A sequence of expressions that should be added to the projection. </param>
        public virtual JoinExpressionBase AddInnerJoin(
            [NotNull] TableExpressionBase tableExpression,
            [NotNull] IEnumerable<Expression> projection)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));
            Check.NotNull(projection, nameof(projection));

            var innerJoinExpression = new InnerJoinExpression(tableExpression);

            _tables.Add(innerJoinExpression);
            _projection.AddRange(projection);

            return innerJoinExpression;
        }

        /// <summary>
        ///     Adds a SQL LEFT OUTER JOIN to this SelectExpression.
        /// </summary>
        /// <param name="tableExpression"> The target table expression. </param>
        public virtual JoinExpressionBase AddLeftOuterJoin([NotNull] TableExpressionBase tableExpression)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));

            return AddLeftOuterJoin(tableExpression, Enumerable.Empty<AliasExpression>());
        }

        /// <summary>
        ///     Adds a SQL LEFT OUTER JOIN to this SelectExpression.
        /// </summary>
        /// <param name="tableExpression"> The target table expression. </param>
        /// <param name="projection"> A sequence of expressions that should be added to the projection. </param>
        public virtual JoinExpressionBase AddLeftOuterJoin(
            [NotNull] TableExpressionBase tableExpression,
            [NotNull] IEnumerable<Expression> projection)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));
            Check.NotNull(projection, nameof(projection));

            var outerJoinExpression = new LeftOuterJoinExpression(tableExpression);

            _tables.Add(outerJoinExpression);
            _projection.AddRange(projection);

            return outerJoinExpression;
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

        /// <summary>
        ///     Removes a table from this SelectExpression.
        /// </summary>
        /// <param name="tableExpression"> The table expression. </param>
        public virtual void RemoveTable([NotNull] TableExpressionBase tableExpression)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));

            _tables.Remove(tableExpression);
        }

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitSelect(this)
                : base.Accept(visitor);
        }

        /// <summary>
        ///     Reduces the node and then calls the <see cref="ExpressionVisitor.Visit(System.Linq.Expressions.Expression)" /> method passing the
        ///     reduced expression.
        ///     Throws an exception if the node isn't reducible.
        /// </summary>
        /// <param name="visitor"> An instance of <see cref="ExpressionVisitor" />. </param>
        /// <returns> The expression being visited, or an expression which should replace it in the tree. </returns>
        /// <remarks>
        ///     Override this method to provide logic to walk the node's children.
        ///     A typical implementation will call visitor.Visit on each of its
        ///     children, and if any of them change, should return a new copy of
        ///     itself with the modified children.
        /// </remarks>
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            foreach (var expression in Projection)
            {
                visitor.Visit(expression);
            }

            foreach (var tableExpressionBase in Tables)
            {
                visitor.Visit(tableExpressionBase);
            }

            visitor.Visit(Predicate);

            foreach (var ordering in OrderBy)
            {
                visitor.Visit(ordering.Expression);
            }

            return this;
        }

        /// <summary>
        ///     Creates the default query SQL generator.
        /// </summary>
        /// <returns>
        ///     The new default query SQL generator.
        /// </returns>
        public virtual IQuerySqlGenerator CreateDefaultQuerySqlGenerator()
            => _querySqlGeneratorFactory.CreateDefault(this);

        /// <summary>
        ///     Creates the FromSql query SQL generator.
        /// </summary>
        /// <param name="sql"> The SQL. </param>
        /// <param name="arguments"> The arguments. </param>
        /// <returns>
        ///     The new FromSql query SQL generator.
        /// </returns>
        public virtual IQuerySqlGenerator CreateFromSqlQuerySqlGenerator(
            [NotNull] string sql,
            [NotNull] Expression arguments)
            => _querySqlGeneratorFactory
                .CreateFromSql(
                    this,
                    Check.NotEmpty(sql, nameof(sql)),
                    Check.NotNull(arguments, nameof(arguments)));

        /// <summary>
        ///     Convert this object into a string representation.
        /// </summary>
        /// <returns>
        ///     A string that represents this object.
        /// </returns>
        public override string ToString()
            => CreateDefaultQuerySqlGenerator()
                .GenerateSql(new Dictionary<string, object>())
                .CommandText;

        // TODO: Make polymorphic

        /// <summary>
        ///     Updates the table expression of any column expressions in the target expression.
        /// </summary>
        /// <param name="expression"> The target expression. </param>
        /// <param name="tableExpression"> The new table expression. </param>
        /// <returns>
        ///     An updated expression.
        /// </returns>
        public virtual Expression UpdateColumnExpression(
            [NotNull] Expression expression,
            [NotNull] TableExpressionBase tableExpression)
        {
            Check.NotNull(expression, nameof(expression));
            Check.NotNull(tableExpression, nameof(tableExpression));

            var columnExpression = expression as ColumnExpression;

            if (columnExpression != null)
            {
                return columnExpression.Property == null
                    ? new ColumnExpression(columnExpression.Name, columnExpression.Type, tableExpression)
                    : new ColumnExpression(columnExpression.Name, columnExpression.Property, tableExpression);
            }

            var aliasExpression = expression as AliasExpression;

            if (aliasExpression != null)
            {
                var selectExpression = aliasExpression.Expression as SelectExpression;
                if (selectExpression != null)
                {
                    return new ColumnExpression(aliasExpression.Alias, selectExpression.Type, tableExpression);
                }

                return new AliasExpression(
                    aliasExpression.Alias,
                    UpdateColumnExpression(aliasExpression.Expression, tableExpression))
                {
                    SourceMember = aliasExpression.SourceMember
                };
            }

            switch (expression.NodeType)
            {
                case ExpressionType.Coalesce:
                {
                    var binaryExpression = (BinaryExpression)expression;
                    var left = UpdateColumnExpression(binaryExpression.Left, tableExpression);
                    var right = UpdateColumnExpression(binaryExpression.Right, tableExpression);
                    return binaryExpression.Update(left, binaryExpression.Conversion, right);
                }
                case ExpressionType.Conditional:
                {
                    var conditionalExpression = (ConditionalExpression)expression;
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
