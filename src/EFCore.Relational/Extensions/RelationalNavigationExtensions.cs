// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Navigation extension methods for relational database metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalNavigationExtensions
{
    /// <summary>
    ///     Gets the value of JSON property name used for the given navigation of an entity mapped to a JSON column.
    /// </summary>
    /// <remarks>
    ///     Unless configured explicitly, navigation name is used.
    /// </remarks>
    /// <param name="navigation">The navigation.</param>
    /// <returns>
    ///     The value for the JSON property used to store the value of this navigation.
    ///     <see langword="null" /> is returned for navigations of entities that are not mapped to a JSON column.
    /// </returns>
    public static string? GetJsonPropertyName(this IReadOnlyNavigationBase navigation)
        => (string?)navigation.FindAnnotation(RelationalAnnotationNames.JsonPropertyName)?.Value
            ?? (!navigation.DeclaringEntityType.IsMappedToJson() ? null : navigation.Name);

    /// <summary>
    ///     Sets the value of JSON property name used for the given navigation of an entity mapped to a JSON column.
    /// </summary>
    /// <param name="navigation">The navigation.</param>
    /// <param name="name">The name to be used.</param>
    public static void SetJsonPropertyName(this IMutableNavigationBase navigation, string? name)
        => navigation.SetOrRemoveAnnotation(
            RelationalAnnotationNames.JsonPropertyName,
            Check.NullButNotEmpty(name, nameof(name)));

    /// <summary>
    ///     Sets the value of JSON property name used for the given navigation of an entity mapped to a JSON column.
    /// </summary>
    /// <param name="navigation">The navigation.</param>
    /// <param name="name">The name to be used.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetJsonPropertyName(
        this IConventionNavigationBase navigation,
        string? name,
        bool fromDataAnnotation = false)
        => (string?)navigation.SetOrRemoveAnnotation(
            RelationalAnnotationNames.JsonPropertyName,
            Check.NullButNotEmpty(name, nameof(name)),
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the JSON property name for a given navigation.
    /// </summary>
    /// <param name="navigation">The navigation.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the JSON property name for a given navigation.</returns>
    public static ConfigurationSource? GetJsonPropertyNameConfigurationSource(this IConventionNavigationBase navigation)
        => navigation.FindAnnotation(RelationalAnnotationNames.JsonPropertyName)?.GetConfigurationSource();
}
