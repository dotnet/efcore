// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
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
                    && _outerSelectExpression.ContainsTableReference(columnExpression.Table))
                {
                    _containsOuterReference = true;

                    return expression;
                }

                return base.Visit(expression);
            }
        }

        private sealed class ProjectionBindingExpressionRemappingExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _queryExpression;

            // Shifting PMs, converting PMs to index/indexMap
            private IDictionary<ProjectionMember, object>? _projectionMemberMappings;

            // Relocating index
            private int[]? _indexMap;

            // Shift pending collection offset
            private int? _pendingCollectionOffset;

            public ProjectionBindingExpressionRemappingExpressionVisitor(Expression queryExpression)
            {
                _queryExpression = queryExpression;
            }

            public Expression RemapProjectionMember(
                Expression expression,
                IDictionary<ProjectionMember, object> projectionMemberMappings,
                int pendingCollectionOffset = 0)
            {
                _projectionMemberMappings = projectionMemberMappings;
                _indexMap = null;
                _pendingCollectionOffset = pendingCollectionOffset;

                return Visit(expression);
            }

            public Expression RemapIndex(Expression expression, int[] indexMap, int pendingCollectionOffset = 0)
            {
                _projectionMemberMappings = null;
                _indexMap = indexMap;
                _pendingCollectionOffset = pendingCollectionOffset;

                return Visit(expression);
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                return extensionExpression switch
                {
                    ProjectionBindingExpression projectionBindingExpression => Remap(projectionBindingExpression),
                    CollectionShaperExpression collectionShaperExpression => Remap(collectionShaperExpression),
                    _ => base.VisitExtension(extensionExpression)
                };
            }

            private CollectionShaperExpression Remap(CollectionShaperExpression collectionShaperExpression)
                => new(
                    new ProjectionBindingExpression(
                        _queryExpression,
                        ((ProjectionBindingExpression)collectionShaperExpression.Projection).Index!.Value + (int)_pendingCollectionOffset!,
                        typeof(object)),
                    collectionShaperExpression.InnerShaper,
                    collectionShaperExpression.Navigation,
                    collectionShaperExpression.ElementType);

            private ProjectionBindingExpression Remap(ProjectionBindingExpression projectionBindingExpression)
            {
                if (_indexMap != null)
                {
                    if (projectionBindingExpression.Index is int index)
                    {
                        return CreateNewBinding(_indexMap[index], projectionBindingExpression.Type);
                    }

                    var indexMap = new Dictionary<IProperty, int>();
                    foreach (var item in projectionBindingExpression.IndexMap!)
                    {
                        indexMap[item.Key] = _indexMap[item.Value];
                    }

                    return CreateNewBinding(indexMap, projectionBindingExpression.Type);
                }

                var currentProjectionMember = projectionBindingExpression.ProjectionMember;
                var newBinding = _projectionMemberMappings![currentProjectionMember!];

                return CreateNewBinding(newBinding, projectionBindingExpression.Type);
            }

            private ProjectionBindingExpression CreateNewBinding(object binding, Type type)
                => binding switch
                {
                    ProjectionMember projectionMember => new ProjectionBindingExpression(
                        _queryExpression, projectionMember, type),

                    int index => new ProjectionBindingExpression(_queryExpression, index, type),

                    IDictionary<IProperty, int> indexMap => new ProjectionBindingExpression(_queryExpression, indexMap),

                    _ => throw new InvalidOperationException(),
                };
        }

        private sealed class SqlRemappingVisitor : ExpressionVisitor
        {
            private readonly SelectExpression _subquery;
            private readonly IDictionary<SqlExpression, ColumnExpression> _mappings;

            public SqlRemappingVisitor(IDictionary<SqlExpression, ColumnExpression> mappings, SelectExpression subquery)
            {
                _subquery = subquery;
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
                        when _subquery.ContainsTableReference(columnExpression.Table):
                        var index = _subquery.AddToProjection(columnExpression);
                        var projectionExpression = _subquery._projection[index];
                        return new ColumnExpression(projectionExpression, _subquery);

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
                        var tableAlias = columnExpression.Table.Alias!;
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
    }
}
