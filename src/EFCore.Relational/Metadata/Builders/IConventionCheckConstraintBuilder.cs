// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a check constraint.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> and
///     <see href="https://aka.ms/efcore-docs-check-constraints">Database check constraints</see> for more information and examples.
/// </remarks>
public interface IConventionCheckConstraintBuilder : IConventionAnnotatableBuilder
{
    /// <summary>
    ///     The check constraint being configured.
    /// </summary>
    new IConventionCheckConstraint Metadata { get; }

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionCheckConstraintBuilder" /> to continue configuration if the annotation was set, <see langword="null" />
    ///     otherwise.
    /// </returns>
    new IConventionCheckConstraintBuilder? HasAnnotation(string name, object? value, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
    ///     Removes the annotation if <see langword="null" /> value is specified.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation. <see langword="null" /> to remove the annotations.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionCheckConstraintBuilder" /> to continue configuration if the annotation was set or removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    new IConventionCheckConstraintBuilder? HasNonNullAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes the annotation with the given name from this object.
    /// </summary>
    /// <param name="name">The name of the annotation to remove.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionCheckConstraintBuilder" /> to continue configuration if the annotation was set, <see langword="null" />
    ///     otherwise.
    /// </returns>
    new IConventionCheckConstraintBuilder? HasNoAnnotation(string name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the database name of the check constraint.
    /// </summary>
    /// <param name="name">The database name of the check constraint.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The same builder instance if the configuration was applied, <see langword="null" /> otherwise.</returns>
    IConventionCheckConstraintBuilder? HasName(string? name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given name can be set for the check constraint.
    /// </summary>
    /// <param name="name">The database name of the check constraint.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the database name can be set for the check constraint.</returns>
    bool CanSetName(string? name, bool fromDataAnnotation = false);
}
