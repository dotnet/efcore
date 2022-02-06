// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a one-to-one relationship.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class ReferenceReferenceBuilder : InvertibleRelationshipBuilderBase
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public ReferenceReferenceBuilder(
        IMutableEntityType declaringEntityType,
        IMutableEntityType relatedEntityType,
        IMutableForeignKey foreignKey)
        : base(declaringEntityType, relatedEntityType, foreignKey)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected ReferenceReferenceBuilder(
        InternalForeignKeyBuilder builder,
        ReferenceReferenceBuilder oldBuilder,
        bool inverted = false,
        bool foreignKeySet = false,
        bool principalKeySet = false,
        bool requiredSet = false)
        : base(builder, oldBuilder, inverted, foreignKeySet, principalKeySet, requiredSet)
    {
    }

    /// <summary>
    ///     Adds or updates an annotation on the relationship. If an annotation with the key specified in
    ///     <paramref name="annotation" /> already exists its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ReferenceReferenceBuilder HasAnnotation(string annotation, object? value)
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
    ///         If <see cref="HasPrincipalKey(Type,string[])" /> is not specified, then an attempt will be made to
    ///         match the data type and order of foreign key properties against the primary key of the principal
    ///         entity type. If they do not match, new shadow state properties that form a unique index will be
    ///         added to the principal entity type to serve as the reference key.
    ///     </para>
    /// </remarks>
    /// <param name="dependentEntityTypeName">
    ///     The name of the entity type that is the dependent in this relationship (the type that has the foreign
    ///     key properties).
    /// </param>
    /// <param name="foreignKeyPropertyNames">
    ///     The name(s) of the foreign key property(s).
    /// </param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ReferenceReferenceBuilder HasForeignKey(
        string dependentEntityTypeName,
        params string[] foreignKeyPropertyNames)
        => new(
            HasForeignKeyBuilder(
                ResolveEntityType(Check.NotNull(dependentEntityTypeName, nameof(dependentEntityTypeName)))!,
                dependentEntityTypeName,
                Check.NotNull(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames))),
            this,
            Builder.Metadata.DeclaringEntityType.Name != ResolveEntityType(dependentEntityTypeName)!.Name,
            foreignKeySet: foreignKeyPropertyNames.Length > 0);

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
    ///         If <see cref="HasPrincipalKey(Type,string[])" /> is not specified, then an attempt will be made to
    ///         match the data type and order of foreign key properties against the primary key of the principal
    ///         entity type. If they do not match, new shadow state properties that form a unique index will be
    ///         added to the principal entity type to serve as the reference key.
    ///     </para>
    /// </remarks>
    /// <param name="dependentEntityType">
    ///     The entity type that is the dependent in this relationship (the type that has the foreign key
    ///     properties).
    /// </param>
    /// <param name="foreignKeyPropertyNames">
    ///     The name(s) of the foreign key property(s).
    /// </param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ReferenceReferenceBuilder HasForeignKey(
        Type dependentEntityType,
        params string[] foreignKeyPropertyNames)
        => new(
            HasForeignKeyBuilder(
                ResolveEntityType(Check.NotNull(dependentEntityType, nameof(dependentEntityType)))!,
                dependentEntityType.ShortDisplayName(),
                Check.NotNull(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames))),
            this,
            Builder.Metadata.DeclaringEntityType.ClrType != dependentEntityType,
            foreignKeySet: foreignKeyPropertyNames.Length > 0);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalForeignKeyBuilder HasForeignKeyBuilder(
        EntityType dependentEntityType,
        string dependentEntityTypeName,
        IReadOnlyList<string> foreignKeyPropertyNames)
        => HasForeignKeyBuilder(
            dependentEntityType, dependentEntityTypeName,
            (b, d) => b.HasForeignKey(foreignKeyPropertyNames, d, ConfigurationSource.Explicit)!);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalForeignKeyBuilder HasForeignKeyBuilder(
        EntityType dependentEntityType,
        string dependentEntityTypeName,
        IReadOnlyList<MemberInfo> foreignKeyMembers)
        => HasForeignKeyBuilder(
            dependentEntityType, dependentEntityTypeName,
            (b, d) => b.HasForeignKey(foreignKeyMembers, d, ConfigurationSource.Explicit)!);

    private InternalForeignKeyBuilder HasForeignKeyBuilder(
        EntityType? dependentEntityType,
        string dependentEntityTypeName,
        Func<InternalForeignKeyBuilder, EntityType, InternalForeignKeyBuilder> hasForeignKey)
    {
        if (dependentEntityType == null)
        {
            throw new InvalidOperationException(
                CoreStrings.DependentEntityTypeNotInRelationship(
                    DeclaringEntityType.DisplayName(),
                    RelatedEntityType.DisplayName(),
                    dependentEntityTypeName));
        }

        using var batch = dependentEntityType.Model.DelayConventions();
        var builder = Builder.HasEntityTypes(
            GetOtherEntityType(dependentEntityType), dependentEntityType, ConfigurationSource.Explicit)!;
        builder = hasForeignKey(builder, dependentEntityType);

        return batch.Run(builder)!;
    }

    /// <summary>
    ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
    ///     method if you want to use a property(s) other than the primary key as the principal property(s). If
    ///     the specified property(s) is not already a unique constraint (or the primary key) then a new unique
    ///     constraint will be introduced.
    /// </summary>
    /// <remarks>
    ///     If multiple principal key properties are specified, the order of principal key properties should
    ///     match the order that the primary key or unique constraint properties were configured on the principal
    ///     entity type.
    /// </remarks>
    /// <param name="principalEntityTypeName">
    ///     The name of the entity type that is the principal in this relationship (the type
    ///     that has the reference key properties).
    /// </param>
    /// <param name="keyPropertyNames">The name(s) of the reference key property(s).</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ReferenceReferenceBuilder HasPrincipalKey(
        string principalEntityTypeName,
        params string[] keyPropertyNames)
        => new(
            HasPrincipalKeyBuilder(
                ResolveEntityType(Check.NotEmpty(principalEntityTypeName, nameof(principalEntityTypeName)))!,
                principalEntityTypeName,
                Check.NotNull(keyPropertyNames, nameof(keyPropertyNames))),
            this,
            inverted: Builder.Metadata.PrincipalEntityType.Name != ResolveEntityType(principalEntityTypeName)!.Name,
            principalKeySet: keyPropertyNames.Length > 0);

    /// <summary>
    ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
    ///     method if you want to use a property(s) other than the primary key as the principal property(s). If
    ///     the specified property(s) is not already a unique constraint (or the primary key) then a new unique
    ///     constraint will be introduced.
    /// </summary>
    /// <remarks>
    ///     If multiple principal key properties are specified, the order of principal key properties should
    ///     match the order that the primary key or unique constraint properties were configured on the principal
    ///     entity type.
    /// </remarks>
    /// <param name="principalEntityType">
    ///     The entity type that is the principal in this relationship (the type
    ///     that has the reference key properties).
    /// </param>
    /// <param name="keyPropertyNames">The name(s) of the reference key property(s).</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ReferenceReferenceBuilder HasPrincipalKey(
        Type principalEntityType,
        params string[] keyPropertyNames)
        => new(
            HasPrincipalKeyBuilder(
                ResolveEntityType(Check.NotNull(principalEntityType, nameof(principalEntityType)))!,
                principalEntityType.ShortDisplayName(),
                Check.NotNull(keyPropertyNames, nameof(keyPropertyNames))),
            this,
            inverted: Builder.Metadata.PrincipalEntityType.ClrType != principalEntityType,
            principalKeySet: keyPropertyNames.Length > 0);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalForeignKeyBuilder HasPrincipalKeyBuilder(
        EntityType principalEntityType,
        string principalEntityTypeName,
        IReadOnlyList<string> foreignKeyPropertyNames)
        => HasPrincipalKeyBuilder(
            principalEntityType, principalEntityTypeName,
            b => b.HasPrincipalKey(foreignKeyPropertyNames, ConfigurationSource.Explicit)!);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalForeignKeyBuilder HasPrincipalKeyBuilder(
        EntityType principalEntityType,
        string principalEntityTypeName,
        IReadOnlyList<MemberInfo> foreignKeyMembers)
        => HasPrincipalKeyBuilder(
            principalEntityType, principalEntityTypeName,
            b => b.HasPrincipalKey(foreignKeyMembers, ConfigurationSource.Explicit)!);

    private InternalForeignKeyBuilder HasPrincipalKeyBuilder(
        EntityType? principalEntityType,
        string principalEntityTypeName,
        Func<InternalForeignKeyBuilder, InternalForeignKeyBuilder> hasPrincipalKey)
    {
        if (principalEntityType == null)
        {
            throw new InvalidOperationException(
                CoreStrings.PrincipalEntityTypeNotInRelationship(
                    DeclaringEntityType.DisplayName(),
                    RelatedEntityType.DisplayName(),
                    principalEntityTypeName));
        }

        using var batch = principalEntityType.Model.DelayConventions();
        var builder = Builder.HasEntityTypes(
            principalEntityType, GetOtherEntityType(principalEntityType), ConfigurationSource.Explicit)!;
        builder = hasPrincipalKey(builder);

        return batch.Run(builder)!;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual EntityType? ResolveEntityType(string entityTypeName)
    {
        if (DeclaringEntityType.Name == entityTypeName)
        {
            return (EntityType)DeclaringEntityType;
        }

        if (RelatedEntityType.Name == entityTypeName)
        {
            return (EntityType)RelatedEntityType;
        }

        if (DeclaringEntityType.DisplayName() == entityTypeName)
        {
            return (EntityType)DeclaringEntityType;
        }

        if (RelatedEntityType.DisplayName() == entityTypeName)
        {
            return (EntityType)RelatedEntityType;
        }

        if (DeclaringEntityType.HasSharedClrType
            && DeclaringEntityType.ShortName() == entityTypeName)
        {
            return (EntityType)DeclaringEntityType;
        }

        return RelatedEntityType.HasSharedClrType && RelatedEntityType.ShortName() == entityTypeName
            ? (EntityType)RelatedEntityType
            : null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual EntityType? ResolveEntityType(Type entityType)
    {
        if (DeclaringEntityType.ClrType == entityType)
        {
            return (EntityType)DeclaringEntityType;
        }

        return RelatedEntityType.ClrType == entityType ? (EntityType)RelatedEntityType : null;
    }

    private EntityType GetOtherEntityType(EntityType entityType)
        => DeclaringEntityType == entityType ? (EntityType)RelatedEntityType : (EntityType)DeclaringEntityType;

    /// <summary>
    ///     Configures whether this is a required relationship (i.e. whether the foreign key property(s) can
    ///     be assigned <see langword="null" />).
    /// </summary>
    /// <param name="required">A value indicating whether this is a required relationship.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ReferenceReferenceBuilder IsRequired(bool required = true)
        => new(Builder.IsRequired(required, ConfigurationSource.Explicit)!, this, requiredSet: true);

    /// <summary>
    ///     Configures the operation applied to dependent entities in the relationship when the
    ///     principal is deleted or the relationship is severed.
    /// </summary>
    /// <param name="deleteBehavior">The action to perform.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ReferenceReferenceBuilder OnDelete(DeleteBehavior deleteBehavior)
        => new(Builder.OnDelete(deleteBehavior, ConfigurationSource.Explicit)!, this);
}
