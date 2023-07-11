// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     <para>
///         Provides a simple API surface for configuring an <see cref="IConventionPropertyBase" /> from conventions.
///     </para>
///     <para>
///         This interface is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IConventionPropertyBaseBuilder<TBuilder> : IConventionAnnotatableBuilder
    where TBuilder : IConventionPropertyBaseBuilder<TBuilder>
{
    /// <summary>
    ///     Gets the property-like object being configured.
    /// </summary>
    new IConventionPropertyBase Metadata { get; }

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder to continue configuration if the annotation was set, <see langword="null" /> otherwise.
    /// </returns>
    new TBuilder? HasAnnotation(string name, object? value, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
    ///     Removes the annotation if <see langword="null" /> value is specified.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation. <see langword="null" /> to remove the annotations.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder to continue configuration if the annotation was set or removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    new TBuilder? HasNonNullAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes the annotation with the given name from this object.
    /// </summary>
    /// <param name="name">The name of the annotation to remove.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder to continue configuration if the annotation was set, <see langword="null" /> otherwise.
    /// </returns>
    new TBuilder? HasNoAnnotation(string name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the backing field to use for this property-like object.
    /// </summary>
    /// <param name="fieldName">The field name.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    TBuilder? HasField(string? fieldName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the backing field to use for this property-like object.
    /// </summary>
    /// <param name="fieldInfo">The field.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    TBuilder? HasField(FieldInfo? fieldInfo, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the backing field can be set for this property-like object
    ///     from the current configuration source.
    /// </summary>
    /// <param name="fieldName">The field name.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the backing field can be set for this property-like object.</returns>
    bool CanSetField(string? fieldName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the backing field can be set for this property-like object
    ///     from the current configuration source.
    /// </summary>
    /// <param name="fieldInfo">The field.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the backing field can be set for this property-like object.</returns>
    bool CanSetField(FieldInfo? fieldInfo, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the <see cref="PropertyAccessMode" /> to use for this property-like object.
    /// </summary>
    /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" /> to use for this property-like object.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    TBuilder? UsePropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the <see cref="PropertyAccessMode" /> can be set for this property-like object
    ///     from the current configuration source.
    /// </summary>
    /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" /> to use for this property-like object.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the <see cref="PropertyAccessMode" /> can be set for this property-like object.</returns>
    bool CanSetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation = false);
}
