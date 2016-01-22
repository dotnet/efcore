// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
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
        ///     Initializes a new instance of the <see cref="EntityEntry{TEntity}" /> class. Instances of this class are returned
        ///     from methods when using the <see cref="ChangeTracker" /> API and it is not designed to be directly
        ///     constructed in your application code.
        /// </summary>
        /// <param name="internalEntry"> The internal entry tracking information about this entity. </param>
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

            var propertyInfo = propertyExpression.GetPropertyAccess();

            return new PropertyEntry<TEntity, TProperty>(this.GetInfrastructure(), propertyInfo.Name);
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
            Check.NotNull(propertyName, nameof(propertyName));

            var property = this.GetInfrastructure().EntityType.FindProperty(propertyName);

            if ((property != null)
                && (property.ClrType != typeof(TProperty)))
            {
                throw new ArgumentException(CoreStrings.WrongGenericPropertyType(propertyName, property.DeclaringEntityType.Name, property.ClrType.Name, typeof(TProperty).Name));
            }

            return new PropertyEntry<TEntity, TProperty>(this.GetInfrastructure(), propertyName);
        }
    }
}
