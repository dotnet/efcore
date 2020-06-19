// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         The validator that enforces rules common for all relational providers.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class RelationalModelValidator : ModelValidator
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RelationalModelValidator" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this service. </param>
        public RelationalModelValidator(
            [NotNull] ModelValidatorDependencies dependencies,
            [NotNull] RelationalModelValidatorDependencies relationalDependencies)
            : base(dependencies)
        {
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            RelationalDependencies = relationalDependencies;
        }

        /// <summary>
        ///     Dependencies used to create a <see cref="ModelValidator" />
        /// </summary>
        protected virtual RelationalModelValidatorDependencies RelationalDependencies { get; }

        /// <summary>
        ///     Validates a model, throwing an exception if any errors are found.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        public override void Validate(IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            base.Validate(model, logger);

            ValidateSharedTableCompatibility(model, logger);
            ValidatePropertyOverrides(model, logger);
            ValidateDefaultValuesOnKeys(model, logger);
            ValidateBoolsWithDefaults(model, logger);
            ValidateDbFunctions(model, logger);
            ValidateIndexProperties(model, logger);
        }

        /// <summary>
        ///     Validates the mapping/configuration of functions in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateDbFunctions(
            [NotNull] IModel model, [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var dbFunction in model.GetDbFunctions())
            {
                var methodInfo = dbFunction.MethodInfo;

                if (dbFunction.TypeMapping == null
                    && dbFunction.ReturnEntityType == null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DbFunctionInvalidReturnType(
                            methodInfo.DisplayName(),
                            methodInfo.ReturnType.ShortDisplayName()));
                }

                foreach (var parameter in dbFunction.Parameters)
                {
                    if (parameter.TypeMapping == null)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.DbFunctionInvalidParameterType(
                                parameter.Name,
                                methodInfo.DisplayName(),
                                parameter.ClrType.ShortDisplayName()));
                    }
                }
            }
        }

        /// <summary>
        ///     Validates the mapping/configuration of <see cref="bool" /> properties in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateBoolsWithDefaults(
            [NotNull] IModel model, [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            Check.NotNull(model, nameof(model));

            foreach (var entityType in model.GetEntityTypes())
            {
                foreach (var property in entityType.GetDeclaredProperties())
                {
                    if (property.ClrType != typeof(bool)
                        || property.ValueGenerated == ValueGenerated.Never)
                    {
                        continue;
                    }

                    var table = property.DeclaringEntityType.GetTableName();
                    var schema = property.DeclaringEntityType.GetSchema();
                    if (IsNotNullAndFalse(property.GetDefaultValue(table, schema))
                        || property.GetDefaultValueSql(table, schema) != null)
                    {
                        logger.BoolWithDefaultWarning(property);
                    }
                }
            }

            static bool IsNotNullAndFalse(object value)
                => value != null
                    && (!(value is bool asBool) || asBool);
        }

        /// <summary>
        ///     Validates the mapping/configuration of default values in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateDefaultValuesOnKeys(
            [NotNull] IModel model, [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var entityType in ((IConventionModel)model).GetEntityTypes())
            {
                foreach (var key in entityType.GetDeclaredKeys())
                {
                    foreach (var property in key.Properties)
                    {
                        var defaultValue = property.FindAnnotation(RelationalAnnotationNames.DefaultValue);
                        if (defaultValue?.Value != null
                            && defaultValue.GetConfigurationSource().Overrides(ConfigurationSource.DataAnnotation))
                        {
                            logger.ModelValidationKeyDefaultValueWarning(property);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Validates the mapping/configuration of shared tables in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateSharedTableCompatibility(
            [NotNull] IModel model,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            var tables = new Dictionary<(string Name, string Schema), List<IEntityType>>();
            foreach (var entityType in model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName == null)
                {
                    continue;
                }

                var name = (tableName, entityType.GetSchema());
                if (!tables.TryGetValue(name, out var mappedTypes))
                {
                    mappedTypes = new List<IEntityType>();
                    tables[name] = mappedTypes;
                }

                mappedTypes.Add(entityType);
            }

            foreach (var tableMapping in tables)
            {
                var mappedTypes = tableMapping.Value;
                var table = tableMapping.Key;
                ValidateSharedTableCompatibility(mappedTypes, table.Name, table.Schema, logger);
                ValidateSharedColumnsCompatibility(mappedTypes, table.Name, table.Schema, logger);
                ValidateSharedKeysCompatibility(mappedTypes, table.Name, table.Schema, logger);
                ValidateSharedForeignKeysCompatibility(mappedTypes, table.Name, table.Schema, logger);
                ValidateSharedIndexesCompatibility(mappedTypes, table.Name, table.Schema, logger);
            }
        }

        /// <summary>
        ///     Validates the compatibility of entity types sharing a given table.
        /// </summary>
        /// <param name="mappedTypes"> The mapped entity types. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateSharedTableCompatibility(
            [NotNull] IReadOnlyList<IEntityType> mappedTypes,
            [NotNull] string tableName,
            [CanBeNull] string schema,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            if (mappedTypes.Count == 1)
            {
                return;
            }

            var unvalidatedTypes = new HashSet<IEntityType>(mappedTypes);
            IEntityType root = null;
            foreach (var mappedType in mappedTypes)
            {
                if (mappedType.BaseType != null && unvalidatedTypes.Contains(mappedType.BaseType))
                {
                    continue;
                }

                if (mappedType.FindPrimaryKey() != null
                        && mappedType.FindForeignKeys(mappedType.FindPrimaryKey().Properties)
                            .Any(fk => fk.PrincipalKey.IsPrimaryKey()
                                    && unvalidatedTypes.Contains(fk.PrincipalEntityType)))
                {
                    if (mappedType.BaseType != null)
                    {
                        var principalType = mappedType.FindForeignKeys(mappedType.FindPrimaryKey().Properties)
                            .First(fk => fk.PrincipalKey.IsPrimaryKey()
                                    && unvalidatedTypes.Contains(fk.PrincipalEntityType))
                            .PrincipalEntityType;
                        throw new InvalidOperationException(
                            RelationalStrings.IncompatibleTableDerivedRelationship(
                                Format(tableName, schema),
                                mappedType.DisplayName(),
                                principalType.DisplayName()));
                    }

                    continue;
                }

                if (root != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.IncompatibleTableNoRelationship(
                            Format(tableName, schema),
                            mappedType.DisplayName(),
                            root.DisplayName()));
                }

                root = mappedType;
            }

            Check.DebugAssert(root != null, "root != null");
            unvalidatedTypes.Remove(root);
            var typesToValidate = new Queue<IEntityType>();
            typesToValidate.Enqueue(root);

            while (typesToValidate.Count > 0)
            {
                var entityType = typesToValidate.Dequeue();
                var comment = entityType.GetComment();
                var typesToValidateLeft = typesToValidate.Count;
                var directlyConnectedTypes = unvalidatedTypes.Where(
                    unvalidatedType =>
                        entityType.IsAssignableFrom(unvalidatedType)
                        || IsIdentifyingPrincipal(unvalidatedType, entityType));

                foreach (var nextEntityType in directlyConnectedTypes)
                {
                    var key = entityType.FindPrimaryKey();
                    var otherKey = nextEntityType.FindPrimaryKey();
                    if (key?.GetName(tableName, schema) != otherKey?.GetName(tableName, schema))
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.IncompatibleTableKeyNameMismatch(
                                Format(tableName, schema),
                                entityType.DisplayName(),
                                nextEntityType.DisplayName(),
                                key?.GetName(tableName, schema),
                                key?.Properties.Format(),
                                otherKey?.GetName(tableName, schema),
                                otherKey?.Properties.Format()));
                    }

                    var nextComment = nextEntityType.GetComment();
                    if (comment != null)
                    {
                        if (nextComment != null
                            && !comment.Equals(nextComment, StringComparison.Ordinal))
                        {
                            throw new InvalidOperationException(
                                RelationalStrings.IncompatibleTableCommentMismatch(
                                    Format(tableName, schema),
                                    entityType.DisplayName(),
                                    nextEntityType.DisplayName(),
                                    comment,
                                    nextComment));
                        }
                    }
                    else
                    {
                        comment = nextComment;
                    }

                    typesToValidate.Enqueue(nextEntityType);
                }

                foreach (var typeToValidate in typesToValidate.Skip(typesToValidateLeft))
                {
                    unvalidatedTypes.Remove(typeToValidate);
                }
            }

            if (unvalidatedTypes.Count == 0)
            {
                return;
            }

            foreach (var invalidEntityType in unvalidatedTypes)
            {
                Check.DebugAssert(root != null, "root is null");
                throw new InvalidOperationException(
                    RelationalStrings.IncompatibleTableNoRelationship(
                        tableName,
                        invalidEntityType.DisplayName(),
                        root.DisplayName()));
            }
        }

        private static bool IsIdentifyingPrincipal(IEntityType dependentEntityType, IEntityType principalEntityType)
            => dependentEntityType.FindForeignKeys(dependentEntityType.FindPrimaryKey().Properties)
                .Any(fk => fk.PrincipalKey.IsPrimaryKey()
                        && fk.PrincipalEntityType == principalEntityType);

        /// <summary>
        ///     Validates the compatibility of properties sharing columns in a given table.
        /// </summary>
        /// <param name="mappedTypes"> The mapped entity types. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateSharedColumnsCompatibility(
            [NotNull] IReadOnlyList<IEntityType> mappedTypes,
            [NotNull] string tableName,
            [CanBeNull] string schema,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            Dictionary<string, IProperty> storeConcurrencyTokens = null;
            HashSet<string> missingConcurrencyTokens = null;
            if (mappedTypes.Count > 1)
            {
                foreach (var property in mappedTypes.SelectMany(et => et.GetDeclaredProperties()))
                {
                    if (property.IsConcurrencyToken
                        && (property.ValueGenerated & ValueGenerated.OnUpdate) != 0)
                    {
                        if (storeConcurrencyTokens == null)
                        {
                            storeConcurrencyTokens = new Dictionary<string, IProperty>();
                        }

                        var columnName = property.GetColumnName(tableName, schema);
                        if (columnName == null)
                        {
                            continue;
                        }

                        storeConcurrencyTokens[columnName] = property;
                        if (missingConcurrencyTokens == null)
                        {
                            missingConcurrencyTokens = new HashSet<string>();
                        }
                    }
                }
            }

            var propertyMappings = new Dictionary<string, IProperty>();
            foreach (var entityType in mappedTypes)
            {
                if (missingConcurrencyTokens != null)
                {
                    missingConcurrencyTokens.Clear();
                    foreach (var tokenPair in storeConcurrencyTokens)
                    {
                        var declaringType = tokenPair.Value.DeclaringEntityType;
                        if (!declaringType.IsAssignableFrom(entityType)
                            && !declaringType.IsInOwnershipPath(entityType)
                            && !entityType.IsInOwnershipPath(declaringType))
                        {
                            missingConcurrencyTokens.Add(tokenPair.Key);
                        }
                    }
                }

                foreach (var property in entityType.GetDeclaredProperties())
                {
                    var columnName = property.GetColumnName(tableName, schema);
                    missingConcurrencyTokens?.Remove(columnName);
                    if (!propertyMappings.TryGetValue(columnName, out var duplicateProperty))
                    {
                        propertyMappings[columnName] = property;
                        continue;
                    }

                    ValidateCompatible(property, duplicateProperty, columnName, tableName, schema, logger);
                }

                if (missingConcurrencyTokens != null)
                {
                    foreach (var missingColumn in missingConcurrencyTokens)
                    {
                        if (entityType.GetAllBaseTypes().SelectMany(t => t.GetDeclaredProperties())
                            .All(p => p.GetColumnName(tableName, schema) != missingColumn))
                        {
                            throw new InvalidOperationException(
                                RelationalStrings.MissingConcurrencyColumn(entityType.DisplayName(), missingColumn, Format(tableName, schema)));
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Validates the compatibility of two properties mapped to the same column.
        /// </summary>
        /// <param name="property"> A property. </param>
        /// <param name="duplicateProperty"> Another property. </param>
        /// <param name="columnName"> The column name. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateCompatible(
            [NotNull] IProperty property,
            [NotNull] IProperty duplicateProperty,
            [NotNull] string columnName,
            [NotNull] string tableName,
            [CanBeNull] string schema,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            if (property.IsNullable != duplicateProperty.IsNullable)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameNullabilityMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        Format(tableName, schema)));
            }

            var currentMaxLength = property.GetMaxLength();
            var previousMaxLength = duplicateProperty.GetMaxLength();
            if (currentMaxLength != previousMaxLength)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameMaxLengthMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        Format(tableName, schema),
                        previousMaxLength,
                        currentMaxLength));
            }

            if (property.IsUnicode() != duplicateProperty.IsUnicode())
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameUnicodenessMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        Format(tableName, schema)));
            }

            if (property.IsFixedLength(tableName, schema) != duplicateProperty.IsFixedLength(tableName, schema))
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameFixedLengthMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        Format(tableName, schema)));
            }

            if (property.IsConcurrencyToken != duplicateProperty.IsConcurrencyToken)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameConcurrencyTokenMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        Format(tableName, schema)));
            }

            var currentTypeString = property.GetColumnType(tableName, schema)
                ?? property.GetRelationalTypeMapping().StoreType;
            var previousTypeString = duplicateProperty.GetColumnType(tableName, schema)
                ?? duplicateProperty.GetRelationalTypeMapping().StoreType;
            if (!string.Equals(currentTypeString, previousTypeString, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        Format(tableName, schema),
                        previousTypeString,
                        currentTypeString));
            }

            var currentComputedColumnSql = property.GetComputedColumnSql(tableName, schema) ?? "";
            var previousComputedColumnSql = duplicateProperty.GetComputedColumnSql(tableName, schema) ?? "";
            if (!currentComputedColumnSql.Equals(previousComputedColumnSql, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameComputedSqlMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        Format(tableName, schema),
                        previousComputedColumnSql,
                        currentComputedColumnSql));
            }

            var currentStored = property.GetIsStored(tableName, schema);
            var previousStored = duplicateProperty.GetIsStored(tableName, schema);
            if (currentStored != previousStored)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameIsStoredMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        Format(tableName, schema),
                        previousStored,
                        currentStored));
            }

            var currentDefaultValue = property.GetDefaultValue(tableName, schema);
            var previousDefaultValue = duplicateProperty.GetDefaultValue(tableName, schema);
            if (!Equals(currentDefaultValue, previousDefaultValue))
            {
                currentDefaultValue = GetDefaultColumnValue(property, tableName, schema);
                previousDefaultValue = GetDefaultColumnValue(duplicateProperty, tableName, schema);

                if (!Equals(currentDefaultValue, previousDefaultValue))
                {
                    throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameDefaultSqlMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        Format(tableName, schema),
                        previousDefaultValue ?? "NULL",
                        currentDefaultValue ?? "NULL"));
                }
            }

            var currentDefaultValueSql = property.GetDefaultValueSql(tableName, schema) ?? "";
            var previousDefaultValueSql = duplicateProperty.GetDefaultValueSql(tableName, schema) ?? "";
            if (!currentDefaultValueSql.Equals(previousDefaultValueSql, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameDefaultSqlMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        Format(tableName, schema),
                        previousDefaultValueSql,
                        currentDefaultValueSql));
            }

            var currentComment = property.GetComment(tableName, schema) ?? "";
            var previousComment = duplicateProperty.GetComment(tableName, schema) ?? "";
            if (!currentComment.Equals(previousComment, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameCommentMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        Format(tableName, schema),
                        previousComment,
                        currentComment));
            }

            var currentCollation = property.GetCollation(tableName, schema) ?? "";
            var previousCollation = duplicateProperty.GetCollation(tableName, schema) ?? "";
            if (!currentCollation.Equals(previousCollation, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameCollationMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        Format(tableName, schema),
                        previousCollation,
                        currentCollation));
            }
        }

        /// <summary>
        ///     Returns the object that is used as the default value for the column the property is mapped to.
        /// </summary>
        /// <param name="property"> The property to get the default value for. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> The object that is used as the default value for the column the property is mapped to. </returns>
        protected virtual object GetDefaultColumnValue(
            [NotNull] IProperty property,
            [NotNull] string tableName,
            [CanBeNull] string schema)
        {
            var value = property.GetDefaultValue(tableName, schema);
            var converter = property.GetValueConverter() ?? property.FindRelationalTypeMapping(tableName, schema)?.Converter;

            return converter != null
                ? converter.ConvertToProvider(value)
                : value;
        }

        /// <summary>
        ///     Validates the compatibility of foreign keys in a given shared table.
        /// </summary>
        /// <param name="mappedTypes"> The mapped entity types. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateSharedForeignKeysCompatibility(
            [NotNull] IReadOnlyList<IEntityType> mappedTypes,
            [NotNull] string tableName,
            [CanBeNull] string schema,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            var foreignKeyMappings = new Dictionary<string, IForeignKey>();

            foreach (var foreignKey in mappedTypes.SelectMany(et => et.GetDeclaredForeignKeys()))
            {
                var foreignKeyName = foreignKey.GetConstraintName(
                    tableName, schema, foreignKey.PrincipalEntityType.GetTableName(), foreignKey.PrincipalEntityType.GetSchema());
                if (!foreignKeyMappings.TryGetValue(foreignKeyName, out var duplicateForeignKey))
                {
                    foreignKeyMappings[foreignKeyName] = foreignKey;
                    continue;
                }

                ValidateCompatible(foreignKey, duplicateForeignKey, foreignKeyName, tableName, schema, logger);
            }
        }

        /// <summary>
        ///     Validates the compatibility of two foreign keys mapped to the same foreign key constraint.
        /// </summary>
        /// <param name="foreignKey"> A foreign key. </param>
        /// <param name="duplicateForeignKey"> Another foreign key. </param>
        /// <param name="foreignKeyName"> The foreign key constraint name. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateCompatible(
            [NotNull] IForeignKey foreignKey,
            [NotNull] IForeignKey duplicateForeignKey,
            [NotNull] string foreignKeyName,
            [NotNull] string tableName,
            [CanBeNull] string schema,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
            => foreignKey.AreCompatible(duplicateForeignKey, tableName, schema, shouldThrow: true);

        /// <summary>
        ///     Validates the compatibility of indexes in a given shared table.
        /// </summary>
        /// <param name="mappedTypes"> The mapped entity types. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateSharedIndexesCompatibility(
            [NotNull] IReadOnlyList<IEntityType> mappedTypes,
            [NotNull] string tableName,
            [CanBeNull] string schema,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            var indexMappings = new Dictionary<string, IIndex>();

            foreach (var index in mappedTypes.SelectMany(et => et.GetDeclaredIndexes()))
            {
                var indexName = index.GetDatabaseName(tableName, schema);
                if (!indexMappings.TryGetValue(indexName, out var duplicateIndex))
                {
                    indexMappings[indexName] = index;
                    continue;
                }

                ValidateCompatible(index, duplicateIndex, indexName, tableName, schema, logger);
            }
        }

        /// <summary>
        ///     Validates the compatibility of two indexes mapped to the same table index.
        /// </summary>
        /// <param name="index"> An index. </param>
        /// <param name="duplicateIndex"> Another index. </param>
        /// <param name="indexName"> The name of the index. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateCompatible(
            [NotNull] IIndex index,
            [NotNull] IIndex duplicateIndex,
            [NotNull] string indexName,
            [NotNull] string tableName,
            [CanBeNull] string schema,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
            => index.AreCompatible(duplicateIndex, tableName, schema, shouldThrow: true);

        /// <summary>
        ///     Validates the compatibility of primary and alternate keys in a given shared table.
        /// </summary>
        /// <param name="mappedTypes"> The mapped entity types. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateSharedKeysCompatibility(
            [NotNull] IReadOnlyList<IEntityType> mappedTypes,
            [NotNull] string tableName,
            [CanBeNull] string schema,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            var keyMappings = new Dictionary<string, IKey>();

            foreach (var key in mappedTypes.SelectMany(et => et.GetDeclaredKeys()))
            {
                var keyName = key.GetName(tableName, schema);

                if (!keyMappings.TryGetValue(keyName, out var duplicateKey))
                {
                    keyMappings[keyName] = key;
                    continue;
                }

                ValidateCompatible(key, duplicateKey, keyName, tableName, schema, logger);
            }
        }

        /// <summary>
        ///     Validates the compatibility of two keys mapped to the same unique constraint.
        /// </summary>
        /// <param name="key"> A key. </param>
        /// <param name="duplicateKey"> Another key. </param>
        /// <param name="keyName"> The name of the unique constraint. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateCompatible(
            [NotNull] IKey key,
            [NotNull] IKey duplicateKey,
            [NotNull] string keyName,
            [NotNull] string tableName,
            [CanBeNull] string schema,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            key.AreCompatible(duplicateKey, tableName, schema, shouldThrow: true);
        }

        /// <summary>
        ///     Validates the mapping/configuration of inheritance in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected override void ValidateInheritanceMapping(
            IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var rootEntityType in model.GetEntityTypes())
            {
                if (rootEntityType.BaseType != null)
                {
                    continue;
                }

                // Hierarchy mapping strategy must be the same across all types of mappings
                var isTPH = rootEntityType.FindPrimaryKey() == null
                    || rootEntityType.GetDiscriminatorProperty() != null;
                if (isTPH)
                {
                    ValidateTPHMapping(rootEntityType, forTables: false);
                    ValidateTPHMapping(rootEntityType, forTables: true);
                    ValidateDiscriminatorValues(rootEntityType);
                }
                else
                {
                    ValidateTPTMapping(rootEntityType, forTables: false);
                    ValidateTPTMapping(rootEntityType, forTables: true);
                }
            }
        }

        private void ValidateTPTMapping(IEntityType rootEntityType, bool forTables)
        {
            var derivedTypes = new Dictionary<(string, string), IEntityType>();
            foreach (var entityType in rootEntityType.GetDerivedTypesInclusive())
            {
                var name = forTables ? entityType.GetTableName() : entityType.GetViewName();
                if (name == null)
                {
                    continue;
                }

                var schema = forTables ? entityType.GetSchema() : entityType.GetViewSchema();
                if (derivedTypes.TryGetValue((name, schema), out var otherType))
                {
                    throw new InvalidOperationException(forTables
                        ? RelationalStrings.NonTPHTableClash(
                            entityType.DisplayName(), otherType.DisplayName(), entityType.GetSchemaQualifiedTableName())
                        : RelationalStrings.NonTPHViewClash(
                             entityType.DisplayName(), otherType.DisplayName(), entityType.GetSchemaQualifiedViewName()));
                }

                derivedTypes[(name, schema)] = entityType;
            }
        }

        private void ValidateTPHMapping(IEntityType rootEntityType, bool forTables)
        {
            string firstName = null;
            string firstSchema = null;
            IEntityType firstType = null;
            foreach (var entityType in rootEntityType.GetDerivedTypesInclusive())
            {
                var name = forTables ? entityType.GetTableName() : entityType.GetViewName();
                if (name == null)
                {
                    continue;
                }

                if (firstType == null)
                {
                    firstType = entityType;
                    firstName = forTables ? firstType.GetTableName() : firstType.GetViewName();
                    firstSchema = forTables ? firstType.GetSchema() : firstType.GetViewSchema();
                    continue;
                }

                var schema = forTables ? entityType.GetSchema() : entityType.GetViewSchema();
                if (name != firstName || schema != firstSchema)
                {
                    throw new InvalidOperationException(forTables
                        ? RelationalStrings.TPHTableMismatch(
                            entityType.DisplayName(), entityType.GetSchemaQualifiedTableName(),
                            firstType.DisplayName(), firstType.GetSchemaQualifiedTableName())
                        : RelationalStrings.TPHViewMismatch(
                            entityType.DisplayName(), entityType.GetSchemaQualifiedViewName(),
                            firstType.DisplayName(), firstType.GetSchemaQualifiedViewName()));
                }
            }
        }

        /// <summary>
        ///     Validates the table-specific property overrides.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidatePropertyOverrides(
            [NotNull] IModel model, [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var entityType in model.GetEntityTypes())
            {
                foreach (var property in entityType.GetDeclaredProperties())
                {
                    var tableOverrides = (SortedDictionary<(string, string), RelationalPropertyOverrides>)
                        property[RelationalAnnotationNames.RelationalOverrides];
                    if (tableOverrides == null)
                    {
                        continue;
                    }

                    foreach (var overrideTable in tableOverrides.Keys)
                    {
                        var (name, schema) = overrideTable;
                        if ((entityType.GetTableName() == name
                            && entityType.GetSchema() == schema)
                            || (entityType.GetViewName() == name
                                && entityType.GetViewSchema() == schema))
                        {
                            throw new InvalidOperationException(RelationalStrings.TableOverrideDeclaredTable(
                                property.Name,
                                (schema == null ? "" : schema + ".") + name,
                                entityType.DisplayName()));
                        }

                        if (!entityType.GetDerivedTypes().Any(d =>
                            (d.GetTableName() == name
                                && d.GetSchema() == schema)
                            || (d.GetViewName() == name
                                && d.GetViewSchema() == schema)))
                        {
                            throw new InvalidOperationException(RelationalStrings.TableOverrideMismatch(
                                entityType.DisplayName() + "." + property.Name,
                                (schema == null ? "" : schema + ".") + name));
                        }
                    }
                }
            }
        }

        private static string Format(string tableName, string schema)
            => schema == null ? tableName : schema + "." + tableName;

        /// <summary>
        ///     Validates that the properties of any one index are
        ///     all mapped to columns on at least one common table.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateIndexProperties(
            [NotNull] IModel model, [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            Check.NotNull(model, nameof(model));

            foreach (var entityType in model.GetEntityTypes())
            {
                foreach (var index in entityType.GetDeclaredIndexes()
                    .Where(i => ConfigurationSource.Convention != ((IConventionIndex)i).GetConfigurationSource()))
                {
                    IProperty propertyNotMappedToAnyTable = null;
                    Tuple<string, List<(string Table, string Schema)>> firstPropertyTables = null;
                    Tuple<string, List<(string Table, string Schema)>> lastPropertyTables = null;
                    HashSet<(string Table, string Schema)> overlappingTables = null;
                    foreach (var property in index.Properties)
                    {
                        var tablesMappedToProperty = property.DeclaringEntityType.GetDerivedTypesInclusive()
                            .Select(t => (t.GetTableName(), t.GetSchema())).Distinct()
                            .Where(n => n.Item1 != null && property.GetColumnName(n.Item1, n.Item2) != null)
                            .ToList<(string Table, string Schema)>();
                        if (tablesMappedToProperty.Count == 0)
                        {
                            propertyNotMappedToAnyTable = property;
                            overlappingTables = null;

                            if (firstPropertyTables != null)
                            {
                                // Property is not mapped but we already found
                                // a property that is mapped.
                                break;
                            }

                            continue;
                        }

                        if (firstPropertyTables == null)
                        {
                            // store off which tables the first member maps to
                            firstPropertyTables =
                                new Tuple<string, List<(string Table, string Schema)>>(property.Name, tablesMappedToProperty);
                        }
                        else
                        {
                            // store off which tables the last member we encountered maps to
                            lastPropertyTables =
                                new Tuple<string, List<(string Table, string Schema)>>(property.Name, tablesMappedToProperty);
                        }

                        if (propertyNotMappedToAnyTable != null)
                        {
                            // Property is mapped but we already found
                            // a property that is not mapped.
                            overlappingTables = null;
                            break;
                        }

                        if (overlappingTables == null)
                        {
                            overlappingTables = new HashSet<(string Table, string Schema)>(tablesMappedToProperty);
                        }
                        else
                        {
                            overlappingTables.IntersectWith(tablesMappedToProperty);
                            if (overlappingTables.Count == 0)
                            {
                                break;
                            }
                        }
                    }

                    if (overlappingTables == null)
                    {
                        if (firstPropertyTables == null)
                        {
                            logger.AllIndexPropertiesNotToMappedToAnyTable(
                                entityType,
                                index);
                        }
                        else
                        {
                            logger.IndexPropertiesBothMappedAndNotMappedToTable(
                                entityType,
                                index,
                                propertyNotMappedToAnyTable.Name);
                        }
                    }
                    else if (overlappingTables.Count == 0)
                    {
                        Debug.Assert(firstPropertyTables != null, nameof(firstPropertyTables));
                        Debug.Assert(lastPropertyTables != null, nameof(lastPropertyTables));

                        logger.IndexPropertiesMappedToNonOverlappingTables(
                            entityType,
                            index,
                            firstPropertyTables.Item1,
                            firstPropertyTables.Item2,
                            lastPropertyTables.Item1,
                            lastPropertyTables.Item2);
                    }
                }
            }
        }
    }
}
