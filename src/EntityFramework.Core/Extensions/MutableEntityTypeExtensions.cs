// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public static class MutableEntityTypeExtensions
    {
        public static IEnumerable<IMutableEntityType> GetDerivedTypes([NotNull] this IMutableEntityType entityType)
            => ((IEntityType)entityType).GetDerivedTypes().Cast<IMutableEntityType>();

        public static IMutableEntityType RootType([NotNull] this IMutableEntityType entityType)
            => (IMutableEntityType)((IEntityType)entityType).RootType();

        public static IMutableKey SetPrimaryKey(
            [NotNull] this IMutableEntityType entityType, [CanBeNull] IMutableProperty property)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.SetPrimaryKey(property == null ? null : new[] { property });
        }

        public static IMutableKey GetOrSetPrimaryKey(
            [NotNull] this IMutableEntityType entityType, [NotNull] IMutableProperty property)
            => entityType.GetOrSetPrimaryKey(new[] { property });

        public static IMutableKey GetOrSetPrimaryKey(
            [NotNull] this IMutableEntityType entityType, [NotNull] IReadOnlyList<IMutableProperty> properties)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.SetPrimaryKey(properties);
        }

        public static IMutableKey FindKey([NotNull] this IMutableEntityType entityType, [NotNull] IProperty property)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.FindKey(new[] { property });
        }

        public static IMutableKey AddKey(
            [NotNull] this IMutableEntityType entityType, [NotNull] IMutableProperty property)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.AddKey(new[] { property });
        }

        public static IMutableKey GetOrAddKey(
            [NotNull] this IMutableEntityType entityType, [NotNull] IMutableProperty property)
            => entityType.GetOrAddKey(new[] { property });

        public static IMutableKey GetOrAddKey(
            [NotNull] this IMutableEntityType entityType, [NotNull] IReadOnlyList<IMutableProperty> properties)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.FindKey(properties) ?? entityType.AddKey(properties);
        }

        public static IEnumerable<IMutableForeignKey> FindForeignKeys(
            [NotNull] this IMutableEntityType entityType, [NotNull] IProperty property)
            => entityType.FindForeignKeys(new[] { property });

        public static IEnumerable<IMutableForeignKey> FindForeignKeys(
            [NotNull] this IMutableEntityType entityType, [NotNull] IReadOnlyList<IProperty> properties)
            => ((IEntityType)entityType).FindForeignKeys(properties).Cast<IMutableForeignKey>();

        public static IMutableForeignKey FindForeignKey(
            [NotNull] this IMutableEntityType entityType,
            [NotNull] IProperty property,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.FindForeignKey(new[] { property }, principalKey, principalEntityType);
        }

        public static IEnumerable<IMutableForeignKey> GetReferencingForeignKeys([NotNull] this IMutableEntityType entityType)
            => ((IEntityType)entityType).GetReferencingForeignKeys().Cast<IMutableForeignKey>();

        public static IMutableForeignKey AddForeignKey(
            [NotNull] this IMutableEntityType entityType,
            [NotNull] IMutableProperty property,
            [NotNull] IMutableKey principalKey,
            [NotNull] IMutableEntityType principalEntityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.AddForeignKey(new[] { property }, principalKey, principalEntityType);
        }

        public static IMutableForeignKey GetOrAddForeignKey(
            [NotNull] this IMutableEntityType entityType,
            [NotNull] IMutableProperty property,
            [NotNull] IMutableKey principalKey,
            [NotNull] IMutableEntityType principalEntityType)
            => entityType.GetOrAddForeignKey(new[] { property }, principalKey, principalEntityType);

        public static IMutableForeignKey GetOrAddForeignKey(
            [NotNull] this IMutableEntityType entityType,
            [NotNull] IReadOnlyList<IMutableProperty> properties,
            [NotNull] IMutableKey principalKey,
            [NotNull] IMutableEntityType principalEntityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.FindForeignKey(properties, principalKey, principalEntityType)
                   ?? entityType.AddForeignKey(properties, principalKey, principalEntityType);
        }

        public static IMutableNavigation FindNavigation(
            [NotNull] this IMutableEntityType entityType, [NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return entityType.FindNavigation(propertyInfo.Name);
        }

        public static IMutableNavigation FindNavigation([NotNull] this IMutableEntityType entityType, [NotNull] string name)
            => (IMutableNavigation)((IEntityType)entityType).FindNavigation(name);

        public static IEnumerable<IMutableNavigation> GetNavigations([NotNull] this IMutableEntityType entityType)
            => ((IEntityType)entityType).GetNavigations().Cast<IMutableNavigation>();

        public static IMutableProperty FindProperty([NotNull] this IMutableEntityType entityType, [NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return entityType.FindProperty(propertyInfo.Name);
        }

        public static IMutableProperty AddProperty(
            [NotNull] this IMutableEntityType entityType, [NotNull] string name, [NotNull] Type propertyType)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(propertyType, nameof(propertyType));

            var property = entityType.AddProperty(name);
            property.ClrType = propertyType;
            return property;
        }

        public static IMutableProperty AddProperty(
            [NotNull] this IMutableEntityType entityType, [NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(entityType, nameof(entityType));
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

        public static IMutableProperty GetOrAddProperty(
            [NotNull] this IMutableEntityType entityType, [NotNull] string name, [NotNull] Type propertyType)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(propertyType, nameof(propertyType));

            var property = entityType.FindProperty(name);
            if (property != null)
            {
                property.ClrType = propertyType;
                return property;
            }

            return entityType.AddProperty(name, propertyType);
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

        public static IMutableProperty GetOrAddProperty(
            [NotNull] this IMutableEntityType entityType, [NotNull] string name)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.FindProperty(name) ?? entityType.AddProperty(name);
        }

        public static IMutableIndex FindIndex([NotNull] this IMutableEntityType entityType, [NotNull] IProperty property)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.FindIndex(new[] { property });
        }

        public static IMutableIndex AddIndex(
            [NotNull] this IMutableEntityType entityType, [NotNull] IMutableProperty property)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.AddIndex(new[] { property });
        }

        public static IMutableIndex GetOrAddIndex(
            [NotNull] this IMutableEntityType entityType, [NotNull] IMutableProperty property)
            => entityType.GetOrAddIndex(new[] { property });

        public static IMutableIndex GetOrAddIndex(
            [NotNull] this IMutableEntityType entityType, [NotNull] IReadOnlyList<IMutableProperty> properties)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.FindIndex(properties) ?? entityType.AddIndex(properties);
        }
    }
}
