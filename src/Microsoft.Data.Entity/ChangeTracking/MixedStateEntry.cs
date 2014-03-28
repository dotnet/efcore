// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class MixedStateEntry : StateEntry
    {
        private object[] _propertyValues;
        private object _entity;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected MixedStateEntry()
        {
        }

        public MixedStateEntry([NotNull] ContextConfiguration configuration, [NotNull] IEntityType entityType, [NotNull] object entity)
            : base(configuration, entityType)
        {
            Check.NotNull(entity, "entity");

            _entity = entity;
            _propertyValues = new object[entityType.ShadowPropertyCount];
        }

        public MixedStateEntry([NotNull] ContextConfiguration configuration, [NotNull] IEntityType entityType, [NotNull] object[] valueBuffer)
            : base(configuration, entityType)
        {
            Check.NotNull(valueBuffer, "valueBuffer");

            _propertyValues = valueBuffer;
        }

        [NotNull]
        public override object Entity
        {
            get { return _entity ?? MaterializeEntity(); }
        }

        private object MaterializeEntity()
        {
            _entity = Configuration.EntityMaterializerSource.GetMaterializer(EntityType)(_propertyValues);

            var properties = EntityType.Properties;
            var shadowValues = new object[EntityType.ShadowPropertyCount];
            for (var i = 0; i < _propertyValues.Length; i++)
            {
                var property = properties[i];
                if (!property.IsClrProperty)
                {
                    shadowValues[property.ShadowIndex] = _propertyValues[i];
                }
            }
            _propertyValues = shadowValues;

            Configuration.StateManager.EntityMaterialized(this);

            return _entity;
        }

        public override object GetPropertyValue(IProperty property)
        {
            Check.NotNull(property, "property");

            if (_entity == null)
            {
                return _propertyValues[property.Index];
            }

            return property.IsClrProperty
                ? Configuration.ClrPropertyGetterSource.GetAccessor(property).GetClrValue(_entity)
                : _propertyValues[property.ShadowIndex];
        }

        public override void SetPropertyValue(IProperty property, object value)
        {
            Check.NotNull(property, "property");

            if (_entity == null)
            {
                _propertyValues[property.Index] = value;
            }
            else if (property.IsClrProperty)
            {
                Configuration.ClrPropertySetterSource.GetAccessor(property).SetClrValue(_entity, value);
            }
            else
            {
                _propertyValues[property.ShadowIndex] = value;
            }
        }
    }
}
