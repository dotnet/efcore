// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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
            ValidateInheritanceMapping(model, logger);
            ValidateDefaultValuesOnKeys(model, logger);
            ValidateBoolsWithDefaults(model, logger);
            ValidateDbFunctions(model, logger);
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
                    && dbFunction.QueryableEntityType == null)
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

            foreach (var property in model.GetEntityTypes().SelectMany(e => e.GetDeclaredProperties()))
            {
                if (property.ClrType == typeof(bool)
                    && property.ValueGenerated != ValueGenerated.Never
                    && (IsNotNullAndFalse(property.GetDefaultValue())
                        || property.GetDefaultValueSql() != null))
                {
                    logger.BoolWithDefaultWarning(property);
                }
            }
        }

        private static bool IsNotNullAndFalse(object value)
            => value != null
                && (!(value is bool asBool) || asBool);

        /// <summary>
        ///     Validates the mapping/configuration of default values in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateDefaultValuesOnKeys(
            [NotNull] IModel model, [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var property in model.GetEntityTypes().SelectMany(
                    t => t.GetDeclaredKeys().SelectMany(k => k.Properties))
                .Where(p => p.GetDefaultValue() != null))
            {
                logger.ModelValidationKeyDefaultValueWarning(property);
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
            var tables = new Dictionary<string, List<IEntityType>>();
            foreach (var entityType in model.GetEntityTypes())
            {
                var name = entityType.GetSchemaQualifiedTableName();
                if (name == null)
                {
                    continue;
                }

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
                var tableName = tableMapping.Key;
                ValidateSharedTableCompatibility(mappedTypes, tableName, logger);
                ValidateSharedColumnsCompatibility(mappedTypes, tableName, logger);
                ValidateSharedKeysCompatibility(mappedTypes, tableName, logger);
                ValidateSharedForeignKeysCompatibility(mappedTypes, tableName, logger);
                ValidateSharedIndexesCompatibility(mappedTypes, tableName, logger);
            }
        }

        /// <summary>
        ///     Validates the compatibility of entity types sharing a given table.
        /// </summary>
        /// <param name="mappedTypes"> The mapped entity types. </param>
        /// <param name="tableName"> The schema-qualified table name. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateSharedTableCompatibility(
            [NotNull] IReadOnlyList<IEntityType> mappedTypes,
            [NotNull] string tableName,
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
                if (mappedType.BaseType != null
                    || (mappedType.FindPrimaryKey() != null
                        && mappedType.FindForeignKeys(mappedType.FindPrimaryKey().Properties)
                            .Any(
                                fk => fk.PrincipalKey.IsPrimaryKey()
                                    && fk.PrincipalEntityType.GetRootType() != mappedType
                                    && unvalidatedTypes.Contains(fk.PrincipalEntityType))))
                {
                    continue;
                }

                if (root != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.IncompatibleTableNoRelationship(
                            tableName,
                            mappedType.DisplayName(),
                            root.DisplayName()));
                }

                root = mappedType;
            }

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
                    if (key?.GetName() != otherKey?.GetName())
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.IncompatibleTableKeyNameMismatch(
                                tableName,
                                entityType.DisplayName(),
                                nextEntityType.DisplayName(),
                                key?.GetName(),
                                key?.Properties.Format(),
                                otherKey?.GetName(),
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
                                    tableName,
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
        /// <param name="tableName"> The schema-qualified table name. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateSharedColumnsCompatibility(
            [NotNull] IReadOnlyList<IEntityType> mappedTypes,
            [NotNull] string tableName,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            Dictionary<string, IProperty> storeConcurrencyTokens = null;
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

                        storeConcurrencyTokens[property.GetColumnName()] = property;
                    }
                }
            }

            var propertyMappings = new Dictionary<string, IProperty>();
            foreach (var entityType in mappedTypes)
            {
                HashSet<string> missingConcurrencyTokens = null;
                if ((storeConcurrencyTokens?.Count ?? 0) != 0)
                {
                    missingConcurrencyTokens = new HashSet<string>();
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
                    var columnName = property.GetColumnName();
                    missingConcurrencyTokens?.Remove(columnName);
                    if (!propertyMappings.TryGetValue(columnName, out var duplicateProperty))
                    {
                        propertyMappings[columnName] = property;
                        continue;
                    }

                    ValidateCompatible(property, duplicateProperty, columnName, tableName);
                }

                if ((missingConcurrencyTokens?.Count ?? 0) != 0)
                {
                    foreach (var missingColumn in missingConcurrencyTokens)
                    {
                        if (entityType.GetAllBaseTypes().SelectMany(t => t.GetDeclaredProperties())
                            .All(p => p.GetColumnName() != missingColumn))
                        {
                            throw new InvalidOperationException(
                                RelationalStrings.MissingConcurrencyColumn(entityType.DisplayName(), missingColumn, tableName));
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
        /// <param name="tableName"> The schema-qualified table name. </param>
        protected virtual void ValidateCompatible(
            [NotNull] IProperty property,
            [NotNull] IProperty duplicateProperty,
            [NotNull] string columnName,
            [NotNull] string tableName)
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
                        tableName));
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
                        tableName,
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
                        tableName));
            }

            if (property.IsFixedLength() != duplicateProperty.IsFixedLength())
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameFixedLengthMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        tableName));
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
                        tableName));
            }

            var currentTypeString = property.GetColumnType()
                ?? property.GetRelationalTypeMapping().StoreType;
            var previousTypeString = duplicateProperty.GetColumnType()
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
                        tableName,
                        previousTypeString,
                        currentTypeString));
            }

            var currentComputedColumnSql = property.GetComputedColumnSql() ?? "";
            var previousComputedColumnSql = duplicateProperty.GetComputedColumnSql() ?? "";
            if (!currentComputedColumnSql.Equals(previousComputedColumnSql, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameComputedSqlMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        tableName,
                        previousComputedColumnSql,
                        currentComputedColumnSql));
            }

            var currentDefaultValue = property.GetDefaultValue();
            var previousDefaultValue = duplicateProperty.GetDefaultValue();
            if (!Equals(currentDefaultValue, previousDefaultValue))
            {
                currentDefaultValue = GetDefaultColumnValue(property);
                previousDefaultValue = GetDefaultColumnValue(duplicateProperty);

                if (!Equals(currentDefaultValue, previousDefaultValue))
                {
                    throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameDefaultSqlMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        tableName,
                        previousDefaultValue ?? "NULL",
                        currentDefaultValue ?? "NULL"));
                }
            }

            var currentDefaultValueSql = property.GetDefaultValueSql() ?? "";
            var previousDefaultValueSql = duplicateProperty.GetDefaultValueSql() ?? "";
            if (!currentDefaultValueSql.Equals(previousDefaultValueSql, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameDefaultSqlMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        tableName,
                        previousDefaultValueSql,
                        currentDefaultValueSql));
            }

            var currentComment = property.GetComment() ?? "";
            var previousComment = duplicateProperty.GetComment() ?? "";
            if (!currentComment.Equals(previousComment, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameCommentMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        tableName,
                        previousComment,
                        currentComment));
            }
        }

        /// <summary>
        ///     Returns the object that is used as the default value for the column the property is mapped to.
        /// </summary>
        /// <param name="property"> The property to get the default value for. </param>
        /// <returns> The object that is used as the default value for the column the property is mapped to. </returns>
        protected virtual object GetDefaultColumnValue([NotNull] IProperty property)
        {
            var value = property.GetDefaultValue();
            var converter = property.GetValueConverter() ?? property.FindRelationalTypeMapping()?.Converter;

            return converter != null
                ? converter.ConvertToProvider(value)
                : value;
        }

        /// <summary>
        ///     Validates the compatibility of foreign keys in a given shared table.
        /// </summary>
        /// <param name="mappedTypes"> The mapped entity types. </param>
        /// <param name="tableName"> The schema-qualified table name. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateSharedForeignKeysCompatibility(
            [NotNull] IReadOnlyList<IEntityType> mappedTypes,
            [NotNull] string tableName,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            var foreignKeyMappings = new Dictionary<string, IForeignKey>();

            foreach (var foreignKey in mappedTypes.SelectMany(et => et.GetDeclaredForeignKeys()))
            {
                var foreignKeyName = foreignKey.GetConstraintName();
                if (!foreignKeyMappings.TryGetValue(foreignKeyName, out var duplicateForeignKey))
                {
                    foreignKeyMappings[foreignKeyName] = foreignKey;
                    continue;
                }

                ValidateCompatible(foreignKey, duplicateForeignKey, foreignKeyName, tableName);
            }
        }

        /// <summary>
        ///     Validates the compatibility of two foreign keys mapped to the same foreign key constraint.
        /// </summary>
        /// <param name="foreignKey"> A foreign key. </param>
        /// <param name="duplicateForeignKey"> Another foreign key. </param>
        /// <param name="foreignKeyName"> The foreign key constraint name. </param>
        /// <param name="tableName"> The schema-qualified table name. </param>
        protected virtual void ValidateCompatible(
            [NotNull] IForeignKey foreignKey,
            [NotNull] IForeignKey duplicateForeignKey,
            [NotNull] string foreignKeyName,
            [NotNull] string tableName)
        {
            foreignKey.AreCompatible(duplicateForeignKey, shouldThrow: true);
        }

        /// <summary>
        ///     Validates the compatibility of indexes in a given shared table.
        /// </summary>
        /// <param name="mappedTypes"> The mapped entity types. </param>
        /// <param name="tableName"> The schema-qualified table name. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateSharedIndexesCompatibility(
            [NotNull] IReadOnlyList<IEntityType> mappedTypes,
            [NotNull] string tableName,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            var indexMappings = new Dictionary<string, IIndex>();

            foreach (var index in mappedTypes.SelectMany(et => et.GetDeclaredIndexes()))
            {
                var indexName = index.GetName();
                if (!indexMappings.TryGetValue(indexName, out var duplicateIndex))
                {
                    indexMappings[indexName] = index;
                    continue;
                }

                ValidateCompatible(index, duplicateIndex, indexName, tableName);
            }
        }

        /// <summary>
        ///     Validates the compatibility of two indexes mapped to the same table index.
        /// </summary>
        /// <param name="index"> An index. </param>
        /// <param name="duplicateIndex"> Another index. </param>
        /// <param name="indexName"> The name of the index. </param>
        /// <param name="tableName"> The schema-qualified table name. </param>
        protected virtual void ValidateCompatible(
            [NotNull] IIndex index,
            [NotNull] IIndex duplicateIndex,
            [NotNull] string indexName,
            [NotNull] string tableName)
            => index.AreCompatible(duplicateIndex, shouldThrow: true);

        /// <summary>
        ///     Validates the compatibility of primary and alternate keys in a given shared table.
        /// </summary>
        /// <param name="mappedTypes"> The mapped entity types. </param>
        /// <param name="tableName"> The schema-qualified table name. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateSharedKeysCompatibility(
            [NotNull] IReadOnlyList<IEntityType> mappedTypes,
            [NotNull] string tableName,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            var keyMappings = new Dictionary<string, IKey>();

            foreach (var key in mappedTypes.SelectMany(et => et.GetDeclaredKeys()))
            {
                var keyName = key.GetName();

                if (!keyMappings.TryGetValue(keyName, out var duplicateKey))
                {
                    keyMappings[keyName] = key;
                    continue;
                }

                ValidateCompatible(key, duplicateKey, keyName, tableName);
            }
        }

        /// <summary>
        ///     Validates the compatibility of two keys mapped to the same unique constraint.
        /// </summary>
        /// <param name="key"> A key. </param>
        /// <param name="duplicateKey"> Another key. </param>
        /// <param name="keyName"> The name of the unique constraint. </param>
        /// <param name="tableName"> The schema-qualified table name. </param>
        protected virtual void ValidateCompatible(
            [NotNull] IKey key,
            [NotNull] IKey duplicateKey,
            [NotNull] string keyName,
            [NotNull] string tableName)
        {
            if (!key.Properties.Select(p => p.GetColumnName())
                    .SequenceEqual(duplicateKey.Properties.Select(p => p.GetColumnName())))
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateKeyColumnMismatch(
                        key.Properties.Format(),
                        key.DeclaringEntityType.DisplayName(),
                        duplicateKey.Properties.Format(),
                        duplicateKey.DeclaringEntityType.DisplayName(),
                        tableName,
                        keyName,
                        key.Properties.FormatColumns(),
                        duplicateKey.Properties.FormatColumns()));
            }
        }

        /// <summary>
        ///     Validates the mapping/configuration of inheritance in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateInheritanceMapping(
            [NotNull] IModel model, [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var entityType in model.GetEntityTypes())
            {
                if (entityType.BaseType != null
                    && ((IConventionEntityType)entityType).FindAnnotation(RelationalAnnotationNames.TableName)?.GetConfigurationSource()
                        == ConfigurationSource.Explicit)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DerivedTypeTable(entityType.DisplayName(), entityType.BaseType.DisplayName()));
                }
            }
        }
    }
}
