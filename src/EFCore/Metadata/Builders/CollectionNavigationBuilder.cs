// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     <para>
///         Provides a simple API for configuring a relationship where configuration began on
///         an end of the relationship with a collection that contains instances of another entity type.
///     </para>
///     <para>
///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///         and it is not designed to be directly constructed in your application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class CollectionNavigationBuilder : IInfrastructure<IConventionForeignKeyBuilder?>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public CollectionNavigationBuilder(
        IMutableEntityType declaringEntityType,
        IMutableEntityType relatedEntityType,
        MemberIdentity navigation,
        IMutableForeignKey? foreignKey,
        IMutableSkipNavigation? skipNavigation)
    {
        DeclaringEntityType = declaringEntityType;
        RelatedEntityType = relatedEntityType;
        CollectionMember = navigation.MemberInfo;
        CollectionName = navigation.Name;
        Builder = ((ForeignKey?)foreignKey)?.Builder;
        SkipNavigation = skipNavigation;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalForeignKeyBuilder? Builder { get; private set; }

    private IMutableSkipNavigation? SkipNavigation { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual string? CollectionName { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual MemberInfo? CollectionMember { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual IMutableEntityType RelatedEntityType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual IMutableEntityType DeclaringEntityType { get; }

    /// <summary>
    ///     <para>
    ///         Gets the internal builder being used to configure the relationship.
    ///     </para>
    ///     <para>
    ///         This property is intended for use by extension methods that need to make use of services
    ///         not directly exposed in the public API surface.
    ///     </para>
    /// </summary>
    IConventionForeignKeyBuilder? IInfrastructure<IConventionForeignKeyBuilder?>.Instance
        => Builder;

    /// <summary>
    ///     Configures this as a one-to-many relationship.
    /// </summary>
    /// <remarks>
    ///     Note that calling this method with no parameters will explicitly configure this side
    ///     of the relationship to use no navigation property, even if such a property exists on the
    ///     entity type. If the navigation property is to be used, then it must be specified.
    /// </remarks>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on the other end of this relationship.
    ///     If null or not specified, then there is no navigation property on the other end of the relationship.
    /// </param>
    /// <returns>An object to further configure the relationship.</returns>
    public virtual ReferenceCollectionBuilder WithOne(string? navigationName = null)
        => new(
            DeclaringEntityType,
            RelatedEntityType,
            WithOneBuilder(
                Check.NullButNotEmpty(navigationName, nameof(navigationName))).Metadata);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalForeignKeyBuilder WithOneBuilder(string? navigationName)
        => WithOneBuilder(MemberIdentity.Create(navigationName));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalForeignKeyBuilder WithOneBuilder(
        MemberInfo? navigationMemberInfo)
        => WithOneBuilder(MemberIdentity.Create(navigationMemberInfo));

    private InternalForeignKeyBuilder WithOneBuilder(MemberIdentity reference)
    {
        if (SkipNavigation != null)
        {
            // Note: we delayed setting the ConfigurationSource of SkipNavigation in HasMany()
            // so we can test it here and override if the skip navigation was originally found
            // by convention.
            if (((IConventionSkipNavigation)SkipNavigation).GetConfigurationSource() == ConfigurationSource.Explicit)
            {
                throw new InvalidOperationException(
                    CoreStrings.ConflictingRelationshipNavigation(
                        SkipNavigation.DeclaringEntityType.DisplayName() + "." + SkipNavigation.Name,
                        RelatedEntityType.DisplayName()
                        + (reference.Name == null
                            ? ""
                            : "." + reference.Name),
                        SkipNavigation.DeclaringEntityType.DisplayName() + "." + SkipNavigation.Name,
                        SkipNavigation.TargetEntityType.DisplayName()
                        + (SkipNavigation.Inverse == null
                            ? ""
                            : "." + SkipNavigation.Inverse.Name)));
            }

            var navigationName = SkipNavigation.Name;
            var declaringEntityType = (EntityType)DeclaringEntityType;

            if (SkipNavigation.Inverse != null)
            {
                ((EntityType)SkipNavigation.Inverse.DeclaringEntityType).Builder.HasNoSkipNavigation(
                    (SkipNavigation)SkipNavigation.Inverse, ConfigurationSource.Explicit);
            }

            declaringEntityType.Builder.HasNoSkipNavigation((SkipNavigation)SkipNavigation, ConfigurationSource.Explicit);

            Builder = declaringEntityType.Builder
                .HasRelationship(
                    (EntityType)RelatedEntityType,
                    navigationName,
                    ConfigurationSource.Explicit,
                    targetIsPrincipal: false);
            SkipNavigation = null;
        }

        var foreignKey = Builder!.Metadata;
        var referenceName = reference.Name;
        if (referenceName != null
            && foreignKey.DependentToPrincipal != null
            && foreignKey.GetDependentToPrincipalConfigurationSource() == ConfigurationSource.Explicit
            && foreignKey.DependentToPrincipal.Name != referenceName)
        {
            InternalForeignKeyBuilder.ThrowForConflictingNavigation(foreignKey, referenceName, newToPrincipal: true);
        }

        return reference.MemberInfo == null || CollectionMember == null
            ? Builder.HasNavigations(
                reference.Name, CollectionName,
                (EntityType)DeclaringEntityType, (EntityType)RelatedEntityType,
                ConfigurationSource.Explicit)!
            : Builder.HasNavigations(
                reference.MemberInfo, CollectionMember,
                (EntityType)DeclaringEntityType, (EntityType)RelatedEntityType,
                ConfigurationSource.Explicit)!;
    }

    /// <summary>
    ///     Configures this as a many-to-many relationship.
    /// </summary>
    /// <param name="navigationName">
    ///     The name of the collection navigation property on the other end of this relationship.
    /// </param>
    /// <returns>An object to further configure the relationship.</returns>
    public virtual CollectionCollectionBuilder WithMany(string? navigationName = null)
    {
        var leftName = Builder?.Metadata.PrincipalToDependent?.Name;
        var collectionCollectionBuilder =
            new CollectionCollectionBuilder(
                RelatedEntityType,
                DeclaringEntityType,
                WithLeftManyNavigation(navigationName),
                WithRightManyNavigation(navigationName, leftName!));

        return collectionCollectionBuilder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual IMutableSkipNavigation WithLeftManyNavigation(MemberInfo inverseMemberInfo)
        => WithLeftManyNavigation(inverseMemberInfo.Name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual IMutableSkipNavigation WithLeftManyNavigation(string? inverseName)
    {
        Check.NullButNotEmpty(inverseName, nameof(inverseName));

        if (SkipNavigation != null)
        {
            return SkipNavigation;
        }

        var foreignKey = Builder!.Metadata;
        var navigationMember = foreignKey.PrincipalToDependent.CreateMemberIdentity();
        if (foreignKey.GetDependentToPrincipalConfigurationSource() == ConfigurationSource.Explicit)
        {
            InternalForeignKeyBuilder.ThrowForConflictingNavigation(
                foreignKey, DeclaringEntityType, RelatedEntityType, navigationMember.Name, inverseName);
        }

        using (foreignKey.DeclaringEntityType.Model.DelayConventions())
        {
            foreignKey.DeclaringEntityType.Builder.HasNoRelationship(foreignKey, ConfigurationSource.Explicit);
            Builder = null;
            return ((EntityType)DeclaringEntityType).Builder.HasSkipNavigation(
                navigationMember,
                (EntityType)RelatedEntityType,
                foreignKey.PrincipalToDependent?.ClrType,
                ConfigurationSource.Explicit,
                collection: true,
                onDependent: false)!.Metadata;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual IMutableSkipNavigation WithRightManyNavigation(string? navigationName, string? inverseName)
        => WithRightManyNavigation(MemberIdentity.Create(navigationName), inverseName);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual IMutableSkipNavigation WithRightManyNavigation(
        MemberInfo navigationMemberInfo,
        string? inverseName)
        => WithRightManyNavigation(MemberIdentity.Create(navigationMemberInfo), inverseName);

    private IMutableSkipNavigation WithRightManyNavigation(MemberIdentity navigationMember, string? inverseName)
    {
        Check.DebugAssert(Builder == null, "Expected no associated foreign key at this point");

        var navigationName = navigationMember.Name;
        using (((EntityType)RelatedEntityType).Model.DelayConventions())
        {
            if (navigationName != null)
            {
                var conflictingNavigation = RelatedEntityType.FindNavigation(navigationName) as IConventionNavigation;
                var foreignKey = (ForeignKey?)conflictingNavigation?.ForeignKey;
                if (conflictingNavigation?.GetConfigurationSource() == ConfigurationSource.Explicit)
                {
                    InternalForeignKeyBuilder.ThrowForConflictingNavigation(
                        foreignKey!, DeclaringEntityType, RelatedEntityType, inverseName, navigationName);
                }

                if (conflictingNavigation != null)
                {
                    foreignKey!.DeclaringEntityType.Builder.HasNoRelationship(foreignKey, ConfigurationSource.Explicit);
                }
                else
                {
                    var skipNavigation = RelatedEntityType.FindSkipNavigation(navigationName);
                    if (skipNavigation != null)
                    {
                        ((SkipNavigation)skipNavigation).UpdateConfigurationSource(ConfigurationSource.Explicit);
                        return skipNavigation;
                    }
                }
            }

            return ((EntityType)RelatedEntityType).Builder.HasSkipNavigation(
                navigationMember,
                (EntityType)DeclaringEntityType,
                ConfigurationSource.Explicit,
                collection: true,
                onDependent: false)!.Metadata;
        }
    }

    #region Hidden System.Object members

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString()
        => base.ToString();

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once BaseObjectEqualsIsObjectEquals
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}
