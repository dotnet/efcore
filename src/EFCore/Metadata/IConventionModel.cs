// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Metadata about the shape of entities, the relationships between them, and how they map to
///     the database. A model is typically created by overriding the
///     <see cref="DbContext.OnModelCreating(ModelBuilder)" /> method on a derived
///     <see cref="DbContext" />.
/// </summary>
/// <remarks>
///     <para>
///         This interface is used during model creation and allows the metadata to be modified.
///         Once the model is built, <see cref="IModel" /> represents a read-only view of the same metadata.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
///     </para>
/// </remarks>
[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2072", Justification = "TODO")]
public interface IConventionModel : IReadOnlyModel, IConventionAnnotatable
{
    /// <summary>
    ///     Gets the builder that can be used to configure this model.
    /// </summary>
    new IConventionModelBuilder Builder { get; }

    /// <summary>
    ///     Prevents conventions from being executed immediately when a metadata aspect is modified. All the delayed conventions
    ///     will be executed after the returned object is disposed.
    /// </summary>
    /// <remarks>
    ///     This is useful when performing multiple operations that depend on each other.
    /// </remarks>
    /// <returns>An object that should be disposed to execute the delayed conventions.</returns>
    IConventionBatch DelayConventions();

    /// <summary>
    ///     Sets the <see cref="PropertyAccessMode" /> to use for properties of all entity types
    ///     in this model.
    /// </summary>
    /// <remarks>
    ///     Note that individual entity types can override this access mode, and individual properties of
    ///     entity types can override the access mode set on the entity type. The value set here will
    ///     be used for any property for which no override has been specified.
    /// </remarks>
    /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" />, or <see langword="null" /> to clear the mode set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    PropertyAccessMode? SetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyModel.GetPropertyAccessMode" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyModel.GetPropertyAccessMode" />.</returns>
    ConfigurationSource? GetPropertyAccessModeConfigurationSource();

    /// <summary>
    ///     Sets the default change tracking strategy to use for entities in the model. This strategy indicates how the
    ///     context detects changes to properties for an instance of an entity type.
    /// </summary>
    /// <param name="changeTrackingStrategy">The strategy to use.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    ChangeTrackingStrategy? SetChangeTrackingStrategy(ChangeTrackingStrategy? changeTrackingStrategy, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyModel.GetChangeTrackingStrategy" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyModel.GetChangeTrackingStrategy" />.</returns>
    ConfigurationSource? GetChangeTrackingStrategyConfigurationSource();

    /// <summary>
    ///     Adds a state entity type of default type to the model.
    /// </summary>
    /// <remarks>
    ///     Shadow entities are not currently supported in a model that is used at runtime with a <see cref="DbContext" />.
    ///     Therefore, shadow state entity types will only exist in migration model snapshots, etc.
    /// </remarks>
    /// <param name="name">The name of the entity to be added.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The new entity type.</returns>
    IConventionEntityType? AddEntityType(string name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Adds an entity type to the model.
    /// </summary>
    /// <param name="type">The CLR class that is used to represent instances of the entity type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The new entity type.</returns>
    IConventionEntityType? AddEntityType(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Adds a shared type entity type to the model.
    /// </summary>
    /// <remarks>
    ///     Shared type entity type is an entity type which can share CLR type with other types in the model but has
    ///     a unique name and always identified by the name.
    /// </remarks>
    /// <param name="name">The name of the entity to be added.</param>
    /// <param name="clrType">The CLR class that is used to represent instances of the entity type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The new entity type.</returns>
    IConventionEntityType? AddEntityType(
        string name,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type clrType,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Adds an owned entity type with a defining navigation to the model.
    /// </summary>
    /// <param name="name">The name of the entity type to be added.</param>
    /// <param name="definingNavigationName">The defining navigation.</param>
    /// <param name="definingEntityType">The defining entity type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The new entity type.</returns>
    IConventionEntityType? AddEntityType(
        string name,
        string definingNavigationName,
        IConventionEntityType definingEntityType,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Adds an owned entity type with a defining navigation to the model.
    /// </summary>
    /// <param name="type">The CLR class that is used to represent instances of this entity type.</param>
    /// <param name="definingNavigationName">The defining navigation.</param>
    /// <param name="definingEntityType">The defining entity type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The new entity type.</returns>
    IConventionEntityType? AddEntityType(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        string definingNavigationName,
        IConventionEntityType definingEntityType,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Adds an owned entity type of default type to the model.
    /// </summary>
    /// <remarks>
    ///     Shadow entities are not currently supported in a model that is used at runtime with a <see cref="DbContext" />.
    ///     Therefore, shadow state entity types will only exist in migration model snapshots, etc.
    /// </remarks>
    /// <param name="name">The name of the entity to be added.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The new entity type.</returns>
    IConventionEntityType? AddOwnedEntityType(string name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Adds an owned entity type to the model.
    /// </summary>
    /// <param name="type">The CLR class that is used to represent instances of the entity type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The new entity type.</returns>
    IConventionEntityType? AddOwnedEntityType(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Adds an owned shared type entity type to the model.
    /// </summary>
    /// <remarks>
    ///     Shared type entity type is an entity type which can share CLR type with other types in the model but has
    ///     a unique name and always identified by the name.
    /// </remarks>
    /// <param name="name">The name of the entity to be added.</param>
    /// <param name="clrType">The CLR class that is used to represent instances of the entity type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The new entity type.</returns>
    IConventionEntityType? AddOwnedEntityType(
        string name,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type clrType,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the entity with the given name. Returns <see langword="null" /> if no entity type with the given name is found
    ///     or the given CLR type is being used by shared type entity type
    ///     or the entity type has a defining navigation.
    /// </summary>
    /// <param name="name">The name of the entity type to find.</param>
    /// <returns>The entity type, or <see langword="null" /> if none is found.</returns>
    new IConventionEntityType? FindEntityType(string name);

    /// <summary>
    ///     Gets the entity type for the given name, defining navigation name
    ///     and the defining entity type. Returns <see langword="null" /> if no matching entity type is found.
    /// </summary>
    /// <param name="name">The name of the entity type to find.</param>
    /// <param name="definingNavigationName">The defining navigation of the entity type to find.</param>
    /// <param name="definingEntityType">The defining entity type of the entity type to find.</param>
    /// <returns>The entity type, or <see langword="null" /> if none is found.</returns>
    IConventionEntityType? FindEntityType(
        string name,
        string definingNavigationName,
        IConventionEntityType definingEntityType);

    /// <summary>
    ///     Gets the entity that maps the given entity class. Returns <see langword="null" /> if no entity type with the given name is found.
    /// </summary>
    /// <param name="type">The type to find the corresponding entity type for.</param>
    /// <returns>The entity type, or <see langword="null" /> if none is found.</returns>
    new IConventionEntityType? FindEntityType(Type type)
        => (IConventionEntityType?)((IReadOnlyModel)this).FindEntityType(type);

    /// <summary>
    ///     Gets the entity type for the given name, defining navigation name
    ///     and the defining entity type. Returns <see langword="null" /> if no matching entity type is found.
    /// </summary>
    /// <param name="type">The type of the entity type to find.</param>
    /// <param name="definingNavigationName">The defining navigation of the entity type to find.</param>
    /// <param name="definingEntityType">The defining entity type of the entity type to find.</param>
    /// <returns>The entity type, or <see langword="null" /> if none is found.</returns>
    IConventionEntityType? FindEntityType(
        Type type,
        string definingNavigationName,
        IConventionEntityType definingEntityType)
        => (IConventionEntityType?)((IReadOnlyModel)this).FindEntityType(type, definingNavigationName, definingEntityType);

    /// <summary>
    ///     Removes an entity type from the model.
    /// </summary>
    /// <param name="entityType">The entity type to be removed.</param>
    /// <returns>The removed entity type, or <see langword="null" /> if the entity type was not found.</returns>
    IConventionEntityType? RemoveEntityType(IConventionEntityType entityType);

    /// <summary>
    ///     Removes an entity type without a defining navigation from the model.
    /// </summary>
    /// <param name="name">The name of the entity type to be removed.</param>
    /// <returns>The entity type that was removed.</returns>
    IConventionEntityType? RemoveEntityType(string name);

    /// <summary>
    ///     Removes an entity type with the given type, defining navigation name
    ///     and the defining entity type.
    /// </summary>
    /// <param name="name">The name of the entity type to be removed.</param>
    /// <param name="definingNavigationName">The defining navigation.</param>
    /// <param name="definingEntityType">The defining entity type.</param>
    /// <returns>The entity type that was removed.</returns>
    IConventionEntityType? RemoveEntityType(
        string name,
        string definingNavigationName,
        IConventionEntityType definingEntityType);

    /// <summary>
    ///     Removes an entity type from the model.
    /// </summary>
    /// <param name="type">The entity type to be removed.</param>
    /// <returns>The entity type that was removed.</returns>
    IConventionEntityType? RemoveEntityType(Type type);

    /// <summary>
    ///     Removes an entity type with the given type, defining navigation name
    ///     and the defining entity type.
    /// </summary>
    /// <param name="type">The CLR class that is used to represent instances of this entity type.</param>
    /// <param name="definingNavigationName">The defining navigation.</param>
    /// <param name="definingEntityType">The defining entity type.</param>
    /// <returns>The entity type that was removed.</returns>
    IConventionEntityType? RemoveEntityType(
        Type type,
        string definingNavigationName,
        IConventionEntityType definingEntityType);

    /// <summary>
    ///     Gets all entity types defined in the model.
    /// </summary>
    /// <returns>All entity types defined in the model.</returns>
    new IEnumerable<IConventionEntityType> GetEntityTypes();

    /// <summary>
    ///     Gets the entity types matching the given type.
    /// </summary>
    /// <param name="type">The type of the entity type to find.</param>
    /// <returns>The entity types found.</returns>
    new IEnumerable<IConventionEntityType> FindEntityTypes(Type type)
        => ((IReadOnlyModel)this).FindEntityTypes(type).Cast<IConventionEntityType>();

    /// <summary>
    ///     Returns the entity types corresponding to the least derived types from the given one.
    /// </summary>
    /// <param name="type">The base type.</param>
    /// <param name="condition">An optional condition for filtering entity types.</param>
    /// <returns>List of entity types corresponding to the least derived types from the given one.</returns>
    new IEnumerable<IConventionEntityType> FindLeastDerivedEntityTypes(
        Type type,
        Func<IReadOnlyEntityType, bool>? condition = null)
        => ((IReadOnlyModel)this).FindLeastDerivedEntityTypes(type, condition)
            .Cast<IConventionEntityType>();

    /// <summary>
    ///     Marks the given entity type as shared, indicating that when discovered matching entity types
    ///     should be configured as shared type entity type.
    /// </summary>
    /// <param name="type">The type of the entity type that should be shared.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    void AddShared(Type type, bool fromDataAnnotation = false);

    /// <summary>
    ///     Marks the given type as not shared, indicating that when discovered matching entity types
    ///     should not be configured as shared type entity types.
    /// </summary>
    /// <param name="type">The type of the entity type that should be shared.</param>
    /// <returns>The removed type.</returns>
    Type? RemoveShared(Type type);

    /// <summary>
    ///     Returns the configuration source if the given type is marked as shared.
    /// </summary>
    /// <param name="type">The type that could be shared.</param>
    /// <returns>
    ///     The configuration source if the given type is marked as shared,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    ConfigurationSource? FindIsSharedConfigurationSource(Type type);

    /// <summary>
    ///     Marks the given entity type as owned, indicating that when discovered entity types using the given type
    ///     should be configured as owned.
    /// </summary>
    /// <param name="type">The type of the entity type that should be owned.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    void AddOwned(Type type, bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes the given owned type, indicating that when discovered matching entity types
    ///     should not be configured as owned.
    /// </summary>
    /// <param name="type">The type of the entity type that should not be owned.</param>
    /// <returns>The name of the removed owned type.</returns>
    string? RemoveOwned(Type type);

    /// <summary>
    ///     Returns a value indicating whether the entity types using the given type should be configured
    ///     as owned types when discovered.
    /// </summary>
    /// <param name="type">The type of the entity type that could be owned.</param>
    /// <returns>
    ///     <see langword="true" /> if the given type is marked as owned,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    bool IsOwned([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
        => FindIsOwnedConfigurationSource(type) != null;

    /// <summary>
    ///     Returns the configuration source if the given type is marked as owned.
    /// </summary>
    /// <param name="type">The type of the entity type that could be owned.</param>
    /// <returns>
    ///     The configuration source if the given type is marked as owned,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    ConfigurationSource? FindIsOwnedConfigurationSource(Type type);

    /// <summary>
    ///     Marks the given entity type name as ignored.
    /// </summary>
    /// <param name="typeName">The name of the entity type to be ignored.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The name of the ignored entity type.</returns>
    string? AddIgnored(string typeName, bool fromDataAnnotation = false);

    /// <summary>
    ///     Marks the given entity type as ignored.
    /// </summary>
    /// <param name="type">The entity type to be ignored.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The name of the ignored entity type.</returns>
    string? AddIgnored(Type type, bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes the ignored entity type name.
    /// </summary>
    /// <param name="typeName">The name of the ignored entity type to be removed.</param>
    /// <returns>The removed ignored type name.</returns>
    string? RemoveIgnored(string typeName);

    /// <summary>
    ///     Indicates whether the given entity type name is ignored.
    /// </summary>
    /// <param name="typeName">The name of the entity type that could be ignored.</param>
    /// <returns><see langword="true" /> if the given entity type name is ignored.</returns>
    bool IsIgnored(string typeName);

    /// <summary>
    ///     Indicates whether the given entity type is ignored.
    /// </summary>
    /// <param name="type">The entity type that might be ignored.</param>
    /// <returns><see langword="true" /> if the given entity type is ignored.</returns>
    bool IsIgnored(Type type);

    /// <summary>
    ///     Indicates whether entity types and properties with the given type should be ignored.
    ///     This configuration is independent from <see cref="IsIgnored(Type)" />
    /// </summary>
    /// <param name="type">The entity type that might be ignored.</param>
    /// <returns><see langword="true" /> if the given entity type is ignored.</returns>
    bool IsIgnoredType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type);

    /// <summary>
    ///     Indicates whether the given entity type name is ignored.
    /// </summary>
    /// <param name="typeName">The name of the entity type that could be ignored.</param>
    /// <returns>
    ///     The configuration source if the given entity type name is ignored,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    ConfigurationSource? FindIgnoredConfigurationSource(string typeName);

    /// <summary>
    ///     Indicates whether the given entity type is ignored.
    /// </summary>
    /// <param name="type">The entity type that might be ignored.</param>
    /// <returns>
    ///     The configuration source if the given entity type is ignored,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    ConfigurationSource? FindIgnoredConfigurationSource(Type type);

    /// <summary>
    ///     Forces post-processing on the model such that it is ready for use by the runtime. This post-
    ///     processing happens automatically when using <see cref="DbContext.OnModelCreating" />; this method allows it to be run
    ///     explicitly in cases where the automatic execution is not possible.
    /// </summary>
    /// <returns>The finalized model.</returns>
    IModel FinalizeModel();
}
