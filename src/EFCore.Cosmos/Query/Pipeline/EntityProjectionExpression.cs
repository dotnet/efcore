// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public class EntityProjectionExpression : Expression, IPrintable, IAccessExpression
    {
        private readonly IDictionary<IProperty, SqlExpression> _propertyExpressionsCache
            = new Dictionary<IProperty, SqlExpression>();
        private readonly IDictionary<INavigation, Expression> _navigationExpressionsCache
            = new Dictionary<INavigation, Expression>();

        public EntityProjectionExpression(IEntityType entityType, Expression accessExpression)
        {
            EntityType = entityType;
            AccessExpression = accessExpression;
            Name = (accessExpression as IAccessExpression)?.Name;
        }

        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => EntityType.ClrType;

        public virtual Expression AccessExpression { get; }
        public virtual IEntityType EntityType { get; }
        public virtual string Name { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var accessExpression = visitor.Visit(AccessExpression);

            return accessExpression != AccessExpression
                ? new EntityProjectionExpression(EntityType, accessExpression)
                : this;
        }

        public SqlExpression BindProperty(IProperty property)
        {
            if (!EntityType.IsAssignableFrom(property.DeclaringEntityType)
                && !property.DeclaringEntityType.IsAssignableFrom(EntityType))
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

        public Expression BindNavigation(INavigation navigation)
        {
            if (!EntityType.IsAssignableFrom(navigation.DeclaringEntityType)
                && !navigation.DeclaringEntityType.IsAssignableFrom(EntityType))
            {
                throw new InvalidOperationException(
                    $"Called EntityProjectionExpression.GetNavigation() with incorrect INavigation. EntityType:{EntityType.DisplayName()}, Navigation:{navigation.Name}");
            }

            if (!_navigationExpressionsCache.TryGetValue(navigation, out var expression))
            {
                if (navigation.IsCollection())
                {
                    expression = new ObjectArrayProjectionExpression(navigation, AccessExpression);
                }
                else
                {
                    expression = new EntityProjectionExpression(
                        navigation.GetTargetType(),
                        new ObjectAccessExpression(navigation, AccessExpression));
                }

                _navigationExpressionsCache[navigation] = expression;
            }

            return expression;
        }

        public Expression BindMember(string name, Type entityClrType, out IPropertyBase propertyBase)
        {
            var entityType = EntityType;
            if (entityClrType != null
                && !entityClrType.IsAssignableFrom(entityType.ClrType))
            {
                entityType = entityType.GetDerivedTypes().First(e => entityClrType.IsAssignableFrom(e.ClrType));
            }

            var property = entityType.FindProperty(name);
            if (property != null)
            {
                propertyBase = property;
                return BindProperty(property);
            }

            var navigation = entityType.FindNavigation(name);
            propertyBase = navigation;
            return BindNavigation(navigation);
        }

        public Expression BindMember(MemberInfo memberInfo, Type entityClrType, out IPropertyBase propertyBase)
        {
            var entityType = EntityType;
            if (entityClrType != null
                && !entityClrType.IsAssignableFrom(entityType.ClrType))
            {
                entityType = entityType.GetDerivedTypes().First(e => entityClrType.IsAssignableFrom(e.ClrType));
            }

            var property = entityType.FindProperty(memberInfo);
            if (property != null)
            {
                propertyBase = property;
                return BindProperty(property);
            }

            var navigation = entityType.FindNavigation(memberInfo);
            propertyBase = navigation;
            return BindNavigation(navigation);
        }

        public void Print(ExpressionPrinter expressionPrinter)
            => expressionPrinter.Visit(AccessExpression);

        public override bool Equals(object obj)
            => obj != null
               && (ReferenceEquals(this, obj)
                   || obj is EntityProjectionExpression entityProjectionExpression
                   && Equals(entityProjectionExpression));

        private bool Equals(EntityProjectionExpression entityProjectionExpression)
            => Equals(EntityType, entityProjectionExpression.EntityType)
               && AccessExpression.Equals(entityProjectionExpression.AccessExpression);

        public override int GetHashCode() => HashCode.Combine(EntityType, AccessExpression);
    }
}
