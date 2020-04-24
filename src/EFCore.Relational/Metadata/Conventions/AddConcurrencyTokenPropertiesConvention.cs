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
    public class AddConcurrencyTokenPropertiesConvention : IModelFinalizingConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="AddConcurrencyTokenPropertiesConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public AddConcurrencyTokenPropertiesConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        public static readonly string ConcurrencyPropertyPrefix = "_concurrency_";

        /// <inheritdoc />
        public virtual void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            var maxIdentifierLength = modelBuilder.Metadata.GetMaxIdentifierLength();

            GetMappings(modelBuilder.Metadata,
                out var tableToEntityTypes, out var concurrencyColumnsToProperties);

            foreach (var table in tableToEntityTypes)
            {
                var tableName = table.Key;
                if (!concurrencyColumnsToProperties.TryGetValue(tableName, out var concurrencyColumns))
                {
                    continue; // this table has no mapped concurrency columns
                }

                var entityTypesMappedToTable = table.Value;

                foreach (var concurrencyColumn in concurrencyColumns)
                {
                    var concurrencyColumnName = concurrencyColumn.Key;
                    var propertiesMappedToConcurrencyColumn = concurrencyColumn.Value;

                    var entityTypesMissingConcurrencyColumn =
                        new Dictionary<IConventionEntityType, IProperty>();
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
                                .Any(p => p.GetColumnName() == concurrencyColumnName);

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

                    foreach(var entityTypeToExampleProperty in
                        BasestEntities(entityTypesMissingConcurrencyColumn))
                    {
                        var entityType = entityTypeToExampleProperty.Key;
                        var exampleProperty = entityTypeToExampleProperty.Value;
                        var allExistingProperties =
                            entityType.GetProperties().Select(p => p.Name)
                            .Union(entityType.GetNavigations().Select(p => p.Name))
                            .ToDictionary(s => s, s => 0);
                        var concurrencyShadowPropertyName =
                            Uniquifier.Uniquify(ConcurrencyPropertyPrefix + concurrencyColumnName, allExistingProperties, maxIdentifierLength);
                        var concurrencyShadowProperty =
                            entityType.AddProperty(concurrencyShadowPropertyName, exampleProperty.ClrType, null);
                        concurrencyShadowProperty.SetColumnName(concurrencyColumnName);
                        concurrencyShadowProperty.SetIsConcurrencyToken(true);
                        concurrencyShadowProperty.SetValueGenerated(exampleProperty.ValueGenerated);
                    }
                }
            }
        }

        private void GetMappings(IConventionModel model,
            out Dictionary<string, IList<IConventionEntityType>> tableToEntityTypes,
            out Dictionary<string, Dictionary<string, IList<IProperty>>> concurrencyColumnsToProperties)
        {
            tableToEntityTypes = new Dictionary<string, IList<IConventionEntityType>>();
            concurrencyColumnsToProperties = new Dictionary<string, Dictionary<string, IList<IProperty>>>();
            foreach (var entityType in model.GetEntityTypes())
            {
                var tableName = entityType.GetSchemaQualifiedTableName();
                if (tableName == null)
                {
                    continue; // unmapped entityType
                }

                if (!tableToEntityTypes.TryGetValue(tableName, out var mappedTypes))
                {
                    mappedTypes = new List<IConventionEntityType>();
                    tableToEntityTypes[tableName] = mappedTypes;
                }

                mappedTypes.Add(entityType);

                foreach (var property in entityType.GetDeclaredProperties())
                {
                    if (property.IsConcurrencyToken
                        && (property.ValueGenerated & ValueGenerated.OnUpdate) != 0)
                    {
                        if (!concurrencyColumnsToProperties.TryGetValue(tableName, out var columnToProperties))
                        {
                            columnToProperties = new Dictionary<string, IList<IProperty>>();
                            concurrencyColumnsToProperties[tableName] = columnToProperties;
                        }

                        var columnName = property.GetColumnName();
                        if (!columnToProperties.TryGetValue(columnName, out var properties))
                        {
                            properties = new List<IProperty>();
                            columnToProperties[columnName] = properties;
                        }

                        properties.Add(property);
                    }
                }
            }
        }

        // Given a (distinct) IEnumerable of EntityTypes (mapped to T),
        // return only the mappings where the EntityType does not inherit
        // from any other EntityType in the list.
        private static IEnumerable<KeyValuePair<IConventionEntityType, T>> BasestEntities<T>(
            IEnumerable<KeyValuePair<IConventionEntityType, T>> entityTypeDictionary)
        {
            var toRemove = new HashSet<KeyValuePair<IConventionEntityType, T>>();
            foreach (var entityType in entityTypeDictionary)
            {
                var otherEntityTypes = entityTypeDictionary
                    .Except(new[] { entityType }).Except(toRemove);
                foreach (var otherEntityType in otherEntityTypes)
                {
                    if (otherEntityType.Key.IsAssignableFrom(entityType.Key))
                    {
                        toRemove.Add(entityType);
                        break;
                    }
                }
            }

            return entityTypeDictionary.Except(toRemove);
        }
    }
}
