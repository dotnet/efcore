// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API surface for configuring an <see cref="IConventionEntityType" /> from conventions.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IConventionEntityTypeBuilder : IConventionAnnotatableBuilder
    {
        /// <summary>
        ///     Gets the entity type being configured.
        /// </summary>
        new IConventionEntityType Metadata { get; }

        /// <summary>
        ///     Sets the base type of this entity type in an inheritance hierarchy.
        /// </summary>
        /// <param name="baseEntityType"> The base entity type or <see langword="null" /> to indicate no base type. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the base type was configured,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionEntityTypeBuilder HasBaseType(
            [CanBeNull] IConventionEntityType baseEntityType,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given type can be set as the base type of this entity type.
        /// </summary>
        /// <param name="baseEntityType"> The base entity type or <see langword="null" /> to indicate no base type. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given type can be set as the base type of this entity type. </returns>
        bool CanSetBaseType([CanBeNull] IConventionEntityType baseEntityType, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns an object that can be used to configure the property with the given name.
        ///     If no matching property exists, then a new property will be added.
        /// </summary>
        /// <param name="propertyType"> The type of value the property will hold. </param>
        /// <param name="propertyName"> The name of the property to be configured. </param>
        /// <param name="setTypeConfigurationSource"> Indicates whether the type configuration source should be set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the property if it exists on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionPropertyBuilder Property(
            [NotNull] Type propertyType,
            [NotNull] string propertyName,
            bool setTypeConfigurationSource = true,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns an object that can be used to configure the property with the given member info.
        ///     If no matching property exists, then a new property will be added.
        /// </summary>
        /// <param name="memberInfo"> The <see cref="PropertyInfo" /> or <see cref="FieldInfo" /> of the property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the property if it exists on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionPropertyBuilder Property([NotNull] MemberInfo memberInfo, bool fromDataAnnotation = false);

        /// <summary>
        ///     Creates a property with a name that's different from any existing properties.
        /// </summary>
        /// <param name="basePropertyName"> The desired property name. </param>
        /// <param name="propertyType"> The type of value the property will hold. </param>
        /// <param name="required"> A value indicating whether the property is required. </param>
        /// <returns>
        ///     An object that can be used to configure the property if it exists on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionPropertyBuilder CreateUniqueProperty([NotNull] Type propertyType, [NotNull] string basePropertyName, bool required);

        /// <summary>
        ///     Returns the existing properties with the given names or creates them if matching CLR members are found.
        /// </summary>
        /// <param name="propertyNames"> The names of the properties. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> A list of properties if they exist on the entity type, <see langword="null" /> otherwise. </returns>
        IReadOnlyList<IConventionProperty> GetOrCreateProperties(
            [CanBeNull] IReadOnlyList<string> propertyNames,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns the existing properties matching the given members or creates them.
        /// </summary>
        /// <param name="memberInfos"> The type members. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> A list of properties if they exist on the entity type, <see langword="null" /> otherwise. </returns>
        IReadOnlyList<IConventionProperty> GetOrCreateProperties(
            [CanBeNull] IEnumerable<MemberInfo> memberInfos,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Removes properties in the given list if they are not part of any metadata object.
        /// </summary>
        /// <param name="properties"> The properties to remove. </param>
        IConventionEntityTypeBuilder RemoveUnusedImplicitProperties([NotNull] IReadOnlyList<IConventionProperty> properties);

        /// <summary>
        ///     Removes shadow properties in the given list if they are not part of any metadata object.
        /// </summary>
        /// <param name="properties"> The properties to remove. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        [Obsolete("Use RemoveUnusedImplicitProperties")]
        IConventionEntityTypeBuilder RemoveUnusedShadowProperties(
            [NotNull] IReadOnlyList<IConventionProperty> properties,
            bool fromDataAnnotation = false)
            => RemoveUnusedImplicitProperties(properties);

        /// <summary>
        ///     Returns an object that can be used to configure the service property with the given member info.
        ///     If no matching property exists, then a new property will be added.
        /// </summary>
        /// <param name="memberInfo"> The <see cref="PropertyInfo" /> or <see cref="FieldInfo" /> of the property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the property if it exists on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionServicePropertyBuilder ServiceProperty(
            [NotNull] MemberInfo memberInfo,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Indicates whether the given member name is ignored for the given configuration source.
        /// </summary>
        /// <param name="memberName"> The name of the member that might be ignored. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     <see langword="false" /> if the entity type contains a member with the given name,
        ///     the given member name hasn't been ignored or it was ignored using a lower configuration source;
        ///     <see langword="true" /> otherwise.
        /// </returns>
        bool IsIgnored([NotNull] string memberName, bool fromDataAnnotation = false);

        /// <summary>
        ///     Excludes the given property from the entity type and prevents conventions from adding a matching property
        ///     or navigation to the type.
        /// </summary>
        /// <param name="memberName"> The name of the member to be removed. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same <see cref="IConventionEntityTypeBuilder" /> instance so that additional configuration calls can be chained
        ///     if the given member was ignored, <see langword="null" /> otherwise.
        /// </returns>
        IConventionEntityTypeBuilder Ignore([NotNull] string memberName, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given member name can be ignored from the given configuration source.
        /// </summary>
        /// <param name="memberName"> The member name to be removed from the entity type. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given member name can be ignored. </returns>
        /// <returns>
        ///     <see langword="false" /> if the entity type contains a member with the given name
        ///     that was configured using a higher configuration source;
        ///     <see langword="true" /> otherwise.
        /// </returns>
        bool CanIgnore([NotNull] string memberName, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the properties that make up the primary key for this entity type.
        /// </summary>
        /// <param name="properties"> The properties that make up the primary key. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> An object that can be used to configure the primary key. </returns>
        /// <returns>
        ///     An object that can be used to configure the primary key if it was set on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionKeyBuilder PrimaryKey([CanBeNull] IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given properties can be set as the primary key for this entity type.
        /// </summary>
        /// <param name="properties"> The properties that make up the primary key. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given properties can be set as the primary key. </returns>
        bool CanSetPrimaryKey([CanBeNull] IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation = false);

        /// <summary>
        ///     Creates an alternate key in the model for this entity type if one does not already exist over the specified
        ///     properties.
        /// </summary>
        /// <param name="properties"> The properties that make up the key. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the key if it exists on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionKeyBuilder HasKey([NotNull] IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation = false);

        /// <summary>
        ///     Removes a primary or alternate key from this entity type.
        /// </summary>
        /// <param name="properties"> The properties that make up the key. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The key that was removed. </returns>
        IConventionEntityTypeBuilder HasNoKey([NotNull] IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation = false);

        /// <summary>
        ///     Removes a primary or alternate key from this entity type.
        /// </summary>
        /// <param name="key"> The key to be removed. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the key was removed,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionEntityTypeBuilder HasNoKey([NotNull] IConventionKey key, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the key can be removed from this entity type.
        /// </summary>
        /// <param name="key"> The key to be removed. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the key can be removed from this entity type. </returns>
        bool CanRemoveKey([NotNull] IConventionKey key, bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures the entity type to have no keys. It will only be usable for queries.
        /// </summary>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the entity type was configured as keyless,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionEntityTypeBuilder HasNoKey(bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the entity type can be marked as keyless.
        /// </summary>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the entity type can be marked as keyless. </returns>
        bool CanRemoveKey(bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures an index on the specified property names.
        ///     If there is an existing index on the given list of property names,
        ///     then the existing index will be returned for configuration.
        /// </summary>
        /// <param name="propertyNames"> The names of the properties that make up the index. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the index if it exists on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionIndexBuilder HasIndex(
            [NotNull] IReadOnlyList<string> propertyNames,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures an index on the specified property names.
        ///     If there is an existing index on the given list of properyt names,
        ///     then the existing index will be returned for configuration.
        /// </summary>
        /// <param name="propertyNames"> The names of the properties that make up the index. </param>
        /// <param name="name"> The name of the index. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the index if it exists on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionIndexBuilder HasIndex(
            [NotNull] IReadOnlyList<string> propertyNames,
            [NotNull] string name,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures an index on the specified properties.
        ///     If there is an existing index on the given list of properties,
        ///     then the existing index will be returned for configuration.
        /// </summary>
        /// <param name="properties"> The properties that make up the index. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the index if it exists on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionIndexBuilder HasIndex(
            [NotNull] IReadOnlyList<IConventionProperty> properties,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures an index on the specified properties, with the specified name.
        ///     If there is an existing index on the given set of properties and with the given name,
        ///     then the existing index will be returned for configuration.
        /// </summary>
        /// <param name="properties"> The properties that make up the index. </param>
        /// <param name="name"> The name of the index. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the index if it exists on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionIndexBuilder HasIndex(
            [NotNull] IReadOnlyList<IConventionProperty> properties,
            [NotNull] string name,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Removes an index from this entity type.
        /// </summary>
        /// <param name="properties"> The properties that make up the index. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the index was removed or didn't exist,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionEntityTypeBuilder HasNoIndex([NotNull] IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation = false);

        /// <summary>
        ///     Removes an index from this entity type.
        /// </summary>
        /// <param name="index"> The index to remove. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the index was removed,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionEntityTypeBuilder HasNoIndex([NotNull] IConventionIndex index, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the index can be removed from this entity type.
        /// </summary>
        /// <param name="index"> The index to remove. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the entity type can be marked as keyless. </returns>
        bool CanRemoveIndex([NotNull] IConventionIndex index, bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures a relationship between this and the target entity type.
        /// </summary>
        /// <param name="targetEntityType"> The entity type that this relationship targets. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        IConventionForeignKeyBuilder HasRelationship(
            [NotNull] IConventionEntityType targetEntityType,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures a relationship between this and the target entity type with the target as the principal end.
        /// </summary>
        /// <param name="principalEntityType"> The entity type that this relationship targets. </param>
        /// <param name="dependentProperties"> The properties on this type that make up the foreign key. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the relationship if it exists on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder HasRelationship(
            [NotNull] IConventionEntityType principalEntityType,
            [NotNull] IReadOnlyList<IConventionProperty> dependentProperties,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures a relationship between this and the target entity type with the target as the principal end.
        /// </summary>
        /// <param name="principalEntityType"> The entity type that this relationship targets. </param>
        /// <param name="principalKey"> The referenced key. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the relationship if it exists on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder HasRelationship(
            [NotNull] IConventionEntityType principalEntityType,
            [NotNull] IConventionKey principalKey,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures a relationship between this and the target entity type with the target as the principal end.
        /// </summary>
        /// <param name="principalEntityType"> The entity type that this relationship targets. </param>
        /// <param name="dependentProperties"> The properties on this type that make up the foreign key </param>
        /// <param name="principalKey"> The referenced key. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the relationship if it exists on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder HasRelationship(
            [NotNull] IConventionEntityType principalEntityType,
            [NotNull] IReadOnlyList<IConventionProperty> dependentProperties,
            [NotNull] IConventionKey principalKey,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures a relationship between this and the target entity type.
        /// </summary>
        /// <param name="targetEntityType"> The entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the navigation property on this entity type that is part of the relationship.
        /// </param>
        /// <param name="setTargetAsPrincipal"> A value indicating whether the target entity type should be configured as the principal end. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the relationship if it exists on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder HasRelationship(
            [NotNull] IConventionEntityType targetEntityType,
            [NotNull] string navigationName,
            bool setTargetAsPrincipal = false,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures a relationship between this and the target entity type.
        /// </summary>
        /// <param name="targetEntityType"> The entity type that this relationship targets. </param>
        /// <param name="navigation"> The navigation property on this entity type that is part of the relationship. </param>
        /// <param name="setTargetAsPrincipal"> A value indicating whether the target entity type should be configured as the principal end. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the relationship if it exists on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder HasRelationship(
            [NotNull] IConventionEntityType targetEntityType,
            [NotNull] MemberInfo navigation,
            bool setTargetAsPrincipal = false,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures a relationship between this and the target entity type.
        /// </summary>
        /// <param name="targetEntityType"> The entity type that this relationship targets. </param>
        /// <param name="navigationName"> The name of the navigation property on this entity type that is part of the relationship. </param>
        /// <param name="inverseNavigationName">
        ///     The name of the navigation property on the target entity type that is part of the relationship. If <see langword="null" />
        ///     is specified, the relationship will be configured without a navigation property on the target end.
        /// </param>
        /// <param name="setTargetAsPrincipal"> A value indicating whether the target entity type should be configured as the principal end. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the relationship if it exists on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder HasRelationship(
            [NotNull] IConventionEntityType targetEntityType,
            [NotNull] string navigationName,
            [CanBeNull] string inverseNavigationName,
            bool setTargetAsPrincipal = false,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures a relationship between this and the target entity type.
        /// </summary>
        /// <param name="targetEntityType"> The entity type that this relationship targets. </param>
        /// <param name="navigation"> The navigation property on this entity type that is part of the relationship. </param>
        /// <param name="inverseNavigation">
        ///     The navigation property on the target entity type that is part of the relationship. If <see langword="null" />
        ///     is specified, the relationship will be configured without a navigation property on the target end.
        /// </param>
        /// <param name="setTargetAsPrincipal"> A value indicating whether the target entity type should be configured as the principal end. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the relationship if it exists on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder HasRelationship(
            [NotNull] IConventionEntityType targetEntityType,
            [NotNull] MemberInfo navigation,
            [CanBeNull] MemberInfo inverseNavigation,
            bool setTargetAsPrincipal = false,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures a relationship where the target entity is owned by (or part of) this entity.
        /// </summary>
        /// <param name="targetEntityType"> The type that this relationship targets. </param>
        /// <param name="navigationName"> The name of the navigation property on this entity type that is part of the relationship. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        IConventionForeignKeyBuilder HasOwnership(
            [NotNull] Type targetEntityType,
            [NotNull] string navigationName,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures a relationship where the target entity is owned by (or part of) this entity.
        /// </summary>
        /// <param name="targetEntityType"> The type that this relationship targets. </param>
        /// <param name="navigation"> The navigation property on this entity type that is part of the relationship. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the relationship if it exists on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder HasOwnership(
            [NotNull] Type targetEntityType,
            [NotNull] MemberInfo navigation,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures a relationship where the target entity is owned by (or part of) this entity.
        /// </summary>
        /// <param name="targetEntityType"> The type that this relationship targets. </param>
        /// <param name="navigationName"> The name of the navigation property on this entity type that is part of the relationship. </param>
        /// <param name="inverseNavigationName">
        ///     The name of the navigation property on the target entity type that is part of the relationship. If <see langword="null" />
        ///     is specified, the relationship will be configured without a navigation property on the target end.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the relationship if it exists on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder HasOwnership(
            [NotNull] Type targetEntityType,
            [NotNull] string navigationName,
            [CanBeNull] string inverseNavigationName,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures a relationship where the target entity is owned by (or part of) this entity.
        /// </summary>
        /// <param name="targetEntityType"> The type that this relationship targets. </param>
        /// <param name="navigation"> The navigation property on this entity type that is part of the relationship. </param>
        /// <param name="inverseNavigation">
        ///     The navigation property on the target entity type that is part of the relationship. If <see langword="null" />
        ///     is specified, the relationship will be configured without a navigation property on the target end.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the relationship if it exists on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionForeignKeyBuilder HasOwnership(
            [NotNull] Type targetEntityType,
            [NotNull] MemberInfo navigation,
            [CanBeNull] MemberInfo inverseNavigation,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Removes a relationship from this entity type.
        /// </summary>
        /// <param name="properties"> The properties that the foreign key is defined on. </param>
        /// <param name="principalKey"> The primary or alternate key that is referenced. </param>
        /// <param name="principalEntityType">
        ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
        ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
        ///     base type of the hierarchy).
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the relationship was removed or didn't exist,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionEntityTypeBuilder HasNoRelationship(
            [NotNull] IReadOnlyList<IConventionProperty> properties,
            [NotNull] IConventionKey principalKey,
            [NotNull] IConventionEntityType principalEntityType,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Removes a foreign key from this entity type.
        /// </summary>
        /// <param name="foreignKey"> The foreign key to be removed. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the foreign key was removed,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionEntityTypeBuilder HasNoRelationship([NotNull] IConventionForeignKey foreignKey, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the foreign key can be removed from this entity type.
        /// </summary>
        /// <param name="foreignKey"> The foreign key to be removed. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the foreign key can be removed from this entity type. </returns>
        bool CanRemoveRelationship([NotNull] IConventionForeignKey foreignKey, bool fromDataAnnotation = false);

        /// <summary>
        ///     Removes a skip navigation from this entity type.
        /// </summary>
        /// <param name="skipNavigation"> The skip navigation to be removed. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the skip navigation was removed,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionEntityTypeBuilder HasNoSkipNavigation([NotNull] ISkipNavigation skipNavigation, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the skip navigation can be removed from this entity type.
        /// </summary>
        /// <param name="skipNavigation"> The skip navigation to be removed. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the skip navigation can be removed from this entity type. </returns>
        bool CanRemoveSkipNavigation([NotNull] ISkipNavigation skipNavigation, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given navigation can be added to this entity type.
        /// </summary>
        /// <param name="navigationName"> The name of the navigation. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        [Obsolete("Use CanHaveNavigation")]
        bool CanAddNavigation([NotNull] string navigationName, bool fromDataAnnotation = false)
            => CanHaveNavigation(navigationName, fromDataAnnotation);

        /// <summary>
        ///     Returns a value indicating whether the given navigation can be added to this entity type.
        /// </summary>
        /// <param name="navigationName"> The name of the navigation. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        bool CanHaveNavigation([NotNull] string navigationName, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given skip navigation can be added to this entity type.
        /// </summary>
        /// <param name="skipNavigationName"> The name of the skip navigation. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        bool CanHaveSkipNavigation([NotNull] string skipNavigationName, bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures a skip navigation and the inverse between this and the target entity type.
        /// </summary>
        /// <param name="targetEntityType"> The entity type that this relationship targets. </param>
        /// <param name="navigation"> The navigation property on this entity type that is part of the relationship. </param>
        /// <param name="inverseNavigation">
        ///     The navigation property on the target entity type that is part of the relationship. If <see langword="null" />
        ///     is specified, the relationship will be configured without a navigation property on the target end.
        /// </param>
        /// <param name="collections"> Whether both of the navigation properties are collections or aren't collections. </param>
        /// <param name="onDependent">
        ///     Whether both of the navigation property are defined on the dependent side of the underlying foreign keys.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the relationship if it exists on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionSkipNavigationBuilder HasSkipNavigation(
            [NotNull] MemberInfo navigation,
            [NotNull] IConventionEntityType targetEntityType,
            [NotNull] MemberInfo inverseNavigation,
            bool? collections = null,
            bool? onDependent = null,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures a skip navigation between this and the target entity type.
        /// </summary>
        /// <param name="navigation"> The navigation property. </param>
        /// <param name="targetEntityType"> The entity type that the navigation targets. </param>
        /// <param name="collection"> Whether the navigation property is a collection property. </param>
        /// <param name="onDependent">
        ///     Whether the navigation property is defined on the dependent side of the underlying foreign key.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the relationship if it exists on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionSkipNavigationBuilder HasSkipNavigation(
            [NotNull] MemberInfo navigation,
            [NotNull] IConventionEntityType targetEntityType,
            bool? collection = null,
            bool? onDependent = null,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures a skip navigation between this and the target entity type.
        /// </summary>
        /// <param name="navigationName"> The navigation property name. </param>
        /// <param name="targetEntityType"> The entity type that the navigation targets. </param>
        /// <param name="collection"> Whether the navigation property is a collection property. </param>
        /// <param name="onDependent">
        ///     Whether the navigation property is defined on the dependent side of the underlying foreign key.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     An object that can be used to configure the relationship if it exists on the entity type,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionSkipNavigationBuilder HasSkipNavigation(
            [NotNull] string navigationName,
            [NotNull] IConventionEntityType targetEntityType,
            bool? collection = null,
            bool? onDependent = null,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Specifies a LINQ predicate expression that will automatically be applied to any queries targeting
        ///     this entity type.
        /// </summary>
        /// <param name="filter"> The LINQ predicate expression. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the query filter was set,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionEntityTypeBuilder HasQueryFilter([CanBeNull] LambdaExpression filter, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given query filter can be set from the current configuration source.
        /// </summary>
        /// <param name="filter"> The LINQ predicate expression. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given query filter can be set. </returns>
        bool CanSetQueryFilter([CanBeNull] LambdaExpression filter, bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures a query used to provide data for a keyless entity type.
        /// </summary>
        /// <param name="query"> The query that will provide the underlying data for the keyless entity type. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the query was set, <see langword="null" /> otherwise.
        /// </returns>
        [Obsolete("Use InMemoryEntityTypeBuilderExtensions.ToInMemoryQuery")]
        IConventionEntityTypeBuilder HasDefiningQuery([CanBeNull] LambdaExpression query, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given defining query can be set from the current configuration source.
        /// </summary>
        /// <param name="query"> The query that will provide the underlying data for the keyless entity type. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given defining query can be set. </returns>
        [Obsolete("Use InMemoryEntityTypeBuilderExtensions.CanSetInMemoryQuery")]
        bool CanSetDefiningQuery([CanBeNull] LambdaExpression query, bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures the <see cref="ChangeTrackingStrategy" /> to be used for this entity type.
        ///     This strategy indicates how the context detects changes to properties for an instance of the entity type.
        /// </summary>
        /// <param name="changeTrackingStrategy">
        ///     The change tracking strategy to be used.
        ///     <see langword="null" /> to reset to default.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the <see cref="ChangeTrackingStrategy" /> was set,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionEntityTypeBuilder HasChangeTrackingStrategy(
            ChangeTrackingStrategy? changeTrackingStrategy,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given change tracking strategy can be set from the current configuration source.
        /// </summary>
        /// <param name="changeTrackingStrategy">
        ///     The change tracking strategy to be used.
        ///     <see langword="null" /> to reset to default.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given change tracking strategy can be set. </returns>
        bool CanSetChangeTrackingStrategy(ChangeTrackingStrategy? changeTrackingStrategy, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the <see cref="PropertyAccessMode" /> to use for all properties of this entity type.
        /// </summary>
        /// <param name="propertyAccessMode">
        ///     The <see cref="PropertyAccessMode" /> to use for properties of this entity type.
        ///     <see langword="null" /> to reset to default.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        /// <returns>
        ///     The same builder instance if the <see cref="PropertyAccessMode" /> was set,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionEntityTypeBuilder UsePropertyAccessMode(
            PropertyAccessMode? propertyAccessMode,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given <see cref="PropertyAccessMode" /> can be set from the current configuration source.
        /// </summary>
        /// <param name="propertyAccessMode">
        ///     The <see cref="PropertyAccessMode" /> to use for properties of this model.
        ///     <see langword="null" /> to reset to default.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given <see cref="PropertyAccessMode" /> can be set. </returns>
        bool CanSetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures the discriminator property used to identify which entity type each row in a table represents
        ///     when an inheritance hierarchy is mapped to a single table in a relational database.
        /// </summary>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> A builder that allows the discriminator property to be configured. </returns>
        IConventionDiscriminatorBuilder HasDiscriminator(bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures the discriminator property used to identify which entity type each row in a table represents
        ///     when an inheritance hierarchy is mapped to a single table in a relational database.
        /// </summary>
        /// <param name="type"> The type of values stored in the discriminator property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> A builder that allows the discriminator property to be configured. </returns>
        IConventionDiscriminatorBuilder HasDiscriminator([NotNull] Type type, bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures the discriminator property used to identify which entity type each row in a table represents
        ///     when an inheritance hierarchy is mapped to a single table in a relational database.
        /// </summary>
        /// <param name="name"> The name of the discriminator property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> A builder that allows the discriminator property to be configured. </returns>
        IConventionDiscriminatorBuilder HasDiscriminator([NotNull] string name, bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures the discriminator property used to identify which entity type each row in a table represents
        ///     when an inheritance hierarchy is mapped to a single table in a relational database.
        /// </summary>
        /// <param name="name"> The name of the discriminator property. </param>
        /// <param name="type"> The type of values stored in the discriminator property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> A builder that allows the discriminator property to be configured. </returns>
        IConventionDiscriminatorBuilder HasDiscriminator([NotNull] string name, [NotNull] Type type, bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures the discriminator property used to identify which entity type each row in a table represents
        ///     when an inheritance hierarchy is mapped to a single table in a relational database.
        /// </summary>
        /// <param name="memberInfo"> The property mapped to the discriminator property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> A builder that allows the discriminator property to be configured. </returns>
        IConventionDiscriminatorBuilder HasDiscriminator([NotNull] MemberInfo memberInfo, bool fromDataAnnotation = false);

        /// <summary>
        ///     Removes the discriminator property from this entity type.
        ///     This method is usually called when the entity type is no longer mapped to the same table as any other type in
        ///     the hierarchy or when this entity type is no longer the root type.
        /// </summary>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the discriminator was configured,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionEntityTypeBuilder HasNoDiscriminator(bool fromDataAnnotation = false);

        /// <summary>
        ///     Removes the discriminator property from this entity type.
        ///     This method is usually called when the entity type is no longer mapped to the same table as any other type in
        ///     the hierarchy or when this entity type is no longer the root type.
        /// </summary>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the discriminator was configured,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        [Obsolete("Use HasNoDiscriminator")]
        IConventionEntityTypeBuilder HasNoDeclaredDiscriminator(bool fromDataAnnotation = false)
            => HasNoDiscriminator(fromDataAnnotation);

        /// <summary>
        ///     Returns a value indicating whether the discriminator property can be configured.
        /// </summary>
        /// <param name="name"> The name of the discriminator property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        bool CanSetDiscriminator([NotNull] string name, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the discriminator property can be configured.
        /// </summary>
        /// <param name="type"> The type of values stored in the discriminator property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        bool CanSetDiscriminator([NotNull] Type type, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the discriminator property can be configured.
        /// </summary>
        /// <param name="type"> The type of values stored in the discriminator property. </param>
        /// <param name="name"> The name of the discriminator property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        bool CanSetDiscriminator([NotNull] string name, [NotNull] Type type, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the discriminator property can be configured.
        /// </summary>
        /// <param name="memberInfo"> The property mapped to the discriminator property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        bool CanSetDiscriminator([NotNull] MemberInfo memberInfo, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the discriminator property can be removed.
        /// </summary>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the discriminator property can be removed. </returns>
        bool CanRemoveDiscriminator(bool fromDataAnnotation = false);
    }
}
