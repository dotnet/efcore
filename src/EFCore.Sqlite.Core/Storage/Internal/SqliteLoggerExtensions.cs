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

        private static string SchemaConfiguredWarning(EventDefinitionBase definition, EventDataBase payload)
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

        private static string SequenceConfiguredWarning(EventDefinitionBase definition, EventDataBase payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (SequenceEventData)payload;
            return d.GenerateMessage(p.Sequence.Name);
        }
    }
}
