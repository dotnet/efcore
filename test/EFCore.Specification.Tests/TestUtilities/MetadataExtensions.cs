// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public static class MetadataExtensions
    {
        public static IQueryable<TEntity> AsTracking<TEntity>(
            this IQueryable<TEntity> source,
            bool tracking)
            where TEntity : class
            => tracking ? source.AsTracking() : source.AsNoTracking();

        public static IEnumerable<T> NullChecked<T>(this IEnumerable<T> enumerable)
            => enumerable ?? Enumerable.Empty<T>();

        public static void ForEach<T>(this IEnumerable<T> @this, Action<T> action)
        {
            foreach (var item in @this)
            {
                action(item);
            }
        }

        public static IModel Clone(this IModel model)
        {
            IMutableModel modelClone = new Model();
            var clonedEntityTypes = new Dictionary<IEntityType, IMutableEntityType>();
            foreach (var entityType in model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;
                var clonedEntityType = clrType == null
                    ? modelClone.AddEntityType(entityType.Name)
                    : modelClone.AddEntityType(clrType);

                clonedEntityTypes.Add(entityType, clonedEntityType);
            }

            foreach (var clonedEntityType in clonedEntityTypes)
            {
                if (clonedEntityType.Key.BaseType != null)
                {
                    clonedEntityType.Value.BaseType = clonedEntityTypes[clonedEntityType.Key.BaseType];
                }
            }

            foreach (var clonedEntityType in clonedEntityTypes)
            {
                CloneProperties(clonedEntityType.Key, clonedEntityType.Value);
            }

            foreach (var clonedEntityType in clonedEntityTypes)
            {
                CloneIndexes(clonedEntityType.Key, clonedEntityType.Value);
            }

            foreach (var clonedEntityType in clonedEntityTypes)
            {
                CloneKeys(clonedEntityType.Key, clonedEntityType.Value);
            }

            foreach (var clonedEntityType in clonedEntityTypes)
            {
                CloneForeignKeys(clonedEntityType.Key, clonedEntityType.Value);
            }

            foreach (var clonedEntityType in clonedEntityTypes)
            {
                CloneNavigations(clonedEntityType.Key, clonedEntityType.Value);
            }

            return modelClone;
        }

        private static void CloneProperties(IEntityType sourceEntityType, IMutableEntityType targetEntityType)
        {
            foreach (var property in sourceEntityType.GetDeclaredProperties())
            {
                var clonedProperty = targetEntityType.AddProperty(property.Name, property.ClrType);
                clonedProperty.IsNullable = property.IsNullable;
                clonedProperty.IsConcurrencyToken = property.IsConcurrencyToken;
                clonedProperty.ValueGenerated = property.ValueGenerated;
                clonedProperty.SetBeforeSaveBehavior(property.GetBeforeSaveBehavior());
                clonedProperty.SetAfterSaveBehavior(property.GetAfterSaveBehavior());
                property.GetAnnotations().ForEach(annotation => clonedProperty[annotation.Name] = annotation.Value);
            }
        }

        private static void CloneKeys(IEntityType sourceEntityType, IMutableEntityType targetEntityType)
        {
            foreach (var key in sourceEntityType.GetDeclaredKeys())
            {
                var clonedKey = targetEntityType.AddKey(
                    key.Properties.Select(p => targetEntityType.FindProperty(p.Name)).ToList());
                if (key.IsPrimaryKey())
                {
                    targetEntityType.SetPrimaryKey(clonedKey.Properties);
                }

                key.GetAnnotations().ForEach(annotation => clonedKey[annotation.Name] = annotation.Value);
            }
        }

        private static void CloneIndexes(IEntityType sourceEntityType, IMutableEntityType targetEntityType)
        {
            foreach (var index in sourceEntityType.GetDeclaredIndexes())
            {
                var clonedIndex = targetEntityType.AddIndex(
                    index.Properties.Select(p => targetEntityType.FindProperty(p.Name)).ToList());
                clonedIndex.IsUnique = index.IsUnique;
                index.GetAnnotations().ForEach(annotation => clonedIndex[annotation.Name] = annotation.Value);
            }
        }

        private static void CloneForeignKeys(IEntityType sourceEntityType, IMutableEntityType targetEntityType)
        {
            foreach (var foreignKey in sourceEntityType.GetDeclaredForeignKeys())
            {
                var targetPrincipalEntityType = targetEntityType.Model.FindEntityType(foreignKey.PrincipalEntityType.Name);
                var clonedForeignKey = targetEntityType.AddForeignKey(
                    foreignKey.Properties.Select(p => targetEntityType.FindProperty(p.Name)).ToList(),
                    targetPrincipalEntityType.FindKey(
                        foreignKey.PrincipalKey.Properties.Select(p => targetPrincipalEntityType.FindProperty(p.Name)).ToList()),
                    targetPrincipalEntityType);
                clonedForeignKey.IsUnique = foreignKey.IsUnique;
                clonedForeignKey.IsRequired = foreignKey.IsRequired;
                foreignKey.GetAnnotations().ForEach(annotation => clonedForeignKey[annotation.Name] = annotation.Value);
            }
        }

        private static void CloneNavigations(IEntityType sourceEntityType, IMutableEntityType targetEntityType)
        {
            foreach (var navigation in sourceEntityType.GetDeclaredNavigations())
            {
                var targetDependentEntityType = targetEntityType.Model.FindEntityType(navigation.ForeignKey.DeclaringEntityType.Name);
                var targetPrincipalEntityType = targetEntityType.Model.FindEntityType(navigation.ForeignKey.PrincipalEntityType.Name);
                var targetForeignKey = targetDependentEntityType.FindForeignKey(
                    navigation.ForeignKey.Properties.Select(p => targetDependentEntityType.FindProperty(p.Name)).ToList(),
                    targetPrincipalEntityType.FindKey(
                        navigation.ForeignKey.PrincipalKey.Properties.Select(
                            p => targetPrincipalEntityType.FindProperty(p.Name)).ToList()),
                    targetPrincipalEntityType);
                var clonedNavigation = navigation.IsOnDependent
                    ? (navigation.GetIdentifyingMemberInfo() != null
                        ? targetForeignKey.SetDependentToPrincipal(navigation.GetIdentifyingMemberInfo())
                        : targetForeignKey.SetDependentToPrincipal(navigation.Name))
                    : (navigation.GetIdentifyingMemberInfo() != null
                        ? targetForeignKey.SetPrincipalToDependent(navigation.GetIdentifyingMemberInfo())
                        : targetForeignKey.SetPrincipalToDependent(navigation.Name));
                navigation.GetAnnotations().ForEach(annotation => clonedNavigation[annotation.Name] = annotation.Value);
            }
        }
    }
}
