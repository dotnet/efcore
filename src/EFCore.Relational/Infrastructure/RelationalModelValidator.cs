// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     The validator that enforces rules common for all relational providers.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public class RelationalModelValidator : ModelValidator
{
    /// <summary>
    ///     Creates a new instance of <see cref="RelationalModelValidator" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this service.</param>
    public RelationalModelValidator(
        ModelValidatorDependencies dependencies,
        RelationalModelValidatorDependencies relationalDependencies)
        : base(dependencies)
    {
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalModelValidatorDependencies RelationalDependencies { get; }

    /// <summary>
    ///     Validates a model, throwing an exception if any errors are found.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    public override void Validate(IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.Validate(model, logger);

        ValidateMappingFragments(model, logger);
        ValidatePropertyOverrides(model, logger);
        ValidateSqlQueries(model, logger);
        ValidateDbFunctions(model, logger);
        ValidateStoredProcedures(model, logger);
        ValidateSharedTableCompatibility(model, logger);
        ValidateSharedViewCompatibility(model, logger);
        ValidateDefaultValuesOnKeys(model, logger);
        ValidateBoolsWithDefaults(model, logger);
        ValidateIndexProperties(model, logger);
        ValidateJsonEntities(model, logger);
    }

    /// <summary>
    ///     Validates the mapping/configuration of SQL queries in the model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSqlQueries(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            var sqlQuery = entityType.GetSqlQuery();
            if (sqlQuery == null)
            {
                continue;
            }

            if (entityType.BaseType != null
                && (entityType.FindDiscriminatorProperty() == null
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
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateDbFunctions(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
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
                    && entityType.FindDiscriminatorProperty() == null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.TableValuedFunctionNonTph(dbFunction.ModelName, entityType.DisplayName()));
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
    ///     Validates the mapping/configuration of stored procedures in the model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateStoredProcedures(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var storedProcedures = new Dictionary<StoreObjectIdentifier, List<IEntityType>>();
        foreach (var entityType in model.GetEntityTypes())
        {
            var mappingStrategy = entityType.GetMappingStrategy() ?? RelationalAnnotationNames.TphMappingStrategy;

            var sprocCount = 0;
            var deleteStoredProcedure = entityType.GetDeleteStoredProcedure();
            if (deleteStoredProcedure != null)
            {
                AddSproc(StoreObjectType.DeleteStoredProcedure, entityType, storedProcedures);
                ValidateSproc(deleteStoredProcedure, mappingStrategy, logger);
                sprocCount++;
            }

            var insertStoredProcedure = entityType.GetInsertStoredProcedure();
            if (insertStoredProcedure != null)
            {
                AddSproc(StoreObjectType.InsertStoredProcedure, entityType, storedProcedures);
                ValidateSproc(insertStoredProcedure, mappingStrategy, logger);
                sprocCount++;
            }

            var updateStoredProcedure = entityType.GetUpdateStoredProcedure();
            if (updateStoredProcedure != null)
            {
                AddSproc(StoreObjectType.UpdateStoredProcedure, entityType, storedProcedures);
                ValidateSproc(updateStoredProcedure, mappingStrategy, logger);
                sprocCount++;
            }

            if (sprocCount > 0
                // TODO: Support this with #28703
                //&& sprocCount < 3
                && entityType.GetTableName() == null)
            {
                throw new InvalidOperationException(RelationalStrings.StoredProcedureUnmapped(entityType.DisplayName()));
            }
        }

        foreach (var (sproc, mappedTypes) in storedProcedures)
        {
            foreach (var mappedType in mappedTypes)
            {
                if (mappedTypes[0].GetRootType() != mappedType.GetRootType())
                {
                    throw new InvalidOperationException(
                        RelationalStrings.StoredProcedureTableSharing(
                            mappedTypes[0].DisplayName(),
                            mappedType.DisplayName(),
                            sproc.DisplayName()));
                }
            }
        }

        static void AddSproc(
            StoreObjectType storedProcedureType,
            IEntityType entityType,
            Dictionary<StoreObjectIdentifier, List<IEntityType>> storedProcedures)
        {
            var sprocId = StoreObjectIdentifier.Create(entityType, storedProcedureType);
            if (sprocId == null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.StoredProcedureNoName(
                        entityType.DisplayName(), storedProcedureType));
            }

            if (!storedProcedures.TryGetValue(sprocId.Value, out var mappedTypes))
            {
                mappedTypes = [];
                storedProcedures[sprocId.Value] = mappedTypes;
            }

            mappedTypes.Add(entityType);
        }
    }

    private static void ValidateSproc(
        IStoredProcedure sproc,
        string mappingStrategy,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var entityType = sproc.EntityType;
        var storeObjectIdentifier = sproc.GetStoreIdentifier();

        var primaryKey = entityType.FindPrimaryKey();
        if (primaryKey == null)
        {
            throw new InvalidOperationException(
                RelationalStrings.StoredProcedureKeyless(
                    entityType.DisplayName(), storeObjectIdentifier.DisplayName()));
        }

        var properties = entityType.GetDeclaredProperties().ToDictionary(p => p.Name);
        if (mappingStrategy == RelationalAnnotationNames.TphMappingStrategy)
        {
            if (entityType.BaseType != null)
            {
                return;
            }

            foreach (var property in entityType.GetDerivedProperties())
            {
                properties.Add(property.Name, property);
            }
        }
        else if (mappingStrategy == RelationalAnnotationNames.TpcMappingStrategy)
        {
            if (entityType.BaseType != null)
            {
                foreach (var property in entityType.BaseType.GetProperties())
                {
                    properties.Add(property.Name, property);
                }
            }
        }
        else if (mappingStrategy == RelationalAnnotationNames.TptMappingStrategy)
        {
            var baseType = entityType.BaseType;
            if (baseType != null)
            {
                foreach (var property in primaryKey.Properties)
                {
                    properties.Add(property.Name, property);
                }

                while (baseType != null && baseType.IsAbstract())
                {
                    if (StoredProcedure.FindDeclaredStoredProcedure(baseType, storeObjectIdentifier.StoreObjectType) != null)
                    {
                        break;
                    }

                    foreach (var property in baseType.GetDeclaredProperties())
                    {
                        if (property.IsPrimaryKey())
                        {
                            continue;
                        }

                        properties.Add(property.Name, property);
                    }

                    baseType = baseType.BaseType;
                }
            }
        }

        var storeGeneratedProperties = storeObjectIdentifier.StoreObjectType switch
        {
            StoreObjectType.InsertStoredProcedure
                => properties.Where(p => p.Value.ValueGenerated.HasFlag(ValueGenerated.OnAdd)).ToDictionary(),
            StoreObjectType.UpdateStoredProcedure
                => properties.Where(p => p.Value.ValueGenerated.HasFlag(ValueGenerated.OnUpdate)).ToDictionary(),
            _ => new Dictionary<string, IProperty>()
        };

        if (mappingStrategy == RelationalAnnotationNames.TptMappingStrategy
            && storeObjectIdentifier.StoreObjectType == StoreObjectType.InsertStoredProcedure
            && entityType.BaseType?.GetInsertStoredProcedure() != null)
        {
            foreach (var property in primaryKey.Properties)
            {
                storeGeneratedProperties.Remove(property.Name);
            }
        }

        var resultColumnNames = new HashSet<string>();
        foreach (var resultColumn in sproc.ResultColumns)
        {
            IProperty? property = null!;
            if (resultColumn.PropertyName != null
                && !properties.TryGetValue(resultColumn.PropertyName, out property))
            {
                throw new InvalidOperationException(
                    RelationalStrings.StoredProcedureResultColumnNotFound(
                        resultColumn.PropertyName, entityType.DisplayName(), storeObjectIdentifier.DisplayName()));
            }

            if (!resultColumnNames.Add(resultColumn.Name))
            {
                throw new InvalidOperationException(
                    RelationalStrings.StoredProcedureDuplicateResultColumnName(
                        resultColumn.Name, storeObjectIdentifier.DisplayName()));
            }

            if (resultColumn.PropertyName == null)
            {
                continue;
            }

            switch (storeObjectIdentifier.StoreObjectType)
            {
                case StoreObjectType.InsertStoredProcedure:
                case StoreObjectType.UpdateStoredProcedure:
                    if (!storeGeneratedProperties.ContainsKey(property.Name))
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.StoredProcedureResultColumnNotGenerated(
                                entityType.DisplayName(), resultColumn.PropertyName, storeObjectIdentifier.DisplayName()));
                    }

                    break;
                case StoreObjectType.DeleteStoredProcedure:
                    throw new InvalidOperationException(
                        RelationalStrings.StoredProcedureResultColumnDelete(
                            entityType.DisplayName(), resultColumn.PropertyName, storeObjectIdentifier.DisplayName()));
                default:
                    Check.DebugFail("Unexpected stored procedure type: " + storeObjectIdentifier.StoreObjectType);
                    break;
            }
        }

        var originalValueProperties = new Dictionary<string, IProperty>(properties);
        var parameterNames = new HashSet<string>();
        foreach (var parameter in sproc.Parameters)
        {
            IProperty property = null!;
            if (parameter.PropertyName != null)
            {
                if (parameter.ForOriginalValue == true)
                {
                    if (!originalValueProperties.TryGetAndRemove(parameter.PropertyName, out property))
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.StoredProcedureParameterNotFound(
                                parameter.PropertyName, entityType.DisplayName(), storeObjectIdentifier.DisplayName()));
                    }

                    if (storeObjectIdentifier.StoreObjectType == StoreObjectType.InsertStoredProcedure)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.StoredProcedureOriginalValueParameterOnInsert(
                                parameter.Name, storeObjectIdentifier.DisplayName()));
                    }
                }
                else
                {
                    if (!properties.TryGetAndRemove(parameter.PropertyName, out property))
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.StoredProcedureParameterNotFound(
                                parameter.PropertyName, entityType.DisplayName(), storeObjectIdentifier.DisplayName()));
                    }

                    if (storeObjectIdentifier.StoreObjectType == StoreObjectType.DeleteStoredProcedure)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.StoredProcedureCurrentValueParameterOnDelete(
                                parameter.Name, storeObjectIdentifier.DisplayName()));
                    }

                    if (parameter.Direction.HasFlag(ParameterDirection.Input))
                    {
                        switch (storeObjectIdentifier.StoreObjectType)
                        {
                            case StoreObjectType.InsertStoredProcedure:
                                if (property.GetBeforeSaveBehavior() != PropertySaveBehavior.Save)
                                {
                                    throw new InvalidOperationException(
                                        RelationalStrings.StoredProcedureInputParameterForInsertNonSaveProperty(
                                            parameter.Name,
                                            storeObjectIdentifier.DisplayName(),
                                            parameter.PropertyName,
                                            entityType.DisplayName(),
                                            property.GetBeforeSaveBehavior()));
                                }

                                break;

                            case StoreObjectType.UpdateStoredProcedure:
                                if (property.GetAfterSaveBehavior() != PropertySaveBehavior.Save)
                                {
                                    throw new InvalidOperationException(
                                        RelationalStrings.StoredProcedureInputParameterForUpdateNonSaveProperty(
                                            parameter.Name,
                                            storeObjectIdentifier.DisplayName(),
                                            parameter.PropertyName,
                                            entityType.DisplayName(),
                                            property.GetAfterSaveBehavior()));
                                }

                                break;

                            case StoreObjectType.DeleteStoredProcedure:
                                break;

                            default:
                                Check.DebugFail("Unexpected stored procedure type: " + storeObjectIdentifier.StoreObjectType);
                                break;
                        }
                    }
                }
            }

            if (!parameterNames.Add(parameter.Name))
            {
                throw new InvalidOperationException(
                    RelationalStrings.StoredProcedureDuplicateParameterName(
                        parameter.Name, storeObjectIdentifier.DisplayName()));
            }

            if (parameter.PropertyName == null)
            {
                continue;
            }

            switch (storeObjectIdentifier.StoreObjectType)
            {
                case StoreObjectType.InsertStoredProcedure:
                case StoreObjectType.UpdateStoredProcedure:
                    if (parameter.Direction != ParameterDirection.Input
                        && !storeGeneratedProperties.Remove(property.Name))
                    {
                        if (sproc.Parameters.Any(
                                p => p.PropertyName == property.Name
                                    && p.ForOriginalValue != parameter.ForOriginalValue
                                    && p.Direction != ParameterDirection.Input))
                        {
                            throw new InvalidOperationException(
                                RelationalStrings.StoredProcedureOutputParameterConflict(
                                    entityType.DisplayName(), parameter.PropertyName, storeObjectIdentifier.DisplayName()));
                        }

                        throw new InvalidOperationException(
                            RelationalStrings.StoredProcedureOutputParameterNotGenerated(
                                entityType.DisplayName(), parameter.PropertyName, storeObjectIdentifier.DisplayName()));
                    }

                    break;
                case StoreObjectType.DeleteStoredProcedure:
                    if (!property.IsPrimaryKey()
                        && !property.IsConcurrencyToken)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.StoredProcedureDeleteNonKeyProperty(
                                entityType.DisplayName(), parameter.PropertyName, storeObjectIdentifier.DisplayName()));
                    }

                    break;
                default:
                    Check.DebugFail("Unexpected stored procedure type: " + storeObjectIdentifier.StoreObjectType);
                    break;
            }
        }

        foreach (var resultColumn in sproc.ResultColumns)
        {
            if (resultColumn.PropertyName == null)
            {
                continue;
            }

            properties.Remove(resultColumn.PropertyName);

            if (!storeGeneratedProperties.Remove(resultColumn.PropertyName))
            {
                throw new InvalidOperationException(
                    RelationalStrings.StoredProcedureResultColumnParameterConflict(
                        entityType.DisplayName(), resultColumn.PropertyName, storeObjectIdentifier.DisplayName()));
            }
        }

        if (storeGeneratedProperties.Count > 0)
        {
            throw new InvalidOperationException(
                RelationalStrings.StoredProcedureGeneratedPropertiesNotMapped(
                    entityType.DisplayName(),
                    storeObjectIdentifier.DisplayName(),
                    storeGeneratedProperties.Values.Format()));
        }

        if (properties.Count > 0)
        {
            foreach (var property in properties.Values.ToList())
            {
                switch (storeObjectIdentifier.StoreObjectType)
                {
                    case StoreObjectType.InsertStoredProcedure:
                        if ((property.ValueGenerated & ValueGenerated.OnAdd) == 0
                            && property.GetBeforeSaveBehavior() != PropertySaveBehavior.Save)
                        {
                            properties.Remove(property.Name);
                        }

                        break;
                    case StoreObjectType.DeleteStoredProcedure:
                        if (!property.IsPrimaryKey()
                            && !property.IsConcurrencyToken)
                        {
                            properties.Remove(property.Name);
                        }

                        break;
                    case StoreObjectType.UpdateStoredProcedure:
                        if (!property.IsPrimaryKey()
                            && !property.IsConcurrencyToken
                            && (property.ValueGenerated & ValueGenerated.OnUpdate) == 0
                            && property.GetAfterSaveBehavior() != PropertySaveBehavior.Save)
                        {
                            properties.Remove(property.Name);
                        }

                        break;
                }
            }

            foreach (var property in properties.Keys.ToList())
            {
                if (!originalValueProperties.ContainsKey(property))
                {
                    properties.Remove(property);
                }
            }

            if (properties.Count > 0)
            {
                throw new InvalidOperationException(
                    RelationalStrings.StoredProcedurePropertiesNotMapped(
                        entityType.DisplayName(),
                        storeObjectIdentifier.DisplayName(),
                        properties.Values.Format()));
            }
        }

        if (sproc.IsRowsAffectedReturned
            || sproc.FindRowsAffectedParameter() != null
            || sproc.FindRowsAffectedResultColumn() != null)
        {
            if (storeObjectIdentifier.StoreObjectType == StoreObjectType.InsertStoredProcedure)
            {
                throw new InvalidOperationException(
                    RelationalStrings.StoredProcedureRowsAffectedForInsert(
                        storeObjectIdentifier.DisplayName()));
            }

            if (originalValueProperties.Values.FirstOrDefault(p => p.IsConcurrencyToken) is { } missedConcurrencyToken)
            {
                logger.StoredProcedureConcurrencyTokenNotMapped(entityType, missedConcurrencyToken, storeObjectIdentifier.DisplayName());
            }

            if (sproc.ResultColumns.Any(c => c != sproc.FindRowsAffectedResultColumn()))
            {
                throw new InvalidOperationException(
                    RelationalStrings.StoredProcedureRowsAffectedWithResultColumns(
                        entityType.DisplayName(),
                        storeObjectIdentifier.DisplayName()));
            }
        }
    }

    /// <summary>
    ///     Validates the mapping/configuration of <see cref="bool" /> properties in the model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateBoolsWithDefaults(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            foreach (var property in entityType.GetDeclaredProperties())
            {
                if (!property.ClrType.IsNullableType()
                    && (property.ClrType.IsEnum || property.ClrType == typeof(bool))
                    && property.ValueGenerated != ValueGenerated.Never
                    && property.FieldInfo?.FieldType.IsNullableType() != true
                    && !((IConventionProperty)property).GetSentinelConfigurationSource().HasValue
                    && (StoreObjectIdentifier.Create(property.DeclaringType, StoreObjectType.Table) is { } table
                        && (IsNotNullAndNotDefault(property.GetDefaultValue(table))
                            || property.GetDefaultValueSql(table) != null)))
                {
                    logger.BoolWithDefaultWarning(property);
                }

                bool IsNotNullAndNotDefault(object? value)
                    => value != null
#pragma warning disable EF1001 // Internal EF Core API usage.
                        && !property.ClrType.IsDefaultValue(value);
#pragma warning restore EF1001 // Internal EF Core API usage.
            }
        }
    }

    /// <summary>
    ///     Validates the mapping/configuration of default values in the model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateDefaultValuesOnKeys(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            foreach (var key in entityType.GetDeclaredKeys())
            {
                IProperty? propertyWithDefault = null;
                foreach (var property in key.Properties)
                {
                    var defaultValue = (IConventionAnnotation?)property.FindAnnotation(RelationalAnnotationNames.DefaultValue);
                    if (!property.IsForeignKey()
                        && defaultValue?.Value != null
                        && defaultValue.GetConfigurationSource().Overrides(ConfigurationSource.DataAnnotation))
                    {
                        propertyWithDefault ??= property;
                    }
                    else
                    {
                        propertyWithDefault = null;
                        break;
                    }
                }

                if (propertyWithDefault != null)
                {
                    logger.ModelValidationKeyDefaultValueWarning(propertyWithDefault);
                }
            }
        }
    }

    /// <summary>
    ///     Validates the mapping/configuration of mutable in the model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected override void ValidateNoMutableKeys(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            foreach (var key in entityType.GetDeclaredKeys())
            {
                var mutableProperty = key.Properties.FirstOrDefault(p => p.ValueGenerated.HasFlag(ValueGenerated.OnUpdate));
                if (mutableProperty != null
                    && !mutableProperty.IsOrdinalKeyProperty())
                {
                    throw new InvalidOperationException(CoreStrings.MutableKeyProperty(mutableProperty.Name));
                }
            }
        }
    }

    /// <summary>
    ///     Validates the mapping/configuration of shared tables in the model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSharedTableCompatibility(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var tables = BuildSharedTableEntityMap(model.GetEntityTypes().Where(e => !e.IsMappedToJson()));
        foreach (var (table, mappedTypes) in tables)
        {
            ValidateSharedTableCompatibility(mappedTypes, table, logger);
            ValidateSharedColumnsCompatibility(mappedTypes, table, logger);
            ValidateSharedKeysCompatibility(mappedTypes, table, logger);
            ValidateSharedForeignKeysCompatibility(mappedTypes, table, logger);
            ValidateSharedIndexesCompatibility(mappedTypes, table, logger);
            ValidateSharedCheckConstraintCompatibility(mappedTypes, table, logger);
            ValidateSharedTriggerCompatibility(mappedTypes, table, logger);

            // Validate optional dependents
            if (mappedTypes.Count == 1)
            {
                continue;
            }

            var principalEntityTypesMap = new Dictionary<IEntityType, (List<IEntityType> EntityTypes, bool Optional)>();
            foreach (var entityType in mappedTypes)
            {
                if (entityType.BaseType != null
                    || entityType.FindPrimaryKey() == null)
                {
                    continue;
                }

                var (principalEntityTypes, optional) = GetPrincipalEntityTypes(entityType);
                if (!optional)
                {
                    continue;
                }

                var principalColumns = principalEntityTypes.SelectMany(e => e.GetProperties())
                    .Select(e => e.GetColumnName(table))
                    .Where(e => e != null)
                    .ToList();
                var requiredNonSharedColumnFound = false;
                foreach (var property in entityType.GetProperties())
                {
                    if (property.IsPrimaryKey()
                        || property.IsNullable)
                    {
                        continue;
                    }

                    var columnName = property.GetColumnName(table);
                    if (columnName != null)
                    {
                        if (!principalColumns.Contains(columnName))
                        {
                            requiredNonSharedColumnFound = true;
                            break;
                        }
                    }
                }

                if (!requiredNonSharedColumnFound)
                {
                    if (entityType.GetReferencingForeignKeys().Select(e => e.DeclaringEntityType).Any(t => mappedTypes.Contains(t)))
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.OptionalDependentWithDependentWithoutIdentifyingProperty(entityType.DisplayName()));
                    }

                    logger.OptionalDependentWithoutIdentifyingPropertyWarning(entityType);
                }
            }

            (List<IEntityType> EntityTypes, bool Optional) GetPrincipalEntityTypes(IEntityType entityType)
            {
                if (!principalEntityTypesMap.TryGetValue(entityType, out var tuple))
                {
                    var list = new List<IEntityType>();
                    var optional = false;
                    foreach (var foreignKey in entityType.FindForeignKeys(entityType.FindPrimaryKey()!.Properties))
                    {
                        var principalEntityType = foreignKey.PrincipalEntityType;
                        if (foreignKey.PrincipalEntityType.IsAssignableFrom(foreignKey.DeclaringEntityType)
                            || !mappedTypes.Contains(principalEntityType))
                        {
                            continue;
                        }

                        list.Add(principalEntityType);
                        var (entityTypes, _) = GetPrincipalEntityTypes(principalEntityType.GetRootType());
                        list.AddRange(entityTypes);

                        optional |= !foreignKey.IsRequiredDependent;
                    }

                    tuple = (list, optional);
                    principalEntityTypesMap.Add(entityType, tuple);
                }

                return tuple;
            }
        }
    }

    private Dictionary<StoreObjectIdentifier, List<IEntityType>> BuildSharedTableEntityMap(IEnumerable<IEntityType> entityTypes)
    {
        var result = new Dictionary<StoreObjectIdentifier, List<IEntityType>>();
        foreach (var entityType in entityTypes)
        {
            var tableId = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table);
            if (tableId == null)
            {
                continue;
            }

            var table = tableId.Value;
            if (!result.TryGetValue(table, out var mappedTypes))
            {
                mappedTypes = [];
                result[table] = mappedTypes;
            }

            mappedTypes.Add(entityType);
        }

        return result;
    }

    /// <summary>
    ///     Validates the compatibility of entity types sharing a given table.
    /// </summary>
    /// <param name="mappedTypes">The mapped entity types.</param>
    /// <param name="storeObject">The table identifier.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSharedTableCompatibility(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (mappedTypes.Count == 1)
        {
            return;
        }

        var unvalidatedTypes = new HashSet<IEntityType>(mappedTypes);
        IEntityType? root = null;
        foreach (var mappedType in mappedTypes)
        {
            if (mappedType.BaseType != null && unvalidatedTypes.Contains(mappedType.BaseType))
            {
                continue;
            }

            var primaryKey = mappedType.FindPrimaryKey();
            if (primaryKey != null
                && (mappedType.FindForeignKeys(primaryKey.Properties)
                    .FirstOrDefault(
                        fk => fk.PrincipalKey.IsPrimaryKey()
                            && !fk.PrincipalEntityType.IsAssignableFrom(fk.DeclaringEntityType)
                            && unvalidatedTypes.Contains(fk.PrincipalEntityType)) is { } linkingFK))
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
            var isExcluded = entityType.IsTableExcludedFromMigrations(storeObject);
            var typesToValidateLeft = typesToValidate.Count;
            var directlyConnectedTypes = unvalidatedTypes.Where(
                unvalidatedType =>
                    entityType.IsAssignableFrom(unvalidatedType)
                    || IsIdentifyingPrincipal(unvalidatedType, entityType));

            foreach (var nextEntityType in directlyConnectedTypes)
            {
                if (key != null)
                {
                    var otherKey = nextEntityType.FindPrimaryKey()!;
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

                if (isExcluded.Equals(!nextEntityType.IsTableExcludedFromMigrations(storeObject)))
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
                    storeObject.DisplayName(),
                    invalidEntityType.DisplayName(),
                    root.DisplayName()));
        }
    }

    /// <summary>
    ///     Validates the mapping/configuration of shared views in the model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSharedViewCompatibility(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var views = new Dictionary<StoreObjectIdentifier, List<IEntityType>>();
        foreach (var entityType in model.GetEntityTypes().Where(e => !e.IsMappedToJson()))
        {
            var viewsName = entityType.GetViewName();
            if (viewsName == null)
            {
                continue;
            }

            var view = StoreObjectIdentifier.View(viewsName, entityType.GetViewSchema());
            if (!views.TryGetValue(view, out var mappedTypes))
            {
                mappedTypes = [];
                views[view] = mappedTypes;
            }

            mappedTypes.Add(entityType);
        }

        foreach (var (view, mappedTypes) in views)
        {
            ValidateSharedViewCompatibility(mappedTypes, view.Name, view.Schema, logger);
            ValidateSharedColumnsCompatibility(mappedTypes, view, logger);
        }
    }

    /// <summary>
    ///     Validates the compatibility of entity types sharing a given view.
    /// </summary>
    /// <param name="mappedTypes">The mapped entity types.</param>
    /// <param name="viewName">The view name.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSharedViewCompatibility(
        IReadOnlyList<IEntityType> mappedTypes,
        string viewName,
        string? schema,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (mappedTypes.Count == 1)
        {
            return;
        }

        var storeObject = StoreObjectIdentifier.View(viewName, schema);
        var unvalidatedTypes = new HashSet<IEntityType>(mappedTypes);
        IEntityType? root = null;
        foreach (var mappedType in mappedTypes)
        {
            if (mappedType.BaseType != null && unvalidatedTypes.Contains(mappedType.BaseType))
            {
                continue;
            }

            if (mappedType.FindPrimaryKey() != null
                && mappedType.FindForeignKeys(mappedType.FindPrimaryKey()!.Properties)
                    .Any(
                        fk => fk.PrincipalKey.IsPrimaryKey()
                            && unvalidatedTypes.Contains(fk.PrincipalEntityType)))
            {
                if (mappedType.BaseType != null)
                {
                    var principalType = mappedType.FindForeignKeys(mappedType.FindPrimaryKey()!.Properties)
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
        => dependentEntityType.FindForeignKeys(dependentEntityType.FindPrimaryKey()!.Properties)
            .Any(
                fk => fk.PrincipalKey.IsPrimaryKey()
                    && fk.PrincipalEntityType == principalEntityType);

    /// <summary>
    ///     Validates the compatibility of properties sharing columns in a given table-like object.
    /// </summary>
    /// <param name="mappedTypes">The mapped entity types.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSharedColumnsCompatibility(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var concurrencyColumns = TableSharingConcurrencyTokenConvention.GetConcurrencyTokensMap(storeObject, mappedTypes);
        HashSet<string>? missingConcurrencyTokens = null;
        if (concurrencyColumns != null
            && storeObject.StoreObjectType == StoreObjectType.Table)
        {
            missingConcurrencyTokens = [];
        }

        var propertyMappings = new Dictionary<string, IProperty>();
        foreach (var entityType in mappedTypes)
        {
            if (missingConcurrencyTokens != null)
            {
                missingConcurrencyTokens.Clear();
                foreach (var (key, readOnlyProperties) in concurrencyColumns!)
                {
                    if (TableSharingConcurrencyTokenConvention.IsConcurrencyTokenMissing(readOnlyProperties, entityType, mappedTypes))
                    {
                        missingConcurrencyTokens.Add(key);
                    }
                }
            }

            foreach (var property in entityType.GetDeclaredProperties())
            {
                var columnName = property.GetColumnName(storeObject);
                if (columnName == null)
                {
                    continue;
                }

                missingConcurrencyTokens?.Remove(columnName);
                if (!propertyMappings.TryGetValue(columnName, out var duplicateProperty))
                {
                    propertyMappings[columnName] = property;
                    continue;
                }

                if (property.DeclaringType.IsAssignableFrom(duplicateProperty.DeclaringType)
                    || duplicateProperty.DeclaringType.IsAssignableFrom(property.DeclaringType))
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DuplicateColumnNameSameHierarchy(
                            duplicateProperty.DeclaringType.DisplayName(),
                            duplicateProperty.Name,
                            property.DeclaringType.DisplayName(),
                            property.Name,
                            columnName,
                            storeObject.DisplayName()));
                }

                ValidateCompatible(property, duplicateProperty, columnName, storeObject, logger);
            }

            if (missingConcurrencyTokens != null)
            {
                foreach (var missingColumn in missingConcurrencyTokens)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.MissingConcurrencyColumn(
                            entityType.DisplayName(), missingColumn, storeObject.DisplayName()));
                }
            }
        }

        var columnOrders = new Dictionary<int, List<string>>();
        foreach (var property in propertyMappings.Values)
        {
            var columnOrder = property.GetColumnOrder(storeObject);
            if (!columnOrder.HasValue)
            {
                continue;
            }

            var columns = columnOrders.GetOrAddNew(columnOrder.Value);
            columns.Add(property.GetColumnName(storeObject)!);
        }

        if (columnOrders.Any(g => g.Value.Count > 1))
        {
            logger.DuplicateColumnOrders(
                storeObject,
                columnOrders.Where(g => g.Value.Count > 1).SelectMany(g => g.Value).ToList());
        }
    }

    /// <summary>
    ///     Validates the compatibility of two properties mapped to the same column.
    /// </summary>
    /// <param name="property">A property.</param>
    /// <param name="duplicateProperty">Another property.</param>
    /// <param name="columnName">The column name.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateCompatible(
        IProperty property,
        IProperty duplicateProperty,
        string columnName,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (property.IsColumnNullable(storeObject) != duplicateProperty.IsColumnNullable(storeObject))
        {
            throw new InvalidOperationException(
                RelationalStrings.DuplicateColumnNameNullabilityMismatch(
                    duplicateProperty.DeclaringType.DisplayName(),
                    duplicateProperty.Name,
                    property.DeclaringType.DisplayName(),
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
                    duplicateProperty.DeclaringType.DisplayName(),
                    duplicateProperty.Name,
                    property.DeclaringType.DisplayName(),
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
                    duplicateProperty.DeclaringType.DisplayName(),
                    duplicateProperty.Name,
                    property.DeclaringType.DisplayName(),
                    property.Name,
                    columnName,
                    storeObject.DisplayName()));
        }

        if (property.IsFixedLength(storeObject) != duplicateProperty.IsFixedLength(storeObject))
        {
            throw new InvalidOperationException(
                RelationalStrings.DuplicateColumnNameFixedLengthMismatch(
                    duplicateProperty.DeclaringType.DisplayName(),
                    duplicateProperty.Name,
                    property.DeclaringType.DisplayName(),
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
                    duplicateProperty.DeclaringType.DisplayName(),
                    duplicateProperty.Name,
                    property.DeclaringType.DisplayName(),
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
                    duplicateProperty.DeclaringType.DisplayName(),
                    duplicateProperty.Name,
                    property.DeclaringType.DisplayName(),
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
                    duplicateProperty.DeclaringType.DisplayName(),
                    duplicateProperty.Name,
                    property.DeclaringType.DisplayName(),
                    property.Name,
                    columnName,
                    storeObject.DisplayName()));
        }

        var currentTypeString = property.GetColumnType(storeObject);
        var previousTypeString = duplicateProperty.GetColumnType(storeObject);
        if (!string.Equals(currentTypeString, previousTypeString, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    duplicateProperty.DeclaringType.DisplayName(),
                    duplicateProperty.Name,
                    property.DeclaringType.DisplayName(),
                    property.Name,
                    columnName,
                    storeObject.DisplayName(),
                    previousTypeString,
                    currentTypeString));
        }

        var typeMapping = property.GetRelationalTypeMapping();
        var duplicateTypeMapping = duplicateProperty.GetRelationalTypeMapping();
        var currentProviderType = typeMapping.Converter?.ProviderClrType.UnwrapNullableType()
            ?? typeMapping.ClrType;
        var previousProviderType = duplicateTypeMapping.Converter?.ProviderClrType.UnwrapNullableType()
            ?? duplicateTypeMapping.ClrType;
        if (currentProviderType != previousProviderType
            && (property.IsKey()
                || duplicateProperty.IsKey()
                || property.IsForeignKey()
                || duplicateProperty.IsForeignKey()
                || (property.IsIndex() && property.GetContainingIndexes().Any(i => i.IsUnique))
                || (duplicateProperty.IsIndex() && duplicateProperty.GetContainingIndexes().Any(i => i.IsUnique))))
        {
            throw new InvalidOperationException(
                RelationalStrings.DuplicateColumnNameProviderTypeMismatch(
                    duplicateProperty.DeclaringType.DisplayName(),
                    duplicateProperty.Name,
                    property.DeclaringType.DisplayName(),
                    property.Name,
                    columnName,
                    storeObject.DisplayName(),
                    previousProviderType.ShortDisplayName(),
                    currentProviderType.ShortDisplayName()));
        }

        var currentComputedColumnSql = property.GetComputedColumnSql(storeObject) ?? "";
        var previousComputedColumnSql = duplicateProperty.GetComputedColumnSql(storeObject) ?? "";
        if (!currentComputedColumnSql.Equals(previousComputedColumnSql, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                RelationalStrings.DuplicateColumnNameComputedSqlMismatch(
                    duplicateProperty.DeclaringType.DisplayName(),
                    duplicateProperty.Name,
                    property.DeclaringType.DisplayName(),
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
                    duplicateProperty.DeclaringType.DisplayName(),
                    duplicateProperty.Name,
                    property.DeclaringType.DisplayName(),
                    property.Name,
                    columnName,
                    storeObject.DisplayName(),
                    previousStored,
                    currentStored));
        }

        var hasDefaultValue = property.TryGetDefaultValue(storeObject, out var currentDefaultValue);
        var duplicateHasDefaultValue = duplicateProperty.TryGetDefaultValue(storeObject, out var previousDefaultValue);
        if ((hasDefaultValue
                || duplicateHasDefaultValue)
            && !Equals(currentDefaultValue, previousDefaultValue))
        {
            currentDefaultValue = GetDefaultColumnValue(property, storeObject);
            previousDefaultValue = GetDefaultColumnValue(duplicateProperty, storeObject);

            if (!Equals(currentDefaultValue, previousDefaultValue))
            {
                throw new InvalidOperationException(
                    RelationalStrings.DuplicateColumnNameDefaultSqlMismatch(
                        duplicateProperty.DeclaringType.DisplayName(),
                        duplicateProperty.Name,
                        property.DeclaringType.DisplayName(),
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
                    duplicateProperty.DeclaringType.DisplayName(),
                    duplicateProperty.Name,
                    property.DeclaringType.DisplayName(),
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
                    duplicateProperty.DeclaringType.DisplayName(),
                    duplicateProperty.Name,
                    property.DeclaringType.DisplayName(),
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
                    duplicateProperty.DeclaringType.DisplayName(),
                    duplicateProperty.Name,
                    property.DeclaringType.DisplayName(),
                    property.Name,
                    columnName,
                    storeObject.DisplayName(),
                    previousCollation,
                    currentCollation));
        }

        var currentColumnOrder = property.GetColumnOrder(storeObject);
        var previousColumnOrder = duplicateProperty.GetColumnOrder(storeObject);
        if (currentColumnOrder != previousColumnOrder)
        {
            throw new InvalidOperationException(
                RelationalStrings.DuplicateColumnNameOrderMismatch(
                    duplicateProperty.DeclaringType.DisplayName(),
                    duplicateProperty.Name,
                    property.DeclaringType.DisplayName(),
                    property.Name,
                    columnName,
                    storeObject.DisplayName(),
                    previousColumnOrder,
                    currentColumnOrder));
        }
    }

    /// <summary>
    ///     Returns the object that is used as the default value for the column the property is mapped to.
    /// </summary>
    /// <param name="property">The property to get the default value for.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The object that is used as the default value for the column the property is mapped to.</returns>
    protected virtual object? GetDefaultColumnValue(
        IProperty property,
        in StoreObjectIdentifier storeObject)
    {
        var value = property.GetDefaultValue(storeObject);
        var converter = property.FindRelationalTypeMapping(storeObject)?.Converter;

        return converter != null
            ? converter.ConvertToProvider(value)
            : value;
    }

    /// <summary>
    ///     Validates the compatibility of foreign keys in a given shared table.
    /// </summary>
    /// <param name="mappedTypes">The mapped entity types.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSharedForeignKeysCompatibility(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
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

            var foreignKeyName = foreignKey.GetConstraintName(storeObject, principalTable.Value, logger);
            if (foreignKeyName == null)
            {
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
    /// <param name="foreignKey">A foreign key.</param>
    /// <param name="duplicateForeignKey">Another foreign key.</param>
    /// <param name="foreignKeyName">The foreign key constraint name.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateCompatible(
        IForeignKey foreignKey,
        IForeignKey duplicateForeignKey,
        string foreignKeyName,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        => foreignKey.AreCompatible(duplicateForeignKey, storeObject, shouldThrow: true);

    /// <summary>
    ///     Validates the compatibility of indexes in a given shared table.
    /// </summary>
    /// <param name="mappedTypes">The mapped entity types.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSharedIndexesCompatibility(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var indexMappings = new Dictionary<string, IIndex>();
        foreach (var index in mappedTypes.SelectMany(et => et.GetDeclaredIndexes()))
        {
            var indexName = index.GetDatabaseName(storeObject, logger);
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
    /// <param name="index">An index.</param>
    /// <param name="duplicateIndex">Another index.</param>
    /// <param name="indexName">The name of the index.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateCompatible(
        IIndex index,
        IIndex duplicateIndex,
        string indexName,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        => index.AreCompatible(duplicateIndex, storeObject, shouldThrow: true);

    /// <summary>
    ///     Validates the compatibility of primary and alternate keys in a given shared table.
    /// </summary>
    /// <param name="mappedTypes">The mapped entity types.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSharedKeysCompatibility(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var keyMappings = new Dictionary<string, IKey>();
        foreach (var key in mappedTypes.SelectMany(et => et.GetDeclaredKeys()))
        {
            var keyName = key.GetName(storeObject, logger);
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
    /// <param name="key">A key.</param>
    /// <param name="duplicateKey">Another key.</param>
    /// <param name="keyName">The name of the unique constraint.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateCompatible(
        IKey key,
        IKey duplicateKey,
        string keyName,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        => key.AreCompatible(duplicateKey, storeObject, shouldThrow: true);

    /// <summary>
    ///     Validates the compatibility of check constraints in a given shared table.
    /// </summary>
    /// <param name="mappedTypes">The mapped entity types.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSharedCheckConstraintCompatibility(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var checkConstraintMappings = new Dictionary<string, ICheckConstraint>();
        foreach (var checkConstraint in mappedTypes.SelectMany(et => et.GetDeclaredCheckConstraints()))
        {
            var checkConstraintName = checkConstraint.GetName(storeObject);
            if (checkConstraintName == null)
            {
                continue;
            }

            if (!checkConstraintMappings.TryGetValue(checkConstraintName, out var duplicateCheckConstraint))
            {
                checkConstraintMappings[checkConstraintName] = checkConstraint;
                continue;
            }

            ValidateCompatible(checkConstraint, duplicateCheckConstraint, checkConstraintName, storeObject, logger);
        }
    }

    /// <summary>
    ///     Validates the compatibility of two check constraints with the same name.
    /// </summary>
    /// <param name="checkConstraint">A check constraint.</param>
    /// <param name="duplicateCheckConstraint">Another check constraint.</param>
    /// <param name="indexName">The name of the check constraint.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateCompatible(
        ICheckConstraint checkConstraint,
        ICheckConstraint duplicateCheckConstraint,
        string indexName,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        => CheckConstraint.AreCompatible(checkConstraint, duplicateCheckConstraint, storeObject, shouldThrow: true);

    /// <summary>
    ///     Validates the compatibility of triggers in a given shared table.
    /// </summary>
    /// <param name="mappedTypes">The mapped entity types.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSharedTriggerCompatibility(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var triggerMappings = new Dictionary<string, ITrigger>();
        foreach (var trigger in mappedTypes.SelectMany(et => et.GetDeclaredTriggers()))
        {
            var triggerName = trigger.GetDatabaseName(storeObject);
            if (triggerName == null)
            {
                continue;
            }

            if (!triggerMappings.TryGetValue(triggerName, out var duplicateTrigger))
            {
                triggerMappings[triggerName] = trigger;
                continue;
            }

            ValidateCompatible(trigger, duplicateTrigger, triggerName, storeObject, logger);
        }
    }

    /// <summary>
    ///     Validates the compatibility of two trigger with the same name.
    /// </summary>
    /// <param name="trigger">A trigger.</param>
    /// <param name="duplicateTrigger">Another trigger.</param>
    /// <param name="indexName">The name of the trigger.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateCompatible(
        ITrigger trigger,
        ITrigger duplicateTrigger,
        string indexName,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
    }

    /// <summary>
    ///     Validates the mapping/configuration of inheritance in the model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected override void ValidateInheritanceMapping(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            var mappingStrategy = (string?)entityType[RelationalAnnotationNames.MappingStrategy];
            if (mappingStrategy != null)
            {
                ValidateMappingStrategy(entityType, mappingStrategy);
                var storeObject = entityType.GetSchemaQualifiedTableName()
                    ?? entityType.GetSchemaQualifiedViewName()
                    ?? entityType.GetFunctionName()
                    ?? entityType.GetSqlQuery()
                    ?? entityType.GetInsertStoredProcedure()?.GetSchemaQualifiedName()
                    ?? entityType.GetDeleteStoredProcedure()?.GetSchemaQualifiedName()
                    ?? entityType.GetUpdateStoredProcedure()?.GetSchemaQualifiedName();
                if (mappingStrategy == RelationalAnnotationNames.TpcMappingStrategy
                    && !entityType.ClrType.IsInstantiable()
                    && storeObject != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.AbstractTpc(entityType.DisplayName(), storeObject));
                }
            }

            foreach (var key in entityType.GetKeys())
            {
                ValidateValueGeneration(entityType, key, logger);
            }

            if (entityType.BaseType != null)
            {
                if (mappingStrategy != null
                    && mappingStrategy != (string?)entityType.BaseType[RelationalAnnotationNames.MappingStrategy])
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DerivedStrategy(entityType.DisplayName(), mappingStrategy));
                }

                continue;
            }

            // Hierarchy mapping strategy must be the same across all types of mappings
            if (entityType.FindDiscriminatorProperty() != null)
            {
                if (mappingStrategy != null
                    && mappingStrategy != RelationalAnnotationNames.TphMappingStrategy)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.NonTphMappingStrategy(mappingStrategy, entityType.DisplayName()));
                }

                ValidateTphMapping(entityType, StoreObjectType.Table);
                ValidateTphMapping(entityType, StoreObjectType.View);
                ValidateTphMapping(entityType, StoreObjectType.Function);
                ValidateTphMapping(entityType, StoreObjectType.InsertStoredProcedure);
                ValidateTphMapping(entityType, StoreObjectType.DeleteStoredProcedure);
                ValidateTphMapping(entityType, StoreObjectType.UpdateStoredProcedure);

                ValidateDiscriminatorValues(entityType);
            }
            else
            {
                if (mappingStrategy != RelationalAnnotationNames.TpcMappingStrategy
                    && entityType.FindPrimaryKey() == null
                    && entityType.GetDirectlyDerivedTypes().Any())
                {
                    throw new InvalidOperationException(
                        RelationalStrings.KeylessMappingStrategy(
                            mappingStrategy ?? RelationalAnnotationNames.TptMappingStrategy, entityType.DisplayName()));
                }

                ValidateNonTphMapping(entityType, StoreObjectType.Table);
                ValidateNonTphMapping(entityType, StoreObjectType.View);
                ValidateNonTphMapping(entityType, StoreObjectType.InsertStoredProcedure);
                ValidateNonTphMapping(entityType, StoreObjectType.DeleteStoredProcedure);
                ValidateNonTphMapping(entityType, StoreObjectType.UpdateStoredProcedure);

                var derivedTypes = entityType.GetDerivedTypesInclusive().ToList();
                var discriminatorValues = new Dictionary<string, IEntityType>();
                foreach (var derivedType in derivedTypes)
                {
                    var discriminatorValue = derivedType.GetDiscriminatorValue();
                    if (!derivedType.ClrType.IsInstantiable()
                        || discriminatorValue is null)
                    {
                        continue;
                    }

                    if (discriminatorValue is not string valueString)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.NonTphDiscriminatorValueNotString(discriminatorValue, derivedType.DisplayName()));
                    }

                    if (discriminatorValues.TryGetValue(valueString, out var duplicateEntityType))
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.EntityShortNameNotUnique(
                                derivedType.Name, discriminatorValue, duplicateEntityType.Name));
                    }

                    discriminatorValues[valueString] = derivedType;
                }
            }
        }
    }

    /// <summary>
    ///     Validates the key value generation is valid.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="key">The key.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateValueGeneration(
        IEntityType entityType,
        IKey key,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (entityType.GetTableName() != null
            && (string?)entityType[RelationalAnnotationNames.MappingStrategy] == RelationalAnnotationNames.TpcMappingStrategy)
        {
            foreach (var storeGeneratedProperty in key.Properties.Where(p => (p.ValueGenerated & ValueGenerated.OnAdd) != 0))
            {
                logger.TpcStoreGeneratedIdentityWarning(storeGeneratedProperty);
            }
        }
    }

    /// <summary>
    ///     Validates that the given mapping strategy is supported
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="mappingStrategy">The mapping strategy.</param>
    protected virtual void ValidateMappingStrategy(IEntityType entityType, string? mappingStrategy)
    {
        switch (mappingStrategy)
        {
            case RelationalAnnotationNames.TphMappingStrategy:
            case RelationalAnnotationNames.TpcMappingStrategy:
            case RelationalAnnotationNames.TptMappingStrategy:
                break;
            default:
                throw new InvalidOperationException(
                    RelationalStrings.InvalidMappingStrategy(
                        mappingStrategy, entityType.DisplayName()));
        }
    }

    private static void ValidateNonTphMapping(IEntityType rootEntityType, StoreObjectType storeObjectType)
    {
        var isTpc = rootEntityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy;
        var derivedTypes = new Dictionary<StoreObjectIdentifier, IEntityType>();
        foreach (var entityType in rootEntityType.GetDerivedTypesInclusive())
        {
            var storeObject = StoreObjectIdentifier.Create(entityType, storeObjectType);
            if (storeObject == null)
            {
                var unmappedOwnedType = entityType.GetReferencingForeignKeys()
                    .Where(fk => fk.IsOwnership)
                    .Select(fk => fk.DeclaringEntityType)
                    .FirstOrDefault(owned => StoreObjectIdentifier.Create(owned, storeObjectType) == null
                        && ((IConventionEntityType)owned).GetStoreObjectConfigurationSource(storeObjectType) == null
                        && !owned.IsMappedToJson());
                if (unmappedOwnedType != null
                    && entityType.GetDerivedTypes().Any(derived => StoreObjectIdentifier.Create(derived, storeObjectType) != null))
                {
                    throw new InvalidOperationException(
                        RelationalStrings.UnmappedNonTPHOwner(
                            entityType.DisplayName(),
                            unmappedOwnedType.FindOwnership()!.PrincipalToDependent?.Name,
                            unmappedOwnedType.DisplayName(),
                            storeObjectType));
                }

                continue;
            }

            if (derivedTypes.TryGetValue(storeObject.Value, out var otherType))
            {
                switch (storeObjectType)
                {
                    case StoreObjectType.Table:
                        throw new InvalidOperationException(
                            RelationalStrings.NonTphTableClash(
                                entityType.DisplayName(), otherType.DisplayName(), storeObject.Value.DisplayName()));
                    case StoreObjectType.View:
                        throw new InvalidOperationException(
                            RelationalStrings.NonTphViewClash(
                                entityType.DisplayName(), otherType.DisplayName(), storeObject.Value.DisplayName()));
                    case StoreObjectType.InsertStoredProcedure:
                    case StoreObjectType.DeleteStoredProcedure:
                    case StoreObjectType.UpdateStoredProcedure:
                        throw new InvalidOperationException(
                            RelationalStrings.NonTphStoredProcedureClash(
                                entityType.DisplayName(), otherType.DisplayName(), storeObject.Value.DisplayName()));
                }
            }

            if (isTpc)
            {
                var rowInternalFk = entityType.FindDeclaredReferencingRowInternalForeignKeys(storeObject.Value)
                    .FirstOrDefault();
                if (rowInternalFk != null
                    && entityType.GetDirectlyDerivedTypes().Any())
                {
                    throw new InvalidOperationException(
                        RelationalStrings.TpcTableSharing(
                            rowInternalFk.DeclaringEntityType.DisplayName(),
                            storeObject.Value.DisplayName(),
                            rowInternalFk.PrincipalEntityType.DisplayName()));
                }
            }

            derivedTypes[storeObject.Value] = entityType;
        }

        var rootStoreObject = StoreObjectIdentifier.Create(rootEntityType, storeObjectType);
        if (rootStoreObject == null)
        {
            return;
        }

        if (rootEntityType.FindRowInternalForeignKeys(rootStoreObject.Value).Any()
            && derivedTypes.Count > 1
            && rootEntityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy)
        {
            var derivedTypePair = derivedTypes.First(kv => kv.Value != rootEntityType);
            throw new InvalidOperationException(
                RelationalStrings.TpcTableSharingDependent(
                    rootEntityType.DisplayName(),
                    rootStoreObject.Value.DisplayName(),
                    derivedTypePair.Value.DisplayName(),
                    derivedTypePair.Key.DisplayName()));
        }
    }

    private static void ValidateTphMapping(IEntityType rootEntityType, StoreObjectType storeObjectType)
    {
        var isSproc = storeObjectType is StoreObjectType.DeleteStoredProcedure
            or StoreObjectType.InsertStoredProcedure
            or StoreObjectType.UpdateStoredProcedure;
        var rootSproc = isSproc ? StoredProcedure.FindDeclaredStoredProcedure(rootEntityType, storeObjectType) : null;
        var rootId = StoreObjectIdentifier.Create(rootEntityType, storeObjectType);
        foreach (var entityType in rootEntityType.GetDerivedTypes())
        {
            var entityId = StoreObjectIdentifier.Create(entityType, storeObjectType);
            if (entityId == null)
            {
                continue;
            }

            if (rootId == entityId)
            {
                if (rootSproc != null)
                {
                    var sproc = StoredProcedure.FindDeclaredStoredProcedure(entityType, storeObjectType);
                    if (sproc != null
                        && sproc != rootSproc)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.StoredProcedureTphDuplicate(
                                entityType.DisplayName(), rootEntityType.DisplayName(), rootId?.DisplayName()));
                    }
                }

                continue;
            }

            switch (storeObjectType)
            {
                case StoreObjectType.Table:
                    throw new InvalidOperationException(
                        RelationalStrings.TphTableMismatch(
                            entityType.DisplayName(), entityId.Value.DisplayName(),
                            rootEntityType.DisplayName(), rootId?.DisplayName()));
                case StoreObjectType.View:
                    throw new InvalidOperationException(
                        RelationalStrings.TphViewMismatch(
                            entityType.DisplayName(), entityId.Value.DisplayName(),
                            rootEntityType.DisplayName(), rootId?.DisplayName()));
                case StoreObjectType.Function:
                    throw new InvalidOperationException(
                        RelationalStrings.TphDbFunctionMismatch(
                            entityType.DisplayName(), entityId.Value.DisplayName(),
                            rootEntityType.DisplayName(), rootId?.DisplayName()));
                case StoreObjectType.InsertStoredProcedure:
                case StoreObjectType.DeleteStoredProcedure:
                case StoreObjectType.UpdateStoredProcedure:
                    throw new InvalidOperationException(
                        RelationalStrings.TphStoredProcedureMismatch(
                            entityType.DisplayName(), entityId.Value.DisplayName(),
                            rootEntityType.DisplayName(), rootId?.DisplayName()));
            }
        }
    }

    /// <inheritdoc />
    protected override bool IsRedundant(IForeignKey foreignKey)
        => base.IsRedundant(foreignKey)
            && !foreignKey.DeclaringEntityType.GetMappingFragments().Any();

    /// <summary>
    ///     Validates the entity type mapping fragments.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateMappingFragments(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            var fragments = EntityTypeMappingFragment.Get(entityType);
            if (fragments == null)
            {
                continue;
            }

            if (entityType.BaseType != null
                || entityType.GetDirectlyDerivedTypes().Any())
            {
                throw new InvalidOperationException(
                    RelationalStrings.EntitySplittingHierarchy(entityType.DisplayName(), fragments.First().StoreObject.DisplayName()));
            }

            var anyTableFragments = false;
            var anyViewFragments = false;
            foreach (var fragment in fragments)
            {
                var mainStoreObject = StoreObjectIdentifier.Create(entityType, fragment.StoreObject.StoreObjectType);
                if (mainStoreObject == null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.EntitySplittingUnmappedMainFragment(
                            entityType.DisplayName(), fragment.StoreObject.DisplayName(), fragment.StoreObject.StoreObjectType));
                }

                if (fragment.StoreObject == mainStoreObject)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.EntitySplittingConflictingMainFragment(
                            entityType.DisplayName(), fragment.StoreObject.DisplayName()));
                }

                foreach (var foreignKey in entityType.FindRowInternalForeignKeys(fragment.StoreObject))
                {
                    var principalMainFragment = StoreObjectIdentifier.Create(
                        foreignKey.PrincipalEntityType, fragment.StoreObject.StoreObjectType)!.Value;
                    if (principalMainFragment != mainStoreObject)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.EntitySplittingUnmatchedMainTableSplitting(
                                entityType.DisplayName(),
                                fragment.StoreObject.DisplayName(),
                                foreignKey.PrincipalEntityType.DisplayName(),
                                principalMainFragment.DisplayName()));
                    }
                }

                var propertiesFound = false;
                foreach (var property in entityType.GetProperties())
                {
                    var columnName = property.GetColumnName(fragment.StoreObject);
                    if (columnName == null)
                    {
                        if (property.IsPrimaryKey())
                        {
                            throw new InvalidOperationException(
                                RelationalStrings.EntitySplittingMissingPrimaryKey(
                                    entityType.DisplayName(), fragment.StoreObject.DisplayName()));
                        }

                        continue;
                    }

                    if (!property.IsPrimaryKey())
                    {
                        propertiesFound = true;
                    }
                }

                if (!propertiesFound)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.EntitySplittingMissingProperties(
                            entityType.DisplayName(), fragment.StoreObject.DisplayName()));
                }

                switch (fragment.StoreObject.StoreObjectType)
                {
                    case StoreObjectType.Table:
                        anyTableFragments = true;
                        break;
                    case StoreObjectType.View:
                        anyViewFragments = true;
                        break;
                }
            }

            if (anyTableFragments)
            {
                ValidateMainMapping(entityType, StoreObjectIdentifier.Create(entityType, StoreObjectType.Table)!.Value);
            }

            if (anyViewFragments)
            {
                ValidateMainMapping(entityType, StoreObjectIdentifier.Create(entityType, StoreObjectType.View)!.Value);
            }
        }

        static StoreObjectIdentifier? ValidateMainMapping(IEntityType entityType, StoreObjectIdentifier mainObject)
        {
            var nonSharedRequiredPropertyFound =
                entityType.FindRowInternalForeignKeys(mainObject).All(fk => fk.IsRequiredDependent);

            var propertyFound = false;
            foreach (var property in entityType.GetProperties())
            {
                if (property.IsPrimaryKey())
                {
                    continue;
                }

                var columnName = property.GetColumnName(mainObject);
                if (columnName != null)
                {
                    propertyFound = true;

                    if (!nonSharedRequiredPropertyFound
                        && !property.IsNullable
                        && property.FindSharedStoreObjectRootProperty(mainObject) == null)
                    {
                        nonSharedRequiredPropertyFound = true;
                    }
                }
            }

            if (!propertyFound)
            {
                throw new InvalidOperationException(
                    RelationalStrings.EntitySplittingMissingPropertiesMainFragment(
                        entityType.DisplayName(), mainObject.DisplayName()));
            }

            if (!nonSharedRequiredPropertyFound)
            {
                var rowInternalFk = entityType.FindRowInternalForeignKeys(mainObject).First(fk => !fk.IsRequiredDependent);
                throw new InvalidOperationException(
                    RelationalStrings.EntitySplittingMissingRequiredPropertiesOptionalDependent(
                        entityType.DisplayName(), mainObject.DisplayName(),
                        $".Navigation(p => p.{rowInternalFk.PrincipalToDependent!.Name}).IsRequired()"));
            }

            return mainObject;
        }
    }

    /// <summary>
    ///     Validates the table-specific property overrides.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidatePropertyOverrides(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            foreach (var property in entityType.GetDeclaredProperties())
            {
                var storeObjectOverrides = RelationalPropertyOverrides.Get(property);
                if (storeObjectOverrides == null)
                {
                    continue;
                }

                foreach (var storeObjectOverride in storeObjectOverrides)
                {
                    if (GetAllMappedStoreObjects(property, storeObjectOverride.StoreObject.StoreObjectType)
                        .Any(o => o == storeObjectOverride.StoreObject))
                    {
                        continue;
                    }

                    var storeObject = storeObjectOverride.StoreObject;
                    switch (storeObject.StoreObjectType)
                    {
                        case StoreObjectType.Table:
                            throw new InvalidOperationException(
                                RelationalStrings.TableOverrideMismatch(
                                    entityType.DisplayName() + "." + property.Name,
                                    storeObjectOverride.StoreObject.DisplayName()));
                        case StoreObjectType.View:
                            throw new InvalidOperationException(
                                RelationalStrings.ViewOverrideMismatch(
                                    entityType.DisplayName() + "." + property.Name,
                                    storeObjectOverride.StoreObject.DisplayName()));
                        case StoreObjectType.SqlQuery:
                            throw new InvalidOperationException(
                                RelationalStrings.SqlQueryOverrideMismatch(
                                    entityType.DisplayName() + "." + property.Name,
                                    storeObjectOverride.StoreObject.DisplayName()));
                        case StoreObjectType.Function:
                            throw new InvalidOperationException(
                                RelationalStrings.FunctionOverrideMismatch(
                                    entityType.DisplayName() + "." + property.Name,
                                    storeObjectOverride.StoreObject.DisplayName()));
                        case StoreObjectType.InsertStoredProcedure:
                        case StoreObjectType.DeleteStoredProcedure:
                        case StoreObjectType.UpdateStoredProcedure:
                            throw new InvalidOperationException(
                                RelationalStrings.StoredProcedureOverrideMismatch(
                                    entityType.DisplayName() + "." + property.Name,
                                    storeObjectOverride.StoreObject.DisplayName()));
                        default:
                            throw new NotSupportedException(storeObject.StoreObjectType.ToString());
                    }
                }
            }
        }
    }

    private static IEnumerable<StoreObjectIdentifier> GetAllMappedStoreObjects(
        IReadOnlyProperty property,
        StoreObjectType storeObjectType)
    {
        var mappingStrategy = property.DeclaringType.GetMappingStrategy();
        if (property.IsPrimaryKey())
        {
            var declaringStoreObject = StoreObjectIdentifier.Create(property.DeclaringType, storeObjectType);
            if (declaringStoreObject != null)
            {
                yield return declaringStoreObject.Value;
            }

            if (storeObjectType is StoreObjectType.Function or StoreObjectType.SqlQuery)
            {
                yield break;
            }

            foreach (var fragment in property.DeclaringType.GetMappingFragments(storeObjectType))
            {
                yield return fragment.StoreObject;
            }

            if (property.DeclaringType is IReadOnlyEntityType entityType)
            {
                foreach (var containingType in entityType.GetDerivedTypes())
                {
                    var storeObject = StoreObjectIdentifier.Create(containingType, storeObjectType);
                    if (storeObject != null)
                    {
                        yield return storeObject.Value;

                        if (mappingStrategy == RelationalAnnotationNames.TphMappingStrategy)
                        {
                            yield break;
                        }
                    }
                }
            }
        }
        else
        {
            var declaringStoreObject = StoreObjectIdentifier.Create(property.DeclaringType, storeObjectType);
            if (storeObjectType is StoreObjectType.Function or StoreObjectType.SqlQuery)
            {
                if (declaringStoreObject != null)
                {
                    yield return declaringStoreObject.Value;
                }

                yield break;
            }

            if (declaringStoreObject != null)
            {
                var fragments = property.DeclaringType.GetMappingFragments(storeObjectType).ToList();
                if (fragments.Count > 0)
                {
                    var overrides = RelationalPropertyOverrides.Find(property, declaringStoreObject.Value);
                    if (overrides != null)
                    {
                        yield return declaringStoreObject.Value;
                    }

                    foreach (var fragment in fragments)
                    {
                        overrides = RelationalPropertyOverrides.Find(property, fragment.StoreObject);
                        if (overrides != null)
                        {
                            yield return fragment.StoreObject;
                        }
                    }

                    yield break;
                }

                yield return declaringStoreObject.Value;
                if (mappingStrategy != RelationalAnnotationNames.TpcMappingStrategy)
                {
                    yield break;
                }
            }

            if (property.DeclaringType is not IReadOnlyEntityType entityType)
            {
                yield break;
            }

            var tableFound = false;
            var queue = new Queue<IReadOnlyEntityType>();
            queue.Enqueue(entityType);
            while (queue.Count > 0 && !tableFound)
            {
                foreach (var containingType in queue.Dequeue().GetDirectlyDerivedTypes())
                {
                    var storeObject = StoreObjectIdentifier.Create(containingType, storeObjectType);
                    if (storeObject != null)
                    {
                        yield return storeObject.Value;
                        tableFound = true;
                        if (mappingStrategy == RelationalAnnotationNames.TphMappingStrategy)
                        {
                            yield break;
                        }
                    }

                    if (!tableFound
                        || mappingStrategy == RelationalAnnotationNames.TpcMappingStrategy)
                    {
                        queue.Enqueue(containingType);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Validates that the properties of any one index are all mapped to columns on at least one common table.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateIndexProperties(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            if (entityType.GetTableName() != null)
            {
                continue;
            }

            foreach (var index in entityType.GetDeclaredIndexes())
            {
                if (ConfigurationSource.Convention != ((IConventionIndex)index).GetConfigurationSource())
                {
                    index.GetDatabaseName(StoreObjectIdentifier.Table(""), logger);
                }
            }
        }
    }


    /// <inheritdoc/>
    protected override void ValidateData(IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            if (entityType.IsMappedToJson() && entityType.GetSeedData().Any())
            {
                throw new InvalidOperationException(RelationalStrings.HasDataNotSupportedForEntitiesMappedToJson(entityType.DisplayName()));
            }

            foreach (var navigation in entityType.GetNavigations().Where(x => x.ForeignKey.IsOwnership && x.TargetEntityType.IsMappedToJson()))
            {
                if (entityType.GetSeedData().Any(x => x.TryGetValue(navigation.Name, out var _)))
                {
                    throw new InvalidOperationException(RelationalStrings.HasDataNotSupportedForEntitiesMappedToJson(entityType.DisplayName()));
                }
            }
        }

        base.ValidateData(model, logger);
    }

    /// <summary>
    ///     Validates that the triggers are unambiguously mapped to exactly one table.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected override void ValidateTriggers(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes().Where(e => e.GetDeclaredTriggers().Any()))
        {
            if (entityType.BaseType is not null
                && entityType.GetMappingStrategy() == RelationalAnnotationNames.TphMappingStrategy)
            {
                logger.TriggerOnNonRootTphEntity(entityType);
            }

            var tableName = entityType.GetTableName();
            var tableSchema = entityType.GetSchema();

            foreach (var trigger in entityType.GetDeclaredTriggers())
            {
                if ((trigger.GetTableName() != tableName
                        || trigger.GetTableSchema() != tableSchema)
                    && entityType.GetMappingFragments(StoreObjectType.Table)
                        .All(f => trigger.GetTableName() != f.StoreObject.Name || trigger.GetTableSchema() != f.StoreObject.Schema))
                {
                    throw new InvalidOperationException(
                        RelationalStrings.TriggerWithMismatchedTable(
                            trigger.ModelName,
                            (trigger.GetTableName()!, trigger.GetTableSchema()).FormatTable(),
                            entityType.DisplayName(),
                            entityType.GetSchemaQualifiedTableName())
                    );
                }
            }
        }
    }

    /// <summary>
    ///     Validates the JSON entities.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateJsonEntities(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var tables = BuildSharedTableEntityMap(model.GetEntityTypes());
        foreach (var (table, mappedTypes) in tables)
        {
            if (mappedTypes.All(x => !x.IsMappedToJson()))
            {
                continue;
            }

            foreach (var jsonEntityType in mappedTypes.Where(x => x.IsMappedToJson()))
            {
                var ownership = jsonEntityType.FindOwnership()!;
                var ownerTableOrViewName = ownership.PrincipalEntityType.GetViewName() ?? ownership.PrincipalEntityType.GetTableName();
                if (table.Name != ownerTableOrViewName)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.JsonEntityMappedToDifferentTableOrViewThanOwner(
                            jsonEntityType.DisplayName(), table.Name, ownership.PrincipalEntityType.DisplayName(), ownerTableOrViewName));
                }
            }

            var nonOwnedTypes = mappedTypes.Where(x => !x.IsOwned());
            var nonOwnedTypesCount = nonOwnedTypes.Count();
            if (nonOwnedTypesCount == 0)
            {
                var nonJsonType = mappedTypes.Where(x => !x.IsMappedToJson()).First();

                // must be owned collection (mapped to a separate table) that owns a JSON type
                // issue #28441
                throw new InvalidOperationException(
                    RelationalStrings.JsonEntityOwnedByNonJsonOwnedType(
                        nonJsonType.DisplayName(), table.DisplayName()));
            }

            var distinctRootTypes = nonOwnedTypes.Select(x => x.GetRootType()).Distinct().ToList();
            if (distinctRootTypes.Count > 1)
            {
                // issue #28442
                throw new InvalidOperationException(
                    RelationalStrings.JsonEntityWithTableSplittingIsNotSupported);
            }

            var rootType = distinctRootTypes[0];
            var jsonEntitiesMappedToSameJsonColumn = mappedTypes
                .Where(x => x.FindOwnership() is IForeignKey ownership && !ownership.PrincipalEntityType.IsOwned())
                .GroupBy(x => x.GetContainerColumnName())
                .Where(x => x.Key is not null)
                .Select(g => new { g.Key, Count = g.Count() })
                .Where(x => x.Count > 1)
                .Select(x => x.Key);

            if (jsonEntitiesMappedToSameJsonColumn.FirstOrDefault() is string jsonEntityMappedToSameJsonColumn)
            {
                // issue #28584
                throw new InvalidOperationException(
                    RelationalStrings.JsonEntityMultipleRootsMappedToTheSameJsonColumn(
                        jsonEntityMappedToSameJsonColumn, table.Name));
            }

            ValidateJsonEntityRoot(table, rootType);

            foreach (var jsonEntityType in mappedTypes.Where(x => x.IsMappedToJson()))
            {
                ValidateJsonEntityNavigations(table, jsonEntityType);
                ValidateJsonEntityKey(table, jsonEntityType);
                ValidateJsonEntityProperties(table, jsonEntityType);
            }
        }

        // TODO: support this for raw SQL and function mappings in #19970 and #21627 and remove the check
        ValidateJsonEntitiesNotMappedToTableOrView(model.GetEntityTypes());
        ValidateJsonViews(model.GetEntityTypes().Where(t => t.IsMappedToJson()));
    }

    private void ValidateJsonEntitiesNotMappedToTableOrView(IEnumerable<IEntityType> entityTypes)
    {
        var entitiesNotMappedToTableOrView = entityTypes.Where(
            x => !x.IsMappedToJson()
                && x.GetSchemaQualifiedTableName() == null
                && x.GetSchemaQualifiedViewName() == null);

        foreach (var entityNotMappedToTableOrView in entitiesNotMappedToTableOrView)
        {
            if (entityNotMappedToTableOrView.GetDeclaredNavigations()
                .Any(x => x.ForeignKey.IsOwnership && x.TargetEntityType.IsMappedToJson()))
            {
                throw new InvalidOperationException(
                    RelationalStrings.JsonEntityWithOwnerNotMappedToTableOrView(
                        entityNotMappedToTableOrView.DisplayName()));
            }
        }
    }

    private void ValidateJsonViews(IEnumerable<IEntityType> entityTypes)
    {
        foreach (var jsonEntityType in entityTypes)
        {
            var viewName = jsonEntityType.GetViewName();
            if (viewName == null)
            {
                continue;
            }

            var ownership = jsonEntityType.FindOwnership()!;
            var ownerTableOrViewName = ownership.PrincipalEntityType.GetViewName() ?? ownership.PrincipalEntityType.GetTableName();
            if (viewName != ownerTableOrViewName)
            {
                throw new InvalidOperationException(
                    RelationalStrings.JsonEntityMappedToDifferentTableOrViewThanOwner(
                        jsonEntityType.DisplayName(), viewName, ownership.PrincipalEntityType.DisplayName(), ownerTableOrViewName));
            }
        }
    }

    /// <summary>
    ///     Validates the root entity mapped to a JSON column.
    /// </summary>
    /// <param name="storeObject">The store object.</param>
    /// <param name="rootType">The entity type to validate.</param>
    protected virtual void ValidateJsonEntityRoot(
        in StoreObjectIdentifier storeObject,
        IEntityType rootType)
    {
        var mappingStrategy = rootType.GetMappingStrategy();
        if (mappingStrategy != null && mappingStrategy != RelationalAnnotationNames.TphMappingStrategy)
        {
            // issue #28443
            throw new InvalidOperationException(
                RelationalStrings.JsonEntityWithNonTphInheritanceOnOwner(rootType.DisplayName()));
        }
    }

    /// <summary>
    ///     Validates navigations of the entity mapped to a JSON column.
    /// </summary>
    /// <param name="storeObject">The store object.</param>
    /// <param name="jsonEntityType">The entity type to validate.</param>
    protected virtual void ValidateJsonEntityNavigations(
        in StoreObjectIdentifier storeObject,
        IEntityType jsonEntityType)
    {
        var ownership = jsonEntityType.FindOwnership()!;

        if (ownership.PrincipalEntityType.IsOwned()
            && !ownership.PrincipalEntityType.IsMappedToJson())
        {
            // issue #28441
            throw new InvalidOperationException(
                RelationalStrings.JsonEntityOwnedByNonJsonOwnedType(
                    ownership.PrincipalEntityType.DisplayName(),
                    storeObject.DisplayName()));
        }

        foreach (var navigation in jsonEntityType.GetDeclaredNavigations())
        {
            if (!navigation.ForeignKey.IsOwnership)
            {
                throw new InvalidOperationException(
                    RelationalStrings.JsonEntityReferencingRegularEntity(
                        jsonEntityType.DisplayName()));
            }
        }
    }

    /// <summary>
    ///     Validate the key of entity mapped to a JSON column.
    /// </summary>
    /// <param name="storeObject">The store object.</param>
    /// <param name="jsonEntityType">The entity type containing the key to validate.</param>
    protected virtual void ValidateJsonEntityKey(
        in StoreObjectIdentifier storeObject,
        IEntityType jsonEntityType)
    {
        var primaryKeyProperties = jsonEntityType.FindPrimaryKey()!.Properties;
        var ownership = jsonEntityType.FindOwnership()!;

        foreach (var primaryKeyProperty in primaryKeyProperties)
        {
            if (primaryKeyProperty.GetJsonPropertyName() != null)
            {
                // issue #28594
                throw new InvalidOperationException(
                    RelationalStrings.JsonEntityWithExplicitlyConfiguredJsonPropertyNameOnKey(
                        primaryKeyProperty.Name, jsonEntityType.DisplayName()));
            }
        }

        if (!ownership.IsUnique)
        {
            // for collection entities, make sure that ordinal key is not explicitly defined
            var ordinalKeyProperty = primaryKeyProperties.Last();
            if (!ordinalKeyProperty.IsOrdinalKeyProperty())
            {
                // issue #28594
                throw new InvalidOperationException(
                    RelationalStrings.JsonEntityWithExplicitlyConfiguredOrdinalKey(
                        jsonEntityType.DisplayName()));
            }
        }

        var ownerEntityTypeKeyPropertiesCount = ownership.PrincipalEntityType.FindPrimaryKey()!.Properties.Count;
        var expectedKeyCount = ownership.IsUnique
            ? ownerEntityTypeKeyPropertiesCount
            : ownerEntityTypeKeyPropertiesCount + 1;

        if (primaryKeyProperties.Count != expectedKeyCount)
        {
            // issue #28594
            throw new InvalidOperationException(
                RelationalStrings.JsonEntityWithIncorrectNumberOfKeyProperties(
                    jsonEntityType.DisplayName(), expectedKeyCount, primaryKeyProperties.Count));
        }
    }

    /// <summary>
    ///     Validate the properties of entity mapped to a JSON column.
    /// </summary>
    /// <param name="storeObject">The store object.</param>
    /// <param name="jsonEntityType">The entity type containing the properties to validate.</param>
    protected virtual void ValidateJsonEntityProperties(
        in StoreObjectIdentifier storeObject,
        IEntityType jsonEntityType)
    {
        var jsonPropertyNames = new List<string>();
        foreach (var property in jsonEntityType.GetDeclaredProperties().Where(p => !string.IsNullOrEmpty(p.GetJsonPropertyName())))
        {
            if (property.TryGetDefaultValue(out var _))
            {
                throw new InvalidOperationException(
                    RelationalStrings.JsonEntityWithDefaultValueSetOnItsProperty(
                        jsonEntityType.DisplayName(), property.Name));
            }

            var jsonPropertyName = property.GetJsonPropertyName()!;
            if (!jsonPropertyNames.Contains(jsonPropertyName))
            {
                jsonPropertyNames.Add(jsonPropertyName);
            }
            else
            {
                throw new InvalidOperationException(
                    RelationalStrings.JsonEntityWithMultiplePropertiesMappedToSameJsonProperty(
                        jsonEntityType.DisplayName(), jsonPropertyName));
            }
        }

        foreach (var navigation in jsonEntityType.GetDeclaredNavigations())
        {
            var jsonPropertyName = navigation.TargetEntityType.GetJsonPropertyName()!;
            if (!jsonPropertyNames.Contains(jsonPropertyName))
            {
                jsonPropertyNames.Add(jsonPropertyName);
            }
            else
            {
                throw new InvalidOperationException(
                    RelationalStrings.JsonEntityWithMultiplePropertiesMappedToSameJsonProperty(
                        jsonEntityType.DisplayName(), jsonPropertyName));
            }
        }
    }

    /// <summary>
    ///     Throws an <see cref="InvalidOperationException" /> with a message containing provider-specific information, when
    ///     available, indicating possible reasons why the property cannot be mapped.
    /// </summary>
    /// <param name="propertyType">The property CLR type.</param>
    /// <param name="typeBase">The structural type.</param>
    /// <param name="unmappedProperty">The property.</param>
    protected override void ThrowPropertyNotMappedException(
        string propertyType,
        IConventionTypeBase typeBase,
        IConventionProperty unmappedProperty)
    {
        var storeType = unmappedProperty.GetColumnType();
        if (storeType != null)
        {
            throw new InvalidOperationException(
                RelationalStrings.PropertyNotMapped(
                    propertyType,
                    typeBase.DisplayName(),
                    unmappedProperty.Name,
                    storeType));
        }

        base.ThrowPropertyNotMappedException(propertyType, typeBase, unmappedProperty);
    }
}
