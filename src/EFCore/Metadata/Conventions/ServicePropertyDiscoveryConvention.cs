// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that adds service properties to entity types.
    /// </summary>
    public class ServicePropertyDiscoveryConvention :
        IEntityTypeAddedConvention,
        IEntityTypeBaseTypeChangedConvention,
        IEntityTypeMemberIgnoredConvention,
        IModelFinalizingConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ServicePropertyDiscoveryConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public ServicePropertyDiscoveryConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Called after an entity type is added to the model.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeAdded(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionContext<IConventionEntityTypeBuilder> context)
            => Process(entityTypeBuilder);

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
            if (entityTypeBuilder.Metadata.BaseType == newBaseType)
            {
                Process(entityTypeBuilder);
            }
        }

        private void Process(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            var entityType = entityTypeBuilder.Metadata;

            if (!entityType.HasClrType())
            {
                return;
            }

            var candidates = entityType.GetRuntimeProperties().Values;

            foreach (var propertyInfo in candidates)
            {
                var name = propertyInfo.GetSimpleMemberName();
                if (entityTypeBuilder.IsIgnored(name)
                    || entityType.FindProperty(propertyInfo) != null
                    || entityType.FindNavigation(propertyInfo) != null
                    || !propertyInfo.IsCandidateProperty(publicOnly: false)
                    || (propertyInfo.IsCandidateProperty()
                        && Dependencies.TypeMappingSource.FindMapping(propertyInfo) != null))
                {
                    continue;
                }

                var factory = Dependencies.ParameterBindingFactories.FindFactory(propertyInfo.PropertyType, name);
                if (factory == null)
                {
                    continue;
                }

                var duplicateMap = GetDuplicateServiceProperties(entityType);
                if (duplicateMap != null
                    && duplicateMap.TryGetValue(propertyInfo.PropertyType, out var duplicateServiceProperties))
                {
                    duplicateServiceProperties.Add(propertyInfo);

                    return;
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

                    return;
                }

                entityTypeBuilder.ServiceProperty(propertyInfo)?.HasParameterBinding(
                    (ServiceParameterBinding)factory.Bind(entityType, propertyInfo.PropertyType, propertyInfo.GetSimpleMemberName()));
            }
        }

        /// <summary>
        ///     Called after an entity type member is ignored.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="name"> The name of the ignored member. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeMemberIgnored(
            IConventionEntityTypeBuilder entityTypeBuilder,
            string name,
            IConventionContext<string> context)
        {
            var entityType = entityTypeBuilder.Metadata;
            var duplicateMap = GetDuplicateServiceProperties(entityType);
            if (duplicateMap == null)
            {
                return;
            }

            var member = (MemberInfo)entityType.GetRuntimeProperties().Find(name)
                ?? entityType.GetRuntimeFields().Find(name);
            var type = member.GetMemberType();
            if (duplicateMap.TryGetValue(type, out var duplicateServiceProperties)
                && duplicateServiceProperties.Remove(member))
            {
                if (duplicateServiceProperties.Count != 1)
                {
                    return;
                }

                var otherMember = duplicateServiceProperties.First();
                var otherName = otherMember.GetSimpleMemberName();
                var factory = Dependencies.ParameterBindingFactories.FindFactory(type, otherName);
                entityType.Builder.ServiceProperty(otherMember)?.HasParameterBinding(
                    (ServiceParameterBinding)factory.Bind(entityType, type, otherName));
                duplicateMap.Remove(type);
                if (duplicateMap.Count == 0)
                {
                    SetDuplicateServiceProperties(entityType.Builder, null);
                }
            }
        }

        /// <inheritdoc />
        public virtual void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
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
        }

        private static void AddDuplicateServiceProperty(IConventionEntityTypeBuilder entityTypeBuilder, MemberInfo serviceProperty)
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

        private static Dictionary<Type, HashSet<MemberInfo>> GetDuplicateServiceProperties(IConventionEntityType entityType)
            => entityType.FindAnnotation(CoreAnnotationNames.DuplicateServiceProperties)?.Value
                as Dictionary<Type, HashSet<MemberInfo>>;

        private static void SetDuplicateServiceProperties(
            IConventionEntityTypeBuilder entityTypeBuilder,
            Dictionary<Type, HashSet<MemberInfo>> duplicateServiceProperties)
            => entityTypeBuilder.HasAnnotation(CoreAnnotationNames.DuplicateServiceProperties, duplicateServiceProperties);
    }
}
