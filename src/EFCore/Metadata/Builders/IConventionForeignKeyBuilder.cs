// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API surface for configuring an <see cref="IConventionForeignKey" /> from conventions.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IConventionForeignKeyBuilder : IConventionAnnotatableBuilder
    {
        /// <summary>
        ///     Gets the foreign key being configured.
        /// </summary>
        new IConventionForeignKey Metadata { get; }

        /// <summary>
        ///     Configures which entity types participate in this relationship.
        ///     By calling this method the principal and dependent types can be switched or the relationship could
        ///     be moved to a base type of one of the participating entity types.
        /// </summary>
        /// <param name="principalEntityType"> The principal entity type to set. </param>
        /// <param name="dependentEntityType"> The dependent entity type to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     A builder instance if the entity types were configured as related,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder HasEntityTypes(
            [NotNull] IConventionEntityType principalEntityType,
            [NotNull] IConventionEntityType dependentEntityType,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the principal and dependent types can be switched or the relationship could
        ///     be moved to a base type of one of the participating entity types.
        /// </summary>
        /// <param name="principalEntityType"> The principal entity type to set. </param>
        /// <param name="dependentEntityType"> The dependent entity type to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     <see langword="true" /> if the principal and dependent entity types can be switched or the relationship could
        ///     be moved to a base type of one of the participating entity types.
        /// </returns>
        bool CanSetEntityTypes(
            [NotNull] IConventionEntityType principalEntityType,
            [NotNull] IConventionEntityType dependentEntityType,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the principal and dependent entity types can be switched
        ///     from the current configuration source
        /// </summary>
        /// <param name="newForeignKeyProperties">
        ///     The properties to be used as the new foreign key or <see langword="null" /> to use any compatible properties.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the principal and dependent entity types can be switched. </returns>
        bool CanInvert(
            [CanBeNull] IReadOnlyList<IConventionProperty> newForeignKeyProperties,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures the properties to use as the foreign key for this relationship.
        /// </summary>
        /// <param name="propertyNames"> The properties to use as the foreign key for this relationship.</param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the properties were configured as the foreign key,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder HasForeignKey(
            [CanBeNull] IReadOnlyList<string> propertyNames,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures the properties to use as the foreign key for this relationship.
        /// </summary>
        /// <param name="properties"> The properties to use as the foreign key for this relationship.</param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the properties were configured as the foreign key,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder HasForeignKey(
            [CanBeNull] IReadOnlyList<IConventionProperty> properties,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given properties can be set as the foreign key for this relationship
        ///     from the current configuration source.
        /// </summary>
        /// <param name="propertyNames"> The properties to use as the foreign key for this relationship. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given properties can be set as the foreign key. </returns>
        bool CanSetForeignKey([CanBeNull] IReadOnlyList<string> propertyNames, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given properties can be set as the foreign key for this relationship
        ///     from the current configuration source.
        /// </summary>
        /// <param name="properties"> The properties to use as the foreign key for this relationship. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given properties can be set as the foreign key. </returns>
        bool CanSetForeignKey([CanBeNull] IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures the properties that this relationship targets.
        /// </summary>
        /// <param name="propertyNames"> The properties for this relationship to target. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the properties were configured as the target for this relationship,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder HasPrincipalKey(
            [CanBeNull] IReadOnlyList<string> propertyNames,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures the properties that this relationship targets.
        /// </summary>
        /// <param name="properties"> The properties for this relationship to target. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the properties were configured as the target for this relationship,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder HasPrincipalKey(
            [CanBeNull] IReadOnlyList<IConventionProperty> properties,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given properties can be set as the target for this relationship
        ///     from the current configuration source
        /// </summary>
        /// <param name="propertyNames"> The properties for this relationship to target. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given properties can be set as the target. </returns>
        bool CanSetPrincipalKey([CanBeNull] IReadOnlyList<string> propertyNames, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given properties can be set as the target for this relationship
        ///     from the current configuration source
        /// </summary>
        /// <param name="properties"> The properties for this relationship to target. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given properties can be set as the target. </returns>
        bool CanSetPrincipalKey([CanBeNull] IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures the property with the given name as a navigation property used by this relationship.
        /// </summary>
        /// <param name="name"> The name of the property to use. </param>
        /// <param name="pointsToPrincipal">
        ///     A value indicating whether the navigation is on the dependent type pointing to the principal type.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the navigation property was configured,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder HasNavigation(
            [CanBeNull] string name,
            bool pointsToPrincipal,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures the given property as a navigation property used by this relationship.
        /// </summary>
        /// <param name="property"> The property to use. </param>
        /// <param name="pointsToPrincipal">
        ///     A value indicating whether the navigation is on the dependent type pointing to the principal type.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the navigation property was configured,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder HasNavigation(
            [CanBeNull] MemberInfo property,
            bool pointsToPrincipal,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures the properties with the given names as the navigation properties used by this relationship.
        /// </summary>
        /// <param name="navigationToPrincipalName">
        ///     The name of the property to use as the navigation to the principal entity type.
        ///     Can be <see langword="null" />.
        /// </param>
        /// <param name="navigationToDependentName">
        ///     The name of the property to use as the navigation to the dependent entity type.
        ///     Can be <see langword="null" />.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the navigation properties were configured,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder HasNavigations(
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures the given properties as the navigation properties used by this relationship.
        /// </summary>
        /// <param name="navigationToPrincipal">
        ///     The property to use as the navigation to the principal entity type.
        ///     Can be <see langword="null" />.
        /// </param>
        /// <param name="navigationToDependent">
        ///     The property to use as the navigation to the dependent entity type.
        ///     Can be <see langword="null" />.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the navigation properties were configured,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder HasNavigations(
            [CanBeNull] MemberInfo navigationToPrincipal,
            [CanBeNull] MemberInfo navigationToDependent,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the property with the given name can be used as a navigation for this relationship
        ///     from the current configuration source.
        /// </summary>
        /// <param name="name"> The name of the property to use. </param>
        /// <param name="pointsToPrincipal">
        ///     A value indicating whether the navigation is on the dependent type pointing to the principal type.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given property can be used as a navigation. </returns>
        bool CanSetNavigation(
            [CanBeNull] string name,
            bool pointsToPrincipal,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given property can be used as a navigation for this relationship
        ///     from the current configuration source.
        /// </summary>
        /// <param name="property"> The property to use. </param>
        /// <param name="pointsToPrincipal">
        ///     A value indicating whether the navigation is on the dependent type pointing to the principal type.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given property can be used as a navigation. </returns>
        bool CanSetNavigation(
            [CanBeNull] MemberInfo property,
            bool pointsToPrincipal,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the properties with the given names can be used as
        ///     the navigation properties for this relationship from the current configuration source
        /// </summary>
        /// <param name="navigationToPrincipalName">
        ///     The name of the property to use as the navigation to the principal entity type.
        ///     Can be <see langword="null" />.
        /// </param>
        /// <param name="navigationToDependentName">
        ///     The name of the property to use as the navigation to the dependent entity type.
        ///     Can be <see langword="null" />.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given properties can be used as navigations. </returns>
        bool CanSetNavigations(
            [CanBeNull] string navigationToPrincipalName,
            [CanBeNull] string navigationToDependentName,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given properties can be used as
        ///     the navigation properties for this relationship from the current configuration source
        /// </summary>
        /// <param name="navigationToPrincipal">
        ///     The property to use as the navigation to the principal entity type.
        ///     Can be <see langword="null" />.
        /// </param>
        /// <param name="navigationToDependent">
        ///     The property to use as the navigation to the dependent entity type.
        ///     Can be <see langword="null" />.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given properties can be used as navigations. </returns>
        bool CanSetNavigations(
            [CanBeNull] MemberInfo navigationToPrincipal,
            [CanBeNull] MemberInfo navigationToDependent,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures whether this is a required relationship (i.e. whether none of the foreign key properties can
        ///     be assigned <see langword="null" />).
        /// </summary>
        /// <param name="required">
        ///     A value indicating whether this is a required relationship.
        ///     <see langword="null" /> to reset to default.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the requiredness was configured,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder IsRequired(bool? required, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the relationship requiredness can be configured
        ///     from the current configuration source.
        /// </summary>
        /// <param name="required">
        ///     A value indicating whether this is a required relationship.
        ///     <see langword="null" /> to reset to default.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the relationship requiredness can be configured. </returns>
        bool CanSetIsRequired(bool? required, bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures whether the dependent end is required (i.e. whether the principal to dependent navigation can
        ///     be assigned <see langword="null" />).
        /// </summary>
        /// <param name="required">
        ///     A value indicating whether the dependent end is required.
        ///     <see langword="null" /> to reset to default.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the dependent end requiredness was configured,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder IsRequiredDependent(bool? required, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the dependent end requiredness can be configured
        ///     from the current configuration source.
        /// </summary>
        /// <param name="required">
        ///     A value indicating whether this is a required relationship.
        ///     <see langword="null" /> to reset to default.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the relationship requiredness can be configured. </returns>
        bool CanSetIsRequiredDependent(bool? required, bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures whether this relationship defines an ownership
        ///     (i.e. whether the dependent entity must always be accessed via the navigation from the principal entity).
        /// </summary>
        /// <param name="ownership">
        ///     A value indicating whether this relationship defines an ownership.
        ///     <see langword="null" /> to reset to default.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the ownership was configured,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder IsOwnership(bool? ownership, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether this relationship can be configured as defining an ownership or not
        ///     from the current configuration source.
        /// </summary>
        /// <param name="ownership">
        ///     A value indicating whether this relationship defines an ownership.
        ///     <see langword="null" /> to reset to default.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the relationship can be configured as defining an ownership. </returns>
        bool CanSetIsOwnership(bool? ownership, bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures whether the dependent entity is unique
        ///     (i.e. whether the navigation to the dependent entity type is not a collection).
        /// </summary>
        /// <param name="unique">
        ///     A value indicating whether the dependent entity is unique.
        ///     <see langword="null" /> to reset to default.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the uniqueness was configured,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder IsUnique(bool? unique, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether this relationship uniqueness can be configured
        ///     from the current configuration source.
        /// </summary>
        /// <param name="unique">
        ///     A value indicating whether the dependent entity is unique.
        ///     <see langword="null" /> to reset to default.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the relationship uniqueness can be configured. </returns>
        bool CanSetIsUnique(bool? unique, bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures the operation applied to dependent entities in the relationship when the
        ///     principal is deleted or the relationship is severed.
        /// </summary>
        /// <param name="deleteBehavior">
        ///     The action to perform.
        ///     <see langword="null" /> to reset to default.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the delete operation was configured,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder OnDelete(DeleteBehavior? deleteBehavior, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the operation on principal deletion can be configured
        ///     from the current configuration source.
        /// </summary>
        /// <param name="deleteBehavior">
        ///     The action to perform.
        ///     <see langword="null" /> to reset to default.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the operation on principal deletion can be configured </returns>
        bool CanSetOnDelete(DeleteBehavior? deleteBehavior, bool fromDataAnnotation = false);
    }
}
