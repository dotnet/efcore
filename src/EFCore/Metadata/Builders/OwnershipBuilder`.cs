// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring an ownership.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class OwnershipBuilder<TEntity, TDependentEntity> : OwnershipBuilder
    where TEntity : class
    where TDependentEntity : class
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public OwnershipBuilder(
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
    protected OwnershipBuilder(
        InternalForeignKeyBuilder builder,
        OwnershipBuilder oldBuilder,
        bool foreignKeySet = false,
        bool principalKeySet = false,
        bool requiredSet = false)
        : base(builder, oldBuilder, foreignKeySet, principalKeySet, requiredSet)
    {
    }

    /// <summary>
    ///     Adds or updates an annotation on the foreign key. If an annotation with the key specified in
    ///     <paramref name="annotation" /> already exists its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual OwnershipBuilder<TEntity, TDependentEntity> HasAnnotation(
        string annotation,
        object? value)
        => (OwnershipBuilder<TEntity, TDependentEntity>)base.HasAnnotation(annotation, value);

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
    ///         If <see cref="HasPrincipalKey(string[])" /> is not specified, then an attempt will be made to
    ///         match the data type and order of foreign key properties against the primary key of the principal
    ///         entity type. If they do not match, new shadow state properties that form a unique index will be
    ///         added to the principal entity type to serve as the reference key.
    ///     </para>
    /// </remarks>
    /// <param name="foreignKeyPropertyNames">
    ///     The name(s) of the foreign key property(s).
    /// </param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual OwnershipBuilder<TEntity, TDependentEntity> HasForeignKey(
        params string[] foreignKeyPropertyNames)
    {
        Builder = Builder.HasForeignKey(
            Check.NotNull(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames)),
            (EntityType)DependentEntityType,
            ConfigurationSource.Explicit)!;
        return new OwnershipBuilder<TEntity, TDependentEntity>(
            Builder,
            this,
            foreignKeySet: foreignKeyPropertyNames.Length > 0);
    }

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
    ///         If <see cref="HasPrincipalKey(Expression{Func{TEntity, object}})" /> is not specified, then an
    ///         attempt will be made to match the data type and order of foreign key properties against the primary
    ///         key of the principal entity type. If they do not match, new shadow state properties that form a
    ///         unique index will be added to the principal entity type to serve as the reference key.
    ///     </para>
    /// </remarks>
    /// <param name="foreignKeyExpression">
    ///     <para>
    ///         A lambda expression representing the foreign key property(s) (<c>t => t.Id1</c>).
    ///     </para>
    ///     <para>
    ///         If the foreign key is made up of multiple properties then specify an anonymous type including the
    ///         properties (<c>t => new { t.Id1, t.Id2 }</c>). The order specified should match the order of
    ///         corresponding properties in <see cref="HasPrincipalKey(Expression{Func{TEntity, object}})" />.
    ///     </para>
    /// </param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual OwnershipBuilder<TEntity, TDependentEntity> HasForeignKey(
        Expression<Func<TDependentEntity, object?>> foreignKeyExpression)
    {
        Builder = Builder.HasForeignKey(
            Check.NotNull(foreignKeyExpression, nameof(foreignKeyExpression)).GetMemberAccessList(),
            (EntityType)DependentEntityType,
            ConfigurationSource.Explicit)!;
        return new OwnershipBuilder<TEntity, TDependentEntity>(
            Builder,
            this,
            foreignKeySet: true);
    }

    /// <summary>
    ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
    ///     method if you want to use a property(s) other than the primary key as the principal property(s). If
    ///     the specified property(s) is not already a unique constraint (or the primary key) then a new unique
    ///     constraint will be introduced.
    /// </summary>
    /// <param name="keyPropertyNames">The name(s) of the reference key property(s).</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual OwnershipBuilder<TEntity, TDependentEntity> HasPrincipalKey(
        params string[] keyPropertyNames)
    {
        Builder = Builder.HasPrincipalKey(
            Check.NotNull(keyPropertyNames, nameof(keyPropertyNames)),
            ConfigurationSource.Explicit)!;
        return new OwnershipBuilder<TEntity, TDependentEntity>(
            Builder,
            this,
            principalKeySet: keyPropertyNames.Length > 0);
    }

    /// <summary>
    ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
    ///     method if you want to use a property(s) other than the primary key as the principal property(s). If
    ///     the specified property(s) is not already a unique constraint (or the primary key) then a new unique
    ///     constraint will be introduced.
    /// </summary>
    /// <param name="keyExpression">
    ///     <para>
    ///         A lambda expression representing the reference key property(s) (<c>t => t.Id</c>).
    ///     </para>
    ///     <para>
    ///         If the principal key is made up of multiple properties then specify an anonymous type including the
    ///         properties (<c>t => new { t.Id1, t.Id2 }</c>). The order specified should match the order of
    ///         corresponding properties in <see cref="HasForeignKey(Expression{Func{TDependentEntity, object}})" />.
    ///     </para>
    /// </param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual OwnershipBuilder<TEntity, TDependentEntity> HasPrincipalKey(
        Expression<Func<TEntity, object?>> keyExpression)
    {
        Builder = Builder.HasPrincipalKey(
            Check.NotNull(keyExpression, nameof(keyExpression)).GetMemberAccessList(),
            ConfigurationSource.Explicit)!;
        return new OwnershipBuilder<TEntity, TDependentEntity>(
            Builder,
            this,
            principalKeySet: true);
    }
}
