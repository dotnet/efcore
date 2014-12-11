// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
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
            [NotNull] DbContextService<DbContext> context)
        {
            Check.NotNull(stateManager, "stateManager");
            Check.NotNull(changeDetector, "changeDetector");
            Check.NotNull(graphIterator, "graphIterator");
            Check.NotNull(context, "context");

            StateManager = stateManager;
            _changeDetector = changeDetector;
            _graphIterator = graphIterator;
            _context = context;
        }

        public virtual IEnumerable<EntityEntry> Entries()
        {
            return StateManager.StateEntries.Select(e => new EntityEntry(_context.Service, e));
        }

        public virtual IEnumerable<EntityEntry<TEntity>> Entries<TEntity>()
        {
            return StateManager.StateEntries
                .Where(e => e.Entity is TEntity)
                .Select(e => new EntityEntry<TEntity>(_context.Service, e));
        }

        public virtual StateManager StateManager { get; }

        public virtual DbContext Context => _context.Service;

        public virtual bool DetectChanges()
        {
            return _changeDetector.DetectChanges(StateManager);
        }

        public virtual void AttachGraph([NotNull] object rootEntity, [NotNull] Action<EntityEntry> callback)
        {
            Check.NotNull(rootEntity, "rootEntity");
            Check.NotNull(callback, "callback");

            foreach (var entry in _graphIterator.TraverseGraph(rootEntity))
            {
                callback(entry);
            }
        }

        public virtual async Task AttachGraphAsync(
            [NotNull] object rootEntity,
            [NotNull] Func<EntityEntry, Task> callback,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(rootEntity, "rootEntity");
            Check.NotNull(callback, "callback");

            foreach (var entry in _graphIterator.TraverseGraph(rootEntity))
            {
                await callback(entry).WithCurrentCulture();
            }
        }
    }
}
