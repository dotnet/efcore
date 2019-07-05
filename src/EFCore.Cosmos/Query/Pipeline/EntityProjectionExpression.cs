// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public class EntityProjectionExpression : Expression
    {
        private readonly IDictionary<IProperty, KeyAccessExpression> _propertyExpressionsCache
            = new Dictionary<IProperty, KeyAccessExpression>();
        private readonly IDictionary<INavigation, ObjectAccessExpression> _navigationExpressionsCache
            = new Dictionary<INavigation, ObjectAccessExpression>();

        public EntityProjectionExpression(IEntityType entityType, RootReferenceExpression accessExpression, string alias)
        {
            EntityType = entityType;
            AccessExpression = accessExpression;
            Alias = alias;
        }

        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => EntityType.ClrType;

        public string Alias { get; }

        public RootReferenceExpression AccessExpression { get; }

        public IEntityType EntityType { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var accessExpression = (RootReferenceExpression)visitor.Visit(AccessExpression);

            return accessExpression != AccessExpression
                ? new EntityProjectionExpression(EntityType, accessExpression, Alias)
                : this;
        }

        public KeyAccessExpression BindProperty(IProperty property)
        {
            if (!EntityType.GetTypesInHierarchy().Contains(property.DeclaringEntityType))
            {
                throw new InvalidOperationException(
                    $"Called EntityProjectionExpression.GetProperty() with incorrect IProperty. EntityType:{EntityType.DisplayName()}, Property:{property.Name}");
            }

            if (!_propertyExpressionsCache.TryGetValue(property, out var expression))
            {
                expression = new KeyAccessExpression(property, AccessExpression);
                _propertyExpressionsCache[property] = expression;
            }

            return expression;
        }

        public ObjectAccessExpression BindNavigation(INavigation navigation)
        {
            if (!EntityType.GetTypesInHierarchy().Contains(navigation.DeclaringEntityType))
            {
                throw new InvalidOperationException(
                    $"Called EntityProjectionExpression.GetNavigation() with incorrect INavigation. EntityType:{EntityType.DisplayName()}, Navigation:{navigation.Name}");
            }

            if (!_navigationExpressionsCache.TryGetValue(navigation, out var expression))
            {
                expression = new ObjectAccessExpression(navigation, AccessExpression);
                _navigationExpressionsCache[navigation] = expression;
            }

            return expression;
        }
    }
}
