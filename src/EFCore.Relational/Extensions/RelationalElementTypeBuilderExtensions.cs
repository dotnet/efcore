// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Relational database specific extension methods for <see cref="ElementTypeBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalElementTypeBuilderExtensions
{
    /// <summary>
    ///     Configures the data type of the elements of the collection.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="elementTypeBuilder">The builder for the elements being configured.</param>
    /// <param name="typeName">The name of the data type of the elements.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ElementTypeBuilder HasStoreType(
        this ElementTypeBuilder elementTypeBuilder,
        string? typeName)
    {
        Check.NullButNotEmpty(typeName, nameof(typeName));

        elementTypeBuilder.Metadata.SetStoreType(typeName);

        return elementTypeBuilder;
    }

    /// <summary>
    ///     Configures the data type of the elements of the collection.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="elementTypeBuilder"> builder for the elements being configured.</param>
    /// <param name="typeName">The name of the data type of the elements.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The same builder instance if the configuration was applied, <see langword="null" /> otherwise.</returns>
    public static IConventionElementTypeBuilder? HasStoreType(
        this IConventionElementTypeBuilder elementTypeBuilder,
        string? typeName,
        bool fromDataAnnotation = false)
    {
        if (!elementTypeBuilder.CanSetStoreType(typeName, fromDataAnnotation))
        {
            return null;
        }

        elementTypeBuilder.Metadata.SetStoreType(typeName, fromDataAnnotation);
        return elementTypeBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the given data type can be set for the elements.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="elementTypeBuilder"> builder for the elements being configured.</param>
    /// <param name="typeName">The name of the data type of the elements.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given data type can be set for the property.</returns>
    public static bool CanSetStoreType(
        this IConventionElementTypeBuilder elementTypeBuilder,
        string? typeName,
        bool fromDataAnnotation = false)
        => elementTypeBuilder.CanSetAnnotation(RelationalAnnotationNames.StoreType, typeName, fromDataAnnotation);

    /// <summary>
    ///     Configures the elements as capable of storing only fixed-length data, such as strings.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="elementTypeBuilder">The builder for the elements being configured.</param>
    /// <param name="fixedLength">A value indicating whether the elements are constrained to fixed length values.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public static ElementTypeBuilder IsFixedLength(
        this ElementTypeBuilder elementTypeBuilder,
        bool fixedLength = true)
    {
        elementTypeBuilder.Metadata.SetIsFixedLength(fixedLength);

        return elementTypeBuilder;
    }

    /// <summary>
    ///     Configures the elements as capable of storing only fixed-length data, such as strings.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="elementTypeBuilder"> builder for the elements being configured.</param>
    /// <param name="fixedLength">A value indicating whether the elements are constrained to fixed length values.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>     The same builder instance if the configuration was applied, <see langword="null" /> otherwise.</returns>
    public static IConventionElementTypeBuilder? IsFixedLength(
        this IConventionElementTypeBuilder elementTypeBuilder,
        bool? fixedLength,
        bool fromDataAnnotation = false)
    {
        if (!elementTypeBuilder.CanSetFixedLength(fixedLength, fromDataAnnotation))
        {
            return null;
        }

        elementTypeBuilder.Metadata.SetIsFixedLength(fixedLength, fromDataAnnotation);
        return elementTypeBuilder;
    }

    /// <summary>
    ///     Returns a value indicating whether the elements can be configured as being fixed length or not.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="elementTypeBuilder"> builder for the elements being configured.</param>
    /// <param name="fixedLength">A value indicating whether the elements are constrained to fixed length values.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the elements can be configured as being fixed length or not.</returns>
    public static bool CanSetFixedLength(
        this IConventionElementTypeBuilder elementTypeBuilder,
        bool? fixedLength,
        bool fromDataAnnotation = false)
        => elementTypeBuilder.CanSetAnnotation(RelationalAnnotationNames.IsFixedLength, fixedLength, fromDataAnnotation);
}
