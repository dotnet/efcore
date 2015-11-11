// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class RelationshipDiscoveryConvention : IEntityTypeConvention, IBaseTypeConvention, INavigationRemovedConvention, IEntityTypeMemberIgnoredConvention
    {
        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            if (!entityTypeBuilder.Metadata.HasClrType())
            {
                return entityTypeBuilder;
            }

            var relationshipCandidates = FindRelationshipCandidates(entityTypeBuilder);
            relationshipCandidates = RemoveIncompatibleWithExistingRelationships(relationshipCandidates, entityTypeBuilder);
            relationshipCandidates = RemoveInheritedInverseNavigations(relationshipCandidates);
            CreateRelationships(relationshipCandidates, entityTypeBuilder);

            return entityTypeBuilder;
        }

        private IReadOnlyList<RelationshipCandidate> FindRelationshipCandidates(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var entityType = entityTypeBuilder.Metadata;
            var relationshipCandidates = new Dictionary<Type, RelationshipCandidate>();
            foreach (var navigationPropertyInfo in entityType.ClrType.GetRuntimeProperties().OrderBy(p => p.Name))
            {
                var targetClrType = FindCandidateNavigationPropertyType(navigationPropertyInfo);
                var navigationName = navigationPropertyInfo.Name;
                if ((targetClrType == null)
                    || entityTypeBuilder.IsIgnored(navigationName, configurationSource: ConfigurationSource.Convention))
                {
                    continue;
                }

                var candidateTargetEntityTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(targetClrType, ConfigurationSource.Convention);
                if (candidateTargetEntityTypeBuilder == null)
                {
                    continue;
                }

                RelationshipCandidate existingCandidate;
                if (relationshipCandidates.TryGetValue(candidateTargetEntityTypeBuilder.Metadata.ClrType, out existingCandidate))
                {
                    if ((candidateTargetEntityTypeBuilder.Metadata != entityType)
                        || !existingCandidate.InverseProperties.Contains(navigationPropertyInfo))
                    {
                        existingCandidate.NavigationProperties.Add(navigationPropertyInfo);
                    }

                    continue;
                }

                var navigations = new HashSet<PropertyInfo> { navigationPropertyInfo };
                var inverseNavigationCandidates = new HashSet<PropertyInfo>();
                foreach (var inversePropertyInfo in candidateTargetEntityTypeBuilder.Metadata.ClrType.GetRuntimeProperties().OrderBy(p => p.Name))
                {
                    var inverseTargetType = FindCandidateNavigationPropertyType(inversePropertyInfo);
                    if ((inverseTargetType == null)
                        || !inverseTargetType.GetTypeInfo().IsAssignableFrom(entityType.ClrType.GetTypeInfo())
                        || (navigationPropertyInfo == inversePropertyInfo)
                        || candidateTargetEntityTypeBuilder.IsIgnored(inversePropertyInfo.Name, ConfigurationSource.Convention))
                    {
                        continue;
                    }

                    inverseNavigationCandidates.Add(inversePropertyInfo);
                }

                relationshipCandidates[candidateTargetEntityTypeBuilder.Metadata.ClrType] =
                    new RelationshipCandidate(candidateTargetEntityTypeBuilder, navigations, inverseNavigationCandidates);
            }

            return relationshipCandidates.Values.ToList();
        }

        private static IReadOnlyList<RelationshipCandidate> RemoveIncompatibleWithExistingRelationships(
            IReadOnlyList<RelationshipCandidate> relationshipCandidates,
            InternalEntityTypeBuilder entityTypeBuilder)
        {
            var filteredRelationshipCandidates = new List<RelationshipCandidate>();
            foreach (var relationshipCandidate in relationshipCandidates)
            {
                var targetEntityTypeBuilder = relationshipCandidate.TargetTypeBuilder;
                var revisitNavigations = true;
                while (revisitNavigations)
                {
                    revisitNavigations = false;
                    foreach (var navigationProperty in relationshipCandidate.NavigationProperties)
                    {
                        var existingNavigation = entityTypeBuilder.Metadata.FindNavigation(navigationProperty.Name);
                        if ((existingNavigation != null)
                            && ((existingNavigation.DeclaringEntityType != entityTypeBuilder.Metadata)
                                || (existingNavigation.GetTargetType() != targetEntityTypeBuilder.Metadata)))
                        {
                            relationshipCandidate.NavigationProperties.Remove(navigationProperty);
                            revisitNavigations = true;
                            break;
                        }

                        var compatibleInverseProperties = new List<PropertyInfo>();
                        Navigation existingInverse = null;
                        foreach (var inversePropertyInfo in relationshipCandidate.InverseProperties)
                        {
                            if ((existingNavigation != null)
                                && !CanMergeWith(existingNavigation, inversePropertyInfo.Name, targetEntityTypeBuilder))
                            {
                                continue;
                            }

                            existingInverse = targetEntityTypeBuilder.Metadata.FindNavigation(inversePropertyInfo.Name);
                            if (existingInverse != null)
                            {
                                if ((existingInverse.DeclaringEntityType != targetEntityTypeBuilder.Metadata)
                                    || !CanMergeWith(existingInverse, navigationProperty.Name, entityTypeBuilder))
                                {
                                    continue;
                                }

                                var otherEntityType = existingInverse.ForeignKey.ResolveOtherEntityType(
                                    existingInverse.DeclaringEntityType);
                                if (!entityTypeBuilder.Metadata.ClrType.GetTypeInfo()
                                    .IsAssignableFrom(otherEntityType.ClrType.GetTypeInfo()))
                                {
                                    continue;
                                }
                            }

                            compatibleInverseProperties.Add(inversePropertyInfo);
                        }

                        if (compatibleInverseProperties.Count == 0)
                        {
                            relationshipCandidate.NavigationProperties.Remove(navigationProperty);
                            revisitNavigations = true;
                            filteredRelationshipCandidates.Add(new RelationshipCandidate(
                                targetEntityTypeBuilder,
                                new HashSet<PropertyInfo> { navigationProperty },
                                new HashSet<PropertyInfo>()));
                            if ((relationshipCandidate.TargetTypeBuilder.Metadata == entityTypeBuilder.Metadata)
                                && (relationshipCandidate.InverseProperties.Count > 0))
                            {
                                var nextSelfRefCandidate = relationshipCandidate.InverseProperties.First();
                                relationshipCandidate.NavigationProperties.Add(nextSelfRefCandidate);
                                relationshipCandidate.InverseProperties.Remove(nextSelfRefCandidate);
                            }
                            break;
                        }

                        if ((compatibleInverseProperties.Count == 1)
                            && ((relationshipCandidate.NavigationProperties.Count == 1)
                                || ((existingInverse != null)
                                    && !CanMergeWith(existingInverse, null, entityTypeBuilder))))
                        {
                            var inverseProperty = compatibleInverseProperties[0];
                            relationshipCandidate.NavigationProperties.Remove(navigationProperty);
                            relationshipCandidate.InverseProperties.Remove(inverseProperty);
                            revisitNavigations = true;
                            filteredRelationshipCandidates.Add(new RelationshipCandidate(
                                targetEntityTypeBuilder,
                                new HashSet<PropertyInfo> { navigationProperty },
                                new HashSet<PropertyInfo> { inverseProperty }));
                            if ((relationshipCandidate.TargetTypeBuilder.Metadata == entityTypeBuilder.Metadata)
                                && (relationshipCandidate.InverseProperties.Count > 0))
                            {
                                var nextSelfRefCandidate = relationshipCandidate.InverseProperties.First();
                                relationshipCandidate.NavigationProperties.Add(nextSelfRefCandidate);
                                relationshipCandidate.InverseProperties.Remove(nextSelfRefCandidate);
                            }
                            break;
                        }
                    }
                }

                if (relationshipCandidate.NavigationProperties.Count > 0)
                {
                    filteredRelationshipCandidates.Add(relationshipCandidate);
                }
            }

            return filteredRelationshipCandidates;
        }

        private static bool CanMergeWith(
            Navigation existingNavigation, string inverseName, InternalEntityTypeBuilder inverseEntityTypeBuilder)
        {
            var fk = existingNavigation.ForeignKey;
            return (fk.IsSelfReferencing()
                    || (fk.ResolveOtherEntityType(existingNavigation.DeclaringEntityType) == inverseEntityTypeBuilder.Metadata))
                   && fk.Builder.CanSetNavigation(inverseName, !existingNavigation.IsDependentToPrincipal(), ConfigurationSource.Convention);
        }

        private IReadOnlyList<RelationshipCandidate> RemoveInheritedInverseNavigations(
            IReadOnlyList<RelationshipCandidate> relationshipCandidates)
        {
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

        private void RemoveInheritedInverseNavigations(
            RelationshipCandidate relationshipCandidate,
            List<RelationshipCandidate> relationshipCandidatesHierarchy,
            HashSet<RelationshipCandidate> filteredRelationshipCandidates)
        {
            if (filteredRelationshipCandidates.Contains(relationshipCandidate)
                || ((relationshipCandidate.NavigationProperties.Count > 1)
                    && (relationshipCandidate.InverseProperties.Count > 0))
                || (relationshipCandidate.InverseProperties.Count > 1))
            {
                return;
            }

            filteredRelationshipCandidates.Add(relationshipCandidate);
            var inverseCandidate = relationshipCandidate.InverseProperties.FirstOrDefault();
            if (inverseCandidate != null)
            {
                var relationshipsToDerivedTypes = relationshipCandidatesHierarchy
                    .Where(r => (r.TargetTypeBuilder != relationshipCandidate.TargetTypeBuilder)
                                && relationshipCandidate.TargetTypeBuilder.Metadata.IsAssignableFrom(r.TargetTypeBuilder.Metadata));
                foreach (var relationshipToDerivedType in relationshipsToDerivedTypes)
                {
                    relationshipToDerivedType.InverseProperties.RemoveWhere(i => i.Name == inverseCandidate.Name);

                    if (!filteredRelationshipCandidates.Contains(relationshipToDerivedType))
                    {
                        // An ambiguity might have been resolved
                        RemoveInheritedInverseNavigations(relationshipToDerivedType, relationshipCandidatesHierarchy, filteredRelationshipCandidates);
                    }
                }
            }
        }

        private static void CreateRelationships(
            IReadOnlyList<RelationshipCandidate> relationshipCandidates, InternalEntityTypeBuilder entityTypeBuilder)
        {
            foreach (var relationshipCandidate in relationshipCandidates)
            {
                if (((relationshipCandidate.NavigationProperties.Count > 1)
                     && (relationshipCandidate.InverseProperties.Count > 0))
                    || (relationshipCandidate.InverseProperties.Count > 1))
                {
                    foreach (var navigationProperty in relationshipCandidate.NavigationProperties)
                    {
                        var existingForeignKey = entityTypeBuilder.Metadata.FindDeclaredNavigation(navigationProperty.Name)?.ForeignKey;
                        if (existingForeignKey != null)
                        {
                            entityTypeBuilder.ModelBuilder.Entity(existingForeignKey.DeclaringEntityType.Name, ConfigurationSource.Convention)
                                .RemoveForeignKey(existingForeignKey, ConfigurationSource.Convention);
                        }
                    }

                    foreach (var inverseProperty in relationshipCandidate.InverseProperties)
                    {
                        var existingForeignKey = relationshipCandidate.TargetTypeBuilder.Metadata.FindDeclaredNavigation(inverseProperty.Name)?.ForeignKey;
                        if (existingForeignKey != null)
                        {
                            entityTypeBuilder.ModelBuilder.Entity(existingForeignKey.DeclaringEntityType.Name, ConfigurationSource.Convention)
                                .RemoveForeignKey(existingForeignKey, ConfigurationSource.Convention);
                        }
                    }

                    break;
                }

                foreach (var navigation in relationshipCandidate.NavigationProperties)
                {
                    var inverse = relationshipCandidate.InverseProperties.SingleOrDefault();
                    entityTypeBuilder.Relationship(
                        relationshipCandidate.TargetTypeBuilder,
                        navigation,
                        inverse,
                        ConfigurationSource.Convention);
                }
            }
        }

        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
        {
            if (oldBaseType != null)
            {
                var oldBaseTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(oldBaseType.Name, ConfigurationSource.Convention);
                if (oldBaseTypeBuilder != null)
                {
                    ApplyOnRelatedEntityTypes(entityTypeBuilder.ModelBuilder, oldBaseTypeBuilder.Metadata);
                    Apply(oldBaseTypeBuilder);
                }
            }

            ApplyOnRelatedEntityTypes(entityTypeBuilder.ModelBuilder, entityTypeBuilder.Metadata);
            return Apply(entityTypeBuilder) != null;
        }

        private void ApplyOnRelatedEntityTypes(InternalModelBuilder modelBuilder, EntityType entityType)
        {
            var relatedEntityTypes = entityType.GetReferencingForeignKeys().Select(fk => fk.DeclaringEntityType)
                .Concat(entityType.GetForeignKeys().Select(fk => fk.PrincipalEntityType))
                .Distinct()
                .ToList();

            foreach (var relatedEntityType in relatedEntityTypes)
            {
                var relatedEntityTypeBuilder = modelBuilder.Entity(relatedEntityType.Name, ConfigurationSource.Convention);
                Apply(relatedEntityTypeBuilder);
            }
        }

        public virtual bool Apply(
            InternalEntityTypeBuilder sourceEntityTypeBuilder,
            InternalEntityTypeBuilder targetEntityTypeBuilder,
            string navigationName)
        {
            sourceEntityTypeBuilder = sourceEntityTypeBuilder.ModelBuilder.Entity(sourceEntityTypeBuilder.Metadata.Name, ConfigurationSource.Convention);
            if (sourceEntityTypeBuilder == null)
            {
                return true;
            }

            if (sourceEntityTypeBuilder.IsIgnored(navigationName, ConfigurationSource.Convention))
            {
                return true;
            }

            Apply(sourceEntityTypeBuilder);

            foreach (var derivedType in sourceEntityTypeBuilder.Metadata.GetDerivedTypes())
            {
                Apply(sourceEntityTypeBuilder.ModelBuilder.Entity(derivedType.Name, ConfigurationSource.Convention));
            }

            return true;
        }

        public virtual Type FindCandidateNavigationPropertyType([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return propertyInfo.FindCandidateNavigationPropertyType(clrType => clrType.IsPrimitive());
        }

        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, string ignoredMemberName)
            => Apply(entityTypeBuilder) != null;

        private class RelationshipCandidate
        {
            public RelationshipCandidate(
                InternalEntityTypeBuilder targetTypeBuilder,
                HashSet<PropertyInfo> navigations,
                HashSet<PropertyInfo> inverseNavigations)
            {
                TargetTypeBuilder = targetTypeBuilder;
                NavigationProperties = navigations;
                InverseProperties = inverseNavigations;
            }

            public InternalEntityTypeBuilder TargetTypeBuilder { get; }
            public HashSet<PropertyInfo> NavigationProperties { get; }
            public HashSet<PropertyInfo> InverseProperties { get; }
        }
    }
}
