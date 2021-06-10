// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures relationships between entity types based on the navigation properties
    ///     as long as there is no ambiguity as to which is the corresponding inverse navigation.
    /// </summary>
    public class RelationshipDiscoveryConvention :
        IEntityTypeAddedConvention,
        IEntityTypeIgnoredConvention,
        IEntityTypeBaseTypeChangedConvention,
        IEntityTypeMemberIgnoredConvention,
        INavigationRemovedConvention,
        INavigationAddedConvention,
        IForeignKeyOwnershipChangedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RelationshipDiscoveryConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public RelationshipDiscoveryConvention(ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        private void DiscoverRelationships(IConventionEntityTypeBuilder entityTypeBuilder, IConventionContext context)
        {
            var relationshipCandidates = FindRelationshipCandidates(entityTypeBuilder);
            relationshipCandidates = RemoveIncompatibleWithExistingRelationships(relationshipCandidates, entityTypeBuilder);
            relationshipCandidates = RemoveInheritedInverseNavigations(relationshipCandidates);
            relationshipCandidates = RemoveSingleSidedBaseNavigations(relationshipCandidates, entityTypeBuilder);

            CreateRelationships(relationshipCandidates, entityTypeBuilder);
        }

        private IReadOnlyList<RelationshipCandidate> FindRelationshipCandidates(IConventionEntityTypeBuilder entityTypeBuilder)
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

            foreach (var candidateTuple in Dependencies.MemberClassifier.GetNavigationCandidates(entityType))
            {
                var navigationPropertyInfo = candidateTuple.Key;
                var (targetClrType, shouldBeOwned) = candidateTuple.Value;

                if (!IsCandidateNavigationProperty(entityTypeBuilder, navigationPropertyInfo.GetSimpleMemberName(), navigationPropertyInfo))
                {
                    continue;
                }

                var candidateTargetEntityTypeBuilder = TryGetTargetEntityTypeBuilder(
                    entityTypeBuilder, targetClrType, navigationPropertyInfo, shouldBeOwned);
                if (candidateTargetEntityTypeBuilder == null)
                {
                    continue;
                }

                if (!entityType.IsInModel)
                {
                    // Current entity type was removed while the target entity type was being added
                    foreach (var relationshipCandidate in relationshipCandidates.Values)
                    {
                        var targetType = relationshipCandidate.TargetTypeBuilder.Metadata;
                        if (targetType.IsInModel
                            && IsImplicitlyCreatedUnusedSharedType(targetType))
                        {
                            targetType.Builder.ModelBuilder.HasNoEntityType(targetType);
                        }
                    }

                    return Array.Empty<RelationshipCandidate>();
                }

                var candidateTargetEntityType = candidateTargetEntityTypeBuilder.Metadata;
                if (candidateTargetEntityType.IsKeyless
                    || (candidateTargetEntityType.IsOwned()
                        && HasDeclaredAmbiguousNavigationsTo(entityType, targetClrType)))
                {
                    continue;
                }

                Check.DebugAssert(entityType.ClrType != targetClrType
                    || !candidateTargetEntityType.IsOwned()
                    || candidateTargetEntityType.FindOwnership()?.PrincipalToDependent?.Name == navigationPropertyInfo.GetSimpleMemberName(),
                    "New self-referencing ownerships shouldn't be discovered");

                var targetOwnership = candidateTargetEntityType.FindOwnership();
                var shouldBeOwnership = candidateTargetEntityType.IsOwned()
                    && (targetOwnership == null
                        || (targetOwnership.PrincipalEntityType == entityType
                            && targetOwnership.PrincipalToDependent?.Name == navigationPropertyInfo.GetSimpleMemberName()));

                if (candidateTargetEntityType.IsOwned()
                    && !shouldBeOwnership
                    && (targetOwnership?.PrincipalEntityType == entityType
                        || !candidateTargetEntityType.IsInOwnershipPath(entityType))
                    && (ownership == null
                        || !entityType.IsInOwnershipPath(candidateTargetEntityType)))
                {
                    // Only the owner or nested ownees can have navigations to an owned type
                    // Also skip non-ownership navigations from the owner
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
                    var inverseCandidates = Dependencies.MemberClassifier.GetNavigationCandidates(candidateTargetEntityType);
                    foreach (var inverseCandidateTuple in inverseCandidates)
                    {
                        var inversePropertyInfo = inverseCandidateTuple.Key;
                        if (navigationPropertyInfo.IsSameAs(inversePropertyInfo)
                            || !IsCandidateNavigationProperty(
                                candidateTargetEntityTypeBuilder, inversePropertyInfo.GetSimpleMemberName(), inversePropertyInfo))
                        {
                            continue;
                        }

                        var inverseTargetType = inverseCandidateTuple.Value.Type;
                        if (inverseTargetType != entityType.ClrType
                            && (!inverseTargetType.IsAssignableFrom(entityType.ClrType)
                                || (!shouldBeOwnership
                                    && !candidateTargetEntityType.IsInOwnershipPath(entityType))))
                        {
                            // Only use inverse of a base type if the target is owned by the current entity type
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
                            && inversePropertyInfo.PropertyType.TryGetSequenceType() != null
                            && navigations.Count == 1)
                        {
                            // Target type should be the principal, discover the relationship from the other side
                            var targetType = candidateTargetEntityType;
                            if (targetType.IsInModel
                                && IsImplicitlyCreatedUnusedSharedType(targetType))
                            {
                                targetType.Builder.ModelBuilder.HasNoEntityType(targetType);
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
                    new RelationshipCandidate(candidateTargetEntityTypeBuilder, navigations, inverseNavigationCandidates, shouldBeOwnership);

                Continue:;
            }

            return UpdateTargetEntityTypes(entityTypeBuilder, relationshipCandidates);
        }

        private List<RelationshipCandidate> UpdateTargetEntityTypes(
            IConventionEntityTypeBuilder entityTypeBuilder,
            Dictionary<IConventionEntityType, RelationshipCandidate> relationshipCandidates)
        {
            var candidates = new List<RelationshipCandidate>();
            foreach (var relationshipCandidate in relationshipCandidates.Values)
            {
                if (relationshipCandidate.TargetTypeBuilder.Metadata.IsInModel)
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
        /// <param name="entityTypeBuilder"> The builder for the referencing entity type. </param>
        /// <param name="targetClrType"> The CLR type of the target entity type. </param>
        /// <param name="navigationMemberInfo"> The navigation member. </param>
        /// <param name="shouldBeOwned"> Whether the target entity type should be owned. </param>
        /// <param name="shouldCreate"> Whether an entity type should be created if one doesn't currently exist. </param>
        /// <returns> The builder for the target entity type or <see langword="null"/> if it can't be created. </returns>
        protected virtual IConventionEntityTypeBuilder? TryGetTargetEntityTypeBuilder(
            IConventionEntityTypeBuilder entityTypeBuilder,
            Type targetClrType,
            MemberInfo navigationMemberInfo,
            bool? shouldBeOwned = null,
            bool shouldCreate = true)
        {
            if (shouldCreate)
            {
                var targetEntityTypeBuilder = ((InternalEntityTypeBuilder)entityTypeBuilder)
                    .GetTargetEntityTypeBuilder(targetClrType, navigationMemberInfo, ConfigurationSource.Convention,
                        shouldBeOwned ?? ShouldBeOwned(targetClrType, entityTypeBuilder.Metadata.Model));
                if (targetEntityTypeBuilder != null)
                {
                    return targetEntityTypeBuilder;
                }
            }

            return ((InternalEntityTypeBuilder)entityTypeBuilder)
                .GetTargetEntityTypeBuilder(targetClrType, navigationMemberInfo, null, shouldBeOwned);
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
                        if (IsCompatibleInverse(
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
                                new List<PropertyInfo> { navigationProperty },
                                new List<PropertyInfo>(),
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

                        continue;
                    }

                    var noOtherCompatibleNavigation = true;
                    foreach (var otherNavigation in relationshipCandidate.NavigationProperties)
                    {
                        if (otherNavigation != navigationProperty
                            && IsCompatibleInverse(otherNavigation, compatibleInverse, entityTypeBuilder, targetEntityTypeBuilder))
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
                                new List<PropertyInfo> { navigationProperty },
                                new List<PropertyInfo> { compatibleInverse },
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
                else if (IsImplicitlyCreatedUnusedSharedType(relationshipCandidate.TargetTypeBuilder.Metadata)
                    && filteredRelationshipCandidates.All(
                        c => c.TargetTypeBuilder.Metadata != relationshipCandidate.TargetTypeBuilder.Metadata))
                {
                    entityTypeBuilder.ModelBuilder
                        .HasNoEntityType(relationshipCandidate.TargetTypeBuilder.Metadata);
                }
            }

            return filteredRelationshipCandidates;
        }

        private static bool IsCompatibleInverse(
            PropertyInfo navigationProperty,
            PropertyInfo inversePropertyInfo,
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionEntityTypeBuilder targetEntityTypeBuilder)
        {
            var entityType = entityTypeBuilder.Metadata;
            var existingNavigation = entityType.FindNavigation(navigationProperty.GetSimpleMemberName());
            if (existingNavigation != null
                && !CanMergeWith(existingNavigation, inversePropertyInfo, targetEntityTypeBuilder))
            {
                return false;
            }

            var existingInverse = targetEntityTypeBuilder.Metadata.FindNavigation(inversePropertyInfo.Name);
            if (existingInverse != null)
            {
                if (existingInverse.DeclaringEntityType != targetEntityTypeBuilder.Metadata
                    || !CanMergeWith(existingInverse, navigationProperty, entityTypeBuilder))
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

        private static bool CanMergeWith(
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
                else if (IsImplicitlyCreatedUnusedSharedType(relationshipCandidate.TargetTypeBuilder.Metadata)
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
                        AddAmbiguous(entityTypeBuilder, relationshipCandidate.NavigationProperties, targetEntityType.ClrType);

                        AddAmbiguous(targetEntityType.Builder, relationshipCandidate.InverseProperties, entityType.ClrType);
                    }

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
                            entityTypeBuilder.HasOwnership(targetEntityType, navigation);
                        }
                        else
                        {
                            entityTypeBuilder.HasRelationship(targetEntityType, navigation);
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
                            entityTypeBuilder.HasOwnership(targetEntityType, navigation, inverse);
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
                if (IsImplicitlyCreatedUnusedSharedType(unusedEntityType))
                {
                    entityTypeBuilder.ModelBuilder.HasNoEntityType(unusedEntityType);
                }
            }
        }

        /// <summary>
        ///     Returns a value indicating whether the given entity type should be added as owned if it isn't currently in the model.
        /// </summary>
        /// <param name="targetType"> Target entity type. </param>
        /// <param name="model"> The model. </param>
        /// <returns> <see langword="true"/> if the given entity type should be owned. </returns>
        protected virtual bool? ShouldBeOwned(Type targetType, IConventionModel model)
            => null;

        private void RemoveNavigation(
            PropertyInfo navigationProperty,
            IConventionEntityType declaringEntityType,
            List<PropertyInfo> toRemoveFrom)
        {
            var navigationPropertyName = navigationProperty.GetSimpleMemberName();
            var existingNavigation = declaringEntityType.FindDeclaredNavigation(navigationPropertyName);
            if (existingNavigation != null)
            {
                if (existingNavigation.ForeignKey.DeclaringEntityType.Builder
                        .HasNoRelationship(existingNavigation.ForeignKey)
                    == null
                    && existingNavigation.ForeignKey.Builder.HasNavigation(
                        (string?)null, existingNavigation.IsOnDependent)
                    == null)
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
            => DiscoverRelationships(entityTypeBuilder, context);

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
                    ProcessEntityTypeMemberIgnoredOnBase(entityType, ignoredMember);
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
        public virtual void ProcessNavigationRemoved(
            IConventionEntityTypeBuilder sourceEntityTypeBuilder,
            IConventionEntityTypeBuilder targetEntityTypeBuilder,
            string navigationName,
            MemberInfo? memberInfo,
            IConventionContext<string> context)
        {
            if ((targetEntityTypeBuilder.Metadata.IsInModel
                    || !sourceEntityTypeBuilder.ModelBuilder.IsIgnored(targetEntityTypeBuilder.Metadata.Name))
                && memberInfo != null
                && IsCandidateNavigationProperty(sourceEntityTypeBuilder, navigationName, memberInfo)
                && Dependencies.MemberClassifier.FindCandidateNavigationPropertyType(
                    memberInfo, targetEntityTypeBuilder.Metadata.Model, out _) != null)
            {
                Process(sourceEntityTypeBuilder.Metadata, navigationName, memberInfo!, context);
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
            IConventionEntityTypeBuilder? sourceEntityTypeBuilder,
            string navigationName,
            MemberInfo memberInfo)
            => sourceEntityTypeBuilder?.IsIgnored(navigationName) == false
                && sourceEntityTypeBuilder.Metadata.FindProperty(navigationName) == null
                && sourceEntityTypeBuilder.Metadata.FindServiceProperty(navigationName) == null
                && (memberInfo is not PropertyInfo propertyInfo || propertyInfo.GetIndexParameters().Length == 0)
                && (!sourceEntityTypeBuilder.Metadata.IsKeyless
                    || (memberInfo as PropertyInfo)?.PropertyType.TryGetSequenceType() == null);

        /// <inheritdoc />
        public virtual void ProcessEntityTypeIgnored(
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
            var anyAmbiguityRemoved = false;
            foreach (var derivedEntityType in entityTypeBuilder.Metadata.GetDerivedTypesInclusive())
            {
                anyAmbiguityRemoved |= ProcessEntityTypeMemberIgnoredOnBase(derivedEntityType, name);
            }

            if (anyAmbiguityRemoved)
            {
                DiscoverRelationships(entityTypeBuilder, context);
            }
        }

        private bool ProcessEntityTypeMemberIgnoredOnBase(IConventionEntityType entityType, string name)
        {
            var ambiguousNavigations = GetAmbiguousNavigations(entityType);
            if (ambiguousNavigations == null)
            {
                return false;
            }

            KeyValuePair<MemberInfo, Type>? ambiguousNavigation = null;
            foreach (var navigation in ambiguousNavigations)
            {
                if (navigation.Key.GetSimpleMemberName() == name)
                {
                    ambiguousNavigation = navigation;
                }
            }

            if (ambiguousNavigation == null)
            {
                return false;
            }

            var targetClrType = ambiguousNavigation.Value.Value;
            RemoveAmbiguous(entityType, targetClrType);

            var targetType = TryGetTargetEntityTypeBuilder(
                entityType.Builder, targetClrType, ambiguousNavigation.Value.Key, shouldCreate: false)?.Metadata;
            if (targetType != null)
            {
                RemoveAmbiguous(targetType, entityType.ClrType);
            }

            return true;
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
            => DiscoverRelationships(relationshipBuilder.Metadata.DeclaringEntityType.Builder, context);

        private static bool IsImplicitlyCreatedUnusedSharedType(IConventionEntityType entityType)
            => entityType.HasSharedClrType
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
                foreach (var ambiguousNavigation in ambiguousNavigations)
                {
                    if (targetType.IsAssignableFrom(ambiguousNavigation.Value))
                    {
                        newAmbiguousNavigations = newAmbiguousNavigations.Remove(ambiguousNavigation.Key);
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
}
