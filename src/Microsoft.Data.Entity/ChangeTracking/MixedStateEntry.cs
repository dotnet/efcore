// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class MixedStateEntry : StateEntry
    {
        private readonly object[] _shadowValues;
        private readonly object _entity;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected MixedStateEntry()
        {
        }

        public MixedStateEntry(
            [NotNull] ContextConfiguration configuration,
            [NotNull] IEntityType entityType,
            [NotNull] object entity)
            : base(configuration, entityType, null)
        {
            Check.NotNull(entity, "entity");

            _entity = entity;
            _shadowValues = new object[entityType.ShadowPropertyCount];
        }

        public MixedStateEntry(
            [NotNull] ContextConfiguration configuration,
            [NotNull] IEntityType entityType,
            [NotNull] object entity,
            [NotNull] object[] valueBuffer)
            : base(configuration, entityType, valueBuffer)
        {
            Check.NotNull(entity, "entity");
            Check.NotNull(valueBuffer, "valueBuffer");

            _entity = entity;
            _shadowValues = ExtractShadowValues(valueBuffer);
        }

        [NotNull]
        public override object Entity
        {
            get { return _entity; }
        }

        public override object GetPropertyValue(IProperty property)
        {
            Check.NotNull(property, "property");

            return property.IsClrProperty
                ? Configuration.ClrPropertyGetterSource.GetAccessor(property).GetClrValue(_entity)
                : _shadowValues[property.ShadowIndex];
        }

        protected override void WritePropertyValue(IProperty property, object value)
        {
            Check.NotNull(property, "property");

            if (property.IsClrProperty)
            {
                Configuration.ClrPropertySetterSource.GetAccessor(property).SetClrValue(_entity, value);
            }
            else
            {
                _shadowValues[property.ShadowIndex] = value;
            }
        }

        private object[] ExtractShadowValues(object[] valueBuffer)
        {
            var shadowValues = new object[EntityType.ShadowPropertyCount];

            var properties = EntityType.Properties;
            for (var i = 0; i < valueBuffer.Length; i++)
            {
                var property = properties[i];
                if (!property.IsClrProperty)
                {
                    var value = valueBuffer[i];
                    shadowValues[property.ShadowIndex] = value == NullSentinel.Value ? null : value;
                }
            }

            return shadowValues;
        }
    }
}
