// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     Converts <see cref="DateTime" /> to and from arrays of bytes.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
/// </remarks>
public class DateTimeOffsetToBytesConverter : ValueConverter<DateTimeOffset, byte[]>
{
    private static readonly ConverterMappingHints DefaultHints = new(size: 12);
    private static readonly NumberToBytesConverter<long> LongToBytes = new();
    private static readonly NumberToBytesConverter<short> ShortToBytes = new();

    /// <summary>
    ///     Creates a new instance of this converter.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    public DateTimeOffsetToBytesConverter()
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
    public DateTimeOffsetToBytesConverter(ConverterMappingHints? mappingHints)
        : base(
            v => ToBytes(v),
            v => FromBytes(v),
            DefaultHints.With(mappingHints))
    {
    }

    /// <summary>
    ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
    /// </summary>
    public static ValueConverterInfo DefaultInfo { get; }
        = new(typeof(DateTimeOffset), typeof(byte[]), i => new DateTimeOffsetToBytesConverter(i.MappingHints), DefaultHints);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static byte[] ToBytes(DateTimeOffset value)
    {
        var timeBytes = (byte[])LongToBytes.ConvertToProvider(value.DateTime.ToBinary())!;
        var offsetBytes = (byte[])ShortToBytes.ConvertToProvider(value.Offset.TotalMinutes)!;
        return timeBytes.Concat(offsetBytes).ToArray();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static DateTimeOffset FromBytes(byte[] bytes)
    {
        var timeBinary = (long)LongToBytes.ConvertFromProvider(bytes)!;
        var offsetMins = (short)ShortToBytes.ConvertFromProvider(bytes.Skip(8).ToArray())!;
        return new DateTimeOffset(DateTime.FromBinary(timeBinary), new TimeSpan(0, offsetMins, 0));
    }
}
