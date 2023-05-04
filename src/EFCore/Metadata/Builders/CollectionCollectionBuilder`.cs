// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a many-to-many relationship.
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
/// <typeparam name="TLeftEntity">One of the entity types in this relationship.</typeparam>
/// <typeparam name="TRightEntity">One of the entity types in this relationship.</typeparam>
public class CollectionCollectionBuilder<
    [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TLeftEntity,
    [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRightEntity> : CollectionCollectionBuilder
    where TLeftEntity : class
    where TRightEntity : class
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public CollectionCollectionBuilder(
        IMutableEntityType leftEntityType,
        IMutableEntityType rightEntityType,
        IMutableSkipNavigation leftNavigation,
        IMutableSkipNavigation rightNavigation)
        : base(leftEntityType, rightEntityType, leftNavigation, rightNavigation)
    {
    }

    /// <summary>
    ///     Configures the join entity type implementing the many-to-many relationship.
    /// </summary>
    /// <typeparam name="TJoinEntity">The CLR type of the join entity.</typeparam>
    /// <returns>The builder for the join entity type.</returns>
    public virtual EntityTypeBuilder<TJoinEntity> UsingEntity
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TJoinEntity>()
        where TJoinEntity : class
        => Using<TJoinEntity>(joinEntityName: null, configureRight: null, configureLeft: null);

    /// <summary>
    ///     Configures the join entity type implementing the many-to-many relationship.
    /// </summary>
    /// <param name="joinEntityName">The name of the join entity.</param>
    /// <typeparam name="TJoinEntity">The CLR type of the join entity.</typeparam>
    /// <returns>The builder for the join entity type.</returns>
    public virtual EntityTypeBuilder<TJoinEntity> UsingEntity
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TJoinEntity>(string joinEntityName)
        where TJoinEntity : class
    {
        Check.NotEmpty(joinEntityName, nameof(joinEntityName));

        return Using<TJoinEntity>(joinEntityName, configureRight: null, configureLeft: null);
    }

    /// <summary>
    ///     Configures the join entity type implementing the many-to-many relationship.
    /// </summary>
    /// <param name="configureJoinEntityType">The configuration of the join entity type.</param>
    /// <returns>The builder for the originating entity type so that multiple configuration calls can be chained.</returns>
    public new virtual EntityTypeBuilder<TRightEntity> UsingEntity(
        Action<EntityTypeBuilder> configureJoinEntityType)
    {
        Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));
        Check.DebugAssert(LeftNavigation.JoinEntityType != null, "LeftNavigation.JoinEntityType is null");
        Check.DebugAssert(RightNavigation.JoinEntityType != null, "RightNavigation.JoinEntityType is null");
        Check.DebugAssert(
            LeftNavigation.JoinEntityType == RightNavigation.JoinEntityType,
            "LeftNavigation.JoinEntityType != RightNavigation.JoinEntityType");

        configureJoinEntityType(new EntityTypeBuilder(LeftNavigation.JoinEntityType));

        return new EntityTypeBuilder<TRightEntity>(RightEntityType);
    }

    /// <summary>
    ///     Configures the join entity type implementing the many-to-many relationship.
    /// </summary>
    /// <param name="joinEntityType">The CLR type of the join entity.</param>
    /// <param name="configureJoinEntityType">The configuration of the join entity type.</param>
    /// <returns>The builder for the originating entity type so that multiple configuration calls can be chained.</returns>
    public new virtual EntityTypeBuilder<TRightEntity> UsingEntity(
        Type joinEntityType,
        Action<EntityTypeBuilder> configureJoinEntityType)
    {
        Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));

        configureJoinEntityType(UsingEntity(joinEntityType));

        return new EntityTypeBuilder<TRightEntity>(RightEntityType);
    }

    /// <summary>
    ///     Configures the join entity type implementing the many-to-many relationship.
    /// </summary>
    /// <param name="joinEntityName">The name of the join entity.</param>
    /// <param name="configureJoinEntityType">The configuration of the join entity type.</param>
    /// <returns>The builder for the originating entity type so that multiple configuration calls can be chained.</returns>
    public new virtual EntityTypeBuilder<TRightEntity> UsingEntity(
        string joinEntityName,
        Action<EntityTypeBuilder> configureJoinEntityType)
    {
        Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));

        configureJoinEntityType(UsingEntity(joinEntityName));

        return new EntityTypeBuilder<TRightEntity>(RightEntityType);
    }

    /// <summary>
    ///     Configures the join entity type implementing the many-to-many relationship.
    /// </summary>
    /// <param name="joinEntityName">The name of the join entity.</param>
    /// <param name="joinEntityType">The CLR type of the join entity.</param>
    /// <param name="configureJoinEntityType">The configuration of the join entity type.</param>
    /// <returns>The builder for the originating entity type so that multiple configuration calls can be chained.</returns>
    public new virtual EntityTypeBuilder<TRightEntity> UsingEntity(
        string joinEntityName,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type joinEntityType,
        Action<EntityTypeBuilder> configureJoinEntityType)
    {
        Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));

        configureJoinEntityType(UsingEntity(joinEntityName, joinEntityType));

        return new EntityTypeBuilder<TRightEntity>(RightEntityType);
    }

    /// <summary>
    ///     Configures the join entity type implementing the many-to-many relationship.
    /// </summary>
    /// <param name="configureJoinEntityType">The configuration of the join entity type.</param>
    /// <typeparam name="TJoinEntity">The CLR type of the join entity.</typeparam>
    /// <returns>The builder for the originating entity type so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder<TRightEntity> UsingEntity
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TJoinEntity>(
            Action<EntityTypeBuilder<TJoinEntity>> configureJoinEntityType)
        where TJoinEntity : class
    {
        Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));

        var entityTypeBuilder = UsingEntity<TJoinEntity>();
        configureJoinEntityType(entityTypeBuilder);

        return new EntityTypeBuilder<TRightEntity>(RightEntityType);
    }

    /// <summary>
    ///     Configures the join entity type implementing the many-to-many relationship.
    /// </summary>
    /// <param name="joinEntityName">The name of the join entity.</param>
    /// <param name="configureJoinEntityType">The configuration of the join entity type.</param>
    /// <typeparam name="TJoinEntity">The CLR type of the join entity.</typeparam>
    /// <returns>The builder for the originating entity type so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder<TRightEntity> UsingEntity
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TJoinEntity>(
            string joinEntityName,
            Action<EntityTypeBuilder<TJoinEntity>> configureJoinEntityType)
        where TJoinEntity : class
    {
        Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));

        var entityTypeBuilder = UsingEntity<TJoinEntity>(joinEntityName);
        configureJoinEntityType(entityTypeBuilder);

        return new EntityTypeBuilder<TRightEntity>(RightEntityType);
    }

    /// <summary>
    ///     Configures the relationships to the entity types participating in the many-to-many relationship.
    /// </summary>
    /// <param name="configureRight">The configuration for the relationship to the right entity type.</param>
    /// <param name="configureLeft">The configuration for the relationship to the left entity type.</param>
    /// <typeparam name="TJoinEntity">The CLR type of the join entity.</typeparam>
    /// <returns>The builder for the join type.</returns>
    public virtual EntityTypeBuilder<TJoinEntity> UsingEntity
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TJoinEntity>(
            Func<EntityTypeBuilder<TJoinEntity>, ReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            Func<EntityTypeBuilder<TJoinEntity>, ReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft)
        where TJoinEntity : class
    {
        Check.NotNull(configureRight, nameof(configureRight));
        Check.NotNull(configureLeft, nameof(configureLeft));

        return Using(joinEntityName: null, configureRight, configureLeft);
    }

    /// <summary>
    ///     Configures the relationships to the entity types participating in the many-to-many relationship.
    /// </summary>
    /// <param name="joinEntityName">The name of the join entity.</param>
    /// <param name="configureRight">The configuration for the relationship to the right entity type.</param>
    /// <param name="configureLeft">The configuration for the relationship to the left entity type.</param>
    /// <typeparam name="TJoinEntity">The CLR type of the join entity.</typeparam>
    /// <returns>The builder for the join entity type.</returns>
    public virtual EntityTypeBuilder<TJoinEntity> UsingEntity
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TJoinEntity>(
            string joinEntityName,
            Func<EntityTypeBuilder<TJoinEntity>, ReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            Func<EntityTypeBuilder<TJoinEntity>, ReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft)
        where TJoinEntity : class
    {
        Check.NotEmpty(joinEntityName, nameof(joinEntityName));
        Check.NotNull(configureRight, nameof(configureRight));
        Check.NotNull(configureLeft, nameof(configureLeft));

        return Using(joinEntityName, configureRight, configureLeft);
    }

    /// <summary>
    ///     Configures the relationships to the entity types participating in the many-to-many relationship.
    /// </summary>
    /// <param name="configureRight">The configuration for the relationship to the right entity type.</param>
    /// <param name="configureLeft">The configuration for the relationship to the left entity type.</param>
    /// <param name="configureJoinEntityType">The configuration of the join entity type.</param>
    /// <returns>The builder for the originating entity type so that multiple configuration calls can be chained.</returns>
    public new virtual EntityTypeBuilder<TRightEntity> UsingEntity(
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureRight,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureLeft,
        Action<EntityTypeBuilder> configureJoinEntityType)
    {
        Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));

        configureJoinEntityType(UsingEntity(configureRight, configureLeft));

        return new EntityTypeBuilder<TRightEntity>(RightEntityType);
    }

    /// <summary>
    ///     Configures the relationships to the entity types participating in the many-to-many relationship.
    /// </summary>
    /// <param name="joinEntityType">The CLR type of the join entity.</param>
    /// <param name="configureRight">The configuration for the relationship to the right entity type.</param>
    /// <param name="configureLeft">The configuration for the relationship to the left entity type.</param>
    /// <param name="configureJoinEntityType">The configuration of the join entity type.</param>
    /// <returns>The builder for the originating entity type so that multiple configuration calls can be chained.</returns>
    public new virtual EntityTypeBuilder<TRightEntity> UsingEntity(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type joinEntityType,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureRight,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureLeft,
        Action<EntityTypeBuilder> configureJoinEntityType)
    {
        Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));

        configureJoinEntityType(UsingEntity(joinEntityType, configureRight, configureLeft));

        return new EntityTypeBuilder<TRightEntity>(RightEntityType);
    }

    /// <summary>
    ///     Configures the relationships to the entity types participating in the many-to-many relationship.
    /// </summary>
    /// <param name="joinEntityName">The name of the join entity.</param>
    /// <param name="configureRight">The configuration for the relationship to the right entity type.</param>
    /// <param name="configureLeft">The configuration for the relationship to the left entity type.</param>
    /// <param name="configureJoinEntityType">The configuration of the join entity type.</param>
    /// <returns>The builder for the originating entity type so that multiple configuration calls can be chained.</returns>
    public new virtual EntityTypeBuilder<TRightEntity> UsingEntity(
        string joinEntityName,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureRight,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureLeft,
        Action<EntityTypeBuilder> configureJoinEntityType)
    {
        Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));

        configureJoinEntityType(UsingEntity(joinEntityName, configureRight, configureLeft));

        return new EntityTypeBuilder<TRightEntity>(RightEntityType);
    }

    /// <summary>
    ///     Configures the relationships to the entity types participating in the many-to-many relationship.
    /// </summary>
    /// <param name="joinEntityName">The name of the join entity.</param>
    /// <param name="joinEntityType">The CLR type of the join entity.</param>
    /// <param name="configureRight">The configuration for the relationship to the right entity type.</param>
    /// <param name="configureLeft">The configuration for the relationship to the left entity type.</param>
    /// <param name="configureJoinEntityType">The configuration of the join entity type.</param>
    /// <returns>The builder for the originating entity type so that multiple configuration calls can be chained.</returns>
    public new virtual EntityTypeBuilder<TRightEntity> UsingEntity(
        string joinEntityName,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type joinEntityType,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureRight,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureLeft,
        Action<EntityTypeBuilder> configureJoinEntityType)
    {
        Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));

        configureJoinEntityType(UsingEntity(joinEntityName, joinEntityType, configureRight, configureLeft));

        return new EntityTypeBuilder<TRightEntity>(RightEntityType);
    }

    /// <summary>
    ///     Configures the relationships to the entity types participating in the many-to-many relationship.
    /// </summary>
    /// <param name="configureRight">The configuration for the relationship to the right entity type.</param>
    /// <param name="configureLeft">The configuration for the relationship to the left entity type.</param>
    /// <param name="configureJoinEntityType">The configuration of the join entity type.</param>
    /// <typeparam name="TJoinEntity">The CLR type of the join entity.</typeparam>
    /// <returns>The builder for the originating entity type so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder<TRightEntity> UsingEntity
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TJoinEntity>(
            Func<EntityTypeBuilder<TJoinEntity>, ReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            Func<EntityTypeBuilder<TJoinEntity>, ReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft,
            Action<EntityTypeBuilder<TJoinEntity>> configureJoinEntityType)
        where TJoinEntity : class
    {
        Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));

        var entityTypeBuilder = UsingEntity(configureRight, configureLeft);
        configureJoinEntityType(entityTypeBuilder);

        return new EntityTypeBuilder<TRightEntity>(RightEntityType);
    }

    /// <summary>
    ///     Configures the relationships to the entity types participating in the many-to-many relationship.
    /// </summary>
    /// <param name="joinEntityName">The name of the join entity.</param>
    /// <param name="configureRight">The configuration for the relationship to the right entity type.</param>
    /// <param name="configureLeft">The configuration for the relationship to the left entity type.</param>
    /// <param name="configureJoinEntityType">The configuration of the join entity type.</param>
    /// <typeparam name="TJoinEntity">The CLR type of the join entity.</typeparam>
    /// <returns>The builder for the originating entity type so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder<TRightEntity> UsingEntity<
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TJoinEntity>(
        string joinEntityName,
        Func<EntityTypeBuilder<TJoinEntity>, ReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
        Func<EntityTypeBuilder<TJoinEntity>, ReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft,
        Action<EntityTypeBuilder<TJoinEntity>> configureJoinEntityType)
        where TJoinEntity : class
    {
        Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));

        var entityTypeBuilder = UsingEntity(joinEntityName, configureRight, configureLeft);
        configureJoinEntityType(entityTypeBuilder);

        return new EntityTypeBuilder<TRightEntity>(RightEntityType);
    }

    private EntityTypeBuilder<TJoinEntity> Using<[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TJoinEntity>(
        string? joinEntityName,
        Func<EntityTypeBuilder<TJoinEntity>, ReferenceCollectionBuilder<TLeftEntity, TJoinEntity>>? configureRight,
        Func<EntityTypeBuilder<TJoinEntity>, ReferenceCollectionBuilder<TRightEntity, TJoinEntity>>? configureLeft)
        where TJoinEntity : class
        => new(
            UsingEntity(
                joinEntityName,
                typeof(TJoinEntity),
                configureRight != null
                    ? e => configureRight(new EntityTypeBuilder<TJoinEntity>(e)).Metadata
                    : null,
                configureLeft != null
                    ? e => configureLeft(new EntityTypeBuilder<TJoinEntity>(e)).Metadata
                    : null));
}
