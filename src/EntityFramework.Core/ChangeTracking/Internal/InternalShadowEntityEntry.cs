// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class InternalShadowEntityEntry : InternalEntityEntry
    {
        private readonly object[] _propertyValues;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected InternalShadowEntityEntry()
        {
        }

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
            [NotNull] IValueReader valueReader)
            : base(stateManager, entityType, metadataServices)
        {
            _propertyValues = new object[valueReader.Count];

            var index = 0;
            foreach (var property in entityType.GetProperties())
            {
                _propertyValues[index++] = metadataServices.ReadValueFromReader(valueReader, property);
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
