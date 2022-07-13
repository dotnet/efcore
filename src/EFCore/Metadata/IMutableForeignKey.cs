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
///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
///         examples.
///     </para>
/// </remarks>
public interface IMutableForeignKey : IReadOnlyForeignKey, IMutableAnnotatable
{
    /// <summary>
    ///     Gets the foreign key properties in the dependent entity.
    /// </summary>
    new IReadOnlyList<IMutableProperty> Properties { get; }

    /// <summary>
    ///     Gets the primary or alternate key that the relationship targets.
    /// </summary>
    new IMutableKey PrincipalKey { get; }

    /// <summary>
    ///     Gets the dependent entity type. This may be different from the type that <see cref="Properties" />
    ///     are defined on when the relationship is defined a derived type in an inheritance hierarchy (since the properties
    ///     may be defined on a base type).
    /// </summary>
    new IMutableEntityType DeclaringEntityType { get; }

    /// <summary>
    ///     Gets the principal entity type that this relationship targets. This may be different from the type that
    ///     <see cref="PrincipalKey" /> is defined on when the relationship targets a derived type in an inheritance
    ///     hierarchy (since the key is defined on the base type of the hierarchy).
    /// </summary>
    new IMutableEntityType PrincipalEntityType { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the values assigned to the foreign key properties are unique.
    /// </summary>
    new bool IsUnique { get; set; }

    /// <summary>
    ///     Sets a value indicating whether the principal entity is required.
    ///     If <see langword="true" />, the dependent entity must always be assigned to a valid principal entity.
    /// </summary>
    new bool IsRequired { get; set; }

    /// <summary>
    ///     Sets a value indicating whether the dependent entity is required.
    ///     If <see langword="true" />, the principal entity must always have a valid dependent entity assigned.
    /// </summary>
    new bool IsRequiredDependent { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this relationship defines ownership. If true, the dependent entity must always be
    ///     accessed via the navigation from the principal entity.
    /// </summary>
    new bool IsOwnership { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating how a delete operation is applied to dependent entities in the relationship when the
    ///     principal is deleted or the relationship is severed.
    /// </summary>
    new DeleteBehavior DeleteBehavior { get; set; }

    /// <summary>
    ///     Gets the navigation property on the dependent entity type that points to the principal entity.
    /// </summary>
    new IMutableNavigation? DependentToPrincipal { get; }

    /// <summary>
    ///     Gets the navigation property on the principal entity type that points to the dependent entity.
    /// </summary>
    new IMutableNavigation? PrincipalToDependent { get; }

    /// <summary>
    ///     Sets the foreign key properties and that target principal key.
    /// </summary>
    /// <param name="properties">Foreign key properties in the dependent entity.</param>
    /// <param name="principalKey">The primary or alternate key to target.</param>
    void SetProperties(IReadOnlyList<IMutableProperty> properties, IMutableKey principalKey);

    /// <summary>
    ///     Sets the navigation property on the dependent entity type that points to the principal entity.
    /// </summary>
    /// <param name="name">
    ///     The name of the navigation property on the dependent type. Passing <see langword="null" /> will result in there being
    ///     no navigation property defined.
    /// </param>
    /// <returns>The newly set navigation property.</returns>
    IMutableNavigation? SetDependentToPrincipal(string? name);

    /// <summary>
    ///     Sets the navigation property on the dependent entity type that points to the principal entity.
    /// </summary>
    /// <param name="property">
    ///     The navigation property on the dependent type. Passing <see langword="null" /> will result in there being
    ///     no navigation property defined.
    /// </param>
    /// <returns>The newly set navigation property.</returns>
    IMutableNavigation? SetDependentToPrincipal(MemberInfo? property);

    /// <summary>
    ///     Sets the navigation property on the principal entity type that points to the dependent entity.
    /// </summary>
    /// <param name="name">
    ///     The name of the navigation property on the principal type. Passing <see langword="null" /> will result in there being
    ///     no navigation property defined.
    /// </param>
    /// <returns>The newly set navigation property.</returns>
    IMutableNavigation? SetPrincipalToDependent(string? name);

    /// <summary>
    ///     Sets the navigation property on the principal entity type that points to the dependent entity.
    /// </summary>
    /// <param name="property">
    ///     The name of the navigation property on the principal type. Passing <see langword="null" /> will result in there being
    ///     no navigation property defined.
    /// </param>
    /// <returns>The newly set navigation property.</returns>
    IMutableNavigation? SetPrincipalToDependent(MemberInfo? property);

    /// <summary>
    ///     Gets all skip navigations using this foreign key.
    /// </summary>
    /// <returns>The skip navigations using this foreign key.</returns>
    new IEnumerable<IMutableSkipNavigation> GetReferencingSkipNavigations()
        => ((IReadOnlyForeignKey)this).GetReferencingSkipNavigations().Cast<IMutableSkipNavigation>();

    /// <summary>
    ///     Gets the entity type related to the given one.
    /// </summary>
    /// <param name="entityType">One of the entity types related by the foreign key.</param>
    /// <returns>The entity type related to the given one.</returns>
    new IMutableEntityType GetRelatedEntityType(IReadOnlyEntityType entityType)
        => (IMutableEntityType)((IReadOnlyForeignKey)this).GetRelatedEntityType(entityType);

    /// <summary>
    ///     Returns a navigation associated with this foreign key.
    /// </summary>
    /// <param name="pointsToPrincipal">
    ///     A value indicating whether the navigation is on the dependent type pointing to the principal type.
    /// </param>
    /// <returns>
    ///     A navigation associated with this foreign key or <see langword="null" />.
    /// </returns>
    new IMutableNavigation? GetNavigation(bool pointsToPrincipal)
        => pointsToPrincipal ? DependentToPrincipal : PrincipalToDependent;
}
