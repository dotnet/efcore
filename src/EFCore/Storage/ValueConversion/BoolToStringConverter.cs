// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     Converts <see cref="bool" /> values to and from two string values.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
/// </remarks>
public class BoolToStringConverter : BoolToTwoValuesConverter<string>
{
    /// <summary>
    ///     Creates a new instance of this converter. A case-insensitive first character test is used
    ///     when converting from the store.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    /// <param name="falseValue">The string to use for <see langword="false" />.</param>
    /// <param name="trueValue">The string to use for <see langword="true" />.</param>
    /// <param name="mappingHints">
    ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
    ///     facets for the converted data.
    /// </param>
    public BoolToStringConverter(
        string falseValue,
        string trueValue,
        ConverterMappingHints? mappingHints = null)
        : base(
            Check.NotNull(falseValue, nameof(falseValue)),
            Check.NotNull(trueValue, nameof(trueValue)),
            FromProvider(trueValue),
            new ConverterMappingHints(size: Math.Max(falseValue.Length, trueValue.Length)).With(mappingHints))
    {
    }

    /// <summary>
    ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
    /// </summary>
    public static ValueConverterInfo DefaultInfo { get; }
        = new(
            typeof(bool),
            typeof(string),
            i => new BoolToStringConverter("0", "1", i.MappingHints),
            new ConverterMappingHints(size: 1));

    private static Expression<Func<string, bool>> FromProvider(string trueValue)
    {
        var testChar = trueValue.ToUpperInvariant()[0];

        return v => !string.IsNullOrEmpty(v)
            && v.ToUpperInvariant()[0] == testChar;
    }
}
