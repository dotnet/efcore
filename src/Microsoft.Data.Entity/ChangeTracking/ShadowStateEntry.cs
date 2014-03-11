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

            Contract.Assert(!property.HasClrProperty);

            return _propertyValues[property.ShadowIndex];
        }

        public override void SetPropertyValue(IProperty property, object value)
        {
            Check.NotNull(property, "property");

            _propertyValues[property.ShadowIndex] = value;
        }
    }
}
