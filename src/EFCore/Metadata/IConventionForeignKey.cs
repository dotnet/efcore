// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a relationship where a foreign key property(s) in a dependent entity type
///     reference a corresponding primary or alternate key in a principal entity type.
/// </summary>
/// <remarks>
///     <para>
///         This interface is used during model creation and allows the metadata to be modified.
///         Once the model is built, <see cref="IForeignKey" /> represents a read-only view of the same metadata.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
///     </para>
/// </remarks>
public interface IConventionForeignKey : IReadOnlyForeignKey, IConventionAnnotatable
{
    /// <summary>
    ///     Gets the builder that can be used to configure this foreign key.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the foreign key has been removed from the model.</exception>
    new IConventionForeignKeyBuilder Builder { get; }

    /// <summary>
    ///     Gets the foreign key properties in the dependent entity.
    /// </summary>
    new IReadOnlyList<IConventionProperty> Properties { get; }

    /// <summary>
    ///     Gets the primary or alternate key that the relationship targets.
    /// </summary>
    new IConventionKey PrincipalKey { get; }

    /// <summary>
    ///     Gets the dependent entity type. This may be different from the type that <see cref="Properties" />
    ///     are defined on when the relationship is defined a derived type in an inheritance hierarchy (since the properties
    ///     may be defined on a base type).
    /// </summary>
    new IConventionEntityType DeclaringEntityType { get; }

    /// <summary>
    ///     Gets the principal entity type that this relationship targets. This may be different from the type that
    ///     <see cref="PrincipalKey" /> is defined on when the relationship targets a derived type in an inheritance
    ///     hierarchy (since the key is defined on the base type of the hierarchy).
    /// </summary>
    new IConventionEntityType PrincipalEntityType { get; }

    /// <summary>
    ///     Gets the navigation property on the dependent entity type that points to the principal entity.
    /// </summary>
    new IConventionNavigation? DependentToPrincipal { get; }

    /// <summary>
    ///     Gets the navigation property on the principal entity type that points to the dependent entity.
    /// </summary>
    new IConventionNavigation? PrincipalToDependent { get; }

    /// <summary>
    ///     Returns the configuration source for this property.
    /// </summary>
    /// <returns>The configuration source.</returns>
    ConfigurationSource GetConfigurationSource();

