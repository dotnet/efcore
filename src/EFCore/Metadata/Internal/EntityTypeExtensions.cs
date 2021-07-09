// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
        public static string DisplayName(this TypeBase entityType)
            => ((IReadOnlyTypeBase)entityType).DisplayName();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static string ShortName(this TypeBase entityType)
            => ((IReadOnlyTypeBase)entityType).ShortName();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static MemberInfo GetNavigationMemberInfo(
            this IReadOnlyEntityType entityType,
            string navigationName)
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
        public static bool IsOwned(this IReadOnlyEntityType entityType)
            => entityType.IsOwned();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IReadOnlyForeignKey? FindDeclaredOwnership(this IReadOnlyEntityType entityType)
            => entityType.GetDeclaredForeignKeys().FirstOrDefault(fk => fk.IsOwnership);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IConventionForeignKey? FindDeclaredOwnership(this IConventionEntityType entityType)
            => entityType.GetDeclaredForeignKeys().FirstOrDefault(fk => fk.IsOwnership);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IReadOnlyEntityType? FindInOwnershipPath(this IReadOnlyEntityType entityType, Type targetType)
        {
            if (entityType.ClrType == targetType)
            {
                return entityType;
            }

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
        public static bool IsInOwnershipPath(this IReadOnlyEntityType entityType, Type targetType)
            => entityType.FindInOwnershipPath(targetType) != null;


        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsInOwnershipPath(this IReadOnlyEntityType entityType, IReadOnlyEntityType targetType)
        {
            if (entityType == targetType)
            {
                return true;
            }

            var owner = entityType;
            while (true)
            {
                var ownership = owner.FindOwnership();
                if (ownership == null)
                {
                    return false;
                }

                owner = ownership.PrincipalEntityType;
                if (owner.IsAssignableFrom(targetType))
                {
                    return true;
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public static string GetOwnedName(this IReadOnlyTypeBase type, string simpleName, string ownershipNavigation)
            => type.Name + "." + ownershipNavigation + "#" + simpleName;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool UseEagerSnapshots(this IReadOnlyEntityType entityType)
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
        public static int StoreGeneratedCount(this IEntityType entityType)
            => GetCounts(entityType).StoreGeneratedCount;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static int RelationshipPropertyCount(this IEntityType entityType)
            => GetCounts(entityType).RelationshipCount;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static int OriginalValueCount(this IEntityType entityType)
            => GetCounts(entityType).OriginalValueCount;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static int ShadowPropertyCount(this IEntityType entityType)
            => GetCounts(entityType).ShadowCount;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static int NavigationCount(this IEntityType entityType)
            => GetCounts(entityType).NavigationCount;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static int PropertyCount(this IEntityType entityType)
            => GetCounts(entityType).PropertyCount;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static PropertyCounts GetCounts(this IEntityType entityType)
            => ((IRuntimeEntityType)entityType).Counts;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static PropertyCounts CalculateCounts(this IRuntimeEntityType entityType)
        {
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

                ((IRuntimePropertyBase)property).PropertyIndexes = indexes;
            }

            var isNotifying = entityType.GetChangeTrackingStrategy() != ChangeTrackingStrategy.Snapshot;

            foreach (var navigation in entityType.GetDeclaredNavigations()
                .Union<IPropertyBase>(entityType.GetDeclaredSkipNavigations()))
            {
                var indexes = new PropertyIndexes(
                    index: navigationIndex++,
                    originalValueIndex: -1,
                    shadowIndex: navigation.IsShadowProperty() ? shadowIndex++ : -1,
                    relationshipIndex: ((IReadOnlyNavigationBase)navigation).IsCollection && isNotifying ? -1 : relationshipIndex++,
                    storeGenerationIndex: -1);

                ((IRuntimePropertyBase)navigation).PropertyIndexes = indexes;
            }

            foreach (var serviceProperty in entityType.GetDeclaredServiceProperties())
            {
                var indexes = new PropertyIndexes(
                    index: -1,
                    originalValueIndex: -1,
                    shadowIndex: -1,
                    relationshipIndex: -1,
                    storeGenerationIndex: -1);

                ((IRuntimePropertyBase)serviceProperty).PropertyIndexes = indexes;
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
        public static Func<MaterializationContext, object> GetInstanceFactory(this IEntityType entityType)
            => ((IRuntimeEntityType)entityType).InstanceFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static Func<ISnapshot> GetEmptyShadowValuesFactory(this IEntityType entityType)
            => ((IRuntimeEntityType)entityType).EmptyShadowValuesFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static EntityType? LeastDerivedType(this EntityType entityType, EntityType otherEntityType)
            => (EntityType?)((IReadOnlyEntityType)entityType).LeastDerivedType(otherEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsAssignableFrom(this EntityType entityType, IReadOnlyEntityType otherEntityType)
            => ((IReadOnlyEntityType)entityType).IsAssignableFrom(otherEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsStrictlyDerivedFrom(this EntityType entityType, IReadOnlyEntityType otherEntityType)
            => ((IReadOnlyEntityType)entityType).IsStrictlyDerivedFrom(otherEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static object? GetDiscriminatorValue(this EntityType entityType)
            => ((IReadOnlyEntityType)entityType).GetDiscriminatorValue();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IReadOnlyKey? FindDeclaredPrimaryKey(this IReadOnlyEntityType entityType)
            => entityType.BaseType == null ? entityType.FindPrimaryKey() : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEnumerable<IReadOnlyNavigation> FindDerivedNavigations(
            this IReadOnlyEntityType entityType,
            string navigationName)
            => entityType.GetDerivedNavigations().Where(navigation => navigationName == navigation.Name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEnumerable<IPropertyBase> GetPropertiesAndNavigations(
            this IEntityType entityType)
            => entityType.GetProperties().Concat<IPropertyBase>(entityType.GetNavigations());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IProperty CheckPropertyBelongsToType(
            this IEntityType entityType, IProperty property)
        {
            Check.NotNull(property, nameof(property));

            if (!property.DeclaringEntityType.IsAssignableFrom(entityType))
            {
                throw new InvalidOperationException(
                    CoreStrings.PropertyDoesNotBelong(property.Name, property.DeclaringEntityType.DisplayName(), entityType.DisplayName()));
            }

            return property;
        }
    }
}
