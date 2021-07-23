// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        public virtual ImmutableSortedDictionary<PropertyInfo, (Type Type, bool? ShouldBeOwned)> GetNavigationCandidates(
            IConventionEntityType entityType)
        {
            if (entityType.FindAnnotation(CoreAnnotationNames.NavigationCandidates)?.Value
                is ImmutableSortedDictionary<PropertyInfo, (Type Type, bool? ShouldBeOwned)> navigationCandidates)
            {
                return navigationCandidates;
            }

            var dictionaryBuilder = ImmutableSortedDictionary.CreateBuilder<PropertyInfo, (Type Type, bool? shouldBeOwned)>(
                MemberInfoNameComparer.Instance);

            var configuration = ((Model)entityType.Model).Configuration;
            foreach (var propertyInfo in entityType.GetRuntimeProperties().Values)
            {
                var targetType = FindCandidateNavigationPropertyType(propertyInfo, configuration, out var shouldBeOwned);
                if (targetType != null)
                {
                    dictionaryBuilder[propertyInfo] = (targetType, shouldBeOwned);
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
        public virtual Type? FindCandidateNavigationPropertyType(
            MemberInfo memberInfo, IConventionModel model, out bool? shouldBeOwned)
            => FindCandidateNavigationPropertyType(memberInfo, ((Model)model).Configuration, out shouldBeOwned);

        private Type? FindCandidateNavigationPropertyType(
            MemberInfo memberInfo, ModelConfiguration? configuration, out bool? shouldBeOwned)
        {
            shouldBeOwned = null;
            var propertyInfo = memberInfo as PropertyInfo;
            var targetType = memberInfo.GetMemberType();
            var targetSequenceType = targetType.TryGetSequenceType();
            return targetSequenceType != null
                && (propertyInfo == null
                    || propertyInfo.IsCandidateProperty(needsWrite: false))
                && IsCandidateNavigationPropertyType(targetSequenceType, memberInfo, configuration, out shouldBeOwned)
                ? targetSequenceType
                : (propertyInfo == null
                    || propertyInfo.IsCandidateProperty(needsWrite: true))
                    && IsCandidateNavigationPropertyType(targetType, memberInfo, configuration, out shouldBeOwned)
                    ? targetType
                    : null;
        }

        private bool IsCandidateNavigationPropertyType(
           Type targetType, MemberInfo memberInfo, ModelConfiguration? configuration, out bool? shouldBeOwned)
        {
            shouldBeOwned = null;
            var configurationType = configuration?.GetConfigurationType(targetType);
            var isConfiguredAsEntityType = configurationType.IsEntityType();
            if (isConfiguredAsEntityType == false
                || !targetType.IsValidEntityType())
            {
                return false;
            }

            if (configurationType != null)
            {
                shouldBeOwned = configurationType == TypeConfigurationType.OwnedEntityType;
            }

            return isConfiguredAsEntityType == true
                || (targetType != typeof(object)
                    && _parameterBindingFactories.FindFactory(memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName()) == null
                    && _typeMappingSource.FindMapping(targetType) == null);
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
