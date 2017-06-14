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
        {
            // No DiagnosticsSource events because these are purely design-time messages

            var definition = SqliteStrings.LogFoundColumn;

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
                        notNull,
                        defaultValue));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyColumnFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName,
            long id,
            [CanBeNull] string principalTableName,
            [CanBeNull] string columnName,
            [CanBeNull] string principalColumnName,
            [CanBeNull] string deleteAction)
        {
            // No DiagnosticsSource events because these are purely design-time messages

            var definition = SqliteStrings.LogFoundForeignKeyColumn;

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
                        id,
                        principalTableName,
                        columnName,
                        principalColumnName,
                        deleteAction));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SchemasNotSupportedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics)
            // No DiagnosticsSource events because these are purely design-time messages
            => SqliteStrings.LogUsingSchemaSelectionsWarning.Log(diagnostics);
    }
}
