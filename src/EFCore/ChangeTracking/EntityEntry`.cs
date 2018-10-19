// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     <para>
    ///         Provides access to change tracking information and operations for a given entity.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ChangeTracker" /> API and it is
    ///         not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    /// <typeparam name="TEntity"> The type of entity being tracked by this entry. </typeparam>
    public class EntityEntry<TEntity> : EntityEntry
        where TEntity : class
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityEntry([NotNull] InternalEntityEntry internalEntry)
            : base(internalEntry)
        {
        }

        /// <summary>
        ///     Gets the entity being tracked by this entry.
        /// </summary>
        public new virtual TEntity Entity => (TEntity)base.Entity;

        /// <summary>
        ///     Provides access to change tracking information and operations for a given
        ///     property of this entity.
        /// </summary>
        /// <param name="propertyExpression">
        ///     A lambda expression representing the property to access information and operations for
        ///     (<c>t => t.Property1</c>).
        /// </param>
        /// <returns> An object that exposes change tracking information and operations for the given property. </returns>
        public virtual PropertyEntry<TEntity, TProperty> Property<TProperty>(
            [NotNull] Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            Check.NotNull(propertyExpression, nameof(propertyExpression));

            return new PropertyEntry<TEntity, TProperty>(InternalEntry, propertyExpression.GetPropertyAccess().GetSimpleMemberName());
        }

        /// <summary>
        ///     Provides access to change tracking and loading information for a reference (i.e. non-collection)
        ///     navigation property that associates this entity to another entity.
        /// </summary>
        /// <param name="propertyExpression">
        ///     A lambda expression representing the property to access information and operations for
        ///     (<c>t => t.Property1</c>).
        /// </param>
        /// <returns>
        ///     An object that exposes change tracking information and operations for the
        ///     given navigation property.
        /// </returns>
        public virtual ReferenceEntry<TEntity, TProperty> Reference<TProperty>(
            [NotNull] Expression<Func<TEntity, TProperty>> propertyExpression)
            where TProperty : class
        {
            Check.NotNull(propertyExpression, nameof(propertyExpression));

            return new ReferenceEntry<TEntity, TProperty>(InternalEntry, propertyExpression.GetPropertyAccess().GetSimpleMemberName());
        }

        /// <summary>
        ///     Provides access to change tracking and loading information for a collection
        ///     navigation property that associates this entity to a collection of another entities.
        /// </summary>
        /// <param name="propertyExpression">
        ///     A lambda expression representing the property to access information and operations for
        ///     (<c>t => t.Property1</c>).
        /// </param>
        /// <returns>
        ///     An object that exposes change tracking information and operations for the
        ///     given navigation property.
        /// </returns>
        public virtual CollectionEntry<TEntity, TProperty> Collection<TProperty>(
            [NotNull] Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression)
            where TProperty : class
        {
            Check.NotNull(propertyExpression, nameof(propertyExpression));

            return new CollectionEntry<TEntity, TProperty>(InternalEntry, propertyExpression.GetPropertyAccess().GetSimpleMemberName());
        }

        /// <summary>
        ///     Provides access to change tracking and loading information for a reference (i.e. non-collection)
        ///     navigation property that associates this entity to another entity.
        /// </summary>
        /// <param name="propertyName"> The name of the navigation property. </param>
        /// <returns>
        ///     An object that exposes change tracking information and operations for the
        ///     given navigation property.
        /// </returns>
        public virtual ReferenceEntry<TEntity, TProperty> Reference<TProperty>([NotNull] string propertyName)
            where TProperty : class
        {
            Check.NotEmpty(propertyName, nameof(propertyName));

            return new ReferenceEntry<TEntity, TProperty>(InternalEntry, propertyName);
        }

        /// <summary>
        ///     Provides access to change tracking and loading information for a collection
        ///     navigation property that associates this entity to a collection of another entities.
        /// </summary>
        /// <param name="propertyName"> The name of the navigation property. </param>
        /// <returns>
        ///     An object that exposes change tracking information and operations for the
        ///     given navigation property.
        /// </returns>
        public virtual CollectionEntry<TEntity, TProperty> Collection<TProperty>([NotNull] string propertyName)
            where TProperty : class
        {
            Check.NotEmpty(propertyName, nameof(propertyName));

            return new CollectionEntry<TEntity, TProperty>(InternalEntry, propertyName);
        }

        /// <summary>
        ///     Provides access to change tracking information and operations for a given
        ///     property of this entity.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property. </typeparam>
        /// <param name="propertyName"> The property to access information and operations for. </param>
        /// <returns> An object that exposes change tracking information and operations for the given property. </returns>
        public virtual PropertyEntry<TEntity, TProperty> Property<TProperty>(
            [NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, nameof(propertyName));

            ValidateType<TProperty>(InternalEntry.EntityType.FindProperty(propertyName));

            return new PropertyEntry<TEntity, TProperty>(InternalEntry, propertyName);
        }

        private static void ValidateType<TProperty>(IProperty property)
        {
            if (property != null
                && property.ClrType != typeof(TProperty))
            {
                throw new ArgumentException(
                    CoreStrings.WrongGenericPropertyType(
                        property.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.ClrType.ShortDisplayName(),
                        typeof(TProperty).ShortDisplayName()));
            }
        }
    }
}
