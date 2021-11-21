// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     Converts arrays of bytes to and from strings.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
/// </remarks>
public class BytesToStringConverter : ValueConverter<byte[]?, string?>
{
    /// <summary>
    ///     Creates a new instance of this converter.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    public BytesToStringConverter()
        : this(null)
    {
    }

    /// <summary>
    ///     Creates a new instance of this converter.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    /// <param name="mappingHints">
    ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
    ///     facets for the converted data.
    /// </param>
    public BytesToStringConverter(ConverterMappingHints? mappingHints)
        : base(
            v => Convert.ToBase64String(v!),
            v => Convert.FromBase64String(v!),
            mappingHints)
    {
    }

    /// <summary>
    ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
    /// </summary>
    public static ValueConverterInfo DefaultInfo { get; }
        = new(typeof(byte[]), typeof(string), i => new BytesToStringConverter(i.MappingHints));
}
