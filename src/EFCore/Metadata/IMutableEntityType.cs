// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Represents an entity in an <see cref="IMutableModel" />.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="IEntityType" /> represents a ready-only view of the same metadata.
    ///     </para>
    /// </summary>
    public interface IMutableEntityType : IEntityType, IMutableTypeBase
    {
        /// <summary>
        ///     Gets the model this entity belongs to.
        /// </summary>
        new IMutableModel Model { get; }

        /// <summary>
        ///     Gets or sets the base type of the entity. Returns null if this is not a derived type in an inheritance hierarchy.
        /// </summary>
        new IMutableEntityType BaseType { get; [param: CanBeNull] set; }

        /// <summary>
        ///     Gets or sets the LINQ expression filter automatically applied to queries for this entity type.
        /// </summary>
        new LambdaExpression QueryFilter { get; [param: CanBeNull] set; }

        /// <summary>
        ///     Gets or sets whether this entity type is a query type.
        /// </summary>
        /// <returns> true if the entity type is a query type; otherwise false. </returns>
        new bool IsQueryType { get; set; }

        /// <summary>
        ///     Gets the LINQ query used as the default source for queries of this type.
        /// </summary>
        new LambdaExpression DefiningQuery { get; [param: CanBeNull] set; }

        /// <summary>
        ///     Sets the primary key for this entity.
        /// </summary>
        /// <param name="properties"> The properties that make up the primary key. </param>
        /// <returns> The newly created key. </returns>
        IMutableKey SetPrimaryKey([CanBeNull] IReadOnlyList<IMutableProperty> properties);

        /// <summary>
        ///     <para>
        ///         Gets primary key for this entity. Returns null if no primary key is defined.
        ///     </para>
        ///     <para>
        ///         To be a valid model, each entity type must have a primary key defined. Therefore, the primary key may be
        ///         null while the model is being created, but will be present by the time the model is used with a <see cref="DbContext" />.
        ///     </para>
        /// </summary>
        /// <returns> The primary key, or null if none is defined. </returns>
        new IMutableKey FindPrimaryKey();

        /// <summary>
        ///     Adds a new alternate key to this entity type.
        /// </summary>
        /// <param name="properties"> The properties that make up the alternate key. </param>
        /// <returns> The newly created key. </returns>
        IMutableKey AddKey([NotNull] IReadOnlyList<IMutableProperty> properties);

        /// <summary>
        ///     Gets the primary or alternate key that is defined on the given properties. Returns null if no key is defined
        ///     for the given properties.
        /// </summary>
        /// <param name="properties"> The properties that make up the key. </param>
        /// <returns> The key, or null if none is defined. </returns>
        new IMutableKey FindKey([NotNull] IReadOnlyList<IProperty> properties);

        /// <summary>
        ///     Gets the primary and alternate keys for this entity.
        /// </summary>
        /// <returns> The primary and alternate keys. </returns>
        new IEnumerable<IMutableKey> GetKeys();

        /// <summary>
        ///     Removes a primary or alternate key from this entity.
        /// </summary>
        /// <param name="properties"> The properties that make up the key. </param>
        /// <returns> The key that was removed. </returns>
        IMutableKey RemoveKey([NotNull] IReadOnlyList<IProperty> properties);

        /// <summary>
        ///     Adds a new relationship to this entity.
        /// </summary>
        /// <param name="properties"> The properties that the foreign key is defined on. </param>
        /// <param name="principalKey"> The primary or alternate key that is referenced. </param>
        /// <param name="principalEntityType">
        ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
        ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
        ///     base type of the hierarchy).
        /// </param>
        /// <returns> The newly created foreign key. </returns>
        IMutableForeignKey AddForeignKey(
            [NotNull] IReadOnlyList<IMutableProperty> properties,
            [NotNull] IMutableKey principalKey,
            [NotNull] IMutableEntityType principalEntityType);

        /// <summary>
        ///     Gets the foreign key for the given properties that points to a given primary or alternate key. Returns null
        ///     if no foreign key is found.
        /// </summary>
        /// <param name="properties"> The properties that the foreign key is defined on. </param>
        /// <param name="principalKey"> The primary or alternate key that is referenced. </param>
        /// <param name="principalEntityType">
        ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
        ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
        ///     base type of the hierarchy).
        /// </param>
        /// <returns> The foreign key, or null if none is defined. </returns>
        new IMutableForeignKey FindForeignKey(
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType);

        /// <summary>
        ///     Gets the foreign keys defined on this entity.
        /// </summary>
        /// <returns> The foreign keys defined on this entity. </returns>
        new IEnumerable<IMutableForeignKey> GetForeignKeys();

        /// <summary>
        ///     Removes a relationship from this entity.
        /// </summary>
        /// <param name="properties"> The properties that the foreign key is defined on. </param>
        /// <param name="principalKey"> The primary or alternate key that is referenced. </param>
        /// <param name="principalEntityType">
        ///     The entity type that the relationship targets. This may be different from the type that <paramref name="principalKey" />
        ///     is defined on when the relationship targets a derived type in an inheritance hierarchy (since the key is defined on the
        ///     base type of the hierarchy).
        /// </param>
        /// <returns> The foreign key that was removed. </returns>
        IMutableForeignKey RemoveForeignKey(
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType);

        /// <summary>
        ///     Adds an index to this entity.
        /// </summary>
        /// <param name="properties"> The properties that are to be indexed. </param>
        /// <returns> The newly created index. </returns>
        IMutableIndex AddIndex([NotNull] IReadOnlyList<IMutableProperty> properties);

        /// <summary>
        ///     Gets the index defined on the given properties. Returns null if no index is defined.
        /// </summary>
        /// <param name="properties"> The properties to find the index on. </param>
        /// <returns> The index, or null if none is found. </returns>
        new IMutableIndex FindIndex([NotNull] IReadOnlyList<IProperty> properties);

        /// <summary>
        ///     Gets the indexes defined on this entity.
        /// </summary>
        /// <returns> The indexes defined on this entity. </returns>
        new IEnumerable<IMutableIndex> GetIndexes();

        /// <summary>
        ///     Removes an index from this entity.
        /// </summary>
        /// <param name="properties"> The properties that make up the index. </param>
        /// <returns> The index that was removed. </returns>
        IMutableIndex RemoveIndex([NotNull] IReadOnlyList<IProperty> properties);

        /// <summary>
        ///     Adds a property to this entity.
        /// </summary>
        /// <param name="name"> The name of the property to add. </param>
        /// <param name="propertyType"> The type of value the property will hold. </param>
        /// <returns> The newly created property. </returns>
        IMutableProperty AddProperty([NotNull] string name, [CanBeNull] Type propertyType);

        /// <summary>
        ///     <para>
        ///         Gets the property with a given name. Returns null if no property with the given name is defined.
        ///     </para>
        ///     <para>
        ///         This API only finds scalar properties and does not find navigation properties. Use
        ///         <see cref="MutableEntityTypeExtensions.FindNavigation(IMutableEntityType, string)" /> to find
        ///         a navigation property.
        ///     </para>
        /// </summary>
        /// <param name="name"> The name of the property. </param>
        /// <returns> The property, or null if none is found. </returns>
        new IMutableProperty FindProperty([NotNull] string name);

        /// <summary>
        ///     <para>
        ///         Gets the properties defined on this entity.
        ///     </para>
        ///     <para>
        ///         This API only returns scalar properties and does not return navigation properties. Use
        ///         <see cref="MutableEntityTypeExtensions.GetNavigations(IMutableEntityType)" /> to get navigation
        ///         properties.
        ///     </para>
        /// </summary>
        /// <returns> The properties defined on this entity. </returns>
        new IEnumerable<IMutableProperty> GetProperties();

        /// <summary>
        ///     Removes a property from this entity.
        /// </summary>
        /// <param name="name"> The name of the property to remove. </param>
        /// <returns> The property that was removed. </returns>
        IMutableProperty RemoveProperty([NotNull] string name);

        /// <summary>
        ///     Adds a <see cref="IMutableServiceProperty" /> to this entity.
        /// </summary>
        /// <param name="memberInfo"> The <see cref="PropertyInfo" /> or <see cref="FieldInfo" /> of the property to add. </param>
        /// <returns> The newly created property. </returns>
        IMutableServiceProperty AddServiceProperty([NotNull] MemberInfo memberInfo);

        /// <summary>
        ///     <para>
        ///         Gets the <see cref="IMutableServiceProperty" /> with a given name. Returns null if no property with the given name is defined.
        ///     </para>
        ///     <para>
        ///         This API only finds service properties and does not find scalar or navigation properties.
        ///     </para>
        /// </summary>
        /// <param name="name"> The name of the property. </param>
        /// <returns> The service property, or null if none is found. </returns>
        new IMutableServiceProperty FindServiceProperty([NotNull] string name);

        /// <summary>
        ///     <para>
        ///         Gets all the <see cref="IMutableServiceProperty" /> defined on this entity.
        ///     </para>
        ///     <para>
        ///         This API only returns service properties and does not return scalar or navigation properties.
        ///     </para>
        /// </summary>
        /// <returns> The service properties defined on this entity. </returns>
        new IEnumerable<IMutableServiceProperty> GetServiceProperties();

        /// <summary>
        ///     Removes an <see cref="IMutableServiceProperty" /> from this entity.
        /// </summary>
        /// <param name="name"> The name of the property to remove. </param>
        /// <returns> The property that was removed. </returns>
        IMutableServiceProperty RemoveServiceProperty([NotNull] string name);
    }
}
