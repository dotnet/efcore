// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     Converts identifiers to the plural and singular equivalents.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public interface IPluralizer
{
    /// <summary>
    ///     Gets the plural version of the given identifier. Returns the same
    ///     identifier if it is already pluralized.
    /// </summary>
    /// <param name="identifier">The identifier to be pluralized.</param>
    /// <returns>The pluralized identifier.</returns>
    string Pluralize(string identifier);

    /// <summary>
    ///     Gets the singular version of the given identifier. Returns the same
    ///     identifier if it is already singularized.
    /// </summary>
    /// <param name="identifier">The identifier to be singularized.</param>
    /// <returns>The singularized identifier.</returns>
    string Singularize(string identifier);
}
