// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class SelectExpression : TableExpressionBase
    {
        private IDictionary<ProjectionMember, Expression> _projectionMapping
            = new Dictionary<ProjectionMember, Expression>();

        private List<TableExpressionBase> _tables = new List<TableExpressionBase>();
        private readonly List<ProjectionExpression> _projection = new List<ProjectionExpression>();
        private List<OrderingExpression> _orderings = new List<OrderingExpression>();
        public IReadOnlyList<ProjectionExpression> Projection => _projection;
        public IReadOnlyList<TableExpressionBase> Tables => _tables;
        public IReadOnlyList<OrderingExpression> Orderings => _orderings;
        public SqlExpression Predicate { get; private set; }
        public SqlExpression Limit { get; private set; }
        public SqlExpression Offset { get; private set; }
        public bool IsDistinct { get; private set; }

        public SelectExpression(
            string alias,
            IDictionary<ProjectionMember, Expression> projectionMapping,
            List<ProjectionExpression> projections,
            List<TableExpressionBase> tables)
            : base(alias)
        {
            _projectionMapping = projectionMapping;
            _projection = projections;
            _tables = tables;
        }

        public SelectExpression(IEntityType entityType)
            : base("")
        {
            var tableExpression = new TableExpression(
                entityType.Relational().TableName,
                entityType.Relational().Schema,
                entityType.Relational().TableName.ToLower().Substring(0, 1));

            _tables.Add(tableExpression);

            _projectionMapping[new ProjectionMember()] = new EntityProjectionExpression(entityType, tableExpression, false);
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

            var index = 0;
            var result = new Dictionary<ProjectionMember, Expression>();
            foreach (var keyValuePair in _projectionMapping)
            {
                result[keyValuePair.Key] = Constant(index);
                if (keyValuePair.Value is EntityProjectionExpression entityProjection)
                {
                    foreach (var property in entityProjection.EntityType.GetProperties())
                    {
                        var columnExpression = entityProjection.GetProperty(property);
                        _projection.Add(new ProjectionExpression(columnExpression, ""));
                        index++;
                    }
                }
                else
                {
                    _projection.Add(new ProjectionExpression((SqlExpression)keyValuePair.Value, ""));
                    index++;
                }
            }

            _projectionMapping = result;
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

        public void ApplyProjection(IDictionary<ProjectionMember, Expression> projectionMapping)
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

        public void ApplyOrderBy(OrderingExpression orderingExpression)
        {
            _orderings.Clear();
            _orderings.Add(orderingExpression);
        }

        public void ApplyThenBy(OrderingExpression orderingExpression)
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

        public void Reverse()
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

            return new SelectExpression(alias, projectionMapping, _projection.ToList(), _tables.ToList())
            {
                Predicate = Predicate,
                _orderings = _orderings.ToList(),
                Offset = Offset,
                Limit = Limit,
                IsDistinct = IsDistinct
            };
        }

        public void PushdownIntoSubQuery()
        {
            var subquery = Clone("t");

            if (subquery.Limit == null && subquery.Offset == null)
            {
                subquery.ClearOrdering();
            }

            _projectionMapping.Clear();
            var columnNameCounter = 0;
            var index = 0;
            var result = new Dictionary<ProjectionMember, Expression>();
            foreach (var projection in subquery._projectionMapping)
            {
                result[projection.Key] = Constant(index);
                if (projection.Value is EntityProjectionExpression entityProjection)
                {
                    var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();
                    foreach (var property in entityProjection.EntityType.GetProperties())
                    {
                        var innerColumn = entityProjection.GetProperty(property);
                        var projectionExpression = new ProjectionExpression(innerColumn, innerColumn.Name);
                        subquery._projection.Add(projectionExpression);
                        propertyExpressions[property] = new ColumnExpression(projectionExpression, subquery, innerColumn.Nullable);
                        index++;
                    }

                    _projectionMapping[projection.Key] = new EntityProjectionExpression(
                        entityProjection.EntityType, propertyExpressions);
                }
                else
                {
                    var projectionExpression = new ProjectionExpression(
                        (SqlExpression)projection.Value, "c" + columnNameCounter++);
                    subquery._projection.Add(projectionExpression);
                    _projectionMapping[projection.Key] = new ColumnExpression(
                        projectionExpression, subquery, IsNullableProjection(projectionExpression));
                    index++;
                }
            }

            subquery._projectionMapping = result;

            var currentOrderings = _orderings.ToList();
            _orderings.Clear();
            foreach (var ordering in currentOrderings)
            {
                var orderingExpression = ordering.Expression;
                var innerProjection = subquery._projection.FirstOrDefault(
                    pe => pe.Expression.Equals(orderingExpression));
                if (innerProjection != null)
                {
                    _orderings.Add(new OrderingExpression(new ColumnExpression(innerProjection, subquery, IsNullableProjection(innerProjection)), ordering.Ascending));
                }
                else
                {
                    var projectionExpression = new ProjectionExpression(ordering.Expression, "c" + columnNameCounter++);
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
        }

        private static bool IsNullableProjection(ProjectionExpression projection)
        {
            return projection.Expression is ColumnExpression column ? column.Nullable : true;
        }

        public void AddInnerJoin(SelectExpression innerSelectExpression, SqlExpression joinPredicate, Type transparentIdentifierType)
        {
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
                var newSelectExpression = new SelectExpression(Alias, projectionMapping, projections, tables)
                {
                    Predicate = predicate,
                    _orderings = orderings,
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

            return new SelectExpression(alias, projectionMapping, projections, tables)
            {
                Predicate = predicate,
                _orderings = orderings,
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
                foreach (var projectionMapping in _projectionMapping)
                {
                    hashCode = (hashCode * 397) ^ projectionMapping.Key.GetHashCode();
                    hashCode = (hashCode * 397) ^ projectionMapping.Value.GetHashCode();
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
    }
}
