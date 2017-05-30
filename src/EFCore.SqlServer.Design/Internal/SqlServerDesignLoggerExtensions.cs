// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class SqlServerDesignLoggerExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ColumnFound(
            [NotNull] this IDiagnosticsLogger<EF.LoggerCategories.Scaffolding> diagnostics,
            [CanBeNull] string tableName,
            [CanBeNull] string columnName,
            [CanBeNull] string dataTypeName,
            int? ordinal,
            bool? nullable,
            int? primaryKeyOrdinal,
            [CanBeNull] string defaultValue,
            [CanBeNull] string computedValue,
            int? precision,
            int? scale,
            int? maxLength,
            [CanBeNull] bool? identity,
            [CanBeNull] bool? computed)
        {
            var definition = SqlServerDesignStrings.LogFoundColumn;

            Debug.Assert(LogLevel.Debug == definition.Level);

            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    l => l.LogDebug(
                        definition.EventId,
                        null,
                        definition.MessageFormat,
                        tableName,
                        columnName,
                        dataTypeName,
                        ordinal,
                        nullable,
                        primaryKeyOrdinal,
                        defaultValue,
                        computedValue,
                        precision,
                        scale,
                        maxLength,
                        identity,
                        computed));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName,
                        ColumnName = columnName,
                        DataTypeName = dataTypeName,
                        Ordinal = ordinal,
                        Nullable = nullable,
                        PrimaryKeyOrdinal = primaryKeyOrdinal,
                        DefaultValue = defaultValue,
                        ComputedValue = computedValue,
                        Precision = precision,
                        Scale = scale,
                        MaxLength = maxLength,
                        Identity = identity,
                        Computed = computed
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyColumnFound(
            [NotNull] this IDiagnosticsLogger<EF.LoggerCategories.Scaffolding> diagnostics,
            [CanBeNull] string tableName,
            [CanBeNull] string foreignKeyName,
            [CanBeNull] string principalTableName,
            [CanBeNull] string columnName,
            [CanBeNull] string principalColumnName,
            [CanBeNull] string updateAction,
            [CanBeNull] string deleteAction,
            int? ordinal)
        {
            var definition = SqlServerDesignStrings.LogFoundForeignKeyColumn;

            Debug.Assert(LogLevel.Debug == definition.Level);

            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    l => l.LogDebug(
                        definition.EventId,
                        null,
                        definition.MessageFormat,
                        tableName,
                        foreignKeyName,
                        principalTableName,
                        columnName,
                        principalColumnName,
                        updateAction,
                        deleteAction,
                        ordinal));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName,
                        ForeignKeyName = foreignKeyName,
                        PrincipalTableName = principalTableName,
                        ColumnName = columnName,
                        PrincipalColumnName = principalColumnName,
                        UpdateAction = updateAction,
                        DeleteAction = deleteAction,
                        Ordinal = ordinal
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void DefaultSchemaFound(
            [NotNull] this IDiagnosticsLogger<EF.LoggerCategories.Scaffolding> diagnostics,
            [CanBeNull] string schemaName)
        {
            var definition = SqlServerDesignStrings.LogFoundDefaultSchema;

            definition.Log(diagnostics, schemaName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        SchemaName = schemaName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void TypeAliasFound(
            [NotNull] this IDiagnosticsLogger<EF.LoggerCategories.Scaffolding> diagnostics,
            [CanBeNull] string typeAliasName,
            [CanBeNull] string systemTypeName)
        {
            var definition = SqlServerDesignStrings.LogFoundTypeAlias;

            definition.Log(diagnostics, typeAliasName, systemTypeName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TypeAliasName = typeAliasName,
                        SystemTypeName = systemTypeName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void DataTypeDoesNotAllowSqlServerIdentityStrategyWarning(
            [NotNull] this IDiagnosticsLogger<EF.LoggerCategories.Scaffolding> diagnostics,
            [CanBeNull] string columnName,
            [CanBeNull] string typeName)
        {
            var definition = SqlServerDesignStrings.LogDataTypeDoesNotAllowSqlServerIdentityStrategy;

            definition.Log(diagnostics, columnName, typeName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        ColumnName = columnName,
                        TypeName = typeName
                    });
            }
        }
    }
}
