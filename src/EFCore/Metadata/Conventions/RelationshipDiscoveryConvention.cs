// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures relationships between entity types based on the navigation properties
    ///     as long as there is no ambiguity as to which is the corresponding inverse navigation.
    /// </summary>
    public class RelationshipDiscoveryConvention :
        IEntityTypeAddedConvention,
        IEntityTypeBaseTypeChangedConvention,
        INavigationRemovedConvention,
        IEntityTypeMemberIgnoredConvention,
        INavigationAddedConvention,
        IForeignKeyOwnershipChangedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RelationshipDiscoveryConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public RelationshipDiscoveryConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        private void DiscoverRelationships(IConventionEntityTypeBuilder entityTypeBuilder, IConventionContext context)
        {
            if (!entityTypeBuilder.Metadata.HasClrType()
                || entityTypeBuilder.ModelBuilder.IsIgnored(entityTypeBuilder.Metadata.ClrType))
            {
                return;
            }

            var relationshipCandidates = FindRelationshipCandidates(entityTypeBuilder);
            relationshipCandidates = RemoveIncompatibleWithExistingRelationships(relationshipCandidates, entityTypeBuilder);
            relationshipCandidates = RemoveInheritedInverseNavigations(relationshipCandidates);
            relationshipCandidates = RemoveSingleSidedBaseNavigations(relationshipCandidates, entityTypeBuilder);

            using (context.DelayConventions())
            {
                CreateRelationships(relationshipCandidates, entityTypeBuilder);
            }
        }

        private IReadOnlyList<RelationshipCandidate> FindRelationshipCandidates(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            var entityType = entityTypeBuilder.Metadata;
            var model = entityType.Model;
            var relationshipCandidates = new Dictionary<IConventionEntityType, RelationshipCandidate>();
            var ownership = entityTypeBuilder.Metadata.FindOwnership();
            if (ownership == null
                && model.FindIsOwnedConfigurationSource(entityTypeBuilder.Metadata.ClrType) != null)
            {
                return relationshipCandidates.Values.ToList();
            }

            foreach (var candidateTuple in GetNavigationCandidates(entityType))
            {
                var navigationPropertyInfo = candidateTuple.Key;
                var targetClrType = candidateTuple.Value;

                if (!IsCandidateNavigationProperty(entityTypeBuilder, navigationPropertyInfo.GetSimpleMemberName(), navigationPropertyInfo)
                    || (model.FindIsOwnedConfigurationSource(targetClrType) != null
                        && HasDeclaredAmbiguousNavigationsTo(entityType, targetClrType)))
                {
                    continue;
                }

                IConventionEntityTypeBuilder candidateTargetEntityTypeBuilder = ((InternalEntityTypeBuilder)entityTypeBuilder)
                    .GetTargetEntityTypeBuilder(targetClrType, navigationPropertyInfo, ConfigurationSource.Convention);

                if (candidateTargetEntityTypeBuilder == null)
                {
                    continue;
                }

                var candidateTargetEntityType = candidateTargetEntityTypeBuilder.Metadata;
                if (candidateTargetEntityType.IsKeyless)
                {
                    continue;
                }

                if (entityType.Builder == null)
                {
                    foreach (var relationshipCandidate in relationshipCandidates.Values)
                    {
                        var targetType = relationshipCandidate.TargetTypeBuilder.Metadata;
                        if (targetType.Builder != null
                            && targetType.HasDefiningNavigation()
                            && targetType.DefiningEntityType.FindNavigation(targetType.DefiningNavigationName) == null)
                        {
                            targetType.Builder.ModelBuilder.HasNoEntityType(targetType);
                        }
                    }

                    return Array.Empty<RelationshipCandidate>();
                }

                if (model.FindIsOwnedConfigurationSource(targetClrType) == null)
                {
                    var targetOwnership = candidateTargetEntityType.FindOwnership();
                    if (targetOwnership != null
                        && (targetOwnership.PrincipalEntityType != entityType
                            || targetOwnership.PrincipalToDependent.Name != navigationPropertyInfo.GetSimpleMemberName())
                        && (ownership == null
                            || ownership.PrincipalEntityType != candidateTargetEntityType))
                    {
                        continue;
                    }
                }

                if (relationshipCandidates.TryGetValue(candidateTargetEntityType, out var existingCandidate))
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

                var navigations = new List<PropertyInfo>
                {
                    navigationPropertyInfo
                };
                var inverseNavigationCandidates = new List<PropertyInfo>();

                if (!entityType.IsKeyless)
                {
                    var inverseCandidates = GetNavigationCandidates(candidateTargetEntityType);
                    foreach (var inverseCandidateTuple in inverseCandidates)
                    {
                        var inversePropertyInfo = inverseCandidateTuple.Key;
                        var inverseTargetType = inverseCandidateTuple.Value;

                        if ((inverseTargetType != entityType.ClrType
                             && (!inverseTargetType.IsAssignableFrom(entityType.ClrType)
                                 || (model.FindIsOwnedConfigurationSource(targetClrType) == null
                                     && !candidateTargetEntityType.IsInOwnershipPath(entityType))))
                            || navigationPropertyInfo.IsSameAs(inversePropertyInfo)
                            || (ownership != null
                                && !candidateTargetEntityType.IsInOwnershipPath(entityType)
                                && (candidateTargetEntityType.IsOwned()
                                    || model.FindIsOwnedConfigurationSource(targetClrType) == null)
                                && (ownership.PrincipalEntityType != candidateTargetEntityType
                                    || ownership.PrincipalToDependent.Name != inversePropertyInfo.GetSimpleMemberName()))
                            || (entityType.HasDefiningNavigation()
                                && !candidateTargetEntityType.IsInDefinitionPath(entityType.ClrType)
                                && (entityType.DefiningEntityType != candidateTargetEntityType
                                    || entityType.DefiningNavigationName != inversePropertyInfo.GetSimpleMemberName()))
                            || !IsCandidateNavigationProperty(
                                candidateTargetEntityTypeBuilder, inversePropertyInfo.GetSimpleMemberName(), inversePropertyInfo))
                        {
                            continue;
                        }

                        if (!inverseNavigationCandidates.Contains(inversePropertyInfo))
                        {
                            inverseNavigationCandidates.Add(inversePropertyInfo);
                        }
                    }
                }

                relationshipCandidates[candidateTargetEntityType] =
                    new RelationshipCandidate(candidateTargetEntityTypeBuilder, navigations, inverseNavigationCandidates);
            }

            var candidates = new List<RelationshipCandidate>();
            foreach (var relationshipCandidate in relationshipCandidates.Values)
            {
                if (relationshipCandidate.TargetTypeBuilder.Metadata.Builder != null)
                {
                    candidates.Add(relationshipCandidate);
                    continue;
                }

                if (relationshipCandidate.NavigationProperties.Count > 1)
                {
                    continue;
                }

                // The entity type might have been converted to a weak entity type
                var actualTargetEntityTypeBuilder =
                    ((InternalEntityTypeBuilder)entityTypeBuilder).GetTargetEntityTypeBuilder(
                        relationshipCandidate.TargetTypeBuilder.Metadata.ClrType,
                        relationshipCandidate.NavigationProperties.Single(),
                        ConfigurationSource.Convention);

                if (actualTargetEntityTypeBuilder == null)
                {
                    continue;
                }

                candidates.Add(
                    new RelationshipCandidate(
                        actualTargetEntityTypeBuilder, relationshipCandidate.NavigationProperties,
                        relationshipCandidate.InverseProperties));
            }

            return candidates;
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
                    var existingNavigation = entityType.FindNavigation(navigationProperty.GetSimpleMemberName());
                    if (existingNavigation != null
                        && (existingNavigation.DeclaringEntityType != entityType
                            || existingNavigation.GetTargetType() != targetEntityType))
                    {
                        relationshipCandidate.NavigationProperties.Remove(navigationProperty);
                        continue;
                    }

                    if (relationshipCandidate.NavigationProperties.Count == 1
                        && relationshipCandidate.InverseProperties.Count == 0)
                    {
                        break;
                    }

                    PropertyInfo compatibleInverse = null;
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
                                new List<PropertyInfo>
                                {
                                    navigationProperty
                                },
                                new List<PropertyInfo>()));

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
                    foreach (var n in relationshipCandidate.NavigationProperties)
                    {
                        if (n != navigationProperty
                            && IsCompatibleInverse(n, compatibleInverse, entityTypeBuilder, targetEntityTypeBuilder))
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
                                new List<PropertyInfo>
                                {
                                    navigationProperty
                                },
                                new List<PropertyInfo>
                                {
                                    compatibleInverse
                                })
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
                else if (IsCandidateUnusedOwnedType(relationshipCandidate.TargetTypeBuilder.Metadata)
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

                var otherEntityType = existingInverse.GetTargetType();
                if (!entityType.ClrType.GetTypeInfo()
                    .IsAssignableFrom(otherEntityType.ClrType.GetTypeInfo()))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CanMergeWith(
            IConventionNavigation existingNavigation, MemberInfo inverse, IConventionEntityTypeBuilder inverseEntityTypeBuilder)
        {
            var fk = existingNavigation.ForeignKey;
            return (fk.IsSelfReferencing()
                    || fk.GetRelatedEntityType(existingNavigation.DeclaringEntityType) == inverseEntityTypeBuilder.Metadata)
                   && fk.Builder.CanSetNavigation(inverse, !existingNavigation.IsDependentToPrincipal());
        }

        private static IReadOnlyList<RelationshipCandidate> RemoveInheritedInverseNavigations(
            IReadOnlyList<RelationshipCandidate> relationshipCandidates)
        {
            if (relationshipCandidates.Count == 0)
            {
                return relationshipCandidates;
            }

            var relationshipCandidatesByRoot = relationshipCandidates.GroupBy(r => r.TargetTypeBuilder.Metadata.RootType())
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
                        .Any(n => n.FindInverse() != null))
                    {
                        relationshipCandidate.NavigationProperties.Remove(navigation);
                    }
                }

                if (relationshipCandidate.NavigationProperties.Count > 0)
                {
                    filteredRelationshipCandidates.Add(relationshipCandidate);
                }
                else if (IsCandidateUnusedOwnedType(relationshipCandidate.TargetTypeBuilder.Metadata)
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
            IEnumerable<RelationshipCandidate> relationshipCandidates, IConventionEntityTypeBuilder entityTypeBuilder)
        {
            var unusedEntityTypes = new List<IConventionEntityType>();
            foreach (var relationshipCandidate in relationshipCandidates)
            {
                var entityType = entityTypeBuilder.Metadata;
                var targetEntityType = relationshipCandidate.TargetTypeBuilder.Metadata;
                var isAmbiguousOnBase = entityType.BaseType != null
                                        && HasAmbiguousNavigationsTo(
                                            entityType.BaseType, targetEntityType.ClrType)
                                        || targetEntityType.BaseType != null
                                        && HasAmbiguousNavigationsTo(
                                            targetEntityType.BaseType, entityType.ClrType);

                var ambiguousOwnership = relationshipCandidate.NavigationProperties.Count == 1
                                         && relationshipCandidate.InverseProperties.Count == 1
                                         && entityType.GetConfigurationSource() != ConfigurationSource.Explicit
                                         && targetEntityType.GetConfigurationSource() != ConfigurationSource.Explicit
                                         && targetEntityType.Model.IsOwned(entityType.ClrType)
                                         && targetEntityType.Model.IsOwned(targetEntityType.ClrType);

                if (ambiguousOwnership)
                {
                    var existingNavigation =
                        entityType.FindNavigation(relationshipCandidate.NavigationProperties.Single().GetSimpleMemberName());
                    if (existingNavigation != null
                        && existingNavigation.ForeignKey.DeclaringEntityType == targetEntityType
                        && existingNavigation.ForeignKey.GetPrincipalEndConfigurationSource()
                            .OverridesStrictly(ConfigurationSource.Convention))
                    {
                        ambiguousOwnership = false;
                    }
                    else
                    {
                        var existingInverse =
                            targetEntityType.FindNavigation(relationshipCandidate.InverseProperties.Single().GetSimpleMemberName());
                        if (existingInverse != null
                            && existingInverse.ForeignKey.PrincipalEntityType == targetEntityType
                            && existingInverse.ForeignKey.GetPrincipalEndConfigurationSource()
                                .OverridesStrictly(ConfigurationSource.Convention))
                        {
                            ambiguousOwnership = false;
                        }
                    }
                }

                if ((relationshipCandidate.NavigationProperties.Count > 1
                     && relationshipCandidate.InverseProperties.Count > 0
                     && (!targetEntityType.Model.IsOwned(targetEntityType.ClrType)
                         || entityType.IsInOwnershipPath(targetEntityType)))
                    || relationshipCandidate.InverseProperties.Count > 1
                    || isAmbiguousOnBase
                    || ambiguousOwnership
                    || HasDeclaredAmbiguousNavigationsTo(entityType, targetEntityType.ClrType)
                    || HasDeclaredAmbiguousNavigationsTo(targetEntityType, entityType.ClrType))
                {
                    if (!isAmbiguousOnBase)
                    {
                        Dependencies.Logger.MultipleNavigationProperties(
                            relationshipCandidate.NavigationProperties.Count == 0
                                ? new[]
                                {
                                    new Tuple<MemberInfo, Type>(null, targetEntityType.ClrType)
                                }
                                : relationshipCandidate.NavigationProperties.Select(
                                    n => new Tuple<MemberInfo, Type>(n, entityType.ClrType)),
                            relationshipCandidate.InverseProperties.Count == 0
                                ? new[]
                                {
                                    new Tuple<MemberInfo, Type>(null, targetEntityType.ClrType)
                                }
                                : relationshipCandidate.InverseProperties.Select(
                                    n => new Tuple<MemberInfo, Type>(n, targetEntityType.ClrType)));
                    }

                    foreach (var navigationProperty in relationshipCandidate.NavigationProperties.ToList())
                    {
                        var existingNavigation = entityType.FindDeclaredNavigation(navigationProperty.GetSimpleMemberName());
                        if (existingNavigation != null
                            && existingNavigation.ForeignKey.DeclaringEntityType.Builder
                                .HasNoRelationship(existingNavigation.ForeignKey) == null
                            && existingNavigation.ForeignKey.Builder.HasNavigation(
                                (string)null, existingNavigation.IsDependentToPrincipal()) == null)
                        {
                            // Navigations of higher configuration source are not ambiguous
                            relationshipCandidate.NavigationProperties.Remove(navigationProperty);
                        }
                    }

                    foreach (var inverseProperty in relationshipCandidate.InverseProperties.ToList())
                    {
                        var existingInverse = targetEntityType.FindDeclaredNavigation(inverseProperty.GetSimpleMemberName());
                        if (existingInverse != null
                            && existingInverse.ForeignKey.DeclaringEntityType.Builder
                                .HasNoRelationship(existingInverse.ForeignKey) == null
                            && existingInverse.ForeignKey.Builder.HasNavigation(
                                (string)null, existingInverse.IsDependentToPrincipal()) == null)
                        {
                            // Navigations of higher configuration source are not ambiguous
                            relationshipCandidate.InverseProperties.Remove(inverseProperty);
                        }
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
                    if (targetEntityType.Builder == null
                        && !targetEntityType.Model.IsOwned(targetEntityType.ClrType))
                    {
                        continue;
                    }

                    if (InversePropertyAttributeConvention.IsAmbiguous(entityType, navigation, targetEntityType))
                    {
                        unusedEntityTypes.Add(targetEntityType);
                        continue;
                    }

                    var targetOwned = targetEntityType.Model.IsOwned(targetEntityType.ClrType)
                                      && !entityType.IsInOwnershipPath(targetEntityType);

                    var inverse = relationshipCandidate.InverseProperties.SingleOrDefault();
                    if (inverse == null)
                    {
                        if (targetOwned)
                        {
                            entityTypeBuilder.HasOwnership(
                                targetEntityType.ClrType,
                                navigation);
                        }
                        else
                        {
                            entityTypeBuilder.HasRelationship(
                                targetEntityType,
                                navigation);
                        }
                    }
                    else
                    {
                        if (InversePropertyAttributeConvention.IsAmbiguous(targetEntityType, inverse, entityType))
                        {
                            unusedEntityTypes.Add(targetEntityType);
                            continue;
                        }

                        if (targetOwned
                            && entityType.Model.IsOwned(entityType.ClrType))
                        {
                            var existingInverse = targetEntityType.FindNavigation(inverse.GetSimpleMemberName());
                            if (inverse.PropertyType.TryGetSequenceType() != null
                                || targetEntityType.GetConfigurationSource() == ConfigurationSource.Explicit
                                || (existingInverse != null
                                    && existingInverse.ForeignKey.DeclaringEntityType == entityType
                                    && existingInverse.ForeignKey.GetPrincipalEndConfigurationSource()
                                        .OverridesStrictly(ConfigurationSource.Convention)))
                            {
                                // Target type is the principal, so the ownership should be configured from the other side
                                targetOwned = false;
                            }
                        }

                        if (targetOwned)
                        {
                            entityTypeBuilder.HasOwnership(
                                targetEntityType.ClrType,
                                navigation,
                                inverse);
                        }
                        else
                        {
                            entityTypeBuilder.HasRelationship(
                                targetEntityType,
                                navigation,
                                inverse);
                        }
                    }
                }

                if (relationshipCandidate.NavigationProperties.Count == 0)
                {
                    if (relationshipCandidate.InverseProperties.Count == 0
                        || targetEntityType.Model.IsOwned(targetEntityType.ClrType))
                    {
                        unusedEntityTypes.Add(targetEntityType);
                    }
                    else
                    {
                        foreach (var inverse in relationshipCandidate.InverseProperties)
                        {
                            if (targetEntityType.Builder == null)
                            {
                                continue;
                            }

                            if (InversePropertyAttributeConvention.IsAmbiguous(targetEntityType, inverse, entityType))
                            {
                                unusedEntityTypes.Add(targetEntityType);
                                continue;
                            }

                            targetEntityType.Builder.HasRelationship(
                                entityTypeBuilder.Metadata,
                                inverse);
                        }
                    }
                }
            }

            foreach (var unusedEntityType in unusedEntityTypes)
            {
                if (IsCandidateUnusedOwnedType(unusedEntityType)
                    && unusedEntityType.DefiningEntityType.FindNavigation(unusedEntityType.DefiningNavigationName) == null)
                {
                    entityTypeBuilder.ModelBuilder.HasNoEntityType(unusedEntityType);
                }
            }
        }

        /// <summary>
        ///     Called after an entity type is added to the model.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeAdded(
            IConventionEntityTypeBuilder entityTypeBuilder, IConventionContext<IConventionEntityTypeBuilder> context)
            => DiscoverRelationships(entityTypeBuilder, context);

        /// <summary>
        ///     Called after the base type of an entity type changes.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="newBaseType"> The new base entity type. </param>
        /// <param name="oldBaseType"> The old base entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeBaseTypeChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionEntityType newBaseType,
            IConventionEntityType oldBaseType,
            IConventionContext<IConventionEntityType> context)
        {
            var oldBaseTypeBuilder = oldBaseType?.Builder;
            if (oldBaseTypeBuilder != null)
            {
                DiscoverRelationships(oldBaseTypeBuilder, context);
            }

            if (entityTypeBuilder.Metadata.BaseType != newBaseType)
            {
                return;
            }

            ApplyOnRelatedEntityTypes(entityTypeBuilder.Metadata, context);
            foreach (var entityType in entityTypeBuilder.Metadata.GetDerivedTypesInclusive())
            {
                DiscoverRelationships(entityType.Builder, context);
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
                var relatedEntityTypeBuilder = relatedEntityType.Builder;
                DiscoverRelationships(relatedEntityTypeBuilder, context);
            }
        }

        /// <summary>
        ///     Called after a navigation is removed from the entity type.
        /// </summary>
        /// <param name="sourceEntityTypeBuilder"> The builder for the entity type that contained the navigation. </param>
        /// <param name="targetEntityTypeBuilder"> The builder for the target entity type of the navigation. </param>
        /// <param name="navigationName"> The navigation name. </param>
        /// <param name="memberInfo"> The member used for by the navigation. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessNavigationRemoved(
            IConventionEntityTypeBuilder sourceEntityTypeBuilder,
            IConventionEntityTypeBuilder targetEntityTypeBuilder,
            string navigationName,
            MemberInfo memberInfo,
            IConventionContext<string> context)
        {
            if ((targetEntityTypeBuilder.Metadata.Builder != null
                 || !sourceEntityTypeBuilder.ModelBuilder.IsIgnored(targetEntityTypeBuilder.Metadata.Name))
                && IsCandidateNavigationProperty(sourceEntityTypeBuilder, navigationName, memberInfo))
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

        [ContractAnnotation("memberInfo:null => false")]
        private static bool IsCandidateNavigationProperty(
            IConventionEntityTypeBuilder sourceEntityTypeBuilder, string navigationName, MemberInfo memberInfo)
            => memberInfo != null
               && sourceEntityTypeBuilder?.IsIgnored(navigationName) == false
               && sourceEntityTypeBuilder.Metadata.FindProperty(navigationName) == null
               && sourceEntityTypeBuilder.Metadata.FindServiceProperty(navigationName) == null
               && (!(memberInfo is PropertyInfo propertyInfo) || propertyInfo.GetIndexParameters().Length == 0)
               && (!sourceEntityTypeBuilder.Metadata.IsKeyless
                   || (memberInfo as PropertyInfo)?.PropertyType.TryGetSequenceType() == null);

        /// <summary>
        ///     Called after an entity type member is ignored.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="name"> The name of the ignored member. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeMemberIgnored(
            IConventionEntityTypeBuilder entityTypeBuilder, string name, IConventionContext<string> context)
        {
            var anyAmbiguityRemoved = false;
            foreach (var derivedEntityType in entityTypeBuilder.Metadata.GetDerivedTypesInclusive())
            {
                var ambiguousNavigations = GetAmbiguousNavigations(derivedEntityType);
                if (ambiguousNavigations == null)
                {
                    continue;
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
                    continue;
                }

                anyAmbiguityRemoved = true;

                var targetClrType = ambiguousNavigation.Value.Value;
                RemoveAmbiguous(derivedEntityType, targetClrType);

                var targetType = ((InternalEntityTypeBuilder)entityTypeBuilder)
                    .GetTargetEntityTypeBuilder(targetClrType, ambiguousNavigation.Value.Key, null)?.Metadata;
                if (targetType != null)
                {
                    RemoveAmbiguous(targetType, derivedEntityType.ClrType);
                }
            }

            if (anyAmbiguityRemoved)
            {
                DiscoverRelationships(entityTypeBuilder, context);
            }
        }

        /// <summary>
        ///     Called after a navigation is added to the entity type.
        /// </summary>
        /// <param name="relationshipBuilder"> The builder for the foreign key. </param>
        /// <param name="navigation"> The navigation. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessNavigationAdded(
            IConventionRelationshipBuilder relationshipBuilder,
            IConventionNavigation navigation,
            IConventionContext<IConventionNavigation> context)
        {
            foreach (var entityType in navigation.DeclaringEntityType.GetDerivedTypesInclusive())
            {
                // Only run the convention if an ambiguity might have been removed
                var ambiguityRemoved = RemoveAmbiguous(entityType, navigation.GetTargetType().ClrType);
                var targetAmbiguityRemoved = RemoveAmbiguous(navigation.GetTargetType(), entityType.ClrType);

                if (ambiguityRemoved)
                {
                    DiscoverRelationships(entityType.Builder, context);
                }

                if (targetAmbiguityRemoved)
                {
                    DiscoverRelationships(navigation.GetTargetType().Builder, context);
                }
            }

            if (relationshipBuilder.Metadata.Builder == null)
            {
                context.StopProcessing();
            }
        }

        /// <summary>
        ///     Called after the ownership value for a foreign key is changed.
        /// </summary>
        /// <param name="relationshipBuilder"> The builder for the foreign key. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessForeignKeyOwnershipChanged(
            IConventionRelationshipBuilder relationshipBuilder,
            IConventionContext<IConventionRelationshipBuilder> context)
            => DiscoverRelationships(relationshipBuilder.Metadata.DeclaringEntityType.Builder, context);

        private Type FindCandidateNavigationPropertyType([NotNull] PropertyInfo propertyInfo)
            => Dependencies.MemberClassifier.FindCandidateNavigationPropertyType(propertyInfo);

        private ImmutableSortedDictionary<PropertyInfo, Type> GetNavigationCandidates(IConventionEntityType entityType)
        {
            if (entityType.FindAnnotation(CoreAnnotationNames.NavigationCandidates)?.Value
                is ImmutableSortedDictionary<PropertyInfo, Type> navigationCandidates)
            {
                return navigationCandidates;
            }

            var dictionaryBuilder = ImmutableSortedDictionary.CreateBuilder<PropertyInfo, Type>(MemberInfoNameComparer.Instance);
            if (entityType.HasClrType())
            {
                foreach (var propertyInfo in entityType.GetRuntimeProperties().Values.OrderBy(p => p.Name))
                {
                    var targetType = FindCandidateNavigationPropertyType(propertyInfo);
                    if (targetType != null)
                    {
                        dictionaryBuilder[propertyInfo] = targetType;
                    }
                }
            }

            navigationCandidates = dictionaryBuilder.ToImmutable();
            SetNavigationCandidates(entityType.Builder, navigationCandidates);
            return navigationCandidates;
        }

        private static void SetNavigationCandidates(
            IConventionEntityTypeBuilder entityTypeBuilder,
            ImmutableSortedDictionary<PropertyInfo, Type> navigationCandidates)
            => entityTypeBuilder.HasAnnotation(CoreAnnotationNames.NavigationCandidates, navigationCandidates);

        private static bool IsCandidateUnusedOwnedType(IConventionEntityType entityType)
            => entityType.HasDefiningNavigation() && !entityType.GetForeignKeys().Any();

        private static bool IsAmbiguous(IConventionEntityType entityType, MemberInfo navigationProperty)
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

        private static bool HasAmbiguousNavigationsTo(IConventionEntityType sourceEntityType, Type targetClrType)
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

        private static ImmutableSortedDictionary<MemberInfo, Type> GetAmbiguousNavigations(IConventionEntityType entityType)
            => entityType.FindAnnotation(CoreAnnotationNames.AmbiguousNavigations)?.Value
                as ImmutableSortedDictionary<MemberInfo, Type>;

        private static void AddAmbiguous(
            IConventionEntityTypeBuilder entityTypeBuilder, IReadOnlyList<PropertyInfo> navigationProperties, Type targetType)
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

        private class MemberInfoNameComparer : IComparer<MemberInfo>
        {
            public static readonly MemberInfoNameComparer Instance = new MemberInfoNameComparer();

            private MemberInfoNameComparer()
            {
            }

            public int Compare(MemberInfo x, MemberInfo y) => StringComparer.Ordinal.Compare(x.Name, y.Name);
        }

        private class RelationshipCandidate
        {
            public RelationshipCandidate(
                IConventionEntityTypeBuilder targetTypeBuilder,
                List<PropertyInfo> navigations,
                List<PropertyInfo> inverseNavigations)
            {
                TargetTypeBuilder = targetTypeBuilder;
                NavigationProperties = navigations;
                InverseProperties = inverseNavigations;
            }

            public IConventionEntityTypeBuilder TargetTypeBuilder { [DebuggerStepThrough] get; }
            public List<PropertyInfo> NavigationProperties { [DebuggerStepThrough] get; }
            public List<PropertyInfo> InverseProperties { [DebuggerStepThrough] get; }
        }
    }
}
