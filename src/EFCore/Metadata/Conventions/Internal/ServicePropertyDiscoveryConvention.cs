// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ServicePropertyDiscoveryConvention :
        IEntityTypeAddedConvention,
        IBaseTypeChangedConvention,
        IEntityTypeMemberIgnoredConvention,
        IModelBuiltConvention
    {
        private readonly ITypeMappingSource _typeMappingSource;
        private readonly IParameterBindingFactories _parameterBindingFactories;

        private const string DuplicateServicePropertiesAnnotationName = "RelationshipDiscoveryConvention:DuplicateServiceProperties";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ServicePropertyDiscoveryConvention(
            [NotNull] ITypeMappingSource typeMappingSource,
            [NotNull] IParameterBindingFactories parameterBindingFactories)
        {
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));
            Check.NotNull(parameterBindingFactories, nameof(parameterBindingFactories));

            _typeMappingSource = typeMappingSource;
            _parameterBindingFactories = parameterBindingFactories;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            var entityType = entityTypeBuilder.Metadata;

            if (!entityType.HasClrType())
            {
                return entityTypeBuilder;
            }

            var candidates = entityType.GetRuntimeProperties().Values;

            foreach (var propertyInfo in candidates)
            {
                if (entityTypeBuilder.IsIgnored(propertyInfo.GetSimpleMemberName(), ConfigurationSource.Convention)
                    || entityType.FindProperty(propertyInfo) != null
                    || entityType.FindNavigation(propertyInfo) != null
                    || !propertyInfo.IsCandidateProperty(publicOnly: false)
                    || (propertyInfo.IsCandidateProperty()
                        && _typeMappingSource.FindMapping(propertyInfo) != null))
                {
                    continue;
                }

                var factory = _parameterBindingFactories.FindFactory(propertyInfo.PropertyType, propertyInfo.GetSimpleMemberName());
                if (factory == null)
                {
                    continue;
                }

                var duplicateMap = GetDuplicateServiceProperties(entityType);
                if (duplicateMap != null
                    && duplicateMap.TryGetValue(propertyInfo.PropertyType, out var duplicateServiceProperties))
                {
                    duplicateServiceProperties.Add(propertyInfo);

                    return entityTypeBuilder;
                }

                var otherServicePropertySameType = entityType.GetServiceProperties()
                    .FirstOrDefault(p => p.ClrType == propertyInfo.PropertyType);
                if (otherServicePropertySameType != null)
                {
                    if (ConfigurationSource.Convention.Overrides(otherServicePropertySameType.GetConfigurationSource()))
                    {
                        otherServicePropertySameType.DeclaringEntityType.RemoveServiceProperty(otherServicePropertySameType.Name);
                    }

                    AddDuplicateServiceProperty(entityTypeBuilder, propertyInfo);
                    AddDuplicateServiceProperty(entityTypeBuilder, otherServicePropertySameType.GetIdentifyingMemberInfo());

                    return entityTypeBuilder;
                }

                entityTypeBuilder.ServiceProperty(propertyInfo, ConfigurationSource.Convention)?.SetParameterBinding(
                    (ServiceParameterBinding)factory.Bind(entityType, propertyInfo.PropertyType, propertyInfo.GetSimpleMemberName()),
                    ConfigurationSource.Convention);
            }

            return entityTypeBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
            => Apply(entityTypeBuilder) != null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, string ignoredMemberName)
        {
            var entityType = entityTypeBuilder.Metadata;
            var duplicateMap = GetDuplicateServiceProperties(entityType);
            if (duplicateMap == null)
            {
                return true;
            }

            var member = (MemberInfo)entityType.GetRuntimeProperties().Find(ignoredMemberName)
                         ?? entityType.GetRuntimeFields().Find(ignoredMemberName);
            var type = member.GetMemberType();
            if (duplicateMap.TryGetValue(type, out var duplicateServiceProperties)
                && duplicateServiceProperties.Remove(member))
            {
                if (duplicateServiceProperties.Count != 1)
                {
                    return true;
                }

                var otherMember = duplicateServiceProperties.First();
                var factory = _parameterBindingFactories.FindFactory(type, otherMember.GetSimpleMemberName());
                entityType.Builder.ServiceProperty(otherMember, ConfigurationSource.Convention)?.SetParameterBinding(
                    (ServiceParameterBinding)factory.Bind(entityType, type, otherMember.GetSimpleMemberName()),
                    ConfigurationSource.Convention);
                duplicateMap.Remove(type);
                if (duplicateMap.Count == 0)
                {
                    SetDuplicateServiceProperties(entityType.Builder, null);
                }

                return true;
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                var duplicateMap = GetDuplicateServiceProperties(entityType);
                if (duplicateMap == null)
                {
                    continue;
                }

                foreach (var duplicateServiceProperties in duplicateMap.Values)
                {
                    foreach (var duplicateServiceProperty in duplicateServiceProperties)
                    {
                        if (entityType.FindProperty(duplicateServiceProperty.GetSimpleMemberName()) == null
                            && entityType.FindNavigation(duplicateServiceProperty.GetSimpleMemberName()) == null)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.AmbiguousServiceProperty(
                                    duplicateServiceProperty.Name,
                                    duplicateServiceProperty.GetMemberType().ShortDisplayName(),
                                    entityType.DisplayName()));
                        }
                    }
                }

                SetDuplicateServiceProperties(entityType.Builder, null);
            }

            return modelBuilder;
        }

        private static void AddDuplicateServiceProperty(InternalEntityTypeBuilder entityTypeBuilder, MemberInfo serviceProperty)
        {
            var duplicateMap = GetDuplicateServiceProperties(entityTypeBuilder.Metadata)
                               ?? new Dictionary<Type, HashSet<MemberInfo>>(1);

            var type = serviceProperty.GetMemberType();
            if (!duplicateMap.TryGetValue(type, out var duplicateServiceProperties))
            {
                duplicateServiceProperties = new HashSet<MemberInfo>();
                duplicateMap[type] = duplicateServiceProperties;
            }

            duplicateServiceProperties.Add(serviceProperty);

            SetDuplicateServiceProperties(entityTypeBuilder, duplicateMap);
        }

        private static Dictionary<Type, HashSet<MemberInfo>> GetDuplicateServiceProperties(EntityType entityType)
            => entityType.FindAnnotation(DuplicateServicePropertiesAnnotationName)?.Value
                as Dictionary<Type, HashSet<MemberInfo>>;

        private static void SetDuplicateServiceProperties(
            InternalEntityTypeBuilder entityTypeBuilder,
            Dictionary<Type, HashSet<MemberInfo>> duplicateServiceProperties)
            => entityTypeBuilder.HasAnnotation(DuplicateServicePropertiesAnnotationName, duplicateServiceProperties, ConfigurationSource.Convention);
    }
}
