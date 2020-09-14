// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         An expression that represents an entity in the projection of <see cref="SelectExpression" />.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class EntityProjectionExpression : Expression
    {
        private readonly IDictionary<IProperty, ColumnExpression> _propertyExpressionMap = new Dictionary<IProperty, ColumnExpression>();

        private readonly IDictionary<INavigation, EntityShaperExpression> _ownedNavigationMap
            = new Dictionary<INavigation, EntityShaperExpression>();

        /// <summary>
        ///     Creates a new instance of the <see cref="EntityProjectionExpression" /> class.
        /// </summary>
        /// <param name="entityType"> The entity type to shape. </param>
        /// <param name="innerTable"> The table from which entity columns are being projected out. </param>
        /// <param name="nullable"> A bool value indicating whether this entity instance can be null. </param>
        [Obsolete("Use the constructor which takes populated column expressions map.", error: true)]
        public EntityProjectionExpression([NotNull] IEntityType entityType, [NotNull] TableExpressionBase innerTable, bool nullable)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="EntityProjectionExpression" /> class.
        /// </summary>
        /// <param name="entityType"> The entity type to shape. </param>
        /// <param name="propertyExpressionMap"> A dictionary of column expressions corresponding to properties of the entity type. </param>
        /// <param name="discriminatorExpression"> A <see cref="SqlExpression" /> to generate discriminator for each concrete entity type in hierarchy. </param>
        public EntityProjectionExpression(
            [NotNull] IEntityType entityType,
            [NotNull] IDictionary<IProperty, ColumnExpression> propertyExpressionMap,
            [CanBeNull] SqlExpression discriminatorExpression = null)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(propertyExpressionMap, nameof(propertyExpressionMap));

            EntityType = entityType;
            _propertyExpressionMap = propertyExpressionMap;
            DiscriminatorExpression = discriminatorExpression;
        }

        /// <summary>
        ///     The entity type being projected out.
        /// </summary>
        public virtual IEntityType EntityType { get; }

        /// <summary>
        ///     A <see cref="SqlExpression" /> to generate discriminator for entity type.
        /// </summary>
        public virtual SqlExpression DiscriminatorExpression { get; }

        /// <inheritdoc />
        public sealed override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type
            => EntityType.ClrType;

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var changed = false;
            var propertyExpressionMap = new Dictionary<IProperty, ColumnExpression>();
            foreach (var expression in _propertyExpressionMap)
            {
                var newExpression = (ColumnExpression)visitor.Visit(expression.Value);
                changed |= newExpression != expression.Value;

                propertyExpressionMap[expression.Key] = newExpression;
            }

            var discriminatorExpression = (SqlExpression)visitor.Visit(DiscriminatorExpression);
            changed |= discriminatorExpression != DiscriminatorExpression;

            return changed
                ? new EntityProjectionExpression(EntityType, propertyExpressionMap, discriminatorExpression)
                : this;
        }

        /// <summary>
        ///     Makes entity instance in projection nullable.
        /// </summary>
        /// <returns> A new entity projection expression which can project nullable entity. </returns>
        public virtual EntityProjectionExpression MakeNullable()
        {
            var propertyExpressionMap = new Dictionary<IProperty, ColumnExpression>();
            foreach (var expression in _propertyExpressionMap)
            {
                propertyExpressionMap[expression.Key] = expression.Value.MakeNullable();
            }

            // We don't need to process DiscriminatorExpression because they are already nullable
            return new EntityProjectionExpression(EntityType, propertyExpressionMap, DiscriminatorExpression);
        }

        /// <summary>
        ///     Updates the entity type being projected out to one of the derived type.
        /// </summary>
        /// <param name="derivedType"> A derived entity type which should be projected. </param>
        /// <returns> A new entity projection expression which has the derived type being projected. </returns>
        public virtual EntityProjectionExpression UpdateEntityType([NotNull] IEntityType derivedType)
        {
            Check.NotNull(derivedType, nameof(derivedType));

            if (!derivedType.GetAllBaseTypes().Contains(EntityType))
            {
                throw new InvalidOperationException(
                    RelationalStrings.InvalidDerivedTypeInEntityProjection(
                        derivedType.DisplayName(), EntityType.DisplayName()));
            }

            var propertyExpressionMap = new Dictionary<IProperty, ColumnExpression>();
            foreach (var kvp in _propertyExpressionMap)
            {
                var property = kvp.Key;
                if (derivedType.IsAssignableFrom(property.DeclaringEntityType)
                    || property.DeclaringEntityType.IsAssignableFrom(derivedType))
                {
                    propertyExpressionMap[property] = kvp.Value;
                }
            }

            var discriminatorExpression = DiscriminatorExpression;
            if (DiscriminatorExpression is CaseExpression caseExpression)
            {
                var entityTypesToSelect = derivedType.GetTptDiscriminatorValues();
                var whenClauses = caseExpression.WhenClauses
                    .Where(wc => entityTypesToSelect.Contains((string)((SqlConstantExpression)wc.Result).Value))
                    .ToList();

                discriminatorExpression = caseExpression.Update(operand: null, whenClauses, elseResult: null);
            }

            return new EntityProjectionExpression(derivedType, propertyExpressionMap, discriminatorExpression);
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
                    RelationalStrings.UnableToBindMemberToEntityProjection("property", property.Name, EntityType.DisplayName()));
            }

            return _propertyExpressionMap[property];
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
                    RelationalStrings.UnableToBindMemberToEntityProjection("navigation", navigation.Name, EntityType.DisplayName()));
            }

            _ownedNavigationMap[navigation] = entityShaper;
        }

        /// <summary>
        ///     Binds a navigation with this entity projection to get entity shaper for the target entity type of the navigation which was
        ///     previously added using <see cref="AddNavigationBinding(INavigation, EntityShaperExpression)" /> method.
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
                    RelationalStrings.UnableToBindMemberToEntityProjection("navigation", navigation.Name, EntityType.DisplayName()));
            }

            return _ownedNavigationMap.TryGetValue(navigation, out var expression)
                ? expression
                : null;
        }
    }
}
