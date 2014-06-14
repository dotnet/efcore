// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
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
            [NotNull] DbContextConfiguration configuration,
            [NotNull] IEntityType entityType,
            [NotNull] object entity)
            : base(configuration, entityType)
        {
            Check.NotNull(entity, "entity");

            _entity = entity;
            _shadowValues = new object[entityType.ShadowPropertyCount];
        }

        public MixedStateEntry(
            [NotNull] DbContextConfiguration configuration,
            [NotNull] IEntityType entityType,
            [NotNull] object entity,
            [NotNull] IValueReader valueReader)
            : base(configuration, entityType)
        {
            Check.NotNull(entity, "entity");
            Check.NotNull(valueReader, "valueReader");

            _entity = entity;
            _shadowValues = ExtractShadowValues(valueReader);
        }

        [NotNull]
        public override object Entity
        {
            get { return _entity; }
        }

        protected override object ReadPropertyValue(IPropertyBase propertyBase)
        {
            Check.NotNull(propertyBase, "propertyBase");

            var property = propertyBase as IProperty;

            return property == null || property.IsClrProperty
                ? base.ReadPropertyValue(propertyBase)
                : _shadowValues[property.ShadowIndex];
        }

        protected override void WritePropertyValue(IPropertyBase propertyBase, object value)
        {
            Check.NotNull(propertyBase, "propertyBase");

            var property = propertyBase as IProperty;

            if (property == null || property.IsClrProperty)
            {
                base.WritePropertyValue(propertyBase, value);
            }
            else
            {
                _shadowValues[property.ShadowIndex] = value;
            }
        }

        private object[] ExtractShadowValues(IValueReader valueReader)
        {
            var shadowValues = new object[EntityType.ShadowPropertyCount];

            var properties = EntityType.Properties;
            for (var i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                if (!property.IsClrProperty)
                {
                    // TODO: Consider using strongly typed ReadValue instead of always object
                    shadowValues[property.ShadowIndex] = valueReader.IsNull(i) ? null : valueReader.ReadValue<object>(i);
                }
            }

            return shadowValues;
        }
    }
}
