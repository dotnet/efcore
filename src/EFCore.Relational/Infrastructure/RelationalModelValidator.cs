// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     The validator that enforces rules common for all relational providers.
/// </summary>
/// <param name="dependencies">Parameter object containing dependencies for this service.</param>
/// <param name="relationalDependencies">Parameter object containing relational dependencies for this service.</param>
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
public class RelationalModelValidator(
    ModelValidatorDependencies dependencies,
    RelationalModelValidatorDependencies relationalDependencies)
    : ModelValidator(dependencies)
{

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalModelValidatorDependencies RelationalDependencies { get; } = relationalDependencies;

    /// <summary>
    ///     Validates a model, throwing an exception if any errors are found.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="logger">The logger to use.</param>
    public override void Validate(IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.Validate(model, logger);

        var tables = new Dictionary<StoreObjectIdentifier, List<IEntityType>>();
        var views = new Dictionary<StoreObjectIdentifier, List<IEntityType>>();
        var storedProcedures = new Dictionary<StoreObjectIdentifier, List<IEntityType>>();
        foreach (var entityType in model.GetEntityTypes())
        {
            var tableId = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table);
            if (tableId != null)
            {
                var table = tableId.Value;
                if (!tables.TryGetValue(table, out var tableMappedTypes))
                {
                    tableMappedTypes = [];
                    tables[table] = tableMappedTypes;
                }

                tableMappedTypes.Add(entityType);
            }

            var viewId = StoreObjectIdentifier.Create(entityType, StoreObjectType.View);
            if (viewId != null)
            {
                var view = viewId.Value;
                if (!views.TryGetValue(view, out var viewMappedTypes))
                {
                    viewMappedTypes = [];
                    views[view] = viewMappedTypes;
                }

                viewMappedTypes.Add(entityType);
            }

            AddStoredProcedure(StoreObjectType.DeleteStoredProcedure, entityType, storedProcedures);
            AddStoredProcedure(StoreObjectType.InsertStoredProcedure, entityType, storedProcedures);
            AddStoredProcedure(StoreObjectType.UpdateStoredProcedure, entityType, storedProcedures);
        }

        foreach (var (table, mappedTypes) in tables)
        {
            ValidateTable(mappedTypes, table, logger);
        }

        foreach (var (view, mappedTypes) in views)
        {
            ValidateView(mappedTypes, view, logger);
        }

        foreach (var dbFunction in model.GetDbFunctions())
        {
            ValidateDbFunction(dbFunction, logger);
        }

        foreach (var sequence in model.GetSequences())
        {
            ValidateSequence(sequence, logger);
        }

        foreach (var (sproc, mappedTypes) in storedProcedures)
        {
            ValidateStoredProcedure(mappedTypes, sproc, logger);
        }

        static void AddStoredProcedure(
            StoreObjectType storedProcedureType,
            IEntityType entityType,
            Dictionary<StoreObjectIdentifier, List<IEntityType>> storedProcedures)
        {
            var sprocId = StoreObjectIdentifier.Create(entityType, storedProcedureType);
            if (sprocId == null)
            {
                return;
            }

            if (!storedProcedures.TryGetValue(sprocId.Value, out var mappedTypes))
            {
                mappedTypes = [];
                storedProcedures[sprocId.Value] = mappedTypes;
            }

            mappedTypes.Add(entityType);
        }
    }

    /// <inheritdoc />
    protected override void ValidateEntityType(
        IEntityType entityType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.ValidateEntityType(entityType, logger);

        ValidateMappingFragment(entityType, logger);
        ValidateSqlQuery(entityType, logger);
        ValidateDbFunctionMapping(entityType, logger);
        ValidateStoredProcedures(entityType, logger);
        ValidateContainerColumnType(entityType, logger);
        ValidateTphTriggers(entityType, logger);
        // TODO: support this for raw SQL and function mappings in #19970 and #21627 and remove the check
        ValidateJsonEntityOwnerMappedToTableOrView(entityType, logger);
    }

    /// <summary>
    ///     Logs a warning if triggers are defined on a non-root entity type using TPH mapping strategy.
    /// </summary>
    /// <param name="entityType">The entity type to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateTphTriggers(
        IEntityType entityType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (entityType.GetDeclaredTriggers().Any()
            && entityType.BaseType is not null
            && entityType.GetMappingStrategy() == RelationalAnnotationNames.TphMappingStrategy)
        {
            logger.TriggerOnNonRootTphEntity(entityType);
        }
    }

    /// <inheritdoc />
    protected override void ValidateProperty(
        IProperty property,
        ITypeBase structuralType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.ValidateProperty(property, structuralType, logger);

        if (RelationalPropertyOverrides.Get(property) is { } storeObjectOverrides)
        {
            foreach (var storeObjectOverride in storeObjectOverrides)
            {
                ValidatePropertyOverride(property, storeObjectOverride, logger);
            }
        }

        ValidateBoolWithDefaults(property, logger);
    }

    /// <inheritdoc />
    protected override void ValidateKey(
        IKey key,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.ValidateKey(key, logger);

        ValidateDefaultValuesOnKey(key, logger);
        ValidateValueGeneration(key, logger);
    }

    /// <summary>
    ///     Validates a primitive collection property.
    /// </summary>
    /// <param name="property">The property to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected override void ValidatePrimitiveCollection(
        IProperty property,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.ValidatePrimitiveCollection(property, logger);

        if (property is { IsPrimitiveCollection: true }
            && property.GetTypeMapping().ElementTypeMapping?.ElementTypeMapping != null)
        {
            throw new InvalidOperationException(
                RelationalStrings.NestedCollectionsNotSupported(
                    property.ClrType.ShortDisplayName(), property.DeclaringType.DisplayName(), property.Name));
        }
    }

    /// <inheritdoc />
    protected override void ValidatePropertyMapping(
        IComplexProperty complexProperty,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.ValidatePropertyMapping(complexProperty, logger);

        if (complexProperty.IsCollection && !complexProperty.ComplexType.IsMappedToJson())
        {
            throw new InvalidOperationException(
                RelationalStrings.ComplexCollectionNotMappedToJson(
                    complexProperty.DeclaringType.DisplayName(), complexProperty.Name));
        }

        if (!complexProperty.ComplexType.IsMappedToJson()
            && complexProperty.IsNullable
            && complexProperty.ComplexType.GetProperties().All(m => m.IsNullable))
        {
            throw new InvalidOperationException(
                RelationalStrings.ComplexPropertyOptionalTableSharing(complexProperty.ComplexType.DisplayName(), complexProperty.Name));
        }

        if (complexProperty.GetJsonPropertyName() != null)
        {
            if (complexProperty.ComplexType.FindAnnotation(RelationalAnnotationNames.ContainerColumnName)?.Value is string columnName)
            {
                throw new InvalidOperationException(
                    RelationalStrings.ComplexPropertyBothJsonColumnAndJsonPropertyName(
                        $"{complexProperty.DeclaringType.DisplayName()}.{complexProperty.Name}",
                        columnName,
                        complexProperty.GetJsonPropertyName()));
            }

            if (!complexProperty.DeclaringType.IsMappedToJson())
            {
                throw new InvalidOperationException(
                    RelationalStrings.ComplexPropertyJsonPropertyNameWithoutJsonMapping(
                        $"{complexProperty.DeclaringType.DisplayName()}.{complexProperty.Name}"));
            }
        }

        if (complexProperty.ComplexType.IsMappedToJson())
        {
            if (!complexProperty.DeclaringType.IsMappedToJson()
                && complexProperty.DeclaringType is IComplexType)
            {
                // Issue #36558
                throw new InvalidOperationException(
                    RelationalStrings.NestedComplexPropertyJsonWithTableSharing(
                        $"{complexProperty.DeclaringType.DisplayName()}.{complexProperty.Name}",
                        complexProperty.DeclaringType.DisplayName()));
            }

            ValidateJsonProperties(complexProperty.ComplexType);
        }
    }

    /// <summary>
    ///     Validates the SQL query mapping for an entity type.
    /// </summary>
    /// <param name="entityType">The entity type to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSqlQuery(
        IEntityType entityType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var sqlQuery = entityType.GetSqlQuery();
        if (sqlQuery == null)
        {
            return;
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

    /// <summary>
    ///     Validates a single sequence.
    /// </summary>
    /// <param name="sequence">The sequence to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSequence(
        ISequence sequence,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
    }

    /// <summary>
    ///     Validates a single database function.
    /// </summary>
    /// <param name="dbFunction">The database function to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateDbFunction(
        IDbFunction dbFunction,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var model = dbFunction.Model;
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

    /// <summary>
    ///     Validates the function mapping for an entity type.
    /// </summary>
    /// <param name="entityType">The entity type to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateDbFunctionMapping(
        IEntityType entityType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var mappedFunctionName = entityType.GetFunctionName();
        if (mappedFunctionName == null)
        {
            return;
        }

        var mappedFunction = entityType.Model.FindDbFunction(mappedFunctionName);
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

    /// <summary>
    ///     Validates the stored procedures for a entity type.
    /// </summary>
    /// <param name="entityType">The entity type to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateStoredProcedures(
        IEntityType entityType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var mappingStrategy = entityType.GetMappingStrategy() ?? RelationalAnnotationNames.TphMappingStrategy;

        var sprocCount = 0;
        var deleteStoredProcedure = entityType.GetDeleteStoredProcedure();
        if (deleteStoredProcedure != null)
        {
            ValidateStoredProcedureName(StoreObjectType.DeleteStoredProcedure, entityType);
            ValidateSproc(deleteStoredProcedure, mappingStrategy, logger);
            sprocCount++;
        }

        var insertStoredProcedure = entityType.GetInsertStoredProcedure();
        if (insertStoredProcedure != null)
        {
            ValidateStoredProcedureName(StoreObjectType.InsertStoredProcedure, entityType);
            ValidateSproc(insertStoredProcedure, mappingStrategy, logger);
            sprocCount++;
        }

        var updateStoredProcedure = entityType.GetUpdateStoredProcedure();
        if (updateStoredProcedure != null)
        {
            ValidateStoredProcedureName(StoreObjectType.UpdateStoredProcedure, entityType);
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

        static void ValidateStoredProcedureName(
            StoreObjectType storedProcedureType,
            IEntityType entityType)
        {
            var sprocId = StoreObjectIdentifier.Create(entityType, storedProcedureType);
            if (sprocId == null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.StoredProcedureNoName(
                        entityType.DisplayName(), storedProcedureType));
            }
        }
    }

    /// <summary>
    ///     Validates a single stored procedure and all entity types mapped to it.
    /// </summary>
    /// <param name="mappedTypes">The entity types mapped to the stored procedure.</param>
    /// <param name="storedProcedure">The stored procedure identifier.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateStoredProcedure(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier storedProcedure,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        ValidateStoredProcedureCompatibility(mappedTypes, storedProcedure, logger);
    }

    /// <summary>
    ///     Validates that a stored procedure is not shared across unrelated entity types.
    /// </summary>
    /// <param name="mappedTypes">The entity types mapped to the stored procedure.</param>
    /// <param name="storedProcedure">The stored procedure identifier.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateStoredProcedureCompatibility(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier storedProcedure,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var mappedType in mappedTypes)
        {
            if (mappedTypes[0].GetRootType() != mappedType.GetRootType())
            {
                throw new InvalidOperationException(
                    RelationalStrings.StoredProcedureTableSharing(
                        mappedTypes[0].DisplayName(),
                        mappedType.DisplayName(),
                        storedProcedure.DisplayName()));
            }
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
            IProperty? property = null;
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
                        && !storeGeneratedProperties.Remove(property!.Name))
                    {
                        if (sproc.Parameters.Any(p => p.PropertyName == property.Name
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
                    if (!property!.IsPrimaryKey()
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
    ///     Validates a <see cref="bool" /> property with defaults.
    /// </summary>
    /// <param name="property">The property to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateBoolWithDefaults(
        IProperty property,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (!property.ClrType.IsNullableType()
            && (property.ClrType.IsEnum || property.ClrType == typeof(bool))
            && property.ValueGenerated != ValueGenerated.Never
            && property.FieldInfo?.FieldType.IsNullableType() != true
            && !((IConventionProperty)property).GetSentinelConfigurationSource().HasValue
            && StoreObjectIdentifier.Create(property.DeclaringType, StoreObjectType.Table) is { } table
                && (IsNotNullAndNotDefault(property.GetDefaultValue(table))
                    || property.GetDefaultValueSql(table) != null))
        {
            logger.BoolWithDefaultWarning(property);
        }

        bool IsNotNullAndNotDefault(object? value)
            => value != null
#pragma warning disable EF1001 // Internal EF Core API usage.
                && !property.ClrType.IsDefaultValue(value);
#pragma warning restore EF1001 // Internal EF Core API usage.
    }

    /// <summary>
    ///     Validates default values on a key.
    /// </summary>
    /// <param name="key">The key to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateDefaultValuesOnKey(
        IKey key,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
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

    /// <summary>
    ///     Validates that a key doesn't have mutable properties.
    /// </summary>
    /// <param name="key">The key to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected override void ValidateMutableKey(
        IKey key,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var mutableProperty = key.Properties.FirstOrDefault(p => p.ValueGenerated.HasFlag(ValueGenerated.OnUpdate));
        if (mutableProperty != null
            && !mutableProperty.IsOrdinalKeyProperty())
        {
            throw new InvalidOperationException(CoreStrings.MutableKeyProperty(mutableProperty.Name));
        }
    }

    /// <summary>
    ///     Validates a single table and all entity types mapped to it.
    /// </summary>
    /// <param name="mappedTypes">The entity types mapped to the table.</param>
    /// <param name="table">The table identifier.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateTable(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier table,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var nonJsonMappedTypes = mappedTypes.Where(e => !e.IsMappedToJson()).ToList();
        if (nonJsonMappedTypes.Count > 0)
        {
            ValidateSharedTableCompatibility(nonJsonMappedTypes, table, logger);
            ValidateSharedColumnsCompatibility(nonJsonMappedTypes, table, logger);
            ValidateSharedKeysCompatibility(nonJsonMappedTypes, table, logger);
            ValidateSharedForeignKeysCompatibility(nonJsonMappedTypes, table, logger);
            ValidateSharedIndexesCompatibility(nonJsonMappedTypes, table, logger);
            ValidateSharedCheckConstraintCompatibility(nonJsonMappedTypes, table, logger);
            ValidateSharedTriggerCompatibility(nonJsonMappedTypes, table, logger);
            ValidateOptionalDependents(nonJsonMappedTypes, table, logger);
        }

        if (mappedTypes.Count != nonJsonMappedTypes.Count)
        {
            ValidateJsonTable(mappedTypes, table, logger);
        }
    }

    /// <summary>
    ///     Validates that optional dependents have an identifying non-nullable property.
    /// </summary>
    /// <param name="mappedTypes">The mapped entity types.</param>
    /// <param name="table">The table identifier.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateOptionalDependents(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier table,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (mappedTypes.Count == 1)
        {
            return;
        }

        var tableIdentifier = table;
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
                .Select(e => e.GetColumnName(tableIdentifier))
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

                var columnName = property.GetColumnName(tableIdentifier);
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

    /// <summary>
    ///     Validates the compatibility of entity types sharing a given table.
    /// </summary>
    /// <param name="mappedTypes">The mapped entity types.</param>
    /// <param name="table">The table identifier.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSharedTableCompatibility(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier table,
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
                    .FirstOrDefault(fk => fk.PrincipalKey.IsPrimaryKey()
                        && !fk.PrincipalEntityType.IsAssignableFrom(fk.DeclaringEntityType)
                        && unvalidatedTypes.Contains(fk.PrincipalEntityType)) is { } linkingFK))
            {
                if (mappedType.BaseType != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.IncompatibleTableDerivedRelationship(
                            table.DisplayName(),
                            mappedType.DisplayName(),
                            linkingFK.PrincipalEntityType.DisplayName()));
                }

                continue;
            }

            if (root != null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.IncompatibleTableNoRelationship(
                        table.DisplayName(),
                        mappedType.DisplayName(),
                        root.DisplayName()));
            }

            root = mappedType;
        }

        Check.DebugAssert(root != null);
        unvalidatedTypes.Remove(root);
        var typesToValidate = new Queue<IEntityType>();
        typesToValidate.Enqueue(root);

        while (typesToValidate.Count > 0)
        {
            var entityType = typesToValidate.Dequeue();
            var key = entityType.FindPrimaryKey();
            var comment = entityType.GetComment();
            var isExcluded = entityType.IsTableExcludedFromMigrations(table);
            var typesToValidateLeft = typesToValidate.Count;
            var directlyConnectedTypes = unvalidatedTypes.Where(unvalidatedType =>
                entityType.IsAssignableFrom(unvalidatedType)
                || IsIdentifyingPrincipal(unvalidatedType, entityType));

            foreach (var nextEntityType in directlyConnectedTypes)
            {
                if (key != null)
                {
                    var otherKey = nextEntityType.FindPrimaryKey()!;
                    if (key.GetName(table) != otherKey.GetName(table))
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.IncompatibleTableKeyNameMismatch(
                                table.DisplayName(),
                                entityType.DisplayName(),
                                nextEntityType.DisplayName(),
                                key.GetName(table),
                                key.Properties.Format(),
                                otherKey.GetName(table),
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
                                table.DisplayName(),
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

                if (isExcluded.Equals(!nextEntityType.IsTableExcludedFromMigrations(table)))
                {
                    throw new InvalidOperationException(
                        RelationalStrings.IncompatibleTableExcludedMismatch(
                            table.DisplayName(),
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
                    table.DisplayName(),
                    invalidEntityType.DisplayName(),
                    root.DisplayName()));
        }
    }

    /// <summary>
    ///     Validates a single view and all entity types mapped to it.
    /// </summary>
    /// <param name="mappedTypes">The entity types mapped to the view.</param>
    /// <param name="view">The view identifier.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateView(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier view,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var nonJsonMappedTypes = mappedTypes.Where(e => !e.IsMappedToJson()).ToList();
        if (nonJsonMappedTypes.Count > 0)
        {
            ValidateSharedViewCompatibility(nonJsonMappedTypes, view.Name, view.Schema, logger);
            ValidateSharedColumnsCompatibility(nonJsonMappedTypes, view, logger);
        }

        if (mappedTypes.Count != nonJsonMappedTypes.Count)
        {
            ValidateJsonView(mappedTypes, view, logger);
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
                    .Any(fk => fk.PrincipalKey.IsPrimaryKey()
                        && unvalidatedTypes.Contains(fk.PrincipalEntityType)))
            {
                if (mappedType.BaseType != null)
                {
                    var principalType = mappedType.FindForeignKeys(mappedType.FindPrimaryKey()!.Properties)
                        .First(fk => fk.PrincipalKey.IsPrimaryKey()
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

        Check.DebugAssert(root != null);
        unvalidatedTypes.Remove(root);
        var typesToValidate = new Queue<IEntityType>();
        typesToValidate.Enqueue(root);

        while (typesToValidate.Count > 0)
        {
            var entityType = typesToValidate.Dequeue();
            var typesToValidateLeft = typesToValidate.Count;
            var directlyConnectedTypes = unvalidatedTypes.Where(unvalidatedType =>
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
            .Any(fk => fk.PrincipalKey.IsPrimaryKey()
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
                foreach (var (concurrencyColumn, concurrencyProperties) in concurrencyColumns!)
                {
                    if (TableSharingConcurrencyTokenConvention.IsConcurrencyTokenMissing(concurrencyProperties, entityType, mappedTypes))
                    {
                        missingConcurrencyTokens.Add(concurrencyColumn);
                    }
                }
            }

            ValidateCompatible(entityType, storeObject, propertyMappings, missingConcurrencyTokens, logger);

            if (missingConcurrencyTokens != null)
            {
                foreach (var concurrencyColumn in missingConcurrencyTokens)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.MissingConcurrencyColumn(
                            entityType.DisplayName(), concurrencyColumn, storeObject.DisplayName()));
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

        return;

        void ValidateCompatible(
            ITypeBase structuralType,
            in StoreObjectIdentifier storeObject,
            Dictionary<string, IProperty> propertyMappings,
            HashSet<string>? missingConcurrencyTokens,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        {
            foreach (var property in structuralType.GetDeclaredProperties())
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

                this.ValidateCompatible(property, duplicateProperty, columnName, storeObject, logger);
            }

            foreach (var complexProperty in structuralType.GetDeclaredComplexProperties())
            {
                ValidateCompatible(complexProperty.ComplexType, storeObject, propertyMappings, missingConcurrencyTokens, logger);
            }
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
        // NB: Properties can have different nullability, the resulting column will be non-nullable if any of the properties is non-nullable

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
    /// <param name="table">The table identifier.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSharedForeignKeysCompatibility(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier table,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (table.StoreObjectType != StoreObjectType.Table)
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

            var foreignKeyName = foreignKey.GetConstraintName(table, principalTable.Value, logger);
            if (foreignKeyName == null)
            {
                continue;
            }

            if (!foreignKeyMappings.TryGetValue(foreignKeyName, out var duplicateForeignKey))
            {
                foreignKeyMappings[foreignKeyName] = foreignKey;
                continue;
            }

            ValidateCompatible(foreignKey, duplicateForeignKey, foreignKeyName, table, logger);
        }
    }

    /// <summary>
    ///     Validates the compatibility of two foreign keys mapped to the same foreign key constraint.
    /// </summary>
    /// <param name="foreignKey">A foreign key.</param>
    /// <param name="duplicateForeignKey">Another foreign key.</param>
    /// <param name="foreignKeyName">The foreign key constraint name.</param>
    /// <param name="table">The table identifier.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateCompatible(
        IForeignKey foreignKey,
        IForeignKey duplicateForeignKey,
        string foreignKeyName,
        in StoreObjectIdentifier table,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        => foreignKey.AreCompatible(duplicateForeignKey, table, shouldThrow: true);

    /// <summary>
    ///     Validates the compatibility of indexes in a given shared table.
    /// </summary>
    /// <param name="mappedTypes">The mapped entity types.</param>
    /// <param name="table">The table identifier.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSharedIndexesCompatibility(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier table,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var indexMappings = new Dictionary<string, IIndex>();
        foreach (var index in mappedTypes.SelectMany(et => et.GetDeclaredIndexes()))
        {
            var indexName = index.GetDatabaseName(table);
            if (indexName == null)
            {
                ValidateIndexPropertyMapping(index, logger);
                continue;
            }

            if (!indexMappings.TryGetValue(indexName, out var duplicateIndex))
            {
                indexMappings[indexName] = index;
                continue;
            }

            ValidateCompatible(index, duplicateIndex, indexName, table, logger);
        }
    }

    /// <summary>
    ///     Validates the compatibility of two indexes mapped to the same table index.
    /// </summary>
    /// <param name="index">An index.</param>
    /// <param name="duplicateIndex">Another index.</param>
    /// <param name="indexName">The name of the index.</param>
    /// <param name="table">The table identifier.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateCompatible(
        IIndex index,
        IIndex duplicateIndex,
        string indexName,
        in StoreObjectIdentifier table,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        => index.AreCompatible(duplicateIndex, table, shouldThrow: true);

    /// <summary>
    ///     Validates the compatibility of primary and alternate keys in a given shared table.
    /// </summary>
    /// <param name="mappedTypes">The mapped entity types.</param>
    /// <param name="table">The table identifier.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSharedKeysCompatibility(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier table,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var keyMappings = new Dictionary<string, IKey>();
        foreach (var key in mappedTypes.SelectMany(et => et.GetDeclaredKeys()))
        {
            var keyName = key.GetName(table, logger);
            if (keyName == null)
            {
                continue;
            }

            if (!keyMappings.TryGetValue(keyName, out var duplicateKey))
            {
                keyMappings[keyName] = key;
                continue;
            }

            ValidateCompatible(key, duplicateKey, keyName, table, logger);
        }
    }

    /// <summary>
    ///     Validates the compatibility of two keys mapped to the same unique constraint.
    /// </summary>
    /// <param name="key">A key.</param>
    /// <param name="duplicateKey">Another key.</param>
    /// <param name="keyName">The name of the unique constraint.</param>
    /// <param name="table">The table identifier.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateCompatible(
        IKey key,
        IKey duplicateKey,
        string keyName,
        in StoreObjectIdentifier table,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        => key.AreCompatible(duplicateKey, table, shouldThrow: true);

    /// <summary>
    ///     Validates the compatibility of check constraints in a given shared table.
    /// </summary>
    /// <param name="mappedTypes">The mapped entity types.</param>
    /// <param name="table">The table identifier.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSharedCheckConstraintCompatibility(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier table,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var checkConstraintMappings = new Dictionary<string, ICheckConstraint>();
        foreach (var checkConstraint in mappedTypes.SelectMany(et => et.GetDeclaredCheckConstraints()))
        {
            var checkConstraintName = checkConstraint.GetName(table);
            if (checkConstraintName == null)
            {
                continue;
            }

            if (!checkConstraintMappings.TryGetValue(checkConstraintName, out var duplicateCheckConstraint))
            {
                checkConstraintMappings[checkConstraintName] = checkConstraint;
                continue;
            }

            ValidateCompatible(checkConstraint, duplicateCheckConstraint, checkConstraintName, table, logger);
        }
    }

    /// <summary>
    ///     Validates the compatibility of two check constraints with the same name.
    /// </summary>
    /// <param name="checkConstraint">A check constraint.</param>
    /// <param name="duplicateCheckConstraint">Another check constraint.</param>
    /// <param name="indexName">The name of the check constraint.</param>
    /// <param name="table">The table identifier.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateCompatible(
        ICheckConstraint checkConstraint,
        ICheckConstraint duplicateCheckConstraint,
        string indexName,
        in StoreObjectIdentifier table,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
        => CheckConstraint.AreCompatible(checkConstraint, duplicateCheckConstraint, table, shouldThrow: true);

    /// <summary>
    ///     Validates the compatibility of triggers in a given shared table.
    /// </summary>
    /// <param name="mappedTypes">The mapped entity types.</param>
    /// <param name="table">The table identifier.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateSharedTriggerCompatibility(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier table,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var triggerMappings = new Dictionary<string, ITrigger>();
        foreach (var trigger in mappedTypes.SelectMany(et => et.GetDeclaredTriggers()))
        {
            var triggerName = trigger.GetDatabaseName(table);
            if (triggerName == null)
            {
                continue;
            }

            if (!triggerMappings.TryGetValue(triggerName, out var duplicateTrigger))
            {
                triggerMappings[triggerName] = trigger;
                continue;
            }

            ValidateCompatible(trigger, duplicateTrigger, triggerName, table, logger);
        }
    }

    /// <summary>
    ///     Validates the compatibility of two trigger with the same name.
    /// </summary>
    /// <param name="trigger">A trigger.</param>
    /// <param name="duplicateTrigger">Another trigger.</param>
    /// <param name="indexName">The name of the trigger.</param>
    /// <param name="table">The table identifier.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateCompatible(
        ITrigger trigger,
        ITrigger duplicateTrigger,
        string indexName,
        in StoreObjectIdentifier table,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
    }

    /// <summary>
    ///     Validates inheritance mapping for an entity type.
    /// </summary>
    /// <param name="entityType">The entity type to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected override void ValidateInheritanceMapping(
        IEntityType entityType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var mappingStrategy = (string?)entityType[RelationalAnnotationNames.MappingStrategy];
        if (mappingStrategy != null)
        {
            ValidateMappingStrategy(entityType, mappingStrategy);
            var storeObjectName = entityType.GetSchemaQualifiedTableName()
                ?? entityType.GetSchemaQualifiedViewName()
                ?? entityType.GetFunctionName()
                ?? entityType.GetSqlQuery()
                ?? entityType.GetInsertStoredProcedure()?.GetSchemaQualifiedName()
                ?? entityType.GetDeleteStoredProcedure()?.GetSchemaQualifiedName()
                ?? entityType.GetUpdateStoredProcedure()?.GetSchemaQualifiedName();
            if (mappingStrategy == RelationalAnnotationNames.TpcMappingStrategy
                && !entityType.ClrType.IsInstantiable()
                && storeObjectName != null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.AbstractTpc(entityType.DisplayName(), storeObjectName));
            }
        }

        if (entityType.BaseType != null)
        {
            if (mappingStrategy != null
                && mappingStrategy != (string?)entityType.BaseType[RelationalAnnotationNames.MappingStrategy])
            {
                throw new InvalidOperationException(
                    RelationalStrings.DerivedStrategy(entityType.DisplayName(), mappingStrategy));
            }

            return;
        }

        // Hierarchy mapping strategy must be the same across all types of mappings (only for root types)
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
                foreach (var complexProperty in derivedType.GetDeclaredComplexProperties())
                {
                    ValidateDiscriminatorValues(complexProperty.ComplexType);
                }

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

    /// <summary>
    ///     Validates the key value generation is valid.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateValueGeneration(
        IKey key,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var entityType = key.DeclaringEntityType;
        if (entityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy
            && entityType.BaseType == null)
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
    ///     Validates the mapping fragments for an entity type.
    /// </summary>
    /// <param name="entityType">The entity type to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateMappingFragment(
        IEntityType entityType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var fragments = EntityTypeMappingFragment.Get(entityType);
        if (fragments == null)
        {
            return;
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
    ///     Validates a table-specific property override for a property.
    /// </summary>
    /// <param name="property">The property to validate.</param>
    /// <param name="propertyOverride">The property override to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidatePropertyOverride(
        IProperty property,
        IReadOnlyRelationalPropertyOverrides propertyOverride,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (GetAllMappedStoreObjects(property, propertyOverride.StoreObject.StoreObjectType)
            .Any(o => o == propertyOverride.StoreObject))
        {
            return;
        }

        var storeObject = propertyOverride.StoreObject;
        switch (storeObject.StoreObjectType)
        {
            case StoreObjectType.Table:
                throw new InvalidOperationException(
                    RelationalStrings.TableOverrideMismatch(
                        property.DeclaringType.DisplayName() + "." + property.Name,
                        propertyOverride.StoreObject.DisplayName()));
            case StoreObjectType.View:
                throw new InvalidOperationException(
                    RelationalStrings.ViewOverrideMismatch(
                        property.DeclaringType.DisplayName() + "." + property.Name,
                        propertyOverride.StoreObject.DisplayName()));
            case StoreObjectType.SqlQuery:
                throw new InvalidOperationException(
                    RelationalStrings.SqlQueryOverrideMismatch(
                        property.DeclaringType.DisplayName() + "." + property.Name,
                        propertyOverride.StoreObject.DisplayName()));
            case StoreObjectType.Function:
                throw new InvalidOperationException(
                    RelationalStrings.FunctionOverrideMismatch(
                        property.DeclaringType.DisplayName() + "." + property.Name,
                        propertyOverride.StoreObject.DisplayName()));
            case StoreObjectType.InsertStoredProcedure:
            case StoreObjectType.DeleteStoredProcedure:
            case StoreObjectType.UpdateStoredProcedure:
                throw new InvalidOperationException(
                    RelationalStrings.StoredProcedureOverrideMismatch(
                        property.DeclaringType.DisplayName() + "." + property.Name,
                        propertyOverride.StoreObject.DisplayName()));
            default:
                throw new NotSupportedException(storeObject.StoreObjectType.ToString());
        }
    }

    private static IEnumerable<StoreObjectIdentifier> GetAllMappedStoreObjects(
        IReadOnlyProperty property,
        StoreObjectType storeObjectType)
    {
        var declaringType = property.DeclaringType;

        // Complex types inherit their table mapping from the containing entity type
        if (declaringType is IReadOnlyComplexType)
        {
            declaringType = property.DeclaringType.ContainingEntityType;
        }

        var mappingStrategy = declaringType.GetMappingStrategy();
        if (property.IsPrimaryKey())
        {
            var declaringStoreObject = StoreObjectIdentifier.Create(declaringType, storeObjectType);
            if (declaringStoreObject != null)
            {
                yield return declaringStoreObject.Value;
            }

            if (storeObjectType is StoreObjectType.Function or StoreObjectType.SqlQuery)
            {
                yield break;
            }

            foreach (var fragment in declaringType.GetMappingFragments(storeObjectType))
            {
                yield return fragment.StoreObject;
            }

            if (declaringType is IReadOnlyEntityType entityType)
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
            var declaringStoreObject = StoreObjectIdentifier.Create(declaringType, storeObjectType);
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
                var fragments = declaringType.GetMappingFragments(storeObjectType).ToList();
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

            if (declaringType is not IReadOnlyEntityType entityType)
            {
                yield break;
            }

            var tableFound = false;
            var queue = new Queue<IReadOnlyEntityType>();
            queue.Enqueue(entityType);
            while (queue.Count > 0
                   && (!tableFound || mappingStrategy == RelationalAnnotationNames.TpcMappingStrategy))
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
    ///     Validates a single index.
    /// </summary>
    /// <param name="index">The index to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected override void ValidateIndex(
        IIndex index,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.ValidateIndex(index, logger);

        ValidateIndexMappedToTable(index, logger);
    }

    /// <summary>
    ///     Validates that the properties of the index are all mapped to columns on at least one table.
    /// </summary>
    /// <param name="index">The index to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateIndexMappedToTable(
        IIndex index,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (index.DeclaringEntityType.GetTableName() != null
            || ((IConventionIndex)index).GetConfigurationSource() == ConfigurationSource.Convention)
        {
            return;
        }

        // The case where the index declaring type is mapped to a table is handled in ValidateSharedIndexesCompatibility
        ValidateIndexPropertyMapping(index, logger);
    }

    /// <summary>
    ///     Validates that the properties of the index are all mapped to columns on a given table.
    /// </summary>
    /// <param name="index">The index to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateIndexPropertyMapping(
        IIndex index,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (((IConventionIndex)index).GetConfigurationSource() == ConfigurationSource.Convention)
        {
            return;
        }

        IReadOnlyProperty? propertyNotMappedToAnyTable = null;
        (string, List<StoreObjectIdentifier>)? firstPropertyTables = null;
        (string, List<StoreObjectIdentifier>)? lastPropertyTables = null;
        HashSet<StoreObjectIdentifier>? overlappingTables = null;
        foreach (var property in index.Properties)
        {
            var tablesMappedToProperty = property.GetMappedStoreObjects(StoreObjectType.Table).ToList();
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
                firstPropertyTables = (property.Name, tablesMappedToProperty);
            }
            else
            {
                lastPropertyTables = (property.Name, tablesMappedToProperty);
            }

            if (propertyNotMappedToAnyTable != null)
            {
                // Property is mapped but we already found a property that is not mapped.
                overlappingTables = null;
                break;
            }

            if (overlappingTables == null)
            {
                overlappingTables = [..tablesMappedToProperty];
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
                logger.AllIndexPropertiesNotMappedToAnyTable(
                    index.DeclaringEntityType, index);
            }
            else
            {
                logger.IndexPropertiesBothMappedAndNotMappedToTable(
                    index.DeclaringEntityType, index, propertyNotMappedToAnyTable!.Name);
            }
        }
        else if (overlappingTables.Count == 0)
        {
            Check.DebugAssert(firstPropertyTables != null);
            Check.DebugAssert(lastPropertyTables != null);

            logger.IndexPropertiesMappedToNonOverlappingTables(
                index.DeclaringEntityType,
                index,
                firstPropertyTables.Value.Item1,
                firstPropertyTables.Value.Item2.Select(t => (t.Name, t.Schema)).ToList(),
                lastPropertyTables.Value.Item1,
                lastPropertyTables.Value.Item2.Select(t => (t.Name, t.Schema)).ToList());
        }
    }

    /// <inheritdoc />
    protected override void ValidateData(
        IEntityType entityType,
        Dictionary<IKey, IIdentityMap> identityMaps,
        bool sensitiveDataLogged,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (entityType.IsMappedToJson() && entityType.GetSeedData().Any())
        {
            throw new InvalidOperationException(RelationalStrings.HasDataNotSupportedForEntitiesMappedToJson(entityType.DisplayName()));
        }

        foreach (var navigation in entityType.GetNavigations()
                     .Where(x => x.ForeignKey.IsOwnership && x.TargetEntityType.IsMappedToJson()))
        {
            if (entityType.GetSeedData().Any(x => x.TryGetValue(navigation.Name, out _)))
            {
                throw new InvalidOperationException(
                    RelationalStrings.HasDataNotSupportedForEntitiesMappedToJson(entityType.DisplayName()));
            }
        }

        base.ValidateData(entityType, identityMaps, sensitiveDataLogged, logger);
    }

    /// <inheritdoc />
    protected override void ValidateTrigger(
        ITrigger trigger,
        IEntityType entityType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.ValidateTrigger(trigger, entityType, logger);

        ValidateTriggerTableMapping(trigger, entityType, logger);
    }

    /// <summary>
    ///     Validates that a trigger is mapped to the same table as its entity type.
    /// </summary>
    /// <param name="trigger">The trigger to validate.</param>
    /// <param name="entityType">The entity type containing the trigger.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateTriggerTableMapping(
        ITrigger trigger,
        IEntityType entityType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var tableName = entityType.GetTableName();
        var tableSchema = entityType.GetSchema();

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

    /// <summary>
    ///     Validates the container column type configuration for a specific entity type.
    /// </summary>
    /// <param name="entityType">The entity type to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateContainerColumnType(
        IEntityType entityType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (entityType[RelationalAnnotationNames.ContainerColumnType] != null)
        {
            if (entityType.FindOwnership()?.PrincipalEntityType.IsOwned() == true)
            {
                throw new InvalidOperationException(RelationalStrings.ContainerTypeOnNestedOwnedEntityType(entityType.DisplayName()));
            }

            if (!entityType.IsOwned()
                || entityType.GetContainerColumnName() == null)
            {
                throw new InvalidOperationException(RelationalStrings.ContainerTypeOnNonContainer(entityType.DisplayName()));
            }
        }
    }

    /// <summary>
    ///     Validates the JSON entities mapped to a table.
    /// </summary>
    /// <param name="mappedTypes">The entity types mapped to the table.</param>
    /// <param name="table">The table identifier.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateJsonTable(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier table,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        var jsonColumnMappings = new Dictionary<string, List<ITypeBase>>();
        foreach (var entityType in mappedTypes)
        {
            if (entityType.FindOwnership() is not { } ownership
                || !entityType.IsMappedToJson())
            {
                continue;
            }

            ValidateJsonEntityNavigations(table, entityType);
            ValidateJsonEntityKey(table, entityType);
            ValidateJsonEntityProperties(table, entityType);
            ValidateJsonEntitySingularMapping(table, entityType);

            if (ownership.PrincipalEntityType.IsMappedToJson())
            {
                continue;
            }

            var columnName = entityType.GetContainerColumnName();
            Check.DebugAssert(columnName != null);

            if (!jsonColumnMappings.TryGetValue(columnName, out var sources))
            {
                sources = [];
                jsonColumnMappings[columnName] = sources;
            }

            sources.Add(entityType);
        }

        var nonOwnedTypes = mappedTypes.Where(x => !x.IsOwned());
        var nonOwnedTypesCount = nonOwnedTypes.Count();
        if (nonOwnedTypesCount == 0)
        {
            var nonJsonType = mappedTypes.FirstOrDefault(x => !x.IsMappedToJson());
            if (nonJsonType != null)
            {
                // must be an owned collection (mapped to a separate table) that owns a JSON type
                // Issue #28441
                throw new InvalidOperationException(
                    RelationalStrings.JsonEntityOwnedByNonJsonOwnedType(
                        nonJsonType.DisplayName(), table.DisplayName()));
            }
        }

        var distinctRootTypes = nonOwnedTypes.Select(x => x.GetRootType()).Distinct().ToList();
        if (distinctRootTypes.Count > 1)
        {
            // Issue #28442
            throw new InvalidOperationException(
                RelationalStrings.JsonEntityWithTableSplittingIsNotSupported);
        }

        var rootType = distinctRootTypes[0];
        foreach (var entityType in mappedTypes)
        {
            if (!entityType.IsMappedToJson())
            {
                ValidateNestedComplexTypes(jsonColumnMappings, entityType);
            }
        }

        var conflictingColumn = jsonColumnMappings.FirstOrDefault(kvp => kvp.Value.Count > 1);
        if (conflictingColumn.Key != null)
        {
            // TODO: handle JSON columns on views, issue #28584
            throw new InvalidOperationException(
                RelationalStrings.JsonEntityMultipleRootsMappedToTheSameJsonColumn(
                    conflictingColumn.Key, table.Name));
        }

        ValidateJsonEntityRoot(table, rootType);

        static void ValidateNestedComplexTypes(Dictionary<string, List<ITypeBase>> jsonColumnMappings, ITypeBase structuralType)
        {
            foreach (var complexProperty in structuralType.GetComplexProperties())
            {
                var columnName = complexProperty.ComplexType.GetContainerColumnName();
                if (!string.IsNullOrEmpty(columnName))
                {
                    if (!jsonColumnMappings.TryGetValue(columnName, out var sources))
                    {
                        sources = [];
                        jsonColumnMappings[columnName] = sources;
                    }

                    sources.Add(complexProperty.ComplexType);
                }
                else
                {
                    ValidateNestedComplexTypes(jsonColumnMappings, complexProperty.ComplexType);
                }
            }
        }
    }

    /// <summary>
    ///     Validates that an entity type owning a JSON entity is mapped to a table or view.
    /// </summary>
    /// <param name="entityType">The entity type to validate.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateJsonEntityOwnerMappedToTableOrView(
        IEntityType entityType,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        if (entityType.GetTableName() != null
            || entityType.GetViewName() != null
            || entityType.IsMappedToJson())
        {
            return;
        }

        if (entityType.GetDeclaredNavigations()
            .Any(x => x.ForeignKey.IsOwnership && x.TargetEntityType.IsMappedToJson()))
        {
            throw new InvalidOperationException(
                RelationalStrings.JsonEntityWithOwnerNotMappedToTableOrView(
                    entityType.DisplayName()));
        }
    }

    /// <summary>
    ///     Validates the singular (non-collection) mapping aspects of an entity mapped to a JSON column.
    /// </summary>
    /// <param name="table">The table identifier.</param>
    /// <param name="jsonEntityType">The entity type to validate.</param>
    protected virtual void ValidateJsonEntitySingularMapping(
        in StoreObjectIdentifier table,
        IEntityType jsonEntityType)
    {
        var ownership = jsonEntityType.FindOwnership()!;
        var ownerTableOrViewName = ownership.PrincipalEntityType.GetViewName() ?? ownership.PrincipalEntityType.GetTableName();
        if (table.Name != ownerTableOrViewName)
        {
            throw new InvalidOperationException(
                RelationalStrings.JsonEntityMappedToDifferentTableOrViewThanOwner(
                    jsonEntityType.DisplayName(), table.Name, ownership.PrincipalEntityType.DisplayName(), ownerTableOrViewName));
        }

        var principalContainerColumn = ownership.PrincipalEntityType.GetContainerColumnName();
        if (principalContainerColumn != null
            && principalContainerColumn != jsonEntityType.GetContainerColumnName())
        {
            throw new InvalidOperationException(
                RelationalStrings.JsonEntityMappedToDifferentColumnThanOwner(
                    jsonEntityType.DisplayName(), jsonEntityType.GetContainerColumnName(),
                    ownership.PrincipalEntityType.DisplayName(), principalContainerColumn));
        }
    }

    /// <summary>
    ///     Validates the JSON entities mapped to a view.
    /// </summary>
    /// <param name="mappedTypes">The entity types mapped to the view.</param>
    /// <param name="view">The view identifier.</param>
    /// <param name="logger">The logger to use.</param>
    protected virtual void ValidateJsonView(
        IReadOnlyList<IEntityType> mappedTypes,
        in StoreObjectIdentifier view,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var jsonEntityType in mappedTypes.Where(x => x.IsMappedToJson()))
        {
            var ownership = jsonEntityType.FindOwnership()!;
            var ownerTableOrViewName = ownership.PrincipalEntityType.GetViewName() ?? ownership.PrincipalEntityType.GetTableName();
            if (view.Name != ownerTableOrViewName)
            {
                throw new InvalidOperationException(
                    RelationalStrings.JsonEntityMappedToDifferentTableOrViewThanOwner(
                        jsonEntityType.DisplayName(), view.Name, ownership.PrincipalEntityType.DisplayName(), ownerTableOrViewName));
            }
        }
    }

    /// <summary>
    ///     Validates the root entity mapped to a JSON column.
    /// </summary>
    /// <param name="table">The table identifier.</param>
    /// <param name="rootType">The entity type to validate.</param>
    protected virtual void ValidateJsonEntityRoot(
        in StoreObjectIdentifier table,
        IEntityType rootType)
    {
        var mappingStrategy = rootType.GetMappingStrategy();
        if (mappingStrategy != null && mappingStrategy != RelationalAnnotationNames.TphMappingStrategy)
        {
            // TODO: issue #37445
            throw new InvalidOperationException(
                RelationalStrings.JsonEntityWithNonTphInheritanceOnOwner(rootType.DisplayName()));
        }
    }

    /// <summary>
    ///     Validates navigations of the entity mapped to a JSON column.
    /// </summary>
    /// <param name="table">The table identifier.</param>
    /// <param name="jsonEntityType">The entity type to validate.</param>
    protected virtual void ValidateJsonEntityNavigations(
        in StoreObjectIdentifier table,
        IEntityType jsonEntityType)
    {
        var ownership = jsonEntityType.FindOwnership()!;

        if (ownership.PrincipalEntityType.IsOwned()
            && !ownership.PrincipalEntityType.IsMappedToJson())
        {
            //TODO: Allow non-JSON owner, issue #28441
            throw new InvalidOperationException(
                RelationalStrings.JsonEntityOwnedByNonJsonOwnedType(
                    ownership.PrincipalEntityType.DisplayName(),
                    table.DisplayName()));
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
    /// <param name="table">The table identifier.</param>
    /// <param name="jsonEntityType">The entity type containing the key to validate.</param>
    protected virtual void ValidateJsonEntityKey(
        in StoreObjectIdentifier table,
        IEntityType jsonEntityType)
    {
        var primaryKeyProperties = jsonEntityType.FindPrimaryKey()!.Properties;
        var ownership = jsonEntityType.FindOwnership()!;

        foreach (var primaryKeyProperty in primaryKeyProperties)
        {
            if (primaryKeyProperty.GetJsonPropertyName() != null)
            {
                // Issue #28594
                throw new InvalidOperationException(
                    RelationalStrings.JsonEntityWithExplicitlyConfiguredJsonPropertyNameOnKey(
                        primaryKeyProperty.Name, jsonEntityType.DisplayName()));
            }

            if (!ownership.IsUnique)
            {
                // For collection entities, no key properties other than the generated ones are allowed because they
                // will not be persisted.
                if (!primaryKeyProperty.IsOrdinalKeyProperty()
                    && !primaryKeyProperty.IsForeignKey())
                {
                    // issue #28594
                    throw new InvalidOperationException(
                        RelationalStrings.JsonEntityWithExplicitlyConfiguredKey(
                            jsonEntityType.DisplayName(), primaryKeyProperty.Name));
                }
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
    /// <param name="table">The table identifier.</param>
    /// <param name="jsonEntityType">The entity type containing the properties to validate.</param>
    protected virtual void ValidateJsonEntityProperties(
        in StoreObjectIdentifier table,
        IEntityType jsonEntityType)
    {
        var jsonPropertyNames = ValidateJsonProperties(jsonEntityType);
        foreach (var navigation in jsonEntityType.GetNavigations())
        {
            if (!navigation.TargetEntityType.IsMappedToJson()
                || navigation.IsOnDependent)
            {
                continue;
            }

            var jsonPropertyName = navigation.TargetEntityType.GetJsonPropertyName();
            if (jsonPropertyName != null)
            {
                CheckUniqueness(jsonPropertyName, navigation.Name, jsonEntityType, jsonPropertyNames);
            }
        }
    }

    private static Dictionary<string, string> ValidateJsonProperties(ITypeBase typeBase)
    {
        var jsonPropertyNames = new Dictionary<string, string>();
        foreach (var property in typeBase.GetProperties())
        {
            var jsonPropertyName = property.GetJsonPropertyName();
            if (string.IsNullOrEmpty(jsonPropertyName))
            {
                continue;
            }

            var columnNameAnnotation = property.FindAnnotation(RelationalAnnotationNames.ColumnName);
            if (columnNameAnnotation != null && !string.IsNullOrEmpty((string?)columnNameAnnotation.Value))
            {
                throw new InvalidOperationException(
                    RelationalStrings.PropertyBothColumnNameAndJsonPropertyName(
                        $"{typeBase.DisplayName()}.{property.Name}",
                        (string)columnNameAnnotation.Value,
                        jsonPropertyName));
            }

            if (property.TryGetDefaultValue(out _))
            {
                // Issue #35934
                throw new InvalidOperationException(
                    RelationalStrings.JsonEntityWithDefaultValueSetOnItsProperty(
                        typeBase.DisplayName(), property.Name));
            }

            CheckUniqueness(jsonPropertyName, property.Name, typeBase, jsonPropertyNames);

            if (property.IsConcurrencyToken)
            {
                throw new InvalidOperationException(
                    RelationalStrings.ConcurrencyTokenOnJsonMappedProperty(
                        property.Name, typeBase.DisplayName()));
            }
        }

        foreach (var complexProperty in typeBase.GetComplexProperties())
        {
            var jsonPropertyName = complexProperty.GetJsonPropertyName();
            if (jsonPropertyName != null)
            {
                CheckUniqueness(jsonPropertyName, complexProperty.Name, typeBase, jsonPropertyNames);
            }
        }

        return jsonPropertyNames;
    }

    private static void CheckUniqueness(
        string jsonPropertyName,
        string propertyName,
        IReadOnlyTypeBase structuralType,
        Dictionary<string, string> jsonPropertyNames)
    {
        if (jsonPropertyNames.TryGetValue(jsonPropertyName, out var existingProperty))
        {
            throw new InvalidOperationException(
                RelationalStrings.JsonObjectWithMultiplePropertiesMappedToSameJsonProperty(
                    existingProperty,
                    propertyName,
                    structuralType.DisplayName(),
                    jsonPropertyName));
        }

        jsonPropertyNames[jsonPropertyName] = propertyName;
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
        ITypeBase typeBase,
        IProperty unmappedProperty)
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
