// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Relational database specific extension methods for <see cref="NavigationBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalNavigationBuilderExtensions
{
    /// <summary>
    ///     Configures the navigation of an entity mapped to a JSON column, mapping the navigation to a specific JSON property,
    ///     rather than using the navigation name.
    /// </summary>
    /// <param name="navigationBuilder">The builder for the navigation being configured.</param>
    /// <param name="name">JSON property name to be used.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionNavigationBuilder? HasJsonPropertyName(
        this IConventionNavigationBuilder navigationBuilder,
        string? name,
        bool fromDataAnnotation = false)
    {
        if (!navigationBuilder.CanSetJsonPropertyName(name, fromDataAnnotation))
        {
            return null;
        }

        navigationBuilder.Metadata.SetJsonPropertyName(name, fromDataAnnotation);

        return navigationBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the given value can be used as a JSON property name for a given navigation.
    /// </summary>
    /// <param name="navigationBuilder">The builder for the navigation being configured.</param>
    /// <param name="name">JSON property name to be used.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given value can be set as JSON property name for this navigation.</returns>
    public static bool CanSetJsonPropertyName(
        this IConventionNavigationBuilder navigationBuilder,
        string? name,
        bool fromDataAnnotation = false)
        => navigationBuilder.CanSetAnnotation(RelationalAnnotationNames.JsonPropertyName, name, fromDataAnnotation);
}
