// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     The default relational <see cref="QueryModel" /> visitor.
    /// </summary>
    public class RelationalQueryModelVisitor : EntityQueryModelVisitor
    {
        /// <summary>
        ///     The SelectExpressions for this query, mapped by query source.
        /// </summary>
        /// <value>
        ///     A map of query source to select expression.
        /// </value>
        protected virtual Dictionary<IQuerySource, SelectExpression> QueriesBySource { get; } =
            new Dictionary<IQuerySource, SelectExpression>();

        private readonly Dictionary<IQuerySource, RelationalQueryModelVisitor> _subQueryModelVisitorsBySource
            = new Dictionary<IQuerySource, RelationalQueryModelVisitor>();

        private readonly ISqlTranslatingExpressionVisitorFactory _sqlTranslatingExpressionVisitorFactory;
        private readonly ICompositePredicateExpressionVisitorFactory _compositePredicateExpressionVisitorFactory;
        private readonly IConditionalRemovingExpressionVisitorFactory _conditionalRemovingExpressionVisitorFactory;

        private bool _requiresClientSelectMany;
        private bool _requiresClientJoin;
        private bool _requiresClientFilter;
        private bool _requiresClientProjection;
        private bool _requiresClientOrderBy;
        private bool _requiresClientResultOperator;
        private ResultOperatorBase _groupResultOperatorInQueryModel;

        private readonly List<GroupJoinClause> _unflattenedGroupJoinClauses = new List<GroupJoinClause>();
        private readonly List<AdditionalFromClause> _flattenedAdditionalFromClauses = new List<AdditionalFromClause>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalQueryModelVisitor(
            [NotNull] EntityQueryModelVisitorDependencies dependencies,
            [NotNull] RelationalQueryModelVisitorDependencies relationalDependencies,
            // ReSharper disable once SuggestBaseTypeForParameter
            [NotNull] RelationalQueryCompilationContext queryCompilationContext,
            [CanBeNull] RelationalQueryModelVisitor parentQueryModelVisitor)
            : base(
                dependencies.With(Check.NotNull(relationalDependencies, nameof(relationalDependencies)).RelationalResultOperatorHandler),
                queryCompilationContext)
        {
            _sqlTranslatingExpressionVisitorFactory = relationalDependencies.SqlTranslatingExpressionVisitorFactory;
            _compositePredicateExpressionVisitorFactory = relationalDependencies.CompositePredicateExpressionVisitorFactory;
            _conditionalRemovingExpressionVisitorFactory = relationalDependencies.ConditionalRemovingExpressionVisitorFactory;

            ContextOptions = relationalDependencies.ContextOptions;
            ParentQueryModelVisitor = parentQueryModelVisitor;
        }

        /// <summary>
        ///     Gets the options for the target context.
        /// </summary>
        /// <value>
        ///     Options for the target context.
        /// </value>
        protected virtual IDbContextOptions ContextOptions { get; }

        /// <summary>
        ///     Determine whether a defining query should be applied when querying the target entity type.
        /// </summary>
        /// <param name="entityType">The target entity type.</param>
        /// <param name="querySource">The target query source.</param>
        /// <returns>true if the target type should have a defining query applied.</returns>
        public override bool ShouldApplyDefiningQuery(IEntityType entityType, IQuerySource querySource)
        {
            return !(entityType.FindAnnotation(RelationalAnnotationNames.TableName) is ConventionalAnnotation tableNameAnnotation)
                    || tableNameAnnotation?.GetConfigurationSource() == ConfigurationSource.Convention
                   && QueryCompilationContext.QueryAnnotations
                       .OfType<FromSqlResultOperator>()
                       .All(a => a.QuerySource != querySource);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the query requires client eval.
        /// </summary>
        /// <value>
        ///     true if the query requires client eval, false if not.
        /// </value>
        public virtual bool RequiresClientEval { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the query requires client select many.
        /// </summary>
        /// <value>
        ///     true if the query requires client select many, false if not.
        /// </value>
        public virtual bool RequiresClientSelectMany
        {
            get => _requiresClientSelectMany || RequiresClientEval;
            set => _requiresClientSelectMany = value;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the query requires client join.
        /// </summary>
        /// <value>
        ///     true if the query requires client join, false if not.
        /// </value>
        public virtual bool RequiresClientJoin
        {
            get => _requiresClientJoin || RequiresClientEval;
            set => _requiresClientJoin = value;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the query requires client filter.
        /// </summary>
        /// <value>
        ///     true if the query requires client filter, false if not.
        /// </value>
        public virtual bool RequiresClientFilter
        {
            get => _requiresClientFilter || RequiresClientEval;
            set => _requiresClientFilter = value;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the query requires client order by.
        /// </summary>
        /// <value>
        ///     true if the query requires client order by, false if not.
        /// </value>
        public virtual bool RequiresClientOrderBy
        {
            get => _requiresClientOrderBy || RequiresClientEval;
            set => _requiresClientOrderBy = value;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the query requires client projection.
        /// </summary>
        /// <value>
        ///     true if the query requires client projection, false if not.
        /// </value>
        public virtual bool RequiresClientProjection
        {
            get => _requiresClientProjection || RequiresClientEval;
            set => _requiresClientProjection = value;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the query requires client result operator.
        /// </summary>
        /// <value>
        ///     true if the query requires client result operator, false if not.
        /// </value>
        public virtual bool RequiresClientResultOperator
        {
            get => _unflattenedGroupJoinClauses.Any() || _requiresClientResultOperator || RequiresClientEval;
            set => _requiresClientResultOperator = value;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the query requires streaming group result operator.
        /// </summary>
        /// <value>
        ///     true if the query requires streaming result operator, false if not.
        /// </value>
        public virtual bool RequiresStreamingGroupResultOperator { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this query model visitor will be
        ///     able to bind directly to properties from its parent query without requiring
        ///     parameter injection.
        /// </summary>
        /// <value>
        ///     true if the query model visitor can bind to its parent's properties, false if not.
        /// </value>
        public virtual bool CanBindToParentQueryModel { get; protected set; }

        /// <summary>
        ///     Gets a value indicating whether query model visitor's resulting expression
        ///     can be lifted into the parent query. Liftable queries contain a single SelectExpression.
        /// </summary>
        public virtual bool IsLiftable
            => Queries.Count == 1
               && !RequiresClientEval
               && !RequiresClientSelectMany
               && !RequiresClientJoin
               && !RequiresClientFilter
               && !RequiresClientProjection
               && !RequiresClientOrderBy
               && !RequiresClientResultOperator
               && !RequiresStreamingGroupResultOperator;

        /// <summary>
        ///     Context for the query compilation.
        /// </summary>
        public new virtual RelationalQueryCompilationContext QueryCompilationContext
            => (RelationalQueryCompilationContext)base.QueryCompilationContext;

        /// <summary>
        ///     The SelectExpressions active in the current query compilation.
        /// </summary>
        public virtual ICollection<SelectExpression> Queries => QueriesBySource.Values;

        /// <summary>
        ///     Gets the parent query model visitor, or null if there is no parent.
        /// </summary>
        /// <value>
        ///     The parent query model visitor, or null if there is no parent.
        /// </value>
        public virtual RelationalQueryModelVisitor ParentQueryModelVisitor { get; }

        /// <summary>
        ///     Registers a sub query visitor.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        /// <param name="queryModelVisitor"> The query model visitor. </param>
        public virtual void RegisterSubQueryVisitor(
            [NotNull] IQuerySource querySource,
            [NotNull] RelationalQueryModelVisitor queryModelVisitor)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));

            _subQueryModelVisitorsBySource[querySource] = queryModelVisitor;
        }

        /// <summary>
        ///     Adds a SelectExpression to this query.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        /// <param name="selectExpression"> The select expression. </param>
        public virtual void AddQuery([NotNull] IQuerySource querySource, [NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(selectExpression, nameof(selectExpression));

            QueriesBySource.Add(querySource, selectExpression);
        }

        /// <summary>
        ///     Try and get the active SelectExpression for a given query source.
        /// </summary>
        /// <param name="querySource"> The query source. </param>
        /// <returns>
        ///     A SelectExpression, or null.
        /// </returns>
        public virtual SelectExpression TryGetQuery([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            return QueriesBySource.TryGetValue(querySource, out var selectExpression)
                ? selectExpression
                : QueriesBySource.Values.LastOrDefault(se => se.HandlesQuerySource(querySource));
        }

        /// <summary>
        ///     Visit a query model.
        /// </summary>
        /// <param name="queryModel"> The query model. </param>
        public override void VisitQueryModel(QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            base.VisitQueryModel(queryModel);

            if (RequiresStreamingGroupResultOperator)
            {
                WarnClientEval(queryModel, _groupResultOperatorInQueryModel);
            }

            var joinEliminator = new JoinEliminator();
            var compositePredicateVisitor = _compositePredicateExpressionVisitorFactory.Create();

            foreach (var selectExpression in QueriesBySource.Values)
            {
                joinEliminator.EliminateJoins(selectExpression);
                compositePredicateVisitor.Visit(selectExpression);
            }
        }

        private class JoinEliminator : ExpressionVisitor
        {
            private readonly List<TableExpression> _tables = new List<TableExpression>();
            private readonly List<ColumnExpression> _columns = new List<ColumnExpression>();

            private bool _canEliminate;

            public void EliminateJoins(SelectExpression selectExpression)
            {
                for (var i = selectExpression.Tables.Count - 1; i >= 0; i--)
                {
                    var tableExpression = selectExpression.Tables[i];

                    if (tableExpression is LeftOuterJoinExpression joinExpressionBase)
                    {
                        _tables.Clear();
                        _columns.Clear();
                        _canEliminate = true;

                        Visit(joinExpressionBase.Predicate);

                        if (_canEliminate
                            && _columns.Count > 0
                            && _columns.Count % 2 == 0
                            && _tables.Count == 2
                            && _tables[0].Table == _tables[1].Table
                            && _tables[0].Schema == _tables[1].Schema)
                        {
                            for (var j = 0; j < _columns.Count - 1; j += 2)
                            {
                                if (_columns[j].Name != _columns[j + 1].Name)
                                {
                                    _canEliminate = false;

                                    break;
                                }
                            }

                            if (_canEliminate)
                            {
                                var newTableExpression
                                    = _tables.Single(t => !ReferenceEquals(t, joinExpressionBase.TableExpression));

                                selectExpression.RemoveTable(joinExpressionBase);

                                if (ReferenceEquals(selectExpression.ProjectStarTable, joinExpressionBase))
                                {
                                    selectExpression.ProjectStarTable = selectExpression.GetTableForQuerySource(newTableExpression.QuerySource);
                                }

                                var sqlTableReferenceReplacingVisitor
                                    = new SqlTableReferenceReplacingVisitor(
                                        joinExpressionBase.TableExpression,
                                        newTableExpression);

                                var newProjection
                                    = selectExpression.Projection
                                        .Select(expression => sqlTableReferenceReplacingVisitor.Visit(expression))
                                        .ToList();

                                selectExpression.ReplaceProjection(newProjection);

                                foreach (var tableExpressionBase in selectExpression.Tables)
                                {
                                    switch (tableExpressionBase)
                                    {
                                        case PredicateJoinExpressionBase predicateJoinExpressionBase:
                                        {
                                            predicateJoinExpressionBase.Predicate
                                                = sqlTableReferenceReplacingVisitor.Visit(predicateJoinExpressionBase.Predicate);

                                            break;
                                        }

                                        // TODO: Visit sub-query (SelectExpression) here?
                                    }
                                }

                                selectExpression.Predicate
                                    = sqlTableReferenceReplacingVisitor.Visit(selectExpression.Predicate);

                                var newOrderBy
                                    = selectExpression.OrderBy.Select(
                                            ordering => new Ordering(
                                                sqlTableReferenceReplacingVisitor.Visit(ordering.Expression), ordering.OrderingDirection))
                                        .ToList();

                                selectExpression.ReplaceOrderBy(newOrderBy);
                            }
                        }
                    }
                }
            }

            private class SqlTableReferenceReplacingVisitor : ExpressionVisitor
            {
                private readonly TableExpressionBase _oldTableExpression;
                private readonly TableExpressionBase _newTableExpression;

                public SqlTableReferenceReplacingVisitor(
                    TableExpressionBase oldTableExpression, TableExpressionBase newTableExpression)
                {
                    _oldTableExpression = oldTableExpression;
                    _newTableExpression = newTableExpression;
                }

                public override Expression Visit(Expression expression)
                {
                    switch (expression)
                    {
                        case ColumnExpression columnExpression
                        when ReferenceEquals(columnExpression.Table, _oldTableExpression):
                        {
                            return new ColumnExpression(
                                columnExpression.Name, columnExpression.Property, _newTableExpression);
                        }
                    }

                    return base.Visit(expression);
                }
            }

            public override Expression Visit(Expression expression)
            {
                switch (expression)
                {
                    case ColumnExpression columnExpression:
                    {
                        if (columnExpression.Table is TableExpression tableExpression)
                        {
                            _tables.Add(tableExpression);
                            _columns.Add(columnExpression);
                        }

                        return expression;
                    }
                    case BinaryExpression binaryExpression:
                    {
                        return base.Visit(binaryExpression);
                    }
                    case NullableExpression nullableExpression:
                    {
                        return base.Visit(nullableExpression);
                    }
                    case UnaryExpression unaryExpression
                    when unaryExpression.NodeType == ExpressionType.Convert:
                    {
                        return base.Visit(unaryExpression);
                    }
                }

                _canEliminate = false;

                return expression;
            }
        }

        /// <summary>
        ///     Visit a sub-query model.
        /// </summary>
        /// <param name="queryModel"> The sub-query model. </param>
        public virtual void VisitSubQueryModel([NotNull] QueryModel queryModel)
        {
            CanBindToParentQueryModel = true;

            VisitQueryModel(queryModel);
        }

        /// <summary>
        ///     Compile main from clause expression.
        /// </summary>
        /// <param name="mainFromClause"> The main from clause. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression CompileMainFromClauseExpression(
            MainFromClause mainFromClause,
            QueryModel queryModel)
        {
            Check.NotNull(mainFromClause, nameof(mainFromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            Expression expression = null;
            if (mainFromClause.FromExpression is SubQueryExpression subQueryExpression)
            {
                expression = LiftSubQuery(mainFromClause, subQueryExpression);
            }

            expression = expression ?? base.CompileMainFromClauseExpression(mainFromClause, queryModel);

            return expression;
        }

        /// <summary>
        ///     Visit an additional from clause.
        /// </summary>
        /// <param name="fromClause"> The from clause being visited. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <param name="index"> Index of the node being visited. </param>
        public override void VisitAdditionalFromClause(
            AdditionalFromClause fromClause,
            QueryModel queryModel,
            int index)
        {
            Check.NotNull(fromClause, nameof(fromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            if (_flattenedAdditionalFromClauses.Contains(fromClause))
            {
                return;
            }

            var previousQuerySource = FindPreviousQuerySource(queryModel, index);
            var previousSelectExpression = TryGetQuery(previousQuerySource);
            var previousProjectionCount = previousSelectExpression?.Projection.Count ?? 0;

            base.VisitAdditionalFromClause(fromClause, queryModel, index);

            if (!TryFlattenSelectMany(fromClause, queryModel, index, previousProjectionCount))
            {
                RequiresClientSelectMany = true;
            }

            if (RequiresClientSelectMany)
            {
                WarnClientEval(queryModel, fromClause);
            }
        }

        /// <summary>
        ///     Compile an additional from clause expression.
        /// </summary>
        /// <param name="additionalFromClause"> The additional from clause being compiled. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression CompileAdditionalFromClauseExpression(
            AdditionalFromClause additionalFromClause,
            QueryModel queryModel)
        {
            Check.NotNull(additionalFromClause, nameof(additionalFromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            Expression expression = null;
            if (additionalFromClause.FromExpression is SubQueryExpression subQueryExpression)
            {
                expression = LiftSubQuery(additionalFromClause, subQueryExpression);
            }

            expression = expression ?? base.CompileAdditionalFromClauseExpression(additionalFromClause, queryModel);

            return expression;
        }

        /// <summary>
        ///     Visit a join clause.
        /// </summary>
        /// <param name="joinClause"> The join clause being visited. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <param name="index"> Index of the node being visited. </param>
        public override void VisitJoinClause(
            JoinClause joinClause,
            QueryModel queryModel,
            int index)
        {
            Check.NotNull(joinClause, nameof(joinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var previousQuerySource = FindPreviousQuerySource(queryModel, index);
            var previousSelectExpression = TryGetQuery(previousQuerySource);
            var previousProjectionCount = previousSelectExpression?.Projection.Count ?? 0;
            var previousParameter = CurrentParameter;
            var previousMapping = SnapshotQuerySourceMapping(queryModel);

            base.VisitJoinClause(joinClause, queryModel, index);

            if (!TryFlattenJoin(joinClause, queryModel, index, previousProjectionCount, previousParameter, previousMapping))
            {
                RequiresClientJoin = true;
            }

            if (RequiresClientJoin)
            {
                WarnClientEval(queryModel, joinClause);
            }
        }

        /// <summary>
        ///     Compile a join clause inner sequence expression.
        /// </summary>
        /// <param name="joinClause"> The join clause being compiled. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression CompileJoinClauseInnerSequenceExpression(
            JoinClause joinClause,
            QueryModel queryModel)
        {
            Check.NotNull(joinClause, nameof(joinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            Expression expression = null;
            if (joinClause.InnerSequence is SubQueryExpression subQueryExpression)
            {
                expression = LiftSubQuery(joinClause, subQueryExpression);
            }

            expression = expression ?? base.CompileJoinClauseInnerSequenceExpression(joinClause, queryModel);

            return expression;
        }

        /// <summary>
        ///     Visit a group join clause.
        /// </summary>
        /// <param name="groupJoinClause"> The group join being visited. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <param name="index"> Index of the node being visited. </param>
        public override void VisitGroupJoinClause(
            GroupJoinClause groupJoinClause,
            QueryModel queryModel,
            int index)
        {
            Check.NotNull(groupJoinClause, nameof(groupJoinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var previousQuerySource = FindPreviousQuerySource(queryModel, index);
            var previousSelectExpression = TryGetQuery(previousQuerySource);
            var previousProjectionCount = previousSelectExpression?.Projection.Count ?? 0;
            var previousParameter = CurrentParameter;
            var previousMapping = SnapshotQuerySourceMapping(queryModel);

            base.VisitGroupJoinClause(groupJoinClause, queryModel, index);

            _unflattenedGroupJoinClauses.Add(groupJoinClause);

            if (!TryFlattenGroupJoin(
                groupJoinClause,
                queryModel,
                index,
                previousProjectionCount,
                previousParameter,
                previousMapping))
            {
                RequiresClientJoin = true;
            }

            if (RequiresClientJoin)
            {
                WarnClientEval(queryModel, groupJoinClause.JoinClause);
            }
        }

        private Dictionary<IQuerySource, Expression> SnapshotQuerySourceMapping(QueryModel queryModel)
        {
            var previousMapping
                = new Dictionary<IQuerySource, Expression>
                {
                    {
                        queryModel.MainFromClause,
                        QueryCompilationContext.QuerySourceMapping
                            .GetExpression(queryModel.MainFromClause)
                    }
                };

            foreach (var querySource in queryModel.BodyClauses.OfType<IQuerySource>())
            {
                if (QueryCompilationContext.QuerySourceMapping.ContainsMapping(querySource))
                {
                    previousMapping[querySource]
                        = QueryCompilationContext.QuerySourceMapping
                            .GetExpression(querySource);

                    if (querySource is GroupJoinClause groupJoinClause
                        && QueryCompilationContext.QuerySourceMapping.ContainsMapping(groupJoinClause.JoinClause))
                    {
                        previousMapping.Add(
                            groupJoinClause.JoinClause,
                            QueryCompilationContext.QuerySourceMapping
                                .GetExpression(groupJoinClause.JoinClause));
                    }
                }
            }

            return previousMapping;
        }

        private class OuterJoinOrderingExtractor : ExpressionVisitor
        {
            private readonly List<Expression> _expressions = new List<Expression>();

            public bool DependentToPrincipalFound { get; private set; }

            public IEnumerable<Expression> Expressions => _expressions;

            private IForeignKey _matchingCandidate;
            private List<IProperty> _matchingCandidateProperties;

            protected override Expression VisitBinary(BinaryExpression binaryExpression)
            {
                if (DependentToPrincipalFound)
                {
                    return binaryExpression;
                }

                if (binaryExpression.NodeType == ExpressionType.Equal)
                {
                    var leftExpression = binaryExpression.Left.RemoveConvert();
                    var rightExpression = binaryExpression.Right.RemoveConvert();
                    var leftProperty = (((leftExpression as NullableExpression)?.Operand ?? leftExpression) as ColumnExpression)?.Property;
                    var rightProperty = (((rightExpression as NullableExpression)?.Operand ?? rightExpression) as ColumnExpression)?.Property;
                    if (leftProperty != null
                        && rightProperty != null
                        && leftProperty.IsForeignKey()
                        && rightProperty.IsKey())
                    {
                        var keyDeclaringEntityType = rightProperty.GetContainingKeys().First().DeclaringEntityType;
                        var matchingForeignKeys = leftProperty.GetContainingForeignKeys().Where(k => k.PrincipalKey.DeclaringEntityType == keyDeclaringEntityType).ToList();
                        if (matchingForeignKeys.Count == 1)
                        {
                            var matchingKey = matchingForeignKeys.Single();
                            if (rightProperty.GetContainingKeys().Contains(matchingKey.PrincipalKey))
                            {
                                var matchingForeignKey = matchingKey;
                                if (_matchingCandidate == null)
                                {
                                    _matchingCandidate = matchingForeignKey;
                                    _matchingCandidateProperties = new List<IProperty> { leftProperty };
                                }
                                else if (_matchingCandidate == matchingForeignKey)
                                {
                                    _matchingCandidateProperties.Add(leftProperty);
                                }

                                if (_matchingCandidate.Properties.All(p => _matchingCandidateProperties.Contains(p)))
                                {
                                    DependentToPrincipalFound = true;
                                    return binaryExpression;
                                }
                            }
                        }
                    }

                    _expressions.Add(leftExpression);

                    return binaryExpression;
                }

                return binaryExpression.NodeType == ExpressionType.AndAlso
                    ? base.VisitBinary(binaryExpression)
                    : binaryExpression;
            }
        }

        private static IQuerySource FindPreviousQuerySource(QueryModel queryModel, int index)
        {
            for (var i = index; i >= 0; i--)
            {
                var candidate = i == 0
                    ? queryModel.MainFromClause
                    : queryModel.BodyClauses[i - 1] as IQuerySource;

                if (candidate != null)
                {
                    return candidate;
                }
            }

            return null;
        }

        /// <summary>
        ///     Compile a group join inner sequence expression.
        /// </summary>
        /// <param name="groupJoinClause"> The group join clause being compiled. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression CompileGroupJoinInnerSequenceExpression(
            GroupJoinClause groupJoinClause,
            QueryModel queryModel)
        {
            Check.NotNull(groupJoinClause, nameof(groupJoinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            Expression expression = null;
            if (groupJoinClause.JoinClause.InnerSequence is SubQueryExpression subQueryExpression)
            {
                expression = LiftSubQuery(groupJoinClause.JoinClause, subQueryExpression);
            }

            expression = expression ?? base.CompileGroupJoinInnerSequenceExpression(groupJoinClause, queryModel);

            return expression;
        }

        private Expression LiftSubQuery(
            IQuerySource querySource,
            SubQueryExpression subQueryExpression)
        {
            var subQueryModelVisitor
                = (RelationalQueryModelVisitor)QueryCompilationContext
                    .CreateQueryModelVisitor(this);

            var subQueryModel = subQueryExpression.QueryModel;

            var queryModelMapping = new Dictionary<QueryModel, QueryModel>();
            subQueryModel.PopulateQueryModelMapping(queryModelMapping);

            subQueryModelVisitor.VisitSubQueryModel(subQueryModel);

            if ((subQueryModelVisitor.Expression as MethodCallExpression)?.Method.MethodIsClosedFormOf(QueryCompilationContext.QueryMethodProvider.GroupByMethod) == true
                && !(querySource is AdditionalFromClause))
            {
                var subSelectExpression = subQueryModelVisitor.Queries.First();
                AddQuery(querySource, subSelectExpression);

                return subQueryModelVisitor.Expression;
            }

            if (subQueryModelVisitor.IsLiftable)
            {
                var subSelectExpression = subQueryModelVisitor.Queries.First();

                if ((!subSelectExpression.OrderBy.Any()
                     || subSelectExpression.Limit != null
                     || subSelectExpression.Offset != null)
                    && (QueryCompilationContext.IsLateralJoinSupported
                        || !subSelectExpression.IsCorrelated()
                        || !(querySource is AdditionalFromClause)))
                {
                    if (!subSelectExpression.IsIdentityQuery())
                    {
                        subSelectExpression.PushDownSubquery().QuerySource = querySource;
                    }

                    AddQuery(querySource, subSelectExpression);

                    var newExpression
                        = new QuerySourceUpdater(
                                querySource,
                                QueryCompilationContext,
                                LinqOperatorProvider,
                                subSelectExpression)
                            .Visit(subQueryModelVisitor.Expression);

                    return newExpression;
                }
            }

            subQueryModel.RecreateQueryModelFromMapping(queryModelMapping);

            return null;
        }

        private sealed class QuerySourceUpdater : ExpressionVisitorBase
        {
            private readonly IQuerySource _querySource;
            private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;
            private readonly ILinqOperatorProvider _linqOperatorProvider;
            private readonly SelectExpression _selectExpression;
            private bool _insideShapedQueryMethod;

            public QuerySourceUpdater(
                IQuerySource querySource,
                RelationalQueryCompilationContext relationalQueryCompilationContext,
                ILinqOperatorProvider linqOperatorProvider,
                SelectExpression selectExpression)
            {
                _querySource = querySource;
                _relationalQueryCompilationContext = relationalQueryCompilationContext;
                _linqOperatorProvider = linqOperatorProvider;
                _selectExpression = selectExpression;
            }

            protected override Expression VisitConstant(ConstantExpression constantExpression)
            {
                if (constantExpression.Value is Shaper shaper)
                {
                    foreach (var queryAnnotation
                        in _relationalQueryCompilationContext.QueryAnnotations
                            .Where(qa => shaper.IsShaperForQuerySource(qa.QuerySource)))
                    {
                        queryAnnotation.QuerySource = _querySource;
                    }

                    if (_insideShapedQueryMethod
                        && shaper is EntityShaper
                        && !_relationalQueryCompilationContext.QuerySourceRequiresMaterialization(_querySource))
                    {
                        return Expression.Constant(new ValueBufferShaper(_querySource));
                    }

                    shaper.UpdateQuerySource(_querySource);

                    _selectExpression.ExplodeStarProjection();
                }

                return base.VisitConstant(constantExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                _insideShapedQueryMethod = methodCallExpression.Method.MethodIsClosedFormOf(
                    _relationalQueryCompilationContext.QueryMethodProvider.ShapedQueryMethod);

                var arguments = VisitAndConvert(methodCallExpression.Arguments, "VisitMethodCall");

                if (arguments != methodCallExpression.Arguments)
                {
                    if (_insideShapedQueryMethod)
                    {
                        return Expression.Call(
                            _relationalQueryCompilationContext.QueryMethodProvider.ShapedQueryMethod
                                .MakeGenericMethod(((Shaper)((ConstantExpression)arguments[2]).Value).Type),
                            arguments);
                    }

                    if (methodCallExpression.Method.MethodIsClosedFormOf(
                            _linqOperatorProvider.Cast)
                        && arguments[0].Type.GetSequenceType() == typeof(ValueBuffer))
                    {
                        return arguments[0];
                    }
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
            {
                Check.NotNull(lambdaExpression, nameof(lambdaExpression));

                var newBodyExpression = Visit(lambdaExpression.Body);

                return newBodyExpression != lambdaExpression.Body
                    ? Expression.Lambda(newBodyExpression, lambdaExpression.Parameters)
                    : lambdaExpression;
            }
        }

        /// <summary>
        ///     Visit a where clause.
        /// </summary>
        /// <param name="whereClause"> The where clause being visited. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <param name="index"> Index of the node being visited. </param>
        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            Check.NotNull(whereClause, nameof(whereClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var selectExpression = TryGetQuery(queryModel.MainFromClause);
            var requiresClientFilter = selectExpression == null;

            if (!requiresClientFilter)
            {
                var sqlTranslatingExpressionVisitor
                    = _sqlTranslatingExpressionVisitorFactory.Create(
                        queryModelVisitor: this,
                        targetSelectExpression: selectExpression,
                        topLevelPredicate: whereClause.Predicate);

                var sqlPredicateExpression = sqlTranslatingExpressionVisitor.Visit(whereClause.Predicate);

                if (sqlPredicateExpression != null)
                {
                    sqlPredicateExpression =
                        _conditionalRemovingExpressionVisitorFactory
                            .Create()
                            .Visit(sqlPredicateExpression);

                    selectExpression.AddToPredicate(sqlPredicateExpression);
                }
                else
                {
                    requiresClientFilter = true;
                }

                if (sqlTranslatingExpressionVisitor.ClientEvalPredicate != null
                    && selectExpression.Predicate != null)
                {
                    requiresClientFilter = true;
                    whereClause = new WhereClause(sqlTranslatingExpressionVisitor.ClientEvalPredicate);
                }
            }

            RequiresClientFilter |= requiresClientFilter;

            if (RequiresClientFilter)
            {
                WarnClientEval(queryModel, whereClause);

                base.VisitWhereClause(whereClause, queryModel, index);
            }
        }

        /// <summary>
        ///     Visit an order by clause.
        /// </summary>
        /// <param name="orderByClause"> The order by clause. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <param name="index"> Index of the node being visited. </param>
        public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            Check.NotNull(orderByClause, nameof(orderByClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var selectExpression = TryGetQuery(queryModel.MainFromClause);
            var requiresClientOrderBy = selectExpression == null;

            if (!requiresClientOrderBy)
            {
                var sqlTranslatingExpressionVisitor
                    = _sqlTranslatingExpressionVisitorFactory.Create(
                        queryModelVisitor: this,
                        targetSelectExpression: selectExpression);

                var orderings = new List<Ordering>();

                foreach (var ordering in orderByClause.Orderings)
                {
                    // we disable this for order by, because you can't have a parameter (that is integer) in the order by
                    var canBindPropertyToOuterParameter = _canBindPropertyToOuterParameter;

                    _canBindPropertyToOuterParameter = false;

                    var sqlOrderingExpression
                        = sqlTranslatingExpressionVisitor
                            .Visit(ordering.Expression);

                    _canBindPropertyToOuterParameter = canBindPropertyToOuterParameter;

                    if (sqlOrderingExpression == null)
                    {
                        break;
                    }

                    orderings.Add(
                        new Ordering(
                            sqlOrderingExpression,
                            ordering.OrderingDirection));
                }

                if (orderings.Count == orderByClause.Orderings.Count)
                {
                    selectExpression.PrependToOrderBy(orderings);
                }
                else
                {
                    requiresClientOrderBy = true;
                }
            }

            RequiresClientOrderBy |= requiresClientOrderBy;

            if (RequiresClientOrderBy)
            {
                WarnClientEval(queryModel, orderByClause);

                base.VisitOrderByClause(orderByClause, queryModel, index);
            }
        }

        /// <summary>
        ///     Determines whether correlated collections (if any) can be optimized.
        /// </summary>
        /// <returns>True if optimization is allowed, false otherwise.</returns>
        protected override bool CanOptimizeCorrelatedCollections()
        {
            if (!base.CanOptimizeCorrelatedCollections())
            {
                return false;
            }

            if (RequiresClientEval
                || RequiresClientFilter
                || RequiresClientJoin
                || RequiresClientOrderBy
                || RequiresClientSelectMany)
            {
                return false;
            }

            var injectParametersFinder = new InjectParametersFindingVisitor(QueryCompilationContext.QueryMethodProvider.InjectParametersMethod);
            injectParametersFinder.Visit(Expression);

            return !injectParametersFinder.InjectParametersFound;
        }

        private class InjectParametersFindingVisitor : ExpressionVisitorBase
        {
            private MethodInfo _injectParametersMethod;

            public InjectParametersFindingVisitor(MethodInfo injectParametersMethod)
            {
                _injectParametersMethod = injectParametersMethod;
            }

            public bool InjectParametersFound { get; private set; }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.MethodIsClosedFormOf(_injectParametersMethod))
                {
                    InjectParametersFound = true;
                }

                return base.VisitMethodCall(node);
            }
        }

        /// <summary>
        ///     Visits <see cref="SelectClause" /> nodes.
        /// </summary>
        /// <param name="selectClause"> The node being visited. </param>
        /// <param name="queryModel"> The query. </param>
        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            Check.NotNull(selectClause, nameof(selectClause));
            Check.NotNull(queryModel, nameof(queryModel));

            base.VisitSelectClause(selectClause, queryModel);

            if (Expression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method.MethodIsClosedFormOf(LinqOperatorProvider.Select))
            {
                var shapedQuery = methodCallExpression.Arguments[0] as MethodCallExpression;

                if (IsShapedQueryExpression(shapedQuery))
                {
                    shapedQuery = UnwrapShapedQueryExpression(shapedQuery);

                    var oldShaper = ExtractShaper(shapedQuery, 0);

                    var matchingIncludes
                        = from i in QueryCompilationContext.QueryAnnotations.OfType<IncludeResultOperator>()
                          where oldShaper.IsShaperForQuerySource(i.QuerySource)
                          select i;

                    if (!matchingIncludes.Any())
                    {
                        var materializer = (LambdaExpression)methodCallExpression.Arguments[1];

                        if (selectClause.Selector.Type == typeof(AnonymousObject))
                        {
                            // We will end up fully translating this projection, so turn
                            // this into a no-op.

                            materializer
                                = Expression.Lambda(
                                    Expression.Default(materializer.Body.Type),
                                    materializer.Parameters);
                        }

                        var qsreFinder = new QuerySourceReferenceFindingExpressionVisitor();

                        qsreFinder.Visit(materializer.Body);

                        if (!qsreFinder.FoundAny)
                        {
                            Shaper newShaper = null;

                            if (selectClause.Selector is QuerySourceReferenceExpression querySourceReferenceExpression)
                            {
                                newShaper = oldShaper.Unwrap(querySourceReferenceExpression.ReferencedQuerySource);
                            }

                            newShaper = newShaper ?? ProjectionShaper.Create(oldShaper, materializer);

                            Expression =
                                Expression.Call(
                                    shapedQuery.Method
                                        .GetGenericMethodDefinition()
                                        .MakeGenericMethod(Expression.Type.GetSequenceType()),
                                    shapedQuery.Arguments[0],
                                    shapedQuery.Arguments[1],
                                    Expression.Constant(newShaper));
                        }
                    }
                }
            }
        }

        private class QuerySourceReferenceFindingExpressionVisitor : ExpressionVisitorBase
        {
            public bool FoundAny { get; private set; }

            protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
            {
                FoundAny = true;

                return base.VisitQuerySourceReference(expression);
            }
        }

        /// <summary>
        ///     Visit a result operator.
        /// </summary>
        /// <param name="resultOperator"> The result operator being visited. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <param name="index"> Index of the node being visited. </param>
        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            base.VisitResultOperator(resultOperator, queryModel, index);

            if (RequiresStreamingGroupResultOperator)
            {
                _groupResultOperatorInQueryModel = resultOperator;
            }

            if (RequiresClientResultOperator)
            {
                WarnClientEval(queryModel, resultOperator);
            }
        }

        /// <summary>
        ///     Applies optimizations to the query.
        /// </summary>
        /// <param name="queryModel"> The query. </param>
        /// <param name="asyncQuery"> True if we are compiling an async query; otherwise false. </param>
        protected override void OptimizeQueryModel(
            QueryModel queryModel,
            bool asyncQuery)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            base.OptimizeQueryModel(queryModel, asyncQuery);

            var typeIsExpressionTranslatingVisitor = new TypeIsExpressionTranslatingVisitor(QueryCompilationContext.Model);
            queryModel.TransformExpressions(typeIsExpressionTranslatingVisitor.Visit);
        }

        /// <summary>
        ///     Generated a client-eval warning
        /// </summary>
        /// <param name="queryModel"> The query model </param>
        /// <param name="queryModelElement"> The expression being client-eval'd. </param>
        protected virtual void WarnClientEval(
            [NotNull] QueryModel queryModel,
            [NotNull] object queryModelElement)
        {
            Check.NotNull(queryModelElement, nameof(queryModelElement));

            QueryCompilationContext.Logger.QueryClientEvaluationWarning(queryModel, queryModelElement);
        }

        private class TypeIsExpressionTranslatingVisitor : ExpressionVisitorBase
        {
            private readonly IModel _model;

            public TypeIsExpressionTranslatingVisitor(IModel model)
            {
                _model = model;
            }

            protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinaryExpression)
            {
                if (typeBinaryExpression.NodeType != ExpressionType.TypeIs)
                {
                    return base.VisitTypeBinary(typeBinaryExpression);
                }

                var entityType = _model.FindEntityType(typeBinaryExpression.TypeOperand);

                if (entityType == null)
                {
                    return base.VisitTypeBinary(typeBinaryExpression);
                }

                var concreteEntityTypes
                    = entityType.GetConcreteTypesInHierarchy().ToList();

                if (concreteEntityTypes.Count != 1
                    || concreteEntityTypes[0].RootType() != concreteEntityTypes[0])
                {
                    var discriminatorProperty
                        = concreteEntityTypes[0].Relational().DiscriminatorProperty;

                    var discriminatorPropertyExpression
                        = typeBinaryExpression.Expression.CreateEFPropertyExpression(discriminatorProperty);

                    var discriminatorPredicate
                        = concreteEntityTypes
                            .Select(
                                concreteEntityType =>
                                    Expression.Equal(
                                        discriminatorPropertyExpression,
                                        Expression.Constant(concreteEntityType.Relational().DiscriminatorValue, discriminatorPropertyExpression.Type)))
                            .Aggregate((current, next) => Expression.OrElse(next, current));

                    return discriminatorPredicate;
                }

                return Expression.Constant(true, typeof(bool));
            }
        }

        #region Flattening

        private bool IsShapedQueryExpression(Expression expression)
        {
            var methodCallExpression = expression as MethodCallExpression;

            if (methodCallExpression == null)
            {
                return false;
            }

            var linqMethods = QueryCompilationContext.LinqOperatorProvider;

            if (methodCallExpression.Method.MethodIsClosedFormOf(linqMethods.DefaultIfEmpty)
                || methodCallExpression.Method.MethodIsClosedFormOf(linqMethods.DefaultIfEmptyArg))
            {
                methodCallExpression = methodCallExpression.Arguments[0] as MethodCallExpression;

                if (methodCallExpression == null)
                {
                    return false;
                }
            }

            var queryMethods = QueryCompilationContext.QueryMethodProvider;

            if (methodCallExpression.Method.MethodIsClosedFormOf(queryMethods.ShapedQueryMethod)
                || methodCallExpression.Method.MethodIsClosedFormOf(queryMethods.DefaultIfEmptyShapedQueryMethod))
            {
                return true;
            }

            return false;
        }

        private MethodCallExpression UnwrapShapedQueryExpression(MethodCallExpression expression)
        {
            if (expression.Method.MethodIsClosedFormOf(LinqOperatorProvider.DefaultIfEmpty)
                || expression.Method.MethodIsClosedFormOf(LinqOperatorProvider.DefaultIfEmptyArg))
            {
                return (MethodCallExpression)expression.Arguments[0];
            }

            return expression;
        }

        private Shaper ExtractShaper(MethodCallExpression shapedQueryExpression, int offset)
        {
            var shaper = (Shaper)((ConstantExpression)UnwrapShapedQueryExpression(shapedQueryExpression).Arguments[2]).Value;

            return shaper.WithOffset(offset);
        }

        private bool TryFlattenSelectMany(
            IQuerySource fromClause,
            QueryModel queryModel,
            int index,
            int previousProjectionCount)
        {
            if (RequiresClientJoin || RequiresClientSelectMany)
            {
                return false;
            }

            var outerQuerySource = FindPreviousQuerySource(queryModel, index);
            var outerSelectExpression = TryGetQuery(outerQuerySource);

            if (outerSelectExpression == null)
            {
                return false;
            }

            var innerSelectExpression = TryGetQuery(fromClause);

            if (innerSelectExpression?.Tables.Count != 1)
            {
                return false;
            }

            var correlated = innerSelectExpression.IsCorrelated();

            if (innerSelectExpression.IsCorrelated()
                && !QueryCompilationContext.IsLateralJoinSupported)
            {
                return false;
            }

            var selectManyMethodCallExpression = Expression as MethodCallExpression;

            var outerShapedQuery
                = selectManyMethodCallExpression?.Arguments.FirstOrDefault() as MethodCallExpression;

            var innerShapedQuery
                = (selectManyMethodCallExpression?.Arguments.Skip(1).FirstOrDefault() as LambdaExpression)
                ?.Body as MethodCallExpression;

            if (selectManyMethodCallExpression == null
                || !selectManyMethodCallExpression.Method.MethodIsClosedFormOf(LinqOperatorProvider.SelectMany)
                || !IsShapedQueryExpression(outerShapedQuery)
                || !IsShapedQueryExpression(innerShapedQuery))
            {
                return false;
            }

            if (!QueryCompilationContext.QuerySourceRequiresMaterialization(outerQuerySource))
            {
                outerSelectExpression.RemoveRangeFromProjection(previousProjectionCount);
            }

            var joinExpression
                = correlated
                    ? outerSelectExpression.AddCrossJoinLateral(
                        innerSelectExpression.Tables.First(),
                        innerSelectExpression.Projection)
                    : outerSelectExpression.AddCrossJoin(
                        innerSelectExpression.Tables.First(),
                        innerSelectExpression.Projection);

            joinExpression.QuerySource = fromClause;

            QueriesBySource.Remove(fromClause);

            var outerShaper = ExtractShaper(outerShapedQuery, 0);
            var innerShaper = ExtractShaper(innerShapedQuery, previousProjectionCount);

            var materializerLambda = (LambdaExpression)selectManyMethodCallExpression.Arguments.Last();

            var compositeShaper
                = CompositeShaper.Create(fromClause, outerShaper, innerShaper, materializerLambda);

            compositeShaper.SaveAccessorExpression(QueryCompilationContext.QuerySourceMapping);

            innerShaper.UpdateQuerySource(fromClause);

            Expression
                = Expression.Call(
                    // ReSharper disable once PossibleNullReferenceException
                    outerShapedQuery.Method
                        .GetGenericMethodDefinition()
                        .MakeGenericMethod(materializerLambda.ReturnType),
                    outerShapedQuery.Arguments[0],
                    outerShapedQuery.Arguments[1],
                    Expression.Constant(compositeShaper));

            return true;
        }

        private bool TryFlattenJoin(
            JoinClause joinClause,
            QueryModel queryModel,
            int index,
            int previousProjectionCount,
            ParameterExpression previousParameter,
            Dictionary<IQuerySource, Expression> previousMapping)
        {
            if (RequiresClientJoin || RequiresClientSelectMany)
            {
                return false;
            }

            var joinMethodCallExpression = Expression as MethodCallExpression;

            var outerShapedQuery
                = joinMethodCallExpression?.Arguments.FirstOrDefault() as MethodCallExpression;

            var innerShapedQuery
                = joinMethodCallExpression?.Arguments.Skip(1).FirstOrDefault() as MethodCallExpression;

            if (joinMethodCallExpression == null
                || !joinMethodCallExpression.Method.MethodIsClosedFormOf(LinqOperatorProvider.Join)
                || !IsShapedQueryExpression(outerShapedQuery)
                || !IsShapedQueryExpression(innerShapedQuery))
            {
                return false;
            }

            var outerQuerySource = FindPreviousQuerySource(queryModel, index);
            var outerSelectExpression = TryGetQuery(outerQuerySource);
            var innerSelectExpression = TryGetQuery(joinClause);

            if (outerSelectExpression == null
                || innerSelectExpression == null)
            {
                return false;
            }

            var sqlTranslatingExpressionVisitor
                = _sqlTranslatingExpressionVisitorFactory.Create(this);

            var predicate
                = sqlTranslatingExpressionVisitor.Visit(
                    Expression.Equal(joinClause.OuterKeySelector, joinClause.InnerKeySelector));

            if (predicate == null)
            {
                return false;
            }

            QueriesBySource.Remove(joinClause);

            outerSelectExpression.RemoveRangeFromProjection(previousProjectionCount);

            var projection
                = QueryCompilationContext.QuerySourceRequiresMaterialization(joinClause)
                    ? innerSelectExpression.Projection
                    : Enumerable.Empty<Expression>();

            var joinExpression
                = outerSelectExpression.AddInnerJoin(
                    innerSelectExpression.Tables.Single(),
                    projection,
                    innerSelectExpression.Predicate);

            joinExpression.Predicate = predicate;
            joinExpression.QuerySource = joinClause;

            var outerShaper = ExtractShaper(outerShapedQuery, 0);
            var innerShaper = ExtractShaper(innerShapedQuery, previousProjectionCount);

            if (innerShaper.Type == typeof(AnonymousObject))
            {
                Expression = outerShapedQuery;
                CurrentParameter = previousParameter;

                foreach (var mapping in previousMapping)
                {
                    QueryCompilationContext.QuerySourceMapping
                        .ReplaceMapping(mapping.Key, mapping.Value);
                }
            }
            else
            {
                var materializerLambda = (LambdaExpression)joinMethodCallExpression.Arguments.Last();

                var compositeShaper
                    = CompositeShaper.Create(joinClause, outerShaper, innerShaper, materializerLambda);

                compositeShaper.SaveAccessorExpression(QueryCompilationContext.QuerySourceMapping);

                innerShaper.UpdateQuerySource(joinClause);

                Expression
                    = Expression.Call(
                        // ReSharper disable once PossibleNullReferenceException
                        outerShapedQuery.Method
                            .GetGenericMethodDefinition()
                            .MakeGenericMethod(materializerLambda.ReturnType),
                        outerShapedQuery.Arguments[0],
                        outerShapedQuery.Arguments[1],
                        Expression.Constant(compositeShaper));
            }

            return true;
        }

        private bool TryFlattenGroupJoin(
            GroupJoinClause groupJoinClause,
            QueryModel queryModel,
            int index,
            int previousProjectionCount,
            ParameterExpression previousParameter,
            Dictionary<IQuerySource, Expression> previousMapping)
        {
            if (RequiresClientJoin || RequiresClientSelectMany)
            {
                return false;
            }

            var groupJoinMethodCallExpression = Expression as MethodCallExpression;

            var outerShapedQuery
                = groupJoinMethodCallExpression?.Arguments.FirstOrDefault() as MethodCallExpression;

            var innerShapedQuery
                = groupJoinMethodCallExpression?.Arguments.Skip(1).FirstOrDefault() as MethodCallExpression;

            if (groupJoinMethodCallExpression == null
                || !groupJoinMethodCallExpression.Method.MethodIsClosedFormOf(LinqOperatorProvider.GroupJoin)
                || !IsShapedQueryExpression(outerShapedQuery)
                || !IsShapedQueryExpression(innerShapedQuery))
            {
                return false;
            }

            if (!IsFlattenableGroupJoinDefaultIfEmpty(groupJoinClause, queryModel, index))
            {
                var shaperType = innerShapedQuery?.Arguments.Last().Type;
                if (shaperType == null
                    || !typeof(EntityShaper).IsAssignableFrom(shaperType))
                {
                    return false;
                }
            }

            var joinClause = groupJoinClause.JoinClause;

            var outerQuerySource = FindPreviousQuerySource(queryModel, index);
            var outerSelectExpression = TryGetQuery(outerQuerySource);
            var innerSelectExpression = TryGetQuery(joinClause);

            if (outerSelectExpression == null
                || innerSelectExpression == null)
            {
                return false;
            }

            var sqlTranslatingExpressionVisitor
                = _sqlTranslatingExpressionVisitorFactory.Create(this);

            var predicate
                = sqlTranslatingExpressionVisitor.Visit(
                    Expression.Equal(joinClause.OuterKeySelector, joinClause.InnerKeySelector));

            if (predicate == null)
            {
                return false;
            }

            if (innerSelectExpression.Predicate != null)
            {
                var subSelectExpression = innerSelectExpression.PushDownSubquery();
                innerSelectExpression.ExplodeStarProjection();
                subSelectExpression.IsProjectStar = true;
                subSelectExpression.QuerySource = joinClause;

                predicate
                    = sqlTranslatingExpressionVisitor.Visit(
                        Expression.Equal(joinClause.OuterKeySelector, joinClause.InnerKeySelector));
            }

            QueriesBySource.Remove(joinClause);

            outerSelectExpression.RemoveRangeFromProjection(previousProjectionCount);

            var projections
                = QueryCompilationContext.QuerySourceRequiresMaterialization(joinClause)
                    ? innerSelectExpression.Projection
                    : Enumerable.Empty<Expression>();

            var joinExpression
                = outerSelectExpression.AddLeftOuterJoin(
                    innerSelectExpression.Tables.Single(),
                    projections);

            joinExpression.Predicate = predicate;
            joinExpression.QuerySource = joinClause;

            if (TryFlattenGroupJoinDefaultIfEmpty(
                groupJoinClause,
                queryModel,
                index,
                previousProjectionCount,
                previousParameter,
                previousMapping))
            {
                return true;
            }

            outerSelectExpression.LiftOrderBy();

            var outerJoinOrderingExtractor = new OuterJoinOrderingExtractor();
            outerJoinOrderingExtractor.Visit(predicate);

            if (!outerJoinOrderingExtractor.DependentToPrincipalFound)
            {
                foreach (var expression in outerJoinOrderingExtractor.Expressions)
                {
                    outerSelectExpression.AddToOrderBy(
                        new Ordering(expression, OrderingDirection.Asc));
                }
            }

            var outerShaper = ExtractShaper(outerShapedQuery, 0);
            var innerShaper = ExtractShaper(innerShapedQuery, previousProjectionCount);

            innerShaper.UpdateQuerySource(joinClause);

            var queryMethodProvider = QueryCompilationContext.QueryMethodProvider;

            var groupJoinMethod
                = queryMethodProvider.GroupJoinMethod.MakeGenericMethod(
                    outerShaper.Type,
                    innerShaper.Type,
                    ((LambdaExpression)groupJoinMethodCallExpression.Arguments[2]).ReturnType,
                    ((LambdaExpression)groupJoinMethodCallExpression.Arguments.Last()).ReturnType);

            var newShapedQueryMethod
                = Expression.Call(
                    queryMethodProvider.QueryMethod,
                    // ReSharper disable once PossibleNullReferenceException
                    outerShapedQuery.Arguments[0],
                    outerShapedQuery.Arguments[1]);

            Expression =
                Expression.Call(
                    groupJoinMethod,
                    Expression.Convert(
                        QueryContextParameter,
                        typeof(RelationalQueryContext)),
                    newShapedQueryMethod,
                    Expression.Constant(outerShaper),
                    Expression.Constant(innerShaper),
                    groupJoinMethodCallExpression.Arguments[3],
                    groupJoinMethodCallExpression.Arguments[4]);

            return true;
        }

        private bool IsFlattenableGroupJoinDefaultIfEmpty(
            [NotNull] GroupJoinClause groupJoinClause,
            QueryModel queryModel,
            int index)
        {
            var additionalFromClause
                = queryModel.BodyClauses.ElementAtOrDefault(index + 1)
                    as AdditionalFromClause;

            var subQueryModel
                = (additionalFromClause?.FromExpression as SubQueryExpression)
                ?.QueryModel;

            var referencedQuerySource
                = subQueryModel?.MainFromClause.FromExpression.TryGetReferencedQuerySource();

            if (referencedQuerySource != groupJoinClause
                || queryModel.CountQuerySourceReferences(groupJoinClause) != 1
                || subQueryModel.BodyClauses.Count != 0
                || subQueryModel.ResultOperators.Count != 1
                || !(subQueryModel.ResultOperators[0] is DefaultIfEmptyResultOperator))
            {
                return false;
            }

            return true;
        }

        private bool TryFlattenGroupJoinDefaultIfEmpty(
            [NotNull] GroupJoinClause groupJoinClause,
            QueryModel queryModel,
            int index,
            int previousProjectionCount,
            ParameterExpression previousParameter,
            Dictionary<IQuerySource, Expression> previousMapping)
        {
            var additionalFromClause
                = queryModel.BodyClauses.ElementAtOrDefault(index + 1)
                    as AdditionalFromClause;

            var subQueryModel
                = (additionalFromClause?.FromExpression as SubQueryExpression)
                ?.QueryModel;

            var referencedQuerySource
                = subQueryModel?.MainFromClause.FromExpression.TryGetReferencedQuerySource();

            if (referencedQuerySource != groupJoinClause
                || queryModel.CountQuerySourceReferences(groupJoinClause) != 1
                || subQueryModel.BodyClauses.Count != 0
                || subQueryModel.ResultOperators.Count != 1
                || !(subQueryModel.ResultOperators[0] is DefaultIfEmptyResultOperator))
            {
                return false;
            }

            _unflattenedGroupJoinClauses.Remove(groupJoinClause);
            _flattenedAdditionalFromClauses.Add(additionalFromClause);

            var groupJoinMethodCallExpression = (MethodCallExpression)Expression;

            var outerShapedQuery = (MethodCallExpression)groupJoinMethodCallExpression.Arguments[0];
            var innerShapedQuery = (MethodCallExpression)groupJoinMethodCallExpression.Arguments[1];

            var outerShaper = ExtractShaper(outerShapedQuery, 0);
            var innerShaper = ExtractShaper(innerShapedQuery, previousProjectionCount);

            CurrentParameter = previousParameter;

            foreach (var mapping in previousMapping)
            {
                QueryCompilationContext.AddOrUpdateMapping(mapping.Key, mapping.Value);
            }

            var innerItemParameter
                = Expression.Parameter(
                    innerShaper.Type,
                    additionalFromClause.ItemName);

            innerShaper.UpdateQuerySource(additionalFromClause);

            QueryCompilationContext.AddOrUpdateMapping(additionalFromClause, innerItemParameter);

            var transparentIdentifierType
                = CreateTransparentIdentifierType(
                    previousParameter.Type,
                    innerShaper.Type);

            var materializerLambda = Expression.Lambda(
                CallCreateTransparentIdentifier(
                    transparentIdentifierType,
                    previousParameter,
                    innerItemParameter),
                previousParameter,
                innerItemParameter);

            var compositeShaper
                = CompositeShaper.Create(additionalFromClause, outerShaper, innerShaper, materializerLambda);

            IntroduceTransparentScope(additionalFromClause, queryModel, index, transparentIdentifierType);

            compositeShaper.SaveAccessorExpression(QueryCompilationContext.QuerySourceMapping);

            Expression
                = Expression.Call(
                    outerShapedQuery.Method
                        .GetGenericMethodDefinition()
                        .MakeGenericMethod(transparentIdentifierType),
                    outerShapedQuery.Arguments[0],
                    outerShapedQuery.Arguments[1],
                    Expression.Constant(compositeShaper));

            return true;
        }

        #endregion

        #region Binding

        /// <summary>
        ///     Bind a member expression to a value buffer access.
        /// </summary>
        /// <param name="memberExpression"> The member access expression. </param>
        /// <param name="expression"> The target expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression BindMemberToValueBuffer(MemberExpression memberExpression, Expression expression)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(expression, nameof(expression));

            return BindMemberExpression(
                memberExpression,
                (property, querySource, selectExpression) =>
                    {
                        var projectionIndex = selectExpression.GetProjectionIndex(property, querySource);

                        Debug.Assert(projectionIndex > -1);

                        return BindReadValueMethod(memberExpression.Type, expression, projectionIndex, property);
                    },
                bindSubQueries: true);
        }

        /// <summary>
        ///     Bind a method call expression to a value buffer access.
        /// </summary>
        /// <param name="methodCallExpression"> The method call expression. </param>
        /// <param name="expression"> The target expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression BindMethodCallToValueBuffer(
            MethodCallExpression methodCallExpression,
            Expression expression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));
            Check.NotNull(expression, nameof(expression));

            return BindMethodCallExpression(
                       methodCallExpression,
                       (property, querySource, selectExpression) =>
                           {
                               var projectionIndex = selectExpression.GetProjectionIndex(property, querySource);

                               Debug.Assert(projectionIndex > -1);

                               return BindReadValueMethod(methodCallExpression.Type, expression, projectionIndex, property);
                           },
                       bindSubQueries: true)
                   ?? ParentQueryModelVisitor?
                       .BindMethodCallToValueBuffer(methodCallExpression, expression);
        }

        /// <summary>
        ///     Bind a member expression.
        /// </summary>
        /// <typeparam name="TResult"> Type of the result. </typeparam>
        /// <param name="memberExpression"> The member access expression. </param>
        /// <param name="memberBinder"> The member binder. </param>
        /// <param name="bindSubQueries"> true to bind sub queries. </param>
        /// <returns>
        ///     A TResult.
        /// </returns>
        public virtual TResult BindMemberExpression<TResult>(
            [NotNull] MemberExpression memberExpression,
            [NotNull] Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder,
            bool bindSubQueries = false)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            return BindMemberExpression(memberExpression, null, memberBinder, bindSubQueries);
        }

        private TResult BindMemberExpression<TResult>(
            [NotNull] MemberExpression memberExpression,
            [CanBeNull] IQuerySource querySource,
            Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder,
            bool bindSubQueries)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            return base.BindMemberExpression(
                memberExpression, querySource,
                (property, qs) => BindMemberOrMethod(memberBinder, qs, property, bindSubQueries));
        }

        /// <summary>
        ///     Bind a member to a parameter from the outer query.
        /// </summary>
        /// <param name="memberExpression"> The expression to bind. </param>
        /// <returns> The bound expression. </returns>
        public virtual Expression BindMemberToOuterQueryParameter(
            [NotNull] MemberExpression memberExpression)
            => base.BindMemberExpression(
                memberExpression,
                null,
                (property, qs) => BindPropertyToOuterParameter(qs, property, true));

        /// <summary>
        ///     Bind a method call expression.
        /// </summary>
        /// <typeparam name="TResult"> Type of the result. </typeparam>
        /// <param name="methodCallExpression"> The method call expression. </param>
        /// <param name="memberBinder"> The member binder. </param>
        /// <param name="bindSubQueries"> true to bind sub queries. </param>
        /// <returns>
        ///     A TResult.
        /// </returns>
        public virtual TResult BindMethodCallExpression<TResult>(
            [NotNull] MethodCallExpression methodCallExpression,
            [NotNull] Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder,
            bool bindSubQueries = false)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            return BindMethodCallExpression(methodCallExpression, null, memberBinder, bindSubQueries);
        }

        private TResult BindMethodCallExpression<TResult>(
            MethodCallExpression methodCallExpression,
            IQuerySource querySource,
            Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder,
            bool bindSubQueries)
            => base.BindMethodCallExpression(
                methodCallExpression,
                querySource,
                (property, qs) => BindMemberOrMethod(memberBinder, qs, property, bindSubQueries));

        /// <summary>
        ///     Bind a local method call expression.
        /// </summary>
        /// <param name="methodCallExpression"> The local method call expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public virtual Expression BindLocalMethodCallExpression(
            [NotNull] MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            return base.BindMethodCallExpression<Expression>(
                methodCallExpression, null,
                (property, qs) =>
                    {
                        if (methodCallExpression.Arguments[0] is ParameterExpression parameterExpression)
                        {
                            return new PropertyParameterExpression(parameterExpression.Name, property);
                        }

                        if (methodCallExpression.Arguments[0] is ConstantExpression constantExpression)
                        {
                            return Expression.Constant(
                                property.GetGetter().GetClrValue(constantExpression.Value),
                                methodCallExpression.Method.GetGenericArguments()[0]);
                        }

                        return null;
                    });
        }

        /// <summary>
        ///     Bind a method call  to a parameter from the outer query.
        /// </summary>
        /// <param name="methodCallExpression"> The expression to bind. </param>
        /// <returns> The bound expression. </returns>
        public virtual Expression BindMethodToOuterQueryParameter(
            [NotNull] MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            return base.BindMethodCallExpression<Expression>(
                methodCallExpression,
                null,
                (property, qs) => BindPropertyToOuterParameter(qs, property, false));
        }

        private TResult BindMemberOrMethod<TResult>(
            Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder,
            IQuerySource querySource,
            IProperty property,
            bool bindSubQueries)
        {
            if (querySource != null)
            {
                var selectExpression = TryGetQuery(querySource);

                if (selectExpression == null
                    && bindSubQueries)
                {
                    if (_subQueryModelVisitorsBySource.TryGetValue(querySource, out var subQueryModelVisitor))
                    {
                        if (!subQueryModelVisitor.RequiresClientProjection)
                        {
                            selectExpression = subQueryModelVisitor.Queries.SingleOrDefault();

                            selectExpression?
                                .AddToProjection(
                                    property,
                                    querySource);
                        }
                    }
                }

                if (selectExpression != null)
                {
                    return memberBinder(property, querySource, selectExpression);
                }

                selectExpression
                    = ParentQueryModelVisitor?.TryGetQuery(querySource);

                selectExpression?.AddToProjection(
                    property,
                    querySource);
            }

            return default;
        }

        #endregion

        private bool _canBindPropertyToOuterParameter = true;

        private const string OuterQueryParameterNamePrefix = @"_outer_";

        private readonly Dictionary<string, Expression> _injectedParameters = new Dictionary<string, Expression>();

        private ParameterExpression BindPropertyToOuterParameter(IQuerySource querySource, IProperty property, bool isMemberExpression)
        {
            if (querySource != null
                && _canBindPropertyToOuterParameter
                && ParentQueryModelVisitor != null)
            {
                var isBindable = CanBindToParentUsingOuterParameter(querySource);

                // binding to grouping qsre - it's safe to do using outer parameter, even though it's client GroupJoin
                if (!isBindable
                    && querySource is MainFromClause mainFromClause
                    && mainFromClause.FromExpression is QuerySourceReferenceExpression mainClauseQsre
                    && mainClauseQsre.ReferencedQuerySource is GroupJoinClause groupJoinClause)
                {
                    isBindable = CanBindToParentUsingOuterParameter(groupJoinClause);
                }

                if (isBindable)
                {
                    var parameterName = OuterQueryParameterNamePrefix + property.Name;
                    var parameterWithSamePrefixCount
                        = QueryCompilationContext.ParentQueryReferenceParameters.Count(p => p.StartsWith(parameterName, StringComparison.Ordinal));

                    if (parameterWithSamePrefixCount > 0)
                    {
                        parameterName += parameterWithSamePrefixCount;
                    }

                    QueryCompilationContext.ParentQueryReferenceParameters.Add(parameterName);

                    var querySourceReference = new QuerySourceReferenceExpression(querySource);
                    var propertyExpression = isMemberExpression
                        ? querySourceReference.CreateEFPropertyExpression(property.PropertyInfo)
                        : querySourceReference.CreateEFPropertyExpression(property);

                    if (propertyExpression.Type.GetTypeInfo().IsValueType)
                    {
                        propertyExpression = Expression.Convert(propertyExpression, typeof(object));
                    }

                    _injectedParameters[parameterName] = propertyExpression;

                    Expression
                        = CreateInjectParametersExpression(
                            Expression,
                            new Dictionary<string, Expression> { [parameterName] = propertyExpression });

                    return Expression.Parameter(
                        property.ClrType,
                        parameterName);
                }
            }

            return null;
        }

        private bool CanBindToParentUsingOuterParameter(IQuerySource querySource)
        {
            var outerQueryModelVisitor = ParentQueryModelVisitor;
            var result = outerQueryModelVisitor?.TryGetQuery(querySource) != null;
            while (!result
                   && outerQueryModelVisitor != null)
            {
                outerQueryModelVisitor = outerQueryModelVisitor.ParentQueryModelVisitor;
                result = outerQueryModelVisitor?.TryGetQuery(querySource) != null;
            }

            return result;
        }

        private Expression CreateInjectParametersExpression(Expression expression, Dictionary<string, Expression> parameters)
        {
            var parameterNameExpressions = new List<ConstantExpression>();
            var parameterValueExpressions = new List<Expression>();

            if (expression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method.MethodIsClosedFormOf(QueryCompilationContext.QueryMethodProvider.InjectParametersMethod))
            {
                var existingParameterNamesExpression = (NewArrayExpression)methodCallExpression.Arguments[2];
                parameterNameExpressions.AddRange(existingParameterNamesExpression.Expressions.Cast<ConstantExpression>());

                var existingParameterValuesExpression = (NewArrayExpression)methodCallExpression.Arguments[3];
                parameterValueExpressions.AddRange(existingParameterValuesExpression.Expressions);

                expression = methodCallExpression.Arguments[1];
            }

            parameterNameExpressions.AddRange(parameters.Keys.Select(Expression.Constant));
            parameterValueExpressions.AddRange(parameters.Values);

            var elementType = expression.Type.GetTypeInfo().GenericTypeArguments.Single();

            return Expression.Call(
                QueryCompilationContext.QueryMethodProvider.InjectParametersMethod.MakeGenericMethod(elementType),
                QueryContextParameter,
                expression,
                Expression.NewArrayInit(typeof(string), parameterNameExpressions),
                Expression.NewArrayInit(typeof(object), parameterValueExpressions));
        }

        /// <summary>
        ///     Lifts the outer parameters injected into a subquery into the query
        ///     expression that is being built by this query model visitor, so that
        ///     the subquery can be lifted.
        /// </summary>
        /// <param name="subQueryModelVisitor"> The query model visitor for the subquery being lifted. </param>
        public virtual void LiftInjectedParameters([NotNull] RelationalQueryModelVisitor subQueryModelVisitor)
        {
            Check.NotNull(subQueryModelVisitor, nameof(subQueryModelVisitor));

            if (!subQueryModelVisitor._injectedParameters.Any())
            {
                return;
            }

            foreach (var pair in subQueryModelVisitor._injectedParameters)
            {
                _injectedParameters[pair.Key] = pair.Value;
            }

            Expression = CreateInjectParametersExpression(Expression, subQueryModelVisitor._injectedParameters);
        }
    }
}
