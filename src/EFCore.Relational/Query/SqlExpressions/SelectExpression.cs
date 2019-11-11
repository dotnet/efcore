// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class SelectExpression : TableExpressionBase
    {
        private readonly IDictionary<EntityProjectionExpression, IDictionary<IProperty, int>> _entityProjectionCache
            = new Dictionary<EntityProjectionExpression, IDictionary<IProperty, int>>();

        private readonly List<ProjectionExpression> _projection = new List<ProjectionExpression>();
        private readonly List<TableExpressionBase> _tables = new List<TableExpressionBase>();
        private readonly List<SqlExpression> _groupBy = new List<SqlExpression>();
        private readonly List<OrderingExpression> _orderings = new List<OrderingExpression>();
        private readonly List<SqlExpression> _identifier = new List<SqlExpression>();
        private readonly List<SqlExpression> _childIdentifiers = new List<SqlExpression>();
        private readonly List<SelectExpression> _pendingCollections = new List<SelectExpression>();

        private IDictionary<ProjectionMember, Expression> _projectionMapping = new Dictionary<ProjectionMember, Expression>();

        public IReadOnlyList<ProjectionExpression> Projection => _projection;
        public IReadOnlyList<TableExpressionBase> Tables => _tables;
        public IReadOnlyList<SqlExpression> GroupBy => _groupBy;
        public IReadOnlyList<OrderingExpression> Orderings => _orderings;
        public ISet<string> Tags { get; private set; } = new HashSet<string>();
        public SqlExpression Predicate { get; private set; }
        public SqlExpression Having { get; private set; }
        public SqlExpression Limit { get; private set; }
        public SqlExpression Offset { get; private set; }
        public bool IsDistinct { get; private set; }

        public void ApplyTags(ISet<string> tags)
        {
            Tags = tags;
        }

        internal SelectExpression(
            string alias,
            List<ProjectionExpression> projections,
            List<TableExpressionBase> tables,
            List<SqlExpression> groupBy,
            List<OrderingExpression> orderings)
            : base(alias)
        {
            _projection = projections;
            _tables = tables;
            _groupBy = groupBy;
            _orderings = orderings;
        }

        internal SelectExpression(IEntityType entityType)
            : this(
                entityType, new TableExpression(
                    entityType.GetTableName(),
                    entityType.GetSchema(),
                    entityType.GetTableName().ToLower().Substring(0, 1)))
        {
        }

        internal SelectExpression(IEntityType entityType, string sql, Expression arguments)
            : this(
                entityType, new FromSqlExpression(
                    sql,
                    arguments,
                    entityType.GetTableName().ToLower().Substring(0, 1)))
        {
        }

        private SelectExpression(IEntityType entityType, TableExpressionBase tableExpression)
            : base(null)
        {
            _tables.Add(tableExpression);

            var entityProjection = new EntityProjectionExpression(entityType, tableExpression, false);
            _projectionMapping[new ProjectionMember()] = entityProjection;

            if (entityType.FindPrimaryKey() != null)
            {
                foreach (var property in entityType.FindPrimaryKey().Properties)
                {
                    _identifier.Add(entityProjection.BindProperty(property));
                }
            }
        }

        public bool IsNonComposedFromSql()
            => Limit == null
                && Offset == null
                && !IsDistinct
                && Predicate == null
                && GroupBy.Count == 0
                && Having == null
                && Orderings.Count == 0
                && Tables.Count == 1
                && Tables[0] is FromSqlExpression fromSql
                && Projection.All(
                    pe => pe.Expression is ColumnExpression column
                        && string.Equals(fromSql.Alias, column.Table.Alias, StringComparison.OrdinalIgnoreCase));

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

                    foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
                    {
                        map[property] = AddToProjection(entityProjection.BindProperty(property));
                    }

                    result[keyValuePair.Key] = Constant(map);
                }
                else
                {
                    result[keyValuePair.Key] = Constant(
                        AddToProjection(
                            (SqlExpression)keyValuePair.Value, keyValuePair.Key.Last?.Name));
                }
            }

            _projectionMapping = result;
        }

        private static IEnumerable<IProperty> GetAllPropertiesInHierarchy(IEntityType entityType)
            => entityType.GetTypesInHierarchy().SelectMany(EntityTypeExtensions.GetDeclaredProperties);

        public void ReplaceProjectionMapping(IDictionary<ProjectionMember, Expression> projectionMapping)
        {
            _projectionMapping.Clear();
            foreach (var kvp in projectionMapping)
            {
                _projectionMapping[kvp.Key] = kvp.Value;
            }
        }

        public Expression GetMappedProjection(ProjectionMember projectionMember)
            => _projectionMapping[projectionMember];

        public int AddToProjection(SqlExpression sqlExpression)
            => AddToProjection(sqlExpression, null);

        private int AddToProjection(SqlExpression sqlExpression, string alias)
        {
            var existingIndex = _projection.FindIndex(pe => pe.Expression.Equals(sqlExpression));
            if (existingIndex != -1)
            {
                return existingIndex;
            }

            var baseAlias = alias ?? (sqlExpression as ColumnExpression)?.Name ?? (Alias != null ? "c" : null);
            var currentAlias = baseAlias ?? "";
            if (Alias != null
                && baseAlias != null)
            {
                var counter = 0;
                while (_projection.Any(pe => string.Equals(pe.Alias, currentAlias, StringComparison.OrdinalIgnoreCase)))
                {
                    currentAlias = $"{baseAlias}{counter++}";
                }
            }

            _projection.Add(new ProjectionExpression(sqlExpression, currentAlias));

            return _projection.Count - 1;
        }

        public IDictionary<IProperty, int> AddToProjection(EntityProjectionExpression entityProjection)
        {
            if (!_entityProjectionCache.TryGetValue(entityProjection, out var dictionary))
            {
                dictionary = new Dictionary<IProperty, int>();
                foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
                {
                    dictionary[property] = AddToProjection(entityProjection.BindProperty(property));
                }

                _entityProjectionCache[entityProjection] = dictionary;
            }

            return dictionary;
        }

        public void PrepareForAggregate()
        {
            if (IsDistinct
                || Limit != null
                || Offset != null
                || GroupBy.Count > 0)
            {
                PushdownIntoSubquery();
            }
        }

        public void ApplyPredicate(SqlExpression expression)
        {
            if (expression is SqlConstantExpression sqlConstant
                && (bool)sqlConstant.Value)
            {
                return;
            }

            if (Limit != null
                || Offset != null)
            {
                expression = new SqlRemappingVisitor(PushdownIntoSubquery(), (SelectExpression)Tables[0]).Remap(expression);
            }

            if (_groupBy.Count > 0)
            {
                Having = Having == null
                    ? expression
                    : new SqlBinaryExpression(
                        ExpressionType.AndAlso,
                        Having,
                        expression,
                        typeof(bool),
                        expression.TypeMapping);
            }
            else
            {
                Predicate = Predicate == null
                    ? expression
                    : new SqlBinaryExpression(
                        ExpressionType.AndAlso,
                        Predicate,
                        expression,
                        typeof(bool),
                        expression.TypeMapping);
            }
        }

        public void ApplyGrouping(Expression keySelector)
        {
            ClearOrdering();

            AppendGroupBy(keySelector);
        }

        private void AppendGroupBy(Expression keySelector)
        {
            switch (keySelector)
            {
                case SqlExpression sqlExpression:
                    if (!(sqlExpression is SqlConstantExpression
                        || sqlExpression is SqlParameterExpression))
                    {
                        _groupBy.Add(sqlExpression);
                    }

                    break;

                case NewExpression newExpression:
                    foreach (var argument in newExpression.Arguments)
                    {
                        AppendGroupBy(argument);
                    }

                    break;

                case MemberInitExpression memberInitExpression:
                    AppendGroupBy(memberInitExpression.NewExpression);
                    foreach (var argument in memberInitExpression.Bindings)
                    {
                        AppendGroupBy(((MemberAssignment)argument).Expression);
                    }

                    break;

                case UnaryExpression unaryExpression
                    when unaryExpression.NodeType == ExpressionType.Convert
                    || unaryExpression.NodeType == ExpressionType.ConvertChecked:
                    AppendGroupBy(unaryExpression.Operand);
                    break;

                default:
                    throw new InvalidOperationException("Invalid keySelector for Group By");
            }
        }

        public void ApplyOrdering(OrderingExpression orderingExpression)
        {
            if (IsDistinct
                || Limit != null
                || Offset != null)
            {
                orderingExpression = orderingExpression.Update(
                    new SqlRemappingVisitor(PushdownIntoSubquery(), (SelectExpression)Tables[0])
                        .Remap(orderingExpression.Expression));
            }

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
            if (Limit != null)
            {
                PushdownIntoSubquery();
            }

            Limit = sqlExpression;
        }

        public void ApplyOffset(SqlExpression sqlExpression)
        {
            if (Limit != null
                || Offset != null)
            {
                PushdownIntoSubquery();
            }

            Offset = sqlExpression;
        }

        public void ReverseOrderings()
        {
            if (Limit != null
                || Offset != null)
            {
                PushdownIntoSubquery();
            }

            var existingOrdering = _orderings.ToArray();

            _orderings.Clear();

            for (var i = 0; i < existingOrdering.Length; i++)
            {
                _orderings.Add(
                    new OrderingExpression(
                        existingOrdering[i].Expression,
                        !existingOrdering[i].IsAscending));
            }
        }

        public void ApplyDistinct()
        {
            if (Limit != null
                || Offset != null)
            {
                PushdownIntoSubquery();
            }

            IsDistinct = true;

            ClearOrdering();
        }

        public void ApplyDefaultIfEmpty(ISqlExpressionFactory sqlExpressionFactory)
        {
            var nullSqlExpression = sqlExpressionFactory.ApplyDefaultTypeMapping(
                new SqlConstantExpression(Constant(null, typeof(string)), null));

            var dummySelectExpression = new SelectExpression(
                alias: "empty",
                new List<ProjectionExpression> { new ProjectionExpression(nullSqlExpression, "empty") },
                new List<TableExpressionBase>(),
                new List<SqlExpression>(),
                new List<OrderingExpression>());

            if (Orderings.Any()
                || Limit != null
                || Offset != null
                || IsDistinct
                || Predicate != null
                || Tables.Count > 1
                || GroupBy.Count > 1)
            {
                PushdownIntoSubquery();
            }

            var joinPredicate = sqlExpressionFactory.Equal(sqlExpressionFactory.Constant(1), sqlExpressionFactory.Constant(1));
            var joinTable = new LeftJoinExpression(Tables.Single(), joinPredicate);
            _tables.Clear();
            _tables.Add(dummySelectExpression);
            _tables.Add(joinTable);

            var projectionMapping = new Dictionary<ProjectionMember, Expression>();
            foreach (var projection in _projectionMapping)
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

                projectionMapping[projection.Key] = projectionToAdd;
            }

            for (var i = 0; i < _identifier.Count; i++)
            {
                if (_identifier[i] is ColumnExpression column)
                {
                    _identifier[i] = column.MakeNullable();
                }
            }

            for (var i = 0; i < _childIdentifiers.Count; i++)
            {
                if (_childIdentifiers[i] is ColumnExpression column)
                {
                    _childIdentifiers[i] = column.MakeNullable();
                }
            }

            _projectionMapping = projectionMapping;
        }

        public void ClearOrdering()
        {
            _orderings.Clear();
        }

        private enum SetOperationType
        {
            Except,
            Intersect,
            Union
        }

        public void ApplyExcept(SelectExpression source2, bool distinct)
            => ApplySetOperation(SetOperationType.Except, source2, distinct);

        public void ApplyIntersect(SelectExpression source2, bool distinct)
            => ApplySetOperation(SetOperationType.Intersect, source2, distinct);

        public void ApplyUnion(SelectExpression source2, bool distinct)
            => ApplySetOperation(SetOperationType.Union, source2, distinct);

        private void ApplySetOperation(SetOperationType setOperationType, SelectExpression select2, bool distinct)
        {
            // TODO: throw if there are pending collection joins
            // TODO: What happens when applying set operations on 2 queries with one of them being grouping

            var select1 = new SelectExpression(
                null, new List<ProjectionExpression>(), _tables.ToList(), _groupBy.ToList(), _orderings.ToList())
            {
                IsDistinct = IsDistinct,
                Predicate = Predicate,
                Having = Having,
                Offset = Offset,
                Limit = Limit
            };

            select1._projectionMapping = new Dictionary<ProjectionMember, Expression>(_projectionMapping);
            _projectionMapping.Clear();

            select1._identifier.AddRange(_identifier);
            _identifier.Clear();

            if (select1.Orderings.Count != 0
                || select1.Limit != null
                || select1.Offset != null)
            {
                select1.PushdownIntoSubquery();
                select1.ClearOrdering();
            }

            if (select2.Orderings.Count != 0
                || select2.Limit != null
                || select2.Offset != null)
            {
                select2.PushdownIntoSubquery();
                select2.ClearOrdering();
            }

            var setExpression = setOperationType switch
            {
                SetOperationType.Except => (SetOperationBase)new ExceptExpression("t", select1, select2, distinct),
                SetOperationType.Intersect => new IntersectExpression("t", select1, select2, distinct),
                SetOperationType.Union => new UnionExpression("t", select1, select2, distinct),
                _ => throw new InvalidOperationException($"Invalid {nameof(setOperationType)}: {setOperationType}")
            };

            if (_projection.Any()
                || select2._projection.Any())
            {
                throw new InvalidOperationException(
                    "Can't process set operations after client evaluation, consider moving the operation"
                    + " before the last Select() call (see issue #16243)");
            }

            if (select1._projectionMapping.Count != select2._projectionMapping.Count)
            {
                // Should not be possible after compiler checks
                throw new InvalidOperationException("Different projection mapping count in set operation");
            }

            foreach (var joinedMapping in select1._projectionMapping.Join(
                select2._projectionMapping,
                kv => kv.Key,
                kv => kv.Key,
                (kv1, kv2) => (kv1.Key, Value1: kv1.Value, Value2: kv2.Value)))
            {
                if (joinedMapping.Value1 is EntityProjectionExpression entityProjection1
                    && joinedMapping.Value2 is EntityProjectionExpression entityProjection2)
                {
                    HandleEntityMapping(joinedMapping.Key, select1, entityProjection1, select2, entityProjection2);
                    continue;
                }

                if (joinedMapping.Value1 is SqlExpression innerColumn1
                    && joinedMapping.Value2 is SqlExpression innerColumn2)
                {
                    // For now, make sure that both sides output the same store type, otherwise the query may fail.
                    // TODO: with #15586 we'll be able to also allow different store types which are implicitly convertible to one another.
                    if (innerColumn1.TypeMapping.StoreType != innerColumn2.TypeMapping.StoreType)
                    {
                        throw new InvalidOperationException("Set operations over different store types are currently unsupported");
                    }

                    var alias = GenerateUniqueAlias(
                        joinedMapping.Key.Last?.Name
                        ?? (innerColumn1 as ColumnExpression)?.Name
                        ?? "c");

                    var innerProjection1 = new ProjectionExpression(innerColumn1, alias);
                    var innerProjection2 = new ProjectionExpression(innerColumn2, alias);
                    select1._projection.Add(innerProjection1);
                    select2._projection.Add(innerProjection2);
                    var outerProjection = new ColumnExpression(innerProjection1, setExpression);

                    if (IsNullableProjection(innerProjection1)
                        || IsNullableProjection(innerProjection2))
                    {
                        outerProjection = outerProjection.MakeNullable();
                    }

                    _projectionMapping[joinedMapping.Key] = outerProjection;
                    continue;
                }

                throw new InvalidOperationException(
                    $"Non-matching or unknown projection mapping type in set operation ({joinedMapping.Value1.GetType().Name} and {joinedMapping.Value2.GetType().Name})");
            }

            Offset = null;
            Limit = null;
            IsDistinct = false;
            Predicate = null;
            Having = null;
            _groupBy.Clear();
            _orderings.Clear();
            _tables.Clear();
            _tables.Add(setExpression);

            void HandleEntityMapping(
                ProjectionMember projectionMember,
                SelectExpression select1, EntityProjectionExpression projection1,
                SelectExpression select2, EntityProjectionExpression projection2)
            {
                if (projection1.EntityType != projection2.EntityType)
                {
                    throw new InvalidOperationException(
                        "Set operations over different entity types are currently unsupported (see #16298)");
                }

                var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();
                foreach (var property in GetAllPropertiesInHierarchy(projection1.EntityType))
                {
                    propertyExpressions[property] = AddSetOperationColumnProjections(
                        select1, projection1.BindProperty(property),
                        select2, projection2.BindProperty(property));
                }

                _projectionMapping[projectionMember] = new EntityProjectionExpression(projection1.EntityType, propertyExpressions);
            }

            ColumnExpression AddSetOperationColumnProjections(
                SelectExpression select1, ColumnExpression column1,
                SelectExpression select2, ColumnExpression column2)
            {
                var alias = GenerateUniqueAlias(column1.Name);
                var innerProjection1 = new ProjectionExpression(column1, alias);
                var innerProjection2 = new ProjectionExpression(column2, alias);
                select1._projection.Add(innerProjection1);
                select2._projection.Add(innerProjection2);
                var outerProjection = new ColumnExpression(innerProjection1, setExpression);
                if (IsNullableProjection(innerProjection1)
                    || IsNullableProjection(innerProjection2))
                {
                    outerProjection = outerProjection.MakeNullable();
                }

                if (select1._identifier.Contains(column1))
                {
                    _identifier.Add(outerProjection);
                }

                return outerProjection;
            }

            string GenerateUniqueAlias(string baseAlias)
            {
                var currentAlias = baseAlias ?? "";
                var counter = 0;
                while (select1._projection.Any(pe => string.Equals(pe.Alias, currentAlias, StringComparison.OrdinalIgnoreCase)))
                {
                    currentAlias = $"{baseAlias}{counter++}";
                }

                return currentAlias;
            }

            static bool IsNullableProjection(ProjectionExpression projectionExpression)
                => projectionExpression.Expression switch
                {
                    ColumnExpression columnExpression => columnExpression.IsNullable,
                    SqlConstantExpression sqlConstantExpression => sqlConstantExpression.Value == null,
                    _ => true,
                };
        }

        private ColumnExpression GenerateOuterColumn(SqlExpression projection, string alias = null)
        {
            var index = AddToProjection(projection, alias);
            return new ColumnExpression(_projection[index], this);
        }

        public IDictionary<SqlExpression, ColumnExpression> PushdownIntoSubquery()
        {
            var subquery = new SelectExpression(
                "t", new List<ProjectionExpression>(), _tables.ToList(), _groupBy.ToList(), _orderings.ToList())
            {
                IsDistinct = IsDistinct,
                Predicate = Predicate,
                Having = Having,
                Offset = Offset,
                Limit = Limit
            };

            var projectionMap = new Dictionary<SqlExpression, ColumnExpression>();

            // Projections may be present if added by lifting SingleResult/Enumerable in projection through join
            if (_projection.Any())
            {
                var projections = _projection.Select(pe => pe.Expression).ToList();
                _projection.Clear();
                foreach (var projection in projections)
                {
                    var outerColumn = subquery.GenerateOuterColumn(projection);
                    AddToProjection(outerColumn);
                    projectionMap[projection] = outerColumn;
                }
            }

            foreach (var mapping in _projectionMapping.ToList())
            {
                // If projectionMapping's value is ConstantExpression then projection has already been applied
                // And captured in _projections above so we don't need to process this.
                if (mapping.Value is ConstantExpression)
                {
                    break;
                }

                if (mapping.Value is EntityProjectionExpression entityProjection)
                {
                    _projectionMapping[mapping.Key] = LiftEntityProjectionFromSubquery(entityProjection);
                }
                else
                {
                    var innerColumn = (SqlExpression)mapping.Value;
                    var outerColumn = subquery.GenerateOuterColumn(innerColumn);
                    projectionMap[innerColumn] = outerColumn;
                    _projectionMapping[mapping.Key] = outerColumn;
                }
            }

            var identifiers = _identifier.ToList();
            _identifier.Clear();
            // TODO: See issue#15873
            foreach (var identifier in identifiers)
            {
                if (projectionMap.TryGetValue(identifier, out var outerColumn))
                {
                    _identifier.Add(outerColumn);
                }
                else if (!IsDistinct
                    && GroupBy.Count == 0)
                {
                    outerColumn = subquery.GenerateOuterColumn(identifier);
                    _identifier.Add(outerColumn);
                }
            }

            var childIdentifiers = _childIdentifiers.ToList();
            _childIdentifiers.Clear();
            // TODO: See issue#15873
            foreach (var identifier in childIdentifiers)
            {
                if (projectionMap.TryGetValue(identifier, out var outerColumn))
                {
                    _childIdentifiers.Add(outerColumn);
                }
                else if (!IsDistinct
                    && GroupBy.Count == 0)
                {
                    outerColumn = subquery.GenerateOuterColumn(identifier);
                    _childIdentifiers.Add(outerColumn);
                }
            }

            var pendingCollections = _pendingCollections.ToList();
            _pendingCollections.Clear();
            _pendingCollections.AddRange(pendingCollections.Select(new SqlRemappingVisitor(projectionMap, subquery).Remap));

            _orderings.Clear();
            // Only lift order by to outer if subquery does not have distinct
            if (!subquery.IsDistinct)
            {
                foreach (var ordering in subquery._orderings)
                {
                    var orderingExpression = ordering.Expression;
                    if (!projectionMap.TryGetValue(orderingExpression, out var outerColumn))
                    {
                        outerColumn = subquery.GenerateOuterColumn(orderingExpression);
                    }

                    _orderings.Add(ordering.Update(outerColumn));
                }
            }

            if (subquery.Offset == null
                && subquery.Limit == null)
            {
                subquery.ClearOrdering();
            }

            Offset = null;
            Limit = null;
            IsDistinct = false;
            Predicate = null;
            Having = null;
            _tables.Clear();
            _tables.Add(subquery);
            _groupBy.Clear();

            return projectionMap;

            EntityProjectionExpression LiftEntityProjectionFromSubquery(EntityProjectionExpression entityProjection)
            {
                var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();
                foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
                {
                    var innerColumn = entityProjection.BindProperty(property);
                    var outerColumn = subquery.GenerateOuterColumn(innerColumn);
                    projectionMap[innerColumn] = outerColumn;
                    propertyExpressions[property] = outerColumn;
                }

                var newEntityProjection = new EntityProjectionExpression(entityProjection.EntityType, propertyExpressions);
                // Also lift nested entity projections
                foreach (var navigation in entityProjection.EntityType.GetTypesInHierarchy()
                    .SelectMany(EntityTypeExtensions.GetDeclaredNavigations))
                {
                    var boundEntityShaperExpression = entityProjection.BindNavigation(navigation);
                    if (boundEntityShaperExpression != null)
                    {
                        var innerEntityProjection = (EntityProjectionExpression)boundEntityShaperExpression.ValueBufferExpression;
                        var newInnerEntityProjection = LiftEntityProjectionFromSubquery(innerEntityProjection);
                        boundEntityShaperExpression = boundEntityShaperExpression.Update(newInnerEntityProjection);
                        newEntityProjection.AddNavigationBinding(navigation, boundEntityShaperExpression);
                    }
                }

                return newEntityProjection;
            }
        }

        public Expression AddSingleProjection(ShapedQueryExpression shapedQueryExpression)
        {
            var innerSelectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;
            var shaperExpression = shapedQueryExpression.ShaperExpression;
            var innerExpression = RemoveConvert(shaperExpression);
            if (!(innerExpression is EntityShaperExpression))
            {
                var sentinelExpression = innerSelectExpression.Limit;
                ProjectionBindingExpression dummyProjection;
                if (innerSelectExpression.Projection.Any())
                {
                    var index = innerSelectExpression.AddToProjection(sentinelExpression);
                    dummyProjection = new ProjectionBindingExpression(
                        innerSelectExpression, index, sentinelExpression.Type);
                }
                else
                {
                    innerSelectExpression._projectionMapping[new ProjectionMember()] = sentinelExpression;
                    dummyProjection = new ProjectionBindingExpression(
                        innerSelectExpression, new ProjectionMember(), sentinelExpression.Type);
                }

                shaperExpression = Condition(
                    Equal(dummyProjection, Default(dummyProjection.Type)),
                    Default(shaperExpression.Type),
                    shaperExpression);
            }

            innerSelectExpression.ApplyProjection();
            var projectionCount = innerSelectExpression.Projection.Count;
            AddOuterApply(innerSelectExpression, null);

            // Joined SelectExpression may different based on left join or outer apply
            // And it will always be SelectExpression because of presence of Take(1)
            // So we need to remap projections from that SelectExpression to outer SelectExpression
            var addedSelectExperssion = (SelectExpression)((JoinExpressionBase)_tables[_tables.Count - 1]).Table;
            var indexOffset = _projection.Count;
            // We only take projectionCount since the subquery can have additional projections for identifiers
            // Which are not relevant for this translation
            foreach (var projection in addedSelectExperssion.Projection.Take(projectionCount))
            {
                AddToProjection(MakeNullable(addedSelectExperssion.GenerateOuterColumn(projection.Expression)));
            }

            return new ShaperRemappingExpressionVisitor(this, innerSelectExpression, indexOffset)
                .Visit(shaperExpression);

            static Expression RemoveConvert(Expression expression)
                => expression is UnaryExpression unaryExpression
                    && unaryExpression.NodeType == ExpressionType.Convert
                    ? RemoveConvert(unaryExpression.Operand)
                    : expression;
        }

        public CollectionShaperExpression AddCollectionProjection(
            ShapedQueryExpression shapedQueryExpression, INavigation navigation, Type elementType)
        {
            var innerSelectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;
            _pendingCollections.Add(innerSelectExpression);

            return new CollectionShaperExpression(
                new ProjectionBindingExpression(this, _pendingCollections.Count - 1, typeof(object)),
                shapedQueryExpression.ShaperExpression,
                navigation,
                elementType);
        }

        public Expression ApplyCollectionJoin(
            int collectionIndex, int collectionId, Expression innerShaper, INavigation navigation, Type elementType)
        {
            var innerSelectExpression = _pendingCollections[collectionIndex];
            _pendingCollections[collectionIndex] = null;
            var parentIdentifier = GetIdentifierAccessor(_identifier);
            var outerIdentifier = GetIdentifierAccessor(_identifier.Concat(_childIdentifiers));
            innerSelectExpression.ApplyProjection();
            var selfIdentifier = innerSelectExpression.GetIdentifierAccessor(innerSelectExpression._identifier);

            if (collectionIndex == 0)
            {
                foreach (var column in _identifier)
                {
                    AppendOrdering(new OrderingExpression(column, ascending: true));
                }
            }

            var joinPredicate = TryExtractJoinKey(innerSelectExpression);
            var containsOuterReference = new SelectExpressionCorrelationFindingExpressionVisitor(Tables)
                .ContainsOuterReference(innerSelectExpression);
            if (containsOuterReference && joinPredicate != null)
            {
                innerSelectExpression.ApplyPredicate(joinPredicate);
                joinPredicate = null;
            }

            if (innerSelectExpression.Offset != null
                || innerSelectExpression.Limit != null
                || innerSelectExpression.IsDistinct
                || innerSelectExpression.Predicate != null
                || innerSelectExpression.Tables.Count > 1
                || innerSelectExpression.GroupBy.Count > 1)
            {
                var sqlRemappingVisitor = new SqlRemappingVisitor(
                    innerSelectExpression.PushdownIntoSubquery(),
                    (SelectExpression)innerSelectExpression.Tables[0]);
                joinPredicate = sqlRemappingVisitor.Remap(joinPredicate);
            }

            var joinExpression = joinPredicate == null
                ? (TableExpressionBase)new OuterApplyExpression(innerSelectExpression.Tables.Single())
                : new LeftJoinExpression(innerSelectExpression.Tables.Single(), joinPredicate);
            _tables.Add(joinExpression);

            foreach (var ordering in innerSelectExpression.Orderings)
            {
                AppendOrdering(ordering.Update(MakeNullable(ordering.Expression)));
            }

            var indexOffset = _projection.Count;
            foreach (var projection in innerSelectExpression.Projection)
            {
                AddToProjection(MakeNullable(projection.Expression));
            }

            foreach (var identifier in innerSelectExpression._identifier.Concat(innerSelectExpression._childIdentifiers))
            {
                var updatedColumn = MakeNullable(identifier);
                _childIdentifiers.Add(updatedColumn);
                AppendOrdering(new OrderingExpression(updatedColumn, ascending: true));
            }

            var shaperRemapper = new ShaperRemappingExpressionVisitor(this, innerSelectExpression, indexOffset);
            innerShaper = shaperRemapper.Visit(innerShaper);
            selfIdentifier = shaperRemapper.Visit(selfIdentifier);

            return new RelationalCollectionShaperExpression(
                collectionId, parentIdentifier, outerIdentifier, selfIdentifier, innerShaper, navigation, elementType);
        }

        private static SqlExpression MakeNullable(SqlExpression sqlExpression)
            => sqlExpression is ColumnExpression column ? column.MakeNullable() : sqlExpression;

        private Expression GetIdentifierAccessor(IEnumerable<SqlExpression> identifyingProjection)
        {
            var updatedExpressions = new List<Expression>();
            foreach (var keyExpression in identifyingProjection)
            {
                var index = AddToProjection(keyExpression);
                var projectionBindingExpression = new ProjectionBindingExpression(this, index, keyExpression.Type.MakeNullable());

                updatedExpressions.Add(
                    projectionBindingExpression.Type.IsValueType
                        ? Convert(projectionBindingExpression, typeof(object))
                        : (Expression)projectionBindingExpression);
            }

            return NewArrayInit(
                typeof(object),
                updatedExpressions);
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
                    var oldIndexMap = (IDictionary<IProperty, int>)GetProjectionIndex(
                        (ProjectionBindingExpression)entityShaper.ValueBufferExpression);
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
                    ? ((ConstantExpression)_innerSelectExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember)).Value
                    : (projectionBindingExpression.Index != null
                        ? (object)projectionBindingExpression.Index
                        : projectionBindingExpression.IndexMap);
            }
        }

        private SqlExpression TryExtractJoinKey(SelectExpression selectExpression)
        {
            if (selectExpression.Limit == null
                && selectExpression.Offset == null
                && selectExpression.Predicate != null)
            {
                var columnExpressions = new List<ColumnExpression>();
                var joinPredicate = TryExtractJoinKey(selectExpression, selectExpression.Predicate, columnExpressions, out var predicate);
                if (joinPredicate != null)
                {
                    joinPredicate = RemoveRedundantNullChecks(joinPredicate, columnExpressions);
                }

                selectExpression.Predicate = predicate;

                return joinPredicate;
            }

            return null;
        }

        private SqlExpression TryExtractJoinKey(
            SelectExpression selectExpression,
            SqlExpression predicate,
            List<ColumnExpression> columnExpressions,
            out SqlExpression updatedPredicate)
        {
            if (predicate is SqlBinaryExpression sqlBinaryExpression)
            {
                var joinPredicate = ValidateKeyComparison(selectExpression, sqlBinaryExpression, columnExpressions);
                if (joinPredicate != null)
                {
                    updatedPredicate = null;

                    return joinPredicate;
                }

                if (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso)
                {
                    var leftJoinKey = TryExtractJoinKey(
                        selectExpression, sqlBinaryExpression.Left, columnExpressions, out var leftPredicate);
                    var rightJoinKey = TryExtractJoinKey(
                        selectExpression, sqlBinaryExpression.Right, columnExpressions, out var rightPredicate);

                    updatedPredicate = CombineNonNullExpressions(leftPredicate, rightPredicate);

                    return CombineNonNullExpressions(leftJoinKey, rightJoinKey);
                }
            }

            updatedPredicate = predicate;

            return null;
        }

        private static SqlExpression CombineNonNullExpressions(SqlExpression left, SqlExpression right)
            => left != null
                ? right != null
                    ? new SqlBinaryExpression(ExpressionType.AndAlso, left, right, left.Type, left.TypeMapping)
                    : left
                : right;

        private SqlBinaryExpression ValidateKeyComparison(
            SelectExpression inner, SqlBinaryExpression sqlBinaryExpression, List<ColumnExpression> columnExpressions)
        {
            if (sqlBinaryExpression.OperatorType == ExpressionType.Equal)
            {
                if (sqlBinaryExpression.Left is ColumnExpression leftColumn
                    && sqlBinaryExpression.Right is ColumnExpression rightColumn)
                {
                    if (ContainsTableReference(leftColumn.Table)
                        && inner.ContainsTableReference(rightColumn.Table))
                    {
                        columnExpressions.Add(leftColumn);

                        return sqlBinaryExpression;
                    }

                    if (ContainsTableReference(rightColumn.Table)
                        && inner.ContainsTableReference(leftColumn.Table))
                    {
                        columnExpressions.Add(rightColumn);

                        return sqlBinaryExpression.Update(
                            sqlBinaryExpression.Right,
                            sqlBinaryExpression.Left);
                    }
                }
            }

            // null checks are considered part of join key
            if (sqlBinaryExpression.OperatorType == ExpressionType.NotEqual)
            {
                if (sqlBinaryExpression.Left is ColumnExpression leftNullCheckColumn
                    && ContainsTableReference(leftNullCheckColumn.Table)
                    && sqlBinaryExpression.Right is SqlConstantExpression rightConstant
                    && rightConstant.Value == null)
                {
                    return sqlBinaryExpression;
                }

                if (sqlBinaryExpression.Right is ColumnExpression rightNullCheckColumn
                    && ContainsTableReference(rightNullCheckColumn.Table)
                    && sqlBinaryExpression.Left is SqlConstantExpression leftConstant
                    && leftConstant.Value == null)
                {
                    return sqlBinaryExpression.Update(
                        sqlBinaryExpression.Right,
                        sqlBinaryExpression.Left);
                }
            }

            return null;
        }

        private SqlExpression RemoveRedundantNullChecks(SqlExpression predicate, List<ColumnExpression> columnExpressions)
        {
            if (predicate is SqlBinaryExpression sqlBinaryExpression)
            {
                if (sqlBinaryExpression.OperatorType == ExpressionType.NotEqual
                    && sqlBinaryExpression.Left is ColumnExpression leftColumn
                    && columnExpressions.Contains(leftColumn)
                    && sqlBinaryExpression.Right is SqlConstantExpression sqlConstantExpression
                    && sqlConstantExpression.Value == null)
                {
                    return null;
                }

                if (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso)
                {
                    var leftPredicate = RemoveRedundantNullChecks(sqlBinaryExpression.Left, columnExpressions);
                    var rightPredicate = RemoveRedundantNullChecks(sqlBinaryExpression.Right, columnExpressions);

                    return CombineNonNullExpressions(leftPredicate, rightPredicate);
                }
            }

            return predicate;
        }

        private bool ContainsTableReference(TableExpressionBase table)
            => Tables.Any(te => ReferenceEquals(te is JoinExpressionBase jeb ? jeb.Table : te, table));

        private class SelectExpressionCorrelationFindingExpressionVisitor : ExpressionVisitor
        {
            private readonly IReadOnlyList<TableExpressionBase> _tables;
            private bool _containsOuterReference;

            public SelectExpressionCorrelationFindingExpressionVisitor(IReadOnlyList<TableExpressionBase> tables)
            {
                _tables = tables;
            }

            public bool ContainsOuterReference(SelectExpression selectExpression)
            {
                _containsOuterReference = false;

                Visit(selectExpression);

                return _containsOuterReference;
            }

            public override Expression Visit(Expression expression)
            {
                if (_containsOuterReference)
                {
                    return expression;
                }

                if (expression is ColumnExpression columnExpression
                    && _tables.Contains(columnExpression.Table))
                {
                    _containsOuterReference = true;

                    return expression;
                }

                return base.Visit(expression);
            }
        }

        private void GetPartitions(SqlExpression sqlExpression, List<SqlExpression> partitions)
        {
            if (sqlExpression is SqlBinaryExpression sqlBinaryExpression)
            {
                if (sqlBinaryExpression.OperatorType == ExpressionType.Equal)
                {
                    partitions.Add(sqlBinaryExpression.Right);
                }
                else if (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso)
                {
                    GetPartitions(sqlBinaryExpression.Left, partitions);
                    GetPartitions(sqlBinaryExpression.Right, partitions);
                }
            }
        }

        private enum JoinType
        {
            InnerJoin,
            LeftJoin,
            CrossJoin,
            CrossApply,
            OuterApply
        }

        private void AddJoin(
            JoinType joinType,
            SelectExpression innerSelectExpression,
            Type transparentIdentifierType,
            SqlExpression joinPredicate = null)
        {
            // Try to convert Apply to normal join
            if (joinType == JoinType.CrossApply
                || joinType == JoinType.OuterApply)
            {
                // Doing for limit only since limit + offset may need sum
                var limit = innerSelectExpression.Limit;
                innerSelectExpression.Limit = null;

                joinPredicate = TryExtractJoinKey(innerSelectExpression);
                if (joinPredicate != null)
                {
                    var containsOuterReference = new SelectExpressionCorrelationFindingExpressionVisitor(Tables)
                        .ContainsOuterReference(innerSelectExpression);
                    if (containsOuterReference)
                    {
                        innerSelectExpression.ApplyPredicate(joinPredicate);
                        innerSelectExpression.ApplyLimit(limit);
                    }
                    else
                    {
                        if (limit != null)
                        {
                            var partitions = new List<SqlExpression>();
                            GetPartitions(joinPredicate, partitions);
                            var orderings = innerSelectExpression.Orderings.Any()
                                ? innerSelectExpression.Orderings
                                : innerSelectExpression._identifier.Select(e => new OrderingExpression(e, true));
                            var rowNumberExpression = new RowNumberExpression(partitions, orderings.ToList(), limit.TypeMapping);
                            innerSelectExpression.ClearOrdering();

                            var projectionMappings = innerSelectExpression.PushdownIntoSubquery();
                            var subquery = (SelectExpression)innerSelectExpression.Tables[0];

                            joinPredicate = new SqlRemappingVisitor(
                                    projectionMappings, subquery)
                                .Remap(joinPredicate);

                            var outerColumn = subquery.GenerateOuterColumn(rowNumberExpression, "row");
                            var predicate = new SqlBinaryExpression(
                                ExpressionType.LessThanOrEqual, outerColumn, limit, typeof(bool), joinPredicate.TypeMapping);
                            innerSelectExpression.ApplyPredicate(predicate);
                        }

                        AddJoin(
                            joinType == JoinType.CrossApply ? JoinType.InnerJoin : JoinType.LeftJoin,
                            innerSelectExpression, transparentIdentifierType, joinPredicate);
                        return;
                    }
                }
                else
                {
                    innerSelectExpression.ApplyLimit(limit);
                }
            }

            // Verify what are the cases of pushdown for inner & outer both sides
            if (Limit != null
                || Offset != null
                || IsDistinct
                || GroupBy.Count > 0)
            {
                var sqlRemappingVisitor = new SqlRemappingVisitor(PushdownIntoSubquery(), (SelectExpression)Tables[0]);
                innerSelectExpression = sqlRemappingVisitor.Remap(innerSelectExpression);
                joinPredicate = sqlRemappingVisitor.Remap(joinPredicate);
            }

            if (innerSelectExpression.Orderings.Any()
                || innerSelectExpression.Limit != null
                || innerSelectExpression.Offset != null
                || innerSelectExpression.IsDistinct
                || innerSelectExpression.Predicate != null
                || innerSelectExpression.Tables.Count > 1
                || innerSelectExpression.GroupBy.Count > 0)
            {
                joinPredicate = new SqlRemappingVisitor(
                        innerSelectExpression.PushdownIntoSubquery(), (SelectExpression)innerSelectExpression.Tables[0])
                    .Remap(joinPredicate);
            }

            if (joinType != JoinType.LeftJoin)
            {
                _identifier.AddRange(innerSelectExpression._identifier);
            }

            var innerTable = innerSelectExpression.Tables.Single();
            var joinTable = joinType switch
            {
                JoinType.InnerJoin => new InnerJoinExpression(innerTable, joinPredicate),
                JoinType.LeftJoin => new LeftJoinExpression(innerTable, joinPredicate),
                JoinType.CrossJoin => new CrossJoinExpression(innerTable),
                JoinType.CrossApply => new CrossApplyExpression(innerTable),
                JoinType.OuterApply => (TableExpressionBase)new OuterApplyExpression(innerTable),
                _ => throw new InvalidOperationException($"Invalid {nameof(joinType)}: {joinType}")
            };

            _tables.Add(joinTable);

            if (transparentIdentifierType != null)
            {
                var outerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer");
                var projectionMapping = new Dictionary<ProjectionMember, Expression>();
                foreach (var projection in _projectionMapping)
                {
                    projectionMapping[projection.Key.Prepend(outerMemberInfo)] = projection.Value;
                }

                var innerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner");
                var innerNullable = joinType == JoinType.LeftJoin || joinType == JoinType.OuterApply;
                foreach (var projection in innerSelectExpression._projectionMapping)
                {
                    var projectionToAdd = projection.Value;
                    if (innerNullable)
                    {
                        if (projectionToAdd is EntityProjectionExpression entityProjection)
                        {
                            projectionToAdd = entityProjection.MakeNullable();
                        }
                        else if (projectionToAdd is ColumnExpression column)
                        {
                            projectionToAdd = column.MakeNullable();
                        }
                    }

                    projectionMapping[projection.Key.Prepend(innerMemberInfo)] = projectionToAdd;
                }

                _projectionMapping = projectionMapping;
            }
        }

        public void AddInnerJoin(SelectExpression innerSelectExpression, SqlExpression joinPredicate, Type transparentIdentifierType)
            => AddJoin(JoinType.InnerJoin, innerSelectExpression, transparentIdentifierType, joinPredicate);

        public void AddLeftJoin(SelectExpression innerSelectExpression, SqlExpression joinPredicate, Type transparentIdentifierType)
            => AddJoin(JoinType.LeftJoin, innerSelectExpression, transparentIdentifierType, joinPredicate);

        public void AddCrossJoin(SelectExpression innerSelectExpression, Type transparentIdentifierType)
            => AddJoin(JoinType.CrossJoin, innerSelectExpression, transparentIdentifierType);

        public void AddCrossApply(SelectExpression innerSelectExpression, Type transparentIdentifierType)
            => AddJoin(JoinType.CrossApply, innerSelectExpression, transparentIdentifierType);

        public void AddOuterApply(SelectExpression innerSelectExpression, Type transparentIdentifierType)
            => AddJoin(JoinType.OuterApply, innerSelectExpression, transparentIdentifierType);

        private class SqlRemappingVisitor : ExpressionVisitor
        {
            private readonly SelectExpression _subquery;
            private readonly IDictionary<SqlExpression, ColumnExpression> _mappings;

            public SqlRemappingVisitor(IDictionary<SqlExpression, ColumnExpression> mappings, SelectExpression subquery)
            {
                _subquery = subquery;
                _mappings = mappings;
            }

            public SqlExpression Remap(SqlExpression sqlExpression) => (SqlExpression)Visit(sqlExpression);
            public SelectExpression Remap(SelectExpression sqlExpression) => (SelectExpression)Visit(sqlExpression);

            public override Expression Visit(Expression expression)
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

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            // We have to do in-place mutation till we have applied pending collections because of shaper references
            // This is pseudo finalization phase for select expression.
            if (_pendingCollections.Any(e => e != null))
            {
                if (Projection.Any())
                {
                    var projections = _projection.ToList();
                    _projection.Clear();
                    _projection.AddRange(projections.Select(e => (ProjectionExpression)visitor.Visit(e)));
                }
                else
                {
                    var projectionMapping = new Dictionary<ProjectionMember, Expression>();
                    foreach (var mapping in _projectionMapping)
                    {
                        var newProjection = visitor.Visit(mapping.Value);

                        projectionMapping[mapping.Key] = newProjection;
                    }

                    _projectionMapping = projectionMapping;
                }

                var tables = _tables.ToList();
                _tables.Clear();
                _tables.AddRange(tables.Select(e => (TableExpressionBase)visitor.Visit(e)));

                Predicate = (SqlExpression)visitor.Visit(Predicate);

                var groupBy = _groupBy.ToList();
                _groupBy.Clear();
                _groupBy.AddRange(
                    groupBy.Select(e => (SqlExpression)visitor.Visit(e))
                        .Where(e => !(e is SqlConstantExpression || e is SqlParameterExpression)));

                Having = (SqlExpression)visitor.Visit(Having);

                var orderings = _orderings.ToList();
                _orderings.Clear();
                _orderings.AddRange(orderings.Select(e => e.Update((SqlExpression)visitor.Visit(e.Expression))));

                Offset = (SqlExpression)visitor.Visit(Offset);
                Limit = (SqlExpression)visitor.Visit(Limit);

                return this;
            }

            var changed = false;

            var newProjections = _projection;
            var newProjectionMapping = _projectionMapping;
            if (_projection.Any())
            {
                for (var i = 0; i < _projection.Count; i++)
                {
                    var item = _projection[i];
                    var projection = (ProjectionExpression)visitor.Visit(item);
                    if (projection != item
                        && newProjections == _projection)
                    {
                        newProjections = new List<ProjectionExpression>(_projection.Count);
                        for (var j = 0; j < i; j++)
                        {
                            newProjections.Add(_projection[j]);
                        }

                        changed = true;
                    }

                    if (newProjections != _projection)
                    {
                        newProjections.Add(projection);
                    }
                }
            }
            else
            {
                foreach (var mapping in _projectionMapping)
                {
                    var newProjection = visitor.Visit(mapping.Value);
                    if (newProjection != mapping.Value
                        && newProjectionMapping == _projectionMapping)
                    {
                        newProjectionMapping = new Dictionary<ProjectionMember, Expression>(_projectionMapping);
                        changed = true;
                    }

                    if (newProjectionMapping != _projectionMapping)
                    {
                        newProjectionMapping[mapping.Key] = newProjection;
                    }
                }
            }

            var newTables = _tables;
            for (var i = 0; i < _tables.Count; i++)
            {
                var table = _tables[i];
                var newTable = (TableExpressionBase)visitor.Visit(table);
                if (newTable != table
                    && newTables == _tables)
                {
                    newTables = new List<TableExpressionBase>(_tables.Count);
                    for (var j = 0; j < i; j++)
                    {
                        newTables.Add(_tables[j]);
                    }

                    changed = true;
                }

                if (newTables != _tables)
                {
                    newTables.Add(newTable);
                }
            }

            var predicate = (SqlExpression)visitor.Visit(Predicate);
            changed |= predicate != Predicate;

            var newGroupBy = _groupBy;
            for (var i = 0; i < _groupBy.Count; i++)
            {
                var groupingKey = _groupBy[i];
                var newGroupingKey = (SqlExpression)visitor.Visit(groupingKey);
                if (newGroupingKey != groupingKey
                    || newGroupingKey is SqlConstantExpression
                    || newGroupingKey is SqlParameterExpression)
                {
                    if (newGroupBy == _groupBy)
                    {
                        newGroupBy = new List<SqlExpression>(_groupBy.Count);
                        for (var j = 0; j < i; j++)
                        {
                            newGroupBy.Add(_groupBy[j]);
                        }
                    }

                    changed = true;
                }

                if (newGroupBy != _groupBy
                    && !(newGroupingKey is SqlConstantExpression
                        || newGroupingKey is SqlParameterExpression))
                {
                    newGroupBy.Add(newGroupingKey);
                }
            }

            var havingExpression = (SqlExpression)visitor.Visit(Having);
            changed |= havingExpression != Having;

            var newOrderings = _orderings;
            for (var i = 0; i < _orderings.Count; i++)
            {
                var ordering = _orderings[i];
                var newOrdering = (OrderingExpression)visitor.Visit(ordering);
                if (newOrdering != ordering
                    && newOrderings == _orderings)
                {
                    newOrderings = new List<OrderingExpression>(_orderings.Count);
                    for (var j = 0; j < i; j++)
                    {
                        newOrderings.Add(_orderings[j]);
                    }

                    changed = true;
                }

                if (newOrderings != _orderings)
                {
                    newOrderings.Add(newOrdering);
                }
            }

            var offset = (SqlExpression)visitor.Visit(Offset);
            changed |= offset != Offset;

            var limit = (SqlExpression)visitor.Visit(Limit);
            changed |= limit != Limit;

            if (changed)
            {
                var newSelectExpression = new SelectExpression(Alias, newProjections, newTables, newGroupBy, newOrderings)
                {
                    _projectionMapping = newProjectionMapping,
                    Predicate = predicate,
                    Having = havingExpression,
                    Offset = offset,
                    Limit = limit,
                    IsDistinct = IsDistinct,
                    Tags = Tags
                };

                newSelectExpression._identifier.AddRange(_identifier);
                newSelectExpression._identifier.AddRange(_childIdentifiers);

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

            if (_projectionMapping.Count != selectExpression._projectionMapping.Count)
            {
                return false;
            }

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

            if (!Tags.SequenceEqual(selectExpression.Tags))
            {
                return false;
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

            if (!_pendingCollections.SequenceEqual(selectExpression._pendingCollections))
            {
                return false;
            }

            if (!_groupBy.SequenceEqual(selectExpression._groupBy))
            {
                return false;
            }

            if (!(Having == null && selectExpression.Having == null
                || Having != null && Predicate.Equals(selectExpression.Having)))
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

#pragma warning disable IDE0046 // Convert to conditional expression
            if (!(Limit == null && selectExpression.Limit == null
#pragma warning restore IDE0046 // Convert to conditional expression
                || Limit != null && Limit.Equals(selectExpression.Limit)))
            {
                return false;
            }

            return IsDistinct == selectExpression.IsDistinct;
        }

        // This does not take internal states since when using this method SelectExpression should be finalized
        public SelectExpression Update(
            List<ProjectionExpression> projections,
            List<TableExpressionBase> tables,
            SqlExpression predicate,
            List<SqlExpression> groupBy,
            SqlExpression havingExpression,
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

            return new SelectExpression(alias, projections, tables, groupBy, orderings)
            {
                _projectionMapping = projectionMapping,
                Predicate = predicate,
                Having = havingExpression,
                Offset = offset,
                Limit = limit,
                IsDistinct = distinct,
                Tags = Tags
            };
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(base.GetHashCode());

            foreach (var projectionMapping in _projectionMapping)
            {
                hash.Add(projectionMapping.Key);
                hash.Add(projectionMapping.Value);
            }

            foreach (var tag in Tags)
            {
                hash.Add(tag);
            }

            foreach (var table in _tables)
            {
                hash.Add(table);
            }

            hash.Add(Predicate);

            foreach (var groupingKey in _groupBy)
            {
                hash.Add(groupingKey);
            }

            hash.Add(Having);

            foreach (var ordering in _orderings)
            {
                hash.Add(ordering);
            }

            hash.Add(Offset);
            hash.Add(Limit);
            hash.Add(IsDistinct);

            return hash.ToHashCode();
        }

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.AppendLine("Projection Mapping:");
            using (expressionPrinter.Indent())
            {
                foreach (var projectionMappingEntry in _projectionMapping)
                {
                    expressionPrinter.AppendLine();
                    expressionPrinter.Append(projectionMappingEntry.Key + " -> ");
                    expressionPrinter.Visit(projectionMappingEntry.Value);
                }
            }

            expressionPrinter.AppendLine();

            foreach (var tag in Tags)
            {
                expressionPrinter.Append($"-- {tag}");
            }

            IDisposable indent = null;

            if (Alias != null)
            {
                expressionPrinter.AppendLine("(");
                indent = expressionPrinter.Indent();
            }

            expressionPrinter.Append("SELECT ");

            if (IsDistinct)
            {
                expressionPrinter.Append("DISTINCT ");
            }

            if (Limit != null
                && Offset == null)
            {
                expressionPrinter.Append("TOP(");
                expressionPrinter.Visit(Limit);
                expressionPrinter.Append(") ");
            }

            if (Projection.Any())
            {
                expressionPrinter.VisitList(Projection);
            }
            else
            {
                expressionPrinter.Append("1");
            }

            if (Tables.Any())
            {
                expressionPrinter.AppendLine().Append("FROM ");

                expressionPrinter.VisitList(Tables, p => p.AppendLine());
            }

            if (Predicate != null)
            {
                expressionPrinter.AppendLine().Append("WHERE ");
                expressionPrinter.Visit(Predicate);
            }

            if (GroupBy.Any())
            {
                expressionPrinter.AppendLine().Append("GROUP BY ");
                expressionPrinter.VisitList(GroupBy);
            }

            if (Having != null)
            {
                expressionPrinter.AppendLine().Append("HAVING ");
                expressionPrinter.Visit(Having);
            }

            if (Orderings.Any())
            {
                expressionPrinter.AppendLine().Append("ORDER BY ");
                expressionPrinter.VisitList(Orderings);
            }
            else if (Offset != null)
            {
                expressionPrinter.AppendLine().Append("ORDER BY (SELECT 1)");
            }

            if (Offset != null)
            {
                expressionPrinter.AppendLine().Append("OFFSET ");
                expressionPrinter.Visit(Offset);
                expressionPrinter.Append(" ROWS");

                if (Limit != null)
                {
                    expressionPrinter.Append(" FETCH NEXT ");
                    expressionPrinter.Visit(Limit);
                    expressionPrinter.Append(" ROWS ONLY");
                }
            }

            if (Alias != null)
            {
                indent?.Dispose();
                expressionPrinter.AppendLine().Append(") AS " + Alias);
            }
        }
    }
}
