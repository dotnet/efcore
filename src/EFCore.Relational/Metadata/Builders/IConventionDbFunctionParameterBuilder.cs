// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a <see cref="IConventionDbFunctionParameter" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IConventionDbFunctionParameterBuilder : IConventionAnnotatableBuilder
{
    /// <summary>
    ///     The function parameter metadata that is being built.
    /// </summary>
    new IConventionDbFunctionParameter Metadata { get; }

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionDbFunctionParameterBuilder" /> to continue configuration if the annotation was set, <see langword="null" />
    ///     otherwise.
    /// </returns>
    new IConventionDbFunctionParameterBuilder? HasAnnotation(string name, object? value, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
    ///     Removes the annotation if <see langword="null" /> value is specified.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation. <see langword="null" /> to remove the annotations.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionDbFunctionParameterBuilder" /> to continue configuration if the annotation was set or removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    new IConventionDbFunctionParameterBuilder? HasNonNullAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes the annotation with the given name from this object.
    /// </summary>
    /// <param name="name">The name of the annotation to remove.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionDbFunctionParameterBuilder" /> to continue configuration if the annotation was set, <see langword="null" />
    ///     otherwise.
    /// </returns>
    new IConventionDbFunctionParameterBuilder? HasNoAnnotation(string name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the store type of the function parameter in the database.
    /// </summary>
    /// <param name="storeType">The store type of the function parameter in the database.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The same builder instance if the configuration was applied; <see langword="null" /> otherwise.</returns>
    IConventionDbFunctionParameterBuilder? HasStoreType(string? storeType, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the store type can be set for this property
    ///     from the current configuration source.
    /// </summary>
    /// <param name="storeType">The store type of the function parameter in the database.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the store type can be set for this property.</returns>
    bool CanSetStoreType(string? storeType, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the <see cref="RelationalTypeMapping" /> of the function parameter.
    /// </summary>
    /// <param name="typeMapping">The type mapping to use for the function parameter.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The same builder instance if the configuration was applied; <see langword="null" /> otherwise.</returns>
    IConventionDbFunctionParameterBuilder? HasTypeMapping(
        RelationalTypeMapping? typeMapping,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether <see cref="RelationalTypeMapping" /> can be set for this property
    ///     from the current configuration source.
    /// </summary>
    /// <param name="typeMapping">The type mapping to use for the function parameter.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the type mapping can be set for this property.</returns>
    bool CanSetTypeMapping(RelationalTypeMapping? typeMapping, bool fromDataAnnotation = false);
}
