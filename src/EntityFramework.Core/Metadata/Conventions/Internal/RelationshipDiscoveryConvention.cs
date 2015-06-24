// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class RelationshipDiscoveryConvention :
        IEntityTypeConvention, IEntityTypeMemberIgnoredConvention, INavigationRemovedConvention, IBaseTypeConvention
    {
        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            var entityType = entityTypeBuilder.Metadata;

            var navigationPairCandidates = new Dictionary<Type, Tuple<List<PropertyInfo>, List<PropertyInfo>>>();
            if (entityType.HasClrType)
            {
                foreach (var navigationPropertyInfo in entityType.ClrType.GetRuntimeProperties().OrderBy(p => p.Name))
                {
                    var entityClrType = navigationPropertyInfo.FindCandidateNavigationPropertyType();
                    if (entityClrType == null
                        || !entityTypeBuilder.CanAddNavigation(navigationPropertyInfo.Name, ConfigurationSource.Convention))
                    {
                        continue;
                    }

                    var targetEntityTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(entityClrType, ConfigurationSource.Convention);
                    if (targetEntityTypeBuilder == null)
                    {
                        continue;
                    }

                    // The navigation could have been added when the target entity type was added
                    if (!entityTypeBuilder.CanAddNavigation(navigationPropertyInfo.Name, ConfigurationSource.Convention))
                    {
                        continue;
                    }

                    if (navigationPairCandidates.ContainsKey(targetEntityTypeBuilder.Metadata.ClrType))
                    {
                        if (entityType != targetEntityTypeBuilder.Metadata
                            || !navigationPairCandidates[targetEntityTypeBuilder.Metadata.ClrType].Item2.Contains(navigationPropertyInfo))
                        {
                            navigationPairCandidates[targetEntityTypeBuilder.Metadata.ClrType].Item1.Add(navigationPropertyInfo);
                        }
                        continue;
                    }

                    var navigations = new List<PropertyInfo> { navigationPropertyInfo };
                    var reverseNavigations = new List<PropertyInfo>();

                    navigationPairCandidates[targetEntityTypeBuilder.Metadata.ClrType] =
                        new Tuple<List<PropertyInfo>, List<PropertyInfo>>(navigations, reverseNavigations);
                    foreach (var reversePropertyInfo in targetEntityTypeBuilder.Metadata.ClrType.GetRuntimeProperties().OrderBy(p => p.Name))
                    {
                        var reverseEntityClrType = reversePropertyInfo.FindCandidateNavigationPropertyType();
                        if (reverseEntityClrType == null
                            || !targetEntityTypeBuilder.CanAddNavigation(reversePropertyInfo.Name, ConfigurationSource.Convention)
                            || entityType.ClrType != reverseEntityClrType
                            || navigationPropertyInfo == reversePropertyInfo)
                        {
                            continue;
                        }

                        reverseNavigations.Add(reversePropertyInfo);
                    }
                }

                foreach (var navigationPairCandidate in navigationPairCandidates)
                {
                    var navigationCandidates = navigationPairCandidate.Value.Item1;
                    var reverseNavigationCandidates = navigationPairCandidate.Value.Item2;

                    if (navigationCandidates.Count > 1
                        && reverseNavigationCandidates.Count > 0)
                    {
                        // Ambiguous navigations
                        return entityTypeBuilder;
                    }

                    if (reverseNavigationCandidates.Count > 1)
                    {
                        // Ambiguous navigations
                        return entityTypeBuilder;
                    }

                    foreach (var navigationCandidate in navigationCandidates)
                    {
                        var targetEntityTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(
                            navigationCandidate.FindCandidateNavigationPropertyType(), ConfigurationSource.Convention);
                        targetEntityTypeBuilder.Relationship(
                            entityTypeBuilder,
                            reverseNavigationCandidates.SingleOrDefault(),
                            navigationCandidate,
                            ConfigurationSource.Convention);
                    }
                }
            }

            return entityTypeBuilder;
        }

        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, string ignoredMemberName)
            => Apply(entityTypeBuilder) != null;

        public virtual bool Apply(InternalRelationshipBuilder relationshipBuilder, string navigationName, bool pointsToPrincipal)
        {
            var owner = pointsToPrincipal
                ? relationshipBuilder.Metadata.DeclaringEntityType
                : relationshipBuilder.Metadata.PrincipalEntityType;
            return Apply(relationshipBuilder.ModelBuilder.Entity(owner.Name, ConfigurationSource.Convention)) != null;
        }

        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
            => Apply(entityTypeBuilder) != null;
    }
}
