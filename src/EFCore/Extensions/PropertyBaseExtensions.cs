// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Extension methods for <see cref="IReadOnlyPropertyBase" />.
/// </summary>
public static class PropertyBaseExtensions
{
    /// <summary>
    ///     Creates a formatted string representation of the given properties such as is useful
    ///     when throwing exceptions about keys, indexes, etc. that use the properties.
    /// </summary>
    /// <param name="properties">The properties to format.</param>
    /// <param name="includeTypes">If true, then type names are included in the string. The default is <see langword="false" />.</param>
    /// <returns>The string representation.</returns>
    public static string Format(this IEnumerable<IReadOnlyPropertyBase> properties, bool includeTypes = false)
        => "{"
            + string.Join(
                ", ",
                properties.Select(
                    p => "'" + p.Name + "'" + (includeTypes ? " : " + p.ClrType.DisplayName(fullName: false) : "")))
            + "}";
}
