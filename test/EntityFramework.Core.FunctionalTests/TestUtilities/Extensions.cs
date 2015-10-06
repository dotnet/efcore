// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Entity.FunctionalTests
{
    public static class Extensions
    {
        public static IServiceCollection ServiceCollection(this EntityFrameworkServicesBuilder builder)
            => builder.GetService();

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
            var modelClone = new Model();
            var clonedEntityTypes = new Dictionary<IEntityType, EntityType>();
            foreach (var entityType in model.EntityTypes)
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

        private static void CloneProperties(IEntityType sourceEntityType, EntityType targetEntityType)
        {
            foreach (var property in sourceEntityType.GetDeclaredProperties())
            {
                var clonedProperty = targetEntityType.AddProperty(property.Name);
                clonedProperty.ClrType = property.ClrType;
                clonedProperty.IsShadowProperty = property.IsShadowProperty;
                clonedProperty.IsNullable = property.IsNullable;
                clonedProperty.IsConcurrencyToken = property.IsConcurrencyToken;
                clonedProperty.RequiresValueGenerator = property.RequiresValueGenerator;
                clonedProperty.ValueGenerated = property.ValueGenerated;
                clonedProperty.IsReadOnlyBeforeSave = property.IsReadOnlyBeforeSave;
                clonedProperty.IsReadOnlyAfterSave = property.IsReadOnlyAfterSave;
                property.Annotations.ForEach(annotation => clonedProperty[annotation.Name] = annotation.Value);
            }
        }

        private static void CloneKeys(IEntityType sourceEntityType, EntityType targetEntityType)
        {
            foreach (var key in sourceEntityType.GetDeclaredKeys())
            {
                var clonedKey = targetEntityType.AddKey(
                    key.Properties.Select(p => targetEntityType.GetProperty(p.Name)).ToList());
                if (key.IsPrimaryKey())
                {
                    targetEntityType.SetPrimaryKey(clonedKey.Properties);
                }
                key.Annotations.ForEach(annotation => clonedKey[annotation.Name] = annotation.Value);
            }
        }

        private static void CloneIndexes(IEntityType sourceEntityType, EntityType targetEntityType)
        {
            foreach (var index in sourceEntityType.GetDeclaredIndexes())
            {
                var clonedIndex = targetEntityType.AddIndex(
                    index.Properties.Select(p => targetEntityType.GetProperty(p.Name)).ToList());
                clonedIndex.IsUnique = index.IsUnique;
                index.Annotations.ForEach(annotation => clonedIndex[annotation.Name] = annotation.Value);
            }
        }

        private static void CloneForeignKeys(IEntityType sourceEntityType, EntityType targetEntityType)
        {
            foreach (var foreignKey in sourceEntityType.GetDeclaredForeignKeys())
            {
                var targetPrincipalEntityType = targetEntityType.Model.GetEntityType(foreignKey.PrincipalEntityType.Name);
                var clonedForeignKey = targetEntityType.AddForeignKey(
                    foreignKey.Properties.Select(p => targetEntityType.GetProperty(p.Name)).ToList(),
                    targetPrincipalEntityType.GetKey(
                        foreignKey.PrincipalKey.Properties.Select(p => targetPrincipalEntityType.GetProperty(p.Name)).ToList()),
                    targetPrincipalEntityType);
                clonedForeignKey.IsUnique = foreignKey.IsUnique;
                clonedForeignKey.IsRequired = foreignKey.IsRequired;
                foreignKey.Annotations.ForEach(annotation => clonedForeignKey[annotation.Name] = annotation.Value);
            }
        }

        private static void CloneNavigations(IEntityType sourceEntityType, EntityType targetEntityType)
        {
            foreach (var navigation in sourceEntityType.GetDeclaredNavigations())
            {
                var targetDependentEntityType = targetEntityType.Model.GetEntityType(navigation.ForeignKey.DeclaringEntityType.Name);
                var targetForeignKey = targetDependentEntityType.GetForeignKey(
                    navigation.ForeignKey.Properties.Select(p => targetDependentEntityType.GetProperty(p.Name)).ToList());
                var clonedNavigation = targetEntityType.AddNavigation(navigation.Name, targetForeignKey, pointsToPrincipal: navigation.PointsToPrincipal());
                navigation.Annotations.ForEach(annotation => clonedNavigation[annotation.Name] = annotation.Value);
            }
        }
    }
}
