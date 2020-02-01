// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Represents an entity in an <see cref="IConventionModel" />.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="IEntityType" /> represents a read-only view of the same metadata.
    ///     </para>
    /// </summary>
    public interface IConventionEntityType : IEntityType, IConventionTypeBase
    {
        /// <summary>
        ///     Returns the configuration source for this entity type.
        /// </summary>
        /// <returns> The configuration source. </returns>
        ConfigurationSource GetConfigurationSource();

        /// <summary>
        ///     Gets the model this entity belongs to.
        /// </summary>
        new IConventionModel Model { get; }

        /// <summary>
        ///     Gets the builder that can be used to configure this entity type.
        /// </summary>
        IConventionEntityTypeBuilder Builder { get; }

        /// <summary>
        ///     Gets the base type of this entity type. Returns <c>null</c> if this is not a derived type in an inheritance hierarchy.
        /// </summary>
        new IConventionEntityType BaseType { get; }

        /// <summary>
        ///     Gets the defining entity type.
        /// </summary>
        new IConventionEntityType DefiningEntityType { get; }

        /// <summary>
        ///     Gets a value indicating whether the entity type has no keys.
        ///     If <c>true</c> it will only be usable for queries.
        /// </summary>
        bool IsKeyless { get; }

        /// <summary>
        ///     Sets the base type of this entity type. Returns <c>null</c> if this is not a derived type in an inheritance hierarchy.
        /// </summary>
        /// <param name="entityType"> The base entity type.</param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        void HasBaseType([CanBeNull] IConventionEntityType entityType, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets a value indicating whether the entity type has no keys.
        ///     When set to <c>true</c> it will only be usable for queries.
        ///     <c>null</c> to reset to default.
        /// </summary>
        /// <param name="keyless"> A value indicating whether the entity type to has no keys. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        void HasNoKey(bool? keyless, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the primary key for this entity type.
        /// </summary>
        /// <param name="properties"> The properties that make up the primary key. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created key. </returns>
        IConventionKey SetPrimaryKey([CanBeNull] IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation = false);

        /// <summary>
        ///     Gets primary key for this entity type. Returns <c>null</c> if no primary key is defined.
        /// </summary>
        /// <returns> The primary key, or <c>null</c> if none is defined. </returns>
        new IConventionKey FindPrimaryKey();

        /// <summary>
        ///     Returns the configuration source for the primary key.
        /// </summary>
        /// <returns> The configuration source for the primary key. </returns>
        ConfigurationSource? GetPrimaryKeyConfigurationSource();

        /// <summary>
        ///     Adds a new alternate key to this entity type.
        /// </summary>
        /// <param name="properties"> The properties that make up the alternate key. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created key. </returns>
        IConventionKey AddKey([NotNull] IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation = false);

        /// <summary>
        ///     Gets the primary or alternate key that is defined on the given properties.
        ///     Returns <c>null</c> if no key is defined for the given properties.
        /// </summary>
        /// <param name="properties"> The properties that make up the key. </param>
        /// <returns> The key, or <c>null</c> if none is defined. </returns>
        new IConventionKey FindKey([NotNull] IReadOnlyList<IProperty> properties);

        /// <summary>
        ///     Gets the primary and alternate keys for this entity type.
        /// </summary>
        /// <returns> The primary and alternate keys. </returns>
        new IEnumerable<IConventionKey> GetKeys();

        /// <summary>
        ///     Removes a primary or alternate key from this entity type.
        /// </summary>
        /// <param name="key"> The key to be removed. </param>
        void RemoveKey([NotNull] IConventionKey key);

        /// <summary>
        ///     Adds a new relationship to this entity type.
        /// </summary>
        /// <param name="properties"> The properties that the foreign key is defined on. </param>
        /// <param name="principalKey"> The primary or alternate key that is referenced. </param>
        /// <param name="principalEntityType">
        ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
        ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
        ///     base type of the hierarchy).
        /// </param>
        /// <param name="setComponentConfigurationSource">
        ///     Indicates whether the configuration source should be set for the properties, principal key and principal end.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created foreign key. </returns>
        IConventionForeignKey AddForeignKey(
            [NotNull] IReadOnlyList<IConventionProperty> properties,
            [NotNull] IConventionKey principalKey,
            [NotNull] IConventionEntityType principalEntityType,
            bool setComponentConfigurationSource = true,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Gets the foreign key for the given properties that points to a given primary or alternate key.
        ///     Returns <c>null</c> if no foreign key is found.
        /// </summary>
        /// <param name="properties"> The properties that the foreign key is defined on. </param>
        /// <param name="principalKey"> The primary or alternate key that is referenced. </param>
        /// <param name="principalEntityType">
        ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
        ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
        ///     base type of the hierarchy).
        /// </param>
        /// <returns> The foreign key, or <c>null</c> if none is defined. </returns>
        new IConventionForeignKey FindForeignKey(
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType);

        /// <summary>
        ///     Gets the foreign keys defined on this entity type.
        /// </summary>
        /// <returns> The foreign keys defined on this entity type. </returns>
        new IEnumerable<IConventionForeignKey> GetForeignKeys();

        /// <summary>
        ///     Removes a foreign key from this entity type.
        /// </summary>
        /// <param name="foreignKey"> The foreign key to be removed. </param>
        void RemoveForeignKey([NotNull] IConventionForeignKey foreignKey);

        /// <summary>
        ///     Adds a new skip navigation properties to this entity type.
        /// </summary>
        /// <param name="name"> The name of the skip navigation property to add. </param>
        /// <param name="memberInfo">
        ///     <para>
        ///         The corresponding CLR type member or <c>null</c> for a shadow property.
        ///     </para>
        ///     <para>
        ///         An indexer with a <c>string</c> parameter and <c>object</c> return type can be used.
        ///     </para>
        /// </param>
        /// <param name="targetEntityType"> The entity type that the skip navigation property will hold an instance(s) of.</param>
        /// <param name="collection"> Whether the navigation property is a collection property. </param>
        /// <param name="onDependent">
        ///     Whether the navigation property is defined on the dependent side of the underlying foreign key.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created skip navigation property. </returns>
        IConventionSkipNavigation AddSkipNavigation(
            [NotNull] string name,
            [CanBeNull] MemberInfo memberInfo,
            [NotNull] IConventionEntityType targetEntityType,
            bool collection,
            bool onDependent,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Gets a skip navigation property on this entity type. Returns <c>null</c> if no navigation property is found.
        /// </summary>
        /// <param name="memberInfo"> The navigation property on the entity class. </param>
        /// <returns> The navigation property, or <c>null</c> if none is found. </returns>
        new IConventionSkipNavigation FindSkipNavigation([NotNull] MemberInfo memberInfo)
            => (IConventionSkipNavigation)((IEntityType)this).FindSkipNavigation(memberInfo);

        /// <summary>
        ///     Gets a skip navigation property on this entity type. Returns <c>null</c> if no skip navigation property is found.
        /// </summary>
        /// <param name="name"> The name of the navigation property on the entity class. </param>
        /// <returns> The navigation property, or <c>null</c> if none is found. </returns>
        new IConventionSkipNavigation FindSkipNavigation([NotNull] string name);

        /// <summary>
        ///     Gets a skip navigation property on this entity type. Does not return skip navigation properties defined on a base type.
        ///     Returns <c>null</c> if no skip navigation property is found.
        /// </summary>
        /// <param name="name"> The name of the navigation property on the entity class. </param>
        /// <returns> The navigation property, or <c>null</c> if none is found. </returns>
        new IConventionSkipNavigation FindDeclaredSkipNavigation([NotNull] string name)
            => (IConventionSkipNavigation)((IEntityType)this).FindDeclaredSkipNavigation(name);

        /// <summary>
        ///     <para>
        ///         Gets all skip navigation properties declared on this entity type.
        ///     </para>
        ///     <para>
        ///         This method does not return skip navigation properties declared declared on base types.
        ///         It is useful when iterating over all entity types to avoid processing the same foreign key more than once.
        ///         Use <see cref="GetSkipNavigations" /> to also return skip navigation properties declared on base types.
        ///     </para>
        /// </summary>
        /// <returns> Declared foreign keys. </returns>
        new IEnumerable<IConventionSkipNavigation> GetDeclaredSkipNavigations()
            => ((IEntityType)this).GetDeclaredSkipNavigations().Cast<IConventionSkipNavigation>();

        /// <summary>
        ///     Gets all skip navigation properties on this entity type.
        /// </summary>
        /// <returns> All skip navigation properties on this entity type. </returns>
        new IEnumerable<IConventionSkipNavigation> GetSkipNavigations();

        /// <summary>
        ///     Removes a skip navigation property from this entity type.
        /// </summary>
        /// <param name="navigation"> The skip navigation to be removed. </param>
        void RemoveSkipNavigation([NotNull] IConventionSkipNavigation navigation);

        /// <summary>
        ///     Adds an index to this entity type.
        /// </summary>
        /// <param name="properties"> The properties that are to be indexed. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created index. </returns>
        IConventionIndex AddIndex([NotNull] IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation = false);

        /// <summary>
        ///     Gets the index defined on the given properties. Returns <c>null</c> if no index is defined.
        /// </summary>
        /// <param name="properties"> The properties to find the index on. </param>
        /// <returns> The index, or <c>null</c> if none is found. </returns>
        new IConventionIndex FindIndex([NotNull] IReadOnlyList<IProperty> properties);

        /// <summary>
        ///     Gets the indexes defined on this entity type.
        /// </summary>
        /// <returns> The indexes defined on this entity type. </returns>
        new IEnumerable<IConventionIndex> GetIndexes();

        /// <summary>
        ///     Removes an index from this entity type.
        /// </summary>
        /// <param name="index"> The index to remove. </param>
        void RemoveIndex([NotNull] IConventionIndex index);

        /// <summary>
        ///     Adds a property to this entity type.
        /// </summary>
        /// <param name="name"> The name of the property to add. </param>
        /// <param name="propertyType"> The type of value the property will hold. </param>
        /// <param name="memberInfo">
        ///     <para>
        ///         The corresponding CLR type member or <c>null</c> for a shadow property.
        ///     </para>
        ///     <para>
        ///         An indexer with a <c>string</c> parameter and <c>object</c> return type can be used.
        ///     </para>
        /// </param>
        /// <param name="setTypeConfigurationSource"> Indicates whether the type configuration source should be set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created property. </returns>
        IConventionProperty AddProperty(
            [NotNull] string name,
            [NotNull] Type propertyType,
            [CanBeNull] MemberInfo memberInfo,
            bool setTypeConfigurationSource = true,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     <para>
        ///         Gets the property with a given name. Returns <c>null</c> if no property with the given name is defined.
        ///     </para>
        ///     <para>
        ///         This API only finds scalar properties and does not find navigation properties. Use
        ///         <see cref="ConventionEntityTypeExtensions.FindNavigation(IConventionEntityType, string)" /> to find
        ///         a navigation property.
        ///     </para>
        /// </summary>
        /// <param name="name"> The name of the property. </param>
        /// <returns> The property, or <c>null</c> if none is found. </returns>
        new IConventionProperty FindProperty([NotNull] string name);

        /// <summary>
        ///     <para>
        ///         Gets the properties defined on this entity type.
        ///     </para>
        ///     <para>
        ///         This API only returns scalar properties and does not return navigation properties. Use
        ///         <see cref="ConventionEntityTypeExtensions.GetNavigations(IConventionEntityType)" /> to get navigation
        ///         properties.
        ///     </para>
        /// </summary>
        /// <returns> The properties defined on this entity type. </returns>
        new IEnumerable<IConventionProperty> GetProperties();

        /// <summary>
        ///     Removes a property from this entity type.
        /// </summary>
        /// <param name="property"> The property to remove. </param>
        void RemoveProperty([NotNull] IConventionProperty property);

        /// <summary>
        ///     Adds a <see cref="IConventionServiceProperty" /> to this entity type.
        /// </summary>
        /// <param name="memberInfo"> The <see cref="PropertyInfo" /> or <see cref="FieldInfo" /> of the property to add. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created property. </returns>
        IConventionServiceProperty AddServiceProperty([NotNull] MemberInfo memberInfo, bool fromDataAnnotation = false);

        /// <summary>
        ///     <para>
        ///         Gets the <see cref="IConventionServiceProperty" /> with a given name.
        ///         Returns <c>null</c> if no property with the given name is defined.
        ///     </para>
        ///     <para>
        ///         This API only finds service properties and does not find scalar or navigation properties.
        ///     </para>
        /// </summary>
        /// <param name="name"> The name of the property. </param>
        /// <returns> The service property, or <c>null</c> if none is found. </returns>
        new IConventionServiceProperty FindServiceProperty([NotNull] string name);

        /// <summary>
        ///     <para>
        ///         Gets all the <see cref="IConventionServiceProperty" /> defined on this entity type.
        ///     </para>
        ///     <para>
        ///         This API only returns service properties and does not return scalar or navigation properties.
        ///     </para>
        /// </summary>
        /// <returns> The service properties defined on this entity type. </returns>
        new IEnumerable<IConventionServiceProperty> GetServiceProperties();

        /// <summary>
        ///     Removes an <see cref="IConventionServiceProperty" /> from this entity type.
        /// </summary>
        /// <param name="name"> The name of the property to remove. </param>
        /// <returns> The property that was removed. </returns>
        IConventionServiceProperty RemoveServiceProperty([NotNull] string name);
    }
}
