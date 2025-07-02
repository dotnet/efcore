// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Complex property extension methods for relational database metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalComplexPropertyExtensions
{
    /// <summary>
    ///     Gets the value of JSON property name used for the given complex property of an entity mapped to a JSON column.
    /// </summary>
    /// <remarks>
    ///     Unless configured explicitly, complex property name is used.
    /// </remarks>
    /// <param name="complexProperty">The complex property.</param>
    /// <returns>
    ///     The value for the JSON property used to store the value of this complex property.
    ///     <see langword="null" /> is returned for complex properties of entities that are not mapped to a JSON column.
    /// </returns>
    public static string? GetJsonPropertyName(this IReadOnlyComplexProperty complexProperty)
        => (string?)complexProperty.FindAnnotation(RelationalAnnotationNames.JsonPropertyName)?.Value
            ?? (!complexProperty.DeclaringType.IsMappedToJson() ? null : complexProperty.Name);

    /// <summary>
    ///     Sets the value of JSON property name used for the given complex property of an entity mapped to a JSON column.
    /// </summary>
    /// <param name="complexProperty">The complex property.</param>
    /// <param name="name">The name to be used.</param>
    public static void SetJsonPropertyName(this IMutableComplexProperty complexProperty, string? name)
        => complexProperty.SetOrRemoveAnnotation(
            RelationalAnnotationNames.JsonPropertyName,
            Check.NullButNotEmpty(name));

    /// <summary>
    ///     Sets the value of JSON property name used for the given complex property of an entity mapped to a JSON column.
    /// </summary>
    /// <param name="complexProperty">The complex property.</param>
    /// <param name="name">The name to be used.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetJsonPropertyName(
        this IConventionComplexProperty complexProperty,
        string? name,
        bool fromDataAnnotation = false)
        => (string?)complexProperty.SetOrRemoveAnnotation(
            RelationalAnnotationNames.JsonPropertyName,
            Check.NullButNotEmpty(name),
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the JSON property name for a given complex property.
    /// </summary>
    /// <param name="complexProperty">The complex property.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the JSON property name for a given complex property.</returns>
    public static ConfigurationSource? GetJsonPropertyNameConfigurationSource(this IConventionComplexProperty complexProperty)
        => complexProperty.FindAnnotation(RelationalAnnotationNames.JsonPropertyName)?.GetConfigurationSource();
}
