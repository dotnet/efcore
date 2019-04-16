// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public abstract class NavigationAttributeEntityTypeConvention<TAttribute> :
        IEntityTypeAddedConvention,
        IEntityTypeIgnoredConvention,
        INavigationAddedConvention,
        IBaseTypeChangedConvention,
        IEntityTypeMemberIgnoredConvention
        where TAttribute : Attribute
    {
        private readonly IMemberClassifier _memberClassifier;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected NavigationAttributeEntityTypeConvention(
            [NotNull] IMemberClassifier memberClassifier)
        {
            Check.NotNull(memberClassifier, nameof(memberClassifier));

            _memberClassifier = memberClassifier;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var entityType = entityTypeBuilder.Metadata;
            if (!entityType.HasClrType())
            {
                return entityTypeBuilder;
            }

            foreach (var navigationPropertyInfo in entityType.GetRuntimeProperties().Values.OrderBy(p => p.Name))
            {
                var targetClrType = FindCandidateNavigationWithAttributePropertyType(navigationPropertyInfo);
                if (targetClrType == null)
                {
                    continue;
                }

                var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(inherit: true);
                foreach (var attribute in attributes)
                {
                    if (Apply(entityTypeBuilder, navigationPropertyInfo, targetClrType, attribute) == null)
                    {
                        return null;
                    }
                }
            }

            return entityTypeBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalModelBuilder modelBuilder, string name, Type type)
        {
            if (type == null)
            {
                return true;
            }

            foreach (var navigationPropertyInfo in type.GetRuntimeProperties().OrderBy(p => p.Name))
            {
                var targetClrType = FindCandidateNavigationWithAttributePropertyType(navigationPropertyInfo);
                if (targetClrType == null)
                {
                    continue;
                }

                var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(true);
                foreach (var attribute in attributes)
                {
                    if (!Apply(modelBuilder, type, navigationPropertyInfo, targetClrType, attribute))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Apply(
            InternalRelationshipBuilder relationshipBuilder, Navigation navigation)
        {
            var navigationPropertyInfo = navigation.GetIdentifyingMemberInfo();
            if (navigationPropertyInfo == null
                || !Attribute.IsDefined(navigationPropertyInfo, typeof(TAttribute), inherit: true))
            {
                return relationshipBuilder;
            }

            var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(true);
            foreach (var attribute in attributes)
            {
                relationshipBuilder = Apply(relationshipBuilder, navigation, attribute);
                if (relationshipBuilder == null)
                {
                    return null;
                }
            }

            return relationshipBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
        {
            var entityType = entityTypeBuilder.Metadata;
            if (!entityType.HasClrType())
            {
                return true;
            }

            foreach (var navigationPropertyInfo in entityType.GetRuntimeProperties().Values.OrderBy(p => p.Name))
            {
                var targetClrType = FindCandidateNavigationWithAttributePropertyType(navigationPropertyInfo);
                if (targetClrType == null)
                {
                    continue;
                }

                var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(true);
                foreach (var attribute in attributes)
                {
                    if (!Apply(entityTypeBuilder, oldBaseType, navigationPropertyInfo, targetClrType, attribute))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, string ignoredMemberName)
        {
            var navigationPropertyInfo =
                entityTypeBuilder.Metadata.GetRuntimeProperties()?.Find(ignoredMemberName);
            if (navigationPropertyInfo == null)
            {
                return true;
            }

            var targetClrType = FindCandidateNavigationWithAttributePropertyType(navigationPropertyInfo);
            if (targetClrType == null)
            {
                return true;
            }

            var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(true);
            foreach (var attribute in attributes)
            {
                if (!ApplyIgnored(entityTypeBuilder, navigationPropertyInfo, targetClrType, attribute))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual Type FindCandidateNavigationPropertyType([NotNull] PropertyInfo propertyInfo)
            => _memberClassifier.FindCandidateNavigationPropertyType(propertyInfo);

        private Type FindCandidateNavigationWithAttributePropertyType([NotNull] PropertyInfo propertyInfo)
        {
            var targetClrType = _memberClassifier.FindCandidateNavigationPropertyType(propertyInfo);
            return targetClrType == null
                || !Attribute.IsDefined(propertyInfo, typeof(TAttribute), inherit: true)
                ? null
                : targetClrType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Apply(
            [NotNull] InternalEntityTypeBuilder entityTypeBuilder,
            [NotNull] PropertyInfo navigationPropertyInfo,
            [NotNull] Type targetClrType,
            [NotNull] TAttribute attribute) => throw new NotImplementedException();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(
            [NotNull] InternalModelBuilder modelBuilder,
            [NotNull] Type type,
            [NotNull] PropertyInfo navigationPropertyInfo,
            [NotNull] Type targetClrType,
            [NotNull] TAttribute attribute) => throw new NotImplementedException();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Apply(
            [NotNull] InternalRelationshipBuilder relationshipBuilder,
            [NotNull] Navigation navigation,
            [NotNull] TAttribute attribute) => throw new NotImplementedException();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(
            [NotNull] InternalEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] EntityType oldBaseType,
            [NotNull] PropertyInfo navigationPropertyInfo,
            [NotNull] Type targetClrType,
            [NotNull] TAttribute attribute) => throw new NotImplementedException();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ApplyIgnored(
            [NotNull] InternalEntityTypeBuilder entityTypeBuilder,
            [NotNull] PropertyInfo navigationPropertyInfo,
            [NotNull] Type targetClrType,
            [NotNull] TAttribute attribute) => throw new NotImplementedException();
    }
}
