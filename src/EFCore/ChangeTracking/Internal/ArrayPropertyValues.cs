// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ArrayPropertyValues : PropertyValues
    {
        private readonly object?[] _values;
        private IReadOnlyList<IProperty>? _properties;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ArrayPropertyValues(InternalEntityEntry internalEntry, object?[] values)
            : base(internalEntry)
            => _values = values;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override object ToObject()
            => MaterializerSource.GetMaterializer(EntityType)(
                new MaterializationContext(
                    new ValueBuffer(_values),
                    InternalEntry.StateManager.Context));

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
                for (var i = 0; i < _values.Length; i++)
                {
                    if (!Properties[i].IsShadowProperty())
                    {
                        SetValue(i, Properties[i].GetGetter().GetClrValue(obj));
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override PropertyValues Clone()
        {
            var copies = new object[_values.Length];
            Array.Copy(_values, copies, _values.Length);

            return new ArrayPropertyValues(InternalEntry, copies);
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

            for (var i = 0; i < _values.Length; i++)
            {
                SetValue(i, propertyValues[Properties[i].Name]);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override IReadOnlyList<IProperty> Properties
            => _properties ??= EntityType.GetProperties().ToList();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override object? this[string propertyName]
        {
            get => _values[EntityType.GetProperty(propertyName).GetIndex()];
            set => SetValue(EntityType.GetProperty(propertyName).GetIndex(), value);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override object? this[IProperty property]
        {
            get => _values[EntityType.CheckPropertyBelongsToType(property).GetIndex()];
            set => SetValue(EntityType.CheckPropertyBelongsToType(property).GetIndex(), value);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override TValue GetValue<TValue>(string propertyName)
            => (TValue)this[propertyName]!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override TValue GetValue<TValue>(IProperty property)
            => (TValue)this[property]!;

        private void SetValue(int index, object? value)
        {
            var property = Properties[index];

            if (value != null)
            {
                if (!property.ClrType.IsAssignableFrom(value.GetType()))
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
