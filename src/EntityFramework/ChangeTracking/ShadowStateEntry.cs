// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class ShadowStateEntry : StateEntry
    {
        private readonly object[] _propertyValues;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ShadowStateEntry()
        {
        }

        public override object Entity
        {
            get { return null; }
        }

        public ShadowStateEntry(
            [NotNull] DbContextConfiguration configuration,
            [NotNull] IEntityType entityType,
            [NotNull] StateEntryMetadataServices metadataServices)
            : base(configuration, entityType, metadataServices)
        {
            _propertyValues = new object[entityType.ShadowPropertyCount];
        }

        public ShadowStateEntry(
            [NotNull] DbContextConfiguration configuration,
            [NotNull] IEntityType entityType,
            [NotNull] StateEntryMetadataServices metadataServices,
            [NotNull] IValueReader valueReader)
            : base(configuration, entityType, metadataServices)
        {
            Check.NotNull(valueReader, "valueReader");

            _propertyValues = new object[valueReader.Count];

            for (var i = 0; i < valueReader.Count; i++)
            {
                // TODO: Consider using strongly typed ReadValue instead of always object
                // Issue #738
                _propertyValues[i] = valueReader.IsNull(i) ? null : valueReader.ReadValue<object>(i);
            }
        }

        protected override object ReadPropertyValue(IPropertyBase propertyBase)
        {
            Check.NotNull(propertyBase, "propertyBase");

            var property = propertyBase as IProperty;
            Contract.Assert(property != null && property.IsShadowProperty);

            return _propertyValues[property.ShadowIndex];
        }

        protected override void WritePropertyValue(IPropertyBase propertyBase, object value)
        {
            Check.NotNull(propertyBase, "propertyBase");

            var property = propertyBase as IProperty;
            Contract.Assert(property != null && property.IsShadowProperty);

            _propertyValues[property.ShadowIndex] = value;
        }
    }
}
