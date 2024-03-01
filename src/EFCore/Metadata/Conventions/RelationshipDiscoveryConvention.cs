// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures relationships between entity types based on the navigation properties
///     as long as there is no ambiguity as to which is the corresponding inverse navigation.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class RelationshipDiscoveryConvention :
    ITypeIgnoredConvention,
    IEntityTypeAddedConvention,
    IEntityTypeBaseTypeChangedConvention,
    IEntityTypeMemberIgnoredConvention,
    INavigationRemovedConvention,
    INavigationAddedConvention,
    IForeignKeyOwnershipChangedConvention,
    IForeignKeyNullNavigationSetConvention,
    IForeignKeyRemovedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="RelationshipDiscoveryConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="useAttributes">Whether the convention will use attributes found on the members.</param>
    public RelationshipDiscoveryConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        bool useAttributes = true)
    {
        Dependencies = dependencies;
        UseAttributes = useAttributes;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     A value indicating whether the convention will use attributes found on the members.
    /// </summary>
    protected virtual bool UseAttributes { get; }

    /// <summary>
    ///     Discovers the relationships for the given entity type.
    /// </summary>
    /// <param name="entityTypeBuilder">The entity type builder.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    /// <param name="discoverUnmatchedInverses">Whether to discover unmatched inverse navigations.</param>
    protected virtual void DiscoverRelationships(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext context,
        bool discoverUnmatchedInverses = false)
    {
        var unmatchedInverseCandidates = discoverUnmatchedInverses
            ? Dependencies.MemberClassifier.GetInverseCandidateTypes(entityTypeBuilder.Metadata, UseAttributes).ToList()
            : null;
        var relationshipCandidates = FindRelationshipCandidates(entityTypeBuilder, unmatchedInverseCandidates);
        relationshipCandidates = RemoveIncompatibleWithExistingRelationships(relationshipCandidates, entityTypeBuilder);
        relationshipCandidates = RemoveInheritedInverseNavigations(relationshipCandidates);
        relationshipCandidates = RemoveSingleSidedBaseNavigations(relationshipCandidates, entityTypeBuilder);

        CreateRelationships(relationshipCandidates, entityTypeBuilder);

        DiscoverUnidirectionalInverses(entityTypeBuilder, context, unmatchedInverseCandidates);
    }

    private IReadOnlyList<RelationshipCandidate> FindRelationshipCandidates(
        IConventionEntityTypeBuilder entityTypeBuilder,
        List<Type>? otherInverseCandidateTypes)
    {
        var entityType = entityTypeBuilder.Metadata;
        var relationshipCandidates = new Dictionary<IConventionEntityType, RelationshipCandidate>();
        var ownership = entityType.FindOwnership();
        if (ownership == null
            && entityType.IsOwned())
        {
            // Handled when the ownership is actually added
            return relationshipCandidates.Values.ToList();
        }

        foreach (var candidateTuple in Dependencies.MemberClassifier.GetNavigationCandidates(entityType, UseAttributes))
        {
            var navigationPropertyInfo = candidateTuple.Key;
            var (targetClrType, shouldBeOwned) = candidateTuple.Value;

            if (entityType.FindNavigation(navigationPropertyInfo) == null
                && entityType.FindSkipNavigation(navigationPropertyInfo) == null
                && (!IsCandidateNavigationProperty(entityType, navigationPropertyInfo.GetSimpleMemberName(), navigationPropertyInfo)
                    || IsNewSharedType(targetClrType, entityType)))
            {
                continue;
            }

            if (((Model)entityType.Model).FindIsComplexConfigurationSource(targetClrType) != null)
            {
                continue;
            }

            var candidateTargetEntityTypeBuilder = TryGetTargetEntityTypeBuilder(
                entityTypeBuilder, targetClrType, navigationPropertyInfo, shouldBeOwned);
            if (candidateTargetEntityTypeBuilder == null)
            {
                continue;
            }

            var candidateTargetEntityType = candidateTargetEntityTypeBuilder.Metadata;
            if (!entityType.IsInModel)
            {
                // Current entity type was removed while the target entity type was being added
                relationshipCandidates[candidateTargetEntityType] =
                    new RelationshipCandidate(
                        candidateTargetEntityTypeBuilder, [], [], false);
                break;
            }

            if (candidateTargetEntityType.IsKeyless
                || (candidateTargetEntityType.IsOwned()
                    && (HasDeclaredAmbiguousNavigationsTo(entityType, targetClrType)
                        || entityType.IsKeyless)))
            {
                relationshipCandidates[candidateTargetEntityType] =
                    new RelationshipCandidate(
                        candidateTargetEntityTypeBuilder, [], [], false);
                continue;
            }

            Check.DebugAssert(
                entityType.ClrType != targetClrType
                || !candidateTargetEntityType.IsOwned()
                || candidateTargetEntityType.FindOwnership()?.PrincipalToDependent?.Name
                == navigationPropertyInfo.GetSimpleMemberName(),
                "Self-referencing ownerships shouldn't be discovered");

            var targetOwnership = candidateTargetEntityType.FindOwnership();
            var shouldBeOwnership = candidateTargetEntityType.IsOwned()
                && (targetOwnership == null
                    || (targetOwnership.PrincipalEntityType == entityType
                        && targetOwnership.PrincipalToDependent?.Name == navigationPropertyInfo.GetSimpleMemberName()))
                && (ownership == null
                    || !entityType.IsInOwnershipPath(candidateTargetEntityType));

            if (!candidateTargetEntityType.HasSharedClrType)
            {
                otherInverseCandidateTypes?.Remove(targetClrType);
            }

            if (candidateTargetEntityType.IsOwned()
                && !shouldBeOwnership
                && (targetOwnership?.PrincipalEntityType == entityType
                    || !candidateTargetEntityType.IsInOwnershipPath(entityType))
                && (ownership == null
                    || !entityType.IsInOwnershipPath(candidateTargetEntityType)))
            {
                // Only the owner or nested ownees can have navigations to an owned type
                // Also skip non-ownership navigations from the owner
                relationshipCandidates[candidateTargetEntityType] =
                    new RelationshipCandidate(
                        candidateTargetEntityTypeBuilder, [], [], false);
                continue;
            }

            if (!shouldBeOwnership
                && ownership != null
                && navigationPropertyInfo.PropertyType != targetClrType)
            {
                // Don't try to configure a collection on an owned type unless it represents a sub-ownership
                relationshipCandidates[candidateTargetEntityType] =
                    new RelationshipCandidate(
                        candidateTargetEntityTypeBuilder, [], [], false);
                continue;
            }

            if (relationshipCandidates.TryGetValue(candidateTargetEntityType, out var existingCandidate))
            {
                if (!existingCandidate.IsOwnership
                    && !shouldBeOwnership)
                {
                    if (candidateTargetEntityType != entityType
                        || !existingCandidate.InverseProperties.Contains(navigationPropertyInfo))
                    {
                        if (!existingCandidate.NavigationProperties.Contains(navigationPropertyInfo))
                        {
                            existingCandidate.NavigationProperties.Add(navigationPropertyInfo);
                        }
                    }

                    continue;
                }

                var sharedTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(
                    targetClrType, navigationPropertyInfo.GetSimpleMemberName(), entityType);
                if (sharedTypeBuilder == null)
                {
                    continue;
                }

                candidateTargetEntityType = sharedTypeBuilder.Metadata;
            }

            var navigations = new List<PropertyInfo> { navigationPropertyInfo };
            var inverseNavigationCandidates = new List<PropertyInfo>();

            if (!entityType.IsKeyless)
            {
                var inverseCandidates = Dependencies.MemberClassifier.GetNavigationCandidates(candidateTargetEntityType, UseAttributes);
                foreach (var (inversePropertyInfo, value) in inverseCandidates)
                {
                    if (navigationPropertyInfo.IsSameAs(inversePropertyInfo)
                        || (candidateTargetEntityType.FindNavigation(inversePropertyInfo) == null
                            && candidateTargetEntityType.FindSkipNavigation(inversePropertyInfo) == null
                            && !IsCandidateNavigationProperty(
                                candidateTargetEntityType, inversePropertyInfo.GetSimpleMemberName(), inversePropertyInfo)))
                    {
                        continue;
                    }

                    var inverseTargetType = value.Type;
                    var inverseIsCollection = inverseTargetType != inversePropertyInfo.PropertyType;
                    if (inverseTargetType != entityType.ClrType
                        && (!inverseTargetType.IsAssignableFrom(entityType.ClrType)
                            || inverseIsCollection
                            || (!shouldBeOwnership
                                && !candidateTargetEntityType.IsInOwnershipPath(entityType))))
                    {
                        // Only use inverse of a base type if the target is owned by the current entity type,
                        // unless it's a collection
                        continue;
                    }

                    if (ownership != null
                        && !shouldBeOwnership
                        && !candidateTargetEntityType.IsInOwnershipPath(entityType)
                        && (ownership.PrincipalEntityType == candidateTargetEntityType
                            || !entityType.IsInOwnershipPath(candidateTargetEntityType))
                        && (ownership.PrincipalEntityType != candidateTargetEntityType
                            || ownership.PrincipalToDependent?.Name != inversePropertyInfo.GetSimpleMemberName()))
                    {
                        // Only the owner or nested ownees can have navigations to an owned type
                        // Also skip non-ownership inverse candidates from the owner
                        continue;
                    }

                    if (shouldBeOwnership
                        && inverseIsCollection
                        && navigations.Count == 1)
                    {
                        // Target type should be the principal, discover the relationship from the other side
                        if (candidateTargetEntityType.IsInModel
                            && IsImplicitlyCreatedUnusedType(candidateTargetEntityType))
                        {
                            candidateTargetEntityType.Builder.ModelBuilder.HasNoEntityType(candidateTargetEntityType);
                        }

                        goto Continue;
                    }

                    if (!inverseNavigationCandidates.Contains(inversePropertyInfo))
                    {
                        inverseNavigationCandidates.Add(inversePropertyInfo);
                    }
                }
            }

            relationshipCandidates[candidateTargetEntityType] =
                new RelationshipCandidate(
                    candidateTargetEntityTypeBuilder, navigations, inverseNavigationCandidates, shouldBeOwnership);

            Continue: ;
        }

        return UpdateTargetEntityTypes(entityTypeBuilder, relationshipCandidates);

        bool IsNewSharedType(Type targetClrType, IConventionEntityType entityType)
            => (entityType.Model.IsShared(targetClrType)
                    || targetClrType.IsGenericType
                    && targetClrType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                && ShouldBeOwned(targetClrType, entityType.Model) != true
                && !entityType.Model.IsOwned(targetClrType)
                && !entityType.IsInOwnershipPath(targetClrType);
    }

    private List<RelationshipCandidate> UpdateTargetEntityTypes(
        IConventionEntityTypeBuilder entityTypeBuilder,
        Dictionary<IConventionEntityType, RelationshipCandidate> relationshipCandidates)
    {
        var candidates = new List<RelationshipCandidate>();
        foreach (var relationshipCandidate in relationshipCandidates.Values)
        {
            var targetType = relationshipCandidate.TargetTypeBuilder.Metadata;
            if (!entityTypeBuilder.Metadata.IsInModel
                || relationshipCandidate.NavigationProperties.Count == 0)
            {
                if (IsImplicitlyCreatedUnusedType(targetType))
                {
                    targetType.Builder.ModelBuilder.HasNoEntityType(targetType);
                }

                continue;
            }

            if (targetType.IsInModel)
            {
                candidates.Add(relationshipCandidate);
                continue;
            }

            if (relationshipCandidate.NavigationProperties.Count > 1)
            {
                continue;
            }

            // The entity type might have been converted to a shared type entity type
            var actualTargetEntityTypeBuilder = TryGetTargetEntityTypeBuilder(
                entityTypeBuilder,
                relationshipCandidate.TargetTypeBuilder.Metadata.ClrType,
                relationshipCandidate.NavigationProperties.Single());

            if (actualTargetEntityTypeBuilder == null)
            {
                continue;
            }

            candidates.Add(
                new RelationshipCandidate(
                    actualTargetEntityTypeBuilder,
                    relationshipCandidate.NavigationProperties,
                    relationshipCandidate.InverseProperties,
                    relationshipCandidate.IsOwnership));
        }

        return candidates;
    }

    /// <summary>
    ///     Finds or tries to create an entity type target for the given navigation member.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the referencing entity type.</param>
    /// <param name="targetClrType">The CLR type of the target entity type.</param>
    /// <param name="navigationMemberInfo">The navigation member.</param>
    /// <param name="shouldBeOwned">Whether the target entity type should be owned.</param>
    /// <param name="shouldCreate">Whether an entity type should be created if one doesn't currently exist.</param>
    /// <returns>The builder for the target entity type or <see langword="null" /> if it can't be created.</returns>
    protected virtual IConventionEntityTypeBuilder? TryGetTargetEntityTypeBuilder(
        IConventionEntityTypeBuilder entityTypeBuilder,
        Type targetClrType,
        MemberInfo navigationMemberInfo,
        bool? shouldBeOwned = null,
        bool shouldCreate = true)
    {
        if (shouldCreate)
        {
            var targetEntityTypeBuilder = entityTypeBuilder
                .GetTargetEntityTypeBuilder(
                    targetClrType,
                    navigationMemberInfo,
                    createIfMissing: true,
                    shouldBeOwned ?? ShouldBeOwned(targetClrType, entityTypeBuilder.Metadata.Model));
            if (targetEntityTypeBuilder != null)
            {
                return targetEntityTypeBuilder;
            }
        }

        return entityTypeBuilder
            .GetTargetEntityTypeBuilder(targetClrType, navigationMemberInfo, createIfMissing: false, shouldBeOwned);
    }

    private static IReadOnlyList<RelationshipCandidate> RemoveIncompatibleWithExistingRelationships(
        IReadOnlyList<RelationshipCandidate> relationshipCandidates,
        IConventionEntityTypeBuilder entityTypeBuilder)
    {
        if (relationshipCandidates.Count == 0)
        {
            return relationshipCandidates;
        }

        var entityType = entityTypeBuilder.Metadata;
        var filteredRelationshipCandidates = new List<RelationshipCandidate>();
        foreach (var relationshipCandidate in relationshipCandidates)
        {
            var targetEntityTypeBuilder = relationshipCandidate.TargetTypeBuilder;
            var targetEntityType = targetEntityTypeBuilder.Metadata;
            while (relationshipCandidate.NavigationProperties.Count > 0)
            {
                var navigationProperty = relationshipCandidate.NavigationProperties[0];
                var navigationPropertyName = navigationProperty.GetSimpleMemberName();
                var existingNavigation = entityType.FindNavigation(navigationPropertyName);
                if (existingNavigation != null)
                {
                    if (existingNavigation.DeclaringEntityType != entityType
                        || existingNavigation.TargetEntityType != targetEntityType)
                    {
                        relationshipCandidate.NavigationProperties.Remove(navigationProperty);
                        continue;
                    }
                }
                else
                {
                    var existingSkipNavigation = entityType.FindSkipNavigation(navigationPropertyName);
                    if (existingSkipNavigation != null
                        && (existingSkipNavigation.DeclaringEntityType != entityType
                            || existingSkipNavigation.TargetEntityType != targetEntityType))
                    {
                        relationshipCandidate.NavigationProperties.Remove(navigationProperty);
                        continue;
                    }
                }

                if (relationshipCandidate.NavigationProperties.Count == 1
                    && relationshipCandidate.InverseProperties.Count == 0)
                {
                    break;
                }

                PropertyInfo? compatibleInverse = null;
                foreach (var inverseProperty in relationshipCandidate.InverseProperties)
                {
                    if (AreCompatible(
                            navigationProperty, inverseProperty, entityTypeBuilder, targetEntityTypeBuilder))
                    {
                        if (compatibleInverse == null)
                        {
                            compatibleInverse = inverseProperty;
                        }
                        else
                        {
                            goto NextCandidate;
                        }
                    }
                }

                if (compatibleInverse == null)
                {
                    relationshipCandidate.NavigationProperties.Remove(navigationProperty);

                    filteredRelationshipCandidates.Add(
                        new RelationshipCandidate(
                            targetEntityTypeBuilder,
                            [navigationProperty],
                            [],
                            relationshipCandidate.IsOwnership));

                    if (relationshipCandidate.TargetTypeBuilder.Metadata == entityTypeBuilder.Metadata
                        && relationshipCandidate.InverseProperties.Count > 0)
                    {
                        var nextSelfRefCandidate = relationshipCandidate.InverseProperties.First();
                        if (!relationshipCandidate.NavigationProperties.Contains(nextSelfRefCandidate))
                        {
                            relationshipCandidate.NavigationProperties.Add(nextSelfRefCandidate);
                        }

                        relationshipCandidate.InverseProperties.Remove(nextSelfRefCandidate);
                    }

                    if (relationshipCandidate.NavigationProperties.Count == 0)
                    {
                        foreach (var inverseProperty in relationshipCandidate.InverseProperties.ToList())
                        {
                            if (!AreCompatible(
                                    null, inverseProperty, entityTypeBuilder, targetEntityTypeBuilder))
                            {
                                relationshipCandidate.InverseProperties.Remove(inverseProperty);
                            }
                        }
                    }

                    continue;
                }

                var noOtherCompatibleNavigation = true;
                foreach (var otherNavigation in relationshipCandidate.NavigationProperties)
                {
                    if (otherNavigation != navigationProperty
                        && AreCompatible(otherNavigation, compatibleInverse, entityTypeBuilder, targetEntityTypeBuilder))
                    {
                        noOtherCompatibleNavigation = false;
                        break;
                    }
                }

                if (noOtherCompatibleNavigation)
                {
                    relationshipCandidate.NavigationProperties.Remove(navigationProperty);
                    relationshipCandidate.InverseProperties.Remove(compatibleInverse);

                    filteredRelationshipCandidates.Add(
                        new RelationshipCandidate(
                            targetEntityTypeBuilder,
                            [navigationProperty],
                            [compatibleInverse],
                            relationshipCandidate.IsOwnership)
                    );

                    if (relationshipCandidate.TargetTypeBuilder.Metadata == entityTypeBuilder.Metadata
                        && relationshipCandidate.NavigationProperties.Count == 0
                        && relationshipCandidate.InverseProperties.Count > 0)
                    {
                        var nextSelfRefCandidate = relationshipCandidate.InverseProperties.First();
                        if (!relationshipCandidate.NavigationProperties.Contains(nextSelfRefCandidate))
                        {
                            relationshipCandidate.NavigationProperties.Add(nextSelfRefCandidate);
                        }

                        relationshipCandidate.InverseProperties.Remove(nextSelfRefCandidate);
                    }

                    continue;
                }

                NextCandidate:
                break;
            }

            if (relationshipCandidate.NavigationProperties.Count > 0
                || relationshipCandidate.InverseProperties.Count > 0)
            {
                filteredRelationshipCandidates.Add(relationshipCandidate);
            }
            else if (IsImplicitlyCreatedUnusedType(relationshipCandidate.TargetTypeBuilder.Metadata)
                     && filteredRelationshipCandidates.All(
                         c => c.TargetTypeBuilder.Metadata != relationshipCandidate.TargetTypeBuilder.Metadata))
            {
                entityTypeBuilder.ModelBuilder
                    .HasNoEntityType(relationshipCandidate.TargetTypeBuilder.Metadata);
            }
        }

        return filteredRelationshipCandidates;
    }

    private static bool AreCompatible(
        PropertyInfo? navigationProperty,
        PropertyInfo? inversePropertyInfo,
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityTypeBuilder targetEntityTypeBuilder)
    {
        var entityType = entityTypeBuilder.Metadata;
        if (navigationProperty != null)
        {
            var existingNavigation = entityType.FindNavigation(navigationProperty.GetSimpleMemberName());
            if (existingNavigation != null
                && ((inversePropertyInfo != null
                        && !CanSetInverse(existingNavigation, inversePropertyInfo, targetEntityTypeBuilder))
                    || (!existingNavigation.TargetEntityType.IsAssignableFrom(targetEntityTypeBuilder.Metadata)
                        && !targetEntityTypeBuilder.Metadata.IsAssignableFrom(existingNavigation.TargetEntityType))))
            {
                return false;
            }
        }

        if (inversePropertyInfo == null)
        {
            return true;
        }

        var existingInverse = targetEntityTypeBuilder.Metadata.FindNavigation(inversePropertyInfo.Name);
        if (existingInverse != null)
        {
            if (existingInverse.DeclaringEntityType != targetEntityTypeBuilder.Metadata
                || (navigationProperty != null
                    && !CanSetInverse(existingInverse, navigationProperty, entityTypeBuilder))
                || (!existingInverse.TargetEntityType.IsAssignableFrom(entityTypeBuilder.Metadata)
                    && !entityTypeBuilder.Metadata.IsAssignableFrom(existingInverse.TargetEntityType)))
            {
                return false;
            }

            var otherEntityType = existingInverse.TargetEntityType;
            if (!entityType.ClrType.IsAssignableFrom(otherEntityType.ClrType))
            {
                return false;
            }
        }

        return true;
    }

    private static bool CanSetInverse(
        IConventionNavigation existingNavigation,
        MemberInfo inverse,
        IConventionEntityTypeBuilder inverseEntityTypeBuilder)
    {
        var fk = existingNavigation.ForeignKey;
        return (fk.IsSelfReferencing()
                || fk.GetRelatedEntityType(existingNavigation.DeclaringEntityType) == inverseEntityTypeBuilder.Metadata)
            && fk.Builder.CanSetNavigation(inverse, !existingNavigation.IsOnDependent);
    }

    private static IReadOnlyList<RelationshipCandidate> RemoveInheritedInverseNavigations(
        IReadOnlyList<RelationshipCandidate> relationshipCandidates)
    {
        if (relationshipCandidates.Count == 0)
        {
            return relationshipCandidates;
        }

        var relationshipCandidatesByRoot = relationshipCandidates.GroupBy(r => r.TargetTypeBuilder.Metadata.GetRootType())
            .ToDictionary(g => g.Key, g => g.ToList());
        foreach (var relationshipCandidatesHierarchy in relationshipCandidatesByRoot.Values)
        {
            var filteredRelationshipCandidates = new HashSet<RelationshipCandidate>();
            foreach (var relationshipCandidate in relationshipCandidatesHierarchy)
            {
                RemoveInheritedInverseNavigations(
                    relationshipCandidate, relationshipCandidatesHierarchy, filteredRelationshipCandidates);
            }
        }

        return relationshipCandidates;
    }

    private static void RemoveInheritedInverseNavigations(
        RelationshipCandidate relationshipCandidate,
        List<RelationshipCandidate> relationshipCandidatesHierarchy,
        HashSet<RelationshipCandidate> filteredRelationshipCandidates)
    {
        if (filteredRelationshipCandidates.Contains(relationshipCandidate)
            || (relationshipCandidate.NavigationProperties.Count > 1
                && relationshipCandidate.InverseProperties.Count > 0)
            || relationshipCandidate.InverseProperties.Count > 1)
        {
            return;
        }

        filteredRelationshipCandidates.Add(relationshipCandidate);
        var inverseCandidate = relationshipCandidate.InverseProperties.FirstOrDefault();
        if (inverseCandidate != null)
        {
            var relationshipsToDerivedTypes = relationshipCandidatesHierarchy
                .Where(
                    r => r.TargetTypeBuilder != relationshipCandidate.TargetTypeBuilder
                        && relationshipCandidate.TargetTypeBuilder.Metadata.IsAssignableFrom(r.TargetTypeBuilder.Metadata));
            foreach (var relationshipToDerivedType in relationshipsToDerivedTypes)
            {
                relationshipToDerivedType.InverseProperties.RemoveAll(
                    i => i.GetSimpleMemberName() == inverseCandidate.GetSimpleMemberName());

                if (!filteredRelationshipCandidates.Contains(relationshipToDerivedType))
                {
                    // An ambiguity might have been resolved
                    RemoveInheritedInverseNavigations(
                        relationshipToDerivedType, relationshipCandidatesHierarchy, filteredRelationshipCandidates);
                }
            }
        }
    }

    private static IReadOnlyList<RelationshipCandidate> RemoveSingleSidedBaseNavigations(
        IReadOnlyList<RelationshipCandidate> relationshipCandidates,
        IConventionEntityTypeBuilder entityTypeBuilder)
    {
        if (relationshipCandidates.Count == 0)
        {
            return relationshipCandidates;
        }

        var filteredRelationshipCandidates = new List<RelationshipCandidate>();
        foreach (var relationshipCandidate in relationshipCandidates)
        {
            if (relationshipCandidate.InverseProperties.Count > 0)
            {
                filteredRelationshipCandidates.Add(relationshipCandidate);
                continue;
            }

            foreach (var navigation in relationshipCandidate.NavigationProperties.ToList())
            {
                if (entityTypeBuilder.Metadata.FindDerivedNavigations(navigation.GetSimpleMemberName())
                    .Any(n => n.Inverse != null))
                {
                    relationshipCandidate.NavigationProperties.Remove(navigation);
                }
            }

            if (relationshipCandidate.NavigationProperties.Count > 0)
            {
                filteredRelationshipCandidates.Add(relationshipCandidate);
            }
            else if (IsImplicitlyCreatedUnusedType(relationshipCandidate.TargetTypeBuilder.Metadata)
                     && filteredRelationshipCandidates.All(
                         c => c.TargetTypeBuilder.Metadata != relationshipCandidate.TargetTypeBuilder.Metadata))
            {
                entityTypeBuilder.ModelBuilder
                    .HasNoEntityType(relationshipCandidate.TargetTypeBuilder.Metadata);
            }
        }

        return filteredRelationshipCandidates;
    }

    private void CreateRelationships(
        IEnumerable<RelationshipCandidate> relationshipCandidates,
        IConventionEntityTypeBuilder entityTypeBuilder)
    {
        var unusedEntityTypes = new List<IConventionEntityType>();
        foreach (var relationshipCandidate in relationshipCandidates)
        {
            var entityType = entityTypeBuilder.Metadata;

            RemoveExtraOwnershipInverse(entityType, relationshipCandidate);

            var targetEntityType = relationshipCandidate.TargetTypeBuilder.Metadata;
            if (RemoveIfAmbiguous(entityType, relationshipCandidate))
            {
                unusedEntityTypes.Add(targetEntityType);
                continue;
            }

            foreach (var navigation in relationshipCandidate.NavigationProperties)
            {
                if (!targetEntityType.IsInModel
                    && !targetEntityType.IsOwned())
                {
                    continue;
                }

                if (InversePropertyAttributeConvention.IsAmbiguous(entityType, navigation, targetEntityType))
                {
                    unusedEntityTypes.Add(targetEntityType);
                    continue;
                }

                var inverse = relationshipCandidate.InverseProperties.SingleOrDefault();
                if (inverse == null)
                {
                    if (relationshipCandidate.IsOwnership)
                    {
                        var ownership = entityTypeBuilder.HasOwnership(targetEntityType, navigation);
                        if (ownership == null)
                        {
                            unusedEntityTypes.Add(targetEntityType);
                        }
                    }
                    else
                    {
                        var relationship = entityTypeBuilder.HasRelationship(targetEntityType, navigation);
                        if (relationship == null)
                        {
                            unusedEntityTypes.Add(targetEntityType);
                        }
                    }
                }
                else
                {
                    if (InversePropertyAttributeConvention.IsAmbiguous(targetEntityType, inverse, entityType))
                    {
                        unusedEntityTypes.Add(targetEntityType);
                        continue;
                    }

                    if (relationshipCandidate.IsOwnership)
                    {
                        var ownership = entityTypeBuilder.HasOwnership(targetEntityType, navigation, inverse);
                        if (ownership == null)
                        {
                            unusedEntityTypes.Add(targetEntityType);
                        }
                    }
                    else if (entityTypeBuilder.HasRelationship(targetEntityType, navigation, inverse) == null)
                    {
                        var navigationTargetType = navigation.PropertyType.TryGetSequenceType();
                        var inverseTargetType = inverse.PropertyType.TryGetSequenceType();
                        if (navigationTargetType == targetEntityType.ClrType
                            && inverseTargetType == entityType.ClrType)
                        {
                            entityTypeBuilder.HasSkipNavigation(
                                navigation, targetEntityType, inverse, collections: true, onDependent: false);
                        }

                        if (entityType.FindNavigation(navigation) == null)
                        {
                            unusedEntityTypes.Add(targetEntityType);
                        }
                    }
                }
            }

            if (relationshipCandidate.NavigationProperties.Count == 0)
            {
                if (relationshipCandidate.InverseProperties.Count == 0
                    || targetEntityType.IsOwned())
                {
                    unusedEntityTypes.Add(targetEntityType);
                }
                else
                {
                    foreach (var inverse in relationshipCandidate.InverseProperties)
                    {
                        if (!targetEntityType.IsInModel)
                        {
                            continue;
                        }

                        if (InversePropertyAttributeConvention.IsAmbiguous(targetEntityType, inverse, entityType))
                        {
                            unusedEntityTypes.Add(targetEntityType);
                            continue;
                        }

                        targetEntityType.Builder.HasRelationship(entityTypeBuilder.Metadata, inverse);
                    }
                }
            }
        }

        foreach (var unusedEntityType in unusedEntityTypes)
        {
            if (IsImplicitlyCreatedUnusedType(unusedEntityType))
            {
                entityTypeBuilder.ModelBuilder.HasNoEntityType(unusedEntityType);
            }
        }
    }

    private void DiscoverUnidirectionalInverses(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext context,
        List<Type>? otherInverseCandidateTypes)
    {
        var model = entityTypeBuilder.Metadata.Model;
        if (otherInverseCandidateTypes == null)
        {
            return;
        }

        foreach (var inverseCandidateType in otherInverseCandidateTypes)
        {
            foreach (var inverseCandidateEntityType in model.FindEntityTypes(inverseCandidateType).ToList())
            {
                if (inverseCandidateEntityType.IsInModel)
                {
                    DiscoverRelationships(inverseCandidateEntityType.Builder, context);
                }
            }
        }
    }

    private static void RemoveExtraOwnershipInverse(IConventionEntityType entityType, RelationshipCandidate relationshipCandidate)
    {
        if (relationshipCandidate.NavigationProperties.Count > 1
            && entityType.FindOwnership()?.PrincipalEntityType == relationshipCandidate.TargetTypeBuilder.Metadata)
        {
            Type? mostDerivedType = null;
            foreach (var navigationProperty in relationshipCandidate.NavigationProperties)
            {
                var propertyType = navigationProperty.GetMemberType();
                if (mostDerivedType == null)
                {
                    mostDerivedType = propertyType;
                }
                else if (!propertyType.IsAssignableFrom(mostDerivedType)
                         && mostDerivedType.IsAssignableFrom(propertyType))
                {
                    mostDerivedType = propertyType;
                }
            }

            relationshipCandidate.NavigationProperties.RemoveAll(
                p =>
                    p.GetMemberType().IsAssignableFrom(mostDerivedType) && p.GetMemberType() != mostDerivedType);
        }

        if (relationshipCandidate.InverseProperties.Count > 1
            && relationshipCandidate.IsOwnership)
        {
            Type? mostDerivedType = null;
            foreach (var inverseProperty in relationshipCandidate.InverseProperties)
            {
                var inverseType = inverseProperty.GetMemberType();
                if (mostDerivedType == null)
                {
                    mostDerivedType = inverseType;
                }
                else if (!inverseType.IsAssignableFrom(mostDerivedType)
                         && mostDerivedType.IsAssignableFrom(inverseType))
                {
                    mostDerivedType = inverseType;
                }
            }

            relationshipCandidate.InverseProperties.RemoveAll(
                p =>
                    p.GetMemberType().IsAssignableFrom(mostDerivedType) && p.GetMemberType() != mostDerivedType);
        }
    }

    /// <summary>
    ///     Returns a value indicating whether the given entity type should be added as owned if it isn't currently in the model.
    /// </summary>
    /// <param name="targetType">Target entity type.</param>
    /// <param name="model">The model.</param>
    /// <returns><see langword="true" /> if the given entity type should be owned.</returns>
    protected virtual bool? ShouldBeOwned(Type targetType, IConventionModel model)
        => null;

    private bool RemoveIfAmbiguous(IConventionEntityType entityType, RelationshipCandidate relationshipCandidate)
    {
        var targetEntityType = relationshipCandidate.TargetTypeBuilder.Metadata;
        var isAmbiguousOnBase = entityType.BaseType != null
            && HasAmbiguousNavigationsTo(entityType.BaseType, targetEntityType.ClrType)
            || (targetEntityType.BaseType != null
                && HasAmbiguousNavigationsTo(targetEntityType.BaseType, entityType.ClrType));
        if ((relationshipCandidate.NavigationProperties.Count > 1
                && relationshipCandidate.InverseProperties.Count > 0
                && !relationshipCandidate.IsOwnership)
            || relationshipCandidate.InverseProperties.Count > 1
            || isAmbiguousOnBase
            || HasDeclaredAmbiguousNavigationsTo(entityType, targetEntityType.ClrType)
            || HasDeclaredAmbiguousNavigationsTo(targetEntityType, entityType.ClrType))
        {
            if (entityType.IsOwned())
            {
                var ownership = entityType.FindOwnership()!;
                if (ownership.PrincipalEntityType == targetEntityType)
                {
                    // Even if there are ambiguous navigations to the owner the ownership shouldn't be removed
                    relationshipCandidate.InverseProperties.Remove(ownership.PrincipalToDependent!.PropertyInfo!);
                }
            }

            if (!isAmbiguousOnBase)
            {
                Dependencies.Logger.MultipleNavigationProperties(
                    relationshipCandidate.NavigationProperties.Count == 0
                        ? new[] { new Tuple<MemberInfo?, Type>(null, targetEntityType.ClrType) }
                        : relationshipCandidate.NavigationProperties.Select(
                            n => new Tuple<MemberInfo?, Type>(n, entityType.ClrType)),
                    relationshipCandidate.InverseProperties.Count == 0
                        ? new[] { new Tuple<MemberInfo?, Type>(null, targetEntityType.ClrType) }
                        : relationshipCandidate.InverseProperties.Select(
                            n => new Tuple<MemberInfo?, Type>(n, targetEntityType.ClrType)));
            }

            foreach (var navigationProperty in relationshipCandidate.NavigationProperties.ToList())
            {
                RemoveNavigation(
                    navigationProperty, entityType, relationshipCandidate.NavigationProperties);
            }

            foreach (var inverseProperty in relationshipCandidate.InverseProperties.ToList())
            {
                RemoveNavigation(
                    inverseProperty, targetEntityType, relationshipCandidate.InverseProperties);
            }

            if (!isAmbiguousOnBase)
            {
                AddAmbiguous(entityType.Builder, relationshipCandidate.NavigationProperties, targetEntityType.ClrType);

                AddAmbiguous(targetEntityType.Builder, relationshipCandidate.InverseProperties, entityType.ClrType);
            }

            return true;
        }

        return false;
    }

    private static void RemoveNavigation(
        PropertyInfo navigationProperty,
        IConventionEntityType declaringEntityType,
        List<PropertyInfo> toRemoveFrom)
    {
        var navigationPropertyName = navigationProperty.GetSimpleMemberName();
        var existingNavigation = declaringEntityType.FindDeclaredNavigation(navigationPropertyName);
        if (existingNavigation != null)
        {
            var removed = true;
            if (existingNavigation.ForeignKey.IsOwnership)
            {
                if (existingNavigation.IsOnDependent)
                {
                    removed = existingNavigation.ForeignKey.Builder.HasNavigation((string?)null, existingNavigation.IsOnDependent)
                        != null;
                }
                else if (IsImplicitlyCreatedUnusedType(existingNavigation.TargetEntityType))
                {
                    removed = declaringEntityType.Builder.ModelBuilder.HasNoEntityType(existingNavigation.TargetEntityType)
                        != null;
                }
                else
                {
                    removed = existingNavigation.ForeignKey.DeclaringEntityType.Builder
                            .HasNoRelationship(existingNavigation.ForeignKey)
                        != null;
                }
            }
            else if (existingNavigation.ForeignKey.DeclaringEntityType.Builder
                         .HasNoRelationship(existingNavigation.ForeignKey)
                     == null)
            {
                removed = existingNavigation.ForeignKey.Builder.HasNavigation((string?)null, existingNavigation.IsOnDependent)
                    != null;
            }

            if (!removed)
            {
                // Navigations of higher configuration source are not ambiguous
                toRemoveFrom.Remove(navigationProperty);
            }
        }
        else
        {
            var skipNavigation = declaringEntityType.FindDeclaredSkipNavigation(navigationPropertyName);
            if (skipNavigation != null)
            {
                var inverse = skipNavigation.Inverse;
                if (declaringEntityType.Builder.HasNoSkipNavigation(skipNavigation) == null)
                {
                    // Navigations of higher configuration source are not ambiguous
                    toRemoveFrom.Remove(navigationProperty);
                }
                else if (inverse?.IsInModel == true)
                {
                    inverse.DeclaringEntityType.Builder.HasNoSkipNavigation(inverse);
                }
            }
        }
    }

    /// <inheritdoc />
    public virtual void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
    {
        DiscoverRelationships(entityTypeBuilder, context, discoverUnmatchedInverses: true);
        if (!entityTypeBuilder.Metadata.IsInModel)
        {
            context.StopProcessing();
        }
    }

    /// <inheritdoc />
    public virtual void ProcessEntityTypeBaseTypeChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? oldBaseType,
        IConventionContext<IConventionEntityType> context)
    {
        if (oldBaseType?.IsInModel == true)
        {
            DiscoverRelationships(oldBaseType.Builder, context);
        }

        var entityType = entityTypeBuilder.Metadata;
        if (entityType.BaseType != newBaseType)
        {
            return;
        }

        if (newBaseType != null)
        {
            foreach (var ignoredMember in newBaseType.GetAllBaseTypesInclusive().SelectMany(et => et.GetIgnoredMembers()))
            {
                if (entityTypeBuilder.Metadata.GetRuntimeProperties().TryGetValue(ignoredMember, out var ignoredPropertyInfo))
                {
                    ProcessEntityTypeMemberIgnoredOnBase(entityType, ignoredMember, ignoredPropertyInfo);
                }
            }
        }

        ApplyOnRelatedEntityTypes(entityType, context);
        foreach (var derivedEntityType in entityType.GetDerivedTypesInclusive())
        {
            DiscoverRelationships(derivedEntityType.Builder, context);
        }
    }

    private void ApplyOnRelatedEntityTypes(IConventionEntityType entityType, IConventionContext context)
    {
        var relatedEntityTypes = entityType.GetReferencingForeignKeys().Select(fk => fk.DeclaringEntityType)
            .Concat(entityType.GetForeignKeys().Select(fk => fk.PrincipalEntityType))
            .Distinct()
            .ToList();

        foreach (var relatedEntityType in relatedEntityTypes)
        {
            if (relatedEntityType.IsInModel)
            {
                DiscoverRelationships(relatedEntityType.Builder, context);
            }
        }
    }

    /// <inheritdoc />
    public virtual void ProcessForeignKeyRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionForeignKey foreignKey,
        IConventionContext<IConventionForeignKey> context)
    {
        if (entityTypeBuilder.Metadata.IsInModel
            && foreignKey.IsOwnership
            && !entityTypeBuilder.Metadata.IsOwned())
        {
            DiscoverRelationships(entityTypeBuilder, context, discoverUnmatchedInverses: true);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessNavigationRemoved(
        IConventionEntityTypeBuilder sourceEntityTypeBuilder,
        IConventionEntityTypeBuilder targetEntityTypeBuilder,
        string navigationName,
        MemberInfo? memberInfo,
        IConventionContext<string> context)
    {
        if (sourceEntityTypeBuilder.Metadata.IsInModel
            && (targetEntityTypeBuilder.Metadata.IsInModel
                || !sourceEntityTypeBuilder.ModelBuilder.IsIgnored(targetEntityTypeBuilder.Metadata.Name))
            && memberInfo != null
            && sourceEntityTypeBuilder.Metadata.FindNavigation(navigationName) == null
            && IsCandidateNavigationProperty(
                sourceEntityTypeBuilder.Metadata, navigationName, memberInfo)
            && Dependencies.MemberClassifier.FindCandidateNavigationPropertyType(
                memberInfo, targetEntityTypeBuilder.Metadata.Model, UseAttributes, out _)
            != null)
        {
            Process(sourceEntityTypeBuilder.Metadata, navigationName, memberInfo, context);
        }
    }

    private void Process(
        IConventionEntityType entityType,
        string navigationName,
        MemberInfo memberInfo,
        IConventionContext context)
    {
        DiscoverRelationships(entityType.Builder, context);
        if (entityType.FindNavigation(navigationName) != null
            || IsAmbiguous(entityType, memberInfo))
        {
            return;
        }

        foreach (var derivedEntityType in entityType.GetDirectlyDerivedTypes())
        {
            Process(derivedEntityType, navigationName, memberInfo, context);
        }
    }

    private static bool IsCandidateNavigationProperty(
        IConventionEntityType sourceEntityType,
        string navigationName,
        MemberInfo memberInfo)
        => sourceEntityType.Builder.IsIgnored(navigationName) == false
            && sourceEntityType.FindProperty(navigationName) == null
            && sourceEntityType.FindServiceProperty(navigationName) == null
            && sourceEntityType.FindComplexProperty(navigationName) == null
            && (memberInfo is not PropertyInfo propertyInfo || propertyInfo.GetIndexParameters().Length == 0)
            && (!sourceEntityType.IsKeyless
                || (memberInfo as PropertyInfo)?.PropertyType.TryGetSequenceType() == null);

    /// <inheritdoc />
    public virtual void ProcessTypeIgnored(
        IConventionModelBuilder modelBuilder,
        string name,
        Type? type,
        IConventionContext<string> context)
    {
        if (type == null)
        {
            return;
        }

        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            var ambiguityRemoved = RemoveAmbiguous(entityType, type);
            if (ambiguityRemoved)
            {
                DiscoverRelationships(entityType.Builder, context);
            }
        }
    }

    /// <inheritdoc />
    public virtual void ProcessEntityTypeMemberIgnored(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string name,
        IConventionContext<string> context)
    {
        if (!entityTypeBuilder.Metadata.GetRuntimeProperties().TryGetValue(name, out var ignoredPropertyInfo))
        {
            return;
        }

        var anyAmbiguityRemoved = false;
        foreach (var derivedEntityType in entityTypeBuilder.Metadata.GetDerivedTypesInclusive())
        {
            anyAmbiguityRemoved |= ProcessEntityTypeMemberIgnoredOnBase(derivedEntityType, name, ignoredPropertyInfo);
        }

        if (anyAmbiguityRemoved)
        {
            DiscoverRelationships(entityTypeBuilder, context);
        }
    }

    private bool ProcessEntityTypeMemberIgnoredOnBase(IConventionEntityType entityType, string name, PropertyInfo property)
    {
        var ambiguousNavigations = GetAmbiguousNavigations(entityType);
        if (ambiguousNavigations == null)
        {
            return false;
        }

        var ambiguousNavigationFound = false;
        foreach (var (memberInfo, targetClrType) in ambiguousNavigations)
        {
            if (memberInfo.GetSimpleMemberName() != name
                && memberInfo.GetMemberType() != property.PropertyType)
            {
                continue;
            }

            ambiguousNavigationFound = true;

            RemoveAmbiguous(entityType, targetClrType);

            var targetType = TryGetTargetEntityTypeBuilder(
                entityType.Builder, targetClrType, memberInfo, shouldCreate: false)?.Metadata;
            if (targetType != null)
            {
                RemoveAmbiguous(targetType, entityType.ClrType);
            }
        }

        return ambiguousNavigationFound;
    }

    /// <inheritdoc />
    public virtual void ProcessForeignKeyNullNavigationSet(
        IConventionForeignKeyBuilder relationshipBuilder,
        bool pointsToPrincipal,
        IConventionContext<IConventionNavigation> context)
    {
        var entityType =
            pointsToPrincipal
                ? relationshipBuilder.Metadata.DeclaringEntityType
                : relationshipBuilder.Metadata.PrincipalEntityType;

        var targetEntityType =
            pointsToPrincipal
                ? relationshipBuilder.Metadata.PrincipalEntityType
                : relationshipBuilder.Metadata.DeclaringEntityType;
        var ambiguousNavigations = GetAmbiguousNavigations(entityType);
        if (ambiguousNavigations == null)
        {
            return;
        }

        var ambiguousNavigationFound = false;
        foreach (var (memberInfo, targetClrType) in ambiguousNavigations)
        {
            if (targetClrType != targetEntityType.ClrType)
            {
                continue;
            }

            ambiguousNavigationFound = true;

            RemoveAmbiguous(entityType, targetClrType);

            var targetType = TryGetTargetEntityTypeBuilder(
                entityType.Builder, targetClrType, memberInfo, shouldCreate: false)?.Metadata;
            if (targetType != null)
            {
                RemoveAmbiguous(targetType, entityType.ClrType);
            }
        }

        if (ambiguousNavigationFound)
        {
            DiscoverRelationships(entityType.Builder, context);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessNavigationAdded(
        IConventionNavigationBuilder navigationBuilder,
        IConventionContext<IConventionNavigationBuilder> context)
    {
        var navigation = navigationBuilder.Metadata;
        foreach (var entityType in navigation.DeclaringEntityType.GetDerivedTypesInclusive())
        {
            var targetEntityType = navigation.TargetEntityType;
            // Only run the convention if an ambiguity might have been removed
            var ambiguityRemoved = RemoveAmbiguous(entityType, targetEntityType.ClrType);
            var targetAmbiguityRemoved = RemoveAmbiguous(targetEntityType, entityType.ClrType);

            if (ambiguityRemoved)
            {
                DiscoverRelationships(entityType.Builder, context);
            }

            if (targetAmbiguityRemoved)
            {
                DiscoverRelationships(targetEntityType.Builder, context);
            }
        }

        if (!navigationBuilder.Metadata.IsInModel)
        {
            context.StopProcessing();
        }
    }

    /// <inheritdoc />
    public virtual void ProcessForeignKeyOwnershipChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext<bool?> context)
        => DiscoverRelationships(relationshipBuilder.Metadata.DeclaringEntityType.Builder, context, discoverUnmatchedInverses: true);

    // TODO: Rely on layering to remove these when no longer referenced #15898
    private static bool IsImplicitlyCreatedUnusedType(IConventionEntityType entityType)
        => (entityType.IsOwned() || entityType.HasSharedClrType)
            && entityType.GetConfigurationSource() == ConfigurationSource.Convention
            && !entityType.GetForeignKeys().Any()
            && !entityType.GetReferencingForeignKeys().Any();

    private static bool IsAmbiguous(IConventionEntityType? entityType, MemberInfo navigationProperty)
    {
        while (entityType != null)
        {
            var ambiguousNavigations = GetAmbiguousNavigations(entityType);
            if (ambiguousNavigations?.ContainsKey(navigationProperty) == true)
            {
                return true;
            }

            entityType = entityType.BaseType;
        }

        return false;
    }

    private static bool HasAmbiguousNavigationsTo(IConventionEntityType? sourceEntityType, Type targetClrType)
    {
        while (sourceEntityType != null)
        {
            if (HasDeclaredAmbiguousNavigationsTo(sourceEntityType, targetClrType))
            {
                return true;
            }

            sourceEntityType = sourceEntityType.BaseType;
        }

        return false;
    }

    private static bool HasDeclaredAmbiguousNavigationsTo(IConventionEntityType sourceEntityType, Type targetClrType)
        => GetAmbiguousNavigations(sourceEntityType)?.ContainsValue(targetClrType) == true;

    private static ImmutableSortedDictionary<MemberInfo, Type>? GetAmbiguousNavigations(IConventionEntityType entityType)
        => entityType.FindAnnotation(CoreAnnotationNames.AmbiguousNavigations)?.Value
            as ImmutableSortedDictionary<MemberInfo, Type>;

    private static void AddAmbiguous(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IReadOnlyList<PropertyInfo> navigationProperties,
        Type targetType)
    {
        if (navigationProperties.Count == 0)
        {
            return;
        }

        var currentAmbiguousNavigations = GetAmbiguousNavigations(entityTypeBuilder.Metadata);
        var newAmbiguousNavigations = ImmutableSortedDictionary.CreateRange(
            MemberInfoNameComparer.Instance,
            navigationProperties.Where(n => currentAmbiguousNavigations?.ContainsKey(n) != true)
                .Select(n => new KeyValuePair<MemberInfo, Type>(n, targetType)));

        if (currentAmbiguousNavigations != null)
        {
            newAmbiguousNavigations = newAmbiguousNavigations.Count > 0
                ? currentAmbiguousNavigations.AddRange(newAmbiguousNavigations)
                : currentAmbiguousNavigations;
        }

        SetAmbiguousNavigations(entityTypeBuilder, newAmbiguousNavigations);
    }

    private static bool RemoveAmbiguous(IConventionEntityType entityType, Type targetType)
    {
        var ambiguousNavigations = GetAmbiguousNavigations(entityType);
        if (ambiguousNavigations?.IsEmpty == false)
        {
            var newAmbiguousNavigations = ambiguousNavigations;
            foreach (var (memberInfo, type) in ambiguousNavigations)
            {
                if (targetType.IsAssignableFrom(type))
                {
                    newAmbiguousNavigations = newAmbiguousNavigations.Remove(memberInfo);
                }
            }

            if (ambiguousNavigations.Count != newAmbiguousNavigations.Count)
            {
                SetAmbiguousNavigations(entityType.Builder, newAmbiguousNavigations);
                return true;
            }
        }

        return false;
    }

    private static void SetAmbiguousNavigations(
        IConventionEntityTypeBuilder entityTypeBuilder,
        ImmutableSortedDictionary<MemberInfo, Type> ambiguousNavigations)
        => entityTypeBuilder.HasAnnotation(CoreAnnotationNames.AmbiguousNavigations, ambiguousNavigations);

    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    private sealed class RelationshipCandidate
    {
        public RelationshipCandidate(
            IConventionEntityTypeBuilder targetTypeBuilder,
            List<PropertyInfo> navigations,
            List<PropertyInfo> inverseNavigations,
            bool ownership)
        {
            TargetTypeBuilder = targetTypeBuilder;
            NavigationProperties = navigations;
            InverseProperties = inverseNavigations;
            IsOwnership = ownership;
        }

        public IConventionEntityTypeBuilder TargetTypeBuilder { [DebuggerStepThrough] get; }
        public List<PropertyInfo> NavigationProperties { [DebuggerStepThrough] get; }
        public List<PropertyInfo> InverseProperties { [DebuggerStepThrough] get; }
        public bool IsOwnership { [DebuggerStepThrough] get; }

        private string DebuggerDisplay()
            => TargetTypeBuilder.Metadata.ToDebugString(MetadataDebugStringOptions.SingleLineDefault)
                + ": ["
                + string.Join(", ", NavigationProperties.Select(p => p.Name))
                + "] - ["
                + string.Join(", ", InverseProperties.Select(p => p.Name))
                + "]"
                + (IsOwnership ? " Ownership" : "");
    }
}
