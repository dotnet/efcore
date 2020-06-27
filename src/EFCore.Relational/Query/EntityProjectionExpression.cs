// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         An expression that represents an entity in the projection of <see cref="SelectExpression"/>.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class EntityProjectionExpression : Expression
    {
        private readonly IDictionary<IProperty, ColumnExpression> _propertyExpressionsCache
            = new Dictionary<IProperty, ColumnExpression>();

        private readonly IDictionary<INavigation, EntityShaperExpression> _navigationExpressionsCache
            = new Dictionary<INavigation, EntityShaperExpression>();

        private readonly TableExpressionBase _innerTable;
        private readonly bool _nullable;

        /// <summary>
        ///     Creates a new instance of the <see cref="EntityProjectionExpression" /> class.
        /// </summary>
        /// <param name="entityType"> The entity type to shape. </param>
        /// <param name="innerTable"> The table from which entity columns are being projected out. </param>
        /// <param name="nullable"> A bool value indicating whether this entity instance can be null. </param>
        public EntityProjectionExpression([NotNull] IEntityType entityType, [NotNull] TableExpressionBase innerTable, bool nullable)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(innerTable, nameof(innerTable));

            EntityType = entityType;
            _innerTable = innerTable;
            _nullable = nullable;
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="EntityProjectionExpression" /> class.
        /// </summary>
        /// <param name="entityType"> The entity type to shape. </param>
        /// <param name="propertyExpressions"> A dictionary of column expressions corresponding to properties of the entity type. </param>
        public EntityProjectionExpression([NotNull] IEntityType entityType, [NotNull] IDictionary<IProperty, ColumnExpression> propertyExpressions)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(propertyExpressions, nameof(propertyExpressions));

            EntityType = entityType;
            _propertyExpressionsCache = propertyExpressions;
        }

        /// <summary>
        ///     The entity type being projected out.
        /// </summary>
        public virtual IEntityType EntityType { get; }
        /// <inheritdoc />
        public sealed override ExpressionType NodeType => ExpressionType.Extension;
        /// <inheritdoc />
        public override Type Type => EntityType.ClrType;

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            if (_innerTable != null)
            {
                var table = (TableExpressionBase)visitor.Visit(_innerTable);

                return table != _innerTable
                    ? new EntityProjectionExpression(EntityType, table, _nullable)
                    : this;
            }

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

        /// <summary>
        ///     Makes entity instance in projection nullable.
        /// </summary>
        /// <returns> A new entity projection expression which can project nullable entity. </returns>
        public virtual EntityProjectionExpression MakeNullable()
        {
            if (_innerTable != null)
            {
                return new EntityProjectionExpression(EntityType, _innerTable, nullable: true);
            }

            var newCache = new Dictionary<IProperty, ColumnExpression>();
            foreach (var expression in _propertyExpressionsCache)
            {
                newCache[expression.Key] = expression.Value.MakeNullable();
            }

            return new EntityProjectionExpression(EntityType, newCache);
        }

        /// <summary>
        ///     Updates the entity type being projected out to one of the derived type.
        /// </summary>
        /// <param name="derivedType"> A derived entity type which should be projected. </param>
        /// <returns> A new entity projection expression which has the derived type being projected. </returns>
        public virtual EntityProjectionExpression UpdateEntityType([NotNull] IEntityType derivedType)
        {
            Check.NotNull(derivedType, nameof(derivedType));

            if (_innerTable != null)
            {
                return new EntityProjectionExpression(derivedType, _innerTable, _nullable);
            }

            var propertyExpressionCache = new Dictionary<IProperty, ColumnExpression>();
            foreach (var kvp in _propertyExpressionsCache)
            {
                var property = kvp.Key;
                if (derivedType.IsAssignableFrom(property.DeclaringEntityType)
                    || property.DeclaringEntityType.IsAssignableFrom(derivedType))
                {
                    propertyExpressionCache[property] = kvp.Value;
                }
            }

            return new EntityProjectionExpression(derivedType, propertyExpressionCache);
        }

        /// <summary>
        ///     Binds a property with this entity projection to get the SQL representation.
        /// </summary>
        /// <param name="property"> A property to bind. </param>
        /// <returns> A column which is a SQL representation of the property. </returns>
        public virtual ColumnExpression BindProperty([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            if (!EntityType.IsAssignableFrom(property.DeclaringEntityType)
                && !property.DeclaringEntityType.IsAssignableFrom(EntityType))
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityProjectionExpressionCalledWithIncorrectInterface(
                        "BindProperty",
                        "IProperty",
                        EntityType.DisplayName(),
                        property.Name));
            }

            if (!_propertyExpressionsCache.TryGetValue(property, out var expression))
            {
                expression = new ColumnExpression(property, _innerTable, _nullable);
                _propertyExpressionsCache[property] = expression;
            }

            return expression;
        }

        /// <summary>
        ///     Adds a navigation binding for this entity projection when the target entity type of the navigation is owned or weak.
        /// </summary>
        /// <param name="navigation"> A navigation to add binding for. </param>
        /// <param name="entityShaper"> An entity shaper expression for the target type. </param>
        public virtual void AddNavigationBinding([NotNull] INavigation navigation, [NotNull] EntityShaperExpression entityShaper)
        {
            Check.NotNull(navigation, nameof(navigation));
            Check.NotNull(entityShaper, nameof(entityShaper));

            if (!EntityType.IsAssignableFrom(navigation.DeclaringEntityType)
                && !navigation.DeclaringEntityType.IsAssignableFrom(EntityType))
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityProjectionExpressionCalledWithIncorrectInterface(
                        "AddNavigationBinding",
                        "INavigation",
                        EntityType.DisplayName(),
                        navigation.Name));
            }

            _navigationExpressionsCache[navigation] = entityShaper;
        }

        /// <summary>
        ///     Binds a navigation with this entity projection to get entity shaper for the target entity type of the navigation which was
        ///     previously added using <see cref="AddNavigationBinding(INavigation, EntityShaperExpression)"/> method.
        /// </summary>
        /// <param name="navigation"> A navigation to bind. </param>
        /// <returns> An entity shaper expression for the target entity type of the navigation. </returns>
        public virtual EntityShaperExpression BindNavigation([NotNull] INavigation navigation)
        {
            Check.NotNull(navigation, nameof(navigation));

            if (!EntityType.IsAssignableFrom(navigation.DeclaringEntityType)
                && !navigation.DeclaringEntityType.IsAssignableFrom(EntityType))
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityProjectionExpressionCalledWithIncorrectInterface(
                        "BindNavigation",
                        "INavigation",
                        EntityType.DisplayName(),
                        navigation.Name));
            }

            return _navigationExpressionsCache.TryGetValue(navigation, out var expression)
                ? expression
                : null;
        }
    }
}
