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
public class ReferenceCollectionBuilder : RelationshipBuilderBase
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
    ///     <paramref name="annotation" />
    ///     already exists its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ReferenceCollectionBuilder HasAnnotation(string annotation, object? value)
    {
        Check.NotEmpty(annotation, nameof(annotation));

        Builder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);

        return this;
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
    ///         If <see cref="HasPrincipalKey" /> is not specified, then an attempt will be made to match
    ///         the data type and order of foreign key properties against the primary key of the principal
    ///         entity type. If they do not match, new shadow state properties that form a unique index will be
    ///         added to the principal entity type to serve as the reference key.
    ///     </para>
    /// </remarks>
    /// <param name="foreignKeyPropertyNames">
    ///     The name(s) of the foreign key property(s).
    /// </param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ReferenceCollectionBuilder HasForeignKey(params string[] foreignKeyPropertyNames)
        => new(
            HasForeignKeyBuilder(Check.NotEmpty(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames))),
            this,
            foreignKeySet: true);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalForeignKeyBuilder HasForeignKeyBuilder(IReadOnlyList<string> foreignKeyPropertyNames)
        => Builder.HasForeignKey(foreignKeyPropertyNames, (EntityType)DependentEntityType, ConfigurationSource.Explicit)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalForeignKeyBuilder HasForeignKeyBuilder(IReadOnlyList<MemberInfo> foreignKeyMembers)
        => Builder.HasForeignKey(foreignKeyMembers, (EntityType)DependentEntityType, ConfigurationSource.Explicit)!;

    /// <summary>
    ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
    ///     method if you want to use a property(s) other than the primary key as the principal property(s). If
    ///     the specified property(s) is not already a unique constraint (or the primary key) then a new unique
    ///     constraint will be introduced.
    /// </summary>
    /// <param name="keyPropertyNames">The name(s) of the referenced key property(s).</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ReferenceCollectionBuilder HasPrincipalKey(params string[] keyPropertyNames)
        => new(
            HasPrincipalKeyBuilder(Check.NotEmpty(keyPropertyNames, nameof(keyPropertyNames))),
            this,
            principalKeySet: true);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalForeignKeyBuilder HasPrincipalKeyBuilder(IReadOnlyList<string> keyPropertyNames)
        => Builder.HasPrincipalKey(keyPropertyNames, ConfigurationSource.Explicit)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalForeignKeyBuilder HasPrincipalKeyBuilder(IReadOnlyList<MemberInfo> keyMembers)
        => Builder.HasPrincipalKey(keyMembers, ConfigurationSource.Explicit)!;

    /// <summary>
    ///     Configures whether this is a required relationship (i.e. whether the foreign key property(s) can
    ///     be assigned <see langword="null" />).
    /// </summary>
    /// <param name="required">A value indicating whether this is a required relationship.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ReferenceCollectionBuilder IsRequired(bool required = true)
        => new(Builder.IsRequired(required, ConfigurationSource.Explicit)!, this, requiredSet: true);

    /// <summary>
    ///     Configures the operation applied to dependent entities in the relationship when the
    ///     principal is deleted or the relationship is severed.
    /// </summary>
    /// <param name="deleteBehavior">The action to perform.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ReferenceCollectionBuilder OnDelete(DeleteBehavior deleteBehavior)
        => new(Builder.OnDelete(deleteBehavior, ConfigurationSource.Explicit)!, this);
}
