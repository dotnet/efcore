// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class EntityProjectionExpression : Expression
    {
        private readonly IDictionary<IProperty, ColumnExpression> _propertyExpressionsCache
            = new Dictionary<IProperty, ColumnExpression>();
        private readonly TableExpressionBase _innerTable;
        private readonly bool _nullable;

        public EntityProjectionExpression(IEntityType entityType, TableExpressionBase innerTable, bool nullable)
        {
            EntityType = entityType;
            _innerTable = innerTable;
            _nullable = nullable;
        }

        public EntityProjectionExpression(IEntityType entityType, IDictionary<IProperty, ColumnExpression> propertyExpressions)
        {
            EntityType = entityType;
            _propertyExpressionsCache = propertyExpressions;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            if (_innerTable != null)
            {
                var table = (TableExpressionBase)visitor.Visit(_innerTable);

                return table != _innerTable
                    ? new EntityProjectionExpression(EntityType, table, _nullable)
                    : this;
            }
            else
            {
                var changed = false;
                var newCache = new Dictionary<IProperty, ColumnExpression>();
                foreach (var expression in _propertyExpressionsCache)
                {
                    var newExpression = (ColumnExpression)visitor.Visit(expression.Value);
                    changed |= newExpression != expression.Value;

                    newCache[expression.Key] = newExpression;
                }

                return changed
                    ? new EntityProjectionExpression(EntityType, newCache)
                    : this;
            }
        }

        public EntityProjectionExpression MakeNullable()
        {
            if (_innerTable != null)
            {
                return new EntityProjectionExpression(EntityType, _innerTable, true);
            }
            else
            {
                var newCache = new Dictionary<IProperty, ColumnExpression>();
                foreach (var expression in _propertyExpressionsCache)
                {
                    newCache[expression.Key] = expression.Value.MakeNullable();
                }

                return new EntityProjectionExpression(EntityType, newCache);
            }
        }

        public EntityProjectionExpression UpdateEntityType(IEntityType derivedType)
        {
            if (_innerTable != null)
            {
                return new EntityProjectionExpression(derivedType, _innerTable, _nullable);
            }

            throw new InvalidOperationException("EntityProjectionExpression: Cannot update EntityType when _innerTable is null");
        }

        public IEntityType EntityType { get; }
        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => EntityType.ClrType;

        public ColumnExpression GetProperty(IProperty property)
        {
            if (!EntityType.GetTypesInHierarchy().Contains(property.DeclaringEntityType))
            {
                throw new InvalidOperationException("Called EntityProjectionExpression.GetProperty() with incorrect IProperty");
            }

            if (!_propertyExpressionsCache.TryGetValue(property, out var expression))
            {
                expression = new ColumnExpression(property, _innerTable, _nullable);
                _propertyExpressionsCache[property] = expression;
            }

            return expression;
        }
    }
}
