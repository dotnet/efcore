// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents scalar type configuration.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public sealed class RuntimeTypeMappingConfiguration : RuntimeAnnotatableBase, ITypeMappingConfiguration
{
    private readonly ValueConverter? _valueConverter;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public RuntimeTypeMappingConfiguration(
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

    /// <inheritdoc />
    [DebuggerStepThrough]
    int? ITypeMappingConfiguration.GetMaxLength()
        => (int?)this[CoreAnnotationNames.MaxLength];

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool? ITypeMappingConfiguration.IsUnicode()
        => (bool?)this[CoreAnnotationNames.Unicode];

    /// <inheritdoc />
    [DebuggerStepThrough]
    int? ITypeMappingConfiguration.GetPrecision()
        => (int?)this[CoreAnnotationNames.Precision];

    /// <inheritdoc />
    [DebuggerStepThrough]
    int? ITypeMappingConfiguration.GetScale()
        => (int?)this[CoreAnnotationNames.Scale];

    /// <inheritdoc />
    [DebuggerStepThrough]
    ValueConverter? ITypeMappingConfiguration.GetValueConverter()
        => _valueConverter;

    /// <inheritdoc />
    [DebuggerStepThrough]
    Type? ITypeMappingConfiguration.GetProviderClrType()
        => (Type?)this[CoreAnnotationNames.ProviderClrType];
}
