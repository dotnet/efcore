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
        [Obsolete("Use IRelationalTypeMappingSource.")]
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
#pragma warning disable 618
            ValidateDataTypes(model);
#pragma warning restore 618
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
                var methodInfo = dbFunction.MethodInfo;

                if (string.IsNullOrEmpty(dbFunction.FunctionName))
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DbFunctionNameEmpty(methodInfo.DisplayName()));
                }

                if (dbFunction.Translation == null)
                {
                    if (RelationalDependencies.TypeMappingSource.FindMapping(methodInfo.ReturnType) == null)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.DbFunctionInvalidReturnType(
                                methodInfo.DisplayName(),
                                methodInfo.ReturnType.ShortDisplayName()));
                    }

                    foreach (var parameter in methodInfo.GetParameters())
                    {
                        if (RelationalDependencies.TypeMappingSource.FindMapping(parameter.ParameterType) == null)
                        {
                            throw new InvalidOperationException(
                                RelationalStrings.DbFunctionInvalidParameterType(
                                    parameter.Name,
                                    methodInfo.DisplayName(),
                                    parameter.ParameterType.ShortDisplayName()));
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
                    && property.ValueGenerated != ValueGenerated.Never
                    && (IsNotNullAndFalse(property.Relational().DefaultValue)
                        || property.Relational().DefaultValueSql != null))
                {
                    Dependencies.Logger.BoolWithDefaultWarning(property);
                }
            }
        }

        private static bool IsNotNullAndFalse(object value)
            => value != null
               && (!(value is bool asBool) || asBool);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Obsolete("Now happens as a part of FindTypeMapping.")]
        protected virtual void ValidateDataTypes([NotNull] IModel model)
        {
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
            foreach (var entityType in model.GetEntityTypes().Where(et => !et.IsQueryType))
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

            var unvalidatedTypes = new HashSet<IEntityType>(mappedTypes);
            IEntityType root = null;
            foreach (var mappedType in mappedTypes)
            {
                if (mappedType.BaseType != null
                    || mappedType.FindForeignKeys(mappedType.FindPrimaryKey().Properties)
                        .Any(
                            fk => fk.PrincipalKey.IsPrimaryKey()
                                  && fk.PrincipalEntityType.RootType() != mappedType
                                  && unvalidatedTypes.Contains(fk.PrincipalEntityType)))
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
                var typesToValidateLeft = typesToValidate.Count;
                var directlyConnectedTypes = unvalidatedTypes.Where(
                    unvalidatedType =>
                        entityType.IsAssignableFrom(unvalidatedType)
                        || IsIdentifyingPrincipal(unvalidatedType, entityType));
                foreach (var nextEntityType in directlyConnectedTypes)
                {
                    var key = entityType.FindPrimaryKey();
                    var otherKey = nextEntityType.FindPrimaryKey();
                    if (key.Relational().Name != otherKey.Relational().Name)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.IncompatibleTableKeyNameMismatch(
                                tableName,
                                entityType.DisplayName(),
                                nextEntityType.DisplayName(),
                                key.Relational().Name,
                                Property.Format(key.Properties),
                                otherKey.Relational().Name,
                                Property.Format(otherKey.Properties)));
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
                throw new InvalidOperationException(
                    RelationalStrings.IncompatibleTableNoRelationship(
                        tableName,
                        invalidEntityType.DisplayName(),
                        root.DisplayName()));
            }
        }

        private static bool IsIdentifyingPrincipal(IEntityType dependentEntityType, IEntityType principalEntityType)
        {
            return dependentEntityType.FindForeignKeys(dependentEntityType.FindPrimaryKey().Properties)
                .Any(
                    fk => fk.PrincipalKey.IsPrimaryKey()
                          && fk.PrincipalEntityType == principalEntityType);
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
                                            ?? property.FindRelationalMapping()?.StoreType;
                    var previousTypeString = previousAnnotations.ColumnType
                                             ?? duplicateProperty.FindRelationalMapping()?.StoreType;
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
                var foreignKeyName = foreignKey.Relational().Name;
                if (!foreignKeyMappings.TryGetValue(foreignKeyName, out var duplicateForeignKey))
                {
                    foreignKeyMappings[foreignKeyName] = foreignKey;
                    continue;
                }

                foreignKey.AreCompatible(duplicateForeignKey, shouldThrow: true);
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

                index.AreCompatible(duplicateIndex, shouldThrow: true);
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

            foreach (var entityType in model.GetEntityTypes())
            {
                if (entityType.BaseType != null
                    && entityType.IsQueryType
                    && entityType[RelationalAnnotationNames.TableName] != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DerivedQueryTypeView(entityType.DisplayName(), entityType.BaseType.DisplayName()));
                }
            }
        }

        private static void ValidateDiscriminator(IEntityType entityType)
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

        private static void ValidateDiscriminatorValues(IEntityType rootEntityType)
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
