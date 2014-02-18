// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class EntityEntry
    {
        private readonly ChangeTrackerEntry _entry;

        internal EntityEntry(ChangeTrackerEntry entry)
        {
            _entry = entry;
        }

        public EntityEntry([NotNull] ChangeTracker changeTracker, [NotNull] object entity)
        {
            Check.NotNull(changeTracker, "changeTracker");
            Check.NotNull(entity, "entity");

            _entry = new ChangeTrackerEntry(changeTracker, entity);
        }

        public virtual object Entity
        {
            get { return _entry.Entity; }
        }

        public virtual EntityKey Key
        {
            get { return _entry.Key; }
        }

        public virtual EntityState State
        {
            get { return _entry.EntityState; }
            set
            {
                Check.IsDefined(value, "value");

                _entry.EntityState = value;
            }
        }

        internal ChangeTrackerEntry Entry
        {
            get { return _entry; }
        }

        public virtual PropertyEntry Property([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, "propertyName");

            return new PropertyEntry(this, propertyName);
        }
    }
}
