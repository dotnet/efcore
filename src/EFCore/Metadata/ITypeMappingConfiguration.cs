// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents the configuration for a scalar type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface ITypeMappingConfiguration : IAnnotatable
{
    /// <summary>
    ///     Gets the type configured by this object.
    /// </summary>
    Type ClrType { get; }

    /// <summary>
    ///     Gets the maximum length of data that is allowed in this property. For example, if the property is a <see cref="string" />
    ///     then this is the maximum number of characters.
    /// </summary>
    /// <returns>The maximum length, or <see langword="null" /> if none is defined.</returns>
    int? GetMaxLength();

    /// <summary>
    ///     Gets the precision of data that is allowed in this property.
    ///     For example, if the property is a <see cref="decimal" /> then this is the maximum number of digits.
    /// </summary>
    /// <returns>The precision, or <see langword="null" /> if none is defined.</returns>
    int? GetPrecision();

    /// <summary>
    ///     Gets the scale of data that is allowed in this property.
    ///     For example, if the property is a <see cref="decimal" /> then this is the maximum number of decimal places.
    /// </summary>
    /// <returns>The scale, or <see langword="null" /> if none is defined.</returns>
    int? GetScale();

    /// <summary>
    ///     Gets a value indicating whether or not the property can persist Unicode characters.
    /// </summary>
    /// <returns>The Unicode setting, or <see langword="null" /> if none is defined.</returns>
    bool? IsUnicode();

    /// <summary>
    ///     Gets the custom <see cref="ValueConverter" /> set for this property.
    /// </summary>
    /// <returns>The converter, or <see langword="null" /> if none has been set.</returns>
    ValueConverter? GetValueConverter();

    /// <summary>
    ///     Gets the type that the property value will be converted to before being sent to the database provider.
    /// </summary>
    /// <returns>The provider type, or <see langword="null" /> if none has been set.</returns>
    Type? GetProviderClrType();
}
