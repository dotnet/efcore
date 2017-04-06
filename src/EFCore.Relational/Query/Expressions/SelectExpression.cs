// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
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
        private const string SubqueryAliasPrefix = "t";
        private const string ColumnAliasPrefix = "c";
#if DEBUG
        internal string DebugView => ToString();
#endif

        private static readonly ExpressionEqualityComparer _expressionEqualityComparer = new ExpressionEqualityComparer();
        private readonly RelationalQueryCompilationContext _queryCompilationContext;
        private readonly IRelationalAnnotationProvider _relationalAnnotationProvider;
        private readonly List<Expression> _projection = new List<Expression>();
        private readonly List<TableExpressionBase> _tables = new List<TableExpressionBase>();
        private readonly List<Ordering> _orderBy = new List<Ordering>();
        private readonly Dictionary<MemberInfo, Expression> _memberInfoProjectionMapping = new Dictionary<MemberInfo, Expression>();
        private readonly List<Expression> _starProjection = new List<Expression>();

        private Expression _limit;
        private Expression _offset;
        private TableExpressionBase _projectStarTable;

        private bool _isDistinct;
        private bool _isProjectStar;

        /// <summary>
        ///     Creates a new instance of SelectExpression.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        /// <param name="queryCompilationContext"> Context for the query compilation. </param>
        public SelectExpression(
            [NotNull] SelectExpressionDependencies dependencies,
            [NotNull] RelationalQueryCompilationContext queryCompilationContext)
            : base(null, null)
        {
            Check.NotNull(dependencies, nameof(dependencies));
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));

            Dependencies = dependencies;
            _relationalAnnotationProvider = dependencies.RelationalAnnotationProvider;
            _queryCompilationContext = queryCompilationContext;
        }

        /// <summary>
        ///     Creates a new instance of SelectExpression.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        /// <param name="queryCompilationContext"> Context for the query compilation. </param>
        /// <param name="alias"> The alias. </param>
        public SelectExpression(
            [NotNull] SelectExpressionDependencies dependencies,
            [NotNull] RelationalQueryCompilationContext queryCompilationContext,
            [NotNull] string alias)
            : this(dependencies, queryCompilationContext)
        {
            Check.NotNull(alias, nameof(alias));

            // When assigning alias to select expression make it unique
            // ReSharper disable once VirtualMemberCallInConstructor
            Alias = queryCompilationContext.CreateUniqueTableAlias(alias);
        }

        /// <summary>
        ///     Dependencies used to create a <see cref="SelectExpression" />
        /// </summary>
        protected virtual SelectExpressionDependencies Dependencies { get; }

        /// <summary>
        ///     Gets or sets the predicate corresponding to the WHERE part of the SELECT expression.
        /// </summary>
        /// <value>
        ///     The predicate.
        /// </value>
        public virtual Expression Predicate { get; [param: CanBeNull] set; }

        /// <summary>
        ///     Gets or sets the table to be used for star projection.
        /// </summary>
        /// <value>
        ///     The table.
        /// </value>
        public virtual TableExpressionBase ProjectStarTable
        {
            get { return _projectStarTable ?? (_tables.Count == 1 ? _tables.Single() : null); }
            [param: CanBeNull]
            set { _projectStarTable = value; }
        }

        /// <summary>
        ///     Type of this expression.
        /// </summary>
        public override Type Type => _projection.Count == 1
            ? _projection[0].Type
            : base.Type;

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
        public virtual bool IsProjectStar
        {
            get => _isProjectStar;
            set
            {
                _isProjectStar = value;

                if (value)
                {
                    _starProjection.AddRange(_projection);
                }
                else
                {
                    _starProjection.Clear();
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this SelectExpression is DISTINCT.
        /// </summary>
        /// <value>
        ///     true if this SelectExpression is distinct, false if not.
        /// </value>
        public virtual bool IsDistinct
        {
            get => _isDistinct;
            set
            {
                if (_offset != null)
                {
                    PushDownSubquery();
                }

                if (value && _orderBy.Any(o => !_projection.Contains(o.Expression, _expressionEqualityComparer)))
                {
                    ClearOrderBy();
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
            get => _limit;
            [param: CanBeNull]
            set
            {
                if (value != null && _limit != null)
                {
                    PushDownSubquery();
                    LiftOrderBy();
                }

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
            get => _offset;
            [param: CanBeNull]
            set
            {
                if (_limit != null
                    && value != null)
                {
                    PushDownSubquery();
                    LiftOrderBy();
                }

                _offset = value;
            }
        }

        /// <summary>
        ///     The projection of this SelectExpression.
        /// </summary>
        public virtual IReadOnlyList<Expression> Projection => _projection;

        /// <summary>
        ///     The SQL ORDER BY of this SelectExpression.
        /// </summary>
        public virtual IReadOnlyList<Ordering> OrderBy => _orderBy;

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
                = new SelectExpression(Dependencies, _queryCompilationContext)
                {
                    _limit = _limit,
                    _offset = _offset,
                    _isDistinct = _isDistinct,
                    Predicate = Predicate,
                    ProjectStarTable = ProjectStarTable,
                    IsProjectStar = IsProjectStar
                };

            if (alias != null)
            {
                selectExpression.Alias = _queryCompilationContext.CreateUniqueTableAlias(alias);
            }

            selectExpression._tables.AddRange(_tables);
            selectExpression._projection.AddRange(_projection);
            selectExpression._orderBy.AddRange(_orderBy);

            return selectExpression;
        }

        /// <summary>
        ///     Clears all elements of this SelectExpression.
        /// </summary>
        public virtual void Clear()
        {
            _tables.Clear();
            _projection.Clear();
            _starProjection.Clear();
            _orderBy.Clear();
            _limit = null;
            _offset = null;
            _isDistinct = false;
            Predicate = null;
            ProjectStarTable = null;
            IsProjectStar = false;
        }

        /// <summary>
        ///     Determines whether this SelectExpression is an identity query. An identity query
        ///     has a single table, and returns all of the rows from that table, unmodified.
        /// </summary>
        /// <returns>
        ///     true if this SelectExpression is an identity query, false if not.
        /// </returns>
        public virtual bool IsIdentityQuery()
            => !IsProjectStar
               && !IsDistinct
               && Predicate == null
               && Limit == null
               && Offset == null
               && Projection.Count == 0
               && OrderBy.Count == 0
               && Tables.Count == 1;

        /// <summary>
        ///     Determines whether or not this SelectExpression handles the given query source.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        /// <returns>
        ///     true if the supplied query source is handled by this SelectExpression; otherwise false.
        /// </returns>
        public override bool HandlesQuerySource(IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            var processedQuerySource = PreProcessQuerySource(querySource);

            return _tables.Any(te => te.QuerySource == processedQuerySource || te.HandlesQuerySource(processedQuerySource))
                || base.HandlesQuerySource(querySource);
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

            return _tables.FirstOrDefault(te => te.QuerySource == querySource || te.HandlesQuerySource(querySource))
                ?? ProjectStarTable;
        }

        /// <summary>
        ///     Creates a subquery based on this SelectExpression and makes that table the single entry in
        ///     <see cref="Tables" />. Clears all other top-level aspects of this SelectExpression.
        /// </summary>
        /// <returns>
        ///     A SelectExpression.
        /// </returns>
        public virtual SelectExpression PushDownSubquery()
        {
            var subquery = new SelectExpression(Dependencies, _queryCompilationContext, SubqueryAliasPrefix);

            foreach (var expression in _projection)
            {
                var expressionToAdd = expression;

                switch (expressionToAdd)
                {
                    case AliasExpression aliasExpression:
                    {
                        expressionToAdd = new AliasExpression(
                            subquery.CreateUniqueProjectionAlias(aliasExpression.Alias, useColumnAliasPrefix: true),
                            aliasExpression.Expression);

                        break;
                    }
                    case ColumnExpression columnExpression:
                    {
                        var uniqueAlias = subquery.CreateUniqueProjectionAlias(columnExpression.Name, useColumnAliasPrefix: true);

                        if (!string.Equals(columnExpression.Name, uniqueAlias, StringComparison.OrdinalIgnoreCase))
                        {
                            expressionToAdd = new AliasExpression(uniqueAlias, columnExpression);
                        }

                        break;
                    }

                    case ColumnReferenceExpression columnReferenceExpression:
                    {
                        var uniqueAlias = subquery.CreateUniqueProjectionAlias(columnReferenceExpression.Name, useColumnAliasPrefix: true);

                        if (!string.Equals(columnReferenceExpression.Name, uniqueAlias, StringComparison.OrdinalIgnoreCase))
                        {
                            expressionToAdd = new AliasExpression(uniqueAlias, columnReferenceExpression);
                        }

                        break;
                    }
                    default:
                    {
                        expressionToAdd = new AliasExpression(subquery.CreateUniqueProjectionAlias(ColumnAliasPrefix), expression);
                        break;
                    }
                }

                var memberInfo = _memberInfoProjectionMapping.FirstOrDefault(kvp => _expressionEqualityComparer.Equals(kvp.Value, expression)).Key;

                if (memberInfo != null)
                {
                    _memberInfoProjectionMapping[memberInfo] = expressionToAdd.LiftExpressionFromSubquery(subquery);
                }

                subquery._projection.Add(expressionToAdd);
            }

            subquery._tables.AddRange(_tables);
            subquery._orderBy.AddRange(_orderBy);

            subquery.Predicate = Predicate;

            subquery._limit = _limit;
            subquery._offset = _offset;
            subquery._isDistinct = _isDistinct;
            subquery.ProjectStarTable = ProjectStarTable;
            subquery.IsProjectStar = IsProjectStar || !subquery._projection.Any();

            Clear();

            _tables.Add(subquery);
            ProjectStarTable = subquery;

            IsProjectStar = true;

            if (subquery.Limit == null
                && subquery.Offset == null)
            {
                subquery.ClearOrderBy();
            }

            return subquery;
        }

        /// <summary>
        ///     Ensure that order by expressions from Project Star table of this select expression
        ///     are copied on outer level to preserve ordering.
        /// </summary>
        public virtual void LiftOrderBy()
        {
            if (_projectStarTable is SelectExpression subquery)
            {
                if (subquery._orderBy.Count == 0)
                {
                    subquery.LiftOrderBy();
                }

                foreach (var ordering in subquery.OrderBy.ToList())
                {
                    var expression = ordering.Expression;

                    if (expression is NullableExpression nullableExpression)
                    {
                        expression = nullableExpression.Operand;
                    }

                    var expressionToAdd
                        = expression.LiftExpressionFromSubquery(subquery)
                          ?? subquery.Projection[subquery.AddToProjection(expression, resetProjectStar: false)].LiftExpressionFromSubquery(subquery);

                    _orderBy.Add(new Ordering(expressionToAdd, ordering.OrderingDirection));
                }

                if (subquery.Limit == null
                    && subquery.Offset == null)
                {
                    subquery.ClearOrderBy();
                }
            }
        }

        /// <summary>
        ///     Adds a table to this SelectExpression.
        /// </summary>
        /// <param name="tableExpression"> The table expression. </param>
        public virtual void AddTable([NotNull] TableExpressionBase tableExpression)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));

            _tables.Add(tableExpression);
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
        ///     Removes any tables added to this SelectExpression.
        /// </summary>
        public virtual void ClearTables() => _tables.Clear();

        /// <summary>
        ///     Generates an expression bound to this select expression for the supplied property.
        /// </summary>
        /// <param name="property"> The corresponding EF property. </param>
        /// <param name="querySource"> The originating query source. </param>
        /// <returns>
        ///     The bound expression which can be used to refer column from this select expression.
        /// </returns>
        public virtual Expression BindPropertyToSelectExpression(
            [NotNull] IProperty property,
            [NotNull] IQuerySource querySource)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(querySource, nameof(querySource));

            var table = GetTableForQuerySource(querySource);

            if (table is JoinExpressionBase joinTable)
            {
                table = joinTable.TableExpression;
            }

            var projectedExpressionToSearch = table is SelectExpression subquerySelectExpression
                ? (Expression)subquerySelectExpression.BindPropertyToSelectExpression(property, querySource)
                    .LiftExpressionFromSubquery(table)
                : new ColumnExpression(_relationalAnnotationProvider.For(property).ColumnName, property, table);

            return IsProjectStar
                ? GetOrAddToStarProjection(projectedExpressionToSearch)
                : (_projection.Find(e => _expressionEqualityComparer.Equals(e, projectedExpressionToSearch)) ?? projectedExpressionToSearch);
        }

        private Expression GetOrAddToStarProjection([NotNull] Expression expression)
        {
            var projectedExpression = _starProjection.Find(e => _expressionEqualityComparer.Equals(e, expression));

            if (projectedExpression != null)
            {
                return projectedExpression;
            }

            _starProjection.Add(expression);

            return expression;
        }

        /// <summary>
        ///     Adds a column to the projection.
        /// </summary>
        /// <param name="property"> The corresponding EF property. </param>
        /// <param name="querySource"> The originating query source. </param>
        /// <returns>
        ///     The corresponding index of the added expression in <see cref="Projection" />.
        /// </returns>
        public virtual int AddToProjection(
            [NotNull] IProperty property,
            [NotNull] IQuerySource querySource)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(querySource, nameof(querySource));

            return AddToProjection(
                BindPropertyToSelectExpression(property, querySource));
        }

        /// <summary>
        ///     Adds an expression to the projection.
        /// </summary>
        /// <param name="expression"> The expression. </param>
        /// <param name="resetProjectStar"> true to reset the value of <see cref="IsProjectStar" />. </param>
        /// <returns>
        ///     The corresponding index of the added expression in <see cref="Projection" />.
        /// </returns>
        public virtual int AddToProjection([NotNull] Expression expression, bool resetProjectStar = true)
        {
            Check.NotNull(expression, nameof(expression));

            if (expression.NodeType == ExpressionType.Convert)
            {
                var unaryExpression = (UnaryExpression)expression;

                if (unaryExpression.Type.UnwrapNullableType()
                    == unaryExpression.Operand.Type)
                {
                    expression = unaryExpression.Operand;
                }
            }

            var projectionIndex
                = _projection.FindIndex(e => _expressionEqualityComparer.Equals(e, expression));

            if (projectionIndex != -1)
            {
                return projectionIndex;
            }

            if (!(expression is ColumnExpression || expression is ColumnReferenceExpression))
            {
                var indexInOrderBy = _orderBy.FindIndex(o => _expressionEqualityComparer.Equals(o.Expression, expression));

                if (indexInOrderBy != -1)
                {
                    expression = new AliasExpression(CreateUniqueProjectionAlias(ColumnAliasPrefix), expression);
                    var updatedOrdering = new Ordering(expression, _orderBy[indexInOrderBy].OrderingDirection);

                    _orderBy.RemoveAt(indexInOrderBy);
                    _orderBy.Insert(indexInOrderBy, updatedOrdering);
                }
            }

            // Alias != null means SelectExpression in subquery which needs projections to have unique aliases
            if (Alias != null)
            {
                switch (expression)
                {
                    case ColumnExpression columnExpression:
                    {
                        var currentAlias = columnExpression.Name;
                        var uniqueAlias = CreateUniqueProjectionAlias(currentAlias);

                        expression
                            = !string.Equals(currentAlias, uniqueAlias, StringComparison.OrdinalIgnoreCase)
                                ? (Expression)new AliasExpression(uniqueAlias, columnExpression)
                                : columnExpression;

                        break;
                    }
                    case AliasExpression aliasExpression:
                    {
                        var currentAlias = aliasExpression.Alias;
                        var uniqueAlias = CreateUniqueProjectionAlias(currentAlias);

                        if (!string.Equals(currentAlias, uniqueAlias, StringComparison.OrdinalIgnoreCase))
                        {
                            expression = new AliasExpression(uniqueAlias, aliasExpression.Expression);
                        }

                        break;
                    }
                    case ColumnReferenceExpression columnReferenceExpression:
                    {
                        var currentAlias = columnReferenceExpression.Name;
                        var uniqueAlias = CreateUniqueProjectionAlias(currentAlias);

                        expression
                            = !string.Equals(currentAlias, uniqueAlias, StringComparison.OrdinalIgnoreCase)
                                ? (Expression)new AliasExpression(uniqueAlias, columnReferenceExpression)
                                : columnReferenceExpression;

                        break;
                    }
                }
            }

            _projection.Add(expression);

            if (resetProjectStar)
            {
                IsProjectStar = false;
            }

            return _projection.Count - 1;
        }

        /// <summary>
        ///     Gets the types of the expressions in <see cref="Projection" />.
        /// </summary>
        /// <returns>
        ///     The types of the expressions in <see cref="Projection" />.
        /// </returns>
        public virtual IEnumerable<Type> GetProjectionTypes()
        {
            if (_projection.Any()
                || !IsProjectStar)
            {
                return _projection.Select(e =>
                    e.NodeType == ExpressionType.Convert
                    && e.Type == typeof(object)
                        ? ((UnaryExpression)e).Operand.Type
                        : e.Type);
            }

            return _tables.OfType<SelectExpression>().SelectMany(e => e.GetProjectionTypes());
        }

        /// <summary>
        ///     Sets an expression as the single projected expression in this SelectExpression.
        /// </summary>
        /// <param name="expression"> The expression. </param>
        public virtual void SetProjectionExpression([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (_limit != null || _isDistinct)
            {
                PushDownSubquery();
            }

            ClearProjection();
            AddToProjection(expression);
        }

        /// <summary>
        ///     Clears the projection.
        /// </summary>
        public virtual void ClearProjection() => _projection.Clear();

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
        ///     Computes the index in <see cref="Projection" /> corresponding to the supplied property and query source.
        /// </summary>
        /// <param name="property"> The corresponding EF property. </param>
        /// <param name="querySource"> The originating query source. </param>
        /// <returns>
        ///     The projection index.
        /// </returns>
        public virtual int GetProjectionIndex(
            [NotNull] IProperty property,
            [NotNull] IQuerySource querySource)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(querySource, nameof(querySource));

            var projectedExpressionToSearch = BindPropertyToSelectExpression(property, querySource);

            return _projection
                .FindIndex(e => _expressionEqualityComparer.Equals(e, projectedExpressionToSearch));
        }

        /// <summary>
        ///     Transforms the projection of this SelectExpression by expanding the wildcard ('*') projection
        ///     into individual explicit projection expressions.
        /// </summary>
        public virtual void ExplodeStarProjection()
        {
            if (IsProjectStar)
            {
                var subquery = (SelectExpression)_tables.Single();

                foreach (var projectedExpression in subquery._projection)
                {
                    _projection.Add(projectedExpression.LiftExpressionFromSubquery(subquery));
                }

                IsProjectStar = false;
            }
        }

        private string CreateUniqueProjectionAlias(string currentAlias, bool useColumnAliasPrefix = false)
        {
            var uniqueAlias = currentAlias ?? ColumnAliasPrefix;

            if (useColumnAliasPrefix)
            {
                currentAlias = ColumnAliasPrefix;
            }

            var counter = 0;

            while (_projection.Select(e => e is ColumnExpression ce ? ce.Name : (e is ColumnReferenceExpression cre ? cre.Name : (e is AliasExpression ae ? ae.Alias : null)))
                .Any(p => string.Equals(p, uniqueAlias, StringComparison.OrdinalIgnoreCase)))
            {
                uniqueAlias = currentAlias + counter++;
            }

            return uniqueAlias;
        }

        /// <summary>
        ///     Gets the projection corresponding to supplied member info.
        /// </summary>
        /// <param name="memberInfo"> The corresponding member info. </param>
        /// <returns>
        ///     The projection.
        /// </returns>
        public virtual Expression GetProjectionForMemberInfo([NotNull] MemberInfo memberInfo)
        {
            Check.NotNull(memberInfo, nameof(memberInfo));

            return _memberInfoProjectionMapping.ContainsKey(memberInfo)
                ? _memberInfoProjectionMapping[memberInfo]
                : null;
        }

        /// <summary>
        ///     Sets the supplied expression as the projection for the supplied member info.
        /// </summary>
        /// <param name="memberInfo"> The member info. </param>
        /// <param name="projection"> The corresponding projection. </param>
        public virtual void SetProjectionForMemberInfo([NotNull] MemberInfo memberInfo, [NotNull] Expression projection)
        {
            Check.NotNull(memberInfo, nameof(memberInfo));
            Check.NotNull(projection, nameof(projection));

            _memberInfoProjectionMapping[memberInfo] = projection;
        }

        /// <summary>
        ///     Adds a predicate expression to this SelectExpression, combining it with
        ///     any existing predicate if necessary.
        /// </summary>
        /// <param name="predicate"> The predicate expression to add. </param>
        public virtual void AddToPredicate([NotNull] Expression predicate)
        {
            Check.NotNull(predicate, nameof(predicate));

            Predicate = Predicate != null ? AndAlso(Predicate, predicate) : predicate;
        }

        /// <summary>
        ///     Adds a single <see cref="Ordering" /> to the order by.
        /// </summary>
        /// <param name="ordering"> The ordering. </param>
        /// <returns>
        ///     The ordering added to select expression.
        /// </returns>
        public virtual Ordering AddToOrderBy([NotNull] Ordering ordering)
        {
            Check.NotNull(ordering, nameof(ordering));

            var existingOrdering = _orderBy.Find(o => _expressionEqualityComparer.Equals(o.Expression, ordering.Expression) && o.OrderingDirection == ordering.OrderingDirection);

            if (existingOrdering != null)
            {
                return existingOrdering;
            }

            _orderBy.Add(ordering);

            return ordering;
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
        ///     Clears the ORDER BY of this SelectExpression.
        /// </summary>
        public virtual void ClearOrderBy() => _orderBy.Clear();

        /// <summary>
        ///     Adds a SQL CROSS JOIN to this SelectExpression.
        /// </summary>
        /// <param name="tableExpression"> The target table expression. </param>
        /// <param name="projection"> A sequence of expressions that should be added to the projection. </param>
        public virtual JoinExpressionBase AddCrossJoin(
            [NotNull] TableExpressionBase tableExpression,
            [NotNull] IEnumerable<Expression> projection)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));
            Check.NotNull(projection, nameof(projection));

            var crossJoinExpression = new CrossJoinExpression(tableExpression);

            _tables.Add(crossJoinExpression);
            _projection.AddRange(projection);

            return crossJoinExpression;
        }

        /// <summary>
        ///     Adds a SQL CROSS JOIN LATERAL to this SelectExpression.
        /// </summary>
        /// <param name="tableExpression"> The target table expression. </param>
        /// <param name="projection"> A sequence of expressions that should be added to the projection. </param>
        public virtual JoinExpressionBase AddCrossJoinLateral(
            [NotNull] TableExpressionBase tableExpression,
            [NotNull] IEnumerable<Expression> projection)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));
            Check.NotNull(projection, nameof(projection));

            var crossJoinLateralExpression = new CrossJoinLateralExpression(tableExpression);

            _tables.Add(crossJoinLateralExpression);
            _projection.AddRange(projection);

            return crossJoinLateralExpression;
        }

        /// <summary>
        ///     Adds a SQL INNER JOIN to this SelectExpression.
        /// </summary>
        /// <param name="tableExpression"> The target table expression. </param>
        public virtual PredicateJoinExpressionBase AddInnerJoin([NotNull] TableExpressionBase tableExpression)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));

            return AddInnerJoin(tableExpression, Enumerable.Empty<AliasExpression>(), innerPredicate: null);
        }

        /// <summary>
        ///     Adds a SQL INNER JOIN to this SelectExpression.
        /// </summary>
        /// <param name="tableExpression"> The target table expression. </param>
        /// <param name="projection"> A sequence of expressions that should be added to the projection. </param>
        /// <param name="innerPredicate">A predicate which should be appended to current predicate. </param>
        public virtual PredicateJoinExpressionBase AddInnerJoin(
            [NotNull] TableExpressionBase tableExpression,
            [NotNull] IEnumerable<Expression> projection,
            [CanBeNull] Expression innerPredicate)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));
            Check.NotNull(projection, nameof(projection));

            var innerJoinExpression = new InnerJoinExpression(tableExpression);

            _tables.Add(innerJoinExpression);
            _projection.AddRange(projection);

            if (innerPredicate != null)
            {
                AddToPredicate(innerPredicate);
            }

            return innerJoinExpression;
        }

        /// <summary>
        ///     Adds a SQL LEFT OUTER JOIN to this SelectExpression.
        /// </summary>
        /// <param name="tableExpression"> The target table expression. </param>
        public virtual PredicateJoinExpressionBase AddLeftOuterJoin([NotNull] TableExpressionBase tableExpression)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));

            return AddLeftOuterJoin(tableExpression, Enumerable.Empty<AliasExpression>());
        }

        /// <summary>
        ///     Adds a SQL LEFT OUTER JOIN to this SelectExpression.
        /// </summary>
        /// <param name="tableExpression"> The target table expression. </param>
        /// <param name="projection"> A sequence of expressions that should be added to the projection. </param>
        public virtual PredicateJoinExpressionBase AddLeftOuterJoin(
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
            => Dependencies.QuerySqlGeneratorFactory.CreateDefault(this);

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
            => Dependencies.QuerySqlGeneratorFactory
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

        #region Temporary Functions To Support Include
        // TODO: Remove whole region when IncludeExpressionVisitor is removed

        /// <summary>
        ///     This method is available temporily to support current include pipeline. It will be removed when new include pipeline is fully working.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="property"></param>
        /// <param name="table"></param>
        /// <param name="querySource"></param>
        /// <returns></returns>
        public virtual int AddToProjection(
            [NotNull] string column,
            [NotNull] IProperty property,
            [NotNull] TableExpressionBase table,
            [NotNull] IQuerySource querySource)
        {
            Check.NotEmpty(column, nameof(column));
            Check.NotNull(property, nameof(property));
            Check.NotNull(table, nameof(table));
            Check.NotNull(querySource, nameof(querySource));

            return AddToProjection(
                BindPropertyToSelectExpression(property, table, querySource));
        }

        /// <summary>
        ///     This method is available temporily to support current include pipeline. It will be removed when new include pipeline is fully working.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="table"></param>
        /// <param name="querySource"></param>
        /// <returns></returns>
        public virtual Expression BindPropertyToSelectExpression(
            [NotNull] IProperty property,
            [NotNull] TableExpressionBase table,
            [NotNull] IQuerySource querySource)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(querySource, nameof(querySource));

            if (table is JoinExpressionBase joinTable)
            {
                table = joinTable.TableExpression;
            }

            var projectedExpressionToSearch = table is SelectExpression subquerySelectExpression
                ? (Expression)subquerySelectExpression.BindPropertyToSelectExpression(property, querySource)
                    .LiftExpressionFromSubquery(table)
                : new ColumnExpression(_relationalAnnotationProvider.For(property).ColumnName, property, table);

            return IsProjectStar
                ? GetOrAddToStarProjection(projectedExpressionToSearch)
                : (_projection.Find(e => _expressionEqualityComparer.Equals(e, projectedExpressionToSearch)) ?? projectedExpressionToSearch);
        }

        /// <summary>
        ///     This method is available temporily to support current include pipeline. It will be removed when new include pipeline is fully working.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="table"></param>
        /// <param name="querySource"></param>
        /// <param name="orderingDirection"></param>
        /// <returns></returns>
        public virtual Ordering AddToOrderBy(
            [NotNull] IProperty property,
            [NotNull] TableExpressionBase table,
            [NotNull] IQuerySource querySource,
            OrderingDirection orderingDirection)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(table, nameof(table));

            var orderingExpression = BindPropertyToSelectExpression(property, table, querySource);

            return AddToOrderBy(new Ordering(orderingExpression, orderingDirection));
        }

        #endregion
    }
}
