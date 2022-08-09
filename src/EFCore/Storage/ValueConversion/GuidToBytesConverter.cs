// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     Converts a <see cref="Guid" /> to and from an array of <see cref="byte" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
/// </remarks>
public class GuidToBytesConverter : ValueConverter<Guid, byte[]>
{
    private static readonly ConverterMappingHints DefaultHints
        = new(size: 16, valueGeneratorFactory: (_, _) => new SequentialGuidValueGenerator());

    /// <summary>
    ///     Creates a new instance of this converter.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This converter does not preserve order because the ordering of bits in
    ///         the standard binary representation of a GUID does not match the ordering
    ///         in the standard string representation.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    ///     </para>
    /// </remarks>
    public GuidToBytesConverter()
        : this(null)
    {
    }

    /// <summary>
    ///     Creates a new instance of this converter.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This converter does not preserve order because the ordering of bits in
    ///         the standard binary representation of a GUID does not match the ordering
    ///         in the standard string representation.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="mappingHints">
    ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
    ///     facets for the converted data.
    /// </param>
    public GuidToBytesConverter(ConverterMappingHints? mappingHints)
        : base(
            v => v.ToByteArray(),
            v => new Guid(v),
            DefaultHints.With(mappingHints))
    {
    }

    /// <summary>
    ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
    /// </summary>
    public static ValueConverterInfo DefaultInfo { get; }
        = new(typeof(Guid), typeof(byte[]), i => new GuidToBytesConverter(i.MappingHints), DefaultHints);
}
