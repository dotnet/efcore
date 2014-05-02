// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

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
            : base(configuration, entityType)
        {
            _propertyValues = new object[entityType.ShadowPropertyCount];
        }

        public ShadowStateEntry(
            [NotNull] ContextConfiguration configuration,
            [NotNull] IEntityType entityType,
            [NotNull] IValueReader valueReader)
            : base(configuration, entityType)
        {
            Check.NotNull(valueReader, "valueReader");

            _propertyValues = new object[valueReader.Count];

            for (var i = 0; i < valueReader.Count; i++)
            {
                // TODO: Consider using strongly typed ReadValue instead of always object
                _propertyValues[i] = valueReader.IsNull(i) ? null : valueReader.ReadValue<object>(i);
            }
        }

        protected override object ReadPropertyValue(IProperty property)
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
