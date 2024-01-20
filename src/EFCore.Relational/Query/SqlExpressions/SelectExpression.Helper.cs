// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

public sealed partial class SelectExpression
{
    private sealed class EntityShaperNullableMarkingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression extensionExpression)
            => extensionExpression is StructuralTypeShaperExpression shaper
                ? shaper.MakeNullable()
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
                && _outerSelectExpression.ContainsReferencedTable(columnExpression))
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

    private sealed class SqlRemappingVisitor(
        Dictionary<SqlExpression, ColumnExpression> mappings,
        SelectExpression subquery,
        string tableAlias)
        : ExpressionVisitor
    {
        private readonly SelectExpression _subquery = subquery;
        private readonly string _tableAlias = tableAlias;
        private readonly Dictionary<SqlExpression, ColumnExpression> _mappings = mappings;
        private readonly HashSet<SqlExpression> _correlatedTerms = new(ReferenceEqualityComparer.Instance);
        private bool _groupByDiscovery = subquery._groupBy.Count > 0;

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
                    when _groupByDiscovery && _subquery.ContainsReferencedTable(columnExpression):
                    _correlatedTerms.Add(columnExpression);
                    return columnExpression;

                case SqlExpression sqlExpression
                    when !_groupByDiscovery
                    && sqlExpression is not SqlConstantExpression and not SqlParameterExpression
                    && _correlatedTerms.Contains(sqlExpression):
                    var outerColumn = _subquery.GenerateOuterColumn(_tableAlias, sqlExpression);
                    _mappings[sqlExpression] = outerColumn;
                    return outerColumn;

                case ColumnExpression columnExpression
                    when !_groupByDiscovery && _subquery.ContainsReferencedTable(columnExpression):
                    var outerColumn1 = _subquery.GenerateOuterColumn(_tableAlias, columnExpression);
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

    private sealed class IdentifierComparer : IEqualityComparer<(ColumnExpression Column, ValueComparer Comparer)>
    {
        public bool Equals((ColumnExpression Column, ValueComparer Comparer) x, (ColumnExpression Column, ValueComparer Comparer) y)
            => x.Column.Equals(y.Column);

        public int GetHashCode((ColumnExpression Column, ValueComparer Comparer) obj)
            => obj.Column.GetHashCode();
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

    // We sometimes clone when the result will be integrated in the same query tree (e.g. GroupBy - this needs to be reviewed and hopefully
    // improved); for those cases SqlAliasManager is passed in and ensures unique table aliases across the entire query.
    // But for split query, we clone in order to create a completely separate query, in which case we don't want unique aliases - and so
    // SqlAliasManager isn't passed in.
    private sealed class CloningExpressionVisitor(SqlAliasManager? sqlAliasManager) : ExpressionVisitor
    {
        private readonly Dictionary<string, string> _tableAliasMap = new();

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            switch (expression)
            {
                case TableExpressionBase table:
                {
                    if (sqlAliasManager is null || table.Alias is null)
                    {
                        return table.Clone(table.Alias, this);
                    }

                    var newTableAlias = sqlAliasManager.GenerateTableAlias(table.Alias);
                    _tableAliasMap[table.Alias] = newTableAlias;
                    return table.Clone(newTableAlias, this);
                }

                case ColumnExpression column when _tableAliasMap.TryGetValue(column.TableAlias, out var newTableAlias):
                    return new ColumnExpression(column.Name, newTableAlias, column.Type, column.TypeMapping, column.IsNullable);

                default:
                    return base.Visit(expression);
            }
        }
    }

    private sealed class TpcTableExpressionRemovingExpressionVisitor : ExpressionVisitor
    {
        private readonly SqlAliasManager _sqlAliasManager;

        public TpcTableExpressionRemovingExpressionVisitor(SqlAliasManager sqlAliasManager)
        {
            _sqlAliasManager = sqlAliasManager;
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            if (expression is SelectExpression { _tpcDiscriminatorValues.Count: > 0 } selectExpression)
            {
                // If selectExpression doesn't have any other component and only TPC tables then we can lift it
                // We ignore projection here because if this selectExpression has projection from inner TPC
                // Then TPC will have superset of projection
                var identitySelect = selectExpression is
                    {
                        Tables.Count: 1,
                        Predicate: null,
                        Orderings: [],
                        Limit: null,
                        Offset: null,
                        IsDistinct: false,
                        GroupBy: [],
                        Having: null
                    }
                    // Any non-column projection means some composition which cannot be removed
                    && selectExpression.Projection.All(e => e.Expression is ColumnExpression);

                foreach (var (tpcTablesExpression, (_, discriminatorValues)) in selectExpression._tpcDiscriminatorValues)
                {
                    var subSelectExpressions = tpcTablesExpression.Prune(discriminatorValues).SelectExpressions;
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

                    RemapProjections(reindexingMap, firstSelectExpression);
                    var result = subSelectExpressions[0];
                    for (var i = 1; i < subSelectExpressions.Count; i++)
                    {
                        var source1 = result;
                        var source2 = subSelectExpressions[i];
                        RemapProjections(reindexingMap, source2);
                        var generatedSelectExpression = new SelectExpression(alias: null, _sqlAliasManager);

                        var setOperationAlias = _sqlAliasManager.GenerateTableAlias("union");
                        var unionExpression = new UnionExpression(setOperationAlias, source1, source2, distinct: false);
                        generatedSelectExpression._tables.Add(unionExpression);
                        foreach (var projection in result.Projection)
                        {
                            generatedSelectExpression._projection.Add(
                                new ProjectionExpression(
                                    CreateColumnExpression(projection, setOperationAlias), projection.Alias));
                        }

                        generatedSelectExpression.IsMutable = false;
                        result = generatedSelectExpression;
                    }

                    if (identitySelect)
                    {
                        if (selectExpression.Alias == null)
                        {
                            // If top-level them copy over bindings for shaper
                            result._projectionMapping = selectExpression._projectionMapping;
                            result._clientProjections = selectExpression._clientProjections;
                        }
                        else
                        {
                            result = result.WithAlias(selectExpression.Alias);
                        }

                        // Since identity select implies only 1 table so we can return without worrying about another iteration.
                        // Identity select shouldn't require base visit.
                        return result;
                    }

                    result = result.WithAlias(tpcTablesExpression.Alias);
                    var tableIndex =
                        selectExpression._tables.FindIndex(teb => ReferenceEquals(teb.UnwrapJoin(), tpcTablesExpression));
                    var table = selectExpression._tables[tableIndex];
                    selectExpression._tables[tableIndex] = (TableExpressionBase)ReplacingExpressionVisitor.Replace(
                        tpcTablesExpression, result, table);
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
