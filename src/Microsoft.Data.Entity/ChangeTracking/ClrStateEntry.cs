// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class ClrStateEntry : StateEntry
    {
        private readonly object _entity;

        // Intended only for creation of test doubles
        internal ClrStateEntry()
        {
        }

        public ClrStateEntry([NotNull] StateManager stateManager, [NotNull] IEntityType entityType, [NotNull] object entity)
            : base(stateManager, entityType)
        {
            Check.NotNull(entity, "entity");

            _entity = entity;
        }

        [NotNull]
        public override object Entity
        {
            get { return _entity; }
        }

        public override object GetPropertyValue(IProperty property)
        {
            Check.NotNull(property, "property");

            return property.GetValue(_entity);
        }

        public override void SetPropertyValue(IProperty property, object value)
        {
            Check.NotNull(property, "property");

            property.SetValue(_entity, value);
        }
    }
}
