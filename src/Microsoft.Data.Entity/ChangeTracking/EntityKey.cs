// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public abstract class EntityKey
    {
        private readonly IEntityType _entityType;

        protected EntityKey([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            _entityType = entityType;
        }

        public virtual object Value
        {
            get { return GetValue(); }
        }

        public virtual IEntityType EntityType
        {
            get { return _entityType; }
        }

        protected abstract object GetValue();
    }
}
