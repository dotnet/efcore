// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     Converts strings to and from arrays of bytes.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
/// </remarks>
public class StringToBytesConverter : ValueConverter<string?, byte[]?>
{
    private static readonly MethodInfo EncodingGetBytesMethodInfo = typeof(Encoding).GetMethod(nameof(Encoding.GetBytes), [typeof(string)])!;
    private static readonly MethodInfo EncodingGetStringMethodInfo = typeof(Encoding).GetMethod(nameof(Encoding.GetString), [typeof(byte[])])!;
    private static readonly MethodInfo EncodingGetEncodingMethodInfo = typeof(Encoding).GetMethod(nameof(Encoding.GetEncoding), [typeof(int)])!;

    /// <summary>
    ///     Creates a new instance of this converter.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    /// <param name="encoding">The string encoding to use.</param>
    /// <param name="mappingHints">
    ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
    ///     facets for the converted data.
    /// </param>
    public StringToBytesConverter(
        Encoding encoding,
        ConverterMappingHints? mappingHints = null)
        : base(
            FromProvider(encoding),
            ToProvider(encoding),
            mappingHints)
    {
    }

    /// <summary>
    ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
    /// </summary>
    public static ValueConverterInfo DefaultInfo { get; }
        = new(typeof(string), typeof(byte[]), i => new StringToBytesConverter(Encoding.UTF8, i.MappingHints));

    private static Expression<Func<string?, byte[]?>> FromProvider(Encoding encoding)
    {
        // v => encoding.GetBytes(v!),
        var prm = Expression.Parameter(typeof(string), "v");
        var result = Expression.Lambda<Func<string?, byte[]?>>(
            Expression.Call(
                Expression.Call(
                    EncodingGetEncodingMethodInfo,
                    Expression.Constant(encoding.CodePage)),
                EncodingGetBytesMethodInfo, prm),
            prm);

        return result;
    }

    private static Expression<Func<byte[]?, string?>> ToProvider(Encoding encoding)
    {
        // v => encoding.GetString(v!)
        var prm = Expression.Parameter(typeof(byte[]), "v");
        var result = Expression.Lambda<Func<byte[]?, string?>>(
            Expression.Call(
                Expression.Call(
                    EncodingGetEncodingMethodInfo,
                    Expression.Constant(encoding.CodePage)),
                EncodingGetStringMethodInfo, prm),
            prm);

        return result;
    }
}
