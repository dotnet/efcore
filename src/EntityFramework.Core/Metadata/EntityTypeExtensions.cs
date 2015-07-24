// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
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

        public static IEnumerable<EntityType> GetDerivedTypes([NotNull] this EntityType entityType)
            => ((IEntityType)entityType).GetDerivedTypes().Cast<EntityType>();

        public static IEnumerable<IEntityType> GetConcreteTypesInHierarchy([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return new[] { entityType }
                .Concat(entityType.GetDerivedTypes())
                .Where(et => !et.IsAbstract());
        }
        
        public static IEnumerable<EntityType> GetConcreteTypesInHierarchy([NotNull] this EntityType entityType)
            => ((IEntityType)entityType).GetConcreteTypesInHierarchy().Cast<EntityType>();

        private static IEnumerable<IEntityType> GetDerivedTypes(IModel model, IEntityType entityType)
        {
            foreach (var et1 in GetDirectlyDerivedTypes(model, entityType))
            {
                yield return et1;

                foreach (var et2 in GetDerivedTypes(model, et1))
                {
                    yield return et2;
                }
            }
        }

        public static IEnumerable<IEntityType> GetDirectlyDerivedTypes([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return GetDirectlyDerivedTypes(entityType.Model, entityType);
        }

        public static IEnumerable<EntityType> GetDirectlyDerivedTypes([NotNull] this EntityType entityType)
        {
            return ((IEntityType)entityType).GetDirectlyDerivedTypes().Cast<EntityType>();
        }

        private static IEnumerable<IEntityType> GetDirectlyDerivedTypes(IModel model, IEntityType entityType)
            => model.EntityTypes.Where(et1 => et1.BaseType == entityType);

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

        public static bool IsAssignableFrom([NotNull] this IEntityType entityType, [NotNull] IEntityType derivedType)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(derivedType, nameof(derivedType));

            var baseType = derivedType;
            while (baseType != null)
            {
                if (baseType == entityType)
                {
                    return true;
                }
                baseType = baseType.BaseType;
            }
            return false;
        }

        public static bool IsAbstract([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.ClrType?.GetTypeInfo().IsAbstract ?? false;
        }

        public static IEnumerable<IProperty> GetDeclaredProperties([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.GetProperties().Where(p => p.DeclaringEntityType == entityType);
        }

        public static IProperty GetProperty([NotNull] this IEntityType entityType, [NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return entityType.GetProperty(propertyInfo.Name);
        }

        public static IProperty GetProperty([NotNull] this IEntityType entityType, [NotNull] string propertyName)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotEmpty(propertyName, nameof(propertyName));

            var property = entityType.FindProperty(propertyName);
            if (property == null)
            {
                throw new ModelItemNotFoundException(Strings.PropertyNotFound(propertyName, entityType.Name));
            }

            return property;
        }

        public static IEnumerable<IProperty> FindDerivedProperties([NotNull] this IEntityType entityType, [NotNull] IEnumerable<string> propertyNames)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(propertyNames, nameof(propertyNames));

            var searchProperties = new HashSet<string>(propertyNames);

            return entityType.GetDerivedTypes()
                .SelectMany(et => et.GetDeclaredProperties()
                    .Where(property => searchProperties.Contains(property.Name)));
        }

        [NotNull]
        public static INavigation GetNavigation([NotNull] this IEntityType entityType, [NotNull] string name)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(name, nameof(name));

            var navigation = entityType.FindNavigation(name);
            if (navigation == null)
            {
                throw new ModelItemNotFoundException(Strings.NavigationNotFound(name, entityType.Name));
            }
            return navigation;
        }

        public static IEnumerable<INavigation> GetDeclaredNavigations([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.GetNavigations().Where(p => p.DeclaringEntityType == entityType);
        }

        public static IEnumerable<IPropertyBase> GetPropertiesAndNavigations(
            [NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.GetProperties().Concat<IPropertyBase>(entityType.GetNavigations());
        }

        public static IEnumerable<IForeignKey> FindReferencingForeignKeys([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.Model.FindReferencingForeignKeys(entityType);
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

        public static IIndex GetIndex([NotNull] this IEntityType entityType, [NotNull] IProperty property)
            => entityType.GetIndex(new[] { property });

        public static IIndex GetIndex([NotNull] this IEntityType entityType, [NotNull] IReadOnlyList<IProperty> properties)
        {
            var index = entityType.FindIndex(properties);
            if (index == null)
            {
                throw new ModelItemNotFoundException(Strings.IndexNotFound(Property.Format(properties), entityType.Name));
            }
            return index;
        }

        public static IEnumerable<IIndex> GetDeclaredIndexes([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.GetIndexes().Where(p => p.DeclaringEntityType == entityType);
        }

        public static IForeignKey GetForeignKey([NotNull] this IEntityType entityType, [NotNull] IProperty property)
            => entityType.GetForeignKey(new[] { property });

        public static IForeignKey GetForeignKey([NotNull] this IEntityType entityType, [NotNull] IReadOnlyList<IProperty> properties)
        {
            var foreignKey = entityType.FindForeignKey(properties);
            if (foreignKey == null)
            {
                throw new ModelItemNotFoundException(Strings.ForeignKeyNotFound(Property.Format(properties), entityType.Name));
            }

            return foreignKey;
        }

        public static IEnumerable<IForeignKey> GetDeclaredForeignKeys([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.GetForeignKeys().Where(p => p.DeclaringEntityType == entityType);
        }

        public static ForeignKey FindForeignKey(
            [NotNull] this EntityType entityType,
            [NotNull] EntityType principalType,
            [CanBeNull] string navigationToPrincipal,
            [CanBeNull] string navigationToDependent,
            [CanBeNull] IReadOnlyList<Property> foreignKeyProperties,
            [CanBeNull] IReadOnlyList<Property> principalProperties,
            bool? isUnique)
        {
            Check.NotNull(principalType, nameof(principalType));

            return entityType.GetForeignKeys().FirstOrDefault(fk =>
                fk.IsCompatible(
                    principalType,
                    entityType,
                    navigationToPrincipal,
                    navigationToDependent,
                    foreignKeyProperties,
                    principalProperties,
                    isUnique));
        }

        public static IKey GetPrimaryKey([NotNull] this IEntityType entityType)
        {
            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey == null)
            {
                throw new ModelItemNotFoundException(Strings.EntityRequiresKey(entityType.Name));
            }

            return primaryKey;
        }

        public static IKey FindDeclaredPrimaryKey([NotNull] this IEntityType entityType)
        {
            var primaryKey = entityType.FindPrimaryKey();
            return primaryKey.EntityType == entityType ? primaryKey : null;
        }

        public static IKey GetKey([NotNull] this IEntityType entityType, [NotNull] IProperty property)
            => entityType.GetKey(new[] { property });

        public static IKey GetKey([NotNull] this IEntityType entityType, [NotNull] IReadOnlyList<IProperty> properties)
        {
            var key = entityType.FindKey(properties);
            if (key == null)
            {
                throw new ModelItemNotFoundException(Strings.KeyNotFound(Property.Format(properties), entityType.Name));
            }

            return key;
        }

        public static IEnumerable<IKey> GetDeclaredKeys([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.GetKeys().Where(p => p.EntityType == entityType);
        }
    }
}
