// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class AccessiblePropertyExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static PropertyAccessors GetPropertyAccessors([NotNull] this IAccessibleProperty property)
            => property.AsAccessibleProperty().Accessors;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IClrPropertyGetter GetGetter([NotNull] this IAccessibleProperty property)
            => property.AsAccessibleProperty().Getter;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IClrPropertySetter GetSetter([NotNull] this IAccessibleProperty property)
            => property.AsAccessibleProperty().Setter;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static MemberInfo GetMemberInfo(
            [NotNull] this IAccessibleProperty property,
            bool forConstruction,
            bool forSet)
        {
            MemberInfo memberInfo;
            string errorMessage;
            if (property.TryGetMemberInfo(forConstruction, forSet, out memberInfo, out errorMessage))
            {
                return memberInfo;
            }

            throw new InvalidOperationException(errorMessage);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool TryGetMemberInfo(
            [NotNull] this IAccessibleProperty property,
            bool forConstruction,
            bool forSet,
            [CanBeNull] out MemberInfo memberInfo,
            [CanBeNull] out string errorMessage)
        {
            memberInfo = null;
            errorMessage = null;

            var propertyInfo = property.PropertyInfo;
            var fieldInfo = property.FieldInfo;
            var isCollectionNav = (property as INavigation)?.IsCollection() == true;

            var mode = property.GetPropertyAccessMode();
            if (mode == null
                || mode == PropertyAccessMode.FieldDuringConstruction)
            {
                if (forConstruction
                    && fieldInfo != null
                    && !fieldInfo.IsInitOnly)
                {
                    memberInfo = fieldInfo;
                    return true;
                }

                if (forConstruction)
                {
                    if (fieldInfo != null)
                    {
                        if (!fieldInfo.IsInitOnly)
                        {
                            memberInfo = fieldInfo;
                            return true;
                        }

                        if (mode == PropertyAccessMode.FieldDuringConstruction
                            && !isCollectionNav)
                        {
                            errorMessage = CoreStrings.ReadonlyField(fieldInfo.Name, property.DeclaringType.DisplayName());
                            return false;
                        }
                    }

                    if (mode == PropertyAccessMode.FieldDuringConstruction)
                    {
                        if (!isCollectionNav)
                        {
                            errorMessage = CoreStrings.NoBackingField(
                                property.Name, property.DeclaringType.DisplayName(), nameof(PropertyAccessMode));
                            return false;
                        }
                        return true;
                    }
                }

                if (forSet)
                {
                    var setterProperty = propertyInfo?.FindSetterProperty();
                    if (setterProperty != null)
                    {
                        memberInfo = setterProperty;
                        return true;
                    }

                    if (fieldInfo != null)
                    {
                        if (!fieldInfo.IsInitOnly)
                        {
                            memberInfo = fieldInfo;
                            return true;
                        }

                        if (!isCollectionNav)
                        {
                            errorMessage = CoreStrings.ReadonlyField(fieldInfo.Name, property.DeclaringType.DisplayName());
                            return false;
                        }
                    }

                    if (!isCollectionNav)
                    {
                        errorMessage = CoreStrings.NoFieldOrSetter(property.Name, property.DeclaringType.DisplayName());
                        return false;
                    }

                    return true;
                }

                var getterPropertyInfo = propertyInfo?.FindGetterProperty();
                if (getterPropertyInfo != null)
                {
                    memberInfo = getterPropertyInfo;
                    return true;
                }

                if (fieldInfo != null)
                {
                    memberInfo = fieldInfo;
                    return true;
                }

                errorMessage = CoreStrings.NoFieldOrGetter(property.Name, property.DeclaringType.DisplayName());
                return false;
            }

            if (mode == PropertyAccessMode.Field)
            {
                if (fieldInfo == null)
                {
                    if (!forSet
                        || !isCollectionNav)
                    {
                        errorMessage = CoreStrings.NoBackingField(
                            property.Name, property.DeclaringType.DisplayName(), nameof(PropertyAccessMode));
                        return false;
                    }
                    return true;
                }

                if (forSet
                    && fieldInfo.IsInitOnly)
                {
                    if (!isCollectionNav)
                    {
                        errorMessage = CoreStrings.ReadonlyField(fieldInfo.Name, property.DeclaringType.DisplayName());
                        return false;
                    }
                    return true;
                }

                memberInfo = fieldInfo;
                return true;
            }

            if (propertyInfo == null)
            {
                errorMessage = CoreStrings.NoProperty(fieldInfo.Name, property.DeclaringType.DisplayName(), nameof(PropertyAccessMode));
                return false;
            }

            if (forSet)
            {
                var setterProperty = propertyInfo.FindSetterProperty();
                if (setterProperty == null
                    && !isCollectionNav)
                {
                    errorMessage = CoreStrings.NoSetter(property.Name, property.DeclaringType.DisplayName(), nameof(PropertyAccessMode));
                    return false;
                }

                memberInfo = setterProperty;
                return true;
            }

            var getterProperty = propertyInfo.FindGetterProperty();
            if (getterProperty == null)
            {
                errorMessage = CoreStrings.NoGetter(property.Name, property.DeclaringType.DisplayName(), nameof(PropertyAccessMode));
                return false;
            }

            memberInfo = getterProperty;
            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static AccessibleProperty AsAccessibleProperty([NotNull] this IAccessibleProperty property, [NotNull] [CallerMemberName] string methodName = "")
            => property.AsConcreteMetadataType<IAccessibleProperty, AccessibleProperty>(methodName);
    }
}
