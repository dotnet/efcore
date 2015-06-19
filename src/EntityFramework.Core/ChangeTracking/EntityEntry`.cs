// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
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
        /// <param name="context"> The context that is tracking the entity. </param>
        /// <param name="internalEntry"> The internal entry tracking information about this entity. </param>
        public EntityEntry([NotNull] DbContext context, [NotNull] InternalEntityEntry internalEntry)
            : base(context, internalEntry)
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

            return new PropertyEntry<TEntity, TProperty>(this.GetService(), propertyInfo.Name);
        }

        public virtual PropertyEntry<TEntity, TProperty> Property<TProperty>(
            [NotNull] string propertyName)
        {
            Check.NotNull(propertyName, nameof(propertyName));

            var property = this.GetService().EntityType.GetProperty(propertyName);

            if (property.ClrType != typeof(TProperty))
            {
                throw new ArgumentException(Strings.WrongGenericPropertyType(propertyName, property.EntityType.Name, property.ClrType.Name, typeof(TProperty).Name));
            }

            return new PropertyEntry<TEntity, TProperty>(this.GetService(), propertyName);
        }
    }
}
