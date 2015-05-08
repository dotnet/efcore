// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public static class EntityTypeExtensions
    {
        public static IEnumerable<IEntityType> GetDerivedTypes([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return GetDerivedTypes(entityType.Model, entityType);
        }

        public static IEnumerable<IEntityType> GetConcreteTypesInHierarchy([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return new[] { entityType }
                .Concat(entityType.GetDerivedTypes())
                .Where(et => !et.IsAbstract);
        }

        private static IEnumerable<IEntityType> GetDerivedTypes(IModel model, IEntityType entityType)
        {
            foreach (var et1 in model.EntityTypes
                .Where(et1 => et1.BaseType == entityType))
            {
                yield return et1;

                foreach (var et2 in GetDerivedTypes(model, et1))
                {
                    yield return et2;
                }
            }
        }

        public static string DisplayName([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            if (entityType.ClrType != null)
            {
                return TypeNameHelper.GetTypeDisplayName(entityType.ClrType, false);
            }

            var lastDot = entityType.Name.LastIndexOfAny(new[] { '.', '+' });

            return lastDot > 0 ? entityType.Name.Substring(lastDot + 1) : entityType.Name;
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

        public static bool HasClrType([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.ClrType != null;
        }

        public static IEntityType RootType([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.BaseType?.RootType() ?? entityType;
        }

        public static IEnumerable<IProperty> GetDeclaredProperties([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.GetProperties().Where(p => p.EntityType == entityType);
        }

        public static IEnumerable<IPropertyBase> GetPropertiesAndNavigations(
            [NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.GetProperties().Concat<IPropertyBase>(entityType.GetNavigations());
        }

        public static IEnumerable<IForeignKey> GetReferencingForeignKeys([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.Model.GetReferencingForeignKeys(entityType);
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
    }
}
