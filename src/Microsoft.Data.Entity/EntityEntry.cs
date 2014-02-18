// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class EntityEntry
    {
        private readonly object _entity;
        private EntityState _entityState;

        public EntityEntry([NotNull] object entity)
        {
            Check.NotNull(entity, "entity");

            _entity = entity;
        }

        public virtual object Entity
        {
            get { return _entity; }
        }

        public virtual EntityState EntityState
        {
            get { return _entityState; }
            set
            {
                Check.IsDefined(value, "value");

                _entityState = value;
            }
        }
    }
}
