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
    public class RelationshipDiscoveryConvention : IEntityTypeConvention
    {
        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            var entityType = entityTypeBuilder.Metadata;

            var navigationPairCandidates = new Dictionary<InternalEntityTypeBuilder, Tuple<List<PropertyInfo>, List<PropertyInfo>>>();
            if (entityType.HasClrType)
            {
                foreach (var navigationPropertyInfo in entityType.ClrType.GetRuntimeProperties().OrderBy(p => p.Name))
                {
                    var entityClrType = FindCandidateNavigationPropertyType(navigationPropertyInfo);
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
                    foreach (var reversePropertyInfo in targetEntityTypeBuilder.Metadata.ClrType.GetRuntimeProperties().OrderBy(p => p.Name))
                    {
                        var reverseEntityClrType = FindCandidateNavigationPropertyType(reversePropertyInfo);
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
                    var targetEntityTypeBuilder = navigationPairCandidate.Key;
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
                        TryBuildRelationship(entityTypeBuilder, targetEntityTypeBuilder, navigationCandidate, reverseNavigationCandidates.SingleOrDefault());
                    }
                }
            }

            return entityTypeBuilder;
        }

        protected virtual Type FindCandidateNavigationPropertyType([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            if (!propertyInfo.IsCandidateProperty())
            {
                return null;
            }

            var targetType = propertyInfo.PropertyType;
            targetType = targetType.TryGetSequenceType() ?? targetType;
            targetType = targetType.UnwrapNullableType();

            var typeInfo = targetType.GetTypeInfo();
            if (targetType.IsPrimitive()
                || typeInfo.IsValueType
                || typeInfo.IsAbstract
                || typeInfo.IsInterface)
            {
                return null;
            }

            return targetType;
        }

        private static void TryBuildRelationship(
            [NotNull] InternalEntityTypeBuilder sourceBuilder,
            [NotNull] InternalEntityTypeBuilder targetBuilder,
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
