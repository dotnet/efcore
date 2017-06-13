// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     The validator that enforces rules common for all relational providers.
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
        ///     Gets the type mapper.
        /// </summary>
        protected virtual IRelationalTypeMapper TypeMapper => RelationalDependencies.TypeMapper;

        /// <summary>
        ///     Validates a model, throwing an exception if any errors are found.
        /// </summary>
        /// <param name="model"> The model to validate. </param>
        public override void Validate(IModel model)
        {
            base.Validate(model);

            ValidateSharedTableCompatibility(model);
            ValidateInheritanceMapping(model);
            ValidateDataTypes(model);
            ValidateDefaultValuesOnKeys(model);
            ValidateBoolsWithDefaults(model);
            ValidateDbFunctions(model);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ValidateDbFunctions([NotNull] IModel model)
        {
            foreach (var dbFunction in model.Relational().DbFunctions)
            {
                var dbFuncName = $"{dbFunction.MethodInfo.DeclaringType?.Name}.{dbFunction.MethodInfo.Name}";

                if (string.IsNullOrEmpty(dbFunction.Name))
                    throw new InvalidOperationException(CoreStrings.DbFunctionNameEmpty(dbFuncName));

                var paramIndexes = dbFunction.Parameters.Select(fp => fp.Index).ToArray();
                if (paramIndexes.Distinct().Count() != dbFunction.Parameters.Count)
                    throw new InvalidOperationException(CoreStrings.DbFunctionParametersDuplicateIndex(dbFuncName));

                if (Enumerable.Range(0, paramIndexes.Length).Except(paramIndexes).Any())
                    throw new InvalidOperationException(CoreStrings.DbFunctionNonContinuousIndex(dbFuncName));

                if (dbFunction.MethodInfo.IsStatic == false
                    && dbFunction.MethodInfo.DeclaringType.GetTypeInfo().IsSubclassOf(typeof(DbContext)))
                {
                    throw new InvalidOperationException(CoreStrings.DbFunctionDbContextMethodMustBeStatic(dbFuncName));
                }

                if (dbFunction.TranslateCallback == null)
                {
                    if (dbFunction.ReturnType == null
                        || RelationalDependencies.TypeMapper.IsTypeMapped(dbFunction.ReturnType) == false)
                        throw new InvalidOperationException(CoreStrings.DbFunctionInvalidReturnType(dbFunction.MethodInfo, dbFunction.ReturnType));

                    foreach (var parameter in dbFunction.Parameters)
                    {
                        if (parameter.ParameterType == null
                            || RelationalDependencies.TypeMapper.IsTypeMapped(parameter.ParameterType) == false)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.DbFunctionInvalidParameterType(
                                    dbFunction.MethodInfo,
                                    parameter.Name,
                                    parameter.ParameterType?.ShortDisplayName()));
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ValidateBoolsWithDefaults([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            foreach (var property in model.GetEntityTypes().SelectMany(e => e.GetDeclaredProperties()))
            {
                if (property.ClrType == typeof(bool)
                    && (property.Relational().DefaultValue != null
                        || property.Relational().DefaultValueSql != null))
                {
                    Dependencies.Logger.BoolWithDefaultWarning(property);
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ValidateDataTypes([NotNull] IModel model)
        {
            foreach (var entityType in model.GetEntityTypes())
            {
                foreach (var property in entityType.GetDeclaredProperties())
                {
                    var dataType = property.Relational().ColumnType;
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
        protected virtual void ValidateDefaultValuesOnKeys([NotNull] IModel model)
        {
            foreach (var property in model.GetEntityTypes().SelectMany(
                t => t.GetDeclaredKeys().SelectMany(k => k.Properties))
                .Where(p => p.Relational().DefaultValue != null))
            {
                Dependencies.Logger.ModelValidationKeyDefaultValueWarning(property);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ValidateSharedTableCompatibility([NotNull] IModel model)
        {
            var tables = new Dictionary<string, List<IEntityType>>();
            foreach (var entityType in model.GetEntityTypes())
            {
                var annotations = entityType.Relational();
                var tableName = Format(annotations.Schema, annotations.TableName);

                if (!tables.TryGetValue(tableName, out var mappedTypes))
                {
                    mappedTypes = new List<IEntityType>();
                    tables[tableName] = mappedTypes;
                }

                mappedTypes.Add(entityType);
            }

            foreach (var tableMapping in tables)
            {
                var mappedTypes = tableMapping.Value;
                var tableName = tableMapping.Key;
                ValidateSharedTableCompatibility(mappedTypes, tableName);
                ValidateSharedColumnsCompatibility(mappedTypes, tableName);
                ValidateSharedKeysCompatibility(mappedTypes, tableName);
                ValidateSharedForeignKeysCompatibility(mappedTypes, tableName);
                ValidateSharedIndexesCompatibility(mappedTypes, tableName);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ValidateSharedTableCompatibility(
            [NotNull] IReadOnlyList<IEntityType> mappedTypes, [NotNull] string tableName)
        {
            if (mappedTypes.Count == 1)
            {
                return;
            }

            var firstValidatedType = mappedTypes[0];
            var validatedTypes = new List<IEntityType> { firstValidatedType };
            var unvalidatedTypes = new Queue<IEntityType>(mappedTypes.Skip(1));
            while (unvalidatedTypes.Count > 0)
            {
                var entityType = unvalidatedTypes.Dequeue();
                var key = entityType.FindPrimaryKey();
                var otherKey = firstValidatedType.FindPrimaryKey();
                if (key.Relational().Name != otherKey.Relational().Name)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.IncompatibleTableKeyNameMismatch(
                            tableName,
                            entityType.DisplayName(),
                            firstValidatedType.DisplayName(),
                            key.Relational().Name,
                            Property.Format(key.Properties),
                            otherKey.Relational().Name,
                            Property.Format(otherKey.Properties)));
                }

                var relationshipFound = validatedTypes.Any(validatedType =>
                    entityType.RootType() == validatedType.RootType()
                    || IsIdentifyingPrincipal(entityType, validatedType)
                    || IsIdentifyingPrincipal(validatedType, entityType));
                if (!relationshipFound)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.IncompatibleTableNoRelationship(
                            tableName,
                            entityType.DisplayName(),
                            firstValidatedType.DisplayName(),
                            Property.Format(key.Properties),
                            Property.Format(otherKey.Properties)));
                }

                validatedTypes.Add(entityType);
            }
        }

        private static bool IsIdentifyingPrincipal(IEntityType dependEntityType, IEntityType principalEntityType)
        {
            var identifyingForeignKeys = new Queue<IForeignKey>(
                dependEntityType.FindForeignKeys(dependEntityType.FindPrimaryKey().Properties));
            while (identifyingForeignKeys.Count > 0)
            {
                var fk = identifyingForeignKeys.Dequeue();
                if (fk.PrincipalKey.IsPrimaryKey())
                {
                    if (fk.PrincipalEntityType == principalEntityType)
                    {
                        return true;
                    }

                    foreach (var principalFk in fk.PrincipalEntityType.FindForeignKeys(fk.PrincipalEntityType.FindPrimaryKey().Properties))
                    {
                        identifyingForeignKeys.Enqueue(principalFk);
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ValidateSharedColumnsCompatibility(
            [NotNull] IReadOnlyList<IEntityType> mappedTypes, [NotNull] string tableName)
        {
            var propertyMappings = new Dictionary<string, IProperty>();

            foreach (var property in mappedTypes.SelectMany(et => et.GetDeclaredProperties()))
            {
                var propertyAnnotations = property.Relational();
                var columnName = propertyAnnotations.ColumnName;
                if (propertyMappings.TryGetValue(columnName, out var duplicateProperty))
                {
                    var previousAnnotations = duplicateProperty.Relational();
                    var currentTypeString = propertyAnnotations.ColumnType
                                            ?? TypeMapper.GetMapping(property).StoreType;
                    var previousTypeString = previousAnnotations.ColumnType
                                             ?? TypeMapper.GetMapping(duplicateProperty).StoreType;
                    if (!currentTypeString.Equals(previousTypeString, StringComparison.OrdinalIgnoreCase))
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

                    if (property.IsColumnNullable() != duplicateProperty.IsColumnNullable())
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

                    var currentComputedColumnSql = propertyAnnotations.ComputedColumnSql ?? "";
                    var previousComputedColumnSql = previousAnnotations.ComputedColumnSql ?? "";
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

                    var currentDefaultValue = propertyAnnotations.DefaultValue;
                    var previousDefaultValue = previousAnnotations.DefaultValue;
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

                    var currentDefaultValueSql = propertyAnnotations.DefaultValueSql ?? "";
                    var previousDefaultValueSql = previousAnnotations.DefaultValueSql ?? "";
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
                }
                else
                {
                    propertyMappings[columnName] = property;
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ValidateSharedForeignKeysCompatibility(
            [NotNull] IReadOnlyList<IEntityType> mappedTypes, [NotNull] string tableName)
        {
            var foreignKeyMappings = new Dictionary<string, IForeignKey>();

            foreach (var foreignKey in mappedTypes.SelectMany(et => et.GetDeclaredForeignKeys()))
            {
                var foreignKeyAnnotations = foreignKey.Relational();
                var foreignKeyName = foreignKeyAnnotations.Name;

                if (!foreignKeyMappings.TryGetValue(foreignKeyName, out var duplicateForeignKey))
                {
                    foreignKeyMappings[foreignKeyName] = foreignKey;
                    continue;
                }

                var principalAnnotations = foreignKey.PrincipalEntityType.Relational();
                var principalTable = Format(principalAnnotations.Schema, principalAnnotations.TableName);
                var duplicateAnnotations = duplicateForeignKey.PrincipalEntityType.Relational();
                var duplicatePrincipalTable = Format(duplicateAnnotations.Schema, duplicateAnnotations.TableName);
                if (!string.Equals(principalTable, duplicatePrincipalTable, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateForeignKeyPrincipalTableMismatch(
                            Property.Format(foreignKey.Properties),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            Property.Format(duplicateForeignKey.Properties),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            tableName,
                            foreignKeyName,
                            principalTable,
                            duplicatePrincipalTable));
                }

                if (!foreignKey.Properties.Select(p => p.Relational().ColumnName)
                    .SequenceEqual(duplicateForeignKey.Properties.Select(p => p.Relational().ColumnName)))
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateForeignKeyColumnMismatch(
                            Property.Format(foreignKey.Properties),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            Property.Format(duplicateForeignKey.Properties),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            tableName,
                            foreignKeyName,
                            foreignKey.Properties.FormatColumns(),
                            duplicateForeignKey.Properties.FormatColumns()));
                }

                if (!foreignKey.PrincipalKey.Properties
                    .Select(p => p.Relational().ColumnName)
                    .SequenceEqual(
                        duplicateForeignKey.PrincipalKey.Properties
                            .Select(p => p.Relational().ColumnName)))
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateForeignKeyPrincipalColumnMismatch(
                            Property.Format(foreignKey.Properties),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            Property.Format(duplicateForeignKey.Properties),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            tableName,
                            foreignKeyName,
                            foreignKey.PrincipalKey.Properties.FormatColumns(),
                            duplicateForeignKey.PrincipalKey.Properties.FormatColumns()));
                }

                if (foreignKey.IsUnique != duplicateForeignKey.IsUnique)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateForeignKeyUniquenessMismatch(
                            Property.Format(foreignKey.Properties),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            Property.Format(duplicateForeignKey.Properties),
                            duplicateForeignKey.DeclaringEntityType.DisplayName(),
                            tableName,
                            foreignKeyName));
                }

                if (foreignKey.DeleteBehavior != duplicateForeignKey.DeleteBehavior)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateForeignKeyDeleteBehaviorMismatch(
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ValidateSharedIndexesCompatibility(
            [NotNull] IReadOnlyList<IEntityType> mappedTypes, [NotNull] string tableName)
        {
            var indexMappings = new Dictionary<string, IIndex>();

            foreach (var index in mappedTypes.SelectMany(et => et.GetDeclaredIndexes()))
            {
                var indexName = index.Relational().Name;

                if (!indexMappings.TryGetValue(indexName, out var duplicateIndex))
                {
                    indexMappings[indexName] = index;
                    continue;
                }

                if (!index.Properties.Select(p => p.Relational().ColumnName)
                    .SequenceEqual(duplicateIndex.Properties.Select(p => p.Relational().ColumnName)))
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateIndexColumnMismatch(
                            Property.Format(index.Properties),
                            index.DeclaringEntityType.DisplayName(),
                            Property.Format(duplicateIndex.Properties),
                            duplicateIndex.DeclaringEntityType.DisplayName(),
                            tableName,
                            indexName,
                            index.Properties.FormatColumns(),
                            duplicateIndex.Properties.FormatColumns()));
                }

                if (index.IsUnique != duplicateIndex.IsUnique)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateIndexUniquenessMismatch(
                            Property.Format(index.Properties),
                            index.DeclaringEntityType.DisplayName(),
                            Property.Format(duplicateIndex.Properties),
                            duplicateIndex.DeclaringEntityType.DisplayName(),
                            tableName,
                            indexName));
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ValidateSharedKeysCompatibility(
            [NotNull] IReadOnlyList<IEntityType> mappedTypes, [NotNull] string tableName)
        {
            var keyMappings = new Dictionary<string, IKey>();

            foreach (var key in mappedTypes.SelectMany(et => et.GetDeclaredKeys()))
            {
                var keyName = key.Relational().Name;

                if (!keyMappings.TryGetValue(keyName, out var duplicateKey))
                {
                    keyMappings[keyName] = key;
                    continue;
                }

                if (!key.Properties.Select(p => p.Relational().ColumnName)
                    .SequenceEqual(duplicateKey.Properties.Select(p => p.Relational().ColumnName)))
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateKeyColumnMismatch(
                            Property.Format(key.Properties),
                            key.DeclaringEntityType.DisplayName(),
                            Property.Format(duplicateKey.Properties),
                            duplicateKey.DeclaringEntityType.DisplayName(),
                            tableName,
                            keyName,
                            key.Properties.FormatColumns(),
                            duplicateKey.Properties.FormatColumns()));
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
            var annotations = entityType.Relational();
            if (annotations.DiscriminatorProperty == null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.NoDiscriminatorProperty(entityType.DisplayName()));
            }
            if (annotations.DiscriminatorValue == null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.NoDiscriminatorValue(entityType.DisplayName()));
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

                var discriminatorValue = derivedType.Relational().DiscriminatorValue;
                if (discriminatorValues.TryGetValue(discriminatorValue, out var duplicateEntityType))
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateDiscriminatorValue(
                            derivedType.DisplayName(), discriminatorValue, duplicateEntityType.DisplayName()));
                }
                discriminatorValues[discriminatorValue] = derivedType;
            }
        }

        private static string Format(string schema, string name)
            => (string.IsNullOrEmpty(schema) ? "" : schema + ".") + name;
    }
}
