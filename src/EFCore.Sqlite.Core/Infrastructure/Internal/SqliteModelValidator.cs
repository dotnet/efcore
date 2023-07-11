// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteModelValidator : RelationalModelValidator
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteModelValidator(
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
        base.Validate(model, logger);

        ValidateNoSchemas(model, logger);
        ValidateNoSequences(model, logger);
        ValidateNoStoredProcedures(model, logger);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void ValidateNoSchemas(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes().Where(e => e.GetSchema() != null))
        {
            logger.SchemaConfiguredWarning(entityType, entityType.GetSchema()!);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void ValidateNoSequences(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var sequence in model.GetSequences())
        {
            logger.SequenceConfiguredWarning(sequence);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void ValidateNoStoredProcedures(
        IModel model,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        foreach (var entityType in model.GetEntityTypes())
        {
            if (entityType.GetInsertStoredProcedure() is not null
                || entityType.GetUpdateStoredProcedure() is not null
                || entityType.GetDeleteStoredProcedure() is not null)
            {
                throw new InvalidOperationException(SqliteStrings.StoredProceduresNotSupported(entityType.DisplayName()));
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void ValidateCompatible(
        IProperty property,
        IProperty duplicateProperty,
        string columnName,
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        base.ValidateCompatible(property, duplicateProperty, columnName, storeObject, logger);

        var propertySrid = property.GetSrid(storeObject);
        var duplicatePropertySrid = duplicateProperty.GetSrid(storeObject);
        if (propertySrid != duplicatePropertySrid)
        {
            throw new InvalidOperationException(
                SqliteStrings.DuplicateColumnNameSridMismatch(
                    duplicateProperty.DeclaringType.DisplayName(),
                    duplicateProperty.Name,
                    property.DeclaringType.DisplayName(),
                    property.Name,
                    columnName,
                    storeObject.DisplayName()));
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
        base.ValidateValueGeneration(entityType, key, logger);

        var keyProperties = key.Properties;
        if (!entityType.IsMappedToJson()
            && key.IsPrimaryKey()
            && keyProperties.Count(p => p.ClrType.UnwrapNullableType().IsInteger()) > 1
            && keyProperties.Any(
                p => p.ValueGenerated == ValueGenerated.OnAdd
                    && p.ClrType.UnwrapNullableType().IsInteger()
                    && !p.TryGetDefaultValue(out _)
                    && p.GetDefaultValueSql() == null
                    && !p.IsForeignKey()))
        {
            logger.CompositeKeyWithValueGeneration(key);
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
        in StoreObjectIdentifier storeObject,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> logger)
    {
        bool? firstSqlOutputSetting = null;
        IEntityType? firstMappedType = null;
        foreach (var mappedType in mappedTypes)
        {
            if (((IConventionEntityType)mappedType).GetUseSqlReturningClauseConfigurationSource() is null)
            {
                continue;
            }

            if (firstSqlOutputSetting is null)
            {
                (firstSqlOutputSetting, firstMappedType) = (mappedType.IsSqlReturningClauseUsed(), mappedType);
            }
            else if (mappedType.IsSqlReturningClauseUsed() != firstSqlOutputSetting)
            {
                throw new InvalidOperationException(
                    SqliteStrings.IncompatibleSqlReturningClauseMismatch(
                        storeObject.DisplayName(), firstMappedType!.DisplayName(), mappedType.DisplayName(),
                        firstSqlOutputSetting.Value ? firstMappedType.DisplayName() : mappedType.DisplayName(),
                        !firstSqlOutputSetting.Value ? firstMappedType.DisplayName() : mappedType.DisplayName()));
            }
        }

        base.ValidateSharedTableCompatibility(mappedTypes, storeObject, logger);
    }
}
