// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class ShadowStateEntry : StateEntry
    {
        private readonly object[] _propertyValues;

        // Intended only for creation of test doubles
        internal ShadowStateEntry()
        {
        }

        public override object Entity
        {
            get { return null; }
        }

        public ShadowStateEntry([NotNull] StateManager stateManager, [NotNull] IEntityType entityType)
            : base(stateManager, entityType)
        {
            Check.NotNull(stateManager, "stateManager");
            Check.NotNull(entityType, "entityType");

            _propertyValues = new object[entityType.ShadowPropertyCount];
        }

        public override object GetPropertyValue(IProperty property)
        {
            Check.NotNull(property, "property");

            Debug.Assert(!property.HasClrProperty);

            return _propertyValues[property.ShadowIndex];
        }

        public override void SetPropertyValue(IProperty property, object value)
        {
            Check.NotNull(property, "property");

            _propertyValues[property.ShadowIndex] = value;
        }
    }
}
