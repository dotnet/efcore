// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public sealed partial class SelectExpression
    {
        private sealed class EntityShaperNullableMarkingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitExtension(Expression extensionExpression)
            {
                Check.NotNull(extensionExpression, nameof(extensionExpression));

                return extensionExpression is EntityShaperExpression entityShaper
                    ? entityShaper.MakeNullable()
                    : base.VisitExtension(extensionExpression);
            }
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
                SelectExpression queryExpression, Dictionary<ProjectionMember, ProjectionMember> projectionMemberMappings)
            {
                _queryExpression = queryExpression;
                _projectionMemberMappings = projectionMemberMappings;
            }

            [return: NotNullIfNotNull("expression")]
            public override Expression? Visit(Expression? expression)
            {
                if (expression is ProjectionBindingExpression projectionBindingExpression)
                {
                    Check.DebugAssert(projectionBindingExpression.ProjectionMember != null,
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
                SelectExpression queryExpression, Dictionary<ProjectionMember, int> projectionMemberMappings)
            {
                _queryExpression = queryExpression;
                _projectionMemberMappings = projectionMemberMappings;
            }

            [return: NotNullIfNotNull("expression")]
            public override Expression? Visit(Expression? expression)
            {
                if (expression is ProjectionBindingExpression projectionBindingExpression)
                {
                    Check.DebugAssert(projectionBindingExpression.ProjectionMember != null,
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
                SelectExpression oldSelectExpression, SelectExpression newSelectExpression, int[] indexMap)
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
                    Check.DebugAssert(projectionBindingExpression.Index != null,
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

            public SqlRemappingVisitor(
                Dictionary<SqlExpression, ColumnExpression> mappings,
                SelectExpression subquery,
                TableReferenceExpression tableReferenceExpression)
            {
                _subquery = subquery;
                _tableReferenceExpression = tableReferenceExpression;
                _mappings = mappings;
            }

            [return: NotNullIfNotNull("sqlExpression")]
            public SqlExpression? Remap(SqlExpression? sqlExpression)
                => (SqlExpression?)Visit(sqlExpression);

            [return: NotNullIfNotNull("sqlExpression")]
            public SelectExpression? Remap(SelectExpression? sqlExpression)
                => (SelectExpression?)Visit(sqlExpression);

            [return: NotNullIfNotNull("expression")]
            public override Expression? Visit(Expression? expression)
            {
                switch (expression)
                {
                    case SqlExpression sqlExpression
                        when _mappings.TryGetValue(sqlExpression, out var outer):
                        return outer;

                    case ColumnExpression columnExpression
                        when _subquery.ContainsTableReference(columnExpression):
                        var outerColumn = _subquery.GenerateOuterColumn(_tableReferenceExpression, columnExpression);
                        _mappings[columnExpression] = outerColumn;
                        return outerColumn;

                    default:
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
                        var tableAlias = columnExpression.TableAlias!;
                        if (_columnReferenced!.ContainsKey(tableAlias))
                        {
                            if (_columnReferenced[tableAlias] == null)
                            {
                                _columnReferenced[tableAlias] = new HashSet<string>();
                            }

                            _columnReferenced[tableAlias]!.Add(columnExpression.Name);
                        }

                        // Always skip the table of ColumnExpression since it will traverse into deeper subquery
                        return columnExpression;

                    case LeftJoinExpression leftJoinExpression:
                        var leftJoinTableAlias = leftJoinExpression.Table.Alias!;
                        // Visiting the join predicate will add some columns for join table.
                        // But if all the referenced columns are in join predicate only then we can remove the join table.
                        // So if there are no referenced columns yet means there is still potential to remove this table,
                        // In such case we moved the columns encountered in join predicate to other dictionary and later merge
                        // if there are more references to the join table outside of join predicate.
                        // We currently do this only for LeftJoin since that is the only predicate join table we remove.
                        // We should also remove references to the outer if this column gets removed then that subquery can also remove projections
                        // But currently we only remove table for TPT scenario in which there are all table expressions which connects via joins.
                        var joinOnSameLevel = _columnReferenced!.ContainsKey(leftJoinTableAlias);
                        var noReferences = !joinOnSameLevel || _columnReferenced[leftJoinTableAlias] == null;
                        base.Visit(leftJoinExpression);
                        if (noReferences && joinOnSameLevel)
                        {
                            _columnsUsedInJoinCondition![leftJoinTableAlias] = _columnReferenced[leftJoinTableAlias];
                            _columnReferenced[leftJoinTableAlias] = null;
                        }

                        return leftJoinExpression;

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

            public int GetHashCode([DisallowNull] (ColumnExpression Column, ValueComparer Comparer) obj) => obj.Column.GetHashCode();
        }

        private sealed class AliasUniquefier : ExpressionVisitor
        {
            private readonly HashSet<string> _usedAliases;
            private readonly List<SelectExpression> _visitedSelectExpressions = new();

            public AliasUniquefier(HashSet<string> usedAliases)
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

            public override Type Type => typeof(object);

            public override ExpressionType NodeType => ExpressionType.Extension;
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
                => Alias.GetHashCode();
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
                    _ => true,
                };

            public ConcreteColumnExpression(
                string name, TableReferenceExpression table, Type type, RelationalTypeMapping typeMapping, bool nullable)
                : base(type, typeMapping)
            {
                Check.NotEmpty(name, nameof(name));
                Check.NotNull(table, nameof(table));
                Check.NotEmpty(table.Alias, $"{nameof(table)}.{nameof(table.Alias)}");

                Name = name;
                _table = table;
                IsNullable = nullable;
            }

            public override string Name { get; }

            public override TableExpressionBase Table => _table.Table;

            public override string TableAlias => _table.Alias;

            public override bool IsNullable { get; }

            /// <inheritdoc />
            protected override Expression VisitChildren(ExpressionVisitor visitor)
            {
                Check.NotNull(visitor, nameof(visitor));

                return this;
            }

            public override ConcreteColumnExpression MakeNullable()
                => new(Name, _table, Type, TypeMapping!, true);

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

                    throw new InvalidOperationException();
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
                            switch(table)
                            {
                                case PredicateJoinExpressionBase predicateJoinExpressionBase:
                                    Verify(predicateJoinExpressionBase.Table, _tableReferencesInScope);
                                    currentLevelTableReferences.Add(tableReference);
                                    Verify(predicateJoinExpressionBase.JoinPredicate,
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
                    // We ignore projection binding related elements as we don't want to copy them over for top level
                    // Nested level will have _projection populated and no binding elements
                    var newProjections = selectExpression._projection.Select(Visit).ToList<ProjectionExpression>();

                    var newTables = selectExpression._tables.Select(Visit).ToList<TableExpressionBase>();
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

                    var newSelectExpression = new SelectExpression(selectExpression.Alias, newProjections, newTables, newTableReferences, newGroupBy, newOrderings)
                    {
                        Predicate = predicate,
                        Having = havingExpression,
                        Offset = offset,
                        Limit = limit,
                        IsDistinct = selectExpression.IsDistinct,
                        Tags = selectExpression.Tags,
                        _usedAliases = selectExpression._usedAliases.ToHashSet()
                    };

                    newSelectExpression._tptLeftJoinTables.AddRange(selectExpression._tptLeftJoinTables);
                    // Since identifiers are ColumnExpression, they are not visited since they don't contain SelectExpression inside it.
                    newSelectExpression._identifier.AddRange(selectExpression._identifier);
                    newSelectExpression._childIdentifiers.AddRange(selectExpression._childIdentifiers);

                    // Remap tableReferences in new select expression
                    foreach (var tableReference in newTableReferences)
                    {
                        tableReference.UpdateTableReference(selectExpression, newSelectExpression);
                    }

                    // Now that we have SelectExpression, we visit all components and update table references inside columns
                    newSelectExpression = (SelectExpression)new ColumnExpressionReplacingExpressionVisitor(selectExpression, newSelectExpression)
                        .Visit(newSelectExpression);

                    return newSelectExpression;

                }

                return expression is ICloneable cloneable ? (Expression)cloneable.Clone() : base.Visit(expression);
            }
        }

        private sealed class ColumnExpressionReplacingExpressionVisitor : ExpressionVisitor
        {
            private readonly SelectExpression _oldSelectExpression;
            private readonly Dictionary<string, TableReferenceExpression> _newTableReferences;

            public ColumnExpressionReplacingExpressionVisitor(SelectExpression oldSelectExpression, SelectExpression newSelectExpression)
            {
                _oldSelectExpression = oldSelectExpression;
                _newTableReferences = newSelectExpression._tableReferences.ToDictionary(e => e.Alias);
            }

            [return: NotNullIfNotNull("expression")]
            public override Expression? Visit(Expression? expression)
            {
                return expression is ConcreteColumnExpression concreteColumnExpression
                    && _oldSelectExpression.ContainsTableReference(concreteColumnExpression)
                    ? new ConcreteColumnExpression(
                        concreteColumnExpression.Name,
                        _newTableReferences[concreteColumnExpression.TableAlias],
                        concreteColumnExpression.Type,
                        concreteColumnExpression.TypeMapping!,
                        concreteColumnExpression.IsNullable)
                    : base.Visit(expression);
            }
        }
    }
}
