// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     <see cref="IElementType" /> extension methods for relational database metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalElementTypeExtensions
{
    /// <summary>
    ///     Returns the database type of the elements, or <see langword="null" /> if the database type could not be found.
    /// </summary>
    /// <param name="elementType">The element.</param>
    /// <returns>
    ///     The database type of the elements, or <see langword="null" /> if the database type could not be found.
    /// </returns>
    public static string? GetStoreType(this IReadOnlyElementType elementType)
        => (string?)(elementType.FindAnnotation(RelationalAnnotationNames.StoreType)?.Value
            ?? elementType.FindRelationalTypeMapping()?.StoreType);

    /// <summary>
    ///     Returns the database type of the elements.
    /// </summary>
    /// <param name="elementType">The element.</param>
    /// <returns>The database type of the elements.</returns>
    public static string GetStoreType(this IElementType elementType)
        => ((IReadOnlyElementType)elementType).GetStoreType()!;

    /// <summary>
    ///     Sets the database type of the elements.
    /// </summary>
    /// <param name="elementType">The element.</param>
    /// <param name="value">The value to set.</param>
    public static void SetStoreType(this IMutableElementType elementType, string? value)
        => elementType.SetOrRemoveAnnotation(
            RelationalAnnotationNames.StoreType,
            Check.NullButNotEmpty(value, nameof(value)));

    /// <summary>
    ///     Sets the database type of the elements.
    /// </summary>
    /// <param name="elementType">The element.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetStoreType(
        this IConventionElementType elementType,
        string? value,
        bool fromDataAnnotation = false)
        => (string?)elementType.SetOrRemoveAnnotation(
            RelationalAnnotationNames.StoreType,
            Check.NullButNotEmpty(value, nameof(value)),
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the database type.
    /// </summary>
    /// <param name="elementType">The element.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the column name.</returns>
    public static ConfigurationSource? GetStoreTypeConfigurationSource(this IConventionElementType elementType)
        => elementType.FindAnnotation(RelationalAnnotationNames.StoreType)?.GetConfigurationSource();

    /// <summary>
    ///     Returns a flag indicating whether the elements are capable of storing only fixed-length data, such as strings.
    /// </summary>
    /// <param name="elementType">The element.</param>
    /// <returns>A flag indicating whether the elements arecapable of storing only fixed-length data, such as strings.</returns>
    public static bool? IsFixedLength(this IReadOnlyElementType elementType)
        => (bool?)elementType.FindAnnotation(RelationalAnnotationNames.IsFixedLength)?.Value;

    /// <summary>
    ///     Returns a flag indicating whether the elements are capable of storing only fixed-length data, such as strings.
    /// </summary>
    /// <param name="elementType">The element.</param>
    /// <param name="storeObject">The identifier of the table-like store object containing the column.</param>
    /// <returns>A flag indicating whether the elements are capable of storing only fixed-length data, such as strings.</returns>
    public static bool? IsFixedLength(this IReadOnlyElementType elementType, in StoreObjectIdentifier storeObject)
        => (bool?)elementType.FindAnnotation(RelationalAnnotationNames.IsFixedLength)?.Value;

    /// <summary>
    ///     Sets a flag indicating whether the elements are capable of storing only fixed-length data, such as strings.
    /// </summary>
    /// <param name="elementType">The element.</param>
    /// <param name="fixedLength">A value indicating whether the elements are constrained to fixed length values.</param>
    public static void SetIsFixedLength(this IMutableElementType elementType, bool? fixedLength)
        => elementType.SetOrRemoveAnnotation(RelationalAnnotationNames.IsFixedLength, fixedLength);

    /// <summary>
    ///     Sets a flag indicating whether the elements are capable of storing only fixed-length data, such as strings.
    /// </summary>
    /// <param name="elementType">The element.</param>
    /// <param name="fixedLength">A value indicating whether the element are constrained to fixed length values.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static bool? SetIsFixedLength(
        this IConventionElementType elementType,
        bool? fixedLength,
        bool fromDataAnnotation = false)
        => (bool?)elementType.SetOrRemoveAnnotation(
            RelationalAnnotationNames.IsFixedLength,
            fixedLength,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for <see cref="IsFixedLength(IReadOnlyElementType)" />.
    /// </summary>
    /// <param name="elementType">The element.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for <see cref="IsFixedLength(IReadOnlyElementType)" />.</returns>
    public static ConfigurationSource? GetIsFixedLengthConfigurationSource(this IConventionElementType elementType)
        => elementType.FindAnnotation(RelationalAnnotationNames.IsFixedLength)?.GetConfigurationSource();

    /// <summary>
    ///     Returns the <see cref="RelationalTypeMapping" /> for the given element on a finalized model.
    /// </summary>
    /// <param name="elementType">The element.</param>
    /// <returns>The type mapping.</returns>
    [DebuggerStepThrough]
    public static RelationalTypeMapping GetRelationalTypeMapping(this IReadOnlyElementType elementType)
        => (RelationalTypeMapping)elementType.GetTypeMapping();

    /// <summary>
    ///     Returns the <see cref="RelationalTypeMapping" /> for the given element on a finalized model.
    /// </summary>
    /// <param name="elementType">The element.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    [DebuggerStepThrough]
    public static RelationalTypeMapping? FindRelationalTypeMapping(this IReadOnlyElementType elementType)
        => (RelationalTypeMapping?)elementType.FindTypeMapping();
}
