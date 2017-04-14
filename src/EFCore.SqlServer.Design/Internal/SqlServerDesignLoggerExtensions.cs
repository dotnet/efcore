// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
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
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
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
            var eventId = SqlServerDesignEventId.ColumnFound;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    SqlServerDesignStrings.FoundColumn(
                        tableName, columnName, dataTypeName, ordinal, nullable,
                        primaryKeyOrdinal, defaultValue, computedValue, precision, scale, maxLength, identity, computed));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName,
            [CanBeNull] string foreignKeyName,
            [CanBeNull] string principalTableName,
            [CanBeNull] string columnName,
            [CanBeNull] string principalColumnName,
            [CanBeNull] string updateAction,
            [CanBeNull] string deleteAction,
            int? ordinal)
        {
            var eventId = SqlServerDesignEventId.ForeignKeyColumnFound;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    SqlServerDesignStrings.FoundForeignKeyColumn(
                        tableName, foreignKeyName, principalTableName,
                        columnName, principalColumnName, updateAction, deleteAction, ordinal));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string schemaName)
        {
            var eventId = SqlServerDesignEventId.DefaultSchemaFound;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    SqlServerDesignStrings.FoundDefaultSchema(schemaName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string typeAliasName,
            [CanBeNull] string systemTypeName)
        {
            var eventId = SqlServerDesignEventId.TypeAliasFound;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    SqlServerDesignStrings.FoundTypeAlias(typeAliasName, systemTypeName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
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
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string columnName,
            [CanBeNull] string typeName)
        {
            var eventId = SqlServerDesignEventId.DataTypeDoesNotAllowSqlServerIdentityStrategyWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    SqlServerDesignStrings.DataTypeDoesNotAllowSqlServerIdentityStrategy(columnName, typeName));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        ColumnName = columnName,
                        TypeName = typeName
                    });
            }
        }
    }
}
