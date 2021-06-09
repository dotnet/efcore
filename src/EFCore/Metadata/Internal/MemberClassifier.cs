// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class MemberClassifier : IMemberClassifier
    {
        private readonly ITypeMappingSource _typeMappingSource;
        private readonly IParameterBindingFactories _parameterBindingFactories;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public MemberClassifier(
            ITypeMappingSource typeMappingSource,
            IParameterBindingFactories parameterBindingFactories)
        {
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));
            Check.NotNull(parameterBindingFactories, nameof(parameterBindingFactories));

            _typeMappingSource = typeMappingSource;
            _parameterBindingFactories = parameterBindingFactories;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ImmutableSortedDictionary<PropertyInfo, Type> GetNavigationCandidates(IConventionEntityType entityType)
        {
            if (entityType.FindAnnotation(CoreAnnotationNames.NavigationCandidates)?.Value
                is ImmutableSortedDictionary<PropertyInfo, Type> navigationCandidates)
            {
                return navigationCandidates;
            }

            var dictionaryBuilder = ImmutableSortedDictionary.CreateBuilder<PropertyInfo, Type>(MemberInfoNameComparer.Instance);

            var configuration = ((Model)entityType.Model).Configuration;
            foreach (var propertyInfo in entityType.GetRuntimeProperties().Values)
            {
                var targetType = FindCandidateNavigationPropertyType(propertyInfo, configuration);
                if (targetType != null)
                {
                    dictionaryBuilder[propertyInfo] = targetType;
                }
            }

            navigationCandidates = dictionaryBuilder.ToImmutable();

            if (!((Annotatable)entityType).IsReadOnly
                && entityType.IsInModel)
            {
                entityType.Builder.HasAnnotation(CoreAnnotationNames.NavigationCandidates, navigationCandidates);
            }
            return navigationCandidates;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Type? FindCandidateNavigationPropertyType(MemberInfo memberInfo, ModelConfiguration? configuration)
        {
            var targetType = memberInfo.GetMemberType();
            var targetSequenceType = targetType.TryGetSequenceType();
            if (!(memberInfo is PropertyInfo propertyInfo)
                || !propertyInfo.IsCandidateProperty(targetSequenceType == null))
            {
                return null;
            }

            var isConfiguredAsEntityType = configuration?.GetConfigurationType(targetType).IsEntityType();
            if (isConfiguredAsEntityType == false)
            {
                return null;
            }

            if (targetSequenceType != null)
            {
                isConfiguredAsEntityType ??= configuration?.GetConfigurationType(targetSequenceType).IsEntityType();
                if (isConfiguredAsEntityType == false)
                {
                    return null;
                }
            }

            targetType = targetSequenceType ?? targetType;
            if (!targetType.IsValidEntityType())
            {
                return null;
            }

            targetType = targetType.UnwrapNullableType();
            return isConfiguredAsEntityType == null
                && (targetType == typeof(object)
                    || _parameterBindingFactories.FindFactory(targetType, memberInfo.GetSimpleMemberName()) != null
                    || _typeMappingSource.FindMapping(targetType) != null)
                    ? null
                    : targetType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsCandidatePrimitiveProperty(PropertyInfo propertyInfo, ModelConfiguration? configuration)
        {
            if (!propertyInfo.IsCandidateProperty())
            {
                return false;
            }

            var configurationType = configuration?.GetConfigurationType(propertyInfo.PropertyType);
            return configurationType == TypeConfigurationType.Property
                    || (configurationType == null && _typeMappingSource.FindMapping(propertyInfo) != null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IParameterBindingFactory? FindServicePropertyCandidateBindingFactory(
            PropertyInfo propertyInfo, ModelConfiguration? configuration)
        {
            if (!propertyInfo.IsCandidateProperty(publicOnly: false))
            {
                return null;
            }

            var type = propertyInfo.PropertyType;
            var configurationType = configuration?.GetConfigurationType(type);
            if (configurationType != TypeConfigurationType.ServiceProperty)
            {
                if (configurationType != null)
                {
                    return null;
                }

                if (propertyInfo.IsCandidateProperty()
                    && _typeMappingSource.FindMapping(propertyInfo) != null)
                {
                    return null;
                }
            }

            return _parameterBindingFactories.FindFactory(type, propertyInfo.GetSimpleMemberName());
        }
    }
}
