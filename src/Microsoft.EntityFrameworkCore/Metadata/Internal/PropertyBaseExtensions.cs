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
        public static PropertyIndexes GetPropertyIndexes([NotNull] this IPropertyBase propertyBase)
            => (propertyBase as IProperty)?.AsProperty()?.PropertyIndexes
               ?? ((INavigation)propertyBase).AsNavigation().PropertyIndexes;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static PropertyIndexes CalculateIndexes([NotNull] this IEntityType entityType, [NotNull] IPropertyBase propertyBase)
        {
            var index = 0;
            var navigationIndex = 0;
            var shadowIndex = 0;
            var originalValueIndex = 0;
            var relationshipIndex = 0;
            var storeGenerationIndex = 0;

            var baseCounts = entityType.BaseType?.GetCounts();
            if (baseCounts != null)
            {
                index = baseCounts.PropertyCount;
                navigationIndex = baseCounts.NavigationCount;
                shadowIndex = baseCounts.ShadowCount;
                originalValueIndex = baseCounts.OriginalValueCount;
                relationshipIndex = baseCounts.RelationshipCount;
                storeGenerationIndex = baseCounts.StoreGeneratedCount;
            }

            PropertyIndexes callingPropertyIndexes = null;

            foreach (var property in entityType.GetDeclaredProperties())
            {
                var indexes = new PropertyIndexes(
                    index: index++,
                    originalValueIndex: property.RequiresOriginalValue() ? originalValueIndex++ : -1,
                    shadowIndex: property.IsShadowProperty ? shadowIndex++ : -1,
                    relationshipIndex: property.IsKeyOrForeignKey() ? relationshipIndex++ : -1,
                    storeGenerationIndex: property.MayBeStoreGenerated() ? storeGenerationIndex++ : -1);

                TrySetIndexes(property, indexes);

                if (propertyBase == property)
                {
                    callingPropertyIndexes = indexes;
                }
            }

            var isNotifying = entityType.GetChangeTrackingStrategy() != ChangeTrackingStrategy.Snapshot;

            foreach (var navigation in entityType.GetDeclaredNavigations())
            {
                var indexes = new PropertyIndexes(
                    index: navigationIndex++,
                    originalValueIndex: -1,
                    shadowIndex: -1,
                    relationshipIndex: navigation.IsCollection() && isNotifying ? -1 : relationshipIndex++,
                    storeGenerationIndex: -1);

                TrySetIndexes(navigation, indexes);

                if (propertyBase == navigation)
                {
                    callingPropertyIndexes = indexes;
                }
            }

            foreach (var derivedType in entityType.GetDirectlyDerivedTypes())
            {
                derivedType.CalculateIndexes(propertyBase);
            }

            return callingPropertyIndexes;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void TrySetIndexes([NotNull] this IPropertyBase propertyBase, [CanBeNull] PropertyIndexes indexes)
        {
            var property = propertyBase as IProperty;
            if (property != null)
            {
                property.AsProperty().PropertyIndexes = indexes;
            }
            else
            {
                ((INavigation)propertyBase).AsNavigation().PropertyIndexes = indexes;
            }
        }

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
        public static MemberInfo GetMemberInfo(
            [NotNull] this IPropertyBase propertyBase,
            bool forConstruction,
            bool forSet)
        {
            MemberInfo memberInfo;
            string errorMessage;
            if (propertyBase.TryGetMemberInfo(forConstruction, forSet, out memberInfo, out errorMessage))
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
            [CanBeNull] out MemberInfo memberInfo,
            [CanBeNull] out string errorMessage)
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
                            errorMessage = CoreStrings.ReadonlyField(fieldInfo.Name, propertyBase.DeclaringType.DisplayName());
                            return false;
                        }
                    }

                    if (mode == PropertyAccessMode.FieldDuringConstruction)
                    {
                        if (!isCollectionNav)
                        {
                            errorMessage = CoreStrings.NoBackingField(
                                propertyBase.Name, propertyBase.DeclaringType.DisplayName(), nameof(PropertyAccessMode));
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
                        errorMessage = CoreStrings.NoBackingField(
                            propertyBase.Name, propertyBase.DeclaringType.DisplayName(), nameof(PropertyAccessMode));
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static PropertyBase AsPropertyBase([NotNull] this IPropertyBase propertyBase, [NotNull] [CallerMemberName] string methodName = "")
            => MetadataExtensions.AsConcreteMetadataType<IPropertyBase, PropertyBase>(propertyBase, methodName);
    }
}
