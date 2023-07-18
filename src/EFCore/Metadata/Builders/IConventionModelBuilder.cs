// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     <para>
///         Provides a simple API surface for configuring an <see cref="IConventionModelBuilder" /> from conventions.
///     </para>
///     <para>
///         This interface is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IConventionModelBuilder : IConventionAnnotatableBuilder
{
    /// <summary>
    ///     Gets the model being configured.
    /// </summary>
    new IConventionModel Metadata { get; }

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionModelBuilder" /> to continue configuration if the annotation was set, <see langword="null" /> otherwise.
    /// </returns>
    new IConventionModelBuilder? HasAnnotation(string name, object? value, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists with same or lower <see cref="ConfigurationSource" />.
    ///     Removes the annotation if <see langword="null" /> value is specified.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation. <see langword="null" /> to remove the annotations.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionModelBuilder" /> to continue configuration if the annotation was set or removed,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    new IConventionModelBuilder? HasNonNullAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes the annotation with the given name from this object.
    /// </summary>
    /// <param name="name">The name of the annotation to remove.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionModelBuilder" /> to continue configuration if the annotation was set, <see langword="null" /> otherwise.
    /// </returns>
    new IConventionModelBuilder? HasNoAnnotation(string name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns an object that can be used to configure a given entity type in the model.
    ///     If an entity type with the provided name is not already part of the model,
    ///     a new shadow entity type will be added to the model.
    /// </summary>
    /// <param name="name">The name of the entity type to be configured.</param>
    /// <param name="shouldBeOwned">
    ///     <see langword="true" /> if the entity type should be owned,
    ///     <see langword="false" /> if the entity type should not be owned
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the entity type if the entity type was added or already part of the model,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionEntityTypeBuilder? Entity(string name, bool? shouldBeOwned = false, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns an object that can be used to configure a given shared type entity type in the model.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If an entity type with the provided name is not already part of the model, a new entity type with provided CLR
    ///         type will be added to the model as shared type entity type.
    ///     </para>
    ///     <para>
    ///         Shared type entity type is an entity type which can share CLR type with other types in the model but has
    ///         a unique name and always identified by the name.
    ///     </para>
    /// </remarks>
    /// <param name="name">The name of the entity type to be configured.</param>
    /// <param name="type">The type of the entity type to be configured.</param>
    /// <param name="shouldBeOwned">
    ///     <see langword="true" /> if the entity type should be owned,
    ///     <see langword="false" /> if the entity type should not be owned
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the entity type if the entity type was added or already part of the model,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionEntityTypeBuilder? SharedTypeEntity(
        string name,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        bool? shouldBeOwned = false,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns an object that can be used to configure a given entity type in the model.
    ///     If an entity type with the provided type is not already part of the model,
    ///     a new entity type will be added to the model.
    /// </summary>
    /// <param name="type">The type of the entity type to be configured.</param>
    /// <param name="shouldBeOwned">
    ///     <see langword="true" /> if the entity type should be owned,
    ///     <see langword="false" /> if the entity type should not be owned
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the entity type if the entity type was added or already part of the model,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionEntityTypeBuilder? Entity(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        bool? shouldBeOwned = false,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns an object that can be used to configure a given entity type with defining navigation.
    ///     If an entity type with the provided name is not already part of the model,
    ///     a new shadow entity type will be added to the model.
    /// </summary>
    /// <param name="name">The name of the entity type to be configured.</param>
    /// <param name="definingNavigationName">The defining navigation.</param>
    /// <param name="definingEntityType">The defining entity type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the entity type if the entity type was added or already part of the model,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionEntityTypeBuilder? Entity(
        string name,
        string definingNavigationName,
        IConventionEntityType definingEntityType,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns an object that can be used to configure a given entity type with defining navigation.
    ///     If an entity type with the provided type is not already part of the model,
    ///     a new entity type will be added to the model.
    /// </summary>
    /// <param name="type">The type of the entity type to be configured.</param>
    /// <param name="definingNavigationName">The defining navigation.</param>
    /// <param name="definingEntityType">The defining entity type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to configure the entity type if the entity type was added or already part of the model,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionEntityTypeBuilder? Entity(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        string definingNavigationName,
        IConventionEntityType definingEntityType,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Marks an entity type as owned. All references to this type will be configured as
    ///     separate owned type instances.
    /// </summary>
    /// <param name="type">The entity type to be configured.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An object that can be used to provide default configuration for the owned entity types.
    /// </returns>
    IConventionOwnedEntityTypeBuilder? Owned(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Marks a type as complex. All references to this type will be configured as complex properties.
    /// </summary>
    /// <param name="type">The type to be configured.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     An <see cref="IConventionModelBuilder" /> to continue configuration if the annotation was set, <see langword="null" /> otherwise.
    /// </returns>
    IConventionModelBuilder? ComplexType(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Indicates whether the given entity type name is ignored for the current configuration source.
    /// </summary>
    /// <param name="type">The name of the entity type that might be ignored.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given entity type name is ignored.</returns>
    bool IsIgnored([DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type, bool fromDataAnnotation = false);

    /// <summary>
    ///     Indicates whether the given entity type name is ignored for the current configuration source.
    /// </summary>
    /// <param name="typeName">The name of the entity type that might be ignored.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given entity type name is ignored.</returns>
    bool IsIgnored(string typeName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Excludes the given entity type from the model and prevents it from being added by convention.
    /// </summary>
    /// <param name="type">The entity type to be removed from the model.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance so that additional configuration calls can be chained
    ///     if the given entity type was ignored, <see langword="null" /> otherwise.
    /// </returns>
    IConventionModelBuilder? Ignore(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Excludes the given entity type name from the model and prevents it from being added by convention.
    /// </summary>
    /// <param name="typeName">The entity type name to be removed from the model.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given entity type name was ignored.</returns>
    /// <returns>
    ///     The same builder instance if the given entity type name was ignored, <see langword="null" /> otherwise.
    /// </returns>
    IConventionModelBuilder? Ignore(string typeName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given entity type can be added to the model.
    /// </summary>
    /// <param name="name">The name of the entity type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the entity type can be added.</returns>
    bool CanHaveEntity(
        string name,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given entity type can be added to the model.
    /// </summary>
    /// <param name="type">The type of the entity type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the entity type can be added.</returns>
    bool CanHaveEntity(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given entity type can be added to the model.
    /// </summary>
    /// <param name="name">The name of the entity type.</param>
    /// <param name="type">The type of the entity type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the entity type can be added.</returns>
    bool CanHaveSharedTypeEntity(
        string name,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type? type,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes the given entity type from the model.
    /// </summary>
    /// <param name="entityType">The entity type to be removed from the model.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the given entity type was removed, <see langword="null" /> otherwise.
    /// </returns>
    IConventionModelBuilder? HasNoEntityType(IConventionEntityType entityType, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the entity type can be removed from the model.
    /// </summary>
    /// <param name="entityType">The entity type to be removed.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the navigation can be removed from this entity type.</returns>
    bool CanRemoveEntity(IConventionEntityType entityType, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given entity type can be ignored from the current configuration source.
    /// </summary>
    /// <param name="type">The entity type to be removed from the model.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given entity type can be ignored.</returns>
    bool CanIgnore([DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given entity type name can be ignored from the current configuration source
    /// </summary>
    /// <param name="typeName">The entity type name to be removed from the model.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given entity type name can be ignored.</returns>
    bool CanIgnore(string typeName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures the default <see cref="ChangeTrackingStrategy" /> to be used for this model.
    ///     This strategy indicates how the context detects changes to properties for an instance of an entity type.
    /// </summary>
    /// <param name="changeTrackingStrategy">The change tracking strategy to be used.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was successful, <see langword="null" /> otherwise.
    /// </returns>
    IConventionModelBuilder? HasChangeTrackingStrategy(ChangeTrackingStrategy? changeTrackingStrategy, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given change tracking strategy can be set from the current configuration source
    /// </summary>
    /// <param name="changeTrackingStrategy">The change tracking strategy to be used.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given change tracking strategy can be set.</returns>
    bool CanSetChangeTrackingStrategy(ChangeTrackingStrategy? changeTrackingStrategy, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the <see cref="PropertyAccessMode" /> to use for all properties of this entity type.
    /// </summary>
    /// <remarks>
    ///     By default, the backing field, if one is found by convention or has been specified, is used when
    ///     new objects are constructed, typically when entities are queried from the database.
    ///     Properties are used for all other accesses. Calling this method will change that behavior
    ///     for all properties in the model as described in the <see cref="PropertyAccessMode" /> enum.
    /// </remarks>
    /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" /> to use for properties of this model.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was successful, <see langword="null" /> otherwise.
    /// </returns>
    IConventionModelBuilder? UsePropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given property access mode can be set from the current configuration source
    /// </summary>
    /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" /> to use for properties of this model.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given property access mode can be set.</returns>
    bool CanSetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation = false);
}
