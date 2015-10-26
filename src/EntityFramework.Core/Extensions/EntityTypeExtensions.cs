// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
    public static class EntityTypeExtensions
    {
        public static IEnumerable<IEntityType> GetDerivedTypes([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var derivedType in entityType.Model.GetEntityTypes())
            {
                if (derivedType.BaseType != null
                    && derivedType != entityType
                    && entityType.IsAssignableFrom(derivedType))
                {
                    yield return derivedType;
                }
            }
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

        public static IKey FindKey([NotNull] this IEntityType entityType, [NotNull] IProperty property)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.FindKey(new[] { property });
        }

        public static IEnumerable<IForeignKey> FindForeignKeys([NotNull] this IEntityType entityType, [NotNull] IProperty property)
            => entityType.FindForeignKeys(new[] { property });

        public static IEnumerable<IForeignKey> FindForeignKeys(
            [NotNull] this IEntityType entityType, [NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));

            return entityType.GetForeignKeys()
                .Where(foreignKey => PropertyListComparer.Instance.Equals(foreignKey.Properties, properties));
        }

        public static IForeignKey FindForeignKey(
            [NotNull] this IEntityType entityType,
            [NotNull] IProperty property,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.FindForeignKey(new[] { property }, principalKey, principalEntityType);
        }

        public static IEnumerable<IForeignKey> GetReferencingForeignKeys([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.Model.GetEntityTypes().SelectMany(et => et.GetDeclaredForeignKeys())
                .Where(fk => fk.PrincipalEntityType.IsAssignableFrom(entityType));
        }

        public static INavigation FindNavigation([NotNull] this IEntityType entityType, [NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return entityType.FindNavigation(propertyInfo.Name);
        }

        public static INavigation FindNavigation([NotNull] this IEntityType entityType, [NotNull] string name)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(name, nameof(name));

            return entityType.GetNavigations().FirstOrDefault(n => n.Name.Equals(name));
        }

        public static IEnumerable<INavigation> GetNavigations([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            var fastEntityType = entityType as ICanGetNavigations;
            if (fastEntityType != null)
            {
                return fastEntityType.GetNavigations();
            }

            return entityType.BaseType?.GetNavigations().Concat(entityType.GetDeclaredNavigations())
                   ?? entityType.GetDeclaredNavigations();
        }

        public static IProperty FindProperty([NotNull] this IEntityType entityType, [NotNull] PropertyInfo propertyInfo)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            return entityType.FindProperty(propertyInfo.Name);
        }

        public static IIndex FindIndex([NotNull] this IEntityType entityType, [NotNull] IProperty property)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.FindIndex(new[] { property });
        }
    }
}
