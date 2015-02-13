// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class EntityEntryGraphIterator
    {
        private readonly DbContextService<DbContext> _context;
        private readonly StateManager _stateManager;

        public EntityEntryGraphIterator(
            [NotNull] DbContextService<DbContext> context,
            [NotNull] StateManager stateManager)
        {
            _context = context;
            _stateManager = stateManager;
        }

        public virtual IEnumerable<EntityEntry> TraverseGraph([NotNull] object entity)
        {
            var entry = new EntityEntry(_context.Service, _stateManager.GetOrCreateEntry(entity));

            if (entry.State != EntityState.Detached)
            {
                yield break;
            }

            yield return entry;

            if (entry.State != EntityState.Detached)
            {
                var navigations = entry.InternalEntry.EntityType.Navigations;

                foreach (var navigation in navigations)
                {
                    var navigationValue = entry.InternalEntry[navigation];

                    if (navigationValue != null)
                    {
                        if (navigation.IsCollection())
                        {
                            foreach (var relatedEntity in (IEnumerable)navigationValue)
                            {
                                foreach (var relatedEntry in TraverseGraph(relatedEntity))
                                {
                                    yield return relatedEntry;
                                }
                            }
                        }
                        else
                        {
                            foreach (var relatedEntry in TraverseGraph(navigationValue))
                            {
                                yield return relatedEntry;
                            }
                        }
                    }
                }
            }
        }
    }
}
