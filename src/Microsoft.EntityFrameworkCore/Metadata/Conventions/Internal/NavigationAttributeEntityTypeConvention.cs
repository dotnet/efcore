// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
    public abstract class NavigationAttributeEntityTypeConvention<TAttribute> : IEntityTypeConvention, IEntityTypeIgnoredConvention, INavigationConvention, IBaseTypeConvention, IEntityTypeMemberIgnoredConvention
        where TAttribute : Attribute
    {
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

            foreach (var navigationPropertyInfo in entityType.ClrType.GetRuntimeProperties().OrderBy(p => p.Name))
            {
                var targetClrType = FindCandidateNavigationPropertyType(navigationPropertyInfo);
                if (targetClrType == null)
                {
                    continue;
                }

                var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(true);
                if (attributes != null)
                {
                    foreach (var attribute in attributes)
                    {
                        if (Apply(entityTypeBuilder, navigationPropertyInfo, targetClrType, attribute) == null)
                        {
                            return null;
                        }
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
                var targetClrType = FindCandidateNavigationPropertyType(navigationPropertyInfo);
                if (targetClrType == null)
                {
                    continue;
                }

                var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(true);
                if (attributes != null)
                {
                    foreach (var attribute in attributes)
                    {
                        if (!Apply(modelBuilder, type, navigationPropertyInfo, targetClrType, attribute))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder, Navigation navigation)
        {
            var navigationPropertyInfo = navigation.PropertyInfo;
            if (navigationPropertyInfo == null)
            {
                return relationshipBuilder;
            }

            var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(true);
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    relationshipBuilder = Apply(relationshipBuilder, navigation, attribute);
                    if (relationshipBuilder == null)
                    {
                        return null;
                    }
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
            var clrType = entityTypeBuilder.Metadata.ClrType;
            if (clrType == null)
            {
                return true;
            }

            foreach (var navigationPropertyInfo in clrType.GetRuntimeProperties().OrderBy(p => p.Name))
            {
                var targetClrType = FindCandidateNavigationPropertyType(navigationPropertyInfo);
                if (targetClrType == null)
                {
                    continue;
                }

                var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(true);
                if (attributes != null)
                {
                    foreach (var attribute in attributes)
                    {
                        if (!Apply(entityTypeBuilder, oldBaseType, navigationPropertyInfo, targetClrType, attribute))
                        {
                            return false;
                        }
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
                entityTypeBuilder.Metadata.ClrType.GetRuntimeProperties().FirstOrDefault(p => p.Name == ignoredMemberName);
            if (navigationPropertyInfo == null)
            {
                return true;
            }

            var targetClrType = FindCandidateNavigationPropertyType(navigationPropertyInfo);
            if (targetClrType == null)
            {
                return true;
            }

            var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(true);
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    if (!ApplyIgnored(entityTypeBuilder, navigationPropertyInfo, targetClrType, attribute))
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
        public virtual Type FindCandidateNavigationPropertyType([NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return propertyInfo.FindCandidateNavigationPropertyType(clrType => clrType.IsPrimitive());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Apply(
            [NotNull] InternalEntityTypeBuilder entityTypeBuilder,
            [NotNull] PropertyInfo navigationPropertyInfo,
            [NotNull] Type targetClrType,
            [NotNull] TAttribute attribute)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(
            [NotNull] InternalModelBuilder modelBuilder,
            [NotNull] Type type,
            [NotNull] PropertyInfo navigationPropertyInfo,
            [NotNull] Type targetClrType,
            [NotNull] TAttribute attribute)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Apply(
            [NotNull] InternalRelationshipBuilder relationshipBuilder,
            [NotNull] Navigation navigation,
            [NotNull] TAttribute attribute)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(
            [NotNull] InternalEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] EntityType oldBaseType,
            [NotNull] PropertyInfo navigationPropertyInfo,
            [NotNull] Type targetClrType,
            [NotNull] TAttribute attribute)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ApplyIgnored(
            [NotNull] InternalEntityTypeBuilder entityTypeBuilder,
            [NotNull] PropertyInfo navigationPropertyInfo,
            [NotNull] Type targetClrType,
            [NotNull] TAttribute attribute)
        {
            throw new NotImplementedException();
        }
    }
}
