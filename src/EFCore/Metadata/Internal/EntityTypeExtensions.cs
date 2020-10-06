// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleNamedExpression
namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class EntityTypeExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static MemberInfo GetNavigationMemberInfo(
            [NotNull] this IEntityType entityType,
            [NotNull] string navigationName)
        {
            var memberInfo = entityType.ClrType.GetMembersInHierarchy(navigationName).FirstOrDefault();

            if (memberInfo == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.NoClrNavigation(navigationName, entityType.DisplayName()));
            }

            return memberInfo;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IForeignKey FindDeclaredOwnership([NotNull] this IEntityType entityType)
            => ((EntityType)entityType).FindDeclaredOwnership();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEntityType FindInDefinitionPath([NotNull] this IEntityType entityType, [NotNull] Type targetType)
        {
            var root = entityType;
            while (root != null)
            {
                if (root.ClrType == targetType)
                {
                    return root;
                }

                root = root.DefiningEntityType;
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static EntityType FindInDefinitionPath([NotNull] this EntityType entityType, [NotNull] Type targetType)
            => (EntityType)((IEntityType)entityType).FindInDefinitionPath(targetType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEntityType FindInDefinitionPath([NotNull] this IEntityType entityType, [NotNull] string targetTypeName)
        {
            var root = entityType;
            while (root != null)
            {
                if (root.Name == targetTypeName)
                {
                    return root;
                }

                root = root.DefiningEntityType;
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static EntityType FindInDefinitionPath([NotNull] this EntityType entityType, [NotNull] string targetTypeName)
            => (EntityType)((IEntityType)entityType).FindInDefinitionPath(targetTypeName);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsInDefinitionPath([NotNull] this IEntityType entityType, [NotNull] Type targetType)
            => entityType.FindInDefinitionPath(targetType) != null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsInDefinitionPath([NotNull] this IEntityType entityType, [NotNull] string targetTypeName)
            => entityType.FindInDefinitionPath(targetTypeName) != null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEntityType FindInOwnershipPath([NotNull] this IEntityType entityType, [NotNull] Type targetType)
        {
            var owner = entityType;
            while (true)
            {
                var ownership = owner.FindOwnership();
                if (ownership == null)
                {
                    return null;
                }

                owner = ownership.PrincipalEntityType;
                if (owner.ClrType.IsAssignableFrom(targetType))
                {
                    return owner;
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsInOwnershipPath([NotNull] this IEntityType entityType, [NotNull] Type targetType)
            => entityType.FindInOwnershipPath(targetType) != null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool UseEagerSnapshots([NotNull] this IEntityType entityType)
        {
            var changeTrackingStrategy = entityType.GetChangeTrackingStrategy();

            return changeTrackingStrategy == ChangeTrackingStrategy.Snapshot
                || changeTrackingStrategy == ChangeTrackingStrategy.ChangedNotifications;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static int StoreGeneratedCount([NotNull] this IEntityType entityType)
            => GetCounts(entityType).StoreGeneratedCount;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static int RelationshipPropertyCount([NotNull] this IEntityType entityType)
            => GetCounts(entityType).RelationshipCount;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static int OriginalValueCount([NotNull] this IEntityType entityType)
            => GetCounts(entityType).OriginalValueCount;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static int ShadowPropertyCount([NotNull] this IEntityType entityType)
            => GetCounts(entityType).ShadowCount;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static int NavigationCount([NotNull] this IEntityType entityType)
            => GetCounts(entityType).NavigationCount;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static int PropertyCount([NotNull] this IEntityType entityType)
            => GetCounts(entityType).PropertyCount;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static PropertyCounts GetCounts([NotNull] this IEntityType entityType)
            => ((EntityType)entityType).Counts;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static PropertyCounts CalculateCounts([NotNull] this EntityType entityType)
        {
            Check.DebugAssert(entityType.Model.IsValidated, "Should not be called on a non-validated model");

            var index = 0;
            var navigationIndex = 0;
            var originalValueIndex = 0;
            var shadowIndex = 0;
            var relationshipIndex = 0;
            var storeGenerationIndex = 0;

            var baseCounts = entityType.BaseType?.GetCounts();
            if (baseCounts != null)
            {
                index = baseCounts.PropertyCount;
                navigationIndex = baseCounts.NavigationCount;
                originalValueIndex = baseCounts.OriginalValueCount;
                shadowIndex = baseCounts.ShadowCount;
                relationshipIndex = baseCounts.RelationshipCount;
                storeGenerationIndex = baseCounts.StoreGeneratedCount;
            }

            foreach (var property in entityType.GetDeclaredProperties())
            {
                var indexes = new PropertyIndexes(
                    index: index++,
                    originalValueIndex: property.RequiresOriginalValue() ? originalValueIndex++ : -1,
                    shadowIndex: property.IsShadowProperty() ? shadowIndex++ : -1,
                    relationshipIndex: property.IsKey() || property.IsForeignKey() ? relationshipIndex++ : -1,
                    storeGenerationIndex: property.MayBeStoreGenerated() ? storeGenerationIndex++ : -1);

                property.PropertyIndexes = indexes;
            }

            var isNotifying = entityType.GetChangeTrackingStrategy() != ChangeTrackingStrategy.Snapshot;

            foreach (var navigation in entityType.GetDeclaredNavigations()
                .Union<PropertyBase>(entityType.GetDeclaredSkipNavigations()))
            {
                var indexes = new PropertyIndexes(
                    index: navigationIndex++,
                    originalValueIndex: -1,
                    shadowIndex: navigation.IsShadowProperty() ? shadowIndex++ : -1,
                    relationshipIndex: ((INavigationBase)navigation).IsCollection && isNotifying ? -1 : relationshipIndex++,
                    storeGenerationIndex: -1);

                navigation.PropertyIndexes = indexes;
            }

            foreach (var serviceProperty in entityType.GetDeclaredServiceProperties())
            {
                var indexes = new PropertyIndexes(
                    index: -1,
                    originalValueIndex: -1,
                    shadowIndex: -1,
                    relationshipIndex: -1,
                    storeGenerationIndex: -1);

                serviceProperty.PropertyIndexes = indexes;
            }

            return new PropertyCounts(
                index,
                navigationIndex,
                originalValueIndex,
                shadowIndex,
                relationshipIndex,
                storeGenerationIndex);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static Func<MaterializationContext, object> GetInstanceFactory([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().InstanceFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static Func<ISnapshot> GetEmptyShadowValuesFactory([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().EmptyShadowValuesFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static EntityType LeastDerivedType([NotNull] this EntityType entityType, [NotNull] EntityType otherEntityType)
            => (EntityType)((IEntityType)entityType).LeastDerivedType(otherEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IKey FindDeclaredPrimaryKey([NotNull] this IEntityType entityType)
            => entityType.BaseType == null ? entityType.FindPrimaryKey() : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEnumerable<INavigation> FindDerivedNavigations(
            [NotNull] this IEntityType entityType,
            [NotNull] string navigationName)
            => entityType.GetDerivedTypes().SelectMany(
                et => et.GetDeclaredNavigations().Where(navigation => navigationName == navigation.Name));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEnumerable<Navigation> GetDerivedNavigations([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().GetDerivedNavigations();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEnumerable<SkipNavigation> GetDerivedSkipNavigations([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().GetDerivedSkipNavigations();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEnumerable<IPropertyBase> GetPropertiesAndNavigations(
            [NotNull] this IEntityType entityType)
            => entityType.GetProperties().Concat<IPropertyBase>(entityType.GetNavigations());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEnumerable<IPropertyBase> GetNotificationProperties(
            [NotNull] this IEntityType entityType,
            [CanBeNull] string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                foreach (var property in entityType.GetProperties()
                    .Where(p => p.GetAfterSaveBehavior() == PropertySaveBehavior.Save))
                {
                    yield return property;
                }

                foreach (var navigation in entityType.GetNavigations())
                {
                    yield return navigation;
                }

                foreach (var navigation in entityType.GetSkipNavigations())
                {
                    yield return navigation;
                }
            }
            else
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                var property = entityType.FindProperty(propertyName)
                    ?? entityType.FindNavigation(propertyName)
                    ?? (IPropertyBase)entityType.FindSkipNavigation(propertyName);

                if (property != null)
                {
                    yield return property;
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IProperty CheckPropertyBelongsToType([NotNull] this IEntityType entityType, [NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            if (!property.DeclaringEntityType.IsAssignableFrom(entityType))
            {
                throw new InvalidOperationException(
                    CoreStrings.PropertyDoesNotBelong(property.Name, property.DeclaringEntityType.DisplayName(), entityType.DisplayName()));
            }

            return property;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static EntityType AsEntityType([NotNull] this IEntityType entityType, [NotNull] [CallerMemberName] string methodName = "")
            => MetadataExtensions.AsConcreteMetadataType<IEntityType, EntityType>(entityType, methodName);
    }
}
