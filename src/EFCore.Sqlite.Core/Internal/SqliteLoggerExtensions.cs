// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public static class SqliteLoggerExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SchemaConfiguredWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            [NotNull] IEntityType entityType,
            [NotNull] string schema)
        {
            var definition = SqliteStrings.LogSchemaConfigured;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(diagnostics, entityType.DisplayName(), schema);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new EntityTypeSchemaEventData(
                        definition,
                        SchemaConfiguredWarning,
                        entityType,
                        schema));
            }
        }

        private static string SchemaConfiguredWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (EntityTypeSchemaEventData)payload;
            return d.GenerateMessage(
                p.EntityType.DisplayName(),
                p.Schema);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SequenceConfiguredWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            [NotNull] ISequence sequence)
        {
            var definition = SqliteStrings.LogSequenceConfigured;

            definition.Log(diagnostics, sequence.Name);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new SequenceEventData(
                        definition,
                        SequenceConfiguredWarning,
                        sequence));
            }
        }

        private static string SequenceConfiguredWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (SequenceEventData)payload;
            return d.GenerateMessage(p.Sequence.Name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ColumnFound(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string tableName,
                [CanBeNull] string columnName,
                [CanBeNull] string dataTypeName,
                bool notNull,
                [CanBeNull] string defaultValue)
            // No DiagnosticsSource events because these are purely design-time messages
            => SqliteStrings.LogFoundColumn.Log(diagnostics, tableName, columnName, dataTypeName, notNull, defaultValue);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SchemasNotSupportedWarning(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics)
            // No DiagnosticsSource events because these are purely design-time messages
            => SqliteStrings.LogUsingSchemaSelectionsWarning.Log(diagnostics);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyReferencesMissingTableWarning(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string foreignKeyName)
            // No DiagnosticsSource events because these are purely design-time messages
            => SqliteStrings.LogForeignKeyScaffoldErrorPrincipalTableNotFound.Log(diagnostics, foreignKeyName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void TableFound(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string tableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => SqliteStrings.LogFoundTable.Log(diagnostics, tableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MissingTableWarning(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string tableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => SqliteStrings.LogMissingTable.Log(diagnostics, tableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyPrincipalColumnMissingWarning(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string foreignKeyName,
                [CanBeNull] string tableName,
                [CanBeNull] string principalColumnName,
                [CanBeNull] string principalTableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => SqliteStrings.LogPrincipalColumnNotFound.Log(diagnostics, foreignKeyName, tableName, principalColumnName, principalTableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IndexFound(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string indexName,
                [CanBeNull] string tableName,
                bool? unique)
            // No DiagnosticsSource events because these are purely design-time messages
            => SqliteStrings.LogFoundIndex.Log(diagnostics, indexName, tableName, unique);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyFound(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string tableName,
                long id,
                [CanBeNull] string principalTableName,
                [CanBeNull] string deleteAction)
            // No DiagnosticsSource events because these are purely design-time messages
            => SqliteStrings.LogFoundForeignKey.Log(diagnostics, tableName, id, principalTableName, deleteAction);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void PrimaryKeyFound(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string primaryKeyName,
                [CanBeNull] string tableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => SqliteStrings.LogFoundPrimaryKey.Log(diagnostics, primaryKeyName, tableName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void UniqueConstraintFound(
                [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
                [CanBeNull] string uniqueConstraintName,
                [CanBeNull] string tableName)
            // No DiagnosticsSource events because these are purely design-time messages
            => SqliteStrings.LogFoundUniqueConstraint.Log(diagnostics, uniqueConstraintName, tableName);
    }
}
