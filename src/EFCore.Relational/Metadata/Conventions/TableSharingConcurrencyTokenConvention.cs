// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
            GetMappings(modelBuilder.Metadata,
                out var tableToEntityTypes, out var concurrencyColumnsToProperties);

            foreach (var tableToEntityType in tableToEntityTypes)
            {
                var table = tableToEntityType.Key;
                if (!concurrencyColumnsToProperties.TryGetValue(table, out var concurrencyColumns))
                {
                    continue; // this table has no mapped concurrency columns
                }

                var entityTypesMappedToTable = tableToEntityType.Value;

                foreach (var concurrencyColumn in concurrencyColumns)
                {
                    var concurrencyColumnName = concurrencyColumn.Key;
                    var propertiesMappedToConcurrencyColumn = concurrencyColumn.Value;

                    var entityTypesMissingConcurrencyColumn =
                        new Dictionary<IConventionEntityType, IConventionProperty>();
                    foreach (var entityType in entityTypesMappedToTable)
                    {
                        var foundMappedProperty = false;
                        foreach (var mappedProperty in propertiesMappedToConcurrencyColumn)
                        {
                            var declaringEntityType = mappedProperty.DeclaringEntityType;
                            if (declaringEntityType.IsAssignableFrom(entityType)
                                || declaringEntityType.IsInOwnershipPath(entityType)
                                || entityType.IsInOwnershipPath(declaringEntityType))
                            {
                                foundMappedProperty = true;
                                break;
                            }
                        }

                        foundMappedProperty = foundMappedProperty
                            || entityType.GetAllBaseTypes().SelectMany(t => t.GetDeclaredProperties())
                                .Any(p => p.GetColumnName(table.Table, table.Schema) == concurrencyColumnName);

                        if (!foundMappedProperty)
                        {
                            // store the entity type which is missing the
                            // concurrency token property, mapped to an example
                            // property which _is_ mapped to this concurrency token
                            // column and which will be used later as a template
                            entityTypesMissingConcurrencyColumn.Add(
                                entityType, propertiesMappedToConcurrencyColumn.First());
                        }
                    }

                    RemoveDerivedEntityTypes(ref entityTypesMissingConcurrencyColumn);

                    foreach(var entityTypeToExampleProperty in entityTypesMissingConcurrencyColumn)
                    {
                        var entityType = entityTypeToExampleProperty.Key;
                        var exampleProperty = entityTypeToExampleProperty.Value;
                        var concurrencyShadowPropertyBuilder =
#pragma warning disable EF1001 // Internal EF Core API usage.
                            ((InternalEntityTypeBuilder)entityType.Builder).CreateUniqueProperty(
                                ConcurrencyPropertyPrefix + exampleProperty.Name,
                                exampleProperty.ClrType,
                                !exampleProperty.IsNullable).Builder;
                        concurrencyShadowPropertyBuilder
                            .HasColumnName(concurrencyColumnName)
                            .HasColumnType(exampleProperty.GetColumnType())
                            ?.IsConcurrencyToken(true)
                            ?.ValueGenerated(exampleProperty.ValueGenerated);
#pragma warning restore EF1001 // Internal EF Core API usage.
                    }
                }
            }
        }

        private void GetMappings(IConventionModel model,
            out Dictionary<(string Table, string Schema), IList<IConventionEntityType>> tableToEntityTypes,
            out Dictionary<(string Table, string Schema), Dictionary<string, IList<IConventionProperty>>> concurrencyColumnsToProperties)
        {
            tableToEntityTypes = new Dictionary<(string Table, string Schema), IList<IConventionEntityType>>();
            concurrencyColumnsToProperties = new Dictionary<(string Table, string Schema), Dictionary<string, IList<IConventionProperty>>>();
            foreach (var entityType in model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName == null)
                {
                    continue; // unmapped entityType
                }

                var table = (Name: tableName, Schema: entityType.GetSchema());

                if (!tableToEntityTypes.TryGetValue(table, out var mappedTypes))
                {
                    mappedTypes = new List<IConventionEntityType>();
                    tableToEntityTypes[table] = mappedTypes;
                }

                mappedTypes.Add(entityType);

                foreach (var property in entityType.GetDeclaredProperties())
                {
                    if (property.IsConcurrencyToken)
                    {
                        if (!concurrencyColumnsToProperties.TryGetValue(table, out var columnToProperties))
                        {
                            columnToProperties = new Dictionary<string, IList<IConventionProperty>>();
                            concurrencyColumnsToProperties[table] = columnToProperties;
                        }

                        var columnName = property.GetColumnName(tableName, table.Schema);
                        if (columnName == null)
                        {
                            continue;
                        }

                        if (!columnToProperties.TryGetValue(columnName, out var properties))
                        {
                            properties = new List<IConventionProperty>();
                            columnToProperties[columnName] = properties;
                        }

                        properties.Add(property);
                    }
                }
            }
        }

        // Given a Dictionary of EntityTypes (mapped to T), remove
        // any mappings where the EntityType inherits from any
        // other EntityType in the Dictionary.
        private static void RemoveDerivedEntityTypes<T>(
            ref Dictionary<IConventionEntityType, T> entityTypeDictionary)
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
