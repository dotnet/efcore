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
    public static class SqliteDesignLoggerExtensions
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
            bool? notNull, 
            int? primaryKeyOrdinal, 
            [CanBeNull] string defaultValue)
        {
            var eventId = SqliteDesignEventId.ColumnFound;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    SqliteDesignStrings.FoundColumn(
                        tableName, columnName, dataTypeName, ordinal,
                        notNull, primaryKeyOrdinal, defaultValue));
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
                        NotNull = notNull,
                        PrimaryKeyOrdinal = primaryKeyOrdinal,
                        DefaultValue = defaultValue
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
            int id, 
            [CanBeNull] string principalTableName, 
            [CanBeNull] string columnName, 
            [CanBeNull] string principalColumnName, 
            [CanBeNull] string deleteAction, 
            int? ordinal)
        {
            var eventId = SqliteDesignEventId.ForeignKeyColumnFound;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    SqliteDesignStrings.FoundForeignKeyColumn(
                        tableName, id, principalTableName, columnName,
                        principalColumnName, deleteAction, ordinal));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        TableName = tableName,
                        Id = id,
                        PrincipalTableName = principalTableName,
                        ColumnName = columnName,
                        PrincipalColumnName = principalColumnName,
                        DeleteAction = deleteAction,
                        Ordinal = ordinal
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SchemasNotSupportedWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Scaffolding> diagnostics)
        {
            var eventId = SqliteDesignEventId.SchemasNotSupportedWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    SqliteDesignStrings.UsingSchemaSelectionsWarning);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(eventId.Name, null);
            }
        }
    }
}
