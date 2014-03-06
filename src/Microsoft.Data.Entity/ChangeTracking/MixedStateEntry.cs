// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class MixedStateEntry : StateEntry
    {
        private readonly object[] _propertyValues;
        private readonly object _entity;

        // Intended only for creation of test doubles
        internal MixedStateEntry()
        {
        }

        public MixedStateEntry([NotNull] StateManager stateManager, [NotNull] IEntityType entityType, [NotNull] object entity)
            : base(stateManager, entityType)
        {
            Check.NotNull(entity, "entity");

            _entity = entity;
            _propertyValues = new object[entityType.ShadowPropertyCount];
        }

        [NotNull]
        public override object Entity
        {
            get { return _entity; }
        }

        public override object GetPropertyValue(IProperty property)
        {
            Check.NotNull(property, "property");

            return property.HasClrProperty ? property.GetValue(_entity) : _propertyValues[property.ShadowIndex];
        }

        public override void SetPropertyValue(IProperty property, object value)
        {
            Check.NotNull(property, "property");

            if (property.HasClrProperty)
            {
                property.SetValue(_entity, value);
            }
            else
            {
                _propertyValues[property.ShadowIndex] = value;
            }
        }
    }
}
