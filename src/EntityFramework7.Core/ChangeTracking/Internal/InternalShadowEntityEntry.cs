// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class InternalShadowEntityEntry : InternalEntityEntry
    {
        private readonly object[] _propertyValues;

        public override object Entity => null;

        public InternalShadowEntityEntry(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityType entityType,
            [NotNull] IEntityEntryMetadataServices metadataServices)
            : base(stateManager, entityType, metadataServices)
        {
            _propertyValues = new object[entityType.ShadowPropertyCount()];
        }

        public InternalShadowEntityEntry(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityType entityType,
            [NotNull] IEntityEntryMetadataServices metadataServices,
            ValueBuffer valueBuffer)
            : base(stateManager, entityType, metadataServices)
        {
            _propertyValues = new object[valueBuffer.Count];

            var index = 0;
            foreach (var property in entityType.GetProperties())
            {
                _propertyValues[index++] = valueBuffer[property.Index];
            }
        }

        protected override object ReadPropertyValue(IPropertyBase propertyBase)
        {
            var property = propertyBase as IProperty;
            Debug.Assert(property != null && property.IsShadowProperty);

            return _propertyValues[property.GetShadowIndex()];
        }

        protected override void WritePropertyValue(IPropertyBase propertyBase, object value)
        {
            var property = propertyBase as IProperty;
            Debug.Assert(property != null && property.IsShadowProperty);

            _propertyValues[property.GetShadowIndex()] = value;
        }
    }
}
