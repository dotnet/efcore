// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract class EntryPropertyValues : PropertyValues
    {
        private IReadOnlyList<IProperty>? _properties;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected EntryPropertyValues(InternalEntityEntry internalEntry)
            : base(internalEntry)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override object ToObject()
            => Clone().ToObject();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void SetValues(object obj)
        {
            Check.NotNull(obj, nameof(obj));

            if (obj.GetType() == EntityType.ClrType)
            {
                foreach (var property in Properties.Where(p => !p.IsShadowProperty()))
                {
                    SetValueInternal(property, property.GetGetter().GetClrValue(obj));
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override PropertyValues Clone()
        {
            var values = new object?[Properties.Count];
            for (var i = 0; i < values.Length; i++)
            {
                values[i] = GetValueInternal(Properties[i]);
            }

            return new ArrayPropertyValues(InternalEntry, values);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override IReadOnlyList<IProperty> Properties
        {
            [DebuggerStepThrough] get => _properties ??= EntityType.GetProperties().ToList();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override object? this[string propertyName]
        {
            get => GetValueInternal(EntityType.GetProperty(propertyName));
            set => SetValueInternal(EntityType.GetProperty(propertyName), value);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override object? this[IProperty property]
        {
            get => GetValueInternal(EntityType.CheckPropertyBelongsToType(property));
            set => SetValueInternal(EntityType.CheckPropertyBelongsToType(property), value);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract void SetValueInternal(IProperty property, object? value);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract object? GetValueInternal(IProperty property);
    }
}
