// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleNamedExpression
namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class EntityTypeExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DebuggerStepThrough]
        public static string ShortName([NotNull] this IEntityType type)
        {
            if (type.ClrType != null)
            {
                return type.ClrType.ShortDisplayName();
            }

            var plusIndex = type.Name.LastIndexOf("+", StringComparison.Ordinal);
            var dotIndex = type.Name.LastIndexOf(".", StringComparison.Ordinal);
            return plusIndex == -1
                ? dotIndex == -1
                    ? type.Name
                    : type.Name.Substring(dotIndex + 1, type.Name.Length - dotIndex - 1)
                : type.Name.Substring(plusIndex + 1, type.Name.Length - plusIndex - 1);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DebuggerStepThrough]
        // Issue#11266 This method is being used by provider code. Do not break.
        public static string DisplayName([NotNull] this IEntityType type)
        {
            if (!type.HasDefiningNavigation())
            {
                return ((ITypeBase)type).DisplayName();
            }

            var builder = new StringBuilder();
            var path = new Stack<string>();
            var root = type;
            while (true)
            {
                var definingNavigationName = root.DefiningNavigationName;
                if (definingNavigationName == null)
                {
                    break;
                }

                root = root.DefiningEntityType;
                path.Push("#");
                path.Push(definingNavigationName);
                path.Push(".");
                path.Push(((ITypeBase)root).DisplayName());
            }

            if (root != type)
            {
                builder.AppendJoin(path, "");
            }

            builder.Append(((ITypeBase)type).DisplayName());
            return builder.ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Issue#11266 This method is being used by provider code. Do not break.
        public static IEnumerable<IEntityType> GetAllBaseTypesInclusive([NotNull] this IEntityType entityType)
            => new List<IEntityType>(GetAllBaseTypes(entityType))
            {
                entityType
            };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<IEntityType> GetAllBaseTypes([NotNull] this IEntityType entityType)
        {
            var baseTypes = new List<IEntityType>();
            var currentEntityType = entityType;
            while (currentEntityType.BaseType != null)
            {
                currentEntityType = currentEntityType.BaseType;
                baseTypes.Add(currentEntityType);
            }

            baseTypes.Reverse();

            return baseTypes;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<IEntityType> GetDirectlyDerivedTypes([NotNull] this IEntityType entityType)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var derivedType in entityType.Model.GetEntityTypes())
            {
                if (derivedType.BaseType == entityType)
                {
                    yield return derivedType;
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<IEntityType> GetDerivedTypesInclusive([NotNull] this IEntityType entityType)
            => new[] { entityType }.Concat(entityType.GetDerivedTypes());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IForeignKey FindOwnership([NotNull] this IEntityType entityType)
            => ((EntityType)entityType).FindOwnership();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static ForeignKey FindOwnership([NotNull] this EntityType entityType)
            => entityType.GetForeignKeys().FirstOrDefault(fk => fk.IsOwnership);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IForeignKey FindDeclaredOwnership([NotNull] this IEntityType entityType)
            => ((EntityType)entityType).FindDeclaredOwnership();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static ForeignKey FindDeclaredOwnership([NotNull] this EntityType entityType)
            => entityType.GetDeclaredForeignKeys().FirstOrDefault(fk => fk.IsOwnership);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static INavigation FindDefiningNavigation([NotNull] this IEntityType entityType)
        {
            if (!entityType.HasDefiningNavigation())
            {
                return null;
            }

            var definingNavigation = entityType.DefiningEntityType.FindNavigation(entityType.DefiningNavigationName);
            return definingNavigation?.GetTargetType() == entityType ? definingNavigation : null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Navigation FindDefiningNavigation([NotNull] this EntityType entityType)
            => (Navigation)((IEntityType)entityType).FindDefiningNavigation();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static EntityType FindInDefinitionPath([NotNull] this EntityType entityType, [NotNull] Type targetType)
            => (EntityType)((IEntityType)entityType).FindInDefinitionPath(targetType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static EntityType FindInDefinitionPath([NotNull] this EntityType entityType, [NotNull] string targetTypeName)
            => (EntityType)((IEntityType)entityType).FindInDefinitionPath(targetTypeName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsInDefinitionPath([NotNull] this IEntityType entityType, [NotNull] Type targetType)
            => entityType.FindInDefinitionPath(targetType) != null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsInDefinitionPath([NotNull] this IEntityType entityType, [NotNull] string targetTypeName)
            => entityType.FindInDefinitionPath(targetTypeName) != null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
                if (owner.ClrType == targetType)
                {
                    return owner;
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsInOwnershipPath([NotNull] this EntityType entityType, [NotNull] Type targetType)
            => entityType.FindInOwnershipPath(targetType) != null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsInOwnershipPath([NotNull] this EntityType entityType, [NotNull] EntityType targetType)
        {
            var owner = entityType;
            while (true)
            {
                var ownOwnership = owner.FindOwnership();
                if (ownOwnership == null)
                {
                    return false;
                }

                owner = ownOwnership.PrincipalEntityType;
                if (owner == targetType)
                {
                    return true;
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool UseEagerSnapshots([NotNull] this IEntityType entityType)
        {
            var changeTrackingStrategy = entityType.GetChangeTrackingStrategy();

            return changeTrackingStrategy == ChangeTrackingStrategy.Snapshot
                   || changeTrackingStrategy == ChangeTrackingStrategy.ChangedNotifications;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static int StoreGeneratedCount([NotNull] this IEntityType entityType)
            => GetCounts(entityType).StoreGeneratedCount;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static int RelationshipPropertyCount([NotNull] this IEntityType entityType)
            => GetCounts(entityType).RelationshipCount;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static int OriginalValueCount([NotNull] this IEntityType entityType)
            => GetCounts(entityType).OriginalValueCount;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static int ShadowPropertyCount([NotNull] this IEntityType entityType)
            => GetCounts(entityType).ShadowCount;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static int NavigationCount([NotNull] this IEntityType entityType)
            => GetCounts(entityType).NavigationCount;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static int PropertyCount([NotNull] this IEntityType entityType)
            => GetCounts(entityType).PropertyCount;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static PropertyCounts GetCounts([NotNull] this IEntityType entityType)
            => ((EntityType)entityType).Counts;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static PropertyCounts CalculateCounts([NotNull] this IEntityType entityType)
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
                    shadowIndex: property.IsShadowProperty ? shadowIndex++ : -1,
                    relationshipIndex: property.IsKeyOrForeignKey() ? relationshipIndex++ : -1,
                    storeGenerationIndex: property.MayBeStoreGenerated() ? storeGenerationIndex++ : -1);

                property.SetIndexes(indexes);
            }

            var isNotifying = entityType.GetChangeTrackingStrategy() != ChangeTrackingStrategy.Snapshot;

            foreach (var navigation in entityType.GetDeclaredNavigations())
            {
                var indexes = new PropertyIndexes(
                    index: navigationIndex++,
                    originalValueIndex: -1,
                    shadowIndex: navigation.IsShadowProperty ? shadowIndex++ : -1,
                    relationshipIndex: navigation.IsCollection() && isNotifying ? -1 : relationshipIndex++,
                    storeGenerationIndex: -1);

                navigation.SetIndexes(indexes);
            }

            foreach (var serviceProperty in entityType.GetDeclaredServiceProperties())
            {
                var indexes = new PropertyIndexes(
                    index: -1,
                    originalValueIndex: -1,
                    shadowIndex: -1,
                    relationshipIndex: -1,
                    storeGenerationIndex: -1);

                serviceProperty.SetIndexes(indexes);
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Func<InternalEntityEntry, ISnapshot> GetRelationshipSnapshotFactory([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().RelationshipSnapshotFactory;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Func<InternalEntityEntry, ISnapshot> GetOriginalValuesFactory([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().OriginalValuesFactory;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Func<ValueBuffer, ISnapshot> GetShadowValuesFactory([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().ShadowValuesFactory;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Func<ISnapshot> GetEmptyShadowValuesFactory([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().EmptyShadowValuesFactory;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<IEntityType> GetConcreteTypesInHierarchy([NotNull] this IEntityType entityType)
            => entityType.GetDerivedTypesInclusive().Where(et => !et.IsAbstract());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsSameHierarchy([NotNull] this IEntityType firstEntityType, [NotNull] IEntityType secondEntityType)
            => firstEntityType.IsAssignableFrom(secondEntityType)
               || secondEntityType.IsAssignableFrom(firstEntityType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static EntityType LeastDerivedType([NotNull] this EntityType entityType, [NotNull] EntityType otherEntityType)
            => (EntityType)((IEntityType)entityType).LeastDerivedType(otherEntityType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IKey FindDeclaredPrimaryKey([NotNull] this IEntityType entityType)
            => entityType.BaseType == null ? entityType.FindPrimaryKey() : null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<IKey> GetDeclaredKeys([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().GetDeclaredKeys();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<IForeignKey> GetDeclaredForeignKeys([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().GetDeclaredForeignKeys();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<INavigation> GetDeclaredNavigations([NotNull] this IEntityType entityType)
            => entityType.GetDeclaredForeignKeys()
                .Concat(entityType.GetDeclaredReferencingForeignKeys())
                .SelectMany(foreignKey => foreignKey.FindNavigationsFrom(entityType))
                .Distinct()
                .OrderBy(m => m.Name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<IForeignKey> GetDeclaredReferencingForeignKeys([NotNull] this IEntityType entityType)
            => entityType.Model.GetEntityTypes().SelectMany(et => et.GetDeclaredForeignKeys())
                .Where(fk => fk.PrincipalEntityType == entityType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<INavigation> FindDerivedNavigations(
            [NotNull] this IEntityType entityType, [NotNull] string navigationName)
            => entityType.GetDerivedTypes().SelectMany(
                et => et.GetDeclaredNavigations().Where(navigation => navigationName == navigation.Name));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<Navigation> GetDerivedNavigations([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().GetDerivedNavigations();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<Navigation> GetDerivedNavigationsInclusive([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().GetDerivedNavigationsInclusive();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Issue#11266 This method is being used by provider code. Do not break.
        public static IEnumerable<IProperty> GetDeclaredProperties([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().GetDeclaredProperties();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<IServiceProperty> GetDeclaredServiceProperties([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().GetDeclaredServiceProperties();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<IProperty> FindDerivedProperties(
            [NotNull] this IEntityType entityType, [NotNull] string propertyName)
            => entityType.GetDerivedTypes().SelectMany(
                et => et.GetDeclaredProperties().Where(property => propertyName.Equals(property.Name)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<IPropertyBase> GetPropertiesAndNavigations(
            [NotNull] this IEntityType entityType)
            => entityType.GetProperties().Concat<IPropertyBase>(entityType.GetNavigations());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Issue#11266 This method is being used by provider code. Do not break.
        public static IEnumerable<IIndex> GetDeclaredIndexes([NotNull] this IEntityType entityType)
            => entityType.GetIndexes().Where(p => p.DeclaringEntityType == entityType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string CheckChangeTrackingStrategy([NotNull] this IEntityType entityType, ChangeTrackingStrategy value)
        {
            if (entityType.ClrType != null)
            {
                if (value != ChangeTrackingStrategy.Snapshot
                    && !typeof(INotifyPropertyChanged).GetTypeInfo().IsAssignableFrom(entityType.ClrType.GetTypeInfo()))
                {
                    return CoreStrings.ChangeTrackingInterfaceMissing(entityType.DisplayName(), value, nameof(INotifyPropertyChanged));
                }

                if ((value == ChangeTrackingStrategy.ChangingAndChangedNotifications
                     || value == ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)
                    && !typeof(INotifyPropertyChanging).GetTypeInfo().IsAssignableFrom(entityType.ClrType.GetTypeInfo()))
                {
                    return CoreStrings.ChangeTrackingInterfaceMissing(entityType.DisplayName(), value, nameof(INotifyPropertyChanging));
                }
            }

            return null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<IPropertyBase> GetNotificationProperties(
            [NotNull] this IEntityType entityType, [CanBeNull] string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                foreach (var property in entityType.GetProperties()
                    .Where(p => p.AfterSaveBehavior == PropertySaveBehavior.Save))
                {
                    yield return property;
                }

                foreach (var navigation in entityType.GetNavigations())
                {
                    yield return navigation;
                }
            }
            else
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                var property = (IPropertyBase)entityType.FindProperty(propertyName)
                               ?? entityType.FindNavigation(propertyName);
                if (property != null)
                {
                    yield return property;
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<IDictionary<string, object>> GetData([NotNull] this IEntityType entityType, bool providerValues = false)
            => entityType.AsEntityType().GetData(providerValues);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string ToDebugString([NotNull] this IEntityType entityType, bool singleLine = true, [NotNull] string indent = "")
        {
            var builder = new StringBuilder();

            builder
                .Append(indent)
                .Append(!entityType.IsQueryType ? "EntityType: " : "QueryType: ")
                .Append(entityType.DisplayName());

            if (entityType.BaseType != null)
            {
                builder.Append(" Base: ").Append(entityType.BaseType.DisplayName());
            }

            if (entityType.IsAbstract())
            {
                builder.Append(" Abstract");
            }

            if (entityType.GetChangeTrackingStrategy() != ChangeTrackingStrategy.Snapshot)
            {
                builder.Append(" ChangeTrackingStrategy.").Append(entityType.GetChangeTrackingStrategy());
            }

            if (!singleLine)
            {
                var properties = entityType.GetDeclaredProperties().ToList();
                if (properties.Count != 0)
                {
                    builder.AppendLine().Append(indent).Append("  Properties: ");
                    foreach (var property in properties)
                    {
                        builder.AppendLine().Append(property.ToDebugString(false, indent + "    "));
                    }
                }

                var navigations = entityType.GetDeclaredNavigations().ToList();
                if (navigations.Count != 0)
                {
                    builder.AppendLine().Append(indent).Append("  Navigations: ");
                    foreach (var navigation in navigations)
                    {
                        builder.AppendLine().Append(navigation.ToDebugString(false, indent + "    "));
                    }
                }

                var serviceProperties = entityType.GetDeclaredServiceProperties().ToList();
                if (serviceProperties.Count != 0)
                {
                    builder.AppendLine().Append(indent).Append("  Service properties: ");
                    foreach (var serviceProperty in serviceProperties)
                    {
                        builder.AppendLine().Append(serviceProperty.ToDebugString(false, indent + "    "));
                    }
                }

                var keys = entityType.GetDeclaredKeys().ToList();
                if (keys.Count != 0)
                {
                    builder.AppendLine().Append(indent).Append("  Keys: ");
                    foreach (var key in keys)
                    {
                        builder.AppendLine().Append(key.ToDebugString(false, indent + "    "));
                    }
                }

                var fks = entityType.GetDeclaredForeignKeys().ToList();
                if (fks.Count != 0)
                {
                    builder.AppendLine().Append(indent).Append("  Foreign keys: ");
                    foreach (var fk in fks)
                    {
                        builder.AppendLine().Append(fk.ToDebugString(false, indent + "    "));
                    }
                }

                builder.Append(entityType.AnnotationsToDebugString(indent + "  "));
            }

            return builder.ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IProperty GetProperty([NotNull] this IEntityType entityType, [NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            var property = entityType.FindProperty(name);
            if (property == null)
            {
                if (entityType.FindNavigation(name) != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.PropertyIsNavigation(
                            name, entityType.DisplayName(),
                            nameof(EntityEntry.Property), nameof(EntityEntry.Reference), nameof(EntityEntry.Collection)));
                }

                throw new InvalidOperationException(CoreStrings.PropertyNotFound(name, entityType.DisplayName()));
            }

            return property;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static EntityType AsEntityType([NotNull] this IEntityType entityType, [NotNull] [CallerMemberName] string methodName = "")
            => MetadataExtensions.AsConcreteMetadataType<IEntityType, EntityType>(entityType, methodName);
    }
}
