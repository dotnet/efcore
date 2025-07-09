// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Complex type extension methods for relational database metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalComplexTypeExtensions
{
    /// <summary>
    ///     Gets the container column name to which the complex type is mapped.
    /// </summary>
    /// <param name="complexType">The complex type to get the container column name for.</param>
    /// <returns>The container column name to which the complex type is mapped.</returns>
    public static string? GetContainerColumnName(this IReadOnlyComplexType complexType)
        => complexType.FindAnnotation(RelationalAnnotationNames.ContainerColumnName)?.Value is string columnName
            ? columnName
            : complexType.ComplexProperty.DeclaringType.GetContainerColumnName();

    /// <summary>
    ///     Sets the name of the container column to which the complex type is mapped.
    /// </summary>
    /// <param name="complexType">The complex type to set the container column name for.</param>
    /// <param name="columnName">The name to set.</param>
    public static void SetContainerColumnName(this IMutableComplexType complexType, string? columnName)
        => complexType.SetOrRemoveAnnotation(RelationalAnnotationNames.ContainerColumnName, columnName);

    /// <summary>
    ///     Sets the name of the container column to which the complex type is mapped.
    /// </summary>
    /// <param name="complexType">The complex type to set the container column name for.</param>
    /// <param name="columnName">The name to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetContainerColumnName(
        this IConventionComplexType complexType,
        string? columnName,
        bool fromDataAnnotation = false)
        => (string?)complexType.SetAnnotation(RelationalAnnotationNames.ContainerColumnName, columnName, fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the container column name.
    /// </summary>
    /// <param name="complexType">The complex type to set the container column name for.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the container column name.</returns>
    public static ConfigurationSource? GetContainerColumnNameConfigurationSource(this IConventionComplexType complexType)
        => complexType.FindAnnotation(RelationalAnnotationNames.ContainerColumnName)
            ?.GetConfigurationSource();
}
