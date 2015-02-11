// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
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
    public class ChangeTracker
    {
        private readonly ChangeDetector _changeDetector;
        private readonly EntityEntryGraphIterator _graphIterator;
        private readonly DbContextService<DbContext> _context;
        private readonly EntityAttacherFactory _attacherFactory;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ChangeTracker()
        {
        }

        public ChangeTracker(
            [NotNull] StateManager stateManager,
            [NotNull] ChangeDetector changeDetector,
            [NotNull] EntityEntryGraphIterator graphIterator,
            [NotNull] DbContextService<DbContext> context,
            [NotNull] EntityAttacherFactory attacherFactory)
        {
            Check.NotNull(stateManager, nameof(stateManager));
            Check.NotNull(changeDetector, nameof(changeDetector));
            Check.NotNull(graphIterator, nameof(graphIterator));
            Check.NotNull(context, nameof(context));
            Check.NotNull(attacherFactory, nameof(attacherFactory));

            StateManager = stateManager;
            _changeDetector = changeDetector;
            _graphIterator = graphIterator;
            _context = context;
            _attacherFactory = attacherFactory;
        }

        public virtual bool AutoDetectChangesEnabled { get; set; } = true;

        public virtual IEnumerable<EntityEntry> Entries()
        {
            TryDetectChanges();

            return StateManager.Entries.Select(e => new EntityEntry(_context.Service, e));
        }

        public virtual IEnumerable<EntityEntry<TEntity>> Entries<TEntity>() where TEntity : class
        {
            TryDetectChanges();

            return StateManager.Entries
                .Where(e => e.Entity is TEntity)
                .Select(e => new EntityEntry<TEntity>(_context.Service, e));
        }

        private void TryDetectChanges()
        {
            if (AutoDetectChangesEnabled)
            {
                DetectChanges();
            }
        }

        public virtual StateManager StateManager { get; }

        public virtual DbContext Context => _context.Service;

        public virtual void DetectChanges()
        {
            _changeDetector.DetectChanges(StateManager);
        }

        public virtual void TrackGraph([NotNull] object rootEntity, [NotNull] Action<EntityEntry> callback)
        {
            Check.NotNull(rootEntity, nameof(rootEntity));
            Check.NotNull(callback, nameof(callback));

            foreach (var entry in _graphIterator.TraverseGraph(rootEntity))
            {
                callback(entry);
            }
        }

        public virtual void TrackGraph([NotNull] object rootEntity)
        {
            Check.NotNull(rootEntity, nameof(rootEntity));

            var attacher = _attacherFactory.CreateForAttach();
            TrackGraph(rootEntity, attacher.HandleEntity);
        }

        public virtual void UpdateGraph([NotNull] object rootEntity)
        {
            Check.NotNull(rootEntity, nameof(rootEntity));

            var attacher = _attacherFactory.CreateForUpdate();
            TrackGraph(rootEntity, attacher.HandleEntity);
        }
    }
}
