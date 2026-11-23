// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Relational database specific extension methods for <see cref="IConventionComplexTypeBuilder" />.
/// </summary>
public static class RelationalComplexTypeBuilderExtensions
{
    /// <summary>
    ///     Configures the complex type to map to a specific JSON property, rather than using the complex property name.
    /// </summary>
    /// <param name="complexTypeBuilder">The builder for the complex type being configured.</param>
    /// <param name="name">JSON property name to be used.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionComplexTypeBuilder? HasJsonPropertyName(
        this IConventionComplexTypeBuilder complexTypeBuilder,
        string? name,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(name);

        if (!complexTypeBuilder.CanSetJsonPropertyName(name, fromDataAnnotation))
        {
            return null;
        }

        complexTypeBuilder.Metadata.SetJsonPropertyName(name, fromDataAnnotation);

        return complexTypeBuilder;
    }

    /// <summary>
    ///     Configures the name of the container column to which the complex type is mapped.
    /// </summary>
    /// <param name="complexTypeBuilder">The builder for the complex type.</param>
    /// <param name="columnName">The name of the container column.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionComplexTypeBuilder? HasContainerColumnName(
        this IConventionComplexTypeBuilder complexTypeBuilder,
        string? columnName,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(columnName);

        if (!complexTypeBuilder.CanSetContainerColumnName(columnName, fromDataAnnotation))
        {
            return null;
        }

        complexTypeBuilder.Metadata.SetContainerColumnName(columnName, fromDataAnnotation);

        return complexTypeBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the container column name can be configured
    ///     using the specified configuration source.
    /// </summary>
    /// <param name="complexTypeBuilder">The builder for the complex type.</param>
    /// <param name="columnName">The name of the container column.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the configuration can be applied.</returns>
    public static bool CanSetContainerColumnName(
        this IConventionComplexTypeBuilder complexTypeBuilder,
        string? columnName,
        bool fromDataAnnotation = false)
        => complexTypeBuilder.CanSetAnnotation(RelationalAnnotationNames.ContainerColumnName, columnName, fromDataAnnotation);

    /// <summary>
    ///     Configures the column type for the JSON column that stores the complex type.
    /// </summary>
    /// <param name="complexTypeBuilder">The builder for the complex type being configured.</param>
    /// <param name="columnType">The database type for the column, or <see langword="null" /> to use the database default.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionComplexTypeBuilder? HasContainerColumnType(
        this IConventionComplexTypeBuilder complexTypeBuilder,
        string? columnType,
        bool fromDataAnnotation = false)
    {
        if (!complexTypeBuilder.CanSetContainerColumnType(columnType, fromDataAnnotation))
        {
            return null;
        }

        complexTypeBuilder.Metadata.SetContainerColumnType(columnType, fromDataAnnotation);

        return complexTypeBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the container column type can be configured
    ///     using the specified configuration source.
    /// </summary>
    /// <param name="complexTypeBuilder">The builder for the complex type.</param>
    /// <param name="columnType">The database type for the container column.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the configuration can be applied.</returns>
    public static bool CanSetContainerColumnType(
        this IConventionComplexTypeBuilder complexTypeBuilder,
        string? columnType,
        bool fromDataAnnotation = false)
        => complexTypeBuilder.CanSetAnnotation(RelationalAnnotationNames.ContainerColumnType, columnType, fromDataAnnotation);

    /// <summary>
    ///     Returns a value indicating whether the JSON property name can be configured
    ///     using the specified configuration source.
    /// </summary>
    /// <param name="complexTypeBuilder">The builder for the complex type.</param>
    /// <param name="name">The JSON property name to be used.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the configuration can be applied.</returns>
    public static bool CanSetJsonPropertyName(
        this IConventionComplexTypeBuilder complexTypeBuilder,
        string? name,
        bool fromDataAnnotation = false)
        => complexTypeBuilder.CanSetAnnotation(RelationalAnnotationNames.JsonPropertyName, name, fromDataAnnotation);
}
