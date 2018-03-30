// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public abstract class EntryPropertyValues : PropertyValues
    {
        private IReadOnlyList<IProperty> _properties;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected EntryPropertyValues([NotNull] InternalEntityEntry internalEntry)
            : base(internalEntry)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override object ToObject()
            => Clone().ToObject();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void SetValues(object obj)
        {
            Check.NotNull(obj, nameof(obj));

            if (obj.GetType() == EntityType.ClrType)
            {
                foreach (var property in Properties.Where(p => !p.IsShadowProperty))
                {
                    SetValueInternal(property, ((Property)property).Getter.GetClrValue(obj));
                }
            }
            else
            {
                foreach (var property in Properties)
                {
                    var getter = obj.GetType().GetAnyProperty(property.Name)?.FindGetterProperty();
                    if (getter != null)
                    {
                        SetValueInternal(property, getter.GetValue(obj));
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
            var values = new object[Properties.Count];
            for (var i = 0; i < values.Length; i++)
            {
                values[i] = GetValueInternal(Properties[i]);
            }

            return new ArrayPropertyValues(InternalEntry, values);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void SetValues(PropertyValues propertyValues)
        {
            Check.NotNull(propertyValues, nameof(propertyValues));

            foreach (var property in Properties)
            {
                SetValueInternal(property, propertyValues[property.Name]);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IReadOnlyList<IProperty> Properties
        {
            [DebuggerStepThrough] get => _properties ?? (_properties = EntityType.GetProperties().ToList());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override object this[string propertyName]
        {
            get => GetValueInternal(EntityType.GetProperty(propertyName));
            set => SetValueInternal(EntityType.GetProperty(propertyName), value);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override object this[IProperty property]
        {
            get => GetValueInternal(EntityType.CheckPropertyBelongsToType(property));
            set => SetValueInternal(EntityType.CheckPropertyBelongsToType(property), value);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected abstract void SetValueInternal([NotNull] IProperty property, [CanBeNull] object value);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected abstract object GetValueInternal([NotNull] IProperty property);
    }
}
