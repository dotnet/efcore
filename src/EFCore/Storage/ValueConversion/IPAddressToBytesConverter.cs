// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     Converts a <see cref="IPAddress" /> to and from a <see cref="byte" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
/// </remarks>
// ReSharper disable once InconsistentNaming
public class IPAddressToBytesConverter : ValueConverter<IPAddress?, byte[]?>
{
    private static readonly ConverterMappingHints DefaultHints = new(size: 16);

    /// <summary>
    ///     Creates a new instance of this converter.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    public IPAddressToBytesConverter()
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
    public IPAddressToBytesConverter(ConverterMappingHints? mappingHints)
        : base(
            v => v!.GetAddressBytes(),
            v => new IPAddress(v!),
            DefaultHints.With(mappingHints))
    {
    }

    /// <summary>
    ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
    /// </summary>
    public static ValueConverterInfo DefaultInfo { get; }
        = new(typeof(IPAddress), typeof(byte[]), i => new IPAddressToBytesConverter(i.MappingHints), DefaultHints);
}
