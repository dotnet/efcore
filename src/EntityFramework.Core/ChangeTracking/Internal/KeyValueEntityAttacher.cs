// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class KeyValueEntityAttacher : IEntityAttacher
    {
        private readonly bool _updateExistingEntities;

        public KeyValueEntityAttacher(bool updateExistingEntities)
        {
            _updateExistingEntities = updateExistingEntities;
        }

        public virtual void HandleEntity(EntityEntry entry)
            => entry.InternalEntry.SetEntityState(DetermineState(entry), acceptChanges: true);

        public virtual EntityState DetermineState([NotNull] EntityEntry entry)
            => entry.IsKeySet
                ? (_updateExistingEntities ? EntityState.Modified : EntityState.Unchanged)
                : EntityState.Added;
    }
}
