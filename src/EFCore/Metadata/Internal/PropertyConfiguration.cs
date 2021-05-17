// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class PropertyConfiguration : AnnotatableBase
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public PropertyConfiguration(Type clrType)
        {
            Check.NotNull(clrType, nameof(clrType));

            ClrType = clrType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Type ClrType { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Apply(IMutableProperty property)
        {
            foreach (var annotation in GetAnnotations())
            {
                switch (annotation.Name)
                {
                    case CoreAnnotationNames.MaxLength:
                        property.SetMaxLength((int?)annotation.Value);

                        break;
                    case CoreAnnotationNames.Unicode:
                        property.SetIsUnicode((bool?)annotation.Value);

                        break;
                    case CoreAnnotationNames.Precision:
                        property.SetPrecision((int?)annotation.Value);

                        break;
                    case CoreAnnotationNames.Scale:
                        property.SetScale((int?)annotation.Value);

                        break;
                    case CoreAnnotationNames.ProviderClrType:
                        property.SetProviderClrType((Type?)annotation.Value);

                        break;
                    case CoreAnnotationNames.ValueConverterType:
                        property.SetValueConverter((Type?)annotation.Value);

                        break;
                    case CoreAnnotationNames.ValueComparerType:
                        property.SetValueComparer((Type?)annotation.Value);

                        break;
                    default:
                        if (!CoreAnnotationNames.AllNames.Contains(annotation.Name))
                        {
                            property.SetAnnotation(annotation.Name, annotation.Value);
                        }
                        break;
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetMaxLength(int? maxLength)
        {
            if (maxLength != null
                && maxLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLength));
            }

            this[CoreAnnotationNames.MaxLength] = maxLength;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetIsUnicode(bool? unicode)
            => this[CoreAnnotationNames.Unicode] = unicode;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetPrecision(int? precision)
        {
            if (precision != null && precision < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(precision));
            }

            this[CoreAnnotationNames.Precision] = precision;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetScale(int? scale)
        {
            if (scale != null && scale < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scale));
            }

            this[CoreAnnotationNames.Scale] = scale;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetProviderClrType(Type? providerClrType)
            => this[CoreAnnotationNames.ProviderClrType] = providerClrType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetValueConverter(Type? converterType)
        {
            if (converterType != null)
            {
                if (!typeof(ValueConverter).IsAssignableFrom(converterType))
                {
                    throw new InvalidOperationException(
                        CoreStrings.BadValueConverterType(converterType.ShortDisplayName(), typeof(ValueConverter).ShortDisplayName()));
                }
            }

            this[CoreAnnotationNames.ValueConverterType] = converterType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetValueComparer(Type? comparerType)
        {
            if (comparerType != null)
            {
                if (!typeof(ValueComparer).IsAssignableFrom(comparerType))
                {
                    throw new InvalidOperationException(
                        CoreStrings.BadValueComparerType(comparerType.ShortDisplayName(), typeof(ValueComparer).ShortDisplayName()));
                }
            }

            this[CoreAnnotationNames.ValueComparerType] = comparerType;
        }
    }
}