    /// <summary>
    ///     Sets the foreign key properties and that target principal key.
    /// </summary>
    /// <param name="properties">Foreign key properties in the dependent entity.</param>
    /// <param name="principalKey">The primary or alternate key to target.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured foreign key properties.</returns>
    IReadOnlyList<IConventionProperty> SetProperties(
        IReadOnlyList<IConventionProperty> properties,
        IConventionKey principalKey,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyForeignKey.Properties" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyForeignKey.Properties" />.</returns>
    ConfigurationSource? GetPropertiesConfigurationSource();

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyForeignKey.PrincipalKey" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyForeignKey.PrincipalKey" />.</returns>
    ConfigurationSource? GetPrincipalKeyConfigurationSource();

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyForeignKey.PrincipalEntityType" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyForeignKey.PrincipalEntityType" />.</returns>
    ConfigurationSource? GetPrincipalEndConfigurationSource();

    /// <summary>
    ///     Sets a value indicating whether the values assigned to the foreign key properties are unique.
    /// </summary>
    /// <param name="unique">A value indicating whether the values assigned to the foreign key properties are unique.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured uniqueness.</returns>
    bool? SetIsUnique(bool? unique, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyForeignKey.IsUnique" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyForeignKey.IsUnique" />.</returns>
    ConfigurationSource? GetIsUniqueConfigurationSource();

    /// <summary>
    ///     Sets a value indicating whether the principal entity is required.
    ///     If <see langword="true" />, the dependent entity must always be assigned to a valid principal entity.
    /// </summary>
    /// <param name="required">A value indicating whether the principal entity is required.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured requiredness.</returns>
    bool? SetIsRequired(bool? required, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyForeignKey.IsRequired" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyForeignKey.IsRequired" />.</returns>
    ConfigurationSource? GetIsRequiredConfigurationSource();

    /// <summary>
    ///     Sets a value indicating whether the dependent entity is required.
    ///     If <see langword="true" />, the principal entity must always have a valid dependent entity assigned.
    /// </summary>
    /// <param name="required">A value indicating whether the dependent entity is required.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured requiredness.</returns>
    bool? SetIsRequiredDependent(bool? required, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyForeignKey.IsRequiredDependent" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyForeignKey.IsRequiredDependent" />.</returns>
    ConfigurationSource? GetIsRequiredDependentConfigurationSource();

    /// <summary>
    ///     Sets a value indicating whether this relationship defines an ownership.
    ///     If <see langword="true" />, the dependent entity must always be accessed via the navigation from the principal entity.
    /// </summary>
    /// <param name="ownership">A value indicating whether this relationship defines an ownership.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured ownership.</returns>
    bool? SetIsOwnership(bool? ownership, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyForeignKey.IsOwnership" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyForeignKey.IsOwnership" />.</returns>
    ConfigurationSource? GetIsOwnershipConfigurationSource();

    /// <summary>
    ///     Sets a value indicating how a delete operation is applied to dependent entities in the relationship when the
    ///     principal is deleted or the relationship is severed.
    /// </summary>
    /// <param name="deleteBehavior">
    ///     A value indicating how a delete operation is applied to dependent entities in the relationship when the
    ///     principal is deleted or the relationship is severed.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured behavior.</returns>
    DeleteBehavior? SetDeleteBehavior(DeleteBehavior? deleteBehavior, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyForeignKey.DeleteBehavior" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyForeignKey.DeleteBehavior" />.</returns>
    ConfigurationSource? GetDeleteBehaviorConfigurationSource();

    /// <summary>
    ///     Sets the navigation property on the dependent entity type that points to the principal entity.
    /// </summary>
    /// <param name="name">
    ///     The name of the navigation property on the dependent type. Passing <see langword="null" /> will result in there being
    ///     no navigation property defined.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created navigation property.</returns>
    IConventionNavigation? SetDependentToPrincipal(string? name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the navigation property on the dependent entity type that points to the principal entity.
    /// </summary>
    /// <param name="property">
    ///     The navigation property on the dependent type. Passing <see langword="null" /> will result in there being
    ///     no navigation property defined.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created navigation property.</returns>
    IConventionNavigation? SetDependentToPrincipal(MemberInfo? property, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyForeignKey.DependentToPrincipal" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyForeignKey.DependentToPrincipal" />.</returns>
    ConfigurationSource? GetDependentToPrincipalConfigurationSource();

    /// <summary>
    ///     Sets the navigation property on the principal entity type that points to the dependent entity.
    /// </summary>
    /// <param name="name">
    ///     The name of the navigation property on the principal type. Passing <see langword="null" /> will result in there being
    ///     no navigation property defined.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created navigation property.</returns>
    IConventionNavigation? SetPrincipalToDependent(string? name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the navigation property on the principal entity type that points to the dependent entity.
    /// </summary>
    /// <param name="property">
    ///     The name of the navigation property on the principal type. Passing <see langword="null" /> will result in there being
    ///     no navigation property defined.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly created navigation property.</returns>
    IConventionNavigation? SetPrincipalToDependent(MemberInfo? property, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyForeignKey.PrincipalToDependent" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyForeignKey.PrincipalToDependent" />.</returns>
    ConfigurationSource? GetPrincipalToDependentConfigurationSource();

    /// <summary>
    ///     Gets all skip navigations using this foreign key.
    /// </summary>
    /// <returns>The skip navigations using this foreign key.</returns>
    new IEnumerable<IConventionSkipNavigation> GetReferencingSkipNavigations()
        => ((IReadOnlyForeignKey)this).GetReferencingSkipNavigations().Cast<IConventionSkipNavigation>();

    /// <summary>
    ///     Gets the entity type related to the given one.
    /// </summary>
    /// <param name="entityType">One of the entity types related by the foreign key.</param>
    /// <returns>The entity type related to the given one.</returns>
    new IConventionEntityType GetRelatedEntityType(IReadOnlyEntityType entityType)
        => (IConventionEntityType)((IReadOnlyForeignKey)this).GetRelatedEntityType(entityType);

    /// <summary>
    ///     Returns a navigation associated with this foreign key.
    /// </summary>
    /// <param name="pointsToPrincipal">
    ///     A value indicating whether the navigation is on the dependent type pointing to the principal type.
    /// </param>
    /// <returns>
    ///     A navigation associated with this foreign key or <see langword="null" />.
    /// </returns>
    new IConventionNavigation? GetNavigation(bool pointsToPrincipal)
        => pointsToPrincipal ? DependentToPrincipal : PrincipalToDependent;
}
