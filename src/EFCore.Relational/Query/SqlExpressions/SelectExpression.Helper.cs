// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

public sealed partial class SelectExpression
{
    private sealed class EntityShaperNullableMarkingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression extensionExpression)
            => extensionExpression is EntityShaperExpression entityShaper
                ? entityShaper.MakeNullable()
                : base.VisitExtension(extensionExpression);
    }

    private sealed class SelectExpressionCorrelationFindingExpressionVisitor : ExpressionVisitor
    {
        private readonly SelectExpression _outerSelectExpression;
        private bool _containsOuterReference;

        public SelectExpressionCorrelationFindingExpressionVisitor(SelectExpression outerSelectExpression)
        {
            _outerSelectExpression = outerSelectExpression;
        }

        public bool ContainsOuterReference(SelectExpression selectExpression)
        {
            _containsOuterReference = false;

            Visit(selectExpression);

            return _containsOuterReference;
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            if (_containsOuterReference)
            {
                return expression;
            }

            if (expression is ColumnExpression columnExpression
                && _outerSelectExpression.ContainsTableReference(columnExpression))
            {
                _containsOuterReference = true;

                return expression;
            }

            return base.Visit(expression);
        }
    }

    private sealed class ProjectionMemberRemappingExpressionVisitor : ExpressionVisitor
    {
        private readonly SelectExpression _queryExpression;
        private readonly Dictionary<ProjectionMember, ProjectionMember> _projectionMemberMappings;

        public ProjectionMemberRemappingExpressionVisitor(
            SelectExpression queryExpression,
            Dictionary<ProjectionMember, ProjectionMember> projectionMemberMappings)
        {
            _queryExpression = queryExpression;
            _projectionMemberMappings = projectionMemberMappings;
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            if (expression is ProjectionBindingExpression projectionBindingExpression)
            {
                Check.DebugAssert(
                    projectionBindingExpression.ProjectionMember != null,
                    "ProjectionBindingExpression must have projection member.");

                return new ProjectionBindingExpression(
                    _queryExpression,
                    _projectionMemberMappings[projectionBindingExpression.ProjectionMember],
                    projectionBindingExpression.Type);
            }

            return base.Visit(expression);
        }
    }

    private sealed class ProjectionMemberToIndexConvertingExpressionVisitor : ExpressionVisitor
    {
        private readonly SelectExpression _queryExpression;
        private readonly Dictionary<ProjectionMember, int> _projectionMemberMappings;

        public ProjectionMemberToIndexConvertingExpressionVisitor(
            SelectExpression queryExpression,
            Dictionary<ProjectionMember, int> projectionMemberMappings)
        {
            _queryExpression = queryExpression;
            _projectionMemberMappings = projectionMemberMappings;
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            if (expression is ProjectionBindingExpression projectionBindingExpression)
            {
                Check.DebugAssert(
                    projectionBindingExpression.ProjectionMember != null,
                    "ProjectionBindingExpression must have projection member.");

                return new ProjectionBindingExpression(
                    _queryExpression,
                    _projectionMemberMappings[projectionBindingExpression.ProjectionMember],
                    projectionBindingExpression.Type);
            }

            return base.Visit(expression);
        }
    }

    private sealed class ProjectionIndexRemappingExpressionVisitor : ExpressionVisitor
    {
        private readonly SelectExpression _oldSelectExpression;
        private readonly SelectExpression _newSelectExpression;
        private readonly int[] _indexMap;

        public ProjectionIndexRemappingExpressionVisitor(
            SelectExpression oldSelectExpression,
            SelectExpression newSelectExpression,
            int[] indexMap)
        {
            _oldSelectExpression = oldSelectExpression;
            _newSelectExpression = newSelectExpression;
            _indexMap = indexMap;
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            if (expression is ProjectionBindingExpression projectionBindingExpression
                && ReferenceEquals(projectionBindingExpression.QueryExpression, _oldSelectExpression))
            {
                Check.DebugAssert(
                    projectionBindingExpression.Index != null,
                    "ProjectionBindingExpression must have index.");

                return new ProjectionBindingExpression(
                    _newSelectExpression,
                    _indexMap[projectionBindingExpression.Index.Value],
                    projectionBindingExpression.Type);
            }

            return base.Visit(expression);
        }
    }

    private sealed class SqlRemappingVisitor : ExpressionVisitor
    {
        private readonly SelectExpression _subquery;
        private readonly TableReferenceExpression _tableReferenceExpression;
        private readonly Dictionary<SqlExpression, ColumnExpression> _mappings;
        private readonly HashSet<SqlExpression> _correlatedTerms;
        private bool _groupByDiscovery;

        public SqlRemappingVisitor(
            Dictionary<SqlExpression, ColumnExpression> mappings,
            SelectExpression subquery,
            TableReferenceExpression tableReferenceExpression)
        {
            _subquery = subquery;
            _tableReferenceExpression = tableReferenceExpression;
            _mappings = mappings;
            _groupByDiscovery = subquery._groupBy.Count > 0;
            _correlatedTerms = new HashSet<SqlExpression>(ReferenceEqualityComparer.Instance);
        }

        [return: NotNullIfNotNull("sqlExpression")]
        public SqlExpression? Remap(SqlExpression? sqlExpression)
            => (SqlExpression?)Visit(sqlExpression);

        [return: NotNullIfNotNull("selectExpression")]
        public SelectExpression? Remap(SelectExpression? selectExpression)
        {
            var result = (SelectExpression?)Visit(selectExpression);

            if (_correlatedTerms.Count > 0)
            {
                new EnclosingTermFindingVisitor(_correlatedTerms).Visit(selectExpression);
                _groupByDiscovery = false;
                result = (SelectExpression?)Visit(selectExpression);
            }

            return result;
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            switch (expression)
            {
                case SqlExpression sqlExpression
                    when _mappings.TryGetValue(sqlExpression, out var outer):
                    return outer;

                case ColumnExpression columnExpression
                    when _groupByDiscovery
                    && _subquery.ContainsTableReference(columnExpression):
                    _correlatedTerms.Add(columnExpression);
                    return columnExpression;

                case SqlExpression sqlExpression
                    when !_groupByDiscovery
                    && sqlExpression is not SqlConstantExpression or SqlParameterExpression
                    && _correlatedTerms.Contains(sqlExpression):
                    var outerColumn = _subquery.GenerateOuterColumn(_tableReferenceExpression, sqlExpression);
                    _mappings[sqlExpression] = outerColumn;
                    return outerColumn;

                case ColumnExpression columnExpression
                    when !_groupByDiscovery
                    && _subquery.ContainsTableReference(columnExpression):
                    var outerColumn1 = _subquery.GenerateOuterColumn(_tableReferenceExpression, columnExpression);
                    _mappings[columnExpression] = outerColumn1;
                    return outerColumn1;

                default:
                    return base.Visit(expression);
            }
        }

        private sealed class EnclosingTermFindingVisitor : ExpressionVisitor
        {
            private readonly HashSet<SqlExpression> _correlatedTerms;
            private bool _doesNotContainLocalTerms;

            public EnclosingTermFindingVisitor(HashSet<SqlExpression> correlatedTerms)
            {
                _correlatedTerms = correlatedTerms;
                _doesNotContainLocalTerms = true;
            }

            [return: NotNullIfNotNull("expression")]
            public override Expression? Visit(Expression? expression)
            {
                if (expression is SqlExpression sqlExpression)
                {
                    if (_correlatedTerms.Contains(sqlExpression)
                        || sqlExpression is SqlConstantExpression or SqlParameterExpression)
                    {
                        _correlatedTerms.Add(sqlExpression);
                        return sqlExpression;
                    }

                    var parentDoesNotContainLocalTerms = _doesNotContainLocalTerms;
                    _doesNotContainLocalTerms = sqlExpression is not ColumnExpression;
                    base.Visit(expression);
                    if (_doesNotContainLocalTerms)
                    {
                        _correlatedTerms.Add(sqlExpression);
                    }

                    _doesNotContainLocalTerms = _doesNotContainLocalTerms && parentDoesNotContainLocalTerms;

                    return expression;
                }

                return base.Visit(expression);
            }
        }
    }

    private sealed class ColumnExpressionFindingExpressionVisitor : ExpressionVisitor
    {
        private Dictionary<string, HashSet<string>?>? _columnReferenced;
        private Dictionary<string, HashSet<string>?>? _columnsUsedInJoinCondition;

        public Dictionary<string, HashSet<string>?> FindColumns(SelectExpression selectExpression)
        {
            _columnReferenced = new Dictionary<string, HashSet<string>?>();
            _columnsUsedInJoinCondition = new Dictionary<string, HashSet<string>?>();

            foreach (var table in selectExpression.Tables)
            {
                var tableAlias = table is JoinExpressionBase joinExpressionBase
                    ? joinExpressionBase.Table.Alias!
                    : table.Alias!;
                _columnReferenced[tableAlias] = null;
            }

            Visit(selectExpression);

            foreach (var keyValuePair in _columnsUsedInJoinCondition)
            {
                var tableAlias = keyValuePair.Key;
                if (_columnReferenced[tableAlias] != null
                    && _columnsUsedInJoinCondition[tableAlias] != null)
                {
                    _columnReferenced[tableAlias]!.UnionWith(_columnsUsedInJoinCondition[tableAlias]!);
                }
            }

            return _columnReferenced;
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            switch (expression)
            {
                case ColumnExpression columnExpression:
                    var tableAlias = columnExpression.TableAlias;
                    if (_columnReferenced!.ContainsKey(tableAlias))
                    {
                        _columnReferenced[tableAlias] ??= new HashSet<string>();

                        _columnReferenced[tableAlias]!.Add(columnExpression.Name);
                    }

                    // Always skip the table of ColumnExpression since it will traverse into deeper subquery
                    return columnExpression;

                case PredicateJoinExpressionBase predicateJoinExpressionBase:
                    var predicateJoinTableAlias = predicateJoinExpressionBase.Table.Alias!;
                    // Visiting the join predicate will add some columns for join table.
                    // But if all the referenced columns are in join predicate only then we can remove the join table.
                    // So if there are no referenced columns yet means there is still potential to remove this table,
                    // In such case we moved the columns encountered in join predicate to other dictionary and later merge
                    // if there are more references to the join table outside of join predicate.
                    // We should also remove references to the outer if this column gets removed then that subquery can also remove projections
                    // But currently we only remove table for TPT & entity splitting scenario
                    // in which there are all table expressions which connects via joins.
                    var joinOnSameLevel = _columnReferenced!.ContainsKey(predicateJoinTableAlias);
                    var noReferences = !joinOnSameLevel || _columnReferenced[predicateJoinTableAlias] == null;
                    base.Visit(predicateJoinExpressionBase);
                    if (noReferences && joinOnSameLevel)
                    {
                        _columnsUsedInJoinCondition![predicateJoinTableAlias] = _columnReferenced[predicateJoinTableAlias];
                        _columnReferenced[predicateJoinTableAlias] = null;
                    }

                    return predicateJoinExpressionBase;

                default:
                    return base.Visit(expression);
            }
        }
    }

    private sealed class TableReferenceUpdatingExpressionVisitor : ExpressionVisitor
    {
        private readonly SelectExpression _oldSelect;
        private readonly SelectExpression _newSelect;

        public TableReferenceUpdatingExpressionVisitor(SelectExpression oldSelect, SelectExpression newSelect)
        {
            _oldSelect = oldSelect;
            _newSelect = newSelect;
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            if (expression is ConcreteColumnExpression columnExpression)
            {
                columnExpression.UpdateTableReference(_oldSelect, _newSelect);
            }

            return base.Visit(expression);
        }
    }

    private sealed class IdentifierComparer : IEqualityComparer<(ColumnExpression Column, ValueComparer Comparer)>
    {
        public bool Equals((ColumnExpression Column, ValueComparer Comparer) x, (ColumnExpression Column, ValueComparer Comparer) y)
            => x.Column.Equals(y.Column);

        public int GetHashCode((ColumnExpression Column, ValueComparer Comparer) obj)
            => obj.Column.GetHashCode();
    }

    private sealed class AliasUniquifier : ExpressionVisitor
    {
        private readonly HashSet<string> _usedAliases;
        private readonly List<SelectExpression> _visitedSelectExpressions = new();

        public AliasUniquifier(HashSet<string> usedAliases)
        {
            _usedAliases = usedAliases;
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            if (expression is SelectExpression innerSelectExpression
                && !_visitedSelectExpressions.Contains(innerSelectExpression))
            {
                for (var i = 0; i < innerSelectExpression._tableReferences.Count; i++)
                {
                    var newAlias = GenerateUniqueAlias(_usedAliases, innerSelectExpression._tableReferences[i].Alias);
                    innerSelectExpression._tableReferences[i].Alias = newAlias;
                    UnwrapJoinExpression(innerSelectExpression._tables[i]).Alias = newAlias;
                }

                _visitedSelectExpressions.Add(innerSelectExpression);
            }

            return base.Visit(expression);
        }
    }

    private sealed class TableReferenceExpression : Expression
    {
        private SelectExpression _selectExpression;

        public TableReferenceExpression(SelectExpression selectExpression, string alias)
        {
            _selectExpression = selectExpression;
            Alias = alias;
        }

        public TableExpressionBase Table
            => _selectExpression.Tables.Single(
                e => string.Equals((e as JoinExpressionBase)?.Table.Alias ?? e.Alias, Alias, StringComparison.OrdinalIgnoreCase));

        public string Alias { get; internal set; }

        public override Type Type
            => typeof(object);

        public override ExpressionType NodeType
            => ExpressionType.Extension;

        public void UpdateTableReference(SelectExpression oldSelect, SelectExpression newSelect)
        {
            if (ReferenceEquals(oldSelect, _selectExpression))
            {
                _selectExpression = newSelect;
            }
        }

        internal void Verify(SelectExpression selectExpression)
        {
            if (!ReferenceEquals(selectExpression, _selectExpression))
            {
                throw new InvalidOperationException("Dangling TableReferenceExpression.");
            }
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is TableReferenceExpression tableReferenceExpression
                    && Equals(tableReferenceExpression));

        // Since table reference is owned by SelectExpression, the select expression should be the same reference if they are matching.
        // That means we also don't need to compute the hashcode for it.
        // This allows us to break the cycle in computation when traversing this graph.
        private bool Equals(TableReferenceExpression tableReferenceExpression)
            => string.Equals(Alias, tableReferenceExpression.Alias, StringComparison.OrdinalIgnoreCase)
                && ReferenceEquals(_selectExpression, tableReferenceExpression._selectExpression);

        /// <inheritdoc />
        public override int GetHashCode()
            => 0;
    }

    private sealed class TpcTablesExpression : TableExpressionBase
    {
        public TpcTablesExpression(
            string? alias,
            IEntityType entityType,
            IReadOnlyList<SelectExpression> subSelectExpressions)
            : base(alias)
        {
            EntityType = entityType;
            SelectExpressions = subSelectExpressions;
        }

        private TpcTablesExpression(
            string? alias,
            IEntityType entityType,
            IReadOnlyList<SelectExpression> subSelectExpressions,
            IEnumerable<IAnnotation>? annotations)
            : base(alias, annotations)
        {
            EntityType = entityType;
            SelectExpressions = subSelectExpressions;
        }

        [NotNull]
        public override string? Alias
        {
            get => base.Alias!;
            internal set => base.Alias = value;
        }

        public IEntityType EntityType { get; }

        public IReadOnlyList<SelectExpression> SelectExpressions { get; }

        public TpcTablesExpression Prune(IReadOnlyList<string> discriminatorValues)
        {
            var subSelectExpressions = discriminatorValues.Count == 0
                ? new List<SelectExpression> { SelectExpressions[0] }
                : SelectExpressions.Where(
                    se =>
                        discriminatorValues.Contains((string)((SqlConstantExpression)se.Projection[^1].Expression).Value!)).ToList();

            Check.DebugAssert(subSelectExpressions.Count > 0, "TPC must have at least 1 table selected.");

            return new TpcTablesExpression(Alias, EntityType, subSelectExpressions, GetAnnotations());
        }

        // This is implementation detail hence visitors are not supposed to see inside unless they really need to.
        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => this;

        protected override TableExpressionBase CreateWithAnnotations(IEnumerable<IAnnotation> annotations)
            => new TpcTablesExpression(Alias, EntityType, SelectExpressions, annotations);

        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.AppendLine("(");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.VisitCollection(SelectExpressions, e => e.AppendLine().AppendLine("UNION ALL"));
            }

            expressionPrinter.AppendLine()
                .AppendLine(") AS " + Alias);
            PrintAnnotations(expressionPrinter);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is TpcTablesExpression tpcTablesExpression
                    && Equals(tpcTablesExpression));

        private bool Equals(TpcTablesExpression tpcTablesExpression)
        {
            if (!base.Equals(tpcTablesExpression)
                || EntityType != tpcTablesExpression.EntityType)
            {
                return false;
            }

            return SelectExpressions.SequenceEqual(tpcTablesExpression.SelectExpressions);
        }

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), EntityType);
    }

    private sealed class ConcreteColumnExpression : ColumnExpression
    {
        private readonly TableReferenceExpression _table;

        public ConcreteColumnExpression(IProperty property, IColumnBase column, TableReferenceExpression table, bool nullable)
            : this(
                column.Name,
                table,
                property.ClrType.UnwrapNullableType(),
                column.PropertyMappings.First(m => m.Property == property).TypeMapping,
                nullable || column.IsNullable)
        {
        }

        public ConcreteColumnExpression(ProjectionExpression subqueryProjection, TableReferenceExpression table)
            : this(
                subqueryProjection.Alias, table,
                subqueryProjection.Type, subqueryProjection.Expression.TypeMapping!,
                IsNullableProjection(subqueryProjection))
        {
        }

        private static bool IsNullableProjection(ProjectionExpression projectionExpression)
            => projectionExpression.Expression switch
            {
                ColumnExpression columnExpression => columnExpression.IsNullable,
                SqlConstantExpression sqlConstantExpression => sqlConstantExpression.Value == null,
                _ => true
            };

        public ConcreteColumnExpression(
            string name,
            TableReferenceExpression table,
            Type type,
            RelationalTypeMapping typeMapping,
            bool nullable)
            : base(type, typeMapping)
        {
            Name = name;
            _table = table;
            IsNullable = nullable;
        }

        public override string Name { get; }

        public override TableExpressionBase Table
            => _table.Table;

        public override string TableAlias
            => _table.Alias;

        public override bool IsNullable { get; }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => this;

        public override ConcreteColumnExpression MakeNullable()
            => IsNullable ? this : new ConcreteColumnExpression(Name, _table, Type, TypeMapping!, true);

        public void UpdateTableReference(SelectExpression oldSelect, SelectExpression newSelect)
            => _table.UpdateTableReference(oldSelect, newSelect);

        internal void Verify(IReadOnlyList<TableReferenceExpression> tableReferences)
        {
            if (!tableReferences.Contains(_table, ReferenceEqualityComparer.Instance))
            {
                throw new InvalidOperationException("Dangling column.");
            }
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is ConcreteColumnExpression concreteColumnExpression
                    && Equals(concreteColumnExpression));

        private bool Equals(ConcreteColumnExpression concreteColumnExpression)
            => base.Equals(concreteColumnExpression)
                && Name == concreteColumnExpression.Name
                && _table.Equals(concreteColumnExpression._table)
                && IsNullable == concreteColumnExpression.IsNullable;

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Name, _table, IsNullable);
    }

    private struct SingleCollectionInfo
    {
        public SingleCollectionInfo(
            Expression parentIdentifier,
            Expression outerIdentifier,
            Expression selfIdentifier,
            IReadOnlyList<ValueComparer> parentIdentifierValueComparers,
            IReadOnlyList<ValueComparer> outerIdentifierValueComparers,
            IReadOnlyList<ValueComparer> selfIdentifierValueComparers,
            Expression shaperExpression)
        {
            ParentIdentifier = parentIdentifier;
            OuterIdentifier = outerIdentifier;
            SelfIdentifier = selfIdentifier;
            ParentIdentifierValueComparers = parentIdentifierValueComparers;
            OuterIdentifierValueComparers = outerIdentifierValueComparers;
            SelfIdentifierValueComparers = selfIdentifierValueComparers;
            ShaperExpression = shaperExpression;
        }

        public Expression ParentIdentifier { get; }
        public Expression OuterIdentifier { get; }
        public Expression SelfIdentifier { get; }
        public IReadOnlyList<ValueComparer> ParentIdentifierValueComparers { get; }
        public IReadOnlyList<ValueComparer> OuterIdentifierValueComparers { get; }
        public IReadOnlyList<ValueComparer> SelfIdentifierValueComparers { get; }
        public Expression ShaperExpression { get; }
    }

    private struct SplitCollectionInfo
    {
        public SplitCollectionInfo(
            Expression parentIdentifier,
            Expression childIdentifier,
            IReadOnlyList<ValueComparer> identifierValueComparers,
            SelectExpression selectExpression,
            Expression shaperExpression)
        {
            ParentIdentifier = parentIdentifier;
            ChildIdentifier = childIdentifier;
            IdentifierValueComparers = identifierValueComparers;
            SelectExpression = selectExpression;
            ShaperExpression = shaperExpression;
        }

        public Expression ParentIdentifier { get; }
        public Expression ChildIdentifier { get; }
        public IReadOnlyList<ValueComparer> IdentifierValueComparers { get; }
        public SelectExpression SelectExpression { get; }
        public Expression ShaperExpression { get; }
    }

    private sealed class ClientProjectionRemappingExpressionVisitor : ExpressionVisitor
    {
        private readonly List<object> _clientProjectionIndexMap;

        public ClientProjectionRemappingExpressionVisitor(List<object> clientProjectionIndexMap)
        {
            _clientProjectionIndexMap = clientProjectionIndexMap;
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            if (expression is ProjectionBindingExpression projectionBindingExpression)
            {
                var value = _clientProjectionIndexMap[projectionBindingExpression.Index!.Value];
                if (value is int intValue)
                {
                    return new ProjectionBindingExpression(
                        projectionBindingExpression.QueryExpression, intValue, projectionBindingExpression.Type);
                }

                if (value is Expression innerShaper)
                {
                    return Visit(innerShaper);
                }

                throw new InvalidCastException();
            }

            if (expression is CollectionResultExpression collectionResultExpression)
            {
                var innerProjectionBindingExpression = collectionResultExpression.ProjectionBindingExpression;
                var value = _clientProjectionIndexMap[innerProjectionBindingExpression.Index!.Value];
                if (value is SingleCollectionInfo singleCollectionInfo)
                {
                    return new RelationalCollectionShaperExpression(
                        singleCollectionInfo.ParentIdentifier,
                        singleCollectionInfo.OuterIdentifier,
                        singleCollectionInfo.SelfIdentifier,
                        singleCollectionInfo.ParentIdentifierValueComparers,
                        singleCollectionInfo.OuterIdentifierValueComparers,
                        singleCollectionInfo.SelfIdentifierValueComparers,
                        singleCollectionInfo.ShaperExpression,
                        collectionResultExpression.Navigation,
                        collectionResultExpression.ElementType);
                }

                if (value is SplitCollectionInfo splitCollectionInfo)
                {
                    return new RelationalSplitCollectionShaperExpression(
                        splitCollectionInfo.ParentIdentifier,
                        splitCollectionInfo.ChildIdentifier,
                        splitCollectionInfo.IdentifierValueComparers,
                        splitCollectionInfo.SelectExpression,
                        splitCollectionInfo.ShaperExpression,
                        collectionResultExpression.Navigation,
                        collectionResultExpression.ElementType);
                }

                if (value is int)
                {
                    var binding = (ProjectionBindingExpression)Visit(collectionResultExpression.ProjectionBindingExpression);

                    return collectionResultExpression.Update(binding);
                }

                throw new InvalidOperationException();
            }

            if (expression is RelationalGroupByResultExpression relationalGroupByResultExpression)
            {
                // Only element shaper needs remapping
                return new RelationalGroupByResultExpression(
                    relationalGroupByResultExpression.KeyIdentifier,
                    relationalGroupByResultExpression.KeyIdentifierValueComparers,
                    relationalGroupByResultExpression.KeyShaper,
                    Visit(relationalGroupByResultExpression.ElementShaper));
            }

            return base.Visit(expression);
        }
    }

    private sealed class SelectExpressionVerifyingExpressionVisitor : ExpressionVisitor
    {
        private readonly List<TableReferenceExpression> _tableReferencesInScope = new();

        public SelectExpressionVerifyingExpressionVisitor(IEnumerable<TableReferenceExpression> tableReferencesInScope)
        {
            _tableReferencesInScope.AddRange(tableReferencesInScope);
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            switch (expression)
            {
                case SelectExpression selectExpression:
                    foreach (var tableReference in selectExpression._tableReferences)
                    {
                        tableReference.Verify(selectExpression);
                    }

                    var currentLevelTableReferences = new List<TableReferenceExpression>();
                    for (var i = 0; i < selectExpression._tables.Count; i++)
                    {
                        var table = selectExpression._tables[i];
                        var tableReference = selectExpression._tableReferences[i];
                        switch (table)
                        {
                            case PredicateJoinExpressionBase predicateJoinExpressionBase:
                                Verify(predicateJoinExpressionBase.Table, _tableReferencesInScope);
                                currentLevelTableReferences.Add(tableReference);
                                Verify(
                                    predicateJoinExpressionBase.JoinPredicate,
                                    _tableReferencesInScope.Concat(currentLevelTableReferences));
                                break;

                            case SelectExpression innerSelectExpression:
                                Verify(innerSelectExpression, _tableReferencesInScope);
                                break;

                            case CrossApplyExpression crossApplyExpression:
                                Verify(crossApplyExpression, _tableReferencesInScope.Concat(currentLevelTableReferences));
                                break;

                            case OuterApplyExpression outerApplyExpression:
                                Verify(outerApplyExpression, _tableReferencesInScope.Concat(currentLevelTableReferences));
                                break;

                            case JoinExpressionBase joinExpressionBase:
                                Verify(joinExpressionBase.Table, _tableReferencesInScope);
                                break;

                            case SetOperationBase setOperationBase:
                                Verify(setOperationBase.Source1, _tableReferencesInScope);
                                Verify(setOperationBase.Source2, _tableReferencesInScope);
                                break;
                        }

                        if (table is not PredicateJoinExpressionBase)
                        {
                            currentLevelTableReferences.Add(tableReference);
                        }
                    }

                    _tableReferencesInScope.AddRange(currentLevelTableReferences);

                    foreach (var projection in selectExpression._projection)
                    {
                        Visit(projection);
                    }

                    foreach (var keyValuePair in selectExpression._projectionMapping)
                    {
                        Visit(keyValuePair.Value);
                    }

                    foreach (var clientProjection in selectExpression._clientProjections)
                    {
                        Visit(clientProjection);
                    }

                    foreach (var grouping in selectExpression._groupBy)
                    {
                        Visit(grouping);
                    }

                    foreach (var ordering in selectExpression._orderings)
                    {
                        Visit(ordering);
                    }

                    Visit(selectExpression.Predicate);
                    Visit(selectExpression.Having);
                    Visit(selectExpression.Offset);
                    Visit(selectExpression.Limit);

                    foreach (var identifier in selectExpression._identifier)
                    {
                        Visit(identifier.Column);
                    }

                    foreach (var childIdentifier in selectExpression._childIdentifiers)
                    {
                        Visit(childIdentifier.Column);
                    }

                    return selectExpression;

                case ConcreteColumnExpression concreteColumnExpression:
                    concreteColumnExpression.Verify(_tableReferencesInScope);
                    return concreteColumnExpression;

                case ShapedQueryExpression shapedQueryExpression:
                    Verify(shapedQueryExpression.QueryExpression, _tableReferencesInScope);
                    return shapedQueryExpression;
            }

            return base.Visit(expression);
        }

        public static void Verify(Expression expression, IEnumerable<TableReferenceExpression> tableReferencesInScope)
            => new SelectExpressionVerifyingExpressionVisitor(tableReferencesInScope)
                .Visit(expression);
    }

    private sealed class CloningExpressionVisitor : ExpressionVisitor
    {
        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            if (expression is SelectExpression selectExpression)
            {
                var newProjectionMappings = new Dictionary<ProjectionMember, Expression>(selectExpression._projectionMapping.Count);
                foreach (var (projectionMember, value) in selectExpression._projectionMapping)
                {
                    newProjectionMappings[projectionMember] = Visit(value);
                }

                var newProjections = selectExpression._projection.Select(Visit).ToList<ProjectionExpression>();

                var newTables = selectExpression._tables.Select(Visit).ToList<TableExpressionBase>();
                var tpcTablesMap = selectExpression._tables.Select(UnwrapJoinExpression).Zip(newTables.Select(UnwrapJoinExpression))
                    .Where(e => e.First is TpcTablesExpression)
                    .ToDictionary(e => (TpcTablesExpression)e.First, e => (TpcTablesExpression)e.Second);

                // Since we are cloning we need to generate new table references
                // In other cases (like VisitChildren), we just reuse the same table references and update the SelectExpression inside it.
                // We initially assign old SelectExpression in table references and later update it once we construct clone
                var newTableReferences = selectExpression._tableReferences
                    .Select(e => new TableReferenceExpression(selectExpression, e.Alias)).ToList();
                Check.DebugAssert(
                    newTables.Select(e => GetAliasFromTableExpressionBase(e)).SequenceEqual(newTableReferences.Select(e => e.Alias)),
                    "Alias of updated tables must match the old tables.");

                var predicate = (SqlExpression?)Visit(selectExpression.Predicate);
                var newGroupBy = selectExpression._groupBy.Select(Visit)
                    .Where(e => !(e is SqlConstantExpression || e is SqlParameterExpression))
                    .ToList<SqlExpression>();
                var havingExpression = (SqlExpression?)Visit(selectExpression.Having);
                var newOrderings = selectExpression._orderings.Select(Visit).ToList<OrderingExpression>();
                var offset = (SqlExpression?)Visit(selectExpression.Offset);
                var limit = (SqlExpression?)Visit(selectExpression.Limit);

                var newSelectExpression = new SelectExpression(
                    selectExpression.Alias, newProjections, newTables, newTableReferences, newGroupBy, newOrderings,
                    selectExpression.GetAnnotations())
                {
                    Predicate = predicate,
                    Having = havingExpression,
                    Offset = offset,
                    Limit = limit,
                    IsDistinct = selectExpression.IsDistinct,
                    Tags = selectExpression.Tags,
                    _usedAliases = selectExpression._usedAliases.ToHashSet(),
                    _projectionMapping = newProjectionMappings,
                };
                newSelectExpression._mutable = selectExpression._mutable;

                newSelectExpression._removableJoinTables.AddRange(selectExpression._removableJoinTables);

                foreach (var kvp in selectExpression._tpcDiscriminatorValues)
                {
                    newSelectExpression._tpcDiscriminatorValues[tpcTablesMap[kvp.Key]] = kvp.Value;
                }

                // Since identifiers are ColumnExpression, they are not visited since they don't contain SelectExpression inside it.
                newSelectExpression._identifier.AddRange(selectExpression._identifier);
                newSelectExpression._childIdentifiers.AddRange(selectExpression._childIdentifiers);

                // Remap tableReferences in new select expression
                foreach (var tableReference in newTableReferences)
                {
                    tableReference.UpdateTableReference(selectExpression, newSelectExpression);
                }

                // Now that we have SelectExpression, we visit all components and update table references inside columns
                newSelectExpression = (SelectExpression)new ColumnExpressionReplacingExpressionVisitor(
                    selectExpression, newSelectExpression._tableReferences).Visit(newSelectExpression);

                return newSelectExpression;
            }

            if (expression is TpcTablesExpression tpcTablesExpression)
            {
                // Deep clone
                var subSelectExpressions = tpcTablesExpression.SelectExpressions.Select(Visit).ToList<SelectExpression>();
                var newTpcTable = new TpcTablesExpression(tpcTablesExpression.Alias, tpcTablesExpression.EntityType, subSelectExpressions);
                foreach (var annotation in tpcTablesExpression.GetAnnotations())
                {
                    newTpcTable.AddAnnotation(annotation.Name, annotation.Value);
                }

                return newTpcTable;
            }

            return expression is IClonableTableExpressionBase cloneable ? cloneable.Clone() : base.Visit(expression);
        }
    }

    private sealed class ColumnExpressionReplacingExpressionVisitor : ExpressionVisitor
    {
        private readonly SelectExpression _oldSelectExpression;
        private readonly Dictionary<string, TableReferenceExpression> _newTableReferences;

        public ColumnExpressionReplacingExpressionVisitor(
            SelectExpression oldSelectExpression,
            IEnumerable<TableReferenceExpression> newTableReferences)
        {
            _oldSelectExpression = oldSelectExpression;
            _newTableReferences = newTableReferences.ToDictionary(e => e.Alias);
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
            => expression is ConcreteColumnExpression concreteColumnExpression
                && _oldSelectExpression.ContainsTableReference(concreteColumnExpression)
                && _newTableReferences.ContainsKey(concreteColumnExpression.TableAlias)
                    ? new ConcreteColumnExpression(
                        concreteColumnExpression.Name,
                        _newTableReferences[concreteColumnExpression.TableAlias],
                        concreteColumnExpression.Type,
                        concreteColumnExpression.TypeMapping!,
                        concreteColumnExpression.IsNullable)
                    : base.Visit(expression);
    }

    private sealed class TpcTableExpressionRemovingExpressionVisitor : ExpressionVisitor
    {
        private readonly HashSet<string> _usedAliases;

        public TpcTableExpressionRemovingExpressionVisitor(HashSet<string> usedAliases)
        {
            _usedAliases = usedAliases;
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            if (expression is SelectExpression selectExpression
                && selectExpression._tpcDiscriminatorValues.Count > 0)
            {
                // If selectExpression doesn't have any other component and only TPC tables then we can lift it
                // We ignore projection here because if this selectExpression has projection from inner TPC
                // Then TPC will have superset of projection
                var identitySelect = selectExpression.Offset == null
                    && selectExpression.Limit == null
                    && !selectExpression.IsDistinct
                    && selectExpression.Predicate == null
                    && selectExpression.Having == null
                    && selectExpression.Orderings.Count == 0
                    && selectExpression.GroupBy.Count == 0
                    && selectExpression.Tables.Count == 1
                    // Any non-column projection means some composition which cannot be removed
                    && selectExpression.Projection.All(e => e.Expression is ColumnExpression);

                foreach (var kvp in selectExpression._tpcDiscriminatorValues)
                {
                    var tpcTablesExpression = kvp.Key;
                    var subSelectExpressions = tpcTablesExpression.Prune(kvp.Value.Item2).SelectExpressions
                        .Select(e => AssignUniqueAliasToTable(e)).ToList();
                    var firstSelectExpression = subSelectExpressions[0]; // There will be at least one.

                    int[]? reindexingMap = null;
                    if (identitySelect && selectExpression.Alias == null)
                    {
                        // Alias would be null when it is Exists/In like query or top level
                        // In Exists like query there is no projection
                        // In InExpression with subquery there will be only 1 projection
                        // In top-level the ordering of projection matters for shaper
                        // So for all cases in case of identity select when we are doing the lift, we need to remap projections
                        reindexingMap = new int[selectExpression.Projection.Count];
                        var innerProjections = firstSelectExpression.Projection.Select(e => e.Alias).ToList();
                        var identityMap = true;
                        for (var i = 0; i < selectExpression.Projection.Count; i++)
                        {
                            var newIndex = innerProjections.FindIndex(
                                e => string.Equals(e, selectExpression.Projection[i].Alias, StringComparison.Ordinal));
                            if (newIndex == -1)
                            {
                                // If for whatever reason outer has additional projection which cannot be remapped we avoid lift
                                identitySelect = false;
                                reindexingMap = null;
                                break;
                            }

                            identityMap &= (i == newIndex);
                            reindexingMap[i] = newIndex;
                        }

                        if (identityMap)
                        {
                            // If projection is same on outer/inner we don't need remapping
                            reindexingMap = null;
                        }
                    }

                    if (identitySelect)
                    {
                        // If we are lifting then we remove the alias for tpc because it will be unused.
                        _usedAliases.Remove(tpcTablesExpression.Alias);
                    }

                    RemapProjections(reindexingMap, firstSelectExpression);
                    var result = subSelectExpressions[0];
                    for (var i = 1; i < subSelectExpressions.Count; i++)
                    {
                        var setOperationAlias = GenerateUniqueAlias(_usedAliases, "t");
                        var source1 = result;
                        var source2 = subSelectExpressions[i];
                        RemapProjections(reindexingMap, source2);
                        var generatedSelectExpression = new SelectExpression(alias: null);

                        var unionExpression = new UnionExpression(setOperationAlias, source1, source2, distinct: false);
                        var tableReferenceExpression = new TableReferenceExpression(generatedSelectExpression, setOperationAlias);
                        generatedSelectExpression._tables.Add(unionExpression);
                        generatedSelectExpression._tableReferences.Add(tableReferenceExpression);
                        foreach (var projection in result.Projection)
                        {
                            generatedSelectExpression._projection.Add(
                                new ProjectionExpression(
                                    new ConcreteColumnExpression(projection, tableReferenceExpression), projection.Alias));
                        }

                        generatedSelectExpression._mutable = false;
                        result = generatedSelectExpression;
                    }

                    if (identitySelect)
                    {
                        result.Alias = selectExpression.Alias;
                        if (selectExpression.Alias == null)
                        {
                            // If top-level them copy over bindings for shaper
                            result._projectionMapping = selectExpression._projectionMapping;
                            result._clientProjections = selectExpression._clientProjections;
                        }

                        // Since identity select implies only 1 table so we can return without worrying about another iteration.
                        // Identity select shouldn't require base visit.
                        return result;
                    }

                    {
                        result.Alias = tpcTablesExpression.Alias;
                        var tableIndex =
                            selectExpression._tables.FindIndex(teb => ReferenceEquals(UnwrapJoinExpression(teb), tpcTablesExpression));
                        var table = selectExpression._tables[tableIndex];
                        selectExpression._tables[tableIndex] = (TableExpressionBase)ReplacingExpressionVisitor.Replace(
                            tpcTablesExpression, result, table);
                    }

                    SelectExpression AssignUniqueAliasToTable(SelectExpression se)
                    {
                        // we assign unique alias to inner tables here so that we can avoid wasting aliases on pruned tables
                        var table = se._tables[0];
                        var alias = GenerateUniqueAlias(_usedAliases, table.Alias!);
                        table.Alias = alias;
                        se._tableReferences[0].Alias = alias;

                        return se;
                    }
                }

                selectExpression._tpcDiscriminatorValues.Clear();
            }

            return base.Visit(expression);
        }

        private void RemapProjections(int[]? map, SelectExpression selectExpression)
        {
            if (map != null)
            {
                var projections = selectExpression.Projection.ToList();
                selectExpression._projection.Clear();
                for (var i = 0; i < map.Length; i++)
                {
                    selectExpression._projection.Add(projections[map[i]]);
                }
            }
        }
    }
}
