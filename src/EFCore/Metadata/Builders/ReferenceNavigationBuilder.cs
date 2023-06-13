// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a relationship where configuration began on
///     an end of the relationship with a reference that points to an instance of another entity type.
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
public class ReferenceNavigationBuilder : IInfrastructure<IConventionForeignKeyBuilder>
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
    {
        DeclaringEntityType = declaringEntityType;
        RelatedEntityType = relatedEntityType;
        ReferenceName = navigationName;
        Builder = ((ForeignKey)foreignKey).Builder;
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
    {
        DeclaringEntityType = declaringEntityType;
        RelatedEntityType = relatedEntityType;
        ReferenceMember = navigationMemberInfo;
        ReferenceName = navigationMemberInfo?.GetSimpleMemberName();
        Builder = ((ForeignKey)foreignKey).Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalForeignKeyBuilder Builder { [DebuggerStepThrough] get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual string? ReferenceName { [DebuggerStepThrough] get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual MemberInfo? ReferenceMember { [DebuggerStepThrough] get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual IMutableEntityType RelatedEntityType { [DebuggerStepThrough] get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual IMutableEntityType DeclaringEntityType { [DebuggerStepThrough] get; }

    /// <summary>
    ///     Gets the internal builder being used to configure the relationship.
    /// </summary>
    IConventionForeignKeyBuilder IInfrastructure<IConventionForeignKeyBuilder>.Instance
        => Builder;

    /// <summary>
    ///     Configures this as a one-to-many relationship.
    /// </summary>
    /// <remarks>
    ///     Note that calling this method with no parameters will explicitly configure this side
    ///     of the relationship to use no navigation property, even if such a property exists on the
    ///     entity type. If the navigation property is to be used, then it must be specified.
    /// </remarks>
    /// <param name="collection">
    ///     The name of the collection navigation property on the other end of this relationship.
    ///     If null or not specified, there is no navigation property on the other end of the relationship.
    /// </param>
    /// <returns>An object to further configure the relationship.</returns>
    public virtual ReferenceCollectionBuilder WithMany(string? collection = null)
        => new(
            RelatedEntityType,
            DeclaringEntityType,
            WithManyBuilder(Check.NullButNotEmpty(collection, nameof(collection))).Metadata);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalForeignKeyBuilder WithManyBuilder(string? navigationName)
        => WithManyBuilder(MemberIdentity.Create(navigationName));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalForeignKeyBuilder WithManyBuilder(MemberInfo? navigationMemberInfo)
        => WithManyBuilder(MemberIdentity.Create(navigationMemberInfo));

    private InternalForeignKeyBuilder WithManyBuilder(MemberIdentity collection)
    {
        var builder = Builder.HasEntityTypes(
            (EntityType)RelatedEntityType, (EntityType)DeclaringEntityType, ConfigurationSource.Explicit)!;
        var collectionName = collection.Name;
        if (builder.Metadata is { IsUnique: true, PrincipalToDependent: not null }
            && builder.Metadata.GetPrincipalToDependentConfigurationSource() == ConfigurationSource.Explicit
            && collectionName != null)
        {
            InternalForeignKeyBuilder.ThrowForConflictingNavigation(builder.Metadata, collectionName, false);
        }

        builder = builder.IsUnique(false, ConfigurationSource.Explicit)!;
        var foreignKey = builder.Metadata;
        if (collectionName != null
            && foreignKey.PrincipalToDependent != null
            && foreignKey.GetPrincipalToDependentConfigurationSource() == ConfigurationSource.Explicit
            && foreignKey.PrincipalToDependent.Name != collectionName)
        {
            InternalForeignKeyBuilder.ThrowForConflictingNavigation(foreignKey, collectionName, false);
        }

        return collection.MemberInfo == null || ReferenceMember == null
            ? builder.HasNavigations(
                ReferenceName, collection.Name,
                (EntityType)RelatedEntityType, (EntityType)DeclaringEntityType, ConfigurationSource.Explicit)!
            : builder.HasNavigations(
                ReferenceMember, collection.MemberInfo,
                (EntityType)RelatedEntityType, (EntityType)DeclaringEntityType, ConfigurationSource.Explicit)!;
    }

    /// <summary>
    ///     Configures this as a one-to-one relationship.
    /// </summary>
    /// <remarks>
    ///     Note that calling this method with no parameters will explicitly configure this side
    ///     of the relationship to use no navigation property, even if such a property exists on the
    ///     entity type. If the navigation property is to be used, then it must be specified.
    /// </remarks>
    /// <param name="reference">
    ///     The name of the reference navigation property on the other end of this relationship.
    ///     If null or not specified, there is no navigation property on the other end of the relationship.
    /// </param>
    /// <returns>An object that can be used to configure the relationship.</returns>
    public virtual ReferenceReferenceBuilder WithOne(string? reference = null)
        => new(DeclaringEntityType, RelatedEntityType, WithOneBuilder(Check.NullButNotEmpty(reference, nameof(reference))).Metadata);

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
    protected virtual InternalForeignKeyBuilder WithOneBuilder(MemberInfo? navigationMemberInfo)
        => WithOneBuilder(MemberIdentity.Create(navigationMemberInfo));

    private InternalForeignKeyBuilder WithOneBuilder(MemberIdentity reference)
    {
        var referenceName = reference.Name;
        if (!Builder.Metadata.IsUnique
            && Builder.Metadata.PrincipalToDependent != null
            && Builder.Metadata.GetPrincipalToDependentConfigurationSource() == ConfigurationSource.Explicit
            && referenceName != null)
        {
            InternalForeignKeyBuilder.ThrowForConflictingNavigation(Builder.Metadata, referenceName, false);
        }

        using var batch = Builder.Metadata.DeclaringEntityType.Model.DelayConventions();
        var builder = Builder.IsUnique(true, ConfigurationSource.Explicit)!;
        var foreignKey = builder.Metadata;
        if (foreignKey.IsSelfReferencing()
            && referenceName != null
            && ReferenceName == referenceName)
        {
            throw new InvalidOperationException(
                CoreStrings.ConflictingPropertyOrNavigation(
                    referenceName, RelatedEntityType.DisplayName(), RelatedEntityType.DisplayName()));
        }

        var pointsToPrincipal = !foreignKey.IsSelfReferencing()
            && (!foreignKey.DeclaringEntityType.IsAssignableFrom(DeclaringEntityType)
                || !foreignKey.PrincipalEntityType.IsAssignableFrom(RelatedEntityType)
                || (foreignKey.DeclaringEntityType.IsAssignableFrom(RelatedEntityType)
                    && foreignKey.PrincipalEntityType.IsAssignableFrom(DeclaringEntityType)
                    && foreignKey.PrincipalToDependent != null
                    && foreignKey.PrincipalToDependent.Name == ReferenceName));

        if (referenceName != null
            && ((pointsToPrincipal
                    && foreignKey.DependentToPrincipal != null
                    && foreignKey.GetDependentToPrincipalConfigurationSource() == ConfigurationSource.Explicit
                    && foreignKey.DependentToPrincipal.Name != referenceName)
                || (!pointsToPrincipal
                    && foreignKey.PrincipalToDependent != null
                    && foreignKey.GetPrincipalToDependentConfigurationSource() == ConfigurationSource.Explicit
                    && foreignKey.PrincipalToDependent.Name != referenceName)))
        {
            InternalForeignKeyBuilder.ThrowForConflictingNavigation(foreignKey, referenceName, pointsToPrincipal);
        }

        var referenceProperty = reference.MemberInfo;
        if (pointsToPrincipal)
        {
            builder = referenceProperty == null || ReferenceMember == null
                ? builder.HasNavigations(
                    referenceName, ReferenceName,
                    (EntityType)DeclaringEntityType, (EntityType)RelatedEntityType, ConfigurationSource.Explicit)!
                : builder.HasNavigations(
                    referenceProperty, ReferenceMember,
                    (EntityType)DeclaringEntityType, (EntityType)RelatedEntityType, ConfigurationSource.Explicit)!;
        }
        else
        {
            builder = referenceProperty == null || ReferenceMember == null
                ? builder.HasNavigations(
                    ReferenceName, referenceName,
                    (EntityType)RelatedEntityType, (EntityType)DeclaringEntityType, ConfigurationSource.Explicit)!
                : builder.HasNavigations(
                    ReferenceMember, referenceProperty,
                    (EntityType)RelatedEntityType, (EntityType)DeclaringEntityType, ConfigurationSource.Explicit)!;
        }

        return batch.Run(builder)!;
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
