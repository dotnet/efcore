// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that finds entity types which share a table
    ///     which has a concurrency token column where those entity types
    ///     do not have a property mapped to that column. It then
    ///     creates a shadow concurrency property mapped to that column
    ///     on the base-most entity type(s).
    /// </summary>
    public class TableSharingConcurrencyTokenConvention : IModelFinalizingConvention
    {
        private const string ConcurrencyPropertyPrefix = "_TableSharingConcurrencyTokenConvention_";

        /// <summary>
        ///     Creates a new instance of <see cref="TableSharingConcurrencyTokenConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention. </param>
        public TableSharingConcurrencyTokenConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <inheritdoc />
        public virtual void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            var tableToEntityTypes = new Dictionary<(string Name, string Schema), List<IConventionEntityType>>();
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName == null)
                {
                    continue;
                }

                var table = (tableName, entityType.GetSchema());
                if (!tableToEntityTypes.TryGetValue(table, out var mappedTypes))
                {
                    mappedTypes = new List<IConventionEntityType>();
                    tableToEntityTypes[table] = mappedTypes;
                }

                mappedTypes.Add(entityType);
            }

            foreach (var tableToEntityType in tableToEntityTypes)
            {
                var table = tableToEntityType.Key;
                var mappedTypes = tableToEntityType.Value;

                var concurrencyColumns = GetConcurrencyTokensMap(StoreObjectIdentifier.Table(table.Name, table.Schema), mappedTypes);
                if (concurrencyColumns == null)
                {
                    continue;
                }

                foreach (var concurrencyColumn in concurrencyColumns)
                {
                    var concurrencyColumnName = concurrencyColumn.Key;
                    var propertiesMappedToConcurrencyColumn = concurrencyColumn.Value;

                    Dictionary<IConventionEntityType, IProperty> entityTypesMissingConcurrencyColumn = null;
                    foreach (var entityType in mappedTypes)
                    {
                        var foundMappedProperty = !IsConcurrencyTokenMissing(propertiesMappedToConcurrencyColumn, entityType, mappedTypes)
                            || entityType.GetProperties()
                                .Any(p => p.GetColumnName(StoreObjectIdentifier.Table(table.Name, table.Schema)) == concurrencyColumnName);

                        if (!foundMappedProperty)
                        {
                            if (entityTypesMissingConcurrencyColumn == null)
                            {
                                entityTypesMissingConcurrencyColumn = new Dictionary<IConventionEntityType, IProperty>();
                            }

                            // store the entity type which is missing the
                            // concurrency token property, mapped to an example
                            // property which _is_ mapped to this concurrency token
                            // column and which will be used later as a template
                            entityTypesMissingConcurrencyColumn.Add(
                                entityType, propertiesMappedToConcurrencyColumn.First());
                        }
                    }

                    if (entityTypesMissingConcurrencyColumn == null)
                    {
                        continue;
                    }

                    RemoveDerivedEntityTypes(entityTypesMissingConcurrencyColumn);

                    foreach (var entityTypeToExampleProperty in entityTypesMissingConcurrencyColumn)
                    {
                        var exampleProperty = entityTypeToExampleProperty.Value;
                        entityTypeToExampleProperty.Key.Builder.CreateUniqueProperty(
                                exampleProperty.ClrType,
                                ConcurrencyPropertyPrefix + exampleProperty.Name,
                                !exampleProperty.IsNullable)
                            .HasColumnName(concurrencyColumnName)
                            .HasColumnType(exampleProperty.GetColumnType())
                            .IsConcurrencyToken(true)
                            .ValueGenerated(exampleProperty.ValueGenerated);
                    }
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public static Dictionary<string, List<IProperty>> GetConcurrencyTokensMap(
            in StoreObjectIdentifier storeObject,
            [NotNull] IReadOnlyList<IEntityType> mappedTypes)
        {
            if (mappedTypes.Count < 2)
            {
                return null;
            }

            Dictionary<string, List<IProperty>> concurrencyColumns = null;
            var nonHierarchyTypesCount = 0;
            foreach (var entityType in mappedTypes)
            {
                if (entityType.BaseType == null
                    || !mappedTypes.Contains(entityType.BaseType))
                {
                    nonHierarchyTypesCount++;
                }

                foreach (var property in entityType.GetDeclaredProperties())
                {
                    if (!property.IsConcurrencyToken
                        || (property.ValueGenerated & ValueGenerated.OnUpdate) == 0)
                    {
                        continue;
                    }

                    var columnName = property.GetColumnName(storeObject);
                    if (columnName == null)
                    {
                        continue;
                    }

                    if (concurrencyColumns == null)
                    {
                        concurrencyColumns = new Dictionary<string, List<IProperty>>();
                    }

                    if (!concurrencyColumns.TryGetValue(columnName, out var properties))
                    {
                        properties = new List<IProperty>();
                        concurrencyColumns[columnName] = properties;
                    }

                    properties.Add(property);
                }
            }

            return nonHierarchyTypesCount < 2 ? null : concurrencyColumns;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public static bool IsConcurrencyTokenMissing(
            [NotNull] List<IProperty> propertiesMappedToConcurrencyColumn,
            [NotNull] IEntityType entityType,
            [NotNull] IReadOnlyList<IEntityType> mappedTypes)
        {
            if (entityType.FindPrimaryKey() == null)
            {
                return false;
            }

            var propertyMissing = false;
            foreach (var mappedProperty in propertiesMappedToConcurrencyColumn)
            {
                var declaringEntityType = mappedProperty.DeclaringEntityType;
                if (declaringEntityType.IsAssignableFrom(entityType)
                    || entityType.IsAssignableFrom(declaringEntityType)
                    || declaringEntityType.IsInOwnershipPath(entityType)
                    || entityType.IsInOwnershipPath(declaringEntityType))
                {
                    // The concurrency token is in the same hierarchy or in the same aggregate
                    continue;
                }

                var linkingFks = declaringEntityType.FindForeignKeys(declaringEntityType.FindPrimaryKey().Properties)
                    .Where(
                        fk => fk.PrincipalKey.IsPrimaryKey()
                            && mappedTypes.Contains(fk.PrincipalEntityType)).ToList();
                if (linkingFks.Count > 0
                    && !linkingFks.Any(fk => fk.PrincipalEntityType == entityType)
                    && linkingFks.Any(
                        fk => fk.PrincipalEntityType.IsAssignableFrom(entityType)
                            || entityType.IsAssignableFrom(fk.PrincipalEntityType)))
                {
                    // The concurrency token is on a type that shares the row with a base or derived type
                    continue;
                }

                propertyMissing = true;
                break;
            }

            return propertyMissing;
        }

        private static void RemoveDerivedEntityTypes<T>(Dictionary<IConventionEntityType, T> entityTypeDictionary)
        {
            var toRemove = new HashSet<KeyValuePair<IConventionEntityType, T>>();
            var entityTypesWithDerivedTypes =
                entityTypeDictionary.Where(e => e.Key.GetDirectlyDerivedTypes().Any()).ToList();
            foreach (var entityType in entityTypeDictionary.Where(e => e.Key.BaseType != null))
            {
                foreach (var otherEntityType in entityTypesWithDerivedTypes)
                {
                    if (toRemove.Contains(otherEntityType)
                        || otherEntityType.Equals(entityType))
                    {
                        continue;
                    }

                    if (otherEntityType.Key.IsAssignableFrom(entityType.Key))
                    {
                        toRemove.Add(entityType);
                        break;
                    }
                }
            }

            foreach (var entityType in toRemove)
            {
                entityTypeDictionary.Remove(entityType.Key);
            }
        }
    }
}
