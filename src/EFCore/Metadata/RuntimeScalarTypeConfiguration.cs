// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a scalar property type.
    /// </summary>
    public sealed class RuntimeScalarTypeConfiguration : AnnotatableBase, IScalarTypeConfiguration
    {
        private readonly ValueConverter? _valueConverter;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public RuntimeScalarTypeConfiguration(
            Type clrType,
            int? maxLength,
            bool? unicode,
            int? precision,
            int? scale,
            Type? providerClrType,
            ValueConverter? valueConverter)
        {
            ClrType = clrType;

            if (maxLength != null)
            {
                SetAnnotation(CoreAnnotationNames.MaxLength, maxLength);
            }

            if (unicode != null)
            {
                SetAnnotation(CoreAnnotationNames.Unicode, unicode);
            }

            if (precision != null)
            {
                SetAnnotation(CoreAnnotationNames.Precision, precision);
            }

            if (scale != null)
            {
                SetAnnotation(CoreAnnotationNames.Scale, scale);
            }

            if (providerClrType != null)
            {
                SetAnnotation(CoreAnnotationNames.ProviderClrType, providerClrType);
            }

            _valueConverter = valueConverter;
        }

        /// <summary>
        ///     Gets the type of value that this property-like object holds.
        /// </summary>
        public Type ClrType { get; }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        int? IScalarTypeConfiguration.GetMaxLength() => (int?)this[CoreAnnotationNames.MaxLength];

        /// <inheritdoc/>
        [DebuggerStepThrough]
        bool? IScalarTypeConfiguration.IsUnicode() => (bool?)this[CoreAnnotationNames.Unicode];

        /// <inheritdoc/>
        [DebuggerStepThrough]
        int? IScalarTypeConfiguration.GetPrecision() => (int?)this[CoreAnnotationNames.Precision];

        /// <inheritdoc/>
        [DebuggerStepThrough]
        int? IScalarTypeConfiguration.GetScale() => (int?)this[CoreAnnotationNames.Scale];

        /// <inheritdoc/>
        [DebuggerStepThrough]
        ValueConverter? IScalarTypeConfiguration.GetValueConverter()
            => _valueConverter;

        /// <inheritdoc/>
        [DebuggerStepThrough]
        Type? IScalarTypeConfiguration.GetProviderClrType()
            => (Type?)this[CoreAnnotationNames.ProviderClrType];
    }
}
