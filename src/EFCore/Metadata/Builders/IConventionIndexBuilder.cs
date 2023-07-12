// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     <para>
///         Provides a simple API surface for configuring an <see cref="IConventionIndex" /> from conventions.
///     </para>
///     <para>
///         This interface is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IConventionIndexBuilder : IConventionAnnotatableBuilder
{
    /// <summary>
    ///     Gets the index being configured.
    /// </summary>
    new IConventionIndex Metadata { get; }

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionIndexBuilder" /> to continue configuration if the annotation was set, <see langword="null" /> otherwise.
    /// </returns>
    new IConventionIndexBuilder? HasAnnotation(string name, object? value, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
    ///     Removes the annotation if <see langword="null" /> value is specified.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation. <see langword="null" /> to remove the annotations.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionIndexBuilder" /> to continue configuration if the annotation was set or removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    new IConventionIndexBuilder? HasNonNullAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes the annotation with the given name from this object.
    /// </summary>
    /// <param name="name">The name of the annotation to remove.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionIndexBuilder" /> to continue configuration if the annotation was set, <see langword="null" /> otherwise.
    /// </returns>
    new IConventionIndexBuilder? HasNoAnnotation(string name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures whether this index is unique (i.e. each set of values must be unique).
    /// </summary>
    /// <param name="unique">A value indicating whether the index is unique.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The same builder instance if the uniqueness was configured, <see langword="null" /> otherwise.</returns>
    IConventionIndexBuilder? IsUnique(bool? unique, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether this index uniqueness can be configured from the current configuration source.
    /// </summary>
    /// <param name="unique">A value indicating whether the index is unique.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the index uniqueness can be configured.</returns>
    bool CanSetIsUnique(bool? unique, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures the sort order(s) for the columns of this index (ascending or descending).
    /// </summary>
    /// <param name="descending">A set of values indicating whether each corresponding index column has descending sort order.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The same builder instance if the uniqueness was configured, <see langword="null" /> otherwise.</returns>
    IConventionIndexBuilder? IsDescending(IReadOnlyList<bool>? descending, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether this index sort order can be configured from the current configuration source.
    /// </summary>
    /// <param name="descending">A set of values indicating whether each corresponding index column has descending sort order.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the index uniqueness can be configured.</returns>
    bool CanSetIsDescending(IReadOnlyList<bool>? descending, bool fromDataAnnotation = false);
}
