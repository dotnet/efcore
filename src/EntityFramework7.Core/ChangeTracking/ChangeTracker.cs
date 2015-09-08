// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    // This is the app-developer facing public API to the change tracker
    /// <summary>
    ///     Provides access to change tracking information and operations for entity instances the context is tracking.
    ///     Instances of this class are typically obtained from <see cref="DbContext.ChangeTracker" /> and it is not designed
    ///     to be directly constructed in your application code.
    /// </summary>
    public class ChangeTracker : IAccessor<IStateManager>
    {
        private readonly IStateManager _stateManager;
        private readonly IChangeDetector _changeDetector;
        private readonly IEntityEntryGraphIterator _graphIterator;
        private readonly DbContext _context;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChangeTracker" /> class. Instances of this class are typically
        ///     obtained from <see cref="DbContext.ChangeTracker" /> and it is not designed to be directly constructed
        ///     in your application code.
        /// </summary>
        /// <param name="stateManager"> The internal state manager being used to store information about tracked entities. </param>
        /// <param name="changeDetector"> The internal change detector used to identify changes in tracked entities. </param>
        /// <param name="graphIterator"> The internal graph iterator used to traverse graphs of entities. </param>
        /// <param name="context"> The context this change tracker belongs to. </param>
        public ChangeTracker(
            [NotNull] IStateManager stateManager,
            [NotNull] IChangeDetector changeDetector,
            [NotNull] IEntityEntryGraphIterator graphIterator,
            [NotNull] DbContext context)
        {
            Check.NotNull(stateManager, nameof(stateManager));
            Check.NotNull(changeDetector, nameof(changeDetector));
            Check.NotNull(graphIterator, nameof(graphIterator));
            Check.NotNull(context, nameof(context));

            _stateManager = stateManager;
            _changeDetector = changeDetector;
            _graphIterator = graphIterator;
            _context = context;
        }

        /// <summary>
        ///     <para>
        ///         Gets or sets a value indicating whether the <see cref="ChangeTracker.DetectChanges()" /> method is called
        ///         automatically by methods of <see cref="DbContext" /> and related classes.
        ///     </para>
        ///     <para>
        ///         The default value is true. This ensures the context is aware of any changes to tracked entity instances
        ///         before performing operations such as <see cref="DbContext.SaveChanges()" /> or returning change tracking
        ///         information. If you disable automatic detect changes then you must ensure that
        ///         <see cref="DetectChanges" /> is called when entity instances have been modified.
        ///         Failure to do so may result in some changes not being persisted during
        ///         <see cref="DbContext.SaveChanges()" /> or out-of-date change tracking information being returned.
        ///     </para>
        /// </summary>
        public virtual bool AutoDetectChangesEnabled { get; set; } = true;

        /// <summary>
        ///     Gets an <see cref="EntityEntry" /> for each entity being tracked by the context.
        ///     The entries provide access to change tracking information and operations for each entity.
        /// </summary>
        /// <returns> An entry for each entity being tracked. </returns>
        public virtual IEnumerable<EntityEntry> Entries()
        {
            TryDetectChanges();

            return _stateManager.Entries.Select(e => new EntityEntry(_context, e));
        }

        /// <summary>
        ///     Gets an <see cref="EntityEntry" /> for all entities of a given type being tracked by the context.
        ///     The entries provide access to change tracking information and operations for each entity.
        /// </summary>
        /// <typeparam name="TEntity"> The type of entities to get entries for. </typeparam>
        /// <returns> An entry for each entity of the given type that is being tracked. </returns>
        public virtual IEnumerable<EntityEntry<TEntity>> Entries<TEntity>() where TEntity : class
        {
            TryDetectChanges();

            return _stateManager.Entries
                .Where(e => e.Entity is TEntity)
                .Select(e => new EntityEntry<TEntity>(_context, e));
        }

        private void TryDetectChanges()
        {
            if (AutoDetectChangesEnabled)
            {
                DetectChanges();
            }
        }

        /// <summary>
        ///     Gets the internal state manager being used to store information about tracked entities.
        /// </summary>
        IStateManager IAccessor<IStateManager>.Service => _stateManager;

        /// <summary>
        ///     Gets the context this change tracker belongs to.
        /// </summary>
        public virtual DbContext Context => _context;

        /// <summary>
        ///     Scans the tracked entity instances to detect any changes made to the instance data. <see cref="DetectChanges" />
        ///     is usually called automatically by the context when up-to-date information is required (before
        ///     <see cref="DbContext.SaveChanges()" />
        ///     and when returning change tracking information). You typically only need to call this method if you have disabled
        ///     <see cref="AutoDetectChangesEnabled" />.
        /// </summary>
        public virtual void DetectChanges() => _changeDetector.DetectChanges(_stateManager);

        /// <summary>
        ///     Accepts all changes made to entities in the context. It will be assumed that the tracked entities
        ///     represent the current state of the database.
        /// </summary>
        public virtual void AcceptAllChanges() => _stateManager.AcceptAllChanges();

        /// <summary>
        ///     <para>
        ///         Begins tracking an entity and any entities that are reachable by traversing it's navigation properties.
        ///         Traversal is recursive so the navigation properties of any discovered entities will also be scanned.
        ///         The specified <paramref name="callback" /> is called for each discovered entity and must set the
        ///         <see cref="EntityEntry.State" /> that each entity should be tracked in. If no state is set, the entity
        ///         remains untracked.
        ///     </para>
        ///     <para>
        ///         This method is designed for use in disconnected scenarios where entities are retrieved using one instance of
        ///         the contextand then changes are saved using a different instance of the context. An example of this is a
        ///         web service where one servicecall retrieves entities from the database and another service call persists
        ///         any changes to the entities. Each service call uses a new instance of the context that is disposed when the
        ///         call is complete.
        ///     </para>
        ///     <para>
        ///         If an entity is discovered that is already tracked by the context, that entity is not processed (and it's
        ///         navigation properties are not traversed).
        ///     </para>
        /// </summary>
        /// <param name="rootEntity"> The entity to begin traversal from. </param>
        /// <param name="callback">
        ///     An action to configure the change tracking information for each entity. For the entity to begin being tracked,
        ///     the <see cref="EntityEntry.State" /> must be set.
        /// </param>
        public virtual void TrackGraph([NotNull] object rootEntity, [NotNull] Action<EntityEntry> callback)
        {
            Check.NotNull(rootEntity, nameof(rootEntity));
            Check.NotNull(callback, nameof(callback));

            foreach (var entry in _graphIterator.TraverseGraph(rootEntity))
            {
                callback(entry);
            }
        }
    }
}
