// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Extension methods for <see cref="TypeConfigurationType" />.
/// </summary>
/// <remarks>
///     This type is typically used by database providers (and other extensions). It is generally
///     not used in application code.
/// </remarks>
public static class TypeConfigurationTypeExtensions
{
    /// <summary>
    ///     Returns a value indicating whether the configuration type represents an entity type, or
    ///     <see langword="null" /> if the type has no explicit configuration.
    /// </summary>
    /// <param name="configurationType">The configuration type, or <see langword="null" />.</param>
    /// <returns>
    ///     <see langword="true" /> if the type is configured as an entity type; <see langword="false" /> if it is
    ///     configured as something else; <see langword="null" /> if there is no explicit configuration.
    /// </returns>
    public static bool? IsEntityType(this TypeConfigurationType? configurationType)
        => configurationType?.IsEntityType();

    /// <summary>
    ///     Returns a value indicating whether the configuration type represents an entity type.
    /// </summary>
    /// <param name="configurationType">The configuration type.</param>
    /// <returns><see langword="true" /> if the type is configured as an entity type; otherwise <see langword="false" />.</returns>
    public static bool IsEntityType(this TypeConfigurationType configurationType)
        => configurationType is TypeConfigurationType.EntityType
            or TypeConfigurationType.SharedTypeEntityType
            or TypeConfigurationType.OwnedEntityType;
}
