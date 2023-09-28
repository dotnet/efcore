// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a one-to-many relationship.
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
/// <typeparam name="TPrincipalEntity">The principal entity type in this relationship.</typeparam>
/// <typeparam name="TDependentEntity">The dependent entity type in this relationship.</typeparam>
public class ReferenceCollectionBuilder<TPrincipalEntity, TDependentEntity> : ReferenceCollectionBuilder
    where TPrincipalEntity : class
    where TDependentEntity : class
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public ReferenceCollectionBuilder(
        IMutableEntityType principalEntityType,
        IMutableEntityType dependentEntityType,
        IMutableForeignKey foreignKey)
        : base(principalEntityType, dependentEntityType, foreignKey)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected ReferenceCollectionBuilder(
        InternalForeignKeyBuilder builder,
        ReferenceCollectionBuilder oldBuilder,
        bool foreignKeySet = false,
        bool principalKeySet = false,
        bool requiredSet = false)
        : base(builder, oldBuilder, foreignKeySet, principalKeySet, requiredSet)
    {
    }

    /// <summary>
    ///     Adds or updates an annotation on the relationship. If an annotation with the key specified in
    ///     <paramref name="annotation" /> already exists its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual ReferenceCollectionBuilder<TPrincipalEntity, TDependentEntity> HasAnnotation(
        string annotation,
        object? value)
        => (ReferenceCollectionBuilder<TPrincipalEntity, TDependentEntity>)base.HasAnnotation(
            Check.NotEmpty(annotation, nameof(annotation)),
            Check.NotNull(value, nameof(value)));

    /// <summary>
    ///     Configures the property(s) to use as the foreign key for this relationship.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If the specified property name(s) do not exist on the entity type then a new shadow state
    ///         property(s) will be added to serve as the foreign key. A shadow state property is one
    ///         that does not have a corresponding property in the entity class. The current value for the
    ///         property is stored in the <see cref="ChangeTracker" /> rather than being stored in instances
    ///         of the entity class.
    ///     </para>
    ///     <para>
    ///         If <see cref="HasPrincipalKey(System.Linq.Expressions.Expression{System.Func{TPrincipalEntity,object}})" /> is not specified,
    ///         then an attempt will be made to match the data type and order of foreign key properties against
    ///         the primary key of the principal entity type. If they do not match, new shadow state properties
    ///         that form a unique index will be added to the principal entity type to serve as the reference key.
    ///     </para>
    /// </remarks>
    /// <param name="foreignKeyPropertyNames">
    ///     The name(s) of the foreign key property(s).
    /// </param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual ReferenceCollectionBuilder<TPrincipalEntity, TDependentEntity> HasForeignKey(
        params string[] foreignKeyPropertyNames)
        => new(
            HasForeignKeyBuilder(Check.NotEmpty(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames))),
            this,
            foreignKeySet: true);

    /// <summary>
    ///     Configures the property(s) to use as the foreign key for this relationship.
    /// </summary>
    /// <remarks>
    ///     If <see cref="HasPrincipalKey(Expression{Func{TPrincipalEntity, object}})" /> is not specified, then
    ///     an attempt will be made to match the data type and order of foreign key properties against the
    ///     primary key of the principal entity type. If they do not match, new shadow state properties that
    ///     form a unique index will be added to the principal entity type to serve as the reference key.
    ///     A shadow state property is one that does not have a corresponding property in the entity class. The
    ///     current value for the property is stored in the <see cref="ChangeTracker" /> rather than being
    ///     stored in instances of the entity class.
    /// </remarks>
    /// <param name="foreignKeyExpression">
    ///     <para>
    ///         A lambda expression representing the foreign key property(s) (<c>post => post.BlogId</c>).
    ///     </para>
    ///     <para>
    ///         If the foreign key is made up of multiple properties then specify an anonymous type including the
    ///         properties (<c>comment => new { comment.BlogId, comment.PostTitle }</c>). The order specified should match the order of
    ///         corresponding properties in <see cref="HasPrincipalKey(Expression{Func{TPrincipalEntity,object}})" />.
    ///     </para>
    /// </param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ReferenceCollectionBuilder<TPrincipalEntity, TDependentEntity> HasForeignKey(
        Expression<Func<TDependentEntity, object?>> foreignKeyExpression)
        => new(
            HasForeignKeyBuilder(Check.NotNull(foreignKeyExpression, nameof(foreignKeyExpression)).GetMemberAccessList()),
            this,
            foreignKeySet: true);

    /// <summary>
    ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
    ///     method if you want to use a property(s) other than the primary key as the principal property(s). If
    ///     the specified property(s) is not already a unique constraint (or the primary key) then a new unique
    ///     constraint will be introduced.
    /// </summary>
    /// <param name="keyPropertyNames">The name(s) of the referenced key property(s).</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual ReferenceCollectionBuilder<TPrincipalEntity, TDependentEntity> HasPrincipalKey(
        params string[] keyPropertyNames)
        => new(
            HasPrincipalKeyBuilder(Check.NotEmpty(keyPropertyNames, nameof(keyPropertyNames))),
            this,
            principalKeySet: true);

    /// <summary>
    ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
    ///     method if you want to use a property(s) other than the primary key as the principal property(s). If
    ///     the specified property(s) is not already a unique constraint (or the primary key) then a new unique
    ///     constraint will be introduced.
    /// </summary>
    /// <param name="keyExpression">
    ///     <para>
    ///         A lambda expression representing the referenced key property(s) (<c>blog => blog.BlogId</c>).
    ///     </para>
    ///     <para>
    ///         If the principal key is made up of multiple properties then specify an anonymous type including the
    ///         properties (<c>t => new { t.Id1, t.Id2 }</c>). The order specified should match the order of
    ///         corresponding properties in <see cref="HasForeignKey(Expression{Func{TDependentEntity,object}})" />.
    ///     </para>
    /// </param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ReferenceCollectionBuilder<TPrincipalEntity, TDependentEntity> HasPrincipalKey(
        Expression<Func<TPrincipalEntity, object?>> keyExpression)
        => new(
            HasPrincipalKeyBuilder(Check.NotNull(keyExpression, nameof(keyExpression)).GetMemberAccessList()),
            this,
            principalKeySet: true);

    /// <summary>
    ///     Configures whether this is a required relationship (i.e. whether the foreign key property(s) can
    ///     be assigned <see langword="null" />).
    /// </summary>
    /// <param name="required">A value indicating whether this is a required relationship.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual ReferenceCollectionBuilder<TPrincipalEntity, TDependentEntity> IsRequired(bool required = true)
        => new(Builder.IsRequired(required, ConfigurationSource.Explicit)!, this, requiredSet: true);

    /// <summary>
    ///     Configures the operation applied to dependent entities in the relationship when the
    ///     principal is deleted or the relationship is severed.
    /// </summary>
    /// <param name="deleteBehavior">The action to perform.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual ReferenceCollectionBuilder<TPrincipalEntity, TDependentEntity> OnDelete(DeleteBehavior deleteBehavior)
        => new(Builder.OnDelete(deleteBehavior, ConfigurationSource.Explicit)!, this);
}
