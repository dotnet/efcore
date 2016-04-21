// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public static class EntityTypeExtensions
    {
        public static string DisplayName([NotNull] this IEntityType entityType)
            => entityType.ClrType != null
                ? entityType.ClrType.DisplayName(fullName: false)
                : entityType.Name;

        public static IEnumerable<IEntityType> GetAllBaseTypesInclusive([NotNull] this IEntityType entityType)
        {
            var baseTypes = new List<IEntityType>();
            var currentEntityType = entityType;
            while (currentEntityType.BaseType != null)
            {
                currentEntityType = currentEntityType.BaseType;
                baseTypes.Add(currentEntityType);
            }

            baseTypes.Reverse();
            baseTypes.Add(entityType);

            return baseTypes;
        }

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

        public static IEnumerable<IEntityType> GetDerivedTypesInclusive([NotNull] this IEntityType entityType)
            => new[] { entityType }.Concat(entityType.GetDerivedTypes());

        public static bool UseEagerSnapshots([NotNull] this IEntityType entityType)
        {
            var changeTrackingStrategy = entityType.GetChangeTrackingStrategy();

            return changeTrackingStrategy == ChangeTrackingStrategy.Snapshot
                   || changeTrackingStrategy == ChangeTrackingStrategy.ChangedNotifications;
        }

        public static int StoreGeneratedCount([NotNull] this IEntityType entityType)
            => GetCounts(entityType).StoreGeneratedCount;

        public static int RelationshipPropertyCount([NotNull] this IEntityType entityType)
            => GetCounts(entityType).RelationshipCount;

        public static int OriginalValueCount([NotNull] this IEntityType entityType)
            => GetCounts(entityType).OriginalValueCount;

        public static int ShadowPropertyCount([NotNull] this IEntityType entityType)
            => GetCounts(entityType).ShadowCount;

        public static int NavigationCount([NotNull] this IEntityType entityType)
            => GetCounts(entityType).NavigationCount;

        public static int PropertyCount([NotNull] this IEntityType entityType)
            => GetCounts(entityType).PropertyCount;

        public static PropertyCounts GetCounts([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().Counts;

        public static PropertyCounts CalculateCounts([NotNull] this IEntityType entityType)
        {
            var properties = entityType.GetDeclaredProperties().ToList();
            var navigations = entityType.GetDeclaredNavigations();

            var isNotifying = entityType.GetChangeTrackingStrategy() != ChangeTrackingStrategy.Snapshot;

            var propertyCount = properties.Count;
            var navigationCount = navigations.Count();
            var originalValueCount = properties.Count(p => p.RequiresOriginalValue());
            var shadowCount = properties.Count(p => p.IsShadowProperty);
            var relationshipCount = (isNotifying ? navigations.Count(n => !n.IsCollection()) : navigationCount) 
                + properties.Count(p => p.IsKeyOrForeignKey());
            var storeGeneratedCount = properties.Count(p => p.MayBeStoreGenerated());

            var baseCounts = entityType.BaseType?.CalculateCounts();

            return baseCounts == null
                ? new PropertyCounts(
                    propertyCount,
                    navigationCount,
                    originalValueCount,
                    shadowCount,
                    relationshipCount,
                    storeGeneratedCount)
                : new PropertyCounts(
                    baseCounts.PropertyCount + propertyCount,
                    baseCounts.NavigationCount + navigationCount,
                    baseCounts.OriginalValueCount + originalValueCount,
                    baseCounts.ShadowCount + shadowCount,
                    baseCounts.RelationshipCount + relationshipCount,
                    baseCounts.StoreGeneratedCount + storeGeneratedCount);
        }

        public static Func<InternalEntityEntry, ISnapshot> GetRelationshipSnapshotFactory([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().RelationshipSnapshotFactory;

        public static Func<InternalEntityEntry, ISnapshot> GetOriginalValuesFactory([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().OriginalValuesFactory;

        public static Func<ValueBuffer, ISnapshot> GetShadowValuesFactory([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().ShadowValuesFactory;

        public static Func<ISnapshot> GetEmptyShadowValuesFactory([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().EmptyShadowValuesFactory;

        public static bool HasClrType([NotNull] this IEntityType entityType)
            => entityType.ClrType != null;

        public static IEnumerable<IEntityType> GetConcreteTypesInHierarchy([NotNull] this IEntityType entityType)
            => entityType.GetDerivedTypesInclusive().Where(et => !et.IsAbstract());

        public static bool IsSameHierarchy([NotNull] this IEntityType firstEntityType, [NotNull] IEntityType secondEntityType)
            => firstEntityType.IsAssignableFrom(secondEntityType)
               || secondEntityType.IsAssignableFrom(firstEntityType);

        public static EntityType LeastDerivedType([NotNull] this EntityType entityType, [NotNull] EntityType otherEntityType)
            => (EntityType)((IEntityType)entityType).LeastDerivedType(otherEntityType);

        public static bool IsAbstract([NotNull] this IEntityType entityType)
            => entityType.ClrType?.GetTypeInfo().IsAbstract ?? false;

        public static IKey FindDeclaredPrimaryKey([NotNull] this IEntityType entityType)
            => entityType.BaseType == null ? entityType.FindPrimaryKey() : null;

        public static IEnumerable<IKey> GetDeclaredKeys([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().GetDeclaredKeys();

        public static IEnumerable<IForeignKey> GetDeclaredForeignKeys([NotNull] this IEntityType entityType)
            => entityType.AsEntityType().GetDeclaredForeignKeys();

        public static IEnumerable<INavigation> GetDeclaredNavigations([NotNull] this IEntityType entityType)
            => entityType.GetDeclaredForeignKeys()
                .Concat(entityType.GetDeclaredReferencingForeignKeys())
                .SelectMany(foreignKey => foreignKey.FindNavigationsFrom(entityType))
                .Distinct()
                .OrderBy(m => m.Name);

        public static IEnumerable<IForeignKey> GetDeclaredReferencingForeignKeys([NotNull] this IEntityType entityType)
            => entityType.Model.GetEntityTypes().SelectMany(et => et.GetDeclaredForeignKeys())
                .Where(fk => fk.PrincipalEntityType == entityType);

        public static IEnumerable<INavigation> FindDerivedNavigations(
            [NotNull] this IEntityType entityType, [NotNull] string navigationName)
            => entityType.GetDerivedTypes().SelectMany(et =>
                et.GetDeclaredNavigations().Where(navigation => navigationName == navigation.Name));

        public static IEnumerable<IProperty> GetDeclaredProperties([NotNull] this IEntityType entityType)
            => entityType.GetProperties().Where(p => p.DeclaringEntityType == entityType);

        public static IEnumerable<IProperty> FindDerivedProperties(
            [NotNull] this IEntityType entityType, [NotNull] string propertyName)
            => entityType.GetDerivedTypes().SelectMany(et =>
                et.GetDeclaredProperties().Where(property => propertyName.Equals(property.Name)));

        public static IEnumerable<IPropertyBase> GetPropertiesAndNavigations(
            [NotNull] this IEntityType entityType)
            => entityType.GetProperties().Concat<IPropertyBase>(entityType.GetNavigations());

        public static IEnumerable<IIndex> GetDeclaredIndexes([NotNull] this IEntityType entityType)
            => entityType.GetIndexes().Where(p => p.DeclaringEntityType == entityType);

        public static string CheckChangeTrackingStrategy([NotNull] this IEntityType entityType, ChangeTrackingStrategy value)
        {
            if (entityType.ClrType != null)
            {
                if (value != ChangeTrackingStrategy.Snapshot
                    && !typeof(INotifyPropertyChanged).GetTypeInfo().IsAssignableFrom(entityType.ClrType.GetTypeInfo()))
                {
                    return CoreStrings.ChangeTrackingInterfaceMissing(entityType.DisplayName(), value, typeof(INotifyPropertyChanged).Name);
                }

                if ((value == ChangeTrackingStrategy.ChangingAndChangedNotifications
                     || value == ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)
                    && !typeof(INotifyPropertyChanging).GetTypeInfo().IsAssignableFrom(entityType.ClrType.GetTypeInfo()))
                {
                    return CoreStrings.ChangeTrackingInterfaceMissing(entityType.DisplayName(), value, typeof(INotifyPropertyChanging).Name);
                }
            }

            return null;
        }

        public static EntityType AsEntityType([NotNull] this IEntityType entityType, [NotNull] [CallerMemberName] string methodName = "")
            => entityType.AsConcreteMetadataType<IEntityType, EntityType>(methodName);
    }
}
