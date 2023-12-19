// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
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
public class CollectionCollectionBuilder
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
    {
        Check.DebugAssert(((IConventionEntityType)leftEntityType).IsInModel, "Not in model");
        Check.DebugAssert(((IConventionEntityType)rightEntityType).IsInModel, "Not in model");
        Check.DebugAssert(((IConventionSkipNavigation)leftNavigation).IsInModel, "Not in model");
        Check.DebugAssert(((IConventionSkipNavigation)rightNavigation).IsInModel, "Not in model");

        if (leftNavigation == rightNavigation)
        {
            throw new InvalidOperationException(
                CoreStrings.ManyToManyOneNav(leftEntityType.DisplayName(), leftNavigation.Name));
        }

        LeftEntityType = leftEntityType;
        RightEntityType = rightEntityType;
        LeftNavigation = leftNavigation;
        RightNavigation = rightNavigation;

        var leftSkipNavigation = (SkipNavigation)leftNavigation;
        var rightSkipNavigation = (SkipNavigation)rightNavigation;

        leftSkipNavigation.Builder.HasInverse(rightSkipNavigation, ConfigurationSource.Explicit);

        // We delayed setting the ConfigurationSource of SkipNavigation in HasMany().
        // But now we know that both navigations are skip navigations.
        leftSkipNavigation.UpdateConfigurationSource(ConfigurationSource.Explicit);
        rightSkipNavigation.UpdateConfigurationSource(ConfigurationSource.Explicit);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual IMutableEntityType LeftEntityType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual IMutableEntityType RightEntityType { get; }

    /// <summary>
    ///     One of the navigations involved in the relationship.
    /// </summary>
    public virtual IMutableSkipNavigation LeftNavigation { get; }

    /// <summary>
    ///     One of the navigations involved in the relationship.
    /// </summary>
    public virtual IMutableSkipNavigation RightNavigation { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalModelBuilder ModelBuilder
        => ((EntityType)LeftEntityType).Model.Builder;

    /// <summary>
    ///     Configures the join entity type implementing the many-to-many relationship.
    /// </summary>
    /// <param name="joinEntityType">The CLR type of the join entity.</param>
    /// <returns>The builder for the join entity type.</returns>
    public virtual EntityTypeBuilder UsingEntity(
        Type joinEntityType)
    {
        Check.NotNull(joinEntityType, nameof(joinEntityType));

        return Using(joinEntityName: null, joinEntityType, configureRight: null, configureLeft: null);
    }

    /// <summary>
    ///     Configures the join entity type implementing the many-to-many relationship.
    /// </summary>
    /// <param name="joinEntityName">The name of the join entity.</param>
    /// <returns>The builder for the join entity type.</returns>
    public virtual EntityTypeBuilder UsingEntity(
        string joinEntityName)
    {
        Check.NotEmpty(joinEntityName, nameof(joinEntityName));

        return Using(joinEntityName, joinEntityType: null, configureRight: null, configureLeft: null);
    }

    /// <summary>
    ///     Configures the join entity type implementing the many-to-many relationship.
    /// </summary>
    /// <param name="joinEntityName">The name of the join entity.</param>
    /// <param name="joinEntityType">The CLR type of the join entity.</param>
    /// <returns>The builder for the join entity type.</returns>
    public virtual EntityTypeBuilder UsingEntity(
        string joinEntityName,
        Type joinEntityType)
    {
        Check.NotEmpty(joinEntityName, nameof(joinEntityName));
        Check.NotNull(joinEntityType, nameof(joinEntityType));

        return Using(joinEntityName, joinEntityType, configureRight: null, configureLeft: null);
    }

    /// <summary>
    ///     Configures the join entity type implementing the many-to-many relationship.
    /// </summary>
    /// <param name="configureJoinEntityType">The configuration of the join entity type.</param>
    /// <returns>The builder for the originating entity type so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder UsingEntity(
        Action<EntityTypeBuilder> configureJoinEntityType)
    {
        Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));
        Check.DebugAssert(LeftNavigation.JoinEntityType != null, "LeftNavigation.JoinEntityType is null");
        Check.DebugAssert(RightNavigation.JoinEntityType != null, "RightNavigation.JoinEntityType is null");
        Check.DebugAssert(
            LeftNavigation.JoinEntityType == RightNavigation.JoinEntityType,
            "LeftNavigation.JoinEntityType != RightNavigation.JoinEntityType");

        configureJoinEntityType(new EntityTypeBuilder(LeftNavigation.JoinEntityType));

        return new EntityTypeBuilder(RightEntityType);
    }

    /// <summary>
    ///     Configures the join entity type implementing the many-to-many relationship.
    /// </summary>
    /// <param name="joinEntityType">The CLR type of the join entity.</param>
    /// <param name="configureJoinEntityType">The configuration of the join entity type.</param>
    /// <returns>The builder for the originating entity type so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder UsingEntity(
        Type joinEntityType,
        Action<EntityTypeBuilder> configureJoinEntityType)
    {
        Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));

        configureJoinEntityType(UsingEntity(joinEntityType));

        return new EntityTypeBuilder(RightEntityType);
    }

    /// <summary>
    ///     Configures the join entity type implementing the many-to-many relationship.
    /// </summary>
    /// <param name="joinEntityName">The name of the join entity.</param>
    /// <param name="configureJoinEntityType">The configuration of the join entity type.</param>
    /// <returns>The builder for the originating entity type so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder UsingEntity(
        string joinEntityName,
        Action<EntityTypeBuilder> configureJoinEntityType)
    {
        Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));

        configureJoinEntityType(UsingEntity(joinEntityName));

        return new EntityTypeBuilder(RightEntityType);
    }

    /// <summary>
    ///     Configures the join entity type implementing the many-to-many relationship.
    /// </summary>
    /// <param name="joinEntityName">The name of the join entity.</param>
    /// <param name="joinEntityType">The CLR type of the join entity.</param>
    /// <param name="configureJoinEntityType">The configuration of the join entity type.</param>
    /// <returns>The builder for the originating entity type so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder UsingEntity(
        string joinEntityName,
        Type joinEntityType,
        Action<EntityTypeBuilder> configureJoinEntityType)
    {
        Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));

        configureJoinEntityType(UsingEntity(joinEntityName, joinEntityType));

        return new EntityTypeBuilder(RightEntityType);
    }

    /// <summary>
    ///     Configures the relationships to the entity types participating in the many-to-many relationship.
    /// </summary>
    /// <param name="configureRight">The configuration for the relationship to the right entity type.</param>
    /// <param name="configureLeft">The configuration for the relationship to the left entity type.</param>
    /// <returns>The builder for the join entity type.</returns>
    public virtual EntityTypeBuilder UsingEntity(
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureRight,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureLeft)
    {
        Check.NotNull(configureRight, nameof(configureRight));
        Check.NotNull(configureLeft, nameof(configureLeft));

        return Using(joinEntityName: null, joinEntityType: null, configureRight, configureLeft);
    }

    /// <summary>
    ///     Configures the relationships to the entity types participating in the many-to-many relationship.
    /// </summary>
    /// <param name="joinEntityType">The CLR type of the join entity.</param>
    /// <param name="configureRight">The configuration for the relationship to the right entity type.</param>
    /// <param name="configureLeft">The configuration for the relationship to the left entity type.</param>
    /// <returns>The builder for the join entity type.</returns>
    public virtual EntityTypeBuilder UsingEntity(
        Type joinEntityType,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureRight,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureLeft)
    {
        Check.NotNull(joinEntityType, nameof(joinEntityType));
        Check.NotNull(configureRight, nameof(configureRight));
        Check.NotNull(configureLeft, nameof(configureLeft));

        return Using(joinEntityName: null, joinEntityType, configureRight, configureLeft);
    }

    /// <summary>
    ///     Configures the relationships to the entity types participating in the many-to-many relationship.
    /// </summary>
    /// <param name="joinEntityName">The name of the join entity.</param>
    /// <param name="configureRight">The configuration for the relationship to the right entity type.</param>
    /// <param name="configureLeft">The configuration for the relationship to the left entity type.</param>
    /// <returns>The builder for the join entity type.</returns>
    public virtual EntityTypeBuilder UsingEntity(
        string joinEntityName,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureRight,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureLeft)
    {
        Check.NotEmpty(joinEntityName, nameof(joinEntityName));
        Check.NotNull(configureRight, nameof(configureRight));
        Check.NotNull(configureLeft, nameof(configureLeft));

        return Using(joinEntityName, joinEntityType: null, configureRight, configureLeft);
    }

    /// <summary>
    ///     Configures the relationships to the entity types participating in the many-to-many relationship.
    /// </summary>
    /// <param name="joinEntityName">The name of the join entity.</param>
    /// <param name="joinEntityType">The CLR type of the join entity.</param>
    /// <param name="configureRight">The configuration for the relationship to the right entity type.</param>
    /// <param name="configureLeft">The configuration for the relationship to the left entity type.</param>
    /// <returns>The builder for the join entity type.</returns>
    public virtual EntityTypeBuilder UsingEntity(
        string joinEntityName,
        Type joinEntityType,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureRight,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureLeft)
    {
        Check.NotEmpty(joinEntityName, nameof(joinEntityName));
        Check.NotNull(joinEntityType, nameof(joinEntityType));
        Check.NotNull(configureRight, nameof(configureRight));
        Check.NotNull(configureLeft, nameof(configureLeft));

        return Using(joinEntityName, joinEntityType, configureRight, configureLeft);
    }

    /// <summary>
    ///     Configures the relationships to the entity types participating in the many-to-many relationship.
    /// </summary>
    /// <param name="configureRight">The configuration for the relationship to the right entity type.</param>
    /// <param name="configureLeft">The configuration for the relationship to the left entity type.</param>
    /// <param name="configureJoinEntityType">The configuration of the join entity type.</param>
    /// <returns>The builder for the originating entity type so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder UsingEntity(
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureRight,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureLeft,
        Action<EntityTypeBuilder> configureJoinEntityType)
    {
        Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));

        configureJoinEntityType(UsingEntity(configureRight, configureLeft));

        return new EntityTypeBuilder(RightEntityType);
    }

    /// <summary>
    ///     Configures the relationships to the entity types participating in the many-to-many relationship.
    /// </summary>
    /// <param name="joinEntityType">The CLR type of the join entity.</param>
    /// <param name="configureRight">The configuration for the relationship to the right entity type.</param>
    /// <param name="configureLeft">The configuration for the relationship to the left entity type.</param>
    /// <param name="configureJoinEntityType">The configuration of the join entity type.</param>
    /// <returns>The builder for the originating entity type so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder UsingEntity(
        Type joinEntityType,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureRight,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureLeft,
        Action<EntityTypeBuilder> configureJoinEntityType)
    {
        Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));

        configureJoinEntityType(UsingEntity(joinEntityType, configureRight, configureLeft));

        return new EntityTypeBuilder(RightEntityType);
    }

    /// <summary>
    ///     Configures the relationships to the entity types participating in the many-to-many relationship.
    /// </summary>
    /// <param name="joinEntityName">The name of the join entity.</param>
    /// <param name="configureRight">The configuration for the relationship to the right entity type.</param>
    /// <param name="configureLeft">The configuration for the relationship to the left entity type.</param>
    /// <param name="configureJoinEntityType">The configuration of the join entity type.</param>
    /// <returns>The builder for the originating entity type so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder UsingEntity(
        string joinEntityName,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureRight,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureLeft,
        Action<EntityTypeBuilder> configureJoinEntityType)
    {
        Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));

        configureJoinEntityType(UsingEntity(joinEntityName, configureRight, configureLeft));

        return new EntityTypeBuilder(RightEntityType);
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
    public virtual EntityTypeBuilder UsingEntity(
        string joinEntityName,
        Type joinEntityType,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureRight,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureLeft,
        Action<EntityTypeBuilder> configureJoinEntityType)
    {
        Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));

        configureJoinEntityType(UsingEntity(joinEntityName, joinEntityType, configureRight, configureLeft));

        return new EntityTypeBuilder(RightEntityType);
    }

    private EntityTypeBuilder Using(
        string? joinEntityName,
        Type? joinEntityType,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder>? configureRight,
        Func<EntityTypeBuilder, ReferenceCollectionBuilder>? configureLeft)
        => new(
            UsingEntity(
                joinEntityName,
                joinEntityType,
                configureRight != null
                    ? e => configureRight(new EntityTypeBuilder(e)).Metadata
                    : null,
                configureLeft != null
                    ? e => configureLeft(new EntityTypeBuilder(e)).Metadata
                    : null));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual IMutableEntityType UsingEntity(
        string? joinEntityName,
        Type? joinEntityType,
        Func<IMutableEntityType, IMutableForeignKey>? configureRight,
        Func<IMutableEntityType, IMutableForeignKey>? configureLeft)
    {
        using var _ = LeftEntityType.Model.DelayConventions();

        var existingJoinEntityType = (EntityType?)(LeftNavigation.JoinEntityType ?? RightNavigation.JoinEntityType);
        EntityType? newJoinEntityType = null;
        EntityType.Snapshot? entityTypeSnapshot = null;
        if (existingJoinEntityType != null)
        {
            if ((joinEntityType == null || existingJoinEntityType.ClrType == joinEntityType)
                && (joinEntityName == null || string.Equals(existingJoinEntityType.Name, joinEntityName, StringComparison.Ordinal)))
            {
                newJoinEntityType = existingJoinEntityType;
            }
            else
            {
                ModelBuilder.RemoveImplicitJoinEntity(existingJoinEntityType, configurationSource: ConfigurationSource.DataAnnotation);

                entityTypeSnapshot = InternalEntityTypeBuilder.DetachAllMembers(existingJoinEntityType);
                if (entityTypeSnapshot != null)
                {
                    ModelBuilder.HasNoEntityType(existingJoinEntityType, ConfigurationSource.Explicit);
                }
            }
        }

        if (newJoinEntityType == null)
        {
            var existingEntityType = joinEntityName == null
                ? ModelBuilder.Metadata.FindEntityType(joinEntityType!)
                : ModelBuilder.Metadata.FindEntityType(joinEntityName);
            if (existingEntityType != null
                && (joinEntityType == null || existingEntityType.ClrType == joinEntityType))
            {
                newJoinEntityType = existingEntityType;
            }
            else
            {
                joinEntityType ??= Model.DefaultPropertyBagType;

                newJoinEntityType = joinEntityName == null
                    ? ModelBuilder.Entity(joinEntityType, ConfigurationSource.Explicit, shouldBeOwned: false)!.Metadata
                    : ModelBuilder.SharedTypeEntity(joinEntityName, joinEntityType, ConfigurationSource.Explicit)!.Metadata;
            }
        }

        if (entityTypeSnapshot != null)
        {
            entityTypeSnapshot.Attach(newJoinEntityType.Builder);
        }

        IMutableForeignKey? rightForeignKey;
        if (configureRight != null)
        {
            newJoinEntityType.SetAnnotation(CoreAnnotationNames.SkipNavigationBeingConfigured, RightNavigation);
            rightForeignKey = configureRight(newJoinEntityType);
            newJoinEntityType.RemoveAnnotation(CoreAnnotationNames.SkipNavigationBeingConfigured);
        }
        else
        {
            rightForeignKey = GetOrCreateSkipNavigationForeignKey((SkipNavigation)RightNavigation, newJoinEntityType);
        }
        ((SkipNavigation)RightNavigation).Builder
            .HasForeignKey((ForeignKey)rightForeignKey, ConfigurationSource.Explicit);

        IMutableForeignKey? leftForeignKey;
        if (configureLeft != null)
        {
            newJoinEntityType.SetAnnotation(CoreAnnotationNames.SkipNavigationBeingConfigured, LeftNavigation);
            leftForeignKey = configureLeft(newJoinEntityType);
            newJoinEntityType.RemoveAnnotation(CoreAnnotationNames.SkipNavigationBeingConfigured);
        }
        else
        {
            leftForeignKey = GetOrCreateSkipNavigationForeignKey((SkipNavigation)LeftNavigation, newJoinEntityType);
        }
        ((SkipNavigation)LeftNavigation).Builder
            .HasForeignKey((ForeignKey)leftForeignKey, ConfigurationSource.Explicit);

        return newJoinEntityType;

        static ForeignKey GetOrCreateSkipNavigationForeignKey(
            SkipNavigation skipNavigation,
            EntityType joinEntityType)
        {
            ForeignKey? compatibleFk = null;
            foreach (var fk in joinEntityType.GetDeclaredForeignKeys())
            {
                if (fk.PrincipalEntityType != skipNavigation.DeclaringEntityType
                    || fk.IsUnique)
                {
                    continue;
                }

                if (compatibleFk != null)
                {
                    compatibleFk = null;
                    break;
                }

                compatibleFk = fk;
            }

            if (compatibleFk != null)
            {
                return compatibleFk;
            }

            return joinEntityType
                .Builder
                .HasRelationship(
                    skipNavigation.DeclaringEntityType,
                    ConfigurationSource.Convention,
                    required: true,
                    skipNavigation.Inverse!.Name)!
                .IsUnique(false, ConfigurationSource.Convention)!
                .Metadata;
        }
    }

    #region Hidden System.Object members

    /// <inheritdoc />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString()
        => base.ToString();

    /// <inheritdoc />
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once BaseObjectEqualsIsObjectEquals
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <inheritdoc />
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}
