// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class SelectExpression : TableExpressionBase
    {
        private IDictionary<ProjectionMember, Expression> _projectionMapping = new Dictionary<ProjectionMember, Expression>();

        private readonly IDictionary<Expression, ProjectionBindingExpression> _projectionCache
            = new Dictionary<Expression, ProjectionBindingExpression>();
        private readonly List<TableExpressionBase> _tables = new List<TableExpressionBase>();
        private readonly List<ProjectionExpression> _projection = new List<ProjectionExpression>();
        private readonly List<OrderingExpression> _orderings = new List<OrderingExpression>();
        private readonly List<SqlExpression> _identifyingProjection = new List<SqlExpression>();
        private readonly List<(List<SqlExpression> outerKey, List<SqlExpression> innerKey, SelectExpression innerSelectExpression)> _pendingCollectionJoins
            = new List<(List<SqlExpression> outerKey, List<SqlExpression> innerKey, SelectExpression innerSelectExpression)>();

        public IReadOnlyList<ProjectionExpression> Projection => _projection;
        public IReadOnlyList<TableExpressionBase> Tables => _tables;
        public IReadOnlyList<OrderingExpression> Orderings => _orderings;
        public SqlExpression Predicate { get; private set; }
        public SqlExpression Limit { get; private set; }
        public SqlExpression Offset { get; private set; }
        public bool IsDistinct { get; private set; }

        internal SelectExpression(
            string alias,
            List<ProjectionExpression> projections,
            List<TableExpressionBase> tables,
            List<OrderingExpression> orderings)
            : base(alias ?? "")
        {
            _projection = projections;
            _tables = tables;
            _orderings = orderings;
        }

        internal SelectExpression(IEntityType entityType)
            : base("")
        {
            var tableExpression = new TableExpression(
                entityType.GetTableName(),
                entityType.GetSchema(),
                entityType.GetTableName().ToLower().Substring(0, 1));

            _tables.Add(tableExpression);

            var entityProjection = new EntityProjectionExpression(entityType, tableExpression, false);
            _projectionMapping[new ProjectionMember()] = entityProjection;

            if (entityType.FindPrimaryKey() != null)
            {
                foreach (var property in entityType.FindPrimaryKey().Properties)
                {
                    _identifyingProjection.Add(entityProjection.GetProperty(property));
                }
            }
        }

        public SelectExpression(IEntityType entityType, string sql, Expression arguments)
            : base("")
        {
            var fromSqlExpression = new FromSqlExpression(
                sql,
                arguments,
                entityType.GetTableName().ToLower().Substring(0, 1));

            _tables.Add(fromSqlExpression);

            _projectionMapping[new ProjectionMember()] = new EntityProjectionExpression(entityType, fromSqlExpression, false);
        }

        public bool IsNonComposedFromSql()
        {
            return Limit == null
                && Offset == null
                && !IsDistinct
                && Predicate == null
                && Orderings.Count == 0
                && Tables.Count == 1
                && Tables[0] is FromSqlExpression fromSql
                && Projection.All(pe => pe.Expression is ColumnExpression column ? ReferenceEquals(column.Table, fromSql) : false);
        }

        public SqlExpression BindProperty(Expression projectionExpression, IProperty property)
        {
            var member = (projectionExpression as ProjectionBindingExpression).ProjectionMember;

            return ((EntityProjectionExpression)_projectionMapping[member]).GetProperty(property);
        }

        public void ApplyProjection()
        {
            if (Projection.Any())
            {
                return;
            }

            var result = new Dictionary<ProjectionMember, Expression>();
            foreach (var keyValuePair in _projectionMapping)
            {
                if (keyValuePair.Value is EntityProjectionExpression entityProjection)
                {
                    var map = new Dictionary<IProperty, int>();
                    foreach (var property in entityProjection.EntityType
                        .GetDerivedTypesInclusive().SelectMany(e => e.GetDeclaredProperties()))
                    {
                        var columnExpression = entityProjection.GetProperty(property);
                        map[property] = _projection.Count;
                        _projection.Add(new ProjectionExpression(columnExpression, alias: ""));
                    }
                    result[keyValuePair.Key] = Constant(map);
                }
                else
                {
                    result[keyValuePair.Key] = Constant(_projection.Count);
                    _projection.Add(new ProjectionExpression((SqlExpression)keyValuePair.Value, alias: ""));
                }
            }

            _projectionMapping = result;
        }

        public void ReplaceProjection(IDictionary<ProjectionMember, Expression> projectionMapping)
        {
            _projectionMapping.Clear();

            foreach (var kvp in projectionMapping)
            {
                _projectionMapping[kvp.Key] = kvp.Value;
            }
        }

        public Expression GetProjectionExpression(ProjectionMember projectionMember)
        {
            return _projectionMapping[projectionMember];
        }

        public ProjectionBindingExpression AddToProjection(SqlExpression sqlExpression, Type type)
        {
            if (!_projectionCache.TryGetValue(sqlExpression, out var result))
            {
                _projection.Add(new ProjectionExpression(sqlExpression, alias: ""));
                result = new ProjectionBindingExpression(this, _projection.Count - 1, type);
                _projectionCache[sqlExpression] = result;
            }

            return result;
        }

        public ProjectionBindingExpression AddToProjection(ProjectionBindingExpression projectionBindingExpression)
        {
            var entityProjection = (EntityProjectionExpression)_projectionMapping[projectionBindingExpression.ProjectionMember];
            if (!_projectionCache.TryGetValue(entityProjection, out var result))
            {
                var map = new Dictionary<IProperty, int>();
                foreach (var property in entityProjection.EntityType
                    .GetDerivedTypesInclusive().SelectMany(e => e.GetDeclaredProperties()))
                {
                    var columnExpression = entityProjection.GetProperty(property);
                    map[property] = _projection.Count;
                    _projection.Add(new ProjectionExpression(columnExpression, alias: ""));
                }

                result = new ProjectionBindingExpression(this, map);
                _projectionCache[entityProjection] = result;
            }

            return result;
        }

        public void ApplyPredicate(SqlExpression expression)
        {
            if (expression is SqlConstantExpression sqlConstant
                && (bool)sqlConstant.Value)
            {
                return;
            }

            if (Predicate == null)
            {
                Predicate = expression;
            }
            else
            {
                Predicate = new SqlBinaryExpression(
                    ExpressionType.AndAlso,
                    Predicate,
                    expression,
                    typeof(bool),
                    expression.TypeMapping);
            }
        }

        public override ExpressionType NodeType => ExpressionType.Extension;


        public void ApplyOrdering(OrderingExpression orderingExpression)
        {
            _orderings.Clear();
            _orderings.Add(orderingExpression);
        }

        public void AppendOrdering(OrderingExpression orderingExpression)
        {
            if (_orderings.FirstOrDefault(o => o.Expression.Equals(orderingExpression.Expression)) == null)
            {
                _orderings.Add(orderingExpression);
            }
        }

        public void ApplyLimit(SqlExpression sqlExpression)
        {
            Limit = sqlExpression;
        }

        public void ApplyOffset(SqlExpression sqlExpression)
        {
            if (Limit != null
                || Offset != null)
            {
                PushdownIntoSubQuery();
            }

            Offset = sqlExpression;
        }

        public void ReverseOrderings()
        {
            var existingOrdering = _orderings.ToArray();

            _orderings.Clear();

            for (var i = 0; i < existingOrdering.Length; i++)
            {
                _orderings.Add(
                    new OrderingExpression(
                        existingOrdering[i].Expression,
                        !existingOrdering[i].Ascending));
            }
        }

        public void ApplyDistinct()
        {
            if (Limit != null
                || Offset != null)
            {
                PushdownIntoSubQuery();
            }

            IsDistinct = true;
            ClearOrdering();
        }

        public void ClearOrdering()
        {
            _orderings.Clear();
        }

        private SelectExpression Clone(string alias)
        {
            var projectionMapping = new Dictionary<ProjectionMember, Expression>();
            foreach (var kvp in _projectionMapping)
            {
                projectionMapping[kvp.Key] = kvp.Value;
            }

            return new SelectExpression(alias, _projection.ToList(), _tables.ToList(), _orderings.ToList())
            {
                _projectionMapping = projectionMapping,
                Predicate = Predicate,
                Offset = Offset,
                Limit = Limit,
                IsDistinct = IsDistinct
            };
        }

        private string GenerateUniqueName(HashSet<string> usedNames, string prefix)
        {
            if (!usedNames.Contains(prefix))
            {
                return prefix;
            }

            var counter = 0;
            var uniqueName = prefix + counter;
            while (usedNames.Contains(uniqueName))
            {
                uniqueName = prefix + counter++;
            }

            return uniqueName;
        }

        public SelectExpression PushdownIntoSubQuery()
        {
            var subquery = Clone("t");

            if (subquery.Limit == null && subquery.Offset == null)
            {
                subquery.ClearOrdering();
            }

            _projectionMapping.Clear();
            var projectionMap = new Dictionary<SqlExpression, ColumnExpression>();
            var usedNames = new HashSet<string>();
            if (_projection.Any())
            {
                _projection.Clear();
                var subqueryProjection = subquery._projection.ToList();
                subquery._projection.Clear();
                foreach (var projection in subqueryProjection)
                {
                    var innerColumn = projection.Expression;
                    var name = GenerateUniqueName(usedNames, innerColumn is ColumnExpression column ? column.Name : "c");
                    usedNames.Add(name);
                    var projectionExpression = new ProjectionExpression(innerColumn, name);
                    subquery._projection.Add(projectionExpression);
                    var outerColumn = new ColumnExpression(projectionExpression, subquery, IsNullableProjection(projectionExpression));
                    _projection.Add(new ProjectionExpression(outerColumn, alias: ""));
                    projectionMap[innerColumn] = outerColumn;
                }
            }
            else
            {
                foreach (var projection in subquery._projectionMapping)
                {
                    if (projection.Value is EntityProjectionExpression entityProjection)
                    {
                        var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();
                        foreach (var property in entityProjection.EntityType
                            .GetDerivedTypesInclusive().SelectMany(e => e.GetDeclaredProperties()))
                        {
                            var innerColumn = entityProjection.GetProperty(property);
                            var name = GenerateUniqueName(usedNames, innerColumn.Name);
                            usedNames.Add(name);
                            var projectionExpression = new ProjectionExpression(innerColumn, name);
                            subquery._projection.Add(projectionExpression);
                            var outerColumn = new ColumnExpression(projectionExpression, subquery, innerColumn.Nullable);
                            propertyExpressions[property] = outerColumn;
                            projectionMap[innerColumn] = outerColumn;
                        }

                        _projectionMapping[projection.Key] = new EntityProjectionExpression(
                            entityProjection.EntityType, propertyExpressions);
                    }
                    else
                    {
                        var innerColumn = (SqlExpression)projection.Value;
                        var name = GenerateUniqueName(usedNames, "c");
                        usedNames.Add(name);
                        var projectionExpression = new ProjectionExpression(innerColumn, name);
                        subquery._projection.Add(projectionExpression);
                        var outerColumn = new ColumnExpression(
                            projectionExpression, subquery, IsNullableProjection(projectionExpression));
                        _projectionMapping[projection.Key] = outerColumn;
                        projectionMap[innerColumn] = outerColumn;
                    }
                }

                subquery._projectionMapping = null;
            }

            var identifyingProjection = _identifyingProjection.ToList();
            _identifyingProjection.Clear();
            foreach (var projection in identifyingProjection)
            {
                // TODO: See issue#15873
                if (projectionMap.TryGetValue(projection, out var column))
                {
                    _identifyingProjection.Add(column);
                }
            }

            var currentOrderings = _orderings.ToList();
            _orderings.Clear();
            foreach (var ordering in currentOrderings)
            {
                var orderingExpression = ordering.Expression;
                if (projectionMap.TryGetValue(orderingExpression, out var outerColumn))
                {
                    _orderings.Add(new OrderingExpression(outerColumn, ordering.Ascending));
                }
                else
                {
                    var name = GenerateUniqueName(usedNames, "c");
                    usedNames.Add(name);
                    var projectionExpression = new ProjectionExpression(ordering.Expression, name);
                    subquery._projection.Add(projectionExpression);
                    _orderings.Add(new OrderingExpression(
                        new ColumnExpression(projectionExpression, subquery, IsNullableProjection(projectionExpression)), ordering.Ascending));

                }
            }

            Offset = null;
            Limit = null;
            IsDistinct = false;
            Predicate = null;
            _tables.Clear();
            _tables.Add(subquery);

            return subquery;
        }

        private static bool IsNullableProjection(ProjectionExpression projection)
        {
            return projection.Expression is ColumnExpression column ? column.Nullable : true;
        }

        public CollectionShaperExpression AddCollectionProjection(ShapedQueryExpression shapedQueryExpression, INavigation navigation)
        {
            var innerSelectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;
            _pendingCollectionJoins.Add(
                (GetIdentifyingProjection(),
                innerSelectExpression.GetIdentifyingProjection(),
                innerSelectExpression));

            return new CollectionShaperExpression(
                new ProjectionBindingExpression(this, _pendingCollectionJoins.Count - 1, typeof(object)),
                shapedQueryExpression.ShaperExpression,
                navigation);
        }

        public RelationalCollectionShaperExpression ApplyCollectionJoin(int collectionId, Expression shaperExpression, INavigation navigation)
        {
            var snapshot = _pendingCollectionJoins[collectionId];
            var outerKey = ConvertKeyExpressions(snapshot.outerKey);
            var innerSelectExpression = snapshot.innerSelectExpression;
            innerSelectExpression.ApplyProjection();
            var innerKey = innerSelectExpression.ConvertKeyExpressions(snapshot.innerKey);
            var boolTypeMapping = innerSelectExpression.Predicate.TypeMapping;
            foreach (var orderingKey in snapshot.outerKey)
            {
                AppendOrdering(new OrderingExpression(orderingKey, ascending: true));
            }

            if (collectionId > 0)
            {
                foreach (var orderingKey in _pendingCollectionJoins[collectionId - 1].innerKey)
                {
                    AppendOrdering(new OrderingExpression(orderingKey, ascending: true));
                }

                outerKey = ConvertKeyExpressions(snapshot.outerKey.Concat(_pendingCollectionJoins[collectionId - 1].innerKey).ToList());
            }

            var (outer, inner) = TryExtractJoinKey(innerSelectExpression);
            if (outer != null)
            {
                if (IsDistinct
                   || Limit != null
                   || Offset != null)
                {
                    var subquery = PushdownIntoSubQuery();
                    outer = LiftFromSubquery(subquery, outer);
                }

                if (innerSelectExpression.Offset != null
                    || innerSelectExpression.Limit != null
                    || innerSelectExpression.IsDistinct
                    || innerSelectExpression.Predicate != null
                    || innerSelectExpression.Tables.Count > 1)
                {
                    var subquery = innerSelectExpression.PushdownIntoSubQuery();
                    inner = LiftFromSubquery(subquery, inner);
                }

                var leftJoinExpression = new LeftJoinExpression(innerSelectExpression.Tables.Single(),
                    new SqlBinaryExpression(ExpressionType.Equal, outer, inner, typeof(bool), boolTypeMapping));
                _tables.Add(leftJoinExpression);
                var indexOffset = _projection.Count;
                foreach (var projection in innerSelectExpression.Projection)
                {
                    var projectionToAdd = projection.Expression;
                    if (projectionToAdd is ColumnExpression column)
                    {
                        projectionToAdd = column.MakeNullable();
                    }
                    _projection.Add(projection.Update(projectionToAdd));
                }

                var shaperRemapper = new ShaperRemappingExpressionVisitor(this, innerSelectExpression, indexOffset);
                var innerShaper = shaperRemapper.Visit(shaperExpression);
                innerKey = shaperRemapper.Visit(innerKey);

                return new RelationalCollectionShaperExpression(
                    collectionId,
                    outerKey,
                    innerKey,
                    innerShaper,
                    navigation);

            }

            throw new NotImplementedException();
        }

        private Expression ConvertKeyExpressions(List<SqlExpression> keyExpressions)
        {
            var updatedExpressions = new List<Expression>();
            foreach (var keyExpression in keyExpressions)
            {
                var index = _projection.FindIndex(pe => pe.Expression.Equals(keyExpression));
                if (index == -1)
                {
                    index = _projection.Count;
                    _projection.Add(new ProjectionExpression(keyExpression, alias: ""));
                }

                var projectionBindingExpression = new ProjectionBindingExpression(this, index, keyExpression.Type);

                updatedExpressions.Add(
                    projectionBindingExpression.Type.IsValueType
                    ? Convert(projectionBindingExpression, typeof(object))
                    : (Expression)projectionBindingExpression);
            }

            return NewArrayInit(
                typeof(object),
                updatedExpressions);
        }

        private List<SqlExpression> GetIdentifyingProjection()
        {
            return _identifyingProjection.ToList();
        }

        private class ShaperRemappingExpressionVisitor : ExpressionVisitor
        {
            private readonly SelectExpression _queryExpression;
            private readonly SelectExpression _innerSelectExpression;
            private readonly int _offset;

            public ShaperRemappingExpressionVisitor(SelectExpression queryExpression, SelectExpression innerSelectExpression, int offset)
            {
                _queryExpression = queryExpression;
                _innerSelectExpression = innerSelectExpression;
                _offset = offset;
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                if (extensionExpression is ProjectionBindingExpression projectionBindingExpression)
                {
                    var oldIndex = (int)GetProjectionIndex(projectionBindingExpression);

                    return new ProjectionBindingExpression(_queryExpression, oldIndex + _offset, projectionBindingExpression.Type);
                }

                if (extensionExpression is EntityShaperExpression entityShaper)
                {
                    var oldIndexMap = (IDictionary<IProperty, int>)GetProjectionIndex(entityShaper.ValueBufferExpression);
                    var indexMap = new Dictionary<IProperty, int>();
                    foreach (var keyValuePair in oldIndexMap)
                    {
                        indexMap[keyValuePair.Key] = keyValuePair.Value + _offset;
                    }

                    return new EntityShaperExpression(
                        entityShaper.EntityType,
                        new ProjectionBindingExpression(_queryExpression, indexMap),
                        nullable: true);
                }

                return base.VisitExtension(extensionExpression);
            }

            private object GetProjectionIndex(ProjectionBindingExpression projectionBindingExpression)
            {
                return projectionBindingExpression.ProjectionMember != null
                    ? ((ConstantExpression)_innerSelectExpression.GetProjectionExpression(projectionBindingExpression.ProjectionMember)).Value
                    : (projectionBindingExpression.Index != null
                        ? (object)projectionBindingExpression.Index
                        : projectionBindingExpression.IndexMap);
            }
        }

        private (SqlExpression outer, SqlExpression inner) TryExtractJoinKey(SelectExpression inner)
        {
            if (inner.Predicate is SqlBinaryExpression sqlBinaryExpression)
            {
                // TODO: Handle composite key case
                var keyComparison = ValidateKeyComparison(inner, sqlBinaryExpression);
                if (keyComparison.outer != null)
                {
                    inner.Predicate = null;
                    return keyComparison;
                }
            }

            return (null, null);
        }

        private (SqlExpression outer, SqlExpression inner) ValidateKeyComparison(SelectExpression inner, SqlBinaryExpression sqlBinaryExpression)
        {
            if (sqlBinaryExpression.OperatorType == ExpressionType.Equal)
            {
                if (sqlBinaryExpression.Left is ColumnExpression leftColumn
                    && sqlBinaryExpression.Right is ColumnExpression rightColumn)
                {
                    if (ContainsTableReference(this, leftColumn.Table)
                        && ContainsTableReference(inner, rightColumn.Table))
                    {
                        return (leftColumn, rightColumn);
                    }

                    if (ContainsTableReference(this, rightColumn.Table)
                        && ContainsTableReference(inner, leftColumn.Table))
                    {
                        return (rightColumn, leftColumn);
                    }
                }
            }

            return (null, null);
        }

        private static bool ContainsTableReference(SelectExpression selectExpression, TableExpressionBase table)
        {
            return selectExpression.Tables.Any(te => ReferenceEquals(te is JoinExpressionBase jeb ? jeb.Table : te, table));
        }

        private ColumnExpression LiftFromSubquery(SelectExpression subquery, SqlExpression column)
        {
            var subqueryProjection = subquery._projection.Single(pe => pe.Expression.Equals(column));

            return new ColumnExpression(subqueryProjection, subquery, IsNullableProjection(subqueryProjection));
        }

        public void AddInnerJoin(SelectExpression innerSelectExpression, SqlExpression joinPredicate, Type transparentIdentifierType)
        {
            _identifyingProjection.AddRange(innerSelectExpression._identifyingProjection);
            var joinTable = new InnerJoinExpression(innerSelectExpression.Tables.Single(), joinPredicate);
            _tables.Add(joinTable);

            var outerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer");
            var projectionMapping = new Dictionary<ProjectionMember, Expression>();
            foreach (var projection in _projectionMapping)
            {
                projectionMapping[projection.Key.ShiftMember(outerMemberInfo)] = projection.Value;
            }

            var innerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner");
            foreach (var projection in innerSelectExpression._projectionMapping)
            {
                projectionMapping[projection.Key.ShiftMember(innerMemberInfo)] = projection.Value;
            }

            _projectionMapping = projectionMapping;
        }

        public void AddLeftJoin(SelectExpression innerSelectExpression, SqlExpression joinPredicate, Type transparentIdentifierType)
        {
            var joinTable = new LeftJoinExpression(innerSelectExpression.Tables.Single(), joinPredicate);
            _tables.Add(joinTable);

            var outerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer");
            var projectionMapping = new Dictionary<ProjectionMember, Expression>();
            foreach (var projection in _projectionMapping)
            {
                projectionMapping[projection.Key.ShiftMember(outerMemberInfo)] = projection.Value;
            }

            var innerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner");
            foreach (var projection in innerSelectExpression._projectionMapping)
            {
                var projectionToAdd = projection.Value;
                if (projectionToAdd is EntityProjectionExpression entityProjection)
                {
                    projectionToAdd = entityProjection.MakeNullable();
                }
                else if (projectionToAdd is ColumnExpression column)
                {
                    projectionToAdd = column.MakeNullable();
                }

                projectionMapping[projection.Key.ShiftMember(innerMemberInfo)] = projectionToAdd;
            }

            _projectionMapping = projectionMapping;
        }

        public void AddCrossJoin(SelectExpression innerSelectExpression, Type transparentIdentifierType)
        {
            _identifyingProjection.AddRange(innerSelectExpression._identifyingProjection);
            var joinTable = new CrossJoinExpression(innerSelectExpression.Tables.Single());
            _tables.Add(joinTable);

            var outerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer");
            var projectionMapping = new Dictionary<ProjectionMember, Expression>();
            foreach (var projection in _projectionMapping)
            {
                projectionMapping[projection.Key.ShiftMember(outerMemberInfo)] = projection.Value;
            }

            var innerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner");
            foreach (var projection in innerSelectExpression._projectionMapping)
            {
                projectionMapping[projection.Key.ShiftMember(innerMemberInfo)] = projection.Value;
            }

            _projectionMapping = projectionMapping;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var changed = false;

            var projections = new List<ProjectionExpression>();
            IDictionary<ProjectionMember, Expression> projectionMapping;
            if (Projection.Any())
            {
                projectionMapping = _projectionMapping;
                foreach (var item in Projection)
                {
                    var projection = (ProjectionExpression)visitor.Visit(item);
                    projections.Add(projection);

                    changed |= projection != item;
                }
            }
            else
            {
                projectionMapping = new Dictionary<ProjectionMember, Expression>();
                foreach (var mapping in _projectionMapping)
                {
                    var newProjection = visitor.Visit(mapping.Value);
                    changed |= newProjection != mapping.Value;

                    projectionMapping[mapping.Key] = newProjection;
                }
            }

            var tables = new List<TableExpressionBase>();
            foreach (var table in _tables)
            {
                var newTable = (TableExpressionBase)visitor.Visit(table);
                changed |= newTable != table;
                tables.Add(newTable);
            }

            var predicate = (SqlExpression)visitor.Visit(Predicate);
            changed |= predicate != Predicate;

            var orderings = new List<OrderingExpression>();
            foreach (var ordering in _orderings)
            {
                var orderingExpression = (SqlExpression)visitor.Visit(ordering.Expression);
                changed |= orderingExpression != ordering.Expression;
                orderings.Add(ordering.Update(orderingExpression));
            }

            var offset = (SqlExpression)visitor.Visit(Offset);
            changed |= offset != Offset;

            var limit = (SqlExpression)visitor.Visit(Limit);
            changed |= limit != Limit;

            if (changed)
            {
                var newSelectExpression = new SelectExpression(Alias, projections, tables, orderings)
                {
                    _projectionMapping = projectionMapping,
                    Predicate = predicate,
                    Offset = offset,
                    Limit = limit,
                    IsDistinct = IsDistinct
                };

                return newSelectExpression;
            }

            return this;
        }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SelectExpression selectExpression
                    && Equals(selectExpression));

        private bool Equals(SelectExpression selectExpression)
        {
            if (!base.Equals(selectExpression))
            {
                return false;
            }

            if (_projectionMapping != null
                && selectExpression._projectionMapping != null)
            {
                foreach (var projectionMapping in _projectionMapping)
                {
                    if (!selectExpression._projectionMapping.TryGetValue(projectionMapping.Key, out var projection))
                    {
                        return false;
                    }

                    if (!projectionMapping.Value.Equals(projection))
                    {
                        return false;
                    }
                }
            }

            if (!_tables.SequenceEqual(selectExpression._tables))
            {
                return false;
            }

            if (!(Predicate == null && selectExpression.Predicate == null
                || Predicate != null && Predicate.Equals(selectExpression.Predicate)))
            {
                return false;
            }

            if (!_orderings.SequenceEqual(selectExpression._orderings))
            {
                return false;
            }

            if (!(Offset == null && selectExpression.Offset == null
                || Offset != null && Offset.Equals(selectExpression.Offset)))
            {
                return false;
            }

            if (!(Limit == null && selectExpression.Limit == null
                || Limit != null && Limit.Equals(selectExpression.Limit)))
            {
                return false;
            }

            return IsDistinct == selectExpression.IsDistinct;
        }

        public SelectExpression Update(
            List<ProjectionExpression> projections,
            List<TableExpressionBase> tables,
            SqlExpression predicate,
            List<OrderingExpression> orderings,
            SqlExpression limit,
            SqlExpression offset,
            bool distinct,
            string alias)
        {
            var projectionMapping = new Dictionary<ProjectionMember, Expression>();
            foreach (var kvp in _projectionMapping)
            {
                projectionMapping[kvp.Key] = kvp.Value;
            }

            return new SelectExpression(alias, projections, tables, orderings)
            {
                _projectionMapping = projectionMapping,
                Predicate = predicate,
                Offset = offset,
                Limit = limit,
                IsDistinct = distinct
            };
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                if (_projectionMapping != null)
                {
                    foreach (var projectionMapping in _projectionMapping)
                    {
                        hashCode = (hashCode * 397) ^ projectionMapping.Key.GetHashCode();
                        hashCode = (hashCode * 397) ^ projectionMapping.Value.GetHashCode();
                    }
                }

                hashCode = (hashCode * 397) ^ _tables.Aggregate(
                    0, (current, value) => current + ((current * 397) ^ value.GetHashCode()));

                hashCode = (hashCode * 397) ^ (Predicate?.GetHashCode() ?? 0);

                hashCode = (hashCode * 397) ^ _orderings.Aggregate(
                    0, (current, value) => current + ((current * 397) ^ value.GetHashCode()));

                hashCode = (hashCode * 397) ^ (Offset?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Limit?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ IsDistinct.GetHashCode();

                return hashCode;
            }
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.AppendLine("Projection Mapping:");
            using (expressionPrinter.StringBuilder.Indent())
            {
                if (_projectionMapping != null)
                {
                    foreach (var projectionMappingEntry in _projectionMapping)
                    {
                        expressionPrinter.StringBuilder.AppendLine();
                        expressionPrinter.StringBuilder.Append(projectionMappingEntry.Key + " -> ");
                        expressionPrinter.Visit(projectionMappingEntry.Value);
                    }
                }
            }

            expressionPrinter.StringBuilder.AppendLine();
            if (!string.IsNullOrEmpty(Alias))
            {
                expressionPrinter.StringBuilder.AppendLine("(");
                expressionPrinter.StringBuilder.IncrementIndent();
            }

            expressionPrinter.StringBuilder.Append("SELECT ");

            if (IsDistinct)
            {
                expressionPrinter.StringBuilder.Append("DISTINCT ");
            }

            if (Limit != null
                && Offset == null)
            {
                expressionPrinter.StringBuilder.Append("TOP(");
                expressionPrinter.Visit(Limit);
                expressionPrinter.StringBuilder.Append(") ");
            }

            if (Projection.Any())
            {
                expressionPrinter.VisitList(Projection);
            }
            else
            {
                expressionPrinter.StringBuilder.Append("1");
            }

            if (Tables.Any())
            {
                expressionPrinter.StringBuilder.AppendLine().Append("FROM ");

                expressionPrinter.VisitList(Tables, p => p.StringBuilder.AppendLine());
            }

            if (Predicate != null)
            {
                expressionPrinter.StringBuilder.AppendLine().Append("WHERE ");
                expressionPrinter.Visit(Predicate);
            }

            if (Orderings.Any())
            {
                var orderings = Orderings.ToList();
                if (orderings.Count > 0
                    && (Limit != null || Offset != null))
                {
                    expressionPrinter.StringBuilder.AppendLine().Append("ORDER BY ");
                    expressionPrinter.VisitList(orderings);
                }
            }
            else if (Offset != null)
            {
                expressionPrinter.StringBuilder.AppendLine().Append("ORDER BY (SELECT 1)");
            }

            if (Offset != null)
            {
                expressionPrinter.StringBuilder.AppendLine().Append("OFFSET ");
                expressionPrinter.Visit(Offset);
                expressionPrinter.StringBuilder.Append(" ROWS");

                if (Limit != null)
                {
                    expressionPrinter.StringBuilder.Append(" FETCH NEXT ");
                    expressionPrinter.Visit(Limit);
                    expressionPrinter.StringBuilder.Append(" ROWS ONLY");
                }
            }

            if (!string.IsNullOrEmpty(Alias))
            {
                expressionPrinter.StringBuilder.DecrementIndent();
                expressionPrinter.StringBuilder.AppendLine().Append(") AS " + Alias);
            }
        }
    }
}
