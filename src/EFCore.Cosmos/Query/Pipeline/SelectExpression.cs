// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public class SelectExpression : Expression
    {
        private const string RootAlias = "c";

        private IDictionary<ProjectionMember, Expression> _projectionMapping = new Dictionary<ProjectionMember, Expression>();
        private readonly List<ProjectionExpression> _projection = new List<ProjectionExpression>();
        private readonly List<OrderingExpression> _orderings = new List<OrderingExpression>();

        public SelectExpression(IEntityType entityType)
        {
            ContainerName = entityType.GetCosmosContainerName();
            FromExpression = new RootReferenceExpression(entityType, RootAlias);
            _projectionMapping[new ProjectionMember()] = new EntityProjectionExpression(entityType, FromExpression);
        }

        public SelectExpression(
            List<ProjectionExpression> projections, RootReferenceExpression fromExpression, List<OrderingExpression> orderings)
        {
            _projection = projections;
            FromExpression = fromExpression;
            _orderings = orderings;
        }

        public string ContainerName { get; }
        public IReadOnlyList<ProjectionExpression> Projection => _projection;
        public RootReferenceExpression FromExpression { get; }
        public IReadOnlyList<OrderingExpression> Orderings => _orderings;
        public SqlExpression Predicate { get; private set; }
        public SqlExpression Limit { get; private set; }
        public SqlExpression Offset { get; private set; }
        public bool IsDistinct { get; private set; }

        public Expression GetMappedProjection(ProjectionMember projectionMember)
            => _projectionMapping[projectionMember];

        public void ApplyProjection()
        {
            if (Projection.Any())
            {
                return;
            }

            var result = new Dictionary<ProjectionMember, Expression>();
            foreach (var keyValuePair in _projectionMapping)
            {
                result[keyValuePair.Key] = Constant(AddToProjection(
                        keyValuePair.Value,
                        keyValuePair.Key.LastMember?.Name));
            }

            _projectionMapping = result;
        }

        public void ReplaceProjectionMapping(IDictionary<ProjectionMember, Expression> projectionMapping)
        {
            _projectionMapping.Clear();
            foreach (var kvp in projectionMapping)
            {
                _projectionMapping[kvp.Key] = kvp.Value;
            }
        }

        public int AddToProjection(SqlExpression sqlExpression) => AddToProjection(sqlExpression, null);

        public int AddToProjection(EntityProjectionExpression entityProjection) => AddToProjection(entityProjection, null);

        public int AddToProjection(ObjectArrayProjectionExpression objectArrayProjection) => AddToProjection(objectArrayProjection, null);

        private int AddToProjection(Expression expression, string alias)
        {
            var existingIndex = _projection.FindIndex(pe => pe.Expression.Equals(expression));
            if (existingIndex != -1)
            {
                return existingIndex;
            }

            var baseAlias = alias
                ?? (expression as IAccessExpression)?.Name
                ?? "c";

            var currentAlias = baseAlias;
            var counter = 0;
            while (_projection.Any(pe => string.Equals(pe.Alias, currentAlias, StringComparison.OrdinalIgnoreCase)))
            {
                currentAlias = $"{baseAlias}{counter++}";
            }

            _projection.Add(new ProjectionExpression(expression, currentAlias));

            return _projection.Count - 1;
        }

        public void ApplyDistinct()
        {
            IsDistinct = true;
        }

        public void ClearOrdering()
        {
            _orderings.Clear();
        }

        public void ApplyPredicate(SqlExpression expression)
        {
            if (expression is SqlConstantExpression sqlConstant
                && (bool)sqlConstant.Value)
            {
                return;
            }

            Predicate = Predicate == null
                ? expression
                : new SqlBinaryExpression(
                    ExpressionType.AndAlso,
                    Predicate,
                    expression,
                    typeof(bool),
                    expression.TypeMapping);
        }

        public void ApplyLimit(SqlExpression sqlExpression)
        {
            if (Limit != null)
            {
                throw new InvalidOperationException("See issue#16156");
            }

            Limit = sqlExpression;
        }

        public void ApplyOffset(SqlExpression sqlExpression)
        {
            if (Limit != null
                || Offset != null)
            {
                throw new InvalidOperationException("See issue#16156");
            }

            Offset = sqlExpression;
        }

        public void ApplyOrdering(OrderingExpression orderingExpression)
        {
            if (IsDistinct
                || Limit != null
                || Offset != null)
            {
                throw new InvalidOperationException("See issue#16156");
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

        public void ReverseOrderings()
        {
            if (Limit != null
                || Offset != null)
            {
                throw new InvalidOperationException();
            }

            var existingOrderings = _orderings.ToArray();

            _orderings.Clear();

            foreach (var existingOrdering in existingOrderings)
            {
                _orderings.Add(
                    new OrderingExpression(
                        existingOrdering.Expression,
                        !existingOrdering.Ascending));
            }
        }

        public override Type Type => typeof(JObject);
        public override ExpressionType NodeType => ExpressionType.Extension;

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

            var fromExpression = (RootReferenceExpression)visitor.Visit(FromExpression);
            changed |= fromExpression != FromExpression;

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
                var newSelectExpression = new SelectExpression(projections, fromExpression, orderings)
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
    }
}
