// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     A registry of <see cref="ValueConverterInfo" /> that can be used to find
///     the preferred converter to use to convert to and from a given model type
///     to a type that the database provider supports.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
///     </para>
/// </remarks>
public interface IValueConverterSelector
{
    /// <summary>
    ///     Returns the list of <see cref="ValueConverterInfo" /> instances that can be
    ///     used to convert the given model type. Converters nearer the front of
    ///     the list should be used in preference to converters nearer the end.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    /// <param name="modelClrType">The type for which a converter is needed.</param>
    /// <param name="providerClrType">The store type to target, or null for any.</param>
    /// <returns>The converters available.</returns>
    IEnumerable<ValueConverterInfo> Select(
        Type modelClrType,
        Type? providerClrType = null);
}
