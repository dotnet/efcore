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

    private sealed class SelectExpressionCorrelationFindingExpressionVisitor(SelectExpression outerSelectExpression) : ExpressionVisitor
    {
        private bool _containsOuterReference;

        public bool ContainsOuterReference(SelectExpression selectExpression)
        {
            _containsOuterReference = false;

            Visit(selectExpression);

            return _containsOuterReference;
        }

        [return: NotNullIfNotNull(nameof(expression))]
        public override Expression? Visit(Expression? expression)
        {
            if (_containsOuterReference)
            {
                return expression;
            }

            if (expression is ColumnExpression columnExpression
                && outerSelectExpression.ContainsReferencedTable(columnExpression))
            {
                _containsOuterReference = true;

                return expression;
            }

            return base.Visit(expression);
        }
    }

    // #30915: shared state for a remapping visitor that needs to track which New/MemberInit shaper nodes it rebuilt
    // (old instance → new instance). Callers re-key any _nonEntityNullabilityMarkers entry whose key was one of the
    // rebuilt nodes, so a previously-recorded marker key does not go stale when a remap creates a fresh node instance.
    // Composed into visitors (rather than shared via a base class) because they derive from different bases. Lazily
    // allocated since most remaps rebuild nothing and most callers never read it.
    private sealed class RebuiltNodeTracker
    {
        private static readonly IReadOnlyDictionary<Expression, Expression> Empty
            = new Dictionary<Expression, Expression>(ReferenceEqualityComparer.Instance);

        private Dictionary<Expression, Expression>? _nodes;

        public IReadOnlyDictionary<Expression, Expression> Nodes
            => _nodes ?? Empty;

        public T Record<T>(T original, T visited)
            where T : Expression
        {
            if (!ReferenceEquals(visited, original))
            {
                (_nodes ??= new Dictionary<Expression, Expression>(ReferenceEqualityComparer.Instance))[original] = visited;
            }

            return visited;
        }
    }

    private sealed class ProjectionMemberRemappingExpressionVisitor(
        SelectExpression queryExpression,
        Dictionary<ProjectionMember, ProjectionMember> projectionMemberMappings)
        : ExpressionVisitor
    {
        private readonly RebuiltNodeTracker _tracker = new();

        public IReadOnlyDictionary<Expression, Expression> RebuiltNodes
            => _tracker.Nodes;

        protected override Expression VisitExtension(Expression expression)
        {
            if (expression is ProjectionBindingExpression projectionBindingExpression)
            {
                Check.DebugAssert(
                    projectionBindingExpression.ProjectionMember is not null,
                    "ProjectionBindingExpression must have projection member.");

                return new ProjectionBindingExpression(
                    queryExpression,
                    projectionMemberMappings[projectionBindingExpression.ProjectionMember],
                    projectionBindingExpression.Type);
            }

            return base.VisitExtension(expression);
        }

        protected override Expression VisitNew(NewExpression node)
            => _tracker.Record(node, (NewExpression)base.VisitNew(node));

        protected override Expression VisitMemberInit(MemberInitExpression node)
            => _tracker.Record(node, (MemberInitExpression)base.VisitMemberInit(node));
    }

    // #22517/#30915: rebuilds a grouping element's shaper against a correlated-subquery clone, tracking any
    // New/MemberInit nodes it rebuilds (old instance -> new instance) via RebuiltNodes. Used only through
    // SelectExpression.RemapGroupingElementShaper, which owns the propagate/visit/re-key protocol.
    private sealed class GroupingElementShaperRemappingExpressionVisitor(SelectExpression oldQuery, SelectExpression newQuery)
        : QueryExpressionReplacingExpressionVisitor(oldQuery, newQuery)
    {
        private readonly RebuiltNodeTracker _tracker = new();

        public IReadOnlyDictionary<Expression, Expression> RebuiltNodes
            => _tracker.Nodes;

        protected override Expression VisitNew(NewExpression node)
            => _tracker.Record(node, (NewExpression)base.VisitNew(node));

        protected override Expression VisitMemberInit(MemberInitExpression node)
            => _tracker.Record(node, (MemberInitExpression)base.VisitMemberInit(node));
    }

    // #22517/#30915: all logic that reads or writes _nonEntityNullabilityMarkers is gathered here (the field itself
    // lives with the other SelectExpression fields). The marker is transient build-time state that gates the whole-object
    // projection of a non-entity from the nullable side of an outer join to null on no-match rows; see the
    // _nonEntityNullabilityMarkers field comment for its lifecycle. AddJoin records/re-keys it, the grouping-element
    // subquery lowering propagates it across a clone, and the projection binder consults it.
    #region Non-entity nullability markers

    // true when this SelectExpression currently carries any non-entity nullability marker. Used to assert the invariant
    // that a non-grouping subquery clone never strands a live marker (Clone() deliberately drops markers).
    internal bool HasNonEntityNullabilityMarkers
        => _nonEntityNullabilityMarkers is { Count: > 0 };

    // Rebuilds a grouping element's shaper against `clone` (a correlated-subquery clone of this SelectExpression) while
    // carrying any recorded non-entity nullability marker across. Owns the whole protocol -- construct the remapper,
    // propagate markers before visiting, visit, re-key after visiting -- so callers cannot get the ordering wrong.
    // Internal infrastructure: only called within EFCore.Relational (ApplyGrouping and the grouping-element subquery
    // translation in RelationalQueryableMethodTranslatingExpressionVisitor), so it is not part of the public surface.
    internal Expression RemapGroupingElementShaper(SelectExpression clone, Expression shaperExpression)
    {
        var remapper = new GroupingElementShaperRemappingExpressionVisitor(this, clone);
        PropagateNonEntityNullabilityMarkersTo(clone, remapper);
        var rebuiltShaperExpression = remapper.Visit(shaperExpression);
        clone.RekeyPropagatedNonEntityNullabilityMarkers(remapper);
        return rebuiltShaperExpression;
    }

    private void PropagateNonEntityNullabilityMarkersTo(
        SelectExpression clone,
        GroupingElementShaperRemappingExpressionVisitor remapper)
    {
        // #22517/#30915: a grouping element's per-group correlated subquery is (re)bound against a clone of this
        // SelectExpression -- once by ApplyGrouping (the clone captured in the RelationalGroupByShaperExpression's
        // GroupingEnumerable), and again by RelationalQueryableMethodTranslatingExpressionVisitor when that clone is
        // itself cloned to translate a grouping-element subquery (e.g. `els.Select(...).FirstOrDefault()`). Any
        // non-entity nullability marker recorded on this SelectExpression (see _nonEntityNullabilityMarkers) needs
        // to be propagated onto the clone -- with its marker binding rebound through the same visitor used to
        // rebind the shaper's other ProjectionBindingExpressions -- otherwise the marker is stranded on an instance
        // nothing consults after the clone is made, and the whole-object null gate never fires.
        //
        // This method only copies the entry onto the clone (still keyed by the original shaper node) with a rebound
        // binding; re-keying onto the node the remapper rebuilds is done by the companion
        // RekeyPropagatedNonEntityNullabilityMarkers, called after the shaper has been visited.
        //
        // Fail-safe: only propagate an entry whose marker binding is a ProjectionBindingExpression that actually
        // resolves against this SelectExpression. If that check fails, skip the entry rather than copying a binding
        // that would resolve incorrectly (or not at all) against the clone; the gate then simply does not fire for
        // that entry and behavior degrades to the prior throw, never an incorrect result.
        //
        // This copies onto the clone and deliberately leaves this._nonEntityNullabilityMarkers intact (matching the
        // AddJoin design): the source's stale keys are shaper nodes it no longer projects, so a lookup against the
        // source cannot match them. Safe unless a future change re-visits the source's shaper against a rebuilt node.
        if (_nonEntityNullabilityMarkers is null)
        {
            return;
        }

        foreach (var (oldNode, markerBinding) in _nonEntityNullabilityMarkers)
        {
            if (markerBinding is ProjectionBindingExpression { QueryExpression: var markerQuery }
                && ReferenceEquals(markerQuery, this))
            {
                var reboundBinding = remapper.Visit(markerBinding);
                (clone._nonEntityNullabilityMarkers ??=
                    new Dictionary<Expression, Expression>(ReferenceEqualityComparer.Instance))[oldNode] = reboundBinding;
            }
        }
    }

    private void RekeyPropagatedNonEntityNullabilityMarkers(GroupingElementShaperRemappingExpressionVisitor remapper)
    {
        // Re-key any marker just propagated by PropagateNonEntityNullabilityMarkersTo (called on this
        // SelectExpression, the target of the propagation) whose shaper node was itself rebuilt by the element
        // remap -- this happens when the marker-bearing New/MemberInit node is nested inside the shaper root, e.g.
        // as a constructor argument of an outer projection -- so TryGetNonEntityNullabilityMarker still finds it
        // against its rebuilt identity.
        //
        // Unlike the AddJoin-branch re-key (see RekeyNonEntityNullabilityMarkersAfterOuterShaperRemap), this needs no
        // binding-resolution guard: the marker binding stored on the clone was already validated and rebound by
        // PropagateNonEntityNullabilityMarkersTo, so RemapNonEntityNullabilityMarker reuses it as-is. Do not add an
        // AddJoin-style guard here -- it would be redundant, and its absence is intentional, not an oversight.
        if (_nonEntityNullabilityMarkers is null)
        {
            return;
        }

        foreach (var oldNode in _nonEntityNullabilityMarkers.Keys.ToList())
        {
            if (remapper.RebuiltNodes.TryGetValue(oldNode, out var newNode))
            {
                RemapNonEntityNullabilityMarker(oldNode, newNode, _nonEntityNullabilityMarkers[oldNode]);
            }
        }
    }

    private void RekeyNonEntityNullabilityMarkersAfterOuterShaperRemap(
        ProjectionMemberRemappingExpressionVisitor outerRemapper,
        Dictionary<ProjectionMember, ProjectionMember> mapping)
    {
        // #30915: the outer remap in AddJoin rebuilds any New/MemberInit node whose projection bindings changed; a
        // node previously recorded as a nullability-marker key is now a stale instance. Re-key each such marker
        // onto its rebuilt node, re-binding the marker value through the same remap so it still resolves.
        // Only the (outer client-eval == false) AddJoin branch is covered; the other AddJoin branches that remap the
        // outer shaper are intentionally not re-keyed (no reachable repro). If one is ever hit with a live prior
        // marker, the fail-safe holds: the gate simply does not fire and behavior falls back to the prior throw
        // rather than producing an incorrect result. Tracked with the #30915 follow-ups.
        if (_nonEntityNullabilityMarkers is null)
        {
            return;
        }

        foreach (var oldNode in _nonEntityNullabilityMarkers.Keys.ToList())
        {
            // Re-key only when the key node was rebuilt AND the marker binding still resolves through this
            // remap. The marker column is not referenced by the shaper tree, so its projection member could in
            // principle have been pruned from _projectionMapping while the key node survived; in that case
            // skip the re-key rather than letting outerRemapper.Visit hit the throwing indexer
            // (ProjectionMemberRemappingExpressionVisitor.VisitExtension). Skipping preserves the fail-safe:
            // the gate does not fire and behavior degrades to the prior throw, never a KeyNotFoundException.
            var existingMarkerBinding = _nonEntityNullabilityMarkers[oldNode];
            if (outerRemapper.RebuiltNodes.TryGetValue(oldNode, out var newNode)
                && existingMarkerBinding is ProjectionBindingExpression { ProjectionMember: { } markerMember }
                && mapping.ContainsKey(markerMember))
            {
                var reboundBinding = outerRemapper.Visit(existingMarkerBinding);
                RemapNonEntityNullabilityMarker(oldNode, newNode, reboundBinding);
            }
        }
    }

    private void TryRecordNonEntityNullabilityMarker(Expression innerShaper, Expression? markerBinding)
    {
        // #30915: record the finalized inner-shaper node (post remap and post entity-nullable marking) against its remapped
        // marker binding, so the projection binder can later gate the whole inner object to null on no-match rows. Keyed on the
        // node instance because the binder receives this exact New/MemberInit node (member-folds return arguments by reference;
        // see ReplacingExpressionVisitor.VisitMember), and the inner shaper is left unwrapped so those folds keep working.
        if (markerBinding is not null && innerShaper is NewExpression or MemberInitExpression)
        {
            (_nonEntityNullabilityMarkers ??= new Dictionary<Expression, Expression>(ReferenceEqualityComparer.Instance))[innerShaper] =
                markerBinding;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public bool TryGetNonEntityNullabilityMarker(Expression shaper, [NotNullWhen(true)] out Expression? markerBinding)
    {
        // #30915: looks up the nullability marker recorded for a non-entity inner shaper of an outer join (see
        // _nonEntityNullabilityMarkers). The projection binder consults this when projecting the whole inner object, to gate it to
        // null on no-match rows.
        if (_nonEntityNullabilityMarkers is not null
            && _nonEntityNullabilityMarkers.TryGetValue(shaper, out markerBinding))
        {
            return true;
        }

        markerBinding = null;
        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public void RemapNonEntityNullabilityMarker(Expression oldShaper, Expression newShaper, Expression newMarkerBinding)
    {
        // #30915: the recorded non-entity inner-shaper node is keyed by reference, and its marker binding is a projection binding
        // valid only against the projection representation that existed when it was recorded. The TransparentIdentifier-rooted
        // projection-binding pass (RelationalProjectionBindingExpressionVisitor) rebuilds the inner node into the final projection
        // representation and rebinds its columns, leaving both the old node reference and the old marker binding stale. That pass
        // calls this to re-key the recorded entry onto the rebuilt node with a freshly-rebound marker, so the *final* whole-object
        // projection (a later pass over the same SelectExpression) still finds a valid node and marker binding to gate on.
        if (_nonEntityNullabilityMarkers is not null
            && _nonEntityNullabilityMarkers.ContainsKey(oldShaper))
        {
            _nonEntityNullabilityMarkers[newShaper] = newMarkerBinding;

            // Guard the (not-currently-reachable) self-reference case: if the rebuilt node is reference-equal to the old node,
            // the assignment above already updated the single entry in place; removing oldShaper would then delete it. Only drop
            // the stale entry when the node identity actually changed.
            if (!ReferenceEquals(oldShaper, newShaper))
            {
                _nonEntityNullabilityMarkers.Remove(oldShaper);
            }
        }
    }

    #endregion

    private sealed class ProjectionMemberToIndexConvertingExpressionVisitor(
        SelectExpression queryExpression,
        Dictionary<ProjectionMember, int> projectionMemberMappings)
        : ExpressionVisitor
    {
        [return: NotNullIfNotNull(nameof(expression))]
        public override Expression? Visit(Expression? expression)
        {
            if (expression is ProjectionBindingExpression projectionBindingExpression)
            {
                Check.DebugAssert(
                    projectionBindingExpression.ProjectionMember != null,
                    "ProjectionBindingExpression must have projection member.");

                return new ProjectionBindingExpression(
                    queryExpression,
                    projectionMemberMappings[projectionBindingExpression.ProjectionMember],
                    projectionBindingExpression.Type);
            }

            return base.Visit(expression);
        }
    }

    private sealed class ProjectionIndexRemappingExpressionVisitor(
        SelectExpression oldSelectExpression,
        SelectExpression newSelectExpression,
        int[] indexMap)
        : ExpressionVisitor
    {
        [return: NotNullIfNotNull(nameof(expression))]
        public override Expression? Visit(Expression? expression)
        {
            if (expression is ProjectionBindingExpression projectionBindingExpression
                && ReferenceEquals(projectionBindingExpression.QueryExpression, oldSelectExpression))
            {
                Check.DebugAssert(
                    projectionBindingExpression.Index != null,
                    "ProjectionBindingExpression must have index.");

                return new ProjectionBindingExpression(
                    newSelectExpression,
                    indexMap[projectionBindingExpression.Index.Value],
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
        private readonly HashSet<SqlExpression> _correlatedTerms = new(ReferenceEqualityComparer.Instance);
        private bool _groupByDiscovery = subquery._groupBy.Count > 0;

        [return: NotNullIfNotNull(nameof(sqlExpression))]
        public SqlExpression? Remap(SqlExpression? sqlExpression)
            => (SqlExpression?)Visit(sqlExpression);

        [return: NotNullIfNotNull(nameof(selectExpression))]
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

        [return: NotNullIfNotNull(nameof(expression))]
        public override Expression? Visit(Expression? expression)
        {
            switch (expression)
            {
                case SqlExpression sqlExpression
                    when mappings.TryGetValue(sqlExpression, out var outer):
                    return outer;

                case ColumnExpression columnExpression
                    when _groupByDiscovery && subquery.ContainsReferencedTable(columnExpression):
                    _correlatedTerms.Add(columnExpression);
                    return columnExpression;

                case SqlExpression sqlExpression
                    when !_groupByDiscovery
                    && sqlExpression is not SqlConstantExpression and not SqlParameterExpression
                    && _correlatedTerms.Contains(sqlExpression):
                    var outerColumn = subquery.GenerateOuterColumn(tableAlias, sqlExpression);
                    mappings[sqlExpression] = outerColumn;
                    return outerColumn;

                case ColumnExpression columnExpression
                    when !_groupByDiscovery && subquery.ContainsReferencedTable(columnExpression):
                    var outerColumn1 = subquery.GenerateOuterColumn(tableAlias, columnExpression);
                    mappings[columnExpression] = outerColumn1;
                    return outerColumn1;

                default:
                    return base.Visit(expression);
            }
        }

        private sealed class EnclosingTermFindingVisitor(HashSet<SqlExpression> correlatedTerms) : ExpressionVisitor
        {
            private bool _doesNotContainLocalTerms = true;

            [return: NotNullIfNotNull(nameof(expression))]
            public override Expression? Visit(Expression? expression)
            {
                if (expression is SqlExpression sqlExpression and not SqlFragmentExpression)
                {
                    if (correlatedTerms.Contains(sqlExpression)
                        || sqlExpression is SqlConstantExpression or SqlParameterExpression)
                    {
                        correlatedTerms.Add(sqlExpression);
                        return sqlExpression;
                    }

                    var parentDoesNotContainLocalTerms = _doesNotContainLocalTerms;
                    _doesNotContainLocalTerms = sqlExpression is not ColumnExpression;
                    base.Visit(expression);
                    if (_doesNotContainLocalTerms)
                    {
                        correlatedTerms.Add(sqlExpression);
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

    private readonly struct SingleCollectionInfo(
        Expression parentIdentifier,
        Expression outerIdentifier,
        Expression selfIdentifier,
        IReadOnlyList<ValueComparer> parentIdentifierValueComparers,
        IReadOnlyList<ValueComparer> outerIdentifierValueComparers,
        IReadOnlyList<ValueComparer> selfIdentifierValueComparers,
        Expression shaperExpression)
    {
        public Expression ParentIdentifier { get; } = parentIdentifier;
        public Expression OuterIdentifier { get; } = outerIdentifier;
        public Expression SelfIdentifier { get; } = selfIdentifier;
        public IReadOnlyList<ValueComparer> ParentIdentifierValueComparers { get; } = parentIdentifierValueComparers;
        public IReadOnlyList<ValueComparer> OuterIdentifierValueComparers { get; } = outerIdentifierValueComparers;
        public IReadOnlyList<ValueComparer> SelfIdentifierValueComparers { get; } = selfIdentifierValueComparers;
        public Expression ShaperExpression { get; } = shaperExpression;
    }

    private readonly struct SplitCollectionInfo(
        Expression parentIdentifier,
        Expression childIdentifier,
        IReadOnlyList<ValueComparer> identifierValueComparers,
        SelectExpression selectExpression,
        Expression shaperExpression)
    {
        public Expression ParentIdentifier { get; } = parentIdentifier;
        public Expression ChildIdentifier { get; } = childIdentifier;
        public IReadOnlyList<ValueComparer> IdentifierValueComparers { get; } = identifierValueComparers;
        public SelectExpression SelectExpression { get; } = selectExpression;
        public Expression ShaperExpression { get; } = shaperExpression;
    }

    private sealed class ClientProjectionRemappingExpressionVisitor(List<object> clientProjectionIndexMap) : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression expression)
        {
            switch (expression)
            {
                case ProjectionBindingExpression projectionBindingExpression:
                {
                    var value = clientProjectionIndexMap[projectionBindingExpression.Index!.Value];
                    return value switch
                    {
                        int intValue => new ProjectionBindingExpression(
                            projectionBindingExpression.QueryExpression, intValue, projectionBindingExpression.Type),

                        Expression innerShaper => Visit(innerShaper),

                        _ => throw new InvalidCastException()
                    };
                }

                case CollectionResultExpression
                {
                    QueryExpression: ProjectionBindingExpression innerProjectionBindingExpression
                } collectionResultExpression:
                {
                    var navigation = collectionResultExpression.StructuralProperty switch
                    {
                        INavigationBase n => n,
                        null or IComplexProperty => null,
                        _ => throw new UnreachableException()
                    };

                    var value = clientProjectionIndexMap[innerProjectionBindingExpression.Index!.Value];
                    return value switch
                    {
                        SingleCollectionInfo singleCollectionInfo
                            => new RelationalCollectionShaperExpression(
                                singleCollectionInfo.ParentIdentifier, singleCollectionInfo.OuterIdentifier,
                                singleCollectionInfo.SelfIdentifier, singleCollectionInfo.ParentIdentifierValueComparers,
                                singleCollectionInfo.OuterIdentifierValueComparers, singleCollectionInfo.SelfIdentifierValueComparers,
                                singleCollectionInfo.ShaperExpression, navigation,
                                collectionResultExpression.ElementType),

                        SplitCollectionInfo splitCollectionInfo
                            => new RelationalSplitCollectionShaperExpression(
                                splitCollectionInfo.ParentIdentifier, splitCollectionInfo.ChildIdentifier,
                                splitCollectionInfo.IdentifierValueComparers, splitCollectionInfo.SelectExpression,
                                splitCollectionInfo.ShaperExpression, navigation,
                                collectionResultExpression.ElementType),

                        int => collectionResultExpression.Update(
                            (ProjectionBindingExpression)Visit(innerProjectionBindingExpression)),

                        _ => throw new InvalidOperationException()
                    };
                }

                case RelationalGroupByResultExpression relationalGroupByResultExpression:
                    // Only element shaper needs remapping
                    return new RelationalGroupByResultExpression(
                        relationalGroupByResultExpression.KeyIdentifier,
                        relationalGroupByResultExpression.KeyIdentifierValueComparers,
                        relationalGroupByResultExpression.KeyShaper,
                        Visit(relationalGroupByResultExpression.ElementShaper));

                default:
                    return base.VisitExtension(expression);
            }
        }
    }

    // We sometimes clone when the result will be integrated in the same query tree (e.g. GroupBy - this needs to be reviewed and hopefully
    // improved); for those cases SqlAliasManager is passed in and ensures unique table aliases across the entire query.
    // But for split query, we clone in order to create a completely separate query, in which case we don't want unique aliases - and so
    // SqlAliasManager isn't passed in.
    private sealed class CloningExpressionVisitor(SqlAliasManager? sqlAliasManager, bool cloneClientProjections = true) : ExpressionVisitor
    {
        private readonly Dictionary<string, string> _tableAliasMap = new();

        [return: NotNullIfNotNull(nameof(expression))]
        public override Expression? Visit(Expression? expression)
        {
            switch (expression)
            {
                case ShapedQueryExpression shapedQuery:
                    return shapedQuery.UpdateQueryExpression(Visit(shapedQuery.QueryExpression));

                case TableExpressionBase table:
                {
                    var newTableAlias = table.Alias;
                    if (sqlAliasManager is not null && table.Alias is not null)
                    {
                        newTableAlias = sqlAliasManager.GenerateTableAlias(table.Alias);
                        _tableAliasMap[table.Alias] = newTableAlias;
                    }

                    return table is SelectExpression select
                        ? select.Clone(newTableAlias, this, cloneClientProjections)
                        : table.Clone(newTableAlias, this);
                }

                case ColumnExpression column when _tableAliasMap.TryGetValue(column.TableAlias, out var newTableAlias):
                    return new ColumnExpression(column.Name, newTableAlias, column.Column, column.Type, column.TypeMapping, column.IsNullable);

                default:
                    return base.Visit(expression);
            }
        }
    }

    private sealed class TpcTableExpressionRemovingExpressionVisitor(SqlAliasManager sqlAliasManager) : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression expression)
        {
            if (expression is not SelectExpression selectExpression)
            {
                return base.VisitExtension(expression);
            }

            // If selectExpression doesn't have any other component and only TPC tables then we can lift it
            // We ignore projection here because if this selectExpression has projection from inner TPC
            // Then TPC will have superset of projection
            var identitySelect = selectExpression is
                {
                    Tables: [TpcTablesExpression],
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

            TableExpressionBase[]? visitedTables = null;
            for (var i = 0; i < selectExpression.Tables.Count; i++)
            {
                var table = selectExpression.Tables[i];
                if (table.UnwrapJoin() is not TpcTablesExpression tpcTablesExpression)
                {
                    // Note that we don't visit non-TpcTablesExpressions - we'll be calling base.VisitExtension at the end.
                    if (visitedTables is not null)
                    {
                        visitedTables[i] = table;
                    }

                    continue;
                }

                if (visitedTables is null)
                {
                    visitedTables = new TableExpressionBase[selectExpression.Tables.Count];
                    for (var j = 0; j < i; j++)
                    {
                        visitedTables[j] = selectExpression.Tables[j];
                    }
                }

                var subSelectExpressions = tpcTablesExpression.Prune(tpcTablesExpression.DiscriminatorValues).SelectExpressions;
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
                    for (var j = 0; j < selectExpression.Projection.Count; j++)
                    {
                        var newIndex = innerProjections.FindIndex(e => string.Equals(
                            e, selectExpression.Projection[j].Alias, StringComparison.Ordinal));
                        if (newIndex == -1)
                        {
                            // If for whatever reason outer has additional projection which cannot be remapped we avoid lift
                            identitySelect = false;
                            reindexingMap = null;
                            break;
                        }

                        identityMap &= (j == newIndex);
                        reindexingMap[j] = newIndex;
                    }

                    if (identityMap)
                    {
                        // If projection is same on outer/inner we don't need remapping
                        reindexingMap = null;
                    }
                }

                RemapProjections(reindexingMap, firstSelectExpression);
                var result = subSelectExpressions[0];
                for (var j = 1; j < subSelectExpressions.Count; j++)
                {
                    var source1 = result;
                    var source2 = subSelectExpressions[j];
                    RemapProjections(reindexingMap, source2);

                    // Note that we give the same alias to the union as to the (final) wrapping SelectExpression below.
                    // In the end SQL, as this is a simple set operation, all select expressions get elided - but this still isn't ideal.
                    var unionExpression = new UnionExpression(tpcTablesExpression.Alias, source1, source2, distinct: false);
                    var projections = new List<ProjectionExpression>();
                    foreach (var projection in result.Projection)
                    {
                        projections.Add(
                            new ProjectionExpression(
                                CreateColumnExpression(projection, tpcTablesExpression.Alias), projection.Alias));
                    }

                    result = CreateImmutable(alias: null!, tables: [unionExpression], projections, sqlAliasManager);
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
                var resultTable = (TableExpressionBase)ReplacingExpressionVisitor.Replace(tpcTablesExpression, result, tpcTablesExpression);

                visitedTables[i] = table is JoinExpressionBase join
                    ? join.Update(resultTable)
                    : result;
            }

            return base.VisitExtension(
                selectExpression.Update(
                    visitedTables ?? selectExpression.Tables,
                    selectExpression.Predicate,
                    selectExpression.GroupBy,
                    selectExpression.Having,
                    selectExpression.Projection,
                    selectExpression.Orderings,
                    selectExpression.Offset,
                    selectExpression.Limit));
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
