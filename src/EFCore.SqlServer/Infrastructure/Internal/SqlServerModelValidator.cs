// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Extensions.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class SqlServerModelValidator : RelationalModelValidator
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerModelValidator(
            ModelValidatorDependencies dependencies,
            RelationalModelValidatorDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void Validate(IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            ValidateIndexIncludeProperties(model, logger);

            base.Validate(model, logger);

            ValidateDecimalColumns(model, logger);
            ValidateByteIdentityMapping(model, logger);
            ValidateNonKeyValueGeneration(model, logger);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void ValidateDecimalColumns(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (IConventionProperty property in model.GetEntityTypes()
                .SelectMany(t => t.GetDeclaredProperties())
                .Where(p => p.ClrType.UnwrapNullableType() == typeof(decimal)
                        && !p.IsForeignKey()))
            {
                var valueConverterConfigurationSource = property.GetValueConverterConfigurationSource();
                var valueConverterProviderType = property.GetValueConverter()?.ProviderClrType;
                if (!ConfigurationSource.Convention.Overrides(valueConverterConfigurationSource)
                    && typeof(decimal) != valueConverterProviderType)
                {
                    continue;
                }

                var columnTypeConfigurationSource = property.GetColumnTypeConfigurationSource();
                if (((columnTypeConfigurationSource == null
                            && ConfigurationSource.Convention.Overrides(property.GetTypeMappingConfigurationSource()))
                        || (columnTypeConfigurationSource != null
                            && ConfigurationSource.Convention.Overrides(columnTypeConfigurationSource)))
                    && (ConfigurationSource.Convention.Overrides(property.GetPrecisionConfigurationSource())
                        || ConfigurationSource.Convention.Overrides(property.GetScaleConfigurationSource())))
                {
                    logger.DecimalTypeDefaultWarning((IProperty)property);
                }

                if (property.IsKey())
                {
                    logger.DecimalTypeKeyWarning((IProperty)property);
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void ValidateByteIdentityMapping(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var entityType in model.GetEntityTypes())
            {
                // TODO: Validate this per table
                foreach (var property in entityType.GetDeclaredProperties()
                    .Where(
                        p => p.ClrType.UnwrapNullableType() == typeof(byte)
                            && p.GetValueGenerationStrategy() == SqlServerValueGenerationStrategy.IdentityColumn))
                {
                    logger.ByteIdentityColumnWarning(property);
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void ValidateNonKeyValueGeneration(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var entityType in model.GetEntityTypes())
            {
                foreach (var property in entityType.GetDeclaredProperties()
                    .Where(
                        p => p.GetValueGenerationStrategy() == SqlServerValueGenerationStrategy.SequenceHiLo
                            && ((IConventionProperty)p).GetValueGenerationStrategyConfigurationSource() != null
                            && !p.IsKey()
                            && p.ValueGenerated != ValueGenerated.Never
                            && (!(p.FindAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy) is IConventionAnnotation strategy)
                                || !ConfigurationSource.Convention.Overrides(strategy.GetConfigurationSource()))))
                {
                    throw new InvalidOperationException(
                        SqlServerStrings.NonKeyValueGeneration(property.Name, property.DeclaringEntityType.DisplayName()));
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void ValidateIndexIncludeProperties(
            IModel model,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var index in model.GetEntityTypes().SelectMany(t => t.GetDeclaredIndexes()))
            {
                var includeProperties = index.GetIncludeProperties();
                if (includeProperties?.Count > 0)
                {
                    var notFound = includeProperties
                        .FirstOrDefault(i => index.DeclaringEntityType.FindProperty(i) == null);

                    if (notFound != null)
                    {
                        throw new InvalidOperationException(
                            SqlServerStrings.IncludePropertyNotFound(
                                notFound,
                                index.Name == null ? index.Properties.Format() : "'" + index.Name + "'",
                                index.DeclaringEntityType.DisplayName()));
                    }

                    var duplicateProperty = includeProperties
                        .GroupBy(i => i)
                        .Where(g => g.Count() > 1)
                        .Select(y => y.Key)
                        .FirstOrDefault();

                    if (duplicateProperty != null)
                    {
                        throw new InvalidOperationException(
                            SqlServerStrings.IncludePropertyDuplicated(
                                index.DeclaringEntityType.DisplayName(),
                                duplicateProperty,
                                index.Name == null ? index.Properties.Format() : "'" + index.Name + "'"));
                    }

                    var coveredProperty = includeProperties
                        .FirstOrDefault(i => index.Properties.Any(p => i == p.Name));

                    if (coveredProperty != null)
                    {
                        throw new InvalidOperationException(
                            SqlServerStrings.IncludePropertyInIndex(
                                index.DeclaringEntityType.DisplayName(),
                                coveredProperty,
                                index.Name == null ? index.Properties.Format() : "'" + index.Name + "'"));
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
        protected override void ValidateSharedTableCompatibility(
            IReadOnlyList<IEntityType> mappedTypes,
            string tableName,
            string? schema,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            var firstMappedType = mappedTypes[0];
            var isMemoryOptimized = firstMappedType.IsMemoryOptimized();

            foreach (var otherMappedType in mappedTypes.Skip(1))
            {
                if (isMemoryOptimized != otherMappedType.IsMemoryOptimized())
                {
                    throw new InvalidOperationException(
                        SqlServerStrings.IncompatibleTableMemoryOptimizedMismatch(
                            tableName, firstMappedType.DisplayName(), otherMappedType.DisplayName(),
                            isMemoryOptimized ? firstMappedType.DisplayName() : otherMappedType.DisplayName(),
                            !isMemoryOptimized ? firstMappedType.DisplayName() : otherMappedType.DisplayName()));
                }
            }

            base.ValidateSharedTableCompatibility(mappedTypes, tableName, schema, logger);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void ValidateSharedColumnsCompatibility(
            IReadOnlyList<IEntityType> mappedTypes,
            in StoreObjectIdentifier storeObject,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            base.ValidateSharedColumnsCompatibility(mappedTypes, storeObject, logger);

            var identityColumns = new Dictionary<string, IProperty>();

            foreach (var property in mappedTypes.SelectMany(et => et.GetDeclaredProperties()))
            {
                if (property.GetValueGenerationStrategy(storeObject) == SqlServerValueGenerationStrategy.IdentityColumn)
                {
                    var columnName = property.GetColumnName(storeObject);
                    if (columnName == null)
                    {
                        continue;
                    }

                    identityColumns[columnName] = property;
                }
            }

            if (identityColumns.Count > 1)
            {
                var sb = new StringBuilder()
                    .AppendJoin(identityColumns.Values.Select(p => "'" + p.DeclaringEntityType.DisplayName() + "." + p.Name + "'"));
                throw new InvalidOperationException(SqlServerStrings.MultipleIdentityColumns(sb, storeObject.DisplayName()));
            }
        }

        /// <inheritdoc />
        protected override void ValidateCompatible(
            IProperty property,
            IProperty duplicateProperty,
            string columnName,
            in StoreObjectIdentifier storeObject,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            base.ValidateCompatible(property, duplicateProperty, columnName, storeObject, logger);

            var propertyStrategy = property.GetValueGenerationStrategy(storeObject);
            var duplicatePropertyStrategy = duplicateProperty.GetValueGenerationStrategy(storeObject);
            if (propertyStrategy != duplicatePropertyStrategy)
            {
                var isConflicting = ((IConventionProperty)property)
                    .FindAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy)
                    ?.GetConfigurationSource() == ConfigurationSource.Explicit
                    || propertyStrategy != SqlServerValueGenerationStrategy.None;
                var isDuplicateConflicting = ((IConventionProperty)duplicateProperty)
                    .FindAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy)
                    ?.GetConfigurationSource() == ConfigurationSource.Explicit
                    || duplicatePropertyStrategy != SqlServerValueGenerationStrategy.None;

                if (isConflicting && isDuplicateConflicting)
                {
                    throw new InvalidOperationException(
                        SqlServerStrings.DuplicateColumnNameValueGenerationStrategyMismatch(
                            duplicateProperty.DeclaringEntityType.DisplayName(),
                            duplicateProperty.Name,
                            property.DeclaringEntityType.DisplayName(),
                            property.Name,
                            columnName,
                            storeObject.DisplayName()));
                }
            }
            else
            {
                switch (propertyStrategy)
                {
                    case SqlServerValueGenerationStrategy.IdentityColumn:
                        var increment = property.GetIdentityIncrement(storeObject);
                        var duplicateIncrement = duplicateProperty.GetIdentityIncrement(storeObject);
                        if (increment != duplicateIncrement)
                        {
                            throw new InvalidOperationException(
                                SqlServerStrings.DuplicateColumnIdentityIncrementMismatch(
                                    duplicateProperty.DeclaringEntityType.DisplayName(),
                                    duplicateProperty.Name,
                                    property.DeclaringEntityType.DisplayName(),
                                    property.Name,
                                    columnName,
                                    storeObject.DisplayName()));
                        }

                        var seed = property.GetIdentitySeed(storeObject);
                        var duplicateSeed = duplicateProperty.GetIdentitySeed(storeObject);
                        if (seed != duplicateSeed)
                        {
                            throw new InvalidOperationException(
                                SqlServerStrings.DuplicateColumnIdentitySeedMismatch(
                                    duplicateProperty.DeclaringEntityType.DisplayName(),
                                    duplicateProperty.Name,
                                    property.DeclaringEntityType.DisplayName(),
                                    property.Name,
                                    columnName,
                                    storeObject.DisplayName()));
                        }

                        break;
                    case SqlServerValueGenerationStrategy.SequenceHiLo:
                        if (property.GetHiLoSequenceName(storeObject) != duplicateProperty.GetHiLoSequenceName(storeObject)
                            || property.GetHiLoSequenceSchema(storeObject) != duplicateProperty.GetHiLoSequenceSchema(storeObject))
                        {
                            throw new InvalidOperationException(
                                SqlServerStrings.DuplicateColumnSequenceMismatch(
                                    duplicateProperty.DeclaringEntityType.DisplayName(),
                                    duplicateProperty.Name,
                                    property.DeclaringEntityType.DisplayName(),
                                    property.Name,
                                    columnName,
                                    storeObject.DisplayName()));
                        }

                        break;
                }
            }

            if (property.IsSparse(storeObject) != duplicateProperty.IsSparse(storeObject))
            {
                throw new InvalidOperationException(
                    SqlServerStrings.DuplicateColumnSparsenessMismatch(
                        duplicateProperty.DeclaringEntityType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringEntityType.DisplayName(),
                        property.Name,
                        columnName,
                        storeObject.DisplayName()));
            }
        }

        /// <inheritdoc />
        protected override void ValidateCompatible(
            IKey key,
            IKey duplicateKey,
            string keyName,
            in StoreObjectIdentifier storeObject,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            base.ValidateCompatible(key, duplicateKey, keyName, storeObject, logger);

            key.AreCompatibleForSqlServer(duplicateKey, storeObject, shouldThrow: true);
        }

        /// <inheritdoc />
        protected override void ValidateCompatible(
            IIndex index,
            IIndex duplicateIndex,
            string indexName,
            in StoreObjectIdentifier storeObject,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            base.ValidateCompatible(index, duplicateIndex, indexName, storeObject, logger);

            index.AreCompatibleForSqlServer(duplicateIndex, storeObject, shouldThrow: true);
        }
    }
}
