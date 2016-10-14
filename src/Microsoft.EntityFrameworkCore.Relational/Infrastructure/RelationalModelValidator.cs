// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     The validator that enforces rules common for all relational providers.
    /// </summary>
    public class RelationalModelValidator : ModelValidator
    {
        /// <summary>
        ///     Gets the relational annotation provider.
        /// </summary>
        /// <value>
        ///     The relational annotation provider.
        /// </value>
        protected virtual IRelationalAnnotationProvider RelationalExtensions { get; }

        /// <summary>
        ///     Gets the type mapper.
        /// </summary>
        /// <value>
        ///     The type mapper.
        /// </value>
        protected virtual IRelationalTypeMapper TypeMapper { get; }

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalModelValidator" />.
        /// </summary>
        /// <param name="loggerFactory"> The logger factory. </param>
        /// <param name="relationalExtensions"> The relational annotation provider. </param>
        /// <param name="typeMapper"> The type mapper. </param>
        public RelationalModelValidator(
            [NotNull] ILogger<RelationalModelValidator> loggerFactory,
            [NotNull] IRelationalAnnotationProvider relationalExtensions,
            [NotNull] IRelationalTypeMapper typeMapper)
            : base(loggerFactory)
        {
            RelationalExtensions = relationalExtensions;
            TypeMapper = typeMapper;
        }

        /// <summary>
        ///     Validates a model, throwing an exception if any errors are found.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        public override void Validate(IModel model)
        {
            EnsureDistinctTableNames(model);
            EnsureSharedColumnsCompatibility(model);
            EnsureSharedForeignKeysCompatibility(model);
            EnsureSharedIndexesCompatibility(model);
            ValidateInheritanceMapping(model);
            EnsureDataTypes(model);
            EnsureNoDefaultValuesOnKeys(model);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void EnsureDataTypes([NotNull] IModel model)
        {
            foreach (var entityType in model.GetEntityTypes())
            {
                foreach (var property in entityType.GetDeclaredProperties())
                {
                    var dataType = RelationalExtensions.For(property).ColumnType;
                    if (dataType != null)
                    {
                        TypeMapper.ValidateTypeName(dataType);
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void EnsureNoDefaultValuesOnKeys([NotNull] IModel model)
        {
            foreach (var property in model.GetEntityTypes().SelectMany(
                t => t.GetDeclaredKeys().SelectMany(
                    k => k.Properties))
                .Where(p => RelationalExtensions.For(p).DefaultValue != null))
            {
                ShowWarning(RelationalEventId.ModelValidationKeyDefaultValueWarning,
                    RelationalStrings.KeyHasDefaultValue(property.Name, property.DeclaringEntityType.DisplayName()));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void EnsureDistinctTableNames([NotNull] IModel model)
        {
            var tables = new HashSet<string>();
            foreach (var entityType in model.GetRootEntityTypes())
            {
                var annotations = RelationalExtensions.For(entityType);

                var name = Format(annotations.Schema, annotations.TableName);

                if (!tables.Add(name))
                {
                    ShowError(RelationalStrings.DuplicateTableName(annotations.TableName, annotations.Schema, entityType.DisplayName()));
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void EnsureSharedColumnsCompatibility([NotNull] IModel model)
        {
            foreach (var rootEntityType in model.GetRootEntityTypes())
            {
                var annotations = RelationalExtensions.For(rootEntityType);
                var table = Format(annotations.Schema, annotations.TableName);
                var properties = rootEntityType.GetDerivedTypesInclusive().SelectMany(et => et.GetDeclaredProperties());
                var propertyTypeMappings = new Dictionary<string, IProperty>(StringComparer.OrdinalIgnoreCase);

                foreach (var property in properties)
                {
                    var propertyAnnotations = RelationalExtensions.For(property);
                    var columnName = propertyAnnotations.ColumnName;
                    IProperty duplicateProperty;
                    if (propertyTypeMappings.TryGetValue(columnName, out duplicateProperty))
                    {
                        var previousAnnotations = RelationalExtensions.For(duplicateProperty);
                        var currentTypeString = propertyAnnotations.ColumnType
                                                ?? TypeMapper.GetMapping(property).StoreType;
                        var previousTypeString = previousAnnotations.ColumnType
                                                 ?? TypeMapper.GetMapping(duplicateProperty).StoreType;
                        if (!currentTypeString.Equals(previousTypeString, StringComparison.OrdinalIgnoreCase))
                        {
                            ShowError(RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                                duplicateProperty.DeclaringEntityType.DisplayName(),
                                duplicateProperty.Name,
                                property.DeclaringEntityType.DisplayName(),
                                property.Name,
                                columnName,
                                table,
                                previousTypeString,
                                currentTypeString));
                        }

                        if (property.IsColumnNullable() != duplicateProperty.IsColumnNullable())
                        {
                            ShowError(RelationalStrings.DuplicateColumnNameNullabilityMismatch(
                                duplicateProperty.DeclaringEntityType.DisplayName(),
                                duplicateProperty.Name,
                                property.DeclaringEntityType.DisplayName(),
                                property.Name,
                                columnName,
                                table));
                        }

                        var currentComputedColumnSql = propertyAnnotations.ComputedColumnSql ?? "";
                        var previousComputedColumnSql = previousAnnotations.ComputedColumnSql ?? "";
                        if (!currentComputedColumnSql.Equals(previousComputedColumnSql, StringComparison.OrdinalIgnoreCase))
                        {
                            ShowError(RelationalStrings.DuplicateColumnNameComputedSqlMismatch(
                                duplicateProperty.DeclaringEntityType.DisplayName(),
                                duplicateProperty.Name,
                                property.DeclaringEntityType.DisplayName(),
                                property.Name,
                                columnName,
                                table,
                                previousComputedColumnSql,
                                currentComputedColumnSql));
                        }

                        var currentDefaultValue = propertyAnnotations.DefaultValue;
                        var previousDefaultValue = previousAnnotations.DefaultValue;
                        if (!Equals(currentDefaultValue, previousDefaultValue))
                        {
                            ShowError(RelationalStrings.DuplicateColumnNameDefaultSqlMismatch(
                                duplicateProperty.DeclaringEntityType.DisplayName(),
                                duplicateProperty.Name,
                                property.DeclaringEntityType.DisplayName(),
                                property.Name,
                                columnName,
                                table,
                                previousDefaultValue ?? "NULL",
                                currentDefaultValue ?? "NULL"));
                        }

                        var currentDefaultValueSql = propertyAnnotations.DefaultValueSql ?? "";
                        var previousDefaultValueSql = previousAnnotations.DefaultValueSql ?? "";
                        if (!currentDefaultValueSql.Equals(previousDefaultValueSql, StringComparison.OrdinalIgnoreCase))
                        {
                            ShowError(RelationalStrings.DuplicateColumnNameDefaultSqlMismatch(
                                duplicateProperty.DeclaringEntityType.DisplayName(),
                                duplicateProperty.Name,
                                property.DeclaringEntityType.DisplayName(),
                                property.Name,
                                columnName,
                                table,
                                previousDefaultValueSql,
                                currentDefaultValueSql));
                        }
                    }
                    else
                    {
                        propertyTypeMappings[columnName] = property;
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void EnsureSharedForeignKeysCompatibility([NotNull] IModel model)
        {
            foreach (var rootEntityType in model.GetRootEntityTypes())
            {
                var annotations = RelationalExtensions.For(rootEntityType);
                var tableName = Format(annotations.Schema, annotations.TableName);
                var foreignKeys = rootEntityType.GetDerivedTypesInclusive().SelectMany(et => et.GetDeclaredForeignKeys());
                var foreignKeyMappings = new Dictionary<string, IForeignKey>(StringComparer.OrdinalIgnoreCase);

                foreach (var foreignKey in foreignKeys)
                {
                    var foreignKeyAnnotations = RelationalExtensions.For(foreignKey);
                    var foreignKeyName = foreignKeyAnnotations.Name;

                    IForeignKey duplicateForeignKey;
                    if (!foreignKeyMappings.TryGetValue(foreignKeyName, out duplicateForeignKey))
                    {
                        foreignKeyMappings[foreignKeyName] = foreignKey;
                        continue;
                    }

                    var principalAnnotations = RelationalExtensions.For(foreignKey.PrincipalEntityType);
                    var principalTable = Format(principalAnnotations.Schema, principalAnnotations.TableName);
                    var duplicateAnnotations = RelationalExtensions.For(duplicateForeignKey.PrincipalEntityType);
                    var duplicatePrincipalTable = Format(duplicateAnnotations.Schema, duplicateAnnotations.TableName);
                    if (!string.Equals(principalTable, duplicatePrincipalTable, StringComparison.OrdinalIgnoreCase))
                    {
                        ShowError(RelationalStrings.DuplicateForeignKeyPrincipalTableMismatch(
                            Property.Format(foreignKey.Properties),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            Property.Format(duplicateForeignKey.Properties),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            tableName,
                            foreignKeyName,
                            principalTable,
                            duplicatePrincipalTable));
                    }

                    var foreignKeyColumns = foreignKey.Properties.Select(p => RelationalExtensions.For(p).ColumnName).ToList();
                    var duplicateForeignKeyColumns = duplicateForeignKey.Properties.Select(p => RelationalExtensions.For(p).ColumnName).ToList();
                    if (!foreignKeyColumns.SequenceEqual(duplicateForeignKeyColumns, StringComparer.OrdinalIgnoreCase))
                    {
                        ShowError(RelationalStrings.DuplicateForeignKeyColumnMismatch(
                            Property.Format(foreignKey.Properties),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            Property.Format(duplicateForeignKey.Properties),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            tableName,
                            foreignKeyName,
                            Format(foreignKeyColumns),
                            Format(duplicateForeignKeyColumns)));
                    }

                    var foreignKeyPrincipalColumns = foreignKey.PrincipalKey.Properties.Select(
                        p => RelationalExtensions.For(p).ColumnName).ToList();
                    var duplicateForeignKeyPrincipalColumns = duplicateForeignKey.PrincipalKey.Properties.Select(
                        p => RelationalExtensions.For(p).ColumnName).ToList();
                    if (!foreignKeyPrincipalColumns.SequenceEqual(duplicateForeignKeyPrincipalColumns, StringComparer.OrdinalIgnoreCase))
                    {
                        ShowError(RelationalStrings.DuplicateForeignKeyPrincipalColumnMismatch(
                            Property.Format(foreignKey.Properties),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            Property.Format(duplicateForeignKey.Properties),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            tableName,
                            foreignKeyName,
                            Format(foreignKeyPrincipalColumns),
                            Format(duplicateForeignKeyPrincipalColumns)));
                    }

                    if (foreignKey.IsUnique != duplicateForeignKey.IsUnique)
                    {
                        ShowError(RelationalStrings.DuplicateForeignKeyUniquenessMismatch(
                            Property.Format(foreignKey.Properties),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            Property.Format(duplicateForeignKey.Properties),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            tableName,
                            foreignKeyName));
                    }

                    if (foreignKey.IsRequired != duplicateForeignKey.IsRequired)
                    {
                        ShowError(RelationalStrings.DuplicateForeignKeyUniquenessMismatch(
                            Property.Format(foreignKey.Properties),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            Property.Format(duplicateForeignKey.Properties),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            tableName,
                            foreignKeyName));
                    }

                    if (foreignKey.DeleteBehavior != duplicateForeignKey.DeleteBehavior)
                    {
                        ShowError(RelationalStrings.DuplicateForeignKeyDeleteBehaviorMismatch(
                            Property.Format(foreignKey.Properties),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            Property.Format(duplicateForeignKey.Properties),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            tableName,
                            foreignKeyName,
                            foreignKey.DeleteBehavior,
                            duplicateForeignKey.DeleteBehavior));
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void EnsureSharedIndexesCompatibility([NotNull] IModel model)
        {
            foreach (var rootEntityType in model.GetRootEntityTypes())
            {
                var annotations = RelationalExtensions.For(rootEntityType);
                var tableName = Format(annotations.Schema, annotations.TableName);
                var indexes = rootEntityType.GetDerivedTypesInclusive().SelectMany(et => et.GetDeclaredIndexes());
                var indexMappings = new Dictionary<string, IIndex>(StringComparer.OrdinalIgnoreCase);

                foreach (var index in indexes)
                {
                    var indexAnnotations = RelationalExtensions.For(index);
                    var indexName = indexAnnotations.Name;

                    IIndex duplicateIndex;
                    if (!indexMappings.TryGetValue(indexName, out duplicateIndex))
                    {
                        indexMappings[indexName] = index;
                        continue;
                    }

                    var indexColumns = index.Properties.Select(p => RelationalExtensions.For(p).ColumnName).ToList();
                    var duplicateIndexColumns = duplicateIndex.Properties.Select(p => RelationalExtensions.For(p).ColumnName).ToList();
                    if (!indexColumns.SequenceEqual(duplicateIndexColumns, StringComparer.OrdinalIgnoreCase))
                    {
                        ShowError(RelationalStrings.DuplicateIndexColumnMismatch(
                            Property.Format(index.Properties),
                            index.DeclaringEntityType.DisplayName(),
                            Property.Format(duplicateIndex.Properties),
                            duplicateIndex.DeclaringEntityType.DisplayName(),
                            tableName,
                            indexName,
                            Format(indexColumns),
                            Format(duplicateIndexColumns)));
                    }

                    if (index.IsUnique != duplicateIndex.IsUnique)
                    {
                        ShowError(RelationalStrings.DuplicateIndexUniquenessMismatch(
                            Property.Format(index.Properties),
                            index.DeclaringEntityType.DisplayName(),
                            Property.Format(duplicateIndex.Properties),
                            duplicateIndex.DeclaringEntityType.DisplayName(),
                            tableName,
                            indexName));
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ValidateInheritanceMapping([NotNull] IModel model)
        {
            foreach (var rootEntityType in model.GetRootEntityTypes())
            {
                ValidateDiscriminatorValues(rootEntityType);
            }
        }

        private void ValidateDiscriminator(IEntityType entityType)
        {
            var annotations = RelationalExtensions.For(entityType);
            if (annotations.DiscriminatorProperty == null)
            {
                ShowError(RelationalStrings.NoDiscriminatorProperty(entityType.DisplayName()));
            }
            if (annotations.DiscriminatorValue == null)
            {
                ShowError(RelationalStrings.NoDiscriminatorValue(entityType.DisplayName()));
            }
        }

        private void ValidateDiscriminatorValues(IEntityType rootEntityType)
        {
            var discriminatorValues = new Dictionary<object, IEntityType>();
            var derivedTypes = rootEntityType.GetDerivedTypesInclusive().ToList();
            if (derivedTypes.Count == 1)
            {
                return;
            }

            foreach (var derivedType in derivedTypes)
            {
                if (derivedType.ClrType?.IsInstantiable() != true)
                {
                    continue;
                }

                ValidateDiscriminator(derivedType);

                var discriminatorValue = RelationalExtensions.For(derivedType).DiscriminatorValue;
                IEntityType duplicateEntityType;
                if (discriminatorValues.TryGetValue(discriminatorValue, out duplicateEntityType))
                {
                    ShowError(RelationalStrings.DuplicateDiscriminatorValue(
                        derivedType.DisplayName(), discriminatorValue, duplicateEntityType.DisplayName()));
                }
                discriminatorValues[discriminatorValue] = derivedType;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ShowWarning(RelationalEventId eventId, [NotNull] string message)
            => Logger.LogWarning(eventId, () => message);

        private static string Format(IEnumerable<string> columnNames)
            => "{" + string.Join(", ", columnNames.Select(c => "'" + c + "'")) + "}";

        private static string Format(string schema, string name)
            => (string.IsNullOrEmpty(schema) ? "" : schema + ".") + name;
    }
}
