// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.NetworkInformation;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     Converts a <see cref="PhysicalAddress" /> to and from a <see cref="string" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
/// </remarks>
public class PhysicalAddressToStringConverter : ValueConverter<PhysicalAddress?, string?>
{
    private static readonly ConverterMappingHints DefaultHints = new(size: 20);

    /// <summary>
    ///     Creates a new instance of this converter.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    public PhysicalAddressToStringConverter()
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
    public PhysicalAddressToStringConverter(ConverterMappingHints? mappingHints)
        : base(
            ToString(),
            ToPhysicalAddress(),
            DefaultHints.With(mappingHints))
    {
    }

    /// <summary>
    ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
    /// </summary>
    public static ValueConverterInfo DefaultInfo { get; }
        = new(typeof(PhysicalAddress), typeof(string), i => new PhysicalAddressToStringConverter(i.MappingHints), DefaultHints);

    private static new Expression<Func<PhysicalAddress?, string?>> ToString()
        => v => v!.ToString();

    private static Expression<Func<string?, PhysicalAddress?>> ToPhysicalAddress()
        => v => PhysicalAddress.Parse(v!);
}
