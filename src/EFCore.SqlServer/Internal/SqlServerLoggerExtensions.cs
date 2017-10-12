// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class SqlServerLoggerExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void DecimalTypeDefaultWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            [NotNull] IProperty property)
        {
            var definition = SqlServerStrings.LogDefaultDecimalTypeColumn;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(diagnostics, property.Name, property.DeclaringEntityType.DisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new PropertyEventData(
                        definition,
                        DecimalTypeDefaultWarning,
                        property));
            }
        }

        private static string DecimalTypeDefaultWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (PropertyEventData)payload;
            return d.GenerateMessage(
                p.Property.Name,
                p.Property.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ByteIdentityColumnWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            [NotNull] IProperty property)
        {
            var definition = SqlServerStrings.LogByteIdentityColumn;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(diagnostics, property.Name, property.DeclaringEntityType.DisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new PropertyEventData(
                        definition,
                        ByteIdentityColumnWarning,
                        property));
            }
        }

        private static string ByteIdentityColumnWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (PropertyEventData)payload;
            return d.GenerateMessage(
                p.Property.Name,
                p.Property.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ColumnFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [NotNull] string tableName,
            [NotNull] string columnName,
            int ordinal,
            [NotNull] string dataTypeName,
            int maxLength,
            int precision,
            int scale,
            bool nullable,
            bool identity,
            [CanBeNull] string defaultValue,
            [CanBeNull] string computedValue)
        {
            // No DiagnosticsSource events because these are purely design-time messages

            var definition = SqlServerStrings.LogFoundColumn;

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
                        ordinal,
                        dataTypeName,
                        maxLength,
                        precision,
                        scale,
                        nullable,
                        identity,
                        defaultValue,
                        computedValue));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyFound(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [NotNull] string foreignKeyName,
                [NotNull] string tableName,
                [NotNull] string principalTableName,
                [NotNull] string onDeleteAction)
            // No DiagnosticsSource events because these are purely design-time messages
            => SqlServerStrings.LogFoundForeignKey.Log(diagnostics, foreignKeyName, tableName, principalTableName, onDeleteAction);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void DefaultSchemaFound(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [NotNull] string schemaName)
            // No DiagnosticsSource events because these are purely design-time messages
            => SqlServerStrings.LogFoundDefaultSchema.Log(diagnostics, schemaName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void TypeAliasFound(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [NotNull] string typeAliasName,
                [NotNull] string systemTypeName)
            // No DiagnosticsSource events because these are purely design-time messages
            => SqlServerStrings.LogFoundTypeAlias.Log(diagnostics, typeAliasName, systemTypeName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void PrimaryKeyFound(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [NotNull] string primaryKeyName,
                [NotNull] string tableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => SqlServerStrings.LogFoundPrimaryKey.Log(diagnostics, primaryKeyName, tableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void UniqueConstraintFound(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [NotNull] string uniqueConstraintName,
                [NotNull] string tableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => SqlServerStrings.LogFoundUniqueConstraint.Log(diagnostics, uniqueConstraintName, tableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IndexFound(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [NotNull] string indexName,
                [NotNull] string tableName,
                bool unique)
            // No DiagnosticsSource events because these are purely design-time messages
            => SqlServerStrings.LogFoundIndex.Log(diagnostics, indexName, tableName, unique);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyReferencesMissingPrincipalTableWarning(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string foreignKeyName,
                [CanBeNull] string tableName,
                [CanBeNull] string principalTableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => SqlServerStrings.LogPrincipalTableNotInSelectionSet.Log(diagnostics, foreignKeyName, tableName, principalTableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyPrincipalColumnMissingWarning(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [NotNull] string foreignKeyName,
                [NotNull] string tableName,
                [NotNull] string principalColumnName,
                [NotNull] string principalTableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => SqlServerStrings.LogPrincipalColumnNotFound.Log(diagnostics, foreignKeyName, tableName, principalColumnName, principalTableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MissingSchemaWarning(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string schemaName)
            // No DiagnosticsSource events because these are purely design-time messages
            => SqlServerStrings.LogMissingSchema.Log(diagnostics, schemaName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MissingTableWarning(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string tableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => SqlServerStrings.LogMissingTable.Log(diagnostics, tableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SequenceFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [NotNull] string sequenceName,
            [NotNull] string sequenceTypeName,
            bool cyclic,
            int increment,
            long start,
            long min,
            long max)
        {
            // No DiagnosticsSource events because these are purely design-time messages
            var definition = SqlServerStrings.LogFoundSequence;

            Debug.Assert(LogLevel.Debug == definition.Level);

            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    l => l.LogDebug(
                        definition.EventId,
                        null,
                        definition.MessageFormat,
                        sequenceName,
                        sequenceTypeName,
                        cyclic,
                        increment,
                        start,
                        min,
                        max));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void TableFound(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [NotNull] string tableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => SqlServerStrings.LogFoundTable.Log(diagnostics, tableName);
    }
}
