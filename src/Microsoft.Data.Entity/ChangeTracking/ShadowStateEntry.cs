// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;
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
            [NotNull] ContextConfiguration configuration,
            [NotNull] IEntityType entityType)
            : base(configuration, entityType, null)
        {
            _propertyValues = new object[entityType.ShadowPropertyCount];
        }

        public ShadowStateEntry(
            [NotNull] ContextConfiguration configuration,
            [NotNull] IEntityType entityType,
            [NotNull] object[] valueBuffer)
            // Can't use valueBuffer optimization for original values because using for current values
            : base(configuration, entityType, null)
        {
            Check.NotNull(valueBuffer, "valueBuffer");

            _propertyValues = valueBuffer;
        }

        public override object GetPropertyValue(IProperty property)
        {
            Check.NotNull(property, "property");

            Contract.Assert(!property.IsClrProperty);

            return _propertyValues[property.ShadowIndex];
        }

        protected override void WritePropertyValue(IProperty property, object value)
        {
            Check.NotNull(property, "property");

            _propertyValues[property.ShadowIndex] = value;
        }
    }
}
