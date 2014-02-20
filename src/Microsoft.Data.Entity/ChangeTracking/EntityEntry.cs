// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class EntityEntry
    {
        private readonly EntityEntryImpl _impl;

        internal EntityEntry(EntityEntryImpl impl)
        {
            _impl = impl;
        }

        public EntityEntry([NotNull] ChangeTracker changeTracker, [NotNull] object entity)
        {
            Check.NotNull(changeTracker, "changeTracker");
            Check.NotNull(entity, "entity");

            _impl = new EntityEntryImpl(changeTracker, entity);
        }

        public virtual object Entity
        {
            get { return _impl.Entity; }
        }

        public virtual EntityKey Key
        {
            get { return _impl.Key; }
        }

        public virtual EntityState State
        {
            get { return _impl.EntityState; }
            set
            {
                Check.IsDefined(value, "value");

                _impl.EntityState = value;
            }
        }

        internal EntityEntryImpl Impl
        {
            get { return _impl; }
        }

        public virtual PropertyEntry Property([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, "propertyName");

            return new PropertyEntry(this, propertyName);
        }
    }
}
