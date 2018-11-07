// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class PropertyBaseExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static int GetShadowIndex([NotNull] this IPropertyBase property)
            => property.GetPropertyIndexes().ShadowIndex;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static int GetStoreGeneratedIndex([NotNull] this IPropertyBase propertyBase)
            => propertyBase.GetPropertyIndexes().StoreGenerationIndex;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static int GetRelationshipIndex([NotNull] this IPropertyBase propertyBase)
            => propertyBase.GetPropertyIndexes().RelationshipIndex;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static int GetIndex([NotNull] this IPropertyBase property)
            => property.GetPropertyIndexes().Index;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static int GetOriginalValueIndex([NotNull] this IPropertyBase propertyBase)
            => propertyBase.GetPropertyIndexes().OriginalValueIndex;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static PropertyIndexes GetPropertyIndexes([NotNull] this IPropertyBase propertyBase)
            => propertyBase.AsPropertyBase()?.PropertyIndexes;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SetIndexes([NotNull] this IPropertyBase propertyBase, [CanBeNull] PropertyIndexes indexes)
            => propertyBase.AsPropertyBase().PropertyIndexes = indexes;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static PropertyAccessors GetPropertyAccessors([NotNull] this IPropertyBase propertyBase)
            => propertyBase.AsPropertyBase().Accessors;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IClrPropertyGetter GetGetter([NotNull] this IPropertyBase propertyBase)
            => propertyBase.AsPropertyBase().Getter;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IClrPropertySetter GetSetter([NotNull] this IPropertyBase propertyBase)
            => propertyBase.AsPropertyBase().Setter;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Note: only use this to find the property/field that defines the property in the model. Use
        // GetMemberInfo to get the property/field to use, which may be different.
        public static MemberInfo GetIdentifyingMemberInfo(
            [NotNull] this IPropertyBase propertyBase)
            => propertyBase.PropertyInfo ?? (MemberInfo)propertyBase.FieldInfo;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static MemberInfo GetMemberInfo(
            [NotNull] this IPropertyBase propertyBase,
            bool forConstruction,
            bool forSet)
        {
            if (propertyBase.TryGetMemberInfo(forConstruction, forSet, out var memberInfo, out var errorMessage))
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
            [NotNull] this IPropertyBase propertyBase,
            bool forConstruction,
            bool forSet,
            out MemberInfo memberInfo,
            out string errorMessage)
        {
            memberInfo = null;
            errorMessage = null;

            var propertyInfo = propertyBase.PropertyInfo;
            var fieldInfo = propertyBase.FieldInfo;
            var isCollectionNav = (propertyBase as INavigation)?.IsCollection() == true;

            var mode = propertyBase.GetPropertyAccessMode();
            if (mode == null
                || mode == PropertyAccessMode.FieldDuringConstruction)
            {
                if (forConstruction
                    && fieldInfo?.IsInitOnly == false)
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
                            errorMessage = CoreStrings.ReadonlyField(fieldInfo.Name, propertyBase.DeclaringType.DisplayName());
                            return false;
                        }
                    }

                    if (mode == PropertyAccessMode.FieldDuringConstruction)
                    {
                        if (!isCollectionNav)
                        {
                            errorMessage = GetNoFieldErrorMessage(propertyBase);
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
                            errorMessage = CoreStrings.ReadonlyField(fieldInfo.Name, propertyBase.DeclaringType.DisplayName());
                            return false;
                        }
                    }

                    if (!isCollectionNav)
                    {
                        errorMessage = CoreStrings.NoFieldOrSetter(propertyBase.Name, propertyBase.DeclaringType.DisplayName());
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

                errorMessage = CoreStrings.NoFieldOrGetter(propertyBase.Name, propertyBase.DeclaringType.DisplayName());
                return false;
            }

            if (mode == PropertyAccessMode.Field)
            {
                if (fieldInfo == null)
                {
                    if (!forSet
                        || !isCollectionNav)
                    {
                        errorMessage = GetNoFieldErrorMessage(propertyBase);
                        return false;
                    }

                    return true;
                }

                if (forSet
                    && fieldInfo.IsInitOnly)
                {
                    if (!isCollectionNav)
                    {
                        errorMessage = CoreStrings.ReadonlyField(fieldInfo.Name, propertyBase.DeclaringType.DisplayName());
                        return false;
                    }

                    return true;
                }

                memberInfo = fieldInfo;
                return true;
            }

            if (propertyInfo == null)
            {
                errorMessage = CoreStrings.NoProperty(fieldInfo.Name, propertyBase.DeclaringType.DisplayName(), nameof(PropertyAccessMode));
                return false;
            }

            if (forSet)
            {
                var setterProperty = propertyInfo.FindSetterProperty();
                if (setterProperty == null
                    && !isCollectionNav)
                {
                    errorMessage = CoreStrings.NoSetter(propertyBase.Name, propertyBase.DeclaringType.DisplayName(), nameof(PropertyAccessMode));
                    return false;
                }

                memberInfo = setterProperty;
                return true;
            }

            var getterProperty = propertyInfo.FindGetterProperty();
            if (getterProperty == null)
            {
                errorMessage = CoreStrings.NoGetter(propertyBase.Name, propertyBase.DeclaringType.DisplayName(), nameof(PropertyAccessMode));
                return false;
            }

            memberInfo = getterProperty;
            return true;
        }

        private static string GetNoFieldErrorMessage(IPropertyBase propertyBase)
        {
            var constructorBinding = (ConstructorBinding)propertyBase.DeclaringType[CoreAnnotationNames.ConstructorBinding];

            return constructorBinding?.ParameterBindings
                       .OfType<ServiceParameterBinding>()
                       .Any(b => b.ServiceType == typeof(ILazyLoader)) == true
                ? CoreStrings.NoBackingFieldLazyLoading(
                    propertyBase.Name, propertyBase.DeclaringType.DisplayName())
                : CoreStrings.NoBackingField(
                    propertyBase.Name, propertyBase.DeclaringType.DisplayName(), nameof(PropertyAccessMode));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static PropertyBase AsPropertyBase([NotNull] this IPropertyBase propertyBase, [NotNull] [CallerMemberName] string methodName = "")
            => MetadataExtensions.AsConcreteMetadataType<IPropertyBase, PropertyBase>(propertyBase, methodName);
    }
}
