// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class EntityProjectionExpression : Expression
    {
        private readonly IDictionary<IProperty, ColumnExpression> _propertyExpressionsCache
            = new Dictionary<IProperty, ColumnExpression>();
        private readonly TableExpressionBase _innerTable;

        public EntityProjectionExpression(IEntityType entityType, TableExpressionBase innerTable)
        {
            EntityType = entityType;
            _innerTable = innerTable;
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
                    ? new EntityProjectionExpression(EntityType, table)
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

        public IEntityType EntityType { get; }

        public ColumnExpression GetProperty(IProperty property)
        {
            if (!_propertyExpressionsCache.TryGetValue(property, out var expression))
            {
                expression = new ColumnExpression(property, _innerTable);
                _propertyExpressionsCache[property] = expression;
            }

            return expression;
        }
    }
}
