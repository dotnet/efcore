// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
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
    [DebuggerDisplay("{_internalEntityEntry,nq}")]
    public class EntityEntry : IAccessor<InternalEntityEntry>
    {
        private readonly InternalEntityEntry _internalEntityEntry;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityEntry" /> class. Instances of this class are returned from
        ///     methods when using the <see cref="ChangeTracker" /> API and it is not designed to be directly constructed in
        ///     your application code.
        /// </summary>
        /// <param name="context"> The context that is tracking the entity. </param>
        /// <param name="internalEntry"> The internal entry tracking information about this entity. </param>
        public EntityEntry([NotNull] DbContext context, [NotNull] InternalEntityEntry internalEntry)
        {
            Check.NotNull(internalEntry, nameof(internalEntry));
            Check.NotNull(context, nameof(context));

            _internalEntityEntry = internalEntry;
            Context = context;
        }

        /// <summary>
        ///     Gets the entity being tracked by this entry.
        /// </summary>
        public virtual object Entity => _internalEntityEntry.Entity;

        /// <summary>
        ///     <para>
        ///         Gets or sets that state that this entity is being tracked in.
        ///     </para>
        ///     <para>
        ///         When setting the state, the entity will always end up in the specified state. For example, if you
        ///         change the state to <see cref="EntityState.Deleted" /> the entity will be marked for deletion regardless
        ///         of its current state. This is different than calling <see cref="DbSet{TEntity}.Remove" /> where the entity
        ///         will be disconnected (rather than marked for deletion) if it is in the <see cref="EntityState.Added" /> state.
        ///     </para>
        /// </summary>
        public virtual EntityState State
        {
            get { return _internalEntityEntry.EntityState; }
            set
            {
                Check.IsDefined(value, nameof(value));

                _internalEntityEntry.SetEntityState(value);
            }
        }

        /// <summary>
        ///     Gets the internal entry that is tracking information about this entity.
        /// </summary>
        InternalEntityEntry IAccessor<InternalEntityEntry>.Service => _internalEntityEntry;

        /// <summary>
        ///     Gets the context that is tracking the entity.
        /// </summary>
        public virtual DbContext Context { get; }

        /// <summary>
        ///     Gets the metadata the context is using to reason about this entity.
        /// </summary>
        public virtual IEntityType Metadata => _internalEntityEntry.EntityType;

        /// <summary>
        ///     Provides access to change tracking information and operations for a given
        ///     property of this entity.
        /// </summary>
        /// <param name="propertyName"> The property to access information and operations for. </param>
        /// <returns> An object that exposes change tracking information and operations for the given property. </returns>
        public virtual PropertyEntry Property([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, nameof(propertyName));

            return new PropertyEntry(_internalEntityEntry, propertyName);
        }

        /// <summary>
        ///     <para>
        ///         Gets a value indicating if the key values of this entity have been assigned a value.
        ///         False if one or more of the key properties is assigned null or <see cref="IProperty.SentinelValue" />,
        ///         otherwise true.
        ///     </para>
        ///     <para>
        ///         By default <see cref="IProperty.SentinelValue" /> is set to the CLR default for the type of
        ///         the property (i.e. null for string, 0 for int, etc.)
        ///     </para>
        /// </summary>
        public virtual bool IsKeySet => _internalEntityEntry.IsKeySet;
    }
}
