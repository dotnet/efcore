// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.ModelConventions
{
    public class RelationshipDiscoveryConvention : IEntityTypeConvention
    {
        public virtual InternalEntityBuilder Apply(InternalEntityBuilder entityBuilder)
        {
            Check.NotNull(entityBuilder, nameof(entityBuilder));
            var entityType = entityBuilder.Metadata;

            var navigationPairCandidates = new Dictionary<InternalEntityBuilder, Tuple<List<PropertyInfo>, List<PropertyInfo>>>();
            if (entityType.HasClrType)
            {
                foreach (var navigationPropertyInfo in entityType.Type.GetRuntimeProperties().OrderBy(p => p.Name))
                {
                    Type entityClrType;
                    if (!navigationPropertyInfo.IsCandidateNavigationProperty(out entityClrType)
                        || !entityBuilder.CanAddNavigation(navigationPropertyInfo.Name, ConfigurationSource.Convention))
                    {
                        continue;
                    }

                    var targetEntityTypeBuilder = entityBuilder.ModelBuilder.Entity(entityClrType, ConfigurationSource.Convention);
                    if (targetEntityTypeBuilder == null)
                    {
                        continue;
                    }

                    // The navigation could have been added when the target entity type was added
                    if (!entityBuilder.CanAddNavigation(navigationPropertyInfo.Name, ConfigurationSource.Convention))
                    {
                        continue;
                    }

                    if (navigationPairCandidates.ContainsKey(targetEntityTypeBuilder))
                    {
                        if (entityType != targetEntityTypeBuilder.Metadata
                            || !navigationPairCandidates[targetEntityTypeBuilder].Item2.Contains(navigationPropertyInfo))
                        {
                            navigationPairCandidates[targetEntityTypeBuilder].Item1.Add(navigationPropertyInfo);
                        }
                        continue;
                    }

                    var navigations = new List<PropertyInfo> { navigationPropertyInfo };
                    var reverseNavigations = new List<PropertyInfo>();

                    navigationPairCandidates[targetEntityTypeBuilder] =
                        new Tuple<List<PropertyInfo>, List<PropertyInfo>>(navigations, reverseNavigations);
                    foreach (var reversePropertyInfo in targetEntityTypeBuilder.Metadata.Type.GetRuntimeProperties().OrderBy(p => p.Name))
                    {
                        Type reverseEntityClrType;
                        if (!reversePropertyInfo.IsCandidateNavigationProperty(out reverseEntityClrType)
                            || !targetEntityTypeBuilder.CanAddNavigation(reversePropertyInfo.Name, ConfigurationSource.Convention)
                            || entityType.Type != reverseEntityClrType
                            || navigationPropertyInfo == reversePropertyInfo)
                        {
                            continue;
                        }

                        reverseNavigations.Add(reversePropertyInfo);
                    }
                }

                foreach (var navigationPairCandidate in navigationPairCandidates)
                {
                    var targetEntityTypeBuilder = navigationPairCandidate.Key;
                    var navigationCandidates = navigationPairCandidate.Value.Item1;
                    var reverseNavigationCandidates = navigationPairCandidate.Value.Item2;

                    if (navigationCandidates.Count > 1
                        && reverseNavigationCandidates.Count > 0)
                    {
                        // Ambiguous navigations
                        return entityBuilder;
                    }

                    if (reverseNavigationCandidates.Count > 1)
                    {
                        // Ambiguous navigations
                        return entityBuilder;
                    }

                    foreach (var navigationCandidate in navigationCandidates)
                    {
                        TryBuildRelationship(entityBuilder, targetEntityTypeBuilder, navigationCandidate, reverseNavigationCandidates.SingleOrDefault());
                    }
                }
            }

            return entityBuilder;
        }

        private static void TryBuildRelationship(
            [NotNull] InternalEntityBuilder sourceBuilder,
            [NotNull] InternalEntityBuilder targetBuilder,
            [NotNull] PropertyInfo navigationToTarget,
            [CanBeNull] PropertyInfo navigationToSource)
        {
            var isToTargetNavigationCollection = navigationToTarget.PropertyType.TryGetSequenceType() != null;

            if (isToTargetNavigationCollection)
            {
                if (navigationToSource?.PropertyType.TryGetSequenceType() != null)
                {
                    // TODO: Support many to many
                    return;
                }

                targetBuilder.Relationship(
                    sourceBuilder,
                    targetBuilder,
                    navigationToSource?.Name,
                    navigationToTarget.Name,
                    configurationSource: ConfigurationSource.Convention, isUnique: false, strictPrincipal: true);
            }
            else
            {
                if (navigationToSource == null)
                {
                    targetBuilder.Relationship(
                        targetBuilder,
                        sourceBuilder,
                        navigationToTarget.Name,
                        navigationToDependentName: null,
                        configurationSource: ConfigurationSource.Convention, isUnique: null, strictPrincipal: false);
                }
                else
                {
                    if (navigationToSource.PropertyType.TryGetSequenceType() == null)
                    {
                        targetBuilder.Relationship(
                            sourceBuilder,
                            targetBuilder,
                            navigationToSource.Name,
                            navigationToTarget.Name,
                            configurationSource: ConfigurationSource.Convention, isUnique: true, strictPrincipal: false);
                    }
                }
            }
        }
    }
}
