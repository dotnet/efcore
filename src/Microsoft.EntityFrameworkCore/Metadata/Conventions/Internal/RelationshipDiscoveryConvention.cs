// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RelationshipDiscoveryConvention :
        IEntityTypeConvention,
        IBaseTypeConvention,
        INavigationRemovedConvention,
        IEntityTypeMemberIgnoredConvention,
        INavigationConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public const string NavigationCandidatesAnnotationName = "RelationshipDiscoveryConvention:NavigationCandidates";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
            var relationshipCandidates = new Dictionary<Type, RelationshipCandidate>();
            var navigationCandidates = GetNavigationCandidates(entityTypeBuilder)
                .AddRange(entityTypeBuilder.Metadata.GetNavigations()
                    .Select(n => new KeyValuePair<PropertyInfo, Type>(n.PropertyInfo, n.GetTargetType().ClrType)));
            foreach (var candidateTuple in navigationCandidates)
            {
                var navigationPropertyInfo = candidateTuple.Key;
                var targetClrType = candidateTuple.Value;

                var candidateTargetEntityTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(targetClrType, ConfigurationSource.Convention);
                if (candidateTargetEntityTypeBuilder == null)
                {
                    continue;
                }

                var entityType = entityTypeBuilder.Metadata;
                RelationshipCandidate existingCandidate;
                if (relationshipCandidates.TryGetValue(targetClrType, out existingCandidate))
                {
                    if (candidateTargetEntityTypeBuilder.Metadata != entityType
                        || !existingCandidate.InverseProperties.Contains(navigationPropertyInfo))
                    {
                        existingCandidate.NavigationProperties.Add(navigationPropertyInfo);
                    }

                    continue;
                }

                var navigations = new HashSet<PropertyInfo> { navigationPropertyInfo };
                var inverseCandidates = GetNavigationCandidates(candidateTargetEntityTypeBuilder)
                    .AddRange(candidateTargetEntityTypeBuilder.Metadata.GetNavigations()
                        .Select(n => new KeyValuePair<PropertyInfo, Type>(n.PropertyInfo, n.GetTargetType().ClrType)));
                var inverseNavigationCandidates = new HashSet<PropertyInfo>();
                foreach (var inverseCandidateTuple in inverseCandidates)
                {
                    var inversePropertyInfo = inverseCandidateTuple.Key;
                    var inverseTargetType = inverseCandidateTuple.Value;
                    if (inverseTargetType != entityType.ClrType
                        || navigationPropertyInfo == inversePropertyInfo)
                    {
                        continue;
                    }

                    inverseNavigationCandidates.Add(inversePropertyInfo);
                }

                relationshipCandidates[targetClrType] =
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

                        if (compatibleInverseProperties.Count == 1
                            && (relationshipCandidate.NavigationProperties.Count == 1
                                || (existingInverse != null
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
                Debug.Assert(relationshipCandidate.NavigationProperties.Count > 0);
                if ((relationshipCandidate.NavigationProperties.Count > 1
                     && relationshipCandidate.InverseProperties.Count > 0)
                    || relationshipCandidate.InverseProperties.Count > 1)
                {
                    foreach (var navigationProperty in relationshipCandidate.NavigationProperties)
                    {
                        var existingForeignKey = entityTypeBuilder.Metadata.FindDeclaredNavigation(navigationProperty.Name)?.ForeignKey;
                        existingForeignKey?.DeclaringEntityType.Builder
                            .RemoveForeignKey(existingForeignKey, ConfigurationSource.Convention);
                    }

                    foreach (var inverseProperty in relationshipCandidate.InverseProperties)
                    {
                        var existingForeignKey = relationshipCandidate.TargetTypeBuilder.Metadata
                            .FindDeclaredNavigation(inverseProperty.Name)?.ForeignKey;
                        existingForeignKey?.DeclaringEntityType.Builder
                            .RemoveForeignKey(existingForeignKey, ConfigurationSource.Convention);
                    }

                    continue;
                }

                foreach (var navigation in relationshipCandidate.NavigationProperties)
                {
                    var inverse = relationshipCandidate.InverseProperties.SingleOrDefault();
                    if (inverse == null)
                    {
                        entityTypeBuilder.Navigation(
                            relationshipCandidate.TargetTypeBuilder,
                            navigation,
                            ConfigurationSource.Convention);
                    }
                    else
                    {
                        entityTypeBuilder.Relationship(
                            relationshipCandidate.TargetTypeBuilder,
                            navigation,
                            inverse,
                            ConfigurationSource.Convention);
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
        {
            foreach (var entityType in entityTypeBuilder.Metadata.GetDerivedTypesInclusive())
            {
                SetNavigationCandidates(entityType.Builder, null);
            }

            var oldBaseTypeBuilder = oldBaseType?.Builder;
            if (oldBaseTypeBuilder != null)
            {
                ApplyOnRelatedEntityTypes(oldBaseTypeBuilder.Metadata);
                Apply(oldBaseTypeBuilder);
            }

            ApplyOnRelatedEntityTypes(entityTypeBuilder.Metadata);
            return Apply(entityTypeBuilder) != null;
        }

        private void ApplyOnRelatedEntityTypes(EntityType entityType)
        {
            var relatedEntityTypes = entityType.GetReferencingForeignKeys().Select(fk => fk.DeclaringEntityType)
                .Concat(entityType.GetForeignKeys().Select(fk => fk.PrincipalEntityType))
                .Distinct()
                .ToList();

            foreach (var relatedEntityType in relatedEntityTypes)
            {
                var relatedEntityTypeBuilder = relatedEntityType.Builder;
                Apply(relatedEntityTypeBuilder);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(
            InternalEntityTypeBuilder sourceEntityTypeBuilder,
            InternalEntityTypeBuilder targetEntityTypeBuilder,
            string navigationName,
            PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                return true;
            }

            sourceEntityTypeBuilder = sourceEntityTypeBuilder.Metadata.Builder;
            if (sourceEntityTypeBuilder == null
                || sourceEntityTypeBuilder.ModelBuilder.IsIgnored(sourceEntityTypeBuilder.Metadata.Name, ConfigurationSource.Convention))
            {
                return true;
            }

            if (sourceEntityTypeBuilder.IsIgnored(navigationName, ConfigurationSource.Convention))
            {
                return true;
            }

            foreach (var entityType in sourceEntityTypeBuilder.Metadata.GetDerivedTypesInclusive())
            {
                if (entityType.FindNavigation(navigationName) != null)
                {
                    continue;
                }

                var entityTypeBuilder = entityType.Builder;
                var navigationCandidates = GetNavigationCandidates(entityTypeBuilder);
                navigationCandidates = navigationCandidates.Add(propertyInfo, FindCandidateNavigationPropertyType(propertyInfo));
                SetNavigationCandidates(entityTypeBuilder, navigationCandidates);
                Apply(entityTypeBuilder);
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, string ignoredMemberName)
        {
            var navigationCandidates = GetNavigationCandidates(entityTypeBuilder);
            var candidate = navigationCandidates.Keys.FirstOrDefault(p => p.Name == ignoredMemberName);
            if (candidate == null)
            {
                return true;
            }

            navigationCandidates = navigationCandidates.Remove(candidate);
            SetNavigationCandidates(entityTypeBuilder, navigationCandidates);
            Apply(entityTypeBuilder);
            return entityTypeBuilder.IsIgnored(ignoredMemberName, ConfigurationSource.Convention);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder, Navigation navigation)
        {
            foreach (var entityType in navigation.DeclaringEntityType.GetDerivedTypesInclusive())
            {
                var entityTypeBuilder = entityType.Builder;
                var navigationCandidates = GetNavigationCandidates(entityTypeBuilder);
                var targetType = navigation.GetTargetType().ClrType;
                var candidate = navigationCandidates.Keys.FirstOrDefault(p => p.Name == navigation.Name);
                if (candidate != null)
                {
                    navigationCandidates = navigationCandidates.Remove(candidate);
                    SetNavigationCandidates(entityTypeBuilder, navigationCandidates);
                }

                // Only run the convention if an ambiguity might have been removed
                if (navigationCandidates.ContainsValue(targetType))
                {
                    Apply(entityTypeBuilder);
                }
            }

            if (relationshipBuilder.Metadata.Builder == null)
            {
                relationshipBuilder = navigation.DeclaringEntityType.FindNavigation(navigation.Name)?.ForeignKey?.Builder;
            }

            return relationshipBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Type FindCandidateNavigationPropertyType([NotNull] PropertyInfo propertyInfo)
            => Check.NotNull(propertyInfo, nameof(propertyInfo)).FindCandidateNavigationPropertyType(SharedTypeExtensions.IsPrimitive);

        private ImmutableSortedDictionary<PropertyInfo, Type> GetNavigationCandidates(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var entityType = entityTypeBuilder.Metadata;
            var navigationCandidates = entityType.FindAnnotation(NavigationCandidatesAnnotationName)?.Value
                as ImmutableSortedDictionary<PropertyInfo, Type>;
            if (navigationCandidates == null)
            {
                var navigations = new HashSet<string>(entityTypeBuilder.Metadata.GetNavigations().Select(n => n.Name));
                var dictionaryBuilder = ImmutableSortedDictionary.CreateBuilder<PropertyInfo, Type>(PropertyInfoNameComparer.Instance);
                if (entityType.HasClrType())
                {
                    foreach (var propertyInfo in entityType.ClrType.GetRuntimeProperties().OrderBy(p => p.Name))
                    {
                        var targetType = FindCandidateNavigationPropertyType(propertyInfo);
                        if (targetType != null
                            && !entityTypeBuilder.IsIgnored(propertyInfo.Name, ConfigurationSource.Convention)
                            && !navigations.Contains(propertyInfo.Name))
                        {
                            dictionaryBuilder[propertyInfo] = targetType;
                        }
                    }
                }
                navigationCandidates = dictionaryBuilder.ToImmutable();
                SetNavigationCandidates(entityTypeBuilder, navigationCandidates);
            }
            return navigationCandidates;
        }

        private void SetNavigationCandidates(
            InternalEntityTypeBuilder entityTypeBuilder,
            ImmutableSortedDictionary<PropertyInfo, Type> navigationCandidates)
            => entityTypeBuilder.HasAnnotation(NavigationCandidatesAnnotationName, navigationCandidates, ConfigurationSource.Convention);

        private class PropertyInfoNameComparer : IComparer<PropertyInfo>
        {
            public static readonly PropertyInfoNameComparer Instance = new PropertyInfoNameComparer();

            private PropertyInfoNameComparer()
            {
            }

            public int Compare(PropertyInfo x, PropertyInfo y) => StringComparer.Ordinal.Compare(x.Name, y.Name);
        }

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
