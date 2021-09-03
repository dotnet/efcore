// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Metadata about the shape of entities, the relationships between them, and how they map to
    ///         the database. A model is typically created by overriding the
    ///         <see cref="DbContext.OnModelCreating(ModelBuilder)" /> method on a derived
    ///         <see cref="DbContext" />.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="IModel" /> represents a read-only view of the same metadata.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information.
    /// </remarks>
    public interface IMutableModel : IReadOnlyModel, IMutableAnnotatable
    {
        /// <summary>
        ///     <para>
        ///         Prevents conventions from being executed immediately when a metadata aspect is modified. All the delayed conventions
        ///         will be executed after the returned object is disposed.
        ///     </para>
        ///     <para>
        ///         This is useful when performing multiple operations that depend on each other.
        ///     </para>
        /// </summary>
        /// <returns> An object that should be disposed to execute the delayed conventions. </returns>
        IConventionBatch DelayConventions();

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="PropertyAccessMode" /> to use for properties of all entity types
        ///         in this model.
        ///     </para>
        ///     <para>
        ///         Note that individual entity types can override this access mode, and individual properties of
        ///         entity types can override the access mode set on the entity type. The value set here will
        ///         be used for any property for which no override has been specified.
        ///     </para>
        /// </summary>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" />, or <see langword="null" /> to clear the mode set.</param>
        void SetPropertyAccessMode(PropertyAccessMode? propertyAccessMode);

        /// <summary>
        ///     Sets the default change tracking strategy to use for entities in the model. This strategy indicates how the
        ///     context detects changes to properties for an instance of an entity type.
        /// </summary>
        /// <param name="changeTrackingStrategy"> The strategy to use. </param>
        void SetChangeTrackingStrategy(ChangeTrackingStrategy? changeTrackingStrategy);

        /// <summary>
        ///     <para>
        ///         Adds an entity type of default type to the model.
        ///     </para>
        ///     <para>
        ///         Shadow entities are not currently supported in a model that is used at runtime with a <see cref="DbContext" />.
        ///         Therefore, shadow state entity types will only exist in migration model snapshots, etc.
        ///     </para>
        /// </summary>
        /// <param name="name"> The name of the entity to be added. </param>
        /// <returns> The new entity type. </returns>
        IMutableEntityType AddEntityType(string name);

        /// <summary>
        ///     Adds an entity type to the model.
        /// </summary>
        /// <param name="type"> The CLR class that is used to represent instances of the entity type. </param>
        /// <returns> The new entity type. </returns>
        IMutableEntityType AddEntityType(Type type);

        /// <summary>
        ///     <para>
        ///         Adds a shared type entity type to the model.
        ///     </para>
        ///     <para>
        ///         Shared type entity type is an entity type which can share CLR type with other types in the model but has
        ///         a unique name and always identified by the name.
        ///     </para>
        /// </summary>
        /// <param name="name"> The name of the entity to be added. </param>
        /// <param name="type"> The CLR class that is used to represent instances of the entity type. </param>
        /// <returns> The new entity type. </returns>
        IMutableEntityType AddEntityType(string name, Type type);

        /// <summary>
        ///     Adds an owned entity type with a defining navigation to the model.
        /// </summary>
        /// <param name="name"> The name of the entity type to be added. </param>
        /// <param name="definingNavigationName"> The defining navigation. </param>
        /// <param name="definingEntityType"> The defining entity type. </param>
        /// <returns> The new entity type. </returns>
        IMutableEntityType AddEntityType(
            string name,
            string definingNavigationName,
            IMutableEntityType definingEntityType);

        /// <summary>
        ///     Adds an owned entity type with a defining navigation to the model.
        /// </summary>
        /// <param name="type"> The CLR class that is used to represent instances of this entity type. </param>
        /// <param name="definingNavigationName"> The defining navigation. </param>
        /// <param name="definingEntityType"> The defining entity type. </param>
        /// <returns> The new entity type. </returns>
        IMutableEntityType AddEntityType(
            Type type,
            string definingNavigationName,
            IMutableEntityType definingEntityType);

        /// <summary>
        ///     <para>
        ///         Adds an owned entity type of default type to the model.
        ///     </para>
        ///     <para>
        ///         Shadow entities are not currently supported in a model that is used at runtime with a <see cref="DbContext" />.
        ///         Therefore, shadow state entity types will only exist in migration model snapshots, etc.
        ///     </para>
        /// </summary>
        /// <param name="name"> The name of the entity to be added. </param>
        /// <returns> The new entity type. </returns>
        IMutableEntityType AddOwnedEntityType(string name);

        /// <summary>
        ///     Adds an owned entity type to the model.
        /// </summary>
        /// <param name="type"> The CLR class that is used to represent instances of the entity type. </param>
        /// <returns> The new entity type. </returns>
        IMutableEntityType AddOwnedEntityType(Type type);

        /// <summary>
        ///     <para>
        ///         Adds an owned shared type entity type to the model.
        ///     </para>
        ///     <para>
        ///         Shared type entity type is an entity type which can share CLR type with other types in the model but has
        ///         a unique name and always identified by the name.
        ///     </para>
        /// </summary>
        /// <param name="name"> The name of the entity to be added. </param>
        /// <param name="type"> The CLR class that is used to represent instances of the entity type. </param>
        /// <returns> The new entity type. </returns>
        IMutableEntityType AddOwnedEntityType(string name, Type type);

        /// <summary>
        ///     Gets the entity with the given name. Returns <see langword="null" /> if no entity type with the given name is found
        ///     or the given CLR type is being used by shared type entity type
        ///     or the entity type has a defining navigation.
        /// </summary>
        /// <param name="name"> The name of the entity type to find. </param>
        /// <returns> The entity type, or <see langword="null" /> if none is found. </returns>
        new IMutableEntityType? FindEntityType(string name);

        /// <summary>
        ///     Gets the entity type for the given name, defining navigation name
        ///     and the defining entity type. Returns <see langword="null" /> if no matching entity type is found.
        /// </summary>
        /// <param name="name"> The name of the entity type to find. </param>
        /// <param name="definingNavigationName"> The defining navigation of the entity type to find. </param>
        /// <param name="definingEntityType"> The defining entity type of the entity type to find. </param>
        /// <returns> The entity type, or <see langword="null" /> if none is found. </returns>
        IMutableEntityType? FindEntityType(
            string name,
            string definingNavigationName,
            IMutableEntityType definingEntityType);

        /// <summary>
        ///     Gets the entity that maps the given entity class. Returns <see langword="null" /> if no entity type with
        ///     the given CLR type is found or the given CLR type is being used by shared type entity type
        ///     or the entity type has a defining navigation.
        /// </summary>
        /// <param name="type"> The type to find the corresponding entity type for. </param>
        /// <returns> The entity type, or <see langword="null" /> if none is found. </returns>
        new IMutableEntityType? FindEntityType(Type type)
            => (IMutableEntityType?)((IReadOnlyModel)this).FindEntityType(type);

        /// <summary>
        ///     Gets the entity type for the given name, defining navigation name
        ///     and the defining entity type. Returns <see langword="null" /> if no matching entity type is found.
        /// </summary>
        /// <param name="type"> The type of the entity type to find. </param>
        /// <param name="definingNavigationName"> The defining navigation of the entity type to find. </param>
        /// <param name="definingEntityType"> The defining entity type of the entity type to find. </param>
        /// <returns> The entity type, or <see langword="null" /> if none is found. </returns>
        IMutableEntityType? FindEntityType(
            Type type,
            string definingNavigationName,
            IMutableEntityType definingEntityType)
            => (IMutableEntityType?)((IReadOnlyModel)this).FindEntityType(type, definingNavigationName, definingEntityType);

        /// <summary>
        ///     Removes an entity type from the model.
        /// </summary>
        /// <param name="entityType"> The entity type to be removed. </param>
        /// <returns> The removed entity type, or <see langword="null" /> if the entity type was not found. </returns>
        IMutableEntityType? RemoveEntityType(IMutableEntityType entityType);

        /// <summary>
        ///     Removes an entity type from the model.
        /// </summary>
        /// <param name="type"> The entity type to be removed. </param>
        /// <returns> The entity type that was removed. </returns>
        IMutableEntityType? RemoveEntityType(Type type);

        /// <summary>
        ///     Removes an entity type with the given type, defining navigation name
        ///     and the defining entity type
        /// </summary>
        /// <param name="type"> The CLR class that is used to represent instances of this entity type. </param>
        /// <param name="definingNavigationName"> The defining navigation. </param>
        /// <param name="definingEntityType"> The defining entity type. </param>
        /// <returns> The entity type that was removed. </returns>
        IMutableEntityType? RemoveEntityType(
            Type type,
            string definingNavigationName,
            IMutableEntityType definingEntityType);

        /// <summary>
        ///     Removes an entity type without a defining navigation from the model.
        /// </summary>
        /// <param name="name"> The name of the entity type to be removed. </param>
        /// <returns> The entity type that was removed. </returns>
        IMutableEntityType? RemoveEntityType(string name);

        /// <summary>
        ///     Removes an entity type with the given type, defining navigation name
        ///     and the defining entity type
        /// </summary>
        /// <param name="name"> The name of the entity type to be removed. </param>
        /// <param name="definingNavigationName"> The defining navigation. </param>
        /// <param name="definingEntityType"> The defining entity type. </param>
        /// <returns> The entity type that was removed. </returns>
        IMutableEntityType? RemoveEntityType(
            string name,
            string definingNavigationName,
            IMutableEntityType definingEntityType);

        /// <summary>
        ///     Gets all entity types defined in the model.
        /// </summary>
        /// <returns> All entity types defined in the model. </returns>
        new IEnumerable<IMutableEntityType> GetEntityTypes();

        /// <summary>
        ///     Gets the entity types matching the given type.
        /// </summary>
        /// <param name="type"> The type of the entity type to find. </param>
        /// <returns> The entity types found. </returns>
        new IEnumerable<IMutableEntityType> FindEntityTypes(Type type)
            => ((IReadOnlyModel)this).FindEntityTypes(type).Cast<IMutableEntityType>();

        /// <summary>
        ///     Returns the entity types corresponding to the least derived types from the given one.
        /// </summary>
        /// <param name="type"> The base type. </param>
        /// <param name="condition"> An optional condition for filtering entity types. </param>
        /// <returns> List of entity types corresponding to the least derived types from the given one. </returns>
        new IEnumerable<IMutableEntityType> FindLeastDerivedEntityTypes(
            Type type,
            Func<IReadOnlyEntityType, bool>? condition = null)
            => ((IReadOnlyModel)this).FindLeastDerivedEntityTypes(type, condition)
                .Cast<IMutableEntityType>();

        /// <summary>
        ///     Marks the given entity type as shared, indicating that when discovered matching entity types
        ///     should be configured as shared type entity type.
        /// </summary>
        /// <param name="type"> The type of the entity type that should be shared. </param>
        void AddShared(Type type);

        /// <summary>
        ///     Marks the given type as not shared, indicating that when discovered matching entity types
        ///     should not be configured as shared type entity types.
        /// </summary>
        /// <param name="type"> The type of the entity type that should be shared. </param>
        /// <returns> The removed type. </returns>
        Type? RemoveShared(Type type);

        /// <summary>
        ///     Marks the given entity type as owned, indicating that when discovered matching entity types
        ///     should be configured as owned.
        /// </summary>
        /// <param name="type"> The type of the entity type that should be owned. </param>
        void AddOwned(Type type);

        /// <summary>
        ///     Removes the given owned type, indicating that when discovered matching entity types
        ///     should not be configured as owned.
        /// </summary>
        /// <param name="type"> The type of the entity type that should not be owned. </param>
        /// <returns> The name of the removed owned type. </returns>
        string? RemoveOwned(Type type);

        /// <summary>
        ///     Returns a value indicating whether the entity types using the given type should be configured
        ///     as owned types when discovered by conventions.
        /// </summary>
        /// <param name="type"> The type of the entity type that might be owned. </param>
        /// <returns>
        ///     <see langword="true" /> if a matching entity type should be configured as owned when discovered,
        ///     <see langword="false" /> otherwise.
        /// </returns>
        bool IsOwned(Type type);

        /// <summary>
        ///     Marks the given entity type name as ignored, preventing conventions from adding a matching entity type to the model.
        /// </summary>
        /// <param name="typeName"> The name of the entity type to be ignored. </param>
        /// <returns> The name of the ignored type. </returns>
        string AddIgnored(string typeName);

        /// <summary>
        ///     Marks the given entity type as ignored, preventing conventions from adding a matching entity type to the model.
        /// </summary>
        /// <param name="type"> The entity type to be ignored. </param>
        /// <returns> The name of the ignored type. </returns>
        string AddIgnored(Type type);

        /// <summary>
        ///     Removes the ignored entity type name.
        /// </summary>
        /// <param name="typeName"> The name of the ignored entity type to be removed. </param>
        /// <returns> The removed ignored type name. </returns>
        string? RemoveIgnored(string typeName);

        /// <summary>
        ///     Removes the ignored entity type.
        /// </summary>
        /// <param name="type"> The ignored entity type to be removed. </param>
        /// <returns> The name of the removed ignored type. </returns>
        string? RemoveIgnored(Type type);

        /// <summary>
        ///     Indicates whether the given entity type name is ignored.
        /// </summary>
        /// <param name="typeName"> The name of the entity type that might be ignored. </param>
        /// <returns> <see langword="true" /> if the given entity type name is ignored. </returns>
        bool IsIgnored(string typeName);

        /// <summary>
        ///     Indicates whether the given entity type name is ignored.
        /// </summary>
        /// <param name="type"> The entity type that might be ignored. </param>
        /// <returns> <see langword="true" /> if the given entity type name is ignored. </returns>
        bool IsIgnored(Type type);

        /// <summary>
        ///     Forces post-processing on the model such that it is ready for use by the runtime. This post-
        ///     processing happens automatically when using <see cref="DbContext.OnModelCreating" />; this method allows it to be run
        ///     explicitly in cases where the automatic execution is not possible.
        /// </summary>
        /// <returns> The finalized model. </returns>
        IModel FinalizeModel();
    }
}
