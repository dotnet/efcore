// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
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

            ValidatePropertyOverrides(model, logger);
            ValidateSqlQueries(model, logger);
            ValidateDbFunctions(model, logger);
            ValidateSharedTableCompatibility(model, logger);
            ValidateSharedViewCompatibility(model, logger);
            ValidateDefaultValuesOnKeys(model, logger);
            ValidateBoolsWithDefaults(model, logger);
            ValidateIndexProperties(model, logger);
        }

        /// <summary>
        ///     Validates the mapping/configuration of SQL queries in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateSqlQueries(
            [NotNull] IModel model,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var entityType in model.GetEntityTypes())
            {
                var sqlQuery = entityType.GetSqlQuery();
                if (sqlQuery == null)
                {
                    continue;
                }

                if (entityType.BaseType != null
                    && (entityType.GetDiscriminatorProperty() == null
                        || sqlQuery != entityType.BaseType.GetSqlQuery()))
                {
                    throw new InvalidOperationException(
                        RelationalStrings.InvalidMappedSqlQueryDerivedType(
                            entityType.DisplayName(), entityType.BaseType.DisplayName()));
                }
            }
        }

        /// <summary>
        ///     Validates the mapping/configuration of functions in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateDbFunctions(
            [NotNull] IModel model,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var dbFunction in model.GetDbFunctions())
            {
                if (dbFunction.IsScalar)
                {
                    if (dbFunction.TypeMapping == null)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.DbFunctionInvalidReturnType(
                                dbFunction.ModelName,
                                dbFunction.ReturnType.ShortDisplayName()));
                    }
                }
                else
                {
                    var elementType = dbFunction.ReturnType.GetGenericArguments()[0];
                    var entityType = model.FindEntityType(elementType);

                    if (entityType?.IsOwned() == true
                        || ((IConventionModel)model).IsOwned(elementType)
                        || (entityType == null && model.GetEntityTypes().Any(e => e.ClrType == elementType)))
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.DbFunctionInvalidIQueryableOwnedReturnType(
                                dbFunction.ModelName, elementType.ShortDisplayName()));
                    }

                    if (entityType == null)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.DbFunctionInvalidReturnEntityType(
                                dbFunction.ModelName, dbFunction.ReturnType.ShortDisplayName(), elementType.ShortDisplayName()));
                    }

                    if ((entityType.BaseType != null || entityType.GetDerivedTypes().Any())
                        && entityType.GetDiscriminatorProperty() == null)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.TableValuedFunctionNonTPH(dbFunction.ModelName, entityType.DisplayName()));
                    }
                }

                foreach (var parameter in dbFunction.Parameters)
                {
                    if (parameter.TypeMapping == null)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.DbFunctionInvalidParameterType(
                                parameter.Name,
                                dbFunction.ModelName,
                                parameter.ClrType.ShortDisplayName()));
                    }
                }
            }

            foreach (var entityType in model.GetEntityTypes())
            {
                var mappedFunctionName = entityType.GetFunctionName();
                if (mappedFunctionName == null)
                {
                    continue;
                }

                var mappedFunction = model.FindDbFunction(mappedFunctionName);
                if (mappedFunction == null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.MappedFunctionNotFound(entityType.DisplayName(), mappedFunctionName));
                }

                if (entityType.BaseType != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.InvalidMappedFunctionDerivedType(
                            entityType.DisplayName(), mappedFunctionName, entityType.BaseType.DisplayName()));
                }

                if (mappedFunction.IsScalar
                    || mappedFunction.ReturnType.GetGenericArguments()[0] != entityType.ClrType)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.InvalidMappedFunctionUnmatchedReturn(
                            entityType.DisplayName(),
                            mappedFunctionName,
                            mappedFunction.ReturnType.ShortDisplayName(),
                            entityType.ClrType.ShortDisplayName()));
                }

                if (mappedFunction.Parameters.Count > 0)
                {
                    var parameters = "{"
                        + string.Join(
                            ", ",
                            mappedFunction.Parameters.Select(p => "'" + p.Name + "'"))
                        + "}";
                    throw new InvalidOperationException(
                        RelationalStrings.InvalidMappedFunctionWithParameters(
                            entityType.DisplayName(), mappedFunctionName, parameters));
                }
            }
        }

        /// <summary>
        ///     Validates the mapping/configuration of <see cref="bool" /> properties in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateBoolsWithDefaults(
            [NotNull] IModel model,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
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

                    var table = StoreObjectIdentifier.Table(
                        property.DeclaringEntityType.GetTableName(), property.DeclaringEntityType.GetSchema());
                    if (IsNotNullAndFalse(property.GetDefaultValue(table))
                        || property.GetDefaultValueSql(table) != null)
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
            [NotNull] IModel model,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
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
            var tables = new Dictionary<StoreObjectIdentifier, List<IEntityType>>();
            foreach (var entityType in model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName == null)
                {
                    continue;
                }

                var table = StoreObjectIdentifier.Table(tableName, entityType.GetSchema());
                if (!tables.TryGetValue(table, out var mappedTypes))
                {
                    mappedTypes = new List<IEntityType>();
                    tables[table] = mappedTypes;
                }

                mappedTypes.Add(entityType);
            }

            foreach (var tableMapping in tables)
            {
                var mappedTypes = tableMapping.Value;
                var table = tableMapping.Key;
                ValidateSharedTableCompatibility(mappedTypes, table.Name, table.Schema, logger);
                ValidateSharedColumnsCompatibility(mappedTypes, table, logger);
                ValidateSharedKeysCompatibility(mappedTypes, table, logger);
                ValidateSharedForeignKeysCompatibility(mappedTypes, table, logger);
                ValidateSharedIndexesCompatibility(mappedTypes, table, logger);
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

            var storeObject = StoreObjectIdentifier.Table(tableName, schema);
            var unvalidatedTypes = new HashSet<IEntityType>(mappedTypes);
            IEntityType root = null;
            foreach (var mappedType in mappedTypes)
            {
                if (mappedType.BaseType != null && unvalidatedTypes.Contains(mappedType.BaseType))
                {
                    continue;
                }

                var primaryKey = mappedType.FindPrimaryKey();
                if (primaryKey != null
                    && (mappedType.FindForeignKeys(primaryKey.Properties)
                        .FirstOrDefault(fk => fk.PrincipalKey.IsPrimaryKey()
                                && unvalidatedTypes.Contains(fk.PrincipalEntityType)) is IForeignKey linkingFK))
                {
                    if (mappedType.BaseType != null)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.IncompatibleTableDerivedRelationship(
                                storeObject.DisplayName(),
                                mappedType.DisplayName(),
                                linkingFK.PrincipalEntityType.DisplayName()));
                    }

                    continue;
                }

                if (root != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.IncompatibleTableNoRelationship(
                            storeObject.DisplayName(),
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
                var key = entityType.FindPrimaryKey();
                var comment = entityType.GetComment();
                var isExcluded = entityType.IsTableExcludedFromMigrations();
                var typesToValidateLeft = typesToValidate.Count;
                var directlyConnectedTypes = unvalidatedTypes.Where(
                    unvalidatedType =>
                        entityType.IsAssignableFrom(unvalidatedType)
                        || IsIdentifyingPrincipal(unvalidatedType, entityType));

                foreach (var nextEntityType in directlyConnectedTypes)
                {
                    if (key != null)
                    {
                        var otherKey = nextEntityType.FindPrimaryKey();
                        if (key.GetName(storeObject) != otherKey.GetName(storeObject))
                        {
                            throw new InvalidOperationException(
                                RelationalStrings.IncompatibleTableKeyNameMismatch(
                                    storeObject.DisplayName(),
                                    entityType.DisplayName(),
                                    nextEntityType.DisplayName(),
                                    key.GetName(storeObject),
                                    key.Properties.Format(),
                                    otherKey.GetName(storeObject),
                                    otherKey.Properties.Format()));
                        }
                    }

                    var nextComment = nextEntityType.GetComment();
                    if (comment != null)
                    {
                        if (nextComment != null
                            && !comment.Equals(nextComment, StringComparison.Ordinal))
                        {
                            throw new InvalidOperationException(
                                RelationalStrings.IncompatibleTableCommentMismatch(
                                    storeObject.DisplayName(),
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

                    if (isExcluded.Equals(!nextEntityType.IsTableExcludedFromMigrations()))
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.IncompatibleTableExcludedMismatch(
                                storeObject.DisplayName(),
                                entityType.DisplayName(),
                                nextEntityType.DisplayName()));
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

        /// <summary>
        ///     Validates the mapping/configuration of shared views in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateSharedViewCompatibility(
            [NotNull] IModel model,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            var views = new Dictionary<StoreObjectIdentifier, List<IEntityType>>();
            foreach (var entityType in model.GetEntityTypes())
            {
                var viewsName = entityType.GetViewName();
                if (viewsName == null)
                {
                    continue;
                }

                var view = StoreObjectIdentifier.View(viewsName, entityType.GetViewSchema());
                if (!views.TryGetValue(view, out var mappedTypes))
                {
                    mappedTypes = new List<IEntityType>();
                    views[view] = mappedTypes;
                }

                mappedTypes.Add(entityType);
            }

            foreach (var tableMapping in views)
            {
                var mappedTypes = tableMapping.Value;
                var table = tableMapping.Key;
                ValidateSharedViewCompatibility(mappedTypes, table.Name, table.Schema, logger);
                ValidateSharedColumnsCompatibility(mappedTypes, table, logger);
            }
        }

        /// <summary>
        ///     Validates the compatibility of entity types sharing a given view.
        /// </summary>
        /// <param name="mappedTypes"> The mapped entity types. </param>
        /// <param name="viewName"> The view name. </param>
        /// <param name="schema"> The schema. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateSharedViewCompatibility(
            [NotNull] IReadOnlyList<IEntityType> mappedTypes,
            [NotNull] string viewName,
            [CanBeNull] string schema,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            if (mappedTypes.Count == 1)
            {
                return;
            }

            var storeObject = StoreObjectIdentifier.View(viewName, schema);
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
                        .Any(
                            fk => fk.PrincipalKey.IsPrimaryKey()
                                && unvalidatedTypes.Contains(fk.PrincipalEntityType)))
                {
                    if (mappedType.BaseType != null)
                    {
                        var principalType = mappedType.FindForeignKeys(mappedType.FindPrimaryKey().Properties)
                            .First(
                                fk => fk.PrincipalKey.IsPrimaryKey()
                                    && unvalidatedTypes.Contains(fk.PrincipalEntityType))
                            .PrincipalEntityType;
                        throw new InvalidOperationException(
                            RelationalStrings.IncompatibleViewDerivedRelationship(
                                storeObject.DisplayName(),
                                mappedType.DisplayName(),
                                principalType.DisplayName()));
                    }

                    continue;
                }

                if (root != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.IncompatibleViewNoRelationship(
                            storeObject.DisplayName(),
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
                var typesToValidateLeft = typesToValidate.Count;
                var directlyConnectedTypes = unvalidatedTypes.Where(
                    unvalidatedType =>
                        entityType.IsAssignableFrom(unvalidatedType)
                        || IsIdentifyingPrincipal(unvalidatedType, entityType));

                foreach (var nextEntityType in directlyConnectedTypes)
                {
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
                    RelationalStrings.IncompatibleViewNoRelationship(
                        viewName,
                        invalidEntityType.DisplayName(),
                        root.DisplayName()));
            }
        }

        private static bool IsIdentifyingPrincipal(IEntityType dependentEntityType, IEntityType principalEntityType)
            => dependentEntityType.FindForeignKeys(dependentEntityType.FindPrimaryKey().Properties)
                .Any(fk => fk.PrincipalKey.IsPrimaryKey()
                        && fk.PrincipalEntityType == principalEntityType);

        /// <summary>
        ///     Validates the compatibility of properties sharing columns in a given table-like object.
        /// </summary>
        /// <param name="mappedTypes"> The mapped entity types. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateSharedColumnsCompatibility(
            [NotNull] IReadOnlyList<IEntityType> mappedTypes,
            in StoreObjectIdentifier storeObject,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            var concurrencyColumns = TableSharingConcurrencyTokenConvention.GetConcurrencyTokensMap(storeObject, mappedTypes);
            HashSet<string> missingConcurrencyTokens = null;
            if (concurrencyColumns != null
                && storeObject.StoreObjectType == StoreObjectType.Table)
            {
                missingConcurrencyTokens = new HashSet<string>();
            }

            var propertyMappings = new Dictionary<string, IProperty>();
            foreach (var entityType in mappedTypes)
            {
                if (missingConcurrencyTokens != null)
                {
                    missingConcurrencyTokens.Clear();
                    foreach (var tokenPair in concurrencyColumns)
                    {
                        if (TableSharingConcurrencyTokenConvention.IsConcurrencyTokenMissing(tokenPair.Value, entityType, mappedTypes))
                        {
                            missingConcurrencyTokens.Add(tokenPair.Key);
                        }
                    }
                }

                foreach (var property in entityType.GetDeclaredProperties())
                {
                    var columnName = property.GetColumnName(storeObject);
                    missingConcurrencyTokens?.Remove(columnName);
                    if (!propertyMappings.TryGetValue(columnName, out var duplicateProperty))
                    {
                        propertyMappings[columnName] = property;
                        continue;
                    }

                    ValidateCompatible(property, duplicateProperty, columnName, storeObject, logger);
                }

                if (missingConcurrencyTokens != null)
                {
                    foreach (var missingColumn in missingConcurrencyTokens)
                    {
                        var columnFound = false;
                        foreach (var property in entityType.GetAllBaseTypesAscending().SelectMany(t => t.GetDeclaredProperties()))
                        {
                            if (property.GetColumnName(storeObject) == missingColumn)
                            {
                                columnFound = true;
                                break;
                            }
                        }

                        if (!columnFound)
                        {
                            throw new InvalidOperationException(
                                RelationalStrings.MissingConcurrencyColumn(
                                    entityType.DisplayName(), missingColumn, storeObject.DisplayName()));
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
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateCompatible(
            [NotNull] IProperty property,
            [NotNull] IProperty duplicateProperty,
            [NotNull] string columnName,
            in StoreObjectIdentifier storeObject,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            if (property.IsColumnNullable(storeObject) != duplicateProperty.IsColumnNullable(storeObject))
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameNullabilityMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        storeObject.DisplayName()));
            }

            var currentMaxLength = property.GetMaxLength(storeObject);
            var previousMaxLength = duplicateProperty.GetMaxLength(storeObject);
            if (currentMaxLength != previousMaxLength)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameMaxLengthMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        storeObject.DisplayName(),
                        previousMaxLength,
                        currentMaxLength));
            }

            if (property.IsUnicode(storeObject) != duplicateProperty.IsUnicode(storeObject))
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameUnicodenessMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        storeObject.DisplayName()));
            }

            if (property.IsFixedLength(storeObject) != duplicateProperty.IsFixedLength(storeObject))
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameFixedLengthMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        storeObject.DisplayName()));
            }

            var currentPrecision = property.GetPrecision(storeObject);
            var previousPrecision = duplicateProperty.GetPrecision(storeObject);
            if (currentPrecision != previousPrecision)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNamePrecisionMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        storeObject.DisplayName(),
                        currentPrecision,
                        previousPrecision));
            }

            var currentScale = property.GetScale(storeObject);
            var previousScale = duplicateProperty.GetScale(storeObject);
            if (currentScale != previousScale)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameScaleMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        storeObject.DisplayName(),
                        currentScale,
                        previousScale));
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
                        storeObject.DisplayName()));
            }

            var currentTypeString = property.GetColumnType(storeObject)
                ?? property.GetRelationalTypeMapping().StoreType;
            var previousTypeString = duplicateProperty.GetColumnType(storeObject)
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
                        storeObject.DisplayName(),
                        previousTypeString,
                        currentTypeString));
            }

            var currentComputedColumnSql = property.GetComputedColumnSql(storeObject) ?? "";
            var previousComputedColumnSql = duplicateProperty.GetComputedColumnSql(storeObject) ?? "";
            if (!currentComputedColumnSql.Equals(previousComputedColumnSql, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameComputedSqlMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        storeObject.DisplayName(),
                        previousComputedColumnSql,
                        currentComputedColumnSql));
            }

            var currentStored = property.GetIsStored(storeObject);
            var previousStored = duplicateProperty.GetIsStored(storeObject);
            if (currentStored != previousStored)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameIsStoredMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        storeObject.DisplayName(),
                        previousStored,
                        currentStored));
            }

            var currentDefaultValue = property.GetDefaultValue(storeObject);
            var previousDefaultValue = duplicateProperty.GetDefaultValue(storeObject);
            if (!Equals(currentDefaultValue, previousDefaultValue))
            {
                currentDefaultValue = GetDefaultColumnValue(property, storeObject);
                previousDefaultValue = GetDefaultColumnValue(duplicateProperty, storeObject);

                if (!Equals(currentDefaultValue, previousDefaultValue))
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateColumnNameDefaultSqlMismatch(
                            duplicateProperty.DeclaringEntityType.DisplayName(),
                            duplicateProperty.Name,
                            property.DeclaringEntityType.DisplayName(),
                            property.Name,
                            columnName,
                            storeObject.DisplayName(),
                            previousDefaultValue ?? "NULL",
                            currentDefaultValue ?? "NULL"));
                }
            }

            var currentDefaultValueSql = property.GetDefaultValueSql(storeObject) ?? "";
            var previousDefaultValueSql = duplicateProperty.GetDefaultValueSql(storeObject) ?? "";
            if (!currentDefaultValueSql.Equals(previousDefaultValueSql, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameDefaultSqlMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        storeObject.DisplayName(),
                        previousDefaultValueSql,
                        currentDefaultValueSql));
            }

            var currentComment = property.GetComment(storeObject) ?? "";
            var previousComment = duplicateProperty.GetComment(storeObject) ?? "";
            if (!currentComment.Equals(previousComment, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameCommentMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        storeObject.DisplayName(),
                        previousComment,
                        currentComment));
            }

            var currentCollation = property.GetCollation(storeObject) ?? "";
            var previousCollation = duplicateProperty.GetCollation(storeObject) ?? "";
            if (!currentCollation.Equals(previousCollation, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameCollationMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        storeObject.DisplayName(),
                        previousCollation,
                        currentCollation));
            }
        }

        /// <summary>
        ///     Returns the object that is used as the default value for the column the property is mapped to.
        /// </summary>
        /// <param name="property"> The property to get the default value for. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <returns> The object that is used as the default value for the column the property is mapped to. </returns>
        protected virtual object GetDefaultColumnValue(
            [NotNull] IProperty property,
            in StoreObjectIdentifier storeObject)
        {
            var value = property.GetDefaultValue(storeObject);
            var converter = property.GetValueConverter() ?? property.FindRelationalTypeMapping(storeObject)?.Converter;

            return converter != null
                ? converter.ConvertToProvider(value)
                : value;
        }

        /// <summary>
        ///     Validates the compatibility of foreign keys in a given shared table.
        /// </summary>
        /// <param name="mappedTypes"> The mapped entity types. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateSharedForeignKeysCompatibility(
            [NotNull] IReadOnlyList<IEntityType> mappedTypes,
            in StoreObjectIdentifier storeObject,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            if (storeObject.StoreObjectType != StoreObjectType.Table)
            {
                return;
            }

            var foreignKeyMappings = new Dictionary<string, IForeignKey>();

            foreach (var foreignKey in mappedTypes.SelectMany(et => et.GetDeclaredForeignKeys()))
            {
                var principalTable = foreignKey.PrincipalKey.IsPrimaryKey()
                    ? StoreObjectIdentifier.Create(foreignKey.PrincipalEntityType, StoreObjectType.Table)
                    : StoreObjectIdentifier.Create(foreignKey.PrincipalKey.DeclaringEntityType, StoreObjectType.Table);
                if (principalTable == null)
                {
                    continue;
                }

                var foreignKeyName = foreignKey.GetConstraintName(storeObject, principalTable.Value);
                if (foreignKeyName == null)
                {
                    var derivedTables = foreignKey.DeclaringEntityType.GetDerivedTypes()
                        .Select(t => StoreObjectIdentifier.Create(t, StoreObjectType.Table))
                        .Where(t => t != null);
                    if (foreignKey.GetConstraintName() != null
                        && derivedTables.All(t => foreignKey.GetConstraintName(
                            t.Value,
                            principalTable.Value) == null))
                    {
                        logger.ForeignKeyPropertiesMappedToUnrelatedTables(foreignKey);
                    }

                    continue;
                }

                if (!foreignKeyMappings.TryGetValue(foreignKeyName, out var duplicateForeignKey))
                {
                    foreignKeyMappings[foreignKeyName] = foreignKey;
                    continue;
                }

                ValidateCompatible(foreignKey, duplicateForeignKey, foreignKeyName, storeObject, logger);
            }
        }

        /// <summary>
        ///     Validates the compatibility of two foreign keys mapped to the same foreign key constraint.
        /// </summary>
        /// <param name="foreignKey"> A foreign key. </param>
        /// <param name="duplicateForeignKey"> Another foreign key. </param>
        /// <param name="foreignKeyName"> The foreign key constraint name. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateCompatible(
            [NotNull] IForeignKey foreignKey,
            [NotNull] IForeignKey duplicateForeignKey,
            [NotNull] string foreignKeyName,
            in StoreObjectIdentifier storeObject,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
            => foreignKey.AreCompatible(duplicateForeignKey, storeObject, shouldThrow: true);

        /// <summary>
        ///     Validates the compatibility of indexes in a given shared table.
        /// </summary>
        /// <param name="mappedTypes"> The mapped entity types. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateSharedIndexesCompatibility(
            [NotNull] IReadOnlyList<IEntityType> mappedTypes,
            in StoreObjectIdentifier storeObject,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            var indexMappings = new Dictionary<string, IIndex>();
            foreach (var index in mappedTypes.SelectMany(et => et.GetDeclaredIndexes()))
            {
                var indexName = index.GetDatabaseName(storeObject);
                if (indexName == null)
                {
                    continue;
                }

                if (!indexMappings.TryGetValue(indexName, out var duplicateIndex))
                {
                    indexMappings[indexName] = index;
                    continue;
                }

                ValidateCompatible(index, duplicateIndex, indexName, storeObject, logger);
            }
        }

        /// <summary>
        ///     Validates the compatibility of two indexes mapped to the same table index.
        /// </summary>
        /// <param name="index"> An index. </param>
        /// <param name="duplicateIndex"> Another index. </param>
        /// <param name="indexName"> The name of the index. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateCompatible(
            [NotNull] IIndex index,
            [NotNull] IIndex duplicateIndex,
            [NotNull] string indexName,
            in StoreObjectIdentifier storeObject,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
            => index.AreCompatible(duplicateIndex, storeObject, shouldThrow: true);

        /// <summary>
        ///     Validates the compatibility of primary and alternate keys in a given shared table.
        /// </summary>
        /// <param name="mappedTypes"> The mapped entity types. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateSharedKeysCompatibility(
            [NotNull] IReadOnlyList<IEntityType> mappedTypes,
            in StoreObjectIdentifier storeObject,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            var keyMappings = new Dictionary<string, IKey>();
            foreach (var key in mappedTypes.SelectMany(et => et.GetDeclaredKeys()))
            {
                var keyName = key.GetName(storeObject);
                if (keyName == null)
                {
                    continue;
                }

                if (!keyMappings.TryGetValue(keyName, out var duplicateKey))
                {
                    keyMappings[keyName] = key;
                    continue;
                }

                ValidateCompatible(key, duplicateKey, keyName, storeObject, logger);
            }
        }

        /// <summary>
        ///     Validates the compatibility of two keys mapped to the same unique constraint.
        /// </summary>
        /// <param name="key"> A key. </param>
        /// <param name="duplicateKey"> Another key. </param>
        /// <param name="keyName"> The name of the unique constraint. </param>
        /// <param name="storeObject"> The identifier of the store object. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateCompatible(
            [NotNull] IKey key,
            [NotNull] IKey duplicateKey,
            [NotNull] string keyName,
            in StoreObjectIdentifier storeObject,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            key.AreCompatible(duplicateKey, storeObject, shouldThrow: true);
        }

        /// <summary>
        ///     Validates the mapping/configuration of inheritance in the model.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected override void ValidateInheritanceMapping(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
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
                    throw new InvalidOperationException(
                        forTables
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
                    throw new InvalidOperationException(
                        forTables
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
            [NotNull] IModel model,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var entityType in model.GetEntityTypes())
            {
                foreach (var property in entityType.GetDeclaredProperties())
                {
                    var tableOverrides = (SortedDictionary<StoreObjectIdentifier, RelationalPropertyOverrides>)
                        property[RelationalAnnotationNames.RelationalOverrides];
                    if (tableOverrides == null)
                    {
                        continue;
                    }

                    foreach (var storeOverride in tableOverrides.Keys)
                    {
                        var name = storeOverride.Name;
                        var schema = storeOverride.Schema;
                        switch (storeOverride.StoreObjectType)
                        {
                            case StoreObjectType.Table:
                                if (!entityType.GetDerivedTypes().Any(
                                    d =>
                                        d.GetTableName() == name
                                        && d.GetSchema() == schema))
                                {
                                    throw new InvalidOperationException(
                                        RelationalStrings.TableOverrideMismatch(
                                            entityType.DisplayName() + "." + property.Name,
                                            (schema == null ? "" : schema + ".") + name));
                                }

                                break;
                            case StoreObjectType.View:
                                if (!entityType.GetDerivedTypes().Any(
                                    d =>
                                        d.GetViewName() == name
                                        && d.GetViewSchema() == schema))
                                {
                                    throw new InvalidOperationException(
                                        RelationalStrings.ViewOverrideMismatch(
                                            entityType.DisplayName() + "." + property.Name,
                                            (schema == null ? "" : schema + ".") + name));
                                }

                                break;
                            case StoreObjectType.SqlQuery:
                                if (!entityType.GetDerivedTypes().Any(d => d.GetDefaultSqlQueryName() == name))
                                {
                                    throw new InvalidOperationException(
                                        RelationalStrings.SqlQueryOverrideMismatch(
                                            entityType.DisplayName() + "." + property.Name, name));
                                }

                                break;
                            case StoreObjectType.Function:
                                if (!entityType.GetDerivedTypes().Any(d => d.GetFunctionName() == name))
                                {
                                    throw new InvalidOperationException(
                                        RelationalStrings.FunctionOverrideMismatch(
                                            entityType.DisplayName() + "." + property.Name, name));
                                }

                                break;
                            default:
                                throw new NotImplementedException(storeOverride.StoreObjectType.ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Validates that the properties of any one index are
        ///     all mapped to columns on at least one common table.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        /// <param name="logger"> The logger to use. </param>
        protected virtual void ValidateIndexProperties(
            [NotNull] IModel model,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
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
                            .Where(n => n.Item1 != null && property.GetColumnName(StoreObjectIdentifier.Table(n.Item1, n.Item2)) != null)
                            .ToList<(string Table, string Schema)>();
                        if (tablesMappedToProperty.Count == 0)
                        {
                            propertyNotMappedToAnyTable = property;
                            overlappingTables = null;

                            if (firstPropertyTables != null)
                            {
                                // Property is not mapped but we already found a property that is mapped.
                                break;
                            }

                            continue;
                        }

                        if (firstPropertyTables == null)
                        {
                            firstPropertyTables =
                                new Tuple<string, List<(string Table, string Schema)>>(property.Name, tablesMappedToProperty);
                        }
                        else
                        {
                            lastPropertyTables =
                                new Tuple<string, List<(string Table, string Schema)>>(property.Name, tablesMappedToProperty);
                        }

                        if (propertyNotMappedToAnyTable != null)
                        {
                            // Property is mapped but we already found a property that is not mapped.
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
