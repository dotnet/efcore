// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     Converts a <see cref="Uri" /> to and from a <see cref="string" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
/// </remarks>
public class UriToStringConverter : StringUriConverter<Uri?, string?>
{
    /// <summary>
    ///     Creates a new instance of this converter. This converter preserves order.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    public UriToStringConverter()
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
    public UriToStringConverter(ConverterMappingHints? mappingHints)
        : base(
            ToString(),
            ToUri(),
            mappingHints)
    {
    }

    /// <summary>
    ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
    /// </summary>
    public static ValueConverterInfo DefaultInfo { get; }
        = new(typeof(Uri), typeof(string), i => new UriToStringConverter(i.MappingHints));
}
