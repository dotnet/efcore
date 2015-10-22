// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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

        private static IEnumerable<IEntityType> GetDirectlyDerivedTypes(IModel model, IEntityType entityType)
            => model.GetEntityTypes().Where(et1 => et1.BaseType == entityType);

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
                throw new ModelItemNotFoundException(CoreStrings.PropertyNotFound(propertyName, entityType.Name));
            }

            return property;
        }

        public static IMutableProperty GetProperty([NotNull] this IMutableEntityType entityType, [NotNull] PropertyInfo propertyInfo)
            => (IMutableProperty)((IEntityType)entityType).GetProperty(propertyInfo);

        public static IMutableProperty GetProperty([NotNull] this IMutableEntityType entityType, [NotNull] string propertyName)
            => (IMutableProperty)((IEntityType)entityType).GetProperty(propertyName);

        public static IMutableProperty FindProperty([NotNull] this IMutableEntityType entityType, [NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return entityType.FindProperty(propertyInfo.Name);
        }

        public static IEnumerable<IProperty> FindDerivedProperties(
            [NotNull] this IEntityType entityType,
            [NotNull] string propertyName)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(propertyName, nameof(propertyName));
            
            return entityType.GetDerivedTypes().SelectMany(et =>
                et.GetDeclaredProperties().Where(property => propertyName.Equals(property.Name)));
        }

        public static IMutableProperty GetOrAddProperty(
            [NotNull] this IMutableEntityType entityType, [NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            var property = entityType.FindProperty(propertyInfo);
            if (property != null)
            {
                property.ClrType = propertyInfo.PropertyType;
                property.IsShadowProperty = false;
                return property;
            }

            return entityType.AddProperty(propertyInfo);
        }

        public static IMutableProperty AddProperty(
            [NotNull] this IMutableEntityType entityType, [NotNull] string name, [NotNull] Type propertyType)
        {
            Check.NotNull(entityType, nameof(entityType));

            var property = entityType.AddProperty(name);
            property.ClrType = propertyType;
            return property;
        }

        public static IMutableProperty AddProperty(
            [NotNull] this IMutableEntityType entityType, [NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            if (entityType.HasClrType()
                && !propertyInfo.DeclaringType.GetTypeInfo().IsAssignableFrom(entityType.ClrType.GetTypeInfo()))
            {
                throw new ArgumentException(CoreStrings.PropertyWrongEntityClrType(
                    propertyInfo.Name, entityType.DisplayName(), propertyInfo.DeclaringType.Name));
            }

            var property = entityType.AddProperty(propertyInfo.Name, propertyInfo.PropertyType);
            property.IsShadowProperty = false;
            return property;
        }

        public static INavigation GetNavigation([NotNull] this IEntityType entityType, [NotNull] string name)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(name, nameof(name));

            var navigation = entityType.FindNavigation(name);
            if (navigation == null)
            {
                throw new ModelItemNotFoundException(CoreStrings.NavigationNotFound(name, entityType.Name));
            }

            return navigation;
        }

        public static IEnumerable<INavigation> GetDeclaredNavigations([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.GetNavigations().Where(p => p.DeclaringEntityType == entityType);
        }

        public static IEnumerable<INavigation> FindDerivedNavigations(
            [NotNull] this IEntityType entityType,
            [NotNull] string navigationName)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(navigationName, nameof(navigationName));

            return entityType.GetDerivedTypes().SelectMany(et =>
                et.GetDeclaredNavigations().Where(navigation => navigationName == navigation.Name));
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
                throw new ModelItemNotFoundException(CoreStrings.IndexNotFound(Property.Format(properties), entityType.Name));
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
                throw new ModelItemNotFoundException(CoreStrings.ForeignKeyNotFound(Property.Format(properties), entityType.Name));
            }

            return foreignKey;
        }

        public static ForeignKey FindForeignKey([NotNull] this EntityType entityType, [NotNull] Property property)
            => entityType.FindForeignKey(new[] { property });

        public static IEnumerable<IForeignKey> GetDeclaredForeignKeys([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.GetForeignKeys().Where(p => p.DeclaringEntityType == entityType);
        }

        public static IMutableForeignKey AddForeignKey(
            [NotNull] this IMutableEntityType entityType,
            [NotNull] IMutableProperty property,
            [NotNull] IMutableKey principalKey,
            [NotNull] IMutableEntityType principalEntityType)
            => entityType.AddForeignKey(new[] { property }, principalKey, principalEntityType);

        public static IKey GetPrimaryKey([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey == null)
            {
                throw new ModelItemNotFoundException(CoreStrings.EntityRequiresKey(entityType.Name));
            }

            return primaryKey;
        }

        public static IMutableKey GetPrimaryKey([NotNull] this IMutableEntityType entityType)
            => (IMutableKey)((IEntityType)entityType).GetPrimaryKey();

        public static IKey FindDeclaredPrimaryKey([NotNull] this IEntityType entityType)
        {
            var primaryKey = entityType.FindPrimaryKey();
            return primaryKey.DeclaringEntityType == entityType ? primaryKey : null;
        }
        public static IMutableKey SetPrimaryKey(
            [NotNull] this IMutableEntityType entityType, [NotNull] Property property)
            => entityType.SetPrimaryKey(new[] { property });

        public static IKey GetKey([NotNull] this IEntityType entityType, [NotNull] IProperty property)
            => entityType.GetKey(new[] { property });

        public static IKey GetKey([NotNull] this IEntityType entityType, [NotNull] IReadOnlyList<IProperty> properties)
        {
            var key = entityType.FindKey(properties);
            if (key == null)
            {
                throw new ModelItemNotFoundException(CoreStrings.KeyNotFound(Property.Format(properties), entityType.Name));
            }

            return key;
        }

        public static IEnumerable<IKey> GetDeclaredKeys([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.GetKeys().Where(p => p.DeclaringEntityType == entityType);
        }

        public static IMutableKey AddKey(
            [NotNull] this IMutableEntityType entityType, [NotNull] IMutableProperty property)
            => entityType.AddKey(new[] { property });
    }
}
