// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     <para>
///         Provides a simple API surface for configuring an <see cref="IConventionComplexProperty" /> from conventions.
///     </para>
///     <para>
///         This interface is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IConventionComplexTypeBuilder : IConventionTypeBaseBuilder
{
    /// <summary>
    ///     Gets the property being configured.
    /// </summary>
    new IConventionComplexType Metadata { get; }

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionComplexTypeBuilder" /> to continue configuration if the annotation was set, <see langword="null" /> otherwise.
    /// </returns>
    new IConventionComplexTypeBuilder? HasAnnotation(string name, object? value, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
    ///     Removes the annotation if <see langword="null" /> value is specified.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation. <see langword="null" /> to remove the annotations.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionComplexTypeBuilder" /> to continue configuration if the annotation was set or removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    new IConventionComplexTypeBuilder? HasNonNullAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes the annotation with the given name from this object.
    /// </summary>
    /// <param name="name">The name of the annotation to remove.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionComplexTypeBuilder" /> to continue configuration if the annotation was set, <see langword="null" /> otherwise.
    /// </returns>
    new IConventionComplexTypeBuilder? HasNoAnnotation(string name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes properties in the given list if they are not part of any metadata object.
    /// </summary>
    /// <param name="properties">The properties to remove.</param>
    new IConventionComplexTypeBuilder RemoveUnusedImplicitProperties(IReadOnlyList<IConventionProperty> properties);

    /// <summary>
    ///     Removes a property from this complex type.
    /// </summary>
    /// <param name="property">The property to be removed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the property was removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    new IConventionComplexTypeBuilder? HasNoProperty(IConventionProperty property, bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes a complex property from this complex type.
    /// </summary>
    /// <param name="complexProperty">The complex property to be removed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the complex property was removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    new IConventionComplexTypeBuilder? HasNoComplexProperty(IConventionComplexProperty complexProperty, bool fromDataAnnotation = false);

    /// <summary>
    ///     Excludes the given property from the complex type and prevents conventions from adding a matching property
    ///     or navigation to the type.
    /// </summary>
    /// <param name="memberName">The name of the member to be removed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance so that additional configuration calls can be chained
    ///     if the given member was ignored, <see langword="null" /> otherwise.
    /// </returns>
    new IConventionComplexTypeBuilder? Ignore(string memberName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures the <see cref="ChangeTrackingStrategy" /> to be used for this complex type.
    ///     This strategy indicates how the context detects changes to properties for an instance of the complex type.
    /// </summary>
    /// <param name="changeTrackingStrategy">
    ///     The change tracking strategy to be used.
    ///     <see langword="null" /> to reset to default.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the <see cref="ChangeTrackingStrategy" /> was set,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    new IConventionComplexTypeBuilder? HasChangeTrackingStrategy(
        ChangeTrackingStrategy? changeTrackingStrategy,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the <see cref="PropertyAccessMode" /> to use for all properties of this complex type.
    /// </summary>
    /// <param name="propertyAccessMode">
    ///     The <see cref="PropertyAccessMode" /> to use for properties of this complex type.
    ///     <see langword="null" /> to reset to default.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    /// <returns>
    ///     The same builder instance if the <see cref="PropertyAccessMode" /> was set,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    new IConventionComplexTypeBuilder? UsePropertyAccessMode(
        PropertyAccessMode? propertyAccessMode,
        bool fromDataAnnotation = false);
}
