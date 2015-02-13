// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class KeyValueEntityTracker
    {
        private readonly bool _updateExistingEntities;

        public KeyValueEntityTracker(bool updateExistingEntities)
        {
            _updateExistingEntities = updateExistingEntities;
        }

        public virtual void TrackEntity([NotNull] EntityEntry entry)
        {
            Check.NotNull(entry, nameof(entry));

            entry.InternalEntry.SetEntityState(DetermineState(entry), acceptChanges: true);
        }

        public virtual EntityState DetermineState([NotNull] EntityEntry entry)
        {
            Check.NotNull(entry, nameof(entry));

            return entry.IsKeySet
                ? (_updateExistingEntities ? EntityState.Modified : EntityState.Unchanged)
                : EntityState.Added;
        }
    }
}
