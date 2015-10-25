// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public static class EntityTypeExtensions
    {
        public static string DisplayName([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            if (entityType.ClrType != null)
            {
                return entityType.ClrType.DisplayName(false);
            }

            var lastDot = entityType.Name.LastIndexOfAny(new[] { '.', '+' });

            return lastDot > 0 ? entityType.Name.Substring(lastDot + 1) : entityType.Name;
        }

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
            Check.NotNull(entityType, nameof(entityType));

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
        {
            Check.NotNull(entityType, nameof(entityType));

            return new[] { entityType }.Concat(entityType.GetDerivedTypes());
        }

        public static bool UseEagerSnapshots([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return (entityType as EntityType)?.UseEagerSnapshots ?? false;
        }

        public static int OriginalValueCount([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.GetProperties().Count(p => p.GetOriginalValueIndex() >= 0);
        }

        public static int ShadowPropertyCount([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.GetProperties().Count(p => p.IsShadowProperty);
        }

        public static bool HasPropertyChangingNotifications([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.ClrType == null
                   || typeof(INotifyPropertyChanging).GetTypeInfo().IsAssignableFrom(entityType.ClrType.GetTypeInfo());
        }

        public static bool HasPropertyChangedNotifications([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.ClrType == null
                   || typeof(INotifyPropertyChanged).GetTypeInfo().IsAssignableFrom(entityType.ClrType.GetTypeInfo());
        }

        public static bool HasClrType([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.ClrType != null;
        }

        public static IEnumerable<IEntityType> GetConcreteTypesInHierarchy([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.GetDerivedTypesInclusive()
                .Where(et => !et.IsAbstract());
        }

        public static bool IsSameHierarchy([NotNull] this IEntityType firstEntityType, [NotNull] IEntityType secondEntityType)
        {
            Check.NotNull(firstEntityType, nameof(firstEntityType));
            Check.NotNull(secondEntityType, nameof(secondEntityType));

            return firstEntityType.IsAssignableFrom(secondEntityType)
                   || secondEntityType.IsAssignableFrom(firstEntityType);
        }

        public static bool IsAbstract([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.ClrType?.GetTypeInfo().IsAbstract ?? false;
        }

        public static IKey FindDeclaredPrimaryKey([NotNull] this IEntityType entityType)
            => entityType.BaseType == null ? entityType.FindPrimaryKey() : null;

        public static IEnumerable<IKey> GetDeclaredKeys([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.GetKeys().Where(p => p.DeclaringEntityType == entityType);
        }

        public static IEnumerable<IForeignKey> GetDeclaredForeignKeys([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.GetForeignKeys().Where(p => p.DeclaringEntityType == entityType);
        }

        public static IEnumerable<INavigation> GetDeclaredNavigations([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.GetNavigations().Where(p => p.DeclaringEntityType == entityType);
        }

        public static IEnumerable<INavigation> FindDerivedNavigations(
            [NotNull] this IEntityType entityType, [NotNull] string navigationName)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(navigationName, nameof(navigationName));

            return entityType.GetDerivedTypes().SelectMany(et =>
                et.GetDeclaredNavigations().Where(navigation => navigationName == navigation.Name));
        }

        public static IEnumerable<IProperty> GetDeclaredProperties([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.GetProperties().Where(p => p.DeclaringEntityType == entityType);
        }

        public static IEnumerable<IProperty> FindDerivedProperties(
            [NotNull] this IEntityType entityType, [NotNull] string propertyName)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(propertyName, nameof(propertyName));

            return entityType.GetDerivedTypes().SelectMany(et =>
                et.GetDeclaredProperties().Where(property => propertyName.Equals(property.Name)));
        }

        public static IEnumerable<IPropertyBase> GetPropertiesAndNavigations(
            [NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.GetProperties().Concat<IPropertyBase>(entityType.GetNavigations());
        }

        public static IEnumerable<IIndex> GetDeclaredIndexes([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.GetIndexes().Where(p => p.DeclaringEntityType == entityType);
        }
    }
}
