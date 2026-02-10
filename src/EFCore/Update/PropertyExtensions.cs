// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     Extension methods for <see cref="IProperty" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public static class PropertyExtensions
{
    /// <summary>
    ///     Converts the value of a property to the provider-expected value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="property">The property the <paramref name="value"/> is for.</param>
    /// <returns>The converted value.</returns>
    public static object? ConvertToProviderValue(this IProperty property, object? value)
    {
        var typeMapping = property.GetTypeMapping();
        value = value?.GetType().IsInteger() == true && typeMapping.ClrType.UnwrapNullableType().IsEnum
            ? Enum.ToObject(typeMapping.ClrType.UnwrapNullableType(), value)
            : value;

        var converter = typeMapping.Converter;
        if (converter != null)
        {
            value = converter.ConvertToProvider(value);
        }

        return value;
    }
}
