// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ArrayPropertyValues : PropertyValues
    {
        private readonly object[] _values;
        private IReadOnlyList<IProperty> _properties;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ArrayPropertyValues([NotNull] InternalEntityEntry internalEntry, [NotNull] object[] values)
            : base(internalEntry) => _values = values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override object ToObject()
            => MaterializerSource.GetMaterializer(EntityType)(
                new MaterializationContext(
                    new ValueBuffer(_values),
                    InternalEntry.StateManager.Context));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void SetValues(object obj)
        {
            Check.NotNull(obj, nameof(obj));

            if (obj.GetType() == EntityType.ClrType)
            {
                for (var i = 0; i < _values.Length; i++)
                {
                    if (!Properties[i].IsShadowProperty)
                    {
                        SetValue(i, ((Property)Properties[i]).Getter.GetClrValue(obj));
                    }
                }
            }
            else
            {
                for (var i = 0; i < _values.Length; i++)
                {
                    var getter = obj.GetType().GetAnyProperty(Properties[i].Name)?.FindGetterProperty();
                    if (getter != null)
                    {
                        SetValue(i, getter.GetValue(obj));
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override PropertyValues Clone()
        {
            var copies = new object[_values.Length];
            Array.Copy(_values, copies, _values.Length);

            return new ArrayPropertyValues(InternalEntry, copies);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void SetValues(PropertyValues propertyValues)
        {
            Check.NotNull(propertyValues, nameof(propertyValues));

            for (var i = 0; i < _values.Length; i++)
            {
                SetValue(i, propertyValues[Properties[i].Name]);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IReadOnlyList<IProperty> Properties
            => _properties ?? (_properties = EntityType.GetProperties().ToList());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override object this[string propertyName]
        {
            get => _values[EntityType.GetProperty(propertyName).GetIndex()];
            set => SetValue(EntityType.GetProperty(propertyName).GetIndex(), value);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override object this[IProperty property]
        {
            get => _values[EntityType.CheckPropertyBelongsToType(property).GetIndex()];
            set => SetValue(EntityType.CheckPropertyBelongsToType(property).GetIndex(), value);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override TValue GetValue<TValue>(string propertyName)
            => (TValue)this[propertyName];

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override TValue GetValue<TValue>(IProperty property)
            => (TValue)this[property];

        private void SetValue(int index, object value)
        {
            var property = Properties[index];

            if (value != null)
            {
                if (!property.ClrType.GetTypeInfo().IsAssignableFrom(value.GetType().GetTypeInfo()))
                {
                    throw new InvalidCastException(
                        CoreStrings.InvalidType(
                            property.Name,
                            property.DeclaringEntityType.DisplayName(),
                            value.GetType().DisplayName(),
                            property.ClrType.DisplayName()));
                }
            }
            else
            {
                if (!property.ClrType.IsNullableType())
                {
                    throw new InvalidOperationException(
                        CoreStrings.ValueCannotBeNull(
                            property.Name,
                            property.DeclaringEntityType.DisplayName(),
                            property.ClrType.DisplayName()));
                }
            }

            _values[index] = value;
        }

        private IEntityMaterializerSource MaterializerSource
            => InternalEntry.StateManager.EntityMaterializerSource;
    }
}
