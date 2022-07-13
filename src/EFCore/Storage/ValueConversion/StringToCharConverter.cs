// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     Converts strings to and from <see cref="char" /> values.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
/// </remarks>
public class StringToCharConverter : StringCharConverter<string, char>
{
    private static readonly ConverterMappingHints DefaultHints = new(size: 1);

    /// <summary>
    ///     Creates a new instance of this converter. This converter preserves order.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    public StringToCharConverter()
        : this(null)
    {
    }

    /// <summary>
    ///     Creates a new instance of this converter. This converter preserves order.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    /// <param name="mappingHints">
    ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
    ///     facets for the converted data.
    /// </param>
    public StringToCharConverter(ConverterMappingHints? mappingHints)
        : base(
            ToChar(),
            ToString(),
            DefaultHints.With(mappingHints))
    {
    }

    /// <summary>
    ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
    /// </summary>
    public static ValueConverterInfo DefaultInfo { get; }
        = new(typeof(string), typeof(char), i => new StringToCharConverter(i.MappingHints), DefaultHints);
}
