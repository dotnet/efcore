// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API surface for configuring an <see cref="IConventionModelBuilder" /> from conventions.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IConventionModelBuilder : IConventionAnnotatableBuilder
    {
        /// <summary>
        ///     The model being configured.
        /// </summary>
        new IConventionModel Metadata { get; }

        /// <summary>
        ///     Returns an object that can be used to configure a given entity type in the model.
        ///     If an entity type with the provided name is not already part of the model,
        ///     a new shadow entity type will be added to the model.
        /// </summary>
        /// <param name="name"> The name of the entity type to be configured. </param>
        /// <param name="shouldBeOwned">
        ///     <c>true</c> if the entity type should be owned,
        ///     <c>false</c> if the entity type should not be owned
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the entity type if the entity type was added or already part of the model,
        ///     <c>null</c> otherwise.
        /// </returns>
        IConventionEntityTypeBuilder Entity([NotNull] string name, bool? shouldBeOwned = false, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns an object that can be used to configure a given entity type in the model.
        ///     If an entity type with the provided type is not already part of the model,
        ///     a new entity type will be added to the model.
        /// </summary>
        /// <param name="type"> The type of the entity type to be configured. </param>
        /// <param name="shouldBeOwned">
        ///     <c>true</c> if the entity type should be owned,
        ///     <c>false</c> if the entity type should not be owned
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the entity type if the entity type was added or already part of the model,
        ///     <c>null</c> otherwise.
        /// </returns>
        IConventionEntityTypeBuilder Entity([NotNull] Type type, bool? shouldBeOwned = false, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns an object that can be used to configure a given entity type with defining navigation.
        ///     If an entity type with the provided name is not already part of the model,
        ///     a new shadow entity type will be added to the model.
        /// </summary>
        /// <param name="name"> The name of the entity type to be configured. </param>
        /// <param name="definingNavigationName"> The defining navigation. </param>
        /// <param name="definingEntityType"> The defining entity type. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the entity type if the entity type was added or already part of the model,
        ///     <c>null</c> otherwise.
        /// </returns>
        IConventionEntityTypeBuilder Entity(
            [NotNull] string name,
            [NotNull] string definingNavigationName,
            [NotNull] IConventionEntityType definingEntityType,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns an object that can be used to configure a given entity type with defining navigation.
        ///     If an entity type with the provided type is not already part of the model,
        ///     a new entity type will be added to the model.
        /// </summary>
        /// <param name="type"> The type of the entity type to be configured. </param>
        /// <param name="definingNavigationName"> The defining navigation. </param>
        /// <param name="definingEntityType"> The defining entity type. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the entity type if the entity type was added or already part of the model,
        ///     <c>null</c> otherwise.
        /// </returns>
        IConventionEntityTypeBuilder Entity(
            [NotNull] Type type,
            [NotNull] string definingNavigationName,
            [NotNull] IConventionEntityType definingEntityType,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Marks an entity type as owned. All references to this type will be configured as
        ///     separate owned type instances.
        /// </summary>
        /// <param name="type"> The entity type to be configured. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to provide default configuration for the owned entity types.
        /// </returns>
        IConventionOwnedEntityTypeBuilder Owned([NotNull] Type type, bool fromDataAnnotation = false);

        /// <summary>
        ///     Indicates whether the given entity type name is ignored for the current configuration source.
        /// </summary>
        /// <param name="type"> The name of the entity type that might be ignored. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given entity type name is ignored. </returns>
        bool IsIgnored([NotNull] Type type, bool fromDataAnnotation = false);

        /// <summary>
        ///     Indicates whether the given entity type name is ignored for the current configuration source.
        /// </summary>
        /// <param name="typeName"> The name of the entity type that might be ignored. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given entity type name is ignored. </returns>
        bool IsIgnored([NotNull] string typeName, bool fromDataAnnotation = false);

        /// <summary>
        ///     Excludes the given entity type from the model and prevents it from being added by convention.
        /// </summary>
        /// <param name="type"> The entity type to be removed from the model. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance so that additional configuration calls can be chained
        ///     if the given entity type was ignored, <c>null</c> otherwise.
        /// </returns>
        IConventionModelBuilder Ignore([NotNull] Type type, bool fromDataAnnotation = false);

        /// <summary>
        ///     Excludes the given entity type name from the model and prevents it from being added by convention.
        /// </summary>
        /// <param name="typeName"> The entity type name to be removed from the model. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given entity type name was ignored. </returns>
        /// <returns>
        ///     The same builder instance if the given entity type name was ignored, <c>null</c> otherwise.
        /// </returns>
        IConventionModelBuilder Ignore([NotNull] string typeName, bool fromDataAnnotation = false);

        /// <summary>
        ///     Removes the given entity type from the model.
        /// </summary>
        /// <param name="entityType"> The entity type to be removed from the model. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the given entity type was removed, <c>null</c> otherwise.
        /// </returns>
        IConventionModelBuilder HasNoEntityType([NotNull] IConventionEntityType entityType, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given entity type can be ignored from the current configuration source
        /// </summary>
        /// <param name="type"> The entity type to be removed from the model. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given entity type can be ignored. </returns>
        bool CanIgnore([NotNull] Type type, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given entity type name can be ignored from the current configuration source
        /// </summary>
        /// <param name="typeName"> The entity type name to be removed from the model. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given entity type name can be ignored. </returns>
        bool CanIgnore([NotNull] string typeName, bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures the default <see cref="ChangeTrackingStrategy" /> to be used for this model.
        ///     This strategy indicates how the context detects changes to properties for an instance of an entity type.
        /// </summary>
        /// <param name="changeTrackingStrategy"> The change tracking strategy to be used. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was successful, <c>null</c> otherwise.
        /// </returns>
        IConventionModelBuilder HasChangeTrackingStrategy(ChangeTrackingStrategy? changeTrackingStrategy, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given change tracking strategy can be set from the current configuration source
        /// </summary>
        /// <param name="changeTrackingStrategy"> The change tracking strategy to be used. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given change tracking strategy can be set. </returns>
        bool CanSetChangeTrackingStrategy(ChangeTrackingStrategy? changeTrackingStrategy, bool fromDataAnnotation = false);

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="PropertyAccessMode" /> to use for all properties of this entity type.
        ///     </para>
        ///     <para>
        ///         By default, the backing field, if one is found by convention or has been specified, is used when
        ///         new objects are constructed, typically when entities are queried from the database.
        ///         Properties are used for all other accesses. Calling this method will change that behavior
        ///         for all properties in the model as described in the <see cref="PropertyAccessMode" /> enum.
        ///     </para>
        /// </summary>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" /> to use for properties of this model. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was successful, <c>null</c> otherwise.
        /// </returns>
        IConventionModelBuilder UsePropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given property access mode can be set from the current configuration source
        /// </summary>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" /> to use for properties of this model. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given property access mode can be set. </returns>
        bool CanSetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation = false);
    }
}
