// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Extensions.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ValidateTemporalTables(model, logger);
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
                     .Where(
                         p => p.ClrType.UnwrapNullableType() == typeof(decimal)
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
    protected override void ValidateValueGeneration(
        IEntityType entityType,
        IKey key,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (entityType.GetTableName() != null
            && (string?)entityType[RelationalAnnotationNames.MappingStrategy] == RelationalAnnotationNames.TpcMappingStrategy)
        {
            foreach (var storeGeneratedProperty in key.Properties.Where(
                         p => (p.ValueGenerated & ValueGenerated.OnAdd) != 0
                             && p.GetValueGenerationStrategy() == SqlServerValueGenerationStrategy.IdentityColumn))
            {
                logger.TpcStoreGeneratedIdentityWarning(storeGeneratedProperty);
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
                            index.DisplayName(),
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
                            index.DisplayName()));
                }

                var coveredProperty = includeProperties
                    .FirstOrDefault(i => index.Properties.Any(p => i == p.Name));

                if (coveredProperty != null)
                {
                    throw new InvalidOperationException(
                        SqlServerStrings.IncludePropertyInIndex(
                            index.DeclaringEntityType.DisplayName(),
                            coveredProperty,
                            index.DisplayName()));
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
    protected virtual void ValidateTemporalTables(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var temporalEntityTypes = model.GetEntityTypes().Where(t => t.IsTemporal()).ToList();
        foreach (var temporalEntityType in temporalEntityTypes)
        {
            if (temporalEntityType.BaseType != null)
            {
                throw new InvalidOperationException(SqlServerStrings.TemporalOnlyOnRoot(temporalEntityType.DisplayName()));
            }

            ValidateTemporalPeriodProperty(temporalEntityType, periodStart: true);
            ValidateTemporalPeriodProperty(temporalEntityType, periodStart: false);

            var derivedTableMappings = temporalEntityType.GetDerivedTypes().Select(t => t.GetTableName()).Distinct().ToList();
            if (derivedTableMappings.Count > 0
                && (derivedTableMappings.Count != 1 || derivedTableMappings.First() != temporalEntityType.GetTableName()))
            {
                throw new InvalidOperationException(SqlServerStrings.TemporalOnlySupportedForTPH(temporalEntityType.DisplayName()));
            }
        }
    }

    private static void ValidateTemporalPeriodProperty(IEntityType temporalEntityType, bool periodStart)
    {
        var annotationPropertyName = periodStart
            ? temporalEntityType.GetPeriodStartPropertyName()
            : temporalEntityType.GetPeriodEndPropertyName();

        if (annotationPropertyName == null)
        {
            throw new InvalidOperationException(
                SqlServerStrings.TemporalMustDefinePeriodProperties(
                    temporalEntityType.DisplayName()));
        }

        var periodProperty = temporalEntityType.FindProperty(annotationPropertyName);
        if (periodProperty == null)
        {
            throw new InvalidOperationException(
                SqlServerStrings.TemporalExpectedPeriodPropertyNotFound(
                    temporalEntityType.DisplayName(), annotationPropertyName));
        }

        if (!periodProperty.IsShadowProperty() && !temporalEntityType.IsPropertyBag)
        {
            throw new InvalidOperationException(
                SqlServerStrings.TemporalPeriodPropertyMustBeInShadowState(
                    temporalEntityType.DisplayName(), periodProperty.Name));
        }

        if (periodProperty.IsNullable
            || periodProperty.ClrType != typeof(DateTime))
        {
            throw new InvalidOperationException(
                SqlServerStrings.TemporalPeriodPropertyMustBeNonNullableDateTime(
                    temporalEntityType.DisplayName(), periodProperty.Name, nameof(DateTime)));
        }

        const string expectedPeriodColumnNameWithoutPrecision = "datetime2";
        const string expectedPeriodColumnNameWithPrecision = "datetime2({0})";

        var precision = periodProperty.GetPrecision();
        var expectedPeriodColumnName = precision != null
            ? string.Format(expectedPeriodColumnNameWithPrecision, precision.Value)
            : expectedPeriodColumnNameWithoutPrecision;

        if (periodProperty.GetColumnType() != expectedPeriodColumnName)
        {
            throw new InvalidOperationException(
                SqlServerStrings.TemporalPeriodPropertyMustBeMappedToDatetime2(
                    temporalEntityType.DisplayName(), periodProperty.Name, expectedPeriodColumnName));
        }

        if (periodProperty.TryGetDefaultValue(out var _))
        {
            throw new InvalidOperationException(
                SqlServerStrings.TemporalPeriodPropertyCantHaveDefaultValue(
                    temporalEntityType.DisplayName(), periodProperty.Name));
        }

        if (periodProperty.ValueGenerated != ValueGenerated.OnAddOrUpdate)
        {
            throw new InvalidOperationException(
                SqlServerStrings.TemporalPropertyMappedToPeriodColumnMustBeValueGeneratedOnAddOrUpdate(
                    temporalEntityType.DisplayName(), periodProperty.Name, nameof(ValueGenerated.OnAddOrUpdate)));
        }

        // TODO: check that period property is excluded from query (once the annotation is added)
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void ValidateSharedTableCompatibility(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier storeObject,
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
                        storeObject.DisplayName(), firstMappedType.DisplayName(), otherMappedType.DisplayName(),
                        isMemoryOptimized ? firstMappedType.DisplayName() : otherMappedType.DisplayName(),
                        !isMemoryOptimized ? firstMappedType.DisplayName() : otherMappedType.DisplayName()));
            }
        }

        bool? firstSqlOutputSetting = null;
        firstMappedType = null;
        foreach (var mappedType in mappedTypes)
        {
            if (((IConventionEntityType)mappedType).GetUseSqlOutputClauseConfigurationSource() is null)
            {
                continue;
            }

            if (firstSqlOutputSetting is null)
            {
                (firstSqlOutputSetting, firstMappedType) = (mappedType.IsSqlOutputClauseUsed(), mappedType);
            }
            else if (mappedType.IsSqlOutputClauseUsed() != firstSqlOutputSetting)
            {
                throw new InvalidOperationException(
                    SqlServerStrings.IncompatibleSqlOutputClauseMismatch(
                        storeObject.DisplayName(), firstMappedType!.DisplayName(), mappedType.DisplayName(),
                        firstSqlOutputSetting.Value ? firstMappedType.DisplayName() : mappedType.DisplayName(),
                        !firstSqlOutputSetting.Value ? firstMappedType.DisplayName() : mappedType.DisplayName()));
            }
        }

        if (mappedTypes.Any(t => t.IsTemporal())
            && mappedTypes.Select(t => t.GetRootType()).Distinct().Count() > 1)
        {
            // table splitting is only supported when all entities mapped to this table have consistent temporal period mappings also
            var expectedPeriodStartColumnName = default(string);
            var expectedPeriodEndColumnName = default(string);

            foreach (var mappedType in mappedTypes.Where(t => t.BaseType == null))
            {
                if (!mappedType.IsTemporal())
                {
                    throw new InvalidOperationException(
                        SqlServerStrings.TemporalAllEntitiesMappedToSameTableMustBeTemporal(
                            mappedType.DisplayName()));
                }

                var periodStartPropertyName = mappedType.GetPeriodStartPropertyName();
                var periodEndPropertyName = mappedType.GetPeriodEndPropertyName();

                var periodStartProperty = mappedType.GetProperty(periodStartPropertyName!);
                var periodEndProperty = mappedType.GetProperty(periodEndPropertyName!);

                var periodStartColumnName = periodStartProperty.GetColumnName(storeObject);
                var periodEndColumnName = periodEndProperty.GetColumnName(storeObject);

                if (expectedPeriodStartColumnName == null)
                {
                    expectedPeriodStartColumnName = periodStartColumnName;
                }
                else if (expectedPeriodStartColumnName != periodStartColumnName)
                {
                    throw new InvalidOperationException(
                        SqlServerStrings.TemporalNotSupportedForTableSplittingWithInconsistentPeriodMapping(
                            "start",
                            mappedType.DisplayName(),
                            periodStartPropertyName,
                            periodStartColumnName,
                            expectedPeriodStartColumnName));
                }

                if (expectedPeriodEndColumnName == null)
                {
                    expectedPeriodEndColumnName = periodEndColumnName;
                }
                else if (expectedPeriodEndColumnName != periodEndColumnName)
                {
                    throw new InvalidOperationException(
                        SqlServerStrings.TemporalNotSupportedForTableSplittingWithInconsistentPeriodMapping(
                            "end",
                            mappedType.DisplayName(),
                            periodEndPropertyName,
                            periodEndColumnName,
                            expectedPeriodEndColumnName));
                }
            }
        }

        base.ValidateSharedTableCompatibility(mappedTypes, storeObject, logger);
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
            var columnName = property.GetColumnName(storeObject);
            if (columnName == null)
            {
                continue;
            }

            if (property.GetValueGenerationStrategy(storeObject) == SqlServerValueGenerationStrategy.IdentityColumn)
            {
                identityColumns[columnName] = property;
            }
        }

        if (identityColumns.Count > 1)
        {
            var sb = new StringBuilder()
                .AppendJoin(identityColumns.Values.Select(p => "'" + p.DeclaringType.DisplayName() + "." + p.Name + "'"));
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
                ?.GetConfigurationSource()
                == ConfigurationSource.Explicit
                || propertyStrategy != SqlServerValueGenerationStrategy.None;
            var isDuplicateConflicting = ((IConventionProperty)duplicateProperty)
                .FindAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy)
                ?.GetConfigurationSource()
                == ConfigurationSource.Explicit
                || duplicatePropertyStrategy != SqlServerValueGenerationStrategy.None;

            if (isConflicting && isDuplicateConflicting)
            {
                throw new InvalidOperationException(
                    SqlServerStrings.DuplicateColumnNameValueGenerationStrategyMismatch(
                        duplicateProperty.DeclaringType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringType.DisplayName(),
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
                                duplicateProperty.DeclaringType.DisplayName(),
                                duplicateProperty.Name,
                                property.DeclaringType.DisplayName(),
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
                                duplicateProperty.DeclaringType.DisplayName(),
                                duplicateProperty.Name,
                                property.DeclaringType.DisplayName(),
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
                                duplicateProperty.DeclaringType.DisplayName(),
                                duplicateProperty.Name,
                                property.DeclaringType.DisplayName(),
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
                    duplicateProperty.DeclaringType.DisplayName(),
                    duplicateProperty.Name,
                    property.DeclaringType.DisplayName(),
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
