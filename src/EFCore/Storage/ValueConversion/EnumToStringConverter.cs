// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     Converts enum values to and from their string representation.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
/// </remarks>
public class EnumToStringConverter<TEnum> : StringEnumConverter<TEnum, string, TEnum>
    where TEnum : struct, Enum
{
    /// <summary>
    ///     Creates a new instance of this converter. This converter does not preserve order.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    public EnumToStringConverter()
        : this(null)
    {
    }

    /// <summary>
    ///     Creates a new instance of this converter. This converter does not preserve order.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    /// <param name="mappingHints">
    ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
    ///     facets for the converted data.
    /// </param>
    public EnumToStringConverter(ConverterMappingHints? mappingHints)
        : base(
            ToString(),
            ToEnum(),
            mappingHints)
    {
    }

    /// <summary>
    ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
    /// </summary>
    public static ValueConverterInfo DefaultInfo { get; }
        = new(typeof(TEnum), typeof(string), i => new EnumToStringConverter<TEnum>(i.MappingHints));
}
