// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a relationship where configuration began on an end of the
///     relationship with a reference that points to an instance of another entity type.
/// </summary>
/// <remarks>
///     <para>
///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///         and it is not designed to be directly constructed in your application code.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
///         examples.
///     </para>
/// </remarks>
/// <typeparam name="TEntity">The entity type to be configured.</typeparam>
/// <typeparam name="TRelatedEntity">The entity type that this relationship targets.</typeparam>
public class ReferenceNavigationBuilder<TEntity, TRelatedEntity> : ReferenceNavigationBuilder
    where TEntity : class
    where TRelatedEntity : class
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public ReferenceNavigationBuilder(
        IMutableEntityType declaringEntityType,
        IMutableEntityType relatedEntityType,
        string? navigationName,
        IMutableForeignKey foreignKey)
        : base(declaringEntityType, relatedEntityType, navigationName, foreignKey)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public ReferenceNavigationBuilder(
        IMutableEntityType declaringEntityType,
        IMutableEntityType relatedEntityType,
        MemberInfo? navigationMemberInfo,
        IMutableForeignKey foreignKey)
        : base(declaringEntityType, relatedEntityType, navigationMemberInfo, foreignKey)
    {
    }

    /// <summary>
    ///     Configures this as a one-to-many relationship.
    /// </summary>
    /// <remarks>
    ///     Note that calling this method with no parameters will explicitly configure this side
    ///     of the relationship to use no navigation property, even if such a property exists on the
    ///     entity type. If the navigation property is to be used, then it must be specified.
    /// </remarks>
    /// <param name="navigationName">
    ///     The name of the collection navigation property on the other end of this relationship.
    ///     If null or not specified, there is no navigation property on the other end of the relationship.
    /// </param>
    /// <returns>An object to further configure the relationship.</returns>
    public new virtual ReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany(
        string? navigationName = null)
        => new(
            RelatedEntityType,
            DeclaringEntityType,
            WithManyBuilder(
                Check.NullButNotEmpty(navigationName, nameof(navigationName))).Metadata);

    /// <summary>
    ///     Configures this as a one-to-many relationship.
    /// </summary>
    /// <remarks>
    ///     Note that calling this method with no parameters will explicitly configure this side
    ///     of the relationship to use no navigation property, even if such a property exists on the
    ///     entity type. If the navigation property is to be used, then it must be specified.
    /// </remarks>
    /// <param name="navigationExpression">
    ///     A lambda expression representing the collection navigation property on the other end of this
    ///     relationship (<c>blog => blog.Posts</c>). If no property is specified, the relationship will be
    ///     configured without a navigation property on the other end of the relationship.
    /// </param>
    /// <returns>An object to further configure the relationship.</returns>
    public virtual ReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany(
        Expression<Func<TRelatedEntity, IEnumerable<TEntity>?>>? navigationExpression)
        => new(
            RelatedEntityType,
            DeclaringEntityType,
            WithManyBuilder(navigationExpression?.GetMemberAccess()).Metadata);

    /// <summary>
    ///     Configures this as a one-to-one relationship.
    /// </summary>
    /// <remarks>
    ///     Note that calling this method with no parameters will explicitly configure this side
    ///     of the relationship to use no navigation property, even if such a property exists on the
    ///     entity type. If the navigation property is to be used, then it must be specified.
    /// </remarks>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on the other end of this relationship.
    ///     If null or not specified, there is no navigation property on the other end of the relationship.
    /// </param>
    /// <returns>An object to further configure the relationship.</returns>
    public new virtual ReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(
        string? navigationName = null)
        => new(
            DeclaringEntityType,
            RelatedEntityType,
            WithOneBuilder(
                Check.NullButNotEmpty(navigationName, nameof(navigationName))).Metadata);

    /// <summary>
    ///     Configures this as a one-to-one relationship.
    /// </summary>
    /// <remarks>
    ///     Note that calling this method with no parameters will explicitly configure this side
    ///     of the relationship to use no navigation property, even if such a property exists on the
    ///     entity type. If the navigation property is to be used, then it must be specified.
    /// </remarks>
    /// <param name="navigationExpression">
    ///     A lambda expression representing the reference navigation property on the other end of this
    ///     relationship (<c>blog => blog.BlogInfo</c>). If no property is specified, the relationship will be
    ///     configured without a navigation property on the other end of the relationship.
    /// </param>
    /// <returns>An object to further configure the relationship.</returns>
    public virtual ReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(
        Expression<Func<TRelatedEntity, TEntity?>>? navigationExpression)
        => new(
            DeclaringEntityType,
            RelatedEntityType,
            WithOneBuilder(navigationExpression?.GetMemberAccess()).Metadata);
}
